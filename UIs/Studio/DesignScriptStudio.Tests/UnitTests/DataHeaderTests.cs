using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;
using DesignScriptStudio.Graph.Core;

namespace DesignScriptStudio.Tests.UnitTests
{
    class DataHeaderTests
    {
        [Test]
        public void TestDeserilaizeOperationException()
        {
            IGraphController graphController = new GraphController(null);
            IStorage storage = new BinaryStorage();
            DataHeader header = new DataHeader();

            storage.WriteUnsignedInteger(FieldCode.DataHeaderSignature, 21);
            storage.Seek(0, SeekOrigin.Begin);

            Assert.Throws<InvalidOperationException>(() =>
            {
                header.Deserialize(storage);
            });
        }

        [Test]
        public void TestDeserializeNullException()
        {
            IGraphController graphController = new GraphController(null);
            IStorage storage = null;
            DataHeader header = new DataHeader();

            Assert.Throws<ArgumentNullException>(() =>
            {
                header.Deserialize(storage);
            });
        }

        [Test]
        public void TestSerializeNullException()
        {
            IGraphController graphController = new GraphController(null);
            IStorage storage = null;
            DataHeader header = new DataHeader();

            Assert.Throws<ArgumentNullException>(() =>
            {
                header.Serialize(storage);
            });
        }

        [Test]
        public void TestSerializeDeserialize()
        {
            IGraphController graphController = new GraphController(null);
            IStorage storage = new BinaryStorage();
            DataHeader header1 = new DataHeader();
            header1.DataSize = 21;
            DataHeader header2 = new DataHeader();
            header2.DataSize = 12;

            header1.Serialize(storage);
            storage.Seek(0, SeekOrigin.Begin);
            header2.Deserialize(storage);

            Assert.AreEqual(21, header2.DataSize);
        }
    }
}
