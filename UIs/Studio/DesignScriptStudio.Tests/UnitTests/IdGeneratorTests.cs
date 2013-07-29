using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DesignScriptStudio.Graph.Core;

namespace DesignScriptStudio.Tests.UnitTests
{
    class IdGeneratorTests
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
        public void GetNextIdTest01()
        {
            // Test to make sure that fresh IdGenerator always start with '1'.
            IdGenerator generator = new IdGenerator();
            uint firstNodeId = generator.GetNextId(ComponentType.Node);
            uint firstSlotId = generator.GetNextId(ComponentType.Slot);
            uint firstEdgeId = generator.GetNextId(ComponentType.Edge);
            uint firstBubbleId = generator.GetNextId(ComponentType.Bubble);

            Assert.AreEqual((((uint)ComponentType.Node) | 0x01), firstNodeId);
            Assert.AreEqual((((uint)ComponentType.Slot) | 0x01), firstSlotId);
            Assert.AreEqual((((uint)ComponentType.Edge) | 0x01), firstEdgeId);
            Assert.AreEqual((((uint)ComponentType.Bubble) | 0x01), firstBubbleId);
        }

        [Test]
        public void GetNextIdTest02()
        {
            // Test to make sure that "GetNextId" always increase id by '1'.
            IdGenerator generator = new IdGenerator();
            uint firstNodeId = generator.GetNextId(ComponentType.Node);
            uint firstSlotId = generator.GetNextId(ComponentType.Slot);
            uint firstEdgeId = generator.GetNextId(ComponentType.Edge);
            uint firstBubbleId = generator.GetNextId(ComponentType.Bubble);

            uint secondNodeId = generator.GetNextId(ComponentType.Node);
            uint secondSlotId = generator.GetNextId(ComponentType.Slot);
            uint secondEdgeId = generator.GetNextId(ComponentType.Edge);
            uint secondBubbleId = generator.GetNextId(ComponentType.Bubble);

            Assert.AreEqual(1, secondNodeId - firstNodeId);
            Assert.AreEqual(1, secondSlotId - firstSlotId);
            Assert.AreEqual(1, secondEdgeId - firstEdgeId);
            Assert.AreEqual(1, secondBubbleId - firstBubbleId);
        }

        [Test]
        public void GetNextIdTest03()
        {
            // Test to make sure "GetNextId" returns the type it should.
            IdGenerator generator = new IdGenerator();
            uint firstNodeId = generator.GetNextId(ComponentType.Node);
            uint firstSlotId = generator.GetNextId(ComponentType.Slot);
            uint firstEdgeId = generator.GetNextId(ComponentType.Edge);
            uint firstBubbleId = generator.GetNextId(ComponentType.Bubble);

            Assert.AreEqual(ComponentType.Node, IdGenerator.GetType(firstNodeId));
            Assert.AreEqual(ComponentType.Slot, IdGenerator.GetType(firstSlotId));
            Assert.AreEqual(ComponentType.Edge, IdGenerator.GetType(firstEdgeId));
            Assert.AreEqual(ComponentType.Bubble, IdGenerator.GetType(firstBubbleId));
        }

        [Test]
        public void GetNextIdTest04()
        {
            // Test to make sure "GetNextId" throws an exception for wrong input.
            Assert.Throws<ArgumentException>(() =>
            {
                IdGenerator generator = new IdGenerator();
                generator.GetNextId((ComponentType)1234);
            });
        }

        [Test]
        public void SetStartIdTest01()
        {
            // Test to make sure the start id is properly set, and that 
            // trying to get the next id returns the next higher number.
            IdGenerator generator = new IdGenerator();
            uint maxNodeId = (((uint)ComponentType.Node) | 0x1234);
            uint maxSlotId = (((uint)ComponentType.Slot) | 0x4567);
            uint maxEdgeId = (((uint)ComponentType.Edge) | 0x69ab);
            uint maxBubbleId = (((uint)ComponentType.Bubble) | 0x89ab);

            generator.SetStartId(maxNodeId);
            generator.SetStartId(maxSlotId);
            generator.SetStartId(maxEdgeId);
            generator.SetStartId(maxBubbleId);

            uint nextNodeId = generator.GetNextId(ComponentType.Node);
            uint nextSlotId = generator.GetNextId(ComponentType.Slot);
            uint nextEdgeId = generator.GetNextId(ComponentType.Edge);
            uint nextBubbleId = generator.GetNextId(ComponentType.Bubble);

            Assert.AreEqual(1, nextNodeId - maxNodeId);
            Assert.AreEqual(1, nextSlotId - maxSlotId);
            Assert.AreEqual(1, nextEdgeId - maxEdgeId);
            Assert.AreEqual(1, nextBubbleId - maxBubbleId);
        }

        [Test]
        public void SetStartIdTest02()
        {
            // Test to make sure "SetStartId" throws an exception for wrong input.
            Assert.Throws<ArgumentException>(() =>
            {
                IdGenerator generator = new IdGenerator();
                generator.SetStartId(0x12345678);
            });
        }

        [Test]
        public void GetTypeTest01()
        {
            // Test to make sure "GetType" returns the right component type.
            uint someNodeId = (((uint)ComponentType.Node) | 0x1234);
            uint someSlotId = (((uint)ComponentType.Slot) | 0x4567);
            uint someEdgeId = (((uint)ComponentType.Edge) | 0x69ab);
            uint someBubbleId = (((uint)ComponentType.Bubble) | 0x89ab);

            Assert.AreEqual(ComponentType.Node, IdGenerator.GetType(someNodeId));
            Assert.AreEqual(ComponentType.Slot, IdGenerator.GetType(someSlotId));
            Assert.AreEqual(ComponentType.Edge, IdGenerator.GetType(someEdgeId));
            Assert.AreEqual(ComponentType.Bubble, IdGenerator.GetType(someBubbleId));
        }

        [Test]
        public void GetTypeTest02()
        {
            // Test to make sure "GetType" return ComponentType.None when id is incorrect
            Assert.AreEqual(ComponentType.None, IdGenerator.GetType(0x12345678));
        }
    }
}
