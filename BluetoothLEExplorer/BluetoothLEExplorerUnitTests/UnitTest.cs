
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Windows.Storage.Streams;
using System.Text;
using Windows.Security.Cryptography;
using System.Collections;
using GattHelper.Converters;

namespace BluetoothLEExplorer.UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void StringTest1()
        {
            string testString = "Hello World";

            DataWriter writer = new DataWriter();
            writer.WriteString(testString);
            IBuffer testBuffer = writer.DetachBuffer();

            Assert.AreEqual(testString, GattConvert.ToUTF8String(testBuffer));
            Assert.AreNotEqual("Goodbye", GattConvert.ToUTF8String(testBuffer));
        }

        [TestMethod]
        public void IntTest1()
        {
            int data = 42;

            IBuffer buf = GattConvert.ToIBuffer(data);

            Assert.AreEqual(data, GattConvert.ToInt32(buf));
            Assert.AreNotEqual(43, GattConvert.ToInt32(buf));
        }


        [TestMethod]
        public void IntTest2()
        {
            byte[] data = { 42, 0 };
            DataWriter writer = new DataWriter();
            writer.ByteOrder = ByteOrder.LittleEndian;
            writer.WriteBytes(data);

            IBuffer result = writer.DetachBuffer();

            Assert.AreEqual(42, GattConvert.ToInt32(result));
            Assert.AreNotEqual(43, GattConvert.ToInt32(result));
        }

        [TestMethod]
        public void IntTest3()
        {
            Int16 data = 42;
            DataWriter writer = new DataWriter();
            writer.ByteOrder = ByteOrder.LittleEndian;
            writer.WriteInt16(data);

            IBuffer result = writer.DetachBuffer();

            Assert.AreEqual(42, GattConvert.ToInt32(result));
            Assert.AreNotEqual(43, GattConvert.ToInt32(result));
        }

        [TestMethod]
        public void UInt16Test()
        {
            UInt16 data = 42;

            Assert.AreEqual(data, GattConvert.ToUInt16(GattConvert.ToIBuffer(data)));
            Assert.AreNotEqual(data - 1, GattConvert.ToUInt16(GattConvert.ToIBuffer(data)));
        }

        [TestMethod]
        public void UInt32Test()
        {
            UInt32 data = 42;

            Assert.AreEqual(data, GattConvert.ToUInt32(GattConvert.ToIBuffer(data)));
            Assert.AreNotEqual(data - 1, GattConvert.ToUInt32(GattConvert.ToIBuffer(data)));
        }

        [TestMethod]
        public void UInt64Test()
        {
            UInt64 data = 42;

            Assert.AreEqual(data, GattConvert.ToUInt16(GattConvert.ToIBuffer(data)));
            Assert.AreNotEqual(data - 1, GattConvert.ToUInt64(GattConvert.ToIBuffer(data)));
        }

        [TestMethod]
        public void Int16Test()
        {
            Int16 data = 42;
            Int64 wrong = Int64.MaxValue;

            Assert.AreEqual(data, GattConvert.ToInt32(GattConvert.ToIBuffer(data)));
            Assert.AreNotEqual(data - 1, GattConvert.ToInt16(GattConvert.ToIBuffer(data)));

            Assert.AreNotEqual(wrong, GattConvert.ToInt16(GattConvert.ToIBuffer(wrong)));
        }

        [TestMethod]
        public void Int16Test2()
        {
            byte[] input = { 0, 42, 0, 42, 0, 42, 0 };
            byte[] expected = { 0, 42 };

            Assert.AreEqual(BitConverter.ToInt16(expected, 0), GattConvert.ToInt16(GattConvert.ToIBuffer(input)));
        }
        [TestMethod]
        public void Int32Test()
        {
            Int32 data = 42;

            Assert.AreEqual(data, GattConvert.ToInt32(GattConvert.ToIBuffer(data)));
            Assert.AreNotEqual(data - 1, GattConvert.ToInt32(GattConvert.ToIBuffer(data)));
        }

        [TestMethod]
        public void Int64Test()
        {
            Int64 data = 42;

            Assert.AreEqual(data, GattConvert.ToInt64(GattConvert.ToIBuffer(data)));
            Assert.AreNotEqual(data - 1, GattConvert.ToInt64(GattConvert.ToIBuffer(data)));
        }

        [TestMethod]
        public void Int64Test2()
        {
            Int16 data = 42;

            Assert.IsTrue(data == GattConvert.ToInt64(GattConvert.ToIBuffer(data)));
            Assert.IsFalse((data - 1) ==  GattConvert.ToInt64(GattConvert.ToIBuffer(data)));
        }

        [TestMethod]
        public void HexTest1()
        {
            byte[] data = { 0, 1, 2, 42 };
            DataWriter writer = new DataWriter();
            writer.WriteBytes(data);

            IBuffer result = writer.DetachBuffer();

            Assert.AreEqual("00-01-02-2A", GattConvert.ToHexString(result));
            Assert.AreNotEqual("00-00-00-00", GattConvert.ToHexString(result));
        }

        [TestMethod]
        public void HexTest2()
        {
            byte[] data = { 0, 1, 2, 42 };
            byte[] incorrect = { 0, 0, 0, 0 };
            DataWriter writer = new DataWriter();
            writer.WriteBytes(data);
            IBuffer dataIBuffer = writer.DetachBuffer();

            IBuffer resultIBuffer = GattConvert.ToIBufferFromHexString(GattConvert.ToHexString(dataIBuffer));
            byte[] result;
            CryptographicBuffer.CopyToByteArray(resultIBuffer, out result);

            Assert.IsTrue(StructuralComparisons.StructuralEqualityComparer.Equals(data, result));
            Assert.IsFalse(StructuralComparisons.StructuralEqualityComparer.Equals(incorrect, result));
        }

        [TestMethod]
        public void IBufferTest1()
        {
            byte[] correct = { 0x48, 0x65, 0x6C, 0x6C, 0x6F };
            byte[] incorrect = { 0x42, 0x42, 0x42, 0x42, 0x42 };

            byte[] result;
            CryptographicBuffer.CopyToByteArray(GattConvert.ToIBuffer("Hello"), out result);

            Assert.IsTrue(StructuralComparisons.StructuralEqualityComparer.Equals(correct, result));
            Assert.IsFalse(StructuralComparisons.StructuralEqualityComparer.Equals(incorrect, result));

        }

    }
}
