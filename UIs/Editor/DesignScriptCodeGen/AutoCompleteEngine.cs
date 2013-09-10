using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoCore.CodeModel;
using System.ComponentModel;
using System.Windows.Threading;
using DesignScript.Editor.CodeGen;
using System.IO;
using System.Data.SQLite;

namespace DesignScript.Editor.AutoComplete
{
    #region Public Classes/Interfaces

    public class CompletionItem
    {
        MemberType memberType = MemberType.Private | MemberType.Property;

        public enum MemberType
        {
            Invalid = 0x0000,
            Static = 0x0001,
            Public = 0x0002,
            Protected = 0x0004,
            Private = 0x0008,
            FromBase = 0x0010,
            Constructor = 0x0020,
            Method = 0x0040,
            Property = 0x0080,
            External = 0x0100,
        }

        public CompletionItem(string name)
        {
            this.Name = name;
        }

        #region Public Class Properties

        public string Name { get; private set; }
        public MemberType Type { get { return memberType; } }

        public bool IsStatic
        {
            get { return memberType.HasFlag(MemberType.Static); }
            set { AddOrRemoveFlag(MemberType.Static, value); }
        }

        public bool IsConstructor
        {
            get { return memberType.HasFlag(MemberType.Constructor); }
            set { AddOrRemoveFlag(MemberType.Constructor, value); }
        }

        public bool IsMethod
        {
            get { return memberType.HasFlag(MemberType.Method); }
            set { AddOrRemoveFlag(MemberType.Method, value); }
        }

        public bool IsProperty
        {
            get { return memberType.HasFlag(MemberType.Property); }
            set { AddOrRemoveFlag(MemberType.Property, value); }
        }

        public bool IsExternal
        {
            get { return memberType.HasFlag(MemberType.External); }
            set { AddOrRemoveFlag(MemberType.External, value); }
        }

        public ProtoCore.DSASM.AccessSpecifier Access
        {
            get
            {
                if (memberType.HasFlag(MemberType.Public))
                    return ProtoCore.DSASM.AccessSpecifier.kPublic;
                else if (memberType.HasFlag(MemberType.Protected))
                    return ProtoCore.DSASM.AccessSpecifier.kProtected;
                return ProtoCore.DSASM.AccessSpecifier.kPrivate;
            }

            set
            {
                // Clear existing flags...
                memberType &= ~MemberType.Public;
                memberType &= ~MemberType.Protected;
                memberType &= ~MemberType.Private;

                switch (value)
                {
                    case ProtoCore.DSASM.AccessSpecifier.kPublic:
                        memberType |= MemberType.Public;
                        break;
                    case ProtoCore.DSASM.AccessSpecifier.kProtected:
                        memberType |= MemberType.Protected;
                        break;
                    case ProtoCore.DSASM.AccessSpecifier.kPrivate:
                        memberType |= MemberType.Private;
                        break;
                }
            }
        }

        #endregion

        #region Private Class Helper Methods

        private void AddOrRemoveFlag(MemberType flag, bool add)
        {
            memberType &= ~flag;
            memberType |= (add ? flag : 0);
        }

        #endregion
    }

    public class MethodSignature : CompletionItem
    {
        private List<string> arguments = null;

        #region Public Class Operational Methods

        public MethodSignature(string name)
            : base(name)
        {
            this.IsProperty = false;
            this.IsExternal = false;
            this.ReturnType = string.Empty;
        }

        internal void AddArgument(string name, string type, string defValue)
        {
            if (null == arguments)
                arguments = new List<string>();

            if (string.IsNullOrEmpty(defValue))
                arguments.Add(string.Format("{0} : {1}", name, type));
            else
                arguments.Add(string.Format("{0} : {1} = {2}", name, type, defValue));
        }

        public override string ToString()
        {
            StringBuilder content = new StringBuilder();
            if (this.IsExternal) content.Append("external ");
            if (this.IsConstructor)
                content.AppendFormat("constructor {0} (", this.Name);
            else
            {
                if (string.IsNullOrEmpty(this.ReturnType))
                    content.AppendFormat("def {0}(", this.Name);
                else
                    content.AppendFormat("def {0} : {1}(", this.Name, this.ReturnType);
            }

            if (null != arguments)
            {
                bool firstEntry = true;
                foreach (string argument in arguments)
                {
                    if (false == firstEntry)
                        content.Append(", ");

                    content.Append(argument);
                    firstEntry = false;
                }
            }

            content.Append(")");
            return content.ToString();
        }

        #endregion

        #region Public Class Properties

        public int Arguments
        {
            get { return ((null == arguments) ? 0 : arguments.Count); }
        }

        public string ReturnType { get; set; }

        #endregion
    }

    public interface IAutoCompleteEngine
    {
        void UpdateDatabase(string codeSnapshot, string scriptPath);
        List<CompletionItem> GetCompletionList(CodePoint location, string idents);
        List<MethodSignature> GetMethodSignatures(CodePoint location, string idents);

        bool HandleNextUpdateRequest { set; }
    }

    public class ClassFactory
    {
        public static IAutoCompleteEngine CreateAutoCompleteEngine()
        {
            if (null != CoreCodeGen.AutoCompleteEngine)
            {
                string message = "'CreateAutoCompleteEngine' called twice!";
                throw new InvalidOperationException(message);
            }

            CoreCodeGen.AutoCompleteEngine = new AutoCompleteEngine();
            return CoreCodeGen.AutoCompleteEngine;
        }
    }

    #endregion

    #region Internal Implementation Class

    class AutoCompleteWorkData
    {
        public AutoCompleteWorkData(string scriptPath, string codeSnapshot)
        {
            if (string.IsNullOrEmpty(scriptPath))
                scriptPath = string.Empty;

            this.ScriptPath = scriptPath;
            this.CodeSnapshot = codeSnapshot;
        }

        public string ScriptPath { get; private set; }
        public string CodeSnapshot { get; private set; }
    }

    class AutoCompleteEngine : IAutoCompleteEngine
    {
        #region Private Class Data Members

        object updateSyncRoot = new object();
        object promotionSyncRoot = new object();
        DispatcherTimer updateTrigger = null;
        BackgroundWorker updateWorker = null;

        AutoCompleteWorkData workData = null;
        SymbolDatabaseProxy stagingDatabase = null;
        SymbolDatabaseProxy productionDatabase = null;
        Dictionary<ScopeInfo, CodeRange> scopeIdentifiers = null;

        #endregion

        #region Public Class Interface Methods/Properties

        // IMPORTANT: This method is to be called only from UI thread.
        public void UpdateDatabase(string codeSnapshot, string scriptPath)
        {
            if (string.IsNullOrEmpty(codeSnapshot))
                return;

            // "AutoCompleteEngine.UpdateDatabase" method is called by background
            // parser of the IDE, when the code does not have any syntactical 
            // errors. The interval of that happening is around 200ms as of this 
            // comment, and user could continue typing as the background parser 
            // is working. That leads to this method being called very frequently,
            // but we do not want to process each and every call of this method.
            // Instead, we rely on a timer that expires few moments after this 
            // method is called, ignoring all the prior calls of the method that 
            // may happen too frequently.
            // 
            if (null == updateTrigger)
            {
                updateTrigger = new DispatcherTimer();
                updateTrigger.Tick += new EventHandler(OnUpdateTriggered);
                updateTrigger.Interval = TimeSpan.FromMilliseconds(2000);
            }

            if ((null != updateWorker) && updateWorker.IsBusy)
                return; // The database is being updated right now.

            // The background worker isn't busy, so take a snapshot of 
            // the code and then process it later when the timer expires.
            this.workData = new AutoCompleteWorkData(scriptPath, codeSnapshot);

            updateTrigger.Stop();
            if (false != HandleNextUpdateRequest)
            {
                // In the event of user pressing keys like semi-colon (;) or 
                // closing curly bracket (}), "HandleNextUpdateRequest" will be 
                // set to "true", in which case the "UpdateDatabase" call should
                // be handled immediately without waiting for a timer to expire.
                // 
                HandleNextUpdateRequest = false;
                UpdateDatabaseInternal();
            }
            else
            {
                // No immediate update is requested, we should start processing 
                // after a few moment (just in case the next update comes along 
                // before the timer expires, we will stop the timer and restart
                // it).
                // 
                updateTrigger.Start();
            }
        }

        public List<CompletionItem> GetCompletionList(CodePoint location, string idents)
        {
            lock (promotionSyncRoot) // Block until progressing promotion is done.
            {
                if (null != productionDatabase)
                    return productionDatabase.GetCompletionList(location, idents);

                return null;
            }
        }

        public List<MethodSignature> GetMethodSignatures(CodePoint location, string idents)
        {
            lock (promotionSyncRoot)
            {
                if (null != productionDatabase)
                    return productionDatabase.GetMethodSignatures(location, idents);

                return null;
            }
        }

        public bool HandleNextUpdateRequest { get; set; }

        public ProtoCore.IOutputStream MessageHandler { get; set; }

        #endregion

        #region Public Class Operational Methods

        internal void AddScopeIdentifier(int blockId, int classId, int procId,
            int startLine, int startColumn, int endLine, int endColumn, string scriptPath)
        {
            // Generated methods, built-in functions, class getters/setters
            // have no valid source location information, skip processing them.
            if (startLine < 0 || (startColumn < 0) || endLine < 0 || (endColumn < 0))
                return;

            if (null == scopeIdentifiers)
                scopeIdentifiers = new Dictionary<ScopeInfo, CodeRange>();

            ScopeInfo scope = new ScopeInfo()
            {
                BlockId = blockId,
                ClassScope = classId,
                FunctionIndex = procId
            };

            if (scopeIdentifiers.ContainsKey(scope))
                throw new InvalidOperationException("No two scopes should be identical!");

            scopeIdentifiers[scope] = SymbolDatabaseProxy.MakeCodeRange(
                startLine, startColumn, endLine, endColumn, scriptPath);
        }

        #endregion

        #region Private Event Handlers

        private void OnUpdateTriggered(object sender, EventArgs e)
        {
            updateTrigger.Stop();
            UpdateDatabaseInternal();
        }

        private void OnUpdateDatabaseDoWork(object sender, DoWorkEventArgs e)
        {
            lock (updateSyncRoot)
            {
                AutoCompleteWorkData workData = e.Argument as AutoCompleteWorkData;

                // At the beginning of build and database population, assume
                // that at any point if we return earlier, the operation is 
                // considered a failure, and no further database promotion 
                // should be done.
                e.Result = false;

                ProtoLanguage.CompileStateTracker compileState = CompileCodeSnapshot(workData);
                if (null != compileState)
                {
                    stagingDatabase = new SymbolDatabaseProxy(compileState, scopeIdentifiers);
                    e.Result = true;
                }
            }
        }

        private void OnUpdateDatabaseCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            bool updateSucceeded = ((bool)e.Result);
            if (false == updateSucceeded)
                return; // Update has failed.

            PromoteStagingDatabase();
        }

        #endregion

        #region Private Class Helper Methods

        private void UpdateDatabaseInternal()
        {
            // This method can be called from two cases: UpdateDatabase() and 
            // OnUpdateTriggered(). These two methods are always guaranteed to 
            // be executed on the main UI thread, so this method is also 
            // executed on the main UI thread.
            // 
            if (null == updateWorker)
            {
                updateWorker = new BackgroundWorker();
                updateWorker.DoWork += new DoWorkEventHandler(OnUpdateDatabaseDoWork);
                updateWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(OnUpdateDatabaseCompleted);
            }

            if (false != updateWorker.IsBusy)
                return;

            // If there's any promotion going on (from staging database to 
            // production database), then wait for a little while, promotion 
            // should be a speedy operation with just a pointer assignment.
            // 
            lock (promotionSyncRoot)
            {
                if (null != stagingDatabase)
                {
                    // Staging database should be invalidated after a promotion.
                    string message = "Staging database should be closed after promotion!";
                    throw new InvalidOperationException(message);
                }

                // The background update worker is not actively updating the 
                // database right now, so make it take on the work immediately.
                updateWorker.RunWorkerAsync(this.workData);
            }
        }

        private ProtoLanguage.CompileStateTracker CompileCodeSnapshot(AutoCompleteWorkData workData)
        {
            if (null != this.scopeIdentifiers)
            {
                this.scopeIdentifiers.Clear();
                this.scopeIdentifiers = null;
            }

            ProtoLanguage.CompileOptions options = new ProtoLanguage.CompileOptions();
            options.RootModulePathName = workData.ScriptPath;
            ProtoLanguage.CompileStateTracker compileState = new ProtoLanguage.CompileStateTracker(options);

            compileState.CurrentDSFileName = workData.ScriptPath;
            ProtoFFI.DLLFFIHandler.Register(ProtoFFI.FFILanguage.CSharp, new ProtoFFI.CSModuleHelper());

            // Register a message stream if we do have one.
            if (null != this.MessageHandler)
                compileState.BuildStatus.MessageHandler = this.MessageHandler;

            MemoryStream stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(workData.CodeSnapshot));
            ProtoCore.DesignScriptParser.Scanner s = new ProtoCore.DesignScriptParser.Scanner(stream);
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
                compileState = null;
            }
            finally
            {
                // Do necessary clean-up here.
            }

            return compileState;
        }

        private void PromoteStagingDatabase()
        {
            // This method promotes the staging database connection to a 
            // production database connection (closing out the previous one 
            // if one exists).
            // 
            lock (promotionSyncRoot)
            {
                if (null != productionDatabase)
                {
                    productionDatabase.CloseConnection();
                    productionDatabase = null;
                }

                productionDatabase = stagingDatabase;
                stagingDatabase = null;
            }
        }

        #endregion
    }

    #endregion
}
