using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;
using DesignScriptStudio.Graph.Core;

namespace DesignScriptStudio.Tests.UnitTests
{
    class DeleteNode
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
        public void DeleteCodeBlock()
        {
            //Create Code Block
            //Select Code block
            //Delete Code Block
            string commands = @"
                CreateCodeBlockNode|d:381.0|d:265.0|s:Double click and type
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
            Assert.AreEqual(0, controller.GetVisualNodes().Count);
        }
        [Test]
        public void DeleteCodeBlock_BackSpace()
        {

            //Create Code Block
            //Select Code block
            //Delete Code Block
            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10397.0|d:10139.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:100|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
            Assert.AreEqual(0, controller.GetVisualNodes().Count);

            commands = @"UndoOperation";
            bool result2 = controller.RunCommands(commands);
            Assert.AreEqual(true, result2);
            Assert.AreEqual(1, controller.GetVisualNodes().Count);
        }

        [Test]
        public void DeleteDriver()
        {

            //Create driver
            // Select driver
            //Delete driver
            string commands = @"
                CreateDriverNode|d:425.0|d:254.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
            Assert.AreEqual(0, controller.GetVisualNodes().Count);

        }
        [Test]
        public void DeleteIdentifier()
        {

            //Create identifier
            // Select identifier
            //Delete identifier
            string commands = @"
                CreateIdentifierNode|d:387.0|d:300.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
            Assert.AreEqual(0, controller.GetVisualNodes().Count);
        }

        [Test]
        public void DeleteIdentifier_backspace()
        {

            //Create identifier
            //Select identifier
            //Delete identifier
            string commands = @"
                CreateIdentifierNode|d:367.0|d:274.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
            commands = "UndoOperation";
            bool result2 = controller.RunCommands(commands);
            IVisualNode node2 = controller.GetVisualNode(0x10000001);
            Assert.NotNull(node2); // Check if node is created 
            Assert.AreEqual(NodeType.Identifier, node2.VisualType);
            Assert.AreEqual(1, controller.GetVisualNodes().Count);
            commands = "RedoOperation";
            bool result3 = controller.RunCommands(commands);
            Assert.AreEqual(0, controller.GetVisualNodes().Count);
        }

        [Test]
        public void DeleteIdentifier_2()
        {

            //Create identifier
            // Select identifier by clicking on the preview 
            //Delete identifier
            // crashes
            string commands = @"
                CreateIdentifierNode|d:358.0|d:287.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Preview|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Preview|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
            Assert.AreEqual(0, controller.GetVisualNodes().Count);
        }
        [Test]
        public void DeleteRange()
        {

            //Create identifier
            // Select identifier by clicking on the preview 
            //Delete identifier

            string commands = @"
                CreateFunctionNode|d:376.0|d:281.0|s:|s:Range|s:double,double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
            Assert.AreEqual(0, controller.GetVisualNodes().Count);
        }
        [Test]
        public void DeleteRange_2()
        {

            //Create identifier
            // Select identifier by clicking on the preview 
            //Delete identifier
            // crashes
            string commands = @"
                CreateFunctionNode|d:449.0|d:276.0|s:|s:Range|s:double,double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Preview|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Preview|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
            Assert.AreEqual(0, controller.GetVisualNodes().Count);
        }
        [Test]
        public void DeleteFunction_BackSpace()
        {

            //Create identifier
            // Select identifier by clicking on the preview 
            //Delete identifier
            // crashes
            string commands = @"
                CreateFunctionNode|d:366.0|d:288.0|s:|s:Range|s:double,double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
            Assert.AreEqual(0, controller.GetVisualNodes().Count);

            commands = "UndoOperation";
            bool result2 = controller.RunCommands(commands);
            IVisualNode node2 = controller.GetVisualNode(0x10000001);
            Assert.NotNull(node2); // Check if node is created 
            Assert.AreEqual(NodeType.Function, node2.VisualType);
            Assert.AreEqual(1, controller.GetVisualNodes().Count);

            commands = "RedoOperation";
            bool result3 = controller.RunCommands(commands);
            Assert.AreEqual(0, controller.GetVisualNodes().Count);

        }
        [Test]
        public void DeleteAdd()
        {

            //Create identifier
            // Select identifier by clicking on the preview 
            //Delete identifier
            // crashes
            string commands = @"
                CreateFunctionNode|d:487.0|d:297.0|s:|s:+|s:double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
            Assert.AreEqual(0, controller.GetVisualNodes().Count);
        }
        [Test]
        public void DeleteAdd_BackSpace()
        {

            //Create identifier
            // Select identifier by clicking on the preview 
            //Delete identifier
            // crashes
            string commands = @"
                CreateFunctionNode|d:504.0|d:287.0|s:|s:+|s:double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
            Assert.AreEqual(0, controller.GetVisualNodes().Count);

            commands = "UndoOperation";
            bool result2 = controller.RunCommands(commands);
            IVisualNode node2 = controller.GetVisualNode(0x10000001);
            Assert.NotNull(node2); // Check if node is created 
            Assert.AreEqual(NodeType.Function, node2.VisualType);
            Assert.AreEqual(1, controller.GetVisualNodes().Count);

            commands = "RedoOperation";
            bool result3 = controller.RunCommands(commands);
            Assert.AreEqual(0, controller.GetVisualNodes().Count);

        }
        [Test]
        public void DeletePoint()
        {

            string commands = @"
                CreateFunctionNode|d:516.0|d:275.0|s:ProtoGeometry.dll|s:Point.ByCoordinates|s:double,double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
            Assert.AreEqual(0, controller.GetVisualNodes().Count);
        }
        [Test]
        public void Delete_BackSpace()
        {

            //Create identifier
            // Select identifier by clicking on the preview 
            //Delete identifier
            // crashes
            string commands = @"
                CreateFunctionNode|d:448.0|d:408.0|s:ProtoGeometry.dll|s:CoordinateSystem.ByOriginVectors|s:Point,Vector,Vector,Vector,bool,bool
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
            Assert.AreEqual(0, controller.GetVisualNodes().Count);

            commands = "UndoOperation";
            bool result2 = controller.RunCommands(commands);
            IVisualNode node2 = controller.GetVisualNode(0x10000001);
            Assert.NotNull(node2); // Check if node is created 
            Assert.AreEqual(NodeType.Function, node2.VisualType);
            Assert.AreEqual(1, controller.GetVisualNodes().Count);

            commands = "RedoOperation";
            bool result3 = controller.RunCommands(commands);
            Assert.AreEqual(0, controller.GetVisualNodes().Count);

        }
        [Test]
        public void DeleteMath()
        {

            string commands = @"
                CreateFunctionNode|d:550.0|d:313.0|s:Math.dll|s:Math.Asin|s:double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
            Assert.AreEqual(0, controller.GetVisualNodes().Count);
        }
        [Test]
        public void DeleteFunctionFourSlot()
        {

            // Create Range node- function node with 4 input slots
            // Select node
            // Delete it


            string commands = @"
                CreateFunctionNode|d:417.0|d:232.0|s:ProtoGeometry.dll|s:CoordinateSystem.ByOriginVectors|s:Point,Vector,Vector,Vector
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents";
            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
            Assert.AreEqual(0, controller.GetVisualNodes().Count);
        }
        [Test]
        public void DeleteFunctionFiveSlot()
        {

            // Create Range node- function node with 5 input slots
            // Select node
            // Delete it
            // Undo

            string commands = @"
                CreateFunctionNode|d:504.0|d:227.0|s:ProtoGeometry.dll|s:Surface.Revolve|s:Curve,Point,Vector,double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents";
            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
            Assert.AreEqual(0, controller.GetVisualNodes().Count);
        }
        [Test]
        public void DeleteFunctionSixSlot()
        {

            // Create Range node- function node with 6 input slots
            // Select node
            // Delete it
            // Undo

            string commands = @"
                CreateFunctionNode|d:821.42857143|d:782.57142857|s:ProtoGeometry.dll|s:CoordinateSystem.ByOriginVectors|s:Point,Vector,Vector,Vector,bool,bool
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents";
            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
            Assert.AreEqual(0, controller.GetVisualNodes().Count);
        }
        [Test]
        public void DeleteAfterConnect()
        {

            //Create nodes
            //Connect them
            //Delete one of them

            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10286.0|d:10179.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:100|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10336.0|d:10300.0|s:
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:200|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateFunctionNode|d:10520.0|d:10216.0|s:|s:+|s:double,double
                CreateIdentifierNode|d:10850.0|d:10233.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10320.0|d:10190.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10509.0|d:10226.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10373.0|d:10313.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:10518.0|d:10257.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10607.0|d:10247.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10840.0|d:10250.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
            Assert.AreEqual(3, controller.GetVisualNodes().Count);
        }
        [Test]
        public void DeleteMultiple()
        {

            // Create multiple nodes
            // Select by window
            // Delete one of them

            string commands = @"
                CreateCodeBlockNode|d:406.0|d:191.0|s:Double Click and type
                CreateCodeBlockNode|d:487.0|d:325.0|s:Double Click and type
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:100|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:200|b:True
                CreateFunctionNode|d:660.0|d:256.0|s:|s:+|s:double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:522.0|d:340.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:664.0|d:282.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:439.0|d:202.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:665.0|d:268.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                CreateIdentifierNode|d:873.0|d:268.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:782.0|d:282.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:875.0|d:283.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:595.0|d:182.0
                SelectComponent|u:0x10000003|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x60000001|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x60000002|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x60000003|e:System.Windows.Input.ModifierKeys,None
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:805.0|d:361.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
            Assert.AreEqual(3, controller.GetVisualNodes().Count);
        }
        [Test]
        public void Delete_Multiple_BackSpace()
        {

            //Create identifier
            // Select identifier by clicking on the preview 
            //Delete identifier
            // crashes
            string commands = @"
                CreateIdentifierNode|d:307.0|d:219.0
                CreateFunctionNode|d:450.0|d:273.0|s:|s:Range|s:double,double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
            Assert.AreEqual(0, controller.GetVisualNodes().Count);

            commands = @"UndoOperation
                        UndoOperation";
            bool result2 = controller.RunCommands(commands);
            IVisualNode node2 = controller.GetVisualNode(0x10000001);
            Assert.NotNull(node2); // Check if node is created 
            Assert.AreEqual(NodeType.Identifier, node2.VisualType);
            IVisualNode node3 = controller.GetVisualNode(0x10000001);
            Assert.NotNull(node3); // Check if node is created 
            Assert.AreEqual(NodeType.Identifier, node3.VisualType);

            Assert.AreEqual(2, controller.GetVisualNodes().Count);

            commands = @"RedoOperation
                         RedoOperation";
            bool result3 = controller.RunCommands(commands);
            Assert.AreEqual(0, controller.GetVisualNodes().Count);
        }

        [Test]
        public void DeleteNode_Preview()
        {

            string commands = @"
                CreateFunctionNode|d:356.0|d:377.0|s:ProtoGeometry.dll|s:Solid.Revolve|s:Curve,Line,double,double
                CreateFunctionNode|d:380.0|d:242.0|s:ProtoGeometry.dll|s:CoordinateSystem.ByOriginVectors|s:Point,Vector,Vector,Vector,bool
                CreateFunctionNode|d:655.0|d:317.0|s:ProtoGeometry.dll|s:CoordinateSystem.ByOriginVectors|s:Point,Vector,Vector,Vector,bool,bool
                CreateFunctionNode|d:372.0|d:158.0|s:|s:+|s:double,double
                CreateFunctionNode|d:574.0|d:173.0|s:|s:Range|s:double,double,double
                CreateIdentifierNode|d:781.0|d:192.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Preview|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Preview|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Preview|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Preview|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Preview|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Preview|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,Preview|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,Preview|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Preview|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Preview|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,Preview|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,Preview|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
            Assert.AreEqual(0, controller.GetVisualNodes().Count);
        }
        [Test]
        public void Delete_BackSpace_Preview()
        {

            // Create identifier
            // Select identifier by clicking on the preview 
            // Delete identifier
            // crashes
            string commands = @"
                CreateIdentifierNode|d:418.0|d:223.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Preview|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Preview|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
            Assert.AreEqual(0, controller.GetVisualNodes().Count);

            commands = "UndoOperation";
            bool result2 = controller.RunCommands(commands);
            IVisualNode node2 = controller.GetVisualNode(0x10000001);
            Assert.NotNull(node2); // Check if node is created 
            Assert.AreEqual(NodeType.Identifier, node2.VisualType);
            Assert.AreEqual(1, controller.GetVisualNodes().Count);

            commands = "RedoOperation";
            bool result3 = controller.RunCommands(commands);
            Assert.AreEqual(0, controller.GetVisualNodes().Count);

        }
        [Test]
        public void DeleteUndo()
        {

            // Delete the identifier node
            // Undo delete

            string commands = @"
                CreateIdentifierNode|d:423.0|d:214.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents
                UndoOperation";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
            Assert.AreEqual(1, controller.GetVisualNodes().Count);


        }
        [Test]
        public void DeleteOutsideNodeSelection()
        {

            //create Identifier Node.
            //Select near to Node, click anywhere near to node but do select it.
            //Press delete key on key board.
            // Defect logged http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-958
            string commands = @"
                CreateCodeBlockNode|d:459.0|d:184.0|s:Double click and type
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

        }

        [Test]
        public void UndoDeleteCodeBlock()
        {

            // Create Code Block
            // Select Code block
            // Delete Code Block
            // Undo
            string commands = @"
                CreateCodeBlockNode|d:420.0|d:238.0|s:Double click and type
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:100|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents
                UndoOperation";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
            Assert.AreEqual(1, controller.GetVisualNodes().Count);
            Assert.AreEqual(0x10000001, controller.GetVisualNodes().ElementAt(0).NodeId);
            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.NotNull(node); // Check if node is created 
            Assert.AreEqual(NodeType.CodeBlock, node.VisualType);

        }
        [Test]
        public void UndoDeleteMultipleCodeblocks()
        {

            // Create Code Block
            // Select multiple Code block
            // Delete them
            // Undo
            string commands = @"
                CreateCodeBlockNode|d:501.0|d:211.0|s:Double click and type
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:536.0|d:236.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:536.0|d:236.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:100|b:True
                CreateCodeBlockNode|d:505.0|d:287.0|s:Double click and type
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None|d:537.0|d:304.0
                EndNodeEdit|u:0x10000002|s:200|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:545.0|d:323.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents
                UndoOperation
                UndoOperation";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            Assert.AreEqual(0x10000001, controller.GetVisualNodes().ElementAt(0).NodeId);
            Assert.AreEqual(0x10000002, controller.GetVisualNodes().ElementAt(1).NodeId);
            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.NotNull(node); // Check if node is created 
            Assert.AreEqual(NodeType.CodeBlock, node.VisualType);
            IVisualNode node2 = controller.GetVisualNode(0x10000002);
            Assert.NotNull(node2); // Check if node is created 
            Assert.AreEqual(NodeType.CodeBlock, node2.VisualType);

        }
        [Test]
        public void UndoDeleteSelectionbyWindow()
        {

            // Create Code Block
            // Select Code block by window
            // Delete Code Block
            // Undo
            string commands = @"
                CreateCodeBlockNode|d:501.0|d:211.0|s:Double click and type
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:536.0|d:236.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:536.0|d:236.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:100|b:True
                CreateCodeBlockNode|d:505.0|d:287.0|s:Double click and type
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None|d:537.0|d:304.0
                EndNodeEdit|u:0x10000002|s:200|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:545.0|d:323.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents
                UndoOperation
                UndoOperation";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.NotNull(node); // Check if node is created 
            Assert.AreEqual(NodeType.CodeBlock, node.VisualType);
            IVisualNode node2 = controller.GetVisualNode(0x10000002);
            Assert.NotNull(node2); // Check if node is created 
            Assert.AreEqual(NodeType.CodeBlock, node2.VisualType);

        }
        [Test]
        public void UndoDeleteDriverNode()
        {

            // Create Driver Node
            // Select node
            // Delete it
            // Undo
            string commands = @"
                CreateDriverNode|d:441.0|d:179.0
                CreateDriverNode|d:480.0|d:279.0
                CreateDriverNode|d:732.0|d:197.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents
                UndoOperation
                UndoOperation
                UndoOperation";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.NotNull(node); // Check if node is created 
            Assert.AreEqual(NodeType.Driver, node.VisualType);

            IVisualNode node2 = controller.GetVisualNode(0x10000002);
            Assert.NotNull(node2); // Check if node is created 
            Assert.AreEqual(NodeType.Driver, node2.VisualType);

            IVisualNode node3 = controller.GetVisualNode(0x10000003);
            Assert.NotNull(node3); // Check if node is created 
            Assert.AreEqual(NodeType.Driver, node3.VisualType);
        }
        [Test]
        public void UndoDeleteIdentifierNode()
        {
            // Create identifier node
            // Select it 
            // Delete 
            // Undo

            string commands = @"
                CreateIdentifierNode|d:429.0|d:221.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents
                UndoOperation";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.NotNull(node); // Check if node is created 
            Assert.AreEqual(NodeType.Identifier, node.VisualType);
        }
        [Test]
        public void UndoDeleteSelectbyPreview()
        {

            // Create identifier
            // Select by clickingo n preview
            // Delete 
            // Undo
            string commands = @"
                CreateIdentifierNode|d:414.0|d:220.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Preview|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Preview|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents
                UndoOperation";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.NotNull(node); // Check if node is created 
            Assert.AreEqual(NodeType.Identifier, node.VisualType);

        }
        [Test]
        public void UndoDeleteFunctionOneSlot()
        {

            // NodeType - one Input Slots
            // Create Add node
            // Select it
            // Delete 
            // Undo
            string commands = @"CreateFunctionNode|d:375.0|d:338.0|s:DesignScriptStudio.Tests.dll|s:Vehicle.ByPrice|s:int
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents
                UndoOperation";
            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.NotNull(node); // Check if node is created 
            Assert.AreEqual(NodeType.Function, node.VisualType);
            Assert.AreEqual(1, node.GetInputSlots().Count());
        }
        [Test]
        public void UndoDeleteFunctionTwoSlot()
        {

            // NodeType - Two Input Slots
            // Create Add node
            // Select it
            // Delete 
            // Undo
            string commands = @"CreateFunctionNode|d:342.0|d:287.0|s:|s:+|s:double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents
                UndoOperation";
            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.NotNull(node); // Check if node is created 
            Assert.AreEqual(NodeType.Function, node.VisualType);
            Assert.AreEqual(2, node.GetInputSlots().Count());
        }
        [Test]
        public void UndoDeleteFunctionThreeSlot()
        {

            // Create Range node- function node with 3 input slots 
            // Select node
            // Delete it
            // Undo

            string commands = @"
                CreateFunctionNode|d:404.0|d:275.0|s:|s:Range|s:double,double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents
                UndoOperation";
            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.NotNull(node); // Check if node is created 
            Assert.AreEqual(NodeType.Function, node.VisualType);
            Assert.AreEqual(3, node.GetInputSlots().Count());
        }
        [Test]
        public void UndoDeleteFunctionFourSlot()
        {

            // Create Range node- function node with 4 input slots
            // Select node
            // Delete it
            // Undo

            string commands = @"
                CreateFunctionNode|d:417.0|d:232.0|s:ProtoGeometry.dll|s:CoordinateSystem.ByOriginVectors|s:Point,Vector,Vector,Vector
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents
                UndoOperation";
            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.NotNull(node); // Check if node is created 
            Assert.AreEqual(NodeType.Function, node.VisualType);
            Assert.AreEqual(4, node.GetInputSlots().Count());
        }
        [Test]
        public void UndoDeleteFunctionFiveSlot()
        {

            // Create Range node- function node with 5 input slots
            // Select node
            // Delete it
            // Undo

            string commands = @"
                CreateFunctionNode|d:504.0|d:227.0|s:ProtoGeometry.dll|s:Surface.Revolve|s:Curve,Point,Vector,double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents
                UndoOperation";
            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.NotNull(node); // Check if node is created 
            Assert.AreEqual(NodeType.Function, node.VisualType);
            Assert.AreEqual(5, node.GetInputSlots().Count());
        }
        [Test]
        public void UndoDeleteFunctionSixSlot()
        {

            // Create Range node- function node with 6 input slots
            // Select node
            // Delete it
            // Undo

            string commands = @"
                CreateFunctionNode|d:821.42857143|d:782.57142857|s:ProtoGeometry.dll|s:CoordinateSystem.ByOriginVectors|s:Point,Vector,Vector,Vector,bool,bool
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents
                UndoOperation";
            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.NotNull(node); // Check if node is created 
            Assert.AreEqual(NodeType.Function, node.VisualType);
            Assert.AreEqual(6, node.GetInputSlots().Count());
        }
        [Test]
        public void UndoDeleteMath()
        {

            // Create Math
            // Select it
            // Delete 
            // Undo
            string commands = @"CreateFunctionNode|d:346.0|d:299.0|s:Math.dll|s:Math.Cos|s:double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents
                UndoOperation";
            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.NotNull(node); // Check if node is created 
            Assert.AreEqual(NodeType.Function, node.VisualType);
        }


        [Test]
        public void UndoDeleteConnectednodes()
        {

            // Create some connected nodes
            // Delete one of them
            // Undo
            string commands = @"CreateCodeBlockNode|d:350.0|d:201.0|s:Double click and type
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:403.0|d:229.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:402.0|d:229.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:100|b:True
                CreateCodeBlockNode|d:363.0|d:250.0|s:Double click and type
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:200|b:True
                CreateFunctionNode|d:464.0|d:231.0|s:|s:+|s:double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None|d:376.0|d:265.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None|d:369.0|d:372.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None|d:355.0|d:211.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:360.0|d:260.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:385.0|d:219.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:467.0|d:245.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:391.0|d:372.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:467.0|d:259.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                CreateIdentifierNode|d:740.0|d:248.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:587.0|d:264.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:745.0|d:265.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents
                UndoOperation";
            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.NotNull(node); // Check if node is created 
            Assert.AreEqual(NodeType.CodeBlock, node.VisualType);

            IVisualNode node2 = controller.GetVisualNode(0x10000002);
            Assert.NotNull(node2); // Check if node is created 
            Assert.AreEqual(NodeType.CodeBlock, node2.VisualType);

            IVisualNode node3 = controller.GetVisualNode(0x10000003);
            Assert.NotNull(node3); // Check if node is created 
            Assert.AreEqual(NodeType.Function, node3.VisualType);

            IVisualNode node4 = controller.GetVisualNode(0x10000004);
            Assert.NotNull(node4); // Check if node is created 
            Assert.AreEqual(NodeType.Identifier, node4.VisualType);
        }

        [Test]
        public void Defect_IDE_1239()
        {
            // * [CRASH] Deleting a Node while in edit mode DSS is crashing.*
            // Double click to start Code Block node. ( No need to type anything.)
            // Now click on left side of Code Block node (inside node), to select it.
            // You will observe that node is still in edit mode but you can select the node as well.
            // Now press delete button or backspace key.
            // Now click outside or else where in canvas.
            // Above action will produces a crash.

            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10502.0|d:10245.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:1|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node = controller.GetVisualNode(0x10000001);

            commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:|b:False
                DeleteComponents";

            bool result01 = controller.RunCommands(commands);
            Assert.IsTrue(result01);
            
            Assert.AreEqual(0, controller.GetVisualNodes().Count());
        }

        [Test]
        public void Defect_IDE_1389()
        {
            // Deffect 1389
            // [CRASH] after deleting a node, when there is a warning message for Replication guides.
            // Drag and drop Range Node.
            // Add one layer of replication guides.
            // Edit the guide for first slot to something like "45454545" and press enter.
            // After above step DSS will pop up a message for "replication guide is not supported..."
            // Now press delete key withing clicking anywhere in the canvas.
            // now you will notice that Nodes is coming back after few second and clicking on the Range node causing DSS to crash.

            string commands = @"
                CreateFunctionNode|d:15492.0|d:15173.0|s:ProtoGeometry.dll|s:Point.ByCoordinates|s:double,double,double
                CreateRadialMenu|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|u:0x10000001                
                SelectMenuItem|i:1|d:15403.0|d:15117.0|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,ReplicationGuide|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,ReplicationGuide|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,ReplicationGuide|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,ReplicationGuide|i:1|e:System.Windows.Input.ModifierKeys,None
                EditReplicationGuide|u:0x10000001|i:0|i:1|s:a
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                CreateRadialMenu|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|u:0x10000001
                SelectMenuItem|i:3|d:15404.0|d:15147.0|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
            Assert.AreEqual(0, controller.GetVisualNodes().Count);
        }

        [Test]
        public void Defect_IDE_1413()
        {
            // IDE-1413 [CRASH] Deleting a node by selecting preview is causing crash.
            // Drag an drop Identifier or any other node. (except Code Block)
            // Select preview and press delete.
            // DSS Will crash after above step.

            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10355.0|d:10250.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:0|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateFunctionNode|d:10467.0|d:10256.0|s:ProtoGeometry.dll|s:Point.ByCoordinates|s:double,double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10379.0|d:10265.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10459.57142857|d:10265.57142857
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10385.85714286|d:10269.57142857
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:10456.71428571|d:10286.71428571
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10384.71428571|d:10268.42857143
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None|d:10459.0|d:10305.57142857
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Preview|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Preview|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents
                ";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            Assert.AreEqual(1, controller.GetVisualNodes().Count);
        }

        [Test]
        public void Defect_IDE_1665()
        {
            // test 1665
            // 1. create a driver node 
            // 2. select and delete the node
            // 3. Undo
            // 4. Select the node and delete again - it throws error Darn Something went wrong 

            string commands = @"
                CreateDriverNode|d:15438.0|d:15283.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents
                UndoOperation
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            Assert.AreEqual(0, controller.GetVisualNodes().Count);
        }

        [Test]
        public void Defect_IDE_1491()
        {
            // IDE-deffect-1413 
            //[CRASH] Deleting property node crashes DesignScript Studio (detail steps are mentioned in the defect.)


            string commands = @"
                CreateFunctionNode|d:15413.0|d:15194.0|s:ProtoGeometry.dll|s:Point.ByCoordinates|s:double,double,double
                CreateRadialMenu|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast|u:0x10000001
                SelectMenuItem|i:1015|d:15506.0|d:15201.0|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents
                ";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            Assert.AreEqual(1, controller.GetVisualNodes().Count);
        }

        [Test]
        public void Defect_IDE_1437()
        {
            // to test deffect -1437 
            // 1. Create a node with preview eg: identifier
            // 2. Select Node by clicking any where and move the cursor on to preview 
            // 3. Undo twice it crashes ds


            string commands = @"
                CreateIdentifierNode|d:15418.0|d:15228.0
                CreateCodeBlockNode|d:15315.0|d:15233.0|s:Your code goes here
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                EndNodeEdit|u:0x10000002|s:1|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15270.0|d:15234.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15310.0|d:15228.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15317.0|d:15231.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15391.0|d:15234.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents
                UndoOperation
                DeleteComponents
                UndoOperation
                ";
            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            

        }


    }
}