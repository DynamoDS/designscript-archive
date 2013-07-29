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
    public partial class ReadOnlyPrompt : Window
    {
        public ReadOnlyDialogResult userOption = ReadOnlyDialogResult.Cancel;

        public ReadOnlyPrompt(bool allowOverWriteOption)
        {
            InitializeComponent();
            InitializeReadOnlyPromptWindow();
            if (!allowOverWriteOption)
            {
                OverwriteButton.Visibility = Visibility.Collapsed;
                InfoBlock.Text = "You can save the file in a different location.";
            }
            else
            {
                OverwriteButton.Visibility = Visibility.Visible;
                InfoBlock.Text = "You can either save the file in a different location or DesignScript Studio can attempt to remove the write-protection and overwrite the file in its current location.";
            }

            SaveAsButton.Click += new RoutedEventHandler(OnSaveAs);
            OverwriteButton.Click += new RoutedEventHandler(OnOverwrite);
            CancelButton.Click += new RoutedEventHandler(OnCancel);
        }

        public void InitializeReadOnlyPromptWindow()
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
            userOption = ReadOnlyDialogResult.SaveAs;
            this.Close();
        }

        public void OnOverwrite(object sender, RoutedEventArgs e)
        {
            userOption = ReadOnlyDialogResult.OverWrite;
            this.Close();
        }

        public void OnCancel(object sender, RoutedEventArgs e)
        {
            userOption = ReadOnlyDialogResult.Cancel;
            this.Close();
        }
    }
}
