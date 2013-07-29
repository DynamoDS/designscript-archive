using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace DesignScript.Editor.Core
{
    internal class UndoRedoRecorder
    {
        private List<string> textBufferLineList = null;
        private List<ModificationGroup> undoStack = new List<ModificationGroup>();
        private List<ModificationGroup> redoStack = new List<ModificationGroup>();
        private ModificationGroup currentUndoGroup = null;
        private ModificationGroup currentRedoGroup = null;

        #region Public Class Operational Methods

        internal UndoRedoRecorder(List<string> textBufferLineList)
        {
            this.textBufferLineList = textBufferLineList;
        }

        internal void RecordInsertion(int lineInsertedAt, int newLineCount, List<string> originalContents, Point cursorPosition)
        {
            redoStack.Clear();
            RecordInsertionInternal(lineInsertedAt, newLineCount, originalContents, cursorPosition);
        }

        internal void RecordRemoval(int lineRemovedAt, int newLineCount, List<string> originalContents, Point cursorPosition)
        {
            redoStack.Clear();
            RecordRemovalInternal(lineRemovedAt, newLineCount, originalContents, cursorPosition);
        }

        internal void Undo(ref Point cursorPosition)
        {
            CloseUndoGroup();
            if (undoStack.Count == 0)
                return;

            ModificationGroup topMostGroup = undoStack[undoStack.Count - 1];
            topMostGroup.ReverseModifications();

            foreach (Modification modification in topMostGroup.List)
                UndoModification(modification, ref cursorPosition);

            // Redo group is always closed immediately.
            if (null != currentRedoGroup)
            {
                redoStack.Add(currentRedoGroup);
                currentRedoGroup = null;
            }

            undoStack.RemoveAt(undoStack.Count - 1);
        }

        internal void Redo(ref Point cursorPosition)
        {
            if (redoStack.Count == 0)
                return;

            CloseUndoGroup();

            ModificationGroup topMostGroup = redoStack[redoStack.Count - 1];
            topMostGroup.ReverseModifications();

            foreach (Modification modification in topMostGroup.List)
                RedoModification(modification, ref cursorPosition);

            CloseUndoGroup();
            redoStack.RemoveAt(redoStack.Count - 1);
        }

        internal void CloseUndoGroup()
        {
            if (null != currentUndoGroup)
            {
                undoStack.Add(currentUndoGroup);
                currentUndoGroup = null;
            }
        }

        #endregion

        #region Public State Query Methods

        internal ModificationGroup GetTopMostUndoGroup()
        {
            if (null != currentUndoGroup)
                return currentUndoGroup;

            if (undoStack.Count <= 0)
                return null;
            return (undoStack[undoStack.Count - 1]);
        }

        internal ModificationGroup GetTopMostRedoGroup()
        {
            if (null != currentRedoGroup)
                return currentRedoGroup;

            if (redoStack.Count <= 0)
                return null;
            return (redoStack[redoStack.Count - 1]);
        }

        #endregion

        #region Private Class Helper Methods

        private void RecordInsertionInternal(int lineInsertedAt, int newLineCount, List<string> originalContents, Point cursorPosition)
        {
            if (currentUndoGroup == null)
                currentUndoGroup = new ModificationGroup();

            if (currentUndoGroup.ModificationCount == 1)
            {
                Modification lastModification = currentUndoGroup.GetLastModification();
                if (null != lastModification && (false != lastModification.IsInsertion) && lastModification.StartLine == lineInsertedAt)
                    return; // Insertions after another insertion are ignored.
            }

            currentUndoGroup.Add(new Modification(lineInsertedAt, newLineCount,
                originalContents, Modification.Action.Insert, cursorPosition));
        }

        private void RecordRemovalInternal(int lineRemovedAt, int lineCount, List<string> originalContents, Point cursorPosition)
        {
            if (currentUndoGroup == null)
                currentUndoGroup = new ModificationGroup();
            else
            {
                Modification lastModification = currentUndoGroup.GetLastModification();
                if (null != lastModification)
                {
                    if (false != lastModification.IsRemoval)
                        return;

                    if (false != lastModification.IsInsertion)
                    {
                        CloseUndoGroup();
                        currentUndoGroup = new ModificationGroup();
                    }
                }
            }

            currentUndoGroup.Add(new Modification(lineRemovedAt, lineCount,
                originalContents, Modification.Action.Remove, cursorPosition));
        }

        #endregion

        #region Private Class Helper Methods

        private void UndoModification(Modification modification, ref Point cursorPosition)
        {
            int lineCountToRemove = modification.NewLinesCount;
            if (modification.StartLine + lineCountToRemove > textBufferLineList.Count)
                lineCountToRemove = textBufferLineList.Count - modification.StartLine;

            List<string> linesToRemove = textBufferLineList.GetRange(modification.StartLine, lineCountToRemove);

            if (modification.ActionType == Modification.Action.Insert)
            {
                AddInsertionToRedoGroup(modification.StartLine,
                    modification.OriginalContents.Count, linesToRemove, cursorPosition);
            }
            else if (modification.ActionType == Modification.Action.Remove)
            {
                AddRemoveToRedoGroup(modification.StartLine,
                    modification.OriginalContents.Count, linesToRemove, cursorPosition);
            }

            cursorPosition = modification.CursorPosition;
            textBufferLineList.RemoveRange(modification.StartLine, lineCountToRemove);
            textBufferLineList.InsertRange(modification.StartLine, modification.OriginalContents);
        }

        private void AddInsertionToRedoGroup(int lineInsertedAt, int newLineCount, List<string> originalContents, Point cursorPosition)
        {
            if (currentRedoGroup == null)
                currentRedoGroup = new ModificationGroup();

            currentRedoGroup.Add(new Modification(lineInsertedAt, newLineCount,
                originalContents, Modification.Action.Insert, cursorPosition));
        }

        private void AddRemoveToRedoGroup(int lineInsertedAt, int newLineCount, List<string> originalContents, Point cursorPosition)
        {
            if (currentRedoGroup == null)
                currentRedoGroup = new ModificationGroup();

            currentRedoGroup.Add(new Modification(lineInsertedAt, newLineCount,
                originalContents, Modification.Action.Remove, cursorPosition));
        }

        private void RedoModification(Modification modification, ref Point cursorPosition)
        {
            // If modification is to be done on the line beyond the last, then we are essentially 
            // dealing with a non-existence line. In such cases, the "original line contents" (the
            // thing we would like to backup on the undo group) will be an empty line.
            // 
            bool modificationBeyondFinalLine = (textBufferLineList.Count == modification.StartLine);

            List<string> originalContents = null;
            if (false != modificationBeyondFinalLine)
            {
                originalContents = new List<string>();
                originalContents.Add(string.Empty);
            }
            else
            {
                // Before we redo the changes, there will be the "original contents" that we 
                // would want to backup in the undo group. Store them somewhere safe and then 
                // remove the lines from the line list.
                // 
                originalContents = textBufferLineList.GetRange(modification.StartLine, modification.NewLinesCount);
            }

            if (modification.ActionType == Modification.Action.Insert)
            {
                RecordInsertionInternal(modification.StartLine,
                    modification.OriginalContents.Count, originalContents, cursorPosition);
            }
            else if (modification.ActionType == Modification.Action.Remove)
            {
                RecordRemovalInternal(modification.StartLine,
                    modification.OriginalContents.Count, originalContents, cursorPosition);
            }

            if (false == modificationBeyondFinalLine)
                textBufferLineList.RemoveRange(modification.StartLine, modification.NewLinesCount);

            textBufferLineList.InsertRange(modification.StartLine, modification.OriginalContents);
            cursorPosition = modification.CursorPosition;
        }

        #endregion

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(string.Format("Line list size: {0}, ", textBufferLineList.Count));
            builder.Append(string.Format("Undo stack size: {0}, ", undoStack.Count));
            builder.Append(string.Format("Redo stack size: {0} ", redoStack.Count));
            return builder.ToString();
        }
    }
}
