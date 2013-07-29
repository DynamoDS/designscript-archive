using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DesignScriptStudio.Graph.Core
{
    struct VariableSlotInfo
    {
        #region Struct Public/Internal Methods

        internal VariableSlotInfo(string variable, int line, uint slotId)
        {
            if (line < 0)
                throw new ArgumentException("Line cannot be negative (5CAB220BD538)", "line");
            if (string.IsNullOrEmpty(variable))
                throw new ArgumentException("Invalid variable name (659245F53898)", "variable");

            this.variable = variable;
            this.line = line;
            this.slotId = slotId;
        }

        public static bool operator ==(VariableSlotInfo variableLineSlot1, VariableSlotInfo variableLineSlot2)
        {
            return variableLineSlot1.Equals(variableLineSlot2);
        }

        public static bool operator !=(VariableSlotInfo variableLineSlot1, VariableSlotInfo variableLineSlot2)
        {
            return !variableLineSlot1.Equals(variableLineSlot2);
        }

        public override bool Equals(object obj)
        {
            if (obj is VariableSlotInfo)
            {
                return this.Equals((VariableSlotInfo)obj);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return variable.GetHashCode() ^ line ^ (int)slotId;
        }

        #endregion

        #region Struct Private Methods

        private bool Equals(VariableSlotInfo other)
        {
            return (this.variable == other.variable) && (this.line == other.line) && (this.slotId == other.slotId);
        }

        #endregion

        #region Struct Public Properties

        internal string Variable { get { return variable; } }

        internal int Line { get { return line; } }

        internal uint SlotId { get { return slotId; } }

        #endregion

        #region Struct Data Member

        private string variable;
        private int line;
        private uint slotId;

        #endregion
    }
}
