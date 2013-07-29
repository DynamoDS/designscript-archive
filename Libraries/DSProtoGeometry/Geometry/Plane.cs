using System;
using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    public class DSPlane : DSGeometry
    {
        #region DATA MEMBERS
        internal static readonly double kDefaultSize = 10.0;
        private DSPolygon mDisplayPolygon = null;
        private DSPoint mOrigin;
        #endregion

        internal IPlaneEntity PlaneEntity { get { return HostImpl as IPlaneEntity; } }

        #region INTERNAL_METHODS

        private void InitializeGuaranteedProperties()
        {
            Normal = new DSVector(PlaneEntity.Normal);
        }

        #endregion

        #region PRIVATE_CONSTRUCTORS

        static void InitType()
        {
            RegisterHostType(typeof(IPlaneEntity), (IGeometryEntity host, bool persist) => { return new DSPlane(host as IPlaneEntity, kDefaultSize, persist); });
        }

        private DSPlane(IPlaneEntity entity, double size, bool display = false)
            : base(entity, false)
        {
            InitializeGuaranteedProperties();
            if (display && size>1)
                mDisplayPolygon = CreatePlaneVisuals(size,true);
        }

        internal override IDisplayable Display
        {
            get
            {
                if (null == mDisplayPolygon)
                    mDisplayPolygon = CreatePlaneVisuals(kDefaultSize, false);

                return mDisplayPolygon.Display;
            }
        }

        public override bool Visible
        {
            get
            {
                return Display.Visible;
            }
            set
            {
                Display.Visible = value;
            }
        }

        private DSPolygon CreatePlaneVisuals(double size, bool persist)
        {
            //  to display a plane we would be required to draw a rectangle in the plane
            //
            //
            /*
            given origin, X & Y
            we want to evaluate the four corner points based on size

            where o is the origin of the plane

                Y 
                    |
                    |        
                    |      v1  ______________  v2
                    |         |              |
                    |         |              |
                    |         |      o       |
                    |         |              |
                    |      v4 |______________| v3
                    |
                    |                                       X
                    |_________________________________________
             * 
            */
            ICoordinateSystemEntity coordSys = PlaneEntity.GetCoordinateSystem();
            IPointEntity orig = coordSys.Origin;

            var xNorm = new DSVector(coordSys.XAxis).Normalize().MultiplyBy(size / 2.0);
            var yNorm = new DSVector(coordSys.YAxis).Normalize().MultiplyBy(size / 2.0);

            var dirv1 = yNorm - xNorm;
            var dirv2 = yNorm + xNorm;
            var dirv3 = dirv1.Negate();
            var dirv4 = dirv2.Negate();

            DSPoint[] positions = { orig.Add(dirv1).ToPoint(false, null), orig.Add(dirv2).ToPoint(false, null), 
                                    orig.Add(dirv3).ToPoint(false, null), orig.Add(dirv4).ToPoint(false, null) };
            return new DSPolygon(positions, persist);
        }

        private static DSPlane FromCoordinateSystem(DSCoordinateSystem cs)
        {
            if (null == cs)
                return null;

            DSPlane plane = DSPlane.ByOriginNormal(cs.Origin, cs.ZAxis);
            if (null == plane)
                return null;

            plane.ContextCoordinateSystem = cs;
            return plane;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                DSGeometryExtension.DisposeObject(ref mOrigin);

            base.Dispose(disposing);
        }

        protected override void DisposeDisplayable()
        {
            //We need to call Dispose on display polygon so that this polygon's 
            //display is disposed.
            if (null != mDisplayPolygon)
                mDisplayPolygon.Dispose();

            base.DisposeDisplayable();
        }

        #endregion

        #region PROTECTED_CONSTRUCTORS

        protected internal DSPlane(DSPoint origin, DSVector normal, double size, bool display = false, DSGeometry context = null)
            : base(ByOriginNormalCore(origin, ref normal, size), false)
        {
            InitializeGuaranteedProperties();
            Size = size;
            Context = context;
            if (display)
                mDisplayPolygon = CreatePlaneVisuals(size,true);
        }

        #endregion

        #region DESIGNSCRIPT_CONSTRUCTORS

        /// <summary>
        /// Constructs a Plane object using a point on the plane and its normal vector.
        /// </summary>
        /// <param name="origin">Origin of the plane</param>
        /// <param name="normal">Normal to the surface of the plane</param>
        /// <returns></returns>
        public static DSPlane ByOriginNormal(DSPoint origin, DSVector normal)
        {
            return new DSPlane(origin, normal, kDefaultSize, true);
        }

        /// <summary>
        /// Constructs a Plane using a point on the plane and its normal and displays it using the display size as input.
        /// </summary>
        /// <param name="origin">Origin of the plane</param>
        /// <param name="normal">Normal to the surface of the plane</param>
        /// <param name="size">Size of the plane, must be >= 1</param>
        /// <returns></returns>
        public static DSPlane ByOriginNormal(DSPoint origin, DSVector normal, double size)
        {
            return new DSPlane(origin, normal, size, true);
        }

        /// <summary>
        /// Constructors a plane at a point on the curve by given paramater and 
        /// use the tangent at the point as Normal of the plane
        /// </summary>
        /// <param name="contextCurve"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static DSPlane AtParameter(DSCurve contextCurve, double t)
        {
            if (null == contextCurve)
                throw new ArgumentNullException("contextCurve");
            
            return contextCurve.PlaneAtParameter(t);
        }

        /// <summary>
        /// Constructors a plane on a surface by given parameter. 
        /// </summary>
        /// <param name="contextSurface"></param>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static DSPlane AtParameter(DSSurface contextSurface, double u, double v)
        {
            DSCoordinateSystem cs = DSCoordinateSystem.AtParameterCore(contextSurface, u, v, false);
            DSPlane plane = DSPlane.FromCoordinateSystem(cs);
            plane.Context = contextSurface;
            plane.U = u;
            plane.V = v;
            return plane;
        }

        /// <summary>
        /// Constructors a plane on a curve by given distance. 
        /// </summary>
        /// <param name="contextCurve"></param>
        /// <param name="distance">Curvilinear distance.</param>
        /// <returns></returns>
        public static DSPlane AtDistance(DSCurve contextCurve, double distance)
        {
            if (null == contextCurve)
                throw new ArgumentNullException("contextCurve");

            return contextCurve.PlaneAtDistance(distance);
        }

        #endregion

        #region CORE_METHODS
        
        private static IPlaneEntity ByOriginNormalCore(DSPoint origin, ref DSVector normal, double size)
        {
            if (origin == null)
            {
                throw new ArgumentNullException("origin");
            }
            else if (normal == null)
            {
                throw new ArgumentNullException("normal");
            }
            else if (normal.IsZeroVector())
                throw new ArgumentException(string.Format(Properties.Resources.IsZeroVector, normal), "normal");

            normal = normal.IsNormalized ? normal : normal.Normalize();
            IPlaneEntity entity = HostFactory.Factory.PlaneByOriginNormal(origin.PointEntity, normal.IVector);
            if (null == entity)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "DSPlane.ByOriginNormal"));
            return entity;
        }

        #endregion

        #region DESIGNSCRIPT_METHODS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        public DSPlane Offset(double distance)
        {
            return Offset(Normal.MultiplyBy(distance));
        }

        private DSPlane Offset(DSVector offset)
        {
            IPointEntity origin = Origin.PointEntity;
            IPointEntity newOrigin = origin.Add(offset);
            DSPlane plane = DSPlane.ByOriginNormal(newOrigin.ToPoint(false, null), Normal, Size);
            plane.Context = this;
            return plane;
        }

        /// <summary>
        /// Translates any geometry type by the given distance in the given 
        /// direction.
        /// </summary>
        /// <param name="offset">Displacement direction and distance.</param>
        /// <returns>Transformed Geometry.</returns>
        protected override DSGeometry Translate(DSVector offset)
        {
            return Offset(offset);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public DSPoint Project(DSPoint point)
        {
            return Project(point, Normal);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public DSPoint Project(DSPoint point, DSVector direction)
        {
            if (point == null || direction == null || direction.IsZeroVector())
            {
                return null;
            }

            IPointEntity projectedPt = PlaneEntity.Project(point.PointEntity, direction.IVector);
            if (null == projectedPt)
            {
                return null;
            }
            return projectedPt.ToPoint(true, this);
        } 

        #endregion

        #region PROTECTED_METHODS

        internal override IGeometryEntity[] IntersectWithCurve(DSCurve curve)
        {
            return curve.CurveEntity.IntersectWith(PlaneEntity);
        }

        internal override IGeometryEntity[] IntersectWithPlane(DSPlane plane)
        {
            ILineEntity line = PlaneEntity.IntersectWith(plane.PlaneEntity);
            if (null != line)
                return new IGeometryEntity[] { line };

            return null;
        }

        internal override IGeometryEntity[] IntersectWithSurface(DSSurface surf)
        {
            return surf.SurfaceEntity.IntersectWith(PlaneEntity);
        }

        internal override IGeometryEntity[] IntersectWithSolid(DSSolid solid)
        {
            return solid.SolidEntity.IntersectWith(PlaneEntity);
        }

        internal override IPointEntity ClosestPointTo(IPointEntity otherPoint)
        {
            return PlaneEntity.Project(otherPoint, Normal.IVector);
        }

        internal override DSGeometry TransformBy(ICoordinateSystemEntity csEntity)
        {
            //Let the default code handle orthogonal transform.
            if (csEntity.IsScaledOrtho())
                return base.TransformBy(csEntity);

            using (IPointEntity origin = Origin.PointEntity.CopyAndTransform(DSCoordinateSystem.WCS.CSEntity, csEntity) as IPointEntity)
            {
                using (IPointEntity pt = Origin.PointEntity.CopyAndTranslate(Normal.IVector) as IPointEntity)
                {
                    using (IPointEntity transformPt = pt.CopyAndTransform(DSCoordinateSystem.WCS.CSEntity, csEntity) as IPointEntity)
                    {
                        DSVector normal = origin.GetVectorTo(transformPt).Normalize();
                        return DSPlane.ByOriginNormal(origin.ToPoint(false, null), normal, Size);
                    }
                }
            }
        }

        #endregion

        #region PROPERTIES

        /// <summary>
        /// 
        /// </summary>
        public DSPoint Origin
        {
            get 
            { 
                if(null == mOrigin)
                    mOrigin = PlaneEntity.Origin.ToPoint(false, this);
                return mOrigin; 
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DSVector Normal { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public double Size { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public double? T { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public double? U { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public double? V { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public double? Distance { get; internal set; }

        #endregion

        #region FROM_OBJECT

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected override bool Equals(DesignScriptEntity obj)
        {
            var pln = obj as DSPlane;
            if (pln == null)
            {
                return false;
            }

            return Origin.Equals(pln.Origin) && Normal.Equals(pln.Normal);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("DSPlane(Origin = {0}, Normal = {1}, Size ={2})",
                            Origin, Normal, Size.ToString(DSGeometryExtension.DoublePrintFormat));
        }

        #endregion
    }
}

