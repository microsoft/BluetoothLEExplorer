using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace GattHelper.Converters
{
    public static class GattConvert
    {
        public static IBuffer ToIBufferFromHexString(string data)
        {
            DataWriter writer = new DataWriter();
            data = data.Replace("-", "");

            if (data.Length > 0)
            {
                if (data.Length % 2 != 0)
                {
                    data = "0" + data;
                }

                int NumberChars = data.Length;
                byte[] bytes = new byte[NumberChars / 2];

                for (int i = 0; i < NumberChars; i += 2)
                {
                    bytes[i / 2] = Convert.ToByte(data.Substring(i, 2), 16);
                }
                writer.WriteBytes(bytes);
            }
            return writer.DetachBuffer();
        }

        public static IBuffer ToIBuffer(bool data)
        {
            DataWriter writer = new DataWriter();
            writer.ByteOrder = ByteOrder.LittleEndian;
            writer.WriteBoolean(data);
            return writer.DetachBuffer();
        }

        public static IBuffer ToIBuffer(byte data)
        {
            DataWriter writer = new DataWriter();
            writer.ByteOrder = ByteOrder.LittleEndian;
            writer.WriteByte(data);
            return writer.DetachBuffer();
        }

        public static IBuffer ToIBuffer(byte[] data)
        {
            DataWriter writer = new DataWriter();
            writer.ByteOrder = ByteOrder.LittleEndian;
            writer.WriteBytes(data);
            return writer.DetachBuffer();
        }

        public static IBuffer ToIBuffer(double data)
        {
            DataWriter writer = new DataWriter();
            writer.ByteOrder = ByteOrder.LittleEndian;
            writer.WriteDouble(data);
            return writer.DetachBuffer();
        }

        public static IBuffer ToIBuffer(Int16 data)
        {
            DataWriter writer = new DataWriter();
            writer.ByteOrder = ByteOrder.LittleEndian;
            writer.WriteInt16(data);
            return writer.DetachBuffer();
        }

        public static IBuffer ToIBuffer(Int32 data)
        {
            DataWriter writer = new DataWriter();
            writer.ByteOrder = ByteOrder.LittleEndian;
            writer.WriteInt32(data);
            return writer.DetachBuffer();
        }

        public static IBuffer ToIBuffer(Int64 data)
        {
            DataWriter writer = new DataWriter();
            writer.ByteOrder = ByteOrder.LittleEndian;
            writer.WriteInt64(data);
            return writer.DetachBuffer();
        }

        public static IBuffer ToIBuffer(Single data)
        {
            DataWriter writer = new DataWriter();
            writer.ByteOrder = ByteOrder.LittleEndian;
            writer.WriteSingle(data);
            return writer.DetachBuffer();
        }

        public static IBuffer ToIBuffer(UInt16 data)
        {
            DataWriter writer = new DataWriter();
            writer.ByteOrder = ByteOrder.LittleEndian;
            writer.WriteUInt16(data);
            return writer.DetachBuffer();
        }

        public static IBuffer ToIBuffer(UInt32 data)
        {
            DataWriter writer = new DataWriter();
            writer.ByteOrder = ByteOrder.LittleEndian;
            writer.WriteUInt32(data);
            return writer.DetachBuffer();
        }

        public static IBuffer ToIBuffer(UInt64 data)
        {
            DataWriter writer = new DataWriter();
            writer.ByteOrder = ByteOrder.LittleEndian;
            writer.WriteUInt64(data);
            return writer.DetachBuffer();
        }

        public static IBuffer ToIBuffer(string data)
        {
            DataWriter writer = new DataWriter();
            writer.ByteOrder = ByteOrder.LittleEndian;
            writer.WriteString(data);
            return writer.DetachBuffer();
        }

        public static string ToUTF8String(IBuffer buffer)
        {
            DataReader reader = DataReader.FromBuffer(buffer);
            reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
            reader.ByteOrder = Windows.Storage.Streams.ByteOrder.LittleEndian;
            return reader.ReadString(buffer.Length);
        }

        public static string ToUTF16String(IBuffer buffer)
        {
            DataReader reader = DataReader.FromBuffer(buffer);
            reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf16LE;
            reader.ByteOrder = Windows.Storage.Streams.ByteOrder.LittleEndian;

            // UTF16 characters are 2 bytes long and ReadString takes the character count,
            // divide the buffer length by 2.
            return reader.ReadString(buffer.Length / 2);
        }

        public static Int16 ToInt16(IBuffer buffer)
        {
            byte[] data = new byte[buffer.Length];
            DataReader reader = DataReader.FromBuffer(buffer);
            reader.ByteOrder = ByteOrder.LittleEndian;
            reader.ReadBytes(data);
            data = PadBytes(data, 2);
            return BitConverter.ToInt16(data, 0);
        }

        public static int ToInt32(IBuffer buffer)
        {
            if (buffer.Length > sizeof(Int32))
            {
                throw new ArgumentException("Cannot convert to Int32, buffer is too large");
            }

            byte[] data = new byte[buffer.Length];
            DataReader reader = DataReader.FromBuffer(buffer);
            reader.ByteOrder = ByteOrder.LittleEndian;
            reader.ReadBytes(data);
            data = PadBytes(data, 4);
            return BitConverter.ToInt32(data, 0);
        }

        public static Int64 ToInt64(IBuffer buffer)
        { 
            if (buffer.Length > sizeof(Int64))
            {
                throw new ArgumentException("Cannot convert to Int64, buffer is too large");
            }

            byte[] data = new byte[buffer.Length];
            DataReader reader = DataReader.FromBuffer(buffer);
            reader.ByteOrder = ByteOrder.LittleEndian;
            reader.ReadBytes(data);
            data = PadBytes(data, 8);
            return BitConverter.ToInt32(data, 0);
        }

        public static Single ToSingle(IBuffer buffer)
        {
            byte[] data = new byte[buffer.Length];
            DataReader reader = DataReader.FromBuffer(buffer);
            reader.ByteOrder = ByteOrder.LittleEndian;
            reader.ReadBytes(data);
            data = PadBytes(data, 4);
            return BitConverter.ToSingle(data, 0);
        }

        public static Double ToDouble(IBuffer buffer)
        {
            byte[] data = new byte[buffer.Length];
            DataReader reader = DataReader.FromBuffer(buffer);
            reader.ByteOrder = ByteOrder.LittleEndian;
            reader.ReadBytes(data);
            data = PadBytes(data, 8);
            return BitConverter.ToDouble(data, 0);
        }

        public static UInt16 ToUInt16(IBuffer buffer)
        {
            byte[] data = new byte[buffer.Length];
            DataReader reader = DataReader.FromBuffer(buffer);
            reader.ByteOrder = ByteOrder.LittleEndian;
            reader.ReadBytes(data);
            data = PadBytes(data, 2);
            return BitConverter.ToUInt16(data, 0);
        }

        public static UInt32 ToUInt32(IBuffer buffer)
        {
            byte[] data = new byte[buffer.Length];
            DataReader reader = DataReader.FromBuffer(buffer);
            reader.ByteOrder = ByteOrder.LittleEndian;
            reader.ReadBytes(data);
            data = PadBytes(data, 4);
            return BitConverter.ToUInt32(data, 0);
        }

        public static UInt64 ToUInt64(IBuffer buffer)
        {
            byte[] data = new byte[buffer.Length];
            DataReader reader = DataReader.FromBuffer(buffer);
            reader.ByteOrder = ByteOrder.LittleEndian;
            reader.ReadBytes(data);
            data = PadBytes(data, 8);
            return BitConverter.ToUInt64(data, 0);
        }

        public static byte[] ToByteArray(IBuffer buffer)
        {
            byte[] data = new byte[buffer.Length];
            DataReader reader = DataReader.FromBuffer(buffer);
            reader.ByteOrder = ByteOrder.LittleEndian;
            reader.ReadBytes(data);
            return data;
        }

        public static string ToHexString(IBuffer buffer)
        {
            byte[] data;
            CryptographicBuffer.CopyToByteArray(buffer, out data);
            return BitConverter.ToString(data);
        }

        /// <summary>
        /// Takes an input array of bytes and returns an array with more zeros in the front
        /// </summary>
        /// <param name="input"></param>
        /// <param name="length"></param>
        /// <returns>A byte array with more zeros in back per little endianness"/></returns>
        private static byte[] PadBytes(byte[] input, int length)
        {
            if (input.Length >= length)
            {
                return input;
            }

            byte[] ret = new byte[length];
            Array.Copy(input, ret, input.Length);
            return ret;
        }
    }
}
