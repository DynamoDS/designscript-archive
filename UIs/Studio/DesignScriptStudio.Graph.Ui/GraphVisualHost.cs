using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Resources;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.Win32;
using GraphToDSCompiler;
using System.Timers;
using DesignScriptStudio.Graph.Core;
using System.Windows.Media.Animation;

namespace DesignScriptStudio.Graph.Ui
{
    public class GraphVisualHost : FrameworkElement, IGraphVisualHost
    {
        #region Class Data Members

        private GraphCanvas graphCanvas = null;
        private GraphControl graphControl = null;
        private IGraphController graphController = null;

        private Dictionary<DrawingVisual, uint> nodeVisuals = new Dictionary<DrawingVisual, uint>();
        private Dictionary<DrawingVisual, uint> edgeVisuals = new Dictionary<DrawingVisual, uint>();
        private Dictionary<DrawingVisual, uint> bubbleVisuals = new Dictionary<DrawingVisual, uint>();
        private DrawingVisual selectionBoxVisual = null;
        private List<ExtendedPreview> extendedBubble = new List<ExtendedPreview>();

        private VisualCollection children = null;
        private DrawingVisual hitVisual = null;

        private Point mouseDownPosition = new Point(-1, -1);
        private Point mouseDownScreenPosition = new Point(-1, -1);
        private Point mousePosition;
        private bool isMouseButtonDown = false;

        private CustomTextBox editTextbox = null;

        private ExtendedPreview previewTextbox = null;
        private List<uint> transientExtendedBubble = new List<uint>();

        private bool nodeEditFlag = false;
        private bool replicationEditFlag = false;
        private uint nodeEditId = uint.MaxValue;
        private string text = string.Empty;
        private Rect currentNodePartRegion = new Rect();

        private List<RadialMenu> radialMenus = new List<RadialMenu>();
        private RadialMenu radialMenu = null;
        private RadialMenu subRadialMenu = null;
        private int selectedMenuId = int.MinValue;
        //Zoom and Pan
        Point zoomPanPivotPoint = new Point();
        bool panning = false;
        bool zoomToCursor = false;
        int sliderStep = 6; // refer to Configurations.SliderStep[6] = 1, 
        public bool animationOn = false;
        bool scroll = false;
        public static bool sliderScrollHandled = false;

        //timer
        private Timer radialMenuTimer;
        private uint radialNodeId = uint.MaxValue;
        private NodePart radialNodePart = NodePart.None;

        //Click and Drag Drawing Edge
        uint lastClickedNodeId = uint.MaxValue;
        int lastClickedNodeIndex = int.MaxValue;
        NodePart lastClickedNodePart = NodePart.None;

        // Drag Selection
        private List<uint> selectedComponent = new List<uint>();

        //radial Menu
        private NodePart lastHoveredNodePart = NodePart.None;
        private uint lastHoveredNodeId = uint.MaxValue;
        private uint lastHoveredBubbleId = uint.MaxValue;
        //SubRadialMenuTimer
        System.Timers.Timer subRadialMenuTimer = null;
        private Point subRadialMenuPoint = new Point();

        //ReplicationGuide
        int editingReplicationIndex = int.MinValue;

        #endregion

        #region IGraphVisualHost Interface Members

        public void BeginDragSelection()
        {
            UpdateDragSelectionRect(mouseDownPosition, mousePosition);
            graphControl.selectionBorder.Visibility = Visibility.Visible;
        }

        public void EndDragSelection()
        {
            graphControl.selectionBorder.Visibility = Visibility.Collapsed;
            selectedComponent.Clear();
            graphControl.SetCursor(CursorSet.Index.Pointer);
        }

        public List<uint> GetComponents()
        {
            UpdateDragSelectionRect(mouseDownPosition, mousePosition);
            Point topLeft = new Point(graphControl.selectionBorder.Margin.Left,
                                      graphControl.selectionBorder.Margin.Top);
            Point bottomRight = new Point(topLeft.X + graphControl.selectionBorder.Width,
                                          topLeft.Y + graphControl.selectionBorder.Height);

            Point topLeftOnCanvas = graphControl.canvasScrollViewer.
                                                 TranslatePoint(topLeft, graphControl.CurrentGraphCanvas);
            Point bottomRightOnCanvas = graphControl.canvasScrollViewer.
                                     TranslatePoint(bottomRight, graphControl.CurrentGraphCanvas);

            return GetComponentInRegion(new Rect(topLeftOnCanvas.X, topLeftOnCanvas.Y,
                                                 bottomRightOnCanvas.X - topLeftOnCanvas.X,
                                                 bottomRightOnCanvas.Y - topLeftOnCanvas.Y));
        }

        public bool IsInRecordingMode { get; set; }

        public DrawingVisual GetDrawingVisualForNode(uint nodeId)
        {
            if (IdGenerator.GetType(nodeId) != ComponentType.Node)
                throw new ArgumentException("Invalid node id (C91251A90A4A)", "nodeId");

            if (this.nodeVisuals.ContainsValue(nodeId))
            {
                KeyValuePair<DrawingVisual, uint> found =
                    this.nodeVisuals.First((x) => (x.Value == nodeId));

                return found.Key;
            }

            // A drawing visual wasn't found, create a new one.
            DrawingVisual drawingVisual = new DrawingVisual();
            drawingVisual.Transform = new TranslateTransform();
            nodeVisuals.Add(drawingVisual, nodeId);
            this.children.Add(drawingVisual);
            return drawingVisual;
        }

        public DrawingVisual GetDrawingVisualForEdge(uint edgeId)
        {
            if (0 != edgeId) // "edgeId" can either be 0, or a valid Edge Id.
            {
                if (IdGenerator.GetType(edgeId) != ComponentType.Edge)
                    throw new ArgumentException("Invalid edge id (C92A4D483EA)", "edgeId");
            }

            if (this.edgeVisuals.ContainsValue(edgeId))
            {
                KeyValuePair<DrawingVisual, uint> found =
                    this.edgeVisuals.First((x) => (x.Value == edgeId));

                if (0 == edgeId)
                {
                    children.Remove(found.Key);
                    children.Add(found.Key);
                }

                return found.Key;
            }

            // Drawing visual wasn't found, create a new one for it.
            DrawingVisual drawingVisual = new DrawingVisual();
            edgeVisuals.Add(drawingVisual, edgeId);
            this.children.Insert(0, drawingVisual);
            return drawingVisual;
        }

        public DrawingVisual GetDrawingVisualForBubble(uint bubbleId)
        {
            foreach (var visual in bubbleVisuals)
            {
                if (visual.Value == bubbleId)
                {
                    return visual.Key;
                }
            }

            DrawingVisual drawingVisual = new DrawingVisual();
            drawingVisual.Transform = new TranslateTransform();
            bubbleVisuals.Add(drawingVisual, bubbleId);
            this.children.Add(drawingVisual);
            return drawingVisual;
        }

        public DrawingVisual GetDrawingVisualForSelectionBox()
        {
            if (this.selectionBoxVisual == null)
            {
                this.selectionBoxVisual = new DrawingVisual();
                this.children.Add(selectionBoxVisual);
            }
            this.children.Remove(selectionBoxVisual);
            this.children.Insert(0, selectionBoxVisual);
            return selectionBoxVisual;
        }

        // if it is node, once it is selected, it will be brought to the top
        // if it is bubble, if its selected, it will follow its owners' z-index, if unselected, send to back,
        // edge is different, when the edge is moving, it is drawn in another drawingVisual which is topmost,
        // when the edge moving is done, it will draw on the old/own drawingVisual
        //
        public void RearrangeDrawingVisual(uint compId, bool bringToFront, uint ownerId)
        {
            DrawingVisual drawingVisual = null;

            if ((nodeVisuals.Count == 0) && (edgeVisuals.Count == 0))
                throw new InvalidOperationException();

            switch (IdGenerator.GetType(compId))
            {
                case ComponentType.Node:
                    {
                        KeyValuePair<DrawingVisual, uint> selected =
                                    nodeVisuals.FirstOrDefault((x) => (x.Value == compId));
                        drawingVisual = selected.Key;
                        break;
                    }

                case ComponentType.Bubble:
                    {
                        KeyValuePair<DrawingVisual, uint> selected =
                                    bubbleVisuals.FirstOrDefault((x) => (x.Value == compId));
                        drawingVisual = selected.Key;
                        break;
                    }
                default:
                    return;
            }

            if (drawingVisual == null)
                return;

            children.Remove(drawingVisual);
            if (bringToFront)
            {
                children.Add(drawingVisual);
                return;
            }

            // for bubble only (node no need to put to the layers at the back)
            int layer = -1;
            if (ownerId != uint.MaxValue)
            {
                DrawingVisual nodeVisual = GetDrawingVisualForNode(ownerId);
                if (nodeVisual != null)
                    layer = children.IndexOf(nodeVisual);
                else
                    layer = -1;
            }
            else
            {
                // bubble should be above the arc while below the node
                layer = GetVisualLayerForBubble();
            }

            if (layer < 0) // failed to get a proper layer for preview, usually shouldn't
                children.Add(drawingVisual);
            else
                children.Insert(layer, drawingVisual);
        }

        public bool RemoveDrawingVisual(uint compId)
        {
            if ((nodeVisuals.Count == 0) && (edgeVisuals.Count == 0))
                throw new InvalidOperationException();

            switch (IdGenerator.GetType(compId))
            {
                case ComponentType.Node:
                    {
                        KeyValuePair<DrawingVisual, uint> selected =
                            nodeVisuals.FirstOrDefault((x) => (x.Value == compId));

                        if (selected.Key == null)
                            return false;

                        nodeVisuals.Remove(selected.Key);
                        children.Remove(selected.Key);
                        break;
                    }
                case ComponentType.Edge:
                    {
                        KeyValuePair<DrawingVisual, uint> selected =
                            edgeVisuals.FirstOrDefault((x) => (x.Value == compId));

                        if (selected.Key == null)
                            return false;

                        edgeVisuals.Remove(selected.Key);
                        children.Remove(selected.Key);
                        break;
                    }
                case ComponentType.Bubble:
                    {
                        KeyValuePair<DrawingVisual, uint> selected =
                            bubbleVisuals.FirstOrDefault((x) => (x.Value == compId));

                        if (selected.Key == null)
                            return false;

                        bubbleVisuals.Remove(selected.Key);
                        children.Remove(selected.Key);
                        break;
                    }
                default:
                    return false;
            }
            return true;
        }

        public void ExtendBubble(uint bubbleId, string extendedString, double width, double height)
        {
            IInfoBubble bubble;
            graphController.GetInfoBubble(bubbleId, out bubble);

            previewTextbox = new ExtendedPreview(extendedString, width, height, graphControl, this);

            graphCanvas.AddTextbox(previewTextbox);
            extendedBubble.Add(previewTextbox);
            previewTextbox.Tag = bubbleId;
            previewTextbox.Visibility = Visibility.Visible;

            previewTextbox.SetValue(Canvas.LeftProperty, (double)(int)(bubble.AnchorPoint.X - (previewTextbox.InternalTextBox.Width + Configurations.PreviewBubbleExtendedScrollBarWidth - 2) / 2));
            previewTextbox.SetValue(Canvas.TopProperty, bubble.AnchorPoint.Y);

            previewTextbox.PreviewMouseLeftButtonDown += OnPreviewTextboxMouseDown;
            previewTextbox.InternalTextBox.CaptureMouse();
        }

        public void GetExtendedBubbleSize(uint bubbleId, out double x, out double y)
        {
            ExtendedPreview bubble = null;
            foreach (ExtendedPreview previewTextBox in extendedBubble)
            {
                if ((uint)previewTextbox.Tag == bubbleId)
                {
                    bubble = previewTextbox;
                    break;
                }
            }
            if (bubble != null)
            {
                x = bubble.InternalTextBox.ActualWidth + Configurations.PreviewBubbleExtendedScrollBarWidth;
                y = bubble.InternalTextBox.ActualHeight;
            }
            else
            {
                x = 0;
                y = 0;
            }
        }

        public void RemoveExtendedBubble(uint bubbleId)
        {
            ExtendedPreview bubbleToRemove = null;
            foreach (ExtendedPreview previewTextBox in extendedBubble)
            {
                if ((uint)previewTextbox.Tag == bubbleId)
                {
                    bubbleToRemove = previewTextbox;
                    break;
                }
            }
            if (bubbleToRemove != null)
            {
                graphCanvas.RemoveTextBox(bubbleToRemove);
                extendedBubble.Remove(bubbleToRemove);
            }
        }

        public void ScrollToVisual(DrawingVisual drawingVisual)
        {
            Rect visualBound = drawingVisual.ContentBounds;
            TranslateTransform translateTransform = drawingVisual.Transform as TranslateTransform;

            if (translateTransform != null)
                visualBound.Offset(new Vector(translateTransform.X, translateTransform.Y));

            double offsetX;
            double offsetY;
            double scale;
            //GetScrollOffset(visualBound, out offsetX, out offsetY, Key.None);
            GetScaleScrollOffset(visualBound, out scale, out offsetX, out offsetY);

            ZoomAndPanAnimationHelper animationHelper = new ZoomAndPanAnimationHelper(
                                                offsetX, offsetY, scale, this, this.graphControl, true);
            //double offsetX;
            //double offsetY;
            //GetScrollOffset(visualBound, out offsetX, out offsetY, Key.None);

            //ZoomAndPanAnimationHelper animationHelper = new ZoomAndPanAnimationHelper(
            //                                    offsetX, offsetY, this, this.graphControl);
        }

        public void ScrollToMenuVisual(DrawingVisual drawingVisual, double width)
        {
            Rect visualBound = drawingVisual.ContentBounds;
            TranslateTransform translateTransform = drawingVisual.Transform as TranslateTransform;

            if (translateTransform != null)
                visualBound.Offset(new Vector(translateTransform.X, translateTransform.Y));

            if (width > double.MinValue)
                visualBound.Width = width;

            double offsetX;
            double offsetY;
            double scale;
            //GetScrollOffset(visualBound, out offsetX, out offsetY, Key.None);
            GetScaleScrollOffset(visualBound, out scale, out offsetX, out offsetY);

            ZoomAndPanAnimationHelper animationHelper = new ZoomAndPanAnimationHelper(
                                                offsetX, offsetY, scale, this, this.graphControl, true);
        }

        #endregion

        #region Public Class Operational Methods

        public GraphVisualHost(GraphCanvas graphCanvas, GraphControl graphControl, string startupFile)
        {
            this.graphCanvas = graphCanvas;
            this.graphControl = graphControl;

            if (children == null)
                children = new VisualCollection(this);

            if (string.IsNullOrEmpty(startupFile))
                this.graphController = ClassFactory.CreateGraphController(this);
            else
            {
                try
                {
                    this.graphController = ClassFactory.CreateGraphController(this, startupFile);
                }
                catch (FileNotFoundException e)
                {
                    MessageBox.Show(string.Format("FileNotFoundException: {0}", e.FileName));
                }
                catch (IOException e)
                {
                    MessageBox.Show(string.Format("IOException: {0}", e.Message));
                }
                catch (InvalidDataException e)
                {
                    MessageBox.Show(string.Format("InvalidDataException: {0}", e.Message));
                }
                catch (FileVersionException e)
                {
                    string message = string.Format(UiStrings.FutureFileVersionFmt, e.RequiredAppVersion);

                    MessageBoxResult result = MessageBoxResult.None;
                    string caption = UiStrings.IncompatibleVersion;
                    result = MessageBox.Show(message, caption, MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK)
                    {
                        System.Diagnostics.Process.Start(CoreStrings.DesignScriptSiteUrl);
                    }
                }
                catch (Exception e)
                {
                    this.graphController = ClassFactory.CreateGraphController(this);
                    this.graphControl.DisplayException(e);
                }
                finally
                {
                    if (null == graphController)
                        this.graphController = ClassFactory.CreateGraphController(this);
                }
            }

            DrawingVisual foregroundVisual = new DrawingVisual();
            edgeVisuals.Add(foregroundVisual, 0);
            this.children.Add(foregroundVisual);

            if (!string.IsNullOrEmpty(startupFile))
                this.ZoomToFit();
        }

        public HitTestResultBehavior VisualCallBack(HitTestResult result)
        {
            if (result == null || result.VisualHit == null)
                throw new ArgumentNullException();

            if (result.VisualHit is DrawingVisual)
                hitVisual = (DrawingVisual)result.VisualHit;

            if (graphController.CurrentDragState == DragState.None)
            {
                return HitTestResultBehavior.Stop;
            }
            else
            {
                if (nodeVisuals.ContainsKey(hitVisual))
                {
                    return HitTestResultBehavior.Stop;
                }
                else
                    return HitTestResultBehavior.Continue;
            }
        }

        internal string DumpRecordedStates(bool dumpUiStates)
        {
            if (null == graphController)
                return string.Empty;

            if (false != dumpUiStates)
                return graphController.DumpRecordedUiStates();
            else
                return graphController.DumpRecordedVmStates();
        }

        #endregion

        #region Public User Input Event Handlers

        internal void HandleKeyDown(object sender, KeyEventArgs e)
        {
            //Self-TODO(Joy):this function need to be revised 
            if (radialMenu != null)
            {
                if (subRadialMenu != null)
                    subRadialMenu.UpdateKeyword(e);
                else
                    radialMenu.UpdateKeyword(e);
                graphCanvas.graphCanvas.Focus();
                e.Handled = true;
                return;
            }

            ModifierKeys modifier = Keyboard.Modifiers;
            Key k = e.Key;

            if (modifier.HasFlag(ModifierKeys.Control) && e.Key == Key.A)
            {
                foreach (uint nodeId in nodeVisuals.Values)
                    graphController.DoSelectComponent(nodeId, ModifierKeys.None);
                ZoomToFit();
                return;
            }
            if (modifier.HasFlag(ModifierKeys.Shift))
                return;
            if (e.Key == Key.Delete || e.Key == Key.Enter || e.Key == Key.Back)
                return;

            //scroll to left/right/top/bottom of the working space
            if (e.Key == Key.Home
                || e.Key == Key.End
                || e.Key == Key.Prior || e.Key == Key.PageUp
                || e.Key == Key.Next || e.Key == Key.PageDown)
            {
                Rect workingRegion = GetNodeRegion();
                if (workingRegion == Rect.Empty)
                    return;

                double offsetX, offsetY;
                GetScrollOffset(workingRegion, out offsetX, out offsetY, e.Key);

                ZoomAndPanAnimationHelper animationHelper = new ZoomAndPanAnimationHelper(
                                                offsetX, offsetY, this, this.graphControl);

                e.Handled = true;
                return;
            }
        }

        internal void HandleKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (panning)
                {
                    EndPan();
                    return;
                }

                if (graphController.CurrentDragState == DragState.CurveDrawing
                    || graphController.CurrentDragState == DragState.EdgeReconnection
                    || graphController.CurrentDragState == DragState.RegionSelection)
                {
                    graphController.DoEndDrag(MouseButton.Left, 0, NodePart.None, -1, ModifierKeys.None, -1, -1);
                    lastClickedNodeId = uint.MaxValue; // reset
                    lastClickedNodeIndex = int.MaxValue;
                    lastClickedNodePart = NodePart.None;
                    mouseDownPosition = new Point(-1, -1);
                    graphControl.SetCursor(CursorSet.Index.Pointer);
                }

                graphController.DoClearSelection();
                return;
            }

            if (e.Key == Key.Home
                || e.Key == Key.End
                || e.Key == Key.Prior || e.Key == Key.PageUp
                || e.Key == Key.Next || e.Key == Key.PageDown)
            {
                e.Handled = true;
                return;
            }
        }

        internal void HandleMouseMove(object sender, MouseEventArgs e)
        {
            if (animationOn)
                return;
            if (editTextbox != null) //prevent the code blocks from moving
                return;

            bool cursorSet = false;

            ModifierKeys modifiers = Keyboard.Modifiers;
            uint nodeId = uint.MaxValue;
            uint edgeId = uint.MaxValue;
            int index = -1;
            NodePart nodePart = NodePart.None;
            DrawingVisual drawingVisual;

            // Pan
            ScrollViewer scrollViewer = graphControl.canvasScrollViewer;
            Point mouseScreenPosition = e.GetPosition(scrollViewer);
            if (panning)
            {
                if (!isMouseButtonDown)
                {
                    graphControl.SetCursor(CursorSet.Index.HandPan);
                    return;
                }

                graphControl.SetCursor(CursorSet.Index.HandPanActive);

                double shiftX = mouseScreenPosition.X - mouseDownScreenPosition.X;
                double shiftY = mouseScreenPosition.Y - mouseDownScreenPosition.Y;

                mouseDownScreenPosition = mouseScreenPosition;

                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - shiftX);
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - shiftY);

                return;
            }
            //Auto Pan the canvas when mouse near the border of the window
            AutoPan(mouseScreenPosition);

            if (graphController.CurrentDragState == DragState.None)
            {
                // In the section below for the "BeginDrag", we are using the mouseDownPosition instead current mouse position
                // The reason is that once we mousedown and move the mouse so fast then the mousePosition is so far away the mouseDownPosition
                // IDE-1509
                //
                drawingVisual = FindChildren(mouseDownPosition);
                if (drawingVisual != null)
                {
                    if (nodeVisuals.TryGetValue(drawingVisual, out nodeId))
                        graphController.HitTestComponent(nodeId, mouseDownPosition, out nodePart, out index);
                    else
                    {
                        // If the mouse cursor is over an edge, then we'll reset 
                        // "nodeId" to indicate there's no node being hit right now.
                        nodeId = uint.MaxValue;
                    }
                }

                bool ignorDrag = false;

                if (!((lastClickedNodeId == nodeId)
                 && (lastClickedNodeIndex == index)
                 && (lastClickedNodePart == nodePart))  //if mouse not on the same slot
                    && (!isMouseButtonDown))
                    ignorDrag = true;

                if (!ignorDrag)
                {
                    MouseButton button = MouseButton.Left;
                    if (e.RightButton == MouseButtonState.Pressed)
                        button = MouseButton.Right;

                    //(TODO:Joy)(? from Vic: is it the correct place for it?)
                    //remove the radial menu when dragging
                    RemoveRadialMenu();
                    RemoveSubRadialMenu();
                    // Begin graph controller into a dragging mode since 
                    // this is the first mouse move sample after mouse down.
                    graphController.DoBeginDrag(button, nodeId, nodePart, index, modifiers, mouseDownPosition.X, mouseDownPosition.Y);
                }
            }

            nodeId = uint.MaxValue;
            index = -1;
            nodePart = NodePart.None;
            mousePosition = e.GetPosition(graphCanvas);
            drawingVisual = FindChildren(mousePosition);

            // remove extended preview, excluding ones that got selected text or got radial menu
            if (extendedBubble.Count() > 0 && e.LeftButton != MouseButtonState.Pressed)
            {
                List<ExtendedPreview> previewToRemove = new List<ExtendedPreview>();

                foreach (ExtendedPreview previewTextBox in extendedBubble)
                {
                    if (!IsMouseInExtendedBubble(mousePosition, previewTextbox)
                        && string.IsNullOrEmpty(previewTextbox.InternalTextBox.SelectedText)
                        && (uint)previewTextbox.Tag != lastHoveredBubbleId)
                        previewToRemove.Add(previewTextbox);
                }

                foreach (ExtendedPreview previewTextBox in previewToRemove)
                    graphController.TransientExtendPreviewBubble((uint)previewTextbox.Tag, false);
            }

            List<uint> ignoreTransientExtendedBubble = new List<uint>();
            if (nodeVisuals.Count > 0 && drawingVisual != null)
            {
                if (nodeVisuals.TryGetValue(drawingVisual, out nodeId))          // check for node
                {
                    graphController.HitTestComponent(nodeId, mousePosition, out nodePart, out index);
                }
                else if (edgeVisuals.TryGetValue(drawingVisual, out edgeId))     // check for edge
                {
                    // If the mouse cursor is over an edge, then we'll reset 
                    // "nodeId" to indicate there's no node being hit right now.
                    nodeId = uint.MaxValue;

                    if (!graphController.CheckEdgeType(edgeId))//implecit edge
                        edgeId = uint.MaxValue;
                }
                else                                                            // check for preview
                {
                    nodeId = uint.MaxValue;
                    edgeId = uint.MaxValue;
                    if (graphController.CurrentDragState == DragState.None)
                    {
                        uint bubbleId = uint.MaxValue;
                        if ((!FindExtendedBubbleOn(mousePosition)) && (FindBubble(drawingVisual, out bubbleId)))  // hover on condensed drawing visual
                        {
                            IInfoBubble bubble = null;
                            graphController.GetInfoBubble(bubbleId, out bubble);

                            //radial menu for bubble
                            Point pt = new Point();
                            Point anchorPoint = bubble.AnchorPoint;
                            pt.X = mousePosition.X - drawingVisual.Transform.Value.OffsetX;
                            pt.Y = mousePosition.Y - drawingVisual.Transform.Value.OffsetY;
                            anchorPoint.Offset(bubble.Width / 2, 0);
                            if (this.graphController.PreviewBubbleHittest(pt, bubbleId))
                            {
                                this.CreateRadialMenuForPreview(anchorPoint, bubbleId);
                                return;
                            }

                            if (transientExtendedBubble.Contains(bubbleId))
                                ignoreTransientExtendedBubble.Add(bubbleId);

                            // condense extended bubbles
                            List<ExtendedPreview> previewToRemove = new List<ExtendedPreview>();
                            foreach (ExtendedPreview previewTextBox in extendedBubble)
                                previewToRemove.Add(previewTextbox);

                            if (WithinBubbleToggleVisibilityRegion(mousePosition, drawingVisual))
                            {
                                if (bubble.Collapsed && !bubble.Extended)
                                {
                                    graphController.TransientShowPreviewBubble(bubbleId, true);
                                    transientExtendedBubble.Add(bubbleId);
                                    ignoreTransientExtendedBubble.Add(bubbleId);
                                }
                                else
                                {
                                    // if the extended preview box of hovered preview is found,
                                    // condense needed
                                    //
                                    ExtendedPreview hoveredPreviewBox = null;
                                    foreach (ExtendedPreview previewTextBox in extendedBubble)
                                    {
                                        if ((uint)previewTextbox.Tag == bubbleId)
                                        {
                                            hoveredPreviewBox = previewTextbox;
                                            break;
                                        }
                                    }
                                    if (hoveredPreviewBox != null)
                                        previewToRemove.Remove(hoveredPreviewBox);
                                }

                                if (!bubble.Collapsed && !transientExtendedBubble.Contains(bubbleId))
                                    graphControl.SetCursor(CursorSet.Index.Condense);
                                else
                                    graphControl.SetCursor(CursorSet.Index.Expand);
                                cursorSet = true;
                            }
                            else
                            {
                                graphController.TransientExtendPreviewBubble(bubbleId, true);
                            }

                            foreach (ExtendedPreview previewTextBox in previewToRemove)
                                graphController.TransientExtendPreviewBubble((uint)previewTextbox.Tag, false);
                        }
                    }
                }
            }

            // radial menu hovered bubble
            if (lastHoveredBubbleId != uint.MaxValue && !ignoreTransientExtendedBubble.Contains(lastHoveredBubbleId))
                ignoreTransientExtendedBubble.Add(lastHoveredBubbleId);

            // toggle to hide all the temprarily showed PreviewBubble but the currently hovered one
            foreach (uint bubbleId in transientExtendedBubble.Except(ignoreTransientExtendedBubble))
            {
                graphController.TransientShowPreviewBubble(bubbleId, false);
            }
            transientExtendedBubble.Clear();
            transientExtendedBubble.AddRange(ignoreTransientExtendedBubble);

            // radial menu
            if (drawingVisual != null)
            {
                if (subRadialMenu != null && radialMenu != null)
                {
                    if (subRadialMenu.GetRadialVisual() == drawingVisual || radialMenu.GetRadialVisual() == drawingVisual)
                    {
                        Point temp;
                        int tempId;
                        bool result = subRadialMenu.HandleMouseMove(mousePosition, out temp, out tempId);
                        if (result == false)
                        {
                            radialMenu.HandleMouseMove(mousePosition, out temp, out tempId);
                            if (tempId != selectedMenuId && tempId != int.MinValue)
                            {
                                RemoveSubRadialMenu();
                                radialMenu.UnFade();
                                selectedMenuId = tempId;
                                ResetSubRadialMenuTimer();
                            }
                        }
                    }
                    else
                    {
                        ResetSubRadialMenuTimer();
                        RemoveSubRadialMenu();
                    }
                }
                else if (subRadialMenu == null && radialMenu != null)
                {
                    if (radialMenu.GetRadialVisual() == drawingVisual)
                    {
                        Point temp;
                        int tempId;
                        bool result = radialMenu.HandleMouseMove(mousePosition, out temp, out tempId);
                        if (tempId != selectedMenuId)
                        {
                            RemoveSubRadialMenu();
                            if (selectedMenuId != tempId)
                            {
                                selectedMenuId = tempId;
                                ResetSubRadialMenuTimer();
                            }
                        }
                        if (subRadialMenu == null && result == true)
                        {
                            if (selectedMenuId == tempId && tempId > Configurations.PropertyBase)
                            {
                                CreateSubRadialMenuTimer();
                                subRadialMenuPoint = temp;
                            }
                        }
                    }
                    else
                        RemoveRadialMenu();
                }
                else if (drawingVisual == selectionBoxVisual && graphController.CurrentDragState == DragState.None)
                {
                    if (this.graphController.SelectionBoxHittest(mousePosition))
                        CreateRadialMenuForSelectionBox();
                }
                else
                {
                    if (nodePart == NodePart.NorthEast || nodePart == NodePart.NorthWest || nodePart == NodePart.North || nodePart == NodePart.South)
                    {
                        if (this.graphController.CurrentDragState == DragState.None)
                            CreateRadialMenu(nodeId, nodePart, new Point());
                    }
                }
            }
            else
            {
                RemoveRadialMenu();
                RemoveSubRadialMenu();
                ResetSubRadialMenuTimer();
            }

            // window select node
            if (graphController.CurrentDragState == DragState.RegionSelection)
            {
                List<uint> currentSelectedComponent = this.GetComponents();

                //update newly add element
                foreach (uint componentId in currentSelectedComponent.Except(selectedComponent))
                    graphController.PreviewSelectComponent(componentId, modifiers);
                //update element that not in the rectangle region anylonger
                foreach (uint componentId in selectedComponent.Except(currentSelectedComponent))
                    graphController.ClearPreviewSelection(componentId);

                selectedComponent = currentSelectedComponent;
            }

            if (!(lastClickedNodeId == nodeId
                 && lastClickedNodeIndex == index
                 && lastClickedNodePart == nodePart))
            {
                lastClickedNodeId = uint.MaxValue;
                lastClickedNodeIndex = int.MaxValue;
                lastClickedNodePart = NodePart.None;
            }

            graphController.SetMouseCursor(nodeId, nodePart, index, modifiers, isMouseButtonDown, mousePosition.X, mousePosition.Y);

            if (cursorSet)
                return;
            else if (edgeId != uint.MaxValue && edgeId != 0)
                graphControl.SetCursor(CursorSet.Index.ArcSelect);
            else if (graphController.EdgeConnection == EdgeConnectionFlag.NewEdge)
                graphControl.SetCursor(CursorSet.Index.ArcAddEnd);
            else if (graphController.EdgeConnection == EdgeConnectionFlag.ReconnectEdge)
                graphControl.SetCursor(CursorSet.Index.ArcRemove);
            else if (graphController.CurrentDragState == DragState.RegionSelection)
                graphControl.SetCursor(CursorSet.Index.RectangularSelection);
            else if ((graphController.CurrentDragState == DragState.None)
              && (nodePart == NodePart.InputSlot || nodePart == NodePart.OutputSlot))
                graphControl.SetCursor(CursorSet.Index.ArcAdd);
            else
                graphControl.SetCursor(CursorSet.Index.Pointer);
        }

        internal void HandlePreviewMouseDown()
        {
            if (editTextbox == null)
                this.graphCanvas.graphCanvas.Focus();
        }

        internal void HandleMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isMouseButtonDown = true;
            ScrollViewer scrollViewer = graphControl.canvasScrollViewer;
            mouseDownScreenPosition = e.GetPosition(scrollViewer); // for zoom and pan

            if (panning)
                return;

            if ((graphController.CurrentDragState == DragState.CurveDrawing)
                || (graphController.CurrentDragState == DragState.EdgeReconnection))
                return;

            if (extendedBubble.Count() > 0) // toggle the exteded bubbles(one with text selected)
            {
                List<ExtendedPreview> previewToRemove = new List<ExtendedPreview>();
                foreach (ExtendedPreview previewTextBox in extendedBubble)
                    previewToRemove.Add(previewTextbox);

                foreach (ExtendedPreview previewTextBox in previewToRemove)
                    graphController.TransientExtendPreviewBubble((uint)previewTextbox.Tag, false);
            }

            mousePosition = e.GetPosition(graphCanvas);
            mouseDownPosition = mousePosition;

            ModifierKeys modifiers = Keyboard.Modifiers;

            uint newCompId = uint.MaxValue;
            int index = -1;
            NodePart nodePart = NodePart.None;
            string temp = string.Empty;

            DrawingVisual visual = FindChildren(mousePosition);
            if (visual != null)
            {
                edgeVisuals.TryGetValue(visual, out newCompId);
                if (!graphController.CheckEdgeType(newCompId))//it is implicit edge
                    newCompId = uint.MaxValue;

                if ((newCompId != uint.MaxValue) && (newCompId != 0))
                {
                    if (!modifiers.HasFlag(ModifierKeys.Control) && !modifiers.HasFlag(ModifierKeys.Shift))
                        graphController.DoClearSelection();
                    graphController.DoSelectComponent(newCompId, modifiers);
                    return;
                }

                bubbleVisuals.TryGetValue(visual, out newCompId); // toggle bubble visibility
                if (newCompId != uint.MaxValue && newCompId != 0)
                {
                    IInfoBubble bubble;
                    graphController.GetInfoBubble(newCompId, out bubble);

                    if (bubble == null || !bubble.IsPreviewBubble
                        || !WithinBubbleToggleVisibilityRegion(mousePosition, visual))
                        return;

                    if (transientExtendedBubble.Contains(newCompId))
                        transientExtendedBubble.Remove(newCompId);

                    graphController.DoTogglePreview(newCompId);

                    if (bubble.Collapsed)
                        graphControl.SetCursor(CursorSet.Index.Expand);
                    else
                        graphControl.SetCursor(CursorSet.Index.Condense);
                    return;
                }

                nodeVisuals.TryGetValue(visual, out newCompId);
                graphController.HitTestComponent(newCompId, mousePosition, out nodePart, out index);

                if (radialMenu != null)
                {
                    int menuId = 0;
                    if (subRadialMenu != null)
                    {
                        menuId = subRadialMenu.HandleClick(mousePosition);
                        if (menuId > 0)
                            this.graphController.DoSelectMenuItem(menuId, mousePosition.X, mousePosition.Y, radialMenu.GetNodeId(), radialMenu.GetNodePart());
                        else if (menuId == -1) //first radial Menu clicked
                        {
                            menuId = radialMenu.HandleClick(mousePosition);
                            if (menuId > 0)
                                this.graphController.DoSelectMenuItem(menuId, mousePosition.X, mousePosition.Y, radialMenu.GetNodeId(), radialMenu.GetNodePart());
                        }
                    }
                    else
                    {
                        menuId = radialMenu.HandleClick(mousePosition);
                        if (menuId > 0)
                            this.graphController.DoSelectMenuItem(menuId, mousePosition.X, mousePosition.Y, radialMenu.GetNodeId(), radialMenu.GetNodePart());
                    }
                    if (menuId != 0)
                    {
                        RemoveRadialMenu();
                        RemoveSubRadialMenu();
                    }
                    return;
                }
            }
            else
            {
                RemoveRadialMenu();
                RemoveSubRadialMenu();
            }

            if (e.ClickCount == 2)
                HandleDoubleClick(nodePart, newCompId, index, visual);

            if (nodePart == NodePart.InputSlot || nodePart == NodePart.OutputSlot)//for click(mouse down and up) and drag to connect edge
            {
                if (lastClickedNodeId == newCompId
                     && lastClickedNodeIndex == index
                     && lastClickedNodePart == nodePart)
                {
                    lastClickedNodeId = uint.MaxValue; // reset
                    lastClickedNodeIndex = int.MaxValue;
                    lastClickedNodePart = NodePart.None;
                }
                else
                {
                    lastClickedNodeId = newCompId;
                    lastClickedNodeIndex = index;
                    lastClickedNodePart = nodePart;
                }
            }

            graphController.DoMouseDown(MouseButton.Left, newCompId, nodePart, index, modifiers);
        }

        private void HandleDoubleClick(NodePart nodePart, uint newCompId, int index, DrawingVisual visual)
        {
            graphControl.SetCursor(CursorSet.Index.Pointer);

            string temp;
            ModifierKeys modifiers = Keyboard.Modifiers;
            if (nodePart == NodePart.Caption || nodePart == NodePart.Text)
            {
                if (editTextbox == null)
                {
                    NodeType type = NodeType.None;
                    graphController.GetNodePartRegion(newCompId, nodePart, mousePosition, out currentNodePartRegion, out type);
                    //calc center position of textbox in rect
                    double x = mousePosition.X;
                    double y = mousePosition.Y;
                    x = (currentNodePartRegion.Right - currentNodePartRegion.Left) / 2;
                    y = (currentNodePartRegion.Bottom - currentNodePartRegion.Top) / 2;
                    SolidColorBrush backgroundColor;
                    FontWeight weight;

                    if (type != NodeType.CodeBlock)
                    {
                        double minTextboxWidth = 0;
                        graphController.GetTextboxMinSize(newCompId, out minTextboxWidth);
                        switch (type)
                        {
                            case NodeType.Identifier:
                                backgroundColor = Configurations.RectWhite;
                                weight = FontWeights.Normal;
                                minTextboxWidth = 0;
                                break;
                            case NodeType.Function:
                                if (nodePart == NodePart.Caption)
                                {
                                    graphController.DoMouseDown(MouseButton.Left, newCompId, nodePart, index, modifiers);
                                    mouseDownPosition = mousePosition;
                                    return;
                                }
                                else
                                {
                                    backgroundColor = Configurations.RectWhite;
                                    weight = FontWeights.SemiBold;
                                }
                                //y -= 2;
                                break;
                            case NodeType.Driver:
                                if (nodePart == NodePart.Caption)
                                    backgroundColor = Configurations.RectWhite;
                                else
                                    backgroundColor = Configurations.RectGrey;
                                weight = FontWeights.SemiBold;
                                minTextboxWidth = 0;
                                break;
                            case NodeType.Property:
                                if (nodePart == NodePart.Caption)
                                {
                                    graphController.DoMouseDown(MouseButton.Left, newCompId, nodePart, index, modifiers);
                                    mouseDownPosition = mousePosition;
                                    return;
                                }
                                else
                                {
                                    backgroundColor = Configurations.RectWhite;
                                    weight = FontWeights.SemiBold;
                                }
                                break;
                            default:
                                backgroundColor = Configurations.RectWhite;
                                weight = FontWeights.SemiBold;
                                minTextboxWidth = 0;
                                break;
                        }
                        CreateEditTextbox(false, backgroundColor, Configurations.Font, weight, Configurations.TextSize, minTextboxWidth);
                        editTextbox.Alignment = TextAlignment.Center;
                        editTextbox.SelectAll = true;
                        x += currentNodePartRegion.Left - 3;//-3
                        y += currentNodePartRegion.Top - 2;//-2
                        graphController.GetNodeText(newCompId, nodePart, out temp);
                        editTextbox.Text = temp;
                        if (temp.Length > 25)
                            temp = temp.Substring(0, 25);
                        FormattedText newText = new FormattedText(temp,
                            Configurations.culture,
                            FlowDirection.LeftToRight,
                            Configurations.TypeFace,
                            Configurations.TextSize,
                            Configurations.TextNormalColor);
                        //use gettextsize() from utilities
                        double offset = newText.WidthIncludingTrailingWhitespace;
                        if (offset < minTextboxWidth)
                            offset = minTextboxWidth;

                        x -= ((offset) / 2);
                        y -= Configurations.TextSize / 2;
                    }
                    else
                    {
                        CreateEditTextbox(true, Configurations.RectWhite, Configurations.Font, FontWeights.Normal, Configurations.TextSize, 0);
                        graphController.GetNodeText(newCompId, nodePart, out temp);
                        x = currentNodePartRegion.X + Configurations.TextHorizontalOffset;
                        y = currentNodePartRegion.Y + Configurations.TextVerticalOffset;

                        if (temp == Configurations.CodeBlockInitialMessage)
                            editTextbox.Text = "";
                        else
                            editTextbox.Text = temp;
                    }
                    Point cursorScreenPos = mousePosition;
                    editTextbox.SetCursorScreenPos(cursorScreenPos);
                    editTextbox.SetValue(Canvas.LeftProperty, x);
                    editTextbox.SetValue(Canvas.TopProperty, y);
                    nodeEditFlag = true;
                    nodeEditId = newCompId;
                    graphController.DoBeginNodeEdit(nodeEditId, nodePart);
                    graphController.UpdateNodeText(newCompId, editTextbox.Text);
                }
            }
            else
            {
                if (nodeEditFlag == false && nodePart == NodePart.None && visual == null)
                {
                    //create textbox
                    if (editTextbox == null)
                    {
                        CreateEditTextbox(true, Configurations.RectWhite, Configurations.Font, FontWeights.Normal, Configurations.TextSize, 0);
                        editTextbox.SetValue(Canvas.LeftProperty, mousePosition.X - 3);
                        editTextbox.SetValue(Canvas.TopProperty, mousePosition.Y - Configurations.TextVerticalOffset - 1);
                        editTextbox.UpdateCursor();
                    }
                    graphController.DoCreateCodeBlockNode(mousePosition.X, mousePosition.Y, editTextbox.Text);
                    editTextbox.Text = Configurations.CodeBlockInitialMessage;
                    graphController.GetLastNodeId(out newCompId);
                    nodeEditFlag = true;
                    nodeEditId = newCompId;
                    graphController.DoBeginNodeEdit(newCompId, NodePart.Text);
                    graphController.UpdateNodeText(newCompId, editTextbox.Text);
                }
            }

            if (nodePart == NodePart.ReplicationGuide)
            {
                //replication guide edit
                Rect rect = new Rect();
                NodeType type = NodeType.None;
                int replicationIndex;
                int slotIndex;
                nodeEditFlag = true;
                replicationEditFlag = true;
                nodeEditId = newCompId;
                graphController.GetNodePartRegion(newCompId, nodePart, mousePosition, out rect, out type);
                CreateEditTextbox(false, Configurations.RectWhite, Configurations.Font, FontWeights.Normal, Configurations.TextSize, 25);
                graphController.GetReplicationIndex(newCompId, mousePosition, out replicationIndex, out slotIndex);
                string tempText = string.Empty;
                if (replicationIndex != -1)
                {
                    this.editingReplicationIndex = slotIndex;
                    graphController.GetReplicationText(newCompId, slotIndex, out tempText);
                    if (tempText == string.Empty)
                    {
                        tempText = Configurations.ReplicationInitialString;
                    }
                    else
                        editTextbox.SetCursorScreenPos(mouseDownPosition);
                    editTextbox.Text = tempText;
                    editTextbox.SetValue(Canvas.LeftProperty, rect.Location.X - 7);
                    editTextbox.SetValue(Canvas.TopProperty, rect.Location.Y + 1);
                    graphController.DoEditReplicationGuide(newCompId, slotIndex);
                }
            }
        }

        internal void HandleMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isMouseButtonDown = false;

            if (panning)
            {
                graphControl.SetCursor(CursorSet.Index.HandPan);
                return;
            }

            ModifierKeys modifiers = Keyboard.Modifiers;
            mousePosition = e.GetPosition(graphCanvas);
            uint newCompId = uint.MaxValue;
            int index = -1;
            NodePart nodePart = NodePart.None;

            DrawingVisual visual = FindChildren(mousePosition);

            if (visual != null)
            {
                edgeVisuals.TryGetValue(visual, out newCompId);
                if ((newCompId != uint.MaxValue) && (newCompId != 0) && graphController.CurrentDragState == DragState.None)
                {
                    ScrollToVisual(visual);
                    return;
                }

                nodeVisuals.TryGetValue(visual, out newCompId);
                graphController.HitTestComponent(newCompId, mousePosition, out nodePart, out index);
            }

            if (graphController.CurrentDragState == DragState.RegionSelection)
            {
                foreach (uint compId in selectedComponent)
                    graphController.DoSelectComponent(compId, modifiers);
            }

            if (graphController.CurrentDragState != DragState.None)
            {
                // The graph controller is in a dragging state when mouse button 
                // is released, send a signal for it to get out of dragging mode.
                bool ignoreEndDrag = false;

                if (graphController.CurrentDragState == DragState.CurveDrawing
                    || graphController.CurrentDragState == DragState.EdgeReconnection)
                {
                    if ((lastClickedNodeId == newCompId)
                     && (lastClickedNodeIndex == index)
                     && (lastClickedNodePart == nodePart))  //if release mouse on the same slot
                        ignoreEndDrag = true;
                }

                if (!ignoreEndDrag)
                {
                    graphController.DoEndDrag(MouseButton.Left, newCompId, nodePart,
                        index, modifiers, mousePosition.X, mousePosition.Y);
                    mouseDownPosition = new Point(-1, -1);
                    lastClickedNodeId = uint.MaxValue; // reset
                    lastClickedNodeIndex = int.MaxValue;
                    lastClickedNodePart = NodePart.None;
                }
            }

            graphController.DoMouseUp(MouseButton.Left, newCompId, nodePart, index, modifiers);
        }

        internal void HandleMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            isMouseButtonDown = true;

            if (graphController.CurrentDragState == DragState.CurveDrawing
                    || graphController.CurrentDragState == DragState.EdgeReconnection)
            {
                graphController.DoEndDrag(MouseButton.Left, 0, NodePart.None, -1, ModifierKeys.None, -1, -1);
                lastClickedNodeId = uint.MaxValue; // reset
                lastClickedNodeIndex = int.MaxValue;
                lastClickedNodePart = NodePart.None;
                mouseDownPosition = new Point(-1, -1);
                graphControl.SetCursor(CursorSet.Index.Pointer);
                return;
            }

            ScrollViewer scrollViewer = graphControl.canvasScrollViewer;
            mouseDownScreenPosition = e.GetPosition(scrollViewer);
            BeginPan();

            //mousePosition = e.GetPosition((UIElement)sender);
            //ModifierKeys modifiers = Keyboard.Modifiers;
            //uint newCompId = uint.MaxValue;
            //int index = -1;
            //NodePart nodePart = NodePart.None;
            //string temp = string.Empty;

            //DrawingVisual visual = FindChildren(mousePosition);
            //if (visual != null)
            //{
            //    nodeVisuals.TryGetValue(visual, out newCompId);
            //    graphController.HitTestComponent(newCompId, mousePosition, out nodePart, out index);
            //}

            //if (nodePart != NodePart.None && nodePart != NodePart.Preview && nodePart != NodePart.PreviewNorthEast && nodePart != NodePart.PreviewNorthWest)
            //{
            //    graphController.DoSelectComponent(newCompId,modifiers);
            //    return;
            //}
        }

        internal void HandleMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            isMouseButtonDown = false;

            if (panning)
            {
                EndPan();
                return;
            }

            //mousePosition = e.GetPosition((UIElement)sender);
            //ModifierKeys modifiers = Keyboard.Modifiers;
            //uint newCompId = uint.MaxValue;
            //int index = -1;
            //NodePart nodePart = NodePart.None;
            //string temp = string.Empty;

            //DrawingVisual visual = FindChildren(mousePosition);
            //if (visual != null)
            //{
            //    nodeVisuals.TryGetValue(visual, out newCompId);
            //    graphController.HitTestComponent(newCompId, mousePosition, out nodePart, out index);
            //}

            //if (nodePart != NodePart.None && nodePart!= NodePart.Preview && nodePart!= NodePart.PreviewNorthEast && nodePart!= NodePart.PreviewNorthWest)
            //    CreateRadialMenu(newCompId, nodePart);
        }

        internal void HandleMouseMiddleButtonDown(object sender, MouseButtonEventArgs e)
        {
            isMouseButtonDown = true;
            ScrollViewer scrollViewer = graphControl.canvasScrollViewer;
            mouseDownScreenPosition = e.GetPosition(scrollViewer);
            BeginPan();
        }

        internal void HandleMouseMiddleButtonUp(object sender, MouseButtonEventArgs e)
        {
            isMouseButtonDown = false;

            if (panning)
            {
                EndPan();
            }
        }

        internal void HandleMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (animationOn)
                return;

            ModifierKeys modifiers = Keyboard.Modifiers;
            if (radialMenu != null)
            {
                if (subRadialMenu != null)
                {
                    if (e.Delta > 0)
                        subRadialMenu.HandleScrollUp();
                    else if (e.Delta < 0)
                        subRadialMenu.HandleScrollDown();
                }
                else
                {
                    RemoveSubRadialMenu();
                    if (e.Delta > 0)
                        radialMenu.HandleScrollUp();
                    else if (e.Delta < 0)
                        radialMenu.HandleScrollDown();
                }
                e.Handled = true;
                return;
            }
            else
            {
                ScrollViewer scrollViewer = graphControl.canvasScrollViewer;

                if (modifiers.HasFlag(ModifierKeys.Control)) // Pan up / down 
                {
                    if (e.Delta > 0)
                    {
                        scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - Configurations.PanningSpeed);
                        //ZoomAndPanAnimationHelper animationHelper = new ZoomAndPanAnimationHelper(0, -Configurations.PanningSpeed, this, this.graphControl);
                    }
                    if (e.Delta < 0)
                    {
                        scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + Configurations.PanningSpeed);
                        //ZoomAndPanAnimationHelper animationHelper = new ZoomAndPanAnimationHelper(0, Configurations.PanningSpeed, this, this.graphControl);
                    }
                }
                else if (modifiers.HasFlag(ModifierKeys.Shift)) // Pan left / right 
                {
                    if (e.Delta > 0)
                    {
                        scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - Configurations.PanningSpeed);
                        //ZoomAndPanAnimationHelper animationHelper = new ZoomAndPanAnimationHelper(-Configurations.PanningSpeed, 0, this, this.graphControl);
                    }
                    if (e.Delta < 0)
                    {
                        scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + Configurations.PanningSpeed);
                        //ZoomAndPanAnimationHelper animationHelper = new ZoomAndPanAnimationHelper(Configurations.PanningSpeed, 0, this, this.graphControl);
                    }
                }
                else // Zoom in/out
                {
                    zoomToCursor = true;
                    sliderScrollHandled = false;

                    if (e.Delta > 0)
                    {
                        ZoomIn();
                    }
                    if (e.Delta < 0)
                    {
                        ZoomOut();
                    }
                }
                e.Handled = true;
            }
        }

        internal void HandleZoomToFit()
        {
            ZoomToFit();
            if (editTextbox != null)
                editTextbox.InternalTextBox.Focus();
        }

        internal void HandleZoomIn()
        {
            zoomToCursor = false;

            ZoomIn();
            if (editTextbox != null)
                editTextbox.InternalTextBox.Focus();
        }

        internal void HandleZoomOut()
        {
            zoomToCursor = false;

            ZoomOut();
            if (editTextbox != null)
                editTextbox.InternalTextBox.Focus();
        }

        internal void HandleTogglePan()
        {
            TogglePan();
        }

        internal void HandleSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sliderScrollHandled)
                return;

            // Scroll
            ScrollViewer scrollViewer = graphControl.canvasScrollViewer;
            if (zoomToCursor)
            {
                zoomPanPivotPoint = Mouse.GetPosition(graphCanvas);
            }
            else
            {
                var centerOfViewport = new Point(scrollViewer.ViewportWidth / 2, scrollViewer.ViewportHeight / 2);
                zoomPanPivotPoint = scrollViewer.TranslatePoint(centerOfViewport, graphCanvas);
            }

            ScaleTransform scaleTransform = graphControl.scaleTransform;
            scaleTransform.ScaleX = e.NewValue;
            scaleTransform.ScaleY = e.NewValue;

            scroll = false; // this is the only entry for the HandleScrollViewerScrollChanged
        }

        internal void HandleScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (scroll || sliderScrollHandled)
                return;

            if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0)
            {
                Grid grid = graphControl.canvasGrid;
                ScrollViewer scrollViewer = graphControl.canvasScrollViewer;

                Point targetBefore;
                Point targetNow;

                targetBefore = zoomPanPivotPoint;
                if (zoomToCursor)
                {
                    targetNow = Mouse.GetPosition(graphCanvas);
                }
                else
                {
                    var centerOfViewport = new Point(scrollViewer.ViewportWidth / 2, scrollViewer.ViewportHeight / 2);
                    Point centerOfTargetNow = scrollViewer.TranslatePoint(centerOfViewport, graphCanvas);
                    targetNow = centerOfTargetNow;
                }

                double shiftX = targetNow.X - targetBefore.X;
                double shiftY = targetNow.Y - targetBefore.Y;

                double multiplicatorX = e.ExtentWidth / grid.Width;
                double multiplicatorY = e.ExtentHeight / grid.Height;

                double newOffsetX = scrollViewer.HorizontalOffset - shiftX * multiplicatorX;
                double newOffsetY = scrollViewer.VerticalOffset - shiftY * multiplicatorY;

                if (double.IsNaN(newOffsetX) || double.IsNaN(newOffsetY))
                {
                    return;
                }

                scrollViewer.ScrollToHorizontalOffset(newOffsetX);
                scrollViewer.ScrollToVerticalOffset(newOffsetY);
            }
            scroll = true;
        }

        internal void HandleDrop(object sender, LibraryItem item, DragEventArgs e)
        {
            if (graphController.CurrentDragState != DragState.None)
                return;

            System.Windows.Point mouse = e.GetPosition(graphCanvas);

            CreateNode(item, mouse);
        }

        internal void HandleAddNodeToCanvas(LibraryItem item)
        {
            ScrollViewer scrollViewer = graphControl.canvasScrollViewer;

            Random rand = new Random();
            int xShift = rand.Next(-Configurations.AddNodeToCanvasRegion, Configurations.AddNodeToCanvasRegion);
            int yShift = rand.Next(-Configurations.AddNodeToCanvasRegion, Configurations.AddNodeToCanvasRegion);

            Point centerPosition = new Point(scrollViewer.ActualWidth / 2 + xShift, scrollViewer.ActualHeight / 2 + yShift);
            Point centerPositionOnCanvas = scrollViewer.TranslatePoint(centerPosition, graphControl.CurrentGraphCanvas);
            CreateNode(item, centerPositionOnCanvas);
        }

        internal void RemoveRadialMenu()
        {
            if (radialMenu != null)
            {
                children.Remove(radialMenu.GetRadialVisual());
                radialMenu = null;

                lastHoveredBubbleId = uint.MaxValue;
                //lastHoveredNodeId = uint.MaxValue;
            }
        }

        #endregion

        #region Public Shortcut Key Event Handlers

        internal bool HandleConvertNodesToCode()
        {
            return graphController.DoConvertSelectionToCode();
        }

        internal bool HandleShortcutSave()
        {
            string currentFilePath = graphController.FilePath;
            if (string.IsNullOrEmpty(currentFilePath))
            {
                string newPath = SavePathDialog();
                if (string.IsNullOrEmpty(newPath))
                    return false;
                else
                    currentFilePath = newPath;
            }

            if (graphController.DoSaveGraph(currentFilePath))
            {
                this.graphControl.GetTabControl().UpdateTabText(this.Controller.Identifier, currentFilePath);
                return true;
            }
            else
                return false;
        }

        internal bool HandleShortcutSaveAs()
        {
            string currentFilePath = SavePathDialog();
            if (string.IsNullOrEmpty(currentFilePath))
                return false;

            if (graphController.DoSaveGraph(currentFilePath))
            {
                this.graphControl.GetTabControl().UpdateTabText(this.Controller.Identifier, currentFilePath);
                return true;
            }
            else
                return false;
        }

        internal void HandleShortcutUndo()
        {
            graphController.DoUndoOperation();
        }

        internal void HandleShortcutRedo()
        {
            graphController.DoRedoOperation();
        }

        internal void HandleShortcutDelete()
        {
            if (nodeEditFlag == true)
            {
                graphController.DoEndNodeEdit(nodeEditId, "", false);
                nodeEditId = uint.MaxValue;
                nodeEditFlag = false;
            }

            RemoveRadialMenu();

            if (editTextbox != null)
                RemoveTextbox(editTextbox);

            if (graphController.SelectedComponentsCount() > 0)
                graphController.DoDeleteComponents();
        }

        #endregion

        #region Public Class Properties

        public IGraphController Controller { get { return graphController; } }

        #endregion

        #region Protected Override Methods

        protected override int VisualChildrenCount
        {
            get { return children.Count; }
        }

        // Provide a required override for the GetVisualChild method.
        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= children.Count)
            {
                throw new ArgumentOutOfRangeException();
            }
            return children[index];
        }

        #endregion

        #region Private Class Event Handlers

        private void OnInternalTextBoxLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (editTextbox == null)
                return;

            // Fix: IDE-1336 Right-click to display context menu while editing 
            // a node ends the edit. If the new focus is a context menu, then 
            // do not dismiss the current edit box.
            ContextMenu focusedMenu = e.NewFocus as ContextMenu;
            if (null != focusedMenu)
                return;

            if (nodeEditFlag == true && nodeEditId != uint.MaxValue)
            {
                if (replicationEditFlag != true)
                    graphController.DoEndNodeEdit(nodeEditId, editTextbox.Text, true); //attempting commit
                else if (replicationEditFlag == true)
                {
                    int replicationIndex;
                    int slotIndex;
                    Point replicationPosition = new Point((double)editTextbox.GetValue(Canvas.LeftProperty) + 2, (double)editTextbox.GetValue(Canvas.TopProperty));
                    graphController.GetReplicationIndex(nodeEditId, replicationPosition, out replicationIndex, out slotIndex);
                    //graphController.DoEditReplicationGuide(nodeEditId, replicationIndex, slotIndex, editTextbox.Text);
                    if (editTextbox.Text == Configurations.ReplicationInitialString)
                        editTextbox.Text = string.Empty;
                    graphController.DoSetReplicationGuideText(nodeEditId, slotIndex, editTextbox.Text);
                }
            }

            editTextbox.Text = "";
            nodeEditId = uint.MaxValue;
            nodeEditFlag = false;
            replicationEditFlag = false;
            RemoveTextbox(editTextbox);
        }

        private void OnInternalTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (editTextbox != null) // trace why is this condition required
            {
                if (editTextbox.IsKeyboardFocusWithin == true && nodeEditFlag == true && replicationEditFlag != true)
                    graphController.UpdateNodeText(nodeEditId, editTextbox.Text);
                else if (editTextbox.IsKeyboardFocusWithin == true && nodeEditFlag == true && replicationEditFlag == true)
                    graphController.TransientUpdateReplicationText(nodeEditId, editTextbox.Text, this.editingReplicationIndex);
            }
        }


        private void OnInternalTextboxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && (editTextbox.IsKeyboardFocusWithin == true))
            {
                if (nodeEditFlag == true)
                {
                    bool commitEditing = false;
                    if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                        commitEditing = true;
                    if (false == editTextbox.MultiLineSupport)
                        commitEditing = true;

                    if (false != commitEditing)
                    {
                        TraversalRequest t = new TraversalRequest(FocusNavigationDirection.Previous);
                        editTextbox.MoveFocus(t);
                        return;
                    }
                }
                else if (nodeEditFlag == false) // Repetition
                {
                    if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                    {
                        graphController.DoCreateCodeBlockNode(mouseDownPosition.X, mouseDownPosition.Y, editTextbox.Text);
                        CreateEditTextbox(true, Configurations.RectWhite, "Segoe UI", FontWeights.Normal, Configurations.TextSize, 0);
                    }
                }
            }
            else if (e.Key == Key.Escape)
            {
                if (nodeEditFlag == true && replicationEditFlag == false)
                    graphController.DoEndNodeEdit(nodeEditId, "", false);
                else if (replicationEditFlag == true)
                    replicationEditFlag = false;

                nodeEditId = uint.MaxValue;
                nodeEditFlag = false;
                RemoveTextbox(editTextbox);
            }
        }

        #endregion

        #region Private Class Helper Methods

        private void CreateNode(LibraryItem item, Point position)
        {
            if (null == item)
                return;

            switch (item.ItemType)
            {
                case NodeType.Identifier:
                    graphController.DoCreateIdentifierNode(position.X, position.Y);
                    break;

                case NodeType.Render:
                    graphController.DoCreateRenderNode(position.X, position.Y);
                    break;

                case NodeType.Driver:
                    graphController.DoCreateDriverNode(position.X, position.Y);
                    break;

                case NodeType.CodeBlock:
                    // @TODO(Ben/Joy): Create a "code box" for script input?
                    graphController.DoCreateCodeBlockNode(position.X, position.Y, Configurations.CodeBlockInitialMessage);

                    uint compId;
                    Rect rect = new Rect();
                    NodeType type = NodeType.None;
                    string temp;

                    graphController.GetLastNodeId(out compId);
                    graphController.GetNodePartRegion(compId, NodePart.Text, mouseDownPosition, out rect, out type);
                    CreateEditTextbox(true, Configurations.RectWhite, Configurations.Font, FontWeights.Normal, Configurations.TextSize, 0);
                    graphController.GetNodeText(compId, NodePart.Text, out temp);

                    editTextbox.Text = temp;
                    double x = rect.X + Configurations.TextHorizontalOffset;
                    double y = rect.Y + Configurations.TextVerticalOffset;
                    editTextbox.SetValue(Canvas.LeftProperty, x);
                    editTextbox.SetValue(Canvas.TopProperty, y);
                    nodeEditFlag = true;
                    nodeEditId = compId;

                    graphController.DoBeginNodeEdit(nodeEditId, NodePart.Text);
                    graphController.UpdateNodeText(compId, editTextbox.Text);
                    break;

                case NodeType.Function:
                    graphController.DoCreateFunctionNode(position.X, position.Y,
                        item.Assembly, item.QualifiedName, item.ArgumentTypes);
                    break;

                case NodeType.Property:
                    graphController.DoCreatePropertyNode(position.X, position.Y, item.Assembly, item.QualifiedName, item.ArgumentTypes);
                    break;

                default:
                    throw new ArgumentException("Unhandled type!", "item.ItemType");
            }
        }

        private DrawingVisual FindChildren(System.Windows.Point mouse)
        {
            this.hitVisual = null;
            VisualTreeHelper.HitTest(this, null, new HitTestResultCallback(VisualCallBack), new PointHitTestParameters(mouse));
            return this.hitVisual;
        }

        private void CreateEditTextbox(bool allowNewLine, SolidColorBrush backgroundColor, String font, FontWeight weight, double fontSize, double minTextboxWidth)
        {
            editTextbox = new CustomTextBox(graphCanvas, allowNewLine, backgroundColor, font, weight, fontSize, Configurations.TextboxMaxWidth, minTextboxWidth);
            graphCanvas.AddTextbox(editTextbox);
            editTextbox.Visibility = Visibility.Visible;
            editTextbox.PreviewKeyDown += OnInternalTextboxPreviewKeyDown;
            editTextbox.InternalTextBox.TextChanged += new TextChangedEventHandler(OnInternalTextBoxTextChanged);
            editTextbox.LostKeyboardFocus += new KeyboardFocusChangedEventHandler(OnInternalTextBoxLostKeyboardFocus);
        }

        private void RemoveTextbox(CustomTextBox textbox)
        {
            if (textbox != null)
            {
                graphCanvas.RemoveTextBox(textbox);
                textbox.Visibility = Visibility.Collapsed;

                if (this.editTextbox == textbox)
                    this.editTextbox = null;
            }
        }

        private string SavePathDialog()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            string tabName = graphControl.GetTabControl().FindGraphTabItem(Controller.Identifier).Header;
            if (tabName.ToLower().EndsWith(".bin"))
                tabName = tabName.Substring(0, tabName.Length - 4);
            if (tabName.EndsWith("*"))
                tabName = tabName.Substring(0, tabName.Length - 1);
            tabName = tabName.Trim();

            saveFileDialog.FileName = tabName;
            saveFileDialog.DefaultExt = ".bin";
            saveFileDialog.Filter = "DesignScript Graph Files (.bin)|*.bin";

            bool? result = saveFileDialog.ShowDialog();
            if (result.HasValue && (result.Value == true))
                return saveFileDialog.FileName;
            else
                return null;
        }

        #region graph Manupulation

        private void UpdateDragSelectionRect(Point startPoint, Point endPoint)
        {
            Point topLeftOnCanvas = new Point(Math.Min(startPoint.X, endPoint.X),
                                                Math.Min(startPoint.Y, endPoint.Y));
            Point bottomRightOnCanvas = new Point(Math.Max(startPoint.X, endPoint.X),
                                                Math.Max(startPoint.Y, endPoint.Y));

            Point topLeft = graphCanvas.TranslatePoint(topLeftOnCanvas, graphControl.canvasScrollViewer);
            Point bottomRight = graphCanvas.TranslatePoint(bottomRightOnCanvas, graphControl.canvasScrollViewer);

            graphControl.selectionBorder.Margin = new Thickness(topLeft.X, topLeft.Y, 0, 0);
            graphControl.selectionBorder.Width = bottomRight.X - topLeft.X;
            graphControl.selectionBorder.Height = bottomRight.Y - topLeft.Y;
        }

        private List<uint> GetComponentInRegion(Rect rect)
        {
            List<uint> componentInRegion = new List<uint>();

            foreach (var entry in nodeVisuals)
            {
                Rect compRect;
                graphController.GetNodeRegion(entry.Value, false, out compRect);

                if (rect.Contains(compRect))
                {
                    componentInRegion.Add(entry.Value);
                }
            }

            foreach (var entry in edgeVisuals)
            {
                if (!graphController.CheckEdgeType(entry.Value))
                    continue;

                if ((rect.Width <= Configurations.GlowPen.Thickness / 2)
                    || (rect.Height <= Configurations.GlowPen.Thickness / 2)) // minimial selection region for edge
                    continue;

                Rect compRect = entry.Key.ContentBounds;
                if (rect.Contains(compRect))
                {
                    componentInRegion.Add(entry.Value);
                }
                else if (rect.IntersectsWith(compRect))
                {
                    if (graphController.DeterminEdgeSelection(entry.Value, rect.TopLeft, rect.BottomRight))
                        componentInRegion.Add(entry.Value);
                }
            }

            return componentInRegion;
        }

        private void TogglePan()
        {
            if (panning)
                EndPan();
            else
                BeginPan();
        }

        private void BeginPan()
        {
            ScrollViewer scrollViewer = graphControl.canvasScrollViewer;
            panning = true;
            graphControl.SetCursor(CursorSet.Index.HandPan);

            if (editTextbox != null)
            {
                editTextbox.IsHitTestVisible = false;
            }
        }

        private void EndPan()
        {
            ScrollViewer scrollViewer = graphControl.canvasScrollViewer;
            panning = false;
            graphControl.SetCursor(CursorSet.Index.Pointer);

            mouseDownScreenPosition = new Point();

            if (editTextbox != null)
            {
                editTextbox.InternalTextBox.Focus();
                editTextbox.IsHitTestVisible = true;
            }
        }

        private void AutoPan(Point mousePositionOnWindow)
        {
            ScrollViewer scrollViewer = graphControl.canvasScrollViewer;
            if (mousePositionOnWindow.X < 0)
            {
                double offSetStep = -Configurations.ScrollSpeedFactor * mousePositionOnWindow.X;
                if (offSetStep > Configurations.MaxScrollSpeed)
                    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - Configurations.MaxScrollSpeed);
                else
                    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - offSetStep);
            }
            if (mousePositionOnWindow.X > scrollViewer.ActualWidth)
            {
                double offSetStep = Configurations.ScrollSpeedFactor * (mousePositionOnWindow.X - scrollViewer.ActualWidth);
                if (offSetStep > Configurations.MaxScrollSpeed)
                    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + Configurations.MaxScrollSpeed);
                else
                    scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + offSetStep);
            }
            if (mousePositionOnWindow.Y < 0)
            {
                double offSetStep = -Configurations.ScrollSpeedFactor * mousePositionOnWindow.Y;
                if (offSetStep > Configurations.MaxScrollSpeed)
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - Configurations.MaxScrollSpeed);
                else
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - offSetStep);
            }
            if (mousePositionOnWindow.Y > scrollViewer.ActualHeight)
            {
                double offSetStep = Configurations.ScrollSpeedFactor * (mousePositionOnWindow.Y - scrollViewer.ActualHeight);
                if (offSetStep > Configurations.MaxScrollSpeed)
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + Configurations.MaxScrollSpeed);
                else
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + offSetStep);
            }
        }

        private void ZoomIn()
        {
            Grid grid = graphControl.canvasGrid;
            Slider slider = graphControl.slider;

            if (sliderStep + 1 <= Configurations.MaxSliderStep)
                slider.Value = Configurations.SliderStep[++sliderStep];
            else
            {
                sliderStep = Configurations.MaxSliderStep;
                slider.Value = Configurations.SliderStep[sliderStep];
            }
            sliderScrollHandled = false;
        }

        private void ZoomOut()
        {
            Grid grid = graphControl.canvasGrid;
            Slider slider = graphControl.slider;

            if (sliderStep - 1 >= Configurations.MinSliderStep)
                slider.Value = Configurations.SliderStep[--sliderStep];
            else
            {
                sliderStep = Configurations.MinSliderStep;
                slider.Value = Configurations.SliderStep[sliderStep];
            }
            sliderScrollHandled = false;
        }

        private void ZoomToFit()
        {
            if (panning)
                EndPan();

            ScrollViewer scrollViewer = graphControl.canvasScrollViewer;
            Grid grid = graphControl.canvasGrid;
            ScaleTransform scaleTransform = graphControl.scaleTransform;

            Rect frame = this.GetSelectedRegion();

            //scale
            double zoomInRatioX = scrollViewer.ActualWidth / frame.Width;
            double zoomInRatioY = scrollViewer.ActualHeight / frame.Height;
            if ((zoomInRatioX == 0) || (zoomInRatioY == 0))
                return;

            double newScale = Math.Min(zoomInRatioX, zoomInRatioY);
            newScale = ConvertScale(newScale);

            //Scroll Viewer Offset
            zoomPanPivotPoint = new Point(frame.Left + frame.Width / 2, frame.Top + frame.Height / 2);
            Point centerPosition = new Point(scrollViewer.ViewportWidth / 2, scrollViewer.ViewportHeight / 2);

            double offsetX = zoomPanPivotPoint.X * newScale - centerPosition.X;
            double offsetY = zoomPanPivotPoint.Y * newScale - centerPosition.Y;

            //AnimationHelper(offsetX, newScale, Configurations.ShortAnimationTime);

            ZoomAndPanAnimationHelper animationHelper = new ZoomAndPanAnimationHelper(offsetX, offsetY, newScale, this, this.graphControl, false);
        }

        private double ConvertScale(double newScale)
        {
            int sliderStep = Configurations.MaxSliderStep;

            if (newScale < Configurations.SliderStep[Configurations.MinSliderStep + 1])
                sliderStep = Configurations.MinSliderStep;
            else
            {
                int newSliderStep = 1;
                while (newSliderStep < Configurations.MaxSliderStep)
                {
                    if (newScale > Configurations.SliderStep[newSliderStep - 1] &&
                        newScale < Configurations.SliderStep[newSliderStep])
                        sliderStep = newSliderStep - 1;
                    newSliderStep++;
                }
            }

            this.sliderStep = sliderStep;
            return Configurations.SliderStep[sliderStep];
        }

        private Rect GetSelectedRegion()
        {
            Rect region = new Rect(0, 0, 0, 0);

            foreach (var entry in nodeVisuals)
            {
                if (graphController.IsComponentSelected(entry.Value))
                    region = AddToRegion(entry, region);
            }

            foreach (var entry in edgeVisuals)
            {
                if (graphController.IsComponentSelected(entry.Value))
                    region = AddToRegion(entry, region);
            }

            foreach (var entry in bubbleVisuals)
            {
                IInfoBubble bubble;
                graphController.GetInfoBubble(entry.Value, out bubble);

                if (graphController.IsComponentSelected(bubble.OwnerId))
                    region = AddToRegion(entry, region);
            }

            if (region.Width == 0)
            {
                foreach (var entry in nodeVisuals)
                    region = AddToRegion(entry, region);

                foreach (var entry in edgeVisuals)
                {
                    if (entry.Value != 0)
                        region = AddToRegion(entry, region);
                }

                foreach (var entry in bubbleVisuals)
                    region = AddToRegion(entry, region);
            }

            if (region.Width != 0)
                region.Inflate(graphControl.canvasScrollViewer.ActualWidth / 10,
                               graphControl.canvasScrollViewer.ActualHeight / 10);

            return region;
        }

        private Rect AddToRegion(KeyValuePair<DrawingVisual, uint> entry, Rect region)
        {
            Rect compRect = entry.Key.ContentBounds;

            if (compRect == Rect.Empty)
                return region;

            TranslateTransform transform = entry.Key.Transform as TranslateTransform;
            if (transform != null)
            {
                double x = transform.X;
                double y = transform.Y;
                compRect.Offset(new Vector(x, y));
            }

            if (region.Width == 0)
                return compRect;
            else
            {
                region.Union(compRect);
                return region;
            }
        }

        private Rect GetNodeRegion()
        {
            Rect workingRegion = Rect.Empty;
            foreach (var entry in nodeVisuals)
            {
                Rect compRect;
                graphController.GetNodeRegion(entry.Value, false, out compRect);
                if (workingRegion == Rect.Empty)
                    workingRegion = compRect;
                else
                    workingRegion.Union(compRect);
            }

            return workingRegion;
        }

        private void GetScrollOffset(Rect region, out double offsetX, out double offsetY, Key key)
        {
            ScrollViewer scrollViewer = graphControl.canvasScrollViewer;

            Point topLeftOnCanvas = region.TopLeft;
            Point bottomRightOnCanvas = region.BottomRight;

            Point topLeft = graphControl.CurrentGraphCanvas
                                .TranslatePoint(topLeftOnCanvas, scrollViewer);
            Point bottomRight = graphControl.CurrentGraphCanvas
                                .TranslatePoint(bottomRightOnCanvas, scrollViewer);

            offsetX = scrollViewer.HorizontalOffset;
            offsetY = scrollViewer.VerticalOffset;

            if (topLeft.X < 0 || key == Key.Home)
            {
                offsetX += topLeft.X - Configurations.SafeMargin;
            }
            else if (bottomRight.X > scrollViewer.ActualWidth || key == Key.End)
            {
                offsetX += (bottomRight.X - scrollViewer.ActualWidth) + Configurations.SafeMargin;
            }

            if (topLeft.Y < 0 || key == Key.Prior || key == Key.PageUp)
            {
                offsetY += topLeft.Y - Configurations.SafeMargin;
            }

            else if (bottomRight.Y > scrollViewer.ActualHeight || key == Key.Next || key == Key.PageDown)
            {
                offsetY += (bottomRight.Y - scrollViewer.ActualHeight) + Configurations.SafeMargin;
            }
        }

        private void GetScaleScrollOffset(Rect region, out double scale, out double newOffsetX, out double newOffsetY)
        {
            ScrollViewer scrollViewer = graphControl.canvasScrollViewer;

            Point topLeftOnCanvas = region.TopLeft;
            Point bottomRightOnCanvas = region.BottomRight;

            Point topLeft = graphControl.CurrentGraphCanvas
                                .TranslatePoint(topLeftOnCanvas, scrollViewer);
            Point bottomRight = graphControl.CurrentGraphCanvas
                                .TranslatePoint(bottomRightOnCanvas, scrollViewer);

            double offsetX = scrollViewer.HorizontalOffset;
            double offsetY = scrollViewer.VerticalOffset;
            newOffsetX = offsetX;
            newOffsetY = offsetY;
            scale = graphControl.slider.Value;

            if (topLeft.X < 0)
            {
                newOffsetX += topLeft.X;
            }
            else if (bottomRight.X > scrollViewer.ActualWidth)
            {
                newOffsetX += (bottomRight.X - scrollViewer.ActualWidth);
                if (topLeft.X - (newOffsetX - offsetX) < 0) // check if the above offset makes top-let conor of the visual out of the screen
                    newOffsetX = offsetX + topLeft.X;
            }

            if (topLeft.Y < 0)
            {
                newOffsetY += topLeft.Y;
            }
            else if (bottomRight.Y > scrollViewer.ActualHeight)
            {
                newOffsetY += (bottomRight.Y - scrollViewer.ActualHeight);
                if (topLeft.Y - (newOffsetY - offsetY) < 0) // check if the above offset makes top-let conor of the visual out of the screen
                    newOffsetY = offsetY + topLeft.Y;
            }

            // after the above step, the top-left cornor of the visual is gareenteed to be shown on the screen
            // proceed to check if scaling(zoom out) needed
            if (bottomRight.X - (newOffsetX - offsetX) > scrollViewer.ActualWidth
                || bottomRight.Y - (newOffsetY - offsetY) > scrollViewer.ActualHeight)
            {
                ScaleTransform scaleTransform = graphControl.scaleTransform;

                //scale
                double zoomInRatioX = (region.Width * scaleTransform.ScaleX) / scrollViewer.ActualWidth;
                double zoomInRatioY = (region.Height * scaleTransform.ScaleY) / scrollViewer.ActualHeight;
                if ((zoomInRatioX == 0) || (zoomInRatioY == 0))
                    return;

                scale = ConvertScale(scaleTransform.ScaleX / Math.Max(zoomInRatioX, zoomInRatioY) * 2 / 3);

                //Scroll Viewer Offset
                Point zoomPanPivotPoint = new Point(region.Left + region.Width / 2, region.Top + region.Height / 2);
                Point centerPosition = new Point(scrollViewer.ViewportWidth / 2, scrollViewer.ViewportHeight / 2);

                newOffsetX = zoomPanPivotPoint.X * scale - centerPosition.X;
                newOffsetY = zoomPanPivotPoint.Y * scale - centerPosition.Y;
            }
        }

        #endregion

        #region Radial Menu

        internal void CreateRadialMenuForPreview(Point pt, uint bubbleId)
        {
            if (radialMenu != null)
                return;

            ModifierKeys keys = Keyboard.Modifiers;
            if (keys.HasFlag(ModifierKeys.Control))
                return;

            lastHoveredBubbleId = bubbleId;

            double startAngle = 90;
            Point worldCoords = pt;
            NodePart part = NodePart.NorthEast;
            worldCoords.Offset(-Configurations.CenterOffsetX, Configurations.CenterOffsetY);

            Dictionary<int, string> menuItems = new Dictionary<int, string>();
            Dictionary<int, string> overloadedItems = new Dictionary<int, string>();

            IInfoBubble infoBubble = null;
            this.graphController.GetInfoBubble(bubbleId, out infoBubble);
            if (infoBubble != null)
            {
                States nodeState;
                graphController.GetNodeState(infoBubble.OwnerId, out nodeState);
                bool textualPreviewFlag = nodeState.HasFlag(States.TextualPreview);
                bool geometryPreviewFlag = nodeState.HasFlag(States.GeometryPreview);
                if (textualPreviewFlag == false && geometryPreviewFlag == false)
                {
                    textualPreviewFlag = true;
                    geometryPreviewFlag = true;
                }

                if (textualPreviewFlag)
                    menuItems.Add(Configurations.GeometricOutput, "Geometric Preview");
                if (geometryPreviewFlag)
                    menuItems.Add(Configurations.TextualOutput, "Textual Preview");
                if (nodeState.HasFlag(States.PreviewHidden))
                    menuItems.Add(Configurations.HidePreview, "Show Preview");
                else
                    menuItems.Add(Configurations.HidePreview, "Hide Preview");
            }

            if (this.lastHoveredNodePart != part || this.lastHoveredNodeId != int.MaxValue)
            {
                this.graphController.DoCreateRadialMenu(part, bubbleId);
                lastHoveredNodePart = part;
                lastHoveredNodeId = uint.MaxValue;
                selectedMenuId = int.MinValue;
            }

            if (menuItems != null && menuItems.Count > 0)
            {
                radialMenu = new RadialMenu(bubbleId, worldCoords, Configurations.CircleRadius, menuItems, startAngle, part);
                this.children.Add(radialMenu.GetRadialVisual());
                ScrollToMenuVisual(radialMenu.GetRadialVisual(), radialMenu.GetMenuWidth());

                if (Application.Current != null && (Application.Current.MainWindow != null))
                {
                    if (Application.Current.MainWindow.IsFocused == false)
                    {
                        Application.Current.MainWindow.Activate();
                        Application.Current.MainWindow.Topmost = true;
                        Application.Current.MainWindow.Topmost = false;
                        Application.Current.MainWindow.Focus();
                    }
                }
                Keyboard.Focus(this.graphCanvas.graphCanvas);
            }
        }

        private void CreateRadialMenuForSelectionBox()
        {
            double startAngle = 90;
            Point worldCoords = this.graphController.GetNorthEastPositionOfSelectionBox();
            NodePart part = NodePart.NorthEast;
            worldCoords.Offset(-Configurations.CenterOffsetX, Configurations.CenterOffsetY);

            Dictionary<int, string> menuItems = new Dictionary<int, string>();
            Dictionary<int, string> overloadedItems = new Dictionary<int, string>();

            if (this.lastHoveredNodePart != part || this.lastHoveredNodeId != int.MaxValue)
            {
                this.graphController.DoCreateRadialMenu(part, uint.MaxValue);
                lastHoveredNodePart = part;
                lastHoveredNodeId = uint.MaxValue;
                selectedMenuId = int.MinValue;
            }

            this.graphController.GetMenuItems(out menuItems, out overloadedItems);
            if (menuItems != null && menuItems.Count > 0)
            {
                radialMenu = new RadialMenu(uint.MaxValue, worldCoords, Configurations.CircleRadius, menuItems, startAngle, part);
                this.children.Add(radialMenu.GetRadialVisual());
                ScrollToMenuVisual(radialMenu.GetRadialVisual(), radialMenu.GetMenuWidth());

                if (Application.Current != null && (Application.Current.MainWindow != null))
                {
                    if (Application.Current.MainWindow.IsFocused == false)
                    {
                        Application.Current.MainWindow.Activate();
                        Application.Current.MainWindow.Topmost = true;
                        Application.Current.MainWindow.Topmost = false;
                        Application.Current.MainWindow.Focus();
                    }
                }
                Keyboard.Focus(this.graphCanvas.graphCanvas);
            }
        }

        private void CreateRadialMenu(uint nodeId, NodePart part, Point subRadialMenuOffset)
        {
            ModifierKeys keys = Keyboard.Modifiers;
            if (keys.HasFlag(ModifierKeys.Control))
                return;
            Rect result;
            NodeType type;
            double startAngle;
            Point worldCoords;

            if (this.Controller.GetNodePartRegion(nodeId, NodePart.None, mouseDownPosition, out result, out type) == false)
                return;
            worldCoords = result.Location;

            switch (part)
            {
                case NodePart.NorthEast:
                    startAngle = 90;
                    worldCoords.X += result.Width;
                    worldCoords.Offset(-Configurations.CenterOffsetX, Configurations.CenterOffsetY);
                    break;
                case NodePart.North:
                    startAngle = 180;
                    this.Controller.GetNodePartRegion(nodeId, NodePart.North, mouseDownPosition, out result, out type);
                    worldCoords.X += result.Width;
                    worldCoords.Offset(Configurations.CenterOffsetX, Configurations.CenterOffsetY);
                    break;
                case NodePart.NorthWest:
                    startAngle = 180;
                    worldCoords.Offset(Configurations.CenterOffsetX, Configurations.CenterOffsetY);
                    break;
                case NodePart.South:
                    startAngle = 270;
                    this.Controller.GetNodePartRegion(nodeId, NodePart.South, mouseDownPosition, out result, out type);
                    worldCoords.X += result.Width;
                    worldCoords.Y += result.Height;
                    worldCoords.Offset(Configurations.CenterOffsetX, -Configurations.CenterOffsetY);
                    break;
                default:
                    startAngle = 90;
                    worldCoords.X += result.Width;
                    worldCoords.Offset(-Configurations.CenterOffsetX, Configurations.CenterOffsetY);
                    break;
            }

            Dictionary<int, string> menuItems = null;
            Dictionary<int, string> overloadedItems = null;
            if (this.lastHoveredNodePart != part || this.lastHoveredNodeId != nodeId)
            {
                this.graphController.DoCreateRadialMenu(part, nodeId);
                lastHoveredNodePart = part;
                lastHoveredNodeId = nodeId;
                selectedMenuId = int.MinValue;
            }
            this.graphController.GetMenuItems(out menuItems, out overloadedItems);
            if (null != menuItems && (menuItems.Count > 0))
            {
                radialMenu = new RadialMenu(nodeId, worldCoords, Configurations.CircleRadius, menuItems, startAngle, part);
                this.children.Add(radialMenu.GetRadialVisual());
                ScrollToMenuVisual(radialMenu.GetRadialVisual(), radialMenu.GetMenuWidth());

                if (Application.Current != null && (Application.Current.MainWindow != null))
                {
                    if (Application.Current.MainWindow.IsFocused == false)
                    {
                        Application.Current.MainWindow.Activate();
                        Application.Current.MainWindow.Topmost = true;
                        Application.Current.MainWindow.Topmost = false;
                        Application.Current.MainWindow.Focus();
                    }
                }
                Keyboard.Focus(this.graphCanvas.graphCanvas);
            }
        }

        private void CreateSubRadialMenu(Point subRadialMenuOffset)
        {
            if (radialMenu != null)
            {
                Point worldCoords = radialMenu.GetWorldCoords();
                double startAngle = radialMenu.GetStartAngle();
                startAngle = (startAngle / Math.PI) * 180;
                worldCoords.Offset(subRadialMenuOffset.X, subRadialMenuOffset.Y + Configurations.CircleRadius / 2);
                Dictionary<int, string> menuItems = new Dictionary<int, string>();
                Dictionary<int, string> overloadedItems = new Dictionary<int, string>();
                graphController.DoCreateSubRadialMenu(selectedMenuId);
                this.graphController.GetMenuItems(out menuItems, out overloadedItems);
                if (overloadedItems.Count > 0)
                {
                    subRadialMenu = new RadialMenu(lastHoveredNodeId, worldCoords, Configurations.CircleRadius, overloadedItems, startAngle, lastHoveredNodePart);
                    this.children.Add(subRadialMenu.GetRadialVisual());
                    radialMenu.Fade();
                }
            }
            return;
        }

        private void RemoveSubRadialMenu()
        {
            if (subRadialMenu != null)
            {
                children.Remove(subRadialMenu.GetRadialVisual());
                subRadialMenu = null;
                graphController.DoCreateRadialMenu(lastHoveredNodePart, lastHoveredNodeId);
            }
        }

        private void StartTimer()
        {
            if (radialMenuTimer == null)
            {
                radialMenuTimer = new Timer(Configurations.RadialMenuDelay);
                radialMenuTimer.Elapsed += new ElapsedEventHandler(OnRadialMenuTimerElapsed);
                radialMenuTimer.Disposed += new EventHandler(OnRadialMenuTimerDisposed);
                radialMenuTimer.Enabled = true;
                radialMenuTimer.Start();
                radialMenuTimer.AutoReset = true;
            }
        }

        private void OnRadialMenuTimerDisposed(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                if (this.graphController.CurrentDragState == DragState.None)
                    CreateRadialMenu(radialNodeId, radialNodePart, new Point());
            }));
        }

        private void OnRadialMenuTimerElapsed(object sender, ElapsedEventArgs e)
        {
            (sender as Timer).Stop();
            radialMenuTimer.Dispose();
        }

        private void CreateSubRadialMenuTimer()
        {
            if (subRadialMenuTimer == null)
            {
                subRadialMenuTimer = new Timer();
                subRadialMenuTimer.Interval = Configurations.RadialMenuDelay;
                subRadialMenuTimer.Elapsed += new ElapsedEventHandler(OnSubRadialMenuTimerElapsed);
                subRadialMenuTimer.Start();
            }
        }

        void OnSubRadialMenuTimerElapsed(object sender, ElapsedEventArgs e)
        {
            (sender as Timer).Stop();

            this.Dispatcher.Invoke((Action)(() =>
            {
                RemoveSubRadialMenu();
                CreateSubRadialMenu(subRadialMenuPoint);
            }));

            (sender as Timer).Dispose();
            subRadialMenuTimer = null;
        }

        private void ResetSubRadialMenuTimer()
        {
            if (subRadialMenuTimer != null)
            {
                subRadialMenuTimer.Stop();
                subRadialMenuTimer.Dispose();
                subRadialMenuTimer = null;
            }
        }


        #endregion

        #region preview bubble

        private int GetVisualLayerForBubble()
        {
            // bubble should be above the arc while below the node
            // from children[0] to  children.last(), the visuals are arranged
            // in order of : arc -> bubble -> node,
            // we need to find the first visual index that is not arc (is bubble or node)
            // and insert the bubble on that index.
            for (int i = 0; i <= children.Count; i++)
            {
                DrawingVisual dv = children[i] as DrawingVisual;
                if (bubbleVisuals.ContainsKey(dv) || nodeVisuals.ContainsKey(dv))
                    return i;
            }

            return -1;
        }

        private bool FindBubble(DrawingVisual drawingVisual, out uint bubbleId)
        {
            if (bubbleVisuals.TryGetValue(drawingVisual, out bubbleId))
            {
                IInfoBubble bubble;
                graphController.GetInfoBubble(bubbleId, out bubble);

                if (!bubble.IsPreviewBubble)
                {
                    bubbleId = uint.MaxValue;
                    return false;
                }

                foreach (ExtendedPreview previewTextBox in extendedBubble)
                {
                    if (((uint)previewTextbox.Tag) == bubbleId)
                    {
                        bubbleId = uint.MaxValue;
                        return false;
                    }
                }
                return true;
            }
            else
            {
                bubbleId = uint.MaxValue;
                return false;
            }
        }

        private bool FindExtendedBubbleOn(Point mousePosition)
        {
            if (extendedBubble.Count() == 0)
                return false;

            foreach (ExtendedPreview previewTextBox in extendedBubble)
                if (IsMouseInExtendedBubble(mousePosition, previewTextbox))
                    return true;

            return false;
        }

        // (TODO: Victor after the animation of the expand/condense is done, remove the "30px margin" 
        //                                                                              -- by Victor)
        private bool IsMouseInExtendedBubble(Point mousePosition, ExtendedPreview previewTextBox)
        {
            Point positionOnTextBox = new Point(mousePosition.X - (double)previewTextbox.GetValue(Canvas.LeftProperty),
                                                mousePosition.Y - (double)previewTextbox.GetValue(Canvas.TopProperty) - Configurations.InfoBubbleArrowHeight);

            if (positionOnTextBox.X > -30 && positionOnTextBox.X < previewTextbox.PreviewPanel.ActualWidth + 30   // within the preview region
                && positionOnTextBox.Y > -1 && positionOnTextBox.Y < previewTextbox.PreviewPanel.ActualHeight + 30)
                return true;
            else
                return false;
        }

        private bool WithinBubbleToggleVisibilityRegion(Point mousePosition, DrawingVisual drawingVisual)
        {
            Rect visualRect = drawingVisual.ContentBounds;
            TranslateTransform offset = drawingVisual.Transform as TranslateTransform;

            visualRect.Offset(new Vector(offset.X, offset.Y));

            double middleX = visualRect.X + visualRect.Width / 2;

            if (mousePosition.X > middleX - Configurations.InfoBubbleArrowHitTestWidth / 2 - 1
                && mousePosition.X < middleX + Configurations.InfoBubbleArrowHitTestWidth / 2 + 1
                && mousePosition.Y > visualRect.Y
                && mousePosition.Y < visualRect.Y + Configurations.InfoBubbleArrowHitTestHeight)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void OnPreviewTextboxMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 3)
                ((ExtendedPreview)sender).InternalTextBox.SelectAll();
        }

        #endregion

        #endregion
    }
}
