

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using ProtoCore.Exceptions;
using System.Timers;
using ProtoCore.DSASM;
using ProtoCore.Utils;
using ProtoFFI;
using ProtoCore;
using Autodesk.DesignScript.Interfaces;

namespace ProtoLanguage
{
    public enum ExecutionMode
    {
        Parallel,
        Serial
    }

    public class CompileOptions
    {
        public CompileOptions()
        {
            DumpByteCode = false;
            Verbose = false;

            FullSSA = false;
            DumpIL = false;

            DumpFunctionResolverLogic = false;
            DumpOperatorToMethodByteCode = false;
            SuppressBuildOutput = false;
            BuildOptWarningAsError = false;
            BuildOptErrorAsWarning = false;
            ExecutionMode = ProtoLanguage.ExecutionMode.Serial;
            IDEDebugMode = false;
            WatchTestMode = false;
            IncludeDirectories = new List<string>();

            // defaults to 6 decimal places
            //
            FormatToPrintFloatingPoints = "F6";
            RootCustomPropertyFilterPathName = @"C:\arxapiharness\Bin\AcDesignScript\CustomPropertyFilter.txt";
            CompileToLib = false;
            AssocOperatorAsMethod = true;

            EnableProcNodeSanityCheck = true;
            EnableReturnTypeCheck = true;

            RootModulePathName = Path.GetFullPath(@".");
            staticCycleCheck = true;
            dynamicCycleCheck = true;
            RecursionChecking = false;
            EmitBreakpoints = true;

            localDependsOnGlobalSet = false;
            LHSGraphNodeUpdate = true;
            TempReplicationGuideEmptyFlag = true;
            AssociativeToImperativePropagation = true;
            SuppressFunctionResolutionWarning = true;
            EnableVariableAccumulator = true;
            WebRunner = false;
            DisableDisposeFunctionDebug = true;
            GenerateExprID = true;
            IsDeltaExecution = false;
            ElementBasedArrayUpdate = true;

        }


        public bool DumpByteCode { get; set; }
        public bool DumpIL { get; private set; }
        public bool FullSSA { get; set; }
        public bool Verbose { get; set; }
        public bool DumpOperatorToMethodByteCode { get; set; }
        public bool SuppressBuildOutput { get; set; }
        public bool BuildOptWarningAsError { get; set; }
        public bool BuildOptErrorAsWarning { get; set; }
        public bool IDEDebugMode { get; set; }      //set to true if two way mapping b/w DesignScript and JIL code is needed
        public bool WatchTestMode { get; set; }     // set to true when running automation tests for expression interpreter
        public ExecutionMode ExecutionMode { get; set; }
        public string FormatToPrintFloatingPoints { get; set; }
        public bool CompileToLib { get; set; }
        public bool AssocOperatorAsMethod { get; set; }
        public string LibPath { get; set; }
        public bool staticCycleCheck { get; set; }
        public bool dynamicCycleCheck { get; set; }
        public bool RecursionChecking { get; set; }
        public bool DumpFunctionResolverLogic { get; set; }
        public bool EmitBreakpoints { get; set; }
        public bool localDependsOnGlobalSet { get; set; }
        public bool LHSGraphNodeUpdate { get; set; }
        public bool SuppressFunctionResolutionWarning { get; set; }
        public bool WebRunner { get; set; }

        public bool TempReplicationGuideEmptyFlag { get; set; }
        public bool AssociativeToImperativePropagation { get; set; }
        public bool EnableVariableAccumulator { get; set; }
        public bool DisableDisposeFunctionDebug { get; set; }
        public bool GenerateExprID { get; set; }
        public bool IsDeltaExecution { get; set; }
        public bool ElementBasedArrayUpdate { get; set; }


        // This is being moved to Core.Options as this needs to be overridden for the Watch test framework runner        
        public int kDynamicCycleThreshold = 2000;

        public double Tolerance
        {
            get { return ProtoCore.Utils.MathUtils.Tolerance; }
            set { ProtoCore.Utils.MathUtils.Tolerance = value; }
        }

        public List<string> IncludeDirectories { get; set; }
        public string RootModulePathName { get; set; }

        private string rootCustomPropertyFilterPathName;
        public string RootCustomPropertyFilterPathName
        {
            get
            {
                return rootCustomPropertyFilterPathName;
            }
            set
            {
                if (value == null)
                {
                    rootCustomPropertyFilterPathName = null;
                }
                else
                {
                    var fileName = value;
                    if (System.IO.File.Exists(fileName))
                    {
                        rootCustomPropertyFilterPathName = fileName;

                        System.IO.StreamReader stream = null;
                        try
                        {
                            stream = new System.IO.StreamReader(fileName);
                        }
                        catch (System.Exception ex)
                        {
                            throw new System.IO.FileLoadException(string.Format("Custom property filter file {0} can't be read. Error Message:{1}", fileName, ex.Message));
                        }
                        finally
                        {
                            if (stream != null)
                            {
                                stream.Dispose();
                            }
                        }
                    }
                    else
                    {
                        //throw new System.IO.FileNotFoundException(string.Format("Custom property filter file {0} does not exists", fileName));
                        rootCustomPropertyFilterPathName = null;
                    }
                }
            }
        }

        public bool EnableReturnTypeCheck { get; set; }

        public bool EnableProcNodeSanityCheck { get; set; }

    }

    public class CompileStateTracker
    {
        public const int FIRST_CORE_ID = 0;

        public int ID { get; private set; }

        public bool compileSucceeded { get; set; }

        //recurtion
        public List<FunctionCounter> recursivePoint { get; set; }
        public List<FunctionCounter> funcCounterTable { get; set; }
        public bool calledInFunction;
        
        // This flag is set true when we call GraphUtilities.PreloadAssembly to load libraries in Graph UI
        public bool IsParsingPreloadedAssembly { get; set; }
        
        // THe ImportModuleHandler owned by the temporary core used in Graph UI precompilation
        // needed to detect if the same assembly is not being imported more than once
        public ProtoFFI.ImportModuleHandler ImportHandler { get; set; }
        
        // This is set to true when the temporary core is used for precompilation of CBN's in GraphUI
        public bool IsParsingCodeBlockNode { get; set; }

        // This is the AST node list of default imported libraries needed for Graph Compiler
        public ProtoCore.AST.AssociativeAST.CodeBlockNode ImportNodes { get; set; }

        // The root AST node obtained from parsing an expression in a Graph node in GraphUI
        public List<ProtoCore.AST.Node> AstNodeList { get; set; }

        public enum ErrorType
        {
            OK,
            Error,
            Warning
        }

        public struct ErrorEntry
        {
            public ProtoLanguage.CompileStateTracker.ErrorType Type;
            public string FileName;
            public string Message;
            public ProtoCore.BuildData.WarningID BuildId;
            public ProtoCore.RuntimeData.WarningID RuntimeId;
            public int Line;
            public int Col;
        }

        public Dictionary<ulong, ulong> codeToLocation = new Dictionary<ulong, ulong>();
        public Dictionary<ulong, ErrorEntry> LocationErrorMap = new Dictionary<ulong, ErrorEntry>();

        //STop
        public Stopwatch StopWatch;
        public void StartTimer()
        {
            StopWatch = new Stopwatch();
            StopWatch.Start();
        }
        public TimeSpan GetCurrentTime()
        {
            TimeSpan ts = StopWatch.Elapsed;
            return ts;
        }

        public ProtoCore.Lang.FunctionTable FunctionTable { get; set; }

        public Script Script { get; set; }
        public LangVerify Langverify = new LangVerify();
        public Dictionary<Language, ProtoCore.Executive> Executives { get; private set; }

        public ProtoCore.Executive CurrentExecutive { get; private set; }
        public Stack<ExceptionRegistration> stackActiveExceptionRegistration { get; set; }
        public int GlobOffset { get; set; }
        public int GlobHeapOffset { get; set; }
        public int BaseOffset { get; set; }
        public int GraphNodeUID { get; set; }

        public Heap Heap { get; set; }
        public ProtoCore.Runtime.RuntimeMemory Rmem { get; set; }

        public int ClassIndex { get; set; }     // Holds the current class scope
        public int RunningBlock { get; set; }
        public int CodeBlockIndex { get; set; }
        public int RuntimeTableIndex { get; set; }
        public List<CodeBlock> CodeBlockList { get; set; }
        // The Complete Code Block list contains all the code blocks
        // unlike the codeblocklist which only stores the outer most code blocks
        public List<CodeBlock> CompleteCodeBlockList { get; set; }
        public Executable DSExecutable { get; set; }

        public List<Instruction> Breakpoints { get; set; }

        public CompileOptions Options { get; private set; }
        public BuildStatus BuildStatus { get; private set; }

        public TypeSystem TypeSystem { get; set; }

        // The global class table and function tables
        public ClassTable ClassTable { get; set; }
        public ProcedureTable ProcTable { get; set; }
        public ProcedureNode ProcNode { get; set; }

        // The function pointer table
        public FunctionPointerTable FunctionPointerTable { get; set; }

        //The dynamic string table and function table
        public DynamicVariableTable DynamicVariableTable { get; set; }
        public DynamicFunctionTable DynamicFunctionTable { get; set; }

        public IExecutiveProvider ExecutiveProvider { get; set; }

        public Dictionary<string, object> Configurations { get; set; }

        //Manages injected context data.
        public ContextDataManager ContextDataManager { get; set; }

        public ParseMode ParsingMode { get; set; }

        public FFIPropertyChangedMonitor FFIPropertyChangedMonitor { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void AddContextData(Dictionary<string, Object> data)
        {
            if (data == null)
                return;

            ContextDataManager.GetInstance(this).AddData(data);
        }


        // Cached replication guides for the current call. 
        // TODO Jun: Store this in the dynamic table node
        public List<List<int>> replicationGuides;

        // if CompileToLib is true, this is used to output the asm instruction to the dsASM file
        // if CompilerToLib is false, this will be set to Console.Out
        public TextWriter AsmOutput;
        public int AsmOutputIdents;

        public string CurrentDSFileName { get; set; }
        // this field is used to store the inferedtype information  when the code gen cross one langeage to another 
        // otherwize the inferedtype information will be lost
        public ProtoCore.Type InferedType;

        public DebugProperties DebugProps;
        
        //public Stack<List<ProtoCore.AssociativeGraph.GraphNode>> stackNodeExecutedSameTimes { get; set; }
        //public Stack<AssociativeGraph.GraphNode> stackExecutingGraphNodes { get; set; }
        public Stack<InterpreterProperties> InterpreterProps { get; set; }

        // Continuation properties used for Serial mode execution and Debugging of Replicated calls
        public ProtoCore.Lang.ContinuationStructure ContinuationStruct { get; set; }

        /// <summary>
        /// Gets the reason why the execution was last suspended
        /// </summary>
        public ReasonForExecutionSuspend ReasonForExecutionSuspend { get; internal set; }

        public delegate void DisposeDelegate(ProtoLanguage.CompileStateTracker sender);
        public event DisposeDelegate Dispose;
        public event EventHandler<ExecutionStateEventArgs> ExecutionEvent;


        public int ExecutionState { get; set; }

        public bool builtInsLoaded { get; set; }
        public List<string> LoadedDLLs = new List<string>();
        public int deltaCompileStartPC { get; set; }

        public void LogErrorInGlobalMap(ProtoLanguage.CompileStateTracker.ErrorType type, string msg, string fileName = null, int line = -1, int col = -1,
            ProtoCore.BuildData.WarningID buildId = ProtoCore.BuildData.WarningID.kDefault, ProtoCore.RuntimeData.WarningID runtimeId = ProtoCore.RuntimeData.WarningID.kDefault)
        {
            ulong location = (((ulong)line) << 32 | ((uint)col));
            ProtoLanguage.CompileStateTracker.ErrorEntry newError = new ProtoLanguage.CompileStateTracker.ErrorEntry
            {
                Type = type,
                FileName = fileName,
                Message = msg,
                Line = line,
                Col = col,
                BuildId = buildId,
                RuntimeId = runtimeId
            };

            if (this.LocationErrorMap.ContainsKey(location))
            {
                ProtoLanguage.CompileStateTracker.ErrorEntry error = this.LocationErrorMap[location];

                // If there is a warning, replace it with an error
                if (error.Type == ProtoLanguage.CompileStateTracker.ErrorType.Warning && type == ProtoLanguage.CompileStateTracker.ErrorType.Error)
                {
                    this.LocationErrorMap[location] = newError;
                }
            }
            else
            {
                this.LocationErrorMap.Add(location, newError);
            }
        }

        public void NotifyExecutionEvent(ExecutionStateEventArgs.State state)
        {
            switch (state)
            {
                case ExecutionStateEventArgs.State.kExecutionBegin:
                    Validity.Assert(ExecutionState == (int)ExecutionStateEventArgs.State.kInvalid, "Invalid Execution state being notified.");
                    break;
                case ExecutionStateEventArgs.State.kExecutionEnd:
                    if (ExecutionState == (int)ExecutionStateEventArgs.State.kInvalid) //execution never begun.
                        return;
                    break;
                case ExecutionStateEventArgs.State.kExecutionBreak:
                    Validity.Assert(ExecutionState == (int)ExecutionStateEventArgs.State.kExecutionBegin || ExecutionState == (int)ExecutionStateEventArgs.State.kExecutionResume, "Invalid Execution state being notified.");
                    break;
                case ExecutionStateEventArgs.State.kExecutionResume:
                    Validity.Assert(ExecutionState == (int)ExecutionStateEventArgs.State.kExecutionBreak, "Invalid Execution state being notified.");
                    break;
                default:
                    Validity.Assert(false, "Invalid Execution state being notified.");
                    break;
            }
            ExecutionState = (int)state;
            if (null != ExecutionEvent)
                ExecutionEvent(this, new ExecutionStateEventArgs(state));
        }

        public class CodeBlockCompilationSnapshot
        {
            public CodeBlockCompilationSnapshot(int codeBlocKId, int graphNodeCount, int endPC)
            {
                CodeBlockId = codeBlocKId;
                GraphNodeCount = graphNodeCount;
                InstructionCount = endPC;
            }

            public static List<CodeBlockCompilationSnapshot> CaptureCoreCompileState(Core core)
            {
                List<CodeBlockCompilationSnapshot> snapShots = new List<CodeBlockCompilationSnapshot>();
                if (core.CodeBlockList != null)
                {
                    foreach (var codeBlock in core.CodeBlockList)
                    {
                        int codeBlockId = codeBlock.codeBlockId;
                        InstructionStream istream = core.CodeBlockList[codeBlockId].instrStream;
                        int graphCount = istream.dependencyGraph.GraphList.Count;
                        int instructionCount = istream.instrList.Count;

                        snapShots.Add(new CodeBlockCompilationSnapshot(codeBlockId, graphCount, instructionCount));
                    }
                }
                return snapShots;
            }

            public int CodeBlockId { get; set;} 
            public int GraphNodeCount { get; set;} 
            public int InstructionCount { get; set;}
        }

        public void ResetDeltaCompileFromSnapshot(List<CodeBlockCompilationSnapshot> snapShots)
        {
            if (snapShots == null)
                throw new ArgumentNullException("snapshots");

            foreach (var snapShot in snapShots)
            {
                InstructionStream istream = CodeBlockList[snapShot.CodeBlockId].instrStream;

                int instrCount = istream.instrList.Count - snapShot.InstructionCount;
                if (instrCount > 0)
                {
                    istream.instrList.RemoveRange(snapShot.InstructionCount, instrCount);
                }

                int graphNodeCount = istream.dependencyGraph.GraphList.Count - snapShot.GraphNodeCount;
                if (graphNodeCount > 0)
                {
                    istream.dependencyGraph.GraphList.RemoveRange(snapShot.GraphNodeCount, graphNodeCount);
                }
            }
        }

        /// <summary>
        /// Reset properties for recompilation
        /// Generated instructions for imported libraries are preserved. 
        /// This means they are just reloaded, not regenerated
        /// </summary>
        private void ResetDeltaCompile()
        {
            if (CodeBlockList.Count > 0)
            {
                // Preserve only the instructions of libraries that were previously loaded
                // Other instructions need to be removed and regenerated
                int instrCount = CodeBlockList[0].instrStream.instrList.Count;

                // Remove from these indices
                int from = deltaCompileStartPC;
                int count = instrCount - deltaCompileStartPC;

                CodeBlockList[0].instrStream.instrList.RemoveRange(from, count);

                // Remove graphnodes from this range
                // TODO Jun: Optimize this - determine which graphnodes need to be removed during compilation
                int removeGraphnodesFrom = ProtoCore.DSASM.Constants.kInvalidIndex;
                for (int n = 0; n < CodeBlockList[0].instrStream.dependencyGraph.GraphList.Count; ++n)
                {
                    ProtoCore.AssociativeGraph.GraphNode graphNode = CodeBlockList[0].instrStream.dependencyGraph.GraphList[n];
                    if (graphNode.updateBlock.startpc >= deltaCompileStartPC)
                    {
                        removeGraphnodesFrom = n;
                        break;
                    }
                }

                if (ProtoCore.DSASM.Constants.kInvalidIndex != removeGraphnodesFrom)
                {
                    count = CodeBlockList[0].instrStream.dependencyGraph.GraphList.Count - removeGraphnodesFrom;

                    int classIndex = CodeBlockList[0].instrStream.dependencyGraph.GraphList[removeGraphnodesFrom].classIndex;
                    int procIndex = CodeBlockList[0].instrStream.dependencyGraph.GraphList[removeGraphnodesFrom].procIndex;

                    // TODO Jun: Find the better way to remove the graphnodes from the nodemap
                    // Does getting the classindex and proindex of the first node sufficient?
                    CodeBlockList[0].instrStream.dependencyGraph.RemoveNodesFromScope(classIndex, procIndex);

                    // Remove the graphnodes from them main list
                    CodeBlockList[0].instrStream.dependencyGraph.GraphList.RemoveRange(removeGraphnodesFrom, count);
                }
                else
                {
                    // @keyu: This is for the first run. Just simply remove 
                    // global graph node from the map to avoid they are marked
                    // as dirty at the next run.
                    CodeBlockList[0].instrStream.dependencyGraph.RemoveNodesFromScope(Constants.kInvalidIndex, Constants.kInvalidIndex);
                }
            }
        }

        // @keyu: ResetDeltaExection() resets everything so that the core will 
        // compile new code and execute it, but in some cases we dont want to 
        // compile code, just re-execute the existing code, therefore only some 
        // states need to be reset.
        public void ResetForExecution()
        {
            ExecMode = ProtoCore.DSASM.InterpreterMode.kNormal;
            ExecutionState = (int)ExecutionStateEventArgs.State.kInvalid;
            RunningBlock = 0;
        }

        // Comment Jun:
        // The core is reused on delta execution
        // These are properties that need to be reset on subsequent executions
        // All properties require reset except for the runtime memory
        public void ResetForDeltaExecution()
        {
            compileSucceeded = false;

            ClassIndex = ProtoCore.DSASM.Constants.kInvalidIndex;


            watchClassScope = ProtoCore.DSASM.Constants.kInvalidIndex;
            watchFunctionScope = ProtoCore.DSASM.Constants.kInvalidIndex;
            watchBaseOffset = 0;
            watchStack = new List<StackValue>();
            watchSymbolList = new List<SymbolNode>();
            watchFramePointer = ProtoCore.DSASM.Constants.kInvalidIndex;

            ID = FIRST_CORE_ID;

            //recurtion
            recursivePoint = new List<FunctionCounter>();
            funcCounterTable = new List<FunctionCounter>();
            calledInFunction = false;

            //GlobOffset = 0;
            GlobHeapOffset = 0;
            BaseOffset = 0;
            GraphNodeUID = 0;
            RunningBlock = 0;
            //CodeBlockList = new List<DSASM.CodeBlock>();
            CompleteCodeBlockList = new List<ProtoCore.DSASM.CodeBlock>();
            DSExecutable = new ProtoCore.DSASM.Executable();

            AssocNode = null;


            //
            //
            // Comment Jun: Delta execution should not reset the class tables as they are preserved
            //
            //      FunctionTable = new Lang.FunctionTable();
            //      ClassTable = new DSASM.ClassTable();
            //      TypeSystem = new TypeSystem();
            //      TypeSystem.SetClassTable(ClassTable);
            //      ProcNode = null;
            //      ProcTable = new DSASM.ProcedureTable(ProtoCore.DSASM.Constants.kGlobalScope);
            //
            //      CodeBlockList = new List<DSASM.CodeBlock>();
            //
            //

            //      CodeBlockIndex = 0;
            //      RuntimeTableIndex = 0;

            //


            //Initialize the function pointer table
            FunctionPointerTable = new ProtoCore.DSASM.FunctionPointerTable();

            //Initialize the dynamic string table and dynamic function table
            DynamicVariableTable = new ProtoCore.DSASM.DynamicVariableTable();
            DynamicFunctionTable = new ProtoCore.DSASM.DynamicFunctionTable();
            replicationGuides = new List<List<int>>();

            ExceptionHandlingManager = new ExceptionHandlingManager();
            startPC = ProtoCore.DSASM.Constants.kInvalidIndex;

            if (Options.SuppressBuildOutput)
            {
                //  don't log any of the build related messages
                //  just accumulate them in relevant containers with
                //  BuildStatus object
                //
                BuildStatus = new BuildStatus(this, false, false, false);
            }
            else
            {
                BuildStatus = new BuildStatus(this, Options.BuildOptWarningAsError, null, Options.BuildOptErrorAsWarning);
            }

            SSASubscript = 0;
            ExpressionUID = 0;
            ModifierBlockUID = 0;
            ModifierStateSubscript = 0;

            ExprInterpreterExe = null;
            ExecMode = ProtoCore.DSASM.InterpreterMode.kNormal;

            assocCodegen = null;
            FunctionCallDepth = 0;

            // Default execution log is Console.Out.
            this.ExecutionLog = Console.Out;
            ExecutionState = (int)ExecutionStateEventArgs.State.kInvalid; //not yet started

            DebugProps = new DebugProperties();
            InterpreterProps = new Stack<InterpreterProperties>();
            stackActiveExceptionRegistration = new Stack<ExceptionRegistration>();

            ExecutiveProvider = new ExecutiveProvider();
            ParsingMode = ProtoCore.ParseMode.Normal;

            // Reset PC dictionary containing PC to line/col map
            if (codeToLocation != null)
                codeToLocation.Clear();

            if (LocationErrorMap != null)
                LocationErrorMap.Clear();

            if (AstNodeList != null)
                AstNodeList.Clear();

            ResetDeltaCompile();
        }

        public void ResetForPrecompilation()
        {
            compileSucceeded = false;

            GraphNodeUID = 0;
            CodeBlockIndex = 0;
            RuntimeTableIndex = 0;
            
            //Initialize the dynamic string table and dynamic function table
            DynamicVariableTable = new ProtoCore.DSASM.DynamicVariableTable();
            DynamicFunctionTable = new ProtoCore.DSASM.DynamicFunctionTable();

            // If the previous compilation for import resulted in a build error, 
            // ignore it and continue compiling other import statements
            /*if (BuildStatus.ErrorCount > 0)
            {
                ImportHandler = null;
                CodeBlockList.Clear();
                CompleteCodeBlockList.Clear();
            }*/

            if (Options.SuppressBuildOutput)
            {
                //  don't log any of the build related messages
                //  just accumulate them in relevant containers with
                //  BuildStatus object
                //
                BuildStatus = new BuildStatus(this, false, false, false);
            }
            else
            {
                BuildStatus = new BuildStatus(this, Options.BuildOptWarningAsError);
            }
            
            if (AstNodeList != null) 
                AstNodeList.Clear();

            ExpressionUID = 0;
        }

        private void ResetAll(CompileOptions options)
        {
            compileSucceeded = false;

            ProtoCore.Utils.Validity.AssertExpiry();
            Options = options;
            Executives = new Dictionary<ProtoCore.Language, ProtoCore.Executive>();
            FunctionTable = new ProtoCore.Lang.FunctionTable();
            ClassIndex = ProtoCore.DSASM.Constants.kInvalidIndex;

            Heap = new ProtoCore.DSASM.Heap();
            Rmem = new ProtoCore.Runtime.RuntimeMemory(Heap);

            watchClassScope = ProtoCore.DSASM.Constants.kInvalidIndex;
            watchFunctionScope = ProtoCore.DSASM.Constants.kInvalidIndex;
            watchBaseOffset = 0;
            watchStack = new List<StackValue>();
            watchSymbolList = new List<SymbolNode>();
            watchFramePointer = ProtoCore.DSASM.Constants.kInvalidIndex;

            ID = FIRST_CORE_ID;

            //recurtion
            recursivePoint = new List<FunctionCounter>();
            funcCounterTable = new List<FunctionCounter>();
            calledInFunction = false;

            GlobOffset = 0;
            GlobHeapOffset = 0;
            BaseOffset = 0;
            GraphNodeUID = 0;
            RunningBlock = 0;
            CodeBlockIndex = 0;
            RuntimeTableIndex = 0;
            CodeBlockList = new List<ProtoCore.DSASM.CodeBlock>();
            CompleteCodeBlockList = new List<ProtoCore.DSASM.CodeBlock>();
            DSExecutable = new ProtoCore.DSASM.Executable();

            AssocNode = null;

            // TODO Jun/Luke type system refactoring
            // Initialize the globalClass table and type system
            ClassTable = new ProtoCore.DSASM.ClassTable();
            TypeSystem = new TypeSystem();
            TypeSystem.SetClassTable(ClassTable);
            ProcNode = null;
            ProcTable = new ProtoCore.DSASM.ProcedureTable(ProtoCore.DSASM.Constants.kGlobalScope);

            //Initialize the function pointer table
            FunctionPointerTable = new ProtoCore.DSASM.FunctionPointerTable();

            //Initialize the dynamic string table and dynamic function table
            DynamicVariableTable = new ProtoCore.DSASM.DynamicVariableTable();
            DynamicFunctionTable = new ProtoCore.DSASM.DynamicFunctionTable();
            replicationGuides = new List<List<int>>();

            ExceptionHandlingManager = new ExceptionHandlingManager();
            startPC = ProtoCore.DSASM.Constants.kInvalidIndex;

            deltaCompileStartPC = ProtoCore.DSASM.Constants.kInvalidIndex;

            if (options.SuppressBuildOutput)
            {
                //  don't log any of the build related messages
                //  just accumulate them in relevant containers with
                //  BuildStatus object
                //
                BuildStatus = new BuildStatus(this, false, false, false);
            }
            else
            {
                BuildStatus = new BuildStatus(this, Options.BuildOptWarningAsError, null, Options.BuildOptErrorAsWarning);
            }

            SSASubscript = 0;
            ExpressionUID = 0;
            ModifierBlockUID = 0;
            ModifierStateSubscript = 0;

            ExprInterpreterExe = null;
            ExecMode = ProtoCore.DSASM.InterpreterMode.kNormal;

            assocCodegen = null;
            FunctionCallDepth = 0;

            // Default execution log is Console.Out.
            this.ExecutionLog = Console.Out;
            ExecutionState = (int)ExecutionStateEventArgs.State.kInvalid; //not yet started

            DebugProps = new DebugProperties();
            //stackNodeExecutedSameTimes = new Stack<List<AssociativeGraph.GraphNode>>();
            //stackExecutingGraphNodes = new Stack<AssociativeGraph.GraphNode>();
            InterpreterProps = new Stack<InterpreterProperties>();
            stackActiveExceptionRegistration = new Stack<ExceptionRegistration>();

            ExecutiveProvider = new ExecutiveProvider();

            Configurations = new Dictionary<string, object>();

            ContinuationStruct = new ProtoCore.Lang.ContinuationStructure();
            ParsingMode = ProtoCore.ParseMode.Normal;
            
            IsParsingPreloadedAssembly = false;
            IsParsingCodeBlockNode = false;
            ImportHandler = null;

            deltaCompileStartPC = 0;
            builtInsLoaded = false;

            //FFIPropertyChangedMonitor = new FFIPropertyChangedMonitor(this);


        }

        // The unique subscript for SSA temporaries
        // TODO Jun: Organize these variables in core into proper enums/classes/struct
        public int SSASubscript { get; set; }
        public int ExpressionUID { get; set; }
        public int ModifierBlockUID { get; set; }
        public int ModifierStateSubscript { get; set; }

        public ExceptionHandlingManager ExceptionHandlingManager { get; set; }

        private int tempVarId = 0;
        private int tempLanguageId = 0;

        // TODO Jun: Cleansify me - i dont need to be here
        public ProtoCore.AST.AssociativeAST.AssociativeNode AssocNode { get; set; }
        public int startPC { get; set; }


        //
        // TODO Jun: This is the expression interpreters executable. 
        //           It must be moved to its own core, whre each core is an instance of a compiler+interpreter
        //
        public Executable ExprInterpreterExe { get; set; }
        public ProtoCore.DSASM.InterpreterMode ExecMode { get; set; }
        public List<SymbolNode> watchSymbolList { get; set; }
        public int watchClassScope { get; set; }
        public int watchFunctionScope { get; set; }
        public int watchBaseOffset { get; set; }
        public List<StackValue> watchStack { get; set; }
        public int watchFramePointer { get; set; }

        public ProtoCore.CodeGen assocCodegen { get; set; }

        // this one is to address the issue that when the execution control is in a language block
        // which is further inside a function, the compiler feprun is false, 
        // when inspecting value in that language block or the function, debugger will assume the function index is -1, 
        // name look up will fail beacuse all the local variables inside 
        // that language block and fucntion has non-zero function index 
        public int FunctionCallDepth { get; set; }
        public System.IO.TextWriter ExecutionLog { get; set; }

        protected void OnDispose()
        {
            if (Dispose != null)
            {
                Dispose(this);
            }
        }

        public void Cleanup()
        {
            OnDispose();
            ProtoFFI.CLRModuleType.ClearTypes();
        }


        public void InitializeContextGlobals(Dictionary<string, object> context)
        {
            int globalBlock = 0;
            foreach (KeyValuePair<string, object> global in context)
            {
                int stackIndex = CodeBlockList[globalBlock].symbolTable.IndexOf(global.Key);

                if (global.Value.GetType() != typeof(Double) && global.Value.GetType() != typeof(Int32))
                    throw new NotImplementedException("Context that's aren't double are not yet supported @TODO: Jun,Sharad,Luke ASAP");

                double dValue = Convert.ToDouble(global.Value);
                StackValue svData = StackUtils.BuildDouble(dValue);
                Rmem.SetGlobalStackData(stackIndex, svData);
            }
        }

        public CompileStateTracker(CompileOptions options)
        {
            ResetAll(options);
        }

        public SymbolNode GetSymbolInFunction(string name, int classScope, int functionScope, CodeBlock codeBlock)
        {
            Debug.Assert(functionScope != Constants.kGlobalScope);
            if (Constants.kGlobalScope == functionScope)
            {
                return null;
            }

            int symbolIndex = Constants.kInvalidIndex;

            if (classScope != Constants.kGlobalScope)
            {
                //Search local variable for the class member function
                symbolIndex = ClassTable.ClassNodes[classScope].symbols.IndexOf(name, classScope, functionScope);
                if (symbolIndex != Constants.kInvalidIndex)
                {
                    return ClassTable.ClassNodes[classScope].symbols.symbolList[symbolIndex];
                }

                //Search class members
                symbolIndex = ClassTable.ClassNodes[classScope].symbols.IndexOf(name, classScope, Constants.kGlobalScope);
                if (symbolIndex != Constants.kInvalidIndex)
                {
                    return ClassTable.ClassNodes[classScope].symbols.symbolList[symbolIndex];
                }
            }

            while (symbolIndex == Constants.kInvalidIndex &&
                   codeBlock != null &&
                   codeBlock.blockType != CodeBlockType.kFunction)
            {
                symbolIndex = codeBlock.symbolTable.IndexOf(name, classScope, functionScope);
                if (symbolIndex != Constants.kInvalidIndex)
                {
                    return codeBlock.symbolTable.symbolList[symbolIndex];
                }
                else
                {
                    codeBlock = codeBlock.parent;
                }
            }

            if (symbolIndex == Constants.kInvalidIndex &&
                codeBlock != null &&
                codeBlock.blockType == CodeBlockType.kFunction)
            {
                symbolIndex = codeBlock.symbolTable.IndexOf(name, classScope, functionScope);
                if (symbolIndex != Constants.kInvalidIndex)
                {
                    return codeBlock.symbolTable.symbolList[symbolIndex];
                }
            }

            return null;
        }

        public SymbolNode GetFirstVisibleSymbol(string name, int classscope, int function, CodeBlock codeblock)
        {
            //  
            //

            Debug.Assert(null != codeblock);
            if (null == codeblock)
            {
                return null;
            }

            int symbolIndex = ProtoCore.DSASM.Constants.kInvalidIndex;
            bool stillInsideFunction = function != ProtoCore.DSASM.Constants.kInvalidIndex;
            ProtoCore.DSASM.CodeBlock searchBlock = codeblock;
            // TODO(Jiong): Code Duplication, Consider moving this if else block inside the while loop 
            if (stillInsideFunction)
            {
                symbolIndex = searchBlock.symbolTable.IndexOf(name, classscope, function);

                if (function != ProtoCore.DSASM.Constants.kInvalidIndex &&
                    searchBlock.procedureTable != null &&
                    searchBlock.procedureTable.procList.Count > function &&   // Note: This check assumes we can not define functions inside a fucntion 
                    symbolIndex == ProtoCore.DSASM.Constants.kInvalidIndex)
                    symbolIndex = searchBlock.symbolTable.IndexOf(name, classscope, ProtoCore.DSASM.Constants.kInvalidIndex);
            }
            else
            {
                symbolIndex = searchBlock.symbolTable.IndexOf(name, classscope, ProtoCore.DSASM.Constants.kInvalidIndex);
            }
            while (ProtoCore.DSASM.Constants.kInvalidIndex == symbolIndex)
            {
                // if the search block is of type function, it means our search has gone out of the function itself
                // so, we should ignore the given function index and only search its parent block's global variable
                if (searchBlock.blockType == ProtoCore.DSASM.CodeBlockType.kFunction)
                    stillInsideFunction = false;

                searchBlock = searchBlock.parent;
                if (null != searchBlock)
                {
                    // Continue searching
                    if (stillInsideFunction)
                    {
                        // we are still inside a function, first search the local variable defined in this function 
                        // if not found, then search the enclosing block by specifying the function index as -1
                        symbolIndex = searchBlock.symbolTable.IndexOf(name, classscope, function);

                        // this check is to avoid unnecessary search
                        // for example if we have a for loop inside an imperative block which is further inside a function
                        // when we are searching inside the for loop or language block, there is no need to search twice
                        // we need to search twice only when we are searching directly inside the function, 
                        if (function != ProtoCore.DSASM.Constants.kInvalidIndex &&
                            searchBlock.procedureTable != null &&
                            searchBlock.procedureTable.procList.Count > function && // Note: This check assumes we can not define functions inside a fucntion 
                            symbolIndex == ProtoCore.DSASM.Constants.kInvalidIndex)

                            symbolIndex = searchBlock.symbolTable.IndexOf(name, classscope, ProtoCore.DSASM.Constants.kInvalidIndex);

                    }
                    else
                    {
                        symbolIndex = searchBlock.symbolTable.IndexOf(name, classscope, ProtoCore.DSASM.Constants.kInvalidIndex);
                    }
                }
                else
                {
                    // End of nested blocks

                    /*
                    // Not found? Look at the class scope
                    if (ProtoCore.DSASM.Constants.kInvalidIndex != classscope)
                    {
                        // Look at the class members and base class members
                        bool hasSymbol = false;
                        ProtoCore.DSASM.AddressType addrType =  DSASM.AddressType.Invalid;
                        ProtoCore.DSASM.ClassNode cnode = classTable.list[classscope];
                        symbolIndex = cnode.GetFirstVisibleSymbol(name, classscope, function, out hasSymbol, out addrType);
                        if (ProtoCore.DSASM.Constants.kInvalidIndex != symbolIndex)
                        {
                            if (addrType == DSASM.AddressType.StaticMemVarIndex)
                            {
                                return codeBlockList[0].symbolTable.symbolList[symbolIndex];
                            }
                            else
                            {
                                return classTable.list[classscope].symbols.symbolList[symbolIndex];
                            }
                        }

                        // Look at the class constructors and functions
                        symbolIndex = classTable.list[classscope].symbols.IndexOf(name, classscope, function);
                        if (ProtoCore.DSASM.Constants.kInvalidIndex != symbolIndex)
                        {
                            return classTable.list[classscope].symbols.symbolList[symbolIndex];
                        }
                    }


                    // Not found? Look at the global scope
                    symbolIndex = searchBlock.symbolTable.IndexOf(name, ProtoCore.DSASM.Constants.kInvalidIndex, ProtoCore.DSASM.Constants.kGlobalScope);
                    if (ProtoCore.DSASM.Constants.kInvalidIndex == symbolIndex)
                    {
                        return null;
                    }
                    break;
                     * */
                    return null;
                }
            }
            return searchBlock.symbolTable.symbolList[symbolIndex];
        }

        public bool IsFunctionCodeBlock(CodeBlock cblock)
        {
            // Determine if the immediate block is a function block
            // Construct blocks are ignored
            Debug.Assert(null != cblock);
            while (null != cblock)
            {
                if (ProtoCore.DSASM.CodeBlockType.kFunction == cblock.blockType)
                {
                    return true;
                }
                else if (ProtoCore.DSASM.CodeBlockType.kLanguage == cblock.blockType)
                {
                    return false;
                }
                cblock = cblock.parent;
            }
            return false;
        }

        public ProcedureNode GetFirstVisibleProcedure(string name, List<ProtoCore.Type> argTypeList, CodeBlock codeblock)
        {
            Debug.Assert(null != codeblock);
            if (null == codeblock)
            {
                return null;
            }

            ProtoCore.DSASM.CodeBlock searchBlock = codeblock;
            while (null != searchBlock)
            {
                if (null == searchBlock.procedureTable)
                {
                    searchBlock = searchBlock.parent;
                    continue;
                }

                // The class table is passed just to check for coercion values
                int procIndex = searchBlock.procedureTable.IndexOf(name, argTypeList, ClassTable);
                if (ProtoCore.DSASM.Constants.kInvalidIndex != procIndex)
                {
                    return searchBlock.procedureTable.procList[procIndex];
                }
                searchBlock = searchBlock.parent;
            }
            return null;
        }

        public ProtoCore.DSASM.CodeBlock GetCodeBlock(List<ProtoCore.DSASM.CodeBlock> blockList, int blockId)
        {
            ProtoCore.DSASM.CodeBlock codeblock = null;
            codeblock = blockList.Find(x => x.codeBlockId == blockId);
            if (codeblock == null)
            {
                foreach (ProtoCore.DSASM.CodeBlock block in blockList)
                {
                    codeblock = GetCodeBlock(block.children, blockId);
                    if (codeblock != null)
                    {
                        break;
                    }
                }
            }
            return codeblock;
        }

        //public StackValue Bounce(int exeblock, int entry, ProtoCore.Runtime.Context context, ProtoCore.DSASM.StackFrame stackFrame, int locals = 0, ProtoCore.DebugServices.EventSink sink = null)
        //{
        //    if (stackFrame != null)
        //    {
        //        ProtoCore.DSASM.StackValue svThisPtr = stackFrame.GetAt(ProtoCore.DSASM.StackFrame.AbsoluteIndex.kThisPtr);
        //        int ci = (int)stackFrame.GetAt(ProtoCore.DSASM.StackFrame.AbsoluteIndex.kClass).opdata;
        //        int fi = (int)stackFrame.GetAt(ProtoCore.DSASM.StackFrame.AbsoluteIndex.kFunction).opdata;
        //        int returnAddr = (int)stackFrame.GetAt(ProtoCore.DSASM.StackFrame.AbsoluteIndex.kReturnAddress).opdata;
        //        int blockDecl = (int)stackFrame.GetAt(ProtoCore.DSASM.StackFrame.AbsoluteIndex.kFunctionBlock).opdata;
        //        int blockCaller = (int)stackFrame.GetAt(ProtoCore.DSASM.StackFrame.AbsoluteIndex.kFunctionCallerBlock).opdata;
        //        ProtoCore.DSASM.StackFrameType callerFrameType = (ProtoCore.DSASM.StackFrameType)stackFrame.GetAt(ProtoCore.DSASM.StackFrame.AbsoluteIndex.kCallerStackFrameType).opdata;
        //        ProtoCore.DSASM.StackFrameType frameType = (ProtoCore.DSASM.StackFrameType)stackFrame.GetAt(ProtoCore.DSASM.StackFrame.AbsoluteIndex.kStackFrameType).opdata;
        //        Validity.Assert(frameType == StackFrameType.kTypeLanguage);

        //        int depth = (int)stackFrame.GetAt(ProtoCore.DSASM.StackFrame.AbsoluteIndex.kStackFrameDepth).opdata;
        //        int framePointer = (int)stackFrame.GetAt(ProtoCore.DSASM.StackFrame.AbsoluteIndex.kFramePointer).opdata;
        //        List<StackValue> registers = stackFrame.GetRegisters();

        //        Rmem.PushStackFrame(svThisPtr, ci, fi, returnAddr, blockDecl, blockCaller, callerFrameType, frameType, depth + 1, framePointer, registers, locals, 0);
        //    }

        //    ProtoCore.Language id = DSExecutable.instrStreamList[exeblock].language;
        //    CurrentExecutive = Executives[id];
        //    ProtoCore.DSASM.StackValue sv = Executives[id].Execute(exeblock, entry, context, sink);
        //    return sv;
        //}

        //public StackValue Bounce(int exeblock, int entry, ProtoCore.Runtime.Context context, List<Instruction> breakpoints, ProtoCore.DSASM.StackFrame stackFrame, int locals = 0,
        //    ProtoCore.DSASM.Executive exec = null, ProtoCore.DebugServices.EventSink sink = null, bool fepRun = false)
        //{
        //    if (stackFrame != null)
        //    {
        //        ProtoCore.DSASM.StackValue svThisPtr = stackFrame.GetAt(ProtoCore.DSASM.StackFrame.AbsoluteIndex.kThisPtr);
        //        int ci = (int)stackFrame.GetAt(ProtoCore.DSASM.StackFrame.AbsoluteIndex.kClass).opdata;
        //        int fi = (int)stackFrame.GetAt(ProtoCore.DSASM.StackFrame.AbsoluteIndex.kFunction).opdata;
        //        int returnAddr = (int)stackFrame.GetAt(ProtoCore.DSASM.StackFrame.AbsoluteIndex.kReturnAddress).opdata;
        //        int blockDecl = (int)stackFrame.GetAt(ProtoCore.DSASM.StackFrame.AbsoluteIndex.kFunctionBlock).opdata;
        //        int blockCaller = (int)stackFrame.GetAt(ProtoCore.DSASM.StackFrame.AbsoluteIndex.kFunctionCallerBlock).opdata;
        //        ProtoCore.DSASM.StackFrameType callerFrameType = (ProtoCore.DSASM.StackFrameType)stackFrame.GetAt(ProtoCore.DSASM.StackFrame.AbsoluteIndex.kCallerStackFrameType).opdata;
        //        ProtoCore.DSASM.StackFrameType frameType = (ProtoCore.DSASM.StackFrameType)stackFrame.GetAt(ProtoCore.DSASM.StackFrame.AbsoluteIndex.kStackFrameType).opdata;
        //        Validity.Assert(frameType == StackFrameType.kTypeLanguage);

        //        int depth = (int)stackFrame.GetAt(ProtoCore.DSASM.StackFrame.AbsoluteIndex.kStackFrameDepth).opdata;
        //        int framePointer = (int)stackFrame.GetAt(ProtoCore.DSASM.StackFrame.AbsoluteIndex.kFramePointer).opdata;
        //        List<StackValue> registers = stackFrame.GetRegisters();

        //        DebugProps.SetUpBounce(exec, blockCaller, returnAddr);

        //        Rmem.PushStackFrame(svThisPtr, ci, fi, returnAddr, blockDecl, blockCaller, callerFrameType, frameType, depth + 1, framePointer, registers, locals, 0);
        //    }

        //    ProtoCore.Language id = DSExecutable.instrStreamList[exeblock].language;
        //    CurrentExecutive = Executives[id];

        //    ProtoCore.DSASM.StackValue sv = Executives[id].Execute(exeblock, entry, context, breakpoints, sink, fepRun);
        //    return sv;
        //}

        private void BfsBuildSequenceTable(CodeBlock codeBlock, SymbolTable[] runtimeSymbols)
        {
            if (ProtoCore.DSASM.CodeBlockType.kLanguage == codeBlock.blockType
                || ProtoCore.DSASM.CodeBlockType.kFunction == codeBlock.blockType
                || ProtoCore.DSASM.CodeBlockType.kConstruct == codeBlock.blockType)
            {
                Debug.Assert(codeBlock.symbolTable.runtimeIndex < RuntimeTableIndex);
                runtimeSymbols[codeBlock.symbolTable.runtimeIndex] = codeBlock.symbolTable;
            }

            foreach (ProtoCore.DSASM.CodeBlock child in codeBlock.children)
            {
                BfsBuildSequenceTable(child, runtimeSymbols);
            }
        }

        private void BfsBuildProcedureTable(CodeBlock codeBlock, ProcedureTable[] procTable)
        {
            if (ProtoCore.DSASM.CodeBlockType.kLanguage == codeBlock.blockType || ProtoCore.DSASM.CodeBlockType.kFunction == codeBlock.blockType)
            {
                Debug.Assert(codeBlock.procedureTable.runtimeIndex < RuntimeTableIndex);
                procTable[codeBlock.procedureTable.runtimeIndex] = codeBlock.procedureTable;
            }

            foreach (ProtoCore.DSASM.CodeBlock child in codeBlock.children)
            {
                BfsBuildProcedureTable(child, procTable);
            }
        }

        private void BfsBuildInstructionStreams(CodeBlock codeBlock, InstructionStream[] istreamList)
        {
            if (null != codeBlock)
            {
                if (ProtoCore.DSASM.CodeBlockType.kLanguage == codeBlock.blockType || ProtoCore.DSASM.CodeBlockType.kFunction == codeBlock.blockType)
                {
                    Debug.Assert(codeBlock.codeBlockId < CodeBlockIndex);
                    istreamList[codeBlock.codeBlockId] = codeBlock.instrStream;
                }

                foreach (ProtoCore.DSASM.CodeBlock child in codeBlock.children)
                {
                    BfsBuildInstructionStreams(child, istreamList);
                }
            }
        }

        
        public void GenerateExprExe()
        {
            // TODO Jun: Determine if we really need another executable for the expression interpreter
            Validity.Assert(null == ExprInterpreterExe);
            ExprInterpreterExe = new ProtoCore.DSASM.Executable();

            // Copy all tables
            ExprInterpreterExe.classTable = DSExecutable.classTable;
            ExprInterpreterExe.procedureTable = DSExecutable.procedureTable;
            ExprInterpreterExe.runtimeSymbols = DSExecutable.runtimeSymbols;
            ExprInterpreterExe.isSingleAssocBlock = DSExecutable.isSingleAssocBlock;
            
            // Copy all instruction streams
            // TODO Jun: What method to copy all? Use that
            ExprInterpreterExe.instrStreamList = new InstructionStream[DSExecutable.instrStreamList.Length];
            for (int i = 0; i < DSExecutable.instrStreamList.Length; ++i)
            {
                if (null != DSExecutable.instrStreamList[i])
                {
                    ExprInterpreterExe.instrStreamList[i] = new InstructionStream(DSExecutable.instrStreamList[i].language, this);
                    for (int j = 0; j < DSExecutable.instrStreamList[i].instrList.Count; ++j)
                    {
                        ExprInterpreterExe.instrStreamList[i].instrList.Add(DSExecutable.instrStreamList[i].instrList[j]);
                    }
                }
            }
        }


        public void GenerateExprExeInstructions(int blockScope)
        {
            // Append the expression instruction at the end of the current block
            for (int n = 0; n < ExprInterpreterExe.iStreamCanvas.instrList.Count; ++n)
            {
                ExprInterpreterExe.instrStreamList[blockScope].instrList.Add(ExprInterpreterExe.iStreamCanvas.instrList[n]);
            }
        }

        public void GenerateExecutable()
        {
            Debug.Assert(CodeBlockList.Count >= 0);

            // Retrieve the class table directly since it is a global table
            DSExecutable.classTable = ClassTable;

            // Build the runtime symbols
            DSExecutable.runtimeSymbols = new ProtoCore.DSASM.SymbolTable[RuntimeTableIndex];
            for (int n = 0; n < CodeBlockList.Count; ++n)
            {
                BfsBuildSequenceTable(CodeBlockList[n], DSExecutable.runtimeSymbols);
            }

            // Build the runtime procedure table
            DSExecutable.procedureTable = new ProtoCore.DSASM.ProcedureTable[RuntimeTableIndex];
            for (int n = 0; n < CodeBlockList.Count; ++n)
            {
                BfsBuildProcedureTable(CodeBlockList[n], DSExecutable.procedureTable);
            }

            // Build the executable instruction streams
            DSExecutable.instrStreamList = new ProtoCore.DSASM.InstructionStream[CodeBlockIndex];
            for (int n = 0; n < CodeBlockList.Count; ++n)
            {
                BfsBuildInstructionStreams(CodeBlockList[n], DSExecutable.instrStreamList);
            }

            // Single associative block means the first instruction is an immediate bounce 
            // This variable is only used by the mirror to determine if the GetValue()
            // block parameter needs to be incremented or not in order to get the correct global variable
            if (DSExecutable.isSingleAssocBlock)
            {
                DSExecutable.isSingleAssocBlock = (ProtoCore.DSASM.OpCode.BOUNCE == CodeBlockList[0].instrStream.instrList[0].opCode) ? true : false;
            }

            // Properties directly accessed by the runtime from core, should now be accessed from the executable
            DSExecutable.CodeBlockList = new List<CodeBlock>(CodeBlockList);
            DSExecutable.FunctionTable = FunctionTable;
            DSExecutable.CompleteCodeBlockList = CompleteCodeBlockList;
            DSExecutable.DynamicVariableTable = DynamicVariableTable;
            DSExecutable.DynamicFunctionTable = DynamicFunctionTable;
            DSExecutable.FunctionPointerTable = FunctionPointerTable;

            GenerateExprExe();
        }



        public string GenerateTempVar()
        {
            tempVarId++;
            return ProtoCore.DSASM.Constants.kTempVar + tempVarId.ToString();
        }


        public string GenerateTempPropertyVar()
        {
            tempVarId++;
            return ProtoCore.DSASM.Constants.kTempPropertyVar + tempVarId.ToString();
        }

        public string GenerateTempLangageVar()
        {
            tempLanguageId++;
            return ProtoCore.DSASM.Constants.kTempLangBlock + tempLanguageId.ToString();
        }

        public bool IsTempVar(String varName)
        {
            if (String.IsNullOrEmpty(varName))
            {
                return false;
            }
            return varName[0] == '%';
        }

        public string GetModifierBlockTemp(string modifierName)
        {
            // The naming convention for auto-generated modifier block states begins with a '%'
            // followed by "<Constants.kTempModifierStateNamePrefix>_<modifier_block_name>_<index>
            string modStateTemp = ProtoCore.DSASM.Constants.kTempModifierStateNamePrefix + modifierName + ModifierStateSubscript.ToString();
            ++ModifierStateSubscript;
            return modStateTemp;
        }

        public List<int> GetAncestorBlockIdsOfBlock(int blockId)
        {
            if (blockId >= this.CompleteCodeBlockList.Count || blockId < 0)
            {
                return new List<int>();
            }
            CodeBlock thisBlock = this.CompleteCodeBlockList[blockId];

            var ancestors = new List<int>();
            CodeBlock codeBlock = thisBlock.parent;
            while (codeBlock != null)
            {
                ancestors.Add(codeBlock.codeBlockId);
                codeBlock = codeBlock.parent;
            }
            return ancestors;
        }

        public int GetCurrentBlockId()
        {
            int constructBlockId = this.Rmem.CurrentConstructBlockId;
            if (constructBlockId == Constants.kInvalidIndex)
                return DebugProps.CurrentBlockId;

            CodeBlock constructBlock = GetCodeBlock(CodeBlockList, constructBlockId);
            while (null != constructBlock && constructBlock.blockType == CodeBlockType.kConstruct)
            {
                constructBlock = constructBlock.parent;
            }

            if (null != constructBlock)
                constructBlockId = constructBlock.codeBlockId;

            if (constructBlockId != DebugProps.CurrentBlockId)
                return DebugProps.CurrentBlockId;
            else
                return this.Rmem.CurrentConstructBlockId;
        }

        public ProtoCore.AssociativeGraph.GraphNode GetExecutingGraphNode()
        {
            foreach (var prop in this.InterpreterProps)
            {
                if (prop.executingGraphNode != null)
                {
                    return prop.executingGraphNode;
                }
            }

            return null;
        }

        public bool IsEvalutingPropertyChanged()
        {
            foreach (var prop in this.InterpreterProps)
            {
                if (prop.updateStatus == ProtoCore.AssociativeEngine.UpdateStatus.kPropertyChangedUpdate)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

