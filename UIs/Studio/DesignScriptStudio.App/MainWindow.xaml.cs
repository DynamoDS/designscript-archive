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
using DesignScriptStudio.Graph.Core;
using DesignScriptStudio.Graph.Ui;
using Autodesk.DesignScript.Interfaces;

namespace DesignScriptStudio.App
{
    public partial class MainWindow : Window
    {
        GraphControl graphControl = null;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(OnMainWindowLoaded);
            this.Closing += OnMainWindowClosing;
            this.StateChanged += new EventHandler(OnMainWindowStateChanged);
        }

        internal void DisplayException(Exception exception)
        {
            graphControl.DisplayException(exception);
        }

        internal IGraphController CurrentGraphController
        {
            get
            {
                if (null == graphControl)
                    return null;

                return graphControl.GetController(uint.MaxValue);
            }
        }

        private void OnMainWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != graphControl)
            {
                if (!graphControl.Shutdown())
                    e.Cancel = true;
            }
        }

        private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            HostApplication app = Application.Current as HostApplication;
            graphControl = new GraphControl(app.IsInRecordingMode, app as IGraphEditorHostApplication);
            grid.Children.Add(graphControl);
        }

        private void OnMainWindowStateChanged(object sender, EventArgs e)
        {
            if (((Window)sender).WindowState == WindowState.Maximized)
                this.WindowGrid.Margin = new Thickness(8);
            else
                this.WindowGrid.Margin = new Thickness(0);
        }
    }
}
