
using System.Collections.Generic;
using System.Diagnostics;

namespace ProtoImperative
{
	public class Executive : ProtoCore.Executive
    {
        public Executive()
        {
        }

        public Executive(ProtoCore.Core core)
            : base(core)
		{
		}

        public override bool Compile(ProtoLanguage.CompileStateTracker compileState, out int blockId, ProtoCore.DSASM.CodeBlock parentBlock, ProtoCore.LanguageCodeBlock langBlock, ProtoCore.CompileTime.Context callContext, ProtoCore.DebugServices.EventSink sink, ProtoCore.AST.Node codeBlockNode, ProtoCore.AssociativeGraph.GraphNode graphNode = null)
        {
            Debug.Assert(langBlock != null);
            blockId = ProtoCore.DSASM.Constants.kInvalidIndex;

            bool buildSucceeded = false;
            bool isLanguageSignValid = isLanguageSignValid = compileState.Langverify.Verify(langBlock);

            if (isLanguageSignValid)
            {
                try
                {
                    ProtoImperative.CodeGen codegen = new ProtoImperative.CodeGen(compileState, parentBlock);

                    codegen.context = callContext;
                    codegen.codeBlock.EventSink = sink;
                    blockId = codegen.Emit(codeBlockNode as ProtoCore.AST.ImperativeAST.CodeBlockNode, graphNode);
                }
                catch (ProtoCore.BuildHaltException e)
                {
#if DEBUG
                    //core.BuildStatus.LogSemanticError(e.errorMsg);
#endif
                }

                int errors = 0;
                int warnings = 0;
                buildSucceeded = compileState.BuildStatus.GetBuildResult(out errors, out warnings);
            }
            return buildSucceeded;
        }

        public override ProtoCore.DSASM.StackValue Execute(int codeblock, int entry, ProtoCore.Runtime.Context callContext, ProtoCore.DebugServices.EventSink sink)
        {
            ProtoCore.DSASM.StackValue sv = new ProtoCore.DSASM.StackValue();
            if (!core.Options.CompileToLib)
            {
                ProtoCore.DSASM.Interpreter interpreter = new ProtoCore.DSASM.Interpreter(core);
                CurrentDSASMExec = interpreter.runtime;
                sv = interpreter.Run(codeblock, entry, ProtoCore.Language.kImperative);
                return sv;
            }

            return new ProtoCore.DSASM.StackValue();
        }


        public override ProtoCore.DSASM.StackValue Execute(int codeblock, int entry, ProtoCore.Runtime.Context callContext, List<ProtoCore.DSASM.Instruction> breakpoints, ProtoCore.DebugServices.EventSink sink, bool fepRun = false)
        {
            ProtoCore.DSASM.Interpreter interpreter = new ProtoCore.DSASM.Interpreter(core);
            CurrentDSASMExec = interpreter.runtime;
            return interpreter.Run(breakpoints, codeblock, entry, ProtoCore.Language.kImperative);
        }

	}
}

