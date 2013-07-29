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
    using ProtoCore.CodeModel;

    class LineHeadingLayer : VisualLayer
    {
        BitmapImage breakpointImage = null;
        BitmapImage runIndiactorImage = null;
        BitmapImage errorImage = null;
        BitmapImage warningImage = null;
        BitmapImage errorEditImage = null;

        public LineHeadingLayer(TextEditorCanvas textEditorCanvas)
            : base(textEditorCanvas)
        {
            breakpointImage = new BitmapImage(new Uri(Images.BreakpointImage, UriKind.Absolute));
            runIndiactorImage = new BitmapImage(new Uri(Images.RunIndicator, UriKind.Absolute));
            errorImage = new BitmapImage(new Uri(Images.ErrorImage, UriKind.Absolute));
            warningImage = new BitmapImage(new Uri(Images.WarningImage, UriKind.Absolute));
            errorEditImage = new BitmapImage(new Uri(Images.ErrorEditImage, UriKind.Absolute));
        }

        protected override void UpdateLayerForScriptCore(IScriptObject hostScript)
        {
        }

        protected override DrawingVisual RenderCore(RenderParameters renderParams)
        {
            if (null == drawingVisual)
                drawingVisual = new DrawingVisual();

            if (null == currentScript)
                return drawingVisual; // There's nothing here yet.

            // Even when a line is half shown, we draw it in full.
            renderParams.maxVisibleLines = renderParams.maxVisibleLines + 1;

            ITextBuffer textBuffer = currentScript.GetTextBuffer();
            int lineCount = textBuffer.GetLineCount();
            lineCount = ((0 == lineCount) ? 1 : lineCount);
            if (lineCount > renderParams.maxVisibleLines)
                lineCount = renderParams.maxVisibleLines;
            int lastVisibleLine = renderParams.firstVisibleLine + lineCount;

            DrawingContext context = drawingVisual.RenderOpen();
            double columnHeight = renderParams.maxVisibleLines * Configurations.FontDisplayHeight;

            // BreakpointIcon Rectangle
            Rect breakpointRectangle = new Rect(0, 0, Configurations.BreakpointColumnWidth, columnHeight);
            SolidColorBrush breakpointRectangleBrush = new SolidColorBrush(UIColors.BreakpointColor);
            context.DrawRectangle(breakpointRectangleBrush, null, breakpointRectangle);

            // Line Number Rectangle
            double lineColumnStart = Configurations.LineNumberColumnStart;
            double lineColumnWidth = Configurations.LineNumberColumnWidth;
            Rect numberRectangle = new Rect(lineColumnStart, 0, lineColumnWidth, columnHeight);
            SolidColorBrush lineColumnBrush = new SolidColorBrush(UIColors.LineColumnBrushColor);
            context.DrawRectangle(lineColumnBrush, null, numberRectangle);

            //Shadow Lines
            double halfShadowWidth = Configurations.ShadowWidth / 2;
            double start = Configurations.LineNumberColumnEnd;
            Point startPointDark = new Point(start, 0);
            Point startPointLight = new Point(start + halfShadowWidth, 0);
            Point endPointDark = new Point(start, columnHeight);
            Point endPointLight = new Point(start + halfShadowWidth, columnHeight);
            SolidColorBrush shadowDarkBrush = new SolidColorBrush(UIColors.ShadowDarkColor);
            SolidColorBrush shadowLightBrush = new SolidColorBrush(UIColors.ShadowLightColor);
            Pen darkPen = new Pen(shadowDarkBrush, halfShadowWidth);
            Pen lightPen = new Pen(shadowLightBrush, halfShadowWidth);
            context.DrawLine(darkPen, startPointDark, endPointDark);
            context.DrawLine(lightPen, startPointLight, endPointLight);

            double textColumnRightEdge = Configurations.LineNumberColumnStart +
                lineColumnWidth - Configurations.LineNumberColumnPadding;
            Point point = new Point(textColumnRightEdge, 0);
            Typeface typeface = new Typeface(Configurations.FontFace);

            for (int index = renderParams.firstVisibleLine + 1; index <= lastVisibleLine; index++)
            {
                FormattedText formattedtext = new FormattedText(index.ToString(), CultureInfo.GetCultureInfo("en-US"),
                    FlowDirection.LeftToRight, typeface, Configurations.FontHeight, Brushes.Gray);
                formattedtext.TextAlignment = TextAlignment.Right;
                context.DrawText(formattedtext, point);
                point.Y = point.Y + Configurations.FontDisplayHeight;
            }

            RenderBreakpoints(context, renderParams.firstVisibleLine, lastVisibleLine);
            HighlightCurrentLine(context, renderParams.firstVisibleLine, lastVisibleLine);
            RenderInlineIcons(context, renderParams.firstVisibleLine, lastVisibleLine);

            context.Close();
            return drawingVisual;
        }

        void RenderBreakpoints(DrawingContext context, int firstVisibleLine, int lastVisibleLine)
        {
            ITextBuffer textBuffer = currentScript.GetTextBuffer();
            if (textBuffer.GetLineContent(0) == null || (null == breakpointsRef))
                return;

            Size size = new Size(Configurations.IconWidth, Configurations.IconHeight);

            foreach (CodeRange breakpoint in breakpointsRef)
            {
                if (FallsWithinActiveScript(breakpoint.StartInclusive) == false)
                    continue; // The breakpoint does not belong to this script.

                // Offset breakpoint line index because it is 1-based.
                int breakpointLine = breakpoint.StartInclusive.LineNo;
                if (breakpointLine > 0)
                    breakpointLine = breakpoint.StartInclusive.LineNo - 1;

                if (breakpointLine < firstVisibleLine || (breakpointLine > lastVisibleLine))
                    continue; // Here we exclude breakpoints that are beyond the visible range.

                breakpointLine = breakpointLine - firstVisibleLine;
                double topCoord = breakpointLine * Configurations.FontDisplayHeight;
                Point point = new Point(Configurations.IconPadding, topCoord);
                Rect rect = new Rect(point, size);
                context.DrawImage(breakpointImage, rect);
            }
        }

        void RenderInlineIcons(DrawingContext context, int firstVisibleLine, int lastVisibleLine)
        {
            List<InlineMessageItem> messageList = Solution.Current.GetInlineMessage();
            if (messageList != null)
            {
                Size size = new Size(Configurations.IconWidth, Configurations.IconHeight);

                foreach (InlineMessageItem message in messageList)
                {
                    //don't show icons beyond visible range (firstVisibleLine)
                    int line = message.Line;
                    if (line < firstVisibleLine || line > lastVisibleLine)
                        continue;

                    if (Solution.Current.ActiveScript != null)
                    {
                        if (message.FilePath != Solution.Current.ActiveScript.GetParsedScript().GetScriptPath())
                            continue;
                    }

                    if (line >= 0)
                    {
                        line = line - firstVisibleLine;
                        double topCoord = line * Configurations.FontDisplayHeight;
                        Point point = new Point(Configurations.InlineIconStart, topCoord);
                        Rect rect = new Rect(point, size);

                        switch (message.Type)
                        {
                            case InlineMessageItem.OutputMessageType.Error:
                                context.DrawImage(errorImage, rect);
                                break;
                            case InlineMessageItem.OutputMessageType.Warning:
                                context.DrawImage(warningImage, rect);
                                break;
                            case InlineMessageItem.OutputMessageType.PossibleError:
                                context.DrawImage(errorEditImage, rect);
                                break;
                            case InlineMessageItem.OutputMessageType.PossibleWarning:
                                context.DrawImage(errorEditImage, rect);
                                break;
                        }
                    }
                }
            }
        }


        void HighlightCurrentLine(DrawingContext context, int firstVisibleLine, int lastVisibleLine)
        {
            if (FallsWithinActiveScript(executionCursor.StartInclusive) == false)
                return;
            if (executionCursor.StartInclusive.LineNo <= 0)
                return;

            // ExecutionCursor is 1-based, so decrement it here.
            int lineIndex = executionCursor.StartInclusive.LineNo - 1;
            if (lineIndex < firstVisibleLine || (lineIndex > lastVisibleLine))
                return; // Don't draw if it is beyond the visible range.

            lineIndex = lineIndex - firstVisibleLine;
            double topCoord = lineIndex * Configurations.FontDisplayHeight;
            Point point = new Point(Configurations.IconPadding, topCoord);
            Size size = new Size(Configurations.IconWidth, Configurations.IconHeight);
            Rect rectIcon = new Rect(Configurations.LineNumberColumnEnd - 4, topCoord + 2, runIndiactorImage.Width * 3, runIndiactorImage.Height * 3);
            context.DrawImage(runIndiactorImage, rectIcon);
        }
    }
}
