using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using DesignScriptStudio.Graph.Core;

namespace DesignScriptStudio.Tests.UnitTests
{
    class UndoRedoRecorderTests
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
        public void TestEndGroup()
        {
            GraphController graphController = new GraphController(null);
            UndoRedoRecorder urr = new UndoRedoRecorder(graphController);

            Assert.Throws<InvalidOperationException>(() =>
            {
                urr.EndGroup();
            });
        }

        [Test]
        public void TestRecordRuntimeStatesForUndo00()
        {
            GraphController graphController = new GraphController(null);
            UndoRedoRecorder urr = new UndoRedoRecorder(graphController);
            RuntimeStates runtimeStates = new RuntimeStates();

            Assert.Throws<InvalidOperationException>(() =>
            {
                urr.RecordRuntimeStatesForUndo(runtimeStates);
            });
        }

        [Test]
        public void TestRecordRuntimeStatesForUndo01()
        {
            GraphController graphController = new GraphController(null);
            UndoRedoRecorder urr = new UndoRedoRecorder(graphController);

            Assert.Throws<ArgumentNullException>(() =>
            {
                urr.BeginGroup();
                urr.RecordRuntimeStatesForUndo(null);
            });
        }

        [Test]
        public void TestRecordEdgeCreationForUndo00()
        {
            GraphController graphController = new GraphController(null);
            UndoRedoRecorder urr = new UndoRedoRecorder(graphController);
            EdgeController edgeController = new EdgeController(graphController);

            List<IVisualEdge> edgeList = new List<IVisualEdge>();
            VisualEdge edge = new VisualEdge(edgeController, EdgeType.ExplicitConnection);
            edgeList.Add(edge);

            Assert.Throws<InvalidOperationException>(() =>
            {
                urr.RecordEdgeCreationForUndo(edgeList);
            });
        }

        [Test]
        public void TestRecordEdgeCreationForUndo01()
        {
            GraphController graphController = new GraphController(null);
            UndoRedoRecorder urr = new UndoRedoRecorder(graphController);

            Assert.Throws<ArgumentException>(() =>
            {
                urr.BeginGroup();
                urr.RecordEdgeCreationForUndo(null);
            });
        }

        [Test]
        public void TestRecordEdgeCreationForUndo02()
        {
            GraphController graphController = new GraphController(null);
            UndoRedoRecorder urr = new UndoRedoRecorder(graphController);

            Assert.Throws<ArgumentException>(() =>
            {
                urr.BeginGroup();
                urr.RecordEdgeCreationForUndo(new List<IVisualEdge>());
            });
        }

        [Test]
        public void TestRecordEdgeDeletionForUndo00()
        {
            GraphController graphController = new GraphController(null);
            UndoRedoRecorder urr = new UndoRedoRecorder(graphController);
            EdgeController edgeController = new EdgeController(graphController);

            List<IVisualEdge> edgeList = new List<IVisualEdge>();
            VisualEdge edge = new VisualEdge(edgeController, EdgeType.ExplicitConnection);
            edgeList.Add(edge);

            Assert.Throws<InvalidOperationException>(() =>
            {
                urr.RecordEdgeDeletionForUndo(edgeList);
            });
        }

        [Test]
        public void TestRecordEdgeDeletionForUndo01()
        {
            GraphController graphController = new GraphController(null);
            UndoRedoRecorder urr = new UndoRedoRecorder(graphController);

            Assert.Throws<ArgumentException>(() =>
            {
                urr.BeginGroup();
                urr.RecordEdgeDeletionForUndo(null);
            });
        }

        [Test]
        public void TestRecordEdgeDeletionForUndo02()
        {
            GraphController graphController = new GraphController(null);
            UndoRedoRecorder urr = new UndoRedoRecorder(graphController);

            Assert.Throws<ArgumentException>(() =>
            {
                urr.BeginGroup();
                urr.RecordEdgeDeletionForUndo(new List<IVisualEdge>());
            });
        }

        [Test]
        public void TestRecordEdgeModificationForUndo00()
        {
            GraphController graphController = new GraphController(null);
            UndoRedoRecorder urr = new UndoRedoRecorder(graphController);
            EdgeController edgeController = new EdgeController(graphController);

            List<IVisualEdge> edgeList = new List<IVisualEdge>();
            VisualEdge edge = new VisualEdge(edgeController, EdgeType.ExplicitConnection);
            edgeList.Add(edge);

            Assert.Throws<InvalidOperationException>(() =>
            {
                urr.RecordEdgeModificationForUndo(edgeList);
            });
        }

        [Test]
        public void TestRecordEdgeModificationForUndo01()
        {
            GraphController graphController = new GraphController(null);
            UndoRedoRecorder urr = new UndoRedoRecorder(graphController);

            Assert.Throws<ArgumentException>(() =>
            {
                urr.BeginGroup();
                urr.RecordEdgeModificationForUndo(null);
            });
        }

        [Test]
        public void TestRecordEdgeModificationForUndo02()
        {
            GraphController graphController = new GraphController(null);
            UndoRedoRecorder urr = new UndoRedoRecorder(graphController);

            Assert.Throws<ArgumentException>(() =>
            {
                urr.BeginGroup();
                urr.RecordEdgeModificationForUndo(new List<IVisualEdge>());
            });
        }

        [Test]
        public void TestRecordNodeCreationForUndo00()
        {
            GraphController graphController = new GraphController(null);
            UndoRedoRecorder urr = new UndoRedoRecorder(graphController);
            
            List<IVisualNode> nodeList = new List<IVisualNode>();
            DriverNode node = new DriverNode(graphController);
            nodeList.Add(node);

            Assert.Throws<InvalidOperationException>(() =>
            {
                urr.RecordNodeCreationForUndo(nodeList);
            });
        }

        [Test]
        public void TestRecordNodeCreationForUndo01()
        {
            GraphController graphController = new GraphController(null);
            UndoRedoRecorder urr = new UndoRedoRecorder(graphController);

            Assert.Throws<ArgumentException>(() =>
            {
                urr.BeginGroup();
                urr.RecordNodeCreationForUndo(null);
            });
        }

        [Test]
        public void TestRecordNodeCreationForUndo02()
        {
            GraphController graphController = new GraphController(null);
            UndoRedoRecorder urr = new UndoRedoRecorder(graphController);

            Assert.Throws<ArgumentException>(() =>
            {
                urr.BeginGroup();
                urr.RecordNodeCreationForUndo(new List<IVisualNode>());
            });
        }

        [Test]
        public void TestRecordNodeDeletionForUndo00()
        {
            GraphController graphController = new GraphController(null);
            UndoRedoRecorder urr = new UndoRedoRecorder(graphController);

            List<IVisualNode> nodeList = new List<IVisualNode>();
            DriverNode node = new DriverNode(graphController);
            nodeList.Add(node);

            Assert.Throws<InvalidOperationException>(() =>
            {
                urr.RecordNodeDeletionForUndo(nodeList);
            });
        }

        [Test]
        public void TestRecordNodeDeletionForUndo01()
        {
            GraphController graphController = new GraphController(null);
            UndoRedoRecorder urr = new UndoRedoRecorder(graphController);

            urr.BeginGroup();
            urr.RecordNodeDeletionForUndo(null);
            Assert.AreEqual(0, urr.ActionCount);
        }

        [Test]
        public void TestRecordNodeDeletionForUndo02()
        {
            GraphController graphController = new GraphController(null);
            UndoRedoRecorder urr = new UndoRedoRecorder(graphController);

            urr.BeginGroup();
            urr.RecordNodeDeletionForUndo(new List<IVisualNode>());
            Assert.AreEqual(0, urr.ActionCount);
        }

        [Test]
        public void TestRecordNodeModficationForUndo00()
        {
            GraphController graphController = new GraphController(null);
            UndoRedoRecorder urr = new UndoRedoRecorder(graphController);

            List<IVisualNode> nodeList = new List<IVisualNode>();
            DriverNode node = new DriverNode(graphController);
            nodeList.Add(node);

            Assert.Throws<InvalidOperationException>(() =>
            {
                urr.RecordNodeModificationForUndo(nodeList);
            });
        }

        [Test]
        public void TestRecordNodeModificationForUndo01()
        {
            GraphController graphController = new GraphController(null);
            UndoRedoRecorder urr = new UndoRedoRecorder(graphController);

            urr.BeginGroup();
            urr.RecordNodeModificationForUndo(null);
            Assert.AreEqual(0, urr.ActionCount);
        }

        [Test]
        public void TestRecordNodeModificationForUndo02()
        {
            GraphController graphController = new GraphController(null);
            UndoRedoRecorder urr = new UndoRedoRecorder(graphController);

            urr.BeginGroup();
            urr.RecordNodeModificationForUndo(new List<IVisualNode>());
            Assert.AreEqual(0, urr.ActionCount);
        }

        [Test]
        public void TestPopRecordFromUndoStorage00()
        {
            GraphController graphController = new GraphController(null);
            UndoRedoRecorder urr = new UndoRedoRecorder(graphController);

            Assert.Throws<InvalidOperationException>(() =>
            {
                urr.PopRecordFromUndoStorage();
            });
        }

        [Test]
        public void TestUndo()
        {
            GraphController graphController = new GraphController(null);
            UndoRedoRecorder urr = new UndoRedoRecorder(graphController);

            DeltaNodes deltaNodes = new DeltaNodes();
            bool result = urr.Undo(deltaNodes);
            Assert.AreEqual(false, result);
        }

        [Test]
        public void TestRedo()
        {
            GraphController graphController = new GraphController(null);
            UndoRedoRecorder urr = new UndoRedoRecorder(graphController);

            DeltaNodes deltaNodes = new DeltaNodes();
            bool result = urr.Redo(deltaNodes);
            Assert.AreEqual(false, result);
        }

        [Test]
        public void TestUndoRedoForRuntimeStates()
        {
            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10353.0|d:10113.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:a =10;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:10565.0|d:10172.0|s:
                BeginNodeEdit|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,NorthWest|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000002|s:b = 20;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:b = 10;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController graphController = new GraphController(null);
            bool result00 = graphController.RunCommands(commands);
            Assert.AreEqual(true, result00);

            VisualNode node00 = graphController.GetVisualNode(0x10000001);
            VisualNode node01 = graphController.GetVisualNode(0x10000002);
            Assert.AreEqual("'b' is already defined.\n", node00.ErrorMessage);
            Assert.IsEmpty(node01.ErrorMessage);

            commands = @"UndoOperation";
            bool result01 = graphController.RunCommands(commands);
            Assert.AreEqual(true, result01);
            Assert.IsEmpty(node00.ErrorMessage);
            Assert.IsEmpty(node01.ErrorMessage);

            commands = @"RedoOperation";
            bool result02 = graphController.RunCommands(commands);
            Assert.AreEqual(true, result01);
            Assert.AreEqual("'b' is already defined.\n", node00.ErrorMessage);
            Assert.IsEmpty(node01.ErrorMessage);
        }

        [Test]
        public void TestUndoRedoForEdgeCreation()
        {
            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:15334.0|d:15155.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:10;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateIdentifierNode|d:15536.0|d:15194.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15352.0|d:15154.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15518.0|d:15195.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None";

            GraphController graphController = new GraphController(null);
            bool result00 = graphController.RunCommands(commands);
            Assert.AreEqual(true, result00);

            VisualEdge edge00 = graphController.GetVisualEdge(0x60000001);
            Assert.IsNotNull(edge00);

            commands = @"UndoOperation";
            bool result01 = graphController.RunCommands(commands);
            Assert.AreEqual(true, result01);

            VisualEdge edge01 = graphController.GetVisualEdge(0x60000001);
            Assert.IsNull(edge01);

            commands = @"RedoOperation";
            bool result02 = graphController.RunCommands(commands);
            Assert.AreEqual(true, result02);

            VisualEdge edge02 = graphController.GetVisualEdge(0x60000001);
            Assert.IsNotNull(edge02);
        }

        [Test]
        public void TestUndoRedoForEdgeDeletion()
        {
            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:15351.0|d:15180.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:10;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateIdentifierNode|d:15548.0|d:15199.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15374.0|d:15179.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15521.0|d:15200.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                ClearSelection
                SelectComponent|u:0x60000001|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents";

            GraphController graphController = new GraphController(null);
            bool result00 = graphController.RunCommands(commands);
            Assert.AreEqual(true, result00);

            VisualEdge edge00 = graphController.GetVisualEdge(0x60000001);
            Assert.IsNull(edge00);

            commands = @"UndoOperation";
            bool result01 = graphController.RunCommands(commands);
            Assert.AreEqual(true, result01);

            VisualEdge edge01 = graphController.GetVisualEdge(0x60000001);
            Assert.IsNotNull(edge01);

            commands = @"RedoOperation";
            bool result02 = graphController.RunCommands(commands);
            Assert.AreEqual(true, result02);

            VisualEdge edge02 = graphController.GetVisualEdge(0x60000001);
            Assert.IsNull(edge02);
        }

        [Test]
        public void TestUndoRedoForEdgeModification()
        {
            string commands = @"
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateCodeBlockNode|d:15359.0|d:15149.0|s:
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:-1|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:10;|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                CreateIdentifierNode|d:15542.0|d:15145.0
                CreateIdentifierNode|d:15542.0|d:15242.0
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,OutputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15380.0|d:15149.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15520.0|d:15144.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                ClearSelection
                SelectComponent|u:0x60000001|e:System.Windows.Input.ModifierKeys,None
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000002|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15521.0|d:15146.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None|d:15520.0|d:15243.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000003|e:DesignScriptStudio.Graph.Core.NodePart,InputSlot|i:0|e:System.Windows.Input.ModifierKeys,None";

            GraphController graphController = new GraphController(null);
            bool result00 = graphController.RunCommands(commands);
            Assert.AreEqual(true, result00);

            VisualEdge edge00 = graphController.GetVisualEdge(0x60000001);
            Assert.IsNotNull(edge00);
            Assert.AreEqual(0x30000001, edge00.StartSlotId);
            Assert.AreEqual(0x30000004, edge00.EndSlotId);

            commands = @"UndoOperation";
            bool result01 = graphController.RunCommands(commands);
            Assert.AreEqual(true, result01);

            VisualEdge edge01 = graphController.GetVisualEdge(0x60000001);
            Assert.IsNotNull(edge01);
            Assert.AreEqual(0x30000001, edge00.StartSlotId);
            Assert.AreEqual(0x30000002, edge00.EndSlotId);

            commands = @"RedoOperation";
            bool result02 = graphController.RunCommands(commands);
            Assert.AreEqual(true, result02);

            VisualEdge edge02 = graphController.GetVisualEdge(0x60000001);
            Assert.IsNotNull(edge02);
            Assert.AreEqual(0x30000001, edge00.StartSlotId);
            Assert.AreEqual(0x30000004, edge00.EndSlotId);
        }

        [Test]
        public void TestUndoRedoForNodeCreation()
        {
            string commands = @"
                CreateFunctionNode|d:525.0|d:275.0|s:Math.dll|s:Math.Sin|s:double";

            GraphController graphController = new GraphController(null);
            bool result00 = graphController.RunCommands(commands);
            Assert.AreEqual(true, result00);
            Assert.AreEqual(1, graphController.GetVisualNodes().Count);

            commands = @"UndoOperation";
            bool result01 = graphController.RunCommands(commands);
            Assert.AreEqual(true, result01);
            Assert.AreEqual(0, graphController.GetVisualNodes().Count);

            commands = @"RedoOperation";
            bool result02 = graphController.RunCommands(commands);
            Assert.AreEqual(true, result02);
            Assert.AreEqual(1, graphController.GetVisualNodes().Count);
            Assert.AreEqual(0x10000001, graphController.GetVisualNodes().ElementAt(0).NodeId);
        }

        [Test]
        public void TestUndoRedoForNodeDeletion()
        {
            string commands = @"
                CreateFunctionNode|d:574.0|d:249.0|s:Math.dll|s:Math.Sin|s:double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                DeleteComponents";

            GraphController graphController = new GraphController(null);
            bool result00 = graphController.RunCommands(commands);
            Assert.AreEqual(true, result00);
            Assert.AreEqual(0, graphController.GetVisualNodes().Count);

            commands = @"UndoOperation";
            bool result01 = graphController.RunCommands(commands);
            Assert.AreEqual(true, result01);
            Assert.AreEqual(1, graphController.GetVisualNodes().Count);
            Assert.AreEqual(0x10000001, graphController.GetVisualNodes().ElementAt(0).NodeId);

            commands = @"RedoOperation";
            bool result02 = graphController.RunCommands(commands);
            Assert.AreEqual(true, result02);
            Assert.AreEqual(0, graphController.GetVisualNodes().Count);
        }

        [Test]
        public void TestUndoRedoForNodeModification()
        {
            string commands = @"
                CreateFunctionNode|d:306.0|d:293.0|s:Math.dll|s:Math.Sin|s:double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None
                BeginDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None|d:419.0|d:310.0
                EndDrag|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None|d:940.0|d:305.0
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Caption|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController graphController = new GraphController(null);
            bool result00 = graphController.RunCommands(commands);
            Assert.AreEqual(true, result00);

            IVisualNode node = graphController.GetVisualNodes().ElementAt(0);
            double X = node.X;
            double Y = node.Y;
            
            commands = @"UndoOperation";
            bool result01 = graphController.RunCommands(commands);
            Assert.AreEqual(true, result01);
            Assert.AreEqual(306.0, node.X);
            Assert.AreEqual(293.0, node.Y);

            commands = @"RedoOperation";
            bool result02 = graphController.RunCommands(commands);
            Assert.AreEqual(true, result02);
            Assert.AreEqual(X, node.X);
            Assert.AreEqual(Y, node.Y);
        }

        [Test]
        public void TestUndoAfterPopRecordFromUndoStorage()
        {
            string commands = @"
                CreateFunctionNode|d:10461.0|d:10243.0|s:Math.dll|s:Math.Sin|s:double
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                BeginNodeEdit|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0x10000001|e:DesignScriptStudio.Graph.Core.NodePart,Text|i:0|e:System.Windows.Input.ModifierKeys,None
                EndNodeEdit|u:0x10000001|s:|b:True
                MouseDown|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None
                MouseUp|e:System.Windows.Input.MouseButton,Left|u:0xffffffff|e:DesignScriptStudio.Graph.Core.NodePart,None|i:-1|e:System.Windows.Input.ModifierKeys,None";

            GraphController graphController = new GraphController(null);
            bool result00 = graphController.RunCommands(commands);
            Assert.AreEqual(true, result00);

            commands = @"UndoOperation";
            bool result01 = graphController.RunCommands(commands);
            Assert.AreEqual(true, result01);
            Assert.AreEqual(0, graphController.GetVisualNodes().Count);
            Assert.AreEqual(0, graphController.GetSlots().Count);
        }
    }
}
