using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;
using DesignScriptStudio.Graph.Core;

namespace DesignScriptStudio.Tests.UnitTests
{
    class SlotTests
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
        public void TestSlotId()
        {
            IGraphController controller = new GraphController(null);
            controller.DoCreateFunctionNode(1, 1, "Sample.dll", "Function", "int,int");

            List<IVisualNode> nodes = ((GraphController)controller).GetVisualNodes();
            VisualNode node = (VisualNode)nodes[0];
            uint slotId = node.GetInputSlot(0);
            ISlot slot = ((GraphController)controller).GetSlot(slotId);
            bool result = false;
            if (slot.SlotId != 0)
                result = true;
            Assert.AreEqual(true, result);
        }

        [Test]
        public void TestSlotType()
        {
            IGraphController controller = new GraphController(null);
            controller.DoCreateFunctionNode(1, 1, "", "Function", "double,double");

            List<IVisualNode> nodes = ((GraphController)controller).GetVisualNodes();
            VisualNode node = (VisualNode)nodes[0];
            uint slotId = node.GetInputSlot(0);
            ISlot slot = ((GraphController)controller).GetSlot(slotId);
            bool result = false;
            if (slot.SlotType != SlotType.None)
                result = true;
            Assert.AreEqual(true, result);
        }

        [Test]
        public void TestOwners()
        {
            IGraphController controller = new GraphController(null);
            controller.DoCreateFunctionNode(1, 1, "", "Function", "int,int");

            List<IVisualNode> nodes = ((GraphController)controller).GetVisualNodes();
            VisualNode node = (VisualNode)nodes[0];
            uint slotId = node.GetInputSlot(0);
            ISlot slot = ((GraphController)controller).GetSlot(slotId);
            bool result = false;

            uint[] owners = slot.Owners;

            if (owners.Count() != 0)
                result = true;

            Assert.AreEqual(true, result);
        }

        [Test]
        public void TestConnectingSlots()
        {
            string commands = @"
                CreateFunctionNode|d:350.0|d:346.0|s:Math.dll|s:Math.Cos|s:double
                CreateFunctionNode|d:636.0|d:275.0|s:ProtoGeometry.dll|s:Point.ByCoordinates|s:double,double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:499.0|d:372.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:638.0|d:290.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            List<IVisualNode> nodes = ((GraphController)controller).GetVisualNodes();
            VisualNode node = (VisualNode)nodes[0];
            uint slotId = node.GetOutputSlot(0);
            ISlot slot = ((GraphController)controller).GetSlot(slotId);
            result = false;
            if (slot.ConnectingSlots != null)
                result = true;
            Assert.AreEqual(true, result);

            node = (VisualNode)nodes[1];
            slotId = node.GetOutputSlot(0);
            slot = ((GraphController)controller).GetSlot(slotId);
            result = false;
            if (slot.ConnectingSlots == null)
                result = true;
            Assert.AreEqual(true, result);
        }

        [Test]
        public void TestDeserilaizeOperationException()
        {
            IGraphController graphController = new GraphController(null);
            IStorage storage = new BinaryStorage();
            IVisualNode node = new FunctionNode(graphController, "", "-", "double,double");
            ISlot slot = new Slot(graphController, SlotType.Input, node);

            storage.WriteUnsignedInteger(FieldCode.SlotSignature, 21);
            storage.Seek(0, SeekOrigin.Begin);

            Assert.Throws<InvalidOperationException>(() =>
            {
                slot.Deserialize(storage);
            });
        }

        [Test]
        public void TestDeserializeNullException()
        {
            IGraphController graphController = new GraphController(null);
            IStorage storage = null;
            IVisualNode node = new FunctionNode(graphController, "", "-", "double,double");
            ISlot slot = new Slot(graphController, SlotType.Input, node);

            Assert.Throws<ArgumentNullException>(() =>
            {
                slot.Deserialize(storage);
            });
        }

        [Test]
        public void TestSerializeNullException()
        {
            IGraphController graphController = new GraphController(null);
            IStorage storage = null;
            IVisualNode node = new FunctionNode(graphController, "", "-", "double,double");
            ISlot slot = new Slot(graphController, SlotType.Input, node);

            Assert.Throws<ArgumentNullException>(() =>
            {
                slot.Serialize(storage);
            });
        }

        [Test]
        public void TestSerializeDeserialize()
        {
            IGraphController graphController = new GraphController(null);
            IStorage storage = new BinaryStorage();
            IVisualNode node = new FunctionNode(graphController, "", "-", "double,double");
            ISlot slot1 = new Slot(graphController, SlotType.Input, node);
            ISlot slot2 = new Slot(graphController, SlotType.Output, node);

            slot1.Serialize(storage);
            storage.Seek(0, SeekOrigin.Begin);
            slot2.Deserialize(storage);

            Assert.AreEqual(SlotType.Input, slot2.SlotType);
            Assert.AreEqual(slot1.SlotId, slot2.SlotId);
            Assert.AreEqual(slot1.Owners[0], slot2.Owners[0]);
            Assert.AreEqual(slot1.ConnectingSlots, slot2.ConnectingSlots);
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
                ISlot slot = Slot.Create(graphController, storage);
            });
        }

        [Test]
        public void TestCreate01()
        {
            IGraphController graphController = new GraphController(null);
            IStorage storage = null;

            Assert.Throws<ArgumentNullException>(() =>
            {
                ISlot slot = Slot.Create(graphController, storage);
            });
        }

        [Test]
        public void TestCreate02()
        {
            IGraphController graphController = new GraphController(null);
            IStorage storage = new BinaryStorage();
            IVisualNode node = new FunctionNode(graphController, "", "-", "double,double");
            ISlot slot1 = new Slot(graphController, SlotType.Input, node);

            slot1.Serialize(storage);
            storage.Seek(0, SeekOrigin.Begin);
            ISlot slot2 = Slot.Create(graphController, storage);

            Assert.AreEqual(SlotType.Input, slot2.SlotType);
            Assert.AreEqual(slot1.SlotId, slot2.SlotId);
            Assert.AreEqual(slot1.Owners[0], slot2.Owners[0]);
            Assert.AreEqual(slot1.ConnectingSlots, slot2.ConnectingSlots);
        }

        [Test]
        public void TestSlotConnections01()
        {
            // 1. Create 5 identifier nodes and one function node ( Max : with 2 input slots ) 
            // 2. connect output of first identifier to input of all 4 identifiers
            // 3. Connect output of one identifier to 2 slots of the Max function
            // 4. Connect output of another identifier to one input slot of the Max function ( the original connection to this slot gets deleted )            


            string commands = @"
                    CreateIdentifierNode|d:561.0|d:189.0
                    CreateIdentifierNode|d:701.0|d:100.0
                    CreateIdentifierNode|d:743.0|d:193.0
                    CreateIdentifierNode|d:741.0|d:317.0
                    BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:376.0|d:276.0
                    SelectComponent|u:0x10000001|e:System.Windows.Input.ModifierKeys,None
                    EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:621.0|d:328.0
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    CreateIdentifierNode|d:638.0|d:349.0
                    CreateFunctionNode|d:509.0|d:423.0|s:Math.dll|s:Math.Max|s:int,int
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                    BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:596.0|d:206.0
                    EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:701.0|d:113.0
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                    BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:597.0|d:210.0
                    EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:746.0|d:208.0
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                    BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:596.0|d:210.0
                    EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:742.0|d:330.0
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                    BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:596.0|d:212.0
                    EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:638.0|d:364.0
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                    BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None|d:677.0|d:364.0
                    EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:495.0|d:410.0
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                    BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:674.0|d:365.0
                    EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:509.0|d:436.0
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                    BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:674.0|d:370.0
                    EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:508.0|d:450.0
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                    BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:777.0|d:335.0
                    EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:508.0|d:438.0
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualEdge edge = controller.GetVisualEdge(0x60000005);
            Assert.AreEqual(null, edge);
            edge = controller.GetVisualEdge(0x60000006);
            Assert.AreEqual(0x60000006, edge.EdgeId);
            
            ISlot outputSlot = controller.GetSlot(0x30000008);
            uint[] connecting = outputSlot.ConnectingSlots;
            Assert.IsNotNull(connecting);
            Assert.AreEqual(0x3000000b, connecting[0]);          
        }
        [Test]
        public void TestSlottypes()
        {
                   //Create a code block node "1+2"
                   // input slots are not expectd here


            string commands = @"
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    CreateCodeBlockNode|d:15325.0|d:15221.0|s:
                    BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                    EndNodeEdit|u:0x10000001|s:1+2|b:True
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            List<ISlot> outputSlot = controller.GetSlots();

            foreach (ISlot i in outputSlot)
            {
                Assert.AreEqual(SlotType.Output, i.SlotType);
            }
          

        }

        #endregion
    }
}
