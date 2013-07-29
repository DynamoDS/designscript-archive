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
using DesignScriptStudio.Graph.Core;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Controls.Primitives;
using System.Globalization;
using System.ComponentModel;
using System.Collections.Specialized;
using Microsoft.Win32;

namespace DesignScriptStudio.Graph.Ui
{
    public partial class LibraryView : UserControl
    {
        enum ExpansionFlag
        {
            Collapsed,
            ShallowExpanded,
            DeepExpanded
        }

        enum LibraryOperationStatus
        {
            Operation,
            Search
        }

        public struct SearchItem
        {
            internal LibraryItemViewModel searchItem;
            internal string searchName;
        }

        public struct DecoratedString
        {
            internal bool isHilighted;
            internal string text;
        }

        LibraryItem library = null;
        LibraryItem draggedItem;
        LibraryTreeViewModel libraryTree;
        GraphControl graphControl = null;
        ExpansionFlag currentExpansion = ExpansionFlag.Collapsed;

        private LibraryOperationStatus operationStatus = LibraryOperationStatus.Operation;
        private List<SearchItem> searchList = null;
        private List<SearchItem> resultList = null;
        private string keyword;

        private ContextMenu folderMenu;
        private ContextMenu externalFolderMenu;
        private ContextMenu itemMenu;

        double restoreHeight;
        double restoreContentHeight;

        bool internalRequestBeingIntoView = false;

        //for library search
        BackgroundWorker backgroundSearcher;

        public ContextMenu FolderMenu
        {
            get
            {
                if (folderMenu == null)
                {
                    folderMenu = new ContextMenu();

                    MenuItem expandItem = new MenuItem();
                    expandItem.Header = "Expand";
                    expandItem.Click += OnTreeViewItemExpand;
                    folderMenu.Items.Add(expandItem);

                    MenuItem collapseItem = new MenuItem();
                    collapseItem.Header = "Collapse";
                    collapseItem.Click += OnTreeViewItemCollapse;
                    folderMenu.Items.Add(collapseItem);
                }

                return folderMenu;
            }
        }

        public ContextMenu ExternalFolderMenu
        {
            get
            {
                if (externalFolderMenu == null)
                {
                    externalFolderMenu = new ContextMenu();

                    //MenuItem removeItem = new MenuItem();
                    //removeItem.Header = "Remove";
                    //removeItem.Click += OnTreeViewItemRemove;
                    //externalFolderMenu.Items.Add(removeItem);

                    //MenuItem refreshItem = new MenuItem();
                    //refreshItem.Header = "Refresh";
                    //refreshItem.Click += OnTreeViewItemRefresh;
                    //externalFolderMenu.Items.Add(refreshItem);

                    MenuItem expandItem = new MenuItem();
                    expandItem.Header = "Expand";
                    expandItem.Click += OnTreeViewItemExpand;
                    externalFolderMenu.Items.Add(expandItem);

                    MenuItem collapseItem = new MenuItem();
                    collapseItem.Header = "Collapse";
                    collapseItem.Click += OnTreeViewItemCollapse;
                    externalFolderMenu.Items.Add(collapseItem);
                }

                return externalFolderMenu;
            }
        }

        public ContextMenu ItemMenu
        {
            get
            {
                if (itemMenu == null)
                {
                    itemMenu = new ContextMenu();

                    MenuItem addToCanvasItem = new MenuItem();
                    addToCanvasItem.Header = "Add to canvas";
                    addToCanvasItem.Click += OnTreeViewItemAddToCanvas;
                    itemMenu.Items.Add(addToCanvasItem);
                }

                return itemMenu;
            }
        }

        #region Public Class Operational Methods

        public LibraryView(GraphControl graphControl)
        {
            InitializeComponent();

            this.graphControl = graphControl;

            library = new LibraryItem(NodeType.None, String.Empty);
            library.AddChildItem(new LibraryItem(NodeType.None, Configurations.LoadingMessage));
            libraryTree = new LibraryTreeViewModel(library, this);

            searchList = new List<SearchItem>();

            backgroundSearcher = new BackgroundWorker();
            backgroundSearcher.DoWork += new DoWorkEventHandler(BackgroundSearcherDoWork);
            backgroundSearcher.ProgressChanged += new ProgressChangedEventHandler(BackgroundSearcherProgressChanged);
            backgroundSearcher.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundSearcherRunWorkerCompleted);
            backgroundSearcher.WorkerReportsProgress = true;
            backgroundSearcher.WorkerSupportsCancellation = true;

            base.DataContext = libraryTree;
        }

        internal void BindLibraryItemData(LibraryItem rootItem)
        {
            library = rootItem;
            TextBox searchTextBox = FindVisualChild<TextBox>(this, "SearchTextBox");
            searchTextBox.Text = string.Empty;

            FinishLoadingLibrary();
        }

        internal void HandleAddNew()
        {
            string fileFilter = "Library Files (*.dll, *.ds)|*.dll;*.ds|"
                              + "Assembly Library Files (*.dll)|*.dll|"
                              + "DesignScript Files (*.ds)|*.ds|"
                //+ ".NET Assembly Library Files (*.exe)|*.exe|"
                              + "All Files (*.*)|*.*";

            // Displays an OpenFileDialog for user to select a file.
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = fileFilter;
            openFileDialog.Title = UiStrings.TitleImporting;
            openFileDialog.Multiselect = true;
            openFileDialog.RestoreDirectory = true;

            bool? result = openFileDialog.ShowDialog();
            if (result.HasValue && (result.Value == true))
            {
                foreach (string filePath in openFileDialog.FileNames)
                {
                    if (!filePath.ToLower().EndsWith(".dll") && !filePath.ToLower().EndsWith(".ds") && !filePath.ToLower().EndsWith(".exe"))
                    {
                        graphControl.AddFeedbackMessage(ResourceNames.Error, UiStrings.IllegalImporting);
                        return;
                    }

                    if (filePath.ToLower().EndsWith(".ds"))
                        graphControl.CurrentGraphCanvas.Controller.DoImportScript(filePath);
                    else
                    {
                        string fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
                        ClassFactory.CurrCoreComponent.ImportAssembly(filePath, null, false);
                    }
                }

                FinishLoadingLibrary();
            }
        }

        internal void FinishLoadingLibrary()
        {
            libraryTree = new LibraryTreeViewModel(library, this);
            InitializeSearchList();
            base.DataContext = null;
            base.DataContext = libraryTree;
        }

        internal void SetHeight(double newHeight)
        {
            double contentMaxHeight = newHeight - Configurations.LibraryTopMargin - Configurations.LibraryHeaderHeight;

            if (this.content.Height > contentMaxHeight)
            {
                this.Height = contentMaxHeight + Configurations.LibraryHeaderHeight;
                this.content.Height = contentMaxHeight;
            }
        }

        #endregion

        #region Private Class Event Handlers

        private void OnTreeViewItemDoubleClick(object sender, MouseEventArgs e)
        {
            LibraryItemViewModel selectedItem = ((TreeViewItem)sender).Header as LibraryItemViewModel;
            if (selectedItem != null)
            {
                LibraryItem selectedLibraryItem = selectedItem.LibraryItem;
                if (selectedLibraryItem.ItemType != NodeType.None)
                    graphControl.CurrentGraphCanvas.VisualHost.HandleAddNodeToCanvas(selectedLibraryItem);
            }
            e.Handled = true;
        }

        private void OnTreeViewItemRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            if (internalRequestBeingIntoView)
                internalRequestBeingIntoView = false;
            else
                e.Handled = true;
        }

        private void OnTreeViewItemClick(object sender, MouseEventArgs e)
        {
            TreeViewItem treeViewItem = ((Border)sender).TemplatedParent as TreeViewItem;

            if (treeViewItem != null)
                treeViewItem.IsSelected = true;

            //e.Handled = true;
        }

        private void OnTreeViewItemExpandCollapse(object sender, MouseEventArgs e)
        {
            LibraryItemViewModel libraryItem = null;
            TreeViewItem treeViewItem = ((Border)sender).TemplatedParent as TreeViewItem;
            if (treeViewItem != null)
                libraryItem = treeViewItem.Header as LibraryItemViewModel;

            if (libraryItem == null)
                return;

            libraryItem.IsSelected = true;

            if (libraryItem.Children != null && libraryItem.Children.Count > 0)
            {
                if (libraryItem.IsExpanded && libraryItem.IsChildVisible)
                    libraryItem.IsExpanded = false;
                else
                {
                    libraryItem.IsExpanded = true;

                    // make the filtered out item visible
                    ExpandChildItem(libraryItem);
                }
            }

            //Request Bring into View
            if (libraryItem.IsExpanded && libraryItem.Level == 0)
            {
                internalRequestBeingIntoView = true;
                treeViewItem.BringIntoView();

                // to ensure the header of the item is shown
                Border bd = FindVisualChild<Border>(treeViewItem, "Exp");
                if (bd != null)
                {
                    internalRequestBeingIntoView = true;
                    bd.BringIntoView();
                }
            }

            e.Handled = true;
        }

        private void OnTreeViewItemMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                draggedItem = null;

                Point currentPosition = e.GetPosition(InternalTreeView);
                LibraryItemViewModel draggedView = (LibraryItemViewModel)InternalTreeView.SelectedItem;
                if (draggedView != null)
                    draggedItem = draggedView.LibraryItem;
                if ((draggedItem != null) && (draggedItem.Level > 0))
                {
                    DragDropEffects dragEffect;

                    while ((draggedItem.ItemType == NodeType.None) && (draggedItem.Children != null))
                    {
                        draggedItem = draggedItem.Children[0];
                    }
                    if (draggedItem.ItemType != NodeType.None)
                        dragEffect = DragDrop.DoDragDrop(InternalTreeView, draggedItem,
                            DragDropEffects.Move);
                }
            }
        }

        private void OnTreeViewItemAddToCanvas(object sender, RoutedEventArgs e)
        {
            LibraryItemViewModel libraryItem = ((MenuItem)sender).DataContext as LibraryItemViewModel;

            if (libraryItem != null)
                graphControl.CurrentGraphCanvas.VisualHost.HandleAddNodeToCanvas(libraryItem.LibraryItem);
        }

        private void OnTopThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            double contentMaxHeight = this.graphControl.canvasScrollViewer.ActualHeight - Configurations.LibraryTopMargin - Configurations.LibraryHeaderHeight;

            if (this.content.Height - e.VerticalChange > contentMaxHeight)//reach the maximun
            {
                this.Height = contentMaxHeight + Configurations.LibraryHeaderHeight;
                this.content.Height = contentMaxHeight;
                this.topThumb.ReleaseMouseCapture();
            }
            else if ((this.Height - e.VerticalChange < Configurations.LibraryMinHeight) && (e.VerticalChange > 0))//reach the minimum
            {
                restoreHeight = Configurations.LibraryHeight;
                restoreContentHeight = Configurations.LibraryContentHeight;
                MinimizeLibrary();
                this.topThumb.ReleaseMouseCapture();
            }
            else
            {
                if (this.content.Visibility == Visibility.Collapsed)//minimized window
                {
                    MaximizeLibrary();
                }
                this.Height -= e.VerticalChange;
                this.content.Height -= e.VerticalChange;
            }
        }

        private void OnTopThumbMouseUp(object sender, MouseEventArgs e)
        {
            if (this.Height < Configurations.LibraryMinHeight)
            {
                restoreHeight = Configurations.LibraryHeight;
                restoreContentHeight = Configurations.LibraryContentHeight;
                MinimizeLibrary();
            }
        }

        private void OnMaxiMinimizeClick(object sender, RoutedEventArgs e)
        {
            graphControl.SetCanvasFocus();

            if (this.content.Visibility == Visibility.Visible)//minimize
            {
                restoreHeight = this.Height;
                restoreContentHeight = this.content.Height;
                MinimizeLibrary();
            }
            else//maximize
            {
                this.Height = restoreHeight;
                this.content.Height = restoreContentHeight;
                MaximizeLibrary();
            }
        }

        private void MinimizeLibrary()
        {
            this.Height = Configurations.LibraryHeaderHeight;
            this.content.Height = 0;
            this.content.Visibility = Visibility.Collapsed;
            this.MinimizeView.Visibility = Visibility.Hidden;
            this.MaximizeView.Visibility = Visibility.Visible;
        }

        private void MaximizeLibrary()
        {
            this.content.Visibility = Visibility.Visible;
            this.MinimizeView.Visibility = Visibility.Visible;
            this.MaximizeView.Visibility = Visibility.Hidden;
        }

        private void OnAddNewClick(object sender, RoutedEventArgs e)
        {
            graphControl.SetCanvasFocus();
            HandleAddNew();
        }

        private void OnExpandClick(object sender, RoutedEventArgs e)
        {
            graphControl.SetCanvasFocus();

            ToggleLibrary(true);
        }

        private void OnCollapseClick(object sender, RoutedEventArgs e)
        {
            graphControl.SetCanvasFocus();

            ToggleLibrary(false);
        }

        private void OnTreeViewItemExpand(object sender, RoutedEventArgs e)
        {
            LibraryItemViewModel libraryItem = ((MenuItem)sender).DataContext as LibraryItemViewModel;

            if (libraryItem == null)
                return;

            libraryItem.IsSelected = true;
            if (!libraryItem.IsExpanded)
            {
                libraryItem.IsExpanded = true;
                ExpandChildItem(libraryItem);
            }
        }

        private void OnTreeViewItemCollapse(object sender, RoutedEventArgs e)
        {
            LibraryItemViewModel libraryItem = ((MenuItem)sender).DataContext as LibraryItemViewModel;

            if (libraryItem == null)
                return;

            libraryItem.IsSelected = true;
            libraryItem.IsExpanded = false;
        }

        private void OnTreeViewItemRemove(object sender, RoutedEventArgs e)
        {
            LibraryItemViewModel libraryItem = ((MenuItem)sender).DataContext as LibraryItemViewModel;

            if (libraryItem == null)
                return;

            ClassFactory.CurrCoreComponent.RemoveAssembly(libraryItem.DisplayText);
        }

        private void OnTreeViewItemRefresh(object sender, RoutedEventArgs e)
        {
            LibraryItemViewModel libraryItem = ((MenuItem)sender).DataContext as LibraryItemViewModel;

            if (libraryItem == null)
                return;

            ClassFactory.CurrCoreComponent.RefreshAssembly(libraryItem.DisplayText);
        }

        private void OnLibraryTreeViewClick(object sender, RoutedEventArgs e)
        {
            content.Focus();
            ClearSearch();
        }

        private void OnLibraryKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                ClearSearch();
                return;
            }

            char typedChar = e.Key.ToString()[0];

            if ((char.IsSymbol(typedChar) || char.IsLetterOrDigit(typedChar))
                && !SearchTextBox.IsFocused)
            {
                SearchTextBox.Focus();
            }
        }

        private void OnSearchBarClick(object sender, RoutedEventArgs e)
        {
            if (!SearchTextBox.IsFocused)
            {
                SearchTextBox.Focus();
            }
        }

        private void OnSearchTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            keyword = ((TextBox)sender).Text.ToLower().Trim();

            if (backgroundSearcher.IsBusy)
                //backgroundSearcher.CancelAsync();
                return;

            if (keyword.Length == 0)
            {
                ResetTreeView();
                return;
            }

            operationStatus = LibraryOperationStatus.Search;
            backgroundSearcher.RunWorkerAsync();
        }

        private void OnCloseSearch(object sender, RoutedEventArgs e)
        {
            ClearSearch();
        }

        private void BackgroundSearcherDoWork(object sender, DoWorkEventArgs e)
        {
            if (backgroundSearcher.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            InitializeSearch();
            resultList = SearchLibrary(searchList, keyword);

        }

        private void BackgroundSearcherProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }

        private void BackgroundSearcherRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            UpdateLibraryItemVisibility(resultList);
            UpdateTreeView();
            operationStatus = LibraryOperationStatus.Operation;
        }

        #endregion

        #region Private Class Helper Methods

        #region Search Helpers

        private void InitializeSearchList()
        {
            searchList = new List<SearchItem>();

            foreach (LibraryItemViewModel item in libraryTree.RootItems)
            {
                if (item.DisplayText.ToLower() == Configurations.NoResultMessage.ToLower())
                    continue;

                SearchItem searchItem = new SearchItem();
                searchItem.searchItem = item;
                searchItem.searchName = item.DisplayText.ToLower(); //ignore the letter case
                searchList.Add(searchItem);

                InitializeSearchList(item);
            }
        }

        private void InitializeSearchList(LibraryItemViewModel libraryItem)
        {
            if (libraryItem.Children == null)
                return;

            foreach (LibraryItemViewModel item in libraryItem.Children)
            {
                SearchItem searchItem = new SearchItem();
                searchItem.searchItem = item;
                searchItem.searchName = item.DisplayText.ToLower(); //ignore the letter case
                searchList.Add(searchItem);

                InitializeSearchList(item);
            }
        }

        private void InitializeSearch()
        {
            foreach (SearchItem resultItem in searchList)
                resultItem.searchItem.Visibility = Visibility.Collapsed;
        }

        private void BeginSearch()
        {

        }

        private void EndSearch()
        {

        }

        private void ClearSearch()
        {
            operationStatus = LibraryOperationStatus.Operation;
            ResetTreeView();
            SearchTextBox.Text = "";
            //graphControl.SetCanvasFocus();
        }

        private List<SearchItem> SearchLibrary(List<SearchItem> searchList, string keyword)
        {
            List<SearchItem> resultList = new List<SearchItem>();

            foreach (SearchItem searchItem in searchList)
            {
                if (searchItem.searchName.Contains(keyword))
                    resultList.Add(searchItem);
            }

            return resultList;
        }

        private void UpdateLibraryItemVisibility(List<SearchItem> resultList)
        {
            foreach (SearchItem resultItem in resultList)
            {
                resultItem.searchItem.Visibility = Visibility.Visible;
            }
        }

        private void UpdateTreeView()
        {
            for (int i = 1; i < libraryTree.RootItems.Count(); i++)
            {
                UpdateTreeViewItem(libraryTree.RootItems[i]);
            }

            LibraryItemViewModel notFoundItem = libraryTree.RootItems[0];

            if (resultList.Count() == 0)
            {
                notFoundItem.Visibility = Visibility.Visible;
                for (int i = 1; i < libraryTree.RootItems.Count(); i++)
                {
                    libraryTree.RootItems[i].Visibility = Visibility.Collapsed;
                }
            }
            else
                notFoundItem.Visibility = Visibility.Collapsed;
        }

        private bool UpdateTreeViewItem(LibraryItemViewModel libraryItem)
        {
            //if (libraryItem.Children.Count < 1)
            //    return false;

            bool isChildVisible = false;

            // update child
            foreach (LibraryItemViewModel childItem in libraryItem.Children)
            {
                bool isVisible = false;

                isVisible = UpdateTreeViewItem(childItem);
                if (isVisible)
                    isChildVisible = true;
            }

            if (libraryItem.Visibility == Visibility.Visible)
            {
                // libraryItem matches the search keyword, highlight the text
                HighlightItemHeader(libraryItem);

                // if the root is the fully match the search keywords, then all the child of the root should be shown
                if (libraryItem.DisplayText.ToLower() == keyword.ToLower())
                    ExpandChildItem(libraryItem);
                else if (isChildVisible)
                    libraryItem.IsExpanded = true;
            }
            else
            {
                ResetItemHeader(libraryItem);
                if (isChildVisible) //if there is child that is visible, the root should be visible as well,
                {
                    libraryItem.Visibility = Visibility.Visible;
                    libraryItem.IsExpanded = true;
                }
                else if (libraryItem.Level == 0)
                {
                    libraryItem.Visibility = Visibility.Visible;
                }
                else
                {
                    libraryItem.Visibility = Visibility.Collapsed;
                    return false;   // when there is no child visible and it self is not visible, the whole item is hidden
                }
            }

            return true;
        }

        private void ResetTreeView()
        {
            libraryTree.RootItems[0].Visibility = Visibility.Collapsed;

            for (int i = 1; i < libraryTree.RootItems.Count; i++)
            {
                LibraryItemViewModel libraryItem = libraryTree.RootItems[i];
                ResetTreeViewItem(libraryItem);
                libraryItem.Visibility = Visibility.Visible;
                libraryItem.IsExpanded = false;
                ResetItemHeader(libraryItem);
            }
        }

        private void ResetTreeViewItem(LibraryItemViewModel libraryItem)
        {
            if (libraryItem.Children.Count <= 0)
                return;

            foreach (LibraryItemViewModel childItem in libraryItem.Children)
            {
                ResetTreeViewItem(childItem);
                childItem.Visibility = Visibility.Visible;
                childItem.IsExpanded = false;
                ResetItemHeader(childItem);
            }
        }

        private void ResetItemHeader(LibraryItemViewModel libraryItem)
        {
            libraryItem.PrePiece = libraryItem.DisplayText;
            libraryItem.HighlightPiece = string.Empty;
            libraryItem.PostPiece = string.Empty;
        }

        private void HighlightItemHeader(LibraryItemViewModel libraryItem)
        {
            String displayText = libraryItem.DisplayText.ToLower();
            int index = displayText.IndexOf(keyword.ToLower());

            libraryItem.PrePiece = libraryItem.DisplayText.Substring(0, index);
            libraryItem.HighlightPiece = libraryItem.DisplayText.Substring(index, keyword.Count());
            libraryItem.PostPiece = libraryItem.DisplayText.Substring(index + keyword.Count());
        }

        #endregion

        private void ExpandChildItem(LibraryItemViewModel libraryItem)
        {
            if (libraryItem.Children.Count <= 0)
                return;

            // Set the visibility to true to all the child 
            // (applies to items expand items after search)
            // Expand the child if it's a overload folder
            foreach (LibraryItemViewModel childItem in libraryItem.Children)
            {
                childItem.Visibility = Visibility.Visible;

                // expand overloaded method
                if (childItem.Children != null && childItem.Children.Count > 0 && childItem.IsOverloaded)
                    ExpandChildItem(childItem);
            }

            libraryItem.IsExpanded = true;
        }

        private void ExpandAllChildItem(LibraryItemViewModel libraryItem)
        {
            if (libraryItem.Children.Count <= 0)
                return;

            foreach (LibraryItemViewModel childItem in libraryItem.Children)
            {
                childItem.Visibility = Visibility.Visible;
                ExpandAllChildItem(childItem);
            }

            libraryItem.IsExpanded = true;
        }

        private void ToggleLibrary(bool expand)
        {
            if (false != expand)
            {
                if (currentExpansion == ExpansionFlag.Collapsed)
                    currentExpansion = ExpansionFlag.ShallowExpanded;
                else if (currentExpansion == ExpansionFlag.ShallowExpanded)
                    currentExpansion = ExpansionFlag.DeepExpanded;
                else
                    return;
            }
            else
            {
                int level = GetExpansionLevel();

                if (level > 2)//(currentExpansion == ExpansionFlag.DeepExpanded)
                    currentExpansion = ExpansionFlag.ShallowExpanded;
                else //if (currentExpansion == ExpansionFlag.ShallowExpanded)
                    currentExpansion = ExpansionFlag.Collapsed;
                //else
                //    return;
            }

            foreach (LibraryItemViewModel item in libraryTree.RootItems)
            {
                item.IsExpanded = currentExpansion != ExpansionFlag.Collapsed;
                if (item.Children.Count > 0)
                    ToggleLibraryItem(item, currentExpansion);
                if (item.IsExpanded)    // expand overloaded method
                    ExpandChildItem(item);
            }

        }

        private void ToggleLibraryItem(LibraryItemViewModel libraryItem, ExpansionFlag expansion)
        {
            bool expanded = expansion == ExpansionFlag.DeepExpanded;
            foreach (LibraryItemViewModel item in libraryItem.Children)
            {
                item.IsExpanded = expanded;
                if (item.Children.Count > 0)
                    ToggleLibraryItem(item, expansion);
                if (expanded) // expand overloaded method
                    ExpandChildItem(item);
            }
        }

        private int GetExpansionLevel()
        {
            int maxExpansionLevel = -1;

            for (int i = 1; i < libraryTree.RootItems.Count; i++)
            {
                LibraryItemViewModel item = libraryTree.RootItems[i];
                int expansionLevel = GetExpansionLevel(item);
                if (maxExpansionLevel < expansionLevel)
                    maxExpansionLevel = expansionLevel;
            }

            return maxExpansionLevel;
        }

        private int GetExpansionLevel(LibraryItemViewModel libraryItem)
        {
            int MaxChildExpansionLevel = -1;

            if (libraryItem.Children.Count < 1 || !libraryItem.IsExpanded)
                return 1;
            if (libraryItem.IsExpanded && libraryItem.IsOverloaded) // overloaded function folder
                return 1;

            foreach (LibraryItemViewModel item in libraryItem.Children)
            {
                int childExpansionLevel = GetExpansionLevel(item);
                if (MaxChildExpansionLevel < childExpansionLevel)
                    MaxChildExpansionLevel = childExpansionLevel;
            }

            return MaxChildExpansionLevel + 1;
        }

        private childItem FindVisualChild<childItem>(DependencyObject obj)
                                            where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        private childItem FindVisualChild<childItem>(DependencyObject obj, string name)
                                            where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem && ((FrameworkElement)child).Name == name)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child, name);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        #endregion
    }
}
