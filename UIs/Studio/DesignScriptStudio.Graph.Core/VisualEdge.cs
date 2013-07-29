using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace DesignScriptStudio.Graph.Core
{
    class VisualEdge : IVisualEdge
    {
        private enum Version { Version0, Current = Version0 }

        #region Class Data Members

        private EdgeController edgeController = null;
        private VisualEdge.Version version;
        private States edgeState = States.None;
        private List<Point> controlPoints = new List<Point>(); //Bezier Segment Control Point

        #endregion

        #region Public Interface Properties

        public uint EdgeId { get; private set; }
        public uint StartSlotId { get; private set; }
        public uint EndSlotId { get; private set; }

        #endregion

        #region Public Interface Methods

        public bool Deserialize(IStorage storage)
        {
            if (storage == null)
                throw new ArgumentNullException("storage");

            if (storage.ReadUnsignedInteger(FieldCode.EdgeSignature) != Configurations.EdgeSignature)
                throw new InvalidOperationException("Invalid input data");

            try
            {
                this.EdgeType = (EdgeType)storage.ReadInteger(FieldCode.EdgeType);
                this.version = (VisualEdge.Version)storage.ReadInteger(FieldCode.EdgeVersion);
                this.EdgeId = storage.ReadUnsignedInteger(FieldCode.EdgeId);
                this.edgeState = (States)storage.ReadInteger(FieldCode.EdgeState);
                this.StartSlotId = storage.ReadUnsignedInteger(FieldCode.StartSlotId);
                this.EndSlotId = storage.ReadUnsignedInteger(FieldCode.EndSlotId);

                int controlPointsCount = storage.ReadInteger(FieldCode.ControlPointsCount);
                this.controlPoints.Clear();
                for (int i = 0; i < controlPointsCount; i++)
                {
                    double ptX = storage.ReadDouble(FieldCode.ControlPointsX);
                    double ptY = storage.ReadDouble(FieldCode.ControlPointsY);
                    Point pt = new Point(ptX, ptY);
                    this.controlPoints.Add(pt);
                }

                this.Dirty = true;
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n Edge deserialization failed.");
                return false;
            }
        }

        public bool Serialize(IStorage storage)
        {
            if (storage == null)
                throw new ArgumentNullException("storage");

            try
            {
                storage.WriteUnsignedInteger(FieldCode.EdgeSignature, Configurations.EdgeSignature);
                storage.WriteInteger(FieldCode.EdgeType, (int)this.EdgeType);
                storage.WriteInteger(FieldCode.EdgeVersion, (int)VisualEdge.Version.Current);
                storage.WriteUnsignedInteger(FieldCode.EdgeId, this.EdgeId);
                storage.WriteInteger(FieldCode.EdgeState, (int)this.edgeState);
                storage.WriteUnsignedInteger(FieldCode.StartSlotId, this.StartSlotId);
                storage.WriteUnsignedInteger(FieldCode.EndSlotId, this.EndSlotId);

                storage.WriteInteger(FieldCode.ControlPointsCount, this.controlPoints.Count);
                foreach (Point pt in this.controlPoints)
                {
                    storage.WriteDouble(FieldCode.ControlPointsX, pt.X);
                    storage.WriteDouble(FieldCode.ControlPointsY, pt.Y);
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n Edge serialization failed.");
                return false;
            }
        }

        public AuditStatus Audit()
        {
            GraphController controller = edgeController.GetGraphController();
            Slot startSlot = controller.GetSlot(this.StartSlotId) as Slot;
            Slot endSlot = controller.GetSlot(this.EndSlotId) as Slot;

            if (startSlot != null && endSlot != null)
            {
                Point startPoint = startSlot.GetPosition();
                Point endPoint = endSlot.GetPosition();
                bool positionChanged = false;

                if (startSlot.SlotType == SlotType.Output)
                {
                    if (startPoint != controlPoints[0] || endPoint != controlPoints[3])
                        positionChanged = true;
                }
                else
                {
                    if (startPoint != controlPoints[3] || endPoint != controlPoints[0])
                        positionChanged = true;
                }

                if (positionChanged)
                {
                    UpdateControlPoint(startPoint, endPoint, startSlot.SlotType != SlotType.Output);
                    //this.Dirty = true;
                    //this.Compose();
                    return AuditStatus.PersistentDataChanged;
                }
            }

            return AuditStatus.NoChange;
        }

        #endregion

        #region Internal Class Properties

        internal EdgeType EdgeType { get; private set; }

        internal bool Dirty
        {
            get { return this.edgeState.HasFlag(States.Dirty); }
            set
            {
                this.edgeState &= ~States.Dirty;
                if (value)
                    this.edgeState |= States.Dirty;
            }
        }

        internal bool PreviewSelected
        {
            get { return edgeState.HasFlag(States.PreviewSelected); }

            set
            {
                if (value) // Caller wants to select edge
                {
                    if (this.edgeState.HasFlag(States.PreviewSelected))
                        return;

                    this.edgeState |= States.PreviewSelected;
                }
                else // If caller wants to deselect the edge
                {
                    if (!this.edgeState.HasFlag(States.PreviewSelected))
                        return;

                    this.edgeState &= ~States.PreviewSelected;
                }

                this.edgeState |= States.Dirty;
            }
        }

        internal bool Selected
        {
            get { return edgeState.HasFlag(States.Selected); }

            set
            {
                if (value) // Caller wants to select edge
                {
                    if (this.edgeState.HasFlag(States.Selected))
                        return;

                    this.edgeState |= States.Selected;
                    this.edgeState |= States.PreviewSelected;
                }
                else // If caller wants to deselect the edge
                {
                    if (!this.edgeState.HasFlag(States.Selected))
                        return;

                    this.edgeState &= ~States.Selected;
                    this.edgeState &= ~States.PreviewSelected;
                }

                this.edgeState |= States.Dirty;
            }
        }

        #endregion

        #region Internal Class Method

        internal static IVisualEdge Create(EdgeController edgeController, IStorage storage)
        {
            if (edgeController == null || storage == null)
                throw new ArgumentNullException("edgeController, storage");

            IVisualEdge edge = new VisualEdge(edgeController);
            edge.Deserialize(storage);
            return edge;
        }

        internal VisualEdge(EdgeController edgeController, EdgeType edgeType)
        {
            this.edgeController = edgeController;
            this.EdgeType = edgeType;
            controlPoints.Add(new Point());
            controlPoints.Add(new Point());
            controlPoints.Add(new Point());
            controlPoints.Add(new Point());
        }

        internal VisualEdge(EdgeController edgeController, uint startSlotId, uint endSlotId, bool isImplicit)
        {
            this.edgeController = edgeController;
            if (isImplicit)
                this.EdgeType = EdgeType.ImplicitConnection;
            else
                this.EdgeType = EdgeType.ExplicitConnection;
            this.version = VisualEdge.Version.Current;
            this.StartSlotId = startSlotId;
            this.EndSlotId = endSlotId;

            // Generate new ID for this visual edge.
            GraphController graphController = edgeController.GetGraphController();
            IdGenerator idGenerator = graphController.GetIdGenerator();
            this.EdgeId = idGenerator.GetNextId(ComponentType.Edge);

            controlPoints.Add(new Point());
            controlPoints.Add(new Point());
            controlPoints.Add(new Point());
            controlPoints.Add(new Point());

            GraphController controller = edgeController.GetGraphController();
            Slot startSlot = controller.GetSlot(this.StartSlotId) as Slot;
            Slot endSlot = controller.GetSlot(this.EndSlotId) as Slot;

            if (startSlot != null && endSlot != null)
            {
                Point startPoint = startSlot.GetPosition();
                Point endPoint = endSlot.GetPosition();
                UpdateControlPoint(startPoint, endPoint, startSlot.SlotType != SlotType.Output);
            }
            else
                throw new ArgumentNullException("startSlot or endSlot");
        }

        internal void Compose()
        {
            if (false == this.Dirty)
                return;

            GraphController controller = this.edgeController.GetGraphController();
            IGraphVisualHost visualHost = controller.GetVisualHost();

            if (null == visualHost)
                return;

            DrawingVisual visual = visualHost.GetDrawingVisualForEdge(this.EdgeId);
            DrawingContext context = visual.RenderOpen();

            DrawCurve(context, this.PreviewSelected);

            context.Close();
        }

        internal void UpdateControlPoint()
        {
            GraphController controller = edgeController.GetGraphController();
            Slot startSlot = controller.GetSlot(this.StartSlotId) as Slot;
            Slot endSlot = controller.GetSlot(this.EndSlotId) as Slot;
            Point startPoint = startSlot.GetPosition();
            Point endPoint = endSlot.GetPosition();

            UpdateControlPoint(startPoint, endPoint, startSlot.SlotType != SlotType.Output);

            this.Dirty = true;
        }

        internal void ComposeOnDrawing(DrawingContext context)
        {
            GraphController controller = edgeController.GetGraphController();

            Slot startSlot = controller.GetSlot(this.StartSlotId) as Slot;
            Slot endSlot = controller.GetSlot(this.EndSlotId) as Slot;
            Point startPoint = startSlot.GetPosition();
            Point endPoint = endSlot.GetPosition();

            UpdateControlPoint(startPoint, endPoint, startSlot.SlotType != SlotType.Output);
            DrawCurve(context, this.PreviewSelected);
        }

        internal void ComposeConnectingEdge(DrawingContext context, Point startPoint, Point endPoint, bool isReversed)
        {
            if (isReversed)
            {
                context.DrawLine(Configurations.GrayPen, new Point(endPoint.X - 1, endPoint.Y - 5), new Point(endPoint.X - 1, endPoint.Y + 5));
                context.DrawLine(Configurations.GrayPen, new Point(endPoint.X, endPoint.Y - 3), new Point(endPoint.X, endPoint.Y + 3));
            }
            else
            {
                context.DrawLine(Configurations.GrayPen, new Point(endPoint.X + 1, endPoint.Y - 5), new Point(endPoint.X + 1, endPoint.Y + 5));
                context.DrawLine(Configurations.GrayPen, new Point(endPoint.X, endPoint.Y - 3), new Point(endPoint.X, endPoint.Y + 3));
            }

            UpdateControlPoint(startPoint, endPoint, isReversed);
            DrawCurve(context, true);
        }

        internal void ReconnectSlots(uint startSlotId, uint endSlotId)
        {
            this.StartSlotId = startSlotId;
            this.EndSlotId = endSlotId;
        }

        internal bool DeterminSelection(Point topLeft, Point bottomRight)
        {
            if (BezierLineIntersect(controlPoints, topLeft, new Point(topLeft.X, bottomRight.Y)))
                return true;
            if (BezierLineIntersect(controlPoints, topLeft, new Point(bottomRight.X, topLeft.Y)))
                return true;
            if (BezierLineIntersect(controlPoints, new Point(topLeft.X, bottomRight.Y), bottomRight))
                return true;
            if (BezierLineIntersect(controlPoints, new Point(bottomRight.X, topLeft.Y), bottomRight))
                return true;

            return false;
        }

        #endregion

        #region Private Helper Methods

        private VisualEdge(EdgeController edgeController)
        {
            this.edgeController = edgeController;
        }

        private void UpdateControlPoint(Point startPoint, Point endPoint, bool isReversed)
        {
            // add the control point to draw the bspline curve
            if (!isReversed)
            {
                controlPoints[0] = startPoint;
                controlPoints[3] = endPoint;
            }
            else
            {
                controlPoints[0] = endPoint;
                controlPoints[3] = startPoint;
            }

            double offsetX = (controlPoints[3].X - controlPoints[0].X) / 4;
            double offsetY = (controlPoints[3].Y - controlPoints[0].Y) / 4;

            if (Math.Abs(offsetX) < Math.Abs(offsetY))
            {
                if (offsetX == 0)
                    offsetX = Math.Abs(offsetY);
                else
                    offsetX = Math.Abs(offsetY) * offsetX / Math.Abs(offsetX);
                offsetY /= 2;
            }
            else if (offsetX > 0)
            {
                offsetY = 0;
            }

            controlPoints[1] = new Point(controlPoints[0].X + Math.Abs(offsetX), controlPoints[0].Y + offsetY);
            controlPoints[2] = new Point(controlPoints[3].X - Math.Abs(offsetX), controlPoints[3].Y - offsetY);
        }

        private void DrawCurve(DrawingContext context, bool isHighlighted)
        {
            PathFigure pathFigure = null;
            pathFigure = new PathFigure();
            pathFigure.StartPoint = controlPoints[0];
            pathFigure.Segments.Add(
                new BezierSegment(controlPoints[1], controlPoints[2], controlPoints[3], true));

            PathGeometry curve = new PathGeometry();
            curve.Figures.Add(pathFigure);

            if (isHighlighted)
                context.DrawGeometry(null, Configurations.GlowPen, curve);

            if (EdgeType == EdgeType.ImplicitConnection)
                context.DrawGeometry(null, Configurations.DashPen, curve);
            else
                context.DrawGeometry(null, Configurations.SolidPen, curve);

            context.DrawGeometry(null, Configurations.TransparentPen, curve);

            //curve shape drawing debugging
            //context.DrawLine(new Pen(Brushes.Black, 1), p2, p3);

            //curve drag selection debugging

            //List<Point> points = new List<Point>();
            //points.Add(p1);
            //points.Add(p2);
            //points.Add(p3);
            //points.Add(p4);
            //List<Point> bezierApprox = BezierApprox(points);
            //List<Point> upperBounding = Offset(bezierApprox, 10);
            //List<Point> lowerBounding = Offset(bezierApprox, -10);
            //for (int i = 1; i < upperBounding.Count(); i++)
            //    context.DrawLine( new Pen(Brushes.Black,1), upperBounding[i - 1], upperBounding[i]);
            //for (int i = 1; i < lowerBounding.Count(); i++)
            //    context.DrawLine(new Pen(Brushes.Black, 1), lowerBounding[i - 1], lowerBounding[i]);
        }

        private bool LineLineIntersect(Point a1, Point a2, Point b1, Point b2)
        {
            double ua_t = (b2.X - b1.X) * (a1.Y - b1.Y) - (b2.Y - b1.Y) * (a1.X - b1.X);
            double ub_t = (a2.X - a1.X) * (a1.Y - b1.Y) - (a2.Y - a1.Y) * (a1.X - b1.X);
            double u_b = (b2.Y - b1.Y) * (a2.X - a1.X) - (b2.X - b1.X) * (a2.Y - a1.Y);

            if (u_b != 0)
            {
                double ua = ua_t / u_b;
                double ub = ub_t / u_b;

                if (0 <= ua && ua <= 1 && 0 <= ub && ub <= 1)
                    return true;
                else
                    return false;
            }
            else
            {
                if (ua_t == 0 || ub_t == 0)
                    return true; //Coincident
                else
                    return false; //Parallel
            }
        }

        //private bool BezierLineIntersect(Point p1, Point p2, Point p3, Point p4, Point a1, Point a2)
        //{
        //    Vector c3, c2, c1, c0;   // coefficients of cubic
        //    double cl;               // c coefficient for normal form of line
        //    Vector n;                // normal for normal form of line
        //    Point min = new Point(Math.Min(a1.X, a2.X), Math.Min(a1.Y, a2.Y)); // used to determine if point is on line segment
        //    Point max = new Point(Math.Max(a1.X, a2.X), Math.Max(a1.Y, a2.Y)); // used to determine if point is on line segment
        //    // Start with Bezier using Bernstein polynomials for weighting functions:
        //    //     (1-t)^3P1 + 3t(1-t)^2P2 + 3t^2(1-t)P3 + t^3P4
        //    //
        //    // Expand and collect terms to form linear combinations of original Bezier
        //    // controls.  This ends up with a vector cubic in t:
        //    //     (-P1+3P2-3P3+P4)t^3 + (3P1-6P2+3P3)t^2 + (-3P1+3P2)t + P1
        //    //             /\                  /\                /\       /\
        //    //             ||                  ||                ||       ||
        //    //             c3                  c2                c1       c0

        //    // Calculate the coefficients
        //    c3 = new Vector(-p1.X + 3 * p2.X - 3 * p3.X + p4.X, -p1.Y + 3 * p2.Y - 3 * p3.Y + p4.Y);
        //    c2 = new Vector(3 * p1.X - 6 * p2.X + 3 * p3.X, 3 * p1.Y - 6 * p2.Y + 3 * p3.Y);
        //    c1 = new Vector(-3 * p1.X + 3 * p2.X, -3 * p1.Y + 3 * p2.Y);
        //    c0 = new Vector(p1.X, p1.Y);

        //    // Convert line to normal form: ax + by + c = 0
        //    // Find normal to line: negative inverse of original line's slope
        //    n = new Vector(a1.Y - a2.Y, a2.X - a1.X);

        //    // Determine new c coefficient
        //    cl = a1.X * a2.Y - a2.X * a1.Y;

        //    // ?Rotate each cubic coefficient using line for new coordinate system?
        //    // Find roots of rotated cubic
        //    List<double> roots = GetPolynomialRoot(Vector.Multiply(n, c3), Vector.Multiply(n, c2),
        //                                        Vector.Multiply(n, c1), Vector.Multiply(n, c0) + cl);

        //    // Any roots in closed interval [0,1] are intersections on Bezier, but
        //    // might not be on the line segment.
        //    // Find intersections and calculate point coordinates
        //    for (int i = 0; i < roots.Count; i++)
        //    {
        //        double t = roots[i];

        //        if (0 <= t && t <= 1)
        //        {
        //            // We're within the Bezier curve
        //            // Find point on Bezier
        //            Point p5 = Interpolation(p1, p2, t);
        //            Point p6 = Interpolation(p2, p3, t);
        //            Point p7 = Interpolation(p3, p4, t);

        //            Point p8 = Interpolation(p5, p6, t);
        //            Point p9 = Interpolation(p6, p7, t);

        //            Point p10 = Interpolation(p8, p9, t);

        //            // See if point is on line segment
        //            // Had to make special cases for vertical and horizontal lines due
        //            // to slight errors in calculation of p10
        //            if (a1.X == a2.X)
        //            {
        //                if (min.Y <= p10.Y && p10.Y <= max.Y)
        //                {
        //                    return true;
        //                }
        //            }
        //            else if (a1.Y == a2.Y)
        //            {
        //                if (min.X <= p10.X && p10.X <= max.Y)
        //                {
        //                    return true;
        //                }
        //            }
        //            else if (p10.X >= min.X && p10.Y >= min.Y && p10.X <= max.X && p10.Y <= max.Y)
        //            {
        //                return true;
        //            }
        //        }
        //    }
        //    return false;
        //}

        private bool BezierLineIntersect(List<Point> points, Point a1, Point a2)
        {
            List<Point> bezierApprox = BezierApprox(points);
            List<Point> upperBounding = Offset(bezierApprox, 2);
            List<Point> lowerBounding = Offset(bezierApprox, -2);

            if (BezierSubLineIntersect(upperBounding, a1, a2) && BezierSubLineIntersect(lowerBounding, a1, a2))
                return true;
            else
                return false;
        }

        private bool BezierSubLineIntersect(List<Point> points, Point a1, Point a2)
        {
            for (int i = 1; i < points.Count(); i++)
            {
                if (LineLineIntersect(points[i - 1], points[i], a1, a2))
                    return true;
            }

            return false;
        }

        private List<Point> BezierApprox(List<Point> points)
        {
            if (points.Count() < 6)
            {
                List<Point> interPoints = new List<Point>();
                interPoints.Add(points[0]);
                for (int i = 1; i < points.Count(); i++)
                    interPoints.Add(Interpolation(points[i - 1], points[i], 0.5));
                interPoints.Add(points.Last());

                return BezierApprox(interPoints);
            }
            else
            {
                List<Point> interPoints = new List<Point>();
                interPoints.Add(points[0]);
                interPoints.Add(Interpolation(points[0], points[1], 0.5));
                interPoints.Add(Interpolation(points[1], points[2], 0.2));
                interPoints.Add(Interpolation(points[points.Count() - 3], points[points.Count() - 2], 0.8));
                interPoints.Add(Interpolation(points[points.Count() - 2], points.Last(), 0.5));
                interPoints.Add(points.Last());

                points.RemoveRange(0, 2);
                points.RemoveRange(points.Count() - 2, 2);
                interPoints.InsertRange(3, points);

                return interPoints;
            }
        }

        private List<Point> Offset(List<Point> points, double offset)
        {
            List<Point> newPoints = new List<Point>();

            if ((points[0].X - points.Last().X) * (points[0].Y - points.Last().Y) > 0)
            {
                foreach (Point point in points)
                {
                    newPoints.Add(new Point(point.X + offset, point.Y - offset));
                    //point.Offset(offset, -offset);
                }
            }
            else
            {
                foreach (Point point in points)
                {
                    newPoints.Add(new Point(point.X + offset, point.Y + offset));
                    //point.Offset(offset, offset);
                }
            }
            return newPoints;
        }

        //private List<double> GetPolynomialRoot(double c3, double c2, double c1, double c0)
        //{
        //    double torlerance = 0.000001;
        //    List<double> results = new List<double>();

        //    c2 = c2 / c3;
        //    c1 = c1 / c3;
        //    c0 = c0 / c3;

        //    double a = (3 * c1 - c2 * c2) / 3;
        //    double b = (2 * c2 * c2 * c2 - 9 * c1 * c2 + 27 * c0) / 27;
        //    double offset = c2 / 3;
        //    double discrim = b * b / 4 + a * a * a / 27;
        //    double halfB = b / 2;

        //    if (Math.Abs(discrim) <= torlerance)
        //        discrim = 0;

        //    if (discrim > 0)
        //    {
        //        double e = Math.Sqrt(discrim);
        //        double tmp;
        //        double root;

        //        tmp = -halfB + e;
        //        if (tmp >= 0)
        //            root = Math.Pow(tmp, 1 / 3);
        //        else
        //            root = -Math.Pow(-tmp, 1 / 3);

        //        tmp = -halfB - e;
        //        if (tmp >= 0)
        //            root += Math.Pow(tmp, 1 / 3);
        //        else
        //            root -= Math.Pow(-tmp, 1 / 3);

        //        results.Add(root - offset);
        //    }
        //    else if (discrim < 0)
        //    {
        //        double distance = Math.Sqrt(-a / 3);
        //        double angle = Math.Atan2(Math.Sqrt(-discrim), -halfB) / 3;
        //        double cos = Math.Cos(angle);
        //        double sin = Math.Sin(angle);
        //        double sqrt3 = Math.Sqrt(3);

        //        results.Add(2 * distance * cos - offset);
        //        results.Add(-distance * (cos + sqrt3 * sin) - offset);
        //        results.Add(-distance * (cos - sqrt3 * sin) - offset);
        //    }
        //    else
        //    {
        //        double tmp;

        //        if (halfB >= 0)
        //            tmp = -Math.Pow(halfB, 1 / 3);
        //        else
        //            tmp = Math.Pow(-halfB, 1 / 3);

        //        results.Add(2 * tmp - offset);
        //        // really should return next root twice, but we return only one
        //        results.Add(-tmp - offset);
        //    }
        //    return results;
        //}

        private Point Min(Point p1, Point p2)
        {
            return new Point(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y));
        }

        private Point Max(Point p1, Point p2)
        {
            return new Point(Math.Max(p1.X, p2.X), Math.Max(p1.Y, p2.Y));
        }

        private Point Interpolation(Point p1, Point p2, double percentage)
        {
            return new Point(p1.X + (p2.X - p1.X) * percentage, p1.Y + (p2.Y - p1.Y) * percentage);
        }

        #endregion
    }
}
