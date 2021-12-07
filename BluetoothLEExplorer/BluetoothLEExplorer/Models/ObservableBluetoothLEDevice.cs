// <copyright file="ObservableBluetoothLEDevice.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BluetoothLEExplorer.Services.DispatcherService;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Foundation.Metadata;
using System.Collections;
using System.Collections.Generic;
using BluetoothLEExplorer.Services.GattUuidHelpers;

namespace BluetoothLEExplorer.Models
{
    /// <summary>
    /// Wrapper around <see cref="BluetoothLEDevice"/> to make it easier to use
    /// </summary>
    public class ObservableBluetoothLEDevice : INotifyPropertyChanged, IEquatable<ObservableBluetoothLEDevice>, IDisposable
    {
        /// <summary>
        /// Compares RSSI values between ObservableBluetoothLEDevice. Sorts based on closest to furthest where 0 is unknown
        /// and is sorted as furthest away
        /// </summary>
        public class RSSIComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                ObservableBluetoothLEDevice a = x as ObservableBluetoothLEDevice;
                ObservableBluetoothLEDevice b = y as ObservableBluetoothLEDevice;

                if( a == null || b == null)
                {
                    throw new InvalidOperationException("Compared objects are not ObservableBluetoothLEDevice");
                }

                // If they're equal sort by bluetooth address
                if(a.RSSI == b.RSSI)
                {
                    return a.BluetoothAddressAsString.CompareTo(b.BluetoothAddressAsString);
                }

                // RSSI == 0 means we don't know it. Always make that the end.
                if(b.RSSI == 0)
                {
                    return -1;
                }

                if(a.RSSI < b.RSSI || a.rssi == 0)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
        }

        /// <summary>
        /// Source for <see cref="BluetoothLEDevice"/>
        /// </summary>
        private BluetoothLEDevice bluetoothLEDevice;

        /// <summary>
        /// Gets the bluetooth device this class wraps
        /// </summary>
        public BluetoothLEDevice BluetoothLEDevice
        {
            get
            {
                return bluetoothLEDevice;
            }

            private set
            {
                bluetoothLEDevice = value;
                OnPropertyChanged(new PropertyChangedEventArgs("BluetoothLEDevice"));
            }
        }

        /// <summary>
        /// Source for <see cref="Glyph"/>
        /// </summary>
        private BitmapImage glyph;

        /// <summary>
        /// Gets or sets the glyph of this bluetooth device
        /// </summary>
        public BitmapImage Glyph
        {
            get
            {
                return glyph;
            }

            set
            {
                glyph = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Glyph"));
            }
        }

        /// <summary>
        /// Source for <see cref="DeviceInfo"/>
        /// </summary>
        private DeviceInformation deviceInfo;

        /// <summary>
        /// Gets the device information for the device this class wraps
        /// </summary>
        public DeviceInformation DeviceInfo
        {
            get
            {
                return deviceInfo;
            }

            private set
            {
                deviceInfo = value;
                OnPropertyChanged(new PropertyChangedEventArgs("DeviceInfo"));
            }
        }

        /// <summary>
        /// Source for <see cref="IsConnected"/>
        /// </summary>
        private bool isConnected;

        /// <summary>
        /// Gets or sets a value indicating whether this device is connected
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return isConnected;
            }

            set
            {
                if (isConnected != value)
                {
                    isConnected = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsConnected"));
                }
            }
        }

        /// <summary>
        /// Gets if the device is connectable
        /// </summary>
        public bool IsConnectable
        {
            get
            {
                return DeviceInfo.Properties.Keys.Contains("System.Devices.Aep.Bluetooth.Le.IsConnectable") &&
                            (bool)DeviceInfo.Properties["System.Devices.Aep.Bluetooth.Le.IsConnectable"];
            }
        }

        /// <summary>
        /// Source for <see cref="LastSeenTime"/>
        /// </summary>
        private DateTime lastSeenTime;

        /// <summary>
        /// Gets or sets a value indicating the last time an advertisement was seen from the device
        /// </summary>
        public DateTime LastSeenTime
        {
            get
            {
                return lastSeenTime;
            }

            set
            {
                if (lastSeenTime != value)
                {
                    lastSeenTime = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("LastSeenTime"));
                }
            }
        }

        /// <summary>
        /// Source for <see cref="IsPaired"/>
        /// </summary>
        private bool isPaired;

        /// <summary>
        /// Gets or sets a value indicating whether this device is paired
        /// </summary>
        public bool IsPaired
        {
            get
            {
                return isPaired;
            }

            set
            {
                if (isPaired != value)
                {
                    isPaired = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsPaired"));
                }
            }
        }

        /// <summary>
        /// Source for <see cref="CanPair"/>
        /// </summary>
        private bool canPair;

        /// <summary>
        /// Gets or sets a value indicating whether this device is paired
        /// </summary>
        public bool CanPair
        {
            get
            {
                return canPair;
            }

            set
            {
                if (canPair != value)
                {
                    canPair = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("CanPair"));
                }
            }
        }

        private bool isSecureConnection;

        public bool IsSecureConnection
        {
            get
            {
                return isSecureConnection;
            }

            set
            {
                if (isSecureConnection != value)
                {
                    isSecureConnection = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsSecureConnection"));
                }
            }
        }

        // Make this variable static so we only query IsPropertyPresent once
        private static bool isSecureConnectionSupported = ApiInformation.IsPropertyPresent("Windows.Devices.Bluetooth.BluetoothLEDevice", "WasSecureConnectionUsedForPairing");

        private ObservableCollection<ObservableBluetoothLEAdvertisement> advertisements = new ObservableCollection<ObservableBluetoothLEAdvertisement>();

        public ObservableCollection<ObservableBluetoothLEAdvertisement> Advertisements
        {
            get
            {
                return advertisements;
            }
        }

        /// <summary>
        /// Source for <see cref="Services"/>
        /// </summary>
        private DisposableObservableCollection<ObservableGattDeviceService> services = new DisposableObservableCollection<ObservableGattDeviceService>();

        /// <summary>
        /// Gets the services this device supports
        /// </summary>
        public DisposableObservableCollection<ObservableGattDeviceService> Services
        {
            get
            {
                return services;
            }

            private set
            {
                services = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Services"));
            }
        }

        /// <summary>
        /// Source for <see cref="ServiceCount"/>
        /// </summary>
        private int serviceCount;

        /// <summary>
        /// Gets or sets the number of services this device has
        /// </summary>
        public int ServiceCount
        {
            get
            {
                return serviceCount;
            }

            private set
            {
                if (serviceCount < value)
                {
                    serviceCount = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("ServiceCount"));
                }
            }
        }

        private int advertisementServiceCount;

        public int AdvertisementServiceCount
        {
            get
            {
                return advertisementServiceCount;
            }

            private set
            {
                if (advertisementServiceCount < value)
                {
                    advertisementServiceCount = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("AdvertisementServiceCount"));
                }
            }
        }

        /// <summary>
        /// Source for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Gets the name of this device
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }

            private set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Name"));
                }
            }
        }

        /// <summary>
        /// Source for <see cref="ErrorText"/>
        /// </summary>
        private string errorText;

        /// <summary>
        /// Gets the error text when connecting to this device fails
        /// </summary>
        public string ErrorText
        {
            get
            {
                return errorText;
            }

            private set
            {
                errorText = value;
                OnPropertyChanged(new PropertyChangedEventArgs("ErrorText"));
            }
        }

        private Queue<int> RssiValue = new Queue<int>(10);

        /// <summary>
        /// Source for <see cref="RSSI"/>
        /// </summary>
        private int rssi;

        /// <summary>
        /// Gets the RSSI value of this device
        /// </summary>
        public int RSSI
        {
            get
            {
                return rssi;
            }

            private set
            {
                if (RssiValue.Count >= 10)
                {
                    RssiValue.Dequeue();
                }
                RssiValue.Enqueue(value);

                int newValue = (int)Math.Round(RssiValue.Average(), 0);

                if (rssi < newValue - 2 ||
                    rssi > newValue + 2)
                {
                    rssi = newValue;
                    OnPropertyChanged(new PropertyChangedEventArgs("RSSI"));
                }
            }
        }

        private string bluetoothAddressAsString;
        /// <summary>
        /// Gets the bluetooth address of this device as a string
        /// </summary>
        public string BluetoothAddressAsString
        {
            get
            {
                return bluetoothAddressAsString;
            }

            private set
            {
                if(bluetoothAddressAsString != value)
                {
                    bluetoothAddressAsString = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("BluetoothAddressAsString"));
                }
            }
        }

        private ulong bluetoothAddressAsUlong;
        /// <summary>
        /// Gets the bluetooth address of this device
        /// </summary>
        public ulong BluetoothAddressAsUlong
        {
            get
            {
                return bluetoothAddressAsUlong;
            }

            private set
            {
                if (bluetoothAddressAsUlong != value)
                {
                    bluetoothAddressAsUlong = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("BluetoothAddressAsUlong"));
                }
            }
        }

        private ulong gattServicesChangedInstances = 0;

        public ulong GattServicesChangedInstances
        {
            get
            {
                return gattServicesChangedInstances;
            }

            private set
            {
                if (gattServicesChangedInstances != value)
                {
                    gattServicesChangedInstances = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("GattServicesChangedInstances"));
                }
            }
        }

        /// <summary>
        /// Releases references to Services and the BluetoothLEDevice
        /// </summary>
        public void Dispose()
        {
            lock (Services)
            {
                Services.Clear();
                ServiceCount = 0;
            }

            var temp = bluetoothLEDevice;
            BluetoothLEDevice = null;

            if (temp != null)
            {
                // Tear down event handlers
                temp.ConnectionStatusChanged -= BluetoothLEDevice_ConnectionStatusChanged;
                temp.NameChanged -= BluetoothLEDevice_NameChanged;
                temp.GattServicesChanged -= BluetoothLEDevice_GattServicesChanged;
                temp.Dispose();
            }

            IsConnected = false;
        }

        /// <summary>
        /// Initializes a new instance of the<see cref="ObservableBluetoothLEDevice" /> class.
        /// </summary>
        /// <param name="deviceInfo">The device info that describes this bluetooth device"/></param>
        public ObservableBluetoothLEDevice(DeviceInformation deviceInfo)
        {
            DeviceInfo = deviceInfo;
            Name = DeviceInfo.Name;

            string ret = String.Empty;

            if (DeviceInfo.Properties.ContainsKey("System.Devices.Aep.DeviceAddress"))
            {
                BluetoothAddressAsString = ret = DeviceInfo.Properties["System.Devices.Aep.DeviceAddress"].ToString();
                BluetoothAddressAsUlong = Convert.ToUInt64(BluetoothAddressAsString.Replace(":", String.Empty), 16);
            }

            IsPaired = DeviceInfo.Pairing.IsPaired;
            CanPair = DeviceInfo.Pairing.CanPair;

            LoadGlyph();

            this.PropertyChanged += ObservableBluetoothLEDevice_PropertyChanged;
        }

        private void ObservableBluetoothLEDevice_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "DeviceInfo")
            {
                if (DeviceInfo.Properties.ContainsKey("System.Devices.Aep.Bluetooth.LastSeenTime") && (DeviceInfo.Properties["System.Devices.Aep.Bluetooth.LastSeenTime"] != null))
                {
                    LastSeenTime = ((System.DateTimeOffset)DeviceInfo.Properties["System.Devices.Aep.Bluetooth.LastSeenTime"]).UtcDateTime;
                }
            }
        }

        /// <summary>
        /// result of finding all the services
        /// </summary>
        private GattDeviceServicesResult result;

        /// <summary>
        /// Event to notify when this object has changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Connect to this bluetooth device
        /// </summary>
        /// <returns>Connection task</returns>
        public async Task<bool> Connect()
        {
            bool ret = false;
            string debugMsg = String.Format("Connect: ");

            Debug.WriteLine(debugMsg + "Entering");

            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunTaskAsync(async () =>
            {
                Debug.WriteLine(debugMsg + "In UI thread");
                try
                {
                    Dispose();
                    Debug.WriteLine(debugMsg + "Calling BluetoothLEDevice.FromIdAsync");
                    BluetoothLEDevice = await BluetoothLEDevice.FromIdAsync(DeviceInfo.Id);

                    if (bluetoothLEDevice == null)
                    {
                        ret = false;
                        Debug.WriteLine(debugMsg + "BluetoothLEDevice is null");

                        MessageDialog dialog = new MessageDialog("No permission to access device", "Connection error");
                        await dialog.ShowAsync();
                    }
                    else
                    {
                        Debug.WriteLine(debugMsg + "BluetoothLEDevice is " + BluetoothLEDevice.Name);

                        // Setup our event handlers and view model properties
                        BluetoothLEDevice.ConnectionStatusChanged += BluetoothLEDevice_ConnectionStatusChanged;
                        BluetoothLEDevice.NameChanged += BluetoothLEDevice_NameChanged;
                        GattServicesChangedInstances = 0;
                        BluetoothLEDevice.GattServicesChanged += BluetoothLEDevice_GattServicesChanged;

                        IsPaired = DeviceInfo.Pairing.IsPaired;
                        CanPair = DeviceInfo.Pairing.CanPair;
                        IsConnected = BluetoothLEDevice.ConnectionStatus == BluetoothConnectionStatus.Connected;
                        Name = BluetoothLEDevice.Name;

                        UpdateSecureConnectionStatus();

                        BluetoothCacheMode cacheMode =  BluetoothLEExplorer.Services.SettingsServices.SettingsService.Instance.UseCaching ? BluetoothCacheMode.Cached : BluetoothCacheMode.Uncached;

                        ret = await GetAllPrimaryServices(cacheMode);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(debugMsg + "Exception - " + ex.Message);
                    string msg = String.Format("Message:\n{0}\n\nInnerException:\n{1}\n\nStack:\n{2}", ex.Message, ex.InnerException, ex.StackTrace);

                    var messageDialog = new MessageDialog(msg, "Exception");
                    await messageDialog.ShowAsync();

                    // Debugger break here so we can catch unknown exceptions
                    Debugger.Break();
                }
            });

            if (ret)
            {
                Debug.WriteLine(debugMsg + "Exiting (0)");
            }
            else
            {
                Debug.WriteLine(debugMsg + "Exiting (-1)");
            }

            return ret;
        }

        private async Task<bool> GetAllPrimaryServices(BluetoothCacheMode cacheMode)
        {
            var succeeded = false;
            string debugMsg = String.Format("GetAllPrimaryServices: ");

            Debug.WriteLine(debugMsg + "Entering");

            // Get all the services for this device
            var result = await BluetoothLEDevice.GetGattServicesAsync(cacheMode);

            if (result.Status == GattCommunicationStatus.Success)
            {
                System.Diagnostics.Debug.WriteLine(debugMsg + "GetGattServiceAsync SUCCESS");

                lock (Services)
                {
                    foreach (var serv in result.Services)
                    {
                        if (!GattServiceUuidHelper.IsReserved(serv.Uuid))
                        {
                            var temp = new ObservableGattDeviceService(serv);
                            // This isn't awaited so that the user can disconnect while the services are still being enumerated
                            temp.Initialize();
                            Services.Add(temp);
                        }
                        else
                        {
                            serv.Dispose();
                        }
                    }
                    ServiceCount = Services.Count();
                }

                succeeded = true;
            }
            else if (result.Status == GattCommunicationStatus.ProtocolError)
            {
                ErrorText = debugMsg + "GetGattServiceAsync Error: Protocol Error - " + result.ProtocolError.Value;
                System.Diagnostics.Debug.WriteLine(ErrorText);
                string msg = "Connection protocol error: " + result.ProtocolError.Value.ToString();
                var messageDialog = new MessageDialog(msg, "Connection failures");
                await messageDialog.ShowAsync();
            }
            else if (result.Status == GattCommunicationStatus.Unreachable)
            {
                ErrorText = debugMsg + "GetGattServiceAsync Error: Unreachable";
                System.Diagnostics.Debug.WriteLine(ErrorText);
                string msg = "Device unreachable";
                var messageDialog = new MessageDialog(msg, "Connection failures");
                await messageDialog.ShowAsync();
            }

            return succeeded;
        }

        public async Task<bool> DoInAppPairing()
        {
            Debug.WriteLine("Trying in app pairing");

            // BT_Code: Pair the currently selected device.
            DevicePairingResult result = await DeviceInfo.Pairing.PairAsync();

            CanPair = DeviceInfo.Pairing.CanPair;
            IsPaired = DeviceInfo.Pairing.IsPaired;

            Debug.WriteLine($"Pairing result: {result.Status.ToString()}");

            if (result.Status == DevicePairingResultStatus.Paired ||
                result.Status == DevicePairingResultStatus.AlreadyPaired)
            {
                return true;
            }
            else
            {
                MessageDialog d = new MessageDialog("Pairing error", result.Status.ToString());
                await d.ShowAsync();
                return false;
            }
        }

        /// <summary>
        /// Executes when the name of this devices changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void BluetoothLEDevice_NameChanged(BluetoothLEDevice sender, object args)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
            {
                try
                {
                    Name = BluetoothLEDevice.Name;
                }
                catch
                {
                    Name = "Unknown (exception)";
                }
            });
        }

        private async void BluetoothLEDevice_GattServicesChanged(BluetoothLEDevice sender, object args)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
            {
                GattServicesChangedInstances += 1;
            });

        }

        /// <summary>
        /// Executes when the connection state changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void BluetoothLEDevice_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
            {
                IsPaired = DeviceInfo.Pairing.IsPaired;
                CanPair = DeviceInfo.Pairing.CanPair;
                try
                {
                    IsConnected = bluetoothLEDevice.ConnectionStatus == BluetoothConnectionStatus.Connected;
                    UpdateSecureConnectionStatus();
                }
                catch
                {
                    IsConnected = false;
                }
            });
        }

        /// <summary>
        /// Load the glyph for this device
        /// </summary>
        private async void LoadGlyph()
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                async () =>
                {
                    try
                    {
                        DeviceThumbnail deviceThumbnail = await DeviceInfo.GetGlyphThumbnailAsync();
                        BitmapImage glyphBitmapImage = new BitmapImage();
                        await glyphBitmapImage.SetSourceAsync(deviceThumbnail);
                        Glyph = glyphBitmapImage;
                    }
                    catch (Exception)
                    {
                        Glyph = null;
                    }
                });
        }

        /// <summary>
        /// Executes when a property is changed
        /// </summary>
        /// <param name="e"></param>
        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }

        /// <summary>
        /// Overrides the ToString function to return the name of the device
        /// </summary>
        /// <returns>Name of this characteristic</returns>
        public override string ToString()
        {
            return this.name;
        }

        /// <summary>
        /// Compares this device to other bluetooth devices by checking the id
        /// </summary>
        /// <param name="other"></param>
        /// <returns>true for equal</returns>
        bool IEquatable<ObservableBluetoothLEDevice>.Equals(ObservableBluetoothLEDevice other)
        {
            if (other == null)
            {
                return false;
            }

            if (this.DeviceInfo.Id == other.DeviceInfo.Id)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Updates this device's deviceInformation
        /// </summary>
        /// <param name="deviceUpdate"></param>
        public void Update(DeviceInformationUpdate deviceUpdate)
        {
            DeviceInfo.Update(deviceUpdate);

            OnPropertyChanged(new PropertyChangedEventArgs("DeviceInfo"));
        }

        /// <summary>
        /// Helper method which checks to see if the Secure Connection APIs are supported
        /// and updates the current status.
        /// </summary>
        private void UpdateSecureConnectionStatus()
        {
            if (isSecureConnectionSupported)
            {
                IsSecureConnection = bluetoothLEDevice.WasSecureConnectionUsedForPairing;
            }
            else
            {
                IsSecureConnection = false;
            }
        }

        public void Update(BluetoothLEAdvertisementReceivedEventArgs advertisementEvent)
        {
            // Update our current RSSI for the device.
            RSSI = advertisementEvent.RawSignalStrengthInDBm;

            if (advertisementEvent.Advertisement.ServiceUuids != null)
            {
                AdvertisementServiceCount = advertisementEvent.Advertisement.ServiceUuids.Count();
            }

            Advertisements.Add(new ObservableBluetoothLEAdvertisement(advertisementEvent));
        }
    }
}
