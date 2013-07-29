using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
using System.Collections.ObjectModel;
using System.Collections;
using DesignScript.Editor.Core;

namespace DesignScript.Editor
{
    /// <summary>
    /// Interaction logic for InspectionViewControl.xaml
    /// </summary>
    public partial class InspectionViewControl : UserControl
    {
        private string currentQualifiedName = string.Empty;
        private static InspectionViewControl treeControl = null;
        private InspectionData currentEditingData = null;
        private InspectionDataCollection dataCollection = null;

        #region Public Operational Class Methods

        /// <summary>
        /// Constructor for InspectionViewControl
        /// </summary>
        internal InspectionViewControl()
        {
            InitializeComponent();
            inspectionView.KeyDown += new KeyEventHandler(OnWatchWindowKeyDown);
            inspectionView.PreviewMouseDoubleClick += new MouseButtonEventHandler(OnWatchWindowPreviewMouseDoubleClick);
            inspectionView.MouseLeftButtonUp += new MouseButtonEventHandler(OnWatchWindowMouseLeftButtonUp);

            dataCollection = new InspectionDataCollection();
            inspectionView.ItemsSource = dataCollection.Data;
        }

        /// <summary>
        /// Generic method to add item to the watch window. Creates and adds item to list of items
        /// in watch window and refreshes screen.
        /// </summary>
        /// <param name="name"> Name of the variable to be added </param>
        internal void AddInspectionVariable(string name)
        {
            name = ProcessDroppedExpression(name);
            if (!string.IsNullOrEmpty(name))
            {
                bool duplicateExpression = Solution.Current.AddWatchExpressions(name);
                if (duplicateExpression == true)
                {
                    TextEditorControl.Instance.DisplayStatusMessage(StatusTypes.Info, Configurations.WatchWindowDuplicateInfo, 3);
                    return;
                }
                dataCollection.AddInspectionData(name, false);
                inspectionView.Items.Refresh();
                RefreshInspectionView();
            }
        }

        /// <summary>
        /// Restores all variables stored in Solution file back into the Watch Window
        /// </summary>
        internal void PopulateVariablesFromSolution()
        {
            dataCollection.RemoveAllInspectionData();

            List<string> watchExpressions = new List<string>();
            Solution.Current.GetWatchExpressions(watchExpressions);
            if (watchExpressions.Count > 0)
            {
                foreach (string expression in watchExpressions)
                    dataCollection.AddInspectionData(expression, false);
            }

            inspectionView.Items.Refresh();
        }

        /// <summary>
        /// Clears watch window of all variables
        /// </summary>
        internal void RemoveAllVariables()
        {
            dataCollection.RemoveAllInspectionData();
            inspectionView.Items.Refresh();
        }

        /// <summary>
        /// This method acts as a 'refresh' button for the watch window, refreshing 
        /// all data attached to each variable
        /// </summary>
        internal void RefreshInspectionView()
        {
            UpdateExpansionStates();
            dataCollection.RefreshInspectionView();
            inspectionView.Items.Refresh();
            DoExpansion(dataCollection.Data);
        }

        #endregion

        #region Public Class Properties

        internal static InspectionViewControl Instance
        {
            get
            {
                if (null != treeControl)
                    return treeControl;

                treeControl = new InspectionViewControl();
                TextEditorControl.Instance.InsertWidget(EditorWidgetBar.Widget.Watch, treeControl);
                return treeControl;
            }
        }

        #endregion

        #region Protected Override Methods

        /// <summary>
        ///     This method is called when data is dropped onto the watch window from elsewhere.
        ///     Inspection window adds variab;es 
        /// </summary>
        /// <param name="e"> Used to extract the text from the dragged object</param>
        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);
            // If the DataObject contains string data, extract it.
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                string dataString = (string)e.Data.GetData(DataFormats.StringFormat);
                AddInspectionVariable(dataString);

                e.Effects = DragDropEffects.Copy;
            }
            e.Handled = true;
        }

        #endregion

        #region Private Helper Class Methods

        private string ProcessDroppedExpression(string name)
        {
            if (string.IsNullOrEmpty(name))
                return string.Empty;

            int lineBreakIndex = name.IndexOf('\n');
            if (-1 != lineBreakIndex)
                name = name.Substring(0, lineBreakIndex);

            int equalSignIndex = name.IndexOf('=');
            if (-1 != equalSignIndex)
                name = name.Substring(0, equalSignIndex);

            return (name.Trim()); // Remove white spaces.
        }

        private bool HighlightQualifiedName(string qualifiedName, bool highlight)
        {
            // Nope, "ToString" turns it into "True" and "False" which are invalid.
            string boolean = highlight ? "true" : "false";
            string expression = string.Format("{0}.Highlight({1})", qualifiedName, boolean);
            return (Solution.Current.ExecutionSession.ExecuteExpression(expression));
        }

        private InspectionViewItem ContainerFromItem(InspectionViewItem parentItem, object item)
        {
            ItemContainerGenerator generator = inspectionView.ItemContainerGenerator;
            if (null != parentItem)
                generator = parentItem.ItemContainerGenerator;

            return (generator.ContainerFromItem(item) as InspectionViewItem);
        }

        private InspectionViewItem ContainerFromItem(InspectionData inspectionData)
        {
            List<InspectionData> reversedDataList = new List<InspectionData>();
            InspectionData currentData = inspectionData;

            while (null != currentData)
            {
                reversedDataList.Insert(0, currentData);
                currentData = currentData.ParentData;
            }

            InspectionViewItem viewItem = null;
            foreach (InspectionData data in reversedDataList)
                viewItem = ContainerFromItem(viewItem, data);

            return viewItem;
        }

        private InspectionData GetSiblingItemFromData(InspectionData data)
        {
            if (null == data || (null == dataCollection))
                return null;

            ObservableCollection<InspectionData> collection = null;
            collection = (dataCollection.Data as ObservableCollection<InspectionData>);
            int index = collection.IndexOf(data);
            if (-1 == index)
                return null;

            // Move to the next sibling by default. If doing so causes index 
            // to go beyond the last valid item (not including the dummy empty 
            // item), then move back to look for the previous item.
            index = index + 1;
            if (index >= collection.Count - 1)
                index = index - 2;
            if (index < 0)
                return null; // There's no sibling.

            return collection[index];
        }

        /// <summary>
        /// An 'Items.Refresh' call erases all the previous expansion data. This recursive
        /// method complements 'CheckExpansion' method where it does the expansion of items
        /// that were expanded before the refresh method closed them.
        /// </summary>
        /// <param name="dataVal">
        /// The Observable Collection that holds all the data for the level below. 
        /// Each InspectionData.Derivations is also an Observable Collection.
        /// </param>
        /// <param name="parent">
        /// If parent is null, it means there is no parent and it is the top level data.
        /// Otherwise, send the parent item of the dataVal parameter
        /// </param>
        private void DoExpansion(ICollection<InspectionData> dataVal, InspectionViewItem parent = null)
        {
            if (dataVal == null)
                return;

            foreach (InspectionData item in dataVal)
            {
                if (parent != null)
                {
                    parent.IsExpanded = true;
                    parent.UpdateLayout();
                }

                InspectionViewItem treeListViewItem = ContainerFromItem(parent, item);
                if (null == treeListViewItem)
                    return;

                if (item.IsExpanded)
                    treeListViewItem.IsExpanded = true;

                if (item.HasItems && item.IsExpanded)
                    DoExpansion(item.Derivations, treeListViewItem);
            }
        }

        void UpdateExpansionStates()
        {
            if (null == dataCollection || (null == dataCollection.Data))
                return;

            UpdateExpansionStates(dataCollection.Data, null);
        }

        /// <summary>
        /// This method checks all the items in the Watch window and takes note 
        /// of what is expanded by toggling the 'isExpanded' flag in the InspectionData class.
        /// This information is later used by 'DoExpansion' to do the actual expansion
        /// </summary>
        /// <param name="dataVal">
        /// The Observable Collection that holds all the data for the level below. 
        /// Each InspectionData.Derivations is also an Observable Collection.
        /// </param>
        /// <param name="parent">
        /// If parent is null, it means there is no parent and it is the top level data.
        /// Otherwise, send the parent item of the dataVal parameter
        /// </param>
        void UpdateExpansionStates(ICollection<InspectionData> dataVal, InspectionViewItem parent = null)
        {
            foreach (InspectionData item in dataVal)
            {
                if (item.IsEmptyData)
                    break;

                InspectionViewItem treeListViewItem = ContainerFromItem(parent, item);
                if (null != treeListViewItem)
                    item.IsExpanded = treeListViewItem.IsExpanded;

                if (item.HasItems && item.IsExpanded)
                    UpdateExpansionStates(item.Derivations, treeListViewItem);
            }
        }

        /// <summary>
        /// This method, working with 'ItemExpansionCount' assumes each item in a treelist item has it's own index, 
        /// checks all expanded trees and gives the final index of a selected parent index. It is used in the 'Enter' key
        /// in the OnWatchWindowKeyDown method to calculate the position of the editing text box
        /// Example tree: a
        ///                -a[0]
        ///                -a[1]
        ///               b
        /// Assume b is selected. Parent index is 1 (it is a parent at level = 0)
        /// In this case, it will be returned as 3, i.e. the expanded index
        /// </summary>
        /// <param name="index"> Parent index of the watch window expression where 'Enter' was pressed</param>
        /// <returns> Expanded index of the watch window expression </returns>
        private int GetExpandedIndex(int index)
        {
            int expandedIndex = 0;

            ItemExpansionCount(dataCollection.Data, index, ref expandedIndex);

            return expandedIndex;
        }

        /// <summary>
        /// Recursive method to iterate through all expanded items and return the expanded index value. Works in
        /// conjunction with the 'GetExpandedIndex' method
        /// </summary>
        /// <param name="dataVal">
        /// The Observable Collection that holds all the data for the level below. 
        /// Each InspectionData.Derivations is also an Observable Collection.
        /// </param>
        /// <param name="indexToCheck"> Parent index of the watch window expression </param>
        /// <param name="expandedIndex"> Out parameter, returning expanded index of the watch expression </param>
        /// <param name="parent"> 
        /// If parent is null, it means there is no parent and it is the top level data.
        /// Otherwise, send the parent item of the dataVal parameter
        /// </param>
        private void ItemExpansionCount(ICollection<InspectionData> dataVal, int indexToCheck, ref int expandedIndex, TreeViewItem parent = null)
        {
            InspectionViewItem parentItem = parent as InspectionViewItem;

            foreach (InspectionData item in dataVal)
            {
                InspectionViewItem treeViewItem = ContainerFromItem(parentItem, item);
                if (treeViewItem.Level == 0)
                {
                    if (dataCollection.GetParentIndex(item) == indexToCheck)
                        break;
                }

                expandedIndex++;
                if (item.IsExpanded && item.HasItems)
                    ItemExpansionCount(item.Derivations, indexToCheck, ref expandedIndex, treeViewItem);
            }
        }

        /// <summary>
        /// Searches down the subtree of elements, using breadth-first approach,
        /// and returns the first descendant of type T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        private T FindDescendant<T>(DependencyObject obj) where T : DependencyObject
        {
            // Check if this object is the specified type
            if (obj is T)
                return obj as T;

            // Check for children
            int childrenCount = VisualTreeHelper.GetChildrenCount(obj);
            if (childrenCount < 1)
                return null;

            // First check all the children
            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is T)
                    return child as T;
            }

            // Then check the childrens children
            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = FindDescendant<T>(VisualTreeHelper.GetChild(obj, i));
                if (child != null && child is T)
                    return child as T;
            }

            return null;
        }

        private void ShowVariableEditBox(string initialString)
        {
            // Only clicking on the top-most level item should bring up edit box.
            InspectionViewItem parent = ContainerFromItem(null, inspectionView.SelectedItem);
            if (null == parent || (0 != parent.Level))
                return;

            currentEditingData = (InspectionData)inspectionView.SelectedItem;
            TextBlock nameBlock = FindDescendant<TextBlock>(parent);
            if (nameBlock != null)
            {
                txtVariableEditor.Height = nameBlock.Height;
                txtVariableEditor.Width = colName.Width;

                GeneralTransform transform = parent.TransformToAncestor(inspectionView);
                Point leftTopCoords = transform.Transform(new Point(0, 0));

                if (string.IsNullOrEmpty(initialString))
                    initialString = currentEditingData.DisplayText;

                txtVariableEditor.Margin = new Thickness(2, leftTopCoords.Y, 0, 0);
                txtVariableEditor.Text = initialString;
                txtVariableEditor.Visibility = Visibility.Visible;
                txtVariableEditor.Focus();
                txtVariableEditor.SelectAll();
            }
        }

        private void HideVariableEditBox(bool commitChanges, bool setFocusOnView)
        {
            if (null == currentEditingData)
                return; // There's no editing item now.

            if (currentEditingData.IsEmptyData == false)
                Solution.Current.RemoveWatchExpressions(currentEditingData.Expression);

            if (!string.IsNullOrEmpty(txtVariableEditor.Text) && (false != commitChanges))
            {
                int index = dataCollection.GetParentIndex(currentEditingData);
                if (index >= 0 && index < dataCollection.Data.Count)
                {
                    bool duplicateExpression = Solution.Current.AddWatchExpressions(txtVariableEditor.Text);
                    if (duplicateExpression == true)
                    {
                        TextEditorControl.Instance.DisplayStatusMessage(StatusTypes.Info, Configurations.WatchWindowDuplicateInfo, 3);
                        return;
                    }

                    dataCollection.UpdateVariableName(txtVariableEditor.Text, index);
                    RefreshInspectionView();
                }
            }

            InspectionViewItem viewItem = ContainerFromItem(null, currentEditingData);
            currentEditingData = null;

            if (null != viewItem)
                viewItem.IsSelected = true;

            txtVariableEditor.Visibility = Visibility.Hidden;
            UIElement elementToFocus = inspectionView;
            if (false != setFocusOnView)
                elementToFocus = TextEditorControl.Instance.textCanvas;

            elementToFocus.Focus();
        }

        #endregion

        #region Private Class Event Handlers

        /// <summary>
        /// Event trigger when a keyboard button is pressed in the watch window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">
        /// Keys accepted:
        /// Delete - to delete an item in the watch window
        /// Enter - to bring up the editing textbox for watch variables
        /// </param>
        private void OnWatchWindowKeyDown(object sender, KeyEventArgs e)
        {
            // Method to delete items from the watch window
            if (e.Key == Key.Delete)
            {
                InspectionData selectedData = inspectionView.SelectedItem as InspectionData;
                InspectionViewItem parent = ContainerFromItem(null, selectedData);
                if (null != parent && parent.Level == 0)
                {
                    // Get the sibling item so that we can move selection on it later.
                    InspectionData siblingData = GetSiblingItemFromData(selectedData);

                    InspectionData removedItem = (InspectionData)inspectionView.SelectedItem;
                    if (dataCollection.RemoveInspectionData(inspectionView.SelectedItem))
                    {
                        Solution.Current.RemoveWatchExpressions(removedItem.DisplayText);
                        inspectionView.Items.Refresh();
                    }

                    // Set focus back to the item.
                    InspectionViewItem siblingItem = ContainerFromItem(null, siblingData);
                    if (null != siblingItem)
                        siblingItem.IsSelected = true;
                }
            }
            // Method to allow user to bring up the editing textbox in the
            // watch window. This makes usage of watch window easy
            else if (e.Key == Key.Enter)
                ShowVariableEditBox(null);
            else if (e.Key >= Key.A && (e.Key <= Key.Z))
            {
                char character = TextEditorControl.GetKeyboardCharacter(e);
                ShowVariableEditBox(new string(character, 1));
            }
        }



        /// <summary>
        /// Clicking on a parent variable in the watch window should provide a space to edit
        /// the variable using the keyboard. This is done by using the textbox 'txtVariableEditor'.
        /// However this should not appear immediately on the first click of the expression as that
        /// gives bad user experience. Thus, this code checks to see if the user has already selected that
        /// watch expression once, and then on the second check, shows the 'txtVariableEditor'.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"> 
        /// MouseEventArgs object used to calculate the position of the mouse
        /// on the watch window, so that the position of displaying 'txtVariableEditor' can 
        /// be calculated.
        /// </param>
        private void OnWatchWindowPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            ShowVariableEditBox(null);
        }

        private void OnWatchWindowMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            InspectionData itemSelected = (InspectionData)inspectionView.SelectedItem;
            string newQualifiedName = string.Empty;
            if (null != itemSelected)
                newQualifiedName = itemSelected.GetQualifiedName();

            if (currentQualifiedName == newQualifiedName)
                return;

            // Unhighlight through the current qualified name, if any.
            if (string.IsNullOrEmpty(currentQualifiedName) == false)
                HighlightQualifiedName(currentQualifiedName, false);

            // Highlight the new qualified name...
            currentQualifiedName = string.Empty;
            if (string.IsNullOrEmpty(newQualifiedName) == false)
            {
                if (HighlightQualifiedName(newQualifiedName, true))
                    currentQualifiedName = newQualifiedName;
            }
        }

        /// <summary>
        /// Event triggered when typing in the txtVariableEditor textbox
        /// As it is a textbox, normal typing rules are followed by the base class of the textBox.
        /// This method adds an additional behaviour for the 'Enter' key.
        /// The Enter key causes the txtVariableEditor to disappear and add the edited variable back into
        /// the watch window. It also causes the txtVariableEditor to lose focus.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnVariableEditorKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                HideVariableEditBox(true, false);
            else if (e.Key == Key.Escape)
                HideVariableEditBox(false, false);
        }

        /// <summary>
        /// Event triggered when txtVariableEditor loses focus in any form (user clicks outside, presses Enter)
        /// Losing focus means the textbox has to be hidden from user once more and watch expression behind to be updated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnVariableEditorLostFocus(object sender, RoutedEventArgs e)
        {
            HideVariableEditBox(true, false);
        }

        /// <summary>
        /// 'Expander' is part of the toggle button defined in the XAML that shows the [+] or [-]
        /// sign when there are items under the current watch variable that can be inspected. 
        /// To maximise efficiency, the expansion of items must happen only when the expander is clicked
        /// when showing [+]. This generated the child items from the Virtual Machine.
        /// </summary>
        /// <param name="sender"> The sender is the Toggle Button</param>
        /// <param name="e"></param>
        private void OnExpanderClicked(object sender, RoutedEventArgs e)
        {
            ToggleButton expanderToggle = (ToggleButton)sender;
            if (null == expanderToggle)
                return;

            // The parent we are trying to locate here is the InspectionViewItem. The 
            // expander is buried beneath several layers of stackpanels and grids under the Item
            // and therefore we have to make numerous calls to find the InspectionViewItem.
            InspectionViewItem parent = (InspectionViewItem)VisualTreeHelper.GetParent
                (VisualTreeHelper.GetParent
                (VisualTreeHelper.GetParent
                (VisualTreeHelper.GetParent
                (VisualTreeHelper.GetParent
                (VisualTreeHelper.GetParent(expanderToggle))))));

            object data = ((null == parent) ? null : parent.DataContext);
            InspectionData selectedData = data as InspectionData;
            if (null == selectedData)
                return;

            // expanderToggle.IsChecked == true means the [+] --> [-]
            if (expanderToggle.IsChecked == true)
            {
                parent.IsExpanded = false;
                dataCollection.ExpandInspection(selectedData);
                parent.IsExpanded = true;

                UpdateExpansionStates();
                inspectionView.Items.Refresh();
                DoExpansion(dataCollection.Data);

#if false
                parent.IsExpanded = false;
                InspectionData selectedData = (InspectionData)parent.DataContext;
                dataCollection.ExpandInspection(selectedData);

                // If there is a large amount of data, the user may have to wait. 
                // This is indicated by updating the mouse cursor.
                Mouse.SetCursor(Cursors.Wait);
                parent.IsSelected = true;
                parent.IsExpanded = true;
                selectedData.IsExpanded = true;

                UpdateExpansionStates();
                inspectionView.Items.Refresh();
                DoExpansion(dataCollection.Data);
                Mouse.SetCursor(Cursors.Arrow);
#endif
            }

            // Select the item that is currently being expanded/collapsed.
            InspectionViewItem itemToSelect = ContainerFromItem(selectedData);
            if (null != itemToSelect)
                itemToSelect.IsSelected = true;
        }

        private void OnInspectionClearAll(object sender, RoutedEventArgs e)
        {
            RemoveAllVariables();
            Solution.Current.RemoveAllWatchExpressions();
            inspectionView.Items.Refresh();
        }

        private void OnDeleteClicked(object sender, RoutedEventArgs e)
        {
            InspectionData removedItem = (InspectionData)inspectionView.SelectedItem;
            if (removedItem != null)
            {
                if (dataCollection.RemoveInspectionData(inspectionView.SelectedItem))
                {
                    Solution.Current.RemoveWatchExpressions(removedItem.DisplayText);
                    inspectionView.Items.Refresh();
                }
            }
        }

        private void TreeViewItem_MouseRightButtonDown(object sender, MouseEventArgs e)
        {
            TreeViewItem item = sender as TreeViewItem;
            if (item != null)
            {
                item.Focus();
                e.Handled = true;
            }
        }

        #endregion


    }
}
