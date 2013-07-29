using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DesignScriptStudio.Graph.Core
{
    public class IdGenerator
    {
        private uint nodeId = 0;
        private uint slotId = 0;
        private uint edgeId = 0;
        private uint bubbleId = 0;

        public static ComponentType GetType(uint id)
        {
            uint type = 0xff000000 & id;
            switch ((ComponentType)type)
            {
                case ComponentType.Node: return ComponentType.Node;
                case ComponentType.Slot: return ComponentType.Slot;
                case ComponentType.Edge: return ComponentType.Edge;
                case ComponentType.Bubble: return ComponentType.Bubble;
            }

            return ComponentType.None;
        }

        internal void SetStartId(uint id)
        {
            switch (GetType(id))
            {
                case ComponentType.Node: this.nodeId = Unmask(id); return;
                case ComponentType.Slot: this.slotId = Unmask(id); return;
                case ComponentType.Edge: this.edgeId = Unmask(id); return;
                case ComponentType.Bubble: this.bubbleId = Unmask(id); return;
            }

            throw new ArgumentException("Invalid argument 'id'");
        }

        internal uint GetNextId(ComponentType type)
        {
            uint marker = ((uint)type);
            switch (type)
            {
                case ComponentType.Node: return (marker | (++nodeId));
                case ComponentType.Slot: return (marker | (++slotId));
                case ComponentType.Edge: return (marker | (++edgeId));
                case ComponentType.Bubble: return (marker | (++bubbleId));
            }

            throw new ArgumentException("Invalid argument 'type'");
        }

        internal static uint Unmask(uint id)
        {
            return ((0x00ffffff) & id);
        }
    }
}
