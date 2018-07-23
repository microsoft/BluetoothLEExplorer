// <copyright file="CurrentTimeCharacteristic.cs" company="Microsoft Corporation">
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
using Windows.System.Threading;

namespace GattServicesLibrary.Characteristics
{
    /// <summary>
    /// Implementation of the battery profile
    /// </summary>
    public class CurrentTimeCharacteristic : GenericGattCharacteristic
    {
        private ThreadPoolTimer m_timeUpdate = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentTimeCharacteristic" /> class.
        /// </summary>
        /// <param name="characteristic">The characteristic that this wraps</param>
        public CurrentTimeCharacteristic(GattLocalCharacteristic characteristic, GenericGattService service) : base(characteristic, service)
        {
            UpdateCurrentTimeValue();
        }

        public override void NotifyValue()
        {
            UpdateCurrentTimeValue();
            base.NotifyValue();
        }

        protected override void Characteristic_SubscribedClientsChanged(GattLocalCharacteristic sender, object args)
        {
            lock (this)
            {
                if (sender.SubscribedClients.Count == 0)
                {
                    if (m_timeUpdate != null)
                    {
                        m_timeUpdate.Cancel();
                        m_timeUpdate = null;
                    }
                }
                else if (m_timeUpdate == null)
                {
                    m_timeUpdate = ThreadPoolTimer.CreatePeriodicTimer(
                        (source) =>
                        {
                            UpdateCurrentTimeValue();
                            NotifyValue();
                        },
                        TimeSpan.FromMinutes(15));
                }
            }

            base.Characteristic_SubscribedClientsChanged(sender, args);
        }

        /// <summary>
        /// Method that updates <see cref="GenericGattCharacteristic.Value"/> with the current battery level 
        /// </summary>
        private void UpdateCurrentTimeValue()
        {
            var writer = new DataWriter();
            // Date time according to: https://www.bluetooth.com/specifications/gatt/viewer?attributeXmlFile=org.bluetooth.characteristic.date_time.xml
            DateTime exactDateTime = DateTime.Now;
            writer.WriteUInt16((ushort)exactDateTime.Year);
            writer.WriteByte((byte)exactDateTime.Month);
            writer.WriteByte((byte)exactDateTime.Day);
            writer.WriteByte((byte)exactDateTime.Hour);
            writer.WriteByte((byte)exactDateTime.Minute);
            writer.WriteByte((byte)exactDateTime.Second);
            // Day of week according to: https://www.bluetooth.com/specifications/gatt/viewer?attributeXmlFile=org.bluetooth.characteristic.day_of_week.xml
            // Going to leave this "not known" for now - would have to perform a rotate of DayOfWeek property
            writer.WriteByte(0x0);

            Value = writer.DetachBuffer();
        }
    }
}
