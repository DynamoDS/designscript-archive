using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;
using DesignScriptStudio.Graph.Core;

namespace DesignScriptStudio.Tests.UnitTests
{
    class VisualEdgeTests
    {
        [SetUp]
        public void SetupTest()
        {
            ICoreComponent coreComponent = ClassFactory.CreateCoreComponent(null);
            coreComponent.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            ClassFactory.DestroyCoreComponent();
        }

        [Test]
        [Ignore]
        public void TestDeserilaizeOperationException()
        {
            IStorage storage = new BinaryStorage();
            GraphController graphController = new GraphController(null);
            EdgeController edgeController = new EdgeController(graphController);
            IVisualEdge edge = new VisualEdge(edgeController, 0x30000001, 0x30000002, false);

            storage.WriteUnsignedInteger(FieldCode.EdgeSignature, 12);
            storage.Seek(0, SeekOrigin.Begin);

            Assert.Throws<InvalidOperationException>(() =>
            {
                edge.Deserialize(storage);
            });
        }

        [Test]
        [Ignore]
        public void TestDeserializeNullException()
        {
            IStorage storage = null;
            GraphController graphController = new GraphController(null);
            EdgeController edgeController = new EdgeController(graphController);
            IVisualEdge edge = new VisualEdge(edgeController, 0x30000001, 0x30000002, false);

            Assert.Throws<ArgumentNullException>(() =>
            {
                edge.Deserialize(storage);
            });
        }

        [Test]
        [Ignore]
        public void TestSerializeNullException()
        {
            IStorage storage = null;
            GraphController graphController = new GraphController(null);
            EdgeController edgeController = new EdgeController(graphController);
            IVisualEdge edge = new VisualEdge(edgeController, 0x30000001, 0x30000002, false);

            Assert.Throws<ArgumentNullException>(() =>
            {
                edge.Serialize(storage);
            });
        }

        [Test]
        [Ignore]
        public void TestSerializeDeserialize()
        {
            IStorage storage = new BinaryStorage();
            GraphController graphController = new GraphController(null);
            EdgeController edgeController = new EdgeController(graphController);
            IVisualEdge edge00 = new VisualEdge(edgeController, 0x30000001, 0x30000002, false);

            edge00.Serialize(storage);
            storage.Seek(0, SeekOrigin.Begin);
            IVisualEdge edge01 = VisualEdge.Create(edgeController, storage);

            Assert.AreEqual(edge00.EdgeId, edge01.EdgeId);
            Assert.AreEqual(edge00.StartSlotId, edge01.StartSlotId);
            Assert.AreEqual(edge00.EndSlotId, edge01.EndSlotId);
        }

        [Test]
        public void TestCreate00()
        {
            IStorage storage = new BinaryStorage();
            EdgeController edgeController = null;

            Assert.Throws<ArgumentNullException>(() =>
            {
                IVisualEdge edge = VisualEdge.Create(edgeController, storage);
            });
        }

        [Test]
        public void TestCreate01()
        {
            IStorage storage = null;
            GraphController graphController = new GraphController(null);
            EdgeController edgeController = new EdgeController(graphController);

            Assert.Throws<ArgumentNullException>(() =>
            {
                IVisualEdge edge = VisualEdge.Create(edgeController, storage);
            });
        }

        [Test]
        [Ignore]
        public void TestCreate02()
        {
            IStorage storage = new BinaryStorage();
            GraphController graphController = new GraphController(null);
            EdgeController edgeController = new EdgeController(graphController);
            IVisualEdge edge00 = new VisualEdge(edgeController, 0x30000001, 0x30000002, false);

            edge00.Serialize(storage);
            storage.Seek(0, SeekOrigin.Begin);
            IVisualEdge edge01 = VisualEdge.Create(edgeController, storage);

            Assert.AreEqual(edge00.EdgeId, edge01.EdgeId);
            Assert.AreEqual(edge00.StartSlotId, edge01.StartSlotId);
            Assert.AreEqual(edge00.EndSlotId, edge01.EndSlotId);
        }
    }
}
