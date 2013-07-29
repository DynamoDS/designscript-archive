using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DesignScript.Editor.Core;
using DesignScript.Editor.Ui;
using Microsoft.Win32;
using System.Windows;

namespace DesignScript.Editor
{
    public class DialogProvider : IDialogProvider
    {
        #region IDialogProvider Members

        public ReadOnlyDialogResult ShowReadOnlyDialog(bool allowOverwriteOption)
        {
            if (false != TextEditorControl.Instance.IsInPlaybackMode)
                return ReadOnlyDialogResult.OverWrite;

            ReadOnlyPrompt dialog = new ReadOnlyPrompt(allowOverwriteOption);
            dialog.ShowDialog();
            return dialog.userOption;
        }

        public void ShowExceptionDialog(Exception exception)
        {
            try
            {
                Logger.LogInfo("ShowExceptionDialog", exception.ToString());
            }
            catch (Exception)
            {

            }

            // We don't want to show exception dialog in automation mode.
            if (false == TextEditorControl.Instance.IsInPlaybackMode)
            {
                ExceptionWindow exceptionWindow = new ExceptionWindow(exception);
                if (null != Application.Current)
                    exceptionWindow.Owner = Application.Current.MainWindow;
                exceptionWindow.ShowDialog();
            }
        }

        public bool ShowSaveFileDialog(ref string fileName)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = fileName; // Default file name
            saveFileDialog.DefaultExt = ".ds"; // Default file extension
            saveFileDialog.Filter = "DesignScript Files|*.ds"; // Filter files by extension

            bool? result = saveFileDialog.ShowDialog();
            fileName = saveFileDialog.FileName;
            return (result.HasValue && (result.Value == true));
        }

        public void ShowFileAlreadyOpenDialog()
        {
            string message = "File is currently in use. Cannot be overwritten.";
            System.Windows.Forms.MessageBox.Show(message, "DesignScript IDE",
                System.Windows.Forms.MessageBoxButtons.OK,
                System.Windows.Forms.MessageBoxIcon.Warning);
        }

        public bool ShowReloadDialog(string fileName)
        {
            string message = fileName + " has been modified by another program.\nDo you want to reload it?";
            System.Windows.Forms.DialogResult result = System.Windows.Forms.MessageBox.Show(message, "DesignScript IDE",
                System.Windows.Forms.MessageBoxButtons.YesNo);
            return (result == System.Windows.Forms.DialogResult.Yes);
        }

        public void DisplayStatusMessage(StatusTypes statusType, string message, int seconds)
        {
            TextEditorControl.Instance.DisplayStatusMessage(statusType, message, seconds);
        }

        #endregion
    }
}
