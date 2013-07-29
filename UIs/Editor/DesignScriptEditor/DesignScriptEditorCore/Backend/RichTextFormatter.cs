using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Documents;
using System.Windows;
using System.IO;

namespace DesignScript.Editor.Core
{
    class RichTextFormatter
    {
        #region Private Class Data Members

        ITextBuffer textBuffer = null;
        CodeFragmentManager codeFragmentManager = null;

        #endregion

        #region Public Class Operational Methods

        internal RichTextFormatter(ITextBuffer textBuffer, CodeFragmentManager codeFragmentManager)
        {
            this.textBuffer = textBuffer;
            this.codeFragmentManager = codeFragmentManager;
        }

        internal string Format(CharPosition start, CharPosition end)
        {
            Paragraph paragraph = new Paragraph();
            for (int lineIndex = start.CharacterY; lineIndex <= end.CharacterY; ++lineIndex)
            {
                string lineContent = textBuffer.GetLineContent(lineIndex);
                if (string.IsNullOrEmpty(lineContent))
                    continue;

                int charStart = 0, lineLength = lineContent.Length;
                if (lineIndex == start.CharacterY) // Starting line.
                    charStart = start.CharacterX;
                if (lineIndex == end.CharacterY) // Ending line.
                    lineLength = end.CharacterX;

                for (int charIndex = charStart; charIndex < lineLength; ++charIndex)
                {
                    CodeFragment fragment = null;
                    int fragmentWidth = codeFragmentManager.GetFragment(
                        charIndex, lineIndex, out fragment);

                    if (null == fragment)
                    {
                        // We have encountered a whitespace character (e.g. space, tab, etc), 
                        // scan forward until we get a non-whitespace character, then flush
                        // whatever we have found (whitespaces) into the paragraph at one go.
                        // 
                        int startIndex = charIndex;
                        while (charIndex < lineLength)
                        {
                            fragmentWidth = codeFragmentManager.GetFragment(
                                charIndex, lineIndex, out fragment);

                            if (fragmentWidth > 0 && (null != fragment))
                            {
                                string text = lineContent.Substring(startIndex, charIndex - startIndex);
                                FormatFragment(null, text, paragraph);
                                break;
                            }

                            charIndex = charIndex + 1;
                        }

                        if (charIndex >= lineLength)
                        {
                            // If there was no fragment at the time we reached the
                            // end of line, then flush out the remaining whitespaces,
                            // if there is any.
                            // 
                            if (null == fragment && (startIndex < charIndex))
                            {
                                FormatFragment(null, lineContent.Substring(startIndex,
                                    charIndex - startIndex), paragraph);
                            }

                            break; // Reach the end of line.
                        }
                    }

                    int offsetToNextChar = fragmentWidth - 1;
                    if ((charIndex + fragmentWidth) > (lineLength - 1))
                    {
                        fragmentWidth = (lineLength - 1) - charIndex;
                        offsetToNextChar = fragmentWidth - 1;
                    }

                    // Initialize the text store.
                    string textContent = string.Empty;
                    textContent = lineContent.Substring(charIndex,
                        ((fragmentWidth == 0) ? 1 : fragmentWidth));

                    FormatFragment(fragment, textContent, paragraph);
                    if (fragmentWidth > 0)
                        charIndex += offsetToNextChar;
                }
            }

            FlowDocument flowDocument = new FlowDocument();
            flowDocument.TextAlignment = TextAlignment.Left;
            flowDocument.FontSize = 11;
            flowDocument.FontFamily = new FontFamily("Consolas");
            flowDocument.FlowDirection = FlowDirection.LeftToRight;
            flowDocument.TextAlignment = TextAlignment.Left;
            flowDocument.Blocks.Add(paragraph);

            TextRange range = new TextRange(flowDocument.ContentStart, flowDocument.ContentEnd);
            if (range.CanSave(DataFormats.Rtf))
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    range.Save(stream, DataFormats.Rtf);
                    stream.Flush();
                    return (Encoding.UTF8.GetString(stream.ToArray()));
                }
            }

            return string.Empty;
        }

        internal static Color GetFragmentTypeColor(CodeFragment.Type code)
        {
            int index = ((int)code);
            if (index < 0 || (index >= CodeFragment.colorTable.Length))
                return Colors.Red;

            return CodeFragment.colorTable[index];
        }

        #endregion

        #region Private Class Helper Methods

        private void FormatFragment(CodeFragment fragment, string content, Paragraph paragraph)
        {
            Run textRun = new Run(content);
            Color textColor = Colors.Black;
            if (null != fragment)
                textColor = CodeFragment.GetFragmentColor(fragment.CodeType);

            textRun.Foreground = new SolidColorBrush(textColor);
            paragraph.Inlines.Add(textRun);

            if (content.Contains('\n'))
                paragraph.Inlines.Add(new LineBreak());
        }

        #endregion
    }
}
