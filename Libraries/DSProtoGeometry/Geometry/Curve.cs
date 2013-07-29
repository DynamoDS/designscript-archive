using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

namespace Autodesk.DesignScript.Geometry
{
    public abstract class DSCurve : DSGeometry
    {
        #region DATA MEMBERS
        private DSPoint mStartPoint = null;
        private DSPoint mEndPoint = null;
        #endregion

        #region CURVE_PROPERTIES

        protected abstract DSPoint GetStartPoint();
        protected abstract DSPoint GetEndPoint();

        /// <summary>
        /// 
        /// </summary>
        /// 
        [Category("Primary")]
        public DSPoint StartPoint 
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
        public DSPoint EndPoint 
        {
            get
            {
                if(null == mEndPoint)
                    mEndPoint = GetEndPoint();
                return mEndPoint;
            }
        }

        public DSCurve ContextCurve
        {
            get { return Context as DSCurve; }
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual double Length 
        {
            get
            {
                return CurveEntity.GetLength();
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
            bool paramchange = DSGeometryExtension.ClipParamRange(ref param);
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
        public DSSolid RevolveAsSolid(DSPoint axisOrigin, DSVector axisDirection, double startAngle, double sweepAngle)
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

            return DSSolid.Revolve(this, axisOrigin, axisDirection, startAngle, sweepAngle);
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
        public DSSolid RevolveAsSolid(DSPoint axisOrigin, DSVector axisDirection)
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

            return DSSolid.Revolve(this, axisOrigin, axisDirection);
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
        public DSSolid RevolveAsSolid(DSLine axis, double startAngle, double sweepAngle)
        {
            if (!IsClosed)
                throw new System.InvalidOperationException(Properties.Resources.CurveNotClosed);
            else if (!IsPlanar)
                throw new System.InvalidOperationException(Properties.Resources.CurveNotPlanar);
            else if (axis == null)
                throw new System.ArgumentNullException("axis");
            else if (axis.Length.EqualsTo(0.0))
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZeroLength, "axis length"), "axis");

            return DSSolid.Revolve(this, axis, startAngle, sweepAngle);
        }

        /// <summary>
        /// def RevolveAsSolid : Solid (axis : Line)
        /// 
        /// Returns a Solid by revolving the closed curve about a given axis 
        /// line by 360 revolution.
        /// </summary>
        /// <param name="axis">Line curve as axis of revolution</param>
        /// <returns>Revolved Solid</returns>
        public DSSolid RevolveAsSolid(DSLine axis)
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
        public DSSurface RevolveAsSurface(DSPoint axisOrigin, DSVector axisDirection, double startAngle, double sweepAngle)
        {
            if (axisOrigin == null)
                throw new System.ArgumentNullException("axisOrigin");
            else if (axisDirection == null)
                throw new System.ArgumentNullException("axisDirection");
            else if (axisDirection.IsZeroVector())
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZeroVector, axisDirection), "axisDirection");
            else if (sweepAngle.EqualsTo(0.0))
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZeroAngle, "sweep"), "sweepAngle");

            return DSSurface.Revolve(this, axisOrigin, axisDirection, startAngle, sweepAngle);
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
        public DSSurface RevolveAsSurface(DSPoint axisOrigin, DSVector axisDirection)
        {
            if (axisOrigin == null)
                throw new System.ArgumentNullException("axisOrigin");
            else if (axisDirection == null)
                throw new System.ArgumentNullException("axisDirection");
            else if (axisDirection.IsZeroVector())
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZeroVector, axisDirection), "axisDirection");

            return DSSurface.Revolve(this, axisOrigin, axisDirection);
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
        public DSSurface RevolveAsSurface(DSLine axis, double startAngle, double sweepAngle)
        {
            if (axis == null)
                throw new System.ArgumentNullException("axis");
            else if (axis.Length.EqualsTo(0.0))
                throw new System.ArgumentException(Properties.Resources.IsZeroLength, "axis");
            else if (sweepAngle.EqualsTo(0.0))
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZeroAngle, "sweep"), "sweepAngle");

            return DSSurface.Revolve(this, axis, startAngle, sweepAngle);
        }

        /// <summary>
        /// def RevolveAsSurface : Surface (axis : Line)
        /// 
        /// Returns a Surface by revolving this curve about a line axis. 
        /// Assuming sweep angle = 360 and start angle = 0.
        /// </summary>
        /// <param name="axis">Line curve as axis of revolution</param>
        /// <returns>Revolved Surface</returns>
        public DSSurface RevolveAsSurface(DSLine axis)
        {
            if (axis == null)
                throw new System.ArgumentNullException("axis");
            else if (axis.Length.EqualsTo(0.0))
                throw new System.ArgumentException(Properties.Resources.IsZeroLength, "axis");

            return DSSurface.Revolve(this, axis);
        }

        /// <summary>
        /// def SweepAsSurface : Surface (path : Curve)
        /// 
        /// Return a surface by sweeping this curve as profile on a given path 
        /// curve
        /// </summary>
        /// <param name="path">Path Curve for sweeping</param>
        /// <returns>Swept Surface</returns>
        public DSSurface SweepAsSurface(DSCurve path)
        {
            if (path == null)
                throw new System.ArgumentNullException("path");

            return DSSurface.Sweep(this, path);
        }

        /// <summary>
        /// def SweepAsSolid : Solid (path : Curve)
        /// 
        /// Return a solid by sweeping this curve as profile on a given path 
        /// curve. This curve must be closed.
        /// </summary>
        /// <param name="path">Path Curve for sweeping</param>
        /// <returns>Swept Solid</returns>
        public DSSolid SweepAsSolid(DSCurve path)
        {
            if (path == null)
                throw new System.ArgumentNullException("path");

            return DSSolid.Sweep(this, path);
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
        public DSSurface ExtrudeAsSurface(double distance, DSVector direction)
        {
            if (direction == null)
                throw new System.ArgumentNullException("direction");
            if (!IsPlanar)
                throw new System.InvalidOperationException(Properties.Resources.CurveNotPlanar);

            ISurfaceEntity extrude = CurveEntity.Extrude(direction.IVector, distance);
            return extrude.ToSurf(true, this);
        }

        /// <summary>
        /// Creates a patch surface from this closed non-self intersecting 
        /// profile curve.
        /// </summary>
        /// <returns>Patch Surface</returns>
        public DSSurface CreatePatchSurface()
        {
            return DSSurface.CreateFromCurve(this);
        }

        /// <summary>
        /// Returns a point at a given parameter on the input curve.
        /// </summary>
        /// <param name="param">Parameter value.</param>
        /// <returns>Point (Non-persisted).</returns>
        public virtual DSPoint PointAtParameter(double param)
        {
            IPointEntity point = CurveEntity.PointAtParameter(param);
            DSPoint result = point.ToPoint(true, this);
            result.T = param;
            result.Distance = CurveEntity.DistanceAtParameter(param);
            return result;
        }


        /// <summary>
        /// Returns a point along a curve with a input distance from start point of the curve.
        /// </summary>
        /// <param name="distance">Distance value.</param>
        /// <returns>Point (Non-persisted).</returns>
        public virtual DSPoint PointAtDistance(double distance)
        {
            double param = CurveEntity.ParameterAtDistance(distance);
            IPointEntity point = CurveEntity.PointAtParameter(param);
            DSPoint result = point.ToPoint(true, this);
            result.Distance = distance;
            result.T = param;
            return result;
        }

        /// <summary>
        /// Returns the T value (parameter) of a point on the curve.
        /// </summary>
        /// <param name="point">Input Point for classification.</param>
        /// <returns>Parameter value.</returns>
        public virtual double ParameterAtPoint(DSPoint point)
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
        public bool IsPointOnCurve(DSPoint point)
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
        public virtual double DistanceAtPoint(DSPoint point)
        {
            var param = ParameterAtPoint(point);
            return CurveEntity.DistanceAtParameter(param);
        }

        /// <summary>
        /// Returns the tangent vector at the given point on the curve.
        /// </summary>
        /// <param name="point">Input point for classification.</param>
        /// <returns>Tangent Vector.</returns>
        public virtual DSVector TangentAtPoint(DSPoint point)
        {
            var param = ParameterAtPoint(point);
            return TangentAtParameter(param);
        }

        /// <summary>
        /// Returns the tangent vector at a point on a curve with given parameter.
        /// </summary>
        /// <param name="param">Input parameter value.</param>
        /// <returns>Tangent Vector.</returns>
        public virtual DSVector TangentAtParameter(double param)
        {
            DSVector result = new DSVector(CurveEntity.TangentAtParameter(param));
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
        public virtual DSVector TangentAtDistance(double distance)
        {
            var param = ParameterAtDistance(distance);
            DSVector result = new DSVector(CurveEntity.TangentAtParameter(param));
            result.Distance = distance;
            result.T = param;
            return result;
        }

        /// <summary>
        /// Returns a normal vector at a given point on the curve.
        /// </summary>
        /// <param name="point">Input Point for classification.</param>
        /// <returns>Normal Vector.</returns>
        public virtual DSVector NormalAtPoint(DSPoint point)
        {
            var param = ParameterAtPoint(point);
            return NormalAtParameter(param);
        }

        /// <summary>
        /// Returns a normal vector at a given parameter value on a curve.
        /// </summary>
        /// <param name="param">Input parameter value.</param>
        /// <returns>Normal Vector.</returns>
        public virtual DSVector NormalAtParameter(double param)
        {
            DSVector result = new DSVector(CurveEntity.NormalAtParameter(param));
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
        public virtual DSVector NormalAtDistance(double distance)
        {
            var param = ParameterAtDistance(distance);
            DSVector result = new DSVector(CurveEntity.NormalAtParameter(param));
            result.Distance = distance;
            result.T = param;
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        internal virtual DSPoint Project(DSPoint point)
        {
            if (null == point)
                throw new System.ArgumentNullException("point");

            IPointEntity closestPt = CurveEntity.GetClosestPointTo(point.PointEntity, false);
            return closestPt.ToPoint(true, this);
        }

        /// <summary>
        /// Projects the given point along the given direction onto this curve
        /// </summary>
        /// <param name="point">Point for projection</param>
        /// <param name="direction">Direction for projection</param>
        /// <returns>Project point on curve</returns>
        internal DSPoint Project(DSPoint point, DSVector direction)
        {
            string kMethodName = "DSCurve.Project";
            if (point == null)
                throw new System.ArgumentNullException("point");
            else if (direction == null)
                throw new System.ArgumentNullException("direction");
            else if (direction.IsZeroVector())
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZeroVector, direction), "direction");

            IPointEntity projectedPt = CurveEntity.Project(point.PointEntity, direction.IVector);
            if (projectedPt == null)
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));

            return projectedPt.ToPoint(true, this);
        }

        /// <summary>
        /// Returns the projection of this curve on a given plane with plane 
        /// normal as direction
        /// </summary>
        /// <param name="contextPlane">Projection plane</param>
        /// <returns>Projected curve on the context plane</returns>
        public DSCurve Project(DSPlane contextPlane)
        {
            string kMethodName = "DSCurve.Project";
            if (null == contextPlane)
                throw new System.ArgumentNullException("contextPlane");

            ICurveEntity entity = CurveEntity.ProjectOn(contextPlane.PlaneEntity, contextPlane.Normal.IVector);
            if (entity == null)
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));

            return entity.ToCurve(true, this);
        }

        /// <summary>
        /// Returns the projection of the curve on the plane with given direction
        /// Argument Requirement:
        /// direction.Length > 0
        /// </summary>
        /// <param name="contextPlane">Projection plane</param>
        /// <param name="direction">Projection direction</param>
        /// <returns>Projected curve on the context plane</returns>
        public DSCurve Project(DSPlane contextPlane, DSVector direction)
        {
            string kMethodName = "DSCurve.Project";
            if (null == contextPlane)
                throw new System.ArgumentNullException("contextPlane");
            else if (null == direction)
                throw new System.ArgumentNullException("direction");
            if (direction.Length.EqualsTo(0.0))
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZero, "length of the direction vector"), "direction");
            if (direction.IsPerpendicular(contextPlane.Normal))
                return null;

            ICurveEntity entity = CurveEntity.ProjectOn(contextPlane.PlaneEntity, direction.IVector);
            if (null == entity)
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));

            return entity.ToCurve(true, this);
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
        public DSCurve[] Project(DSSurface surface, DSVector direction)
        {
            string kMethodName = "DSCurve.Project";
            if (null == surface)
                throw new System.ArgumentNullException("surface");
            else if (null == direction)
                throw new System.ArgumentNullException("direction");

            IGeometryEntity[] entities = CurveEntity.ProjectOn(surface.SurfaceEntity, direction.IVector);
            if(null == entities)
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));

            return entities.ToArray<DSCurve, IGeometryEntity>(true);
        }

        /// <summary>
        /// Returns an array of curves by splitting the curve with parameter input
        /// </summary>
        /// <param name="splitParameters"></param>
        /// <returns>Array of split curves</returns>
        public DSCurve[] Split(double[] splitParameters)
        {
            string kMethodName = "DSCurve.Split";
            if (null == splitParameters)
                throw new System.ArgumentNullException("splitParameters");

            ICurveEntity[] entities = CurveEntity.Split(splitParameters);
            if (null == entities)
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));

            return entities.ToArray<DSCurve, ICurveEntity>(true);
        }

        /// <summary>
        /// Returns curves by trimming away the curve segment between 2 input points
        /// </summary>
        /// <param name="firstPoint"></param>
        /// <param name="secondPoint"></param>
        /// <param name="discardBetweenPoints"></param>
        /// <returns></returns>
        [AllowRankReduction]
        public DSCurve[] Trim(DSPoint firstPoint, DSPoint secondPoint, bool discardBetweenPoints)
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
        public DSCurve[] Trim(double startParameter, double endParameter, bool discardBetweenParameters)
        {
            string kMethodName = "DSCurve.Trim";
            ICurveEntity[] entities = CurveEntity.Trim(startParameter, endParameter, discardBetweenParameters);
            if (null == entities)
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));

            Hide(this);

            return entities.ToArray<DSCurve, ICurveEntity>(true);
        }

        /// <summary>
        /// Trim the curve using input parameter, bDeleteEvenSegments controls
        /// whether trim away the even curve segments or odd curve segments
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="deleteEvenSegments"></param>
        /// <returns></returns>
        public DSCurve[] Trim(double[] parameters, bool deleteEvenSegments)
        {
            string kMethodName = "DSCurve.Trim";
            if (null == parameters)
                throw new System.ArgumentNullException("parameters");

            ICurveEntity[] entities = CurveEntity.Trim(parameters, deleteEvenSegments);
            if (null == entities)
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));

            Hide(this);

            return entities.ToArray<DSCurve, ICurveEntity>(true);
        }

        /// <summary>
        /// Returns the composite curve by joining a given set of input curves 
        /// Argument Requirement:
        ///     no input curves can be closed
        /// </summary>
        /// <param name="contextCurves"></param>
        /// <returns></returns>
        public static DSCurve Composite(DSCurve[] contextCurves)
        {
            string kMethodName = "DSCurve.Composite";
            if (null == contextCurves)
                throw new System.ArgumentNullException("contextCurves");

            ICurveEntity firstCurve;
            List<ICurveEntity> hostCurves = GetHostCurves(contextCurves, out firstCurve);
            if (hostCurves.Count == 0)
                return firstCurve.ToCurve(false, null);

            IBSplineCurveEntity entity = firstCurve.JoinWith(hostCurves.ToArray());
            if (null == entity)
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));

            return entity.ToCurve(true, null) as DSCurve;
        }

        /// <summary>
        /// Returns the composite curve by joining a given set of input curves within a given tolerance
        /// Argument Requirement:
        ///     no input curves can be closed
        /// </summary>
        /// <param name="contextCurves"></param>
        /// <param name="bridgeTolerance"></param>
        /// <returns></returns>
        [AllowRankReduction]
        public static DSCurve[] Composite(DSCurve[] contextCurves, double bridgeTolerance)
        {
            string kMethodName = "DSCurve.Composite";
            ICurveEntity firstCurve;
            List<ICurveEntity> hostCurves = GetHostCurves(contextCurves, out firstCurve);
            if (hostCurves.Count == 0)
                return new DSCurve[] { firstCurve.ToCurve(false, null) };

            ICurveEntity[] entities = firstCurve.JoinWith(hostCurves.ToArray(), bridgeTolerance);
            if (null == entities)
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));

            return entities.ToArray<DSCurve, ICurveEntity>(true);
        }

        /// <summary>
        /// Return a closest point on another curve to the curve.
        /// </summary>
        /// <param name="other">Curve</param>
        /// <returns>Closest Point</returns>
        public DSPoint ClosestPointTo(DSCurve other)
        {
            string kMethodName = "DSCurve.ClosestPointTo";
            if (null == other)
                throw new System.ArgumentNullException("other");

            IPointEntity closestPt = CurveEntity.GetClosestPointTo(other.CurveEntity);
            if (null == closestPt)
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));

            return closestPt.ToPoint(true, this);
        }

        /// <summary>
        /// Returns a curve with reversed parameterization from input curve.
        /// </summary>
        /// <returns>Reversed Curve</returns>
        public DSCurve Reverse()
        {
            ICurveEntity curve = CurveEntity.Reverse();
            return curve.ToCurve(true, this);
        }

        /// <summary>
        /// Returns a parallel curve with a specific distance from the curve
        /// </summary>
        /// <param name="distance">Offset distance value</param>
        /// <returns>Offset curve</returns>
        public DSCurve Offset(double distance)
        {
            string kMethodName = "DSCurve.Offset";
            if (distance.EqualsTo(0.0))
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZero, "distance"), "distance");

            ICurveEntity curveEntity = CurveEntity.GetOffsetCurve(distance);
            if (null == curveEntity)
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));

            DSCurve curve = curveEntity.ToCurve(true, this);
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
        public DSCoordinateSystem CoordinateSystemAtParameter(double t)
        {
            return DSCoordinateSystem.AtParameter(this, t);
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
        public DSCoordinateSystem CoordinateSystemAtParameter(double t, DSVector upVector)
        {
            return DSCoordinateSystem.AtParameter(this, t, upVector);
        }

        /// <summary>
        /// Returns a uniformly scaled orthogonal coordinate system at the 
        /// given distance on a curve. The X axis is the tangent at the point, 
        /// The Y axis is the Normal at the point.
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        public DSCoordinateSystem CoordinateSystemAtDistance(double distance)
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
        public DSCoordinateSystem CoordinateSystemAtDistance(double distance, DSVector upVector)
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
        public DSCoordinateSystem[] CoordinateSystemsAtEqualArcLength(int numberOfCoordinateSystems)
        {
            if (numberOfCoordinateSystems < 1)
                throw new System.ArgumentException(string.Format(Properties.Resources.LessThan, "number of coordinateSystems", "one"), "numberOfCoordinateSystems");
            IPointEntity[] pts = PointsAtEqualArcLengthCore(numberOfCoordinateSystems);
            if (null == pts)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidArguments, "number of coordinateSystems"), "numberOfCoordinateSystems");

            DSCoordinateSystem[] css = new DSCoordinateSystem[pts.Length];
            int i = 0;
            foreach (var p in pts)
            {
                double param = CurveEntity.ParameterAtPoint(p);
                try
                {
                    css[i++] = DSCoordinateSystem.AtParameter(this, param);
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
        public DSCoordinateSystem[] CoordinateSystemsAtEqualArcLength(int numberOfCoordinateSystems, DSVector upVector)
        {
            if (numberOfCoordinateSystems < 1)
                throw new System.ArgumentException(string.Format(Properties.Resources.LessThan, "number of coordinateSystems", "one"), "numberOfCoordinateSystems");
            IPointEntity[] pts = PointsAtEqualArcLengthCore(numberOfCoordinateSystems);
            if (null == pts)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidArguments, "number of coordinateSystems"), "numberOfCoordinateSystems");

            DSCoordinateSystem[] css = new DSCoordinateSystem[pts.Length];
            int i = 0;
            foreach (var p in pts)
            {
                double param = CurveEntity.ParameterAtPoint(p);
                try
                {
                    css[i++] = DSCoordinateSystem.AtParameter(this, param, upVector);
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
        public DSPlane PlaneAtParameter(double param)
        {
            DSPlane result = PlaneAtParameter(param, DSPlane.kDefaultSize);
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
        public DSPlane PlaneAtParameter(double param, double planeSize)
        {
            var origin = CurveEntity.PointAtParameter(param).ToPoint(false, null);
            var tangent = TangentAtParameter(param);
            DSPlane result = new DSPlane(origin, tangent, planeSize, true, this);
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
        public DSPlane PlaneAtDistance(double distance)
        {
            return PlaneAtDistance(distance, DSPlane.kDefaultSize);
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
        public DSPlane PlaneAtDistance(double distance, double planeSize)
        {
            if (distance < 0.0)
                throw new System.ArgumentException(string.Format(Properties.Resources.LessThanZero, "distance"), "distance");

            double param = ParameterAtDistance(distance);
            DSPlane result = PlaneAtParameter(param, planeSize);
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
        public DSPlane[] PlanesAtEqualArcLength(int numberOfPlanes)
        {
            if (numberOfPlanes < 1)
                throw new System.ArgumentException(string.Format(Properties.Resources.LessThan, "number of planes", "one"), "numberOfPlanes");

            IPointEntity[] pts = PointsAtEqualArcLengthCore(numberOfPlanes);
            if (null == pts)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidArguments, "number of planes"), "numberOfPlanes");

            DSPlane[] plns = new DSPlane[pts.Length];
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
        public DSPoint[] PointsAtEqualArcLength(int numberOfPoints)
        {
            IPointEntity[] pts = PointsAtEqualArcLengthCore(numberOfPoints);
            List<DSPoint> points = new List<DSPoint>();
            foreach (IPointEntity pt in pts)
            {
                DSPoint pnt = pt.ToPoint(true, this);
                pnt.Distance = DistanceAtPoint(pnt);
                pnt.T = ParameterAtPoint(pnt);
                points.Add(pnt);
            }

            return points.ToArray();
        }

        #endregion

        #region INTERNAL_METHODS

        internal DSCoordinateSystem CoordinateSystemAtParameterCore(double t, DSVector upVector)
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

                return DSCoordinateSystem.ByOriginVectors(origin, tangent, normalx, binormalx, true);
            }

            return DSCoordinateSystem.ByOriginVectors(origin, tangent, normal);
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

            DSColor c = (this.Color == null) ? DSColor.Yellow : this.Color;
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

        internal DSCurve(ICurveEntity host, bool persist) : base(host, persist)
        {
        }

        /// <summary>
        /// Copy contructor as well as a mechanism for subclassing from external
        /// libraries.
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="persist"></param>
        protected DSCurve(DSCurve curve, bool persist) : base(curve.CurveEntity.Clone(), persist)
        {
        }

        #endregion

        #region PROTECTED_METHODS
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DSGeometryExtension.DisposeObject(ref mStartPoint);
                DSGeometryExtension.DisposeObject(ref mEndPoint);
            }
            base.Dispose(disposing);
        }

        internal override IGeometryEntity[] IntersectWithCurve(DSCurve curve)
        {
            return CurveEntity.IntersectWith(curve.CurveEntity);
        }

        internal override IGeometryEntity[] IntersectWithPlane(DSPlane plane)
        {
            return CurveEntity.IntersectWith(plane.PlaneEntity);
        }

        internal override IGeometryEntity[] IntersectWithSurface(DSSurface surf)
        {
            return CurveEntity.IntersectWith(surf.SurfaceEntity);
        }

        internal override IGeometryEntity[] IntersectWithSolid(DSSolid solid)
        {
            return CurveEntity.IntersectWith(solid.SolidEntity);
        }

        internal override IPointEntity ClosestPointTo(IPointEntity otherPoint)
        {
            return CurveEntity.GetClosestPointTo(otherPoint, false);
        }

        internal override IGeometryEntity[] ProjectOn(DSGeometry other, DSVector direction)
        {
            IVector dir = direction.IVector;
            //Only point can be projected on curve.
            DSSurface surf = other as DSSurface;
            if (null != surf)
                return CurveEntity.ProjectOn(surf.SurfaceEntity, dir);

            DSPlane plane = other as DSPlane;
            if (null != plane)
            {
                ICurveEntity curve = CurveEntity.ProjectOn(plane.PlaneEntity, dir);
                return new IGeometryEntity[] { curve };
            }

            DSSolid solid = other as DSSolid;
            if (null != solid)
                return solid.SolidEntity.Project(CurveEntity, dir);

            return base.ProjectOn(other, direction);
        }

        internal static List<ICurveEntity> GetHostCurves(DSCurve[] contextCurves, out ICurveEntity firstCurve)
        {
            firstCurve = null;
            List<ICurveEntity> hostCurves = new List<ICurveEntity>();
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
