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
using DesignScript.Editor.Core;

namespace DesignScript.Editor.Extensions
{
    public delegate void SliderValueUpdated(object sender, EventArgs e);
    /// <summary>
    /// Interaction logic for NumericSlider.xaml
    /// </summary>
    public partial class NumericSlider : UserControl
    {
        public int max = 1000;
        public int min = -1000;
        public int intValue;
        public double doubleValue;
        private double lastVal = 0.0;
        private bool reset = false;
        private double prevSliderValue = 0;
        public SliderMode numSliderMode = SliderMode.Integer;
        TextEditorControl textEditorControl;
        public static RoutedCommand DismissSlider = new RoutedCommand();

        // Delegate event for Extensions namespace to allow slider changing values to be
        // reacted upon by the relevant handler located in NumericSliderExtension class
        public event SliderValueUpdated UpdateSliderValue = null;

        public enum SliderMode
        {
            Double,
            Integer
        };

        #region Public Class Operational Methods

        public NumericSlider()
        {
            InitializeComponent();

            CommandBinding commandBinding = new CommandBinding(DismissSlider,
            DismissSliderExecuted, DismissSliderCanExecute);
            this.CommandBindings.Add(commandBinding);

            MaxValue.Text = max.ToString();
            MinValue.Text = min.ToString();
            CurrentPoint = new System.Drawing.Point();
            ValueChanged = false;
            txtMax.Text = MaxValue.Text;
            txtMin.Text = MinValue.Text;
        }

        /// <summary>
        /// Returns the current slider value after computinf the equivalent of the log function on the slider.
        /// </summary>
        /// <returns>Current slider value.</returns>
        public double GetSliderValue()
        {
            double sliderValue = Slider.Value - 5.0;
            double returnValue;

            if (sliderValue > 0)
            {
                double linearStep = (sliderValue / 5.0) * (Math.Log(max));
                returnValue = Math.Exp(linearStep);
                return returnValue;
            }
            else if (sliderValue < 0)
            {
                double linearStep = (sliderValue / 5.0) * (Math.Log(Math.Abs(min)));
                returnValue = Math.Exp(Math.Abs(linearStep)) * -1;
                return returnValue;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Sets the slider value after calculating on the log function.
        /// </summary>
        /// <param name="actualValue">The actual value of the slider</param>
        public void SetSliderValue(double actualValue)
        {
            NumSlider.Visibility = Visibility.Visible;

            double sliderValue = 5.0;
            if (actualValue > 0)
                sliderValue = sliderValue + 5.0 * (Math.Log(actualValue) / Math.Log(max));
            else if (actualValue < 0)
                sliderValue = sliderValue + (-1.0) * (5.0 * Math.Log(Math.Abs(actualValue)) / Math.Log(Math.Abs(min)));
            else Slider.Value = 0;

            Slider.Value = sliderValue;
        }

        /// <summary>
        /// Resets the text and position of the slider for an integer by calling SetSliderValue
        /// </summary>
        /// <param name="textEditorControl">Current TextEditorControl</param>
        /// <param name="fragmentValue">The new slider value that needs to be set.</param>
        public void ResetSlider(TextEditorControl textEditorControl, int fragmentValue)
        {
            reset = true;
            SetSliderValue(fragmentValue);
            CurrentValue.Text = fragmentValue.ToString();
            this.textEditorControl = textEditorControl;
            Logger.LogInfo("Slider-ResetSlider", fragmentValue.ToString());

        }

        /// <summary>
        /// Resets the text and position of the slider for a double value.
        /// </summary>
        /// <param name="textEditorControl"></param>
        /// <param name="fragmentValue"></param>
        public void ResetSlider(TextEditorControl textEditorControl, double fragmentValue)
        {
            reset = true;
            SetSliderValue(fragmentValue);
            CurrentValue.Text = fragmentValue.ToString();
            this.textEditorControl = textEditorControl;
            Logger.LogInfo("Slider-ResetSlider", fragmentValue.ToString());

        }

        // Sets the current position on the screen.
        public void SetCurrentPointOnScreen(System.Drawing.Point position)
        {
            if (!ValueChanged)
            {
                CurrentPoint = position;
            }
        }

        #endregion

        #region Public Class Properties

        public bool ValueChanged { get; set; }
        public System.Drawing.Point CurrentPoint { get; private set; }

        #endregion

        #region Private Class Event Handlers

        private void OntxtMaxLostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                max = Convert.ToInt32(txtMax.Text);
                MaxValue.Text = max.ToString();
            }
            catch
            {
                textEditorControl.DisplayStatusMessage(StatusTypes.Warning, Configurations.NumericSliderWarning, 3);
            }

            Logger.LogInfo("Slider-OntxtMinLostFocus", max.ToString());

        }

        private void OntxtMinLostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                min = Convert.ToInt32(txtMin.Text);
                MinValue.Text = min.ToString();
            }
            catch
            {
                textEditorControl.DisplayStatusMessage(StatusTypes.Warning, Configurations.NumericSliderWarning, 3);
            }

            Logger.LogInfo("Slider-OntxtMinLostFocus", min.ToString());

        }

        private void OnRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            RadioButton li = (sender as RadioButton);

            if (li.Content != null)
            {
                if (li.Content.ToString() == "Double")
                {
                    Logger.LogInfo("Slider-OnRadioButtonChecked", "SliderMode.Double");

                    numSliderMode = SliderMode.Double;
                }
                else
                {
                    Logger.LogInfo("Slider-OnRadioButtonChecked", "SliderMode.Integer");

                    numSliderMode = SliderMode.Integer;
                }
            }
        }

        private void OnSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (reset)
            {
                reset = false;
                CurrentValue.Text = "0";
                return;
            }

            doubleValue = GetSliderValue();
            intValue = Convert.ToInt32(doubleValue);

            if (doubleValue != lastVal)
            {
                Logger.LogInfo("Slider-OnSliderValueChanged", doubleValue.ToString());


                ValueChanged = true;
                lastVal = doubleValue;
                if (numSliderMode == SliderMode.Integer)
                    CurrentValue.Text = intValue.ToString();
                else
                    CurrentValue.Text = doubleValue.ToString();

                // Slider value changed needs to be handled by the class containing the slider
                // In this case it is the NumericSliderExtension class
                if (null != UpdateSliderValue)
                    UpdateSliderValue(this, e);

                // Updates the UI for the current script to show the change in TextBuffer.
                textEditorControl.UpdateUiForModifiedScript(Solution.Current.ActiveScript);
            }
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            Logger.LogInfo("Slider-OnLostKeyboardFocus", "OnLostKeyboardFocus");

            if (SliderMenu.IsOpen == false)
            {
                if (this.IsKeyboardFocusWithin == false)
                {
                    (this.Parent as ExtensionPopup).DismissExtensionPopup(true);
                    if (Slider.Value != prevSliderValue)
                        textEditorControl.RunScript();
                    prevSliderValue = Slider.Value;
                }
                base.OnLostKeyboardFocus(e);
            }
        }

        #endregion

        private void DismissSliderExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (SliderMenu.IsOpen == false)
            {
                (this.Parent as ExtensionPopup).DismissExtensionPopup(true); ;
                prevSliderValue = Slider.Value;
            }
        }

        private void DismissSliderCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = true;
        }
    }
}
