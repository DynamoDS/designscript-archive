using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;
using DesignScriptStudio.Graph.Core;

namespace DesignScriptStudio.Tests.UnitTests
{
    class IdentifierNodeTests
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
            IVisualNode node = new IdentifierNode(graphController, "a");

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
            IVisualNode node = new IdentifierNode(graphController, "a");

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
            IVisualNode node = new IdentifierNode(graphController, "a");

            Assert.Throws<ArgumentNullException>(() =>
            {
                node.Serialize(storage);
            });
        }

        [Test]
        public void TestSerializeDeserialize()
        {
            IGraphController graphController = new GraphController(null);
            IStorage storage = new BinaryStorage();

            IVisualNode node1 = new IdentifierNode(graphController, "a");
            IVisualNode node2 = new IdentifierNode(graphController, "b");

            node1.Serialize(storage);
            storage.Seek(0, SeekOrigin.Begin);
            node2.Deserialize(storage);

            Assert.AreEqual(NodeType.Identifier, node2.VisualType);
            Assert.AreEqual(node1.NodeId, node2.NodeId);
            Assert.AreEqual(true, ((IdentifierNode)node2).Dirty);
            Assert.AreEqual(((IdentifierNode)node1).Text, ((IdentifierNode)node2).Text);
            Assert.AreEqual(((IdentifierNode)node1).Caption, ((IdentifierNode)node2).Caption);
            Assert.AreEqual(node1.X, node2.X);
            Assert.AreEqual(node1.Y, node2.Y);
            Assert.AreEqual(1, node2.GetInputSlots().Length);
            Assert.AreEqual(1, node2.GetOutputSlots().Length);
        }

        [Test]
        public void Defect_IDE_1677()
        {
            string commands = @"CreateIdentifierNode|d:15430.0|d:15290.5
                                CreateIdentifierNode|d:15387.0|d:15217.5
                                CreateIdentifierNode|d:15555.0|d:15123.0";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            VisualNode node;
            node = (VisualNode)controller.GetVisualNode(0x10000001);
            Assert.IsNullOrEmpty(node.ErrorMessage);
            node = (VisualNode)controller.GetVisualNode(0x10000002);
            Assert.IsNullOrEmpty(node.ErrorMessage);
            node = (VisualNode)controller.GetVisualNode(0x10000003);
            Assert.IsNullOrEmpty(node.ErrorMessage);
        }

        [Test]
        public void Defect_IDE_1670()
        {
            string commands = @"CreateIdentifierNode|d:15339.0|d:15187.5
                                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                                CreateDriverNode|d:15422.0|d:15329.5
                                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15456.0|d:15335.0
                                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15307.0|d:15193.0
                                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                                CreateIdentifierNode|d:15463.0|d:15206.5";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            VisualNode node;

            node = (VisualNode)controller.GetVisualNode(0x10000001);
            Assert.IsNullOrEmpty(node.ErrorMessage);
            node = (VisualNode)controller.GetVisualNode(0x10000002);
            Assert.IsNullOrEmpty(node.ErrorMessage);
            node = (VisualNode)controller.GetVisualNode(0x10000003);
            Assert.IsNullOrEmpty(node.ErrorMessage);
        }
    }
}
