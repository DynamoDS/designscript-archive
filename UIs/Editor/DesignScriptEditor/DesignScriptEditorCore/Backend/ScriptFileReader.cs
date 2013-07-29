using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DesignScript.Editor.Core
{
    class ScriptFileReader
    {
        TextReader textReader = null;
        List<string> internalLines = new List<string>();

        public ScriptFileReader(string fileNameOrContent, bool isFilePath)
        {
            if (isFilePath != false)
                this.textReader = new StreamReader(fileNameOrContent);
            else
                this.textReader = new StringReader(fileNameOrContent);
        }

        public List<string> ReadInput()
        {
            try
            {
                TextReader sr = this.textReader;

                StringBuilder sb = new StringBuilder();

                while (sr.Peek() >= 0)
                {
                    bool foundLineBreak = false;
                    char character = (char)sr.Read();

                    if (character == '\r')
                    {
                        char nextChar = (char)sr.Peek();
                        if (nextChar == '\n') // Windows EOL format.
                            nextChar = (char)sr.Read(); // Remove that '\n'.

                        foundLineBreak = true;
                    }
                    else if (character == '\n')
                        foundLineBreak = true;

                    if (false != foundLineBreak)
                    {
                        sb.Append('\n');
                        internalLines.Add sb.ToString());
                        sb.Clear();
                    }
                    else
                    {
                        // Simply append the character and move on.
                        sb.Append(character);
                    }
                }

                if  sb.Length > 0)
                {
                    // Something remained in the buffer.
                    internalLines.Add sb.ToString());
                }
            }
            catch (Exception e)
            {
                System.Console.WriteLine("The file could not be read:");
                System.Console.WriteLine(e);
            }
            finally
            {
                if (null != textReader && (textReader is StreamReader))
                {
                    textReader.Close();
                    textReader = null;
                }
            }

            return internalLines;
        }
    }
}
