using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace DesignScript.Editor.Core
{
    using ProtoCore;
    using ProtoCore.CodeModel;
    using DesignScript.Parser;

    public class ScriptCountChangedEventArgs : EventArgs
    {
        private List<IScriptObject> scriptsAdded = null;
        private List<IScriptObject> scriptsRemoved = null;

        internal ScriptCountChangedEventArgs()
        {
            scriptsAdded = new List<IScriptObject>();
            scriptsRemoved = new List<IScriptObject>();
        }

        public List<IScriptObject> ScriptsAdded
        {
            get { return this.scriptsAdded; }
        }

        public List<IScriptObject> ScriptsRemoved
        {
            get { return this.scriptsRemoved; }
        }
    }

    public delegate void ScriptCountChangedHandler(
        object sender, ScriptCountChangedEventArgs e);

    public class Solution
    {
        #region Solution Specific Data Members

        // Public class data members.
        public static event ScriptCountChangedHandler ScriptCountChanged = null;

        // Private class data members.
        private bool solutionModified = false;
        private bool showSaveSolutionDialog = false;
        private string filePath = string.Empty;
        private SolutionData solutionData = null;
        private LinesUpdatedHandler linesUpdatedHandler = null;
        private static Solution currentSolution = null;

        #endregion

        #region Script and Execution Specific Data Members

        private List<IScriptObject> loadedScripts = null;
        private IScriptObject activeScript = null;
        private IExecutionSession executionSession = null;
        private EditorOutputStream outputStream = null;

        #endregion

        #region Public Class Properties

        public static Solution Current
        {
            get
            {
                if (null == currentSolution)
                    currentSolution = new Solution();

                return currentSolution;
            }
        }

        public bool Asynchronous { get; set; }

        public bool IsModified { get { return solutionModified; } }

        public bool ShowSaveDialog { get { return showSaveSolutionDialog; } }

        public IScriptObject ActiveScript { get { return activeScript; } }

        public int ActiveScriptIndex
        {
            get
            {
                if (null == activeScript)
                    return -1;

                return loadedScripts.IndexOf(activeScript);
            }
        }

        public IExecutionSession ExecutionSession
        {
            get
            {
                if (null == activeScript)
                    return executionSession;
                if (null == executionSession)
                {
                    IHostApplication host = TextEditorCore.HostApplication;
                    executionSession = new ExecutionSession(host);
                }

                return executionSession;
            }
        }

        public int ScriptCount
        {
            get
            {
                if (null == loadedScripts)
                    return 0;

                return loadedScripts.Count;
            }
        }

        internal int BreakpointCount
        {
            get
            {
                if (null == solutionData || (null == solutionData.Breakpoints))
                    return 0;

                return solutionData.Breakpoints.Count;
            }
        }

        internal string SolutionFilePath { get { return this.filePath; } }

        #endregion

        #region Public File Operation Methods

        public bool SaveToFile(string filePath)
        {
            if (this.solutionModified == false)
                return true; // Nothing to save.

            // If the caller does not provide a name...
            if (string.IsNullOrEmpty(filePath))
            {
                // ... then we'd better have one locally.
                if (string.IsNullOrEmpty(this.filePath))
                    return false;
            }
            else
            {
                // Caller provided a name, use it.
                this.filePath = filePath;
            }

            SerializeToScriptItems();

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(SolutionData));
                FileStream fileStream = new FileStream(this.filePath, FileMode.Create, FileAccess.Write);
                serializer.Serialize(fileStream, solutionData);
                fileStream.Close();
            }
            catch (Exception)
            {
                return false;
            }

            solutionModified = false;
            showSaveSolutionDialog = false;
            return true;
        }

        internal bool OpenFromFile(string filePath)
        {
            if (null != solutionData) // Close existing if any.
            {
                if (this.CloseSolution() == false)
                    return false;
            }

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(SolutionData));
                FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                solutionData = serializer.Deserialize(fileStream) as SolutionData;
                if (null == solutionData)
                    return false;

                fileStream.Close();
            }
            catch (Exception)
            {
                return false;
            }

            this.filePath = filePath;
            DeserializeFromScriptItems();
            this.solutionModified = false;
            this.showSaveSolutionDialog = false;
            return true;
        }

        #endregion

        #region Public Script Related Methods

        public int GetScriptIndexFromPath(string scriptPath)
        {
            if (string.IsNullOrEmpty(scriptPath) != false)
                return -1;

            int scriptIndex = 0;
            foreach (IScriptObject script in loadedScripts)
            {
                IParsedScript parsed = script.GetParsedScript();
                string path = (null == parsed ? null : parsed.GetScriptPath());

                if (string.IsNullOrEmpty(path) == false)
                {
                    StringComparison comparison = StringComparison.CurrentCultureIgnoreCase;
                    if (scriptPath.Equals(path, comparison))
                        return scriptIndex; // Found the script!
                }

                scriptIndex = scriptIndex + 1;
            }

            return -1;
        }

        public string GetScriptPathFromIndex(int index)
        {
            if (index < 0 || (index >= loadedScripts.Count))
                return string.Empty;

            IParsedScript currentScript = loadedScripts[index].GetParsedScript();
            return ((null != currentScript) ? currentScript.GetScriptPath() : string.Empty);
        }

        internal IScriptObject AddNewScript()
        {
            return AddNewScript("");
        }

        internal IScriptObject AddNewScript(string fileContent)
        {
            solutionModified = true; // Changes should be persisted.
            showSaveSolutionDialog = true;

            IScriptObject script = new ScriptObject(fileContent);
            loadedScripts.Add(script);

            // Notify event subscribers that the script count has changed.
            if (null != ScriptCountChanged)
            {
                ScriptCountChangedEventArgs eventArgs = new ScriptCountChangedEventArgs();
                eventArgs.ScriptsAdded.Add(script);
                ScriptCountChanged(this, eventArgs);
            }

            // Subscribe to the new text buffer event notification.
            ITextBuffer textBuffer = script.GetTextBuffer();
            textBuffer.LinesUpdated += linesUpdatedHandler;
            return script;
        }

        internal IScriptObject OpenScript(string scriptPath)
        {
            solutionModified = true; // Changes should be persisted.
            showSaveSolutionDialog = true;

            if (File.Exists(scriptPath) == false)
                return null; // The file does not exist.

            // If the entire script is so messed up that it cannot even be 
            // parsed in a meaningful way, then "ParseScript" call will fail. 
            // When that happens, we don't return "null" from here, the file 
            // open should continue (and errors be shown as red colored text).
            IParsedScript parsedScript = InterfaceFactory.CreateParsedScript();
            parsedScript.ParseScript(scriptPath);

            // Add the new script to loaded scripts.
            IScriptObject hostScript = new ScriptObject(parsedScript);
            loadedScripts.Add(hostScript);

            // Notify event subscribers that the script count has changed.
            if (null != ScriptCountChanged)
            {
                ScriptCountChangedEventArgs eventArgs = new ScriptCountChangedEventArgs();
                eventArgs.ScriptsAdded.Add(hostScript);
                ScriptCountChanged(this, eventArgs);
            }

            // Subscribe to the new text buffer event notification.
            ITextBuffer textBuffer = hostScript.GetTextBuffer();
            textBuffer.LinesUpdated += linesUpdatedHandler;
            return hostScript;
        }

        internal bool CloseScript(int index)
        {
            solutionModified = true; // Changes should be persisted.
            showSaveSolutionDialog = true;

            if (index < 0 || (index >= loadedScripts.Count))
                throw new InvalidOperationException("Script does not exist!");

            ScriptObject script = loadedScripts[index] as ScriptObject;

            // Unsubscribe from text buffer event notification.
            ITextBuffer textBuffer = script.GetTextBuffer();
            textBuffer.LinesUpdated -= linesUpdatedHandler;

            loadedScripts.RemoveAt(index);
            if (0 == loadedScripts.Count)
                this.activeScript = null;
            if (activeScript == script)
                activeScript = null;

            // Notify event subscribers that the script count has changed.
            if (null != ScriptCountChanged)
            {
                ScriptCountChangedEventArgs eventArgs = new ScriptCountChangedEventArgs();
                eventArgs.ScriptsRemoved.Add(script);
                ScriptCountChanged(this, eventArgs);
            }

            script.DestroyScript();
            return true;
        }

        internal bool ActivateScript(int index)
        {
            solutionModified = true; // Changes should be persisted.

            if (index < 0 || (index >= loadedScripts.Count))
                return false;

            activeScript = loadedScripts[index];
            return true;
        }

        #endregion

        #region Public Breakpoint Related Methods

        public bool InsertBreakpoint(string filePath, int column, int line)
        {
            solutionModified = true; // Changes should be persisted.
            showSaveSolutionDialog = true;

            // Attempt to remove a breakpoint if there's one.
            if (RemoveBreakpoint(filePath, column, line))
                return true;

            BreakpointItem breakpoint = new BreakpointItem();
            breakpoint.AbsolutePath = filePath;
            breakpoint.Line = line;
            breakpoint.Column = column;
            breakpoint.RelativePath = string.Empty;

            solutionData.Breakpoints.Add(breakpoint);
            return (ToggleBreakpoint(breakpoint));
        }

        public void GetBreakpoints(List<CodeRange> breakpoints)
        {
            if (null == activeScript)
                return; // No breakpoints to draw.

            bool isInDebugMode = false;
            if (IsExecutionActive(ref isInDebugMode) != false) // We have a VM!
            {
                if (IsExecutionInProgress() == false) // Don't query otherwise.
                {
                    List<CodeRange> liveBreakpoints = null;
                    if (executionSession.GetBreakpoints(out liveBreakpoints))
                        breakpoints.AddRange(liveBreakpoints);
                }

                return;
            }

            // When this method is called, the breakpoint rendering codes depend 
            // on Solution to tell them where the breakpoints are to be drawn. This 
            // means that there's no VM around for that information. In such cases,
            // we'll simply highlight the entire line for each breakpoint. Note that
            // unlike VM, the Solution does know what the active script is, so it 
            // can always filter out breakpoints that are falling outside of the 
            // current script as this method will be called again when user switches 
            // to another script.
            // 
            string activePath = activeScript.GetParsedScript().GetScriptPath();
            ITextBuffer textBuffer = activeScript.GetTextBuffer();

            foreach (BreakpointItem breakpoint in solutionData.Breakpoints)
            {
                if (string.Compare(activePath, breakpoint.AbsolutePath, true) != 0)
                    continue; // The paths do not match up, next!

                int line = breakpoint.Line;

                CodePoint startInclusive = new CodePoint();
                startInclusive.CharNo = textBuffer.GetFirstNonWhiteSpaceChar(-1, line) + 1;
                startInclusive.LineNo = breakpoint.Line + 1;
                startInclusive.SourceLocation = new CodeFile();
                startInclusive.SourceLocation.FilePath = breakpoint.AbsolutePath;

                int length = textBuffer.GetCharacterCount(line, false, false);
                CodePoint endInclusive = new CodePoint();
                endInclusive.CharNo = length + 1;
                endInclusive.LineNo = line + 1;
                endInclusive.SourceLocation = new CodeFile();
                endInclusive.SourceLocation.FilePath = breakpoint.AbsolutePath;

                CodeRange codeRange = new CodeRange();
                codeRange.StartInclusive = startInclusive;
                codeRange.EndExclusive = endInclusive;
                breakpoints.Add(codeRange);
            }
        }

        internal bool RemoveBreakpoint(string filePath, int column, int line)
        {
            solutionModified = true; // Changes should be persisted.
            showSaveSolutionDialog = true;

            int currDifference = int.MaxValue;
            BreakpointItem toBeRemoved = null;

            List<BreakpointItem> breakpoints = solutionData.Breakpoints;
            foreach (BreakpointItem breakpoint in breakpoints)
            {
                // Integer comparison is faster (than string).
                if (line == breakpoint.Line)
                {
                    // If line matches then only we check on the file path.
                    if (string.Compare(breakpoint.AbsolutePath, filePath, true) == 0)
                    {
                        int difference = Math.Abs(breakpoint.Column - column);
                        if (difference < currDifference)
                        {
                            currDifference = difference;
                            toBeRemoved = breakpoint;
                        }
                    }
                }
            }

            if (null != toBeRemoved)
            {
                breakpoints.Remove(toBeRemoved);
                ToggleBreakpoint(toBeRemoved);
                return true; // A breakpoint was removed.
            }

            return false;
        }

        #endregion

        #region Public Inspection Related Methods

        public bool AddWatchExpressions(string expression)
        {
            Logger.LogInfo("Solution.AddWatchExpressions", expression);

            bool isExpressionRemoved = RemoveWatchExpressions(expression);
            List<ExpressionItem> expressions = solutionData.WatchExpressions;

            ExpressionItem newItem = new ExpressionItem();
            newItem.Content = expression;
            expressions.Add(newItem);

            return isExpressionRemoved;
        }

        public bool RemoveWatchExpressions(string expression)
        {
            Logger.LogInfo("Solution.RemoveWatchExpressions", expression);

            List<ExpressionItem> expressions = solutionData.WatchExpressions;
            foreach (ExpressionItem item in expressions)
            {
                if (string.Compare(expression, item.Content, false) == 0)
                {
                    expressions.Remove(item);
                    return true; // Case sensitive comparison, found in the list.
                }
            }

            return false;
        }

        public void RemoveAllWatchExpressions()
        {
            Logger.LogInfo("Solution.RemoveAllWatchExpressions", "RemoveAllWatchExpressions");

            //Reset the expression list
            List<ExpressionItem> expressions = solutionData.WatchExpressions;
            expressions.RemoveRange(0, expressions.Count);
        }

        public void GetWatchExpressions(List<string> watchExpressions)
        {
            watchExpressions.Clear();
            foreach (ExpressionItem expression in solutionData.WatchExpressions)
                watchExpressions.Add(expression.Content);
        }

        #endregion

        #region Public In Line Message Related Methods

        public void AddInlineMessages(List<ProtoCore.OutputMessage> outputMessages)
        {
            solutionData.InlineMessages.Clear();
            if (outputMessages != null)
            {
                foreach (ProtoCore.OutputMessage message in outputMessages)
                {
                    InlineMessageItem newItem = new InlineMessageItem();
                    newItem.FilePath = message.FilePath;
                    if (string.IsNullOrEmpty(newItem.FilePath))
                        newItem.FilePath = "dummy";
                    newItem.Line = message.Line - 1;
                    newItem.Column = message.Column;
                    newItem.SetType(message.Type);
                    solutionData.InlineMessages.Add(newItem);
                }
            }
        }

        public List<InlineMessageItem> GetInlineMessage()
        {
            return solutionData.InlineMessages;
        }

        #endregion

        #region Public Class Operational Methods

        public static Solution CreateFromFile(string filePath)
        {
            Solution newSolution = new Solution();

            if (newSolution.OpenFromFile(filePath) == false)
                return null;

            // Replace the current solution.
            Solution.currentSolution = newSolution;
            return Solution.currentSolution;
        }

        public static bool CloseSolution(Solution solution, bool discardChanges)
        {

            if (discardChanges == true)
                currentSolution.solutionModified = false;

            if (solution.CloseSolution() == false)
                return false;

            if (currentSolution == solution)
                currentSolution = null;

            return true;
        }

        public IOutputStream GetMessage(bool clearExisting)
        {
            if (null == outputStream)
                outputStream = new EditorOutputStream();
            else if (clearExisting)
                outputStream.Clear();

            return outputStream;
        }

        internal bool HandleLineModification(int line, int affectedLines, int delta)
        {
            if (null == activeScript || (null == solutionData.Breakpoints))
                return false;

            List<BreakpointItem> breakpointsToBeRemoved = null;

            string scriptPath = activeScript.GetParsedScript().GetScriptPath();


            if (solutionData.InlineMessages != null)
            {
                foreach (InlineMessageItem inlineMessage in solutionData.InlineMessages)
                {
                    if (inlineMessage.Line == line)
                    {
                        if (inlineMessage.Type == InlineMessageItem.OutputMessageType.Error)
                            inlineMessage.SetType(InlineMessageItem.OutputMessageType.PossibleError);
                        else if (inlineMessage.Type == InlineMessageItem.OutputMessageType.Warning)
                            inlineMessage.SetType(InlineMessageItem.OutputMessageType.PossibleWarning);
                    }
                }
            }

            if (solutionData.Breakpoints.Count <= 0)
                return false;

            foreach (BreakpointItem breakpoint in solutionData.Breakpoints)
            {
                if (string.Compare(breakpoint.AbsolutePath, scriptPath, true) != 0)
                    continue; // Both paths do not match up, next!

                // Modification is after this breakpoint, so don't have to do 
                // anything with it.
                if (breakpoint.Line < line)
                    continue;

                if (line < breakpoint.Line && breakpoint.Line <= (line + affectedLines + delta))
                {
                    breakpoint.Line += delta;
                    continue;
                }

                if (breakpoint.Line == line)
                {
                    if (!activeScript.GetTextBuffer().IsNullOrWhiteSpace(line))
                        continue;
                }

                // This breakpoint is after the lines being modified, so nothing 
                // should change other than having to offset the line number.
                if (breakpoint.Line > line + affectedLines + delta)
                {
                    breakpoint.Line += delta;
                    continue;
                }

#if false       // This part of code which was removed to solve break point errors

                // The breakpoint that falls through to here is the one that lies 
                // right within the lines that are being modified. There are 
                // basically three cases for the line region that changed: more 
                // lines are added to the region (+ve delta), some lines are 
                // removed from the region (-ve delta), and line numbers are kept 
                // the same (delta = 0).
                // 
                // First we deal with the case where line number remains the same.
                // In this case since no line has been removed, simply move on.
                if (0 == delta)
                    continue;

                // Then we deal with the case where more lines are added to the 
                // region of lines being modified, again, nothing to do here.
                if (delta > 0)
                    continue;

                // Now comes the hard part: few lines are being modified (-ve delta).
                int unaffectedLines = affectedLines + delta;
                int relativeOffset = breakpoint.Line - line;
                if (relativeOffset < unaffectedLines)
                    continue;

#endif

                // Okay this breakpoint needs to be removed, add it to the list.
                if (null == breakpointsToBeRemoved)
                    breakpointsToBeRemoved = new List<BreakpointItem>();

                breakpointsToBeRemoved.Add(breakpoint);
            }

            if (null != breakpointsToBeRemoved) // If we have any to remove at all.
            {
                foreach (BreakpointItem item in breakpointsToBeRemoved)
                    solutionData.Breakpoints.Remove(item);

                return true; // The breakpoint list has changed.
            }

            return false;
        }

        internal bool RunWithoutDebugger()
        {
            bool isInDebugMode = false;
            if (IsExecutionActive(ref isInDebugMode))
                return true; // Already started running!

            IExecutionSession session = this.ExecutionSession;
            session.RunWithoutDebugger();
            return true;
        }

        internal bool RunWithDebugger(RunMode runMode)
        {
            IExecutionSession session = this.ExecutionSession;
            if (session.RunWithDebugger(runMode) == false)
                return false;

            // Returns 'true' if execution is not ended.
            return (false == session.ExecutionEnded);
        }

        internal void FlushBreakpointsToVm()
        {
            if (null != solutionData.Breakpoints)
            {
                foreach (BreakpointItem breakpoint in solutionData.Breakpoints)
                    ToggleBreakpoint(breakpoint);
            }
        }

        internal static Solution CreateTemporary(bool updateCurrent = true)
        {
            Solution newSolution = new Solution();
            if (false != updateCurrent)
                Solution.currentSolution = newSolution;

            return newSolution;
        }

        #endregion

        #region Private Class Helper Methods

        private Solution()
        {
            this.Asynchronous = false;
            solutionData = new SolutionData(this);
            loadedScripts = new List<IScriptObject>();
            linesUpdatedHandler = new LinesUpdatedHandler(OnTextBufferLinesUpdated);
        }

        private bool CloseSolution()
        {
            if (null == solutionData)
                return true;

            if (false != solutionModified)
            {
                if (SaveToFile(null) == false)
                    return false;
            }

            solutionData = null;
            return true;
        }

        private void DeserializeFromScriptItems()
        {
            string directory = Path.GetDirectoryName(this.filePath);
            if (directory.EndsWith("\\") == false)
                directory += "\\";

            string oldDirectory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(directory);

            if (null != ScriptCountChanged)
            {
                // Notify event subscribers that the script count has changed.
                ScriptCountChangedEventArgs eventArgs = new ScriptCountChangedEventArgs();
                eventArgs.ScriptsRemoved.AddRange(loadedScripts);
                ScriptCountChanged(this, eventArgs);
            }

            // Unsubscribe all existing handlers before they are cleared.
            foreach (IScriptObject existingScript in loadedScripts)
            {
                ITextBuffer textBuffer = existingScript.GetTextBuffer();
                textBuffer.LinesUpdated -= linesUpdatedHandler;
            }

            loadedScripts.Clear();
            foreach (ScriptItem scriptItem in solutionData.Scripts)
            {
                string scriptPath = scriptItem.RelativePath;
                IScriptObject scriptObject = OpenScript(Path.GetFullPath(scriptPath));
                if (null != scriptObject && (scriptItem.EntryPoint != false))
                    this.activeScript = scriptObject;
            }

            foreach (BreakpointItem breakpoint in solutionData.Breakpoints)
            {
                string filePath = breakpoint.RelativePath;
                breakpoint.AbsolutePath = Path.GetFullPath(filePath);
            }

            Directory.SetCurrentDirectory(oldDirectory);
        }

        private void SerializeToScriptItems()
        {
            string directory = Path.GetDirectoryName(this.filePath);
            if (directory.EndsWith("\\") == false)
                directory += "\\";

            Uri solutionPath = new Uri(directory, UriKind.Absolute);

            List<ScriptItem> scripts = solutionData.Scripts;
            scripts.Clear();

            // Update each script path relative to the solution file.
            foreach (IScriptObject scriptObject in loadedScripts)
            {
                IParsedScript parsedScript = scriptObject.GetParsedScript();
                if (null != parsedScript)
                {
                    try
                    {
                        Uri scriptPath = new Uri(parsedScript.GetScriptPath(), UriKind.Absolute);
                        Uri relativePath = solutionPath.MakeRelativeUri(scriptPath);

                        ScriptItem script = new ScriptItem();
                        script.RelativePath = relativePath.OriginalString;
                        script.EntryPoint = (activeScript == scriptObject);
                        scripts.Add(script);
                    }
                    catch (Exception exception)
                    {
                        // Invalid file path, deal with it.
                        string message = exception.Message;
                    }
                }
            }

            // Update breakpoint path relative to the solution file.
            foreach (BreakpointItem breakpoint in solutionData.Breakpoints)
            {
                Uri absolutePath = new Uri(breakpoint.AbsolutePath, UriKind.Absolute);
                Uri relativePath = solutionPath.MakeRelativeUri(absolutePath);
                breakpoint.RelativePath = relativePath.OriginalString;
            }
        }

        private bool IsExecutionActive(ref bool isInDebugMode)
        {
            if (null == executionSession)
                return false;

            return executionSession.IsExecutionActive(ref isInDebugMode);
        }

        private bool IsExecutionInProgress()
        {
            if (null == executionSession)
                return false;

            return executionSession.IsBusy;
        }

        private bool ToggleBreakpoint(BreakpointItem breakpoint)
        {
            bool isInDebugMode = false;
            if (IsExecutionActive(ref isInDebugMode) == false)
                return true;
            if (IsExecutionInProgress() != false)
                return false; // Can't set breakpoint now!

            CodePoint codePoint = new CodePoint();
            codePoint.CharNo = breakpoint.Column;
            codePoint.LineNo = breakpoint.Line;
            CodeFile script = new CodeFile();
            script.FilePath = breakpoint.AbsolutePath;
            codePoint.SourceLocation = script;

            return (executionSession.SetBreakpointAt(codePoint));
        }

        private void OnTextBufferLinesUpdated(object sender, LinesUpdateEventArgs e)
        {
            this.HandleLineModification(e.LineIndex, e.AffectedLineCount, e.DeltaLines);
        }

        private void OnExecutionStateChanged(object sender, ExecutionStateChangedEventArgs e)
        {
        }

        #endregion
    }
}
