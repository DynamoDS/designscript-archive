using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace DesignScriptStudio.Graph.Core
{
    [Flags]
    enum DotTypes
    {
        // @TODO(Joy)DONE: Please use 0x00000040 form for clarity,
        // rather than these decimal values.
        None = 0x0000,
        TopLeft = 0x0001,
        Top = 0x0002,
        TopRight = 0x0004,
        MiddleLeft = 0x0008,
        Middle = 0x0010,
        MiddleRight = 0x0020,
        BottomLeft = 0x0040,
        Bottom = 0x0080,
        BottomRight = 0x0100,
        Connection
    }

    enum AnchorPoint
    {
        TopRight,
        TopLeft,
        BottomRight,
        BottomLeft
    }

    class DrawingUtilities
    {
        internal static void DrawDots(DrawingContext drawingContext, DotTypes types, Point p, AnchorPoint anchor, bool selected)
        {
            int gridSize = Configurations.DotGridSize;
            int cellSize = gridSize / 3; //3x3 grid
            Size rectSize = new Size(2, 2);
            Rect rect = new Rect();
            p.X = (int)p.X;
            p.Y = (int)p.Y;
            Point pt = p;

            switch (anchor)
            {
                case AnchorPoint.TopLeft:
                    break;
                case AnchorPoint.TopRight:
                    p.X -= gridSize;
                    break;
                case AnchorPoint.BottomLeft:
                    p.Y -= gridSize;
                    break;
                case AnchorPoint.BottomRight:
                    p.X -= gridSize;
                    p.Y -= gridSize;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            if (types.HasFlag(DotTypes.TopLeft))
            {
                pt = p;
                pt.Offset(0, 0);
                rect = new Rect(pt, rectSize);
                DrawSmallDots(drawingContext, selected, rect);
            }

            if (types.HasFlag(DotTypes.Top))
            {
                pt = p;
                pt.Offset(cellSize, 0);
                rect = new Rect(pt, rectSize);
                DrawSmallDots(drawingContext, selected, rect);
            }

            if (types.HasFlag(DotTypes.TopRight))
            {
                pt = p;
                pt.Offset(2 * cellSize, 0);
                rect = new Rect(pt, rectSize);
                DrawSmallDots(drawingContext, selected, rect);
            }

            if (types.HasFlag(DotTypes.MiddleLeft))
            {
                pt = p;
                pt.Offset(0, cellSize);
                rect = new Rect(pt, rectSize);
                DrawSmallDots(drawingContext, selected, rect);
            }

            if (types.HasFlag(DotTypes.Middle))
            {
                pt = p;
                pt.Offset(0, cellSize);
                rect = new Rect(pt, rectSize);
                DrawSmallDots(drawingContext, selected, rect);
            }

            if (types.HasFlag(DotTypes.MiddleRight))
            {
                pt = p;
                pt.Offset(2 * cellSize, cellSize);
                rect = new Rect(pt, rectSize);
                DrawSmallDots(drawingContext, selected, rect);
            }

            if (types.HasFlag(DotTypes.BottomLeft))
            {
                pt = p;
                pt.Offset(0, 2 * cellSize);
                rect = new Rect(pt, rectSize);
                DrawSmallDots(drawingContext, selected, rect);
            }

            if (types.HasFlag(DotTypes.Bottom))
            {
                pt = p;
                pt.Offset(cellSize, 2 * cellSize);
                rect = new Rect(pt, rectSize);
                DrawSmallDots(drawingContext, selected, rect);
            }

            if (types.HasFlag(DotTypes.BottomRight))
            {
                pt = p;
                pt.Offset(2 * cellSize, 2 * cellSize);
                rect = new Rect(pt, rectSize);
                DrawSmallDots(drawingContext, selected, rect);
            }
            return;
        }

        internal static void DrawContextualMenu(DrawingVisual drawingVisual, Dictionary<int, string> itemsList)
        {
            //draw the stuff

            //3 cases
            //menu items == 7
            //menu items < 7
            //menu items > 7

            DrawingContext context = drawingVisual.RenderOpen();
            int itemsCount = itemsList.Count;
            if (itemsCount == 7)
            {

            }

            if (itemsCount < 7)
            {

            }

            if (itemsCount > 7)
            {

            }
            // position the menu correctly
        }

        private static void DrawSmallDots(DrawingContext drawingContext, bool selectedFlag, Rect rect)
        {
            if (selectedFlag == true)
                drawingContext.DrawRectangle(Configurations.DotColorSelected, Configurations.NoBorderPen, rect);
            else
                drawingContext.DrawRectangle(Configurations.DotColor, Configurations.NoBorderPen, rect);
        }


    }
}
