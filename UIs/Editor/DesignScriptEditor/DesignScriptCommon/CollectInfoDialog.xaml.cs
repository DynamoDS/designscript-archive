using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Automation.Peers;
using System.Windows.Automation;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace DesignScript.Editor.Common
{
    /// <summary>
    /// Interaction logic for CollectInfoDialog.xaml
    /// </summary>
    public partial class CollectInfoDialog : Window
    {
        public CollectInfoDialog()
        {
            InitializeComponent();
            this.CollectFeedback = false;
        }

        private void OnContinueClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void OnWindowClose(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.CollectFeedback = false;
            if (acceptCheck.IsChecked.HasValue)
                this.CollectFeedback = acceptCheck.IsChecked.Value;
        }

        public bool CollectFeedback { get; private set; }
    }
}