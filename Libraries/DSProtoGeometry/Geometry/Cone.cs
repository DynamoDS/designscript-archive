using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    public class DSCone : DSSolid
    {
        internal IConeEntity ConeEntity { get { return HostImpl as IConeEntity; } }

        #region DATA MEMBERS
        private DSPoint mEndPoint;
        private DSLine mCenterLine;
        private DSPoint mStartPoint;
        private double? mHeight;
        private double? mEndRadius;
        private double? mStartRadius;
        #endregion

        #region PRIVATE_CONSTRUCTORS
        private DSCone(IConeEntity entity, bool persist = false)
            : base(entity, persist) 
        {
            InitializeGuaranteedProperties();
        }

        static void InitType()
        {
            //Do nothing, Cylinder registers geometry constructor for IConeEntity.
        }

        internal static DSCone ToCone(IConeEntity host, bool persist = false)
        {
            if (host.Owner == null)
                return new DSCone(host, persist);

            return host.Owner as DSCone;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DSGeometryExtension.DisposeObject(ref mStartPoint);
                DSGeometryExtension.DisposeObject(ref mEndPoint);
                DSGeometryExtension.DisposeObject(ref mCenterLine);
            }

            base.Dispose(disposing);
        }

        #endregion

        #region INTERNAL_METHODS

        private void InitializeGuaranteedProperties()
        {            
        }

        protected override DSGeometry Translate(DSVector offset)
        {
            IConeEntity clone = GeomEntity.CopyAndTranslate(offset.IVector) as IConeEntity;
            if (null == clone)
                throw new System.InvalidOperationException("Failed to clone and translate geometry.");

            return new DSCone(clone, true);
        }

        internal override DSGeometry TransformBy(ICoordinateSystemEntity csEntity)
        {
            IGeometryEntity clone = GeomEntity.CopyAndTransform(DSCoordinateSystem.WCS.CSEntity, csEntity);
            if (null == clone)
                throw new System.InvalidOperationException("Failed to clone and transform cone.");

            IConeEntity cone = clone as IConeEntity;
            if (null != cone)
                return new DSCone(cone, true);
            return ToGeometry(clone, true);
        }
        #endregion

        #region PROTECTED_CONSTRUCTORS

        protected DSCone(DSPoint startPoint, DSPoint endPoint, double startRadius, double endRadius,bool persist)
            : base(ByStartPointEndPointRadiusCore(startPoint, endPoint, startRadius, endRadius),persist)
        {
            InitializeGuaranteedProperties();
            StartPoint = startPoint;
            EndPoint = endPoint;
            StartRadius = startRadius;
            EndRadius = endRadius;
        }

        protected DSCone(DSCoordinateSystem contextCoordinateSystem, double startRadius, double endRadius, double height, bool persist)
            : base(ByRadiusHeightCore(contextCoordinateSystem, startRadius, endRadius, height), persist)
        {
            InitializeGuaranteedProperties();
            ContextCoordinateSystem = contextCoordinateSystem;
        }

        protected DSCone(DSLine centerLine, double startRadius, double endRadius, bool persist)
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
        public static DSCone ByStartPointEndPointRadius(DSPoint startPoint, DSPoint endPoint, double startRadius, double endRadius)
        {
            return new DSCone(startPoint, endPoint, startRadius, endRadius, true);
        }

        /// <summary>
        /// Constructs a solid cone defined by a line the start and end radii of its base and top respectively.
        /// </summary>
        /// <param name="centerLine">The center line of the cone</param>
        /// <param name="startRadius">The radius of the base of the cone</param>
        /// <param name="endRadius">The radius of the top of the cone</param>
        /// <returns></returns>
        public static DSCone ByCenterLineRadius(DSLine centerLine, double startRadius, double endRadius)
        {
            return new DSCone(centerLine, startRadius, endRadius, true);
        }

        /// <summary>
        /// Constructs a solid cone defined a parent CoordinateSystem, start radius, end radius and height
        /// </summary>
        /// <param name="contextCoordinateSystem">The parent CoordinateSystem of the cone</param>
        /// <param name="startRadius">The radius of the base of the cone</param>
        /// <param name="endRadius">The radius of the top of the cone</param>
        /// <param name="height">The height of the cone</param>
        /// <returns></returns>
        public static DSCone ByRadiusHeight(DSCoordinateSystem contextCoordinateSystem, double startRadius, double endRadius, double height)
        {
            return new DSCone(contextCoordinateSystem, startRadius, endRadius, height, true);
        }

        #endregion

        #region CORE_METHODS

        private static IConeEntity ByCenterLineRadiusCore(DSLine centerLine, double startRadius, double endRadius)
        {
            if (null == centerLine)
                throw new System.ArgumentNullException("centerLine");

            IConeEntity entity = ByStartPointEndPointRadiusCore(centerLine.StartPoint, centerLine.EndPoint, startRadius, endRadius);
            return entity;
        }

        private static IConeEntity ByRadiusHeightCore(DSCoordinateSystem contextCoordinateSystem, double startRadius, double endRadius, double height)
        {
            string kMethod = "DSCone.ByRadiusHeight";

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
                    throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "Non Uniform Scaled DSCoordinateSystem", kMethod));
                else
                    throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "Shear DSCoordinateSystem", kMethod));
            }

            IConeEntity entity = HostFactory.Factory.ConeByRadiusLength(contextCoordinateSystem.CSEntity, startRadius, endRadius, height);
            if (null == entity)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, kMethod));
            return entity;
        }

        private static IConeEntity ByStartPointEndPointRadiusCore(DSPoint startPoint, DSPoint endPoint, double startRadius, double endRadius)
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

            IConeEntity entity = HostFactory.Factory.ConeByPointsRadius(startPoint.PointEntity, endPoint.PointEntity, startRadius, endRadius);
            if (null == entity)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "DSCone.ByStartPointEndPointRadius"));
            return entity;
        }

        #endregion

        #region PROPERTIES

        /// <summary>
        /// 
        /// </summary>
        public DSPoint StartPoint 
        {
            get
            {
                if (null == mStartPoint)
                    mStartPoint = ConeEntity.GetStartPoint().ToPoint(false, this);
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
        public DSPoint EndPoint 
        {
            get
            {
                if (null == mEndPoint)
                    mEndPoint = ConeEntity.GetEndPoint().ToPoint(false, this);
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
                    mStartRadius = ConeEntity.GetStartRadius();
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
                    mEndRadius = ConeEntity.GetRadiusRatio() * StartRadius;
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
        public DSLine CenterLine
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
                    mHeight = ConeEntity.GetHeight();
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
