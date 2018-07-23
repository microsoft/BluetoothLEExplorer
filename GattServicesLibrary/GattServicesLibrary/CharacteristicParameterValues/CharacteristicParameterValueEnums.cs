// <copyright file="CharacteristicParameterValueEnums.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[module: System.Diagnostics.CodeAnalysis.SuppressMessage(
    "StyleCop.CSharp.DocumentationRules",
    "SA1649:FileHeaderFileNameDocumentationMustMatchTypeName",
    Justification = "This is a enum only file")]

namespace GattServicesLibrary.CharacteristicParameterValues
{
    /// <summary>
    /// Bluetooth sig: https://www.bluetooth.com/specifications/gatt/viewer?attributeXmlFile=org.bluetooth.characteristic.alert_notification_control_point.xml
    /// </summary>
    public enum AlertNotificationControlPointCommandId : byte
    {
        EnableNewIncomgAlertNotification = 0,
        EnableUnreadCategoryStatusNotification = 1,
        DisableNewIncomingAlertNotification = 2,
        DisableUnreadCategoryStatusNotification = 3,
        NotifyNewIncomgAlertImmediately = 4,
        NotifyUnreadCategoryStatusImmedately = 5,
    }

    public enum AlertCategoryId : byte
    {
        SimpleAlert = 0,
        Email = 1,
        News = 2,
        Call = 3,
        MissedCall = 4,
        SMS_MMS = 5,
        VoiceMail = 6,
        Schedule = 7,
        HighPrioritizedAlert = 8,
        InstantMessage = 9,
        All = 0xFF,
    }

    /// <summary>
    /// Bluetooth sig: https://www.bluetooth.com/specifications/gatt/viewer?attributeXmlFile=org.bluetooth.characteristic.alert_category_id_bit_mask.xml 
    /// </summary>
    [Flags]
    public enum AlertCategoryIdBitMask_0 : byte
    {
        None = 0,           // 0b00000000
        SimpleAlert = 1,    // 0b00000001
        Email = 2,          // 0b00000010
        News = 4,           // 0b00000100
        Call = 8,           // 0b00001000
        MissedCall = 16,    // 0b00010000
        SMS_MMS = 32,       // 0b00100000
        VoiceMail = 64,     // 0b01000000
        Schedule = 128,     // 0b10000000
    }

    [Flags]
    public enum AlertCategoryIdBitMask_1 : byte
    {
        None = 0,
        HighPrioritizedAlert = 1,
        InstantMessage = 2,
    }

    public class AlertCategoryIdBitMask
    {
        public byte[] Value { get; private set; }

        public AlertCategoryIdBitMask_0 Mask0
        {
            get
            {
                return (AlertCategoryIdBitMask_0)Value[0];
            }
        }

        public AlertCategoryIdBitMask_1 Mask1
        {
            get
            {
                if (Value.Length > 1)
                {
                    return (AlertCategoryIdBitMask_1)Value[1];
                }
                return AlertCategoryIdBitMask_1.None;
            }
        }

        public AlertCategoryIdBitMask(
            AlertCategoryIdBitMask_0 lsb = AlertCategoryIdBitMask_0.None,
            AlertCategoryIdBitMask_1 msb = AlertCategoryIdBitMask_1.None)
        {
            if (msb != AlertCategoryIdBitMask_1.None)
            {
                Value = new byte[] { (byte)lsb, (byte)msb };
            }
            else
            {
                Value = new byte[] { (byte)lsb };
            }
        }
    }

    /// <summary>
    /// Bluetooth sig: https://www.bluetooth.com/specifications/gatt/viewer?attributeXmlFile=org.bluetooth.characteristic.blood_pressure_feature.xml
    /// </summary>
    public enum BloodPressureFeatures : byte
    {
        None = 0,                           // 0b00000000
        BodyMovementDetection = 1,          // 0b00000001
        CuffFitDetection = 2,               // 0b00000010
        IrregularPulseDetection = 4,        // 0b00000100
        PulseRateRangeDetection = 8,        // 0b00001000
        MeasurementPositionDetection = 16,  // 0b00010000
        MultipleBondSupport = 32            // 0b00010000
    }
}
