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
using DesignScript.Editor.Core;

namespace DesignScript.Editor
{
    using DesignScript.Parser;
    using ProtoCore.CodeModel;

    public struct RenderParameters
    {
        public int firstVisibleLine;
        public int maxVisibleLines;
        public int firstVisibleColumn;
        public int maxVisibleColumns;
    }

    /// <summary>
    /// Summary description for Visual Layer
    /// </summary>
    public abstract class VisualLayer
    {
        protected DrawingVisual drawingVisual = null;
        protected TextEditorCanvas textEditorCanvas = null;
        protected CodeRange executionCursor = new CodeRange();
        protected List<CodeRange> breakpointsRef = null;
        protected IScriptObject currentScript = null;

        public VisualLayer(TextEditorCanvas textEditorCanvas)
        {
            this.textEditorCanvas = textEditorCanvas;
        }

        public void SetBreakpointsReference(List<CodeRange> breakpoints)
        {
            this.breakpointsRef = breakpoints;
        }

        public void ResetExecutionCursor()
        {
            executionCursor = new CodeRange();
        }

        public void SetExecutionCursor(ProtoCore.CodeModel.CodeRange executionCursor)
        {
            this.executionCursor = executionCursor;
        }

        public void UpdateLayerForScript(IScriptObject hostScript)
        {
            currentScript = hostScript;
            UpdateLayerForScriptCore(hostScript);
        }

        public DrawingVisual Render()
        {
            RenderParameters renderParams;
            renderParams.firstVisibleLine = textEditorCanvas.FirstVisibleLine;
            renderParams.maxVisibleLines = textEditorCanvas.MaxVisibleLines;
            renderParams.firstVisibleColumn = textEditorCanvas.FirstVisibleColumn;
            renderParams.maxVisibleColumns = textEditorCanvas.MaxVisibelColumns;
            return this.RenderCore(renderParams);
        }

        protected bool FallsWithinActiveScript(CodePoint codePoint)
        {
            if (null == codePoint.SourceLocation || (null == currentScript))
                return false;
            if (string.IsNullOrEmpty(codePoint.SourceLocation.FilePath))
                throw new InvalidOperationException("'SourceLocation' not specified!");

            IParsedScript parsedScript = currentScript.GetParsedScript();
            if (null == parsedScript)
                return false;

            string sourcePath = codePoint.SourceLocation.FilePath;
            string activePath = parsedScript.GetScriptPath();
            return (string.Compare(sourcePath, activePath, true) == 0);
        }

        protected List<Rect> CalculateRectangles(CharPosition start, CharPosition end, int firstVisibleLine)
        {
            if (start.CharacterY > end.CharacterY)
                return null;
            else if (start.CharacterY == end.CharacterY)
            {
                if (start.CharacterX == end.CharacterX)
                    return null;
            }

            List<Rect> results = new List<Rect>();
            double vertDisplayOffset = firstVisibleLine * Configurations.FontDisplayHeight;
            double horzDisplayOffset = textEditorCanvas.FirstVisibleColumn * Configurations.FormatFontWidth;

            if (start.CharacterY == end.CharacterY) // Start and end are on the same line.
            {
                double x = start.ScreenX - horzDisplayOffset;
                double y = start.ScreenY - vertDisplayOffset;
                double width = end.ScreenX - start.ScreenX;
                results.Add(new Rect(x, y, width, Configurations.FontDisplayHeight));
                return results;
            }

            ITextBuffer textBuffer = currentScript.GetTextBuffer();
            for (int index = start.CharacterY; index <= end.CharacterY; ++index)
            {
                double x = 0, y = 0, width = 0;

                if (index == start.CharacterY) // If this is the first line.
                {
                    x = start.ScreenX -Configurations.ShadowWidth;
                    
                    y = start.ScreenY;
                    width = Configurations.FormatFontWidth * textBuffer.GetCharacterCount(index, true, true);
                    width = (width - (start.ScreenX - Configurations.CanvasMarginLeft))+Configurations.ShadowWidth;
                }
                else if (index == end.CharacterY) // If this is the last line.
                {
                    x = Configurations.CanvasMarginLeft - Configurations.ShadowWidth;
                    y = end.ScreenY;
                    width = end.ScreenX - Configurations.CanvasMarginLeft + Configurations.ShadowWidth;
                }
                else // All lines except the first and last lines.
                {
                    x = Configurations.CanvasMarginLeft - Configurations.ShadowWidth;
                    y = index * Configurations.FontDisplayHeight;
                    width = Configurations.FormatFontWidth * textBuffer.GetCharacterCount(index, true, true) + Configurations.ShadowWidth;
                }

                if (width > 0) // Sorry babe, we're only interested in non-empty lines.
                {
                    y = y - vertDisplayOffset;
                    x = x - horzDisplayOffset;
                    results.Add(new Rect(x, y, width, Configurations.FontDisplayHeight));
                }
            }

            return results;
        }

        protected Rect CalculateRectangleFullLine(CharPosition start, int firstVisibleLine)
        {
            double x = 0, y = 0, width = 0;
            double vertDisplayOffset = firstVisibleLine * Configurations.FontDisplayHeight;
            double horzDisplayOffset = textEditorCanvas.FirstVisibleColumn * Configurations.FormatFontWidth;

            x = Configurations.CanvasMarginLeft - Configurations.ShadowWidth;
            y = start.ScreenY;
            width = textEditorCanvas.ActualWidth + textEditorCanvas.HorizontalOffset;
            if (width > 0)
            {
                y = y - vertDisplayOffset;
                x = x - horzDisplayOffset;
                return new Rect(x, y, width, Configurations.FontDisplayHeight);
            }

            return new Rect();
        }

        protected void RenderRectangles(DrawingContext context, List<Rect> rectangles, Color color)
        {
            if (null == rectangles || (rectangles.Count <= 0))
                return; // Nothing to draw here, move on.

            SolidColorBrush colorBrush = new SolidColorBrush(color);
            foreach (Rect rectangle in rectangles)
                context.DrawRectangle(colorBrush, null, rectangle);
        }

        protected void RenderRectangle(DrawingContext context, Rect rectangle, Color color)
        {
            if (rectangle.IsEmpty)
                return;

            SolidColorBrush colorBrush = new SolidColorBrush(color);
            context.DrawRectangle(colorBrush, null, rectangle);
        }

        protected abstract void UpdateLayerForScriptCore(IScriptObject hostScript);

        protected abstract DrawingVisual RenderCore(RenderParameters renderParams);
    }
}
