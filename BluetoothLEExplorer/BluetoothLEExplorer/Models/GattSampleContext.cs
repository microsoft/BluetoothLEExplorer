// <copyright file="GattSampleContext.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using BluetoothLEExplorer.ViewModels;

using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Enumeration;
using Windows.UI.Popups;
using Windows.Foundation.Metadata;

using System.Threading;
using Template10.Common;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace BluetoothLEExplorer.Models
{
    /// <summary>
    /// Context for the entire app. This is where all app wide variables are stored
    /// </summary>
    public class GattSampleContext : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the app context
        /// </summary>
        public static GattSampleContext Context { get; private set; } = new GattSampleContext();

        /// <summary>
        /// AQS search string used to find bluetooth devices
        /// </summary>
        private const string BTLEDeviceWatcherAQSString = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";

        /// <summary>
        /// AQS search string to find bluetooth device dev nodes
        /// </summary>
        private const string DevNodeBTLEDeviceWatcherAQSString = "(System.Devices.ClassGuid:=\"{e0cbf06c-cd8b-4647-bb8a-263b43f0f974}\")";

        /// <summary>
        /// Gets or sets the list of available bluetooth devices
        /// </summary>
        public DisposableObservableCollection<ObservableBluetoothLEDevice> BluetoothLEDevices { get; set; } = new DisposableObservableCollection<ObservableBluetoothLEDevice>();

        private SemaphoreSlim AdvertisementsLock = new SemaphoreSlim(1, 1);

        public ObservableDictionary<String, ObservableBluetoothLEAdvertisement> Advertisements { get; set; } = new ObservableDictionary<String, ObservableBluetoothLEAdvertisement>();

        /// <summary>
        /// Gets or sets the selected bluetooth device
        /// </summary>
        public ObservableBluetoothLEDevice SelectedBluetoothLEDevice { get; set; } = null;

        /// <summary>
        /// Gets or sets the selected device service
        /// </summary>
        public ObservableGattDeviceService SelectedService { get; set; } = null;

        public ObservableBluetoothLEAdvertisement SelectedAdvertisement { get; set; } = null;

        /// <summary>
        /// Gets or sets the selected characteristic
        /// </summary>
        public ObservableGattCharacteristics SelectedCharacteristic { get; set; } = null;

        /// <summary>
        /// Gets or sets the selected descriptor
        /// </summary>
        public ObservableGattDescriptors SelectedDescriptor { get; set; } = null;

        /// <summary>
        /// Lock around the <see cref="BluetoothLEDevices"/>. Used in the Add/Removed/Updated callbacks
        /// </summary>
        private SemaphoreSlim BluetoothLEDevicesLock = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Lock around the <see cref="devNodes"/>. Used in the Add/Removed/Updated callbacks
        /// </summary>
        private SemaphoreSlim DevNodeLock = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Device watcher used to find bluetooth devices
        /// </summary>
        private DeviceWatcher deviceWatcher;

        /// <summary>
        /// Device watcher used to find bluetooth device dev nodes
        /// </summary>
        private DeviceWatcher devNodeWatcher;

        /// <summary>
        /// Advertisement watcher used to find bluetooth devices
        /// </summary>
        private BluetoothLEAdvertisementWatcher advertisementWatcher;

        private GattReliableWriteTransaction transaction;

        /// <summary>
        /// We need to cache all DeviceInformation objects we get as they may
        /// get updated in the future. The update may make them eligible to be put on
        /// the displayed list.
        /// </summary>
        private List<DeviceInformation> unusedDevices = new List<DeviceInformation>();

        /// <summary>
        /// All the bluetooth dev nodes on the system
        /// </summary>
        private List<DeviceInformation> devNodes = new List<DeviceInformation>();

        /// <summary>
        /// Gets or sets the list of created service
        /// </summary>
        public ObservableCollection<GenericGattServiceViewModel> CreatedServices { get; set; } = new ObservableCollection<GenericGattServiceViewModel>();

        /// <summary>
        /// Gets or sets the currently selected gatt server service
        /// </summary>
        public GenericGattServiceViewModel SelectedGattServerService { get; set; } = null;

        /// <summary>
        /// Event to notify when this object has changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Source for <see cref="IsEnumerating"/> property
        /// </summary>
        private bool isEnumerating = false;

        /// <summary>
        /// Gets a value indicating whether app is currently enumerating
        /// </summary>
        public bool IsEnumerating
        {
            get
            {
                return isEnumerating;
            }

            private set
            {
                if (isEnumerating != value)
                {
                    isEnumerating = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsEnumerating"));
                }
            }
        }

        /// <summary>
        /// Source for <see cref="EnumerationFinished"/> property
        /// </summary>
        private bool enumorationFinished = false;

        /// <summary>
        /// Gets a value indicating whether the app is finished enumerating
        /// </summary>
        public bool EnumerationFinished
        {
            get
            {
                return enumorationFinished;
            }

            private set
            {
                if (enumorationFinished != value)
                {
                    enumorationFinished = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("EnumerationFinished"));
                }
            }
        }

        private bool advertisementWatcherStarted = false;

        public bool AdvertisementWatcherStarted
        {
            get
            {
                return advertisementWatcherStarted;
            }

            private set
            {
                if (advertisementWatcherStarted != value)
                {
                    advertisementWatcherStarted = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("AdvertisementWatcherStarted"));
                }
            }
        }

        /// <summary>
        /// Source for <see cref="IsPeripheralRoleSupported"/>
        /// </summary>
        private bool isPeripheralRoleSupported = true;

        /// <summary>
        /// Gets a value indicating whether peripheral mode is supported by this device
        /// </summary>
        public bool IsPeripheralRoleSupported
        {
            get
            {
                return isPeripheralRoleSupported;
            }

            private set
            {
                if (isPeripheralRoleSupported != value)
                {
                    isPeripheralRoleSupported = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsPeripheralRoleSupported"));
                }
            }
        }

        /// <summary>
        /// Source for <see cref="IsCentralRoleSupported"/>
        /// </summary>
        private bool isCentralRoleSupported = true;

        /// <summary>
        /// Gets a value indicating whether central role is supported by this device
        /// </summary>
        public bool IsCentralRoleSupported
        {
            get
            {
                return isCentralRoleSupported;
            }

            private set
            {
                if (isCentralRoleSupported != value)
                {
                    isCentralRoleSupported = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsCentralRoleSupported"));
                }
            }
        }

        // Make this variable static so we only query IsPropertyPresent once
        private static bool isSecureConnectionSupported = ApiInformation.IsPropertyPresent("Windows.Devices.Bluetooth.BluetoothLEDevice", "WasSecureConnectionUsedForPairing");

        /// <summary>
        /// Gets a value indicating if the version of Windows on this machine supports
        /// secure connections.
        /// </summary>
        public bool IsSecureConnectionSupported
        {
            get
            {
                return isSecureConnectionSupported;
            }
        }

        private bool isTransactionInProgress = false;

        public bool IsTransactionInProgress
        {
            get
            {
                return isTransactionInProgress;
            }

            private set
            {
                if (isTransactionInProgress != value)
                {
                    isTransactionInProgress = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsTransactionInProgress"));
                }
            }
        }

        public String AdvertisementContentFilter { get; set; } = "";

        public ObservableCollection<ObservableBluetoothLEBeacon> Beacons { get; set; } = new ObservableCollection<ObservableBluetoothLEBeacon>();
        public ObservableBluetoothLEBeacon SelectedBeacon { get; set; } = null;

        /// <summary>
        /// Prevents a default instance of the <see cref="GattSampleContext" /> class from being created.
        /// </summary>
        private GattSampleContext()
        {
            Init();
        }

        /// <summary>
        /// Initializes the app context
        /// </summary>
        private async void Init()
        {
            Windows.Devices.Bluetooth.BluetoothAdapter adapter = await Windows.Devices.Bluetooth.BluetoothAdapter.GetDefaultAsync();

            if(adapter ==  null)
            {
                MessageDialog msg = new MessageDialog("Error getting access to Bluetooth adapter. Do you have a have bluetooth enabled?", "Error");
                await msg.ShowAsync();

                IsPeripheralRoleSupported = false;
                IsCentralRoleSupported = false;
            }
            else
            {
                IsPeripheralRoleSupported = adapter.IsPeripheralRoleSupported;
                IsCentralRoleSupported = adapter.IsCentralRoleSupported;
            }

            // Start the dev node watcher
            string[] requestedProperties = {};

            devNodeWatcher =
                DeviceInformation.CreateWatcher(
                    DevNodeBTLEDeviceWatcherAQSString,
                    requestedProperties,
                    DeviceInformationKind.Device);

            devNodeWatcher.Added += DevNodeWatcher_Added;
            devNodeWatcher.Removed += DevNodeWatcher_Removed;
            devNodeWatcher.Updated += DevNodeWatcher_Updated;

            devNodeWatcher.Start();

            advertisementWatcher = new BluetoothLEAdvertisementWatcher();
            advertisementWatcher.Received += AdvertisementWatcher_Received;
        }

        private async void DevNodeWatcher_Added(DeviceWatcher sender, DeviceInformation args)
        {
            try
            {
                await DevNodeLock.WaitAsync();
                devNodes.Add(args);
            }
            finally
            {
                DevNodeLock.Release();
            }
        }

        private async void DevNodeWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            try
            {
                await DevNodeLock.WaitAsync();

                int index = devNodes.FindIndex(x => x.Id == args.Id);
                devNodes.RemoveAt(index);
            }
            finally
            {
                DevNodeLock.Release();
            }

        }

        private async void DevNodeWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            DeviceInformation dev = null;
            try
            {
                await DevNodeLock.WaitAsync();
                dev = devNodes.FirstOrDefault(x => x.Id == args.Id);
            }
            finally
            {
                DevNodeLock.Release();
            }

            if(dev != null)
            {
                dev.Update(args);
            }
        }

        /// <summary>
        /// Add : to bluetooth address without them
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        private string GetDelimitedAddr(string addr)
        {
            // Add : delimiters to raw address
            var list = Enumerable.Range(0, addr.Length / 2).Select(i => addr.Substring(i * 2, 2)).ToList();
            return string.Join(":", list);
        }

        /// <summary>
        /// Clears all devices
        /// </summary>
        public void ClearAllDevices()
        {
            try
            {
                BluetoothLEDevicesLock.Wait();
                BluetoothLEDevices.Clear();
            }
            finally
            {
                BluetoothLEDevicesLock.Release();
            }
        }

        /// <summary>
        /// Release all resources without clearing all devices
        /// </summary>
        public void ReleaseAllResources()
        {
            try
            {
                BluetoothLEDevicesLock.Wait();
                BluetoothLEDevices.Dispose();
            }
            finally
            {
                BluetoothLEDevicesLock.Release();
            }
        }

        /// <summary>
        /// Starts enumeration of bluetooth device
        /// </summary>
        public void StartEnumeration()
        {
            // Additional properties we would like about the device.
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 5))
            {
                string[] requestedProperties =
                {
                    "System.Devices.GlyphIcon",
                    "System.Devices.Aep.Category",
                    "System.Devices.Aep.ContainerId",
                    "System.Devices.Aep.DeviceAddress",
                    "System.Devices.Aep.IsConnected",
                    "System.Devices.Aep.IsPaired",
                    "System.Devices.Aep.IsPresent",
                    "System.Devices.Aep.ProtocolId",
                    "System.Devices.Aep.Bluetooth.Le.IsConnectable",
                    "System.Devices.Aep.SignalStrength",
                    "System.Devices.Aep.Bluetooth.LastSeenTime",
                    "System.Devices.Aep.Bluetooth.Le.IsConnectable",
                };

                // BT_Code: Currently Bluetooth APIs don't provide a selector to get ALL devices that are both paired and non-paired.
                deviceWatcher = DeviceInformation.CreateWatcher(
                    BTLEDeviceWatcherAQSString,
                    requestedProperties,
                    DeviceInformationKind.AssociationEndpoint);
            }
            else
            {
                string[] requestedProperties =
                {
                    "System.Devices.GlyphIcon",
                    "System.Devices.Aep.Category",
                    "System.Devices.Aep.ContainerId",
                    "System.Devices.Aep.DeviceAddress",
                    "System.Devices.Aep.IsConnected",
                    "System.Devices.Aep.IsPaired",
                    "System.Devices.Aep.IsPresent",
                    "System.Devices.Aep.ProtocolId",
                    "System.Devices.Aep.Bluetooth.Le.IsConnectable",
                    "System.Devices.Aep.SignalStrength",
                };

                // BT_Code: Currently Bluetooth APIs don't provide a selector to get ALL devices that are both paired and non-paired.
                deviceWatcher = DeviceInformation.CreateWatcher(
                    BTLEDeviceWatcherAQSString,
                    requestedProperties,
                    DeviceInformationKind.AssociationEndpoint);
            }

            // Register event handlers before starting the watcher.
            deviceWatcher.Added += DeviceWatcher_Added;
            deviceWatcher.Updated += DeviceWatcher_Updated;
            deviceWatcher.Removed += DeviceWatcher_Removed;
            deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
            deviceWatcher.Stopped += DeviceWatcher_Stopped;

            ClearAllDevices();

            deviceWatcher.Start();
            IsEnumerating = true;
            EnumerationFinished = false;

            UpdateAdvertisementFilter(new BluetoothLEAdvertisementFilter());
            StartAdvertisementWatcher(BluetoothLEScanningMode.Active);
        }

        /// <summary>
        /// Stops enumeration of bluetooth device
        /// </summary>
        public void StopEnumeration()
        {
            StopAdvertisementWatcher();

            if (deviceWatcher != null)
            {
                // Unregister the event handlers.
                deviceWatcher.Added -= DeviceWatcher_Added;
                deviceWatcher.Updated -= DeviceWatcher_Updated;
                deviceWatcher.Removed -= DeviceWatcher_Removed;
                deviceWatcher.EnumerationCompleted -= DeviceWatcher_EnumerationCompleted;
                deviceWatcher.Stopped -= DeviceWatcher_Stopped;

                // Stop the watchers
                deviceWatcher.Stop();
                deviceWatcher = null;

                IsEnumerating = false;
                EnumerationFinished = false;
            }
        }

        public void StartAdvertisementWatcher(BluetoothLEScanningMode scanningMode)
        {
            if (!AdvertisementWatcherStarted)
            {
                Advertisements.Clear();
                advertisementWatcher.ScanningMode = scanningMode;
                if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 10))
                {
                    advertisementWatcher.AllowExtendedAdvertisements = true;
                }
                advertisementWatcher.Start();
                AdvertisementWatcherStarted = true;
            }
        }

        public void StopAdvertisementWatcher()
        {
            if (AdvertisementWatcherStarted)
            {
                advertisementWatcher.Stop();
                AdvertisementWatcherStarted = false;
            }
        }

        public void UpdateAdvertisementFilter(BluetoothLEAdvertisementFilter filter)
        {
            advertisementWatcher.AdvertisementFilter = filter;
            if (AdvertisementWatcherStarted)
            {
                advertisementWatcher.Stop();
                Advertisements.Clear();
                advertisementWatcher.Start();
            }
        }

        public void CreateTransaction()
        {
            transaction = new GattReliableWriteTransaction();
            IsTransactionInProgress = true;
        }

        public async void CommitTransaction()
        {
            var result = await transaction.CommitWithResultAsync();
            if (result.Status == GattCommunicationStatus.Success)
            {

            }

            transaction = null;
            IsTransactionInProgress = false;
        }

        public void WriteTransaction(GattCharacteristic characteristic, IBuffer value)
        {
            transaction.WriteValue(characteristic, value);
        }

        /// <summary>
        /// Updates device metadata based on advertisement received
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void AdvertisementWatcher_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            try
            {
                await AddAdvertisementToList(args);

                await BluetoothLEDevicesLock.WaitAsync();
                foreach (ObservableBluetoothLEDevice d in BluetoothLEDevices)
                {
                    if (d.BluetoothAddressAsUlong == args.BluetoothAddress)
                    {
                        await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                            Windows.UI.Core.CoreDispatcherPriority.Normal,
                            () =>
                            {
                                d.Update(args);
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AdvertisementWatcher_Received: ", ex.Message);
            }
            finally
            {
                BluetoothLEDevicesLock.Release();
            }
        }

        /// <summary>
        /// Callback when a new device is found
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="deviceInfo"></param>
        private async void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation deviceInfo)
        {
            try
            {
                // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                if (sender == deviceWatcher)
                {
                    await AddDeviceToList(deviceInfo);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DeviceWatcher_Added: " + ex.Message);
            }
        }

        /// <summary>
        /// Executes when a device is updated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="deviceInfoUpdate"></param>
        private async void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            DeviceInformation di = null;
            bool addNewDI = false;

            try
            {
                // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                if (sender == deviceWatcher)
                {
                    ObservableBluetoothLEDevice dev;

                    // Need to lock as another DeviceWatcher might be modifying BluetoothLEDevices 
                    try
                    {
                        await BluetoothLEDevicesLock.WaitAsync();
                        dev = BluetoothLEDevices.FirstOrDefault(device => device.DeviceInfo.Id == deviceInfoUpdate.Id);
                        if (dev != null)
                        {   // Found a device in the list, updating it
                            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                                Windows.UI.Core.CoreDispatcherPriority.Normal,
                                async () =>
                                {
                                    dev.Update(deviceInfoUpdate);
                                });
                        }
                        else
                        {
                            // Need to add this device. Can't do that here as we have the lock
                            addNewDI = true;
                        }
                    }
                    finally
                    {
                        BluetoothLEDevicesLock.Release();
                    }

                    if(addNewDI == true)
                    {
                        try
                        {
                            await BluetoothLEDevicesLock.WaitAsync();
                            di = unusedDevices.FirstOrDefault(device => device.Id == deviceInfoUpdate.Id);
                            if (di != null)
                            {   // We found this device before.
                                unusedDevices.Remove(di);
                                di.Update(deviceInfoUpdate);
                            }
                        }
                        finally
                        {
                            BluetoothLEDevicesLock.Release();
                        }

                        if (di != null)
                        {
                            await AddDeviceToList(di);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DeviceWatcher_Updated: " + ex.Message);
            }
        }

        /// <summary>
        /// Executes when a device is removed from enumeration
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="deviceInfoUpdate"></param>
        private async void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            try
            {
                // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                if (sender == deviceWatcher)
                {
                    ObservableBluetoothLEDevice dev;

                    try
                    {
                        // Need to lock as another DeviceWatcher might be modifying BluetoothLEDevices 
                        await BluetoothLEDevicesLock.WaitAsync();

                        // Find the corresponding DeviceInformation in the collection and remove it.
                        dev = BluetoothLEDevices.FirstOrDefault(device => device.DeviceInfo.Id == deviceInfoUpdate.Id);
                        if (dev != null)
                        {   // Found it in our displayed devices
                            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                                Windows.UI.Core.CoreDispatcherPriority.Normal,
                                () =>
                            {
                                Debug.Assert(BluetoothLEDevices.Remove(dev), "DeviceWatcher_Removed: Failed to remove device from list");
                            });
                        }
                        else
                        {   // Did not find in diplayed list, let's check the unused list
                            DeviceInformation di = unusedDevices.FirstOrDefault(device => device.Id == deviceInfoUpdate.Id);

                            if (di != null)
                            {   // Found in unused devices, remove it
                                Debug.Assert(unusedDevices.Remove(di), "DeviceWatcher_Removed: Failed to remove device from unused");
                            }
                        }
                    }
                    finally
                    {
                        BluetoothLEDevicesLock.Release();
                    }

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DeviceWatcher_Removed: " + ex.Message);
            }
        }

        /// <summary>
        /// Executes when Enumeration has finished
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object e)
        {
            // Protect against race condition if the task runs after the app stopped the deviceWatcher.
            if (sender == deviceWatcher)
            {
                StopEnumeration();
                EnumerationFinished = true;
            }
        }

        /// <summary>
        /// Adds the new or updated device to the displayed or unused list
        /// </summary>
        /// <param name="deviceInfo"></param>
        /// <returns></returns>
        private async Task AddDeviceToList(DeviceInformation deviceInfo)
        {
            ObservableBluetoothLEDevice dev = new ObservableBluetoothLEDevice(deviceInfo);

            // Let's make it connectable by default, we have error handles in case it doesn't work
            bool shouldDisplay =
                ((dev.DeviceInfo.Properties.Keys.Contains("System.Devices.Aep.Bluetooth.Le.IsConnectable") &&
                    (bool)dev.DeviceInfo.Properties["System.Devices.Aep.Bluetooth.Le.IsConnectable"])) ||
                ((dev.DeviceInfo.Properties.Keys.Contains("System.Devices.Aep.IsConnected") &&
                    (bool)dev.DeviceInfo.Properties["System.Devices.Aep.IsConnected"])) ||
                ((dev.DeviceInfo.Properties.Keys.Contains("System.Devices.Aep.IsPaired") &&
                    (bool)dev.DeviceInfo.Properties["System.Devices.Aep.IsPaired"]));
                
            if (shouldDisplay)
            {
                // Need to lock as another DeviceWatcher might be modifying BluetoothLEDevices 
                try
                {
                    await BluetoothLEDevicesLock.WaitAsync();

                    if (!BluetoothLEDevices.Contains(dev))
                    {
                        await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                            Windows.UI.Core.CoreDispatcherPriority.Normal,
                            () =>
                        {
                            BluetoothLEDevices.Add(dev);
                        });
                    }
                }
                finally
                {
                    BluetoothLEDevicesLock.Release();
                }
            }
            else
            {
                try
                {
                    await BluetoothLEDevicesLock.WaitAsync();
                    unusedDevices.Add(deviceInfo);
                    dev.Dispose();
                }
                finally
                {
                    BluetoothLEDevicesLock.Release();
                }
            }
        }

        private async Task AddAdvertisementToList(BluetoothLEAdvertisementReceivedEventArgs advertisementEvent)
        {
            try
            {
                await AdvertisementsLock.WaitAsync();

                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                    Windows.UI.Core.CoreDispatcherPriority.Normal,
                    () =>
                    {
                        var advertisement = new ObservableBluetoothLEAdvertisement(advertisementEvent);

                        Advertisements[advertisement.InternalHashString] = advertisement;
                    });
            }
            finally
            {
                AdvertisementsLock.Release();
            }
        }

        /// <summary>
        /// Executes when device watcher has stopped
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeviceWatcher_Stopped(DeviceWatcher sender, object e)
        {
            // Implimented for completeness
        }

        /// <summary>
        /// Executes when a property has changed
        /// </summary>
        /// <param name="e"></param>
        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }
    }
}
