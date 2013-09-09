using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using ProtoCore.CodeModel;
using ProtoCore.DSASM;
using DesignScript.Editor.CodeGen;

namespace DesignScript.Editor.AutoComplete
{
    struct ParentChildPair
    {
        private bool isClass;
        private int parent, child;

        public ParentChildPair(int parent, int child, bool isClass)
        {
            this.parent = parent;
            this.child = child;
            this.isClass = isClass;
        }

        public int Parent { get { return this.parent; } }
        public int Child { get { return this.child; } }
        public bool IsClass { get { return this.isClass; } }
    }

    class TableNames
    {
        public const string ScopeRanges = "ScopeRanges";
        public const string DataTypes = "DataTypes";
        public const string Classes = "Classes";
        public const string Symbols = "Symbols";
        public const string Procedures = "Procedures";
    }

    class SymbolDatabaseProxy
    {
        ProtoLanguage.CompileStateTracker compileState = null;
        SQLiteConnection connection = null;
        List<ParentChildPair> hierarchy = null;
        Dictionary<string, int> scriptIdentifiers = null;

        #region Public Class Operational Methods

        internal SymbolDatabaseProxy(ProtoLanguage.CompileStateTracker compileState, Dictionary<ScopeInfo, CodeRange> scopeIdentifiers)
        {
            this.compileState = compileState;

            try
            {
                string connString = "Data Source=:memory:";
                connection = new SQLiteConnection(connString);
                connection.Open();
            }
            catch (Exception)
            {
                if (null != connection)
                    connection.Dispose();

                connection = null;
                return;
            }

            PopulateDatabase(scopeIdentifiers);
        }

        internal void CloseConnection()
        {
            if (null != connection)
            {
                if (connection.State != System.Data.ConnectionState.Closed)
                    connection.Close();
                connection.Dispose();
            }

            connection = null;
        }

        internal List<CompletionItem> GetCompletionList(CodePoint location, string idents)
        {
            ScopeInfo scopeIdentifier = new ScopeInfo();
            if (!GetScopeIdentifier(location, ref scopeIdentifier))
                return null; // Unknown scope.

            bool isStatic = false;
            int identifierType = GetIdentifierType(scopeIdentifier, location, idents, true, ref isStatic);
            if (ProtoCore.DSASM.Constants.kInvalidIndex == identifierType)
                return null; // No identifier of given name found in scope.

            return (GetClassMembers(identifierType, isStatic, scopeIdentifier));
        }

        internal List<MethodSignature> GetMethodSignatures(CodePoint location, string idents)
        {
            ScopeInfo scopeIdentifier = new ScopeInfo();
            if (!GetScopeIdentifier(location, ref scopeIdentifier))
                return null; // Unknown scope.

            // This identifier list traversal is slightly different from the one for 
            // 'GetCompletionList'. For example, "a.b.c.d.e.foo" where the "foo" 
            // identifier is expected to be a method name, so the logic needs only to
            // traverse up to "e" to resolve its type, disregarding "foo".
            // 
            bool isStatic = false;
            int identifierType = GetIdentifierType(scopeIdentifier, location, idents, false, ref isStatic);

            // If the "search site" is not within any class body/method.
            if (ProtoCore.DSASM.Constants.kInvalidIndex == scopeIdentifier.ClassScope)
            {
                // If the "search target" is in global scope...
                if (ProtoCore.DSASM.Constants.kInvalidIndex == identifierType)
                {
                    // Only get those global methods that belong to the same 
                    // block as the search site (methods residing in another 
                    // block cannot be accessed within the current block).
                    // 
                    // GetMethodSignatures(scopeIdentifier)
                }

                // If the "search target" is in a class...
                else
                {
                    // While looking up methods of a class in a global scope,
                    // it can at most access methods that are made public.
                    // 
                    // GetClassMethods(identifierType, false, AccessSpecifier.kPublic, results);
                }
            }

            // If the "search site" is within a class body/method.
            else
            {
                // If user only types "foo" (instead of "a.b.foo") within a class 
                // body/method, then look for "foo" within the current class, or 
                // in any base classes if it can be accessed from the search site.
                // 
                if (ProtoCore.DSASM.Constants.kInvalidIndex == identifierType)
                {
                }

                // If user types "a.b.foo", then at most public methods can be 
                // accessed. But if user types "this.foo", then a traversal is 
                // required up the class hierarchy to obtain methods with name "foo".
                else
                {
                }
            }

            return null;
        }

        internal static CodeRange MakeCodeRange(int startLine, int startCol, int endLine, int endCol, string filePath)
        {
            ProtoCore.CodeModel.CodeFile codeFile = new ProtoCore.CodeModel.CodeFile()
            {
                FilePath = filePath
            };

            return new ProtoCore.CodeModel.CodeRange
            {
                StartInclusive = new ProtoCore.CodeModel.CodePoint
                {
                    LineNo = startLine,
                    CharNo = startCol,
                    SourceLocation = codeFile
                },

                EndExclusive = new ProtoCore.CodeModel.CodePoint
                {
                    LineNo = endLine,
                    CharNo = endCol,
                    SourceLocation = codeFile
                }
            };
        }

        #endregion

        #region Public Class Properties

        internal string ErrorMessage { get; private set; }

        internal bool IsValid
        {
            get { return (null != compileState && (null != connection)); }
        }

        #endregion

        #region Private Database Creation Methods

        private void PopulateDatabase(Dictionary<ScopeInfo, CodeRange> scopeIdentifiers)
        {
            if (CreateTables())
            {
                ProcessScopeIdentifiers(scopeIdentifiers);

                List<CodeBlock> codeBlockList = compileState.CompleteCodeBlockList;
                if (null == codeBlockList || (codeBlockList.Count <= 0))
                    codeBlockList = compileState.CodeBlockList;

                if (null != codeBlockList)
                {
                    foreach (CodeBlock codeBlock in codeBlockList)
                        ProcessCodeBlock(codeBlock);
                }

                foreach (ClassNode classNode in compileState.ClassTable.ClassNodes)
                    ProcessClassNode(classNode);
            }
        }

        private bool CreateTables()
        {
            List<string> statements = new List<string>();

            string[] scopeRanges = 
            {
                "Script         INTEGER", // CONSTRAINT FkScopeRangesScripts REFERENCES Scripts(Id)
                "StartLine      INTEGER",
                "StartColumn    INTEGER",
                "EndLine        INTEGER",
                "EndColumn      INTEGER",
                "CodeBlock      INTEGER",
                "ClassScope     INTEGER",
                "Procedure      INTEGER",
            };

            statements.Add(CreateTableStatement(TableNames.ScopeRanges, scopeRanges));

            string[] dataTypes = 
            {
                "Id             INTEGER PRIMARY KEY",
                "IsIndexable    BOOLEAN",
                "Name           NCHAR(255)",
                "Rank           INTEGER"
            };

            statements.Add(CreateTableStatement(TableNames.DataTypes, dataTypes));

            string[] classes =
            {
                "ClassScope     INTEGER",
                "Name           NCHAR(255)",
                "IsImported     BOOLEAN",
                "Rank           INTEGER",
                "Size           INTEGER"
            };

            statements.Add(CreateTableStatement(TableNames.Classes, classes));

            string[] symbols =
            {
                "CodeBlock      INTEGER",
                "ClassScope     INTEGER",
                "Procedure      INTEGER",
                "Name           NCHAR(255)",
                "AccessSpec     INTEGER",
                "DataType       INTEGER",
                "StaticType     INTEGER",
                "IsArgument     BOOLEAN",
                "IsArray        BOOLEAN",
                "IsStatic       BOOLEAN",
                "IsTemporary    BOOLEAN"
            };

            statements.Add(CreateTableStatement(TableNames.Symbols, symbols));

            string[] procedures =
            {
                "CodeBlock      INTEGER",
                "ClassScope     INTEGER",
                "Procedure      INTEGER",
                "Name           NCHAR(255)",
                "AccessSpec     INTEGER",
                "ReturnType     INTEGER",
                "IsExternal     BOOLEAN",
                "IsStatic       BOOLEAN",
                "IsConstructor  BOOLEAN"
            };

            statements.Add(CreateTableStatement(TableNames.Procedures, procedures));

            try
            {
                List<SQLiteCommand> commands = new List<SQLiteCommand>();
                foreach (string statement in statements)
                    commands.Add(new SQLiteCommand(statement, connection));

                SQLiteTransaction transaction = connection.BeginTransaction();
                foreach (SQLiteCommand command in commands)
                    command.ExecuteNonQuery();

                transaction.Commit();
            }
            catch (Exception exception)
            {
                HandleException(exception);
                return false;
            }

            return true;
        }

        private void ProcessScopeIdentifiers(Dictionary<ScopeInfo, CodeRange> scopeIdentifiers)
        {
            if (null == scopeIdentifiers)
                return;

            foreach (var identifier in scopeIdentifiers)
            {
                ScopeInfo scope = identifier.Key;
                CodePoint start = identifier.Value.StartInclusive;
                CodePoint end = identifier.Value.EndExclusive;
                string scriptPath = start.SourceLocation.FilePath;

                string[] fields =
                {
                    "Script",
                    "StartLine",
                    "StartColumn",
                    "EndLine",
                    "EndColumn",
                    "CodeBlock",
                    "ClassScope",
                    "Procedure",
                };

                string[] values =
                {
                    GetScriptIdentifier(scriptPath).ToString(),
                    start.LineNo.ToString(),
                    start.CharNo.ToString(),
                    end.LineNo.ToString(),
                    end.CharNo.ToString(),
                    scope.BlockId.ToString(),
                    scope.ClassScope.ToString(),
                    scope.FunctionIndex.ToString()
                };

                try
                {
                    string statement = InsertIntoStatement(TableNames.ScopeRanges, fields, values);
                    SQLiteCommand command = new SQLiteCommand(statement, connection);
                    command.ExecuteNonQuery();
                }
                catch (Exception exception)
                {
                    HandleException(exception);
                }
            }
        }

        private void ProcessCodeBlock(CodeBlock codeBlock)
        {
            if (null == codeBlock)
                return;

            int parent = ((null == codeBlock.parent) ? -1 : codeBlock.parent.codeBlockId);
            DefineHierarchy(parent, codeBlock.codeBlockId, false);

            ProcedureTable procTable = codeBlock.procedureTable;
            foreach (ProcedureNode procNode in procTable.procList)
                ProcessProcedureNode(procNode);

            SymbolTable symbolTable = codeBlock.symbolTable;
            foreach (var symbolNode in symbolTable.symbolList)
                ProcessSymbolNode(symbolNode.Value);
        }

        private void ProcessClassNode(ClassNode classNode)
        {
            if (null == classNode)
                return;

            // Define class inheritance relationships.
            foreach (int baseClassId in classNode.baseList)
                DefineHierarchy(baseClassId, classNode.classId, true);

            string[] fields =
            {
                "ClassScope",
                "Name",
                "IsImported",
                "Rank",
                "Size"
            };

            string[] values =
            {
                classNode.classId.ToString(),
                GetDatabaseType(classNode.name),
                GetDatabaseType(classNode.IsImportedClass),
                classNode.rank.ToString(),
                classNode.size.ToString()
            };

            try
            {
                string statement = InsertIntoStatement(TableNames.Classes, fields, values);
                SQLiteCommand command = new SQLiteCommand(statement, connection);
                command.ExecuteNonQuery();
            }
            catch (Exception exception)
            {
                HandleException(exception);
            }

            // Now process all the class procedures (class methods), excluding
            // all those that are auto-generated (or temporary). Excluded 
            // procedure IDs are placed in 'excludedProcId' list for use later
            // when symbol nodes are processed.
            // 
            List<int> excludedProcId = new List<int>();
            if (null != classNode.vtable && (null != classNode.vtable.procList))
            {
                foreach (ProcedureNode procedure in classNode.vtable.procList)
                {
                    // Place generated procedures in a list.
                    if (ProcedureToBeExcluded(procedure))
                    {
                        excludedProcId.Add(procedure.procId);
                        continue;
                    }

                    // Constructors have classScope set to "-1", 
                    // so we must be careful not to exclude them.
                    if (procedure.isConstructor && (-1 == procedure.classScope))
                    {
                        procedure.classScope = classNode.classId;
                        ProcessProcedureNode(procedure);
                        procedure.classScope = -1;
                        continue;
                    }

                    // Adding a regular class method.
                    ProcessProcedureNode(procedure);
                }
            }

            // Go through each of the symbol nodes defined in this class node, 
            // they can either be class data members or locals defined in class 
            // methods. Symbol nodes defined locally in those procedures that 
            // were excluded in the previous pass are also present here, 
            // therefore it is crucial that they are properly filtered here.
            // 
            if (null != classNode.symbols && (null != classNode.symbols.symbolList))
            {
                int verifiedProcId = -1000;
                bool procedureIncluded = false;
                foreach (var symbol in classNode.symbols.symbolList)
                {
                    SymbolNode symbolNode = symbol.Value;
                    if (symbolNode.classScope != classNode.classId)
                        continue; // Data member of base class (processed).

                    // New function index, verify if it needs to be included.
                    if (symbolNode.functionIndex != verifiedProcId)
                    {
                        procedureIncluded = false;
                        verifiedProcId = symbolNode.functionIndex;
                        if (excludedProcId.IndexOf(verifiedProcId) == -1)
                            procedureIncluded = true; // Procedure was included.
                    }

                    if (false != procedureIncluded)
                        ProcessSymbolNode(symbolNode);
                }
            }
        }

        private void ProcessProcedureNode(ProcedureNode procedureNode)
        {
            if (ProcedureToBeExcluded(procedureNode))
                return; // Non-public visible procedures.

            string[] fields =
            {
                "CodeBlock",
                "ClassScope",
                "Procedure",
                "Name",
                "AccessSpec",
                "ReturnType",
                "IsExternal",
                "IsStatic",
                "IsConstructor"
            };

            string[] values =
            {
                procedureNode.runtimeIndex.ToString(),
                procedureNode.classScope.ToString(),
                procedureNode.procId.ToString(),
                GetDatabaseType(procedureNode.name),
                ((int)procedureNode.access).ToString(),
                procedureNode.returntype.UID.ToString(),
                GetDatabaseType(procedureNode.isExternal),
                GetDatabaseType(procedureNode.isStatic),
                GetDatabaseType(procedureNode.isConstructor)
            };

            try
            {
                string statement = InsertIntoStatement(TableNames.Procedures, fields, values);
                SQLiteCommand command = new SQLiteCommand(statement, connection);
                command.ExecuteNonQuery();
            }
            catch (Exception exception)
            {
                HandleException(exception);
            }
        }

        private void ProcessSymbolNode(SymbolNode symbolNode)
        {
            if (SymbolToBeExcluded(symbolNode))
                return;

            string[] fields =
            {
                "CodeBlock",
                "ClassScope",
                "Procedure",
                "Name",
                "AccessSpec",
                "DataType",
                "StaticType",
                "IsArgument",
                "IsArray",
                "IsStatic",
                "IsTemporary"
            };

            string[] values =
            {
                symbolNode.codeBlockId.ToString(),
                symbolNode.classScope.ToString(),
                symbolNode.functionIndex.ToString(),
                GetDatabaseType(symbolNode.name),
                ((int)symbolNode.access).ToString(),
                symbolNode.datatype.UID.ToString(),
                symbolNode.staticType.UID.ToString(),
                GetDatabaseType(symbolNode.isArgument),
                GetDatabaseType(symbolNode.isArray),
                GetDatabaseType(symbolNode.isStatic),
                GetDatabaseType(symbolNode.isTemp)
            };

            try
            {
                string statement = InsertIntoStatement(TableNames.Symbols, fields, values);
                SQLiteCommand command = new SQLiteCommand(statement, connection);
                command.ExecuteNonQuery();
            }
            catch (Exception exception)
            {
                HandleException(exception);
            }
        }

        #endregion

        #region Private Database Query Methods

        private bool GetScopeIdentifier(CodePoint location, ref ScopeInfo scopeIdentifier)
        {
            string[] conditions = 
            {
                "StartLine <= " + location.LineNo.ToString(),
                "EndLine >= " + location.LineNo.ToString()
            };

            Dictionary<ScopeInfo, CodeRange> scopeIdentifiers = null;

            try
            {
                string statement = SelectFromStatement(TableNames.ScopeRanges, conditions);
                SQLiteCommand command = new SQLiteCommand(statement, connection);
                SQLiteDataReader reader = command.ExecuteReader();
                if (null == reader || (false == reader.HasRows))
                    return false; // No record was found matching.

                while (reader.Read())
                {
                    int startLine = reader.GetInt32(1);
                    int startColumn = reader.GetInt32(2);
                    int endLine = reader.GetInt32(3);
                    int endColumn = reader.GetInt32(4);

                    bool withinRange = true;
                    if (location.LineNo == startLine && (location.CharNo < startColumn))
                        withinRange = false;
                    else if (location.LineNo == endLine && (location.CharNo >= endColumn))
                        withinRange = false;

                    if (false == withinRange)
                        continue;

                    if (null == scopeIdentifiers)
                        scopeIdentifiers = new Dictionary<ScopeInfo, CodeRange>();

                    string path = GetScriptPath(reader.GetInt32(0));
                    ScopeInfo scope = new ScopeInfo()
                    {
                        BlockId = reader.GetInt32(5),
                        ClassScope = reader.GetInt32(6),
                        FunctionIndex = reader.GetInt32(7)
                    };

                    CodeRange range = MakeCodeRange(startLine, startColumn, endLine, endColumn, path);
                    scopeIdentifiers.Add(scope, range);
                }
            }
            catch (Exception exception)
            {
                HandleException(exception);
                return false;
            }

            if (null == scopeIdentifiers || (scopeIdentifiers.Count <= 0))
                return false;

            // Attempt to find the inner-most scope by seeing 
            // if one scope completely contains another scope.
            // 
            bool foundInnerMostScope = false;
            CodeRange innerMostScope = new CodeRange();
            foreach (var identifier in scopeIdentifiers)
            {
                if (false == foundInnerMostScope)
                {
                    innerMostScope = identifier.Value;
                    scopeIdentifier = identifier.Key;
                    foundInnerMostScope = true;
                    continue;
                }

                if (innerMostScope.InsideRange(identifier.Value))
                {
                    innerMostScope = identifier.Value;
                    scopeIdentifier = identifier.Key;
                }
            }

            return foundInnerMostScope;
        }

        private int GetIdentifierType(ScopeInfo scopeIdentifier, CodePoint location,
            string identifier, bool includeLastIdent, ref bool isStatic)
        {
            isStatic = false;
            if (string.IsNullOrEmpty(identifier))
                return ProtoCore.DSASM.Constants.kInvalidIndex;

            // Split the identifier list (e.g. "a.b.c" -> { "a", "b", "c" })
            char[] seperator = { '.' };
            string[] idents = identifier.Split(seperator, StringSplitOptions.RemoveEmptyEntries);
            if (null == idents || (idents.Length <= 0))
                return ProtoCore.DSASM.Constants.kInvalidIndex;

            // The caller may choose to drop the last identifier in the list, if 
            // it represents a method name. For example, "a.b.foo", the search 
            // only goes up to determine the type of "b" while disregarding "foo".
            if (false == includeLastIdent)
            {
                List<string> intermediate = idents.ToList();
                intermediate.RemoveAt(intermediate.Count - 1);
                if (intermediate.Count <= 0)
                    return ProtoCore.DSASM.Constants.kInvalidIndex;

                idents = idents.ToArray();
            }

            // First we need to find out the type of the "root 
            // variable". For example, "a" in the case of "a.b.c".
            // 
            string rootIdentifier = idents[0];
            int rootType = ProtoCore.DSASM.Constants.kInvalidIndex;

            if (ProtoCore.DSASM.Constants.kInvalidIndex != scopeIdentifier.ClassScope)
            {
                // If this symbol is in a language block within a class definition...
                if (scopeIdentifier.BlockId != 0)
                    rootType = GetBlockMemberType(scopeIdentifier, rootIdentifier);

                // This identifier is found within a class definition region.
                if (ProtoCore.DSASM.Constants.kInvalidIndex == rootType)
                    rootType = GetClassMemberType(scopeIdentifier, rootIdentifier, isStatic);
            }
            else
            {
                // For identifiers found outside of the class definition region...
                rootType = GetBlockMemberType(scopeIdentifier, rootIdentifier);
            }

            // It seems like the identifier does not correspond to a valid variable
            // name given the scope it resides in. Check to see if it represents the 
            // name of an existing class.
            if (ProtoCore.DSASM.Constants.kInvalidIndex == rootType)
            {
                bool isImported = false;
                rootType = GetClassScopeFromName(rootIdentifier, ref isImported);
                if (ProtoCore.DSASM.Constants.kInvalidIndex == rootType)
                    return ProtoCore.DSASM.Constants.kInvalidIndex;

                // We have found a class with name matching "rootIdentifier", 
                // which means the subsequent identifier(s) is a static member.
                isStatic = true;
            }

            int finalType = ProtoCore.DSASM.Constants.kInvalidIndex;
            for (int index = 1; index < idents.Length; ++index)
            {
                string ident = idents[index];
                finalType = GetClassMemberType(scopeIdentifier, ident, isStatic);
                if (ProtoCore.DSASM.Constants.kInvalidIndex == finalType)
                    return ProtoCore.DSASM.Constants.kInvalidIndex;

                isStatic = false; // Can't be static after the root level.
            }

            return finalType;
        }

        private List<CompletionItem> GetClassMembers(int classScope, bool isStatic, ScopeInfo scopeIdentifier)
        {
            if (ProtoCore.DSASM.Constants.kInvalidIndex == classScope)
            {
                string mesg = "'GetClassMembers' called for invalid class scope!";
                throw new InvalidOperationException(mesg);
            }

            List<int> classesToSearch = GetAllBaseClasses(classScope);

            bool withinClassHierarchy = false;
            if (ProtoCore.DSASM.Constants.kInvalidIndex != scopeIdentifier.ClassScope)
            {
                // "scopeIdentifier" represents the scope in which AutoComplete list 
                // has been requested, this is referred to as "search site". This line 
                // checks to see if the search site is falling within the class 
                // hierarchy leading up from 'classScope' (i.e. the class out of which 
                // members are to be retrieved).
                // 
                withinClassHierarchy = classesToSearch.Contains(scopeIdentifier.ClassScope);
            }

            // The resulting completion list.
            List<CompletionItem> results = new List<CompletionItem>();

            while (classesToSearch.Count > 0)
            {
                int currentClass = classesToSearch[0];
                classesToSearch.RemoveAt(0);

                // If the site is within a class body or class method, then the 
                // result of searching the same class should include even the 
                // private class properties/methods. As the search goes up to 
                // the parent class, then search results should exclude private 
                // properties/methods. If the search site is outside of a class 
                // body, then only public properties/methods should be returned.
                // 
                AccessSpecifier maxAllowedAccess = AccessSpecifier.kPublic;
                if (currentClass == classScope)
                {
                    // Within the current class, allow private access.
                    maxAllowedAccess = AccessSpecifier.kPrivate;
                }
                else if (false != withinClassHierarchy)
                {
                    // Not within the 'classScope', but somewhere along 
                    // the class hierarchy, allow access to protected members.
                    maxAllowedAccess = AccessSpecifier.kProtected;
                }

                GetClassProperties(currentClass, isStatic, maxAllowedAccess, results);
                GetClassMethods(currentClass, isStatic, maxAllowedAccess, results);
            }

            return results;
        }

        private void GetMethodSignatures(ScopeInfo scopeIdentifier, bool isStatic,
            string name, AccessSpecifier maxAllowedAccess, List<MethodSignature> results)
        {
            // Searching for a named method(s) in a given class (goes way up in hierarchy).
            if (ProtoCore.DSASM.Constants.kInvalidIndex != scopeIdentifier.ClassScope)
            {
                List<int> classesToSearch = GetAllBaseClasses(scopeIdentifier.ClassScope);
                while (classesToSearch.Count > 0)
                {
                    int currentClass = classesToSearch[0];
                    classesToSearch.RemoveAt(0);

                    // 0  "CodeBlock      INTEGER",
                    // 1  "ClassScope     INTEGER",
                    // 2  "Procedure      INTEGER",
                    // 3  "Name           NCHAR(255)",
                    // 4  "AccessSpec     INTEGER",
                    // 5  "ReturnType     INTEGER",
                    // 6  "IsExternal     BOOLEAN",
                    // 7  "IsStatic       BOOLEAN",
                    // 8  "IsConstructor  BOOLEAN"
                    // 
                    // 9  "Id             INTEGER PRIMARY KEY",
                    // 10 "IsIndexable    BOOLEAN",
                    // 11 "Name           NCHAR(255)",
                    // 12 "Rank           INTEGER"

                    string[] conditions =
                    {
                        "CodeBlock = 0",
                        "ClassScope = " + currentClass.ToString(),
                        "Name = " + GetDatabaseType(name),
                        "AccessSpec <= " + ((int)maxAllowedAccess).ToString(),
                        "IsStatic = " + GetDatabaseType(isStatic)
                    };

                    string[] joints = 
                    {
                        "p.ReturnType = t.Id"
                    };

                    try
                    {
                        string leftTable = TableNames.Procedures + " p";
                        string rightTable = TableNames.DataTypes + " t";

                        string statement = SelectJoinStatement(
                            leftTable, rightTable, joints, conditions);

                        SQLiteCommand command = new SQLiteCommand(statement, connection);
                        SQLiteDataReader reader = command.ExecuteReader();
                        if (null != reader && (false != reader.HasRows))
                        {
                            while (reader.Read())
                            {
                                ScopeInfo methodScope = new ScopeInfo()
                                {
                                    BlockId = 0,
                                    ClassScope = currentClass,
                                    FunctionIndex = reader.GetInt32(2) // "p.Procedure" field.
                                };

                                MethodSignature signature = GetMethodSignature(methodScope, name);
                                if (null == signature)
                                    continue;

                                signature.ReturnType = reader.GetString(11); // "t.Name" field.
                                signature.IsProperty = false;
                                signature.IsExternal = reader.GetBoolean(6); // "IsExternal" field.
                                signature.IsStatic = reader.GetBoolean(7); // "IsStatic" field.
                                signature.Access = ((AccessSpecifier)reader.GetInt32(4)); // "AccessSpec" field.

                                if (reader.GetBoolean(8)) // "IsConstructor" field.
                                    signature.IsConstructor = true;
                                else
                                    signature.IsMethod = true;

                                results.Add(signature);
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        HandleException(exception);
                    }

                    // While the search moves up the class hierarchy, access will 
                    // turn from "private" to "protected". If the 'maxAllowedAccess' 
                    // was "public" to begin with, then it stays as "public".
                    // 
                    if (maxAllowedAccess == AccessSpecifier.kPrivate)
                        maxAllowedAccess = AccessSpecifier.kProtected;
                }
            }
            else
            {
                // Looking for a global method in scope with the given name.
            }
        }

        private MethodSignature GetMethodSignature(ScopeInfo methodScope, string methodName)
        {
            // @TODO(Ben): Remove these comments.
            // 
            // 0  "CodeBlock      INTEGER",
            // 1  "ClassScope     INTEGER",
            // 2  "Procedure      INTEGER",
            // 3  "Name           NCHAR(255)",
            // 4  "AccessSpec     INTEGER",
            // 5  "DataType       INTEGER",
            // 6  "StaticType     INTEGER",
            // 7  "IsArgument     BOOLEAN",
            // 8  "IsArray        BOOLEAN",
            // 9  "IsStatic       BOOLEAN",
            // 10 "IsTemporary    BOOLEAN"
            // 
            // 11 "Id             INTEGER PRIMARY KEY",
            // 12 "IsIndexable    BOOLEAN",
            // 13 "Name           NCHAR(255)",
            // 14 "Rank           INTEGER"

            string[] conditions =
            {
                "CodeBlock = " + methodScope.BlockId.ToString(),
                "ClassScope = " + methodScope.ClassScope.ToString(),
                "Procedure = " + methodScope.FunctionIndex.ToString()
            };

            string[] joints =
            {
                "s.StaticType = t.Id"
            };

            MethodSignature signature = new MethodSignature(methodName);

            try
            {
                string leftTable = TableNames.Symbols + " s";
                string rightTable = TableNames.DataTypes + " t";
                string statement = SelectJoinStatement(leftTable, rightTable, joints, conditions);
                SQLiteCommand command = new SQLiteCommand(statement, connection);
                SQLiteDataReader reader = command.ExecuteReader();
                if (null != reader && (false != reader.HasRows))
                {
                    while (reader.Read())
                    {
                        // "s.Name", "t.Name" and no default argument.
                        signature.AddArgument(reader.GetString(3), reader.GetString(13), null);
                    }
                }
            }
            catch (Exception exception)
            {
                HandleException(exception);
            }

            return null;
        }

        private void GetClassProperties(int classScope, bool isStatic,
            AccessSpecifier maxAllowedAccess, List<CompletionItem> results)
        {
            string[] conditions =
            {
                "CodeBlock = 0",
                "ClassScope = " + classScope.ToString(),
                "Procedure = -1",
                "IsStatic = " + GetDatabaseType(isStatic),
                "AccessSpec <= " + ((int)maxAllowedAccess).ToString()
            };

            try
            {
                string statement = SelectFromStatement(TableNames.Symbols, conditions);
                SQLiteCommand command = new SQLiteCommand(statement, connection);
                SQLiteDataReader reader = command.ExecuteReader();
                if (null != reader && (false != reader.HasRows))
                {
                    while (reader.Read())
                    {
                        results.Add(new CompletionItem(reader.GetString(3))
                        {
                            IsStatic = reader.GetBoolean(9),
                            IsConstructor = false,
                            IsMethod = false,
                            IsProperty = true,
                            Access = ((AccessSpecifier)reader.GetInt32(4))
                        });
                    }
                }
            }
            catch (Exception exception)
            {
                HandleException(exception);
            }
        }

        private void GetClassMethods(int classScope, bool isStatic,
            AccessSpecifier maxAllowedAccess, List<CompletionItem> results)
        {
            string[] conditions =
            {
                "CodeBlock = 0",
                "ClassScope = " + classScope.ToString(),
                "IsStatic = " + GetDatabaseType(isStatic),
                "AccessSpec <= " + ((int)maxAllowedAccess).ToString()
            };

            try
            {
                string statement = SelectFromStatement(TableNames.Procedures, conditions);
                SQLiteCommand command = new SQLiteCommand(statement, connection);
                SQLiteDataReader reader = command.ExecuteReader();
                if (null != reader && (false != reader.HasRows))
                {
                    while (reader.Read())
                    {
                        results.Add(new CompletionItem(reader.GetString(3))
                        {
                            IsStatic = reader.GetBoolean(7),
                            IsConstructor = reader.GetBoolean(8),
                            IsMethod = !reader.GetBoolean(8),
                            IsProperty = false,
                            Access = ((AccessSpecifier)reader.GetInt32(4))
                        });
                    }
                }
            }
            catch (Exception exception)
            {
                HandleException(exception);
            }
        }

        #endregion

        #region Private SQL Statement Helper Methods

        private string CreateTableStatement(string tableName, string[] fields)
        {
            StringBuilder content = new StringBuilder();
            foreach (string field in fields)
            {
                if (content.Length > 0)
                    content.Append(',');
                content.Append(field);
            }

            return string.Format("CREATE TABLE {0}({1});", tableName, content.ToString());
        }

        private string InsertIntoStatement(string tableName, string[] fields, string[] values)
        {
            if (null == fields || (fields.Length <= 0))
                throw new ArgumentNullException("fields");
            if (null == values || (values.Length <= 0))
                throw new ArgumentNullException("values");
            if (fields.Length != values.Length)
                throw new ArgumentException("'fields' and 'values' must be of the same length!");

            StringBuilder fieldString = new StringBuilder();
            foreach (string field in fields)
            {
                if (fieldString.Length > 0)
                    fieldString.Append(",");
                fieldString.Append(field);
            }

            StringBuilder valueString = new StringBuilder();
            foreach (string value in values)
            {
                if (valueString.Length > 0)
                    valueString.Append(",");
                valueString.Append(value);
            }

            return string.Format("INSERT INTO {0}({1}) VALUES({2});",
                tableName, fieldString, valueString);
        }

        private string SelectFromStatement(string tableName, string[] conditions)
        {
            StringBuilder content = new StringBuilder();
            content.Append(string.Format("SELECT * FROM {0}", tableName));

            if (null != conditions && (conditions.Length > 0))
            {
                bool firstEntry = true;
                content.Append(" WHERE ");
                foreach (string condition in conditions)
                {
                    if (false == firstEntry)
                        content.Append(" AND ");

                    content.Append(condition);
                    firstEntry = false;
                }
            }

            content.Append(";");
            return content.ToString();
        }

        private string SelectJoinStatement(string leftTable,
            string rightTable, string[] joints, string[] conditions)
        {
            StringBuilder content = new StringBuilder();
            content.Append(string.Format("SELECT * FROM {0} INNER JOIN {1}", leftTable, rightTable));

            if (null != joints && (joints.Length > 0))
            {
                bool firstEntry = true;
                content.Append(" ON ");
                foreach (string join in joints)
                {
                    if (false == firstEntry)
                        content.Append(" AND ");

                    content.Append(join);
                    firstEntry = false;
                }
            }

            if (null != conditions && (conditions.Length > 0))
            {
                bool firstEntry = true;
                content.Append(" WHERE ");
                foreach (string condition in conditions)
                {
                    if (false == firstEntry)
                        content.Append(" AND ");

                    content.Append(condition);
                    firstEntry = false;
                }
            }

            content.Append(";");
            return content.ToString();
        }

        #endregion

        #region Private Class Helper Methods

        private void DefineHierarchy(int parent, int child, bool isClass)
        {
            if (null == hierarchy)
                hierarchy = new List<ParentChildPair>();

            hierarchy.Add(new ParentChildPair(parent, child, isClass));
        }

        private bool ProcedureToBeExcluded(ProcedureNode procedureNode)
        {
            if (null == procedureNode)
                return true;

            bool autoGenerated =
                procedureNode.isAutoGenerated ||
                procedureNode.isAutoGeneratedThisProc ||
                procedureNode.isAssocOperator;

            if (autoGenerated)
                return true;
            if (string.IsNullOrEmpty(procedureNode.name))
                return true;
            if (procedureNode.name.StartsWith("%"))
                return true;

            return false;
        }

        private bool SymbolToBeExcluded(SymbolNode symbolNode)
        {
            if (null == symbolNode || (symbolNode.isTemp))
                return true; // Exclude temporary variables.
            if (string.IsNullOrEmpty(symbolNode.name))
                return true;
            if (symbolNode.name[0] == '%')
                return true;

            return false;
        }

        private string GetDatabaseType(string value)
        {
            return string.Format("'{0}'", value);
        }

        private string GetDatabaseType(bool value)
        {
            return value ? "1" : "0";
        }

        private int GetClassScopeFromName(string className, ref bool isImported)
        {
            isImported = false;

            string[] conditions =
            {
                "Name = " + GetDatabaseType(className)
            };

            try
            {
                string statement = SelectFromStatement(TableNames.Classes, conditions);
                SQLiteCommand command = new SQLiteCommand(statement, connection);
                SQLiteDataReader reader = command.ExecuteReader();
                if (null != reader && (false != reader.HasRows))
                {
                    if (reader.Read()) // Read in the only record.
                    {
                        isImported = reader.GetBoolean(2); // "IsImported" field.
                        return (reader.GetInt32(0)); // "ClassScope" field.
                    }
                }
            }
            catch (Exception exception)
            {
                HandleException(exception);
            }

            return ProtoCore.DSASM.Constants.kInvalidIndex; // Invalid type.
        }

        private int GetScriptIdentifier(string scriptPath)
        {
            if (string.IsNullOrEmpty(scriptPath))
                return -1;

            if (null == scriptIdentifiers)
                scriptIdentifiers = new Dictionary<string, int>();

            scriptPath = scriptPath.ToLower();
            if (scriptIdentifiers.ContainsKey(scriptPath))
                return scriptIdentifiers[scriptPath];

            int identifier = scriptIdentifiers.Count;
            scriptIdentifiers[scriptPath] = identifier;
            return identifier;
        }

        private string GetScriptPath(int identifier)
        {
            if (null == scriptIdentifiers || (scriptIdentifiers.Count <= 0))
                return string.Empty;

            foreach (var scriptIdentifier in scriptIdentifiers)
            {
                if (scriptIdentifier.Value == identifier)
                    return scriptIdentifier.Key;
            }

            return string.Empty; // Script not found!
        }

        private int GetParentBlock(int identifier)
        {
            if (null == hierarchy)
                return ProtoCore.DSASM.Constants.kInvalidIndex;

            foreach (ParentChildPair pair in hierarchy)
            {
                if (pair.IsClass && (identifier != pair.Child))
                    continue;

                return pair.Parent;
            }

            return ProtoCore.DSASM.Constants.kInvalidIndex;
        }

        private List<int> GetAllBaseClasses(int classScope)
        {
            List<int> classesToExamine = new List<int>();
            classesToExamine.Add(classScope);

            int currentIndex = 0;
            while (currentIndex < classesToExamine.Count)
            {
                int currentClass = classesToExamine[currentIndex++];
                List<int> parentClasses = GetParentClasses(currentClass);
                if (null != parentClasses && (parentClasses.Count > 0))
                    classesToExamine.AddRange(parentClasses);
            }

            List<int> results = new List<int>();
            foreach (int parent in classesToExamine)
            {
                if (results.Contains(parent) == false)
                    results.Add(parent);
            }

            return results;
        }

        private List<int> GetParentClasses(int identifier)
        {
            if (null == hierarchy)
                return null;

            List<int> parentClasses = null;
            foreach (ParentChildPair pair in hierarchy)
            {
                if (pair.IsClass && (identifier == pair.Child))
                {
                    if (null == parentClasses)
                        parentClasses = new List<int>();

                    parentClasses.Add(pair.Parent);
                }
            }

            return parentClasses;
        }

        private int GetClassMemberType(ScopeInfo scopeIdentifier, string identifier, bool isStatic)
        {
            // The identifier is within a class body, having a name 
            // "this" would have made its type the same as the class.
            if (identifier == "this")
                return scopeIdentifier.ClassScope;

            List<int> classesToSearch = GetAllBaseClasses(scopeIdentifier.ClassScope);
            while (classesToSearch.Count > 0)
            {
                int currentClass = classesToSearch[0];
                classesToSearch.RemoveAt(0);

                string[] conditions =
                {
                    "Name = " + GetDatabaseType(identifier),
                    "CodeBlock = " + scopeIdentifier.BlockId.ToString(),
                    "ClassScope = " + currentClass.ToString(),
                    "Procedure = " + scopeIdentifier.FunctionIndex.ToString(),
                    "IsStatic = " + GetDatabaseType(isStatic)
                };

                try
                {
                    string statement = SelectFromStatement(TableNames.Symbols, conditions);
                    SQLiteCommand command = new SQLiteCommand(statement, connection);
                    SQLiteDataReader reader = command.ExecuteReader();
                    if (null != reader && (false != reader.HasRows))
                    {
                        if (reader.Read()) // Read in the only record.
                        {
                            int dataType = reader.GetInt32(5); // "DataType" field.
                            int staticType = reader.GetInt32(6); // "StaticType" field.
                            return GetSymbolType(staticType, dataType);
                        }
                    }
                }
                catch (Exception exception)
                {
                    HandleException(exception);
                }

                // The procedure ID is only applicable to the current class, as we 
                // proceed to check in base classes, we will not need the procedure 
                // ID as it won't be applicable there (even if another procedure with 
                // the same ID exists in that parent class). So we'll just set it to 
                // an invalid value. The same rule applies to a block (it is possible 
                // to have a block embedded within a class member function, in which 
                // case the block will not be zero).
                // 
                scopeIdentifier.BlockId = 0;
                scopeIdentifier.FunctionIndex = ProtoCore.DSASM.Constants.kInvalidIndex;
            }

            return ProtoCore.DSASM.Constants.kInvalidIndex; // Invalid type.
        }

        private int GetBlockMemberType(ScopeInfo scopeIdentifier, string identifier)
        {
            int currentBlock = scopeIdentifier.BlockId;
            while (currentBlock != ProtoCore.DSASM.Constants.kInvalidIndex)
            {
                string[] conditions =
                {
                    "Name = " + GetDatabaseType(identifier),
                    "CodeBlock = " + currentBlock.ToString(),
                    "ClassScope = " + scopeIdentifier.ClassScope.ToString(),
                    "Procedure = " + scopeIdentifier.FunctionIndex.ToString()
                };

                try
                {
                    string statement = SelectFromStatement(TableNames.Symbols, conditions);
                    SQLiteCommand command = new SQLiteCommand(statement, connection);
                    SQLiteDataReader reader = command.ExecuteReader();
                    if (null != reader && (false != reader.HasRows))
                    {
                        if (reader.Read()) // Read in the only record.
                        {
                            int dataType = reader.GetInt32(5); // "DataType" field.
                            int staticType = reader.GetInt32(6); // "StaticType" field.
                            return GetSymbolType(staticType, dataType);
                        }
                    }
                }
                catch (Exception exception)
                {
                    HandleException(exception);
                }

                // Nothing found, go up to the parent block.
                currentBlock = GetParentBlock(currentBlock);
            }

            return ProtoCore.DSASM.Constants.kInvalidIndex; // Invalid type.
        }

        private int GetSymbolType(int staticType, int dataType)
        {
            if (staticType == ((int)ProtoCore.PrimitiveType.kTypeVar))
                return dataType; // A "var" has a "runtime" type, use it.

            return staticType;
        }

        private void HandleException(Exception exception)
        {
            this.ErrorMessage = string.Empty;
            if (exception is SQLiteException)
                this.ErrorMessage += "SQLiteException: ";

            this.ErrorMessage += exception.Message;
        }

        #endregion
    }
}
