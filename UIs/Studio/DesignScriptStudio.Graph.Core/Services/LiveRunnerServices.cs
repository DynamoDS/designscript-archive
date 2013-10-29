using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GraphToDSCompiler;
using ProtoCore.Utils;
using ProtoScript.Runners.Obsolete;
using System.Windows;
using System.Threading;
using ProtoCore.Mirror;
using ProtoCore.DSASM;
using Autodesk.DesignScript.Interfaces;

namespace DesignScriptStudio.Graph.Core.VMServices
{
    internal class LiveRunnerFactory
    {
        internal static ILiveRunner CreateLiveRunner(GraphController controller)
        {
            LiveRunner.Options opt = CreateLiveRunnerOptions(controller);
            return new LiveRunner(opt);
        }

        internal static LiveRunner.Options CreateLiveRunnerOptions(GraphController controller)
        {
            LiveRunner.Options opt = new LiveRunner.Options();
            if (null != CoreComponent.Instance.HostApplication)
            {
                // IDE-1964 Opening existing file not clearing the OGL window. Update the session key 
                // to match the latest one obtained directly from the "GraphController".
                opt.PassThroughConfiguration = CoreComponent.Instance.HostApplication.Configurations;
                opt.PassThroughConfiguration[ConfigurationKeys.SessionKey] = controller.Identifier.ToString();
            }

            opt.RootModulePathName = controller.FilePath;
            return opt;
        }
    }

    internal class Synchronizer
    {
        private ILiveRunner runner = null;
        private GraphController controller = null;

        #region Public Operational Methods

        internal Synchronizer(GraphController controller)
        {
            Validity.Assert(controller != null);

            this.controller = controller;
            this.controller.Saved += new GraphSavedHandler(OnGraphSaved);

            // If we're running in NUnit, then the LiveRunner will not be created 
            // since we do not yet have a way to test asynchronous calls and 
            // their return values (and currently calls into LiveRunner 
            // asynchronously creates some random failures in NUnit test cases).
            // 
            if (null != CoreComponent.Instance.HostApplication)
            {
                this.runner = LiveRunnerFactory.CreateLiveRunner(controller);
                runner.GraphUpdateReady += new GraphUpdateReadyEventHandler(OnRunnerGraphUpdateReady);
                runner.NodeValueReady += new NodeValueReadyEventHandler(OnRunnerNodeValueReady);
                runner.NodesToCodeCompleted += new NodesToCodeCompletedEventHandler(OnNodesToCodeCompleted);
            }
            else
            {
#if false
                // This part of the code will be enabled .
                this.runner = LiveRunnerFactory.CreateLiveRunner(controller);
                runner.GraphUpdateReady += OnNUnitGraphUpdateReady;
                runner.NodeValueReady += OnNUnitNodeValueReady;
                runner.NodesToCodeCompleted += OnNUnitNodesToCodeCompleted;
#endif
            }
        }

        internal void PushUpdate(SynchronizeData data)
        {
            if (null == this.runner) // Running in headless mode.
                return;
            if (Configurations.DumpDebugInfo)
                System.Diagnostics.Debug.WriteLine("Sync: " + data);

            Validity.Assert(data != null);

            try
            {
                runner.BeginUpdateGraph(data);
            }
            catch (Exception e)
            {
                //TODO Failsafe handler here
                System.Diagnostics.Debug.WriteLine("PushUpdate threw: " + e);
            }
        }

        internal void BeginQueryNodeValue(uint nodeId)
        {
            if (null == this.runner) // Running in headless mode.
                return;

            try
            {
                runner.BeginQueryNodeValue(nodeId);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("BeginQueryNodeValue threw: " + e);
            }
        }

        internal void BeginQueryNodeValue(List<VisualNode> nodes)
        {
            if (null == this.runner) // Running in headless mode.
                return;
            if (nodes == null || (nodes.Count <= 0))
                return;

            try
            {
                if (nodes.Count > 0)
                {
                    List<uint> nodeIds = new List<uint>();
                    foreach (VisualNode node in nodes)
                        nodeIds.Add(node.NodeId);

                    runner.BeginQueryNodeValue(nodeIds);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("BeginQueryNodeValue threw: " + e);
            }
        }

        internal void ConvertNodesToCode(List<SnapshotNode> snapshotNodes)
        {
            if (null == this.runner) // Running in headless mode.
                return;
            if (Configurations.DumpDebugInfo)
                System.Diagnostics.Debug.WriteLine("ConvertNodesToCode: " + snapshotNodes);

            Validity.Assert(snapshotNodes != null);

            try
            {
                runner.BeginConvertNodesToCode(snapshotNodes);
            }
            catch (Exception e)
            {
                //TODO Failsafe handler here
                System.Diagnostics.Debug.WriteLine("BeginConvertNodesToCode threw: " + e);
            }
        }

        #endregion

        #region Private Event Handlers

        private void OnRunnerGraphUpdateReady(object sender, GraphUpdateReadyEventArgs e)
        {
            if (null == CoreComponent.CurrentDispatcher)
                return; // Headless mode?

            if (Configurations.DumpDebugInfo)
                System.Diagnostics.Debug.WriteLine("OnRunnerGraphUpdateReady: " + e.SyncData);

            if (e.ResultStatus == EventStatus.Error)
            {
                //@TODO(Zx) Replace this with proper error propagation and notification
                System.Diagnostics.Debug.WriteLine("GraphUpdateReady reported: " + e);
                CoreComponent.CurrentDispatcher.Invoke(new Action(() =>
                {
                    CoreComponent.Instance.AddFeedbackMessage(ResourceNames.Error, e.ErrorString);
                }));
            }

            if (e.Errors.Count > 0)
            {
                List<GraphUpdateReadyEventArgs.ErrorObject> errors = e.Errors;
                foreach (GraphUpdateReadyEventArgs.ErrorObject error in errors)
                {
                    if (error.Id == 0)
                    {
                        CoreComponent.CurrentDispatcher.Invoke(new Action(() =>
                        {
                            CoreComponent.Instance.AddFeedbackMessage(ResourceNames.Error, e.ErrorString);
                        }));
                    }
                    else
                        ReportErrorStringOnUi(error.Id, error.Message);
                }
            }

            if (e.Warnings.Count > 0)
            {
                List<GraphUpdateReadyEventArgs.ErrorObject> warnings = e.Warnings;
                foreach (GraphUpdateReadyEventArgs.ErrorObject warning in warnings)
                {
                    if (warning.Id == 0)
                    {
                        CoreComponent.CurrentDispatcher.Invoke(new Action(() =>
                        {
                            CoreComponent.Instance.AddFeedbackMessage(ResourceNames.Warning, warning.Message);
                        }));
                    }
                    else
                        ReportWarningStringOnUi(warning.Id, warning.Message);
                }
            }

            List<uint> changedNodeIds = new List<uint>();
            foreach (SnapshotNode node in e.SyncData.ModifiedNodes)
            {
                if (IdGenerator.GetType(node.Id) != ComponentType.Node)
                    continue; // This isn't a visual node, don't bother.

                changedNodeIds.Add(node.Id);
            }

            foreach (SnapshotNode node in e.SyncData.AddedNodes)
            {
                if (IdGenerator.GetType(node.Id) != ComponentType.Node)
                    continue; // This isn't a visual node, don't bother.

                changedNodeIds.Add(node.Id);
            }

            if (0 < changedNodeIds.Count)
            {
                CoreComponent.CurrentDispatcher.Invoke(new Action(() =>
                {
                    if (RemoveNodesNotQualifiedForPreview(changedNodeIds))
                        runner.BeginQueryNodeValue(changedNodeIds);
                }));
            }
        }

        private void OnRunnerNodeValueReady(object sender, NodeValueReadyEventArgs e)
        {
            if (Configurations.DumpDebugInfo)
                System.Diagnostics.Debug.WriteLine("OnRunnerNodeValueReady: " + e.NodeId.ToString("x8"));
            //Notify host application regarding graph update.
            if (null != CoreComponent.Instance.HostApplication)
                CoreComponent.Instance.HostApplication.PostGraphUpdate();

            if (e.ResultStatus == EventStatus.Error)
            {
                //@TODO(Zx) Replace this with proper error propagation and notification
                System.Diagnostics.Debug.WriteLine("GraphUpdateReady reported: " + e);
                ReportErrorStringOnUi(e.NodeId, e.ErrorString);
                return;
            }
            else if (e is NodeValueNotAvailableEventArgs)
            {
                ReportErrorStringOnUi(e.NodeId, "");
                return;
            }

            // We want to marshal this back onto the UI thread for two things: node 
            // retrieval (as the user may be manipulating existing nodes) and actual 
            // rendering of the node (which can only be done on the UI thread).
            if (null != CoreComponent.CurrentDispatcher) // Headless mode?
            {
                CoreComponent.CurrentDispatcher.Invoke(new Action(() =>
                {
                    DisplayPreviewValueOnUi(e);
                }));
            }
        }

        private void OnNodesToCodeCompleted(object sender, NodesToCodeCompletedEventArgs e)
        {
            // Note: this method is executed (i.e. called) from a background thread.
            if (null == CoreComponent.CurrentDispatcher)
                return; // Headless mode?

            CoreComponent.CurrentDispatcher.Invoke(new Action(() =>
            {
                controller.ProcessNodesToCodeConversion(e.InputNodeIds, e.OutputNodes);
            }));
        }

        private void OnGraphSaved(object sender, EventArgs e)
        {
            if (null == this.runner) // Running in headless mode.
                return;
            GraphController controller = sender as GraphController;
            if (null != controller)
            {
                LiveRunner liveRunner = runner as LiveRunner;
                //Update the options with new path.
                liveRunner.SetOptions(LiveRunnerFactory.CreateLiveRunnerOptions(controller));
                GraphUtilities.SetRootModulePath(controller.FilePath);
            }
        }

        private void OnNUnitGraphUpdateReady(object sender, GraphUpdateReadyEventArgs e)
        {
            List<uint> changedNodeIds = new List<uint>();
            foreach (SnapshotNode node in e.SyncData.ModifiedNodes)
            {
                if (IdGenerator.GetType(node.Id) != ComponentType.Node)
                    continue; // This isn't a visual node, don't bother.

                changedNodeIds.Add(node.Id);
            }

            foreach (SnapshotNode node in e.SyncData.AddedNodes)
            {
                if (IdGenerator.GetType(node.Id) != ComponentType.Node)
                    continue; // This isn't a visual node, don't bother.

                changedNodeIds.Add(node.Id);
            }

            if (0 < changedNodeIds.Count)
                runner.BeginQueryNodeValue(changedNodeIds);
        }

        private void OnNUnitNodeValueReady(object sender, NodeValueReadyEventArgs e)
        {
            PreviewRequest request = controller.GetPreviewRequest();
            request.AppendPreviewValue(e.NodeId, e.RuntimeMirror.GetData());
        }

        private void OnNUnitNodesToCodeCompleted(object sender, NodesToCodeCompletedEventArgs e)
        {
        }

        #endregion

        #region Private Class Helper Methods

        private void ReportErrorStringOnUi(uint nodeId, string errorString)
        {
            if (null == CoreComponent.CurrentDispatcher)
                return; // Headless mode?

            CoreComponent.CurrentDispatcher.Invoke(new Action(() =>
            {
                if (IdGenerator.GetType(nodeId) == ComponentType.Node)
                {
                    VisualNode visualNode = controller.GetVisualNode(nodeId);
                    if (visualNode != null)
                    {
                        if (!String.IsNullOrEmpty(errorString))
                        {
                            visualNode.SetErrorValue(errorString, ErrorType.Semantic, true);
                        }
                        visualNode.SetPreviewValue(null);
                    }
                }

                // See if we have the first line-break...
                errorString = errorString.Replace("\r\n", "\n");
                int lineBreakIndex = errorString.IndexOf('\n');
                if (lineBreakIndex >= 0) // ... if we do...
                {
                    // See if we have a second line-break character...
                    lineBreakIndex = errorString.IndexOf('\n', lineBreakIndex + 1);
                    if (lineBreakIndex >= 0) // ... if so, remove everything after it.
                        errorString = errorString.Substring(0, lineBreakIndex);
                }

                // Now, remove the only line-break left now (turning it into single line.
                errorString = errorString.Replace("\n", "");
                System.Diagnostics.Debug.WriteLine(errorString);
            }));
        }

        private void ReportWarningStringOnUi(uint nodeId, string warningString)
        {
            if (null == CoreComponent.CurrentDispatcher)
                return; // Headless mode?

            CoreComponent.CurrentDispatcher.Invoke(new Action(() =>
            {
                if (IdGenerator.GetType(nodeId) == ComponentType.Node)
                {
                    VisualNode visualNode = controller.GetVisualNode(nodeId);
                    if (visualNode != null)
                    {
                        if (!String.IsNullOrEmpty(warningString))
                        {
                            visualNode.SetErrorValue(warningString, ErrorType.Warning, true);
                        }
                        visualNode.SetPreviewValue(null);
                    }
                }

                // See if we have the first line-break...
                warningString = warningString.Replace("\r\n", "\n");
                int lineBreakIndex = warningString.IndexOf('\n');
                if (lineBreakIndex >= 0) // ... if we do...
                {
                    // See if we have a second line-break character...
                    lineBreakIndex = warningString.IndexOf('\n', lineBreakIndex + 1);
                    if (lineBreakIndex >= 0) // ... if so, remove everything after it.
                        warningString = warningString.Substring(0, lineBreakIndex);
                }

                // Now, remove the only line-break left now (turning it into single line.
                warningString = warningString.Replace("\n", "");
                System.Diagnostics.Debug.WriteLine(warningString);
            }));
        }

        private void DisplayPreviewValueOnUi(NodeValueReadyEventArgs e)
        {
            VisualNode visualNode = controller.GetVisualNode(e.NodeId);
            if (null == visualNode || (false != visualNode.Error))
                return; // Node not found, or it is in error state.

            MirrorData mirrorData = e.RuntimeMirror.GetData();
            if (null == mirrorData)
                return;

            //set the returnType and assemblyusing mirror data
            Dictionary<string, List<string>> assemblyAndReturnTypes = e.RuntimeMirror.GetDynamicAssemblyType();

            string assembly = string.Empty;
            foreach (string assem in assemblyAndReturnTypes.Keys)
            {
                assembly += assem;
                assembly += ",";
            }
            if (string.IsNullOrEmpty(assembly) == false)
                assembly = assembly.TrimEnd(',');
            string returnTypes = string.Empty;
            foreach (List<string> returnType in assemblyAndReturnTypes.Values)
            {
                foreach (string type in returnType)
                {
                    returnTypes += type;
                    returnTypes += ",";
                }
            }
            if (string.IsNullOrEmpty(returnTypes) == false)
                returnTypes = returnTypes.TrimEnd(',');

            bool geometryPreviewFlag = false;
            bool textualPreviewFlag = false;

            if (visualNode.NodeStates.HasFlag(States.TextualPreview))
                textualPreviewFlag = true;
            else
                geometryPreviewFlag = true;

            // If user wants geometric preview and we are not running in headless mode.
            if (geometryPreviewFlag == true && (null != CoreComponent.CurrentDispatcher))
            {
                // If the geometric preview is enabled.
                if (!CoreComponent.Instance.GeometricPreviewEnabled)
                {
                    // Geometric preview is not enabled, show no preview placeholder.
                    visualNode.SetPreviewValue(CoreComponent.Instance.PreviewPlaceholder);
                }
                else
                {
                    List<IGraphicItem> graphicItems = mirrorData.GetGraphicsItems();
                    if (graphicItems == null || graphicItems.Count < 0)
                        textualPreviewFlag = true; // No graphics item, display text.
                    else
                    {
                        CoreComponent.CurrentDispatcher.BeginInvoke(new Action(() =>
                        {
                            // Start to request for a visual (e.g. geometry preview).
                            CoreComponent coreComponent = CoreComponent.Instance;
                            if (!coreComponent.RequestVisualization(controller.Identifier, e.NodeId, graphicItems))
                                DisplayPreviewFailureMessage(e.NodeId);
                        }));
                    }
                }
            }

            if (textualPreviewFlag == true)
            {
                //show textual preview
                // Fix: IDE-1263 Preview window shouldn't appear when there's nothing to preview.
                string preview = null;
                if (mirrorData.GetStackValue().optype != AddressType.Null)
                    preview = e.RuntimeMirror.GetStringData();

                visualNode.SetPreviewValue(preview);
            }

            visualNode.SetReturnTypeAndAssembly(returnTypes, assembly);
        }

        private void DisplayPreviewFailureMessage(uint nodeId)
        {
            if (null == CoreComponent.CurrentDispatcher)
                return; // Headless mode?

            CoreComponent.CurrentDispatcher.Invoke(new Action(() =>
            {
                VisualNode visualNode = controller.GetVisualNode(nodeId);
                if (null != visualNode && (false == visualNode.Error))
                    visualNode.SetPreviewValue("Framebuffer not supported");
            }));
        }

        private bool RemoveNodesNotQualifiedForPreview(List<uint> changedNodeIds)
        {
            if (null == changedNodeIds || (changedNodeIds.Count <= 0))
                return false;

            List<VisualNode> changedNodes = new List<VisualNode>();
            foreach (uint nodeId in changedNodeIds)
            {
                if (IdGenerator.GetType(nodeId) != ComponentType.Node)
                    continue;

                VisualNode node = controller.GetVisualNode(nodeId);

                // node may have been deleted/modified/updated on the UI when
                // the LiveRunner was busy with updating graph. Who knows...
                if (null == node)
                    continue;

                changedNodes.Add(node);
            }

            changedNodeIds.Clear();
            if (changedNodes.Count <= 0)
                return false;

            if (!RemoveNodesNotQualifiedForPreview(changedNodes))
                return false;

            foreach (VisualNode node in changedNodes)
            {
                if (node.NodeStates.HasFlag(States.PreviewHidden))
                    node.DisplayCollapsedPreview();
                else
                    changedNodeIds.Add(node.NodeId);
            }

            return (changedNodeIds.Count > 0);
        }

        public static bool RemoveNodesNotQualifiedForPreview(List<VisualNode> changedNodes)
        {
            List<VisualNode> retained = new List<VisualNode>();
            foreach (VisualNode node in changedNodes)
            {
                if (node.NodeStates.HasFlag(States.SuppressPreview))
                    continue; // The node decided to suppress its preview.

                if (node.VisualType == NodeType.Condensed
                    || node.VisualType == NodeType.Driver)
                    continue;

                // We don't need all input slots of a code block node to 
                // have input connections before the preview can be shown.
                if (node.VisualType == NodeType.CodeBlock)
                {
                    // Always generate preview for code block node.
                    retained.Add(node);
                    continue;
                }

                if (node.AllSlotsConnected(SlotType.Input))
                    retained.Add(node);
                else
                {
                    // Now the node does not have all its input slots 
                    // connected, we'll just remove the preview.
                    node.SetPreviewValue(null);
                }
            }

            changedNodes.Clear();
            changedNodes.AddRange(retained);
            return changedNodes.Count > 0;
        }

        #endregion
    }
}
