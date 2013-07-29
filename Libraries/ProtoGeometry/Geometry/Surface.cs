using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

namespace Autodesk.DesignScript.Geometry
{
    public abstract class Surface : Geometry
    {
        #region PRIVATE CONSTRUCTOR
        internal ISurfaceEntity SurfaceEntity { get { return GetSurfaceEntity(); } }

        internal abstract ISurfaceEntity GetSurfaceEntity();

        internal Surface(IGeometryEntity host, bool persist = false)
            : base(host, persist)
        {
        }

        protected virtual bool GetIsClosed()
        {
            return SurfaceEntity.GetIsClosed();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // For BSplineSurface, the Patches property points to itself
                // hence it should not be disposing itself by disposing its property
                if (mPatches != null)
                {
                    foreach (var item in mPatches)
                    {
                        if (object.ReferenceEquals(this, item))
                            continue;
                        item.DisposeObject();
                    }
                }
            }
            mPatches = null;
            base.Dispose(disposing);
        }

        private BSplineSurface[] mPatches = null;
        #endregion

        #region SPECIALISED SURFACE CONSTRUCTORS "STATIC METHODS"

        /// <summary>
        /// Constructs a LoftedSurface by lofting an array of curve cross sections.
        /// </summary>
        /// <param name="crossSections">crossSections (curves) needs to be all closed or all open.</param>
        /// <returns>Loft surface.</returns>
        public static LoftedSurface LoftFromCrossSections(Curve[] crossSections)
        {
            return LoftedSurface.FromCrossSections(crossSections);
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
        public static LoftedSurface LoftFromCrossSectionsGuides(Curve[] crossSections, Curve[] guides)
        {
            return LoftedSurface.FromCrossSectionsGuides(crossSections, guides);
        }

        /// <summary>
        /// Construct a LoftedSurface by lofting an array of curve cross sections and 
        /// using a curve as lofting path to control the lofting direction.
        /// </summary>
        /// <param name="crossSections">crossSections (curves) needs to be all closed or all open.</param>
        /// <param name="path">lofting path curve.</param>
        /// <returns>Loftedsurface</returns>
        public static LoftedSurface LoftFromCrossSectionsPath(Curve[] crossSections, Curve path)
        {
            return LoftedSurface.FromCrossSectionsPath(crossSections, path);
        }

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
        public static RevolvedSurface Revolve(Curve profile, Point axisOrigin, Vector axisDirection, double startAngle, double sweepAngle)
        {
            return RevolvedSurface.ByProfileAxisOriginDirectionAngle(profile, axisOrigin, axisDirection, startAngle, sweepAngle);
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
        public static RevolvedSurface Revolve(Curve profile, Point axisOrigin, Vector axisDirection)
        {
            return RevolvedSurface.ByProfileAxisOriginDirection(profile, axisOrigin, axisDirection);
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
        public static RevolvedSurface Revolve(Curve profile, Line axis, double startAngle, double sweepAngle)
        {
            return RevolvedSurface.ByProfileAxisAngle(profile, axis, startAngle, sweepAngle);
        }

        /// <summary>
        /// Construct a Surface by revolving curve about a line axis. Assuming 
        /// sweep angle = 360 and start angle = 0.
        /// </summary>
        /// <param name="profile">Profile Curve for revolve surface.</param>
        /// <param name="axis">Line to define axis of revolution.</param>
        /// <returns>SRevolvedSurface</returns>
        public static RevolvedSurface Revolve(Curve profile, Line axis)
        {
            return RevolvedSurface.ByProfileAxis(profile, axis);
        }

        /// <summary>
        /// Construct a Surface by sweeping a profile curve along a path curve.
        /// </summary>
        /// <param name="profile">Profile curve for sweep.</param>
        /// <param name="path">Path curve to sweep along.</param>
        /// <returns>SweptSurface</returns>
        public static SweptSurface Sweep(Curve profile, Curve path)
        {
            return SweptSurface.ByProfilePath(profile, path);
        }

        /// <summary>
        /// Constructs a patch surface from the give closed non-self intersecting 
        /// profile curve.
        /// </summary>
        /// <param name="profile">Profile curve for patch surface</param>
        /// <returns>Patch Surface</returns>
        public static PatchSurface CreateFromCurve(Curve profile)
        {
            return PatchSurface.FromCurve(profile);
        }

        #endregion

        #region PROPERTIES

        public Surface ContextSurface
        {
            get { return Context as Surface; }
        }

        [Category("Primary")]
        public double Area
        {
            get { return SurfaceEntity.GetArea(); }
        }

        [Category("Primary")]
        public double Perimeter
        {
            get { return SurfaceEntity.GetPerimeter(); }
        }

        [Category("Primary")]
        public bool IsClosed
        {
            get { return GetIsClosed(); }
        }

        [Category("Primary")]
        public bool IsClosedInU 
        {
            get { return SurfaceEntity.GetIsClosedInU(); }
        }

        [Category("Primary")]
        public bool IsClosedInV 
        {
            get { return SurfaceEntity.GetIsClosedInV(); }
        }

        public BSplineSurface[] Patches
        {
            get
            {
                if (null == mPatches)
                    mPatches = ConvertToBSplineSurface(false);
                return mPatches;
            }
        }

        #endregion

        #region "MODIFIER METHODS"

        /// <summary>
        /// Offsets the surface by the given thickness to create a Solid. 
        /// 'bothSides' flag can be used to choose if the surface is required 
        /// to be offset on both sides.
        /// </summary>
        /// <param name="thickness">Thickness value in one side of the surface.</param>
        /// <param name="bothSides">Whether to offset surface in both sides.</param>
        /// <returns>Solid.</returns>
        public Solid Thicken(double thickness, bool bothSides)
        {
            ISolidEntity entity = SurfaceEntity.Thicken(thickness, bothSides);
            if (null == entity)
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, "Solid.Thicken"));

            return entity.ToSolid(true, this);
        }

        /// <summary>
        /// Returns a normal to the surface at the specified point
        /// </summary>
        /// <param name="pointOnSurface">Point on the surface at which the normal is required</param>
        /// <returns></returns>
        public Vector NormalAtPoint(Point pointOnSurface)
        {
            if (pointOnSurface == null)
                throw new System.ArgumentNullException("pointOnSurface");
            IVector normal = SurfaceEntity.GetNormalAtPoint(pointOnSurface.PointEntity);
            if(normal == null)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "Surface.NormalAtPoint"));
            return new Vector(normal);
        }

        /// <summary>
        /// Returns a parallel Surface with specific distance from original surface. 
        /// </summary>
        /// <param name="distance">Offset distance value.</param>
        /// <returns>Surface.</returns>
        public Surface Offset(double distance)
        {
            if (distance.EqualsTo(0.0))
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZero, "offset distance"), "distance");

            ISurfaceEntity surfEntity = SurfaceEntity.Offset(distance);
            if (null == surfEntity)
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, "Surface.Offset"));

            Surface surf =  surfEntity.ToSurf(true, this);
            surf.Context = this;
            return surf;
        }

        /// <summary>
        /// Split this surface using a plane as cutting tool.
        /// </summary>
        /// <param name="splittingPlane">Plane as cutting tool.</param>
        /// <returns>Array of surfaces.</returns>
        public Surface[] Split(Plane splittingPlane)
        {
            ISurfaceEntity[] splitSurfaces = SurfaceEntity.Split(splittingPlane.HostImpl as IPlaneEntity);

            if (null == splitSurfaces || splitSurfaces.Length < 1)
            {
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, "Surface.Split"));
            }

            return splitSurfaces.ConvertAll((ISurfaceEntity host) => host.ToSurf(true, this));
        }

        /// <summary>
        /// Split this surface using a surface as cutting tool.
        /// </summary>
        /// <param name="splittingSurface">Surface as cutting tool.</param>
        /// <returns>Array of surfaces.</returns>
        public Surface[] Split(Surface splittingSurface)
        {
            ISurfaceEntity[] splitSurfaces = SurfaceEntity.Split(splittingSurface.SurfaceEntity);

            if (null == splitSurfaces || splitSurfaces.Length < 1)
            {
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, "Surface.Split"));
            }

            return splitSurfaces.ConvertAll((ISurfaceEntity host) => host.ToSurf(true, this));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="curves"></param>
        /// <param name="planes"></param>
        /// <param name="surfaces"></param>
        /// <param name="solids"></param>
        /// <param name="selectPoint"></param>
        /// <param name="autoExtend"></param>
        /// <returns></returns>
        public Surface Trim(Curve[] curves, Plane[] planes, Surface[] surfaces, Solid[] solids, Point selectPoint, bool autoExtend)
        {
            if (null == selectPoint)
                throw new System.ArgumentNullException("selectPoint");

            ICurveEntity[] hostCurves = curves.ConvertAll(GeometryExtension.ToEntity<Curve, ICurveEntity>);
            IPlaneEntity[] hostPlanes = planes.ConvertAll(GeometryExtension.ToEntity<Plane, IPlaneEntity>);
            ISurfaceEntity[] hostSurfaces = surfaces.ConvertAll(GeometryExtension.ToEntity<Surface, ISurfaceEntity>);
            ISolidEntity[] hostSolids = solids.ConvertAll(GeometryExtension.ToEntity<Solid, ISolidEntity>);

            IPointEntity hostPoint = selectPoint.PointEntity;

            if (hostCurves == null && hostPlanes == null && hostSurfaces == null && hostSolids == null)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "Geometry", "Surface.Trim"));

            ISurfaceEntity trimSurface = SurfaceEntity.Trim(hostCurves, hostPlanes, hostSurfaces, hostSolids, hostPoint, autoExtend);

            //For trim operation, if the return value is not null, hide the original tools and surfaces.
            if (null != trimSurface)
            {
                Hide(curves);
                Hide(planes);
                Hide(surfaces);
                Hide(solids);
                SetVisibility(false);
            }

            return trimSurface.ToSurf(true, this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="selectPoint"></param>
        /// <param name="autoExtend"></param>
        /// <returns></returns>
        public Surface Trim(Curve curve, Point selectPoint, bool autoExtend)
        {
            if (null == curve)
                throw new System.ArgumentNullException("curve");

            Curve[] curves = { curve };
            return Trim(curves, null, null, null, selectPoint, autoExtend);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="curves"></param>
        /// <param name="selectPoint"></param>
        /// <param name="autoExtend"></param>
        /// <returns></returns>
        public Surface Trim(Curve[] curves, Point selectPoint, bool autoExtend)
        {
            if (null == curves)
                throw new System.ArgumentNullException("curves");
            return Trim(curves, null, null, null, selectPoint, autoExtend);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="selectPoint"></param>
        /// <param name="autoExtend"></param>
        /// <returns></returns>
        public Surface Trim(Plane plane, Point selectPoint, bool autoExtend)
        {
            if(null == plane)
                throw new System.ArgumentNullException("plane");

            Plane[] planes = {plane};
            return Trim(null, planes, null, null, selectPoint, autoExtend);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="planes"></param>
        /// <param name="selectPoint"></param>
        /// <param name="autoExtend"></param>
        /// <returns></returns>
        public Surface Trim(Plane[] planes, Point selectPoint, bool autoExtend)
        {
            if(null == planes)
                throw new System.ArgumentNullException("planes");

            return Trim(null, planes, null, null, selectPoint, autoExtend);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="surface"></param>
        /// <param name="selectPoint"></param>
        /// <param name="autoExtend"></param>
        /// <returns></returns>
        public Surface Trim(Surface surface, Point selectPoint, bool autoExtend)
        {
            if(null == surface)
                throw new System.ArgumentNullException("surface");

            Surface[] surfaces = {surface};
            return Trim(null, null, surfaces, null, selectPoint, autoExtend);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="surfaces"></param>
        /// <param name="selectPoint"></param>
        /// <param name="autoExtend"></param>
        /// <returns></returns>
        public Surface Trim(Surface[] surfaces, Point selectPoint, bool autoExtend)
        {
            if(null == surfaces)
                throw new System.ArgumentNullException("surfaces");

            return Trim(null, null, surfaces, null, selectPoint, autoExtend);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="solid"></param>
        /// <param name="selectPoint"></param>
        /// <param name="autoExtend"></param>
        /// <returns></returns>
        public Surface Trim(Solid solid, Point selectPoint, bool autoExtend)
        {
            if(null == solid)
                throw new System.ArgumentNullException("solid");

            Solid[] solids = { solid };
            return Trim(null, null, null, solids, selectPoint, autoExtend);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="solids"></param>
        /// <param name="selectPoint"></param>
        /// <param name="autoExtend"></param>
        /// <returns></returns>
        public Surface Trim(Solid[] solids, Point selectPoint, bool autoExtend)
        {
            if(null == solids)
                throw new System.ArgumentNullException("solids");

            return Trim(null, null, null, solids, selectPoint, autoExtend);
        }

        /// <summary>
        /// Returns an array of surfaces by trimming an array of surfaces with 
        /// a trimming solid. It will either return part of surfaces inside the
        /// trimSolid or outside the trimSolid based on flag keepInside.
        /// </summary>
        /// <param name="surfaces">The input array of surfaces</param>
        /// <param name="trimmingSolid">The trimming solid</param>
        /// <param name="keepInside">
        /// Depending on the keepInside input, the input surfaces or those 
        /// parts of the input surfaces inside the trimSolid or outside the
        /// trimSolid will be returned</param>
        /// <returns>Returns an array of trimmed Surfaces</returns>
        [AllowRankReduction]
        public static Surface[] SelectTrim(Surface[] surfaces, Solid trimmingSolid, bool keepInside)
        {
            string kMethodName = "Surface.SelectTrim";

            if (null == trimmingSolid)
                throw new System.ArgumentNullException("trimmingSolid");

            ISurfaceEntity[] surfacehosts = surfaces.ConvertAll(GeometryExtension.ToEntity<Surface, ISurfaceEntity>);
            if (surfacehosts == null || surfacehosts.Length == 0)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "surfaces", kMethodName), "surfaces");

            ISolidEntity trimmingEntity = trimmingSolid.SolidEntity;
            Surface[] result = SelectTrimCore(surfacehosts, trimmingEntity, keepInside);
            if (null != result)
            { 
                Hide(surfaces);
                Hide(trimmingSolid);
            }

            return result;
        }

        /// <summary>
        /// Returns an array of surfaces by trimming an array of surfaces with 
        /// an array of trimming solids. It will either return part of surfaces
        /// inside the trimSolids or outside the trimSolids based on flag 
        /// keepInside. 
        /// </summary>
        /// <param name="surfaces">The input array of surfaces</param>
        /// <param name="trimmingSolids">The array of trimming solids</param>
        /// <param name="keepInside">
        /// Depending on the keepInside input, the input surfaces or those 
        /// parts of the input surfaces inside the trimSolids or outside the
        /// trimSolids will be returned</param>
        /// <returns>Returns an array of trimmed Surfaces</returns>
        [AllowRankReduction]
        public static Surface[] SelectTrim(Surface[] surfaces, Solid[] trimmingSolids, bool keepInside)
        {
            string kMethodName = "Surface.SelectTrim";
            ISurfaceEntity[] surfacehosts = surfaces.ConvertAll(GeometryExtension.ToEntity<Surface, ISurfaceEntity>);
            if (surfacehosts == null || surfacehosts.Length == 0)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "surfaces", kMethodName), "surfaces");

            ISolidEntity[] solidhosts = trimmingSolids.ConvertAll(GeometryExtension.ToEntity<Solid, ISolidEntity>);
            if (null == solidhosts || solidhosts.Length == 0)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "trimmingSolids", kMethodName), "trimmingSolids");

            Surface[] result = null;
            if (solidhosts.Length == 1)
            {
                result = SelectTrimCore(surfacehosts, solidhosts[0], keepInside);
            }
            else
            {
                Solid unionSolid = Solid.UnionCore(solidhosts[0], solidhosts, false);
                if (null == unionSolid)
                    throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));

                result = SelectTrimCore(surfacehosts, unionSolid.SolidEntity, keepInside);
            }

            if (null != result)
            {
                Hide(surfaces);
                Hide(trimmingSolids);
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="toolSolid"></param>
        /// <returns></returns>
        public Surface[] Difference(Solid toolSolid)
        {
            string kMethod = "Surface.Difference";
            if (null == toolSolid)
                throw new System.ArgumentNullException("toolSolid");

            IGeometryEntity[] geoms = SurfaceEntity.SubtractFrom(toolSolid.SolidEntity);
            if (null == geoms)
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethod));

            return geoms.ConvertAll(
                (IGeometryEntity g) =>
                {
                    ISurfaceEntity s = g as ISurfaceEntity;
                    if (null == s)
                        return null;
                    return s.ToSurf(true, this);
                });
        }

        /// <summary>
        /// Constructs a CoordinateSystem at the given u, v parameters on 
        /// the given surface. The x-axis of the CoordinateSystem is aligned 
        /// with the u-directional tangent, the y-axis with the v-directional 
        /// tangent and the z-axis is mutually perpendicular to the x and y axes.
        /// </summary>
        /// <param name="u">U parameter value in the range of [0.0, 1.0]. </param>
        /// <param name="v">V parameter value in the range of [0.0, 1.0]. </param>
        /// <returns>CoordinateSystem</returns>
        public CoordinateSystem CoordinateSystemAtParameter(double u, double v)
        {
            return CoordinateSystem.AtParameter(this, u, v);
        }

        /// <summary>
        /// Returns a point with offset distance from a point at U.V parameter of the surface
        /// </summary>
        /// <param name="u">U parameter value in the range of [0.0, 1.0]. </param>
        /// <param name="v">V parameter value in the range of [0.0, 1.0]. </param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public Point PointAtParameter(double u, double v, double offset)
        {
          return Point.AtParameter(this, u, v, offset);
        }

        /// <summary>
        /// Returns a point on a surface at given U, V parameter.
        /// </summary>
        /// <param name="u">U parameter value.</param>
        /// <param name="v">V parameter value.</param>
        /// <returns>Point</returns>
        public Point PointAtParameter(double u, double v)
        {
            return Point.AtParameter(this, u, v);
        }

        /// <summary>
        /// Returns the U, V parameter at a given point of a surface
        /// </summary>
        /// <param name="point"></param>
        /// <returns>double[] == the first element is U, the second double is V</returns>
        public double[] ParameterAtPoint(Point point)
        {
            System.Tuple<double, double> parametersTuple = SurfaceEntity.GetUVParameterAtPoint(point.PointEntity);
            List<double> parameters = new List<double>();
            parameters.Add(parametersTuple.Item1);
            parameters.Add(parametersTuple.Item2);
            return parameters.ToArray();
        }

        /// <summary>
        /// Returns the surface curvature at given U, V parameter
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public SurfaceCurvature CurvatureAtParameter(double u, double v)
        {
            return SurfaceCurvature.BySurfaceParameters(this, u, v);
        }

        /// <summary>
        /// Returns single or array of BSplineSurfaces from Surface.
        /// </summary>
        /// <returns>Array of BSplineSurface.</returns>
        [AllowRankReduction]
        public BSplineSurface[] ConvertToBSplineSurface()
        {
            return ConvertToBSplineSurface(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="persist"></param>
        /// <returns></returns>
        protected BSplineSurface[] ConvertToBSplineSurface(bool persist)
        {
            if (SurfaceEntity is IBSplineSurfaceEntity)
            {
                return new BSplineSurface[] { this as BSplineSurface };                
            }

            IBSplineSurfaceEntity[] entities = SurfaceEntity.ConvertToBSplineSurface();
            return entities.ConvertAll((IBSplineSurfaceEntity h) => h.ToBSurf(persist, this));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private BSplineSurface ApproxBSplineSurface()
        {
            IBSplineSurfaceEntity surface = SurfaceEntity.ApproxBSpline(-1.0);
            return new BSplineSurface(surface, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tol"></param>
        /// <returns></returns>
        private BSplineSurface ApproxBSplineSurface(double tol)
        {
            IBSplineSurfaceEntity surface = SurfaceEntity.ApproxBSpline(tol);
            return new BSplineSurface(surface, true);
        }

        #endregion

        #region INTERNAL_METHODS
        /// <summary>
        /// 
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        internal Point PointAtParametersCore(ref double u, ref double v, double offset)
        {
            bool uchange = GeometryExtension.ClipParamRange(ref u);
            bool vchange = GeometryExtension.ClipParamRange(ref v);
            // TO DO - throw a warning each time a condition above is satisfied.    
            //throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "u or v parameter", "Surface.PointAtParameters"));

            IPointEntity pt = SurfaceEntity.PointAtParameter(u, v);
            if (!offset.EqualsTo(0.0))
            {
                Vector normal = new Vector(SurfaceEntity.NormalAtPoint(pt));
                Vector translation = normal.Normalize().Scale(offset);
                IPointEntity offsetPt = pt.Add(translation);
                pt.Dispose();
                pt = offsetPt;
            }
            return pt.ToPoint(true, this);
        }

        internal override IGeometryEntity[] IntersectWithCurve(Curve curve)
        {
            return curve.CurveEntity.IntersectWith(SurfaceEntity);
        }

        internal override IGeometryEntity[] IntersectWithPlane(Plane plane)
        {
            return SurfaceEntity.IntersectWith(plane.PlaneEntity);
        }

        internal override IGeometryEntity[] IntersectWithSolid(Solid solid)
        {
            return solid.SolidEntity.IntersectWith(SurfaceEntity);
        }

        internal override IGeometryEntity[] IntersectWithSurface(Surface surf)
        {
            return SurfaceEntity.IntersectWith(surf.SurfaceEntity);
        }

        internal override IPointEntity ClosestPointTo(IPointEntity otherPoint)
        {
            return SurfaceEntity.GetClosestPoint(otherPoint);
        }

        internal override IGeometryEntity[] ProjectOn(Geometry other, Vector direction)
        {
            //Solid solid = other as Solid;
            //if (null != solid)
            //    return solid.SolidEntity.Project(SurfaceEntity, direction);

            return base.ProjectOn(other, direction);
        }

        internal CoordinateSystem GetCSAtParameters(double u, double v)
        {
            bool uchange = GeometryExtension.ClipParamRange(ref u);
            bool vchange = GeometryExtension.ClipParamRange(ref v);
            // TO DO - throw a warning each time a condition above is satisfied.    
            //throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "u or v parameter", "Surface.PointAtUVParameters"));

            IPointEntity pos = SurfaceEntity.PointAtParameter(u, v);
            Point origin = pos.ToPoint(false, null);
            Vector xAxis = new Vector(SurfaceEntity.TangentAtUParameter(u, v));
            Vector yAxis = new Vector(SurfaceEntity.TangentAtVParameter(u, v));
            Vector zAxis = xAxis.Cross(yAxis);

            return CoordinateSystem.ByOriginVectors(origin, xAxis, yAxis, zAxis, false, true, false);
        }

        private static Surface[] SelectTrimCore(ISurfaceEntity[] surfacehosts, ISolidEntity trimmingEntity, bool keepInside)
        {
            List<IGeometryEntity> geometries = new List<IGeometryEntity>();
            foreach (var surf in surfacehosts)
            {
                if (keepInside)
                    geometries.AddRange(trimmingEntity.IntersectWith(surf));
                else
                {
                    IGeometryEntity[] trimmedSurfaces = surf.SubtractFrom(trimmingEntity);
                    if(trimmedSurfaces!=null)
                        geometries.AddRange(trimmedSurfaces);
                }
            }

            IGeometryEntity[] geoms = geometries.ToArray();
            return geoms.ConvertAll(
                (IGeometryEntity g) =>
                {
                    ISurfaceEntity s = g as ISurfaceEntity;
                    if (null == s)
                        return null;
                    return s.ToSurf(true, null);
                });
        }
        #endregion
    }

    class GenericSurface : Surface
    {
        #region PRIVATE CONSTRUCTOR
        static void InitType()
        {
            RegisterHostType(typeof(ISurfaceEntity), (IGeometryEntity host, bool persist) => { return new GenericSurface(host as ISurfaceEntity, persist); });
        }

        protected GenericSurface(ISurfaceEntity host, bool persist = false)
            : base(host, persist)
        {
        }

        internal override ISurfaceEntity GetSurfaceEntity()
        {
            return HostImpl as ISurfaceEntity;
        }

        public override string ToString()
        {
            return string.Format("Surface()");
        }
        #endregion
    }
}
