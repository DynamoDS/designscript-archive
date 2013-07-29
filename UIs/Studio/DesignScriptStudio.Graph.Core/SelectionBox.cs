using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DesignScriptStudio.Graph.Core
{
    class SelectionBox
    {
        #region Class Properties
        internal double x = 0;
        internal double y = 0;
        internal double width = 0;
        internal double height = 0;
        private double relativePositionToMouseX = 0;
        private double relativePositionToMouseY = 0;

        private IGraphController graphController = null;
        private IGraphVisualHost visualHost = null;
        #endregion

        #region Class Internal Methods
        public SelectionBox(IGraphController graphController, IGraphVisualHost visualHost)
        {
            this.graphController = graphController;
            this.visualHost = visualHost;
            InitializeSelectionBox();
        }

        internal void UpdateSelectionBox(List<IVisualNode> selectedNodes)
        {
            double x;
            double y;
            double width;
            double height;
            GetPositionSizeFromNodes(selectedNodes, out x, out y, out width, out height);
            UpdateSelectionBox(x, y, width, height);
        }

        internal void UpdateSelectionBox(double x, double y, double width, double height)
        {
            Move(x, y);
            Resize(width, height);
        }

        internal void UpdateSelectionBox(double mouseX, double mouseY)
        {
            double newX = mouseX - relativePositionToMouseX;
            double newY = mouseY - relativePositionToMouseY;
            Move(newX, newY);
        }

        internal bool Hittest(Point mousePosition)
        {
            Point pt = new Point(this.x, this.y);
            pt.Offset(this.width, 0);

            if (mousePosition.X > pt.X - Configurations.HittestPixels && mousePosition.X < pt.X)
                if (mousePosition.Y > pt.Y && mousePosition.Y < pt.Y + Configurations.HittestPixels)
                    return true;
            return false;
        }

        internal void SetRelativePositionToMouse(double mouseX, double mouseY)
        {
            this.relativePositionToMouseX = mouseX - this.x;
            this.relativePositionToMouseY = mouseY - this.y;
        }
        #endregion

        #region Class Private Helper Methods
        private void GetPositionSizeFromNodes(List<IVisualNode> nodes, out double x, out double y, out double w, out double h)
        {
            double xTopLeft = -1;
            double yTopLeft = -1;
            double xBotRight = -1;
            double yBotRight = -1;
            if (nodes.Count > 1)
            {
                foreach (IVisualNode node in nodes)
                {
                    double xNewTopLeft = node.X;
                    double yNewTopLeft = node.Y;
                    double xNewBotRight = node.X + node.Width;
                    double yNewBotRight = node.Y + node.Height;
                    if (xTopLeft == -1 || xTopLeft > xNewTopLeft)
                        xTopLeft = xNewTopLeft;
                    if (yTopLeft == -1 || yTopLeft > yNewTopLeft)
                        yTopLeft = yNewTopLeft;
                    if (xBotRight == -1 || xBotRight < xNewBotRight)
                        xBotRight = xNewBotRight;
                    if (yBotRight == -1 || yBotRight < yNewBotRight)
                        yBotRight = yNewBotRight;
                }
            }
            x = xTopLeft - 20;
            y = yTopLeft - 20;
            w = xBotRight - xTopLeft + 40;
            h = yBotRight - yTopLeft + 40;
        }

        private void InitializeSelectionBox()
        {
            //headless mode, return
            if (this.visualHost == null)
                return;
            DrawingVisual visual = this.visualHost.GetDrawingVisualForSelectionBox();
            visual.Transform = new TranslateTransform();
        }

        private void DrawSelectionBox(double width, double height)
        {
            //headless mode, return
            if (this.visualHost == null)
                return;
            DrawingVisual visual = this.visualHost.GetDrawingVisualForSelectionBox();
            DrawingContext drawingContext = visual.RenderOpen();
            Rect rect = new Rect(new Size(width, height));
            drawingContext.DrawRectangle(Configurations.SelectionBoxBackgroundColor, Configurations.SelectionBoxPen, rect);

            Point p = new Point(-1, 2);
            p.Offset(width, 0);
            DrawingUtilities.DrawDots(drawingContext, DotTypes.Top | DotTypes.TopRight | DotTypes.MiddleRight, p, AnchorPoint.TopRight, false);

            drawingContext.Close();

            this.width = width;
            this.height = height;
        }

        private void Move(double x, double y)
        {
            //headless mode, return
            if (this.visualHost == null)
                return;
            DrawingVisual visual = this.visualHost.GetDrawingVisualForSelectionBox();
            TranslateTransform tranform = visual.Transform as TranslateTransform;
            tranform.X = x;
            tranform.Y = y;
            this.x = x;
            this.y = y;
        }

        private void Resize(double width, double height)
        {
            DrawSelectionBox(width, height);
        }

        #endregion
    }
}
