// <copyright file="ValueConverter.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using GattHelper.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace GattServicesLibrary.Helpers
{
    /// <summary>
    /// Helper class to change the values so they're easily consumable
    /// </summary>
    public class ValueConverter
    {
        /// <summary>
        /// Converts GenericGattCharacteristic.Value to a string based on the presentation format
        /// </summary>
        /// <param name="characteristic"></param>
        /// <returns>value as a string</returns>
        public static string ConvertGattCharacteristicValueToString(GenericGattCharacteristic characteristic)
        {
            if (characteristic.Value == null)
            {
                return String.Empty;
            }

            GattPresentationFormat format = null;

            if (characteristic.Characteristic.PresentationFormats.Count > 0)
            {
                format = characteristic.Characteristic.PresentationFormats[0];
            }

            return ConvertValueBufferToString(characteristic.Value, format);
        }

        /// <summary>
        /// Converts GenericGattCharacteristic.Value to a string based on the presentation format
        /// </summary>
        /// <param name="value">value to convert</param>
        /// <param name="format">presentation format to use</param>
        /// <returns>value as string</returns>
        public static string ConvertValueBufferToString(IBuffer value, GattPresentationFormat format = null)
        {
            // no format, return bytes
            if (format == null)
            {
                return GattConvert.ToHexString(value);
            }

            // Bool
            if (format.FormatType == GattPresentationFormatTypes.Boolean)
            {
                // Previous implementation was incorrect. Need to implement in GattHelper.
                throw new NotImplementedException();
            }
            else if (format.FormatType == GattPresentationFormatTypes.Bit2 ||
                     format.FormatType == GattPresentationFormatTypes.Nibble)
            {
                // 2bit or nibble - no exponent
                // Previous implementation was incorrect. Need to implement in GattHelper.
                return GattConvert.ToHexString(value);
            }
            else if (format.FormatType == GattPresentationFormatTypes.UInt8 ||
                     format.FormatType == GattPresentationFormatTypes.UInt12 ||
                     format.FormatType == GattPresentationFormatTypes.UInt16)
            {
                // Previous implementation was incorrect. Need to implement in GattHelper.
                return GattConvert.ToHexString(value);
            }
            else if (format.FormatType == GattPresentationFormatTypes.UInt24 ||
                     format.FormatType == GattPresentationFormatTypes.UInt32)
            {
                // Previous implementation was incorrect. Need to implement in GattHelper.
                return GattConvert.ToHexString(value);
            }
            else if (format.FormatType == GattPresentationFormatTypes.UInt48 ||
                     format.FormatType == GattPresentationFormatTypes.UInt64)
            {
                // Previous implementation was incorrect. Need to implement in GattHelper.
                return GattConvert.ToHexString(value);
            }
            else if (format.FormatType == GattPresentationFormatTypes.SInt8 ||
                     format.FormatType == GattPresentationFormatTypes.SInt12 ||
                     format.FormatType == GattPresentationFormatTypes.SInt16)
            {
                // Previous implementation was incorrect. Need to implement in GattHelper.
                return GattConvert.ToHexString(value);
            }
            else if (format.FormatType == GattPresentationFormatTypes.SInt24 ||
                    format.FormatType == GattPresentationFormatTypes.SInt32)
            {
                return GattConvert.ToInt32(value).ToString();
            }
            else if (format.FormatType == GattPresentationFormatTypes.Utf8)
            {
                return GattConvert.ToUTF8String(value);
            }
            else if (format.FormatType == GattPresentationFormatTypes.Utf16)
            {
                return GattConvert.ToUTF16String(value);
            }
            else
            {
                // format.FormatType == GattPresentationFormatTypes.UInt128 ||
                // format.FormatType == GattPresentationFormatTypes.SInt128 ||
                // format.FormatType == GattPresentationFormatTypes.DUInt16 ||
                // format.FormatType == GattPresentationFormatTypes.SInt64 ||
                // format.FormatType == GattPresentationFormatTypes.Struct ||
                // format.FormatType == GattPresentationFormatTypes.Float ||
                // format.FormatType == GattPresentationFormatTypes.Float32 ||
                // format.FormatType == GattPresentationFormatTypes.Float64
                return GattConvert.ToHexString(value);
            }
        }
    }
}
