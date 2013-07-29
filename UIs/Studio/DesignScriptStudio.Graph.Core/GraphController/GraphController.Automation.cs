using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DesignScriptStudio.Graph.Core
{
    internal class PreviewRequest
    {
        #region Private Class Data Members

        private object syncRoot = new object();
        private AutoResetEvent autoEvent = null;
        private List<uint> requestedNodes = null;
        private Dictionary<uint, ProtoCore.Mirror.MirrorData> results = null;

        #endregion

        #region Public Operational Methods

        internal PreviewRequest()
        {
            autoEvent = new AutoResetEvent(false);
            requestedNodes = new List<uint>();
            results = new Dictionary<uint, ProtoCore.Mirror.MirrorData>();
        }

        /// <summary>
        /// An NUnit test case calls this method (indirectly through GraphController) 
        /// for a list of node IDs whose values it is interested in obtaining. The 
        /// PreviewRequest object tries to satisfy this request and wait for all the 
        /// values for these nodes to return from the LiveRunner before giving up.
        /// </summary>
        /// <param name="nodeIds">A list of node IDs whose values are to be obtained.
        /// </param>
        internal void AddNodeIds(uint[] nodeIds)
        {
            if (null == nodeIds || (nodeIds.Length <= 0))
                throw new ArgumentNullException("nodeIds");

            lock (syncRoot)
            {
                requestedNodes.AddRange(nodeIds);
            }
        }

        /// <summary>
        /// Whenever the LiveRunner.NodeValueReady event is raised, its handler calls 
        /// this method to store the resulting value (of the node) in the result list.
        /// </summary>
        /// <param name="nodeId">The node ID whose value is in 'data' parameter.</param>
        /// <param name="data">The value of the node indicated by 'nodeId'.</param>
        internal void AppendPreviewValue(uint nodeId, ProtoCore.Mirror.MirrorData data)
        {
            lock (syncRoot)
            {
                if (!requestedNodes.Contains(nodeId))
                    return; // The node value wasn't requested.

                requestedNodes.Remove(nodeId);
                results[nodeId] = data;

                // Here the consumer thread (presumably the LiveRunner) 
                // signals to unblock "WaitForCompletion", which is running in 
                // another thread (as part of "System.Threading.Tasks.Task").
                autoEvent.Set();
            }
        }

        /// <summary>
        /// An NUnit test case calls this to obtain the computed value for a node 
        /// whose ID is denoted by 'nodeId'.
        /// </summary>
        /// <param name="nodeId">The ID of a node whose value is to be retrieved.
        /// </param>
        /// <returns>Returns the StackValue for the 'nodeId'.</returns>
        internal ProtoCore.DSASM.StackValue GetNodeValue(uint nodeId)
        {
            lock (syncRoot)
            {
                ProtoCore.Mirror.MirrorData data = results[nodeId];
                return data.GetStackValue();
            }
        }

        /// <summary>
        /// An NUnit test case calls this method (indirectly through GraphController)
        /// and wait till values of all the requested nodes are computed and returned 
        /// by the LiveRunner. This method call blocks on the calling thread until 
        /// the specified duration ellapsed, or until values of all the requested 
        /// nodes are fulfilled.
        /// </summary>
        /// <param name="millisecondsTimeout">The amount of time to wait (in 
        /// milliseconds) before giving up and returning to the caller.</param>
        /// <returns>Returns true if values of all requested nodes could be obtained,
        /// or false if the wait timed out.</returns>
        internal bool WaitForCompletion(int millisecondsTimeout)
        {
            while (true)
            {
                // Wait few seconds to break free if timed out.
                if (!autoEvent.WaitOne(millisecondsTimeout))
                    return false;

                lock (syncRoot) // Gain access to the shared dictionary.
                {
                    // All requested results are fulfilled.
                    if (requestedNodes.Count <= 0)
                        break;
                }
            }

            return true;
        }

        #endregion

    }

    partial class GraphController : IGraphController
    {
        #region Internal Automation Supporting Methods

        PreviewRequest previewRequest = null;

        /// <summary>
        /// An NUnit test case calls this method to register its interest in 
        /// obtaining the values of all the nodes represented by 'nodeIds'.
        /// </summary>
        /// <param name="nodeIds">Node IDs of the nodes whose values are to be 
        /// waited upon.</param>
        internal void RegisterPreviewRequest(uint[] nodeIds)
        {
            if (null == nodeIds || (nodeIds.Length <= 0))
                throw new ArgumentNullException("nodeIds");
            if (null != this.previewRequest)
                throw new InvalidOperationException("'RegisterPreviewRequest' called twice.");

            this.previewRequest = new PreviewRequest();
            this.previewRequest.AddNodeIds(nodeIds);
        }

        internal PreviewRequest GetPreviewRequest()
        {
            return this.previewRequest;
        }

        /// <summary>
        /// An NUnit test case calls this method to wait for node values to be computed.
        /// This method internally creates an asynchronous Task and blocks on its 'Result' 
        /// property until either the specified duration elapsed, or all the requested node 
        /// values are computed.
        /// </summary>
        /// <param name="millisecondsTimeout">The amount of time this method is expected 
        /// to block (in milliseconds). If node values cannot be computed before this time 
        /// expires, then not all requested node values will be ready.</param>
        /// <returns>Returns true if all requested nodes have their values computed, or 
        /// false if the method timed out.</returns>
        internal bool WaitForPreviewRequestCompletion(int millisecondsTimeout)
        {
            if (millisecondsTimeout <= 0)
                throw new ArgumentException("Invalid time (05DAF7B65562)", "millisecondsTimeout");
            if (null == previewRequest)
                throw new InvalidOperationException("'RegisterPreviewRequest' was not called.");

            Task<bool> previewTask = Task.Factory.StartNew<bool>(() =>
            {
                return this.previewRequest.WaitForCompletion(millisecondsTimeout);
            });

            return previewTask.Result;
        }

        internal bool RunCommands(string inputCommands)
        {
            inputCommands = inputCommands.Replace("\r\n", "\n");
            char[] delimiter = new char[] { '\n' };
            string[] commands = inputCommands.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);

            if (null == commands || (commands.Length == 0))
                return false;

            foreach (string command in commands)
            {
                if (null == command)
                    continue;

                string trimmed = command.Trim();
                if (string.IsNullOrEmpty(trimmed))
                    continue;

                GraphCommand graphCommand = GraphCommand.FromString(trimmed);
                if (!RerouteToHandler(graphCommand))
                    return false;
            }

            return true;
        }

        private string DumpVmStatesToExternalFile()
        {
            if (null == this.sdc || (null == this.sdc.sdList))
                return String.Empty;

            string tempFilePath = Path.GetTempPath();
            string tempFileName = DateTime.Now.ToString("yyyyMMdd'-'HHmmss");
            tempFilePath += string.Format("snapshot-nodes-{0}.xml", tempFileName);

            using (TextWriter writer = File.CreateText(tempFilePath))
            {
                writer.Write(DumpVmStatesInternal());
            }

            return (File.Exists(tempFilePath) ? tempFilePath : string.Empty);
        }

        private string DumpVmStatesInternal()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine((this.sdc).Serialize());
            return builder.ToString();
        }

        private string DumpUiStatesToExternalFile(Exception exception)
        {
            if (null == visualHost || (!visualHost.IsInRecordingMode))
                throw new InvalidOperationException("'DumpRecordedStates' cannot be called at this time.");

            string tempFilePath = Path.GetTempPath();
            string tempFileName = DateTime.Now.ToString("yyyyMMdd'-'HHmmss");
            tempFilePath += string.Format("graph-states-{0}.log", tempFileName);

            using (TextWriter writer = File.CreateText(tempFilePath))
            {
                writer.Write(DumpStatesInternal(exception));
            }

            return (File.Exists(tempFilePath) ? tempFilePath : string.Empty);
        }

        private string DumpStatesInternal(Exception exception)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine("======== Recorded Commands =====================");
            foreach (string command in recordedCommands)
                builder.AppendLine(command);

            RuntimeStates runtimeStates = GetRuntimeStates();
            if (null != runtimeStates)
            {
                builder.AppendLine("\n======== Runtime State Data =======================");
                builder.AppendLine(runtimeStates.ToString());
            }

            builder.AppendLine("\n======== Node Collection =======================");
            foreach (var entry in nodeCollection)
            {
                IVisualNode node = entry.Value;
                builder.AppendLine(node.ToString() + "\n");
            }

            builder.AppendLine("======== Slot Collection =======================");
            foreach (var entry in slotCollection)
            {
                ISlot slot = entry.Value;
                builder.AppendLine(slot.ToString() + "\n");
            }

            builder.AppendLine("======== Edge Collection =======================");
            if (null != edgeController)
                builder.AppendLine(edgeController.ToString() + "\n");

            if (null != exception)
            {
                builder.AppendLine("======== Runtime Exception =====================");
                builder.AppendLine(exception.Message + "\n");
                if (!String.IsNullOrEmpty(exception.StackTrace))
                    builder.AppendLine(exception.StackTrace);
            }

            return builder.ToString();
        }

        #endregion
    }
}
