using System.Collections.Generic;

namespace ProtoCore
{
	public abstract class Executive
	{
        protected Core core;

        public Executive()
        {
            this.core = null;
        }

        public Executive(ProtoCore.Core core)
		{
            System.Diagnostics.Debug.Assert(core != null);
            this.core = core;
		}

        public ProtoCore.DSASM.Executive CurrentDSASMExec { get; set; }
        public abstract bool Compile(ProtoLanguage.CompileStateTracker compileState, out int blockId, ProtoCore.DSASM.CodeBlock parentBlock, ProtoCore.LanguageCodeBlock codeblock, ProtoCore.CompileTime.Context callContext, ProtoCore.DebugServices.EventSink sink = null, ProtoCore.AST.Node codeBlockNode = null, ProtoCore.AssociativeGraph.GraphNode graphNode = null);
        public abstract ProtoCore.DSASM.StackValue Execute(int codeblock, int entry, ProtoCore.Runtime.Context callContext, ProtoCore.DebugServices.EventSink sink = null);
        public abstract ProtoCore.DSASM.StackValue Execute(int codeblock, int entry, ProtoCore.Runtime.Context callContext, List<ProtoCore.DSASM.Instruction> breakpoints, ProtoCore.DebugServices.EventSink sink = null, bool fepRun = false);

	}
}

 