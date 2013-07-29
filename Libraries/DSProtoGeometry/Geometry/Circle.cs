using System;
using System.ComponentModel;
using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    public class DSCircle : DSCurve
    {
        internal ICircleEntity CircleEntity { get { return HostImpl as ICircleEntity; } }

        #region DATA MEMBERS
        private DSPoint mFirstPoint;
        private DSPoint mSecondPoint;
        private DSPoint mThirdPoint;
        private DSPoint mCenterPoint;
        #endregion

        #region OVERRIDES

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        override protected  DSPoint GetStartPoint()
        {
            return CurveEntity.StartPoint.ToPoint(false, this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        override protected  DSPoint GetEndPoint()
        {
            return CurveEntity.EndPoint.ToPoint(false, this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DSGeometryExtension.DisposeObject(ref mFirstPoint);
                DSGeometryExtension.DisposeObject(ref mSecondPoint);
                DSGeometryExtension.DisposeObject(ref mThirdPoint);
                DSGeometryExtension.DisposeObject(ref mCenterPoint);
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
        public DSPoint CenterPoint
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
        public DSVector Normal { get; private set; }

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
        public DSPoint PointOnCurve
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
            RegisterHostType(typeof(ICircleEntity), (IGeometryEntity host, bool persist) => { return new DSCircle(host as ICircleEntity, persist); });
        }

        private DSCircle(ICircleEntity entity, bool persist = false) : base(entity, persist)
        {
            InitializeGuaranteedProperties();
        }

        #endregion

        #region INTERNAL_METHODS

        private void InitializeGuaranteedProperties()
        {
            Normal = new DSVector(CircleEntity.Normal);
            Radius = CircleEntity.Radius;
        }

        #endregion

        #region PROTECTED_CONSTRUCTORS

        protected DSCircle(DSPoint firstPoint, DSPoint secondPoint, DSPoint thirdPoint,bool persist)
            : base(ByPointsOnCurveCore(firstPoint, secondPoint, thirdPoint),persist)
        {
            InitializeGuaranteedProperties();
            FirstPoint = firstPoint;
            SecondPoint = secondPoint;
            ThirdPoint = thirdPoint;
        }

        protected DSCircle(DSPoint centerPoint, double radius, DSVector normal,bool persist)
            : base(ByCenterPointRadiusCore(centerPoint, radius, ref normal),persist)
        {
            InitializeGuaranteedProperties();
        }

        protected DSCircle(DSPoint firstPoint, DSPoint secondPoint, DSVector normal,bool persist)
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
        public static DSCircle By2Points(DSPoint firstPoint, DSPoint secondPoint , DSVector normal )
        {
            return new DSCircle(firstPoint, secondPoint, normal, true);
        }
        
        /// <summary>
        /// Constructs a circle passing through 3 points. 
        /// </summary>
        /// <param name="firstPoint">First point through which the circle passes</param>
        /// <param name="secondPoint">Second point through which the circle passes</param>
        /// <param name="thirdPoint">Third point through which the circle passes</param>
        /// <returns></returns>
        public static DSCircle  ByPointsOnCurve(DSPoint firstPoint, DSPoint secondPoint , DSPoint thirdPoint )
        {
            return new DSCircle(firstPoint, secondPoint, thirdPoint,true);
        }
        
        /// <summary>
        /// Constructs a circle from its center point and radius with normal along the positive Z-axis
        /// </summary>
        /// <param name="centerPoint">The desired center point of the circle</param>
        /// <param name="radius">The desired radius of the circle</param>
        /// <returns></returns>
        public static DSCircle  ByCenterPointRadius(DSPoint centerPoint, double radius)
        {
            return new DSCircle(centerPoint, radius, DSVector.ZAxis, true);
        }

        /// <summary>
        /// Constructs a circle from its center point and radius with given vector
        /// </summary>
        /// <param name="centerPoint">The desired center point of the circle</param>
        /// <param name="radius">The desired radius of the circle</param>
        /// <param name="normal">Normal to the plane of the surface</param>
        /// <returns></returns>
        public static DSCircle ByCenterPointRadius(DSPoint centerPoint, double radius, DSVector normal)
        {
            return new DSCircle(centerPoint, radius, normal, true);
        }
        
        #endregion

        #region CORE_METHODS

        private static ICircleEntity By2PointsCore(DSPoint firstPoint, DSPoint secondPoint, ref DSVector normal)
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
            var circleEntity = DSCircle.ByCenterPointRadiusCore(centerPt, cptPos.DistanceTo(fptPos), ref normal);
            if (circleEntity == null)
                throw new Exception(string.Format(Properties.Resources.OperationFailed, "DSCircle.By2Points"));
            return circleEntity;
        }

        private static ICircleEntity ByPointsOnCurveCore(DSPoint firstPoint, DSPoint secondPoint, DSPoint thirdPoint)
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
            else if (DSGeometryExtension.ArePointsColinear(firstPoint, secondPoint, thirdPoint))
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
                throw new Exception(string.Format(Properties.Resources.OperationFailed, "DSCircle.ByPointsOnCurve"));
            return entity;
        }

        private static ICircleEntity ByCenterPointRadiusCore(DSPoint centerPoint, double radius, ref DSVector normal)
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
                throw new Exception(string.Format(Properties.Resources.OperationFailed, "DSCircle.ByCenterPointRadius"));
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

            var other = obj as DSCircle;
            if (obj == null || other == null)
            {
                return false;
            }


            return CenterPoint.Equals(other.CenterPoint) && DSGeometryExtension.Equals(Radius, other.Radius) &&
                    Normal.Equals(other.Normal);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("DSCircle(CenterPoint = {0}, Radius = {1}, Normal = {2})", CenterPoint, 
                                    Radius.ToString(DSGeometryExtension.DoublePrintFormat), Normal);
        }

        #endregion
    }

}
