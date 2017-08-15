// <copyright file="MicrosoftReadLongCharacteristic.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GattServicesLibrary.Helpers;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Power;
using Windows.Foundation;
using Windows.Storage.Streams;

namespace GattServicesLibrary.Characteristics
{
    /// <summary>
    /// Microsoft boilerplate characteristic that has a long characteristic value provided for completeness. 
    /// </summary>
    public class MicrosoftReadLongCharacteristic : GenericGattCharacteristic
    {
        #region Local variables
        /// <summary>
        /// Long characteristic payload 
        /// </summary>
        private byte[] longCharacteristicData = new byte[800];

        /// <summary>
        /// Lock around longCharacteristicData
        /// </summary>
        private object dataLock = new object();

        /// <summary>
        /// Microsoft service long characteristics offset value
        /// </summary>
        private int longCharacteristicReadOffset = 0;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MSFTReadLongCharacteristic" /> class.
        /// </summary>
        /// <param name="characteristic">Characteristic this wraps</param>
        public MicrosoftReadLongCharacteristic(GattLocalCharacteristic characteristic, GenericGattService service) : base(characteristic, service)
        {
            for (int i = 0; i < longCharacteristicData.Length; i++)
            {
                longCharacteristicData[i] = (byte)(i % 10);
            }
        }

        /// <summary>
        /// Read request callback to update the value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected override async void Characteristic_ReadRequested(GattLocalCharacteristic sender, GattReadRequestedEventArgs args)
        {
            Deferral deferral = args.GetDeferral();

            Debug.WriteLine($"MSFTLongReadCharacteristic: ReadRequested - MaxPduSize {args.Session.MaxPduSize}");
            await UpdateValue(args);

            base.Characteristic_ReadRequested(sender, args, deferral);
        }

        /// <summary>
        /// Updates the Value which is what gets send back by the <see cref="base.Characteristic_ReadRequested"/>. It verifies that the 
        /// source data is bigger than a single read request so that a ReadBlobRequest has to be done
        /// </summary>
        /// <param name="args"></param>
        private async Task UpdateValue(GattReadRequestedEventArgs args)
        {

            DataWriter writer = new DataWriter();
            int maxPayloadSize = args.Session.MaxPduSize - 1;

            // start getting the read request
            var requestTask = args.GetRequestAsync();

            // make sure our source data is bigger than a single read request to make sure a ReadBlobRequest is done
            if (longCharacteristicData.Length < maxPayloadSize)
            {
                // This should not be required as the server should only be processing one request at a time
                // but it's better to be safe than sorry
                lock (dataLock)
                {
                    longCharacteristicData = new byte[(int)(maxPayloadSize * 2.5)];

                    for (int i = 0; i < longCharacteristicData.Length; i++)
                    {
                        longCharacteristicData[i] = (byte)(i % 10);
                    }
                }
            }

            // finish getting the read request
            GattReadRequest request = await requestTask;
            int offset = (int)request.Offset;

            // calculate the size of the data we send back
            int chunk = Math.Min(maxPayloadSize, longCharacteristicData.Length - offset);
            Debug.WriteLine($"UpdateValue: payloadSize: {maxPayloadSize}, chunk {chunk}");

            // prep the data we send back
            var readValue = String.Empty;
            var buffer = new byte[chunk];
            buffer.Initialize();

            // copy from source to target
            Array.Copy(longCharacteristicData, longCharacteristicReadOffset, buffer, 0, chunk);
 
            // write to our internal Value which will be used to send back the data
            writer.WriteBytes(buffer);
            readValue = buffer.BytesToString();
            Debug.WriteLine("MicrosoftReadLongCharacteristic: Read request value: {readValue}");
            Value = writer.DetachBuffer();

        }
    }
}