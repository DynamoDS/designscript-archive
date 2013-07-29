using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Globalization;
using System.Windows;
using System.Windows.Media.Effects;
using DesignScript.Editor.Core;
using GraphToDSCompiler;

namespace DesignScriptStudio.Graph.Core
{
    class CodeBlockNode : VisualNode
    {
        #region Class Data Members

        private string compilableText = string.Empty;
        private Dictionary<int, List<VariableLine>> inputLines = new Dictionary<int, List<VariableLine>>();
        private List<VariableLine> outputLines = new List<VariableLine>();
        private List<double> lineHeights = null;
        #endregion

        #region Public Interface Methods

        public override bool Deserialize(IStorage storage)
        {
            if (storage == null)
                throw new ArgumentNullException("storage");

            try
            {
                base.Deserialize(storage);
                this.compilableText = storage.ReadString(FieldCode.CompilableText);

                int inputLinesCount = storage.ReadInteger(FieldCode.InputLinesCount);
                this.inputLines.Clear();
                for (int i = 0; i < inputLinesCount; i++)
                {
                    int statementIndex = storage.ReadInteger(FieldCode.StatmentIndex);
                    int variableLineCount = storage.ReadInteger(FieldCode.VariableLineCount);
                    List<VariableLine> variableLines = new List<VariableLine>();
                    for (int j = 0; j < variableLineCount; j++)
                    {
                        string variable = storage.ReadString(FieldCode.Variable);
                        int line = storage.ReadInteger(FieldCode.Line);
                        variableLines.Add(new VariableLine(variable, line));
                    }
                    this.inputLines.Add(statementIndex, variableLines);
                }

                int outputLinesCount = storage.ReadInteger(FieldCode.OutputLinesCount);
                this.outputLines.Clear();
                for (int i = 0; i < outputLinesCount; i++)
                {
                    string variable = storage.ReadString(FieldCode.Variable);
                    int line = storage.ReadInteger(FieldCode.Line);
                    this.outputLines.Add(new VariableLine(variable, line));
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n Code block node deserialization failed.");
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
                storage.WriteString(FieldCode.CompilableText, this.compilableText);

                storage.WriteInteger(FieldCode.InputLinesCount, this.inputLines.Count);
                foreach (KeyValuePair<int, List<VariableLine>> kvp in this.inputLines)
                {
                    storage.WriteInteger(FieldCode.StatmentIndex, kvp.Key);
                    storage.WriteInteger(FieldCode.VariableLineCount, kvp.Value.Count);
                    foreach (VariableLine variableLine in kvp.Value)
                    {
                        storage.WriteString(FieldCode.Variable, variableLine.variable);
                        storage.WriteInteger(FieldCode.Line, variableLine.line);
                    }
                }

                storage.WriteInteger(FieldCode.OutputLinesCount, this.outputLines.Count);
                foreach (VariableLine variableLine in this.outputLines)
                {
                    storage.WriteString(FieldCode.Variable, variableLine.variable);
                    storage.WriteInteger(FieldCode.Line, variableLine.line);
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n Code block node serialization failed.");
                return false;
            }
        }

        #endregion

        #region Internal Static Methods

        internal static int GetLineNumberFromInputSlotIndex(Dictionary<int, List<VariableLine>> inputLines, int slotIndex)
        {
            if (inputLines == null)
                throw new ArgumentNullException("input");

            List<VariableLine> variableLines = new List<VariableLine>();
            foreach (KeyValuePair<int, List<VariableLine>> kvp in inputLines)
                variableLines.AddRange(kvp.Value);
            return variableLines.ElementAt(slotIndex).line;
        }

        internal static int GetInputSlotIndexFromLineNumber(Dictionary<int, List<VariableLine>> inputLines, int lineNumber)
        {
            if (inputLines == null)
                throw new ArgumentNullException("input");
            if (lineNumber < 0)
                throw new ArgumentException("'lineNumber' is negative");

            List<int> lines = new List<int>();
            foreach (KeyValuePair<int, List<VariableLine>> kvp in inputLines)
            {
                foreach (VariableLine variableLine in kvp.Value)
                    lines.Add(variableLine.line);
            }
            return lines.IndexOf(lineNumber); //return -1 if not found
        }

        internal static List<int> GetInputSlotIndicesByName(Dictionary<int, List<VariableLine>> inputLines, string variableName)
        {
            List<int> inputSlotIndices = new List<int>();
            int indexCount = 0;
            foreach (List<VariableLine> inputStatement in inputLines.Values)
            {
                foreach (VariableLine variableLine in inputStatement)
                {
                    if (variableLine.variable == variableName)
                        inputSlotIndices.Add(indexCount);
                    indexCount++;
                }
            }
            return inputSlotIndices;
        }

        internal static string GetInputSlotNameFromInputSlotIndex(Dictionary<int, List<VariableLine>> inputLines, int slotIndex)
        {
            if (inputLines == null)
                throw new ArgumentNullException("input");

            List<VariableLine> variableLines = new List<VariableLine>();
            foreach (KeyValuePair<int, List<VariableLine>> kvp in inputLines)
                variableLines.AddRange(kvp.Value);
            return variableLines.ElementAt(slotIndex).variable;
        }

        internal static List<string> GetReferencedVariables(Dictionary<int, List<VariableLine>> inputLines, bool includeTempVariables)
        {
            if (inputLines == null)
                throw new ArgumentNullException("input");

            List<string> referencedVariables = new List<string>();
            foreach (KeyValuePair<int, List<VariableLine>> kvp in inputLines)
            {
                foreach (VariableLine variableLine in kvp.Value)
                {
                    if (Utilities.IsTempVariable(variableLine.variable) && !includeTempVariables)
                        continue;
                    referencedVariables.Add(variableLine.variable);
                }
            }
            return referencedVariables;
        }

        internal static int GetLineNumberFromOutputSlotIndex(List<VariableLine> outputLines, int slotIndex)
        {
            if (outputLines == null)
                throw new ArgumentNullException("output");

            return outputLines.ElementAt(slotIndex).line;
        }

        internal static int GetOutputSlotIndexFromLineNumber(List<VariableLine> outputLines, int lineNumber)
        {
            if (outputLines == null)
                throw new ArgumentNullException("output");
            if (lineNumber < 0)
                throw new ArgumentException("'lineNumber' is negative");

            List<int> lines = new List<int>();
            foreach (VariableLine variableLine in outputLines)
                lines.Add(variableLine.line);
            return lines.IndexOf(lineNumber); //return -1 if not found
        }

        internal static List<int> GetOutputSlotIndicesByName(List<VariableLine> outputLines, string variableName)
        {
            List<int> outputSlotsIndices = new List<int>();
            foreach (VariableLine variableLine in outputLines)
            {
                if (variableLine.variable == variableName)
                    outputSlotsIndices.Add(outputLines.IndexOf(variableLine));
            }
            return outputSlotsIndices;
        }

        internal static string GetOutputSlotNameFromOutputSlotIndex(List<VariableLine> outputLines, int slotIndex)
        {
            if (outputLines == null)
                throw new ArgumentNullException("output");

            return outputLines.ElementAt(slotIndex).variable;
        }

        internal static List<string> GetDefinedVariables(List<VariableLine> outputLines, bool includeTempVariables)
        {
            if (outputLines == null)
                throw new ArgumentNullException("output");

            List<string> definedVariables = new List<string>();
            foreach (VariableLine variableLine in outputLines)
            {
                if (!includeTempVariables && Utilities.IsTempVariable(variableLine.variable))
                    continue;
                definedVariables.Add(variableLine.variable);
            }
            return definedVariables;
        }

        #endregion

        #region Internal Class Methods

        internal CodeBlockNode(IGraphController graphController, string text)
            : base(graphController, NodeType.CodeBlock)
        {
            this.Caption = string.Empty;
            this.Text = text;
        }

        internal CodeBlockNode(IGraphController graphController, SnapshotNode snapshotNode)
            : base(graphController, NodeType.CodeBlock)
        {
            this.Caption = string.Empty;
            this.Text = snapshotNode.Content;
            string error;
            ErrorType errorType;
            AmendInputOutputSlots(out error, out errorType);
        }

        internal CodeBlockNode(IGraphController graphController) : base(graphController) { }

        internal override string GetSlotName(SlotType slotType, uint slotId)
        {
            int slotIndex = this.GetSlotIndex(slotId);
            if (slotType == SlotType.Input)
                return CodeBlockNode.GetInputSlotNameFromInputSlotIndex(this.inputLines, slotIndex);
            else if (slotType == SlotType.Output)
                return CodeBlockNode.GetOutputSlotNameFromOutputSlotIndex(this.outputLines, slotIndex);
            else
                throw new InvalidOperationException("'slotType' is none");
        }

        internal override Point GetSlotPosition(Slot slot)
        {
            if (slot == null)
                throw new ArgumentNullException("slot");

            Point slotPosition = this.nodePosition;
            slotPosition.Offset(0, 1);
            int slotIndex = this.GetSlotIndex(slot.SlotId);
            int lineNumber = -1;

            if (slot.SlotType == SlotType.Input)
                lineNumber = CodeBlockNode.GetLineNumberFromInputSlotIndex(this.inputLines, slotIndex);
            else
            {
                slotPosition.X += this.Width;
                lineNumber = CodeBlockNode.GetLineNumberFromOutputSlotIndex(this.outputLines, slotIndex);
            }

            slotPosition.Y += Configurations.SlotOffset + Configurations.SlotSize / 2;
            //slotPosition.Y += lineNumber * Configurations.FontPixels;
            slotPosition.Y += GetHeightFromLineNumber(lineNumber);
            return slotPosition;
        }

        internal override List<uint> GetSlotIdsByName(SlotType slotType, string variableName)
        {
            if (slotType == SlotType.None)
                throw new InvalidOperationException("'sloType' is none");

            if (slotType == SlotType.Input)
            {
                List<int> inputSlotIndices = CodeBlockNode.GetInputSlotIndicesByName(this.inputLines, variableName);
                List<uint> inputSlotIds = new List<uint>();
                foreach (int inputSlotIndex in inputSlotIndices)
                    inputSlotIds.Add(inputSlots[inputSlotIndex]);
                return inputSlotIds;
            }
            else
            {
                List<int> outputSlotIndices = CodeBlockNode.GetOutputSlotIndicesByName(this.outputLines, variableName);
                List<uint> outputSlotIds = new List<uint>();
                foreach (int outputSlotIndex in outputSlotIndices)
                    outputSlotIds.Add(outputSlots[outputSlotIndex]);
                return outputSlotIds;
            }

            throw new InvalidOperationException("'variableName' is not exist");
        }

        internal override List<string> GetDefinedVariables(bool includeTemporaryVariables)
        {
            return CodeBlockNode.GetDefinedVariables(this.outputLines, includeTemporaryVariables).Distinct().ToList<string>();
        }

        internal override List<string> GetReferencedVariables(bool includeTemporaryVariables)
        {
            return CodeBlockNode.GetReferencedVariables(this.inputLines, includeTemporaryVariables).Distinct().ToList<string>();
        }

        internal override string ToCode(out SnapshotNodeType type)
        {
            type = GraphUtilities.AnalyzeString(this.compilableText);
            int[] lineNumbers = this.GetAllLinesHavingInputConnection().Distinct().ToArray();
            return Utilities.DiscardTemporaryVariableAssignment(this.compilableText, lineNumbers);
        }

        internal override string PreprocessInputString(string input)
        {
            if (input.Trim() == string.Empty)
                return string.Empty;
            else
                return input;
        }

        internal override ErrorType ValidateInputString(out string error)
        {
            error = string.Empty;
            ErrorType errorType = ErrorType.None;
            this.AmendInputOutputSlots(out error, out errorType);
            return errorType;
        }

        internal override NodePart HitTest(double x, double y, out int index)
        {
            System.Windows.Point pt = this.nodePosition;
            index = -1;
            int lineNumber = -1;
            double singleLineHeight = Math.Ceiling(Utilities.GetFormattedTextHeight("A"));

            if (this.IsWithinClickRegion(pt, x, y))
                return NodePart.NorthWest;

            pt.X += this.Width;
            if (this.IsWithinClickRegion(pt, x, y))
                return NodePart.NorthEast;

            pt = this.nodePosition;
            if (y > pt.Y + this.Height + Configurations.InfoBubbleTopMargin)
                return NodePart.Preview;

            pt = this.nodePosition;
            if (y > pt.Y && y < pt.Y + this.Height)
            {
                if (x > pt.X - Configurations.HittestPixels && x < pt.X + Configurations.HittestPixels)
                {
                    lineNumber = GetLineNumberFromHeight(y - pt.Y - Configurations.TextVerticalOffset);
                    if (this.InputSlotIsVisible(lineNumber, out index))
                        return NodePart.InputSlot;
                    else
                        return NodePart.Text;
                }

                if (x > pt.X + this.Width - Configurations.HittestPixels && x < pt.X + this.Width + Configurations.OutputHittestPixels && this.outputSlots.Count > 0)
                {
                    lineNumber = GetLineNumberFromHeight(y - pt.Y - Configurations.TextVerticalOffset);
                    if (this.OutputSlotIsVisible(lineNumber, out index))
                        return NodePart.OutputSlot;
                    else
                        return NodePart.Text;
                }

                if (x > pt.X && x < pt.X + this.Width)
                    return NodePart.Text;
            }

            return NodePart.None;
        }

        internal override void UpdateSlotVisibility()
        {
            this.UpdateInputSlotVisibility();
            this.UpdateOutputSlotVisibility();
        }

        internal override void GetAssignmentStatements(List<AssignmentStatement> assignments)
        {
            if (null == this.outputLines || (this.outputLines.Count <= 0))
                return;

            int statements = this.outputLines.Count;
            for (int statement = 0; statement < statements; ++statement)
            {
                string output = this.outputLines[statement].variable;
                if (Utilities.IsTempVariable(output))
                    output = string.Empty;

                AssignmentStatement assignment = new AssignmentStatement(output);
                assignments.Add(assignment);

                if (null == this.inputLines && (this.inputLines.Count <= 0))
                    continue;

                if (!this.inputLines.ContainsKey(statement))
                    continue;

                List<VariableLine> variablesInStatement = this.inputLines[statement];
                if (null != variablesInStatement && (variablesInStatement.Count > 0))
                {
                    foreach (VariableLine line in variablesInStatement)
                    {
                        if (!Utilities.IsTempVariable(line.variable))
                            assignment.References.Add(line.variable);

                    }
                }
            }
        }

        #endregion

        #region Internal Class Properties

        /// <summary>
        /// This is a public class property that is used only for verification 
        /// purposes. It returns a copy of the internal data member 'inputLines'
        /// instead of the reference to it so outline world cannot tamper with 
        /// the private class data member.
        /// </summary>
        internal Dictionary<int, List<VariableLine>> InputLines
        {
            get
            {
                if (null == inputLines || (inputLines.Count <= 0))
                    return null;

                Dictionary<int, List<VariableLine>> results = new Dictionary<int, List<VariableLine>>();
                foreach (KeyValuePair<int, List<VariableLine>> inputLine in inputLines)
                {
                    List<VariableLine> lines = new List<VariableLine>();
                    lines.AddRange(inputLine.Value);
                    results[inputLine.Key] = lines;
                }

                return results;
            }
        }

        /// <summary>
        /// This is a public class property that is used only for verification 
        /// purposes. It returns a copy of the internal data member 'outputLines'
        /// instead of the reference to it so outline world cannot tamper with 
        /// the private class data member.
        /// </summary>
        internal List<VariableLine> OutputLines
        {
            get
            {
                if (null == outputLines || (outputLines.Count <= 0))
                    return null;

                List<VariableLine> results = new List<VariableLine>();
                results.AddRange(this.outputLines);
                return results;
            }
        }

        #endregion

        #region Protected Class Methods

        protected override void ComposeCore(DrawingContext drawingContext, DrawingVisual visual)
        {
            //double textWidth = this.GetTextWidth(this.Text);
            //if (textWidth > Configurations.NodeMaxWidthCodeBlock)
            //    textWidth = Configurations.NodeMaxWidthCodeBlock;
            double textWidth = Utilities.GetFormattedTextWidth(this.Text);
            GetLineHeights();

            double textHeight = 0;
            foreach (double lineHeight in lineHeights)
                textHeight += lineHeight;

            if (textHeight == 0)
                this.nodeDimension.Height = Configurations.NodeHeightCodeBlock + 2 + Configurations.TextVerticalOffset * 2;
            else
                this.nodeDimension.Height = textHeight + 2 + Configurations.TextVerticalOffset * 2;
            //this.nodeDimension.Height = Configurations.NodeHeightCodeBlock + textHeight + 2 + Configurations.TextVerticalOffset;
            this.nodeDimension.Width = Configurations.NodeWidthCodeBlock + textWidth + 3;

            base.ComposeCore(drawingContext, visual);

            //NorthEast
            Point p = new Point(0, 0);
            p.Offset(this.Width, Configurations.ContextMenuMargin);
            //DrawingUtilities.DrawDots(drawingContext, DotTypes.Top | DotTypes.TopRight | DotTypes.MiddleRight, p, AnchorPoint.TopRight, false);

            //NorthWest
            p = new Point(0, 0);
            p.Offset(Configurations.ContextMenuMargin, Configurations.ContextMenuMargin);
            //DrawingUtilities.DrawDots(drawingContext, DotTypes.TopLeft | DotTypes.Top | DotTypes.MiddleLeft, p, AnchorPoint.TopLeft, false);

            Point p1 = new Point(Configurations.CodeBlockHorizontalOffset, 0.5);
            Point p2 = new Point(this.Width - Configurations.CodeBlockHorizontalOffset, 0.5);
            if (this.Selected == false)
            {
                //drawingContext.DrawLine(Configurations.WhitePen, p1, p2);
                //p1.Y += this.Height;
                //p2.Y += this.Height;
                //drawingContext.DrawLine(Configurations.WhitePen, p1, p2);
                drawingContext.DrawRectangle(Brushes.White, Configurations.WhitePen, new Rect(p1.X, p1.Y, this.Width - 2 * Configurations.CodeBlockHorizontalOffset, (int)this.Height + 0.5));
            }
            Point pt = new Point(Configurations.TextHorizontalOffset + 3, Configurations.TextVerticalOffset + 1);
            Utilities.DrawText(drawingContext, this.Text, pt, Configurations.TextNormalColor, (int)Configurations.NodeMaxWidthCodeBlock);

            //InputSlots
            p1 = new Point(-1, 0);
            pt.Y += Configurations.TextVerticalOffset;
            double singleLineHeight = Utilities.GetFormattedTextHeight("A");
            foreach (KeyValuePair<int, List<VariableLine>> kvp in this.inputLines)
            {
                foreach (VariableLine variableLine in kvp.Value)
                {
                    int slotIndex;
                    if (this.InputSlotIsVisible(variableLine.line, out slotIndex))
                    {
                        p1.Y = singleLineHeight - Configurations.SlotSize / 2;
                        p1.Y += GetHeightFromLineNumber(variableLine.line);
                        Utilities.DrawSlot(drawingContext, p1);
                    }
                }
            }

            //OutputSlots
            p1 = pt;
            p1.X = this.Width;
            foreach (VariableLine variableLine in this.outputLines)
            {
                int slotIndex;
                if (this.OutputSlotIsVisible(variableLine.line, out slotIndex))
                {
                    p1.Y = singleLineHeight - Configurations.SlotSize / 2;
                    p1.Y += GetHeightFromLineNumber(variableLine.line);
                    DrawingUtilities.DrawDots(drawingContext, DotTypes.BottomRight | DotTypes.MiddleRight | DotTypes.TopRight, p1, AnchorPoint.TopRight, false);
                    p1.Offset(0, -singleLineHeight / 4);
                    drawingContext.DrawRectangle(Configurations.SlotHitTestColor, Configurations.NoBorderPen, new Rect(p1, new Size(Configurations.HittestPixels, Configurations.FontPixels)));
                }
            }

            //Preview
            if (previewBubble != null)
                this.previewBubble.Compose();
        }

        protected override void SwapAssignmentOnSlot(uint slotId)
        {
            //Find the inputLine
            int count = 0, statementIndex = -1, inputLineIndex = -1;
            int inputSlotIndex = this.GetSlotIndex(slotId);
            VariableLine inputLine = new VariableLine();
            foreach (KeyValuePair<int, List<VariableLine>> kvp in this.inputLines)
            {
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    if (count == inputSlotIndex)
                    {
                        inputLine = kvp.Value[i];
                        inputLineIndex = i;
                        break;
                    }
                    count++;
                }

                if (inputLineIndex != -1)
                {
                    statementIndex = kvp.Key;
                    break;
                }
            }

            //Find the outputLine
            int outputSlotIndex = 0;
            VariableLine outputLine = new VariableLine();
            foreach (VariableLine variableLine in this.outputLines)
            {
                if (variableLine.line == inputLine.line)
                {
                    outputLine = variableLine;
                    break;
                }
                outputSlotIndex++;
            }

            //Swap inputLine and outputLine
            List<VariableLine> variableLines = new List<VariableLine>();
            this.inputLines.TryGetValue(statementIndex, out variableLines);
            variableLines.RemoveAt(inputLineIndex);
            variableLines.Insert(inputLineIndex, outputLine);
            this.outputLines.RemoveAt(outputSlotIndex);
            this.outputLines.Insert(outputSlotIndex, inputLine);


            //Swap assignment in compilableText
            this.compilableText = Utilities.SwapAssignment(this.compilableText, inputLine.line);

            //Update variable mapping
            RuntimeStates runtimeStates = graphController.GetRuntimeStates();
            runtimeStates.UpdateVariablesDefinedInNode(this, false);
        }

        #endregion

        #region Private Class Methods

        private bool InputSlotIsVisible(int lineNumber, out int index)
        {
            if (lineNumber < 0)
                throw new ArgumentException("'lineNumber' is negative");

            index = CodeBlockNode.GetInputSlotIndexFromLineNumber(this.inputLines, lineNumber);
            if (index != -1)
            {
                uint slotId = this.inputSlots.ElementAt(index);
                return ((Slot)this.graphController.GetSlot(slotId)).Visible;
            }
            return false;
        }

        private bool OutputSlotIsVisible(int lineNumber, out int index)
        {
            if (lineNumber < 0)
                throw new ArgumentException("'lineNumber' is negative");

            index = CodeBlockNode.GetOutputSlotIndexFromLineNumber(this.outputLines, lineNumber);
            if (index != -1)
            {
                uint slotId = this.outputSlots.ElementAt(index);
                return ((Slot)this.graphController.GetSlot(slotId)).Visible;
            }
            return false;
        }

        private int[] GetAllLinesHavingInputConnection()
        {
            List<int> lines = new List<int>();
            foreach (uint slotId in this.inputSlots)
            {
                Slot slot = this.graphController.GetSlot(slotId) as Slot;
                if (slot.Visible)
                {
                    if (slot.ConnectingSlots != null && slot.ConnectingSlots.Length > 0)
                    {
                        int slotIndex = GetSlotIndex(slot.SlotId);
                        lines.Add(GetLineNumberFromInputSlotIndex(this.inputLines, slotIndex));
                    }
                }
            }
            return lines.ToArray();
        }

        private uint GetExistingInputSlotByName(string variable)
        {
            List<VariableLine> variableLines = new List<VariableLine>();
            foreach (KeyValuePair<int, List<VariableLine>> kvp in this.inputLines)
                variableLines.AddRange(kvp.Value);

            int index = 0;
            foreach (VariableLine variableLine in variableLines)
            {
                if (variableLine.variable == variable) // Found it!
                    return this.inputSlots[index];
                index++;
            }

            return uint.MaxValue; // Not found.
        }

        private uint GetExistingOutputSlotByName(string variable)
        {
            int index = 0;
            foreach (VariableLine variableLine in this.outputLines)
            {
                if (variableLine.variable == variable) // Found it!
                    return this.outputSlots[index];
                index++;
            }

            return uint.MaxValue; // Not found.
        }

        private List<string> GetReferencedVariablesForStatement(KeyValuePair<int, List<VariableLine>> statementLinesMapping)
        {
            if (statementLinesMapping.Equals(null))
                throw new ArgumentNullException("statementLinesMapping");

            List<string> referencedVariable = new List<string>();
            foreach (VariableLine statementLine in statementLinesMapping.Value)
                referencedVariable.Add(statementLine.variable);
            return referencedVariable;
        }

        private string GetDefinedVariablesForStatement(int statementNumber)
        {
            return outputLines.ElementAt(statementNumber).variable;
        }

        private Dictionary<int, List<VariableLine>> FormatNewInputLinesFromDicitionary(Dictionary<int, List<VariableLine>> dictionary)
        {
            if (dictionary == null)
                throw new ArgumentNullException("dictionary");

            Dictionary<int, List<VariableLine>> newDicitionary = new Dictionary<int, List<VariableLine>>();
            foreach (KeyValuePair<int, List<VariableLine>> kvp in dictionary)
            {
                List<VariableLine> list = new List<VariableLine>();
                foreach (VariableLine variableLine in kvp.Value)
                {
                    VariableLine newVariableLine = new VariableLine(variableLine.variable, variableLine.line - 1);
                    list.Add(newVariableLine);
                }
                newDicitionary.Add(kvp.Key - 1, list);
            }
            return newDicitionary;
        }

        private List<VariableLine> FormatNewOutputLinesFromHashSet(HashSet<VariableLine> hashSet)
        {
            if (hashSet == null)
                throw new ArgumentNullException("hashSet");

            List<VariableLine> list = new List<VariableLine>();
            foreach (VariableLine variableLine in hashSet)
            {
                VariableLine newVariableLine = new VariableLine(variableLine.variable, variableLine.line - 1);
                list.Add(newVariableLine);
            }
            return list;
        }

        private void SetInputSlotsAndLines(Dictionary<int, List<VariableLine>> newInputLines)
        {
            if (newInputLines == null)
                throw new ArgumentNullException("newInputLines");

            List<string> oldSetVariables = CodeBlockNode.GetReferencedVariables(this.inputLines, true);
            List<string> newSetVariables = CodeBlockNode.GetReferencedVariables(newInputLines, true);
            List<string> variablesToBeRemoved = oldSetVariables.Except(newSetVariables).ToList<string>();

            List<uint> newInputSlots = new List<uint>();
            foreach (string newVariable in newSetVariables)
            {
                uint slotId = this.GetExistingInputSlotByName(newVariable);
                if (slotId == uint.MaxValue)
                {
                    if (variablesToBeRemoved.Count > 0) //Assign unreferenced slot to new variable
                    {
                        int index = oldSetVariables.IndexOf(variablesToBeRemoved[0]);
                        slotId = this.inputSlots.ElementAt(index);
                        variablesToBeRemoved.RemoveAt(0);
                    }
                    else //Create new slot for new variable if there is no more unreferenced slot
                    {
                        ISlot slot = new Slot(this.graphController, SlotType.Input, this);
                        this.graphController.AddSlot(slot);
                        slotId = slot.SlotId;
                    }
                }
                newInputSlots.Add(slotId);
            }

            foreach (string variableToBeRemoved in variablesToBeRemoved) //Remove remaining unreferenced slot
            {
                int index = oldSetVariables.IndexOf(variableToBeRemoved);
                uint slotId = this.inputSlots.ElementAt(index);
                this.graphController.DeleteEdgesConnectTo(slotId);
                this.graphController.RemoveSlot(slotId);
            }

            //Replace old state with new state
            this.inputSlots = newInputSlots;
            this.inputLines = newInputLines;
        }

        private void SetOutputSlotsAndLines(List<VariableLine> newOutputLines)
        {
            if (newOutputLines == null)
                throw new ArgumentNullException("newOutputLines");

            // As it goes through and create fresh output slots, the CBN tries 
            // its best to retain existing output connections. However, those 
            // connections can only be preserved, only if the output variables
            // remain the same names after the change. In other word, this process 
            // is highly specific to names of variables being defined in the new 
            // content. To illustrate this process better, let's consider the 
            // following scenario:
            // 
            //      oldVariables = { a,   b,  }
            //      oldSlotIds   = { 301, 302 }
            // 
            //      newVariables = { b,   c,   d   }
            //      newSlotIds   = { 302, ???, ??? }
            // 
            List<string> newVariables = CodeBlockNode.GetDefinedVariables(newOutputLines, true);

            // Here we figure out what are the slot IDs the CBN currently has, and 
            // what variable names they map to. This map is important since there can 
            // be more than one slot mapping to the same variable (imagine a variable
            // is assigned multiple times, each of them will have one output slot, it
            // is just that we only display the first output slot as of this writing).
            // 
            // In our scenario, the map would have the following content:
            // 
            //      slotToNameMap = { [301, a], [302, b] }
            // 
            int i = 0;
            Dictionary<uint, string> slotToNameMap = new Dictionary<uint, string>();
            Dictionary<uint, string> slotsToRemove = new Dictionary<uint, string>();
            foreach (VariableLine variableLine in this.outputLines)
            {
                slotToNameMap.Add(this.outputSlots[i], variableLine.variable);
                slotsToRemove.Add(this.outputSlots[i], variableLine.variable);
                i++;
            }

            // Given the old list { a, b } and the new { b, c, d }, we then compute 
            // the delta and figure out which are the old ones we need to remove. In 
            // our example here the dictionary would contain a single element 'a':
            // 
            //      slotsToRemove = { [301, a] }
            // 
            foreach (string newVariable in newVariables)
            {
                // There may be more than one output slot with the same variable name 
                // (e.g. variable redefinition), so we will remove them one at a time.
                if (slotsToRemove.ContainsValue(newVariable))
                {
                    KeyValuePair<uint, string> foundSlot = slotsToRemove.First((x) => (x.Value == newVariable));
                    slotsToRemove.Remove(foundSlot.Key); // Remove the entry, we still need this variable.
                }
            }

            // New output slots to be generated in this loop.
            List<uint> newOutputSlots = new List<uint>();
            foreach (string newVariable in newVariables) // newSetVariables = { b, c, d }
            {
                // See if any of the { b, c, d } has an output slot before (only 'b' should have).
                // 
                uint slotId = uint.MaxValue;
                if (slotToNameMap.ContainsValue(newVariable))
                {
                    KeyValuePair<uint, string> foundSlot = slotToNameMap.First((x) => (x.Value == newVariable));
                    slotId = foundSlot.Key; // Found a slot to be reused, for 'b' it is '302'.
                    slotToNameMap.Remove(foundSlot.Key); // Now that's reused, remove it from map.
                }

                if (slotId == uint.MaxValue) // Variable does not match to any output slot.
                {
                    // No slot could be found for 'c', we know we need a new slot ID for it.
                    // But instead of go ahead to create a new slot, we'll see what can be 
                    // reused in "slotsToRemove = { [301, a] }" since we are going to remove 
                    // it. So here we found 'a', which are going to be removed, but can be 
                    // reused, so we reuse its slot ID '301' here.
                    // 
                    if (slotsToRemove.Count > 0)
                    {
                        // Here we found 'a' can be reused, so reuse slot ID '301'.
                        KeyValuePair<uint, string> slotToReuse = slotsToRemove.First();
                        slotsToRemove.Remove(slotToReuse.Key);
                        slotId = slotToReuse.Key;
                    }
                    else
                    {
                        // We will get here for new variable 'd', because after 'a' was 
                        // removed above, 'd' will not find any left over for it to take 
                        // from "slotsToRemove". In this case, a new slot object needs 
                        // to be created for 'd' (which is '304' in our case).
                        // 
                        ISlot slot = new Slot(this.graphController, SlotType.Output, this);
                        this.graphController.AddSlot(slot);
                        slotId = slot.SlotId; // Newly created slot object will have new ID.
                    }
                }

                newOutputSlots.Add(slotId); // In the end, newOutputSlots = { 302, 301, 304 }
            }

            // Say if we were not able to reuse all those old variables (e.g. when 
            // we have less new variables than old variables), then here we remove 
            // them, as well as the edges connecting to them. Of course this includes 
            // updating the content of another slot that is connecting to the slot we 
            // are removing.
            // 
            foreach (KeyValuePair<uint, string> slotToRemove in slotsToRemove)
            {
                this.graphController.DeleteEdgesConnectTo(slotToRemove.Key);
                this.graphController.RemoveSlot(slotToRemove.Key);
            }

            // Replace old state with new state
            this.outputLines = FilterArrayElementVariable(newOutputLines);
            this.outputSlots = PreventDumplicatedSlotId(newOutputSlots);
            EnsureOutputEdgeConnectivity();
        }

        private List<VariableLine> FilterArrayElementVariable(List<VariableLine> variableLines)
        {
            for (int i = 0; i < variableLines.Count; i++)
            {
                VariableLine variableLine = variableLines[i];
                string variable = variableLine.variable;
                if (variable.Contains('['))
                {
                    variableLine.variable = variable.Substring(0, variable.IndexOf('['));
                    variableLines[i] = variableLine;
                }
            }
            return variableLines;
        }

        private List<uint> PreventDumplicatedSlotId(List<uint> slotIds)
        {
            List<uint> subSlotIdList = new List<uint>();
            for (int i = 0; i < slotIds.Count; i++)
            {
                subSlotIdList = slotIds.GetRange(0, i);
                if (subSlotIdList.Contains(slotIds[i]))
                {
                    int dumplicateIdIndex = subSlotIdList.IndexOf(slotIds[i]);
                    ISlot slot = new Slot(this.graphController, SlotType.Output, this);
                    this.graphController.AddSlot(slot);
                    slotIds[dumplicateIdIndex] = slot.SlotId;
                }
            }
            return slotIds;
        }

        private void EnsureOutputEdgeConnectivity()
        {
            // Ensuring that remaining edges are connecting to a visible slot.
            // A slot can becomes invisible after node editing, consider the 
            // following case:
            // 
            //      [ a = 1; b = 2; ]       Both slots are visible in this case
            //                              and there's a connection from 'a'.
            // 
            //      [ a = 1; a = 2; ]       'b' is renamed to 'a', so the first
            //                              slot gets hidden by design. The edge 
            //                              is still connecting to the old slot 
            //                              since it still exists, except for 
            //                              the fact that it is hidden now. So 
            //                              the edge needs to be adjusted to go 
            //                              on the last slot represeting 'a'.
            // 
            int i = 0;
            Dictionary<uint, string> slotToNameMap = new Dictionary<uint, string>();
            foreach (VariableLine variableLine in this.outputLines)
            {
                slotToNameMap.Add(this.outputSlots[i++], variableLine.variable);
            }

            foreach (KeyValuePair<uint, string> slotToNamePair in slotToNameMap)
            {
                string name = slotToNamePair.Value;
                int count = slotToNameMap.Values.Count((x) => (x == name));
                if (count <= 1) // If there's only one slot for this given variable,
                    continue; // then we don't have to move its edge anywhere else.

                // Now we have more than one slot for this given variable. Make sure
                // the relevant edges are only connected to the last slot (the only 
                // one that should be visible for the same variable).
                // 
                KeyValuePair<uint, string> lastSlot = slotToNameMap.Last((x) => (x.Value == name));
                uint correctSlotId = lastSlot.Key;

                // If this is the last slot we're looking at, then no point checking 
                // further, since the edges connecting to it (if any) are not going 
                // anywhere.
                // 
                uint slotId = slotToNamePair.Key;
                if (slotId == correctSlotId)
                    continue;

                // Do we have any edges connecting to this output slot?
                List<VisualEdge> edges = this.graphController.FindEdgesConnectingTo(slotId);
                if (null == edges || (edges.Count <= 0))
                    continue; // There's no edge connected to it, we're safe.

                Slot wrongSlot = graphController.GetSlot(slotId) as Slot;
                Slot rightSlot = graphController.GetSlot(correctSlotId) as Slot;

                // We are about the move these edges to the rightful slot, back 'em up!
                UndoRedoRecorder undoRecorder = graphController.GetUndoRedoRecorder();
                undoRecorder.RecordEdgeModificationForUndo(edges.ToList<IVisualEdge>());

                foreach (VisualEdge edge in edges)
                {
                    // Update internal slot connectivity information.
                    Slot endSlot = graphController.GetSlot(edge.EndSlotId) as Slot;
                    Slot.Disconnect(wrongSlot, endSlot);
                    Slot.Connect(rightSlot, endSlot);

                    // Update edge to connect to the right slots.
                    edge.ReconnectSlots(correctSlotId, edge.EndSlotId);
                    edge.UpdateControlPoint();
                }
            }
        }

        private void SetInputSlotVisibility(List<VariableLine> variableLines, bool visible)
        {
            if (variableLines == null || variableLines.Count <= 0)
                throw new ArgumentException("variableLines");

            List<VariableLine> allVariableLines = new List<VariableLine>();
            foreach (KeyValuePair<int, List<VariableLine>> kvp in this.inputLines)
                allVariableLines.AddRange(kvp.Value);

            foreach (VariableLine variableLine in variableLines)
            {
                int slotIndex = allVariableLines.IndexOf(variableLine);
                uint slotId = this.inputSlots.ElementAt(slotIndex);
                ((Slot)this.graphController.GetSlot(slotId)).Visible = visible;
            }
        }

        private void SetOutputSlotVisibility(uint slotId, bool visible)
        {
            if (slotId == uint.MaxValue)
                throw new ArgumentException("slotId");

            ((Slot)this.graphController.GetSlot(slotId)).Visible = visible;
        }

        private void UpdateInputSlotVisibility()
        {
            foreach (KeyValuePair<int, List<VariableLine>> statementVariableLinesMap in this.inputLines)
            {
                string definedVariable = this.GetDefinedVariablesForStatement(statementVariableLinesMap.Key);
                List<string> referencedVariables = this.GetReferencedVariablesForStatement(statementVariableLinesMap);
                string referencedVariable = referencedVariables[0];
                // Case "a = b + c;" or "b + c;". Since there are more than one 
                // referenced variable on the right-hand-side, then no slot 
                // should be displayed (since both of the input slots will be 
                // overlapping visually anyway).
                if (statementVariableLinesMap.Value.Count > 1)
                {
                    this.SetInputSlotVisibility(statementVariableLinesMap.Value, false);
                    continue;
                }

                // Case "a = temp;". This is the case where user types "a;" in the CBN, 
                // and having an explicit edge connecting to its input slot (causing the 
                // assignment to be swapped from "temp = a;" to "a = temp;"). In this 
                // case, since there's already an explicit edge connecting to it, the 
                // slot should be displayed.
                if (Utilities.IsTempVariable(referencedVariable))
                {
                    this.SetInputSlotVisibility(statementVariableLinesMap.Value, true);
                    continue;
                }

                // Case "a = b;". In this case by design user should not be allowed to 
                // directly connect an edge to the input slot (even it exists for 'b').
                if (!Utilities.IsTempVariable(definedVariable))
                {
                    this.SetInputSlotVisibility(statementVariableLinesMap.Value, false);
                    continue;
                }

                // Case "temp = a;" If the referenced variable 'a' is not defined anywhere,
                // then the slot needs to be displayed so user has a chance to link to it.
                RuntimeStates runtimeStates = this.graphController.GetRuntimeStates();
                if (runtimeStates.HasVariableDefinitionNotYetDefined(referencedVariable))
                    this.SetInputSlotVisibility(statementVariableLinesMap.Value, true);
                else
                    this.SetInputSlotVisibility(statementVariableLinesMap.Value, false);
            }
        }

        private void UpdateOutputSlotVisibility()
        {
            if (null == this.outputSlots || (this.outputSlots.Count <= 0))
                return; // There's no output slot for now.

            int i = 0;
            List<string> variables = new List<string>();
            Dictionary<uint, bool> slotVisibilities = new Dictionary<uint, bool>();
            Dictionary<uint, string> slotToNameMap = new Dictionary<uint, string>();
            foreach (VariableLine variableLine in this.outputLines)
            {
                uint slotId = this.outputSlots[i++];
                variables.Add(variableLine.variable);
                slotVisibilities.Add(slotId, false); // Invisible by default.
                slotToNameMap.Add(slotId, variableLine.variable);
            }

            // Take in only unique names.
            variables = variables.Distinct().ToList();
            foreach (string variable in variables)
            {
                KeyValuePair<uint, string> lastElement = slotToNameMap.Last((x) => (x.Value == variable));
                slotVisibilities[lastElement.Key] = true; // Last element is visible.
            }

            foreach (KeyValuePair<uint, bool> slotVisibility in slotVisibilities)
            {
                uint slotId = slotVisibility.Key;
                bool visible = slotVisibility.Value;
                this.SetOutputSlotVisibility(slotId, visible);
            }
        }

        private bool UpdateCompilableText()
        {
            if (String.IsNullOrEmpty(this.Text))
                throw new ArgumentException("'this.Text' is null or empty");

            this.Text = this.Text.Replace("\r\n", "\n");
            string tempText = this.Text.Trim();
            if (tempText.Substring(tempText.Length - 1) != ";") //Append ';' if it is missing in the last statement
                this.Text += ";";

            this.compilableText = string.Empty;
            List<string> errors = new List<string>();
            List<string> textToFormat = new List<string>();
            //List<string> statements = new List<string>();
            List<string> statements = null;

            RuntimeStates runtimeStates = this.graphController.GetRuntimeStates();

            List<string> statementToFormat = new List<string>();
            GraphUtilities.CompileExpression(this.Text, out statements);
            
            if (GraphUtilities.BuildStatus.ErrorCount > 0)
                return false;
            if (statements.Count == 0)
            {
                return false;
            }


            /*if (statements.Count == 0)
            {
                this.compilableText = this.Text;
                return;
            }*/

            //Re-formating
            string intermediate = string.Empty;
            //foreach (string statement in statements)
            //    intermediate += statement;
            for (int i = 0; i < statements.Count; i++)
            {
                intermediate += statements[i];
                if (i < statements.Count - 1 && !statements[i].EndsWith("\n"))
                    intermediate += "\n";
            }
            char[] newlineSign = { '\n' };
            string[] lines = intermediate.Split(newlineSign, StringSplitOptions.None);

            //replace temp variables
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("%t"))
                {
                    string temporaryVariable = runtimeStates.GenerateTempVariable(uint.MaxValue);
                    this.compilableText += lines[i].Replace("%t", temporaryVariable);
                    lines[i] = lines[i].Replace("%t =", "");
                }
                else
                {
                    this.compilableText += lines[i];
                }
                if (i < lines.Length - 1)
                {
                    lines[i] += "\n";
                    this.compilableText += "\n";
                }
            }

            this.Text = string.Empty;
            for (int i = 0; i < lines.Length; i++)
            {
                List<string> toFormat = new List<string>();
                toFormat.Add(lines[i]);
                SmartFormatter.Instance.Format(toFormat);
                this.Text += SmartFormatter.Instance.FormattedOutput;
            }
            return true;
        }

        private void AmendInputOutputSlots(out string error, out ErrorType errorType)
        {
            if (String.IsNullOrEmpty(this.Text))
                throw new ArgumentException("'this.Text' is null or empty");

            error = string.Empty;
            errorType = ErrorType.None;

            bool noError = false;
            //List<ProtoCore.BuildData.WarningEntry> warnings = new List<ProtoCore.BuildData.WarningEntry>();
            List<string> errors = new List<string>();
            Dictionary<int, List<VariableLine>> tempInputLines = new Dictionary<int, List<VariableLine>>();
            HashSet<VariableLine> tempOutputLines = new HashSet<VariableLine>();

            //Loading, undo and redo does not require recomputation of the input and output. Editing require recomputation.
            if (graphController.FileLoadInProgress || graphController.IsInUndoRedoCommand)
            {
                //noError = GraphUtilities.GetInputOutputInfo(this.compilableText, warnings, out errors, null, null);
                noError = GraphUtilities.GetInputOutputInfo(this.compilableText, null, null);
                //warnings = GraphUtilities.BuildStatus.Warnings;
                foreach (var err in GraphUtilities.BuildStatus.Errors)
                {
                    errors.Add(err.Message);
                }
            }
            else
            {
                noError = this.UpdateCompilableText();

                if (noError)
                {
                    noError = GraphUtilities.GetInputOutputInfo(this.compilableText, tempInputLines, tempOutputLines);
                }
                //warnings = GraphUtilities.BuildStatus.Warnings;
                foreach (var err in GraphUtilities.BuildStatus.Errors)
                {
                    errors.Add(err.Message);
                }

                if (noError)
                {
                    Dictionary<int, List<VariableLine>> newInputLines = this.FormatNewInputLinesFromDicitionary(tempInputLines);
                    List<VariableLine> newOutputLines = this.FormatNewOutputLinesFromHashSet(tempOutputLines);

                    this.SetInputSlotsAndLines(newInputLines);
                    this.SetOutputSlotsAndLines(newOutputLines);
                }
                else
                {
                    // In erroneous condition, reset the input/output slots.
                    this.SetInputSlotsAndLines(new Dictionary<int, List<VariableLine>>());
                    this.SetOutputSlotsAndLines(new List<VariableLine>());
                }

                this.nodeState &= ~States.SuppressPreview;
                if (Utilities.IsLiteralValue(this.Text))
                    this.nodeState |= States.SuppressPreview;
            }

            if (!noError)
            {
                for (int i = 0; i < errors.Count; i++)
                {
                    error += errors[0];
                    if (i != errors.Count - 1)
                        error += "\n";
                }
                errorType = ErrorType.Syntax;
            }
        }

        private double GetHeightFromLineNumber(int lineNumber)
        {
            double height = 0;
            if (lineHeights != null && (lineNumber <= this.lineHeights.Count))
            {
                for (int i = 0; i < lineNumber; i++)
                    height += this.lineHeights[i];
            }
            return height;
        }

        private int GetLineNumberFromHeight(double height)
        {
            int lineNumber = 0;

            if (this.lineHeights != null)
            {
                for (int i = 0; i < this.lineHeights.Count; i++)
                {
                    height -= this.lineHeights[i];
                    if (height < 0)
                    {
                        lineNumber = i;
                        return lineNumber;
                    }
                }
            }
            return lineNumber;
        }

        private void GetLineHeights()
        {
            this.lineHeights = new List<double>();
            double singleLineHeight = Utilities.GetFormattedTextHeight("A");

            if (string.IsNullOrEmpty(this.Text) == false)
            {
                string[] arr = this.Text.Split('\n');
                foreach (string str in arr)
                {
                    if (str == "")
                        this.lineHeights.Add(singleLineHeight);
                    else
                        this.lineHeights.Add(Utilities.GetFormattedTextHeight(str));
                }
            }
        }

        #endregion
    }
}