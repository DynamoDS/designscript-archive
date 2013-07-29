using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;
using DesignScriptStudio.Graph.Core;

namespace DesignScriptStudio.Tests.UnitTests
{
    class CondensedNodeTests
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
        public void TestDeserilaizeOperationException()
        {
            IGraphController graphController = new GraphController(null);
            IStorage storage = new BinaryStorage();
            CondensedNode node = new CondensedNode(graphController, "Hello World");

            ulong signature = Utilities.MakeEightCC('T', 'E', 'S', 'T', ' ', ' ', ' ', ' ');
            storage.WriteUnsignedInteger(signature, 21);
            storage.Seek(0, SeekOrigin.Begin);

            bool result = node.Deserialize(storage);
            Assert.AreEqual(result, false);
        }

        [Test]
        public void TestDeserializeNullException()
        {
            IGraphController graphController = new GraphController(null);
            IStorage storage = null;
            CondensedNode node = new CondensedNode(graphController, "Hello World");

            Assert.Throws<ArgumentNullException>(() =>
            {
                node.Deserialize(storage);
            });
        }

        [Test]
        public void TestSerializeNullException()
        {
            IGraphController graphController = new GraphController(null);
            IStorage storage = null;
            CondensedNode node = new CondensedNode(graphController, "Hello World");

            Assert.Throws<ArgumentNullException>(() =>
            {
                node.Serialize(storage);
            });
        }

        [Test]
        [Ignore] // to create regression to nunit in jenkins - will remove this - Monika
        public void TestSerializeDeserialize()
        {
            IGraphController graphController = new GraphController(null);
            IStorage storage = new BinaryStorage();

            IVisualNode node1 = new CondensedNode(graphController, "Hello World");
            IVisualNode node2 = new CondensedNode(graphController, "ABC");

            node1.Serialize(storage);
            storage.Seek(0, SeekOrigin.Begin);
            node2.Deserialize(storage);

            Assert.AreEqual(NodeType.Condensed, node2.VisualType);
            Assert.AreEqual(node1.NodeId, node2.NodeId);
            Assert.AreEqual(true, ((CondensedNode)node2).Dirty);
            Assert.AreEqual(((CondensedNode)node1).Text, ((CondensedNode)node2).Text);
            Assert.AreEqual(((CondensedNode)node1).Caption, ((CondensedNode)node2).Caption);
            Assert.AreEqual(node1.X, node2.X);
            Assert.AreEqual(node1.Y, node2.Y);
        }
    }
}
