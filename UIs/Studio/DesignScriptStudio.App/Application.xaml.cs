using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Autodesk.DesignScript.Interfaces;
using System.Xml.Serialization;
using System.IO;
using DesignScriptStudio.Graph.Core;

namespace DesignScriptStudio.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class HostApplication : Application, IGraphEditorHostApplication
    {
        #region Private Class Data Members

        Dictionary<string, object> configs = new Dictionary<string, object>();

        #endregion

        #region IGraphEditorHostApplication interfaces

        public Dictionary<string, object> Configurations
        {
            get { return configs; }
        }

        public void GraphActivated(uint graphId)
        {
        }

        public void PostGraphUpdate()
        {
        }

        #endregion

        #region Public Class Properties

        internal bool IsInRecordingMode { get; private set; }

        #endregion

        #region Class Overridable Methods/Event Handlers

        protected override void OnStartup(StartupEventArgs e)
        {
            IsInRecordingMode = false;

            base.OnStartup(e);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(OnUnhandledException);
            LoadConfigurationFromFile();
            ProcessStartupArguments(e.Args);

            IsInRecordingMode = true;
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            (this.MainWindow as MainWindow).DisplayException(e.ExceptionObject as Exception);
            AttemptToRelaunchApplication(ClassFactory.CurrCoreComponent.SessionName);
        }

        #endregion

        #region Private Class Helper Methods

        private void LoadConfigurationFromFile()
        {
            Configuration appConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (null != appConfig && (null != appConfig.AppSettings))
            {
                KeyValueConfigurationCollection elements = appConfig.AppSettings.Settings;
                if (null != elements)
                {
                    foreach (KeyValueConfigurationElement element in elements)
                        configs.Add(element.Key, element.Value);
                }
            }
        }

        private void ProcessStartupArguments(string[] arguments)
        {
            bool geometricPreviewEnabled = true;

            foreach (string argument in arguments)
            {
                if (String.IsNullOrEmpty(argument))
                    continue;

                if (argument[0] == '/')
                {
                    string name = argument.Substring(1).ToLower();
                    if (string.IsNullOrEmpty(name))
                        continue;

                    switch (name)
                    {
                        case "r":
                            IsInRecordingMode = true;
                            break;

                        case "nogeometrypreview":
                            geometricPreviewEnabled = false;
                            break;
                    }

                    // When an instance of DesignScript Studio crashes, its session name 
                    // will be passed to a new instance of the application through command 
                    // line argument. See "GraphController.GetBackupFileNameFormat" for 
                    // more details.
                    // 
                    if (name.StartsWith("sn:"))
                    {
                        string sessionName = name.Substring(3);
                        configs.Add(CoreStrings.SessionNameKey, sessionName);
                    }
                }
            }

            // Determine if the geometric preview should be enabled on the UI.
            configs.Add(ConfigurationKeys.GeometricPreviewEnabled, geometricPreviewEnabled);
        }

        private void AttemptToRelaunchApplication(string sessionName)
        {
            string exePath = System.Reflection.Assembly.GetEntryAssembly().Location;
            System.Diagnostics.Process.Start(exePath, string.Format("/sn:{0}", sessionName));
        }

        #endregion
    }
}
