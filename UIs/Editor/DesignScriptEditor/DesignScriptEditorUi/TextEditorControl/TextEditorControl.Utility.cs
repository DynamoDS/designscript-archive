using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Timers;
using System.Windows;
using System.Xml.Serialization;
using System.IO;
using System.Windows.Controls.Primitives;
using DesignScript.Editor.Core;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace DesignScript.Editor
{
    using ProtoCore.CodeModel;
    using Microsoft.Win32;
    using DesignScript.Editor.Automation;
    using DesignScript.Editor.CodeGen;
    using DesignScript.Parser;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using DesignScript.Editor.StartUp;
    using ProtoCore;

    public partial class TextEditorControl : UserControl
    {
        private List<CommandAssert> asserts = null;

        #region Keyboard Input Related Utility Methods

        // Disable not CLS-compliant warnings due to "uint".
#pragma warning disable 3001, 3002

        [DllImport("user32.dll")]
        public static extern int ToUnicode(
            uint wVirtKey,
            uint wScanCode,
            byte[] lpKeyState,
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 4)] 
            StringBuilder pwszBuff,
            int cchBuff,
            uint wFlags);

        [DllImport("user32.dll")]
        public static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint uCode, uint uMapType);

#pragma warning restore 3001, 3002

        public static char GetKeyboardCharacter(KeyEventArgs e)
        {
            char ch = ' ';

            byte[] keyboardState = new byte[256];
            GetKeyboardState(keyboardState);

            uint virtualKey = ((uint)KeyInterop.VirtualKeyFromKey(e.Key));
            uint scanCode = MapVirtualKey(virtualKey, 0x0);
            StringBuilder stringBuilder = new StringBuilder(2);

            int result = ToUnicode(virtualKey, scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0);
            switch (result)
            {
                case -1:
                case 0:
                    break;
                case 1:
                default:
                    ch = stringBuilder[0];
                    break;
            }
            if (ch == '\r')
                ch = '\n';
            return ch;
        }


        char HandleCharacterKey(KeyEventArgs eventArgs)
        {
            bool shift = Keyboard.IsKeyDown(Key.RightShift) || Keyboard.IsKeyDown(Key.LeftShift);
            bool capsLock = Console.CapsLock;
            bool alt = Keyboard.IsKeyDown(Key.RightAlt) || Keyboard.IsKeyDown(Key.LeftAlt);
            bool control = Keyboard.IsKeyDown(Key.RightCtrl) || Keyboard.IsKeyDown(Key.LeftCtrl);
            char character = GetKeyboardCharacter(eventArgs);

            if (alt && control && character.Equals(' '))
                return character;
            else
                textCore.InsertText(character);

            return character;
        }

        public void ReplaceIdentifierAtCursor(string textToInsert)
        {
            ITextBuffer textBuffer = textCore.CurrentTextBuffer;
            int line = textCore.CursorPosition.Y;
            int column = textCore.CursorPosition.X;
            string lineContent = textBuffer.GetLineContent(line);

            if (string.IsNullOrEmpty(lineContent) == false)
            {
                if (column > 0 && (column <= lineContent.Length))
                {
                    // Check the preceeding character to see if it is a "dot". 
                    // If it isn't, then we may need to scan backward for a "dot"
                    // symbol, and replace the text starting after that "dot", 
                    // to the current cursor position (column).
                    // 
                    int scanner = column;
                    while (scanner > 0)
                    {
                        char preceeding = lineContent[scanner - 1];
                        if (preceeding != '@' && (preceeding != '_'))
                        {
                            if (!char.IsLetterOrDigit(preceeding))
                                break;
                        }

                        scanner = scanner - 1;
                    }

                    // We have found a symbol (e.g. a "dot", a space, or any 
                    // symbols), or we have reached the beginning of the line.
                    // Start replacing anything that is after the symbol, or 
                    // the cursor position by making a selection here.
                    textCore.SelectPartial(scanner, line, column, line);
                }
            }

            textCore.InsertText(textToInsert);
            UpdateCaretPosition();
            UpdateUiForModifiedScript(Solution.Current.ActiveScript);
        }

        #endregion

        #region Mouse Input Related Utility Methods

        private bool IsMouseInClickableRegion(object sender, MouseEventArgs e)
        {
            if ((sender is ScrollViewer) == false)
                return false;

            ScrollViewer scrollViewer = ((ScrollViewer)sender);
            System.Windows.Point screenPoint = e.GetPosition(scrollViewer);
            if (screenPoint.X < 0)
                return false;

            return true; // Mouse is in "client area" (term from ol' school Win32 time).
        }

        public System.Windows.Point GetRelativeCanvasPosition(MouseEventArgs e)
        {
            return (ScreenToCanvasPosition(e.GetPosition((UIElement)textCanvas)));
        }

        private System.Windows.Point GetRelativeCanvasPosition(DragEventArgs e)
        {
            return (ScreenToCanvasPosition(e.GetPosition((UIElement)textCanvas)));
        }

        private System.Windows.Point ScreenToCanvasPosition(System.Windows.Point screenPoint)
        {
            // We won't attempt to offset the cursor position when it falls within the 
            // line number column. The relative offset of mouse only matters when the 
            // cursor is actually within the source code region.
            // 
            double horzOffset = 0;
            if (screenPoint.X > Configurations.LineNumberColumnEnd)
                horzOffset = textCanvas.FirstVisibleColumn * Configurations.FormatFontWidth;

            screenPoint.Offset(horzOffset, textCanvas.FirstVisibleLine * Configurations.FontDisplayHeight);
            return screenPoint;
        }

        #endregion

        #region State Query Utility Methods

        bool IsPointWithinLineColumn(System.Windows.Point point)
        {
            if (point.X < Configurations.LineNumberColumnStart)
                return false;
            if (point.X >= Configurations.LineNumberColumnEnd)
                return false;
            return true;
        }

        bool IsPointWithinBreakColumn(System.Windows.Point point)
        {
            if (point.X >= 0 && (point.X < Configurations.LineNumberColumnStart))
                return true;

            return false;
        }

        bool IsPointOnInlineIconColumn(System.Windows.Point point)
        {
            return (point.X >= Configurations.InlineIconStart &&
                point.X <= (Configurations.InlineIconStart + Configurations.IconWidth));
        }

        bool IsDebuggingOrExecuting()
        {
            bool isExecuting = false;
            bool isInDebugMode = false;
            IExecutionSession session = Solution.Current.ExecutionSession;
            isExecuting = session.IsExecutionActive(ref isInDebugMode);

            return (isExecuting || isInDebugMode);
        }

        #endregion

        #region Interface Updating Utility Methods

        internal void UpdateScriptDisplay(IScriptObject script)
        {
            textCanvas.UpdateVisualForScript(script);
            scrollViewer.InvalidateScrollInfo();
        }

        internal void UpdateCaretPosition(bool ensureCursorVisible = true)
        {
            if (null == Solution.Current.ActiveScript)
                return;

            IScriptObject activeScript = Solution.Current.ActiveScript;
            CharPosition converter = activeScript.CreateCharPosition();
            System.Drawing.Point cursorPosition = textCore.CursorPosition;
            converter.SetCharacterPosition(cursorPosition);
            textCanvas.SetCursorScreenPos(converter.GetScreenPosition());
            if (ensureCursorVisible == true)
                textCanvas.EnsureCursorVisible(cursorPosition.X, cursorPosition.Y);

            // As the caret is moved around, more often than never the selection also 
            // gets changed. It would make sense to update the visual here at the same time.
            textCanvas.SetSelectionRange(textCore.SelectionStart, textCore.SelectionEnd);

            // @TODO(Salman) merge these into one label control.
            LineCol.Text = "Line: " + Convert.ToString(cursorPosition.Y + 1) + "  " + "Col: " + Convert.ToString(cursorPosition.X + 1);
        }

        internal void HandleScriptActivation()
        {

            //Check if its in playback mode, as it should be treated differently due to blocking
            if (player == null)
            {
                if (textCore.ChangeScript(ScriptTabControl.CurrentTab))
                    CheckTabControlVisibility(false);
                else
                    return;

            }
            else
            {
                CheckTabControlVisibility(false);
                TextEditorCommand command = new TextEditorCommand(TextEditorCommand.Method.ChangeScript);
                command.AppendArgument(ScriptTabControl.CurrentTab);
                textCore.PlaybackCommand(command);
            }

            // Resize canvas to fit the next script.
            //if (numSlider != null)
            //    numSlider.Visibility = Visibility.Collapsed;
            UpdateCanvasDimension();

            IExecutionSession session = Solution.Current.ExecutionSession;
            UpdateScriptDisplay(Solution.Current.ActiveScript);
            UpdateCaretPosition(false);
            textCanvas.BreakpointsUpdated();

            if (null != session)
            {
                CodeRange executionCursor = new CodeRange();
                if (session.GetExecutionCursor(ref executionCursor))
                    textCanvas.SetExecutionCursor(executionCursor);
            }

            if (null != textEditorControl.textCore.CurrentTextBuffer)
            {
                textCanvas.ScrollOwner.ScrollToVerticalOffset(
                    textEditorControl.textCore.VerticalScrollPosition);
            }
        }

        internal void UpdateCanvasDimension()
        {
            ITextBuffer textBuffer = textCore.CurrentTextBuffer;
            int totalLines = (null == textBuffer ? 0 : textBuffer.GetLineCount());
            double height = totalLines * Configurations.FontDisplayHeight;

            int widestWidth = (null == textBuffer ? 0 : textBuffer.GetWidestLineWidth(true));
            double width = ((widestWidth + 1) * Configurations.FormatFontWidth);
            width = width + Configurations.CanvasMarginLeft;
            textCanvas.SetComputedCanvasDimension(width, height);
        }

        internal int ErrorCount()
        {
            int errorcount = 0;
            List<InlineMessageItem> outputMessages = Solution.Current.GetInlineMessage();
            foreach (InlineMessageItem message in outputMessages)
            {
                if (message.Type == InlineMessageItem.OutputMessageType.Error)
                    errorcount++;
            }
            return errorcount;
        }

        public void UpdateUiForModifiedScript(IScriptObject currScript)
        {
            if (null != currScript)
            {
                ITextBuffer textBuffer = currScript.GetTextBuffer();
                if (null != textBuffer && (textBuffer.ScriptModified != false))
                {
                    // Only need to refresh display for modifications.
                    UpdateScriptDisplay(currScript);

                    // If the context content was modified, 
                    // then recalculate canvas dimension.
                    UpdateCanvasDimension();
                }
            }
        }

        public void RunScript()
        {
            UpdateUiForRun(textCore.Run());
            UpdateUiForStop(textCore.Stop());

            int errorcount = ErrorCount();
            /// Activate the Output Tab if no errors else activate Error Tab
            if (errorcount > 0)
                editorWidgetBar.ActivateWidget(EditorWidgetBar.Widget.Errors, true);
            else
                editorWidgetBar.ActivateWidget(EditorWidgetBar.Widget.Output, true);
        }

        internal void InsertWidget(EditorWidgetBar.Widget widget, UserControl userControl)
        {
            editorWidgetBar.AssociateWidget(widget, userControl);
        }

        #endregion

        #region Generic Utility Methods

        public void ReleaseThreads()
        {
            Logger.CancelProcessing();
        }

        public bool EnsureScriptsSaved()
        {
            int numberOfTabs = ScriptTabControl.TabCount;
            for (int i = 0; i < numberOfTabs; i++)
            {
                ScriptTabControl.ActivateTab(0);
                HandleScriptActivation();
                if (textEditorControl.ScriptTabClosingCallback())
                {
                    ScriptTabControl.CloseTab(0);
                    textCore.CloseScript(0);
                }
                else
                    return false;
            }

            return true;
        }

        private bool ActivateScriptByPath(string scriptPath)
        {
            int index = Solution.Current.GetScriptIndexFromPath(scriptPath);
            if (-1 == index)
            {
                // The script is not currently opened, attempt to do that.
                if (textCore.LoadScriptFromFile(scriptPath) == false)
                    return false;

                SetupTabInternal(scriptPath);
                return true;
            }

            index = Solution.Current.GetScriptIndexFromPath(scriptPath);
            if (ScriptTabControl.CurrentTab == index)
                return true;

            ScriptTabControl.ActivateTab(index);
            HandleScriptActivation();
            return true;
        }

        private bool RunScriptInternal()
        {
            if (RunDebugExecutedInProgress)
                return false;

            RunDebugExecutedInProgress = true;

            OutputWindow.ClearOutput();

            Logger.LogDebug("Text-Editor-Control-OnRunButton", "UserPressed");
            Logger.LogDebug("Text-Editor-Control-Script", textCore.CurrentTextBuffer.GetContent());

            Stopwatch sw = new Stopwatch();
            sw.Start();

            UpdateUiForRun(textCore.Run());
            UpdateUiForStop(textCore.Stop());

            int errorcount = ErrorCount();
            // Activate the Output Tab if no errors else activate Error Tab
            if (errorcount > 0)
                editorWidgetBar.ActivateWidget(EditorWidgetBar.Widget.Errors, true);
            else
                editorWidgetBar.ActivateWidget(EditorWidgetBar.Widget.Output, true);

            Logger.LogPerf("Text-Editor-Control-TextEditorControl.OnRunButton", sw.ElapsedMilliseconds + " ms");
            RunDebugExecutedInProgress = false;
            prevRunMode = RunModes.Run;
            return true;
        }

        private bool DebugScriptInternal()
        {
            if (RunDebugExecutedInProgress)
                return false;

            RunDebugExecutedInProgress = true;

            Logger.LogDebug("Text-Editor-Control-OnRunToButton", "UserPressed");
            Logger.LogDebug("Text-Editor-Control-RunDebugExecuted", textCore.CurrentTextBuffer.GetContent());


            Stopwatch sw = new Stopwatch();
            sw.Start();

            // Only clear if this is first "Run".
            if (false == textCore.ReadOnlyState)
                OutputWindow.ClearOutput();


            // Activate Watch Window tab
            if (UpdateUiForStepNext(textCore.Step(RunMode.RunTo)))
                editorWidgetBar.ActivateWidget(EditorWidgetBar.Widget.Watch, true);
            else
            {
                int errorcount = ErrorCount();
                if (errorcount > 0)
                    editorWidgetBar.ActivateWidget(EditorWidgetBar.Widget.Errors, true);
                else
                    editorWidgetBar.ActivateWidget(EditorWidgetBar.Widget.Watch, true);
            }

            Logger.LogPerf("Text-Editor-Control-TextEditorControl.RunToButton", sw.ElapsedMilliseconds + " ms");
            RunDebugExecutedInProgress = false;
            prevRunMode = RunModes.RunDebug;
            return true;
        }

        private bool OpenSolutionInternal()
        {
            string fileFilter = "DesignScript Solution " +
                "(*.dssln)|*.dssln|All Files (*.*)|*.*";

            // Displays an OpenFileDialog for user to select a source file.
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = fileFilter;
            openFileDialog.Title = "Select a DesignScript Solution";

            bool? result = openFileDialog.ShowDialog();
            if (result.HasValue == false || (result.Value == false))
                return false;

            if (Solution.CreateFromFile(openFileDialog.FileName) == null)
            {
                MessageBox.Show("Failed to open solution file!",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                return false;
            }

            return true;
        }

        private bool SaveSolutionInternal()
        {
            for (int index = 0; index < Solution.Current.ScriptCount; index++)
            {
                string scriptPath = Solution.Current.GetScriptPathFromIndex(index);
                if (!string.IsNullOrEmpty(scriptPath))
                    continue;

                MessageBox.Show(Configurations.OpenScriptNotSaved, "Save Solution",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                return false;
            }

            if (Solution.Current.SaveToFile(string.Empty) == false)
            {
                // We need a file name to save the solution as.
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.FileName = string.Empty; // Default file name
                saveFileDialog.DefaultExt = ".ds"; // Default file extension
                saveFileDialog.Filter = "DesignScript Solution (.dssln)|*.dssln";

                bool? result = saveFileDialog.ShowDialog();
                if (false == result.HasValue || (result.Value == false))
                    return false; // User cancels off the file saving.

                if (!Solution.Current.SaveToFile(saveFileDialog.FileName))
                {
                    MessageBox.Show("Failed to save solution file!",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    return false;
                }
            }

            return true;
        }

        private bool CanSetBreakpointAt(int column, int line)
        {
            IScriptObject script = Solution.Current.ActiveScript;
            ITextBuffer textBuffer = ((null == script) ? null : script.GetTextBuffer());
            if (null == textBuffer)
                return false;

            // Only non-empty lines, please.
            if (textBuffer.IsNullOrWhiteSpace(line))
                return false;

            CodeFragment fragment = null;
            if (textCore.GetFragment(column, line, out fragment) > 0)
            {
                switch (fragment.CodeType)
                {
                    case CodeFragment.Type.Comment:
                        return false;

                    // Even "None" should be okay because 
                    // empty spots are all of type "None".
                    case CodeFragment.Type.None:
                    default:
                        return true;
                }
            }

            return false;
        }

        private void UpdateTabDisplayText()
        {
            IScriptObject currScript = Solution.Current.ActiveScript;
            IParsedScript parsedScript = currScript.GetParsedScript();
            if (null != parsedScript)
            {
                string filePath = parsedScript.GetScriptPath();
                FileInfo fileInfo = new FileInfo(filePath);
                int tabIndex = ScriptTabControl.CurrentTab;
                ScriptTabControl.SetDisplayText(tabIndex, fileInfo.Name);
                currScript.GetTextBuffer().ScriptModified = false;
            }
        }

        #endregion

    }
}
