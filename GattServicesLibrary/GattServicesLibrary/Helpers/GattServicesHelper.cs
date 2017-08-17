// <copyright file="GattServicesHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using System.Diagnostics;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace GattServicesLibrary.Helpers
{
    /// <summary>
    /// Gatt Service Helper Class containing Utility functions used across the services.
    /// </summary>
    public static class GattServicesHelper
    {
        /// <summary>
        /// Get Characteristics from the Characteristics Result
        /// </summary>
        /// <param name="result">Gatt Characteristics Result</param>
        /// <param name="characteristics">Gatt characteristics</param>
        public static void GetCharacteristicsFromResult(GattLocalCharacteristicResult result, ref GattLocalCharacteristic characteristics)
        {
            if (result.Error == BluetoothError.Success)
            {
                characteristics = result.Characteristic;
            }
            else
            {
                Debug.WriteLine(result.Error.ToString());
            }
        }

        /// <summary>
        /// Converts byte value into Buffer
        /// </summary>
        /// <param name="byteValue">Byte value</param>
        /// <returns>Data writer buffer</returns>
        public static IBuffer ConvertValueToBuffer(byte byteValue)
        {
            //TODO: User GattConvert here
            DataWriter writer = new DataWriter();
            writer.WriteByte(byteValue);
            return writer.DetachBuffer();
        }

        /// <summary>
        /// Converts two byte values into buffer
        /// </summary>
        /// <param name="byteValue1">Byte value 1</param>
        /// <param name="byteValue2">Byte value 2</param>
        /// <returns>Data writer buffer</returns>
        public static IBuffer ConvertValueToBuffer(byte byteValue1, byte byteValue2)
        {
            DataWriter writer = new DataWriter();
            writer.WriteByte(byteValue1);
            writer.WriteByte(byteValue2);

            return writer.DetachBuffer();
        }

        /// <summary>
        /// Converts date time value to buffer
        /// </summary>
        /// <param name="time">DateTime value</param>
        /// <returns>Data Writer Buffer</returns>
        public static IBuffer ConvertValueToBuffer(DateTime time)
        {
            DataWriter writer = new DataWriter();

            // Date time according to: https://www.bluetooth.com/specifications/gatt/viewer?attributeXmlFile=org.bluetooth.characteristic.date_time.xml
            writer.WriteUInt16((ushort)time.Year);
            writer.WriteByte((byte)time.Month);
            writer.WriteByte((byte)time.Day);
            writer.WriteByte((byte)time.Hour);
            writer.WriteByte((byte)time.Minute);
            writer.WriteByte((byte)time.Second);

            // Day of week according to: https://www.bluetooth.com/specifications/gatt/viewer?attributeXmlFile=org.bluetooth.characteristic.day_of_week.xml
            // Going to leave this "not known" for now - would have to perform a rotate of DayOfWeek property
            writer.WriteByte(0x0);

            return writer.DetachBuffer();
        }
    }
}
