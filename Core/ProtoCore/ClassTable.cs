using System.Collections.Generic;
using System.Diagnostics;
using ProtoCore.Utils;

namespace ProtoCore.DSASM
{
    [System.Diagnostics.DebuggerDisplay("{name}, classId = {classId}")]
    public class ClassNode
    {
        public string name { get; set; }
        public SymbolTable symbols { get; set; }
        public List<AST.AssociativeAST.BinaryExpressionNode> defaultArgExprList { get; set; } 
        public ProcedureTable vtable { get; set; }
        public int size { get; set; }
        public int rank { get; set; }
        public int classId { get; set; }
        public List<int> baseList { get; set; }
        public bool IsImportedClass { get; set; }

        /// <summary>
        /// String description of where the classnode was loaded from 
        /// The implementation of the class is stored from here
        /// </summary>
        public string ExternLib { get; set; }

        public TypeSystem typeSystem { get; set; }
        public List<AttributeEntry> Attributes { get; set; }
        // A map of allowed coercions and their respective scores
        public Dictionary<int, int> coerceTypes { get; set; }

        private ProcedureNode disposeMethod;
        private bool hasCachedDisposeMethod;

        public ClassNode()
        {
            IsImportedClass = false;
            name = null;
            size = 0;
            hasCachedDisposeMethod = false;
            disposeMethod = null;
            rank = ProtoCore.DSASM.Constants.kDefaultClassRank;
            symbols = new SymbolTable("classscope", 0);
            defaultArgExprList = new List<AST.AssociativeAST.BinaryExpressionNode>();
            classId = (int)PrimitiveType.kInvalidType;

            // Jun TODO: how significant is runtime index for class procedures?
            int classRuntimProc = ProtoCore.DSASM.Constants.kInvalidIndex;
            vtable = new ProcedureTable(classRuntimProc);
            baseList = new List<int>();
            ExternLib = string.Empty;

            // Set default allowed coerce types
            coerceTypes = new Dictionary<int, int>();
            coerceTypes.Add((int)ProtoCore.PrimitiveType.kTypeVar, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            coerceTypes.Add((int)ProtoCore.PrimitiveType.kTypeArray, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            coerceTypes.Add((int)ProtoCore.PrimitiveType.kTypeNull, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
        }

        public bool ConvertibleTo(int type)
        {
            Debug.Assert(null != coerceTypes);
            Debug.Assert((int)PrimitiveType.kInvalidType != classId);

            if ((int)PrimitiveType.kTypeNull == classId || coerceTypes.ContainsKey(type))
            { 
                return true;
            }

            //chars are convertible to string

            else if (classId == (int)PrimitiveType.kTypeChar && type==(int)PrimitiveType.kTypeString)
            {
                return true;
            }

            //user defined type to bool
            else if (classId >=(int)PrimitiveType.kMaxPrimitives && type == (int)PrimitiveType.kTypeBool)
            {
                return true;
            }
                
                //string to boolean

            else if (classId == (int)PrimitiveType.kTypeString && type == (int)PrimitiveType.kTypeBool)
            {
                return true;
            }
            //char to boolean
            else if (classId == (int)PrimitiveType.kTypeChar && type == (int)PrimitiveType.kTypeBool)
            {
                return true;
            }
            
            return false;
        }

        public int GetCoercionScore(int type)
        {
            Debug.Assert(null != coerceTypes);
            int score = (int)ProtoCore.DSASM.ProcedureDistance.kNotMatchScore;

            if (type == classId)
                return (int)ProtoCore.DSASM.ProcedureDistance.kExactMatchScore;

            if ((int)PrimitiveType.kTypeNull == classId)
            {
                score = (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore;
            }
            else
            {
                if (coerceTypes.ContainsKey(type))
                {
                    score = coerceTypes[type];
                }
            }


            return score;
        }

        public bool IsMyBase(int type)
        {
            if ((int)PrimitiveType.kInvalidType == type)
                return false;

            foreach (int baseIndex in baseList)
            {
                Debug.Assert(baseIndex != (int)PrimitiveType.kInvalidType);
                if (type == baseIndex)
                    return true;

                ClassNode baseClassNode = typeSystem.classTable.ClassNodes[baseIndex];
                if (baseClassNode.IsMyBase(type))
                    return true;
            }

            return false;
        }

        // classScope is a global context, it tells we are in which class's scope
        // functionScope is telling us which function we are in. 
        // 
        // 1. Try to find if the target is a member function's local variable
        //        classScope != kInvalidIndex && functionScope != kInvalidIndex;
        // 
        // 2. Try to find if the target is a member variable
        //     2.1 In a member functions classScope != kInvalidIndex && functionScope != kInvalidIndex.
        //         Returns member in derived class, or non-private member in base classes
        // 
        //     2.2 In a global functions classScope == kInvalidIndex && functionScope != kInvalidIndex.
        //         Returns public member in derived class, or public member in base classes
        // 
        //     2.3 Otherwise, classScope == kInvalidIndex && functionScope == kInvalidIndex
        //         Return public member in derived class, or public member in base classes 
        public int GetSymbolIndex(string name, int classScope, int functionScope, int blockId, ProtoLanguage.CompileStateTracker compileState, out bool hasThisSymbol, out ProtoCore.DSASM.AddressType addressType)
        {
            hasThisSymbol = false;
            addressType = ProtoCore.DSASM.AddressType.Invalid;

            if (symbols == null)
            {
                return ProtoCore.DSASM.Constants.kInvalidIndex;
            }

            HashSet<SymbolNode> allSymbols = symbols.GetNodeForName(name);
            if (allSymbols == null)
            {                
                return ProtoCore.DSASM.Constants.kInvalidIndex;
            }

            int myself = typeSystem.classTable.IndexOf(this.name);
            bool isInMemberFunctionContext = (classScope == myself) && (functionScope != ProtoCore.DSASM.Constants.kInvalidIndex);
            bool isInStaticFunction = isInMemberFunctionContext &&  (vtable.procList[functionScope].isStatic);

            // Try for member function variables
            var blocks = compileState.GetAncestorBlockIdsOfBlock(blockId);
            blocks.Insert(0, blockId);

            Dictionary<int, SymbolNode> symbolOfBlockScope = new Dictionary<int, SymbolNode>();
            foreach (var memvar in allSymbols)
            {
                if ((isInMemberFunctionContext) && (memvar.functionIndex == functionScope))
                {
                    symbolOfBlockScope[memvar.codeBlockId] = memvar;
                }
            }
            if (symbolOfBlockScope.Count > 0)
            {
                foreach (var blockid in blocks)
                {
                    if (symbolOfBlockScope.ContainsKey(blockid))
                    {
                        hasThisSymbol = true;
                        addressType = AddressType.VarIndex;
                        return symbolOfBlockScope[blockid].symbolTableIndex;
                    }
                }
            }

            // Try for member variables. 
            var candidates = new List<SymbolNode>();
            foreach (var memvar in allSymbols)
            {
                if (memvar.functionIndex == ProtoCore.DSASM.Constants.kGlobalScope)
                {
                    candidates.Add(memvar);
                }
            }
            // Sort candidates descending based on their class scopes so that
            // we can search member variable in reverse order of hierarchy tree.
            candidates.Sort((lhs, rhs) => rhs.classScope.CompareTo(lhs.classScope));
            hasThisSymbol = candidates.Count > 0;

            foreach (var symbol in candidates)
            {
                bool isAccessible = false;
                if (isInMemberFunctionContext)
                {
                    isAccessible = (symbol.classScope == myself) || (symbol.access != AccessSpecifier.kPrivate);
                    if (isInStaticFunction)
                        isAccessible = isAccessible && symbol.isStatic;
                }
                else
                {
                    isAccessible = symbol.access == AccessSpecifier.kPublic;
                }

                if (isAccessible)
                {
                    addressType = symbol.isStatic ? AddressType.StaticMemVarIndex : AddressType.MemVarIndex;
                    return symbol.symbolTableIndex;
                }
            }

            return ProtoCore.DSASM.Constants.kInvalidIndex;
        }

        public int GetSymbolIndex(string name, int classScope, int functionScope, int blockId, Core core, out bool hasThisSymbol, out ProtoCore.DSASM.AddressType addressType)
        {
            hasThisSymbol = false;
            addressType = ProtoCore.DSASM.AddressType.Invalid;

            if (symbols == null)
            {
                return ProtoCore.DSASM.Constants.kInvalidIndex;
            }

            HashSet<SymbolNode> allSymbols = symbols.GetNodeForName(name);
            if (allSymbols == null)
            {
                return ProtoCore.DSASM.Constants.kInvalidIndex;
            }

            int myself = typeSystem.classTable.IndexOf(this.name);
            bool isInMemberFunctionContext = (classScope == myself) && (functionScope != ProtoCore.DSASM.Constants.kInvalidIndex);
            bool isInStaticFunction = isInMemberFunctionContext && (vtable.procList[functionScope].isStatic);

            // Try for member function variables
            var blocks = core.GetAncestorBlockIdsOfBlock(blockId);
            blocks.Insert(0, blockId);

            Dictionary<int, SymbolNode> symbolOfBlockScope = new Dictionary<int, SymbolNode>();
            foreach (var memvar in allSymbols)
            {
                if ((isInMemberFunctionContext) && (memvar.functionIndex == functionScope))
                {
                    symbolOfBlockScope[memvar.codeBlockId] = memvar;
                }
            }
            if (symbolOfBlockScope.Count > 0)
            {
                foreach (var blockid in blocks)
                {
                    if (symbolOfBlockScope.ContainsKey(blockid))
                    {
                        hasThisSymbol = true;
                        addressType = AddressType.VarIndex;
                        return symbolOfBlockScope[blockid].symbolTableIndex;
                    }
                }
            }

            // Try for member variables. 
            var candidates = new List<SymbolNode>();
            foreach (var memvar in allSymbols)
            {
                if (memvar.functionIndex == ProtoCore.DSASM.Constants.kGlobalScope)
                {
                    candidates.Add(memvar);
                }
            }
            // Sort candidates descending based on their class scopes so that
            // we can search member variable in reverse order of hierarchy tree.
            candidates.Sort((lhs, rhs) => rhs.classScope.CompareTo(lhs.classScope));
            hasThisSymbol = candidates.Count > 0;

            foreach (var symbol in candidates)
            {
                bool isAccessible = false;
                if (isInMemberFunctionContext)
                {
                    isAccessible = (symbol.classScope == myself) || (symbol.access != AccessSpecifier.kPrivate);
                    if (isInStaticFunction)
                        isAccessible = isAccessible && symbol.isStatic;
                }
                else
                {
                    isAccessible = symbol.access == AccessSpecifier.kPublic;
                }

                if (isAccessible)
                {
                    addressType = symbol.isStatic ? AddressType.StaticMemVarIndex : AddressType.MemVarIndex;
                    return symbol.symbolTableIndex;
                }
            }

            return ProtoCore.DSASM.Constants.kInvalidIndex;
        }


        public int GetFirstVisibleSymbolNoAccessCheck(string name)
        {
            HashSet<SymbolNode> allSymbols = symbols.GetNodeForName(name);
            if (allSymbols == null)
            {
                return ProtoCore.DSASM.Constants.kInvalidIndex;
            }

            // Try for member variables. 
            foreach (var memvar in allSymbols)
            {
                if (memvar.functionIndex == ProtoCore.DSASM.Constants.kGlobalScope)
                {
                    return memvar.symbolTableIndex;
                }
            }
            return ProtoCore.DSASM.Constants.kInvalidIndex;
        }

        // 1. In some class's scope, classScope != kInvalidIndex;
        //    1.1 In the same class scope, return member function directly
        //    1.2 In the derive class scope, return member fucntion != kPrivate
        //    1.3 Return member function whose access == kPublic
        //
        // 2. In global scope, classScope == kInvalidIndex;
        public ProtoCore.DSASM.ProcedureNode GetMemberFunction(string procName, List<ProtoCore.Type> argTypeList, int classScope, out bool isAccessible, out int functionHostClassIndex, bool isStaticOrConstructor = false)
        {
            isAccessible = false;
            functionHostClassIndex = ProtoCore.DSASM.Constants.kInvalidIndex;

            if (vtable == null)
                return null;

            ProtoCore.DSASM.ProcedureNode procNode = null;

            int functionIndex = vtable.IndexOf(procName, argTypeList, typeSystem.classTable, isStaticOrConstructor);
            if (functionIndex != ProtoCore.DSASM.Constants.kInvalidIndex)
            {
                int myClassIndex = typeSystem.classTable.IndexOf(name);
                functionHostClassIndex = myClassIndex;
                procNode = vtable.procList[functionIndex];

                if (classScope == ProtoCore.DSASM.Constants.kInvalidIndex)
                {
                    isAccessible = (procNode.access == AccessSpecifier.kPublic);
                }
                else if (classScope == myClassIndex) 
                {
                    isAccessible = true;
                }
                else if (typeSystem.classTable.ClassNodes[classScope].IsMyBase(myClassIndex))
                {
                    isAccessible = (procNode.access != AccessSpecifier.kPrivate);
                }
                else
                {
                    isAccessible = (procNode.access == AccessSpecifier.kPublic);
                }

                return procNode;
            }

            foreach (int baseClassIndex in baseList)
            {
                procNode = typeSystem.classTable.ClassNodes[baseClassIndex].GetMemberFunction(procName, argTypeList, classScope, out isAccessible, out functionHostClassIndex, isStaticOrConstructor);
                if (procNode != null && isAccessible)
                    break;
            }

            return procNode;
        }

        public ProtoCore.DSASM.ProcedureNode GetFirstMemberFunction(string procName)
        {
            ProtoCore.DSASM.ProcedureNode procNode = null;
            if (null != vtable)
            {
                procNode = vtable.GetFirst(procName);
                if (null == procNode)
                {
                    foreach (int baseClassIndex in baseList)
                    {
                        procNode = typeSystem.classTable.ClassNodes[baseClassIndex].GetFirstMemberFunction(procName);
                        if (null != procNode)
                        {
                            break;
                        }
                    }
                }
            }
            return procNode;
        }

        public ProtoCore.DSASM.ProcedureNode GetFirstMemberFunction(string procName, int argCount)
        {
            ProtoCore.DSASM.ProcedureNode procNode = null;
            if (null != vtable)
            {
                procNode = vtable.GetFirst(procName, argCount);
                if (null == procNode)
                {
                    foreach (int baseClassIndex in baseList)
                    {
                        procNode = typeSystem.classTable.ClassNodes[baseClassIndex].GetFirstMemberFunction(procName, argCount);
                        if (null != procNode)
                        {
                            break;
                        }
                    }
                }
            }
            return procNode;
        }

        public ProtoCore.DSASM.ProcedureNode GetFirstStaticMemberFunction(string procName)
        {
            ProtoCore.DSASM.ProcedureNode procNode = null;
            if (null != vtable)
            {
                procNode = vtable.GetFirstStatic(procName);
                if (null == procNode)
                {
                    foreach (int baseClassIndex in baseList)
                    {
                        procNode = typeSystem.classTable.ClassNodes[baseClassIndex].GetFirstMemberFunction(procName);
                        if (null != procNode)
                        {
                            break;
                        }
                    }
                }
            }
            return procNode;
        }

        private ProcedureNode GetProcNode(string variableName)
        {
            Validity.Assert(null != variableName && variableName.Length > 0);
            string getterName = ProtoCore.DSASM.Constants.kGetterPrefix + variableName;
            int index = vtable.IndexOfFirst(getterName);
            if (ProtoCore.DSASM.Constants.kInvalidIndex == index)
            {
                return null;
            }
            return vtable.procList[index];
        }

        public bool IsStaticMemberVariable(string variableName)
        {
            Validity.Assert(null != variableName && variableName.Length > 0);
            ProcedureNode proc = GetProcNode(variableName);
            Validity.Assert(null != proc);
            return proc.isStatic;
        }

        public bool IsMemberVariable(string variableName)
        {
            // Jun:
            // To find a member variable, get its getter name and check the vtable
            Validity.Assert(null != variableName && variableName.Length > 0);
            string getterName = ProtoCore.DSASM.Constants.kGetterPrefix + variableName;
            return ProtoCore.DSASM.Constants.kInvalidIndex != vtable.IndexOfFirst(getterName) ? true : false;
        }

        public bool IsMemberVariable(SymbolNode symbol)
        {
            // Jun:
            // A fast version of the find member variable where we search the symboltable directly
            Validity.Assert(null != symbol);
            return ProtoCore.DSASM.Constants.kInvalidIndex != symbols.IndexOf(symbol.name)

                // A symbol is a member if it doesnt belong to any function
                && ProtoCore.DSASM.Constants.kInvalidIndex == symbol.functionIndex;
        }

        public ProcedureNode GetDisposeMethod()
        {
            if (!hasCachedDisposeMethod)
            {
                hasCachedDisposeMethod = true;
                if (vtable == null)
                {
                    disposeMethod = null;
                }
                else
                {
                    foreach (ProcedureNode procNode in vtable.procList)
                    {
                        if (procNode.name == ProtoCore.DSDefinitions.Kw.kw_Dispose && procNode.argInfoList.Count == 0)
                        {
                            disposeMethod = procNode;
                            break;
                        }
                    }
                }
            }
            return disposeMethod;
        }
    }



    public class ClassTable
    {
        // Don't directly modify class table list.
        public IList<ClassNode> ClassNodes 
        {
            get
            {
                return classNodes.AsReadOnly();
            }
        }

        private List<ClassNode> classNodes = new List<ClassNode>();

        // It is a performance bottleneck for finding index of a class when
        // importing a HUGE extension dll. The other alternative is using 
        // Trie. - Yu Ke
        private Dictionary<string, int> classIndexMap = new Dictionary<string, int>();

        public ClassTable()
        {
            classIndexMap[ProtoCore.DSDefinitions.Kw.kw_invalid] = ProtoCore.DSASM.Constants.kInvalidIndex;
        }

        public void Reserve(int size)
        {
            for (int n = 0; n < size; ++n)
            {
                ProtoCore.DSASM.ClassNode cnode = new ProtoCore.DSASM.ClassNode { name = ProtoCore.DSDefinitions.Kw.kw_invalid, size = 0, rank = 0, symbols = null, vtable = null };
                classNodes.Add(cnode);
            }
        }

        public int Append(ClassNode node)
        {
            if (IndexOf(node.name) != ProtoCore.DSASM.Constants.kInvalidIndex)
            {
                return ProtoCore.DSASM.Constants.kInvalidIndex;
            }

            classNodes.Add(node);
            node.classId = classNodes.Count - 1;
            classIndexMap[node.name] = node.classId;
            return node.classId;
        }

        public void SetClassNodeAt(ClassNode node, int index)
        {
            classNodes[index] = node;
            classIndexMap[node.name] = index;
        }

        public int IndexOf(string name)
        {
            Debug.Assert(null != name);

            int index = ProtoCore.DSASM.Constants.kInvalidIndex;
            if (!classIndexMap.TryGetValue(name, out index))
            {
                return ProtoCore.DSASM.Constants.kInvalidIndex;
            }
            else
            {
                return index;
            }
        }

        public bool DoesExist(string name)
        {
            Validity.Assert(null != name);
            return ProtoCore.DSASM.Constants.kInvalidIndex != IndexOf(name);
        }

        public string GetTypeName(int UID)
        {
            if (UID == (int)PrimitiveType.kInvalidType ||
                UID > ClassNodes.Count)
            {
                return null;
            }
            else
            {
                return ClassNodes[UID].name; 
            }
        }
    }
}
