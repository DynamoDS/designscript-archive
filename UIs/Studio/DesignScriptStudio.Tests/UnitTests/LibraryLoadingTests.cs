using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using NUnit.Framework;
using DesignScriptStudio.Graph.Core;

namespace DesignScriptStudio.Tests.UnitTests
{
    partial class LibraryLoadingTests
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
        public void TestDLLLoading01()
        {
            // 1. Load a dll library.
            // 
            GraphController controller = new GraphController(null);

            string testPath = Path.GetFullPath("..\\..\\..\\..\\Studio\\DesignScriptStudio.Tests\\UnitTests\\testfiles\\");
            string fileName = "experimental.dll";
            string assemblyPath = testPath + fileName;
            string assemblyName = System.IO.Path.GetFileNameWithoutExtension(assemblyPath); 

            bool result = CoreComponent.Instance.ImportAssembly(assemblyPath, null, false);
            Assert.AreEqual(true, result);

            // there should not be any errors for the importing
            ProtoCore.BuildStatus buildStatus = GraphToDSCompiler.GraphUtilities.BuildStatus;
            Assert.AreEqual(0, buildStatus.ErrorCount);
            Assert.AreEqual(0, buildStatus.WarningCount);

            // imported dll path should be stored properly in the StudioSettings
            Assert.AreEqual(true, CoreComponent.Instance.StudioSettings.LoadedAssemblies.Contains(assemblyPath));
        }

        [Test]
        public void TestDSScriptLoading01()
        {
            // 1. Load a ds library.
            // 
            string testPath = Path.GetFullPath("..\\..\\..\\..\\Studio\\DesignScriptStudio.Tests\\UnitTests\\testfiles\\");
            string fileName = "test.ds";
            string commands = @"ImportScript|s:" + testPath + fileName;

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            // there should not be any errors for the importing
            ProtoCore.BuildStatus buildStatus = GraphToDSCompiler.GraphUtilities.BuildStatus;
            Assert.AreEqual(0, buildStatus.ErrorCount);
            Assert.AreEqual(0, buildStatus.WarningCount);

            // imported ds script path should be stored properly in ImportedScripts
            Assert.AreEqual(true, controller.GetImportedScripts().Contains(testPath + fileName));
        }
        [Test]
        public void T006DSScriptImportLoading_2052()
        {
            // steps are at http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-2052
            // 
            string testPath = Path.GetFullPath("..\\..\\..\\..\\Studio\\DesignScriptStudio.Tests\\UnitTests\\testfiles\\");
            string fileName = "td1.ds";
            string commands = @"ImportScript|s:" + testPath + fileName;

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            // there should not be any errors for the importing
            ProtoCore.BuildStatus buildStatus = GraphToDSCompiler.GraphUtilities.BuildStatus;
            Assert.AreEqual(0, buildStatus.ErrorCount);
            Assert.AreEqual(0, buildStatus.WarningCount);

            // imported ds script path should be stored properly in ImportedScripts
            Assert.AreEqual(true, controller.GetImportedScripts().Contains(testPath + fileName));
        }
        [Test]
        public void T001_Defect_IDE_1635()
        {
            // 1. Load a dll library. 
            // 
            GraphController controller = new GraphController(null);

            string testPath = Path.GetFullPath("..\\..\\..\\..\\Studio\\DesignScriptStudio.Tests\\UnitTests\\testfiles\\");
            string fileName = "MinimalFFITest.dll";
            string assemblyPath = testPath + fileName;
            string assemblyName = System.IO.Path.GetFileNameWithoutExtension(assemblyPath);

            bool result = CoreComponent.Instance.ImportAssembly(assemblyPath, null, false);
            Assert.AreEqual(true, result);

            // there should not be any errors for the importing
            ProtoCore.BuildStatus buildStatus = GraphToDSCompiler.GraphUtilities.BuildStatus;
            Assert.AreEqual(0, buildStatus.ErrorCount);
            Assert.AreEqual(0, buildStatus.WarningCount);

            // imported dll path should be stored properly in the StudioSettings
            Assert.AreEqual(true, CoreComponent.Instance.StudioSettings.LoadedAssemblies.Contains(assemblyPath));

            string commands = @"
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateCodeBlockNode|d:15330.0|d:15127.0|s:
BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000001|s:1..5|b:True
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15340.0|d:15233.0
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15340.0|d:15234.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateCodeBlockNode|d:15340.0|d:15235.0|s:
BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000002|s:0..4|b:True
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateFunctionNode|d:15506.0|d:15150.0|s:User defined|s:+|s:double,double
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15359.0|d:15130.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15454.0|d:15149.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15368.0|d:15236.0
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:15452.0|d:15169.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
CreateFunctionNode|d:15642.0|d:15318.0|s:MinimalFFITest.dll|s:Minimal.staticFunction|s:double
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15548.0|d:15162.0
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15576.0|d:15326.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000004|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
";


            bool result00 = controller.RunCommands(commands);
            Assert.AreEqual(true, result00);

            VisualNode node;

            node = (VisualNode)controller.GetVisualNode(0x10000004);
            Assert.AreEqual(false, node.Error);

        }

        [Test]
        public void T002_Defect_IDE_1807()
        {
            // 1. Load a ds library.
            // 
            string testPath = Path.GetFullPath("..\\..\\..\\..\\Studio\\DesignScriptStudio.Tests\\UnitTests\\testfiles\\");
            string fileName = "AdditionFunction.ds";
            string commands = @"ImportScript|s:" + testPath + fileName;

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            // there should not be any errors for the importing
            ProtoCore.BuildStatus buildStatus = GraphToDSCompiler.GraphUtilities.BuildStatus;
            Assert.AreEqual(0, buildStatus.ErrorCount);
            Assert.AreEqual(0, buildStatus.WarningCount);

            // imported ds script path should be stored properly in ImportedScripts
            Assert.AreEqual(true, controller.GetImportedScripts().Contains(testPath + fileName));

            commands  = @"CreateFunctionNode|d:15554.0|d:15125.0|s:AdditionFunction.ds|s:Addition|s:double,double
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateCodeBlockNode|d:15345.0|d:15115.0|s:
BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000002|s:1|b:True
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateCodeBlockNode|d:15335.0|d:15224.0|s:
BeginNodeEdit|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000003|s:2|b:True
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15419.0|d:15184.0
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15416.0|d:15181.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15366.0|d:15118.0
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15494.0|d:15124.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15351.0|d:15230.0
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:15505.0|d:15139.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None

";
            result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            VisualNode node;

            node = (VisualNode)controller.GetVisualNode(0x10000001);
            Assert.AreEqual("Var1", node.Text);
        }

        [Test]
        public void T003_IDE_2196()
        {
            // Steps : http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-2196
             
            string testPath = Path.GetFullPath("..\\..\\..\\..\\Studio\\DesignScriptStudio.Tests\\UnitTests\\testfiles\\");
            string fileName = "T003_IDE_2196.ds";
            string commands = @"ImportScript|s:" + testPath + fileName;

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            
            ProtoCore.BuildStatus buildStatus = GraphToDSCompiler.GraphUtilities.BuildStatus;
            IEnumerator<ProtoCore.BuildData.ErrorEntry> e1 =buildStatus.Errors.GetEnumerator();
            e1.Reset();
            e1.MoveNext();
            ProtoCore.BuildData.ErrorEntry err = e1.Current;
            String Message = err.Message;

            Assert.AreEqual(1, buildStatus.ErrorCount);
            Assert.AreEqual("Defining language blocks are not yet supported", Message);
            
        }

        [Test]
        public void T004_IDE_2197()
        {

            // Steps : http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-2197
            // 1. Load the T004_IDE_2197.ds file
            // 2. Instantiate the FacadePanel.FromFace class -> verify no errors

            string testPath = Path.GetFullPath("..\\..\\..\\..\\Studio\\DesignScriptStudio.Tests\\UnitTests\\testfiles\\");
            string fileName = "T004_IDE_2197.ds";
            string commands = @"ImportScript|s:" + testPath + fileName;

            GraphController controller = new GraphController(null);
            bool result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);

            ProtoCore.BuildStatus buildStatus = GraphToDSCompiler.GraphUtilities.BuildStatus;
            Assert.AreEqual(0, buildStatus.ErrorCount);
            Assert.AreEqual(0, buildStatus.WarningCount);

            commands = @"CreateFunctionNode|d:15514.0|d:15267.0|s:facade_panel_class.ds|s:FacadePanel.FromFace|s:Face,double,double,double,double";
            result = controller.RunCommands(commands);
            Assert.AreEqual(true, result);
            buildStatus = GraphToDSCompiler.GraphUtilities.BuildStatus;
            Assert.AreEqual(0, buildStatus.ErrorCount);
            Assert.AreEqual(0, buildStatus.WarningCount);
        }

        [Test]
        public void T005_IDE_1992()
        {
            // steps as per defect http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-1992
            // 1. Import the ds file Import_1992.ds
            // 2. Create a CBN : 1;5;
            // 3. Create a WCS node
            // 4. Create a function node 'CreateTriangle'  => verify it's geometric preview ( curve) is coming
                string testPath = Path.GetFullPath("..\\..\\..\\..\\Studio\\DesignScriptStudio.Tests\\UnitTests\\testfiles\\");
                string fileName = "Import_1992.ds";
                string commands = @"ImportScript|s:" + testPath + fileName;

                GraphController controller = new GraphController(null);
                bool result = controller.RunCommands(commands);
                Assert.AreEqual(true, result);

                ProtoCore.BuildStatus buildStatus = GraphToDSCompiler.GraphUtilities.BuildStatus;
                Assert.AreEqual(0, buildStatus.ErrorCount);
                Assert.AreEqual(0, buildStatus.WarningCount);

                commands = @"MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreateCodeBlockNode|d:15380.0|d:15242.0|s:
BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
EndNodeEdit|u:0x10000001|s:1;\n5;|b:True
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
CreatePropertyNode|d:15373.0|d:15138.0|s:ProtoGeometry.dll|s:CoordinateSystem.WCS|s:
CreateFunctionNode|d:15560.0|d:15158.0|s:Import_1992.ds|s:CreateTriangle|s:CoordinateSystem,double,double
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15393.0|d:15153.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None|d:15480.0|d:15134.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15394.0|d:15149.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15474.0|d:15143.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15396.0|d:15243.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:15477.0|d:15156.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:1|e:System.Windows.Input.ModifierKeys,None
BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:1|e:System.Windows.Input.ModifierKeys,None|d:15396.0|d:15262.0
EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None|d:15473.0|d:15186.0
MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:2|e:System.Windows.Input.ModifierKeys,None";
                
                result = controller.RunCommands(commands);
                Assert.AreEqual(true, result);

                VisualNode node = (VisualNode)controller.GetVisualNode(0x10000003);
                // Need to add verification code to verify the geometric preview is coming
                //Assert.AreEqual(States.GeometryPreview, node.NodeStates);
               // Assert.AreEqual("Geometric Preview", node.PreviewValue);                       
        }


    }
}