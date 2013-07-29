using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Timers;
using System.Windows;
using System.Xml.Serialization;
using System.IO;
using System.Windows.Controls.Primitives;

namespace DesignScript.Editor.Core
{
    using ProtoCore.CodeModel;
    using Microsoft.Win32;
    using DesignScript.Parser;

    internal partial class TextEditorCore : ITextEditorCore
    {
        #region Generic Utility Methods

        private System.Drawing.Point CursorPositionFromString(System.Drawing.Point startingPoint, string text)
        {
            System.Drawing.Point offset = startingPoint;

            foreach (char c in text)
            {
                if (c == '\n')
                {
                    offset.Y++;
                    offset.X = 0;
                }
                else
                    offset.X++;
            }

            return offset;
        }

        private void UpdateVisualOffsetFromCursor()
        {
            int line = scriptState.cursorPosition.CharacterY;
            int column = scriptState.cursorPosition.CharacterX;
            UpdateVisualOffset(column, line);
        }

        private void UpdateVisualOffset(int column, int line)
        {
            CharPosition converter = Solution.Current.ActiveScript.CreateCharPosition();
            scriptState.visualOffset = converter.CharToVisualOffset(line, column);
        }

        private bool DeleteSelection()
        {
            if (HasSelection == false)
                return false; // No selection!

            System.Drawing.Point cursor = scriptState.selectionStart.GetCharacterPosition();
            ReplaceTextInternal("", cursor);
            scriptState.mouseDownPosition = cursor; // Fix: IDE-125.
            return true;
        }

        private string CopySelectedText()
        {
            if (null == scriptState)
                return string.Empty;

            string textToCopy = scriptState.textBuffer.GetText(scriptState.selectionStart, scriptState.selectionEnd);

            textToCopy = textToCopy.Replace("\n", "\r\n");

            try
            {
                RichTextFormatter formatter = new RichTextFormatter(
                    scriptState.textBuffer, codeFragmentManager);

                string formattedContent = formatter.Format(
                    scriptState.selectionStart, scriptState.selectionEnd);

                DataObject clipboardData = new DataObject();
                clipboardData.SetData(DataFormats.Text, textToCopy);
                clipboardData.SetData(DataFormats.Rtf, formattedContent);
                Clipboard.SetDataObject(clipboardData);
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                System.Threading.Thread.Sleep(0);
                try
                {
                    Clipboard.SetData(DataFormats.Text, textToCopy);
                }
                catch (System.Runtime.InteropServices.COMException)
                {
                    MessageBox.Show("Can't Access Clipboard");
                }
            }

            return textToCopy;
        }

        public static bool IsPointInRegion(System.Drawing.Point regionStart,
            System.Drawing.Point regionEnd, System.Drawing.Point point)
        {
            if (point.Y < regionStart.Y || (point.Y > regionEnd.Y))
                return false;
            if (point.Y == regionStart.Y && (point.X < regionStart.X))
                return false;
            if (point.Y == regionEnd.Y && (point.X > regionEnd.X))
                return false;

            return true;
        }

        #endregion
    }
}
