using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using DesignScriptStudio.Graph.Core.Properties;
using DesignScriptStudio.Graph.Core.VMServices;
using GraphToDSCompiler;
using System.Collections.ObjectModel;

namespace DesignScriptStudio.Graph.Core
{
    partial class GraphController : IGraphController
    {
        #region Internal Class Data Members

        internal EdgeConnectionFlag edgeConnection = EdgeConnectionFlag.None;

        #endregion

        #region Internal Class Propreties

        internal bool FileLoadInProgress { get; private set; }

        internal bool IsInUndoRedoCommand
        {
            get
            {
                //GC methods is only accessible for undo and redo operation
                return (currentCommand == GraphCommand.Name.UndoOperation ||
                    (currentCommand == GraphCommand.Name.RedoOperation));
            }
        }

        #endregion

        #region Internal Class Operational Methods

        internal GraphController(IGraphVisualHost visualHost)
        {
            this.InitializeInternal(visualHost);
        }

        internal GraphController(IGraphVisualHost visualHost, string filePath)
        {
            if (!File.Exists(filePath))
                throw new ArgumentException("'filePath' is not a valid path");

            this.FileLoadInProgress = true;
            this.InitializeInternal(visualHost);
            this.LoadFileInternal(filePath);
            this.filePath = filePath;

            try
            {
                // Attempt to see if we are creating the graph controller from recovery file.
                int index = -1; uint graphId = uint.MaxValue;
                Utilities.GetBackupFileIndex(this.filePath, ref graphId, ref index);

                // If we reach here without exception, then "filePath" is a backup file 
                // name. In which case we should not assign a name to "this.filePath" so 
                // that the user will be prompted to supply an alternative file name.
                // 
                this.filePath = string.Empty;
            }
            catch (Exception)
            {
            }

            DeltaNodes deltaNodes = new DeltaNodes();
            deltaNodes.AppendToAddedNodes(this.nodeCollection.Values.ToList<IVisualNode>());
            this.ValidateNodesSyntax(deltaNodes.AddedNodes);
            this.ValidateDefinedAndReferencedVariables();
            this.UpdateDirtyNodes();
            edgeController.AuditEdges();
            this.edgeController.UpdateDirtyEdges(null);
            if (this.nodeCollection.Count > 0)
                this.SynchronizeToLiveRunner(deltaNodes);
            this.FileLoadInProgress = false;
        }

        internal IGraphVisualHost GetVisualHost()
        {
            return this.visualHost;
        }

        internal IdGenerator GetIdGenerator()
        {
            if (this.idGenerator == null)
                this.idGenerator = new IdGenerator();

            return this.idGenerator;
        }

        internal RuntimeStates GetRuntimeStates()
        {
            return this.graphProperties.RuntimeStates;
        }

        internal UndoRedoRecorder GetUndoRedoRecorder()
        {
            return this.undoRedoRecorder;
        }

        internal ISlot GetSlot(uint slotId)
        {
            ISlot slot = null;
            this.slotCollection.TryGetValue(slotId, out slot);
            return slot;
        }

        internal List<ISlot> GetSlots()
        {
            return new List<ISlot>(this.slotCollection.Values);
        }

        internal VisualNode GetVisualNode(uint nodeId)
        {
            //@TODO(Ben) fix once there is some consistency between VisualNode and IVisualNode
            IVisualNode node = null;
            this.nodeCollection.TryGetValue(nodeId, out node);
            return (VisualNode)node;
        }

        internal List<IVisualNode> GetVisualNodes()
        {
            return new List<IVisualNode>(this.nodeCollection.Values);
        }

        internal VisualEdge GetVisualEdge(uint edgeId)
        {
            //@TODO(Ben) fix once there is some consistency between VisualEdge and IVisualEdge
            if (null == edgeController)
                throw new InvalidOperationException("'edgeController' was not created");

            return (VisualEdge)edgeController.GetVisualEdge(edgeId);
        }

        internal IVisualEdge CreateEdgeFromStorage(IStorage storage)
        {
            if (!this.IsInUndoRedoCommand)
                throw new InvalidOperationException("Can only be access for undo/redo");

            if (null == edgeController)
                throw new InvalidOperationException("'edgeController' was not created");

            if (null == storage)
                throw new ArgumentNullException("storage");

            return VisualEdge.Create(this.edgeController, storage);
        }

        internal void AddVisualEdge(IVisualEdge edge)
        {
            if (null == edge)
                throw new ArgumentNullException("edge");

            if (null == edgeController)
                throw new InvalidOperationException("'edgeController' was not created");

            this.edgeController.AddEdge((VisualEdge)edge);
        }

        internal void AddSlot(ISlot slot)
        {
            if (null == slot)
                throw new ArgumentNullException("slot");

            this.slotCollection.Add(slot.SlotId, slot);
        }

        internal void AddVisualNode(IVisualNode node)
        {
            if (null == node)
                throw new ArgumentNullException("node");

            this.nodeCollection.Add(node.NodeId, node);
        }

        internal void AddBubble(InfoBubble bubble)
        {
            if (null == bubble)
                throw new ArgumentNullException("bubble");

            this.bubbleCollection.Add(bubble.BubbleId, bubble);
        }

        internal void RemoveVisualEdge(uint edgeId)
        {
            if (!this.IsInUndoRedoCommand)
                throw new InvalidOperationException("Can only be access for undo/redo and editing");

            if (null == edgeController)
                throw new InvalidOperationException("'edgeController' was not created");

            this.edgeController.RemoveEdge(edgeId);
        }

        internal void RemoveSlot(uint slotId)
        {
            if (!this.IsInUndoRedoCommand && !this.IsInEndNodeEditCommand && !this.IsSelectMenuItemCommand)
                throw new InvalidOperationException("Can only be access for undo/redo, editing and changing construtor");

            this.slotCollection.Remove(slotId);
        }

        internal void RemoveVisualNode(uint nodeId)
        {
            if (!this.IsInUndoRedoCommand)
                throw new InvalidOperationException("Can only be access for undo/redo and editing");

            this.nodeCollection.Remove(nodeId);
        }

        internal void RemoveBubble(uint bubbleId)
        {
            if (!this.IsInUndoRedoCommand)
                throw new InvalidOperationException("Can only be access for undo/redo and editing");

            this.bubbleCollection.Remove(bubbleId);
        }

        internal void RemoveDrawingVisual(uint compId)
        {
            if (!this.IsInUndoRedoCommand)
                throw new InvalidOperationException("Can only be access for undo/redo");

            if (this.visualHost != null)
                this.visualHost.RemoveDrawingVisual(compId);
        }

        internal void RemoveExtendedBubble(uint bubbleId)
        {
            if (!this.IsInUndoRedoCommand)
                throw new InvalidOperationException("Can only be access for undo/redo");

            if (this.visualHost != null)
                this.visualHost.RemoveExtendedBubble(bubbleId);
        }

        internal void DeleteEdgesConnectTo(uint slotId)
        {
            if (null == edgeController)
                throw new InvalidOperationException("'edgeController' was not created");

            edgeController.DeleteEdgesConnectTo(slotId);
        }

        internal List<VisualEdge> FindEdgesConnectingTo(uint slotId)
        {
            if (null == edgeController)
                throw new InvalidOperationException("'edgeController' was not created");

            return edgeController.FindEdgesConnectingToSlot(slotId, false);
        }

        internal bool ContainSlotKey(uint slotId)
        {
            return this.slotCollection.ContainsKey(slotId);
        }

        internal void SynchronizeToLiveRunner(DeltaNodes deltaNodes)
        {
            if (false != deltaNodes.IsEmpty)
                throw new InvalidOperationException("Nothing to send to 'lifeRunner'");

            // If there is something that we would like to query 
            // the value for, add it into the modified node list.
            if (false == scheduledDeltaNodes.IsEmpty)
            {
                deltaNodes.AppendToRemovedNodes(scheduledDeltaNodes.RemovedNodes);
                deltaNodes.AppendToAddedNodes(scheduledDeltaNodes.AddedNodes);
                deltaNodes.AppendToModifiedNodes(scheduledDeltaNodes.ModifiedNodes);
                scheduledDeltaNodes.Reset();
            }

            // Compact the DeltaNodes so that there is no duplicate and no node
            // exists in more than one list (e.g. addedNodes and modifiedNodes).
            deltaNodes.Compact();

            SynchronizeData sd = new SynchronizeData();
            if (null != deltaNodes.RemovedNodes && (deltaNodes.RemovedNodes.Count > 0))
            {
                foreach (IVisualNode node in deltaNodes.RemovedNodes)
                    sd.RemovedNodes.Add(node.NodeId);
            }

            if (null != deltaNodes.AddedNodes && (deltaNodes.AddedNodes.Count > 0))
                sd.AddedNodes = this.CreateSnapshotNodesFromVisualNodes(deltaNodes.AddedNodes);
            if (null != deltaNodes.ModifiedNodes && (deltaNodes.ModifiedNodes.Count > 0))
                sd.ModifiedNodes = this.CreateSnapshotNodesFromVisualNodes(deltaNodes.ModifiedNodes);

            // Tell the graph compiler that some variables are undefined.
            MergeUndefinedVariablesIntoData(deltaNodes.Undefinitions, sd);

            this.sdc.sdList.Add(sd);
            if (Configurations.EnableLiveExection)
            {
                try
                {
                    this.CurrentSynchronizer.PushUpdate(sd);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }
            }
        }

        public bool ProcessNodesToCodeConversion(List<uint> originalNodeIds, List<SnapshotNode> snapshotNodes)
        {
            if (originalNodeIds == null || originalNodeIds.Count == 0 || snapshotNodes == null || snapshotNodes.Count == 0)
                return false;

            undoRedoRecorder.BeginGroup();
            undoRedoRecorder.RecordRuntimeStatesForUndo(graphProperties.RuntimeStates);

            List<IVisualNode> originalNodes = GetNodesByIds(originalNodeIds);
            List<IVisualNode> modifiedNodes = new List<IVisualNode>();
            DeleteNodesWithRecord(originalNodes, out modifiedNodes);

            List<Connection> inputConnections = null;
            Dictionary<string, string> identifierTempVariableMap = null;
            List<VisualNode> identifierNodes = CreateIntermediateIdentifierNodes(snapshotNodes, out inputConnections, out identifierTempVariableMap);
            ReplaceVariableNamesInSnapshotNodes(identifierTempVariableMap, snapshotNodes);
            List<VisualNode> codeBlockNodes = CreateVisualNodesFromSnapshotNodes(snapshotNodes);

            List<IVisualNode> createdNodes = new List<IVisualNode>();
            createdNodes.AddRange(identifierNodes);
            createdNodes.AddRange(codeBlockNodes);

            SetNodesPosition(createdNodes, originalNodes[0].X, originalNodes[0].Y);

            Dictionary<VisualNode, List<Connection>> outputNodeConnectionMap = GetNodeOutputConnectionMapFromSnapshotNodes(snapshotNodes, codeBlockNodes);
            Dictionary<VisualNode, List<Connection>> inputNodeConnectionMap = GetNodeInputConnectionMapFromSnapshotNodes(identifierNodes, inputConnections);
            EstablishExplicitConnections(inputNodeConnectionMap, outputNodeConnectionMap, modifiedNodes);

            EstablishImplicitConnections(modifiedNodes);
            undoRedoRecorder.EndGroup();

            ValidateDefinedAndReferencedVariables();
            BeautifyCode(codeBlockNodes);
            UpdateSlotsVisiblity();
            selectionBox.UpdateSelectionBox(GetSelectedNodes());
            UpdateDirtyNodes();
            edgeController.UpdateDirtyEdges(createdNodes);

            //update delta nodes send to liveRunner
            DeltaNodes deltaNodes = new DeltaNodes();
            deltaNodes.AppendToAddedNodes(createdNodes);
            deltaNodes.AppendToModifiedNodes(modifiedNodes.Distinct().ToList());
            deltaNodes.AppendToRemovedNodes(originalNodes);
            SynchronizeToLiveRunner(deltaNodes);
            return true;
        }

        internal string MapAssemblyPath(string assembly)
        {
            foreach (string assemblyPath in this.GetImportedScripts())
            {
                if (Path.GetFileName(assemblyPath) == assembly)
                    return assemblyPath;
            }

            return CoreComponent.Instance.MapAssemblyPath(assembly);
        }

        #endregion
    }
}
