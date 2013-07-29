using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace DesignScript.Editor
{
    using DesignScript.Parser;
    using DesignScript.Editor.Core;
    using System.Windows.Controls.Primitives;
    using System.Windows.Media;
    using ProtoCore.CodeModel;

    public partial class TextEditorControl : UserControl
    {
        void OnMainWindowKeyDown(object sender, KeyEventArgs e)
        {
            IScriptObject activeScript = Solution.Current.ActiveScript;
            if (null == activeScript)
                return; // No active script.

            IParsedScript parsedScript = activeScript.GetParsedScript();
            if (null == parsedScript)
                return; // No active script.

            bool shift = Keyboard.IsKeyDown(Key.RightShift) || Keyboard.IsKeyDown(Key.LeftShift);
            bool control = Keyboard.IsKeyDown(Key.RightCtrl) || Keyboard.IsKeyDown(Key.LeftCtrl);
            bool capsLock = Console.CapsLock;
            bool alt = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);

            textCore.ReadOnlyState = IsDebuggingOrExecuting();
            bool programRunning = textCore.ReadOnlyState;

            parseTimer.Stop();
            parseTimer.Start();
            textCanvas.PauseCaretTimer(true);
            textCanvas.PauseCaretTimer(false);

            // Allow extensions to handle key event before allowing it to go into 
            // the text canvas. This way extensions can choose to filter or prevent 
            // a key from being processed.
            // 
            if (null != editorExtensions)
            {
                foreach (EditorExtension extension in editorExtensions)
                {
                    extension.PreKeyDownEvent(e);
                    if (e.Handled != false)
                        return;
                }
            }

            e.Handled = true;
            bool possibleLineChanges = false;
            char character = char.MinValue;

            switch (e.Key)
            {
                case Key.Delete:
                case Key.Back:
                    if (!CheckExecutionState())
                    {
                        possibleLineChanges = true;
                        textCore.DoControlCharacter(e.Key);
                    }
                    break;
            }

            if (false == control) //control == false
            {
                switch (e.Key)
                {
                    case Key.Space:
                    case Key.Tab:
                        if (!CheckExecutionState())
                            character = HandleCharacterKey(e);
                        break;
                    case Key.Enter:
                        if (!CheckExecutionState())
                        {
                            possibleLineChanges = true;
                            textCore.InsertText(GetKeyboardCharacter(e));
                        }
                        break;

                    default:
                        if ((e.Key >= Key.D0 && e.Key <= Key.Z) ||
                            (e.Key >= Key.NumPad0 && e.Key <= Key.Divide) ||
                            (e.Key >= Key.OemSemicolon && e.Key <= Key.Oem102))
                        {
                            if (!CheckExecutionState())
                                character = HandleCharacterKey(e);
                        }
                        break;
                }
            }

            if (control == true && alt == true)
            {
                if ((e.Key >= Key.D0 && e.Key <= Key.Z) ||
                    (e.Key >= Key.NumPad0 && e.Key <= Key.Divide) ||
                    (e.Key >= Key.OemSemicolon && e.Key <= Key.Oem102))
                {
                    if (!CheckExecutionState())
                        character = HandleCharacterKey(e);
                }
            }

            if (false != possibleLineChanges)
            {
                UpdateCanvasDimension();
                UpdateCaretPosition();
                textCanvas.BreakpointsUpdated();
            }

            switch (e.Key)
            {
                case Key.Left:
                case Key.Right:
                case Key.Home:
                case Key.End:
                case Key.PageUp:
                case Key.PageDown:
                    textCore.DoNavigation(e.Key);
                    break;

                case Key.Up:
                case Key.Down:
                    textCore.DoNavigation(e.Key);
                    break;

                case Key.F9:
                    {
                        bool isInDebugMode = false;
                        if (textCore.ToggleBreakpoint())
                            textCanvas.BreakpointsUpdated();
                        else if (string.IsNullOrEmpty(activeScript.GetParsedScript().GetScriptPath()))
                            DisplayStatusMessage(StatusTypes.Warning, Configurations.SaveFileBreakpointWarning, 3);
                        else if (Solution.Current.ExecutionSession.IsExecutionActive(ref isInDebugMode))
                            DisplayStatusMessage(StatusTypes.Warning, Configurations.BreakpointDebugWarning, 3);
                    }
                    break;
            }

            if (null != editorExtensions)
            {
                // Allow extensions to process the key after it is processed.
                foreach (EditorExtension extension in editorExtensions)
                    extension.PostKeyDownEvent(e, character);
            }

            // Some keys do not change the position of the cursor, for example 
            // CTRL and SHIFT keys. In such cases, the script doesn't need update.
            if (IsCursorMovingKeyEvent(e))
            {
                UpdateCaretPosition();
                UpdateUiForModifiedScript(Solution.Current.ActiveScript);
            }
        }

        private bool CheckExecutionState()
        {
            bool programRunning = textCore.ReadOnlyState;
            if (textCore.ReadOnlyState)
                DisplayStatusMessage(StatusTypes.Error, Configurations.EditingError, 3);
            return programRunning;
        }

        private bool IsCursorMovingKeyEvent(KeyEventArgs e)
        {
            if (null == e)
                return true;

            switch (e.Key)
            {
                case Key.LeftShift:
                case Key.RightShift:
                case Key.LeftAlt:
                case Key.RightAlt:
                case Key.LeftCtrl:
                case Key.RightCtrl:
                case Key.System: // This is the ALT key.
                    return false;
            }

            return true;
        }
    }
}
