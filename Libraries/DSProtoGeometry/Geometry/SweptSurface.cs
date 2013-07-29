using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    public class DSSweptSurface : DSSurface
    {
        #region DATA MEMBERS
        private DSCurve mProfile;
        private DSCurve mPath;
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

        internal DSSweptSurface(ISurfaceEntity host, bool persist = false)
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

        protected DSSweptSurface(DSCurve profile, DSCurve path,bool persist)
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
                DSGeometryExtension.DisposeObject(ref mProfile);
                DSGeometryExtension.DisposeObject(ref mPath);
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
        public static DSSweptSurface ByProfilePath(DSCurve profile, DSCurve path)
        {
            return new DSSweptSurface(profile, path, true);
        }

        #endregion

        #region CORE_METHODS

        private static ISurfaceEntity ByProfilePathCore(DSCurve profile, DSCurve path)
        {
            if (null == profile)
                throw new System.ArgumentNullException("profile");

            if (null == path)
                throw new System.ArgumentNullException("path");

            ISurfaceEntity entity = HostFactory.Factory.SurfaceBySweep(profile.CurveEntity, path.CurveEntity);
            if (entity == null)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "DSSweptSurface.ByProfilePath"));
            return entity;
        }

        #endregion

        #region PROPERTIES

        public DSCurve Profile
        {
            get { return mProfile; }
            set { value.AssignTo(ref mProfile); }
        }

        public DSCurve Path
        {
            get { return mPath; }
            set { value.AssignTo(ref mPath); }
        }

        #endregion
    }
}
