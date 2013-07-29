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
using DesignScriptStudio.Graph.Core;

namespace DesignScriptStudio.Graph.Ui
{
    /// <summary>
    /// Interaction logic for Splash.xaml
    /// </summary>
    /// 
    public partial class Splash : UserControl
    {
        GraphControl graphControl;

        public Splash(GraphControl graphControl)
        {
            InitializeComponent();
            this.graphControl = graphControl;
        }

        internal void FadeSplash()
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

        void OnOffsetAnimationCompleted(object sender, EventArgs e)
        {
            this.graphControl.FeedbackMsssagePanel.Children.Remove(this);
        }

        private void OnDontShowPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
