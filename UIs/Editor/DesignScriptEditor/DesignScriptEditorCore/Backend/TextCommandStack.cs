using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DesignScript.Editor.Core
{
    internal class Modification
    {
        public enum Action { None, Insert, Remove };

        #region Public Class Properties

        public Action ActionType { get; private set; }
        public int StartLine { get; private set; }
        public int NewLinesCount { get; private set; }
        public List<string> OriginalContents { get; private set; }
        public System.Drawing.Point CursorPosition { get; private set; }

        public bool IsInsertion { get { return (this.ActionType == Action.Insert); } }
        public bool IsRemoval { get { return (this.ActionType == Action.Remove); } }

        # endregion

        public Modification(int startLine, int newLinesCount, List<string> originalContents, Action action, System.Drawing.Point cursorPosition)
        {
            this.StartLine = startLine;
            this.NewLinesCount = newLinesCount;
            this.OriginalContents = originalContents;
            this.ActionType = action;
            CursorPosition = cursorPosition;
        }
    }

    internal class ModificationGroup
    {
        private List<Modification> modifications = null;

        #region Public Class Operational Methods

        internal ModificationGroup()
        {
            modifications = new List<Modification>();
            RedoCaretPosition = new System.Drawing.Point();
        }

        internal void Add(Modification modification)
        {
            this.modifications.Add(modification);
        }

        internal void ReverseModifications()
        {
            this.modifications.Reverse();
        }

        #endregion

        #region Public State Query Methods

        internal Modification GetLastModification()
        {
            if (null == modifications || (modifications.Count <= 0))
                return null;

            return modifications[modifications.Count - 1];
        }

        internal Modification GetSecondLastModification()
        {
            if (null == modifications || (modifications.Count <= 1))
                return null;

            return modifications[modifications.Count - 2];
        }

        #endregion

        #region Public Class Properties

        internal int ModificationCount
        {
            get { return modifications.Count; }
        }

        internal List<Modification> List
        {
            get { return modifications; }
        }

        internal System.Drawing.Point RedoCaretPosition { get; set; }

        #endregion
    }
}
