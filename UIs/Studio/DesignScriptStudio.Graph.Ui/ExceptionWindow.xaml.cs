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
    /// Interaction logic for ExceptionWindow.xaml
    /// </summary>
    public partial class ExceptionWindow : Window
    {
        public ExceptionWindow(Exception exception)
        {
            InitializeComponent();
            this.CallStackInfo.Text = exception.Message + "\n\n" + exception.StackTrace;

            this.CloseButton.Click += new RoutedEventHandler(OnCloseButtonClick);
            this.AddHandler(FrameworkElement.KeyUpEvent, new KeyEventHandler(OnExceptionWindowKeyUp), true);
        }

        void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        void OnExceptionWindowKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
        }
    }
}
