using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DesignScript.Editor.Core
{
    public class NavigationParser : INavigationParser
    {
        public List<string> Tokenize(string content)
        {
            if (string.IsNullOrEmpty(content))
                return null;

            List<string> tokens = new List<string>();
            MemoryStream memstream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            Scanner scanner = new Scanner(memstream);

            while (true)
            {
                Token token = scanner.Peek();

                if (token.kind == 0)
                    break;

                if ((tokens.Count > 0) && (token.val == " "))
                {
                    if (string.IsNullOrWhiteSpace(tokens[tokens.Count - 1]))
                        tokens[tokens.Count - 1] += token.val;
                    else
                        tokens.Add(token.val);
                }
                else
                    tokens.Add(token.val);
            }

            return tokens;
        }
    }
}
