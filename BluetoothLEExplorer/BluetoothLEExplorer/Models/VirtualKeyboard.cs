using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Input;

using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Background;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Power;
using Windows.Storage.Streams;

namespace BluetoothLEExplorer.Models
{
    class VirtualKeyboard
    {
        private static readonly GattLocalCharacteristicParameters c_hidInputReportParameters = new GattLocalCharacteristicParameters
        {
            CharacteristicProperties = GattCharacteristicProperties.Read | GattCharacteristicProperties.Notify,
            ReadProtectionLevel = GattProtectionLevel.EncryptionRequired
        };

        private static readonly uint c_hidReportReferenceDescriptorShortUuid = 0x2908;

        private static readonly GattLocalDescriptorParameters c_hidKeyboardReportReferenceParameters = new GattLocalDescriptorParameters
        {
            ReadProtectionLevel = GattProtectionLevel.EncryptionRequired,
            StaticValue = new byte[]
            {
                0x01, // Report ID: 1
                0x01  // Report Type: Input
            }.AsBuffer()
        };

        private static readonly GattLocalCharacteristicParameters c_hidReportMapParameters = new GattLocalCharacteristicParameters
        {
            CharacteristicProperties = GattCharacteristicProperties.Read,
            ReadProtectionLevel = GattProtectionLevel.EncryptionRequired,
            StaticValue = new byte[]
            {
                0x05, 0x01,       // USAGE_PAGE (Generic Desktop)
                0x09, 0x06,       // USAGE (Keyboard)
                0xa1, 0x01,       // COLLECTION (Application)
                0x05, 0x07,       //   USAGE_PAGE (Keyboard)
                0x85, 0x01,       //   REPORT_ID (1)
                0x19, 0xe0,       //   USAGE_MINIMUM (Keyboard LeftControl)
                0x29, 0xe7,       //   USAGE_MAXIMUM (Keyboard Right GUI)
                0x15, 0x00,       //   LOGICAL_MINIMUM (0)
                0x25, 0x01,       //   LOGICAL_MAXIMUM (1)
                0x75, 0x01,       //   REPORT_SIZE (1)
                0x95, 0x08,       //   REPORT_COUNT (8)
                0x81, 0x02,       //   INPUT (Data,Var,Abs)
                0x75, 0x08,       //   REPORT_SIZE (8)
                0x95, 0x07,       //   REPORT_COUNT (7)
                0x19, 0x00,       //   USAGE_MINIMUM (Reserved (no event indicated))
                0x29, 0x65,       //   USAGE_MAXIMUM (Keyboard Application)
                0x15, 0x00,       //   LOGICAL_MINIMUM (0)
                0x25, 0x65,       //   LOGICAL_MAXIMUM (101)
                0x81, 0x00,       //   INPUT (Data,Ary,Abs)
                0xc0,             // END_COLLECTION
            }.AsBuffer()
        };

        private static readonly GattLocalCharacteristicParameters c_hidInformationParameters = new GattLocalCharacteristicParameters
        {
            CharacteristicProperties = GattCharacteristicProperties.Read,
            ReadProtectionLevel = GattProtectionLevel.EncryptionRequired,
            StaticValue = new byte[]
            {
                0x11, 0x01, // HID Version: 1101
                0x00,       // Country Code: 0
                0x01        // Not Normally Connectable, Remote Wake supported
            }.AsBuffer()
        };

        private static readonly GattLocalCharacteristicParameters c_hidControlPointParameters = new GattLocalCharacteristicParameters
        {
            CharacteristicProperties = GattCharacteristicProperties.WriteWithoutResponse,
            WriteProtectionLevel = GattProtectionLevel.EncryptionRequired
        };

        private static readonly GattLocalCharacteristicParameters c_batteryLevelParameters = new GattLocalCharacteristicParameters
        {
            CharacteristicProperties = GattCharacteristicProperties.Read | GattCharacteristicProperties.Notify,
            ReadProtectionLevel = GattProtectionLevel.Plain
        };

        private static readonly uint c_sizeOfKeyboardReportDataInBytes = 0x8;

        private GattServiceProvider m_hidServiceProvider;
        private GattLocalService m_hidService;
        private GattLocalCharacteristic m_hidKeyboardReport;
        private GattLocalDescriptor m_hidKeyboardReportReference;
        private GattLocalCharacteristic m_hidReportMap;
        private GattLocalCharacteristic m_hidInformation;
        private GattLocalCharacteristic m_hidControlPoint;

        private Object m_lock = new Object();

        private bool m_initializationFinished = false;

        private HashSet<byte> m_currentlyDepressedModifierKeys = new HashSet<byte>();
        private HashSet<byte> m_currentlyDepressedKeys = new HashSet<byte>();
        private byte[] m_lastSentKeyboardReportValue = new byte[c_sizeOfKeyboardReportDataInBytes];

        public delegate void SubscribedHidClientsChangedHandler(IReadOnlyList<GattSubscribedClient> subscribedClients);
        public event SubscribedHidClientsChangedHandler SubscribedHidClientsChanged;

        private static string GetStringFromBuffer(IBuffer buffer)
        {
            return GetStringFromBuffer(buffer.ToArray());
        }

        private static string GetStringFromBuffer(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", " ");
        }

        public async Task InitiliazeAsync()
        {
            await CreateHidService();

            lock (m_lock)
            {
                m_initializationFinished = true;
            }
        }

        public void Enable()
        {
            PublishService(m_hidServiceProvider);
        }

        public void Disable()
        {
            UnpublishService(m_hidServiceProvider);
        }

        public void PressKey(uint ps2Set1keyScanCode)
        {
            try
            {
                ChangeKeyState(KeyEvent.KeyMake, HidHelper.GetHidUsageFromPs2Set1(ps2Set1keyScanCode));
            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to change the key state due to: " + e.Message);
            }
        }

        public void ReleaseKey(uint ps2Set1keyScanCode)
        {
            try
            {
                ChangeKeyState(KeyEvent.KeyBreak, HidHelper.GetHidUsageFromPs2Set1(ps2Set1keyScanCode));
            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to change the key state due to: " + e.Message);
            }
        }

        private async Task CreateHidService()
        {
            // HID service.
            var hidServiceProviderCreationResult = await GattServiceProvider.CreateAsync(GattServiceUuids.HumanInterfaceDevice);
            if (hidServiceProviderCreationResult.Error != BluetoothError.Success)
            {
                Debug.WriteLine("Failed to create the HID service provider: " + hidServiceProviderCreationResult.Error);
                throw new Exception("Failed to create the HID service provider: " + hidServiceProviderCreationResult.Error);
            }
            m_hidServiceProvider = hidServiceProviderCreationResult.ServiceProvider;
            m_hidService = m_hidServiceProvider.Service;

            // HID keyboard Report characteristic.
            var hidKeyboardReportCharacteristicCreationResult = await m_hidService.CreateCharacteristicAsync(GattCharacteristicUuids.Report, c_hidInputReportParameters);
            if (hidKeyboardReportCharacteristicCreationResult.Error != BluetoothError.Success)
            {
                Debug.WriteLine("Failed to create the keyboard report characteristic: " + hidKeyboardReportCharacteristicCreationResult.Error);
                throw new Exception("Failed to create the keyboard report characteristic: " + hidKeyboardReportCharacteristicCreationResult.Error);
            }
            m_hidKeyboardReport = hidKeyboardReportCharacteristicCreationResult.Characteristic;
            m_hidKeyboardReport.SubscribedClientsChanged += HidKeyboardReport_SubscribedClientsChanged;

            // HID keyboard Report Reference descriptor.
            var hidKeyboardReportReferenceCreationResult = await m_hidKeyboardReport.CreateDescriptorAsync(BluetoothUuidHelper.FromShortId(c_hidReportReferenceDescriptorShortUuid), c_hidKeyboardReportReferenceParameters);
            if (hidKeyboardReportReferenceCreationResult.Error != BluetoothError.Success)
            {
                Debug.WriteLine("Failed to create the keyboard report reference descriptor: " + hidKeyboardReportReferenceCreationResult.Error);
                throw new Exception("Failed to create the keyboard report reference descriptor: " + hidKeyboardReportReferenceCreationResult.Error);
            }
            m_hidKeyboardReportReference = hidKeyboardReportReferenceCreationResult.Descriptor;

            // HID Report Map characteristic.
            var hidReportMapCharacteristicCreationResult = await m_hidService.CreateCharacteristicAsync(GattCharacteristicUuids.ReportMap, c_hidReportMapParameters);
            if (hidReportMapCharacteristicCreationResult.Error != BluetoothError.Success)
            {
                Debug.WriteLine("Failed to create the HID report map characteristic: " + hidReportMapCharacteristicCreationResult.Error);
                throw new Exception("Failed to create the HID report map characteristic: " + hidReportMapCharacteristicCreationResult.Error);
            }
            m_hidReportMap = hidReportMapCharacteristicCreationResult.Characteristic;

            // HID Information characteristic.
            var hidInformationCharacteristicCreationResult = await m_hidService.CreateCharacteristicAsync(GattCharacteristicUuids.HidInformation, c_hidInformationParameters);
            if (hidInformationCharacteristicCreationResult.Error != BluetoothError.Success)
            {
                Debug.WriteLine("Failed to create the HID information characteristic: " + hidInformationCharacteristicCreationResult.Error);
                throw new Exception("Failed to create the HID information characteristic: " + hidInformationCharacteristicCreationResult.Error);
            }
            m_hidInformation = hidInformationCharacteristicCreationResult.Characteristic;

            // HID Control Point characteristic.
            var hidControlPointCharacteristicCreationResult = await m_hidService.CreateCharacteristicAsync(GattCharacteristicUuids.HidControlPoint, c_hidControlPointParameters);
            if (hidControlPointCharacteristicCreationResult.Error != BluetoothError.Success)
            {
                Debug.WriteLine("Failed to create the HID control point characteristic: " + hidControlPointCharacteristicCreationResult.Error);
                throw new Exception("Failed to create the HID control point characteristic: " + hidControlPointCharacteristicCreationResult.Error);
            }
            m_hidControlPoint = hidControlPointCharacteristicCreationResult.Characteristic;
            m_hidControlPoint.WriteRequested += HidControlPoint_WriteRequested;

            m_hidServiceProvider.AdvertisementStatusChanged += HidServiceProvider_AdvertisementStatusChanged;
        }

        // Assumes that the lock is being held.
        private void PublishService(GattServiceProvider provider)
        {
            var advertisingParameters = new GattServiceProviderAdvertisingParameters
            {
                IsDiscoverable = true,
                IsConnectable = true // Peripheral role support is required for Windows to advertise as connectable.
            };

            provider.StartAdvertising(advertisingParameters);
        }

        private void UnpublishService(GattServiceProvider provider)
        {
            try
            {
                if ((provider.AdvertisementStatus == GattServiceProviderAdvertisementStatus.Started) ||
                    (provider.AdvertisementStatus == GattServiceProviderAdvertisementStatus.Aborted))
                {
                    provider.StopAdvertising();
                    SubscribedHidClientsChanged?.Invoke(null);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to stop advertising due to: " + e.Message);
            }
        }

        private async void HidControlPoint_WriteRequested(GattLocalCharacteristic sender, GattWriteRequestedEventArgs args)
        {
            try
            {
                var deferral = args.GetDeferral();
                var writeRequest = await args.GetRequestAsync();
                Debug.WriteLine("Value written to HID Control Point: " + GetStringFromBuffer(writeRequest.Value));
                // Control point only supports WriteWithoutResponse.
                deferral.Complete();
            }
            catch (Exception e)
            {
                Debug.WriteLine("Failed to handle write to Hid Control Point due to: " + e.Message);
            }
        }

        private void HidServiceProvider_AdvertisementStatusChanged(GattServiceProvider sender, GattServiceProviderAdvertisementStatusChangedEventArgs args)
        {
            Debug.WriteLine("HID advertisement status changed to " + args.Status);
        }

        private void BatteryServiceProvider_AdvertisementStatusChanged(GattServiceProvider sender, GattServiceProviderAdvertisementStatusChangedEventArgs args)
        {
            Debug.WriteLine("Battery advertisement status changed to " + args.Status);
        }

        private void HidKeyboardReport_SubscribedClientsChanged(GattLocalCharacteristic sender, object args)
        {
            Debug.WriteLine("Number of clients now registered for keyboard notifications: " + sender.SubscribedClients.Count);
            SubscribedHidClientsChanged?.Invoke(sender.SubscribedClients);
        }

        private void ChangeKeyState(KeyEvent keyEvent, byte hidUsageScanCode)
        {
            lock (m_lock)
            {
                if (!m_initializationFinished)
                {
                    return;
                }

                if (keyEvent == KeyEvent.KeyMake)
                {
                    if (HidHelper.IsMofifierKey(hidUsageScanCode))
                    {
                        Debug.WriteLine("Modifier key depressed: " + hidUsageScanCode);
                        m_currentlyDepressedModifierKeys.Add(hidUsageScanCode);
                    }
                    else
                    {
                        Debug.WriteLine("Key depressed: " + hidUsageScanCode);
                        m_currentlyDepressedKeys.Add(hidUsageScanCode);
                    }
                }
                else
                {
                    if (HidHelper.IsMofifierKey(hidUsageScanCode))
                    {
                        Debug.WriteLine("Modifier key released: " + hidUsageScanCode);
                        m_currentlyDepressedModifierKeys.Remove(hidUsageScanCode);
                    }
                    else
                    {
                        Debug.WriteLine("Key released: " + hidUsageScanCode);
                        m_currentlyDepressedKeys.Remove(hidUsageScanCode);
                    }
                }

                if (m_hidKeyboardReport.SubscribedClients.Count == 0)
                {
                    Debug.WriteLine("No clients are currently subscribed to the keyboard report.");
                    return;
                }

                var reportValue = new byte[c_sizeOfKeyboardReportDataInBytes];

                // The first byte of the report data is a modifier key bitfield.
                reportValue[0] = 0x0;
                foreach (var modifierKeyPressedScanCode in m_currentlyDepressedModifierKeys)
                {
                    reportValue[0] |= HidHelper.GetFlagOfModifierKey(modifierKeyPressedScanCode);
                }

                // The second byte up to the last byte represent one key per byte.
                int reportIndex = 1;
                foreach (var keyPressedScanCode in m_currentlyDepressedKeys)
                {
                    if (reportIndex >= reportValue.Length)
                    {
                        Debug.WriteLine("Too many keys currently depressed to fit into the report data. Truncating.");
                        break;
                    }

                    reportValue[reportIndex] = keyPressedScanCode;
                    reportIndex++;
                }

                if (!reportValue.SequenceEqual(m_lastSentKeyboardReportValue))
                {
                    Debug.WriteLine("Sending keyboard report value notification with data: " + GetStringFromBuffer(reportValue));
                    reportValue.CopyTo(m_lastSentKeyboardReportValue, 0);

                    // Waiting for this operation to complete is no longer necessary since now ordering of notifications
                    // is guaranteed for each client. Not waiting for it to complete reduces delays and lags.
                    // Note that doing this makes us unable to know if the notification failed to be sent.
                    var asyncOp = m_hidKeyboardReport.NotifyValueAsync(reportValue.AsBuffer());
                }
            }
        }
    }
}
