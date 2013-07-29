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
        public void TestFunctionInputSlotClickingCrash()
        {
            // 1. Create "sin" function node (takes single argument).
            // 2. Click on second input slot (non-existence).
            // 3. Crashes.
            // 
            string commands = @"
                CreateFunctionNode|d:684.0|d:129.0|s:Math.dll|s:sin|s:double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            ISlot slot = controller.GetSlot(0x30000001);
            Assert.IsNotNull(slot);
            Assert.IsNull(slot.ConnectingSlots);
        }

        [Test]
        public void TestDeleteNodeCrash()
        {
            // 1. Create operator node "+".
            // 2. Create operator node "+".
            // 3. Connect output of second "+" node to the input of first "+"
            // 4. Select the second "+" node.
            // 5. Hit DELETE key, crashes.
            // 
            string commands = @"
                CreateFunctionNode|d:1086.0|d:357.0|s:|s:+|s:double,double
                CreateFunctionNode|d:529.0|d:292.0|s:|s:+|s:double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                ClearSelection
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
        }

        [Ignore]
        public void TestReconnectionCrash()
        {
            // 1. Create three literal nodes
            // 2. Create two addition node
            // 3. Connect two literal node to first addition node, and output of addition node to input of second addition node
            // 4. Now try to replace one of the literal node using third literal node
            // 5. Crash
            // 
            string commands = @"
                CreateCodeBlockNode|d:0.0|d:0.0|s:1
                CreateCodeBlockNode|d:444.0|d:260.0|s:2
                CreateCodeBlockNode|d:475.0|d:341.0|s:3
                CreateFunctionNode|d:715.0|d:189.0|s:|s:+|s:double,double
                CreateFunctionNode|d:880.0|d:302.0|s:|s:+|s:double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                ClearSelection
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void TestClickingOnNonExistentSlotCrash()
        {
            // 1. Create a plus function node.
            // 2. Click on the function node to select it.
            // 3. Click right below the second input slot
            // 4. Crash
            // 
            string commands = @"
                CreateFunctionNode|d:563.0|d:218.0|s:|s:+|s:double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                ClearSelection";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void TestNonExistentSlotHitTestCrash()
        {
            // 1. Create a "+" function node.
            // 2. Click on the function node to select it.
            // 3. Click right below the second input slot
            // 4. Crash
            // 
            string commands = @"
                CreateFunctionNode|d:517.0|d:242.0|s:|s:+|s:double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void TestNodeDeletionMulipleUndoRedoCrash()
        {
            // 1. Create 2 function node.
            // 2. Join the first node output to the second node input.
            // 3. Delete the first node.
            // 4. Upon second redo, it crash
            //
            string commands = @"
                CreateFunctionNode|d:328.0|d:317.0|s:Math.dll|s:Math.Sin|s:double
                CreateFunctionNode|d:737.0|d:323.0|s:Math.dll|s:Math.Cos|s:double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:474.0|d:343.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:751.0|d:341.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents";

            GraphController graphController = new GraphController(null);
            bool result = graphController.RunCommands(commands);
            Assert.AreEqual(true, result);

            commands = @"UndoOperation";
            bool result01 = graphController.RunCommands(commands);
            Assert.AreEqual(true, result01);
            Assert.AreEqual(2, graphController.GetVisualNodes().Count);

            commands = @"RedoOperation";
            bool result02 = graphController.RunCommands(commands);
            Assert.AreEqual(true, result02);
            Assert.AreEqual(1, graphController.GetVisualNodes().Count);

            commands = @"UndoOperation";
            bool result03 = graphController.RunCommands(commands);
            Assert.AreEqual(true, result03);
            Assert.AreEqual(2, graphController.GetVisualNodes().Count);

            commands = @"RedoOperation";
            bool result04 = graphController.RunCommands(commands);
            Assert.AreEqual(true, result04);
            Assert.AreEqual(1, graphController.GetVisualNodes().Count);
        }

        [Test]
        public void TestPressDeleteWithoutAnySelectionCrash()
        {
            //Press "Delete" without selecting any nodes or edge will cause a crash
            //
            string commands = @"DeleteComponents";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
        }

        [Ignore]
        public void TestLiteralInputSlotClicking()
        {
            // 1. Create a literal node.
            // 2. Click on the input slot of literal node (there's none).
            // 3. Crashes.
            // 
            // Updated to exclude the last "MouseDown" command because with the 
            // fix that command would not even occur (Literal node has got no 
            // input slot so the NodePart.InputSlot will not even be sent.
            // 
            // string commands = @"
            //     CreateNode|e:DesignScriptStudio.Graph.Core.NodeType,Literal|s:Literal|d:684.0|d:186.0
            //     MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
            //     MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:1
            //     MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None";
            // 
            string commands = @"
                CreateCodeBlockNode|d:684.0|d:186.0|s:Literal
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,None|i:1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            // Ensure the literal node is created.
            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.IsNotNull(node);
            Assert.AreEqual(NodeType.CodeBlock, node.VisualType);

            // Ensure the slot exists.
            ISlot slot = controller.GetSlot(0x30000001);
            Assert.IsNotNull(slot.Owners);
            Assert.AreEqual(0x10000001, slot.Owners[0]);

            // Ensure the only slot does not connect to other slots.
            Assert.IsNull(slot.ConnectingSlots);
        }
    }
}
