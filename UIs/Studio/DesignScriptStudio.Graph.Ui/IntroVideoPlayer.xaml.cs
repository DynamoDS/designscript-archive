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
using System.Windows.Shapes;
using System.Windows.Threading;
using DesignScriptStudio.Graph.Core;
using System.Reflection;
using System.Windows.Resources;

namespace DesignScriptStudio.Graph.Ui
{
    /// <summary>
    /// Interaction logic for IntroVideoPlayer.xaml
    /// </summary>
    public partial class IntroVideoPlayer : Window
    {
        enum VideoStates
        {
            None,
            Playing,
            Paused
        }

        int currentBreakpoint = 0;
        VideoStates videoState = VideoStates.None;
        DispatcherTimer timer = null;
        List<int> breakpoints = new List<int>(); // in milliseconds

        public IntroVideoPlayer()
        {
            InitializeComponent();
            mediaElement.ScrubbingEnabled = true;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(10);
            timer.Tick += new EventHandler(OnTimerTick);
            this.GetIntervals();
            this.Title = "Intro to DesignScriptStudio";
            this.Loaded += new RoutedEventHandler(OnIntroVideoPlayerLoaded);
            PrevButton.MouseLeftButtonUp += new MouseButtonEventHandler(OnPrevButtonMouseLeftButtonUp);
            NextButton.MouseLeftButtonUp += new MouseButtonEventHandler(OnNextButtonMouseLeftButtonUp);
        }

        private void OnIntroVideoPlayerLoaded(object sender, RoutedEventArgs e)
        {
            string temporaryVideoPath = System.IO.Path.GetTempPath() + @"IntroVideo.wmv";

            if (!System.IO.File.Exists(temporaryVideoPath))
                ExtractVideoResourceToFile(temporaryVideoPath);

            mediaElement.Source = new Uri(temporaryVideoPath);
            mediaElement.LoadedBehavior = MediaState.Manual;
            mediaElement.UnloadedBehavior = MediaState.Manual;
            mediaElement.Play();
            videoState = VideoStates.Playing;
            timer.Start();
        }

        private void ExtractVideoResourceToFile(string targetFile)
        {
            // Create temporary output stream file name.
            System.IO.FileStream file = new System.IO.FileStream(
                targetFile, System.IO.FileMode.Create);

            Uri uri = new Uri(ResourceNames.IntroductionVideo, UriKind.RelativeOrAbsolute);
            StreamResourceInfo resourceInfo = Application.GetResourceStream(uri);
            System.IO.Stream resourceStream = resourceInfo.Stream;

            if (null != resourceStream)
            {
                try
                {
                    int offset = 0, bytesRead = 0;
                    byte[] buffer = new byte[4096];
                    while ((bytesRead = resourceStream.Read(buffer, offset, buffer.Length)) > 0)
                    {
                        file.Write(buffer, offset, bytesRead);
                    }

                    file.Flush();
                    file.Close();
                }
                catch (Exception)
                {
                }
            }
        }

        private void OnPrevButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (videoState != VideoStates.Paused)
                return; // No skipping while playing.

            if (currentBreakpoint > 0)
                currentBreakpoint = currentBreakpoint - 1;

            JumpToBreakpoint(currentBreakpoint, false);
        }

        private void OnNextButtonMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (videoState != VideoStates.Paused)
                return; // No skipping while playing.

            JumpToBreakpoint(currentBreakpoint, true);
        }

        private void JumpToBreakpoint(int breakpoint, bool startPlaying)
        {
            if (breakpoint < 0 || (breakpoint >= breakpoints.Count))
                return; // Invalid breakpoint index.

            if (false == startPlaying) // Jump and pause.
            {
                mediaElement.Pause();
                mediaElement.Position = TimeSpan.FromMilliseconds(breakpoints[breakpoint]);
                videoState = VideoStates.Paused;
                ButtonPanel.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                mediaElement.Position = TimeSpan.FromMilliseconds(breakpoints[breakpoint]);
                mediaElement.Play();
                videoState = VideoStates.Playing;
                ButtonPanel.Visibility = System.Windows.Visibility.Collapsed;
            }

            //this.Title = mediaElement.Position.TotalMilliseconds.ToString();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {

            if (videoState != VideoStates.Playing)
                return; // Not playing, don't check.

            if (currentBreakpoint >= 0 && (currentBreakpoint < (breakpoints.Count - 1)))
            {
                //this.Title = mediaElement.Position.TotalMilliseconds.ToString();

                int nextBreakpoint = currentBreakpoint + 1;
                if (mediaElement.Position.TotalMilliseconds > breakpoints[nextBreakpoint])
                {
                    JumpToBreakpoint(nextBreakpoint, false);
                    currentBreakpoint = nextBreakpoint;
                }
            }
        }

        private void OnMediaElementMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (videoState == VideoStates.Paused)
            {
                if (currentBreakpoint >= breakpoints.Count - 1)
                    currentBreakpoint = 0;

                JumpToBreakpoint(currentBreakpoint, true);
            }
        }

        private void OnMediaElementMediaEnded(object sender, RoutedEventArgs e)
        {
            currentBreakpoint = 0;
            mediaElement.Pause();
            mediaElement.Position = TimeSpan.FromMilliseconds(0);
            videoState = VideoStates.Paused;
            ButtonPanel.Visibility = System.Windows.Visibility.Visible;
        }

        private void GetIntervals()
        {
            breakpoints.Add(0);
            breakpoints.Add(780);
            breakpoints.Add(1270);
            breakpoints.Add(1860);
            breakpoints.Add(4910);
            breakpoints.Add(5850);
            breakpoints.Add(9420);
            breakpoints.Add(9890);
            breakpoints.Add(10370);
            breakpoints.Add(10900);
            breakpoints.Add(11390);
            breakpoints.Add(17020);
            breakpoints.Add(19660);
            breakpoints.Add(20880);
            breakpoints.Add(21460);
            breakpoints.Add(21960);
            breakpoints.Add(22540);
            breakpoints.Add(23290);
            breakpoints.Add(23860);
            breakpoints.Add(26370);
            breakpoints.Add(27010);
            breakpoints.Add(29250);
            breakpoints.Add(31290);
            breakpoints.Add(31860);
            breakpoints.Add(34880);
        }
    }
}
