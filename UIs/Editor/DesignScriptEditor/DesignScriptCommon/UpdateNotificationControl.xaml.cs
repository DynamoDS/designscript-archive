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

namespace DesignScript.Editor.Common
{
    using DesignScript.Editor.Core;
    /// <summary>
    /// Interaction logic for UpdateNoificationControl.xaml
    /// </summary>
    public partial class UpdateNotificationControl : UserControl
    {
        ILoggerWrapper logger;

        public UpdateNotificationControl(ILoggerWrapper logger)
        {
            InitializeComponent();
            this.logger = logger;
            this.InstallButton.Click += new RoutedEventHandler(OnInstallButtonClicked);
        }

        private void OnInstallButtonClicked(object sender, RoutedEventArgs e)
        {
            logger.LogInfo("UpdateNotificationControl-OnInstallButtonClicked",
                "UpdateNotificationControl-OnInstallButtonClicked");

            UpdateManager.Instance.QuitAndInstallUpdate(); // Quit application
        }
    }
}