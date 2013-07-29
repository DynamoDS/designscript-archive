using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;
using DesignScriptStudio.Graph.Core;

namespace DesignScriptStudio.Tests.UnitTests
{
    class UndoRedoOperationTests
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
        public void RedoCodeBlockCreation()
        {

            // Create Code Block
            // Undo craetion of Code Block (Using Ctrl+Z)
            // Redo node block creation.(Using Ctrl+Y)
            string commands = @"
                CreateCodeBlockNode|d:10330.0|d:10139.0|s:Your code goes here
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                EndNodeEdit|u:0x10000001|s:a = 10;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                UndoOperation
                RedoOperation";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

        }
        [Test]
        public void RedoIdentifierNodeCreation()
        {

            // Create IdentifierNode
            // Undo craetion of IdentifierNode (Using Ctrl+Z)
            // Redo IdentifierNode creation.(Using Ctrl+Y)
            // Currently it is crashing while doing above steps manually and defect logged for this. (http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-956)
            string commands = @"
                CreateIdentifierNode|d:647.0|d:184.0
                UndoOperation
                RedoOperation";
  
            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.NotNull(node); // Check if node is created 
            Assert.AreEqual(NodeType.Identifier, node.VisualType); // Check if node type is code block node

        }
        [Test]
        public void RedoFunctionNodeCreation()
        {

            // Create FunctionNode
            // Undo craetion of FunctionNode (Using Ctrl+Z)
            // Redo FunctionNode creation.(Using Ctrl+Y)
            string commands = @"
                CreateFunctionNode|d:450.0|d:217.0|s:|s:Range|s:double,double,double
                UndoOperation
                RedoOperation";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

        }
        [Test]
        public void RedoRepositioningOfFunctionNode()
        {
            // Create code block node just by typing some value.(say 45)
            // Create another code block node just by typing some value.(say 5)
            // Create Addition node.
            // Connect first code block node to fisrt slot of addition node.
            // Connect second code block node to second slot of addition node.
            // Now do select all by draging cursor from right bottom to left top corner for selecting all nodes.
            // Noe move them little bit.
            // Undo move.
            // Redo move.
            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:350.0|d:157.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:45|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:356.0|d:258.0|s:
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:50|b:True
                CreateFunctionNode|d:571.0|d:206.0|s:|s:+|s:double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:375.0|d:169.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:573.0|d:217.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:381.0|d:271.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:573.0|d:231.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:792.0|d:390.0
                SelectComponent|u:0x10000001|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000002|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000003|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x60000001|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x60000002|e:System.Windows.Input.ModifierKeys,None
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:327.0|d:82.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:607.0|d:223.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:797.0|d:372.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                UndoOperation
                RedoOperation";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void TestUndoRedoBehaviour()
        {
            // 1. Create a node.
            // 2. Undo.
            // 3. Create another node.
            // 4. Redo. The first node should not be repopulated back onto the canvas.
            //
            string commands = @"
                CreateDriverNode|d:396.0|d:254.0
                UndoOperation
                CreateIdentifierNode|d:489.0|d:248.0
                RedoOperation";

            GraphController graphController = new GraphController(null);
            bool result = graphController.RunCommands(commands);
            Assert.AreEqual(true, result);
            Assert.AreEqual(1, graphController.GetVisualNodes().Count);
        }

        [Test]
        public void RedoCreationAndConnectionOfTwoNodes()
        {
            // Create CodeBlockNode
            // Create IdentifierNode
            // Connect Both Nodes.
            // Undo all
            // Redo all
            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:375.0|d:219.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:100|b:True
                CreateIdentifierNode|d:573.0|d:237.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:408.0|d:232.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:576.0|d:252.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                UndoOperation
                UndoOperation
                UndoOperation
                RedoOperation
                RedoOperation
                RedoOperation";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000001);
            IVisualNode node02 = controller.GetVisualNode(0x10000002);

            ISlot outputSlot = controller.GetSlot(0x30000001); // Output 0x10000001
            Assert.IsNotNull(outputSlot);
            ISlot inputSlot = controller.GetSlot(0x30000002); // Input 0x10000002
            Assert.IsNotNull(inputSlot);
            
            // Output slot 0x30000001 should connect to input slot 0x30000003.
            uint[] connecting = outputSlot.ConnectingSlots;
            Assert.IsNotNull(connecting);
            Assert.AreEqual(0x30000002, connecting[0]);

            Assert.AreEqual(2, controller.GetVisualNodes().Count); //Check if node count is 2           
            Assert.NotNull(new object[] { node01, node02 }); // Check if node is created
            Assert.AreEqual(NodeType.CodeBlock, node01.VisualType); // Check if node type is code block node
            Assert.AreEqual(NodeType.Identifier, node02.VisualType); // Check if node type is identifier block node

        }

        [Test]
        public void RedoCreationOfSolidRevolve()
        {

            // Create Three Point Nodes
            // Create three code block node with value 0,10, and 5
            // Create One line node and one Circle Node
            // create One Revolve Node.
            // Connect all node as per inputs required for Point, Line, circle and Revolve node.
            // Do Undo All
            // Do Redo all
            string commands = @"
                CreateFunctionNode|d:349.0|d:247.0|s:ProtoGeometry.dll|s:Point.ByCoordinates|s:double,double,double
                CreateCodeBlockNode|d:185.0|d:267.0|s:Your code goes here
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                EndNodeEdit|u:0x10000002|s:0|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateFunctionNode|d:360.0|d:400.0|s:ProtoGeometry.dll|s:Point.ByCoordinates|s:double,double,double
                CreateCodeBlockNode|d:194.0|d:418.0|s:Your code goes here
                BeginNodeEdit|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text
                EndNodeEdit|u:0x10000004|s:10|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateFunctionNode|d:607.0|d:367.0|s:ProtoGeometry.dll|s:Line.ByStartPointEndPoint|s:Point,Point
                CreateFunctionNode|d:689.0|d:645.0|s:ProtoGeometry.dll|s:Circle.ByCenterPointRadius|s:Point,double
                CreateFunctionNode|d:501.0|d:655.0|s:ProtoGeometry.dll|s:Point.ByCoordinates|s:double,double,double
                CreateCodeBlockNode|d:341.0|d:665.0|s:Your code goes here
                BeginNodeEdit|u:0x10000008|e:DesignScriptStudio.Graph.Core.NodePart,Text
                EndNodeEdit|u:0x10000008|s:5|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateFunctionNode|d:986.0|d:515.0|s:ProtoGeometry.dll|s:Solid.Revolve|s:Curve,Line
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:203.0|d:279.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:352.0|d:262.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:201.0|d:278.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:354.0|d:278.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:200.0|d:278.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None|d:352.0|d:295.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:216.0|d:429.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:364.0|d:414.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:201.0|d:279.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:372.0|d:431.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:200.0|d:282.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None|d:375.0|d:446.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:471.0|d:299.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:613.0|d:382.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:483.0|d:451.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:615.0|d:397.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000008|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000008|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:358.0|d:678.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:504.0|d:670.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000008|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000008|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:358.0|d:676.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:512.0|d:689.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:201.0|d:280.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None|d:508.0|d:701.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:621.0|d:710.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:598.0|d:698.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:599.0|d:699.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:599.0|d:697.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:606.0|d:697.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:609.0|d:696.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:610.0|d:692.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:701.0|d:658.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:592.0|d:571.0|s:Your code goes here
                BeginNodeEdit|u:0x1000000a|e:DesignScriptStudio.Graph.Core.NodePart,Text
                EndNodeEdit|u:0x1000000a|s:1|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x1000000a|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x1000000a|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:611.0|d:586.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:698.0|d:678.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:815.0|d:680.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:821.0|d:680.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:826.0|d:678.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000009|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:993.0|d:543.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000009|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:731.0|d:401.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000009|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:992.0|d:530.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000009|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                UndoOperation
                UndoOperation
                UndoOperation
                UndoOperation
                UndoOperation
                UndoOperation
                UndoOperation
                UndoOperation
                UndoOperation
                UndoOperation
                UndoOperation
                UndoOperation
                UndoOperation
                UndoOperation
                UndoOperation
                UndoOperation
                UndoOperation
                UndoOperation
                UndoOperation
                UndoOperation
                UndoOperation
                UndoOperation
                UndoOperation
                UndoOperation
                UndoOperation
                UndoOperation
                UndoOperation
                UndoOperation
                UndoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000001);// Function Node (Point)
            IVisualNode node02 = controller.GetVisualNode(0x10000002);// Code Block node (0)
            IVisualNode node03 = controller.GetVisualNode(0x10000003);// Function Node (Point)
            IVisualNode node04 = controller.GetVisualNode(0x10000004);// Code Block node (10)
            IVisualNode node05 = controller.GetVisualNode(0x10000005);// Function Node (Line)
            IVisualNode node06 = controller.GetVisualNode(0x10000006);// Function Node (Circle)
            IVisualNode node07 = controller.GetVisualNode(0x10000007);// Function Node (Point)
            IVisualNode node08 = controller.GetVisualNode(0x10000008);// Code Block node (5)
            IVisualNode node09 = controller.GetVisualNode(0x10000009);// Function Node (Solid)
            IVisualNode node10 = controller.GetVisualNode(0x1000000a);// Code Block node (1)

            Assert.AreEqual(10, controller.GetVisualNodes().Count); //Check if node count is 10           
            Assert.NotNull(new object[] { node01, node02, node03, node04, node05, node06, node07, node08, node09, node10 }); // Check if all nodes are created
            Assert.AreEqual(NodeType.Function, node01.VisualType); // Check if node type Function Node Point
            Assert.AreEqual(NodeType.CodeBlock, node02.VisualType); // Check if node type is code block node
            Assert.AreEqual(NodeType.Function, node05.VisualType); // Check if node type Function Node Line
            Assert.AreEqual(NodeType.Function, node09.VisualType); // Check if node type Function Node Solid
        }

        [Test]
        public void RedoAfterCreatingNewFileCreation()
        {

            // Create FunctionNode
            // Undo craetion of FunctionNode (Using Ctrl+Z)
            // Redo FunctionNode creation.(Using Ctrl+Y)
            string commands = @"
                UndoOperation
                RedoOperation";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

        }

        [Test]
        public void RedoRepositionAndDeletionOfNodesAndConnectionLines()
        {

            // Create Min Node from Math
            // Create Two Code block node by typing any values. 
            // Connect Both code block nodes to input slots of Min node.
            // Move Min node, and also move both Code block nodes.
            // Now delete Both connection lines.(window selection)
            // Undo all
            // Redo all

            string commands = @"
                CreateFunctionNode|d:622.0|d:208.0|s:Math.dll|s:Math.Min|s:double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:372.0|d:177.0|s:
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:10|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:371.0|d:249.0|s:
                BeginNodeEdit|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000003|s:20|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:395.0|d:190.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:628.0|d:220.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:394.0|d:264.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:628.0|d:234.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None|d:387.0|d:191.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None|d:392.0|d:154.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None|d:386.0|d:265.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None|d:391.0|d:307.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:669.0|d:221.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:619.0|d:227.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:515.0|d:354.0
                SelectComponent|u:0x60000001|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x60000002|e:System.Windows.Input.ModifierKeys,None
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:495.0|d:124.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents
                UndoOperation
                UndoOperation
                UndoOperation
                UndoOperation
                UndoOperation
                UndoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000001);
            IVisualNode node02 = controller.GetVisualNode(0x10000002);
            IVisualNode node03 = controller.GetVisualNode(0x10000003);
            
            ISlot outputSlot = controller.GetSlot(0x30000004); // Output 0x10000002
            Assert.IsNotNull(outputSlot);
            ISlot inputSlot = controller.GetSlot(0x30000001); // Input 0x10000001
            Assert.IsNotNull(inputSlot);
            // Output slot 0x10000002 should connect to input slot 0x10000001.
            uint[] connecting = outputSlot.ConnectingSlots;
            Assert.IsNotNull(connecting);
            Assert.AreEqual(0x30000001, connecting[0]);
            
            ISlot outputSlot1 = controller.GetSlot(0x30000005); // Output 0x10000002
            Assert.IsNotNull(outputSlot);
            ISlot inputSlot1 = controller.GetSlot(0x30000002); // Input 0x10000001
            Assert.IsNotNull(inputSlot1);
            // Output slot 0x10000003 should connect to input slot 0x10000001.
            uint[] connecting1 = outputSlot1.ConnectingSlots;
            Assert.IsNotNull(connecting);
            Assert.AreEqual(0x30000002, connecting1[0]);

            Assert.AreEqual(3, controller.GetVisualNodes().Count); //Check if node count is 3           
            Assert.NotNull(new object[] { node01, node02, node03 }); // Check if node is created
            Assert.AreEqual(NodeType.Function, node01.VisualType); // Check if node type is Function node
            Assert.AreEqual(NodeType.CodeBlock, node02.VisualType); // Check if node type is Code block node
            Assert.AreEqual(NodeType.CodeBlock, node03.VisualType); // Check if node type is Code block node
        }

        [Test]
        public void RedoConnectionLines()
        {

            // Create one add node and two code block node.
            // Connect to code block nodes to input slots of add node.
            // Undo connection
            // Redo connection

            string commands = @"
                CreateFunctionNode|d:781.0|d:217.0|s:|s:+|s:double,double
                CreateCodeBlockNode|d:471.0|d:168.0|s:Double Click and type
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:9|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:482.0|d:274.0|s:Double Click and type
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000003|s:7|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:491.0|d:181.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:787.0|d:229.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:503.0|d:289.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:789.0|d:242.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                UndoOperation
                UndoOperation
                RedoOperation
                RedoOperation";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000001);
            IVisualNode node02 = controller.GetVisualNode(0x10000002);
            IVisualNode node03 = controller.GetVisualNode(0x10000003);

            ISlot outputSlot = controller.GetSlot(0x30000001); // Output 0x10000002
            Assert.IsNotNull(outputSlot);
            ISlot inputSlot = controller.GetSlot(0x30000004); // Input 0x10000001
            Assert.IsNotNull(inputSlot);
            // Output slot 0x10000002 should connect to input slot 0x10000001.
            uint[] connecting = outputSlot.ConnectingSlots;
            Assert.IsNotNull(connecting);
            Assert.AreEqual(0x30000004, connecting[0]);

            ISlot outputSlot1 = controller.GetSlot(0x30000005); // Output 0x10000003
            Assert.IsNotNull(outputSlot1);
            ISlot inputSlot1 = controller.GetSlot(0x30000002); // Input 0x10000001
            Assert.IsNotNull(inputSlot1);
            // Output slot 0x10000003 should connect to input slot 0x10000001.
            uint[] connecting1 = outputSlot1.ConnectingSlots;
            Assert.IsNotNull(connecting1);
            Assert.AreEqual(0x30000002, connecting1[0]);

            Assert.AreEqual(3, controller.GetVisualNodes().Count); //Check if node count is 3           
            Assert.NotNull(new object[] { node01, node02, node03 }); // Check if node is created
            Assert.AreEqual(NodeType.Function, node01.VisualType); // Check if node type is Function node
            Assert.AreEqual(NodeType.CodeBlock, node02.VisualType); // Check if node type is Code block node
            Assert.AreEqual(NodeType.CodeBlock, node02.VisualType); // Check if node type is Code block node
        }

        [Test]
        public void RedoReplaceConnectionLines()
        {

            // Create one add node and two code block node.
            // Connect two code block nodes to input slots of add node.
            // Replace connection of second node 
            // Undo connection
            // Redo connection

            string commands = @"
                CreateFunctionNode|d:644.0|d:185.0|s:|s:+|s:double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:410.0|d:191.0|s:
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:45|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:414.0|d:266.0|s:
                BeginNodeEdit|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000003|s:98|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:435.0|d:204.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:652.0|d:196.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:438.0|d:279.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:652.0|d:211.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:430.0|d:203.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:648.0|d:212.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                UndoOperation
                RedoOperation";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000001);
            IVisualNode node02 = controller.GetVisualNode(0x10000002);
            IVisualNode node03 = controller.GetVisualNode(0x10000003);

            ISlot outputSlot = controller.GetSlot(0x30000004); // Output 0x10000002
            Assert.IsNotNull(outputSlot);
            ISlot inputSlot = controller.GetSlot(0x30000001); // Input 0x10000001
            Assert.IsNotNull(inputSlot);
            // Output slot 0x10000002 should connect to input slot 0x10000001.
            uint[] connecting = outputSlot.ConnectingSlots;
            Assert.IsNotNull(connecting);
            Assert.IsTrue(connecting.Contains((uint)0x30000001));

            ISlot outputSlot1 = controller.GetSlot(0x30000004); // Output 0x10000002
            Assert.IsNotNull(outputSlot1);
            ISlot inputSlot1 = controller.GetSlot(0x30000002); // Input 0x10000001
            Assert.IsNotNull(inputSlot1);
            // Output slot 0x10000002 should connect to input slot 0x10000001.
            uint[] connecting1 = outputSlot1.ConnectingSlots;
            Assert.IsNotNull(connecting1);
            Assert.IsTrue(connecting1.Contains((uint)0x30000002));

            ISlot outputSlot2 = controller.GetSlot(0x30000005); // Output 0x10000003
            //connecting2 = outputSlot2.ConnectingSlots;
            Assert.IsNull(outputSlot2.ConnectingSlots);

            Assert.AreEqual(3, controller.GetVisualNodes().Count); //Check if node count is 3           
            Assert.IsTrue(new object[] { node01, node02, node03 }.All((object o)=>o!=null)); // Check if node is created
            Assert.AreEqual(NodeType.Function, node01.VisualType); // Check if node type is Function node
            Assert.AreEqual(NodeType.CodeBlock, node02.VisualType); // Check if node type is Code block node
            Assert.AreEqual(NodeType.CodeBlock, node02.VisualType); // Check if node type is Code block node
        }

        [Test]
        public void RedoCreationOfIdentityNode()
        {
            // Create Identity Node
            // Do Undo 
            // Do Redo
            // Doing above steps manually crashing system. Defect Logged: http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-956
            string commands = @"
                CreateFunctionNode|d:515.0|d:279.0|s:ProtoGeometry.dll|s:CoordinateSystem.Identity|s:
                UndoOperation
                RedoOperation";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

        }

        [Test]
        public void UndoRedoCreationOfNode()
        {
            // Create CodeBlock Node
            // Do Undo 
            string commands = @"
                CreateFunctionNode|d:594.0|d:306.0|s:|s:+|s:double,double
                UndoOperation";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.IsTrue(result);

            Assert.AreEqual(0, controller.GetVisualNodes().Count); //Check there should not be any node.  
        }

        [Test]
        public void UndoRedoCreationOfAllTypeOfNodes()
        {
            // Create CodeBlock Node
            // Do Undo 
            string commands = @"
                CreateCodeBlockNode|d:425.0|d:94.0|s:Your code goes here
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                EndNodeEdit|u:0x10000001|s:a = 10;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateDriverNode|d:669.0|d:114.0
                CreateIdentifierNode|d:708.0|d:199.0
                CreateFunctionNode|d:329.0|d:172.0|s:|s:Range|s:double,double,double
                CreateFunctionNode|d:770.0|d:304.0|s:|s:+|s:double,double|s:value,value
                CreateFunctionNode|d:501.0|d:328.0|s:ProtoGeometry.dll|s:Solid.Revolve|s:Curve,Point,Vector,double,double
                CreateFunctionNode|d:334.0|d:354.0|s:ProtoGeometry.dll|s:Circle.ByCenterPointRadius|s:Point,double
                CreateFunctionNode|d:162.0|d:42.0|s:ProtoGeometry.dll|s:Block.Insert|s:CoordinateSystem,string,string
                CreateFunctionNode|d:736.0|d:426.0|s:Math.dll|s:Math.Max|s:double,double";
            
            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.IsTrue(result);

            IVisualNode node01 = controller.GetVisualNode(0x10000001);
            IVisualNode node02 = controller.GetVisualNode(0x10000002);
            IVisualNode node03 = controller.GetVisualNode(0x10000003);
            IVisualNode node04 = controller.GetVisualNode(0x10000004);
            IVisualNode node05 = controller.GetVisualNode(0x10000005);
            IVisualNode node06 = controller.GetVisualNode(0x10000006);
            IVisualNode node07 = controller.GetVisualNode(0x10000007);
            IVisualNode node08 = controller.GetVisualNode(0x10000008);
            IVisualNode node09 = controller.GetVisualNode(0x10000009);
            
            commands = @"
                UndoOperation
                UndoOperation
                UndoOperation
                UndoOperation
                UndoOperation
                UndoOperation
                UndoOperation
                UndoOperation
                UndoOperation";

            bool result01 = controller.RunCommands(commands);
            Assert.IsTrue(result01);

            Assert.AreEqual(0, controller.GetVisualNodes().Count); //Check there should not be any node.  

            commands = @"
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation";

            bool result02 = controller.RunCommands(commands);
            Assert.IsTrue(result02);

            Assert.AreEqual(9, controller.GetVisualNodes().Count); //Check there should be 9 nodes.
            Assert.AreEqual(NodeType.CodeBlock, node01.VisualType); // Check if node type is CodeBlock node
            Assert.AreEqual(NodeType.Driver, node02.VisualType); // Check if node type is Driver node
            Assert.AreEqual(NodeType.Identifier, node03.VisualType); // Check if node type is Identifier node
            Assert.AreEqual(NodeType.Function, node04.VisualType); // Check if node type is Function node
            Assert.AreEqual(NodeType.Function, node05.VisualType); // Check if node type is Function node
            Assert.AreEqual(NodeType.Function, node06.VisualType); // Check if node type is Function node
            Assert.AreEqual(NodeType.Function, node07.VisualType); // Check if node type is Function node
            Assert.AreEqual(NodeType.Function, node08.VisualType); // Check if node type is Function node
            Assert.AreEqual(NodeType.Function, node09.VisualType); // Check if node type is Function node
        }

        [Test]
        public void UndoRedoMove_TestMovingOfSingleNode()
        {
            // Create CodeBlock Node
            // Move it to some other position.
            // Do Undo 
            // Do Redo
            string commands = @"
                CreateCodeBlockNode|d:654.0|d:169.0|s:Your code goes here
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                EndNodeEdit|u:0x10000001|s:a = 10;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None|d:706.0|d:178.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None|d:503.0|d:316.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.IsTrue(result);

            IVisualNode node = controller.GetVisualNodes().ElementAt(0);
            double X = node.X;
            double Y = node.Y;

            commands = @"UndoOperation";
            bool result01 = controller.RunCommands(commands);
            Assert.IsTrue(result01);
            Assert.AreEqual(654.0, node.X);
            Assert.AreEqual(169.0, node.Y);

            commands = @"RedoOperation";
            bool result02 = controller.RunCommands(commands);
            Assert.IsTrue(result02);
            Assert.AreEqual(X, node.X);
            Assert.AreEqual(Y, node.Y);

        }

        [Test]
        public void UndoRedoMove_TestMovingOfIdentifierNodeMutipleLocation()
        {
            // Create Identifier Node
            // Move it to new position 3 time.
            // Do Undo 3 time.
            string commands = @"
                CreateIdentifierNode|d:295.0|d:72.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None|d:314.0|d:82.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None|d:689.0|d:148.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node = controller.GetVisualNodes().ElementAt(0);
            double X1 = node.X;
            double Y1 = node.Y;

            commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None|d:688.0|d:153.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None|d:463.0|d:321.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None";
            
            bool result01 = controller.RunCommands(commands);
            Assert.AreEqual(true, result01);
            
            double X2 = node.X;
            double Y2 = node.Y;

            commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None|d:464.0|d:322.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None|d:803.0|d:359.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                UndoOperation";

            bool result02 = controller.RunCommands(commands);
            Assert.AreEqual(true, result02);

            Assert.AreEqual(X2, node.X);
            Assert.AreEqual(Y2, node.Y);

            commands = @"UndoOperation";
            bool result03 = controller.RunCommands(commands);
            Assert.AreEqual(true, result03);

            Assert.AreEqual(X1, node.X);
            Assert.AreEqual(Y1, node.Y);

            commands = @"UndoOperation";
            bool result04 = controller.RunCommands(commands);
            Assert.AreEqual(true, result04);

            Assert.AreEqual(295.0, node.X);
            Assert.AreEqual(72.0, node.Y);
        }
        [Test]
        public void UndoRedoMove_TestMovingOfCodeBlockNodeWhenItIsConnectedToAdd()
        {
            // Create Two CodeBlock Node
            // Create One Add node
            // Connect Both Code Block node to Add node
            // Now move first Code Block node to new position
            // Do Undo 
            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:426.0|d:164.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:5|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:428.0|d:254.0|s:
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:45|b:True
                CreateFunctionNode|d:669.0|d:226.0|s:|s:+|s:double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:445.0|d:178.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:675.0|d:238.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:452.0|d:266.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:672.0|d:251.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None|d:438.0|d:175.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None|d:618.0|d:106.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node = controller.GetVisualNodes().ElementAt(0);
            double X = node.X;
            double Y = node.Y;

            commands = @"UndoOperation";
            bool result02 = controller.RunCommands(commands);
            Assert.AreEqual(true, result02);
            Assert.AreEqual(426.0, node.X);
            Assert.AreEqual(164.0, node.Y);
        }

        [Test]
        public void UndoRedoMove_TestUndoInBetweenCreateAndDeleteOperation()
        {
            // Create Coed block Node AND EDIT IT
            // Create Identifier node
            // Delete Identifier node.
            // Create Add node.
            // Do Undo twice

            string commands = @"
                CreateCodeBlockNode|d:373.0|d:149.0|s:Double Click and type
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:56565665|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateIdentifierNode|d:504.0|d:212.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents
                CreateFunctionNode|d:379.0|d:249.0|s:|s:Add|s:double,double";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000001);
            //IVisualNode node02 = controller.GetVisualNode(0x10000002);
            IVisualNode node03 = controller.GetVisualNode(0x10000003);

            Assert.AreEqual(2, controller.GetVisualNodes().Count);
            //Assert.AreEqual(NodeType.Identifier, node02.VisualType);
            Assert.AreEqual(NodeType.Function, node03.VisualType);

            commands = @"UndoOperation";
            bool result03 = controller.RunCommands(commands);
            Assert.AreEqual(true, result03);

            Assert.AreEqual(1, controller.GetVisualNodes().Count);
            Assert.AreEqual(NodeType.CodeBlock, node01.VisualType);
            
            commands = @"UndoOperation";
            bool result04 = controller.RunCommands(commands);
            Assert.AreEqual(true, result04);
            IVisualNode node02 = controller.GetVisualNode(0x10000002);

            Assert.AreEqual(2, controller.GetVisualNodes().Count);
            Assert.AreEqual(NodeType.CodeBlock, node01.VisualType); // Check if node type is Code block node
            Assert.AreEqual(NodeType.Identifier, node02.VisualType); // Check if node type is Code block node
        }

        [Test]
        public void UndoRedoMove_TestMovingOfMultipleNodesWhenTheyAreConnected()
        {
            // Create Identifier Node
            // Move it to new position 3 time.
            // Do Undo 3 time.
            string commands = @"
                CreateFunctionNode|d:366.0|d:76.0|s:ProtoGeometry.dll|s:Point.ByCoordinates|s:double,double,double
                CreateFunctionNode|d:374.0|d:200.0|s:ProtoGeometry.dll|s:Point.ByCoordinates|s:double,double,double
                CreateFunctionNode|d:399.0|d:331.0|s:ProtoGeometry.dll|s:Point.ByCoordinates|s:double,double,double
                CreateFunctionNode|d:614.0|d:304.0|s:ProtoGeometry.dll|s:Line.ByStartPointEndPoint|s:Point,Point
                CreateFunctionNode|d:652.0|d:117.0|s:ProtoGeometry.dll|s:Circle.ByCenterPointRadius|s:Point,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:262.0|d:84.0|s:
                BeginNodeEdit|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000006|s:5|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:293.0|d:187.0|s:
                BeginNodeEdit|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000007|s:0|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:306.0|d:315.0|s:
                BeginNodeEdit|u:0x10000008|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000008|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000008|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000008|s:10|b:True
                CreateFunctionNode|d:1030.0|d:231.0|s:ProtoGeometry.dll|s:Surface.Revolve|s:Curve,Line
                UndoOperation
                CreateFunctionNode|d:1012.0|d:233.0|s:ProtoGeometry.dll|s:Solid.Revolve|s:Curve,Line
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:361.0|d:121.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:478.0|d:101.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:360.0|d:121.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:480.0|d:117.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:357.0|d:120.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None|d:481.0|d:133.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:356.0|d:121.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:489.0|d:232.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:357.0|d:119.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None|d:488.0|d:247.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:357.0|d:121.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None|d:495.0|d:366.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000008|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000008|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:354.0|d:212.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:487.0|d:215.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000008|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000008|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:356.0|d:211.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:495.0|d:331.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000008|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000008|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:358.0|d:216.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:496.0|d:351.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:603.0|d:137.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:731.0|d:157.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:611.0|d:250.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:728.0|d:171.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:612.0|d:372.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:606.0|d:370.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:614.0|d:367.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:735.0|d:302.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:685.0|d:315.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:735.0|d:321.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:849.0|d:174.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x1000000a|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:1021.0|d:246.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x1000000a|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:864.0|d:323.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x1000000a|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:1017.0|d:261.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x1000000a|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None|d:674.0|d:298.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None|d:693.0|d:246.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000004);// Line
            IVisualNode node02 = controller.GetVisualNode(0x10000005);// Circle
            IVisualNode node03 = controller.GetVisualNode(0x10000006);// CodeBlock

            IVisualNode node = controller.GetVisualNodes().ElementAt(3);
            double X1 = node03.X;
            double Y1 = node03.Y;

            commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:774.0|d:167.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:816.0|d:123.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None";

            bool result01 = controller.RunCommands(commands);
            Assert.AreEqual(true, result01);

            double X2 = node01.X;
            double Y2 = node01.Y;

            commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:822.0|d:307.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:898.0|d:352.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                UndoOperation";

            bool result02 = controller.RunCommands(commands);
            Assert.AreEqual(true, result02);

            Assert.AreEqual(652.0, node02.X);
            Assert.AreEqual(117, node02.Y);

            commands = @"UndoOperation";
            bool result03 = controller.RunCommands(commands);
            Assert.AreEqual(true, result03);

            Assert.AreEqual(614.0, node01.X);
            Assert.AreEqual(304.0, node01.Y);

            commands = @"UndoOperation";
            bool result04 = controller.RunCommands(commands);
            Assert.AreEqual(true, result04);

            Assert.AreEqual(262.0, node03.X);
            Assert.AreEqual(84.0, node03.Y);
            Assert.AreEqual(9, controller.GetVisualNodes().Count);

            commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,Control
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,Control
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,Control
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,Control
                DeleteComponents";

            bool result05 = controller.RunCommands(commands);
            Assert.AreEqual(true, result05);

            Assert.AreEqual(6, controller.GetVisualNodes().Count);

            commands = @"UndoOperation";
            bool result06 = controller.RunCommands(commands);
            Assert.AreEqual(true, result06);
            Assert.AreEqual(9, controller.GetVisualNodes().Count);

        }
        [Test]
        public void UndoRedoMove_TestMovingOfAllTypesOfNodesUsingWindowSelection()
        {
            // Create Code Blok, Identifier, Add, Dirver and two revolve Node (having 4 and 3 slots)
            // Now select all nodes using window selection.
            // Now move all nodes to new position
            // Do Undo 
            string commands = @"
                CreateCodeBlockNode|d:368.0|d:124.0|s:Your code goes here
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                EndNodeEdit|u:0x10000001|s:a = 10;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateDriverNode|d:574.0|d:133.0
                CreateIdentifierNode|d:678.0|d:231.0
                CreateFunctionNode|d:401.0|d:223.0|s:|s:+|s:double,double|s:value,value
                CreateFunctionNode|d:660.0|d:373.0|s:ProtoGeometry.dll|s:Solid.Revolve|s:Curve,Point,Vector,double,double
                CreateFunctionNode|d:754.0|d:158.0|s:ProtoGeometry.dll|s:Solid.Revolve|s:Curve,Point,Vector
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:947.0|d:516.0
                SelectComponent|u:0x10000001|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000002|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000003|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000004|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000005|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000006|e:System.Windows.Input.ModifierKeys,None
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:337.0|d:82.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:502.0|d:246.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:423.0|d:438.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000001);// Line
            IVisualNode node02 = controller.GetVisualNode(0x10000002);// Circle
            IVisualNode node03 = controller.GetVisualNode(0x10000003);// CodeBlock
            IVisualNode node04 = controller.GetVisualNode(0x10000004);// Line
            IVisualNode node05 = controller.GetVisualNode(0x10000005);// Circle
            IVisualNode node06 = controller.GetVisualNode(0x10000006);// CodeBlock

            double X = node01.X;
            double Y = node01.Y;
            double X1 = node02.X;
            double Y1 = node02.Y;
            double X2 = node03.X;
            double Y2 = node03.Y;
            double X3 = node04.X;
            double Y3 = node04.Y;
            double X4 = node05.X;
            double Y4 = node05.Y;
            double X5 = node06.X;
            double Y5 = node06.Y;

            commands = @"UndoOperation";
            bool result02 = controller.RunCommands(commands);
            Assert.AreEqual(true, result02);
            
            Assert.AreEqual(368.0, node01.X);
            Assert.AreEqual(124.0, node01.Y);

            Assert.AreEqual(574.0, node02.X);
            Assert.AreEqual(133.0, node02.Y);

            Assert.AreEqual(678.0, node03.X);
            Assert.AreEqual(231.0, node03.Y);

            Assert.AreEqual(401.0, node04.X);
            Assert.AreEqual(223.0, node04.Y);

            Assert.AreEqual(660.0, node05.X);
            Assert.AreEqual(373.0, node05.Y);

            Assert.AreEqual(754.0, node06.X);
            Assert.AreEqual(158.0, node06.Y);

            Assert.AreEqual(6, controller.GetVisualNodes().Count);
        }
        [Test]
        public void UndoRedoMove_TestMovingOfTwoNodesBySelectingThemUsingCtrlKey()
        {
            // Create Two Code block, Identifier and Add nodes.
            // Now select two nodes using ctrl key.
            // Now move them to new position
            // Do Undo 
            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10342.0|d:10133.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:a = 1;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateIdentifierNode|d:10760.0|d:10186.0
                CreateFunctionNode|d:10554.0|d:10194.0|s:|s:+|s:double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10363.0|d:10272.0|s:
                BeginNodeEdit|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000004|s:56|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10388.0|d:10148.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10551.0|d:10204.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10391.0|d:10289.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:10551.0|d:10231.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10639.0|d:10228.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10756.0|d:10204.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,Control
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,Control
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:0|e:System.Windows.Input.ModifierKeys,None|d:10594.0|d:10212.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:0|e:System.Windows.Input.ModifierKeys,None|d:10598.0|d:10075.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:0|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000001);// Code Block
            IVisualNode node02 = controller.GetVisualNode(0x10000002);// Identifier
            IVisualNode node03 = controller.GetVisualNode(0x10000003);// Function
            IVisualNode node04 = controller.GetVisualNode(0x10000004);// Code Block

            double X = node01.X;
            double Y = node01.Y;
            double X1 = node03.X;
            double Y1 = node03.Y;

            commands = @"UndoOperation";
            bool result02 = controller.RunCommands(commands);
            Assert.AreEqual(true, result02);

            Assert.AreEqual(10342.0, node01.X);
            Assert.AreEqual(10133.0, node01.Y);

            Assert.AreEqual(10554.0, node03.X);
            Assert.AreEqual(10194.0, node03.Y);

            Assert.AreEqual(4, controller.GetVisualNodes().Count);
        }
        
        [Test]
        public void UndoRedoMove_TestCreateNodeUndoItAgainCreateNewAgainUndo()
        {
            // Create Code Blok, and Add nodes.
            // Do undo twice.
            // Create Identifier and add node.
            // Again do undo 3 times.

            string commands = @"
                CreateCodeBlockNode|d:393.0|d:158.0|s:Your code goes here
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                EndNodeEdit|u:0x10000001|s:a = 10;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateFunctionNode|d:685.0|d:109.0|s:|s:+|s:double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:a=1;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                UndoOperation
                UndoOperation
                UndoOperation
                CreateIdentifierNode|d:504.0|d:206.0
                CreateFunctionNode|d:700.0|d:232.0|s:|s:+|s:double,double
                UndoOperation
                UndoOperation
                UndoOperation
                UndoOperation";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            Assert.AreEqual(0, controller.GetVisualNodes().Count);
        }
        
        [Test]
        public void UndoRedoMove_TestRedoOfSingleNodeMove()
        {
            // Create Add Node and move it to new position.
            // Do undo.
            // Do Redo

            string commands = @"
                CreateFunctionNode|d:562.0|d:180.0|s:|s:+|s:double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:654.0|d:195.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:483.0|d:434.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None";
                
            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000001);// Add Node

            double X = node01.X;
            double Y = node01.Y;

            commands =@"                
                UndoOperation
                RedoOperation";

            bool result01 = controller.RunCommands(commands);
            Assert.AreEqual(true, result01);

            Assert.AreEqual(X, node01.X);
            Assert.AreEqual(Y, node01.Y);

            Assert.AreEqual(1, controller.GetVisualNodes().Count);
        }

        [Test]
        public void UndoRedoMove_DriverNodeUndoRedo()
        {
            // Create Driver Node.
            // Do undo.
            // Do Redo.

            string commands = @"
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateDriverNode|d:10832.0|d:10397.0";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000001);// codeBlock node Node created using directly typing in the canvas

            Assert.AreEqual(NodeType.Driver, node01.VisualType); // Check if node type is CodeBlock node
            Assert.AreEqual(1, controller.GetVisualNodes().Count);

            commands = @"
                UndoOperation";

            bool result01 = controller.RunCommands(commands);
            Assert.AreEqual(true, result01);

            Assert.AreEqual(0, controller.GetVisualNodes().Count);

            commands = @"
                RedoOperation";

            bool result02 = controller.RunCommands(commands);
            Assert.AreEqual(true, result02);

            Assert.AreEqual(NodeType.Driver, node01.VisualType); // Check if node type is CodeBlock node
            Assert.AreEqual(1, controller.GetVisualNodes().Count);

        }

        [Test]
        public void UndoRedoMove_TestRedoOfDriverNodeMove()
        {
            // Create Add Node and move it to new position.
            // Do undo.
            // Do Redo

            string commands = @"
                CreateDriverNode|d:10633.0|d:10295.0";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000001);// Add Node

            Assert.AreEqual(NodeType.Driver, node01.VisualType); // Check if node type is CodeBlock node

            double X = node01.X;
            double Y = node01.Y;

            commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None|d:10688.0|d:10306.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None|d:10888.0|d:10570.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None";

            bool result01 = controller.RunCommands(commands);
            Assert.AreEqual(true, result01);

            Assert.AreEqual(NodeType.Driver, node01.VisualType); // Check if node type is Driver node

            double X1 = node01.X;
            double Y1 = node01.Y;

            commands = @"                
                UndoOperation";

            bool result02 = controller.RunCommands(commands);
            Assert.AreEqual(true, result02);

            Assert.AreEqual(NodeType.Driver, node01.VisualType); // Check if node type is Driver node
            Assert.AreEqual(X, node01.X);
            Assert.AreEqual(Y, node01.Y);

            commands = @"                
                RedoOperation";

            bool result03 = controller.RunCommands(commands);
            Assert.AreEqual(true, result03);

            Assert.AreEqual(NodeType.Driver, node01.VisualType); // Check if node type is Driver node
            Assert.AreEqual(X1, node01.X);
            Assert.AreEqual(Y1, node01.Y);            
            Assert.AreEqual(1, controller.GetVisualNodes().Count);
        }

        [Test]
        public void UndoRedoMove_TestRedoOfNodeMoveMaintainConnections()
        {
            // Create Two CodeBlock Nodes and one Add Node. 
            // Connect both Code Block nodes to Add node.
            // Move both Clode Block nodes to new position one by one.
            // Do undo twice.
            // Do Redo twice.

            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:369.0|d:166.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:50|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:352.0|d:251.0|s:
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:69|b:True
                CreateFunctionNode|d:519.0|d:226.0|s:|s:+|s:double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:396.0|d:182.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:526.0|d:238.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:377.0|d:262.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:525.0|d:252.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None|d:385.0|d:176.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None|d:553.0|d:90.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None";
                
            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000001);// CodeBlock Node
            IVisualNode node02 = controller.GetVisualNode(0x10000002);// CodeBlock Node
            IVisualNode node03 = controller.GetVisualNode(0x10000003);// Add Node

            double X = node01.X;
            double Y = node01.Y;

            commands = @"  
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None|d:366.0|d:265.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None|d:473.0|d:397.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                UndoOperation
                UndoOperation
                RedoOperation
                RedoOperation";
            
            Assert.AreEqual(X, node01.X);
            Assert.AreEqual(Y, node01.Y);

            ISlot outputSlot = controller.GetSlot(0x30000001); // Output of first CodeBlock ndoe.
            Assert.IsNotNull(outputSlot);
            ISlot inputSlot = controller.GetSlot(0x30000003); // First Input of Add Node.
            Assert.IsNotNull(inputSlot);
            // Output slot CoedBlock Node should connect to input slot Add node.
            uint[] connecting = outputSlot.ConnectingSlots;
            Assert.IsNotNull(connecting);
            Assert.IsTrue(connecting.Contains((uint)0x30000003));

            Assert.AreEqual(3, controller.GetVisualNodes().Count);
        }
        
        [Test]
        public void UndoRedoMove_TestRedoOfGeometryNodesWhenTheyAreConnectedToEachOther()
        {
            // Create Geometry Workflow to create lineusing two input points.
            // After joining all relevant connections select all of them using window selection from right bottom to left top.
            // Move them to new position.
            // Do undo.
            // Do Redo

            string commands = @"
                CreateFunctionNode|d:514.0|d:167.0|s:ProtoGeometry.dll|s:Point.ByCoordinates|s:double,double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:347.0|d:147.0|s:
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:0|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:347.0|d:233.0|s:
                BeginNodeEdit|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000003|s:10|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:364.0|d:161.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:516.0|d:181.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:364.0|d:158.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:516.0|d:195.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:362.0|d:160.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None|d:518.0|d:210.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None
                CreateFunctionNode|d:760.0|d:287.0|s:ProtoGeometry.dll|s:Line.ByStartPointEndPoint|s:Point,Point
                CreateFunctionNode|d:526.0|d:311.0|s:ProtoGeometry.dll|s:Point.ByCoordinates|s:double,double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:370.0|d:244.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:530.0|d:327.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:364.0|d:160.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:529.0|d:339.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:363.0|d:161.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None|d:530.0|d:353.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:638.0|d:214.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:766.0|d:300.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:649.0|d:356.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:769.0|d:313.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:927.0|d:441.0
                SelectComponent|u:0x10000001|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000002|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000003|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000004|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000005|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x60000001|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x60000002|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x60000003|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x60000004|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x60000005|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x60000006|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x60000007|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x60000008|e:System.Windows.Input.ModifierKeys,None
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:317.0|d:91.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:578.0|d:192.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:878.0|d:84.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000001);// Point Node
            IVisualNode node02 = controller.GetVisualNode(0x10000002);// CodeBlock
            IVisualNode node03 = controller.GetVisualNode(0x10000003);// CodeBlock
            IVisualNode node04 = controller.GetVisualNode(0x10000004);// Line Node
            IVisualNode node05 = controller.GetVisualNode(0x10000005);// Point Node

            double X = node01.X;
            double Y = node01.Y;
            double X1 = node02.X;
            double Y1 = node02.Y;
            double X2 = node03.X;
            double Y2 = node03.Y;
            double X3 = node04.X;
            double Y3 = node04.Y;
            double X4 = node05.X;
            double Y4 = node05.Y;

            commands = @"  
                UndoOperation
                RedoOperation";

            Assert.AreEqual(X, node01.X);
            Assert.AreEqual(Y, node01.Y);
            Assert.AreEqual(X1, node02.X);
            Assert.AreEqual(Y1, node02.Y);
            Assert.AreEqual(X2, node03.X);
            Assert.AreEqual(Y2, node03.Y);
            Assert.AreEqual(X3, node04.X);
            Assert.AreEqual(Y3, node04.Y);
            Assert.AreEqual(X4, node05.X);
            Assert.AreEqual(Y4, node05.Y);

            ISlot outputSlot = controller.GetSlot(0x30000005); // Output of first CodeBlock ndoe.
            Assert.IsNotNull(outputSlot);
            ISlot inputSlot = controller.GetSlot(0x30000003); // Input of Point Node.
            Assert.IsNotNull(inputSlot);
            // Output slot CoedBlock Node should connect to input slot Add node.
            uint[] connecting = outputSlot.ConnectingSlots;
            Assert.IsNotNull(connecting);
            Assert.IsTrue(connecting.Contains((uint)0x30000003));

            Assert.AreEqual("0;", ((CodeBlockNode)node02).Text); // Check if text is 0
            Assert.AreEqual("10;", ((CodeBlockNode)node03).Text);// Check if text is 10

            Assert.AreEqual(5, controller.GetVisualNodes().Count);
        }

        [Test]
        public void UndoRedoMove_TestNodeDeletionAfterMoveAndThenRedoEverything()
        {
            // Create two code block nodes.
            // Create one Add node.
            // Connect both CodeBlock nodes to Add node.
            // Move second CodeBlock node to new position.
            // Delete moved node.
            // Do undo.
            // Do Undo.
            // Do Redo.
            // Do Redo.

            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:373.0|d:138.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:97|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:409.0|d:286.0|s:
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:61|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateFunctionNode|d:633.0|d:205.0|s:|s:Add|s:double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:398.0|d:150.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:635.0|d:218.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:436.0|d:299.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:638.0|d:231.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None|d:426.0|d:300.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None|d:544.0|d:463.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000001);// CodeBlock Node
            IVisualNode node02 = controller.GetVisualNode(0x10000002);// CodeBlock Node
            IVisualNode node03 = controller.GetVisualNode(0x10000003);// Max Node

            double X1 = node02.X;
            double Y1 = node02.Y;

            commands = @"
                DeleteComponents
                UndoOperation  
                UndoOperation
                RedoOperation";
            bool result02 = controller.RunCommands(commands);
            Assert.AreEqual(true, result02);

            Assert.AreEqual(X1, node02.X);
            Assert.AreEqual(Y1, node02.Y);

            ISlot outputSlot = controller.GetSlot(0x30000002); // Output of first CodeBlock ndoe.
            Assert.IsNotNull(outputSlot);
            ISlot inputSlot = controller.GetSlot(0x30000004); // First Input of Add Node.
            Assert.IsNotNull(inputSlot);
            // Output slot CoedBlock Node should connect to input slot Max node.
            uint[] connecting = outputSlot.ConnectingSlots;
            Assert.IsNotNull(connecting);
            Assert.IsTrue(connecting.Contains((uint)0x30000004));
            Assert.AreEqual("97;", ((CodeBlockNode)node01).Text); // Check if text is 97
            Assert.AreEqual("61;", ((CodeBlockNode)node02).Text);// Check if text is 61
            Assert.AreEqual(3, controller.GetVisualNodes().Count);

            commands = @"RedoOperation";
            bool result03 = controller.RunCommands(commands);
            Assert.AreEqual(true, result03);

            Assert.AreEqual(2, controller.GetVisualNodes().Count);
        }

        [Test]
        public void UndoRedoMove_TestRedoOfAllNodesBySelectingThemUsingWindowSelectionGeometryWorkflow()
        {
            // Create all Required Nodes to craete Sweep Surface using Profile,Path overload.
            // Select All ndoes using window selection and move them to new position.
            // Do undo.
            // Do Redo.

            string commands = @"
                CreateFunctionNode|d:884.0|d:385.0|s:ProtoGeometry.dll|s:Surface.Sweep|s:Curve,Curv
                CreateFunctionNode|d:624.0|d:331.0|s:ProtoGeometry.dll|s:Circle.ByCenterPointRadius|s:Point,double
                CreateFunctionNode|d:616.0|d:515.0|s:ProtoGeometry.dll|s:Line.ByStartPointEndPoint|s:Point,Point
                CreateFunctionNode|d:329.0|d:291.0|s:ProtoGeometry.dll|s:Point.ByCoordinates|s:double,double,double
                CreateFunctionNode|d:336.0|d:500.0|s:ProtoGeometry.dll|s:Point.ByCoordinates|s:double,double,double
                CreateCodeBlockNode|d:171.0|d:330.0|s:Double Click and type
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000006|s:0|b:True
                CreateCodeBlockNode|d:193.0|d:394.0|s:Double Click and type
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000007|s:10|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:193.0|d:346.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:333.0|d:307.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:187.0|d:343.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:332.0|d:323.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:189.0|d:346.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None|d:332.0|d:337.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:216.0|d:407.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:339.0|d:516.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:215.0|d:406.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:216.0|d:409.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000007|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:188.0|d:347.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:338.0|d:533.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:188.0|d:347.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None|d:338.0|d:547.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:451.0|d:340.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:627.0|d:345.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:460.0|d:548.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:621.0|d:546.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:452.0|d:343.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:618.0|d:532.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:541.0|d:389.0|s:Double Click and type
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000008|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000008|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000008|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000008|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000008|s:1|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000008|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000008|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:557.0|d:407.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:628.0|d:361.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:752.0|d:360.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:888.0|d:399.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:740.0|d:546.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:887.0|d:415.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:104.0|d:215.0
                SelectComponent|u:0x10000001|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000002|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000003|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000004|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000005|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000006|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000007|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000008|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x60000001|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x60000002|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x60000003|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x60000004|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x60000005|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x60000006|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x60000007|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x60000008|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x60000009|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x6000000a|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x6000000b|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x6000000c|e:System.Windows.Input.ModifierKeys,None
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:965.0|d:681.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:394.0|d:328.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:1045.0|d:453.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000001);// Surface
            IVisualNode node02 = controller.GetVisualNode(0x10000002);// Circle
            IVisualNode node03 = controller.GetVisualNode(0x10000003);// Line
            IVisualNode node04 = controller.GetVisualNode(0x10000004);// Point
            IVisualNode node05 = controller.GetVisualNode(0x10000005);// Point
            IVisualNode node06 = controller.GetVisualNode(0x10000006);// CodeBlock (0)
            IVisualNode node07 = controller.GetVisualNode(0x10000007);// CodeBlock (10)
            IVisualNode node08 = controller.GetVisualNode(0x10000008);// CodeBlock (1)

            double X = node01.X;
            double Y = node01.Y;
            double X1 = node02.X;
            double Y1 = node02.Y;
            double X2 = node03.X;
            double Y2 = node03.Y;
            double X3 = node04.X;
            double Y3 = node04.Y;
            double X4 = node05.X;
            double Y4 = node05.Y;
            double X5 = node06.X;
            double Y5 = node06.Y;
            double X6 = node07.X;
            double Y6 = node07.Y;
            double X7 = node08.X;
            double Y7 = node08.Y;

            commands = @"
                UndoOperation
                RedoOperation";

            bool result02 = controller.RunCommands(commands);
            Assert.AreEqual(true, result02);

            Assert.AreEqual(X, node01.X);
            Assert.AreEqual(Y, node01.Y);
            Assert.AreEqual(X1, node02.X);
            Assert.AreEqual(Y1, node02.Y);
            Assert.AreEqual(X2, node03.X);
            Assert.AreEqual(Y2, node03.Y);
            Assert.AreEqual(X3, node04.X);
            Assert.AreEqual(Y3, node04.Y);
            Assert.AreEqual(X4, node05.X);
            Assert.AreEqual(Y4, node05.Y);
            Assert.AreEqual(X5, node06.X);
            Assert.AreEqual(Y5, node06.Y);
            Assert.AreEqual(X6, node07.X);
            Assert.AreEqual(Y6, node07.Y);
            Assert.AreEqual(X7, node08.X);
            Assert.AreEqual(Y7, node08.Y);

            Assert.AreEqual("0;", ((CodeBlockNode)node06).Text); // Check if text is 0
            Assert.AreEqual("10;", ((CodeBlockNode)node07).Text);// Check if text is 10
            Assert.AreEqual("1;", ((CodeBlockNode)node08).Text);// Check if text is 1
            Assert.AreEqual(8, controller.GetVisualNodes().Count);
        }

        [Ignore]
        public void UndoRedoMove_TestFunctionNodeWithNoInputSolt()
        {
            // Create GeometrySettings, Reset and Identity nodes.
            // Do undo.
            // Do Redo

            string commands = @"MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000001);// Point Node
            IVisualNode node02 = controller.GetVisualNode(0x10000002);// CodeBlock
            IVisualNode node03 = controller.GetVisualNode(0x10000003);// CodeBlock
            IVisualNode node04 = controller.GetVisualNode(0x10000004);// LIne Node
            IVisualNode node05 = controller.GetVisualNode(0x10000005);// Point Node

            double X = node01.X;
            double Y = node01.Y;
            double X1 = node02.X;
            double Y1 = node02.Y;
            double X2 = node03.X;
            double Y2 = node03.Y;
            double X3 = node04.X;
            double Y3 = node04.Y;
            double X4 = node05.X;
            double Y4 = node05.Y;

            commands = @"  
                UndoOperation
                RedoOperation";

            Assert.AreEqual(X, node01.X);
            Assert.AreEqual(Y, node01.Y);
            Assert.AreEqual(X1, node02.X);
            Assert.AreEqual(Y1, node02.Y);
            Assert.AreEqual(X2, node03.X);
            Assert.AreEqual(Y2, node03.Y);
            Assert.AreEqual(X3, node04.X);
            Assert.AreEqual(Y3, node04.Y);
            Assert.AreEqual(X4, node05.X);
            Assert.AreEqual(Y4, node05.Y);

            ISlot outputSlot = controller.GetSlot(0x30000005); // Output of first CodeBlock ndoe.
            Assert.IsNotNull(outputSlot);
            ISlot inputSlot = controller.GetSlot(0x30000003); // First Input of Add Node.
            Assert.IsNotNull(inputSlot);
            // Output slot CoedBlock Node should connect to input slot Add node.
            uint[] connecting = outputSlot.ConnectingSlots;
            Assert.IsNotNull(connecting);
            Assert.IsTrue(connecting.Contains((uint)0x30000003));

            Assert.AreEqual("0", ((CodeBlockNode)node02).Text); // Check if text is 0
            Assert.AreEqual("10", ((CodeBlockNode)node03).Text);// Check if text is 10

            Assert.AreEqual(5, controller.GetVisualNodes().Count);
        }

        [Test]
        public void UndoRedoEdit_EditCodeBlockText()
        {
            // Create CodeBlock Node and edit it by typing a=1;
            // Do undo.
            // Do Redo

            string commands = @"
                CreateCodeBlockNode|d:10449.0|d:10207.0|s:Your code goes here
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                EndNodeEdit|u:0x10000001|s:a = 1;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";
                
            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000001);
            Assert.AreEqual("a = 1;", ((CodeBlockNode)node01).Text); // Check if text is "a = 1";
            Assert.AreEqual(1, controller.GetVisualNodes().Count);
            
            commands = @"UndoOperation";
            bool result01 = controller.RunCommands(commands);
            Assert.AreEqual(true, result01);

            IVisualNode node02 = controller.GetVisualNode(0x10000001);
            Assert.Null(node02);// Undo delete the code block
            Assert.AreEqual(0, controller.GetVisualNodes().Count);

            commands = @"RedoOperation";
            bool result02 = controller.RunCommands(commands);
            Assert.AreEqual(true, result02);

            IVisualNode node03 = controller.GetVisualNode(0x10000001);
            Assert.AreEqual("a = 1;", ((CodeBlockNode)node03).Text); // Check if text is "a = 1";
            Assert.AreEqual(1, controller.GetVisualNodes().Count);
        }

        [Test]
        public void UndoRedoEdit_EditCaptionOfIdentifierNode()
        {
            // Create Identifier Node.
            // Change its caption
            // Do undo.
            // Do Redo

            string commands = @"CreateIdentifierNode|d:395.0|d:191.0";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
           
            IVisualNode node01 = controller.GetVisualNode(0x10000001);// Point Node
            string caption = ((IdentifierNode)node01).Caption; // catch the caption

            commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None|d:411.0|d:206.0
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None|d:411.0|d:206.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:myCoordinateSystem|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            bool result01 = controller.RunCommands(commands);
            Assert.AreEqual(true, result01);

            Assert.AreEqual("myCoordinateSystem", ((IdentifierNode)node01).Caption); // Check if caption is myCoordinateSystem.

            commands = @"UndoOperation";
            bool result02 = controller.RunCommands(commands);
            Assert.AreEqual(true, result02);
            
            Assert.AreEqual(caption, ((IdentifierNode)node01).Caption); // Check if text is 0

            commands = @"RedoOperation";
            bool result03 = controller.RunCommands(commands);
            Assert.AreEqual(true, result03);

            Assert.AreEqual("myCoordinateSystem", ((IdentifierNode)node01).Caption); // Check if text is 0

            Assert.AreEqual(1, controller.GetVisualNodes().Count);
        }

        [Test]
        public void UndoRedoEdit_EditNameOfRangeNode()
        {
            // Create Range Node.
            // Change its name to myRangeValue
            // Do undo.
            // Do Redo

            string commands = @"CreateFunctionNode|d:660.23148624|d:239.28845578|s:|s:Range|s:double,double,double";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000001);// Range Node
            string text = ((FunctionNode)node01).Text; // catch the name
            string caption = ((FunctionNode)node01).Caption; // catch the caption

            commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:764.23491407|d:281.53878061
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:764.23491407|d:281.53878061
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:myRangeValue|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:736.56520817|d:308.29494005
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:736.56520817|d:308.29494005
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            bool result01 = controller.RunCommands(commands);
            Assert.AreEqual(true, result01);

            Assert.AreEqual("myRangeValue", ((FunctionNode)node01).Text); // Check if text is myrangeValue.

            commands = @"UndoOperation";
            bool result02 = controller.RunCommands(commands);
            Assert.AreEqual(true, result02);

            Assert.AreEqual(text, ((FunctionNode)node01).Text); // Check if text is default one

            commands = @"RedoOperation";
            bool result03 = controller.RunCommands(commands);
            Assert.AreEqual(true, result03);

            Assert.AreEqual("myRangeValue", ((FunctionNode)node01).Text); // Check if text is myRangeValue
            Assert.AreEqual(caption, ((FunctionNode)node01).Caption); // Check if caption is Range

            Assert.AreEqual(1, controller.GetVisualNodes().Count);
        }

        [Test]
        public void UndoRedoEdit_EditNameOfAddNode()
        {
            // Create Add Node.
            // Change its name to addTwoValues
            // Do undo.
            // Do Redo

            string commands = @"CreateFunctionNode|d:482.0|d:199.0|s:|s:+|s:double,double";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000001);// Add Node
            string text = ((FunctionNode)node01).Text; // catch the name
            string caption = ((FunctionNode)node01).Caption; // catch the caption

            commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:addTwoValues|b:True";

            bool result01 = controller.RunCommands(commands);
            Assert.AreEqual(true, result01);

            Assert.AreEqual("addTwoValues", ((FunctionNode)node01).Text); // Check if text is addTwoValues.

            commands = @"UndoOperation";
            bool result02 = controller.RunCommands(commands);
            Assert.AreEqual(true, result02);

            Assert.AreEqual(text, ((FunctionNode)node01).Text); // Check if text is default one

            commands = @"RedoOperation";
            bool result03 = controller.RunCommands(commands);
            Assert.AreEqual(true, result03);

            Assert.AreEqual("addTwoValues", ((FunctionNode)node01).Text); // Check if text is addTwoValues
            Assert.AreEqual(caption, ((FunctionNode)node01).Caption); // Check if caption is Add

            Assert.AreEqual(1, controller.GetVisualNodes().Count);
        }

        [Test]
        public void UndoRedoEdit_EditNameOfPointNodeWhichWasDefinedByUser()
        {
            // Create Add Node.
            // Change its name to addTwoValues
            // Do undo.
            // Do Redo

            string commands = @"
                CreateFunctionNode|d:470.0|d:179.0|s:ProtoGeometry.dll|s:Point.ByCoordinates|s:double,double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:myStartPoint|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000001);// Point Node
            string text1 = ((FunctionNode)node01).Text; // catch the name myStartPoint
            string caption = ((FunctionNode)node01).Caption; // catch the caption

            commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:myEndPoint|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            bool result01 = controller.RunCommands(commands);
            Assert.AreEqual(true, result01);

            string text2 = ((FunctionNode)node01).Text; // catch the name myEndpoint

            commands = @"UndoOperation";
            bool result02 = controller.RunCommands(commands);
            Assert.AreEqual(true, result02);

            Assert.AreEqual(text1, ((FunctionNode)node01).Text); // Check if text is default one

            commands = @"RedoOperation";
            bool result03 = controller.RunCommands(commands);
            Assert.AreEqual(true, result03);

            Assert.AreEqual(text2, ((FunctionNode)node01).Text); // Check if text is addTwoValues
            Assert.AreEqual(caption, ((FunctionNode)node01).Caption); // Check if caption is Point

            Assert.AreEqual(1, controller.GetVisualNodes().Count);
        }

        [Test]
        public void UndoRedoEdit_EditNameOfCoordinateSystemByUniversalTransformWhichIshaving6Slots()
        {
            // Create Add Node.
            // Change its name to addTwoValues
            // Do undo.
            // Do Redo

            string commands = @"CreateFunctionNode|d:500.0|d:247.0|s:ProtoGeometry.dll|s:CoordinateSystem.ByUniversalTransform|s:CoordinateSystem,double,double,int,double,bool";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000001);// Add Node
            string text = ((FunctionNode)node01).Text; // catch the name
            string caption = ((FunctionNode)node01).Caption; // catch the caption

            commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:634.0|d:330.0
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:634.0|d:330.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:myCoordinateSystem|b:True";

            bool result01 = controller.RunCommands(commands);
            Assert.AreEqual(true, result01);
            string text1 = ((FunctionNode)node01).Text; // catch the name

            Assert.AreEqual(text1, ((FunctionNode)node01).Text); // Check if text is myCoordinateSystem.

            commands = @"UndoOperation";
            bool result02 = controller.RunCommands(commands);
            Assert.AreEqual(true, result02);

            Assert.AreEqual(text, ((FunctionNode)node01).Text); // Check if text is default one

            commands = @"RedoOperation";
            bool result03 = controller.RunCommands(commands);
            Assert.AreEqual(true, result03);

            Assert.AreEqual(text1, ((FunctionNode)node01).Text); // Check if text is myCoordinateSystem
            Assert.AreEqual(caption, ((FunctionNode)node01).Caption); // Check if caption is CoordinateSystem

            Assert.AreEqual(1, controller.GetVisualNodes().Count);
        }

        [Test]
        public void UndoRedoEdit_EditNameOfSolidRevolveWhichIshaving5Slots()
        {
            // Create Solid.Revolve Node.
            // Change its name to myRevolvedSolid.
            // Do undo.
            // Do Redo.

            string commands = @"CreateFunctionNode|d:501.0|d:211.0|s:ProtoGeometry.dll|s:Solid.Revolve|s:Curve,Point,Vector,double,double";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000001);// Solid.Revolve Node
            string text = ((FunctionNode)node01).Text; // catch the name
            string caption = ((FunctionNode)node01).Caption; // catch the caption

            commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:myRevolvedSolid|b:True";

            bool result01 = controller.RunCommands(commands);
            Assert.AreEqual(true, result01);
            string text1 = ((FunctionNode)node01).Text; // catch the name

            commands = @"UndoOperation";
            bool result02 = controller.RunCommands(commands);
            Assert.AreEqual(true, result02);

            Assert.AreEqual(text, ((FunctionNode)node01).Text); // Check if text is default one

            commands = @"RedoOperation";
            bool result03 = controller.RunCommands(commands);
            Assert.AreEqual(true, result03);

            Assert.AreEqual(text1, ((FunctionNode)node01).Text); // Check if text is myRevolvedSolid
            Assert.AreEqual(caption, ((FunctionNode)node01).Caption); // Check if caption is Solid

            Assert.AreEqual(1, controller.GetVisualNodes().Count);
        }

        [Test]
        public void UndoRedoEdit_EditNameWhichIsHavingRangeExpression()
        {
            // Create Code Block by double clicking in canvas.
            // type range expression like a = a < 5 ? 3 : 4;
            // Do undo.
            // Do Redo.

            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:451.5|d:205.50226077|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:a = a < 5 ? 3 : 4;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000001);
            string text = ((CodeBlockNode)node01).Text; // catch the name which is typed by user "a = a < 5 ? 3 : 4;"
            Assert.AreEqual(1, controller.GetVisualNodes().Count);

            commands = @"UndoOperation";
            bool result01 = controller.RunCommands(commands);
            Assert.AreEqual(true, result01);

            IVisualNode node02 = controller.GetVisualNode(0x10000001);
            Assert.Null(node02); // Undo delete the code block
            Assert.AreEqual(0, controller.GetVisualNodes().Count);

            commands = @"RedoOperation";
            bool result02 = controller.RunCommands(commands);
            Assert.AreEqual(true, result02);

            IVisualNode node03 = controller.GetVisualNode(0x10000001);
            Assert.AreEqual("a = a < 5 ? 3 : 4;", ((CodeBlockNode)node03).Text);
            Assert.AreEqual(1, controller.GetVisualNodes().Count);
        }

        [Test]
        public void UndoRedoEdit_EditMultipleTimesAndDoUndoRedo()
        {
            // Create ByoriginVectors node from CoordinateSystem (first option).
            // Change its name by appening to its original default name (ad myCS at the end).
            // Change its name again by remvoing myCS and change it to myCoordinateSytem.
            // Do undo twice.
            // Do Redo twice.

            string commands = @"
                CreateFunctionNode|d:397.0|d:225.0|s:ProtoGeometry.dll|s:CoordinateSystem.ByOriginVectors|s:Point,Vector,Vector
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:F268435457myCS|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000001);// CoordinateSystem node.
            string text = ((FunctionNode)node01).Text; // catch the name which should be F268435457myCS
            string caption = ((FunctionNode)node01).Caption; // catch the caption

            commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:F268435457myCoordinateSystem|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            bool result01 = controller.RunCommands(commands);
            Assert.AreEqual(true, result01);
            string text1 = ((FunctionNode)node01).Text; // catch the name which should be F268435457myCoordinateSystem

            commands = @"UndoOperation";
            bool result02 = controller.RunCommands(commands);
            Assert.AreEqual(true, result02);

            Assert.AreEqual(text, ((FunctionNode)node01).Text); // Check if text is F268435457myCS
            Assert.AreEqual(caption, ((FunctionNode)node01).Caption); // Check if caption is CoordinateSystem

            commands = @"UndoOperation";
            bool result03 = controller.RunCommands(commands);
            Assert.AreEqual(true, result03);

            Assert.AreEqual("Var1", ((FunctionNode)node01).Text); // Check if text is default one
            Assert.AreEqual(caption, ((FunctionNode)node01).Caption); // Check if caption is CoordinateSystem

            commands = @"RedoOperation";
            bool result04 = controller.RunCommands(commands);
            Assert.AreEqual(true, result04);
            Assert.AreEqual(text, ((FunctionNode)node01).Text); // Check if text is F268435457myCS

            commands = @"RedoOperation";
            bool result05 = controller.RunCommands(commands);
            Assert.AreEqual(true, result05);
            Assert.AreEqual(text1, ((FunctionNode)node01).Text); // Check if text is F268435457myCoordinateSystem
            
            Assert.AreEqual(1, controller.GetVisualNodes().Count);
        }

        [Test]
        public void UndoRedoEdit_EditNameInBetweenNodeCreationAndTestUndoRedo()
        {
            // Create Code Block and Identifier node by droping them on canvas from library
            // Edit name of identifier node by apending to its existing name.
            // Drag and drop range node.
            // Edit name of Range node.
            // Do undo 3 times to make the Indetifier node back to its original name.
            // Redo 3 times to bring back Range node with its user typed name.

            string commands = @"
                CreateCodeBlockNode|d:449.0|d:121.0|s:Your code goes here
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                EndNodeEdit|u:0x10000001|s:a = 10;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateIdentifierNode|d:445.0|d:216.0";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000002);// Identifier node.
            string caption = ((IdentifierNode)node01).Caption; // catch the caption

            commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:I268435458myIdentifier|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";
            bool result01 = controller.RunCommands(commands);
            Assert.AreEqual(true, result01);
            string caption1 = ((IdentifierNode)node01).Caption; // catch the name which should be I268435458myIdentifier

            commands = @"CreateFunctionNode|d:749.0|d:281.0|s:|s:Range|s:double,double,double";
            bool result02 = controller.RunCommands(commands);
            Assert.AreEqual(true, result02);

            IVisualNode node02 = controller.GetVisualNode(0x10000003);// Function (Range) node.
            string text2 = ((FunctionNode)node02).Text; // catch the name which should be F268435459

            commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000003|s:myRangeExpression|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";
            bool result03 = controller.RunCommands(commands);
            Assert.AreEqual(true, result03);
            string text3 = ((FunctionNode)node02).Text; // catch the name which should be myRangeExpression

            commands = @"UndoOperation";
            bool result04 = controller.RunCommands(commands);
            Assert.AreEqual(true, result04);
            Assert.AreEqual(text2, ((FunctionNode)node02).Text); // Check if text is F268435459

            commands = @"UndoOperation";
            bool result05 = controller.RunCommands(commands);
            Assert.AreEqual(true, result05);
            Assert.AreEqual(2, controller.GetVisualNodes().Count);
            Assert.AreEqual(caption1, ((IdentifierNode)node01).Caption); // Check if text is I268435458myIdentifier

            commands = @"UndoOperation";
            bool result06 = controller.RunCommands(commands);
            Assert.AreEqual(true, result06);
            Assert.AreEqual(2, controller.GetVisualNodes().Count);
            Assert.AreEqual(caption, ((IdentifierNode)node01).Caption); // Check if text is I268435458

            commands = @"RedoOperation";
            bool result07 = controller.RunCommands(commands);
            Assert.AreEqual(true, result07);
            Assert.AreEqual(2, controller.GetVisualNodes().Count);
            Assert.AreEqual(caption1, ((IdentifierNode)node01).Caption); // Check if text is I268435458myIdentifier
            
            commands = @"RedoOperation";
            bool result08 = controller.RunCommands(commands);
            Assert.AreEqual(true, result08);
            node02 = controller.GetVisualNode(0x10000003);
            Assert.AreEqual(3, controller.GetVisualNodes().Count);
            Assert.AreEqual(text2, ((FunctionNode)node02).Text); // Check if text is F268435459
            
            commands = @"RedoOperation";
            bool result09 = controller.RunCommands(commands);
            Assert.AreEqual(true, result09);
            Assert.AreEqual(3, controller.GetVisualNodes().Count);
            Assert.AreEqual(text3, ((FunctionNode)node02).Text); // Check if text is myRangeExpression
        }

        [Test]
        public void UndoRedoEdit_EditDriverNodeCaption()
        {
            // Create Driver Node.
            // Double click on caption to edit. type myDriverNode as a new caption.
            // Do undo.
            // Do Redo

            string commands = @"
                CreateDriverNode|d:10983.875|d:10555.74166667";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000001);// CodeBlock Node
            string caption =  ((DriverNode)node01).Caption; // Catch the default cation name;

            commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:myDriverNode|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            bool result01 = controller.RunCommands(commands);
            Assert.AreEqual(true, result01);

            string caption01 = ((DriverNode)node01).Caption;

            commands = @"UndoOperation";
            bool result02 = controller.RunCommands(commands);
            Assert.AreEqual(true, result02);

            Assert.AreEqual(caption, ((DriverNode)node01).Caption); // Check if the caption is default one;
            Assert.AreEqual(1, controller.GetVisualNodes().Count);

            commands = @"RedoOperation";
            bool result03 = controller.RunCommands(commands);
            Assert.AreEqual(true, result03);

            Assert.AreEqual(caption01, ((DriverNode)node01).Caption); // Check if the caption is myDriverNode;

        }

        [Test]
        public void Defect_IDE_956()
        {
            // Create Driver Node, Identity, GeoemtrySettings and Reset nodes.
            // Undo creation of all nodes.
            // Redo all

            string commands = @"
                CreateFunctionNode|d:10727.0|d:10200.0|s:ProtoGeometry.dll|s:CoordinateSystem.Identity|s:
                CreateFunctionNode|d:10686.57142857|d:10380.66666667|s:ProtoGeometry.dll|s:GeometrySettings.GeometrySettings|s:
                CreateFunctionNode|d:10726.57142857|d:10465.23809524|s:ProtoGeometry.dll|s:GeometrySettings.Reset|s:
                CreateDriverNode|d:10590.0|d:10575.52380952
                UndoOperation
                UndoOperation
                UndoOperation
                UndoOperation
                RedoOperation
                RedoOperation
                RedoOperation
                RedoOperation";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            Assert.AreEqual(4, controller.GetVisualNodes().Count); // Check if there are 4 nodes.

        }

        [Test]
        public void Defect_IDE_1472()
        {
            // IDE-1472: [CRASH] while redoing implicit connection line.
            // Create two CBN with values "a=1;" and "b=a;".
            // Now do Undo and then Redo.
            // After redo DSS is crashing.

            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10376.0|d:10162.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:a=1;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10396.0|d:10252.0|s:
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:b=a;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                UndoOperation
                RedoOperation";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            Assert.AreEqual(2, controller.GetVisualNodes().Count); // Check if there are 4 nodes.

            // Check for Implicit connection
            ISlot outputSlot = controller.GetSlot(0x30000001); // Output of CB Node.
            Assert.IsNotNull(outputSlot);
            ISlot inputSlot = controller.GetSlot(0x30000002); // Input slot of CB node.
            Assert.IsNotNull(inputSlot);
            // Output slot of first CB Node should connect to input slot second CB node.
            uint[] connecting = outputSlot.ConnectingSlots;
            Assert.IsNotNull(connecting);
            Assert.IsTrue(connecting.Contains((uint)0x30000002));
        }

        [Test]
        public void Defect_IDE_1464()
        {
            // IDE-1464 Implicit connection line is going way while doing undo for explicit connection line.
            // Create two CBN with values "a=1;" and "b=a;".
            // Now drag and drop identifier node.
            // Now connect first CBN to Identifier node.
            // Now do Undo.
            // After undo DSS removing explicit connection line (which is correct), along with implicit connection line. (which is wrong.) 

            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10444.0|d:10171.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:a=1;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10464.28571429|d:10239.9047619|s:
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:b=a;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateIdentifierNode|d:10572.28571429|d:10192.47619048
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10488.85714286|d:10184.47619048
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10573.42857143|d:10214.19047619
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                UndoOperation";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            ISlot outputSlot = controller.GetSlot(0x30000001); // Output slot of CB Node.
            Assert.IsNotNull(outputSlot);
            ISlot inputSlot = controller.GetSlot(0x30000002); // Input slot of CB node.
            Assert.IsNotNull(inputSlot);
            // Output slot of first CB Node should connect to Input slot second CB node.
            uint[] connecting = outputSlot.ConnectingSlots;
            Assert.IsNotNull(connecting);
            Assert.IsTrue(connecting.Contains((uint)0x30000002));

            Assert.AreEqual(3, controller.GetVisualNodes().Count);

        }
        [Test]
        public void Defect_IDE_1765()
        {
            // Create CBN with value 0.
            // drag a output connection line and drop it on canvas without connecting .
            // Now do undo.
            // DSS is crashing after undo.

            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:15386.0|d:15174.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:0|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15399.0|d:15173.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15489.0|d:15410.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                UndoOperation";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            Assert.AreEqual(0, controller.GetVisualNodes().Count);

        }
        [Test]
        public void Defect_IDE_2015()
        {
        //http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-2015

            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15339.0|d:15197.0|s:
            BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000001|s:1|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15334.0|d:15317.0|s:
            BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000002|s:2|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateFunctionNode|d:15635.0|d:15209.0|s:ProtoGeometry.dll|s:Line.ByStartPointDirectionLength|s:Point,Vector,double
            CreateFunctionNode|d:15496.0|d:15095.0|s:ProtoGeometry.dll|s:Point.ByCoordinates|s:double,double,double
            CreateRadialMenu|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|u:0x10000002
            CreateFunctionNode|d:15506.0|d:15305.0|s:ProtoGeometry.dll|s:Vector.ByCoordinates|s:double,double,double
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15355.0|d:15202.0
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15439.0|d:15072.0
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            CreateRadialMenu|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|u:0x10000001
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15352.0|d:15199.0
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:15446.0|d:15090.0
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15352.0|d:15199.0
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None|d:15453.0|d:15118.0
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15348.0|d:15318.0
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15454.0|d:15288.0
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15351.0|d:15321.0
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:15456.0|d:15314.0
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15349.0|d:15319.0
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15462.0|d:15330.0
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateRadialMenu|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast|u:0x10000005
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15350.4798931|d:15317.0
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None|d:15448.4798931|d:15326.0
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:1|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:1|e:System.Windows.Input.ModifierKeys,None|d:15620.4798931|d:15216.0
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:1|e:System.Windows.Input.ModifierKeys,None|d:15683.4798931|d:15182.0
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15540.4798931|d:15112.0
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15620.4798931|d:15155.0
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15553.4798931|d:15324.0
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:15619.4798931|d:15173.0
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            CreateCodeBlockNode|d:15553.4798931|d:15222.0|s:
            BeginNodeEdit|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,Text
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
            EndNodeEdit|u:0x10000006|s:1|b:True
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15566.4798931|d:15226.0
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None|d:15626.4798931|d:15195.0
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
            UndoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            UndoOperation
            ";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            Assert.AreEqual(0, controller.GetVisualNodes().Count);

        }
    }
}