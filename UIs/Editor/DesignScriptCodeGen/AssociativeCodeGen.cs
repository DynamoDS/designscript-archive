using System;
using ProtoCore;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.Exceptions;

namespace DesignScript.Editor.CodeGen
{
    class AssociativeCodeGen : CoreCodeGen
    {
        private bool ignoreRankCheck;
        private List<AssociativeNode> astNodes;

        private ProtoCore.DSASM.AssociativeCompilePass compilePass;

        // Jun Comment: 'setConstructorStartPC' a flag to check if the graphnode pc needs to be adjusted by -1
        // This is because constructors auto insert an allocc instruction before any cosntructo body is traversed
        private bool setConstructorStartPC;

        public AssociativeCodeGen(ProtoLanguage.CompileStateTracker compileState, ProtoCore.DSASM.CodeBlock parentBlock = null)
            : base(compileState, parentBlock)
        {
            classOffset = 0;

            //  either of these should set the console to flood
            //
            ignoreRankCheck = false;

            astNodes = new List<AssociativeNode>();
            setConstructorStartPC = false;

            // Create a new symboltable for this block
            // Set the new symbol table's parent
            // Set the new table as a child of the parent table

            codeBlock = new ProtoCore.DSASM.CodeBlock(
                ProtoCore.DSASM.CodeBlockType.kLanguage,
                ProtoCore.Language.kAssociative,
                compileState.CodeBlockIndex,
                new ProtoCore.DSASM.SymbolTable("associative lang block", compileState.RuntimeTableIndex),
                new ProtoCore.DSASM.ProcedureTable(compileState.RuntimeTableIndex),
                false,
                compileState);

            ++compileState.CodeBlockIndex;
            ++compileState.RuntimeTableIndex;

            compileState.CodeBlockList.Add(codeBlock);
            if (null != parentBlock)
            {
                // This is a nested block
                parentBlock.children.Add(codeBlock);
                codeBlock.parent = parentBlock;
            }

            compilePass = ProtoCore.DSASM.AssociativeCompilePass.kClassName;
        }

        private void UpdateType(string ident, int funcIndex, ProtoCore.Type dataType)
        {
            int symbolindex = ProtoCore.DSASM.Constants.kInvalidIndex;
            ProtoCore.DSASM.SymbolNode node = null;

            if ((int)ProtoCore.DSASM.Constants.kInvalidIndex != globalClassIndex)
            {
                symbolindex = compileState.ClassTable.ClassNodes[globalClassIndex].symbols.IndexOf(ident, globalClassIndex, funcIndex);
                if ((int)ProtoCore.DSASM.Constants.kInvalidIndex != symbolindex)
                {
                    node = compileState.ClassTable.ClassNodes[globalClassIndex].symbols.symbolList[symbolindex];
                }
            }
            else
            {
                ProtoCore.DSASM.CodeBlock searchBlock = codeBlock;

                symbolindex = searchBlock.symbolTable.IndexOf(ident, globalClassIndex, funcIndex);
                while (ProtoCore.DSASM.Constants.kInvalidIndex == symbolindex)
                {
                    ProtoCore.DSASM.CodeBlock parentBlock = searchBlock.parent;
                    if (null == parentBlock)
                        return;

                    searchBlock = parentBlock;
                    symbolindex = searchBlock.symbolTable.IndexOf(ident, globalClassIndex, funcIndex);
                }
                node = searchBlock.symbolTable.symbolList[symbolindex];
            }
            if (node != null)
                node.datatype = dataType;
        }

        private ProtoCore.DSASM.SymbolNode Allocate(
            int classIndex,  // In which class table this variable will be allocated to ?
            int classScope,  // Variable's class scope. For example, it is a variable in base class
            int funcIndex,   // In which function this variable is defined? 
            string ident,
            ProtoCore.Type datatype,
            int datasize = (int)ProtoCore.DSASM.Constants.kPrimitiveSize,
            bool isStatic = false,
            ProtoCore.DSASM.AccessSpecifier access = ProtoCore.DSASM.AccessSpecifier.kPublic,
            ProtoCore.DSASM.MemoryRegion region = ProtoCore.DSASM.MemoryRegion.kMemStack
            )
        {
            bool allocateForBaseVar = classScope < classIndex;

            ProtoCore.DSASM.SymbolNode symbolnode = new ProtoCore.DSASM.SymbolNode();
            symbolnode.name = ident;
            symbolnode.size = datasize;
            symbolnode.functionIndex = funcIndex;
            symbolnode.datatype = datatype;
            symbolnode.staticType = compileState.TypeSystem.BuildTypeObject((int)PrimitiveType.kTypeVar, false, -2);
            symbolnode.isArgument = false;
            symbolnode.memregion = region;
            symbolnode.classScope = classScope;
            symbolnode.runtimeTableIndex = codeBlock.symbolTable.runtimeIndex;
            symbolnode.isStatic = isStatic;
            symbolnode.access = access;

            int symbolindex = ProtoCore.DSASM.Constants.kInvalidIndex;
            if ((int)ProtoCore.DSASM.Constants.kInvalidIndex != classIndex)
            {
                // NOTE: the following comment and code is OBSOLETE - member
                // variable is not supported now
                // 
                // Yu Ke: it is possible that class table contains same-named 
                // symbols if a class inherits some member variables from base 
                // class, so we need to check name + class index + function 
                // index.
                // 
                //if (core.classTable.list[classIndex].symbols.IndexOf(ident, classIndex, funcIndex) != (int)ProtoCore.DSASM.Constants.kInvalidIndex)
                //    return null;

                symbolindex = compileState.ClassTable.ClassNodes[classIndex].symbols.IndexOf(ident);
                if (symbolindex != (int)ProtoCore.DSASM.Constants.kInvalidIndex)
                {
                    ProtoCore.DSASM.SymbolNode node = compileState.ClassTable.ClassNodes[classIndex].symbols.symbolList[symbolindex];
                    if (node.functionIndex == ProtoCore.DSASM.Constants.kGlobalScope &&
                        funcIndex == ProtoCore.DSASM.Constants.kGlobalScope)
                        return null;
                }

                symbolindex = compileState.ClassTable.ClassNodes[classIndex].symbols.Append(symbolnode);
                if (symbolindex == ProtoCore.DSASM.Constants.kInvalidIndex)
                {
                    return null;
                }

                if (isStatic)
                {
                    Debug.Assert(funcIndex == ProtoCore.DSASM.Constants.kGlobalScope);
                    ProtoCore.DSASM.SymbolNode staticSymbolnode = new ProtoCore.DSASM.SymbolNode();
                    staticSymbolnode.name = ident;
                    staticSymbolnode.size = datasize;
                    staticSymbolnode.functionIndex = funcIndex;
                    staticSymbolnode.datatype = datatype;
                    staticSymbolnode.staticType = compileState.TypeSystem.BuildTypeObject((int)PrimitiveType.kTypeVar, false, -2);
                    staticSymbolnode.isArgument = false;
                    staticSymbolnode.memregion = region;
                    staticSymbolnode.classScope = classScope;
                    staticSymbolnode.runtimeTableIndex = codeBlock.symbolTable.runtimeIndex;
                    staticSymbolnode.isStatic = isStatic;
                    staticSymbolnode.access = access;

                    // If inherits a static property from base class, that propery
                    // symbol should have been added to code block's symbol table,
                    // so we just update symbolTableIndex 
                    int staticSymbolindex = codeBlock.symbolTable.IndexOf(ident, classScope);
                    if (staticSymbolindex == ProtoCore.DSASM.Constants.kInvalidIndex)
                    {
                        AllocateVar(staticSymbolnode);
                        staticSymbolindex = codeBlock.symbolTable.Append(staticSymbolnode);
                        if (staticSymbolindex == ProtoCore.DSASM.Constants.kInvalidIndex)
                        {
                            return null;
                        }
                        staticSymbolnode.symbolTableIndex = staticSymbolindex;
                    }
                    symbolnode.symbolTableIndex = staticSymbolindex;
                }
                else
                {
                    AllocateVar(symbolnode);
                }
            }
            else
            {
                AllocateVar(symbolnode);
                symbolindex = codeBlock.symbolTable.Append(symbolnode);
                if (symbolindex == ProtoCore.DSASM.Constants.kInvalidIndex)
                {
                    return null;
                }
                symbolnode.symbolTableIndex = symbolindex;
            }

            // TODO Jun: Set the symbol table index of the first local variable of 'funcIndex'
            if (null != localProcedure && null == localProcedure.firstLocal)
            {
                localProcedure.firstLocal = symbolnode.index;
            }


            if (ProtoCore.DSASM.Constants.kInvalidIndex == symbolindex)
            {
                return null;
            }
            return symbolnode;
        }

        private int AllocateArg(
            string ident,
            int funcIndex,
            ProtoCore.Type datatype,
            int size = 1,
            int datasize = (int)ProtoCore.DSASM.Constants.kPrimitiveSize,
            AssociativeNode nodeArray = null,
            ProtoCore.DSASM.MemoryRegion region = ProtoCore.DSASM.MemoryRegion.kMemStack)
        {
            ProtoCore.DSASM.SymbolNode node = new ProtoCore.DSASM.SymbolNode(
                ident,
                ProtoCore.DSASM.Constants.kInvalidIndex,
                ProtoCore.DSASM.Constants.kInvalidIndex,
                funcIndex,
                datatype,
                compileState.TypeSystem.BuildTypeObject((int)PrimitiveType.kTypeVar, false, -2),
                size,
                datasize,
                true,
                codeBlock.symbolTable.runtimeIndex,
                region,
                false,
                null,
                globalClassIndex);

            node.name = ident;
            node.size = datasize;
            node.functionIndex = funcIndex;
            node.datatype = datatype;
            node.isArgument = true;
            node.memregion = ProtoCore.DSASM.MemoryRegion.kMemStack;
            node.classScope = globalClassIndex;

            // Comment Jun: The local count will be adjusted and all dependent symbol offsets after the function body has been traversed
            int locOffset = 0;

            // This will be offseted by the local count after locals have been allocated
            node.index = -1 - ProtoCore.DSASM.StackFrame.kStackFrameSize - (locOffset + argOffset);
            ++argOffset;

            int symbolindex = ProtoCore.DSASM.Constants.kInvalidIndex;
            if ((int)ProtoCore.DSASM.Constants.kInvalidIndex != globalClassIndex)
            {
                symbolindex = compileState.ClassTable.ClassNodes[globalClassIndex].symbols.Append(node);
            }
            else
            {
                symbolindex = codeBlock.symbolTable.Append(node);
            }
            return symbolindex;
        }

        private void InferTypes()
        {
            int size = astNodes.Count;
            for (int n = 0; n < size; ++n)
            {
                if (astNodes[n] is BinaryExpressionNode)
                {
                    ProtoCore.Type type = new ProtoCore.Type();
                    type.UID = (int)ProtoCore.PrimitiveType.kTypeVoid;
                    type.IsIndexable = false;

                    BinaryExpressionNode b = astNodes[n] as BinaryExpressionNode;
                    InferDFSTraverse(b.RightNode, ref type);

                    // Do we even need to update lhs?
                    Debug.Assert(b.LeftNode is IdentifierNode);
                    IdentifierNode t = b.LeftNode as IdentifierNode;

                    codeBlock.symbolTable.SetDataType(t.Name, globalProcIndex, type);
                }
            }
        }

        private void InferDFSTraverse(AssociativeNode node, ref ProtoCore.Type inferedType)
        {
            if (node is IdentifierNode)
            {
                IdentifierNode t = node as IdentifierNode;
                if (compileState.TypeSystem.IsHigherRank(t.datatype.UID, inferedType.UID))
                {
                    ProtoCore.Type type = new ProtoCore.Type();
                    type.UID = t.datatype.UID;
                    type.IsIndexable = false;
                    inferedType = type;
                }
            }
            else if (node is FunctionCallNode)
            {
                FunctionCallNode f = node as FunctionCallNode;
            }
            else if (node is BinaryExpressionNode)
            {
                BinaryExpressionNode b = node as BinaryExpressionNode;
                InferDFSTraverse(b.LeftNode, ref inferedType);
                InferDFSTraverse(b.RightNode, ref inferedType);
            }
        }

        public override void TraverseFunctionCall(ProtoCore.AST.Node node, ProtoCore.AST.Node parentNode, int lefttype, int depth, ref ProtoCore.Type inferedType, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone)
        {
            FunctionCallNode funcCall = node as FunctionCallNode;
            string procName = funcCall.Function.Name;
            List<ProtoCore.Type> arglist = new List<ProtoCore.Type>();
            ProtoCore.Type dotCallType = new ProtoCore.Type();
            dotCallType.UID = (int)PrimitiveType.kTypeVar;
            dotCallType.IsIndexable = false;
            if (procName == "%dot")
            {
                if (funcCall.FormalArguments[0] is IdentifierNode)
                {
                    int ci = compileState.ClassTable.IndexOf((funcCall.FormalArguments[0] as IdentifierNode).Name);
                    if (ci != ProtoCore.DSASM.Constants.kInvalidIndex)
                    {
                        funcCall.FormalArguments[0] = new IntNode() { value = ci.ToString() };
                        dotCallType.UID = ci;
                    }

                    if (funcCall.FormalArguments.Count > 4)
                    {
                        int dynamicRhsIndex = int.Parse((funcCall.FormalArguments[1] as IntNode).value);
                        compileState.DynamicFunctionTable.functionTable[dynamicRhsIndex].classIndex = globalClassIndex;
                    }
                    else if (subPass != ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier)
                    {
                        ProtoCore.DSASM.SymbolNode symbolnode = null;
                        bool isAccessible = false;
                        bool isAllocated = VerifyAllocation((funcCall.FormalArguments[0] as IdentifierNode).Name, globalClassIndex, out symbolnode, out isAccessible);
                        if (isAllocated && symbolnode.datatype.UID != (int)PrimitiveType.kTypeVar)
                        {
                            int rhsIndex = int.Parse((funcCall.FormalArguments[1] as IntNode).value);
                            string rhsName = compileState.DynamicVariableTable.variableTable[rhsIndex].variableName;
                            isAllocated = VerifyAllocation(rhsName, symbolnode.datatype.UID, out symbolnode, out isAccessible);
                            if (null == symbolnode)    //unbound identifier
                            {
                                if (isAllocated && !isAccessible)
                                {
                                    ProtoCore.Type leftType = new ProtoCore.Type();
                                    leftType.UID = (int)PrimitiveType.kInvalidType;
                                    leftType.IsIndexable = false;
                                    bool isFirstIdent = true;
                                    DfsEmitIdentList(funcCall.FormalArguments[0], null, globalClassIndex, ref leftType, ref depth, ref inferedType, false, ref isFirstIdent, graphNode, subPass);
                                    inferedType.UID = (int)PrimitiveType.kTypeNull;
                                    return;
                                }
                            }
                        }
                    }
                }
                else if (funcCall.FormalArguments[0] is IntNode)
                {
                    dotCallType.UID = int.Parse((funcCall.FormalArguments[0] as IntNode).value);
                }
            }

            foreach (AssociativeNode paramNode in funcCall.FormalArguments)
            {
                ProtoCore.Type paramType = new ProtoCore.Type();
                paramType.UID = (int)ProtoCore.PrimitiveType.kTypeVoid;
                paramType.IsIndexable = false;

                //emitReplicationGuide = paramNode is IdentifierNode || paramNode is ExprListNode;

                // If it's a binary node then continue type check, otherwise disable type check and just take the type of paramNode itself
                // f(1+2.0) -> type check enabled - param is typed as double
                // f(2) -> type check disabled - param is typed as int
                enforceTypeCheck = !(paramNode is BinaryExpressionNode);

                DfsTraverse(paramNode, ref paramType, false, graphNode, subPass);
                enforceTypeCheck = true;

                arglist.Add(paramType);
            }

            if (subPass == ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier)
            {
                return;
            }

            ProtoCore.DSASM.ProcedureNode procNode = null;
            int type = ProtoCore.DSASM.Constants.kInvalidIndex;
            bool isConstructor = false;
            bool isStatic = false;
            bool hasLogError = false;

            int refClassIndex = ProtoCore.DSASM.Constants.kInvalidIndex;
            if (parentNode != null && parentNode is ProtoCore.AST.AssociativeAST.IdentifierListNode)
            {
                ProtoCore.AST.Node leftnode = (parentNode as ProtoCore.AST.AssociativeAST.IdentifierListNode).LeftNode;
                if (leftnode != null && leftnode is ProtoCore.AST.AssociativeAST.IdentifierNode)
                {
                    refClassIndex = compileState.ClassTable.IndexOf(leftnode.Name);
                }
            }

            // If lefttype is a valid class then check if calling a constructor
            if ((int)ProtoCore.PrimitiveType.kInvalidType != inferedType.UID && (int)ProtoCore.PrimitiveType.kTypeVoid != inferedType.UID)
            {
                bool isAccessible;
                int realType;
                if (inferedType.UID > (int)PrimitiveType.kMaxPrimitives)
                {
                    //check if it is function pointer
                    ProtoCore.DSASM.SymbolNode symbolnode = null;
                    bool isAllocated = VerifyAllocation(procName, inferedType.UID, out symbolnode, out isAccessible);
                    if (isAllocated)
                    {
                        procName = ProtoCore.DSASM.Constants.kFunctionPointerCall;
                    }
                }
                if (procName != ProtoCore.DSASM.Constants.kFunctionPointerCall)
                {
                    procNode = compileState.ClassTable.ClassNodes[inferedType.UID].GetMemberFunction(procName, arglist, globalClassIndex, out isAccessible, out realType);

                    if (procNode != null)
                    {
                        Debug.Assert(realType != ProtoCore.DSASM.Constants.kInvalidIndex);
                        isConstructor = procNode.isConstructor;
                        isStatic = procNode.isStatic;
                        type = lefttype = realType;

                        if ((refClassIndex != ProtoCore.DSASM.Constants.kInvalidIndex) && !procNode.isStatic && !procNode.isConstructor)
                        {
                            inferedType.UID = (int)PrimitiveType.kTypeNull;
                            return;
                        }

                        if (!isAccessible)
                        {
                            type = lefttype = realType;
                            procNode = null;
                            hasLogError = true;
                        }
                    }
                }
            }

            if (!isConstructor && !isStatic && procName != ProtoCore.DSASM.Constants.kFunctionPointerCall)
            {
                //check if it is function pointer
                bool isAccessibleFp;
                ProtoCore.DSASM.SymbolNode symbolnode = null;
                bool isAllocated = VerifyAllocation(procName, globalClassIndex, out symbolnode, out isAccessibleFp);
                if (isAllocated) // not checking the type against function pointer, as the type could be var
                {
                    procName = ProtoCore.DSASM.Constants.kFunctionPointerCall;
                }
                else
                {
                    procNode = compileState.GetFirstVisibleProcedure(procName, arglist, codeBlock);
                    if (null != procNode)
                    {
                        if ((int)ProtoCore.DSASM.Constants.kInvalidIndex != procNode.procId)
                        {
                            // A global function
                            if (compileState.TypeSystem.IsHigherRank((int)procNode.returntype.UID, inferedType.UID))
                            {
                                inferedType = procNode.returntype;
                            }
                        }
                        else
                        {
                            procNode = null;
                        }
                    }
                    else
                    {
                        type = (lefttype != ProtoCore.DSASM.Constants.kInvalidIndex) ? lefttype : globalClassIndex;
                        if (type != ProtoCore.DSASM.Constants.kInvalidIndex)
                        {
                            int realType;
                            bool isAccessible;
                            ProtoCore.DSASM.ProcedureNode memProcNode = compileState.ClassTable.ClassNodes[type].GetMemberFunction(procName, arglist, globalClassIndex, out isAccessible, out realType);

                            if (memProcNode != null)
                            {
                                Debug.Assert(realType != ProtoCore.DSASM.Constants.kInvalidIndex);
                                procNode = memProcNode;
                                inferedType = procNode.returntype;
                                type = realType;

                                if ((refClassIndex != ProtoCore.DSASM.Constants.kInvalidIndex) && !procNode.isStatic && !procNode.isConstructor)
                                {
                                    inferedType.UID = (int)PrimitiveType.kTypeNull;
                                    return;
                                }

                                if (!isAccessible)
                                {
                                    procNode = null;
                                    if (!hasLogError)
                                    {
                                        hasLogError = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (null != procNode)
            {
                //if call is replication call
                if (procNode.isThisCallReplication)
                {
                    inferedType.IsIndexable = true;
                    inferedType.rank++;
                }

                if ((int)ProtoCore.DSASM.Constants.kInvalidIndex != procNode.procId)
                {
                    if (dotCallType.UID != (int)PrimitiveType.kTypeVar)
                    {
                        inferedType.UID = dotCallType.UID;
                    }
                }
            }
            else
            {
                if (depth <= 0 && procName != ProtoCore.DSASM.Constants.kFunctionPointerCall)
                {
                    // At compile time we only check if function name and the number of parameter are
                    // both matched, so if it is a global function call but we still can't get procNode,
                    // it is hopeless to resolve that at run-time. Then we should return null. -yu ke
                    inferedType.UID = (int)PrimitiveType.kTypeNull;
                }
                else
                {
                    if (procName == ProtoCore.DSASM.Constants.kFunctionPointerCall && depth == 0)
                    {
                        ProtoCore.DSASM.DynamicFunctionNode dynamicFunctionNode = new ProtoCore.DSASM.DynamicFunctionNode(procName, arglist, lefttype);
                        compileState.DynamicFunctionTable.functionTable.Add(dynamicFunctionNode);
                        IdentifierNode iNode = new IdentifierNode()
                        {
                            Value = funcCall.Function.Name,
                            Name = funcCall.Function.Name,
                            datatype = compileState.TypeSystem.BuildTypeObject(PrimitiveType.kTypeVar, false)
                        };
                        EmitIdentifierNode(iNode, ref inferedType);
                    }
                    else
                    {
                        ProtoCore.DSASM.DynamicFunctionNode dynamicFunctionNode = new ProtoCore.DSASM.DynamicFunctionNode(funcCall.Function.Name, arglist, lefttype);
                        compileState.DynamicFunctionTable.functionTable.Add(dynamicFunctionNode);
                    }
                    inferedType.UID = (int)PrimitiveType.kTypeVar;
                }
            }
        }

        public void Emit(ProtoCore.AST.AssociativeAST.DependencyTracker tracker)
        {
            // TODO Jun: Only HydrogenRunner uses this. Consider removing if HydrogenRunner becomes redundant
            throw new NotSupportedException();
        }

        public int Emit(CodeBlockNode codeblocknode)
        {
            bool isTopBlock = null == codeBlock.parent;
            if (!isTopBlock)
            {
                // If this is an inner block where there can be no classes, we can start at parsing at the global function state
                compilePass = ProtoCore.DSASM.AssociativeCompilePass.kGlobalFuncSig;
            }

            if (codeBlock.parent != null)
                CodeRangeTable.AddCodeBlockRangeEntry(codeBlock.codeBlockId,
                    codeblocknode.line,
                    codeblocknode.col,
                    codeblocknode.endLine,
                    codeblocknode.endCol,
                    compileState.CurrentDSFileName);
            else
                CodeRangeTable.AddCodeBlockRangeEntry(codeBlock.codeBlockId,
                    0, 0, int.MaxValue, int.MaxValue, compileState.CurrentDSFileName);

            ProtoCore.Type inferedType = new ProtoCore.Type();
            while (ProtoCore.DSASM.AssociativeCompilePass.kDone != compilePass)
            {
                foreach (AssociativeNode node in codeblocknode.Body)
                {
                    inferedType = new ProtoCore.Type();
                    inferedType.UID = (int)ProtoCore.PrimitiveType.kTypeVar;
                    inferedType.IsIndexable = false;

                    //
                    // TODO Jun:    Handle stand alone language blocks
                    //              Integrate the subPass into a proper pass
                    //
                    if (node is LanguageBlockNode)
                    {
                        // Build a binaryn node with a temporary lhs for every stand-alone language block
                        IdentifierNode iNode = new IdentifierNode()
                        {
                            Value = ProtoCore.DSASM.Constants.kTempLangBlock,
                            Name = ProtoCore.DSASM.Constants.kTempLangBlock,
                            datatype = compileState.TypeSystem.BuildTypeObject(PrimitiveType.kTypeVar, false)
                        };
                        BinaryExpressionNode langBlockNode = new BinaryExpressionNode();
                        langBlockNode.LeftNode = iNode;
                        langBlockNode.Optr = ProtoCore.DSASM.Operator.assign;
                        langBlockNode.RightNode = node;

                        DfsTraverse(langBlockNode, ref inferedType, false, null, ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier);
                        //DfsTraverse(node, ref inferedType, false, null, ProtoCore.DSASM.AssociativeSubCompilePass.kNone);
                    }
                    else
                    {
                        DfsTraverse(node, ref inferedType, false, null, ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier);
                    }
                }
                compilePass++;

                // We have compiled all classes
                if (compilePass == ProtoCore.DSASM.AssociativeCompilePass.kGlobalScope && isTopBlock)
                {
                    EmitFunctionCallToInitStaticProperty(codeblocknode.Body);
                }
            }
            compileState.InferedType = inferedType;

            return codeBlock.codeBlockId;
        }

        private void EmitFunctionCallToInitStaticProperty(List<AssociativeNode> codeblock)
        {
            List<AssociativeNode> functionCalls = new List<AssociativeNode>();
            for (int i = 0; i < compileState.ClassTable.ClassNodes.Count; ++i)
            {
                var classNode = compileState.ClassTable.ClassNodes[i];
                if (classNode.vtable != null &&
                    classNode.vtable.procList.Exists(procNode => procNode.name == ProtoCore.DSASM.Constants.kStaticPropertiesInitializer && procNode.isStatic))
                {
                    functionCalls.Add(new BinaryExpressionNode()
                    {
                        LeftNode = new IdentifierNode()
                        {
                            Value = ProtoCore.DSASM.Constants.kTempFunctionReturnVar,
                            Name = ProtoCore.DSASM.Constants.kTempFunctionReturnVar,
                            datatype = compileState.TypeSystem.BuildTypeObject(PrimitiveType.kTypeVar, false)
                        },

                        Optr = ProtoCore.DSASM.Operator.assign,

                        RightNode = new IdentifierListNode()
                        {
                            LeftNode = new IdentifierNode() { Value = classNode.name, Name = classNode.name },
                            Optr = ProtoCore.DSASM.Operator.dot,
                            RightNode = new FunctionCallNode()
                            {
                                Function = new IdentifierNode()
                                {
                                    Value = ProtoCore.DSASM.Constants.kStaticPropertiesInitializer,
                                    Name = ProtoCore.DSASM.Constants.kStaticPropertiesInitializer
                                },
                                FormalArguments = new List<AssociativeNode>(),
                            }
                        }
                    });
                }
            }

            codeblock.InsertRange(0, functionCalls);
        }

        public int AllocateMemberVariable(int classIndex, int classScope, string name, ProtoCore.Type datatype, ProtoCore.DSASM.AccessSpecifier access = ProtoCore.DSASM.AccessSpecifier.kPublic, bool isStatic = false)
        {
            // TODO Jun: Create a class table for holding the primitive and custom data types
            int datasize = ProtoCore.DSASM.Constants.kPointerSize;
            ProtoCore.DSASM.SymbolNode symnode = Allocate(classIndex, classScope, ProtoCore.DSASM.Constants.kGlobalScope, name, datatype, datasize, isStatic, access);
            if (null == symnode)
            {
                return ProtoCore.DSASM.Constants.kInvalidIndex;
            }
            return symnode.symbolTableIndex;
        }

        public int AllocateTypedMemberVariable(int classIndex, int classScope, string name, ProtoCore.Type datatype, ProtoCore.DSASM.AccessSpecifier access = ProtoCore.DSASM.AccessSpecifier.kPublic, bool isStatic = false)
        {
            // TODO Jun: Create a class table for holding the primitive and custom data types
            int datasize = ProtoCore.DSASM.Constants.kPointerSize;
            ProtoCore.DSASM.SymbolNode symnode = Allocate(classIndex, classScope, ProtoCore.DSASM.Constants.kGlobalScope, name, datatype, datasize, isStatic, access);
            if (null == symnode)
            {
                return ProtoCore.DSASM.Constants.kInvalidIndex;
            }

            symnode.SetStaticType(datatype);
            return symnode.symbolTableIndex;
        }

        private void EmitIdentifierNode(AssociativeNode node, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone)
        {
            IdentifierNode t = node as IdentifierNode;

            int dimensions = 0;

            ProtoCore.DSASM.SymbolNode symbolnode = null;
            int runtimeIndex = codeBlock.symbolTable.runtimeIndex;

            ProtoCore.Type type = new ProtoCore.Type();
            type.UID = (int)ProtoCore.PrimitiveType.kTypeVoid;
            type.IsIndexable = false;

            bool isAccessible = false;

            if (null == t.ArrayDimensions)
            {
                //check if it is a function instance
                ProtoCore.DSASM.ProcedureNode procNode = null;
                procNode = compileState.GetFirstVisibleProcedure(t.Name, null, codeBlock);
                if (null != procNode)
                {
                    if ((int)ProtoCore.DSASM.Constants.kInvalidIndex != procNode.procId)
                    {
                        // A global function
                        inferedType.IsIndexable = false;
                        inferedType.UID = (int)PrimitiveType.kTypeFunctionPointer;
                        if (ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier != subPass)
                        {
                            int fptr = compileState.FunctionPointerTable.functionPointerDictionary.Count;
                            ProtoCore.DSASM.FunctionPointerNode fptrNode = new ProtoCore.DSASM.FunctionPointerNode(procNode.procId, procNode.runtimeIndex);
                            compileState.FunctionPointerTable.functionPointerDictionary.TryAdd(fptr, fptrNode);
                            compileState.FunctionPointerTable.functionPointerDictionary.TryGetBySecond(fptrNode, out fptr);
                        }
                        return;
                    }
                }
            }

            bool isAllocated = VerifyAllocation(t.Name, globalClassIndex, out symbolnode, out isAccessible);

            if (ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier == subPass)
            {
                if (!isAllocated || !isAccessible)
                {
                    inferedType.UID = (int)ProtoCore.PrimitiveType.kTypeNull;
                    ProtoCore.Type nullType = new ProtoCore.Type();
                    nullType.UID = (int)PrimitiveType.kTypeNull;
                    nullType.IsIndexable = false;

                    // TODO Jun: Refactor Allocate() to just return the symbol node itself
                    ProtoCore.DSASM.SymbolNode symnode = Allocate(globalClassIndex, globalClassIndex, globalProcIndex, t.Value, nullType);
                    Debug.Assert(symnode != null);
                    IdentLocation.AddEntry(symnode, t.line, t.col, compileState.CurrentDSFileName);
                    int symbolindex = symnode.symbolTableIndex;
                    if ((int)ProtoCore.DSASM.Constants.kInvalidIndex != globalClassIndex)
                    {
                        symbolnode = compileState.ClassTable.ClassNodes[globalClassIndex].symbols.symbolList[symbolindex];
                    }
                    else
                    {
                        symbolnode = codeBlock.symbolTable.symbolList[symbolindex];
                    }
                }
            }
            else if (isAllocated)
            {
                type = symbolnode.datatype;
                runtimeIndex = symbolnode.runtimeTableIndex;

                if (null != t.ArrayDimensions)
                {
                    /* Remove static type checkings.
                    if (type.UID != (int)PrimitiveType.kTypeArray && !type.IsIndexable)
                        buildStatus.LogWarning(ProtoCore.BuildData.WarningID.kWarnMax, string.Format("'{0}' is not indexable at compile time", t.Name));
                    */
                    dimensions = DfsEmitArrayIndexHeap(t.ArrayDimensions);
                }

                //fix type's rank   
                if (type.rank >= 0)
                {
                    type.rank -= dimensions;
                    if (type.rank < 0)
                    {
                        //throw new Exception("Exceed maximum rank!");
                        type.rank = 0;
                    }
                }

                //check whether the value is an array
                if (type.rank == 0)
                {
                    type.IsIndexable = false;
                }

                if (ignoreRankCheck || compileState.TypeSystem.IsHigherRank(type.UID, inferedType.UID))
                {
                    inferedType = type;
                }
                // We need to get inferedType for boolean variable so that we can perform type check
                inferedType.UID = (isBooleanOp || (type.UID == (int)PrimitiveType.kTypeBool)) ? (int)PrimitiveType.kTypeBool : inferedType.UID;
            }

        }
#if ENABLE_INC_DEC_FIX
        private void EmitPostFixNode(AssociativeNode node, ref ProtoCore.Type inferedType)
        {
            bool parseGlobal = null == localProcedure && ProtoCore.DSASM.AssociativeCompilePass.kAll == compilePass;
            bool parseGlobalFunction = null != localProcedure && ProtoCore.DSASM.AssociativeCompilePass.kGlobalFuncBody == compilePass;
            bool parseMemberFunction = ProtoCore.DSASM.Constants.kGlobalScope != classIndex && ProtoCore.DSASM.AssociativeCompilePass.kClassMemFuncBody == compilePass;

            if (parseGlobal || parseGlobalFunction || parseMemberFunction)
            {
                PostFixNode pfNode = node as PostFixNode;

                //convert post fix operation to a binary operation
                BinaryExpressionNode binRight = new BinaryExpressionNode();
                BinaryExpressionNode bin = new BinaryExpressionNode();

                binRight.LeftNode = pfNode.Identifier;
                binRight.RightNode = new IntNode() { value = "1" };
                binRight.Optr = (ProtoCore.DSASM.UnaryOperator.Increment == pfNode.Operator) ? ProtoCore.DSASM.Operator.add : ProtoCore.DSASM.Operator.sub;
                bin.LeftNode = pfNode.Identifier;
                bin.RightNode = binRight;
                bin.Optr = ProtoCore.DSASM.Operator.assign;
                EmitBinaryExpressionNode(bin, ref inferedType);
            }
        }
#endif
        private void EmitRangeExprNode(AssociativeNode node, ref ProtoCore.Type inferedType, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone)
        {
            // (Ayush) Using Decimal type instead of Double for the sake of precision. For eg. doing 0.3d - 0.1d gives a result a little less than 0.2. These inaccuracies propogate
            // through a range expression and quickly cause wrong values to be returned.

            RangeExprNode range = node as RangeExprNode;

            if ((range.FromNode is NullNode || range.FromNode is BooleanNode)
                 || (range.ToNode is NullNode || range.ToNode is BooleanNode)
                 || (range.StepNode is NullNode || range.StepNode is BooleanNode))
            {
                throw new ProtoCore.Exceptions.CompileErrorsOccured("Invalid range expression");
            }
            else if ((range.FromNode is IntNode || range.FromNode is DoubleNode)
                && (range.ToNode is IntNode || range.ToNode is DoubleNode)
                && (range.StepNode is IntNode || range.StepNode is DoubleNode))
            {
                ProtoCore.Type type = new ProtoCore.Type();
                type.UID = (int)PrimitiveType.kTypeVoid;
                type.IsIndexable = true;
                type.Name = "double";
                type.UID = (int)ProtoCore.PrimitiveType.kTypeDouble;
                type.rank = 1;


                int totalSteps = 0;
                bool terminate = false;
                decimal current = (range.FromNode is IntNode) ? Int64.Parse((range.FromNode as IntNode).value) : Decimal.Parse((range.FromNode as DoubleNode).value);
                decimal end = (range.ToNode is IntNode) ? Int64.Parse((range.ToNode as IntNode).value) : Decimal.Parse((range.ToNode as DoubleNode).value);
                ProtoCore.DSASM.RangeStepOperator stepoperator = range.stepoperator;
                decimal step = (range.StepNode is IntNode) ? Int64.Parse((range.StepNode as IntNode).value) : Decimal.Parse((range.StepNode as DoubleNode).value);
                decimal stepsize = 1.0M;

                if (stepoperator == ProtoCore.DSASM.RangeStepOperator.stepsize)
                {
                    if (step == 0)
                    {
                        return;
                    }
                    if ((end > current && step < 0) || (end < current && step > 0))
                    {
                        return;
                    }
                    stepsize = step;
                }
                else if (stepoperator == ProtoCore.DSASM.RangeStepOperator.num)
                {
                    if (!(range.StepNode is IntNode))
                    {
                        return;
                    }

                    if (step <= 0)
                    {
                        return;
                    }
                    if ((step - 1) == 0)
                        stepsize = 0;
                    else
                        stepsize = (end - current) / (step - 1);
                }
                else if (stepoperator == ProtoCore.DSASM.RangeStepOperator.approxsize)
                {
                    if (step == 0)
                    {
                        return;
                    }
                    RangeExprNode rnode = range;
                    IntNode newStep = new IntNode();
                    rnode.StepNode = newStep; rnode.stepoperator = ProtoCore.DSASM.RangeStepOperator.num;

                    decimal dist = end - current;
                    if (dist == 0)
                    {
                        newStep.value = "1";
                    }
                    else
                    {
                        decimal ceilStepSize = Math.Ceiling((dist) / step);
                        decimal floorStepSize = Math.Floor((dist) / step);
                        decimal numOfSteps;

                        if ((ceilStepSize == 0) || (floorStepSize == 0))
                            numOfSteps = 2;
                        else
                            numOfSteps = (Math.Abs(dist / ceilStepSize - step) < Math.Abs(dist / floorStepSize - step)) ? ceilStepSize + 1 : floorStepSize + 1;
                        newStep.value = numOfSteps.ToString();
                    }

                    EmitRangeExprNode(rnode, ref inferedType);
                    return;
                }


                bool isIntArray = (range.FromNode is IntNode) &&
                                  (range.ToNode is IntNode) &&
                                  (range.StepNode is IntNode) &&
                                  (Math.Equals(stepsize, Math.Truncate(stepsize)));
                if (isIntArray)
                {
                    type.Name = "int";
                    type.UID = (int)ProtoCore.PrimitiveType.kTypeInt;
                }

                if (stepsize == 0)
                {
                    for (int i = 0; i < step; ++i)
                    {
                        if (isIntArray)
                            EmitIntNode(new IntNode { value = current.ToString() }, ref type);
                        else
                            EmitDoubleNode(new DoubleNode { value = current.ToString() }, ref type);
                        ++totalSteps;
                    }
                }
                else
                {
                    while (true)
                    {
                        if (stepoperator == ProtoCore.DSASM.RangeStepOperator.stepsize)
                            terminate = (step < 0) ? ((current < end) ? true : false) : ((current > end) ? true : false);
                        else if (stepoperator == ProtoCore.DSASM.RangeStepOperator.num)
                            terminate = (totalSteps >= step) ? true : false;
                        if (terminate) break;

                        if (isIntArray)
                            EmitIntNode(new IntNode { value = current.ToString() }, ref type);
                        else
                            EmitDoubleNode(new DoubleNode { value = current.ToString() }, ref type);
                        current += stepsize;
                        ++totalSteps;
                    }
                }

                inferedType = type;
            }
            else
            {
                // traverse the from node
                DfsTraverse(range.FromNode, ref inferedType);

                // traverse the To node
                DfsTraverse(range.ToNode, ref inferedType);

                // traverse the step node
                DfsTraverse(range.StepNode, ref inferedType, false, null, subPass);


                inferedType.IsIndexable = true;
                inferedType.rank++;
            }
        }

        private void EmitForLoopNode(AssociativeNode node)
        {
            throw new NotImplementedException();
        }

        private void EmitLanguageBlockNode(AssociativeNode node, ref ProtoCore.Type inferedType, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone)
        {
            if (IsParsingGlobal() || IsParsingGlobalFunctionBody() || IsParsingMemberFunctionBody())
            {
                if (subPass == ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier)
                {
                    return;
                }

                LanguageBlockNode langblock = node as LanguageBlockNode;

                //Debug.Assert(ProtoCore.Language.kInvalid != langblock.codeblock.language);
                if (ProtoCore.Language.kInvalid == langblock.codeblock.language)
                    throw new BuildHaltException("Invalid language block type");

                ProtoCore.CompileTime.Context context = new ProtoCore.CompileTime.Context();

                int blockId = ProtoCore.DSASM.Constants.kInvalidIndex;

                // Top block signifies the auto inserted global block
                bool isTopBlock = null == codeBlock.parent;


                // Set the current class scope so the next language can refer to it
                compileState.ClassIndex = globalClassIndex;

                if (globalProcIndex != ProtoCore.DSASM.Constants.kInvalidIndex && compileState.ProcNode == null)
                {
                    if (globalClassIndex != ProtoCore.DSASM.Constants.kInvalidIndex)
                        compileState.ProcNode = compileState.ClassTable.ClassNodes[globalClassIndex].vtable.procList[globalProcIndex];
                    else
                        compileState.ProcNode = codeBlock.procedureTable.procList[globalProcIndex];
                }

                if (langblock.codeblock.language == Language.kAssociative)
                {
                    AssociativeCodeGen codegen = new AssociativeCodeGen(compileState, codeBlock);
                    blockId = codegen.Emit(langblock.CodeBlockNode as ProtoCore.AST.AssociativeAST.CodeBlockNode);
                }
                else if (langblock.codeblock.language == Language.kImperative)
                {
                    ImperativeCodeGen codegen = new ImperativeCodeGen(compileState, codeBlock);
                    blockId = codegen.Emit(langblock.CodeBlockNode as ProtoCore.AST.ImperativeAST.CodeBlockNode);
                }
                //core.Executives[langblock.codeblock.language].Compile(out blockId, codeBlock, langblock.codeblock, context, codeBlock.EventSink, langblock.CodeBlockNode);
                inferedType = compileState.InferedType;

                ExceptionRegistration registration = compileState.ExceptionHandlingManager.ExceptionTable.GetExceptionRegistration(blockId, globalProcIndex, globalClassIndex);
                if (registration == null)
                {
                    registration = compileState.ExceptionHandlingManager.ExceptionTable.Register(blockId, globalProcIndex, globalClassIndex);
                    Debug.Assert(registration != null);
                }
            }
        }

        private void EmitGetterForMemVar(ProtoCore.AST.AssociativeAST.ClassDeclNode cnode, ProtoCore.DSASM.SymbolNode memVar)
        {
            FunctionDefinitionNode getter = new FunctionDefinitionNode()
            {
                Name = ProtoCore.DSASM.Constants.kGetterPrefix + memVar.name,
                Singnature = new ArgumentSignatureNode(),
                Pattern = null,
                ReturnType = compileState.TypeSystem.BuildTypeObject((int)PrimitiveType.kTypeVar, false),
                FunctionBody = new CodeBlockNode(),
                IsExternLib = false,
                IsDNI = false,
                ExternLibName = null,
                access = memVar.access,
                IsStatic = memVar.isStatic
            };

            getter.FunctionBody.Body.Add(new BinaryExpressionNode()
            {
                // return = property;
                LeftNode = new IdentifierNode()
                {
                    Value = ProtoCore.DSDefinitions.Kw.kw_return,
                    Name = ProtoCore.DSDefinitions.Kw.kw_return,
                    datatype = compileState.TypeSystem.BuildTypeObject(PrimitiveType.kTypeReturn, false)
                },
                Optr = ProtoCore.DSASM.Operator.assign,
                RightNode = new IdentifierNode()
                {
                    Value = memVar.name,
                    Name = memVar.name,
                    datatype = compileState.TypeSystem.BuildTypeObject(PrimitiveType.kTypeVar, false)
                }
            });

            cnode.funclist.Add(getter);
        }

        private void EmitSetterForMemVar(ProtoCore.AST.AssociativeAST.ClassDeclNode cnode, ProtoCore.DSASM.SymbolNode memVar)
        {
            var argument = new ProtoCore.AST.AssociativeAST.VarDeclNode()
            {
                memregion = ProtoCore.DSASM.MemoryRegion.kMemStack,
                access = ProtoCore.DSASM.AccessSpecifier.kPublic,
                NameNode = new ProtoCore.AST.AssociativeAST.IdentifierNode()
                {
                    Value = ProtoCore.DSASM.Constants.kTempArg,
                    Name = ProtoCore.DSASM.Constants.kTempArg,
                    datatype = compileState.TypeSystem.BuildTypeObject(PrimitiveType.kTypeVar, false)
                },
                ArgumentType = new ProtoCore.Type
                {
                    Name = compileState.TypeSystem.GetType((int)PrimitiveType.kTypeVar),
                    UID = (int)PrimitiveType.kTypeVar
                }
            };

            var argumentSingature = new ArgumentSignatureNode();
            argumentSingature.AddArgument(argument);

            FunctionDefinitionNode setter = new FunctionDefinitionNode()
            {
                Name = ProtoCore.DSASM.Constants.kSetterPrefix + memVar.name,
                Singnature = argumentSingature,
                Pattern = null,
                ReturnType = compileState.TypeSystem.BuildTypeObject((int)PrimitiveType.kTypeNull, false),
                FunctionBody = new CodeBlockNode(),
                IsExternLib = false,
                IsDNI = false,
                ExternLibName = null,
                access = memVar.access,
                IsStatic = memVar.isStatic
            };

            // property = %tmpArg
            setter.FunctionBody.Body.Add(new BinaryExpressionNode()
            {
                // return = property;
                LeftNode = new IdentifierNode()
                {
                    Value = memVar.name,
                    Name = memVar.name,
                    datatype = compileState.TypeSystem.BuildTypeObject(PrimitiveType.kTypeVar, false)
                },
                Optr = ProtoCore.DSASM.Operator.assign,
                RightNode = new IdentifierNode()
                {
                    Value = ProtoCore.DSASM.Constants.kTempArg,
                    Name = ProtoCore.DSASM.Constants.kTempArg,
                    datatype = compileState.TypeSystem.BuildTypeObject(PrimitiveType.kTypeVar, false)
                }
            });

            // return = null;
            setter.FunctionBody.Body.Add(new BinaryExpressionNode()
            {
                LeftNode = new IdentifierNode()
                {
                    Value = ProtoCore.DSDefinitions.Kw.kw_return,
                    Name = ProtoCore.DSDefinitions.Kw.kw_return,
                    datatype = compileState.TypeSystem.BuildTypeObject(PrimitiveType.kTypeReturn, false)
                },
                Optr = ProtoCore.DSASM.Operator.assign,
                RightNode = new NullNode()
            });

            cnode.funclist.Add(setter);
        }

        private void EmitClassDeclNode(AssociativeNode node, ref ProtoCore.Type inferedType, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone)
        {
            ClassDeclNode classDecl = node as ClassDeclNode;

            // Handling n-pass on class declaration
            if (ProtoCore.DSASM.AssociativeCompilePass.kClassName == compilePass)
            {
                ProtoCore.DSASM.ClassNode cnode = new ProtoCore.DSASM.ClassNode();
                cnode.name = classDecl.className;
                cnode.size = classDecl.varlist.Count;
                cnode.IsImportedClass = classDecl.IsImportedClass;
                cnode.typeSystem = compileState.TypeSystem;
                globalClassIndex = compileState.ClassTable.Append(cnode);
                CodeRangeTable.AddClassBlockRangeEntry(globalClassIndex,
                    classDecl.line,
                    classDecl.col,
                    classDecl.endLine,
                    classDecl.endCol,
                    compileState.CurrentDSFileName);
            }
            else if (ProtoCore.DSASM.AssociativeCompilePass.kClassHeirarchy == compilePass)
            {
                // Class heirarchy pass
                // Populating each class entry with their respective base classes
                globalClassIndex = compileState.ClassTable.IndexOf(classDecl.className);

                ProtoCore.DSASM.ClassNode cnode = compileState.ClassTable.ClassNodes[globalClassIndex];

                if (null != classDecl.superClass)
                {
                    for (int n = 0; n < classDecl.superClass.Count; ++n)
                    {
                        int baseClass = compileState.ClassTable.IndexOf(classDecl.superClass[n]);
                        if (ProtoCore.DSASM.Constants.kInvalidIndex != baseClass)
                        {
                            cnode.baseList.Add(baseClass);
                            cnode.coerceTypes.Add(baseClass, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceBaseClass);
                        }
                    }
                }

            }
            else if (ProtoCore.DSASM.AssociativeCompilePass.kClassMemVar == compilePass)
            {
                // Class member variable pass
                // Populating each class entry symbols with their respective member variables

                globalClassIndex = compileState.ClassTable.IndexOf(classDecl.className);
                ProtoCore.DSASM.ClassNode cnode = compileState.ClassTable.ClassNodes[globalClassIndex];

                // Handle member vars from base class
                if (null != classDecl.superClass)
                {
                    for (int n = 0; n < classDecl.superClass.Count; ++n)
                    {
                        int baseClass = compileState.ClassTable.IndexOf(classDecl.superClass[n]);
                        if (ProtoCore.DSASM.Constants.kInvalidIndex != baseClass)
                        {
                            // Append the members variables of every class that this class inherits from
                            foreach (ProtoCore.DSASM.SymbolNode symnode in compileState.ClassTable.ClassNodes[baseClass].symbols.symbolList.Values)
                            {
                                // It is a member variables
                                if (ProtoCore.DSASM.Constants.kGlobalScope == symnode.functionIndex)
                                {
                                    Debug.Assert(!symnode.isArgument);
                                    int symbolIndex = AllocateMemberVariable(globalClassIndex, symnode.isStatic ? symnode.classScope : baseClass, symnode.name, symnode.datatype, symnode.access, symnode.isStatic);

                                    if (symbolIndex != ProtoCore.DSASM.Constants.kInvalidIndex)
                                        cnode.size += ProtoCore.DSASM.Constants.kPointerSize;
                                }
                            }
                        }
                        else
                        {
                            Debug.Assert(false, "n-pass compile error, fixme Jun....");
                        }
                    }
                }

                // This list will store all static properties initialization
                // expression (say x = 1).
                List<BinaryExpressionNode> staticPropertyInitList = new List<BinaryExpressionNode>();

                foreach (VarDeclNode vardecl in classDecl.varlist)
                {
                    IdentifierNode varIdent = null;
                    if (vardecl.NameNode is IdentifierNode)
                    {
                        varIdent = vardecl.NameNode as IdentifierNode;
                    }
                    else if (vardecl.NameNode is BinaryExpressionNode)
                    {
                        BinaryExpressionNode bNode = vardecl.NameNode as BinaryExpressionNode;
                        varIdent = bNode.LeftNode as IdentifierNode;
                        if (vardecl.IsStatic)
                            staticPropertyInitList.Add(bNode);
                        else
                            cnode.defaultArgExprList.Add(bNode);
                    }
                    else
                    {
                        Debug.Assert(false, "Check generated AST");
                    }

                    // It is possible that fail to allocate variable. In that 
                    // case we should remove initializing expression from 
                    // cnode's defaultArgExprList
                    ProtoCore.Type datatype = vardecl.ArgumentType;
                    if (string.IsNullOrEmpty(vardecl.ArgumentType.Name))
                        datatype.UID = (int)PrimitiveType.kTypeVar;
                    else
                    {
                        datatype.UID = compileState.TypeSystem.GetType(vardecl.ArgumentType.Name);
                        if (datatype.UID == ProtoCore.DSASM.Constants.kInvalidIndex)
                            datatype.UID = (int)PrimitiveType.kTypeVar;
                    }
                    datatype.Name = compileState.TypeSystem.GetType(datatype.UID);
                    int symbolIndex = ProtoCore.DSASM.Constants.kInvalidIndex;

                    if (varIdent is ProtoCore.AST.AssociativeAST.TypedIdentifierNode)
                        symbolIndex = AllocateTypedMemberVariable(globalClassIndex, globalClassIndex, varIdent.Value, datatype, vardecl.access, vardecl.IsStatic);
                    else
                        symbolIndex = AllocateMemberVariable(globalClassIndex, globalClassIndex, varIdent.Value, datatype, vardecl.access, vardecl.IsStatic);

                    if (symbolIndex != ProtoCore.DSASM.Constants.kInvalidIndex &&
                        !classDecl.IsExternLib)
                    {
                        ProtoCore.DSASM.SymbolNode memVar =
                            vardecl.IsStatic
                            ? compileState.CodeBlockList[0].symbolTable.symbolList[symbolIndex]
                            : compileState.ClassTable.ClassNodes[globalClassIndex].symbols.symbolList[symbolIndex];

                        EmitGetterForMemVar(classDecl, memVar);
                        EmitSetterForMemVar(classDecl, memVar);
                    }
                }
                classOffset = 0;

                // Now we are going to create a static function __init_static_properties()
                // which will initialize all static properties. We will emit a 
                // call to this function after all classes have been compiled.
                if (staticPropertyInitList.Count > 0)
                {
                    FunctionDefinitionNode initFunc = new FunctionDefinitionNode()
                    {
                        Name = ProtoCore.DSASM.Constants.kStaticPropertiesInitializer,
                        Singnature = new ArgumentSignatureNode(),
                        Pattern = null,
                        ReturnType = new ProtoCore.Type { Name = compileState.TypeSystem.GetType((int)PrimitiveType.kTypeNull), UID = (int)PrimitiveType.kTypeNull },
                        FunctionBody = new CodeBlockNode(),
                        IsExternLib = false,
                        IsDNI = false,
                        ExternLibName = null,
                        access = ProtoCore.DSASM.AccessSpecifier.kPublic,
                        IsStatic = true
                    };
                    classDecl.funclist.Add(initFunc);

                    staticPropertyInitList.ForEach(bNode => initFunc.FunctionBody.Body.Add(bNode));
                    initFunc.FunctionBody.Body.Add(new BinaryExpressionNode()
                    {
                        LeftNode = new IdentifierNode()
                        {
                            Value = ProtoCore.DSDefinitions.Kw.kw_return,
                            Name = ProtoCore.DSDefinitions.Kw.kw_return,
                            datatype = compileState.TypeSystem.BuildTypeObject(PrimitiveType.kTypeReturn, false)
                        },
                        Optr = ProtoCore.DSASM.Operator.assign,
                        RightNode = new NullNode()
                    });
                }
            }
            else if (ProtoCore.DSASM.AssociativeCompilePass.kClassMemFuncSig == compilePass)
            {
                // Class member variable pass
                // Populating each class entry vtables with their respective member variables signatures

                globalClassIndex = compileState.ClassTable.IndexOf(classDecl.className);
                foreach (AssociativeNode funcdecl in classDecl.funclist)
                {
                    DfsTraverse(funcdecl, ref inferedType);
                }

                if (!classDecl.IsExternLib)
                {
                    ProtoCore.DSASM.ProcedureTable vtable = compileState.ClassTable.ClassNodes[globalClassIndex].vtable;
                    if (vtable.IndexOfExact(classDecl.className, new List<ProtoCore.Type>()) == ProtoCore.DSASM.Constants.kInvalidIndex)
                    {
                        ConstructorDefinitionNode defaultConstructor = new ConstructorDefinitionNode();
                        defaultConstructor.Name = classDecl.className;
                        defaultConstructor.localVars = 0;
                        defaultConstructor.Signature = new ArgumentSignatureNode();
                        defaultConstructor.Pattern = null;
                        defaultConstructor.ReturnType = new ProtoCore.Type { Name = "var", UID = 0 };
                        defaultConstructor.FunctionBody = new CodeBlockNode();
                        defaultConstructor.baseConstr = null;
                        defaultConstructor.access = ProtoCore.DSASM.AccessSpecifier.kPublic;
                        defaultConstructor.IsExternLib = false;
                        defaultConstructor.ExternLibName = null;
                        DfsTraverse(defaultConstructor, ref inferedType);
                        classDecl.funclist.Add(defaultConstructor);
                    }
                }
            }
            else if (ProtoCore.DSASM.AssociativeCompilePass.kClassMemFuncBody == compilePass)
            {
                // Class member variable pass
                // Populating the function body of each member function defined in the class vtables

                globalClassIndex = compileState.ClassTable.IndexOf(classDecl.className);

                foreach (AssociativeNode funcdecl in classDecl.funclist)
                {
                    // reset the inferedtype between functions
                    inferedType = new ProtoCore.Type();
                    DfsTraverse(funcdecl, ref inferedType, false, null, subPass);
                }
            }

            // Reset the class index
            compileState.ClassIndex = globalClassIndex = ProtoCore.DSASM.Constants.kGlobalScope;
        }

        private void EmitCallingForBaseConstructor(int thisClassIndex, ProtoCore.AST.AssociativeAST.FunctionCallNode baseConstructor)
        {
            List<ProtoCore.Type> argTypeList = new List<ProtoCore.Type>();
            int ctorIndex = ProtoCore.DSASM.Constants.kInvalidIndex;
            int baseIndex = ProtoCore.DSASM.Constants.kInvalidIndex;
            string baseConstructorName = null;

            if (baseConstructor != null)
            {
                if (baseConstructor.Function == null)
                {
                    int baseClassIndex = compileState.ClassTable.ClassNodes[thisClassIndex].baseList[0];
                    baseConstructorName = compileState.ClassTable.ClassNodes[baseClassIndex].name; 
                }
                else
                {
                    baseConstructorName = baseConstructor.Function.Name;
                }

                foreach (AssociativeNode paramNode in baseConstructor.FormalArguments)
                {
                    ProtoCore.Type paramType = new ProtoCore.Type();
                    paramType.UID = (int)ProtoCore.PrimitiveType.kTypeVoid;
                    paramType.IsIndexable = false;

                    enforceTypeCheck = !(paramNode is BinaryExpressionNode);
                    DfsTraverse(paramNode, ref paramType);
                    enforceTypeCheck = true;
                    argTypeList.Add(paramType);
                }

                List<int> myBases = compileState.ClassTable.ClassNodes[globalClassIndex].baseList;
                foreach (int bidx in myBases)
                {
                    int cidx = compileState.ClassTable.ClassNodes[bidx].vtable.IndexOf(baseConstructorName, argTypeList, compileState.ClassTable);
                    if ((cidx != ProtoCore.DSASM.Constants.kInvalidIndex) &&
                        compileState.ClassTable.ClassNodes[bidx].vtable.procList[cidx].isConstructor)
                    {
                        ctorIndex = cidx;
                        baseIndex = bidx;
                        break;
                    }
                }
            }
            else
            {
                // call base class's default constructor
                // TODO keyu: to support multiple inheritance
                List<int> myBases = compileState.ClassTable.ClassNodes[globalClassIndex].baseList;
                foreach (int bidx in myBases)
                {
                    baseConstructorName = compileState.ClassTable.ClassNodes[bidx].name;
                    int cidx = compileState.ClassTable.ClassNodes[bidx].vtable.IndexOf(baseConstructorName, argTypeList, compileState.ClassTable);
                    Debug.Assert(cidx != ProtoCore.DSASM.Constants.kInvalidIndex);
                    ctorIndex = cidx;
                    baseIndex = bidx;
                }
            }
        }

        private void EmitConstructorDefinitionNode(AssociativeNode node, ref ProtoCore.Type inferedType, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone)
        {
            ConstructorDefinitionNode funcDef = node as ConstructorDefinitionNode;
            ProtoCore.DSASM.CodeBlockType originalBlockType = codeBlock.blockType;
            codeBlock.blockType = ProtoCore.DSASM.CodeBlockType.kFunction;

            if (IsParsingMemberFunctionSig())
            {
                Debug.Assert(null == localProcedure);
                localProcedure = new ProtoCore.DSASM.ProcedureNode();

                localProcedure.name = funcDef.Name;
                localProcedure.pc = ProtoCore.DSASM.Constants.kInvalidIndex;
                localProcedure.localCount = 0;// Defer till all locals are allocated
                localProcedure.returntype.UID = compileState.TypeSystem.GetType(funcDef.ReturnType.Name);
                if (localProcedure.returntype.UID == ProtoCore.DSASM.Constants.kInvalidIndex)
                    localProcedure.returntype.UID = (int)PrimitiveType.kTypeVar;
                localProcedure.returntype.Name = compileState.TypeSystem.GetType(localProcedure.returntype.UID);
                localProcedure.returntype.IsIndexable = false;
                localProcedure.isConstructor = true;
                localProcedure.runtimeIndex = 0;

                Debug.Assert(ProtoCore.DSASM.Constants.kInvalidIndex != globalClassIndex, "A constructor node must be associated with class");
                localProcedure.localCount = 0;

                int peekFunctionindex = compileState.ClassTable.ClassNodes[globalClassIndex].vtable.procList.Count;

                if (!funcDef.IsExternLib)
                    CodeRangeTable.AddClassBlockFunctionEntry(globalClassIndex,
                        peekFunctionindex,
                        funcDef.FunctionBody.line,
                        funcDef.FunctionBody.col,
                        funcDef.FunctionBody.endLine,
                        funcDef.FunctionBody.endCol,
                        compileState.CurrentDSFileName);

                // Append arg symbols
                List<KeyValuePair<string, ProtoCore.Type>> argsToBeAllocated = new List<KeyValuePair<string, ProtoCore.Type>>();
                if (null != funcDef.Signature)
                {
                    int argNumber = 0;
                    foreach (VarDeclNode argNode in funcDef.Signature.Arguments)
                    {
                        ++argNumber;

                        IdentifierNode paramNode = null;
                        bool aIsDefault = false;
                        ProtoCore.AST.Node aDefaultExpression = null;
                        if (argNode.NameNode is IdentifierNode)
                        {
                            paramNode = argNode.NameNode as IdentifierNode;
                        }
                        else if (argNode.NameNode is BinaryExpressionNode)
                        {
                            BinaryExpressionNode bNode = argNode.NameNode as BinaryExpressionNode;
                            paramNode = bNode.LeftNode as IdentifierNode;
                            aIsDefault = true;
                            aDefaultExpression = bNode;
                            //buildStatus.LogSemanticError("Default parameters are not supported");
                            //throw new BuildHaltException();
                        }
                        else
                        {
                            Debug.Assert(false, "Check generated AST");
                        }

                        ProtoCore.Type argType = new ProtoCore.Type();
                        argType.UID = compileState.TypeSystem.GetType(argNode.ArgumentType.Name);
                        if (argType.UID == ProtoCore.DSASM.Constants.kInvalidIndex)
                            argType.UID = (int)PrimitiveType.kTypeVar;
                        argType.Name = compileState.TypeSystem.GetType(argType.UID);
                        argType.IsIndexable = argNode.ArgumentType.IsIndexable;
                        argType.rank = argNode.ArgumentType.rank;

                        argsToBeAllocated.Add(new KeyValuePair<string, ProtoCore.Type>(paramNode.Value, argType));
                        localProcedure.argTypeList.Add(argType);
                        ProtoCore.DSASM.ArgumentInfo argInfo = new ProtoCore.DSASM.ArgumentInfo { isDefault = aIsDefault, defaultExpression = aDefaultExpression, Name = paramNode.Name };
                        localProcedure.argInfoList.Add(argInfo);
                    }
                }

                int findex = compileState.ClassTable.ClassNodes[globalClassIndex].vtable.Append(localProcedure);

                // Comment Jun: Catch this assert given the condition as this type of mismatch should never occur
                if (ProtoCore.DSASM.Constants.kInvalidIndex != findex)
                {
                    Debug.Assert(peekFunctionindex == localProcedure.procId);
                    argsToBeAllocated.ForEach(arg =>
                    {
                        int symbolIndex = AllocateArg(arg.Key, findex, arg.Value);
                        if (ProtoCore.DSASM.Constants.kInvalidIndex == symbolIndex)
                        {
                            throw new BuildHaltException();
                        }
                    });
                }
            }
            else if (IsParsingMemberFunctionBody())
            {
                // Build arglist for comparison
                List<ProtoCore.Type> argList = new List<ProtoCore.Type>();
                if (null != funcDef.Signature)
                {
                    foreach (VarDeclNode argNode in funcDef.Signature.Arguments)
                    {
                        int argType = compileState.TypeSystem.GetType(argNode.ArgumentType.Name);
                        bool isArray = argNode.ArgumentType.IsIndexable;
                        int rank = argNode.ArgumentType.rank;
                        argList.Add(compileState.TypeSystem.BuildTypeObject(argType, isArray, rank));
                    }
                }

                globalProcIndex = compileState.ClassTable.ClassNodes[globalClassIndex].vtable.IndexOfExact(funcDef.Name, argList);

                Debug.Assert(null == localProcedure);
                localProcedure = compileState.ClassTable.ClassNodes[globalClassIndex].vtable.procList[globalProcIndex];

                Debug.Assert(null != localProcedure);
                if (null == localProcedure)
                    return;

                if (-1 == localProcedure.classScope && (localProcedure.classScope != globalClassIndex))
                    localProcedure.classScope = globalClassIndex; // Assign class index if it's not done.

                ProtoCore.FunctionEndPoint fep = null;
                if (!funcDef.IsExternLib)
                {
                    // Traverse default assignment for the class
                    foreach (BinaryExpressionNode bNode in compileState.ClassTable.ClassNodes[globalClassIndex].defaultArgExprList)
                    {
                        EmitBinaryExpressionNode(bNode, ref inferedType, false, null, subPass);
                        UpdateType(bNode.LeftNode.Name, ProtoCore.DSASM.Constants.kInvalidIndex, inferedType);
                    }

                    //Traverse default argument for the constructor
                    foreach (ProtoCore.DSASM.ArgumentInfo argNode in localProcedure.argInfoList)
                    {
                        if (!argNode.isDefault)
                        {
                            continue;
                        }
                        BinaryExpressionNode bNode = argNode.defaultExpression as BinaryExpressionNode;
                        // build a temporay node for statement : temp = defaultarg;
                        IdentifierNode iNodeTemp = new IdentifierNode()
                        {
                            Value = ProtoCore.DSASM.Constants.kTempDefaultArg,
                            Name = ProtoCore.DSASM.Constants.kTempDefaultArg,
                            datatype = compileState.TypeSystem.BuildTypeObject(PrimitiveType.kTypeVar, false)
                        };
                        BinaryExpressionNode bNodeTemp = new BinaryExpressionNode();
                        bNodeTemp.LeftNode = iNodeTemp;
                        bNodeTemp.Optr = ProtoCore.DSASM.Operator.assign;
                        bNodeTemp.RightNode = bNode.LeftNode;
                        EmitBinaryExpressionNode(bNodeTemp, ref inferedType);
                        //duild an inline conditional node for statement: defaultarg = (temp == DefaultArgNode) ? defaultValue : temp;
                        InlineConditionalNode icNode = new InlineConditionalNode();
                        BinaryExpressionNode cExprNode = new BinaryExpressionNode();
                        cExprNode.Optr = ProtoCore.DSASM.Operator.eq;
                        cExprNode.LeftNode = iNodeTemp;
                        cExprNode.RightNode = new DefaultArgNode();
                        icNode.ConditionExpression = cExprNode;
                        icNode.TrueExpression = bNode.RightNode;
                        icNode.FalseExpression = iNodeTemp;
                        bNodeTemp.LeftNode = bNode.LeftNode;
                        bNodeTemp.RightNode = icNode;
                        EmitBinaryExpressionNode(bNodeTemp, ref inferedType);
                    }

                    // Traverse definition
                    foreach (AssociativeNode bnode in funcDef.FunctionBody.Body)
                    {
                        inferedType.UID = (int)PrimitiveType.kTypeVoid;
                        inferedType.rank = 0;
                        DfsTraverse(bnode, ref inferedType, false, null, subPass);
                    }

                    // All locals have been stack allocated, update the local count of this function
                    localProcedure.localCount = compileState.BaseOffset;
                    compileState.ClassTable.ClassNodes[globalClassIndex].vtable.procList[globalProcIndex].localCount = compileState.BaseOffset;

                    // Update the param stack indices of this function
                    foreach (ProtoCore.DSASM.SymbolNode symnode in compileState.ClassTable.ClassNodes[globalClassIndex].symbols.symbolList.Values)
                    {
                        if (symnode.functionIndex == globalProcIndex && symnode.isArgument)
                        {
                            symnode.index -= localProcedure.localCount;
                        }
                    }

                    // JIL FEP
                    ProtoCore.Lang.JILActivationRecord record = new ProtoCore.Lang.JILActivationRecord();
                    record.pc = localProcedure.pc;
                    record.locals = localProcedure.localCount;
                    record.classIndex = globalClassIndex;
                    record.funcIndex = globalProcIndex;

                    // Construct the fep arguments
                    fep = new ProtoCore.Lang.JILFunctionEndPoint(record);
                }
                else
                {
                    ProtoCore.Lang.JILActivationRecord jRecord = new ProtoCore.Lang.JILActivationRecord();
                    jRecord.pc = localProcedure.pc;
                    jRecord.locals = localProcedure.localCount;
                    jRecord.classIndex = globalClassIndex;
                    jRecord.funcIndex = localProcedure.procId;

                    ProtoCore.Lang.FFIActivationRecord record = new ProtoCore.Lang.FFIActivationRecord();
                    record.JILRecord = jRecord;
                    record.FunctionName = funcDef.Name;
                    record.ModuleName = funcDef.ExternLibName;
                    record.ModuleType = "dll";
                    record.IsDNI = false;
                    record.ReturnType = funcDef.ReturnType;
                    record.ParameterTypes = localProcedure.argTypeList;
                    fep = new ProtoCore.Lang.FFIFunctionEndPoint(record);
                }

                // Construct the fep arguments
                fep.FormalParams = new ProtoCore.Type[localProcedure.argTypeList.Count];
                localProcedure.argTypeList.CopyTo(fep.FormalParams, 0);

                // TODO Jun: 'classIndexAtCallsite' is the class index as it is stored at the callsite function tables
                // Determine whether this still needs to be aligned to the actual 'classIndex' variable
                // The factors that will affect this is whether the 2 function tables (compiler and callsite) need to be merged
                int classIndexAtCallsite = globalClassIndex + 1;
                compileState.FunctionTable.AddFunctionEndPointer(classIndexAtCallsite, funcDef.Name, fep);
                
                // Build and append a graphnode for this return statememt
                ProtoCore.DSASM.SymbolNode returnNode = new ProtoCore.DSASM.SymbolNode();
                returnNode.name = "return";
            }

            // Constructors have no return statemetns, reset variables here
            compileState.ProcNode = localProcedure = null;
            globalProcIndex = ProtoCore.DSASM.Constants.kGlobalScope;
            compileState.BaseOffset = 0;
            argOffset = 0;
            classOffset = 0;
            codeBlock.blockType = originalBlockType;
        }

        private void EmitFunctionDefinitionNode(AssociativeNode node, ref ProtoCore.Type inferedType, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone)
        {
            bool parseGlobalFunctionBody = null == localProcedure && ProtoCore.DSASM.AssociativeCompilePass.kGlobalFuncBody == compilePass;
            bool parseMemberFunctionBody = ProtoCore.DSASM.Constants.kGlobalScope != globalClassIndex && ProtoCore.DSASM.AssociativeCompilePass.kClassMemFuncBody == compilePass;

            FunctionDefinitionNode funcDef = node as FunctionDefinitionNode;
            bool hasReturnStatement = false;

            codeBlock.blockType = ProtoCore.DSASM.CodeBlockType.kFunction;
            if (IsParsingGlobalFunctionSig() || IsParsingMemberFunctionSig())
            {
                Debug.Assert(null == localProcedure);
                localProcedure = new ProtoCore.DSASM.ProcedureNode();

                localProcedure.name = funcDef.Name;
                localProcedure.pc = ProtoCore.DSASM.Constants.kInvalidIndex;
                localProcedure.localCount = 0; // Defer till all locals are allocated
                localProcedure.returntype.Name = funcDef.ReturnType.Name;
                localProcedure.returntype.UID = compileState.TypeSystem.GetType(funcDef.ReturnType.Name);
                if (localProcedure.returntype.UID == ProtoCore.DSASM.Constants.kInvalidIndex)
                    localProcedure.returntype.UID = (int)PrimitiveType.kTypeVar;
                localProcedure.returntype.Name = compileState.TypeSystem.GetType(localProcedure.returntype.UID);
                localProcedure.returntype.IsIndexable = funcDef.ReturnType.IsIndexable;
                localProcedure.returntype.rank = funcDef.ReturnType.rank;
                localProcedure.isConstructor = false;
                localProcedure.isStatic = funcDef.IsStatic;
                localProcedure.runtimeIndex = codeBlock.codeBlockId;
                localProcedure.access = funcDef.access;
                localProcedure.isExternal = funcDef.IsExternLib;
                localProcedure.isAutoGenerated = funcDef.IsAutoGenerated;
                localProcedure.classScope = globalClassIndex;
                localProcedure.isAssocOperator = funcDef.IsAssocOperator;

                int peekFunctionindex = ProtoCore.DSASM.Constants.kInvalidIndex;
                if (ProtoCore.DSASM.Constants.kInvalidIndex == globalClassIndex)
                {
                    peekFunctionindex = codeBlock.procedureTable.procList.Count;
                }
                else
                {
                    peekFunctionindex = compileState.ClassTable.ClassNodes[globalClassIndex].vtable.procList.Count;
                }

                if (IsParsingMemberFunctionSig() && !funcDef.IsExternLib)
                    CodeRangeTable.AddClassBlockFunctionEntry(globalClassIndex,
                        peekFunctionindex,
                        funcDef.FunctionBody.line,
                        funcDef.FunctionBody.col,
                        funcDef.FunctionBody.endLine,
                        funcDef.FunctionBody.endCol,
                        compileState.CurrentDSFileName);
                else if (!funcDef.IsExternLib)
                    CodeRangeTable.AddCodeBlockFunctionEntry(codeBlock.codeBlockId,
                        peekFunctionindex,
                        funcDef.FunctionBody.line,
                        funcDef.FunctionBody.col,
                        funcDef.FunctionBody.endLine,
                        funcDef.FunctionBody.endCol,
                        compileState.CurrentDSFileName);

                // Append arg symbols
                List<KeyValuePair<string, ProtoCore.Type>> argsToBeAllocated = new List<KeyValuePair<string, ProtoCore.Type>>();
                if (null != funcDef.Singnature)
                {
                    int argNumber = 0;
                    foreach (VarDeclNode argNode in funcDef.Singnature.Arguments)
                    {
                        ++argNumber;

                        IdentifierNode paramNode = null;
                        bool aIsDefault = false;
                        ProtoCore.AST.Node aDefaultExpression = null;
                        if (argNode.NameNode is IdentifierNode)
                        {
                            paramNode = argNode.NameNode as IdentifierNode;
                        }
                        else if (argNode.NameNode is BinaryExpressionNode)
                        {
                            BinaryExpressionNode bNode = argNode.NameNode as BinaryExpressionNode;
                            paramNode = bNode.LeftNode as IdentifierNode;
                            aIsDefault = true;
                            aDefaultExpression = bNode;
                            //buildStatus.LogSemanticError("Defualt parameters are not supported");
                            //throw new BuildHaltException();
                        }
                        else
                        {
                            Debug.Assert(false, "Check generated AST");
                        }

                        ProtoCore.Type argType = new ProtoCore.Type();
                        argType.UID = compileState.TypeSystem.GetType(argNode.ArgumentType.Name);
                        if (argType.UID == ProtoCore.DSASM.Constants.kInvalidIndex)
                            argType.UID = (int)PrimitiveType.kTypeVar;
                        argType.Name = compileState.TypeSystem.GetType(argType.UID);
                        argType.IsIndexable = argNode.ArgumentType.IsIndexable;
                        argType.rank = argNode.ArgumentType.rank;
                        // We dont directly allocate arguments now
                        argsToBeAllocated.Add(new KeyValuePair<string, ProtoCore.Type>(paramNode.Value, argType));

                        localProcedure.argTypeList.Add(argType);
                        ProtoCore.DSASM.ArgumentInfo argInfo = new ProtoCore.DSASM.ArgumentInfo { isDefault = aIsDefault, defaultExpression = aDefaultExpression, Name = paramNode.Name };
                        localProcedure.argInfoList.Add(argInfo);
                    }
                }

                if (ProtoCore.DSASM.Constants.kInvalidIndex == globalClassIndex)
                {
                    globalProcIndex = codeBlock.procedureTable.Append(localProcedure);
                }
                else
                {
                    globalProcIndex = compileState.ClassTable.ClassNodes[globalClassIndex].vtable.Append(localProcedure);
                }

                // Comment Jun: Catch this assert given the condition as this type of mismatch should never occur
                if (ProtoCore.DSASM.Constants.kInvalidIndex != globalProcIndex)
                {
                    Debug.Assert(peekFunctionindex == localProcedure.procId);

                    argsToBeAllocated.ForEach(arg =>
                    {
                        int symbolIndex = AllocateArg(arg.Key, globalProcIndex, arg.Value);
                        if (ProtoCore.DSASM.Constants.kInvalidIndex == symbolIndex)
                        {
                            throw new BuildHaltException();
                        }
                    });


                    ExceptionRegistration registration = compileState.ExceptionHandlingManager.ExceptionTable.GetExceptionRegistration(codeBlock.codeBlockId, globalProcIndex, globalClassIndex);
                    if (registration == null)
                    {
                        registration = compileState.ExceptionHandlingManager.ExceptionTable.Register(codeBlock.codeBlockId, globalProcIndex, globalClassIndex);
                        Debug.Assert(registration != null);
                    }
                }
            }
            else if (parseGlobalFunctionBody || parseMemberFunctionBody)
            {
                // Build arglist for comparison
                List<ProtoCore.Type> argList = new List<ProtoCore.Type>();
                if (null != funcDef.Singnature)
                {
                    foreach (VarDeclNode argNode in funcDef.Singnature.Arguments)
                    {
                        int argType = compileState.TypeSystem.GetType(argNode.ArgumentType.Name);
                        bool isArray = argNode.ArgumentType.IsIndexable;
                        int rank = argNode.ArgumentType.rank;
                        argList.Add(compileState.TypeSystem.BuildTypeObject(argType, isArray, rank));
                    }
                }

                // Get the exisitng procedure that was added on the previous pass
                if (ProtoCore.DSASM.Constants.kInvalidIndex == globalClassIndex)
                {
                    globalProcIndex = codeBlock.procedureTable.IndexOfExact(funcDef.Name, argList);
                    localProcedure = codeBlock.procedureTable.procList[globalProcIndex];
                }
                else
                {
                    globalProcIndex = compileState.ClassTable.ClassNodes[globalClassIndex].vtable.IndexOfExact(funcDef.Name, argList);
                    localProcedure = compileState.ClassTable.ClassNodes[globalClassIndex].vtable.procList[globalProcIndex];
                }

                Debug.Assert(null != localProcedure);

                // Its only on the parse body pass where the real pc is determined. Update this procedures' pc
                //Debug.Assert(ProtoCore.DSASM.Constants.kInvalidIndex == localProcedure.pc);

                // Copy the active function to the core so nested language blocks can refer to it
                compileState.ProcNode = localProcedure;

                ProtoCore.FunctionEndPoint fep = null;
                if (!funcDef.IsExternLib)
                {
                    foreach (ProtoCore.DSASM.ArgumentInfo argNode in localProcedure.argInfoList)
                    {
                        if (!argNode.isDefault)
                        {
                            continue;
                        }
                        BinaryExpressionNode bNode = argNode.defaultExpression as BinaryExpressionNode;
                        // build a temporay node for statement : temp = defaultarg;
                        IdentifierNode iNodeTemp = new IdentifierNode()
                        {
                            Value = ProtoCore.DSASM.Constants.kTempDefaultArg,
                            Name = ProtoCore.DSASM.Constants.kTempDefaultArg,
                            datatype = compileState.TypeSystem.BuildTypeObject(PrimitiveType.kTypeVar, false)
                        };
                        BinaryExpressionNode bNodeTemp = new BinaryExpressionNode();
                        bNodeTemp.LeftNode = iNodeTemp;
                        bNodeTemp.Optr = ProtoCore.DSASM.Operator.assign;
                        bNodeTemp.RightNode = bNode.LeftNode;
                        EmitBinaryExpressionNode(bNodeTemp, ref inferedType);
                        ////duild an inline conditional node for statement: defaultarg = (temp == DefaultArgNode) ? defaultValue : temp;
                        //InlineConditionalNode icNode = new InlineConditionalNode();
                        //BinaryExpressionNode cExprNode = new BinaryExpressionNode();
                        //cExprNode.Optr = ProtoCore.DSASM.Operator.eq;
                        //cExprNode.LeftNode = iNodeTemp;
                        //cExprNode.RightNode = new DefaultArgNode();
                        //icNode.ConditionExpression = cExprNode;
                        //icNode.TrueExpression = bNode.RightNode;
                        //icNode.FalseExpression = iNodeTemp;
                        //bNodeTemp.LeftNode = bNode.LeftNode;
                        //bNodeTemp.RightNode = icNode;
                        //EmitBinaryExpressionNode(bNodeTemp, ref inferedType);
                    }

                    // Traverse definition
                    foreach (AssociativeNode bnode in funcDef.FunctionBody.Body)
                    {

                        //
                        // TODO Jun:    Handle stand alone language blocks
                        //              Integrate the subPass into a proper pass
                        //

                        ProtoCore.Type itype = new ProtoCore.Type();
                        itype.UID = (int)PrimitiveType.kTypeVar;

                        if (bnode is LanguageBlockNode)
                        {
                            // Build a binaryn node with a temporary lhs for every stand-alone language block
                            IdentifierNode iNode = new IdentifierNode()
                            {
                                Value = ProtoCore.DSASM.Constants.kTempLangBlock,
                                Name = ProtoCore.DSASM.Constants.kTempLangBlock,
                                datatype = compileState.TypeSystem.BuildTypeObject(PrimitiveType.kTypeVar, false)
                            };
                            BinaryExpressionNode langBlockNode = new BinaryExpressionNode();
                            langBlockNode.LeftNode = iNode;
                            langBlockNode.Optr = ProtoCore.DSASM.Operator.assign;
                            langBlockNode.RightNode = bnode;

                            //DfsTraverse(bnode, ref itype, false, null, ProtoCore.DSASM.AssociativeSubCompilePass.kNone);
                            DfsTraverse(langBlockNode, ref itype, false, null, subPass);
                        }
                        else
                        {
                            DfsTraverse(bnode, ref itype, false, null, subPass);
                        }

                        BinaryExpressionNode binaryNode = bnode as BinaryExpressionNode;
                        hasReturnStatement = hasReturnStatement || ((binaryNode != null) && (binaryNode.LeftNode.Name == ProtoCore.DSDefinitions.Kw.kw_return));
                    }

                    // All locals have been stack allocated, update the local count of this function
                    localProcedure.localCount = compileState.BaseOffset;

                    if (ProtoCore.DSASM.Constants.kInvalidIndex == globalClassIndex)
                    {
                        localProcedure.localCount = compileState.BaseOffset;

                        // Update the param stack indices of this function
                        foreach (ProtoCore.DSASM.SymbolNode symnode in codeBlock.symbolTable.symbolList.Values)
                        {
                            if (symnode.functionIndex == localProcedure.procId && symnode.isArgument)
                            {
                                symnode.index -= localProcedure.localCount;
                            }
                        }
                    }
                    else
                    {
                        compileState.ClassTable.ClassNodes[globalClassIndex].vtable.procList[localProcedure.procId].localCount = compileState.BaseOffset;

                        // Update the param stack indices of this function
                        foreach (ProtoCore.DSASM.SymbolNode symnode in compileState.ClassTable.ClassNodes[globalClassIndex].symbols.symbolList.Values)
                        {
                            if (symnode.functionIndex == localProcedure.procId && symnode.isArgument)
                            {
                                symnode.index -= localProcedure.localCount;
                            }
                        }
                    }

                    ProtoCore.Lang.JILActivationRecord record = new ProtoCore.Lang.JILActivationRecord();
                    record.pc = localProcedure.pc;
                    record.locals = localProcedure.localCount;
                    record.classIndex = globalClassIndex;
                    record.funcIndex = localProcedure.procId;
                    fep = new ProtoCore.Lang.JILFunctionEndPoint(record);
                }
                else
                {
                    ProtoCore.Lang.JILActivationRecord jRecord = new ProtoCore.Lang.JILActivationRecord();
                    jRecord.pc = localProcedure.pc;
                    jRecord.locals = localProcedure.localCount;
                    jRecord.classIndex = globalClassIndex;
                    jRecord.funcIndex = localProcedure.procId;

                    // TODO Jun/Luke: Wrap this into Core.Options and extend if needed
                    /*bool isCSFFI = false;
                    if (isCSFFI)
                    {
                        ProtoCore.Lang.CSFFIActivationRecord record = new ProtoCore.Lang.CSFFIActivationRecord();
                        record.JILRecord = jRecord;
                        record.FunctionName = funcDef.Name;
                        record.ModuleName = funcDef.ExternLibName;
                        record.ModuleType = "dll";
                        record.IsDNI = funcDef.IsDNI;
                        record.ReturnType = funcDef.ReturnType;
                        record.ParameterTypes = localProcedure.argTypeList;
                        fep = new ProtoCore.Lang.CSFFIFunctionEndPoint(record);
                    }
                    else
                    {*/
                        ProtoCore.Lang.FFIActivationRecord record = new ProtoCore.Lang.FFIActivationRecord();
                        record.JILRecord = jRecord;
                        record.FunctionName = funcDef.Name;
                        record.ModuleName = funcDef.ExternLibName;
                        record.ModuleType = "dll";
                        record.IsDNI = funcDef.IsDNI;
                        record.ReturnType = funcDef.ReturnType;
                        record.ParameterTypes = localProcedure.argTypeList;
                        fep = new ProtoCore.Lang.FFIFunctionEndPoint(record);
                    //}
                }


                // Construct the fep arguments
                fep.FormalParams = new ProtoCore.Type[localProcedure.argTypeList.Count];
                fep.BlockScope = this.codeBlock.codeBlockId;
                localProcedure.argTypeList.CopyTo(fep.FormalParams, 0);

                // TODO Jun: 'classIndexAtCallsite' is the class index as it is stored at the callsite function tables
                // Determine whether this still needs to be aligned to the actual 'classIndex' variable
                // The factors that will affect this is whether the 2 function tables (compiler and callsite) need to be merged
                int classIndexAtCallsite = globalClassIndex + 1;
                compileState.FunctionTable.AddFunctionEndPointer(classIndexAtCallsite, funcDef.Name, fep);
            }

            compileState.ProcNode = localProcedure = null;
            globalProcIndex = ProtoCore.DSASM.Constants.kGlobalScope;
            compileState.BaseOffset = 0;
            argOffset = 0;
        }

        private void EmitFunctionCallNode(AssociativeNode node, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone)
        {
            TraverseFunctionCall(node, null, (int)ProtoCore.DSASM.Constants.kInvalidIndex, 0, ref inferedType, graphNode, subPass);
            inferedType.UID = isBooleanOp ? (int)PrimitiveType.kTypeBool : inferedType.UID;
        }

        private void EmitModifierStackNode(AssociativeNode node, ref ProtoCore.Type inferedType)
        {
            ModifierStackNode m = node as ModifierStackNode;
            foreach (AssociativeNode modifierNode in m.ElementNodes)
            {
                DfsTraverse(modifierNode, ref inferedType);
            }
        }

        private void EmitIfStatementNode(AssociativeNode node, ref ProtoCore.Type inferedType, ProtoCore.AssociativeGraph.GraphNode graphNode = null)
        {
            // If-expr
            IfStatementNode ifnode = node as IfStatementNode;
            DfsTraverse(ifnode.ifExprNode, ref inferedType, false, graphNode);


            // Create a new codeblock for this block
            // Set the current codeblock as the parent of the new codeblock
            // Set the new codeblock as a new child of the current codeblock
            // Set the new codeblock as the current codeblock
            ProtoCore.DSASM.CodeBlock localCodeBlock = new ProtoCore.DSASM.CodeBlock(
                ProtoCore.DSASM.CodeBlockType.kConstruct,
                Language.kInvalid,
                compileState.CodeBlockIndex++,
                new ProtoCore.DSASM.SymbolTable(GetConstructBlockName("if"), compileState.RuntimeTableIndex++),
                null);


            localCodeBlock.instrStream = codeBlock.instrStream;
            localCodeBlock.parent = codeBlock;
            codeBlock.children.Add(localCodeBlock);
            codeBlock = localCodeBlock;
            compileState.CodeBlockList.Add(codeBlock);
            // If-body
            foreach (AssociativeNode ifBody in ifnode.IfBody)
            {
                inferedType = new ProtoCore.Type();
                inferedType.UID = (int)PrimitiveType.kTypeVar;
                DfsTraverse(ifBody, ref inferedType, false, graphNode);
            }

            // Restore - Set the local codeblock parent to be the current codeblock
            codeBlock = localCodeBlock.parent;

            Debug.Assert(null != ifnode.ElseBody);
            if (0 != ifnode.ElseBody.Count)
            {
                // Create a new symboltable for this block
                // Set the current table as the parent of the new table
                // Set the new table as a new child of the current table
                // Set the new table as the current table
                // Create a new codeblock for this block
                // Set the current codeblock as the parent of the new codeblock
                // Set the new codeblock as a new child of the current codeblock
                // Set the new codeblock as the current codeblock
                localCodeBlock = new ProtoCore.DSASM.CodeBlock(
                    ProtoCore.DSASM.CodeBlockType.kConstruct,
                    Language.kInvalid,
                    compileState.CodeBlockIndex++,
                    new ProtoCore.DSASM.SymbolTable(GetConstructBlockName("else"), compileState.RuntimeTableIndex++),
                    null);

                localCodeBlock.instrStream = codeBlock.instrStream;
                localCodeBlock.parent = codeBlock;
                codeBlock.children.Add(localCodeBlock);
                codeBlock = localCodeBlock;
                compileState.CodeBlockList.Add(codeBlock);
                foreach (AssociativeNode elseBody in ifnode.ElseBody)
                {
                    inferedType = new ProtoCore.Type();
                    inferedType.UID = (int)PrimitiveType.kTypeVar;
                    DfsTraverse(elseBody, ref inferedType, false, graphNode);
                }

                // Restore - Set the local codeblock parent to be the current codeblock
                codeBlock = localCodeBlock.parent;
            }
        }

        private void EmitInlineConditionalNode(AssociativeNode node, ref ProtoCore.Type inferedType, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone)
        {
            if (ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier == subPass)
            {
                return;
            }

            InlineConditionalNode inlineConNode = node as InlineConditionalNode;
            IfStatementNode ifNode = new IfStatementNode();
            ifNode.ifExprNode = inlineConNode.ConditionExpression;
            List<AssociativeNode> ifBody = new List<AssociativeNode>();
            List<AssociativeNode> elseBody = new List<AssociativeNode>();
            ifBody.Add(inlineConNode.TrueExpression);
            elseBody.Add(inlineConNode.FalseExpression);
            ifNode.IfBody = ifBody;
            ifNode.ElseBody = elseBody;

            EmitIfStatementNode(ifNode, ref inferedType, graphNode);
        }

        private void EmitUnaryExpressionNode(AssociativeNode node, ref ProtoCore.Type inferedType, ProtoCore.AssociativeGraph.GraphNode graphNode = null)
        {
            UnaryExpressionNode u = node as UnaryExpressionNode;
            bool isPrefixOperation = ProtoCore.DSASM.UnaryOperator.Increment == u.Operator || ProtoCore.DSASM.UnaryOperator.Decrement == u.Operator;

            //(Ayush) In case of prefix, apply prefix operation first
            if (isPrefixOperation)
            {
                if (u.Expression is IdentifierListNode || u.Expression is IdentifierNode)
                {
                    BinaryExpressionNode binRight = new BinaryExpressionNode();
                    BinaryExpressionNode bin = new BinaryExpressionNode();
                    binRight.LeftNode = u.Expression;
                    binRight.RightNode = new IntNode() { value = "1" };
                    binRight.Optr = (ProtoCore.DSASM.UnaryOperator.Increment == u.Operator) ? ProtoCore.DSASM.Operator.add : ProtoCore.DSASM.Operator.sub;
                    bin.LeftNode = u.Expression; bin.RightNode = binRight; bin.Optr = ProtoCore.DSASM.Operator.assign;
                    EmitBinaryExpressionNode(bin, ref inferedType);
                }
                else
                    throw new BuildHaltException("Invalid use of prefix operation.");
            }

            DfsTraverse(u.Expression, ref inferedType, false, graphNode);
        }

        private void EmitBinaryExpressionNode(AssociativeNode node, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone)
        {
            if (!IsParsingGlobal() && !IsParsingGlobalFunctionBody() && !IsParsingMemberFunctionBody())
                return;

            bool isBooleanOperation = false;
            BinaryExpressionNode bnode = node as BinaryExpressionNode;

            ProtoCore.Type leftType = new ProtoCore.Type();
            leftType.UID = (int)ProtoCore.PrimitiveType.kTypeVar;
            leftType.IsIndexable = false;

            ProtoCore.Type rightType = new ProtoCore.Type();
            rightType.UID = (int)ProtoCore.PrimitiveType.kTypeVar;
            rightType.IsIndexable = false;

            // If this is an assignment statement, setup the top level graph node
            if (ProtoCore.DSASM.Operator.assign != bnode.Optr)
            {
                // Traversing the left node if this binary expression is not an assignment
                //
                isBooleanOperation = ProtoCore.DSASM.Operator.lt == bnode.Optr
                     || ProtoCore.DSASM.Operator.gt == bnode.Optr
                     || ProtoCore.DSASM.Operator.le == bnode.Optr
                     || ProtoCore.DSASM.Operator.ge == bnode.Optr
                     || ProtoCore.DSASM.Operator.eq == bnode.Optr
                     || ProtoCore.DSASM.Operator.nq == bnode.Optr
                     || ProtoCore.DSASM.Operator.and == bnode.Optr
                     || ProtoCore.DSASM.Operator.or == bnode.Optr;

                DfsTraverse(bnode.LeftNode, ref inferedType, isBooleanOperation, graphNode, subPass);

                leftType.UID = inferedType.UID;
                leftType.IsIndexable = inferedType.IsIndexable;
            }

            int startpc = ProtoCore.DSASM.Constants.kInvalidIndex;
            // (Ayush) in case of PostFixNode, only traverse the identifier now. Post fix operation will be applied later.
#if ENABLE_INC_DEC_FIX
                if (bnode.RightNode is PostFixNode)
                {
                    DfsTraverse((bnode.RightNode as PostFixNode).Identifier, ref inferedType, isBooleanOperation, graphNode);
                }
                else
                {
#endif
            if ((ProtoCore.DSASM.Operator.assign == bnode.Optr) && (bnode.RightNode is LanguageBlockNode))
            {
                inferedType.UID = (int)ProtoCore.PrimitiveType.kTypeVar;
                inferedType.IsIndexable = false;
            }

            if (null != localProcedure && localProcedure.isConstructor && setConstructorStartPC)
            {
                startpc -= 1;
                setConstructorStartPC = false;
            }

            DfsTraverse(bnode.RightNode, ref inferedType, isBooleanOperation, graphNode, subPass);
#if ENABLE_INC_DEC_FIX
                }
#endif

            rightType.UID = inferedType.UID;
            rightType.IsIndexable = inferedType.IsIndexable;

            BinaryExpressionNode rightNode = bnode.RightNode as BinaryExpressionNode;
            if ((rightNode != null) && (ProtoCore.DSASM.Operator.assign == rightNode.Optr))
            {
                DfsTraverse(rightNode.LeftNode, ref inferedType, false, graphNode);
            }

            if (bnode.Optr != ProtoCore.DSASM.Operator.assign)
            {
                if (subPass == ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier)
                {
                    return;
                }
                isBooleanOp = false;

                //if post fix, now traverse the post fix
#if ENABLE_INC_DEC_FIX
                if (bnode.RightNode is PostFixNode)
                    EmitPostFixNode(bnode.RightNode, ref inferedType);
#endif
                return;
            }

            DfsTraverse(bnode.RightNode, ref inferedType, isBooleanOperation, graphNode, ProtoCore.DSASM.AssociativeSubCompilePass.kNone);
            subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier;

            if (bnode.LeftNode is IdentifierNode)
            {
                // TODO Jun: Cleansify this block where the lhs is being handled.
                // For one, make the return as a return node
                IdentifierNode t = bnode.LeftNode as IdentifierNode;
                ProtoCore.DSASM.SymbolNode symbolnode = null;

                string s = t.Value;
                if (s == ProtoCore.DSDefinitions.Kw.kw_return)
                {
                    Debug.Assert(null == symbolnode);
                    symbolnode = new ProtoCore.DSASM.SymbolNode();
                    symbolnode.name = s;
                    symbolnode.functionIndex = globalProcIndex;
                    symbolnode.classScope = globalClassIndex;

                    EmitReturnStatement(node, inferedType);
                }
                else
                {
                    {
                        // check whether the variable name is a function name
                        bool isAccessibleFp;
                        int realType;
                        ProtoCore.DSASM.ProcedureNode procNode = null;
                        if (globalClassIndex != ProtoCore.DSASM.Constants.kGlobalScope)
                        {
                            procNode = compileState.ClassTable.ClassNodes[globalClassIndex].GetMemberFunction(t.Name, null, globalClassIndex, out isAccessibleFp, out realType);
                        }
                        if (procNode == null)
                        {
                            procNode = compileState.GetFirstVisibleProcedure(t.Name, null, codeBlock);
                        }
                    }

                    //int type = (int)ProtoCore.PrimitiveType.kTypeVoid;
                    bool isAccessible = false;
                    bool isAllocated = VerifyAllocation(t.Name, globalClassIndex, out symbolnode, out isAccessible);
                    int runtimeIndex = (!isAllocated || !isAccessible) ? codeBlock.symbolTable.runtimeIndex : symbolnode.runtimeTableIndex;

                    int dimensions = 0;
                    if (null != t.ArrayDimensions)
                    {
                        dimensions = DfsEmitArrayIndexHeap(t.ArrayDimensions);
                    }

                    ProtoCore.Type castType = compileState.TypeSystem.BuildTypeObject((int)PrimitiveType.kTypeVar, false);
                    var tident = bnode.LeftNode as TypedIdentifierNode;
                    if (tident != null)
                    {
                        int castUID = tident.datatype.UID;
                        if ((int)PrimitiveType.kInvalidType == castUID)
                        {
                            castUID = compileState.ClassTable.IndexOf(tident.datatype.Name);
                        }

                        if ((int)PrimitiveType.kInvalidType == castUID)
                        {
                            castType = compileState.TypeSystem.BuildTypeObject((int)PrimitiveType.kInvalidType, false);
                            castType.Name = tident.datatype.Name;
                            castType.rank = tident.datatype.rank;
                            castType.IsIndexable = (castType.rank != 0);
                        }
                        else
                        {
                            castType = compileState.TypeSystem.BuildTypeObject(castUID, tident.datatype.IsIndexable, tident.datatype.rank);
                        }
                    }

                    if (ProtoCore.DSASM.Constants.kInvalidIndex != globalClassIndex)
                    {
                        // In a class

                        // TODO Jun: refactor this by having symbol table functions for retrieval of node index
                        int symbol = ProtoCore.DSASM.Constants.kInvalidIndex;
                        bool isMemVar = false;
                        if (symbolnode != null)
                        {
                            if (symbolnode.classScope != ProtoCore.DSASM.Constants.kInvalidIndex &&
                                symbolnode.functionIndex == ProtoCore.DSASM.Constants.kGlobalScope)
                            {
                                isMemVar = true;
                            }
                            symbol = symbolnode.symbolTableIndex;
                        }

                        if (!isMemVar)
                        {
                            // This is local variable
                            // TODO Jun: If this local var exists globally, should it allocate a local copy?
                            if (!isAllocated || !isAccessible)
                            {
                                symbolnode = Allocate(globalClassIndex, globalClassIndex, globalProcIndex, t.Name, inferedType);
                                Debug.Assert(symbolnode != null);
                                IdentLocation.AddEntry(symbolnode, t.line, t.col, compileState.CurrentDSFileName);
                            }

                            symbol = symbolnode.symbolTableIndex;

                            if (bnode.LeftNode is TypedIdentifierNode)
                            {
                                symbolnode.SetStaticType(castType);
                            }
                        }
                        else
                        {
                            if (bnode.LeftNode is TypedIdentifierNode)
                            {
                                symbolnode.SetStaticType(castType);
                            }
                        }
                    }
                    else
                    {
                        if (0 == dimensions)
                        {
                            // Not indexing an array, allocate memory for this 
                            if (!isAllocated)
                            {
                                symbolnode = Allocate(globalClassIndex, globalClassIndex, globalProcIndex, t.Name, inferedType);
                                Debug.Assert(symbolnode != null);
                                IdentLocation.AddEntry(symbolnode, t.line, t.col, compileState.CurrentDSFileName);
                            }
                        }

                        if (bnode.LeftNode is TypedIdentifierNode)
                        {
                            symbolnode.SetStaticType(castType);
                        }
                    }
                }

            }
            else if (bnode.LeftNode is IdentifierListNode)
            {
                int depth = 0;

                ProtoCore.Type lastType = new ProtoCore.Type();
                lastType.UID = (int)PrimitiveType.kInvalidType;
                lastType.IsIndexable = false;

                bool isFirstIdent = false;
                bool isIdentReference = DfsEmitIdentList(bnode.LeftNode, bnode, globalClassIndex, ref lastType, ref depth, ref inferedType, true, ref isFirstIdent, graphNode, subPass);


                // Jun: Ge the left most identifier node and use that as the parent graph
                // This may have to go through re-design if we want full dependency on a.b.c...
                IdentifierListNode listNode = bnode.LeftNode as IdentifierListNode;
                int leftClassIndex = ProtoCore.DSASM.Constants.kInvalidIndex;
                if (listNode.LeftNode is IdentifierNode)
                {
                    leftClassIndex = compileState.ClassTable.IndexOf(listNode.LeftNode.Name);
                }
                AssociativeNode iNode = (listNode.LeftNode.Name == ProtoCore.DSDefinitions.Kw.kw_this ||
                                         leftClassIndex != ProtoCore.DSASM.Constants.kInvalidIndex) ? listNode.RightNode : listNode.LeftNode;
                while (iNode is IdentifierListNode)
                {
                    if ((iNode as IdentifierListNode).LeftNode.Name != ProtoCore.DSDefinitions.Kw.kw_this)
                    {
                        iNode = (iNode as IdentifierListNode).LeftNode;
                    }
                    else
                    {
                        iNode = (iNode as IdentifierListNode).RightNode;
                    }
                }

                Debug.Assert(iNode is IdentifierNode);
                iNode = iNode as IdentifierNode;
                if (null != iNode)
                {
                    ProtoCore.DSASM.SymbolNode symnode = null;
                    bool isAccessible = false;

                    int oldClassIndex = globalClassIndex;
                    if (leftClassIndex != ProtoCore.DSASM.Constants.kInvalidIndex)
                        globalClassIndex = leftClassIndex;

                    bool isAllocated = VerifyAllocation(iNode.Name, globalClassIndex, out symnode, out isAccessible);
                    globalClassIndex = oldClassIndex;
                }
            }

            //if post fix, now traverse the post fix
#if ENABLE_INC_DEC_FIX
                if (bnode.RightNode is PostFixNode)
                    EmitPostFixNode(bnode.RightNode, ref inferedType);
#endif
        }

        private void EmitImportNode(AssociativeNode node, ref ProtoCore.Type inferedType, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone)
        {
            ImportNode importNode = node as ImportNode;
            CodeBlockNode codeBlockNode = importNode.CodeNode;
            string origSourceLocation = compileState.CurrentDSFileName;
            compileState.CurrentDSFileName = importNode.ModuleName;

            if (compilePass == ProtoCore.DSASM.AssociativeCompilePass.kClassName)
            {
                CodeRangeTable.AddCodeBlockRangeEntry(codeBlock.codeBlockId,
                    0, 0, int.MaxValue, int.MaxValue, compileState.CurrentDSFileName);
                ImportTable.AddEntry(origSourceLocation, compileState.CurrentDSFileName);
            }
            if (codeBlockNode != null)
            {
                foreach (AssociativeNode assocNode in codeBlockNode.Body)
                {
                    DfsTraverse(assocNode, ref inferedType, false, null, subPass);
                }
            }
            compileState.CurrentDSFileName = origSourceLocation;
        }

        protected void EmitExceptionHandlingNode(AssociativeNode node, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone)
        {
            if (!IsParsingGlobal() && !IsParsingGlobalFunctionBody() && !IsParsingMemberFunctionBody())
                return;

            tryLevel++;

            ExceptionHandlingNode exceptionNode = node as ExceptionHandlingNode;
            if (exceptionNode == null)
                return;

            ExceptionHandler exceptionHandler = new ExceptionHandler();
            exceptionHandler.TryLevel = tryLevel;

            ExceptionRegistration registration = compileState.ExceptionHandlingManager.ExceptionTable.GetExceptionRegistration(codeBlock.codeBlockId, globalProcIndex, globalClassIndex);
            if (registration == null)
            {
                registration = compileState.ExceptionHandlingManager.ExceptionTable.Register(codeBlock.codeBlockId, globalProcIndex, globalClassIndex);
                Debug.Assert(registration != null);
            }
            registration.Add(exceptionHandler);

            TryBlockNode tryNode = exceptionNode.tryBlock;
            Debug.Assert(tryNode != null);
            foreach (var subnode in tryNode.body)
            {
                ProtoCore.Type inferedType = new ProtoCore.Type();
                inferedType.UID = (int)ProtoCore.PrimitiveType.kTypeVar;
                inferedType.IsIndexable = false;
                DfsTraverse(subnode, ref inferedType, false, graphNode, subPass);
            }

            foreach (var catchBlock in exceptionNode.catchBlocks)
            {
                CatchHandler catchHandler = new CatchHandler();
                exceptionHandler.AddCatchHandler(catchHandler);

                CatchFilterNode filterNode = catchBlock.catchFilter;
                Debug.Assert(filterNode != null);
                catchHandler.FilterTypeUID = compileState.TypeSystem.GetType(filterNode.type.Name);

                ProtoCore.Type inferedType = new ProtoCore.Type();
                inferedType.UID = (int)ProtoCore.PrimitiveType.kTypeVar;
                inferedType.IsIndexable = false;
                DfsTraverse(filterNode.var, ref inferedType, false, graphNode, subPass);

                foreach (var subnode in catchBlock.body)
                {
                    inferedType.UID = (int)ProtoCore.PrimitiveType.kTypeVar;
                    inferedType.IsIndexable = false;
                    DfsTraverse(subnode, ref inferedType, false, graphNode, subPass);
                }

                // Jmp to code after catch block                
            }

            tryLevel--;
        }

        protected FunctionCallNode EmitGetterSetterForIdent(IdentifierNode inode, bool isSetter = false)
        {
            if (isSetter)
            {
                FunctionCallNode setter = new FunctionCallNode();
                setter.Function = inode;

                IdentifierNode tmpArg = new IdentifierNode();
                tmpArg.Name = tmpArg.Value = ProtoCore.DSASM.Constants.kTempArg;
                tmpArg.datatype = compileState.TypeSystem.BuildTypeObject(PrimitiveType.kTypeVar, false);
                setter.FormalArguments.Add(tmpArg);

                return setter;
            }
            else
            {
                FunctionCallNode getter = new FunctionCallNode();
                getter.Function = inode;
                return getter;
            }
        }

        protected void EmitGetterSetterForIdentList(ProtoCore.AST.Node node, bool isSetter = false)
        {
            IdentifierListNode inode = node as IdentifierListNode;
            if (isSetter && inode != null)
            {
                // replace right-most property with setter
                if (inode.RightNode is IdentifierNode)
                {
                    FunctionCallNode setter = EmitGetterSetterForIdent(inode.RightNode as IdentifierNode, true);
                    inode.RightNode = setter;
                }
                if (inode.LeftNode is IdentifierNode)
                {
                    FunctionCallNode getter = EmitGetterSetterForIdent(inode.LeftNode as IdentifierNode, false);
                    inode.LeftNode = getter;
                    return;
                }
                inode = inode.LeftNode as IdentifierListNode;
            }

            while (inode != null)
            {
                if (inode.RightNode is IdentifierNode)
                {
                    FunctionCallNode getter = EmitGetterSetterForIdent(inode.RightNode as IdentifierNode, false);
                    inode.RightNode = getter;
                }

                if (inode.LeftNode is IdentifierNode)
                {
                    FunctionCallNode getter = EmitGetterSetterForIdent(inode.LeftNode as IdentifierNode, false);
                    inode.LeftNode = getter;
                    return;
                }

                inode = inode.LeftNode as IdentifierListNode;
            }
        }

        private bool IsParsingGlobal()
        {
            return (null == localProcedure) && (ProtoCore.DSASM.AssociativeCompilePass.kGlobalScope == compilePass);
        }

        private bool IsParsingGlobalFunctionBody()
        {
            return (null != localProcedure) && (ProtoCore.DSASM.AssociativeCompilePass.kGlobalFuncBody == compilePass);
        }

        private bool IsParsingMemberFunctionBody()
        {
            return (ProtoCore.DSASM.Constants.kGlobalScope != globalClassIndex) && (ProtoCore.DSASM.AssociativeCompilePass.kClassMemFuncBody == compilePass);
        }

        private bool IsParsingGlobalFunctionSig()
        {
            return (null == localProcedure) && (ProtoCore.DSASM.AssociativeCompilePass.kGlobalFuncSig == compilePass);
        }

        private bool IsParsingMemberFunctionSig()
        {
            return (ProtoCore.DSASM.Constants.kGlobalScope != globalClassIndex) && (ProtoCore.DSASM.AssociativeCompilePass.kClassMemFuncSig == compilePass);
        }

        protected override void DfsTraverse(ProtoCore.AST.Node pNode, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone)
        {
            AssociativeNode node = pNode as AssociativeNode;
            if ((null == node) || node.skipMe)
                return;

            if (node is IdentifierNode)
            {
                EmitIdentifierNode(node, ref inferedType, isBooleanOp, graphNode, subPass);
            }
            else if (node is IntNode)
            {
                EmitIntNode(node, ref inferedType, isBooleanOp, subPass);
            }
            else if (node is DoubleNode)
            {
                EmitDoubleNode(node, ref inferedType, isBooleanOp, subPass);
            }
            else if (node is BooleanNode)
            {
                EmitBooleanNode(node, ref inferedType, subPass);
            }
            else if (node is CharNode)
            {
                EmitCharNode(node, ref inferedType, isBooleanOp, subPass);
            }
            else if (node is StringNode)
            {
                EmitStringNode(node, ref inferedType, subPass);
            }
            else if (node is NullNode)
            {
                EmitNullNode(node, ref inferedType, isBooleanOp, subPass);
            }
#if ENABLE_INC_DEC_FIX
            else if (node is PostFixNode)
            {
                EmitPostFixNode(node, ref inferedType);
            }
#endif
            else if (node is ReturnNode)
            {
                EmitReturnNode(node);
            }
            else if (node is RangeExprNode)
            {
                EmitRangeExprNode(node, ref inferedType, subPass);
            }
            else if (node is ForLoopNode)
            {
                EmitForLoopNode(node);
            }
            else if (node is LanguageBlockNode)
            {
                EmitLanguageBlockNode(node, ref inferedType, subPass);
            }
            else if (node is ClassDeclNode)
            {
                EmitClassDeclNode(node, ref inferedType, subPass);
            }
            else if (node is ConstructorDefinitionNode)
            {
                EmitConstructorDefinitionNode(node, ref inferedType, subPass);
            }
            else if (node is FunctionDefinitionNode)
            {
                EmitFunctionDefinitionNode(node, ref inferedType, subPass);
            }
            else if (node is FunctionCallNode)
            {
                EmitFunctionCallNode(node, ref inferedType, isBooleanOp, graphNode, subPass);
            }
            else if (node is ModifierStackNode)
            {
                EmitModifierStackNode(node, ref inferedType);
            }
            else if (node is ExprListNode)
            {
                ScopedArray scopedArray = null;
                ArrayElementType parentElementType = this.arrayElementType;

                // The same code is parsed in multiple passes, don't populate array element type table all 
                // the times, we should only do it when all things are parsed.
                bool generateArrayElementTypes = subPass == ProtoCore.DSASM.AssociativeSubCompilePass.kNone;

                if (generateArrayElementTypes && (null != CoreCodeGen.arrayTypeTable))
                {
                    ScopeInfo scopeInfo = new ScopeInfo()
                    {
                        // For global scope, "localProcedure" will be of null value.
                        BlockId = ((null == localProcedure) ? 0 : localProcedure.runtimeIndex),
                        ClassScope = ((null == localProcedure) ? -1 : localProcedure.classScope),
                        FunctionIndex = ((null == localProcedure) ? -1 : localProcedure.procId)
                    };

                    node.Name = node.Name == null ? string.Empty : node.Name;
                    if (null != parentElementType || (!string.IsNullOrEmpty(node.Name)))
                    {
                        scopedArray = new ScopedArray(scopeInfo, node.Name);
                        this.arrayElementType = new ArrayElementType();
                    }
                }

                EmitExprListNode(node, ref inferedType, graphNode, subPass);

                if (generateArrayElementTypes && (null != CoreCodeGen.arrayTypeTable))
                {
                    if (null == parentElementType)
                    {
                        CoreCodeGen.arrayTypeTable.InsertRootElementType(
                            scopedArray, this.arrayElementType);

                        this.arrayElementType = null;
                    }
                    else
                    {
                        parentElementType.AppendChild(this.arrayElementType);
                        this.arrayElementType = parentElementType;
                    }
                }
            }
            else if (node is IdentifierListNode)
            {
                EmitIdentifierListNode(node, ref inferedType, isBooleanOp, graphNode, subPass);
            }
            else if (node is IfStatementNode)
            {
                EmitIfStatementNode(node, ref inferedType);
            }
            else if (node is InlineConditionalNode)
            {
                EmitInlineConditionalNode(node, ref inferedType, graphNode, subPass);
            }
            else if (node is UnaryExpressionNode)
            {
                EmitUnaryExpressionNode(node, ref inferedType, graphNode);
            }
            else if (node is BinaryExpressionNode)
            {
                EmitBinaryExpressionNode(node, ref inferedType, isBooleanOp, graphNode, subPass);
            }
            else if (node is ImportNode)
            {
                EmitImportNode(node, ref inferedType, subPass);
            }
            else if (node is ExceptionHandlingNode)
            {
                EmitExceptionHandlingNode(node, graphNode, subPass);
            }
            else if (node is ThrowNode)
            {
            }
        }
    }
}
