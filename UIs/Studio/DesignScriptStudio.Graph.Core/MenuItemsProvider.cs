using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using ProtoCore.DSASM;

namespace DesignScriptStudio.Graph.Core
{
    class MenuItemsProvider
    {
        GraphController graphController = null;
        NodePart nodePart = NodePart.None;
        uint nodeId = uint.MaxValue;
        Dictionary<int, LibraryItem> libraryItemCollection;
        Dictionary<int, LibraryItem> overloadedItemCollection;

        public MenuItemsProvider(GraphController controller, NodePart part, uint id)
        {
            this.graphController = controller;
            this.nodePart = part;
            this.nodeId = id;
            this.GenerateMenuItems();
        }

        #region Private Class Helper Methods

        private void GenerateMenuItems()
        {
            Dictionary<int, string> list = new Dictionary<int, string>();

            VisualNode node = graphController.GetVisualNode(nodeId);

            if (nodeId == uint.MaxValue)
                libraryItemCollection = new Dictionary<int, LibraryItem>();
            if (null == node)
                return;

            switch (node.VisualType)
            {
                case NodeType.Function:
                    GetFunctionNodeMenuItems();
                    break;
                case NodeType.Driver:
                    GetDriverNodeMenuItems(list);
                    break;
                case NodeType.CodeBlock:
                    GetCodeBlockNodeMenuItems(list);
                    break;
                case NodeType.Identifier:
                    GetIdentifierNodeMenuItems();
                    break;
                case NodeType.Property:
                    GetPropertyNodeMenuItems();
                    break;
                default:
                    return;
            }
            return;
        }

        private void GetIdentifierNodeMenuItems()
        {
            ObservableCollection<LibraryItem> libraryItems = new ObservableCollection<LibraryItem>();
            libraryItemCollection = new Dictionary<int, LibraryItem>();
            VisualNode node = graphController.GetVisualNode(this.nodeId);
            if (node.GetAssembly() == null || node.ReturnType == null)
                return;
            string[] assemblies = (node.GetAssembly()).Split(',');
            string[] returnTypes = (node.ReturnType.Split(','));

            switch (nodePart)
            {
                case NodePart.NorthEast:
                    this.GetMethodsAndProperties(assemblies, returnTypes, libraryItems);
                    break;
                default:
                    break;
            }
            this.PopulateItems(libraryItems);
            return;
        }

        private void GetCodeBlockNodeMenuItems(Dictionary<int, string> codeBlockNodeItems)
        {
            switch (nodePart)
            {
                case NodePart.NorthEast:
                    //codeBlockNodeItems.Add(1, "To Node");
                    //codeBlockNodeItems.Add(2, "To Code");
                    //codeBlockNodeItems.Add(3, "Explode!");
                    break;
                default:
                    break;
            }
        }

        private void GetDriverNodeMenuItems(Dictionary<int, string> driverNodeItems)
        {
            switch (nodePart)
            {
                case NodePart.NorthEast:
                    //driverNodeItems.Add(1, "Delete Node");
                    //driverNodeItems.Add(2, "Copy");
                    //driverNodeItems.Add(3, "Explode");
                    break;
                default:
                    break;
            }
        }

        private void GetFunctionNodeMenuItems()
        {

            ObservableCollection<LibraryItem> libraryItems = new ObservableCollection<LibraryItem>();
            libraryItemCollection = new Dictionary<int, LibraryItem>();
            VisualNode node = graphController.GetVisualNode(this.nodeId);
            FunctionNode fNode = (FunctionNode)node;
            Dictionary<int, string> functionNodeItems = new Dictionary<int, string>();
            if (node.GetAssembly() == null || node.ReturnType == null)
                return;
            string[] assemblies = (node.GetAssembly()).Split(',');
            string[] returnTypes = (node.ReturnType.Split(','));

            switch (nodePart)
            {
                case NodePart.NorthEast:
                //case NodePart.South:
                    this.GetMethodsAndProperties(assemblies, returnTypes, libraryItems);
                    break;
                case NodePart.North:
                    if (fNode.MemberType == LibraryItem.MemberType.Constructor)
                        libraryItems = CoreComponent.Instance.GetConstructors(fNode.Assembly, fNode.QualifiedName);
                    else if (fNode.MemberType == LibraryItem.MemberType.InstanceMethod)
                    {
                        string argumentTypes = fNode.ArgumentTypes;
                        string[] tempArray = argumentTypes.Split(',');
                        string parentReturnType = tempArray[0];
                        if (parentReturnType == "this")
                        {
                            uint slotId = node.GetInputSlot(0);
                            ISlot slot = graphController.GetSlot(slotId);
                            uint[] connectingSlots = slot.ConnectingSlots;
                            if (connectingSlots != null)
                            {
                                ISlot parentSlot = graphController.GetSlot(connectingSlots[0]);
                                VisualNode visualNode = graphController.GetVisualNode(parentSlot.Owners[0]);
                                parentReturnType = visualNode.ReturnType;
                                string[] tempParent = { parentReturnType };
                                this.GetMethodsAndProperties(assemblies, tempParent, libraryItems);
                                //libraryItems = CoreComponent.Instance.GetMethodsAndProperties(fNode.Assembly, parentReturnType);
                            }
                        }
                        else
                            libraryItems = CoreComponent.Instance.GetMethodsAndProperties(fNode.Assembly, parentReturnType);
                    }
                    break;
                case NodePart.NorthWest:
                    Dictionary<int, string> temp = new Dictionary<int, string>();
                    this.GetNodeOptions(this.nodeId, out temp);
                    functionNodeItems = temp;
                    break;
                default:
                    break;
            }

            this.PopulateItems(libraryItems);
            return;
        }

        private void GetPropertyNodeMenuItems()
        {

            ObservableCollection<LibraryItem> libraryItems = new ObservableCollection<LibraryItem>();
            libraryItemCollection = new Dictionary<int, LibraryItem>();
            VisualNode node = graphController.GetVisualNode(this.nodeId);
            PropertyNode pNode = (PropertyNode)node;
            List<string> parents = GraphToDSCompiler.GraphUtilities.GetParentClasses(pNode.ReturnType);
            if (node.GetAssembly() == null || node.ReturnType == null)
                return;
            string[] assemblies = (node.GetAssembly()).Split(',');
            string[] returnTypes = (node.ReturnType.Split(','));

            switch (nodePart)
            {
                case NodePart.NorthEast:
                    this.GetMethodsAndProperties(assemblies, returnTypes, libraryItems);
                    //ProtoCore.Mirror.MethodMirror mirror = new ProtoCore.Mirror.MethodMirror(GraphToDSCompiler.GraphUtilities.GetCore(), Name);
                    //mirror.Get
                    //ObservableCollection<LibraryItem> classLibraryItems = CoreComponent.Instance.GetMethodsAndProperties(pNode.Assembly, pNode.ReturnType);
                    //foreach (LibraryItem item in classLibraryItems)
                    //    libraryItems.Add(item);

                    //if (parents.Count != 0)
                    //{
                    //    foreach (string parentClass in parents)
                    //    {
                    //        ObservableCollection<LibraryItem> parentLibraryItems = CoreComponent.Instance.GetMethodsAndProperties(pNode.Assembly, parentClass);
                    //        foreach (LibraryItem item in parentLibraryItems)
                    //            libraryItems.Add(item);
                    //    }
                    //}
                    break;
                case NodePart.NorthWest:
                    //propertyNodeItems.Add(Configurations.AddReplicationGuides, "Add Replication Guides");
                    //propertyNodeItems.Add(Configurations.RemoveReplicationGuides, "Remove Replication Guides");
                    //propertyNodeItems.Add(Configurations.DeleteNode, "Delete Node");
                    //propertyNodeItems.Add(10, "Close");
                    break;
                default:
                    break;
            }

            this.PopulateItems(libraryItems);
        }

        private void GetSelectionBoxMenuItems(out Dictionary<int, string> nodeItems)
        {
            libraryItemCollection = new Dictionary<int, LibraryItem>();
            nodeItems = new Dictionary<int, string>();
            nodeItems.Add(Configurations.ConvertNodeToCode, "Convert Nodes to Code");
        }

        private void PopulateItems(ObservableCollection<LibraryItem> libraryItems)
        {
            int propertyBaseId = Configurations.PropertyBase;
            int methodBaseId = Configurations.MethodBase;
            int constructorBaseId = Configurations.ConstructorBase;
            int overloadBaseId = Configurations.OverloadBase;
            overloadedItemCollection = new Dictionary<int, LibraryItem>();
            if (libraryItems != null)
            {
                foreach (LibraryItem item in libraryItems)
                {
                    if (item.IsOverloaded == true)
                        overloadedItemCollection.Add(overloadBaseId++, item);
                    else
                    {
                        if (item.Type == LibraryItem.MemberType.InstanceProperty)
                            libraryItemCollection.Add(propertyBaseId++, item);
                        if (item.Type == LibraryItem.MemberType.InstanceMethod)
                            libraryItemCollection.Add(methodBaseId++, item);
                        if (item.Type == LibraryItem.MemberType.Constructor)
                            libraryItemCollection.Add(constructorBaseId++, item);
                    }
                }
                //sort in ascending order
                libraryItemCollection = libraryItemCollection.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            }
        }

        private void GetNodeOptions(uint nodeId, out Dictionary<int, string> nodeItems)
        {
            VisualNode node = this.graphController.GetVisualNode(nodeId);
            nodeItems = new Dictionary<int, string>();

            if (node.VisualType == NodeType.Function)
            {
                FunctionNode fNode = ((FunctionNode)node);
                if (fNode.GetAddReplicationState() == true)
                    nodeItems.Add(Configurations.AddReplicationGuides, "Add Replication Guides");
                if (fNode.GetRemoveReplicationState() == true)
                    nodeItems.Add(Configurations.RemoveReplicationGuides, "Remove Replication Guides");
            }
            nodeItems.Add(Configurations.DeleteNode, "Delete Node");

        }

        #endregion

        #region Internal Class Methods

        internal Dictionary<int, string> GetMenuItems()
        {
            Dictionary<int, string> nodeItems = new Dictionary<int, string>();
            if (libraryItemCollection != null)
            {
                if (libraryItemCollection.Count != 0)
                {
                    LibraryItem tempItem;
                    foreach (int id in libraryItemCollection.Keys)
                    {
                        if (libraryItemCollection.TryGetValue(id, out tempItem))
                        {
                            string tempString = tempItem.DisplayText;
                            if (tempItem.Children != null && tempItem.Children.Count > 0)
                            {
                                if (nodePart == NodePart.NorthEast)
                                    tempString = tempItem.DisplayText + " >";
                                else if (nodePart == NodePart.North)
                                    tempString = "< " + tempItem.DisplayText;
                            }
                            nodeItems.Add(id, tempString);
                        }
                    }
                }
                else
                {
                    if (nodePart == NodePart.NorthWest)
                        this.GetNodeOptions(this.nodeId, out nodeItems);
                    else if (nodeId == uint.MaxValue)
                        this.GetSelectionBoxMenuItems(out nodeItems);
                }

            }

            return nodeItems;
        }

        internal Dictionary<int, string> GetOverloadedItems()
        {
            Dictionary<int, string> nodeItems = new Dictionary<int, string>();
            if (overloadedItemCollection != null)
            {
                if (overloadedItemCollection.Count != 0)
                {
                    LibraryItem tempItem;
                    foreach (int id in overloadedItemCollection.Keys)
                    {
                        if (overloadedItemCollection.TryGetValue(id, out tempItem))
                            nodeItems.Add(id, tempItem.DisplayText);
                    }
                }
            }

            return nodeItems;

        }

        internal LibraryItem GetItem(int menuId)
        {
            LibraryItem selectedItem = null;
            if (libraryItemCollection != null)
            {
                foreach (int id in libraryItemCollection.Keys)
                {
                    if (id == menuId)
                        libraryItemCollection.TryGetValue(id, out selectedItem);
                }
            }

            if (selectedItem == null && overloadedItemCollection != null)
            {
                foreach (int id in overloadedItemCollection.Keys)
                {
                    if (id == menuId)
                        overloadedItemCollection.TryGetValue(id, out selectedItem);
                }
            }

            return selectedItem;
        }

        internal void PopulateOverloads(int selectedMenuId)
        {
            LibraryItem selectedItem = this.GetItem(selectedMenuId);
            ObservableCollection<LibraryItem> overloadedItems = CoreComponent.Instance.GetOverloads(selectedItem);
            this.PopulateItems(overloadedItems);
            return;
        }

        private void GetMethodsAndProperties(string[] assemblies, string[] returnTypes, ObservableCollection<LibraryItem> libraryItems)
        {
            List<string> parents = new List<string>();
            List<string> allParentList = new List<string>();
            List<string> allQualifiedNameList = new List<string>();
            ObservableCollection<LibraryItem> classLibraryItems;

            foreach (string assembly in assemblies)
            {
                foreach (string returnType in returnTypes)
                {
                    parents = GraphToDSCompiler.GraphUtilities.GetParentClasses(returnType);
                    classLibraryItems = CoreComponent.Instance.GetMethodsAndProperties(assembly, returnType);
                    if (classLibraryItems.Count == 0)
                        classLibraryItems = CoreComponent.Instance.GetMethodsAndProperties(GraphToDSCompiler.GraphUtilities.GetAssemblyFromClassName(returnType), returnType);
                    foreach (LibraryItem item in classLibraryItems)
                    {
                        if (allQualifiedNameList.Contains(item.QualifiedName) == false)
                        {
                            libraryItems.Add(item);
                            allQualifiedNameList.Add(item.QualifiedName);
                        }
                    }

                    foreach (string parent in parents)
                    {
                        if (allParentList.Contains(parent) == false)
                        {
                            allParentList.Add(parent);
                            classLibraryItems = CoreComponent.Instance.GetMethodsAndProperties(assembly, parent);
                            if (classLibraryItems.Count == 0)
                                classLibraryItems = CoreComponent.Instance.GetMethodsAndProperties(GraphToDSCompiler.GraphUtilities.GetAssemblyFromClassName(parent), parent);
                            foreach (LibraryItem item in classLibraryItems)
                                libraryItems.Add(item);
                        }
                    }
                }
            }
        }
        #endregion

    }
}
