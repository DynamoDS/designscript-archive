using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DesignScript.Editor.Core
{
    public class CharPosition
    {
        private System.Windows.Point screenPosition;
        private System.Drawing.Point characterPosition;
        private ITextBuffer textBuffer;

        #region Public Class Operational Methods

        internal CharPosition(ITextBuffer tb)
        {
            textBuffer = tb;
        }

        public System.Drawing.Point GetCharacterPosition()
        {
            return characterPosition;
        }

        public System.Windows.Point GetScreenPosition()
        {
            return screenPosition;
        }

        public void SetCharacterPosition(System.Drawing.Point pt, bool includeLineBreak = false)
        {
            SetCharacterPosition(pt.X, pt.Y, includeLineBreak);
        }

        public void SetCharacterPosition(int column, int line, bool includeLineBreak = false)
        {
            ValidateCharacterPos(column, line, includeLineBreak);
            CalculateScreenPos();
        }

        public void SetScreenPosition(System.Windows.Point pt)
        {
            SetScreenPosition(pt.X, pt.Y);
        }

        public void SetScreenPosition(double x, double y)
        {
            ValidateScreenPos(x, y);
            CalculateCharacterPos();
        }

        public override string ToString()
        {
            return characterPosition.ToString();
        }

        public int CharToVisualOffset(int line, int offset)
        {
            if (offset < 0)
                return 0;

            string lineContent = textBuffer.GetLineContent(line);
            if (string.IsNullOrEmpty(lineContent))
                return 0;

            int visualOffset = 0;
            int tabSize = Configurations.TabSpaces.Length;
            for (int i = 0; i < lineContent.Length; i++)
            {
                if (i == offset)
                    break;

                int increment = ((lineContent[i] == '\t') ? tabSize : 1);
                visualOffset = visualOffset + increment;
            }

            return visualOffset;
        }

        public int VisualToCharOffset(int line, int offset)
        {
            if (offset < 0)
                return 0;

            string lineContent = textBuffer.GetLineContent(line);
            if (string.IsNullOrEmpty(lineContent))
                return 0;

            int counter = 0, index;
            for (index = 0; index < lineContent.Length; index++)
            {
                if (counter >= offset)
                    return index;

                switch (lineContent[index])
                {
                    case '\t': counter = counter + 4; break;
                    case '\n': counter = counter + 0; break;
                    default: counter = counter + 1; break;
                }
            }

            return index;
        }

        internal int ToNonSpaceCharCount()
        {
            int line = this.characterPosition.Y;
            string lineContent = textBuffer.GetLineContent(line);
            if (string.IsNullOrEmpty(lineContent))
                return 0;

            int firstCharacter = 0;
            while (firstCharacter < lineContent.Length)
            {
                // Skipping past heading white space characters.
                if (!char.IsWhiteSpace(lineContent[firstCharacter]))
                    break;

                firstCharacter = firstCharacter + 1;
            }

            int nonWhiteSpaceChars = 0;
            for (int index = firstCharacter; index < lineContent.Length; ++index)
            {
                if (index >= this.characterPosition.X)
                    break;
                if (char.IsWhiteSpace(lineContent[index]) == false)
                    nonWhiteSpaceChars++;
            }

            return nonWhiteSpaceChars;
        }

        internal void FromNonSpaceCharCount(int charOffset)
        {
            int line = this.characterPosition.Y;
            string lineContent = textBuffer.GetLineContent(line);
            if (string.IsNullOrEmpty(lineContent))
                return;

            int firstCharacter = 0;
            while (firstCharacter < lineContent.Length)
            {
                // Skipping past heading white space characters.
                if (!char.IsWhiteSpace(lineContent[firstCharacter]))
                    break;

                firstCharacter = firstCharacter + 1;
            }

            int nonWhiteSpaceChars = 0;
            int characterX = firstCharacter;
            for (; characterX < lineContent.Length; ++characterX)
            {
                if (nonWhiteSpaceChars >= charOffset)
                    break;
                if (char.IsWhiteSpace(lineContent[characterX]) == false)
                    nonWhiteSpaceChars++;
            }

            // Adjust horizontal offset while remaining on the same line.
            this.SetCharacterPosition(characterX, line);
        }

        #endregion

        #region Public Class Properties

        public int CharacterX { get { return characterPosition.X; } }
        public int CharacterY { get { return characterPosition.Y; } }
        public double ScreenX { get { return screenPosition.X; } }
        public double ScreenY { get { return screenPosition.Y; } }

        #endregion

        #region Private Class Helper Methods

        void ValidateCharacterPos(int column, int line, bool includeLineBreak)
        {
            characterPosition.X = column;
            characterPosition.Y = line;
            int totalLines = textBuffer.GetLineCount();
            if (totalLines <= 0)
            {
                characterPosition.X = 0;
                characterPosition.Y = 0;
                return;
            }

            if (characterPosition.Y < 0)
                characterPosition.Y = 0;
            else if (characterPosition.Y >= totalLines)
                characterPosition.Y = totalLines - 1;

            if (characterPosition.X < 0)
                characterPosition.X = 0;
            else
            {
                int characterCount = textBuffer.GetCharacterCount(
                    characterPosition.Y, false, includeLineBreak);

                if (characterPosition.X > characterCount)
                    characterPosition.X = characterCount;
            }
        }

        void ValidateScreenPos(double x, double y)
        {
            // The screen coordinates get here including the top/left margin,
            // so we need to normalize the values to exclude those here.
            // 
            screenPosition.X = x - Configurations.CanvasMarginLeft;
            screenPosition.Y = y - Configurations.CanvasMarginTop;

            if (screenPosition.Y < 0)
                screenPosition.Y = 0;
            else
            {
                // Screen Y coordinate should always be within the code area.
                int totalLines = textBuffer.GetLineCount();
                int totalHeight = totalLines * Configurations.FontDisplayHeight;
                if (screenPosition.Y >= totalHeight)
                    screenPosition.Y = totalHeight - 1;
            }

            // Normalize Y coordinate to be of integral values.
            double lineIndex = screenPosition.Y / ((double)Configurations.FontDisplayHeight);
            screenPosition.Y = Math.Floor(lineIndex * Configurations.FontDisplayHeight);

            if (screenPosition.X < 0)
                screenPosition.X = 0;

            // Now calculation is done, denormalize it.
            screenPosition.X += Configurations.CanvasMarginLeft;
            screenPosition.Y += Configurations.CanvasMarginTop;
        }

        void CalculateCharacterPos()
        {
            double screenX = screenPosition.X - Configurations.CanvasMarginLeft;
            double screenY = screenPosition.Y - Configurations.CanvasMarginTop;

            // "screenPosition" should have been validated before this, 
            // so it is guaranteed that its values are always within range.
            double lineIndex = screenY / ((double)Configurations.FontDisplayHeight);
            characterPosition.Y = (int)Math.Floor(lineIndex);

            if (textBuffer.GetLineCount() <= 0)
            {
                characterPosition.X = 0;
                characterPosition.Y = 0;
                return; // Can't convert it.
            }

            double currMidPoint = 0;
            double tabVisualWidth = Configurations.FormatFontWidth * Configurations.TabSpaces.Length;

            int index = 0;
            characterPosition.X = 0;
            string line = textBuffer.GetLineContent(characterPosition.Y);
            if (null != line)
            {
                for (index = 0; index < line.Length; ++index)
                {
                    double halfCharWidth = Configurations.FormatFontWidth * 0.5;
                    if (line[index] == '\t')
                        halfCharWidth = tabVisualWidth * 0.5;

                    currMidPoint += halfCharWidth;
                    if (screenX < currMidPoint)
                    {
                        currMidPoint += halfCharWidth;
                        characterPosition.X = index;
                        break;
                    }

                    currMidPoint += halfCharWidth;
                }

                if (index == line.Length)
                    characterPosition.X = index;
            }
        }

        void CalculateScreenPos()
        {
            // "characterPosition" is already validated before this, just use it!
            screenPosition.Y = characterPosition.Y * Configurations.FontDisplayHeight;

            if (textBuffer.GetLineCount() <= 0)
            {
                screenPosition.X = Configurations.CanvasMarginLeft;
                screenPosition.Y = Configurations.CanvasMarginTop;
                return; // Can't convert it.
            }

            double currLeftEdge = 0;
            double tabVisualWidth = Configurations.FormatFontWidth * Configurations.TabSpaces.Length;

            int index = 0;
            screenPosition.X = 0;
            string line = textBuffer.GetLineContent(characterPosition.Y);
            if (null != line)
            {
                for (index = 0; index < line.Length; ++index)
                {
                    if (characterPosition.X == index)
                    {
                        screenPosition.X = currLeftEdge;
                        break;
                    }

                    if (line[index] == '\t')
                        currLeftEdge += tabVisualWidth;
                    else
                        currLeftEdge += Configurations.FormatFontWidth;
                }

                if (index == line.Length)
                    screenPosition.X = currLeftEdge;
            }

            screenPosition.X += Configurations.CanvasMarginLeft;
            screenPosition.Y += Configurations.CanvasMarginTop;
        }

        #endregion

    }
}
