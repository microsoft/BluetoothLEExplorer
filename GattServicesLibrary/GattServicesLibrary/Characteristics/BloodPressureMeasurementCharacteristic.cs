// <copyright file="BloodPressureMeasurementCharacteristic.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GattServicesLibrary.Helpers;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Power;
using Windows.Storage.Streams;
using System.Threading;
using GattHelper.Converters;

namespace GattServicesLibrary.Characteristics
{
    /// <summary>
    /// Microsoft boilerplate characteristic that supports 'Indicate' provided for completeness. This service is almost identical to MicrosoftNotifyCharacteristic.
    /// </summary>
    public class BloodPressureMeasurementCharacteristic : GenericGattCharacteristic
    {
        private Timer bloodPressureTicker = null;
        private Int16 Systolic = 120;
        private Int16 Diastolic = 80;
        private Random rand = new Random();

        /// <summary>
        /// Initializes a new instance of the <see cref="BloodPressureMeasurementCharacteristic" /> class.
        /// </summary>
        /// <param name="characteristic">Characteristic this wraps</param>
        public BloodPressureMeasurementCharacteristic(GattLocalCharacteristic characteristic, GenericGattService service) : base(characteristic, service)
        {
            bloodPressureTicker = new Timer(updateBloodPressure, "", TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }

        private void updateBloodPressure(Object state)
        {
            // Create random blood pressure between 100-160 over 60-100
            Systolic = (Int16)rand.Next(100, 160);
            Diastolic = (Int16)rand.Next(60, 100);

            UInt16 MAP = (UInt16)(((2 * Diastolic) + Systolic) / 3);

            // https://www.bluetooth.com/specifications/gatt/viewer?attributeXmlFile=org.bluetooth.characteristic.blood_pressure_measurement.xml
            byte[] flags = { 0 };
            byte[] value = flags.Concat(
                BitConverter.GetBytes(Systolic)).Concat(
                BitConverter.GetBytes(Diastolic)).Concat(
                BitConverter.GetBytes(MAP)).ToArray();

            Value = GattConvert.ToIBuffer(value);
            NotifyValue();
        }

        /// <summary>
        /// Override so we can update the value before notifying or indicating the client
        /// </summary>
        public override void NotifyValue()
        {
            base.NotifyValue();
        }
    }
}
