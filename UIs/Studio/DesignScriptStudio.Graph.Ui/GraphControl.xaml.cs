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
using DesignScriptStudio.Graph.Core.Services;
using Autodesk.DesignScript.Interfaces;
using System.Windows.Resources;
using DesignScript.Editor.Common;

namespace DesignScriptStudio.Graph.Ui
{
    class CursorSet
    {
        internal enum Index
        {
            Pointer,
            ArcAdd,
            ArcAddEnd,
            ArcSelect,
            ArcRemove,
            Hand,
            HandPan,
            HandPanActive,
            RectangularSelection,
            ResizeDiagonal,
            ResizeHorizontal,
            ResizeVertical,
            Expand,
            Condense
        }

        internal CursorSet(Panel cursorOwner)
        {
            this.cursorOwner = cursorOwner;

            Dictionary<Index, string> resources = new Dictionary<Index, string>();
            resources.Add(Index.Pointer, "pointer.cur");
            resources.Add(Index.ArcAdd, "arc_add.cur");
            resources.Add(Index.ArcAddEnd, "add_arc_end.cur");
            resources.Add(Index.ArcSelect, "arc_select.cur");
            resources.Add(Index.ArcRemove, "arc_remove.cur");
            resources.Add(Index.Hand, "hand.cur");
            resources.Add(Index.HandPan, "hand_pan.cur");
            resources.Add(Index.HandPanActive, "hand_pan_active.cur");
            resources.Add(Index.RectangularSelection, "rectangular_selection.cur");
            resources.Add(Index.ResizeDiagonal, "resize_diagonal.cur");
            resources.Add(Index.ResizeHorizontal, "resize_horizontal.cur");
            resources.Add(Index.ResizeVertical, "resize_vertical.cur");
            resources.Add(Index.Expand, "expand.cur");
            resources.Add(Index.Condense, "condense.cur");

            foreach (KeyValuePair<Index, string> resource in resources)
            {
                Uri uri = new Uri(ResourceNames.ResourceBaseUri + resource.Value);
                StreamResourceInfo cursorStream = Application.GetResourceStream(uri);
                cursors.Add(new Cursor(cursorStream.Stream));
            }
        }

        internal void SetCursor(Index cursorIndex)
        {
            int index = ((int)cursorIndex);
            this.cursorOwner.Cursor = this.cursors[index];
            this.currentCursor = cursorIndex;
        }

        internal Index GetCursor()
        {
            return currentCursor;
        }

        private Index currentCursor = Index.Pointer;
        private Panel cursorOwner = null;
        private List<Cursor> cursors = new List<Cursor>();
    }

    /// <summary>
    /// Interaction logic for GraphControl.xaml
    /// </summary>
    public partial class GraphControl : UserControl, IGraphUiContainer
    {
        enum InitializationState
        {
            None,
            UiInitialized,
            CoreInitialized
        }

        private bool isInRecordingMode = false;
        private bool geometricPreviewEnabled = true;

        private GraphTabControl tabControl = null;
        private LibraryView libraryView = null;
        private ICoreComponent coreComponent = null;
        private CollectInfoManager collectInfoManager = null;
        private GraphUpdateNotificationControl updateNotifier = null;
        private InitializationState initializationState = InitializationState.None;
        private CursorSet cursorSet = null;

        private ZoomAndPanControl zoomAndPanControl = null;

        private int currentGraphCanvas = -1;
        private List<GraphCanvas> graphCanvases = new List<GraphCanvas>();
        private IGraphEditorHostApplication hostApplication = null;
        private int newCanvasCount;

        private FeedbackMessage errorFeedback = null;
        private Dictionary<uint, string> filesToRecover = null;

        #region Public Interface Methods

        public IGraphController GetController(uint identifier)
        {
            if (uint.MaxValue == identifier) // Get the current controller.
            {
                if (null == graphCanvases || (graphCanvases.Count <= 0))
                    return null;

                GraphCanvas graphCanvas = graphCanvases[currentGraphCanvas];
                if (null == graphCanvas.VisualHost) // When file first loaded.
                    return null;

                return graphCanvases[currentGraphCanvas].Controller;
            }

            foreach (GraphCanvas canvas in graphCanvases)
            {
                IGraphController controller = canvas.Controller;
                if (controller.Identifier == identifier)
                    return controller;
            }

            return null;
        }

        public System.Windows.Threading.Dispatcher CurrentDispatcher
        {
            get { return this.Dispatcher; }
        }

        public IGraphEditorHostApplication HostApplication
        {
            get { return hostApplication; }
        }

        #endregion

        #region Public Class Operational Methods

        public GraphControl()
        {
            InitControl(false);
        }

        public GraphControl(bool isInRecordingMode, IGraphEditorHostApplication hostApp = null)
        {
            InitControl(isInRecordingMode, hostApp);
        }

        public GraphControl(bool isInRecordingMode, Dictionary<string, object> configurations)
        {
            InitControl(isInRecordingMode);
        }

        public GraphControl(IGraphEditorHostApplication hostApp)
        {
            bool recordingMode = false;
            if (null != hostApp && null != hostApp.Configurations)
            {
                object value = hostApp.Configurations[ConfigurationKeys.RecordingUserActions];
                if (null != value)
                    recordingMode = (bool)value;
            }
            InitControl(recordingMode, hostApp);
        }

        public bool Shutdown()
        {
            if (!tabControl.CloseAllTab())
                return false;

            if (null != this.coreComponent)
                this.coreComponent.Shutdown();

            return true;
        }

        public void DisplayException(Exception exception)
        {
            //Push the exception
            Logger.LogError("Exception", exception.ToString());
            Logger.LogError("Exception-StackTrace", exception.StackTrace);

            if (null != CurrentGraphCanvas.VisualHost)
            {
                // When a crash happens during file-load, the controller would have 
                // been 'null' and no UI or VM states would have been recorded. Here 
                // we determine if "VisualHost" is 'null' (in which case the file is
                // still being loaded), if so then we won't attempt to store the 
                // recorded states.
                string uiStates = CurrentGraphCanvas.Controller.GetRecordedUiStates();
                string vmStates = CurrentGraphCanvas.Controller.GetRecordedVmStates();

                Logger.LogError("Exception-uiStates", uiStates);
                Logger.LogError("Exception-vmStates", vmStates);
            }

#if DEBUG
            ExceptionWindow window = new ExceptionWindow(exception);

            if (null != Application.Current)
                window.Owner = Application.Current.MainWindow;

            window.ShowDialog();
#else
            // Show the end-user facing dialog.
            PopupDialog relaunchDialog = new PopupDialog(ResourceNames.Error, UiStrings.Relaunch, string.Empty, string.Empty, "Relaunch");
            if (null != Application.Current)
                relaunchDialog.Owner = Application.Current.MainWindow;

            relaunchDialog.ShowDialog();
#endif
        }

        internal void SetCursor(CursorSet.Index cursorIndex)
        {
            this.cursorSet.SetCursor(cursorIndex);
        }

        internal CursorSet.Index GetCursor()
        {
            return this.cursorSet.GetCursor();
        }

        public void AddFeedbackMessage(string iconPath, string message)
        {
            if (this.FeedbackMsssagePanel.Children.Count > 0
                && this.FeedbackMsssagePanel.Children[0] is Splash)
            {
                ((Splash)this.FeedbackMsssagePanel.Children[0]).FadeSplash();
            }

            FeedbackMessage feedbackMessage = new FeedbackMessage(iconPath, message, this);
            feedbackMessage.SetValue(GraphControl.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            feedbackMessage.SetValue(GraphControl.VerticalAlignmentProperty, VerticalAlignment.Bottom);
            feedbackMessage.SetValue(GraphControl.MarginProperty, new Thickness(0, 0, 0, 5));
            this.FeedbackMsssagePanel.Children.Add(feedbackMessage);

            //if (iconPath != ResourceNames.Confirmation)
            //{
            //    if (errorFeedback != null)
            //        errorFeedback.FadeFeedbackMessage();
            //    errorFeedback = feedbackMessage;
            //}
        }

        internal void RemoveFeedbackMessage(FeedbackMessage feedbackMessage)
        {
            if (errorFeedback == feedbackMessage)
                errorFeedback = null;
            this.FeedbackMsssagePanel.Children.Remove(feedbackMessage);
        }

        internal void GraphControllerLoaded(uint identifier)
        {
            IGraphController graphController = GetController(identifier);
            if (graphController == null)
                return;

            // update the libraryView if there is any script need to be imported
            // the actuall importing is in the GraphControl::LoadFileInternal()
            List<string> importedScripts = graphController.GetImportedScripts();
            if (importedScripts != null && importedScripts.Count > 0)
                this.libraryView.FinishLoadingLibrary();

            graphController.Modified += new CanvasModifiedHandler(OnCanvasModified);
        }

        public void FinishLoadingLibrary()
        {
            if (libraryView != null)
                libraryView.FinishLoadingLibrary();
        }

        internal void SetCanvasFocus()
        {
            if (CurrentGraphCanvas != null)
                CurrentGraphCanvas.graphCanvas.Focus();
        }

        internal void ActivateCanvas(GraphCanvas canvas)
        {
            if (canvas == CurrentGraphCanvas)
                return;

            //canvas
            //this.canvasGrid.Children.Remove(canvas);
            //this.canvasGrid.Children.Add(canvas);
            canvas.graphCanvas.Focus();

            //scale
            GraphVisualHost.sliderScrollHandled = true;// set the slider value and scrollview, 
            //no need HandleScrollViewerScrollChanged
            double sliderValue;
            double horizontalOffset;
            double verticalOffset;
            canvas.GetZoomAndPanInfo(out sliderValue, out horizontalOffset, out verticalOffset);
            slider.Value = sliderValue;
            this.scaleTransform.ScaleX = sliderValue;
            this.scaleTransform.ScaleY = sliderValue;
            canvasScrollViewer.ScrollToHorizontalOffset(horizontalOffset);
            canvasScrollViewer.ScrollToVerticalOffset(verticalOffset);

            //record the canvasId
            currentGraphCanvas = graphCanvases.IndexOf(canvas);
        }

        internal void RemoveCanvas(uint identifier)
        {
            // there is another ResetZoomAndPan() in the AddGraphCanvas()
            // to ensure the scroll viewer and slider to be reset properly.
            //
            ResetZoomAndPan();
            GraphCanvas canvas = GetCanvas(identifier);
            canvasGrid.Children.Remove(canvas);
            graphCanvases.Remove(canvas);

            // need to keep the keyboard focus on the Master Grid
            // to enable the command short cut keys
            //
            Keyboard.Focus(this.MasterGrid);

            if (graphCanvases.Count() == 0)
            {
                this.libraryView.Visibility = Visibility.Collapsed;
                this.zoomAndPanControl.Visibility = Visibility.Collapsed;
                this.FeedbackMsssagePanel.Children.Clear();
                return;
            }

        }

        internal void SnapShotCanvas(uint identifier)
        {
            GraphCanvas canvas = GetCanvas(identifier);
            if (canvas != null)
                canvas.SetZoomAndPanInfo(slider.Value, canvasScrollViewer.HorizontalOffset, canvasScrollViewer.VerticalOffset);

            ResetZoomAndPan();
        }

        internal void UpdateCurrentCanvas(uint identifier)
        {
            GraphCanvas canvas = GetCanvas(identifier);
            if (canvas != null)
                currentGraphCanvas = graphCanvases.IndexOf(canvas);
        }

        internal GraphTabControl GetTabControl()
        {
            return this.tabControl;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            Window w = Window.GetWindow(this);
            if (w != null)
            {
                w.StateChanged += new EventHandler(OnWindowStateChanged);
            }
        }

        #endregion

        #region Public Class Properties

        internal bool IsInRecordingMode { get { return isInRecordingMode; } }

        internal GraphCanvas CurrentGraphCanvas
        {
            get
            {
                if (null == graphCanvases || (graphCanvases.Count <= 0))
                    return null;
                if (currentGraphCanvas < 0 || (currentGraphCanvas >= graphCanvases.Count))
                    return null;

                return graphCanvases[currentGraphCanvas];
            }
        }

        #endregion

        #region Private Class Event Handlers

        private void OnGraphControlLoaded(object sender, RoutedEventArgs e)
        {
            // Kick start library loading on a background thread...
            coreComponent = ClassFactory.CreateCoreComponent(this, geometricPreviewEnabled);
            coreComponent.InitializeAsync(this.OnCoreComponentInitialized);

            // Cursor resources.
            this.cursorSet = new CursorSet(this.MasterGrid);

            // Kick start a check for update version info, if the update manager is idle.
            UpdateManager.Instance.UpdateDownloaded += OnUpdatePackageDownloaded;
            UpdateManager.Instance.CheckForProductUpdate();
            UpdateManager.Instance.ShutdownRequested += OnUpdateManagerShutdownRequested;

            //Library
            libraryView = new LibraryView(this);
            libraryView.SetValue(GraphControl.WidthProperty, 250.0);
            libraryView.SetValue(GraphControl.HeightProperty, 400.0);
            libraryView.SetValue(GraphControl.VerticalAlignmentProperty, VerticalAlignment.Bottom);
            libraryView.SetValue(GraphControl.HorizontalAlignmentProperty, HorizontalAlignment.Left);
            libraryView.SetValue(GraphControl.MarginProperty, new Thickness(5, 0, 0, 0));
            libraryView.SetValue(Panel.ZIndexProperty, 60000);
            MasterGrid.Children.Add(libraryView);

            //Zoom and Pan Bar
            zoomAndPanControl = new ZoomAndPanControl(this);
            zoomAndPanControl.SetValue(GraphControl.VerticalAlignmentProperty, VerticalAlignment.Top);
            zoomAndPanControl.SetValue(GraphControl.HorizontalAlignmentProperty, HorizontalAlignment.Right);
            zoomAndPanControl.SetValue(GraphControl.MarginProperty, new Thickness(0, 5, 5, 0));
            zoomAndPanControl.SetValue(Panel.ZIndexProperty, 60000);
            zoomAndPanControl.Focusable = false;
            MasterGrid.Children.Add(zoomAndPanControl);

            //TabControl
            newCanvasCount = 0;
            tabControl = new GraphTabControl(this);
            tabControl.SetValue(Panel.ZIndexProperty, 60000);
            tabControl.SetValue(GraphControl.MarginProperty, new Thickness(0, -1, 0, 0));
            TabBorder.Child = tabControl;

            // for zoom and pan
            this.canvasScrollViewer.KeyDown += OnScrollViewerKeyDown;
            this.canvasScrollViewer.ScrollChanged += OnScrollViewerScrollChanged;
            this.slider.ValueChanged += OnSliderValueChanged;

            filesToRecover = Utilities.GetFilesToRecover(coreComponent.SessionName);
            if (null == filesToRecover || (filesToRecover.Count == 0))
                CreateBlankOrRecoveredCanvas(false); // Nothing to recover, create default.

            // Either UI or Core can be completely loaded before another. In the event 
            // that UI completes its loading first, it sets the "initializationState"
            // to be "InitializationState.UiInitialized". If at this point Core has 
            // already finished loading, then the library data can readily be obtained 
            // and "Library" tree view populated.
            // 
            if (initializationState == InitializationState.None)
                initializationState = InitializationState.UiInitialized;
            else if (initializationState == InitializationState.CoreInitialized)
                OnLoadingComplete();

            if (!coreComponent.StudioSettings.DontShowSplash)
            {
                Splash splashMessage = new Splash(this);
                splashMessage.SetValue(GraphControl.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                splashMessage.SetValue(GraphControl.VerticalAlignmentProperty, VerticalAlignment.Bottom);
                splashMessage.MouseLeftButtonDown += OnSplashMouseLeftButtonDown;
                this.FeedbackMsssagePanel.Children.Add(splashMessage);
            }
        }

        private void OnCoreComponentInitialized(bool loadSucceeded)
        {
            // Either UI or Core can be completely loaded before another. In the event 
            // that Core completes its loading first, it sets the "initializationState"
            // to be "InitializationState.CoreInitialized". If at this point UI has 
            // already finished loading, then the library data that is loaded in Core 
            // can be used to populate the "Library" tree view.
            // 
            if (initializationState == InitializationState.None)
                initializationState = InitializationState.CoreInitialized;
            else if (initializationState == InitializationState.UiInitialized)
                OnLoadingComplete();
        }

        private void OnLoadingComplete()
        {
            // PopulateLibraryFromCurrentGraph
            coreComponent = ClassFactory.CurrCoreComponent;
            LibraryItem rootItem = coreComponent.GetRootLibraryItem();

            if (rootItem != null)
            {
                rootItem.Children.Insert(0, new LibraryItem(NodeType.None, DesignScriptStudio.Graph.Core.Configurations.NoResultMessage));
                libraryView.BindLibraryItemData(rootItem);
            }

            if (null != filesToRecover && (filesToRecover.Count > 0))
                CreateBlankOrRecoveredCanvas(true); // Go into recovery mode.
        }

        private void OnSplashMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Splash splashMessage = sender as Splash;
            splashMessage.FadeSplash();
            if (splashMessage.dontShow.IsChecked == true)
            {
                coreComponent = ClassFactory.CurrCoreComponent;
                coreComponent.DisableSplash();
            }
        }

        private void OnUpdatePackageDownloaded(object sender, UpdateDownloadedEventArgs e)
        {
            if (null != e && null == e.Error && e.UpdateAvailable)
            {
                if (null == updateNotifier)
                {
                    updateNotifier = new GraphUpdateNotificationControl(new LoggerWrapper());
                    updateNotifier.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                    updateNotifier.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    updateNotifier.ToolTip = "New update available";
                    this.ShortcutBar.Children.Add(updateNotifier);
                    this.ShortcutBar.UpdateLayout();
                }
                updateNotifier.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void OnUpdateManagerShutdownRequested(object sender, EventArgs e)
        {
            if (Application.Current != null)
            {
                Application.Current.Exit += new ExitEventHandler(UpdateManager.Instance.HostApplicationBeginQuit);
                Application.Current.Shutdown();
            }
        }

        private void InitControl(bool isInRecordingMode, IGraphEditorHostApplication hostApp = null)
        {
            if (null != hostApp)
            {
                object value = null;
                string configName = ConfigurationKeys.GeometricPreviewEnabled;
                if (hostApp.Configurations.TryGetValue(configName, out value))
                    geometricPreviewEnabled = ((bool)value);
            }

            //consent UI feedback
            collectInfoManager = new CollectInfoManager(new LoggerWrapper());

            this.isInRecordingMode = isInRecordingMode;
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(OnGraphControlLoaded);
            this.hostApplication = hostApp;

            Logger.LogInfo("VersionNumber", UpdateManager.CreateInstance(new LoggerWrapper()).ProductVersion.ToString());
        }

        private void OnWindowMinimize(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).WindowState = WindowState.Minimized;
        }

        private void OnWindowMaximize(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this).WindowState == WindowState.Maximized)
                Window.GetWindow(this).WindowState = WindowState.Normal;
            else
            {
                Window.GetWindow(this).WindowState = WindowState.Maximized;
            }
        }

        private void OnWindowStateChanged(object sender, EventArgs e)
        {
            Window w = Window.GetWindow(this);
            if (w != null)
            {
                if (w.WindowState == WindowState.Maximized)
                    this.Max.Tag = "Restore";
                else
                    this.Max.Tag = "Max";
            }
        }

        private void OnWindowClose(object sender, RoutedEventArgs e)
        {
            if (tabControl.CloseAllTab())
                Window.GetWindow(this).Close();
        }

        private void OnMenuItemSubmenuOpened(object sender, RoutedEventArgs e)
        {
            if (collectInfoManager == null)
                throw new InvalidOperationException("CollectInfoManager is not initialized (0AEF12FF068A)");

            this.EnableUsabilityDataReportingItem.IsChecked = collectInfoManager.GetCollectInfoOption();
            this.TogglePreviewsItem.IsChecked = !coreComponent.StudioSettings.SuppressPreview;
        }

        private void OnSettingsTogglePreview(object sender, RoutedEventArgs e)
        {
            bool suppress = this.coreComponent.StudioSettings.SuppressPreview;
            this.coreComponent.StudioSettings.SuppressPreview = !suppress;

            if (this.CurrentGraphCanvas != null)
                this.CurrentGraphCanvas.Controller.DoTogglePreview(uint.MaxValue);
        }

        private void OnDataReportingClicked(object sender, RoutedEventArgs e)
        {
            if (collectInfoManager == null)
                throw new InvalidOperationException("CollectInfoManager is not initialized (31648931FBC4)");

            collectInfoManager.SetCollectInfoOption(this.EnableUsabilityDataReportingItem.IsChecked);
        }

        private void OnOptionAboutClicked(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow(new LoggerWrapper());
            if (null != Application.Current)
            {
                aboutWindow.Owner = Application.Current.MainWindow;
                aboutWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }

            aboutWindow.ShowDialog();
            if (false != aboutWindow.InstallNewUpdate)
            {
                // The about window was closed by clicking on the "Update" link.
                UpdateManager.Instance.QuitAndInstallUpdate();
            }
        }

        private void OnOptionReportClicked(object sender, RoutedEventArgs e)
        {
            if (collectInfoManager == null)
                throw new InvalidOperationException("CollectInfoManager is not initialized (D8BF3BF5D96B)");

            if (collectInfoManager.GetCollectInfoOption())
            {
                ReportIssueFeedback reportIssueWindow = new ReportIssueFeedback();
                if (null != Application.Current)
                    reportIssueWindow.Owner = Application.Current.MainWindow;
                reportIssueWindow.Closing += OnReportIssueWindowClosing;
                reportIssueWindow.Show();
            }
            else
            {
                new PopupDialog(ResourceNames.Warning, UiStrings.ReportIssueDisabled, string.Empty, string.Empty, "Ok");
            }
        }

        private void OnReportIssueWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ReportIssueFeedback reportIssueWindow = sender as ReportIssueFeedback;
            if (!reportIssueWindow.SendMessage)
                return;

            try
            {
                System.Security.Cryptography.SHA1 sha = new System.Security.Cryptography.SHA1CryptoServiceProvider();
                byte[] b64Name = System.Text.ASCIIEncoding.ASCII.GetBytes(System.Environment.UserName);

                byte[] hashed = sha.ComputeHash(b64Name);

                string hashedName = System.Text.ASCIIEncoding.ASCII.GetString(hashed);

                Logger.LogInfo("UserReportInfo", hashedName);
            }

            catch (Exception)
            {
            }

            Logger.LogInfo("UserReportType", reportIssueWindow.FeedbackType);
            Logger.LogInfo("UserReport", reportIssueWindow.Report);

            if (reportIssueWindow.Email != string.Empty)
                Logger.LogInfo("UserEmail", reportIssueWindow.Email);

            if (CurrentGraphCanvas != null && CurrentGraphCanvas.VisualHost != null)
            {
                string uiStates = CurrentGraphCanvas.Controller.GetRecordedUiStates();
                string vmStates = CurrentGraphCanvas.Controller.GetRecordedVmStates();
                Logger.LogError("Backtrace-uiStates", uiStates);
                Logger.LogError("Backtrace-vmStates", vmStates);
            }

            MessageBox.Show(UiStrings.ReportIssueAcknowledge, "Report Issue");
        }

        private void OnMasterGridSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (this.libraryView != null)
            {
                libraryView.SetHeight(e.NewSize.Height);
            }
        }

        void OnScrollViewerKeyDown(object sender, KeyEventArgs e)
        {
            return;// Avoid calling base implementation of keydown here,
            // so that the END/HOME keys will have no effect on the canvas.
        }

        void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // This event will be sent in two known conditions:
            //     1. When user pans with view.
            //     2. When user types within the edit box that shows up while editing node.
            // 
            // The second case is due to adjusting the edit box location that indirectly 
            // causes the layout of the canvas to be updated. That results in scroll changed 
            // event getting sent. 
            // 
            // Here we would only like to handle the first case, when user actually pans the
            // view, and nothing else. To determine if its the first case, we will then have 
            // to see who is the "original source" of this scroll event. If this is called 
            // due to sources other than the scroll viewer itself, ignore it.
            // 
            ScrollViewer scrollViewer = e.Source as ScrollViewer; // Sent from scroll viewer?
            if (null != scrollViewer && CurrentGraphCanvas != null && (CurrentGraphCanvas.VisualHost != null))
                if (scrollViewer.Name == "canvasScrollViewer")
                    CurrentGraphCanvas.VisualHost.HandleScrollViewerScrollChanged(sender, e);
            e.Handled = true;
        }

        void OnSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CurrentGraphCanvas != null && CurrentGraphCanvas.VisualHost != null)
                CurrentGraphCanvas.VisualHost.HandleSliderValueChanged(sender, e);
            e.Handled = true;
        }

        private void OnCanvasModified(object sender, EventArgs e)
        {
            IGraphController graphController = (IGraphController)sender;
            if (graphController == null)
                return;

            tabControl.UpdateTabText(graphController.Identifier);
        }

        private void HandleShortcutDumpState(bool dumpUiStates)
        {
            if (null != CurrentGraphCanvas.VisualHost)
            {
                string filePath = CurrentGraphCanvas.VisualHost.DumpRecordedStates(dumpUiStates);
                if (System.IO.File.Exists(filePath))
                    System.Diagnostics.Process.Start(filePath);
            }
        }

        #endregion

        #region Private Class Helper Methods

        private void CreateBlankOrRecoveredCanvas(bool recovery)
        {
            GraphCanvas graphCanvas = null;

            if (false != recovery) // We're in recovery mode.
            {
                if (null == filesToRecover || (filesToRecover.Count <= 0))
                    throw new InvalidOperationException("Nothing to recover (A0FFDCB8BBE9)");

                try
                {
                    // We did find some files to recover from...
                    uint graphId = filesToRecover.Keys.ElementAt(0);
                    string fileToRecover = filesToRecover[graphId];
                    graphCanvas = new GraphCanvas(this, fileToRecover);
                }
                catch (Exception)
                {
                    // Some problem due to recovery, we can't do much about those 
                    // backup files now, so proceed to create the default blank 
                    // canvas as per normal. We don't want some backup files to 
                    // cause DSS to be not-launchable.
                }
            }

            if (null == graphCanvas) // Just in case recovery failed.
            {
                // Not in recovery mode, create empty.
                graphCanvas = new GraphCanvas(this);
            }

            //create the first canvas
            graphCanvases.Add(graphCanvas);
            currentGraphCanvas = graphCanvases.Count - 1;
            canvasGrid.Children.Add(graphCanvas);
            newCanvasCount++;
            string header = "Graph" + newCanvasCount.ToString();
            tabControl.AddTab(header, graphCanvas);

            canvasScrollViewer.Focusable = false;
            graphCanvas.Focusable = true;
            graphCanvas.Focus();

            ResetZoomAndPan();
        }

        private void AddGraphCanvas(GraphCanvas graphCanvas, string canvasName)
        {
            if (graphCanvases.Count == 0)
            {
                this.libraryView.Visibility = Visibility.Visible;
                this.zoomAndPanControl.Visibility = Visibility.Visible;
            }
            //add canvas
            graphCanvases.Add(graphCanvas);
            canvasGrid.Children.Add(graphCanvas);
            currentGraphCanvas = graphCanvases.Count - 1;

            //add tab
            string tabName = string.Empty;
            if (string.IsNullOrEmpty(canvasName))
            {
                newCanvasCount++;
                tabName = DesignScriptStudio.Graph.Core.Configurations.DefaultName + newCanvasCount.ToString();
            }
            else
                tabName = canvasName;
            tabControl.AddTab(tabName, graphCanvas);

            //reset canvas
            ResetZoomAndPan();

            if (null != this.hostApplication)
                this.hostApplication.GraphActivated((uint)currentGraphCanvas);
        }

        private GraphCanvas GetCanvas(uint identifier)
        {
            IGraphController controller = GetController(identifier);
            foreach (GraphCanvas canvas in graphCanvases)
            {
                if (canvas.Controller == controller)
                    return canvas;
            }
            return null;
        }

        private void ResetZoomAndPan()
        {
            GraphVisualHost.sliderScrollHandled = true;

            slider.Value = 1;
            this.scaleTransform.ScaleX = 1;
            this.scaleTransform.ScaleY = 1;
            canvasScrollViewer.ScrollToHorizontalOffset(canvasGrid.Width / 2);
            canvasScrollViewer.ScrollToVerticalOffset(canvasGrid.Height / 2);
        }

        private bool CheckPath(object tabItem)
        {
            GraphTabControl.GraphTabItem item = tabItem as GraphTabControl.GraphTabItem;
            if (item != null)
            {
                if (!string.IsNullOrEmpty(item.Canvas.Controller.FilePath))
                    return true;
            }
            return false;
        }

        #endregion
    }
}
