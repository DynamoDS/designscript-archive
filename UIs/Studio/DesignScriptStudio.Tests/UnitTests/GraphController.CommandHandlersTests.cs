using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;
using DesignScriptStudio.Graph.Core;

namespace DesignScriptStudio.Tests.UnitTests
{
    partial class GraphControllerTests
    {
        [Test]
        public void TestHandleCreateIdentifierNode()
        {
            string commands = @"
                CreateIdentifierNode|d:594.0|d:281.0";

            GraphController graphController = new GraphController(null);
            bool result = graphController.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node = graphController.GetVisualNodes().ElementAt(0);
            Assert.NotNull(node);
            Assert.AreEqual(NodeType.Identifier, node.VisualType);
        }

        [Test]
        public void TestHandleCreateDriverNode()
        {
            string commands = @"
                CreateDriverNode|d:574.0|d:265.0";

            GraphController graphController = new GraphController(null);
            bool result = graphController.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node = graphController.GetVisualNodes().ElementAt(0);
            Assert.NotNull(node);
            Assert.AreEqual(NodeType.Driver, node.VisualType);
        }

        [Test]
        public void TestHandleCreateCodeblockNode()
        {
            string commands = @"
                CreateCodeBlockNode|d:503.0|d:309.0|s:";

            GraphController graphController = new GraphController(null);
            bool result = graphController.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node = graphController.GetVisualNodes().ElementAt(0);
            Assert.NotNull(node);
            Assert.AreEqual(NodeType.CodeBlock, node.VisualType);
        }

        [Test]
        public void TestHandleCreateFunctionNode()
        {
            string commands = @"
                CreateFunctionNode|d:560.0|d:295.0|s:Math.dll|s:Math.Sin|s:double";

            GraphController graphController = new GraphController(null);
            bool result = graphController.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node = graphController.GetVisualNodes().ElementAt(0);
            Assert.NotNull(node);
            Assert.AreEqual(NodeType.Function, node.VisualType);
        }

        [Test]
        public void TestHandleSaveGraphAndLoadGraph()
        {
            try
            {
                GraphController graphController1 = new GraphController(null);
                IVisualNode node1 = new CodeBlockNode(graphController1, "Double Click and Type");
                IVisualNode node2 = new DriverNode(graphController1, Configurations.DriverInitialTextValue);
                IVisualNode node3 = new FunctionNode(graphController1, "", "-", "double,double");
                IVisualNode node4 = new IdentifierNode(graphController1, "c");

                string filePath = Path.GetTempPath() + "test.bin";
                graphController1.DoSaveGraph(filePath);
                GraphController graphController2 = new GraphController(null, filePath);

                Assert.AreEqual(4, graphController2.GetVisualNodes().Count);
            }
            finally
            {
                File.Delete(Path.GetTempPath() + "test.bin");
            }
        }

        [Test]
        public void TestHandleBeginAndEndDrag()
        {
            string commands = @"
                CreateFunctionNode|d:323.0|d:150.0|s:Math.dll|s:Math.Sin|s:double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:432.0|d:167.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None|d:955.0|d:422.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController graphController = new GraphController(null);
            bool result = graphController.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node = graphController.GetVisualNodes().ElementAt(0);
            Assert.AreNotEqual(323.0, node.X);
            Assert.AreNotEqual(150.0, node.Y);
        }

        [Test]
        public void TestHandleBeginAndEndNodeEdit()
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
            Assert.AreEqual("abc", ((VisualNode)node).Text);
        }

        [Test]
        public void TestHandleDeleteComponents()
        {
            string commands = @"
                CreateFunctionNode|d:379.0|d:299.0|s:Math.dll|s:Math.Sin|s:double
                CreateFunctionNode|d:743.0|d:277.0|s:Math.dll|s:Math.Sin|s:double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:523.0|d:325.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:744.0|d:297.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:346.0|d:213.0
                SelectComponent|u:0x10000001|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x60000001|e:System.Windows.Input.ModifierKeys,None
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:672.0|d:434.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents";

            GraphController graphController = new GraphController(null);
            bool result = graphController.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node = graphController.GetVisualNodes().ElementAt(0);
            Assert.AreEqual(1, graphController.GetVisualNodes().Count());
            Assert.AreEqual(0x10000002, node.NodeId);
            Assert.Null(graphController.GetVisualEdge(0x60000001));
        }

        [Test]
        public void TestHandleSelectComponent()
        {
            string commands = @"
                CreateFunctionNode|d:379.0|d:299.0|s:Math.dll|s:Math.Sin|s:double
                CreateFunctionNode|d:743.0|d:277.0|s:Math.dll|s:Math.Sin|s:double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:523.0|d:325.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:744.0|d:297.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:346.0|d:213.0
                SelectComponent|u:0x10000001|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x60000001|e:System.Windows.Input.ModifierKeys,None
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:672.0|d:434.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";
            
            GraphController graphController = new GraphController(null);
            bool result = graphController.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = graphController.GetVisualNode(0x10000001);
            IVisualNode node02 = graphController.GetVisualNode(0x10000002);
            IVisualEdge edge = graphController.GetVisualEdge(0x60000001);

            Assert.AreEqual(true, ((VisualNode)node01).Selected);
            Assert.AreEqual(false, ((VisualNode)node02).Selected);
            Assert.AreEqual(true, ((VisualEdge)edge).Selected);
        }

        [Test]
        public void TestHandleClearSelection()
        {
            string commands = @"
                CreateFunctionNode|d:405.0|d:301.0|s:Math.dll|s:Math.Sin|s:double
                CreateFunctionNode|d:746.0|d:267.0|s:Math.dll|s:Math.Sin|s:double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:548.0|d:325.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:754.0|d:288.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:353.0|d:199.0
                SelectComponent|u:0x10000001|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x10000002|e:System.Windows.Input.ModifierKeys,None
                SelectComponent|u:0x60000001|e:System.Windows.Input.ModifierKeys,None
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:926.0|d:409.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                ClearSelection";

            GraphController graphController = new GraphController(null);
            bool result = graphController.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node01 = graphController.GetVisualNode(0x10000001);
            IVisualNode node02 = graphController.GetVisualNode(0x10000002);
            IVisualEdge edge = graphController.GetVisualEdge(0x60000001);
            Assert.AreEqual(false, ((VisualNode)node01).Selected);
            Assert.AreEqual(false, ((VisualNode)node02).Selected);
            Assert.AreEqual(false, ((VisualEdge)edge).Selected);
        }
    }
}
