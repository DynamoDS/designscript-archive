using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.TextFormatting;
using System.Globalization;

namespace DesignScript.Editor
{
    using DesignScript.Parser;
    using DesignScript.Editor.Core;
    using System.Windows.Threading;
    using System.Timers;

    class SourceDisplayLayer : VisualLayer
    {
        private int numberOfLines;

        public SourceDisplayLayer(TextEditorCanvas textEditorCanvas)
            : base(textEditorCanvas)
        {
        }

        protected override void UpdateLayerForScriptCore(IScriptObject hostScript)
        {
        }

        protected override DrawingVisual RenderCore(RenderParameters renderParams)
        {
            if (drawingVisual == null)
                drawingVisual = new DrawingVisual();

            if (null == currentScript)
                return drawingVisual; // No active script yet!

            System.Windows.Point linePosition = new System.Windows.Point(
                Configurations.CanvasMarginLeft, 0);

            ITextEditorCore textCore = textEditorCanvas.TextEditorCore;
            ITextBuffer textBuffer = currentScript.GetTextBuffer();
            numberOfLines = textBuffer.GetLineCount();
            int lastVisibleLine = renderParams.firstVisibleLine + renderParams.maxVisibleLines;
            if (lastVisibleLine >= numberOfLines)
                lastVisibleLine = numberOfLines - 1;

            // Retrieve the DrawingContext from the DrawingVisual.
            DrawingContext context = drawingVisual.RenderOpen();

            int maxColumns = textEditorCanvas.MaxVisibelColumns + 1;
            double hiddenWidth = textEditorCanvas.FirstVisibleColumn * Configurations.FormatFontWidth;

            CharPosition converter = this.currentScript.CreateCharPosition();

            Typeface font = new Typeface(Configurations.FontFace);
            for (int lineIndex = renderParams.firstVisibleLine; lineIndex <= lastVisibleLine; lineIndex++)
            {
                // A constant starting point on left edge.
                linePosition.X = Configurations.CanvasMarginLeft;
                int startColumn = converter.VisualToCharOffset(lineIndex, textEditorCanvas.FirstVisibleColumn);

                string lineContent = textBuffer.GetLineContent(lineIndex);
                int lineLength = (lineContent != null ? lineContent.Length : 0);
                if (lineLength <= startColumn)
                {
                    linePosition.Y += Configurations.FontDisplayHeight;
                    continue;
                }

                if (startColumn > 0)
                {
                    string hiddenText = lineContent.Substring(0, startColumn);
                    hiddenText = hiddenText.Replace("\t", Configurations.TabSpaces);

                    // Accounting for the part that is hidden beyond the left of canvas.
                    double hiddenTextWidth = hiddenText.Length * Configurations.FormatFontWidth;
                    linePosition.X = linePosition.X + hiddenTextWidth - hiddenWidth;
                }

                int endColumn = startColumn + maxColumns;
                endColumn = ((endColumn >= lineLength) ? lineLength - 1 : endColumn);
                for (int charIndex = startColumn; charIndex <= endColumn; charIndex++)
                {
                    CodeFragment fragment = null;
                    int fragmentWidth = textCore.GetFragment(
                        charIndex, lineIndex, out fragment);

                    // We may be showing partial fragment now.
                    if (null != fragment)
                        fragmentWidth -= (charIndex - fragment.ColStart);

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

                    // Replace all TAB characters with actual spaces.
                    textContent = textContent.Replace("\t", Configurations.TabSpaces);

                    CodeFragment.Type fragmentType = CodeFragment.Type.None;
                    if (null != fragment)
                        fragmentType = fragment.CodeType;

                    Color textColor = CodeFragment.GetFragmentColor(fragmentType);
                    if ((textBuffer.ParsePending == true) && (fragmentType == CodeFragment.Type.None))
                        textColor = Colors.Black;

                    FormattedText formattedText = new FormattedText(
                        textContent,
                        CultureInfo.GetCultureInfo("en-us"),
                        FlowDirection.LeftToRight,
                        font,
                        Configurations.FontHeight,
                        new SolidColorBrush(textColor));

                    // Draw the formatted text into the drawing context.
                    context.DrawText(formattedText, linePosition);

                    if (lineContent[charIndex] == '\n')
                        break;
                    else
                    {
                        linePosition.X += formattedText.WidthIncludingTrailingWhitespace;
                    }

                    if (fragmentWidth > 0)
                        charIndex += offsetToNextChar;
                }

                // Update the line position coordinate for the displayed line.
                linePosition.Y += Configurations.FontDisplayHeight;
            }

            // Persist the drawn text content.
            context.Close();
            return drawingVisual;
        }
    }
}
