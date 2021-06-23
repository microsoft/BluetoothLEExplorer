// <copyright file="GenericGattService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using GattServicesLibrary.Helpers;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Foundation.Metadata;
using Windows.Storage.Streams;

namespace GattServicesLibrary
{
    /// <summary>
    /// An abstract class defines the Generic parameters, properties, APIs common to GATT Services.
    /// </summary>
    public abstract class GenericGattService : INotifyPropertyChanged
    {
        #region Generic helper parameters
        /// <summary>
        /// Gatt Local characteristics parameter for Reading Parameters
        /// </summary>
        protected static readonly GattLocalCharacteristicParameters PlainReadParameter = new GattLocalCharacteristicParameters
        {
            CharacteristicProperties = GattCharacteristicProperties.Read,
            WriteProtectionLevel = GattProtectionLevel.Plain,
            ReadProtectionLevel = GattProtectionLevel.Plain
        };

        /// <summary>
        /// Gatt Local characteristics parameter for Writing Parameters
        /// </summary>
        protected static readonly GattLocalCharacteristicParameters PlainWriteOrWriteWithoutRespondsParameter = new GattLocalCharacteristicParameters
        {
            CharacteristicProperties = GattCharacteristicProperties.Write | GattCharacteristicProperties.WriteWithoutResponse,
            WriteProtectionLevel = GattProtectionLevel.Plain
        };

        /// <summary>
        /// Gatt Local characteristics parameter for Reading and Notifying Parameters
        /// </summary>
        protected static readonly GattLocalCharacteristicParameters PlainReadNotifyParameters = new GattLocalCharacteristicParameters
        {
            CharacteristicProperties = GattCharacteristicProperties.Read | GattCharacteristicProperties.Notify,
            ReadProtectionLevel = GattProtectionLevel.Plain,
            WriteProtectionLevel = GattProtectionLevel.Plain
        };

        /// <summary>
        /// Gatt Local characteristics parameter for Notifying Parameters
        /// </summary>
        protected static readonly GattLocalCharacteristicParameters PlainNotifyParameters = new GattLocalCharacteristicParameters
        {
            CharacteristicProperties = GattCharacteristicProperties.Notify,
            WriteProtectionLevel = GattProtectionLevel.Plain
        };

        /// <summary>
        /// Gatt Local characteristics parameter for Indicating Parameters
        /// </summary>
        protected static readonly GattLocalCharacteristicParameters PlainIndicateParameters = new GattLocalCharacteristicParameters
        {
            CharacteristicProperties = GattCharacteristicProperties.Indicate,
            WriteProtectionLevel = GattProtectionLevel.Plain
        };
        #endregion

        #region INotifyPropertyChanged requirements
        /// <summary>
        /// Property changed event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Property changed method
        /// </summary>
        /// <param name="e">Property that changed</param>
        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }
        #endregion

        /// <summary>
        /// Gets the name of the characteristic
        /// </summary>
        public abstract string Name
        {
            get;
        }

        private bool isPublishing = false;

        public bool IsPublishing
        {
            get
            {
                return isPublishing;
            }

            private set
            {
                if (value != isPublishing)
                {
                    isPublishing = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsPublishing"));
                }
            }
        }

        private bool isAdvertising = false;

        public bool IsAdvertising
        {
            get
            {
                return isAdvertising;
            }

            private set
            {
                if(value != isAdvertising)
                {
                    isAdvertising = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsAdvertising"));
                }
            }
        }


        public bool IsConnectable
        {
            get
            {
                return ad.IsConnectable;
            }

            set
            {
                if (value != ad.IsConnectable)
                {
                    ad.IsConnectable = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsConnectable"));
                }
            }
        }

        public bool IsDiscoverable
        {
            get
            {
                return ad.IsDiscoverable;
            }

            set
            {
                if (value != ad.IsDiscoverable)
                {
                    ad.IsDiscoverable = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IsDiscoverable"));
                }
            }
        }

        private bool includeServiceData;

        public bool IncludeServiceData
        {
            get
            {
                return includeServiceData;
            }

            set
            {
                if (value != includeServiceData)
                {
                    includeServiceData = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("IncludeServiceData"));
                }
            }
        }

        private IBuffer serviceData = null;

        public IBuffer ServiceData
        {
            get
            {
                return serviceData;
            }

            set
            {
                serviceData = value;
            }
        }

        /// <summary>
        /// Internal ServiceProvider
        /// </summary>
        private GattServiceProvider serviceProvider;

        private GattServiceProviderAdvertisingParameters ad = new GattServiceProviderAdvertisingParameters();
        
        /// <summary>
        /// Gets or sets the Gatt Service Provider
        /// </summary>
        public GattServiceProvider ServiceProvider
        {
            get
            {
                return serviceProvider;
            }

            protected set
            {
                if (serviceProvider != value)
                {
                    serviceProvider = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("ServiceProvider"));
                    serviceProvider.AdvertisementStatusChanged += ServiceProvider_AdvertisementStatusChanged;
                }
            }
        }

        private void ServiceProvider_AdvertisementStatusChanged(GattServiceProvider sender, GattServiceProviderAdvertisementStatusChangedEventArgs args)
        {
            if ((args.Status == GattServiceProviderAdvertisementStatus.Stopped) ||
                (args.Status == GattServiceProviderAdvertisementStatus.Aborted))
            {
                IsAdvertising = false;
            }
            else
            {
                IsAdvertising = true;
            }
        }

        /// <summary>
        /// Gets the Constant Read parameters using the Input value
        /// </summary>
        /// <param name="value">specific characteristic value for given profile <see cref="GattServicesLibrary.CharacteristicParameterValues"/></param>
        protected GattLocalCharacteristicParameters GetPlainReadParameterWithValue(byte[] value)
        {
            return new GattLocalCharacteristicParameters
            {
                CharacteristicProperties = GattCharacteristicProperties.Read,
                StaticValue = value.AsBuffer(),
                ReadProtectionLevel = GattProtectionLevel.Plain,
            };
        }

        /// <summary>
        /// Abstract method used to initialize this class
        /// </summary>
        /// <returns>Tasks that initializes the class</returns>
        public abstract Task Init();

        /// <summary>
        /// Starts the Gatt Service
        /// </summary>
        public virtual void Start()
        {
            try
            {
                if (IncludeServiceData)
                {
                    if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 10))
                    {
                        ad.ServiceData = serviceData;
                    }
                    else
                    {
                        IncludeServiceData = false;
                    }
                }

                ServiceProvider.StartAdvertising(ad);
                IsPublishing = true;
                OnPropertyChanged(new PropertyChangedEventArgs("IsPublishing"));
            }
            catch (Exception)
            {
                Debug.WriteLine($"Exception while start Advertising {ServiceProvider.Service.Uuid}");
                IsPublishing = false;
                throw;
            }
        }

        /// <summary>
        /// Stops the already running Service
        /// </summary>
        public virtual void Stop()
        {
            try
            {
                ServiceProvider.StopAdvertising();
                IsPublishing = false;
                OnPropertyChanged(new PropertyChangedEventArgs("IsPublishing"));
            }
            catch (Exception)
            {
                Debug.WriteLine($"Exception while Stop Advertising {ServiceProvider.Service.Uuid}");
                IsPublishing = true;
                throw;
            }
        }

        /// <summary>
        /// Creates the Gatt Service provider
        /// </summary>
        /// <param name="uuid">UUID of the Service to create</param>
        protected async Task CreateServiceProvider(Guid uuid)
        {
            // Create Service Provider - similar to RFCOMM APIs
            GattServiceProviderResult result = await GattServiceProvider.CreateAsync(uuid);

            if (result.Error != BluetoothError.Success)
            {
                throw new CreateServiceException(result);
            }

            ServiceProvider = result.ServiceProvider;
        }
    }
}