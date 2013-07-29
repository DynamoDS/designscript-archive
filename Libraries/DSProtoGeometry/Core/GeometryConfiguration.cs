using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.ComponentModel;
using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    internal class ProtoGeometryConfigurationManager
    {
        static ProtoGeometryConfigurationManager()
        {
            string fullPath = ConfigFileFullPath;
            Settings = ProtoGeometryConfiguration.Deserialize(fullPath);

            if (!File.Exists(fullPath))
            {
                ProtoGeometryConfiguration.Serialize(fullPath, Settings);
            }
        }

        static string mSettingFile = "ProtoGeometry.config";

        static string ConfigFileFullPath
        {
            get
            {
                System.Reflection.Assembly executingAssembly = System.Reflection.Assembly.GetExecutingAssembly();
                Uri codeBaseUri = new Uri(executingAssembly.CodeBase);
                string fullPath = Path.Combine(Path.GetDirectoryName(codeBaseUri.LocalPath), mSettingFile);
                return File.Exists(fullPath) ? fullPath : DSApplication.Instance.Session.SearchFile(mSettingFile);
            }
        }

        public static void Save()
        {
            ProtoGeometryConfiguration.Serialize(ConfigFileFullPath, Settings);
        }

        public static ProtoGeometryConfiguration Settings { get; set; }
    }

    [Browsable(false)]
    public interface IProtoGeometryConfiguration
    {
        string GeometryFactoryFileName { get; set; }
        string PersistentManagerFileName { get; set; }
    }

    [Browsable(false)]
    public class ProtoGeometryConfiguration : IProtoGeometryConfiguration
    {
        internal static bool Serialize(string filePath, ProtoGeometryConfiguration configuration)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ProtoGeometryConfiguration));
                FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
                serializer.Serialize(fileStream, configuration);
                fileStream.Flush();
                fileStream.Close();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        internal static ProtoGeometryConfiguration Deserialize(string filePath)
        {
            ProtoGeometryConfiguration configuration = null;
            if (!string.IsNullOrEmpty(filePath) && (File.Exists(filePath) != false))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ProtoGeometryConfiguration));
                    FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    configuration = serializer.Deserialize(fileStream) as ProtoGeometryConfiguration;
                    fileStream.Close();
                }
                catch (Exception)
                {
                }
            }

            if (null == configuration) // Default settings.
                configuration = new ProtoGeometryConfiguration();

            return configuration;
        }

        public string GeometryFactoryFileName { get; set; }
        public string PersistentManagerFileName { get; set; }

        private ProtoGeometryConfiguration()
        {
            // Default geometry settings go here...
            this.GeometryFactoryFileName = "ProtoAcadGeometry.dll";
            this.PersistentManagerFileName = "ProtoAcadGeometry.dll";
        }
    }
}
