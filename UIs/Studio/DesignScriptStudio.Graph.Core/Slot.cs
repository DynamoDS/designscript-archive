using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;
using ProtoCore.Utils;

namespace DesignScriptStudio.Graph.Core
{
    sealed class Slot : ISlot
    {
        private enum Version { Version0, Current = Version0 }

        #region Class Data Members

        private GraphController graphController = null;
        private SlotType slotType = SlotType.None;
        private Slot.Version version;
        private uint slotId;
        private SlotStates slotState = SlotStates.None;
        private List<uint> owners = new List<uint>();
        private List<uint> connectingSlots = new List<uint>();

        #endregion

        #region  Public Interface Methods

        /// <summary>
        /// ID of this slot
        /// </summary>
        public uint SlotId
        {
            get { return slotId; }
        }

        public SlotType SlotType
        {
            get { return slotType; }
        }

        public SlotStates States
        {
            get { return slotState; }
        }

        /// <summary>
        /// List of IDs of Nodes that own this slot
        /// </summary>
        public uint[] Owners
        {
            get
            {
                if (owners.Count <= 0)
                    return null;

                uint[] list = new uint[owners.Count];
                owners.CopyTo(list);
                return list;
            }
        }

        /// <summary>
        /// List of Slots connected to this slot
        /// </summary>
        public uint[] ConnectingSlots
        {
            get
            {
                if (connectingSlots.Count <= 0)
                    return null;

                uint[] list = new uint[connectingSlots.Count];
                connectingSlots.CopyTo(list);
                return list;
            }
        }


        public bool Deserialize(IStorage storage)
        {
            if (storage == null)
                throw new ArgumentNullException("storage");

            if (storage.ReadUnsignedInteger(FieldCode.SlotSignature) != Configurations.SlotSignature)
                throw new InvalidOperationException("Invalid input data {9D939DA2}");

            try
            {
                this.slotType = (SlotType)storage.ReadInteger(FieldCode.SlotType);
                this.version = (Slot.Version)storage.ReadInteger(FieldCode.SlotVersion);
                this.slotId = storage.ReadUnsignedInteger(FieldCode.SlotId);
                this.slotState = (SlotStates)storage.ReadInteger(FieldCode.SlotState);

                int ownersCount = storage.ReadInteger(FieldCode.OwnersCount);
                this.owners.Clear();
                for (int i = 0; i < ownersCount; i++)
                    owners.Add(storage.ReadUnsignedInteger(FieldCode.Owners));

                int connectingSlotsCount = storage.ReadInteger(FieldCode.ConnectingSlotsCount);
                this.connectingSlots.Clear();
                for (int i = 0; i < connectingSlotsCount; i++)
                    connectingSlots.Add(storage.ReadUnsignedInteger(FieldCode.ConnectingSlots));

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n Slot deserialization failed.");
                return false;
            }
        }

        public bool Serialize(IStorage storage)
        {
            if (storage == null)
                throw new ArgumentNullException("storage");

            try
            {
                storage.WriteUnsignedInteger(FieldCode.SlotSignature, Configurations.SlotSignature);
                storage.WriteInteger(FieldCode.SlotType, (int)this.slotType);
                storage.WriteInteger(FieldCode.SlotVersion, (int)Slot.Version.Current);
                storage.WriteUnsignedInteger(FieldCode.SlotId, this.slotId);
                storage.WriteInteger(FieldCode.SlotState, (int)this.slotState);

                storage.WriteInteger(FieldCode.OwnersCount, this.owners.Count);
                foreach (uint i in this.owners)
                    storage.WriteUnsignedInteger(FieldCode.Owners, i);

                storage.WriteInteger(FieldCode.ConnectingSlotsCount, this.connectingSlots.Count);
                foreach (uint j in this.connectingSlots)
                    storage.WriteUnsignedInteger(FieldCode.ConnectingSlots, j);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n Slot serialization failed.");
                return false;
            }
        }

        public AuditStatus Audit()
        {
            return AuditStatus.NoChange;
        }

        #endregion

        #region Public Class Methods

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("------------------------------------------------\n");
            builder.Append(string.Format("Slot Id: 0x{0} ({1})\n", slotId.ToString("x8"), slotType.ToString()));
            builder.Append(string.Format("Visible: {0}\n", slotState.HasFlag(SlotStates.Visible)));

            if (owners.Count > 0)
            {
                builder.Append("Owners: ");
                foreach (uint owner in owners)
                    builder.Append(string.Format("0x{0} ", owner.ToString("x8")));
                builder.Append("\n");
            }
            else
            {
                builder.Append("Owners: <none>\n");
            }

            if (connectingSlots.Count > 0)
            {
                builder.Append("Connecting slots: ");
                foreach (uint slot in connectingSlots)
                    builder.Append(string.Format("0x{0} ", slot.ToString("x8")));
                builder.Append("\n");
            }
            else
            {
                builder.Append("Connecting slots: <none>\n");
            }

            return builder.ToString();
        }

        #endregion

        #region Internal Class Properties

        internal bool Visible
        {
            get { return this.slotState.HasFlag(SlotStates.Visible); }
            set
            {
                if (value)
                {
                    if (this.slotState.HasFlag(SlotStates.Visible))
                        return;

                    this.slotState |= SlotStates.Visible;
                }
                else
                {
                    if (!this.slotState.HasFlag(SlotStates.Visible))
                        return;

                    this.slotState &= ~SlotStates.Visible;
                }
            }
        }

        internal bool Connected
        {
            get { return (this.connectingSlots.Count > 0); }
        }

        #endregion

        #region Internal Class Methods

        /// <summary>
        /// Create an instance of a slot given storage
        /// </summary>
        /// <param name="graphController"></param>
        /// <param name="storage"></param>
        /// <returns></returns>
        internal static ISlot Create(IGraphController graphController, IStorage storage)
        {
            if (graphController == null || storage == null)
                throw new ArgumentNullException("graphController, storage");

            ISlot slot = new Slot(graphController);
            slot.Deserialize(storage);
            return slot;
        }

        internal static void Connect(Slot start, Slot end)
        {
            if (start == null)
                throw new ArgumentNullException("start");
            if (end == null)
                throw new ArgumentNullException("end");

            start.AddConnectingSlot(end.slotId);
            end.AddConnectingSlot(start.slotId);
        }

        internal static void Disconnect(Slot start, Slot end)
        {
            if (start == null)
                throw new ArgumentNullException("start");
            if (end == null)
                throw new ArgumentNullException("end");

            start.RemoveConnectingSlot(end.slotId);
            end.RemoveConnectingSlot(start.slotId);
        }

        internal static bool IsConnected(Slot start, Slot end)
        {
            if (start == null)
                throw new ArgumentNullException("start");
            if (end == null)
                throw new ArgumentNullException("end");

            List<uint> startConnectingSlots = new List<uint>();
            if (start.ConnectingSlots != null)
                startConnectingSlots.AddRange(start.ConnectingSlots);

            List<uint> endConnectingSlots = new List<uint>();
            if (end.ConnectingSlots != null)
                endConnectingSlots.AddRange(end.ConnectingSlots);


            if (startConnectingSlots.Contains(end.SlotId) &&
                endConnectingSlots.Contains(start.SlotId))
                return true;
            return false;
        }

        /// <summary>
        /// Create a new slot
        /// </summary>
        /// <param name="gc">Owning graph controller</param>
        /// <param name="type">Input/Output slot</param>
        /// <param name="firstOwner">Node where there slot resides</param>
        internal Slot(IGraphController gc, SlotType type, IVisualNode firstOwner)
        {
            if (gc == null || firstOwner == null)
                throw new ArgumentNullException();

            this.graphController = gc as GraphController;
            Validity.Assert(this.graphController != null);
            IdGenerator idGenerator = this.graphController.GetIdGenerator();

            this.slotType = type;
            this.version = Slot.Version.Current;
            this.slotId = idGenerator.GetNextId(ComponentType.Slot);
            this.slotState = SlotStates.Visible;
            this.owners.Add(firstOwner.NodeId);
        }

        /// <summary>
        /// Get the top left corner of the slot relative to the canvas
        /// This is for the last owner
        /// </summary>
        /// <returns></returns>
        internal Point GetPosition()
        {
            //@TODO(Victor): Get this magic string out'ta here, GUIDify it
            if (owners.Count <= 0)
                throw new InvalidOperationException("'owners' cannot be empty or null");

            uint owner = owners.Last();
            //search for the owner using the id

            IVisualNode node = this.graphController.GetVisualNode(owner);
            return ((VisualNode)node).GetSlotPosition(this);
        }

        internal string GetSlotName()
        {
            if (this.owners.Count() <= 0)
                throw new InvalidDataException("'owners' cannot be empty or null");

            uint nodeId = this.owners[0];
            VisualNode node = this.graphController.GetVisualNode(nodeId);
            return node.GetSlotName(this.slotType, this.slotId);
        }

        #endregion

        #region Private Class Methods

        private Slot(IGraphController graphController)
        {
            this.graphController = graphController as GraphController;
        }

        /// <summary>
        /// Add a new owner
        /// </summary>
        /// <param name="newOwner"></param>
        private void PushOwner(IVisualNode newOwner)
        {
            owners.Add(newOwner.NodeId);
        }

        /// <summary>
        /// Remove last owner
        /// </summary>
        /// <returns></returns>
        private uint PopOwner()
        {
            if (owners.Count == 1)
                throw new InvalidOperationException();

            uint lastOwner = owners[owners.Count - 1];
            owners.RemoveAt(owners.Count - 1);
            return lastOwner;
        }

        private void PushConnectingSlot(uint slotId)
        {
            foreach (uint id in connectingSlots)
            {
                if (slotId == id)
                    throw new InvalidOperationException(); // slot already exists
            }

            connectingSlots.Add(slotId);
        }

        private void AddConnectingSlot(uint slotId)
        {
            this.connectingSlots.Add(slotId);
        }

        private void RemoveConnectingSlot(uint slotId)
        {
            if (!this.connectingSlots.Contains(slotId))
                throw new InvalidOperationException("'slotId' not in 'connectingSlots'");

            this.connectingSlots.Remove(slotId);
        }

        #endregion
    }
}
