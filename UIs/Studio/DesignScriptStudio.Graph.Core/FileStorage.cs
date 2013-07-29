using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DesignScriptStudio.Graph.Core
{
    class FileStorage : BinaryStorage
    {
        #region Internal Class Methods

        internal void Load(string filePath)
        {
            FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            file.CopyTo(this.stream);
            file.Close();
            this.Seek(0, SeekOrigin.Begin); //Reset stream position to the beginning where the reading should start
        }

        internal void Save(string filePath)
        {
            this.Seek(0, SeekOrigin.Begin); //Reset stream position to the beginning where the writing should start
            FileStream file = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            this.stream.CopyTo(file);
            file.Flush();
            file.Close();
        }

        #endregion
    }
}
