// <copyright file="ObservableGattDeviceService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BluetoothLEExplorer.Services.GattUuidHelpers;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BluetoothLEExplorer.Models
{
    /// <summary>
    /// Wrapper around <see cref="GattDeviceService"/> to make it easier to use
    /// </summary>
    public class ObservableGattDeviceService : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// Source for <see cref="Service"/>
        /// </summary>
        private GattDeviceService service;

        /// <summary>
        /// Gets or sets the service this class wraps
        /// </summary>
        public GattDeviceService Service
        {
            get
            {
                return service;
            }

            set
            {
                if (service != value)
                {
                    service = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Service"));
                }
            }
        }

        public void Dispose()
        {
            var temp = service;
            Service = null;
            if (temp != null)
            {
                temp.Dispose();
            }
        }

        /// <summary>
        /// Source for <see cref="Characteristics"/>
        /// </summary>
        private ObservableCollection<ObservableGattCharacteristics> characteristics = new ObservableCollection<ObservableGattCharacteristics>();

        /// <summary>
        /// Gets or sets all the characteristics of this service
        /// </summary>
        public ObservableCollection<ObservableGattCharacteristics> Characteristics
        {
            get
            {
                return characteristics;
            }

            set
            {
                if (characteristics != value)
                {
                    characteristics = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Characteristics"));
                }
            }
        }

        /// <summary>
        /// Source for <see cref="SelectedCharacteristic"/>
        /// </summary>
        private ObservableGattCharacteristics selectedCharacteristic;

        /// <summary>
        /// Gets or sets the currently selected characteristic
        /// </summary>
        public ObservableGattCharacteristics SelectedCharacteristic
        {
            get
            {
                return selectedCharacteristic;
            }

            set
            {
                if (selectedCharacteristic != value)
                {
                    selectedCharacteristic = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("SelectedCharacteristic"));

                    // The SelectedProperty doesn't exist when this object is first created. This takes
                    // care of adding the correct event handler after the first time it's changed.
                    SelectedCharacteristic_PropertyChanged();
                }
            }
        }

        /// <summary>
        /// Source for <see cref="Name"/>
        /// </summary>
        private string name;

        /// <summary>
        /// Gets or sets the name of this service
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Name"));
                }
            }
        }

        /// <summary>
        /// Source for <see cref="UUID"/>
        /// </summary>
        private string uuid;

        /// <summary>
        /// Gets or sets the UUID of this service
        /// </summary>
        public string UUID
        {
            get
            {
                return uuid;
            }

            set
            {
                if (uuid != value)
                {
                    uuid = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("UUID"));
                }
            }
        }

        private ushort attributeHandle;

        public ushort AttributeHandle
        {
            get
            {
                return attributeHandle;
            }

            set
            {
                if (attributeHandle != value)
                {
                    attributeHandle = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("AttributeHandle"));
                }
            }
        }

        /// <summary>
        /// Determines if the SelectedCharacteristic_PropertyChanged has been added
        /// </summary>
        private bool hasSelectedCharacteristicPropertyChangedHandler = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableGattDeviceService" /> class.
        /// </summary>
        /// <param name="service">The service this class wraps</param>
        public ObservableGattDeviceService(GattDeviceService service)
        {
            Service = service;
            Name = GattServiceUuidHelper.ConvertUuidToName(service.Uuid);
            UUID = service.Uuid.ToString();
        }

        public async Task Initialize()
        {
            await GetAllServiceAttributes();
        }

        private async Task GetAllServiceAttributes()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("ObservableGattDeviceService::GetAllServiceAttributes: ");
            sb.Append(Name);

            // Request the necessary access permissions for the service and abort
            // if permissions are denied.
            GattOpenStatus status = await Service.OpenAsync(GattSharingMode.SharedReadAndWrite);
            if (status != GattOpenStatus.Success && status != GattOpenStatus.AlreadyOpened)
            {
                string error = " - Error: " + status.ToString();
                Name += error;
                sb.Append(error);
                Debug.WriteLine(sb.ToString());
                return;
            }

            await GetAllIncludedServices();
            await GetAllCharacteristics();
        }

        /// <summary>
        /// Adds the SelectedCharacteristic_PropertyChanged event handler
        /// </summary>
        private void SelectedCharacteristic_PropertyChanged()
        {
            if (hasSelectedCharacteristicPropertyChangedHandler == false)
            {
                selectedCharacteristic.PropertyChanged += SelectedCharacteristic_PropertyChanged;
                hasSelectedCharacteristicPropertyChangedHandler = true;
            }
        }

        /// <summary>
        /// Updates the selected characteristic in the app context
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectedCharacteristic_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            GattSampleContext.Context.SelectedCharacteristic = selectedCharacteristic;
        }

        /// <summary>
        /// Turns on notifications for all characterisitics within the service
        /// </summary>
        public async Task<bool> TurnOnAllNotifications()
        {
            bool success = true;
            foreach (var gattchar in Characteristics)
            {
                if (gattchar.Characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
                {
                    if (!await gattchar.SetNotify())
                    {
                        success = false;
                    }
                }
            }
            return success;
        }

        /// <summary>
        /// Turns off notifications for all characterisitics within the service
        /// </summary>
        public async Task<bool> TurnOffAllNotifications()
        {
            bool success = true;
            foreach (var gattchar in Characteristics)
            {
                if (gattchar.Characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
                {
                    if (!await gattchar.StopNotify())
                    {
                        success = false;
                    }
                }
            }
            return success;
        }

        /// <summary>
        /// Turns on indications for all characterisitics within the service
        /// </summary>
        public async Task<bool> TurnOnAllIndications()
        {
            bool success = true;
            foreach (var gattchar in Characteristics)
            {
                if (gattchar.Characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Indicate))
                {
                    if (!await gattchar.SetIndicate())
                    {
                        success = false;
                    }
                }
            }
            return success;
        }

        /// <summary>
        /// Turns on notifications for all characterisitics within the service
        /// </summary>
        public async Task<bool> TurnOffAllIndications()
        {
            bool success = true;
            foreach (var gattchar in Characteristics)
            {
                if (gattchar.Characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Indicate))
                {
                    if (!await gattchar.StopIndicate())
                    {
                        success = false;
                    }
                }
            }
            return success;
        }

        /// <summary>
        /// Verify that the service has at least one characterisitic that has Notify enabled
        /// </summary>
        public bool CanNotify()
        {
            foreach (var gattchar in Characteristics)
            {
                if (gattchar.Characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Verify that the service has at least one characterisitic that has Indicate enabled
        /// </summary>
        public bool CanIndicate()
        {
            foreach (var gattchar in Characteristics)
            {
                if (gattchar.Characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Indicate))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Verify that all characterisitcs of the service that have Notify enabled have Notify set
        /// </summary>
        public bool IsNotifySet()
        {
            foreach (var gattchar in Characteristics)
            {
                if (gattchar.Characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
                {
                    if (!gattchar.IsNotifySet)
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Verify that all characterisitcs of the service that have Indicate enabled have Indicate set
        /// </summary>
        public bool IsIndicateSet()
        {
            foreach (var gattchar in Characteristics)
            {
                if (gattchar.Characteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Indicate))
                {
                    if (!gattchar.IsIndicateSet)
                        return false;
                }
            }
            return true;
        }

        private async Task GetAllIncludedServices()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("ObservableGattDeviceService::getAllIncludedServices: ");
            sb.Append(name);

            try
            {
                var result = await service.GetIncludedServicesAsync(Services.SettingsServices.SettingsService.Instance.UseCaching ? BluetoothCacheMode.Cached : BluetoothCacheMode.Uncached);
                if (result.Status == GattCommunicationStatus.Success)
                {
                    sb.Append(" - getAllIncludedServices found ");
                    sb.Append(result.Services.Count());
                    sb.Append(" services");
                    Debug.WriteLine(sb);
                }
                else if (result.Status == GattCommunicationStatus.Unreachable)
                {
                    sb.Append(" - getAllIncludedServices failed with Unreachable");
                    Debug.WriteLine(sb.ToString());
                }
                else if (result.Status == GattCommunicationStatus.ProtocolError)
                {
                    sb.Append(" - getAllIncludedServices failed with Unreachable");
                    Debug.WriteLine(sb.ToString());
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine("getAllIncludedServices: Exception - {0}" + ex.Message);
                Name += " - Exception: " + ex.Message;
            }
        }

        /// <summary>
        /// Gets all the characteristics of this service
        /// </summary>
        private async Task GetAllCharacteristics()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("ObservableGattDeviceService::getAllCharacteristics: ");
            sb.Append(name);

            try
            {
                // Request the necessary access permissions for the service and abort
                // if permissions are denied.
                GattOpenStatus status = await service.OpenAsync(GattSharingMode.SharedReadAndWrite);

                if (status != GattOpenStatus.Success && status != GattOpenStatus.AlreadyOpened)
                {
                    string error = " - Error: " + status.ToString();
                    Name += error;
                    sb.Append(error);
                    Debug.WriteLine(sb.ToString());
                    return;
                }

                var result = await service.GetCharacteristicsAsync(Services.SettingsServices.SettingsService.Instance.UseCaching ? BluetoothCacheMode.Cached : BluetoothCacheMode.Uncached);

                if (result.Status == GattCommunicationStatus.Success)
                {
                    sb.Append(" - getAllCharacteristics found ");
                    sb.Append(result.Characteristics.Count());
                    sb.Append(" characteristics");
                    Debug.WriteLine(sb);
                    foreach (GattCharacteristic gattchar in result.Characteristics)
                    {
                        ObservableGattCharacteristics temp = new ObservableGattCharacteristics(gattchar, this);
                        await temp.Initialize();
                        Characteristics.Add(temp);
                    }
                }
                else if (result.Status == GattCommunicationStatus.Unreachable)
                {
                    sb.Append(" - getAllCharacteristics failed with Unreachable");
                    Debug.WriteLine(sb.ToString());
                }
                else if (result.Status == GattCommunicationStatus.ProtocolError)
                {
                    sb.Append(" - getAllCharacteristics failed with Unreachable");
                    Debug.WriteLine(sb.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("getAllCharacteristics: Exception - {0}" + ex.Message);
                Name += " - Exception: " + ex.Message;
            }
        }

        /// <summary>
        /// Event to notify when this object has changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Property changed
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
