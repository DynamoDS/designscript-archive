using System;
using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    public class Sphere : Solid
    {
        internal ISphereEntity SphereEntity { get { return HostImpl as ISphereEntity; } }

        #region DATA MEMBERS
        private Point mCenterPoint;
        #endregion

        #region INTERNAL_METHODS

        private void InitializeGuaranteedProperties()
        {
            Radius = SphereEntity.GetRadius();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                GeometryExtension.DisposeObject(ref mCenterPoint);
            base.Dispose(disposing);
        }

        #endregion

        #region PRIVATE_CONSTRUCTORS

        static void InitType()
        {
            RegisterHostType(typeof(ISphereEntity), (IGeometryEntity host, bool persist) => { return new Sphere(host as ISphereEntity, persist); });
        }

        private Sphere(ISphereEntity entity, bool persist = false)
            : base(entity, persist) 
        {
            InitializeGuaranteedProperties();
        }
        #endregion

        #region PROTECTED_CONSTRUCTORS

        protected Sphere(Point centerPoint, double radius,bool persist)
            : base(ByCenterPointRadiusCore(centerPoint, radius),persist)
        {
            InitializeGuaranteedProperties();
        }

        #endregion

        #region DESIGNSCRIPT_CONSTRUCTORS
        /// <summary>
        /// Construct a solid sphere with input point as center point and value as radius
        /// </summary>
        /// <param name="centerPoint">The center point of the sphere</param>
        /// <param name="radius">The radius of the sphere</param>
        /// <returns></returns>
        public static Sphere ByCenterPointRadius(Point centerPoint, double radius)
        {
            return new Sphere(centerPoint, radius, true);
        }

        #endregion

        #region CORE_METHODS

        private static ISphereEntity ByCenterPointRadiusCore(Point centerPoint, double radius)
        {
            if (radius.LessThanOrEqualTo(0.0))
                throw new ArgumentException(string.Format(Properties.Resources.LessThanZero, "radius"), "radius");
            if (centerPoint == null)
                throw new ArgumentNullException("centerPoint");

            ISphereEntity entity = HostFactory.Factory.SphereByCenterPointRadius(centerPoint.PointEntity, radius);
            if (null == entity)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "Sphere.ByCenterPointRadius"));
            return entity;
        }

        #endregion

        #region PROPERTIES
        /// <summary>
        /// 
        /// </summary>
        public double Radius { get; private set; }
        
        /// <summary>
        /// 
        /// </summary>
        public Point CenterPoint
        {
            get 
            { 
                if(null == mCenterPoint)
                    mCenterPoint = SphereEntity.GetCenterPoint().ToPoint(false, this);
                return mCenterPoint; 
            }
        }

        #endregion
    }
}
