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

namespace DesignScriptStudio.Graph.Ui
{
    /// <summary>
    /// Interaction logic for ZoomAndPanControl.xaml
    /// </summary>
    public partial class ZoomAndPanControl : UserControl
    {
        GraphControl graphControl = null;

        public ZoomAndPanControl(GraphControl graphControl)
        {
            InitializeComponent();
            this.graphControl = graphControl;
        }

        #region Private Class Event Handlers

        private void OnZoomToFitClick(object sender, RoutedEventArgs e)
        {
            if (graphControl.CurrentGraphCanvas == null)
                return;
            graphControl.SetCanvasFocus();
            graphControl.CurrentGraphCanvas.OnZoomToFitClick();
        }

        private void OnZoomInClick(object sender, RoutedEventArgs e)
        {
            if (graphControl.CurrentGraphCanvas == null)
                return;
            graphControl.SetCanvasFocus();
            graphControl.CurrentGraphCanvas.OnZoomInClick();
        }

        private void OnZoomOutClick(object sender, RoutedEventArgs e)
        {
            if (graphControl.CurrentGraphCanvas == null)
                return;
            graphControl.SetCanvasFocus();
            graphControl.CurrentGraphCanvas.OnZoomOutClick();
        }

        private void OnPanClick(object sender, RoutedEventArgs e)
        {
            if (graphControl.CurrentGraphCanvas == null)
                return;
            graphControl.SetCanvasFocus();
            graphControl.CurrentGraphCanvas.OnPanClick();
        }
        
        #endregion

    }
}
