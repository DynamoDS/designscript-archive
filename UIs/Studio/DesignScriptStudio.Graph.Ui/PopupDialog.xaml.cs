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
using System.Windows.Shapes;

namespace DesignScriptStudio.Graph.Ui
{
    /// <summary>
    /// Interaction logic for PopupDialog.xaml
    /// </summary>
    public partial class PopupDialog : Window
    {
        public PopupDialog(string icon, string message, string button0, string button1, string button2)
        {
            InitializeComponent();

            this.DialogImage.Source = new BitmapImage(new Uri(icon));
            this.DialogMessage.Text = message;

            if (button0 != string.Empty)
            {
                this.Button0.Content = button0;
                this.Button0.Visibility = Visibility.Visible;
            }
            if (button1 != string.Empty)
            {
                this.Button1.Content = button1;
                this.Button1.Visibility = Visibility.Visible;
            }
            if (button2 != string.Empty)
            {
                this.Button2.Content = button2;
                this.Button2.Visibility = Visibility.Visible;
            }

            if (null != Application.Current)
                this.Owner = Application.Current.MainWindow;
            this.ShowDialog();
        }

        private void OnWindowClose(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnWindowKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape || e.Key == Key.Enter)
                Close();
        }
    }
}
