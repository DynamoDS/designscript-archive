using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;
using DesignScriptStudio.Graph.Core;

namespace DesignScriptStudio.Tests.UnitTests
{
    class VisualNodeTests
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

        #region Test Public Interface Methods

        [Test]
        public void TestGetInputSlots()
        {
            IGraphController controller = new GraphController(null);
            controller.DoCreateFunctionNode(1, 1, "", "Function", "int,int");

            List<IVisualNode> nodes = ((GraphController)controller).GetVisualNodes();
            uint[] list = nodes[0].GetInputSlots();
            int count = list.Count();
            Assert.AreEqual(2, count);
        }

        [Test]
        public void TestGetOutputSlots()
        {
            IGraphController controller = new GraphController(null);
            controller.DoCreateFunctionNode(1, 1, "", "Function", "int,int");

            List<IVisualNode> nodes = ((GraphController)controller).GetVisualNodes();
            uint[] list = nodes[0].GetOutputSlots();
            int count = list.Count();
            Assert.AreEqual(1, count);
        }

        [Test]
        public void TestGetInputSlot()
        {
            IGraphController controller = new GraphController(null);
            controller.DoCreateFunctionNode(1, 1, "", "Function", "int,int");

            List<IVisualNode> nodes = ((GraphController)controller).GetVisualNodes();
            uint slotId = nodes[0].GetInputSlot(0);
            ISlot slot = ((GraphController)controller).GetSlot(slotId);
            Assert.AreEqual(SlotType.Input, slot.SlotType);
        }


        [Test]
        public void TestGetOutputSlot()
        {
            IGraphController controller = new GraphController(null);
            controller.DoCreateFunctionNode(1, 1, "", "Function", "int,int");

            List<IVisualNode> nodes = ((GraphController)controller).GetVisualNodes();
            uint slotId = nodes[0].GetOutputSlot(0);
            ISlot slot = ((GraphController)controller).GetSlot(slotId);
            Assert.AreEqual(SlotType.Output, slot.SlotType);
        }

        [Test]
        public void TestEnableEdit()
        {
            IGraphController controller = new GraphController(null);
            controller.DoCreateFunctionNode(1, 1, "", "Function", "int,int");

            List<IVisualNode> nodes = ((GraphController)controller).GetVisualNodes();
            VisualNode newNode = nodes[0] as VisualNode;

            newNode.EnableEdit(NodePart.Caption,-1);

            bool result = false;
            result = newNode.EnableEdit(NodePart.None,-1);
            Assert.AreEqual(false, result);
            newNode.DisableEdit();
            result = newNode.EnableEdit(NodePart.Caption,-1);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void TestDisableEdit()
        {
            IGraphController controller = new GraphController(null);
            controller.DoCreateFunctionNode(1, 1, "", "Function", "int,int");

            List<IVisualNode> nodes = ((GraphController)controller).GetVisualNodes();
            VisualNode newNode = nodes[0] as VisualNode;
            Assert.Throws<InvalidOperationException>(() =>
            {
                newNode.DisableEdit();
            });

            bool result = false;
            newNode.EnableEdit(NodePart.Caption,-1);
            result = newNode.DisableEdit();
            Assert.AreEqual(true, result);
        }

        [Test]
        public void TestEdit()
        {

            IGraphController controller = new GraphController(null);
            controller.DoCreateFunctionNode(1, 1, "", "Function", "int,int");
            List<IVisualNode> nodes = ((GraphController)controller).GetVisualNodes();
            VisualNode newNode = nodes[0] as VisualNode;
            Assert.Throws<InvalidOperationException>(() =>
            {
                newNode.Edit("newText", false);
            });

            bool result = false;
            newNode.EnableEdit(NodePart.Caption,-1);
            result = newNode.Edit("newText", false);
            newNode.DisableEdit();
            Assert.AreEqual(true, result);

            if (((VisualNode)nodes[0]).Caption == "newText")
                result = true;

            Assert.AreEqual(true, result);
        }
        #endregion

        #region Test Public Class Methods

        [Test]
        public void TestCreate00()
        {
            IGraphController graphController = null;
            IStorage storage = new BinaryStorage();

            Assert.Throws<ArgumentNullException>(() =>
            {
                IVisualNode node = VisualNode.Create(graphController, storage);
            });
        }

        [Test]
        public void TestCreate01()
        {
            IGraphController graphController = new GraphController(null);
            IStorage storage = null;

            Assert.Throws<ArgumentNullException>(() =>
            {
                IVisualNode node = VisualNode.Create(graphController, storage);
            });
        }

        [Test]
        public void TestCreate02()
        {
            IGraphController graphController = new GraphController(null);
            IStorage storage = new BinaryStorage();

            storage.WriteUnsignedInteger(FieldCode.NodeSignature, 12);
            storage.WriteInteger(FieldCode.NodeType, 21);
            storage.Seek(0, SeekOrigin.Begin);

            Assert.Throws<ArgumentException>(() =>
            {
                IVisualNode node = VisualNode.Create(graphController, storage);
            });
        }

        #endregion

        #region Test Public Slot Related Methods

        [Test]
        public void TestGetSlotIndex()
        {
            IGraphController controller = new GraphController(null);
            controller.DoCreateFunctionNode(1, 1, "", "Function", "int,int");

            List<IVisualNode> nodes = ((GraphController)controller).GetVisualNodes();

            VisualNode node = ((VisualNode)nodes[0]);
            uint slotId = node.GetInputSlot(0);
            int result = node.GetSlotIndex(slotId);
            Assert.AreEqual(0, result);

            slotId = node.GetOutputSlot(0);
            result = node.GetSlotIndex(slotId);
            Assert.AreEqual(0, result);

            //try to get index of a non-existent slot
            Assert.Throws<InvalidOperationException>(() =>
            {
                result = node.GetSlotIndex(0x12345678);
            });
        }

        [Test]
        public void TestGetSlotPosition()
        {
            IGraphController controller = new GraphController(null);
            controller.DoCreateFunctionNode(1, 1, "", "Function", "int,int");

            List<IVisualNode> nodes = ((GraphController)controller).GetVisualNodes();
            VisualNode node = ((VisualNode)nodes[0]);

            uint slotId = node.GetOutputSlot(0);
            ISlot slot = ((GraphController)controller).GetSlot(slotId);
            System.Windows.Point pt = node.GetSlotPosition((Slot)slot);

            bool result = false;
            if (pt != null)
                result = true;
            Assert.AreEqual(true, result);

            slot = null;
            Assert.Throws<ArgumentNullException>(() =>
            {
                pt = node.GetSlotPosition((Slot)slot);
            });
        }

        [Test]
        public void TestGetSlot()
        {
            //Victor please add this test
        }

        #endregion
    }
}
