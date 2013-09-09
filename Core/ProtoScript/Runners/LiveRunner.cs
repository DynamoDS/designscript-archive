
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using GraphToDSCompiler;
using ProtoCore.DSASM.Mirror;
using System.Diagnostics;
using ProtoCore.Utils;
using System.ComponentModel;
using System.Threading;
using ProtoFFI;
using ProtoCore.AssociativeGraph;


namespace ProtoScript.Runners
{
    public enum EventStatus
    {
        OK,
        Error,
        Warning
    }


    public class NodeValueReadyEventArgs : EventArgs
    {
        public NodeValueReadyEventArgs(ProtoCore.Mirror.RuntimeMirror mirror, uint nodeId)
            : this(mirror, nodeId, EventStatus.OK, null)
        {
        }

        public NodeValueReadyEventArgs(ProtoCore.Mirror.RuntimeMirror mirror, uint nodeId, EventStatus resultStatus, String errorString)
        {
            this.RuntimeMirror = mirror;
            this.NodeId = nodeId;
            ResultStatus = resultStatus;
            ErrorString = errorString;
        }

        public uint NodeId { get; private set; }
        public EventStatus ResultStatus { get; private set; }
        public String ErrorString { get; private set; }
        public ProtoCore.Mirror.RuntimeMirror RuntimeMirror { get; private set; }
    }

    public class NodeValueNotAvailableEventArgs: NodeValueReadyEventArgs
    {
        public NodeValueNotAvailableEventArgs(uint nodeId):
            base(null, nodeId, EventStatus.OK, "Not value available for this node")
        {}
    }

    public class GraphUpdateReadyEventArgs : EventArgs
    {

        public SynchronizeData SyncData { get; private set; }
        public EventStatus ResultStatus { get; private set; }
        public String ErrorString { get; private set; }

        public struct ErrorObject
        {
            public string Message;

            /// <summary>
            /// SSN UID
            /// </summary>
            public uint Id;

            //public bool IsError;
        }
        public List<ErrorObject> Errors { get; set; }
        public List<ErrorObject> Warnings { get; set; }

        public GraphUpdateReadyEventArgs(SynchronizeData syncData) : this(syncData, EventStatus.OK, null)
        {
        }

        public GraphUpdateReadyEventArgs(SynchronizeData syncData, EventStatus resultStatus, String errorString)
        {
            this.SyncData = syncData;
            this.ResultStatus = resultStatus;
            this.ErrorString = errorString;

            if (string.IsNullOrEmpty(this.ErrorString))
                this.ErrorString = "";

            Errors = new List<ErrorObject>();
            Warnings = new List<ErrorObject>();
        }
    }

    public class NodesToCodeCompletedEventArgs : EventArgs
    {
        public List<uint> InputNodeIds { get; private set; }
        public List<SnapshotNode> OutputNodes { get; private set; }
        public EventStatus ResultStatus { get; private set; }
        public String ErrorString { get; private set; }

        public NodesToCodeCompletedEventArgs(List<uint> inputNodeIds,
            List<SnapshotNode> outputNodes, EventStatus resultStatus, String errorString)
        {
            this.InputNodeIds = inputNodeIds;
            this.OutputNodes = outputNodes;
            this.ResultStatus = resultStatus;
            this.ErrorString = errorString;
        }
    }

    public delegate void NodeValueReadyEventHandler(object sender, NodeValueReadyEventArgs e);
    public delegate void GraphUpdateReadyEventHandler(object sender, GraphUpdateReadyEventArgs e);
    public delegate void NodesToCodeCompletedEventHandler(object sender, NodesToCodeCompletedEventArgs e);

    public interface ILiveRunner
    {
        void UpdateGraph(GraphToDSCompiler.SynchronizeData syncData);
        void BeginUpdateGraph(GraphToDSCompiler.SynchronizeData syncData);
        void BeginConvertNodesToCode(List<SnapshotNode> snapshotNodes);

        void BeginQueryNodeValue(uint nodeId);
        ProtoCore.Mirror.RuntimeMirror QueryNodeValue(uint nodeId);
        ProtoCore.Mirror.RuntimeMirror QueryNodeValue(string nodeName);
        void BeginQueryNodeValue(List<uint> nodeIds);

        event NodeValueReadyEventHandler NodeValueReady;
        event GraphUpdateReadyEventHandler GraphUpdateReady;
        event NodesToCodeCompletedEventHandler NodesToCodeCompleted;
    }

    public class LiveRunner : ILiveRunner
    {
        /// <summary>
        ///  These are configuration parameters passed by host application to be consumed by geometry library and persistent manager implementation. 
        /// </summary>
        public class Options
        {
            /// <summary>
            /// The configuration parameters that needs to be passed to
            /// different applications.
            /// </summary>
            public Dictionary<string, object> PassThroughConfiguration;

            /// <summary>
            /// The path of the root graph/module
            /// </summary>
            public string RootModulePathName;

            /// <summary>
            /// List of search directories to resolve any file reference
            /// </summary>
            public List<string> SearchDirectories;

            /// <summary>
            /// If the Interpreter mode is true, the LiveRunner takes in code statements as input strings
            /// and not SyncData
            /// </summary>
            public bool InterpreterMode = false;
        }

        private abstract class Task
        {
            protected LiveRunner runner;
            protected Task(LiveRunner runner)
            {
                this.runner = runner;
            }
            public abstract void Execute();
        }

        private class NodeValueRequestTask : Task
        {
            private uint nodeId;
            public NodeValueRequestTask(uint nodeId, LiveRunner runner) : base(runner)
            {
                this.nodeId = nodeId;
            }

            public override void Execute()
            {
                lock (runner.operationsMutex)
                {
                     if (runner.NodeValueReady != null) // If an event handler is registered.
                     {
                         NodeValueReadyEventArgs retArgs = null;

                        try
                        {

                            // Now that background worker is free, get the value...
                            ProtoCore.Mirror.RuntimeMirror mirror = runner.InternalGetNodeValue(nodeId);
                            if (null != mirror)
                            {
                                retArgs = new NodeValueReadyEventArgs(mirror, nodeId);

                                System.Diagnostics.Debug.WriteLine("Node {0} => {1}",
                                    nodeId.ToString("x8"), mirror.GetData().GetStackValue());
                            }
                            else
                            {
                                retArgs = new NodeValueNotAvailableEventArgs(nodeId);
                            }

                        }
                        catch (Exception e)
                        {
                            retArgs = new NodeValueReadyEventArgs(null, nodeId, EventStatus.Error,  e.ToString());
                        }

                        if (null != retArgs)
                        {
                            runner.NodeValueReady(this, retArgs); // Notify all listeners (e.g. UI).
                        }
                    }
                }
            }
        }

        private class PropertyChangedTask : Task
        {
            public PropertyChangedTask(LiveRunner runner, GraphNode graphNode) 
                : base(runner)
            {
                objectCreationGraphNode = graphNode;
            }

            public override void Execute()
            {
                if (!objectCreationGraphNode.propertyChanged)
                { 
                    return;
                }
                objectCreationGraphNode.propertyChanged = false;
                UpdateNodeRef updateNode = objectCreationGraphNode.updateNodeRefList[0];

                GraphUpdateReadyEventArgs retArgs = null;
                lock (runner.operationsMutex)
                {
                    try
                    {
                        // @keyu: graph nodes may have been recreated caused of
                        // some update on the UI so that we have to find out
                        // new graph code that create this ffi object.
                        var graph = runner.runnerCore.DSExecutable.instrStreamList[0].dependencyGraph;
                        var graphnodes = graph.GetGraphNodesAtScope(this.objectCreationGraphNode.classIndex, this.objectCreationGraphNode.procIndex);
                        foreach (var graphnode in graphnodes)
                        {
                            if ((graphnode == objectCreationGraphNode) ||
                                (graphnode.updateNodeRefList.Count == 1 &&
                                 updateNode.IsEqual(graphnode.updateNodeRefList[0])))
                            {
                                graphnode.propertyChanged = true;
                                break;
                            }
                        }

                        runner.ResetVMForExecution();
                        runner.Execute();

                        var modfiedGuidList = runner.GetModifiedGuidList();
                        runner.ResetModifiedSymbols();
                        var syncDataReturn = runner.CreateSynchronizeDataForGuidList(modfiedGuidList);
                        retArgs = new GraphUpdateReadyEventArgs(syncDataReturn);
                    }
                    catch (Exception e)
                    {
                        retArgs = new GraphUpdateReadyEventArgs(new SynchronizeData(), 
                                                                EventStatus.Error, 
                                                                e.Message);
                    }
                }

                if (runner.GraphUpdateReady != null)
                {
                    runner.GraphUpdateReady(this, retArgs); 
                }
            }

            public GraphNode objectCreationGraphNode { get; set; }
        }

        private Dictionary<uint, string> GetModifiedGuidList()
        {
            // Retrieve the actual modified nodes 
            // Execution is complete, get all the modified guids 
            // Get the modified symbol names from the VM
            List<string> modifiedNames = this.runnerCore.Rmem.GetModifiedSymbolString();
            Dictionary<uint, string> modfiedGuidList = new Dictionary<uint, string>();
            foreach (string name in modifiedNames)
            {
                // Get the uid of the modified symbol
                if (this.graphCompiler.mapModifiedName.ContainsKey(name))
                {
                    uint id = this.graphCompiler.mapModifiedName[name];
                    if (!modfiedGuidList.ContainsKey(id))
                    {
                        // Append the modified guid into the modified list
                        modfiedGuidList.Add(this.graphCompiler.mapModifiedName[name], name);
                    }
                }
            }
            return modfiedGuidList;
        }

        private void ResetModifiedSymbols()
        {
             this.runnerCore.Rmem.ResetModifedSymbols();
        }

        private SynchronizeData CreateSynchronizeDataForGuidList(Dictionary<uint, string> modfiedGuidList)
        {
            Dictionary<uint, SnapshotNode> modifiedGuids = new Dictionary<uint, SnapshotNode>();
            SynchronizeData syncDataReturn = new SynchronizeData();

            if (modfiedGuidList != null)
            {
                //foreach (uint guid in modfiedGuidList)
                foreach (var kvp in modfiedGuidList)
                {
                    // Get the uid recognized by the graphIDE
                    uint guid = kvp.Key;
                    string name = kvp.Value;
                    SnapshotNode sNode = new SnapshotNode(this.graphCompiler.GetRealUID(guid), SnapshotNodeType.Identifier, name);
                    if (!modifiedGuids.ContainsKey(sNode.Id))
                    {
                        modifiedGuids.Add(sNode.Id, sNode);
                    }
                }

                foreach (KeyValuePair<uint, SnapshotNode> kvp in modifiedGuids)
                    syncDataReturn.ModifiedNodes.Add(kvp.Value);
            }

            return syncDataReturn;
        }

        private class UpdateGraphTask : Task
        {
            private SynchronizeData syncData;
            public UpdateGraphTask(SynchronizeData syncData, LiveRunner runner) : base(runner)
            {
                this.syncData = syncData;
                
            }

            public override void Execute()
            {
                GraphUpdateReadyEventArgs retArgs;

                lock (runner.operationsMutex)
                {
                    try
                    {
                        string code = null;
                        runner.SynchronizeInternal(syncData, out code);

                        var modfiedGuidList = runner.GetModifiedGuidList();
                        runner.ResetModifiedSymbols(); 
                        var syncDataReturn = runner.CreateSynchronizeDataForGuidList(modfiedGuidList);

                        retArgs = null;

                        ReportErrors(code, syncDataReturn, modfiedGuidList, ref retArgs);

                        //ReportBuildErrorsAndWarnings(code, syncDataReturn, modfiedGuidList, ref retArgs);
                        //ReportRuntimeWarnings(code, syncDataReturn, modfiedGuidList, ref retArgs);

                        if(retArgs == null)
                            retArgs = new GraphUpdateReadyEventArgs(syncDataReturn);
                    }
                    // Any exceptions that are caught here are most likely from the graph compiler
                    catch (Exception e)
                    {
                        retArgs = new GraphUpdateReadyEventArgs(syncData, EventStatus.Error, e.Message);
                    }
                }

                if (runner.GraphUpdateReady != null)
                {                    
                    runner.GraphUpdateReady(this, retArgs); // Notify all listeners (e.g. UI).
                }
            }

            private void ReportErrors(string code, SynchronizeData syncDataReturn, Dictionary<uint, string> modifiedGuidList, ref GraphUpdateReadyEventArgs retArgs)
            {
                Dictionary<ulong, ProtoCore.Core.ErrorEntry> errorMap = runner.runnerCore.LocationErrorMap;

                if (errorMap.Count == 0)
                    return;

                retArgs = new GraphUpdateReadyEventArgs(syncDataReturn);
                foreach (var kvp in errorMap)
                {
                    ProtoCore.Core.ErrorEntry err = kvp.Value;
                    string msg = err.Message;
                    int lineNo = err.Line;

                    // If error is a Build error
                    if (err.BuildId != ProtoCore.BuildData.WarningID.kDefault)
                    {
                        // Error comes from imported DS file
                        if (!string.IsNullOrEmpty(err.FileName))
                        {
                            msg += " At line " + err.Line + ", column " + err.Col + ", in " + Path.GetFileName(err.FileName);
                            if (err.Type == ProtoCore.Core.ErrorType.Error)
                            {
                                retArgs.Errors.Add(new GraphUpdateReadyEventArgs.ErrorObject { Message = msg, Id = 0 });
                            }
                            else if (err.Type == ProtoCore.Core.ErrorType.Warning)
                            {
                                retArgs.Warnings.Add(new GraphUpdateReadyEventArgs.ErrorObject { Message = msg, Id = 0 });
                            }
                            continue;
                        }

                    }

                    string varName = GetVarNameFromCode(lineNo, code);

                    // Errors
                    if (err.Type == ProtoCore.Core.ErrorType.Error)
                    {
                        // TODO: How can the lineNo be invalid ?
                        if (lineNo == ProtoCore.DSASM.Constants.kInvalidIndex || varName == null)
                        {
                            retArgs.Errors.Add(new GraphUpdateReadyEventArgs.ErrorObject { Message = msg, Id = 0 });
                            continue;
                        }

                        foreach (var pair in runner.graphCompiler.mapModifiedName)
                        {
                            string name = pair.Key;
                            if (name.Equals(varName))
                            {
                                uint guid = pair.Value;
                                retArgs.Errors.Add(new GraphUpdateReadyEventArgs.ErrorObject { Message = msg, Id = runner.graphCompiler.GetRealUID(guid) });
                                break;
                            }

                        }
                    }
                    else if(err.Type == ProtoCore.Core.ErrorType.Warning) // Warnings
                    {
                        // TODO: How can the lineNo be invalid ?
                        if (lineNo == ProtoCore.DSASM.Constants.kInvalidIndex || varName == null)
                        {
                            retArgs.Warnings.Add(new GraphUpdateReadyEventArgs.ErrorObject { Message = msg, Id = 0 });
                            continue;
                        }

                        foreach (var pair in modifiedGuidList)
                        {
                            // Get the uid recognized by the graphIDE                            
                            string name = pair.Value;
                            if (name.Equals(varName))
                            {
                                uint guid = pair.Key;
                                retArgs.Warnings.Add(new GraphUpdateReadyEventArgs.ErrorObject { Message = msg, Id = runner.graphCompiler.GetRealUID(guid) });
                                break;
                            }
                        }
                        if (retArgs.Warnings.Count == 0)
                        {
                            foreach (var pair in runner.graphCompiler.mapModifiedName)
                            {
                                string name = pair.Key;
                                if (name.Equals(varName))
                                {
                                    uint guid = pair.Value;
                                    retArgs.Warnings.Add(new GraphUpdateReadyEventArgs.ErrorObject { Message = msg, Id = runner.graphCompiler.GetRealUID(guid) });
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            private void ReportBuildErrorsAndWarnings(string code, SynchronizeData syncDataReturn, Dictionary<uint, string> modifiedGuidList, ref GraphUpdateReadyEventArgs retArgs)
            {
                //GraphUpdateReadyEventArgs retArgs = null;

                if (runner.runnerCore.BuildStatus.ErrorCount > 0)
                {
                    retArgs = new GraphUpdateReadyEventArgs(syncDataReturn);

                    foreach (var err in runner.runnerCore.BuildStatus.Errors)
                    {
                        string msg = err.Message;
                        int lineNo = err.Line;

                        // TODO: How can the lineNo be invalid ?
                        if (lineNo == ProtoCore.DSASM.Constants.kInvalidIndex)
                        {
                            retArgs.Errors.Add(new GraphUpdateReadyEventArgs.ErrorObject { Message = msg, Id = 0 });
                            continue;
                        }

                        string varName = GetVarNameFromCode(lineNo, code);

                        foreach (var ssnode in syncData.AddedNodes)
                        {
                            if (ssnode.Content.Contains(varName))
                            {
                                uint id = ssnode.Id;
                                
                                retArgs.Errors.Add(new GraphUpdateReadyEventArgs.ErrorObject { Message = msg, Id = id });
                                break;
                            }
                        }
                        if (retArgs.Errors.Count == 0)
                        {
                            foreach (var ssnode in syncData.ModifiedNodes)
                            {
                                if (ssnode.Content.Contains(varName))
                                {
                                    uint id = ssnode.Id;

                                    retArgs.Errors.Add(new GraphUpdateReadyEventArgs.ErrorObject { Message = msg, Id = id });
                                    break;
                                }
                            }
                        }
                    }
                }
                if (runner.runnerCore.BuildStatus.WarningCount > 0)
                {
                    if (retArgs == null)
                        retArgs = new GraphUpdateReadyEventArgs(syncDataReturn);

                    foreach (var warning in runner.runnerCore.BuildStatus.Warnings)
                    {
                        string msg = warning.msg;
                        int lineNo = warning.line;

                        // TODO: How can the lineNo be invalid ?
                        if (lineNo == ProtoCore.DSASM.Constants.kInvalidIndex)
                        {
                            retArgs.Warnings.Add(new GraphUpdateReadyEventArgs.ErrorObject { Message = msg, Id = 0 });
                            continue;
                        }

                        string varName = GetVarNameFromCode(lineNo, code);

                        // This array should be empty for Build errors
                        
                        /*foreach (var ssnode in syncDataReturn.ModifiedNodes)
                        {
                            if(ssnode.Content.Contains(varName))
                            {
                                uint id = ssnode.Id;

                                retArgs.Warnings.Add(new GraphUpdateReadyEventArgs.ErrorObject { Message = msg, Id = id });
                                break;
                            }
                        }*/
                        foreach (var kvp in modifiedGuidList)
                        {
                            // Get the uid recognized by the graphIDE
                            uint guid = kvp.Key;
                            string name = kvp.Value;
                            if (name.Equals(varName))
                            {
                                retArgs.Warnings.Add(new GraphUpdateReadyEventArgs.ErrorObject { Message = msg, Id = runner.graphCompiler.GetRealUID(guid) });
                                break;
                            }
                        }
                        
                        if(retArgs.Warnings.Count == 0)
                        {
                            LogWarningsFromInputNodes(retArgs, varName, msg);
                        }
                        
                    }
                }

            }

            void LogWarningsFromInputNodes(GraphUpdateReadyEventArgs retArgs, string varName, string msg)
            {
                
                foreach (var ssnode in syncData.AddedNodes)
                {
                    if (ssnode.Content.Contains(varName))
                    {
                        uint id = ssnode.Id;

                        retArgs.Warnings.Add(new GraphUpdateReadyEventArgs.ErrorObject { Message = msg, Id = id });
                        break;
                    }
                }

                if (retArgs.Warnings.Count == 0)
                {
                    foreach (var ssnode in syncData.ModifiedNodes)
                    {
                        if (ssnode.Content.Contains(varName))
                        {
                            uint id = ssnode.Id;

                            retArgs.Warnings.Add(new GraphUpdateReadyEventArgs.ErrorObject { Message = msg, Id = id });
                            break;
                        }
                    }
                }
            }

            private void ReportRuntimeWarnings(string code, SynchronizeData syncDataReturn, Dictionary<uint, string> modifiedGuidList, ref GraphUpdateReadyEventArgs retArgs)
            {
                //GraphUpdateReadyEventArgs retArgs = null;

                if (runner.runnerCore.RuntimeStatus.Warnings.Count > 0)
                {
                    if(retArgs == null)
                        retArgs = new GraphUpdateReadyEventArgs(syncDataReturn);

                    foreach (var err in runner.runnerCore.RuntimeStatus.Warnings)
                    {
                        string msg = err.message;
                        int lineNo = err.Line;
                        
                        // TODO: How can the lineNo be invalid ?
                        if (lineNo == ProtoCore.DSASM.Constants.kInvalidIndex)
                        {
                            retArgs.Warnings.Add(new GraphUpdateReadyEventArgs.ErrorObject { Message = msg, Id = 0 });
                            continue;
                        }

                        string varName = GetVarNameFromCode(lineNo, code);

                        foreach (var kvp in modifiedGuidList)
                        {
                            // Get the uid recognized by the graphIDE
                            uint guid = kvp.Key;
                            string name = kvp.Value;
                            if(name.Equals(varName))
                            {
                                retArgs.Warnings.Add(new GraphUpdateReadyEventArgs.ErrorObject { Message = msg, Id =  runner.graphCompiler.GetRealUID(guid)});
                                break;
                            }
                        }

                        if (retArgs.Warnings.Count == 0)
                        {
                            LogWarningsFromInputNodes(retArgs, varName, msg);
                        }
                    }
                }

            }

            private string GetVarNameFromCode(int lineNo, string code)
            {
                string varName = null;

                // Search the code using the input line no.
                /*string[] lines = code.Split('\n');
                string stmt = "";
                for (int i = lineNo - 1; i < lines.Length; ++i)
                {
                    stmt += lines[i];
                }

                List<ProtoCore.AST.Node> nodes = GraphUtilities.ParseCodeBlock(stmt);
                 
                // The first node must be a binary expressions
                ProtoCore.AST.AssociativeAST.BinaryExpressionNode ben = nodes[0] as ProtoCore.AST.AssociativeAST.BinaryExpressionNode;*/

                // Search for the binary expression in the input code which contains the warning
                ProtoCore.AST.AssociativeAST.BinaryExpressionNode ben = null;
                Validity.Assert(runner.runnerCore.AstNodeList != null);
                foreach (var node in runner.runnerCore.AstNodeList)
                {
                    if (node is ProtoCore.AST.AssociativeAST.BinaryExpressionNode)
                    {
                        if (lineNo >= node.line && lineNo <= node.endLine)
                        {
                            ben = node as ProtoCore.AST.AssociativeAST.BinaryExpressionNode;
                            break;
                        }
                    }
                }
                
                if (ben != null)
                {
                    ProtoCore.AST.AssociativeAST.IdentifierNode lhs = ben.LeftNode as ProtoCore.AST.AssociativeAST.IdentifierNode;
                    //Validity.Assert(lhs != null);
                    if (lhs != null)
                    {
                        varName = lhs.Name;
                    }
                    else // lhs could be an IdentifierListNode in a CodeBlock
                    {
                        ProtoCore.AST.AssociativeAST.IdentifierListNode lhsIdent = ben.LeftNode as ProtoCore.AST.AssociativeAST.IdentifierListNode;
                        Validity.Assert(lhsIdent != null);
   
                        // Extract line of code corresponding to this Ast node and get its LHS string
                        string identstmt = ProtoCore.Utils.ParserUtils.ExtractStatementFromCode(code, lhsIdent);
                        int equalIndex = identstmt.IndexOf('=');
                        varName = ProtoCore.Utils.ParserUtils.GetLHSatAssignment(identstmt, equalIndex)[0]; 
                    }
                    
                }

                return varName;
            }

            
        }

        private class UpdateCmdLineInterpreterTask : Task
        {
            private string cmdLineString;
            public UpdateCmdLineInterpreterTask(string code, LiveRunner runner)
                : base(runner)
            {
                this.cmdLineString = code;
            }

            public override void Execute()
            {
                GraphUpdateReadyEventArgs retArgs = null;

                lock (runner.operationsMutex)
                {
                    try
                    {
                        string code = null;
                        runner.SynchronizeInternal(code);

                        var modfiedGuidList = runner.GetModifiedGuidList();
                        runner.ResetModifiedSymbols();
                        var syncDataReturn = runner.CreateSynchronizeDataForGuidList(modfiedGuidList);

                        retArgs = null;

                        // TODO: Implement ReportErrors override to report errors for command line statements
                        //ReportErrors(code, syncDataReturn, modfiedGuidList, ref retArgs);
                        
                        if (retArgs == null)
                            retArgs = new GraphUpdateReadyEventArgs(syncDataReturn);
                    }
                    // Any exceptions that are caught here are most likely from the graph compiler
                    catch (Exception e)
                    {
                        //retArgs = new GraphUpdateReadyEventArgs(syncData, EventStatus.Error, e.Message);
                    }
                }

                if (runner.GraphUpdateReady != null)
                {
                    runner.GraphUpdateReady(this, retArgs); // Notify all listeners (e.g. UI).
                }
            }

        }

        private class ConvertNodesToCodeTask : Task
        {
            private List<SnapshotNode> snapshotNodes;
            public ConvertNodesToCodeTask(List<SnapshotNode> snapshotNodes, LiveRunner runner)
                : base(runner)
            {
                if (null == snapshotNodes || (snapshotNodes.Count <= 0))
                    throw new ArgumentException("snapshotNodes", "Invalid SnapshotNode list (35CA7759D0C9)");

                this.snapshotNodes = snapshotNodes;
            }

            public override void Execute()
            {
                NodesToCodeCompletedEventArgs args = null;

                // Gather a list of node IDs to be sent back.
                List<uint> inputNodeIds = new List<uint>();
                foreach (SnapshotNode node in snapshotNodes)
                    inputNodeIds.Add(node.Id);

                lock (runner.operationsMutex)
                {
                    try
                    {
                        // Do the thing you do...
                        List<SnapshotNode> outputNodes = GraphToDSCompiler.GraphUtilities.NodeToCodeBlocks(snapshotNodes, runner.graphCompiler);
                        args = new NodesToCodeCompletedEventArgs(inputNodeIds,
                            outputNodes, EventStatus.OK, "Yay, it works!");
                    }
                    catch (Exception exception)
                    {
                        args = new NodesToCodeCompletedEventArgs(inputNodeIds,
                            null, EventStatus.Error, exception.Message);
                    }
                }

                // Notify the listener (e.g. UI) of the completion.
                if (null != runner.NodesToCodeCompleted)
                    runner.NodesToCodeCompleted(this, args);
            }
        }

        private ProtoScriptTestRunner runner;
        private ProtoRunner.ProtoVMState vmState;
        private GraphToDSCompiler.GraphCompiler graphCompiler;
        private ProtoCore.Core runnerCore = null;
        private ProtoLanguage.CompileStateTracker compileState = null;
        private ProtoCore.Options coreOptions = null;
        private Options executionOptions = null;
        private bool syncCoreConfigurations = false;
        private int deltaSymbols = 0;
        private ProtoCore.CompileTime.Context staticContext = null;

        private readonly Object operationsMutex = new object();

        private Queue<Task> taskQueue; 

        private Thread workerThread;

        public LiveRunner()
        {
            InitRunner( new Options());
        }

        
        public LiveRunner(Options options)
        {
            InitRunner(options);
        }
        public GraphToDSCompiler.GraphCompiler GetCurrentGraphCompilerInstance()
        { 
            return graphCompiler;
        }
        private void InitRunner(Options options)
        {

            graphCompiler = GraphToDSCompiler.GraphCompiler.CreateInstance();
            graphCompiler.SetCore(GraphUtilities.GetCompilationState());
            runner = new ProtoScriptTestRunner();

            executionOptions = options;
            InitOptions();
            InitCore();


            taskQueue = new Queue<Task>();

            workerThread = new Thread(new ThreadStart(TaskExecMethod));
            

            workerThread.IsBackground = true;
            workerThread.Start();

            staticContext = new ProtoCore.CompileTime.Context();
        }

        private void InitOptions()
        {

            // Build the options required by the core
            Validity.Assert(coreOptions == null);
            coreOptions = new ProtoCore.Options();
            coreOptions.GenerateExprID = true;
            coreOptions.IsDeltaExecution = true;
            coreOptions.BuildOptErrorAsWarning = true;
            
            coreOptions.WebRunner = false;
            coreOptions.ExecutionMode = ProtoCore.ExecutionMode.Serial;
            //coreOptions.DumpByteCode = true;
            //coreOptions.Verbose = true;

            // This should have been set in the consturctor
            Validity.Assert(executionOptions != null); 
        }

        private void InitCore()
        {
            Validity.Assert(coreOptions != null);

            // Comment Jun:
            // It must be guaranteed that in delta exeuction, expression id's must not be autogerated
            // expression Id's must be propagated from the graphcompiler to the DS codegenerators
            //Validity.Assert(coreOptions.IsDeltaExecution && !coreOptions.GenerateExprID);

            runnerCore = new ProtoCore.Core(coreOptions);
            
            SyncCoreConfigurations(runnerCore, executionOptions);

            runnerCore.Executives.Add(ProtoCore.Language.kAssociative, new ProtoAssociative.Executive(runnerCore));
            runnerCore.Executives.Add(ProtoCore.Language.kImperative, new ProtoImperative.Executive(runnerCore));
            runnerCore.FFIPropertyChangedMonitor.FFIPropertyChangedEventHandler += FFIPropertyChanged;
            vmState = null;
        }

        private void FFIPropertyChanged(FFIPropertyChangedEventArgs arg)
        {
            lock (taskQueue)
            {
                taskQueue.Enqueue(new PropertyChangedTask(this, arg.hostGraphNode));
            }
        }

        private static void SyncCoreConfigurations(ProtoCore.Core core, Options options)
        {
            if (null == options)
                return;
            //update the root module path name, if set.
            if(!string.IsNullOrEmpty(options.RootModulePathName))
                core.Options.RootModulePathName = options.RootModulePathName;
            //then update the search path, if set.
            if(null != options.SearchDirectories)
                core.Options.IncludeDirectories = options.SearchDirectories;

            //Finally update the pass thru configuration values
            if (null == options.PassThroughConfiguration)
                return;
            foreach (var item in options.PassThroughConfiguration)
            {
                core.Configurations[item.Key] = item.Value;
            }
        }


        public void SetOptions(Options options)
        {
            executionOptions = options;
            syncCoreConfigurations = true; //request syncing the configuration
        }


        #region Public Live Runner Events

        public event NodeValueReadyEventHandler NodeValueReady = null;
        public event GraphUpdateReadyEventHandler GraphUpdateReady = null;
        public event NodesToCodeCompletedEventHandler NodesToCodeCompleted = null;

        #endregion



        /// <summary>
        /// Push new synchronization data, returns immediately and will
        /// trigger a GraphUpdateReady when the value when the execution
        /// is completed
        /// </summary>
        /// <param name="syncData"></param>
        public void BeginUpdateGraph(SynchronizeData syncData)
        {
            lock (taskQueue)
            {
                taskQueue.Enqueue(
                    new UpdateGraphTask(syncData, this));
            }

            //Todo(Luke) add a Monitor queue to prevent having to have the 
            //work poll
        }

        /// <summary>
        /// Async call from command-line interpreter to LiveRunner
        /// </summary>
        /// <param name="cmdLineString"></param>
        public void BeginUpdateCmdLineInterpreter(string cmdLineString)
        {
            lock (taskQueue)
            {
                taskQueue.Enqueue(
                    new UpdateCmdLineInterpreterTask(cmdLineString, this));
            }
        }

        /// <summary>
        /// Takes in a list of SnapshotNode objects, condensing them into one 
        /// or more SnapshotNode objects which caller can then turn into a more 
        /// compact representation of the former SnapshotNode objects.
        /// </summary>
        /// <param name="snapshotNodes">A list of source SnapshotNode objects 
        /// from which the resulting list of SnapshotNode is to be computed.
        /// </param>
        public void BeginConvertNodesToCode(List<SnapshotNode> snapshotNodes)
        {
            if (null == snapshotNodes || (snapshotNodes.Count <= 0))
                return; // Do nothing, there's no nodes to be converted.

            lock (taskQueue)
            {
                taskQueue.Enqueue(
                    new ConvertNodesToCodeTask(snapshotNodes, this));
            }
        }

        /// <summary>
        /// Query For a node value this will trigger a NodeValueReady callback
        /// when the value is available
        /// </summary>
        /// <param name="nodeId"></param>
        public void BeginQueryNodeValue(uint nodeId)
        {
            lock (taskQueue)
            {
                taskQueue.Enqueue(
                    new NodeValueRequestTask(nodeId, this));
            }
        }

        /// <summary>
        /// Query For a node value this will trigger a NodeValueReady callback
        /// when the value is available
        /// This version is more efficent than calling the BeginQueryNodeValue(uint)
        /// repeatedly 
        /// </summary>
        /// <param name="nodeId"></param>
        public void BeginQueryNodeValue(List<uint> nodeIds)
        {
            lock (taskQueue)
            {
                foreach (uint nodeId in nodeIds)
                {
                    taskQueue.Enqueue(
                        new NodeValueRequestTask(nodeId, this));
                }
            }
        }



        /// <summary>
        /// Query for a node value. This will block until the value is available.
        /// It will only serviced when all ASync calls have been completed
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public ProtoCore.Mirror.RuntimeMirror QueryNodeValue(uint nodeId)
        {
            while (true)
            {
                lock (taskQueue)
                {
                    //Spin waiting for the queue to be empty
                    if (taskQueue.Count == 0)
                    {

                        //No entries and we have the lock
                        //Synchronous query to get the node

                        return InternalGetNodeValue(nodeId);
                    }
                }
                Thread.Sleep(0);
            }

        }
        public ProtoCore.Mirror.RuntimeMirror QueryNodeValue(string nodeName)
        {
            while (true)
            {
                lock (taskQueue)
                {
                    //Spin waiting for the queue to be empty
                    if (taskQueue.Count == 0)
                    {

                        //No entries and we have the lock
                        //Synchronous query to get the node

                        return InternalGetNodeValue(nodeName);
                    }
                }
                Thread.Sleep(0);
            }

        }

        public void UpdateGraph(SynchronizeData syndData)
        {


            while (true)
            {
                lock (taskQueue)
                {
                    //Spin waiting for the queue to be empty
                    if (taskQueue.Count == 0)
                    {
                        string code = null;
                         SynchronizeInternal(syndData, out code);
                        return;

                    }
                }
                Thread.Sleep(0);
            }
            
        }

        public void UpdateCmdLineInterpreter(string code)
        {
            while (true)
            {
                lock (taskQueue)
                {
                    //Spin waiting for the queue to be empty
                    if (taskQueue.Count == 0)
                    {
                        SynchronizeInternal(code);
                        return;
                    }
                }
                Thread.Sleep(0);
            }
        }

        //Secondary thread
        private void TaskExecMethod()
        {
            while (true)
            {
                Task task = null;

                lock (taskQueue)
                {
                    if (taskQueue.Count > 0)
                        task = taskQueue.Dequeue();
                }

                if (task != null)
                {
                        task.Execute();
                    continue;
                    
                }

                Thread.Sleep(50);

            }

        }



        #region Internal Implementation


        private ProtoCore.Mirror.RuntimeMirror InternalGetNodeValue(string varname)
        {
            Validity.Assert(null != vmState);

            // Comment Jun: all symbols are in the global block as there is no notion of scoping the the graphUI yet.
            const int blockID = 0;

            return  vmState.LookupName(varname, blockID);
        }

        private ProtoCore.Mirror.RuntimeMirror InternalGetNodeValue(uint nodeId)
        {
            //ProtoCore.DSASM.Constants.kInvalidIndex tells the UpdateUIDForCodeblock to look for the lastindex for given codeblock
            nodeId = graphCompiler.UpdateUIDForCodeblock(nodeId, ProtoCore.DSASM.Constants.kInvalidIndex);
            Validity.Assert(null != vmState);
            string varname = graphCompiler.GetVarName(nodeId);
            if (string.IsNullOrEmpty(varname))
            {
                return null;
            }
            return InternalGetNodeValue(varname);
        }



        private ProtoLanguage.CompileStateTracker Compile(string code, out int blockId)
        {
            staticContext.SetData(code, new Dictionary<string, object>(), graphCompiler.ExecutionFlagList);

            compileState = runner.Compile(staticContext, runnerCore, out blockId);
            Validity.Assert(null != compileState);
            if (compileState.compileSucceeded)
            {
                // This is the boundary between compilestate and runtime core
                // Generate the executable
                compileState.GenerateExecutable();

                // Get the executable from the compileState
                runnerCore.DSExecutable = compileState.DSExecutable;

                // Update the symbol tables
                // TODO Jun: Expand to accomoadate the list of symbols
                staticContext.symbolTable = runnerCore.DSExecutable.runtimeSymbols[0];
            }
            return compileState;
        }


        private ProtoRunner.ProtoVMState Execute()
        {
            // runnerCore.GlobOffset is the number of global symbols that need to be allocated on the stack
            // The argument to Reallocate is the number of ONLY THE NEW global symbols as the stack needs to accomodate this delta
            int newSymbols = compileState.GlobOffset - deltaSymbols;

            // If there are lesser symbols to allocate for this run, then it means nodes were deleted.
            // TODO Jun: Determine if it is safe to just leave them in the global stack 
            //           as no symbols point to this memory location in the stack anyway
            if (newSymbols >= 0)
            {
                runnerCore.Rmem.ReAllocateMemory(newSymbols);
            }

            // Store the current number of global symbols
            deltaSymbols = compileState.GlobOffset;

            // Initialize the runtime context and pass it the execution delta list from the graph compiler
            ProtoCore.Runtime.Context runtimeContext = new ProtoCore.Runtime.Context();
            runtimeContext.execFlagList = graphCompiler.ExecutionFlagList;

            runner.Execute(runnerCore, runtimeContext, null);

            return new ProtoRunner.ProtoVMState(runnerCore);
        }

        private bool CompileAndExecute(string code)
        {
            // TODO Jun: Revisit all the Compile functions and remove the blockId out argument
            int blockId = ProtoCore.DSASM.Constants.kInvalidIndex;
            compileState = Compile(code, out blockId);
            Validity.Assert(null != compileState);
            if (compileState.compileSucceeded)
            {
                runnerCore.RunningBlock = blockId;
                vmState = Execute();
            }
            return compileState.compileSucceeded;
        }

        private void ResetVMForExecution()
        {
            runnerCore.ResetForExecution();
            compileState.ResetForExecution();
        }

        private void ResetVMForDeltaExecution()
        {
            runnerCore.ResetForDeltaExecution();
            compileState.ResetForExecution();
        }

        private void SynchronizeInternal(GraphToDSCompiler.SynchronizeData syncData, out string code)
        {
            Validity.Assert(null != runner);
            Validity.Assert(null != graphCompiler);

            if (syncData.AddedNodes.Count == 0 &&
                syncData.ModifiedNodes.Count == 0 &&
                syncData.RemovedNodes.Count == 0)
            {
                code = "";
                ResetVMForDeltaExecution();
                return;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Begin SyncInternal: {0}", syncData);
                GraphToDSCompiler.GraphBuilder g = new GraphBuilder(syncData, graphCompiler);
                code = g.BuildGraphDAG();

                System.Diagnostics.Debug.WriteLine("SyncInternal => " + code);

                //List<string> deletedVars = new List<string>();
                ResetVMForDeltaExecution();

                //Synchronize the core configuration before compilation and execution.
                if (syncCoreConfigurations)
                {
                    SyncCoreConfigurations(runnerCore, executionOptions);
                    syncCoreConfigurations = false;
                }

                bool succeeded = CompileAndExecute(code);
                if (succeeded)
                {
                    graphCompiler.ResetPropertiesForNextExecution();
                }
            }
        }

        private void SynchronizeInternal(string code)
        {
            Validity.Assert(null != runner);
            Validity.Assert(null != graphCompiler);

            if (string.IsNullOrEmpty(code))
            {
                code = "";
                ResetVMForDeltaExecution();
                return;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("SyncInternal => " + code);

                //List<string> deletedVars = new List<string>();
                ResetVMForDeltaExecution();

                //Synchronize the core configuration before compilation and execution.
                if (syncCoreConfigurations)
                {
                    SyncCoreConfigurations(runnerCore, executionOptions);
                    syncCoreConfigurations = false;
                }

                bool succeeded = CompileAndExecute(code);
                if (succeeded)
                {
                    graphCompiler.ResetPropertiesForNextExecution();
                }
            }
        }
        #endregion
    }
}
