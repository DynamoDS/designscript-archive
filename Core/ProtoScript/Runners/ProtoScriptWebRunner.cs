using System;
using System.Text;
using System.Collections.Generic;
using ProtoCore.DSASM.Mirror;
using System.Diagnostics;
using ProtoCore.Utils;


namespace ProtoScript.Runners
{
    public class ProtoScriptWebRunner
    {
        public ProtoCore.DebugServices.EventSink EventSink = new ProtoCore.DebugServices.ConsoleEventSink();

        public ProtoScriptWebRunner()
        {

        }

        public ExecutionMirror LoadAndExecute(string fileName, ProtoCore.Core core, bool isTest = true)
        {
            string codeContent = string.Empty;

            try
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(fileName, Encoding.UTF8, true))
                {
                    codeContent = reader.ReadToEnd();
                }
            }
            catch (System.IO.IOException)
            {
                throw new FatalError("Cannot open file " + fileName);
            }

            //Start the timer       
            core.StartTimer();
            core.CurrentDSFileName = ProtoCore.Utils.FileUtils.GetFullPathName(fileName);
            core.Options.RootModulePathName = core.CurrentDSFileName;

            Execute(codeContent, core);
            if (!core.Options.CompileToLib && (null != core.CurrentExecutive))
                return new ExecutionMirror(core.CurrentExecutive.CurrentDSASMExec, core);

            return null;
        }

        private ExecutionMirror Execute(string code, ProtoCore.Core core, bool isTest = true)
        {
            int blockId = ProtoCore.DSASM.Constants.kInvalidIndex;
            ProtoLanguage.CompileStateTracker compileState = Compile(code, out blockId);
            Validity.Assert(null != compileState);
            if (compileState.compileSucceeded)
            {
                // This is the boundary between compilestate and runtime core
                // Generate the executable
                compileState.GenerateExecutable();

                // Get the executable from the compileState
                core.DSExecutable = compileState.DSExecutable;

                core.Rmem.PushGlobFrame(compileState.GlobOffset);
                core.RunningBlock = blockId;
                Execute(core);

                if (!isTest) { core.Heap.Free(); }


                if (isTest && !core.Options.CompileToLib)
                    return new ExecutionMirror(core.CurrentExecutive.CurrentDSASMExec, core);
                else
                    return null;
            }
            //else
            //  throw new ProtoCore.Exceptions.CompileErrorsOccured();

            //if (isTest && !core.Options.CompileToLib)
            //    return new ExecutionMirror(core.CurrentExecutive.CurrentDSASMExec, core);
            //else
            return null;
        }

        private void Execute(ProtoCore.Core core)
        {
            try
            {
                foreach (ProtoCore.DSASM.CodeBlock codeblock in core.CodeBlockList)
                {
                    ProtoCore.Runtime.Context context = new ProtoCore.Runtime.Context();

                    int locals = 0;
                    core.Bounce(codeblock.codeBlockId, codeblock.instrStream.entrypoint, context, new ProtoCore.DSASM.StackFrame(core.GlobOffset), locals, EventSink);
                }
            }
            catch
            {
                throw;
            }
        }

        private ProtoLanguage.CompileStateTracker Compile(string code, out int blockId)
        {
            ProtoLanguage.CompileStateTracker compileState = ProtoScript.CompilerUtils.BuildDefaultCompilerState();

            compileState.ExecMode = ProtoCore.DSASM.InterpreterMode.kNormal;

            blockId = ProtoCore.DSASM.Constants.kInvalidIndex;
            try
            {
                // No More HashAngleReplace for unified parser (Fuqiang)
                //String strSource = ProtoCore.Utils.LexerUtils.HashAngleReplace(code);    

                //defining the global Assoc block that wraps the entire .ds source file
                ProtoCore.LanguageCodeBlock globalBlock = new ProtoCore.LanguageCodeBlock();
                globalBlock.language = ProtoCore.Language.kAssociative;
                globalBlock.body = code;
                //the wrapper block can be given a unique id to identify it as the global scope
                globalBlock.id = ProtoCore.LanguageCodeBlock.OUTERMOST_BLOCK_ID;


                //passing the global Assoc wrapper block to the compiler
                ProtoCore.CompileTime.Context context = new ProtoCore.CompileTime.Context();
                ProtoCore.Language id = globalBlock.language;
                compileState.Executives[id].Compile(compileState, out blockId, null, globalBlock, context, EventSink);

                compileState.BuildStatus.ReportBuildResult();

                int errors = 0;
                int warnings = 0;
                compileState.compileSucceeded = compileState.BuildStatus.GetBuildResult(out errors, out warnings);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            return compileState;
        }
    }
}
