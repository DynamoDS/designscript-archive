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
using ProtoCore;

namespace DesignScript.Editor
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>

    using DesignScript.Editor.Core;

    public partial class ErrorWindow : UserControl
    {
        private static ErrorWindow errorWindow = null;
        List<ProtoCore.OutputMessage> errorMessages = new List<OutputMessage>();
        int offset;

        #region Public Class Operational Methods

        public static void ClearError()
        {
            if (null != ErrorWindow.errorWindow)
                ErrorWindow.errorWindow.ClearErrorWindow();
        }

        public ErrorWindow()
        {
            InitializeComponent();
            offset = 0;
            lstErrorMessages.MouseWheel += new MouseWheelEventHandler(this.OnErrorWindowMouseWheel);
        }

        internal void SetErrorMessage(List<ProtoCore.OutputMessage> messages)
        {
            if (errorMessages != null)
                errorMessages.Clear();

            foreach (ProtoCore.OutputMessage message in messages)
            {
                if ((message.Type != OutputMessage.MessageType.Error) &&
                    (message.Type != OutputMessage.MessageType.Warning))
                    continue;

                if (null == errorMessages)
                    errorMessages = new List<ProtoCore.OutputMessage>();

                errorMessages.Add(message);
            }

            lstErrorMessages.ItemsSource = this.errorMessages;
            lstErrorMessages.Items.Refresh();
        }

        internal List<ProtoCore.OutputMessage> GetErrorMessage()
        {
            return this.errorMessages;
        }

        internal void ClearErrorWindow()
        {
            if (null != errorMessages)
            {
                errorMessages.Clear();
                lstErrorMessages.Items.Refresh();
            }
        }
        #endregion

        #region Public Class Properties

        internal static ErrorWindow Instance
        {
            get
            {
                if (null != ErrorWindow.errorWindow)
                    return ErrorWindow.errorWindow;

                ErrorWindow.errorWindow = new ErrorWindow();
                TextEditorControl.Instance.InsertWidget(EditorWidgetBar.Widget.Errors, errorWindow);
                return ErrorWindow.errorWindow;
            }
        }

        #endregion

        #region Private Class Methods

        private void OnErrorMenuCopy(object sender, RoutedEventArgs e)
        {
            MenuItem menuitem = e.Source as MenuItem;

            if (menuitem.Header.ToString() == "Copy")
            {
                CopyErrorMessage();
            }
        }

        private void OnErrorMenuClearAll(object sender, RoutedEventArgs e)
        {
            ClearError();
        }

        private void CopyErrorMessage()
        {
            string textToCopy = null;
            int itemsSelected = 0;

            for (int i = 0; i < lstErrorMessages.Items.Count; i++)
            {
                ListBoxItem item = ((ListBoxItem)lstErrorMessages.ItemContainerGenerator.ContainerFromIndex(i));
                if (item.IsSelected == true)
                {
                    OutputMessage message = ((OutputMessage)lstErrorMessages.Items.GetItemAt(i));
                    if (null != textToCopy)
                        textToCopy += '\n';
                    textToCopy += message.Message;
                    itemsSelected++;
                }
            }

            if (itemsSelected == 0 && lstErrorMessages.Items.Count > 0)
            {
                textToCopy = null;
                for (int i = 0; i < lstErrorMessages.Items.Count; i++)
                {
                    OutputMessage message = ((OutputMessage)lstErrorMessages.Items.GetItemAt(i));
                    textToCopy += '\n';
                    textToCopy += message.Message;
                }

            }

            if (textToCopy == null)
                return;
            else
                textToCopy.Replace("\n", "\r\n");

            try
            {
                Clipboard.SetData(DataFormats.Text, textToCopy);
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
        }

        #endregion

        #region Event Handler

        private void OnErrorWindowMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (System.Windows.Forms.Control.ModifierKeys == System.Windows.Forms.Keys.Control)
            {
                if (e.Delta > 0 && lstErrorMessages.FontSize < 24)
                    lstErrorMessages.FontSize = lstErrorMessages.FontSize + 2;
                else if (e.Delta < 0 && lstErrorMessages.FontSize > 12)
                    lstErrorMessages.FontSize = lstErrorMessages.FontSize - 2;

            }
            else
            {
                if (e.Delta > 0 && offset < errorWindow.Height)
                    ErrorScrollViewer.ScrollToVerticalOffset(offset);
                else if (e.Delta < 0 && offset > 0)
                    ErrorScrollViewer.ScrollToVerticalOffset(offset--);
            }
            e.Handled = true;
        }

        private void OnGridPreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (lstErrorMessages.IsMouseCaptureWithin == false)
            {
                for (int i = 0; i < lstErrorMessages.Items.Count; i++)
                {
                    ListBoxItem item = ((ListBoxItem)lstErrorMessages.ItemContainerGenerator.ContainerFromIndex(i));
                    item.IsSelected = false;
                }
            }
        }

        private void OnMessageClicked(object sender, MouseButtonEventArgs e)
        {
            OutputMessage message = (OutputMessage)lstErrorMessages.SelectedItem;
            if (message != null)
            {
                string activeScriptPath = Solution.Current.ActiveScript.GetParsedScript().GetScriptPath();
                if (!activeScriptPath.Equals(message.FilePath))
                {
                    if (TextEditorControl.Instance.TextCore.LoadScriptFromFile(message.FilePath))
                        TextEditorControl.Instance.SetupTabInternal(message.FilePath);
                    else
                    {
                        int index = Solution.Current.GetScriptIndexFromPath(message.FilePath);
                        if (index >= 0)
                            TextEditorControl.Instance.ScriptTabControl.ActivateTab(index);
                    }

                    TextEditorControl.Instance.HandleScriptActivation();
                }

                TextEditorControl.Instance.TextCore.SetCursorPosition(message.Column - 1, message.Line - 1);
                TextEditorControl.Instance.UpdateCaretPosition();
            }
        }

        #endregion
    }

    [ValueConversion(typeof(int), typeof(string))]
    public class OutputMessageLineConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // @TODO: This is weird, revise the logic!
            string lineNoText = "";
            if ((int)value < 0)
                return "" + lineNoText;
            else
                return "Line " + value.ToString() + " - ";
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
