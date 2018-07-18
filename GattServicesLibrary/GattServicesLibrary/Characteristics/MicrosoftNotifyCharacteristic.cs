// <copyright file="MicrosoftNotifyCharacteristic.cs" company="Microsoft Corporation">
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

namespace GattServicesLibrary.Characteristics
{
    /// <summary>
    /// Microsoft boilerplate characteristic that supports 'Notify' provided for completeness. This service is almost identical to MicrosoftIndicateCharacteristic.
    /// </summary>
    public class MicrosoftNotifyCharacteristic : GenericGattCharacteristic
    {
        /// <summary>
        /// Random number generator used when updating the value
        /// </summary>
        private Random rand = new Random();

        /// <summary>
        /// Initializes a new instance of the <see cref="MSFTNotifyCharacteristic" /> class.
        /// </summary>
        /// <param name="characteristic">Characteristic this wraps</param>
        public MicrosoftNotifyCharacteristic(GattLocalCharacteristic characteristic, GenericGattService service) : base(characteristic, service)
        {
            DataWriter writer = new DataWriter();
            writer.WriteString("Hello World!");
            Value = writer.DetachBuffer();
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
