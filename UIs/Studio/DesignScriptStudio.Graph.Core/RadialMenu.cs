using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Animation;
using System.Windows.Input;

namespace DesignScriptStudio.Graph.Core
{

    enum MenuItemAnchor
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    public enum ScrollerStates
    {
        None,
        Edge,
        Hovered,
        Normal
    }

    public enum ScrollerType
    {
        None,
        Up,
        Down
    }


    public class Scroller
    {
        private ScrollerType type;
        public ScrollerStates state;
        // This is the center position of Scroller
        public Point position;

        public ScrollerType Type
        {
            get { return this.type; }
        }

        public Scroller(ScrollerType type)
        {
            this.state = ScrollerStates.Normal;
            this.position = new Point();
            this.type = type;
        }

        internal void Render(DrawingContext context)
        {
            // Renders the RadialMenu Scrollers depending on its state
            // States: Normal, Hovered, Edge

            double arrowWidth = 10;
            double arrowHeight = 5;
            Brush blueBrush = Configurations.BlueBrush;
            Brush whiteBrush = Configurations.WhiteBrush;
            Brush hoverBrush = Configurations.HoverBrush;
            Pen bluePen = new Pen(blueBrush, 2);
            Pen whitePen = new Pen(whiteBrush, 2);

            double scrollerHittestArea = Configurations.ScrollerHittestArea;
            Point p = this.position;
            p.Offset(-scrollerHittestArea / 2, -scrollerHittestArea / 2);
            Rect hitTestArea = new Rect(p, new Size(scrollerHittestArea, scrollerHittestArea));
            context.DrawRectangle(Brushes.Transparent, null, hitTestArea);
            double backHeight = 50;
            double backWidth = 100;
            Point q = this.position;
            q.Offset(-backWidth / 2, -backHeight / 2);
            Rect transparentBackground = new Rect(q, new Size(backWidth, backHeight));
            context.DrawRectangle(Brushes.Transparent, null, transparentBackground);
            if (this.state == ScrollerStates.Normal || this.state == ScrollerStates.Hovered)
            {

                Point arrowLeft = this.position;
                arrowLeft.Offset(-arrowWidth / 2, -arrowHeight + 2);
                Point arrowRight = arrowLeft;
                arrowRight.Offset(arrowWidth, 0);
                Point anchor = this.position;
                anchor.Y += 2;
                if (this.Type == ScrollerType.Up)
                {
                    anchor.Offset(0, -arrowHeight);
                    arrowLeft.Offset(0, arrowHeight);
                    arrowRight.Offset(0, arrowHeight);
                }

                LineSegment[] segments = new LineSegment[] { new LineSegment(arrowLeft, true), new LineSegment(arrowRight, true) };
                PathFigure figure = new PathFigure(anchor, segments, true);
                PathGeometry geo = new PathGeometry(new PathFigure[] { figure });
                if (this.state == ScrollerStates.Hovered)
                {
                    //draw rect
                    double width = Configurations.ScrollerRectWidth;
                    double height = Configurations.ScrollerRectHeight;
                    Rect rect = new Rect();
                    rect.Size = new Size(width, height);
                    Point center = this.position;
                    center.Offset(-width / 2, -height / 2);
                    rect.Location = center;
                    context.DrawRectangle(hoverBrush, null, rect);
                    context.DrawGeometry(whiteBrush, null, geo);
                }
                else
                    context.DrawGeometry(blueBrush, null, geo);

                return;
            }
            else if (this.state == ScrollerStates.Edge)
            {
                Point left = this.position;
                Point right = this.position;

                left.Offset(-arrowWidth / 2, 0);
                right.Offset(arrowWidth / 2, 0);
                context.DrawLine(bluePen, left, right);
            }
            else
                return;
        }
    }

    class MenuItemInfo
    {
        //DataStructure storing info related to a specific item in the menu
        public struct ItemGeometry
        {
            public Geometry text;
            public Geometry background;
            public Geometry rectangleTrailing;
            public Geometry rectangleLeading;
        }
        //Determines which corner of the item will be used as the center for ScaleTransform
        MenuItemAnchor anchor = MenuItemAnchor.BottomRight;
        ItemGeometry itemGeometry;
        Geometry geometry = null;
        double height = 0.0;
        SolidColorBrush color = Configurations.NormalTextColor;

        Point currentPosition;
        Point previousPosition;

        int keywordStartIndex = 0;

        #region Internal Class Properties

        internal bool SeletedFlag { get; set; }
        internal int MenuId { get; set; }
        //Normalized displacement determines how far away the item is from
        //from the current mouse position
        internal double NormalizedDisplacement { get; set; }
        internal double NormalizedWeight { get; set; }
        internal double ItemLength { get; private set; }
        internal double ItemHeight
        {
            get
            {
                return this.height;
            }
            set
            {
                ScaleTransform scaleTransform = GetScaleTransform(value);
                geometry.Transform = scaleTransform;
                itemGeometry.text.Transform = scaleTransform;
                itemGeometry.background.Transform = scaleTransform;
                itemGeometry.rectangleTrailing.Transform = scaleTransform;
                itemGeometry.rectangleLeading.Transform = scaleTransform;
            }
        }
        internal Point ItemPosition
        {
            //Top left corner of the item
            get { return this.currentPosition; }
            set
            {
                this.previousPosition = this.currentPosition;
                this.currentPosition = value;
            }

        }
        internal SolidColorBrush ItemColor
        {
            get { return this.color; }
            set { this.color = value; }
        }

        #endregion

        #region Internal Class Methods

        internal MenuItemInfo(string menuText, string keyword, MenuItemAnchor anchor)
        {
            FormattedText geomItem = new FormattedText(menuText,
                     Configurations.culture,
                     FlowDirection.LeftToRight,
                     Configurations.TypeFace,
                     Configurations.MinimumTextSize,
                     this.color);

            if (this.SetKeyword(keyword, menuText))
                geomItem.SetFontWeight(FontWeights.UltraBlack, this.keywordStartIndex, keyword.Length);
            double length = geomItem.WidthIncludingTrailingWhitespace;
            this.currentPosition = new Point(0, 0);
            this.previousPosition = new Point(0, 0);
            this.anchor = anchor;
            this.ItemPosition = new Point(0, 0);
            this.ItemLength = length;
            double rectHeight = (Configurations.MinimumTextSize + Configurations.MaxHeightIncrement);

            //Each item has a trailing and leading rectangle which is painted with a linear gradient
            //from transparent to white. 
            Rect background = new Rect(this.ItemPosition, new Size(this.ItemLength, rectHeight));
            Rect rectTrailing = new Rect(this.ItemPosition, new Size(Configurations.EndRectangleLength, rectHeight));
            Rect rectLeading = new Rect(this.ItemPosition, new Size(Configurations.EndRectangleLength, rectHeight));
            this.color = Configurations.NormalTextColor;

            itemGeometry.text = geomItem.BuildGeometry(this.ItemPosition);
            itemGeometry.rectangleTrailing = new RectangleGeometry(rectTrailing);
            itemGeometry.rectangleLeading = new RectangleGeometry(rectLeading);
            itemGeometry.background = new RectangleGeometry(background);

            geometry = geomItem.BuildGeometry(this.ItemPosition);
            this.ItemHeight = Configurations.MinimumTextSize;
        }

        internal void Render(DrawingContext drawingContext, bool fadeFlag)
        {
            if (drawingContext != null)
            {
                Transform previousTransform = geometry.Transform;
                Transform previousTextTransform = itemGeometry.text.Transform;
                Transform previousBackgroundTransform = itemGeometry.background.Transform;
                Transform previousTrailingRectTransform = itemGeometry.rectangleTrailing.Transform;
                Transform previousLeadingRectTransform = itemGeometry.rectangleTrailing.Transform;
                Transform translateTransform = new TranslateTransform(ItemPosition.X + GetXOffset(), ItemPosition.Y - ItemHeight);

                TransformGroup transformGroup = new TransformGroup();
                transformGroup.Children.Add(previousTransform);
                transformGroup.Children.Add(translateTransform);
                geometry.Transform = transformGroup;

                transformGroup = new TransformGroup();
                transformGroup.Children.Add(previousTextTransform);
                transformGroup.Children.Add(translateTransform);
                itemGeometry.text.Transform = transformGroup;

                transformGroup = new TransformGroup();
                Transform backgroundTranslateTransform = new TranslateTransform(ItemPosition.X + GetXOffset(), ItemPosition.Y - ItemHeight / 1.25);
                transformGroup.Children.Add(previousBackgroundTransform);
                transformGroup.Children.Add(backgroundTranslateTransform);
                itemGeometry.background.Transform = transformGroup;

                transformGroup = new TransformGroup();
                double backgroundRectLength = (itemGeometry.background.Bounds.Right) - (itemGeometry.background.Bounds.Left);
                Transform TrailingRectTranslateTransform = new TranslateTransform(ItemPosition.X + GetXOffset() + backgroundRectLength, ItemPosition.Y - ItemHeight / 1.25);
                transformGroup.Children.Add(previousTrailingRectTransform);
                transformGroup.Children.Add(TrailingRectTranslateTransform);
                itemGeometry.rectangleTrailing.Transform = transformGroup;

                transformGroup = new TransformGroup();
                backgroundRectLength = (itemGeometry.background.Bounds.Right) - (itemGeometry.background.Bounds.Left);
                Transform LeadingRectTranslateTransform = new TranslateTransform(ItemPosition.X + GetXOffset() - Configurations.EndRectangleLength, ItemPosition.Y - ItemHeight / 1.25);
                transformGroup.Children.Add(previousLeadingRectTransform);
                transformGroup.Children.Add(LeadingRectTranslateTransform);
                itemGeometry.rectangleLeading.Transform = transformGroup;

                //drawingContext.DrawGeometry(this.color, null, geometry);
                LinearGradientBrush trailingBrush = new LinearGradientBrush(Colors.White, Colors.Transparent, 0);
                LinearGradientBrush leadingBrush = new LinearGradientBrush(Colors.Transparent, Colors.White, 0);
                drawingContext.DrawGeometry(Configurations.BackgroundRectColor, null, itemGeometry.background);
                //drawingContext.DrawGeometry(Brushes.Gray, null, itemGeometry.background);
                SolidColorBrush textColor = this.color;
                if (fadeFlag == true && this.SeletedFlag == false)
                    textColor = Configurations.FadedTextColor;
                drawingContext.DrawGeometry(trailingBrush, null, itemGeometry.rectangleTrailing);
                drawingContext.DrawGeometry(leadingBrush, null, itemGeometry.rectangleLeading);
                drawingContext.DrawGeometry(textColor, null, itemGeometry.text);
            }
        }

        internal Point GetNewRadialMenuPosition()
        {
            return this.ItemPosition;
        }

        private bool SetKeyword(string keyword, string text)
        {
            //Sets the keyword which shortlists the items in the menu
            string temp = text;
            text = text.ToLower();
            for (int i = 0; i < temp.Length; i++)
            {
                if (text.StartsWith(keyword))
                {
                    this.keywordStartIndex = i;
                    return true;
                }
                else
                    text = text.Substring(1);
            }
            return false;
        }

        private double GetXOffset()
        {
            //Returns how much the item should be offset in the X direction
            //depending on the anchor
            double result = 0;
            switch (anchor)
            {
                case MenuItemAnchor.BottomLeft:
                    result = 0;
                    break;
                case MenuItemAnchor.BottomRight:
                case MenuItemAnchor.TopRight:
                    result = -this.ItemLength;
                    break;
                default:
                    break;
            }
            return result;
        }

        private ScaleTransform GetScaleTransform(double value)
        {
            this.height = value;
            double scaleFactor = value / Configurations.MinimumTextSize;
            ScaleTransform transform = new ScaleTransform(scaleFactor, scaleFactor);
            switch (anchor)
            {
                case MenuItemAnchor.BottomLeft:
                    break;
                case MenuItemAnchor.BottomRight:
                case MenuItemAnchor.TopRight:
                    transform.CenterX += this.ItemLength;
                    break;
                default:
                    break;
            }
            return transform;
        }

        #endregion

    }

    public class RadialMenu
    {
        Point centerPosition = new Point();
        Point worldCoords = new Point();
        DrawingVisual radialMenuVisual = null;
        uint nodeId = uint.MaxValue;
        double radius = 1.0;
        double startAngle = 180 * Math.PI / 180;
        double endAngle = 90 * Math.PI / 180;
        NodePart nodePart = NodePart.North;
        MenuItemAnchor anchor = MenuItemAnchor.BottomLeft;

        Dictionary<int, string> itemsList = new Dictionary<int, string>();
        Dictionary<int, string> reducedList = new Dictionary<int, string>();

        Point arcStartPoint = new Point();
        Point arcEndPoint = new Point();
        Scroller upScroller;
        Scroller downScroller;

        bool fadeFlag = false;
        LinkedList<MenuItemInfo> visibleMenuItems = new LinkedList<MenuItemInfo>();

        string keyword = string.Empty;

        int index = 0;

        #region Public Class Methods

        public RadialMenu(uint nodeId, Point worldCoords, double radius, Dictionary<int, string> items,
            double startAngle, NodePart part)
        {
            this.startAngle = startAngle * Math.PI / 180;
            this.endAngle = (startAngle - 90) * Math.PI / 180; // 90: the radial menu is a quadrant
            this.nodePart = part;
            this.worldCoords = worldCoords;
            this.nodeId = nodeId;
            this.radius = radius;

            radialMenuVisual = new DrawingVisual();
            radialMenuVisual.Transform = new TranslateTransform(worldCoords.X, worldCoords.Y);

            SetAnchor();
            itemsList = items;
            reducedList = items;

            InitializeMenuItems();

            UpdateItemsArrangement(-1);
            InitializeItemWeight();

            ValidateScrollers();
            Compose();
        }

        public DrawingVisual GetRadialVisual()
        {
            return this.radialMenuVisual;
        }

        public uint GetNodeId()
        {
            return this.nodeId;
        }

        public NodePart GetNodePart()
        {
            return nodePart;
        }

        public int HandleClick(Point mousePosition)
        {
            if (null == visibleMenuItems || (visibleMenuItems.Count <= 0))
                return -1;
            mousePosition.X -= radialMenuVisual.Transform.Value.OffsetX;
            mousePosition.Y -= radialMenuVisual.Transform.Value.OffsetY;

            if (upScroller != null && downScroller != null)
            {
                Scroller tempScroller = new Scroller(ScrollerType.None);
                bool result = ScrollerHitTest(mousePosition, out tempScroller);

                if (tempScroller.Type == ScrollerType.Up)
                {
                    this.HandleScrollUp();
                    return 0;
                }
                else if (tempScroller.Type == ScrollerType.Down)
                {
                    this.HandleScrollDown();
                    return 0;
                }
            }
            if (IsWithinHitTestRegion(mousePosition) == false)
                return -1;

            int itemIndex;
            double min;
            GetClosestItemToAnchor(out itemIndex, out min);
            MenuItemInfo item = visibleMenuItems.ElementAt(itemIndex);
            Point temp;
            int tempId;
            HandleMouseMove(mousePosition, out temp, out tempId);
            return item.MenuId;
        }

        public bool HandleMouseMove(Point mousePosition, out Point newRadialMenuPoint, out int selectedItemId)
        {
            newRadialMenuPoint = new Point();
            selectedItemId = int.MinValue;
            if (null == visibleMenuItems || (visibleMenuItems.Count <= 0))
                return false;
            mousePosition.X -= radialMenuVisual.Transform.Value.OffsetX;
            mousePosition.Y -= radialMenuVisual.Transform.Value.OffsetY;

            if (upScroller != null && downScroller != null)
            {
                Scroller tempScroller = new Scroller(ScrollerType.None);
                if (ScrollerHitTest(mousePosition, out tempScroller))
                {
                    if (tempScroller.state != ScrollerStates.Edge)
                        tempScroller.state = ScrollerStates.Hovered;
                }
                else
                {
                    if (upScroller.state != ScrollerStates.Edge)
                        upScroller.state = ScrollerStates.Normal;
                    if (downScroller.state != ScrollerStates.Edge)
                        downScroller.state = ScrollerStates.Normal;
                }
            }
            if (IsWithinHitTestRegion(mousePosition) == false)
            {
                InitializeMenuItems();
                UpdateItemsArrangement(-1);
                InitializeItemWeight();
                Compose();
                return false;
            }

            int anchorIndex;
            double minWeight;
            double increment;
            Point anchorPoint = GetHorizontalProjectionOnCircle(mousePosition);
            double anchorWeight = NormalizePoint(anchorPoint);
            //GetClosestItemToAnchor(out anchorIndex, out minWeight);

            foreach (MenuItemInfo item in visibleMenuItems)
            {
                item.NormalizedDisplacement = item.NormalizedWeight - anchorWeight;
                increment = Configurations.MaxHeightIncrement * Utilities.GetGaussianValue(item.NormalizedDisplacement * Configurations.AmplifyingFactor);//+some value from gaussian function
                item.ItemHeight = Configurations.MinimumTextSize + increment;
                item.ItemColor = Configurations.NormalTextColor;
                item.SeletedFlag = false;
            }
            GetClosestItemToAnchor(out anchorIndex, out minWeight);
            visibleMenuItems.ElementAt(anchorIndex).ItemColor = Configurations.SelectedTextColor;
            visibleMenuItems.ElementAt(anchorIndex).SeletedFlag = true;
            newRadialMenuPoint = visibleMenuItems.ElementAt(anchorIndex).GetNewRadialMenuPosition();
            selectedItemId = visibleMenuItems.ElementAt(anchorIndex).MenuId;
            //UpdateItemsArrangement(anchorIndex);
            Compose();
            return true;
        }

        public bool HandleTextChanged()
        {
            //call search algorithm
            Dictionary<int, string> temp = reducedList;
            if (keyword == "")
                reducedList = itemsList;
            else
            {
                keyword = keyword.Trim();
                keyword = keyword.ToLowerInvariant();
                reducedList = new Dictionary<int, string>();
                string value;
                foreach (int itemKey in itemsList.Keys)
                {
                    itemsList.TryGetValue(itemKey, out value);
                    string lowerCase = value.ToLowerInvariant();
                    if (lowerCase.Contains(keyword))
                    {
                        reducedList.Add(itemKey, value);
                    }
                }
            }
            if (reducedList.Count == 0)
                reducedList = temp;
            index = 0;
            InitializeMenuItems();
            UpdateItemsArrangement(-1);
            InitializeItemWeight();
            ValidateScrollers();
            this.Compose();
            return false;
        }

        public void UpdateKeyword(KeyEventArgs e)
        {
            Key keyPressed = e.Key;

            if (keyPressed == Key.Escape)
            {
                keyword = "";
            }
            else if (keyPressed == Key.Back)//the backspace key
            {
                if (keyword != string.Empty)
                    keyword = keyword.Remove(keyword.Length - 1);
            }
            else
            {
                char c = Utilities.GetKeyboardCharacter(e);
                keyword += c;
            }
            HandleTextChanged();
        }

        public void HandleScrollUp()
        {
            if (index > 0)
                index--;
            else
                return;
            InitializeMenuItems();
            UpdateItemsArrangement(-1);
            InitializeItemWeight();
            Compose();
        }

        public void HandleScrollDown()
        {
            if (index < reducedList.Count - 7)
                index++;
            else
                return;
            InitializeMenuItems();
            UpdateItemsArrangement(-1);
            InitializeItemWeight();
            Compose();
        }

        public double GetMenuWidth()
        {
            double maxWidth = double.MinValue;
            foreach (string item in itemsList.Values)
            {
                FormattedText tempText = new FormattedText(item,
                    Configurations.culture,
                    FlowDirection.LeftToRight,
                    Configurations.TypeFace,
                    Configurations.TextSize,
                    Configurations.TextNormalColor);

                if (tempText.WidthIncludingTrailingWhitespace > maxWidth)
                    maxWidth = tempText.WidthIncludingTrailingWhitespace;
            }
            return maxWidth * (1 + Configurations.MaxHeightIncrement / Configurations.MinimumTextSize) + Configurations.CircleRadius;
        }

        public Point GetWorldCoords()
        {
            return this.worldCoords;
        }

        public double GetStartAngle()
        {
            return this.startAngle;
        }

        public void Fade()
        {
            this.fadeFlag = true;
        }

        public void UnFade()
        {
            this.fadeFlag = false;
        }

        #endregion

        #region Private Helper Methods

        private bool IsWithinHitTestRegion(Point mousePosition)
        {
            //returns true when the mouse position has passed the arc 
            TranslateTransform radialTransform = this.radialMenuVisual.Transform as TranslateTransform;
            double x = radialTransform.X;
            double y = radialTransform.Y;
            Rect radialBounds = this.radialMenuVisual.ContentBounds;
            radialBounds.Offset(new Vector(x, y));
            double distance = Math.Sqrt(Math.Pow((mousePosition.X - centerPosition.X), 2) + Math.Pow((mousePosition.Y - centerPosition.Y), 2));
            radialBounds.X -= radialMenuVisual.Transform.Value.OffsetX;
            radialBounds.Y -= radialMenuVisual.Transform.Value.OffsetY;
            if (mousePosition.X > radialBounds.TopLeft.X && mousePosition.X < radialBounds.TopRight.X)
                if (mousePosition.Y > radialBounds.TopLeft.Y && mousePosition.Y < radialBounds.BottomRight.Y)
                    if (distance > this.radius - 12)
                        return true;
            return false;
        }

        private bool ScrollerHitTest(Point mousePosition, out Scroller hitScroller)
        {
            //hitScroller.state = ScrollerStates.None;
            //hitScroller.Position = new Point();
            bool result = false;
            Point pt = upScroller.position;
            hitScroller = new Scroller(ScrollerType.None);

            if (IsWithinSrollerRegion(pt, mousePosition))
            {
                hitScroller = upScroller;
                result = true;
            }
            pt = downScroller.position;
            if (IsWithinSrollerRegion(pt, mousePosition))
            {
                hitScroller = downScroller;
                result = true;
            }
            return result;
        }

        private void UpdateScrollerState()
        {
            if (this.upScroller != null && this.downScroller != null)
            {
                if (index == 0)
                {
                    this.upScroller.state = ScrollerStates.Edge;
                    this.downScroller.state = ScrollerStates.Normal;
                }
                else if (index == itemsList.Count - Configurations.RadialMenuMaxItems)
                {
                    this.upScroller.state = ScrollerStates.Normal;
                    this.downScroller.state = ScrollerStates.Edge;
                }
                else
                {
                    this.upScroller.state = ScrollerStates.Normal;
                    this.downScroller.state = ScrollerStates.Normal;
                }
            }
        }

        private bool IsWithinSrollerRegion(Point pt, Point mousePosition)
        {
            double width = 20;
            double height = 10;
            if (mousePosition.X > pt.X - width / 2 && mousePosition.X < pt.X + width / 2)
                if (mousePosition.Y > pt.Y - height / 2 && mousePosition.Y < pt.Y + height / 2)
                    return true;
            return false;
        }

        private void SetAnchor()
        {
            switch (nodePart)
            {
                case NodePart.NorthEast:
                    this.anchor = MenuItemAnchor.BottomLeft;
                    break;
                case NodePart.North:
                case NodePart.NorthWest:
                    this.anchor = MenuItemAnchor.BottomRight;
                    break;
                case NodePart.South:
                    this.anchor = MenuItemAnchor.TopRight;
                    break;
                default:
                    this.anchor = MenuItemAnchor.BottomLeft;
                    break;
            }
        }

        private void InitializeMenuItems()
        {
            visibleMenuItems = new LinkedList<MenuItemInfo>();
            int i = index;

            int itemsCount = reducedList.Count;
            if (itemsCount > Configurations.RadialMenuMaxItems)
                itemsCount = Configurations.RadialMenuMaxItems;
            for (; i < index + itemsCount; i++)
            {
                MenuItemInfo menuItem = new MenuItemInfo(reducedList.ElementAt(i).Value, this.keyword, anchor);
                menuItem.MenuId = reducedList.ElementAt(i).Key;
                menuItem.ItemPosition = centerPosition;
                visibleMenuItems.AddLast(menuItem);
            }
            this.UpdateScrollerState();
            arcStartPoint.X = centerPosition.X + radius * Math.Cos(startAngle);
            arcStartPoint.Y = centerPosition.Y - radius * Math.Sin(startAngle);

            arcEndPoint.X = centerPosition.X + radius * Math.Cos(endAngle);
            arcEndPoint.Y = centerPosition.Y - radius * Math.Sin(endAngle);
        }

        private void InitializeItemWeight()
        {
            double weight;
            foreach (MenuItemInfo item in visibleMenuItems)
            {
                weight = NormalizePoint(item.ItemPosition);
                item.NormalizedWeight = weight;
            }
        }

        private void UpdateItemsArrangement(int anchorIndex)
        {
            if (null == visibleMenuItems || (visibleMenuItems.Count <= 0))
                return;

            Point newPosition = new Point();

            if (anchorIndex != -1)
            {
                newPosition = visibleMenuItems.ElementAt(anchorIndex).ItemPosition;
            }
            else
            {
                double deltaH = Math.Abs((arcEndPoint.Y - arcStartPoint.Y) / 2);
                if (nodePart == NodePart.North || nodePart == NodePart.NorthEast || nodePart == NodePart.NorthWest)
                    newPosition = GetHorizontalProjectionOnCircle(new Point(centerPosition.X, centerPosition.Y - deltaH));
                else
                    newPosition = GetHorizontalProjectionOnCircle(new Point(centerPosition.X, centerPosition.Y + 1.2 * deltaH));
                anchorIndex = visibleMenuItems.Count / 2;
            }
            PositionItems(newPosition, anchorIndex);
        }

        private void GetClosestItemToAnchor(out int index, out double min)
        {
            min = 1.0;
            index = 0;
            for (int i = 0; i < visibleMenuItems.Count; i++)
            {
                if (Math.Abs(visibleMenuItems.ElementAt(i).NormalizedDisplacement) < min)
                {
                    min = Math.Abs(visibleMenuItems.ElementAt(i).NormalizedDisplacement);
                    index = i;
                }
            }
        }

        private void Compose()
        {
            // @TODO(Joy)DONE: Go through each MenuItemInfo, get it to draw itself.
            if (null == visibleMenuItems || (visibleMenuItems.Count <= 0))
                return;
            DrawingContext context = radialMenuVisual.RenderOpen();

            //Rect hitTestArea = new Rect(new Point(0, -140), new Size(300, arcEndPoint.Y - arcStartPoint.Y));
            //LinearGradientBrush brush = new LinearGradientBrush(Colors.Red, Colors.Black, 0);
            ////context.DrawRectangle(Brushes.Transparent, null, hitTestArea);
            //Drawing drawing = new GeometryDrawing();

            //context.DrawRectangle(Brushes.Transparent, null, hitTestArea);
            AddEffect();
            int itemsCount = visibleMenuItems.Count;
            Rect background = new Rect(new Size(150, 150));
            if (nodePart == NodePart.NorthEast)
                background.Location = new Point(this.radius / 2, -(arcEndPoint.Y - arcStartPoint.Y) / 2 - background.Height / 2);
            else if (nodePart == NodePart.North)
                background.Location = new Point(-this.radius / 2 - background.Width, arcEndPoint.Y - 20);
            context.DrawRectangle(Brushes.Transparent, null, background);
            foreach (MenuItemInfo item in visibleMenuItems)
            {
                item.Render(context, fadeFlag);
            }

            DrawArc(context);

            //Rect topScrollerBackground = new Rect(new Size(100, 50));
            //Rect bottomScrollerBackground = new Rect(new Size(100, 50));
            //topScrollerBackground.Location = arcStartPoint;
            //bottomScrollerBackground.Location = arcEndPoint;
            //context.DrawRectangle(Brushes.Yellow, null, topScrollerBackground);
            //context.DrawRectangle(Brushes.Yellow, null, bottomScrollerBackground);
            //Rect startTest = new Rect(arcStartPoint, new Size(5, 5));
            //Rect endTest = new Rect(arcEndPoint, new Size(5, 5));
            //Rect centerTest = new Rect(centerPosition, new Size(5, 5));
            //context.DrawRectangle(Brushes.SkyBlue, null, startTest);
            //context.DrawRectangle(Brushes.Firebrick, null, endTest);
            //context.DrawRectangle(Brushes.Black, null, centerTest);
            //DrawScroller(context, upScroller);
            //DrawScroller(context, downScroller);
            if (upScroller != null && downScroller != null)
            {
                upScroller.Render(context);
                downScroller.Render(context);
            }
            context.Close();
        }

        private void AddEffect()
        {
            DropShadowEffect bacgroundEffect = new DropShadowEffect();
            bacgroundEffect.BlurRadius = Configurations.RadialEffectBlurRadius;
            bacgroundEffect.Color = Colors.White;
            bacgroundEffect.Direction = Configurations.RadialEffectDirection;
            bacgroundEffect.ShadowDepth = Configurations.RadialEffectShadowDepth;
            bacgroundEffect.Opacity = Configurations.Opacity;

            radialMenuVisual.Effect = bacgroundEffect;
        }

        private void DrawArc(DrawingContext context)
        {
            //arc
            Point startPoint = arcStartPoint;
            Point endPoint = arcEndPoint;

            double sValue1 = 0;
            double sValue2 = 0;
            Color sColor1 = Colors.White;
            Color sColor2 = Colors.White;

            double gradientAngle;
            switch (nodePart)
            {
                case NodePart.NorthEast:
                    startPoint.Offset(-10, 5);
                    endPoint.Offset(-12, 5);
                    gradientAngle = -45;
                    sValue1 = 0;
                    sValue2 = 0.45;
                    sColor1 = Colors.Transparent;
                    sColor2 = Configurations.GradientBlue;
                    break;
                case NodePart.North:
                case NodePart.NorthWest:
                    startPoint.Offset(10, 5);
                    endPoint.Offset(12, 5);
                    gradientAngle = 45;
                    sValue1 = 0.2;
                    sValue2 = 0.75;
                    sColor1 = Configurations.GradientBlue;
                    sColor2 = Colors.Transparent;
                    break;
                case NodePart.South:
                    startPoint.Offset(0, -10);
                    endPoint.Offset(12, 0);
                    gradientAngle = 135;
                    sValue1 = 0;
                    sValue2 = 0.4;
                    sColor2 = Configurations.GradientBlue;
                    sColor1 = Colors.Transparent;
                    break;
                default:
                    gradientAngle = -45;
                    break;
            }

            ArcSegment arc = new ArcSegment(startPoint, new Size(this.radius, this.radius), 45, false, SweepDirection.Counterclockwise, true);

            PathFigure pathFigure = new PathFigure();
            pathFigure.StartPoint = endPoint;
            PathSegmentCollection collection = new PathSegmentCollection();
            collection.Add(arc);
            pathFigure.Segments = collection;
            PathFigureCollection figureCollection = new PathFigureCollection();
            figureCollection.Add(pathFigure);
            PathGeometry geom = new PathGeometry();
            geom.Figures = figureCollection;

            RadialGradientBrush radialGradient = new RadialGradientBrush(Configurations.ArcColor, Colors.Transparent);
            radialGradient.SpreadMethod = GradientSpreadMethod.Reflect;
            radialGradient.RadiusX = 0.9;
            radialGradient.RadiusY = 0.9;
            Pen Curve = new Pen(radialGradient, 2);
            context.DrawGeometry(null, Curve, geom);

            //shadow of arc
            startPoint.Offset(0, 1);
            endPoint.Offset(0, 1);
            ArcSegment arc1 = new ArcSegment(startPoint, new Size(this.radius, this.radius), 45, false, SweepDirection.Counterclockwise, true);
            LineSegment p1 = new LineSegment(startPoint, true);
            LineSegment p2 = new LineSegment(endPoint, true);

            pathFigure = new PathFigure();
            pathFigure.StartPoint = endPoint;
            collection = new PathSegmentCollection();
            collection.Add(arc1);
            collection.Add(p1);
            collection.Add(p2);
            pathFigure.Segments = collection;
            figureCollection = new PathFigureCollection();
            figureCollection.Add(pathFigure);
            geom = new PathGeometry();
            geom.Figures = figureCollection;

            GradientStopCollection gradientStopCollection = new GradientStopCollection();
            GradientStop stopValue1 = new GradientStop(sColor1, sValue1);
            GradientStop stopValue2 = new GradientStop(sColor2, sValue2);

            gradientStopCollection.Add(stopValue1);
            gradientStopCollection.Add(stopValue2);

            LinearGradientBrush gradientBrush = new LinearGradientBrush(gradientStopCollection, gradientAngle);
            //context.DrawRectangle(gradientBrush, null, new Rect(startPoint, endPoint));
            context.DrawGeometry(gradientBrush, null, geom);
        }

        private void PositionItems(Point anchorPosition, int anchorItemIndex)
        {
            //Places the item at the anchorIndexItem at the center of the menu
            //Remaining items are place before/after this item
            if (null == visibleMenuItems || (visibleMenuItems.Count <= 0))
                return;

            visibleMenuItems.ElementAt(anchorItemIndex).ItemPosition = anchorPosition;
            Point temp = anchorPosition;
            for (int i = anchorItemIndex; i >= 0; i--)
            {
                visibleMenuItems.ElementAt(i).ItemPosition = temp;
                if (i != 0)
                {
                    temp.Y -= Configurations.ItemSeparation + visibleMenuItems.ElementAt(i - 1).ItemHeight;
                    temp = GetHorizontalProjectionOnCircle(temp);
                }
            }
            temp = anchorPosition;
            for (int i = anchorItemIndex + 1; i < visibleMenuItems.Count; i++)
            {
                temp.Y += Configurations.ItemSeparation + visibleMenuItems.ElementAt(i).ItemHeight;
                temp = GetHorizontalProjectionOnCircle(temp);
                visibleMenuItems.ElementAt(i).ItemPosition = temp;
            }
        }

        private Point DisplacePointOnCircumference(Point pt, double angle)
        {
            //convert to radians
            angle = Math.PI * angle / 180;
            double theta = Math.Atan(pt.Y / pt.X);
            theta = theta + angle;

            double x = Configurations.CircleRadius * Math.Sin(theta);
            double y = Configurations.CircleRadius * Math.Cos(theta);

            x = Math.Abs(x - Math.Abs(pt.X));
            y = Math.Abs(y - Math.Abs(pt.Y));

            if (angle > 0)
            {
                pt.X -= x;
                pt.Y -= y;
            }
            else
            {
                pt.X += x;
                pt.Y += y;
            }
            return pt;
        }

        private double NormalizePoint(Point pt)
        {
            //Converts the item position to an arbitrary value from 0-1
            double deltaY = Math.Abs(pt.Y);
            double deltaX = Math.Abs(pt.X);

            double angle = Math.Atan(deltaY / deltaX);

            double result = (angle - endAngle) / (startAngle - endAngle);

            return Math.Abs(result % 1);
        }

        private Point GetHorizontalProjectionOnCircle(Point pt)
        {
            //Returns the horizontally projected point
            double radius = this.radius;
            double deltaX;
            double deltaY;

            if (pt.Y < centerPosition.Y - radius)
                pt.Y = centerPosition.Y - radius;

            if (pt.Y > centerPosition.Y + radius)
                pt.Y = centerPosition.Y + radius;


            deltaY = centerPosition.Y - pt.Y;
            deltaX = Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(deltaY, 2));

            // -/+ ? 
            switch (nodePart)
            {
                case NodePart.NorthEast:
                    pt.X = centerPosition.X + deltaX;
                    break;
                case NodePart.NorthWest:
                case NodePart.North:
                    pt.X = centerPosition.X - deltaX;
                    break;
                case NodePart.South:
                    pt.X = centerPosition.X - deltaX;
                    break;
                default:
                    pt.X = centerPosition.X + deltaX;
                    break;
            }
            return pt;
        }

        private Point GetRadialProjectionOnCircle(Point pt)
        {
            double deltaX;
            double deltaY;

            deltaY = pt.Y - centerPosition.Y;
            deltaX = pt.X - centerPosition.X;

            double angle = Math.Atan(deltaY / deltaX);
            pt.X = centerPosition.X + Math.Cos(angle) * radius;
            pt.Y = centerPosition.Y + Math.Sin(angle) * radius;
            // -/+ ? 
            return pt;
        }

        private void ValidateScrollers()
        {
            //Checks to see if scrollers are required or not
            if (reducedList.Count > Configurations.RadialMenuMaxItems)
            {
                upScroller = new Scroller(ScrollerType.Up);
                downScroller = new Scroller(ScrollerType.Down);
                GetScrollerPosition(out upScroller.position, out downScroller.position);
            }
            else
            {
                upScroller = null;
                downScroller = null;
            }
        }

        private void DrawScroller(DrawingContext context, Scroller tempScroller)
        {
            if (upScroller.state == ScrollerStates.None || downScroller.state == ScrollerStates.None)
                return;
            double width = 20;
            double height = 10;
            Rect rect = new Rect();
            rect.Size = new Size(width, height);
            Point center = tempScroller.position;
            center.Offset(-width / 2, -height / 2);
            rect.Location = center;

            Brush b = Brushes.White;
            if (tempScroller.state == ScrollerStates.Hovered)
                b = Brushes.Red;
            else if (tempScroller.state == ScrollerStates.Edge)
                b = Brushes.Blue;
            else if (tempScroller.state == ScrollerStates.Normal)
                b = Brushes.Green;

            context.DrawRectangle(b, null, rect);

            Rect r = new Rect(tempScroller.position, new Size(1, 1));
            context.DrawRectangle(Brushes.Black, null, r);

        }

        private void GetScrollerPosition(out Point scrollUp, out Point scrollDown)
        {
            double scrollerOffset = 75;

            scrollUp = centerPosition;
            scrollDown = centerPosition;

            switch (nodePart)
            {
                case NodePart.NorthEast:
                    scrollUp.Offset(scrollerOffset, -this.radius);
                    scrollDown.Offset(scrollUp.X + scrollerOffset, -5);
                    break;
                case NodePart.North:
                case NodePart.NorthWest:
                    scrollUp.Offset(-scrollerOffset, -this.radius);
                    scrollDown.Offset(scrollUp.X - scrollerOffset, 0);
                    break;
                default:
                    break;
            }

            Rect rect = new Rect(scrollUp, new Size(5, 5));
            //context.DrawRectangle(Brushes.DeepSkyBlue, null, rect);
            rect.Location = scrollDown;
            //context.DrawRectangle(Brushes.DeepSkyBlue, null, rect);
        }

        private void CreateInternalRadialMenu()
        {
            //this.internalRadialMenu = new RadialMenu(this.graphController,this.nodeId,
        }
        #endregion

    }
}
