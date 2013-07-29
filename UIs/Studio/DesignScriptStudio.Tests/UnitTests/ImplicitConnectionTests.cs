using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DesignScriptStudio.Graph.Core;
using NUnit.Framework;

namespace DesignScriptStudio.Tests.UnitTests
{
    class ImplicitConnectionTests
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
        public void TestInputOutputImplicitConnections()
        {
            // 1. double click on the canvas and create a code block node
            // 2. Add code : "a = 1;" => verify only one output slot is created
            string commands = @"
                CreateCodeBlockNode|d:15344.0|d:15068.0|s:Your code goes here
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                EndNodeEdit|u:0x10000001|s:a=10|b:True
                CreateCodeBlockNode|d:15296.0|d:15158.0|s:Your code goes here
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                EndNodeEdit|u:0x10000002|s:b=11|b:True
                CreateCodeBlockNode|d:15364.0|d:15249.0|s:Your code goes here
                BeginNodeEdit|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text
                EndNodeEdit|u:0x10000003|s:j=7|b:True
                CreateCodeBlockNode|d:15606.0|d:15349.0|s:Your code goes here
                BeginNodeEdit|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text
                EndNodeEdit|u:0x10000004|s:p=j|b:True
                CreateCodeBlockNode|d:15614.0|d:15147.0|s:Your code goes here
                BeginNodeEdit|u:0x10000005|e:DesignScriptStudio.Graph.Core.NodePart,Text
                EndNodeEdit|u:0x10000005|s:q=g;|b:True
                CreateCodeBlockNode|d:15386.0|d:15025.0|s:Your code goes here
                BeginNodeEdit|u:0x10000006|e:DesignScriptStudio.Graph.Core.NodePart,Text
                EndNodeEdit|u:0x10000006|s:c;\n\nd+e;\n\nf=a+b;\n\ng=b;\n\nh=a;\n\ni+j;|b:True";

            GraphController controller = new GraphController(null);
            bool runCommandResult = controller.RunCommands(commands);
            Assert.AreEqual(true, runCommandResult);

            TestHelpers helper = new TestHelpers(controller);

            // Ensure all necessary nodes are created.
            helper.EnsureNodeSlotInfo(0x10000001, null, new uint[] { 0x30000001 });
            helper.EnsureNodeSlotInfo(0x10000002, null, new uint[] { 0x30000002 });
            helper.EnsureNodeSlotInfo(0x10000003, null, new uint[] { 0x30000003 });
            helper.EnsureNodeSlotInfo(0x10000004, new uint[] { 0x30000004 }, new uint[] { 0x30000005 });
            helper.EnsureNodeSlotInfo(0x10000005, new uint[] { 0x30000006 }, new uint[] { 0x30000007 });
            helper.EnsureNodeSlotInfo(0x10000006,
                new uint[] { 0x30000008, 0x30000009, 0x3000000a, 0x3000000b, 0x3000000c, 0x3000000d, 0x3000000e },
                new uint[] { 0x3000000f, 0x30000010, 0x30000011, 0x30000012, 0x30000013, 0x30000014 });

            // Ensure all the slots are what we expect them to be.
            helper.EnsureSlotInfo(0x30000001, SlotType.Output, true, new uint[] { 0x10000001 }, new uint[] { 0x3000000b });
            helper.EnsureSlotInfo(0x30000002, SlotType.Output, true, new uint[] { 0x10000002 }, new uint[] { 0x3000000c });
            helper.EnsureSlotInfo(0x30000003, SlotType.Output, true, new uint[] { 0x10000003 }, new uint[] { 0x30000004, 0x3000000e });
            helper.EnsureSlotInfo(0x30000004, SlotType.Input, false, new uint[] { 0x10000004 }, new uint[] { 0x30000003 });
            helper.EnsureSlotInfo(0x30000005, SlotType.Output, true, new uint[] { 0x10000004 }, null);
            helper.EnsureSlotInfo(0x30000006, SlotType.Input, false, new uint[] { 0x10000005 }, new uint[] { 0x30000012 });
            helper.EnsureSlotInfo(0x30000007, SlotType.Output, true, new uint[] { 0x10000005 }, null);
            helper.EnsureSlotInfo(0x30000008, SlotType.Input, true, new uint[] { 0x10000006 }, null);
            helper.EnsureSlotInfo(0x30000009, SlotType.Input, false, new uint[] { 0x10000006 }, null);
            helper.EnsureSlotInfo(0x3000000a, SlotType.Input, false, new uint[] { 0x10000006 }, null);
            helper.EnsureSlotInfo(0x3000000b, SlotType.Input, false, new uint[] { 0x10000006 }, new uint[] { 0x30000001 });
            helper.EnsureSlotInfo(0x3000000c, SlotType.Input, false, new uint[] { 0x10000006 }, new uint[] { 0x30000002 });
            helper.EnsureSlotInfo(0x3000000d, SlotType.Input, false, new uint[] { 0x10000006 }, null);
            helper.EnsureSlotInfo(0x3000000e, SlotType.Input, false, new uint[] { 0x10000006 }, new uint[] { 0x30000003 });
            helper.EnsureSlotInfo(0x3000000f, SlotType.Output, true, new uint[] { 0x10000006 }, null);
            helper.EnsureSlotInfo(0x30000010, SlotType.Output, true, new uint[] { 0x10000006 }, null);
            helper.EnsureSlotInfo(0x30000011, SlotType.Output, true, new uint[] { 0x10000006 }, null);
            helper.EnsureSlotInfo(0x30000012, SlotType.Output, true, new uint[] { 0x10000006 }, new uint[] { 0x30000006 });
            helper.EnsureSlotInfo(0x30000013, SlotType.Output, true, new uint[] { 0x10000006 }, null);
            helper.EnsureSlotInfo(0x30000014, SlotType.Output, true, new uint[] { 0x10000006 }, null);

            // Ensure all the edges are what we expect them to be.
            helper.EnsureEdgeInfo(0x60000001, 0x30000003, 0x30000004, EdgeType.ImplicitConnection);
            helper.EnsureEdgeInfo(0x60000002, 0x30000003, 0x3000000e, EdgeType.ImplicitConnection);
            helper.EnsureEdgeInfo(0x60000003, 0x30000012, 0x30000006, EdgeType.ImplicitConnection);
            helper.EnsureEdgeInfo(0x60000004, 0x30000001, 0x3000000b, EdgeType.ImplicitConnection);
            helper.EnsureEdgeInfo(0x60000005, 0x30000002, 0x3000000c, EdgeType.ImplicitConnection);
        }

        [Test]
        public void Test_Defect_1843()
        {
            // Steps as per defect : http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1843
            string commands = @"
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateCodeBlockNode|d:15374.0|d:15156.0|s:
BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000001|s:x|b:True
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateCodeBlockNode|d:15423.0|d:15232.0|s:
BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000002|s:x|b:True
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateCodeBlockNode|d:15575.0|d:15255.0|s:
BeginNodeEdit|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000003|s:x|b:True
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateCodeBlockNode|d:15549.0|d:15129.0|s:
BeginNodeEdit|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000004|s:1|b:True
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15565.0|d:15130.0
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15568.0|d:15263.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
";


            GraphController controller = new GraphController(null);
            bool runCommandResult = controller.RunCommands(commands);
            Assert.AreEqual(true, runCommandResult);

            TestHelpers helper = new TestHelpers(controller);           

            // Ensure all the edges are what we expect them to be.

            helper.EnsureEdgeInfo(0x60000001, 0x30000007, 0x30000005, EdgeType.ExplicitConnection);
            helper.EnsureEdgeInfo(0x60000002, 0x30000006, 0x30000001, EdgeType.ImplicitConnection);
            helper.EnsureEdgeInfo(0x60000003, 0x30000006, 0x30000003, EdgeType.ImplicitConnection);
            
        }
    }
}
