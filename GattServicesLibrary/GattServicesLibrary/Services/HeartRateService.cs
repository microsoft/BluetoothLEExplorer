// <copyright file="HeartRateService.cs" company="Microsoft Corporation">
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
    /// Class for Heart rate service
    /// </summary>
    public class HeartRateService : GenericGattService
    {
        /// <summary>
        /// Name of the service
        /// </summary>
        public override string Name
        {
            get
            {
                return "Heart Rate Service";
            }
        }
        
        /// <summary>
        /// This characteristic is used to send a heart rate measurement.
        /// </summary>
        private GenericGattCharacteristic heartRateMeasurement;

        /// <summary>
        /// Gets or Sets the heart rate characteristic
        /// </summary>
        public GenericGattCharacteristic HeartRateMeasurement
        {
            get
            {
                return heartRateMeasurement;
            }

            set
            {
                if (heartRateMeasurement != value)
                {
                    heartRateMeasurement = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("HeartRateMeasurement"));
                }
            }
        }

        /// <summary>
        /// Starts the Heart rate service
        /// </summary>
        public override async Task Init()
        {
            await CreateServiceProvider(GattServiceUuids.HeartRate);

            // Preparing the Blood pressure characteristics
            var heartRateCharacteristics = PlainNotifyParameters;
            heartRateCharacteristics.UserDescription = "Heart Rates in Beats per Minute";
            heartRateCharacteristics.PresentationFormats.Add(
                GattPresentationFormat.FromParts(
                    Convert.ToByte(PresentationFormats.FormatTypes.Unsigned16BitInteger),
                    PresentationFormats.Exponent,
                    Convert.ToUInt16(PresentationFormats.Units.PeriodBeatsPerMinute),
                    Convert.ToByte(PresentationFormats.NamespaceId.BluetoothSigAssignedNumber),
                    PresentationFormats.Description));

            // Create the heart rate characteristic for the service
            GattLocalCharacteristicResult result = 
                await ServiceProvider.Service.CreateCharacteristicAsync(
                    GattCharacteristicUuids.HeartRateMeasurement,
                    PlainNotifyParameters);

            // Grab the characterist object from the service set it to the HeartRate property which is of a specfic Characteristic type
            GattLocalCharacteristic baseHeartRateMeasurement = null;
            GattServicesHelper.GetCharacteristicsFromResult(result, ref baseHeartRateMeasurement);

            if (baseHeartRateMeasurement != null)
            {
                HeartRateMeasurement = new Characteristics.HeartRateMeasurementCharacteristic(baseHeartRateMeasurement, this);
            }
        }
    }
}
