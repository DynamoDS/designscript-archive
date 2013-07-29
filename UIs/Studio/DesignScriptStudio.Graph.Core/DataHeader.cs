using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DesignScriptStudio.Graph.Core
{
    class DataHeader : ISerializable
    {
        private enum Version { Version0, Current = Version0 }

        #region Class Data Members

        private DataHeader.Version version;

        #endregion

        #region Public Interface Methods

        public bool Deserialize(IStorage storage)
        {
            if (storage == null)
                throw new ArgumentNullException("storage");

            if (storage.ReadUnsignedInteger(FieldCode.DataHeaderSignature) != Configurations.DataHeaderSignature)
                throw new InvalidOperationException("Invalid input data");

            try
            {
                this.version = (DataHeader.Version)storage.ReadInteger(FieldCode.DataHeaderVersion);
                this.HeaderSize = storage.ReadLong(FieldCode.HeaderSize);
                this.DataSize = storage.ReadLong(FieldCode.DataSize);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n Data header deserialization failed.");
                return false;
            }
        }

        public bool Serialize(IStorage storage)
        {
            if (storage == null)
                throw new ArgumentNullException("storage");

            try
            {
                long before = storage.GetPosition();
                storage.WriteUnsignedInteger(FieldCode.DataHeaderSignature, Configurations.DataHeaderSignature);
                storage.WriteInteger(FieldCode.DataHeaderVersion, (int)DataHeader.Version.Current);
                storage.WriteLong(FieldCode.HeaderSize, this.HeaderSize);
                storage.WriteLong(FieldCode.DataSize, this.DataSize);
                long after = storage.GetPosition();

                // Ensuring the header size agrees with what we say it is.
                System.Diagnostics.Debug.Assert((after - before) == this.HeaderSize);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n Data header serialization failed.");
                return false;
            }
        }

        public AuditStatus Audit()
        {
            return AuditStatus.NoChange;
        }

        #endregion

        #region Internal Class Properties

        internal long DataSize { get; set; }

        internal long HeaderSize { get; private set; }

        #endregion

        #region Internal Class Methods

        internal DataHeader()
        {
            this.version = DataHeader.Version.Current;
            this.DataSize = 0;
            this.HeaderSize = Configurations.DataHeaderSize;
        }

        #endregion
    }
}
