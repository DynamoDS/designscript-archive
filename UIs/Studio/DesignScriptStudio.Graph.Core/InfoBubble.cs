using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace DesignScriptStudio.Graph.Core
{
    abstract class InfoBubble : IInfoBubble
    {
        #region Class Data Members

        protected object content = null; // Either 'string' or 'BitmapSource'.

        // @TODO(Joy): Make this "uint".
        protected uint nodeId = uint.MaxValue;

        protected Point anchorPoint;
        protected Point arrowLeft;
        protected Point arrowRight;
        protected Point rectPoint;
        protected int rectWidth;
        protected int rectHeight;
        protected uint bubbleId = uint.MaxValue;
        protected GraphController graphController = null;
        #endregion

        #region Public Class Properties

        public uint BubbleId { get { return this.bubbleId; } }

        // @TODO(Joy): Make this return "uint".
        public uint OwnerId
        {
            get { return this.nodeId; }
        }

        public object Content
        {
            get { return this.content; }
        }

        public Point RectPosition
        {
            get { return this.rectPoint; }
        }

        public int Width
        {
            get { return this.rectWidth; }
        }

        public int Height
        {
            get { return this.rectHeight; }
        }

        public Point AnchorPoint
        {
            get
            {
                VisualNode node = graphController.GetVisualNode(this.nodeId);
                return new Point(node.X + this.anchorPoint.X, node.Y + Math.Min(this.anchorPoint.Y, this.arrowLeft.Y));
            }
        }

        public bool IsPreviewBubble
        {
            get { return this is PreviewBubble; }
        }

        public bool Collapsed
        {
            get
            {
                if (this is PreviewBubble)
                {
                    PreviewBubble bubble = this as PreviewBubble;
                    return (bubble.PreviewType.HasFlag(PreviewTypes.Collapsed));
                }
                return false;
            }
        }

        public bool Extended
        {
            get
            {
                if (this is PreviewBubble)
                {
                    return ((PreviewBubble)this).Extended;
                }
                return false;
            }
        }

        public bool Extendable
        {
            get
            {
                if (this is PreviewBubble)
                {
                    PreviewBubble bubble = this as PreviewBubble;
                    return (bubble.PreviewType.HasFlag(PreviewTypes.String));
                }
                return false;
            }
        }

        #endregion

        #region Internal Class Methods

        internal InfoBubble(IGraphController controller, uint nodeId)
        {
            this.graphController = controller as GraphController;
            this.nodeId = nodeId;
            IdGenerator idGenerator = this.graphController.GetIdGenerator();
            this.bubbleId = idGenerator.GetNextId(ComponentType.Bubble);
            this.graphController.AddBubble(this);
        }

        internal abstract void SetContent(object content);



        #endregion

        #region Protected Class Methods

        protected void DrawBubble(DrawingContext drawingContext, Brush backgroundColor, Pen borderPen, bool drawRectangle)
        {
            if (!drawRectangle)
            {
                //Inverted Arrow
                LineSegment[] segments = new LineSegment[] { new LineSegment(arrowLeft, true), new LineSegment(arrowRight, true) };
                PathFigure figure = new PathFigure(new Point(anchorPoint.X, anchorPoint.Y), segments, true);
                PathGeometry geo = new PathGeometry(new PathFigure[] { figure });
                drawingContext.DrawGeometry(backgroundColor, borderPen, geo);
            }
            else
            {
                //Rounded Rectangle
                RectangleGeometry roundedRect = new RectangleGeometry();
                roundedRect.Rect = new Rect(rectPoint, new Size(rectWidth, rectHeight));
                roundedRect.RadiusX = Configurations.CornerRadius;
                roundedRect.RadiusY = Configurations.CornerRadius;
                drawingContext.DrawGeometry(backgroundColor, borderPen, roundedRect);

                //Rectangle below Arrow for hittesting
                drawingContext.DrawRectangle(Brushes.Transparent, Configurations.TransparentPen,
                                             new Rect(anchorPoint.X - Configurations.InfoBubbleArrowHitTestWidth / 2, anchorPoint.Y,
                                                      Configurations.InfoBubbleArrowHitTestWidth, Configurations.InfoBubbleArrowHitTestHeight));

                //Arrow
                LineSegment[] segments = new LineSegment[] { new LineSegment(arrowLeft, true), new LineSegment(arrowRight, true) };
                PathFigure figure = new PathFigure(new Point(anchorPoint.X, anchorPoint.Y), segments, true);
                PathGeometry geo = new PathGeometry(new PathFigure[] { figure });
                drawingContext.DrawGeometry(backgroundColor, borderPen, geo);

                //rectangle background
                RectangleGeometry backgroundRect = new RectangleGeometry();
                backgroundRect.RadiusX = Configurations.CornerRadius;
                backgroundRect.RadiusY = Configurations.CornerRadius;
                backgroundRect.Rect = new Rect(rectPoint.X + 0.5, rectPoint.Y + 0.5, rectWidth - 1, rectHeight - 1);
                drawingContext.DrawGeometry(backgroundColor, null, backgroundRect);
            }
        }

        #endregion
    }
}