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
using DesignScript.Editor.Core;
using DesignScript.Parser;
using System.IO;

namespace DesignScript.Editor
{
    /// <summary>
    /// Interaction logic for ReadOnlyPrompt.xaml
    /// </summary>
    public partial class SampleFilePrompt : Window
    {
        public SampleFilePromptResult userOption = SampleFilePromptResult.Cancel;

        public SampleFilePrompt()
        {
            InitializeComponent();
            InitializSampleFilePromptWindow();
            SaveAsButton.Click += new RoutedEventHandler(OnSaveAs);
            CancelButton.Click += new RoutedEventHandler(OnCancel);
        }

        public void InitializSampleFilePromptWindow()
        {
            this.AddHandler(FrameworkElement.KeyUpEvent, new KeyEventHandler(OnPromptWindowKeyUp), true);
        }

        public void OnPromptWindowKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
        }

        public void OnSaveAs(object sender, RoutedEventArgs e)
        {
            userOption = SampleFilePromptResult.SaveAs;
            this.Close();
        }

        public void OnCancel(object sender, RoutedEventArgs e)
        {
            userOption = SampleFilePromptResult.Cancel;
            this.Close();
        }
    }
}
