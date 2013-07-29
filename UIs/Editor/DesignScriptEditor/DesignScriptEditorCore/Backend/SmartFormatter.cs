using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;

namespace DesignScript.Editor.Core
{
    public class SmartFormatter
    {
        #region Private Regular Expressions and Patterns

        private Regex formatTriggerRule = null;
        private Regex negativeSignRule = null;
        private Regex negativeUnpaddingRule = null;
        private Regex modifierStackRule = null;
        private Regex exponentNumberRule = null;
        private Regex replicationGuideRule = null;
        private Regex bracketsUnpaddingRule = null;
        private Regex loneOperatorRule = null;
        private Regex pairOperatorRule = null;
        private Regex logicalOperatorRule = null;
        private Regex bothEndsUnpaddingRule = null;
        private Regex bothEndsPaddingRule = null;
        private Regex forwardPaddingRule = null;
        private Regex forwardOnlyPaddingRule = null;
        private Regex backwardPaddingRule = null;
        private Regex backwardOnlyPaddingRule = null;
        private Regex pairSymbolPaddingRule = null;
        private Regex ifStatementPaddingRule = null;
        private Regex spaceNormalizationRule = null;
        private Regex spaceTrimmingRule = null;
        private Regex indentationRule = null;
        private Regex scopeOpeningRule = null;
        private Regex scopeClosureRule = null;
        private Regex blockCommentRule = null;
        private Regex lineClosureRule = null;
        private List<string> lineList = null;

        #endregion

        private static SmartFormatter smartFormatter = null;

        #region Public Operational Methods

        public static SmartFormatter Instance
        {
            get
            {
                if (null == smartFormatter)
                    smartFormatter = new SmartFormatter();

                return smartFormatter;
            }
        }

        internal static bool ContainsFormattingTrigger(string textContent)
        {
            return SmartFormatter.Instance.ContainsTrigger(textContent);
        }

        public bool Format(List<string> lineList)
        {
            if (null == lineList || (lineList.Count <= 0))
                return false;

            return Format(lineList, 0, lineList.Count - 1);
        }

        public bool Format(List<string> lineList, int from, int to)
        {
            this.OriginalContent = string.Empty;
            this.FormattedOutput = string.Empty;

            if (null == lineList || (lineList.Count <= 0) || (from > to))
                return false;
            if (from < 0 || (from >= lineList.Count))
                return false;
            if (to < 0 || (to > lineList.Count))
                return false;

            // This case will handle hitting <Enter> on the very last line, 
            // in which case we create an empty line at the end of the list.
            if (to == lineList.Count)
                lineList.Add(string.Empty);

            this.lineList = lineList;
            CurrentIndentation = 0;

            if (from > 0) // Lines other than first.
            {
                int previousLine = (from > 0 ? from - 1 : from);
                CurrentIndentation = GetLineIndentation(previousLine);
            }

            StringBuilder original = new StringBuilder();
            StringBuilder builder = new StringBuilder();
            for (int index = from; index <= to; ++index)
            {
                original.Append(lineList[index]);
                builder.Append(FormatLine(index));

                // If we're on the last formatted line...
                if ((from < to) && (index == to))
                {
                    // If the line before the last formatted line was not closed 
                    // (for example, in an "if" statement), then the indentation 
                    // can be adjusted to add a tab worth of spaces. This value 
                    // will be used by caller to determine where the final cursor
                    // is to be positioned.
                    // 
                    if (PreviousLineWasClosed(to) == false)
                    {
                        int tabSize = Configurations.TabSpaces.Length;
                        CurrentIndentation = CurrentIndentation + tabSize;
                    }
                }
            }

            this.OriginalContent = original.ToString();
            this.FormattedOutput = builder.ToString();

            // The formatter indicates "failure" if original string has not been changed.
            return (!string.Equals(OriginalContent, FormattedOutput, StringComparison.Ordinal));
        }

        #endregion

        #region Public/Internal Class Properties

        public string OriginalContent { get; private set; }
        public string FormattedOutput { get; private set; }

        internal static int CurrentIndentation { get; private set; }
        internal TextEditorCore AlternateEditorCore { get; set; }

        #endregion

        #region Private Class Helper Methods

        private SmartFormatter()
        {
            formatTriggerRule = new Regex(@"[\n\{\}\];]$");
            negativeSignRule = new Regex(@"([\?:<>=\[(,])\s*([\-])\s*([a-zA-Z0-9]+)");
            negativeUnpaddingRule = new Regex(@"([\[(])\s*([\-])\s*([a-zA-Z0-9]+)");
            modifierStackRule = new Regex(@"=\s+>");
            exponentNumberRule = new Regex(@"[-+]?[0-9]+\.[0-9]+[eE][\s]*[-+][\s]*[0-9]+");
            replicationGuideRule = new Regex(@"(\s*<[_\.\w\s]+>)");
            bracketsUnpaddingRule = new Regex(@"(\s*\([^\S\n]*)|(\s*\)\s*)|(\s*\[[^\S\n]*)|(\s*\]\s*)");
            loneOperatorRule = new Regex(@"\s*([\?:<>&\|=\+\-\*/])[^\S\n]*");
            pairOperatorRule = new Regex(@"([!<>&=\|\+\-\*/])\s*(=)");
            logicalOperatorRule = new Regex(@"([&|\|])\s+(\1)");
            bothEndsUnpaddingRule = new Regex(@"\s*([;!\(\)\[\]])[^\S\n]*");
            bothEndsPaddingRule = new Regex(@"([\W]*)(\belse\b)([\W]*)");
            forwardPaddingRule = new Regex(@"(\s*)([\{])[^\S\n]*");
            forwardOnlyPaddingRule = new Regex(@"(\s*)([,;])[^\S\n]*");
            backwardPaddingRule = new Regex(@"\s*([\}])([^\S\n]*)");
            backwardOnlyPaddingRule = new Regex(@"\s*([!])([^\S\n]*)");
            pairSymbolPaddingRule = new Regex(@"(\))\s*(\{)");
            ifStatementPaddingRule = new Regex(@"(\bif\b)\s*(\()");
            spaceNormalizationRule = new Regex(@"([^\S\n]+)");
            spaceTrimmingRule = new Regex(@"^[ \t]+|[ \t]+$");
            indentationRule = new Regex(@"^[^\S\n]*");
            scopeOpeningRule = new Regex(@"[\{]\s*$");
            scopeClosureRule = new Regex(@"^\s*[\}]");
            blockCommentRule = new Regex(@"(/\s*\*)|(\*\s*/)");
            lineClosureRule = new Regex(@"[};][^\S\n]*$|\[Imperative\]\s*$|\[Associative\]\s*$");
        }

        private bool ContainsTrigger(string textContent)
        {
            Match match = formatTriggerRule.Match(textContent);
            return (null != match && (match.Length > 0));
        }

        private string FormatLine(int lineIndex)
        {
            string content = lineList[lineIndex];
            TextEditorCore textCore = AlternateEditorCore;
            textCore = (null != textCore ? textCore : TextEditorCore.Instance);
            if (null != textCore && (textCore.IsLineCommented(lineIndex, true)))
                return content;

            string comment = string.Empty;
            int commentIndex = content.IndexOf("//");
            if (commentIndex != -1)
            {
                // Strip off comment momentarily.
                comment = content.Substring(commentIndex);
                content = content.Substring(0, commentIndex);
            }

            string result = string.Empty;
            bool isInStringSegment = false;

            int index = 0;
            int quoteOffset = ScanForNextQuote(content, 0);
            while (index < content.Length)
            {
                if (quoteOffset < 0)
                    quoteOffset = content.Length;

                if (false == isInStringSegment)
                {
                    // If we're parsing a regular code segment (i.e. not a 
                    // string literal), then hand it down for formatting.
                    string segment = content.Substring(index, quoteOffset - index);
                    result += FormatSegment(segment);
                }
                else
                {
                    // If we're currently in a string segment, then 
                    // move ahead to include the closing quote character.
                    quoteOffset = quoteOffset + 1;

                    // This is true when the string ends with an open bracket.
                    if (quoteOffset > content.Length)
                        quoteOffset = content.Length;

                    string segment = content.Substring(index, quoteOffset - index);
                    result += segment;
                }

                index = quoteOffset;
                quoteOffset = ScanForNextQuote(content, quoteOffset + 1);
                isInStringSegment = !isInStringSegment;
            }

            // Final excessive space trimming rules.
            result = spaceNormalizationRule.Replace(result, " ");
            result = spaceTrimmingRule.Replace(result, "");

            // Restore comment towards the end of the string (if any).
            if (!string.IsNullOrEmpty(result) && (!string.IsNullOrEmpty(comment)))
                result = result + " "; // We have both content and comment, add space!

            content = result + comment;
            return PostFormatProcessing(lineIndex, content);
        }

        private string FormatSegment(string segment)
        {
            segment = bothEndsUnpaddingRule.Replace(segment, "$1");
            segment = bothEndsPaddingRule.Replace(segment, "$1 $2 $3");
            segment = loneOperatorRule.Replace(segment, " $1 ");
            segment = pairOperatorRule.Replace(segment, "$1$2");
            segment = logicalOperatorRule.Replace(segment, "$1$2");
            segment = forwardPaddingRule.Replace(segment, "$1$2 ");
            segment = forwardOnlyPaddingRule.Replace(segment, "$2 ");
            segment = backwardPaddingRule.Replace(segment, " $1$2");
            // segment = backwardOnlyPaddingRule.Replace(segment, " $1");
            segment = pairSymbolPaddingRule.Replace(segment, "$1 $2");
            segment = ifStatementPaddingRule.Replace(segment, "$1 $2");
            segment = negativeSignRule.Replace(segment, "$1 $2$3");
            segment = negativeUnpaddingRule.Replace(segment, "$1$2$3");
            segment = modifierStackRule.Replace(segment, "=>");
            segment = replicationGuideRule.Replace(segment, EvaluateReplicationGuideMatch);
            segment = bracketsUnpaddingRule.Replace(segment, EvaluateBracketsMatch);
            segment = blockCommentRule.Replace(segment, EvaluateBlockCommentMatch);
            segment = exponentNumberRule.Replace(segment, EvaluateExponentNumberMatch);

            return segment;
        }

        private string PostFormatProcessing(int lineIndex, string content)
        {
            int indentation = CurrentIndentation;
            int tabSize = Configurations.TabSpaces.Length;

            bool scopeBoundary = false;

            // First check for closure (order here is important).
            if (scopeClosureRule.IsMatch(content))
            {
                scopeBoundary = true;
                if (CurrentIndentation >= tabSize)
                {
                    CurrentIndentation = CurrentIndentation - tabSize;
                    indentation = CurrentIndentation;
                }
            }

            // Second check for opening (order here is important).
            if (scopeOpeningRule.IsMatch(content))
            {
                scopeBoundary = true;
                CurrentIndentation = CurrentIndentation + tabSize;
            }

            if (false == scopeBoundary)
            {
                if (PreviousLineWasClosed(lineIndex) == false)
                    indentation = indentation + tabSize;
            }

            if (0 != indentation)
            {
                string indent = new string((char)' ', indentation);
                content = indentationRule.Replace(content, indent);
            }

            return content;
        }

        private int GetLineIndentation(int lineIndex)
        {
            int unpairedBracket = 1;
            int tabSize = Configurations.TabSpaces.Length;

            while (lineIndex >= 0)
            {
                string content = lineList[lineIndex--];
                if (string.IsNullOrWhiteSpace(content))
                    continue; // Search for non-empty lines.

                bool scopeBoundary = false;

                // Indentation is to be defined at scope level.
                if (scopeOpeningRule.IsMatch(content) != false)
                {
                    unpairedBracket = unpairedBracket - 1;
                    scopeBoundary = true;
                }
                if (scopeClosureRule.IsMatch(content) != false)
                {
                    unpairedBracket = unpairedBracket + 1;
                    scopeBoundary = true;
                }

                // If we are not at a scope boundary, then proceed to theck the 
                // previous line. If we are at a scope boundary, see if we have 
                // the open-bracket that we're looking for. With unpairedBracket 
                // being 0, we have managed to find the open-bracket that is 
                // defining the scope for the input "lineIndex".
                // 
                if (false == scopeBoundary || (unpairedBracket > 0))
                    continue;

                int firstNonWhiteSpace = 0;
                while (firstNonWhiteSpace < content.Length)
                {
                    // If a scope opening line is of "x" indentation,
                    // then the scope it encloses must be of "tabSize"
                    // bigger than it is.
                    char character = content[firstNonWhiteSpace];
                    if (char.IsWhiteSpace(character) == false)
                        return firstNonWhiteSpace + tabSize;

                    int size = (character == '\t' ? tabSize : 1);
                    firstNonWhiteSpace = firstNonWhiteSpace + size;
                }
            }

            return 0;
        }

        private bool PreviousLineWasClosed(int lineIndex)
        {
            TextEditorCore textCore = AlternateEditorCore;
            textCore = (null != textCore ? textCore : TextEditorCore.Instance);

            lineIndex = lineIndex - 1;
            while (lineIndex >= 0)
            {
                string lineContent = lineList[lineIndex];
                if (null == textCore)
                    lineIndex = lineIndex - 1;
                else
                {
                    if (textCore.IsLineCommented(lineIndex--, false))
                        continue; // Ignore commented line.
                }

                // Strip off the trailing comment if there's any.
                int commentIndex = lineContent.IndexOf("//");
                if (commentIndex != -1)
                    lineContent = lineContent.Substring(0, commentIndex);

                if (string.IsNullOrWhiteSpace(lineContent))
                    continue; // Skipping past all blank lines.

                // If the line is closed, then return it.
                if (lineClosureRule.IsMatch(lineContent))
                    return true;

                // If the line wasn't closed but it was an open 
                // bracket, then treat it as being closed.
                return (scopeOpeningRule.IsMatch(lineContent));
            }

            return true;
        }

        private int ScanForNextQuote(string input, int offset)
        {
            if (string.IsNullOrEmpty(input))
                return -1;
            if (offset < 0 || (offset >= input.Length))
                return -1;

            for (; offset < input.Length; ++offset)
            {
                bool isQuoteCharacter = false;
                if (input[offset] == '"')
                {
                    // We found a quote here, but we only want a 
                    // quote that is not preceeded by a backslash.
                    isQuoteCharacter = true;
                    if (offset > 0 && (input[offset - 1] == '\\'))
                        isQuoteCharacter = false;
                }

                if (false != isQuoteCharacter)
                    return offset;
            }

            return -1;
        }

        private string EvaluateReplicationGuideMatch(Match match)
        {
            if (null == match)
                return string.Empty;

            string intermediate = match.ToString();
            return intermediate.Replace(" ", "");
        }

        private string EvaluateBracketsMatch(Match match)
        {
            if (null == match)
                return string.Empty;

            string intermediate = match.ToString();
            if (string.IsNullOrEmpty(intermediate))
                return string.Empty;

            int length = intermediate.Length;
            int index = intermediate.IndexOfAny(new char[] { '(', '[' });
            if (-1 != index) // We have a match for open brackets.
            {
                // Remove all spaces after open brackets.
                return intermediate.Substring(0, index + 1);
            }

            index = intermediate.IndexOfAny(new char[] { ')', ']' });
            if (-1 != index)
            {
                // Remove all spaces before the closing bracket.
                return intermediate.Substring(index, length - index);
            }

            return intermediate;
        }

        private string EvaluateBlockCommentMatch(Match match)
        {
            if (null == match)
                return string.Empty;

            string matchString = match.ToString();
            if (string.IsNullOrEmpty(matchString))
                return string.Empty;

            switch (matchString[0])
            {
                case '/': return "/*";
                case '*': return "*/";
            }

            return string.Empty;
        }

        private string EvaluateExponentNumberMatch(Match match)
        {
            if (null == match)
                return string.Empty;

            string intermediate = match.ToString();
            return intermediate.Replace(" ", "");
        }

        #endregion
    }
}
