using System;
using System.ComponentModel;
using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    public class Arc : Curve
    {
        internal IArcEntity ArcEntity { get { return HostImpl as IArcEntity; } }

        #region DATA MEMBERS
        private Vector mStartAxis;
        private double? mStartAngle;
        private double? mEndAngle;
        private double? mSweepAngle;
        private Point mFirstPoint;
        private Point mSecondPoint;
        private Point mThirdPoint;
        private Point mCenterPoint;
        private Point mSweepPoint;
        #endregion

        #region OVERRIDES

        protected override Point GetStartPoint()
        {
            return ArcEntity.StartPoint.ToPoint(false, this);
        }

        protected override Point GetEndPoint()
        {
            return ArcEntity.EndPoint.ToPoint(false, this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GeometryExtension.DisposeObject(ref mFirstPoint);
                GeometryExtension.DisposeObject(ref mSecondPoint);
                GeometryExtension.DisposeObject(ref mThirdPoint);
                GeometryExtension.DisposeObject(ref mCenterPoint);
                GeometryExtension.DisposeObject(ref mSweepPoint);
            }
            base.Dispose(disposing);
        }

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Gets the point on curve, if this arc was created using three points
        /// </summary>
        public Point PointOnCurve
        {
            get
            {
                return SecondPoint;
            }
        }

        /// <summary>
        /// Checks if it's circular in shape. Returns true.
        /// </summary>
        public override bool IsCircular
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Checks if this Arc is closed.
        /// </summary>
        public override bool IsClosed
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool IsElliptical
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool IsLinear
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool IsPlanar
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool IsSelfIntersecting
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Point SweepPoint
        {
            get { return mSweepPoint; }
            private set { value.AssignTo(ref mSweepPoint); }
        }

        /// <summary>
        /// 
        /// </summary>
        public Vector StartAxis 
        {
            get
            {
                if (mStartAxis == null)
                {
                    mStartAxis = CenterPoint.DirectionTo(StartPoint).Normalize();
                }
                return mStartAxis;
            }
            private set 
            {
                mStartAxis = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Category("Primary")]
        public double StartAngle 
        {
            get
            {
                if (!mStartAngle.HasValue)
                {
                    mStartAngle = GeometryExtension.RadiansToDegrees(ArcEntity.StartAngle);
                }
                return mStartAngle.Value;
            }
            private set
            {
                mStartAngle = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Category("Primary")]
        public double EndAngle 
        {
            get
            {
                if (!mEndAngle.HasValue)
                {
                    mEndAngle = GeometryExtension.RadiansToDegrees(ArcEntity.SweepAngle + ArcEntity.StartAngle);
                    if (mEndAngle > 360.0)
                        mEndAngle = mEndAngle - 360.0;
                    else if (mEndAngle < 0.0)
                        mEndAngle = mEndAngle + 360.0;
                }
                return mEndAngle.Value;
            }
            private set
            {
                mEndAngle = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Category("Primary")]
        public double SweepAngle 
        {
            get
            {
                if (!mSweepAngle.HasValue)
                {
                    mSweepAngle = GeometryExtension.RadiansToDegrees(ArcEntity.SweepAngle);
                }
                return mSweepAngle.Value;
            }
            private set 
            {
                mSweepAngle = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Point FirstPoint 
        {
            get { return mFirstPoint; }
            private set { value.AssignTo(ref mFirstPoint); }
        }

        /// <summary>
        /// 
        /// </summary>
        public Point SecondPoint
        {
            get { return mSecondPoint; }
            private set { value.AssignTo(ref mSecondPoint); }
        }

        /// <summary>
        /// 
        /// </summary>
        public Point ThirdPoint
        {
            get { return mThirdPoint; }
            private set { value.AssignTo(ref mThirdPoint); }
        }

        /// <summary>
        /// 
        /// </summary>
        [Category("Primary")]
        public Point CenterPoint
        {
            get 
            { 
                if(null == mCenterPoint)
                    mCenterPoint = ArcEntity.CenterPoint.ToPoint(false, this);
                return mCenterPoint; 
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Category("Primary")]
        public double Radius { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public Vector Normal { get; private set; }
        #endregion

        #region PRIVATE_CONSTRUCTORS

        static void InitType()
        {
            RegisterHostType(typeof(IArcEntity), (IGeometryEntity host, bool persist) => { return new Arc(host as IArcEntity, persist); });
        }

        private Arc(IArcEntity entity, bool persist = false)
            : base(entity, persist)
        {
            if (null != entity)
            {
                InitializeGuaranteedProperties();
            }
        }
        #endregion

        #region INTERNAL_METHODS

        private void InitializeGuaranteedProperties()
        {
            Radius = ArcEntity.Radius;
            StartAngle = GeometryExtension.RadiansToDegrees(ArcEntity.StartAngle);
            SweepAngle = GeometryExtension.RadiansToDegrees(ArcEntity.SweepAngle);
            Normal = new Vector(ArcEntity.Normal);
        }

        #endregion

        #region PROTECTED_CONSTRUCTORS
        protected Arc(Point firstPoint, Point secondPoint, Point thirdPoint, bool persist)
            : base(ByPointsOnCurveCore(firstPoint, secondPoint, thirdPoint), persist)
        {
            InitializeGuaranteedProperties();
            FirstPoint = firstPoint;
            SecondPoint = secondPoint;
            ThirdPoint = thirdPoint;
        }
        protected Arc(Point centerPoint, double radius, double startAngle, double sweepAngle, Vector normal, bool persist) 
            : base(ByCenterPointRadiusAngleCore(centerPoint,radius,startAngle,sweepAngle,ref normal), persist)
        {
             InitializeGuaranteedProperties();
             SweepAngle = sweepAngle; // defect to be corrected later - autocad implementation sending negative angle as sweepangle
        }
        protected Arc(Point centerPoint, Point startPoint, Point sweepPoint, bool persist, bool unused)
            : base(ByCenterPointStartPointSweepPointCore(centerPoint, startPoint, sweepPoint),persist)
        {

            InitializeGuaranteedProperties();
            SweepPoint = sweepPoint;
        }
        protected Arc(Point centerPoint, Point startPoint, double sweepAngle, Vector normal,bool persist)
            : base(ByCenterPointStartPointSweepAngleCore(centerPoint, startPoint, sweepAngle, ref normal),persist)
        {
            InitializeGuaranteedProperties();
            FirstPoint = startPoint;
        }
        #endregion

        #region DESIGNSCRIPT_CONSTRUCTORS

        /// <summary>
        /// Constructs an Arc through three points. The three points should not be collinear
        /// </summary>
        /// <param name="firstPoint">First point on the Arc</param>
        /// <param name="secondPoint">Second point on the Arc</param>
        /// <param name="thirdPoint">Third point on the Arc</param>
        /// <returns></returns>
        public static Arc ByPointsOnCurve(Point firstPoint, Point secondPoint, Point thirdPoint)
        {
            return new Arc(firstPoint, secondPoint, thirdPoint, true);
        }

        /// <summary>
        /// Constructs an Arc using center point, radius and sweep angle as input. 
        /// </summary>
        /// <param name="centerPoint">Center point of the circle with which the Arc is to be constructed</param>
        /// <param name="radius">The radius of the circle with which the Arc is to be constructed</param>
        /// <param name="startAngle">The start angle of the Arc with respect to x-axis</param>
        /// <param name="sweepAngle">The angle to be swept by the Arc with respect to the center point</param>
        /// <param name="normal">Normal to the plane of the Arc</param>
        /// <returns></returns>
        public static Arc ByCenterPointRadiusAngle (Point centerPoint, double radius, double startAngle, double sweepAngle, Vector normal) 
        {
            return new Arc(centerPoint, radius, startAngle, sweepAngle, normal, true);           
        }

        /// <summary>
        ///Constructs an Arc using center point, start point and sweep angle with axis aligned along positive Z-axis.
        /// </summary>
        /// <param name="centerPoint">Center point of the circle with which the Arc is to be constructed</param>
        /// <param name="startPoint">The start point of the Arc</param>
        /// <param name="sweepAngle">The angle to be swept by the Arc from the center point-start point axis</param>
        /// <returns></returns>
        public static Arc ByCenterPointStartPointSweepAngle(Point centerPoint, Point startPoint, double sweepAngle)
        {
            var zAxis = new Vector(0,0,1, true);
            return ByCenterPointStartPointSweepAngle(centerPoint, startPoint, sweepAngle, zAxis);
        }

        /// <summary>
        /// Constructs an Arc from a center point, start point and sweep point on a plane defined by the three points. 
        /// </summary>
        /// <param name="centerPoint">Center point of the circle with which the Arc is to be constructed</param>
        /// <param name="startPoint">The start point of the Arc</param>
        /// <param name="sweepPoint">The point to which the Arc will be constructed</param>
        /// <returns></returns>
        public static Arc ByCenterPointStartPointSweepPoint(Point centerPoint, Point startPoint, Point sweepPoint)
        {
            return new Arc(centerPoint,startPoint,sweepPoint,true,true);
        }

        /// <summary>
        /// Constructs an Arc using a center point, a start point and sweep angle in a plane defined by the center point and normal as input.
        /// </summary>
        /// <param name="centerPoint">Center point of the circle with which the Arc is to be constructed</param>
        /// <param name="startPoint">The start point of the Arc</param>
        /// <param name="sweepAngle">The point to which the Arc will be constructed</param>
        /// <param name="normal">The normal to the plane of the Arc</param>
        /// <returns></returns>
        public static Arc ByCenterPointStartPointSweepAngle(Point centerPoint, Point startPoint, double sweepAngle, Vector normal)
        {
            return new Arc(centerPoint,startPoint,sweepAngle,normal,true);
        }

       
        #endregion

        #region CORE_METHODS

        private static IArcEntity ByPointsOnCurveCore(Point firstPoint, Point secondPoint, Point thirdPoint)
        {
            if (firstPoint == null)
            {
                throw new ArgumentNullException("firstPoint");
            }
            else if (secondPoint == null)
            {
                throw new ArgumentNullException("secondPoint");
            }
            else if (thirdPoint == null)
            {
                throw new ArgumentNullException("thirdPoint");
            }
            else if (firstPoint.Equals(secondPoint))
            {
                throw new ArgumentException(string.Format(Properties.Resources.EqualGeometry, "first point", "second point"), "firstPoint, secondPoint");
            }
            else if (secondPoint.Equals(thirdPoint))
            {
                throw new ArgumentException(string.Format(Properties.Resources.EqualGeometry, "second point", "thrid point"), "secondPoint, thridPoint");
            }
            else if (thirdPoint.Equals(firstPoint))
            {
                throw new ArgumentException(string.Format(Properties.Resources.EqualGeometry, "third point", "first point"), "thirdPoint, firstPoint");
            }
            else if (GeometryExtension.ArePointsColinear(firstPoint, secondPoint, thirdPoint))
            {
                throw new ArgumentException(string.Format(Properties.Resources.PointsColinear, "first, second and thrid points"), "firstPoint, secondPoint, thridPoint");
            }

            IArcEntity entity = HostFactory.Factory.ArcByPointsOnCurve(firstPoint.PointEntity, secondPoint.PointEntity, thirdPoint.PointEntity);
            if (null == entity)
                throw new Exception(string.Format(Properties.Resources.OperationFailed, "Arc.ByPointsOnCurve"));
            return entity;
        }
        private static IArcEntity ByCenterPointStartPointSweepAngleCore(Point centerPoint, Point startPoint, double sweepAngle, ref Vector normal)
        {
            if (centerPoint == null)
            {
                throw new ArgumentNullException("centerPoint");
            }
            else if (startPoint == null)
            {
                throw new ArgumentNullException("startPoint");
            }
            else if (normal == null)
            {
                throw new ArgumentNullException("normal");
            }
            else if (normal.IsZeroVector())
            {
                throw new ArgumentException(string.Format(Properties.Resources.IsZeroVector, "normal vector"), "normal");
            }
            else if (centerPoint.Equals(startPoint))
            {
                throw new ArgumentException(string.Format(Properties.Resources.EqualGeometry, "center point", "start point"), "centerPoint, startPoint");
            }
            else if (sweepAngle == 0.0)
            {
                throw new ArgumentException(string.Format(Properties.Resources.IsZeroAngle, "sweep"), "sweepAngle");
            }
            normal = normal.IsNormalized ? normal : normal.Normalize();
            var entity = HostFactory.Factory.ArcByCenterPointStartPointSweepAngle(centerPoint.PointEntity,
                            startPoint.PointEntity, GeometryExtension.DegreesToRadians(sweepAngle), normal.IVector);
            if (null == entity)
                throw new Exception(string.Format(Properties.Resources.OperationFailed, "Arc.ByCenterPointStartPointSweepAngle"));
            return entity;
        }
        private static IArcEntity ByCenterPointRadiusAngleCore(Point centerPoint, double radius, double startAngle, double sweepAngle, ref Vector normal)
        {
            if (centerPoint == null)
            {
                throw new ArgumentNullException("centerPoint");
            }
            else if (normal == null)
            {
                throw new ArgumentNullException("normal");
            }
            else if (radius <= 0.0)
            {
                throw new ArgumentException(string.Format(Properties.Resources.LessThanZero, "radius"));
            }
            else if (normal.IsZeroVector())
            {
                throw new ArgumentException(string.Format(Properties.Resources.IsZeroVector, normal), "normal");
            }
            normal = normal.IsNormalized ? normal : normal.Normalize();
            var endAngle = (startAngle + sweepAngle);
            IArcEntity entity = HostFactory.Factory.ArcByCenterPointRadiusAngle(centerPoint.PointEntity, radius, GeometryExtension.DegreesToRadians(startAngle), GeometryExtension.DegreesToRadians(endAngle), normal.IVector);
            if (null == entity)
                throw new Exception(string.Format(Properties.Resources.OperationFailed, "Arc.ByCenterPointRadiusAngle"));
            return entity;
        }
        private static IArcEntity ByCenterPointStartPointSweepPointCore(Point centerPoint, Point startPoint, Point sweepPoint)
        {
            if (centerPoint == null)
            {
                throw new ArgumentNullException("centerPoint");
            }
            else if (startPoint == null)
            {
                throw new ArgumentNullException("startPoint");
            }
            else if (sweepPoint == null)
            {
                throw new ArgumentNullException("sweepPoint");
            }
            else if (startPoint.Equals(sweepPoint))
            {
                throw new ArgumentException(string.Format(Properties.Resources.EqualGeometry, "start point", "sweep point"), "startPoint, sweepPoint");
            }
            else if (startPoint.Equals(centerPoint))
            {
                throw new ArgumentException(string.Format(Properties.Resources.EqualGeometry, "start point", "center point"), "startPoint, centerPoint");
            }

            var arcEntity = HostFactory.Factory.ArcByCenterPointStartPointSweepPoint(centerPoint.PointEntity, startPoint.PointEntity, sweepPoint.PointEntity);
            if (null == arcEntity)
                throw new Exception(string.Format(Properties.Resources.OperationFailed, "Arc.ByCenterPointStartPointSweepPoint"));
            return arcEntity;
        }

        #endregion

        #region FROM_OBJECT

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected override bool Equals(DesignScriptEntity obj)
        {
            if (base.Equals(obj))
            {
                return true;
            }

            var arc = obj as Arc;
            if (arc == null)
            {
                return false;
            }

            return CenterPoint.Equals(arc.CenterPoint) && Normal.Equals(arc.Normal)
                && GeometryExtension.Equals(Radius, arc.Radius);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Arc(CenterPoint = {0}, Radius = {1}, Normal = {2})", CenterPoint, 
                            Radius.ToString(GeometryExtension.DoublePrintFormat), Normal);
        }
        #endregion
    }
}
