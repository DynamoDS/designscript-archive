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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Controls.Primitives;
using DesignScriptStudio.Graph.Core;
using System.IO;

namespace DesignScriptStudio.Graph.Ui
{
    public partial class GraphTabControl : UserControl
    {
        public class GraphTabItem
        {
            private bool isSelected;
            private GraphTabControl graphTabControl;
            private GraphControl graphControl;

            public GraphTabItem(GraphTabControl graphTabControl, GraphControl graphControl)
            {
                this.graphTabControl = graphTabControl;
                this.graphControl = graphControl;
            }

            public string Header { get; set; }
            public GraphCanvas Canvas { get; set; }
            public bool IsSelected
            {
                get
                {
                    return isSelected;
                }
                set
                {
                    if (value == true)
                    {
                        isSelected = true;
                        if (Canvas != null)
                        {
                            Canvas.Visibility = Visibility.Visible;
                            graphControl.ActivateCanvas(Canvas);
                        }
                        TabItem tabItem = graphTabControl.FindTabItem(this);
                        if (tabItem != null)// && !graphTabControl.AnimationOn)
                        {
                            tabItem.BringIntoView();
                            tabItem.SetValue(Panel.ZIndexProperty, 100);
                        }
                    }
                    else if (value == false)
                    {
                        isSelected = false;
                        if (Canvas != null)
                        {
                            Canvas.Visibility = Visibility.Collapsed;
                            graphControl.SnapShotCanvas(Canvas.Controller.Identifier);
                        }
                        TabItem tabItem = graphTabControl.FindTabItem(this);
                        if (tabItem != null)
                            tabItem.SetValue(Panel.ZIndexProperty, 20);
                    }
                }
            }
            public bool IsInEditMode { get; set; }
        }

        private GraphControl graphControl;
        private ObservableCollection<GraphTabItem> tabs;

        // for tab renaming
        private bool IsRenameTab = false;
        // for tab rearranging
        private double draggedTabDeltaX = 0;
        private TabItem draggedTab;
        private GraphTabItem selectedItem;

        public static bool animationOn = false;
        public static bool mouseDown = false;

        private TabPanel tabPanel;
        private ScrollViewer scrollViewer;

        public GraphTabControl(GraphControl graphControl)
        {
            InitializeComponent();

            this.graphControl = graphControl;
            tabs = new ObservableCollection<GraphTabItem>();
            TabControl.ItemsSource = tabs;
            AllTabs.ItemsSource = tabs;

            NameScope.SetNameScope(this, new NameScope());

            this.Loaded += new RoutedEventHandler(OnGraphTabControlLoaded);
        }

        #region internal operational methods

        internal void AddTab(string tabName, GraphCanvas canvas)
        {
            GraphTabItem newTab = new GraphTabItem(this, this.graphControl);
            newTab.Header = tabName;
            newTab.Canvas = canvas;
            newTab.IsInEditMode = false;
            tabs.Add(newTab);
            TabControl.SelectedIndex = tabs.Count() - 1;
        }

        internal bool CloseTab(Object tabItem)
        {
            GraphTabItem item;
            if (tabItem is TabItem)
                item = ((TabItem)tabItem).Header as GraphTabItem;
            else
                item = tabItem as GraphTabItem;

            if (item == null)
                return false;

            if (!CloseTabCanvas(item)) // failed to close the file
                return false;

            if (item.IsSelected && tabs.Count > 1) // select next tab
            {
                int tabIndex = tabs.IndexOf(item);
                int nextTabIndex;
                if (tabIndex < tabs.Count - 1)
                    nextTabIndex = tabIndex + 1;
                else
                    nextTabIndex = tabIndex - 1;

                TabControl.SelectedIndex = nextTabIndex;
            }

            tabs.Remove(item);
            if (tabs.Count > 0)
                graphControl.UpdateCurrentCanvas(tabs[TabControl.SelectedIndex].Canvas.Controller.Identifier);

            return true;
        }

        internal void SwitchTab()
        {
            int nextTabIndex;
            if (TabControl.SelectedIndex == tabs.Count - 1)
                nextTabIndex = 0;
            else
                nextTabIndex = TabControl.SelectedIndex + 1;

            TabControl.SelectedIndex = nextTabIndex;
        }

        private bool CloseTabs(List<GraphTabItem> tabsToRemove)
        {
            List<GraphTabItem> removed = new List<GraphTabItem>();
            bool iscomplete = true;

            foreach (GraphTabItem item in tabsToRemove)
            {
                if (CloseTabCanvas(item))
                    tabs.Remove(item);
                else
                    iscomplete = false;
            }

            return iscomplete;
        }


        internal bool CloseAllTab()
        {
            List<GraphTabItem> tabsToRemove = new List<GraphTabItem>();

            foreach (GraphTabItem item in tabs)
                tabsToRemove.Add(item);

            return CloseTabs(tabsToRemove);
        }

        internal bool CloseAllButTab(object tabItem)
        {
            GraphTabItem item = tabItem as GraphTabItem;
            if (item == null)
                return false;

            List<GraphTabItem> tabsToRemove = new List<GraphTabItem>();

            foreach (GraphTabItem clearItem in tabs)
            {
                if (clearItem != item)
                    tabsToRemove.Add(clearItem);
            }

            return CloseTabs(tabsToRemove);
        }

        internal bool CloseAllRightTabCanExecute(object tabItem)
        {
            GraphTabItem item = tabItem as GraphTabItem;
            if (item == null)
                return false;

            if (tabs.IndexOf(item) == tabs.Count() - 1)
                return false;

            return true;
        }

        internal bool CloseAllRightTab(object tabItem)
        {
            GraphTabItem item = tabItem as GraphTabItem;
            if (item == null)
                return false;

            List<GraphTabItem> tabsToRemove = new List<GraphTabItem>();

            for (int i = tabs.IndexOf(item) + 1; i < tabs.Count(); i++)
            {
                tabsToRemove.Add(tabs[i]);
            }

            return CloseTabs(tabsToRemove);
        }

        internal void RenameTab(DependencyObject tabItem)
        {
            if (tabItem == null)
                return;
            BeginRename((TabItem)tabItem);
        }

        internal void UpdateTabText(uint identifier)
        {
            GraphTabItem item = FindGraphTabItem(identifier);
            if (item == null)
                return;
            if (item.Canvas.Controller.IsModified)
            {
                if (!item.Header.EndsWith(" *"))
                    item.Header = item.Header + " *";
            }
            else
            {
                if (item.Header.EndsWith(" *"))
                    item.Header = item.Header.Substring(0, item.Header.Length - 2);
            }

            UpdateVisual(item);
        }

        internal void UpdateTabText(uint identifier, string filePath)
        {
            GraphTabItem item = FindGraphTabItem(identifier);
            if (item == null)
                return;
            item.Header = (new FileInfo(filePath)).Name;

            UpdateVisual(item);
        }

        internal GraphTabItem FindGraphTabItem(uint identifier)
        {
            foreach (GraphTabItem item in tabs)
            {
                if (item.Canvas != null && item.Canvas.Controller.Identifier == identifier)
                    return item;
            }
            return null;
        }

        internal void CopyTabPath(object tabItem)
        {
            Clipboard.SetText(((GraphTabItem)tabItem).Canvas.Controller.FilePath);
        }

        internal void OpenTabFolder(object tabItem)
        {
            string tabPath = ((GraphTabItem)tabItem).Canvas.Controller.FilePath;
            int i = tabPath.LastIndexOf('\\');
            tabPath = tabPath.Substring(0, i + 1);
            System.Diagnostics.Process prc = new System.Diagnostics.Process();
            prc.StartInfo.FileName = tabPath;
            prc.Start();
        }

        internal int FindTab(string filePath)
        {
            foreach (GraphTabItem item in tabs)
            {
                if (item.Canvas.Controller.FilePath == filePath)
                    return tabs.IndexOf(item);
            }
            return -1;
        }

        #endregion

        #region private event handlers

        private void OnGraphTabControlLoaded(object sender, RoutedEventArgs e)
        {
            tabPanel = FindVisualChild<TabPanel>(TabControl);
            scrollViewer = FindVisualChild<ScrollViewer>(TabControl);
        }

        private void OnTabItemMouseDown(object sender, MouseEventArgs e)
        {
            // disabled the default select tab behavior
            // once the user did the mouse up, then select the tab
            mouseDown = true;

            e.Handled = true;
        }

        private void OnTabItemRightClick(object sender, RoutedEventArgs e)
        {
            GraphTabItem item = ((TabItem)e.Source).Header as GraphTabItem;
            TabControl.SelectedIndex = tabs.IndexOf(item);
        }

        private void OnTabControlMouseMove(object sender, MouseEventArgs e)
        {
            if (draggedTab == null && mouseDown && !IsRenameTab)
                BeginDrag();
            if (draggedTab == null)
                return;

            double currentMouseX = Mouse.GetPosition(tabPanel).X;

            //move the current tab
            Matrix transformMatrix = draggedTab.RenderTransform.Value;
            transformMatrix.OffsetX = currentMouseX - draggedTabDeltaX;
            MatrixTransform mT = new MatrixTransform(transformMatrix);
            draggedTab.RenderTransform = mT;

            //find the affected tabs and move them
            if (transformMatrix.OffsetX < 0)  // find the right most one and move
            {
                ShiftTab(-1, transformMatrix.OffsetX);
            }
            else if (transformMatrix.OffsetX > 0) // find the left most one and move
            {
                ShiftTab(1, transformMatrix.OffsetX);
            }
        }

        private void OnTabControlMouseUp(object sender, MouseEventArgs e)
        {
            if (draggedTab == null) //select tab
            {
                double currentMouseX = Mouse.GetPosition(tabPanel).X;
                TabItem tabItem = FindTabItemOn(currentMouseX);
                if (tabItem != null)
                {
                    tabItem.BringIntoView();
                    //ScrollViewer scrollViewer = FindVisualChild<ScrollViewer>(TabControl);
                    GraphVisualHost.sliderScrollHandled = true;
                    TabControl.SelectedIndex = tabs.IndexOf(tabItem.Header as GraphTabItem);
                    TabControl.UpdateLayout();
                }
            }
            else
            {
                EndDrag();
            }
            mouseDown = false;
        }

        private void OnTabControlMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            double currentMouseX = Mouse.GetPosition(tabPanel).X;
            TabItem tabItem = FindTabItemOn(currentMouseX);
            if (tabItem != null)
            {
                BeginRename(tabItem);
            }
        }

        private void OnTabClose(object sender, RoutedEventArgs e)
        {
            TabItem tabItem = ((Button)sender).TemplatedParent as TabItem;
            if (tabItem != null)
            {
                CloseTab(tabItem);
            }
            mouseDown = false;
        }

        private void OnAllTabMenuItemMouseUp(object sender, MouseButtonEventArgs e)
        {
            GraphTabItem item = ((MenuItem)((Border)sender).TemplatedParent).Header as GraphTabItem;
            if (item != null)
            {
                TabControl.SelectedIndex = tabs.IndexOf(item);
            }
        }

        private void OnAllTabCloseTabMouseUp(object sender, MouseButtonEventArgs e)
        {
            GraphTabItem item = ((MenuItem)((Button)sender).TemplatedParent).Header as GraphTabItem;
            if (item != null)
            {
                CloseTab(FindTabItem(item));
            }
        }

        private void OnAllTabSaveTabMouseUp(object sender, MouseButtonEventArgs e)
        {
            GraphTabItem item = ((MenuItem)((Button)sender).TemplatedParent).Header as GraphTabItem;
            if (item != null)
            {
                item.Canvas.VisualHost.HandleShortcutSave();
            }
        }

        private void OnRenameTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            TextBox renameTextBox = sender as TextBox;
            EndRename(renameTextBox);
            e.Handled = true;
        }

        private void OnRenameTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Escape)
            {
                TextBox renameTextBox = sender as TextBox;
                if (e.Key == Key.Escape)
                    renameTextBox.Clear();
                EndRename(renameTextBox);
                e.Handled = true;
            }
        }

        // once the menu is open, update the visual one more time to ensure the item is properly updated
        private void OnMenuHeaderMouseDown(object sender, MouseButtonEventArgs e)
        {
            //foreach (GraphTabItem item in tabs)
            //    UpdateVisual(item);
        }

        #endregion

        #region private helper class

        private bool CloseTabCanvas(GraphTabItem item)
        {
            if (item.Canvas.Controller.IsModified)
            {
                TabControl.SelectedIndex = tabs.IndexOf(item);

                string tabName = item.Header;
                if (tabName.EndsWith(" *"))
                    tabName = tabName.Substring(0, tabName.Length - 2);

                MessageBoxResult saveReminderResult
                    = MessageBox.Show("Save changes to the document \"" + tabName + "\" before closing? ",
                                      "DesignScript Studio", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (saveReminderResult == MessageBoxResult.Yes)
                {
                    if (!item.Canvas.VisualHost.HandleShortcutSave())
                        return false;
                }
                else if (saveReminderResult == MessageBoxResult.Cancel)
                {
                    return false;
                }
            }

            item.Canvas.Controller.CleanUp();
            graphControl.RemoveCanvas(item.Canvas.Controller.Identifier);
            item.Canvas = null;
            return true;
        }

        private void BeginRename(TabItem tabItem)
        {
            IsRenameTab = true;

            ContentPresenter header = FindVisualChild(tabItem, "ContentSite") as ContentPresenter;
            header.Visibility = Visibility.Collapsed;
            TextBox renameTextBox = FindVisualChild(tabItem, "RenameTextBox") as TextBox;
            renameTextBox.Text = ((GraphTabItem)header.Content).Header;
            renameTextBox.Visibility = Visibility.Visible;
            renameTextBox.Focus();
            renameTextBox.SelectAll();
        }

        private void EndRename(TextBox renameTextBox)
        {
            IsRenameTab = false;

            TabItem tabItem = (renameTextBox).TemplatedParent as TabItem;
            ContentPresenter header = FindVisualChild(tabItem, "ContentSite") as ContentPresenter;
            GraphTabItem item;
            if (tabItem == null)
                return;

            if (!string.IsNullOrEmpty(renameTextBox.Text))
            {
                string text = renameTextBox.Text;
                if (text.EndsWith("*"))
                    text = text.Substring(0, text.Length - 1);
                text = text.Trim();

                if (text.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) > -1)
                {
                    new PopupDialog(ResourceNames.Save, UiStrings.IllegalFileName, string.Empty, string.Empty, "Ok");

                    renameTextBox.Focus();
                    renameTextBox.SelectAll();
                    IsRenameTab = true;
                    return;
                }
                else
                {
                    item = tabItem.Header as GraphTabItem;
                    item.Header = text;

                    if (!string.IsNullOrEmpty(item.Canvas.Controller.FilePath))
                    {
                        if (!text.EndsWith(".bin"))
                            item.Header = text + ".bin";

                        string oldFilePath = item.Canvas.Controller.FilePath;
                        string newFilePath = oldFilePath.Substring(0, oldFilePath.LastIndexOf("\\") + 1);
                        newFilePath += item.Header;

                        item.Canvas.Controller.DoSaveGraph(newFilePath);
                        if (oldFilePath.ToLower() != newFilePath.ToLower())
                            System.IO.File.Delete(oldFilePath);
                    }
                    else  //add "*" back if it's a unsaved file
                    {
                        item.Header = item.Header + " *";
                    }

                    TextBlock headerText = FindVisualChild(header, "Header") as TextBlock;
                    headerText.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();  // to make sure the target bind to source properly
                }
            }

            header.Visibility = Visibility.Visible;
            renameTextBox.Visibility = Visibility.Collapsed;
        }

        private void BeginDrag()
        {
            double lastMouseX = Mouse.GetPosition(tabPanel).X;
            TabControl.CaptureMouse();
            draggedTab = FindTabItemOn(lastMouseX);
            selectedItem = tabs[TabControl.SelectedIndex];
            if (draggedTab != null)
            {
                draggedTabDeltaX = lastMouseX - draggedTab.RenderTransform.Value.OffsetX;
                if (!draggedTab.IsSelected)
                {                                                  // if draggedTab is not the selected one,
                    draggedTab.SetValue(Panel.ZIndexProperty, 50); // rearrange it to the top of unselected tab
                    TabControl.UpdateLayout();
                }
            }
        }

        private void EndDrag()
        {
            TabControl.SelectedIndex = tabs.IndexOf(draggedTab.Header as GraphTabItem);
            TabControl.UpdateLayout();

            TabControl.ReleaseMouseCapture();
            AnimationHelper(draggedTab, draggedTab.RenderTransform.Value.OffsetX);
            draggedTab.BringIntoView();
            draggedTab = null;
        }

        private void ShiftTab(int direction, double offsetX)
        {
            int i = tabs.IndexOf((GraphTabItem)draggedTab.Header) + direction;

            if (i < 0 || i >= tabs.Count())
                return;
            if (animationOn)
                return;

            TabItem tabToMove = FindTabItem(tabs[i]);

            if (tabToMove.ActualWidth / 2 + tabToMove.RenderTransform.Value.OffsetX < Math.Abs(offsetX))
            {
                GraphTabItem targetItem = tabToMove.Header as GraphTabItem;
                GraphTabItem sourceItem = draggedTab.Header as GraphTabItem;

                int targetIndex = tabs.IndexOf(targetItem);
                int sourceIndex = tabs.IndexOf(sourceItem);

                tabs[sourceIndex] = targetItem;
                tabs[targetIndex] = sourceItem;

                TabControl.UpdateLayout();
                TabControl.SelectedIndex = tabs.IndexOf(selectedItem);

                TabItem targetTabItem = FindTabItem(targetItem);  // after the update, the tabToMove and draggedTab
                TabItem sourceTabItem = FindTabItem(sourceItem);  // are changed

                targetTabItem.RenderTransform = new MatrixTransform(Matrix.Identity);
                sourceTabItem.RenderTransform = new MatrixTransform(Matrix.Identity);

                // translate the offset of the dragged tab
                Matrix transformMatrix = sourceTabItem.RenderTransform.Value;
                transformMatrix.OffsetX = -(targetTabItem.ActualWidth - Math.Abs(offsetX)) * direction;
                sourceTabItem.RenderTransform = new MatrixTransform(transformMatrix);
                if (sourceTabItem.IsSelected)
                    sourceTabItem.SetValue(Panel.ZIndexProperty, 100);
                else
                    sourceTabItem.SetValue(Panel.ZIndexProperty, 50);

                // for the animation of the target tab
                transformMatrix = targetTabItem.RenderTransform.Value;
                transformMatrix.OffsetX = targetTabItem.ActualWidth * direction;
                targetTabItem.RenderTransform = new MatrixTransform(transformMatrix);
                if (targetTabItem.IsSelected)
                    targetTabItem.SetValue(Panel.ZIndexProperty, 100);
                else
                    targetTabItem.SetValue(Panel.ZIndexProperty, 20);
                AnimationHelper(targetTabItem, targetTabItem.ActualWidth * direction);

                draggedTab = sourceTabItem;
                draggedTabDeltaX += (targetTabItem.ActualWidth) * direction;
            }
        }

        private void UpdateVisual(GraphTabItem item)
        {
            // force the tab item to update the text
            TabItem tabItem = FindTabItem(item);
            TextBlock content = FindVisualChild(tabItem, "Header") as TextBlock;
            BindingExpression expression = content.GetBindingExpression(TextBlock.TextProperty);
            expression.UpdateTarget();

            // update the menu
            MenuItem menuItem = FindMenuItem(item);
            if (menuItem != null)
            {
                content = FindVisualChild(menuItem, "Header") as TextBlock;
                if (content != null)
                {
                    expression = content.GetBindingExpression(TextBlock.TextProperty);
                    expression.UpdateTarget();
                }
                Button saveButton = FindVisualChild(menuItem, "SaveTab") as Button;
                if (saveButton != null)
                {
                    if (item.Canvas.Controller.IsModified)
                        saveButton.Visibility = Visibility.Visible;
                    else
                        saveButton.Visibility = Visibility.Hidden;
                }
            }
        }

        private MenuItem FindMenuItem(GraphTabItem key)
        {
            Popup popup = FindVisualChild<Popup>(AllTabsMenu);
            StackPanel stackPanel = (StackPanel)((Decorator)(popup.Child)).Child;

            if (stackPanel != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(stackPanel); i++)
                {
                    MenuItem child = VisualTreeHelper.GetChild(stackPanel, i) as MenuItem;

                    if (child.Header == key)
                        return child;
                }
            }
            return null;
        }

        private DependencyObject FindVisualChild(DependencyObject obj, string name)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && ((FrameworkElement)child).Name == name)
                    return child;
                else
                {
                    var childOfChild = FindVisualChild(child, name);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        private childItem FindVisualChild<childItem>(DependencyObject obj, GraphTabItem item)
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

        private TabItem FindTabItemOn(double positionX)
        {
            double length = 0;

            for (int i = 0; i < tabs.Count; i++)
            {
                TabItem tabItem = FindTabItem(tabs[i]);
                length += tabItem.ActualWidth;

                if (length + tabItem.RenderTransform.Value.OffsetX > positionX)
                    return tabItem;
            }
            return null;
        }

        private TabItem FindTabItem(GraphTabItem key)
        {
            if (tabPanel != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(tabPanel); i++)
                {
                    TabItem child = VisualTreeHelper.GetChild(tabPanel, i) as TabItem;

                    if (child.Header == key)
                        return child;
                }
            }
            return null;
        }

        private void AnimationHelper(TabItem tabItem, double offset)
        {
            animationOn = true;

            Storyboard storyBoard = new Storyboard();

            TransformGroup group = new TransformGroup();
            TranslateTransform transfrom = new TranslateTransform();
            transfrom.X = offset;

            group.Children.Add(transfrom);
            tabItem.RenderTransform = group;

            DoubleAnimation offsetAnimation = new DoubleAnimation();
            offsetAnimation.From = offset;
            offsetAnimation.To = 0;
            offsetAnimation.Duration = new Duration(TimeSpan.FromSeconds(Configurations.ShortAnimationTime));
            offsetAnimation.Completed += OnOffsetAnimationCompleted;

            storyBoard.Children.Add(offsetAnimation);

            Storyboard.SetTarget(offsetAnimation, tabItem);
            Storyboard.SetTargetProperty(offsetAnimation, new PropertyPath("RenderTransform.Children[0].X"));

            storyBoard.Begin();
        }

        void OnOffsetAnimationCompleted(object sender, EventArgs e)
        {
            animationOn = false;
        }

        #endregion
    }

    public sealed class SaveIconConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DesignScriptStudio.Graph.Ui.GraphTabControl.GraphTabItem item = (DesignScriptStudio.Graph.Ui.GraphTabControl.GraphTabItem)value;

            if (item.Canvas.Controller.IsModified)
                return Visibility.Visible;
            else
                return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("TreeLevelConvertor can only be used for one way conversion.");
        }
    }
}
