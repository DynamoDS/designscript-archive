using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace DesignScript.Editor.StartUp
{
    using System.ComponentModel;
    using System.Windows;
    using System.Reflection;
    using System.Windows.Controls;
    using System.Windows.Markup;
    using DesignScript.Editor.Core;
    using System.Windows.Shapes;
    using System.Windows.Media;

    class StartUpWorker : BackgroundWorker
    {
        private bool startUpScreenFinalized = false;
        private StartUpData startUpData = null;
        internal CurvyTabControl StartUpTabControl = null;

        internal void InitializeStartUpWorker()
        {
            this.DoWork += new DoWorkEventHandler(OnstartUpWorkerDoWork);
        }

        internal bool FinalizeStartUpScreen()
        {
            if (false != startUpScreenFinalized)
                return true; // Already done that.

            startUpScreenFinalized = true; // Mark as being done.

            StartUpScreen startUpScreen = new StartUpScreen();
            startUpScreen.Name = "StartUpScreen";
            Grid.SetRow(startUpScreen, 3);
            Grid.SetRowSpan(startUpScreen, 4);
            Grid.SetZIndex(startUpScreen, 2000);
            TextEditorControl.Instance.grid.Children.Add(startUpScreen);
            TextEditorControl.Instance.StartUpButton.Visibility = Visibility.Collapsed;

            StartUpTabControl = new CurvyTabControl();
            StartUpTabControl.Name = "StartUpTabControl";
            StartUpTabControl.Height = 28;
            StartUpTabControl.Width = 120;
            Grid.SetColumn(StartUpTabControl, 1);
            StartUpTabControl.HorizontalAlignment = HorizontalAlignment.Right;
            StartUpTabControl.VerticalAlignment = VerticalAlignment.Bottom;

            Rectangle rectangle = new Rectangle();
            rectangle.Name = "StartUpRectangle";
            rectangle.VerticalAlignment = VerticalAlignment.Bottom;
            rectangle.HorizontalAlignment = HorizontalAlignment.Stretch;
            rectangle.Height = 2;
            Brush curvtTabColour = new SolidColorBrush(Color.FromRgb(224, 224, 224));
            rectangle.Fill = curvtTabColour;
            rectangle.Width = TextEditorControl.Instance.textCanvas.Width;
            Grid.SetColumn(rectangle, 0);
            Grid.SetColumnSpan(rectangle, 2);
            TextEditorControl.Instance.InnerGrid.Children.Add(rectangle);

            TextEditorControl.Instance.InnerGrid.Children.Add(StartUpTabControl);
            StartUpTabControl.ShowCloseButton = true;
            StartUpTabControl.InsertNewTab("StartUp", startUpScreen);
            StartUpTabControl.RegisterTabClosingCallback(StartUpTabClosingCallback);
            StartUpTabControl.TabChanged += new TabChangeHandler(OnTabControlTabChanged);
            StartUpTabControl.ActivateTab(0);

            return true;
        }

        #region Public Class Properties

        internal StartUpData Data { get { return this.startUpData; } }

        #endregion

        private void OnstartUpWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            //System.Threading.Thread.Sleep(4000);
            System.Diagnostics.Debug.Assert(null == startUpData);
            List<RecentFile> recentFiles = RecentFile.Deserialize();
            startUpData = new StartUpData(recentFiles);
            TextEditorControl.Instance.TextCore.Data = startUpData;
        }

        private bool StartUpTabClosingCallback()
        {
            TextEditorControl.Instance.CheckTabControlVisibility(false, false);
            return false; // return false to prevent closing.
        }

        private void OnTabControlTabChanged(object sender, TabChangedEventArgs e)
        {
            if ((sender is CurvyTabControl) == false)
                return;

            TextEditorControl.Instance.CheckTabControlVisibility(true, true);
        }
    }
}
