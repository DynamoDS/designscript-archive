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

namespace DesignScriptStudio.Graph.Ui
{
    /// <summary>
    /// Interaction logic for ExtendedPreView.xaml
    /// </summary>
    public partial class ExtendedPreview : UserControl
    {
        private double originalWidth = double.MinValue;
        private double originalHeight = double.MinValue;
        private GraphControl graphControl;
        private GraphVisualHost visualHost;

        public ExtendedPreview(String text, double width, double height, GraphControl graphControl, GraphVisualHost visualHost)
        {
            InitializeComponent();

            this.graphControl = graphControl;
            this.visualHost = visualHost;
            this.InternalTextBox.Text = text;
            this.Focusable = true;
            this.IsEnabled = true;
            PreviewPanel.Focusable = true;
            PreviewPanel.IsEnabled = true;
            InternalTextBox.Focusable = true;
            InternalTextBox.IsEnabled = true;
            InternalTextBox.IsReadOnly = true;

            InternalTextBox.FontFamily = new FontFamily(Configurations.Font);
            InternalTextBox.FontWeight = FontWeights.Normal;
            InternalTextBox.FontSize = Configurations.InfoBubbleText;

            FormattedText formattedText = new FormattedText(text,
                                         Configurations.culture,
                                         FlowDirection.LeftToRight,
                                         Configurations.TypeFace,
                                         Configurations.InfoBubbleText,
                                         Configurations.PreviewBubbleExtendedTextColor);

            double textWidth = formattedText.WidthIncludingTrailingWhitespace + 2 * Configurations.InfoBubbleMargin + Configurations.PreviewBubbleExtendedScrollBarWidth;

            double extendedPreviewWidth;
            // the minimum width should allow the first value to be shown in one line
            if (text.Contains(','))
            {
                double minimumTextWidth = double.MinValue;
                string tempText = text.Substring(0, text.IndexOf(','));
                FormattedText tempFormattedText = new FormattedText(tempText,
                                             Configurations.culture,
                                             FlowDirection.LeftToRight,
                                             Configurations.TypeFace,
                                             Configurations.InfoBubbleText,
                                             Configurations.PreviewBubbleExtendedTextColor);
                minimumTextWidth = tempFormattedText.WidthIncludingTrailingWhitespace + 2 * Configurations.InfoBubbleMargin + Configurations.PreviewBubbleExtendedScrollBarWidth + 3;

                extendedPreviewWidth = Math.Max(width, minimumTextWidth);
            }
            else // if there is only one data member, need to fit in it
            {
                extendedPreviewWidth = Math.Max(width, textWidth);
            }

            //leave space for scrollbar and radial menu
            this.InternalTextBox.Width = extendedPreviewWidth - Configurations.PreviewBubbleExtendedScrollBarWidth;

            if (height > 0)
                InternalTextBox.Height = height;

            if (textWidth <= InternalTextBox.Width)
            {
                InternalTextBox.TextAlignment = TextAlignment.Center;
                InternalTextBox.Padding = new Thickness(Configurations.PreviewBubbleExtendedScrollBarWidth, Configurations.InfoBubbleMargin,
                                                        0, Configurations.InfoBubbleMargin);
            }
            else
            {
                InternalTextBox.MaxHeight = Configurations.InfoBubbleMaxHeight;
                InternalTextBox.TextWrapping = TextWrapping.Wrap;
                InternalTextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                InternalTextBox.Padding = new Thickness(Configurations.InfoBubbleMargin, Configurations.InfoBubbleMargin, 0, Configurations.InfoBubbleMargin);
            }
        }

        // prevent unwanted interaction (e.g. radial menu) triggered in canvas
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            visualHost.RemoveRadialMenu();
            e.Handled = true;
        }

        // prevent unwanted scroll on canvas once the preview is extended
        private void OnTextBoxGridMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
        }

        private void OnLeftThumbDragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            ResizeX(-e.HorizontalChange);
        }

        private void OnRightThumbDragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            ResizeX(e.HorizontalChange);
        }

        private void OnBottomThumbDragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            ResizeY(e.VerticalChange);
        }

        private void OnLeftBubbtomThumbDragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            ResizeX(-e.HorizontalChange);
            ResizeY(e.VerticalChange);
        }

        private void OnRightBubbtomThumbDragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            ResizeX(e.HorizontalChange);
            ResizeY(e.VerticalChange);
        }

        private void OnTopMouseMove(object sender, MouseEventArgs e)
        {
            IInfoBubble bubble;
            graphControl.CurrentGraphCanvas.VisualHost.Controller.GetInfoBubble((uint)this.Tag, out bubble);

            if (bubble.Collapsed)
                graphControl.SetCursor(CursorSet.Index.Expand);
            else
                graphControl.SetCursor(CursorSet.Index.Condense);

            e.Handled = true;
        }

        private void ResizeX(double delta)
        {
            ScrollViewer scrollViewer = FindVisualChild<ScrollViewer>(this);
            if (scrollViewer != null)
            {
                if (scrollViewer.ComputedVerticalScrollBarVisibility != System.Windows.Visibility.Visible
                    && delta > 0)
                    return;
            }

            double oldWidth = this.InternalTextBox.ActualWidth;
            double xPosition = (double)this.GetValue(Canvas.LeftProperty);

            if (originalWidth == double.MinValue) // first time to resize the preview
            {
                originalWidth = this.InternalTextBox.ActualWidth;
            }

            if (this.InternalTextBox.ActualWidth + delta / 2 < (Math.Min(originalWidth, Configurations.InfoBubbleResizeMinimumWidth)))
                this.InternalTextBox.Width = Math.Min(originalWidth, Configurations.InfoBubbleResizeMinimumWidth);
            else
                this.InternalTextBox.Width = this.InternalTextBox.ActualWidth + delta / 2;

            // update the position of the preview
            double shift = (this.InternalTextBox.Width - oldWidth) / 2;
            this.SetValue(Canvas.LeftProperty, xPosition - shift);
        }

        private void ResizeY(double delta)
        {
            ScrollViewer scrollViewer = FindVisualChild<ScrollViewer>(this);
            if (scrollViewer != null)
            {
                if (scrollViewer.ComputedVerticalScrollBarVisibility != System.Windows.Visibility.Visible
                    && delta > 0)
                    return;
            }

            if (originalHeight == double.MinValue) // first time to resize the preview
            {
                originalHeight = this.InternalTextBox.ActualHeight;
                this.InternalTextBox.MaxHeight = double.MaxValue;
            }

            if (this.InternalTextBox.ActualHeight + delta < (Math.Min(originalHeight, Configurations.InfoBubbleResizeMinimumHeight)))
                this.InternalTextBox.Height = Math.Min(originalHeight, Configurations.InfoBubbleResizeMinimumHeight);
            else
                this.InternalTextBox.Height = this.InternalTextBox.ActualHeight + delta;
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

        private void OnDotsMouseMove(object sender, MouseEventArgs e)
        {
            Point pt = new Point();
            pt.X = (double)this.GetValue(Canvas.LeftProperty) + this.ActualWidth;
            pt.Y = (double)this.GetValue(Canvas.TopProperty);
            graphControl.CurrentGraphCanvas.VisualHost.CreateRadialMenuForPreview(pt, (uint)(this.Tag));
            e.Handled = true;
        }
    }
}
