using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    public class DSRevolvedSurface : DSSurface
    {
        #region DATA MEMBERS
        private DSCurve mProfile;
        private DSPoint mAxisOrigin;
        private DSLine mAxis;
        #endregion

        #region PRIVATE CONSTRUCTOR
        static void InitType()
        {
        }

        internal DSRevolvedSurface(ISurfaceEntity host, bool persist = false)
            : base(host, persist)
        {
            InitializeGuaranteedProperties();
        }

        internal override ISurfaceEntity GetSurfaceEntity()
        {
            return HostImpl as ISurfaceEntity;
        }

        #endregion

        #region INTERNAL_METHODS

        private void InitializeGuaranteedProperties()
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DSGeometryExtension.DisposeObject(ref mProfile);
                DSGeometryExtension.DisposeObject(ref mAxisOrigin);
                DSGeometryExtension.DisposeObject(ref mAxis);
            }
            base.Dispose(disposing);
        }
        #endregion

        #region PROTECTED_CONSTRUCTORS

        protected DSRevolvedSurface(DSCurve profile, DSPoint axisOrigin, DSVector axisDirection, double startAngle, double sweepAngle, bool persist)
            : base(ByProfileAxisOriginDirectionAngleCore(profile, axisOrigin, axisDirection, startAngle, sweepAngle), persist)
        {
            InitializeGuaranteedProperties();
            Profile = profile;
            AxisOrigin = axisOrigin;
            AxisDirection = axisDirection;
            StartAngle = startAngle;
            SweepAngle = sweepAngle;
        }
        
        protected DSRevolvedSurface(DSCurve profile, DSLine axis, double startAngle, double sweepAngle, bool persist)
            : base(ByProfileAxisAngleCore(profile, axis, startAngle, sweepAngle), persist)
        {
            InitializeGuaranteedProperties();
            Profile = profile;
            AxisOrigin = axis.StartPoint;
            AxisDirection = axis.Direction;
            StartAngle = startAngle;
            SweepAngle = sweepAngle;
            Axis = axis;
        }

        #endregion

        #region DESIGNSCRIPT_CONSTRUCTORS

        /// <summary>
        /// Construct a Surface by revolving the profile curve about an axis 
        /// defined by axisOrigin point and axisDirection Vector. startAngle 
        /// determines where the curve starts to revolve, sweepAngle determines 
        /// the extent of the revolve.
        /// </summary>
        /// <param name="profile">Profile Curve for revolve surface.</param>
        /// <param name="axisOrigin">Origin Point for axis of revolution.</param>
        /// <param name="axisDirection">Direction Vector for axis of revolution.</param>
        /// <param name="startAngle">Start Angle in degreee at which curve starts to revolve.</param>
        /// <param name="sweepAngle">Sweep Angle in degree to define the extent of revolve.</param>
        /// <returns>RevolvedSurface</returns>
        public static DSRevolvedSurface ByProfileAxisOriginDirectionAngle(DSCurve profile, DSPoint axisOrigin, DSVector axisDirection, double startAngle, double sweepAngle)
        {
            return new DSRevolvedSurface(profile, axisOrigin, axisDirection, startAngle, sweepAngle, true);
        }

        private static ISurfaceEntity ByProfileAxisOriginDirectionAngleCore(DSCurve profile, DSPoint axisOrigin, DSVector axisDirection, double startAngle, double sweepAngle)
        {
            if (null == profile)
                throw new System.ArgumentException(string.Format(Properties.Resources.NullArgument, "profile"), "profile");

            if (null == axisOrigin)
                throw new System.ArgumentException(string.Format(Properties.Resources.NullArgument, "axis origin"), "axisOrigin");

            if (null == axisDirection)
                throw new System.ArgumentException(string.Format(Properties.Resources.NullArgument, "axis direction"), "axisDirection");

            ISurfaceEntity entity = HostFactory.Factory.SurfaceByRevolve(profile.CurveEntity, axisOrigin.PointEntity, axisDirection.IVector, startAngle, sweepAngle);
            if (entity == null)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "DSSurface.Revolve"));
            return entity;
        }

        /// <summary>
        /// Construct a Surface by revolving the profile curve about an axis
        /// defined by axisOrigin point and axisDirection Vector.
        /// Assuming sweep angle = 360 and start angle = 0.
        /// </summary>
        /// <param name="profile">Profile Curve for revolve surface.</param>
        /// <param name="axisOrigin">Origin Point for axis of revolution.</param>
        /// <param name="axisDirection">Direction Vector for axis of revolution.</param>
        /// <returns>RevolvedSurface</returns>
        public static DSRevolvedSurface ByProfileAxisOriginDirection(DSCurve profile, DSPoint axisOrigin, DSVector axisDirection)
        {
            return new DSRevolvedSurface(profile, axisOrigin, axisDirection, 0, 360, true);
        }

        /// <summary>
        /// Construct a Surface by revolving  curve about a line axis. startAngle 
        /// determines where the curve starts to revolve, sweepAngle determines 
        /// the revolving angle.
        /// </summary>
        /// <param name="profile">Profile Curve for revolve surface.</param>
        /// <param name="axis">Line to define axis of revolution.</param>
        /// <param name="startAngle">Start Angle in degreee at which curve starts to revolve.</param>
        /// <param name="sweepAngle">Sweep Angle in degree to define the extent of revolve.</param>
        /// <returns>RevolvedSurface</returns>
        public static DSRevolvedSurface ByProfileAxisAngle(DSCurve profile, DSLine axis, double startAngle, double sweepAngle)
        {
            return new DSRevolvedSurface(profile, axis, startAngle, sweepAngle, true);
        }

        private static ISurfaceEntity ByProfileAxisAngleCore(DSCurve profile, DSLine axis, double startAngle, double sweepAngle)
        {
            if (null == axis)
                throw new System.ArgumentNullException("axis");
            ISurfaceEntity surf = ByProfileAxisOriginDirectionAngleCore(profile, axis.StartPoint, axis.Direction, startAngle, sweepAngle);
            if (null == surf)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "DSRevolvedSurface.ByProfileAxisAngle"));
            return surf;
        }

        /// <summary>
        /// Construct a Surface by revolving curve about a line axis. Assuming 
        /// sweep angle = 360 and start angle = 0.
        /// </summary>
        /// <param name="profile">Profile Curve for revolve surface.</param>
        /// <param name="axis">Line to define axis of revolution.</param>
        /// <returns>Surface of revolution.</returns>
        public static DSRevolvedSurface ByProfileAxis(DSCurve profile, DSLine axis)
        {
            return new DSRevolvedSurface(profile, axis, 0, 360, true);
        }

        #endregion

        #region PROPERTIES

        public DSCurve Profile
        {
            get { return mProfile; }
            set { value.AssignTo(ref mProfile); }
        }

        public DSPoint AxisOrigin
        {
            get { return mAxisOrigin; }
            set { value.AssignTo(ref mAxisOrigin); }
        }

        public DSVector AxisDirection { get; private set; }

        public double? StartAngle { get; private set; }

        public double? SweepAngle { get; private set; }

        public DSLine Axis
        {
            get { return mAxis; }
            set { value.AssignTo(ref mAxis); }
        }

        #endregion
    }
}
