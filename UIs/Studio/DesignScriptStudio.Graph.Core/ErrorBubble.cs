using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace DesignScriptStudio.Graph.Core
{
    // The severity of the error type (note that the order of them is 
    // in term of severity, therefore the order cannot be changed).
    enum ErrorType
    {
        None = 0,
        Warning = 1,
        Semantic = 2,
        Syntax = 3,
    }

    class ErrorBubble : InfoBubble
    {
        #region Class Data Members

        private DispatcherTimer timer = null;

        #endregion

        #region Internal Class Properties

        internal ErrorType ErrType { get; private set; }

        #endregion

        #region Internal Class Methods

        internal ErrorBubble(IGraphController graphController, uint nodeId)
            : base(graphController, nodeId)
        {
            this.content = (object)string.Empty;
            this.ErrType = ErrorType.None;
        }

        internal override void SetContent(object content)
        {
            string curContent = this.content as string;
            string newContent = content as string;
            if (newContent != curContent)
            {
                this.content = content;
                graphController.GetVisualNode(this.OwnerId).Dirty = true;
            }
        }

        internal void SetErrorType(ErrorType errType)
        {
            this.ErrType = errType;
            if (this.ErrType == ErrorType.Warning)
                this.StartTimer();
            else if (this.timer != null)
                this.StopTimer();
        }

        internal void Compose()
        {
            GraphController controller = graphController as GraphController;
            IGraphVisualHost visualHost = controller.GetVisualHost();
            if (null == visualHost)
                return; // Running in headless mode, draw no further.

            DrawingVisual bubbleVisual = visualHost.GetDrawingVisualForBubble(this.bubbleId);
            DrawingContext drawingContext = bubbleVisual.RenderOpen();

            string text = this.content as string;
            if (string.IsNullOrEmpty(text))
            {
                drawingContext.Close();
                return;
            }

            if (this.ErrType == ErrorType.None)
                ErrorManager.HandleError(new InvalidOperationException("'errorType' is not set"));

            VisualNode node = graphController.GetVisualNode(this.OwnerId);
            this.anchorPoint.X = node.Width / 2;
            this.anchorPoint.Y = -Configurations.InfoBubbleTopMargin;
            this.anchorPoint = new Point((int)this.anchorPoint.X, (int)this.anchorPoint.Y);
            this.arrowLeft = new Point((int)this.anchorPoint.X - Configurations.InfoBubbleArrowWidthHalf, (int)this.anchorPoint.Y - Configurations.InfoBubbleArrowHeight);
            this.arrowRight = new Point((int)this.anchorPoint.X + Configurations.InfoBubbleArrowWidthHalf, (int)this.anchorPoint.Y - Configurations.InfoBubbleArrowHeight);
            this.arrowLeft.Offset(-1, -2);
            this.arrowRight.Offset(1, -2);

            double textLength;
            int textHorizontalMargin;
            int textVerticalMargin;
            FormattedText newText;
            SolidColorBrush textColor;
            SolidColorBrush backgroundColor;
            Pen borderPen;

            if (this.ErrType == ErrorType.Warning)
            {
                textColor = Configurations.ErrorBubbleWarningTextColor;
                borderPen = Configurations.ErrorBubbleWarningBorderPen;
                backgroundColor = Configurations.ErrorBubbleWarningBackground;
            }
            else
            {
                textColor = Configurations.ErrorBubbleErrorTextColor;
                borderPen = Configurations.ErrorBubbleErrorBorderPen;
                //if (this.node.Selected == true)
                backgroundColor = Configurations.ErrorBubbleErrorBackgroundActive;
                //else
                //    backgroundColor = Configurations.ErrorBubbleErrorBackgroundNonActive;
            }

            textHorizontalMargin = Configurations.InfoBubbleMargin;
            textVerticalMargin = Configurations.InfoBubbleMargin;

            newText = new FormattedText(text,
                                     Configurations.culture,
                                     FlowDirection.LeftToRight,
                                     Configurations.TypeFace,
                                     Configurations.InfoBubbleText,
                                     textColor);

            newText.LineHeight = Configurations.InfoBubbleText;
            textLength = newText.WidthIncludingTrailingWhitespace;

            this.rectWidth = (int)(textLength + textHorizontalMargin * 2);
            this.rectHeight = (int)(textVerticalMargin * 2 + newText.Height);
            if (this.rectWidth < Configurations.InfoBubbleMinWidth)
                this.rectWidth = Configurations.InfoBubbleMinWidth;

            this.rectPoint = new Point((int)(this.anchorPoint.X - this.rectWidth / 2) + 0.5, (int)(this.anchorPoint.Y - Configurations.InfoBubbleArrowHeight - this.rectHeight) + 0.5);

            //Draw Bubble
            base.DrawBubble(drawingContext, backgroundColor, borderPen, true);

            //Draw Text
            drawingContext.DrawText(newText, new Point(this.rectPoint.X + textHorizontalMargin, this.rectPoint.Y + textVerticalMargin));
            drawingContext.Close();

            TranslateTransform transform = bubbleVisual.Transform as TranslateTransform;
            transform.X = node.X;
            transform.Y = node.Y;
        }

        #endregion

        #region Private Class Methods

        private void StartTimer()
        {
            if (timer == null)
            {
                this.timer = new DispatcherTimer();
                this.timer.Tick += new EventHandler(OnWarningTimeOut);
                this.timer.Interval = TimeSpan.FromMilliseconds(3500);
            }
            this.timer.Stop();
            this.timer.Start();
        }

        private void OnWarningTimeOut(object sender, EventArgs e)
        {
            VisualNode node = graphController.GetVisualNode(this.OwnerId);

            //Terminate DispatcherTimer
            this.timer.Stop();
            this.timer = null;

            //Clear state
            this.content = null;
            this.ErrType = ErrorType.None;

            //Redraw
            node.Dirty = true;
            node.Compose();
        }

        private void StopTimer()
        {
            this.timer.Stop();
            this.timer = null;
        }

        #endregion
    }
}