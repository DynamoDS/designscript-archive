using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Globalization;
using System.IO;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using GraphToDSCompiler;

namespace DesignScriptStudio.Graph.Core
{
    abstract class VisualNode : IVisualNode
    {
        //@TODO(Ben): Rename to 'InternalVersion'
        protected enum Version { Version0, Current = Version0 }
        [Flags]
        protected enum MenuDots
        {
            None = 0,
            North = 1,
            NorthEast = 2,
            NorthWest = 4,
            South = 8,
            DotsFlag = 16
        }

        #region Class Data Members

        protected GraphController graphController = null;

        // @TODO(Joy): Perhaps remove this and use "Type" property below?
        private NodeType nodeType = NodeType.None;

        /// <summary>
        /// The selected item being editted
        /// </summary>
        private NodePart partSelectedForEdit = NodePart.None;

        /// <summary>
        /// The current state of the contextual menus?
        /// </summary>
        private NodePart partDisplayingMenu = NodePart.None;
        private int replicationIndex = int.MinValue;

        //Non-persisted data members
        protected ErrorBubble errorBubble = null;
        protected PreviewBubble previewBubble = null;
        protected int hoveredIndex = int.MaxValue;
        protected MenuDots dotsFlag;

        //Persisted data members
        protected VisualNode.Version version;
        protected uint nodeId = uint.MaxValue;
        protected States nodeState = States.None;
        protected System.Windows.Size nodeDimension = new Size();
        protected System.Windows.Point nodePosition = new Point();
        protected double centerLine = 0.0;
        protected List<uint> inputSlots = new List<uint>();
        protected List<uint> outputSlots = new List<uint>();

        #endregion

        #region Public Interface Methods

        public uint[] GetInputSlots()
        {
            if (inputSlots.Count <= 0)
                return null;

            uint[] slots = new uint[this.inputSlots.Count];
            inputSlots.CopyTo(slots);
            return slots;
        }

        public uint[] GetOutputSlots()
        {
            if (outputSlots.Count <= 0)
                return null;
            uint[] slots = new uint[this.outputSlots.Count];
            outputSlots.CopyTo(slots);
            return slots;
        }

        public uint GetInputSlot(int index)
        {
            if (inputSlots.Count <= 0 || index < 0)
                return uint.MaxValue;
            if (index < 0 || index >= inputSlots.Count)
                throw new IndexOutOfRangeException();

            if (((Slot)graphController.GetSlot(inputSlots[index])).Visible)
                return inputSlots[index];
            else
                return uint.MaxValue;
        }

        public uint GetOutputSlot(int index)
        {
            if (outputSlots.Count <= 0 || (index < 0))
                return uint.MaxValue;
            if (index < 0 || (index >= outputSlots.Count))
                throw new IndexOutOfRangeException();

            return outputSlots[index];
        }

        public IEnumerable<IVisualNode> EnumerateInputs(ConnectionType connection)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IVisualNode> EnumerateOutputs(ConnectionType connection)
        {
            throw new NotImplementedException();
        }

        public virtual bool Deserialize(IStorage storage)
        {
            if (storage == null)
                throw new ArgumentNullException("storage");

            if (storage.ReadUnsignedInteger(FieldCode.NodeSignature) != Configurations.NodeSignature)
                throw new InvalidOperationException("Invalid input data");

            try
            {
                this.nodeType = (NodeType)storage.ReadInteger(FieldCode.NodeType);
                this.version = (VisualNode.Version)storage.ReadInteger(FieldCode.NodeVersion);
                this.nodeId = storage.ReadUnsignedInteger(FieldCode.NodeId);
                this.nodeState = (States)storage.ReadInteger(FieldCode.NodeState);
                this.Text = storage.ReadString(FieldCode.Text);
                this.Caption = storage.ReadString(FieldCode.Caption);
                this.nodePosition.X = storage.ReadDouble(FieldCode.NodePositionX);
                this.nodePosition.Y = storage.ReadDouble(FieldCode.NodePositionY);

                int inputSlotsCount = storage.ReadInteger(FieldCode.InputSlotsCount);
                this.inputSlots.Clear();
                for (int i = 0; i < inputSlotsCount; i++)
                    this.inputSlots.Add(storage.ReadUnsignedInteger(FieldCode.InputSlots));

                int outputSlotsCount = storage.ReadInteger(FieldCode.OutputSlotsCount);
                this.outputSlots.Clear();
                for (int j = 0; j < outputSlotsCount; j++)
                    this.outputSlots.Add(storage.ReadUnsignedInteger(FieldCode.OutputSlots));

                this.Dirty = true; // Mark as dirty for painting.
                return true;
            }
            catch (Exception e)
            {
                //@TODO(Zx): Move this to error handler
                Console.WriteLine(e.Message + "\n Visual node deserialization failed.");
                return false;
            }
        }

        public virtual bool Serialize(IStorage storage)
        {
            if (storage == null)
                throw new ArgumentNullException("storage");

            try
            {
                storage.WriteUnsignedInteger(FieldCode.NodeSignature, Configurations.NodeSignature);
                storage.WriteInteger(FieldCode.NodeType, (int)this.nodeType);
                storage.WriteInteger(FieldCode.NodeVersion, (int)VisualNode.Version.Current);
                storage.WriteUnsignedInteger(FieldCode.NodeId, this.nodeId);
                storage.WriteInteger(FieldCode.NodeState, (int)this.nodeState);
                storage.WriteString(FieldCode.Text, this.Text);
                storage.WriteString(FieldCode.Caption, this.Caption);
                storage.WriteDouble(FieldCode.NodePositionX, this.nodePosition.X);
                storage.WriteDouble(FieldCode.NodePositionY, this.nodePosition.Y);

                storage.WriteInteger(FieldCode.InputSlotsCount, this.inputSlots.Count);
                foreach (uint i in this.inputSlots)
                    storage.WriteUnsignedInteger(FieldCode.InputSlots, i);

                storage.WriteInteger(FieldCode.OutputSlotsCount, this.outputSlots.Count);
                foreach (uint j in this.outputSlots)
                    storage.WriteUnsignedInteger(FieldCode.OutputSlots, j);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n Visual node serialization failed.");
                return false;
            }
        }

        public virtual AuditStatus Audit()
        {
            return AuditStatus.NoChange;
        }

        #endregion

        #region Public Interface Properties

        public double X
        {
            get { return this.nodePosition.X; }
            set { this.nodePosition.X = value; }
        }

        public double Y
        {
            get { return this.nodePosition.Y; }
            set { this.nodePosition.Y = value; }
        }

        public double Width
        {
            get { return this.nodeDimension.Width; }
        }

        public double Height
        {
            get { return this.nodeDimension.Height; }
        }

        public double CenterLine
        {
            get { return this.centerLine; }
        }

        public NodeType VisualType
        {
            get { return this.nodeType; }
        }

        public uint NodeId
        {
            get { return this.nodeId; }
        }

        public string ErrorMessage
        {
            get
            {
                if (null == this.errorBubble)
                    return null;

                return errorBubble.Content as string;
            }
        }

        public ErrorType ErrorType
        {
            get
            {
                if (null == this.errorBubble)
                    return ErrorType.None;
                return this.errorBubble.ErrType;
            }
        }

        public string PreviewValue
        {
            get
            {
                if (null == this.previewBubble)
                    return null;

                return this.previewBubble.Content as string;
            }
        }

        public States NodeStates
        {
            get { return this.nodeState; }
        }

        #endregion

        #region Public Class Methods

        public static IVisualNode Create(IGraphController graphController, IStorage storage)
        {
            if (graphController == null || storage == null)
                throw new ArgumentNullException("graphcontroller, storage");

            storage.Seek(12, SeekOrigin.Current); //Skip NodeSignature
            NodeType type = (NodeType)storage.ReadInteger(FieldCode.NodeType);
            storage.Seek(-24, SeekOrigin.Current); //Shift cursor back to the start point of reading NodeSignature
            VisualNode node = null;
            switch (type)
            {
                case NodeType.CodeBlock:
                    node = new CodeBlockNode(graphController);
                    node.Deserialize(storage);
                    break;
                case NodeType.Condensed:
                    node = new CondensedNode(graphController);
                    node.Deserialize(storage);
                    break;
                case NodeType.Driver:
                    node = new DriverNode(graphController);
                    node.Deserialize(storage);
                    break;
                case NodeType.Function:
                    node = new FunctionNode(graphController);
                    node.Deserialize(storage);
                    break;
                case NodeType.Identifier:
                    node = new IdentifierNode(graphController);
                    node.Deserialize(storage);
                    break;
                case NodeType.Property:
                    node = new PropertyNode(graphController);
                    node.Deserialize(storage);
                    break;
                case NodeType.Render:
                    node = new RenderNode(graphController);
                    node.Deserialize(storage);
                    break;
                default:
                    throw new ArgumentException("Invalid 'nodeType'");
            }

            return node;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("------------------------------------------------\n");
            builder.Append(string.Format("Node Id: 0x{0} ({1})\n", nodeId.ToString("x8"), nodeType.ToString()));
            builder.Append(string.Format("Name: {0}\nCaption: {1}\n", this.Text, this.Caption));

            SnapshotNodeType snapshotNodeType = SnapshotNodeType.None;
            builder.Append(string.Format("Code representation: {0}\n", this.ToCode(out snapshotNodeType)));
            builder.Append(string.Format("Snapshot node type: {0}\n", snapshotNodeType.ToString()));

            if (inputSlots.Count > 0)
            {
                builder.Append("Input slots: ");
                foreach (uint slotId in inputSlots)
                    builder.Append(string.Format("0x{0} ", slotId.ToString("x8")));
                builder.Append("\n");
            }
            else
            {
                builder.Append("Input slots: <none>\n");
            }

            if (outputSlots.Count > 0)
            {
                builder.Append("Output slots: ");
                foreach (uint slotId in outputSlots)
                    builder.Append(string.Format("0x{0} ", slotId.ToString("x8")));
                builder.Append("\n");
            }
            else
            {
                builder.Append("Output slots: <none>\n");
            }

            builder.Append(string.Format("Position: {0}, {1}\n", nodePosition.X, nodePosition.Y));
            builder.Append(string.Format("Dimension: {0}, {1}\n", nodeDimension.Width, nodeDimension.Height));
            builder.Append(string.Format("Selected: {0}\n", this.Selected));
            return builder.ToString();
        }

        #endregion

        #region Internal Slot Related Methods

        internal abstract string GetSlotName(SlotType slotType, uint slotId);

        internal virtual Point GetSlotPosition(Slot slot)
        {
            if (slot == null)
                throw new ArgumentNullException();

            if (slot.SlotType == SlotType.Input)
            {
                // @TODO(Victor): There's no checking "inputSlots" against "null" or "0".
                // (Ben): in the GetSlotIndex(), it throws exception when the slot id is invalid
                return GetInputIndexPosition(GetSlotIndex(slot.SlotId));
            }
            else
                return GetOutputIndexPosition();
        }

        internal virtual Point GetInputIndexPosition(int index)
        {
            Point indexPosition = this.nodePosition;
            if (this.Height == 0)
                this.Compose();
            if (this.VisualType == NodeType.Property)
            {
                indexPosition.Y += Configurations.TextVerticalOffset + Configurations.SlotSize + 1;
                return indexPosition;
            }

            indexPosition.Offset(0, 1);
            int slotCount = this.inputSlots.Count;
            if (slotCount == 1)
            {
                indexPosition.Y += this.Height / 2;
            }
            else if (slotCount > 1)
            {
                indexPosition.Y += Configurations.SlotStripeSize / 2;
                indexPosition.Y += index * Configurations.SlotStripeSize;
            }

            return indexPosition;
        }

        internal virtual Point GetOutputIndexPosition()
        {
            Point indexPosition = this.nodePosition;
            indexPosition.Offset(0, 0.25);
            if (this.VisualType == NodeType.Function || this.VisualType == NodeType.Property)
            {
                indexPosition.Offset(Width, (Height / 4));
                indexPosition.Y += (Height / 2) - 1;
                indexPosition.Y += 3;
            }
            else
            {
                indexPosition.Offset(Width, ((Height - 2) / 2));
                indexPosition.Y += 3;
            }

            return indexPosition;
        }

        internal int GetSlotIndex(uint slotId)
        {
            for (int i = 0; i < inputSlots.Count; i++)
            {
                if (slotId == inputSlots[i])
                    return i;
            }

            for (int i = outputSlots.Count - 1; i >= 0; i--)
            {
                if (slotId == outputSlots[i])
                    return i;
            }

            throw new InvalidOperationException("'slotId' cannot be found in 'inputSlots' and 'outputSlots'");
        }

        internal bool AllSlotsConnected(SlotType slotType)
        {
            if (slotType == SlotType.Input)
            {
                if (null != this.inputSlots && (inputSlots.Count > 0))
                {
                    foreach (uint slotId in this.inputSlots)
                    {
                        Slot slot = graphController.GetSlot(slotId) as Slot;
                        if (null == slot || (slot.Connected == false))
                            return false;
                    }
                }

                return true;
            }
            else if (slotType == SlotType.Output)
            {
                if (null != this.outputSlots && (outputSlots.Count > 0))
                {
                    foreach (uint slotId in this.outputSlots)
                    {
                        Slot slot = graphController.GetSlot(slotId) as Slot;
                        if (null == slot || (slot.Connected == false))
                            return false;
                    }
                }

                return true;
            }

            throw new ArgumentException("Invalid slot type (B426420EB6E6)", "slotType");
        }

        #endregion

        #region Internal Class Properties

        internal string Text { get; set; }
        internal string Caption { get; set; }

        internal bool IsEditingText
        {
            get { return this.partSelectedForEdit == NodePart.Text; }
        }

        internal bool IsEditingCaption
        {
            get { return this.partSelectedForEdit == NodePart.Caption; }
        }

        internal bool IsRunningHeadless
        {
            get { return this.graphController.GetVisualHost() == null; }
        }

        internal bool Visible
        {
            get { return this.nodeState.HasFlag(States.Visible); }
            set
            {
                if (value)
                {
                    if (this.nodeState.HasFlag(States.Visible))
                        return;

                    this.nodeState |= States.Visible;
                    this.nodeState |= States.Dirty;
                }
                else
                {
                    if (!this.nodeState.HasFlag(States.Visible))
                        return;

                    this.nodeState &= ~States.Visible;
                }
                this.Compose();
            }
        }

        internal bool Selected
        {
            get { return this.nodeState.HasFlag(States.Selected); }

            set
            {
                if (value) // Caller wants to select node
                {
                    if (this.nodeState.HasFlag(States.Selected))
                        return; // No flag changed, return earlier.

                    this.nodeState |= States.Selected;
                    this.nodeState |= States.PreviewSelected;
                }
                else // If caller wants to deselect the node
                {
                    if (!this.nodeState.HasFlag(States.Selected))
                        return; // No flag changed, return earlier.

                    this.nodeState &= ~States.Selected;
                    this.nodeState &= ~States.PreviewSelected;
                }

                // Mark the node as requiring redraw.
                this.nodeState |= States.Dirty;
            }
        }

        internal bool PreviewSelected
        {
            get { return this.nodeState.HasFlag(States.PreviewSelected); }

            set
            {
                if (value) // Caller wants to preview-select the node
                {
                    if (this.nodeState.HasFlag(States.PreviewSelected))
                        return; // No flag changed, return earlier.

                    this.nodeState |= States.PreviewSelected;
                }
                else // If caller wants to deselect the node
                {
                    if (!this.nodeState.HasFlag(States.PreviewSelected))
                        return; // No flag changed, return earlier.

                    this.nodeState &= ~States.PreviewSelected;
                }

                // Mark the node as requiring redraw.
                this.nodeState |= States.Dirty;
            }
        }

        internal bool Dirty
        {
            get { return this.nodeState.HasFlag(States.Dirty); }
            set
            {
                this.nodeState &= ~States.Dirty;
                if (value)
                    this.nodeState |= States.Dirty;
            }
        }

        internal bool Error
        {
            get { return this.nodeState.HasFlag(States.Error); }
            set
            {
                this.nodeState &= ~States.Error;
                if (value)
                    this.nodeState |= States.Error;
            }
        }

        internal string ReturnType { get; set; }
        internal LibraryItem.MemberType MemberType { get; set; }

        #endregion

        #region Public Abstract and Virtual Methods

        // These are pure virtual methods to be implemented by derived classes.
        internal abstract List<uint> GetSlotIdsByName(SlotType slotType, string variableName);
        internal abstract List<string> GetDefinedVariables(bool includeTemporaryVariables);
        internal abstract List<string> GetReferencedVariables(bool includeTemporaryVariables);
        internal abstract string ToCode(out SnapshotNodeType type);
        internal abstract string PreprocessInputString(string input);
        internal abstract ErrorType ValidateInputString(out string error);
        internal abstract NodePart HitTest(double x, double y, out int index);

        internal virtual double GetBubbleXPosition()
        {
            return this.Width / 2;
        }

        internal virtual void UpdateSlotVisibility()
        {
            return;
        }

        internal virtual void GetAssignmentStatements(List<AssignmentStatement> assignments)
        {
        }

        #endregion

        #region Internal Class Methods

        internal bool EnableEdit(NodePart nodePart, int index)
        {
            if (this.partSelectedForEdit != NodePart.None)
                return false;//throw exception

            // Replication guide index should only be specified for this case.
            if (nodePart != NodePart.ReplicationGuide && (index != -1))
                throw new ArgumentException("Index should not be specified (34BB6918677B)", "index");

            this.partSelectedForEdit = nodePart;
            this.replicationIndex = index;
            return true;
        }

        internal bool DisableEdit()
        {
            if (this.partSelectedForEdit == NodePart.Caption || this.partSelectedForEdit == NodePart.Text || this.partSelectedForEdit == NodePart.ReplicationGuide)
            {
                this.partSelectedForEdit = NodePart.None;
                return true;
            }
            else
                throw new InvalidOperationException();
        }

        internal bool Edit(string text, bool compose)
        {
            switch (this.partSelectedForEdit)
            {
                case NodePart.Text:
                    this.Text = text;
                    break;
                case NodePart.Caption:
                    this.Caption = text;
                    break;
                default:
                    throw new InvalidOperationException("NodePart is non-editable");
            }

            this.Dirty = true;
            if (compose)
                this.Compose();

            return true;
        }

        internal void ToggleContextualMenu(NodePart nodePart)
        {
            if (this.partDisplayingMenu == nodePart)
                this.partDisplayingMenu = NodePart.None;
            else
                this.partDisplayingMenu = nodePart;

            this.Dirty = true;
            this.Compose();
        }

        internal void SetErrorValue(object data, ErrorType errorType, bool runtime = false)
        {
            if (this.errorBubble == null)
            {
                if (data == null)
                    return;
                else
                    this.errorBubble = new ErrorBubble(this.graphController, this.nodeId);
            }

#if DEBUG
            // Ensure priority order is not changed: Syntax, Semantic, Warning, None.
            System.Diagnostics.Debug.Assert(ErrorType.Syntax > ErrorType.Semantic);
            System.Diagnostics.Debug.Assert(ErrorType.Semantic > ErrorType.Warning);
            System.Diagnostics.Debug.Assert(ErrorType.Warning > ErrorType.None);
#endif

            if (null == data) // No error data, reset error bubble.
            {
                data = string.Empty;
                errorType = ErrorType.None;
                this.Error = false;
            }
            else if (data is string)
            {
                // Empty error string, reset error bubble.
                if (string.IsNullOrEmpty(data as string))
                {
                    data = string.Empty;
                    errorType = ErrorType.None;
                    this.Error = false;
                }
            }

            //check if error need to be shown
            if (!runtime || (this.ShowError() == true))
            {
                if (errorType > ErrorType.Warning) // There's an error.
                {
                    // If the new error is of a higher priority than the one we 
                    // have internally, then overwrite the current error message.
                    if (errorType >= this.errorBubble.ErrType)
                    {
                        this.errorBubble.SetContent(data);
                        this.errorBubble.SetErrorType(errorType);
                        this.Error = true;
                    }
                }
                else // Warning or no error.
                {
                    this.errorBubble.SetContent(data);
                    this.errorBubble.SetErrorType(errorType);
                    this.Error = false;
                }
            }
            if (false != this.Error && this.previewBubble != null)
                this.previewBubble.SetContent(null); // Dismiss preview bubble.
        }

        internal void SetPreviewValue(object data)
        {
            if (previewBubble == null)
            {
                if (data == null)
                    return;
                else
                    previewBubble = new PreviewBubble(this.graphController, this.nodeId);
            }

            previewBubble.SetContent(data);
            previewBubble.Compose();
        }

        // Display a small triangle below the node. This happens all without 
        // the node querying its value, so the preview type is set to unknown.
        // A separate query will be made later when user tries to expands the 
        // preview bubble.
        internal void DisplayCollapsedPreview()
        {
            if (previewBubble == null)
                previewBubble = new PreviewBubble(this.graphController, this.nodeId);

            previewBubble.SetUnknownPreview();
            previewBubble.Compose();
        }

        internal string GetAssembly()
        {
            return this.GetAssemblyCore();
        }

        internal void SetReturnTypeAndAssembly(string className, string assembly)
        {
            if (!string.IsNullOrEmpty(className) && (className != "null"))
                this.ReturnType = className;
            //if (!string.IsNullOrEmpty(assembly) && (assembly != "null"))
            //    this.SetAssemblyCore(assembly);

            CheckDots();
            this.Compose();
        }

        internal void SetHoveredIndex(int index)
        {
            hoveredIndex = index;
            this.Dirty = true;
        }

        internal uint GetErrorBubbleId()
        {
            if (errorBubble == null)
                return uint.MaxValue;
            return this.errorBubble.BubbleId;
        }

        internal uint GetPreviewBubbleId()
        {
            if (previewBubble == null)
                return uint.MaxValue;
            return this.previewBubble.BubbleId;
        }

        internal Rect GetRegion(bool includeExtended)
        {
            double x = this.X;
            double y = this.Y;
            double width = this.Width;
            double height = this.Height;

            if ((this.previewBubble != null) && (includeExtended))
            {
                Point bubblePosition = this.previewBubble.RectPosition;
                double bubbleWidth = this.previewBubble.Width;
                double bubbleHeight = this.previewBubble.Height;

                if (bubblePosition.X < 0)
                {
                    x -= Math.Abs(bubblePosition.X);
                    width += Math.Abs(bubblePosition.X);

                    if (bubbleWidth > width)
                        width = bubbleWidth;
                }
                else
                {
                    if (bubblePosition.X + bubbleWidth > width)
                        width = bubblePosition.X + bubbleWidth;
                }

                height = bubblePosition.Y + bubbleHeight;
            }

            return new Rect(x, y, width, height);
        }

        internal void HandleNewConnection(uint slotId)
        {
            if (this.graphController.GetSlot(slotId).SlotType == SlotType.Input)
                this.SwapAssignmentOnSlot(slotId);
        }

        internal void HandleRemoveConnection(uint slotId)
        {
            if (this.graphController.GetSlot(slotId).SlotType == SlotType.Input)
                this.SwapAssignmentOnSlot(slotId);
        }

        internal void PositionAtCenter(uint nodeId)
        {
            IGraphVisualHost visualHost = this.graphController.GetVisualHost();
            if (visualHost != null)
            {
                DrawingVisual visual = visualHost.GetDrawingVisualForNode(nodeId);
                TranslateTransform transform = visual.Transform as TranslateTransform;
                transform.X = (int)(this.nodePosition.X - this.Width / 2);
                transform.Y = (int)(this.nodePosition.Y - this.Height / 2);
                this.nodePosition.X = transform.X;
                this.nodePosition.Y = transform.Y;

                if (this.errorBubble != null)
                {
                    DrawingVisual errorVisual = visualHost.GetDrawingVisualForBubble(this.errorBubble.BubbleId);
                    TranslateTransform errorTransform = errorVisual.Transform as TranslateTransform;
                    errorTransform.X = (int)(this.nodePosition.X);
                    errorTransform.Y = (int)(this.nodePosition.Y);
                }

                if (this.previewBubble != null)
                {
                    DrawingVisual previewVisual = visualHost.GetDrawingVisualForBubble(this.previewBubble.BubbleId);
                    TranslateTransform previewTransform = previewVisual.Transform as TranslateTransform;
                    previewTransform.X = (int)(this.nodePosition.X);
                    previewTransform.Y = (int)(this.nodePosition.Y);
                }
            }
        }

        internal void SetNodeState(States state)
        {
            this.nodeState |= state;
        }

        internal void ClearState(States state)
        {
            this.nodeState &= ~state;
        }

        internal void Compose()
        {
            if (false == this.Dirty)
                return;

            if (IsRunningHeadless)
                return;

            GraphController controller = graphController as GraphController;
            IGraphVisualHost visualHost = controller.GetVisualHost();

            DrawingVisual visual = visualHost.GetDrawingVisualForNode(this.NodeId);
            //TextOptions.SetTextFormattingMode(visual, TextFormattingMode.Display);
            //TextOptions.SetTextRenderingMode(visual, TextRenderingMode.Aliased);
            DrawingContext drawingContext = visual.RenderOpen();
            CultureInfo cultureInfo = new CultureInfo("en-us"); //@TODO(Ben) Move this into a single place and reuse it
            //RenderOptions.SetEdgeMode(visual, EdgeMode.Aliased);
            if (!dotsFlag.HasFlag(MenuDots.DotsFlag))
                this.CheckDots();

            this.ComposeCore(drawingContext, visual);
            drawingContext.Close();

            if (errorBubble != null)
                errorBubble.Compose();

            TranslateTransform nodeTransform = visual.Transform as TranslateTransform;
            nodeTransform.X = this.nodePosition.X;
            nodeTransform.Y = this.nodePosition.Y;

            this.Dirty = false;
        }

        #endregion

        #region Protected Class Methods

        protected VisualNode(IGraphController graphController, NodeType nodeType)
        {
            this.graphController = graphController as GraphController;
            IdGenerator idGenerator = this.graphController.GetIdGenerator();

            this.nodeType = nodeType;
            this.version = VisualNode.Version.Current;
            this.nodeId = idGenerator.GetNextId(ComponentType.Node);
            this.nodeState = States.Visible;
            this.Dirty = true;
            this.graphController.AddVisualNode(this);
        }

        protected VisualNode(IGraphController graphController)
        {
            this.graphController = graphController as GraphController;
            IdGenerator idGenerator = this.graphController.GetIdGenerator();
        }

        protected virtual void ComposeCore(DrawingContext drawingContext, DrawingVisual visual)
        {
            if (this.Visible == false)
            {
                drawingContext.Close();
                return;
            }

            //draw rounded rectangle
            RectangleGeometry roundedRect = new RectangleGeometry();
            int tempWidth = (int)Width;
            int tempHeight = (int)Height;
            //@TODO(Ben) please double check this is recomended behaviour and insert guidance url here
            roundedRect.Rect = new Rect(new Point(0.5, 0.5), new Point(tempWidth + 0.5, tempHeight + 0.5));
            //roundedRect.RadiusX = Configurations.CornerRadius;
            //roundedRect.RadiusY = Configurations.CornerRadius;

            //@TODO(Ben) extract this into a common place, don't recreate.
            //@TODO(Ben) Move this numbers to constant fields in configurations
            DropShadowEffect shadowEffect = new DropShadowEffect();
            shadowEffect.BlurRadius = Configurations.BlurRadius;
            shadowEffect.Color = Configurations.SelectionColor;
            shadowEffect.Direction = Configurations.Direction;
            shadowEffect.ShadowDepth = Configurations.ShadowDepth;
            shadowEffect.Opacity = Configurations.Opacity;

            drawingContext.DrawGeometry(Configurations.RectWhite, Configurations.BorderPen, roundedRect);

            //@TODO(Ben) If previewSelected != true remove the effect, don't just make it white
            if (this.PreviewSelected == true)
            {
                drawingContext.DrawGeometry(Configurations.RectWhite, Configurations.SelectionBorderPen, roundedRect);
                visual.Effect = shadowEffect;
            }
            else
            {
                drawingContext.DrawGeometry(Configurations.RectWhite, Configurations.BorderPen, roundedRect);
                shadowEffect.Opacity = 0;
                shadowEffect.Color = Configurations.SelectionColorWhite;
                visual.Effect = shadowEffect;
            }

            if (partDisplayingMenu == NodePart.NorthEast)
            {
                //@TODO(Joy) Magic numbers, lalalala, magic numbers
                //@TODO: XAMLify this
                Rect newRect = new Rect(new Point(Width - 30, -20), new Size(50, 50));
            }
            else if (partDisplayingMenu == NodePart.PreviewNorthEast)
            {
                Point pt = this.previewBubble.RectPosition;
                pt.Offset(this.previewBubble.Width - 30, -20);

                Rect newRect = new Rect(pt, new Size(50, 50));
            }
        }

        protected virtual void SwapAssignmentOnSlot(uint slotId)
        {
            return;
        }

        protected virtual string GetAssemblyCore()
        {
            return string.Empty;
        }

        protected virtual void SetAssemblyCore(string assembly)
        {
            return;
        }

        // @TODO(Joy)DONE: Please rename the first argument to "centerPoint" 
        // and then the other one "clickPoint".
        protected bool IsWithinClickRegion(Point centerPoint, double clickPtX, double clickPtY)
        {
            double hitTestPixels = Configurations.HittestPixels;

            if (clickPtX > centerPoint.X - hitTestPixels && clickPtX < centerPoint.X + hitTestPixels)
                if (clickPtY > centerPoint.Y - hitTestPixels && clickPtY < centerPoint.Y + hitTestPixels)
                    return true;

            return false;
        }

        protected void CheckDots()
        {
            Dictionary<int, string> list = new Dictionary<int, string>();
            this.dotsFlag = MenuDots.None;

            MenuItemsProvider itemsProvider = new MenuItemsProvider(this.graphController, NodePart.NorthEast, this.NodeId);
            list = itemsProvider.GetMenuItems();
            if (list.Count > 0)
                this.dotsFlag |= MenuDots.NorthEast;

            list = new Dictionary<int, string>();
            itemsProvider = new MenuItemsProvider(this.graphController, NodePart.North, this.NodeId);
            list = itemsProvider.GetMenuItems();
            if (list.Count >= 1)
                this.dotsFlag |= MenuDots.North;

            list = new Dictionary<int, string>();
            itemsProvider = new MenuItemsProvider(this.graphController, NodePart.NorthWest, this.NodeId);
            list = itemsProvider.GetMenuItems();
            if (list.Count != 0)
                this.dotsFlag |= MenuDots.NorthWest;

            list = new Dictionary<int, string>();
            itemsProvider = new MenuItemsProvider(this.graphController, NodePart.South, this.NodeId);
            list = itemsProvider.GetMenuItems();
            if (list.Count > 0)
                this.dotsFlag |= MenuDots.South;

            this.dotsFlag |= MenuDots.DotsFlag;
        }

        #endregion

        #region Private Helper Functions

        private bool ShowError()
        {
            bool result = false;
            List<ISlot> inputSlots = new List<ISlot>();
            List<ISlot> outputSlots = new List<ISlot>();

            foreach (uint inputSlot in this.inputSlots)
                inputSlots.Add(graphController.GetSlot(inputSlot));
            foreach (uint outputSlot in this.outputSlots)
                outputSlots.Add(graphController.GetSlot(outputSlot));

            foreach (ISlot outputSlot in outputSlots)
            {
                if (outputSlot.ConnectingSlots != null
                    && outputSlot.ConnectingSlots.Count() > 0)
                    result = true;
            }

            if (result == false)
            {
                result = true;
                foreach (ISlot inputSlot in inputSlots)
                {
                    if (inputSlot.ConnectingSlots == null)
                        result = false;
                }
            }
            return result;
        }

        #endregion
    }
}
