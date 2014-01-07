using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Interfaces;
using System.ComponentModel;
using System.Text;

namespace Autodesk.DesignScript.Geometry
{
    public class Point : Geometry
    {
        #region DATA MEMBERS
        private Point mReferencePoint;
        #endregion

        internal IPointEntity PointEntity
        {
            get { return HostImpl as IPointEntity; }
        }

        #region PROPERTIES

        //  all things coordinates

        /// <summary>
        /// 
        /// </summary>
        public double XTranslation { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public double YTranslation { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public double ZTranslation { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public double[] GlobalCoordinates { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        [Category("Primary")]
        public double X { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        [Category("Primary")]
        public double Y { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        [Category("Primary")]
        public double Z { get; private set; }

        //  spherical and cylindrical coordinates   

        /// <summary>
        /// 
        /// </summary>
        public double? Radius { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public double? Theta { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public double? Height { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public double? Phi { get; private set; }


        //  other contextual properties

        /// <summary>
        /// 
        /// </summary>
        public Surface ContextSurface
        {
            get
            {
                return Context as Surface;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Curve ContextCurve
        {
            get
            {
                return Context as Curve;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Plane ContextPlane
        {
            get
            {
                return Context as Plane;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Solid ContextSolid
        {
            get
            {
                return Context as Solid;
            }
        }

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
        public double? T { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public double? Distance { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public Vector Direction { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public Point ReferencePoint
        {
            get { return mReferencePoint; }
            set { value.AssignTo(ref mReferencePoint); }
        }

        #endregion

        #region INTERNAL_METHODS

        private void InitializeGuaranteedProperties()
        {
            X = PointEntity.X;
            Y = PointEntity.Y;
            Z = PointEntity.Z;

            XTranslation = X;
            YTranslation = Y;
            ZTranslation = Z;

            ContextCoordinateSystem = null;
            //Initialize the constructor so that it can be recreated later, if it was disposed.
            if(null == mConstructor)
                mConstructor = () => ByCoordinatesCore(X, Y, Z);
        }

        internal override bool TessellateCore(IRenderPackage package)
        {
            if (base.TessellateCore(package))
                return true;

            Color c = (this.Color == null) ? Color.Yellow : this.Color;
            package.PushPointVertex(this.X, this.Y, this.Z);
            package.PushPointVertexColor(c.RedValue, c.GreenValue, c.BlueValue, c.AlphaValue);
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                GeometryExtension.DisposeObject(ref mReferencePoint);
            base.Dispose(disposing);
        }

        #endregion

        #region PRIVATE_CONSTRUCTORS

        static void InitType()
        {
            RegisterHostType(typeof(IPointEntity), (IGeometryEntity host, bool persist) => { return new Point(host as IPointEntity, persist); });
        }

        private Point(IPointEntity host, bool persist = false) : base(host, persist)
        {
            InitializeGuaranteedProperties();
        }

        private Point ProjectOnGeometry(Geometry contextGeometry, Vector direction)
        {
            IPointEntity closestPoint = null;
            if (null == direction)
                return contextGeometry.ClosestPointTo(this);
            else
            {
                IGeometryEntity[] entities = ProjectOn(contextGeometry, direction);
                if (null == entities || entities.Length == 0)
                    throw new InvalidOperationException(string.Format(Properties.Resources.OperationFailed, "Project along direction"));
                                
                int nearestIndex = GetIndexOfNearestGeometry(entities, PointEntity);
                IGeometryEntity closestGeometry = entities[nearestIndex];
                //Clone the closest geometry
                closestPoint = closestGeometry.Clone() as IPointEntity;

                //Done with projected entities, dispose them.
                entities.DisposeObject();
            }

            return new Point(closestPoint, true);
        }

        internal override IPointEntity ClosestPointTo(IPointEntity otherPoint)
        {
            //Itself is the closest point to anything else.
            return PointEntity;
        }

        internal override IGeometryEntity[] ProjectOn(Geometry other, Vector direction)
        {
            var dir = direction.VectorEntity;
            var surf = other as Surface;
            if (null != surf)
                return surf.SurfaceEntity.Project(PointEntity, dir);

            Curve curve = other as Curve;
            if (null != curve)
            {
                return curve.CurveEntity.Project(PointEntity, dir);
            }

            Plane plane = other as Plane;
            if (null != plane)
            {
                IPointEntity pt = plane.PlaneEntity.Project(PointEntity, dir);
                return new IGeometryEntity[] { pt };
            }

            Solid solid = other as Solid;
            if (null != solid)
                return solid.SolidEntity.Project(PointEntity, dir);

            return base.Project(other, direction).Select(x=>x.GeomEntity).ToArray();
        }

        #endregion

        #region PROTECTED_CONSTRUCTORS

        protected internal Point(double xx, double yy, double zz,bool persist)
            : base(ByCoordinatesCore(xx, yy, zz),persist)
        {
            InitializeGuaranteedProperties();
        }

        protected Point(Point refPoint, double deltaX, double deltaY, double deltaZ,bool persist)
            : base(ByOffsetCore(refPoint, deltaX, deltaY, deltaZ),persist)
        {
            InitializeGuaranteedProperties();
        }

        protected Point(CoordinateSystem contextCoordinateSystem, double xTranslation, double yTranslation, double zTranslation,bool persist)
            : base(ByCartesianCoordinatesCore(contextCoordinateSystem,xTranslation,yTranslation,zTranslation),persist)
        {
            InitializeGuaranteedProperties();
            ContextCoordinateSystem = contextCoordinateSystem;
            XTranslation = xTranslation;
            YTranslation = yTranslation;
            ZTranslation = zTranslation;
        }

        protected Point(CoordinateSystem contextCoordinateSystem, double radius, double theta, double height,bool persist,bool unused)
            : base(ByCylindricalCoordinatesCore(contextCoordinateSystem, radius, theta, height),true)
        {
            InitializeGuaranteedProperties();
            Radius = radius;
            Theta = theta;
            Height = height;
        }

        protected Point(CoordinateSystem contextCoordinateSystem, double radius, double theta, double phi,bool persist, int unused)
            : base(BySphericalCoordinatesCore(contextCoordinateSystem, radius, theta, phi),persist)
        {
            InitializeGuaranteedProperties();
            Radius = radius;
            Theta = theta;
            Phi = phi;
        }

        #endregion

        #region DESIGNSCRIPT_CONSTRUCTORS

        /// <summary>
        /// Constructs a Point by coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="z">Z coordinate</param>
        /// <returns></returns>
        public static Point ByCoordinates(double x, double y, double z)
        {
            return new Point(x, y, z, true);
        }
        
        /// <summary>
        /// contructs a point with respect to another point and offset in x,y,z directions
        /// </summary>
        /// <param name="referencePoint">the point with respect to which the new point is contructed</param>
        /// <param name="deltaX">offset in the x direction</param>
        /// <param name="deltaY">offset in the y direction</param>
        /// <param name="deltaZ">offset in the z direction</param>
        /// <returns></returns>
        public static Point ByOffset(Point referencePoint, double deltaX, double deltaY, double deltaZ)
        {
            Point newPnt =  new Point(referencePoint, deltaX, deltaY, deltaZ, true);
            newPnt.ReferencePoint = referencePoint;
            return newPnt;
        }
        
        /// <summary>
        /// Constructs a point at a given parameter on the input curve.
        /// </summary>
        /// <param name="contextCurve">Input curve</param>
        /// <param name="parameter">Parameter value.</param>
        /// <returns>Point</returns>
        public static Point AtParameter(Curve contextCurve, double parameter)
        {
            if (contextCurve == null)
            {
                throw new ArgumentNullException("contextCurve");
            }
            
            var pt = contextCurve.PointAtParameter(parameter);
            if (null == pt)
                return null;
            pt.Context = contextCurve;
            pt.T = parameter;
            pt.Distance = contextCurve.DistanceAtParameter(parameter);
            pt.Persist();
            return pt;
        }

        /// <summary>
        /// Contructs a point on a given surface at given U, V parameter.
        /// </summary>
        /// <param name="contextSurface">Input Surface</param>
        /// <param name="u">U parameter value.</param>
        /// <param name="v">V parameter value.</param>
        /// <returns>Point</returns>
        public static Point AtParameter(Surface contextSurface, double u, double v)
        {
          return AtParameter(contextSurface, u, v, 0.0);
        }

        /// <summary>
        /// Contructs a point on the normal of a given surface at a given offset
        /// </summary>
        /// <param name="contextSurface">Input surface</param>
        /// <param name="u">U parameter value</param>
        /// <param name="v">V parameter value</param>
        /// <param name="offset">Offset in the direction of normal</param>
        /// <returns>Point</returns>
        internal static Point AtParameter(Surface contextSurface, double u, double v, double offset)
        {
          if (contextSurface == null)
            return null;

          var pt = contextSurface.PointAtParametersCore(ref u, ref v, offset);
          if (null == pt)
            return null;

          pt.Context = contextSurface;
          pt.U = u;
          pt.V = v;
          pt.Distance = offset;
          pt.Persist();
          return pt;
        }

        /// <summary>
        /// Constructors a point along a curve with a input distance from start point of the curve.
        /// </summary>
        /// <param name="contextCurve">Input Curve</param>
        /// <param name="distance">Distance value.</param>
        /// <returns>Point</returns>
        public static Point AtDistance(Curve contextCurve, double distance)
        {
            if (contextCurve == null)
            {
                throw new ArgumentNullException("contextCurve");
            }

            var pt = contextCurve.PointAtDistance(distance);
            pt.Context = contextCurve;
            pt.Distance = distance;
            pt.T = contextCurve.ParameterAtDistance(distance);
            pt.Persist();
            return pt;
        }

        /// <summary>
        /// Constructors a point based on the Cartesian CoordinateSystem.
        /// </summary>
        /// <param name="contextCoordinateSystem">The coordinate system with respect to which the x,y,z translation are given</param>
        /// <param name="xTranslation">Translation in the x direction with respect to context coordinate system</param>
        /// <param name="yTranslation">Translation in the y direction with respect to context coordinate system</param>
        /// <param name="zTranslation">Translation in the z direction with respect to context coordinate system</param>
        /// <returns></returns>
        public static Point ByCartesianCoordinates(CoordinateSystem contextCoordinateSystem, double xTranslation, double yTranslation, double zTranslation)
        {
            return new Point(contextCoordinateSystem, xTranslation, yTranslation, zTranslation, true);
        }

        /// <summary>
        /// Constructors a point based on the cylindrical CoordinateSystem.
        /// </summary>
        /// <param name="contextCoordinateSystem">The coordinate system</param>
        /// <param name="radius">The radius of the cylinder</param>
        /// <param name="theta">The theta of the cylinder</param>
        /// <param name="height">The height of the cylinder</param>
        /// <returns></returns>
        public static Point ByCylindricalCoordinates(CoordinateSystem contextCoordinateSystem, double radius, double theta, double height)
        {
            return new Point(contextCoordinateSystem, radius, theta, height, true, true);
        }
        
        /// <summary>
        /// Constructors a point based on the spherical CoordinateSystem.
        /// </summary>
        /// <param name="contextCoordinateSystem">The coordinate system</param>
        /// <param name="radius">The radius of the sphere</param>
        /// <param name="theta">The theta value of the sphere</param>
        /// <param name="phi">The phi value of the sphere</param>
        /// <returns></returns>
        public static Point BySphericalCoordinates(CoordinateSystem contextCoordinateSystem, double radius, double theta, double phi)
        {
            return new Point(contextCoordinateSystem, radius, theta, phi, true, 1);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        protected override Geometry Translate(Vector offset)
        {
            return Translate(offset, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="persist"></param>
        /// <returns></returns>
        internal Point Translate(Vector offset, bool persist)
        {
            if (offset == null)
            {
                throw new ArgumentNullException("direction");
            }

            IPointEntity pthost = PointEntity.Add(offset);
            Point pt = pthost.ToPoint(persist, this);

            //  setup backlinks
            pt.Direction = offset.Normalize();
            pt.Distance = offset.Length;

            return pt;
        }

        #endregion

        #region CORE_METHODS

        private static IPointEntity ByCoordinatesCore(double xx, double yy, double zz)
        {
            IPointEntity position = HostFactory.Factory.PointByCoordinates(xx, yy, zz);
            return position;
        }

        private static IPointEntity ByOffsetCore(Point refPoint, double deltaX, double deltaY, double deltaZ)
        {
            if (refPoint == null)
            {
                throw new ArgumentNullException("refPoint");
            }

            var pt = HostFactory.Factory.PointByCoordinates(refPoint.X + deltaX, refPoint.Y + deltaY, refPoint.Z + deltaZ);
            return pt;
        }
        
        private static IPointEntity ByCartesianCoordinatesCore(CoordinateSystem contextCoordinateSystem, double xTranslation, double yTranslation, double zTranslation)
        {
            if (contextCoordinateSystem == null)
            {
                return ByCoordinatesCore(xTranslation, yTranslation, zTranslation);
            }
            IPointEntity pos = HostFactory.Factory.PointByCartesianCoordinates(contextCoordinateSystem.CSEntity, xTranslation, yTranslation, zTranslation);
            if (pos == null)
                throw new InvalidOperationException(string.Format(Properties.Resources.OperationFailed, "Point.ByCartesianCoordinates"));
            return pos;
        }

        private static IPointEntity ByCylindricalCoordinatesCore(CoordinateSystem contextCoordinateSystem, double radius, double theta, double height)
        {
            if (contextCoordinateSystem == null)
            {
                throw new ArgumentNullException("contextCoordinateSystem");
            }
            else if (radius == 0.0)
            {
                throw new ArgumentException(Properties.Resources.IsZeroRadius);
            }

            using (var csEntity = HostFactory.Factory.CoordinateSystemByCylindricalCoordinates(CoordinateSystem.WCS.CSEntity, radius, theta, height))
            {
                IPointEntity origin = csEntity.Origin;
                IPointEntity host = ByCartesianCoordinatesCore(contextCoordinateSystem, origin.X, origin.Y, origin.Z);
                if (null == host)
                    throw new InvalidOperationException(string.Format(Properties.Resources.OperationFailed, "Point.ByCylindricalCoordinates"));
                return host;
            }
        }

        private static IPointEntity BySphericalCoordinatesCore(CoordinateSystem contextCoordinateSystem, double radius, double theta, double phi)
        {
            if (contextCoordinateSystem == null)
            {
                throw new ArgumentNullException(string.Format("contextCoordinateSystem"));
            }
            else if (radius == 0.0)
            {
                throw new ArgumentException(Properties.Resources.IsZeroRadius);
            }
            using (var csEntity = HostFactory.Factory.CoordinateSystemBySphericalCoordinates(CoordinateSystem.WCS.CSEntity, radius, theta, phi))
            {
                IPointEntity origin = csEntity.Origin;
                IPointEntity pt = ByCartesianCoordinatesCore(contextCoordinateSystem, origin.X, origin.Y, origin.Z);
                if (null == pt)
                    throw new InvalidOperationException(string.Format(Properties.Resources.OperationFailed, "Point.BySphericalCoordinates"));
                return pt;
            }
        }
        
        #endregion

        #region METHODS
        
        /// <summary>
        /// Constructors a point by projecting a point on a plane. It is 
        /// equivalent to finding the nearest point on the plane.
        /// </summary>
        /// <param name="contextPlane">Plane of projection</param>
        /// <returns>Point</returns>
        public Point Project(Plane contextPlane)
        {
            if (contextPlane == null)
            {
                throw new ArgumentNullException("contextPlane");
            }

            var pt = contextPlane.Project(this);
            pt.Context = contextPlane;
            pt.ReferencePoint = this;
            pt.Persist();
            return pt;
        }

        /// <summary>
        /// Constructors a point by projecting a point on a plane with given project direction.
        /// </summary>
        /// <param name="contextPlane">Plane of projection</param>
        /// <param name="direction">Projection direction</param>
        /// <returns>Point</returns>
        public Point Project(Plane contextPlane, Vector direction)
        {
            if (contextPlane == null)
            {
                throw new ArgumentNullException("contextPlane");
            }
            else if (direction == null)
            {
                throw new ArgumentNullException("direction");
            }
            else if (direction.IsZeroVector())
            {
                throw new ArgumentException(string.Format(Properties.Resources.IsZeroVector, "direction"));
            }
            else if (direction.IsPerpendicular(contextPlane.Normal))
            {
                throw new ArgumentException(string.Format(Properties.Resources.IsParallel, "contextPlane", "direction", "Point.Project"));
            }

            var pt = contextPlane.Project(this, direction);
            pt.Context = contextPlane;
            pt.ReferencePoint = this;
            pt.Direction = direction;
            pt.Persist();
            return pt;
        }

        /// <summary>
        /// Constructs a point by projecting a point on a curve which which closest to the curve
        /// </summary>
        /// <param name="curve">the curve on which the projection is to be made.</param>
        /// <returns></returns>
        public Point Project(Curve curve)
        {
            if (curve == null)
            {
                throw new ArgumentNullException("curve");
            }

            var pt = curve.Project(this);
            pt.Context = curve;
            double param = curve.ParameterAtPoint(pt);
            pt.T = param;
            pt.Distance = curve.DistanceAtParameter(param);
            pt.ReferencePoint = this;
            pt.Persist();
            return pt;
        }

        /// <summary>
        /// Constructs a point by projecting a point on a curve with given project direction.
        /// </summary>
        /// <param name="curve">the curve on which the projection is to be made.</param>
        /// <param name="direction">the direction vector of the projection</param>
        /// <returns></returns>
        public Point Project(Curve curve, Vector direction)
        {
            if (curve == null)
            {
                throw new ArgumentNullException("curve");
            }
            else if (direction == null)
            {
                throw new ArgumentNullException("direction");
            }
            else if (direction.IsZeroVector())
            {
                throw new ArgumentException(string.Format(Properties.Resources.IsZeroVector, "direction"));
            }

            var pt = curve.Project(this, direction);
            pt.Context = curve;
            pt.ReferencePoint = this;
            pt.Direction = direction;
            pt.Persist();
            return pt;
        }

        /// <summary>
        /// Constructs a point by projecting a point on surface. It is equivalent 
        /// to finding the nearest point on the surface
        /// </summary>
        /// <param name="contextSurface">The surface on which the projection is to be made.</param>
        /// <returns>Projected point on surface</returns>
        public Point Project(Surface contextSurface)
        {
            if (null == contextSurface)
                throw new ArgumentNullException("contextSurface");

            Point pt = ProjectOnGeometry(contextSurface, null);
            if (null == pt)
                throw new InvalidOperationException(string.Format(Properties.Resources.OperationFailed, "Project on surface"));

            pt.Context = contextSurface;
            pt.ReferencePoint = this;
            double[] parameters = contextSurface.ParameterAtPoint(pt);
            if (null != parameters)
            {
                pt.U = parameters[0];
                pt.V = parameters[1];
            }
            return pt;
        }

        /// <summary>
        /// Constructs a point by projecting a point on surface with given 
        /// project direction.
        /// </summary>
        /// <param name="contextSurface">The surface on which the projection is to be made.</param>
        /// <param name="direction">The direction vector of the projection</param>
        /// <returns>Projected point on surface</returns>
        public Point Project(Surface contextSurface, Vector direction)
        {
            if (null == contextSurface)
                throw new ArgumentNullException("contextSurface");
            if (null == direction || direction.IsZeroVector())
                throw new ArgumentException(string.Format(Properties.Resources.InvalidInput, direction, "Project on surface"), "direction");

            Point pt = ProjectOnGeometry(contextSurface, direction);
            pt.Context = contextSurface;
            pt.Direction = direction;
            pt.ReferencePoint = this;
            double[] parameters = contextSurface.ParameterAtPoint(pt);
            if (null != parameters)
            {
                pt.U = parameters[0];
                pt.V = parameters[1];
            }

            return pt;
        }

        /// <summary>
        /// Constructs a point by projecting a point on solid. It is equivalent 
        /// to finding the nearest point on the solid
        /// </summary>
        /// <param name="contextSolid">The solid on which the projection is to be made.</param>
        /// <returns>Projected point on solid</returns>
        public Point Project(Solid contextSolid)
        {
            if (null == contextSolid)
                throw new ArgumentNullException("contextSolid");

            Point pt = ProjectOnGeometry(contextSolid, null);
            if (null == pt)
                throw new InvalidOperationException(string.Format(Properties.Resources.OperationFailed, "Project on surface"));

            pt.Context = contextSolid;
            pt.ReferencePoint = this;
            return pt;
        }

        /// <summary>
        /// Constructs a point by projecting a point on solid with given 
        /// project direction.
        /// </summary>
        /// <param name="contextSolid">The solid on which the projection is to be made.</param>
        /// <param name="direction">The direction vector of the projection</param>
        /// <returns>Projected point on solid</returns>
        public Point Project(Solid contextSolid, Vector direction)
        {
            if (null == contextSolid)
                throw new ArgumentNullException("contextSurface");
            if (null == direction || direction.IsZeroVector())
                throw new ArgumentException(string.Format(Properties.Resources.InvalidInput, direction, "Project on surface"), "direction");

            Point pt = ProjectOnGeometry(contextSolid, direction);
            pt.Context = contextSolid;
            pt.Direction = direction;
            pt.ReferencePoint = this;

            return pt;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="otherPoint"></param>
        /// <returns></returns>
        public bool IsCoincident(Point otherPoint)
        {
            return Equals(otherPoint);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Geometry SelectNearest(Geometry[] contextGeometries)
        {
            if (contextGeometries == null)
                throw new System.ArgumentNullException("contextGeometries");
            IGeometryEntity[] hostentities = contextGeometries.ConvertAll(GeometryExtension.ToEntity<Geometry, IGeometryEntity>);
            if(hostentities == null)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "contextGeometries", "SelectNearest"), "contextGeometries");
            int nearestIndex = GetIndexOfNearestGeometry(hostentities, PointEntity);
            IGeometryEntity nearestGeom = hostentities[nearestIndex];
            return Geometry.ToGeometry(nearestGeom, false); //returning one of the existing geometry, so persist is no-op.
        }
        public int IndexOfNearest(Geometry[] contextGeometries)
        {
            if (contextGeometries == null)
                throw new System.ArgumentNullException("contextGeometries");
            IGeometryEntity[] hostentities = contextGeometries.ConvertAll(GeometryExtension.ToEntity<Geometry, IGeometryEntity>);
            if(hostentities == null)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "contextGeometries", "IndexOfNearest"), "contextGeometries");
            return GetIndexOfNearestGeometry(hostentities, PointEntity);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="contextGeometries"></param>
        /// <param name="distanceCriteria"></param>
        /// <returns></returns>
        public Autodesk.DesignScript.Geometry.Geometry[] SelectWithinDistance(Geometry[] contextGeometries, double distanceCriteria)
        {
            if (contextGeometries == null)
                throw new System.ArgumentNullException("contextGeometries");
            else if (contextGeometries.Length == 0)
            {
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZero, "number of context geometries"), "contextGeometries");
            }

            var absDist = Math.Abs(distanceCriteria);

            List<Geometry> selectedGeoms = new List<Geometry>();
            foreach (var geom in contextGeometries)
            {
                double distance = geom.GeomEntity.DistanceTo(PointEntity);
                if (GeometryExtension.LessThanOrEquals(distance, absDist))
                {
                    selectedGeoms.Add(geom);
                }
            }

            return selectedGeoms.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contextGeometries"></param>
        /// <returns></returns>
        public Autodesk.DesignScript.Geometry.Geometry[] SortByDistance(Geometry[] contextGeometries)
        {
            if (contextGeometries == null)
                throw new System.ArgumentNullException("contextGeometries");
            else if (contextGeometries.Length == 0)
            {
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZero, "number of context geometries"), "contextGeometries");
            }

            return SortByDistance(contextGeometries, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contextGeometries"></param>
        /// <param name="sortAscending"></param>
        /// <returns></returns>
        public Autodesk.DesignScript.Geometry.Geometry[] SortByDistance(Geometry[] contextGeometries, bool sortAscending)
        {
            if (contextGeometries == null)
                throw new System.ArgumentNullException("contextGeometries");
            else if (contextGeometries.Length == 0)
            {
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZero, "number of context geometries"), "contextGeometries");
            }

            //  insert all the geometries with their distance from this point in this sorted dictionary
            //
            //SortedDictionary<double, Geometry> geomsWithDistance = new SortedDictionary<double, Geometry>();
            List<KeyValuePair<double, Geometry>> geomsWithDistance = new List<KeyValuePair<double, Geometry>>();
            foreach (var geom in contextGeometries)
            {
                geomsWithDistance.Add(new KeyValuePair<double, Geometry>(geom.GeomEntity.DistanceTo(PointEntity), geom));
            }
            DoubleComparer comparer = new DoubleComparer(sortAscending);
            geomsWithDistance.Sort((KeyValuePair<double, Geometry> x, KeyValuePair<double, Geometry> y) => comparer.Compare(x.Key, y.Key));
            Geometry[] sortedList = new Geometry[geomsWithDistance.Count];
            int i = 0;
            foreach (var item in geomsWithDistance)
            {
                sortedList[i++] = item.Value;
            }

            return sortedList;
        }

        #endregion

        #region UTILITY_METHODS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="otherPoint"></param>
        /// <returns></returns>
        public Vector DirectionTo(Point otherPoint)
        {
            if (otherPoint == null)
            {
                return null;
            }

            var delx = otherPoint.X - X;
            var dely = otherPoint.Y - Y;
            var delz = otherPoint.Z - Z;
            return new Vector(delx, dely, delz);
        }

        public double DistanceTo(Point otherPoint)
        {
            if (otherPoint == null)
            {
                return 0.0;
            }

            var delx = otherPoint.X - X;
            var dely = otherPoint.Y - Y;
            var delz = otherPoint.Z - Z;
            var sqDist = delx * delx + dely * dely + delz * delz;
            return Math.Sqrt(sqDist);
        }
        #endregion

        #region FROM_OBJECT

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected override bool Equals(DesignScriptEntity obj)
        {
            if (base.Equals(obj))
            {
                return true;
            }
            var pt = obj as Point;
            if (pt == null)
            {
                return false;
            }
            else
            {
                return GeometryExtension.Equals(X, pt.X) && GeometryExtension.Equals(Y, pt.Y) &&
                    GeometryExtension.Equals(Z, pt.Z);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var f6 = GeometryExtension.DoublePrintFormat;
            return string.Format("Point(X = {0}, Y = {1}, Z = {2})", X.ToString(f6), Y.ToString(f6), Z.ToString(f6));
        }
        #endregion
    }
}
