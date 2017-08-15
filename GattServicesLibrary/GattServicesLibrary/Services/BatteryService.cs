// <copyright file="BatteryService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using GattServicesLibrary.Helpers;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Power;

namespace GattServicesLibrary.Services
{
    /// <summary>
    /// Class for Battery Services
    /// </summary>
    public class BatteryService : GenericGattService
    {
        /// <summary>
        /// Name of the service
        /// </summary>
        public override string Name
        {
            get
            {
                return "Battery Service";
            }
        }

        /// <summary>
        /// Battery level
        /// </summary>
        private GenericGattCharacteristic batteryLevel;

        /// <summary>
        /// Gets or sets the battery level 
        /// </summary>
        public GenericGattCharacteristic BatteryLevel
        {
            get
            {
                return batteryLevel;
            }

            set
            {
                if (batteryLevel != value)
                {
                    batteryLevel = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("BatteryLevel"));
                }
            }
        }

        /// <summary>
        /// Asynchronous initialization
        /// </summary>
        /// <returns>Initialization Task</returns>
        public override async Task Init()
        {
            await CreateServiceProvider(GattServiceUuids.Battery);

            // Preparing the Battery Level characteristics
            GattLocalCharacteristicParameters batteryCharacteristicsParameters = PlainReadNotifyParameters;

            // Set the user descriptions
            batteryCharacteristicsParameters.UserDescription = "Battery Level percentage remaining";

            // Add presentation format - 16-bit integer, with exponent 0, the unit is percentage, defined per Bluetooth SIG with Microsoft as descriptor
            batteryCharacteristicsParameters.PresentationFormats.Add(
                GattPresentationFormat.FromParts(
                    Convert.ToByte(PresentationFormats.FormatTypes.Unsigned8BitInteger),
                    PresentationFormats.Exponent,
                    Convert.ToUInt16(PresentationFormats.Units.Percentage),
                    Convert.ToByte(PresentationFormats.NamespaceId.BluetoothSigAssignedNumber),
                    PresentationFormats.Description));

            // Create the characteristic for the service
            GattLocalCharacteristicResult result = 
                await ServiceProvider.Service.CreateCharacteristicAsync(
                    GattCharacteristicUuids.BatteryLevel, 
                    batteryCharacteristicsParameters);

            // Grab the characterist object from the service set it to the BatteryLevel property which is of a specfic Characteristic type
            GattLocalCharacteristic baseBatteryLevel = null;
            GattServicesHelper.GetCharacteristicsFromResult(result, ref baseBatteryLevel);

            if (baseBatteryLevel != null)
            {
                BatteryLevel = new Characteristics.BatteryLevelCharacteristic(baseBatteryLevel, this);
            }
        }
    }
}