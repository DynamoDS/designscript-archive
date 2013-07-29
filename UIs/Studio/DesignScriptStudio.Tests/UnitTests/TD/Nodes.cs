using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;

namespace DesignScriptStudio.Graph.Core.UnitTests.TD
{
    class nodes
    {
       
        [Test]
        public void L00TestLiteral_SingleNode()
        {
            // Create Single literal node
            
            string commands = @"
                ClearSelection
                CreateCodeBlockNode|d:386.0|d:270.0|s:100";

            GraphController controller = new GraphController(null, null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.NotNull(node);// check if node create 
            Assert.AreEqual(NodeType.Literal, node.VisualType);// check if literal node
            Assert.AreEqual(1, controller.GetSlots().Count);//output slot
            
            

            
        }

        [Test]
        public void L01TestLiteral()
        {
            // Create two literal nodes
            
            string commands = @"
                ClearSelection
                CreateCodeBlockNode|d:315.0|d:179.0|s:100
                ClearSelection
                CreateCodeBlockNode|d:329.0|d:280.0|s:100";

            GraphController controller = new GraphController(null, null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
            Assert.AreEqual(2, controller.GetVisualNodes().Count);//output slot

            //check each Node
            IVisualNode node = controller.GetVisualNode(0x10000001);
            IVisualNode node2 = controller.GetVisualNode(0x10000002);
            Assert.NotNull(new object []{node,node2});// check if node create 
            Assert.AreEqual(NodeType.Literal,node.VisualType);// check if literal node
            Assert.AreEqual(NodeType.Literal, node2.VisualType);// check if literal node
            Assert.AreEqual(2, controller.GetSlots().Count);//output slot
            Assert.AreEqual(null, node.GetInputSlots());//output slot
            Assert.AreEqual(null, node2.GetInputSlots());//output slot


        }
        
        [Test]
        public void L02TestLiteral_Connectivity()
        {
            //Create two literal nodes & Connect them
            string commands = @"
                ClearSelection
                CreateCodeBlockNode|d:335.0|d:169.0|s:100
                CreateIdentifierNode|d:534.0|d:184.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null, null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
            Assert.AreEqual(2, controller.GetVisualNodes().Count);//output slot

            IVisualNode node = controller.GetVisualNode(0x10000001);
            IVisualNode node2 = controller.GetVisualNode(0x10000002);
            
            Assert.NotNull(new object[] { node, node2 });// check if node create 
            Assert.AreEqual(NodeType.Literal, node.VisualType);// check if literal node
            
            Assert.AreEqual(null, node.GetInputSlots());//output slot
            Assert.AreEqual(3, controller.GetSlots().Count);//output slot
            // get slots

            uint outputSlotId = node.GetOutputSlot(0);
            ISlot outputSlot = controller.GetSlot(outputSlotId); 

            uint connectingSlotId = node2.GetInputSlot(0);
            ISlot connectingSlot = controller.GetSlot(connectingSlotId);

            uint[] connecting = connectingSlot.ConnectingSlots;
            Assert.AreEqual(connectingSlotId, outputSlot.ConnectingSlots[0]);
            

        }
        [Test]
        public void L03TestLiteral_Alphanumeric()
        {
            // Create Single literal node

            string commands = @"
                ClearSelection
                CreateCodeBlockNode|d:459.0|d:271.0|s:sas121!@@#$$$";

            GraphController controller = new GraphController(null, null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.NotNull(node);// check if node create 
            Assert.AreEqual(NodeType.Literal, node.VisualType);// check if literal node
            Assert.AreEqual(1, controller.GetSlots().Count);//output slot




        }
        [Test]
        public void L04TestLiteral_Delete_Createnew_click()
        {
            // Create Single literal node

            // @TODO(Monika): Please re-record this test case, due to the changes in 
            // mouse-down/begin-drag/end-drag/mouse-up sequence, these recorded commands are no longer valid.
            string commands = @"
                ClearSelection
                CreateCodeBlockNode|d:365.0|d:296.0|s:100
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:0|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents
                ClearSelection
                BeginDrag|d:419.0|d:320.0";

            GraphController controller = new GraphController(null, null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            /*IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.NotNull(node);// check if node create 
            Assert.AreEqual(NodeType.Literal, node.VisualType);// check if literal node
            Assert.AreEqual(1, controller.GetSlots().Count);//output slot. */

        }
        [Test]
        public void L05TestLiteral_1000characters()
        {
            // Create Single literal node

            string commands = @"
                ClearSelection
                CreateCodeBlockNode|d:434.0|d:270.0|s:1234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123451234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567891234567123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789123456789";

            GraphController controller = new GraphController(null, null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            /*IVisualNode node = controller.GetVisualNode(0x10000001);
            Assert.NotNull(node);// check if node create 
            Assert.AreEqual(NodeType.Literal, node.VisualType);// check if literal node
            Assert.AreEqual(1, controller.GetSlots().Count);//output slot. */

        }
       

    }
}
