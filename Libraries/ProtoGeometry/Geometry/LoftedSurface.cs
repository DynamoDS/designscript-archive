using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    public class LoftedSurface : Surface
    {
        #region DATA MEMBERS
        private Curve[] mCrossSections;
        private Curve[] mGuides;
        private Curve mPath = null;
        #endregion

        #region PRIVATE CONSTRUCTOR
        static void InitType()
        {
        }

        internal LoftedSurface(ISurfaceEntity host, bool persist = false)
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
                GeometryExtension.DisposeObject(ref mCrossSections);
                GeometryExtension.DisposeObject(ref mGuides);
                GeometryExtension.DisposeObject(ref mPath);
            }
            base.Dispose(disposing);
        }

        #endregion

        #region PROTECTED_CONSTRUCTORS

        protected LoftedSurface(Curve[] crossSections,bool persist)
            : base(FromCrossSectionsCore(crossSections),persist)
        {
            InitializeGuaranteedProperties();
            CrossSections = crossSections;
        }

        protected LoftedSurface(Curve[] crossSections, Curve[] guides, bool persist)
            : base(FromCrossSectionsGuidesCore(crossSections, guides),persist)
        {
            InitializeGuaranteedProperties();
            CrossSections = crossSections;
            Guides = guides;
        }

        protected LoftedSurface(Curve[] crossSections, Curve path,bool persist)
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
        public static LoftedSurface FromCrossSections(Curve[] crossSections)
        {
            return new LoftedSurface(crossSections, true);
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
        public static LoftedSurface FromCrossSectionsGuides(Curve[] crossSections, Curve[] guides)
        {
            return new LoftedSurface(crossSections, guides, true);
        }

        /// <summary>
        /// Construct a LoftedSurface by lofting an array of curve cross sections and 
        /// using a curve as lofting path to control the lofting direction.
        /// </summary>
        /// <param name="crossSections">crossSections (curves) needs to be all closed or all open.</param>
        /// <param name="path">lofting path curve.</param>
        /// <returns>Loftedsurface</returns>
        public static LoftedSurface FromCrossSectionsPath(Curve[] crossSections, Curve path)
        {
            return new LoftedSurface(crossSections, path, true);
        }

        #endregion

        #region CORE_METHODS
        
        private static ISurfaceEntity FromCrossSectionsPathCore(Curve[] crossSections, Curve path)
        {
            if (null == path)
                throw new System.ArgumentException(string.Format(Properties.Resources.NullArgument, "path"), "path");

            bool isClosed = crossSections[0].IsClosed;
            //Validation
            ICurveEntity[] hostXCurves = crossSections.ConvertAll((Curve c) => GeometryExtension.GetCurveEntity(c, isClosed));
            if (hostXCurves == null || hostXCurves.Length < 2)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidArguments, "cross sections"), "crossSections");

            ISurfaceEntity entity = HostFactory.Factory.SurfaceByLoft(hostXCurves, path.CurveEntity);
            if (entity == null)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "Surface.LoftFromCrossSectionsPath"));
            return entity;
        }

        private static ISurfaceEntity FromCrossSectionsGuidesCore(Curve[] crossSections, Curve[] guides)
        {
            bool isClosed = crossSections[0].IsClosed;
            //Validation
            ICurveEntity[] hostXCurves = crossSections.ConvertAll((Curve c) => GeometryExtension.GetCurveEntity(c, isClosed));
            if (hostXCurves == null || hostXCurves.Length < 2)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidArguments, "cross sections"), "crossSections");

            ICurveEntity[] hostGuides = guides.ConvertAll(GeometryExtension.ToEntity<Curve, ICurveEntity>);
            if (hostGuides == null || hostGuides.Length < 1)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidArguments, "guides"), "guides");

            ISurfaceEntity entity = HostFactory.Factory.SurfaceByLoftGuides(hostXCurves, hostGuides);
            if (entity == null)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "Surface.SurfaceByLoftGuides"));
            return entity;
        }
        
        private static ISurfaceEntity FromCrossSectionsCore(Curve[] crossSections)
        {
            bool isClosed = crossSections[0].IsClosed;
            //Validation
            ICurveEntity[] hostXCurves = crossSections.ConvertAll((Curve c) => GeometryExtension.GetCurveEntity(c, isClosed));
            if (hostXCurves == null || hostXCurves.Length < 2)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidArguments, "cross sections"), "crossSections");

            ISurfaceEntity entity = HostFactory.Factory.SurfaceByLoft(hostXCurves);
            if (entity == null)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "Surface.LoftFromCrossSections"));
            return entity;
        }

        #endregion

        #region PROPERTIES

        public Curve[] CrossSections
        {
            get { return mCrossSections; }
            private set { value.AssignTo(ref mCrossSections); }
        }

        public Curve[] Guides
        {
            get { return mGuides; }
            private set { value.AssignTo(ref mGuides); }
        }

        public Curve Path 
        {
            get { return mPath; }
            private set { value.AssignTo(ref mPath); }
        }

        #endregion
    }
}
