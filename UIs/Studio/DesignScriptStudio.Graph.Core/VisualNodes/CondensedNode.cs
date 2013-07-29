using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Globalization;

namespace DesignScriptStudio.Graph.Core
{
    class CondensedNode : VisualNode
    {
        #region Class Data Members

        private List<uint> embeddedNodes = new List<uint>();

        #endregion

        #region Public Interface Methods

        public override bool Deserialize(IStorage storage)
        {
            if (storage == null)
                throw new ArgumentNullException("storage");

            try
            {
                base.Deserialize(storage);

                int embeddedNodesCount = storage.ReadInteger(FieldCode.EmbeddedNodesCount);
                this.embeddedNodes.Clear();
                for (int i = 0; i < embeddedNodesCount; i++)
                    this.embeddedNodes.Add(storage.ReadUnsignedInteger(FieldCode.EmbeddedNode));

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n Condensed node deserialization failed.");
                return false;
            }
        }

        public override bool Serialize(IStorage storage)
        {
            if (storage == null)
                throw new ArgumentNullException("storage");

            try
            {
                base.Serialize(storage);

                storage.WriteInteger(FieldCode.EmbeddedNodesCount, this.embeddedNodes.Count);
                foreach (uint i in this.embeddedNodes)
                    storage.WriteUnsignedInteger(FieldCode.EmbeddedNode, i);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n Condensed node serialization failed.");
                return false;
            }
        }

        #endregion
        
        #region Internal Class Methods

        internal CondensedNode(IGraphController graphController, string text)
            : base(graphController, NodeType.Condensed)
        {
            this.Text = text;
            this.Caption = string.Empty;
        }

        internal CondensedNode(IGraphController graphController) : base(graphController) { }

        internal override string GetSlotName(SlotType slotType, uint slotId)
        {
            throw new NotImplementedException("Condensed node should not implement GetSlotName()");
        }

        internal override List<uint> GetSlotIdsByName(SlotType slotType, string variableName)
        {
            throw new NotImplementedException("Condensed node should not implement GetSlotIdByName()");
        }

        internal override List<string>  GetDefinedVariables(bool includeTemporaryVariables)
        {
            return null;
        }

        internal override List<string> GetReferencedVariables(bool includeTemporaryVariables)
        {
            return null;
        }

        internal override string ToCode(out GraphToDSCompiler.SnapshotNodeType type)
        {
            throw new NotImplementedException("Condensed node should not implement ToCode()");
        }

        internal override string PreprocessInputString(string input)
        {
            throw new NotImplementedException("Condensed node should not implement PreprocessInputString()");
        }

        internal override ErrorType ValidateInputString(out string error)
        {
            throw new NotImplementedException("Condensed node should not implement ValidateinputString()");
        }

        internal override NodePart HitTest(double x, double y, out int index)
        {
            index = -1;
            return NodePart.None;
        }

        #endregion
    }
}
