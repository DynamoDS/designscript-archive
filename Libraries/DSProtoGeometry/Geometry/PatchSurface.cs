using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    public class DSPatchSurface : DSSurface
    {
        #region DATA MEMBERS
        private DSCurve mProfile;
        #endregion
        
        #region PRIVATE CONSTRUCTOR
        static void InitType()
        {
        }

        internal DSPatchSurface(ISurfaceEntity host, bool persist = false)
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
                DSGeometryExtension.DisposeObject(ref mProfile);
            base.Dispose(disposing);
        }

        #endregion

        #region PROTECTED_CONSTRUCTORS

        protected DSPatchSurface(DSCurve profile,bool persist)
            : base(FromCurveCore(profile), persist)
        {
            InitializeGuaranteedProperties();
            Profile = profile;
        }

        #endregion

        #region DESIGNSCRIPT_CONSTRUCTORS

        /// <summary>
        /// Constructs a patch surface from the give closed non-self intersecting 
        /// profile curve.
        /// </summary>
        /// <param name="profile">Profile curve for patch surface</param>
        /// <returns>Patch Surface</returns>
        public static DSPatchSurface FromCurve(DSCurve profile)
        {
            return new DSPatchSurface(profile, true);
        }

        #endregion

        #region CORE_METHODS

        private static ISurfaceEntity FromCurveCore(DSCurve profile)
        {
            if (null == profile)
                throw new System.ArgumentNullException("profile");
            if (!profile.IsClosed || profile.IsSelfIntersecting)
                throw new System.ArgumentException(string.Format(Properties.Resources.CurveNotClosed), "profile");

            ISurfaceEntity entity = HostFactory.Factory.SurfacePatchFromCurve(profile.CurveEntity);
            if (entity == null)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "DSSurface.CreateFromCurve"));
            return entity;
        }

        #endregion

        #region PROPERTIES

        public DSCurve Profile
        {
            get { return mProfile; }
            private set { value.AssignTo(ref mProfile); }
        }

        #endregion
    }
}
