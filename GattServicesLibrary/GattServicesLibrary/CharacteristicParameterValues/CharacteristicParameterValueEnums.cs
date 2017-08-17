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
    /// Bluetooth sig: https://www.bluetooth.com/specifications/gatt/viewer?attributeXmlFile=org.bluetooth.characteristic.alert_category_id_bit_mask.xml 
    /// </summary>
    [Flags]
    public enum AlertCategoryID : byte
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
