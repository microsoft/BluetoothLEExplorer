// <copyright file="HeartRateMeasurementCharacteristic.cs" company="Microsoft Corporation">
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
    /// Microsoft boilerplate characteristic that supports 'Notify' provided for completeness. This service is almost identical to MicrosoftIndicateCharacteristic.
    /// </summary>
    public class HeartRateMeasurementCharacteristic : GenericGattCharacteristic
    {
        private Timer heartRateTicker = null;
        private Int16 currentHeartRate = 70;

        /// <summary>
        /// Initializes a new instance of the <see cref="HeartRateMeasurementCharacteristic" /> class.
        /// </summary>
        /// <param name="characteristic">Characteristic this wraps</param>
        public HeartRateMeasurementCharacteristic(GattLocalCharacteristic characteristic, GenericGattService service) : base(characteristic, service)
        {
            heartRateTicker = new Timer(UpdateHeartRate, "", TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }

        private void UpdateHeartRate(Object state)
        {
            if (currentHeartRate == 110)
            {
                currentHeartRate = 70;
            }
            else
            {
                currentHeartRate++;
            }

            SetHeartRate();
        }

        private void SetHeartRate()
        {
            // Heart rate service starts with flags, then the value. I combine them here then set the characterstic value
            byte[] flags = { 0x07 };
            byte[] heartRate = BitConverter.GetBytes(currentHeartRate);

            byte[] value = flags.Concat(heartRate).ToArray();

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
