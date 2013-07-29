﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Net;
using DesignScript.Editor.Core;
using System.ComponentModel;
using System.Windows;

namespace DesignScript.Editor.Common
{
    public delegate void UpdateDownloadedEventHandler(object sender, UpdateDownloadedEventArgs e);
    public delegate void ShutdownRequestedEventHandler(object sender, EventArgs e);

    public class UpdateDownloadedEventArgs : EventArgs
    {
        public UpdateDownloadedEventArgs(Exception error, string fileLocation)
        {
            Error = error;
            UpdateFileLocation = fileLocation;
            UpdateAvailable = !string.IsNullOrEmpty(fileLocation);
        }

        public bool UpdateAvailable { get; private set; }
        public string UpdateFileLocation { get; private set; }
        public Exception Error { get; private set; }
    }

    /// <summary>
    /// This class provides services for product update management.
    /// </summary>
    public class UpdateManager
    {
        #region Private Class Data Members

        struct AppVersionInfo
        {
            public BinaryVersion Version;
            public string VersionInfoURL;
            public string InstallerURL;
        }

        private UpdateManager(ILoggerWrapper logger)
        {
            this.logger = logger;
        }

        private static UpdateManager self = null;
        private bool versionCheckInProgress = false;
        private BinaryVersion productVersion = null;
        private AppVersionInfo? updateInfo;
        private ILoggerWrapper logger = null;

        #endregion

        #region Public Event Handlers

        /// <summary>
        /// Occurs when RequestUpdateDownload operation completes.
        /// </summary>
        public event UpdateDownloadedEventHandler UpdateDownloaded;
        public event ShutdownRequestedEventHandler ShutdownRequested;

        #endregion

        #region Public Class Properties

        public static UpdateManager CreateInstance(ILoggerWrapper logger)
        {
            if (null != UpdateManager.self) // This method has already been called before.
                throw new InvalidOperationException("UpdateManager.CreateInstance called twice");

            if (null == logger)
                throw new ArgumentNullException("logger", "Unspecified logger (61578808A807)");

            UpdateManager.self = new UpdateManager(logger);
            return UpdateManager.self;
        }

        /// <summary>
        /// Obtains singleton object instance of UpdateManager class
        /// </summary>
        public static UpdateManager Instance
        {
            get { return UpdateManager.self; }
        }

        /// <summary>
        /// Obtains product version string
        /// </summary>
        public BinaryVersion ProductVersion
        {
            get
            {
                if (null == productVersion)
                {
                    string executingAssemblyPathName = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo(executingAssemblyPathName);
                    productVersion = BinaryVersion.FromString(myFileVersionInfo.FileVersion.ToString());
                }

                return productVersion;
            }
        }

        /// <summary>
        /// Obtains available update version string 
        /// </summary>
        public BinaryVersion AvailableVersion
        {
            get
            {
                if (!updateInfo.HasValue)
                {
                    CheckForProductUpdate();
                    return ProductVersion;
                }

                return updateInfo.Value.Version;
            }
        }

        /// <summary>
        /// Obtains downloaded update file location.
        /// </summary>
        public string UpdateFileLocation { get; private set; }

        #endregion

        #region Public Class Operational Methods

        /// <summary>
        /// Async call to request the update version info from the web. 
        /// This call raises UpdateFound event notification, if an update is
        /// found.
        /// </summary>
        public void CheckForProductUpdate()
        {
            if (false != versionCheckInProgress)
                return;

            logger.LogInfo("RequestUpdateVersionInfo", "RequestUpdateVersionInfo");

            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string appVersionFileName = Path.Combine(Path.GetDirectoryName(exePath), Configurations.AppVersionFileName);

            if (!File.Exists(appVersionFileName))
            {
                logger.LogError("RequestUpdateVersionInfo",
                    string.Format("'{0}' not found!", Configurations.AppVersionFileName));
                return;
            }

            string[] appVersionInfo = File.ReadAllLines(appVersionFileName);
            if (appVersionInfo.Length != 3)
            {
                logger.LogError("RequestUpdateVersionInfo",
                    string.Format("Invalid '{0}' format!", Configurations.AppVersionFileName));
                return;
            }

            versionCheckInProgress = true;
            WebClient client = new WebClient();
            client.OpenReadCompleted += new OpenReadCompletedEventHandler(OnUpdateVersionRequested);
            client.OpenReadAsync(new System.Uri(appVersionInfo[1]));
        }

        public void QuitAndInstallUpdate()
        {
            string message = string.Format("{0} needs to be closed for a smooth update.\n\n" +
                "Click OK to close {0}\nClick CANCEL to cancel the update", "DesignScript Application");

            MessageBoxResult result = MessageBox.Show(message, "Install DesignScript", MessageBoxButton.OKCancel);
            bool installUpdate = result == MessageBoxResult.OK;

            logger.LogInfo("UpdateManager-QuitAndInstallUpdate",
                (installUpdate ? "Install button clicked" : "Cancel button clicked"));

            if (false != installUpdate)
            {
                if (this.ShutdownRequested != null)
                    this.ShutdownRequested(this, new EventArgs());
            }
        }

        public void HostApplicationBeginQuit(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(UpdateFileLocation))
            {
                if (File.Exists(UpdateFileLocation))
                    Process.Start(UpdateFileLocation);
            }
        }

        #endregion

        #region Private Event Handlers

        private void OnUpdateVersionRequested(object sender, OpenReadCompletedEventArgs e)
        {
            if (null == e || e.Error != null)
            {
                string errorMessage = "Unspecified error";
                if (null != e && (null != e.Error))
                    errorMessage = e.Error.Message;

                logger.LogError("UpdateManager-OnUpdateVersionRequested",
                    string.Format("Request failure: {0}", errorMessage));

                versionCheckInProgress = false;
                return;
            }

            List<string> versionInfo = new List<string>();
            using (StreamReader streamReader = new StreamReader(e.Result))
            {
                string line;
                while (!string.IsNullOrEmpty(line = streamReader.ReadLine()))
                {
                    versionInfo.Add(line);
                }
            }

            if (versionInfo.Count != 3)
            {
                versionCheckInProgress = false;
                return;
            }

            updateInfo = new AppVersionInfo()
            {
                Version = BinaryVersion.FromString(versionInfo[0]),
                VersionInfoURL = versionInfo[1],
                InstallerURL = versionInfo[2]
            };

            logger.LogInfo("UpdateManager-OnUpdateVersionRequested",
                string.Format("Product Version: {0} Available Version : {1}",
                ProductVersion.ToString(), AvailableVersion.ToString()));

            if (updateInfo.Value.Version <= this.ProductVersion)
            {
                versionCheckInProgress = false;
                return; // Up-to-date, no download required.
            }

            DownloadUpdatePackage(updateInfo.Value.InstallerURL, updateInfo.Value.Version);
        }

        private void OnDownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            versionCheckInProgress = false;

            if (e == null)
                return;

            string errorMessage = ((null == e.Error) ? "Successful" : e.Error.Message);
            logger.LogInfo("UpdateManager-OnDownloadFileCompleted", errorMessage);

            UpdateFileLocation = string.Empty;
            if (e.Error == null)
                UpdateFileLocation = (string)e.UserState;

            if (null != UpdateDownloaded)
                UpdateDownloaded(this, new UpdateDownloadedEventArgs(e.Error, UpdateFileLocation));
        }

        #endregion

        #region Private Class Helper Methods

        /// <summary>
        /// Async call to request downloading a file from web.
        /// This call raises UpdateDownloaded event notification.
        /// </summary>
        /// <param name="url">Web URL for file to download.</param>
        /// <param name="version">The version of package that is to be downloaded.</param>
        /// <returns>Request status, it may return false if invalid URL was passed.</returns>
        private bool DownloadUpdatePackage(string url, BinaryVersion version)
        {
            if (string.IsNullOrEmpty(url) || (null == version))
            {
                versionCheckInProgress = false;
                return false;
            }

            UpdateFileLocation = string.Empty;
            string downloadedFileName = string.Empty;
            string downloadedFilePath = string.Empty;

            try
            {
                downloadedFileName = Path.GetFileNameWithoutExtension(url);
                downloadedFileName += "." + version.ToString() + Path.GetExtension(url);
                downloadedFilePath = Path.Combine(Path.GetTempPath(), downloadedFileName);

                if (File.Exists(downloadedFilePath))
                    File.Delete(downloadedFilePath);
            }
            catch (Exception)
            {
                versionCheckInProgress = false;
                return false;
            }

            WebClient client = new WebClient();
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(OnDownloadFileCompleted);
            client.DownloadFileAsync(new Uri(url), downloadedFilePath, downloadedFilePath);
            return true;
        }

        #endregion
    }
}
