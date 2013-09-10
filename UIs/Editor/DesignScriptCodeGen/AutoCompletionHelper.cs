using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoCore.CodeModel;
using System.Text.RegularExpressions;

namespace DesignScript.Editor.CodeGen
{
    // @TODO(Ben): Rename this to "ScopeIdentifier".
    struct ScopeInfo
    {
        public override int GetHashCode()
        {
            return (BlockId.GetHashCode()) ^
                   (ClassScope.GetHashCode()) ^
                   (FunctionIndex.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            ScopeInfo rhs = ((ScopeInfo)obj);
            return ((this.BlockId == rhs.BlockId) &&
                (this.ClassScope == rhs.ClassScope) &&
                (this.FunctionIndex == rhs.FunctionIndex));
        }

        public static bool operator ==(ScopeInfo lhs, ScopeInfo rhs)
        {
            return ((lhs.BlockId == rhs.BlockId) &&
                    (lhs.ClassScope == rhs.ClassScope) &&
                    (lhs.FunctionIndex == rhs.FunctionIndex));
        }

        public static bool operator !=(ScopeInfo lhs, ScopeInfo rhs)
        {
            return !(lhs == rhs);
        }

        public int BlockId { get; set; }
        public int ClassScope { get; set; }
        public int FunctionIndex { get; set; }
    }

    public class AutoCompletionHelper
    {
        #region APIs

        /// <summary>
        /// Bitwise representation of the member types of a class
        /// </summary>
        public enum MemberType
        {
            Invalid = 0x0,
            Static = 0x1,
            Public = 0x2,
            Private = 0x4,
            Protected = 0x8,
            FromBase = 0x10,
            Methods = 0x20,
            Constructor = 0x40,
            Fields = 0x80,
        }

        /// <summary>
        /// This EqualityComparer is used to remove the duplicated functions in auto completion list
        /// the duplication maily comes from overridden methods or overloaded methods 
        /// </summary>
        public class DuplicateMethodNameRemover : EqualityComparer<KeyValuePair<MemberType, string>>
        {
            /// <summary>
            /// Return true if the two members are function, have the same name, both are constructor or both are not, both are static or both are not
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public override bool Equals(KeyValuePair<MemberType, string> x, KeyValuePair<MemberType, string> y)
            {
                if (x.Value == y.Value && // They have the same name
                    (x.Key & y.Key & MemberType.Methods) != MemberType.Invalid && // Both of them are methods
                    (~(x.Key ^ y.Key) & MemberType.Static) != MemberType.Invalid && // Both of them has the same static bit set or reset
                    (~(x.Key ^ y.Key) & MemberType.Constructor) != MemberType.Invalid) // Both of them has the same constructor bit set or reset 
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public override int GetHashCode(KeyValuePair<MemberType, string> obj)
            {
                return obj.Value.Length;
            }
        }

        /// <summary>
        /// This EqualityComparer is used to remove the duplicated functions in function tool tips list
        /// the duplication mainly comes from overriden methos
        /// </summary>
        public class DeplicateMethodSignatureRemover : EqualityComparer<KeyValuePair<int, ProtoCore.DSASM.ProcedureNode>>
        {
            /// <summary>
            /// Return true if the two function has the same name, same parameter count and same parameter type
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            /// <returns></returns>
            public override bool Equals(KeyValuePair<int, ProtoCore.DSASM.ProcedureNode> x, KeyValuePair<int, ProtoCore.DSASM.ProcedureNode> y)
            {
                if (x.Value.name == y.Value.name && // same name
                    x.Value.argTypeList.Count == y.Value.argTypeList.Count) // same number of parameters 
                {
                    for (int ix = 0; ix < x.Value.argTypeList.Count; ++ix)
                    {
                        // different parameter type
                        if (x.Value.argTypeList[ix].UID != y.Value.argTypeList[ix].UID || x.Value.argTypeList[ix].rank != y.Value.argTypeList[ix].rank)
                            return false;
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public override int GetHashCode(KeyValuePair<int, ProtoCore.DSASM.ProcedureNode> obj)
            {
                return 0;
            }
        }

        public class MethodParameter
        {
            public string Name { get; set; }    // eg: foo
            public string Type { get; set; }    // eg: int
            // TODO: Code gen does not populate the string representation of the default arguments 
            public string DefaultArguments { get; set; }    // eg: 10 + 20
            public override string ToString()
            {
                if (DefaultArguments == null)
                    return string.Format("{0} : {1}", Name, Type); // eg: a : int
                else
                    return string.Format("{0} : {1} = {2}", Name, Type, DefaultArguments); // eg : a : int = 10 + 20
            }

        }

        public class MethodSignature
        {
            public MethodSignature()
            {
                Parameters = new List<MethodParameter>();
            }
            internal MethodSignature(ProtoCore.DSASM.ProcedureNode pn, string classType = null)
            {
                Parameters = new List<MethodParameter>();
                if (classType == null)
                    MethodName = pn.name;
                else
                    MethodName = string.Format("{0}.{1}", classType, pn.name);
                isExternal = pn.isExternal;
                isConstructor = pn.isConstructor;
                ReturnType = GetDSTypeName(pn.returntype);
                for (int ix = 0; ix < pn.argTypeList.Count; ++ix)
                {
                    MethodParameter mp = new MethodParameter();
                    mp.Type = GetDSTypeName(pn.argTypeList[ix]);
                    // TODO(Jiong): Get the Parameter name and default argument info
                    mp.Name = pn.argInfoList[ix].Name;
                    if (string.IsNullOrEmpty(mp.Name))
                        mp.Name = string.Format("param{0}", ix);
                    Parameters.Add(mp);
                }
            }
            public List<MethodParameter> Parameters { get; set; }
            public string MethodName { get; set; }
            public string ReturnType { get; set; }
            public bool isExternal { get; set; }
            public bool isConstructor { get; set; }
            public string ExternalLibrary { get; set; }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                if (isExternal)
                    sb.AppendFormat("external (\"{0}\") ", ExternalLibrary);
                if (isConstructor)
                    sb.AppendFormat("constructor {0} (", MethodName);
                else
                    sb.AppendFormat("def {0} : {1}(", MethodName, ReturnType);
                for (int ix = 0; ix < Parameters.Count; ++ix)
                {
                    sb.Append(Parameters[ix].ToString());
                    if (ix != Parameters.Count - 1)
                        sb.Append(", ");
                }
                sb.Append(")");
                return sb.ToString();
            }

            public void FormatSegments(int argumentIndex, List<string> segments)
            {
                if (null == segments)
                    return;

                StringBuilder builder = new StringBuilder();

                if (isExternal)
                    builder.AppendFormat("external (\"{0}\") ", ExternalLibrary);
                if (isConstructor)
                    builder.AppendFormat("constructor {0} (", MethodName);
                else
                    builder.AppendFormat("def {0} : {1}(", MethodName, ReturnType);

                for (int current = 0; current < Parameters.Count; ++current)
                {
                    if (0 != current)
                        builder.Append(", ");

                    if (current == argumentIndex)
                    {
                        segments.Add(builder.ToString());
                        segments.Add(Parameters[current].ToString());

                        builder.Clear();
                        continue;
                    }

                    builder.Append(Parameters[current].ToString());
                }

                builder.Append(")");
                segments.Add(builder.ToString());
            }
        }

        public static void Reset()
        {
            CoreCodeGen.ResetInternalStates();
            AutoCompletionHelper.compileState = null;
        }

        public static bool Compile(string src, string entryFile)
        {
            ProtoLanguage.CompileOptions ops = new ProtoLanguage.CompileOptions();
            ops.RootModulePathName = entryFile;
            if (null != AutoCompletionHelper.searchPaths)
                ops.IncludeDirectories.AddRange(AutoCompletionHelper.searchPaths);

            compileState = new ProtoLanguage.CompileStateTracker(ops);

            compileState.CurrentDSFileName = entryFile;

            CoreCodeGen.ResetInternalStates();
            ProtoFFI.DLLFFIHandler.Register(ProtoFFI.FFILanguage.CSharp, new ProtoFFI.CSModuleHelper());

            // Register a message stream if we do have one.
            if (null != AutoCompletionHelper.MessageHandler)
                compileState.BuildStatus.MessageHandler = MessageHandler;

            MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(src));
            ProtoCore.DesignScriptParser.Scanner s = new ProtoCore.DesignScriptParser.Scanner(ms);
            ProtoCore.DesignScriptParser.Parser p = new ProtoCore.DesignScriptParser.Parser(s, compileState);

            try
            {
                p.Parse();

                CoreCodeGen.arrayTypeTable = new ArrayTypeTable();
                AssociativeCodeGen codegen = new AssociativeCodeGen(compileState);
                codegen.Emit(p.root as ProtoCore.AST.AssociativeAST.CodeBlockNode);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception Caught in CodeGen");
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return false;
            }

            return true;
        }

        public static void SetSearchPaths(List<string> searchPaths)
        {
            if (null == AutoCompletionHelper.searchPaths)
                AutoCompletionHelper.searchPaths = new List<string>();

            AutoCompletionHelper.searchPaths.Clear();
            AutoCompletionHelper.searchPaths.AddRange(searchPaths);
        }

        /// <summary>
        /// API method for autocompletion 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="col"></param>
        /// <param name="src">source code</param>
        /// <param name="identlist">a.b.c.e.f</param>
        /// <param name="file">can be null the the file has not been saved</param>
        /// <returns>a list of class members</returns>
        public static List<KeyValuePair<MemberType, string>> GetList(int line, int col, string src, string identlist, string file)
        {
            List<KeyValuePair<MemberType, string>> members = new List<KeyValuePair<MemberType, string>>();

            // This whole method will be made obsolete.
            if (null != CoreCodeGen.AutoCompleteEngine)
                return members;

            // We need to compile the input source code first.
            AutoCompletionHelper.Reset();
            if (AutoCompletionHelper.Compile(src, file) == false)
                return members;

            ProtoCore.CodeModel.CodePoint cp = new ProtoCore.CodeModel.CodePoint()
            {
                LineNo = line,
                CharNo = col,
                SourceLocation = new ProtoCore.CodeModel.CodeFile()
                {
                    FilePath = file
                }
            };

            // figure out the function index, class index and block id of the typing site
            // based on the code range table
            ScopeInfo si = GetScopeInfo(cp);

            // split the identifier list
            // a.b.c -> { "a", "b", "c" }
            char[] seperator = { '.' };
            string[] idents = identlist.Split(seperator, StringSplitOptions.RemoveEmptyEntries);

            if (idents.Length <= 0)
                return members;

            bool isStatic = false;
            // a.b.c.e -> traverse and get the type of 'e', 
            // isStatic will be true only when the identifier list is "A" and "A" is a class name
            int finalType = TraverseIdentList(idents, idents.Length, si, cp, out isStatic);
            if (finalType == ProtoCore.DSASM.Constants.kInvalidIndex)
                return members;

            // find out all the class members based on finalType
            // Distinct method is used to filter away overloaded methods 
            return GetClassMembers(finalType, si, false, isStatic, isStatic).Distinct(new DuplicateMethodNameRemover()).ToList();
        }

        /// <summary>
        /// This method gathers the list of method signatures (overloaded) given a method name.
        /// </summary>
        public static List<MethodSignature> GetMethodParameterList(int line, int col, string identList, string file)
        {
            // This whole method will be made obsolete.
            if (null != CoreCodeGen.AutoCompleteEngine)
                return new List<MethodSignature>();

            // We need to compile the input source code first.
            System.Diagnostics.Debug.Assert(null != AutoCompletionHelper.compileState);
            if (string.IsNullOrEmpty(identList) || (null == AutoCompletionHelper.compileState))
                return new List<MethodSignature>();

            if (null == CoreCodeGen.CodeRangeTable)
                return new List<MethodSignature>();

            ProtoCore.CodeModel.CodePoint cp = new ProtoCore.CodeModel.CodePoint()
            {
                LineNo = line,
                CharNo = col,
                SourceLocation = new ProtoCore.CodeModel.CodeFile()
                {
                    FilePath = file
                }
            };

            // figure out the function index, class index and block id of the typing site
            // based on the code range table
            ScopeInfo si = GetScopeInfo(cp);

            // split the identifier list
            // a.b.c -> { "a", "b", "c" }
            char[] seperator = { '.' };
            string[] idents = identList.Split(seperator, StringSplitOptions.RemoveEmptyEntries);

            if (idents.Length <= 0)
                return new List<MethodSignature>();

            int finalType = ProtoCore.DSASM.Constants.kInvalidIndex;
            bool isStatic = false;
            // this is slightly different from autocompletion 
            // a.b.c.e.f.foo, foo is a class name, so we should traverse only up to f, not foo, to get the type of "f"
            if (idents.Length >= 2)
            {
                finalType = TraverseIdentList(idents, idents.Length - 1, si, cp, out isStatic);
                if (finalType == ProtoCore.DSASM.Constants.kInvalidIndex)
                    return new List<MethodSignature>();
            }

            List<KeyValuePair<int, ProtoCore.DSASM.ProcedureNode>> methodList = new List<KeyValuePair<int, ProtoCore.DSASM.ProcedureNode>>();

            if (finalType == ProtoCore.DSASM.Constants.kInvalidIndex && si.ClassScope == ProtoCore.DSASM.Constants.kInvalidIndex)
            {
                // the IDE has passed us something like "foo" and we are not inside any classes
                // only global function should be searched
                methodList = GetGlobalMethod(si, idents[0]);
            }
            else if (finalType == ProtoCore.DSASM.Constants.kInvalidIndex && si.ClassScope != ProtoCore.DSASM.Constants.kInvalidIndex)
            {
                // the IDE has passed us something like "foo" and we are inside class with index of finalType
                // search the member function of finalType with a name foo
                methodList = GetMemberFunctions(si.ClassScope, idents[idents.Length - 1], si, isStatic);
            }
            else if (isStatic)
            {
                // the IDE has passed us something like "A.foo", "A" is a class name
                // search the static functions and constructors of finalType with a name "foo"
                methodList = GetStatisMemberFunctionAndConstructor(finalType, idents[idents.Length - 1], si);
            }
            else
            {
                // the IDE has passed us something like "a.b.c.f.foo", 
                // search the normal member functions of type finalType with a name "foo", Note: finalType is the type of "f"
                methodList = GetMemberFunctions(finalType, idents[idents.Length - 1], si, isStatic);
            }

            // Note, the .NET Distinct method will keep the earlier element and delete the later one, 
            // this ensures that if both the derived class and the base class have exactly the same function signature, 
            // the base class one will be removed and the derived one will be kept, which is expected 
            return methodList.Distinct(new DeplicateMethodSignatureRemover()).Select(x => new MethodSignature(x.Value, x.Key == ProtoCore.DSASM.Constants.kInvalidIndex ? null : compileState.ClassTable.ClassNodes[x.Key].name)).ToList();

        }

        #endregion APIs

        #region Compiler Related Code

        public static bool IsHelperReset { get { return (null == AutoCompletionHelper.compileState); } }

        static private ProtoLanguage.CompileStateTracker compileState;

        static private CodeRangeTable codeRangeTable
        {
            get { return CoreCodeGen.CodeRangeTable; }
        }

        static private IdentLocationTable identLocationTable
        {
            get { return CoreCodeGen.IdentLocation; }
        }

        static private ImportTable importTable
        {
            get { return CoreCodeGen.ImportTable; }
        }

        static public ProtoCore.IOutputStream MessageHandler { get; set; }

        #endregion Compiler Related Code

        #region Tiny Utility Methods

        /// <summary>
        /// Convert ProtoCore.DSASM.AccessSpecifier to MemberType.Public, MemberType.Private or MemberType.Protected
        /// </summary>
        /// <param name="access"></param>
        /// <returns></returns>
        private static MemberType AccessToMemberType(ProtoCore.DSASM.AccessSpecifier access)
        {
            if (access == ProtoCore.DSASM.AccessSpecifier.kPublic)
                return MemberType.Public;
            else if (access == ProtoCore.DSASM.AccessSpecifier.kProtected)
                return MemberType.Protected;
            else
                return MemberType.Private;
        }

        private static MemberType StaticToMemberType(bool isStatic)
        {
            return isStatic ? MemberType.Static : MemberType.Invalid;
        }

        private static MemberType ConstructorToMemberType(bool isConstructor)
        {
            return isConstructor ? MemberType.Constructor : MemberType.Invalid;
        }

        private static MemberType BaseToMemberType(bool searchingBase)
        {
            return searchingBase ? MemberType.FromBase : MemberType.Invalid;
        }

        /// <summary>
        /// Reutrn the corresponding access level based on the class index of the searching site 
        /// and the class index you are searching 
        /// </summary>
        /// <param name="fromClassIndex"></param>
        /// <param name="targetClassIndex"></param>
        /// <returns></returns>
        private static ProtoCore.DSASM.AccessSpecifier GetAccessLevel(int fromClassIndex, int targetClassIndex)
        {
            if (fromClassIndex == ProtoCore.DSASM.Constants.kInvalidIndex) // not inside any class, access level is always public
                return ProtoCore.DSASM.AccessSpecifier.kPublic;
            else if (fromClassIndex == targetClassIndex)                    // inside the same class, access level is private
                return ProtoCore.DSASM.AccessSpecifier.kPrivate;
            else if (compileState.ClassTable.ClassNodes[fromClassIndex].baseList.Contains(targetClassIndex))  // inside the derived class, access level is protected
                return ProtoCore.DSASM.AccessSpecifier.kProtected;
            return ProtoCore.DSASM.AccessSpecifier.kPublic;
        }

        private static string GetDSTypeName(ProtoCore.Type t)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(t.Name);

            for (int ix = 0; ix < t.rank; ++ix)
            {
                sb.Append("[]");
            }

            if (t.rank == ProtoCore.DSASM.Constants.kInvalidIndex)
                sb.Append("[]..[]");

            return sb.ToString();
        }

        private static Regex identDelimiters = new Regex(@"([.\[\]])");

        private static List<string> searchPaths = null;

        private static string[] TokenizeArrayIndexers(string variable)
        {
            List<string> results = new List<string>();
            string[] tokens = identDelimiters.Split(variable);

            bool expectingIndex = false;
            foreach (string token in tokens)
            {
                if (string.IsNullOrEmpty(token))
                    continue;

                switch (token)
                {
                    case "[": expectingIndex = true; continue;
                    case "]": expectingIndex = false; continue;

                    default:
                        if (false == expectingIndex)
                            results.Add(token);
                        else
                        {
                            int indexValue = 0;
                            if (int.TryParse(token, out indexValue) == false)
                                indexValue = 0;

                            results.Add(indexValue.ToString());
                        }

                        break;
                }
            }

            return results.ToArray();
        }

        #endregion Tiny Utility Methods

        #region Type Infering Methods

        /// <summary>
        /// This method traverses an identifier list, like a.b.c.d and returns the type of "d"
        /// it also has an out parameter isStatic, this indicates if the type is Static, 
        /// for example: if you are searching for A, A is a class name, then the isStatic will be true
        /// otherwize it will be false
        /// </summary>
        /// <param name="idents">ident list, if you are searching the type of a.b.c, you should pass it { a, b, c }</param>
        /// <param name="si">the scope of the ident list</param>
        /// <param name="cp">the location of the ident list</param>
        /// <param name="isStatic"></param>
        /// <returns></returns>
        private static int TraverseIdentList(string[] idents, int depth, ScopeInfo si, ProtoCore.CodeModel.CodePoint cp, out bool isStatic)
        {
            isStatic = false;
            if (idents.Length == 0 || depth > idents.Length)
                return ProtoCore.DSASM.Constants.kInvalidIndex;

            string firstIdentifier = idents[0];
            int finalType = GetArrayElementType(si, firstIdentifier);

            if (finalType == ProtoCore.DSASM.Constants.kInvalidIndex)
            {
                if (firstIdentifier.IndexOfAny(new char[] { '[', ']' }) != -1)
                {
                    string[] tokens = TokenizeArrayIndexers(firstIdentifier);
                    firstIdentifier = tokens[0];
                }

                // this will get the type of the first identifier in the identifier list (a in  a.b.c.d)
                // it is special because it is plain variables,     ( a is plain variable)
                // and all the followings are class member variabls ( b, c, and d are all class member variables)
                finalType = GetIdentType(si, firstIdentifier, cp);
            }

            if (finalType == ProtoCore.DSASM.Constants.kInvalidIndex)
            {
                // cannot find a variable named "a"
                // check if there is a class named "a:, 
                // if there is return its class index as final type
                // and set isStatic to true
                // we are only interested in its static members and constructors now
                finalType = compileState.ClassTable.IndexOf(idents[0]);
                isStatic = true;
            }
            if (finalType == ProtoCore.DSASM.Constants.kInvalidIndex)
                return ProtoCore.DSASM.Constants.kInvalidIndex;

            // iterate through all the remaining identifiers, (b c and d)
            for (int ix = 1; ix < depth; ++ix)
            {
                ProtoCore.DSASM.SymbolNode sn = GetMemberVariable(finalType, idents[ix], si, isStatic);
                if (sn == null || sn.datatype.UID == ProtoCore.DSASM.Constants.kInvalidIndex)
                    return ProtoCore.DSASM.Constants.kInvalidIndex;
                finalType = GetSymbolType(sn);
                isStatic = false;
            }

            return finalType;
        }

        private static int GetSymbolType(ProtoCore.DSASM.SymbolNode symbolnode)
        {
            ProtoCore.Type staticType = symbolnode.staticType;
            if (staticType.UID == (int)ProtoCore.PrimitiveType.kTypeVar)
            {
                return symbolnode.datatype.UID;
            }
            else
            {
                return staticType.UID;
            }
        }

        /// <summary>
        /// get the type of an identifier based on its location and name
        /// </summary>
        /// <param name="si">where is the ident</param>
        /// <param name="ident">name of the ident</param>
        /// <param name="cp">
        /// where is the ident, not the same usage as parameter "si", 
        /// this is used to see if the ident location is before its definition or after its definition
        /// </param>
        /// <returns></returns>
        private static int GetIdentType(ScopeInfo si, string ident, ProtoCore.CodeModel.CodePoint cp)
        {
            if (si.ClassScope == ProtoCore.DSASM.Constants.kInvalidIndex)
            {
                // Not inside any class
                ProtoCore.DSASM.CodeBlock cb = null;
                List<ProtoCore.DSASM.CodeBlock> codeBlocks = compileState.CodeBlockList;
                if (null != codeBlocks && (si.BlockId >= 0) && (si.BlockId < codeBlocks.Count))
                    cb = codeBlocks[si.BlockId];

                // loop used to search its parent block
                while (cb != null)
                {
                    // search by matching ident name and function index 
                    IEnumerable<ProtoCore.DSASM.SymbolNode> sns = cb.symbolTable.symbolList.Values.Where(x =>
                        x.name == ident &&
                        x.functionIndex == si.FunctionIndex &&
                        x.classScope == ProtoCore.DSASM.Constants.kInvalidIndex);

                    if (sns.Count() > 0) // found something 
                    {
                        ProtoCore.DSASM.SymbolNode symbolNode = sns.ElementAt(0);

                        // if it is argument, it is visible every where inside the fucntion 
                        if (symbolNode.isArgument == true)
                            return GetSymbolType(symbolNode);

                        // check if the variable definition location is before our searching site
                        if (cp.Before(identLocationTable.Table[symbolNode]))
                            return GetSymbolType(symbolNode);

                        // if not argument, check if it is defined some where in the imported files, if yes, it is visible 
                        CodeFile file = identLocationTable.Table[symbolNode].SourceLocation;
                        if (importTable.IsImported(cp.SourceLocation.FilePath, file.FilePath))
                            return GetSymbolType(symbolNode);
                    }

                    if (si.FunctionIndex != ProtoCore.DSASM.Constants.kInvalidIndex)
                    {
                        // no name look up inside a fucntion
                        // TODO: This may need to change, since we are now allowing to access global variables inside a fucntion
                        break;
                    }

                    // update cb to its parent block
                    cb = cb.parent;
                }
            }
            else
            {
                // we are inside a function
                // check if it is this keyword
                if (ident == "this")
                {
                    // the type is the class itself
                    return si.ClassScope;
                }

                // inside a class scope
                ProtoCore.DSASM.CodeBlock cb = compileState.CodeBlockList[si.BlockId];
                ProtoCore.DSASM.SymbolNode sn = null;
                while (cb.parent != null)
                {
                    // if it is inside a language block which is inside a function
                    IEnumerable<ProtoCore.DSASM.SymbolNode> sns = cb.symbolTable.symbolList.Values.Where(x => x.name == ident);
                    if (sns.Count() > 0 &&
                        (sns.ElementAt(0).isArgument == true ||
                        importTable.IsImported(cp.SourceLocation.FilePath, identLocationTable.Table[sns.ElementAt(0)].SourceLocation.FilePath) ||
                        cp.Before(identLocationTable.Table[sns.ElementAt(0)])))
                    {
                        sn = sns.ElementAt(0);
                        break;
                    }

                    cb = cb.parent;
                }

                if (sn == null)
                {
                    // search local variables in the funtion
                    IEnumerable<ProtoCore.DSASM.SymbolNode> sns = compileState.ClassTable.ClassNodes[si.ClassScope].symbols.symbolList.Values.Where(x => x.name == ident && x.functionIndex == si.FunctionIndex);
                    if (sns.Count() > 0)
                    {
                        sn = sns.ElementAt(0);
                    }
                    else
                    {
                        // the final search, search the class member variables 
                        sn = GetMemberVariable(si.ClassScope, ident, si, false);
                        //sn = GetClassMemberVariable(si.ClassScope, ident);
                    }
                }

                if (sn != null)
                {
                    return sn.datatype.UID;
                }
            }

            return ProtoCore.DSASM.Constants.kInvalidIndex;
        }

        private static int GetArrayElementType(ScopeInfo si, string variable)
        {
            if (null == CoreCodeGen.arrayTypeTable)
                return ProtoCore.DSASM.Constants.kInvalidIndex;
            if (variable.IndexOfAny(new char[] { '[', ']' }) == -1)
                return ProtoCore.DSASM.Constants.kInvalidIndex;

            string[] tokens = TokenizeArrayIndexers(variable);
            if (null == tokens || (tokens.Length <= 0))
                return ProtoCore.DSASM.Constants.kInvalidIndex;

            return CoreCodeGen.arrayTypeTable.GetArrayElementType(si, tokens);
        }

        /// <summary>
        /// Figure out the ClassScope, FunctionIndex, BlockId of a given position represented by the three parameters 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="col"></param>
        /// <param name="src">ds file name</param>
        /// <returns></returns>
        private static ScopeInfo GetScopeInfo(ProtoCore.CodeModel.CodePoint cp)
        {
            // initialize scope info to invalid values, -1
            ScopeInfo si = new ScopeInfo()
            {
                BlockId = ProtoCore.DSASM.Constants.kInvalidIndex,
                FunctionIndex = ProtoCore.DSASM.Constants.kInvalidIndex,
                ClassScope = ProtoCore.DSASM.Constants.kInvalidIndex,
            };

            // search if it is inside any class scope
            foreach (KeyValuePair<int, ProtoCore.CodeModel.CodeRange> cr in codeRangeTable.ClassBlock.RangeTable)
            {
                if (cr.Value.InsideRange(cp))
                {   // yes, inside a class scope
                    si.ClassScope = cr.Key;

                    // check if it is inside a function 
                    foreach (KeyValuePair<int, ProtoCore.CodeModel.CodeRange> cr2 in codeRangeTable.ClassBlock.FunctionTable[si.ClassScope].RangeTable)
                    {
                        if (cr2.Value.InsideRange(cp))
                        {
                            // yes, it is inside a function
                            si.FunctionIndex = cr2.Key;
                            break;
                        }
                    }
                    break;
                }
            }

            // search what is the block id
            //
            // usually when the class scope is not -1, the block id should be always 0 since class is only allowed at global scope
            // However, language block is allowed in an expression, so it is possible that the current scope is inside a language block which is inside 
            // a member function of a class
            // eg: class A { def foo : int() { return = [Imperative] { return = 10; } } }
            // it is still necessary to figure out what is the actual block ID
            IEnumerable<KeyValuePair<int, ProtoCore.CodeModel.CodeRange>> blocks = codeRangeTable.CodeBlock.RangeTable.Where(x => x.Value.InsideRange(cp));

            // minCr holds the inner most block range which contais the target position
            ProtoCore.CodeModel.CodeRange minCr = new ProtoCore.CodeModel.CodeRange();
            if (blocks.Count() > 0)
                minCr = blocks.ElementAt(0).Value;
            foreach (KeyValuePair<int, ProtoCore.CodeModel.CodeRange> block in blocks)
            {
                if (minCr.InsideRange(block.Value))
                {
                    // more inner scope found, update minCr and si.BlockId
                    minCr = block.Value;
                    si.BlockId = block.Key;
                }
            }

            // not inside any class scope, need to figure out which function it is in 
            // Note: it is illegal to have a function inside a language block if the language block is inside a function
            // eg: def foo : int() { return = [Imperative] { def foo2 : int() { return = 20 }; return = foo2(); }; } 
            // so there is no need to search the fucntion index if it is inside a class 
            if (si.ClassScope == ProtoCore.DSASM.Constants.kInvalidIndex)
            {
                Dictionary<int, FunctionRangeTable> functionTable = codeRangeTable.CodeBlock.FunctionTable;
                if (null != functionTable && (si.BlockId >= 0) && (si.BlockId < functionTable.Count))
                {
                    // search its function index
                    foreach (KeyValuePair<int, ProtoCore.CodeModel.CodeRange> cr in codeRangeTable.CodeBlock.FunctionTable[si.BlockId].RangeTable)
                    {
                        if (cr.Value.InsideRange(cp))
                        {
                            si.FunctionIndex = cr.Key;
                            break;
                        }
                    }
                }
            }

            return si;
        }

        #endregion Type Infering Methods

        #region Members Querying Methods

        /// <summary>
        /// search global methos by name
        /// </summary>
        /// <param name="si"></param>
        /// <param name="function"></param>
        /// <returns>the key value pair is to keep consistent with the return data type from GetMemberFunctions, GetConstructors</returns>
        private static List<KeyValuePair<int, ProtoCore.DSASM.ProcedureNode>> GetGlobalMethod(ScopeInfo si, string function)
        {
            // in global scope, just search the global functions
            List<KeyValuePair<int, ProtoCore.DSASM.ProcedureNode>> functionList = new List<KeyValuePair<int, ProtoCore.DSASM.ProcedureNode>>();
            if (si.BlockId == ProtoCore.DSASM.Constants.kInvalidIndex)
                return functionList;

            ProtoCore.DSASM.CodeBlock cb = compileState.CodeBlockList[si.BlockId];
            while (cb != null)
            {
                if (null == cb.procedureTable)
                {
                    cb = cb.parent;
                    continue;
                }

                // search by name
                IEnumerable<ProtoCore.DSASM.ProcedureNode> pns = cb.procedureTable.procList.Where(x => x.name == function);
                if (null == pns || (pns.Count() == 0))
                {
                    cb = cb.parent; // search its parent
                    continue;
                }
                foreach (ProtoCore.DSASM.ProcedureNode pn in pns)
                {
                    functionList.Add(new KeyValuePair<int, ProtoCore.DSASM.ProcedureNode>(ProtoCore.DSASM.Constants.kInvalidIndex, pn));
                }
                break;
            }

            return functionList;
        }

        private static ProtoCore.DSASM.SymbolNode GetMemberVariable(int classIndex, string field, ScopeInfo si, bool isStatic)
        {
            if (field.IndexOfAny(new char[] { '[', ']' }) != -1)
            {
                string[] tokens = TokenizeArrayIndexers(field);
                field = tokens[0];
            }

            // one search is enough, because the code gen will copy all the accessible base 
            // class member variabs to the current class table, life easier
            ProtoCore.DSASM.ClassNode cn = compileState.ClassTable.ClassNodes[classIndex];
            if (cn.symbols == null)
                return null;
            for (int ix = cn.symbols.symbolList.Values.Count - 1; ix >= 0; --ix)
            {
                // we should search in reverse order because in code gen 
                // the base class member variable is added first 
                // hence we shoud search the its own variable first 
                ProtoCore.DSASM.SymbolNode sn = cn.symbols.symbolList[ix];
                if (sn.functionIndex != ProtoCore.DSASM.Constants.kInvalidIndex ||
                    sn.name != field ||
                    isStatic != sn.isStatic)
                    continue;

                // here we have found a variable with the same name
                if (GetAccessLevel(si.ClassScope, sn.classScope) >= sn.access)
                    return sn;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="classIndex"></param>
        /// <param name="function"></param>
        /// <param name="si"></param>
        /// <param name="isStatic"></param>
        /// <returns>
        /// the key means the function index of thr procedure, not that it may be different from argument classIndex if 
        /// the procedure comes from its base class
        /// </returns>
        private static List<KeyValuePair<int, ProtoCore.DSASM.ProcedureNode>> GetMemberFunctions(int classIndex, string function, ScopeInfo si, bool isStatic)
        {
            ProtoCore.DSASM.ClassNode cn = compileState.ClassTable.ClassNodes[classIndex];
            List<KeyValuePair<int, ProtoCore.DSASM.ProcedureNode>> functionList = new List<KeyValuePair<int, ProtoCore.DSASM.ProcedureNode>>();
            // first search its own functions
            if (cn.vtable == null)
                return functionList;
            foreach (ProtoCore.DSASM.ProcedureNode pn in cn.vtable.procList)
            {
                if (pn.isConstructor ||
                    pn.name != function ||
                    pn.isStatic != isStatic)
                    continue;

                // here we have found a function with the same name 
                // check if it is accessible
                if (GetAccessLevel(si.ClassScope, classIndex) >= pn.access)
                    functionList.Add(new KeyValuePair<int, ProtoCore.DSASM.ProcedureNode>(classIndex, pn));
            }

            // continue to search its base class
            foreach (int baseClass in cn.baseList)
            {
                functionList.AddRange(GetMemberFunctions(baseClass, function, si, isStatic));
            }

            return functionList;
        }


        private static List<KeyValuePair<int, ProtoCore.DSASM.ProcedureNode>> GetConstructors(int classIndex, string function, ScopeInfo si)
        {
            ProtoCore.DSASM.ClassNode cn = compileState.ClassTable.ClassNodes[classIndex];
            ProtoCore.DSASM.AccessSpecifier access = GetAccessLevel(si.ClassScope, classIndex);
            List<KeyValuePair<int, ProtoCore.DSASM.ProcedureNode>> functionList = new List<KeyValuePair<int, ProtoCore.DSASM.ProcedureNode>>();
            if (cn.vtable == null)
                return functionList;
            foreach (ProtoCore.DSASM.ProcedureNode pn in cn.vtable.procList)
            {
                if (pn.name == function && pn.isConstructor && access >= pn.access)
                    functionList.Add(new KeyValuePair<int, ProtoCore.DSASM.ProcedureNode>(classIndex, pn));
            }

            // no need to search base constructors, 

            return functionList;
        }

        /// <summary>
        /// This is a helper methods, when the IDE passes "A.foo" or "A." we are always expecting both the constructors and static member functions 
        /// This method will search them both making use of the existing GetMemberFunctions and GetConstructors method
        /// </summary>
        /// <param name="classIndex"></param>
        /// <param name="function"></param>
        /// <param name="si"></param>
        /// <returns></returns>
        private static List<KeyValuePair<int, ProtoCore.DSASM.ProcedureNode>> GetStatisMemberFunctionAndConstructor(int classIndex, string function, ScopeInfo si)
        {
            return GetMemberFunctions(classIndex, function, si, true).Concat(GetConstructors(classIndex, function, si)).ToList();
        }

        /// <summary>
        /// Return all the members of a class
        /// </summary>
        /// <param name="classIndex"></param>
        /// <param name="si"></param>
        /// <param name="searchingBase">if searchingBase == true, the MemberType.FromBase of the returned members will be set </param>
        /// <param name="isStatic"></param>
        /// <param name="includeConstructor"></param>
        /// <returns></returns>
        private static List<KeyValuePair<MemberType, string>> GetClassMembers(int classIndex, ScopeInfo si, bool searchingBase, bool isStatic, bool includeConstructor)
        {
            List<KeyValuePair<MemberType, string>> members = new List<KeyValuePair<MemberType, string>>();
            ProtoCore.DSASM.ClassNode cn = compileState.ClassTable.ClassNodes[classIndex];

            // 1. search the member variable
            if (cn.symbols != null && !searchingBase) // when searching base, no need to populate the member variable, it has already been added in code gen
            {
                foreach (ProtoCore.DSASM.SymbolNode sn in cn.symbols.symbolList.Values.Where(x => x.functionIndex == ProtoCore.DSASM.Constants.kInvalidIndex))
                {

                    if (GetAccessLevel(si.ClassScope, sn.classScope) >= sn.access &&
                        isStatic == sn.isStatic &&
                        !sn.name.StartsWith("%") /* block the internally generated members*/ )
                    {
                        KeyValuePair<MemberType, string> member = new KeyValuePair<MemberType, string>(MemberType.Fields | AccessToMemberType(sn.access) | StaticToMemberType(isStatic) | BaseToMemberType(sn.classScope != classIndex), sn.name);
                        members.Add(member);
                    }
                }
            }
            // 2. search the member functions and constructors
            if (cn.vtable != null)
            {
                foreach (ProtoCore.DSASM.ProcedureNode pn in cn.vtable.procList)
                {
                    // get static members 
                    if (GetAccessLevel(si.ClassScope, classIndex) >= pn.access && isStatic == pn.isStatic && pn.isConstructor == false && !pn.name.StartsWith("%") && pn.isAutoGenerated != true)
                    {
                        KeyValuePair<MemberType, string> member = new KeyValuePair<MemberType, string>(MemberType.Methods | AccessToMemberType(pn.access) | StaticToMemberType(pn.isStatic) | BaseToMemberType(searchingBase) | ConstructorToMemberType(pn.isConstructor), pn.name);
                        members.Add(member);
                    }
                    // get constructor 
                    if (GetAccessLevel(si.ClassScope, classIndex) >= pn.access && true == pn.isConstructor && includeConstructor == true && !pn.name.StartsWith("%") && pn.isAutoGenerated != true)
                    {
                        KeyValuePair<MemberType, string> member = new KeyValuePair<MemberType, string>(MemberType.Methods | AccessToMemberType(pn.access) | StaticToMemberType(pn.isStatic) | BaseToMemberType(searchingBase) | ConstructorToMemberType(pn.isConstructor), pn.name);
                        members.Add(member);
                    }
                }
            }

            // 3. search base class
            if (cn.baseList != null) // Note, for nonstatic members it is been copied to the derived class, hence they have been added already 
            {
                foreach (int baseIndex in cn.baseList)
                {
                    // searching base will be set to true since we are searching the base class now
                    members.AddRange(GetClassMembers(baseIndex, si, true, isStatic, false));
                }
            }

            return members;
        }

        #endregion Members Querying Methods
    }
}
