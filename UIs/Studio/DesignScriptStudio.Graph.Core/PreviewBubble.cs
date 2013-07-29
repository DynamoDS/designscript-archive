using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;

namespace DesignScriptStudio.Graph.Core
{
    enum PreviewTypes
    {
        None = 0x00,
        String = 0x01,
        Bitmap = 0x02,
        Unknown = 0x04,
        Extended = 0x10,
        Collapsed = 0x20,
    }

    class PreviewBubble : InfoBubble
    {
        internal PreviewTypes PreviewType
        {
            get { return this.previewType; }
        }

        public bool Extended
        {
            get
            {
                return this.previewType.HasFlag(PreviewTypes.Extended);
            }
        }

        #region Class Data Members

        PreviewTypes previewType = PreviewTypes.None;
        private List<string> condensedString;
        private double extendedBubbleWidth = double.MinValue;
        private double extendedBubbleHeight = double.MinValue;

        #endregion

        #region Internal Class Methods

        internal PreviewBubble(IGraphController graphController, uint nodeId)
            : base(graphController, nodeId)
        {

        }

        internal void SetUnknownPreview()
        {
            this.content = UiStrings.LoadingPreviewMessage;
            previewType = PreviewTypes.Unknown;
            previewType |= PreviewTypes.Collapsed;
            condensedString = Condense(content.ToString());
            this.extendedBubbleWidth = double.MinValue;
            this.extendedBubbleHeight = double.MinValue;
        }

        internal override void SetContent(object content)
        {
            this.content = content;
            previewType &= ~PreviewTypes.String;
            previewType &= ~PreviewTypes.Bitmap;
            previewType &= ~PreviewTypes.Unknown;
            VisualNode node = graphController.GetVisualNode(this.nodeId);

            DesignScriptStudio.Renderer.ThumbnailData thumbnailData = null;
            thumbnailData = content as DesignScriptStudio.Renderer.ThumbnailData;
            if (null != thumbnailData)
            {
                int stride = thumbnailData.width * 4;
                this.content = BitmapSource.Create(thumbnailData.width, thumbnailData.height,
                    96, 96, PixelFormats.Bgra32, null, thumbnailData.pixels, stride);

                previewType |= PreviewTypes.Bitmap;
            }
            else if (null != content as BitmapSource)
            {
                this.content = content as BitmapSource;
                previewType |= PreviewTypes.Bitmap;
            }
            else
            {
                if (content == null)
                    condensedString = null;
                else
                    condensedString = Condense(content.ToString());

                previewType |= PreviewTypes.String;
            }

            this.extendedBubbleWidth = double.MinValue;
            this.extendedBubbleHeight = double.MinValue;
            node.Dirty = true;
        }

        internal double GetExtendedBubbleWidth()
        {
            if (extendedBubbleWidth < 0)
                extendedBubbleWidth = base.rectWidth;

            return extendedBubbleWidth;
        }

        internal double GetExtendedBubbleHeight()
        {
            return extendedBubbleHeight;
        }

        internal void SetExtendedBubbleSize(double width, double height)
        {
            this.extendedBubbleWidth = width;
            this.extendedBubbleHeight = height;
        }

        internal void Extend(bool extend)
        {
            if (previewType.HasFlag(PreviewTypes.Bitmap))
                return;

            if (!extend)
            {
                if (this.previewType.HasFlag(PreviewTypes.Extended))
                {
                    this.previewType &= ~PreviewTypes.Extended;
                    this.Compose();
                }
            }
            else
            {
                if (!this.previewType.HasFlag(PreviewTypes.Extended))
                {
                    this.previewType |= PreviewTypes.Extended;
                    this.Compose();
                }
            }
        }

        internal void Collapse(bool collapse)
        {
            if (!collapse)
            {
                if (previewType.HasFlag(PreviewTypes.Collapsed))
                {
                    this.previewType &= ~PreviewTypes.Collapsed;
                    this.Compose();
                }
            }
            else
            {
                if (!previewType.HasFlag(PreviewTypes.Collapsed))
                {
                    this.previewType |= PreviewTypes.Collapsed;
                    this.Compose();
                }
            }
        }

        internal bool Hittest(Point mousePosition)
        {
            Point pt = this.anchorPoint;
            pt.Offset(this.rectWidth / 2, 0);

            if (mousePosition.X > pt.X - Configurations.HittestPixels && mousePosition.X <= pt.X)
                if (mousePosition.Y >= pt.Y && mousePosition.Y <= pt.Y + Configurations.HittestPixels + Configurations.TextVerticalOffset)
                    return true;
            return false;
        }

        internal void Compose()
        {
            GraphController controller = graphController as GraphController;
            IGraphVisualHost visualHost = controller.GetVisualHost();
            if (null == visualHost)
                return; // Running in headless mode, draw no further.

            VisualNode node = graphController.GetVisualNode(nodeId);

            DrawingVisual bubbleVisual = visualHost.GetDrawingVisualForBubble(this.bubbleId);
            DrawingContext drawingContext = bubbleVisual.RenderOpen();

            if (content == null)
            {
                drawingContext.Close();
                return;
            }
            anchorPoint.X = node.GetBubbleXPosition();
            anchorPoint.Y = node.Height + Configurations.InfoBubbleTopMargin;
            anchorPoint = new Point((int)anchorPoint.X, (int)anchorPoint.Y);
            arrowLeft = new Point((int)(anchorPoint.X - Configurations.InfoBubbleArrowWidthHalf), (int)(anchorPoint.Y + Configurations.InfoBubbleArrowHeight));
            arrowRight = new Point((int)(anchorPoint.X + Configurations.InfoBubbleArrowWidthHalf), (int)(anchorPoint.Y + Configurations.InfoBubbleArrowHeight));

            if (this.previewType.HasFlag(PreviewTypes.Collapsed))
            {
                arrowLeft.Y = anchorPoint.Y;
                anchorPoint.Y = arrowRight.Y;
                arrowRight.Y = arrowLeft.Y;
            }
            else
            {
                arrowLeft.Offset(-1, 2);
                arrowRight.Offset(1, 2);
            }

            if (this.previewType.HasFlag(PreviewTypes.String)
                || this.previewType.HasFlag(PreviewTypes.Unknown))
                this.ComposeValue(drawingContext);
            else if (this.previewType.HasFlag(PreviewTypes.Bitmap))
                this.ComposeGeo(drawingContext);
            drawingContext.Close();

            TranslateTransform transform = bubbleVisual.Transform as TranslateTransform;
            transform.X = node.X;
            transform.Y = node.Y;
        }

        #endregion

        #region Private Class Methods

        private void ComposeValue(DrawingContext drawingContext)
        {
            Brush backgroundColor = null;
            Pen borderPen = null;
            if (!previewType.HasFlag(PreviewTypes.Extended))  // only need to condensed bubble
            {
                string text = Configurations.PreviewBubbleTextDefault;
                string text2 = "";
                if (condensedString != null && condensedString.Count > 0)
                {
                    text = condensedString[0];
                    if (condensedString.Count() > 1)
                        text2 = condensedString[1];
                }

                FormattedText newText;
                FormattedText newText2;
                double textLength;

                SolidColorBrush textColor;
                int textHorizontalMargin = Configurations.InfoBubbleMargin;
                int textVerticalMargin = Configurations.InfoBubbleMargin;

                textColor = Configurations.PreviewBubbleCondensedTextColor;

                newText = new FormattedText(text,
                                         Configurations.culture,
                                         FlowDirection.LeftToRight,
                                         Configurations.TypeFace,
                                         Configurations.InfoBubbleText,
                                         textColor);
                newText2 = new FormattedText(text2,
                                         Configurations.culture,
                                         FlowDirection.LeftToRight,
                                         Configurations.TypeFace,
                                         Configurations.InfoBubbleText,
                                         textColor);

                if (this.content == null)
                    newText.SetFontStyle(FontStyles.Italic);

                newText.LineHeight = Configurations.InfoBubbleText;
                textLength = Math.Max(newText.WidthIncludingTrailingWhitespace, newText2.WidthIncludingTrailingWhitespace);

                rectWidth = (int)(textLength + 2 * textHorizontalMargin);
                if (rectWidth < Configurations.InfoBubbleMinWidth)
                {
                    rectWidth = Configurations.InfoBubbleMinWidth;
                }
                if (this.previewType.HasFlag(PreviewTypes.Extended))
                {
                    rectWidth += Configurations.PreviewBubbleExtendedScrollBarWidth;
                    if (rectWidth > Configurations.InfoBubbleMaxWidth)
                        rectWidth = Configurations.InfoBubbleMaxWidth;
                }

                rectHeight = (int)(textVerticalMargin * 2 + newText.Height);
                if (text2 != "")
                    rectHeight += Configurations.InfoBubbleText + textVerticalMargin;
                if (rectHeight > Configurations.InfoBubbleMaxHeight)
                    rectHeight = Configurations.InfoBubbleMaxHeight;

                backgroundColor = Configurations.PreviewBubbleCondensedBackground;
                borderPen = Configurations.PreviewBubbleCondensedBorderPen;

                rectPoint = new Point((int)(anchorPoint.X - rectWidth / 2) + 0.5, (int)(anchorPoint.Y + Configurations.InfoBubbleArrowHeight) + 0.5);

                //Draw Bubble
                base.DrawBubble(drawingContext, backgroundColor, borderPen, !this.previewType.HasFlag(PreviewTypes.Collapsed));

                if (!this.previewType.HasFlag(PreviewTypes.Collapsed))
                {
                    //Draw text only in the condensed mode
                    double newTextWidth = newText.WidthIncludingTrailingWhitespace;
                    double newText2Width = newText2.WidthIncludingTrailingWhitespace;

                    if (text2 == "")
                    {
                        drawingContext.DrawText(newText, new Point(anchorPoint.X - newTextWidth / 2, rectPoint.Y + textVerticalMargin));
                    }
                    else
                    {
                        drawingContext.DrawText(newText, new Point(rectPoint.X + Configurations.InfoBubbleMargin, rectPoint.Y + textVerticalMargin));
                        drawingContext.DrawText(newText2, new Point(rectPoint.X + Configurations.InfoBubbleMargin, rectPoint.Y + textVerticalMargin + newText.Height));
                    }

                    //Draw Context Menu
                    Point p = new Point(rectPoint.X + rectWidth, rectPoint.Y);
                    p.Offset(0, Configurations.ContextMenuMargin);
                    DrawingUtilities.DrawDots(drawingContext, DotTypes.Top | DotTypes.TopRight | DotTypes.MiddleRight, p, Core.AnchorPoint.TopRight, false);
                }
            }
        }

        private void ComposeGeo(DrawingContext drawingContext)
        {
            rectWidth = Configurations.PreviewBubbleGeoSize;
            rectHeight = Configurations.PreviewBubbleGeoSize;
            BitmapSource image = this.content as BitmapSource;
            if (null != image)
            {
                rectWidth = image.PixelWidth;
                rectHeight = image.PixelHeight;
            }

            rectPoint = new Point((int)(anchorPoint.X - rectWidth / 2),
                (int)(anchorPoint.Y + Configurations.InfoBubbleArrowHeight));

            //Draw Bubble
            Brush backgroundColor;
            Pen borderPen;
            if (this.previewType.HasFlag(PreviewTypes.Collapsed))
            {
                backgroundColor = Configurations.PreviewBubbleMinGeoBackground;
                borderPen = borderPen = Configurations.PreviewBubbleMinGeoBorderPen;
            }
            else
            {
                backgroundColor = Configurations.PreviewBubbleGeoBackground;
                borderPen = borderPen = Configurations.PreviewBubbleGeoBorderPen;
            }
            base.DrawBubble(drawingContext, backgroundColor, borderPen, !this.previewType.HasFlag(PreviewTypes.Collapsed));

            if (!this.previewType.HasFlag(PreviewTypes.Collapsed))
            {
                Rect newRect = new Rect(rectPoint, new Size(rectWidth, rectHeight));
                // @TODO(Ben): Remove "ResourceNames.RadialBlueImagePath" if needed.
                // string imagePath = ResourceNames.RadialBlueImagePath;
                drawingContext.DrawImage(image, newRect);

                //Draw Context Menu
                Point p = new Point(rectPoint.X + rectWidth, rectPoint.Y);
                p.Offset(0, Configurations.ContextMenuMargin);
                DrawingUtilities.DrawDots(drawingContext, DotTypes.Top | DotTypes.TopRight | DotTypes.MiddleRight, p, Core.AnchorPoint.TopRight, false);
            }
        }

        private List<string> Condense(string extendedString)
        {
            List<string> condensedStrings = new List<string>();

            string temp = CondenseNumbers(extendedString);

            if (extendedString[0] == '{')  // Collection
            {
                if (temp.Count() < 100)
                    condensedStrings.Add(temp);
                else
                    condensedStrings = CondenseString(temp);
            }
            else
            {
                condensedStrings = CondenseString(temp);
            }

            return condensedStrings;
        }

        private List<string> CondenseString(string extendedString)
        {
            List<string> condensedStrings = new List<string>();

            if (extendedString.Count() < 50)
                condensedStrings.Add(extendedString);
            else if (extendedString.Count() < 100)
            {
                int half = extendedString.Count() / 2;
                condensedStrings.Add(extendedString.Substring(0, half));
                condensedStrings.Add(extendedString.Substring(half));
            }
            else
            {
                condensedStrings.Add(extendedString.Substring(0, 47) + "...");
                condensedStrings.Add(extendedString.Substring(extendedString.Count() - 1 - 50));
            }

            return condensedStrings;
        }

        private string CondenseNumbers(string extendedString)
        {
            string sourceString = extendedString;
            string targetString = "";

            string[] split = extendedString.Split(new Char[] { ' ', ',', '{', '}', '(', ')' });

            for (int i = 0; i < split.Count(); i++)
            {
                double n;
                if (string.IsNullOrEmpty(split[i]))
                    continue;

                string temp;
                int index;

                if (double.TryParse(split[i], out n))
                    temp = CondenseDigit(split[i]);
                else
                    temp = split[i];

                index = sourceString.IndexOf(split[i]);
                targetString += sourceString.Substring(0, index);
                targetString += temp;
                sourceString = sourceString.Substring(index + split[i].Length, sourceString.Length - split[i].Length - index);
            }
            targetString += sourceString;

            return targetString;
        }

        private string CondenseDigit(string number)
        {
            string condensedNumber = number;
            if (number.Contains("."))
            {
                condensedNumber = condensedNumber.Substring(0, condensedNumber.Length - 3);

                while (condensedNumber.Last() == '0')
                    condensedNumber = condensedNumber.Substring(0, condensedNumber.Length - 1);
                if (condensedNumber.Last() == '.')
                    condensedNumber = condensedNumber.Substring(0, condensedNumber.Length - 1);
            }
            return condensedNumber;
        }

        #endregion
    }
}