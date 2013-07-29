using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using ProtoCore;
using DesignScript.Parser.Associative;

namespace DesignScript.Parser
{
    public class ParsedScript : IParsedScript
    {
        string fullScriptPath = "";
        CodeBlockNode codeBlockNode = null;
        ProtoCore.Core core = new ProtoCore.Core(new ProtoCore.Options());
        string parserErrors;

        #region IParsedScript Members

        public bool ParseScript(string scriptFilePath)
        {
            try
            {
                this.fullScriptPath = scriptFilePath;
                DesignScript.Parser.Scanner scanner = new DesignScript.Parser.Scanner(scriptFilePath);
                DesignScript.Parser.Parser parser = new DesignScript.Parser.Parser(scanner, core);
                parser.Parse();
                parserErrors = parser.errors.errMsgFormat;
                codeBlockNode = parser.root;
            }
            catch (Exception)
            {
                return false;
            }

            return (null != codeBlockNode);
        }

        public bool ParseStream(Stream scriptStream)
        {
            try
            {
                Scanner scanner = new Scanner(scriptStream);
                Parser parser = new Parser(scanner, core);
                parser.Parse();
                parserErrors = parser.errors.errMsgFormat;
                codeBlockNode = parser.root;
                return (null != codeBlockNode);
            }
            catch (Exception exception)
            {
                return false;
            }
        }

        public string GetErrorMessage()
        {
            return parserErrors;
        }

        public void DestroyScript()
        {
            // @TODO: Other forms of clean-up may be required.
            codeBlockNode = null;
        }

        public void CopyParsedResults(IParsedScript other)
        {
            if (null == other)
                return;

            this.codeBlockNode = other.GetParsedResults();
        }

        public CodeBlockNode GetParsedResults()
        {
            return codeBlockNode;
        }

        public string GetScriptPath()
        {
            return this.fullScriptPath;
        }

        #endregion
    }
}
