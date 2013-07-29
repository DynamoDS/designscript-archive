using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows;
using DesignScript.Editor.Core;
using System.Windows.Controls;

namespace DesignScript.Editor.Extensions
{
    public class InspectionToolTipExtension : EditorExtension
    {
        InspectionToolTip inspectionToolTip = null;
        ExtensionPopup inspectionToolTipPopup = null;
        CodeFragment fragmentPointer = null;

        #region Public Class Operational Methods

        public InspectionToolTipExtension()
        {
        }

        #endregion

        #region Protected Class Override Methods

        protected override bool OnMouseHoverCore(CodeFragment variable)
        {
            if (fragmentPointer == variable)
            {
                if (null == variable)
                    return true;

                // If we are looking at the same variable since we last checked, 
                // and the variable is not 'null', we may need to bring up the 
                // inspection tooltip anyway (since it may not have been created).
                // 
                // Non-null variable, and the inspection tooltip is visible, then
                // there's nothing to do. Otherwise we need to proceed to create it.
                if (false != inspectionToolTipPopup.IsOpen)
                    return true;
            }

            fragmentPointer = variable;

            // A valid hover is defined as a Hover over a variable, and the hover lasts an entire time
            // determined by the inspectionToolTipTimer in the TextEditorControl
            if (variable == null)
            {
                Logger.LogInfo("OnMouseHoverCore", "null");
                if ((null != inspectionToolTipPopup) && inspectionToolTipPopup.IsOpen)
                {
                    if (inspectionToolTipPopup.IsMouseOver != true)
                    {
                        // The cursor is outside of the tooltip when "variable == null"
                        // (which means the mouse hovers over an empty spot in the text
                        // editor canvas), we'll handle this by hiding the popup and 
                        // setting the focus back onto the canvas.
                        // 
                        inspectionToolTip.DeactivateTooltip();
                        inspectionToolTipPopup.IsOpen = false;
                        System.Windows.Input.Keyboard.Focus(this.textEditorCanvas);
                    }

                    return true; // Notification handled.
                }

                return true; // Notification handled.
            }
            else
            {
                Logger.LogInfo("OnMouseHoverCore", variable.Text);
                if (variable.CodeType != CodeFragment.Type.Local)
                {
                    if (null != inspectionToolTip)
                        inspectionToolTip.DeactivateTooltip();
                    if (null != inspectionToolTipPopup)
                        inspectionToolTipPopup.IsOpen = false;

                    System.Windows.Input.Keyboard.Focus(this.textEditorCanvas);
                    return true; // We only allow inspection of variable.
                }

                EnsureToolTipCreated();

                inspectionToolTipPopup.IsOpen = false;
                if (inspectionToolTip.ActivateTooltip(variable.Text))
                {
                    inspectionToolTip.Width = inspectionToolTip.GetWidth();
                    inspectionToolTip.Height = inspectionToolTip.GetHeight();
                    inspectionToolTipPopup.Placement = PlacementMode.Custom;
                    inspectionToolTipPopup.PlacementTarget = textEditorCanvas;
                    inspectionToolTipPopup.CursorPosition = new System.Drawing.Point()
                    {
                        X = fragmentPointer.ColStart,
                        Y = fragmentPointer.Line
                    };

                    inspectionToolTipPopup.IsOpen = true;
                    System.Windows.Input.Keyboard.Focus(inspectionToolTip);
                }

                return true;
            }
        }

        protected override bool HasInputFocusCore()
        {
            if (null == inspectionToolTipPopup)
                return false;

            return inspectionToolTipPopup.IsKeyboardFocusWithin;
        }

        #endregion

        #region Private Class Helper Methods

        private void EnsureToolTipCreated()
        {
            if (null == inspectionToolTip)
                inspectionToolTip = new InspectionToolTip();

            if (null == inspectionToolTipPopup)
            {
                inspectionToolTipPopup = new ExtensionPopup(this);
                inspectionToolTipPopup.Child = inspectionToolTip;
                inspectionToolTipPopup.IsOpen = false;
            }
        }

        #endregion
    }
}
