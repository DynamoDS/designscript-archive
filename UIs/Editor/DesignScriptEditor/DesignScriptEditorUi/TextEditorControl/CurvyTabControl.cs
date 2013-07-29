using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Globalization;
using System.Diagnostics;
using DesignScript.Editor.Core;

namespace DesignScript.Editor
{
    public class TabChangedEventArgs : EventArgs
    {
        public TabChangedEventArgs(int oldIndex, int newIndex)
        {
            this.OldTabIndex = oldIndex;
            this.NewTabIndex = newIndex;
        }

        public int OldTabIndex { get; private set; }
        public int NewTabIndex { get; private set; }
    }

    public delegate string GetToolTipCallback(CurvyTabControl tabControl, int tabIndex);
    public delegate bool TabClosingCallback();
    public delegate void TabChangeHandler(object sender, TabChangedEventArgs e);
    public delegate void TabCloseHandler(object sender, TabChangedEventArgs e);

    public class CurvyTabControl : FrameworkElement
    {
        private CurvyTabVisual tabVisual = null;
        private VisualCollection childVisuals = null;
        private GetToolTipCallback getToolTipCallback = null;
        private TabClosingCallback confirmCloseTab = null;

        private int currentMouseOverIndex = -1;
        private ToolTip toolTip = null;

        public event TabChangeHandler TabChanged = null;
        public event TabCloseHandler TabClosed = null;

        #region Public Class Operational Methods

        public CurvyTabControl()
        {
            tabVisual = new CurvyTabVisual(this);
            childVisuals = new VisualCollection(this);
            childVisuals.Add(tabVisual.Render());

            this.LayoutUpdated += new EventHandler(CurvyTabLayoutUpdated);
            this.MouseMove += new MouseEventHandler(CurvyTabMouseMove);
            this.MouseLeave += new MouseEventHandler(CurvyTabMouseLeave);
            this.MouseLeftButtonDown += new MouseButtonEventHandler(CurvyTabMouseLeftButtonDown);
            this.MouseLeftButtonUp += new MouseButtonEventHandler(CurvyTabMouseLeftButtonUp);
            this.MouseRightButtonDown += new MouseButtonEventHandler(CurvyTabMouseRightButtonDown);
            this.MouseRightButtonUp += new MouseButtonEventHandler(CurvyTabMouseRightButtonUp);
            this.MouseDown += new MouseButtonEventHandler(CurvyTabMouseDown);
        }

        internal bool ActivateTab(int index)
        {
            return tabVisual.ActivateTab(index);
        }

        internal void SetHighlightTab(int index, bool value)
        {
            tabVisual.SetHighlightTab(index, value);
        }

        internal int InsertNewTab(string displayText, UIElement visual)
        {
            int insertedIndex = tabVisual.InsertNewTab(displayText, visual);
            LayoutUpdatedInternal();

            // Show the tab control if it has not already been shown.
            if (this.Visibility != System.Windows.Visibility.Visible)
                this.Visibility = System.Windows.Visibility.Visible;

            return insertedIndex;
        }

        internal bool SetDisplayText(int index, string displayText)
        {
            return tabVisual.SetDisplayText(index, displayText);
        }

        internal string GetDisplayText(int index)
        {
            return tabVisual.GetDisplayText(index);
        }

        internal bool CloseTab(int index)
        {
            return tabVisual.CloseTab(index);
        }

        internal void CloseAllTabs()
        {
            tabVisual.CloseAllTabs();
        }

        internal void CloseCurrentTab()
        {
            CloseTabInternal();
        }

        internal bool HandleContextMenu(string menuItemName)
        {
            switch (menuItemName)
            {
                case "CloseTab":
                case "CloseTabItem":
                    return CloseTabInternal();
                case "CloseAllTabs":
                case "CloseAllItem":
                    return CloseAllTabsInternal();
                case "CloseAllToTheRight":
                case "CloseAllToTheRightItem":
                    return CloseTabsToRightInternal();
                case "CloseAllButThis":
                case "CloseAllButThisItem":
                    return CloseAllButThisInternal();
            }

            return false; // Not handled.
        }

        internal void RegisterToolTipCallback(GetToolTipCallback callback)
        {
            this.getToolTipCallback = callback;
        }

        internal void RegisterTabClosingCallback(TabClosingCallback confirmClose)
        {
            this.confirmCloseTab = confirmClose;
        }

        #endregion

        #region Public Class Properties

        internal bool ShowCloseButton { get; set; }
        internal int CurrentTab { get { return tabVisual.CurrentTab; } }
        internal int TabCount { get { return tabVisual.TabCount; } }

        #endregion

        #region Protected Class Overridable Methods

        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            // The message processed, don't proceed here (that 
            // way the contextual menu will not be shown).
            if (this.tabVisual.ProcessContextMenuOpening(e))
                return;

            // Proceed to show contextual menu.
            base.OnContextMenuOpening(e);
        }

        #endregion

        #region Private Class Event Handlers

        void CurvyTabMouseMove(object sender, MouseEventArgs e)
        {
            if (toolTip == null)
                toolTip = new ToolTip();

            int tabIndex = tabVisual.ProcessMouseMove(e.GetPosition(sender as IInputElement).X);

            if (tabIndex == -1)
                toolTip.IsOpen = false;
            else
            {
                currentMouseOverIndex = tabIndex;
                string toolTipText = string.Empty;
                if (null != getToolTipCallback)
                    toolTipText = getToolTipCallback(this, tabIndex);

                // If the owner does not provide one, get it from the tab itself.
                if (string.IsNullOrEmpty(toolTipText))
                    toolTipText = tabVisual.GetDisplayText(tabIndex);

                toolTip.Content = toolTipText;
                toolTip.IsOpen = (string.IsNullOrEmpty(toolTipText) ? false : true);
            }
        }

        void CurvyTabMouseLeave(object sender, MouseEventArgs e)
        {
            if (null != toolTip)
                toolTip.IsOpen = false;

            tabVisual.ProcessMouseMove(-1);
        }

        void CurvyTabMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            int oldIndex = tabVisual.CurrentTab; // The current tab index.

            Point mousePosition = e.GetPosition(sender as IInputElement);
            tabVisual.ProcessMouseLeftButtonDown(mousePosition.X);

            // There was a tab switch if we got here.
            if (null != TabChanged)
            {
                int newIndex = tabVisual.CurrentTab;
                TabChanged(this, new TabChangedEventArgs(oldIndex, newIndex));
            }

            TextEditorControl.Instance.UpdateTabOptions();
        }

        void CurvyTabMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            bool overCloseButton = false;

            Point mousePosition = e.GetPosition(sender as IInputElement);
            if (tabVisual.ProcessMouseLeftButtonUp(mousePosition.X, ref overCloseButton))
            {
                if (false != overCloseButton)
                {
                    CloseTabInternal();
                    tabVisual.Render();
                }
            }
        }

        void CurvyTabMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        void CurvyTabMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                int oldIndex = tabVisual.CurrentTab; // The current tab index.

                Point mousePosition = e.GetPosition(sender as IInputElement);
                if (tabVisual.ProcessMouseLeftButtonDown(mousePosition.X))
                {
                    // There was a tab switch if we got here.
                    if (null != TabChanged)
                    {
                        int newIndex = tabVisual.CurrentTab;
                        TabChanged(this, new TabChangedEventArgs(oldIndex, newIndex));
                    }
                }
                CloseTabInternal();
            }
        }

        void CurvyTabMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            int oldIndex = tabVisual.CurrentTab; // The current tab index.

            Point mousePosition = e.GetPosition(sender as IInputElement);
            if (tabVisual.ProcessMouseRightButtonDown(mousePosition.X))
            {
                // There was a tab switch if we got here.
                if (null != TabChanged)
                {
                    int newIndex = tabVisual.CurrentTab;
                    TabChanged(this, new TabChangedEventArgs(oldIndex, newIndex));
                }

                TextEditorControl.Instance.UpdateTabOptions();
            }
        }

        void CurvyTabLayoutUpdated(object sender, EventArgs e)
        {
            LayoutUpdatedInternal();
        }

        #endregion

        #region Private Class Helper Methods

        private bool CloseTabInternal()
        {
            if (null != confirmCloseTab)
            {
                if (confirmCloseTab() == false)
                    return false;
            }

            int oldIndex = tabVisual.CurrentTab; // The current tab index.
            if (tabVisual.CloseTab(tabVisual.CurrentTab))
            {
                int newIndex = tabVisual.CurrentTab;
                if (null != TabClosed)
                    TabClosed(this, new TabChangedEventArgs(oldIndex, newIndex));

                LayoutUpdatedInternal();
            }

            TextEditorControl.Instance.UpdateTabOptions();

            return true;
        }

        private bool CloseAllTabsInternal()
        {
            while (TabCount > 0)
            {
                if (null != confirmCloseTab)
                {
                    if (confirmCloseTab() == false)
                        return false;
                }

                int oldIndex = tabVisual.CurrentTab; // The current tab index.
                if (tabVisual.CloseTab(tabVisual.CurrentTab))
                {
                    int newIndex = tabVisual.CurrentTab;
                    if (null != TabClosed)
                        TabClosed(this, new TabChangedEventArgs(oldIndex, newIndex));

                    LayoutUpdatedInternal();
                }
            }

            TextEditorControl.Instance.UpdateTabOptions();

            return true;
        }

        private bool CloseTabsToRightInternal()
        {
            if (tabVisual.CurrentTab == tabVisual.TabCount - 1)
                return true;

            TextEditorControl.Instance.TextCore.ChangeScript(tabVisual.CurrentTab + 1);
            ActivateTab(tabVisual.CurrentTab + 1);

            int currentTab = tabVisual.CurrentTab;
            int totalTabs = tabVisual.TabCount;

            while (TabCount > currentTab)
            {
                if (null != confirmCloseTab)
                {
                    if (confirmCloseTab() == false)
                        return false;
                }

                int oldIndex = tabVisual.CurrentTab; // The current tab index.
                if (tabVisual.CloseTab(tabVisual.CurrentTab))
                {
                    int newIndex = tabVisual.CurrentTab;
                    if (null != TabClosed)
                        TabClosed(this, new TabChangedEventArgs(oldIndex, newIndex));

                    LayoutUpdatedInternal();
                }
            }

            TextEditorControl.Instance.UpdateTabOptions();

            return true;
        }

        private bool CloseAllButThisInternal()
        {

            int currentTab = tabVisual.CurrentTab;
            if (tabVisual.TabCount == 0)
                return true;
            TextEditorControl.Instance.TextCore.ChangeScript(tabVisual.CurrentTab + 1);
            ActivateTab(tabVisual.CurrentTab + 1);
            int totalTabs = tabVisual.TabCount;

            while (TabCount > 1)
            {
                if (TabCount == currentTab + 1)
                {
                    TextEditorControl.Instance.TextCore.ChangeScript(0);
                    ActivateTab(0);
                    currentTab = -1;
                    continue;
                }

                if (null != confirmCloseTab)
                {
                    if (confirmCloseTab() == false)
                        return false;
                }

                int oldIndex = tabVisual.CurrentTab; // The current tab index.
                if (tabVisual.CloseTab(tabVisual.CurrentTab))
                {
                    int newIndex = tabVisual.CurrentTab;
                    if (null != TabClosed)
                        TabClosed(this, new TabChangedEventArgs(oldIndex, newIndex));

                    LayoutUpdatedInternal();
                }
            }

            TextEditorControl.Instance.UpdateTabOptions();

            return true;
        }

        private void LayoutUpdatedInternal()
        {
            double totalWidth = (this.ActualWidth - (2 * tabVisual.GapBetweenTabs));
            if (totalWidth > 0)
            {
                double tabWidth = totalWidth / tabVisual.TabCount;
                tabWidth -= tabVisual.GapBetweenTabs * 1.5;
                tabVisual.ResizeTabs(Math.Floor(tabWidth));
            }
        }

        #endregion

        #region Protected Class Overridable Methods

        protected override int VisualChildrenCount
        {
            get { return childVisuals.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return childVisuals[index];
        }

        #endregion
    }

    internal class CurvyTab
    {
        internal enum TabState : uint
        {
            None = 0x0000, Active = 0x0001, MouseOver = 0x0002, CloseButton = 0x0004
        }

        string displayText = string.Empty;
        string fullScriptPath = string.Empty;
        bool highlightTab = false;

        PathGeometry tabOuterPath = null;
        PathGeometry tabInnerPath = null;
        PathGeometry tabCloseButton = null;
        CurvyTabVisual parentVisual = null;
        FormattedText formattedText = null;
        TabState tabState = TabState.None;

        #region Public Class Operational Methods

        internal CurvyTab(CurvyTabVisual parentVisual, string displayText, UIElement visual)
        {
            this.Visual = visual;
            this.displayText = displayText;
            this.parentVisual = parentVisual;

            tabOuterPath = new PathGeometry();
            tabOuterPath.Transform = new TranslateTransform(0, 0);
            tabInnerPath = new PathGeometry();
            tabInnerPath.Transform = new TranslateTransform(0, 0);

            ResizeTab();
        }

        internal void ResizeTab()
        {
            double tabWidth = this.parentVisual.TabWidth;
            tabOuterPath.Figures = CreateOuterPathFigures(tabWidth);
            tabInnerPath.Figures = CreateInnerPathFigures(tabWidth);
            RefreshFormattedText();
        }

        internal void Render(DrawingContext context, GradientBrush tabBackground, double offset)
        {
            // Do the horizontal offset first.
            (tabOuterPath.Transform as TranslateTransform).X = offset;
            (tabInnerPath.Transform as TranslateTransform).X = offset;

            Brush outerBrush = new SolidColorBrush(UIColors.CurvyTabOuterColor);
            context.DrawGeometry(Brushes.White, new Pen(outerBrush, 1), tabOuterPath);

            Brush innerBrush = new SolidColorBrush(UIColors.CurvyTabInnerColor);
            context.DrawGeometry(tabBackground, new Pen(innerBrush, 1), tabInnerPath);

            if (null == formattedText)
                RefreshFormattedText();

            // Finally, draw the display text on the tab.
            context.DrawText(formattedText, new Point(offset + (parentVisual.GapBetweenTabs + 8), 6));

            if (false != parentVisual.ShowCloseButton)
            {
                if (null == tabCloseButton)
                {
                    tabCloseButton = new PathGeometry();
                    tabCloseButton.Transform = new TranslateTransform(0, 0);
                    tabCloseButton.Figures = CreateButtonPathFigures();
                }

                Brush closeBrush = this.OverCloseButton ? Brushes.Black : Brushes.Gray;
                double closeOffset = parentVisual.TabWidth + parentVisual.GapBetweenTabs + 2;
                (tabCloseButton.Transform as TranslateTransform).X = offset + closeOffset;
                context.DrawGeometry(Brushes.Black, new Pen(closeBrush, 2), tabCloseButton);
            }
        }

        #endregion

        #region Public Class Properties

        internal UIElement Visual { get; private set; }

        internal bool Active
        {
            get { return ((this.tabState & TabState.Active) != 0); }
            set
            {
                this.tabState &= ~TabState.Active;
                if (false != value)
                    this.tabState |= TabState.Active;
            }
        }

        internal bool HighlightTab
        {
            get { return highlightTab; }
            set { highlightTab = value; }
        }

        internal bool MouseOver
        {
            get { return ((this.tabState & TabState.MouseOver) != 0); }
            set
            {
                this.tabState &= ~TabState.MouseOver;
                if (false != value)
                    this.tabState |= TabState.MouseOver;
            }
        }

        internal bool OverCloseButton
        {
            get { return ((this.tabState & TabState.CloseButton) != 0); }
            set
            {
                this.tabState &= ~TabState.CloseButton;
                if (false != value)
                    this.tabState |= TabState.CloseButton;
            }
        }

        internal string DisplayText
        {
            get { return displayText; }
            set
            {
                displayText = value;
                RefreshFormattedText();
            }
        }

        internal string FullScriptPath
        {
            get { return fullScriptPath; }
            set
            {
                fullScriptPath = value;
            }
        }

        #endregion

        #region Private Class Helper Methods

        private static PathFigureCollection CreateInnerPathFigures(double tabWidth)
        {
            int length = ((int)tabWidth);
            string innerPathFormat = "m 0,27 c 0,0 1,0 5,0 4,0 8,-12 8,-12 0,0 3,-11 7,-11 l {0},0 c 4,0 7,11 7,11 0,0 4,12 8,12 4,0 5,0 5,0 z";
            return PathFigureCollection.Parse(string.Format(innerPathFormat, length));
        }

        private static PathFigureCollection CreateOuterPathFigures(double tabWidth)
        {
            int length = ((int)tabWidth);
            string outerPathFormat = "m 0,27 c 0,0 0,0 4,0 4,0 8,-12 8,-12 0,0 4,-12 8,-12 l {0},0 c 4,0 8,12 8,12 0,0 4,12 8,12 4,0 4,0 4,0 z";
            return PathFigureCollection.Parse(string.Format(outerPathFormat, length));
        }

        private static PathFigureCollection CreateButtonPathFigures()
        {
            return PathFigureCollection.Parse("m 0,12 -8,8 m 0,-8 8,8");
        }

        private void RefreshFormattedText()
        {
            if (this.HighlightTab == false)
                this.formattedText = new FormattedText(displayText,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    this.parentVisual.NormalFontTypeface,
                    Configurations.CurvyTabFontHeight, Brushes.Black);
            else
            {
                this.HighlightTab = true;
                this.formattedText = new FormattedText(displayText,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    this.parentVisual.HighlightFontTypeface,
                    Configurations.CurvyTabFontHeight, Brushes.Black);
            }
            this.formattedText.MaxTextWidth = parentVisual.TabWidth - parentVisual.GapBetweenTabs - 4;
            this.formattedText.MaxTextHeight = Configurations.CurvyTabFontHeight + 4;
        }

        #endregion
    }

    internal class CurvyTabVisual : Visual
    {
        Pen borderLine = null;
        DrawingVisual drawingVisual = null;
        GradientBrush inactiveBrush = null;
        GradientBrush activeBrush = null;
        FrameworkElement parentVisual = null;
        Typeface normalFontTypeface = null, highlightFontTypeface = null;

        int mouseOverIndex = -1;
        int activeTabIndex = -1;
        List<CurvyTab> curvyTabs = new List<CurvyTab>();

        #region Public Class Operational Methods

        internal CurvyTabVisual(FrameworkElement parentVisual)
        {
            this.parentVisual = parentVisual;
            this.TabWidth = 176.0;

            GradientStopCollection inactiveStops = new GradientStopCollection();
            inactiveStops.Add(new GradientStop(Color.FromRgb(210, 222, 234), 0.0));
            inactiveStops.Add(new GradientStop(Color.FromRgb(190, 201, 214), 1.0));
            inactiveBrush = new LinearGradientBrush(inactiveStops, 90);

            GradientStopCollection activeStops = new GradientStopCollection();
            activeStops.Add(new GradientStop(Color.FromRgb(255, 255, 255), 0.0));
            activeStops.Add(new GradientStop(Color.FromRgb(224, 224, 224), 1.0));
            activeBrush = new LinearGradientBrush(activeStops, 90);

            // GradientStopCollection backgroundStops = new GradientStopCollection();
            // backgroundStops.Add(new GradientStop(Color.FromRgb(127, 127, 127), 0.0));
            // backgroundStops.Add(new GradientStop(Color.FromRgb(160, 160, 160), 0.2));
            // backgroundStops.Add(new GradientStop(Color.FromRgb(224, 224, 224), 1.0));
            // backgroundBrush = new LinearGradientBrush(backgroundStops, 90);

            borderLine = new Pen(new SolidColorBrush(Color.FromRgb(224, 224, 224)), 2);
        }

        internal void ResizeTabs(double width)
        {
            if (width <= 0)
            {
                this.Render();
                return;
            }

            width = ((width < 176.0) ? width : 176.0);
            this.TabWidth = ((width >= 32.0) ? width : 32.0);
            foreach (CurvyTab tab in curvyTabs)
                tab.ResizeTab();

            this.Render();
        }

        internal bool ActivateTab(int index)
        {
            if (IsValidTabIndex(index) == false)
                return false; // Invalid tab index.
            if (index == activeTabIndex)
                return false; // Nothing changed.

            UIElement visualToHide = null;
            UIElement visualToShow = null;

            if (IsValidTabIndex(activeTabIndex))
            {
                visualToHide = curvyTabs[activeTabIndex].Visual;
                curvyTabs[activeTabIndex].Active = false;
                curvyTabs[activeTabIndex].OverCloseButton = false;
            }

            activeTabIndex = index;
            visualToShow = curvyTabs[index].Visual;
            curvyTabs[index].Active = true;

            // If there are actual tab switches.
            if (visualToHide != visualToShow)
            {
                if (null != visualToHide)
                    visualToHide.Visibility = Visibility.Collapsed;
                if (null != visualToShow)
                    visualToShow.Visibility = Visibility.Visible;
            }
            else if (null != visualToShow)
                visualToShow.Visibility = Visibility.Visible;

            this.Render(); // Redraw the entire tab control.
            return true;
        }

        internal void SetHighlightTab(int index, bool value)
        {
            if (IsValidTabIndex(index) == false)
                return;
            else
            {
                curvyTabs[index].HighlightTab = value;
            }
        }

        internal bool SetDisplayText(int index, string displayText)
        {
            if (IsValidTabIndex(index) == false)
                return false;
            else
            {
                curvyTabs[index].DisplayText = displayText;
                this.Render();
                return true;
            }
        }

        internal string GetDisplayText(int index)
        {
            if (IsValidTabIndex(index) == false)
                return null;
            else
            {
                return curvyTabs[index].DisplayText;
            }
        }

        internal int InsertNewTab(string displayText, UIElement visual)
        {
            curvyTabs.Add(new CurvyTab(this, displayText, visual));
            ActivateTab(curvyTabs.Count - 1); // Activate the last tab.
            return activeTabIndex;
        }

        internal bool CloseTab(int index)
        {
            if (IsValidTabIndex(index) == false)
                return false; // Invalid tab index.

            curvyTabs.RemoveAt(index);

            int tabToBeActivated = index;
            if (tabToBeActivated >= curvyTabs.Count)
                tabToBeActivated = curvyTabs.Count - 1;

            if (tabToBeActivated == -1)
            {
                this.activeTabIndex = -1;
                this.Render();
                return true;
            }

            ActivateTab(tabToBeActivated);
            return true;
        }

        internal void CloseAllTabs()
        {
            mouseOverIndex = -1;
            activeTabIndex = -1;
            curvyTabs.Clear();
        }

        internal bool ProcessContextMenuOpening(ContextMenuEventArgs e)
        {
            bool overCloseButton = false;
            int tabIndex = TabIndexFromOffset(e.CursorLeft, ref overCloseButton);
            if (-1 == tabIndex)
                e.Handled = true;

            return e.Handled; // The tab visual has handled the message.
        }

        internal int ProcessMouseMove(double x)
        {
            bool overCloseButton = false;
            int tabIndex = TabIndexFromOffset(x, ref overCloseButton);
            if (tabIndex == mouseOverIndex || (curvyTabs.Count == 0))
            {
                if (IsValidTabIndex(tabIndex))
                {
                    if (curvyTabs[tabIndex].OverCloseButton != overCloseButton)
                    {
                        curvyTabs[tabIndex].OverCloseButton = overCloseButton;
                        this.Render();
                    }
                }

                return mouseOverIndex;
            }

            if (IsValidTabIndex(mouseOverIndex))
            {
                curvyTabs[mouseOverIndex].MouseOver = false;
                curvyTabs[mouseOverIndex].OverCloseButton = false;
            }

            mouseOverIndex = tabIndex;
            if (IsValidTabIndex(mouseOverIndex))
            {
                curvyTabs[mouseOverIndex].MouseOver = true;
                curvyTabs[mouseOverIndex].OverCloseButton = overCloseButton;
            }

            this.Render(); // Redraw the entire tab control.
            return mouseOverIndex;
        }

        internal bool ProcessMouseLeftButtonDown(double x)
        {
            bool overCloseButton = false;
            int tabIndex = TabIndexFromOffset(x, ref overCloseButton);
            return this.ActivateTab(tabIndex);
        }

        internal bool ProcessMouseLeftButtonUp(double x, ref bool overCloseButton)
        {
            int tabIndex = TabIndexFromOffset(x, ref overCloseButton);
            if (IsValidTabIndex(tabIndex))
            {
                // There's this problem that goes like this: assuming there are 
                // five open tabs, and the middle one (3rd tab) is inactive. 
                // Click and hold the mouse button on the close button of 3rd 
                // tab brings it to the front, but the tab isn't closed just yet
                // because mouse is not released. After a while of holding down 
                // the mouse button (on close button of 3rd tab), release it. 
                // This closes out 3rd tab as expected, but at times releasing 
                // the mouse button also immediately triggers another mouse 
                // down-up pair, which closes out the 4th tab right after the 
                // 3rd tab being closed out. To fix that, we make sure the 
                // immediate mouse-up event is not processed (the 4th tab would
                // not have its "OverCloseButton" flag being set to true).
                // 
                if (curvyTabs[tabIndex].OverCloseButton == false)
                    overCloseButton = false;
            }

            return (-1 != tabIndex);
        }

        internal bool ProcessMouseRightButtonDown(double x)
        {
            bool overCloseButton = false;
            int tabIndex = TabIndexFromOffset(x, ref overCloseButton);
            return this.ActivateTab(tabIndex);
        }

        internal DrawingVisual Render()
        {
            if (null == drawingVisual)
                drawingVisual = new DrawingVisual();

            DrawingContext context = drawingVisual.RenderOpen();
            if (null != context)
            {
                Rect rect = new Rect(0, 0, parentVisual.ActualWidth, parentVisual.ActualHeight);
                context.PushClip(new RectangleGeometry(rect));

                if (curvyTabs.Count > 0)
                {
                    double offset = (curvyTabs.Count - 1) * (TabWidth + GapBetweenTabs);
                    for (int index = curvyTabs.Count - 1; index >= 0; index--)
                    {
                        if (index == activeTabIndex)
                        {
                            offset = offset - TabWidth - GapBetweenTabs;
                            continue; // Active tab to be rendered last.
                        }

                        CurvyTab tab = curvyTabs[index];
                        if (tab.MouseOver != false)
                            tab.Render(context, activeBrush, offset);
                        else
                            tab.Render(context, inactiveBrush, offset);

                        offset = offset - TabWidth - GapBetweenTabs;
                    }

                    // Render active tab.
                    if (IsValidTabIndex(activeTabIndex))
                    {
                        offset = (activeTabIndex * (TabWidth + GapBetweenTabs));
                        curvyTabs[activeTabIndex].Render(context, activeBrush, offset);
                    }
                }

                context.DrawLine(borderLine, new Point(0, parentVisual.ActualHeight - 1),
                    new Point(parentVisual.ActualWidth, parentVisual.ActualHeight - 1));

                context.Pop();
                context.Close(); // Done with rendering.
            }

            return drawingVisual;
        }

        #endregion

        #region Public Class Properties

        internal int CurrentTab { get { return activeTabIndex; } }
        internal int TabCount { get { return curvyTabs.Count; } }
        internal double TabWidth { get; private set; }
        internal double GapBetweenTabs { get { return 16.0; } }

        internal bool ShowCloseButton
        {
            get
            {
                return (parentVisual as CurvyTabControl).ShowCloseButton;
            }
        }

        internal Typeface NormalFontTypeface
        {
            get
            {
                if (normalFontTypeface == null)
                {
                    normalFontTypeface = new Typeface(SystemFonts.MessageFontFamily,
                        SystemFonts.MessageFontStyle, SystemFonts.MessageFontWeight, new FontStretch());
                }

                return this.normalFontTypeface;
            }
        }

        internal Typeface HighlightFontTypeface
        {
            get
            {
                if (highlightFontTypeface == null)
                {
                    highlightFontTypeface = new Typeface(SystemFonts.MessageFontFamily,
                        SystemFonts.MessageFontStyle, FontWeights.Bold, new FontStretch());
                }

                return this.highlightFontTypeface;
            }
        }

        #endregion

        #region Private Class Helper Methods

        private int TabIndexFromOffset(double offset, ref bool overCloseButton)
        {
            offset = (offset - (GapBetweenTabs * 0.5)) - 2.0;
            double tabWidth = TabWidth + GapBetweenTabs;
            int tabIndex = (int)Math.Floor(offset / tabWidth);

            overCloseButton = false;
            if ((parentVisual as CurvyTabControl).ShowCloseButton)
            {
                double difference = (offset - (tabIndex * tabWidth));
                if ((tabWidth - difference) < GapBetweenTabs + 8.0)
                    overCloseButton = true;
            }

            if (IsValidTabIndex(tabIndex) == false)
                return -1;

            return tabIndex;
        }

        private bool IsValidTabIndex(int index)
        {
            return (index >= 0 && (index < curvyTabs.Count));
        }

        #endregion
    }
}
