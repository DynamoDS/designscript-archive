
// Define this for parallel vm.
// #define RUNVMASYNC

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.Reflection;

namespace DesignScript.Editor.Core
{
    using ProtoScript.Runners;
    using ProtoCore.DSASM.Mirror;
    using ProtoCore.CodeModel;
    using DesignScript.Parser;
    using ProtoFFI;
    using ProtoCore.Exceptions;
    using ProtoCore;
    using ProtoScript.Config;
    using Autodesk.DesignScript.Interfaces;

    public class ExecutionStateChangedEventArgs : EventArgs
    {
        public enum States
        {
            None = 0,
            Running,
            Debugging,
            Paused,
            Stopped
        }

        internal ExecutionStateChangedEventArgs(States previous, States current)
        {
            this.PreviousState = previous;
            this.CurrentState = current;
        }

        public States PreviousState { get; private set; }
        public States CurrentState { get; private set; }
    }

    public delegate void ExecutionStateChangedHandler(
        object sender, ExecutionStateChangedEventArgs e);

    class WorkerInputOutput
    {
        public void ResetStates()
        {
            ExecutionEnded = true;
            ElapsedMilliseconds = 0;
            CurrentVmState = null;
            ExecException = null;
            RegularRunMirror = null;
        }

        public bool ExecutionEnded { get; set; }
        public long ElapsedMilliseconds { get; set; }
        public System.Exception ExecException { get; set; }
        public ExecutionMirror RegularRunMirror { get; set; }
        public DebugRunner.VMState CurrentVmState { get; set; }
        public DesignScript.Editor.Core.RunMode RequestedRunMode { get; set; }
    }

    class VirtualMachineWorker
    {
        #region Private Class Data Members

        // For script execution with debugger attached.
        DebugRunner debugRunner = null;

        // For script execution without debugger attached.
        ProtoLanguage.CompileStateTracker compileState = null;
        ProtoCore.Core core = null;
        ProtoScriptTestRunner scriptRunner = null;

        // For a variable in the watch window.
        ExecutionMirror currentWatchedMirror = null;
        ProtoCore.Lang.Obj currentWatchedStackValue = null;

        // Background worker and its supporting data.
        bool asynchronousExecution = false;
        MemoryStream ByteCodeStream = null;
        WorkerInputOutput workerParams = new WorkerInputOutput();
        BackgroundWorker internalWorker = null;
        ExecutionStateChangedEventArgs.States executionState = ExecutionStateChangedEventArgs.States.None;

        #endregion

        #region Public Methods - Write Operations

        internal bool SetBreakpointAt(ProtoCore.CodeModel.CodePoint breakpoint)
        {
            if ((null != internalWorker) && internalWorker.IsBusy)
                return false;

            // The script is executed without a debugger attached, 
            // therefore this call does not make sense right now.
            if (null != scriptRunner)
                return false;

            if (null == debugRunner)
                return false; // Not debugging, cannot set a breakpoint.

            CodePoint location = breakpoint;
            location.CharNo = location.CharNo + 1;
            location.LineNo = location.LineNo + 1;
            return (debugRunner.ToggleBreakpoint(location));
        }

        internal bool RunWithoutDebugger(string scriptCode, IOutputStream outputStream)
        {
            // The caller should make sure before calling this!
            System.Diagnostics.Debug.Assert(false == this.IsBusy);
            if (false != this.IsBusy)
                return false;

            if (!SetupRegularRunner())
                return false;

            SetExecutionState(ExecutionStateChangedEventArgs.States.Running);

            if (false == this.asynchronousExecution) // Normal execution requested.
            {
                RunWithoutDebuggerGuts(scriptCode);
                RunWithoutDebuggerFinalize();
            }
            else // Asynchronous execution requested.
            {
                if (null == internalWorker)
                {
                    internalWorker = new BackgroundWorker();
                    internalWorker.DoWork += new DoWorkEventHandler(RunWithoutDebuggerTask);
                    internalWorker.RunWorkerCompleted += RunWithoutDebuggerTaskCompleted;
                }

                internalWorker.RunWorkerAsync(scriptCode);
            }

            return true;
        }

        internal bool RunWithDebugger(string scriptPath, RunMode runMode)
        {
            // The caller should make sure before calling this!
            System.Diagnostics.Debug.Assert(false == this.IsBusy);
            if (false != this.IsBusy)
                return false;

            // The script is executed without a debugger attached, 
            // therefore this call does not make sense right now.
            if (null != scriptRunner)
                return false;

            Logger.LogInfo("RunWithDebugger", "RunMode: " + runMode.ToString());

            if (Solution.Current.ActiveScript != null)
            {
                ITextBuffer textBuffer = Solution.Current.ActiveScript.GetTextBuffer();
                if (null != textBuffer)
                    Logger.LogInfo("TextEditorControl-Run-Script", textBuffer.GetContent());
            }

            workerParams.ExecutionEnded = true;
            workerParams.RequestedRunMode = runMode;

            if (null == debugRunner)
            {
                // If debug session has not started, attempt to attach to the debugger. 
                // If that fails, then we aren't quite ready for debugging, just yet.
                if (this.SetupDebugRunner(scriptPath) == false)
                    return false; // Fail to start debugger for some reason.

                // Okay the first press of "Run (Debug)" button does nothing but 
                // single step through the script. So if this is the first press 
                // of "Run (Debug)" button, then turn it into a single-step instead.
                // 
                workerParams.RequestedRunMode = RunMode.StepNext;
            }

            workerParams.ExecutionEnded = false;
            SetExecutionState(ExecutionStateChangedEventArgs.States.Debugging);

            if (false == this.asynchronousExecution) // Normal execution requested.
            {
                RunWithDebuggerGuts();
                RunWithDebuggerFinalized();
                return ((null != workerParams.CurrentVmState) && (!workerParams.ExecutionEnded));
            }
            else // Asynchronous execution requested.
            {
                if (null == internalWorker)
                {
                    internalWorker = new BackgroundWorker();
                    internalWorker.DoWork += new DoWorkEventHandler(RunWithDebuggerTask);
                    internalWorker.RunWorkerCompleted += RunWithDebuggerTaskCompleted;
                }

                // Place the application into a debugging mode.
                internalWorker.RunWorkerAsync();
                return true; // Always return 'true' when it gets here.
            }
        }

        internal bool StopExecution()
        {
            // @TODO(Ben): We may want to cancel the worker 
            // thread when VM supports cancellation.
            if ((null != internalWorker) && internalWorker.IsBusy)
                return false;

            CleanUpRunners();
            return true;
        }

        internal bool SetValue(int line, int column, int value)
        {
            if ((null != internalWorker) && internalWorker.IsBusy)
                return false;
            if (workerParams.CurrentVmState == null)
                return false;

            if (workerParams.CurrentVmState.mirror != null)
                workerParams.CurrentVmState.mirror.UpdateValue(line, column, value);

            return true;
        }

        internal bool SetValue(int line, int column, double value)
        {
            if ((null != internalWorker) && internalWorker.IsBusy)
                return false;
            if (workerParams.CurrentVmState == null)
                return false;

            if (workerParams.CurrentVmState.mirror != null)
                workerParams.CurrentVmState.mirror.UpdateValue(line, column, value);

            return true;
        }

        #endregion

        #region Public Methods - Read Only Operations

        internal bool IsExecutionActive(ref bool isInDebugMode)
        {
            if ((null != internalWorker) && internalWorker.IsBusy)
                return true; // Yep, active for sure.

            isInDebugMode = (null != debugRunner);
            return (null != debugRunner || (null != scriptRunner));
        }

        internal bool ExecuteExpression(string expression)
        {
            Logger.LogInfo("ExecuteExpression", expression);
            return InterpretExpression(expression);
        }

        internal bool GetExecutionCursor(ref ProtoCore.CodeModel.CodeRange cursor)
        {
            if ((null != internalWorker) && internalWorker.IsBusy)
                return false;

            // The script is executed without a debugger attached, 
            // therefore this call does not make sense right now.
            if (null != scriptRunner)
                return false;

            // There is no execution cursor, either the runner is 
            // not created, or created without having been executed.
            if (null == debugRunner || (null == workerParams.CurrentVmState))
                return false;

            // The old behavior demands that if we have a valid VMState, 
            // then we should use the cursor information it provides us.
            cursor = workerParams.CurrentVmState.ExecutionCursor;
            return true;
        }

        internal bool GetStackValueData(ProtoCore.Lang.Obj stackValue, ref string data)
        {
            data = string.Empty;
            if ((null != internalWorker) && internalWorker.IsBusy)
                return false;

            if (null == currentWatchedMirror)
                return false;

            try
            {
                ITextEditorSettings editorSettings = TextEditorCore.Instance.TextEditorSettings;
                data = currentWatchedMirror.GetStringValue(stackValue.DsasmValue,
                    core.Heap, 0, editorSettings.MaxArrayDisplaySize, editorSettings.MaxOutputDepth);
            }
            catch (Exception exception)
            {
                HandleException(exception);
                return false;
            }

            return true;
        }

        internal bool GetStackValueType(ProtoCore.Lang.Obj stackValue, ref string type)
        {
            type = string.Empty;
            if ((null != internalWorker) && internalWorker.IsBusy)
                return false;

            if (null == currentWatchedMirror)
                return false;

            try
            {
                type = currentWatchedMirror.GetType(stackValue);
            }
            catch (Exception exception)
            {
                HandleException(exception);
                return false;
            }

            return true;
        }

        internal bool GetStackValue(string expression, out ProtoCore.Lang.Obj value)
        {
            value = null;
            if (InterpretExpression(expression) != false)
                value = currentWatchedStackValue;

            return true; // Call is successful.
        }

        internal bool GetBreakpoints(out List<ProtoCore.CodeModel.CodeRange> breakpoints)
        {
            breakpoints = null;
            if ((null != internalWorker) && internalWorker.IsBusy)
                return false; // Sorry but we're kinda busy right now.

            // The script is executed without a debugger attached, 
            // therefore this call does not make sense right now.
            if (null == debugRunner || (null != scriptRunner))
                return false;

            breakpoints = new List<CodeRange>();
            foreach (DebugRunner.Breakpoint breakpoint in debugRunner.RegisteredBreakpoints)
                breakpoints.Add(breakpoint.Location);

            return true;
        }

        internal bool GetArrayElements(ProtoCore.Lang.Obj stackValue, out List<ProtoCore.Lang.Obj> elements)
        {
            elements = null; // Invalidate output argument.
            if ((null != internalWorker) && internalWorker.IsBusy)
                return false; // Sorry but we're kinda busy right now.

            try
            {
                if (null != currentWatchedMirror)
                    elements = currentWatchedMirror.GetArrayElements(stackValue);
            }
            catch (Exception exception)
            {
                HandleException(exception);
                return false;
            }

            return true; // Call is successful.
        }

        internal bool GetClassProperties(ProtoCore.Lang.Obj stackValue, out Dictionary<string, ProtoCore.Lang.Obj> properties)
        {
            properties = null;
            if ((null != internalWorker) && internalWorker.IsBusy)
                return false; // Sorry but we're kinda busy right now.

            try
            {
                if (null != currentWatchedMirror)
                    properties = currentWatchedMirror.GetProperties(stackValue, true);
            }
            catch (Exception exception)
            {
                HandleException(exception);
                return false;
            }

            return true; // Call's successful even when there's no mirror.
        }

        internal bool PopulateInstructions(List<DisassemblyEntry> instructions)
        {
            if ((null != internalWorker) && internalWorker.IsBusy)
                return false; // Sorry but we're kinda busy right now.

            if (null == ByteCodeStream)
            {
                return false;
            }

            var byteCodeStream = new MemoryStream(this.ByteCodeStream.ToArray());
            StreamReader reader = new StreamReader(byteCodeStream);
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                instructions.Add(new DisassemblyEntry() { InstructionString = line });
            }
            reader.Close();

            return true;
        }

        #endregion

        #region Public Class Properties

        internal bool IsBusy { get { return ((null != internalWorker) && internalWorker.IsBusy); } }

        internal bool ExecutionEnded { get { return workerParams.ExecutionEnded; } }

        #endregion

        #region Internal Class Events

        // This event handler is meant for internal DLL consumption only :)
        internal event ExecutionStateChangedHandler ExecutionStateChanged;

        #endregion

        #region Private Class Helper Methods

        private void RunWithDebuggerTask(object sender, DoWorkEventArgs e)
        {
            RunWithDebuggerGuts();
        }

        private void RunWithDebuggerTaskCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RunWithDebuggerFinalized();
        }

        private void RunWithoutDebuggerTask(object sender, DoWorkEventArgs e)
        {
            RunWithoutDebuggerGuts(e.Argument as string);
        }

        private void RunWithoutDebuggerTaskCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RunWithoutDebuggerFinalize();
        }

        private bool SetupRegularRunner()
        {
            if ((null != internalWorker) && internalWorker.IsBusy)
                return false;

            Logger.LogInfo("SetupRegularRunner", "Init");
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            SetupCommonRunner(false);
            this.scriptRunner = new ProtoScriptTestRunner();

            Logger.LogPerf("SetupRegularRunner", sw.ElapsedMilliseconds + " ms");
            return true;
        }

        private bool SetupDebugRunner(string scriptPath)
        {
            if (string.IsNullOrEmpty(scriptPath))
                return false; // No script to execute.
            if (File.Exists(scriptPath) == false)
                return false; // File does not exist.

            Logger.LogInfo("SetupDebugRunner", "Init");
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            SetupCommonRunner(true);

            RunConfiguration runnerConfig = new RunConfiguration();
            runnerConfig.IsParrallel = false;

#if DEBUG
            var verboseFlag = this.core.Options.Verbose;
            var prevAsmOutput = this.core.AsmOutput;

            this.core.Options.Verbose = true;

            this.ByteCodeStream = new MemoryStream();
            this.core.AsmOutput = new StreamWriter(this.ByteCodeStream);
#endif

            debugRunner = new DebugRunner(this.core);
            bool startSucceeded = debugRunner.LoadAndPreStart(scriptPath, runnerConfig);

#if DEBUG
            this.core.Options.Verbose = verboseFlag;
            this.core.AsmOutput.Close();
            this.core.AsmOutput = prevAsmOutput == null ? Console.Out : prevAsmOutput;
#endif

            Logger.LogPerf("SetupDebugRunner", sw.ElapsedMilliseconds + " ms");
            return startSucceeded;
        }

        private void SetupCommonRunner(bool inDebugMode)
        {
            var options = new ProtoCore.Options();
            options.Verbose = GetDisplayOutput();
            options.ExecutionMode = ProtoCore.ExecutionMode.Serial;
            options.SuppressBuildOutput = false;
            options.IDEDebugMode = inDebugMode;
            AppendSearchPaths(options);

            this.core = new ProtoCore.Core(options);
            this.core.CurrentDSFileName = options.RootModulePathName;
            this.core.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(this.core));
            this.core.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(this.core));

            // Get configuration from host app
            var configurations  = Solution.Current.ExecutionSession.HostApplication.Configurations;
            if (configurations != null)
            {
                foreach (var item in configurations)
                {
                    this.core.Configurations[item.Key] = item.Value;                   
                }
            }

            IOutputStream messageList = Solution.Current.GetMessage(false);
            this.core.RuntimeStatus.MessageHandler = messageList;

            workerParams.RegularRunMirror = null;
            DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());
            asynchronousExecution = Solution.Current.Asynchronous;
        }

        private void CleanUpRunners()
        {
            Logger.LogInfo("DetachFromDebugger", "Detach");

            // Clear up all watch related data members.
            this.workerParams.CurrentVmState = null;
            this.currentWatchedMirror = null;
            this.currentWatchedStackValue = null;

            if (null != scriptRunner)
                scriptRunner = null;

            if (null != debugRunner)
            {
                debugRunner.Shutdown();
                debugRunner = null;
            }

            if (null != internalWorker)
            {
                // @TODO(Ben): Perhaps cancellation is needed?
                internalWorker = null;
            }

            //Fix DG-1464973 Sprint25: rev 3444 : Multiple import issue - dispose core after execution
            //Core Cleanup should happen only after execution has finished,
            //DebugRunner is ShutDown.
            if (null != this.core)
            {
                this.core.Cleanup();
                this.core = null;
            }

            workerParams.ResetStates();
            SetExecutionState(ExecutionStateChangedEventArgs.States.None);
        }

        private void RunWithoutDebuggerGuts(string scriptCode) // On background thread, no UI access.
        {
            var timer = new System.Diagnostics.Stopwatch();
            timer.Start();

            try
            {
                workerParams.RegularRunMirror = scriptRunner.Execute(scriptCode, core, out compileState);
            }
            catch (System.Exception exception)
            {
                workerParams.ExecException = exception;
            }
            finally
            {
                timer.Stop();
                workerParams.ElapsedMilliseconds = timer.ElapsedMilliseconds;
            }
        }

        private void RunWithoutDebuggerFinalize() // On main UI thread.
        {
            IOutputStream outputStream = Solution.Current.GetMessage(false);
            if (null != workerParams.ExecException)
            {
                System.Exception ex = workerParams.ExecException;
                switch (ex.GetType().ToString())
                {
                    case "ProtoCore.Exceptions.CompileErrorsOccured":
                        Logger.LogInfo("ExecuteFromCode-CompileErrorsOccured", ex.Message);
                        /*
                        outputStream.Write(new ProtoCore.OutputMessage(OutputMessage.MessageType.Error, ex.Message));
                        */
                        break;

                    default:
                        Logger.LogInfo("ExecuteFromCode-CompileErrorsOccured-Ex", ex.Message);
                        outputStream.Write(new ProtoCore.OutputMessage(OutputMessage.MessageType.Error, ex.Message));
                        break;
                }
            }

            DisplayCoreDump(workerParams.RegularRunMirror, outputStream);

            Logger.LogPerf("ExecuteFromCode-ms", workerParams.ElapsedMilliseconds.ToString());
            string elapsed = string.Format("Time elapsed: {0} ms", workerParams.ElapsedMilliseconds);
            outputStream.Write(new ProtoCore.OutputMessage(OutputMessage.MessageType.Info, elapsed));
            CleanUpRunners();
        }

        private void RunWithDebuggerGuts() // On background thread, no UI access.
        {
            try
            {
                switch (workerParams.RequestedRunMode)
                {
                    case RunMode.RunTo:
                        workerParams.CurrentVmState = debugRunner.Run();
                        break;

                    case RunMode.StepNext:
                        workerParams.CurrentVmState = debugRunner.StepOver();
                        break;

                    case RunMode.StepIn:
                        workerParams.CurrentVmState = debugRunner.Step();
                        break;

                    case RunMode.StepOut:
                        workerParams.CurrentVmState = debugRunner.StepOut();
                        break;

                    default:
                        {
                            string runMode = workerParams.RequestedRunMode.ToString();
                            throw new InvalidOperationException("Unsupported RunMode: " + runMode);
                        }
                }
            }
            catch (Exception exception)
            {
                switch (exception.GetType().ToString())
                {
                    case "ProtoScript.Runners.DebugRunner+EndofScriptException":
                    case "ProtoScript.Runners.DebugRunner.EndofScriptException":
                    case "ProtoScript.Runners.DebugRunner+RunnerNotInitied":
                        workerParams.ExecutionEnded = true;
                        break;

                    default:
                        workerParams.ExecException = exception;
                        break;
                }
            }
        }

        private void RunWithDebuggerFinalized() // On main UI thread.
        {
            if (null != workerParams.ExecException)
            {
                IDialogProvider dialogProvider = TextEditorCore.DialogProvider;
                if (null != dialogProvider)
                    dialogProvider.ShowExceptionDialog(workerParams.ExecException);
            }

            if (false == workerParams.ExecutionEnded)
            {
                if (null != workerParams.CurrentVmState)
                {
                    if (false != workerParams.CurrentVmState.isEnded)
                    {
                        IOutputStream messageList = Solution.Current.GetMessage(false);
                        DisplayCoreDump(workerParams.CurrentVmState.mirror, messageList);
                        SetExecutionState(ExecutionStateChangedEventArgs.States.Stopped);
                    }
                    else
                    {
                        SetExecutionState(ExecutionStateChangedEventArgs.States.Paused);
                    }
                }
            }
            else
            {
                // Done with the script.
                CleanUpRunners();
            }
        }

        private void SetExecutionState(ExecutionStateChangedEventArgs.States state)
        {
            ExecutionStateChangedEventArgs.States oldState = this.executionState;
            this.executionState = state;

            if (null != ExecutionStateChanged)
            {
                ExecutionStateChangedEventArgs args = null;
                args = new ExecutionStateChangedEventArgs(oldState, state);
                ExecutionStateChanged(this, args);
            }
        }

        private bool GetDisplayOutput()
        {
            return (TextEditorCore.Instance.TextEditorSettings.DisplayOutput);
        }

        private ProtoCore.Lang.Obj ExecExecutiveExpression(ExpressionInterpreterRunner exprInterpreter, string expression)
        {
            if (expression.Equals("@pc"))
            {
                int pc = core.CurrentExecutive.CurrentDSASMExec.PC;
                ProtoCore.DSASM.StackValue svpc = ProtoCore.DSASM.StackUtils.BuildInt(pc);
                return new ProtoCore.Lang.Obj(svpc) { Payload = pc, Type = new Type { UID = (int)PrimitiveType.kTypeInt, IsIndexable = false } };
            }
            else if (expression.StartsWith("@rc(") && expression.EndsWith(")"))
            {
                /*
                string subexpression = expression.Substring(4, expression.Length - 5);
                var watchMirror = exprInterpreter.Execute(subexpression);
                if (null != watchMirror)
                {
                    ProtoCore.Lang.Obj value = watchMirror.GetWatchValue();
                    if (value != null)
                    {
                        int rc = watchMirror.GetReferenceCount(value);
                        ProtoCore.DSASM.StackValue rcValue = ProtoCore.DSASM.StackUtils.BuildInt(rc);
                        return new ProtoCore.Lang.Obj(rcValue) { Payload = rc, Type = new Type { UID = (int)PrimitiveType.kTypeInt, IsIndexable = false } };
                    }
                }
                */
            }

            return null;
        }

        private bool InterpretExpression(string expression)
        {
            if ((null != internalWorker) && internalWorker.IsBusy)
                return false; // Sorry but we're kinda busy right now.

            if (string.IsNullOrEmpty(expression))
                return false;

            Logger.LogInfo("InterpretExpression", expression);
            if (null == this.core || (null == debugRunner))
                return false; // Only in debug mode.

            currentWatchedStackValue = null;
            ExpressionInterpreterRunner exprInterpreter = null;

            IOutputStream runtimeStream = core.RuntimeStatus.MessageHandler;

            // Disable output before interpret expression.
            core.RuntimeStatus.MessageHandler = null;
            exprInterpreter = new ExpressionInterpreterRunner(this.core);

            try
            {
                currentWatchedStackValue = ExecExecutiveExpression(exprInterpreter, expression);
                if (currentWatchedStackValue == null)
                {
                    currentWatchedMirror = exprInterpreter.Execute(expression);
                    if (null != currentWatchedMirror)
                    {
                        currentWatchedStackValue = currentWatchedMirror.GetWatchValue();
                    }
                }
            }
            catch (ProtoCore.Exceptions.CompileErrorsOccured compileError)
            {
            }
            catch (Exception exception)
            {
            }

            // Re-enable output after execution is done.
            core.RuntimeStatus.MessageHandler = runtimeStream;
            return (null != currentWatchedStackValue);
        }

        private void DisplayCoreDump(ExecutionMirror executionMirror, IOutputStream outputStream)
        {
            if (null == executionMirror)
                return; // This can be "null" if there's compilation errors.

            ITextEditorSettings editorSettings = TextEditorCore.Instance.TextEditorSettings;
            if (false == editorSettings.DisplayOutput)
                return; // The output display has been disabled, don't display.

            try
            {
                List<string> variableList = null;
                executionMirror.GetCoreDump(out variableList,
                    editorSettings.MaxArrayDisplaySize,
                    editorSettings.MaxOutputDepth);

                if (null != variableList)
                {
                    foreach (string variable in variableList)
                    {
                        outputStream.Write(new ProtoCore.OutputMessage(
                            OutputMessage.MessageType.Info, variable));

                        Logger.LogPerf("CoreDump-ExecSession", variable);
                    }
                }
            }
            catch (System.Exception ex)
            {
                outputStream.Write(new ProtoCore.OutputMessage(OutputMessage.MessageType.Error, ex.Message));
                Logger.LogPerf("CoreDumpEx", ex.Message);

            }
        }

        private void AppendSearchPaths(ProtoCore.Options options)
        {
            string assemblyPath = Assembly.GetAssembly(typeof(ExecutionSession)).Location;
            options.IncludeDirectories.Add(Path.GetDirectoryName(assemblyPath));

            ITextEditorSettings editorSettings = TextEditorCore.Instance.TextEditorSettings;
            if (Directory.Exists(editorSettings.IncludePath))
                options.IncludeDirectories.Add(editorSettings.IncludePath);

            IScriptObject entryPointScript = Solution.Current.ActiveScript;
            if (null != entryPointScript)
            {
                IParsedScript parsedScript = entryPointScript.GetParsedScript();
                if (null != parsedScript)
                {
                    string scriptPath = parsedScript.GetScriptPath();
                    options.RootModulePathName = parsedScript.GetScriptPath();

                    if (string.IsNullOrEmpty(scriptPath) == false)
                    {
                        string directoryName = Path.GetDirectoryName(scriptPath);
                        options.IncludeDirectories.Add(directoryName);
                    }
                }
            }
        }

        private void HandleException(Exception exception)
        {
            IDialogProvider dialogProvider = TextEditorCore.DialogProvider;
            if (null != dialogProvider)
                dialogProvider.ShowExceptionDialog(exception);
        }

        #endregion
    }
}
