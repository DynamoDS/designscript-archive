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

namespace DesignScript.Editor
{
    /// <summary>
    /// Interaction logic for RuggedProgressBar.xaml
    /// </summary>
    public partial class RuggedProgressBar : UserControl
    {
        #region Private Class Data Members

        // Sprite sheet animation control data members.
        int progressCurrentFrame = 0; // Current frame.
        int progressFrameHeight = 20; // Height of each frame.
        int progressFrameCount = 7; // Number of frames in sheet.

        long frameMilliseconds = 40; // 40 milliseconds (25 frames per second).
        DateTime previousFrameTime = DateTime.Now; // Time previous frame was rendered.

        #endregion

        internal RuggedProgressBar()
        {
            InitializeComponent();
        }

        internal void ShowProgressBar(bool show)
        {
            Visibility visibility = ((false == show) ? Visibility.Collapsed : Visibility.Visible);
            if (this.Visibility == visibility)
                return;

            // There's a change in visibility (install/remove rendering handler).
            if (visibility == System.Windows.Visibility.Visible)
                CompositionTarget.Rendering += OnCompositionTargetRendering;
            else
                CompositionTarget.Rendering -= OnCompositionTargetRendering;

            this.Visibility = visibility; // Display the progress bar control.
        }

        private void OnCompositionTargetRendering(object sender, EventArgs e)
        {
            TimeSpan duration = DateTime.Now - previousFrameTime;
            if (duration.Milliseconds < frameMilliseconds)
                return;

            previousFrameTime = DateTime.Now;
            progressCurrentFrame++;
            if (progressCurrentFrame >= progressFrameCount)
                progressCurrentFrame = 0;

            double topOffset = -progressCurrentFrame * progressFrameHeight;
            ProgressBarImage.SetValue(Canvas.TopProperty, topOffset);
        }
    }
}
