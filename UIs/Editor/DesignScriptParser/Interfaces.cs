using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

namespace DesignScript.Parser
{
    using DesignScript.Parser.Associative;

    public interface IParsedScript
    {
        bool ParseScript(string scriptFilePath);
        bool ParseStream(Stream scriptStream);
        string GetErrorMessage();

        void DestroyScript();
        string GetScriptPath();
        void CopyParsedResults(IParsedScript other);
        CodeBlockNode GetParsedResults();
    }

    public class InterfaceFactory
    {
        public static IParsedScript CreateParsedScript()
        {
            return new ParsedScript();
        }
    }
}
