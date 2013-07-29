using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using Autodesk.DesignScript.Interfaces;
using GraphToDSCompiler;

namespace DesignScriptStudio.Graph.Core
{
    public enum NodeType
    {
        None,
        Condensed,
        Driver,
        Function,
        Identifier,
        Render,
        CodeBlock,
        Property,
        Custom
    }

    public enum SlotType
    {
        None, Input, Output
    }

    public enum EdgeType
    {
        None, ExplicitConnection, ImplicitConnection
    }

    public enum ConnectionType
    {
        Logical, Visual
    }

    public enum ComponentType : uint
    {
        None = 0x00000000,
        Node = 0x10000000,
        Slot = 0x30000000,
        Edge = 0x60000000,
        Bubble = 0x80000000
    }

    /// <summary>
    /// Represents parts of the node
    /// </summary>
    public enum NodePart
    {
        None,
        InputLabel,
        Caption,
        Text,
        InputSlot,
        OutputSlot,
        North,
        NorthEast,
        NorthWest,
        South,
        Preview,
        PreviewNorthEast,
        ReplicationGuide
    }

    public enum States
    {
        None = 0x0,
        Visible = 0x01,
        Selected = 0x02,
        Dirty = 0x04,
        PreviewSelected = 0x08,
        Error = 0x10,
        TextualPreview = 0x20,
        GeometryPreview = 0x40,
        PreviewHidden = 0x80,
        SuppressPreview = 0x100
    }

    public enum SlotStates
    {
        None = 0x0,
        Visible = 0x01
    }

    public enum DragState
    {
        None,
        RegionSelection,
        NodeRepositioning,
        CurveDrawing,
        EdgeReconnection
    }

    public enum EdgeConnectionFlag
    {
        None,
        Illegal,
        NewEdge,
        ReconnectEdge,
    }

    public enum AuditStatus
    {
        NoChange,
        PersistentDataChanged,
    }

    public interface ICoreComponent
    {
        void InitializeAsync(ComponentInitializedDelegate initialized);
        void Initialize();
        void Shutdown();
        LibraryItem GetRootLibraryItem();

        /// <summary>
        /// Imports a given library/assembly.
        /// </summary>
        /// <param name="assemblyPath">Full path of the library(ds/dll/exe)</param>
        /// <param name="rootModulePath">Root path of the current graph</param>
        /// <param name="ignoreDuplicateOrEmptyFileError">whether to display 
        /// duplicate/empty file error.</param>
        /// <returns>true/false</returns>
        bool ImportAssembly(string assemblyPath, string rootModulePath, bool ignoreDuplicateOrEmptyFileError);

        void DisableSplash();
        void RemoveAssembly(string assemblyName);
        void RefreshAssembly(string assemblyName);

        PersistentSettings StudioSettings { get; }
        string SessionName { get; }
        bool LaunchedForRecovery { get; }
    }

    public interface IStorage
    {
        long GetPosition();
        void Seek(long offSet, SeekOrigin from);

        byte[] ReadBytes(ulong expectedSignature);
        byte[] ReadBytes(ulong expectedSignature, byte[] defaultValue);
        void WriteBytes(ulong signature, byte[] value);

        int ReadInteger(ulong expectedSignature);
        int ReadInteger(ulong expectedSignature, int defaultValue);
        void WriteInteger(ulong signature, int value);

        uint ReadUnsignedInteger(ulong expectedSignature);
        uint ReadUnsignedInteger(ulong expectedSignature, uint defaultValue);
        void WriteUnsignedInteger(ulong signature, uint value);

        long ReadLong(ulong expectedSignature);
        long ReadLong(ulong expectedSignature, long defaultValue);
        void WriteLong(ulong signature, long value);
        ulong PeekUnsignedLong();

        ulong ReadUnsignedLong(ulong expectedSignature);
        ulong ReadUnsignedLong(ulong expectedSignature, ulong defaultValue);
        void WriteUnsignedLong(ulong signature, ulong value);

        bool ReadBoolean(ulong expectedSignature);
        bool ReadBoolean(ulong expectedSignature, bool defaultValue);
        void WriteBoolean(ulong signature, bool value);

        double ReadDouble(ulong expectedSignature);
        double ReadDouble(ulong expectedSignature, double defaultValue);
        void WriteDouble(ulong signature, double value);

        string ReadString(ulong expectedSignature);
        string ReadString(ulong expectedSignature, string defaultValue);
        void WriteString(ulong signature, string value);
    }

    public interface ISerializable
    {
        bool Deserialize(IStorage storage);
        bool Serialize(IStorage storage);
        AuditStatus Audit();
    }

    public interface IVisualNode : ISerializable
    {
        #region Public Interface Methods

        uint[] GetInputSlots();
        uint[] GetOutputSlots();
        uint GetInputSlot(int index);
        uint GetOutputSlot(int index);
        IEnumerable<IVisualNode> EnumerateInputs(ConnectionType connection);
        IEnumerable<IVisualNode> EnumerateOutputs(ConnectionType connection);

        #endregion

        #region Public Interface Properties

        double X { get; }
        double Y { get; }
        double Width { get; }
        double Height { get; }
        double CenterLine { get; }
        States NodeStates { get; }

        NodeType VisualType { get; }
        uint NodeId { get; }
        string ErrorMessage { get; }
        string PreviewValue { get; }

        #endregion
    }

    public interface IInfoBubble
    {
        uint BubbleId { get; }
        uint OwnerId { get; }
        object Content { get; }
        Point RectPosition { get; }
        int Width { get; }
        int Height { get; }
        Point AnchorPoint { get; }
        bool IsPreviewBubble { get; }
        bool Collapsed { get; }
        bool Extended { get; }
        bool Extendable { get; }
    }

    interface ISlot : ISerializable
    {
        uint SlotId { get; }
        SlotType SlotType { get; }
        SlotStates States { get; }
        uint[] Owners { get; }
        uint[] ConnectingSlots { get; }
    }

    interface IVisualEdge : ISerializable
    {
        uint EdgeId { get; }
        uint StartSlotId { get; }
        uint EndSlotId { get; }
    }

    public interface IGraphUiContainer
    {
        IGraphController GetController(uint identifier);
        void AddFeedbackMessage(string iconPath, string message);
        void FinishLoadingLibrary();
        System.Windows.Threading.Dispatcher CurrentDispatcher { get; }
        IGraphEditorHostApplication HostApplication { get; }
    }

    public interface IGraphController
    {
        #region Graph Controller Operational Methods

        void CleanUp(); // The UI is closing the tab/app.

        //'Read only'

        /// <summary>
        /// For a component and a mouse position find the appropraite selected node-part.
        /// </summary>
        /// <param name="compId">The identifier for the component being hit tested.</param>
        /// <param name="mouse">The mouse position in canvas space.</param>
        /// <param name="nodePart">The part of component under the mouse cursor.</param>
        /// <param name="index">The zero based index for an input slot or an output slot. 
        /// This value will only be valid when nodePart is OutputSlot or InputSlot.</param>
        /// <returns>Returns true if a node part can be computed, or false otherwise.</returns>
        bool HitTestComponent(uint compId, System.Windows.Point mouse, out NodePart nodePart, out int index);
        bool GetNodeText(uint compId, NodePart nodePart, out string text);
        bool GetLastNodeId(out uint compId); //@TODO(Ben): Consider removing
        bool GetMenuItems(out Dictionary<int, string> menuItems, out Dictionary<int, string> overloadedItems);
        void GetNodeRegion(uint nodeId, bool includeExtended, out Rect region);
        bool GetNodeType(uint nodeId, out string returnType);
        bool GetNodeState(uint nodeId, out States nodeState);
        bool DeterminEdgeSelection(uint edgeId, Point topLeft, Point bottomRight);
        bool GetTextboxMinSize(uint nodeId, out double minTextboxWidth);
        bool GetReplicationIndex(uint compId, Point mousePt, out int replicationIndex, out int slotIndex);
        bool GetReplicationText(uint compId, int index, out string text);
        bool SelectionBoxHittest(Point mousePosition);
        bool PreviewBubbleHittest(Point mousePosition, uint bubbleId);
        bool GetInfoBubble(uint bubbleId, out IInfoBubble infoBubble);
        //@TODO(Ben) Get node part region
        bool GetNodePartRegion(uint compId, NodePart nodePart, Point mousePt, out Rect result, out NodeType type);
        bool IsComponentSelected(uint componentId);
        bool CheckEdgeType(uint compId);
        List<string> GetImportedScripts();
        bool CanUndo();
        bool CanRedo();
        int SelectedComponentsCount();
        Point GetNorthEastPositionOfSelectionBox();

        //Transients
        void SetMouseCursor(uint nodeId, NodePart nodePart, int index, ModifierKeys modifiers, bool isMouseButtonDown, double x, double y);

        //@TODO(Ben): Rename to TransientUpdateNodeText
        bool UpdateNodeText(uint compId, string text);
        bool UpdateNodeTextHighFrequency(string text);
        bool TransientUpdateReplicationText(uint compId, string text, int replicationIndex);
        //@TODO(Ben): consider a collection of IDs
        void PreviewSelectComponent(uint componentId, ModifierKeys modifiers);
        void ClearPreviewSelection(uint componentId);
        void TransientExtendPreviewBubble(uint bubbleId, bool extend);
        void TransientShowPreviewBubble(uint bubbleId, bool visibility);


        /// <summary>
        /// Get the recorded command in GraphController, along with 
        /// the current states of all visual nodes, edges and slots.
        /// </summary>
        /// <returns>The log information as a string</returns>
        string GetRecordedUiStates();

        /// <summary>
        /// Get the recorded snapshot nodes for GraphCompiler.
        /// </summary>
        /// <returns>
        /// The XML informations where state information is saved.
        /// </returns>
        string GetRecordedVmStates();

        /// <summary>
        /// Dump the recorded command in GraphController, along with 
        /// the current states of all visual nodes, edges and slots.
        /// </summary>
        /// <returns>
        /// The path of the LOG file where state information is saved.
        /// </returns>
        string DumpRecordedUiStates();

        /// <summary>
        /// Dump the recorded snapshot nodes for GraphCompiler.
        /// </summary>
        /// <returns>
        /// The path of the XML file where state information is saved.
        /// </returns>
        string DumpRecordedVmStates();

        /// <summary>
        /// Convert a group of selected nodes to a single code block
        /// </summary>
        /// <returns>
        /// return true if combine successful, false otherwise
        /// </returns>
        bool ProcessNodesToCodeConversion(List<uint> originalNodeIds, List<SnapshotNode> snapshotNodes);

        #endregion

        #region Commands Related Interface Methods

        bool DoCreateIdentifierNode(double x, double y);
        bool DoCreateRenderNode(double x, double y);
        bool DoCreateDriverNode(double x, double y);
        bool DoCreateCodeBlockNode(double x, double y, string content);
        bool DoCreateFunctionNode(double x, double y, string assembly, string qualifiedName, string argumentTypes);
        bool DoCreatePropertyNode(double x, double y, string assembly, string qualifiedName, string argumentTypes);
        bool DoSaveGraph(string filePath);
        bool DoMouseDown(MouseButton button, uint compId, NodePart nodePart, int index, ModifierKeys modifiers);
        bool DoMouseUp(MouseButton button, uint compId, NodePart nodePart, int index, ModifierKeys modifiers);
        bool DoBeginDrag(MouseButton button, uint compId, NodePart nodePart, int index, ModifierKeys modifiers, double x, double y);
        bool DoEndDrag(MouseButton button, uint compId, NodePart nodePart, int index, ModifierKeys modifiers, double x, double y);
        bool DoBeginNodeEdit(uint compId, NodePart nodePart);
        bool DoEndNodeEdit(uint compId, string text, bool updateFlag);
        bool DoBeginHighFrequencyUpdate(uint nodeId, NodePart nodePart);
        bool DoEndHighFrequencyUpdate(string text);
        bool DoSelectComponent(uint compId, ModifierKeys modifiers);
        bool DoClearSelection();
        bool DoUndoOperation();
        bool DoRedoOperation();
        bool DoDeleteComponents();
        bool DoAddReplicationGuide(uint nodeId);
        bool DoRemoveReplicationGuide(uint nodeId);
        bool DoEditReplicationGuide(uint nodeId, int replicationIndex);
        bool DoSetReplicationGuideText(uint nodeId, int replicationIndex, string text);
        bool DoSelectMenuItem(int menuItemId, double x, double y, uint nodeId, NodePart nodePart);
        bool DoCreateRadialMenu(NodePart nodePart, uint nodeId);
        bool DoCreateSubRadialMenu(int selectedItemId);
        bool DoImportScript(string scriptPath);
        bool DoConvertSelectionToCode();
        bool DoTogglePreview(uint bubbleId);

        #endregion

        #region Public Interface Events

        event CanvasModifiedHandler Modified;

        #endregion

        #region Interface Properties

        uint Identifier { get; }
        DragState CurrentDragState { get; }
        EdgeConnectionFlag EdgeConnection { get; }
        string FilePath { get; }
        bool IsModified { get; }

        #endregion
    }

    public interface IGraphVisualHost
    {
        void BeginDragSelection();
        void EndDragSelection();
        List<uint> GetComponents();

        bool IsInRecordingMode { get; }
        void RearrangeDrawingVisual(uint compId, bool bringToFront, uint ownerId);
        bool RemoveDrawingVisual(uint compId);
        DrawingVisual GetDrawingVisualForNode(uint nodeId);
        DrawingVisual GetDrawingVisualForEdge(uint edgeId);
        DrawingVisual GetDrawingVisualForBubble(uint bubbleId);
        DrawingVisual GetDrawingVisualForSelectionBox();
        void ExtendBubble(uint bubbleId, string extendedString, double width, double height);
        void RemoveExtendedBubble(uint bubbleId);
        void GetExtendedBubbleSize(uint bubbleId, out double x, out double y);
        void ScrollToVisual(DrawingVisual drawingVisual);
    }

    public delegate void ComponentInitializedDelegate(bool loadSucceeded);

    public class ClassFactory
    {
        public static ICoreComponent CreateCoreComponent(IGraphUiContainer uiContainer)
        {
            if (null != CoreComponent.Instance)
                throw new InvalidOperationException("'ClassFactory.CreateCoreComponent' called twice!");

            // This method is called by NUnit test cases, therefore 
            // we do not need geometric preview to be enabled.
            return CoreComponent.CreateSingleton(uiContainer, false);
        }

        public static ICoreComponent CreateCoreComponent(IGraphUiContainer uiContainer, bool enableGeometricPreview)
        {
            if (null != CoreComponent.Instance)
                throw new InvalidOperationException("'ClassFactory.CreateCoreComponent' called twice!");

            return CoreComponent.CreateSingleton(uiContainer, enableGeometricPreview);
        }

        public static void DestroyCoreComponent()
        {
            CoreComponent.DestroySingleton();
        }

        public static ICoreComponent CurrCoreComponent
        {
            get { return CoreComponent.Instance; }
        }

        public static IGraphController CreateGraphController(IGraphVisualHost visualHost)
        {
            return new GraphController(visualHost);
        }

        public static IGraphController CreateGraphController(IGraphVisualHost visualHost, string filePath)
        {
            return new GraphController(visualHost, filePath);
        }
    }

    public class FieldCode
    {
        //DataHeader = HDR
        public static ulong DataHeaderSignature = Utilities.MakeEightCC('H', 'D', 'R', ' ', ' ', ' ', ' ', ' ');
        public static ulong DataHeaderVersion = Utilities.MakeEightCC('H', 'D', 'R', ' ', 'V', 'E', 'R', ' ');
        public static ulong HeaderSize = Utilities.MakeEightCC('H', 'D', 'R', ' ', 'S', 'Z', ' ', ' ');
        public static ulong DataSize = Utilities.MakeEightCC('H', 'D', 'R', ' ', 'D', 'T', 'S', 'Z');

        //Edge = EG
        public static ulong EdgeSignature = Utilities.MakeEightCC('E', 'G', ' ', ' ', ' ', ' ', ' ', ' ');
        public static ulong EdgeCount = Utilities.MakeEightCC('E', 'G', ' ', 'C', ' ', ' ', ' ', ' ');
        public static ulong EdgeType = Utilities.MakeEightCC('E', 'G', ' ', 'T', 'Y', 'P', 'E', ' ');
        public static ulong EdgeVersion = Utilities.MakeEightCC('E', 'G', ' ', 'V', 'E', 'R', ' ', ' ');
        public static ulong EdgeId = Utilities.MakeEightCC('E', 'G', ' ', 'I', 'D', ' ', ' ', ' ');
        public static ulong EdgeState = Utilities.MakeEightCC('E', 'G', ' ', 'S', 'T', 'A', 'T', 'E');
        public static ulong StartSlotId = Utilities.MakeEightCC('S', 'T', 'R', ' ', 'S', 'T', 'I', 'D');
        public static ulong EndSlotId = Utilities.MakeEightCC('E', 'N', 'D', ' ', 'S', 'T', 'I', 'D');
        public static ulong ControlPointsCount = Utilities.MakeEightCC('C', 'T', 'R', 'L', ' ', 'P', 'T', 'C');
        public static ulong ControlPointsX = Utilities.MakeEightCC('C', 'T', 'R', 'L', ' ', 'P', 'T', 'X');
        public static ulong ControlPointsY = Utilities.MakeEightCC('C', 'T', 'R', 'L', ' ', 'P', 'T', 'Y');

        //GraphProperties = GP
        public static ulong GraphPropertiesSignature = Utilities.MakeEightCC('G', 'P', ' ', ' ', ' ', ' ', ' ', ' ');
        public static ulong GraphPropertiesVersion = Utilities.MakeEightCC('G', 'P', ' ', 'V', 'E', 'R', ' ', ' ');
        public static ulong ApplicationVersion = Utilities.MakeEightCC('A', 'P', 'P', ' ', 'V', 'E', 'R', ' ');
        public static ulong AuthorName = Utilities.MakeEightCC('A', 'U', 'T', 'H', 'O', 'R', ' ', ' ');
        public static ulong CompanyName = Utilities.MakeEightCC('C', 'O', 'M', 'P', 'A', 'N', 'Y', ' ');
        public static ulong ImportedScriptsCount = Utilities.MakeEightCC('I', 'P', 'T', 'S', 'C', 'P', 'T', 'C');
        public static ulong ImportedScript = Utilities.MakeEightCC('I', 'P', 'T', 'S', 'C', 'P', 'T', ' ');

        //Node = ND
        public static ulong NodeSignature = Utilities.MakeEightCC('N', 'D', ' ', ' ', ' ', ' ', ' ', ' ');
        public static ulong NodeCount = Utilities.MakeEightCC('N', 'D', ' ', 'C', ' ', ' ', ' ', ' ');
        public static ulong NodeType = Utilities.MakeEightCC('N', 'D', ' ', 'T', 'Y', 'P', 'E', ' ');
        public static ulong NodeVersion = Utilities.MakeEightCC('N', 'D', ' ', 'V', 'E', 'R', ' ', ' ');
        public static ulong NodeId = Utilities.MakeEightCC('N', 'D', ' ', 'I', 'D', ' ', ' ', ' ');
        public static ulong NodeState = Utilities.MakeEightCC('N', 'D', ' ', 'S', 'T', 'A', 'T', 'E');
        public static ulong Text = Utilities.MakeEightCC('N', 'D', ' ', 'T', 'X', 'T', ' ', ' ');
        public static ulong Caption = Utilities.MakeEightCC('N', 'D', ' ', 'C', 'A', 'P', ' ', ' ');
        public static ulong NodePositionX = Utilities.MakeEightCC('N', 'D', ' ', 'P', 'O', 'S', 'X', ' ');
        public static ulong NodePositionY = Utilities.MakeEightCC('N', 'D', ' ', 'P', 'O', 'S', 'Y', ' ');
        public static ulong InputSlotsCount = Utilities.MakeEightCC('I', 'N', ' ', 'S', 'T', 'C', ' ', ' ');
        public static ulong InputSlots = Utilities.MakeEightCC('I', 'N', ' ', 'S', 'T', ' ', ' ', ' ');
        public static ulong OutputSlotsCount = Utilities.MakeEightCC('O', 'U', 'T', ' ', 'S', 'T', 'C', ' ');
        public static ulong OutputSlots = Utilities.MakeEightCC('O', 'U', 'T', ' ', 'S', 'T', ' ', ' ');
        public static ulong CompilableText = Utilities.MakeEightCC('C', 'M', 'P', ' ', 'T', 'X', 'T', ' ');
        public static ulong InputLinesCount = Utilities.MakeEightCC('I', 'N', ' ', 'L', 'N', 'C', ' ', ' ');
        public static ulong InputLinesKey = Utilities.MakeEightCC('I', 'N', ' ', 'L', 'N', ' ', 'K', ' ');
        public static ulong InputLinesValue = Utilities.MakeEightCC('I', 'N', ' ', 'L', 'N', ' ', 'V', ' ');
        public static ulong OutputLinesCount = Utilities.MakeEightCC('O', 'U', 'T', ' ', 'L', 'N', 'C', ' ');
        public static ulong OutputLinesKey = Utilities.MakeEightCC('O', 'U', 'T', ' ', 'L', 'N', ' ', 'K');
        public static ulong OutputLinesValue = Utilities.MakeEightCC('O', 'U', 'T', ' ', 'L', 'N', ' ', 'V');
        public static ulong StatmentIndex = Utilities.MakeEightCC('S', ' ', 'I', 'D', 'X', ' ', ' ', ' ');
        public static ulong VariableLineCount = Utilities.MakeEightCC('V', 'A', 'R', ' ', 'L', 'N', ' ', 'C');
        public static ulong Variable = Utilities.MakeEightCC('V', 'A', 'R', ' ', ' ', ' ', ' ', ' ');
        public static ulong Line = Utilities.MakeEightCC('L', 'N', ' ', ' ', ' ', ' ', ' ', ' ');
        public static ulong EmbeddedNodesCount = Utilities.MakeEightCC('E', 'B', 'D', ' ', 'N', 'D', 'C', ' ');
        public static ulong EmbeddedNode = Utilities.MakeEightCC('E', 'B', 'D', ' ', 'N', 'D', ' ', ' ');
        public static ulong Assembly = Utilities.MakeEightCC('A', 'S', 'S', 'E', 'M', ' ', ' ', ' ');
        public static ulong QualifiedName = Utilities.MakeEightCC('Q', 'L', 'F', ' ', 'N', 'A', 'M', 'E');
        public static ulong ArgumentTypes = Utilities.MakeEightCC('A', 'R', 'G', 'T', 'Y', 'P', 'E', 'S');
        public static ulong ReplicationGuides = Utilities.MakeEightCC('R', 'E', 'P', 'G', 'U', 'I', 'D', 'E');
        public static ulong ReplicationArgumentCount = Utilities.MakeEightCC('R', 'E', 'P', 'A', 'R', 'G', 'S', 'C');
        public static ulong ReplicationIndexCount = Utilities.MakeEightCC('R', 'E', 'P', 'I', 'N', 'D', 'X', 'C');
        public static ulong ReplicationIndex = Utilities.MakeEightCC('R', 'E', 'P', ' ', 'I', 'N', 'D', 'X');

        //RuntimeStates = RS
        public static ulong RuntimeStatesSignature = Utilities.MakeEightCC('R', 'S', ' ', ' ', ' ', ' ', ' ', ' ');
        public static ulong RuntimeStatesVersion = Utilities.MakeEightCC('R', 'S', ' ', 'V', 'E', 'R', ' ', ' ');
        public static ulong IsModified = Utilities.MakeEightCC('I', 'S', ' ', 'M', 'O', 'D', ' ', ' ');
        public static ulong VariableNodesMapCount = Utilities.MakeEightCC('V', 'R', 'N', 'D', 'M', 'P', 'C', ' ');
        public static ulong VariableNodesMapKey = Utilities.MakeEightCC('V', 'R', 'N', 'D', 'M', 'P', 'K', ' ');
        public static ulong VariableNodesMapValueCount = Utilities.MakeEightCC('V', 'R', 'N', 'D', 'M', 'P', 'V', 'C');
        public static ulong VariableNodesMapValue = Utilities.MakeEightCC('V', 'R', 'N', 'D', 'M', 'P', 'V', ' ');

        //Slot = ST
        public static ulong SlotSignature = Utilities.MakeEightCC('S', 'T', ' ', ' ', ' ', ' ', ' ', ' ');
        public static ulong SlotCount = Utilities.MakeEightCC('S', 'T', ' ', 'C', ' ', ' ', ' ', ' ');
        public static ulong SlotType = Utilities.MakeEightCC('S', 'T', ' ', 'T', 'Y', 'P', 'E', ' ');
        public static ulong SlotVersion = Utilities.MakeEightCC('S', 'T', ' ', 'V', 'E', 'R', ' ', ' ');
        public static ulong SlotId = Utilities.MakeEightCC('S', 'T', ' ', 'I', 'D', ' ', ' ', ' ');
        public static ulong SlotState = Utilities.MakeEightCC('S', 'T', ' ', 'S', 'T', 'A', 'T', 'E');
        public static ulong OwnersCount = Utilities.MakeEightCC('O', 'W', 'R', 'S', ' ', 'C', ' ', ' ');
        public static ulong Owners = Utilities.MakeEightCC('O', 'W', 'R', 'S', ' ', ' ', ' ', ' ');
        public static ulong ConnectingSlotsCount = Utilities.MakeEightCC('C', 'N', 'T', ' ', 'S', 'T', 'C', ' ');
        public static ulong ConnectingSlots = Utilities.MakeEightCC('C', 'N', 'T', ' ', 'S', 'T', ' ', ' ');

        //UndoRedoRecorder
        public static ulong ActionCount = Utilities.MakeEightCC('A', 'C', 'T', 'I', 'O', 'N', 'C', ' ');
        public static ulong Shift = Utilities.MakeEightCC('S', 'H', 'I', 'F', 'T', ' ', ' ', ' ');
        public static ulong UserAction = Utilities.MakeEightCC('U', 'R', 'A', 'C', 'T', 'I', 'O', 'N');

        //Statement
        public static ulong StatementSignature = Utilities.MakeEightCC('S', 'T', 'M', ' ', ' ', ' ', ' ', ' ');
        public static ulong ReferencesCount = Utilities.MakeEightCC('S', 'T', 'M', ' ', 'R', 'E', 'C', 'T');
        public static ulong ChildrenCount = Utilities.MakeEightCC('S', 'T', 'M', ' ', 'C', 'H', 'C', 'T');
        public static ulong DefinedVariable = Utilities.MakeEightCC('S', 'T', 'M', ' ', 'D', 'V', 'A', 'R');
        public static ulong StatementFlag = Utilities.MakeEightCC('S', 'T', 'M', ' ', 'F', 'L', 'A', 'G');

        //VariableSlotInfo
        public static ulong VariableSlotInfoVar = Utilities.MakeEightCC('V', 'S', 'I', ' ', 'V', 'A', 'R', ' ');
        public static ulong VariableSlotInfoLine = Utilities.MakeEightCC('V', 'S', 'I', ' ', 'L', 'I', 'L', 'E');
        public static ulong VariableSlotInfoSlotId = Utilities.MakeEightCC('V', 'S', 'I', ' ', 'S', 'L', 'O', 'T');

    }

    public class FileVersionException : System.Exception
    {
        public FileVersionException(string requiredAppVersion)
        {
            this.RequiredAppVersion = requiredAppVersion;
        }

        public string RequiredAppVersion { get; private set; }
    }
}