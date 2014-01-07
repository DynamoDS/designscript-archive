using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    public class Cone : Solid
    {
        internal IConeEntity ConeEntity { get { return HostImpl as IConeEntity; } }

        #region DATA MEMBERS
        private Point mEndPoint;
        private Line mCenterLine;
        private Point mStartPoint;
        private double? mHeight;
        private double? mEndRadius;
        private double? mStartRadius;
        #endregion

        #region PRIVATE_CONSTRUCTORS
        private Cone(IConeEntity entity, bool persist = false)
            : base(entity, persist) 
        {
            InitializeGuaranteedProperties();
        }

        static void InitType()
        {
            //Do nothing, Cylinder registers geometry constructor for IConeEntity.
        }

        internal static Cone ToCone(IConeEntity host, bool persist = false)
        {
            if (host.Owner == null)
                return new Cone(host, persist);

            return host.Owner as Cone;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GeometryExtension.DisposeObject(ref mStartPoint);
                GeometryExtension.DisposeObject(ref mEndPoint);
                GeometryExtension.DisposeObject(ref mCenterLine);
            }

            base.Dispose(disposing);
        }

        #endregion

        #region INTERNAL_METHODS

        private void InitializeGuaranteedProperties()
        {            
        }

        protected override Geometry Translate(Vector offset)
        {
            IConeEntity clone = GeomEntity.CopyAndTranslate(offset.VectorEntity) as IConeEntity;
            if (null == clone)
                throw new System.InvalidOperationException("Failed to clone and translate geometry.");

            return new Cone(clone, true);
        }

        internal override Geometry TransformBy(ICoordinateSystemEntity csEntity)
        {
            IGeometryEntity clone = GeomEntity.CopyAndTransform(CoordinateSystem.WCS.CSEntity, csEntity);
            if (null == clone)
                throw new System.InvalidOperationException("Failed to clone and transform cone.");

            IConeEntity cone = clone as IConeEntity;
            if (null != cone)
                return new Cone(cone, true);
            return ToGeometry(clone, true);
        }
        #endregion

        #region PROTECTED_CONSTRUCTORS

        protected Cone(Point startPoint, Point endPoint, double startRadius, double endRadius,bool persist)
            : base(ByStartPointEndPointRadiusCore(startPoint, endPoint, startRadius, endRadius),persist)
        {
            InitializeGuaranteedProperties();
            StartPoint = startPoint;
            EndPoint = endPoint;
            StartRadius = startRadius;
            EndRadius = endRadius;
        }

        protected Cone(CoordinateSystem contextCoordinateSystem, double startRadius, double endRadius, double height, bool persist)
            : base(ByRadiusHeightCore(contextCoordinateSystem, startRadius, endRadius, height), persist)
        {
            InitializeGuaranteedProperties();
            ContextCoordinateSystem = contextCoordinateSystem;
        }

        protected Cone(Line centerLine, double startRadius, double endRadius, bool persist)
            : base(ByCenterLineRadiusCore(centerLine, startRadius, endRadius), persist)
        {
            InitializeGuaranteedProperties();
            CenterLine = centerLine;
        }

        #endregion

        #region DESIGNSCRIPT_CONSTRUCTORS

        /// <summary>
        /// Constructs a solid cone defined by a start and end point on its axis and the start and end radii of its base and top respectively.
        /// </summary>
        /// <param name="startPoint">The start point on the axis of the cone</param>
        /// <param name="endPoint">The end point on the axis of the cone</param>
        /// <param name="startRadius">The radius of the base of the cone</param>
        /// <param name="endRadius">The radius of the top of the cone</param>
        /// <returns></returns>
        public static Cone ByStartPointEndPointRadius(Point startPoint, Point endPoint, double startRadius, double endRadius)
        {
            return new Cone(startPoint, endPoint, startRadius, endRadius, true);
        }

        /// <summary>
        /// Constructs a solid cone defined by a line the start and end radii of its base and top respectively.
        /// </summary>
        /// <param name="centerLine">The center line of the cone</param>
        /// <param name="startRadius">The radius of the base of the cone</param>
        /// <param name="endRadius">The radius of the top of the cone</param>
        /// <returns></returns>
        public static Cone ByCenterLineRadius(Line centerLine, double startRadius, double endRadius)
        {
            return new Cone(centerLine, startRadius, endRadius, true);
        }

        /// <summary>
        /// Constructs a solid cone defined a parent CoordinateSystem, start radius, end radius and height
        /// </summary>
        /// <param name="contextCoordinateSystem">The parent CoordinateSystem of the cone</param>
        /// <param name="startRadius">The radius of the base of the cone</param>
        /// <param name="endRadius">The radius of the top of the cone</param>
        /// <param name="height">The height of the cone</param>
        /// <returns></returns>
        public static Cone ByRadiusHeight(CoordinateSystem contextCoordinateSystem, double startRadius, double endRadius, double height)
        {
            return new Cone(contextCoordinateSystem, startRadius, endRadius, height, true);
        }

        #endregion

        #region CORE_METHODS

        private static IConeEntity ByCenterLineRadiusCore(Line centerLine, double startRadius, double endRadius)
        {
            if (null == centerLine)
                throw new System.ArgumentNullException("centerLine");

            IConeEntity entity = ByStartPointEndPointRadiusCore(centerLine.StartPoint, centerLine.EndPoint, startRadius, endRadius);
            return entity;
        }

        private static IConeEntity ByRadiusHeightCore(CoordinateSystem contextCoordinateSystem, double startRadius, double endRadius, double height)
        {
            string kMethod = "Cone.ByRadiusHeight";

            if (startRadius.LessThanOrEqualTo(0.0))
                throw new System.ArgumentException(string.Format(Properties.Resources.LessThanZero, "start radius"), "startRadius");
            if (endRadius < 0.0)
                throw new System.ArgumentException(string.Format(Properties.Resources.LessThanZero, "end radius"), "endRadius");
            if (height.LessThanOrEqualTo(0.0))
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZero, "height"), "height");
            if (null == contextCoordinateSystem)
                throw new System.ArgumentNullException("contextCoordinateSystem");
            if (!contextCoordinateSystem.IsUniscaledOrtho())
            {
                if (contextCoordinateSystem.IsScaledOrtho())
                    throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "Non Uniform Scaled CoordinateSystem", kMethod));
                else
                    throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "Shear CoordinateSystem", kMethod));
            }

            IConeEntity entity = HostFactory.Factory.ConeByCoordinateSystemHeightRadii(contextCoordinateSystem.CSEntity, height, startRadius, endRadius );
            if (null == entity)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, kMethod));
            return entity;
        }

        private static IConeEntity ByStartPointEndPointRadiusCore(Point startPoint, Point endPoint, double startRadius, double endRadius)
        {
            if (startRadius.LessThanOrEqualTo(0.0))
                throw new System.ArgumentException(string.Format(Properties.Resources.LessThanZero, "start radius"), "startRadius");
            if (endRadius < 0.0)
                throw new System.ArgumentException(string.Format(Properties.Resources.LessThanZero, "end radius"), "endRadius");
            if (null == startPoint)
                throw new System.ArgumentNullException("startPoint");
            if (null == endPoint)
                throw new System.ArgumentNullException("endPoint");
            if (startPoint.DistanceTo(endPoint).EqualsTo(0.0))
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZeroDistance, "start point", "end point"), "startPoint, endPoint");

            IConeEntity entity = HostFactory.Factory.ConeByPointsRadii(startPoint.PointEntity, endPoint.PointEntity, startRadius, endRadius);
            if (null == entity)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "Cone.ByStartPointEndPointRadius"));
            return entity;
        }

        #endregion

        #region PROPERTIES

        /// <summary>
        /// 
        /// </summary>
        public Point StartPoint 
        {
            get
            {
                if (null == mStartPoint)
                    mStartPoint = ConeEntity.StartPoint.ToPoint(false, this);
                return mStartPoint;
            }
            private set
            {
                value.AssignTo(ref mStartPoint);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public Point EndPoint 
        {
            get
            {
                if (null == mEndPoint)
                    mEndPoint = ConeEntity.EndPoint.ToPoint(false, this);
                return mEndPoint;
            }
            private set
            {
                value.AssignTo(ref mEndPoint);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double StartRadius 
        {
            get
            {
                if (!mStartRadius.HasValue)
                {
                    mStartRadius = ConeEntity.StartRadius;
                }
                return mStartRadius.Value;
            }
            private set
            {
                mStartRadius = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double EndRadius 
        {
            get
            {
                if (!mEndRadius.HasValue)
                {
                    mEndRadius = ConeEntity.RadiusRatio * StartRadius;
                }
                return mEndRadius.Value;
            }
            private set
            {
                mEndRadius = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Line CenterLine
        { 
            get
            {
                if (mCenterLine == null)
                {
                    if (StartPoint != null && EndPoint != null)
                    {
                        ILineEntity entity = HostFactory.Factory.LineByStartPointEndPoint(StartPoint.PointEntity,
                                                        EndPoint.PointEntity);
                        mCenterLine = entity.ToLine(false, this);
                    }
                }
                return mCenterLine;
            }
            private set 
            {
                value.AssignTo(ref mCenterLine); 
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double Height
        {
            get
            {
                if (!mHeight.HasValue)
                {
                    mHeight = ConeEntity.Height;
                }
                return mHeight.Value;
            }
            private set
            {
                mHeight = value;
            }
        }

        #endregion
    }
}
