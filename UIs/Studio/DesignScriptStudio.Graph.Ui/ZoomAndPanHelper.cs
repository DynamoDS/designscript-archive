using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using DesignScriptStudio.Graph.Core;

namespace DesignScriptStudio.Graph.Ui
{
    class ZoomAndPanAnimationHelper
    {
        public static readonly DependencyProperty GridScale = DependencyProperty.Register(
            "Scale", typeof(double), typeof(GraphVisualHost),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender,
                new PropertyChangedCallback(OnScaleChanged)));

        public static readonly DependencyProperty ScrollviewerXOffset = DependencyProperty.Register(
            "XOffset", typeof(double), typeof(GraphVisualHost),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender,
                new PropertyChangedCallback(OnXOffsetChanged)));
        public static readonly DependencyProperty ScrollviewerYOffset = DependencyProperty.Register(
            "YOffset", typeof(double), typeof(GraphVisualHost),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender,
                new PropertyChangedCallback(OnYOffsetChanged)));

        public double XOffset { get; set; }
        public double YOffset { get; set; }

        private static GraphControl staticGraphControl = null;
        private static ScrollViewer staticScrollViewer = null;
        private GraphVisualHost visualHost = null;

        private static double newOffsetX;
        private static double newOffsetY;
        private static double newScale;

        private Storyboard zoomAndPanStoryboard = null;

        private static Point mousePosition = new Point(-1, -1);
        private CursorSet.Index currentCursor = CursorSet.Index.Pointer;

        //Zoom to Fit Animation
        public ZoomAndPanAnimationHelper(double newOffsetX, double newOffsetY, double newScale, GraphVisualHost visualHost, GraphControl graphControl, bool captureMouse)
        {
            staticGraphControl = graphControl;
            this.visualHost = visualHost;
            staticScrollViewer = staticGraphControl.canvasScrollViewer;

            ZoomAndPanAnimationHelper.newOffsetX = newOffsetX;
            ZoomAndPanAnimationHelper.newOffsetY = newOffsetY;
            ZoomAndPanAnimationHelper.newScale = newScale;

            double offsetX = staticGraphControl.canvasScrollViewer.HorizontalOffset;
            double offsetY = staticGraphControl.canvasScrollViewer.VerticalOffset;
            double scale = staticGraphControl.scaleTransform.ScaleX;

            double shiftX = newOffsetX - offsetX;
            double shiftY = newOffsetY - offsetY;
            double scaleChange = newScale - scale;

            if (captureMouse)
            {
                Point mousePositionOnScreen = GetMousePosition();
                mousePosition = new Point((mousePositionOnScreen.X + offsetX) / scale,
                                          (mousePositionOnScreen.Y + offsetY) / scale);

                this.currentCursor = staticGraphControl.GetCursor();
                //System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.None;
            }

            if (Math.Abs(shiftX) < 0.05
                && Math.Abs(shiftY) < 1
                && Math.Abs(scaleChange) < 1)
                return;

            zoomAndPanStoryboard = new Storyboard();

            if (Math.Abs(shiftX) >= 0.05)
            {
                DoubleAnimation scaleAnimation = new DoubleAnimation();
                scaleAnimation.From = scale;
                scaleAnimation.To = newScale;
                scaleAnimation.Duration = TimeSpan.FromMilliseconds(Configurations.ShortAnimationTime);
                Storyboard.SetTarget(scaleAnimation, visualHost);
                Storyboard.SetTargetProperty(scaleAnimation, new PropertyPath("Scale"));
                zoomAndPanStoryboard.Children.Add(scaleAnimation);
            }

            if (Math.Abs(shiftX) >= 1)
            {
                DoubleAnimation xOffsetAnimation = new DoubleAnimation();
                xOffsetAnimation.From = offsetX;
                xOffsetAnimation.To = newOffsetX;
                xOffsetAnimation.Duration = TimeSpan.FromMilliseconds(Configurations.ShortAnimationTime);
                Storyboard.SetTarget(xOffsetAnimation, visualHost);
                Storyboard.SetTargetProperty(xOffsetAnimation, new PropertyPath("XOffset"));
                zoomAndPanStoryboard.Children.Add(xOffsetAnimation);
            }

            if (Math.Abs(shiftY) >= 1)
            {
                DoubleAnimation yOffsetAnimation = new DoubleAnimation();
                yOffsetAnimation.From = offsetY;
                yOffsetAnimation.To = newOffsetY;
                yOffsetAnimation.Duration = TimeSpan.FromMilliseconds(Configurations.ShortAnimationTime);
                Storyboard.SetTarget(yOffsetAnimation, visualHost);
                Storyboard.SetTargetProperty(yOffsetAnimation, new PropertyPath("YOffset"));
                zoomAndPanStoryboard.Children.Add(yOffsetAnimation);
            }

            zoomAndPanStoryboard.Completed += new EventHandler(OnAnimationCompleted);

            BeginAnimation();
        }

        //Ctrl/Shift Pan Animation
        public ZoomAndPanAnimationHelper(double newOffsetX, double newOffsetY, GraphVisualHost visualHost, GraphControl graphControl)
        {
            mousePosition = graphControl.canvasScrollViewer.TranslatePoint(GetMousePosition(), graphControl.CurrentGraphCanvas);
            this.currentCursor = graphControl.GetCursor();
            //System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.None;

            staticGraphControl = graphControl;
            this.visualHost = visualHost;
            staticScrollViewer = staticGraphControl.canvasScrollViewer;

            ZoomAndPanAnimationHelper.newOffsetX = newOffsetX;
            ZoomAndPanAnimationHelper.newOffsetY = newOffsetY;
            ZoomAndPanAnimationHelper.newScale = double.MinValue;

            double offsetX = staticGraphControl.canvasScrollViewer.HorizontalOffset;
            double offsetY = staticGraphControl.canvasScrollViewer.VerticalOffset;

            double shiftX = newOffsetX - offsetX;
            double shiftY = newOffsetY - offsetY;

            if (Math.Abs(shiftX) < 0.01
                && Math.Abs(shiftY) < 0.01)
                return;

            DoubleAnimation xOffsetAnimation = new DoubleAnimation();
            xOffsetAnimation.From = offsetX;
            xOffsetAnimation.To = newOffsetX;
            xOffsetAnimation.Duration = TimeSpan.FromMilliseconds(Configurations.ShortAnimationTime);
            Storyboard.SetTarget(xOffsetAnimation, visualHost);
            Storyboard.SetTargetProperty(xOffsetAnimation, new PropertyPath("XOffset"));

            DoubleAnimation yOffsetAnimation = new DoubleAnimation();
            yOffsetAnimation.From = offsetY;
            yOffsetAnimation.To = newOffsetY;
            yOffsetAnimation.Duration = TimeSpan.FromMilliseconds(Configurations.ShortAnimationTime);
            Storyboard.SetTarget(yOffsetAnimation, visualHost);
            Storyboard.SetTargetProperty(yOffsetAnimation, new PropertyPath("YOffset"));

            zoomAndPanStoryboard = new Storyboard();
            zoomAndPanStoryboard.Children.Add(xOffsetAnimation);
            zoomAndPanStoryboard.Children.Add(yOffsetAnimation);
            zoomAndPanStoryboard.Completed += new EventHandler(OnAnimationCompleted);

            BeginAnimation();
        }

        public void BeginAnimation()
        {
            this.visualHost.animationOn = true;
            staticGraphControl.CurrentGraphCanvas.graphCanvas.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.LowQuality);

            zoomAndPanStoryboard.Begin();
        }

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };
        public static Point GetMousePosition()
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        private static void OnScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            staticGraphControl.scaleTransform.ScaleX = (double)e.NewValue;
            staticGraphControl.scaleTransform.ScaleY = (double)e.NewValue;
        }

        private static void OnXOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            staticScrollViewer.ScrollToHorizontalOffset((double)e.NewValue);
        }

        private static void OnYOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            staticScrollViewer.ScrollToVerticalOffset((double)e.NewValue);
        }

        private void OnAnimationCompleted(object sender, EventArgs e)
        {
            staticGraphControl.CurrentGraphCanvas.graphCanvas.SetValue(RenderOptions.BitmapScalingModeProperty, BitmapScalingMode.Fant);

            //To ensure the scale and offset reach the "end"
            if (ZoomAndPanAnimationHelper.newScale != double.MinValue)
            {
                staticGraphControl.scaleTransform.ScaleX = ZoomAndPanAnimationHelper.newScale;
                staticGraphControl.scaleTransform.ScaleY = ZoomAndPanAnimationHelper.newScale;
            }
            staticScrollViewer.ScrollToHorizontalOffset(ZoomAndPanAnimationHelper.newOffsetX);
            staticScrollViewer.ScrollToVerticalOffset(ZoomAndPanAnimationHelper.newOffsetY);

            Slider slider = staticGraphControl.slider;  // update the slider value
            slider.Value = staticGraphControl.scaleTransform.ScaleX;
            if (mousePosition != new Point(-1, -1))
            {
                SetCursorPos((int)(mousePosition.X * slider.Value - staticScrollViewer.HorizontalOffset),
                             (int)(mousePosition.Y * slider.Value - staticScrollViewer.VerticalOffset));
                mousePosition = new Point(-1, -1);
            }

            staticGraphControl.SetCursor(currentCursor);
            this.visualHost.animationOn = false;
        }
    }
}