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
using System.Windows.Media.Animation;
using System.Windows.Controls.Primitives;
using DesignScript.Editor.Automation;
using DesignScript.Editor.Core;

namespace DesignScript.Editor
{
    /// <summary>
    /// Interaction logic for EditorWidgetBar.xaml
    /// </summary>
    public partial class EditorWidgetBar : UserControl
    {
        #region Private Class Data Members

        internal enum Widget { Output, Watch, Playback, Errors, Disassembly, WidgetCount };

        // State control data members.
        private int foregroundWidget = -1;
        private bool widgetsVisible = false;
        private double animationSeconds = 0.5;
        private double widgetsHeight = 0.0;
        private double minGridHeight = 64.0;

        // Animation related data members.
        Storyboard mainStoryboard = null;
        DoubleAnimation animation = null;

        // Interface components.
        private Grid mainEditorGrid = null;
        private GridSplitter mainSplitter = null;
        private UserControl[] widgets = null;
        private ToggleButton[] toggleButtons = null;

        #endregion

        #region Public Class Operational Methods

        public EditorWidgetBar()
        {
            InitializeComponent();

            widgets = new UserControl[(int)Widget.WidgetCount];
            toggleButtons = new ToggleButton[(int)Widget.WidgetCount];
            toggleButtons[0] = this.OutputWidgetButton;
            toggleButtons[1] = this.WatchWidgetButton;
            toggleButtons[2] = this.PlaybackWidgetButton;
            toggleButtons[3] = this.ErrorWidgetButton;
            toggleButtons[4] = this.DisassemblyButton;

#if !DEBUG
            this.DisassemblyButton.Visibility = System.Windows.Visibility.Collapsed;
#endif

            foreach (ToggleButton button in toggleButtons)
            {
                button.IsChecked = false;
                button.Click += new RoutedEventHandler(OnWidgetButtonClick);
            }
        }

        internal void Initialize(TextEditorControl textEditorControl)
        {
            this.mainEditorGrid = textEditorControl.FindName("grid") as Grid;
            this.mainSplitter = textEditorControl.FindName("MainGridSplitter") as GridSplitter;

            // If you get this assertion, then the "grid" element in XAML has 
            // been modified. Which means the following codes that expect a 
            // particular grid layout, need changing too. Look for all parts 
            // in this file with GRIDLAYOUTSPECIFIC tag and update their logic!
            // 
            System.Diagnostics.Debug.Assert(mainEditorGrid.RowDefinitions.Count == 7);
            System.Diagnostics.Debug.Assert(mainEditorGrid.RowDefinitions[3].Height.IsStar);
            mainEditorGrid.RowDefinitions[5].Height = new GridLength(0.0);
            mainSplitter.Visibility = Visibility.Collapsed;
        }

        internal void ActivateWidget(Widget widget, bool forceActivation)
        {
            if (false == forceActivation)
            {
                // We are not forcing activation here (we're asking nicely). 
                // If the widgets are already visible, then don't proceed.
                if (false != widgetsVisible)
                    return;
            }

            // Toggle the corresponding button first...
            for (int index = 0; index < toggleButtons.Length; ++index)
            {
                bool isChecked = (index == ((int)widget));
                toggleButtons[index].IsChecked = isChecked;
            }

            this.BringWidgetToForeground(widget);
            DisplayWidgets(true);
        }

        internal void AssociateWidget(Widget widget, UserControl userControl)
        {
            int widgetIndex = ((int)widget);
            if (null == widgets[widgetIndex])
            {
                widgets[widgetIndex] = userControl;

                // GRIDLAYOUTSPECIFIC: Revise this logic if grid changes.
                userControl.SetValue(Grid.RowProperty, 5);
                if (mainEditorGrid.Children.Add(userControl) != -1)
                {
                    if (userControl.Visibility != System.Windows.Visibility.Visible)
                        userControl.Visibility = System.Windows.Visibility.Visible;

                    mainEditorGrid.UpdateLayout();
                }

                // Display the button corresponding to the widget.
                toggleButtons[widgetIndex].Visibility = Visibility.Visible;
            }

            // Toggle the corresponding button first...
            for (int index = 0; index < toggleButtons.Length; ++index)
            {
                bool isChecked = (index == ((int)widget));
                toggleButtons[index].IsChecked = isChecked;
            }

            // Start the transition to show the widgets.
            BringWidgetToForeground(widget);
            DisplayWidgets(true);
        }

        #endregion

        #region Public Class Properties

        public static readonly DependencyProperty ExtensionHeightProperty = DependencyProperty.Register(
            "ExtensionHeight", typeof(double), typeof(EditorWidgetBar), new FrameworkPropertyMetadata(0.0,
                FrameworkPropertyMetadataOptions.AffectsRender, new PropertyChangedCallback(OnExtensionHeightChanged)));

        public double ExtensionHeight
        {
            get { return ((double)GetValue(HeightProperty)); }
            set { SetValue(HeightProperty, value); }
        }

        #endregion

        #region Private Class Helper Methods

        private void OnWidgetButtonClick(object sender, RoutedEventArgs e)
        {
            ToggleButton button = sender as ToggleButton;
            if (false != button.IsChecked.Value)
            {
                // Uncheck all other buttons (except the current one).
                for (int index = 0; index < toggleButtons.Length; ++index)
                {
                    ToggleButton otherButton = toggleButtons[index];
                    if (otherButton == button)
                    {
                        EnsureWidgetPreloaded((Widget)index);
                        ActivateWidget((Widget)index, true);
                        continue;
                    }

                    if (otherButton.IsChecked.Value == true)
                        otherButton.IsChecked = false;
                }
            }
            else
            {
                bool atLeastOneButtonToggled = false;
                foreach (ToggleButton toggleButton in toggleButtons)
                {
                    if (toggleButton.IsChecked.Value != false)
                    {
                        atLeastOneButtonToggled = true;
                        break;
                    }
                }

                // Show/hide the widget pane if needed.
                DisplayWidgets(atLeastOneButtonToggled);
            }
        }

        private static void OnExtensionHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            EditorWidgetBar widgetBar = d as EditorWidgetBar;
            if (null != widgetBar)
            {
                GridLength newLength = new GridLength((double)e.NewValue);
                widgetBar.mainEditorGrid.RowDefinitions[5].Height = newLength;
            }
        }

        private void BringWidgetToForeground(Widget widget)
        {
            Logger.LogInfo("Editor-Widget-Bar-BringWidgetToForeground", widget.ToString());

            int widgetIndex = ((int)widget);
            if (foregroundWidget == widgetIndex)
                return;

            if (null != widgets[widgetIndex])
                widgets[widgetIndex].Visibility = Visibility.Visible;

            // Hide the current widget (if any).
            if (foregroundWidget != -1)
            {
                if (null != widgets[foregroundWidget])
                    widgets[foregroundWidget].Visibility = Visibility.Collapsed;
            }

            foregroundWidget = widgetIndex;
        }

        private void EnsureWidgetPreloaded(Widget widget)
        {
            if (null != widgets[(int)widget])
                return;

            // By getting the Instance of each user control, it 
            // forces them to be instantiated and inserted into 
            // the editor widget bar.
            // 
            UserControl userControl = null;

            switch (widget)
            {
                case Widget.Output: userControl = OutputWindow.Instance; break;
                case Widget.Watch: userControl = InspectionViewControl.Instance; break;
                case Widget.Playback: userControl = PlaybackVisualizer.Instance; break;
                case Widget.Disassembly: userControl = Disassembly.Instance; break;
                case Widget.Errors: userControl = ErrorWindow.Instance; break;
            }
        }

        private void DisplayWidgets(bool displayWidgets)
        {
            // TODO: Check if transition is on-going, bail out.

            if (displayWidgets == widgetsVisible)
                return; // Already shown/hidden.

            widgetsVisible = displayWidgets;
            if (false == widgetsVisible) // Begin hiding...
            {
                // GRIDLAYOUTSPECIFIC: Revise this logic if grid changes.
                widgetsHeight = mainEditorGrid.RowDefinitions[5].Height.Value;
            }
            else
            {
                // Begin transition to show the widgets.
                this.mainSplitter.Visibility = Visibility.Visible;
                widgetsHeight = AuditWidgetsHeight(widgetsHeight);
            }

            BeginAnimation(); // Start animation if needed.
        }

        private double AuditWidgetsHeight(double heightValue)
        {
            // GRIDLAYOUTSPECIFIC: Revise this logic if grid changes.

            // We only split the heights of rows for the first time.
            double upperHeight = mainEditorGrid.RowDefinitions[3].ActualHeight;
            if (heightValue < minGridHeight || ((upperHeight - heightValue) < minGridHeight))
            {
                // Height the top row (source code) or the bottom row (widgets) is 
                // of height lesser than desired, we'll just divide them equally.
                heightValue = upperHeight * 0.5;
            }

            return heightValue;
        }

        private void BeginAnimation()
        {
            if (null == mainStoryboard)
            {
                animation = new DoubleAnimation();
                animation.Duration = TimeSpan.FromSeconds(animationSeconds);
                animation.EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseInOut };

                Storyboard.SetTarget(animation, this);
                Storyboard.SetTargetProperty(animation, new PropertyPath("ExtensionHeight"));

                mainStoryboard = new Storyboard();
                mainStoryboard.Children.Add(animation);
                mainStoryboard.Completed += new EventHandler(OnAnimationCompleted);
            }

            if (false != widgetsVisible)
            {
                animation.From = 0.0;
                animation.To = widgetsHeight;
            }
            else
            {
                animation.To = 0.0;
                animation.From = widgetsHeight;
            }

            mainStoryboard.Begin();
        }

        private void OnAnimationCompleted(object sender, EventArgs e)
        {
            if (false == widgetsVisible)
            {
                // Transition to hide the widgets has completed.
                this.mainSplitter.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Transition to show widgets is completed, set the grid to final size.
                mainEditorGrid.RowDefinitions[5].Height = new GridLength(widgetsHeight);
            }
        }

        #endregion
    }
}
