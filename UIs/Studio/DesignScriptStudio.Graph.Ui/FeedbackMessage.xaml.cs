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
using System.Windows.Threading;
using DesignScriptStudio.Graph.Core;

namespace DesignScriptStudio.Graph.Ui
{
    /// <summary>
    /// Interaction logic for FeedbackMessage.xaml
    /// </summary>
    /// 
    public partial class FeedbackMessage : UserControl
    {
        GraphControl graphControl;

        public FeedbackMessage(string iconPath, string message, GraphControl graphControl)
        {
            InitializeComponent();

            this.graphControl = graphControl;
            this.Width = graphControl.FeedbackMsssagePanel.ActualWidth / 2;
            feedbackIcon.Source = new BitmapImage(new Uri(iconPath));
            feedbackMessage.Text = message;

            if (iconPath == ResourceNames.Confirmation)
            {
                DispatcherTimer timer = new DispatcherTimer();
                timer.Tick += new EventHandler(OnFeedBackMessageTimeout);
                timer.Interval = TimeSpan.FromSeconds(Configurations.ConfirmTimeSpan);
                timer.Start();
            }

            PreviewMouseLeftButtonDown += OnFeedbackMessageMouseLeftButtonDown;
        }

        internal void FadeFeedbackMessage()
        {
            Storyboard storyBoard = new Storyboard();

            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.From = 1;
            opacityAnimation.To = 0;
            opacityAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(Configurations.FadeTimeSpan));
            opacityAnimation.Completed += OnOffsetAnimationCompleted;

            storyBoard.Children.Add(opacityAnimation);

            Storyboard.SetTarget(opacityAnimation, this);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath(UIElement.OpacityProperty));

            storyBoard.Begin();
        }

        private void OnFeedBackMessageTimeout(object sender, EventArgs e)
        {
            FadeFeedbackMessage();
        }

        private void OnFeedbackMessageMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            FadeFeedbackMessage();
        }

        void OnOffsetAnimationCompleted(object sender, EventArgs e)
        {
            this.graphControl.RemoveFeedbackMessage(this);
        }
    }
}
