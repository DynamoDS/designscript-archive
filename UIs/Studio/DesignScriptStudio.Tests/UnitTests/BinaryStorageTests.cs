using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;
using DesignScriptStudio.Graph.Core;

namespace DesignScriptStudio.Tests.UnitTests
{
    class BinaryStorageTests
    {
        [Test]
        public void TestGetPosition00()
        {
            //Assert MemoryStream cursor position before/after seek operation.
            //
            IStorage storage = new BinaryStorage();
            long beforeSeek = storage.GetPosition();
            Assert.AreEqual(0, beforeSeek);

            storage.Seek(4, SeekOrigin.Current);
            long afterSeek = storage.GetPosition();
            Assert.AreEqual(4, afterSeek);
        }

        [Test]
        public void TestGetPosition01()
        {
            //Assert MemoryStream cursor position before/after read operation.
            //MemoryStream needs to be written before reading, hence the involvement of write operation in this test.
            //
            IStorage storage = new BinaryStorage();
            long beforeRead = storage.GetPosition();
            Assert.AreEqual(0, beforeRead);

            ulong signature = Utilities.MakeEightCC('T', 'E', 'S', 'T', ' ', 'I', 'N', 'T');
            storage.WriteInteger(signature, 12);
            storage.Seek(-12, SeekOrigin.End);
            storage.ReadInteger(signature);
            long afterRead = storage.GetPosition();
            Assert.AreEqual(12, afterRead);
        }

        [Test]
        public void TestGetPosition02()
        {
            //Assert MemoryStream cursor position before/after write operation.
            //
            IStorage storage = new BinaryStorage();
            long beforeWrite = storage.GetPosition();
            Assert.AreEqual(0, beforeWrite);

            ulong signature = Utilities.MakeEightCC('T', 'E', 'S', 'T', ' ', 'D', 'B', 'L');
            storage.WriteDouble(signature, 1.20);
            long afterWrite = storage.GetPosition();
            Assert.AreEqual(16, afterWrite);
        }

        [Test]
        public void TestReadWriteBytes00()
        {
            //Functionality test on ReadBytes(ulong expectedSignature) and WriteBytes(ulong signature, int value)
            //Exception expected due to unmatched signature.
            //
            IStorage storage = new BinaryStorage();
            string str = "Hello World";
            byte[] actual = Encoding.ASCII.GetBytes(str);
            ulong signature00 = Utilities.MakeEightCC('T', 'E', 'S', 'T', 'B', 'Y', 'T', 'E');
            ulong signature01 = Utilities.MakeEightCC('T', 'E', 'S', 'T', '_', '_', '_', '_');
            
            storage.WriteBytes(signature00, actual);
            long position = storage.GetPosition();
            Assert.AreEqual(23, position);

            storage.Seek(0, SeekOrigin.Begin);
            Assert.Throws<InvalidDataException>(() =>
            {
                byte[] returned = storage.ReadBytes(signature01);
            });
        }

        [Test]
        public void TestReadWriteBytes01()
        {
            //Functionality test on ReadBytes(ulong expectedSignature) and WriteBytes(ulong signature, int value)
            //Initial value expected since matched signature.
            //
            IStorage storage = new BinaryStorage();
            string str = "Hello World";
            byte[] actual = Encoding.ASCII.GetBytes(str);
            ulong signature = Utilities.MakeEightCC('T', 'E', 'S', 'T', 'B', 'Y', 'T', 'E');

            storage.WriteBytes(signature, actual);
            long position = storage.GetPosition();
            Assert.AreEqual(23, position);

            storage.Seek(0, SeekOrigin.Begin);
            byte[] returned = storage.ReadBytes(signature);
            Assert.AreEqual(actual, returned);
        }

        [Test]
        public void TestReadWriteBytes02()
        {
            //Functionality test on ReadBytes(ulong expectedSignature, int defaultValue) and WriteBytes(ulong signature, int value)
            //Default value expected due to unmatched signature.
            //
            IStorage storage = new BinaryStorage();
            string str00 = "Hello World";
            byte[] actual = Encoding.ASCII.GetBytes(str00);
            string str01 = "World Hello";
            byte[] defaultValue = Encoding.ASCII.GetBytes(str01);
            ulong signature00 = Utilities.MakeEightCC('T', 'E', 'S', 'T', 'B', 'Y', 'T', 'E');
            ulong signature01 = Utilities.MakeEightCC('T', 'E', 'S', 'T', '_', '_', '_', '_');

            storage.WriteBytes(signature00, actual);
            long position = storage.GetPosition();
            Assert.AreEqual(23, position);

            storage.Seek(0, SeekOrigin.Begin);
            byte[] returned = storage.ReadBytes(signature01, defaultValue);
            Assert.AreEqual(defaultValue, returned);
        }

        [Test]
        public void TestReadWriteBytes03()
        {
            //Functionality test on ReadBytes(ulong expectedSignature, int defaultValue) and WriteBytes(ulong signature, int value)
            //Initial value expected since matched signature.
            //
            IStorage storage = new BinaryStorage();
            string str00 = "Hello World";
            byte[] actual = Encoding.ASCII.GetBytes(str00);
            string str01 = "World Hello";
            byte[] defaultValue = Encoding.ASCII.GetBytes(str01);
            ulong signature = Utilities.MakeEightCC('T', 'E', 'S', 'T', 'B', 'Y', 'T', 'E');

            storage.WriteBytes(signature, actual);
            long position = storage.GetPosition();
            Assert.AreEqual(23, position);

            storage.Seek(0, SeekOrigin.Begin);
            byte[] returned = storage.ReadBytes(signature, defaultValue);
            Assert.AreEqual(actual, returned);
        }

        [Test]
        public void TestReadWriteInteger00()
        {
            //Functionality test on ReadInteger(ulong expectedSignature) and WriteInteger(ulong signature, int value)
            //Exception expected due to unmatched signature.
            //
            IStorage storage = new BinaryStorage();
            ulong signature00 = Utilities.MakeEightCC('T', 'E', 'S', 'T', ' ', 'I', 'N', 'T');
            ulong signature01 = Utilities.MakeEightCC('T', 'E', 'S', 'T', '_', 'I', 'N', 'T');

            storage.WriteInteger(signature00, 12);
            long position = storage.GetPosition();
            Assert.AreEqual(12, position);

            storage.Seek(0, SeekOrigin.Begin);
            Assert.Throws<InvalidDataException>(() =>
            {
                int returned = storage.ReadInteger(signature01);
            });
        }

        [Test]
        public void TestReadWriteInteger01()
        {
            //Functionality test on ReadInteger(ulong expectedSignature) and WriteInteger(ulong signature, int value)
            //Initial value expected since matched signature.
            //
            IStorage storage = new BinaryStorage();
            ulong signature = Utilities.MakeEightCC('T', 'E', 'S', 'T', ' ', 'I', 'N', 'T');
            
            storage.WriteInteger(signature, 12);
            long position = storage.GetPosition();
            Assert.AreEqual(12, position);

            storage.Seek(0, SeekOrigin.Begin);
            int returned = storage.ReadInteger(signature);
            Assert.AreEqual(12, returned);
        }

        [Test]
        public void TestReadWriteInteger02()
        {
            //Functionality test on ReadInteger(ulong expectedSignature, int defaultValue) and WriteInteger(ulong signature, int value)
            //Default value expected due to unmatched signature.
            //
            IStorage storage = new BinaryStorage();
            ulong signature00 = Utilities.MakeEightCC('T', 'E', 'S', 'T', ' ', 'I', 'N', 'T');
            ulong signature01 = Utilities.MakeEightCC('T', 'E', 'S', 'T', '_', 'I', 'N', 'T');

            storage.WriteInteger(signature00, 12);
            long position = storage.GetPosition();
            Assert.AreEqual(12, position);

            storage.Seek(0, SeekOrigin.Begin);
            int returned = storage.ReadInteger(signature01, 21);
            Assert.AreEqual(21, returned);
        }

        [Test]
        public void TestReadWriteInteger03()
        {
            //Functionality test on ReadInteger(ulong expectedSignature, int defaultValue) and WriteInteger(ulong signature, int value)
            //Initial value expected since matched signature.
            //
            IStorage storage = new BinaryStorage();
            ulong signature = Utilities.MakeEightCC('T', 'E', 'S', 'T', ' ', 'I', 'N', 'T');

            storage.WriteInteger(signature, 12);
            long position = storage.GetPosition();
            Assert.AreEqual(12, position);

            storage.Seek(0, SeekOrigin.Begin);
            int returned = storage.ReadInteger(signature, 21);
            Assert.AreEqual(12, returned);
        }

        [Test]
        public void TestReadWriteUnsignedInteger00()
        {
            //Functionality test on ReadUnsignedInteger(ulong expectedSignature) and WriteUnsignedInteger(ulong signature, uint value)
            //Exception expected due to unmatched signature.
            //
            IStorage storage = new BinaryStorage();
            ulong signature00 = Utilities.MakeEightCC('T', 'E', 'S', 'T', 'U', 'I', 'N', 'T');
            ulong signature01 = Utilities.MakeEightCC('T', 'E', 'S', 'T', '_', 'I', 'N', 'T');

            storage.WriteUnsignedInteger(signature00, 2147483648);
            long position = storage.GetPosition();
            Assert.AreEqual(12, position);

            storage.Seek(0, SeekOrigin.Begin);
            Assert.Throws<InvalidDataException>(() =>
            {
                uint returned = storage.ReadUnsignedInteger(signature01);
            });
        }

        [Test]
        public void TestReadWriteUnsignedInteger01()
        {
            //Functionality test on ReadUnsignedInteger(ulong expectedSignature) and WriteUnsignedInteger(ulong signature, uint value)
            //Initial value expected since matched signature.
            //
            IStorage storage = new BinaryStorage();
            ulong signature = Utilities.MakeEightCC('T', 'E', 'S', 'T', 'U', 'I', 'N', 'T');

            storage.WriteUnsignedInteger(signature, 2147483648);
            long position = storage.GetPosition();
            Assert.AreEqual(12, position);

            storage.Seek(0, SeekOrigin.Begin);
            uint returned = storage.ReadUnsignedInteger(signature);
            Assert.AreEqual(2147483648, returned);
        }

        [Test]
        public void TestReadWriteUnsignedInteger02()
        {
            //Functionality test on ReadUnsignedInteger(ulong expectedSignature, uint defaultValue) and WriteUnsignedInteger(ulong signature, uint value)
            //Default value expected due to unmatched signature.
            //
            IStorage storage = new BinaryStorage();
            ulong signature00 = Utilities.MakeEightCC('T', 'E', 'S', 'T', 'U', 'I', 'N', 'T');
            ulong signature01 = Utilities.MakeEightCC('T', 'E', 'S', 'T', '_', 'I', 'N', 'T');

            storage.WriteUnsignedInteger(signature00, 2147483648);
            long position = storage.GetPosition();
            Assert.AreEqual(12, position);

            storage.Seek(0, SeekOrigin.Begin);
            uint returned = storage.ReadUnsignedInteger(signature01, 2147483649);
            Assert.AreEqual(2147483649, returned);
        }

        [Test]
        public void TestReadWriteUnsignedInteger03()
        {
            //Functionality test on ReadUnsignedInteger(ulong expectedSignature, uint defaultValue) and WriteUnsignedInteger(ulong signature, uint value)
            //Initial value expected since matched signature.
            //
            IStorage storage = new BinaryStorage();
            ulong signature = Utilities.MakeEightCC('T', 'E', 'S', 'T', ' ', 'I', 'N', 'T');

            storage.WriteUnsignedInteger(signature, 2147483648);
            long position = storage.GetPosition();
            Assert.AreEqual(12, position);

            storage.Seek(0, SeekOrigin.Begin);
            uint returned = storage.ReadUnsignedInteger(signature, 2147483649);
            Assert.AreEqual(2147483648, returned);
        }

        [Test]
        public void TestReadWriteLong00()
        {
            //Functionality test on ReadLong(ulong expectedSignature) and WriteLong(ulong signature, long value)
            //Exception expected due to unmatched signature.
            //
            IStorage storage = new BinaryStorage();
            ulong signature00 = Utilities.MakeEightCC('T', 'E', 'S', 'T', ' ', 'L', 'N', 'G');
            ulong signature01 = Utilities.MakeEightCC('T', 'E', 'S', 'T', '_', 'L', 'N', 'G');

            storage.WriteLong(signature00, 3000000000);
            long position = storage.GetPosition();
            Assert.AreEqual(16, position);

            storage.Seek(0, SeekOrigin.Begin);
            Assert.Throws<InvalidDataException>(() =>
            {
                long returned = storage.ReadLong(signature01);
            });
        }

        [Test]
        public void TestReadWriteLong01()
        {
            //Functionality test on ReadLong(ulong expectedSignature) and WriteLong(ulong signature, long value)
            //Initial value expected since matched signature.
            //
            IStorage storage = new BinaryStorage();
            ulong signature = Utilities.MakeEightCC('T', 'E', 'S', 'T', ' ', 'L', 'N', 'G');

            storage.WriteLong(signature, 3000000000);
            long position = storage.GetPosition();
            Assert.AreEqual(16, position);

            storage.Seek(0, SeekOrigin.Begin);
            long returned = storage.ReadLong(signature);
            Assert.AreEqual(3000000000, returned);
        }

        [Test]
        public void TestReadWriteLong02()
        {
            //Functionality test on ReadLong(ulong expectedSignature, long defaultValue) and WriteLong(ulong signature, long value)
            //Default value expected due to unmatched signature.
            //
            IStorage storage = new BinaryStorage();
            ulong signature00 = Utilities.MakeEightCC('T', 'E', 'S', 'T', ' ', 'L', 'N', 'G');
            ulong signature01 = Utilities.MakeEightCC('T', 'E', 'S', 'T', '_', 'L', 'N', 'G');

            storage.WriteLong(signature00, 3000000000);
            long position = storage.GetPosition();
            Assert.AreEqual(16, position);

            storage.Seek(0, SeekOrigin.Begin);
            long returned = storage.ReadLong(signature01, 3000000001);
            Assert.AreEqual(3000000001, returned);
        }

        [Test]
        public void TestReadWriteLong03()
        {
            //Functionality test on ReadLong(ulong expectedSignature, long defaultValue) and WriteLong(ulong signature, long value)
            //Initial value expected since matched signature.
            //
            IStorage storage = new BinaryStorage();
            ulong signature = Utilities.MakeEightCC('T', 'E', 'S', 'T', ' ', 'L', 'N', 'G');

            storage.WriteLong(signature, 3000000000);
            long position = storage.GetPosition();
            Assert.AreEqual(16, position);

            storage.Seek(0, SeekOrigin.Begin);
            long returned = storage.ReadLong(signature, 3000000001);
            Assert.AreEqual(3000000000, returned);
        }

        [Test]
        public void TestReadWriteUnsignedLong00()
        {
            //Functionality test on ReadUnsignedLong(ulong expectedSignature) and WriteUnsignedLong(ulong signature, long value)
            //Exception expected due to unmatched signature.
            //
            IStorage storage = new BinaryStorage();
            ulong signature00 = Utilities.MakeEightCC('T', 'E', 'S', 'T', 'U', 'L', 'N', 'G');
            ulong signature01 = Utilities.MakeEightCC('T', 'E', 'S', 'T', '_', '_', '_', '_');

            storage.WriteUnsignedLong(signature00, 9223372036854775808);
            long position = storage.GetPosition();
            Assert.AreEqual(16, position);

            storage.Seek(0, SeekOrigin.Begin);
            Assert.Throws<InvalidDataException>(() =>
            {
                ulong returned = storage.ReadUnsignedLong(signature01);
            });
        }

        [Test]
        public void TestReadWriteUnsignedLong01()
        {
            //Functionality test on ReadUnsignedLong(ulong expectedSignature) and WriteUnsignedLong(ulong signature, long value)
            //Initial value expected since matched signature.
            //
            IStorage storage = new BinaryStorage();
            ulong signature = Utilities.MakeEightCC('T', 'E', 'S', 'T', 'U', 'L', 'N', 'G');

            storage.WriteUnsignedLong(signature, 9223372036854775808);
            long position = storage.GetPosition();
            Assert.AreEqual(16, position);

            storage.Seek(0, SeekOrigin.Begin);
            ulong returned = storage.ReadUnsignedLong(signature);
            Assert.AreEqual(9223372036854775808, returned);
        }

        [Test]
        public void TestReadWriteUnsignedLong02()
        {
            //Functionality test on ReadUnsignedLong(ulong expectedSignature, long defaultValue) and WriteUnsignedLong(ulong signature, long value)
            //Default value expected due to unmatched signature.
            //
            IStorage storage = new BinaryStorage();
            ulong signature00 = Utilities.MakeEightCC('T', 'E', 'S', 'T', 'U', 'L', 'N', 'G');
            ulong signature01 = Utilities.MakeEightCC('T', 'E', 'S', 'T', '_', '_', '_', '_');

            storage.WriteUnsignedLong(signature00, 9223372036854775808);
            long position = storage.GetPosition();
            Assert.AreEqual(16, position);

            storage.Seek(0, SeekOrigin.Begin);
            ulong returned = storage.ReadUnsignedLong(signature01, 9223372036854775809);
            Assert.AreEqual(9223372036854775809, returned);
        }

        [Test]
        public void TestReadWriteUnsignedLong03()
        {
            //Functionality test on ReadUnsignedLong(ulong expectedSignature, long defaultValue) and WriteUnsignedLong(ulong signature, long value)
            //Initial value expected since matched signature.
            //
            IStorage storage = new BinaryStorage();
            ulong signature = Utilities.MakeEightCC('T', 'E', 'S', 'T', 'U', 'L', 'N', 'G');

            storage.WriteUnsignedLong(signature, 9223372036854775808);
            long position = storage.GetPosition();
            Assert.AreEqual(16, position);

            storage.Seek(0, SeekOrigin.Begin);
            ulong returned = storage.ReadUnsignedLong(signature, 9223372036854775809);
            Assert.AreEqual(9223372036854775808, returned);
        }

        [Test]
        public void TestReadWriteBoolean00()
        {
            //Functionality test on ReadBoolean(ulong expectedSignature) and WriteBoolean(ulong signature, bool value)
            //Exception expected due to unmatched signature.
            //
            IStorage storage = new BinaryStorage();
            ulong signature00 = Utilities.MakeEightCC('T', 'E', 'S', 'T', 'B', 'O', 'O', 'L');
            ulong signature01 = Utilities.MakeEightCC('T', 'E', 'S', 'T', '_', '_', '_', '_');

            storage.WriteBoolean(signature00, true);
            long position = storage.GetPosition();
            Assert.AreEqual(9, position);

            storage.Seek(0, SeekOrigin.Begin);
            Assert.Throws<InvalidDataException>(() =>
            {
                bool returned = storage.ReadBoolean(signature01);
            });
        }

        [Test]
        public void TestReadWriteBoolean01()
        {
            //Functionality test on ReadBoolean(ulong expectedSignature) and WriteBoolean(ulong signature, bool value)
            //Initial value expected since matched signature.
            //
            IStorage storage = new BinaryStorage();
            ulong signature = Utilities.MakeEightCC('T', 'E', 'S', 'T', 'B', 'O', 'O', 'L');

            storage.WriteBoolean(signature, true);
            long position = storage.GetPosition();
            Assert.AreEqual(9, position);

            storage.Seek(0, SeekOrigin.Begin);
            bool returned = storage.ReadBoolean(signature);
            Assert.AreEqual(true, returned);
        }

        [Test]
        public void TestReadWriteBoolean02()
        {
            //Functionality test on ReadBoolean(ulong expectedSignature, bool defaultValue) and WriteBoolean(ulong signature, bool value)
            //Default value expected due to unmatched signature.
            //
            IStorage storage = new BinaryStorage();
            ulong signature00 = Utilities.MakeEightCC('T', 'E', 'S', 'T', 'B', 'O', 'O', 'L');
            ulong signature01 = Utilities.MakeEightCC('T', 'E', 'S', 'T', '_', '_', '_', '_');

            storage.WriteBoolean(signature00, true);
            long position = storage.GetPosition();
            Assert.AreEqual(9, position);

            storage.Seek(0, SeekOrigin.Begin);
            bool returned = storage.ReadBoolean(signature01, false);
            Assert.AreEqual(false, returned);
        }

        [Test]
        public void TestReadWriteBoolean03()
        {
            //Functionality test on ReadBoolean(ulong expectedSignature, bool defaultValue) and WriteBoolean(ulong signature, bool value)
            //Initial value expected since matched signature.
            //
            IStorage storage = new BinaryStorage();
            ulong signature = Utilities.MakeEightCC('T', 'E', 'S', 'T', 'B', 'O', 'O', 'L');

            storage.WriteBoolean(signature, true);
            long position = storage.GetPosition();
            Assert.AreEqual(9, position);

            storage.Seek(0, SeekOrigin.Begin);
            bool returned = storage.ReadBoolean(signature, false);
            Assert.AreEqual(true, returned);
        }

        [Test]
        public void TestReadWriteDouble00()
        {
            //Functionality test on ReadDouble(ulong expectedSignature) and WriteDouble(ulong signature, double value)
            //Exception expected due to unmatched signature.
            //
            IStorage storage = new BinaryStorage();
            ulong signature00 = Utilities.MakeEightCC('T', 'E', 'S', 'T', ' ', 'D', 'B', 'L');
            ulong signature01 = Utilities.MakeEightCC('T', 'E', 'S', 'T', '_', 'D', 'B', 'L');

            storage.WriteDouble(signature00, 1.2);
            long position = storage.GetPosition();
            Assert.AreEqual(16, position);

            storage.Seek(0, SeekOrigin.Begin);
            Assert.Throws<InvalidDataException>(() =>
            {
                double returned = storage.ReadDouble(signature01);
            });
        }

        [Test]
        public void TestReadWriteDouble01()
        {
            //Functionality test on ReadDouble(ulong expectedSignature) and WriteBoolean(ulong signature, double value)
            //Initial value expected since matched signature.
            //
            IStorage storage = new BinaryStorage();
            ulong signature = Utilities.MakeEightCC('T', 'E', 'S', 'T', ' ', 'D', 'B', 'L');

            storage.WriteDouble(signature, 1.2);
            long position = storage.GetPosition();
            Assert.AreEqual(16, position);

            storage.Seek(0, SeekOrigin.Begin);
            double returned = storage.ReadDouble(signature);
            Assert.AreEqual(1.2, returned);
        }

        [Test]
        public void TestReadWriteDouble02()
        {
            //Functionality test on ReadDouble(ulong expectedSignature, double defaultValue) and WriteDouble(ulong signature, double value)
            //Default value expected due to unmatched signature.
            //
            IStorage storage = new BinaryStorage();
            ulong signature00 = Utilities.MakeEightCC('T', 'E', 'S', 'T', ' ', 'D', 'B', 'L');
            ulong signature01 = Utilities.MakeEightCC('T', 'E', 'S', 'T', '_', 'D', 'B', 'L');

            storage.WriteDouble(signature00, 1.2);
            long position = storage.GetPosition();
            Assert.AreEqual(16, position);

            storage.Seek(0, SeekOrigin.Begin);
            double returned = storage.ReadDouble(signature01, 2.1);
            Assert.AreEqual(2.1, returned);
        }

        [Test]
        public void TestReadWriteDouble03()
        {
            //Functionality test on ReadDouble(ulong expectedSignature, double defaultValue) and WriteDouble(ulong signature, double value)
            //Initial value expected since matched signature.
            //
            IStorage storage = new BinaryStorage();
            ulong signature = Utilities.MakeEightCC('T', 'E', 'S', 'T', ' ', 'D', 'B', 'L');

            storage.WriteDouble(signature, 1.2);
            long position = storage.GetPosition();
            Assert.AreEqual(16, position);

            storage.Seek(0, SeekOrigin.Begin);
            double returned = storage.ReadDouble(signature, 2.1);
            Assert.AreEqual(1.2, returned);
        }

        [Test]
        public void TestReadWriteString00()
        {
            //Functionality test on ReadString(ulong expectedSignature) and WriteDouble(ulong signature, string value)
            //Exception expected due to unmatched signature.
            //
            IStorage storage = new BinaryStorage();
            ulong signature00 = Utilities.MakeEightCC('T', 'E', 'S', 'T', ' ', 'S', 'T', 'R');
            ulong signature01 = Utilities.MakeEightCC('T', 'E', 'S', 'T', '_', 'S', 'T', 'R');

            storage.WriteString(signature00, "Hello World");
            long position = storage.GetPosition();
            Assert.AreEqual(20, position);

            storage.Seek(0, SeekOrigin.Begin);
            Assert.Throws<InvalidDataException>(() =>
            {
                string returned = storage.ReadString(signature01);
            });
        }

        [Test]
        public void TestReadWriteString01()
        {
            //Functionality test on ReadString(ulong expectedSignature) and WriteString(ulong signature, string value)
            //Initial value expected since matched signature.
            //
            IStorage storage = new BinaryStorage();
            ulong signature = Utilities.MakeEightCC('T', 'E', 'S', 'T', ' ', 'S', 'T', 'R');

            storage.WriteString(signature, "Hello World");
            long position = storage.GetPosition();
            Assert.AreEqual(20, position);

            storage.Seek(0, SeekOrigin.Begin);
            string returned = storage.ReadString(signature);
            Assert.AreEqual("Hello World", returned);
        }

        [Test]
        public void TestReadWriteString02()
        {
            //Functionality test on ReadString(ulong expectedSignature, string defaultValue) and WriteString(ulong signature, string value)
            //Default value expected due to unmatched signature.
            //
            IStorage storage = new BinaryStorage();
            ulong signature00 = Utilities.MakeEightCC('T', 'E', 'S', 'T', ' ', 'S', 'T', 'R');
            ulong signature01 = Utilities.MakeEightCC('T', 'E', 'S', 'T', '_', 'S', 'T', 'R');

            storage.WriteString(signature00, "Hello World");
            long position = storage.GetPosition();
            Assert.AreEqual(20, position);

            storage.Seek(0, SeekOrigin.Begin);
            string returned = storage.ReadString(signature01, "World Hello");
            Assert.AreEqual("World Hello", returned);
        }

        [Test]
        public void TestReadWriteString03()
        {
            //Functionality test on ReadString(ulong expectedSignature, string defaultValue) and WriteString(ulong signature, string value)
            //Initial value expected since matched signature.
            //
            IStorage storage = new BinaryStorage();
            ulong signature = Utilities.MakeEightCC('T', 'E', 'S', 'T', ' ', 'S', 'T', 'R');

            storage.WriteString(signature, "Hello World");
            long position = storage.GetPosition();
            Assert.AreEqual(20, position);

            storage.Seek(0, SeekOrigin.Begin);
            string returned = storage.ReadString(signature, "World Hello");
            Assert.AreEqual("Hello World", returned);
        }
    }
}
