using System;
using System.ComponentModel;
using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    public class Circle : Curve
    {
        internal ICircleEntity CircleEntity { get { return HostImpl as ICircleEntity; } }

        #region DATA MEMBERS
        private Point mFirstPoint;
        private Point mSecondPoint;
        private Point mThirdPoint;
        private Point mCenterPoint;
        #endregion

        #region OVERRIDES

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        override protected  Point GetStartPoint()
        {
            return CurveEntity.StartPoint.ToPoint(false, this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        override protected  Point GetEndPoint()
        {
            return CurveEntity.EndPoint.ToPoint(false, this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GeometryExtension.DisposeObject(ref mFirstPoint);
                GeometryExtension.DisposeObject(ref mSecondPoint);
                GeometryExtension.DisposeObject(ref mThirdPoint);
                GeometryExtension.DisposeObject(ref mCenterPoint);
            }
            base.Dispose(disposing);
        }

        #endregion

        #region PROPERTIES
        /// <summary>
        /// 
        /// </summary>
        public override bool IsClosed
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool IsCircular
        {
            get
            {
                return true;
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
        public override double Length
        {
            get { return 2.0 * Math.PI * Radius; }
        }

        /// <summary>
        /// 
        /// </summary>
        [Category("Primary")]
        public double Radius { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        [Category("Primary")]
        public Point CenterPoint
        {
            get 
            { 
                if(null == mCenterPoint)
                    mCenterPoint = CircleEntity.CenterPoint.ToPoint(false, this);
                return mCenterPoint; 
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Category("Primary")]
        public Vector Normal { get; private set; }

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
        public Point PointOnCurve
        {
            get
            {
                return SecondPoint;
            }
        }

        #endregion

        #region PRIVATE_CONSTRUCTORS

        static void InitType()
        {
            RegisterHostType(typeof(ICircleEntity), (IGeometryEntity host, bool persist) => { return new Circle(host as ICircleEntity, persist); });
        }

        private Circle(ICircleEntity entity, bool persist = false) : base(entity, persist)
        {
            InitializeGuaranteedProperties();
        }

        #endregion

        #region INTERNAL_METHODS

        private void InitializeGuaranteedProperties()
        {
            Normal = new Vector(CircleEntity.Normal);
            Radius = CircleEntity.Radius;
        }

        #endregion

        #region PROTECTED_CONSTRUCTORS

        protected Circle(Point firstPoint, Point secondPoint, Point thirdPoint,bool persist)
            : base(ByPointsOnCurveCore(firstPoint, secondPoint, thirdPoint),persist)
        {
            InitializeGuaranteedProperties();
            FirstPoint = firstPoint;
            SecondPoint = secondPoint;
            ThirdPoint = thirdPoint;
        }

        protected Circle(Point centerPoint, double radius, Vector normal,bool persist)
            : base(ByCenterPointRadiusCore(centerPoint, radius, ref normal),persist)
        {
            InitializeGuaranteedProperties();
        }

        protected Circle(Point firstPoint, Point secondPoint, Vector normal,bool persist)
            : base(By2PointsCore(firstPoint, secondPoint, ref normal),persist)
        {
            InitializeGuaranteedProperties();
            FirstPoint = firstPoint;
            SecondPoint = secondPoint;
        }

        #endregion

        #region DESIGNSCRIPT_CONSTRUCTORS

        /// <summary>
        /// Constructs a circle passing 2 input points on diameter and using input vector as the normal.
        /// </summary>
        /// <param name="firstPoint">First point on the diameter</param>
        /// <param name="secondPoint">Second point on the diameter</param>
        /// <param name="normal">Normal to the plane of the circle</param>
        /// <returns></returns>
        public static Circle By2Points(Point firstPoint, Point secondPoint , Vector normal )
        {
            return new Circle(firstPoint, secondPoint, normal, true);
        }
        
        /// <summary>
        /// Constructs a circle passing through 3 points. 
        /// </summary>
        /// <param name="firstPoint">First point through which the circle passes</param>
        /// <param name="secondPoint">Second point through which the circle passes</param>
        /// <param name="thirdPoint">Third point through which the circle passes</param>
        /// <returns></returns>
        public static Circle  ByPointsOnCurve(Point firstPoint, Point secondPoint , Point thirdPoint )
        {
            return new Circle(firstPoint, secondPoint, thirdPoint,true);
        }
        
        /// <summary>
        /// Constructs a circle from its center point and radius with normal along the positive Z-axis
        /// </summary>
        /// <param name="centerPoint">The desired center point of the circle</param>
        /// <param name="radius">The desired radius of the circle</param>
        /// <returns></returns>
        public static Circle  ByCenterPointRadius(Point centerPoint, double radius)
        {
            return new Circle(centerPoint, radius, Vector.ZAxis, true);
        }

        /// <summary>
        /// Constructs a circle from its center point and radius with given vector
        /// </summary>
        /// <param name="centerPoint">The desired center point of the circle</param>
        /// <param name="radius">The desired radius of the circle</param>
        /// <param name="normal">Normal to the plane of the surface</param>
        /// <returns></returns>
        public static Circle ByCenterPointRadius(Point centerPoint, double radius, Vector normal)
        {
            return new Circle(centerPoint, radius, normal, true);
        }
        
        #endregion

        #region CORE_METHODS

        private static ICircleEntity By2PointsCore(Point firstPoint, Point secondPoint, ref Vector normal)
        {
            if (null == firstPoint)
                throw new ArgumentNullException("firstPoint");
            if (null == secondPoint)
                throw new ArgumentNullException("secondPoint");
            if (null == normal)
                throw new ArgumentNullException("normal");
            if (firstPoint.Equals(secondPoint))
                throw new ArgumentException(string.Format(Properties.Resources.EqualGeometry, "first point", "second point"), "firstPoint, secondPoint");
            if (normal.IsZeroVector())
                throw new ArgumentException(string.Format(Properties.Resources.IsZeroVector, "normal"), "normal");

            var fptPos = firstPoint.PointEntity;
            var sptPos = secondPoint.PointEntity;
            var cptPos = HostFactory.Factory.CreatePoint((fptPos.X + sptPos.X) / 2.0,
                                                                (fptPos.Y + sptPos.Y) / 2.0,
                                                                (fptPos.Z + sptPos.Z) / 2.0);
            var centerPt = cptPos.ToPoint(false, null);
            var circleEntity = Circle.ByCenterPointRadiusCore(centerPt, cptPos.DistanceTo(fptPos), ref normal);
            if (circleEntity == null)
                throw new Exception(string.Format(Properties.Resources.OperationFailed, "Circle.By2Points"));
            return circleEntity;
        }

        private static ICircleEntity ByPointsOnCurveCore(Point firstPoint, Point secondPoint, Point thirdPoint)
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
                throw new ArgumentException(string.Format(Properties.Resources.EqualGeometry, "second point", "thrid point"), "secondPoint, thirdPoint");
            }
            else if (thirdPoint.Equals(firstPoint))
            {
                throw new ArgumentException(string.Format(Properties.Resources.EqualGeometry, "third point", "first point"), "thirdPoint, firstPoint");
            }
            else if (GeometryExtension.ArePointsColinear(firstPoint, secondPoint, thirdPoint))
            {
                throw new ArgumentException(string.Format(Properties.Resources.PointsColinear, "first, second and thrid points"), "firstPoint, secondPoint, thirdPoint");
            }
            /*
            Vector normal = null;
            var centerPt = Utils.GetCircumCenter(firstPoint, secondPoint, thirdPoint, out normal);
            if (centerPt == null || normal == null)
            {
                return null;
            }
            double rad = firstPoint.PointEntity.DistanceTo(centerPt.PointEntity);
            if( rad <= 0.0)
            {
                return null;
            }
            */
            var entity = HostFactory.Factory.CircleByPointsOnCurve(firstPoint.PointEntity,
                                                        secondPoint.PointEntity, thirdPoint.PointEntity);
            if (null == entity)
                throw new Exception(string.Format(Properties.Resources.OperationFailed, "Circle.ByPointsOnCurve"));
            return entity;
        }

        private static ICircleEntity ByCenterPointRadiusCore(Point centerPoint, double radius, ref Vector normal)
        {
            if (null == centerPoint)
                throw new ArgumentNullException("centerPoint");
            if (null == normal)
                throw new ArgumentNullException("normal");
            if (normal.IsZeroVector())
                throw new ArgumentException(string.Format(Properties.Resources.IsZeroVector, "normal"), "normal");
            if (radius <= 0.0)
                throw new ArgumentException(Properties.Resources.IsZeroRadius);

            normal = normal.IsNormalized ? normal : normal.Normalize();
            ICircleEntity entity = HostFactory.Factory.CircleByCenterPointRadius(centerPoint.PointEntity, radius, normal.IVector);
            if (null == entity)
                throw new Exception(string.Format(Properties.Resources.OperationFailed, "Circle.ByCenterPointRadius"));
            return entity;
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

            var other = obj as Circle;
            if (obj == null || other == null)
            {
                return false;
            }


            return CenterPoint.Equals(other.CenterPoint) && GeometryExtension.Equals(Radius, other.Radius) &&
                    Normal.Equals(other.Normal);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Circle(CenterPoint = {0}, Radius = {1}, Normal = {2})", CenterPoint, 
                                    Radius.ToString(GeometryExtension.DoublePrintFormat), Normal);
        }

        #endregion
    }

}
