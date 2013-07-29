using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Automation.Peers;
using System.Windows.Automation;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows;
using System.Xml.Serialization;
using Microsoft.Win32;
using System.ComponentModel;
using System.Windows.Markup;

namespace DesignScript.Editor
{
    using DesignScript.Editor.Core;
    using DesignScript.Parser;
    using ProtoCore.DSASM.Mirror;
    using ProtoFFI;
    using ProtoCore;
    using ProtoCore.Exceptions;
    using ProtoCore.CodeModel;
    using DesignScript.Editor.Automation;
    using System.Threading;
    using DesignScript.Editor.StartUp;
    using DesignScript.Editor.Common;
    using DesignScript.Editor.Ui;

    public partial class TextEditorControl : UserControl
    {
        private static TextEditorControl textEditorControl = null;
        private static IHostApplication hostApplication = null;
        private static DialogProvider dialogProvider = null;

        // Start up related data members.
        StartUpWorker startUpWorker = null;

        DispatcherTimer parseTimer = null;
        DispatcherTimer statusInfoTimer = null;
        DispatcherTimer slowMotionExecutionTimer = null;
        ITextEditorCore textCore = null;
        CommandPlayer player = null;
        CommandRecorder actionRecorder = null;
        ScriptModifiedHandler scriptModifiedHandler = null;
        List<EditorExtension> editorExtensions = null;
        UpdateNotificationControl updateNotifier = null;
        RuggedProgressBar statusProgressBar = null;
        string importFileName = null;

        int newDocCount = 0;
        bool textEditorInitialized = false;
        bool isGeneratorOn = false;

        #region Public Class Operational Methods

        public TextEditorControl()
        {
            if (null != textEditorControl)
                throw new InvalidOperationException("'TextEditorControl' should be a singleton!");

            textEditorControl = this;
            InitializeComponent();
            InitializeEditor();
        }

        public TextEditorControl(IHostApplication hostApplication)
        {
            Stopwatch launchWatch = new Stopwatch();
            launchWatch.Start();

            if (null != TextEditorControl.hostApplication)
                throw new InvalidOperationException("'TextEditorControl' should be a singleton!");
            if (null != textEditorControl)
                throw new InvalidOperationException("'TextEditorControl' should be a singleton!");

            textEditorControl = this;
            TextEditorControl.hostApplication = hostApplication;

            TextEditorControl.dialogProvider = new DialogProvider();
            CoreInterfaceFactory.RegisterInterfaces(hostApplication, dialogProvider);
            textCore = CoreInterfaceFactory.CreateTextEditorCore(OnExecutionStateChanged);

            actionRecorder = new CommandRecorder();
            textCore.SetCommandRecorder(actionRecorder); // Only for human users.

            InitializeComponent();
            InitializeEditor();
            startUpWorker = new StartUpWorker();
            startUpWorker.InitializeStartUpWorker();
            startUpWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(OnStartUpWorkerRunWorkerCompleted);
            startUpWorker.RunWorkerAsync();
            EnumerateExtensions();

            this.LayoutUpdated += new EventHandler(OnTextEditorLayoutUpdated);
            launchWatch.Stop();
            Logger.LogPerf("TextEditorControl.ctor", launchWatch.ElapsedMilliseconds + " ms");
        }

        public void Shutdown()
        {
            if (null != textCore)
                textCore.Shutdown();
        }

        public ITextEditorSettings GetEditorSettings()
        {
            return textCore.TextEditorSettings;
        }

        public void DisplayStatusMessage(StatusTypes statusType, string message, int seconds)
        {
            if (string.IsNullOrEmpty(message))
                return;

            InfoMessage.Text = message;
            InfoMessage.Visibility = Visibility.Visible;
            InfoStatusImage.Visibility = Visibility.Visible;

            string imageUrl = string.Empty;
            switch (statusType)
            {
                case StatusTypes.Warning: imageUrl = Images.StatusWarning; break;
                case StatusTypes.Info: imageUrl = Images.StatusInfo; break;
                case StatusTypes.Error: imageUrl = Images.StatusError; break;

                default:
                    throw new InvalidOperationException("'statusTypes' unhandled!");
            }

            Uri src = new Uri(imageUrl);
            BitmapImage img = new BitmapImage(src);
            InfoStatusImage.Source = img;

            if (0 != seconds)
            {
                if (null == statusInfoTimer)
                {
                    statusInfoTimer = new DispatcherTimer();
                    statusInfoTimer.Tick += new EventHandler(OnInfoTimerTick);
                }

                statusInfoTimer.Stop();
                statusInfoTimer.Interval = new TimeSpan(0, 0, seconds);
                statusInfoTimer.Start();
            }
        }

        public void HideStatusMessage()
        {
            InfoMessage.Text = string.Empty;
            InfoMessage.Visibility = Visibility.Collapsed;
            InfoStatusImage.Visibility = Visibility.Collapsed;
        }

        public void ScheduleExecution(bool debugMode)
        {
            this.Dispatcher.BeginInvoke(new DoScheduledExecutionDelegate(DoScheduledExecution), debugMode);
        }

        public void SetFocusOnForegroundContent()
        {
            FrameworkElement startUpScreen = null;
            foreach (FrameworkElement child in textEditorControl.grid.Children)
            {
                if (child.Name == "StartUpScreen")
                    startUpScreen = child;
            }

            if (null != startUpScreen)
            {
                if (startUpScreen.Visibility == Visibility.Visible)
                    startUpScreen.Focus();
                return;
            }

            if ((null != textCanvas) && textCanvas.IsVisible)
                Keyboard.Focus(textCanvas);
        }

        internal void PauseActionPlayback(bool pausePlayback)
        {
            if (null != player)
                player.PausePlayback(pausePlayback);
        }

        internal void PlaybackNextCommand()
        {
            if (null == player)
                return;

            player.PlaybackNextCommand();
        }

        internal void ShowTabControls(bool showScriptTabs)
        {
            Visibility scriptTabsVisibility = Visibility.Collapsed;
            if (false != showScriptTabs)
                scriptTabsVisibility = System.Windows.Visibility.Visible;

            scrollViewer.Visibility = scriptTabsVisibility;
            ScriptTabControl.Visibility = scriptTabsVisibility;

            // Hide just the start-up page, keep tab as-is.
            CheckTabControlVisibility(false);
        }

        internal bool ShouldDisplayCaret()
        {
            if (false != textCanvas.IsKeyboardFocused)
                return true;

            if (null != editorExtensions)
            {
                foreach (EditorExtension extension in editorExtensions)
                {
                    if (extension.HasInputFocus())
                        return true;
                }
            }

            return false;
        }

        internal void CheckTabControlVisibility(bool startUpPageVisible)
        {
            bool startUpTabVisible = (startUpWorker.StartUpTabControl.Visibility == Visibility.Visible);
            CheckTabControlVisibility(startUpTabVisible, startUpPageVisible);
        }

        internal void CheckTabControlVisibility(bool startUpTabVisible, bool startUpPageVisible)
        {
            if (false == startUpTabVisible)
                startUpPageVisible = false;

            bool scriptTabVisible = (ScriptTabControl.Visibility == Visibility.Visible);
            bool showRectangle = scriptTabVisible ^ startUpTabVisible;

            FrameworkElement startUpScreen = null, rectangle = null;
            foreach (FrameworkElement child in textEditorControl.InnerGrid.Children)
            {
                if (child.Name == "StartUpRectangle")
                    rectangle = child;
            }

            foreach (FrameworkElement child in textEditorControl.grid.Children)
            {
                if (child.Name == "StartUpScreen")
                    startUpScreen = child;
            }

            if (null != startUpScreen)
            {
                startUpScreen.Visibility = Visibility.Collapsed;
                if (false != startUpPageVisible)
                {
                    startUpScreen.Visibility = Visibility.Visible;
                    startUpScreen.Focus();

                }
            }

            if (null != rectangle)
            {
                rectangle.Visibility = Visibility.Collapsed;
                if (false != showRectangle)
                    rectangle.Visibility = Visibility.Visible;
            }

            startUpWorker.StartUpTabControl.Visibility = Visibility.Collapsed;
            StartUpButton.Visibility = System.Windows.Visibility.Visible;
            if (false != startUpTabVisible)
            {
                startUpWorker.StartUpTabControl.Visibility = Visibility.Visible;
                StartUpButton.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        #endregion

        #region Public Class Properties

        public bool TextCanvasHasKeyboardFocus
        {
            get
            {
                if (false != textCanvas.IsKeyboardFocused)
                    return textCanvas.IsKeyboardFocused;

                if (null != editorExtensions)
                {
                    foreach (EditorExtension extension in editorExtensions)
                    {
                        if (extension.HasInputFocus())
                            return true;
                    }
                }

                return (this.IsKeyboardFocusWithin);
            }
        }

        internal static TextEditorControl Instance
        {
            get { return TextEditorControl.textEditorControl; }
        }

        internal ITextEditorCore TextCore
        {
            get { return textCore; }
        }

        internal bool IsInPlaybackMode { get { return (null != player); } }

        internal static IHostApplication HostApplication
        {
            get { return TextEditorControl.hostApplication; }
        }

        internal static IDialogProvider DialogProvider
        {
            get { return TextEditorControl.dialogProvider; }
        }

        #endregion

        #region Private Class Event Handlers

        private void OnLoaded(object sender, EventArgs e)
        {
            // Kick start a check for update version info, if the update manager is idle.
            UpdateManager.Instance.UpdateDownloaded += OnUpdatePackageDownloaded;
            UpdateManager.Instance.CheckForProductUpdate();
            UpdateManager.Instance.ShutdownRequested += OnUpdateManagerShutdownRequested;

            string filepath = Configurations.GetSettingsFilePath();
            if (!File.Exists(filepath))
            {
                CollectInfoDialog collectInfoDialog = new CollectInfoDialog();
                if (null != Application.Current)
                    collectInfoDialog.Owner = Application.Current.MainWindow;
                collectInfoDialog.ShowDialog();

                textCore.TextEditorSettings.CollectFeedback = collectInfoDialog.CollectFeedback;
                if (textCore.TextEditorSettings.CollectFeedback)
                    Logger.FORCE_Log("CollectFeedbackOptIn", "Opt In");
                else
                    Logger.FORCE_Log("CollectFeedbackOptIn", "Opt Out");
            }

            this.textCanvas.Focus();
        }

        private void OnUpdatePackageDownloaded(object sender, UpdateDownloadedEventArgs e)
        {
            if (null != e && null == e.Error && e.UpdateAvailable)
            {
                if (null == updateNotifier)
                {
                    updateNotifier = new UpdateNotificationControl(new LoggerWrapper());
                    updateNotifier.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    updateNotifier.Margin = new Thickness(0, 0, 1, 0);
                    updateNotifier.SetValue(Grid.ColumnProperty, 1);
                    this.MenuSplitter.Children.Add(updateNotifier);
                    this.MenuSplitter.UpdateLayout();
                }
                updateNotifier.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void OnUpdateManagerShutdownRequested(object sender, EventArgs e)
        {
            hostApplication.BeginQuit += new EventHandler(UpdateManager.Instance.HostApplicationBeginQuit);
            hostApplication.QuitApplication();
        }

        private void OnTextEditorLayoutUpdated(object sender, EventArgs e)
        {
            //System.Threading.Thread.Sleep(4000);
            if (false != textEditorInitialized)
                return; // Already initialized.

            textEditorInitialized = true;

            //Get command line arguments
            string[] arguments = hostApplication.GetApplicationArguments();

            if (arguments != null && (arguments.Length > 0))
            {
                if (arguments[0] == "/mad")
                    isGeneratorOn = true;
                else
                {
                    player = new CommandPlayer(this, textCore);
                    player.SetApplicationArguments(arguments);
                    player.BeginPlayback();
                }
            }

            if (arguments == null || isGeneratorOn)
            {
                //textCore.CreateNewScript();
                //SetupTabInternal(null);

                // Instantiating Stress Testing Generator
                if (isGeneratorOn == true)
                {
                    Generator generator = new Generator(arguments, this.textCore, this);
                }

                grid.UpdateLayout();
                textCanvas.Focus();
            }

            // Start with a blank new file on startup
            if (parseTimer == null)
            {
                parseTimer = new DispatcherTimer();
                parseTimer.Tick += new EventHandler(OnParseTimerTick);
                parseTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
                parseTimer.Start();
            }

            // The start-up thread has finished loading earlier than the main UI, 
            // we'll go ahead and finalize the start-up screen for display.
            if (startUpWorker.IsBusy == false)
                startUpWorker.FinalizeStartUpScreen();
        }

        private void OnStartUpWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new System.Action(this.FinalizeStartUpScreen));
        }

        private void OnStartUpIconClick(object sender, MouseButtonEventArgs e)
        {
            CheckTabControlVisibility(true, true);
        }

        private void OnExecutionStateChanged(object sender, ExecutionStateChangedEventArgs e)
        {
            bool executionInProgress = false;
            bool refreshOutputPanes = false;
            bool refreshInspectionPane = false;
            bool refreshOnScreenBreakpoints = false;
            bool refreshExecutionCursor = false;
            Color textCanvasBackground = Colors.White;
            Color scrollViewerBackground = Colors.White;

            switch (e.CurrentState)
            {
                case ExecutionStateChangedEventArgs.States.None:
                    refreshOutputPanes = true;
                    refreshInspectionPane = true;
                    refreshOnScreenBreakpoints = true;

                    textCore.ReadOnlyState = false;
                    textCanvasBackground = Colors.White;
                    scrollViewerBackground = Colors.White;
                    break;

                case ExecutionStateChangedEventArgs.States.Running:
                    executionInProgress = true;
                    textCore.ReadOnlyState = true;
                    textCanvasBackground = UIColors.TextCanvasBackground;
                    scrollViewerBackground = UIColors.ScrollViewerBackground;
                    break;

                case ExecutionStateChangedEventArgs.States.Debugging:
                    executionInProgress = true;
                    textCore.ReadOnlyState = true;
                    textCanvasBackground = UIColors.TextCanvasBackground;
                    scrollViewerBackground = UIColors.ScrollViewerBackground;
                    break;

                case ExecutionStateChangedEventArgs.States.Paused:
                    refreshOutputPanes = true;
                    refreshInspectionPane = true;
                    refreshOnScreenBreakpoints = true;
                    refreshExecutionCursor = true;

                    textCore.ReadOnlyState = true;
                    textCanvasBackground = UIColors.TextCanvasBackground;
                    scrollViewerBackground = UIColors.ScrollViewerBackground;
                    break;

                case ExecutionStateChangedEventArgs.States.Stopped:
                    refreshOutputPanes = true;
                    refreshInspectionPane = true;
                    refreshOnScreenBreakpoints = true;

                    textCore.ReadOnlyState = true;
                    textCanvasBackground = UIColors.TextCanvasBackground;
                    scrollViewerBackground = UIColors.ScrollViewerBackground;
                    break;
            }

            // Display/hide execution status bar depending on the state.
            ShowStatusProgressBar(executionInProgress);

            if (false != refreshExecutionCursor)
            {
                CodeRange executionCursor = new CodeRange();
                IExecutionSession session = Solution.Current.ExecutionSession;
                if (session.GetExecutionCursor(ref executionCursor))
                {
                    if (null != executionCursor.StartInclusive.SourceLocation)
                    {
                        CodePoint codePoint = executionCursor.StartInclusive;
                        ActivateScriptByPath(codePoint.SourceLocation.FilePath);
                    }

                    UpdateCaretPosition();
                    textCanvas.SetExecutionCursor(executionCursor);
                }
                else
                    textCanvas.ClearExecutionCursor(); // Busy?
            }
            else
            {
                // Either busy executing or not execution at all, 
                // we don't show the execution cursor for both cases.
                textCanvas.ClearExecutionCursor();
            }

            if (false != refreshOutputPanes)
            {
                IOutputStream outputStream = Solution.Current.GetMessage(false);
                List<ProtoCore.OutputMessage> outputMessages = outputStream.GetMessages();
                Solution.Current.AddInlineMessages(outputMessages);
                OutputWindow.Instance.SetOutputMessage(outputMessages);
                ErrorWindow.Instance.SetErrorMessage(outputMessages);
            }

            if (false != refreshInspectionPane)
            {
                // Inspection window cannot be updated while the vm is running 
                // in the background (i.e. being in 'Debugging' or 'Running' 
                // state). Other than that we'll refresh the inspection view, 
                // even when it is 'None', in which case the inspection view is 
                // refreshed with 'null' values.
                InspectionViewControl.Instance.RefreshInspectionView();
            }

            if (false != refreshOnScreenBreakpoints)
                textCanvas.BreakpointsUpdated();

#if DEBUG
            if (e.CurrentState == ExecutionStateChangedEventArgs.States.Debugging)
            {
                if (e.PreviousState == ExecutionStateChangedEventArgs.States.None)
                {
                    List<DisassemblyEntry> instructions = new List<DisassemblyEntry>();
                    Solution.Current.ExecutionSession.PopulateInstructions(instructions);
                    Disassembly.Instance.BindNewInstructionList(instructions);
                }
            }
#endif

            textCanvas.Background = new SolidColorBrush(textCanvasBackground);
            scrollViewer.Background = new SolidColorBrush(scrollViewerBackground);
            CommandManager.InvalidateRequerySuggested(); // Refresh button states.
        }

        private void OnCanvasMenuOpened(object sender, RoutedEventArgs e)
        {
            // This item should be disabled/hidden by default.
            CanvasMenuItemImport.IsEnabled = false;
            CanvasMenuItemImport.Visibility = Visibility.Collapsed;
            CanvasMenuItemCopy.IsEnabled = textCore.HasSelection;
            CanvasMenuItemCut.IsEnabled = !IsExecuting() && textCore.HasSelection;
            CanvasMenuItemPaste.IsEnabled = !IsExecuting();

            CodeFragment fragment = null;

            CanvasMenuItemAddtoWatch.IsEnabled = false;
            int line = textCore.CursorPosition.Y;
            int column = textCore.CursorPosition.X;

            if (textCore.HasSelection)
                CanvasMenuItemAddtoWatch.IsEnabled = true;
            else
            {

                if (textCore.GetFragment(column, line, out fragment) != 0)
                {
                    if (fragment.CodeType == CodeFragment.Type.Local)
                        CanvasMenuItemAddtoWatch.IsEnabled = true;
                }
            }

            if (null != hostApplication)
            {
                // This may not be the first call, so it is possible that some 
                // host-specific items have gotten onto the menu, and they need 
                // to be removed. How can we tell if it's a host-specific menu 
                // item? Well, by looking at 'menuItem.CommandParameter', if it 
                // is a value other than 'null', then we know for sure that it 
                // was being added by a prior call to this method (and therefore 
                // should be removed here).
                // 
                List<object> itemsToRemove = new List<object>();
                ContextMenu contextMenu = sender as ContextMenu;
                foreach (var item in contextMenu.Items)
                {
                    MenuItem menuItem = item as MenuItem;
                    if (null != menuItem && (null != menuItem.CommandParameter))
                        itemsToRemove.Add(item);
                }

                foreach (object item in itemsToRemove)
                    contextMenu.Items.Remove(item);

                if (contextMenu.Items.Count > 0)
                {
                    // Remove the separator if it is the last item on the menu.
                    if (contextMenu.Items[contextMenu.Items.Count - 1] is Separator)
                        contextMenu.Items.RemoveAt(contextMenu.Items.Count - 1);
                }

                ITextBuffer context = textCore.CurrentTextBuffer;
                Dictionary<int, string> menuItems = new Dictionary<int, string>();
                if (hostApplication.GetContextualMenu(menuItems, line, column, context))
                {
                    contextMenu.Items.Add(new Separator());

                    foreach (var menuItem in menuItems)
                    {
                        MenuItem menuItemObject = new MenuItem();
                        menuItemObject.Header = menuItem.Value;
                        menuItemObject.CommandParameter = menuItem.Key;
                        menuItemObject.Click += new RoutedEventHandler(OnHostApplicationMenuItemClick);
                        contextMenu.Items.Add(menuItemObject);
                    }
                }
            }

            textCore.GetFragment(mouseCharacterPosition.X, mouseCharacterPosition.Y, out fragment);

            if (fragment == null)
                return;

            importFileName = string.Empty;
            if (fragment.CodeType == CodeFragment.Type.Keyword) // Possible "import" statement.
            {
                importFileName = GetImportedFileName(fragment);
            }
            else if (fragment.CodeType == CodeFragment.Type.Text) // Possible imported file name.
            {
                CodeFragment bracketFragment = null, importFragment = null;
                textCore.GetPreviousFragment(fragment.ColStart, fragment.Line, out bracketFragment);
                if (null != bracketFragment && (bracketFragment.Text == "("))
                {
                    textCore.GetPreviousFragment(bracketFragment.ColStart,
                        bracketFragment.Line, out importFragment);
                }

                // "importFragment" may or may not be 'null'.
                importFileName = GetImportedFileName(importFragment);
            }

            if (!string.IsNullOrEmpty(importFileName) && (importFileName.Contains(".ds")))
            {
                importFileName = importFileName.Replace("_", "__");
                CanvasMenuItemImport.Header = string.Format(Configurations.OpenImportedFile, importFileName);
                CanvasMenuItemImport.IsEnabled = true;
                CanvasMenuItemImport.Visibility = Visibility.Visible;
            }
        }

        private void OnHostApplicationMenuItemClick(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            int menuItemId = ((int)menuItem.CommandParameter);
            hostApplication.HandleMenuItemClick(menuItemId);
        }

        private string GetImportedFileName(CodeFragment importFragment)
        {
            if (null == importFragment || (importFragment.CodeType != CodeFragment.Type.Keyword))
                return string.Empty;
            if (importFragment.Text != "import")
                return string.Empty;

            CodeFragment bracketFragment = null;
            if (textCore.GetNextFragment(importFragment, out bracketFragment) == 0)
                return string.Empty;
            if (null == bracketFragment || (bracketFragment.Text != "("))
                return string.Empty;

            CodeFragment fileNameFragment = null;
            if (textCore.GetNextFragment(bracketFragment, out fileNameFragment) == 0)
                return string.Empty;
            if (null == fileNameFragment || (fileNameFragment.CodeType != CodeFragment.Type.Text))
                return string.Empty;

            string quoted = fileNameFragment.Text;
            if (string.IsNullOrEmpty(quoted) || (quoted.Length <= 2))
                return string.Empty;

            return quoted.Substring(1, quoted.Length - 2);
        }

        private void OnCanvasMenuItemClick(object sender, RoutedEventArgs e)
        {
            IScriptObject currScript = Solution.Current.ActiveScript;
            MenuItem menuItem = e.OriginalSource as MenuItem;

            bool isContentChangingOperation = false;
            switch (menuItem.Name)
            {
                case "CanvasMenuItemCopy":
                    textCore.DoCopyText(false);
                    break;
                case "CanvasMenuItemCut":
                    {
                        if (textCore.ReadOnlyState == true)
                            DisplayStatusMessage(StatusTypes.Error, Configurations.EditingError, 3);
                        else
                        {
                            textCore.DoCopyText(true);
                            UpdateCaretPosition();
                            UpdateScriptDisplay(currScript);
                            isContentChangingOperation = true;
                        }
                        break;
                    }
                case "CanvasMenuItemPaste":
                    {
                        if (textCore.ReadOnlyState == true)
                            DisplayStatusMessage(StatusTypes.Error, Configurations.EditingError, 3);
                        else
                        {
                            textCore.DoPasteText();
                            UpdateScriptDisplay(currScript);
                            isContentChangingOperation = true;
                        }
                        break;
                    }
                case "CanvasMenuItemAddtoWatch":
                    {
                        string expression = string.Empty;
                        if (textCore.HasSelection == false)
                        {
                            int line = textCore.CursorPosition.Y;
                            int column = textCore.CursorPosition.X;

                            CodeFragment fragment = null;
                            if (textCore.GetFragment(column, line, out fragment) != 0)
                            {
                                if (fragment.CodeType == CodeFragment.Type.Local)
                                    expression = fragment.Text;
                            }
                        }
                        else
                            expression = textCore.SelectionText;

                        InspectionViewControl.Instance.AddInspectionVariable(expression);
                        editorWidgetBar.ActivateWidget(EditorWidgetBar.Widget.Watch, true);
                        break;
                    }

                case "CanvasMenuItemImport":
                    {
                        string filePath = null;
                        string activeScriptPath = Solution.Current.ActiveScript.GetParsedScript().GetScriptPath();

                        if (string.IsNullOrWhiteSpace(activeScriptPath))
                            activeScriptPath = System.IO.Directory.GetCurrentDirectory();

                        string[] directories = 
                        {
                            System.IO.Path.GetDirectoryName(activeScriptPath),
                            textCore.TextEditorSettings.IncludePath
                        };

                        if (importFileName != null)
                        {
                            foreach (string directory in directories)
                            {
                                if (string.IsNullOrEmpty(directory))
                                    continue;

                                string folderPath = directory;
                                if (!folderPath.EndsWith("\\"))
                                    folderPath += "\\";

                                filePath = folderPath + importFileName;
                                if (System.IO.File.Exists(filePath))
                                    break;
                            }
                        }

                        if (string.IsNullOrEmpty(filePath) || (false == System.IO.File.Exists(filePath)))
                        {
                            string message = string.Format("Could not locate file: {0}", importFileName);
                            DisplayStatusMessage(StatusTypes.Error, message, 3);
                            return; // Nothing to do, again.
                        }

                        if (textCore.LoadScriptFromFile(filePath))
                        {
                            SetupTabInternal(filePath);
                        }
                        else
                        {
                            int index = Solution.Current.GetScriptIndexFromPath(filePath);
                            if (index >= 0)
                            {
                                ScriptTabControl.ActivateTab(index);
                            }
                        }

                        HandleScriptActivation();

                        grid.UpdateLayout();
                        CommandManager.InvalidateRequerySuggested();
                        textCanvas.Focus();

                        IScriptObject script = Solution.Current.ActiveScript;
                    }

                    break;
            }

            if (false != isContentChangingOperation)
                UpdateUiForModifiedScript(currScript);
        }

        private void OnTabControlTabChanged(object sender, TabChangedEventArgs e)
        {
            if ((sender is CurvyTabControl) == false)
                return; // We don't handle any other controls

            HandleScriptActivation();
        }

        private void OnTabControlTabClosed(object sender, TabChangedEventArgs e)
        {
            textCore.CloseScript(e.OldTabIndex);

            if (e.NewTabIndex == -1)
            {
                ShowTabControls(false);
                OutputWindow.ClearOutput();
                CheckTabControlVisibility(true);
            }
            else
            {
                // If there's no script left, then 
                // don't bother with script activation.
                HandleScriptActivation();
            }
        }

        private void OnTabMenuItemClick(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)e.OriginalSource;
            if (ScriptTabControl.HandleContextMenu(menuItem.Name))
                return; // Menu item has been processed.

            IScriptObject script = Solution.Current.ActiveScript;
            IParsedScript currentScript = (null == script ? null : script.GetParsedScript());

            switch (menuItem.Name)
            {
                case "CopyFullPath":
                    if (null != currentScript)
                    {
                        string scriptPath = currentScript.GetScriptPath();
                        if (string.IsNullOrEmpty(scriptPath) == false)
                            Clipboard.SetText(scriptPath);
                    }

                    break;

                case "OpenFolder":
                    if (null != currentScript)
                    {
                        string scriptPath = currentScript.GetScriptPath();
                        if (string.IsNullOrEmpty(scriptPath) == false)
                        {
                            string directory = System.IO.Path.GetDirectoryName(scriptPath);
                            System.Diagnostics.Process.Start("explorer.exe", string.Format("/select, \"{0}\"", scriptPath));
                        }
                        else
                        {
                            MessageBox.Show("Perhaps try again after the script is saved?",
                                "Nowhere to go", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }

                    break;
            }
        }

        private void OnCloseScript(object sender, RoutedEventArgs e)
        {
            ScriptTabControl.CloseCurrentTab();
        }

        private void OnParseTimerTick(object sender, EventArgs e)
        {
            // @TODO(Ben): Implement this for red-underscore.
            return;
        }

        private void OnSlowMotionPlayBackTick(object sender, EventArgs e)
        {
            UpdateUiForStepNext(textCore.Step(RunMode.StepNext));
            if (null == slowMotionExecutionTimer)
                CommandManager.InvalidateRequerySuggested();
        }

        private void OnSlowMotionTimerValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int seconds = 0;
            int miliseconds = 0;
            if (sldSlowMoTimer.Value % 1 != 0)
            {
                double dec = sldSlowMoTimer.Value % 1;
                miliseconds = (int)(dec / 0.1);
            }
            seconds = (int)sldSlowMoTimer.Value;
            txtSlowMoTimer.Text = (seconds + 0.1 * miliseconds).ToString();
        }

        private void OnReportButtonClick(object sender, RoutedEventArgs e)
        {
            ITextEditorSettings settings = textCore.TextEditorSettings;
            if (settings.CollectFeedback == false)
            {
                ReportIssueFeedbackDialog reportIssueFeedback = new ReportIssueFeedbackDialog();
                if (null != Application.Current)
                    reportIssueFeedback.Owner = Application.Current.MainWindow;
                reportIssueFeedback.ShowDialog();
                return;
            }

            ReportIssueFeedback reportIssueWindow = new ReportIssueFeedback();
            if (null != Application.Current)
                reportIssueWindow.Owner = Application.Current.MainWindow;
            reportIssueWindow.Closing += OnReportIssueWindowClosing;
            reportIssueWindow.Show();
        }

        private void OnSetIncludePath(object sender, RoutedEventArgs e)
        {
            ITextEditorSettings settings = textCore.TextEditorSettings;
            System.Windows.Forms.FolderBrowserDialog browserDialog = new System.Windows.Forms.FolderBrowserDialog();
            browserDialog.RootFolder = Environment.SpecialFolder.MyComputer;

            if (string.IsNullOrEmpty(settings.IncludePath))
                browserDialog.SelectedPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            else
                browserDialog.SelectedPath = settings.IncludePath;

            string format = "Current library path - {0}\n\nSelect the new library path..";
            browserDialog.Description = string.Format(format, browserDialog.SelectedPath);

            if (browserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                settings.IncludePath = browserDialog.SelectedPath;
            }
        }

        private void OnSetItemSubmenuOpened(object sender, RoutedEventArgs e)
        {
            SmartFormattingItem.IsChecked = textCanvas.TextEditorCore.TextEditorSettings.EnableSmartFormatting;
            DisplayOutputItem.IsChecked = textCanvas.TextEditorCore.TextEditorSettings.DisplayOutput;
            NumericSliderItem.IsChecked = textCanvas.TextEditorCore.TextEditorSettings.EnableNumericSlider;
            DataReportingItem.IsChecked = textCanvas.TextEditorCore.TextEditorSettings.CollectFeedback;
            AsynchronousItem.IsChecked = Solution.Current.Asynchronous;
        }

        private void OnDataReportingClicked(object sender, RoutedEventArgs e)
        {
            ITextEditorSettings settings = textCore.TextEditorSettings;
            settings.CollectFeedback = DataReportingItem.IsChecked;

            if (settings.CollectFeedback)
            {
                Logger.FORCE_Log("OnDataReportingClicked", "Opt In");
            }
            else
            {
                Logger.FORCE_Log("OnDataReportingClicked", "Opt Out");
            }

        }

        private void OnOptionReportIssue(object sender, RoutedEventArgs e)
        {
            ITextEditorSettings settings = textCore.TextEditorSettings;
            if (settings.CollectFeedback == false)
            {
                ReportIssueFeedbackDialog reportIssueFeedback = new ReportIssueFeedbackDialog();
                if (null != Application.Current)
                    reportIssueFeedback.Owner = Application.Current.MainWindow;
                reportIssueFeedback.ShowDialog();
                return;
            }

            ReportIssueFeedback reportIssueWindow = new ReportIssueFeedback();
            if (null != Application.Current)
                reportIssueWindow.Owner = Application.Current.MainWindow;
            reportIssueWindow.Closing += OnReportIssueWindowClosing;
            reportIssueWindow.Show();
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

            // @TODO(Ben) Replace this with the recorded XML file content.
            Logger.LogInfo("Backtrace-XML", CommandRecorder.AccumulatedLog.ToString());
            MessageBox.Show(Configurations.ReportIssueAcknowledge, "Report Issue");
        }

        private void OnOptionHelp(object sender, RoutedEventArgs e)
        {
            Configurations.HelpAndReferenceClick(null);
        }

        private void OnOptionAbout(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow(new LoggerWrapper());
            if (null != Application.Current)
                aboutWindow.Owner = Application.Current.MainWindow;

            aboutWindow.ShowDialog();
            if (false != aboutWindow.InstallNewUpdate)
            {
                // The about window was closed by clicking on the "Update" link.
                UpdateManager.Instance.QuitAndInstallUpdate();
            }
        }

        private void OnOptionRecordedFile(object sender, RoutedEventArgs e)
        {
            string temppath = System.IO.Path.GetTempPath() + Configurations.RecordFolderName;
            System.Diagnostics.Process.Start("explorer.exe", temppath);
        }

        private void OnExternalDrop(object sender, DragEventArgs e)
        {
            if (textCore.ReadOnlyState == true)
                DisplayStatusMessage(StatusTypes.Error, Configurations.EditingError, 3);

            // The drag was originated from within the control.
            if (textCore.InternalDragSourceExists != false)
                return;

            // If the mouse is captured within "TextEditorControl",
            // then this event does not matter, simply ignore it.
            if (false != this.mouseCursorCaptured)
                return;

            e.Handled = true;
            string dragData = e.Data.GetData(DataFormats.StringFormat) as string;
            if (string.IsNullOrEmpty(dragData))
                return; // Drag source was not a string.

            IScriptObject activeScript = Solution.Current.ActiveScript;
            CharPosition dropPosition = activeScript.CreateCharPosition();
            dropPosition.SetScreenPosition(GetRelativeCanvasPosition(e));

            System.Drawing.Point cursor = dropPosition.GetCharacterPosition();
            textCore.SetCursorPosition(cursor.X, cursor.Y);
            textCore.InsertText(dragData.Replace("\r\n", "\n"));

            textCanvas.BreakpointsUpdated();
            UpdateScriptDisplay(Solution.Current.ActiveScript);
        }

        private void OnExternalDragOver(object sender, DragEventArgs e)
        {
            // The drag was originated from external source (internal
            // drag source is already handled by OnDragOver itself).
            // 
            if (textCore.InternalDragSourceExists == false)
                this.OnDragOver(sender, e);
        }

        private void OnInfoTimerTick(object sender, EventArgs e)
        {
            HideStatusMessage();

            if (null != statusInfoTimer)
                statusInfoTimer.Stop();
        }

        private void OnScriptParseCompleted(object sender, EventArgs e)
        {
            UpdateScriptDisplay(Solution.Current.ActiveScript);
            UpdateCaretPosition();
        }

        private void OnExternalFileDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // add logic to change mouse pointer
            }
        }

        private void OnExternalFileDrop(object sender, DragEventArgs e)
        {
            if (null == TextEditorControl.HostApplication)
                return; // Nothing to do.
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            e.Handled = true;
            string[] filePaths = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (filePaths.Length > 8)
            {
                MessageBox.Show("Cannot open more than 8 files in one drop action");
                return;
            }

            int existingScriptIndex = -1;
            bool scriptFileOpened = false;
            foreach (string filePath in filePaths)
            {
                if (string.IsNullOrEmpty(filePath))
                    continue; // Nothing to do,

                if (textCore.LoadScriptFromFile(filePath))
                {
                    SetupTabInternal(filePath);
                    scriptFileOpened = true;
                }
                else
                {
                    // Even though "LoadScriptFromFile" returns false, it may be 
                    // due to the fact that the script has already been opened. 
                    // In such cases, we will be able to get the script index 
                    // given its name. If the index turns out to be valid here,
                    // then we will simply activate the corresponding tab.
                    // 
                    int index = Solution.Current.GetScriptIndexFromPath(filePath);
                    if (index >= 0)
                    {
                        existingScriptIndex = index;
                    }
                    else
                        MessageBox.Show(Configurations.UnsupportedFileType);
                }
            }

            if (existingScriptIndex >= 0)
                ScriptTabControl.ActivateTab(existingScriptIndex);

            if (scriptFileOpened)
            {
                HandleScriptActivation();
                grid.UpdateLayout();
                textCanvas.Focus();
                IScriptObject script = Solution.Current.ActiveScript;

            }
        }

        private string GetToolTipCallback(CurvyTabControl tabControl, int tabIndex)
        {
            return Solution.Current.GetScriptPathFromIndex(tabIndex);
        }

        private bool ScriptTabClosingCallback()
        {
            if (Solution.Current.ActiveScript == null)
                return true;
            ITextBuffer textBuffer = Solution.Current.ActiveScript.GetTextBuffer();
            if (null != textBuffer && (textBuffer.ScriptModified == true))
            {
                string message = "Would you like to save the file before closing?";
                MessageBoxResult savePrompt = MessageBox.Show(message, "Close File", MessageBoxButton.YesNoCancel);
                if (savePrompt == MessageBoxResult.Cancel)
                    return false;
                if (savePrompt == MessageBoxResult.Yes)
                {
                    if (!textCore.SaveScriptToFile(false))
                        return false;
                }
                return true;
            }

            return true; // If the script is unchanged, then always allow to close.
        }

        private bool StartUpTabClosingCallback()
        {
            CheckTabControlVisibility(false, false);
            return false; // return false to prevent closing.
        }

        #endregion

        #region Action Player Helper Methods

        internal void PlaybackCloseTab(int index)
        {
            ScriptTabControl.CloseTab(index);
            if (Solution.Current.ActiveScript == null)
                ShowTabControls(false);

            HandleScriptActivation();
        }

        internal void PlaybackSwitchTab(int index)
        {
            ScriptTabControl.ActivateTab(index);
            HandleScriptActivation();
        }

        internal void DisableAllTabOptions()
        {
            CloseAllItem.IsEnabled = false;
            CloseAllTabs.IsEnabled = false;
            CloseAllButThisItem.IsEnabled = false;
            CloseAllButThis.IsEnabled = false;
            CloseAllToTheRightItem.IsEnabled = false;
            CloseAllToTheRight.IsEnabled = false;
        }

        internal void UpdateTabOptions()
        {
            if (ScriptTabControl.TabCount == 0)
            {
                DisableAllTabOptions();
                return;
            }

            if (ScriptTabControl.TabCount > 1)
            {
                CloseAllButThisItem.IsEnabled = true;
                CloseAllButThis.IsEnabled = true;
                CloseAllItem.IsEnabled = true;
                CloseAllTabs.IsEnabled = true;
            }
            else
            {
                CloseAllButThisItem.IsEnabled = false;
                CloseAllButThis.IsEnabled = false;
                CloseAllItem.IsEnabled = false;
                CloseAllTabs.IsEnabled = false;
            }

            if (ScriptTabControl.CurrentTab == ScriptTabControl.TabCount - 1)
            {
                CloseAllToTheRight.IsEnabled = false;
                CloseAllToTheRightItem.IsEnabled = false;
            }
            else
            {
                CloseAllToTheRight.IsEnabled = true;
                CloseAllToTheRightItem.IsEnabled = true;
            }
        }

        internal void SetupTabInternal(string filePath)
        {
            string displayText = string.Empty;
            if (string.IsNullOrEmpty(filePath))
                displayText = "Script-" + newDocCount + ".ds";
            else
            {
                FileInfo fileInfo = new FileInfo(filePath);
                displayText = fileInfo.Name;
            }

            ScriptTabControl.InsertNewTab(displayText, scrollViewer);
            HandleScriptActivation();

            //On a new tab being opened all tab options except for close all to the right are enabled.
            //The check for close all to the right has to be enabled is done on clicking on a tab and checking the index
            UpdateTabOptions();
        }

        internal bool UpdateUiForStepNext(bool commandResult)
        {
            Logger.LogInfo("TextEditorControl-StepNext", "TextEditorControl-StepNext");

            if (commandResult == false)
                UpdateUiForStop(textCore.Stop());
            else
            {
                bool isInDebugMode = false;
                IExecutionSession session = Solution.Current.ExecutionSession;
                if (session.IsExecutionActive(ref isInDebugMode) == false)
                    return false; // No active script.
            }

            return commandResult;
        }

        internal void UpdateUiForRun(bool commandResult)
        {
            Logger.LogInfo("TextEditorControl-Run", "TextEditorControl-Run");

            if (null != Solution.Current.ActiveScript)
            {
                ITextBuffer textBuffer = Solution.Current.ActiveScript.GetTextBuffer();
                if (null != textBuffer)
                    Logger.LogInfo("TextEditorControl-Run-Script", textBuffer.GetContent());
            }
        }

        internal void UpdateUiForStop(bool commandResult)
        {
            Logger.LogInfo("TextEditorControl-Stop", "TextEditorControl-Stop");

            if (slowMotionExecutionTimer != null)
            {
                slowMotionExecutionTimer.Stop();
                slowMotionExecutionTimer = null;
            }
        }

        internal void SlowMotionPlayBack(double timeInterval)
        {
            Logger.LogInfo("TextEditorControl-SlowMotionPlayBack", "Interval " + timeInterval);

            OutputWindow.ClearOutput();

            int miliseconds = 0;
            if (timeInterval % 1 != 0)
            {
                double dec = timeInterval % 1;
                miliseconds = (int)(dec * 1000);
            }

            int seconds = (int)timeInterval;
            if (slowMotionExecutionTimer == null)
            {
                slowMotionExecutionTimer = new DispatcherTimer();
                slowMotionExecutionTimer.Tick += new EventHandler(OnSlowMotionPlayBackTick);

                if (timeInterval == 0)
                {
                    seconds = 0;
                    miliseconds = 100;
                }

                slowMotionExecutionTimer.Interval = new TimeSpan(0, 0, 0, seconds, miliseconds);
            }

            UpdateUiForStepNext(textCore.Step(RunMode.RunTo));
            if (null != slowMotionExecutionTimer)
                slowMotionExecutionTimer.Start();
        }

        #endregion

        #region Private Class Helper Methods

        private void InitializeEditor()
        {
            // This is needed for drag-and-drop to work!
            textCanvas.Background = Brushes.White;
            textCanvas.ScrollOwner = this.scrollViewer;
            textCanvas.TextEditorCore = this.textCore;

            // Button event handlers.
            ReportButton.Click += new RoutedEventHandler(OnReportButtonClick);

            // Text canvas event handlers setup.
            textCanvas.Drop += new DragEventHandler(OnExternalDrop);
            textCanvas.DragOver += new DragEventHandler(OnExternalDragOver);
            textCanvas.KeyDown += new KeyEventHandler(OnMainWindowKeyDown);
            textCanvas.GotKeyboardFocus += new KeyboardFocusChangedEventHandler(OnTextCanvasGotKeyboardFocus);

            // TextEditorCore event handlers.
            textCore.ParseCompleted += new ParseCompletedHandler(OnScriptParseCompleted);

            // Tab control event handlers setup.
            ScriptTabControl.TabChanged += new TabChangeHandler(OnTabControlTabChanged);
            ScriptTabControl.TabClosed += new TabCloseHandler(OnTabControlTabClosed);
            ScriptTabControl.RegisterToolTipCallback(GetToolTipCallback);
            ScriptTabControl.RegisterTabClosingCallback(ScriptTabClosingCallback);
            ScriptTabControl.ShowCloseButton = true;

            // Scroll viewer event handlers setup.
            scrollViewer.MouseLeftButtonDown += new MouseButtonEventHandler(OnScrollViewerLeftButtonDown);
            scrollViewer.MouseLeftButtonUp += new MouseButtonEventHandler(OnScrollViewerLeftButtonUp);
            scrollViewer.MouseRightButtonDown += new MouseButtonEventHandler(OnScrollViewerRightButtonDown);
            scrollViewer.MouseMove += new MouseEventHandler(OnScrollViewerMouseMove);

            scriptModifiedHandler = new ScriptModifiedHandler(OnTextBufferModified);
            Solution.ScriptCountChanged += new ScriptCountChangedHandler(OnScriptCountChanged);

            editorWidgetBar.Initialize(this);

            Logger.LogInfo("VersionNumber", UpdateManager.CreateInstance(new LoggerWrapper()).ProductVersion.ToString());
        }

        private void FinalizeStartUpScreen()
        {
            // The main UI thread has finished loading earlier than the start-up
            // thread, we'll go ahead and finalize the start-up screen for display.
            if (false != textEditorInitialized)
                startUpWorker.FinalizeStartUpScreen();
        }

        private delegate void DoScheduledExecutionDelegate(bool debugMode);

        private void DoScheduledExecution(bool debugMode)
        {
            if (false != debugMode)
                this.DebugScriptInternal();
            else
                this.RunScriptInternal();
        }

        private void EnumerateExtensions()
        {
            //Getting Application Startup Path
            string assemblyPath = System.AppDomain.CurrentDomain.BaseDirectory;

            string fullPath = System.IO.Path.Combine(assemblyPath, "DesignScript.Editor.Extensions.dll");
            if (File.Exists(fullPath) == false)
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                if (null == assembly)
                    return;

                assemblyPath = assembly.Location;
                assemblyPath = System.IO.Path.GetDirectoryName(assemblyPath);
                fullPath = System.IO.Path.Combine(assemblyPath, "DesignScript.Editor.Extensions.dll");
                if (File.Exists(fullPath) == false)
                    return;
            }

            // Using Reflection to get information from an Assembly:
            System.Reflection.Assembly o = System.Reflection.Assembly.LoadFrom(fullPath);
            System.Type type = o.GetType("DesignScript.Editor.Extensions.ExtensionFactory");
            object result = null;

            if (type != null)
            {
                MethodInfo methodInfo = type.GetMethod("EnumerateExtensions");
                if (methodInfo != null)
                {
                    object classInstance = Activator.CreateInstance(type, null);
                    object[] parametersArray = new object[] { this, textCore };

                    result = methodInfo.Invoke(classInstance, parametersArray);
                }
            }

            editorExtensions = result as List<EditorExtension>;
        }

        private void OnTextCanvasFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (textCanvas.TextEditorCore.CurrentTextBuffer != null)
            {
                textCanvas.TextEditorCore.FindReplace(null, null, FindOptions.FindNext);
            }
            findPane.ReplacePanel.Visibility = Visibility.Collapsed;
        }

        private void ShowStatusProgressBar(bool show)
        {
            if (null == statusProgressBar)
            {
                statusProgressBar = new RuggedProgressBar();
                statusProgressBar.SetValue(Grid.RowProperty, 1);
                statusProgressBar.SetValue(Grid.ColumnProperty, 1);
                statusProgressBar.Margin = new Thickness(0, 0, 6, 0);
                statusProgressBar.HorizontalAlignment = HorizontalAlignment.Right;
                LowerGrid.Children.Add(statusProgressBar);
            }

            statusProgressBar.ShowProgressBar(show);
        }

        #endregion

    }
}
