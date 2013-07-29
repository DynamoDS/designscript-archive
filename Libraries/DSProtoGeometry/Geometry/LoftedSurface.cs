using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    public class DSLoftedSurface : DSSurface
    {
        #region DATA MEMBERS
        private DSCurve[] mCrossSections;
        private DSCurve[] mGuides;
        private DSCurve mPath = null;
        #endregion

        #region PRIVATE CONSTRUCTOR
        static void InitType()
        {
        }

        internal DSLoftedSurface(ISurfaceEntity host, bool persist = false)
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
                DSGeometryExtension.DisposeObject(ref mCrossSections);
                DSGeometryExtension.DisposeObject(ref mGuides);
                DSGeometryExtension.DisposeObject(ref mPath);
            }
            base.Dispose(disposing);
        }

        #endregion

        #region PROTECTED_CONSTRUCTORS

        protected DSLoftedSurface(DSCurve[] crossSections,bool persist)
            : base(FromCrossSectionsCore(crossSections),persist)
        {
            InitializeGuaranteedProperties();
            CrossSections = crossSections;
        }

        protected DSLoftedSurface(DSCurve[] crossSections, DSCurve[] guides, bool persist)
            : base(FromCrossSectionsGuidesCore(crossSections, guides),persist)
        {
            InitializeGuaranteedProperties();
            CrossSections = crossSections;
            Guides = guides;
        }

        protected DSLoftedSurface(DSCurve[] crossSections, DSCurve path,bool persist)
            : base(FromCrossSectionsPathCore(crossSections, path),persist)
        {
            InitializeGuaranteedProperties();
            CrossSections = crossSections;
            Path = path;
        }

        #endregion

        #region DESIGNSCRIPT_CONSTRUCTORS

        /// <summary>
        /// Constructs a Surface by lofting an array of curve cross sections.
        /// </summary>
        /// <param name="crossSections">crossSections (curves) needs to be all closed or all open.</param>
        /// <returns>Loft surface.</returns>
        public static DSLoftedSurface FromCrossSections(DSCurve[] crossSections)
        {
            return new DSLoftedSurface(crossSections, true);
        }

        /// <summary>
        /// Construct a LoftedSurface by lofting an array of curve cross sections and 
        /// using curve(s) as the guide to control the lofting shape.
        /// </summary>
        /// <remarks>
        /// If the cross-section curves do not intersect with the guide curves, 
        /// then these cross-section are ignored.
        /// </remarks>
        /// <param name="crossSections">crossSections (curves) needs to be all closed or all open.</param>
        /// <param name="guides">The guides needs to intersect with all crossSections.</param>
        /// <returns>LoftedSurface</returns>
        public static DSLoftedSurface FromCrossSectionsGuides(DSCurve[] crossSections, DSCurve[] guides)
        {
            return new DSLoftedSurface(crossSections, guides, true);
        }

        /// <summary>
        /// Construct a LoftedSurface by lofting an array of curve cross sections and 
        /// using a curve as lofting path to control the lofting direction.
        /// </summary>
        /// <param name="crossSections">crossSections (curves) needs to be all closed or all open.</param>
        /// <param name="path">lofting path curve.</param>
        /// <returns>Loftedsurface</returns>
        public static DSLoftedSurface FromCrossSectionsPath(DSCurve[] crossSections, DSCurve path)
        {
            return new DSLoftedSurface(crossSections, path, true);
        }

        #endregion

        #region CORE_METHODS
        
        private static ISurfaceEntity FromCrossSectionsPathCore(DSCurve[] crossSections, DSCurve path)
        {
            if (null == path)
                throw new System.ArgumentException(string.Format(Properties.Resources.NullArgument, "path"), "path");

            bool isClosed = crossSections[0].IsClosed;
            //Validation
            ICurveEntity[] hostXCurves = crossSections.ConvertAll((DSCurve c) => DSGeometryExtension.GetCurveEntity(c, isClosed));
            if (hostXCurves == null || hostXCurves.Length < 2)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidArguments, "cross sections"), "crossSections");

            ISurfaceEntity entity = HostFactory.Factory.SurfaceByLoftCrossSectionsPath(hostXCurves, path.CurveEntity);
            if (entity == null)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "DSSurface.LoftFromCrossSectionsPath"));
            return entity;
        }

        private static ISurfaceEntity FromCrossSectionsGuidesCore(DSCurve[] crossSections, DSCurve[] guides)
        {
            bool isClosed = crossSections[0].IsClosed;
            //Validation
            ICurveEntity[] hostXCurves = crossSections.ConvertAll((DSCurve c) => DSGeometryExtension.GetCurveEntity(c, isClosed));
            if (hostXCurves == null || hostXCurves.Length < 2)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidArguments, "cross sections"), "crossSections");

            ICurveEntity[] hostGuides = guides.ConvertAll(DSGeometryExtension.ToEntity<DSCurve, ICurveEntity>);
            if (hostGuides == null || hostGuides.Length < 1)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidArguments, "guides"), "guides");

            ISurfaceEntity entity = HostFactory.Factory.SurfaceByLoftCrossSectionsGuides(hostXCurves, hostGuides);
            if (entity == null)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "DSSurface.LoftFromCrossSectionsGuides"));
            return entity;
        }
        
        private static ISurfaceEntity FromCrossSectionsCore(DSCurve[] crossSections)
        {
            bool isClosed = crossSections[0].IsClosed;
            //Validation
            ICurveEntity[] hostXCurves = crossSections.ConvertAll((DSCurve c) => DSGeometryExtension.GetCurveEntity(c, isClosed));
            if (hostXCurves == null || hostXCurves.Length < 2)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidArguments, "cross sections"), "crossSections");

            ISurfaceEntity entity = HostFactory.Factory.SurfaceByLoftCrossSections(hostXCurves);
            if (entity == null)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "DSSurface.LoftFromCrossSections"));
            return entity;
        }

        #endregion

        #region PROPERTIES

        public DSCurve[] CrossSections
        {
            get { return mCrossSections; }
            private set { value.AssignTo(ref mCrossSections); }
        }

        public DSCurve[] Guides
        {
            get { return mGuides; }
            private set { value.AssignTo(ref mGuides); }
        }

        public DSCurve Path 
        {
            get { return mPath; }
            private set { value.AssignTo(ref mPath); }
        }

        #endregion
    }
}
