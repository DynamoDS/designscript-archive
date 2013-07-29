using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using DesignScript.Parser;

namespace DesignScript.Editor.Core
{
    public enum StatusTypes { Warning, Error, Info };

    public interface IAssertableProperties
    {
        int LineCount { get; }
        int BreakpointCount { get; }
        string SelectionText { get; }
        string ActiveScriptName { get; }
        int CurrentSearchIndex { get; }
        string SearchTerm { get; }
        int MatchCount { get; }
        int SearchOption { get; }

        System.Drawing.Point CursorPosition { get; }
        System.Drawing.Point SelectionStart { get; }
        System.Drawing.Point SelectionEnd { get; }
        System.Drawing.Point ExecutionCursorStart { get; }
        System.Drawing.Point ExecutionCursorEnd { get; }
    }

    public interface ICommandRecorder
    {
        void Record(TextEditorCommand command);
    }

    public interface ITextEditorCore
    {
        #region Core Command Related Interface Methods

        bool CreateNewScript();
        bool LoadScriptFromFile(string scriptFilePath);
        bool SaveScriptToFile(bool saveAs);
        bool CloseScript(int index);
        bool ChangeScript(int index);
        bool SetCursorPosition(int column, int line);
        bool SetMouseDownPosition(int column, int line, MouseEventArgs eventArgs);
        bool SetMouseUpPosition(int column, int line, MouseEventArgs eventArgs);
        bool SetMouseMovePosition(int column, int line, MouseEventArgs eventArgs);
        bool ClearDragDropState();
        bool SelectFragment(int column, int line);
        bool SelectLines(int lineIndex, int lineOffset);
        bool CommentLines(bool commentText);
        bool DeleteCurrentLine();
        bool InsertText(char character);
        bool InsertText(string textContent);
        bool SetPageSize(int pageSize);
        bool DoNavigation(System.Windows.Input.Key key);
        bool DoControlCharacter(System.Windows.Input.Key key);
        bool DoCopyText(bool cutSelection);
        bool DoPasteText();
        bool SelectAllText();
        bool SelectPartial(int startColumn, int startLine, int endColumn, int endLine);
        bool MoveSelectedText(int column, int line, bool copyText);
        bool UndoEditing();
        bool RedoEditing();
        bool FindReplace(string textToSearch, string replacement, FindOptions option);
        bool Run();
        bool Step(RunMode runMode);
        bool Stop();
        bool ToggleBreakpoint();
        bool FormatDocument();

        #endregion

        #region Action playback/recording related methods

        bool PlaybackCommand(TextEditorCommand command);
        void SetCommandRecorder(ICommandRecorder recorder);
        IAssertableProperties GetAssertableProperties();

        #endregion

        #region Public Class Operational Methods

        void Shutdown();
        bool IsPointInSelection(int column, int line);
        int GetFragment(int column, int line, out CodeFragment fragment);
        int GetFragmentForInspection(int column, int line, out CodeFragment forkFragment);
        bool GetLineFragments(int line, out List<CodeFragment> fragments);
        int GetPreviousFragment(int column, int line, out CodeFragment prevFragment);
        int GetNextFragment(CodeFragment curFragment, out CodeFragment nextFragment);
        bool SetFragmentText(int column, int line, string text);
        CodeFragment[] GetCrossHighlightArray();
        void CheckForExternalModifications(ref List<int> scriptsToClose);

        #endregion

        #region Text editor public properties

        bool ReadOnlyState { get; set; }
        bool EnableRegularCommands { get; set; }
        bool InternalDragSourceExists { get; }
        bool HasSelection { get; }
        string SelectionText { get; }
        System.Drawing.Point CursorPosition { get; }
        System.Drawing.Point SelectionStart { get; }
        System.Drawing.Point SelectionEnd { get; }
        double HorizontalScrollPosition { get; set; }
        double VerticalScrollPosition { get; set; }
        ITextBuffer CurrentTextBuffer { get; }
        StartUpData Data { get; set; }
        ITextEditorSettings TextEditorSettings { get; }

        #endregion

        #region Public Interface Events

        event ParseCompletedHandler ParseCompleted;

        #endregion
    }

    public interface INavigationParser
    {
        List<string> Tokenize(string content);
    }

    public interface ITextEditorSettings
    {
        int MaxArrayDisplaySize { get; set; }
        int MaxOutputDepth { get; set; }
        int FontMultiplier { get; set; }

        bool DisplayOutput { get; }
        bool EnableSmartFormatting { get; }
        bool EnableNumericSlider { get; }
        bool CollectFeedback { get; set; }

        string IncludePath { get; set; }

        void ToggleDisplayOutput();
        void ToggleSmartFormatting();
        void ToggleNumericSlider();
    }

    public interface IHostApplication
    {
        string PromptScriptSelection();
        string[] GetApplicationArguments();

        bool PreExecutionSetup(bool debugMode, ref string errorMessage);
        bool PreStepSetup(ref string errorMessage);
        void PostStepTearDown();
        void PostExecutionTearDown();
        void QuitApplication();

        bool GetContextualMenu(Dictionary<int, string> menuItems, int line, int column, ITextBuffer context);
        bool HandleMenuItemClick(int id);

        event EventHandler BeginQuit;

        Dictionary<string, object> Configurations { get; }
    }

    public enum FindOptions
    {
        MatchCase = 0x0001,
        MatchWord = 0x0002,
        FindPrevious = 0x0004,
        FindNext = 0x0008,
        ReplaceOnce = 0x0010,
        ReplaceAll = 0x0020
    };

    public struct FindPosition { public System.Drawing.Point startPoint, endPoint; };

    public interface ITextBuffer
    {
        #region Text Editing Related Interface Methods

        void InsertText(int line, int column, string text);
        void ReplaceText(int startLine, int startColumn, int endLine, int endColumn, string text);
        void FindReplace(string textToSearch, string textToReplace, FindOptions searchOptions);
        void ModifyText(int line, string text);
        void MoveText(System.Drawing.Point originStart, System.Drawing.Point originEnd, System.Drawing.Point destination);
        void UndoTextEditing(ref System.Drawing.Point cursorPosition);
        void RedoTextEditing(ref System.Drawing.Point cursorPosition);

        #endregion

        #region State Query Interface Methods

        int GetLineCount(bool includeVirtualLine = true);
        int GetWidestLineWidth(bool expandTabs);
        int GetFirstNonWhiteSpaceChar(int column, int line);
        int GetCharacterCount(int line, bool expandTabs, bool includeLineBreak);
        bool IsNullOrWhiteSpace(int line);

        #endregion

        #region Content Query Interface Methods

        string GetContent();
        string GetPartialContent(int[] linesToExclude, string replacement);
        Stream GetStream();
        string GetText(CharPosition rangeStart, CharPosition rangeEnd);
        string GetText(int startLine, int startColumn, int endLine, int endColumn);
        string GetLineContent(int line);
        string GetIdentifierBeforeColumn(int line, int column);
        FunctionCallContext GetFunctionCallContext(int line, int column);

        #endregion

        #region Smart Formatting Interface Methods

        bool FormatText();
        bool FormatText(int fromLine, int toLine);
        int FindOpenBrace(int line, int column);
        string RetrieveLineBlock(ref int lineIndex, int column);

        #endregion

        #region Public Interface Events

        event ParsePendingChangedHandler ParsePendingChanged;
        event ScriptModifiedHandler Modified;
        event LinesUpdatedHandler LinesUpdated;

        #endregion

        #region Text Buffer Properties

        bool ParsePending { get; }
        bool ScriptModified { get; set; }
        IScriptObject OwningScript { get; }
        int CurrentSearchIndex { get; }
        List<FindPosition> SearchResult { get; }
        string TextToSearch { get; }
        int SearchOption { get; }

        #endregion
    }

    public enum RunMode
    {
        RunTo, StepNext, StepIn, StepOut
    }

    public interface IScriptObject
    {
        bool SaveScript(bool saveAs);
        CharPosition CreateCharPosition();
        IParsedScript GetParsedScript();
        ITextBuffer GetTextBuffer();
        bool ScriptModifiedExternal { get; }
    }

    public interface IExecutionSession
    {
        // Write operations.
        bool SetBreakpointAt(ProtoCore.CodeModel.CodePoint breakpoint);
        bool RunWithoutDebugger();
        bool RunWithDebugger(RunMode runMode);
        bool StopExecution();
        bool SetValue(int line, int column, int value);
        bool SetValue(int line, int column, double value);

        // Read-only operations.
        bool IsExecutionActive(ref bool isInDebugMode);
        bool ExecuteExpression(string expression);
        bool GetExecutionCursor(ref ProtoCore.CodeModel.CodeRange cursor);
        bool GetStackValueData(ProtoCore.Lang.Obj stackValue, ref string data);
        bool GetStackValueType(ProtoCore.Lang.Obj stackValue, ref string type);
        bool GetStackValue(string expression, out ProtoCore.Lang.Obj value);
        bool GetBreakpoints(out List<ProtoCore.CodeModel.CodeRange> breakpoints);
        bool GetArrayElements(ProtoCore.Lang.Obj stackValue, out List<ProtoCore.Lang.Obj> elements);
        bool GetClassProperties(ProtoCore.Lang.Obj stackValue, out Dictionary<string, ProtoCore.Lang.Obj> properties);
        bool PopulateInstructions(List<DisassemblyEntry> instructions);

        // Interface properties.
        bool IsBusy { get; }
        bool ExecutionEnded { get; }
        IHostApplication HostApplication { get;  }
    }

    public enum ReadOnlyDialogResult
    {
        SaveAs,
        OverWrite,
        Cancel
    }

    public interface IDialogProvider
    {
        ReadOnlyDialogResult ShowReadOnlyDialog(bool allowOverwriteOption);
        void ShowExceptionDialog(Exception exception);
        bool ShowSaveFileDialog(ref string fileName);
        void ShowFileAlreadyOpenDialog();
        bool ShowReloadDialog(string scriptFilePath);
        void DisplayStatusMessage(StatusTypes statusType, string message, int seconds);
    }

    public class DisassemblyEntry
    {
        public string InstructionString { get; set; }
    }

    public class CoreInterfaceFactory
    {
        public static ITextEditorCore CreateTextEditorCore(ExecutionStateChangedHandler handler)
        {
            return TextEditorCore.CreateSingleton(handler);
        }

        public static void RegisterInterfaces(IHostApplication hostApplication, IDialogProvider dialogProvider)
        {
            if (null != TextEditorCore.HostApplication)
            {
                string message = "'RegisterInterfaces' cannot be called twice!";
                throw new InvalidOperationException(message);
            }

            TextEditorCore.HostApplication = hostApplication;
            TextEditorCore.DialogProvider = dialogProvider;
        }
    }
}
