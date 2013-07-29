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


    /// <summary>
    /// Summary description for Highlight Layer
    /// </summary>
    class HighlightLayer : VisualLayer
    {
        private CharPosition selectionStart = null;
        private CharPosition selectionEnd = null;
        private CharPosition lineSelection = null;
        private CharPosition inLineSelection = null;
        private CharPosition allOccurencesSelectionStart = null;
        private CharPosition allOccurencesSelectionEnd = null;
        private CharPosition currentOccurenceSelectionStart = null;
        private CharPosition currentOccurenceSelectionEnd = null;
        System.Drawing.Point prevCursorPosition = new System.Drawing.Point(-1, -1);

        public HighlightLayer(TextEditorCanvas textEditorCanvas)
            : base(textEditorCanvas)
        {
        }

        public void ResetHighlightLayer()
        {
            selectionStart.SetCharacterPosition(0, 0);
            selectionEnd.SetCharacterPosition(0, 0);
        }

        public void SetSelectionRange(System.Drawing.Point start, System.Drawing.Point end)
        {
            selectionStart.SetCharacterPosition(start);
            selectionEnd.SetCharacterPosition(end, true);
        }

        protected override void UpdateLayerForScriptCore(IScriptObject hostScript)
        {
            selectionStart = hostScript.CreateCharPosition();
            selectionEnd = hostScript.CreateCharPosition();
            lineSelection = hostScript.CreateCharPosition();
            inLineSelection = hostScript.CreateCharPosition();
            allOccurencesSelectionStart = hostScript.CreateCharPosition();
            allOccurencesSelectionEnd = hostScript.CreateCharPosition();
            currentOccurenceSelectionStart = hostScript.CreateCharPosition();
            currentOccurenceSelectionEnd = hostScript.CreateCharPosition();
        }

        protected override DrawingVisual RenderCore(RenderParameters renderParams)
        {
            if (null == drawingVisual)
                drawingVisual = new DrawingVisual();

            // There's no script yet.
            if (null == currentScript)
                return drawingVisual;
            if (null == textEditorCanvas.TextEditorCore.CurrentTextBuffer)
                return drawingVisual;

            DrawingContext context = drawingVisual.RenderOpen();


            lineSelection.SetCharacterPosition(textEditorCanvas.TextEditorCore.CursorPosition);
            Rect cursorSelections = CalculateRectangleFullLine(lineSelection, renderParams.firstVisibleLine);
            RenderRectangle(context, cursorSelections, UIColors.CursorSelectionColor);

            List<InlineMessageItem> messageList = Solution.Current.GetInlineMessage();
            if (messageList != null)
            {
                foreach (InlineMessageItem message in messageList)
                {
                    int line = message.Line;
                    int column = message.Column;

                    if (Solution.Current.ActiveScript == null || message.FilePath != Solution.Current.ActiveScript.GetParsedScript().GetScriptPath())
                        continue;

                    if (line >= 0)
                    {
                        System.Drawing.Point inlineSelectionStart = new System.Drawing.Point(column, line);
                        inLineSelection.SetCharacterPosition(inlineSelectionStart);
                        Rect outputSelections = CalculateRectangleFullLine(inLineSelection, renderParams.firstVisibleLine);
                        Color inLineColor = new Color();
                        switch (message.Type)
                        {
                            case InlineMessageItem.OutputMessageType.Error:
                                inLineColor = UIColors.InlinErrorColor;
                                break;
                            case InlineMessageItem.OutputMessageType.Warning:
                                inLineColor = UIColors.InlineWarningColor;
                                break;
                            case InlineMessageItem.OutputMessageType.PossibleError:
                                inLineColor = UIColors.InlinPossibleErrorColor;
                                break;
                            case InlineMessageItem.OutputMessageType.PossibleWarning:
                                inLineColor = UIColors.InlinPossibleWarningColor;
                                break;
                        }
                        RenderRectangle(context, outputSelections, inLineColor);
                    }
                }
            }

            if (null != breakpointsRef)
            {
                CharPosition startPoint = currentScript.CreateCharPosition();
                CharPosition endPoint = currentScript.CreateCharPosition();

                foreach (CodeRange breakpoint in breakpointsRef)
                {
                    if (FallsWithinActiveScript(breakpoint.StartInclusive) == false)
                        continue; // This breakpoint does not belong to the script.

                    int startX = breakpoint.StartInclusive.CharNo - 1;
                    int startY = breakpoint.StartInclusive.LineNo - 1;
                    int endX = breakpoint.EndExclusive.CharNo - 1;
                    int endY = breakpoint.EndExclusive.LineNo - 1;

                    startPoint.SetCharacterPosition(startX, startY);
                    endPoint.SetCharacterPosition(endX, endY);
                    Rect region = CalculateRectangleFullLine(startPoint, renderParams.firstVisibleLine);
                    RenderRectangle(context, region, UIColors.BreakpointLineColor);
                }
            }

            if (FallsWithinActiveScript(executionCursor.StartInclusive))
            {
                CharPosition start = currentScript.CreateCharPosition();
                CharPosition end = currentScript.CreateCharPosition();

                start.SetCharacterPosition(
                    executionCursor.StartInclusive.CharNo - 1,
                    executionCursor.StartInclusive.LineNo - 1);

                end.SetCharacterPosition(
                    executionCursor.EndExclusive.CharNo - 1,
                    executionCursor.EndExclusive.LineNo - 1);

                List<Rect> execCursors = CalculateRectangles(start, end, renderParams.firstVisibleLine);
                RenderRectangles(context, execCursors, UIColors.ExecutionCursorColor);
            }

            List<Rect> selections = CalculateRectangles(selectionStart, selectionEnd, renderParams.firstVisibleLine);
            RenderRectangles(context, selections, UIColors.SelectionsColor);

            List<FindPosition> searchResults = textEditorCanvas.TextEditorCore.CurrentTextBuffer.SearchResult;
            if (searchResults != null)
            {
                System.Drawing.Point start;
                System.Drawing.Point end;

                foreach (FindPosition item in searchResults)
                {
                    start = item.startPoint;
                    end = item.endPoint;
                    end.X += 1;
                    allOccurencesSelectionStart.SetCharacterPosition(start);
                    allOccurencesSelectionEnd.SetCharacterPosition(end);
                    List<Rect> findSelections = CalculateRectangles(allOccurencesSelectionStart, allOccurencesSelectionEnd, renderParams.firstVisibleLine);
                    RenderRectangles(context, findSelections, Colors.Yellow);
                }


                int searchIndex = textEditorCanvas.TextEditorCore.CurrentTextBuffer.CurrentSearchIndex;
                if (searchResults.Count != 0 && searchIndex != -1)
                {
                    FindPosition currentPosition = searchResults[searchIndex];

                    System.Drawing.Point startPoint = currentPosition.startPoint;
                    System.Drawing.Point endPoint = currentPosition.endPoint;

                    endPoint.X += 1;

                    if (startPoint.X == 0 && endPoint.X == 0 &&
                       startPoint.Y == 0 && endPoint.Y == 0)
                    {
                    }
                    else
                    {
                       
                        currentOccurenceSelectionStart.SetCharacterPosition(startPoint);
                        currentOccurenceSelectionEnd.SetCharacterPosition(endPoint);
                        List<Rect> currentOccurenceSelections = CalculateRectangles(currentOccurenceSelectionStart, currentOccurenceSelectionEnd, renderParams.firstVisibleLine);
                        RenderRectangles(context, currentOccurenceSelections, Colors.Orange);
                    }
                }
            }




            context.Close();
            return drawingVisual;
        }
    }
}
