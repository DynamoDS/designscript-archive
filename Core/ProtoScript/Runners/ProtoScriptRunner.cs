
using System;
using System.Collections.Generic;
using ProtoCore.Utils;

namespace ProtoScript.Runners
{
    [Obsolete("ProtoScriptRunner has been deprecated in favor of ProtoScripTestRunner")]
    public class ProtoScriptRunner
    {
        public ProtoCore.DebugServices.EventSink EventSink = new ProtoCore.DebugServices.ConsoleEventSink();

        private string PromptAndInsertInput()
        {
            const string sPromptInput = "DS (q' to quit) > ";
            System.Console.Write(sPromptInput);
            string inString = System.Console.ReadLine();
            System.Console.Write("\n");
            return inString;
        }

        public ProtoLanguage.CompileStateTracker Compile(string code, out int blockId)
        {
            ProtoLanguage.CompileStateTracker compileState = ProtoScript.CompilerUtils.BuildDefaultCompilerState();

            blockId = ProtoCore.DSASM.Constants.kInvalidIndex;
            try
            {
                System.IO.MemoryStream sourceMemStream = new System.IO.MemoryStream(System.Text.Encoding.Default.GetBytes(code));
                ProtoScript.GenerateScript gs = new ProtoScript.GenerateScript(compileState);

                compileState.Script = gs.preParseFromStream(sourceMemStream);

                foreach (ProtoCore.LanguageCodeBlock codeblock in compileState.Script.codeblockList)
                {
                    ProtoCore.CompileTime.Context context = new ProtoCore.CompileTime.Context();
                    ProtoCore.Language id = codeblock.language;

                    compileState.Executives[id].Compile(compileState, out blockId, null, codeblock, context, EventSink);
                }

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

        public void Execute(ProtoCore.Core core)
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


        public void Execute(string code, ProtoCore.Core core)
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

                // Setup the initial size of the global stack
                core.Rmem.PushGlobFrame(compileState.GlobOffset);
                core.RunningBlock = blockId;

                Execute(core);
                core.Heap.Free();


                //core.GenerateExecutable();
                //core.Rmem.PushGlobFrame(core.GlobOffset);
                //core.RunningBlock = blockId;
                //Execute(core);
                //core.Heap.Free();
            }
            else
            {
                throw new ProtoCore.Exceptions.CompileErrorsOccured();
            }
        }

        public void LoadAndExecute(string pathFilename, ProtoCore.Core core)
        {
            System.IO.StreamReader reader = null;
            try
            {
                reader = new System.IO.StreamReader(pathFilename);
            } 
            catch (System.IO.IOException) 
            {
			    throw new FatalError("Cannot open file " + pathFilename);
		    }


            string strSource = reader.ReadToEnd();
            reader.Dispose();

            if (EventSink != null && EventSink.BeginDocument != null)
            {
                EventSink.BeginDocument.Invoke("Started executing script: " + pathFilename + "\n");
            }

            Execute(strSource, core);

            if (EventSink != null && EventSink.EndDocument != null)
            {
                EventSink.EndDocument.Invoke("Done executing script: " + pathFilename + "\n");
            }
        }
    }
}
