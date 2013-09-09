
using System.Collections.Generic;
using System.Diagnostics;
using ProtoCore;

namespace ProtoAssociative
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
            bool isLangSignValid = compileState.Langverify.Verify(langBlock);

            if (isLangSignValid)
            {
                try
                {
                    ProtoCore.CodeGen oldCodegen = compileState.assocCodegen;

                    if (ProtoCore.DSASM.InterpreterMode.kNormal == compileState.ExecMode)
                    {
                        if ((compileState.IsParsingPreloadedAssembly || compileState.IsParsingCodeBlockNode) && parentBlock == null)
                        {
                            if (compileState.CodeBlockList.Count == 0)
                            {
                                compileState.assocCodegen = new ProtoAssociative.CodeGen(compileState, parentBlock);
                            }
                            else 
                            {
                                // We reuse the existing toplevel CodeBlockList's for the procedureTable's 
                                // by calling this overloaded constructor - pratapa
                                compileState.assocCodegen = new ProtoAssociative.CodeGen(compileState);
                            }
                        }
                        else
                            compileState.assocCodegen = new ProtoAssociative.CodeGen(compileState, parentBlock);
                    }

                    if (null != compileState.AssocNode)
                    {
                        ProtoCore.AST.AssociativeAST.CodeBlockNode cnode = new ProtoCore.AST.AssociativeAST.CodeBlockNode();
                        cnode.Body.Add(compileState.AssocNode as ProtoCore.AST.AssociativeAST.AssociativeNode);

                        compileState.assocCodegen.context = callContext;

                        blockId = compileState.assocCodegen.Emit((cnode as ProtoCore.AST.AssociativeAST.CodeBlockNode), graphNode);
                    }
                    else
                    {
                        //if not null, Compile has been called from DfsTraverse. No parsing is needed. 
                        if (codeBlockNode == null)
                        {
                            System.IO.MemoryStream memstream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(langBlock.body));
                            ProtoCore.DesignScriptParser.Scanner s = new ProtoCore.DesignScriptParser.Scanner(memstream);
                            ProtoCore.DesignScriptParser.Parser p = new ProtoCore.DesignScriptParser.Parser(s, compileState, compileState.builtInsLoaded);
                            p.Parse();

                            // TODO Jun: Set this flag inside a persistent object
                            compileState.builtInsLoaded = true;

                            codeBlockNode = p.root;

                            //compileState.AstNodeList = p.GetParsedASTList(codeBlockNode as ProtoCore.AST.AssociativeAST.CodeBlockNode);
                            List<ProtoCore.AST.Node> astNodes = ProtoCore.Utils.ParserUtils.GetAstNodes(codeBlockNode);
                            compileState.AstNodeList = astNodes;
                        }
                        else
                        {
                            if (!compileState.builtInsLoaded)
                            {
                                // Load the built-in methods manually
                                ProtoCore.Utils.CoreUtils.InsertPredefinedAndBuiltinMethods(compileState, codeBlockNode, false);
                                compileState.builtInsLoaded = true;
                            }
                        }

                        compileState.assocCodegen.context = callContext;

                        //Temporarily change the code block for code gen to the current block, in the case it is an imperative block
                        //CodeGen for ProtoImperative is modified to passing in the core object.
                        ProtoCore.DSASM.CodeBlock oldCodeBlock = compileState.assocCodegen.codeBlock;
                        if (compileState.ExecMode == ProtoCore.DSASM.InterpreterMode.kExpressionInterpreter)
                        {
                            int tempBlockId = compileState.GetCurrentBlockId();

                            ProtoCore.DSASM.CodeBlock tempCodeBlock = compileState.GetCodeBlock(compileState.CodeBlockList, tempBlockId);
                            while (null != tempCodeBlock && tempCodeBlock.blockType != ProtoCore.DSASM.CodeBlockType.kLanguage)
                            {
                                tempCodeBlock = tempCodeBlock.parent;
                            }
                            compileState.assocCodegen.codeBlock = tempCodeBlock;
                        }
                        compileState.assocCodegen.codeBlock.EventSink = sink;
                        if (compileState.BuildStatus.Errors.Count == 0) //if there is syntax error, no build needed
                        {
                             blockId = compileState.assocCodegen.Emit((codeBlockNode as ProtoCore.AST.AssociativeAST.CodeBlockNode), graphNode);
                        }
                        if (compileState.ExecMode == ProtoCore.DSASM.InterpreterMode.kExpressionInterpreter)
                        {
                            blockId = compileState.assocCodegen.codeBlock.codeBlockId;
                            //Restore the code block.
                            compileState.assocCodegen.codeBlock = oldCodeBlock;
                        }
                    }

                    // @keyu: we have to restore asscoCodegen here. It may be 
                    // reused later on. Suppose for an inline expression 
                    // "x = 1 == 2 ? 3 : 4", we dynamically create assocCodegen
                    // to compile true and false expression in this inline 
                    // expression, and if we don't restore assocCodegen, the pc
                    // is totally messed up.
                    //
                    // But if directly replace with old assocCodegen, will it
                    // replace some other useful information? Need to revisit it.
                    //
                    // Also refer to defect IDE-2120.
                    if (oldCodegen != null && compileState.assocCodegen != oldCodegen)
                    {
                        compileState.assocCodegen = oldCodegen;
                    }
                }
                catch (ProtoCore.BuildHaltException e)
                {
#if DEBUG
                    //compileState.BuildStatus.LogSemanticError(e.errorMsg);
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
                sv = interpreter.Run(codeblock, entry, Language.kAssociative);
            }
            return sv;
        }

        public override ProtoCore.DSASM.StackValue Execute(int codeblock, int entry, ProtoCore.Runtime.Context callContext, System.Collections.Generic.List<ProtoCore.DSASM.Instruction> breakpoints, ProtoCore.DebugServices.EventSink sink = null, bool fepRun = false)
        {
            ProtoCore.DSASM.Interpreter interpreter = new ProtoCore.DSASM.Interpreter(core, fepRun);
            CurrentDSASMExec = interpreter.runtime;
            ProtoCore.DSASM.StackValue sv = interpreter.Run(breakpoints, codeblock, entry, Language.kAssociative);
            return sv;
        }

	}
}

