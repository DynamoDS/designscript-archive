using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;

namespace DesignScriptStudio.Graph.Core
{
    class EdgeController
    {
        private GraphController graphController = null;

        private Dictionary<uint, VisualEdge> edgeCollection = null;
        List<uint> selectedSlots = null;

        private uint outputSlotId = 0;
        private uint inputSlotId = 0;

        private VisualEdge connectingEdge;

        //for edge selection
        private VisualEdge selectedEdge = null;

        // draw the temporary curve from the active slot when trying to create new edge
        private uint currentlySelectedNodeId = uint.MaxValue;
        private uint currentlySelectedSlotId = uint.MaxValue;
        private SlotType currentlySelectedSlotType = SlotType.None;

        // draw the temporary curve to 
        private uint connectNodeId = uint.MaxValue;
        private uint connectSlotId = uint.MaxValue;
        private SlotType connectSlotType = SlotType.None;

        //for edge reconnection
        private List<uint> reconnectingEdges;
        private List<uint> reconnectingSlots;
        private uint lastConnectedSlotId;
        private uint reconnectEdgeId = uint.MaxValue;

        // for edge replace
        private VisualEdge edgeToBeReplaced;

        #region Public/Internal Methods

        internal EdgeController(GraphController graphController)
        {
            this.graphController = graphController;
            edgeCollection = new Dictionary<uint, VisualEdge>();
            this.selectedSlots = new List<uint>();
            connectingEdge = new VisualEdge(this, EdgeType.ExplicitConnection);
        }

        #region for Automation

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            if (null == edgeCollection || (edgeCollection.Count <= 0))
                builder.Append("No edge exists in the system\n");
            else
            {
                foreach (var edge in edgeCollection)
                {
                    VisualEdge visualEdge = edge.Value;
                    builder.Append(string.Format("Edge 0x{0}: 0x{1} --> 0x{2} (Selected: {3})\n",
                        visualEdge.EdgeId.ToString("x8"), visualEdge.StartSlotId.ToString("x8"),
                        visualEdge.EndSlotId.ToString("x8"), visualEdge.Selected));
                }
            }

            return builder.ToString();
        }
        #endregion

        #region for Edge Operarion(connection, deletion)

        internal bool isImplicitEdge(uint edgeId)
        {
            return EdgeType.ImplicitConnection == this.edgeCollection[edgeId].EdgeType;
        }

        internal GraphController GetGraphController()
        {
            return this.graphController;
        }

        internal EdgeConnectionFlag AttemptConnectEdge(VisualNode node, NodePart nodePart, int index, out List<IVisualNode> nodeToModify)
        {
            nodeToModify = new List<IVisualNode>();

            connectNodeId = uint.MaxValue;
            connectSlotId = uint.MaxValue;
            connectSlotType = SlotType.None;
            SlotType startSlotType = SlotType.None;

            if (node != null)
            {
                connectNodeId = node.NodeId;

                if (graphController.CurrentDragState == DragState.CurveDrawing)
                {
                    if (currentlySelectedNodeId == connectNodeId)
                        return EdgeConnectionFlag.None;

                    startSlotType = currentlySelectedSlotType;
                }
                else
                {
                    foreach (uint slotId in reconnectingSlots)   //try to connect any of the reconnecting node
                    {
                        if (connectNodeId == graphController.GetSlot(slotId).Owners.Last())
                            return EdgeConnectionFlag.None;
                    }

                    if (graphController.GetSlot(lastConnectedSlotId).SlotType == SlotType.Output)
                        startSlotType = SlotType.Input;
                    else
                        startSlotType = SlotType.Output;
                }

                if (nodePart == NodePart.InputSlot || nodePart == NodePart.InputLabel || nodePart == NodePart.ReplicationGuide)
                {
                    connectSlotType = SlotType.Input;
                    connectSlotId = node.GetInputSlot(index);
                }
                else if (nodePart == NodePart.OutputSlot || (nodePart == NodePart.Text && node.VisualType != NodeType.CodeBlock)) // user can only connect to the output slot of the CBN
                {
                    connectSlotType = SlotType.Output;
                    connectSlotId = node.GetOutputSlot(index);
                }
                else if (node.VisualType == NodeType.Identifier && nodePart == NodePart.Caption)
                {
                    if (startSlotType == SlotType.Output)
                    {
                        connectSlotType = SlotType.Input;
                        connectSlotId = node.GetInputSlot(0);
                    }
                    else if (startSlotType == SlotType.Input)
                    {
                        connectSlotType = SlotType.Output;
                        connectSlotId = node.GetOutputSlot(0);
                    }
                }
            }

            if (graphController.CurrentDragState == DragState.CurveDrawing)
            {
                if (connectSlotId == 0 || connectSlotId == uint.MaxValue)
                    return EdgeConnectionFlag.None;
                if (currentlySelectedSlotType == connectSlotType)
                    return EdgeConnectionFlag.Illegal;

                // Check if these is a edge between the two slots already
                if (startSlotType == SlotType.Input)
                {
                    this.inputSlotId = currentlySelectedSlotId;
                    this.outputSlotId = connectSlotId;
                }
                else
                {
                    this.inputSlotId = connectSlotId;
                    this.outputSlotId = currentlySelectedSlotId;
                }

                var entry = edgeCollection.FirstOrDefault(x => IsAdded(x.Value));
                if ((edgeCollection.Count() != 0) && (entry.Value != null))
                    return EdgeConnectionFlag.None;

                nodeToModify.Add(graphController.GetVisualNode(currentlySelectedNodeId));
                nodeToModify.Add(node);

                //check if it is a edge replace(the input slot is connect to other node already)
                if (FindEdgeToBeReplaced(out edgeToBeReplaced))
                {
                    nodeToModify.Add(graphController.GetVisualNode
                                            (graphController.GetSlot(edgeToBeReplaced.StartSlotId).Owners.Last()));
                    nodeToModify.Add(graphController.GetVisualNode
                                            (graphController.GetSlot(edgeToBeReplaced.EndSlotId).Owners.Last()));
                }
                return EdgeConnectionFlag.NewEdge;
            }
            else
            {
                nodeToModify.Add(graphController.GetVisualNode
                                            (graphController.GetSlot(lastConnectedSlotId).Owners.Last()));
                foreach (uint slotId in reconnectingSlots)
                {
                    nodeToModify.Add(graphController.GetVisualNode
                                        (graphController.GetSlot(slotId).Owners.Last()));
                }

                if (connectSlotId == 0 || connectSlotId == uint.MaxValue)
                    return EdgeConnectionFlag.None;
                if (connectSlotType != graphController.GetSlot(lastConnectedSlotId).SlotType)
                    return EdgeConnectionFlag.Illegal;

                if (lastConnectedSlotId == connectSlotId) // node modification is needed only when the it reconnect to different slot
                {
                    nodeToModify = new List<IVisualNode>();
                }
                else
                {
                    nodeToModify.Add(node);

                    //check if it is a edge replace(the input slot is connect to other node already)
                    if (FindEdgeToBeReplaced(reconnectingSlots[0], out edgeToBeReplaced))
                    {
                        if (!reconnectingEdges.Contains(edgeToBeReplaced.EdgeId))
                        {
                            nodeToModify.Add(graphController.GetVisualNode
                                                    (graphController.GetSlot(edgeToBeReplaced.StartSlotId).Owners.Last()));
                            nodeToModify.Add(graphController.GetVisualNode
                                                    (graphController.GetSlot(edgeToBeReplaced.EndSlotId).Owners.Last()));
                        }
                    }
                }

                return EdgeConnectionFlag.ReconnectEdge;
            }
        }

        private bool FindEdgeToBeReplaced(uint startSlotId, out VisualEdge edgeToBeReplaced)
        {
            bool findEdgeToBeReplaced;
            edgeToBeReplaced = null;
            if (connectSlotType == SlotType.Input)
            {
                this.inputSlotId = startSlotId;
                this.outputSlotId = connectSlotId;
            }
            else
            {
                this.inputSlotId = connectSlotId;
                this.inputSlotId = startSlotId;
            }

            findEdgeToBeReplaced = FindEdgeToBeReplaced(out edgeToBeReplaced);
            return findEdgeToBeReplaced;
        }

        private bool FindEdgeToBeReplaced(out VisualEdge edgeToBeReplaced)
        {
            edgeToBeReplaced = null;

            List<uint> slotsConnectToInputSlot = new List<uint>();
            ISlot inputSlot = graphController.GetSlot(this.inputSlotId);

            if (inputSlot != null && inputSlot.ConnectingSlots != null)
                slotsConnectToInputSlot.AddRange(inputSlot.ConnectingSlots);
            if (slotsConnectToInputSlot.Count() > 0)
            {
                this.outputSlotId = inputSlot.ConnectingSlots[0];
                var pair = edgeCollection.FirstOrDefault(x => IsAdded(x.Value));
                edgeToBeReplaced = pair.Value;
            }

            return (edgeToBeReplaced != null);
        }

        internal void DrawConnectingEdgeTo()
        {
            IGraphVisualHost visualHost = graphController.GetVisualHost();
            if (null == visualHost)
                return;
            DrawingVisual visual = visualHost.GetDrawingVisualForEdge(0);
            DrawingContext context = visual.RenderOpen();

            if (graphController.CurrentDragState == DragState.CurveDrawing)
            {
                DrawSingleConnectingEdgeTo(currentlySelectedSlotId, connectSlotId, context);
            }
            else
            {
                foreach (uint reconnectingSlotId in reconnectingSlots)
                {
                    DrawSingleConnectingEdgeTo(reconnectingSlotId, connectSlotId, context);
                }
            }

            context.Close();
        }

        private void DrawSingleConnectingEdgeTo(uint startSlotId, uint endSlotId, DrawingContext context)
        {
            Slot startSlot = graphController.GetSlot(startSlotId) as Slot;
            Slot endSlot = graphController.GetSlot(endSlotId) as Slot;

            Point startPoint = startSlot.GetPosition();
            Point endPoint = endSlot.GetPosition();

            VisualEdge connectingEdge = new VisualEdge(this, EdgeType.ExplicitConnection);
            connectingEdge.ComposeConnectingEdge(context, startPoint, endPoint, (startSlot.SlotType != SlotType.Output));
        }

        internal void DrawConnectingEdgeTo(Point endPoint)
        {
            IGraphVisualHost visualHost = graphController.GetVisualHost();
            if (null == visualHost)
                return;

            DrawingVisual visual = visualHost.GetDrawingVisualForEdge(0);
            DrawingContext context = visual.RenderOpen();

            if (graphController.CurrentDragState == DragState.CurveDrawing)
            {
                DrawSingleConnectingEdgeTo(currentlySelectedSlotId, endPoint, context);
            }
            else
            {
                foreach (uint reconnectingSlotId in reconnectingSlots)
                {
                    DrawSingleConnectingEdgeTo(reconnectingSlotId, endPoint, context);
                }
            }

            context.Close();
        }

        private void DrawSingleConnectingEdgeTo(uint startSlotId, Point endPoint, DrawingContext context)
        {
            Slot startSlot = graphController.GetSlot(startSlotId) as Slot;
            Point startPoint = startSlot.GetPosition();

            VisualEdge connectingEdge = new VisualEdge(this, EdgeType.ExplicitConnection);
            connectingEdge.ComposeConnectingEdge(context, startPoint, endPoint, (startSlot.SlotType != SlotType.Output));
        }

        internal void CreateEdge()
        {
            //reset the nodes
            VisualNode node;
            if (graphController.CurrentDragState == DragState.CurveDrawing)
            {
                node = graphController.GetVisualNode(currentlySelectedNodeId);
                if (null != node)
                    node.Selected = false;
            }
            else
            {
                foreach (uint slotId in reconnectingSlots)
                {
                    node = graphController.GetVisualNode(graphController.GetSlot(slotId).Owners.Last());
                    if (null != node)
                        node.Selected = false;
                }
            }

            if (graphController.edgeConnection != EdgeConnectionFlag.None && graphController.edgeConnection != EdgeConnectionFlag.Illegal)
            {
                //find respective node and slot
                VisualNode inputNode, outputNode;
                uint inputSlotId, outputSlotId;
                List<VisualNode> reconnectingNodes = new List<VisualNode>();

                if (connectSlotType == SlotType.Output)
                {
                    inputNode = graphController.GetVisualNode(currentlySelectedNodeId);
                    inputSlotId = currentlySelectedSlotId;
                    outputNode = graphController.GetVisualNode(connectNodeId);
                    outputSlotId = connectSlotId;
                }
                else
                {
                    outputNode = graphController.GetVisualNode(currentlySelectedNodeId);
                    outputSlotId = currentlySelectedSlotId;
                    inputNode = graphController.GetVisualNode(connectNodeId);
                    inputSlotId = connectSlotId;
                }

                if (graphController.CurrentDragState == DragState.EdgeReconnection)
                {
                    foreach (uint slotId in reconnectingSlots)
                    {
                        reconnectingNodes.Add(graphController.GetVisualNode(
                            ((Slot)graphController.GetSlot(slotId)).Owners.Last()));
                    }
                }


                if (graphController.CurrentDragState == DragState.CurveDrawing)
                {
                    AddEdge(outputSlotId, inputSlotId);
                }
                else
                {
                    if (graphController.GetSlot(lastConnectedSlotId).SlotType == SlotType.Input)
                    {
                        for (int i = 0; i < reconnectingEdges.Count(); i++)
                        {
                            reconnectEdgeId = reconnectingEdges[i];
                            ReconnectEdge(reconnectingEdges[i],
                                            reconnectingSlots[i], inputSlotId);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < reconnectingEdges.Count(); i++)
                        {
                            reconnectEdgeId = reconnectingEdges[i];
                            ReconnectEdge(reconnectingEdges[i],
                                            outputSlotId, reconnectingSlots[i]);
                        }
                    }
                }
            }
            else if (graphController.CurrentDragState == DragState.EdgeReconnection) // try to reconnect edge but drop to canvas
            {
                foreach (uint edgeId in reconnectingEdges)
                    DeleteEdge((VisualEdge)GetVisualEdge(edgeId));
            }

            //clear the foreground drawing
            this.ClearEdgeVisual(0);
            this.reconnectingEdges = new List<uint>();
            this.reconnectEdgeId = uint.MaxValue;
        }

        internal VisualEdge CreateImplicitEdge(Slot outputSlot, Slot inputSlot)
        {
            this.outputSlotId = outputSlot.SlotId;
            this.inputSlotId = inputSlot.SlotId;
            var entry = edgeCollection.FirstOrDefault(x => IsAdded(x.Value));
            if ((edgeCollection.Count() != 0) && (entry.Value != null))
                return null;

            IdGenerator idGenerator = ((GraphController)graphController).GetIdGenerator();
            VisualEdge visualEdge = new VisualEdge(this, outputSlot.SlotId, inputSlot.SlotId, true);
            Slot.Connect(inputSlot, outputSlot);
            edgeCollection.Add(visualEdge.EdgeId, visualEdge);
            visualEdge.Dirty = true;
            visualEdge.Compose();

            //record
            UndoRedoRecorder urr = graphController.GetUndoRedoRecorder();
            List<IVisualEdge> edgesCreated = new List<IVisualEdge>();
            edgesCreated.Add(visualEdge);
            urr.RecordEdgeCreationForUndo(edgesCreated);

            return visualEdge;
        }

        internal VisualEdge CreateLinkingEdge(Slot outputSlot, Slot inputSlot)
        {
            this.outputSlotId = outputSlot.SlotId;
            this.inputSlotId = inputSlot.SlotId;
            var entry = edgeCollection.FirstOrDefault(x => IsAdded(x.Value));
            if ((edgeCollection.Count() != 0) && (entry.Value != null))
                return null;

            IdGenerator idGenerator = ((GraphController)graphController).GetIdGenerator();
            VisualEdge visualEdge = new VisualEdge(this, outputSlot.SlotId, inputSlot.SlotId, false);
            Slot.Connect(outputSlot, inputSlot);
            edgeCollection.Add(visualEdge.EdgeId, visualEdge);
            visualEdge.Dirty = true;
            visualEdge.Compose();

            //record
            UndoRedoRecorder urr = graphController.GetUndoRedoRecorder();
            List<IVisualEdge> edgesCreated = new List<IVisualEdge>();
            edgesCreated.Add(visualEdge);
            urr.RecordEdgeCreationForUndo(edgesCreated);

            return visualEdge;
        }

        #region Helpers

        //for edge drawing
        internal void SetCurrentlySelectedNodeId(uint nodeId)
        {
            this.currentlySelectedNodeId = nodeId;
        }

        internal void SetCurrentlySelectedSlotType(SlotType slotType)
        {
            this.currentlySelectedSlotType = slotType;
        }

        internal void SetCurrentlySelectedSlotId(uint slotId)
        {
            this.currentlySelectedSlotId = slotId;
        }

        internal uint GetCurrentlySelectedNodeId()
        {
            return this.currentlySelectedNodeId;
        }

        internal SlotType GetCurrentlySelectedSlotType()
        {
            return this.currentlySelectedSlotType;
        }

        internal uint GetConnectNodeId()
        {
            return this.connectNodeId;
        }

        internal void SetConnectNodeId(uint nodeId)
        {
            this.connectNodeId = nodeId;
        }

        internal void SetConnectSlotType(SlotType slotType)
        {
            this.connectSlotType = slotType;
        }

        internal void SetConnectSlotId(uint slotId)
        {
            this.connectSlotId = slotId;
        }

        internal void SetConnectedSlotId(uint slotId)
        {
            this.lastConnectedSlotId = slotId;
        }

        internal void SetReconnectingEdges(List<uint> edges)
        {
            this.reconnectingEdges = edges;
        }

        internal void SetReconnectingSlots(List<uint> slots)
        {
            this.reconnectingSlots = slots;
        }

        internal List<VisualEdge> FindEdgesConnectingToSlot(uint slotId, bool onlySelected)
        {
            List<VisualEdge> edges = new List<VisualEdge>();
            ISlot slot = graphController.GetSlot(slotId);

            if (slot.SlotType == SlotType.Input)
            {
                foreach (VisualEdge visualEdge in edgeCollection.Values)
                {
                    if (visualEdge.EndSlotId == slotId)
                    {
                        if ((false == onlySelected) || visualEdge.Selected)
                            edges.Add(visualEdge);
                    }
                }
            }
            else
            {
                foreach (VisualEdge visualEdge in edgeCollection.Values)
                {
                    if (visualEdge.StartSlotId == slotId)
                    {
                        if ((false == onlySelected) || visualEdge.Selected)
                            edges.Add(visualEdge);
                    }
                }
            }

            return edges;
        }

        #endregion

        #endregion

        #region Edge Updating for Node Operation

        internal void BeginUpdateEdge()
        {
            UndoRedoRecorder urr = graphController.GetUndoRedoRecorder();
            List<IVisualEdge> edgeToModify = new List<IVisualEdge>();

            selectedSlots.Clear();
            foreach (VisualEdge visualEdge in edgeCollection.Values)
            {
                if (visualEdge.Selected)
                {
                    edgeToModify.Add(visualEdge);
                    this.ClearEdgeVisual(visualEdge.EdgeId);
                }
            }
            if (edgeToModify.Count > 0)
                urr.RecordEdgeModificationForUndo(edgeToModify);
        }

        internal void EndUpdateEdge()
        {
            this.ClearEdgeVisual(0);

            foreach (VisualEdge visualEdge in edgeCollection.Values)
            {
                if (visualEdge.Selected)
                {
                    visualEdge.Dirty = true;
                }
            }

            UpdateDirtyEdges(null);
            selectedSlots.Clear();
        }

        // @TODO(Victor): Let's discuss about performance improvement on this one.
        internal void UpdateSelectedEdges()
        {
            IGraphVisualHost visualHost = graphController.GetVisualHost();

            if (null == visualHost)
                return;

            DrawingVisual visual = visualHost.GetDrawingVisualForEdge(0);
            DrawingContext context = visual.RenderOpen();

            foreach (VisualEdge visualEdge in edgeCollection.Values)
            {
                if (visualEdge.Selected)
                    visualEdge.ComposeOnDrawing(context);
            }

            context.Close();
        }

        internal void UpdateEdgeConnectTo(VisualNode node)
        {
            uint[] inputs = node.GetInputSlots();
            uint[] outputs = node.GetOutputSlots();

            if (inputs != null)
            {
                foreach (uint slotId in inputs)
                {
                    foreach (VisualEdge visualEdge in edgeCollection.Values)
                        if (visualEdge.EndSlotId == slotId)
                            visualEdge.UpdateControlPoint();
                }
            }
            if (outputs != null)
            {
                foreach (uint slotId in outputs)
                {
                    foreach (VisualEdge visualEdge in edgeCollection.Values)
                        if (visualEdge.StartSlotId == slotId)
                            visualEdge.UpdateControlPoint();
                }
            }

            UpdateDirtyEdges(null);
        }

        internal void ResetAllEdges()
        {
            foreach (VisualEdge visualEdge in edgeCollection.Values)
            {
                if (visualEdge.Selected || visualEdge.PreviewSelected)
                {
                    visualEdge.Selected = false;
                    visualEdge.Compose();
                }
            }
            this.ClearEdgeVisual(0);
            selectedSlots.Clear();
        }

        internal void ResetParameters()
        {
            currentlySelectedNodeId = uint.MaxValue;
            currentlySelectedSlotId = uint.MaxValue;
            currentlySelectedSlotType = SlotType.None;

            connectNodeId = uint.MaxValue;
            connectSlotId = uint.MaxValue;
            connectSlotType = SlotType.None;

            reconnectingEdges = new List<uint>();
            reconnectingSlots = new List<uint>();
            lastConnectedSlotId = uint.MaxValue;
            reconnectEdgeId = uint.MaxValue;

            connectingEdge = null;
        }

        #endregion

        #region Edge Selection

        internal bool SelectComponent(uint componentId, ModifierKeys modifiers)
        {
            selectedEdge = null;
            edgeCollection.TryGetValue(componentId, out selectedEdge);

            if (selectedEdge != null)
            {
                if (modifiers.HasFlag(ModifierKeys.Control))
                    selectedEdge.Selected = !selectedEdge.Selected;
                else
                    selectedEdge.Selected = true;

                selectedEdge.Compose();
            }

            return true;
        }

        internal void PreviewSelectComponent(uint componentId, ModifierKeys modifiers)
        {
            selectedEdge = null;
            edgeCollection.TryGetValue(componentId, out selectedEdge);

            if (selectedEdge != null)
            {
                if (modifiers.HasFlag(ModifierKeys.Control))
                    selectedEdge.PreviewSelected = !selectedEdge.Selected;
                else
                    selectedEdge.PreviewSelected = true;

                selectedEdge.Compose();
            }
        }

        internal void ClearPreviewSelection(uint componentId)
        {
            selectedEdge = null;
            edgeCollection.TryGetValue(componentId, out selectedEdge);

            if (selectedEdge != null)
            {
                selectedEdge.PreviewSelected = selectedEdge.Selected;
                selectedEdge.Compose();
            }
        }

        #endregion

        #region GraphController Surpporting Methods

        internal void AuditEdges()
        {
            foreach (VisualEdge edge in edgeCollection.Values)
            {
                if (edge.Audit() == AuditStatus.PersistentDataChanged)
                    graphController.IsModified = true;
            }
        }

        internal List<KeyValuePair<uint, uint>> GetImplicitlyConnectedSlots()
        {
            List<KeyValuePair<uint, uint>> outputInputSlotsList = new List<KeyValuePair<uint, uint>>();
            foreach (VisualEdge edge in edgeCollection.Values)
            {
                if (edge.EdgeType == EdgeType.ImplicitConnection)
                {
                    KeyValuePair<uint, uint> outputInputID = new KeyValuePair<uint, uint>(edge.StartSlotId, edge.EndSlotId);
                    outputInputSlotsList.Add(outputInputID);
                }
            }
            return outputInputSlotsList;
        }

        internal uint GetEdgeId(uint startSlotId, uint endSlotId)
        {
            foreach (VisualEdge edge in edgeCollection.Values)
            {
                if (startSlotId == edge.StartSlotId && endSlotId == edge.EndSlotId)
                    return edge.EdgeId;
            }
            return uint.MaxValue;
        }

        internal int SelectedEdgeCount()
        {
            int selectedEdgeCount = 0;
            foreach (VisualEdge visualEdge in edgeCollection.Values)
            {
                if (visualEdge.Selected)
                    selectedEdgeCount++;
            }
            return selectedEdgeCount;
        }

        internal uint GetMaxEdgeId()
        {
            return this.edgeCollection.Keys.Max();
        }

        internal List<IVisualEdge> GetVisualEdges()
        {
            return this.edgeCollection.Values.ToList<IVisualEdge>();
        }

        internal void UpdateDirtyEdges(List<IVisualNode> modifiedNodes)
        {
            if (modifiedNodes != null)
            {
                List<VisualEdge> modifiedEdges = GetRelatedEdgesFromNodes(modifiedNodes);
                foreach (VisualEdge edge in modifiedEdges)
                    edge.UpdateControlPoint();
            }

            foreach (VisualEdge edge in edgeCollection.Values.ToList())
            {
                if (edge.Dirty)
                {
                    edge.Compose();
                    edge.Dirty = false;
                }
            }
        }

        internal void DeleteSelectedEdges()
        {
            List<VisualEdge> edgesToDelete = new List<VisualEdge>();

            foreach (VisualEdge visualEdge in edgeCollection.Values)
            {
                if (visualEdge.Selected)
                    edgesToDelete.Add(visualEdge);
            }

            foreach (VisualEdge edgeToDelete in edgesToDelete)
                DeleteEdge(edgeToDelete);

            ResetAllEdges();
        }

        internal void DeleteEdgesConnectTo(uint slotId)
        {
            List<VisualEdge> removedEdges = new List<VisualEdge>();

            if (((Slot)graphController.GetSlot(slotId)).SlotType == SlotType.Input)
            {
                foreach (VisualEdge visualEdge in edgeCollection.Values)
                    if (visualEdge.EndSlotId == slotId)
                        removedEdges.Add(visualEdge);
            }
            else
            {
                foreach (VisualEdge visualEdge in edgeCollection.Values)
                    if (visualEdge.StartSlotId == slotId)
                        removedEdges.Add(visualEdge);
            }
            foreach (VisualEdge edgeToDelete in removedEdges)
                DeleteEdge(edgeToDelete);
        }

        internal void DeleteEdgesConnectTo(IVisualNode visualNode)
        {
            uint[] inputs = visualNode.GetInputSlots();
            uint[] outputs = visualNode.GetOutputSlots();

            List<VisualEdge> edgesToRemove = new List<VisualEdge>();

            if (inputs != null)
            {
                foreach (uint slotId in inputs)
                {
                    foreach (VisualEdge visualEdge in edgeCollection.Values)
                    {
                        if (visualEdge.EndSlotId == slotId || visualEdge.StartSlotId == slotId)
                            edgesToRemove.Add(visualEdge);
                    }
                }
            }
            if (outputs != null)
            {
                foreach (uint slotId in outputs)
                {
                    foreach (VisualEdge visualEdge in edgeCollection.Values)
                    {
                        if (visualEdge.EndSlotId == slotId || visualEdge.StartSlotId == slotId)
                            edgesToRemove.Add(visualEdge);
                    }
                }
            }

            foreach (VisualEdge edgeToDelete in edgesToRemove)
                DeleteEdge(edgeToDelete);
        }

        internal void DeleteUnnecessaryEdges()
        {
            if (edgeCollection == null || edgeCollection.Count == 0)
                return;

            List<IVisualEdge> unnecessaryEdges = new List<IVisualEdge>();

            foreach (VisualEdge edge in this.edgeCollection.Values)
            {
                Slot endSlot = this.graphController.GetSlot(edge.EndSlotId) as Slot;
                for (int i = 0; i < endSlot.ConnectingSlots.Length; i++)
                {
                    if (edge.StartSlotId == endSlot.ConnectingSlots[i] && i != endSlot.ConnectingSlots.Length - 1)
                        unnecessaryEdges.Add(edge);
                }

                Slot startSlot = this.graphController.GetSlot(edge.StartSlotId) as Slot;
                if (edge.EdgeType == EdgeType.ExplicitConnection && (!startSlot.Visible || !endSlot.Visible))
                    unnecessaryEdges.Add(edge);
            }

            unnecessaryEdges = unnecessaryEdges.Distinct().ToList();

            if (unnecessaryEdges.Count > 0)
            {
                UndoRedoRecorder urr = this.graphController.GetUndoRedoRecorder();
                urr.RecordEdgeDeletionForUndo(unnecessaryEdges);
            }

            foreach (VisualEdge edge in unnecessaryEdges)
            {
                IGraphVisualHost visualHost = this.graphController.GetVisualHost();
                if (visualHost != null)
                    visualHost.RemoveDrawingVisual(edge.EdgeId);

                Slot startSlot = this.graphController.GetSlot(edge.StartSlotId) as Slot;
                Slot endSlot = this.graphController.GetSlot(edge.EndSlotId) as Slot;
                Slot.Disconnect(startSlot, endSlot);
                edgeCollection.Remove(edge.EdgeId);
            }
        }

        internal List<IVisualNode> GetNodesFromSelectedEdges()
        {
            List<IVisualNode> connectedNodes = new List<IVisualNode>();

            foreach (VisualEdge visualEdge in edgeCollection.Values)
            {
                if (visualEdge.Selected)
                {
                    ISlot slot = graphController.GetSlot(visualEdge.StartSlotId);
                    IVisualNode visualNode = graphController.GetVisualNode(slot.Owners.Last());

                    if (!connectedNodes.Contains(visualNode))
                        connectedNodes.Add(visualNode);

                    slot = graphController.GetSlot(visualEdge.EndSlotId);
                    visualNode = graphController.GetVisualNode(slot.Owners.Last());

                    if (!connectedNodes.Contains(visualNode))
                        connectedNodes.Add(visualNode);
                }
            }

            return connectedNodes;
        }

        internal IVisualEdge GetVisualEdge(uint edgeId)
        {
            if (null != edgeCollection)
            {
                VisualEdge visualEdge = null;
                if (edgeCollection.TryGetValue(edgeId, out visualEdge))
                    return visualEdge;
            }
            return null;
        }

        internal void SelectEdges(IVisualNode activatedNode)
        {
            uint[] inputs = activatedNode.GetInputSlots();
            uint[] outputs = activatedNode.GetOutputSlots();

            if (null != inputs)
                selectedSlots.AddRange(inputs);
            if (null != outputs)
                selectedSlots.AddRange(outputs);

            foreach (uint slotId in selectedSlots)
            {
                foreach (VisualEdge visualEdge in edgeCollection.Values)
                {
                    if (visualEdge.Selected)
                        continue;

                    if ((visualEdge.StartSlotId == slotId) || (visualEdge.EndSlotId == slotId))
                    {
                        visualEdge.Selected = true;
                        visualEdge.Compose();
                    }
                }
            }
        }

        internal void DeleteImplicitEdge(Slot outputSlot, Slot inputSlot)
        {
            this.inputSlotId = inputSlot.SlotId;
            this.outputSlotId = outputSlot.SlotId;

            var entry = edgeCollection.FirstOrDefault(x => IsAdded(x.Value));

            UndoRedoRecorder urr = graphController.GetUndoRedoRecorder();
            List<IVisualEdge> edgesToDelete = new List<IVisualEdge>();
            edgesToDelete.Add(entry.Value);
            urr.RecordEdgeDeletionForUndo(edgesToDelete);
            DeleteVisualEdge(entry.Value);
            Slot.Disconnect(outputSlot, inputSlot);
        }

        internal List<IVisualEdge> FindEdgeConnectTo(VisualNode node)
        {
            List<IVisualEdge> edgeList = new List<IVisualEdge>();

            uint[] inputs = node.GetInputSlots();
            uint[] outputs = node.GetOutputSlots();

            if (inputs != null)
            {
                foreach (uint slotId in inputs)
                {
                    foreach (VisualEdge visualEdge in edgeCollection.Values)
                        if (visualEdge.EndSlotId == slotId)
                            edgeList.Add(visualEdge);
                }
            }
            if (outputs != null)
            {
                foreach (uint slotId in outputs)
                {
                    foreach (VisualEdge visualEdge in edgeCollection.Values)
                        if (visualEdge.StartSlotId == slotId)
                            edgeList.Add(visualEdge);
                }
            }

            return edgeList;
        }
        #endregion

        #region UndoRedo Support

        internal void AddEdge(VisualEdge edge)
        {
            edgeCollection.Add(edge.EdgeId, edge);
        }

        internal void RemoveEdge(uint edgeId)
        {
            edgeCollection.Remove(edgeId);
        }

        //for undo/redo, reload
        internal void BuildEdges(List<ISlot> slots)
        {
            List<VisualEdge> recoverEdge = new List<VisualEdge>();

            foreach (ISlot slot in slots)
            {
                Slot currentSlot = (Slot)slot;
                uint currentNodeId = currentSlot.Owners[currentSlot.Owners.Length - 1];
                VisualNode currentNode = graphController.GetVisualNode(currentNodeId);
                int currentSlotIndex = currentNode.GetSlotIndex(currentSlot.SlotId);

                if (currentSlot.ConnectingSlots != null)
                {
                    foreach (uint connectingSlotId in currentSlot.ConnectingSlots)
                    {
                        VisualEdge newEdge = null;
                        Slot connectingSlot = (Slot)graphController.GetSlot(connectingSlotId);

                        uint connectingNodeId = connectingSlot.Owners[connectingSlot.Owners.Length - 1];
                        VisualNode connectingNode = graphController.GetVisualNode(connectingNodeId);
                        int connectingSlotIndex = connectingNode.GetSlotIndex(connectingSlotId);

                        if (currentSlot.GetSlotName() != connectingSlot.GetSlotName()) //normal recover edge
                        {
                            if (currentSlot.SlotType == SlotType.Output)
                                newEdge = RebuildEdge(currentNodeId, currentSlot.SlotId, currentSlotIndex,
                                    connectingNodeId, connectingSlotId, connectingSlotIndex);
                            else
                                newEdge = RebuildEdge(connectingNodeId, connectingSlotId, connectingSlotIndex,
                                    currentNodeId, currentSlot.SlotId, currentSlotIndex);
                        }
                        else // rebuild implicit connection
                        {
                            if (currentSlot.SlotType == SlotType.Output)
                                newEdge = RebuildImplicitEdge(currentSlot, connectingSlot);
                            else
                                newEdge = RebuildImplicitEdge(connectingSlot, currentSlot);
                        }
                        if (newEdge != null)
                            recoverEdge.Add(newEdge);
                    }
                }
            }
            List<VisualEdge> edgesToDelete = edgeCollection.Values.Except(recoverEdge).ToList();
            foreach (VisualEdge edgeToDelete in edgesToDelete)
                this.DeleteVisualEdge(edgeToDelete);
        }

        internal void ClearEdgeVisual(uint edgeId)
        {
            IGraphVisualHost visualHost = graphController.GetVisualHost();

            if (null == visualHost)
                return;

            DrawingVisual visual = visualHost.GetDrawingVisualForEdge(edgeId);
            DrawingContext context = visual.RenderOpen();
            context.Close();
        }

        #endregion

        #endregion

        #region Private Methods

        private List<VisualEdge> GetRelatedEdgesFromNodes(List<IVisualNode> nodes)
        {
            List<VisualEdge> modifiedEdges = new List<VisualEdge>();

            foreach (IVisualNode node in nodes)
            {
                uint[] inputSlotIds = node.GetInputSlots();
                if (inputSlotIds == null)
                    continue;
                foreach (uint inputSlotId in inputSlotIds)
                {
                    ISlot inputSlot = this.graphController.GetSlot(inputSlotId);
                    if (inputSlot.ConnectingSlots == null)
                        continue;
                    foreach (uint outputSlotId in inputSlot.ConnectingSlots)
                    {
                        uint modifiedEdgeId = GetEdgeId(outputSlotId, inputSlotId);
                        VisualEdge modifiedEdge = GetVisualEdge(modifiedEdgeId) as VisualEdge;
                        modifiedEdges.Add(modifiedEdge);
                    }
                }
            }

            foreach (IVisualNode node in nodes)
            {
                uint[] outputSlotIds = node.GetOutputSlots();
                if (outputSlotIds == null)
                    continue;
                foreach (uint outputSlotId in outputSlotIds)
                {
                    ISlot outputSlot = this.graphController.GetSlot(outputSlotId);
                    if (outputSlot.ConnectingSlots == null)
                        continue;
                    foreach (uint inputSlotId in outputSlot.ConnectingSlots)
                    {
                        uint modifiedEdgeId = GetEdgeId(outputSlotId, inputSlotId);
                        VisualEdge modifiedEdge = GetVisualEdge(modifiedEdgeId) as VisualEdge;
                        modifiedEdges.Add(modifiedEdge);
                    }
                }
            }
            return modifiedEdges;
        }

        private VisualEdge AddEdge(uint outputSlotId, uint inputSlotId)
        {
            UndoRedoRecorder urr = graphController.GetUndoRedoRecorder();

            //for edge replace
            this.inputSlotId = inputSlotId;
            this.outputSlotId = outputSlotId;
            if (FindEdgeToBeReplaced(out edgeToBeReplaced))
            {
                DeleteEdge(edgeToBeReplaced);
            }

            //add edge
            VisualEdge visualEdge = null;

            IdGenerator idGenerator = ((GraphController)graphController).GetIdGenerator();
            visualEdge = new VisualEdge(this, outputSlotId, inputSlotId, false);

            edgeCollection.Add(visualEdge.EdgeId, visualEdge);

            //maintain connection
            Slot outputSlot = (Slot)graphController.GetSlot(outputSlotId);
            Slot inputSlot = (Slot)graphController.GetSlot(inputSlotId);
            Slot.Connect(outputSlot, inputSlot);

            //addVisual
            visualEdge.Dirty = true;
            visualEdge.Compose();

            //record
            List<IVisualEdge> edgesCreated = new List<IVisualEdge>();
            edgesCreated.Add(visualEdge);
            urr.RecordEdgeCreationForUndo(edgesCreated);

            // Assignment Swapping
            VisualNode outputNode = graphController.GetVisualNode(graphController.GetSlot(outputSlotId).Owners.Last());
            outputNode.HandleNewConnection(outputSlotId);
            VisualNode inputNode = graphController.GetVisualNode(graphController.GetSlot(inputSlotId).Owners.Last());
            inputNode.HandleNewConnection(inputSlotId);

            return visualEdge;
        }

        private VisualEdge ReconnectEdge(uint edgeId, uint outputSlotId, uint inputSlotId)
        {
            this.inputSlotId = inputSlotId;
            this.outputSlotId = outputSlotId;

            //for edge replace
            if (FindEdgeToBeReplaced(out edgeToBeReplaced))
            {
                if (!reconnectingEdges.Contains(edgeToBeReplaced.EdgeId))
                    DeleteEdge(edgeToBeReplaced);
            }

            //add edge
            VisualEdge visualEdge = null;
            edgeCollection.TryGetValue(edgeId, out visualEdge);

            UndoRedoRecorder urr = graphController.GetUndoRedoRecorder();
            List<IVisualEdge> edgesToModify = new List<IVisualEdge>();
            edgesToModify.Add(visualEdge);
            urr.RecordEdgeModificationForUndo(edgesToModify);

            visualEdge.ReconnectSlots(outputSlotId, inputSlotId);

            //maintain connection
            Slot lastConnectedSlot = (Slot)graphController.GetSlot(lastConnectedSlotId);
            Slot outputSlot = (Slot)graphController.GetSlot(outputSlotId);
            Slot inputSlot = (Slot)graphController.GetSlot(inputSlotId);
            // Remove the old connection
            if (lastConnectedSlot.SlotType == SlotType.Output)
                Slot.Disconnect(lastConnectedSlot, inputSlot);
            else
                Slot.Disconnect(outputSlot, lastConnectedSlot);
            // Build the new connection
            Slot.Connect(outputSlot, inputSlot);

            //addVisual
            visualEdge.UpdateControlPoint();
            visualEdge.Compose();

            // Assignment Swapping
            VisualNode lastConnectedNode = graphController.GetVisualNode(graphController.GetSlot(lastConnectedSlotId).Owners.Last());
            lastConnectedNode.HandleRemoveConnection(lastConnectedSlotId);
            if (lastConnectedSlot.SlotType == SlotType.Output)
            {
                VisualNode newConnectedNode = graphController.GetVisualNode(graphController.GetSlot(outputSlotId).Owners.Last());
                newConnectedNode.HandleNewConnection(outputSlotId);
            }
            else
            {
                VisualNode newConnectedNode = graphController.GetVisualNode(graphController.GetSlot(inputSlotId).Owners.Last());
                newConnectedNode.HandleNewConnection(inputSlotId);
            }

            return visualEdge;
        }

        private void DeleteEdge(VisualEdge edgeToDelete)
        {
            //Assignment Swapping
            if (edgeToDelete.EdgeType == EdgeType.ExplicitConnection)
            {
                VisualNode outputNode = graphController.GetVisualNode(graphController.GetSlot(edgeToDelete.StartSlotId).Owners.Last());
                outputNode.HandleRemoveConnection(edgeToDelete.StartSlotId);
                VisualNode intputNode = graphController.GetVisualNode(graphController.GetSlot(edgeToDelete.EndSlotId).Owners.Last());
                intputNode.HandleRemoveConnection(edgeToDelete.EndSlotId);
            }

            UndoRedoRecorder urr = graphController.GetUndoRedoRecorder();
            List<IVisualEdge> edgesToDelete = new List<IVisualEdge>();
            edgesToDelete.Add(edgeToDelete);
            urr.RecordEdgeDeletionForUndo(edgesToDelete);

            IGraphVisualHost visualHost = graphController.GetVisualHost();
            if (null != visualHost)
                visualHost.RemoveDrawingVisual(edgeToDelete.EdgeId);

            Slot.Disconnect((Slot)graphController.GetSlot(edgeToDelete.StartSlotId), (Slot)graphController.GetSlot(edgeToDelete.EndSlotId));
            edgeCollection.Remove(edgeToDelete.EdgeId);
        }

        private VisualEdge RebuildEdge(uint outputNodeId, uint outputSlotId, int outputSlotIndex, uint inputNodeId, uint inputSlotId, int inputSlotIndex)
        {
            this.inputSlotId = inputSlotId;
            var entry = new KeyValuePair<uint, VisualEdge>();

            List<uint> connectingSlots = new List<uint>();
            if (graphController.GetSlot(inputSlotId).ConnectingSlots != null)
                connectingSlots.AddRange(graphController.GetSlot(inputSlotId).ConnectingSlots);

            this.outputSlotId = outputSlotId;

            entry = edgeCollection.FirstOrDefault(x => IsAdded(x.Value));
            if ((edgeCollection.Count() == 0) || (entry.Value == null))//need to recover edge and drawing
            {
                IdGenerator idGenerator = ((GraphController)graphController).GetIdGenerator();
                VisualEdge visualEdge = new VisualEdge(this, outputSlotId, inputSlotId, false);

                edgeCollection.Add(visualEdge.EdgeId, visualEdge);

                //addVisual
                visualEdge.Dirty = true;
                visualEdge.Compose();
                return visualEdge;
            }

            entry.Value.Dirty = true;
            entry.Value.Compose();
            return entry.Value;
        }

        private VisualEdge RebuildImplicitEdge(Slot outputSlot, Slot inputSlot)
        {
            this.outputSlotId = outputSlot.SlotId;
            this.inputSlotId = inputSlot.SlotId;
            var entry = edgeCollection.FirstOrDefault(x => IsAdded(x.Value));
            if ((edgeCollection.Count() == 0) || (entry.Value == null))
            {
                IdGenerator idGenerator = ((GraphController)graphController).GetIdGenerator();
                VisualEdge visualEdge = new VisualEdge(this, outputSlot.SlotId, inputSlot.SlotId, true);
                edgeCollection.Add(visualEdge.EdgeId, visualEdge);
                visualEdge.Dirty = true;
                visualEdge.Compose();
                return visualEdge;
            }

            entry.Value.Dirty = true;
            entry.Value.Compose();
            return entry.Value;
        }

        // undo/redo helpers
        private void DeleteVisualEdge(VisualEdge edgToDelete)//just remove the edge but no operation on slots
        {
            IGraphVisualHost visualHost = graphController.GetVisualHost();
            if (null != visualHost)
                visualHost.RemoveDrawingVisual(edgToDelete.EdgeId);

            edgeCollection.Remove(edgToDelete.EdgeId);
        }

        // undo/redo helpers
        private List<IVisualNode> GetNodesConnectTo(uint inputSlot)
        {
            List<IVisualNode> connectedNodes = new List<IVisualNode>();

            foreach (VisualEdge visualEdge in edgeCollection.Values)
            {
                if (visualEdge.EndSlotId == inputSlot)
                {
                    ISlot slot = graphController.GetSlot(visualEdge.StartSlotId);
                    IVisualNode visualNode = graphController.GetVisualNode(slot.Owners.Last());
                    connectedNodes.Add(visualNode);
                }
            }

            return connectedNodes;
        }

        private bool IsAdded(VisualEdge visualEdge)
        {
            return (visualEdge.StartSlotId == outputSlotId) && (visualEdge.EndSlotId == inputSlotId);
        }

        private bool IsSelected(VisualEdge visualEdge)
        {
            return (visualEdge == selectedEdge);
        }

        #endregion

    }
}