using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Globalization;
using System.Windows;
using System.Windows.Media.Effects;
using GraphToDSCompiler;

namespace DesignScriptStudio.Graph.Core
{
    class DriverNode :VisualNode
    {
        #region Public Interface Methods

        public override bool Deserialize(IStorage storage)
        {
            if (storage == null)
                throw new ArgumentNullException("storage");

            try
            {
                base.Deserialize(storage);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n Driver node deserialization failed.");
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
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n Driver node serialization failed.");
                return false;
            }
        }

        #endregion

        #region Internal Class Methods

        internal DriverNode(IGraphController graphController, string text)
            : base(graphController, NodeType.Driver)
        {
            this.centerLine = Configurations.DriverNodeCenterLine;
            this.Caption = this.graphController.GetRuntimeStates().GenerateTempVariable(this.nodeId);
            this.Text = Configurations.DriverInitialTextValue;

            Slot outputSlot = new Slot(graphController, SlotType.Output, this);
            this.graphController.AddSlot(outputSlot);
            this.outputSlots.Add(outputSlot.SlotId);
        }

        internal DriverNode(IGraphController graphController) : base(graphController) { }

        internal override string GetSlotName(SlotType slotType, uint slotId)
        {
            if (slotType != SlotType.Output)
                throw new InvalidOperationException("'sloType' is not output");

            int slotIndex = this.outputSlots.IndexOf(slotId);
            if (slotIndex == -1)
                throw new InvalidOperationException("'slotId' not in 'outputSlot'");
            return this.Caption;
        }

        internal override List<uint> GetSlotIdsByName(SlotType slotType, string variableName)
        {
            if (slotType == SlotType.None)
                throw new InvalidOperationException("'sloType' is None");

            if (slotType == SlotType.Input)
                throw new InvalidOperationException("'sloType' is Input");

            if (slotType == SlotType.Output && variableName == this.Caption)
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
            if (includeTemporaryVariables || !Utilities.IsTempVariable(this.Caption))
                definedVariables.Add(this.Caption);
            return definedVariables;
        }

        internal override List<string> GetReferencedVariables(bool includeTemporaryVariables)
        {
            return new List<string>();
        }

        internal override string ToCode(out SnapshotNodeType type)
        {
            string code = string.Format("{0} = {1}", this.Caption, this.Text);
            type = SnapshotNodeType.CodeBlock;
            return code;
        }

        internal override string PreprocessInputString(string input)
        {
            return input.Trim();
        }

        internal override ErrorType ValidateInputString(out string error)
        {
            error = string.Empty;
            List<string> errors = new List<string>();
            SnapshotNodeType type = SnapshotNodeType.None;

            type = GraphUtilities.AnalyzeString(this.Text);
            if (type != SnapshotNodeType.Literal)
                errors.Add("Invalid value.");

            type = GraphUtilities.AnalyzeString(this.Caption);
            if (type != SnapshotNodeType.Identifier)
                errors.Add("Invalid variable name.");

            for (int i = 0; i < errors.Count; i++)
            {
                error += errors[i];
                if (i != errors.Count - 1)
                    error += "\n";
            }

            if (error == string.Empty)
                return ErrorType.None;
            return ErrorType.Syntax;
        }


        // @TODO(Joy): There is a lot of similarities between getting hit test codes 
        // and drawing each part of a node, let's discuss to see how these two can be 
        // folded into a common piece of functionality.
        internal override NodePart HitTest(double x, double y, out int index)
        {
            System.Windows.Point pt = this.nodePosition;

            index = 0;
            double xMiddle = pt.X + this.centerLine;
            double yMiddle = pt.Y + (this.Height / 2);

            if (this.IsWithinClickRegion(pt, x, y) && dotsFlag.HasFlag(MenuDots.NorthWest))
                return NodePart.NorthWest;

            pt = this.nodePosition;
            pt.X = pt.X + this.centerLine + 2;
            pt.Y = pt.Y + Configurations.TextVerticalOffset;

            if (this.IsWithinClickRegion(pt, x, y) && dotsFlag.HasFlag(MenuDots.North))
                return NodePart.North;

            pt = this.nodePosition;
            pt.X += this.Width;
            if (this.IsWithinClickRegion(pt, x, y) && dotsFlag.HasFlag(MenuDots.NorthEast))
                return NodePart.NorthEast;

            pt = this.nodePosition;
            if (x > pt.X - Configurations.HittestPixels && x < xMiddle)
                return NodePart.Caption;

            if (y > pt.Y && y < pt.Y + Height)
            {
                if (x > xMiddle && x < xMiddle + this.Width)
                {
                    if (x > pt.X + this.Width - Configurations.HittestPixels && x < pt.X + this.Width + Configurations.OutputHittestPixels)
                    {
                        index = 0;
                        return NodePart.OutputSlot;
                    }
                    return NodePart.Text;
                }
            }

            return NodePart.None;
        }

        #endregion

        #region Protected Class Methods

        protected override void ComposeCore(DrawingContext drawingContext, DrawingVisual visual)
        {
            double captionWidth = Utilities.GetTextWidth(this.Caption);
            double textWidth = Utilities.GetTextWidth(this.Text);
            string tempCaption = string.Empty;
            string tempText = string.Empty;

            if (this.Caption.Count() > 25)
            {
                tempCaption = this.Caption.Substring(0, 25);
                tempCaption += "...";
                captionWidth = Utilities.GetTextWidth(tempCaption);
            }
            else
                tempCaption = this.Caption;

            if (this.Text.Count() > 25)
            {
                tempText = this.Text.Substring(0, 25);
                tempText += "...";
                textWidth = Utilities.GetTextWidth(tempText);
            }
            else
                tempText = this.Text;

            if (this.Text == string.Empty)
                this.Text = Configurations.DriverInitialTextValue;

            this.centerLine = Configurations.DriverNodeCenterLine + captionWidth;
            this.nodeDimension.Width = Configurations.NodeWidthDriver + this.centerLine + textWidth;
            this.nodeDimension.Height = Configurations.NodeHeightDriver;

            base.ComposeCore(drawingContext, visual);
            double offset = this.centerLine;

            //Shade
            Rect shaded = new Rect(new Point(1, 1), new Size(offset, this.Height - 1));
            drawingContext.DrawRectangle(Configurations.RectGrey, Configurations.NoBorderPen, shaded);

            //Draw control dots
            //NorthEast
            Point p = new Point(0, 0);
            p.Offset(this.Width, Configurations.ContextMenuMargin);
            //DrawingUtilities.DrawDots(drawingContext, DotTypes.Top | DotTypes.TopRight | DotTypes.MiddleRight, p, AnchorPoint.TopRight, false);
            //OutputSlot
            p = new Point(0, 0);
            p.Offset(this.Width, ((this.Height - 2) / 2) - 1);
            DrawingUtilities.DrawDots(drawingContext, DotTypes.TopRight | DotTypes.MiddleRight | DotTypes.BottomRight, p, AnchorPoint.TopRight, false);
            p.Y -= Configurations.SlotSize;
            drawingContext.DrawRectangle(Configurations.SlotHitTestColor, Configurations.NoBorderPen, new Rect(p, new Size(Configurations.OutputHittestPixels, Configurations.SlotStripeSize)));
            //North
            p = new Point(0, 0);
            p.Offset(this.centerLine + 3, Configurations.ContextMenuMargin);
            //DrawingUtilities.DrawDots(drawingContext, DotTypes.TopLeft | DotTypes.Top | DotTypes.MiddleLeft, p, AnchorPoint.TopLeft, false);
            //NorthWest
            p = new Point(0, 0);
            p.Offset(Configurations.ContextMenuMargin, Configurations.ContextMenuMargin);
            //DrawingUtilities.DrawDots(drawingContext, DotTypes.TopLeft | DotTypes.Top | DotTypes.MiddleLeft, p, AnchorPoint.TopLeft, false);

            //Draw vertical line
            Point p1 = new Point((int)offset, 0);
            Point p2 = new Point((int)offset, (int)this.Height);
            p1.Offset(0.5, 0);
            p2.Offset(0.5, 0);
            drawingContext.DrawLine(Configurations.BorderPen, p1, p2);

            Utilities.DrawBoldText(drawingContext, tempCaption, new Point(this.centerLine / 2 - (captionWidth / 2), ((this.Height - 2) / 2) - Configurations.TextSize / 2));
            Utilities.DrawText(drawingContext, tempText, new Point(offset + (Configurations.NodeWidthDriver + textWidth) / 2 - textWidth / 2, ((this.Height - 2) / 2) - Configurations.TextSize / 2), Configurations.TextNormalColor);
        }

        #endregion
    }
}
