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

        /// <summary>
        /// Lock around the <see cref="Service"/>.
        /// </summary>
        SemaphoreSlim serviceLock = new SemaphoreSlim(1, 1);

        public void Dispose()
        {
            var temp = service;
            try
            {
                serviceLock.Wait();
                Service = null;
            }
            finally
            {
                serviceLock.Release();
            }
            temp.Dispose();
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
            try
            {
                serviceLock.Wait();
                Service = service;
            }
            finally
            {
                serviceLock.Release();
            }
            Name = GattServiceUuidHelper.ConvertUuidToName(service.Uuid);
            UUID = service.Uuid.ToString();
        }

        public async Task Initialize()
        {
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
        /// Gets all the characteristics of this service
        /// </summary>
        private async Task GetAllCharacteristics()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("ObservableGattDeviceService::getAllCharacteristics: ");
            sb.Append(name);

            try
            {
                await serviceLock.WaitAsync();

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

                GattCharacteristicsResult result = await service.GetCharacteristicsAsync(Services.SettingsServices.SettingsService.Instance.UseCaching ? BluetoothCacheMode.Cached : BluetoothCacheMode.Uncached);

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
            finally
            {
                serviceLock.Release();
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
