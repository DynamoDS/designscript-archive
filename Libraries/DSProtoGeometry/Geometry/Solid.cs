using System;
using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

namespace Autodesk.DesignScript.Geometry
{
    public class DSSolid : DSGeometry
    {
        internal ISolidEntity SolidEntity { get { return HostImpl as ISolidEntity; } }

        #region DATA MEMBERS

        private DSVertex[] mVertices;
        private DSEdge[] mEdges;
        private DSFace[] mFaces;
        private DSShell[] mShells;
        private DSCurve[] mCrossSections;
        private DSCurve mPath;
        private DSCurve[] mGuides;
        private DSCurve mProfile;
        private DSPoint mAxisOrigin;
        private DSLine mAxis;

        #endregion

        #region INTERNAL_METHODS

        private void InitializeGuaranteedProperties()
        {

        }

        #endregion

        #region PRIVATE_CONSTRUCTORS

        static void InitType()
        {

        }

        internal static DSSolid CreateSolid(ISolidEntity host, bool persist)
        {
            return new DSSolid(host, persist);
        }

        internal DSSolid(ISolidEntity entity, bool persist = false)
            : base(entity, persist)
        {
            InitializeGuaranteedProperties();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DSGeometryExtension.DisposeObject(ref mVertices);
                DSGeometryExtension.DisposeObject(ref mEdges);
                DSGeometryExtension.DisposeObject(ref mFaces);
                DSGeometryExtension.DisposeObject(ref mShells);
                DSGeometryExtension.DisposeObject(ref mCrossSections);
                DSGeometryExtension.DisposeObject(ref mPath);
                DSGeometryExtension.DisposeObject(ref mGuides);
                DSGeometryExtension.DisposeObject(ref mProfile);
                DSGeometryExtension.DisposeObject(ref mAxisOrigin);
                DSGeometryExtension.DisposeObject(ref mAxis);
            }
            base.Dispose(disposing);
        }

        #endregion

        #region PROTECTED_CONSTRUCTORS

        protected DSSolid(DSCurve[] crossSections, bool persist)
            : base(LoftFromCrossSectionsCore(crossSections), persist)
        {
            InitializeGuaranteedProperties();
            CrossSections = crossSections;
        }

        protected DSSolid(DSCurve[] crossSections, DSCurve path, bool persist)
            : base(LoftFromCrossSectionsPathCore(crossSections, path), persist)
        {
            InitializeGuaranteedProperties();
            CrossSections = crossSections;
            Path = path;
        }

        protected DSSolid(DSCurve[] crossSections, DSCurve[] guides, bool persist)
            : base(LoftFromCrossSectionsGuidesCore(crossSections, guides), persist)
        {
            InitializeGuaranteedProperties();
            CrossSections = crossSections;
            Guides = guides;
        }

        protected DSSolid(DSCurve profile, DSCurve path, bool persist)
            : base(SweepCore(profile, path), persist)
        {
            InitializeGuaranteedProperties();
            Path = path;
            Profile = profile;
        }

        protected DSSolid(DSCurve profile, DSPoint axisOrigin, DSVector axisDirection, double startAngle, double sweepAngle, bool persist)
            : base(RevolveCore(profile, axisOrigin, axisDirection, startAngle, sweepAngle), persist)
        {
            InitializeGuaranteedProperties();
            Profile = profile;
            AxisOrigin = axisOrigin;
            AxisDirection = axisDirection;
            StartAngle = startAngle;
            SweepAngle = sweepAngle;
        }

        protected DSSolid(DSCurve profile, DSLine axis, double startAngle, double sweepAngle, bool persist)
            : base(RevolveCore(profile, axis, startAngle, sweepAngle), persist)
        {
            InitializeGuaranteedProperties();
            Profile = profile;
            AxisOrigin = axis.StartPoint;
            AxisDirection = axis.Direction;
            Axis = axis;
            StartAngle = startAngle;
            SweepAngle = sweepAngle;
        }

        #endregion

        #region DESIGNSCRIPT_CONSTRUCTORS

        /// <summary>
        /// Construct a Solid by lofting an array of curve cross sections.
        /// </summary>
        /// <param name="crossSections">The input array of curve cross sections</param>
        /// <returns></returns>
        public static DSSolid LoftFromCrossSections(DSCurve[] crossSections)
        {
            return new DSSolid(crossSections, true);
        }

        /// <summary>
        /// Construct a Solid by lofting an array of curve cross sections and using a curve as lofting path to control the lofting direction
        /// </summary>
        /// <param name="crossSections">The input array of curve cross sections</param>
        /// <param name="path">The lofting path to control the lofting direction</param>
        /// <returns></returns>
        public static DSSolid LoftFromCrossSectionsPath(DSCurve[] crossSections, DSCurve path)
        {
            return new DSSolid(crossSections, path, true);
        }

        /// <summary>
        /// Construct a Solid by lofting an array of curve cross sections and using curve(s) as the guide to control the lofting shape.
        /// </summary>
        /// <param name="crossSections">The input array of curve cross sections</param>
        /// <param name="guides">The curve(s) that server as guides to control the lofting shape</param>
        /// <returns></returns>
        public static DSSolid LoftFromCrossSectionsGuides(DSCurve[] crossSections, DSCurve[] guides)
        {
            return new DSSolid(crossSections, guides, true);
        }

        /// <summary>
        /// Constructs a Solid by sweeping a closed profile curve along a path
        /// </summary>
        /// <param name="profile">The closed profile to be swept</param>
        /// <param name="path">The path along which the closed profile will be swept</param>
        /// <returns></returns>
        public static DSSolid Sweep(DSCurve profile, DSCurve path)
        {
            return new DSSolid(profile, path, true);
        }

        /// <summary>
        /// Constructs a Solid by revolving a closed profile curve about an axis defined by axisOrigin point and axisDirection Vector. 
        /// </summary>
        /// <param name="profile">The closed profile to be swept</param>
        /// <param name="axisOrigin">the origin of the axis along which the closed profile curve will be revolved</param>
        /// <param name="axisDirection">The direction of the axis along which the closed profile curve will be revolved</param>
        /// <param name="startAngle">startAngle determines where the curve starts to revolve</param>
        /// <param name="sweepAngle">sweepAngle determines the revolving angle.</param>
        /// <returns></returns>
        public static DSSolid Revolve(DSCurve profile, DSPoint axisOrigin, DSVector axisDirection, double startAngle, double sweepAngle)
        {
            return new DSSolid(profile, axisOrigin, axisDirection, startAngle, sweepAngle, true);
        }

        /// <summary>
        /// Constructs a Solid by revolving  a closed profile curve about an axis defined by axisOrigin point and axisDirection Vector. Assuming sweep angle = 360 and start angle = 0.
        /// </summary>
        /// <param name="profile">The closed profile to be swept</param>
        /// <param name="axisOrigin">the origin of the axis along which the closed profile curve will be revolved</param>
        /// <param name="axisDirection">The direction of the axis along which the closed profile curve will be revolved</param>
        /// <returns></returns>
        public static DSSolid Revolve(DSCurve profile, DSPoint axisOrigin, DSVector axisDirection)
        {
            return new DSSolid(profile, axisOrigin, axisDirection, 0, 360, true);
        }

        /// <summary>
        /// Constructs a Solid by revolving  a closed profile curve about an axis defined by a line axis. 
        /// </summary>
        /// <param name="profile">The closed profile to be swept</param>
        /// <param name="axis">the input axis</param>
        /// <param name="startAngle">startAngle determines where the curve starts to revolve</param>
        /// <param name="sweepAngle">sweepAngle determines the revolving angle.</param>
        /// <returns></returns>
        public static DSSolid Revolve(DSCurve profile, DSLine axis, double startAngle, double sweepAngle)
        {
            return new DSSolid(profile, axis, startAngle, sweepAngle, true);
        }

        /// <summary>
        /// Constructs a Solid by revolving  a closed profile curve about an axis defined by a line axis. Assuming sweep angle = 360 and start angle = 0.
        /// </summary>
        /// <param name="profile">The closed profile to be swept</param>
        /// <param name="axis">the input axis</param>
        /// <returns></returns>
        public static DSSolid Revolve(DSCurve profile, DSLine axis)
        {
            return new DSSolid(profile, axis, 0, 360, true);
        }

        #endregion

        #region CORE_METHODS

        private static ISolidEntity LoftFromCrossSectionsCore(DSCurve[] crossSections)
        {
            //Get all closed host xsections.
            ICurveEntity[] xsections = crossSections.ConvertAll((DSCurve c) => DSGeometryExtension.GetCurveEntity(c, true));
            if (xsections == null || xsections.Length < 2)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidArguments, "cross sections"), "crossSections");

            ISolidEntity entity = HostFactory.Factory.SolidByLoftCrossSections(xsections);
            if (entity == null)
                throw new InvalidOperationException(string.Format(Properties.Resources.OperationFailed, "DSSolid.LoftFromCrossSections"));
            return entity;
        }

        private static ISolidEntity LoftFromCrossSectionsPathCore(DSCurve[] crossSections, DSCurve path)
        {
            //Get all closed host xsections.
            ICurveEntity[] xsections = crossSections.ConvertAll((DSCurve c) => DSGeometryExtension.GetCurveEntity(c, true));
            if (xsections == null || xsections.Length < 2)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidArguments, "cross sections"), "crossSections");
            if (path == null)
                throw new ArgumentNullException("path");

            ISolidEntity entity = HostFactory.Factory.SolidByLoftCrossSectionsPath(xsections, path.CurveEntity);
            if (entity == null)
                throw new InvalidOperationException(string.Format(Properties.Resources.OperationFailed, "DSSolid.LoftFromCrossSectionsPath"));
            return entity;
        }

        private static ISolidEntity LoftFromCrossSectionsGuidesCore(DSCurve[] crossSections, DSCurve[] guides)
        {
            //Get all closed host xsections.
            ICurveEntity[] xsections = crossSections.ConvertAll((DSCurve c) => DSGeometryExtension.GetCurveEntity(c, true));
            if (xsections == null || xsections.Length < 2)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidArguments, "cross sections"), "crossSections");

            ISolidEntity entity = HostFactory.Factory.SolidByLoftCrossSectionsGuides(xsections, guides.ConvertAll(DSGeometryExtension.ToEntity<DSCurve, ICurveEntity>));
            if (entity == null)
                throw new InvalidOperationException(string.Format(Properties.Resources.OperationFailed, "DSSolid.LoftFromCrossSectionsGuides"));
            return entity;
        }

        private static ISolidEntity SweepCore(DSCurve profile, DSCurve path)
        {
            if (profile == null)
                throw new ArgumentNullException("profile");
            if (path == null)
                throw new ArgumentNullException("path");
            if (!profile.IsClosed)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidArguments, "profile"), "profile");

            ISolidEntity entity = HostFactory.Factory.SolidBySweep(profile.CurveEntity, path.CurveEntity);
            if (entity == null)
                throw new InvalidOperationException(string.Format(Properties.Resources.OperationFailed, "DSSolid.Sweep"));
            return entity;
        }

        private static ISolidEntity RevolveCore(DSCurve profile, DSLine axis, double startAngle, double sweepAngle)
        {
            if (profile == null)
                throw new ArgumentNullException("profile");
            if (axis == null)
                throw new ArgumentNullException("axis");
            if (!profile.IsPlanar)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidArguments, "profile"), "profile");
            if (DSGeometryExtension.Equals(axis.Length, 0.0))
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidArguments, "axisDirection"), "axisDirection");
            if (DSGeometryExtension.Equals(sweepAngle, 0.0))
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidArguments, "sweepAngle"), "sweepAngle");

            ISolidEntity entity = HostFactory.Factory.SolidByRevolve(profile.CurveEntity, axis.StartPoint.PointEntity, axis.Direction.IVector, startAngle, sweepAngle);
            if (entity == null)
                throw new InvalidOperationException(string.Format(Properties.Resources.OperationFailed, "DSSolid.Revolve"));
            return entity;
        }

        private static ISolidEntity RevolveCore(DSCurve profile, DSPoint axisOrigin, DSVector axisDirection, double startAngle, double sweepAngle)
        {
            if (profile == null)
                throw new ArgumentNullException("profile");
            if (axisOrigin == null)
                throw new ArgumentNullException("axisOrigin");
            if (axisDirection == null)
                throw new ArgumentNullException("axisDirection");
            if (!profile.IsPlanar)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidArguments, "profile"), "profile");
            if (DSGeometryExtension.Equals(axisDirection.Length, 0.0))
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidArguments, "axisDirection"), "axisDirection");
            if (DSGeometryExtension.Equals(sweepAngle, 0.0))
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidArguments, "sweepAngle"), "sweepAngle");

            ISolidEntity entity = HostFactory.Factory.SolidByRevolve(profile.CurveEntity, axisOrigin.PointEntity, axisDirection.IVector, startAngle, sweepAngle);
            if (entity == null)
                throw new InvalidOperationException(string.Format(Properties.Resources.OperationFailed, "DSSolid.Revolve"));
            return entity;
        }

        #endregion

        #region DESIGNSCRIPT_METHODS

        /// <summary>
        /// Returns a solid by uniting one solid with another solid
        /// </summary>
        /// <param name="otherSolid">The other solid </param>
        /// <returns>Returns a Solid</returns>
        public DSSolid Union(DSSolid otherSolid)
        {
            return Union(otherSolid, true);
        }

        /// <summary>
        /// Returns a solid by doing a non-regular union (if 'isRegular' is set 
        /// to false) of two solids (manifold or non-manifold)
        /// </summary>
        /// <param name="otherSolid">The other solid</param>
        /// <param name="isRegular">Switch for Regular or Non-regular Union</param>
        /// <returns>Solid</returns>
        public DSSolid Union(DSSolid otherSolid, bool isRegular)
        {
            if (otherSolid == null)
                return this;
            IGeometryEntity[] solids = null;
            if (isRegular)
                solids = SolidEntity.UnionWith(otherSolid.SolidEntity);
            else
                solids = SolidEntity.NonRegularUnionWith(otherSolid.SolidEntity);

            if (null == solids || solids.Length == 0)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "DSSolid.Union"));

            ISolidEntity solidhost = solids[0] as ISolidEntity;
            if (solidhost == null)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "DSSolid.Union"));

            return solidhost.ToSolid(true, this);
        }

        /// <summary>
        /// Returns a solid by uniting one solid with an array of solids (any combination of manifold or non-manifold)
        /// </summary>
        /// <param name="otherSolids">An array of solids</param>
        /// <returns>Returns a Solid</returns>
        public DSSolid Union(DSSolid[] otherSolids)
        {
            ISolidEntity[] othersolidhosts = otherSolids.ConvertAll(DSGeometryExtension.ToEntity<DSSolid, ISolidEntity>);
            if (null == othersolidhosts || (othersolidhosts.Length == 0))
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "otherSolids", "DSSolid.Union"), "otherSolids");
            return UnionCore(SolidEntity, othersolidhosts, true);
        }

        /// <summary>
        /// Returns a solid by doing a non-regular unite (if 'isRegular' is set
        /// to false) of one solid with an array of solids (any combination of 
        /// manifold or non-manifold)
        /// </summary>
        /// <param name="otherSolids">An array of solids</param>
        /// <param name="isRegular">Switch for Regular or Non-regular Union</param>
        /// <returns>Returns a solid</returns>
        public DSSolid Union(DSSolid[] otherSolids, bool isRegular)
        {
            ISolidEntity[] othersolidhosts = otherSolids.ConvertAll(DSGeometryExtension.ToEntity<DSSolid, ISolidEntity>);
            if (null == othersolidhosts || (othersolidhosts.Length == 0))
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "otherSolids", "DSSolid.Union"), "otherSolids");

            if (isRegular)
                return UnionCore(SolidEntity, othersolidhosts, true);

            ISolidEntity host = SolidEntity.NonRegularUnionWithMany(othersolidhosts);
            if (host == null)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "DSSolid.Union"));
            return host.ToSolid(true, this);
        }

        /// <summary>
        /// Returns a solid by uniting an array of solids (manifold or non-manifold)
        /// </summary>
        /// <param name="solids">The input array of solids</param>
        /// <returns>Returns a Solid</returns>
        public static DSSolid UnionAll(DSSolid[] solids)
        {
            ISolidEntity[] solidhosts = solids.ConvertAll(DSGeometryExtension.ToEntity<DSSolid, ISolidEntity>);
            if (null == solidhosts || (solidhosts.Length < 2))
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "solids", "DSSolid.UnionAll"), "solids");
            return UnionCore(solidhosts[0], solidhosts, true);
        }

        /// <summary>
        /// Returns a non-manifold solid by imposing the input regular solid onto 
        /// the given solid
        /// </summary>
        /// <param name="otherSolid">The other solid</param>
        /// <returns>Returns a Non-manifold Solid</returns>
        public DSNonManifoldSolid Impose(DSSolid otherSolid)
        {
            if (otherSolid == null)
                throw new ArgumentNullException("otherSolid");

            ISolidEntity host = SolidEntity.NonRegularImpose(otherSolid.SolidEntity);
            if (host == null)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "DSSolid.Impose"));
            return host.ToSolid(true, this) as DSNonManifoldSolid;
        }

        /// <summary>
        /// Returns a solid by subtracting one solid by another solid. 
        /// </summary>
        /// <param name="otherSolid">The other solid</param>
        /// <returns>Returns a Solid</returns>
        public DSSolid Difference(DSSolid otherSolid)
        {
            if (otherSolid == null)
                throw new ArgumentNullException("otherSolid");

            IGeometryEntity[] solids = SolidEntity.SubtractFrom(otherSolid.SolidEntity);
            if (solids == null || solids.Length == 0 || solids[0] == null)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "DSSolid.Impose"));

            ISolidEntity host = solids[0] as ISolidEntity;
            return host.ToSolid(true, this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="otherSolid"></param>
        /// <returns></returns>
        public DSSolid Intersect(DSSolid otherSolid)
        {
            if (otherSolid == null)
                throw new ArgumentNullException("otherSolid");

            IGeometryEntity[] solids = SolidEntity.IntersectWith(otherSolid.SolidEntity);
            if (solids == null || solids.Length == 0 || solids[0] == null)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "DSSolid.Impose"));

            ISolidEntity host = solids[0] as ISolidEntity;
            return host.ToSolid(true, this);
        }

        /// <summary>
        /// Returns disjoint solid bodies
        /// </summary>
        /// <returns>Array of solids</returns>
        public DSSolid[] SeparateSolid()
        {
            IGeometryEntity[] solidHosts = SolidEntity.SeparateSolid();
            if (solidHosts == null || solidHosts.Length == 0)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "DSSolid.SeparateSolid"));
            DSSolid[] sols = solidHosts.ToArray<DSSolid, IGeometryEntity>(true);
            return sols;
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <returns></returns>
        //public Solid Regularise()
        //{
        //    ISolidEntity host = SolidEntity.Regularise();
        //    if (host == null)
        //        throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "Solid.Regularise"));
        //    return host.ToSolid(true, this);
        //}

        /// <summary>
        /// Returns an array of solids (manifold or non-manifold) by slicing 
        /// a Solid (manifold or non-manifold) with a Plane 
        /// </summary>
        /// <param name="plane">The slicing Plane</param>
        /// <returns>Returns an array of Solids (manifold or non-manifold)</returns>
        public DSSolid[] Slice(DSPlane plane)
        {
            return Slice(plane, true);
        }

        /// <summary>
        /// Returns a non-manifold solid by doing a non-regular slice on a 
        /// Solid (manifold or non-manifold) with a Plane 
        /// </summary>
        /// <param name="plane">The slicing Plane</param>
        /// <param name="isRegular">Switch for Regular or Non-regular Slice</param>
        /// <returns>Returns an array of Solids</returns>
        [AllowRankReduction]
        public DSSolid[] Slice(DSPlane plane, bool isRegular)
        {
            if (null == plane)
                throw new ArgumentNullException("plane");

            IGeometryEntity[] solids = null;
            if (isRegular)
                solids = SolidEntity.SliceWithPlane(plane.PlaneEntity);
            else
                solids = new IGeometryEntity[] { SolidEntity.NonRegularSliceWithPlane(plane.PlaneEntity) };
            if (solids == null || solids.Length == 0)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "DSSolid.Slice"));
            return solids.ToArray<DSSolid, IGeometryEntity>(true);
        }

        /// <summary>
        /// Returns an array of solids (manifold or non-manifold) by slicing 
        /// a Solid (manifold or non-manifold) with an array of Planes
        /// </summary>
        /// <param name="planes">Array of slicing Planes</param>
        /// <returns>Returns an array of Solids (manifold or non-manifold)</returns>
        public DSSolid[] Slice(DSPlane[] planes)
        {
            return SliceWithPlanes(planes, true);
        }

        /// <summary>
        /// Returns a non-manifold solid by doing a non-regular slice on 
        /// a Solid (manifold or non-manifold) with an array of Planes 
        /// </summary>
        /// <param name="planes">Array of slicing Planes</param>
        /// <param name="isRegular">Switch for Regular or Non-regular Slice</param>
        /// <returns>Returns an array of Solids (manifold or non-manifold)</returns>
        [AllowRankReduction]
        public DSSolid[] Slice(DSPlane[] planes, bool isRegular)
        {
            return SliceWithPlanes(planes, isRegular);
        }

        private DSSolid[] SliceWithPlanes(DSPlane[] planes, bool isRegular)
        {
            IPlaneEntity[] planeHosts = planes.ConvertAll(DSGeometryExtension.ToEntity<DSPlane, IPlaneEntity>);
            if (null == planeHosts || planeHosts.Length == 0)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "planes", "DSSolid.SliceWithPlanes"), "planes");
            IGeometryEntity[] solids = null;
            if (isRegular)
                solids = SolidEntity.SliceWithPlanes(planeHosts);
            else
                solids = new IGeometryEntity[] { SolidEntity.NonRegularSliceWithPlanes(planeHosts) };
            if (solids == null || solids.Length == 0)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "DSSolid.SliceWithPlanes"));
            return solids.ToArray<DSSolid, IGeometryEntity>(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="surfaces"></param>
        /// <param name="isRegular"></param>
        /// <returns></returns>
        [AllowRankReduction]
        public DSSolid[] Slice(DSSurface[] surfaces, bool isRegular)
        {
            ISurfaceEntity[] surfaceHosts = surfaces.ConvertAll(DSGeometryExtension.ToEntity<DSSurface, ISurfaceEntity>);
            if (null == surfaceHosts || surfaceHosts.Length == 0)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "surfaces", "DSSolid.Slice"), "surfaces");
            IGeometryEntity[] solids = null;
            if (isRegular)
                solids = SolidEntity.SliceWithSurfaces(surfaceHosts);
            else
                solids = new[] { SolidEntity.NonRegularSliceWithSurfaces(surfaceHosts) };
            if (solids == null || solids.Length == 0)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "DSSolid.Slice"));
            return solids.ToArray<DSSolid, IGeometryEntity>(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="surface"></param>
        /// <param name="isRegular"></param>
        /// <returns></returns>
        [AllowRankReduction]
        public DSSolid[] Slice(DSSurface surface, bool isRegular)
        {
            if (null == surface)
                throw new ArgumentNullException("surface");

            IGeometryEntity[] solids = null;
            if (isRegular)
                solids = SolidEntity.SliceWithSurface(surface.SurfaceEntity);
            else
                solids = new[] { SolidEntity.NonRegularSliceWithSurface(surface.SurfaceEntity) };
            if (solids == null || solids.Length == 0)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "DSSolid.Slice"));
            return solids.ToArray<DSSolid, IGeometryEntity>(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uniformFaceThickness"></param>
        /// <returns></returns>
        public DSSolid ThinShell(double uniformFaceThickness)
        {
            if (uniformFaceThickness.EqualsTo(0.0))
                throw new ArgumentException(string.Format(Properties.Resources.IsZero, "uniformFaceThickness"), "uniformFaceThickness");
            if (uniformFaceThickness < 0)
                throw new ArgumentException(string.Format(Properties.Resources.LessThanZero, "uniformFaceThickness"), "uniformFaceThickness");

            ISolidEntity host = SolidEntity.ThinShell(uniformFaceThickness, uniformFaceThickness);
            if (null == host)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "DSSolid.ThinShell"));

            return host.ToSolid(true, this);
        }

        // TODO: To be fixed - pratapa
        /// <summary>
        /// 
        /// </summary>
        /// <param name="planes"></param>
        /// <param name="surfaces"></param>
        /// <param name="solids"></param>
        /// <param name="selectPoint"></param>
        /// <returns></returns>
        public DSSolid Trim(DSPlane[] planes, DSSurface[] surfaces, DSSolid[] solids, DSPoint selectPoint)
        {
            IPlaneEntity[] hostPlanes = planes.ConvertAll(DSGeometryExtension.ToEntity<DSPlane, IPlaneEntity>);
            ISurfaceEntity[] hostSurfaces = surfaces.ConvertAll(DSGeometryExtension.ToEntity<DSSurface, ISurfaceEntity>);
            ISolidEntity[] hostSolids = solids.ConvertAll(DSGeometryExtension.ToEntity<DSSolid, ISolidEntity>);
            if (selectPoint == null)
                throw new System.ArgumentNullException("selectPoint");
            IPointEntity hostPoint = selectPoint.PointEntity;
            if (hostPlanes == null && hostSurfaces == null && hostSolids == null)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "DSGeometry", "DSSolid.Trim"));

            ISolidEntity trimSolid = SolidEntity.Trim(hostPlanes, hostSurfaces, hostSolids, hostPoint);
            if (null == trimSolid)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "DSSolid.Trim"));

            Hide(planes);
            Hide(surfaces);
            Hide(solids);
            SetVisibility(false);

            return new DSSolid(trimSolid, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="selectPoint"></param>
        /// <returns></returns>
        public DSSolid Trim(DSPlane plane, DSPoint selectPoint)
        {
            DSPlane[] planes = { plane };
            return Trim(planes, null, null, selectPoint);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="planes"></param>
        /// <param name="selectPoint"></param>
        /// <returns></returns>
        public DSSolid Trim(DSPlane[] planes, DSPoint selectPoint)
        {
            return Trim(planes, null, null, selectPoint);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="surface"></param>
        /// <param name="selectPoint"></param>
        /// <returns></returns>
        public DSSolid Trim(DSSurface surface, DSPoint selectPoint)
        {
            DSSurface[] surfaces = { surface };
            return Trim(null, surfaces, null, selectPoint);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="surfaces"></param>
        /// <param name="selectPoint"></param>
        /// <returns></returns>
        public DSSolid Trim(DSSurface[] surfaces, DSPoint selectPoint)
        {
            return Trim(null, surfaces, null, selectPoint);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="solid"></param>
        /// <param name="selectPoint"></param>
        /// <returns></returns>
        public DSSolid Trim(DSSolid solid, DSPoint selectPoint)
        {
            DSSolid[] solids = { solid };
            return Trim(null, null, solids, selectPoint);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="solids"></param>
        /// <param name="selectPoint"></param>
        /// <returns></returns>
        public DSSolid Trim(DSSolid[] solids, DSPoint selectPoint)
        {
            return Trim(null, null, solids, selectPoint);
        }

        /// <summary>
        /// Returns an array of solids by trimming an array of solids with 
        /// a trimming solid. It will either return part of solids inside the
        /// trimSolid or outside the trimSolid based on flag keepInside.
        /// </summary>
        /// <param name="solids">The input array of solids</param>
        /// <param name="trimmingSolid">The trimming solid</param>
        /// <param name="keepInside">
        /// Depending on the keepInside input, the input solids or those 
        /// parts of the input solids inside the trimSolid or outside the
        /// trimSolid will be returned</param>
        /// <returns>Returns an array of trimmed Solids</returns>
        [AllowRankReduction]
        public static DSSolid[] SelectTrim(DSSolid[] solids, DSSolid trimmingSolid, bool keepInside)
        {
            string kMethodName = "DSSolid.SelectTrim";

            if (null == trimmingSolid)
                throw new System.ArgumentNullException("trimmingSolid");

            ISolidEntity[] solidhosts = solids.ConvertAll(DSGeometryExtension.ToEntity<DSSolid, ISolidEntity>);
            if (solidhosts == null || solidhosts.Length == 0)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "solids", kMethodName), "solids");

            ISolidEntity trimmingEntity = trimmingSolid.SolidEntity;
            DSSolid[] result = SelectTrimCore(solidhosts, trimmingEntity, keepInside);
            if (null != result)
            {
                Hide(solids);
                Hide(trimmingSolid);
            }
            return result;
        }

        /// <summary>
        /// Returns an array of solids by trimming an array of solids with 
        /// an array of trimming solids. It will either return part of solids 
        /// inside the trimSolids or outside the trimSolids based on flag 
        /// keepInside.
        /// </summary>
        /// <param name="solids">The input array of solids</param>
        /// <param name="trimmingSolids">The array of trimming solids</param>
        /// <param name="keepInside">
        /// Depending on the keepInside input, the input solids or those 
        /// parts of the input solids inside the trimSolids or outside the
        /// trimSolids will be returned</param>
        /// <returns>Returns an array of trimmed Solids</returns>
        [AllowRankReduction]
        public static DSSolid[] SelectTrim(DSSolid[] solids, DSSolid[] trimmingSolids, bool keepInside)
        {
            string kMethodName = "DSSolid.SelectTrim";
            ISolidEntity[] solidhosts = solids.ConvertAll(DSGeometryExtension.ToEntity<DSSolid, ISolidEntity>);
            if (solidhosts == null || solidhosts.Length == 0)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "solids", kMethodName), "solids");

            ISolidEntity[] trimminghosts = trimmingSolids.ConvertAll(DSGeometryExtension.ToEntity<DSSolid, ISolidEntity>);
            if (null == trimminghosts || trimminghosts.Length == 0)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "trimmingSolids", kMethodName), "trimmingSolids");

            DSSolid[] result = null;
            if (trimminghosts.Length == 1)
            {
                result = SelectTrimCore(solidhosts, trimminghosts[0], keepInside);
            }
            else
            {
                DSSolid unionSolid = DSSolid.UnionCore(trimminghosts[0], trimminghosts, false);
                if (null == unionSolid)
                    throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));

                result = SelectTrimCore(solidhosts, unionSolid.SolidEntity, keepInside);
            }
            if (null != result)
            {
                Hide(solids);
                Hide(trimmingSolids);
            }
            return result;
        }

        [AllowRankReduction]
        new public static DSSolid[] ImportFromSAT(string filePath)
        {
            IGeometryEntity[] objects = ImportFromSAT(ref filePath);
            return  objects.ToArray<DSSolid, IGeometryEntity>();
        }

        #endregion

        #region PROPERTIES

        /// <summary>
        /// 
        /// </summary>
        [Category("Primary")]
        public double Area
        {
            get
            {
                return SolidEntity.GetArea();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Category("Primary")]
        public double Volume
        {
            get
            {
                return SolidEntity.GetVolume();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Category("Primary")]
        public DSPoint Centroid
        {
            get
            {
                return SolidEntity.GetCentroid().ToPoint(false, this);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DSCurve[] CrossSections
        {
            get { return mCrossSections; }
            set { value.AssignTo(ref mCrossSections); }
        }

        /// <summary>
        /// 
        /// </summary>
        public DSCurve Path
        {
            get { return mPath; }
            set { value.AssignTo(ref mPath); }
        }

        /// <summary>
        /// 
        /// </summary>
        public DSCurve[] Guides
        {
            get { return mGuides; }
            set { value.AssignTo(ref mGuides); }
        }

        /// <summary>
        /// 
        /// </summary>
        public DSCurve Profile
        {
            get { return mProfile; }
            set { value.AssignTo(ref mProfile); }
        }

        /// <summary>
        /// 
        /// </summary>
        public DSPoint AxisOrigin
        {
            get { return mAxisOrigin; }
            set { value.AssignTo(ref mAxisOrigin); }
        }

        /// <summary>
        /// 
        /// </summary>
        public DSLine Axis
        {
            get { return mAxis; }
            set { value.AssignTo(ref mAxis); }
        }

        /// <summary>
        /// 
        /// </summary>
        public DSVector AxisDirection { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public double? StartAngle { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public double? SweepAngle { get; private set; }

        #endregion

        #region TOPOLOGY_PROPERTIES

        /// <summary>
        /// Accesses the Solid Vertices 
        /// </summary>
        public DSVertex[] Vertices
        {
            get
            {
                if (null == mVertices)
                {
                    IVertexEntity[] vertices = SolidEntity.GetVertices();
                    mVertices = vertices.ConvertAll((IVertexEntity host) => new DSVertex(host));
                }

                return mVertices;
            }
        }

        /// <summary>
        /// Accesses the Solid Edges 
        /// </summary>
        public DSEdge[] Edges
        {
            get
            {
                if (null == mEdges)
                {
                    IEdgeEntity[] edges = SolidEntity.GetEdges();
                    mEdges = edges.ConvertAll((IEdgeEntity host) => new DSEdge(host));
                }

                return mEdges;
            }
        }

        /// <summary>
        /// Accesses the Solid Faces
        /// </summary>
        public DSFace[] Faces
        {
            get
            {
                if (null == mFaces)
                {
                    IFaceEntity[] faces = SolidEntity.GetFaces();
                    mFaces = faces.ConvertAll((IFaceEntity host) => new DSFace(host));
                }

                return mFaces;
            }
        }

        public DSShell[] Shells
        {
            get
            {
                if (null == mShells)
                    mShells = SolidEntity.GetShells().ConvertAll((IShellEntity host) => new DSShell(host));

                return mShells;
            }
        }
        #endregion

        #region OBJECT_METHODS

        protected string GetTopologyInfo()
        {
            int nFaces = SolidEntity.GetFaceCount();
            int nShells = SolidEntity.GetShellCount();
            int nEdges = SolidEntity.GetEdgeCount();
            int nVertices = SolidEntity.GetVertexCount();

            return string.Format("Vertices = {0}, Edges = {1}, Faces = {2}, Shells = {3}", nVertices, nEdges, nFaces, nShells);
        }

        public override string ToString()
        {
            return string.Format("{0}({1})", GetType().Name, GetTopologyInfo());
        }

        #endregion

        #region PROTECTED_METHODS

        internal override IGeometryEntity[] IntersectWithCurve(DSCurve curve)
        {
            return curve.CurveEntity.IntersectWith(SolidEntity);
        }

        internal override IGeometryEntity[] IntersectWithPlane(DSPlane plane)
        {
            return SolidEntity.IntersectWith(plane.PlaneEntity);
        }

        internal override IGeometryEntity[] IntersectWithSolid(DSSolid solid)
        {
            return SolidEntity.IntersectWith(solid.SolidEntity);
        }

        internal override IGeometryEntity[] IntersectWithSurface(DSSurface surf)
        {
            return SolidEntity.IntersectWith(surf.SurfaceEntity);
        }

        private static DSSolid[] SelectTrimCore(ISolidEntity[] solidhosts, ISolidEntity trimmingEntity, bool keepInside)
        {
            List<IGeometryEntity> geometries = new List<IGeometryEntity>();
            foreach (var solid in solidhosts)
            {
                IGeometryEntity[] geometryHosts;
                if (keepInside)
                    geometryHosts = trimmingEntity.IntersectWith(solid);
                else
                    geometryHosts = solid.SubtractFrom(trimmingEntity);

                if (null != geometryHosts)
                    geometries.AddRange(geometryHosts);
            }

            IGeometryEntity[] geoms = geometries.ToArray();
            return geoms.ConvertAll(
                (IGeometryEntity g) =>
                {
                    ISolidEntity s = g as ISolidEntity;
                    if (null == s)
                        return null;
                    return s.ToSolid(true, null);
                });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="thisEntity"></param>
        /// <param name="othersolidhosts"></param>
        /// <param name="persist"></param>
        /// <returns></returns>
        internal static DSSolid UnionCore(ISolidEntity thisEntity, ISolidEntity[] othersolidhosts, bool persist)
        {
            ISolidEntity host = thisEntity;
            foreach (var solidhost in othersolidhosts)
            {
                if (host == solidhost)
                    continue;

                IGeometryEntity[] solids = host.UnionWith(solidhost);
                if (solids != null && solids.Length > 0 && solids[0] != null)
                    host = solids[0] as ISolidEntity;
                else
                    throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "DSSolid.Union"));
            }

            return host.ToSolid(persist, null);
        }
        #endregion
    }
}
