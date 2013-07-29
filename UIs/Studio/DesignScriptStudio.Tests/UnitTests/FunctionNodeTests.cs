using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;
using DesignScriptStudio.Graph.Core;

namespace DesignScriptStudio.Tests.UnitTests
{
    class FunctionNodeTests
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
            IVisualNode node = new FunctionNode(graphController, "", "-", "double,double");

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
            IVisualNode node = new FunctionNode(graphController, "", "-", "double,double");

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
            IVisualNode node = new FunctionNode(graphController, "", "-", "double,double");

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

            IVisualNode node1 = new FunctionNode(graphController, "", "-", "double,double");
            IVisualNode node2 = new FunctionNode(graphController, "", "+", "");

            node1.Serialize(storage);
            storage.Seek(0, SeekOrigin.Begin);
            node2.Deserialize(storage);

            Assert.AreEqual(NodeType.Function, node2.VisualType);
            Assert.AreEqual(node1.NodeId, node2.NodeId);
            Assert.AreEqual(true, ((FunctionNode)node2).Dirty);
            Assert.AreEqual(((FunctionNode)node1).Text, ((FunctionNode)node2).Text);
            Assert.AreEqual(((FunctionNode)node1).Caption, ((FunctionNode)node2).Caption);
            Assert.AreEqual(node1.X, node2.X);
            Assert.AreEqual(node2.Y, node2.Y);
            Assert.AreEqual(node1.GetInputSlots().Length, node2.GetInputSlots().Length);
            Assert.AreEqual(node1.GetOutputSlots().Length, node2.GetOutputSlots().Length);
            Assert.AreEqual(((FunctionNode)node1).Assembly, ((FunctionNode)node2).Assembly);
            Assert.AreEqual(((FunctionNode)node1).QualifiedName, ((FunctionNode)node2).QualifiedName);
            Assert.AreEqual(((FunctionNode)node1).ArgumentTypes, ((FunctionNode)node2).ArgumentTypes);
        }

        [Test]
        public void TestMultipleOutputTest()
        {
            string commands = @"
                CreateFunctionNode|d:307.0|d:375.0|s:|s:+|s:double,double
                CreateFunctionNode|d:518.0|d:339.0|s:|s:+|s:double,double
                CreateFunctionNode|d:512.0|d:435.0|s:|s:+|s:double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void TestFunctionNodeCaption()
        {
            string commands = @"
                CreateFunctionNode|d:444.0|d:211.0|s:ProtoGeometry.dll|s:Point.ByCoordinates|s:double,double,double
                CreateFunctionNode|d:432.0|d:299.0|s:Math.dll|s:Math.Cos|s:double";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            result = false;
            List<IVisualNode> nodes = ((GraphController)controller).GetVisualNodes();
            if (((VisualNode)nodes[0]).Caption == "Point")
                result = true;
            Assert.AreEqual(true, result);

            result = false;
            nodes = ((GraphController)controller).GetVisualNodes();
            if (((VisualNode)nodes[1]).Caption == "Cos")
                result = true;
            Assert.AreEqual(true, result);
        }

        [Test]
        public void Defect_IDE_1307()
        {
            // [CRASH] while after connecting two inputs to Range node.
            // Drag and drop Range Node and create two Code Block nodes with value 1 and 10.
            // Connected Code Block node with value 1 to Start and with value 10 to End.
            // after above step DSS is crashing.

            string commands = @"
                CreateCodeBlockNode|d:9431.0|d:9872.5|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:1|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:9444.66666667|d:9931.5|s:
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:10|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateFunctionNode|d:9532.1|d:9893.18095238|s:|s:Range|s:double,double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:9454.1|d:9884.38095238
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:9528.1|d:9902.38095238
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:9471.3|d:9947.58095238
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:9536.5|d:9927.98095238
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000001);// Code Block node.
            IVisualNode node02 = controller.GetVisualNode(0x10000002);// Code Block node.
            IVisualNode node03 = controller.GetVisualNode(0x10000003);// Range node.

            ISlot outputSlot = controller.GetSlot(0x30000001); // Output of first CodeBlock ndoe.
            Assert.IsNotNull(outputSlot);
            ISlot inputSlot = controller.GetSlot(0x30000003); // Input of first slot of  Range Node.
            Assert.IsNotNull(inputSlot);
            // Output slot CoedBlock Node should connect to first slot of Range node.
            uint[] connecting = outputSlot.ConnectingSlots;
            Assert.IsNotNull(connecting);
            Assert.IsTrue(connecting.Contains((uint)0x30000003));

            ISlot outputSlot01 = controller.GetSlot(0x30000002); // Output of first CodeBlock ndoe.
            Assert.IsNotNull(outputSlot01);
            ISlot inputSlot01 = controller.GetSlot(0x30000004); // Input of second slot of  Range Node.
            Assert.IsNotNull(inputSlot01);
            // Output slot CoedBlock Node should connect to second slot of Range node.
            uint[] connecting01 = outputSlot01.ConnectingSlots;
            Assert.IsNotNull(connecting01);
            Assert.IsTrue(connecting01.Contains((uint)0x30000004));
            Assert.AreEqual(3, controller.GetVisualNodes().Count);
        }

        [Test]
        public void Defect_IDE_1349()
        {

            //1. Create Point.ByCoordinates
            //2. Create another Point.ByCoordinates, and a Line.ByStartPointEndPoint node
            //3. Create a CBN '0' and connect to the 3 inputs of the first Point
            //4. Create a CBN '10', and connect to the 'X' input of the second Point, and connect '0' to the 'Y' and 'Z' inpouts of this point
            //5. Now join the outputs from the two points to the Line node
            //6. Now create a CBN node, and move around the other nodes a bit -> observe the line node should not update
            
            string commands = @"
                CreateFunctionNode|d:10399.0|d:10262.5|s:ProtoGeometry.dll|s:Point.ByCoordinates|s:double,double,double
                CreateFunctionNode|d:10399.0|d:10262.5|s:ProtoGeometry.dll|s:Point.ByCoordinates|s:double,double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None|d:10451.0|d:10298.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None|d:10461.0|d:10145.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                CreateFunctionNode|d:10399.0|d:10262.5|s:ProtoGeometry.dll|s:Line.ByStartPointEndPoint|s:Point,Point
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:1|e:System.Windows.Input.ModifierKeys,None|d:10463.0|d:10289.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:1|e:System.Windows.Input.ModifierKeys,None|d:10652.0|d:10205.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10298.0|d:10153.0|s:
                BeginNodeEdit|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000004|s:0|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10306.0|d:10269.0|s:
                BeginNodeEdit|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000005|s:10|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10322.0|d:10167.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10403.0|d:10126.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10320.0|d:10168.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:10406.0|d:10135.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10320.0|d:10164.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None|d:10402.0|d:10164.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10334.0|d:10281.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10393.0|d:10273.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10322.0|d:10164.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:10391.0|d:10291.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10321.0|d:10169.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None|d:10396.0|d:10320.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10485.0|d:10153.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10586.0|d:10194.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10472.0|d:10307.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:10583.0|d:10211.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10556.0|d:10074.0|s:
                BeginNodeEdit|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000006|s:20|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,Preview|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:10314.0|d:10165.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:10299.0|d:10081.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None|d:10451.0|d:10146.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None|d:10435.0|d:10061.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                ";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            VisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(10399, node.X);
            Assert.AreEqual(10262.5, node.Y);
            Assert.AreEqual("Point", node.Caption);
            Assert.AreEqual(false, node.Selected);

            node = controller.GetVisualNode(0x10000002);
            Assert.AreEqual(10393, node.X);
            Assert.AreEqual(10024.5, node.Y);
            Assert.AreEqual("Point", node.Caption);
            Assert.AreEqual(false, node.Selected);

            node = controller.GetVisualNode(0x10000003);
            Assert.AreEqual(10588, node.X);
            Assert.AreEqual(10178.5, node.Y);
            Assert.AreEqual("Line", node.Caption);
            Assert.AreEqual(false, node.Selected);

            node = controller.GetVisualNode(0x10000004);
            Assert.AreEqual(10283, node.X);
            Assert.AreEqual(10069, node.Y);
            Assert.AreEqual(false, node.Selected);

            node = controller.GetVisualNode(0x10000005);
            Assert.AreEqual(10306, node.X);
            Assert.AreEqual(10269, node.Y);
            Assert.AreEqual(false, node.Selected);

            node = controller.GetVisualNode(0x10000006);
            Assert.AreEqual(10556, node.X);
            Assert.AreEqual(10074, node.Y);
            Assert.AreEqual(false, node.Selected);
        }
        [Test]
        public void Defect_IDE_1477()
        {
            string commands = @"
                CreateFunctionNode|d:15565.0|d:15285.0|s:ProtoGeometry.dll|s:Point.ByCartesianCoordinates|s:CoordinateSystem,double,double,double
                CreatePropertyNode|d:15356.0|d:15173.0|s:ProtoGeometry.dll|s:CoordinateSystem.WCS|s:
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:15368.0|d:15341.0|s:
                BeginNodeEdit|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000003|s:0|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15401.33333333|d:15207.33333333
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15558.66666667|d:15298.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15387.33333333|d:15356.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:15558.0|d:15323.33333333
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15392.66666667|d:15355.33333333
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None|d:15550.66666667|d:15344.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15398.0|d:15358.66666667
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:3|e:System.Windows.Input.ModifierKeys,None|d:15634.0|d:15356.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:3|e:System.Windows.Input.ModifierKeys,None
                ";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result); 

            IVisualNode node02 = controller.GetVisualNode(0x10000002);// Code Block node (0)

            Assert.AreEqual(3, controller.GetVisualNodes().Count); //Check if node count is 10           
            Assert.AreEqual(NodeType.Property, node02.VisualType); // Check if node is WCS property Node.

        }

        [Test]
        public void Defect_IDE_1483()
        {
            string commands = @"
                CreateFunctionNode|d:15755.0|d:15483.0|s:Experimental.dll|s:_CSVExport.ExportToCSV|s:double,string
                CreateFunctionNode|d:15398.0|d:15160.0|s:Experimental.dll|s:_RandTemp.Next|s:double,double
                CreateFunctionNode|d:15429.0|d:15254.0|s:Experimental.dll|s:LogNormalDistribution.ByParams|s:double,double
                CreateFunctionNode|d:15400.0|d:15337.0|s:Experimental.dll|s:LogNormalDistribution.LogNormalDistribution|s:
                CreateFunctionNode|d:15402.0|d:15437.0|s:Experimental.dll|s:NormalDistribution.ByParams|s:double,double
                CreateFunctionNode|d:15345.0|d:15053.0|s:Experimental.dll|s:NormalDistribution.NormalDistribution|s:
                CreateFunctionNode|d:15597.0|d:15084.0|s:Experimental.dll|s:ScreenRecorder.ScreenRecorder|s:string
                CreateFunctionNode|d:15670.0|d:15357.0|s:Experimental.dll|s:SimplexNoise.Generate|s:double,double,double
                CreateFunctionNode|d:16012.0|d:15231.0|s:Experimental.dll|s:SimplexNoise.Generate|s:double,double
                CreateFunctionNode|d:15924.0|d:15399.0|s:Experimental.dll|s:SimplexNoise.Generate|s:double
                CreateFunctionNode|d:15869.0|d:15237.0|s:Experimental.dll|s:SimplexNoise.SimplexNoise|s:";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            Assert.AreEqual(11, controller.GetVisualNodes().Count);        

        }
        
        [Test]
        public void Defect_IDE_1680()
        {
            string commands = @"
                CreateFunctionNode|d:15411.0|d:15315.0|s:ProtoGeometry.dll|s:CoordinateSystem.Identity|s:
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None|d:15404.0|d:15314.0
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None|d:15404.0|d:15314.0
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None|d:15404.0|d:15314.0
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None|d:15404.0|d:15314.0
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None|d:15404.0|d:15314.0
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None|d:15404.0|d:15314.0
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None|d:15404.0|d:15314.0
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None|d:15404.0|d:15314.0
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None|d:15404.0|d:15314.0
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None|d:15404.0|d:15314.0
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None|d:15404.0|d:15314.0
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None|d:15404.0|d:15314.0
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None|d:15404.0|d:15314.0
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None|d:15404.0|d:15314.0
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None|d:15404.0|d:15314.0
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None|d:15404.0|d:15314.0
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None|d:15404.0|d:15314.0
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None|d:15404.0|d:15314.0";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            

        }

        [Test]
        public void Defect_IDE_1668_2()
        {
            // Add a Point.ByCoordinates node
            // click on the 'DistanceTo' method and then delete the method           
            // => verifying no crash
            string commands = @"
CreateFunctionNode|d:15437.0|d:15188.5|s:ProtoGeometry.dll|s:Point.ByCoordinates|s:double,double,double
CreateRadialMenu|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast|u:0x10000001
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
SelectMenuItem|i:2001|d:15549.0|d:15184.0|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:0|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:1|e:System.Windows.Input.ModifierKeys,None
DeleteComponents
";


            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);
        }

        [Test]
        public void Defect_IDE_1668()
        {
            // Add a Point.ByCoordinates node
            // click on the 'ContextSolid' property and then delete the property
            // similarly, click on the 'X' property from the radial menu and delete the property
            // => verifying no crash 

            string commands = @"
CreateFunctionNode|d:15435.0|d:15092.0|s:ProtoGeometry.dll|s:Point.ByCoordinates|s:double,double,double
CreateRadialMenu|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast|u:0x10000001
SelectMenuItem|i:1002|d:15526.0|d:15049.0|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
DeleteComponents
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
SelectMenuItem|i:1016|d:15528.0|d:15100.0|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
DeleteComponents
";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

        }


        [Test]
        public void Defect_IDE_1685()
        {
            // create a Point.ByCoordinates(0,0,0) node 
            // click on the radial menu icon and tryp 'tra' => the Translate method comes up 
            // click on the translate method and verify the new node is created
            string commands = @"
            CreateFunctionNode|d:15417.0|d:15193.0|s:ProtoGeometry.dll|s:Point.ByCoordinates|s:double,double,double
            CreateRadialMenu|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast|u:0x10000001
            CreateSubRadialMenu|i:2021
            CreateRadialMenu|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast|u:0x10000001
            CreateSubRadialMenu|i:2021
            CreateRadialMenu|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast|u:0x10000001
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateSubRadialMenu|i:2021
            SelectMenuItem|i:4001|d:15638.90846453|d:15203.80952381|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast
            CreateRadialMenu|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast|u:0x10000001
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:0|e:System.Windows.Input.ModifierKeys,None";


            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);


            VisualNode node;

            node = (VisualNode)controller.GetVisualNode(0x10000002);            
            Assert.AreEqual("Translate", node.Caption);
           


        }

        [Test]
        public void Defect_IDE_1824()
        {
            // Steps as per defect : http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1824
            string commands = @"
CreateFunctionNode|d:15418.0|d:15225.0|s:ProtoGeometry.dll|s:Point.ByCoordinates|s:double,double,double
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15315.0|d:15305.0
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15315.0|d:15304.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateCodeBlockNode|d:15315.0|d:15304.0|s:
BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000002|s:0|b:True
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15361.0|d:15426.0
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15361.0|d:15425.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15330.0|d:15306.0
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15381.0|d:15204.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
CreateRadialMenu|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast|u:0x10000002
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15328.0|d:15305.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:15386.0|d:15225.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15331.0|d:15308.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None|d:15382.0|d:15253.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None
CreateRadialMenu|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast|u:0x10000001
CreateSubRadialMenu|i:1004
CreateSubRadialMenu|i:1004
CreateSubRadialMenu|i:2021
SelectMenuItem|i:4001|d:15627.0|d:15236.0|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast
CreateRadialMenu|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast|u:0x10000001
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:0|e:System.Windows.Input.ModifierKeys,None
";


            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);


            VisualNode node;

            node = (VisualNode)controller.GetVisualNode(0x10000003);
            Assert.AreEqual("Translate", node.Caption);
            


        }

        [Test]
        public void Defect_IDE_1772()
        {
            // 1. Create a Point.ByCoordinates(0,0,0)
            // 2. Create a CBN : 5
            // 3. Click on Point.Translate -> and Click on Point.Translate(x,y,z) from the secondary radial menu 
            // 4. connect the '5' to all it's 3 inputs => verify the translate node

            string commands = @"
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateCodeBlockNode|d:15337.0|d:15227.0|s:
BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000001|s:0|b:True
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateCodeBlockNode|d:15357.0|d:15325.0|s:
BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000002|s:5|b:True
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15462.0|d:15280.0
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15462.0|d:15279.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateFunctionNode|d:15639.0|d:15200.0|s:ProtoGeometry.dll|s:Point.ByCoordinates|s:double,double,double
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15354.0|d:15229.0
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:0|e:System.Windows.Input.ModifierKeys,None|d:15617.0|d:15185.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:0|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15353.0|d:15229.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:1|e:System.Windows.Input.ModifierKeys,None|d:15612.0|d:15201.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:1|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15352.0|d:15229.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None|d:15597.0|d:15217.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None
CreateRadialMenu|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast|u:0x10000003
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateSubRadialMenu|i:1003
CreateSubRadialMenu|i:2021
SelectMenuItem|i:4001|d:15854.0|d:15211.0|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast
CreateRadialMenu|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast|u:0x10000003
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:0|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:2|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:2|e:System.Windows.Input.ModifierKeys,None|d:15896.0|d:15255.0
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:2|e:System.Windows.Input.ModifierKeys,None|d:15610.0|d:15419.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:2|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15373.0|d:15327.0
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:15551.0|d:15399.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15370.0|d:15328.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None|d:15560.0|d:15420.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15370.0|d:15323.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:3|e:System.Windows.Input.ModifierKeys,None|d:15563.0|d:15440.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:3|e:System.Windows.Input.ModifierKeys,None
";


            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            VisualNode node;
            node = (VisualNode)controller.GetVisualNode(0x10000004);           
            Assert.AreEqual("Translate", node.Caption);
            


        }

    }
}
