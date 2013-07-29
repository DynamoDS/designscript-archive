using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DesignScriptStudio.Graph.Core.VMServices;
using ProtoCore;
using ProtoScript.Runners;
using ProtoFFI;
using ProtoCore.DSASM.Mirror;
using System.Windows.Input;
using System.IO;
using GraphToDSCompiler;
using System.Windows.Media;
using System.Windows;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Xml.Serialization;
using ProtoCore.AST.AssociativeAST;
using System.Windows.Threading;

namespace DesignScriptStudio.Graph.Core
{
    class DraggedNode
    {
        internal IVisualNode node = null;

        #region Public Class Properties

        internal double DeltaX { get; set; }
        internal double DeltaY { get; set; }

        #endregion

        internal DraggedNode(IVisualNode node)
        {
            this.node = node;
            this.DeltaX = 0;
            this.DeltaY = 0;
        }
    }

    class DeltaNodes
    {
        #region Private Class Data Members

        private List<IVisualNode> removedNodes = null;
        private List<IVisualNode> addedNodes = null;
        private List<IVisualNode> modifiedNodes = null;
        private Dictionary<uint, List<string>> undefinitions = null;

        #endregion

        #region Public Class Operational Methods

        internal void Reset()
        {
            if (null != removedNodes)
                removedNodes.Clear();
            if (null != addedNodes)
                addedNodes.Clear();
            if (null != modifiedNodes)
                modifiedNodes.Clear();
        }

        internal void Compact()
        {
            // Priority based elimination process: whatever node that is in the 
            // "removedNodes" should not be in "addedNodes" or "modifiedNodes".
            // Whatever node that is in "addedNodes" should not be in "modifiedNodes".
            // 
            List<IVisualNode> removed = null, added = null, modified = null;

            if (null != this.removedNodes)
                removed = this.removedNodes.Distinct().ToList();

            if (null != this.addedNodes)
            {
                added = this.addedNodes.Distinct().ToList();
                if (null != removed)
                    added.RemoveAll((x) => (removed.Contains(x)));
            }

            if (null != this.modifiedNodes)
            {
                modified = this.modifiedNodes.Distinct().ToList();
                if (null != removed)
                    modified.RemoveAll((x) => (removed.Contains(x)));
                if (null != added)
                    modified.RemoveAll((x) => (added.Contains(x)));
            }

            // Update internal references.
            this.removedNodes = removed;
            this.addedNodes = added;
            this.modifiedNodes = modified;

            // Compact "undefinitions" if it is empty.
            if (null != undefinitions && (undefinitions.Count <= 0))
                undefinitions = null;
        }

        internal void AppendToRemovedNodes(IVisualNode node)
        {
            if (null != node)
            {
                if (null == removedNodes)
                    removedNodes = new List<IVisualNode>();
                removedNodes.Add(node);
            }
        }

        internal void AppendToRemovedNodes(List<IVisualNode> nodes)
        {
            if (null != nodes)
            {
                if (null == removedNodes)
                    removedNodes = new List<IVisualNode>();
                removedNodes.AddRange(nodes);
            }
        }

        internal void AppendToAddedNodes(IVisualNode node)
        {
            if (null != node)
            {
                if (null == addedNodes)
                    addedNodes = new List<IVisualNode>();
                addedNodes.Add(node);
            }
        }

        internal void AppendToAddedNodes(List<IVisualNode> nodes)
        {
            if (null != nodes)
            {
                if (null == addedNodes)
                    addedNodes = new List<IVisualNode>();
                addedNodes.AddRange(nodes);
            }
        }

        internal void AppendToModifiedNodes(IVisualNode node)
        {
            if (null != node)
            {
                if (null == modifiedNodes)
                    modifiedNodes = new List<IVisualNode>();
                modifiedNodes.Add(node);
            }
        }

        internal void AppendToModifiedNodes(List<IVisualNode> nodes)
        {
            if (null != nodes)
            {
                if (null == modifiedNodes)
                    modifiedNodes = new List<IVisualNode>();
                modifiedNodes.AddRange(nodes);
            }
        }

        internal void AppendUndefinedVariables(Dictionary<uint, List<string>> undefinedVariableMap)
        {
            if (null == undefinedVariableMap || (undefinedVariableMap.Count <= 0))
                return; // There's no variable that's being undefined at this time.

            // See "RuntimeStates.BeginDefinitionMonitor" method for details.
            if (null == undefinitions)
                undefinitions = new Dictionary<uint, List<string>>();

            foreach (KeyValuePair<uint, List<string>> undefinedVariables in undefinedVariableMap)
            {
                if (null == undefinedVariables.Value || (undefinedVariables.Value.Count <= 0))
                    continue;

                uint nodeId = undefinedVariables.Key; // The node which undefines one or more variables.
                if (!undefinitions.ContainsKey(nodeId))
                    undefinitions.Add(nodeId, new List<string>());

                List<string> destinationList = undefinitions[nodeId];
                foreach (string variable in undefinedVariables.Value)
                {
                    if (!destinationList.Contains(variable))
                        destinationList.Add(variable);
                }
            }
        }

        #endregion

        #region Public Class Properties

        internal bool IsEmpty
        {
            get
            {
                if (null != removedNodes && (removedNodes.Count > 0))
                    return false;
                if (null != addedNodes && (addedNodes.Count > 0))
                    return false;
                if (null != modifiedNodes && (modifiedNodes.Count > 0))
                    return false;
                return true;
            }
        }

        internal List<IVisualNode> RemovedNodes { get { return this.removedNodes; } }
        internal List<IVisualNode> AddedNodes { get { return this.addedNodes; } }
        internal List<IVisualNode> ModifiedNodes { get { return this.modifiedNodes; } }
        internal Dictionary<uint, List<string>> Undefinitions { get { return this.undefinitions; } }

        #endregion
    }

    struct ImplicitConnectionRequest
    {
        internal string variable;
        internal VisualNode definingNode;
        internal VisualNode referencedNode;
        internal List<IVisualNode> modifiedNodes;
        internal List<KeyValuePair<uint, uint>> existingImplicitEdges;
    }

    partial class GraphController : IGraphController
    {
        #region Class Data Members

        private IGraphVisualHost visualHost = null;
        private static uint identifierCounter = 100;

        // This is a singleton (with respect to one GraphController) synchronizer,
        // we now removed "ILiveRunner" instance in the "GraphController" so that 
        // we will never be able to access it directly (the only way is to go 
        // through "Synchronizer" class, which handles all the asynchronous calls.
        private Synchronizer synchronizer = null;
        private DispatcherTimer autoSaveTimer = null;

        private EdgeController edgeController = null;
        private GraphProperties graphProperties = null;
        private IdGenerator idGenerator = null;
        private UndoRedoRecorder undoRedoRecorder = null;
        private long undoStorageSavedPosition = 0;
        private long undoStoragePreProcessCommandPosition = -1;

        private List<string> recordedCommands = new List<string>();
        private List<DraggedNode> dragSet = new List<DraggedNode>();
        private DeltaNodes scheduledDeltaNodes = new DeltaNodes();
        private Dictionary<uint, ISlot> slotCollection = new Dictionary<uint, ISlot>();
        private Dictionary<uint, IVisualNode> nodeCollection = new Dictionary<uint, IVisualNode>();
        private Dictionary<uint, InfoBubble> bubbleCollection = new Dictionary<uint, InfoBubble>();

        private DragState currentDragState = DragState.None;
        private GraphCommand.Name currentCommand = GraphCommand.Name.None;
        private MenuItemsProvider itemsProvider = null;
        private uint lastHoveredNodeId = uint.MaxValue;

        //The file path that this graph is saved to.
        private string filePath = string.Empty;

        //The text value before editing
        private string previousText = string.Empty;

        //This data member works with Begin/EndHighFrequencyUpdate methods.
        private VisualNode nodeSelectedForHighFrequencyUpdate = null;

        //For NUint testing
        private SynchronizeDataCollection sdc = new SynchronizeDataCollection();

        //Selection Box
        SelectionBox selectionBox = null;

        #endregion

        #region Private Class Properties

        private bool IsInEndNodeEditCommand
        {
            get
            {
                //GC methods is only accessible for node editing operation
                return (currentCommand == GraphCommand.Name.EndNodeEdit);
            }
        }

        private bool IsSelectMenuItemCommand
        {
            get
            {
                //GC methods is only accessible for select menu item operation
                return (this.currentCommand == GraphCommand.Name.SelectMenuItem);
            }
        }

        private Synchronizer CurrentSynchronizer
        {
            get
            {
                if (synchronizer == null)
                    synchronizer = new Synchronizer(this);

                return synchronizer;
            }
        }

        #endregion

        #region Private Class Helper Methods

        private bool RerouteToHandler(GraphCommand command)
        {
            try
            {
                if (null != visualHost && (false != visualHost.IsInRecordingMode))
                    recordedCommands.Add(command.ToString());

                bool callResult = false;
                currentCommand = command.CommandName;
                this.PreProcessCommand(command.CommandName);

                switch (command.CommandName)
                {
                    case GraphCommand.Name.CreateCodeBlockNode:
                        callResult = HandleCreateCodeBlockNode(command);
                        break;
                    case GraphCommand.Name.CreateDriverNode:
                        callResult = HandleCreateDriverNode(command);
                        break;
                    case GraphCommand.Name.CreateFunctionNode:
                        callResult = HandleCreateFunctionNode(command);
                        break;
                    case GraphCommand.Name.CreateIdentifierNode:
                        callResult = HandleCreateIdentifierNode(command);
                        break;
                    case GraphCommand.Name.CreateRenderNode:
                        callResult = HandleCreateRenderNode(command);
                        break;
                    case GraphCommand.Name.CreatePropertyNode:
                        callResult = HandleCreatePropertyNode(command);
                        break;
                    case GraphCommand.Name.SaveGraph:
                        callResult = HandleSaveGraph(command);
                        break;
                    case GraphCommand.Name.MouseDown:
                        callResult = HandleMouseDown(command);
                        break;
                    case GraphCommand.Name.MouseUp:
                        callResult = HandleMouseUp(command);
                        break;
                    case GraphCommand.Name.BeginDrag:
                        callResult = HandleBeginDrag(command);
                        break;
                    case GraphCommand.Name.EndDrag:
                        callResult = HandleEndDrag(command);
                        break;
                    case GraphCommand.Name.BeginNodeEdit:
                        callResult = HandleBeginNodeEdit(command);
                        break;
                    case GraphCommand.Name.EndNodeEdit:
                        callResult = HandleEndNodeEdit(command);
                        break;
                    case GraphCommand.Name.BeginHighFrequencyUpdate:
                        callResult = HandleBeginHighFrequencyUpdate(command);
                        break;
                    case GraphCommand.Name.EndHighFrequencyUpdate:
                        callResult = HandleEndHighFrequencyUpdate(command);
                        break;
                    case GraphCommand.Name.DeleteComponents:
                        callResult = HandleDeleteComponents(command);
                        break;
                    case GraphCommand.Name.SelectComponent:
                        callResult = HandleSelectComponent(command);
                        break;
                    case GraphCommand.Name.ClearSelection:
                        callResult = HandleClearSelection(command);
                        break;
                    case GraphCommand.Name.UndoOperation:
                        callResult = HandleUndoOperation(command);
                        break;
                    case GraphCommand.Name.RedoOperation:
                        callResult = HandleRedoOperation(command);
                        break;
                    case GraphCommand.Name.EditReplicationGuide:
                        callResult = HandleEditReplicationGuide(command);
                        break;
                    case GraphCommand.Name.SetReplicationGuide:
                        callResult = HandleSetReplicationGuideText(command);
                        break;
                    case GraphCommand.Name.SelectMenuItem:
                        callResult = HandleSelectedMenuItem(command);
                        break;
                    case GraphCommand.Name.CreateRadialMenu:
                        callResult = HandleCreateRadialMenu(command);
                        break;
                    case GraphCommand.Name.CreateSubRadialMenu:
                        callResult = HandleCreateSubRadialMenu(command);
                        break;
                    case GraphCommand.Name.ImportScript:
                        callResult = HandleImportScript(command);
                        break;
                    case GraphCommand.Name.ConvertSelectionToCode:
                        callResult = HandleConvertSelectionToCode();
                        break;
                    case GraphCommand.Name.TogglePreview:
                        callResult = HandleTogglePreview(command);
                        break;
                }

                this.PostProcessCommand(this.currentCommand);
                currentCommand = GraphCommand.Name.None;
                return callResult;
            }
            catch (Exception exception)
            {
                if (null != visualHost && (false != visualHost.IsInRecordingMode))
                {
                    string filePath = this.DumpUiStatesToExternalFile(exception);
                    if (System.IO.File.Exists(filePath))
                        System.Diagnostics.Process.Start(filePath);

                    string xmlFilePath = this.DumpVmStatesToExternalFile();
                    if (System.IO.File.Exists(xmlFilePath))
                        System.Diagnostics.Process.Start(xmlFilePath);
                }
                throw exception; // Rethrow to external handler.
            }

            throw new InvalidOperationException("Unhandled command in 'RerouteToHandler'");
        }

        private void BeginDragSelectionRegion(ModifierKeys modifiers)
        {
            if (!modifiers.HasFlag(ModifierKeys.Control) && !modifiers.HasFlag(ModifierKeys.Shift))
                ClearSelectionInternal();

            currentDragState = DragState.RegionSelection;

            if (visualHost != null)
                this.visualHost.BeginDragSelection();
        }

        private bool IsInDragSet(VisualNode node, List<DraggedNode> dragSet)
        {
            foreach (DraggedNode draggedNode in dragSet)
            {
                if (draggedNode.node == node)
                    return true;
            }
            return false;
        }

        private void BeginDragSelectedNodes(double mouseX, double mouseY, VisualNode node)
        {
            if (node.Selected)
            {
                List<IVisualNode> nodeList = new List<IVisualNode>();
                foreach (IVisualNode visualNode in nodeCollection.Values)
                {
                    if (((VisualNode)visualNode).Selected)
                    {
                        nodeList.Add(visualNode);

                        DraggedNode draggedNode = new DraggedNode(visualNode);
                        draggedNode.DeltaX = mouseX - ((VisualNode)visualNode).X;
                        draggedNode.DeltaY = mouseY - ((VisualNode)visualNode).Y;
                        dragSet.Add(draggedNode);

                        edgeController.SelectEdges(visualNode);

                        //both node that the edge is connected to should be bring to front
                        foreach (VisualNode connectingNode in edgeController.GetNodesFromSelectedEdges())
                            RearrangeNodeAndBubbleVisual(connectingNode, true);
                    }
                }

                if (nodeList.Count > 0)
                {
                    this.undoRedoRecorder.BeginGroup();
                    this.undoRedoRecorder.RecordNodeModificationForUndo(nodeList);
                    edgeController.BeginUpdateEdge();
                    this.undoRedoRecorder.EndGroup();
                }
            }
            currentDragState = DragState.NodeRepositioning;

            selectionBox.SetRelativePositionToMouse(mouseX, mouseY);
        }

        private void BeginDragInputOutputSlot(uint compId, NodePart nodePart, int slotIndex)
        {
            IVisualNode node = null;
            nodeCollection.TryGetValue(compId, out node);

            ((VisualNode)node).Selected = true;

            uint slotId = uint.MaxValue;
            if (nodePart == NodePart.OutputSlot)
            {
                slotId = node.GetOutputSlot(slotIndex);
                edgeController.SetCurrentlySelectedSlotType(SlotType.Output);
            }
            else if (nodePart == NodePart.InputSlot)
            {
                slotId = node.GetInputSlot(slotIndex);
                edgeController.SetCurrentlySelectedSlotType(SlotType.Input);
            }
            else
            {
                // 'BeginDragInputOutputSlot' cannot be called for things other than slots.
                throw new InvalidOperationException("Invalid call ot 'BeginDragInputOutputSlot'!");
            }

            List<VisualEdge> reconnectEdges = edgeController.FindEdgesConnectingToSlot(slotId, true);
            if (reconnectEdges.Count() > 0)
            {
                List<uint> connectingSlots = new List<uint>();
                List<uint> connectingEdges = new List<uint>();
                edgeController.ResetAllEdges();
                if (GetSlot(slotId).SlotType == SlotType.Output)
                {
                    foreach (VisualEdge edge in reconnectEdges)
                    {
                        edgeController.ClearEdgeVisual(edge.EdgeId);
                        connectingSlots.Add(edge.EndSlotId);
                        connectingEdges.Add(edge.EdgeId);
                    }
                }
                else
                {
                    foreach (VisualEdge edge in reconnectEdges)
                    {
                        edgeController.ClearEdgeVisual(edge.EdgeId);
                        connectingSlots.Add(edge.StartSlotId);
                        connectingEdges.Add(edge.EdgeId);
                    }
                }
                edgeController.SetConnectedSlotId(slotId);
                edgeController.SetReconnectingEdges(connectingEdges);
                edgeController.SetReconnectingSlots(connectingSlots);
                this.currentDragState = DragState.EdgeReconnection;

                foreach (uint connectingSlotId in connectingSlots)
                {
                    VisualNode visualNode = GetVisualNode(GetSlot(connectingSlotId).Owners.Last());
                    visualNode.Selected = true;
                    visualNode.Compose();
                }
                ((VisualNode)node).Selected = false;
            }
            else // Normal drawing edge
            {
                edgeController.SetCurrentlySelectedNodeId(node.NodeId);
                edgeController.SetCurrentlySelectedSlotId(slotId);
                edgeController.ResetAllEdges();

                this.currentDragState = DragState.CurveDrawing;
            }
        }

        private void ClearSelectionInternal()
        {
            foreach (IVisualNode visualNode in nodeCollection.Values)
            {
                VisualNode node = visualNode as VisualNode;
                if (!node.Selected && (!node.PreviewSelected))
                    continue;

                RearrangeNodeAndBubbleVisual(node, false);
                node.Selected = false; // Clear selection flag.
                node.Compose();
            }

            edgeController.ResetAllEdges();
        }

        // if it is node, once it is selected, it will be brought to the top
        // if it is bubble, if its selected, it will follow its owners' z-index, if unselected, send to back,
        // edge is different, when the edge is moving, it is drawn in another drawingVisual which is topmost,
        // when the edge moving is done, it will draw on the old/own drawingVisual
        //
        private void RearrangeNodeAndBubbleVisual(VisualNode node, bool bringToFront)
        {
            if (null == visualHost)
                return;

            uint errorBubbleId = node.GetErrorBubbleId();
            uint previewBubbleId = node.GetPreviewBubbleId();

            if (bringToFront)
            {
                visualHost.RearrangeDrawingVisual(node.NodeId, bringToFront, uint.MaxValue);

                if (errorBubbleId != uint.MaxValue)
                    visualHost.RearrangeDrawingVisual(errorBubbleId, bringToFront, node.NodeId);

                if (previewBubbleId != uint.MaxValue)
                    visualHost.RearrangeDrawingVisual(previewBubbleId, bringToFront, node.NodeId);
            }
            else
            {
                if (errorBubbleId != uint.MaxValue)
                    visualHost.RearrangeDrawingVisual(errorBubbleId, bringToFront, uint.MaxValue);

                if (previewBubbleId != uint.MaxValue)
                    visualHost.RearrangeDrawingVisual(previewBubbleId, bringToFront, uint.MaxValue);
            }
        }

        private void LoadFileInternal(string filePath)
        {
            FileStorage storage = new FileStorage();
            DataHeader header = new DataHeader();
            storage.Load(filePath);

            //Deserialize states
            header.Deserialize(storage);
            this.graphProperties.Deserialize(storage);

            foreach (string scriptPath in this.graphProperties.ImportedScripts)
            {
                CoreComponent.Instance.ImportAssembly(scriptPath, filePath, true);
            }

            //Deserialize slots
            int slotCount = storage.ReadInteger(FieldCode.SlotCount);
            for (int i = 0; i < slotCount; i++)
            {
                header.Deserialize(storage);
                ISlot slot = Slot.Create(this, storage);
                this.AddSlot(slot);
            }

            //Deserialize nodes
            int nodeCount = storage.ReadInteger(FieldCode.NodeCount);
            for (int j = 0; j < nodeCount; j++)
            {
                header.Deserialize(storage);
                IVisualNode node = VisualNode.Create(this, storage);
                this.AddVisualNode(node);
            }

            //Deserialize edges
            int edgeCount = storage.ReadInteger(FieldCode.EdgeCount);
            for (int j = 0; j < edgeCount; j++)
            {
                header.Deserialize(storage);
                IVisualEdge edge = VisualEdge.Create(this.edgeController, storage);
                this.AddVisualEdge(edge);
            }

            //AuditLoadedGraph();

            // These calls internally figure out the type of id 
            // it is meant for, and set the value accordingly.
            if (nodeCount > 0)
                this.idGenerator.SetStartId(this.nodeCollection.Keys.Max());
            if (slotCount > 0)
                this.idGenerator.SetStartId(this.slotCollection.Keys.Max());
            if (edgeCount > 0)
                this.idGenerator.SetStartId(this.edgeController.GetMaxEdgeId());
        }

        private void AuditLoadedGraph()
        {
            // node.Audit, slot.Audit
            // Check for edges
            edgeController.AuditEdges();
        }

        private void UpdateImportedLibrary()
        {
            List<string> importedLibrary = new List<string>();

            foreach (VisualNode node in nodeCollection.Values)
            {
                string library = node.GetAssembly();
                string libraryPath = this.MapAssemblyPath(library);
                if (library != libraryPath)
                {
                    importedLibrary.Add(libraryPath);
                }
            }
            this.graphProperties.ImportedScripts.Clear();
            this.graphProperties.ImportedScripts = importedLibrary;
        }

        private void SaveFileInternal(string filePath)
        {
            IStorage storage = new FileStorage();
            DataHeader header = new DataHeader();
            long initialPosition, currentPosition = 0;

            //Serialize states
            storage.Seek(header.HeaderSize, SeekOrigin.Current);
            initialPosition = storage.GetPosition();
            this.graphProperties.Serialize(storage);
            currentPosition = storage.GetPosition();
            header.DataSize = currentPosition - initialPosition;
            storage.Seek(-(header.HeaderSize + header.DataSize), SeekOrigin.Current);
            header.Serialize(storage);
            storage.Seek(header.DataSize, SeekOrigin.Current);

            //Serialize slots
            List<ISlot> slotList = new List<ISlot>(this.slotCollection.Values);
            storage.WriteInteger(FieldCode.SlotCount, slotList.Count);
            foreach (ISlot slot in slotList)
            {
                storage.Seek(header.HeaderSize, SeekOrigin.Current);
                initialPosition = storage.GetPosition();
                slot.Serialize(storage);
                currentPosition = storage.GetPosition();
                header.DataSize = currentPosition - initialPosition;
                storage.Seek(-(header.HeaderSize + header.DataSize), SeekOrigin.Current);
                header.Serialize(storage);
                storage.Seek(header.DataSize, SeekOrigin.Current);
            }

            //Serialize nodes
            List<IVisualNode> nodeList = new List<IVisualNode>(this.nodeCollection.Values);
            storage.WriteInteger(FieldCode.NodeCount, nodeList.Count);
            foreach (IVisualNode node in nodeList)
            {
                storage.Seek(header.HeaderSize, SeekOrigin.Current);
                initialPosition = storage.GetPosition();
                node.Serialize(storage);
                currentPosition = storage.GetPosition();
                header.DataSize = currentPosition - initialPosition;
                storage.Seek(-(header.HeaderSize + header.DataSize), SeekOrigin.Current);
                header.Serialize(storage);
                storage.Seek(header.DataSize, SeekOrigin.Current);
            }

            //Serialize edges
            List<IVisualEdge> edgeList = this.edgeController.GetVisualEdges();
            storage.WriteInteger(FieldCode.EdgeCount, edgeList.Count);
            foreach (IVisualEdge edge in edgeList)
            {
                storage.Seek(header.HeaderSize, SeekOrigin.Current);
                initialPosition = storage.GetPosition();
                edge.Serialize(storage);
                currentPosition = storage.GetPosition();
                header.DataSize = currentPosition - initialPosition;
                storage.Seek(-(header.HeaderSize + header.DataSize), SeekOrigin.Current);
                header.Serialize(storage);
                storage.Seek(header.DataSize, SeekOrigin.Current);
            }

            ((FileStorage)storage).Save(filePath);
        }

        private void InitializeInternal(IGraphVisualHost visualHost)
        {
            //The identity of this graph controller
            this.Identifier = GraphController.identifierCounter++;

            //Optional Object
            this.visualHost = visualHost;

            //Essential Object
            this.edgeController = new EdgeController(this);
            this.graphProperties = new GraphProperties();
            this.undoRedoRecorder = new UndoRedoRecorder(this);

            DrawingVisual visual = new DrawingVisual();
            this.selectionBox = new SelectionBox(this, this.visualHost);

            // Create a timer for auto-saving in the background.
            this.autoSaveTimer = new DispatcherTimer();
            this.autoSaveTimer.Interval = TimeSpan.FromSeconds(58);
            this.autoSaveTimer.Tick += OnAutoSaveTimerTicked;
        }

        private void CreateNodeInternal(VisualNode node, double x, double y)
        {
            if (null == node)
                throw new ArgumentNullException("node");

            if (CoreComponent.Instance.StudioSettings.SuppressPreview)
                node.SetNodeState(States.PreviewHidden);

            node.X = x;
            node.Y = y;
            node.Compose(); // Just to compute the dimension.

            DeltaNodes deltaNodes = new DeltaNodes();
            deltaNodes.AppendToAddedNodes(node);

            if (node.VisualType != NodeType.CodeBlock)
            {
                bool visualOnlyNode = (node.VisualType == NodeType.Condensed ||
                    (node.VisualType == NodeType.Render));

                this.undoRedoRecorder.BeginGroup();
                this.undoRedoRecorder.RecordRuntimeStatesForUndo(this.graphProperties.RuntimeStates);
                this.undoRedoRecorder.RecordNodeCreationForUndo(deltaNodes.AddedNodes);
                this.graphProperties.RuntimeStates.AddVariablesDefinedInNode(node, false);

                if (false == visualOnlyNode)
                {
                    List<IVisualNode> modifiedNodes = new List<IVisualNode>();
                    EstablishImplicitConnections(modifiedNodes);
                    deltaNodes.AppendToModifiedNodes(modifiedNodes);
                }

                this.undoRedoRecorder.EndGroup();

                if (false == visualOnlyNode)
                    this.SynchronizeToLiveRunner(deltaNodes);
            }

            this.UpdateDirtyNodes();
            node.PositionAtCenter(node.NodeId);
        }

        private void UpdateDirtyNodes()
        {
            foreach (VisualNode node in this.nodeCollection.Values)
            {
                if (node.Dirty == true)
                    node.Compose();
            }
        }

        private void UpdateSlotsVisiblity()
        {
            foreach (VisualNode node in this.nodeCollection.Values)
                node.UpdateSlotVisibility();
        }

        private void DeleteNodes(List<IVisualNode> nodesToDelete)
        {
            if (this.IsInUndoRedoCommand)
                throw new InvalidOperationException("Cannot be access for undo/redo");

            if (null == nodesToDelete || (nodesToDelete.Count <= 0))
                return;

            foreach (VisualNode nodeToDelete in nodesToDelete)
            {
                //Delete connected edge and edge's drawing visual
                this.edgeController.DeleteEdgesConnectTo(nodeToDelete);

                //Delete node's drawing visual and bubble's drawing visual
                if (null != visualHost)
                {
                    this.visualHost.RemoveDrawingVisual(nodeToDelete.NodeId);
                    this.visualHost.RemoveDrawingVisual(nodeToDelete.GetErrorBubbleId());
                    this.visualHost.RemoveDrawingVisual(nodeToDelete.GetPreviewBubbleId());
                    this.visualHost.RemoveExtendedBubble(nodeToDelete.GetPreviewBubbleId());
                }

                //Clear node's error
                nodeToDelete.SetErrorValue(null, ErrorType.None);

                //Delete defined variable mapping
                RuntimeStates runtimeStates = this.graphProperties.RuntimeStates;
                runtimeStates.RemoveVariablesDefinedInNode(nodeToDelete);

                //Delete nodes and slots
                uint[] outputSlots = nodeToDelete.GetOutputSlots();
                uint[] inputSlots = nodeToDelete.GetInputSlots();
                if (inputSlots != null)
                {
                    foreach (uint inputSlot in inputSlots)
                        this.slotCollection.Remove(inputSlot);
                }
                if (outputSlots != null)
                {
                    foreach (uint outputSlot in outputSlots)
                        this.slotCollection.Remove(outputSlot);
                }
                this.nodeCollection.Remove(nodeToDelete.NodeId);
                this.bubbleCollection.Remove(nodeToDelete.GetErrorBubbleId());
                this.bubbleCollection.Remove(nodeToDelete.GetPreviewBubbleId());
            }
        }

        private void PreProcessCommand(GraphCommand.Name commandName)
        {
            if (commandName == GraphCommand.Name.SaveGraph)
                this.IsModified = true;

            undoStoragePreProcessCommandPosition = undoRedoRecorder.GetUndoPosition();

            //case GraphCommand.Name.SaveGraph:
            //    this.IsModified = false;
            //    break;

            //case GraphCommand.Name.UndoOperation:
            //case GraphCommand.Name.RedoOperation:
            //    // Undo/redo does nothing for modified state.
            //    break;

            //default:
            //    this.IsModified = true;
            //    break;
        }

        private void PostProcessCommand(GraphCommand.Name commandName)
        {
            // The auto-saving timer expires in a given interval, and then it 
            // performs auto-saving for recovery later. Once saved, the timer 
            // is stopped, until user performs an action that alters the graph 
            // content (which is through graph commands channeling through the
            // "RerouteToHandler" method). When the graph is modified, the 
            // timer then restarts here and wait for the interval to expire.
            // 
            if (null != autoSaveTimer && (false == autoSaveTimer.IsEnabled))
            {
                autoSaveTimer.Stop(); // Just to be 100% sure.
                autoSaveTimer.Start(); // Restart timer.
            }

            long undoStoragePosition = undoRedoRecorder.GetUndoPosition();

            switch (commandName)
            {
                case GraphCommand.Name.UndoOperation:
                case GraphCommand.Name.RedoOperation:
                    break;
                case GraphCommand.Name.SaveGraph:
                    undoStorageSavedPosition = undoStoragePosition;
                    break;
                default:
                    // for the case of action -> save -> undo ->action
                    // then the saved state will never be recovered from undoStorage
                    //
                    if (undoStoragePreProcessCommandPosition > undoStoragePosition // the command actually does some actions
                        && undoStorageSavedPosition >= undoStoragePosition)        // which makes the saved state unreachable
                        undoStorageSavedPosition = 0;
                    break;
            }

            if (undoStorageSavedPosition == undoStoragePosition)
                this.IsModified = false;
            else
                this.IsModified = true;

            undoStoragePreProcessCommandPosition = -1;
        }

        private void ScheduleNodesForValueQuery(List<IVisualNode> nodes)
        {
            // During the course of node editing, some unrelated nodes 
            // may have become valid again (e.g. the erroneous node was 
            // caused by a variable defined in another node that is 
            // currently being removed, making the node valid again).
            // For such cases, the GraphController provides a mechanism 
            // to schedule nodes for query after each update is pushed.
            scheduledDeltaNodes.AppendToModifiedNodes(nodes);
        }

        private List<IVisualNode> FindAssociatedNodesFromNodes(List<IVisualNode> nodeList)
        {
            if (nodeList == null)
                throw new ArgumentNullException("nodeList");

            List<uint> nodeIdList = new List<uint>();
            foreach (IVisualNode node in nodeList)
            {
                List<uint> slotIdList = new List<uint>();
                if (node.GetInputSlots() != null)
                    slotIdList.AddRange(node.GetInputSlots());
                if (node.GetOutputSlots() != null)
                    slotIdList.AddRange(node.GetOutputSlots());

                List<uint> connectingSlotIdList = new List<uint>();
                foreach (uint slotId in slotIdList)
                {
                    ISlot slot = this.GetSlot(slotId);
                    if (slot.ConnectingSlots != null)
                        connectingSlotIdList.AddRange(slot.ConnectingSlots);
                }

                foreach (uint slotId in connectingSlotIdList)
                {
                    ISlot slot = this.GetSlot(slotId);
                    nodeIdList.AddRange(slot.Owners);
                }
            }

            List<IVisualNode> associatedNodes = new List<IVisualNode>();
            List<uint> distinctNodeIdList = nodeIdList.Distinct().ToList<uint>();
            foreach (uint nodeId in distinctNodeIdList)
            {
                IVisualNode node = this.GetVisualNode(nodeId);
                if (!(nodeList.Contains(node)))
                    associatedNodes.Add(node);
            }

            return associatedNodes;
        }

        private List<SnapshotNode> CreateSnapshotNodesFromVisualNodes(List<IVisualNode> nodeList)
        {
            if (nodeList == null)
                throw new ArgumentNullException("nodeList");

            List<SnapshotNode> ssnList = new List<SnapshotNode>();
            foreach (VisualNode node in nodeList)
            {
                if (false != node.Error) // Ignore erroneous node.
                    continue;

                SnapshotNodeType type = SnapshotNodeType.None;
                string content = node.ToCode(out type);
                SnapshotNode ssn = new SnapshotNode(node.NodeId, type, content);
                node.GetAssignmentStatements(ssn.Assignments);
                ssn.InputList = new List<Connection>();
                ssn.OutputList = new List<Connection>();

                List<uint> localSlotIdList = new List<uint>();
                if (node.GetInputSlots() != null)
                    localSlotIdList.AddRange(node.GetInputSlots());
                if (node.GetOutputSlots() != null)
                    localSlotIdList.AddRange(node.GetOutputSlots());

                foreach (uint localSlotId in localSlotIdList)
                {
                    ISlot localSlot = this.GetSlot(localSlotId);
                    if (localSlot.ConnectingSlots != null)
                    {
                        foreach (uint otherSlotId in localSlot.ConnectingSlots)
                        {
                            ISlot otherSlot = this.GetSlot(otherSlotId);
                            uint otherNodeId = otherSlot.Owners[0];
                            VisualNode otherNode = this.GetVisualNode(otherNodeId);
                            if (false != otherNode.Error) // Ignore erroneous node.
                                continue;
                            uint edgeId;
                            if (localSlot.SlotType == SlotType.Input)
                                edgeId = edgeController.GetEdgeId(otherSlotId, localSlotId);
                            else
                                edgeId = edgeController.GetEdgeId(localSlotId, otherSlotId);
                            bool isImplicitConnection = edgeController.isImplicitEdge(edgeId);

                            ssn.ConnectTo(
                                otherNodeId,                            // Connect to UID
                                node.GetSlotIndex(localSlotId),         // Local slot index of this node
                                otherNode.GetSlotIndex(otherSlotId),    // Destination node slot index 
                                SlotType.Input == localSlot.SlotType,   // Is this node an input
                                isImplicitConnection,                   //is edge implicit
                                ((Slot)localSlot).GetSlotName());       // The name of the local slot
                        }
                    }
                }

                ssnList.Add(ssn);
            }
            return ssnList;
        }

        private List<VisualNode> CreateVisualNodesFromSnapshotNodes(List<SnapshotNode> snapshotNodes)
        {
            List<IVisualNode> visualNodes = new List<IVisualNode>();
            foreach (SnapshotNode snapshotNode in snapshotNodes)
            {
                VisualNode visualNode = new CodeBlockNode(this, snapshotNode);
                visualNodes.Add(visualNode);
                this.graphProperties.RuntimeStates.AddVariablesDefinedInNode(visualNode, true);
            }
            undoRedoRecorder.RecordNodeCreationForUndo(visualNodes);
            ValidateDefinedAndReferencedVariables();

            List<VisualNode> resultVisualNodes = new List<VisualNode>();
            foreach (VisualNode node in visualNodes)
                resultVisualNodes.Add(node);
            return resultVisualNodes;
        }

        private void UpdateSnapshotNodeIdsByVisualNodes(List<SnapshotNode> snapshotNodes, List<IVisualNode> visualNodes)
        {
            if (snapshotNodes.Count != visualNodes.Count)
                throw new InvalidOperationException("The number of node in snapshotNodeList and visualNodeList must be equal");
            for (int i = 0; i < snapshotNodes.Count; i++)
            {
                snapshotNodes[i].Id = visualNodes[i].NodeId;
            }
        }

        private void TransformBubble(VisualNode node, TranslateTransform transform)
        {
            uint errorBubbleId = node.GetErrorBubbleId();
            uint previewBubbleId = node.GetPreviewBubbleId();

            if (errorBubbleId != uint.MaxValue)
            {
                DrawingVisual errorVisual = visualHost.GetDrawingVisualForBubble(errorBubbleId);
                //RearrangeVisual(errorBubbleId, true);
                errorVisual.Transform = transform;
            }

            if (previewBubbleId != uint.MaxValue)
            {
                DrawingVisual previewVisual = visualHost.GetDrawingVisualForBubble(previewBubbleId);
                //RearrangeVisual(previewBubbleId, true);
                previewVisual.Transform = transform;
            }
        }

        private List<IVisualNode> GetSelectedNodes()
        {
            List<IVisualNode> selectedNodes = new List<IVisualNode>();
            foreach (VisualNode node in nodeCollection.Values)
            {
                if (node.Selected)
                    selectedNodes.Add(node);
            }
            return selectedNodes;
        }

        private void DeleteNodesWithRecord(List<IVisualNode> nodes, out List<IVisualNode> modifiedNodes)
        {
            modifiedNodes = FindAssociatedNodesFromNodes(nodes);

            List<IVisualNode> nodesConnectToEdge = edgeController.GetNodesFromSelectedEdges();
            foreach (IVisualNode node in nodesConnectToEdge)
            {
                bool inDeleteList = (null != nodes && (nodes.Contains(node)));
                bool inModifyList = (null != modifiedNodes && (modifiedNodes.Contains(node)));
                if (!inDeleteList && !inModifyList)
                    modifiedNodes.Add(node);
            }
            this.undoRedoRecorder.RecordNodeDeletionForUndo(nodes);
            this.undoRedoRecorder.RecordNodeModificationForUndo(modifiedNodes);
            foreach (VisualNode node in nodes)
                edgeController.DeleteEdgesConnectTo(node);
            DeleteNodes(nodes);
            ValidateDefinedAndReferencedVariables();
            UpdateSlotsVisiblity();
            UpdateDirtyNodes();
        }

        private void EstablishExplicitConnections(Dictionary<VisualNode, List<Connection>> inputNodeConnectionMap, Dictionary<VisualNode, List<Connection>> outputNodeConnectionMap, List<IVisualNode> modifiedNodes)
        {


            foreach (VisualNode node in inputNodeConnectionMap.Keys)
            {
                foreach (Connection connection in inputNodeConnectionMap[node])
                {
                    if (!connection.IsImplicit)
                        EstablishExplicitInputConnection(node, connection, modifiedNodes);
                }
            }

            foreach (VisualNode node in outputNodeConnectionMap.Keys)
            {
                foreach (Connection connection in outputNodeConnectionMap[node])
                {
                    if (!connection.IsImplicit)
                        EstablishExplicitOutputConnection(node, connection, modifiedNodes);
                }
            }
        }

        private void EstablishExplicitInputConnection(VisualNode endNode, Connection connection, List<IVisualNode> modifiedNodes)
        {
            VisualNode startNode = (VisualNode)nodeCollection[connection.OtherNode];
            uint startSlotId = startNode.GetOutputSlot(connection.OtherIndex);
            uint endSlotId = endNode.GetInputSlot(connection.LocalIndex);
            Slot outputSlot = GetSlot(startSlotId) as Slot;
            Slot inputSlot = GetSlot(endSlotId) as Slot;
            edgeController.CreateLinkingEdge(outputSlot, inputSlot);
            startNode.HandleNewConnection(startSlotId);
            endNode.HandleNewConnection(endSlotId);

            modifiedNodes.Add(startNode);
            modifiedNodes.Add(endNode);
        }

        private void EstablishExplicitOutputConnection(VisualNode startNode, Connection connection, List<IVisualNode> modifiedNodes)
        {
            VisualNode endNode = nodeCollection[connection.OtherNode] as VisualNode;
            List<uint> startSlotIds = (startNode as VisualNode).GetSlotIdsByName(SlotType.Output, connection.LocalName);
            uint startSlotId = startSlotIds.Last();
            uint endSlotId = endNode.GetInputSlot(connection.OtherIndex);
            Slot outputSlot = GetSlot(startSlotId) as Slot;
            Slot inputSlot = GetSlot(endSlotId) as Slot;
            edgeController.CreateLinkingEdge(outputSlot, inputSlot);
            (startNode as VisualNode).HandleNewConnection(startSlotId);
            endNode.HandleNewConnection(endSlotId);

            modifiedNodes.Add(startNode);
            modifiedNodes.Add(endNode);
        }

        private List<Connection> GetInputConnectionsFromSnapshotNodes(List<SnapshotNode> snapshotNodes, bool includeImplicitConnections)
        {
            List<Connection> inputConnections = new List<Connection>();
            foreach (SnapshotNode snapshotNode in snapshotNodes)
            {
                foreach (Connection connection in snapshotNode.InputList)
                {
                    if (includeImplicitConnections || !connection.IsImplicit)
                        inputConnections.Add(connection);
                }
            }

            return inputConnections;
        }

        private List<VisualNode> CreateIntermediateIdentifierNodes(List<SnapshotNode> snapshotNodes, out List<Connection> inputConnections, out Dictionary<string, string> identifierTempVariablesMap)
        {
            // Mapping the old temporary variable to the name of the new identifier node.
            identifierTempVariablesMap = new Dictionary<string, string>();
            // Mapping an external slot to a local variable (in the current CBN).
            Dictionary<uint, string> externalSlotIdentifierVariableMap = new Dictionary<uint, string>();
            List<VisualNode> identifierNodes = new List<VisualNode>();
            inputConnections = new List<Connection>();

            List<Connection> connections = GetInputConnectionsFromSnapshotNodes(snapshotNodes, false);
            foreach (Connection connection in connections)
            {
                if (!Utilities.IsTempVariable(connection.LocalName))
                    continue;
                VisualNode otherNode = (VisualNode)this.nodeCollection[connection.OtherNode];

                uint connectingSlotId = otherNode.GetOutputSlot(connection.OtherIndex);
                if (connectingSlotId == uint.MaxValue)
                    continue;

                if (!externalSlotIdentifierVariableMap.ContainsKey(connectingSlotId))
                {
                    VisualNode identifierNode = CreateIdentifierNode();
                    string identifierName = identifierNode.Caption;
                    identifierTempVariablesMap.Add(identifierName, connection.LocalName.Trim());
                    externalSlotIdentifierVariableMap.Add(connectingSlotId, identifierName);
                    identifierNodes.Add(identifierNode);
                    inputConnections.Add(CreateConnectionStructure(connection.OtherNode, connection.OtherIndex, 0, identifierName, false));
                }
            }
            return identifierNodes;
        }

        private VisualNode CreateIdentifierNode()
        {
            VisualNode identifierNode = new IdentifierNode(this, "Identifier");
            List<IVisualNode> nodes = new List<IVisualNode>();
            nodes.Add(identifierNode);
            this.undoRedoRecorder.RecordNodeCreationForUndo(nodes);
            return identifierNode;
        }

        private void ReplaceVariableNamesInSnapshotNodes(Dictionary<string, string> oldNewVariableMap, List<SnapshotNode> snapshotNodes)
        {
            foreach (SnapshotNode node in snapshotNodes)
            {
                foreach (string newVariable in oldNewVariableMap.Keys)
                {
                    node.Content = node.Content.Replace(oldNewVariableMap[newVariable], newVariable);
                }
                node.Content = node.Content.Replace("temp_NULL", "null");
            }
        }

        private Connection CreateConnectionStructure(uint otherNodeId, int otherIndex, int localIndex, string localName, bool isImplicit)
        {
            Connection connection = new Connection();
            connection.IsImplicit = isImplicit;
            connection.LocalIndex = localIndex;
            connection.LocalName = localName;
            connection.OtherIndex = otherIndex;
            connection.OtherNode = otherNodeId;
            return connection;
        }

        private Dictionary<VisualNode, List<Connection>> GetNodeOutputConnectionMapFromSnapshotNodes(List<SnapshotNode> snapshotNodes, List<VisualNode> visualNodes)
        {
            Dictionary<VisualNode, List<Connection>> nodeConnectionMap = new Dictionary<VisualNode, List<Connection>>();
            for (int i = 0; i < visualNodes.Count; i++)
            {
                List<Connection> connections = new List<Connection>();
                foreach (Connection connection in snapshotNodes[i].OutputList)
                    connections.Add(connection);
                nodeConnectionMap.Add(visualNodes[i], connections);
            }
            return nodeConnectionMap;
        }

        private Dictionary<VisualNode, List<Connection>> GetNodeInputConnectionMapFromSnapshotNodes(List<VisualNode> visualNodes, List<Connection> connections)
        {
            Dictionary<VisualNode, List<Connection>> nodeConnectionMap = new Dictionary<VisualNode, List<Connection>>();
            for (int i = 0; i < visualNodes.Count; i++)
            {
                List<Connection> tempConnections = new List<Connection>();
                tempConnections.Add(connections[i]);
                nodeConnectionMap.Add(visualNodes[i], tempConnections);
            }
            return nodeConnectionMap;
        }

        private List<IVisualNode> GetNodesByIds(List<uint> nodeIds)
        {
            List<IVisualNode> nodes = new List<IVisualNode>();
            foreach (uint id in nodeIds)
                nodes.Add(nodeCollection[id]);
            return nodes;
        }

        private void SetNodesPosition(List<IVisualNode> nodes, double x, double y)
        {
            //this is the temporary positioning solution only, this will be change later
            foreach (IVisualNode node in nodes)
            {
                VisualNode visNode = node as VisualNode;
                visNode.X = x;
                visNode.Y = y;
                visNode.Dirty = true;
                y += 40;
            }
        }

        private void MergeUndefinedVariablesIntoData(Dictionary<uint, List<string>> undefinitions, SynchronizeData sd)
        {
            // For more details please see "RuntimeStates.BeginDefinitionMonitor".
            if (null == undefinitions || (undefinitions.Count <= 0) || (null == sd))
                return;

            List<SnapshotNode> modifiedNodes = sd.ModifiedNodes;
            if ((null == modifiedNodes) || (modifiedNodes.Count <= 0))
                return;

            foreach (KeyValuePair<uint, List<string>> undefinition in undefinitions)
            {
                uint nodeId = undefinition.Key;
                SnapshotNode ssn = modifiedNodes.Find((x) => (x.Id == nodeId));
                if (null != ssn)
                    ssn.UndefinedVariables.AddRange(undefinition.Value);
            }
        }

        private void BeautifyCode(List<VisualNode> nodes)
        {
            foreach (VisualNode node in nodes)
            {
                List<string> variableNames = node.GetReferencedVariables(true);
                List<string> definedVars = node.GetDefinedVariables(true);
                if (null != definedVars && (definedVars.Count > 0))
                    variableNames.AddRange(definedVars);

                List<string> tempVariables = new List<string>();
                foreach (string variable in variableNames)
                {
                    if (Utilities.IsTempVariable(variable))
                        tempVariables.Add(variable);
                }

                foreach (string tempVar in tempVariables)
                {
                    RuntimeStates runtimeStates = GetRuntimeStates();
                    string newVarName = runtimeStates.GenerateTempVariable(node.NodeId);
                    node.Text = node.Text.Replace(tempVar, newVarName);
                    string error;
                    (node as CodeBlockNode).ValidateInputString(out error);
                    this.graphProperties.RuntimeStates.AddVariablesDefinedInNode(node, false);
                }
            }
        }

        private void ShowNodesToCodeErrorMessage()
        {
            if (CoreComponent.Instance == null)
                return;
            CoreComponent.Instance.AddFeedbackMessage(ResourceNames.Error, UiStrings.NodesContainError);
        }

        private bool ToggleAllPreviews()
        {
            List<VisualNode> nodeList = nodeCollection.Values.OfType<VisualNode>().ToList();
            if (!Synchronizer.RemoveNodesNotQualifiedForPreview(nodeList))
                return true; // This is not a failure, it just have nothing to alter.

            undoRedoRecorder.BeginGroup();
            // @TODO(Somebody): Use "nodeList" here instead (when Rec... method is updated to take VisualNode.
            undoRedoRecorder.RecordNodeModificationForUndo(nodeList.OfType<IVisualNode>().ToList());
            undoRedoRecorder.EndGroup();

            bool suppress = CoreComponent.Instance.StudioSettings.SuppressPreview;
            foreach (PreviewBubble bubble in bubbleCollection.Values.OfType<PreviewBubble>())
            {
                bubble.Collapse(suppress);
            }

            foreach (VisualNode node in nodeList)
            {
                if (suppress)
                    node.SetNodeState(States.PreviewHidden);
                else
                    node.ClearState(States.PreviewHidden);
            }

            if (nodeList.Count > 0)
                CurrentSynchronizer.BeginQueryNodeValue(nodeList);

            return true;
        }

        #endregion

        #region Private Auto-Saving Related Methods

        private void SaveGraphForRecovery()
        {
            int currentMaxIndex = 0;

            // Step 1: Gather all the pre-existing backup files 
            // that we have stored so far in this same session. 
            // 
            List<string> backupFiles = Utilities.GetBackupFilePaths(
                CoreComponent.Instance.SessionName, this.Identifier);

            if (null != backupFiles && (backupFiles.Count > 0))
            {
                // Step 2: If there are more than a predetermined number of files, 
                // then removed the older ones so that the total number of files is 
                // lower than that threshold.
                // 
                int filesToRemove = backupFiles.Count - Configurations.MaxBackupFilesPerGraph;
                if (filesToRemove > 0) // If there are more files than we want to keep.
                {
                    for (int index = 0; index < filesToRemove; ++index)
                        File.Delete(backupFiles[index]);
                }

                uint dummy = uint.MaxValue; // A dummy graph ID.
                string latestBackupFile = backupFiles[backupFiles.Count - 1];
                Utilities.GetBackupFileIndex(latestBackupFile, ref dummy, ref currentMaxIndex);
            }

            // Step 3: Get the GraphController (this) to save its content to 
            // a file with a new name (with an index higher than the previous 
            // higest index in the series of file).
            // 
            string backupFolder = Utilities.GetBackupFileFolder();
            string newBackupFileName = Utilities.GetBackupFileName(
                CoreComponent.Instance.SessionName,
                this.Identifier, currentMaxIndex + 1);

            string fullPath = backupFolder + newBackupFileName;
            SaveFileInternal(fullPath);
            System.Diagnostics.Debug.WriteLine(string.Format("Backup file stored: {0}", fullPath));
        }

        #endregion

        #region Private Class Event Handlers

        private void OnAutoSaveTimerTicked(object sender, EventArgs e)
        {
            // Make sure we don't use this event handler for anything 
            // other than the "this.autoSaveTimer".
            System.Diagnostics.Debug.Assert(this.autoSaveTimer == sender);

            // Stop the auto-save timer now, it will resume as and when any 
            // graph command (as a result of a user action) comes through.
            // See "PostProcessCommand" method above for detailed description.
            DispatcherTimer timer = sender as DispatcherTimer;
            timer.Stop();

            SaveGraphForRecovery(); // Perform the actual graph saving.
        }

        #endregion

        #region Validation Helper Methods

        private void ValidateDefinedAndReferencedVariables()
        {
            RuntimeStates runtimeStates = this.graphProperties.RuntimeStates;
            List<IVisualNode> nodesRecoveredFromError = new List<IVisualNode>();

            foreach (VisualNode node in this.nodeCollection.Values)
            {
                if (node.VisualType == NodeType.Condensed
                    || node.VisualType == NodeType.Render
                    || node.ErrorType == ErrorType.Syntax)
                    continue;

                //Track current error status of the node
                bool nodeWasErroneous = node.Error;

                //Reset error
                node.SetErrorValue(null, ErrorType.None);

                //Check for semantic errors (i.e. duplicate defined variable) and set it
                string semanticErrorMessage = string.Empty;
                List<string> definedVariables = node.GetDefinedVariables(false);
                bool hasDuplicate = runtimeStates.HasDuplicateVariableDefinition(
                    definedVariables, node.NodeId, out semanticErrorMessage);

                if (hasDuplicate)
                    node.SetErrorValue((object)semanticErrorMessage, ErrorType.Semantic);
                else //Proceed to check for warning only if there is not semantic errors
                {
                    //Check for warnings (i.e. not yet defined referenced variable) and set it
                    string warningErrorMessages = string.Empty;
                    List<string> referencedVariables = node.GetReferencedVariables(false);
                    bool hasNotYetDefined = runtimeStates.HasVariableDefinitionNotYetDefined(referencedVariables, out warningErrorMessages);
                    if (hasNotYetDefined)
                        node.SetErrorValue((object)warningErrorMessages, ErrorType.Warning);
                }

                //Identity node that has recover from error
                if (nodeWasErroneous && (false == node.Error))
                    nodesRecoveredFromError.Add(node);
            }

            // We have discovered at least a node that has recovered from 
            // an erroneous state (it is now no longer having error), go 
            // ahead and schedule a value-query for it.
            if (nodesRecoveredFromError.Count > 0)
                this.ScheduleNodesForValueQuery(nodesRecoveredFromError);
        }

        private void ValidateNodesSyntax(List<IVisualNode> nodeList)
        {
            if (nodeList == null)
                throw new ArgumentNullException("nodeList");

            foreach (VisualNode node in nodeList)
            {
                //Check for syntax error and set it
                string syntaxErrorMessage = string.Empty;
                ErrorType errorType = node.ValidateInputString(out syntaxErrorMessage);
                if (errorType == ErrorType.Syntax)
                    node.SetErrorValue((object)syntaxErrorMessage, ErrorType.Syntax);
                if (errorType == ErrorType.None)
                    node.SetErrorValue((object)syntaxErrorMessage, ErrorType.None);
            }
        }

        #endregion

        #region Implicit Connection Helper Methods

        private void ConnectNodeImplicitly(ImplicitConnectionRequest request)
        {
            string variable = request.variable;
            VisualNode definingNode = request.definingNode;
            VisualNode referencedNode = request.referencedNode;
            List<IVisualNode> modifiedNodes = request.modifiedNodes;
            List<KeyValuePair<uint, uint>> existingImplicitEdges = request.existingImplicitEdges;

            List<uint> outputSlotIds = definingNode.GetSlotIdsByName(SlotType.Output, variable);
            uint outputSlotID = outputSlotIds[outputSlotIds.Count - 1];

            List<uint> inputSlotIds = referencedNode.GetSlotIdsByName(SlotType.Input, variable);
            uint inputSlotID = inputSlotIds[0];

            KeyValuePair<uint, uint> outputInputSlotID = new KeyValuePair<uint, uint>(outputSlotID, inputSlotID);

            if (existingImplicitEdges.Contains(outputInputSlotID))
                existingImplicitEdges.Remove(outputInputSlotID);
            else
            {
                Slot outputSlot = slotCollection[outputSlotID] as Slot;
                uint outputNodeId = outputSlot.Owners[0];
                VisualNode outputNode = GetVisualNode(outputNodeId);

                Slot inputSlot = slotCollection[inputSlotID] as Slot;
                uint inputNodeId = inputSlot.Owners[0];
                VisualNode inputNode = GetVisualNode(inputNodeId);

                List<IVisualNode> nodeList = new List<IVisualNode>();
                nodeList.Add(outputNode);
                nodeList.Add(inputNode);
                undoRedoRecorder.RecordNodeModificationForUndo(nodeList);

                edgeController.CreateImplicitEdge(outputSlot, inputSlot);

                // Mark these nodes as being modified.
                request.modifiedNodes.Add(outputNode);
                request.modifiedNodes.Add(inputNode);
            }
        }

        private void RemoveEdgeList(List<KeyValuePair<uint, uint>> edgeList, List<IVisualNode> modifiedNodes)
        {
            foreach (KeyValuePair<uint, uint> existingEdge in edgeList)
            {
                Slot outputSlot = GetSlot(existingEdge.Key) as Slot;
                uint outputNodeId = outputSlot.Owners[0];
                VisualNode outputNode = GetVisualNode(outputNodeId);

                Slot inputSlot = GetSlot(existingEdge.Value) as Slot;
                uint inputNodeId = inputSlot.Owners[0];
                VisualNode inputNode = GetVisualNode(inputNodeId);

                List<IVisualNode> nodeList = new List<IVisualNode>();
                nodeList.Add(outputNode);
                nodeList.Add(inputNode);
                undoRedoRecorder.RecordNodeModificationForUndo(nodeList);

                edgeController.DeleteImplicitEdge(outputSlot, inputSlot);

                // Mark these nodes as being modified.
                modifiedNodes.Add(outputNode);
                modifiedNodes.Add(inputNode);
            }
        }

        private void EstablishImplicitConnections(List<IVisualNode> modifiedNodes)
        {
            //Get all exsting implicit edges
            List<KeyValuePair<uint, uint>> existingImplicitEdges = edgeController.GetImplicitlyConnectedSlots();

            // This dictionary contains a string-list pair, where the "string" represents a referenced variable 
            // name, and the "list" represents the list of visual nodes that reference the referenced variable.
            Dictionary<string, List<VisualNode>> referencedVarToNodes = new Dictionary<string, List<VisualNode>>();
            foreach (VisualNode node in nodeCollection.Values)
            {
                List<string> referencedVariableList = node.GetReferencedVariables(false);
                if (null == referencedVariableList)
                    continue;

                foreach (string referenceVariable in referencedVariableList)
                {
                    if (!referencedVarToNodes.ContainsKey(referenceVariable))
                        referencedVarToNodes.Add(referenceVariable, new List<VisualNode>());
                    referencedVarToNodes[referenceVariable].Add(node);
                }
            }

            //For each RefVar get the node that the variable define in: Dict < var, node >
            foreach (string referencedVariable in referencedVarToNodes.Keys)
            {
                uint definingNodeId = this.graphProperties.RuntimeStates.GetDefiningNode(referencedVariable);
                if (definingNodeId == uint.MaxValue) // A variable that is not defined anywhere.
                    continue;

                //Connect the nodes together by GC.ConnectNodesImplicitly
                VisualNode definingNode = this.GetVisualNode(definingNodeId);
                if (definingNode.Error) // We don't connect to/from erroneous nodes.
                    continue;

                foreach (VisualNode referencedNode in referencedVarToNodes[referencedVariable])
                {
                    if (referencedNode.Error) // We don't connect to/from erroneous nodes.
                        continue;

                    if (definingNode.NodeId != referencedNode.NodeId)
                    {
                        ImplicitConnectionRequest request = new ImplicitConnectionRequest();
                        request.variable = referencedVariable;
                        request.definingNode = definingNode;
                        request.referencedNode = referencedNode;
                        request.existingImplicitEdges = existingImplicitEdges;
                        request.modifiedNodes = modifiedNodes;
                        this.ConnectNodeImplicitly(request);
                    }
                }
            }

            //Remove edge that no longer exist
            this.RemoveEdgeList(existingImplicitEdges, modifiedNodes);
            edgeController.DeleteUnnecessaryEdges();
        }

        #endregion

        #region HandleSelectedMenuItem Helper Methods

        private void ProcessPreviewMenuItems(int menuItemId, uint bubbleId)
        {
            InfoBubble previewBubble;
            if (this.bubbleCollection.TryGetValue(bubbleId, out previewBubble) == false)
                return;
            VisualNode node = GetVisualNode(previewBubble.OwnerId);
            if (node == null)
                return;

            List<IVisualNode> nodeList = new List<IVisualNode>();
            nodeList.Add(node);

            undoRedoRecorder.BeginGroup();
            undoRedoRecorder.RecordNodeModificationForUndo(nodeList);
            undoRedoRecorder.EndGroup();

            if (Configurations.GeometricOutput == menuItemId)
            {
                node.ClearState(States.TextualPreview);
                node.SetNodeState(States.GeometryPreview);

                CurrentSynchronizer.BeginQueryNodeValue(node.NodeId);
            }
            else if (Configurations.TextualOutput == menuItemId)
            {
                node.ClearState(States.GeometryPreview);
                node.SetNodeState(States.TextualPreview);

                CurrentSynchronizer.BeginQueryNodeValue(node.NodeId);
            }
            else if (Configurations.HidePreview == menuItemId)
            {
                if (node.NodeStates.HasFlag(States.PreviewHidden))
                {
                    node.ClearState(States.PreviewHidden);
                    CurrentSynchronizer.BeginQueryNodeValue(node.NodeId);
                }
                else
                    node.SetNodeState(States.PreviewHidden);

                ((PreviewBubble)previewBubble).Collapse(node.NodeStates.HasFlag(States.PreviewHidden));
            }

            node.Dirty = true;
        }

        private LibraryItem.MemberType GetItemType(int itemId)
        {
            if (itemId >= Configurations.ConstructorBase)
                return LibraryItem.MemberType.Constructor;
            else if (itemId >= Configurations.MethodBase)
                return LibraryItem.MemberType.InstanceMethod;
            else if (itemId >= Configurations.PropertyBase)
                return LibraryItem.MemberType.InstanceProperty;
            else
                return LibraryItem.MemberType.None;
        }

        private FunctionNode CreateMethodNode(double mouseX, double mouseY, string assembly, string qualifiedName, string argumentTypes)
        {
            FunctionNode node = new FunctionNode(this, assembly, qualifiedName, argumentTypes);
            node.X = mouseX - Configurations.NodeCreationOffset;
            node.Y = mouseY - Configurations.NodeCreationOffset;

            if (CoreComponent.Instance.StudioSettings.SuppressPreview)
                node.SetNodeState(States.PreviewHidden);

            return node;
        }

        private PropertyNode CreatePropertyNode(double mouseX, double mouseY, string assembly, string qualifiedName, string argumentTypes)
        {
            PropertyNode node = new PropertyNode(this, assembly, qualifiedName, argumentTypes);
            node.X = mouseX - Configurations.NodeCreationOffset;
            node.Y = mouseY - Configurations.NodeCreationOffset;

            if (CoreComponent.Instance.StudioSettings.SuppressPreview)
                node.SetNodeState(States.PreviewHidden);

            return node;
        }

        private void RecordAndDeleteNode(uint nodeId)
        {
            DeltaNodes deltaNodes = new DeltaNodes();
            deltaNodes.AppendToRemovedNodes(this.GetVisualNode(nodeId));
            deltaNodes.AppendToModifiedNodes(FindAssociatedNodesFromNodes(deltaNodes.RemovedNodes));

            this.undoRedoRecorder.BeginGroup();
            this.undoRedoRecorder.RecordRuntimeStatesForUndo(this.graphProperties.RuntimeStates);
            this.undoRedoRecorder.RecordNodeDeletionForUndo(deltaNodes.RemovedNodes);
            this.undoRedoRecorder.RecordNodeModificationForUndo(deltaNodes.ModifiedNodes);
            this.DeleteNodes(deltaNodes.RemovedNodes);
            this.undoRedoRecorder.EndGroup();

            this.ValidateDefinedAndReferencedVariables();
            this.UpdateSlotsVisiblity();
            this.SynchronizeToLiveRunner(deltaNodes);
        }

        private void AddReplicationGuide(uint nodeId)
        {
            DeltaNodes deltaNodes = new DeltaNodes();
            IVisualNode node = this.GetVisualNode(nodeId);
            deltaNodes.AppendToModifiedNodes(node);

            this.undoRedoRecorder.BeginGroup();
            this.undoRedoRecorder.RecordNodeModificationForUndo(deltaNodes.ModifiedNodes);
            this.undoRedoRecorder.EndGroup();

            FunctionNode fNode = node as FunctionNode;
            fNode.AddReplicationGuides();
            fNode.Compose();

            //After executing AddReplicationGuides(), the node in nodeList would already be modified
            this.SynchronizeToLiveRunner(deltaNodes);
        }

        private void RemoveReplicationGuide(uint nodeId)
        {
            DeltaNodes deltaNodes = new DeltaNodes();
            IVisualNode node = GetVisualNode(nodeId);
            deltaNodes.AppendToModifiedNodes(node);

            this.undoRedoRecorder.BeginGroup();
            this.undoRedoRecorder.RecordNodeModificationForUndo(deltaNodes.ModifiedNodes);
            this.undoRedoRecorder.EndGroup();

            FunctionNode fNode = node as FunctionNode;
            fNode.RemoveReplicationGuides();
            fNode.Compose();

            //After executing RemoveReplicationGuides(), the node in nodeList would already be modified
            this.SynchronizeToLiveRunner(deltaNodes);
        }

        private void HandleTopLeftSelectedItem(int itemId, uint nodeId)
        {
            if (itemId == Configurations.AddReplicationGuides)
                this.AddReplicationGuide(nodeId);
            else if (itemId == Configurations.RemoveReplicationGuides)
                this.RemoveReplicationGuide(nodeId);
            else if (itemId == Configurations.DeleteNode)
                this.RecordAndDeleteNode(nodeId);
        }

        private void TransferInformationFromMethodToProperty(VisualNode sourceNode, VisualNode destinationNode)
        {
            if (sourceNode == null)
                throw new ArgumentNullException("sourceNode");
            if (sourceNode == null)
                throw new ArgumentNullException("destinationNode");

            destinationNode.Text = sourceNode.Text;

            Slot destinationInputSlot = this.GetSlot(destinationNode.GetInputSlot(0)) as Slot;
            Slot sourceInputSlot = this.GetSlot(sourceNode.GetInputSlot(0)) as Slot;
            if (sourceInputSlot.ConnectingSlots != null)
            {
                Slot outputSlot = this.GetSlot(sourceInputSlot.ConnectingSlots[0]) as Slot;
                this.edgeController.CreateLinkingEdge(outputSlot, destinationInputSlot);
            }

            Slot destinationOutputSlot = this.GetSlot(destinationNode.GetOutputSlot(0)) as Slot;
            Slot sourceOutputSlot = this.GetSlot(sourceNode.GetOutputSlot(0)) as Slot;
            if (sourceOutputSlot.ConnectingSlots != null)
            {
                foreach (uint slotId in sourceOutputSlot.ConnectingSlots)
                {
                    Slot inputSlot = this.GetSlot(slotId) as Slot;
                    this.edgeController.CreateLinkingEdge(destinationOutputSlot, inputSlot);
                }
            }

            List<string> defined = sourceNode.GetDefinedVariables(false);
            graphProperties.RuntimeStates.TransferVariableDefinitionMapping(
                defined, sourceNode.NodeId, destinationNode.NodeId);
        }

        #endregion
    }
}
