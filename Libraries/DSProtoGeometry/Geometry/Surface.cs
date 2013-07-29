using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

namespace Autodesk.DesignScript.Geometry
{
    public abstract class DSSurface : DSGeometry
    {
        #region PRIVATE CONSTRUCTOR
        internal ISurfaceEntity SurfaceEntity { get { return GetSurfaceEntity(); } }

        internal abstract ISurfaceEntity GetSurfaceEntity();

        internal DSSurface(IGeometryEntity host, bool persist = false)
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

        private DSBSplineSurface[] mPatches = null;
        #endregion

        #region SPECIALISED SURFACE CONSTRUCTORS "STATIC METHODS"

        /// <summary>
        /// Constructs a LoftedSurface by lofting an array of curve cross sections.
        /// </summary>
        /// <param name="crossSections">crossSections (curves) needs to be all closed or all open.</param>
        /// <returns>Loft surface.</returns>
        public static DSLoftedSurface LoftFromCrossSections(DSCurve[] crossSections)
        {
            return DSLoftedSurface.FromCrossSections(crossSections);
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
        public static DSLoftedSurface LoftFromCrossSectionsGuides(DSCurve[] crossSections, DSCurve[] guides)
        {
            return DSLoftedSurface.FromCrossSectionsGuides(crossSections, guides);
        }

        /// <summary>
        /// Construct a LoftedSurface by lofting an array of curve cross sections and 
        /// using a curve as lofting path to control the lofting direction.
        /// </summary>
        /// <param name="crossSections">crossSections (curves) needs to be all closed or all open.</param>
        /// <param name="path">lofting path curve.</param>
        /// <returns>Loftedsurface</returns>
        public static DSLoftedSurface LoftFromCrossSectionsPath(DSCurve[] crossSections, DSCurve path)
        {
            return DSLoftedSurface.FromCrossSectionsPath(crossSections, path);
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
        public static DSRevolvedSurface Revolve(DSCurve profile, DSPoint axisOrigin, DSVector axisDirection, double startAngle, double sweepAngle)
        {
            return DSRevolvedSurface.ByProfileAxisOriginDirectionAngle(profile, axisOrigin, axisDirection, startAngle, sweepAngle);
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
        public static DSRevolvedSurface Revolve(DSCurve profile, DSPoint axisOrigin, DSVector axisDirection)
        {
            return DSRevolvedSurface.ByProfileAxisOriginDirection(profile, axisOrigin, axisDirection);
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
        public static DSRevolvedSurface Revolve(DSCurve profile, DSLine axis, double startAngle, double sweepAngle)
        {
            return DSRevolvedSurface.ByProfileAxisAngle(profile, axis, startAngle, sweepAngle);
        }

        /// <summary>
        /// Construct a Surface by revolving curve about a line axis. Assuming 
        /// sweep angle = 360 and start angle = 0.
        /// </summary>
        /// <param name="profile">Profile Curve for revolve surface.</param>
        /// <param name="axis">Line to define axis of revolution.</param>
        /// <returns>SRevolvedSurface</returns>
        public static DSRevolvedSurface Revolve(DSCurve profile, DSLine axis)
        {
            return DSRevolvedSurface.ByProfileAxis(profile, axis);
        }

        /// <summary>
        /// Construct a Surface by sweeping a profile curve along a path curve.
        /// </summary>
        /// <param name="profile">Profile curve for sweep.</param>
        /// <param name="path">Path curve to sweep along.</param>
        /// <returns>SweptSurface</returns>
        public static DSSweptSurface Sweep(DSCurve profile, DSCurve path)
        {
            return DSSweptSurface.ByProfilePath(profile, path);
        }

        /// <summary>
        /// Constructs a patch surface from the give closed non-self intersecting 
        /// profile curve.
        /// </summary>
        /// <param name="profile">Profile curve for patch surface</param>
        /// <returns>Patch Surface</returns>
        public static DSPatchSurface CreateFromCurve(DSCurve profile)
        {
            return DSPatchSurface.FromCurve(profile);
        }

        #endregion

        #region PROPERTIES

        public DSSurface ContextSurface
        {
            get { return Context as DSSurface; }
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

        public DSBSplineSurface[] Patches
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
        public DSSolid Thicken(double thickness, bool bothSides)
        {
            ISolidEntity entity = SurfaceEntity.Thicken(thickness, bothSides);
            if (null == entity)
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, "DSSolid.Thicken"));

            return entity.ToSolid(true, this);
        }

        /// <summary>
        /// Returns a normal to the surface at the specified point
        /// </summary>
        /// <param name="pointOnSurface">Point on the surface at which the normal is required</param>
        /// <returns></returns>
        public DSVector NormalAtPoint(DSPoint pointOnSurface)
        {
            if (pointOnSurface == null)
                throw new System.ArgumentNullException("pointOnSurface");
            IVector normal = SurfaceEntity.GetNormalAtPoint(pointOnSurface.PointEntity);
            if(normal == null)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "DSSurface.NormalAtPoint"));
            return new DSVector(normal);
        }

        /// <summary>
        /// Returns a parallel Surface with specific distance from original surface. 
        /// </summary>
        /// <param name="distance">Offset distance value.</param>
        /// <returns>Surface.</returns>
        public DSSurface Offset(double distance)
        {
            if (distance.EqualsTo(0.0))
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZero, "offset distance"), "distance");

            ISurfaceEntity surfEntity = SurfaceEntity.Offset(distance);
            if (null == surfEntity)
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, "DSSurface.Offset"));

            DSSurface surf =  surfEntity.ToSurf(true, this);
            surf.Context = this;
            return surf;
        }

        /// <summary>
        /// Split this surface using a plane as cutting tool.
        /// </summary>
        /// <param name="splittingPlane">Plane as cutting tool.</param>
        /// <returns>Array of surfaces.</returns>
        public DSSurface[] Split(DSPlane splittingPlane)
        {
            ISurfaceEntity[] splitSurfaces = SurfaceEntity.Split(splittingPlane.HostImpl as IPlaneEntity);

            if (null == splitSurfaces || splitSurfaces.Length < 1)
            {
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, "DSSurface.Split"));
            }

            return splitSurfaces.ConvertAll((ISurfaceEntity host) => host.ToSurf(true, this));
        }

        /// <summary>
        /// Split this surface using a surface as cutting tool.
        /// </summary>
        /// <param name="splittingSurface">Surface as cutting tool.</param>
        /// <returns>Array of surfaces.</returns>
        public DSSurface[] Split(DSSurface splittingSurface)
        {
            ISurfaceEntity[] splitSurfaces = SurfaceEntity.Split(splittingSurface.SurfaceEntity);

            if (null == splitSurfaces || splitSurfaces.Length < 1)
            {
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, "DSSurface.Split"));
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
        public DSSurface Trim(DSCurve[] curves, DSPlane[] planes, DSSurface[] surfaces, DSSolid[] solids, DSPoint selectPoint, bool autoExtend)
        {
            if (null == selectPoint)
                throw new System.ArgumentNullException("selectPoint");

            ICurveEntity[] hostCurves = curves.ConvertAll(DSGeometryExtension.ToEntity<DSCurve, ICurveEntity>);
            IPlaneEntity[] hostPlanes = planes.ConvertAll(DSGeometryExtension.ToEntity<DSPlane, IPlaneEntity>);
            ISurfaceEntity[] hostSurfaces = surfaces.ConvertAll(DSGeometryExtension.ToEntity<DSSurface, ISurfaceEntity>);
            ISolidEntity[] hostSolids = solids.ConvertAll(DSGeometryExtension.ToEntity<DSSolid, ISolidEntity>);

            IPointEntity hostPoint = selectPoint.PointEntity;

            if (hostCurves == null && hostPlanes == null && hostSurfaces == null && hostSolids == null)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "DSGeometry", "DSSurface.Trim"));

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
        public DSSurface Trim(DSCurve curve, DSPoint selectPoint, bool autoExtend)
        {
            if (null == curve)
                throw new System.ArgumentNullException("curve");

            DSCurve[] curves = { curve };
            return Trim(curves, null, null, null, selectPoint, autoExtend);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="curves"></param>
        /// <param name="selectPoint"></param>
        /// <param name="autoExtend"></param>
        /// <returns></returns>
        public DSSurface Trim(DSCurve[] curves, DSPoint selectPoint, bool autoExtend)
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
        public DSSurface Trim(DSPlane plane, DSPoint selectPoint, bool autoExtend)
        {
            if(null == plane)
                throw new System.ArgumentNullException("plane");

            DSPlane[] planes = {plane};
            return Trim(null, planes, null, null, selectPoint, autoExtend);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="planes"></param>
        /// <param name="selectPoint"></param>
        /// <param name="autoExtend"></param>
        /// <returns></returns>
        public DSSurface Trim(DSPlane[] planes, DSPoint selectPoint, bool autoExtend)
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
        public DSSurface Trim(DSSurface surface, DSPoint selectPoint, bool autoExtend)
        {
            if(null == surface)
                throw new System.ArgumentNullException("surface");

            DSSurface[] surfaces = {surface};
            return Trim(null, null, surfaces, null, selectPoint, autoExtend);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="surfaces"></param>
        /// <param name="selectPoint"></param>
        /// <param name="autoExtend"></param>
        /// <returns></returns>
        public DSSurface Trim(DSSurface[] surfaces, DSPoint selectPoint, bool autoExtend)
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
        public DSSurface Trim(DSSolid solid, DSPoint selectPoint, bool autoExtend)
        {
            if(null == solid)
                throw new System.ArgumentNullException("solid");

            DSSolid[] solids = { solid };
            return Trim(null, null, null, solids, selectPoint, autoExtend);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="solids"></param>
        /// <param name="selectPoint"></param>
        /// <param name="autoExtend"></param>
        /// <returns></returns>
        public DSSurface Trim(DSSolid[] solids, DSPoint selectPoint, bool autoExtend)
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
        public static DSSurface[] SelectTrim(DSSurface[] surfaces, DSSolid trimmingSolid, bool keepInside)
        {
            string kMethodName = "DSSurface.SelectTrim";

            if (null == trimmingSolid)
                throw new System.ArgumentNullException("trimmingSolid");

            ISurfaceEntity[] surfacehosts = surfaces.ConvertAll(DSGeometryExtension.ToEntity<DSSurface, ISurfaceEntity>);
            if (surfacehosts == null || surfacehosts.Length == 0)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "surfaces", kMethodName), "surfaces");

            ISolidEntity trimmingEntity = trimmingSolid.SolidEntity;
            DSSurface[] result = SelectTrimCore(surfacehosts, trimmingEntity, keepInside);
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
        public static DSSurface[] SelectTrim(DSSurface[] surfaces, DSSolid[] trimmingSolids, bool keepInside)
        {
            string kMethodName = "DSSurface.SelectTrim";
            ISurfaceEntity[] surfacehosts = surfaces.ConvertAll(DSGeometryExtension.ToEntity<DSSurface, ISurfaceEntity>);
            if (surfacehosts == null || surfacehosts.Length == 0)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "surfaces", kMethodName), "surfaces");

            ISolidEntity[] solidhosts = trimmingSolids.ConvertAll(DSGeometryExtension.ToEntity<DSSolid, ISolidEntity>);
            if (null == solidhosts || solidhosts.Length == 0)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "trimmingSolids", kMethodName), "trimmingSolids");

            DSSurface[] result = null;
            if (solidhosts.Length == 1)
            {
                result = SelectTrimCore(surfacehosts, solidhosts[0], keepInside);
            }
            else
            {
                DSSolid unionSolid = DSSolid.UnionCore(solidhosts[0], solidhosts, false);
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
        public DSSurface[] Difference(DSSolid toolSolid)
        {
            string kMethod = "DSSurface.Difference";
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
        public DSCoordinateSystem CoordinateSystemAtParameter(double u, double v)
        {
            return DSCoordinateSystem.AtParameter(this, u, v);
        }

        /// <summary>
        /// Returns a point with offset distance from a point at U.V parameter of the surface
        /// </summary>
        /// <param name="u">U parameter value in the range of [0.0, 1.0]. </param>
        /// <param name="v">V parameter value in the range of [0.0, 1.0]. </param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public DSPoint PointAtParameter(double u, double v, double offset)
        {
          return DSPoint.AtParameter(this, u, v, offset);
        }

        /// <summary>
        /// Returns a point on a surface at given U, V parameter.
        /// </summary>
        /// <param name="u">U parameter value.</param>
        /// <param name="v">V parameter value.</param>
        /// <returns>Point</returns>
        public DSPoint PointAtParameter(double u, double v)
        {
            return DSPoint.AtParameter(this, u, v);
        }

        /// <summary>
        /// Returns the U, V parameter at a given point of a surface
        /// </summary>
        /// <param name="point"></param>
        /// <returns>double[] == the first element is U, the second double is V</returns>
        public double[] ParameterAtPoint(DSPoint point)
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
        public DSSurfaceCurvature CurvatureAtParameter(double u, double v)
        {
            return DSSurfaceCurvature.BySurfaceParameters(this, u, v);
        }

        /// <summary>
        /// Returns single or array of BSplineSurfaces from Surface.
        /// </summary>
        /// <returns>Array of BSplineSurface.</returns>
        [AllowRankReduction]
        public DSBSplineSurface[] ConvertToBSplineSurface()
        {
            return ConvertToBSplineSurface(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="persist"></param>
        /// <returns></returns>
        protected DSBSplineSurface[] ConvertToBSplineSurface(bool persist)
        {
            if (SurfaceEntity is IBSplineSurfaceEntity)
            {
                return new DSBSplineSurface[] { this as DSBSplineSurface };                
            }

            IBSplineSurfaceEntity[] entities = SurfaceEntity.ConvertToBSplineSurface();
            return entities.ConvertAll((IBSplineSurfaceEntity h) => h.ToBSurf(persist, this));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private DSBSplineSurface ApproxBSplineSurface()
        {
            IBSplineSurfaceEntity surface = SurfaceEntity.ApproxBSpline(-1.0);
            return new DSBSplineSurface(surface, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tol"></param>
        /// <returns></returns>
        private DSBSplineSurface ApproxBSplineSurface(double tol)
        {
            IBSplineSurfaceEntity surface = SurfaceEntity.ApproxBSpline(tol);
            return new DSBSplineSurface(surface, true);
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
        internal DSPoint PointAtParametersCore(ref double u, ref double v, double offset)
        {
            bool uchange = DSGeometryExtension.ClipParamRange(ref u);
            bool vchange = DSGeometryExtension.ClipParamRange(ref v);
            // TO DO - throw a warning each time a condition above is satisfied.    
            //throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "u or v parameter", "Surface.PointAtParameters"));

            IPointEntity pt = SurfaceEntity.PointAtParameter(u, v);
            if (!offset.EqualsTo(0.0))
            {
                DSVector normal = new DSVector(SurfaceEntity.NormalAtPoint(pt));
                DSVector translation = normal.Normalize().Scale(offset);
                IPointEntity offsetPt = pt.Add(translation);
                pt.Dispose();
                pt = offsetPt;
            }
            return pt.ToPoint(true, this);
        }

        internal override IGeometryEntity[] IntersectWithCurve(DSCurve curve)
        {
            return curve.CurveEntity.IntersectWith(SurfaceEntity);
        }

        internal override IGeometryEntity[] IntersectWithPlane(DSPlane plane)
        {
            return SurfaceEntity.IntersectWith(plane.PlaneEntity);
        }

        internal override IGeometryEntity[] IntersectWithSolid(DSSolid solid)
        {
            return solid.SolidEntity.IntersectWith(SurfaceEntity);
        }

        internal override IGeometryEntity[] IntersectWithSurface(DSSurface surf)
        {
            return SurfaceEntity.IntersectWith(surf.SurfaceEntity);
        }

        internal override IPointEntity ClosestPointTo(IPointEntity otherPoint)
        {
            return SurfaceEntity.GetClosestPoint(otherPoint);
        }

        internal override IGeometryEntity[] ProjectOn(DSGeometry other, DSVector direction)
        {
            //Solid solid = other as Solid;
            //if (null != solid)
            //    return solid.SolidEntity.Project(SurfaceEntity, direction);

            return base.ProjectOn(other, direction);
        }

        internal DSCoordinateSystem GetCSAtParameters(double u, double v)
        {
            bool uchange = DSGeometryExtension.ClipParamRange(ref u);
            bool vchange = DSGeometryExtension.ClipParamRange(ref v);
            // TO DO - throw a warning each time a condition above is satisfied.    
            //throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "u or v parameter", "Surface.PointAtUVParameters"));

            IPointEntity pos = SurfaceEntity.PointAtParameter(u, v);
            DSPoint origin = pos.ToPoint(false, null);
            DSVector xAxis = new DSVector(SurfaceEntity.TangentAtUParameter(u, v));
            DSVector yAxis = new DSVector(SurfaceEntity.TangentAtVParameter(u, v));
            DSVector zAxis = xAxis.Cross(yAxis);

            return DSCoordinateSystem.ByOriginVectors(origin, xAxis, yAxis, zAxis, false, true, false);
        }

        private static DSSurface[] SelectTrimCore(ISurfaceEntity[] surfacehosts, ISolidEntity trimmingEntity, bool keepInside)
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

    class GenericSurface : DSSurface
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
            return string.Format("DSSurface()");
        }
        #endregion
    }
}
