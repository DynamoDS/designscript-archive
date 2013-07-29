using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using GraphToDSCompiler;

namespace DesignScriptStudio.Graph.Core
{
    class PropertyNode : VisualNode
    {
        #region Public Interface Methods

        public override bool Deserialize(IStorage storage)
        {
            if (storage == null)
                throw new ArgumentNullException("storage");

            try
            {
                base.Deserialize(storage);
                this.Assembly = storage.ReadString(FieldCode.Assembly);
                this.QualifiedName = storage.ReadString(FieldCode.QualifiedName);
                this.ArgumentTypes = storage.ReadString(FieldCode.ArgumentTypes);
                this.UpdateReturnTypeAndMemberType();
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
                storage.WriteString(FieldCode.Assembly, this.Assembly);
                storage.WriteString(FieldCode.QualifiedName, this.QualifiedName);
                storage.WriteString(FieldCode.ArgumentTypes, this.ArgumentTypes);
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
        internal string QualifiedName { get; private set; }
        internal string ArgumentTypes { get; private set; }

        #endregion

        #region Internal Class Methods

        internal PropertyNode(IGraphController graphController, string assembly, string qualifiedName, string argumentTypes)
            : base(graphController, NodeType.Property)
        {
            this.Caption = qualifiedName;
            string[] splitQualifiedName = qualifiedName.Split('.');
            if (null != splitQualifiedName && (splitQualifiedName.Length >= 1))
                this.Caption = splitQualifiedName[1];
            this.Text = this.graphController.GetRuntimeStates().GenerateTempVariable(this.nodeId);

            this.Assembly = assembly;
            this.QualifiedName = qualifiedName;
            this.ArgumentTypes = argumentTypes;
            this.ReturnType = string.Empty;
            this.UpdateReturnTypeAndMemberType();
            this.UpdateInputSlot();

            //Output slot
            Slot outputSlot = new Slot(graphController, SlotType.Output, this);
            this.graphController.AddSlot(outputSlot);
            this.outputSlots.Add(outputSlot.SlotId);
            this.CheckDots();
        }

        internal PropertyNode(IGraphController graphController) : base(graphController) { }

        internal override string GetSlotName(SlotType slotType, uint slotId)
        {
            if (slotType == SlotType.Input)
            {
                int slotIndex = this.inputSlots.IndexOf(slotId);
                if (slotIndex == -1)
                    throw new InvalidOperationException("'slotId' not in 'inputSlot'");
            }
            else if (slotType == SlotType.Output)
            {
                int slotIndex = this.outputSlots.IndexOf(slotId);
                if (slotIndex == -1)
                    throw new InvalidOperationException("'slotId' not in 'outputSlot'");
            }
            else
                throw new InvalidOperationException("'slotType' is none");

            return this.Text;
        }

        internal override List<uint> GetSlotIdsByName(SlotType slotType, string variableName)
        {
            if (slotType == SlotType.None)
                throw new InvalidOperationException("'sloType' is None");

            if (slotType == SlotType.Input)
                throw new InvalidOperationException("'sloType' is Input");

            if (slotType == SlotType.Output && variableName == this.Text)
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
            if (includeTemporaryVariables || !Utilities.IsTempVariable(this.Text))
                definedVariables.Add(this.Text);
            return definedVariables;
        }

        internal override List<string> GetReferencedVariables(bool includeTemporaryVariables)
        {
            return new List<string>();
        }

        internal override string ToCode(out GraphToDSCompiler.SnapshotNodeType type)
        {
            string assemblyPath = graphController.MapAssemblyPath(this.Assembly);
            // only external library need to convert the assembly path
            if (this.Assembly != assemblyPath)
                assemblyPath = GraphUtilities.ConvertAbsoluteToRelative(assemblyPath);

            type = GraphToDSCompiler.SnapshotNodeType.Property;
            if (this.MemberType == LibraryItem.MemberType.InstanceProperty)
                return (string.Format("{0};{1};{2}", assemblyPath, this.Caption, this.Text));
            else  //this.MemberType == LibraryItem.MemberType.StaticProperty
                return (string.Format("{0};{1};{2}", assemblyPath, this.QualifiedName, this.Text));
        }

        internal override string PreprocessInputString(string input)
        {
            return input.Trim();
        }

        internal override ErrorType ValidateInputString(out string error)
        {
            error = string.Empty;
            SnapshotNodeType type = GraphUtilities.AnalyzeString(this.Text);

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

            pt.Y += this.Height;

            if (this.IsWithinClickRegion(pt, x, y) && dotsFlag.HasFlag(MenuDots.South))
                return NodePart.South;

            pt = this.nodePosition;
            pt.X += this.Width;

            if (this.IsWithinClickRegion(pt, x, y) && dotsFlag.HasFlag(MenuDots.NorthEast))
                return NodePart.NorthEast;

            pt = this.nodePosition;

            if (y > pt.Y && y < pt.Y + this.Height)
            {
                if (x > pt.X - Configurations.OutputHittestPixels && x < pt.X + Configurations.HittestPixels)
                {
                    if (y > pt.Y && y < pt.Y + this.Height / 2 && this.inputSlots.Count != 0)
                    {
                        index = 0;
                        return NodePart.InputSlot;
                    }
                    else
                        return NodePart.Text;
                }
                else if (x >= pt.X + Configurations.HittestPixels && x < pt.X + this.Width - Configurations.HittestPixels)
                {
                    if (y > pt.Y && y < pt.Y + this.Height / 2)
                        return NodePart.Caption;
                    else
                        return NodePart.Text;
                }
                else if (x >= pt.X + this.Width - Configurations.HittestPixels && x < pt.X + this.Width + Configurations.OutputHittestPixels)
                {
                    if (y > pt.Y && y < pt.Y + this.Height / 2)
                        return NodePart.Caption;
                    else
                    {
                        index = 0;
                        return NodePart.OutputSlot;
                    }
                }
            }

            return NodePart.None;
        }

        protected override string GetAssemblyCore()
        {
            return this.Assembly;
        }

        protected override void SetAssemblyCore(string assembly)
        {
            this.Assembly = assembly;
        }

        #endregion

        #region Protected Class Methods

        protected override void ComposeCore(System.Windows.Media.DrawingContext drawingContext, System.Windows.Media.DrawingVisual visual)
        {
            //Text and Caption size
            string tempText = string.Empty;
            double textWidth = Utilities.GetTextWidth(this.Text);
            double captionWidth = Utilities.GetTextWidth(this.Caption);
            if (this.Text.Count() > 25)
            {
                tempText = this.Text.Substring(0, 25);
                tempText += "...";
                textWidth = Utilities.GetTextWidth(tempText);
            }
            else
                tempText = this.Text;

            //Node width
            if (textWidth > captionWidth)
                this.nodeDimension.Width = (int)(Configurations.NodeWidthProperty + textWidth + 1);
            else
                this.nodeDimension.Width = (int)(Configurations.NodeWidthProperty + captionWidth + 1);

            //Node height
            this.nodeDimension.Height = Configurations.NodeHeightProperty;

            base.ComposeCore(drawingContext, visual);

            Rect rect = new Rect(new Point(1, 1), new Size((int)(this.nodeDimension.Width - 1), (int)(this.nodeDimension.Height - 1)));
            drawingContext.DrawRectangle(Configurations.RectGrey, Configurations.NoBorderPen, rect);

            //horizontal Line
            Point p1 = new Point(1, 1);
            p1.Offset(Configurations.TextHorizontalOffset, (int)(this.Height) / 2);
            p1.Offset(0, 0.5);
            Point p2 = p1;
            p2.X = (this.Width - 1) - Configurations.TextHorizontalOffset;
            drawingContext.DrawLine(Configurations.BorderPen, p1, p2);

            //draw caption and text
            p1 = new Point(1, 1);
            p1.Offset(0, Configurations.TextVerticalOffset);
            p1.X = (this.Width / 2) - captionWidth / 2;
            Utilities.DrawText(drawingContext, this.Caption, p1, Configurations.TextNormalColor);
            p1.Y = ((this.Height - 3) / 2) + Configurations.TextVerticalOffset;
            p1.X = (this.Width / 2) - textWidth / 2;
            Utilities.DrawBoldText(drawingContext, tempText, p1);

            //OutputSlot
            p1 = new Point(this.Width, (this.Height / 4));
            p1.Y += (this.Height / 2) - 2;
            DrawingUtilities.DrawDots(drawingContext, DotTypes.TopRight | DotTypes.MiddleRight | DotTypes.BottomRight, p1, AnchorPoint.TopRight, false);
            //OutputSlotHittestPixels
            p1.Y -= (this.Height / 4) - 1;
            drawingContext.DrawRectangle(Configurations.SlotHitTestColor, Configurations.NoBorderPen, new Rect(p1, new Size(Configurations.HittestPixels, this.Height / 2)));

            //NorthEast
            Point p = new Point(0, 0);
            p.Offset(this.Width, Configurations.ContextMenuMargin);
            if (dotsFlag.HasFlag(MenuDots.NorthEast))
                DrawingUtilities.DrawDots(drawingContext, DotTypes.Top | DotTypes.TopRight | DotTypes.MiddleRight, p, AnchorPoint.TopRight, false);

            //NorthWest
            p = new Point(0, 0);
            p.Offset(Configurations.ContextMenuMargin, Configurations.ContextMenuMargin);
            //DrawingUtilities.DrawDots(drawingContext, DotTypes.TopLeft | DotTypes.Top | DotTypes.MiddleLeft, p, AnchorPoint.TopLeft, false);

            //South
            p = new Point(0, 0);
            p.Offset(Configurations.ContextMenuMargin, this.Height);
            //DrawingUtilities.DrawDots(drawingContext, DotTypes.MiddleLeft | DotTypes.BottomLeft | DotTypes.Bottom, p, AnchorPoint.BottomLeft, false);

            //input Slots
            if (this.inputSlots.Count != 0)
            {
                p1 = new Point(-1, 1);
                p1.Y += Configurations.TextVerticalOffset + Configurations.SlotSize / 2;
                Utilities.DrawSlot(drawingContext, p1);
            }

            // Fix: IDE-1616 Geometry property previews are not showing up.
            // Fix: IDE-1623 Preview not coming for the property nodes.
            if (previewBubble != null)
                this.previewBubble.Compose();
        }

        #endregion

        #region Private Class Methods

        private void UpdateReturnTypeAndMemberType()
        {
            LibraryItem libraryItem = CoreComponent.Instance.GetLibraryItem(this.Assembly, this.QualifiedName, this.ArgumentTypes);
            if (libraryItem != null)
            {
                this.ReturnType = libraryItem.ReturnType;
                this.MemberType = libraryItem.Type;
            }
            else
            {
                this.ReturnType = string.Empty;
                this.MemberType = LibraryItem.MemberType.None;
            }
        }

        private void UpdateInputSlot()
        {
            LibraryItem libraryItem = CoreComponent.Instance.GetLibraryItem(this.Assembly, this.QualifiedName, this.ArgumentTypes);
            if (libraryItem.Type == LibraryItem.MemberType.InstanceProperty)
            {
                //1 Input slot 
                Slot newSlot = new Slot(graphController, SlotType.Input, this);
                this.graphController.AddSlot(newSlot);
                this.inputSlots.Add(newSlot.SlotId);
            }
            else if (this.ArgumentTypes.Count() != 0)
            {
                Slot newSlot = new Slot(graphController, SlotType.Input, this);
                this.graphController.AddSlot(newSlot);
                this.inputSlots.Add(newSlot.SlotId);
            }
        }

        #endregion
    }
}
