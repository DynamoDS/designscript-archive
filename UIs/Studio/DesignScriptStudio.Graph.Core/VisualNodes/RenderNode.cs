using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using GraphToDSCompiler;

namespace DesignScriptStudio.Graph.Core
{
    class RenderNode : VisualNode
    {
        #region Public Interface Methods

        public override bool Deserialize(IStorage storage)
        {
            if (storage == null)
                throw new ArgumentNullException("storage");

            try
            {
                // @TODO(Zx)DONE: I don't think this method should return false. It 
                // should call the base method and directly return its value.
                base.Deserialize(storage);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n Render node deserialization failed.");
                return false;
            }
        }

        public override bool Serialize(IStorage storage)
        {
            if (storage == null)
                throw new ArgumentNullException("storage");

            try
            {
                // @TODO(Zx)DONE: I don't think this method should return false. It 
                // should call the base method and directly return its value.
                base.Serialize(storage);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n Render node serialization failed.");
                return false;
            }
        }

        #endregion

        #region Internal Class Methods

        internal RenderNode(IGraphController graphController, int inputSlotCount)
            : base(graphController, NodeType.Render)
        {
            this.Text = string.Empty;
            this.Caption = string.Empty;
        }

        internal RenderNode(IGraphController graphController) : base(graphController) { }

        internal override string GetSlotName(SlotType slotType, uint slotId)
        {
            throw new NotImplementedException("Render node should not implement GetSlotName()");
        }

        internal override List<uint> GetSlotIdsByName(SlotType slotType, string variableName)
        {
            throw new NotImplementedException("Condensed node should not implement GetSlotIdByName()");
        }

        internal override List<string> GetDefinedVariables(bool includeTemporaryVariables)
        {
            return null;
        }

        internal override List<string> GetReferencedVariables(bool includeTemporaryVariables)
        {
            return null;
        }

        internal override string ToCode(out SnapshotNodeType type)
        {
            throw new NotImplementedException("Render node should not implement ToCode()");
        }

        internal override string PreprocessInputString(string input)
        {
            throw new NotImplementedException("Render node should not implement PreprocessInputString()");
        }

        internal override ErrorType ValidateInputString(out string error)
        {
            throw new NotImplementedException("Render node should not implement ValidateinputString()");
        }

        internal override NodePart HitTest(double x, double y, out int index)
        {
            System.Windows.Point pt = this.nodePosition;
            index = -1;

            if (this.IsWithinClickRegion(pt, x, y))
                return NodePart.NorthWest;

            pt = this.nodePosition;
            pt.X = pt.X + Configurations.NodeWidthRender + Utilities.GetTextWidth(this.Caption);

            if (this.IsWithinClickRegion(pt, x, y))
                return NodePart.NorthEast;

            return NodePart.Caption;
        }

        #endregion

        #region Protected Class Methods

        protected override void ComposeCore(System.Windows.Media.DrawingContext drawingContext, System.Windows.Media.DrawingVisual visual)
        {
            this.nodeDimension.Width = Configurations.NodeWidthRender;
            this.nodeDimension.Height = Configurations.NodeHeightRender;

            base.ComposeCore(drawingContext, visual);

            //NorthEast
            Point p = new Point(0, 0);
            p.Offset(Width, Configurations.ContextMenuMargin);
            DrawingUtilities.DrawDots(drawingContext, DotTypes.Top | DotTypes.TopRight | DotTypes.MiddleRight, p, AnchorPoint.TopRight, false);
            //NorthWest
            p = new Point(0, 0);
            p.Offset(Configurations.ContextMenuMargin, Configurations.ContextMenuMargin);
            DrawingUtilities.DrawDots(drawingContext, DotTypes.TopLeft | DotTypes.Top | DotTypes.MiddleLeft, p, AnchorPoint.TopLeft, false);
            //OutputSlots
            p = new Point(0, 0);
            p.Offset(Width, Height - Configurations.ContextMenuMargin);
            DrawingUtilities.DrawDots(drawingContext, DotTypes.TopRight | DotTypes.MiddleRight | DotTypes.BottomRight, p, AnchorPoint.BottomRight, false);
        }

        #endregion
    }
}
