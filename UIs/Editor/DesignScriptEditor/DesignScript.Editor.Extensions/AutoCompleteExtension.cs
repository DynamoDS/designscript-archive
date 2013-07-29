using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows;
using DesignScript.Editor.Core;
using DesignScript.Editor.CodeGen;
using System.Windows.Controls;
using System.Diagnostics;

namespace DesignScript.Editor.Extensions
{
    class AutoCompleteMessageHandler : ProtoCore.IOutputStream
    {
        private TextEditorControl textEditorControl = null;
        private List<ProtoCore.OutputMessage> outputMessages = null;

        public AutoCompleteMessageHandler(TextEditorControl textEditorControl)
        {
            this.textEditorControl = textEditorControl;
            outputMessages = new List<ProtoCore.OutputMessage>();
        }

        #region IOutputStream Members

        public void Write(ProtoCore.OutputMessage message)
        {
            outputMessages.Add(message);
        }

        public List<ProtoCore.OutputMessage> GetMessages()
        {
            return outputMessages;
        }

        #endregion

        #region Public Operational Methods

        internal void ClearMessages()
        {
            outputMessages.Clear();
        }

        internal void DisplayPossibleErrors()
        {
            if (outputMessages.Count > 0)
            {
                ProtoCore.OutputMessage message = outputMessages[0];
                string display = string.Format("Line {0}, Column {1}: {2}",
                    message.Line, message.Column, message.Message);
                textEditorControl.DisplayStatusMessage(StatusTypes.Info, display, 6);
            }
            else
                textEditorControl.HideStatusMessage();
        }

        #endregion
    }

    public class AutoCompleteExtension : EditorExtension
    {
        ExtensionPopup autoCompletePopup = null;
        AutoCompleteList autoCompleteList = null;

        #region Public Class Operational Methods

        /// <summary>
        /// Constructor for AutoCompleteList
        /// </summary>
        public AutoCompleteExtension()
        {
        }

        #endregion

        #region Protected Class Override Methods

        protected override void PreKeyDownEventCore(KeyEventArgs e)
        {
            // The AutoComplete list isn't visible.
            if (this.HasInputFocus() == false)
                return;

            // We would want the AutoComplete list to handle navigation keys when 
            // it is visible, and not allowing them to be sent to the text editor 
            // which will result in text view scrolling or caret repositioning.
            // 
            switch (e.Key)
            {
                case Key.Up:
                case Key.Down:
                case Key.Right:
                case Key.Left:
                case Key.PageUp:
                case Key.PageDown:
                case Key.Home:
                case Key.End:
                    e.Handled = true;
                    break;
            }
        }

        protected override void PostKeyDownEventCore(KeyEventArgs e, char character)
        {
            // These are the keys that trigger an AutoComplete list.
            if (e.Key == Key.Decimal || e.Key == Key.OemPeriod)
                TriggerAutoCompleteList();
        }

        protected override bool OnMouseUpCore(MouseButtonEventArgs e)
        {
            // if autocomplete window is showing, hide it!
            if (null != autoCompletePopup)
                autoCompletePopup.DismissExtensionPopup(false);

            return true;
        }

        protected override bool HasInputFocusCore()
        {
            // The AutoComplete list always has focus if it is visible.
            return (null != autoCompletePopup && (autoCompletePopup.IsOpen));
        }

        #endregion

        #region Private Class Helper Methods

        private void EnsureAutoCompleteListCreated()
        {
            if (null == autoCompleteList)
            {
                autoCompleteList = new AutoCompleteList();
                autoCompleteList.SetTextEditorControl(textEditorControl);
            }

            if (null == autoCompletePopup)
            {
                autoCompletePopup = new ExtensionPopup(this);
                autoCompletePopup.Child = autoCompleteList;
                autoCompletePopup.IsOpen = false;
            }
        }

        private void TriggerAutoCompleteList()
        {
            // With this method one can get A.B when retrieving list for 'A.B.'
            ITextBuffer textBuffer = textCore.CurrentTextBuffer;
            string variable = textBuffer.GetIdentifierBeforeColumn(
                textCore.CursorPosition.Y, textCore.CursorPosition.X - 1);

            if (string.IsNullOrEmpty(variable))
                return;
            if (variable.EndsWith("."))
            {
                if (null != autoCompletePopup)
                    autoCompletePopup.DismissExtensionPopup(true);
                return; // Double dot, hide list!
            }

            Logger.LogDebug("TriggerAutoCompleteList", "UserPressed '.' ");
            Stopwatch sw = new Stopwatch();
            sw.Start();

            Logger.LogDebug("TriggerAutoCompleteList-variable", variable);
            if (Solution.Current.ActiveScript != null)
                Logger.LogDebug("TriggerAutoCompleteList-code", Solution.Current.ActiveScript.GetTextBuffer().GetContent());

            DoAutoComplete(variable);

            Logger.LogPerf("TextEditorControl.DoAutoComplete", sw.ElapsedMilliseconds + " ms");
        }

        /// <summary>
        /// Method to retrieve AutoComplete list and bind it to visual element
        /// </summary>
        private void DoAutoComplete(string variable)
        {
            Logger.LogInfo("DoAutoComplete", variable);


            EnsureAutoCompleteListCreated();

            IScriptObject activeScript = Solution.Current.ActiveScript;
            string filePath = activeScript.GetParsedScript().GetScriptPath();

            if (filePath == string.Empty)
                filePath = null;

            int[] linesToExclude = new int[] { textCore.CursorPosition.Y };
            ITextBuffer textBuffer = textCore.CurrentTextBuffer;
            string partialContent = textBuffer.GetPartialContent(linesToExclude, " \n");

            if (null == AutoCompletionHelper.MessageHandler)
                AutoCompletionHelper.MessageHandler = new AutoCompleteMessageHandler(textEditorControl);

            // Clear output stream messages before attempting another compilation.
            AutoCompleteMessageHandler messageHandler = null;
            messageHandler = AutoCompletionHelper.MessageHandler as AutoCompleteMessageHandler;
            messageHandler.ClearMessages();

            AutoCompletionHelper.SetSearchPaths(ExtensionFactory.GetSearchPaths());

            // Method to contact IDECodeGen.dll to retrieve the list for AutoComplete items
            List<KeyValuePair<AutoCompletionHelper.MemberType, string>> list =
                AutoCompletionHelper.GetList(textCore.CursorPosition.Y,
                textCore.CursorPosition.X, partialContent, variable, filePath);

            messageHandler.DisplayPossibleErrors();
            if (list.Count == 0)
                return;

            CharPosition position = activeScript.CreateCharPosition();
            position.SetCharacterPosition(textCore.CursorPosition.X, textCore.CursorPosition.Y);

            // Add each AutoComplete Item one at a time
            Keyboard.Focus(textEditorCanvas);
            autoCompleteList.ClearList();
            autoCompleteList.AddItemsToList(list);

            autoCompletePopup.Placement = PlacementMode.Custom;
            autoCompletePopup.PlacementTarget = textEditorCanvas;
            autoCompletePopup.CursorPosition = textCore.CursorPosition;
            autoCompletePopup.IsOpen = true;

            // The focus shift is important as the AutoCompleteList will 
            // be re-routing events back to the main control now.
            autoCompleteList.DoFocusOnFirstItem();
            autoCompletePopup.Width = autoCompleteList.Width;
            autoCompletePopup.Height = autoCompleteList.Height;
        }

        #endregion
    }
}
