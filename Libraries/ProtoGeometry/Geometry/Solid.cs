using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

namespace Autodesk.DesignScript.Geometry
{
    public class Solid : Geometry
    {
        internal ISolidEntity SolidEntity { get { return HostImpl as ISolidEntity; } }

        #region DATA MEMBERS

        private Vertex[] mVertices;
        private Edge[] mEdges;
        private Face[] mFaces;
        private Shell[] mShells;
        private Curve[] mCrossSections;
        private Curve mPath;
        private Curve[] mGuides;
        private Curve mProfile;
        private Point mAxisOrigin;
        private Line mAxis;

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

        internal static Solid CreateSolid(ISolidEntity host, bool persist)
        {
            return new Solid(host, persist);
        }

        internal Solid(ISolidEntity entity, bool persist = false)
            : base(entity, persist)
        {
            InitializeGuaranteedProperties();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GeometryExtension.DisposeObject(ref mVertices);
                GeometryExtension.DisposeObject(ref mEdges);
                GeometryExtension.DisposeObject(ref mFaces);
                GeometryExtension.DisposeObject(ref mShells);
                GeometryExtension.DisposeObject(ref mCrossSections);
                GeometryExtension.DisposeObject(ref mPath);
                GeometryExtension.DisposeObject(ref mGuides);
                GeometryExtension.DisposeObject(ref mProfile);
                GeometryExtension.DisposeObject(ref mAxisOrigin);
                GeometryExtension.DisposeObject(ref mAxis);
            }
            base.Dispose(disposing);
        }

        #endregion

        #region PROTECTED_CONSTRUCTORS

        protected Solid(Curve[] crossSections, bool persist)
            : base(LoftFromCrossSectionsCore(crossSections), persist)
        {
            InitializeGuaranteedProperties();
            CrossSections = crossSections;
        }

        protected Solid(Curve[] crossSections, Curve path, bool persist)
            : base(LoftFromCrossSectionsPathCore(crossSections, path), persist)
        {
            InitializeGuaranteedProperties();
            CrossSections = crossSections;
            Path = path;
        }

        protected Solid(Curve[] crossSections, Curve[] guides, bool persist)
            : base(LoftFromCrossSectionsGuidesCore(crossSections, guides), persist)
        {
            InitializeGuaranteedProperties();
            CrossSections = crossSections;
            Guides = guides;
        }

        protected Solid(Curve profile, Curve path, bool persist)
            : base(SweepCore(profile, path), persist)
        {
            InitializeGuaranteedProperties();
            Path = path;
            Profile = profile;
        }

        protected Solid(Curve profile, Point axisOrigin, Vector axisDirection, double startAngle, double sweepAngle, bool persist)
            : base(RevolveCore(profile, axisOrigin, axisDirection, startAngle, sweepAngle), persist)
        {
            InitializeGuaranteedProperties();
            Profile = profile;
            AxisOrigin = axisOrigin;
            AxisDirection = axisDirection;
            StartAngle = startAngle;
            SweepAngle = sweepAngle;
        }

        protected Solid(Curve profile, Line axis, double startAngle, double sweepAngle, bool persist)
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
        public static Solid LoftFromCrossSections(Curve[] crossSections)
        {
            return new Solid(crossSections, true);
        }

        /// <summary>
        /// Construct a Solid by lofting an array of curve cross sections and using a curve as lofting path to control the lofting direction
        /// </summary>
        /// <param name="crossSections">The input array of curve cross sections</param>
        /// <param name="path">The lofting path to control the lofting direction</param>
        /// <returns></returns>
        public static Solid LoftFromCrossSectionsPath(Curve[] crossSections, Curve path)
        {
            return new Solid(crossSections, path, true);
        }

        /// <summary>
        /// Construct a Solid by lofting an array of curve cross sections and using curve(s) as the guide to control the lofting shape.
        /// </summary>
        /// <param name="crossSections">The input array of curve cross sections</param>
        /// <param name="guides">The curve(s) that server as guides to control the lofting shape</param>
        /// <returns></returns>
        public static Solid LoftFromCrossSectionsGuides(Curve[] crossSections, Curve[] guides)
        {
            return new Solid(crossSections, guides, true);
        }

        /// <summary>
        /// Constructs a Solid by sweeping a closed profile curve along a path
        /// </summary>
        /// <param name="profile">The closed profile to be swept</param>
        /// <param name="path">The path along which the closed profile will be swept</param>
        /// <returns></returns>
        public static Solid Sweep(Curve profile, Curve path)
        {
            return new Solid(profile, path, true);
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
        public static Solid Revolve(Curve profile, Point axisOrigin, Vector axisDirection, double startAngle, double sweepAngle)
        {
            return new Solid(profile, axisOrigin, axisDirection, startAngle, sweepAngle, true);
        }

        /// <summary>
        /// Constructs a Solid by revolving  a closed profile curve about an axis defined by axisOrigin point and axisDirection Vector. Assuming sweep angle = 360 and start angle = 0.
        /// </summary>
        /// <param name="profile">The closed profile to be swept</param>
        /// <param name="axisOrigin">the origin of the axis along which the closed profile curve will be revolved</param>
        /// <param name="axisDirection">The direction of the axis along which the closed profile curve will be revolved</param>
        /// <returns></returns>
        public static Solid Revolve(Curve profile, Point axisOrigin, Vector axisDirection)
        {
            return new Solid(profile, axisOrigin, axisDirection, 0, 360, true);
        }

        /// <summary>
        /// Constructs a Solid by revolving  a closed profile curve about an axis defined by a line axis. 
        /// </summary>
        /// <param name="profile">The closed profile to be swept</param>
        /// <param name="axis">the input axis</param>
        /// <param name="startAngle">startAngle determines where the curve starts to revolve</param>
        /// <param name="sweepAngle">sweepAngle determines the revolving angle.</param>
        /// <returns></returns>
        public static Solid Revolve(Curve profile, Line axis, double startAngle, double sweepAngle)
        {
            return new Solid(profile, axis, startAngle, sweepAngle, true);
        }

        /// <summary>
        /// Constructs a Solid by revolving  a closed profile curve about an axis defined by a line axis. Assuming sweep angle = 360 and start angle = 0.
        /// </summary>
        /// <param name="profile">The closed profile to be swept</param>
        /// <param name="axis">the input axis</param>
        /// <returns></returns>
        public static Solid Revolve(Curve profile, Line axis)
        {
            return new Solid(profile, axis, 0, 360, true);
        }

        #endregion

        #region CORE_METHODS

        private static ISolidEntity LoftFromCrossSectionsCore(Curve[] crossSections)
        {
            //Get all closed host xsections.
            ICurveEntity[] xsections = crossSections.ConvertAll((Curve c) => GeometryExtension.GetCurveEntity(c, true));
            if (xsections == null || xsections.Length < 2)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidArguments, "cross sections"), "crossSections");

            ISolidEntity entity = HostFactory.Factory.SolidByLoftCrossSections(xsections);
            if (entity == null)
                throw new InvalidOperationException(string.Format(Properties.Resources.OperationFailed, "Solid.LoftFromCrossSections"));
            return entity;
        }

        private static ISolidEntity LoftFromCrossSectionsPathCore(Curve[] crossSections, Curve path)
        {
            //Get all closed host xsections.
            ICurveEntity[] xsections = crossSections.ConvertAll((Curve c) => GeometryExtension.GetCurveEntity(c, true));
            if (xsections == null || xsections.Length < 2)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidArguments, "cross sections"), "crossSections");
            if (path == null)
                throw new ArgumentNullException("path");

            ISolidEntity entity = HostFactory.Factory.SolidByLoftCrossSectionsPath(xsections, path.CurveEntity);
            if (entity == null)
                throw new InvalidOperationException(string.Format(Properties.Resources.OperationFailed, "Solid.LoftFromCrossSectionsPath"));
            return entity;
        }

        private static ISolidEntity LoftFromCrossSectionsGuidesCore(Curve[] crossSections, Curve[] guides)
        {
            //Get all closed host xsections.
            ICurveEntity[] xsections = crossSections.ConvertAll((Curve c) => GeometryExtension.GetCurveEntity(c, true));
            if (xsections == null || xsections.Length < 2)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidArguments, "cross sections"), "crossSections");

            ISolidEntity entity = HostFactory.Factory.SolidByLoftCrossSectionsGuides(xsections, guides.ConvertAll(GeometryExtension.ToEntity<Curve, ICurveEntity>));
            if (entity == null)
                throw new InvalidOperationException(string.Format(Properties.Resources.OperationFailed, "Solid.LoftFromCrossSectionsGuides"));
            return entity;
        }

        private static ISolidEntity SweepCore(Curve profile, Curve path)
        {
            if (profile == null)
                throw new ArgumentNullException("profile");
            if (path == null)
                throw new ArgumentNullException("path");
            if (!profile.IsClosed)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidArguments, "profile"), "profile");

            ISolidEntity entity = HostFactory.Factory.SolidBySweep(profile.CurveEntity, path.CurveEntity);
            if (entity == null)
                throw new InvalidOperationException(string.Format(Properties.Resources.OperationFailed, "Solid.Sweep"));
            return entity;
        }

        private static ISolidEntity RevolveCore(Curve profile, Line axis, double startAngle, double sweepAngle)
        {
            if (profile == null)
                throw new ArgumentNullException("profile");
            if (axis == null)
                throw new ArgumentNullException("axis");
            if (!profile.IsPlanar)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidArguments, "profile"), "profile");
            if (GeometryExtension.Equals(axis.Length, 0.0))
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidArguments, "axisDirection"), "axisDirection");
            if (GeometryExtension.Equals(sweepAngle, 0.0))
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidArguments, "sweepAngle"), "sweepAngle");

            ISolidEntity entity = HostFactory.Factory.SolidByRevolve(profile.CurveEntity, axis.StartPoint.PointEntity, axis.Direction.VectorEntity, startAngle, sweepAngle);
            if (entity == null)
                throw new InvalidOperationException(string.Format(Properties.Resources.OperationFailed, "Solid.Revolve"));
            return entity;
        }

        private static ISolidEntity RevolveCore(Curve profile, Point axisOrigin, Vector axisDirection, double startAngle, double sweepAngle)
        {
            if (profile == null)
                throw new ArgumentNullException("profile");
            if (axisOrigin == null)
                throw new ArgumentNullException("axisOrigin");
            if (axisDirection == null)
                throw new ArgumentNullException("axisDirection");
            if (!profile.IsPlanar)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidArguments, "profile"), "profile");
            if (GeometryExtension.Equals(axisDirection.Length, 0.0))
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidArguments, "axisDirection"), "axisDirection");
            if (GeometryExtension.Equals(sweepAngle, 0.0))
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidArguments, "sweepAngle"), "sweepAngle");

            ISolidEntity entity = HostFactory.Factory.SolidByRevolve(profile.CurveEntity, axisOrigin.PointEntity, axisDirection.VectorEntity, startAngle, sweepAngle);
            if (entity == null)
                throw new InvalidOperationException(string.Format(Properties.Resources.OperationFailed, "Solid.Revolve"));
            return entity;
        }

        #endregion

        #region DESIGNSCRIPT_METHODS

        /// <summary>
        /// Returns a solid by uniting one solid with another solid
        /// </summary>
        /// <param name="otherSolid">The other solid </param>
        /// <returns>Returns a Solid</returns>
        public Solid Union(Solid otherSolid)
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
        public Solid Union(Solid otherSolid, bool isRegular)
        {
            if (otherSolid == null)
                return this;
            ISolidEntity solidHost = null;
            solidHost = SolidEntity.CSGUnion(otherSolid.SolidEntity);

            if (null == solidHost)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "Solid.Union"));

            return solidHost.ToSolid(true, this);
        }

        /// <summary>
        /// Returns a solid by uniting one solid with an array of solids (any combination of manifold or non-manifold)
        /// </summary>
        /// <param name="otherSolids">An array of solids</param>
        /// <returns>Returns a Solid</returns>
        public Solid Union(Solid[] otherSolids)
        {
            ISolidEntity[] othersolidhosts = otherSolids.ConvertAll(GeometryExtension.ToEntity<Solid, ISolidEntity>);
            if (null == othersolidhosts || (othersolidhosts.Length == 0))
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "otherSolids", "Solid.Union"), "otherSolids");
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
        public Solid Union(Solid[] otherSolids, bool isRegular)
        {
            ISolidEntity[] othersolidhosts = otherSolids.ConvertAll(GeometryExtension.ToEntity<Solid, ISolidEntity>);
            if (null == othersolidhosts || (othersolidhosts.Length == 0))
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "otherSolids", "Solid.Union"), "otherSolids");

            if (isRegular)
                return UnionCore(SolidEntity, othersolidhosts, true);

            ISolidEntity host = this.SolidEntity;

            for (var i = 0; i < othersolidhosts.Length; i++)
            {
                host = SolidEntity.CSGUnion(othersolidhosts[i]);
            }
            
            if (host == null)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "Solid.Union"));
            return host.ToSolid(true, this);
        }

        /// <summary>
        /// Returns a solid by uniting an array of solids (manifold or non-manifold)
        /// </summary>
        /// <param name="solids">The input array of solids</param>
        /// <returns>Returns a Solid</returns>
        public static Solid UnionAll(Solid[] solids)
        {
            ISolidEntity[] solidhosts = solids.ConvertAll(GeometryExtension.ToEntity<Solid, ISolidEntity>);
            if (null == solidhosts || (solidhosts.Length < 2))
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "solids", "Solid.UnionAll"), "solids");
            return UnionCore(solidhosts[0], solidhosts, true);
        }

#region Impose

        /*
        /// <summary>
        /// Returns a non-manifold solid by imposing the input regular solid onto 
        /// the given solid
        /// </summary>
        /// <param name="otherSolid">The other solid</param>
        /// <returns>Returns a Non-manifold Solid</returns>
        public NonManifoldSolid Impose(Solid otherSolid)
        {
            if (otherSolid == null)
                throw new ArgumentNullException("otherSolid");

            ISolidEntity host = SolidEntity.(otherSolid.SolidEntity);
            if (host == null)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "Solid.Impose"));
            return host.ToSolid(true, this) as NonManifoldSolid;
        }
         * */

#endregion

        /// <summary>
        /// Returns a solid by subtracting one solid by another solid. 
        /// </summary>
        /// <param name="otherSolid">The other solid</param>
        /// <returns>Returns a Solid</returns>
        public Solid Difference(Solid otherSolid)
        {
            if (otherSolid == null)
                throw new ArgumentNullException("otherSolid");

            var solid = SolidEntity.CSGDifference(otherSolid.SolidEntity);

            if (solid == null)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "Solid.Difference"));

            return solid.ToSolid(true, this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="otherSolid"></param>
        /// <returns></returns>
        public Solid Intersect(Solid otherSolid)
        {
            if (otherSolid == null)
                throw new ArgumentNullException("otherSolid");

            IGeometryEntity[] solids = SolidEntity.Intersect(otherSolid.SolidEntity);
            if (solids == null || solids.Length == 0 || solids[0] == null)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "Solid.Intersect"));

            ISolidEntity host = solids[0] as ISolidEntity;
            return host.ToSolid(true, this);
        }

        /// <summary>
        /// Returns disjoint solid bodies
        /// </summary>
        /// <returns>Array of solids</returns>
        public Solid[] SeparateSolid()
        {
            IGeometryEntity[] solidHosts = SolidEntity.SeparateSolid();
            if (solidHosts == null || solidHosts.Length == 0)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "Solid.SeparateSolid"));
            Solid[] sols = solidHosts.ToArray<Solid, IGeometryEntity>(true);
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

        #region Commented out slice

        /*

        /// <summary>
        /// Returns an array of solids (manifold or non-manifold) by slicing 
        /// a Solid (manifold or non-manifold) with a Plane 
        /// </summary>
        /// <param name="plane">The slicing Plane</param>
        /// <returns>Returns an array of Solids (manifold or non-manifold)</returns>
        public Solid[] Slice(Plane plane)
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
        public Solid[] Slice(Plane plane, bool isRegular)
        {
            if (null == plane)
                throw new ArgumentNullException("plane");

            IGeometryEntity[] solids = null;
            if (isRegular)
                solids = SolidEntity.SliceWithPlane(plane.PlaneEntity);
            else
                solids = new IGeometryEntity[] { SolidEntity.NonRegularSliceWithPlane(plane.PlaneEntity) };
            if (solids == null || solids.Length == 0)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "Solid.Slice"));
            return solids.ToArray<Solid, IGeometryEntity>(true);
        }

        /// <summary>
        /// Returns an array of solids (manifold or non-manifold) by slicing 
        /// a Solid (manifold or non-manifold) with an array of Planes
        /// </summary>
        /// <param name="planes">Array of slicing Planes</param>
        /// <returns>Returns an array of Solids (manifold or non-manifold)</returns>
        public Solid[] Slice(Plane[] planes)
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
        public Solid[] Slice(Plane[] planes, bool isRegular)
        {
            return SliceWithPlanes(planes, isRegular);
        }

        private Solid[] SliceWithPlanes(Plane[] planes, bool isRegular)
        {
            IPlaneEntity[] planeHosts = planes.ConvertAll(GeometryExtension.ToEntity<Plane, IPlaneEntity>);
            if (null == planeHosts || planeHosts.Length == 0)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "planes", "Solid.SliceWithPlanes"), "planes");
            IGeometryEntity[] solids = null;
            if (isRegular)
                solids = SolidEntity.SliceWithPlanes(planeHosts);
            else
                solids = new IGeometryEntity[] { SolidEntity.NonRegularSliceWithPlanes(planeHosts) };
            if (solids == null || solids.Length == 0)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "Solid.SliceWithPlanes"));
            return solids.ToArray<Solid, IGeometryEntity>(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="surfaces"></param>
        /// <param name="isRegular"></param>
        /// <returns></returns>
        [AllowRankReduction]
        public Solid[] Slice(Surface[] surfaces, bool isRegular)
        {
            ISurfaceEntity[] surfaceHosts = surfaces.ConvertAll(GeometryExtension.ToEntity<Surface, ISurfaceEntity>);
            if (null == surfaceHosts || surfaceHosts.Length == 0)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "surfaces", "Solid.Slice"), "surfaces");
            IGeometryEntity[] solids = null;
            if (isRegular)
                solids = SolidEntity.SliceWithSurfaces(surfaceHosts);
            else
                solids = new[] { SolidEntity.NonRegularSliceWithSurfaces(surfaceHosts) };
            if (solids == null || solids.Length == 0)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "Solid.Slice"));
            return solids.ToArray<Solid, IGeometryEntity>(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="surface"></param>
        /// <param name="isRegular"></param>
        /// <returns></returns>
        [AllowRankReduction]
        public Solid[] Slice(Surface surface, bool isRegular)
        {
            if (null == surface)
                throw new ArgumentNullException("surface");

            IGeometryEntity[] solids = null;
            if (isRegular)
                solids = SolidEntity.SliceWithSurface(surface.SurfaceEntity);
            else
                solids = new[] { SolidEntity.NonRegularSliceWithSurface(surface.SurfaceEntity) };
            if (solids == null || solids.Length == 0)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "Solid.Slice"));
            return solids.ToArray<Solid, IGeometryEntity>(true);
        }

        */
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uniformFaceThickness"></param>
        /// <returns></returns>
        public Solid[] ThinShell(double uniformFaceThickness)
        {
            if (uniformFaceThickness.EqualsTo(0.0))
                throw new ArgumentException(string.Format(Properties.Resources.IsZero, "uniformFaceThickness"), "uniformFaceThickness");
            if (uniformFaceThickness < 0)
                throw new ArgumentException(string.Format(Properties.Resources.LessThanZero, "uniformFaceThickness"), "uniformFaceThickness");

            var hosts = SolidEntity.ThinShell(uniformFaceThickness, uniformFaceThickness);
            if (null == hosts)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "Solid.ThinShell"));

            return hosts.Select(x => x.ToSolid(true, this)).ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="selectPoint"></param>
        /// <returns></returns>
        public Solid Trim(Plane plane, Point selectPoint)
        {
            Plane[] planes = { plane };
            return this.Trim(planes, selectPoint);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="planes"></param>
        /// <param name="selectPoint"></param>
        /// <returns></returns>
        public Solid Trim(Plane[] planes, Point selectPoint)
        {
            return SolidEntity.Trim(planes.Select(x => x.PlaneEntity).ToArray(), selectPoint.PointEntity)
                 .Cast<ISolidEntity>().Select(x => new Solid(x)).FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="surface"></param>
        /// <param name="selectPoint"></param>
        /// <returns></returns>
        public Solid Trim(Surface surface, Point selectPoint)
        {
            Surface[] surfaces = { surface };
            return this.Trim(surfaces, selectPoint);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="surfaces"></param>
        /// <param name="selectPoint"></param>
        /// <returns></returns>
        public Solid Trim(Surface[] surfaces, Point selectPoint)
        {
            return SolidEntity.Trim(surfaces.Select(x => x.SurfaceEntity).ToArray(), selectPoint.PointEntity)
                .Cast<ISolidEntity>().Select(x => new Solid(x)).FirstOrDefault();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="solid"></param>
        /// <param name="selectPoint"></param>
        /// <returns></returns>
        public Solid Trim(Solid solid, Point selectPoint)
        {
            Solid[] solids = { solid };
            return this.Trim(solids, selectPoint);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="solids"></param>
        /// <param name="selectPoint"></param>
        /// <returns></returns>
        public Solid Trim(Solid[] solids, Point selectPoint)
        {
            return SolidEntity.Trim(solids.Select(x => x.GeomEntity).ToArray(), selectPoint.PointEntity)
                .Cast<ISolidEntity>().Select(x => new Solid(x)).FirstOrDefault();
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
        public static Solid[] SelectTrim(Solid[] solids, Solid trimmingSolid, bool keepInside)
        {
            string kMethodName = "Solid.SelectTrim";

            if (null == trimmingSolid)
                throw new System.ArgumentNullException("trimmingSolid");

            ISolidEntity[] solidhosts = solids.ConvertAll(GeometryExtension.ToEntity<Solid, ISolidEntity>);
            if (solidhosts == null || solidhosts.Length == 0)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "solids", kMethodName), "solids");

            ISolidEntity trimmingEntity = trimmingSolid.SolidEntity;
            Solid[] result = SelectTrimCore(solidhosts, trimmingEntity, keepInside);
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
        public static Solid[] SelectTrim(Solid[] solids, Solid[] trimmingSolids, bool keepInside)
        {
            string kMethodName = "Solid.SelectTrim";
            ISolidEntity[] solidhosts = solids.ConvertAll(GeometryExtension.ToEntity<Solid, ISolidEntity>);
            if (solidhosts == null || solidhosts.Length == 0)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "solids", kMethodName), "solids");

            ISolidEntity[] trimminghosts = trimmingSolids.ConvertAll(GeometryExtension.ToEntity<Solid, ISolidEntity>);
            if (null == trimminghosts || trimminghosts.Length == 0)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "trimmingSolids", kMethodName), "trimmingSolids");

            Solid[] result = null;
            if (trimminghosts.Length == 1)
            {
                result = SelectTrimCore(solidhosts, trimminghosts[0], keepInside);
            }
            else
            {
                Solid unionSolid = Solid.UnionCore(trimminghosts[0], trimminghosts, false);
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
        new public static Solid[] ImportFromSAT(string filePath)
        {
            IGeometryEntity[] objects = ImportFromSAT(ref filePath);
            return  objects.ToArray<Solid, IGeometryEntity>();
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
                return SolidEntity.Area;
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
                return SolidEntity.Volume;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Category("Primary")]
        public Point Centroid
        {
            get
            {
                return SolidEntity.GetCentroid().ToPoint(false, this);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Curve[] CrossSections
        {
            get { return mCrossSections; }
            set { value.AssignTo(ref mCrossSections); }
        }

        /// <summary>
        /// 
        /// </summary>
        public Curve Path
        {
            get { return mPath; }
            set { value.AssignTo(ref mPath); }
        }

        /// <summary>
        /// 
        /// </summary>
        public Curve[] Guides
        {
            get { return mGuides; }
            set { value.AssignTo(ref mGuides); }
        }

        /// <summary>
        /// 
        /// </summary>
        public Curve Profile
        {
            get { return mProfile; }
            set { value.AssignTo(ref mProfile); }
        }

        /// <summary>
        /// 
        /// </summary>
        public Point AxisOrigin
        {
            get { return mAxisOrigin; }
            set { value.AssignTo(ref mAxisOrigin); }
        }

        /// <summary>
        /// 
        /// </summary>
        public Line Axis
        {
            get { return mAxis; }
            set { value.AssignTo(ref mAxis); }
        }

        /// <summary>
        /// 
        /// </summary>
        public Vector AxisDirection { get; private set; }

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
        public Vertex[] Vertices
        {
            get
            {
                if (null == mVertices)
                {
                    IVertexEntity[] vertices = SolidEntity.GetVertices();
                    mVertices = vertices.ConvertAll((IVertexEntity host) => new Vertex(host));
                }

                return mVertices;
            }
        }

        /// <summary>
        /// Accesses the Solid Edges 
        /// </summary>
        public Edge[] Edges
        {
            get
            {
                if (null == mEdges)
                {
                    IEdgeEntity[] edges = SolidEntity.GetEdges();
                    mEdges = edges.ConvertAll((IEdgeEntity host) => new Edge(host));
                }

                return mEdges;
            }
        }

        /// <summary>
        /// Accesses the Solid Faces
        /// </summary>
        public Face[] Faces
        {
            get
            {
                if (null == mFaces)
                {
                    IFaceEntity[] faces = SolidEntity.GetFaces();
                    mFaces = faces.ConvertAll((IFaceEntity host) => new Face(host));
                }

                return mFaces;
            }
        }

        public Shell[] Shells
        {
            get
            {
                if (null == mShells)
                    mShells = SolidEntity.GetShells().ConvertAll((IShellEntity host) => new Shell(host));

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

        internal override IGeometryEntity[] IntersectWithCurve(Curve curve)
        {
            return curve.CurveEntity.Intersect(SolidEntity);
        }

        internal override IGeometryEntity[] IntersectWithPlane(Plane plane)
        {
            return SolidEntity.Intersect(plane.PlaneEntity);
        }

        internal override IGeometryEntity[] IntersectWithSolid(Solid solid)
        {
            return SolidEntity.Intersect(solid.SolidEntity);
        }

        internal override IGeometryEntity[] IntersectWithSurface(Surface surf)
        {
            return SolidEntity.Intersect(surf.SurfaceEntity);
        }

        private static Solid[] SelectTrimCore(ISolidEntity[] solidhosts, ISolidEntity trimmingEntity, bool keepInside)
        {
            List<IGeometryEntity> geometries = new List<IGeometryEntity>();
            foreach (var solid in solidhosts)
            {
                IGeometryEntity[] geometryHosts;
                if (keepInside)
                    geometryHosts = trimmingEntity.Intersect(solid);
                else
                    geometryHosts = new IGeometryEntity[]
                    {
                        solid.CSGDifference(trimmingEntity)
                    };

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
        internal static Solid UnionCore(ISolidEntity thisEntity, ISolidEntity[] othersolidhosts, bool persist)
        {
            var host = thisEntity;
            foreach (var solidhost in othersolidhosts)
            {
                if (host == solidhost)
                    continue;

                var union = host.CSGUnion(solidhost);

                if (union == null)
                    throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "Solid.Union"));
            }

            return host.ToSolid(persist, null);
        }
        #endregion
    }
}
