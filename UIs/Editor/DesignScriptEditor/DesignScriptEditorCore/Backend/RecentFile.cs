using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace DesignScript.Editor
{
    public class RecentFile
    {
        public System.DateTime date;
        public string FilePath { get; set; }
        public string FileName { get; set; }

        public RecentFile()
        {
        }

        public RecentFile(string filePath)
        {
            this.FilePath = filePath;
            this.FileName = Path.GetFileName(filePath);
            this.date = DateTime.Now;
        }

        public static bool operator ==(RecentFile a, RecentFile b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the file paths match:
            return a.FilePath == b.FilePath;
        }

        public static bool operator !=(RecentFile a, RecentFile b)
        {
            return !(a == b);
        }

        public bool Equals(RecentFile a)
        {
            // Return true if the fields match:
            return base.Equals(a);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static void Serialize(List<RecentFile> recentFiles)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<RecentFile>));
                string filePath = GetSettingsFilePath();
                TextWriter textWriter = new StreamWriter(filePath);
                serializer.Serialize(textWriter, recentFiles);
                textWriter.Close();
            }
            catch (Exception)
            {
                return;
            }
        }

        public static List<RecentFile> Deserialize()
        {
            List<RecentFile> recentFiles = new List<RecentFile>();
            XmlSerializer serializer = new XmlSerializer(typeof(List<RecentFile>));
            string filePath = GetSettingsFilePath();
            if (File.Exists(filePath))
            {
                try
                {
                    FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    recentFiles = serializer.Deserialize(fileStream) as List<RecentFile>;
                }
                catch (Exception)
                {
                    return recentFiles;
                }
            }

            return recentFiles;
        }

        private static string GetSettingsFilePath()
        {
            try
            {
                string appDataFolder = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.ApplicationData);

                if (!appDataFolder.EndsWith("\\"))
                    appDataFolder += "\\";

                appDataFolder += @"Autodesk\DesignScript Studio\";
                if (Directory.Exists(appDataFolder) == false)
                    Directory.CreateDirectory(appDataFolder);
                appDataFolder += "RecentFiles.xml";

                return appDataFolder;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

    }
}
