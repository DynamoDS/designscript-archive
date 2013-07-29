using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Runtime.InteropServices;
using DesignScript.Editor;
using DesignScript.Parser;

namespace DesignScript.Editor.Core
{
    using ProtoScript.Runners;
    using ProtoCore.CodeModel;
    using ProtoScript.Config;
    using ProtoFFI;
    using ProtoCore.DSASM.Mirror;
    using ProtoCore.Exceptions;
    using System.Reflection;
    using ProtoCore;

    // Alright, this is my attempt on explaining how a debug execution starts.
    // A very important point to note is that an execution session can exist
    // way before the main script (entry point script) is actually executed. In 
    // such cases the execution session may not have an instance of DebugRunner
    // created at all. At a much later time (with possible editings prior to 
    // this), an execution session can be started with a call to Start method.
    // At that point the finalized text content is obtained from IHostScript 
    // object, and sent to the DebugRunner instance.
    // 
    class ExecutionSession : IExecutionSession
    {
        VirtualMachineWorker vmWorker = null;
        IHostApplication hostApplication = null;

        public ExecutionSession(IHostApplication hostApplication)
        {
            if (null == hostApplication)
                throw new ArgumentNullException("hostApplication");

            this.hostApplication = hostApplication;
        }

        public IHostApplication HostApplication
        {
            get
            {
                return this.hostApplication;
            }
        }

        #region IExecutionSession Members - Write Operations

        public bool SetBreakpointAt(ProtoCore.CodeModel.CodePoint breakpoint)
        {
            if (null == vmWorker)
                return false;

            return (vmWorker.SetBreakpointAt(breakpoint));
        }

        public bool RunWithoutDebugger()
        {
            if (null != vmWorker)
            {
                bool isInDebugMode = false;
                if (vmWorker.IsExecutionActive(ref isInDebugMode))
                    return false; // There's already an on-going execution now.
                if (false != vmWorker.IsBusy)
                    return false; // There's already an on-going execution now.
            }

            string entryPointCode = GetEntryPointCode();
            if (string.IsNullOrEmpty(entryPointCode))
                return false; // No script to execute.

            string errorMessage = string.Empty;
            IOutputStream messageList = Solution.Current.GetMessage(true);
            if (hostApplication.PreExecutionSetup(false, ref errorMessage) == false)
            {
                messageList.Write(new OutputMessage(OutputMessage.MessageType.Error, errorMessage));
                return false;
            }

            if (null == vmWorker)
            {
                vmWorker = new VirtualMachineWorker();
                vmWorker.ExecutionStateChanged += OnExecutionStateChanged;
            }

            try
            {
                errorMessage = string.Empty;
                if (false != hostApplication.PreStepSetup(ref errorMessage))
                {
                    // We can execute the script now with the lock.
                    if (vmWorker.RunWithoutDebugger(entryPointCode, messageList))
                        return true; // Call successful.

                    // Call has failed here, which means we need to inform the host 
                    // application instead of waiting for "OnExecutionStateChanged".
                    hostApplication.PostStepTearDown();
                    hostApplication.PostExecutionTearDown();
                }
            }
            catch (Exception exception)
            {
            }

            return false;
        }

        public bool RunWithDebugger(RunMode runMode)
        {
            string scriptPath = string.Empty;
            string errorMessage = string.Empty;

            if (null == vmWorker)
            {
                vmWorker = new VirtualMachineWorker();
                vmWorker.ExecutionStateChanged += OnExecutionStateChanged;
            }

            // There is an on-going execution right now...
            bool isInDebugMode = false;
            if (vmWorker.IsExecutionActive(ref isInDebugMode) == false)
            {
                // We only need the script content if the execution has not started.
                scriptPath = GetEntryScriptPath();
                if (string.IsNullOrEmpty(scriptPath) || !File.Exists(scriptPath))
                    return false; // Entry script path not specified or is invalid.

                // Prepare the host application for a new execution.
                if (hostApplication.PreExecutionSetup(true, ref errorMessage) == false)
                {
                    IOutputStream messageList = Solution.Current.GetMessage(false);
                    messageList.Write(new OutputMessage(OutputMessage.MessageType.Error, errorMessage));
                    return false;
                }
            }

            if (false == vmWorker.IsBusy) // Worker isn't busy right now.
            {
                if (false != hostApplication.PreStepSetup(ref errorMessage))
                {
                    if (vmWorker.RunWithDebugger(scriptPath, runMode))
                        return true;

                    // Call has failed here, which means we need to inform the host 
                    // application instead of waiting for "OnExecutionStateChanged".
                    hostApplication.PostStepTearDown();
                    hostApplication.PostExecutionTearDown();
                }

                return false; // Host application isn't quite ready it seems.
            }

            return true; // Call successful.
        }

        public bool StopExecution()
        {
            // When this is called, we'd better have a worker.
            System.Diagnostics.Debug.Assert(null != vmWorker);
            if ((null != vmWorker) && vmWorker.StopExecution())
            {
                hostApplication.PostExecutionTearDown();
                return true;
            }

            return false;
        }

        public bool SetValue(int line, int column, int value)
        {
            return ((null != vmWorker) && vmWorker.SetValue(line, column, value));
        }

        public bool SetValue(int line, int column, double value)
        {
            return ((null != vmWorker) && vmWorker.SetValue(line, column, value));
        }

        #endregion

        #region IExecutionSession Members - Read Only Operations

        public bool IsExecutionActive(ref bool isInDebugMode)
        {
            return ((null != vmWorker) && vmWorker.IsExecutionActive(ref isInDebugMode));
        }

        public bool ExecuteExpression(string expression)
        {
            if ((null == vmWorker) || string.IsNullOrEmpty(expression))
                return false; // Call has failed.

            bool callSucceeded = false;
            string errorMessage = string.Empty;
            if (false != hostApplication.PreStepSetup(ref errorMessage))
            {
                callSucceeded = vmWorker.ExecuteExpression(expression);
                hostApplication.PostStepTearDown();
            }

            return callSucceeded;
        }

        public bool GetExecutionCursor(ref ProtoCore.CodeModel.CodeRange cursor)
        {
            if (null == vmWorker)
                return false;

            return (vmWorker.GetExecutionCursor(ref cursor));
        }

        public bool GetStackValueData(ProtoCore.Lang.Obj stackValue, ref string data)
        {
            data = string.Empty;
            if (null == vmWorker)
                return false;

            return (vmWorker.GetStackValueData(stackValue, ref data));
        }

        public bool GetStackValueType(ProtoCore.Lang.Obj stackValue, ref string type)
        {
            type = string.Empty;
            if (null == vmWorker)
                return false;

            return (vmWorker.GetStackValueType(stackValue, ref type));
        }

        public bool GetStackValue(string expression, out ProtoCore.Lang.Obj value)
        {
            value = null;
            if (null == vmWorker)
                return false;

            return (vmWorker.GetStackValue(expression, out value));
        }

        public bool GetBreakpoints(out List<ProtoCore.CodeModel.CodeRange> breakpoints)
        {
            breakpoints = null;
            if (null == vmWorker)
                return false;

            return (vmWorker.GetBreakpoints(out breakpoints));
        }

        public bool GetArrayElements(ProtoCore.Lang.Obj stackValue, out List<ProtoCore.Lang.Obj> elements)
        {
            elements = null;
            if (null == vmWorker)
                return false;

            return (vmWorker.GetArrayElements(stackValue, out elements));
        }

        public bool GetClassProperties(ProtoCore.Lang.Obj stackValue, out Dictionary<string, ProtoCore.Lang.Obj> properties)
        {
            properties = null;
            if (null == vmWorker)
                return false;

            return (vmWorker.GetClassProperties(stackValue, out properties));
        }

        public bool PopulateInstructions(List<DisassemblyEntry> instructions)
        {
            if (null == vmWorker)
                return false;

            return (vmWorker.PopulateInstructions(instructions));
        }

        #endregion

        #region IExecutionSession Members - Public Properties

        public bool IsBusy
        {
            get { return ((null != vmWorker) && vmWorker.IsBusy); }
        }

        public bool ExecutionEnded
        {
            get { return ((null == vmWorker) || vmWorker.ExecutionEnded); }
        }

        #endregion

        #region Private Class Helper Methods

        private void OnExecutionStateChanged(object sender, ExecutionStateChangedEventArgs e)
        {
            if (e.CurrentState == ExecutionStateChangedEventArgs.States.Debugging)
            {
                // If this is the first time user hits "Run (Debug)", register all breakpoints.
                if (e.PreviousState == ExecutionStateChangedEventArgs.States.None)
                    Solution.Current.FlushBreakpointsToVm();
            }
            else if (e.CurrentState == ExecutionStateChangedEventArgs.States.Paused)
            {
                // The debugger has just gotten to a breakpoint.
                hostApplication.PostStepTearDown();
                TextEditorCore.Instance.UpdateExecutionCursor();
            }
            else if (e.CurrentState == ExecutionStateChangedEventArgs.States.Stopped)
            {
                // The debugging has come to an end.
                hostApplication.PostStepTearDown();
                TextEditorCore.Instance.UpdateExecutionCursor();
            }
            else if (e.CurrentState == ExecutionStateChangedEventArgs.States.None)
            {
                // "Run" to "None" means running without debugger has just ended.
                if (e.PreviousState == ExecutionStateChangedEventArgs.States.Running)
                {
                    hostApplication.PostStepTearDown();
                    hostApplication.PostExecutionTearDown();
                    TextEditorCore.Instance.UpdateExecutionCursor();
                }
                else if (e.PreviousState == ExecutionStateChangedEventArgs.States.Debugging)
                {
                    // 
                }
            }

            if (null != TextEditorCore.Instance.UiExecutionStateChangeHandler)
                TextEditorCore.Instance.UiExecutionStateChangeHandler(this, e);
        }

        private string GetEntryPointCode()
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            Logger.LogInfo("GetEntryPointCode", "Init");

            IScriptObject entryPointScript = Solution.Current.ActiveScript;
            if (null == entryPointScript)
                return String.Empty; // No script to execute.

            ITextBuffer textBuffer = entryPointScript.GetTextBuffer();
            string scriptPath = GetEntryScriptPath();
            if (string.IsNullOrEmpty(scriptPath) || (!File.Exists(scriptPath)))
                return string.Empty;

            string entryPointCode = File.ReadAllText(scriptPath);

            // The virtual machine's parser does not recognize '\n' as a line-break
            // character, so we have to replace them with '\r\n' instead.
            // 

            if (entryPointCode.IndexOf('\r') == -1)
                entryPointCode = entryPointCode.Replace("\n", "\r\n");

            Logger.LogInfo("EntryPointCode", entryPointCode);
            Logger.LogPerf("EntryPointCode", sw.ElapsedMilliseconds.ToString());

            return entryPointCode;
        }

        private string GetEntryScriptPath()
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            Logger.LogInfo("GetEntryScriptPath", "Init");

            IParsedScript parsedScript = null;
            IScriptObject entryPointScript = Solution.Current.ActiveScript;
            if (null != entryPointScript)
                parsedScript = entryPointScript.GetParsedScript();

            string ret = (null == parsedScript ? null : parsedScript.GetScriptPath());

            Logger.LogInfo("GetEntryScriptPath", ret);

            Logger.LogPerf("GetEntryScriptPath", sw.ElapsedMilliseconds + " ms");
            return ret;
        }

        private string TranslateExceptions(Exception ex)
        {
            string type = ex.GetType().ToString();
            switch (type)
            {
                case "ProtoCore.DSASM.Mirror.NameNotFoundException":
                    return "Variable not found in the current scope";
                case "ProtoCore.DSASM.Mirror.UninitializedVariableException":
                    return "Variable has not been initialised yet";
                default:
                    return ex.Message;
            }
        }

        #endregion
    }
}
