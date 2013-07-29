using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace DesignScriptStudio.Graph.Core
{
    // Made public for serialization.
    public class PersistentSettings
    {
        #region Public Class Operational Methods

        internal static bool Serialize(string filePath, PersistentSettings settingsData)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(PersistentSettings));
                FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                serializer.Serialize(fileStream, settingsData);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        internal static PersistentSettings Deserialize(string filePath)
        {
            PersistentSettings settingsInstance = null;
            if (!string.IsNullOrEmpty(filePath) && (File.Exists(filePath) != false))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(PersistentSettings));
                    FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    settingsInstance = serializer.Deserialize(fileStream) as PersistentSettings;
                }
                catch (Exception)
                {
                }
            }

            if (null == settingsInstance) // Default settings.
                settingsInstance = new PersistentSettings();

            return settingsInstance;
        }

        #endregion

        #region PersistentSettings Members

        public List<string> LoadedAssemblies { get; set; }
        public bool SuppressPreview { get; set; }
        public bool DontShowSplash { get; set; }

        #endregion

        #region Private Class Helper Methods

        private PersistentSettings()
        {
            this.LoadedAssemblies = new List<string>();
            this.SuppressPreview = false;
            this.DontShowSplash = false;
        }

        #endregion
    }
}
