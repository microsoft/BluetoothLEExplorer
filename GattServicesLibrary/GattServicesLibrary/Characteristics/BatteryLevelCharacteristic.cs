// <copyright file="BatteryLevelCharacteristic.cs" company="Microsoft Corporation">
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
    /// Implementation of the battery profile
    /// </summary>
    public class BatteryLevelCharacteristic : GenericGattCharacteristic
    {
        /// <summary>
        /// Access to the battery of this system
        /// </summary>
        private Battery aggBattery = Battery.AggregateBattery;

        /// <summary>
        /// Initializes a new instance of the <see cref="BatteryLevelCharacteristic" /> class.
        /// </summary>
        /// <param name="characteristic">The characteristic that this wraps</param>
        public BatteryLevelCharacteristic(GattLocalCharacteristic characteristic, GenericGattService service) : base(characteristic, service)
        {
            aggBattery.ReportUpdated += AggBattery_ReportUpdated;
            UpdateBatteryValue();
        }

        /// <summary>
        /// Callback when the battery level changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void AggBattery_ReportUpdated(Battery sender, object args)
        {
            UpdateBatteryValue();
            NotifyValue();
        }

        /// <summary>
        /// Method that updates <see cref="GenericGattCharacteristic.Value"/> with the current battery level 
        /// </summary>
        private void UpdateBatteryValue()
        {
            // Get report
            BatteryReport report = aggBattery.GetReport();
            float fullCharge = Convert.ToSingle(report.FullChargeCapacityInMilliwattHours);
            float currentCharge = Convert.ToSingle(report.RemainingCapacityInMilliwattHours);

            float val = (fullCharge > 0) ? (currentCharge / fullCharge) * 100.0f : 0.0f;

            Value = GattHelper.Converters.GattConvert.ToIBuffer((byte)Math.Round(val, 0));
        }
    }
}
