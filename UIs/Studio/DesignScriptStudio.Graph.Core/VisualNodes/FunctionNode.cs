using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Globalization;
using System.Windows;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using GraphToDSCompiler;

namespace DesignScriptStudio.Graph.Core
{
    class FunctionNode : VisualNode
    {
        #region Class Data Members

        private List<string> argumentNames = new List<string>();
        private List<List<int>> replicationGuides = new List<List<int>>();
        private List<string> replicationGuideStrings = new List<string>();
        private List<int> replicationErrorIndex = new List<int>();

        #endregion

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
                this.argumentNames = CoreComponent.Instance.GetArgumentNames(this.Assembly, this.QualifiedName, this.ArgumentTypes);
                this.UpdateReturnTypeAndMemberType();

                replicationGuides.Clear();

                if (storage.PeekUnsignedLong() == FieldCode.ReplicationGuides)
                    this.LoadLegacyReplicationGuide(storage);
                else
                {
                    int replicationArgumentCount = storage.ReadInteger(FieldCode.ReplicationArgumentCount);
                    for (int i = 0; i < replicationArgumentCount; i++)
                    {
                        List<int> repGuidesForThisArgument = new List<int>();
                        int replicationIndexCount = storage.ReadInteger(FieldCode.ReplicationIndexCount);
                        for (int j = 0; j < replicationIndexCount; j++)
                            repGuidesForThisArgument.Add(storage.ReadInteger(FieldCode.ReplicationIndex));

                        replicationGuides.Add(repGuidesForThisArgument);
                    }
                }

                UpdateReplicationGuideStrings();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n Function node deserialization failed.");
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

                // Take for example, this case:
                // 
                //      foo(a<8><9>, b, c<7>)
                // 
                // The serialized replication guides would be as follow:
                //
                //      [RAC|3][RIC|2][RI|8][RI|9][RIC|0][RIC|1][RI|7]
                // 
                // Where:
                // 
                //      RAC = FieldCode.ReplicationArgumentCount
                //      RIC = FieldCode.ReplicationIndexCount
                //      RI  = FieldCode.ReplicationIndex
                // 
                storage.WriteInteger(FieldCode.ReplicationArgumentCount, this.replicationGuides.Count);
                foreach (List<int> list in replicationGuides)
                {
                    storage.WriteInteger(FieldCode.ReplicationIndexCount, list.Count);
                    foreach (int value in list)
                        storage.WriteInteger(FieldCode.ReplicationIndex, value);
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n Function node serialization failed.");
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

        internal FunctionNode(IGraphController graphController, string assembly, string qualifiedName, string argumentTypes)
            : base(graphController, NodeType.Function)
        {
            this.Text = this.graphController.GetRuntimeStates().GenerateTempVariable(this.nodeId);
            this.Assembly = assembly;
            this.QualifiedName = qualifiedName;
            this.ArgumentTypes = argumentTypes;
            this.argumentNames = CoreComponent.Instance.GetArgumentNames(this.Assembly, this.QualifiedName, this.ArgumentTypes);
            this.UpdateReturnTypeAndMemberType();

            int lastDot = 0;
            string className = string.Empty;
            switch (this.MemberType)
            {
                case LibraryItem.MemberType.Constructor:
                    lastDot = qualifiedName.LastIndexOf('.');
                    if (-1 == lastDot)
                        this.Caption = qualifiedName;
                    else
                        this.Caption = qualifiedName.Substring(0, lastDot);
                    break;
                case LibraryItem.MemberType.StaticMethod:
                    lastDot = qualifiedName.LastIndexOf('.');
                    if (-1 == lastDot)
                        this.Caption = qualifiedName;
                    else
                        this.Caption = qualifiedName.Substring(lastDot + 1);
                    break;
                case LibraryItem.MemberType.InstanceMethod:
                case LibraryItem.MemberType.InstanceProperty:
                    lastDot = qualifiedName.LastIndexOf('.');
                    this.Caption = qualifiedName.Substring(lastDot + 1);
                    break;
                default:
                    this.Caption = qualifiedName;
                    break;
            }

            //Input slot
            this.CreateInputSlotsFromArgumentTypes(this.ArgumentTypes);

            //Output slot
            Slot outputSlot = new Slot(graphController, SlotType.Output, this);
            this.graphController.AddSlot(outputSlot);
            this.outputSlots.Add(outputSlot.SlotId);


            this.replicationGuideStrings.Clear();
            this.replicationGuides.Clear();
            foreach (uint slot in this.inputSlots)
            {
                this.replicationGuides.Add(new List<int>());
                this.replicationGuideStrings.Add(string.Empty);
            }

            this.CheckDots();
        }

        internal FunctionNode(IGraphController graphController) : base(graphController) { }

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

        internal override string ToCode(out SnapshotNodeType type)
        {
            string replicationGuideString = string.Empty;
            int i = 0;
            foreach (uint slot in inputSlots)
            {
                List<int> tmp = new List<int>();
                int j = 0;

                foreach (int value in replicationGuides[i])
                {
                    tmp.Add(value);
                    j++;
                }
                replicationGuideString += SnapshotNode.CreateReplicationGuideText(tmp);

                // The delimiter for a group of replication guides is'%' 
                // Put all these characters in a definition structure or file
                replicationGuideString += GraphToDSCompiler.Constants.ReplicationGuideDelimiter;
                i++;
            }
            string assemblyPath = graphController.MapAssemblyPath(this.Assembly);
            // only external library need to convert the assembly path
            if (this.Assembly != assemblyPath)
                assemblyPath = GraphUtilities.ConvertAbsoluteToRelative(assemblyPath);

            if (this.MemberType == LibraryItem.MemberType.InstanceMethod || this.MemberType == LibraryItem.MemberType.InstanceProperty)
            {
                type = SnapshotNodeType.Method;
                string tempArgumentTypes = this.ArgumentTypes;
                // there is additional slot[0] with name "this" and type "this"
                // the "this" type need to be converted to proper class name(first part of the qualifiedName)
                //
                if (this.QualifiedName.Contains('.'))
                {
                    string className = this.QualifiedName.Substring(0, this.QualifiedName.IndexOf('.'));
                    tempArgumentTypes = tempArgumentTypes.Replace("this", className);
                }

                return (string.Format("{0};{1};{2};{3};{4}", assemblyPath, this.Caption, tempArgumentTypes, this.Text, replicationGuideString));
            }
            else
            {
                type = SnapshotNodeType.Function;
                return (string.Format("{0};{1};{2};{3};{4}", assemblyPath, this.QualifiedName, this.ArgumentTypes, this.Text, replicationGuideString));
            }
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

            double xMiddle = pt.X + this.centerLine;
            double yMiddle = pt.Y + (this.Height / 2);

            index = -1;

            if (this.IsWithinClickRegion(pt, x, y) && dotsFlag.HasFlag(MenuDots.NorthWest))
                return NodePart.NorthWest;

            pt.X += this.centerLine + 2;
            pt.Y += 2;

            if (this.IsWithinClickRegion(pt, x, y) && dotsFlag.HasFlag(MenuDots.North))
                return NodePart.North;

            pt = this.nodePosition;
            pt.X += this.Width - 2;
            pt.Y += 2;

            if (this.IsWithinClickRegion(pt, x, y) && dotsFlag.HasFlag(MenuDots.NorthEast))
                return NodePart.NorthEast;

            pt = this.nodePosition;
            pt.X += this.Width - 2;
            pt.Y += this.Height - Configurations.HittestPixels;

            pt = this.nodePosition;
            pt.X += this.centerLine + 2;
            pt.Y += this.Height - Configurations.TextVerticalOffset;

            if (this.IsWithinClickRegion(pt, x, y) && dotsFlag.HasFlag(MenuDots.South))
                return NodePart.South;

            pt = this.nodePosition;
            pt.Offset(0, 1);

            int replicationCount = this.inputSlots.Count;
            double replicationLine = pt.X + this.CenterLine - GetMaxReplicationTextWidth();

            if (y > pt.Y && y < pt.Y + this.Height)
            {
                if (x > pt.X - Configurations.OutputHittestPixels && x <= pt.X + Configurations.HittestPixels)
                {
                    double slotCount = this.inputSlots.Count;
                    double slotHeight = (this.Height) / slotCount;
                    index = (int)((y - pt.Y) / slotHeight);
                    if (this.inputSlots.Count == 0)
                    {
                        index = -1;
                        if (x >= pt.X && x < pt.X + Configurations.HittestPixels)
                            return NodePart.InputLabel;
                    }
                    return NodePart.InputSlot;
                }
                else if (x > pt.X + Configurations.HittestPixels && x < pt.X + this.CenterLine)
                {
                    double slotCount = this.inputSlots.Count;
                    double slotHeight = (this.Height) / slotCount;
                    index = (int)((y - pt.Y) / slotHeight);
                    if (index >= 0 && (index < Configurations.ArraySize))
                    {
                        if (slotCount > 1 && x > replicationLine)
                            return NodePart.ReplicationGuide;
                    }

                    return NodePart.InputLabel;
                }
                else if (x > pt.X + this.CenterLine && x < pt.X + Width - Configurations.HittestPixels)
                {
                    index = 0;
                    if (y > (pt.Y + Height / 2) && y < pt.Y + Height)
                        return NodePart.Text;
                    else
                        return NodePart.Caption;
                }
                else if (x > pt.X + Width - Configurations.HittestPixels && x < pt.X + Width + Configurations.OutputHittestPixels)
                {
                    if (y > pt.Y + Height / 2 && y < pt.Y + Height)
                    {
                        index = 0;
                        return NodePart.OutputSlot;
                    }
                    else
                        return NodePart.Caption;
                }
            }

            if (this.previewBubble != null)
            {
                pt = this.previewBubble.RectPosition;
                pt.Offset(this.nodePosition.X, this.nodePosition.Y);
                pt.Offset(this.previewBubble.Width - 2, 2);
                if (IsWithinClickRegion(pt, x, y))
                    return NodePart.PreviewNorthEast;
            }

            pt = this.nodePosition;
            if (y > pt.Y + this.Height + Configurations.InfoBubbleTopMargin)
                return NodePart.Preview;

            return NodePart.None;
        }

        internal override double GetBubbleXPosition()
        {
            return (this.Width + Configurations.FunctionNodeCenterLine + this.GetSlotWidth()) / 2;
        }

        internal void AddReplicationGuides()
        {

            foreach (List<int> list in replicationGuides)
                list.Add(1);

            UpdateReplicationGuideStrings();
            this.Dirty = true;
        }

        protected override string GetAssemblyCore()
        {
            return this.Assembly;
        }

        protected override void SetAssemblyCore(string assembly)
        {
            //this.Assembly = assembly;
        }

        internal void RemoveReplicationGuides()
        {
            replicationGuides.Clear();
            foreach (uint slot in this.inputSlots)
                replicationGuides.Add(new List<int>());

            UpdateReplicationGuideStrings();
            this.Dirty = true;
        }

        internal double GetMaxReplicationWidth()
        {
            return this.GetMaxReplicationTextWidth();
        }

        internal bool UpdateReplicationGuides(string replicationText, int index)
        {
            if (index < 0 || (index >= replicationGuideStrings.Count))
                throw new ArgumentException("Invalid replication guide index (A80EA706B907)", "index");

            this.replicationGuideStrings[index] = replicationText;
            this.Dirty = true;
            this.Compose();
            return true;
        }

        internal void ChangeConstructor(string qualifiedName, string argumentTypes, string argumentNames)
        {
            if (String.IsNullOrEmpty(qualifiedName))
                throw new ArgumentException("'qualifiedName' is null or empty");
            if (argumentTypes == null)
                throw new ArgumentException("'argumentTypes' is null");

            this.QualifiedName = qualifiedName;
            this.ArgumentTypes = argumentTypes;
            this.argumentNames = CoreComponent.Instance.GetArgumentNames(this.Assembly, this.QualifiedName, this.ArgumentTypes);
            this.UpdateReturnTypeAndMemberType();

            foreach (uint slotId in this.inputSlots)
            {
                this.graphController.DeleteEdgesConnectTo(slotId);
                this.graphController.RemoveSlot(slotId);
            }
            this.inputSlots.Clear();
            this.CreateInputSlotsFromArgumentTypes(this.ArgumentTypes);

            this.Dirty = true;
        }

        internal void ChangeMethod(string qualifiedName, string argumentTypes)
        {
            if (String.IsNullOrEmpty(qualifiedName))
                throw new ArgumentException("'qualifiedName' is null or empty");
            if (String.IsNullOrEmpty(argumentTypes))
                throw new ArgumentException("'argumentTypes' is null or empty");

            this.Caption = (qualifiedName.Split('.'))[1];
            this.QualifiedName = qualifiedName;
            this.ArgumentTypes = argumentTypes;
            this.argumentNames = CoreComponent.Instance.GetArgumentNames(this.Assembly, this.QualifiedName, this.ArgumentTypes);
            this.UpdateReturnTypeAndMemberType();

            for (int i = 1; i < this.inputSlots.Count; i++)
            {
                uint slotId = this.inputSlots[i];
                this.graphController.DeleteEdgesConnectTo(slotId);
                this.graphController.RemoveSlot(slotId);
            }
            uint slotToCarryOver = this.inputSlots[0];
            this.inputSlots.Clear();
            this.inputSlots.Add(slotToCarryOver);
            this.CreateInputSlotsFromArgumentTypes(this.ArgumentTypes.Substring(this.ArgumentTypes.IndexOf(',') + 1));

            this.Dirty = true;
        }

        internal bool GetAddReplicationState()
        {
            if (this.inputSlots.Count <= 1)
                return false; // We have less than 2 argument.

            return true;
        }

        internal bool GetRemoveReplicationState()
        {
            foreach (List<int> list in replicationGuides)
            {
                if (list.Count != 0)
                    return true;
            }
            return false;
        }

        internal string GetReplicationText(int index)
        {
            if (index < 0 || (index >= Configurations.ArraySize))
                return string.Empty;
            if (string.IsNullOrEmpty(replicationGuideStrings[index]))
                return string.Empty;

            return replicationGuideStrings[index];
        }

        internal bool SetReplicationText(int index, string text)
        {
            if (index < 0 || (index >= replicationGuides.Count))
                throw new ArgumentException("Invalid replication guide index (F01B703017EF)", "index");

            if (!ExtractReplicationGuideFromText(replicationGuides[index], text))
            {
                this.SetErrorValue("Invalid Replication Guides", ErrorType.Syntax);
                replicationErrorIndex.Add(index);
                return false;
            }

            this.SetErrorValue(null, ErrorType.Syntax);//clear the error
            replicationErrorIndex.Remove(index);
            UpdateReplicationGuideStrings();
            return true;
        }

        #endregion

        #region Protected Class Methods

        protected override void ComposeCore(DrawingContext drawingContext, DrawingVisual visual)
        {
            double textWidth = Utilities.GetTextWidth(this.Text);
            double captionWidth = Utilities.GetTextWidth(this.Caption);
            string tempText = string.Empty;
            if (this.Text.Count() > 25)
            {
                tempText = this.Text.Substring(0, 25);
                tempText += "...";
                textWidth = Utilities.GetTextWidth(tempText);
            }
            else
                tempText = this.Text;

            //CenterLine
            this.centerLine = Configurations.FunctionNodeCenterLine + this.GetSlotWidth() + (int)GetMaxReplicationTextWidth();

            //Node width
            if (textWidth > captionWidth)
                this.nodeDimension.Width = (int)(Configurations.NodeWidthFunction + this.centerLine + textWidth + 1);
            else
                this.nodeDimension.Width = (int)(Configurations.NodeWidthFunction + this.centerLine + captionWidth + 1);

            //Node height
            this.nodeDimension.Height = Configurations.NodeHeightFunction + 2;
            if (this.inputSlots.Count > 1)
                this.nodeDimension.Height = this.inputSlots.Count * Configurations.SlotStripeSize + 1;
            //the +2 is due to the 2 1px horizontal lines at the top and bottom of the node

            base.ComposeCore(drawingContext, visual);

            //Shade
            double offset = this.centerLine;
            double xOffset = Configurations.TextHorizontalOffset;
            Point p1 = new Point(offset, 1);
            Rect shaded = new Rect(p1, new Size(this.Width - offset, this.Height - 1));
            drawingContext.DrawRectangle(Configurations.RectGrey, Configurations.NoBorderPen, shaded);

            //Horzontal line
            p1 = new Point((int)offset + xOffset, Math.Ceiling(this.Height / 2));
            Point p2 = new Point((int)this.Width - xOffset, Math.Ceiling(this.Height / 2));
            p1.Offset(0, 0.5);
            p2.Offset(0, 0.5);
            drawingContext.DrawLine(Configurations.BorderPen, p1, p2);

            //Draw text
            Point pt = new Point(offset + (this.Width - offset) / 2 - captionWidth / 2, ((this.Height) / 4) - Configurations.TextSize / 2 - 3);
            Utilities.DrawText(drawingContext, this.Caption, pt, Configurations.TextNormalColor);
            pt.Y += ((this.Height) / 2); //-1
            pt.X = offset + (this.Width - offset) / 2 - textWidth / 2;
            Utilities.DrawBoldText(drawingContext, tempText, pt);

            pt.Y += this.Height / 2;
            pt.X += Configurations.TextOffset;

            //Vertical line
            p1 = new Point(offset, 1);
            p2 = new Point(offset, this.Height);
            p1.Offset(0.5, 0);
            p2.Offset(0.5, 0);
            drawingContext.DrawLine(Configurations.BorderPen, p1, p2);

            //Shade slots
            p1 = new Point(1, 1);
            Size slotSize = new Size(offset - 1, Configurations.SlotStripeSize);
            Rect slotRect = new Rect(p1, slotSize);
            if (this.inputSlots.Count == 1)
            {
                slotSize.Height = this.Height - 1;
                slotRect = new Rect(p1, slotSize);
                if (hoveredIndex == 0)
                {
                    drawingContext.DrawRectangle(Configurations.RectHighlighted, Configurations.NoBorderPen, slotRect);
                    hoveredIndex = int.MaxValue;
                }
                else
                    drawingContext.DrawRectangle(Configurations.RectWhite, Configurations.NoBorderPen, slotRect);
            }
            else
            {
                for (int i = 0; i < this.inputSlots.Count; i++)
                {
                    slotRect = new Rect(p1, slotSize);
                    if (hoveredIndex == i)
                    {
                        drawingContext.DrawRectangle(Configurations.RectHighlighted, Configurations.NoBorderPen, slotRect);
                        hoveredIndex = int.MaxValue;
                    }
                    else if (i % 2 == 0)
                        drawingContext.DrawRectangle(Configurations.RectWhite, Configurations.NoBorderPen, slotRect);
                    else
                        drawingContext.DrawRectangle(Configurations.SlotBackground, Configurations.NoBorderPen, slotRect);
                    int temp = (int)this.Height / this.inputSlots.Count;
                    p1.Y += temp;
                }
            }

            //DrawDots
            //NorthEast
            Point p = new Point(0, 0);
            p.Offset(this.Width, Configurations.ContextMenuMargin);
            if (dotsFlag.HasFlag(MenuDots.NorthEast))
                DrawingUtilities.DrawDots(drawingContext, DotTypes.Top | DotTypes.TopRight | DotTypes.MiddleRight, p, AnchorPoint.TopRight, false);
            //North
            p = new Point(0, 0);
            p.Offset(this.centerLine + Configurations.ContextMenuMargin, Configurations.ContextMenuMargin);
            if (dotsFlag.HasFlag(MenuDots.North))
                DrawingUtilities.DrawDots(drawingContext, DotTypes.TopLeft | DotTypes.Top | DotTypes.MiddleLeft, p, AnchorPoint.TopLeft, false);
            //South
            p = new Point(0, 0);
            p.Offset(this.centerLine + Configurations.ContextMenuMargin, this.Height);
            if (dotsFlag.HasFlag(MenuDots.South))
                DrawingUtilities.DrawDots(drawingContext, DotTypes.MiddleLeft | DotTypes.BottomLeft | DotTypes.Bottom, p, AnchorPoint.BottomLeft, false);
            //NorthWest
            p = new Point(0, 0);
            p.Offset(Configurations.ContextMenuMargin, Configurations.ContextMenuMargin);
            DrawingUtilities.DrawDots(drawingContext, DotTypes.TopLeft | DotTypes.Top | DotTypes.MiddleLeft, p, AnchorPoint.TopLeft, false);
            //OutputSlot
            p = new Point(this.Width, (this.Height / 4));
            p.Y += (this.Height / 2) - 1;
            DrawingUtilities.DrawDots(drawingContext, DotTypes.TopRight | DotTypes.MiddleRight | DotTypes.BottomRight, p, AnchorPoint.TopRight, false);
            //OutputSlotHittestPixels
            p.Y -= (this.Height / 4) - 1;
            drawingContext.DrawRectangle(Configurations.SlotHitTestColor, Configurations.NoBorderPen, new Rect(p, new Size(Configurations.HittestPixels, this.Height / 2)));

            //InputSlots//InputLabels
            int slotCount = this.inputSlots.Count;
            p1 = new Point(-1, 1);
            p2 = new Point(Configurations.TextHorizontalOffset + Configurations.TextOffset, 1);
            if (slotCount == 1)
            {
                p1.Y += (this.Height / 2) - (Configurations.SlotSize / 2);
                p2.Y += (this.Height / 2) - (Configurations.SlotSize / 2) - (Configurations.TextSize / 2);
                Utilities.DrawText(drawingContext, this.argumentNames[0], p2, Configurations.TextNormalColor);
                Utilities.DrawSlot(drawingContext, p1);
            }
            else if (slotCount > 1)
            {
                int j = 0;
                p1.Y += (Configurations.SlotStripeSize / 2) - (Configurations.SlotSize / 2);
                p2.Y += (Configurations.SlotStripeSize / 2) - (Configurations.SlotSize / 2) - (Configurations.TextSize / 2);
                foreach (uint slotId in this.inputSlots)
                {
                    Utilities.DrawSlot(drawingContext, p1);
                    Utilities.DrawText(drawingContext, this.argumentNames[j++], p2, Configurations.TextNormalColor);
                    p1.Y += Configurations.SlotStripeSize;
                    p2.Y += Configurations.SlotStripeSize;
                }
            }

            //Preview
            if (this.previewBubble != null)
                this.previewBubble.Compose();

            //Replication guides
            p = new Point(this.centerLine - 5, 2);
            double characterWidth = Utilities.GetTextWidth("8");
            p.X -= GetMaxReplicationTextWidth();
            p.Y += (Configurations.SlotStripeSize / 2) - (Configurations.SlotSize / 2) - (Configurations.TextSize / 2);
            Rect repRect = new Rect(new Size(Configurations.ReplicationGuideWidth, Configurations.SlotStripeSize));

            int argCount = this.inputSlots.Count;
            int index = 0;
            foreach (string str in replicationGuideStrings)
            {
                if (string.IsNullOrEmpty(str) && index < argCount)
                {
                    Point newPoint = p;
                    newPoint.Offset(Configurations.TextHorizontalOffset + 3, Configurations.SlotStripeSize / 2 - 2);
                    drawingContext.DrawRectangle(Configurations.SquareColor, null, new Rect(newPoint, new Size(4, 4)));
                }

                SolidColorBrush textColor = Configurations.TextNormalColor;
                //if (this.replicationErrorIndex.Contains(index) == true)
                //    textColor = Configurations.TextErrorColor;

                Utilities.DrawText(drawingContext, str, p, textColor);
                p.Y += Configurations.SlotStripeSize;
                index++;
            }
        }

        #endregion

        #region Private Class Methods

        internal static bool ExtractReplicationGuideFromText(List<int> guides, string text)
        {
            if (guides == null)
                throw new ArgumentNullException("guides", "EBBA035F7114");
            if (text == null)
                throw new ArgumentNullException("text", "0ED5F107B941");

            // @TODO(Joy): Call validate replication string here.
            guides.Clear(); // Clear before anything starts.

            List<int> intermediate = new List<int>();
            string[] parts = text.Split(new char[] { '<', '>' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts != null && (parts.Length > 0))
            {
                int result = 0;
                foreach (string part in parts)
                {
                    string partString = part.Trim(); // Remove whitespaces.
                    if (string.IsNullOrEmpty(partString))
                        continue;

                    if (!int.TryParse(partString, out result))
                        return false;
                    else if (result <= 0)
                        return false;

                    intermediate.Add(result);
                }
            }

            guides.AddRange(intermediate);
            return true;
        }

        private void UpdateReplicationGuideStrings()
        {
            replicationGuideStrings.Clear();
            foreach (List<int> list in replicationGuides)
            {
                string str = string.Empty;
                if (list != null && (list.Count > 0))
                {
                    foreach (int value in list)
                        str += string.Format("<{0}>", value.ToString());
                }
                replicationGuideStrings.Add(str);
            }
        }

        private double GetMaxReplicationTextWidth()
        {
            double maxWidth = 20.0;
            foreach (string str in replicationGuideStrings)
            {
                double width = Utilities.GetTextWidth(str);
                if (width > maxWidth)
                    maxWidth = width;
            }
            if (this.inputSlots.Count <= 1)
                maxWidth = 0.0;
            return maxWidth;
        }

        private bool VerifyReplicationGuide()
        {
            bool result = true;
            foreach (string str in replicationGuideStrings)
            {
                if (String.IsNullOrEmpty(str) == true)
                    result = true;
                else
                {
                    string tempString = str.Replace(" ", "");

                    for (int i = 0; i < tempString.Length; i = i + 3)
                    {
                        if ((tempString[i] != '<'))
                            return false;
                        if ((tempString[i + 2] != '>'))
                            return false;
                        int t;
                        result = int.TryParse(tempString[i + 1].ToString(), out t);
                        if (result == false)
                            return result;
                    }
                }
            }
            return result;
        }

        private int GetSlotWidth()
        {
            double highestWidth = double.MinValue;
            string longestArgName = "";
            for (int i = 0; i < this.argumentNames.Count; i++)
            {
                if (Utilities.GetTextWidth(this.argumentNames[i]) > Utilities.GetTextWidth(longestArgName))
                    longestArgName = this.argumentNames[i];
            }
            highestWidth = Utilities.GetTextWidth(longestArgName);
            if (highestWidth == double.MinValue)
                return 0;
            return (int)highestWidth;
        }

        private void UpdateReturnTypeAndMemberType()
        {
            LibraryItem libraryItem = CoreComponent.Instance.GetLibraryItem(this.Assembly, this.QualifiedName, this.ArgumentTypes);
            if (libraryItem != null)
            {
                this.ReturnType = libraryItem.ReturnType;
                //   this.ReturnType = MIrror.GetStaticTYype;
                this.MemberType = libraryItem.Type;
            }
            else
            {
                this.ReturnType = string.Empty;
                this.MemberType = LibraryItem.MemberType.None;
            }
        }

        private void CreateInputSlotsFromArgumentTypes(string argumentTypes)
        {
            replicationGuides.Clear();
            if (!String.IsNullOrEmpty(argumentTypes)) // If there's any argument being specified.
            {
                char[] delimiter = new char[] { ',' };
                string[] types = argumentTypes.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);

                if (null != types && (types.Length > 0))
                {
                    foreach (string type in types)
                    {
                        Slot inputSlot = new Slot(graphController, SlotType.Input, this);
                        this.graphController.AddSlot(inputSlot);
                        this.inputSlots.Add(inputSlot.SlotId);
                        replicationGuides.Add(new List<int>());
                    }
                    this.UpdateReplicationGuideStrings();
                }
            }
        }

        private void ComputeNodeDimensions(double textWidth, double captionWidth)
        {
            //CenterLine
            this.centerLine = Configurations.FunctionNodeCenterLine + this.GetSlotWidth();

            //Replication Guides
            foreach (int value in replicationGuides[0])
                centerLine += Configurations.ReplicationGuideWidth + Configurations.TriangleHeight;

            //Node width
            if (textWidth > captionWidth)
                this.nodeDimension.Width = (int)(Configurations.NodeWidthFunction + this.centerLine + textWidth + 1);
            else
                this.nodeDimension.Width = (int)(Configurations.NodeWidthFunction + this.centerLine + captionWidth + 1);

            //Node height
            this.nodeDimension.Height = Configurations.NodeHeightFunction + 2;
            if (this.inputSlots.Count > 1)
                this.nodeDimension.Height = this.inputSlots.Count * Configurations.SlotStripeSize + 1;
            //the +2 is due to the 2 1px horizontal lines at the top and bottom of the node
        }

        private void LoadLegacyReplicationGuide(IStorage storage)
        {
            for (int i = 0; i < Configurations.ArraySize; i++)
            {
                replicationGuides.Add(new List<int>());
                replicationGuideStrings.Add(string.Empty);
                for (int j = 0; j < Configurations.ArraySize; j++)
                {
                    int temp = storage.ReadInteger(FieldCode.ReplicationGuides);
                    if (temp != 0)
                        replicationGuides[i].Add(temp);
                }
            }

            int repGuideCount = Configurations.ArraySize - this.inputSlots.Count;
            replicationGuides.RemoveRange(this.inputSlots.Count, repGuideCount);
            replicationGuideStrings.RemoveRange(this.inputSlots.Count, repGuideCount);
        }

        #endregion
    }
}
