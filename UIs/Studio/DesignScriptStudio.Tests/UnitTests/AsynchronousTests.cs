using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DesignScriptStudio.Graph.Core;
using System.Threading.Tasks;
using ProtoCore.Mirror;

namespace DesignScriptStudio.Tests.UnitTests
{
    class AsynchronousTests
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

        [Ignore]
        public void TestSimpleAddition()
        {
            // 1. Create code block node with value of 123.
            // 2. Create "Add" function node.
            // 3. Drag to connect from output of code block node to first input of add function node.
            // 
            string commands = @"
                CreateCodeBlockNode|d:15381.0|d:15112.0|s:Your code goes here
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                EndNodeEdit|u:0x10000001|s:3|b:True
                CreateCodeBlockNode|d:15339.0|d:15196.0|s:Your code goes here
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                EndNodeEdit|u:0x10000002|s:5|b:True
                CreateFunctionNode|d:15497.0|d:15135.0|s:Special Nodes|s:+|s:double,double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15343.0|d:15113.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15449.0|d:15129.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15301.0|d:15199.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:15436.0|d:15147.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None";

            GraphController controller = new GraphController(null);

            // Indicate that the test case is interested in the values of 
            // all these nodes whose IDs are represented by the input array.
            controller.RegisterPreviewRequest(new uint[]
            {
                0x10000001,
                0x10000002,
                0x10000003
            });

            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            // WaitForPreviewRequestCompletion method internally creates a Task and 
            // blocks on its 'Result' property until either it timed out, or all the 
            // requested node values are computed.
            Assert.AreEqual(true, controller.WaitForPreviewRequestCompletion(100000));

            // These calls are just to validate requested node values.
            PreviewRequest requestedData = controller.GetPreviewRequest();
            ProtoCore.DSASM.StackValue value = requestedData.GetNodeValue(0x10000003);
            Assert.AreEqual(8, value.opdata);

            value = requestedData.GetNodeValue(0x10000001);
            Assert.AreEqual(3, value.opdata);

            value = requestedData.GetNodeValue(0x10000002);
            Assert.AreEqual(5, value.opdata);
        }

    }
}
