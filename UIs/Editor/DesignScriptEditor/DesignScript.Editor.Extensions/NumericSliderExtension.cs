using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls.Primitives;
using DesignScript.Editor.Core;
using System.Windows;
using System.Windows.Controls;

namespace DesignScript.Editor.Extensions
{
    public class NumericSliderExtension : EditorExtension
    {
        NumericSlider numericSlider = null;
        ExtensionPopup numericSliderPopup = null;
        CodeFragment fragmentSign = null, fragmentNumber = null;

        #region Public Class Operational Methods
        /// <summary>
        /// Constructor for Numeric Slider Extension
        /// </summary>
        internal NumericSliderExtension()
        {
        }

        #endregion

        #region Protected Class Override Methods

        /// <summary>
        /// Triggered on a MouseUp event which is activated here
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        protected override bool OnMouseUpCore(System.Windows.Input.MouseButtonEventArgs e)
        {
            Logger.LogInfo("Slider-OnMouseUpCore", "Slider-OnMouseUpCore");


            // When it no longer becomes an alpha feature, this should be removed
            if (textEditorControl.GetEditorSettings().EnableNumericSlider == false)
                return true;

            ShowNumericSlider();
            return true;
        }

        protected override bool HasInputFocusCore()
        {
            if (null == numericSliderPopup)
                return false;

            return numericSliderPopup.IsKeyboardFocusWithin;
        }

        #endregion

        #region Private Class Event Handlers
        /// <summary>
        /// Delegate method invoked when the change to the slider value has to be handled by the TextEditorControl
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSliderValueUpdated(object sender, EventArgs e)
        {
            HandleSliderValueChanged();
        }
        #endregion

        #region Private Class Helper Methods
        /// <summary>
        /// Shows the numeric slider for the first time when there has been a mouse click on a number.
        /// </summary>
        /// <param name="cursor">The cursor position at which the mouse click occurred.</param>
        private void ShowNumericSlider()
        {
            System.Drawing.Point cursor = textCore.CursorPosition;

            // Get the fragment on which the cursor has been placed.
            textCore.GetFragment(cursor.X, cursor.Y, out fragmentNumber);
            if (null == fragmentNumber || (fragmentNumber.CodeType != CodeFragment.Type.Number))
            {
                // Fragment is not numeric, hide the slider.
                if (null != numericSliderPopup)
                    numericSliderPopup.IsOpen = false;
                return;
            }

            if (numericSliderPopup == null)
            {
                numericSlider = new NumericSlider();
                numericSlider.UpdateSliderValue += new SliderValueUpdated(OnSliderValueUpdated);

                // Set the alignment of the slider.
                numericSlider.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                numericSlider.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                numericSlider.Width = 172;
                numericSlider.Height = 40;

                numericSliderPopup = new ExtensionPopup(this);
                numericSliderPopup.Child = numericSlider;
                numericSliderPopup.IsOpen = false;
            }

            // Set the SliderMode to double if the fragment contains a decimal point in it.
            numericSlider.numSliderMode = NumericSlider.SliderMode.Integer;
            if (fragmentNumber.Text.Contains('.'))
                numericSlider.numSliderMode = NumericSlider.SliderMode.Double;

            // Check if the current fragment is '-' sign(fragment type is also Number) 
            // and get the next fragment to set the SliderMode.
            if (fragmentNumber.Text == "-")
            {
                // @TODO(Suba): Use a GetNextFragment that takes "CodeFragment".
                CodeFragment nextFragment;
                //textCore.GetNextFragment(fragmentNumber, out nextFragment);
                textCore.GetFragment(cursor.X + 1, cursor.Y, out nextFragment);
                if (nextFragment == null)
                    return;
                numericSlider.numSliderMode = NumericSlider.SliderMode.Integer;
                if (nextFragment.Text.Contains('.'))
                    numericSlider.numSliderMode = NumericSlider.SliderMode.Double;

                // Set the currentFragment to be the fragment containing the "number" instead of "-"
                fragmentNumber = nextFragment;
            }

            numericSliderPopup.Placement = PlacementMode.Custom;
            numericSliderPopup.PlacementTarget = textEditorCanvas;
            numericSliderPopup.CursorPosition = cursor;
            numericSliderPopup.IsOpen = true;
            System.Windows.Input.Keyboard.Focus(numericSlider);

            numericSlider.SetCurrentPointOnScreen(cursor);
            // Get the previous fragment to determine if it is a '-' sign to set the Slider value.
            // @TODO(Suba): Make this to take CodeFragment instead.
            textCore.GetPreviousFragment(fragmentNumber.ColStart, fragmentNumber.Line, out fragmentSign);

            // If the mode is integer then you set the slider value by calling the ResetSlider which takes an 'int'
            // and if it is a Double value then you call the ResetSlider which takes in a 'double'
            if (numericSlider.numSliderMode == NumericSlider.SliderMode.Integer)
            {
                if (fragmentSign.Text.Equals("-"))
                    numericSlider.ResetSlider(textEditorControl, (-1) * Convert.ToInt32(fragmentNumber.Text));
                else
                {
                    fragmentSign = null;
                    numericSlider.ResetSlider(textEditorControl, Convert.ToInt32(fragmentNumber.Text));
                }
            }
            else
            {
                if (fragmentSign.Text.Equals("-"))
                    numericSlider.ResetSlider(textEditorControl, (-1.0) * Convert.ToDouble(fragmentNumber.Text));
                else
                {
                    fragmentSign = null;
                    numericSlider.ResetSlider(textEditorControl, Convert.ToDouble(fragmentNumber.Text));
                }
            }
        }

        /// <summary>
        /// Makes all the necessary changes to TextBuffer and CodeFragment once the slider value has been changed.
        /// Calls function SetFragmentText to change the value of the CodeFragment and takes in arguments to
        /// accordingly ColEnd of the CodeFragment.
        /// Calls function ModifyText to replace the text in the TextBuffer to show the change in the value of the 
        /// slider. It takes in the entire line to be replaced in the TextBuffer as an argument and makes the change.
        /// </summary>
        private void HandleSliderValueChanged()
        {
            int sliderValue = 0;
            double sliderValueD = 0;
            // FragmentSign contains the fragment which contains the '-' sign 
            // and FragmentNumber contains the fragment with the number value.
            if (fragmentNumber != null && fragmentNumber.CodeType == CodeFragment.Type.Number)
            {
                // Retrieve the current line content from the TextBuffer.
                string oldLineContent = textCore.CurrentTextBuffer.GetLineContent(fragmentNumber.Line);
                string newLineContent = oldLineContent;

                //fragmentSignLineContent = textCore.CurrentTextBuffer.GetLineContent(fragmentSign.Line);
                string numSliderValue = null;

                if (numericSlider.numSliderMode == NumericSlider.SliderMode.Integer)
                {
                    // Get the new slider value to change the line content to contain it.
                    sliderValue = numericSlider.intValue;
                    newLineContent = newLineContent.Remove(fragmentNumber.ColStart, fragmentNumber.ColEnd - fragmentNumber.ColStart + 1);
                    numSliderValue = sliderValue.ToString();
                    // Calls the Mirror to set the value of the number during RunTime.
                    Solution.Current.ExecutionSession.SetValue(fragmentNumber.Line, fragmentNumber.ColStart, sliderValue);
                    // Reset the slider to show the new value.
                    numericSlider.ResetSlider(textEditorControl, sliderValue);
                }
                else
                {
                    // Round the number to 4 decimal places.
                    sliderValueD = numericSlider.doubleValue;
                    sliderValueD = Math.Round(sliderValueD, 4);
                    newLineContent = newLineContent.Remove(fragmentNumber.ColStart, fragmentNumber.ColEnd - fragmentNumber.ColStart + 1);
                    Solution.Current.ExecutionSession.SetValue(fragmentNumber.Line, fragmentNumber.ColStart, sliderValueD);
                    numSliderValue = sliderValueD.ToString();
                    numericSlider.ResetSlider(textEditorControl, sliderValueD);
                }

                // If the number is originally a negative number with the fragment containing the '-' sign in FragmentSign
                // Then replace it by a space as the Fragment text and the line content in the TextBuffer have been updated
                // to contain the '-' sign automatically.
                // Insert the new slider value into the line content.
                if (fragmentSign != null)
                {
                    // Remove the '-' sign from TextBuffer as well ('-' has a length of 1)
                    if (fragmentSign.Line == fragmentNumber.Line)
                    {
                        if (fragmentNumber.ColStart == fragmentSign.ColEnd + 1)
                        {
                            newLineContent = newLineContent.Remove(fragmentSign.ColStart, 1);
                            textCore.SetFragmentText(fragmentSign.ColStart, fragmentSign.Line, string.Empty);
                            newLineContent = newLineContent.Insert(fragmentSign.ColStart, numSliderValue);
                            fragmentSign = null;
                        }


                        else
                        {
                            newLineContent = newLineContent.Remove(fragmentSign.ColStart, 1);
                            //newLineContent = newLineContent.Remove(fragmentSign.ColStart, fragmentNumber.ColStart-fragmentSign.ColEnd-1);
                            textCore.SetFragmentText(fragmentSign.ColStart, fragmentSign.Line, string.Empty);
                            newLineContent = newLineContent.Insert(fragmentNumber.ColStart , numSliderValue);
                            fragmentSign = null;
                        }

                    }
                    else
                    {
                        newLineContent = newLineContent.Insert(fragmentSign.ColStart, numSliderValue);
                        textCore.SetFragmentText(fragmentSign.ColStart, fragmentSign.Line, string.Empty);
                        string fragmentSignLineContent = textCore.CurrentTextBuffer.GetLineContent(fragmentSign.Line);
                        fragmentSignLineContent = fragmentSignLineContent.Remove(fragmentSign.ColStart, 1);
                        textCore.CurrentTextBuffer.ModifyText(fragmentSign.Line, fragmentSignLineContent);
                        fragmentSign = null;
                    }
                }
                else
                {
                    newLineContent = newLineContent.Insert(fragmentNumber.ColStart, numSliderValue);
                }

                // Set the fragment text with the new slider value.
                textCore.SetFragmentText(fragmentNumber.ColStart, fragmentNumber.Line, numSliderValue);
                // Replace the old line content with the new line content.
                textCore.CurrentTextBuffer.ModifyText(fragmentNumber.Line, newLineContent);

                Logger.LogInfo("SliderChange", fragmentNumber.Line.ToString() + " => " + newLineContent);

            }

            numericSlider.ValueChanged = false;
        }

        #endregion

    }
}
