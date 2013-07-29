using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using DesignScriptStudio.Graph.Core.VMServices;
using GraphToDSCompiler;

namespace DesignScriptStudio.Graph.Core
{
    partial class GraphController : IGraphController
    {
        #region Private Class Command Handlers

        private bool HandleCreateCodeBlockNode(GraphCommand command)
        {
            double mouseX = ((double)command.GetArgument(0));
            double mouseY = ((double)command.GetArgument(1));
            string content = command.GetArgument(2) as string;

            CodeBlockNode node = new CodeBlockNode(this, content);
            object parameter = null;

            int intResult = 0;
            bool boolResult = false;
            double dblResult = 0;
            if (Int32.TryParse(content, out intResult))
                parameter = intResult;
            else if (Double.TryParse(content, out dblResult))
                parameter = dblResult;
            else if (Boolean.TryParse(content, out boolResult))
                parameter = boolResult;
            else
                parameter = content; // String type.

            this.CreateNodeInternal(node, mouseX, mouseY);
            return true;
        }

        private bool HandleCreateDriverNode(GraphCommand command)
        {
            double mouseX = ((double)command.GetArgument(0));
            double mouseY = ((double)command.GetArgument(1));

            DriverNode node = new DriverNode(this, Configurations.DriverInitialTextValue);
            this.CreateNodeInternal(node, mouseX, mouseY);
            return true;
        }

        private bool HandleCreateFunctionNode(GraphCommand command)
        {
            double mouseX = ((double)command.GetArgument(0));
            double mouseY = ((double)command.GetArgument(1));
            string assembly = command.GetArgument(2) as string;
            string qualifiedName = command.GetArgument(3) as string;
            string argumentTypes = command.GetArgument(4) as string;

            FunctionNode node = new FunctionNode(this, assembly, qualifiedName, argumentTypes);
            this.CreateNodeInternal(node, mouseX, mouseY);
            return true;
        }

        private bool HandleCreateIdentifierNode(GraphCommand command)
        {
            double mouseX = ((double)command.GetArgument(0));
            double mouseY = ((double)command.GetArgument(1));

            IdentifierNode node = new IdentifierNode(this, "Identifier");
            this.CreateNodeInternal(node, mouseX, mouseY);
            return true;
        }

        private bool HandleCreateRenderNode(GraphCommand command)
        {
            double mouseX = ((double)command.GetArgument(0));
            double mouseY = ((double)command.GetArgument(1));

            RenderNode node = new RenderNode(this, 0);
            this.CreateNodeInternal(node, mouseX, mouseY);
            return true;
        }

        private bool HandleCreatePropertyNode(GraphCommand command)
        {
            double mouseX = ((double)command.GetArgument(0));
            double mouseY = ((double)command.GetArgument(1));
            string assembly = command.GetArgument(2) as string;
            string qualifiedName = command.GetArgument(3) as string;
            string argumentTypes = command.GetArgument(4) as string;

            PropertyNode node = new PropertyNode(this, assembly, qualifiedName, argumentTypes);
            this.CreateNodeInternal(node, mouseX, mouseY);
            return true;
        }

        private bool HandleSaveGraph(GraphCommand command)
        {
            string filePath = command.GetArgument(0) as string;
            this.filePath = filePath;
            this.UpdateImportedLibrary();
            this.SaveFileInternal(filePath);
            this.FireFilePathSavedNotification();

            return true;
        }

        private void FireFilePathSavedNotification()
        {
            if (null != this.Saved)
                Saved(this, new EventArgs());
        }

        private bool HandleMouseDown(GraphCommand command)
        {
            MouseButton button = ((MouseButton)command.GetArgument(0));
            uint compId = ((uint)command.GetArgument(1));
            NodePart nodePart = ((NodePart)command.GetArgument(2));
            int slotIndex = ((int)command.GetArgument(3));
            ModifierKeys modifiers = ((ModifierKeys)command.GetArgument(4));

            if (nodePart != NodePart.None)
            {
                if (IdGenerator.GetType(compId) != ComponentType.Node)
                    throw new InvalidOperationException("Component type mismatch!");
                IVisualNode node = null;
                nodeCollection.TryGetValue(compId, out node);
                if (null == node)
                    throw new ArgumentException("Invalid argument!", "compId");
                VisualNode visualNode = node as VisualNode;

                switch (nodePart)
                {
                    case NodePart.North:
                    case NodePart.South:
                    case NodePart.NorthEast:
                    case NodePart.NorthWest:
                    case NodePart.PreviewNorthEast:
                    case NodePart.Preview:
                        ClearSelectionInternal();
                        RearrangeNodeAndBubbleVisual(visualNode, true);
                        visualNode.Selected = true;
                        break;

                    case NodePart.Caption:
                    case NodePart.Text:
                    case NodePart.ReplicationGuide:
                    case NodePart.InputLabel:
                        if (visualNode.Selected)
                        {
                            if (modifiers.HasFlag(ModifierKeys.Control)) //remove the selected node and update the edge
                                visualNode.Selected = false;
                        }
                        else
                        {
                            if (!modifiers.HasFlag(ModifierKeys.Control) && !modifiers.HasFlag(ModifierKeys.Shift))
                                ClearSelectionInternal();
                            RearrangeNodeAndBubbleVisual(visualNode, true);
                            visualNode.Selected = true;
                        }
                        break;

                    case NodePart.InputSlot:
                    case NodePart.OutputSlot:
                        break;
                }

                if (null != visualNode)
                    visualNode.Compose(); // Redraw for selection change.
            }

            selectionBox.UpdateSelectionBox(GetSelectedNodes());
            return true;
        }

        private bool HandleMouseUp(GraphCommand command)
        {
            MouseButton button = ((MouseButton)command.GetArgument(0));
            uint compId = ((uint)command.GetArgument(1));
            NodePart nodePart = ((NodePart)command.GetArgument(2));
            int slotIndex = ((int)command.GetArgument(3));
            ModifierKeys modifiers = ((ModifierKeys)command.GetArgument(4));

            if (currentDragState == DragState.None)
            {
                IVisualNode node = null;
                VisualNode visNode = null;
                nodeCollection.TryGetValue(compId, out node);
                if (null != node)
                    visNode = node as VisualNode;

                switch (nodePart)
                {
                    case NodePart.InputSlot:
                    case NodePart.OutputSlot:
                    case NodePart.NorthWest:
                    case NodePart.South:
                    case NodePart.Caption:
                    case NodePart.Text:
                    case NodePart.North:
                    case NodePart.ReplicationGuide:
                    case NodePart.InputLabel:
                        break;
                    case NodePart.NorthEast:
                        visNode.ToggleContextualMenu(nodePart);
                        break;
                    case NodePart.Preview:
                        //visNode.TogglePreview();
                        break;
                    case NodePart.PreviewNorthEast:
                        //visNode.ToggleContextualMenu(nodePart);
                        break;
                    default:
                        if (!modifiers.HasFlag(ModifierKeys.Control) && !modifiers.HasFlag(ModifierKeys.Shift))
                            ClearSelectionInternal();
                        break;
                }

                if (visualHost != null && visNode != null && visNode.Selected)
                    visualHost.ScrollToVisual(visualHost.GetDrawingVisualForNode(visNode.NodeId));
            }
            else if (currentDragState != DragState.CurveDrawing && currentDragState != DragState.EdgeReconnection) // for click and drag
                currentDragState = DragState.None;                                                               // it won't end when mouseup on the same slot
            // IDE-1371
            selectionBox.UpdateSelectionBox(GetSelectedNodes());
            return true;
        }

        private bool HandleBeginDrag(GraphCommand command)
        {
            MouseButton button = ((MouseButton)command.GetArgument(0));
            uint compId = ((uint)command.GetArgument(1));
            NodePart nodePart = ((NodePart)command.GetArgument(2));
            int slotIndex = ((int)command.GetArgument(3));
            ModifierKeys modifiers = ((ModifierKeys)command.GetArgument(4));
            double mouseX = ((double)command.GetArgument(5));
            double mouseY = ((double)command.GetArgument(6));

            switch (nodePart)
            {
                case NodePart.None:
                    BeginDragSelectionRegion(modifiers);
                    return true;

                case NodePart.InputSlot:
                case NodePart.OutputSlot:
                    BeginDragInputOutputSlot(compId, nodePart, slotIndex);
                    return true;

                case NodePart.Caption:
                case NodePart.Text:
                case NodePart.ReplicationGuide:
                case NodePart.InputLabel:
                case NodePart.North:
                case NodePart.NorthEast:
                case NodePart.NorthWest:
                case NodePart.South:
                case NodePart.Preview:
                case NodePart.PreviewNorthEast:
                    BeginDragSelectedNodes(mouseX, mouseY, this.GetVisualNode(compId));
                    return true;
            }
            return false;
        }

        private bool HandleEndDrag(GraphCommand command)
        {
            MouseButton button = ((MouseButton)command.GetArgument(0));
            uint compId = ((uint)command.GetArgument(1));
            NodePart nodePart = ((NodePart)command.GetArgument(2));
            int slotIndex = ((int)command.GetArgument(3));
            ModifierKeys modifiers = ((ModifierKeys)command.GetArgument(4));
            double mouseX = ((double)command.GetArgument(5));
            double mouseY = ((double)command.GetArgument(6));

            switch (currentDragState)
            {
                case DragState.NodeRepositioning:
                    foreach (DraggedNode draggedNode in dragSet)
                    {
                        ((VisualNode)draggedNode.node).X = mouseX - draggedNode.DeltaX;
                        ((VisualNode)draggedNode.node).Y = mouseY - draggedNode.DeltaY;
                    }

                    // both node that the edge is connected to should be bring to back
                    // 
                    foreach (VisualNode connectingNode in edgeController.GetNodesFromSelectedEdges())
                    {
                        if (!connectingNode.Selected)
                            RearrangeNodeAndBubbleVisual(connectingNode, false);
                    }

                    dragSet.Clear();
                    edgeController.ResetAllEdges();
                    break;

                case DragState.CurveDrawing:
                case DragState.EdgeReconnection:
                    edgeConnection = EdgeConnectionFlag.None;

                    VisualNode node = GetVisualNode(edgeController.GetConnectNodeId());
                    if (null != node)
                    {
                        node.PreviewSelected = node.Selected;
                        node.Compose(); // Optionally redraw the node.
                    }
                    node = GetVisualNode(edgeController.GetCurrentlySelectedNodeId());
                    if (null != node)
                    {
                        node.Selected = false;
                        node.Compose(); // reset highlighted node
                    }

                    node = GetVisualNode(compId);
                    List<IVisualNode> modifiedNodes = null;
                    edgeConnection = edgeController.AttemptConnectEdge(node, nodePart, slotIndex, out modifiedNodes);

                    DeltaNodes deltaNodes = new DeltaNodes();
                    deltaNodes.AppendToModifiedNodes(modifiedNodes.Distinct().ToList());

                    //reset highlighted node
                    foreach (IVisualNode visualNode in deltaNodes.ModifiedNodes)
                    {
                        ((VisualNode)visualNode).Selected = false;
                    }

                    // Start tracking variables being undefined in this process.
                    {
                        RuntimeStates runtimeStates = graphProperties.RuntimeStates;
                        runtimeStates.BeginDefinitionMonitor();

                        undoRedoRecorder.BeginGroup();
                        undoRedoRecorder.RecordRuntimeStatesForUndo(runtimeStates);
                        undoRedoRecorder.RecordNodeModificationForUndo(deltaNodes.ModifiedNodes);
                        edgeController.CreateEdge();

                        modifiedNodes.Clear();
                        this.EstablishImplicitConnections(modifiedNodes);
                        undoRedoRecorder.EndGroup();

                        Dictionary<uint, List<string>> undefinedVariables = null;
                        runtimeStates.EndDefinitionMonitor(out undefinedVariables);
                        deltaNodes.AppendUndefinedVariables(undefinedVariables);
                    }

                    deltaNodes.AppendToModifiedNodes(modifiedNodes);
                    if (deltaNodes.ModifiedNodes.Count > 0)  //modification have been done
                        SynchronizeToLiveRunner(deltaNodes);
                    else
                        undoRedoRecorder.PopRecordFromUndoStorage();

                    edgeController.ResetParameters();
                    edgeController.ResetAllEdges();
                    currentDragState = DragState.None;
                    edgeConnection = EdgeConnectionFlag.None;
                    break;

                case DragState.RegionSelection:
                    if (visualHost != null)
                        visualHost.EndDragSelection();
                    break;
            }
            //currentDragState = DragState.None;
            selectionBox.UpdateSelectionBox(GetSelectedNodes());
            return true;
        }

        private bool HandleBeginNodeEdit(GraphCommand command)
        {
            uint nodeId = (uint)command.GetArgument(0);
            NodePart nodePart = ((NodePart)command.GetArgument(1));
            VisualNode node = this.GetVisualNode(nodeId);
            if (nodePart == NodePart.Text)
                this.previousText = node.Text;
            else if (nodePart == NodePart.Caption)
                this.previousText = node.Caption;
            node.EnableEdit(nodePart, -1);
            return true;
        }

        private bool HandleEndNodeEdit(GraphCommand command)
        {
            uint nodeId = (uint)command.GetArgument(0);
            string text = command.GetArgument(1) as string;
            bool commit = (bool)command.GetArgument(2);

            VisualNode node = this.GetVisualNode(nodeId);
            bool setToPreviousValue = false;
            bool setToDefaultValue = false;
            bool recordAsCreation = false;
            bool recordAndDelete = false;
            bool validate = false;
            bool delete = false;

            text = node.PreprocessInputString(text);
            bool committingEmptyContent = text == string.Empty;
            bool committingSameContent = text == this.previousText;
            bool startedOffDefaultValueDriver = this.previousText == Configurations.DriverInitialTextValue;
            bool committingEmptyCodeBlock = text == string.Empty || text == Configurations.CodeBlockInitialMessage;
            bool startedOffEmptyCodeBlock = this.previousText == string.Empty || this.previousText == Configurations.CodeBlockInitialMessage;

            //Flow logic
            if (node.VisualType == NodeType.CodeBlock)
            {
                if (commit)
                {
                    if (committingEmptyCodeBlock)
                    {
                        if (startedOffEmptyCodeBlock)
                            delete = true;
                        else
                            recordAndDelete = true;
                    }
                    else //Not Empty
                    {
                        if (!committingSameContent)
                            validate = true;
                        if (startedOffEmptyCodeBlock)
                            recordAsCreation = true;
                    }
                }
                else //Not committing
                {
                    if (startedOffEmptyCodeBlock)
                        delete = true;
                }
            }
            else if (node.VisualType == NodeType.Driver)
            {
                if (commit)
                {
                    if (committingEmptyContent)
                    {
                        if (node.IsEditingText)
                            setToDefaultValue = true;
                        else if (node.IsEditingCaption)
                            setToPreviousValue = true;
                    }
                    else //Not Empty
                    {
                        if (!committingSameContent)
                            validate = true;
                    }
                }
            }
            else //Other nodes
            {
                if (commit)
                {
                    if (committingEmptyContent)
                        setToPreviousValue = true;
                    else //Not empty
                    {
                        if (!committingSameContent)
                            validate = true;
                    }
                }
            }

            //Execution
            node.Edit(this.previousText, false);
            if (delete)
            {
                if (null != visualHost)
                {
                    this.visualHost.RemoveDrawingVisual(node.NodeId);
                    this.visualHost.RemoveDrawingVisual(node.GetErrorBubbleId());
                    this.visualHost.RemoveDrawingVisual(node.GetPreviewBubbleId());
                    this.visualHost.RemoveExtendedBubble(node.GetPreviewBubbleId());
                }
                uint[] outputSlots = node.GetOutputSlots();
                uint[] inputSlots = node.GetInputSlots();
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
                this.nodeCollection.Remove(node.NodeId);
                this.bubbleCollection.Remove(node.GetErrorBubbleId());
                this.bubbleCollection.Remove(node.GetPreviewBubbleId());
            }
            if (recordAndDelete)
            {
                DeltaNodes deltaNodes = new DeltaNodes();
                deltaNodes.AppendToRemovedNodes(node);
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
            if (setToDefaultValue)
            {
                if (!startedOffDefaultValueDriver)
                {
                    DeltaNodes deltaNodes = new DeltaNodes();
                    deltaNodes.AppendToModifiedNodes(node);

                    this.undoRedoRecorder.BeginGroup();
                    this.undoRedoRecorder.RecordNodeModificationForUndo(deltaNodes.ModifiedNodes);
                    this.undoRedoRecorder.EndGroup();

                    node.Edit(Configurations.DriverInitialTextValue, false);
                    this.SynchronizeToLiveRunner(deltaNodes);
                }

                string warningMessage = "Field cannot be empty. Value will be set to 0.";
                node.SetErrorValue((object)warningMessage, ErrorType.Warning);
            }
            if (setToPreviousValue)
            {
                string warningMessage = "Field cannot be empty. Value will be reverted to previous state.";
                node.SetErrorValue((object)warningMessage, ErrorType.Warning);
            }
            if (validate)
            {
                DeltaNodes deltaNodes = new DeltaNodes();
                List<IVisualNode> addedNodes = new List<IVisualNode>();
                List<IVisualNode> modifiedNodes = new List<IVisualNode>();
                addedNodes.Add(node); // deltaNodes.AppendToAddedNodes(node);

                // Start monitoring if we are going to undefine any variables.
                this.graphProperties.RuntimeStates.BeginDefinitionMonitor();

                this.undoRedoRecorder.BeginGroup();
                this.undoRedoRecorder.RecordRuntimeStatesForUndo(this.graphProperties.RuntimeStates);
                if (recordAsCreation)
                    this.undoRedoRecorder.RecordNodeCreationForUndo(addedNodes);
                else //record edited node as modification
                {
                    deltaNodes.AppendToModifiedNodes(node);
                    deltaNodes.AppendToModifiedNodes(FindAssociatedNodesFromNodes(addedNodes));
                    this.undoRedoRecorder.RecordNodeModificationForUndo(deltaNodes.ModifiedNodes);
                }

                // These methods (especially the first two and last two) must be 
                // called in this particular order (due to their dependencies).
                // 
                // GC.ValidateNodesSyntax
                // RS.UpdateVariableDefinitionMap
                // GC.ValidateDefinedAndReferencedVariables
                // GC.UpdateSlotsVisiblity
                // GC.EstablishImplicitConnections
                // 
                node.SetErrorValue(null, ErrorType.None);
                node.Edit(text, false);
                this.ValidateNodesSyntax(addedNodes);
                this.graphProperties.RuntimeStates.UpdateVariablesDefinedInNode(node, false);
                this.ValidateDefinedAndReferencedVariables();
                this.UpdateSlotsVisiblity();
                this.EstablishImplicitConnections(modifiedNodes);
                this.edgeController.DeleteUnnecessaryEdges();
                this.undoRedoRecorder.EndGroup();

                // Now all the modifications are over, see what we have undefined.
                Dictionary<uint, List<string>> undefinedVariables = null;
                this.graphProperties.RuntimeStates.EndDefinitionMonitor(out undefinedVariables);
                deltaNodes.AppendUndefinedVariables(undefinedVariables);

                if (recordAsCreation)
                    deltaNodes.AppendToAddedNodes(addedNodes);

                deltaNodes.AppendToModifiedNodes(modifiedNodes);
                this.SynchronizeToLiveRunner(deltaNodes);
            }

            this.UpdateDirtyNodes();
            this.edgeController.UpdateEdgeConnectTo(node);
            node.DisableEdit();
            if (validate && visualHost != null)
                visualHost.ScrollToVisual(visualHost.GetDrawingVisualForNode(node.NodeId));
            return true;
        }

        private bool HandleBeginHighFrequencyUpdate(GraphCommand command)
        {
            uint nodeId = (uint)command.GetArgument(0);
            NodePart nodePart = ((NodePart)command.GetArgument(1));

            nodeSelectedForHighFrequencyUpdate = GetVisualNode(nodeId);
            if (null == nodeSelectedForHighFrequencyUpdate)
                return false;

            // @TODO(Ben): The caller would have to specify NodePart in 
            // the near future, but for now we are making some assumption.
            // 
            switch (nodeSelectedForHighFrequencyUpdate.VisualType)
            {
                case NodeType.Driver:
                    nodePart = NodePart.Text;
                    break;

                default:
                    throw new InvalidOperationException("Not available for other node type (097D3A07D806)");
            }

            nodeSelectedForHighFrequencyUpdate.EnableEdit(nodePart, -1);
            return true;
        }

        private bool HandleEndHighFrequencyUpdate(GraphCommand command)
        {
            string text = command.GetArgument(0) as string;
            UpdateNodeTextHighFrequency(text);
            nodeSelectedForHighFrequencyUpdate = null;
            return true;
        }

        private bool HandleDeleteComponents(GraphCommand command)
        {
            if (currentDragState != DragState.None)
            {
                currentDragState = DragState.None;
                edgeController.ResetParameters();
            }

            DeltaNodes deltaNodes = new DeltaNodes();
            foreach (VisualNode node in nodeCollection.Values)
            {
                if (node.Selected)
                    deltaNodes.AppendToRemovedNodes(node);
            }

            if ((null == deltaNodes.RemovedNodes) && edgeController.SelectedEdgeCount() == 0)
                return true;

            if (null != deltaNodes.RemovedNodes && (deltaNodes.RemovedNodes.Count > 0))
                deltaNodes.AppendToModifiedNodes(FindAssociatedNodesFromNodes(deltaNodes.RemovedNodes));
            List<IVisualNode> nodesConnectToEdge = edgeController.GetNodesFromSelectedEdges();
            foreach (IVisualNode node in nodesConnectToEdge)
            {
                bool inDeleteList = (null != deltaNodes.RemovedNodes && (deltaNodes.RemovedNodes.Contains(node)));
                bool inModifyList = (null != deltaNodes.ModifiedNodes && (deltaNodes.ModifiedNodes.Contains(node)));

                if (!inDeleteList && !inModifyList)
                    deltaNodes.AppendToModifiedNodes(node);
            }

            // Begin tracking variables being undefined in this process.
            {
                RuntimeStates runtimeStates = this.GetRuntimeStates();
                runtimeStates.BeginDefinitionMonitor();

                this.undoRedoRecorder.BeginGroup();
                this.undoRedoRecorder.RecordRuntimeStatesForUndo(this.graphProperties.RuntimeStates);
                this.undoRedoRecorder.RecordNodeDeletionForUndo(deltaNodes.RemovedNodes);
                this.undoRedoRecorder.RecordNodeModificationForUndo(deltaNodes.ModifiedNodes);
                this.edgeController.DeleteSelectedEdges();
                this.DeleteNodes(deltaNodes.RemovedNodes);
                this.ValidateDefinedAndReferencedVariables();
                this.UpdateSlotsVisiblity();
                EstablishImplicitConnections(this.nodeCollection.Values.ToList());
                this.undoRedoRecorder.EndGroup();

                // Gather the data of variables being undefined.
                Dictionary<uint, List<string>> undefinedVariables = null;
                runtimeStates.EndDefinitionMonitor(out undefinedVariables);
                deltaNodes.AppendUndefinedVariables(undefinedVariables);
            }

            //After DeleteSelectedEdges() and DeleteNodes() are executed, the nodes in the nodeToModify would already be modified
            if (false == deltaNodes.IsEmpty)
                this.SynchronizeToLiveRunner(deltaNodes);

            this.UpdateDirtyNodes();

            //If the node is selected for deletion by clicking on the preview, the lastHoveredNode should be reset
            lastHoveredNodeId = uint.MaxValue;

            selectionBox.UpdateSelectionBox(GetSelectedNodes());
            return true;
        }

        private bool HandleSelectComponent(GraphCommand command)
        {
            uint compId = (uint)command.GetArgument(0);
            ModifierKeys modifiers = ((ModifierKeys)command.GetArgument(1));

            IVisualNode selectedNode = null;
            nodeCollection.TryGetValue(compId, out selectedNode);

            if (selectedNode != null)
            {
                VisualNode node = (VisualNode)selectedNode;

                if (modifiers.HasFlag(ModifierKeys.Control))//toggle
                    node.Selected = !node.Selected;
                else
                    node.Selected = true;
            }
            else
                edgeController.SelectComponent(compId, modifiers);

            this.selectionBox.UpdateSelectionBox(GetSelectedNodes());
            return true;
        }

        private bool HandleSelectedMenuItem(GraphCommand command)
        {
            int menuItemId = (int)command.GetArgument(0);
            double mouseX = ((double)command.GetArgument(1));
            double mouseY = ((double)command.GetArgument(2));
            uint nodeId = (uint)command.GetArgument(3);
            NodePart part = ((NodePart)command.GetArgument(4));
            LibraryItem selectedItem = new LibraryItem();

            VisualNode node = this.GetVisualNode(nodeId);
            if (itemsProvider != null)
                selectedItem = this.itemsProvider.GetItem(menuItemId);

            if (selectedItem == null) //No match in the library
            {
                if (part == NodePart.NorthWest)
                    this.HandleTopLeftSelectedItem(menuItemId, nodeId);
                else if (menuItemId == Configurations.ConvertNodeToCode)
                {
                    HandleConvertSelectionToCode();
                    return true;
                }
                else if (menuItemId < Configurations.PropertyBase)
                {
                    // @TODO(Joy): Use switch-case for "menuItemId".
                    ProcessPreviewMenuItems(menuItemId, nodeId);
                    UpdateDirtyNodes();
                    return true;
                }

            }
            else //Item found in library
            {
                if (selectedItem.Children != null && selectedItem.Children.Count > 0)
                    selectedItem = selectedItem.Children[0];

                LibraryItem.MemberType itemType = this.GetItemType(menuItemId);
                DeltaNodes deltaNodes = new DeltaNodes();
                VisualNode newNode = null;

                if (part == NodePart.NorthEast)
                {
                    if (selectedItem.Type == LibraryItem.MemberType.InstanceProperty)
                    {
                        if (selectedItem.ItemType == NodeType.Function)
                            newNode = this.CreateMethodNode(mouseX, mouseY, selectedItem.Assembly, selectedItem.QualifiedName, selectedItem.ArgumentTypes);
                        else
                            newNode = this.CreatePropertyNode(mouseX, mouseY, selectedItem.Assembly, selectedItem.QualifiedName, selectedItem.ArgumentTypes);

                        deltaNodes.AppendToAddedNodes(newNode);
                    }
                    else if (selectedItem.Type == LibraryItem.MemberType.InstanceMethod)
                    {
                        newNode = this.CreateMethodNode(mouseX, mouseY, selectedItem.Assembly, selectedItem.QualifiedName, selectedItem.ArgumentTypes);
                        deltaNodes.AppendToAddedNodes(newNode);
                    }

                    if (newNode != null && (null != deltaNodes.AddedNodes) && deltaNodes.AddedNodes.Count > 0)
                    {
                        // @keyu: If we are acessing its member function or 
                        // property, there is no reason to mark this node as 
                        // dirty. Comment it out to avoid to recreate object.
                        // 
                        // deltaNodes.AppendToModifiedNodes(node);

                        Slot inputSlot = this.GetSlot(newNode.GetInputSlot(0)) as Slot;
                        Slot outputSlot = this.GetSlot(node.GetOutputSlot(0)) as Slot;

                        this.undoRedoRecorder.BeginGroup();
                        this.undoRedoRecorder.RecordRuntimeStatesForUndo(this.graphProperties.RuntimeStates);
                        this.undoRedoRecorder.RecordNodeModificationForUndo(deltaNodes.ModifiedNodes);
                        this.graphProperties.RuntimeStates.AddVariablesDefinedInNode(newNode, false);
                        this.edgeController.CreateLinkingEdge(outputSlot, inputSlot);
                        this.undoRedoRecorder.RecordNodeCreationForUndo(deltaNodes.AddedNodes);
                        this.undoRedoRecorder.EndGroup();

                        //After EstablishLinkingConnection() is executed, the nodes in the nodeToModify would already be modified
                        this.SynchronizeToLiveRunner(deltaNodes);
                    }
                }
                else if (part == NodePart.North)
                {
                    if (itemType == LibraryItem.MemberType.Constructor)
                    {
                        deltaNodes.AppendToModifiedNodes(node);
                        deltaNodes.AppendToModifiedNodes(FindAssociatedNodesFromNodes(deltaNodes.ModifiedNodes));

                        this.undoRedoRecorder.BeginGroup();
                        this.undoRedoRecorder.RecordNodeModificationForUndo(deltaNodes.ModifiedNodes);
                        //this.undoRedoRecorder.RecordEdgeModificationForUndo(edgeController.FindEdgeConnectTo(node));
                        ((FunctionNode)node).ChangeConstructor(selectedItem.QualifiedName, selectedItem.ArgumentTypes, selectedItem.ArgumentNames);
                        this.undoRedoRecorder.EndGroup();
                        //After ChangeConstructor() is executed, the nodes in the nodeToModify would already be modified
                        this.SynchronizeToLiveRunner(deltaNodes);
                    }
                    else if (itemType == LibraryItem.MemberType.InstanceProperty)
                    {
                        deltaNodes.AppendToRemovedNodes(node);
                        deltaNodes.AppendToModifiedNodes(this.FindAssociatedNodesFromNodes(deltaNodes.RemovedNodes));

                        newNode = this.CreatePropertyNode(mouseX, mouseY, selectedItem.Assembly, selectedItem.QualifiedName, selectedItem.ArgumentTypes);
                        deltaNodes.AppendToAddedNodes(newNode);

                        this.undoRedoRecorder.BeginGroup();
                        this.undoRedoRecorder.RecordRuntimeStatesForUndo(this.graphProperties.RuntimeStates);
                        this.undoRedoRecorder.RecordNodeDeletionForUndo(deltaNodes.RemovedNodes);
                        this.undoRedoRecorder.RecordNodeModificationForUndo(deltaNodes.ModifiedNodes);
                        this.TransferInformationFromMethodToProperty(node, newNode);
                        this.undoRedoRecorder.RecordNodeCreationForUndo(deltaNodes.AddedNodes);
                        this.DeleteNodes(deltaNodes.RemovedNodes);
                        this.undoRedoRecorder.EndGroup();

                        //After DeleteNodes() is executed, the nodes in the nodeToModify would already be modified
                        this.SynchronizeToLiveRunner(deltaNodes);
                    }
                    else if (itemType == LibraryItem.MemberType.InstanceMethod)
                    {
                        deltaNodes.AppendToModifiedNodes(node);
                        deltaNodes.AppendToModifiedNodes(FindAssociatedNodesFromNodes(deltaNodes.ModifiedNodes));

                        this.undoRedoRecorder.BeginGroup();
                        this.undoRedoRecorder.RecordNodeModificationForUndo(deltaNodes.ModifiedNodes);
                        ((FunctionNode)node).ChangeMethod(selectedItem.QualifiedName, selectedItem.ArgumentTypes);
                        this.undoRedoRecorder.EndGroup();

                        //After ChangeConstructor() is executed, the nodes in the nodeToModify would already be modified
                        this.SynchronizeToLiveRunner(deltaNodes);
                    }
                }
            }

            this.UpdateDirtyNodes();
            edgeController.UpdateEdgeConnectTo(node);
            return true;
        }

        private bool HandleCreateRadialMenu(GraphCommand command)
        {
            NodePart part = ((NodePart)command.GetArgument(0));
            uint nodeId = (uint)command.GetArgument(1);
            this.itemsProvider = new MenuItemsProvider(this, part, nodeId);
            return true;
        }

        private bool HandleCreateSubRadialMenu(GraphCommand command)
        {
            int selectedItemId = (int)command.GetArgument(0);
            this.itemsProvider.PopulateOverloads(selectedItemId);
            return true;
        }

        private bool HandleClearSelection(GraphCommand command)
        {
            ClearSelectionInternal();
            return true;
        }

        private bool HandleUndoOperation(GraphCommand command)
        {
            DeltaNodes deltaNodes = new DeltaNodes();

            // Start tracking variables being undefined in this process.
            {
                RuntimeStates runtimeStates = this.GetRuntimeStates();
                runtimeStates.BeginDefinitionMonitor();
                this.undoRedoRecorder.Undo(deltaNodes);

                Dictionary<uint, List<string>> undefinedVariables = null;
                runtimeStates.EndDefinitionMonitor(out undefinedVariables);
                deltaNodes.AppendUndefinedVariables(undefinedVariables);
            }

            this.ValidateNodesSyntax(this.nodeCollection.Values.ToList<IVisualNode>());
            this.ValidateDefinedAndReferencedVariables();
            if (false == deltaNodes.IsEmpty)
                this.SynchronizeToLiveRunner(deltaNodes);

            this.UpdateDirtyNodes();
            this.edgeController.UpdateDirtyEdges(deltaNodes.ModifiedNodes);
            selectionBox.UpdateSelectionBox(GetSelectedNodes());
            return true;
        }

        private bool HandleRedoOperation(GraphCommand command)
        {
            DeltaNodes deltaNodes = new DeltaNodes();

            // Start tracking variables being undefined in this process.
            {
                RuntimeStates runtimeStates = this.GetRuntimeStates();
                runtimeStates.BeginDefinitionMonitor();
                this.undoRedoRecorder.Redo(deltaNodes);

                Dictionary<uint, List<string>> undefinedVariables = null;
                runtimeStates.EndDefinitionMonitor(out undefinedVariables);
                deltaNodes.AppendUndefinedVariables(undefinedVariables);
            }

            this.ValidateNodesSyntax(this.nodeCollection.Values.ToList<IVisualNode>());
            this.ValidateDefinedAndReferencedVariables();
            if (false == deltaNodes.IsEmpty)
                this.SynchronizeToLiveRunner(deltaNodes);

            this.UpdateDirtyNodes();
            this.edgeController.UpdateDirtyEdges(deltaNodes.ModifiedNodes);
            selectionBox.UpdateSelectionBox(GetSelectedNodes());
            return true;
        }

        private bool HandleEditReplicationGuide(GraphCommand command)
        {
            uint nodeId = (uint)command.GetArgument(0);
            int replicationIndex = (int)command.GetArgument(1);
            VisualNode node = this.GetVisualNode(nodeId);
            node.EnableEdit(NodePart.ReplicationGuide, replicationIndex);
            return true;
        }

        private bool HandleSetReplicationGuideText(GraphCommand command)
        {
            uint nodeId = (uint)command.GetArgument(0);
            int replicationIndex = (int)command.GetArgument(1);
            string text = ((string)command.GetArgument(2));

            VisualNode node = GetVisualNode(nodeId);
            if (node.VisualType != NodeType.Function)
                throw new InvalidOperationException("Node must be FunctionNode");

            DeltaNodes deltaNodes = new DeltaNodes();
            IVisualNode iNode = this.GetVisualNode(nodeId);
            deltaNodes.AppendToModifiedNodes(iNode);

            FunctionNode fNode = node as FunctionNode;
            undoRedoRecorder.BeginGroup();
            undoRedoRecorder.RecordNodeModificationForUndo(deltaNodes.ModifiedNodes);
            undoRedoRecorder.EndGroup();
            fNode.SetReplicationText(replicationIndex, text);
            //if(!fNode.SetReplicationText(replicationIndex,text))
            //    undoRedoRecorder.PopRecordFromUndoStorage();
            node.DisableEdit();
            node.Dirty = true;
            node.Compose();
            this.SynchronizeToLiveRunner(deltaNodes);
            return true;
        }

        private bool HandleImportScript(GraphCommand command)
        {
            string scriptPath = (string)command.GetArgument(0);
            if (string.IsNullOrEmpty(scriptPath))
                return false;

            // check if the file is already imported
            if (this.GetImportedScripts().Exists((string file) => scriptPath.Equals(file, StringComparison.InvariantCultureIgnoreCase)))
            {
                CoreComponent.Instance.AddFeedbackMessage(ResourceNames.Error, UiStrings.DuplicateFailure.Replace("fileName", System.IO.Path.GetFileName(scriptPath)));
                return false;
            }

            if (CoreComponent.Instance.ImportAssembly(scriptPath, this.FilePath, false))
            {
                List<string> scripts = this.GetImportedScripts();
                if (!scripts.Exists((string file) => scriptPath.Equals(file, StringComparison.InvariantCultureIgnoreCase)))
                    scripts.Add(scriptPath);
            }

            return true;
        }

        private bool HandleConvertSelectionToCode()
        {
            List<IVisualNode> nodes = GetSelectedNodes();
            if (nodes == null || (nodes.Count <= 0))
                return false;

            foreach (VisualNode node in nodes)
            {
                if (node.Error)
                {
                    ShowNodesToCodeErrorMessage();
                    return false;
                }
            }

            List<SnapshotNode> snapshotNodes = CreateSnapshotNodesFromVisualNodes(nodes);
            if (snapshotNodes == null || (snapshotNodes.Count <= 0))
                return false;

            CurrentSynchronizer.ConvertNodesToCode(snapshotNodes);
            return true;
        }

        private bool HandleTogglePreview(GraphCommand command)
        {
            uint bubbleId = (uint)command.GetArgument(0);
            if (bubbleId == uint.MaxValue)
                return ToggleAllPreviews();

            PreviewBubble previewBubble = bubbleCollection[bubbleId] as PreviewBubble;
            if (previewBubble == null)
                return false;

            VisualNode node = GetVisualNode(previewBubble.OwnerId);
            if (node == null)
                return false;

            List<IVisualNode> nodeList = new List<IVisualNode>();
            nodeList.Add(node);

            undoRedoRecorder.BeginGroup();
            undoRedoRecorder.RecordNodeModificationForUndo(nodeList);
            undoRedoRecorder.EndGroup();

            if (node.NodeStates.HasFlag(States.PreviewHidden))
            {
                node.ClearState(States.PreviewHidden);
                CurrentSynchronizer.BeginQueryNodeValue(node.NodeId);
            }
            else
                node.SetNodeState(States.PreviewHidden);

            previewBubble.Collapse(node.NodeStates.HasFlag(States.PreviewHidden));

            return true;
        }

        #endregion
    }
}
