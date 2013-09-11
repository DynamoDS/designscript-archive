using System;
using System.IO;
using ProtoCore.AST;
using System.Diagnostics;
using ProtoCore;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DesignScript.Editor.AutoComplete;

namespace DesignScript.Editor.CodeGen
{
    abstract class CoreCodeGen
    {
        // IDE-SPECIFIC: This is only in IDE code generator.
        protected ArrayElementType arrayElementType = null;
        internal static ArrayTypeTable arrayTypeTable = null;

        protected ProtoLanguage.CompileStateTracker compileState;
        protected int argOffset;
        protected int classOffset;
        protected int globalClassIndex;
        protected int targetLangBlock;
        protected int blockScope;
        protected bool enforceTypeCheck;
        public ProtoCore.DSASM.CodeBlock codeBlock { get; set; }
        public ProtoCore.CompileTime.Context context { get; set; }
        protected int globalProcIndex;
        protected ProtoCore.DSASM.ProcedureNode localProcedure;
        protected int tryLevel;

        internal static AutoCompleteEngine AutoCompleteEngine { get; set; }
        internal static CodeRangeTable CodeRangeTable { get; set; }
        internal static IdentLocationTable IdentLocation { get; set; }
        internal static ImportTable ImportTable { get; set; }

        internal static void ResetInternalStates()
        {
            CoreCodeGen.CodeRangeTable = null;
            CoreCodeGen.IdentLocation = null;
            CoreCodeGen.ImportTable = null;
        }

        public CoreCodeGen(ProtoLanguage.CompileStateTracker compileState, ProtoCore.DSASM.CodeBlock parentBlock = null)
        {
            Debug.Assert(compileState != null);
            this.compileState = compileState;
            argOffset = 0;
            globalClassIndex = compileState.ClassIndex;

            if (null == CoreCodeGen.CodeRangeTable)
                CoreCodeGen.CodeRangeTable = new CodeRangeTable();
            if (null == CoreCodeGen.IdentLocation)
                CoreCodeGen.IdentLocation = new IdentLocationTable();
            if (null == CoreCodeGen.ImportTable)
                CoreCodeGen.ImportTable = new ImportTable();

            context = new ProtoCore.CompileTime.Context();

            targetLangBlock = ProtoCore.DSASM.Constants.kInvalidIndex;

            enforceTypeCheck = true;

            localProcedure = compileState.ProcNode;
            globalProcIndex = null != localProcedure ? localProcedure.procId : ProtoCore.DSASM.Constants.kGlobalScope;

            tryLevel = 0;
        }

        protected string GetConstructBlockName(string construct)
        {
            string desc = "blockname";
            return blockScope.ToString() + "_" + construct + "_" + desc;
        }

        protected ProtoCore.DSASM.AddressType GetOpType(ProtoCore.PrimitiveType type)
        {
            ProtoCore.DSASM.AddressType optype = ProtoCore.DSASM.AddressType.Int;
            // Data coercion for the prototype
            // The JIL executive handles int primitives
            if (ProtoCore.PrimitiveType.kTypeInt == type
                || ProtoCore.PrimitiveType.kTypeBool == type
                || ProtoCore.PrimitiveType.kTypeChar == type
                || ProtoCore.PrimitiveType.kTypeString == type)
            {
                optype = ProtoCore.DSASM.AddressType.Int;
            }
            else if (ProtoCore.PrimitiveType.kTypeDouble == type)
            {
                optype = ProtoCore.DSASM.AddressType.Double;
            }
            else if (ProtoCore.PrimitiveType.kTypeVar == type)
            {
                optype = ProtoCore.DSASM.AddressType.VarIndex;
            }
            else if (ProtoCore.PrimitiveType.kTypeReturn == type)
            {
                optype = ProtoCore.DSASM.AddressType.Register;
            }
            else
            {
                Debug.Assert(false);
            }
            return optype;
        }

        protected void AllocateVar(ProtoCore.DSASM.SymbolNode symbol)
        {
            symbol.isArray = false;
            if (ProtoCore.DSASM.MemoryRegion.kMemHeap == symbol.memregion)
            {
                SetHeapData(symbol);
            }
            SetStackIndex(symbol);
        }

        protected void SetHeapData(ProtoCore.DSASM.SymbolNode symbol)
        {
            symbol.size = ProtoCore.DSASM.Constants.kPointerSize;
            symbol.heapIndex = compileState.GlobHeapOffset++;
        }

        protected void SetStackIndex(ProtoCore.DSASM.SymbolNode symbol)
        {
            if ((int)ProtoCore.DSASM.Constants.kGlobalScope != globalClassIndex)
            {
                if (null != localProcedure)
                {
                    // Local variable in a member function
                    symbol.index = -1 - ProtoCore.DSASM.StackFrame.kStackFrameSize - compileState.BaseOffset;
                    compileState.BaseOffset += symbol.size;
                }
                else
                {
                    // Member variable: static variable allocated on global
                    // stack
                    if (symbol.isStatic)
                    {
                        symbol.index = compileState.GlobOffset;
                        compileState.GlobOffset += symbol.size;
                    }
                    else
                    {
                        symbol.index = classOffset;
                        classOffset += symbol.size;
                    }
                }
            }
            else if (null != localProcedure)
            {
                // Local variable in a global function
                symbol.index = -1 - ProtoCore.DSASM.StackFrame.kStackFrameSize - compileState.BaseOffset;
                compileState.BaseOffset += symbol.size;
            }
            else
            {
                // Global variable
                symbol.index = compileState.GlobOffset;
                compileState.GlobOffset += symbol.size;
            }
        }

        protected bool DfsEmitIdentList(Node pNode, Node parentNode, int contextClassScope, ref ProtoCore.Type lefttype, ref int depth, ref ProtoCore.Type finalType, bool isLeftidentList, ref bool isFirstIdent, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone)
        {
            bool isRefFromIdentifier = false;

            dynamic node = pNode;
            if (node is ProtoCore.AST.ImperativeAST.IdentifierListNode || node is ProtoCore.AST.AssociativeAST.IdentifierListNode)
            {
                dynamic bnode = node;
                if (ProtoCore.DSASM.Operator.dot != bnode.Optr)
                {
                    throw new BuildHaltException();
                }

                isRefFromIdentifier = DfsEmitIdentList(bnode.LeftNode, bnode, contextClassScope, ref lefttype, ref depth, ref finalType, isLeftidentList, ref isFirstIdent, graphNode, subPass);

                if (lefttype.rank > 0)
                {
                    lefttype.UID = finalType.UID = (int)PrimitiveType.kTypeNull;
                    return false;
                }
                node = bnode.RightNode;
            }

            if (node is ProtoCore.AST.ImperativeAST.IdentifierNode || node is ProtoCore.AST.AssociativeAST.IdentifierNode)
            {
                dynamic identnode = node;

                int ci = compileState.ClassTable.IndexOf(identnode.Value);
                if (ProtoCore.DSASM.Constants.kInvalidIndex != ci)
                {
                    finalType.UID = lefttype.UID = ci;
                }
                else if (identnode.Value == ProtoCore.DSDefinitions.Kw.kw_this)
                {
                    finalType.UID = lefttype.UID = contextClassScope;
                    depth++;
                    return true;
                }
                else
                {
                    ProtoCore.DSASM.SymbolNode symbolnode = null;
                    bool isAllocated = false;
                    bool isAccessible = false;
                    if (lefttype.UID != -1)
                    {
                        isAllocated = VerifyAllocation(identnode.Value, lefttype.UID, out symbolnode, out isAccessible);
                    }
                    else
                    {
                        isAllocated = VerifyAllocation(identnode.Value, contextClassScope, out symbolnode, out isAccessible);
                    }

                    bool callOnClass = false;
                    if (pNode is ProtoCore.AST.ImperativeAST.IdentifierListNode ||
                        pNode is ProtoCore.AST.AssociativeAST.IdentifierListNode)
                    {
                        dynamic leftnode = ((dynamic)pNode).LeftNode;
                        if (leftnode != null &&
                            (leftnode is ProtoCore.AST.ImperativeAST.IdentifierNode ||
                            leftnode is ProtoCore.AST.AssociativeAST.IdentifierNode))
                        {
                            string leftClassName = leftnode.Name;
                            int leftci = compileState.ClassTable.IndexOf(leftClassName);
                            if (leftci != ProtoCore.DSASM.Constants.kInvalidIndex)
                            {
                                callOnClass = true;

                                depth = depth + 1;
                            }
                        }
                    }

                    if (null == symbolnode)    //unbound identifier
                    {
                        if (isAllocated && !isAccessible)
                        {
                            lefttype.UID = finalType.UID = (int)PrimitiveType.kTypeNull;
                            return false;
                        }

                        if (depth == 0)
                        {
                            lefttype.UID = finalType.UID = (int)PrimitiveType.kTypeNull;
                            return false;
                        }
                        else
                        {
                            ProtoCore.DSASM.DyanmicVariableNode dynamicVariableNode = new ProtoCore.DSASM.DyanmicVariableNode(identnode.Value, globalProcIndex, globalClassIndex);
                            compileState.DynamicVariableTable.variableTable.Add(dynamicVariableNode);
                            int dim = 0;
                            if (null != identnode.ArrayDimensions)
                            {
                                dim = DfsEmitArrayIndexHeap(identnode.ArrayDimensions);
                            }
                            lefttype.UID = finalType.UID = (int)PrimitiveType.kTypeVar;
                            depth++;
                            return true;
                        }
                    }
                    else
                    {
                        if (callOnClass && !symbolnode.isStatic)
                        {
                            lefttype.UID = finalType.UID = (int)PrimitiveType.kTypeNull;
                            return false;
                        }
                    }
                    lefttype = symbolnode.datatype;

                    int dimensions = 0;

                    // Get the symbols' table index
                    int runtimeIndex = symbolnode.runtimeTableIndex;

                    ProtoCore.DSASM.AddressType operandType = ProtoCore.DSASM.AddressType.Pointer;

                    if (null != identnode.ArrayDimensions)
                    {
                        /* Remove static type checking
                        if (lefttype.UID != (int)PrimitiveType.kTypeArray && !lefttype.IsIndexable)
                            buildStatus.LogWarning(BuildData.WarningID.kWarnMax, string.Format("'{0}' is not indexable at compile time", identnode.Name));
                        */
                        dimensions = DfsEmitArrayIndexHeap(identnode.ArrayDimensions);
                        operandType = ProtoCore.DSASM.AddressType.ArrayPointer;
                    }

                    //update the rank
                    if (lefttype.rank >= 0)
                    {
                        lefttype.rank -= dimensions;
                        if (lefttype.rank < 0)
                        {
                            //throw new Exception("Exceed maximum rank!");
                            lefttype.rank = 0;
                        }
                    }

                    if (0 == depth || (symbolnode != null && symbolnode.isStatic))
                    {
                        if (ProtoCore.DSASM.Constants.kGlobalScope == symbolnode.functionIndex
                            && ProtoCore.DSASM.Constants.kInvalidIndex != symbolnode.classScope)
                        {
                            // member var
                            operandType = symbolnode.isStatic ? ProtoCore.DSASM.AddressType.StaticMemVarIndex : ProtoCore.DSASM.AddressType.MemVarIndex;
                        }
                        else
                        {
                            operandType = ProtoCore.DSASM.AddressType.VarIndex;
                        }
                    }
                    depth = depth + 1;
                    finalType = lefttype;
                }
                return true;
            }
            else if (node is ProtoCore.AST.ImperativeAST.FunctionCallNode || node is ProtoCore.AST.AssociativeAST.FunctionCallNode)
            {
                TraverseFunctionCall(node, pNode, lefttype.UID, depth, ref finalType, graphNode, subPass);
                //finalType.UID = isBooleanOp ? (int)PrimitiveType.kTypeBool : finalType.UID;
                lefttype = finalType;
                depth = 1;
            }
            //else
            //{
            //    throw new BuildHaltException();
            //}
            return false;
        }


        protected int DfsEmitArrayIndexHeap(Node node)
        {
            int indexCnt = 0;
            Debug.Assert(node is ProtoCore.AST.AssociativeAST.ArrayNode || node is ProtoCore.AST.ImperativeAST.ArrayNode);

            dynamic arrayNode = node;
            while (arrayNode is ProtoCore.AST.AssociativeAST.ArrayNode || arrayNode is ProtoCore.AST.ImperativeAST.ArrayNode)
            {
                ++indexCnt;
                dynamic array = arrayNode;
                ProtoCore.Type lastType = new ProtoCore.Type();
                lastType.UID = (int)PrimitiveType.kTypeVoid;
                lastType.IsIndexable = false;
                DfsTraverse(array.Expr, ref lastType);
                arrayNode = array.Type;
            }
            return indexCnt;
        }

#if USE_PREVIOUS_FUNCCALL_TRAVERSAL 
        protected bool VerifyAllocation(string name, int classScope, out ProtoCore.DSASM.SymbolNode node)
        {
            // Identifier scope resolution
            //  1. Current block
            //  2. Outer language blocks
            //  3. Class scope (TODO Jun: Implement checking the class scope)
            //  4. Global scope (Comment Jun: Is there really a global scope? Conceptually, the outer most block is considered global)
            //node = core.GetFirstVisibleSymbol(name, classScope, functionindex, codeBlock);
            node = core.GetFirstVisibleSymbol(name, classScope, procIndex, codeBlock);
            if (null != node)
            {
                return true;
            }
            return false;
        }
#else
        protected bool VerifyAllocation(string name, int classscope, out ProtoCore.DSASM.SymbolNode node, out bool isAccessible)
        {
            // Identifier scope resolution
            //  1. Current block
            //  2. Outer language blocks
            //  3. Class scope (TODO Jun: Implement checking the class scope)
            //  4. Global scope (Comment Jun: Is there really a global scope? Conceptually, the outer most block is considered global)

            isAccessible = false;


            node = compileState.GetFirstVisibleSymbol(name, globalClassIndex, globalProcIndex, codeBlock);
            if (null != node)
            {
                isAccessible = true;
                return true;
            }

            node = null;
            // TODO Jun: Is this the correct check? checking for null type first?
            if (((int)ProtoCore.PrimitiveType.kTypeVoid == classscope) ||
                (ProtoCore.DSASM.Constants.kInvalidIndex == classscope) ||
                (compileState.ClassTable.ClassNodes[classscope].symbols == null))
            {
                return false;
            }


            bool hasThisSymbol;
            ProtoCore.DSASM.AddressType addressType;
            int symbol;

            symbol = compileState.ClassTable.ClassNodes[classscope].GetSymbolIndex(name, globalClassIndex, globalProcIndex, codeBlock.codeBlockId, compileState, out hasThisSymbol, out addressType);

            if (ProtoCore.DSASM.Constants.kInvalidIndex != symbol)
            {
                // It is static member, then get node from code block
                if (addressType == ProtoCore.DSASM.AddressType.StaticMemVarIndex)
                {
                    node = compileState.CodeBlockList[0].symbolTable.symbolList[symbol];
                }
                else
                {
                    node = compileState.ClassTable.ClassNodes[classscope].symbols.symbolList[symbol];
                }
                isAccessible = true;
                return true;
            }

            return hasThisSymbol;
        }
#endif

        public abstract void TraverseFunctionCall(Node node, Node parentNode, int lefttype, int depth, ref ProtoCore.Type inferedType, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone);

        protected void EmitIntNode(Node node, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone)
        {
            if (subPass == ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier)
            {
                return;
            }

            dynamic iNode = node;
            if (!enforceTypeCheck || compileState.TypeSystem.IsHigherRank((int)PrimitiveType.kTypeInt, inferedType.UID))
            {
                inferedType.UID = (int)PrimitiveType.kTypeInt;
            }
            inferedType.UID = isBooleanOp ? (int)PrimitiveType.kTypeBool : inferedType.UID;
        }

        protected void EmitCharNode(Node node, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone)
        {
            if (subPass == ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier)
            {
                return;
            }

            dynamic cNode = node;
            if (!enforceTypeCheck || compileState.TypeSystem.IsHigherRank((int)PrimitiveType.kTypeChar, inferedType.UID))
            {
                inferedType.UID = (int)PrimitiveType.kTypeChar;
            }
            inferedType.UID = isBooleanOp ? (int)PrimitiveType.kTypeBool : inferedType.UID;
            throw new NotImplementedException();
        }

        protected void EmitDoubleNode(Node node, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone)
        {
            if (subPass == ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier)
            {
                return;
            }

            dynamic dNode = node;
            if (!enforceTypeCheck || compileState.TypeSystem.IsHigherRank((int)PrimitiveType.kTypeDouble, inferedType.UID))
            {
                inferedType.UID = (int)PrimitiveType.kTypeDouble;
            }
            inferedType.UID = isBooleanOp ? (int)PrimitiveType.kTypeBool : inferedType.UID;
        }

        protected void EmitBooleanNode(Node node, ref ProtoCore.Type inferedType, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone)
        {
            if (subPass == ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier)
            {
                return;
            }

            dynamic bNode = node;
            // We need to get inferedType for boolean variable so that we can perform type check
            if (enforceTypeCheck || compileState.TypeSystem.IsHigherRank((int)PrimitiveType.kTypeBool, inferedType.UID))
            {
                inferedType.UID = (int)PrimitiveType.kTypeBool;
            }
        }

        protected void EmitStringNode(Node node, ref ProtoCore.Type inferedType, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone)
        {
            if (subPass == ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier)
            {
                return;
            }

            dynamic sNode = node;
            if (!enforceTypeCheck || compileState.TypeSystem.IsHigherRank((int)PrimitiveType.kTypeString, inferedType.UID))
            {
                inferedType.UID = (int)PrimitiveType.kTypeString;
            }
            throw new NotImplementedException();
        }

        protected void EmitNullNode(Node node, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone)
        {
            if (subPass == ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier)
            {
                return;
            }

            dynamic nullNode = node;
            inferedType.UID = (int)PrimitiveType.kTypeNull;

            inferedType.UID = isBooleanOp ? (int)PrimitiveType.kTypeBool : inferedType.UID;
        }

        protected void EmitReturnNode(Node node)
        {
            throw new NotImplementedException();
        }

        protected void EmitExprListNode(Node node, ref ProtoCore.Type inferedType, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone)
        {
            int firstType = (int)PrimitiveType.kTypeVoid;
            dynamic exprlist = node;

            int rank = 0;
            if (subPass != ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier)
            {
                //get the rank
                dynamic ltNode = exprlist;
                while ((ltNode is ProtoCore.AST.ImperativeAST.ExprListNode || ltNode is ProtoCore.AST.AssociativeAST.ExprListNode) && ltNode.list.Count > 0)
                {
                    rank++;
                    ltNode = ltNode.list[0];
                }
            }

            foreach (Node listNode in exprlist.list)
            {
                // The infered type is the type of the first element
                DfsTraverse(listNode, ref inferedType, false, graphNode, subPass);

                if ((listNode is ProtoCore.AST.AssociativeAST.ExprListNode) == false)
                {
                    // If 'listNode' was another sub-array, then don't bother adding 
                    // it here, as it has added itself in the call to 'DfsTraverse'.
                    if (null != this.arrayElementType)
                        this.arrayElementType.AppendChildType(inferedType);
                }

                if ((int)PrimitiveType.kTypeVoid == firstType)
                {
                    firstType = inferedType.UID;
                }
            }

            inferedType.UID = firstType;
            inferedType.IsIndexable = true;
            inferedType.rank = rank;
        }

        protected void EmitReturnStatement(Node node, ProtoCore.Type inferedType)
        {
            // Check the returned type against the declared return type
            if (null != localProcedure && compileState.IsFunctionCodeBlock(codeBlock))
            {
                if (localProcedure.isConstructor)
                {
                    Debug.Assert(ProtoCore.DSASM.Constants.kInvalidIndex != inferedType.UID);

                    ProtoCore.DSASM.ClassNode typeNode = compileState.ClassTable.ClassNodes[inferedType.UID];
                    Debug.Assert(null != typeNode);
                }
            }
        }

        protected void EmitIdentifierListNode(Node node, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone)
        {
            if (subPass == ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier)
            {
                //to process all unbounded parameters if any
                dynamic iNode = node;
                while (iNode is ProtoCore.AST.AssociativeAST.IdentifierListNode || iNode is ProtoCore.AST.ImperativeAST.IdentifierListNode)
                {
                    dynamic rightNode = iNode.RightNode;
                    if (rightNode is ProtoCore.AST.AssociativeAST.FunctionCallNode || rightNode is ProtoCore.AST.ImperativeAST.FunctionCallNode)
                    {
                        foreach (dynamic paramNode in rightNode.FormalArguments)
                        {
                            ProtoCore.Type paramType = new ProtoCore.Type();
                            paramType.UID = (int)ProtoCore.PrimitiveType.kTypeVoid;
                            paramType.IsIndexable = false;
                            DfsTraverse(paramNode, ref paramType, false, graphNode, ProtoCore.DSASM.AssociativeSubCompilePass.kUnboundIdentifier);
                        }
                    }
                    iNode = iNode.LeftNode;
                }
                return;
            }

            int depth = 0;

            ProtoCore.Type leftType = new ProtoCore.Type();
            leftType.UID = (int)PrimitiveType.kInvalidType;
            leftType.IsIndexable = false;
            bool isFirstIdent = true;

            bool isIdentReference = DfsEmitIdentList(node, null, globalClassIndex, ref leftType, ref depth, ref inferedType, false, ref isFirstIdent, graphNode, subPass);
            inferedType.UID = isBooleanOp ? (int)PrimitiveType.kTypeBool : inferedType.UID;
        }

        protected abstract void DfsTraverse(Node node, ref ProtoCore.Type inferedType, bool isBooleanOp = false, ProtoCore.AssociativeGraph.GraphNode graphNode = null, ProtoCore.DSASM.AssociativeSubCompilePass subPass = ProtoCore.DSASM.AssociativeSubCompilePass.kNone);
    }
}
