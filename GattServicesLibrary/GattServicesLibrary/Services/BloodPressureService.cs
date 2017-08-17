// <copyright file="BloodPressureService.cs" company="Microsoft Corporation">
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
    /// Class for Blood pressure services
    /// </summary>
    public class BloodPressureService : GenericGattService
    {
        /// <summary>
        /// Name of the service
        /// </summary>
        public override string Name
        {
            get
            {
                return "Blood Pressure Service";
            }
        }

        /// <summary>
        /// The Blood Pressure measurement characteristic is used to send a Blood Pressure measurement.
        /// </summary>
        private GenericGattCharacteristic bloodPressureMeasurement;

        /// <summary>
        /// Gets or Sets the blood pressure measurement characteristic
        /// </summary>
        public GenericGattCharacteristic BloodPressureMeasurement
        {
            get
            {
                return bloodPressureMeasurement;
            }

            set
            {
                if (bloodPressureMeasurement != value)
                {
                    bloodPressureMeasurement = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("BloodPressureMeasurement"));
                }
            }
        }

        /// <summary>
        /// The Blood Pressure Feature characteristic is used to describe the supported features of the Blood Pressure Sensor.
        /// </summary>
        private GenericGattCharacteristic bloodPressureFeature;

        /// <summary>
        /// Gets or Sets the blood pressure feature characteristic
        /// </summary>
        public GenericGattCharacteristic BloodPressureFeature
        {
            get
            {
                return bloodPressureFeature;
            }

            set
            {
                if (bloodPressureFeature != value)
                {
                    bloodPressureFeature = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("BloodPressureFeature"));
                }
            }
        }

        /// <summary>
        /// Asynchronous initialization
        /// </summary>
        /// <returns>Initialization Task</returns>
        public override async Task Init()
        {
            await CreateServiceProvider(GattServiceUuids.BloodPressure);

            // Preparing the Blood pressure characteristics
            var bloodPressureCharacteristics = PlainIndicateParameters;
            bloodPressureCharacteristics.UserDescription = "Blood Pressure in mm Hg";
            bloodPressureCharacteristics.PresentationFormats.Add(
                GattPresentationFormat.FromParts(
                    Convert.ToByte(PresentationFormats.FormatTypes.OpaqueStructure),
                    PresentationFormats.Exponent,
                    Convert.ToUInt16(PresentationFormats.Units.PressureMilliMetreofmercury),
                    Convert.ToByte(PresentationFormats.NamespaceId.BluetoothSigAssignedNumber),
                    PresentationFormats.Description));

            // Create the blood pressure measurement characteristic for the service
            GattLocalCharacteristicResult result = 
                await ServiceProvider.Service.CreateCharacteristicAsync(
                    GattCharacteristicUuids.BloodPressureMeasurement,
                    bloodPressureCharacteristics);

            // Grab the characterist object from the service
            GattLocalCharacteristic baseBloodPressureMeasurement = null;
            GattServicesHelper.GetCharacteristicsFromResult(result, ref baseBloodPressureMeasurement);

            if (baseBloodPressureMeasurement != null)
            {
                BloodPressureMeasurement = new Characteristics.BloodPressureMeasurementCharacteristic(baseBloodPressureMeasurement, this);
            }

            result = null;

            // Preparing the Blood pressure feature characteristics
            var bloodPressureFeatureCharacteristics = PlainReadParameter;
            bloodPressureFeatureCharacteristics.UserDescription = "The Blood Pressure Feature characteristic is used to describe the supported features of the Blood Pressure Sensor.";
            bloodPressureFeatureCharacteristics.PresentationFormats.Add(
                GattPresentationFormat.FromParts(
                    Convert.ToByte(PresentationFormats.FormatTypes.Unsigned16BitInteger),
                    PresentationFormats.Exponent,
                    Convert.ToUInt16(PresentationFormats.Units.Unitless),
                    Convert.ToByte(PresentationFormats.NamespaceId.BluetoothSigAssignedNumber),
                    PresentationFormats.Description));

            // Create the blood pressure measurement characteristic for the service
            result = await ServiceProvider.Service.CreateCharacteristicAsync(
                GattCharacteristicUuids.BloodPressureFeature,
                bloodPressureFeatureCharacteristics);

            // Grab the characterist object from the service
            GattLocalCharacteristic baseBloodPressureFeature = null;
            GattServicesHelper.GetCharacteristicsFromResult(result, ref baseBloodPressureFeature);

            if (baseBloodPressureFeature != null)
            {
                BloodPressureFeature = new Characteristics.BloodPressureFeatureCharacteristic(baseBloodPressureFeature, this);
            }
        }
    }
}