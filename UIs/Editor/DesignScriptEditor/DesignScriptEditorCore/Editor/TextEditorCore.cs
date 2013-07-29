
// #define EXPERIMENTAL_AUTO_COMPLETE_ENGINE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using ProtoCore.AST;
using DesignScript.Parser;

namespace DesignScript.Editor.Core
{
    using ProtoCore.CodeModel;
    using System.Diagnostics;
    using System.ComponentModel;
    using System.Windows.Threading;
    using System.Xml.Serialization;
    using DesignScript.Editor;
    using DesignScript.Editor.AutoComplete;

    public delegate void ParseCompletedHandler(
        object sender, EventArgs e);


    public class StartUpData
    {
        private List<RecentFile> recentFiles = null;

        public StartUpData(List<RecentFile> recentFiles)
        {
            this.recentFiles = recentFiles;
        }

        public List<RecentFile> RecentFiles
        {
            get { return recentFiles; }
        }

        internal void AddToRecentFileList(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            RecentFile recentFile = new RecentFile(filePath);
            RecentFile fileToBeRemoved = recentFiles.Find(item => item == recentFile);

            recentFiles.Remove(fileToBeRemoved);
            recentFiles.Add(recentFile);
            if (recentFiles.Count > 10)
                recentFiles.RemoveAt(0);
            RecentFile.Serialize(RecentFiles);
        }

        public void RemoveFromRecentFileList(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            RecentFile recentFile = new RecentFile(filePath);
            RecentFile fileToBeRemoved = recentFiles.Find(item => item == recentFile);

            recentFiles.Remove(fileToBeRemoved);
            RecentFile.Serialize(RecentFiles);
        }
    }

    // Made public for serialization.
    public class EditorSettingsData : ITextEditorSettings
    {
        #region Public Class Operational Methods

        public static bool Serialize(string filePath, ITextEditorSettings settingsData)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(EditorSettingsData));
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    serializer.Serialize(fileStream, settingsData);
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static ITextEditorSettings Deserialize(string filePath)
        {
            EditorSettingsData settingsInstance = null;
            if (!string.IsNullOrEmpty(filePath) && (File.Exists(filePath) != false))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(EditorSettingsData));
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        settingsInstance = serializer.Deserialize(fileStream) as EditorSettingsData;
                    }
                }
                catch (Exception)
                {
                }
            }

            if (null == settingsInstance) // Default settings.
                settingsInstance = new EditorSettingsData();

            return settingsInstance;
        }

        #endregion

        #region ITextEditorSettings Members

        public int MaxArrayDisplaySize { get; set; }
        public int MaxOutputDepth { get; set; }
        public int FontMultiplier { get; set; }

        public bool DisplayOutput { get; set; }
        public bool EnableSmartFormatting { get; set; }
        public bool EnableNumericSlider { get; set; }
        public bool CollectFeedback { get; set; }
        public string IncludePath { get; set; }

        public void ToggleDisplayOutput()
        {
            this.DisplayOutput = !this.DisplayOutput;
        }

        public void ToggleSmartFormatting()
        {
            this.EnableSmartFormatting = !this.EnableSmartFormatting;
        }

        public void ToggleNumericSlider()
        {
            this.EnableNumericSlider = !this.EnableNumericSlider;
        }

        #endregion

        #region Private Class Helper Methods

        private EditorSettingsData()
        {
            // Default text editor settings go here...
            this.MaxArrayDisplaySize = 7;
            this.MaxOutputDepth = 4;
            this.FontMultiplier = 0;
            this.DisplayOutput = false;
            this.EnableSmartFormatting = true;
            this.CollectFeedback = false;
        }

        #endregion
    }

    internal partial class TextEditorCore : ITextEditorCore
    {
        private ScriptState scriptState = null;
        private ITextEditorSettings editorSettings = null;
        private CodeFragmentManager codeFragmentManager = null;
        private CodeFragment[] crossHighlightArray = null;
        private DispatcherTimer backgroundParseTimer = null;
        private BackgroundWorker backgroundParser = null;
        private ICommandRecorder commandRecorder = null;
        private IAssertableProperties assertableProperties = null;
        private IAutoCompleteEngine autoCompleteEngine = null;
        private StartUpData startUpData = null;

        private bool mouseButtonDown = false;
        private bool internalDragSource = false;
        private TextEditorCommand.Modifier overrideModifier;

        private int pageSize = 64;
        private bool allowRegularCommands = true;

        #region ITextEditorCore: Core Command Related Interface Methods

        public bool CreateNewScript()
        {
            TextEditorCommand command = new TextEditorCommand(
                TextEditorCommand.Method.CreateNewScript);

            return DoCommandInternal(command);
        }

        public bool LoadScriptFromFile(string scriptFilePath)
        {
            TextEditorCommand command = new TextEditorCommand(
                TextEditorCommand.Method.LoadScriptFromFile);

            command.AppendArgument(scriptFilePath);
            return DoCommandInternal(command);
        }

        public bool SaveScriptToFile(bool saveAs)
        {
            TextEditorCommand command = new TextEditorCommand(
                TextEditorCommand.Method.SaveScriptToFile);

            command.AppendArgument(saveAs);
            return DoCommandInternal(command);
        }

        public bool CloseScript(int index)
        {
            TextEditorCommand command = new TextEditorCommand(
                TextEditorCommand.Method.CloseScript);

            command.AppendArgument(index);
            return DoCommandInternal(command);
        }

        public bool ChangeScript(int index)
        {
            TextEditorCommand command = new TextEditorCommand(
                TextEditorCommand.Method.ChangeScript);

            command.AppendArgument(index);
            return DoCommandInternal(command);
        }

        public bool SetCursorPosition(int column, int line)
        {
            TextEditorCommand command = new TextEditorCommand(
                TextEditorCommand.Method.SetCursorPosition);

            command.AppendArgument(column);
            command.AppendArgument(line);
            return DoCommandInternal(command);
        }

        public bool SetMouseDownPosition(int column, int line, MouseEventArgs eventArgs)
        {
            TextEditorCommand command = new TextEditorCommand(
                TextEditorCommand.Method.SetMouseDownPosition);

            command.AppendArgument(column);
            command.AppendArgument(line);
            AddMouseModifiers(command, eventArgs);
            return DoCommandInternal(command);
        }

        public bool SetMouseUpPosition(int column, int line, MouseEventArgs eventArgs)
        {
            TextEditorCommand command = new TextEditorCommand(
                TextEditorCommand.Method.SetMouseUpPosition);

            command.AppendArgument(column);
            command.AppendArgument(line);
            AddMouseModifiers(command, eventArgs);
            return DoCommandInternal(command);
        }

        public bool SetMouseMovePosition(int column, int line, MouseEventArgs eventArgs)
        {
            // @TODO(Ben) Don't create this over and over again, just need one for mouse move.
            TextEditorCommand command = new TextEditorCommand(
                TextEditorCommand.Method.SetMouseMovePosition);

            command.AppendArgument(column);
            command.AppendArgument(line);
            AddMouseModifiers(command, eventArgs);
            return DoCommandInternal(command);
        }

        public bool ClearDragDropState()
        {
            TextEditorCommand command = new TextEditorCommand(
                TextEditorCommand.Method.ClearDragDropState);

            return DoCommandInternal(command);
        }

        public bool SelectFragment(int column, int line)
        {
            TextEditorCommand command = new TextEditorCommand(
                TextEditorCommand.Method.SelectFragment);

            command.AppendArgument(column);
            command.AppendArgument(line);
            return DoCommandInternal(command);
        }

        public bool SelectLines(int lineIndex, int lineOffset)
        {
            TextEditorCommand command = new TextEditorCommand(
                TextEditorCommand.Method.SelectLines);

            command.AppendArgument(lineIndex);
            command.AppendArgument(lineOffset);
            return DoCommandInternal(command);
        }

        public bool CommentLines(bool commentText)
        {
            TextEditorCommand command = new TextEditorCommand(
                TextEditorCommand.Method.CommentLines);

            command.AppendArgument(commentText);
            return DoCommandInternal(command);
        }

        public bool DeleteCurrentLine()
        {
            TextEditorCommand command = new TextEditorCommand(
                TextEditorCommand.Method.DeleteCurrentLine);

            return DoCommandInternal(command);
        }

        public bool InsertText(char textContent)
        {

            TextEditorCommand command = new TextEditorCommand(
                TextEditorCommand.Method.InsertText);

            command.AppendArgument(textContent);
            return DoCommandInternal(command);
        }

        public bool InsertText(string textContent)
        {
            if (string.IsNullOrEmpty(textContent))
                return false;

            TextEditorCommand command = new TextEditorCommand(
                TextEditorCommand.Method.InsertText);

            if (textContent.Length == 1)
                command.AppendArgument((char)textContent[0]);
            else
                command.AppendArgument(textContent);

            return DoCommandInternal(command);
        }

        public bool SetPageSize(int pageSize)
        {
            TextEditorCommand command = new TextEditorCommand(
                TextEditorCommand.Method.SetPageSize);

            command.AppendArgument(pageSize);
            return DoCommandInternal(command);
        }

        public bool DoNavigation(System.Windows.Input.Key key)
        {
            TextEditorCommand command = new TextEditorCommand(
                TextEditorCommand.Method.DoNavigation);

            command.AppendArgument(key);
            return DoCommandInternal(command);
        }

        public bool DoControlCharacter(System.Windows.Input.Key key)
        {
            TextEditorCommand command = new TextEditorCommand(
                TextEditorCommand.Method.DoControlCharacter);

            command.AppendArgument(key);
            return DoCommandInternal(command);
        }

        public bool DoCopyText(bool cutSelection)
        {
            TextEditorCommand command = new TextEditorCommand(
                TextEditorCommand.Method.DoCopyText);

            command.AppendArgument(cutSelection);
            return DoCommandInternal(command);
        }

        public bool DoPasteText()
        {
            TextEditorCommand command = new TextEditorCommand(
                TextEditorCommand.Method.DoPasteText);

            return DoCommandInternal(command);
        }

        public bool SelectAllText()
        {
            TextEditorCommand command = new TextEditorCommand(
                TextEditorCommand.Method.SelectAllText);

            return DoCommandInternal(command);
        }

        public bool SelectPartial(int startColumn, int startLine, int endColumn, int endLine)
        {
            TextEditorCommand command = new TextEditorCommand(
                TextEditorCommand.Method.SelectPartial);

            command.AppendArgument(startColumn);
            command.AppendArgument(startLine);
            command.AppendArgument(endColumn);
            command.AppendArgument(endLine);
            return DoCommandInternal(command);
        }

        public bool MoveSelectedText(int column, int line, bool copyText)
        {
            TextEditorCommand command = new TextEditorCommand(
                TextEditorCommand.Method.MoveSelectedText);

            command.AppendArgument(column);
            command.AppendArgument(line);
            command.AppendArgument(copyText);
            return DoCommandInternal(command);
        }

        public bool UndoEditing()
        {
            TextEditorCommand command = new TextEditorCommand(
                TextEditorCommand.Method.UndoEditing);

            return DoCommandInternal(command);
        }

        public bool RedoEditing()
        {
            TextEditorCommand command = new TextEditorCommand(
                TextEditorCommand.Method.RedoEditing);

            return DoCommandInternal(command);
        }

        public bool FindReplace(string textToSearch, string replacement, FindOptions option)
        {
            TextEditorCommand command = new TextEditorCommand(
                TextEditorCommand.Method.FindReplace);

            command.AppendArgument(textToSearch);
            command.AppendArgument(replacement);
            command.AppendArgument((int)option);

            return DoCommandInternal(command);
        }

        public bool Run()
        {
            TextEditorCommand command = new TextEditorCommand(
                TextEditorCommand.Method.Run);

            return DoCommandInternal(command);
        }

        public bool Step(RunMode runMode)
        {
            TextEditorCommand command = new TextEditorCommand(
                TextEditorCommand.Method.Step);

            command.AppendArgument((int)runMode);
            return DoCommandInternal(command);
        }

        public bool Stop()
        {
            TextEditorCommand command = new TextEditorCommand(
                TextEditorCommand.Method.Stop);

            return DoCommandInternal(command);
        }

        public bool ToggleBreakpoint()
        {
            TextEditorCommand command = new TextEditorCommand(
                TextEditorCommand.Method.ToggleBreakpoint);

            return DoCommandInternal(command);
        }

        public bool FormatDocument()
        {
            TextEditorCommand command = new TextEditorCommand(
                TextEditorCommand.Method.FormatDocument);

            return DoCommandInternal(command);
        }

        #endregion

        #region ITextEditorCore: Action playback/recording related methods

        public bool PlaybackCommand(TextEditorCommand command)
        {
            return DispatchCommandToHandler(command);
        }

        public void SetCommandRecorder(ICommandRecorder recorder)
        {
            commandRecorder = recorder;
        }

        public IAssertableProperties GetAssertableProperties()
        {
            if (null == assertableProperties)
                assertableProperties = new AssertableProperties(this);

            return assertableProperties;
        }

        #endregion

        #region ITextEditorCore: Public Class Operational Methods

        internal static TextEditorCore CreateTemporary()
        {
            // This is meant to be used in unit test cases (no UI),so the 
            // "singleton" instance should be specific to one test case. 
            // Here we need to ensure the singleton is created, but the 
            // previous one (from a previous test case) is destroyed (with 
            // a call to "TextEditorCore.InvalidateInstance()" method).
            if (null != TextEditorCore.Instance)
                throw new InvalidOperationException("'InvalidateInstance' isn't called!");

            TextEditorCore.Instance = new TextEditorCore(null);
            return TextEditorCore.Instance;
        }

        internal static void InvalidateInstance()
        {
            TextEditorCore.Instance = null;
        }

        internal static TextEditorCore CreateSingleton(ExecutionStateChangedHandler handler)
        {
            if (null != TextEditorCore.Instance) // Do you really get what "singleton" means?
                throw new InvalidOperationException("'TextEditorCore.CreateSingleton' called more than once!");

            if (null == handler)
                throw new ArgumentNullException("handler");

            TextEditorCore.Instance = new TextEditorCore(handler);
            return TextEditorCore.Instance;
        }

        public void Shutdown()
        {
            if (null != editorSettings)
            {
                string settingsFilePath = GetSettingsFilePath();
                EditorSettingsData.Serialize(settingsFilePath, editorSettings);
                editorSettings = null;
            }
        }

        public bool IsPointInSelection(int column, int line)
        {
            if (this.HasSelection == false)
                return false; // Selection? What selection?
            if (false != internalDragSource)
                return true; // Always show arrow when dragging.

            System.Drawing.Point start = scriptState.selectionStart.GetCharacterPosition();
            System.Drawing.Point end = scriptState.selectionEnd.GetCharacterPosition();
            return TextEditorCore.IsPointInRegion(start, end, new System.Drawing.Point(column, line));
        }

        public int GetFragment(int column, int line, out CodeFragment fragment)
        {
            fragment = null;
            if (null == this.codeFragmentManager)
                return 0;

            return (codeFragmentManager.GetFragment(column, line, out fragment));
        }

        public int GetFragmentForInspection(int column, int line, out CodeFragment forkFragment)
        {
            forkFragment = null;
            if (null == this.codeFragmentManager)
                return 0;

            return (codeFragmentManager.GetFragmentForInspection(column, line, out forkFragment));
        }

        public int GetPreviousFragment(int column, int line, out CodeFragment prevFragment)
        {
            prevFragment = null;
            if (null == codeFragmentManager)
                return 0;

            return (codeFragmentManager.GetPreviousFragment(column, line, out prevFragment));
        }

        public int GetNextFragment(CodeFragment curFragment, out CodeFragment nextFragment)
        {
            nextFragment = null;
            if (null == codeFragmentManager)
                return 0;

            return (codeFragmentManager.GetNextFragment(curFragment, out nextFragment));
        }

        public bool SetFragmentText(int column, int line, string text)
        {
            if (codeFragmentManager == null)
                return false;
            else return codeFragmentManager.SetFragmentText(column, line, text);
        }

        public bool GetLineFragments(int line, out List<CodeFragment> fragments)
        {
            fragments = null;
            return codeFragmentManager.GetLineFragments(line, out fragments);
        }

        public CodeFragment[] GetCrossHighlightArray()
        {
            return crossHighlightArray;
        }

        public void CheckForExternalModifications(ref List<int> scriptsToClose)
        {
            scriptsToClose.Clear();
            // Loop through all ScriptModifiedExternal to check if reload is required for each.
            int currentScriptIndex = Solution.Current.ActiveScriptIndex;
            int scriptCount = Solution.Current.ScriptCount;
            for (int i = 0; i < scriptCount; i++)
            {
                Solution.Current.ActivateScript(i);
                ScriptObject currScript = Solution.Current.ActiveScript as ScriptObject;

                if (!currScript.ScriptModifiedExternal)
                    continue;

                currScript.ScriptModifiedExternal = false;
                string scriptPath = currScript.GetParsedScript().GetScriptPath();

                if (!File.Exists(scriptPath))
                {
                    string message = currScript.GetParsedScript().GetScriptPath() + "\nno longer exists. Do you want to close it?";
                    MessageBoxResult result = MessageBox.Show(message, "DesignScript IDE", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {
                        scriptsToClose.Add(i);
                        if (i == currentScriptIndex)
                            currentScriptIndex = 0;
                    }
                    else
                    {
                        // IDE-644 Crash on file reload if existing open file renamed in explorer.
                        // 
                        // If the script is not to be closed, keep it opened but mark it as being 
                        // modified (so user gets a chance to save it later on).
                        currScript.GetTextBuffer().ScriptModified = true;
                    }

                    continue;
                }

                IDialogProvider dialogProvider = TextEditorCore.DialogProvider;
                bool reloadDialogResult = dialogProvider.ShowReloadDialog(scriptPath);
                if (reloadDialogResult)
                {
                    TextBuffer textBuffer = currScript.GetTextBuffer() as TextBuffer;
                    textBuffer.ReloadFromFile(scriptPath);
                }
            }

            Solution.Current.ActivateScript(currentScriptIndex);
        }

        #endregion

        #region ITextEditorCore: Text editor public properties

        public static IHostApplication HostApplication { get; set; }

        public static IDialogProvider DialogProvider { get; set; }

        public bool ReadOnlyState { get; set; }

        public bool EnableRegularCommands
        {
            get { return allowRegularCommands; }
            set { allowRegularCommands = value; }
        }

        public bool InternalDragSourceExists
        {
            get { return internalDragSource; }
        }

        public bool HasSelection
        {
            get
            {
                if (null == scriptState)
                    return false;

                System.Drawing.Point selStart = scriptState.selectionStart.GetCharacterPosition();
                System.Drawing.Point selEnd = scriptState.selectionEnd.GetCharacterPosition();
                return (selStart != selEnd);
            }
        }

        public string SelectionText
        {
            get { return GetSelectionText(); }
        }

        public System.Drawing.Point CursorPosition
        {
            get { return scriptState.cursorPosition.GetCharacterPosition(); }
        }

        public double HorizontalScrollPosition
        {
            get { return scriptState.horizontalScrollPosition; }
            set { scriptState.horizontalScrollPosition = value; }
        }

        public double VerticalScrollPosition
        {
            get { return scriptState.verticalScrollPosition; }
            set { scriptState.verticalScrollPosition = value; }
        }

        public System.Drawing.Point SelectionStart
        {
            get { return scriptState.selectionStart.GetCharacterPosition(); }
        }

        public System.Drawing.Point SelectionEnd
        {
            get { return scriptState.selectionEnd.GetCharacterPosition(); }
        }

        public int VisualOffset
        {
            get { return scriptState.visualOffset; }
        }

        public ITextBuffer CurrentTextBuffer
        {
            get
            {
                if (null == scriptState)
                    return null;

                return scriptState.textBuffer;
            }
        }

        public StartUpData Data
        {
            get
            {
                System.Diagnostics.Debug.Assert(null != this.startUpData);
                return this.startUpData;
            }
            set
            {
                this.startUpData = value;
            }
        }

        public ITextEditorSettings TextEditorSettings
        {
            get
            {
                if (null == editorSettings)
                {
                    string settingsFilePath = GetSettingsFilePath();
                    editorSettings = EditorSettingsData.Deserialize(settingsFilePath);
                }

                return this.editorSettings;
            }
        }

        internal static TextEditorCore Instance { get; private set; }

        internal ExecutionStateChangedHandler UiExecutionStateChangeHandler { get; private set; }

        #endregion

        #region ITextEditorCore: Public Interface Events

        public event ParseCompletedHandler ParseCompleted = null;

        #endregion

        #region Public Class Operational Methods

        internal bool IsLineCommented(int lineIndex, bool ignoreSingleLineComments)
        {
            if (null == this.codeFragmentManager)
                return false;

            CodeFragment fragment = null;
            int column = CurrentTextBuffer.GetFirstNonWhiteSpaceChar(0, lineIndex);
            if (codeFragmentManager.GetFragment(column, lineIndex, out fragment) == 0)
                return false;

            if (fragment.CodeType == CodeFragment.Type.Comment)
            {
                // We are requested to ignore lines that start with a single-line 
                // comment (i.e. "//" characters). So we need to see if the index 
                // of "//" sub-string, if any, is the same as "column". If so, 
                // then the line starts with "//" sequence and should be ignored.
                // 
                if (false != ignoreSingleLineComments)
                {
                    string lineContent = CurrentTextBuffer.GetLineContent(lineIndex);
                    if (!string.IsNullOrEmpty(lineContent))
                    {
                        int slashesIndex = lineContent.IndexOf("//");
                        if (-1 != slashesIndex && (slashesIndex == column))
                            return false;
                    }
                }

                return true;
            }

            return false; // Nope, not commented.
        }

        internal string GetSelectionText()
        {
            if (null == scriptState.textBuffer || (HasSelection == false))
                return String.Empty;

            ITextBuffer textBuffer = scriptState.textBuffer;
            return textBuffer.GetText(this.scriptState.selectionStart, this.scriptState.selectionEnd);
        }

        internal void SetOverrideModifierFlag(TextEditorCommand.Modifier flag)
        {
            overrideModifier = flag;
        }

        // This method is exposed internally for use in NUnit test cases only.
        internal void ParseScriptImmediate()
        {
            if (null == scriptState || (null == scriptState.textBuffer))
                return;
            if (scriptState.textBuffer.ParsePending == false)
                return; // Script wasn't changed.

            (scriptState.textBuffer as TextBuffer).ParsePending = false;
            IScriptObject currScript = Solution.Current.ActiveScript;
            if (null != currScript)
            {
                IParsedScript parsedScript = currScript.GetParsedScript();
                codeFragmentManager = new CodeFragmentManager(parsedScript);
                CrossHighlight();
            }
        }

        internal void UpdateExecutionCursor()
        {
            CodeRange cursor = new CodeRange();
            IExecutionSession session = Solution.Current.ExecutionSession;

            if (session.GetExecutionCursor(ref cursor))
            {
                int line = cursor.StartInclusive.LineNo - 1;
                int column = cursor.StartInclusive.CharNo - 1;
                if (line >= 0 && column >= 0)
                {
                    scriptState.cursorPosition.SetCharacterPosition(column, line, false);
                    scriptState.mouseDownPosition.X = scriptState.cursorPosition.CharacterX;
                    scriptState.mouseDownPosition.Y = scriptState.cursorPosition.CharacterY;
                }
                if (null != Solution.Current.ActiveScript)
                    UpdateVisualOffsetFromCursor();
            }
        }

        #endregion

        #region Private Class Command Handlers

        private bool DoCommandInternal(TextEditorCommand command)
        {
            if (allowRegularCommands == false)
                return true;

            if (Keyboard.IsKeyDown(Key.RightCtrl) || Keyboard.IsKeyDown(Key.LeftCtrl))
                command.Modifiers |= ((uint)TextEditorCommand.Modifier.Control);
            if (Keyboard.IsKeyDown(Key.RightShift) || Keyboard.IsKeyDown(Key.LeftShift))
                command.Modifiers |= ((uint)TextEditorCommand.Modifier.Shift);
            if (Keyboard.IsKeyDown(Key.RightAlt) || Keyboard.IsKeyDown(Key.LeftAlt))
                command.Modifiers |= ((uint)TextEditorCommand.Modifier.Alt);

            // NUnit only, in which case there's no active key presses...this simulates shift, Alt & Control
            if (command.Modifiers == (uint)TextEditorCommand.Modifier.None)
                command.Modifiers = (uint)overrideModifier;

            if (null != commandRecorder)
                commandRecorder.Record(command);

            overrideModifier = 0; // Reset modifier.
            return DispatchCommandToHandler(command);
        }

        private bool DispatchCommandToHandler(TextEditorCommand command)
        {
            Logger.LogInfo("DispatchCommandToHandler", command.MethodName.ToString());

            switch (command.MethodName)
            {
                case TextEditorCommand.Method.CreateNewScript:
                    return CreateNewScriptHandler(command);
                case TextEditorCommand.Method.LoadScriptFromFile:
                    return LoadScriptFromFileHandler(command);
                case TextEditorCommand.Method.SaveScriptToFile:
                    return SaveScriptToFileHandler(command);
                case TextEditorCommand.Method.CloseScript:
                    return CloseScriptHandler(command);
                case TextEditorCommand.Method.ChangeScript:
                    return ChangeScriptHandler(command);
                case TextEditorCommand.Method.SetCursorPosition:
                    return SetCursorPositionHandler(command);
                case TextEditorCommand.Method.SetMouseDownPosition:
                    return SetMouseDownPositionHandler(command);
                case TextEditorCommand.Method.SetMouseUpPosition:
                    return SetMouseUpPositionHandler(command);
                case TextEditorCommand.Method.SetMouseMovePosition:
                    return SetMouseMovePositionHandler(command);
                case TextEditorCommand.Method.ClearDragDropState:
                    return ClearDragDropStateHandler(command);
                case TextEditorCommand.Method.SelectFragment:
                    return SelectFragmentHandler(command);
                case TextEditorCommand.Method.SelectLines:
                    return SelectLinesHandler(command);
                case TextEditorCommand.Method.CommentLines:
                    return CommentLinesHandler(command);
                case TextEditorCommand.Method.DeleteCurrentLine:
                    return DeleteCurrentLineHandler(command);
                case TextEditorCommand.Method.InsertText:
                    return InsertTextHandler(command);
                case TextEditorCommand.Method.SetPageSize:
                    return SetPageSizeHandler(command);
                case TextEditorCommand.Method.DoNavigation:
                    return DoNavigationHandler(command);
                case TextEditorCommand.Method.DoControlCharacter:
                    return DoControlCharacterHandler(command);
                case TextEditorCommand.Method.DoCopyText:
                    return DoCopyTextHandler(command);
                case TextEditorCommand.Method.DoPasteText:
                    return DoPasteTextHandler(command);
                case TextEditorCommand.Method.SelectAllText:
                    return SelectAllTextHandler(command);
                case TextEditorCommand.Method.SelectPartial:
                    return SelectPartialHandler(command);
                case TextEditorCommand.Method.MoveSelectedText:
                    return MoveSelectedTextHandler(command);
                case TextEditorCommand.Method.UndoEditing:
                    return UndoEditingHandler(command);
                case TextEditorCommand.Method.RedoEditing:
                    return RedoEditingHandler(command);
                case TextEditorCommand.Method.FindReplace:
                    return FindReplaceHandler(command);
                case TextEditorCommand.Method.Run:
                    return RunHandler(command);
                case TextEditorCommand.Method.Step:
                    return StepHandler(command);
                case TextEditorCommand.Method.Stop:
                    return StopHandler(command);
                case TextEditorCommand.Method.ToggleBreakpoint:
                    return ToggleBreakpointHandler(command);
                case TextEditorCommand.Method.FormatDocument:
                    return FormatDocumentHandler(command);
            }

            return false;
        }

        private bool CreateNewScriptHandler(TextEditorCommand command)
        {
            return (Solution.Current.AddNewScript() != null);
        }

        private bool LoadScriptFromFileHandler(TextEditorCommand command)
        {
            string scriptFilePath = command.Arguments[0] as string;
            if (string.IsNullOrEmpty(scriptFilePath))
                return false;

            Logger.LogDebug("LoadScriptFromFileHandler", scriptFilePath);

            Stopwatch loadWatch = new Stopwatch();
            loadWatch.Start();

            if (!scriptFilePath.EndsWith(".ds"))
                return false;

            int scriptIndex = Solution.Current.GetScriptIndexFromPath(scriptFilePath);
            if (scriptIndex >= 0) // The script with this name already exists.
            {
                Data.AddToRecentFileList(scriptFilePath);
                return false;
            }

            IScriptObject script = Solution.Current.OpenScript(scriptFilePath);

            if (null == script)
                return false;

            Data.AddToRecentFileList(scriptFilePath);

            int lines = script.GetTextBuffer().GetLineCount();
            Logger.LogInfo("LoadScriptFromFileHandler-Loaded", script.GetTextBuffer().GetContent());
            Logger.LogPerf("LoadScriptFromFileHandler", lines.ToString() + " in " + loadWatch.ElapsedMilliseconds);
            return (null != script);
        }

        private bool SaveScriptToFileHandler(TextEditorCommand command)
        {
            bool saveAs = ((bool)command.Arguments[0]);
            return SaveScriptToFileInternal(saveAs);
        }

        private bool CloseScriptHandler(TextEditorCommand command)
        {
            int index = ((int)command.Arguments[0]);
            return Solution.Current.CloseScript(index);
        }

        private bool ChangeScriptHandler(TextEditorCommand command)
        {
            int index = ((int)command.Arguments[0]);
            if (Solution.Current.ActivateScript(index) == false)
            {
                scriptState = null;
                return false;
            }

            string filePath = Solution.Current.ActiveScript.GetParsedScript().GetScriptPath();

            if (!filePath.EndsWith(".ds") && !string.IsNullOrWhiteSpace(filePath))
                return false;

            ScriptObject newScript = Solution.Current.ActiveScript as ScriptObject;
            scriptState = newScript.States;
            mouseButtonDown = internalDragSource = false;

            // If we do have a background parser around, ask it to parse the new 
            // script immediately. Otherwise, simply set the "ParsePending" to 
            // "true" and signal the background parsing in a short while later 
            // (which will result in backgroundParser being created anyway).
            //
            TextBuffer textBuffer = scriptState.textBuffer as TextBuffer;
            if (null == backgroundParser)
                textBuffer.ParsePending = true;
            else if (backgroundParser.IsBusy == false)
                backgroundParser.RunWorkerAsync(textBuffer.GetStream());
            else
            {
                // There is a background parser and it is busy.
                // Fixing IDE-668 Parser failure when "Close All" or "Close All But This" in IDE
                textBuffer.ParsePending = true;
            }

            return true;
        }


        private bool SetCursorPositionHandler(TextEditorCommand command)
        {
            int column = ((int)command.Arguments[0]);
            int line = ((int)command.Arguments[1]);

            // Allows cursor position to validate "line"
            scriptState.cursorPosition.SetCharacterPosition(column, line);
            line = scriptState.cursorPosition.GetCharacterPosition().Y;
            int characters = scriptState.textBuffer.GetCharacterCount(line, false, false);
            if (column > characters)
                column = characters;

            scriptState.cursorPosition.SetCharacterPosition(column, line);
            if (command.IsShiftKeyDown != false)
                MakeSelectionToCursor();
            else
            {
                System.Drawing.Point cursor = scriptState.cursorPosition.GetCharacterPosition();
                scriptState.selectionStart.SetCharacterPosition(cursor);
                scriptState.selectionEnd.SetCharacterPosition(cursor);
                scriptState.mouseDownPosition = cursor;
            }

            CharPosition converter = Solution.Current.ActiveScript.CreateCharPosition();
            scriptState.visualOffset = converter.CharToVisualOffset(line, column);
            return true;
        }

        private bool SetMouseDownPositionHandler(TextEditorCommand command)
        {
            mouseButtonDown = true;
            int column = ((int)command.Arguments[0]);
            int line = ((int)command.Arguments[1]);

            // Allows cursor position to validate "line"
            scriptState.cursorPosition.SetCharacterPosition(column, line, false);
            line = scriptState.cursorPosition.GetCharacterPosition().Y;
            int characters = scriptState.textBuffer.GetCharacterCount(line, false, false);

            if (column > characters)
                column = characters;

            scriptState.cursorPosition.SetCharacterPosition(column, line, false);
            UpdateVisualOffsetFromCursor();

            if (IsPointInSelection(column, line))
            {
                // If the mouse is clicking within the selection region, 
                // then do nothing to alter (clear) the current selection.
                scriptState.mouseDownPosition.X = column;
                scriptState.mouseDownPosition.Y = line;
                internalDragSource = true;
            }
            else
            {
                if (command.IsShiftKeyDown)
                    MakeSelectionToCursor();
                else
                {
                    System.Drawing.Point cursor = scriptState.cursorPosition.GetCharacterPosition();
                    scriptState.selectionStart.SetCharacterPosition(cursor);
                    scriptState.selectionEnd.SetCharacterPosition(cursor);
                    scriptState.mouseDownPosition.X = column;
                    scriptState.mouseDownPosition.Y = line;
                }
            }

            return true;
        }

        private bool SetMouseUpPositionHandler(TextEditorCommand command)
        {
            if (false != internalDragSource)
            {
                System.Drawing.Point cursor = scriptState.cursorPosition.GetCharacterPosition();
                scriptState.selectionStart.SetCharacterPosition(cursor);
                scriptState.selectionEnd.SetCharacterPosition(cursor);
            }

            mouseButtonDown = false;
            internalDragSource = false;
            CrossHighlight();
            UpdateVisualOffsetFromCursor();
            return true;
        }

        private bool SetMouseMovePositionHandler(TextEditorCommand command)
        {
            // If the mouse is not pressed...
            if (false == mouseButtonDown)
                return false; // ... bail out.

            int column = ((int)command.Arguments[0]);
            int line = ((int)command.Arguments[1]);

            scriptState.cursorPosition.SetCharacterPosition(column, line, false);

            // If we were not clicking in the selection region, then do the 
            // selection highlighting, otherwise keep the selection as we are
            // getting ready for a drag-and-drop operation.
            // 
            if (false == internalDragSource)
                MakeSelectionToCursor();
            return true;
        }

        private bool ClearDragDropStateHandler(TextEditorCommand command)
        {
            internalDragSource = false;
            mouseButtonDown = false;
            return true;
        }

        private bool SelectFragmentHandler(TextEditorCommand command)
        {
            if (null == codeFragmentManager)
                return false;

            int column = ((int)command.Arguments[0]);
            int line = ((int)command.Arguments[1]);
            if (!codeFragmentManager.GetFragmentRegion(column, line,
                scriptState.selectionStart, scriptState.selectionEnd))
            {
                if (!SelectFragmentInternal(column, line,
                    scriptState.selectionStart,
                    scriptState.selectionEnd))
                {
                    return false;
                }
            }

            // Cursor position is to be set at the selection end.
            scriptState.cursorPosition.SetCharacterPosition(
                scriptState.selectionEnd.CharacterX,
                scriptState.selectionEnd.CharacterY);

            UpdateVisualOffsetFromCursor();
            return true;
        }

        private bool SelectLinesHandler(TextEditorCommand command)
        {
            int lineIndex = ((int)command.Arguments[0]);
            int lineOffset = ((int)command.Arguments[1]);
            int lineCount = scriptState.textBuffer.GetLineCount();

            int startLine = 0, endLine = 0;
            if (lineOffset < 0)
            {
                startLine = lineIndex + lineOffset;
                endLine = lineIndex;
            }
            else
            {
                startLine = lineIndex;
                endLine = startLine + lineOffset;
            }

            if (startLine < 0)
                startLine = 0;
            else if (startLine >= lineCount)
                startLine = lineCount - 1;
            if (endLine < 0)
                endLine = 0;
            else if (endLine >= lineCount)
                endLine = lineCount - 1;

            int characters = scriptState.textBuffer.GetCharacterCount(endLine, false, true);
            scriptState.selectionStart.SetCharacterPosition(0, startLine, true);
            scriptState.selectionEnd.SetCharacterPosition(characters, endLine, true);

            if (lineOffset < 0)
                scriptState.cursorPosition.SetCharacterPosition(scriptState.selectionStart.GetCharacterPosition());
            else if (endLine == lineCount - 1)
                scriptState.cursorPosition.SetCharacterPosition(scriptState.selectionEnd.GetCharacterPosition());
            else
                scriptState.cursorPosition.SetCharacterPosition(0, endLine + 1, true);

            UpdateVisualOffsetFromCursor();
            return true;
        }

        private bool CommentLinesHandler(TextEditorCommand command)
        {
            bool commentText = (bool)command.Arguments[0];
            if (HasSelection)
            {
                int lineLength = scriptState.textBuffer.GetCharacterCount(SelectionEnd.Y, false, true);
                scriptState.selectionStart.SetCharacterPosition(0, SelectionStart.Y);
                scriptState.selectionEnd.SetCharacterPosition(lineLength, SelectionEnd.Y, true);
            }
            else
            {
                int lineLength = scriptState.textBuffer.GetCharacterCount(CursorPosition.Y, false, true);
                scriptState.selectionStart.SetCharacterPosition(0, CursorPosition.Y);
                scriptState.selectionEnd.SetCharacterPosition(lineLength, CursorPosition.Y, true);
            }

            StringBuilder textToReplace = new StringBuilder(null);
            string comment = "//";

            for (int i = SelectionStart.Y; i <= SelectionEnd.Y; i++)
            {
                string lineContent = scriptState.textBuffer.GetLineContent(i);
                // "column" is the column of first non-whitespace character
                int column = scriptState.textBuffer.GetFirstNonWhiteSpaceChar(0, i);

                if (column != -1)
                {
                    if (commentText)
                        lineContent = lineContent.Insert(column, comment);
                    else
                    {
                        if (lineContent.Substring(column).StartsWith("//"))
                            lineContent = lineContent.Remove(column, comment.Length);
                    }
                }

                textToReplace.Append(lineContent);
            }

            (scriptState.textBuffer as TextBuffer).CloseUndoGroup();
            scriptState.textBuffer.ReplaceText(SelectionStart.Y, SelectionStart.X,
                SelectionEnd.Y, SelectionEnd.X, textToReplace.ToString());
            (scriptState.textBuffer as TextBuffer).CloseUndoGroup();

            // The line length would have changed by now since we inserted the forward slashes.
            string lastLine = scriptState.textBuffer.GetLineContent(SelectionEnd.Y);
            int lastLength = string.IsNullOrEmpty(lastLine) ? 0 : lastLine.Length;
            scriptState.selectionEnd.SetCharacterPosition(lastLength, SelectionEnd.Y, true);

            System.Drawing.Point end = SelectionEnd;
            if (!string.IsNullOrEmpty(lastLine) && (lastLine.EndsWith("\n")))
            {
                end.X = 0;
                end.Y = end.Y + 1;
            }

            scriptState.cursorPosition.SetCharacterPosition(end);
            ClearCrossHighlight();
            UpdateVisualOffsetFromCursor();

            return true;
        }

        private bool DeleteCurrentLineHandler(TextEditorCommand command)
        {
            if (this.ReadOnlyState)
                return false;
            int line = scriptState.cursorPosition.GetCharacterPosition().Y;
            int width = scriptState.textBuffer.GetCharacterCount(line, false, true);
            if (width <= 0)
                return false;

            scriptState.textBuffer.ReplaceText(line, 0, line, width, "");
            return true;
        }

        private bool InsertTextHandler(TextEditorCommand command)
        {
            if (this.ReadOnlyState)
                return false;

            string value = string.Empty;
            if (command.Arguments[0] is char)
            {
                int line = scriptState.cursorPosition.CharacterY;
                int col = scriptState.cursorPosition.CharacterX;
                char character = (char)command.Arguments[0];
                if (character == '\t')
                {
                    // TAB is a special key which we'd like to handle differently.
                    // If 'HandleTabInternal' handles it then simply return here.
                    // 
                    if (HasSelection == false)
                    {
                        // There's no selection here.
                        if (HandleTabWithoutSelection(command))
                            return true;
                    }
                    else
                    {
                        if (HandleTabWithSelection(command))
                            return true;
                    }

                    CharPosition converter = Solution.Current.ActiveScript.CreateCharPosition();
                    int visualOffset = converter.CharToVisualOffset(line, col);
                    int remainder = visualOffset % Configurations.TabSpaces.Length;
                    value = new string(' ', Configurations.TabSpaces.Length - remainder);
                }

                // If TAB isn't handled, then proceed the normal way.
                else
                    value = new string(character, 1);
            }
            else
            {
                value = command.Arguments[0] as string;
                if (string.IsNullOrEmpty(value))
                    return false;
            }

            bool shift = command.IsShiftKeyDown;
            if (false != HasSelection)
            {
                // Replace the selection if there's one.
                System.Drawing.Point cursor = this.SelectionStart;
                ReplaceTextInternal(value, CursorPositionFromString(cursor, value));
            }
            else
            {
                // Or simply insert, if there's none.
                InsertTextInternal(this.scriptState.cursorPosition, value);
                System.Drawing.Point cursor = CursorPositionFromString(CursorPosition, value);
                scriptState.cursorPosition.SetCharacterPosition(cursor);
            }

            int nonSpaceChars = -1;

            bool smartFormatted = false;
            ITextEditorSettings settings = this.TextEditorSettings;
            if (settings.EnableSmartFormatting != false)
            {
                if (SmartFormatter.ContainsFormattingTrigger(value))
                {
                    nonSpaceChars = scriptState.cursorPosition.ToNonSpaceCharCount();

                    int fromLine = scriptState.cursorPosition.CharacterY;
                    int toLine = scriptState.cursorPosition.CharacterY;

                    bool containsLineBreak = (value.IndexOf('\n') >= 0);
                    fromLine = (containsLineBreak ? fromLine - 1 : fromLine);
                    if (CurrentTextBuffer.FormatText(fromLine, toLine))
                        smartFormatted = true;
                }
            }

            if (false != smartFormatted)
            {
                // Smart formatting has adjusted the text content on the line(s), 
                // attempt to recover (offset) the cursor position base on its 
                // position before smart formatting happens. To do that, we will 
                // have to know the cursor's offset on the line base on the number
                // of preceeding "non-white-space characters" before formatting.
                // After formatting, the cursor should maintain its offset based 
                // on the number of preceeding non-white-space characters in the 
                // new formatted context.
                // 
                scriptState.cursorPosition.FromNonSpaceCharCount(nonSpaceChars);
            }

            scriptState.mouseDownPosition = scriptState.cursorPosition.GetCharacterPosition();
            ClearCrossHighlight();
            UpdateVisualOffsetFromCursor();
            return true;
        }

        private bool SetPageSizeHandler(TextEditorCommand command)
        {
            int newPageSize = ((int)command.Arguments[0]);
            if (newPageSize <= 0)
                return false;

            this.pageSize = newPageSize;
            return true;
        }

        private bool DoNavigationHandler(TextEditorCommand command)
        {
            int column = scriptState.cursorPosition.GetCharacterPosition().X;
            int line = scriptState.cursorPosition.GetCharacterPosition().Y;
            int totalLines = scriptState.textBuffer.GetLineCount();

            int movementDelta = 0;
            bool verticalNavigation = false;
            bool horizontalNavigation = false;
            switch ((System.Windows.Input.Key)command.Arguments[0])
            {
                case System.Windows.Input.Key.Right:
                    movementDelta = 1;
                    if (command.IsControlKeyDown)
                        movementDelta = OffsetToNextWord(column, line);

                    column = column + movementDelta;
                    horizontalNavigation = true;
                    break;

                case System.Windows.Input.Key.Left:
                    movementDelta = 1;
                    if (command.IsControlKeyDown)
                        movementDelta = OffsetToPreviousWord(column, line);

                    column = column - movementDelta;
                    horizontalNavigation = true;
                    break;

                case System.Windows.Input.Key.Up:
                    line = line - 1;
                    verticalNavigation = true;
                    break;

                case System.Windows.Input.Key.Down:
                    line = line + 1;
                    verticalNavigation = true;
                    break;

                case System.Windows.Input.Key.Home:
                    column = 0;
                    if (command.IsControlKeyDown)
                        line = 0;
                    break;

                case System.Windows.Input.Key.End:
                    if (command.IsControlKeyDown)
                        line = ((totalLines > 0) ? totalLines - 1 : 0);
                    column = scriptState.textBuffer.GetCharacterCount(line, false, false);
                    column = ((column >= 0) ? column : 0);
                    break;

                case System.Windows.Input.Key.PageUp:
                    line = line - pageSize;
                    verticalNavigation = true;
                    break;

                case System.Windows.Input.Key.PageDown:
                    line = line + pageSize;
                    verticalNavigation = true;
                    break;

                default:
                    throw new InvalidOperationException("Unhandled key in 'DoNavigationHandler' method!");
            }

            if (horizontalNavigation)
            {
                if (column < 0)
                {
                    // If we are not already on the first line, then there is 
                    // still room to back-wrap to the end of previous line, 
                    // otherwise stay on the same line and set 'column' to '0'.
                    // 
                    if (line > 0)
                    {
                        line = line - 1;
                        column = scriptState.textBuffer.GetCharacterCount(line, false, true) - 1;
                    }

                    column = ((column < 0) ? 0 : column);
                }
                else
                {
                    int characters = scriptState.textBuffer.GetCharacterCount(line, false, false);
                    if (column > characters)
                    {
                        if (line < totalLines - 1)
                        {
                            column = column - characters - 1;
                            line = line + 1;
                        }
                        else
                        {
                            // Already at the last line, move no more!
                            column = characters;
                        }
                    }
                }
            }
            else
            {
                if (line >= totalLines)
                {
                    line = totalLines - 1;
                    column = scriptState.textBuffer.GetCharacterCount(line, false, false);
                    column = ((column >= 0) ? column : 0);
                }
                else if (false != verticalNavigation)
                {
                    CharPosition converter = Solution.Current.ActiveScript.CreateCharPosition();
                    column = converter.VisualToCharOffset(line, scriptState.visualOffset);
                }

                line = ((line < 0) ? 0 : line);

                // Make sure column does not include \n.
                int characters = scriptState.textBuffer.GetCharacterCount(line, false, false);
                if (column > characters)
                    column = characters;
            }

            scriptState.cursorPosition.SetCharacterPosition(column, line);
            if (command.IsShiftKeyDown == false)
            {
                scriptState.mouseDownPosition.X = column;
                scriptState.mouseDownPosition.Y = line;
            }

            // Adjust visual offset based on the type of navigation we are handling.
            // For example, we want to update it for horizontal navigation, but for 
            // vertical ones, we'll not need to change the visual offset (rather, we
            // should let visual offset determine where the cursor offset should be).
            // 
            switch ((System.Windows.Input.Key)command.Arguments[0])
            {
                case System.Windows.Input.Key.Right:
                case System.Windows.Input.Key.Left:
                case System.Windows.Input.Key.Home:
                case System.Windows.Input.Key.End:
                    UpdateVisualOffsetFromCursor();
                    break;

                case System.Windows.Input.Key.Up:
                case System.Windows.Input.Key.Down:
                case System.Windows.Input.Key.PageUp:
                case System.Windows.Input.Key.PageDown:
                    break; // Do nothing...

                default:
                    throw new InvalidOperationException("Unhandled key in 'DoNavigationHandler' method!");
            }

            CrossHighlight();
            this.MakeSelectionToCursor();
            return true;
        }

        private bool DoControlCharacterHandler(TextEditorCommand command)
        {
            if (this.ReadOnlyState)
                return false;
            System.Windows.Input.Key key =
                ((System.Windows.Input.Key)command.Arguments[0]);

            int column = scriptState.cursorPosition.GetCharacterPosition().X;
            int line = scriptState.cursorPosition.GetCharacterPosition().Y;
            int totalLines = scriptState.textBuffer.GetLineCount();


            if (string.IsNullOrEmpty(scriptState.textBuffer.GetLineContent(0)))
            {
                if (Solution.Current.BreakpointCount > 0)
                    Solution.Current.RemoveBreakpoint(Solution.Current.ActiveScript.GetParsedScript().GetScriptPath(), column, line - 1);
                return true; // Nothing to do, blank script.
            }

            if (key == System.Windows.Input.Key.Back)
            {
                if (HasSelection == false)
                {
                    if (column <= 0)
                    {
                        if (line <= 0)
                            return true;

                        line = line - 1;
                        column = scriptState.textBuffer.GetCharacterCount(line, false, true);
                    }

                    int movementDelta = 1;
                    if (command.IsControlKeyDown)
                        movementDelta = OffsetToPreviousWord(column, line);

                    scriptState.selectionStart.SetCharacterPosition(column - movementDelta, line, true);
                    scriptState.selectionEnd.SetCharacterPosition(column, line, true);
                }

                if (DeleteSelection()) // If there's a selection...
                    return true;       // ... delete it.
            }
            else if (key == System.Windows.Input.Key.Delete)
            {
                if (HasSelection == false)
                {
                    int movementDelta = 1;
                    if (command.IsControlKeyDown)
                        movementDelta = OffsetToNextWord(column, line);

                    scriptState.selectionStart.SetCharacterPosition(column, line, true);
                    scriptState.selectionEnd.SetCharacterPosition(column + movementDelta, line, true);
                }

                if (DeleteSelection()) // If there's a selection...
                    return true;       // ... delete it.
            }
            else
            {
                throw new NotImplementedException();
            }

            return true;
        }

        private bool DoCopyTextHandler(TextEditorCommand command)
        {
            if (HasSelection == false)
                return false;

            CopySelectedText();
            bool cutSelection = ((bool)command.Arguments[0]);
            if (cutSelection != false)
                ReplaceTextInternal("");

            return true;
        }

        private bool DoPasteTextHandler(TextEditorCommand command)
        {
            if (this.ReadOnlyState)
                return false;
            string textToPaste = Clipboard.GetText(TextDataFormat.Text);
            if (string.IsNullOrEmpty(textToPaste))
                return false;

            textToPaste = textToPaste.Replace("\r\n", "\n");

            // Fix: http://adsk-oss.myjetbrains.com/youtrack/issue/IDE-85
            TextBuffer textBuffer = CurrentTextBuffer as TextBuffer;
            textBuffer.CloseUndoGroup(); // Close out any possible undo group.

            if (HasSelection != false)
            {
                System.Drawing.Point offset = CursorPositionFromString(SelectionStart, textToPaste);
                ReplaceTextInternal(textToPaste, offset);
                scriptState.mouseDownPosition = scriptState.cursorPosition.GetCharacterPosition();
            }
            else
            {
                InsertTextInternal(scriptState.cursorPosition, textToPaste);
                System.Drawing.Point offset = CursorPositionFromString(CursorPosition, textToPaste);
                scriptState.cursorPosition.SetCharacterPosition(offset);
                scriptState.mouseDownPosition = scriptState.cursorPosition.GetCharacterPosition();
                UpdateVisualOffsetFromCursor();
            }

            return true;
        }

        private bool SelectAllTextHandler(TextEditorCommand command)
        {
            int lastLineIndex = scriptState.textBuffer.GetLineCount() - 1;
            lastLineIndex = ((lastLineIndex >= 0) ? lastLineIndex : 0);
            int lastLineWidth = scriptState.textBuffer.GetCharacterCount(lastLineIndex, false, true);

            scriptState.selectionStart.SetCharacterPosition(0, 0);
            scriptState.selectionEnd.SetCharacterPosition(lastLineWidth, lastLineIndex);
            scriptState.cursorPosition.SetCharacterPosition(lastLineWidth, lastLineIndex);
            UpdateVisualOffsetFromCursor();
            return true;
        }

        private bool SelectPartialHandler(TextEditorCommand command)
        {
            int startColumn = ((int)command.Arguments[0]);
            int startLine = ((int)command.Arguments[1]);
            int endColumn = ((int)command.Arguments[2]);
            int endLine = ((int)command.Arguments[3]);

            bool swapStartEnd = false;
            if (startLine == endLine) // Same-line selection.
            {
                if (startColumn > endColumn)
                    swapStartEnd = true;
            }
            else // Selection on differnet lines.
            {
                if (startLine > endLine)
                    swapStartEnd = true;
            }

            if (false != swapStartEnd)
            {
                int temporary = startColumn;
                startColumn = endColumn;
                endColumn = temporary;

                temporary = startLine;
                startLine = endLine;
                endLine = temporary;
            }

            scriptState.selectionStart.SetCharacterPosition(startColumn, startLine, true);
            scriptState.selectionEnd.SetCharacterPosition(endColumn, endLine, true);

            if (false == swapStartEnd) // No swap, forward selection.
            {
                scriptState.mouseDownPosition = scriptState.selectionStart.GetCharacterPosition();
                scriptState.cursorPosition.SetCharacterPosition(endColumn, endLine);
            }
            else
            {
                scriptState.mouseDownPosition = scriptState.selectionEnd.GetCharacterPosition();
                scriptState.cursorPosition.SetCharacterPosition(startColumn, startLine);
            }

            return true;
        }

        private bool MoveSelectedTextHandler(TextEditorCommand command)
        {
            // Reset internal states.
            mouseButtonDown = false;
            internalDragSource = false;

            if ((HasSelection == false) || this.ReadOnlyState)
                return false;

            int column = ((int)command.Arguments[0]);
            int line = ((int)command.Arguments[1]);
            bool copyText = ((bool)command.Arguments[2]);

            CharPosition destination = new CharPosition(scriptState.textBuffer);
            destination.SetCharacterPosition(column, line);
            MoveSelectionInternal(destination, copyText);
            return true;
        }

        private bool UndoEditingHandler(TextEditorCommand command)
        {
            if (this.ReadOnlyState)
                return false;

            if (null == scriptState || (null == scriptState.textBuffer))
                return false;

            System.Drawing.Point cursorPosition = scriptState.cursorPosition.GetCharacterPosition();
            scriptState.textBuffer.UndoTextEditing(ref cursorPosition);
            scriptState.cursorPosition.SetCharacterPosition(cursorPosition);
            scriptState.mouseDownPosition.X = scriptState.cursorPosition.CharacterX;
            scriptState.mouseDownPosition.Y = scriptState.cursorPosition.CharacterY;
            scriptState.selectionStart.SetCharacterPosition(cursorPosition);
            scriptState.selectionEnd.SetCharacterPosition(cursorPosition);
            UpdateVisualOffsetFromCursor();
            return true;
        }

        private bool RedoEditingHandler(TextEditorCommand command)
        {
            if (this.ReadOnlyState || (null == scriptState))
                return false;

            System.Drawing.Point cursorPosition = scriptState.cursorPosition.GetCharacterPosition();
            scriptState.textBuffer.RedoTextEditing(ref cursorPosition);
            scriptState.cursorPosition.SetCharacterPosition(cursorPosition);
            scriptState.mouseDownPosition.X = scriptState.cursorPosition.CharacterX;
            scriptState.mouseDownPosition.Y = scriptState.cursorPosition.CharacterY;
            scriptState.selectionStart.SetCharacterPosition(cursorPosition);
            scriptState.selectionEnd.SetCharacterPosition(cursorPosition);
            UpdateVisualOffsetFromCursor();
            return true;
        }

        private bool FindReplaceHandler(TextEditorCommand command)
        {
            string textToSearch = ((string)command.Arguments[0]);
            string replacement = ((string)command.Arguments[1]);
            FindOptions option = ((FindOptions)command.Arguments[2]);

            if (null == Solution.Current.ActiveScript || (null == scriptState.textBuffer))
                return false;

            scriptState.textBuffer.FindReplace(textToSearch, replacement, option);

            if (this.ReadOnlyState == true)
                return false;
            else
                return true;
        }

        private bool FormatDocumentHandler(TextEditorCommand command)
        {
            if (this.ReadOnlyState || (null == scriptState.textBuffer))
                return false;

            return (scriptState.textBuffer.FormatText());
        }

        private bool RunHandler(TextEditorCommand command)
        {
            if (EnsureActiveScriptIsSaved() == false)
                return false; // Could not save file it seems!

            if (null == Solution.Current.ActiveScript)
                return true; // No active script.

            return Solution.Current.RunWithoutDebugger();
        }

        private bool StepHandler(TextEditorCommand command)
        {
            bool isInDebugMode = false;
            IExecutionSession session = Solution.Current.ExecutionSession;
            if (session.IsExecutionActive(ref isInDebugMode) == false)
            {
                if (EnsureActiveScriptIsSaved() == false)
                    return false; // Could not save file it seems!
            }

            // Make sure we don't change these assumptions by mistake.
            System.Diagnostics.Debug.Assert(0 == ((int)RunMode.RunTo));
            System.Diagnostics.Debug.Assert(1 == ((int)RunMode.StepNext));
            System.Diagnostics.Debug.Assert(2 == ((int)RunMode.StepIn));
            System.Diagnostics.Debug.Assert(3 == ((int)RunMode.StepOut));

            if (null == Solution.Current.ActiveScript && isInDebugMode == false)
                return false; // No active script.

            RunMode runMode = ((RunMode)command.Arguments[0]);
            return Solution.Current.RunWithDebugger(runMode);
        }

        private bool StopHandler(TextEditorCommand command)
        {
            bool isInDebugMode = false;
            IExecutionSession session = Solution.Current.ExecutionSession;
            if (session.IsExecutionActive(ref isInDebugMode))
                return (session.StopExecution());

            return true; // Execution/debugging stopped either case.
        }

        private bool ToggleBreakpointHandler(TextEditorCommand command)
        {

            if (null == Solution.Current.ActiveScript)
                return false;

            if (string.IsNullOrEmpty(Solution.Current.ActiveScript.GetParsedScript().GetScriptPath()))
                return false;
            else
            {
                System.Drawing.Point cursor = CursorPosition;
                IScriptObject currScript = Solution.Current.ActiveScript;

                ITextBuffer textBuffer = (null == currScript ? null : currScript.GetTextBuffer());

                if (null != textBuffer)
                {
                    int column = textBuffer.GetFirstNonWhiteSpaceChar(cursor.X, cursor.Y);
                    cursor.X = ((column != -1) ? column : cursor.X);

                    IParsedScript parsedScript = currScript.GetParsedScript();
                    string scriptPath = parsedScript.GetScriptPath();
                    if (CanSetBreakpointAt(cursor.X, cursor.Y))
                    {
                        if (Solution.Current.InsertBreakpoint(scriptPath, cursor.X, cursor.Y))
                            return true;
                        else
                            return false;
                    }
                    else
                    {
                        Solution.Current.RemoveBreakpoint(scriptPath, cursor.X, cursor.Y);
                        return false;
                    }
                }

                return false;
            }
        }

        #endregion

        #region Class Properties for Internal Use

        internal int LineCount
        {
            get { return scriptState.textBuffer.GetLineCount(); }
        }

        #endregion

        #region Getters

        public IParsedScript GetActiveScript()
        {
            IScriptObject activeHostScript = Solution.Current.ActiveScript;
            return ((null == activeHostScript) ? null : activeHostScript.GetParsedScript());
        }

        public string GetLine(int lineIndex)
        {
            return scriptState.textBuffer.GetLineContent(lineIndex);
        }

        public int GetCharacterCount(int lineIndex, bool expandTabs, bool includeLineBreak)
        {
            return scriptState.textBuffer.GetCharacterCount(lineIndex, expandTabs, includeLineBreak);
        }

        public bool GetFragmentRegion(int column, int line, CharPosition start, CharPosition end)
        {
            return (codeFragmentManager.GetFragmentRegion(column, line, start, end));
        }

        #endregion

        #region Private Class Event Handler

        private void OnScriptCountChanged(object sender, ScriptCountChangedEventArgs e)
        {
            foreach (IScriptObject script in e.ScriptsAdded)
            {
                ITextBuffer textBuffer = script.GetTextBuffer();
                textBuffer.ParsePendingChanged += new ParsePendingChangedHandler(OnParsePendingChanged);
            }
        }

        private void OnParsePendingChanged(object sender, ParsePendingChangedEventArgs e)
        {
            if (null != backgroundParseTimer)
                backgroundParseTimer.Stop();

            if (true == e.NewParsePending)
            {
                if (null == backgroundParseTimer)
                {
#if EXPERIMENTAL_AUTO_COMPLETE_ENGINE
                    // This should be 'null' otherwise.
                    autoCompleteEngine = ClassFactory.CreateAutoCompleteEngine();
#endif
                    backgroundParseTimer = new DispatcherTimer();
                    backgroundParseTimer.Interval = new TimeSpan(0, 0, 0, 0, 200); // 200ms
                    backgroundParseTimer.Tick += new EventHandler(OnBackgroundParseTimerTick);
                }

                backgroundParseTimer.Start();
            }
        }

        private void OnBackgroundParseTimerTick(object sender, EventArgs e)
        {
            TextBuffer textBuffer = CurrentTextBuffer as TextBuffer;
            if (null == textBuffer || (false == textBuffer.ParsePending))
                return; // Nothing to do here...

            if (null == backgroundParser)
            {
                backgroundParser = new BackgroundWorker();
                backgroundParser.DoWork += new DoWorkEventHandler(OnBeginBackgroundParsing);
                backgroundParser.RunWorkerCompleted += new RunWorkerCompletedEventHandler(OnBackgroundParsingCompleted);
            }

            // The background parser is busy right now, leave the "ParsePending" 
            // flag as it is (value being "true"). We'll pick it up again when the 
            // parser is done later (in "OnBackgroundParsingCompleted") handler.
            if (backgroundParser.IsBusy != false)
                return;

            // Send the parsing job to the background parser.
            textBuffer.ParsePending = false;
            backgroundParser.RunWorkerAsync(CurrentTextBuffer.GetStream());
        }

        private void OnBeginBackgroundParsing(object sender, DoWorkEventArgs e)
        {
            e.Result = null;

            Stream sourceStream = e.Argument as Stream;
            IParsedScript parsedScript = InterfaceFactory.CreateParsedScript();
            parsedScript.ParseStream(sourceStream);
            e.Result = new CodeFragmentManager(parsedScript);
        }

        private void OnBackgroundParsingCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (null == scriptState || (null == scriptState.textBuffer))
                return; // Too late, maybe next time (the text editor is gone now).

            // Check for modification. "ParsePending" flag was set to "false" the 
            // last time background parser was sent off to parse the script. If we
            // got here with "ParsePending" being "true", it means the original 
            // snapshot of text content at the time work started has already been 
            // modified again. So the results of previous parse wouldn't be that 
            // useful anymore, in which case we should discard the parse result 
            // and start all over again (i.e. sending the background parser off 
            // for another round of parsing with the latest script content).
            // 
            if (scriptState.textBuffer.ParsePending != false)
            {
                (scriptState.textBuffer as TextBuffer).ParsePending = false;
                backgroundParser.RunWorkerAsync(CurrentTextBuffer.GetStream());
                return; // Go parse it, and come back to us at a later time.
            }

            IScriptObject currScript = Solution.Current.ActiveScript;
            if (null != currScript)
            {
                codeFragmentManager = e.Result as CodeFragmentManager;
                IParsedScript parsedScript = currScript.GetParsedScript();
                parsedScript.CopyParsedResults(codeFragmentManager.GetParsedScript());
                CrossHighlight();

                if (null != ParseCompleted)
                    ParseCompleted(this, new EventArgs());

                if (null != autoCompleteEngine)
                {
                    string codeSnapshot = scriptState.textBuffer.GetContent();
                    string scriptPath = parsedScript.GetScriptPath();
                    autoCompleteEngine.UpdateDatabase(codeSnapshot, scriptPath);
                }
            }
        }

        #endregion

        #region Private Class Helper Methods

        private TextEditorCore(ExecutionStateChangedHandler executionStateHandler)
        {
            this.ReadOnlyState = false;
            this.UiExecutionStateChangeHandler = executionStateHandler;
            this.overrideModifier = TextEditorCommand.Modifier.None;
            Solution.ScriptCountChanged += new ScriptCountChangedHandler(OnScriptCountChanged);
        }

        private void AddMouseModifiers(TextEditorCommand command, MouseEventArgs eventArgs)
        {
            if (null == eventArgs)
                return;

            if ((eventArgs.LeftButton & MouseButtonState.Pressed) != 0)
                command.Modifiers |= ((uint)TextEditorCommand.Modifier.LeftButtonDown);
            if ((eventArgs.MiddleButton & MouseButtonState.Pressed) != 0)
                command.Modifiers |= ((uint)TextEditorCommand.Modifier.MiddleButtonDown);
            if ((eventArgs.RightButton & MouseButtonState.Pressed) != 0)
                command.Modifiers |= ((uint)TextEditorCommand.Modifier.RightButtonDown);
        }

        private void ReplaceTextInternal(string p)
        {
            ReplaceTextInternal(p, scriptState.selectionStart.GetCharacterPosition());
        }

        private void ReplaceTextInternal(string newContent, System.Drawing.Point cursor)
        {
            if (this.ReadOnlyState)
                return;

            scriptState.textBuffer.ReplaceText(
                scriptState.selectionStart.CharacterY,
                scriptState.selectionStart.CharacterX,
                scriptState.selectionEnd.CharacterY,
                scriptState.selectionEnd.CharacterX, newContent);

            scriptState.selectionStart.SetCharacterPosition(cursor);
            scriptState.selectionEnd.SetCharacterPosition(cursor);
            scriptState.cursorPosition.SetCharacterPosition(cursor);

            ClearCrossHighlight();
            UpdateVisualOffsetFromCursor();
        }

        private int InsertTextInternal(CharPosition point, string p)
        {
            if (this.ReadOnlyState)
                return 0;

            scriptState.textBuffer.InsertText(point.CharacterY, point.CharacterX, p);
            return p.Length - 1;
        }

        private void MoveSelectionInternal(CharPosition destination, bool copyText)
        {
            System.Drawing.Point cursor = destination.GetCharacterPosition();
            System.Drawing.Point selectionStart = scriptState.selectionStart.GetCharacterPosition();
            System.Drawing.Point selectionEnd = scriptState.selectionEnd.GetCharacterPosition();

            if (!this.ReadOnlyState)
            {
                string textToMove = GetSelectionText();
                if (string.IsNullOrEmpty(textToMove))
                {
                    string msg = "Don't call this when there's no selection!";
                    throw new InvalidOperationException(msg);
                }

                // If Drag and Drop is done onto the selected text -- return 
                if (Utility.IsPointInRegion(selectionStart, selectionEnd, cursor))
                {
                    // Set the cursor to the end of the selected text.
                    scriptState.cursorPosition.SetCharacterPosition(selectionEnd);
                    return;
                }

                if (false != copyText) // It is a copy operation.
                {
                    scriptState.textBuffer.InsertText(destination.CharacterY,
                        destination.CharacterX, textToMove);
                }
                else
                {
                    scriptState.textBuffer.MoveText(scriptState.selectionStart.GetCharacterPosition(),
                        scriptState.selectionEnd.GetCharacterPosition(), cursor);
                }

                cursor = destination.GetCharacterPosition();

                System.Drawing.Point newCursor = destination.GetCharacterPosition();
                newCursor = CursorPositionFromString(newCursor, textToMove);

                int numberOfLinesToMove = textToMove.Split('\n').Length - 1;

                if (false == copyText)
                {
                    // inserted above
                    if (cursor.Y < selectionStart.Y)
                        selectionStart = cursor;
                    // inserted left (same line)
                    else if (cursor.Y == selectionStart.Y && cursor.X < selectionStart.X)
                        selectionStart = cursor;
                    // inserted below
                    else if (cursor.Y > selectionEnd.Y)
                    {
                        selectionStart = cursor;
                        selectionStart.Y -= numberOfLinesToMove;
                        newCursor.Y -= numberOfLinesToMove;
                    }
                    // inserted right (same line)
                    else if (cursor.Y == selectionEnd.Y && cursor.X > selectionEnd.X)
                    {
                        int differenceInSameLine = Math.Abs(selectionStart.X - selectionEnd.X);
                        selectionStart = cursor;
                        selectionStart.Y -= numberOfLinesToMove;
                        newCursor.Y -= numberOfLinesToMove;
                        selectionStart.X -= differenceInSameLine;
                        if (selectionStart.Y == newCursor.Y)
                            newCursor.X -= differenceInSameLine;
                    }
                }
                else
                    selectionStart = cursor;
                cursor = newCursor;
                selectionEnd = cursor;
            }

            // Now that selected text is moved, reset certain internal members.
            scriptState.cursorPosition.SetCharacterPosition(cursor);
            scriptState.selectionStart.SetCharacterPosition(selectionStart);
            scriptState.selectionEnd.SetCharacterPosition(selectionEnd);
            UpdateVisualOffsetFromCursor();
        }

        private bool SelectFragmentInternal(int column, int line, CharPosition start, CharPosition end)
        {
            string lineContent = scriptState.textBuffer.GetLineContent(line);
            if (null == lineContent || (lineContent.Length <= 0))
                return false;

            if (column < 0 || (column >= lineContent.Length))
                return false;

            int first = column;
            int last = column;

            if (char.IsLetterOrDigit(lineContent[column]))
            {
                while (first > 0)
                {
                    if (char.IsLetterOrDigit(lineContent[first - 1]) == false)
                        break;
                    first = first - 1;
                }

                while (last < lineContent.Length - 1)
                {
                    if (char.IsLetterOrDigit(lineContent[last + 1]) == false)
                        break;
                    last = last + 1;
                }
            }
            else if (char.IsPunctuation(lineContent[column]))
            {
                while (first > 0)
                {
                    if (char.IsPunctuation(lineContent[first - 1]) == false)
                        break;
                    first = first - 1;
                }

                while (last < lineContent.Length - 1)
                {
                    if (char.IsPunctuation(lineContent[last + 1]) == false)
                        break;
                    last = last + 1;
                }
            }
            else if (char.IsWhiteSpace(lineContent[column]))
            {
                while (first > 0)
                {
                    if (char.IsWhiteSpace(lineContent[first - 1]) == false)
                        break;
                    first = first - 1;
                }

                while (last < lineContent.Length - 1)
                {
                    if (char.IsWhiteSpace(lineContent[last + 1]) == false)
                        break;
                    last = last + 1;
                }
            }
            else
            {
                return false;
            }

            start.SetCharacterPosition(first, line);
            end.SetCharacterPosition(last + 1, line);
            return true;
        }

        private bool HandleTabWithoutSelection(TextEditorCommand command)
        {
            // A TAB key without SHIFT is just a regular TAB when there is 
            // no selection. In that case we'll just return 'false' here for 
            // the original text insertion to take over.
            // 
            if (command.IsShiftKeyDown == false)
                return false;

            // If we are already at the front of the line, then nothing to do.
            if (scriptState.cursorPosition.CharacterX <= 0)
                return true;

            ITextBuffer textBuffer = scriptState.textBuffer;
            string line = textBuffer.GetLineContent(scriptState.cursorPosition.CharacterY);
            if (string.IsNullOrEmpty(line))
                return true; // On an empty line, nothing more to do.

            int index = ScanForTabOffset(line, scriptState.cursorPosition.CharacterX);
            if (index < scriptState.cursorPosition.CharacterX)
            {
                // Make a selection from 'index' to 'cursorPosition.CharacterX',
                // so that the content can be removed (i.e. replaced by an empty string).
                // 
                System.Drawing.Point cursor = scriptState.cursorPosition.GetCharacterPosition();
                scriptState.selectionStart.SetCharacterPosition(index, cursor.Y);
                scriptState.selectionEnd.SetCharacterPosition(cursor);

                ReplaceTextInternal(""); // Remove the spaces.
            }

            return true;
        }

        private bool HandleTabWithSelection(TextEditorCommand command)
        {
            if (null == Solution.Current.ActiveScript)
                return false;

            bool indent = (command.IsShiftKeyDown == false);
            int firstLine = scriptState.selectionStart.CharacterY;
            int finalLine = scriptState.selectionEnd.CharacterY;

            int tabWidth = Configurations.TabSpaces.Length;
            CharPosition converter = Solution.Current.ActiveScript.CreateCharPosition();

            string content = string.Empty;
            for (int index = firstLine; index <= finalLine; ++index)
            {
                string line = scriptState.textBuffer.GetLineContent(index);

                if (false != indent)
                {
                    // Indentation is adding tab in front.
                    content += Configurations.TabSpaces + line;
                }
                else
                {
                    int start = 0;
                    while (start < line.Length)
                    {
                        if (line[start] != ' ' && line[start] != '\t')
                            break;

                        start = start + 1;
                    }

                    int visualOffset = converter.CharToVisualOffset(index, start);
                    int tabCount = (int)Math.Floor(((double)visualOffset) / tabWidth);
                    if ((visualOffset % tabWidth) == 0)
                        tabCount = tabCount - 1;

                    if (tabCount > 0)
                        content += new string(' ', tabCount * tabWidth);

                    content += line.Substring(start, line.Length - start);
                }
            }

            int endChar = scriptState.textBuffer.GetCharacterCount(finalLine, false, true);
            scriptState.selectionStart.SetCharacterPosition(0, firstLine);
            scriptState.selectionEnd.SetCharacterPosition(endChar, finalLine, true);
            ReplaceTextInternal(content); // Remove the spaces.

            // ReplaceTextInternal resets the selection and cursor position, change it here.
            endChar = scriptState.textBuffer.GetCharacterCount(finalLine, false, true);
            scriptState.selectionStart.SetCharacterPosition(0, firstLine);
            scriptState.selectionEnd.SetCharacterPosition(endChar, finalLine, true);

            System.Drawing.Point cursor = scriptState.selectionEnd.GetCharacterPosition();
            scriptState.cursorPosition.SetCharacterPosition(cursor);
            return true;
        }

        private int ScanForTabOffset(string line, int start)
        {
            int index = start;
            int foundWhiteSpaces = 0;

            int tabWidth = Configurations.TabSpaces.Length;
            while ((foundWhiteSpaces < tabWidth) && (index > 0))
            {
                char prevCharacter = line[index - 1];

                if (prevCharacter == ' ')
                    foundWhiteSpaces++;
                else if (prevCharacter == '\t')
                {
                    // If we have discovered, some spaces before this TAB, 
                    // then we will just remove those spaces excluding this TAB.
                    if (foundWhiteSpaces > 0)
                        break;

                    foundWhiteSpaces += tabWidth;
                }
                else
                {
                    // All other characters, stop right there.
                    break;
                }

                index = index - 1;
            }

            return index;
        }

        private void MakeSelectionToCursor()
        {
            if (null == scriptState.cursorPosition) // Nope, not quite ready yet.
                return;

            CharPosition cursorPosition = scriptState.cursorPosition;
            CharPosition selectionStart = scriptState.selectionStart;
            CharPosition selectionEnd = scriptState.selectionEnd;

            if (scriptState.mouseDownPosition.Y > cursorPosition.GetCharacterPosition().Y)
            {
                selectionStart.SetCharacterPosition(cursorPosition.GetCharacterPosition());
                selectionEnd.SetCharacterPosition(scriptState.mouseDownPosition);
            }
            else if (scriptState.mouseDownPosition.Y == cursorPosition.GetCharacterPosition().Y)
            {
                if (scriptState.mouseDownPosition.X >= cursorPosition.GetCharacterPosition().X)
                {
                    selectionStart.SetCharacterPosition(cursorPosition.GetCharacterPosition());
                    selectionEnd.SetCharacterPosition(scriptState.mouseDownPosition);
                }
                else
                {
                    // This is the only forward selection case.
                    selectionStart.SetCharacterPosition(scriptState.mouseDownPosition);
                    selectionEnd.SetCharacterPosition(cursorPosition.GetCharacterPosition());
                }
            }
            else
            {
                // This is the only forward selection case.
                selectionStart.SetCharacterPosition(scriptState.mouseDownPosition);
                selectionEnd.SetCharacterPosition(cursorPosition.GetCharacterPosition());
            }
        }

        private int OffsetToNextWord(int column, int line)
        {
            string lineContent = scriptState.textBuffer.GetLineContent(line);
            NavigationParser parser = new NavigationParser();
            List<string> lineTokens = parser.Tokenize(lineContent);
            if (null == lineTokens)
                return 0;

            int index = 0;
            int position = 0;

            for (index = 1; index <= lineTokens.Count; index++)
            {
                position += lineTokens[index - 1].Length;
                if (position > column)
                    break;
            }

            while (true)
            {	//if cursor is at end of line and there is a space on the next line, length of space needs to be added
                if (index >= lineTokens.Count)
                {
                    index = 0;       // Reset the loop counter.
                    line = line + 1; // Go to the next line.

                    lineContent = scriptState.textBuffer.GetLineContent(line);
                    lineTokens = parser.Tokenize(lineContent);

                    if (null == lineTokens || (lineTokens.Count <= 0))
                        return position - column;
                }

                if (string.IsNullOrWhiteSpace(lineTokens[index]) == false)
                    break;
                if (lineTokens[index] == "\n")
                    break;

                position += lineTokens[index].Length;
                index = index + 1;
            }

            return position - column;
        }

        private int OffsetToPreviousWord(int column, int line)
        {
            if (column == 0) // We're already at the beginning.
                return 1;

            string lineContent = scriptState.textBuffer.GetLineContent(line);
            NavigationParser parser = new NavigationParser();
            List<string> lineTokens = parser.Tokenize(lineContent);
            if (null == lineTokens)
                return 0;

            int index = 0;
            int position = 0;

            for (index = 1; index <= lineTokens.Count; index++)
            {
                position += lineTokens[index - 1].Length;
                if (position >= column)
                    break;
            }

            while (index > 0)
            {
                index = index - 1;
                position -= lineTokens[index].Length;

                if (string.IsNullOrWhiteSpace(lineTokens[index]) == false)
                    break;
            }

            return column - position;
        }

        private void CrossHighlight()
        {
            if (null == codeFragmentManager)
                return;

            int line = scriptState.cursorPosition.GetCharacterPosition().Y;
            int column = scriptState.cursorPosition.GetCharacterPosition().X;
            CodeFragment toBeHighlighted;
            codeFragmentManager.GetFragment(column, line, out toBeHighlighted);
            int highlightType = codeFragmentManager.GenerateHighlightArray(toBeHighlighted);
            if (highlightType == 0)
                crossHighlightArray = null;
            else if (highlightType == 1)
                crossHighlightArray = codeFragmentManager.GetAllMatchingFragments();
            else if (highlightType == 2)
                crossHighlightArray = codeFragmentManager.GetPairedFragments();
            else if (highlightType == 3)
                return;
        }

        private void ClearCrossHighlight()
        {
            crossHighlightArray = null;
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
            if (GetFragment(column, line, out fragment) > 0)
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

            return true; // Fixed Defect IDE-221
        }

        private bool EnsureActiveScriptIsSaved()
        {
            IScriptObject currScript = Solution.Current.ActiveScript;
            if (null != currScript)
            {
                if (currScript.GetTextBuffer().ScriptModified == false)
                    return true;

                return SaveScriptToFileInternal(false);
            }

            return false; // Active script? What active script?
        }

        private bool SaveScriptToFileInternal(bool saveAs)
        {
            IScriptObject currScript = Solution.Current.ActiveScript;
            return currScript.SaveScript(saveAs);
        }

        private string GetSettingsFilePath()
        {
            try
            {
                string appDataFolder = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.ApplicationData);

                if (!appDataFolder.EndsWith("\\"))
                    appDataFolder += "\\";

                appDataFolder += @"Autodesk\DesignScript Studio\";
                if (Directory.Exists(appDataFolder) == false)
                    Directory.CreateDirectory(appDataFolder);

                return (appDataFolder + @"EditorSettings.xml");
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        #endregion
    }
}
