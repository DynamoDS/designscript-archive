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
using DesignScript.Editor.Core;
using System.IO;

namespace DesignScript.Editor.Automation
{
    public partial class PlaybackVisualizer : UserControl
    {
        private static PlaybackVisualizer visualizer = null;
        private Checkerboard checkerboard = null;
        private ToolTip toolTip = null;
        private PlaybackSnapshot playbackSnapshot = null;

        #region Public Class Operational Methods

        public PlaybackVisualizer()
        {
            InitializeComponent();
            PausePlayback.Click += new RoutedEventHandler(OnPausePlaybackClicked);
        }

        internal void SetCurrentFilePath(string filePath)
        {
            PlaybackScriptPath.ToolTip = filePath;
            PlaybackScriptPath.Text = "Playing back: " + System.IO.Path.GetFileName(filePath);
        }

        internal void SetEditorCommands(List<TextEditorCommand> editorCommands)
        {
            checkerboard.SetEditorCommands(editorCommands);
        }

        internal void SetCurrentCommand(TextEditorCommand command)
        {
            PlaybackProgress.Text = string.Format("Current command: {0} ({1})",
                command.MethodName.ToString(), command.CommandNumber.ToString());
            checkerboard.SetCurrentCommand(command);
        }

        #endregion

        #region Public Class Properties

        internal static PlaybackVisualizer Instance
        {
            get
            {
                if (null != PlaybackVisualizer.visualizer)
                    return PlaybackVisualizer.visualizer;

                PlaybackVisualizer.visualizer = new PlaybackVisualizer();
                PlaybackVisualizer.visualizer.Initialize();
                TextEditorControl.Instance.InsertWidget(EditorWidgetBar.Widget.Playback, visualizer);
                return PlaybackVisualizer.visualizer;
            }
        }

        #endregion

        #region Protected Overridable Methods

        protected override void OnMouseMove(MouseEventArgs e)
        {
            Point mousePosition = e.GetPosition(checkerboard);
            string toolTipText = checkerboard.ToolTipTextFromPoint(mousePosition);

            if (null == toolTipText) // Tool tip text needs to go away.
            {
                if (null != toolTip)
                    toolTip.IsOpen = false;
            }
            else if (toolTipText == string.Empty)
            {
                // There is no change in tooltip, stay where you are.
            }
            else
            {
                if (null == toolTip)
                    toolTip = new ToolTip();

                toolTip.Content = toolTipText;
                toolTip.IsOpen = true;
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            if (null != toolTip)
                toolTip.IsOpen = false;

            base.OnMouseLeave(e);
        }

        #endregion

        #region Private Class Helper Methods

        private void Initialize()
        {
            if (null != checkerboard)
                throw new InvalidOperationException("'Initialize' called twice!");

            checkerboard = new Checkerboard();
            checkerboard.SetValue(Grid.RowProperty, 2);
            PlaybackGrid.Children.Add(checkerboard);
            PlaybackGrid.UpdateLayout();
        }

        private void OnPausePlaybackClicked(object sender, RoutedEventArgs e)
        {
            if (null == playbackSnapshot)
            {
                ITextEditorCore editorCore = TextEditorControl.Instance.TextCore;
                playbackSnapshot = new PlaybackSnapshot();
                playbackSnapshot.Owner = Application.Current.MainWindow;
                playbackSnapshot.Initialize(editorCore.GetAssertableProperties());
            }

            if (playbackSnapshot.Visibility != Visibility.Visible)
            {
                playbackSnapshot.RefreshProperties();
                playbackSnapshot.Show();
                TextEditorControl.Instance.PauseActionPlayback(true);
            }
        }

        #endregion
    }

    class Checkerboard : FrameworkElement
    {
        // Checkerboard settings.
        private const double margin = 2;
        private const double dimension = 16;
        private int boxesPerRow = 0;
        private int mouseOverIndex = -1;

        // Visual related data members...
        private DrawingVisual foregroundVisual = null;
        private DrawingVisual backgroundVisual = null;
        private VisualCollection childVisuals = null;
        private Brush passed = null, failed = null;
        private Pen borderLine = null;

        private TextEditorCommand currentCommand = null;
        private List<int> commandIndices = null;
        private List<TextEditorCommand> editorCommands = null;

        #region Public Class Operational Methods

        internal Checkerboard()
        {
            foregroundVisual = new DrawingVisual();
            backgroundVisual = new DrawingVisual();

            passed = new SolidColorBrush(UIColors.PlayBackPassedColor);
            failed = new SolidColorBrush(UIColors.PlayBackFailedColor);
            borderLine = new Pen(new SolidColorBrush(UIColors.PlayBackBorderLineColor), 1);

            childVisuals = new VisualCollection(this);
            childVisuals.Add(backgroundVisual);
            childVisuals.Add(foregroundVisual);

            LayoutUpdated += new EventHandler(OnCheckerboardLayoutUpdated);
        }

        internal void SetEditorCommands(List<TextEditorCommand> editorCommands)
        {
            currentCommand = null;
            this.editorCommands = editorCommands;
            ComposeBackgroundVisual();
            ComposeForegroundVisual();
        }

        internal void SetCurrentCommand(TextEditorCommand command)
        {
            currentCommand = command;
            ComposeBackgroundVisual();
        }

        internal string ToolTipTextFromPoint(System.Windows.Point cursor)
        {
            int x = (int)Math.Floor(cursor.X / (dimension + margin));
            int y = (int)Math.Floor(cursor.Y / (dimension + margin));
            int index = (y * boxesPerRow) + x;
            if (null == commandIndices || (index == mouseOverIndex))
                return string.Empty; // No change, skip all processing!

            mouseOverIndex = index;
            if (index < 0 || (index >= commandIndices.Count))
                return null; // If there's a tool-tip, hide it.

            // If a command has the associated assertions, then they will all 
            // show up right after the command itself. For an example: 
            // 
            //   [3][4][5][5][5][5][6][7]...
            // 
            // If the mouse hovers over the 3rd '5' for example, 'index' would 
            // have been '4'. Here we need to back trace to the first '5', 
            // which would have at position '2'. In such case, 'startIndex' 
            // will point to '2' and the 'index' will be '5'.
            // 
            int startIndex = index;
            while (startIndex > 0)
            {
                if (commandIndices[startIndex - 1] != commandIndices[index])
                    break;
                startIndex = startIndex - 1;
            }

            int globalIndex = commandIndices[index];
            if (globalIndex < 0 || (globalIndex >= editorCommands.Count))
                return null; // If there's a tool-tip, hide it.

            string toolTipText = string.Empty;
            if (startIndex == index) // This is a command node.
            {
                TextEditorCommand command = editorCommands[globalIndex];
                string arguments = string.Empty;

                int currentArgument = 1;
                if (null != command.Arguments)
                {
                    foreach (object obj in command.Arguments)
                    {
                        arguments += string.Format("Argument {0}: {1}\n",
                            currentArgument++, obj.ToString());
                    }
                }

                toolTipText = string.Format("Command index: {0}\nCommand name: {1}\n{2}",
                    command.CommandNumber, command.MethodName.ToString(), arguments);
            }
            else // This appears to be an assert node.
            {
                int assertIndex = index - startIndex - 1;
                TextEditorCommand command = editorCommands[globalIndex];

                List<CommandAssert> asserts = command.Asserts;
                if (assertIndex < 0 || (assertIndex >= asserts.Count)) // Invalid index?!
                {
                    toolTipText = string.Format("Houston, we have a problem! " +
                        "The index of assertion '{0}' for command '{1}' is invalid!",
                        assertIndex, command.CommandNumber);
                }
                else
                {
                    CommandAssert assertion = asserts[assertIndex];
                    toolTipText = string.Format("Assert property: {0} ({1})\n" +
                        "Status: {2}\nExpected value: {3}", assertion.PropertyName,
                        assertion.AssertNumber, assertion.Passed ? "Passed" : "Failed",
                        assertion.PropertyValue);
                }
            }

            return toolTipText;
        }

        #endregion

        #region Protected Class Overridable Methods

        protected override int VisualChildrenCount
        {
            get { return childVisuals.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return childVisuals[index];
        }

        #endregion

        #region Private Class Helper Methods

        private void OnCheckerboardLayoutUpdated(object sender, EventArgs e)
        {
            ComposeBackgroundVisual();
            ComposeForegroundVisual();
        }

        private void ComposeForegroundVisual()
        {
            if (null == editorCommands || (editorCommands.Count <= 0))
                return; // Nothing to render here...

            // The indices of the current rendered box. The indices are for use in 
            // 'editorCommands' so they are relative to the beginning of the list.
            if (null == commandIndices)
                commandIndices = new List<int>();

            int commandIndex = 0;
            commandIndices.Clear();

            using (DrawingContext context = foregroundVisual.RenderOpen())
            {
                double actualWidth = Math.Floor(this.ActualWidth);
                Rect clippingRect = new Rect(0, 0, this.ActualWidth, this.ActualHeight);
                context.PushClip(new RectangleGeometry(clippingRect));

                // The number of boxes we can fit in a row.
                boxesPerRow = (int)Math.Floor(actualWidth / (dimension + margin));

                GuidelineSet guidelines = new GuidelineSet();
                double halfPenWidth = borderLine.Thickness * 0.5;
                guidelines.GuidelinesX.Add(dimension + halfPenWidth);
                guidelines.GuidelinesY.Add(dimension + halfPenWidth);
                context.PushGuidelineSet(guidelines);

                Rect boxedRegion = new Rect(margin, margin, dimension, dimension);
                foreach (TextEditorCommand command in editorCommands)
                {
                    context.DrawRectangle(null, borderLine, boxedRegion);
                    boxedRegion = AdvanceToNextBlock(boxedRegion, actualWidth);

                    commandIndices.Add(commandIndex);
                    if (null != command.Asserts && (command.Asserts.Count > 0))
                    {
                        foreach (CommandAssert assert in command.Asserts)
                        {
                            commandIndices.Add(commandIndex);
                            context.DrawRectangle(null, borderLine, boxedRegion);
                            context.DrawLine(borderLine, boxedRegion.TopLeft, boxedRegion.BottomRight);
                            boxedRegion = AdvanceToNextBlock(boxedRegion, actualWidth);
                        }
                    }

                    commandIndex = commandIndex + 1;
                }

                context.Pop();
            }
        }

        private void ComposeBackgroundVisual()
        {
            if (null == editorCommands || (editorCommands.Count <= 0))
                return; // Nothing to render here...

            using (DrawingContext context = backgroundVisual.RenderOpen())
            {
                // Returning here (only after RenderOpen) will cause the 
                // "backgroundVisual" to be cleared. Returning any earlier will 
                // retain the old content and that may not be what we want 
                // (e.g. when switching from one XML to another, we want the 
                // background cleared).
                // 
                if (null == currentCommand)
                    return;

                double x = margin, y = margin;
                double actualWidth = Math.Floor(this.ActualWidth);
                Rect clippingRect = new Rect(0, 0, this.ActualWidth, this.ActualHeight);
                context.DrawRectangle(Brushes.White, null, clippingRect);
                context.PushClip(new RectangleGeometry(clippingRect));

                Rect boxedRegion = new Rect(x, y, dimension, dimension);
                foreach (TextEditorCommand command in editorCommands)
                {
                    context.DrawRectangle(passed, null, boxedRegion);
                    boxedRegion = AdvanceToNextBlock(boxedRegion, actualWidth);

                    if (null != command.Asserts && (command.Asserts.Count > 0))
                    {
                        foreach (CommandAssert assert in command.Asserts)
                        {
                            Brush brush = (assert.Passed ? passed : failed);
                            context.DrawRectangle(brush, null, boxedRegion);
                            boxedRegion = AdvanceToNextBlock(boxedRegion, actualWidth);
                        }
                    }

                    if (command == currentCommand)
                        break; // Done drawing for the current command.
                }
            }
        }

        private Rect AdvanceToNextBlock(Rect boxedRegion, double actualWidth)
        {
            boxedRegion.X += dimension + margin;
            if (boxedRegion.Right >= actualWidth)
            {
                boxedRegion.X = margin;
                boxedRegion.Y += dimension + margin;
            }

            return boxedRegion;
        }

        #endregion
    }
}
