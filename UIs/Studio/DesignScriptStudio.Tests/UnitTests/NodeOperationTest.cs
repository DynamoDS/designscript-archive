using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;
using DesignScriptStudio.Graph.Core;

namespace DesignScriptStudio.Tests.UnitTests
{
    class NodeOperationTest
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
        public void Test01NodeSelection()
        {
            // 1. Create node.
            // 2. Click on the node to select it.
            // 
            string commands = @"
                CreateFunctionNode|d:516.0|d:225.0|s:Math.dll|s:Math.Cos|s:double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(true, ((VisualNode)node).Selected);
        }

        [Test]
        public void Test02NodeSelectionClearSelection()
        {
            // 1. Create node.
            // 2. Click on the node to select it.
            // 3. Click on the empty canvas to clear the selection
            // 
            string commands = @"
                CreateFunctionNode|d:516.0|d:225.0|s:Math.dll|s:Math.Cos|s:double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None

                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(false, ((VisualNode)node).Selected);
        }

        [Test]
        public void Test03CtrlNodeSelection()
        {
            // 1. Create two nodes.
            // 2. Select the first node.
            // 3. Hold Ctrl to select the second node
            // 4. Hold Ctrl to select the first node
            // 
            string commands = @"
                CreateFunctionNode|d:454.0|d:126.0|s:Math.dll|s:Math.Cos|s:double
                CreateFunctionNode|d:627.0|d:231.0|s:Math.dll|s:Math.Sin|s:double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(true, ((VisualNode)node).Selected);
            IVisualNode node2 = controller.GetVisualNode(0x10000002);
            Assert.AreEqual(false, ((VisualNode)node2).Selected);

            commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,Control
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,Control";

            bool result01 = controller.RunCommands(commands);
            Assert.AreEqual(true, result01);

            Assert.AreEqual(true, ((VisualNode)node).Selected);
            Assert.AreEqual(true, ((VisualNode)node2).Selected);

            commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,Control
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,Control";

            bool result02 = controller.RunCommands(commands);
            Assert.AreEqual(true, result02);

            Assert.AreEqual(false, ((VisualNode)node).Selected);
            Assert.AreEqual(true, ((VisualNode)node2).Selected);
        }

        [Test]
        public void Test04ShiftNodeSelection()
        {
            // 1. Create two nodes.
            // 2. Select the first node.
            // 3. Hold Shift to select the second node
            // 4. Hold Shift to select the first node
            // 
            string commands = @"
                CreateFunctionNode|d:454.0|d:126.0|s:Math.dll|s:Math.Cos|s:double
                CreateFunctionNode|d:627.0|d:231.0|s:Math.dll|s:Math.Sin|s:double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,Shift
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,Shift";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(true, ((VisualNode)node).Selected);
            IVisualNode node2 = controller.GetVisualNode(0x10000002);
            Assert.AreEqual(true, ((VisualNode)node2).Selected);

            commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,Shift
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,Shift";

            bool result02 = controller.RunCommands(commands);
            Assert.AreEqual(true, result02);

            Assert.AreEqual(true, ((VisualNode)node).Selected);
            Assert.AreEqual(true, ((VisualNode)node2).Selected);
        }

        [Test]
        public void Test05DragSelection()
        {
            // 1. Create one node.
            // 2. Start from the empty canvas to drag a box and covers the node.
            // 3. Release the mouse.
            // 
            string commands = @"
                CreateFunctionNode|d:496.0|d:185.0|s:Math.dll|s:Math.Cos|s:double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:431.0|d:135.0
                SelectComponent|u:0x10000001|e:System.Windows.Input.ModifierKeys,None
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:777.0|d:322.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(true, ((VisualNode)node).Selected);
        }

        [Test]
        public void Test06DragSelection01()
        {
            // 1. Create one node.
            // 2. Start from the empty canvas to drag a box and covers the node.
            // 3. Release the mouse.
            // 
            string commands = @"
                CreateFunctionNode|d:540.0|d:198.0|s:Math.dll|s:Math.Cos|s:double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:454.0|d:138.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:522.0|d:181.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";
            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(false, ((VisualNode)node).Selected);
        }

        [Test]
        public void Test07CtrlDragSelection()
        {
            // 1. Create two nodes.
            // 2. Select the first node.
            // 3. Hold Ctrl to select the two nodes

            string commands = @"
                CreateFunctionNode|d:525.0|d:177.0|s:Math.dll|s:Math.Cos|s:double
                CreateFunctionNode|d:811.0|d:278.0|s:Math.dll|s:Math.Sin|s:double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(true, ((VisualNode)node).Selected);
            IVisualNode node2 = controller.GetVisualNode(0x10000002);
            Assert.AreEqual(false, ((VisualNode)node2).Selected);

            commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,Control
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,Control|d:471.0|d:117.0
                SelectComponent|u:0x10000001|e:System.Windows.Input.ModifierKeys,Control
                SelectComponent|u:0x10000002|e:System.Windows.Input.ModifierKeys,Control
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,Control|d:987.0|d:387.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,Control";

            bool result01 = controller.RunCommands(commands);
            Assert.AreEqual(true, result01);

            Assert.AreEqual(false, ((VisualNode)node).Selected);
            Assert.AreEqual(true, ((VisualNode)node2).Selected);
        }

        [Test]
        public void Test08ShiftDragSelection()
        {
            // 1. Create two nodes.
            // 2. Select the first node.
            // 3. Hold Shift to select the two nodes

            string commands = @"
                CreateFunctionNode|d:409.0|d:214.0|s:Math.dll|s:Math.Cos|s:double
                CreateFunctionNode|d:695.0|d:291.0|s:Math.dll|s:Math.Sin|s:double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,Shift
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,Shift|d:378.0|d:176.0
                SelectComponent|u:0x10000001|e:System.Windows.Input.ModifierKeys,Shift
                SelectComponent|u:0x10000002|e:System.Windows.Input.ModifierKeys,Shift
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,Shift|d:923.0|d:395.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,Shift";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(true, ((VisualNode)node).Selected);
            IVisualNode node2 = controller.GetVisualNode(0x10000002);
            Assert.AreEqual(true, ((VisualNode)node2).Selected);
        }

        [Test]
        public void Test09NodeReposition()
        {
            // 1. Create node.
            // 2. Click on the node and move it.
            // 
            string commands = @"
                CreateFunctionNode|d:69.0|d:26.0|s:Math.dll|s:Math.Cos|s:double";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(69, node.X);
            Assert.AreEqual(26, node.Y);

            commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None|d:141.0|d:39.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None|d:646.0|d:387.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None";

            bool result01 = controller.RunCommands(commands);
            Assert.AreEqual(true, result01);

            Assert.AreEqual(574, node.X);
            Assert.AreEqual(374, node.Y);
            Assert.AreEqual(true, ((VisualNode)node).Selected);
        }

        [Test]
        public void Test10MultipleNodesReposition()
        {
            // 1. Create two nodes.
            // 2. select and and move them.
            // 
            string commands = @"
                CreateFunctionNode|d:454.0|d:173.0|s:Math.dll|s:Math.Cos|s:double
                CreateFunctionNode|d:469.0|d:243.0|s:Math.dll|s:Math.Sin|s:double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:381.0|d:112.0
                SelectComponent|u:0x10000001|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000002|e:System.Windows.Input.ModifierKeys,None
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:771.0|d:373.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(454, node.X);
            Assert.AreEqual(173, node.Y);
            Assert.AreEqual(true, ((VisualNode)node).Selected);

            IVisualNode node2 = controller.GetVisualNode(0x10000002);
            Assert.AreEqual(469, node2.X);
            Assert.AreEqual(243, node2.Y);
            Assert.AreEqual(true, ((VisualNode)node2).Selected);

            commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:581.0|d:260.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:690.0|d:366.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None";

            bool result01 = controller.RunCommands(commands);
            Assert.AreEqual(true, result01);

            Assert.AreEqual(563, node.X);
            Assert.AreEqual(279, node.Y);
            Assert.AreEqual(true, ((VisualNode)node).Selected);

            Assert.AreEqual(578, node2.X);
            Assert.AreEqual(349, node2.Y);
            Assert.AreEqual(true, ((VisualNode)node2).Selected);
        }

        [Test]
        public void Test11MultipleNodesReposition()
        {
            // 1. Create two nodes.
            // 2. Hold Ctrl and hit one of nodes, try to drag.
            // *The two nodes won't move as the hitted node is deselected
            // 
            string commands = @"
                CreateFunctionNode|d:413.0|d:247.0|s:Math.dll|s:Math.Tan|s:double
                CreateFunctionNode|d:503.0|d:327.0|s:Math.dll|s:Math.Sin|s:double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:347.0|d:186.0
                SelectComponent|u:0x10000001|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000002|e:System.Windows.Input.ModifierKeys,None
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:712.0|d:445.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(413, node.X);
            Assert.AreEqual(247, node.Y);
            Assert.AreEqual(true, ((VisualNode)node).Selected);

            IVisualNode node2 = controller.GetVisualNode(0x10000002);
            Assert.AreEqual(503, node2.X);
            Assert.AreEqual(327, node2.Y);
            Assert.AreEqual(true, ((VisualNode)node2).Selected);

            commands = @"
               MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,Control
               BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,Control|d:575.0|d:348.0
               EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,Control|d:756.0|d:491.0
               MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,Control";

            bool result01 = controller.RunCommands(commands);
            Assert.AreEqual(true, result01);

            Assert.AreEqual(413, node.X);
            Assert.AreEqual(247, node.Y);
            Assert.AreEqual(true, ((VisualNode)node).Selected);

            Assert.AreEqual(503, node2.X);
            Assert.AreEqual(327, node2.Y);
            Assert.AreEqual(false, ((VisualNode)node2).Selected);
        }

        [Test]
        public void Test12UndoRedoNodeReposition()
        {
            // 1. Create node.
            // 2. Click on the node and move it.
            // 3. Ctrl+Z to undo
            // 4. Ctrl+Y to redo
            // 
            string commands = @"
                CreateFunctionNode|d:69.0|d:26.0|s:Math.dll|s:Math.Cos|s:double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None|d:141.0|d:39.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None|d:646.0|d:387.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(574, node.X);
            Assert.AreEqual(374, node.Y);

            commands = @"UndoOperation";
            bool result01 = controller.RunCommands(commands);
            Assert.AreEqual(true, result01);

            Assert.AreEqual(69, node.X);
            Assert.AreEqual(26, node.Y);

            commands = @"RedoOperation";
            bool result02 = controller.RunCommands(commands);
            Assert.AreEqual(true, result02);

            Assert.AreEqual(574, node.X);
            Assert.AreEqual(374, node.Y);
        }

        [Test]
        public void Test13UndoRedoNodeEdit()
        {
            string commands = @"
                CreateFunctionNode|d:521.0|d:273.0|s:Math.dll|s:Math.Sin|s:double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:abc|b:True";

            GraphController graphController = new GraphController(null);
            bool result = graphController.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node = graphController.GetVisualNodes().ElementAt(0);

            commands = @"UndoOperation";
            bool result01 = graphController.RunCommands(commands);
            Assert.AreEqual(true, result01);
            Assert.AreNotEqual("abc", ((VisualNode)node).Text);

            commands = @"RedoOperation";
            bool result02 = graphController.RunCommands(commands);
            Assert.AreEqual(true, result02);
            Assert.AreEqual("abc", ((VisualNode)node).Text);
        }

        [Test]
        public void Test14CtrlDeSelection()
        {
            // 1. Create three nodes
            // 2. Select the nodes by window 
            // 3. Hold Ctrl and deslect one of the nodes
            //
            string commands = @"
                CreateCodeBlockNode|d:386.0|d:239.0|s:Double click and type
                CreateDriverNode|d:500.0|d:305.0
                CreateIdentifierNode|d:695.0|d:252.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:365.0|d:210.0
                SelectComponent|u:0x10000001|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000002|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000003|e:System.Windows.Input.ModifierKeys,None
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:772.0|d:370.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,Control
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,Control";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(true, ((VisualNode)node).Selected);
            IVisualNode node2 = controller.GetVisualNode(0x10000002);
            Assert.AreEqual(false, ((VisualNode)node2).Selected);
            IVisualNode node3 = controller.GetVisualNode(0x10000003);
            Assert.AreEqual(true, ((VisualNode)node3).Selected);
        }

        [Test]
        public void Test15CtrlWindowDeSelection()
        {
            // 1. Create two nodes.
            // 2. Select all the nodes
            // 3. Hold Ctrl to deselect one of the nodes
            // 
            string commands = @"
                CreateCodeBlockNode|d:393.0|d:208.0|s:Double click and type
                CreateDriverNode|d:586.0|d:284.0
                CreateIdentifierNode|d:792.0|d:204.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:342.0|d:135.0
                SelectComponent|u:0x10000001|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000002|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000003|e:System.Windows.Input.ModifierKeys,None
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:876.0|d:381.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,Control
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,Control|d:569.0|d:266.0
                SelectComponent|u:0x10000002|e:System.Windows.Input.ModifierKeys,Control
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,Control|d:711.0|d:344.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,Control";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(true, ((VisualNode)node).Selected);
            IVisualNode node2 = controller.GetVisualNode(0x10000002);
            Assert.AreEqual(false, ((VisualNode)node2).Selected);
            IVisualNode node3 = controller.GetVisualNode(0x10000003);
            Assert.AreEqual(true, ((VisualNode)node3).Selected);
        }

        [Test]
        public void Test16WindowSelectionAllNodes()
        {
            // 1. Create all nodes
            // 2. select all nodes using window
            //
            string commands = @"
                CreateCodeBlockNode|d:354.0|d:186.0|s:Double click and type
                CreateDriverNode|d:378.0|d:248.0
                CreateIdentifierNode|d:396.0|d:306.0
                CreateRenderNode|d:499.0|d:366.0
                CreateFunctionNode|d:634.0|d:342.0|s:|s:Range|s:double,double,double
                CreateFunctionNode|d:685.0|d:244.0|s:|s:+|s:double,double
                CreateFunctionNode|d:660.0|d:162.0|s:Math.dll|s:Math.Abs|s:double
                CreateFunctionNode|d:569.0|d:251.0|s:ProtoGeometry.dll|s:Solid.Revolve|s:Curve,Point,Vector,double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:318.0|d:81.0
                SelectComponent|u:0x10000001|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000002|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000003|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000004|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000005|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000006|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000007|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000008|e:System.Windows.Input.ModifierKeys,None
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:942.0|d:493.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(true, ((VisualNode)node).Selected);
            IVisualNode node2 = controller.GetVisualNode(0x10000002);
            Assert.AreEqual(true, ((VisualNode)node2).Selected);
            IVisualNode node3 = controller.GetVisualNode(0x10000003);
            Assert.AreEqual(true, ((VisualNode)node3).Selected);
            IVisualNode node4 = controller.GetVisualNode(0x10000004);
            Assert.AreEqual(true, ((VisualNode)node4).Selected);
            IVisualNode node5 = controller.GetVisualNode(0x10000005);
            Assert.AreEqual(true, ((VisualNode)node5).Selected);
            IVisualNode node6 = controller.GetVisualNode(0x10000006);
            Assert.AreEqual(true, ((VisualNode)node6).Selected);
            IVisualNode node7 = controller.GetVisualNode(0x10000007);
            Assert.AreEqual(true, ((VisualNode)node7).Selected);
            IVisualNode node8 = controller.GetVisualNode(0x10000008);
            Assert.AreEqual(true, ((VisualNode)node8).Selected);
        }

        [Test]
        public void Test17WindowDeSelection_2()
        {
            // 1. Create all nodes
            // 2. Select two nodes using window
            // 3. select 3rd using window - first two is expected to deselect 
            string commands = @"
                CreateCodeBlockNode|d:349.0|d:171.0|s:Double click and type
                CreateDriverNode|d:566.0|d:183.0
                CreateIdentifierNode|d:624.0|d:268.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:326.0|d:141.0
                SelectComponent|u:0x10000001|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000002|e:System.Windows.Input.ModifierKeys,None
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:690.0|d:232.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:607.0|d:251.0
                SelectComponent|u:0x10000003|e:System.Windows.Input.ModifierKeys,None
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:695.0|d:353.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(false, ((VisualNode)node).Selected);
            IVisualNode node2 = controller.GetVisualNode(0x10000002);
            Assert.AreEqual(false, ((VisualNode)node2).Selected);
            IVisualNode node3 = controller.GetVisualNode(0x10000003);
            Assert.AreEqual(true, ((VisualNode)node3).Selected);
        }

        [Test]
        public void Test18DeSelectionAllNodes()
        {
            // 1. Create all nodes
            // 2. select all nodes using window
            // 3. ctrl + window to deselct all nodes
            //
            string commands = @"
                CreateCodeBlockNode|d:342.0|d:243.0|s:Double click and type
                CreateDriverNode|d:343.0|d:284.0
                CreateIdentifierNode|d:346.0|d:211.0
                CreateRenderNode|d:357.0|d:152.0
                CreateFunctionNode|d:489.0|d:170.0|s:|s:Range|s:double,double,double
                CreateFunctionNode|d:525.0|d:297.0|s:|s:+|s:double,double
                CreateFunctionNode|d:376.0|d:348.0|s:ProtoGeometry.dll|s:Solid.Revolve|s:Curve,Point,Vector,double,double
                CreateFunctionNode|d:578.0|d:400.0|s:Math.dll|s:Math.Abs|s:int
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:299.0|d:123.1565762
                SelectComponent|u:0x10000001|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000002|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000003|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000004|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000005|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000006|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000007|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000008|e:System.Windows.Input.ModifierKeys,None
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:793.0|d:574.1565762
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,Control
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,Control|d:326.0|d:137.1565762
                SelectComponent|u:0x10000001|e:System.Windows.Input.ModifierKeys,Control
                SelectComponent|u:0x10000002|e:System.Windows.Input.ModifierKeys,Control
                SelectComponent|u:0x10000003|e:System.Windows.Input.ModifierKeys,Control
                SelectComponent|u:0x10000004|e:System.Windows.Input.ModifierKeys,Control
                SelectComponent|u:0x10000005|e:System.Windows.Input.ModifierKeys,Control
                SelectComponent|u:0x10000006|e:System.Windows.Input.ModifierKeys,Control
                SelectComponent|u:0x10000007|e:System.Windows.Input.ModifierKeys,Control
                SelectComponent|u:0x10000008|e:System.Windows.Input.ModifierKeys,Control
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,Control|d:768.0|d:521.1565762
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,Control";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(false, ((VisualNode)node).Selected);
            IVisualNode node2 = controller.GetVisualNode(0x10000002);
            Assert.AreEqual(false, ((VisualNode)node2).Selected);
            IVisualNode node3 = controller.GetVisualNode(0x10000003);
            Assert.AreEqual(false, ((VisualNode)node3).Selected);
            IVisualNode node4 = controller.GetVisualNode(0x10000004);
            Assert.AreEqual(false, ((VisualNode)node4).Selected);
            IVisualNode node5 = controller.GetVisualNode(0x10000005);
            Assert.AreEqual(false, ((VisualNode)node5).Selected);
            IVisualNode node6 = controller.GetVisualNode(0x10000006);
            Assert.AreEqual(false, ((VisualNode)node6).Selected);
            IVisualNode node7 = controller.GetVisualNode(0x10000007);
            Assert.AreEqual(false, ((VisualNode)node7).Selected);
            IVisualNode node8 = controller.GetVisualNode(0x10000008);
            Assert.AreEqual(false, ((VisualNode)node8).Selected);
        }

        [Test]
        public void Test19WindowDeSelection_2()
        {
            // 1. Create few nodes nodes
            // 2. Select two nodes using window
            // 3. select 3rd using window - first two is expected to deselect 
            string commands = @"
                CreateCodeBlockNode|d:349.0|d:171.0|s:Double click and type
                CreateDriverNode|d:566.0|d:183.0
                CreateIdentifierNode|d:624.0|d:268.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:326.0|d:141.0
                SelectComponent|u:0x10000001|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000002|e:System.Windows.Input.ModifierKeys,None
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:690.0|d:232.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:607.0|d:251.0
                SelectComponent|u:0x10000003|e:System.Windows.Input.ModifierKeys,None
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:695.0|d:353.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(false, ((VisualNode)node).Selected);
            IVisualNode node2 = controller.GetVisualNode(0x10000002);
            Assert.AreEqual(false, ((VisualNode)node2).Selected);
            IVisualNode node3 = controller.GetVisualNode(0x10000003);
            Assert.AreEqual(true, ((VisualNode)node3).Selected);
        }

        [Test]
        public void Test20clickoutsideDeselect()
        {

            // 1. Create multiple nodes
            // 2. Select a few using window
            // 3. click outside the are supposed to deselect
            //
            string commands = @"
                CreateCodeBlockNode|d:314.0|d:191.0|s:Double click and type
                CreateDriverNode|d:328.0|d:236.0
                CreateIdentifierNode|d:358.0|d:286.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:302.0|d:178.0
                SelectComponent|u:0x10000001|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000002|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000003|e:System.Windows.Input.ModifierKeys,None
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:497.0|d:397.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(false, ((VisualNode)node).Selected);
            IVisualNode node2 = controller.GetVisualNode(0x10000002);
            Assert.AreEqual(false, ((VisualNode)node2).Selected);
            IVisualNode node3 = controller.GetVisualNode(0x10000003);
            Assert.AreEqual(false, ((VisualNode)node3).Selected);
        }

        [Test]
        public void Test21CtrlShiftSelection()
        {

            // 1. Create multiple nodes
            // 2. Select a few using window
            // 3. Hold ctrl +shift and select the other nodes
            //
            string commands = @"
                CreateCodeBlockNode|d:333.0|d:127.0|s:Double click and type
                CreateDriverNode|d:343.0|d:169.0
                CreateIdentifierNode|d:552.0|d:131.0
                CreateFunctionNode|d:556.0|d:220.0|s:|s:+|s:double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:321.0|d:114.0
                SelectComponent|u:0x10000001|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000002|e:System.Windows.Input.ModifierKeys,None
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:512.0|d:226.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,Control, Shift
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,Control, Shift
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,Control, Shift
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,Control, Shift";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(true, ((VisualNode)node).Selected);
            IVisualNode node2 = controller.GetVisualNode(0x10000002);
            Assert.AreEqual(true, ((VisualNode)node2).Selected);
            IVisualNode node3 = controller.GetVisualNode(0x10000003);
            Assert.AreEqual(true, ((VisualNode)node3).Selected);
            IVisualNode node4 = controller.GetVisualNode(0x10000004);
            Assert.AreEqual(true, ((VisualNode)node4).Selected);
        }

        [Test]
        public void Test22CtrlShiftDeselection()
        {

            // 1. Create multiple nodes
            // 2. Select all using window
            // 3. Hold ctrl +shift and select the other nodes
            //
            string commands = @"
                CreateCodeBlockNode|d:363.0|d:138.0|s:Double click and type
                CreateDriverNode|d:569.0|d:152.0
                CreateIdentifierNode|d:366.0|d:207.0
                CreateFunctionNode|d:579.0|d:221.0|s:|s:+|s:double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:343.0|d:122.0
                SelectComponent|u:0x10000001|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000002|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000003|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000004|e:System.Windows.Input.ModifierKeys,None
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:771.0|d:332.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,Control, Shift
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,Control, Shift
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,Control, Shift
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,Control, Shift";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(true, ((VisualNode)node).Selected);
            IVisualNode node2 = controller.GetVisualNode(0x10000002);
            Assert.AreEqual(false, ((VisualNode)node2).Selected);
            IVisualNode node3 = controller.GetVisualNode(0x10000003);
            Assert.AreEqual(true, ((VisualNode)node3).Selected);
            IVisualNode node4 = controller.GetVisualNode(0x10000004);
            Assert.AreEqual(false, ((VisualNode)node4).Selected);
        }

        [Test]
        public void Test23windowDeselection()
        {
            // 1. Create multiple nodes
            // 2. Select all using window
            // 3. select only two using window again-
            //
            string commands = @"
                CreateCodeBlockNode|d:374.0|d:137.0|s:Double click and type
                CreateDriverNode|d:381.0|d:197.0
                CreateIdentifierNode|d:614.0|d:144.0
                CreateFunctionNode|d:620.0|d:226.0|s:|s:+|s:double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:363.0|d:112.0
                SelectComponent|u:0x10000001|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000002|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000003|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000004|e:System.Windows.Input.ModifierKeys,None
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:876.0|d:370.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:605.0|d:118.0
                SelectComponent|u:0x10000003|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000004|e:System.Windows.Input.ModifierKeys,None
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:843.0|d:358.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(false, ((VisualNode)node).Selected);
            IVisualNode node2 = controller.GetVisualNode(0x10000002);
            Assert.AreEqual(false, ((VisualNode)node2).Selected);
            IVisualNode node3 = controller.GetVisualNode(0x10000003);
            Assert.AreEqual(true, ((VisualNode)node3).Selected);
            IVisualNode node4 = controller.GetVisualNode(0x10000004);
            Assert.AreEqual(true, ((VisualNode)node4).Selected);
        }

        [Test]
        public void Test24clickAnotherNodeToDeselect()
        {

            // 1. Create multiple nodes
            // 2. Select a few using window
            // 3. click outside the are supposed to deselect
            //
            string commands = @"
                CreateCodeBlockNode|d:351.0|d:186.0|s:Double click and type
                CreateDriverNode|d:360.0|d:229.0
                CreateIdentifierNode|d:599.0|d:208.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:330.0|d:170.0
                SelectComponent|u:0x10000001|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000002|e:System.Windows.Input.ModifierKeys,None
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:526.0|d:290.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(false, ((VisualNode)node).Selected);
            IVisualNode node2 = controller.GetVisualNode(0x10000002);
            Assert.AreEqual(false, ((VisualNode)node2).Selected);
            IVisualNode node3 = controller.GetVisualNode(0x10000003);
            Assert.AreEqual(true, ((VisualNode)node3).Selected);
        }

        [Test]
        public void Test25SelectbyCtrlReposition()
        {
            // 1. Create node.
            // 2. Click on the node and move it.
            //
            string commands = @"
                CreateCodeBlockNode|d:321.0|d:161.0|s:Double click and type
                CreateDriverNode|d:345.0|d:210.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,Control
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,Control
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None|d:364.0|d:228.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None|d:651.0|d:229.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(608, node.X);
            Assert.AreEqual(162, node.Y);

            IVisualNode node2 = controller.GetVisualNode(0x10000002);
            Assert.AreEqual(632, node2.X);
            Assert.AreEqual(211, node2.Y);
            Assert.AreEqual(true, ((VisualNode)node2).Selected);
        }

        [Test]
        public void Test26SelectbyShiftReposition()
        {
            // 1. Create two nodes.
            // 2. Select by Ctrl + click
            // 3. Click on the node and move it
            //
            string commands = @"
                CreateCodeBlockNode|d:410.0|d:191.0|s:Double Click and type
                CreateDriverNode|d:421.0|d:259.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,Shift
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,Shift
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None|d:478.0|d:208.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None|d:640.0|d:212.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(572, node.X);
            Assert.AreEqual(195, node.Y);

            IVisualNode node2 = controller.GetVisualNode(0x10000002);
            Assert.AreEqual(583, node2.X);
            Assert.AreEqual(263, node2.Y);
            Assert.AreEqual(true, ((VisualNode)node2).Selected);
        }

        [Test]
        public void Test27SelectbyCtrlShiftReposition()
        {
            // 1. Create two nodes.
            // 2. Select by Ctrl +Shift+ click
            // 3. Click on the node and move it
            //
            string commands = @"
                CreateFunctionNode|d:375.0|d:299.0|s:|s:+|s:double,double
                CreateFunctionNode|d:421.0|d:243.0|s:|s:Range|s:double,double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,Control, Shift
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,Control, Shift
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:413.0|d:322.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:587.0|d:288.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(549, node.X);
            Assert.AreEqual(265, node.Y);

            IVisualNode node2 = controller.GetVisualNode(0x10000002);
            Assert.AreEqual(595, node2.X);
            Assert.AreEqual(209, node2.Y);
            Assert.AreEqual(true, ((VisualNode)node2).Selected);
        }

        [Test]
        public void Test28RepositionAfterDeselect()
        {
            // 1. Create three nodes.
            // 2. Select all by window
            // 3. Deselect one of  them and move it 
            //
            string commands = @"
                CreateCodeBlockNode|d:387.0|d:209.0|s:Double Click and type
                CreateFunctionNode|d:394.0|d:271.0|s:|s:Range|s:double,double,double
                CreateFunctionNode|d:403.0|d:395.0|s:|s:+|s:double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:372.0|d:175.0
                SelectComponent|u:0x10000001|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000002|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000003|e:System.Windows.Input.ModifierKeys,None
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:594.0|d:489.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,Control
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,Control
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:420.0|d:325.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:595.0|d:322.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualNode node = controller.GetVisualNode(0x10000001);

            Assert.AreEqual(387, node.X);
            Assert.AreEqual(209, node.Y);

            IVisualNode node2 = controller.GetVisualNode(0x10000002);
            Assert.AreEqual(569, node2.X);
            Assert.AreEqual(268, node2.Y);
            Assert.AreEqual(true, ((VisualNode)node2).Selected);

            IVisualNode node3 = controller.GetVisualNode(0x10000003);
            Assert.AreEqual(578, node3.X);
            Assert.AreEqual(392, node3.Y);
            Assert.AreEqual(true, ((VisualNode)node3).Selected);
        }

        [Test]
        public void Test29RepositionAfterUndoDelete()
        {
            // 1. Create three nodes.
            // 2. Select all by window and delete
            // 3. Undo deletion
            // 4. Drag (All node should be moving together)
            //
            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10350.0|d:10117.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:100|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateDriverNode|d:10496.0|d:10150.0
                CreateIdentifierNode|d:10372.0|d:10194.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:10294.0|d:10086.0
                SelectComponent|u:0x10000001|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000002|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000003|e:System.Windows.Input.ModifierKeys,None
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:10681.0|d:10328.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents
                UndoOperation
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None|d:10514.0|d:10165.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None|d:10503.0|d:10352.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(10339, node.X);
            Assert.AreEqual(10304, node.Y);
            Assert.AreEqual(true, ((VisualNode)node).Selected);

            IVisualNode node2 = controller.GetVisualNode(0x10000002);
            Assert.AreEqual(10485, node2.X);
            Assert.AreEqual(10337, node2.Y);
            Assert.AreEqual(true, ((VisualNode)node2).Selected);

            IVisualNode node3 = controller.GetVisualNode(0x10000003);
            Assert.AreEqual(10361, node3.X);
            Assert.AreEqual(10381, node3.Y);
            Assert.AreEqual(true, ((VisualNode)node3).Selected);
        }

        [Test]
        public void Test30UndoReposition()
        {
            // 1. Create a node.
            // 2. drag twice 
            // 3. Undo once
            //
            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10400.0|d:10121.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:a = 10;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:10437.0|d:10139.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:10611.0|d:10190.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:10598.0|d:10187.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:10448.0|d:10241.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(10424, node.X);
            Assert.AreEqual(10226, node.Y);

            commands = @"UndoOperation";
            bool result01 = controller.RunCommands(commands);
            Assert.AreEqual(true, result01);

            IVisualNode node2 = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(10574, node2.X);
            Assert.AreEqual(10172, node2.Y);
            Assert.AreEqual(true, ((VisualNode)node2).Selected);
        }

        [Test]
        public void Test32RedoReposition()
        {
            // 1. Create a node.
            // 2. drag twice 
            // 3. Undo once
            // 4. Redo once
            //
            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10400.0|d:10121.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:a = 10;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:10437.0|d:10139.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:10611.0|d:10190.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:10598.0|d:10187.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:10448.0|d:10241.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(10424, node.X);
            Assert.AreEqual(10226, node.Y);

            commands = @"UndoOperation";
            bool result01 = controller.RunCommands(commands);
            Assert.AreEqual(true, result01);

            IVisualNode node2 = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(10574, node2.X);
            Assert.AreEqual(10172, node2.Y);
            Assert.AreEqual(true, ((VisualNode)node2).Selected);

            commands = @"RedoOperation";
            bool result02 = controller.RunCommands(commands);
            Assert.AreEqual(true, result02);

            IVisualNode node3 = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(10424, node3.X);
            Assert.AreEqual(10226, node3.Y);
            Assert.AreEqual(true, ((VisualNode)node3).Selected);
        }

        [Test]
        public void Test33RepositionConnectedNodes()
        {
            // 1. Create some nodes and connect them.(two code block-> connected to add ->connected to identifier)
            // 2. move one of the nodes
            // 3. undo 
            // 4, redo
            //
            string commands = @"
                CreateCodeBlockNode|d:361.0|d:205.0|s:Double Click and type
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:100|b:True
                CreateCodeBlockNode|d:369.0|d:259.0|s:Double Click and type
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:200|b:True
                CreateFunctionNode|d:540.0|d:236.0|s:|s:+|s:double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:394.0|d:221.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:543.0|d:251.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:401.0|d:274.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:541.0|d:264.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                CreateIdentifierNode|d:796.0|d:247.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:663.0|d:269.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:799.0|d:262.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualNode node = controller.GetVisualNode(0x10000003);
            Assert.AreEqual(540, node.X);
            Assert.AreEqual(236, node.Y);

            commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:590.0|d:259.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:582.0|d:133.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None";
            bool result01 = controller.RunCommands(commands);
            Assert.AreEqual(true, result01);

            IVisualNode node2 = controller.GetVisualNode(0x10000003);
            Assert.AreEqual(532, node2.X);
            Assert.AreEqual(110, node2.Y);
            Assert.AreEqual(true, ((VisualNode)node2).Selected);

            commands = @"UndoOperation";
            bool result02 = controller.RunCommands(commands);
            Assert.AreEqual(true, result02);

            IVisualNode node3 = controller.GetVisualNode(0x10000003);
            Assert.AreEqual(540, node3.X);
            Assert.AreEqual(236, node3.Y);
            Assert.AreEqual(true, ((VisualNode)node3).Selected);

            commands = @"RedoOperation";
            bool result03 = controller.RunCommands(commands);
            Assert.AreEqual(true, result03);

            IVisualNode node4 = controller.GetVisualNode(0x10000003);
            Assert.AreEqual(532, node4.X);
            Assert.AreEqual(110, node4.Y);
            Assert.AreEqual(true, ((VisualNode)node4).Selected);
        }

        [Test]
        public void Test34MultipleConnectionsDrag()
        {
            // 1. Create couple of nodes and connect them(code block to identifier , two sets)
            // 2. Click on two nodes and move them
            // 3. undo
            // 4. redo
            //
            string commands = @"
                CreateCodeBlockNode|d:408.0|d:249.0|s:Double Click and type
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:100|b:True
                CreateCodeBlockNode|d:415.0|d:297.0|s:Double Click and type
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:200|b:True
                CreateIdentifierNode|d:551.0|d:279.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:442.0|d:264.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:553.0|d:293.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                CreateIdentifierNode|d:550.0|d:349.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:447.0|d:314.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:556.0|d:365.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,Control
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,Control
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None|d:563.0|d:368.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None|d:570.0|d:472.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result01 = controller.RunCommands(commands);
            Assert.AreEqual(true, result01);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(415, node.X);
            Assert.AreEqual(353, node.Y);

            IVisualNode node2 = controller.GetVisualNode(0x10000004);
            Assert.AreEqual(557, node2.X);
            Assert.AreEqual(453, node2.Y);

            commands = @"UndoOperation";
            bool result02 = controller.RunCommands(commands);
            Assert.AreEqual(true, result02);

            IVisualNode node3 = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(408, node3.X);
            Assert.AreEqual(249, node3.Y);
            Assert.AreEqual(true, ((VisualNode)node3).Selected);

            IVisualNode node4 = controller.GetVisualNode(0x10000004);
            Assert.AreEqual(550, node4.X);
            Assert.AreEqual(349, node4.Y);
            Assert.AreEqual(true, ((VisualNode)node4).Selected);

            commands = @"RedoOperation";
            bool result03 = controller.RunCommands(commands);
            Assert.AreEqual(true, result03);

            IVisualNode node5 = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(415, node5.X);
            Assert.AreEqual(353, node5.Y);
            Assert.AreEqual(true, ((VisualNode)node5).Selected);

            IVisualNode node6 = controller.GetVisualNode(0x10000004);
            Assert.AreEqual(557, node6.X);
            Assert.AreEqual(453, node6.Y);
            Assert.AreEqual(true, ((VisualNode)node6).Selected);
        }

        [Test]
        public void Test35MultipleConnectionsNodeRepositoning()
        {
            // 1. create two code block node connect the same to add and which is connected to identifier
            // 2. move function node(add)
            //
            string commands = @"
                CreateCodeBlockNode|d:406.0|d:247.0|s:Double Click and type
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:100|b:True
                CreateFunctionNode|d:531.0|d:265.0|s:|s:Add|s:double,double
                CreateCodeBlockNode|d:408.0|d:334.0|s:Double Click and type
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000003|s:200|b:True
                CreateIdentifierNode|d:860.0|d:292.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:439.0|d:260.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:536.0|d:278.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:437.0|d:347.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:535.0|d:289.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:692.0|d:292.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:866.0|d:307.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:571.0|d:293.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:566.0|d:151.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualNode node = controller.GetVisualNode(0x10000002);

            Assert.AreEqual(526, node.X);
            Assert.AreEqual(123, node.Y);
        }

        [Test]
        public void Test36MultipleConnectionsNodeRepositoning()
        {
            // 1. Create couple of nodes and connect them(identifier, value, fucntion nodes)
            // 2. Click on the identifier  node and move it so that the connection line crosses over each other

            string commands = @"
                CreateCodeBlockNode|d:367.0|d:233.0|s:Double Click and type
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:100|b:True
                CreateCodeBlockNode|d:368.0|d:311.0|s:Double Click and type
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:200|b:True
                CreateFunctionNode|d:477.0|d:276.0|s:|s:Add|s:double,double
                CreateIdentifierNode|d:779.0|d:293.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:399.0|d:247.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:480.0|d:291.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:401.0|d:327.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:483.0|d:301.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:640.0|d:305.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:783.0|d:306.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,Control
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,Control
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,Control
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,Control
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:632.0|d:291.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:640.0|d:381.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualNode node = controller.GetVisualNode(0x10000003);
            Assert.AreEqual(485, node.X);
            Assert.AreEqual(366, node.Y);
            IVisualNode node2 = controller.GetVisualNode(0x10000004);
            Assert.AreEqual(787, node2.X);
            Assert.AreEqual(383, node2.Y);
        }

        [Test]
        public void Test37MultipleConnectionsNodeRepositoning()
        {
            // 1. Create couple of nodes and connect them(code block ,identifier, )
            // 2. Click on the literal 'value' node and move it so that the connection line intersects
            //
            string commands = @"
                CreateCodeBlockNode|d:334.0|d:245.0|s:Double Click and type
                CreateIdentifierNode|d:589.0|d:257.0
                CreateCodeBlockNode|d:367.0|d:342.0|s:Double Click and type
                CreateIdentifierNode|d:614.0|d:351.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:100|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000003|s:200|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:363.0|d:261.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:592.0|d:274.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:398.0|d:357.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:622.0|d:366.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None|d:628.0|d:363.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None|d:557.0|d:191.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualNode node = controller.GetVisualNode(0x10000001);

            Assert.AreEqual(334, node.X);
            Assert.AreEqual(245, node.Y);
            IVisualNode node2 = controller.GetVisualNode(0x10000002);

            Assert.AreEqual(589, node2.X);
            Assert.AreEqual(257, node2.Y);
            IVisualNode node3 = controller.GetVisualNode(0x10000003);

            Assert.AreEqual(367, node3.X);
            Assert.AreEqual(342, node3.Y);
            IVisualNode node4 = controller.GetVisualNode(0x10000004);

            Assert.AreEqual(543, node4.X);
            Assert.AreEqual(179, node4.Y);
        }

        [Test]
        public void Test38CtrlZAndCtrlY()
        {
            // 1. Create 2 identifier nodes
            // 2. Connect output of first to output of second
            // 3. type ctrl+z -> the connection goes away
            // 4. type ctrl+y -> the connection comes back
            //
            string commands = @"
                CreateIdentifierNode|d:530.0|d:179.0
                CreateIdentifierNode|d:751.0|d:235.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:567.0|d:195.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:753.0|d:246.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                UndoOperation
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                RedoOperation";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualEdge edge = controller.GetVisualEdge(0x60000001);
            Assert.IsNotNull(edge);
        }

        [Test]
        public void Test39Delete()
        {
            // 1. Create 2 identifier nodes
            // 2. Connect output of second  to input of first
            // 3. select the first and press delete key
            //
            string commands = @"
            CreateIdentifierNode|d:774.0|d:210.0
            CreateIdentifierNode|d:545.0|d:262.0
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:580.0|d:280.0
            EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:774.0|d:223.0
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
            MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
            DeleteComponents";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualNode node = controller.GetVisualNode(0x10000002);
            Assert.AreEqual(545, node.X);
            Assert.AreEqual(262, node.Y);
            node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(null, node);
        }

        [Test]
        public void Test40PreviewToggle()
        {
            // 1. create a function node
            // 2. click on the preview bubble
            // 3. click on the preview bubble again

            string commands = @"CreateFunctionNode|d:515.0|d:235.0|s:DesignScriptStudio.Tests.dll|s:Vehicle.ByBrand|s:string
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Preview|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Preview|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            VisualNode node = (VisualNode)controller.GetVisualNode(0x10000001);
            Assert.AreEqual(true, node.Selected);

            commands = @" MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Preview|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Preview|i:-1|e:System.Windows.Input.ModifierKeys,None";

            bool result01 = controller.RunCommands(commands);
            Assert.AreEqual(true, result01);
            Assert.AreEqual(true, node.Selected);
        }

        [Test]
        public void Test41UseEcsKeyToDeselect()
        {
            // 1. create a literal node
            // 2. click on it to select it
            // 3. press ESC key to deselct it

            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:501.0|d:270.0|s:0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            VisualNode node = (VisualNode)controller.GetVisualNode(0x10000001);
            Assert.AreEqual(true, node.Selected);

            commands = @"ClearSelection";
            bool result01 = controller.RunCommands(commands);
            Assert.AreEqual(true, result01);
            Assert.AreEqual(false, node.Selected);
        }

        [Test]
        public void Test42ClickOnCanvasToDeselect()
        {
            // 1. create 2 identifier nodes
            // 2. click on both using shift key to select both
            // 3. Click on canvas to deselct

            string commands = @"CreateIdentifierNode|d:597.0|d:162.0
                CreateIdentifierNode|d:775.0|d:261.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:632.0|d:178.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:774.0|d:277.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,Shift
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,Shift";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            VisualNode node1 = (VisualNode)controller.GetVisualNode(0x10000001);
            Assert.AreEqual(true, node1.Selected);
            VisualNode node2 = (VisualNode)controller.GetVisualNode(0x10000001);
            Assert.AreEqual(true, node2.Selected);

            commands = @"ClearSelection";

            bool result01 = controller.RunCommands(commands);
            Assert.AreEqual(true, result01);
            Assert.AreEqual(false, node1.Selected);
            Assert.AreEqual(false, node2.Selected);
        }

        [Test]
        public void Test43VerifyCaptionNameEditOnIdentifierNode()
        {
            //1. Create an identifier node 
            //2. Edit the caption to 'a'            
            string commands = @"CreateIdentifierNode|d:569.0|d:169.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:a|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            VisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(569, node.X);
            Assert.AreEqual(169, node.Y);
            Assert.AreEqual("a", node.Caption);
            Assert.AreEqual(false, node.Selected);
        }


        [Test]
        public void Test44VerifyLiteralNodeEdit()
        {
            //1. Create  a literal node of value '0'
            //2. Rename the text on node to '1'
            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:490.0|d:305.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:0|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:1|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            VisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(490, node.X);
            Assert.AreEqual(305, node.Y);
            Assert.AreEqual(false, node.Selected);
            Assert.AreEqual("1;", node.Text);
        }

        [Test]
        public void Test45EditTextOnAddNode()
        {
            //1. Drop the Add function node from the User Defined  library to the canvas
            //2. Rename the text on node to 'add'
            string commands = @"
                CreateFunctionNode|d:657.0|d:257.0|s:|s:Add|s:double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:add|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            VisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(657, node.X);
            Assert.AreEqual(257, node.Y);
            Assert.AreEqual("Add", node.Caption);
            Assert.AreEqual(false, node.Selected);
            Assert.AreEqual("add", node.Text);
        }

        [Test]
        public void Test46EditTextOnRangeNode()
        {
            //1. Drop the Range node from the User Defined Library to the canvas
            //2. Rename the text on range node to '1..2'
            string commands = @"
                CreateFunctionNode|d:561.0|d:157.0|s:|s:Range|s:double,double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:1..2|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            VisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(561, node.X);
            Assert.AreEqual(157, node.Y);
            Assert.AreEqual("Range", node.Caption);
            Assert.AreEqual("1..2", node.Text);
            Assert.AreEqual(false, node.Selected);
        }


        [Test]
        public void Test47EditTextOnFunctionNode_ByRGB()
        {
            //1. Drop the ByRGB function node from the Geometry library to the canvas
            //2. Rename the text on function node to 'RED'
            string commands = @"
                CreateFunctionNode|d:527.0|d:310.0|s:ProtoGeometry.dll|s:Color.ByARGB|s:int,int,int,int
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:RED|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            VisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(527, node.X);
            Assert.AreEqual(310, node.Y);
            Assert.AreEqual("Color", node.Caption);
            Assert.AreEqual("RED", node.Text);
            Assert.AreEqual(false, node.Selected);
        }

        [Test]
        public void Test48EditTextOnFunctionNode_DivRem()
        {
            //1. Drop the DivRem function node from the math library to the canvas
            //2. Rename the text on DivRem function node to 'gg'
            string commands = @"
                CreateFunctionNode|d:652.0|d:295.0|s:Math.dll|s:Math.DivRem|s:int,int
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:gg|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            VisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(652, node.X);
            Assert.AreEqual(295, node.Y);
            Assert.AreEqual("DivRem", node.Caption);
            Assert.AreEqual("gg", node.Text);
            Assert.AreEqual(false, node.Selected);
        }

        [Test]
        public void Test49Codeblock987()
        {
            //1. Create code block node
            //2. Enter multiline text 
            //3. Move the cursor to the middle and press enter
            //
            string commands = @"
                CreateCodeBlockNode|d:499.0|d:287.0|s:Double Click and type
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:fsfsssdfsfsf;\nsfsds\nfsfsf;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            VisualNode node = (VisualNode)controller.GetVisualNode(0x10000001);
            Assert.AreEqual(499.0, node.X);
            Assert.AreEqual(287.0, node.Y);
            var test = node.Text;
            Assert.AreEqual("fsfsssdfsfsf;\nsfsds\nfsfsf;", node.Text);
        }

        [Test]
        public void Test49Codeblock987_2()
        {
            //1. Create code block node
            //2. Enter multiline text 
            //3. Move the cursor to the middle, enter semicolon and press enter
            //
            string commands = @"
                CreateCodeBlockNode|d:499.0|d:287.0|s:Double Click and type
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:fsfsssdfsfsf;\nsfsds;\nfsfsf;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            VisualNode node = (VisualNode)controller.GetVisualNode(0x10000001);
            Assert.AreEqual(499, node.X);
            Assert.AreEqual(287, node.Y);
            string test = node.Text;
            Assert.AreEqual("fsfsssdfsfsf;\nsfsds;\nfsfsf;", node.Text);
        }

        [Test]
        public void Test50EditTextOnCodeBlock()
        {
            //1. Create code block node
            //2. Enter multiline text ( a = 1; b = a + 1" ) => verify this
            //3. The again double click on the code block, edit the text to delete the last line and then add "b = 2"
            //
            string commands = @"
                CreateCodeBlockNode|d:567.0|d:184.0|s:Double Click and type
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:a = 1;\nb = a + 1;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            VisualNode node = (VisualNode)controller.GetVisualNode(0x10000001);
            Assert.AreEqual(567, node.X);
            Assert.AreEqual(184, node.Y);
            Assert.AreEqual(false, node.Selected);
            Assert.AreEqual("a = 1;\nb = a + 1;", node.Text);

            commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:a = 1;\nb = 2;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);
            Assert.AreEqual("a = 1;\nb = 2;", node.Text);
            Assert.AreNotEqual("a = 1;\nb = a + 1;", node.Text);
        }

        [Test]
        public void TestNodeSelection_IDE1025()
        {
            // 1. Create code block and identifier node
            // 2. Connect them and select the edge
            // 
            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10346.0|d:10108.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:100|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateIdentifierNode|d:10516.0|d:10111.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10382.0|d:10124.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10511.0|d:10126.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x60000001|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualEdge edge = controller.GetVisualEdge(0x60000001);
            Assert.AreEqual(true, ((VisualEdge)edge).Selected);
        }

        [Test]
        public void TestClickThreeDots_IDE1026()
        {
            // 1. Create node.
            // 2. Click on the node to select it.
            // 
            string commands = @"
                CreateFunctionNode|d:10400.0|d:10314.0|s:|s:Range|s:double,double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual(true, ((VisualNode)node).Selected);
        }

        [Test]
        public void Defect_IDE_1385()
        {
            // create driver node and connect the output slot to function
            string commands = @"
                CreateFunctionNode|d:10547.0|d:10136.0|s:|s:Range|s:double,double,double
                CreateIdentifierNode|d:10272.0|d:10059.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10312.0|d:10071.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10541.0|d:10149.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = controller.GetVisualNode(0x10000001);
            IVisualNode node02 = controller.GetVisualNode(0x10000002);

            //IVisualEdge edge = controller.GetVisualEdge(0x60000001);
            //Assert.IsTrue((IVisualNode)node02).Selected);
            Assert.AreEqual(true, ((VisualNode)node02).Selected);
            // Assert.AreEqual(true, ((IVisualEdge)edge).Selected);
        }

        [Test]
        public void Defect_IDE_1297()
        {
            // create a codeblock node a =1 connect it to identifier
            // Select any node and click on output slot , a new connection msut be avaialble
            // connect the above to a new identifier
            string commands = @"
                CreateCodeBlockNode|d:10374.0|d:10250.0|s:Your code goes here
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                EndNodeEdit|u:0x10000001|s:a=1|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateIdentifierNode|d:10532.0|d:10252.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10418.0|d:10265.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10525.0|d:10269.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                CreateIdentifierNode|d:10549.0|d:10176.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10419.0|d:10264.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10545.0|d:10192.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualEdge edge01 = controller.GetVisualEdge(0x60000001);
            IVisualEdge edge02 = controller.GetVisualEdge(0x60000002);

            Assert.IsNotNull(edge01);
            Assert.IsNotNull(edge02);
        }

        [Test]
        public void Defect_IDE_1377()
        {
            // 1. Create a codeblock node a =1 connect it to identifier
            // 2. Connect it to the identifier 
            // 3. select the connection and move it in the canvas 
            // 4. Ctrl +Z to undo 
            // 5. repeat step 3 above 

            string commands = @"
                CreateCodeBlockNode|d:10297.0|d:10210.1|s:Your code goes here
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                EndNodeEdit|u:0x10000001|s:a=1|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateIdentifierNode|d:10454.0|d:10191.1
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10342.0|d:10224.1
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10444.0|d:10205.1
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10294.0|d:10287.1|s:Your code goes here
                BeginNodeEdit|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text
                EndNodeEdit|u:0x10000003|s:a=2|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000003|s:b = 2;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateIdentifierNode|d:10466.0|d:10290.1
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10342.0|d:10301.1
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10455.0|d:10304.1
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                ClearSelection
                SelectComponent|u:0x60000001|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10343.0|d:10220.1
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:10376.0|d:10247.1
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                UndoOperation
                ClearSelection
                SelectComponent|u:0x60000003|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:10342.0|d:10223.1
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:10372.0|d:10249.1
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x00000000|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void Defect_IDE_1506()
        {
            // crate a geometry node and delete it using the 'delete node' option on the radial menu on the 3 dots on the top left
            // => verify no crash
            string commands = @"
                    CreateFunctionNode|d:15437.0|d:15185.0|s:ProtoGeometry.dll|s:Arc.ByCenterPointRadiusAngle|s:Point,double,double,double,Vector
                    CreateRadialMenu|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|u:0x10000001
                    SelectMenuItem|i:3|d:15316.0|d:15142.0|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest
                    MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);
        }

        [Test]
        public void Defect_IDE_1267()
        {
            //create a '+' node, and identifier and a driver and then rename all of them using double click
            // => verify the new names
            string commands = @"
                CreateFunctionNode|d:15427.0|d:15312.5|s:|s:+|s:double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:v1|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateIdentifierNode|d:15468.0|d:15204.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:v2|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateDriverNode|d:15432.0|d:15223.5
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None|d:15418.0|d:15219.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None|d:15330.0|d:15102.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Caption
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000003|s:v3|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15363.0|d:15107.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15446.0|d:15206.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            VisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual("+", node.Caption);
            Assert.AreEqual("v1", node.Text);

            node = (VisualNode)controller.GetVisualNode(0x10000002);
            Assert.AreEqual("v2", node.Caption);

            node = (VisualNode)controller.GetVisualNode(0x10000003);
            Assert.AreEqual("v3", node.Caption);
        }

        [Test]
        public void Defect_IDE_1267_2()
        {
            //create a Math.Abs node and a Geometry.Arc node and rename all of them using double click
            // => verify the new names
            string commands = @"
                CreateFunctionNode|d:15366.0|d:15282.5|s:Math.dll|s:Math.Abs|s:int
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:v1|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15411.0|d:15366.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15410.0|d:15366.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateFunctionNode|d:15307.0|d:15343.5|s:ProtoGeometry.dll|s:Arc.ByCenterPointRadiusAngle|s:Point,double,double,double,Vector
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:4|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:4|e:System.Windows.Input.ModifierKeys,None|d:15303.0|d:15376.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:4|e:System.Windows.Input.ModifierKeys,None|d:15505.0|d:15180.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputLabel|i:4|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:v2|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            VisualNode node = controller.GetVisualNode(0x10000001);
            Assert.AreEqual("Abs", node.Caption);
            Assert.AreEqual("v1", node.Text);

            node = (VisualNode)controller.GetVisualNode(0x10000002);
            Assert.AreEqual("Arc", node.Caption);
            Assert.AreEqual("v2", node.Text);
        }

        [Test]
        public void Defect_IDE_1889()
        {
            // Steps as in defect : http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1889
            // 1. Create a CBN : a = 1..100;
            // 2. Triple Click on the preview and CTRL+C
            // 3. Create a CBN b = CTRL+V
            // 4. Click on the canvas outside the CBN and verify the repositioning of the CBN node to accomodate size of the node

            string commands = @"MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateCodeBlockNode|d:15676.0|d:15332.0|s:
BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000001|s:a = 1..100;|b:True
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateCodeBlockNode|d:15609.0|d:15203.0|s:
BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000002|s:b = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 99, 100 }|b:True
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";


            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands); // failing here
            Assert.AreEqual(true, result00);

            // Verification pending
        }

        [Test]
        [Ignore]
        public void Defect_IDE_2204()
        {
            // Expected to fail as the LiveRunner will not be created in the Nunit.
            // See LiveRunnerServices::Synchronizer

            // Steps as in defect : http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-2204

            // 1. Create a CBN : 0
            // 2. Create a Point.ByCoordinates node and connect '0' to all 3 inputs
            // 3. Now click on the preview radial menu and change geometri preview to textual
            // 4. Now click on the preview radial menu and click 'hide' preview'
            // 5. Again click on the preview  -> verify the preview is still textual

            string commands = @"CreateCodeBlockNode|d:15449.0|d:15259.0|s:
                                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                                EndNodeEdit|u:0x10000001|s:Point.ByCoordinates(0,0,0);|b:True
                                CreateRadialMenu|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast|u:0x80000002
                                SelectMenuItem|i:6|d:15589.0|d:15285.0|u:0x80000002|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast
                                CreateRadialMenu|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast|u:0x80000002
                                SelectMenuItem|i:7|d:15631.0|d:15282.0|u:0x80000002|e:DesignScriptStudio.Graph.Core.NodePart,NorthEast";

            GraphController controller = new GraphController(null);
            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            VisualNode node;

            node = (VisualNode)controller.GetVisualNode(0x10000001);
            Assert.AreEqual(false, node.NodeStates.HasFlag(States.GeometryPreview));
            Assert.AreEqual(true, node.NodeStates.HasFlag(States.TextualPreview));
            Assert.AreEqual(true, node.NodeStates.HasFlag(States.PreviewHidden));
        }

        [Test]
        public void Defect_IDE_1619()
        {
            // Steps as in defect : http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1619                     
            // Load the Defect_IDE_2285.bin file
            // Then connect the '0' to the Z input of the Point and verify the Arc node now shows the geometric output
            IStorage s2 = new FileStorage();
            string testPath = Path.GetFullPath("..\\..\\..\\..\\Studio\\DesignScriptStudio.Tests\\UnitTests\\testfiles\\");
            string filePath = testPath + "Defect_IDE_1619.bin";
            ((FileStorage)s2).Load(filePath);

            GraphController controller = new GraphController(null, filePath);

            string commands = @"ClearSelection
ClearSelection
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000009|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000009|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000009|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15296.25|d:15054.83333333
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x1000000a|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None|d:15380.58333333|d:15070.83333333
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x1000000a|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            VisualNode node;

            node = (VisualNode)controller.GetVisualNode(0x1000000b);
            Assert.AreEqual(true, node.NodeStates.HasFlag(States.GeometryPreview));


        }

    }
}