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
using DesignScript.Editor.Core;

namespace DesignScript.Editor
{
    /// <summary>
    /// Interaction logic for OutputWindow.xaml
    /// </summary>
    public partial class OutputWindow : UserControl
    {
        private static OutputWindow outputWindow = null;
        List<ProtoCore.OutputMessage> messages = new List<OutputMessage>();
        int offset;

        #region Public Class Operational Methods

        public static void ClearOutput()
        {
            if (null != OutputWindow.outputWindow)
            {
                Solution.Current.GetMessage(true);
                OutputWindow.outputWindow.ClearOutputWindow();
            }
        }

        public OutputWindow()
        {
            InitializeComponent();
            offset = 0;
            lstOutputMessages.MouseWheel += new MouseWheelEventHandler(this.OnMouseWheel);
        }

        internal void SetOutputMessage(List<ProtoCore.OutputMessage> messages)
        {
            if (this.messages != null)
                this.messages.Clear();

            foreach (ProtoCore.OutputMessage message in messages)
            {
                if (message.Type != OutputMessage.MessageType.Info)
                    continue;

                if (null == this.messages)
                    this.messages = new List<ProtoCore.OutputMessage>();
                this.messages.Add(message);
            }

            lstOutputMessages.ItemsSource = this.messages;
            lstOutputMessages.Items.Refresh();
            if (lstOutputMessages.Items.Count > 0)
                lstOutputMessages.ScrollIntoView(lstOutputMessages.Items[lstOutputMessages.Items.Count - 1]);

            lstOutputMessages.UpdateLayout();
        }

        internal List<ProtoCore.OutputMessage> GetOutputMessage()
        {
            return this.messages;
        }

        internal void ClearOutputWindow()
        {
            if (null != messages)
            {
                messages.Clear();
                lstOutputMessages.Items.Refresh();
            }
        }

        #endregion

        #region Public Class Properties

        internal static OutputWindow Instance
        {
            get
            {
                if (null != OutputWindow.outputWindow)
                    return OutputWindow.outputWindow;

                OutputWindow.outputWindow = new OutputWindow();
                TextEditorControl.Instance.InsertWidget(EditorWidgetBar.Widget.Output, outputWindow);
                return OutputWindow.outputWindow;
            }
        }

        #endregion

        #region Private Class Methods

        private void OnOutputMenuCopy(object sender, RoutedEventArgs e)
        {
            MenuItem menuitem = e.Source as MenuItem;

            if (menuitem.Header.ToString() == "Copy")
            {
                CopyOutputMessage();
            }
        }

        private void OnOutputMenuClearAll(object sender, RoutedEventArgs e)
        {
            ClearOutput();
        }

        private void CopyOutputMessage()
        {
            string textToCopy = null;
            int itemsSelected = 0;

            for (int i = 0; i < lstOutputMessages.Items.Count; i++)
            {
                ListBoxItem item = ((ListBoxItem)lstOutputMessages.ItemContainerGenerator.ContainerFromIndex(i));
                if (item.IsSelected == true)
                {
                    OutputMessage message = ((OutputMessage)lstOutputMessages.Items.GetItemAt(i));
                    if (null != textToCopy)
                        textToCopy += '\n';
                    textToCopy += message.Message;
                    itemsSelected++;
                }
            }

            if (itemsSelected == 0 && lstOutputMessages.Items.Count > 0)
            {
                textToCopy = null;
                for (int i = 0; i < lstOutputMessages.Items.Count; i++)
                {
                    OutputMessage message = ((OutputMessage)lstOutputMessages.Items.GetItemAt(i));
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

        private void OnGridLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (lstOutputMessages.IsMouseCaptureWithin == false)
            {
                for (int i = 0; i < lstOutputMessages.Items.Count; i++)
                {
                    ListBoxItem item = ((ListBoxItem)lstOutputMessages.ItemContainerGenerator.ContainerFromIndex(i));
                    item.IsSelected = false;
                }
            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (System.Windows.Forms.Control.ModifierKeys == System.Windows.Forms.Keys.Control)
            {
                if (e.Delta > 0 && lstOutputMessages.FontSize < 24)
                {
                    lstOutputMessages.FontSize = lstOutputMessages.FontSize + 1;
                    ITextEditorSettings settings = TextEditorControl.Instance.TextCore.TextEditorSettings;
                    settings.FontMultiplier = settings.FontMultiplier + 1;
                }
                else if (e.Delta < 0 && lstOutputMessages.FontSize > 12)
                {
                    lstOutputMessages.FontSize = lstOutputMessages.FontSize - 1;
                    ITextEditorSettings settings = TextEditorControl.Instance.TextCore.TextEditorSettings;
                    settings.FontMultiplier = settings.FontMultiplier + 1;
                }

                e.Handled = true;
            }
            else
            {
                if (e.Delta > 0 && offset < outputWindow.Height)
                    OutputScrollViewer.ScrollToVerticalOffset(offset);
                else if (e.Delta < 0 && offset > 0)
                    OutputScrollViewer.ScrollToVerticalOffset(offset--);
            }
        }

        #endregion
    }

    public class AttachedProperties
    {
        #region HideExpanderArrow AttachedProperty

        [AttachedPropertyBrowsableForType(typeof(Expander))]
        public static bool GetHideExpanderArrow(DependencyObject obj)
        {
            return (bool)obj.GetValue(HideExpanderArrowProperty);
        }

        [AttachedPropertyBrowsableForType(typeof(Expander))]
        public static void SetHideExpanderArrow(DependencyObject obj, bool value)
        {
            obj.SetValue(HideExpanderArrowProperty, value);
        }

        // Using a DependencyProperty as the backing store for HideExpanderArrow.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HideExpanderArrowProperty =
                DependencyProperty.RegisterAttached("HideExpanderArrow", typeof(bool), typeof(AttachedProperties), new UIPropertyMetadata(false, OnHideExpanderArrowChanged));

        private static void OnHideExpanderArrowChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            Expander expander = (Expander)o;

            if (expander.IsLoaded)
            {
                UpdateExpanderArrow(expander, (bool)e.NewValue);
            }
            else
            {
                expander.Loaded += new RoutedEventHandler((x, y) => UpdateExpanderArrow(expander, (bool)e.NewValue));
            }
        }

        private static void UpdateExpanderArrow(Expander expander, bool visible)
        {
            Grid headerGrid =
                    VisualTreeHelper.GetChild(
                            VisualTreeHelper.GetChild(
                                            VisualTreeHelper.GetChild(
                                                    VisualTreeHelper.GetChild(
                                                            VisualTreeHelper.GetChild(
                                                                    expander,
                                                                    0),
                                                            0),
                                                    0),
                                            0),
                                    0) as Grid;

            headerGrid.Children[0].Visibility = visible ? Visibility.Collapsed : Visibility.Visible; // Hide or show the Ellipse
            headerGrid.Children[1].Visibility = visible ? Visibility.Collapsed : Visibility.Visible; // Hide or show the Arrow
            headerGrid.Children[2].SetValue(Grid.ColumnProperty, visible ? 0 : 1); // If the Arrow is not visible, then shift the Header Content to the first column.
            headerGrid.Children[2].SetValue(Grid.ColumnSpanProperty, visible ? 2 : 1); // If the Arrow is not visible, then set the Header Content to span both rows.
            headerGrid.Children[2].SetValue(ContentPresenter.MarginProperty, visible ? new Thickness(0) : new Thickness(4, 0, 0, 0)); // If the Arrow is not visible, then remove the margin from the Content.
        }

        #endregion
    }

    [ValueConversion(typeof(ProtoCore.OutputMessage.MessageType), typeof(string))]
    public class OutputMessageTypeToIconFilenameConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((ProtoCore.OutputMessage.MessageType)value)
            {
                case ProtoCore.OutputMessage.MessageType.Error:
                    return Images.TabErrorIcon;
                case ProtoCore.OutputMessage.MessageType.Info:
                    return Images.OutputMessage;
                case ProtoCore.OutputMessage.MessageType.Warning:
                    return Images.TabWarningIcon;
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
