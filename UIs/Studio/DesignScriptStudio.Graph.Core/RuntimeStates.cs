using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DesignScriptStudio.Graph.Core
{
    class RuntimeStates : ISerializable
    {
        private enum Version { Version0, Current = Version0 }

        #region Class Data Members

        private RuntimeStates.Version version;
        private Dictionary<uint, List<string>> undefinedVarsTracker = null;
        private Dictionary<string, List<uint>> variableNodesMap = new Dictionary<string, List<uint>>();

        #endregion

        #region Public Interface Methods

        public bool Deserialize(IStorage storage)
        {
            if (storage == null)
                throw new ArgumentNullException("storage");

            uint signature = storage.ReadUnsignedInteger(FieldCode.RuntimeStatesSignature);
            if (signature != Configurations.RuntimeStatesSignature)
                throw new InvalidOperationException("Invalid input data");

            try
            {
                // Here we attempt to take a snapshot of what variables are currently 
                // defined in the system, and which is the node defining it.
                // 
                Dictionary<string, uint> oldDefinitions = null;
                if (null != this.undefinedVarsTracker)
                {
                    oldDefinitions = new Dictionary<string, uint>();
                    foreach (KeyValuePair<string, List<uint>> kvp in variableNodesMap)
                    {
                        List<uint> definingNodes = kvp.Value;
                        if (null != definingNodes && (definingNodes.Count > 0))
                            oldDefinitions.Add(kvp.Key, definingNodes[0]);
                    }
                }

                this.version = (RuntimeStates.Version)storage.ReadInteger(FieldCode.RuntimeStatesVersion);
                this.IsModified = storage.ReadBoolean(FieldCode.IsModified);

                int variableNodesMapCount = storage.ReadInteger(FieldCode.VariableNodesMapCount);
                this.variableNodesMap.Clear();
                for (int i = 0; i < variableNodesMapCount; i++)
                {
                    string key = storage.ReadString(FieldCode.VariableNodesMapKey);
                    List<uint> value = new List<uint>();
                    int valueCount = storage.ReadInteger(FieldCode.VariableNodesMapValueCount);
                    for (int j = 0; j < valueCount; j++)
                        value.Add(storage.ReadUnsignedInteger(FieldCode.VariableNodesMapValue));
                    this.variableNodesMap.Add(key, value);
                }

                // After deserialization is done, we'll move all the previously defined variables 
                // into the "undefinedVarsTracker". Note that we copy ALL of them into the tracker,
                // but some of them may still be defined after the "variableNodesMap" got updated.
                // This is because eventually these variables which are still in the system will 
                // be removed in the final pass in "EndDefinitionMonitor" call.
                // 
                if (null != oldDefinitions && (oldDefinitions.Count > 0))
                {
                    foreach (KeyValuePair<string, uint> oldDefinition in oldDefinitions)
                    {
                        string name = oldDefinition.Key;
                        uint nodeId = oldDefinition.Value;
                        if (!undefinedVarsTracker.ContainsKey(nodeId))
                            undefinedVarsTracker[nodeId] = new List<string>();

                        List<string> variables = undefinedVarsTracker[nodeId];
                        if (!variables.Contains(name))
                            variables.Add(name);
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n RuntimeStates deserialization failed.");
                return false;
            }
        }

        public bool Serialize(IStorage storage)
        {
            if (storage == null)
                throw new ArgumentNullException("storage");

            try
            {
                storage.WriteUnsignedInteger(FieldCode.RuntimeStatesSignature, Configurations.RuntimeStatesSignature);
                storage.WriteInteger(FieldCode.RuntimeStatesVersion, (int)(RuntimeStates.Version.Current));
                storage.WriteBoolean(FieldCode.IsModified, this.IsModified);

                storage.WriteInteger(FieldCode.VariableNodesMapCount, this.variableNodesMap.Count);
                foreach (KeyValuePair<string, List<uint>> kvp in this.variableNodesMap)
                {
                    storage.WriteString(FieldCode.VariableNodesMapKey, kvp.Key);
                    List<uint> values = kvp.Value.ToList<uint>();
                    storage.WriteInteger(FieldCode.VariableNodesMapValueCount, values.Count);
                    foreach (uint value in values)
                        storage.WriteUnsignedInteger(FieldCode.VariableNodesMapValue, value);
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n RuntimeStates serialization failed.");
                return false;
            }
        }

        public AuditStatus Audit()
        {
            return AuditStatus.NoChange;
        }

        #endregion

        #region Internal Class Properties

        internal bool IsModified { get; set; }

        #endregion

        #region Public Class Methods

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            if (null == variableNodesMap || (variableNodesMap.Count <= 0))
                builder.Append("No defined variables in the current graph.\n");
            else
            {
                builder.Append("Variable names and nodes defining them:\n\n");
                foreach (KeyValuePair<string, List<uint>> variable in variableNodesMap)
                {
                    builder.Append(string.Format("{0}: ", variable.Key));

                    foreach (uint nodeId in variable.Value)
                        builder.Append(string.Format("0x{0} ", nodeId.ToString("x8")));

                    builder.Append("\n");
                }
            }

            return builder.ToString();
        }

        #endregion

        #region Internal Class Methods

        internal RuntimeStates()
        {
            this.version = RuntimeStates.Version.Current;
            this.IsModified = false;
        }

        internal void BeginDefinitionMonitor()
        {
            if (null != undefinedVarsTracker)
                throw new InvalidOperationException("'EndDefinitionMonitor' was not called! (2F2B9369B571)");

            // "undefinedVarsTracker" is a map between a given nodeId (e.g. 0x1001) 
            // and the list of variables (that used to be defined in this 0x1001) 
            // being undefined in the node. For an example, consider the following:
            // 
            //      Initial CBN content: [ a = 2; b = 3; c = 4; ]
            //      Updated CBN content: [ c = 4; d = 5; e = 6; ]
            // 
            // In this case, both "a" and "b" are getting undefined (while leaving 
            // behind "c" here). So the "undefinedVarsTracker" map will have one entry 
            // that looks like this:
            // 
            //      undefinitions = { 0x1001, { a, b } }
            // 
            undefinedVarsTracker = new Dictionary<uint, List<string>>();
        }

        internal void EndDefinitionMonitor(out Dictionary<uint, List<string>> undefinedVariables)
        {
            undefinedVariables = null;

            if (null == undefinedVarsTracker)
                throw new InvalidOperationException("'BeginDefinitionMonitor' was not called! (EF73A04BAFB2)");

            if (undefinedVarsTracker.Count <= 0)
            {
                undefinedVarsTracker = null;
                return; // Nothing got undefined.
            }

            foreach (KeyValuePair<uint, List<string>> nodes in undefinedVarsTracker)
            {
                foreach (string variableName in nodes.Value) // For each of the variables.
                {
                    // If the variable is being defined by other nodes, then ignore it.
                    if (variableNodesMap.Keys.Contains(variableName))
                        continue; // Variable is still defined.

                    if (null == undefinedVariables)
                        undefinedVariables = new Dictionary<uint, List<string>>();

                    if (!undefinedVariables.ContainsKey(nodes.Key))
                        undefinedVariables.Add(nodes.Key, new List<string>());

                    List<string> variableNames = undefinedVariables[nodes.Key];
                    if (!variableNames.Contains(variableName))
                        variableNames.Add(variableName);
                }
            }

            undefinedVarsTracker = null; // Reset internal states.
        }

        internal string GenerateTempVariable(uint nodeId)
        {
            string tempVariable = string.Empty;
            if (nodeId == uint.MaxValue)
            {
                tempVariable = Configurations.TempVariablePrefix + Guid.NewGuid().ToString("D");
                tempVariable = tempVariable.Replace("-", "");
            }
            else
            {
                uint unmaskId = IdGenerator.Unmask(nodeId);
                tempVariable = Configurations.VariablePrefix + unmaskId;

                uint index = 1;
                List<string> definedVariables = this.variableNodesMap.Keys.ToList<string>();
                while (definedVariables.Contains(tempVariable))
                {
                    if (tempVariable.IndexOf("_") == -1)
                        tempVariable += "_" + index++;
                    else
                    {
                        tempVariable = tempVariable.Substring(0, tempVariable.Length - 1);
                        tempVariable += index++;
                    }
                }
            }
            return tempVariable;
        }

        internal void AddVariablesDefinedInNode(VisualNode node, bool isPriority)
        {
            List<string> variables = node.GetDefinedVariables(false);
            if (variables == null || variables.Count <= 0)
                return;

            foreach (string variable in variables)
                this.AddDefinition(variable, node.NodeId, isPriority);
        }

        internal void RemoveVariablesDefinedInNode(VisualNode node)
        {
            List<string> variables = node.GetDefinedVariables(false);
            if (variables == null || variables.Count <= 0)
                return;

            foreach (string variable in variables)
                this.RemoveDefinition(variable, node.NodeId);
        }

        internal void UpdateVariablesDefinedInNode(VisualNode node, bool isPriority)
        {
            List<string> mappingToRemove = new List<string>();
            List<string> variables = node.GetDefinedVariables(false);

            if (null != variables)
            {
                foreach (KeyValuePair<string, List<uint>> kvp in this.variableNodesMap)
                {
                    if (!variables.Contains(kvp.Key) && kvp.Value.Contains(node.NodeId))
                        mappingToRemove.Add(kvp.Key);
                }
            }

            foreach (string variable in mappingToRemove)
                this.RemoveDefinition(variable, node.NodeId);

            if (null != variables)
            {
                foreach (string variable in variables)
                    this.AddDefinition(variable, node.NodeId, isPriority);
            }

            if (null != undefinedVarsTracker && (mappingToRemove.Count > 0))
            {
                if (!undefinedVarsTracker.ContainsKey(node.NodeId))
                    undefinedVarsTracker.Add(node.NodeId, new List<string>());

                foreach (string variable in mappingToRemove)
                    undefinedVarsTracker[node.NodeId].Add(variable);
            }
        }

        internal void TransferVariableDefinitionMapping(List<string> variables, uint oldNodeId, uint newNodeId)
        {
            if (variables == null || variables.Count <= 0)
                return;

            foreach (string variable in variables)
            {
                if (this.variableNodesMap.Keys.Contains(variable))
                {
                    List<uint> value = new List<uint>();
                    this.variableNodesMap.TryGetValue(variable, out value);
                    int index = value.IndexOf(oldNodeId);
                    value[index] = newNodeId;
                }
            }
        }

        internal bool HasVariableDefinitionNotYetDefined(string variable)
        {
            if (String.IsNullOrEmpty(variable))
                throw new ArgumentException("'variable' is null or empty");

            if (!this.variableNodesMap.ContainsKey(variable))
                return true;
            return false;
        }

        internal bool HasVariableDefinitionNotYetDefined(List<string> variables, out string error)
        {
            error = string.Empty;
            bool hasNotYetDefined = false;

            if (null == variables || (variables.Count <= 0))
                return hasNotYetDefined;

            foreach (string variable in variables)
            {
                if (!this.variableNodesMap.ContainsKey(variable))
                {
                    error += "'" + variable + "' is not yet defined.\n";
                    hasNotYetDefined = true;
                }
            }

            return hasNotYetDefined;
        }

        internal bool HasDuplicateVariableDefinition(List<string> variables, uint nodeId, out string error)
        {
            error = string.Empty;
            bool hasDuplicate = false;

            if (null == variables || (variables.Count <= 0))
                return hasDuplicate;

            foreach (string variable in variables)
            {
                if (!this.variableNodesMap.ContainsKey(variable))
                {
                    string message = String.Format("'{0}' not in 'variableNodesMap'", variable);
                    throw new InvalidOperationException(message);
                }

                List<uint> nodesId = null;
                this.variableNodesMap.TryGetValue(variable, out nodesId);

                if (null != nodesId && (nodesId.Count > 0) && (nodesId[0] != nodeId))
                {
                    error += string.Format(UiStrings.RedefinitionFmt, variable) + "\n";
                    hasDuplicate = true;
                }
            }

            return hasDuplicate;
        }

        internal uint GetDefiningNode(string referencedVariable)
        {
            if (variableNodesMap.ContainsKey(referencedVariable))
                return (variableNodesMap[referencedVariable])[0];
            return uint.MaxValue;
        }

        #endregion

        #region Private Class Methods

        private void AddDefinition(string variable, uint nodeId, bool isPriority)
        {
            if (String.IsNullOrEmpty(variable))
                throw new ArgumentException("'variable' is null or empty'");

            List<uint> nodeIds = new List<uint>();
            if (!this.variableNodesMap.ContainsKey(variable))
            {
                nodeIds.Add(nodeId);
                this.variableNodesMap.Add(variable, nodeIds);
            }
            else
            {
                this.variableNodesMap.TryGetValue(variable, out nodeIds);
                if (!nodeIds.Contains(nodeId))
                {
                    if (isPriority)
                        nodeIds.Insert(0, nodeId);
                    else
                        nodeIds.Add(nodeId);
                }
                else if (isPriority)
                {
                    nodeIds.Remove(nodeId);
                    nodeIds.Insert(0, nodeId);
                }
            }
        }

        private void RemoveDefinition(string variable, uint nodeId)
        {
            if (String.IsNullOrEmpty(variable))
                throw new ArgumentException("'variable' is null or empty'");

            if (!this.variableNodesMap.ContainsKey(variable))
                throw new InvalidOperationException("'variable' not in 'variableNodesMap'");

            List<uint> nodesId = new List<uint>();
            this.variableNodesMap.TryGetValue(variable, out nodesId);

            if (!nodesId.Contains(nodeId))
                return;

            if (nodesId.Count == 1) //No duplicate
                this.variableNodesMap.Remove(variable);
            else //Exist duplicates
                nodesId.Remove(nodeId);
        }

        #endregion
    }
}
