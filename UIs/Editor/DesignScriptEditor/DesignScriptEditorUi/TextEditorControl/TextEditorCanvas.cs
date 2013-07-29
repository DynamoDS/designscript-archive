using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using DesignScript.Editor.Core;

namespace DesignScript.Editor
{
    using ProtoCore.CodeModel;

    public class TextEditorScrollViewer : ScrollViewer
    {
        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            // We're doing this so that external 
            // event handlers get to handle this event.
            base.OnMouseLeftButtonDown(e);
            e.Handled = false;
        }

        protected override void OnMouseRightButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseRightButtonDown(e);
            e.Handled = false;
        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            // base.OnKeyDown(e); // We don't want the key to scroll unnecessarily.
        }

        protected override void OnKeyUp(System.Windows.Input.KeyEventArgs e)
        {
            // base.OnKeyUp(e); // We don't want the key to scroll unnecessarily.
        }
    }

    public class TextEditorCanvas : Panel, IScrollInfo
    {
        #region Class Data Members

        TranslateTransform transform = null;
        ScrollViewer scrollOwner = null;
        EditorVisualHost visualHost = null;

        System.Windows.Point contentOffset;
        System.Windows.Size contentExtent;
        System.Windows.Size viewportSize;
        bool canHorizontallyScroll = false;
        bool canVerticallyScroll = false;

        double mouseWheelWidth = 3.0 * Configurations.FormatFontWidth;
        double mouseWheelHeight = 3.0 * Configurations.FontDisplayHeight;

        #endregion

        #region Public Class Properties

        internal int FirstVisibleLine { get; private set; }

        internal int FirstVisibleColumn { get; private set; }

        internal int MaxVisibleLines
        {
            get { return GetMaxVisibleLines(viewportSize.Height); }
        }

        internal int MaxVisibelColumns
        {
            get { return GetMaxVisibleColumns(viewportSize.Width); }
        }

        internal ITextEditorCore TextEditorCore { get; set; }

        #endregion

        #region Public Class Operational Methods

        public TextEditorCanvas()
        {
            transform = new TranslateTransform();
            this.RenderTransform = this.transform;
            this.FirstVisibleLine = 0;
            this.FirstVisibleColumn = 0;

            contentOffset = new System.Windows.Point();
            contentExtent = new System.Windows.Size();
            viewportSize = new System.Windows.Size();
            visualHost = new EditorVisualHost(this);
            this.Children.Add(visualHost);
        }

        internal void SetComputedCanvasDimension(double width, double height)
        {
            // Allowing one additional line/column to be displayed.
            width = width + Configurations.FormatFontWidth;
            height = height + Configurations.FontDisplayHeight;

            if (width != contentExtent.Width || (height != contentExtent.Height))
            {
                // There is a change in content dimension, update layout.
                this.contentExtent.Width = width;
                this.contentExtent.Height = height;
                RefreshSrollBarVisibility();
                InvalidateOwnerScrollInfo();
            }
        }

        internal void EnsureCursorVisible(int column, int line)
        {
            if (this.MaxVisibleLines <= 0)
                return; // Not ready to do so yet.

            int vertPageSize = this.MaxVisibleLines;
            int currentTopLine = (int)Math.Floor(VerticalOffset / Configurations.FontDisplayHeight);

            int newTopLine = currentTopLine;
            if (line < currentTopLine)
                newTopLine = line;
            else if (line >= (currentTopLine + vertPageSize))
                newTopLine = line - vertPageSize + 1;
            else if (contentExtent.Height <= viewportSize.Height)
                newTopLine = 0;

            if (newTopLine != currentTopLine)
                SetVerticalOffset(newTopLine * Configurations.FontDisplayHeight);

            int horzPageSize = GetMaxVisibleColumns(viewportSize.Width);
            int currentLeftColumn = (int)Math.Floor(HorizontalOffset / Configurations.FormatFontWidth);

            // What is visible is "visual", does not mean the actual character offset.
            // For a meaningful comparison, we need to convert from "actual offset" to 
            // "visual offset" before comparing it with "currentLeftColumn" (visual).
            // 
            CharPosition converter = Solution.Current.ActiveScript.CreateCharPosition();
            int visualColumn = converter.CharToVisualOffset(line, column);

            int newLeftColumn = currentLeftColumn;
            if (visualColumn < currentLeftColumn)
                newLeftColumn = visualColumn;
            else if (visualColumn >= (currentLeftColumn + horzPageSize))
                newLeftColumn = visualColumn - horzPageSize + 1;
            else if (contentExtent.Width <= viewportSize.Width)
                newLeftColumn = 0;

            if (newLeftColumn != currentLeftColumn)
                SetHorizontalOffset(newLeftColumn * Configurations.FormatFontWidth);
        }

        internal void PauseCaretTimer(bool pauseTimer)
        {
            this.visualHost.PauseCaretTimer(pauseTimer);
        }

        internal void BreakpointsUpdated()
        {
            this.visualHost.BreakpointsUpdated();
        }

        internal void SetCursorScreenPos(Point point)
        {
            this.visualHost.SetCursorScreenPos(point);
        }

        internal void UpdateVisualForScript(IScriptObject hostScript)
        {
            this.visualHost.UpdateVisualForScript(hostScript);
        }

        internal void SetSelectionRange(System.Drawing.Point start, System.Drawing.Point end)
        {
            this.visualHost.SetSelectionRange(start, end);
        }

        internal void SetExecutionCursor(ProtoCore.CodeModel.CodeRange cursor)
        {
            this.visualHost.SetExecutionCursor(cursor);
        }

        internal void ClearExecutionCursor()
        {
            this.visualHost.ClearExecutionCursor();
        }

        #endregion

        #region Protected Panel Overridable Class Members

        protected override Size MeasureOverride(Size availableSize)
        {
            HandleMeasureArrange(ref availableSize);
            return availableSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            HandleMeasureArrange(ref finalSize);
            return base.ArrangeOverride(finalSize);
        }

        #endregion

        #region IScrollInfo Members

        public bool CanHorizontallyScroll
        {
            get { return canHorizontallyScroll; }
            set { canHorizontallyScroll = value; }
        }

        public bool CanVerticallyScroll
        {
            get { return canVerticallyScroll; }
            set { canVerticallyScroll = value; }
        }

        public double ExtentHeight
        {
            get { return contentExtent.Height; }
        }

        public double ExtentWidth
        {
            get { return contentExtent.Width; }
        }

        public double HorizontalOffset
        {
            get { return contentOffset.X; }
        }

        public void LineDown()
        {
            SetVerticalOffset(this.VerticalOffset + Configurations.FontDisplayHeight);
        }

        public void LineLeft()
        {
            SetHorizontalOffset(this.HorizontalOffset - Configurations.FormatFontWidth);
        }

        public void LineRight()
        {
            SetHorizontalOffset(this.HorizontalOffset + Configurations.FormatFontWidth);
        }

        public void LineUp()
        {
            SetVerticalOffset(this.VerticalOffset - Configurations.FontDisplayHeight);
        }

        public System.Windows.Rect MakeVisible(Visual visual, System.Windows.Rect rectangle)
        {
            return rectangle;
        }

        public void MouseWheelDown()
        {
            if (System.Windows.Forms.Control.ModifierKeys == System.Windows.Forms.Keys.Control)
            {
                Configurations.DecreaseFontSize();
                TextEditorControl.Instance.UpdateCanvasDimension();
                TextEditorControl.Instance.UpdateScriptDisplay(Solution.Current.ActiveScript);
                TextEditorControl.Instance.UpdateCaretPosition();
            }
            else
                SetVerticalOffset(this.VerticalOffset + mouseWheelHeight);
        }

        public void MouseWheelLeft()
        {
            SetHorizontalOffset(this.HorizontalOffset - mouseWheelWidth);
        }

        public void MouseWheelRight()
        {
            SetHorizontalOffset(this.HorizontalOffset + mouseWheelWidth);
        }

        public void MouseWheelUp()
        {
            if (System.Windows.Forms.Control.ModifierKeys == System.Windows.Forms.Keys.Control)
            {
                Configurations.IncreaseFontSize();
                TextEditorControl.Instance.UpdateCanvasDimension();
                TextEditorControl.Instance.UpdateScriptDisplay(Solution.Current.ActiveScript);
                TextEditorControl.Instance.UpdateCaretPosition();
            }
            else
                SetVerticalOffset(this.VerticalOffset - mouseWheelHeight);
        }

        public void PageDown()
        {
            double offset = this.MaxVisibleLines * Configurations.FontDisplayHeight;
            SetVerticalOffset(this.VerticalOffset + offset);
        }

        public void PageLeft()
        {
            int pageSize = GetMaxVisibleColumns(viewportSize.Width);
            double offset = pageSize * Configurations.FormatFontWidth;
            SetHorizontalOffset(this.HorizontalOffset - offset);
        }

        public void PageRight()
        {
            int pageSize = GetMaxVisibleColumns(viewportSize.Width);
            double offset = pageSize * Configurations.FormatFontWidth;
            SetHorizontalOffset(this.HorizontalOffset + offset);
        }

        public void PageUp()
        {
            double offset = this.MaxVisibleLines * Configurations.FontDisplayHeight;
            SetVerticalOffset(this.VerticalOffset - offset);
        }

        public ScrollViewer ScrollOwner
        {
            get { return scrollOwner; }
            set { scrollOwner = value; }
        }

        public void SetHorizontalOffset(double offset)
        {
            // Make sure the requested offset is well within the range.
            if (offset < 0 || viewportSize.Width >= contentExtent.Width)
                offset = 0;
            else if (offset + viewportSize.Width >= contentExtent.Width)
                offset = contentExtent.Width - viewportSize.Width;

            // Tweak the offset so that it is always on line boundary.
            this.FirstVisibleColumn = (int)Math.Round(offset / Configurations.FormatFontWidth);
            if (this.FirstVisibleColumn < 0)
                this.FirstVisibleColumn = 0;
            offset = this.FirstVisibleColumn * Configurations.FormatFontWidth;

            // See note in "SetVerticalOffset" below.
            if (this.contentOffset.X != offset)
            {
                this.contentOffset.X = offset;
                TextEditorCore.HorizontalScrollPosition = offset;
                InvalidateOwnerScrollInfo();
                visualHost.UpdateVisualOnLayout();
            }
        }

        public void SetVerticalOffset(double offset)
        {
            // Make sure the requested offset is well within the range.
            if (offset < 0 || viewportSize.Height >= contentExtent.Height)
                offset = 0;
            else if (offset + viewportSize.Height >= contentExtent.Height)
                offset = contentExtent.Height - viewportSize.Height;

            // Tweak the offset so that it is always on line boundary.
            double topMostLine = offset / Configurations.FontDisplayHeight;
            this.FirstVisibleLine = (int)Math.Round(topMostLine);
            offset = this.FirstVisibleLine * Configurations.FontDisplayHeight;

            // This method can be called when user drags on the thumb which fires 
            // at a higher frequency, we don't always want to set the content 
            // offset blindly, especially we always try to limit "offset" to be 
            // on the line boundaries. Imagine when the "contentOffset.Y" is 
            // having value of "45.0", user scrolls with thumb and "offset" comes 
            // in with the value of "46.8". The "offset" is then rounded down to 
            // "45.0", which is the same as "contentOffset.Y". In such cases, we 
            // will then not attempt to change the value of "contentOffset.Y", 
            // and avoid having to render something that's essentially the same.
            // 
            if (this.contentOffset.Y != offset)
            {
                this.contentOffset.Y = offset;
                TextEditorCore.VerticalScrollPosition = offset;
                InvalidateOwnerScrollInfo();
                visualHost.UpdateVisualOnLayout();
            }
        }

        public double VerticalOffset
        {
            get { return contentOffset.Y; }
        }

        public double ViewportHeight
        {
            get { return viewportSize.Height; }
        }

        public double ViewportWidth
        {
            get { return viewportSize.Width; }
        }

        #endregion

        #region Private Class Helper Methods

        int GetMaxVisibleColumns(double width)
        {
            double actualWidth = width - Configurations.CanvasMarginLeft;
            return (int)Math.Floor(actualWidth / Configurations.FormatFontWidth);
        }

        int GetMaxVisibleLines(double height)
        {
            int fontHeight = Configurations.FontDisplayHeight;
            return (int)Math.Floor(height / fontHeight);
        }

        void HandleMeasureArrange(ref Size updatedSize)
        {
            if (double.IsInfinity(updatedSize.Width))
                updatedSize.Width = 0.0;
            if (double.IsInfinity(updatedSize.Height))
                updatedSize.Height = 0.0;

            int currMaxLines = GetMaxVisibleLines(viewportSize.Height);
            int newMaxLines = GetMaxVisibleLines(updatedSize.Height);

            if (updatedSize != this.viewportSize)
            {
                this.viewportSize = updatedSize;
                RefreshSrollBarVisibility();
                InvalidateOwnerScrollInfo();
            }

            if (currMaxLines != newMaxLines)
            {
                visualHost.UpdateVisualOnLayout();
                if (null != this.TextEditorCore)
                    this.TextEditorCore.SetPageSize(newMaxLines);
            }
        }

        void RefreshSrollBarVisibility()
        {
            // The new dimension may or may not require scroll bars.
            int fontHeight = Configurations.FontDisplayHeight;
            double fontWidth = Configurations.FormatFontWidth;
            canVerticallyScroll = contentExtent.Height > viewportSize.Height;
            canHorizontallyScroll = contentExtent.Width > viewportSize.Width;

            int maxFirstVisibleLine = 0;
            int maxFirstVisibleCol = 0;

            if (contentExtent.Height > viewportSize.Height)
                maxFirstVisibleLine = (int)Math.Floor((contentExtent.Height - viewportSize.Height) / fontHeight);

            if (FirstVisibleLine > maxFirstVisibleLine)
                FirstVisibleLine = maxFirstVisibleLine;

            if (contentExtent.Width > viewportSize.Width)
                maxFirstVisibleCol = (int)Math.Round((contentExtent.Width - viewportSize.Width) / Configurations.FormatFontWidth);

            if (FirstVisibleColumn > maxFirstVisibleCol)
                FirstVisibleColumn = maxFirstVisibleCol;
        }

        void InvalidateOwnerScrollInfo()
        {
            if (null != this.scrollOwner)
                this.scrollOwner.InvalidateScrollInfo();
        }

        #endregion
    }
}
