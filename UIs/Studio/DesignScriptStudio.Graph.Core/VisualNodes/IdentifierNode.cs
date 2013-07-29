using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using GraphToDSCompiler;

namespace DesignScriptStudio.Graph.Core
{
    class IdentifierNode : VisualNode
    {
        #region Class Data Members

        private VariableLine inputLines = new VariableLine();
        private VariableLine outputLines = new VariableLine();

        #endregion

        #region Public Interface Methods

        public override bool Deserialize(IStorage storage)
        {
            if (storage == null)
                throw new ArgumentNullException("storage");

            try
            {
                base.Deserialize(storage);

                string variable = storage.ReadString(FieldCode.Variable);
                int line = storage.ReadInteger(FieldCode.Line);
                this.inputLines = new VariableLine(variable, line);

                variable = storage.ReadString(FieldCode.Variable);
                line = storage.ReadInteger(FieldCode.Line);
                this.outputLines = new VariableLine(variable, line);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n Identifier node deserialization failed.");
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

                storage.WriteString(FieldCode.Variable, this.inputLines.variable);
                storage.WriteInteger(FieldCode.Line, this.inputLines.line);

                storage.WriteString(FieldCode.Variable, this.outputLines.variable);
                storage.WriteInteger(FieldCode.Line, this.outputLines.line);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n Identifier node serialization failed.");
                return false;
            }
        }

        #endregion

        #region Internal Class Properties
        internal string Assembly { get; private set; }
        #endregion

        #region Internal Class Methods

        internal IdentifierNode(IGraphController graphController, string caption)
            : base(graphController, NodeType.Identifier)
        {
            this.Assembly = string.Empty;
            this.ReturnType = string.Empty;
            this.Caption = this.graphController.GetRuntimeStates().GenerateTempVariable(this.nodeId);
            this.Text = string.Empty;
            this.UpdateInternalData(true);

            //1 input slot
            Slot newSlot = new Slot(graphController, SlotType.Input, this);
            this.graphController.AddSlot(newSlot);
            this.inputSlots.Add(newSlot.SlotId);

            //1 output slot
            newSlot = new Slot(graphController, SlotType.Output, this);
            this.graphController.AddSlot(newSlot);
            this.outputSlots.Add(newSlot.SlotId);
        }

        internal IdentifierNode(IGraphController graphController) : base(graphController) { }

        internal override string GetSlotName(SlotType slotType, uint slotId)
        {
            if (slotType == SlotType.Input)
            {
                int slotIndex = this.inputSlots.IndexOf(slotId);
                if (slotIndex == -1)
                    throw new InvalidOperationException("'slotId' not in 'inputSlot'");

                return this.inputLines.variable;
            }
            else if (slotType == SlotType.Output)
            {
                int slotIndex = this.outputSlots.IndexOf(slotId);
                if (slotIndex == -1)
                    throw new InvalidOperationException("'slotId' not in 'outputSlot'");

                return this.outputLines.variable;
            }
            else
                throw new InvalidOperationException("'slotType' is none");
        }

        internal override List<uint> GetSlotIdsByName(SlotType slotType, string variableName)
        {
            if (slotType == SlotType.None)
                throw new InvalidOperationException("'sloType' is none");

            if (slotType == SlotType.Input && variableName == inputLines.variable)
            {
                List<uint> inputSlotIds = new List<uint>();
                inputSlotIds.Add(inputSlots[0]);
                return inputSlotIds;
            }

            if (slotType == SlotType.Output && variableName == outputLines.variable)
            {
                List<uint> outputSlotIds = new List<uint>();
                outputSlotIds.Add(outputSlots[0]);
                return outputSlotIds;
            }

            throw new InvalidOperationException("'variableName' is not exist");
        }

        internal override List<string> GetDefinedVariables(bool includeTemporaryVariables)
        {
            List<string> definedVariables = new List<string>();
            if (includeTemporaryVariables || !Utilities.IsTempVariable(this.outputLines.variable))
                definedVariables.Add(this.outputLines.variable);
            return definedVariables;
        }

        internal override List<string> GetReferencedVariables(bool includeTemporaryVariables)
        {
            List<string> referencedVariables = new List<string>();
            if (includeTemporaryVariables || !Utilities.IsTempVariable(this.inputLines.variable))
                referencedVariables.Add(this.inputLines.variable);
            return referencedVariables;
        }

        internal override string ToCode(out SnapshotNodeType type)
        {
            string leftHandSideAssignment = this.outputLines.variable;
            if (Utilities.IsTempVariable(leftHandSideAssignment))
            {
                type = SnapshotNodeType.CodeBlock;
                return string.Format("{0} = {1}", leftHandSideAssignment, this.Caption);
            }
            else
            {
                type = SnapshotNodeType.Identifier;
                return string.Format("{0}", this.Caption);
            }
        }

        internal override string PreprocessInputString(string input)
        {
            return input.Trim();
        }

        internal override ErrorType ValidateInputString(out string error)
        {
            if (!this.graphController.FileLoadInProgress && !this.graphController.IsInUndoRedoCommand)
                this.UpdateInternalData(false);

            error = string.Empty;
            SnapshotNodeType type = GraphUtilities.AnalyzeString(this.Caption);

            if (type == SnapshotNodeType.Identifier)
                return ErrorType.None;
            else
            {
                error = "Invalid variable name.";
                return ErrorType.Syntax;
            }
        }

        internal override NodePart HitTest(double x, double y, out int index)
        {
            System.Windows.Point pt = this.nodePosition;
            index = -1;

            if (this.IsWithinClickRegion(pt, x, y) && dotsFlag.HasFlag(MenuDots.NorthWest))
                return NodePart.NorthWest;

            pt = this.nodePosition;
            pt.X = pt.X + Configurations.NodeWidthIdentifer + Utilities.GetTextWidth(this.Caption);

            if (this.IsWithinClickRegion(pt, x, y) && dotsFlag.HasFlag(MenuDots.NorthEast))
                return NodePart.NorthEast;

            pt = this.nodePosition;

            if (y > pt.Y && y < pt.Y + Height)
            {
                if (x > pt.X - Configurations.OutputHittestPixels && x < pt.X + Configurations.HittestPixels)
                {
                    if (this.InputSlotIsVisible())
                    {
                        index = 0;
                        return NodePart.InputSlot;
                    }
                    else
                        return NodePart.None;
                }
                if (x > pt.X + Width - Configurations.HittestPixels && x < pt.X + Width + Configurations.OutputHittestPixels)
                {
                    index = 0;
                    return NodePart.OutputSlot;
                }
            }

            if (this.previewBubble != null)
            {
                pt = this.previewBubble.RectPosition;
                pt.Offset(this.nodePosition.X, this.nodePosition.Y);
                pt.Offset(this.previewBubble.Width - 2, 2);
                if (this.IsWithinClickRegion(pt, x, y))
                    return NodePart.PreviewNorthEast;
            }

            pt = this.nodePosition;
            if (y > pt.Y + this.Height + Configurations.InfoBubbleTopMargin)
                return NodePart.Preview;

            pt = this.nodePosition;
            if (y > pt.Y + this.Height + Configurations.InfoBubbleTopMargin)
                return NodePart.Preview;

            return NodePart.Caption;
        }

        internal override void UpdateSlotVisibility()
        {
            this.UpdateInputSlotVisibility();
        }

        internal override void GetAssignmentStatements(List<AssignmentStatement> assignments)
        {
            string assignee = this.outputLines.variable;
            if (!Utilities.IsTempVariable(assignee))
                assignee = string.Empty;

            AssignmentStatement assignment = new AssignmentStatement(assignee);
            if (!Utilities.IsTempVariable(this.inputLines.variable))
                assignment.References.Add(this.inputLines.variable);

            assignments.Add(assignment);
        }

        protected override string GetAssemblyCore()
        {
            return this.Assembly;
        }

        protected override void SetAssemblyCore(string assembly)
        {
            //this.Assembly = assembly;
        }

        #endregion

        #region Protected Class Methods

        protected override void ComposeCore(System.Windows.Media.DrawingContext drawingContext, System.Windows.Media.DrawingVisual visual)
        {
            double captionWidth = Utilities.GetTextWidth(this.Caption);
            string tempText = string.Empty;
            if (this.Caption.Count() > 25)
            {
                tempText = this.Caption.Substring(0, 25);
                tempText += "...";
                captionWidth = Utilities.GetTextWidth(tempText);
            }
            else
                tempText = this.Caption;

            this.nodeDimension.Width = Configurations.NodeWidthIdentifer + captionWidth + 2;
            this.nodeDimension.Height = Configurations.NodeHeightIdentifier;
            base.ComposeCore(drawingContext, visual);
            RectangleGeometry roundedRect = new RectangleGeometry();

            Rect rect = new Rect(new Point(1, 1), new Size((int)(this.nodeDimension.Width - 1), (int)(this.nodeDimension.Height - 1)));
            drawingContext.DrawRectangle(Configurations.RectGrey, Configurations.NoBorderPen, rect);

            Point pt = new Point(Configurations.TextHorizontalOffset + 2, Configurations.TextVerticalOffset + 2);
            Utilities.DrawBoldText(drawingContext, tempText, pt);

            //NorthEast
            Point p = new Point(0, 0);
            p.Offset(this.Width, Configurations.ContextMenuMargin);
            if (dotsFlag.HasFlag(MenuDots.NorthEast))
                DrawingUtilities.DrawDots(drawingContext, DotTypes.Top | DotTypes.TopRight | DotTypes.MiddleRight, p, AnchorPoint.TopRight, false);
            //OutputSlot
            p = new Point(0, 0);
            p.Offset(this.Width, ((this.Height - 2) / 2) - 1);
            DrawingUtilities.DrawDots(drawingContext, DotTypes.BottomRight | DotTypes.MiddleRight | DotTypes.TopRight, p, AnchorPoint.TopRight, false);
            p.Y -= Configurations.SlotSize;
            drawingContext.DrawRectangle(Configurations.SlotHitTestColor, Configurations.NoBorderPen, new Rect(p, new Size(Configurations.OutputHittestPixels, Configurations.SlotStripeSize)));
            //NorthWest
            p = new Point(0, 0);
            p.Offset(Configurations.ContextMenuMargin, Configurations.ContextMenuMargin);
            if (dotsFlag.HasFlag(MenuDots.NorthWest))
                DrawingUtilities.DrawDots(drawingContext, DotTypes.TopLeft | DotTypes.Top | DotTypes.MiddleLeft, p, AnchorPoint.TopLeft, false);

            //InputSlots
            if (this.InputSlotIsVisible())
            {
                double y = Configurations.NodeHeightIdentifier / 2;
                y -= Configurations.SlotSize / 2;
                Point p1 = new Point(-1, y);
                Utilities.DrawSlot(drawingContext, p1);
            }

            //Preview bubble
            if (this.previewBubble != null)
                this.previewBubble.Compose();
        }

        protected override void SwapAssignmentOnSlot(uint slotId)
        {
            // Swap inputLines adn outputLines
            VariableLine variableLine = this.inputLines;
            this.inputLines = this.outputLines;
            this.outputLines = variableLine;

            // Update variable mapping
            RuntimeStates runtimeStates = graphController.GetRuntimeStates();
            runtimeStates.UpdateVariablesDefinedInNode(this, false);
        }

        #endregion

        #region Private Class Methods

        private bool InputSlotIsVisible()
        {
            uint slotId = this.inputSlots.ElementAt(0);
            return ((Slot)this.graphController.GetSlot(slotId)).Visible;
        }

        private void UpdateInputSlotVisibility()
        {
            string leftHandSideAssignment = this.outputLines.variable;
            if (Utilities.IsTempVariable(leftHandSideAssignment))
            {
                uint slotId = this.inputSlots.ElementAt(0);
                string righthandSideAssignment = this.inputLines.variable;

                if (!this.graphController.GetRuntimeStates().HasVariableDefinitionNotYetDefined(righthandSideAssignment))
                    ((Slot)this.graphController.GetSlot(slotId)).Visible = false;
                else
                    ((Slot)this.graphController.GetSlot(slotId)).Visible = true;

                this.Dirty = true;
            }
        }

        private void UpdateInternalData(bool calledFromConstructor)
        {
            if (false != calledFromConstructor)
            {
                string tempVariable = this.graphController.GetRuntimeStates().GenerateTempVariable(uint.MaxValue);
                this.inputLines = new VariableLine(this.Caption, 0);
                this.outputLines = new VariableLine(tempVariable, 0);
            }
            else
            {
                // Fix: IDE-1899 Rename identifier node displays warning and clears preview value.
                // This method is called at the end of a node-edit operation. For identifier node 
                // the user is allowed only to rename the Caption of the node, which can either be 
                // in "inputLines" (e.g. "t1 = Var1") or the "outputLines" (e.g. "Var1 = t1"), 
                // depending on the connection of the input slot. Here we figure out where the 
                // "Caption" resides and update the variable name accordingly.
                // 
                bool inputIsTemp = Utilities.IsTempVariable(inputLines.variable);
                bool outputIsTemp = Utilities.IsTempVariable(outputLines.variable);
                if (!(inputIsTemp ^ outputIsTemp))
                    throw new InvalidOperationException("Either input or output can/must be temp (BBC71AACEE95)!");

                if (false == inputIsTemp) // "inputLines" has the "Caption".
                    inputLines.variable = this.Caption;
                else // "outputLines" has the "Caption".
                    outputLines.variable = this.Caption;

                // Update variable mapping
                RuntimeStates runtimeStates = graphController.GetRuntimeStates();
                runtimeStates.UpdateVariablesDefinedInNode(this, false);
            }
        }

        #endregion
    }
}
