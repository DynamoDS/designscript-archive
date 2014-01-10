using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    public class SweptSurface : Surface
    {
        #region DATA MEMBERS
        private Curve mProfile;
        private Curve mPath;
        #endregion
        
        #region INTERNAL_METHODS

        private void InitializeGuaranteedProperties()
        {
        }

        #endregion

        #region PRIVATE CONSTRUCTOR

        static void InitType()
        {
        }

        internal SweptSurface(ISurfaceEntity host, bool persist = false)
            : base(host, persist)
        {
            InitializeGuaranteedProperties();
        }

        internal override ISurfaceEntity GetSurfaceEntity()
        {
            return HostImpl as ISurfaceEntity;
        }
        #endregion

        #region PROTECTED_METHODS

        protected SweptSurface(Curve profile, Curve path,bool persist)
            : base(ByProfilePathCore(profile, path),persist)
        {
            InitializeGuaranteedProperties();
            Profile = profile;
            Path = path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GeometryExtension.DisposeObject(ref mProfile);
                GeometryExtension.DisposeObject(ref mPath);
            }
            base.Dispose(disposing);
        }

        #endregion

        #region DESIGNSCRIPT_CONSTRUCTORS

        /// <summary>
        /// Construct a Surface by sweeping a profile curve along a path curve.
        /// </summary>
        /// <param name="profile">Profile curve for sweep.</param>
        /// <param name="path">Path curve to sweep along.</param>
        /// <returns>Sweep Surface.</returns>
        public static SweptSurface ByProfilePath(Curve profile, Curve path)
        {
            return new SweptSurface(profile, path, true);
        }

        #endregion

        #region CORE_METHODS

        private static ISurfaceEntity ByProfilePathCore(Curve profile, Curve path)
        {
            if (null == profile)
                throw new System.ArgumentNullException("profile");

            if (null == path)
                throw new System.ArgumentNullException("path");

            ISurfaceEntity entity = HostFactory.Factory.SurfaceBySweep(path.CurveEntity, profile.CurveEntity);
            if (entity == null)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "SweptSurface.ByProfilePath"));
            return entity;
        }

        #endregion

        #region PROPERTIES

        public Curve Profile
        {
            get { return mProfile; }
            set { value.AssignTo(ref mProfile); }
        }

        public Curve Path
        {
            get { return mPath; }
            set { value.AssignTo(ref mPath); }
        }

        #endregion
    }
}
