using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

namespace Autodesk.DesignScript.Geometry
{
    public abstract class Curve : Geometry
    {
        #region DATA MEMBERS
        private Point mStartPoint = null;
        private Point mEndPoint = null;
        #endregion

        #region CURVE_PROPERTIES

        protected abstract Point GetStartPoint();
        protected abstract Point GetEndPoint();

        /// <summary>
        /// 
        /// </summary>
        /// 
        [Category("Primary")]
        public Point StartPoint 
        {
            get
            {
                if(null == mStartPoint)
                    mStartPoint = GetStartPoint();
                return mStartPoint;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Category("Primary")]
        public Point EndPoint 
        {
            get
            {
                if(null == mEndPoint)
                    mEndPoint = GetEndPoint();
                return mEndPoint;
            }
        }

        public Curve ContextCurve
        {
            get { return Context as Curve; }
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual double Length 
        {
            get
            {
                return CurveEntity.Length;
            } 
        }

        internal ICurveEntity CurveEntity { get { return HostImpl as ICurveEntity; } }

        /// <summary>
        /// 
        /// </summary>
        public abstract bool IsLinear {get;}

        /// <summary>
        /// 
        /// </summary>
        public abstract bool IsCircular { get;}

        /// <summary>
        /// 
        /// </summary>
        public abstract bool IsPlanar { get;}

        /// <summary>
        /// 
        /// </summary>
        public abstract bool IsElliptical { get;}

        /// <summary>
        /// 
        /// </summary>
        public abstract bool IsSelfIntersecting { get;}

        /// <summary>
        /// 
        /// </summary>
        public abstract bool IsClosed { get;}

        #endregion

        #region UTILITY_METHODS

        protected double DeNormalizeParameter(double param)
        {
            double startParam = CurveEntity.StartParameter();
            double endParam = CurveEntity.EndParameter();
            
            var diff = endParam - startParam;
            bool paramchange = GeometryExtension.ClipParamRange(ref param);
          //TO DO - throw a warning if param is changed
            var denormParam = startParam + diff * param; 
            return denormParam;
        }

        #endregion

        #region PUBLIC_METHODS

        /// <summary>
        /// def RevolveAsSolid : Solid (axisOrigin : Point, axisDirection : Vector, startAngle : double, sweepAngle : double)
        /// 
        /// Returns a Solid by revolving the closed curve about an axis defined
        /// by axisOrigin point and axisDirection Vector. startAngle determines 
        /// where the curve starts to revolve, sweepAngle determines the 
        /// revolving angle
        /// </summary>
        /// <param name="axisOrigin">Origin point of axis of revolution</param>
        /// <param name="axisDirection">Direction vector of axis of revolution</param>
        /// <param name="startAngle">Start angle in degrees</param>
        /// <param name="sweepAngle">Sweep angle for rotation in degrees</param>
        /// <returns>Revolved Solid</returns>
        public Solid RevolveAsSolid(Point axisOrigin, Vector axisDirection, double startAngle, double sweepAngle)
        {
            if (axisOrigin == null)
                throw new System.ArgumentNullException("axisOrigin");
            else if (axisDirection == null)
                throw new System.ArgumentNullException("axisDirection");
            else if (!IsClosed)
                throw new System.InvalidOperationException(Properties.Resources.CurveNotClosed);
            else if (!IsPlanar)
                throw new System.InvalidOperationException(Properties.Resources.CurveNotPlanar);
            else if (axisDirection.IsZeroVector())
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZeroVector, axisDirection), "axisDirection");
            else if (sweepAngle.EqualsTo(0.0))
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZeroAngle, "sweep"), "sweepAngle");

            return Solid.Revolve(this, axisOrigin, axisDirection, startAngle, sweepAngle);
        }

        /// <summary>
        /// def RevolveAsSolid : Solid (axisOrigin : Point, axisDirection : Vector)
        /// 
        /// Returns a Solid by revolving close curve about an axis defined by 
        /// axisOrigin point and axisDirection Vector. Assuming sweep angle = 360
        /// and start angle = 0.
        /// </summary>
        /// <param name="axisOrigin">Origin point of axis of revolution</param>
        /// <param name="axisDirection">Direction vector of axis of revolution</param>
        /// <returns>Revolved Solid</returns>
        public Solid RevolveAsSolid(Point axisOrigin, Vector axisDirection)
        {
            if (axisOrigin == null)
                throw new System.ArgumentNullException("axisOrigin");
            else if (axisDirection == null)
                throw new System.ArgumentNullException("axisDirection");
            else if (!IsClosed)
                throw new System.InvalidOperationException(Properties.Resources.CurveNotClosed);
            else if (!IsPlanar)
                throw new System.InvalidOperationException(Properties.Resources.CurveNotPlanar);
            else if (axisDirection.IsZeroVector())
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZeroVector, axisDirection), "axisDirection");

            return Solid.Revolve(this, axisOrigin, axisDirection);
        }

        /// <summary>
        /// def RevolveAsSolid : Solid (axis : Line, startAngle : double, sweepAngle : double)
        /// 
        /// Returns a Solid by revolving the closed curve about a given axis 
        /// line. startAngle determines where the curve starts to revolve, 
        /// sweepAngle determines the revolving angle.
        /// </summary>
        /// <param name="axis">Line curve as axis of revolution</param>
        /// <param name="startAngle">Start angle in degrees</param>
        /// <param name="sweepAngle">Sweep angle for rotation in degrees</param>
        /// <returns>Revolved Solid</returns>
        public Solid RevolveAsSolid(Line axis, double startAngle, double sweepAngle)
        {
            if (!IsClosed)
                throw new System.InvalidOperationException(Properties.Resources.CurveNotClosed);
            else if (!IsPlanar)
                throw new System.InvalidOperationException(Properties.Resources.CurveNotPlanar);
            else if (axis == null)
                throw new System.ArgumentNullException("axis");
            else if (axis.Length.EqualsTo(0.0))
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZeroLength, "axis length"), "axis");

            return Solid.Revolve(this, axis, startAngle, sweepAngle);
        }

        /// <summary>
        /// def RevolveAsSolid : Solid (axis : Line)
        /// 
        /// Returns a Solid by revolving the closed curve about a given axis 
        /// line by 360 revolution.
        /// </summary>
        /// <param name="axis">Line curve as axis of revolution</param>
        /// <returns>Revolved Solid</returns>
        public Solid RevolveAsSolid(Line axis)
        {
            return RevolveAsSolid(axis, 0.0, 360.0);
        }

        /// <summary>
        /// def RevolveAsSurface : Surface (axisOrigin : Point, axisDirection : Vector, startAngle : double, sweepAngle : double)
        /// 
        /// Returns a Surface by revolving this curve about an axis defined by 
        /// axisOrigin point and axisDirection Vector. startAngle determines 
        /// where the curve starts to revolve, sweepAngle determines the 
        /// revolving angle.
        /// </summary>
        /// <param name="axisOrigin">Origin point of axis of revolution</param>
        /// <param name="axisDirection">Direction vector of axis of revolution</param>
        /// <param name="startAngle">Start angle in degrees</param>
        /// <param name="sweepAngle">Sweep angle for rotation in degrees</param>
        /// <returns>Revolved Surface</returns>
        public Surface RevolveAsSurface(Point axisOrigin, Vector axisDirection, double startAngle, double sweepAngle)
        {
            if (axisOrigin == null)
                throw new System.ArgumentNullException("axisOrigin");
            else if (axisDirection == null)
                throw new System.ArgumentNullException("axisDirection");
            else if (axisDirection.IsZeroVector())
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZeroVector, axisDirection), "axisDirection");
            else if (sweepAngle.EqualsTo(0.0))
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZeroAngle, "sweep"), "sweepAngle");

            return Surface.Revolve(this, axisOrigin, axisDirection, startAngle, sweepAngle);
        }

        /// <summary>
        /// def RevolveAsSurface(axisOrigin : Point, axisDirection : Vector)
        /// 
        /// Returns a Surface by revolving this curve about an axis defined by 
        /// axisOrigin point and axisDirection Vector. Assuming sweep angle = 360
        /// and start angle = 0.
        /// </summary>
        /// <param name="axisOrigin">Origin Point of axis of revolution</param>
        /// <param name="axisDirection">Direction Vector of axis of revolution</param>
        /// <returns>Revolved Surface</returns>
        public Surface RevolveAsSurface(Point axisOrigin, Vector axisDirection)
        {
            if (axisOrigin == null)
                throw new System.ArgumentNullException("axisOrigin");
            else if (axisDirection == null)
                throw new System.ArgumentNullException("axisDirection");
            else if (axisDirection.IsZeroVector())
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZeroVector, axisDirection), "axisDirection");

            return Surface.Revolve(this, axisOrigin, axisDirection);
        }

        /// <summary>
        /// def RevolveAsSurface : Surface (axis : Line, startAngle : double, sweepAngle : double)
        /// 
        /// Returns a Surface by revolving this curve about a line axis. 
        /// startAngle determines where the curve starts to revolve, sweepAngle
        /// determines the revolving angle.
        /// </summary>
        /// <param name="axis">Line curve as axis of revolution</param>
        /// <param name="startAngle">Start angle in degrees</param>
        /// <param name="sweepAngle">Sweep angle for rotation in degrees</param>
        /// <returns>Revolved Surface</returns>
        public Surface RevolveAsSurface(Line axis, double startAngle, double sweepAngle)
        {
            if (axis == null)
                throw new System.ArgumentNullException("axis");
            else if (axis.Length.EqualsTo(0.0))
                throw new System.ArgumentException(Properties.Resources.IsZeroLength, "axis");
            else if (sweepAngle.EqualsTo(0.0))
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZeroAngle, "sweep"), "sweepAngle");

            return Surface.Revolve(this, axis, startAngle, sweepAngle);
        }

        /// <summary>
        /// def RevolveAsSurface : Surface (axis : Line)
        /// 
        /// Returns a Surface by revolving this curve about a line axis. 
        /// Assuming sweep angle = 360 and start angle = 0.
        /// </summary>
        /// <param name="axis">Line curve as axis of revolution</param>
        /// <returns>Revolved Surface</returns>
        public Surface RevolveAsSurface(Line axis)
        {
            if (axis == null)
                throw new System.ArgumentNullException("axis");
            else if (axis.Length.EqualsTo(0.0))
                throw new System.ArgumentException(Properties.Resources.IsZeroLength, "axis");

            return Surface.Revolve(this, axis);
        }

        /// <summary>
        /// def SweepAsSurface : Surface (path : Curve)
        /// 
        /// Return a surface by sweeping this curve as profile on a given path 
        /// curve
        /// </summary>
        /// <param name="path">Path Curve for sweeping</param>
        /// <returns>Swept Surface</returns>
        public Surface SweepAsSurface(Curve path)
        {
            if (path == null)
                throw new System.ArgumentNullException("path");

            return Surface.Sweep(this, path);
        }

        /// <summary>
        /// def SweepAsSolid : Solid (path : Curve)
        /// 
        /// Return a solid by sweeping this curve as profile on a given path 
        /// curve. This curve must be closed.
        /// </summary>
        /// <param name="path">Path Curve for sweeping</param>
        /// <returns>Swept Solid</returns>
        public Solid SweepAsSolid(Curve path)
        {
            if (path == null)
                throw new System.ArgumentNullException("path");

            return Solid.Sweep(this, path);
        }

        /// <summary>
        /// def ExtrudeAsSurface: Surface (distance : double, direction : Vector)
        /// 
        /// Returns an extruded surface upon extruding a curve in a given direction 
        /// by an input distance. This surface may have multiple faces, if this 
        /// curve is not C1 continuous.
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public Surface ExtrudeAsSurface(double distance, Vector direction)
        {
            if (direction == null)
                throw new System.ArgumentNullException("direction");
            if (!IsPlanar)
                throw new System.InvalidOperationException(Properties.Resources.CurveNotPlanar);

            ISurfaceEntity extrude = CurveEntity.Extrude(direction.VectorEntity, distance);
            return extrude.ToSurf(true, this);
        }

        /// <summary>
        /// Creates a patch surface from this closed non-self intersecting 
        /// profile curve.
        /// </summary>
        /// <returns>Patch Surface</returns>
        public Surface CreatePatchSurface()
        {
            return Surface.CreateFromCurve(this);
        }

        /// <summary>
        /// Returns a point at a given parameter on the input curve.
        /// </summary>
        /// <param name="param">Parameter value.</param>
        /// <returns>Point (Non-persisted).</returns>
        public virtual Point PointAtParameter(double param)
        {
            IPointEntity point = CurveEntity.PointAtParameter(param);
            Point result = point.ToPoint(true, this);
            result.T = param;
            result.Distance = CurveEntity.DistanceAtParameter(param);
            return result;
        }


        /// <summary>
        /// Returns a point along a curve with a input distance from start point of the curve.
        /// </summary>
        /// <param name="distance">Distance value.</param>
        /// <returns>Point (Non-persisted).</returns>
        public virtual Point PointAtDistance(double distance)
        {
            double param = CurveEntity.ParameterAtDistance(distance);
            IPointEntity point = CurveEntity.PointAtParameter(param);
            Point result = point.ToPoint(true, this);
            result.Distance = distance;
            result.T = param;
            return result;
        }

        /// <summary>
        /// Returns the T value (parameter) of a point on the curve.
        /// </summary>
        /// <param name="point">Input Point for classification.</param>
        /// <returns>Parameter value.</returns>
        public virtual double ParameterAtPoint(Point point)
        {
            if (null == point)
                throw new System.ArgumentNullException("point");

            return CurveEntity.ParameterAtPoint(point.PointEntity);
        }

        /// <summary>
        /// Checks whether a Point lies on this curve
        /// </summary>
        /// <param name="point">The point to be checked</param>
        /// <returns>True if point is on the curve</returns>
        public bool IsPointOnCurve(Point point)
        {
            if (null == point)
                throw new System.ArgumentNullException("point");

            double param = ParameterAtPoint(point);
            return param >= 0.0 && param <= 1.0;
        }

        /// <summary>
        /// Returns the T value (parameter) of a point which has a input 
        /// distance from start point of the curve.
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        public virtual double ParameterAtDistance(double distance)
        {
            return CurveEntity.ParameterAtDistance(distance);
        }

        /// <summary>
        /// Returns the distance value of a point which has a input 
        /// parameter from start point of the curve.
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public virtual double DistanceAtParameter(double param)
        {
            return CurveEntity.DistanceAtParameter(param);
        }

        /// <summary>
        /// Returns distance along the curve from start point of the curve to 
        /// the input point on the curve.
        /// </summary>
        /// <param name="point">Input point for classification.</param>
        /// <returns>Distance value.</returns>
        public virtual double DistanceAtPoint(Point point)
        {
            var param = ParameterAtPoint(point);
            return CurveEntity.DistanceAtParameter(param);
        }

        /// <summary>
        /// Returns the tangent vector at the given point on the curve.
        /// </summary>
        /// <param name="point">Input point for classification.</param>
        /// <returns>Tangent Vector.</returns>
        public virtual Vector TangentAtPoint(Point point)
        {
            var param = ParameterAtPoint(point);
            return TangentAtParameter(param);
        }

        /// <summary>
        /// Returns the tangent vector at a point on a curve with given parameter.
        /// </summary>
        /// <param name="param">Input parameter value.</param>
        /// <returns>Tangent Vector.</returns>
        public virtual Vector TangentAtParameter(double param)
        {
            Vector result = new Vector(CurveEntity.TangentAtParameter(param));
            result.Distance = CurveEntity.DistanceAtParameter(param);
            result.T = param;
            return result;
        }

        /// <summary>
        /// Returns the tangent vector at a point which has a input distance 
        /// from start point of the curve.
        /// </summary>
        /// <param name="distance">Input distance value.</param>
        /// <returns>Tangent Vector.</returns>
        public virtual Vector TangentAtDistance(double distance)
        {
            var param = ParameterAtDistance(distance);
            Vector result = new Vector(CurveEntity.TangentAtParameter(param));
            result.Distance = distance;
            result.T = param;
            return result;
        }

        /// <summary>
        /// Returns a normal vector at a given point on the curve.
        /// </summary>
        /// <param name="point">Input Point for classification.</param>
        /// <returns>Normal Vector.</returns>
        public virtual Vector NormalAtPoint(Point point)
        {
            var param = ParameterAtPoint(point);
            return NormalAtParameter(param);
        }

        /// <summary>
        /// Returns a normal vector at a given parameter value on a curve.
        /// </summary>
        /// <param name="param">Input parameter value.</param>
        /// <returns>Normal Vector.</returns>
        public virtual Vector NormalAtParameter(double param)
        {
            Vector result = new Vector(CurveEntity.NormalAtParameter(param));
            result.Distance = CurveEntity.DistanceAtParameter(param);
            result.T = param;
            return result;
        }

        /// <summary>
        /// Returns the normal vector at a point which has a input distance 
        /// from start point of the curve.
        /// </summary>
        /// <param name="distance">Input distance value.</param>
        /// <returns>Normal Vector.</returns>
        public virtual Vector NormalAtDistance(double distance)
        {
            var param = ParameterAtDistance(distance);
            Vector result = new Vector(CurveEntity.NormalAtParameter(param));
            result.Distance = distance;
            result.T = param;
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        internal virtual Point Project(Point point)
        {
            if (null == point)
                throw new System.ArgumentNullException("point");

            IPointEntity closestPt = CurveEntity.GetClosestPoint(point.PointEntity);
            return closestPt.ToPoint(true, this);
        }

        /// <summary>
        /// Projects the given point along the given direction onto this curve
        /// </summary>
        /// <param name="point">Point for projection</param>
        /// <param name="direction">Direction for projection</param>
        /// <returns>Project point on curve</returns>
        internal Point Project(Point point, Vector direction)
        {
            string kMethodName = "Curve.Project";
            if (point == null)
                throw new System.ArgumentNullException("point");
            else if (direction == null)
                throw new System.ArgumentNullException("direction");
            else if (direction.IsZeroVector())
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZeroVector, direction), "direction");

            var projectedPt = CurveEntity.Project(point.PointEntity, direction.VectorEntity);
            if (projectedPt == null)
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));

            return projectedPt.Cast<IPointEntity>().Select(x => x.ToPoint(true, this)).FirstOrDefault();
        }

        /// <summary>
        /// Returns the projection of this curve on a given plane with plane 
        /// normal as direction
        /// </summary>
        /// <param name="contextPlane">Projection plane</param>
        /// <returns>Projected curve on the context plane</returns>
        public Curve Project(Plane contextPlane)
        {
            string kMethodName = "Curve.Project";
            if (null == contextPlane)
                throw new System.ArgumentNullException("contextPlane");

            var entities = CurveEntity.Project(contextPlane.PlaneEntity, contextPlane.Normal.VectorEntity);
            if (entities == null)
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));

            return entities.Cast<ICurveEntity>().Select(x => x.ToCurve(true, this)).FirstOrDefault();
        }

        /// <summary>
        /// Returns the projection of the curve on the plane with given direction
        /// Argument Requirement:
        /// direction.Length > 0
        /// </summary>
        /// <param name="contextPlane">Projection plane</param>
        /// <param name="direction">Projection direction</param>
        /// <returns>Projected curve on the context plane</returns>
        public Curve[] Project(Plane contextPlane, Vector direction)
        {
            string kMethodName = "Curve.Project";
            if (null == contextPlane)
                throw new System.ArgumentNullException("contextPlane");
            else if (null == direction)
                throw new System.ArgumentNullException("direction");
            if (direction.Length.EqualsTo(0.0))
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZero, "length of the direction vector"), "direction");
            if (direction.IsPerpendicular(contextPlane.Normal))
                return null;

            var entity = CurveEntity.Project(contextPlane.PlaneEntity, direction.VectorEntity);
            if (null == entity)
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));

            return entity.Select(x => (ICurveEntity) x).Select(x => x.ToNurbsCurve()).Select(x => new NurbsCurve(x, true) ).ToArray();
        }

        /// <summary>
        /// Returns the projection of the curve on the surface with given direction
        /// Argument Requirement:
        ///     direction.Length > 0
        /// </summary>
        /// <param name="surface"></param>
        /// <param name="direction"></param>
        /// <returns>Array of projected curves</returns>
        [AllowRankReduction]
        public Curve[] Project(Surface surface, Vector direction)
        {
            string kMethodName = "Curve.Project";
            if (null == surface)
                throw new System.ArgumentNullException("surface");
            else if (null == direction)
                throw new System.ArgumentNullException("direction");

            IGeometryEntity[] entities = CurveEntity.Project(surface.SurfaceEntity, direction.VectorEntity);
            if(null == entities)
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));

            return entities.ToArray<Curve, IGeometryEntity>(true);
        }

        /// <summary>
        /// Returns an array of curves by splitting the curve with parameter input
        /// </summary>
        /// <param name="splitParameters"></param>
        /// <returns>Array of split curves</returns>
        public Curve[] Split(double[] splitParameters)
        {
            string kMethodName = "Curve.Split";
            if (null == splitParameters)
                throw new System.ArgumentNullException("splitParameters");

            ICurveEntity[] entities = CurveEntity.ParameterSplit(splitParameters);
            if (null == entities)
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));

            return entities.ToArray<Curve, ICurveEntity>(true);
        }

        /// <summary>
        /// Returns curves by trimming away the curve segment between 2 input points
        /// </summary>
        /// <param name="firstPoint"></param>
        /// <param name="secondPoint"></param>
        /// <param name="discardBetweenPoints"></param>
        /// <returns></returns>
        [AllowRankReduction]
        public Curve[] Trim(Point firstPoint, Point secondPoint, bool discardBetweenPoints)
        {
            if (null == firstPoint)
                throw new System.ArgumentNullException("firstPoint");
            if (null == secondPoint)
                throw new System.ArgumentNullException("secondPoint");

            double startParam = CurveEntity.ParameterAtPoint(firstPoint.PointEntity);
            double endParam = CurveEntity.ParameterAtPoint(secondPoint.PointEntity);

            return Trim(startParam, endParam, discardBetweenPoints);
        }

        /// <summary>
        /// Trim the curve using input parameter, bDiscardBetweenParameters 
        /// controls whether trim away the curve segment between parameters 
        /// or outside parameters.
        /// </summary>
        /// <param name="startParameter"></param>
        /// <param name="endParameter"></param>
        /// <param name="discardBetweenParameters"></param>
        /// <returns></returns>
        [AllowRankReduction]
        public Curve[] Trim(double startParameter, double endParameter, bool discardBetweenParameters)
        {
            string kMethodName = "Curve.Trim";

            ICurveEntity[] entities;

            if (discardBetweenParameters)
            {
                entities = CurveEntity.ParameterTrimInterior(startParameter, endParameter); 
            }
            else
            {
                var entity = CurveEntity.ParameterTrim(startParameter, endParameter);
                if (null == entity)
                    throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed,
                        kMethodName));

                entities = new ICurveEntity[]{entity};
            }

            if (null == entities)
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed,
                    kMethodName));

            Hide(this);

            return entities.ToArray<Curve, ICurveEntity>(true);
            
        }

        /// <summary>
        /// Trim the curve using input parameter, bDeleteEvenSegments controls
        /// whether trim away the even curve segments or odd curve segments
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="deleteEvenSegments"></param>
        /// <returns></returns>
        public Curve[] Trim(double[] parameters, bool deleteEvenSegments)
        {
            string kMethodName = "Curve.Trim";
            if (null == parameters)
                throw new System.ArgumentNullException("parameters");

            ICurveEntity[] entities = CurveEntity.ParameterTrimSegments(parameters, deleteEvenSegments);
            if (null == entities)
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));

            Hide(this);

            return entities.ToArray<Curve, ICurveEntity>(true);
        }

        //PB: to be superceded by PolyCurve

        ///// <summary>
        ///// Returns the composite curve by joining a given set of input curves 
        ///// Argument Requirement:
        /////     no input curves can be closed
        ///// </summary>
        ///// <param name="contextCurves"></param>
        ///// <returns></returns>
        //public static Curve Composite(Curve[] contextCurves)
        //{
        //    string kMethodName = "Curve.Composite";
        //    if (null == contextCurves)
        //        throw new System.ArgumentNullException("contextCurves");

        //    ICurveEntity firstCurve;
        //    List<ICurveEntity> hostCurves = GetHostCurves(contextCurves, out firstCurve);
        //    if (hostCurves.Count == 0)
        //        return firstCurve.ToCurve(false, null);

        //    INurbsCurveEntity entity = firstCurve.JoinWith(hostCurves.ToArray());
        //    if (null == entity)
        //        throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));

        //    return entity.ToCurve(true, null) as Curve;
        //}

        ///// <summary>
        ///// Returns the composite curve by joining a given set of input curves within a given tolerance
        ///// Argument Requirement:
        /////     no input curves can be closed
        ///// </summary>
        ///// <param name="contextCurves"></param>
        ///// <param name="bridgeTolerance"></param>
        ///// <returns></returns>
        //[AllowRankReduction]
        //public static Curve[] Composite(Curve[] contextCurves, double bridgeTolerance)
        //{
        //    string kMethodName = "Curve.Composite";
        //    ICurveEntity firstCurve;
        //    List<ICurveEntity> hostCurves = GetHostCurves(contextCurves, out firstCurve);
        //    if (hostCurves.Count == 0)
        //        return new Curve[] { firstCurve.ToCurve(false, null) };

        //    ICurveEntity[] entities = firstCurve.JoinWith(hostCurves.ToArray(), bridgeTolerance);
        //    if (null == entities)
        //        throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));

        //    return entities.ToArray<Curve, ICurveEntity>(true);
        //}

        /// <summary>
        /// Return a closest point on another curve to the curve.
        /// </summary>
        /// <param name="other">Curve</param>
        /// <returns>Closest Point</returns>
        public Point ClosestPointTo(Curve other)
        {
            string kMethodName = "Curve.ClosestPointTo";
            if (null == other)
                throw new System.ArgumentNullException("other");

            IPointEntity closestPt = CurveEntity.GetClosestPoint(other.CurveEntity);
            if (null == closestPt)
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));

            return closestPt.ToPoint(true, this);
        }

        /// <summary>
        /// Returns a curve with reversed parameterization from input curve.
        /// </summary>
        /// <returns>Reversed Curve</returns>
        public Curve Reverse()
        {
            ICurveEntity curve = CurveEntity.Reverse();
            return curve.ToCurve(true, this);
        }

        /// <summary>
        /// Returns a parallel curve with a specific distance from the curve
        /// </summary>
        /// <param name="distance">Offset distance value</param>
        /// <returns>Offset curve</returns>
        public Curve Offset(double distance)
        {
            string kMethodName = "Curve.Offset";
            if (distance.EqualsTo(0.0))
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZero, "distance"), "distance");

            ICurveEntity curveEntity = CurveEntity.Offset(distance);
            if (null == curveEntity)
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));

            Curve curve = curveEntity.ToCurve(true, this);
            curve.Context = this;
            return curve;
        }

        /// <summary>
        /// Returns a uniformly scaled orthogonal coordinate system at the 
        /// given parameter on a curve. The X axis is the tangent at the point,
        /// The Y axis is the Normal at the point and the Z-axis is along the 
        /// binormal at the point
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public CoordinateSystem CoordinateSystemAtParameter(double t)
        {
            return CoordinateSystem.AtParameter(this, t);
        }

        /// <summary>
        /// Returns a uniformly scaled orthogonal coordinate system at the 
        /// given parameter on a curve. The X axis is the tangent at the point, 
        /// The Y axis is the Normal at the point and the Z-axis is along the 
        /// binormal at the point.  When Z axis is in the opposite direction of
        /// upVector, the coordinate system will be flipped so that the 
        /// resultant Z axis is in the same direction as the upVector. The 
        /// Y-axis will also be flipped so that it maintains the right-handed 
        /// coordinate system rule.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="upVector"></param>
        /// <returns></returns>
        public CoordinateSystem CoordinateSystemAtParameter(double t, Vector upVector)
        {
            return CoordinateSystem.AtParameter(this, t, upVector);
        }

        /// <summary>
        /// Returns a uniformly scaled orthogonal coordinate system at the 
        /// given distance on a curve. The X axis is the tangent at the point, 
        /// The Y axis is the Normal at the point.
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        public CoordinateSystem CoordinateSystemAtDistance(double distance)
        {
            double param = ParameterAtDistance(distance);
            return CoordinateSystemAtParameter(param);
        }

        /// <summary>
        /// Returns a uniformly scaled orthogonal coordinate system at the 
        /// given distance on a curve. The X axis is the tangent at the point, 
        /// The Y axis is the Normal at the point. When Z axis is in the 
        /// opposite direction of upVector, the coordinate system will be 
        /// flipped so that the resultant Z axis is in the same direction as 
        /// the upVector. The Y-axis will also be flipped so that it maintains
        /// the right-handed coordinate system rule.
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="upVector"></param>
        /// <returns></returns>
        public CoordinateSystem CoordinateSystemAtDistance(double distance, Vector upVector)
        {
            double param = ParameterAtDistance(distance);
            return CoordinateSystemAtParameter(param, upVector);
        }

        /// <summary>
        /// Returns an array of uniformly scaled orthogonal coordinate system 
        /// located on the curve with equal distance. The X axis is the tangent 
        /// at the point, The Y axis is the Normal at the point.
        /// Argument Requirement:
        ///     numberOfCoordinateSystems > 0
        /// </summary>
        /// <param name="numberOfCoordinateSystems"></param>
        /// <returns></returns>
        public CoordinateSystem[] CoordinateSystemsAtEqualArcLength(int numberOfCoordinateSystems)
        {
            if (numberOfCoordinateSystems < 1)
                throw new System.ArgumentException(string.Format(Properties.Resources.LessThan, "number of coordinateSystems", "one"), "numberOfCoordinateSystems");
            IPointEntity[] pts = PointsAtEqualArcLengthCore(numberOfCoordinateSystems);
            if (null == pts)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidArguments, "number of coordinateSystems"), "numberOfCoordinateSystems");

            CoordinateSystem[] css = new CoordinateSystem[pts.Length];
            int i = 0;
            foreach (var p in pts)
            {
                double param = CurveEntity.ParameterAtPoint(p);
                try
                {
                    css[i++] = CoordinateSystem.AtParameter(this, param);
                }
                catch
                {
                    //Proceed with next iteration.
                }
                p.Dispose();
            }

            return css;
        }

        /// <summary><para>
        /// Returns an array of uniformly scaled orthogonal coordinate system 
        /// located on the curve with equal distance. The X axis is the tangent 
        /// at the point, The Y axis is the Normal at the point. </para><para>
        /// When Z axis is in the opposite direction of upVector, the coordinate 
        /// system will be flipped so that the resultant Z axis is in the same 
        /// direction as the upVector. The Y-axis will also be flipped so that 
        /// it maintains the right-handed coordinate system rule.</para>
        /// <para> Argument Requirement:
        ///         numberOfPoints > 0 </para>
        /// </summary>
        /// <param name="numberOfCoordinateSystems"></param>
        /// <param name="upVector"></param>
        /// <returns></returns>
        public CoordinateSystem[] CoordinateSystemsAtEqualArcLength(int numberOfCoordinateSystems, Vector upVector)
        {
            if (numberOfCoordinateSystems < 1)
                throw new System.ArgumentException(string.Format(Properties.Resources.LessThan, "number of coordinateSystems", "one"), "numberOfCoordinateSystems");
            IPointEntity[] pts = PointsAtEqualArcLengthCore(numberOfCoordinateSystems);
            if (null == pts)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidArguments, "number of coordinateSystems"), "numberOfCoordinateSystems");

            CoordinateSystem[] css = new CoordinateSystem[pts.Length];
            int i = 0;
            foreach (var p in pts)
            {
                double param = CurveEntity.ParameterAtPoint(p);
                try
                {
                    css[i++] = CoordinateSystem.AtParameter(this, param, upVector);
                }
                catch
                {
                    //Proceed with next iteration.
                }
                p.Dispose();
            }

            return css;
        }

        /// <summary>
        /// Returns a plane at a point on the curve by given paramater and use 
        /// the tangent at the point as Normal of the plane
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public Plane PlaneAtParameter(double param)
        {
            Plane result = PlaneAtParameter(param, Plane.kDefaultSize);
            result.Distance = CurveEntity.DistanceAtParameter(param);
            result.T = param;
            return result;
        }

        /// <summary>
        /// Returns a plane at a point on the curve by given paramater and use 
        /// the tangent at the point as Normal of the plane. This method can 
        /// specify the size of the plane.
        /// Argument Requirement:
        ///     planeSize >= 1
        /// </summary>
        /// <param name="param"></param>
        /// <param name="planeSize"></param>
        /// <returns></returns>
        public Plane PlaneAtParameter(double param, double planeSize)
        {
            var origin = CurveEntity.PointAtParameter(param).ToPoint(false, null);
            var tangent = TangentAtParameter(param);
            Plane result = new Plane(origin, tangent, planeSize, true, this);
            result.T = param;
            result.Distance = CurveEntity.DistanceAtParameter(param);
            return result;
        }

        /// <summary>
        /// Returns a plane at a point on the curve by given distance and use 
        /// the tangent at the point as Normal of the plane
        /// Argument Requirement:
        ///     distance >= 0.0
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        public Plane PlaneAtDistance(double distance)
        {
            return PlaneAtDistance(distance, Plane.kDefaultSize);
        }

        /// <summary><para>
        /// Returns a plane at a point on the curve by given distance and use 
        /// the tangent at the point as Normal of the plane. This method can 
        /// specify the size of the plane. </para>
        /// <para>Argument Requirement:
        ///     distance >= 0.0
        ///     planeSize >= 1.0
        /// </para></summary>
        /// <param name="distance"></param>
        /// <param name="planeSize"></param>
        /// <returns></returns>
        public Plane PlaneAtDistance(double distance, double planeSize)
        {
            if (distance < 0.0)
                throw new System.ArgumentException(string.Format(Properties.Resources.LessThanZero, "distance"), "distance");

            double param = ParameterAtDistance(distance);
            Plane result = PlaneAtParameter(param, planeSize);
            result.T = param;
            result.Distance = distance;
            return result;
        }

        /// <summary><para>
        /// Returns an array of planes located on the curve with equal distance. 
        /// The tangents at the points are the normals of the planes. </para>
        /// <para> Argument Requirement:
        ///             numberOfPoints > 0
        /// </para>
        /// </summary>
        /// <param name="numberOfPlanes"></param>
        /// <returns></returns>
        public Plane[] PlanesAtEqualArcLength(int numberOfPlanes)
        {
            if (numberOfPlanes < 1)
                throw new System.ArgumentException(string.Format(Properties.Resources.LessThan, "number of planes", "one"), "numberOfPlanes");

            IPointEntity[] pts = PointsAtEqualArcLengthCore(numberOfPlanes);
            if (null == pts)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidArguments, "number of planes"), "numberOfPlanes");

            Plane[] plns = new Plane[pts.Length];
            int i = 0;
            foreach (var p in pts)
            {
                double param = CurveEntity.ParameterAtPoint(p);
                try
                {
                    plns[i++] = PlaneAtParameter(param);
                }
                catch
                {
                    //Proceed with next iteration.
                }
                p.Dispose();
            }

            return plns;
        }

        /// <summary>
        /// Returns an array of points located on the curve with equal distance.
        /// Argument Requirement:
        ///     numberOfPoints > 0
        /// </summary>
        /// <param name="numberOfPoints"></param>
        /// <returns>Returns array of points.</returns>
        public Point[] PointsAtEqualArcLength(int numberOfPoints)
        {
            IPointEntity[] pts = PointsAtEqualArcLengthCore(numberOfPoints);
            List<Point> points = new List<Point>();
            foreach (IPointEntity pt in pts)
            {
                Point pnt = pt.ToPoint(true, this);
                pnt.Distance = DistanceAtPoint(pnt);
                pnt.T = ParameterAtPoint(pnt);
                points.Add(pnt);
            }

            return points.ToArray();
        }

        #endregion

        #region INTERNAL_METHODS

        internal CoordinateSystem CoordinateSystemAtParameterCore(double t, Vector upVector)
        {
            var origin = CurveEntity.PointAtParameter(t).ToPoint(false, null);
            var tangent = TangentAtParameter(t);
            var normal = NormalAtParameter(t);
            if (normal.IsZeroVector() || tangent.IsZeroVector())
            {
                //  degenerate case
                throw new System.InvalidOperationException(string.Format(Properties.Resources.IsZeroVector, "normal or tangent on curve"));
            }

            if (null != upVector)
            {
                var binormal = tangent.Cross(normal);
                var dp = binormal.Dot(upVector);
                var binormalx = dp < 0.0 ? binormal.Scale(-1.0).Normalize() : binormal.Normalize();
                var normalx = binormalx.Cross(tangent.Normalize());

                return CoordinateSystem.ByOriginVectors(origin, tangent, normalx, binormalx, true);
            }

            return CoordinateSystem.ByOriginVectors(origin, tangent, normal);
        }

        internal IPointEntity[] PointsAtEqualArcLengthCore(int numberOfPoints)
        {
            if (numberOfPoints < 1)
                throw new System.ArgumentException(string.Format(Properties.Resources.LessThan, "number of points", "one"), "numberOfPoints");

            double length = Length;
            if (numberOfPoints == 1 || length.EqualsTo(0.0))
                return new[] { StartPoint.PointEntity.Clone() as IPointEntity };
            bool isClosed = IsClosed;
            if (numberOfPoints == 2 && !isClosed)
                return new[] { StartPoint.PointEntity.Clone() as IPointEntity, EndPoint.PointEntity.Clone() as IPointEntity };

            double steps = (isClosed ? length / numberOfPoints : length / (numberOfPoints - 1));
            IPointEntity[] pts = new IPointEntity[numberOfPoints];

            for (int i = 0; i < numberOfPoints; ++i)
            {
                double dist = i * steps;
                pts[i] = CurveEntity.PointAtDistance(dist);
            }
            return pts;
        }

        internal override sealed bool TessellateCore(IRenderPackage package)
        {
            if (base.TessellateCore(package))
                return true;

            Color c = (this.Color == null) ? Color.Yellow : this.Color;
            int nMaxSamples = 16;
            IPointEntity[] samples = GetSamplePoints(nMaxSamples);

            foreach (var item in samples)
            {
                package.PushLineStripVertex(item.X, item.Y, item.Z);
                package.PushLineStripVertexColor(c.RedValue, c.GreenValue, c.BlueValue, c.AlphaValue);
            }

            int nSamples = samples.Length;
            if (this.IsClosed)
            {
                package.PushLineStripVertex(samples[0].X, samples[0].Y, samples[0].Z);
                package.PushLineStripVertexColor(c.RedValue, c.GreenValue, c.BlueValue, c.AlphaValue);
                ++nSamples;
            }
            package.PushLineStripVertexCount(nSamples);
            return true;
        }

        internal virtual IPointEntity[] GetSamplePoints(int maxSamples)
        {
            return PointsAtEqualArcLengthCore(maxSamples);
        }
        #endregion

        #region CONSTRUCTORS

        internal Curve(ICurveEntity host, bool persist) : base(host, persist)
        {
        }

        /// <summary>
        /// Copy contructor as well as a mechanism for subclassing from external
        /// libraries.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="persist"></param>
        protected Curve(Curve curve, bool persist) : base(curve.CurveEntity.Clone(), persist)
        {
        }

        #endregion

        #region PROTECTED_METHODS
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GeometryExtension.DisposeObject(ref mStartPoint);
                GeometryExtension.DisposeObject(ref mEndPoint);
            }
            base.Dispose(disposing);
        }

        internal override IGeometryEntity[] IntersectWithCurve(Curve curve)
        {
            return CurveEntity.Intersect(curve.CurveEntity);
        }

        internal override IGeometryEntity[] IntersectWithPlane(Plane plane)
        {
            return CurveEntity.Intersect(plane.PlaneEntity);
        }

        internal override IGeometryEntity[] IntersectWithSurface(Surface surf)
        {
            return CurveEntity.Intersect(surf.SurfaceEntity);
        }

        internal override IGeometryEntity[] IntersectWithSolid(Solid solid)
        {
            return CurveEntity.Intersect(solid.SolidEntity);
        }

        internal override IPointEntity ClosestPointTo(IPointEntity otherPoint)
        {
            return CurveEntity.GetClosestPoint(otherPoint);
        }

        internal override IGeometryEntity[] ProjectOn(Geometry other, Vector direction)
        {
            var dir = direction.VectorEntity;
            return CurveEntity.Project(other.GeomEntity, dir);
        }

        internal static List<ICurveEntity> GetHostCurves(Curve[] contextCurves, out ICurveEntity firstCurve)
        {
            firstCurve = null;
            var hostCurves = new List<ICurveEntity>();
            foreach (var curve in contextCurves)
            {
                if (null == curve)
                    continue;
                if (null == firstCurve)
                    firstCurve = curve.CurveEntity;
                else
                    hostCurves.Add(curve.CurveEntity);
            }
            if (null == firstCurve)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidArguments, "contextCurves"), "contextCurves");

            return hostCurves;
        }

        #endregion
    }
}
