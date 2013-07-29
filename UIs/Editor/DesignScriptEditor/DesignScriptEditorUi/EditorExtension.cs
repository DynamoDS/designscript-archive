using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using DesignScript.Editor.Core;
using System.Windows;
using System.Windows.Controls;

namespace DesignScript.Editor
{
    /// <summary>
    /// Base class of all extensions for the DesignScript IDE and a generic way of resolving events
    /// </summary>
    public abstract class EditorExtension
    {
        #region Protected Class Members

        protected TextEditorControl textEditorControl = null;
        protected ITextEditorCore textCore = null;
        protected TextEditorCanvas textEditorCanvas = null;
        private ScrollViewer scrollViewer = null;

        #endregion

        #region Public Class Operational Methods

        /// <summary>
        /// As an extension, information of the core and the control needs to be present in all extensions, therefore
        /// it is a protected base class member
        /// </summary>
        /// <param name="editor"> TextEditorControl object </param>
        /// <param name="core"> TextCore Singleton object </param>
        public void SetEditorCore(TextEditorControl editor, ITextEditorCore core)
        {
            textEditorControl = editor;
            textCore = core;
            textEditorCanvas = textEditorControl.FindName("textCanvas") as TextEditorCanvas;
            scrollViewer = textEditorControl.FindName("scrollViewer") as ScrollViewer;
        }

        public void RouteEventToCanvas(RoutedEventArgs e)
        {
            if (null != textEditorCanvas)
                textEditorCanvas.RaiseEvent(e);
        }

        public void SetKeyboardFocusOnCanvas()
        {
            if (null != textEditorCanvas)
                Keyboard.Focus(textEditorCanvas);
        }

        public System.Windows.Point TranslateToScreenPoint(System.Drawing.Point charPosition)
        {
            IScriptObject activeScript = Solution.Current.ActiveScript;
            CharPosition position = activeScript.CreateCharPosition();
            position.SetCharacterPosition(charPosition.X, charPosition.Y);
            System.Windows.Point result = position.GetScreenPosition();

            // The screen position needs offset based on the current scroll position.
            result.Offset(-scrollViewer.HorizontalOffset, -scrollViewer.VerticalOffset);
            return result;
        }

        /// <summary>
        /// DesignScript IDE calls this method before a key gets sent to the 
        /// editor for processing (e.g. navigation or input text). An extension 
        /// can choose to process this and prevent the key event from being 
        /// processed by the text editor by setting the "e.Handled" to "true".
        /// </summary>
        /// <param name="e">The argument for a keyboard event, set "e.Handled" 
        /// to "true" in order to avoid further processing by the IDE.</param>
        public void PreKeyDownEvent(KeyEventArgs e)
        {
            PreKeyDownEventCore(e);
        }

        /// <summary>
        /// DesignScript IDE calls this method after a key has been processed 
        /// by the text editor. If this represents an alpha-numeric key, the 
        /// content of text editor would have been updated at this point. To 
        /// get the up-to-date text content, an extension can choose to handle 
        /// this event.
        /// </summary>
        /// <param name="e">The argument of this keyboard event.</param>
        /// <param name="character">The translated character, if the key maps 
        /// to an alphabet character, otherwise this will be char.MinValue
        /// (for cases like Backspace, etc).</param>
        public void PostKeyDownEvent(KeyEventArgs e, char character)
        {
            PostKeyDownEventCore(e, character);
        }

        /// <summary>
        /// Event to handle LeftMouseButtonUp event
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public bool OnMouseUp(MouseButtonEventArgs e)
        {
            return OnMouseUpCore(e);
        }

        /// <summary>
        /// Event to handle a MouseHover - this is in reference to a MouseOver 
        /// and held there till a timer ticks after a certain interval. Such an 
        /// event doesn't exist and therefore it a compound combination of two 
        /// events, labelled MouseHover
        /// </summary>
        /// <param name="validHover"></param>
        /// <param name="variable"></param>
        /// <returns></returns>
        public bool OnMouseHover(CodeFragment fragment)
        {
            return OnMouseHoverCore(fragment);
        }

        /// <summary>
        /// Event trigger to Handle what happens when the Main Window loses focus
        /// </summary>
        /// <returns></returns>
        public bool OnLostFocus()
        {
            return OnLostFocusCore();
        }

        /// <summary>
        /// This is used to determine if the extension has input focus. It can 
        /// be useful for the host application if it needs to check before switching
        /// the input focus to some other parts of the UI.
        /// </summary>
        /// <returns>Returns true if any child/popup window has the input focus, 
        /// or false otherwise.</returns>
        public bool HasInputFocus()
        {
            return HasInputFocusCore();
        }

        #endregion

        #region Protected Virtual Methods

        protected virtual void PreKeyDownEventCore(KeyEventArgs e)
        {
        }

        protected virtual void PostKeyDownEventCore(KeyEventArgs e, char character)
        {
        }

        protected virtual bool OnMouseUpCore(MouseButtonEventArgs e)
        {
            return false;
        }

        protected virtual bool OnMouseHoverCore(CodeFragment variable = null)
        {
            return false;
        }

        protected virtual bool OnLostFocusCore()
        {
            return false;
        }

        protected virtual bool HasInputFocusCore()
        {
            return false;
        }

        #endregion
    }
}
