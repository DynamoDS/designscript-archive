using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DesignScriptStudio.Graph.Core
{
    class GraphProperties : ISerializable
    {
        private enum Version { Version0, Current = Version0 }

        #region Class Data Members

        private GraphProperties.Version version;

        #endregion

        #region Public Interface Methods

        public bool Deserialize(IStorage storage)
        {
            if (storage == null)
                throw new ArgumentNullException("storage");

            if (storage.ReadUnsignedInteger(FieldCode.GraphPropertiesSignature) != Configurations.GraphPropertiesSignature)
                throw new InvalidOperationException("Invalid input data");

            int value = storage.ReadInteger(FieldCode.GraphPropertiesVersion);
            string appVersion = storage.ReadString(FieldCode.ApplicationVersion, "0.1.0.0");

            GraphProperties.Version loadedVersion = ((GraphProperties.Version)value);
            if (loadedVersion > GraphProperties.Version.Current)
                throw new FileVersionException(appVersion);

            // There are three scenarios when it comes to the value of "appVersion":
            // 
            //   1. It was not stored in the file, in which case it is an ancient file.
            //   2. It was stored, but different from the current DLL version.
            //   3. It was stored, and it is the same as the current DLL version.
            // 
            // In any of the above cases, there's no need to set "ApplicationVersion"
            // locally since appVersion's sole purpose is to be displayed when the 
            // "FileVersionException" is thrown (for use on the dialog). When we 
            // store "GraphProperties", it is always written with the current DLL 
            // version (set in the constructor of "GraphProperties" object).
            // 
            // this.ApplicationVersion = appVersion;

            this.version = loadedVersion;
            this.AuthorName = storage.ReadString(FieldCode.AuthorName);
            this.CompanyName = storage.ReadString(FieldCode.CompanyName);
            this.ImportedScripts.Clear();

            // We optionally store the number of imported scripts in the BIN file.
            int importedSciptsCount = storage.ReadInteger(FieldCode.ImportedScriptsCount, 0);
            for (int i = 0; i < importedSciptsCount; i++)
                this.ImportedScripts.Add(storage.ReadString(FieldCode.ImportedScript));

            this.RuntimeStates.Deserialize(storage);
            return true;
        }

        public bool Serialize(IStorage storage)
        {
            if (storage == null)
                throw new ArgumentNullException("storage");

            try
            {
                storage.WriteUnsignedInteger(FieldCode.GraphPropertiesSignature, Configurations.GraphPropertiesSignature);
                storage.WriteInteger(FieldCode.GraphPropertiesVersion, (int)(GraphProperties.Version.Current));
                storage.WriteString(FieldCode.ApplicationVersion, this.ApplicationVersion);
                storage.WriteString(FieldCode.AuthorName, this.AuthorName);
                storage.WriteString(FieldCode.CompanyName, this.CompanyName);

                storage.WriteInteger(FieldCode.ImportedScriptsCount, this.ImportedScripts.Count);
                foreach (string importedScript in this.ImportedScripts)
                    storage.WriteString(FieldCode.ImportedScript, importedScript);

                this.RuntimeStates.Serialize(storage);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n GraphProperties serialization failed.");
                return false;
            }
        }

        public AuditStatus Audit()
        {
            return AuditStatus.NoChange;
        }

        #endregion

        #region Internal Class Properties

        internal string AuthorName { get; set; }
        internal string CompanyName { get; set; }
        internal string ApplicationVersion { get; private set; }
        internal List<string> ImportedScripts { get; set; }
        internal RuntimeStates RuntimeStates { get; private set; }

        #endregion

        #region Internal Class Methods

        internal GraphProperties()
        {
            // Get the file version of the current assembly (i.e. DesignScriptStudio.Graph.Core.dll).
            string assemblyPathName = Assembly.GetAssembly(typeof(GraphProperties)).Location;
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assemblyPathName);
            this.ApplicationVersion = fileVersionInfo.FileVersion;

            this.version = GraphProperties.Version.Current;
            this.AuthorName = string.Empty;
            this.CompanyName = string.Empty;
            this.RuntimeStates = new RuntimeStates();
            this.ImportedScripts = new List<string>();
        }

        #endregion
    }
}
