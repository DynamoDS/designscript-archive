using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DesignScriptStudio.Graph.Core
{
    class BinaryStorage : IStorage
    {
        #region Class Data Members

        protected MemoryStream stream = null;
        private BinaryReader reader = null;
        private BinaryWriter writer = null;

        #endregion

        #region Public Interface Methods

        public long GetPosition()
        {
            return this.stream.Position;
        }

        public void Seek(long offSet, SeekOrigin from)
        {
            this.stream.Seek(offSet, from);
        }

        #region Read/Write byte[]

        public byte[] ReadBytes(ulong expectedSignature)
        {
            ulong actualSignature = this.reader.ReadUInt64();
            if (actualSignature != expectedSignature)
            {
                string errorMessage = this.FormErrorMessage(expectedSignature, actualSignature);
                throw new InvalidDataException(errorMessage);
            }
            int bytesLength = this.reader.ReadInt32();
            return this.reader.ReadBytes(bytesLength);
        }

        public byte[] ReadBytes(ulong expectedSignature, byte[] defaultValue)
        {
            ulong actualSignature = this.reader.ReadUInt64();
            if (actualSignature != expectedSignature)
            {
                this.stream.Seek(-8, SeekOrigin.Current);
                return defaultValue;
            }
            int bytesLength = this.reader.ReadInt32();
            return this.reader.ReadBytes(bytesLength);
        }

        public void WriteBytes(ulong signature, byte[] value)
        {
            this.writer.Write(signature);
            this.writer.Write(value.Length);
            this.writer.Write(value);
            this.writer.Flush();
        }

        #endregion

        #region Read/Write int

        public int ReadInteger(ulong expectedSignature)
        {
            ulong actualSignature = this.reader.ReadUInt64();
            if (actualSignature != expectedSignature)
            {
                string errorMessage = this.FormErrorMessage(expectedSignature, actualSignature);
                throw new InvalidDataException(errorMessage);
            }
            return this.reader.ReadInt32();
        }

        public int ReadInteger(ulong expectedSignature, int defaultValue)
        {
            ulong actualSignature = this.reader.ReadUInt64();
            if (actualSignature != expectedSignature)
            {
                this.stream.Seek(-8, SeekOrigin.Current);
                return defaultValue;
            }
            return this.reader.ReadInt32();
        }

        public void WriteInteger(ulong signature, int value)
        {
            this.writer.Write(signature);
            this.writer.Write(value);
            this.writer.Flush();
        }

        #endregion

        #region Read/Write uint

        public uint ReadUnsignedInteger(ulong expectedSignature)
        {
            ulong actualSignature = this.reader.ReadUInt64();
            if (actualSignature != expectedSignature)
            {
                string errorMessage = this.FormErrorMessage(expectedSignature, actualSignature);
                throw new InvalidDataException(errorMessage);
            }
            return this.reader.ReadUInt32();
        }

        public uint ReadUnsignedInteger(ulong expectedSignature, uint defaultValue)
        {
            ulong actualSignature = this.reader.ReadUInt64();
            if (actualSignature != expectedSignature)
            {
                this.stream.Seek(-8, SeekOrigin.Current);
                return defaultValue;
            }
            return this.reader.ReadUInt32();
        }

        public void WriteUnsignedInteger(ulong signature, uint value)
        {
            this.writer.Write(signature);
            this.writer.Write(value);
            this.writer.Flush();
        }

        #endregion

        #region Read/Write long

        public long ReadLong(ulong expectedSignature)
        {
            ulong actualSignature = this.reader.ReadUInt64();
            if (actualSignature != expectedSignature)
            {
                string errorMessage = this.FormErrorMessage(expectedSignature, actualSignature);
                throw new InvalidDataException(errorMessage);
            }
            return this.reader.ReadInt64();
        }

        public long ReadLong(ulong expectedSignature, long defaultValue)
        {
            ulong actualSignature = this.reader.ReadUInt64();
            if (actualSignature != expectedSignature)
            {
                this.stream.Seek(-8, SeekOrigin.Current);
                return defaultValue;
            }
            return this.reader.ReadInt64();
        }

        public void WriteLong(ulong signature, long value)
        {
            this.writer.Write(signature);
            this.writer.Write(value);
            this.writer.Flush();
        }

        public ulong PeekUnsignedLong()
        {
            ulong value = this.reader.ReadUInt64();
            this.stream.Seek(-8, SeekOrigin.Current);
            return value;
        }

        #endregion

        #region Read/Write ulong

        public ulong ReadUnsignedLong(ulong expectedSignature)
        {
            ulong actualSignature = this.reader.ReadUInt64();
            if (actualSignature != expectedSignature)
            {
                string errorMessage = this.FormErrorMessage(expectedSignature, actualSignature);
                throw new InvalidDataException(errorMessage);
            }
            return this.reader.ReadUInt64();
        }

        public ulong ReadUnsignedLong(ulong expectedSignature, ulong defaultValue)
        {
            ulong actualSignature = this.reader.ReadUInt64();
            if (actualSignature != expectedSignature)
            {
                this.stream.Seek(-8, SeekOrigin.Current);
                return defaultValue;
            }
            return this.reader.ReadUInt64();
        }

        public void WriteUnsignedLong(ulong signature, ulong value)
        {
            this.writer.Write(signature);
            this.writer.Write(value);
            this.writer.Flush();
        }

        #endregion

        #region Read/Write boolean

        public bool ReadBoolean(ulong expectedSignature)
        {
            ulong actualSignature = this.reader.ReadUInt64();
            if (actualSignature != expectedSignature)
            {
                string errorMessage = this.FormErrorMessage(expectedSignature, actualSignature);
                throw new InvalidDataException(errorMessage);
            }
            return this.reader.ReadBoolean();
        }

        public bool ReadBoolean(ulong expectedSignature, bool defaultValue)
        {
            ulong actualSignature = this.reader.ReadUInt64();
            if (actualSignature != expectedSignature)
            {
                this.stream.Seek(-8, SeekOrigin.Current);
                return defaultValue;
            }
            return this.reader.ReadBoolean();
        }

        public void WriteBoolean(ulong signature, bool value)
        {
            this.writer.Write(signature);
            this.writer.Write(value);
            this.writer.Flush();
        }

        #endregion

        #region Read/Write double

        public double ReadDouble(ulong expectedSignature)
        {
            ulong actualSignature = this.reader.ReadUInt64();
            if (actualSignature != expectedSignature)
            {
                string errorMessage = this.FormErrorMessage(expectedSignature, actualSignature);
                throw new InvalidDataException(errorMessage);
            }
            return this.reader.ReadDouble();
        }

        public double ReadDouble(ulong expectedSignature, double defaultValue)
        {
            ulong actualSignature = this.reader.ReadUInt64();
            if (actualSignature != expectedSignature)
            {
                this.stream.Seek(-8, SeekOrigin.Current);
                return defaultValue;
            }
            return this.reader.ReadDouble();
        }

        public void WriteDouble(ulong signature, double value)
        {
            this.writer.Write(signature);
            this.writer.Write(value);
            this.writer.Flush();
        }

        #endregion

        #region Read/Write string

        public string ReadString(ulong expectedSignature)
        {
            ulong actualSignature = this.reader.ReadUInt64();
            if (actualSignature != expectedSignature)
            {
                string errorMessage = this.FormErrorMessage(expectedSignature, actualSignature);
                throw new InvalidDataException(errorMessage);
            }
            return this.reader.ReadString();
        }

        public string ReadString(ulong expectedSignature, string defaultValue)
        {
            ulong actualSignature = this.reader.ReadUInt64();
            if (actualSignature != expectedSignature)
            {
                this.stream.Seek(-8, SeekOrigin.Current);
                return defaultValue;
            }
            return this.reader.ReadString();
        }

        public void WriteString(ulong signature, string value)
        {
            this.writer.Write(signature);
            this.writer.Write(value);
            this.writer.Flush();
        }

        #endregion

        #endregion

        #region Public Class Methods

        public BinaryStorage()
        {
            this.stream = new MemoryStream();
            this.reader = new BinaryReader(this.stream);
            this.writer = new BinaryWriter(this.stream);
        }

        #endregion

        #region Private Class Methods

        private string FormErrorMessage(ulong expected, ulong actual)
        {
            return string.Format(UiStrings.SignatureMismatchFmt,
                UlongToString(expected), UlongToString(actual));
        }

        private string UlongToString(ulong value)
        {
            StringBuilder builder = new StringBuilder();
            for (int index = 0; index < sizeof(ulong); index++)
            {
                if (index > 0)
                    builder.Append(", ");

                char c = ((char)(value & 0x00000000000000ff));
                c = ((c == 0x00) ? ' ' : c);
                value = value >> 8;
                builder.Append(string.Format("'{0}'", c));
            }

            return builder.ToString();
        }

        #endregion
    }
}
