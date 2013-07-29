using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DesignScriptStudio.Graph.Core
{
    class UndoRedoRecorder
    {
        private enum UserAction
        {
            RuntimeStates,
            EdgeCreation,
            EdgeDeletion,
            EdgeModification,
            NodeCreation,
            NodeDeletion,
            NodeModification
        }

        #region Class Data Members

        private GraphController graphController = null;
        private IStorage undoStorage = new BinaryStorage();
        private IStorage redoStorage = new BinaryStorage();
        private List<uint> recordedComponents = new List<uint>();
        private int actionCount = -1;

        #endregion

        #region Internal Class Properties

        internal bool CanUndo
        {
            get
            {
                return this.undoStorage.GetPosition() > 0;
            }
        }

        internal bool CanRedo
        {
            get
            {
                return this.redoStorage.GetPosition() > 0;
            }
        }

        internal int ActionCount
        {
            get
            {
                return this.actionCount;
            }
        }

        #endregion

        #region Internal Class Related Methods

        internal UndoRedoRecorder(IGraphController graphController)
        {
            this.graphController = graphController as GraphController;
            this.actionCount = -1;
        }

        internal bool Undo(DeltaNodes deltaNodes)
        {
            if (this.ActionCount != -1)
                throw new InvalidOperationException("EndGroup() function was not called");

            //Check if undoStorage is empty
            if (this.undoStorage.GetPosition() <= 0)
                return false;

            //Retrieve number of UserActions in the group
            this.undoStorage.Seek(-12, SeekOrigin.Current);
            int numOfActions = this.undoStorage.ReadInteger(FieldCode.ActionCount);
            this.undoStorage.Seek(-12, SeekOrigin.Current);

            //Loop according to the number of UserAction
            for (int i = 1; i <= numOfActions; i++)
            {
                //Retrieve the start point of the data and UserAction type
                this.undoStorage.Seek(-28, SeekOrigin.Current);
                long shift = this.undoStorage.ReadLong(FieldCode.Shift);
                UserAction userAction = (UserAction)this.undoStorage.ReadInteger(FieldCode.UserAction);
                this.undoStorage.Seek(-(shift + 28), SeekOrigin.Current);

                //Undo previous task accordingly
                switch (userAction)
                {
                    case UserAction.RuntimeStates:
                        //1. Retrieve the current runtimeStates and store the current runtimeStates in the redoStorage
                        //2. Update the runtimeStates through deserialization from the undoStorage
                        this.RecordRuntimeStates(this.redoStorage, this.graphController.GetRuntimeStates(), UserAction.RuntimeStates);
                        this.UpdateRuntimeStates(this.undoStorage, this.graphController.GetRuntimeStates());
                        break;

                    case UserAction.EdgeCreation:
                        //1. Retrieve edgeId from undoStorage and find the corresponding edge from the dictionary
                        //2. Store the corresponding edge in the redoStorage
                        //3. Delete corresponding edge in dictionary
                        List<IVisualEdge> edgeListForCreation = this.RetrieveEdgesFromIdsInStorage(this.undoStorage);
                        this.RecordEdges(this.redoStorage, edgeListForCreation, UserAction.EdgeCreation);
                        this.DeleteEdges(edgeListForCreation);
                        break;

                    case UserAction.EdgeDeletion:
                        //1. Retrieve the edge from undoStorage
                        //2. Store the corresponding edgeId in the redoStorage 
                        //3. Add the corresponding edge to the dictionary
                        List<IVisualEdge> edgeListForDeletion = this.RetrieveEdgesFromStorage(this.undoStorage);
                        this.RecordEdgeIds(this.redoStorage, edgeListForDeletion, UserAction.EdgeDeletion);
                        this.AddEdges(edgeListForDeletion);
                        break;

                    case UserAction.EdgeModification:
                        //1. Retrieve the edgeId from undoStorage without creating the entire edge
                        //2. Retrieve the corresponding edge in the dictionary
                        //3. Store the corresponding edge in the redoStorage
                        //4. Reset cursor position in stream to start of first header, ie. -shift value
                        //5. Update the corresponding edge in the dictionary through deserialization from
                        //   from the undoStorage
                        List<uint> edgeIdList = this.RetrieveEdgeIdsFromStorage(this.undoStorage);
                        List<IVisualEdge> edgeList = this.RetrieveEdgesFromDictionary(edgeIdList);
                        this.RecordEdges(this.redoStorage, edgeList, UserAction.EdgeModification);
                        this.undoStorage.Seek(-shift, SeekOrigin.Current);
                        this.UpdateEdges(this.undoStorage, edgeList);
                        break;

                    case UserAction.NodeCreation:
                        //1. Retrieve nodeId from undoStorage and find the corresponding node from the dictionary
                        //2. Store the corresponding node and slot in the redoStorage
                        //3. Delete corresponding node and slot in dictionary
                        //4. Record the deleted node for LifeRunner
                        List<IVisualNode> nodeListForCreation = this.RetrieveNodesFromIdsInStorage(this.undoStorage);
                        this.RecordNodesAndSlots(this.redoStorage, nodeListForCreation, UserAction.NodeCreation);
                        this.DeleteNodesAndSlots(nodeListForCreation);
                        deltaNodes.AppendToRemovedNodes(nodeListForCreation);
                        break;

                    case UserAction.NodeDeletion:
                        //1. Retrieve the slot from undoStorage
                        //2. Retrieve the node from undoStorage
                        //3. Store the corresponding nodeId in the redoStorage 
                        //4. Add the corresponding node and slot to the dictionary
                        //5. Record the added node for LifeRunner
                        List<ISlot> slotListForDeletion = this.RetrieveSlotsFromStorage(this.undoStorage);
                        List<IVisualNode> nodeListForDeletion = this.RetrieveNodesFromStorage(this.undoStorage);
                        this.RecordNodeIds(this.redoStorage, nodeListForDeletion, UserAction.NodeDeletion);
                        this.AddNodesAndSlots(nodeListForDeletion, slotListForDeletion);
                        deltaNodes.AppendToAddedNodes(nodeListForDeletion);
                        break;

                    case UserAction.NodeModification:
                        //1. Retrieve the slotId from undoStorage without creating the entire slot
                        //2. Retrieve the nodeId from undoStorage without creating the entire node
                        //3. Retrieve the corresponding node in the dictionary
                        //4. Store the corresponding node and slot in the redoStorage
                        //5. Resolve, if any, the extra number of slot after undo
                        //6. Reset cursor position in stream to start of first header, ie, -shift value
                        //7. Update the corresponding node and slot in the dictionary through deserialization 
                        //   from the undoStorage and resolve, if any, the missing slot after undo
                        //8. Record the modified node for LifeRunner
                        List<uint> slotIdList = this.RetrieveSlotIdsFromStorage(this.undoStorage);
                        List<uint> nodeIdList = this.RetrieveNodeIdsFromStorage(this.undoStorage);
                        List<IVisualNode> nodeList = this.RetrieveNodesFromDictionary(nodeIdList);
                        this.RecordNodesAndSlots(this.redoStorage, nodeList, UserAction.NodeModification);
                        this.ResolveExtraNumberOfSlotsAfterAction(nodeList, slotIdList);
                        this.undoStorage.Seek(-shift, SeekOrigin.Current);
                        this.UpdateNodesAndSlostWithResolveMissingSlotAfterAction(this.undoStorage, nodeList, slotIdList);
                        deltaNodes.AppendToModifiedNodes(nodeList);
                        break;

                    default:
                        throw new InvalidOperationException("Unknown UserAction");
                }

                //Set cursor for next undo
                this.undoStorage.Seek(-shift, SeekOrigin.Current);
            }

            //Upon Success
            this.redoStorage.WriteInteger(FieldCode.ActionCount, numOfActions);
            return true;
        }

        internal bool Redo(DeltaNodes deltaNodes)
        {
            if (this.ActionCount != -1)
                throw new InvalidOperationException("EndGroup() function was not called");
            //Check if redo storage is empty
            if (this.redoStorage.GetPosition() <= 0)
                return false;

            //Retrieve number of UserActions in the group
            this.redoStorage.Seek(-12, SeekOrigin.Current);
            int numOfActions = this.redoStorage.ReadInteger(FieldCode.ActionCount);
            this.redoStorage.Seek(-12, SeekOrigin.Current);

            //Loop according to the number of UserAction
            for (int i = 1; i <= numOfActions; i++)
            {
                //Retrieve the start point of the data and UserAction type
                this.redoStorage.Seek(-28, SeekOrigin.Current);
                long shift = this.redoStorage.ReadLong(FieldCode.Shift);
                UserAction userAction = (UserAction)this.redoStorage.ReadInteger(FieldCode.UserAction);
                this.redoStorage.Seek(-(shift + 28), SeekOrigin.Current);

                //Redo previous task accordingly
                switch (userAction)
                {
                    case UserAction.RuntimeStates:
                        //1. Retrieve the current runtimeStates and store the current runtimeStates in the undoStorage
                        //2. Update the runtimeStates through deserialization from the redoStorage
                        this.RecordRuntimeStates(this.undoStorage, this.graphController.GetRuntimeStates(), UserAction.RuntimeStates);
                        this.UpdateRuntimeStates(this.redoStorage, this.graphController.GetRuntimeStates());
                        break;

                    case UserAction.EdgeCreation:
                        //1. Retrieve the edge from redoStorage
                        //2. Store the corresponding edgeId in the undoStorage 
                        //3. Add the corresponding edge to the dictionary
                        List<IVisualEdge> edgeListForCreation = this.RetrieveEdgesFromStorage(this.redoStorage);
                        this.RecordEdgeIds(this.undoStorage, edgeListForCreation, UserAction.EdgeCreation);
                        this.AddEdges(edgeListForCreation);
                        break;

                    case UserAction.EdgeDeletion:
                        //1. Retrieve edgeId from redoStorage and find the corresponding edge from the dictionary
                        //2. Store the corresponding edge in the undoStorage
                        //3. Delete corresponding edge in dictionary
                        List<IVisualEdge> edgeListForDeletion = this.RetrieveEdgesFromIdsInStorage(this.redoStorage);
                        this.RecordEdges(this.undoStorage, edgeListForDeletion, UserAction.EdgeDeletion);
                        this.DeleteEdges(edgeListForDeletion);
                        break;

                    case UserAction.EdgeModification:
                        //1. Retrieve the edgeId from redoStorage without creating the entire edge
                        //2. Retrieve the corresponding edge in the dictionary
                        //3. Store the corresponding edge in the undoStorage
                        //4. Reset cursor position in stream to start of first header, ie. -shift value
                        //5. Update the corresponding edge in the dictionary through deserialization from
                        //   from the redoStorage
                        List<uint> edgeIdList = this.RetrieveEdgeIdsFromStorage(this.redoStorage);
                        List<IVisualEdge> edgeList = this.RetrieveEdgesFromDictionary(edgeIdList);
                        this.RecordEdges(this.undoStorage, edgeList, UserAction.EdgeModification);
                        this.redoStorage.Seek(-shift, SeekOrigin.Current);
                        this.UpdateEdges(this.redoStorage, edgeList);
                        break;

                    case UserAction.NodeCreation:
                        //1. Retrieve the slot from redoStorage
                        //2. Retrieve the node from redoStorage
                        //3. Store the corresponding nodeId in the undoStorage 
                        //4. Add the corresponding node and slot to the dictionary
                        //5. Record the added node for LifeRunner
                        List<ISlot> slotListForCreation = this.RetrieveSlotsFromStorage(this.redoStorage);
                        List<IVisualNode> nodeListForCreation = this.RetrieveNodesFromStorage(this.redoStorage);
                        this.RecordNodeIds(this.undoStorage, nodeListForCreation, UserAction.NodeCreation);
                        this.AddNodesAndSlots(nodeListForCreation, slotListForCreation);
                        deltaNodes.AppendToAddedNodes(nodeListForCreation);
                        break;

                    case UserAction.NodeDeletion:
                        //1. Retrieve nodeId from redoStorage and find the corresponding node from the dictionary
                        //2. Store the corresponding node and slot in the undoStorage
                        //3. Delete corresponding node and slot in dictionary
                        //4. Record the deleted node for LifeRunner
                        List<IVisualNode> nodeListForDeletion = this.RetrieveNodesFromIdsInStorage(this.redoStorage);
                        this.RecordNodesAndSlots(this.undoStorage, nodeListForDeletion, UserAction.NodeDeletion);
                        this.DeleteNodesAndSlots(nodeListForDeletion);
                        deltaNodes.AppendToRemovedNodes(nodeListForDeletion);
                        break;

                    case UserAction.NodeModification:
                        //1. Retrieve the slotId from redoStorage without creating the entire slot
                        //2. Retrieve the nodeId from redoStorage without creating the entire node
                        //3. Retrieve the corresponding node in the dictionary
                        //4. Store the corresponding node and slot in the undoStorage
                        //5. Resolve the, if any, the extra number of slot after redo
                        //6. Reset cursor position in stream to start of first header, ie, -shift value
                        //7. Update the corresponding node and slot in the dictionary through deserilaization 
                        //   from the redoStorage and resolve, if any, the missing slot after redo
                        //8. Record the modified node for LifeRunner
                        List<uint> slotIdList = this.RetrieveSlotIdsFromStorage(this.redoStorage);
                        List<uint> nodeIdList = this.RetrieveNodeIdsFromStorage(this.redoStorage);
                        List<IVisualNode> nodeList = this.RetrieveNodesFromDictionary(nodeIdList);
                        this.RecordNodesAndSlots(this.undoStorage, nodeList, UserAction.NodeModification);
                        this.ResolveExtraNumberOfSlotsAfterAction(nodeList, slotIdList);
                        this.redoStorage.Seek(-shift, SeekOrigin.Current);
                        this.UpdateNodesAndSlostWithResolveMissingSlotAfterAction(this.redoStorage, nodeList, slotIdList);
                        deltaNodes.AppendToModifiedNodes(nodeList);
                        break;

                    default:
                        throw new InvalidOperationException("Unknown UserAction");
                }

                //Set cursor for next redo
                this.redoStorage.Seek(-shift, SeekOrigin.Current);
            }

            //Upon Success
            this.undoStorage.WriteInteger(FieldCode.ActionCount, numOfActions);
            return true;
        }

        internal long GetUndoPosition()
        {
            return this.undoStorage.GetPosition();
        }

        #endregion

        #region Internal Group Related Methods

        internal void BeginGroup()
        {
            this.actionCount = 0;
            this.recordedComponents.Clear();
        }

        internal void EndGroup()
        {
            if (actionCount < 0)
                throw new InvalidOperationException("BeginGroup() is not called");
            if (actionCount > 0)
            {
                this.undoStorage.WriteInteger(FieldCode.ActionCount, this.actionCount);
                this.redoStorage.Seek(0, SeekOrigin.Begin); // Content of redo storage no longer relevant.
            }
            this.actionCount = -1;
            this.recordedComponents.Clear();
        }

        #endregion

        #region Internal Record Related Methods

        internal void RecordRuntimeStatesForUndo(RuntimeStates runtimeStates)
        {
            if (actionCount < 0)
                throw new InvalidOperationException("BeginGroup() is not called");

            if (runtimeStates == null)
                throw new ArgumentNullException("runtimeStates");

            this.RecordRuntimeStates(this.undoStorage, runtimeStates, UserAction.RuntimeStates);
            this.actionCount++;
        }

        internal void RecordEdgeCreationForUndo(List<IVisualEdge> edgeList)
        {
            if (actionCount < 0)
                throw new InvalidOperationException("BeginGroup() is not called");

            if (edgeList == null || edgeList.Count == 0)
                throw new ArgumentException("'edgeList' is null or empty");

            edgeList = this.RemoveRedundancy(edgeList);
            this.RecordEdgeIds(this.undoStorage, edgeList, UserAction.EdgeCreation);
            this.actionCount++;
        }

        internal void RecordEdgeDeletionForUndo(List<IVisualEdge> edgeList)
        {
            if (actionCount < 0)
                throw new InvalidOperationException("BeginGroup() is not called");

            if (edgeList == null || edgeList.Count == 0)
                throw new ArgumentException("'edgeList' is null or empty");

            edgeList = this.RemoveRedundancy(edgeList);
            this.RecordEdges(this.undoStorage, edgeList, UserAction.EdgeDeletion);
            this.actionCount++;
        }

        internal void RecordEdgeModificationForUndo(List<IVisualEdge> edgeList)
        {
            if (actionCount < 0)
                throw new InvalidOperationException("BeginGroup() is not called");

            if (edgeList == null || edgeList.Count == 0)
                throw new ArgumentException("'edgeList' is null or empty");

            edgeList = this.RemoveRedundancy(edgeList);
            this.RecordEdges(this.undoStorage, edgeList, UserAction.EdgeModification);
            this.actionCount++;
        }

        internal void RecordNodeCreationForUndo(List<IVisualNode> nodeList)
        {
            if (actionCount < 0)
                throw new InvalidOperationException("BeginGroup() is not called");

            if (nodeList == null || nodeList.Count == 0)
                throw new ArgumentException("'nodeList' is null or empty");

            nodeList = this.RemoveRedundancy(nodeList);
            this.RecordNodeIds(this.undoStorage, nodeList, UserAction.NodeCreation);
            this.actionCount++;
        }

        internal void RecordNodeDeletionForUndo(List<IVisualNode> nodeList)
        {
            if (actionCount < 0)
                throw new InvalidOperationException("BeginGroup() is not called");

            if (nodeList == null || nodeList.Count == 0)
                return;

            nodeList = this.RemoveRedundancy(nodeList);
            this.RecordNodesAndSlots(this.undoStorage, nodeList, UserAction.NodeDeletion);
            this.actionCount++;
        }

        internal void RecordNodeModificationForUndo(List<IVisualNode> nodeList)
        {
            if (actionCount < 0)
                throw new InvalidOperationException("BeginGroup() is not called");

            if (nodeList == null || nodeList.Count == 0)
                return;

            nodeList = this.RemoveRedundancy(nodeList);
            this.RecordNodesAndSlots(this.undoStorage, nodeList, UserAction.NodeModification);
            this.actionCount++;
        }

        internal void PopRecordFromUndoStorage()
        {
            if (this.undoStorage.GetPosition() <= 0)
                throw new InvalidOperationException("No record to remove");

            this.undoStorage.Seek(-12, SeekOrigin.Current);
            int numOfActions = this.undoStorage.ReadInteger(FieldCode.ActionCount);
            this.undoStorage.Seek(-12, SeekOrigin.Current);

            for (int i = 0; i < numOfActions; i++)
            {
                // Read back 28 bytes which comprise of the following:
                // 
                //      - FieldCode.Shift       (long, 8 bytes)
                //      - Offset distance       (long, 8 bytes)
                //      - FieldCode.UserAction  (long, 8 bytes)
                //      - UserAction value      (long, 4 bytes)
                // 
                this.undoStorage.Seek(-28, SeekOrigin.Current);
                long shift = this.undoStorage.ReadLong(FieldCode.Shift);
                this.undoStorage.Seek(-(shift + 16), SeekOrigin.Current);
            }
        }

        #endregion

        #region Private Record Related Methods

        private void RecordFlag(IStorage storage, long initialPosition, UserAction userAction)
        {
            //Record the number of bytes shifted
            long currentPosition = storage.GetPosition();
            long shift = currentPosition - initialPosition;
            storage.WriteLong(FieldCode.Shift, shift);
            storage.WriteInteger(FieldCode.UserAction, (int)userAction);
        }

        private void RecordRuntimeStates(IStorage storage, RuntimeStates runtimeStates, UserAction userAction)
        {
            DataHeader header = new DataHeader();
            long initialPosition = storage.GetPosition();

            //Record states
            storage.Seek(header.HeaderSize, SeekOrigin.Current);
            long initialPositionForData = storage.GetPosition();
            runtimeStates.Serialize(storage);
            long currentPositionforData = storage.GetPosition();
            header.DataSize = currentPositionforData - initialPositionForData;
            storage.Seek(-(header.HeaderSize + header.DataSize), SeekOrigin.Current);
            header.Serialize(storage);
            storage.Seek(header.DataSize, SeekOrigin.Current);

            this.RecordFlag(storage, initialPosition, userAction);
        }

        private void RecordEdgeIds(IStorage storage, List<IVisualEdge> edgeList, UserAction userAction)
        {
            DataHeader header = new DataHeader();
            header.DataSize = 12;
            long initialPosition = storage.GetPosition();

            //Record the number of edges and the edgeIds
            storage.WriteInteger(FieldCode.EdgeCount, edgeList.Count);
            foreach (IVisualEdge edge in edgeList)
            {
                header.Serialize(storage);
                storage.WriteUnsignedInteger(FieldCode.EdgeId, edge.EdgeId);
            }

            this.RecordFlag(storage, initialPosition, userAction);
        }

        private void RecordEdges(IStorage storage, List<IVisualEdge> edgeList, UserAction userAction)
        {
            DataHeader header = new DataHeader();
            long initialPostionForData, currentPositionForData = 0;
            long initialPosition = storage.GetPosition();

            //Record the number of edges and the edge itself
            storage.WriteInteger(FieldCode.EdgeCount, edgeList.Count);
            foreach (IVisualEdge edge in edgeList)
            {
                storage.Seek(header.HeaderSize, SeekOrigin.Current);
                initialPostionForData = storage.GetPosition();
                edge.Serialize(storage);
                currentPositionForData = storage.GetPosition();
                header.DataSize = currentPositionForData - initialPostionForData;
                storage.Seek(-(header.HeaderSize + header.DataSize), SeekOrigin.Current);
                header.Serialize(storage);
                storage.Seek(header.DataSize, SeekOrigin.Current);
            }

            this.RecordFlag(storage, initialPosition, userAction);
        }

        private void RecordNodeIds(IStorage storage, List<IVisualNode> nodeList, UserAction userAction)
        {
            DataHeader header = new DataHeader();
            header.DataSize = Configurations.UndoReDoDataHeaderSize;
            long initialPosition = storage.GetPosition();

            //Record the number of nodes and the nodeIds
            storage.WriteInteger(FieldCode.NodeCount, nodeList.Count);
            foreach (IVisualNode node in nodeList)
            {
                header.Serialize(storage);
                storage.WriteUnsignedInteger(FieldCode.NodeId, node.NodeId);
            }

            this.RecordFlag(storage, initialPosition, userAction);
        }

        private void RecordNodesAndSlots(IStorage storage, List<IVisualNode> nodeList, UserAction userAction)
        {
            DataHeader header = new DataHeader();
            long initialPostionForData, currentPositionForData = 0;
            long initialPosition = storage.GetPosition();

            //Retrieve all the slots Id in nodeList
            List<uint> allSlotsId = new List<uint>();
            foreach (IVisualNode node in nodeList)
            {
                if (node.GetInputSlots() != null)
                    allSlotsId.AddRange(node.GetInputSlots());
                if (node.GetOutputSlots() != null)
                    allSlotsId.AddRange(node.GetOutputSlots());
            }

            //Record the number of slots and the slot itself
            storage.WriteInteger(FieldCode.SlotCount, allSlotsId.Count());
            foreach (uint slotId in allSlotsId)
            {
                storage.Seek(header.HeaderSize, SeekOrigin.Current);
                initialPostionForData = storage.GetPosition();
                ISlot slot = this.graphController.GetSlot(slotId);
                slot.Serialize(storage);
                currentPositionForData = storage.GetPosition();
                header.DataSize = currentPositionForData - initialPostionForData;
                storage.Seek(-(header.HeaderSize + header.DataSize), SeekOrigin.Current);
                header.Serialize(storage);
                storage.Seek(header.DataSize, SeekOrigin.Current);
            }

            //Record the number of nodes and the node itself
            storage.WriteInteger(FieldCode.NodeCount, nodeList.Count);
            foreach (IVisualNode node in nodeList)
            {
                storage.Seek(header.HeaderSize, SeekOrigin.Current);
                initialPostionForData = storage.GetPosition();
                node.Serialize(storage);
                currentPositionForData = storage.GetPosition();
                header.DataSize = currentPositionForData - initialPostionForData;
                storage.Seek(-(header.HeaderSize + header.DataSize), SeekOrigin.Current);
                header.Serialize(storage);
                storage.Seek(header.DataSize, SeekOrigin.Current);
            }

            this.RecordFlag(storage, initialPosition, userAction);
        }

        #endregion

        #region Private Retrieve Related Methods

        private List<uint> RetrieveSlotIdsFromStorage(IStorage storage)
        {
            DataHeader header = new DataHeader();
            List<uint> slotIdList = new List<uint>();
            int slotCount = storage.ReadInteger(FieldCode.SlotCount);
            for (int i = 0; i < slotCount; i++)
            {
                header.Deserialize(storage);
                storage.Seek(36, SeekOrigin.Current);
                slotIdList.Add(storage.ReadUnsignedInteger(FieldCode.SlotId));
                storage.Seek(header.DataSize - 48, SeekOrigin.Current);
            }
            return slotIdList;
        }

        private List<uint> RetrieveEdgeIdsFromStorage(IStorage storage)
        {
            DataHeader header = new DataHeader();
            List<uint> EdgeIdList = new List<uint>();
            int edgeCount = storage.ReadInteger(FieldCode.EdgeCount);
            for (int i = 0; i < edgeCount; i++)
            {
                header.Deserialize(storage);
                storage.Seek(36, SeekOrigin.Current);
                EdgeIdList.Add(storage.ReadUnsignedInteger(FieldCode.EdgeId));
                storage.Seek(header.DataSize - 48, SeekOrigin.Current);
            }
            return EdgeIdList;
        }

        private List<uint> RetrieveNodeIdsFromStorage(IStorage storage)
        {
            DataHeader header = new DataHeader();
            List<uint> nodeIdList = new List<uint>();
            int nodeCount = storage.ReadInteger(FieldCode.NodeCount);
            for (int i = 0; i < nodeCount; i++)
            {
                header.Deserialize(storage);
                storage.Seek(36, SeekOrigin.Current);
                nodeIdList.Add(storage.ReadUnsignedInteger(FieldCode.NodeId));
                storage.Seek(header.DataSize - 48, SeekOrigin.Current);
            }
            return nodeIdList;
        }

        private List<ISlot> RetrieveSlotsFromStorage(IStorage storage)
        {
            DataHeader header = new DataHeader();
            List<ISlot> slotList = new List<ISlot>();
            int slotCount = storage.ReadInteger(FieldCode.SlotCount);
            for (int i = 0; i < slotCount; i++)
            {
                header.Deserialize(storage);
                ISlot slot = Slot.Create(this.graphController, storage);
                slotList.Add(slot);
            }
            return slotList;
        }

        private List<IVisualEdge> RetrieveEdgesFromStorage(IStorage storage)
        {
            DataHeader header = new DataHeader();
            List<IVisualEdge> edgeList = new List<IVisualEdge>();
            int edgeCount = storage.ReadInteger(FieldCode.EdgeCount);
            for (int i = 0; i < edgeCount; i++)
            {
                header.Deserialize(storage);
                IVisualEdge edge = this.graphController.CreateEdgeFromStorage(storage);
                edgeList.Add(edge);
            }
            return edgeList;
        }

        private List<IVisualNode> RetrieveNodesFromStorage(IStorage storage)
        {
            DataHeader header = new DataHeader();
            List<IVisualNode> nodeList = new List<IVisualNode>();
            int nodeCount = storage.ReadInteger(FieldCode.NodeCount);
            for (int i = 0; i < nodeCount; i++)
            {
                header.Deserialize(storage);
                IVisualNode node = VisualNode.Create(this.graphController, storage);
                nodeList.Add(node);
            }
            return nodeList;
        }

        private List<IVisualEdge> RetrieveEdgesFromIdsInStorage(IStorage storage)
        {
            DataHeader header = new DataHeader();
            List<IVisualEdge> edgeList = new List<IVisualEdge>();
            int edgeCount = storage.ReadInteger(FieldCode.EdgeCount);
            for (int i = 0; i < edgeCount; i++)
            {
                header.Deserialize(storage);
                uint edgeId = storage.ReadUnsignedInteger(FieldCode.EdgeId);
                edgeList.Add(this.graphController.GetVisualEdge(edgeId));
            }
            return edgeList;
        }

        private List<IVisualNode> RetrieveNodesFromIdsInStorage(IStorage storage)
        {
            DataHeader header = new DataHeader();
            List<IVisualNode> nodeList = new List<IVisualNode>();
            int nodeCount = storage.ReadInteger(FieldCode.NodeCount);
            for (int i = 0; i < nodeCount; i++)
            {
                header.Deserialize(storage);
                uint nodeId = storage.ReadUnsignedInteger(FieldCode.NodeId);
                nodeList.Add(this.graphController.GetVisualNode(nodeId));
            }
            return nodeList;
        }

        private List<IVisualEdge> RetrieveEdgesFromDictionary(List<uint> edgeIdList)
        {
            List<IVisualEdge> edgeList = new List<IVisualEdge>();
            foreach (uint edgeId in edgeIdList)
                edgeList.Add(this.graphController.GetVisualEdge(edgeId));
            return edgeList;
        }

        private List<IVisualNode> RetrieveNodesFromDictionary(List<uint> nodeIdList)
        {
            List<IVisualNode> nodeList = new List<IVisualNode>();
            foreach (uint nodeId in nodeIdList)
                nodeList.Add(this.graphController.GetVisualNode(nodeId));
            return nodeList;
        }

        #endregion

        #region Private Object Manipulation Related Methods

        private void DeleteEdges(List<IVisualEdge> edgeList)
        {
            //Delete the edge's visual drawing and edge from dictionary
            foreach (IVisualEdge edge in edgeList)
            {
                this.graphController.RemoveDrawingVisual(edge.EdgeId);
                this.graphController.RemoveVisualEdge(edge.EdgeId);
            }
        }

        private void DeleteNodesAndSlots(List<IVisualNode> nodeList)
        {
            foreach (IVisualNode node in nodeList)
            {
                //Reset node's error
                ((VisualNode)node).SetErrorValue(null, ErrorType.None);

                //Delete the node's drawing visual and the bubble's drawing visual
                this.graphController.RemoveDrawingVisual(node.NodeId);
                this.graphController.RemoveDrawingVisual(((VisualNode)node).GetErrorBubbleId());
                this.graphController.RemoveDrawingVisual(((VisualNode)node).GetPreviewBubbleId());
                this.graphController.RemoveExtendedBubble(((VisualNode)node).GetPreviewBubbleId());

                //Delete node's input slots
                if (node.GetInputSlots() != null)
                {
                    List<uint> inputSlotIdList = new List<uint>(node.GetInputSlots());
                    foreach (uint slotId in inputSlotIdList)
                        this.graphController.RemoveSlot(slotId);
                }

                //Delete node's output slots
                if (node.GetOutputSlots() != null)
                {
                    List<uint> outputSlotIdList = new List<uint>(node.GetOutputSlots());
                    foreach (uint slotId in outputSlotIdList)
                        this.graphController.RemoveSlot(slotId);
                }

                //Delete the node and bubbles from dictionary
                this.graphController.RemoveVisualNode(node.NodeId);
                this.graphController.RemoveBubble(((VisualNode)node).GetErrorBubbleId());
                this.graphController.RemoveBubble(((VisualNode)node).GetPreviewBubbleId());
            }
        }

        private void AddEdges(List<IVisualEdge> edgeList)
        {
            //Add edges in edgeCollection
            foreach (IVisualEdge edge in edgeList)
                this.graphController.AddVisualEdge(edge);
        }

        private void AddNodesAndSlots(List<IVisualNode> nodeList, List<ISlot> slotList)
        {
            //Add slots in slotCollection
            foreach (ISlot slot in slotList)
                this.graphController.AddSlot(slot);

            //Add nodes in nodeCollection
            foreach (IVisualNode node in nodeList)
                this.graphController.AddVisualNode(node);
        }

        private void ResolveExtraNumberOfSlotsAfterAction(List<IVisualNode> nodeList, List<uint> slotIdListAfterAction)
        {
            List<uint> slotIdListBeforeAction = new List<uint>();
            foreach (IVisualNode node in nodeList)
            {
                if (node.GetInputSlots() != null)
                    slotIdListBeforeAction.AddRange(node.GetInputSlots());
                if (node.GetOutputSlots() != null)
                    slotIdListBeforeAction.AddRange(node.GetOutputSlots());
            }
            List<uint> slotIdList = slotIdListBeforeAction.Except(slotIdListAfterAction).ToList<uint>();
            foreach (uint slotId in slotIdList)
            {
                if (this.graphController.ContainSlotKey(slotId))
                    this.graphController.RemoveSlot(slotId);
            }
        }

        private void UpdateEdges(IStorage storage, List<IVisualEdge> edgeList)
        {
            DataHeader header = new DataHeader();
            storage.Seek(12, SeekOrigin.Current); //By-pass the reading of total number of edges stored
            foreach (IVisualEdge edge in edgeList)
            {
                header.Deserialize(storage);
                edge.Deserialize(storage);
            }
        }

        private void UpdateRuntimeStates(IStorage storage, RuntimeStates runtimeStates)
        {
            DataHeader header = new DataHeader();
            header.Deserialize(storage);
            runtimeStates.Deserialize(storage);
        }

        private void UpdateNodesAndSlostWithResolveMissingSlotAfterAction(IStorage storage, List<IVisualNode> nodeList, List<uint> slotIdList)
        {
            DataHeader header = new DataHeader();
            storage.Seek(12, SeekOrigin.Current); //By-pass the reading of total number of slots stored
            foreach (uint slotId in slotIdList)
            {
                header.Deserialize(storage);
                if (this.graphController.ContainSlotKey(slotId))
                {
                    ISlot slot = this.graphController.GetSlot(slotId);
                    slot.Deserialize(storage);
                }
                else
                {
                    ISlot slot = Slot.Create(this.graphController, storage);
                    this.graphController.AddSlot(slot);
                }
            }
            storage.Seek(12, SeekOrigin.Current); //By-pass the reading of total number of nodes stored
            foreach (IVisualNode node in nodeList)
            {
                header.Deserialize(storage);
                node.Deserialize(storage);
            }
        }

        #endregion

        #region Private Redundancy Related Methods

        private List<IVisualNode> RemoveRedundancy(List<IVisualNode> nodeList)
        {
            List<IVisualNode> checkedNodeList = new List<IVisualNode>();
            foreach (IVisualNode node in nodeList)
            {
                if (this.recordedComponents.Contains(node.NodeId))
                    continue;
                checkedNodeList.Add(node);
                this.recordedComponents.Add(node.NodeId);
            }
            return checkedNodeList;
        }

        private List<IVisualEdge> RemoveRedundancy(List<IVisualEdge> edgeList)
        {
            List<IVisualEdge> checkedEdgeList = new List<IVisualEdge>();
            foreach (IVisualEdge edge in edgeList)
            {
                if (this.recordedComponents.Contains(edge.EdgeId))
                    continue;
                checkedEdgeList.Add(edge);
                this.recordedComponents.Add(edge.EdgeId);
            }
            return checkedEdgeList;
        }

        #endregion
    }
}
