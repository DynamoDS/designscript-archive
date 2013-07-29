using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using ProtoFFI;
using ProtoCore.AST.AssociativeAST;
using GraphToDSCompiler;

namespace DesignScriptStudio.Graph.Core
{
    public delegate void CanvasModifiedHandler(object sender, EventArgs e);
    public delegate void GraphSavedHandler(object sender, EventArgs e);

    partial class GraphController : IGraphController
    {
        #region IGraphController: Graph Controller Operational Methods

        public void CleanUp()
        {
            if (null != autoSaveTimer)
                autoSaveTimer.Stop();

            // Remove all backup files stored in this session for this closing graph.
            Utilities.ClearBackupFiles(CoreComponent.Instance.SessionName, this.Identifier);
        }

        public bool HitTestComponent(uint compId, Point mouse, out NodePart nodePart, out int index)
        {
            nodePart = NodePart.None;
            index = -1;

            //Implement hittest:DONE
            IVisualNode node = this.GetVisualNode(compId);
            if (null == node)
                return false;

            nodePart = ((VisualNode)node).HitTest(mouse.X, mouse.Y, out index);
            return (nodePart != NodePart.None);
        }

        public void SetMouseCursor(uint nodeId, NodePart nodePart, int index, ModifierKeys modifiers, bool isMouseButtonDown, double x, double y)
        {
            if (0 == nodeId)
            {
                // When the UI calls this method, we are only expecting two possible 
                // value ranges for 'nodeId': a "uint.MaxValue" when the 'nodeId' is 
                // invalid, or a positive number. This should not be zero, if it is,
                // do let me know. - Ben.
                throw new ArgumentException("121B20BE: Invalid argument!", "nodeId");
            }

            if (currentDragState == DragState.NodeRepositioning)
            {
                if (!isMouseButtonDown)
                    return;

                foreach (DraggedNode draggedNode in dragSet)
                {
                    if (visualHost != null)
                    {
                        VisualNode node = (VisualNode)draggedNode.node;
                        DrawingVisual visual = visualHost.GetDrawingVisualForNode(node.NodeId);
                        TranslateTransform transform = visual.Transform as TranslateTransform;

                        transform.X = x - draggedNode.DeltaX;
                        transform.Y = y - draggedNode.DeltaY;

                        node.X = transform.X;
                        node.Y = transform.Y;

                        this.TransformBubble(node, transform);
                    }
                }
                edgeController.UpdateSelectedEdges();

                selectionBox.UpdateSelectionBox(x, y);
            }
            else if ((currentDragState == DragState.CurveDrawing) || (currentDragState == DragState.EdgeReconnection))
            {
                if (nodePart != NodePart.None && nodeId == uint.MaxValue)
                    throw new InvalidOperationException("Nodepard and NodeId are not in consistent (914C54A64BFD)");

                VisualNode node = null;
                edgeConnection = EdgeConnectionFlag.None;

                if (nodePart != NodePart.None)
                {
                    node = GetVisualNode(nodeId);
                }
                else
                {
                    if (lastHoveredNodeId != uint.MaxValue && lastHoveredNodeId != edgeController.GetCurrentlySelectedNodeId())
                    {
                        node = GetVisualNode(lastHoveredNodeId);
                        node.PreviewSelected = node.Selected;//reset the original state of the node
                        RearrangeNodeAndBubbleVisual(node, false);
                    }
                }
                //verification
                List<IVisualNode> nodeToModify;
                edgeConnection = edgeController.AttemptConnectEdge(node, nodePart, index, out nodeToModify);

                if (nodePart != NodePart.None)
                {
                    if ((edgeConnection == EdgeConnectionFlag.NewEdge || edgeConnection == EdgeConnectionFlag.ReconnectEdge)
                        && (nodePart == NodePart.InputSlot || nodePart == NodePart.InputLabel || nodePart == NodePart.ReplicationGuide))
                        node.SetHoveredIndex(index);

                    node.PreviewSelected = true;
                    RearrangeNodeAndBubbleVisual(node, true);
                }

                if (edgeConnection != EdgeConnectionFlag.None && edgeConnection != EdgeConnectionFlag.Illegal)
                    edgeController.DrawConnectingEdgeTo();
                else
                    edgeController.DrawConnectingEdgeTo(new Point(x, y));

                lastHoveredNodeId = nodeId;
                if (null != node)
                    node.Compose(); // Optionally redraw the node.
            }
        }

        public bool UpdateNodeText(uint compId, string text)
        {
            VisualNode node = GetVisualNode(compId);
            if (node == null)
                return false;

            node.Edit(text, true);
            edgeController.UpdateEdgeConnectTo(node);
            return true;
        }

        public bool UpdateNodeTextHighFrequency(string text)
        {
            if (null == nodeSelectedForHighFrequencyUpdate)
                throw new InvalidOperationException("No node selected for high frequency update (CAADF8564FE9)");

            nodeSelectedForHighFrequencyUpdate.Edit(text, true);
            edgeController.UpdateEdgeConnectTo(nodeSelectedForHighFrequencyUpdate);

            // These calls cause the LiveRunner to update.
            DeltaNodes deltaNodes = new DeltaNodes();
            deltaNodes.AppendToModifiedNodes(nodeSelectedForHighFrequencyUpdate);
            SynchronizeToLiveRunner(deltaNodes);
            return true;
        }

        public bool TransientUpdateReplicationText(uint compId, string text, int replicationIndex)
        {
            VisualNode node = GetVisualNode(compId);
            if (node.VisualType != NodeType.Function)
                throw new InvalidOperationException("Replication Guides can only be added to functio nodes");
            else
            {
                FunctionNode fNode = node as FunctionNode;
                fNode.UpdateReplicationGuides(text, replicationIndex);
            }
            edgeController.UpdateEdgeConnectTo(node);
            return true;
        }

        public bool GetNodeText(uint compId, NodePart nodePart, out string text)
        {
            text = string.Empty;
            VisualNode newNode = this.GetVisualNode(compId);
            if (null == newNode)
                return false;

            if (nodePart == NodePart.Caption)
            {
                text = newNode.Caption;
                return true;
            }
            else if (nodePart == NodePart.Text)
            {
                text = newNode.Text;
                return true;
            }

            return false;
        }

        public bool GetLastNodeId(out uint compId)
        {
            uint maxNodeId = uint.MinValue;
            foreach (uint nodeId in nodeCollection.Keys)
            {
                if (nodeId > maxNodeId)
                    maxNodeId = nodeId;
            }
            compId = maxNodeId;
            if (compId == uint.MinValue)
                return false;

            return true;
        }

        public void GetNodeRegion(uint nodeId, bool includeExtended, out Rect region)
        {
            region = GetVisualNode(nodeId).GetRegion(includeExtended);
        }

        public bool GetNodeType(uint nodeId, out string returnType)
        {
            VisualNode node = this.GetVisualNode(nodeId);
            returnType = string.Empty;
            if (node != null)
            {
                returnType = node.ReturnType;
                return true;
            }
            else
                return false;
        }

        public bool GetNodeState(uint nodeId, out States nodeState)
        {
            VisualNode node = this.GetVisualNode(nodeId);
            if (node != null)
            {
                nodeState = node.NodeStates;
                return true;
            }
            else
            {
                nodeState = States.None;
                return false;
            }
        }

        public bool GetNodePartRegion(uint compId, NodePart nodePart, Point mousePt, out Rect result, out NodeType type)
        {
            result = new Rect();
            type = NodeType.None;

            IVisualNode iNode = GetVisualNode(compId);
            VisualNode node = ((VisualNode)iNode);
            if (null == node)
                return false;

            type = node.VisualType;
            double centerLine = 0;
            double width = node.Width;
            double height = node.Height;

            if (nodePart == NodePart.None)
            {
                result.X = node.X;
                result.Y = node.Y;
                result.Width = node.Width;
                result.Height = node.Height;
                return true;
            }

            if (nodePart == NodePart.North || nodePart == NodePart.South)
            {
                centerLine = node.CenterLine;
                result.X = node.X;
                result.Y = node.Y;
                result.Width = centerLine;
                result.Height = node.Height;
            }

            switch (node.VisualType)
            {
                case NodeType.Function:
                    centerLine = node.CenterLine;
                    if (nodePart == NodePart.Caption)
                    {
                        result = new Rect(new Point(node.X + centerLine, node.Y + 0), new Point(node.X + width, node.Y + height / 2));
                        return true;
                    }
                    if (nodePart == NodePart.Text)
                    {
                        result = new Rect(new Point(node.X + centerLine, node.Y + height / 2), new Point(node.X + width, node.Y + height));
                        return true;
                    }
                    if (nodePart == NodePart.ReplicationGuide)
                    {
                        FunctionNode fNode = node as FunctionNode;
                        result = new Rect(new Size(Configurations.ReplicationGuideWidth, Configurations.SlotStripeSize));
                        mousePt.X -= node.X;
                        mousePt.Y -= node.Y;
                        double deltaX = centerLine - mousePt.X;
                        int replicationIndex = (int)(deltaX / (Configurations.ReplicationGuideWidth + Configurations.TriangleHeight));

                        double slotCount = node.GetInputSlots().Count();
                        double slotHeight = (node.Height) / slotCount;
                        int slotIndex = (int)((mousePt.Y) / slotHeight);

                        result.Location = new Point(node.X + centerLine - fNode.GetMaxReplicationWidth(), node.Y + slotIndex * Configurations.SlotStripeSize + 1);
                        return true;
                    }
                    //take inputSlots into consideration
                    break;
                case NodeType.Driver:
                    centerLine = node.CenterLine;
                    if (nodePart == NodePart.Caption)
                    {
                        result = new Rect(new Point(node.X + 0, node.Y + 0), new Point(node.X + centerLine, node.Y + height));
                        return true;
                    }
                    if (nodePart == NodePart.Text)
                    {
                        result = new Rect(new Point(node.X + centerLine, node.Y + 0), new Point(node.X + width, node.Y + height));
                        return true;
                    }
                    break;
                case NodeType.CodeBlock:
                    if (nodePart == NodePart.Text)
                    {
                        result = new Rect(new Point(node.X + 0, node.Y + 0), new Point(node.X + width, node.Y + height));
                        return true;
                    }
                    break;
                case NodeType.Identifier:
                    if (nodePart == NodePart.Caption)
                    {
                        result = new Rect(new Point(node.X + 0, node.Y + 0), new Point(node.X + width, node.Y + height));
                        return true;
                    }
                    break;
                case NodeType.Property:
                    if (nodePart == NodePart.Caption)
                    {
                        result = new Rect(new Point(node.X, node.Y + 0), new Point(node.X + width, node.Y + height / 2));
                        return true;
                    }
                    else if (nodePart == NodePart.Text)
                    {
                        result = new Rect(new Point(node.X, node.Y + height / 2), new Point(node.X + width, node.Y + height));
                        return true;
                    }
                    break;
                default:
                    result = new Rect();
                    return false;
            }
            return false;
        }

        public bool DeterminEdgeSelection(uint edgeId, Point topLeft, Point bottomRight)
        {
            return GetVisualEdge(edgeId).DeterminSelection(topLeft, bottomRight);
        }

        public bool GetMenuItems(out Dictionary<int, string> menuItems, out Dictionary<int, string> overloadedItems)
        {
            menuItems = new Dictionary<int, string>();
            overloadedItems = new Dictionary<int, string>();
            if (itemsProvider != null)
            {
                menuItems = this.itemsProvider.GetMenuItems();
                overloadedItems = this.itemsProvider.GetOverloadedItems();
            }
            return null != menuItems;
        }

        public bool GetTextboxMinSize(uint nodeId, out double minTextboxWidth)
        {
            VisualNode node = GetVisualNode(nodeId);
            minTextboxWidth = 0;
            if (node != null)
            {
                double captionWidth = Utilities.GetTextWidth(node.Caption);
                minTextboxWidth = captionWidth + 5;
                return true;
            }
            return false;
        }

        public bool GetReplicationIndex(uint compId, Point mousePt, out int replicationIndex, out int slotIndex)
        {
            VisualNode node = GetVisualNode(compId);

            if (node.GetInputSlots().Count() > 1)
            {
                mousePt.X -= node.X;
                mousePt.Y -= node.Y;
                double centerLine = node.CenterLine;
                double deltaX = centerLine - mousePt.X;
                replicationIndex = (int)(deltaX / (Configurations.ReplicationGuideWidth + Configurations.TriangleHeight));

                double slotCount = node.GetInputSlots().Count();
                double slotHeight = (node.Height) / slotCount;
                slotIndex = (int)((mousePt.Y) / slotHeight);

                return true;
            }
            else
            {
                replicationIndex = -1;
                slotIndex = -1;
                return false;
            }
        }

        public bool GetReplicationText(uint compId, int index, out string text)
        {
            VisualNode node = GetVisualNode(compId);
            if (node.VisualType != NodeType.Function)
                throw new InvalidOperationException("node must be function Node");
            else
            {
                FunctionNode fNode = node as FunctionNode;
                text = fNode.GetReplicationText(index);
            }
            return true;
        }

        public bool SelectionBoxHittest(Point mousePosition)
        {
            return this.selectionBox.Hittest(mousePosition);
        }

        public bool PreviewBubbleHittest(Point mousePosition, uint bubbleId)
        {
            //Tests whether the top-right dots are hit or not
            InfoBubble previewBubble;
            if (bubbleCollection.TryGetValue(bubbleId, out previewBubble))
                return ((PreviewBubble)previewBubble).Hittest(mousePosition);
            return false;
        }

        public bool GetInfoBubble(uint bubbleId, out IInfoBubble infoBubble)
        {
            infoBubble = null;
            InfoBubble bubble = null;
            if (this.bubbleCollection.TryGetValue(bubbleId, out bubble))
                infoBubble = bubble as IInfoBubble;

            return true;
        }

        public int SelectedComponentsCount()
        {
            int selectedNodeCount = 0;
            foreach (VisualNode node in nodeCollection.Values)
            {
                if (node.Selected)
                    selectedNodeCount++;
            }
            return selectedNodeCount + edgeController.SelectedEdgeCount();
        }

        public void PreviewSelectComponent(uint componentId, ModifierKeys modifiers)
        {
            IVisualNode selectedNode = null;
            nodeCollection.TryGetValue(componentId, out selectedNode);

            if (selectedNode != null)
            {
                VisualNode node = (VisualNode)selectedNode;

                if (modifiers.HasFlag(ModifierKeys.Control))//toggle
                    node.PreviewSelected = !node.PreviewSelected;
                else
                    node.PreviewSelected = true;

                node.Compose(); // Optionally redraw the node.
            }
            else
                edgeController.PreviewSelectComponent(componentId, modifiers);
        }

        public void ClearPreviewSelection(uint componentId)
        {
            IVisualNode selectedNode = null;
            nodeCollection.TryGetValue(componentId, out selectedNode);
            VisualNode node = selectedNode as VisualNode;

            if (node != null)
            {
                node.PreviewSelected = node.Selected;
                node.Compose(); // Optionally redraw the node.
            }
            else
                edgeController.ClearPreviewSelection(componentId);
        }

        public void TransientShowPreviewBubble(uint bubbleId, bool visible)
        {
            InfoBubble InfoBubble;
            PreviewBubble bubble = null;
            bubbleCollection.TryGetValue(bubbleId, out InfoBubble);
            if (InfoBubble != null)
                bubble = (PreviewBubble)InfoBubble;
            if (bubble == null)
                return;

            VisualNode node = GetVisualNode(bubble.OwnerId);

            if (visible)
            {
                bubble.Collapse(false);
                if (node.NodeStates.HasFlag(States.PreviewHidden))
                    CurrentSynchronizer.BeginQueryNodeValue(node.NodeId);
            }
            else
            {
                bubble.Collapse(node.NodeStates.HasFlag(States.PreviewHidden));
            }
        }

        public void TransientExtendPreviewBubble(uint bubbleId, bool extend)
        {
            InfoBubble InfoBubble;
            PreviewBubble bubble = null;
            bubbleCollection.TryGetValue(bubbleId, out InfoBubble);
            if (InfoBubble != null)
                bubble = (PreviewBubble)InfoBubble;
            if (bubble == null || !bubble.Extendable)
                return;

            bubble.Extend(extend);
            if (extend)
            {
                visualHost.ExtendBubble(bubbleId, bubble.Content.ToString(), bubble.GetExtendedBubbleWidth(), bubble.GetExtendedBubbleHeight());
                visualHost.RearrangeDrawingVisual(bubbleId, true, uint.MaxValue);
            }
            else
            {
                double x, y;
                visualHost.GetExtendedBubbleSize(bubbleId, out x, out y);
                bubble.SetExtendedBubbleSize(x, y);
                if (!GetVisualNode(bubble.OwnerId).Selected)
                    visualHost.RearrangeDrawingVisual(bubbleId, false, uint.MaxValue);

                visualHost.RemoveExtendedBubble(bubbleId);
            }
        }

        public bool IsComponentSelected(uint componentId)
        {
            IVisualNode node = this.GetVisualNode(componentId);
            if (node != null)
            {
                return ((VisualNode)node).Selected;
            }
            IVisualEdge edge = this.GetVisualEdge(componentId);
            if (edge != null)
            {
                return ((VisualEdge)edge).Selected;
            }
            return false;
        }

        public bool CheckEdgeType(uint compId)
        {
            if (compId == 0 || compId == uint.MaxValue)
                return false;

            VisualEdge edge = GetVisualEdge(compId);
            return edge.EdgeType == EdgeType.ExplicitConnection;
        }

        public List<string> GetImportedScripts()
        {
            return this.graphProperties.ImportedScripts;
        }

        public bool CanUndo()
        {
            return this.undoRedoRecorder.CanUndo;
        }

        public bool CanRedo()
        {
            return this.undoRedoRecorder.CanRedo;
        }

        public string GetRecordedUiStates()
        {
            return DumpStatesInternal(null);
        }

        public string GetRecordedVmStates()
        {
            return DumpVmStatesInternal();
        }

        public string DumpRecordedUiStates()
        {
            return DumpUiStatesToExternalFile(null);
        }

        public string DumpRecordedVmStates()
        {
            return DumpVmStatesToExternalFile();
        }

        public Point GetNorthEastPositionOfSelectionBox()
        {
            Point position = new Point();
            position.X = selectionBox.x + selectionBox.width;
            position.Y = selectionBox.y;
            return position;
        }

        #endregion

        #region IGraphController: Commands Related Interface Methods

        public bool DoCreateIdentifierNode(double mouseX, double mouseY)
        {
            GraphCommand command = new GraphCommand(GraphCommand.Name.CreateIdentifierNode);
            command.AppendArgument(mouseX);
            command.AppendArgument(mouseY);
            return RerouteToHandler(command);
        }


        public bool DoCreateRenderNode(double mouseX, double mouseY)
        {
            GraphCommand command = new GraphCommand(GraphCommand.Name.CreateRenderNode);
            command.AppendArgument(mouseX);
            command.AppendArgument(mouseY);
            return RerouteToHandler(command);
        }

        public bool DoCreateDriverNode(double mouseX, double mouseY)
        {
            GraphCommand command = new GraphCommand(GraphCommand.Name.CreateDriverNode);
            command.AppendArgument(mouseX);
            command.AppendArgument(mouseY);
            return RerouteToHandler(command);
        }

        public bool DoCreateCodeBlockNode(double mouseX, double mouseY, string content)
        {
            GraphCommand command = new GraphCommand(GraphCommand.Name.CreateCodeBlockNode);
            command.AppendArgument(mouseX);
            command.AppendArgument(mouseY);
            command.AppendArgument(content);
            return RerouteToHandler(command);
        }

        public bool DoCreateFunctionNode(double mouseX, double mouseY, string assembly, string qualifiedName, string argumentTypes)
        {
            if (null == assembly)
                assembly = String.Empty;
            if (null == argumentTypes)
                argumentTypes = String.Empty;
            if (string.IsNullOrEmpty(qualifiedName))
                throw new ArgumentNullException("qualifiedName");

            GraphCommand command = new GraphCommand(GraphCommand.Name.CreateFunctionNode);
            command.AppendArgument(mouseX);
            command.AppendArgument(mouseY);
            command.AppendArgument(assembly);
            command.AppendArgument(qualifiedName);
            command.AppendArgument(argumentTypes);
            return RerouteToHandler(command);
        }

        public bool DoCreatePropertyNode(double mouseX, double mouseY, string assembly, string qualifiedName, string argumentTypes)
        {
            GraphCommand command = new GraphCommand(GraphCommand.Name.CreatePropertyNode);
            command.AppendArgument(mouseX);
            command.AppendArgument(mouseY);
            command.AppendArgument(assembly);
            command.AppendArgument(qualifiedName);
            command.AppendArgument(argumentTypes);
            return RerouteToHandler(command);
        }

        public bool DoSaveGraph(string filePath)
        {
            GraphCommand command = new GraphCommand(GraphCommand.Name.SaveGraph);
            command.AppendArgument(filePath);
            return RerouteToHandler(command);
        }

        public bool DoMouseDown(MouseButton button, uint compId, NodePart nodePart, int index, ModifierKeys modifier)
        {
            GraphCommand command = new GraphCommand(GraphCommand.Name.MouseDown);
            command.AppendArgument(button);
            command.AppendArgument(compId);
            command.AppendArgument(nodePart);
            command.AppendArgument(index);
            command.AppendArgument(modifier);
            return RerouteToHandler(command);
        }

        public bool DoMouseUp(MouseButton button, uint compId, NodePart nodePart, int index, ModifierKeys modifier)
        {
            GraphCommand command = new GraphCommand(GraphCommand.Name.MouseUp);
            command.AppendArgument(button);
            command.AppendArgument(compId);
            command.AppendArgument(nodePart);
            command.AppendArgument(index);
            command.AppendArgument(modifier);
            return RerouteToHandler(command);
        }

        public bool DoBeginDrag(MouseButton button, uint compId, NodePart nodePart, int index, ModifierKeys modifier, double mouseX, double mouseY)
        {
            GraphCommand command = new GraphCommand(GraphCommand.Name.BeginDrag);
            command.AppendArgument(button);
            command.AppendArgument(compId);
            command.AppendArgument(nodePart);
            command.AppendArgument(index);
            command.AppendArgument(modifier);
            command.AppendArgument(mouseX);
            command.AppendArgument(mouseY);
            return RerouteToHandler(command);
        }

        public bool DoEndDrag(MouseButton button, uint compId, NodePart nodePart, int index, ModifierKeys modifier, double mouseX, double mouseY)
        {
            GraphCommand command = new GraphCommand(GraphCommand.Name.EndDrag);
            command.AppendArgument(button);
            command.AppendArgument(compId);
            command.AppendArgument(nodePart);
            command.AppendArgument(index);
            command.AppendArgument(modifier);
            command.AppendArgument(mouseX);
            command.AppendArgument(mouseY);
            return RerouteToHandler(command);
        }


        public bool DoBeginNodeEdit(uint compId, NodePart nodePart)
        {
            GraphCommand command = new GraphCommand(GraphCommand.Name.BeginNodeEdit);
            command.AppendArgument(compId);
            command.AppendArgument(nodePart);
            return RerouteToHandler(command);
        }

        public bool DoEndNodeEdit(uint compId, string text, bool updateFlag)
        {
            GraphCommand command = new GraphCommand(GraphCommand.Name.EndNodeEdit);
            command.AppendArgument(compId);
            command.AppendArgument(text);
            command.AppendArgument(updateFlag);
            return RerouteToHandler(command);
        }

        public bool DoBeginHighFrequencyUpdate(uint nodeId, NodePart nodePart)
        {
            GraphCommand command = new GraphCommand(GraphCommand.Name.BeginHighFrequencyUpdate);
            command.AppendArgument(nodeId);
            command.AppendArgument(nodePart);
            return RerouteToHandler(command);
        }

        public bool DoEndHighFrequencyUpdate(string text)
        {
            GraphCommand command = new GraphCommand(GraphCommand.Name.EndHighFrequencyUpdate);
            command.AppendArgument(text);
            return RerouteToHandler(command);
        }

        public bool DoSelectComponent(uint compId, ModifierKeys modifiers)
        {
            GraphCommand command = new GraphCommand(GraphCommand.Name.SelectComponent);
            command.AppendArgument(compId);
            command.AppendArgument(modifiers);
            return RerouteToHandler(command);
        }

        public bool DoClearSelection()
        {
            GraphCommand command = new GraphCommand(GraphCommand.Name.ClearSelection);
            return RerouteToHandler(command);
        }

        public bool DoUndoOperation()
        {
            GraphCommand command = new GraphCommand(GraphCommand.Name.UndoOperation);
            return RerouteToHandler(command);
        }

        public bool DoRedoOperation()
        {
            GraphCommand command = new GraphCommand(GraphCommand.Name.RedoOperation);
            return RerouteToHandler(command);
        }

        public bool DoDeleteComponents()
        {
            GraphCommand command = new GraphCommand(GraphCommand.Name.DeleteComponents);
            return RerouteToHandler(command);
        }

        public bool DoAddReplicationGuide(uint nodeId)
        {
            GraphCommand command = new GraphCommand(GraphCommand.Name.AddReplicationGuide);
            command.AppendArgument(nodeId);
            return RerouteToHandler(command);
        }

        public bool DoRemoveReplicationGuide(uint nodeId)
        {
            GraphCommand command = new GraphCommand(GraphCommand.Name.RemoveReplicationGuide);
            command.AppendArgument(nodeId);
            return RerouteToHandler(command);
        }

        public bool DoEditReplicationGuide(uint compId, int replicationIndex)
        {
            GraphCommand command = new GraphCommand(GraphCommand.Name.EditReplicationGuide);
            command.AppendArgument(compId);
            command.AppendArgument(replicationIndex);
            return RerouteToHandler(command);
        }

        public bool DoSetReplicationGuideText(uint nodeId, int replicationIndex, string text)
        {
            GraphCommand command = new GraphCommand(GraphCommand.Name.SetReplicationGuide);
            command.AppendArgument(nodeId);
            command.AppendArgument(replicationIndex);
            command.AppendArgument(text);
            return RerouteToHandler(command);
        }

        public bool DoSelectMenuItem(int menuItemId, double x, double y, uint nodeId, NodePart nodePart)
        {
            GraphCommand command = new GraphCommand(GraphCommand.Name.SelectMenuItem);
            command.AppendArgument(menuItemId);
            command.AppendArgument(x);
            command.AppendArgument(y);
            command.AppendArgument(nodeId);
            command.AppendArgument(nodePart);
            return RerouteToHandler(command);
        }

        public bool DoCreateRadialMenu(NodePart nodePart, uint nodeId)
        {
            GraphCommand command = new GraphCommand(GraphCommand.Name.CreateRadialMenu);
            command.AppendArgument(nodePart);
            command.AppendArgument(nodeId);
            return RerouteToHandler(command);
        }

        public bool DoCreateSubRadialMenu(int selectedItemId)
        {
            GraphCommand command = new GraphCommand(GraphCommand.Name.CreateSubRadialMenu);
            command.AppendArgument(selectedItemId);
            return RerouteToHandler(command);
        }

        public bool DoImportScript(string scriptPath)
        {
            GraphCommand command = new GraphCommand(GraphCommand.Name.ImportScript);
            command.AppendArgument(scriptPath);
            return RerouteToHandler(command);
        }

        public bool DoConvertSelectionToCode()
        {
            GraphCommand command = new GraphCommand(GraphCommand.Name.ConvertSelectionToCode);
            return RerouteToHandler(command);
        }

        public bool DoTogglePreview(uint bubbleId)
        {
            GraphCommand command = new GraphCommand(GraphCommand.Name.TogglePreview);
            command.AppendArgument(bubbleId);
            return RerouteToHandler(command);
        }

        #endregion

        #region IGraphController: Public Interface Events

        public event CanvasModifiedHandler Modified = null;

        public event GraphSavedHandler Saved = null;

        #endregion

        #region IGraphController: Interface Properties

        public uint Identifier { get; private set; }
        public DragState CurrentDragState { get { return currentDragState; } }
        public EdgeConnectionFlag EdgeConnection { get { return edgeConnection; } }
        public string FilePath { get { return this.filePath; } }

        public bool IsModified
        {
            get
            {
                return this.graphProperties.RuntimeStates.IsModified;
            }

            set
            {
                if (this.graphProperties.RuntimeStates.IsModified == value)
                    return;

                this.graphProperties.RuntimeStates.IsModified = value;
                if (null != Modified)
                    Modified(this, new EventArgs());
            }
        }

        #endregion
    }
}
