using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using ProtoCore;
using System.Windows;
using DesignScript.Editor.Core;

namespace DesignScript.Editor
{
    public class InspectionDataCollection
    {
        bool inspectionToolTip = false;
        private ObservableCollection<InspectionData> dataCollection = null;

        #region Public Class Properties

        public ICollection<InspectionData> Data
        {
            get { return dataCollection; }
        }

        #endregion

        #region Public Operational Class Methods

        public InspectionDataCollection()
        {
            // The last item should always be empty (for entry).
            dataCollection = new ObservableCollection<InspectionData>();
            dataCollection.Add(new InspectionData(string.Empty));
        }

        /// <summary>
        /// This method is hit on one condition: the variable CAN be expanded by pressing the [+] 
        /// symbol in the watch window. This means some sort of expansion should replace the silly old
        /// 'This is dummy' and populate it. This method goes about populating the selectedData.Derivations
        /// for arrays and classes (which are the only expandable variables in a watch window). 
        /// For arrays, this method also tests if the array is larger than a certain value, indicating to
        /// the user that it may take some time to expand.
        /// </summary>
        /// <param name="selectedData"> InspectionData object of the variable that is to be expanded</param>
        public void ExpandInspection(InspectionData selectedData)
        {
            selectedData.IsExpanded = true;
            string qualifiedName = selectedData.GetQualifiedName();
            ProtoCore.Lang.Obj stackValue = GetStackValue(qualifiedName);
            CreateDataRecursive(selectedData, stackValue);
#if false
            ProtoCore.Lang.Obj dataObj = null;

            // Use Inspection Parser to parse the string to decipher what kind of expansion required
            InspectionParser parser = new InspectionParser(selectedData.Name);
            bool limit = false;
            int limitValue = 0;
            List<string> parsedCommands = null;

            // An inspection variable needs parsing when it contains:
            // [] (array) and/or 
            // '.' (class properties) and/or 
            // ',' (limit)
            if (parser.NeedsParsing())
            {
                parsedCommands = parser.GetParsedCommands();
                if (parser.IsValid == true)
                {
                    // Use GetParsedObject method to decipher Obj from parsed string
                    dataObj = GetParsedObject(parsedCommands, out limit);

                    // A limit is indicated by a comma (,)
                    if (limit)
                    {
                        // The limit should ALWAYS be the last item in parsed commands
                        string val = parsedCommands[parsedCommands.Count - 1].Replace(",", string.Empty);
                        limitValue = Convert.ToInt32(val);
                    }
                }
                else
                    dataObj = null;
            }
            else
            {
                dataObj = GetInnerInspectionObject(selectedData);
            }

            // If, from any level above the dataObj is null, throw an exception
            if (dataObj == null)
            {
                //Throw exceptions
                string n = selectedData.Name;
                string v = GetStackValuePayload(n);
                string t = GetStackValueType(n);

                // Replace the current data with the exceptions relevant for it
                RemoveInspectionData(selectedData);
                dataCollection.Insert(dataCollection.Count - 1, new InspectionData(n, v, t));
            }
            // Otherwise expand it by one more level yo!
            else
            {
                if (selectedData.Type == "array")
                    ExpandArrayInspectionData(selectedData, limit);
                else
                    ExpandClassInspectionData(selectedData);
            }

#endif

        }

        /// <summary>
        /// Returns maximum width of the inspection data 'Name' string
        /// </summary>
        /// <param name="dataSet">
        /// if equal to null, it means topmost level parent (dataCollection)
        /// Otherwise send in the InspectedData.Derivations of the child
        /// </param>
        /// <param name="level"> Level of the variable being checked. 0 is parent level </param>
        /// <returns>
        /// Maximum width of the variables in the watch window. This is useful for Inspection ToolTip
        /// </returns>
        public double MaxNameWidth(ObservableCollection<InspectionData> dataSet = null, int level = 0)
        {
            if (dataSet == null)
                dataSet = dataCollection;

            // Note - The 'Name' column also has the [+] sign, we have to add some space for this as well. 
            // This is why (level * 19.0) is included, because the LevelToIndentConverter uses that much
            // of space depending on the level of the variable.

            double max = 0;
            double temp = 0;
            double constant = 0;
            foreach (InspectionData data in dataSet)
            {
                if (!data.HasItems && level == 0)
                    constant = 35;

                if (((data.Expression.Length * 12) + (level * 39.0) + constant) > max)
                {
                    int value = data.Expression.Length * 12 > 42 ? data.Expression.Length * 12 : 42;
                    max = value + (level * 42.0) + constant;
                }

                if (data.HasItems)
                {
                    temp = MaxNameWidth(data.Derivations, level + 1);
                    if (temp > max)
                        max = temp;
                }
            }

            return (Utility.ValidateAgainstScreenWidth(max, false, true));
        }

        /// <summary>
        /// Returns maximum width of the inspection data 'Value' string
        /// </summary>
        /// <param name="dataSet">
        /// if equal to null, it means topmost level parent (dataCollection)
        /// Otherwise send in the InspectedData.Derivations of the child
        /// </param>
        /// <param name="level"> Level of the variable being checked. 0 is parent level </param>
        /// <returns>
        /// Maximum width of the variables in the watch window. This is useful for Inspection ToolTip
        /// </returns>
        public double MaxValueWidth(ObservableCollection<InspectionData> dataSet = null)
        {
            if (dataSet == null)
                dataSet = dataCollection;

            double max = 0;
            double temp = 0;
            foreach (InspectionData data in dataSet)
            {
                max = (data.Value.Length * 10) + 20;
                if (data.HasItems)
                {
                    temp = MaxValueWidth(data.Derivations);
                    if (temp > max)
                        max = temp;
                }
            }

            return (Utility.ValidateAgainstScreenWidth(max, false, true));
        }

        /// <summary>
        /// Method to add new inspection data to the Watch Window/ToolTip
        /// This method uses the CreateInspectionData method to populate the information
        /// </summary>
        /// <param name="variableName"> Name of the variable to be added </param>
        /// <returns>
        /// True - if successfully added
        /// False - if exception was thrown
        /// </returns>
        public bool AddInspectionData(string variableName, bool isToolTip)
        {
            inspectionToolTip = isToolTip;
            if (false != inspectionToolTip)
                dataCollection.Clear();

            if (string.IsNullOrWhiteSpace(variableName))
                return false;

            // Always leave last item as  blank data
            InspectionData data = new InspectionData(variableName);
            if (dataCollection.Count == 0)
                dataCollection.Add(data);
            else
                dataCollection.Insert(dataCollection.Count - 1, data);

            // Inspection tool tip requires immediate validation.
            if (false != inspectionToolTip)
                CreateDataRecursive(data, GetStackValue(variableName));

            return true;
#if false

            if (isToolTip)
            {
                toolTips = true;
                dataCollection.Clear();
            }

            if (string.IsNullOrWhiteSpace(variableName))
                return false;

            ProtoCore.Lang.Obj stackValue = null;
            InspectionParser parser = new InspectionParser(variableName);
            variableName = variableName.Trim();
            bool limit = false;
            int limitValue = 0;
            if (parser.NeedsParsing())
            {
                List<string> parsedCommands = parser.GetParsedCommands();
                if (parser.IsValid == true)
                {
                    stackValue = GetParsedObject(parsedCommands, out limit);
                    if (limit)
                    {
                        string val = parsedCommands[parsedCommands.Count - 1].Replace(",", string.Empty);
                        limitValue = Convert.ToInt32(val);
                    }
                }
                else
                    stackValue = null;
            }
            else
            {
                stackValue = GetStackValue(variableName);
            }

            if (stackValue == null)
            {
                //Throw exceptions
                string n = variableName;
                string v = GetStackValuePayload(n);
                string t = GetStackValueType(n);

                // Always leave last item as  blank data
                if (dataCollection.Count != 0)
                    dataCollection.Insert(dataCollection.Count - 1, new InspectionData(n, v, t));
                else
                    dataCollection.Add(new InspectionData(n, v, t));

                return false;
            }
            else
            {
                //Get actual data
                InspectionData inspectedData;
                inspectedData = CreateInspectionData(stackValue, variableName);

                if (dataCollection.Count != 0)
                    dataCollection.Insert(dataCollection.Count - 1, inspectedData);
                else
                    dataCollection.Add(inspectedData);

                return true;
            }
#endif
        }

        /// <summary>
        /// Method to remove an inspection item from the list
        /// </summary>
        /// <param name="item">
        /// InspectionData item to be removed
        /// </param>
        /// <returns>
        /// True - if Item is removed or cannot/should not be removed (like last row of blanks)
        /// False - if Item requested for removal is not found
        /// </returns>
        public bool RemoveInspectionData(object item)
        {
            InspectionData itemToRemove = (InspectionData)item;

            int index = 0;
            foreach (InspectionData data in dataCollection)
            {
                if (itemToRemove.IsEmptyData)
                    return true;
                else if (itemToRemove.Expression == data.Expression &&
                     itemToRemove.Value == data.Value &&
                     itemToRemove.Type == data.Type)
                {
                    dataCollection.RemoveAt(index);
                    if ((0 == dataCollection.Count) && !inspectionToolTip)
                        dataCollection.Add(new InspectionData(string.Empty));

                    return true;
                }

                index = index + 1;
            }

            return false;
        }

        /// <summary>
        /// Quick method to clear all InspectionData and leave only the blank field as when constructed
        /// </summary>
        public void RemoveAllInspectionData()
        {
            if (null != dataCollection)
            {
                dataCollection.Clear();
                dataCollection.Add(new InspectionData(string.Empty));
            }
        }

        /// <summary>
        /// This method acts as a 'refresh' button for the watch window. It causes the variables
        /// to be refreshed and updated with the latest values. After the refresh, it checks whatever
        /// variables have already been Expanded and carries out ExpansionIndividually
        /// </summary>
        public void RefreshInspectionView()
        {
            foreach (InspectionData data in dataCollection)
            {
                ProtoCore.Lang.Obj stackValue = GetStackValue(data.Expression);
                CreateDataRecursive(data, stackValue);
            }

            // foreach (InspectionData data in dataCollection)
            // {
            //     ProtoCore.Lang.Obj stackValue = GetStackValue(data.FullExpression);
            //     if (stackValue == null)
            //     {
            //         string n = data.FullExpression;
            //         string v = GetStackValuePayload(n);
            //         string t = GetStackValueType(n);
            // 
            //         data.Type = t;
            //         data.Value = v;
            //         data.ClearDerivations();
            //     }
            //     else
            //     {
            //         InspectionData updatedValue;
            //         string type = GetStackValueType(stackValue);
            //         if (type == "array" || type == "class")
            //             updatedValue = MakeInspectionData(stackValue, data.FullExpression);
            //         else
            //             updatedValue = CreateInspectionData(stackValue, data.FullExpression);
            // 
            //         data.Type = updatedValue.Type;
            //         data.Value = updatedValue.Value;
            //         if (updatedValue.Derivations != null)
            //         {
            //             if (data.Derivations != null)
            //             {
            //                 if (data.Derivations.Count < updatedValue.Derivations.Count)
            //                     updatedValue = CreateInspectionData(stackValue, data.FullExpression);
            //                 CheckExpansions(data.Derivations, updatedValue.Derivations);
            //             }
            //         }
            // 
            //         data.Derivations = updatedValue.Derivations;
            //     }
            // }
        }

        private void ShrinkDerivations(InspectionData inspectionData, int elementCount)
        {
            for (int i = inspectionData.Derivations.Count - 1; i >= elementCount; i--)
                inspectionData.Derivations.RemoveAt(i);
        }

        private void CreateDataRecursive(InspectionData inspectionData, ProtoCore.Lang.Obj stackValue)
        {
            if (null == stackValue || null == stackValue.Payload)
            {
                inspectionData.Type = string.Empty;
                inspectionData.Value = string.Empty;
                if (!string.IsNullOrEmpty(inspectionData.DisplayText))
                    inspectionData.Value = "null";

                inspectionData.ClearDerivations();
                return;
            }

            inspectionData.OpType = stackValue.DsasmValue.optype;

            switch (stackValue.DsasmValue.optype)
            {
                case ProtoCore.DSASM.AddressType.ArrayPointer: // Array type
                    {
                        inspectionData.Type = "array";
                        inspectionData.Value = "array";

                        if (false == inspectionData.IsExpanded)
                        {
                            // If we are looking at an array, then make sure it 
                            // at least have an empty child data (so that HasItems
                            // property returns "true"). This child data can later
                            // be replaced by the actual value upon expansion.
                            if (false == inspectionData.HasItems)
                            {
                                string parentExpression = inspectionData.Expression;
                                inspectionData.AddDerivation(parentExpression, 0);
                            }
                        }
                        else
                        {
                            List<ProtoCore.Lang.Obj> arrayElements = GetArrayStackValues(stackValue);
                            int elementCount = ((null == arrayElements) ? 0 : arrayElements.Count);
                            inspectionData.Type = string.Format("array[{0}]", elementCount);
                            inspectionData.Value = inspectionData.Type;

                            if (inspectionData.Derivations != null)
                                ShrinkDerivations(inspectionData, elementCount);

                            int elementsToDisplay = elementCount;
                            if (inspectionData.ExpansionLimit > 0)
                            {
                                elementsToDisplay = inspectionData.ExpansionLimit;
                                if (elementsToDisplay > elementCount)
                                    elementsToDisplay = elementCount;
                            }

                            string parentExpression = inspectionData.Expression;
                            for (int index = 0; index < elementsToDisplay; ++index)
                            {
                                InspectionData childData = inspectionData.GetDrivationAtIndex(index);
                                if (null == childData)
                                    childData = inspectionData.AddDerivation(parentExpression, index);

                                CreateDataRecursive(childData, arrayElements[index]);
                            }
                        }

                        break;
                    }

                case ProtoCore.DSASM.AddressType.Pointer: // Class type
                    {
                        if (inspectionData.Expression.Equals("this") && (Int64)stackValue.Payload == -1) 
                        {
                            inspectionData.Type = "Invalid";
                            inspectionData.Value = "null";
                            break;
                        }

                        inspectionData.Type = GetStackValueType(stackValue);
                        inspectionData.Value = GetStackValueData(stackValue);

                        if (false == inspectionData.IsExpanded)
                        {
                            // If we are looking at a class, then make sure it 
                            // at least have an empty child data (so that HasItems
                            // property returns "true"). This child data can later
                            // be replaced by the actual value upon expansion.
                            if (false == inspectionData.HasItems)
                                inspectionData.AddDerivation(string.Empty, -1);
                        }
                        else
                        {
                            // Expansion for a class and class properties
                            Dictionary<string, ProtoCore.Lang.Obj> properties = GetClassProperties(stackValue);
                            if (null == properties || (properties.Count <= 0))
                                return;

                            if (inspectionData.Derivations != null)
                                ShrinkDerivations(inspectionData, properties.Count);

                            int index = 0;
                            foreach (KeyValuePair<string, ProtoCore.Lang.Obj> property in properties)
                            {
                                InspectionData childData = inspectionData.GetDrivationAtIndex(index);
                                if (null == childData)
                                    childData = inspectionData.AddDerivation(property.Key, -1);

                                // The first child data that was created before this class gets 
                                // expanded may not have the appropriate expression to begin with, 
                                // so set it here...
                                childData.Expression = property.Key;

                                if (null != property.Value)
                                    CreateDataRecursive(childData, property.Value);
                                else
                                {
                                    // This may be an external property, try evaluating it for value.
                                    string qualifiedName = childData.GetQualifiedName();
                                    ProtoCore.Lang.Obj childStackValue = GetStackValue(qualifiedName);
                                    CreateDataRecursive(childData, childStackValue);
                                }

                                index = index + 1;
                            }
                        }

                        break;
                    }
                case ProtoCore.DSASM.AddressType.String:
                case ProtoCore.DSASM.AddressType.Char:
                    {
                        inspectionData.Type = GetStackValueType(stackValue);
                        inspectionData.Value = GetStackValueData(stackValue);
                        inspectionData.ClearDerivations();
                        break;
                    }
                default: // This applies to all other primitive types.
                    {
                        inspectionData.Type = GetStackValueType(stackValue);
                        inspectionData.Value = GetStackValuePayload(stackValue);
                        inspectionData.ClearDerivations();
                        break;
                    }
            }
        }

        /// <summary>
        /// When a variable is edited in the Watch Window using the VariableEditor 
        /// textbox, ther name of the variable has to be updated before updateInspection 
        /// is called so that the new data can be populated. This method updates 
        /// the new Variable name.
        /// </summary>
        /// <param name="newVariableName">New value to which the variable is to be renamed</param>
        /// <param name="index">Index of the variable to rename</param>
        public void UpdateVariableName(string newVariableName, int index)
        {
            if (string.IsNullOrWhiteSpace(newVariableName))
                return;

            if (index == dataCollection.Count - 1)
            {
                AddInspectionData(newVariableName, false);
                return;
            }

            dataCollection[index].Expression = newVariableName;
            dataCollection[index].ClearDerivations();
        }

        /// <summary>
        /// This method is used to retrieve the index of an item that is selected. This method
        /// will only return the Parent Index of the selected so child items will return -1
        /// </summary>
        /// <param name="item"> InspectionData item selected </param>
        /// <returns></returns>
        public int GetParentIndex(InspectionData itemSelected)
        {
            int index = 0;
            foreach (InspectionData data in dataCollection)
            {
                if (itemSelected.Expression == data.Expression &&
                    itemSelected.Value == data.Value &&
                    itemSelected.Type == data.Type)
                {
                    break;
                }

                index++;
            }

            // The item selected is either not a parent item or doesn't exist
            if (index >= dataCollection.Count)
                return -1;

            return index;
        }

        #endregion

        #region Private Class Helper Methods

#if false

        private void ExpandArrayInspectionData(InspectionData selectedData, bool limitExpansion)
        {
            ProtoCore.Lang.Obj stackValue = GetStackValue(selectedData.Name);
            List<ProtoCore.Lang.Obj> arrayStackValues = GetArrayStackValues(stackValue);

            ObservableCollection<InspectionData> derivations = new ObservableCollection<InspectionData>();
            int arrayIndex = 0;

            // Give user a notification if a large number of array elements are expanded
            // The limit set is located at Configurations.InspectionArrayLimit
            if ((arrayStackValues.Count > Configurations.InspectionArrayLimit) && !limitExpansion)
            {
                string message = "This array has a large number of items. Displaying may take several seconds. " +
                    "Show all elements anyway? Alternatively you can choose 'No' and limit your array expansion " +
                    "by adding a comma (,) after your variable followed by the number of elements to be shown.";

                MessageBoxResult result = MessageBox.Show(message, "Inspection", MessageBoxButton.YesNo);

                // If user chooses 'Yes' expand all items
                if (result == MessageBoxResult.Yes)
                {
                    foreach (ProtoCore.Lang.Obj element in arrayStackValues)
                    {
                        // Array name should be represented like a[3] for each element
                        string arrayName = (limit ? parsedCommands[0] : selectedData.Name) + "[" + arrayIndex + "]";
                        derivations.Add(CreateInspectionData(element, arrayName));
                        arrayIndex++;

                        if ((limit) && (derivations.Count > limitValue))
                        {
                            break;
                        }
                    }
                    // Value for an array is represented like array[10]
                    string v = selectedData.Type + "[" + arrayStackValues.Count + "]";
                    for (int i = 0; i < selectedData.Derivations.Count; i++)
                    {
                        if (selectedData.Derivations[i].IsExpanded == true)
                        {
                            if (i < derivations.Count)
                                derivations[i].IsExpanded = true;
                        }
                    }

                    selectedData.Derivations = derivations;
                }
            }
            // If there was no limit reached, proceed as normal
            else
            {
                foreach (ProtoCore.Lang.Obj element in arrayStackValues)
                {
                    string arrayName = (limit ? parsedCommands[0] : selectedData.Name) + "[" + arrayIndex + "]";
                    derivations.Add(CreateInspectionData(element, arrayName));
                    arrayIndex++;

                    if ((limit) && (derivations.Count > limitValue))
                    {
                        break;
                    }
                }

                string v = selectedData.Type + "[" + arrayStackValues.Count + "]";

                for (int i = 0; i < selectedData.Derivations.Count; i++)
                {
                    if (selectedData.Derivations[i].IsExpanded == true)
                    {
                        if (i < derivations.Count)
                            derivations[i].IsExpanded = true;
                    }
                }

                selectedData.Derivations = derivations;
            }
        }

        private void ExpandClassInspectionData(InspectionData selectedData)
        {
            // Expansion for a class and class properties
            ProtoCore.Lang.Obj stackValue = GetStackValue(selectedData.Name);
            Dictionary<string, ProtoCore.Lang.Obj> classProperties = GetClassProperties(stackValue);

            if (null != classProperties)
            {
                ObservableCollection<InspectionData> oldDerivations = selectedData.Derivations;
                selectedData.ClearDerivations();

                int index = 0;
                foreach (KeyValuePair<string, ProtoCore.Lang.Obj> property in classProperties)
                {
                    InspectionData inspectionData = CreateInspectionData(property.Value, property.Key);
                    if (null != inspectionData)
                    {
                        selectedData.AddDerivation(inspectionData);
                        if (null != oldDerivations && (index < oldDerivations.Count))
                            inspectionData.IsExpanded = oldDerivations[index].IsExpanded;
                    }

                    index = index + 1;
                }
            }
        }

        /// <summary>
        /// Method to create an inspection data out of a sent variable by talking
        /// to the Virtual machine. The watch window adds the variable with a dummy layer under the expansion. 
        /// The dummy level prevents the Virtual Machine being called too many times, 
        /// and should never be shown to the user.
        /// Eg: Variable name: 'a'
        /// In watch window:
        ///             Name    |Value|Type
        ///             'a'
        ///             - 'This' 'is'  'dummy'
        /// </summary>
        /// <param name="inspectionObj"> Obj type retrieved from Virtual Machine for given variable </param>
        /// <param name="variableName"> Name of the variable to be made into InspectionData </param>
        /// <returns> InspectionData object of the Variable </returns>
        private InspectionData CreateInspectionData(ProtoCore.Lang.Obj inspectionObj, string variableName)
        {
            string stackValueType = GetStackValueType(inspectionObj);
            InspectionData inspectedScope = null;

            if (stackValueType == "int" || stackValueType == "double" || stackValueType == "null" || stackValueType == "bool")
            {
                string n = variableName;
                string v = (inspectionObj.Payload == null) ? ("null") : (inspectionObj.Payload.ToString());
                string t = stackValueType;
                inspectedScope = new InspectionData(n, v, t);
            }
            else if (stackValueType == "array")
            {
                string n = variableName;
                string t = stackValueType;

                List<ProtoCore.Lang.Obj> arrayElements = GetArrayStackValues(inspectionObj);
                int arrayIndex = 0;

                foreach (ProtoCore.Lang.Obj element in arrayElements)
                {
                    string arrayName = variableName + "[" + arrayIndex + "]";
                    arrayIndex++;
                }

                string v = stackValueType + "[" + arrayElements.Count + "]";
                inspectedScope = new InspectionData(n, v, t);

                if (arrayElements.Count > 0)
                {
                    // By adding "this is dummy" into the watch window we aim to prevent overworking the
                    // Virtual Machine and expand only if necessary. Obviously, the user should NEVER see this
                    // so that will be a defect, if ever. This also notifies the XAML binding to show the [+]
                    // next to the watch variable.
                    inspectedScope.AddDerivation(new InspectionData(n, v, t));
                }
            }
            else
            {
                Dictionary<string, ProtoCore.Lang.Obj> classProperties = GetClassProperties(inspectionObj);
                if (null != classProperties)
                {
                    foreach (KeyValuePair<string, ProtoCore.Lang.Obj> property in classProperties)
                    {
                        string propName = property.Key;
                    }

                    string n = variableName;
                    string v = stackValueType;
                    string t = stackValueType;

                    inspectedScope = new InspectionData(n, v, t);
                    if (classProperties.Count > 0)
                        inspectedScope.AddDerivation(new InspectionData(n, v, t));
                }
            }

            return inspectedScope;
        }

        //Recursive system to run through all branches of variable inspected
        public InspectionData MakeInspectionData(ProtoCore.Lang.Obj inspectionObj, string variableName, int limit = 100)
        {
            string stackValueType = GetStackValueType(inspectionObj);
            InspectionData inspectedScope = null;

            if (stackValueType == "int" || stackValueType == "double" || stackValueType == "null" || stackValueType == "bool")
            {
                string n = variableName;
                string v = (inspectionObj.Payload == null) ? ("null") : (inspectionObj.Payload.ToString());
                string t = stackValueType;
                inspectedScope = new InspectionData(n, v, t);
            }
            else if (stackValueType == "array")
            {
                string n = variableName;
                string t = stackValueType;

                List<ProtoCore.Lang.Obj> arrayElements = GetArrayStackValues(inspectionObj);
                int arrayIndex = 0;

                string v = stackValueType + "[" + arrayElements.Count + "]";
                inspectedScope = new InspectionData(n, v, t);

                foreach (ProtoCore.Lang.Obj element in arrayElements)
                {
                    string arrayName = variableName + "[" + arrayIndex + "]";
                    inspectedScope.AddDerivation(MakeInspectionData(element, arrayName));
                    arrayIndex++;
                }
            }
            else
            {
                Dictionary<string, ProtoCore.Lang.Obj> classProperties = GetClassProperties(inspectionObj);

                if (null != classProperties)
                {
                    string n = variableName;
                    string v = stackValueType;
                    string t = stackValueType;

                    inspectedScope = new InspectionData(n, v, t);
                    foreach (KeyValuePair<string, ProtoCore.Lang.Obj> property in classProperties)
                    {
                        string propName = property.Key;
                        inspectedScope.AddDerivation(MakeInspectionData(property.Value, propName));
                    }
                }
            }

            return inspectedScope;
        }

        /// <summary>
        /// This method retrieves the InspectionObject for a variable at any level of the watch window. 
        /// Eg:- a
        ///      -  b
        ///      -  c
        ///         -  d
        ///  The object for 'd' would be retrieved in a two step process
        ///  1. The top most parent is retrieved using the GetTopMostParent method - in this case 'a'
        ///  2. The GetTopMostParent method also tells you if the variable searched for is in the top level or inside
        ///  a child. In this case, bool IsInChild is set to true
        ///  3. Method GetChildObject is used to retrieve the child recursively
        ///  
        /// This is done because the ChildObject cannot be located without the topmost parent Obj
        /// </summary>
        /// <param name="data"> InspectionData object of the variable being searched </param>
        /// <returns> ProtoCore.Lang.Obj object returned for found variable </returns>
        private ProtoCore.Lang.Obj GetInnerInspectionObject(InspectionData data)
        {
            bool IsInChild = false;
            string parent = GetTopMostParent(data, out IsInChild);
            ProtoCore.Lang.Obj parentObj = GetStackValue(parent);

            if (!IsInChild)
                return parentObj;
            else
                return GetChildObject(parentObj, data);
        }

        /// <summary>
        /// Recursively go down the Variable tree child by child till the desired
        /// searched data is met.
        /// </summary>
        /// <param name="parentObj"> Obj object of the parent of the child being searched </param>
        /// <param name="searchedData"> Variable being searched to match child object </param>
        /// <param name="dataSet"> 
        /// if equal to null, it means topmost level parent (dataCollection)
        /// Otherwise send in the InspectedData.Derivations of the child
        /// </param>
        /// <returns></returns>
        private ProtoCore.Lang.Obj GetChildObject(ProtoCore.Lang.Obj parentObj, InspectionData searchedData, ObservableCollection<InspectionData> dataSet = null)
        {
            if (dataSet == null)
                dataSet = dataCollection;

            foreach (InspectionData item in dataSet)
            {
                if ((item.Expression == searchedData.Expression) && (item.Value == searchedData.Value) && (item.Type == searchedData.Type))
                {
                    return parentObj;
                }
                else if (item.HasItems)
                {
                    if (item.Type == "array")
                    {
                        List<ProtoCore.Lang.Obj> arrayElements = GetArrayStackValues(parentObj);
                        foreach (ProtoCore.Lang.Obj element in arrayElements)
                        {
                            ProtoCore.Lang.Obj child = GetChildObject(element, searchedData, item.Derivations);

                            if (null != child)
                                return child;
                        }
                    }
                    else
                    {
                        Dictionary<string, ProtoCore.Lang.Obj> classProperties = GetClassProperties(parentObj);
                        if (null != classProperties)
                        {
                            foreach (KeyValuePair<string, ProtoCore.Lang.Obj> property in classProperties)
                            {
                                ProtoCore.Lang.Obj child = GetChildObject(property.Value, searchedData, item.Derivations);

                                if (null != child)
                                    return child;
                            }
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Retrieves the top-most parent for any InspectionData variable
        /// </summary>
        /// <param name="dataSearched"> InspectionData variable being searched </param>
        /// <param name="IsInChild"> out parameter that toggles to false if  top most parent is dataSearched, else false</param>
        /// <returns> Name of topmost parent of variable </returns>
        private string GetTopMostParent(InspectionData dataSearched, out bool IsInChild)
        {
            foreach (InspectionData item in dataCollection)
            {
                if (CheckChildren(item.Derivations, dataSearched) == true)
                {
                    IsInChild = true;
                    return item.Expression;
                }
                else if ((item.Expression == dataSearched.Expression) && (item.Value == dataSearched.Value) && (item.Type == dataSearched.Type))
                {
                    IsInChild = false;
                    return item.Expression;
                }
            }

            IsInChild = false;
            return null;
        }

        /// <summary>
        /// Recursively go down the Variable tree child by child to see if there is a valid child in the dataSet
        /// </summary>
        /// <param name="dataSet"> 
        /// if equal to null, it means there is no further dataSet to search so return false
        /// Else, send in InspectionData.Derivations
        /// </param>
        /// <param name="dataSearched"> InspectionData variable being searched </param>
        /// <returns> True id a child is found, false if no child</returns>
        private bool CheckChildren(ObservableCollection<InspectionData> dataSet, InspectionData dataSearched)
        {
            if (dataSet == null)
                return false;

            foreach (InspectionData item in dataSet)
            {
                if ((item.Expression == dataSearched.Expression) && (item.Value == dataSearched.Value) && (item.Type == dataSearched.Type))
                {
                    return true;
                }
                else if (item.HasItems)
                {
                    return CheckChildren(item.Derivations, dataSearched);
                }
            }

            return false;
        }

        /// <summary>
        /// If any variable has it's InspectionData.IsExpanded flag set, it will carry out an 
        /// 'ExpandInspection' to update the data values under the variable
        /// </summary>
        /// <param name="data"> ObservableCollection of data being checked for expansion</param>
        private void UpdateExpandedData(ObservableCollection<InspectionData> data)
        {
            for (int i = 0; i < data.Count; i++)
            {
                if (i < data.Count)
                {
                    if (data[i].IsExpanded)
                    {
                        ExpandInspection(data[i]);
                    }
                }

                if (data[i].Derivations != null)
                {
                    UpdateExpandedData(data[i].Derivations);
                }
            }
        }

        /// <summary>
        /// Compares two observable collections, orginalData and updatedData and sets the IsExpanded flag for
        /// updatedData as in originalData. This is required for the following scenario:
        /// When a variable is updated, everything gets refreshed. Therefore updated data has the latest information,
        /// but has no information about the Expansions. Therefore it is compared with the original data,
        /// updated with the IsExpanded info and then the expansion is carried out later.
        /// </summary>
        /// <param name="originalData"> Original un-updated data with the correct IsExpanded flags</param>
        /// <param name="updatedData"> Updated data that needs the new IsExpanded information </param>
        private void CheckExpansions(ObservableCollection<InspectionData> originalData, ObservableCollection<InspectionData> updatedData)
        {
            for (int i = 0; i < updatedData.Count; i++)
            {
                if (updatedData[i].Derivations != null && originalData[i].Derivations != null)
                {
                    CheckExpansions(originalData[i].Derivations, updatedData[i].Derivations);
                }
                if (i < originalData.Count)
                {
                    updatedData[i].IsExpanded = originalData[i].IsExpanded;

                }
            }
        }
#endif

        private ProtoCore.Lang.Obj GetStackValue(string variableName)
        {
            if (Solution.Current.ExecutionSession == null)
                return null;

            ProtoCore.Lang.Obj value = null;
            Solution.Current.ExecutionSession.GetStackValue(variableName, out value);
            return value;
        }

        private string GetStackValueType(ProtoCore.Lang.Obj stackValue)
        {
            if (Solution.Current.ExecutionSession == null)
                return "No running execution session!";

            string type = string.Empty;
            Solution.Current.ExecutionSession.GetStackValueType(stackValue, ref type);
            return type;
        }

        private string GetStackValueData(ProtoCore.Lang.Obj stackValue)
        {
            if (Solution.Current.ExecutionSession == null)
                return "No running execution session!";

            string data = string.Empty;
            Solution.Current.ExecutionSession.GetStackValueData(stackValue, ref data);
            return data;
        }

        private string GetStackValuePayload(ProtoCore.Lang.Obj stackValue)
        {
            if (null == stackValue || (null == stackValue.Payload))
                return "null";
            if (stackValue.DsasmValue.optype == ProtoCore.DSASM.AddressType.Boolean)
            {
                return (stackValue.DsasmValue.opdata == 1) ? "true" : "false";
            }
            return stackValue.Payload.ToString();
        }

        /// <summary>
        /// Returns all class properties in a dictionary pertaing to input inspectionObj
        /// </summary>
        /// <param name="inspectionObj"> Obj of variable to retrieve class properties </param>
        /// <returns> Null on exception. Dictionary of (property name, property Obj) if successful. </returns>
        private Dictionary<string, ProtoCore.Lang.Obj> GetClassProperties(ProtoCore.Lang.Obj inspectionObj)
        {
            if (Solution.Current.ExecutionSession == null)
                return null;

            Dictionary<string, ProtoCore.Lang.Obj> properties = null;
            Solution.Current.ExecutionSession.GetClassProperties(inspectionObj, out properties);
            return properties;
        }

        /// <summary>
        /// Returns all array elements pertaing to input inspectionObj
        /// </summary>
        /// <param name="inspectionObj"> Obj of variable to retrieve class properties </param>
        /// <returns> Null on exception. List of (arrayElement Obj) if successful. </returns>
        private List<ProtoCore.Lang.Obj> GetArrayStackValues(ProtoCore.Lang.Obj inspectionObj)
        {
            if (Solution.Current.ExecutionSession == null)
                return null;

            List<ProtoCore.Lang.Obj> elements = null;
            Solution.Current.ExecutionSession.GetArrayElements(inspectionObj, out elements);
            return elements;
        }

        /// <summary>
        /// Returns a single array element obj as pertaining to an inspectionObj
        /// </summary>
        /// <param name="inspectionObj"> Obj of variable to retrieve array element object </param>
        /// <param name="index"> Index of array element being retrieved </param>
        /// <returns> Null on exception. Array Element Obj if successful. </returns>
        private ProtoCore.Lang.Obj GetArrayElement(ProtoCore.Lang.Obj inspectionObj, int index)
        {
            List<ProtoCore.Lang.Obj> arrayElements = GetArrayStackValues(inspectionObj);
            if (arrayElements == null)
                return null;
            if (index > arrayElements.Count - 1)
                return null;
            else
                return arrayElements[index];
        }

        /// <summary>
        /// Returns a single class property obj as pertaining to an inspectionObj
        /// </summary>
        /// <param name="inspectionObj"> Obj of variable to retrieve class property object </param>
        /// <param name="property"> Name of property being searched </param>
        /// <returns> Null on exception. Class property as Obj if successful. </returns>
        private ProtoCore.Lang.Obj GetClassProperty(ProtoCore.Lang.Obj inspectionObj, string property)
        {
            Dictionary<string, ProtoCore.Lang.Obj> classProperties = GetClassProperties(inspectionObj);
            if ((null != classProperties) && classProperties.ContainsKey(property))
                return classProperties[property];

            return null;
        }

#if false
        /// <summary>
        /// This method retrieves results of the parsing by InspectionParser object
        /// The list of strings is created in the following format
        /// list[0] = name of main variable
        /// Any item with "[" means it is an array item, indext needs to be retrieved
        /// Any item with a "." as the first character is a class property
        /// Any item with a "," as the first character indicates a limit on the number of items to be shown
        /// </summary>
        /// <param name="parsedCommands"> List of parsed commands retrieved from result of InspectionParser </param>
        /// <param name="limit"> If user has set a limit by using comma (,) this boolean flips to true, else false </param>
        /// <returns> Obj of final parsed object </returns>
        private ProtoCore.Lang.Obj GetParsedObject(List<string> parsedCommands, out bool limit)
        {
            ProtoCore.Lang.Obj data = GetStackValue(parsedCommands[0]);
            limit = false;

            for (int i = 1; i < parsedCommands.Count; i++)
            {
                if (parsedCommands[i].Contains('['))
                {
                    string currCommand = parsedCommands[i];
                    string index = currCommand.Substring(currCommand.IndexOf('[') + 1, currCommand.IndexOf(']') - currCommand.IndexOf('[') - 1);

                    data = GetArrayElement(data, Convert.ToInt32(index));
                }
                else if (parsedCommands[i].Contains('.'))
                {
                    string property = parsedCommands[i].Replace(".", string.Empty);
                    data = GetClassProperty(data, property);
                }
                else if (parsedCommands[i].Contains(','))
                {
                    limit = true;
                }
            }
            return data;
        }
#endif

        /// <summary>
        /// Translates different exceptions into string so that they can be shown on the watch window
        /// </summary>
        /// <param name="message"> Exception.Message string to be translated </param>
        /// <returns> Translated version of exception as a string </returns>
        private string TranslateExceptions(string message)
        {
            switch (message)
            {
                case "Exception of type 'ProtoCore.DSASM.Mirror.NameNotFoundException' was thrown.":
                    return "Variable not found in the current scope";
                case "Exception of type 'ProtoCore.DSASM.Mirror.UninitializedVariableException' was thrown.":
                    return "Variable has not been initialised yet";
                default:
                    return "Exception not translated";
            }
        }

        #endregion
    }
}
