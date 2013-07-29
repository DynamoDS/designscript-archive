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
using DesignScript.Editor;

namespace DesignScript.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        HostApplication hostApplication = null;
        TextEditorControl textEditorControl = null;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(OnMainWindowLoaded);
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            if (this != Keyboard.FocusedElement)
            {
                base.OnGotKeyboardFocus(e);
                return;
            }

            // If there is no other child element getting focused as the focus 
            // returned to the main window, then we need to explicitly set the 
            // focus back onto the text canvas (or the start-up page, whichever
            // is currently on top).
            if (null != textEditorControl)
                textEditorControl.SetFocusOnForegroundContent();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (null != textEditorControl)
            {
                if (false != textEditorControl.EnsureScriptsSaved())
                    base.OnClosing(e);
                else
                    e.Cancel = true;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (null != textEditorControl)
            {
                textEditorControl.ReleaseThreads();
                textEditorControl.Shutdown();
            }
        }

        private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            hostApplication = new HostApplication(App.arguments);
            textEditorControl = new TextEditorControl(hostApplication);
            EditorGrid.Children.Add(textEditorControl);
        }
    }
}
