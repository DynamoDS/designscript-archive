using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using DesignScript.Editor.Core;
using DesignScript.Editor.CodeGen;
using System.Windows.Controls;
using System.Diagnostics;

namespace DesignScript.Editor.Extensions
{
    public class FunctionSignatureExtension : EditorExtension
    {
        ExtensionPopup functionSignaturePopup = null;
        FunctionSignatureControl functionSignatureControl = null;

        #region Public Class Operational Methods
        /// <summary>
        /// Constructor for AutoComplete ToolTip
        /// </summary>
        public FunctionSignatureExtension()
        {
        }
        #endregion

        #region Protected Class Override Methods

        protected override void PreKeyDownEventCore(KeyEventArgs e)
        {
            if (IsFunctionSignaturePopupVisible() == false)
                return; // Nothing to see here, move on guys.

            switch (e.Key)
            {
                case Key.Up:
                case Key.Down:
                case Key.PageUp:
                case Key.PageDown:
                case Key.Home:
                case Key.End:
                    {
                        // When there's no overload, then navigation should 
                        // be allowed to happen (and don't eat it, please!)
                        if (functionSignatureControl.TotalOverloads <= 1)
                            DisplayOrRetainToolTip(false);
                        else
                        {
                            e.Handled = true;
                            functionSignatureControl.ChangeOverload(e.Key);
                        }

                        break;
                    }

                case Key.Escape:
                    e.Handled = true;
                    HideFunctionSignaturePopup();
                    break;
            }
        }

        protected override void PostKeyDownEventCore(KeyEventArgs e, char character)
        {
            if (character != char.MinValue && (character == '(' || character == ','))
            {
                // Open bracket, time to show the popup!
                DisplayOrRetainToolTip(true);
                return;
            }

            DisplayOrRetainToolTip(false);
        }


        /// <summary>
        /// Hide the tooltip if the Mouse is clicked when it is visible
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        protected override bool OnMouseUpCore(MouseButtonEventArgs e)
        {
            // if autocomplete tooltip is showing, hide it!
            HideFunctionSignaturePopup();
            return true;
        }

        protected override bool HasInputFocusCore()
        {
            if (null == functionSignaturePopup)
                return false;

            return functionSignaturePopup.IsKeyboardFocusWithin;
        }

        /// <summary>
        /// Hide Tooltip if MainWindow loses focus
        /// </summary>
        /// <returns></returns>
        protected override bool OnLostFocusCore()
        {
            if (functionSignaturePopup != null)
                functionSignaturePopup.IsOpen = false;

            return true;
        }

        #endregion

        #region Private Class Helper Methods

        private void EnsureToolTipCreated()
        {
            if (null == functionSignatureControl)
            {
                functionSignatureControl = new FunctionSignatureControl();
                functionSignatureControl.Visibility = Visibility.Visible;
            }

            if (functionSignaturePopup == null)
            {
                functionSignaturePopup = new ExtensionPopup(this);
                functionSignaturePopup.Child = functionSignatureControl;
                functionSignaturePopup.IsOpen = false;
            }
        }

        private bool DisplayOrRetainToolTip(bool forceEvaluation)
        {
            // If the function tool-tip is visible, then always re-evaluate to 
            // see if the tool-tip needs to be retained. The forced evaluation 
            // can happen when user keys in '(' character, or press some shortcut 
            // keys to trigger function tool-tip at a function call site.
            // 
            bool popupVisible = IsFunctionSignaturePopupVisible();
            if (popupVisible == false && (!forceEvaluation))
                return false;

            int line = textCore.CursorPosition.Y;
            int column = textCore.CursorPosition.X;

            ITextBuffer textBuffer = textCore.CurrentTextBuffer;
            FunctionCallContext callContext = textBuffer.GetFunctionCallContext(line, column);

            // We won't need to do anything if the popup is already in the right state.
            bool validCallContext = (null != callContext && (callContext.IsValidCallContext));
            if (popupVisible == validCallContext)
            {
                // Highlight (or bold) the current argument based on 
                // the cursor position in the function call context.
                if (false != popupVisible)
                    HighlightArgumentMapping(callContext);

                return popupVisible;
            }

            if (false == validCallContext)
            {
                HideFunctionSignaturePopup();
                return false; // Tool-tip hidden.
            }

            if (GenerateCompletionDataForCallContext() == false)
                return false; // Possible compilation failures.

            return (HighlightArgumentMapping(callContext));
        }

        private bool HighlightArgumentMapping(FunctionCallContext callContext)
        {
            int line = textCore.CursorPosition.Y;
            int column = textCore.CursorPosition.X;

            int argument = -1;
            string function = callContext.GetFunctionAtPoint(column, line, out argument);
            return HighlightArgumentMapping(function, argument);
        }

        private bool HighlightArgumentMapping(string function, int argument)
        {
            return DisplayToolTipForCallContext(function, argument);
        }

        /// <summary>
        /// This method is called just once at the beginning when function 
        /// tool-tip is about to be displayed. It generates the necessary data 
        /// ready for queries by the function tool-tip control. This data stays 
        /// for as long as the tool-tip stays visible, user can go from one 
        /// function to the next with arrow keys, the function signature tool-
        /// tip will highlight the corresponding argument, or be refreshed with 
        /// information of a new function (e.g. when user moves the cursor from
        /// one function to the next in a nested function call). Due to this 
        /// reason, this method is not function-specific, it compiles the entire
        /// source code.
        /// </summary>
        private bool GenerateCompletionDataForCallContext()
        {
            if (AutoCompletionHelper.IsHelperReset == false)
                return true; // There's no need to recompute.

            int[] linesToExclude = new int[] { textCore.CursorPosition.Y };
            ITextBuffer textBuffer = textCore.CurrentTextBuffer;
            string partialContent = textBuffer.GetPartialContent(linesToExclude, " \n");

            if (null == AutoCompletionHelper.MessageHandler)
                AutoCompletionHelper.MessageHandler = new AutoCompleteMessageHandler(textEditorControl);

            // Clear output stream messages before attempting another compilation.
            AutoCompleteMessageHandler messageHandler = null;
            messageHandler = AutoCompletionHelper.MessageHandler as AutoCompleteMessageHandler;
            messageHandler.ClearMessages();

            IScriptObject activeScript = Solution.Current.ActiveScript;
            string filePath = activeScript.GetParsedScript().GetScriptPath();
            if (string.IsNullOrEmpty(filePath))
                filePath = null;

            if (AutoCompletionHelper.Compile(partialContent, filePath) == false)
                return false;

            messageHandler.DisplayPossibleErrors();
            return true;
        }

        private bool DisplayToolTipForCallContext(string function, int argument)
        {
            Logger.LogInfo("AutocompleteToolTip-FunctionToolTip", function);

            EnsureToolTipCreated();

            int line = textCore.CursorPosition.Y;
            int column = textCore.CursorPosition.X;
            bool controlShown = functionSignatureControl.UpdateForFunctionArgument(
                line, column, function, argument);

            if (false == controlShown)
                return false;

            if (false == functionSignaturePopup.IsOpen)
            {
                functionSignaturePopup.Placement = PlacementMode.Custom;
                functionSignaturePopup.PlacementTarget = textEditorCanvas;
                functionSignaturePopup.CursorPosition = textCore.CursorPosition;
                functionSignaturePopup.IsOpen = true;
                Keyboard.Focus(functionSignatureControl);
            }

            return true;
        }

        private bool IsFunctionSignaturePopupVisible()
        {
            if (null != functionSignaturePopup)
                return functionSignaturePopup.IsOpen;

            return false;
        }

        private void HideFunctionSignaturePopup()
        {
            AutoCompletionHelper.Reset();
            if (null != functionSignaturePopup)
            {
                functionSignatureControl.ResetContent();
                functionSignaturePopup.IsOpen = false;
            }
        }

        #endregion
    }
}
