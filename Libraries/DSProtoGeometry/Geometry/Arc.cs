using System;
using System.ComponentModel;
using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    public class DSArc : DSCurve
    {
        internal IArcEntity ArcEntity { get { return HostImpl as IArcEntity; } }

        #region DATA MEMBERS
        private DSVector mStartAxis;
        private double? mStartAngle;
        private double? mEndAngle;
        private double? mSweepAngle;
        private DSPoint mFirstPoint;
        private DSPoint mSecondPoint;
        private DSPoint mThirdPoint;
        private DSPoint mCenterPoint;
        private DSPoint mSweepPoint;
        #endregion

        #region OVERRIDES

        protected override DSPoint GetStartPoint()
        {
            return ArcEntity.StartPoint.ToPoint(false, this);
        }

        protected override DSPoint GetEndPoint()
        {
            return ArcEntity.EndPoint.ToPoint(false, this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DSGeometryExtension.DisposeObject(ref mFirstPoint);
                DSGeometryExtension.DisposeObject(ref mSecondPoint);
                DSGeometryExtension.DisposeObject(ref mThirdPoint);
                DSGeometryExtension.DisposeObject(ref mCenterPoint);
                DSGeometryExtension.DisposeObject(ref mSweepPoint);
            }
            base.Dispose(disposing);
        }

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Gets the point on curve, if this arc was created using three points
        /// </summary>
        public DSPoint PointOnCurve
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
        public DSPoint SweepPoint
        {
            get { return mSweepPoint; }
            private set { value.AssignTo(ref mSweepPoint); }
        }

        /// <summary>
        /// 
        /// </summary>
        public DSVector StartAxis 
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
                    mStartAngle = DSGeometryExtension.RadiansToDegrees(ArcEntity.StartAngle);
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
                    mEndAngle = DSGeometryExtension.RadiansToDegrees(ArcEntity.SweepAngle + ArcEntity.StartAngle);
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
                    mSweepAngle = DSGeometryExtension.RadiansToDegrees(ArcEntity.SweepAngle);
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
        public DSPoint FirstPoint 
        {
            get { return mFirstPoint; }
            private set { value.AssignTo(ref mFirstPoint); }
        }

        /// <summary>
        /// 
        /// </summary>
        public DSPoint SecondPoint
        {
            get { return mSecondPoint; }
            private set { value.AssignTo(ref mSecondPoint); }
        }

        /// <summary>
        /// 
        /// </summary>
        public DSPoint ThirdPoint
        {
            get { return mThirdPoint; }
            private set { value.AssignTo(ref mThirdPoint); }
        }

        /// <summary>
        /// 
        /// </summary>
        [Category("Primary")]
        public DSPoint CenterPoint
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
        public DSVector Normal { get; private set; }
        #endregion

        #region PRIVATE_CONSTRUCTORS

        static void InitType()
        {
            RegisterHostType(typeof(IArcEntity), (IGeometryEntity host, bool persist) => { return new DSArc(host as IArcEntity, persist); });
        }

        private DSArc(IArcEntity entity, bool persist = false)
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
            StartAngle = DSGeometryExtension.RadiansToDegrees(ArcEntity.StartAngle);
            SweepAngle = DSGeometryExtension.RadiansToDegrees(ArcEntity.SweepAngle);
            Normal = new DSVector(ArcEntity.Normal);
        }

        #endregion

        #region PROTECTED_CONSTRUCTORS
        protected DSArc(DSPoint firstPoint, DSPoint secondPoint, DSPoint thirdPoint, bool persist)
            : base(ByPointsOnCurveCore(firstPoint, secondPoint, thirdPoint), persist)
        {
            InitializeGuaranteedProperties();
            FirstPoint = firstPoint;
            SecondPoint = secondPoint;
            ThirdPoint = thirdPoint;
        }
        protected DSArc(DSPoint centerPoint, double radius, double startAngle, double sweepAngle, DSVector normal, bool persist) 
            : base(ByCenterPointRadiusAngleCore(centerPoint,radius,startAngle,sweepAngle,ref normal), persist)
        {
             InitializeGuaranteedProperties();
             SweepAngle = sweepAngle; // defect to be corrected later - autocad implementation sending negative angle as sweepangle
        }
        protected DSArc(DSPoint centerPoint, DSPoint startPoint, DSPoint sweepPoint, bool persist, bool unused)
            : base(ByCenterPointStartPointSweepPointCore(centerPoint, startPoint, sweepPoint),persist)
        {

            InitializeGuaranteedProperties();
            SweepPoint = sweepPoint;
        }
        protected DSArc(DSPoint centerPoint, DSPoint startPoint, double sweepAngle, DSVector normal,bool persist)
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
        public static DSArc ByPointsOnCurve(DSPoint firstPoint, DSPoint secondPoint, DSPoint thirdPoint)
        {
            return new DSArc(firstPoint, secondPoint, thirdPoint, true);
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
        public static DSArc ByCenterPointRadiusAngle (DSPoint centerPoint, double radius, double startAngle, double sweepAngle, DSVector normal) 
        {
            return new DSArc(centerPoint, radius, startAngle, sweepAngle, normal, true);           
        }

        /// <summary>
        ///Constructs an Arc using center point, start point and sweep angle with axis aligned along positive Z-axis.
        /// </summary>
        /// <param name="centerPoint">Center point of the circle with which the Arc is to be constructed</param>
        /// <param name="startPoint">The start point of the Arc</param>
        /// <param name="sweepAngle">The angle to be swept by the Arc from the center point-start point axis</param>
        /// <returns></returns>
        public static DSArc ByCenterPointStartPointSweepAngle(DSPoint centerPoint, DSPoint startPoint, double sweepAngle)
        {
            var zAxis = new DSVector(0,0,1, true);
            return ByCenterPointStartPointSweepAngle(centerPoint, startPoint, sweepAngle, zAxis);
        }

        /// <summary>
        /// Constructs an Arc from a center point, start point and sweep point on a plane defined by the three points. 
        /// </summary>
        /// <param name="centerPoint">Center point of the circle with which the Arc is to be constructed</param>
        /// <param name="startPoint">The start point of the Arc</param>
        /// <param name="sweepPoint">The point to which the Arc will be constructed</param>
        /// <returns></returns>
        public static DSArc ByCenterPointStartPointSweepPoint(DSPoint centerPoint, DSPoint startPoint, DSPoint sweepPoint)
        {
            return new DSArc(centerPoint,startPoint,sweepPoint,true,true);
        }

        /// <summary>
        /// Constructs an Arc using a center point, a start point and sweep angle in a plane defined by the center point and normal as input.
        /// </summary>
        /// <param name="centerPoint">Center point of the circle with which the Arc is to be constructed</param>
        /// <param name="startPoint">The start point of the Arc</param>
        /// <param name="sweepAngle">The point to which the Arc will be constructed</param>
        /// <param name="normal">The normal to the plane of the Arc</param>
        /// <returns></returns>
        public static DSArc ByCenterPointStartPointSweepAngle(DSPoint centerPoint, DSPoint startPoint, double sweepAngle, DSVector normal)
        {
            return new DSArc(centerPoint,startPoint,sweepAngle,normal,true);
        }

       
        #endregion

        #region CORE_METHODS

        private static IArcEntity ByPointsOnCurveCore(DSPoint firstPoint, DSPoint secondPoint, DSPoint thirdPoint)
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
            else if (DSGeometryExtension.ArePointsColinear(firstPoint, secondPoint, thirdPoint))
            {
                throw new ArgumentException(string.Format(Properties.Resources.PointsColinear, "first, second and thrid points"), "firstPoint, secondPoint, thridPoint");
            }

            IArcEntity entity = HostFactory.Factory.ArcByPointsOnCurve(firstPoint.PointEntity, secondPoint.PointEntity, thirdPoint.PointEntity);
            if (null == entity)
                throw new Exception(string.Format(Properties.Resources.OperationFailed, "DSArc.ByPointsOnCurve"));
            return entity;
        }
        private static IArcEntity ByCenterPointStartPointSweepAngleCore(DSPoint centerPoint, DSPoint startPoint, double sweepAngle, ref DSVector normal)
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
                            startPoint.PointEntity, DSGeometryExtension.DegreesToRadians(sweepAngle), normal.IVector);
            if (null == entity)
                throw new Exception(string.Format(Properties.Resources.OperationFailed, "DSArc.ByCenterPointStartPointSweepAngle"));
            return entity;
        }
        private static IArcEntity ByCenterPointRadiusAngleCore(DSPoint centerPoint, double radius, double startAngle, double sweepAngle, ref DSVector normal)
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
            IArcEntity entity = HostFactory.Factory.ArcByCenterPointRadiusAngle(centerPoint.PointEntity, radius, DSGeometryExtension.DegreesToRadians(startAngle), DSGeometryExtension.DegreesToRadians(endAngle), normal.IVector);
            if (null == entity)
                throw new Exception(string.Format(Properties.Resources.OperationFailed, "DSArc.ByCenterPointRadiusAngle"));
            return entity;
        }
        private static IArcEntity ByCenterPointStartPointSweepPointCore(DSPoint centerPoint, DSPoint startPoint, DSPoint sweepPoint)
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
                throw new Exception(string.Format(Properties.Resources.OperationFailed, "DSArc.ByCenterPointStartPointSweepPoint"));
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

            var arc = obj as DSArc;
            if (arc == null)
            {
                return false;
            }

            return CenterPoint.Equals(arc.CenterPoint) && Normal.Equals(arc.Normal)
                && DSGeometryExtension.Equals(Radius, arc.Radius);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("DSArc(CenterPoint = {0}, Radius = {1}, Normal = {2})", CenterPoint, 
                            Radius.ToString(DSGeometryExtension.DoublePrintFormat), Normal);
        }
        #endregion
    }
}
