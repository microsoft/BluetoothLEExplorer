// <copyright file="MicrosoftReadCharacteristic.cs" company="Microsoft Corporation">
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
    /// Microsoft boilerplate characteristic that supports 'Read' provided for completeness.
    /// </summary>
    public class MicrosoftReadCharacteristic : GenericGattCharacteristic
    {
        /// <summary>
        /// Random number generator used when updating the value
        /// </summary>
        private Random rand = new Random();

        /// <summary>
        /// Initializes a new instance of the <see cref="MSFTReadCharacteristic" /> class.
        /// </summary>
        /// <param name="characteristic">Characteristic this wraps</param>
        public MicrosoftReadCharacteristic(GattLocalCharacteristic characteristic, GenericGattService service) : base(characteristic, service)
        {
            UpdateValue();
        }

        /// <summary>
        /// Read request callback to update the value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected override bool ReadRequested(GattSession session, GattReadRequest request)
        {
            System.Diagnostics.Debug.WriteLine("Entering MSFTReadRequest.Characteristic_ReadRequested");
            UpdateValue();
            return false;
        }

        /// <summary>
        /// Override so we can update the value when the value is read
        /// </summary>
        private void UpdateValue()
        {
            int readValue = rand.Next(1, 20);
            Value = GattServicesHelper.ConvertValueToBuffer(Convert.ToByte(readValue));
        }
    }
}
