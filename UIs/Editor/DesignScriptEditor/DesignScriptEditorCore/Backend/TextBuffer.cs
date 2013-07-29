using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media;
using System.Windows;

namespace DesignScript.Editor.Core
{
    using DesignScript.Parser;
    using System.Drawing;
    using System.Text.RegularExpressions;

    public delegate void ParsePendingChangedHandler(
        object sender, ParsePendingChangedEventArgs e);

    public delegate void ScriptModifiedHandler(
        object sender, EventArgs e);

    public delegate void LinesUpdatedHandler(
        object sender, LinesUpdateEventArgs e);

    public class ParsePendingChangedEventArgs : EventArgs
    {
        public ParsePendingChangedEventArgs(bool oldValue, bool newValue)
        {
            OldParsePending = oldValue;
            NewParsePending = newValue;
        }

        public bool OldParsePending { get; private set; }
        public bool NewParsePending { get; private set; }
    }

    public class LinesUpdateEventArgs : EventArgs
    {
        public LinesUpdateEventArgs(int line, int affectedLines, int delta)
        {
            this.LineIndex = line;
            this.AffectedLineCount = affectedLines;
            this.DeltaLines = delta;
        }

        public int LineIndex { get; private set; }
        public int AffectedLineCount { get; private set; }
        public int DeltaLines { get; private set; }
    }

    internal class TextBuffer : ITextBuffer
    {
        private enum Operation { None, Insert, Replace, Move };

        private UndoRedoRecorder undoRedoRecorder = null;
        private List<string> lineList = null;
        private IScriptObject owningScript = null;

        private int lastInsertedLineIndex = -1;
        private Operation lastOperation = Operation.None;

        private List<FindPosition> searchResult = null;
        private List<FindPosition> prevSearchResult = null;
        private int currentPosition = -1;
        private string searchTerm = null;
        private string previousSearchTerm = "";
        string prevOption = null;
        int searchOption;

        private bool scriptModified = false;
        private bool parsePending = false;

        #region ITextBuffer: Text Editing Related Interface Methods

        public void InsertText(int line, int column, string text)
        {
            if (line < 0 || (line > lineList.Count))
                return;

            int startLineCount = lineList.Count;
            if (lineList.Count == line)
            {
                if (text.Equals("\n") || UndoGroupNeedsClosing(line, Operation.Insert))
                    undoRedoRecorder.CloseUndoGroup();

                List<string> removedLines = new List<string>();
                removedLines.Add(string.Empty);
                int resultLineCount = LineCountFromString(text);
                undoRedoRecorder.RecordInsertion(line, resultLineCount,
                    removedLines, new Point(column, line));

                lineList.Add(text);
            }
            else
            {

                string original = lineList[line];

                // Validates that 'column' should never exceed the length of 'original'.
                column = ((column <= original.Length) ? column : original.Length);

                string firstPart = (column <= 0) ? string.Empty : original.Substring(0, column);
                string secondPart = string.Empty;
                if (column < original.Length)
                    secondPart = original.Substring(column, original.Length - column);

                string intermediate = firstPart + text + secondPart;

                // The following process breaks the 'intermediate' string into multiple lines, 
                // if it includes any \n character(s) in it.
                // 
                string singleLine = string.Empty;
                List<string> newLines = new List<string>();
                for (int index = 0; index < intermediate.Length; index++)
                {
                    if (intermediate[index].Equals('\n'))
                    {
                        singleLine = singleLine + intermediate[index];
                        newLines.Add(singleLine);
                        singleLine = string.Empty;
                    }
                    else
                    {
                        singleLine = singleLine + intermediate[index];
                        if (index == intermediate.Length - 1)
                            newLines.Add(singleLine);
                    }
                }

                // If the inserted text is an <Enter> key, or the line modification is done on 
                // a different line altogether, close out the previous undo group. The new action
                // will be recorded as the beginning of a new undo group.
                // 
                if (text.Equals("\n") || UndoGroupNeedsClosing(line, Operation.Insert))
                    undoRedoRecorder.CloseUndoGroup();

                // Add the line modification into the undo recorder.
                List<string> replacedLines = new List<string>();
                replacedLines.Add(lineList[line]);
                undoRedoRecorder.RecordInsertion(line, newLines.Count, replacedLines, new Point(column, line));

                // This last process removes the line being edited, and then replace it with 
                // the new line(s) that are formed with above steps.
                // 
                lineList.RemoveAt(line);
                int insertionIndex = line;
                foreach (string newLine in newLines)
                {
                    lineList.Insert(insertionIndex, newLine);
                    insertionIndex++;
                }
            }

            ArrangeTextBuffer();

            if (null != LinesUpdated)
            {
                // Notify listeners of line changes.
                LinesUpdated(this, new LinesUpdateEventArgs(
                    line, 1, lineList.Count - startLineCount));
            }
        }

        public void ReplaceText(int startLine, int startColumn, int endLine, int endColumn, string text)
        {
            System.Drawing.Point cursor = new Point(endColumn, endLine);

            if (endLine >= lineList.Count)
            {
                endLine = lineList.Count - 1;
                endColumn = lineList[lineList.Count - 1].Length;
            }

            if (startLine < 0 || (startLine >= lineList.Count))
                return;
            if (endLine < 0 || (endLine >= lineList.Count))
                return;

            // The simple rule of text buffer modification: if the end of selection 
            // includes "\n" at the end of the final selected line, then it should 
            // be moved to include the subsequent line. The reason for this is that 
            // there is a possibility this "\n" will be removed as part of editing
            // (imagine the case where cursor is placed at the end of the line, and 
            // user hits <Delete> key, then the "\n" is removed causing the line to
            // be merged with the next line), and we really want to include the 
            // next line into undo recording for restoration later.
            // 
            if (endLine != lineList.Count - 1) // If we're not already at the end.
            {
                // If at the end of selection is the "\n" character...
                if (endColumn != 0 && lineList[endLine][endColumn - 1] == '\n')
                {
                    endLine = endLine + 1;
                    endColumn = 0;
                }
            }

            int startLineCount = lineList.Count;
            string resultString = string.Empty;

            string firstLine = lineList[startLine];
            if (startColumn > firstLine.Length)
                startColumn = firstLine.Length;

            resultString = firstLine.Substring(0, startColumn) + text;

            string finalLine = lineList[endLine];
            if (endColumn < finalLine.Length)
                resultString += finalLine.Substring(endColumn, finalLine.Length - endColumn);

            List<string> originalContents = new List<string>();
            if (endLine >= startLine)
            {
                originalContents.AddRange(lineList.GetRange(startLine, endLine - startLine + 1));
                lineList.RemoveRange(startLine, endLine - startLine + 1);
                if (string.IsNullOrEmpty(resultString) == false)
                    lineList.Insert(startLine, resultString);
            }

            if (UndoGroupNeedsClosing(startLine, Operation.Replace))
                undoRedoRecorder.CloseUndoGroup();

            int resultLineCount = LineCountFromString(resultString);
            undoRedoRecorder.RecordInsertion(startLine,
                resultLineCount, originalContents, cursor);

            if (resultLineCount != originalContents.Count)
                undoRedoRecorder.CloseUndoGroup();

            ArrangeTextBuffer();

            if (null != LinesUpdated)
            {
                // Notify listeners of line changes.
                int lineCount = endLine - startLine;
                LinesUpdated(this, new LinesUpdateEventArgs(startLine,
                    lineCount, lineList.Count - startLineCount));
            }
        }

        // Set selection text to currentIndex startPoint and endPoint for every case except ReplaceAll
        public void FindReplace(string textToSearch, string replacement, FindOptions searchOptions)
        {
            if (string.IsNullOrWhiteSpace(textToSearch))
            {
                if (searchResult != null)
                    searchResult.Clear();

                return;
            }

            // Different search option, Find all occurences.
            bool isFindOnly = !searchOptions.HasFlag(FindOptions.ReplaceAll) && !searchOptions.HasFlag(FindOptions.MatchCase)
                && !searchOptions.HasFlag(FindOptions.FindNext) && !searchOptions.Equals(FindOptions.FindPrevious) && string.IsNullOrEmpty(replacement);
            searchTerm = textToSearch;
            searchOption = (int)searchOptions;

            if (!previousSearchTerm.Equals(textToSearch) &&
                !searchOptions.HasFlag(FindOptions.ReplaceOnce) && !searchOptions.HasFlag(FindOptions.MatchCase))
                FindAllOccurences(textToSearch, searchOptions, isFindOnly);

            else
            {
                if (prevSearchResult != null && prevSearchResult.Count > 0 && prevSearchResult.Equals(searchResult))
                {
                    searchResult.RemoveAt(CurrentSearchIndex);
                }
                else
                {
                    if (!previousSearchTerm.Equals(textToSearch))
                        FindAllOccurences(textToSearch, searchOptions, isFindOnly);
                }
            }

            if (searchResult == null)
                return;
            // Nothing found
            if (searchResult.Count <= 0)
                return;
            // Just a find
            if (isFindOnly)
                return;

            if (replacement == null)
            {
                // Find Previous
                if (searchOptions.HasFlag(FindOptions.FindPrevious))
                {
                    CurrentSearchIndex--;
                    if (CurrentSearchIndex < 0)
                        CurrentSearchIndex = searchResult.Count - 1;
                }
                else if (searchOptions.HasFlag(FindOptions.FindNext))
                {
                    // FindNext
                    CurrentSearchIndex++;
                    if (CurrentSearchIndex >= searchResult.Count)
                        CurrentSearchIndex = 0;
                }

                return;
            }

            if ((searchOptions.HasFlag(FindOptions.ReplaceAll)))
            {
                if (UndoGroupNeedsClosing(searchResult[0].startPoint.Y, Operation.Replace))
                    undoRedoRecorder.CloseUndoGroup();
                int index = 0;
                int findCount = searchResult.Count;
                for (int i = 0; i < findCount; i++)
                {
                    ReplaceSingleOccurence(searchResult[i], replacement);
                    UpdateStartEnd(index++, replacement.Length - textToSearch.Length);
                    UpdateEnd(i, replacement.Length - textToSearch.Length);
                }

                CurrentSearchIndex = -1;
                undoRedoRecorder.CloseUndoGroup();
            }
            else
            {
                // Replace once
                if (CurrentSearchIndex == -1)
                    CurrentSearchIndex = 0;
                UpdateStartEnd(CurrentSearchIndex, replacement.Length - textToSearch.Length);
                if (UndoGroupNeedsClosing(searchResult[CurrentSearchIndex].startPoint.Y, Operation.Replace))
                    undoRedoRecorder.CloseUndoGroup();
                ReplaceSingleOccurence(searchResult[CurrentSearchIndex], replacement);
                UpdateEnd(CurrentSearchIndex, replacement.Length - textToSearch.Length);
                undoRedoRecorder.CloseUndoGroup();
                if (CurrentSearchIndex > 0)
                    CurrentSearchIndex--;
                else
                    prevSearchResult = searchResult;
            }

            ArrangeTextBuffer();

            if (null != LinesUpdated)
            {
                // Notify listeners of line changes.
                LinesUpdated(this, new LinesUpdateEventArgs(searchResult[0].startPoint.Y,
                    lineList.Count, lineList.Count));
            }
        }

        public void GetSearchResults(ref List<FindPosition> findResult)
        {
            findResult = searchResult;
        }

        public void ModifyText(int line, string text)
        {
            lineList[line] = text;
            ScriptModified = true;
        }

        public void MoveText(System.Drawing.Point originStart, System.Drawing.Point originEnd, System.Drawing.Point destination)
        {
            #region Set up the stage for text moving

            if (Utility.IsPointInRegion(originStart, originEnd, destination))
                return;

            if (destination.Y == lineList.Count)
                destination = new Point(lineList[destination.Y - 1].Length, destination.Y - 1);

            /*if (lineList[destination.Y].Length <= destination.X && lineList[destination.Y].Length > 0 &&
                lineList[destination.Y][lineList[destination.Y].Length - 1] == '\n')
                destination.X = lineList[destination.Y].Length - 1;*/

            if (originEnd.Y == originStart.Y && originStart.X == 0 && originEnd.X == lineList[originEnd.Y].Length)
            {
                if (lineList[originEnd.Y][lineList[originEnd.Y].Length - 1] == '\n')
                {
                    originEnd.X = 0;
                    originEnd.Y = originEnd.Y + 1;
                }
            }

            if (originEnd.X != 0 && lineList[originEnd.Y][originEnd.X - 1] == '\n' && originEnd.Y != lineList.Count - 1)
            {
                originEnd.Y += 1;
                originEnd.X = 0;
            }

            if (originEnd.Y == lineList.Count)
            {
                originEnd.Y -= 1;
                originEnd.X = this.GetCharacterCount(originEnd.Y, false, true);
            }

            #endregion

            #region Drop destination line adjustment pass

            // Extracts the text that is to be dragged and dropped
            string movingTextContent = GetText(originStart.Y, originStart.X, originEnd.Y, originEnd.X);

            // Calculate the destination (drop point) coordinates. If one or more line is dragged, 
            // the offset and new line number for the drop line has to be recalculated. For the 
            // case of 5-line buffer and 2 lines are moved: After removal of both the lines and 
            // before insertion the buffer will contain only 3 lines. If the destination line is 
            // before the selected text, no offset is needed. If destination line is after the 
            // selected line, the then destination line has to be subtracted by 2 before insertion.
            // 
            int newDestinationLine = 0;
            int newDestinationColumn = 0;
            int undoRemoveLine = 0;

            if (originStart.Y > destination.Y)
            {
                // This is if insertion was done before origin
                newDestinationLine = destination.Y;
                newDestinationColumn = destination.X;
                undoRemoveLine = originStart.Y;
            }
            else if (originEnd.Y < destination.Y)
            {
                // This is if insertion was done after origin
                newDestinationLine = destination.Y - (originEnd.Y - originStart.Y);
                newDestinationColumn = destination.X;
                undoRemoveLine = originStart.Y + (originEnd.Y - originStart.Y);
            }
            else
            {
                // If origin and destination are on the same line
                if (originEnd.Y == destination.Y)
                    newDestinationLine = destination.Y - (originEnd.Y - originStart.Y);

                else if (originStart.Y == destination.Y)
                {
                    newDestinationLine = destination.Y;
                    undoRemoveLine = originStart.Y;
                }

                if (originStart.X < destination.X)
                    newDestinationColumn = destination.X - (originEnd.X - originStart.X);
                else
                    newDestinationColumn = destination.X;
            }

            #endregion

            #region Lines removal pass

            // Remove the moving section of text from buffer.
            string lastSrcLineContent = lineList[originEnd.Y];
            string leftOverContent = lineList[originStart.Y].Substring(0, originStart.X);
            leftOverContent += lastSrcLineContent.Substring(originEnd.X, lastSrcLineContent.Length - originEnd.X);

            List<string> originalContents = lineList.GetRange(originStart.Y, originEnd.Y - originStart.Y + 1);
            undoRedoRecorder.CloseUndoGroup();
            undoRedoRecorder.RecordRemoval(originStart.Y, 1, originalContents, originEnd);

            int startLineCount = lineList.Count;
            lineList.RemoveRange(originStart.Y, originEnd.Y - originStart.Y + 1);
            lineList.Insert(originStart.Y, leftOverContent);

            if (null != LinesUpdated)
            {
                // Notify listeners of lines removal.
                int lineCount = originEnd.Y - originStart.Y + 1;
                LinesUpdated(this, new LinesUpdateEventArgs(originStart.Y,
                    lineCount, lineList.Count - startLineCount));
            }

            #endregion

            #region Lines insertion pass

            string destinationContent = lineList[newDestinationLine];

            int tailLength = destinationContent.Length - newDestinationColumn;
            string newInsertLine = destinationContent.Substring(0, newDestinationColumn);
            newInsertLine += movingTextContent;
            newInsertLine += destinationContent.Substring(newDestinationColumn, tailLength);

            // Determine if there is any linebreak between the line, do not 
            // count the one at the end of the line, this is to negate the 
            // effects of ArrangeTextBuffer.
            // 
            string tempLine = newInsertLine;
            if (tempLine.EndsWith("\n"))
                tempLine = tempLine.Remove(tempLine.Length - 1);
            int lineCountToRemove = 1;
            if (newInsertLine.Contains("\n"))
                lineCountToRemove += tempLine.Count(c => c == '\n');

            originalContents = lineList.GetRange(newDestinationLine, 1);
            undoRedoRecorder.RecordInsertion(newDestinationLine,
                lineCountToRemove, originalContents, destination);
            undoRedoRecorder.CloseUndoGroup();

            startLineCount = lineList.Count;
            lineList.RemoveAt(newDestinationLine);
            lineList.Insert(newDestinationLine, newInsertLine);
            ArrangeTextBuffer();

            if (null != LinesUpdated)
            {
                // Notify listeners of line changes.
                int deltaLines = lineList.Count - startLineCount;
                LinesUpdated(this, new LinesUpdateEventArgs(newDestinationLine, 1, deltaLines));
            }

            #endregion
        }

        public void UndoTextEditing(ref Point cursorPosition)
        {
            int lineIndex = -1, affectedLines = -1;
            int startLineCount = lineList.Count;

            ModificationGroup modifications = undoRedoRecorder.GetTopMostUndoGroup();
            if (null == modifications)
                return;

            Modification modification = modifications.GetLastModification();
            if (null != modification)
            {
                lineIndex = modification.StartLine;
                affectedLines = modification.NewLinesCount;
            }

            undoRedoRecorder.Undo(ref cursorPosition);
            ArrangeTextBuffer();

            if (null != LinesUpdated)
            {
                // Notify listeners of line changes.
                int deltaLines = lineList.Count - startLineCount;
                LinesUpdated(this, new LinesUpdateEventArgs(lineIndex, affectedLines, deltaLines));
            }
        }

        public void RedoTextEditing(ref Point cursorPosition)
        {
            int lineIndex = -1, affectedLines = -1;
            int startLineCount = lineList.Count;

            ModificationGroup modifications = undoRedoRecorder.GetTopMostRedoGroup();
            if (null == modifications)
                return;

            Modification modification = modifications.GetLastModification();
            if (null != modification)
            {
                lineIndex = modification.StartLine;
                affectedLines = modification.OriginalContents.Count;
            }

            undoRedoRecorder.Redo(ref cursorPosition);
            ArrangeTextBuffer();

            if (null != LinesUpdated)
            {
                // Notify listeners of line changes.
                int deltaLines = lineList.Count - startLineCount;
                LinesUpdated(this, new LinesUpdateEventArgs(lineIndex, affectedLines, deltaLines));
            }
        }

        #endregion

        #region ITextBuffer: State Query Interface Methods

        public int GetLineCount(bool includeVirtualLine = true)
        {
            if (lineList == null || (lineList.Count <= 0))
                return 0;

            // If the last line in "lineList" actually ends with '\n' character, 
            // then it is considered to have an additional "virtual line" 
            // following it (but the line does not physically exist). In cases 
            // like rendering source codes, it is expected to have have an 
            // additional empty line after the final line for user to place the 
            // caret on it for typing/selection.
            // 
            if (false == includeVirtualLine)
                return lineList.Count;

            int totalLines = lineList.Count;
            string finalLineContent = lineList[lineList.Count - 1];
            if (finalLineContent.EndsWith("\n"))
                totalLines = totalLines + 1;

            return totalLines;
        }

        public int GetWidestLineWidth(bool expandTabs)
        {
            if (null == lineList)
                return 0;

            int maxCharCount = 0;
            for (int i = 0; i < lineList.Count; i++)
            {
                string line = lineList[i];
                if (expandTabs)
                    line = line.Replace("\t", Configurations.TabSpaces);

                if (null != line && (maxCharCount < line.Length))
                    maxCharCount = line.Length;
            }

            return maxCharCount;
        }

        public int GetFirstNonWhiteSpaceChar(int column, int line)
        {
            if (null == lineList || (lineList.Count == 0))
                return -1;
            if (line < 0 || (line >= lineList.Count))
                return -1;
            if (column < 0)
                column = 0;

            string content = lineList[line];
            if (column >= content.Length)
                return -1;

            if (char.IsWhiteSpace(content[column]) == false)
                return column;

            int firstNonWhiteSpace = 0;
            while (firstNonWhiteSpace < content.Length)
            {
                if (char.IsWhiteSpace(content[firstNonWhiteSpace]) == false)
                    return firstNonWhiteSpace;

                firstNonWhiteSpace = firstNonWhiteSpace + 1;
            }

            return -1;
        }

        public int GetCharacterCount(int line, bool expandTabs, bool includeLineBreak)
        {
            if (line < 0 || (line >= lineList.Count()))
                return 0; // Beyond the scope of this doc.

            if (expandTabs == false)
            {
                string currentLine = lineList[line];
                if (false != includeLineBreak)
                    return currentLine.Length;

                if (currentLine.Count() == 0)
                    return 0;
                else if (currentLine[currentLine.Length - 1] == '\n')
                    return currentLine.Length - 1;
                return currentLine.Length;
            }
            else
            {
                int length = 0;
                foreach (char c in lineList[line])
                {
                    if (c == '\t')
                        length += Configurations.TabSpaces.Length;
                    else if (c == '\n')
                        length += (includeLineBreak ? 1 : 0);
                    else
                        length = length + 1;
                }

                return length;
            }
        }

        public bool IsNullOrWhiteSpace(int line)
        {
            if (null == lineList || (lineList.Count == 0))
                return true;
            if (line < 0 || (line >= lineList.Count))
                return true;

            return (string.IsNullOrWhiteSpace(lineList[line]));
        }

        #endregion

        #region ITextBuffer: Content Query Interface Methods

        public string GetContent()
        {
            StringBuilder bob = new StringBuilder();
            foreach (string line in lineList)
                bob.Append(line);

            return bob.ToString();
        }

        public string GetPartialContent(int[] linesToExclude, string replacement)
        {
            if (null == linesToExclude || (linesToExclude.Length <= 0))
                return GetContent();

            StringBuilder bob = new StringBuilder();
            for (int index = 0; index < lineList.Count; ++index)
            {
                if (linesToExclude.Contains(index))
                {
                    // Replace the line content with something else.
                    if (string.IsNullOrEmpty(replacement) == false)
                        bob.Append(replacement);

                    continue; // Skip this line.
                }

                bob.Append(lineList[index]);
            }

            return bob.ToString();
        }

        public Stream GetStream()
        {
            string lineContent = GetContent();
            return new MemoryStream(System.Text.Encoding.Default.GetBytes(lineContent));
        }

        public string GetText(CharPosition rangeStart, CharPosition rangeEnd)
        {
            int startX = rangeStart.GetCharacterPosition().X;
            int startY = rangeStart.GetCharacterPosition().Y;
            int endX = rangeEnd.GetCharacterPosition().X;
            int endY = rangeEnd.GetCharacterPosition().Y;
            return GetText(startY, startX, endY, endX);
        }

        public string GetText(int startLine, int startColumn, int endLine, int endColumn)
        {
            if (startLine > endLine)
                return string.Empty;

            string textContent = string.Empty;
            if (startLine < endLine)
            {
                for (int lineIndex = startLine; lineIndex <= endLine; lineIndex++)
                {
                    if (lineIndex < 0 || (lineIndex >= lineList.Count))
                        continue; // This line index is invalid.

                    string currentLine = lineList[lineIndex];

                    if (lineIndex == startLine)
                        textContent = currentLine.Substring(startColumn);
                    else if (lineIndex == endLine)
                    {
                        if (endColumn > currentLine.Length)
                            endColumn = currentLine.Length;
                        textContent += currentLine.Substring(0, endColumn);
                    }
                    else
                        textContent += currentLine;
                }
            }
            else
            {
                if (startColumn >= endColumn)
                    return string.Empty;
                if (endLine < 0 || (endLine >= lineList.Count))
                    return string.Empty;

                int charCount = endColumn - startColumn;
                textContent = lineList[endLine].Substring(startColumn, charCount);
            }

            return textContent;
        }

        public string GetLineContent(int line)
        {
            if (0 > line || (line > lineList.Count - 1))
                return string.Empty;

            return lineList[line];
        }

        public string GetIdentifierBeforeColumn(int line, int column)
        {
            string lineContent = GetLineContent(line);
            if (string.IsNullOrEmpty(lineContent))
                return string.Empty;

            // Discard everything after the column.
            lineContent = lineContent.Substring(0, column).Trim();

            Regex nameExtractionRule = new Regex(@"\b(\w+)\b$");
            Regex arrayIndexerRule = new Regex(@"\[[_\.\w\s]+\]$");
            StringBuilder identBuilder = new StringBuilder();

            bool matchedIdentifierName = false;
            while (lineContent.Length > 0)
            {
                string matchedString = string.Empty;
                if (lineContent[lineContent.Length - 1] == '.')
                {
                    // A period character '.' can only come before a variable 
                    // name (e.g. "myLine"). It cannot preceed an array indexer 
                    // (e.g. "[t.b]") or another '.' character.
                    if (false == matchedIdentifierName)
                        break;

                    matchedIdentifierName = false;
                    matchedString = ".";
                }
                else
                {
                    matchedIdentifierName = false;
                    Match match = nameExtractionRule.Match(lineContent);
                    if (null != match && (false != match.Success))
                        matchedIdentifierName = true;
                    else
                    {
                        // Try to match an array indexer instead.
                        match = arrayIndexerRule.Match(lineContent);
                    }

                    if (null == match || (false == match.Success))
                        break;

                    matchedString = match.Value;
                }

                identBuilder.Insert(0, matchedString);
                int remaining = lineContent.Length - matchedString.Length;
                lineContent = lineContent.Substring(0, remaining).Trim();
            }

            identBuilder.Replace(" ", "");
            identBuilder.Replace("\t", "");

            string intermediate = identBuilder.ToString();
            while (intermediate.StartsWith("."))
                intermediate = intermediate.Substring(1);

            return intermediate;
        }

        public FunctionCallContext GetFunctionCallContext(int line, int column)
        {
            return FunctionCallContext.Build(lineList, new Point(column, line));
        }

        #endregion

        #region ITextBuffer: Smart Formatting Interface Methods

        public bool FormatText()
        {
            if (null == lineList || (lineList.Count == 0))
                return false;

            return this.FormatText(0, lineList.Count - 1);
        }

        public bool FormatText(int fromLine, int toLine)
        {
            if (SmartFormatter.Instance.Format(lineList, fromLine, toLine))
            {
                int endColumn = this.GetCharacterCount(toLine, false, true);
                string formatted = SmartFormatter.Instance.FormattedOutput;

                CloseUndoGroup();
                ReplaceText(fromLine, 0, toLine, endColumn, formatted);
                CloseUndoGroup();
                return true;
            }

            return false;
        }

        public int FindOpenBrace(int line, int column)
        {
            int lineIndex = line;
            int columnIndex = column;
            bool openBraceFlag = false;
            bool lineBeforeOpenBrace = false;
            for (; lineIndex >= 0; lineIndex--)
            {
                for (; columnIndex >= 0; columnIndex--)
                {
                    if (lineList[lineIndex][columnIndex] == '{' && openBraceFlag == false)
                        openBraceFlag = true;
                    else if (lineList[lineIndex][columnIndex] != ' ' && lineList[lineIndex][columnIndex] != '\n'
                        && lineList[lineIndex][columnIndex] != '\t' && openBraceFlag == true)
                    {
                        lineBeforeOpenBrace = true;
                        break;
                    }
                }
                if (lineBeforeOpenBrace)
                    break;
                if (lineIndex - 1 >= 0)
                    columnIndex = lineList[lineIndex - 1].Length - 1;
            }
            return lineIndex;
        }

        public string RetrieveLineBlock(ref int lineIndex, int column)
        {
            int columnIndex = column - 1;

            string tempLine = lineList[lineIndex];
            while (!tempLine.Contains(';'))
            {
                lineIndex--;
                tempLine += lineList[lineIndex];
            }

            return tempLine;
        }

        #endregion

        #region ITextBuffer: Public Interface Events

        public event ParsePendingChangedHandler ParsePendingChanged = null;
        public event ScriptModifiedHandler Modified = null;
        public event LinesUpdatedHandler LinesUpdated = null;

        #endregion

        #region ITextBuffer: Text Buffer Properties

        public bool ParsePending
        {
            get { return parsePending; }

            set
            {
                bool oldValue = parsePending;
                parsePending = value;

                if (null != ParsePendingChanged && (parsePending != false))
                {
                    ParsePendingChangedEventArgs args = null;
                    args = new ParsePendingChangedEventArgs(oldValue, parsePending);
                    ParsePendingChanged(this, args);
                }
            }
        }

        public bool ScriptModified
        {
            get { return scriptModified; }

            set
            {
                if (scriptModified == value)
                    return;

                scriptModified = value;
                if (null != Modified)
                    Modified(this, new EventArgs());
            }
        }

        public IScriptObject OwningScript
        {
            get { return owningScript; }
        }

        public int CurrentSearchIndex { get; private set; }

        public List<FindPosition> SearchResult
        {
            get { return searchResult; }
        }

        public string TextToSearch
        {
            get { return searchTerm; }
        }

        public int SearchOption
        {
            get { return searchOption; }
        }

        #endregion

        #region Internal Operational Methods

        internal TextBuffer(IScriptObject owningScript)
        {
            this.owningScript = owningScript;
            IParsedScript parsedScript = owningScript.GetParsedScript();

            if (parsedScript.GetScriptPath() == "")
            {
                this.lineList = new List<string>();
                this.lineList.Add(string.Empty);
            }
            else
            {
                string scriptFilePath = parsedScript.GetScriptPath();
                ScriptFileReader scriptReader = new ScriptFileReader(scriptFilePath, true);
                this.lineList = scriptReader.ReadInput();
            }

            undoRedoRecorder = new UndoRedoRecorder(this.lineList);

            this.parsePending = true;
            this.scriptModified = false;
        }

        internal TextBuffer(string initialContent)
        {
            this.lineList = new List<string>();
            this.lineList.Add(initialContent);
            ArrangeTextBuffer();

            this.scriptModified = false;
            this.parsePending = false;
            undoRedoRecorder = new UndoRedoRecorder(this.lineList);
        }

        internal char GetCharAt(int column, int line)
        {
            if (0 <= line && (line <= lineList.Count - 1))
            {
                if (0 <= column && (column <= lineList[line].Length - 1))
                    return (lineList[line])[column];
            }

            return '\0';
        }

        internal void CloseUndoGroup()
        {
            this.undoRedoRecorder.CloseUndoGroup();
        }

        internal bool ReloadFromFile(string scriptFilePath)
        {
            if (string.IsNullOrEmpty(scriptFilePath))
                return false;

            ScriptFileReader scriptReader = new ScriptFileReader(scriptFilePath, true);
            this.lineList = scriptReader.ReadInput();

            undoRedoRecorder = new UndoRedoRecorder(this.lineList);

            // Setting through these properties instead of directly to the data 
            // members (i.e. this.parsePending and this.scriptModified) so that 
            // UI listeners get notified of such reload event and update the UI 
            // accordingly.
            // 
            this.ParsePending = true;
            this.ScriptModified = false;
            return true;
        }

        #endregion

        #region Private Class Helper Methods

        private bool UndoGroupNeedsClosing(int line, TextBuffer.Operation operation)
        {
            bool needsClosing = false;
            if (lastInsertedLineIndex != line)
            {
                lastInsertedLineIndex = line;
                needsClosing = true;
            }

            if (lastOperation != operation)
            {
                lastOperation = operation;
                needsClosing = true;
            }

            return needsClosing;
        }

        private void ArrangeTextBuffer()
        {
            // This method attempts to do two things:
            // 1. To join two lines if the first does not end with a \n.
            // 2. To break a line if \n is not at the end.
            // 
            for (int index = 0; index < lineList.Count; index++)
            {
                // If this line does not include \n character, 
                // then it needs to be merged with the next line.
                if (lineList[index].Contains('\n') == false)
                {
                    // Note that we don't do this to the final line in the text buffer, 
                    // because there will be nothing more to merge with (no next line).
                    if (index < (lineList.Count - 1))
                    {
                        string combined = lineList[index];
                        lineList[index] = combined + lineList[index + 1];
                        lineList.RemoveAt(index + 1);
                        index = index - 1; // Make sure we reprocess the same line later.
                    }
                }
                else
                {
                    int length = lineList[index].Length;
                    int newLineIndex = lineList[index].IndexOf('\n');
                    if (newLineIndex != length - 1)
                    {
                        // If the \n character is in the middle of the line,
                        // then this line needs to be broken up into two parts.
                        string toBeBroken = lineList[index];
                        lineList.RemoveAt(index);
                        lineList.Insert(index, toBeBroken.Substring(0, newLineIndex + 1));
                        lineList.Insert(index + 1, toBeBroken.Substring(newLineIndex + 1, length - 1 - newLineIndex));
                    }
                }
            }

            this.ParsePending = true;
            this.ScriptModified = true;
        }

        private void FindAllOccurences(string textToSearch, FindOptions option, bool isFindOnly)
        {
            searchResult = new List<FindPosition>();
            int lineNumber = 0;
            int column = 0;

            foreach (string line in lineList)
            {
                if (option.HasFlag(FindOptions.MatchCase) || (prevOption != null && ((option.HasFlag(FindOptions.FindNext) || option.Equals(FindOptions.FindPrevious)) && prevOption.Equals("MatchCase"))))
                    column = line.IndexOf(textToSearch);
                else
                    column = line.IndexOf(textToSearch, StringComparison.CurrentCultureIgnoreCase);

                if (column != -1)
                {
                    while (column != -1)
                    {
                        FindPosition position = new FindPosition();
                        position.startPoint = new Point(column, lineNumber);
                        position.endPoint = new Point(column + textToSearch.Length - 1, lineNumber);
                        searchResult.Add(position);
                        if (column + textToSearch.Length < line.Length)
                        {
                            // "previousColumn stores how much column has already moved
                            int previousColumn = column;

                            if(option.HasFlag(FindOptions.MatchCase) || (prevOption!=null && ((option.HasFlag(FindOptions.FindNext)||option.Equals(FindOptions.FindPrevious))&&prevOption.Equals("MatchCase"))))
                                column = line.Substring(column + textToSearch.Length).IndexOf(textToSearch);
                            else
                                column = line.Substring(column + textToSearch.Length).IndexOf(textToSearch, StringComparison.CurrentCultureIgnoreCase);


                            if (column != -1)
                                column += textToSearch.Length + previousColumn;
                        }
                        else
                            break;
                    }
                }
                lineNumber++;
            }

            if (searchResult.Count <= 0)
                CurrentSearchIndex = -1;

            if ((option.HasFlag(FindOptions.FindNext) || option.HasFlag(FindOptions.FindPrevious) || option.HasFlag(FindOptions.ReplaceOnce) && !isFindOnly))
                return;

            if (searchResult.Count > 0)
                CurrentSearchIndex = 0;

            if (option.HasFlag(FindOptions.MatchCase) || isFindOnly)
                prevOption = option.ToString();

        }

        private void UpdateStartEnd(int currentIndex, int delta)
        {
            FindPosition currentReplace = searchResult[currentIndex];
            for (int i = currentIndex + 1; i < searchResult.Count; i++)
            {
                if (searchResult[i].endPoint.Y != currentReplace.endPoint.Y)
                    break;
                else
                {
                    FindPosition position = searchResult[i];
                    position.startPoint.X += delta;
                    position.endPoint.X += delta;
                    searchResult[i] = position;
                }
            }
        }

        private void UpdateEnd(int index, int delta)
        {
            FindPosition position = searchResult[index];
            position.endPoint.X += delta;
            searchResult[index] = position;
        }

        private void ReplaceSingleOccurence(FindPosition position, string textToReplace)
        {
            string resultString = lineList[position.startPoint.Y].Substring(0, position.startPoint.X);
            resultString += textToReplace;
            resultString += lineList[position.startPoint.Y].Substring(position.endPoint.X + 1);
            List<string> originalContent = new List<string>();
            originalContent.Add(lineList[position.startPoint.Y]);
            undoRedoRecorder.RecordInsertion(position.startPoint.Y,
                1, originalContent, position.endPoint);
            lineList[position.startPoint.Y] = resultString;
        }

        private int LineCountFromString(string content)
        {
            if (string.IsNullOrEmpty(content))
                return 0;

            // Determine how many lines are there in the newly formed string by 
            // breaking it up for each '\n' character. If there is no '\n', the 
            // "parts" will still contain a single element (i.e. 'resultString').
            // If there's a final '\n' at the end of "content", then the last 
            // element in "parts" will be an empty string, which technically 
            // does not exist. In such cases, we'll simply deduct 1 from the line
            // count.
            // 
            string[] parts = content.Split('\n');
            int resultLineCount = ((null == parts) ? 0 : parts.Length);
            if (null != parts && (parts.Length > 0))
            {
                if (string.IsNullOrEmpty(parts[parts.Length - 1]))
                    resultLineCount = resultLineCount - 1;
            }

            return resultLineCount;
        }

        #endregion
    }
}
