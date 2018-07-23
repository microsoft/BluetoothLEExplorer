// <copyright file="CurrentTimeService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using System.Threading.Tasks;
using GattServicesLibrary.Helpers;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using System.ComponentModel;

namespace GattServicesLibrary.Services
{
    /// <summary>
    /// Class for Current time service
    /// </summary>
    public class CurrentTimeService : GenericGattService
    {
        /// <summary>
        /// Name of the service
        /// </summary>
        public override string Name
        {
            get
            {
                return "Current Time Service";
            }
        }

        /// <summary>
        /// Current time characteristic
        /// </summary>
        private GenericGattCharacteristic currentTime;

        /// <summary>
        /// Gets or sets the currentTime
        /// </summary>
        public GenericGattCharacteristic CurrentTime
        {
            get
            {
                return currentTime;
            }

            set
            {
                if (currentTime != value)
                {
                    currentTime = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("CurrentTime"));
                }
            }
        }

        /// <summary>
        /// Asynchronous initialization
        /// </summary>
        /// <returns>Initialization Task</returns>
        public override async Task Init()
        {
            await CreateServiceProvider(GattServiceUuids.CurrentTime);

            GattLocalCharacteristicResult result = await ServiceProvider.Service.CreateCharacteristicAsync(
                GattCharacteristicUuids.CurrentTime,
                PlainReadNotifyParameters);

            GattLocalCharacteristic currentTimeCharacterisitic = null;
            GattServicesHelper.GetCharacteristicsFromResult(result, ref currentTimeCharacterisitic);
            if (currentTimeCharacterisitic != null)
            {
                CurrentTime = new Characteristics.CurrentTimeCharacteristic(currentTimeCharacterisitic, this);
            }
        }
    }
}
