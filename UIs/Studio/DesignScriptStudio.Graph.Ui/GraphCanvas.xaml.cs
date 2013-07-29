using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DesignScriptStudio.Graph.Core;

namespace DesignScriptStudio.Graph.Ui
{
    /// <summary>
    /// Interaction logic for GraphCanvas.xaml
    /// </summary>
    public partial class GraphCanvas : UserControl
    {
        private string startupFile = String.Empty;
        private GraphControl graphControl = null;
        private GraphVisualHost visualHost = null;
        internal CustomTextBox textbox = null;

        //zoom and pan info
        private double sliderValue;
        private double scrollHorizontalOffset;
        private double scrollVerticalOffset;

        bool ignoreMouseMoveEvent = false;

        #region Public Class Operational Methods

        public GraphCanvas()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(OnGraphCanvasLoaded);
        }

        public GraphCanvas(GraphControl graphControl)
        {
            InitializeComponent();
            this.graphControl = graphControl;
            this.Loaded += new RoutedEventHandler(OnGraphCanvasLoaded);
        }

        public GraphCanvas(GraphControl graphControl, string startupFile)
        {
            InitializeComponent();
            this.startupFile = startupFile;
            this.graphControl = graphControl;
            this.Loaded += new RoutedEventHandler(OnGraphCanvasLoaded);
        }

        #endregion

        #region Public Class Properties

        internal string FilePath
        {
            get
            {
                IGraphController graphController = visualHost.Controller;
                return graphController.FilePath;
            }
        }

        internal GraphVisualHost VisualHost { get { return visualHost; } }
        internal IGraphController Controller { get { return visualHost.Controller; } }

        #endregion

        #region Private Event Handlers

        private void OnGraphCanvasLoaded(object sender, RoutedEventArgs e)
        {
            visualHost = new GraphVisualHost(this, this.graphControl, this.startupFile);
            visualHost.IsInRecordingMode = graphControl.IsInRecordingMode;
            graphCanvas.Children.Add(visualHost);
            graphControl.GraphControllerLoaded(this.Controller.Identifier);

            graphCanvas.KeyDown += new KeyEventHandler(OnCanvasKeyDown);
            graphCanvas.KeyUp += new KeyEventHandler(OnCanvasKeyUp);
            graphCanvas.MouseMove += new MouseEventHandler(OnCanvasMouseMove);
            graphCanvas.MouseDown += OnCanvasMouseButtonDown;
            graphCanvas.PreviewMouseDown += OnCanvasPreviewMouseDown;
            graphCanvas.MouseUp += OnCanvasMouseButtonUp;

            //for libraryview
            graphCanvas.AllowDrop = true;
            graphCanvas.Drop += new DragEventHandler(OnCanvasDrop);

            //for Zoom and Pan
            graphCanvas.MouseWheel += OnMouseWheel;

            //if (!string.IsNullOrEmpty(this.startupFile))
            //    this.visualHost.HandleZoomToFit();

            graphCanvas.Focus();
        }

        void OnCanvasKeyDown(object sender, KeyEventArgs e)
        {
            if (visualHost != null)
                visualHost.HandleKeyDown(sender, e);
        }

        void OnCanvasKeyUp(object sender, KeyEventArgs e)
        {
            if (visualHost != null)
                visualHost.HandleKeyUp(sender, e);
        }

        void OnCanvasMouseMove(object sender, MouseEventArgs e)
        {
            if (Application.Current != null && Application.Current.MainWindow != null)
            {
                if (Application.Current.MainWindow.IsActive == false)
                    return;
            }
            if (ignoreMouseMoveEvent != false)
                return;

            if (visualHost != null)
                visualHost.HandleMouseMove(sender, e);
        }

        private void OnCanvasPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.visualHost.HandlePreviewMouseDown();
            // this.graphCanvas.Focus();
        }

        private void OnCanvasMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.graphCanvas.Focus();
            if (visualHost != null)
            {
                switch (e.ChangedButton)
                {
                    case MouseButton.Left:
                        visualHost.HandleMouseLeftButtonDown(sender, e);
                        ignoreMouseMoveEvent = true;
                        Mouse.Capture((UIElement)sender);
                        ignoreMouseMoveEvent = false;
                        return;
                    case MouseButton.Right:
                        visualHost.HandleMouseRightButtonDown(sender, e);
                        ignoreMouseMoveEvent = true;
                        Mouse.Capture((UIElement)sender);
                        ignoreMouseMoveEvent = false;
                        return;
                    case MouseButton.Middle:
                        visualHost.HandleMouseMiddleButtonDown(sender, e);
                        ignoreMouseMoveEvent = true;
                        Mouse.Capture((UIElement)sender);
                        ignoreMouseMoveEvent = false;
                        return;
                }
            }
        }

        private void OnCanvasMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            ignoreMouseMoveEvent = true;
            ((UIElement)sender).ReleaseMouseCapture();
            ignoreMouseMoveEvent = false;

            if (visualHost != null)
            {
                switch (e.ChangedButton)
                {
                    case MouseButton.Left:
                        visualHost.HandleMouseLeftButtonUp(sender, e);
                        return;
                    case MouseButton.Right:
                        visualHost.HandleMouseRightButtonUp(sender, e);
                        return;
                    case MouseButton.Middle:
                        visualHost.HandleMouseMiddleButtonUp(sender, e);
                        return;
                }
            }
        }

        void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (visualHost != null)
                visualHost.HandleMouseWheel(sender, e);
        }

        internal void OnZoomToFitClick()
        {
            if (visualHost != null)
                visualHost.HandleZoomToFit();
        }

        internal void OnZoomInClick()
        {
            if (visualHost != null)
                visualHost.HandleZoomIn();
        }

        internal void OnZoomOutClick()
        {
            if (visualHost != null)
                visualHost.HandleZoomOut();
        }

        internal void OnPanClick()
        {
            if (visualHost != null)
                visualHost.HandleTogglePan();
        }

        internal void AddTextbox(UIElement textbox)
        {
            graphCanvas.Children.Add(textbox);
            textbox.SetValue(Panel.ZIndexProperty, 6500);
        }

        internal void RemoveTextBox(UIElement textbox)
        {
            graphCanvas.Children.Remove(textbox);
        }

        void OnCanvasDrop(object sender, DragEventArgs e)
        {
            if (visualHost != null)
            {
                LibraryItem draggedItem = (LibraryItem)e.Data.GetData("DesignScriptStudio.Graph.Core.LibraryItem");
                visualHost.HandleDrop(sender, draggedItem, e);
                graphCanvas.Focus();
            }
        }

        #endregion

        #region Internal Helper Methods

        internal void SetZoomAndPanInfo(double sliderValue, double horizontalOffset, double verticalOffset)
        {
            this.sliderValue = sliderValue;
            this.scrollHorizontalOffset = horizontalOffset;
            this.scrollVerticalOffset = verticalOffset;
        }

        internal void GetZoomAndPanInfo(out double sliderValue, out double horizontalOffset, out double verticalOffset)
        {
            sliderValue = this.sliderValue;
            horizontalOffset = this.scrollHorizontalOffset;
            verticalOffset = this.scrollVerticalOffset;
        }

        #endregion
    }
}
