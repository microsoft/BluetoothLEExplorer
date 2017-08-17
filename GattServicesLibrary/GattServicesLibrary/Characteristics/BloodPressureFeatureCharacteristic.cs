// <copyright file="BloodPressureFeatureCharacteristic.cs" company="Microsoft Corporation">
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
using GattHelper.Converters;

namespace GattServicesLibrary.Characteristics
{
    /// <summary>
    /// Microsoft boilerplate characteristic that supports 'Indicate' provided for completeness. This service is almost identical to MicrosoftNotifyCharacteristic.
    /// </summary>
    public class BloodPressureFeatureCharacteristic : GenericGattCharacteristic
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BloodPressureMeasurementCharacteristic" /> class.
        /// </summary>
        /// <param name="characteristic">Characteristic this wraps</param>
        public BloodPressureFeatureCharacteristic(GattLocalCharacteristic characteristic, GenericGattService service) : base(characteristic, service)
        {
            // Supports no extra features - required per spec
            // https://www.bluetooth.com/specifications/gatt/viewer?attributeXmlFile=org.bluetooth.characteristic.blood_pressure_feature.xml
            Value = GattConvert.ToIBuffer((Int16)0);
        }
    }
}
