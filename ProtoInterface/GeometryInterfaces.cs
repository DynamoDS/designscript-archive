using System;
using System.ComponentModel;

namespace Autodesk.DesignScript.Interfaces
{
    /// <summary>
    /// This interface represents internal data model for a design script 
    /// objects. The user facing design script object creates and owns
    /// a kernal specific data model that implements this or derived interface.
    /// </summary>
    [Browsable(false)]
    public interface IDesignScriptEntity : IDisposable
    {
        /// <summary>
        /// Gets user facing DesignScript object that owns this entity data.
        /// </summary>
        Object Owner { get; }

        /// <summary>
        /// Sets the owner of this entity for back reference.
        /// </summary>
        /// <param name="owner">Owner object</param>
        void SetOwner(Object owner);
    }

    /// <summary>
    /// Interface to represent Color data object
    /// </summary>
    [Browsable(false)]
    public interface IColor
    {
        /// <summary>
        /// 
        /// </summary>
        byte AlphaValue { get; }

        /// <summary>
        /// 
        /// </summary>
        byte RedValue { get; }

        /// <summary>
        /// 
        /// </summary>
        byte GreenValue { get; }

        /// <summary>
        /// 
        /// </summary>
        byte BlueValue { get; }
    }

    /// <summary>
    /// Interface to represent vector data object
    /// </summary>
    [Browsable(false)]
    public interface IVector
    {
        /// <summary>
        /// 
        /// </summary>
        double X { get; }

        /// <summary>
        /// 
        /// </summary>
        double Y { get; }

        /// <summary>
        /// 
        /// </summary>
        double Z { get; }
    }

    /// <summary>
    /// This Interface represents a displayable object and provides methods to
    /// change it's display specific properties such as Color, Visibility etc.
    /// </summary>
    [Browsable(false)]
    public interface IDisplayable : IDisposable
    {
        /// <summary> <para>
        /// This method hilights/dehilights the entity/geometry implementing 
        /// this interface. </para>
        /// </summary>
        /// <param name="visibility">flag to highlight/dehighlight</param>
        /// <returns>True if success.</returns>
        bool Highlight(bool visibility);

        /// <summary>
        /// This method sets color to entity/geometry implementing this interface.
        /// </summary>
        /// <param name="color">IColor value</param>
        IDisplayable SetColor(IColor color);

        /// <summary>
        /// Shows/Hides the object based on visible flag.
        /// </summary>
        /// <param name="visible">flag to show/hide</param>
        IDisplayable SetVisibility(bool visible);

        /// <summary>
        /// Returns true if the object is visible.
        /// </summary>
        bool Visible { get; set; }
        /// <summary>
        /// IColor property of the object. Returns the color of the object
        /// </summary>
        IColor Color { get; set; }
    }

    [Browsable(false)]
    public interface IPersistentObject : IDisposable
    {
        /// <summary>
        /// Erases the persisted object from database.
        /// </summary>
        /// <returns></returns>
        bool Erase();

        /// <summary>
        /// 
        /// </summary>
        IGeometryEntity Geometry { get; }

        /// <summary>
        /// 
        /// </summary>
        IDisplayable Display { get; }
    }

    [Browsable(false)]
    public interface IPersistenceManager
    {
        /// <summary>
        /// Makes the given entity persistent.
        /// </summary>
        /// <param name="entity">DesignScript entity that needs to be persisted
        /// in host application.</param>
        /// <returns>IPersistentObject</returns>
        IPersistentObject Persist(IDesignScriptEntity entity);

        /// <summary>
        /// Updates the view to show the most up-to-date geometries.
        /// </summary>
        void UpdateDisplay();

        /// <summary>
        /// Gets a persistent object using object's handle.
        /// </summary>
        /// <param name="handle">Handle to persistent object</param>
        /// <returns>IPersistentObject identified from input handle</returns>
        IPersistentObject GetPersistentObjectFromHandle(object handle);

        /// <summary>
        /// Get's Geometry factory that design script system is using.
        /// This property will be set by the system, once GeometryFactory 
        /// is instanciated.
        /// </summary>
        IGeometryFactory GeometryFactory { get; set; }

        /// <summary>
        /// Checks if the host application supports geometry selection or
        /// capture to import geometry from host application to designscript
        /// </summary>
        /// <returns>true/false</returns>
        bool SupportsGeometryCapture();

        /// <summary>
        /// Allows capture of geometry data interactively from host application 
        /// and get an array of IDesignScriptEntity based on selection.
        /// </summary>
        /// <returns>Collection of captured IDesignScriptEntity</returns>
        IDesignScriptEntity[] CaptureGeometry();

        /// <summary>
        /// Create a DesignScript host entity based on the persisted object in
        /// the application
        /// </summary>
        /// <param name="ptr"></param>
        /// <returns></returns>
        IPersistentObject FromObject(long ptr);
    }

    [Browsable(false)]
    public interface ICoordinateSystemEntity : IDesignScriptEntity
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ICoordinateSystemEntity Inverse();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        ICoordinateSystemEntity PostMultiplyBy(ICoordinateSystemEntity other);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        ICoordinateSystemEntity PreMultiplyBy(ICoordinateSystemEntity other);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rotationAngle"></param>
        /// <param name="axis"></param>
        /// <param name="origin"></param>
        /// <returns></returns>
        ICoordinateSystemEntity Rotation(double rotationAngle, IVector axis, IPointEntity origin);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scaleX"></param>
        /// <param name="scaleY"></param>
        /// <param name="scaleZ"></param>
        /// <returns></returns>
        ICoordinateSystemEntity Scale(double scaleX, double scaleY, double scaleZ);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="translationVector"></param>
        /// <returns></returns>
        ICoordinateSystemEntity Translate(IVector translationVector);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool IsSingular();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool IsScaledOrtho();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool IsUniscaledOrtho();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        double GetDeterminant();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        double[] GetScaleFactors();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        bool IsEqualTo(ICoordinateSystemEntity other);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="xAxis"></param>
        /// <param name="yAxis"></param>
        /// <param name="zAxis"></param>
        void Set(IPointEntity origin, IVector xAxis, IVector yAxis, IVector zAxis);

        /// <summary>
        /// Origin point of the coordinate system.
        /// </summary>
        IPointEntity Origin { get; }

        /// <summary>
        /// X-Axis IVector for the coordinate system.
        /// </summary>
        IVector XAxis { get; }

        /// <summary>
        /// Y-Axis IVector for the coordinate system.
        /// </summary>
        IVector YAxis { get; }

        /// <summary>
        /// Z-Axis IVector for the coordinate system.
        /// </summary>
        IVector ZAxis { get; }

        /// <summary>
        /// 
        /// </summary>
        IDisplayable Display { get; }
    }

    [Browsable(false)]
    public interface IGeometryData
    {
        IntPtr EntityPtr { get; }
        IntPtr BodyPtr { get; }
    }

    [Browsable(false)]
    public interface IGeometryEntity : IDesignScriptEntity
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IGeometryEntity Clone();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        IGeometryEntity CopyAndTranslate(IVector offset);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromCS"></param>
        /// <param name="toCS"></param>
        /// <returns></returns>
        IGeometryEntity CopyAndTransform(ICoordinateSystemEntity fromCS, ICoordinateSystemEntity toCS);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        double DistanceTo(IPointEntity point);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        IPointEntity GetClosestPoint(IPointEntity point);
    }

    [Browsable(false)]
    public interface IPointEntity : IGeometryEntity
    {
        /// <summary>
        /// 
        /// </summary>
        double X { get; }

        /// <summary>
        /// 
        /// </summary>
        double Y { get; }

        /// <summary>
        /// 
        /// </summary>
        double Z { get; }
    }

    [Browsable(false)]
    public interface ICurveEntity : IGeometryEntity
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        IVector TangentAtParameter(double param);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        IVector NormalAtParameter(double param);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        double DistanceAtParameter(double param);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        double ParameterAtDistance(double distance);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        double StartParameter();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        double EndParameter();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        double GetLength();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        IPointEntity PointAtParameter(double param);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        IPointEntity PointAtDistance(double distance);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        double ParameterAtPoint(IPointEntity point);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="extend"></param>
        /// <returns></returns>
        IPointEntity GetClosestPointTo(IPointEntity point, bool extend);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="direction"></param>
        /// <param name="extend"></param>
        /// <returns></returns>
        IPointEntity GetClosestPointTo(IPointEntity point, IVector direction, bool extend);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ICurveEntity Reverse();

        /// <summary>
        /// 
        /// </summary>
        bool IsPlanar { get; }

        /// <summary>
        /// 
        /// </summary>
        bool IsClosed { get; }

        /// <summary>
        /// 
        /// </summary>
        IPointEntity StartPoint { get; }

        /// <summary>
        /// 
        /// </summary>
        IPointEntity EndPoint { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iCurveEntity"></param>
        /// <returns></returns>
        IPointEntity GetClosestPointTo(ICurveEntity iCurveEntity);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        ICurveEntity GetOffsetCurve(double distance);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="solid"></param>
        /// <returns></returns>
        IGeometryEntity[] IntersectWith(ISolidEntity solid);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="surface"></param>
        /// <returns></returns>
        IGeometryEntity[] IntersectWith(ISurfaceEntity surface);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="plane"></param>
        /// <returns></returns>
        IGeometryEntity[] IntersectWith(IPlaneEntity plane);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="otherCurve"></param>
        /// <returns></returns>
        IPointEntity[] IntersectWith(ICurveEntity otherCurve);

        /// <summary>
        /// Projects a given point onto this curve in the given direction
        /// </summary>
        /// <param name="point">Point for projection</param>
        /// <param name="direction">Direction vector</param>
        /// <returns>Projected point</returns>
        IPointEntity Project(IPointEntity point, IVector direction);

        /// <summary>
        /// Projects this curve on the given plane.
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="direction"></param>
        /// <returns>Projected curve</returns>
        ICurveEntity ProjectOn(IPlaneEntity plane, IVector direction);

        /// <summary>
        /// Project this curve on the given surface and returns array of geometry.
        /// </summary>
        /// <param name="surface"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        IGeometryEntity[] ProjectOn(ISurfaceEntity surface, IVector direction);

        /// <summary>
        /// Trims this curve by given parameters and optionally discards 
        /// even segments
        /// </summary>
        /// <param name="parameters">array of parameters</param>
        /// <param name="discardEvenSegments">flag to discard even segments</param>
        /// <returns>array of trimmed curves</returns>
        ICurveEntity[] Trim(double[] parameters, bool discardEvenSegments);

        /// <summary>
        /// Trims this curve by given start and end parameters
        /// </summary>
        /// <param name="startParameter"></param>
        /// <param name="endParameter"></param>
        /// <param name="discardBetweenParams"></param>
        /// <returns>array of trimmed curves</returns>
        ICurveEntity[] Trim(double startParameter, double endParameter, bool discardBetweenParams);

        /// <summary>
        /// Splits this curve at given parameters
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns>array of split curves</returns>
        ICurveEntity[] Split(double[] parameters);

        /// <summary>
        /// Joins given set of curve with this curve to return a BSpline curve
        /// </summary>
        /// <param name="curves">array of curves to join with</param>
        /// <returns>BSpline curve</returns>
        IBSplineCurveEntity JoinWith(ICurveEntity[] curves);

        /// <summary>
        /// Joins given set of curve with this curve with a given tolerance
        /// </summary>
        /// <param name="curves">array of curves to join with</param>
        /// <param name="bridgeTolerance">tolerance</param>
        /// <returns>array of curves</returns>
        ICurveEntity[] JoinWith(ICurveEntity[] curves, double bridgeTolerance);

        /// <summary>
        /// Returns an extruded surface upon extruding a curve in a given direction 
        /// by an input distance. This surface may have multiple faces, if this 
        /// curve is not C1 continuous.
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        ISurfaceEntity Extrude(IVector direction, double distance);
    }

    [Browsable(false)]
    public interface ILineEntity : ICurveEntity
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        void UpdateEndPoints(IPointEntity start, IPointEntity end);
    }

    [Browsable(false)]
    public interface ICircleEntity : ICurveEntity
    {
        /// <summary>
        /// 
        /// </summary>
        IPointEntity CenterPoint { get; }

        /// <summary>
        /// 
        /// </summary>
        IVector Normal { get; }

        /// <summary>
        /// 
        /// </summary>
        double Radius { get; }
    }

    [Browsable(false)]
    public interface IArcEntity : ICurveEntity
    {
        /// <summary>
        /// 
        /// </summary>
        IPointEntity CenterPoint { get; }

        /// <summary>
        /// 
        /// </summary>
        double Radius { get; }

        /// <summary>
        /// Returns Start angle in Radians
        /// </summary>
        double StartAngle { get; }

        /// <summary>
        /// Returns Sweep angle in Radians
        /// </summary>
        double SweepAngle { get; }

        /// <summary>
        /// 
        /// </summary>
        IVector Normal { get; }
    }

    [Browsable(false)]
    public interface IBSplineCurveEntity : ICurveEntity
    {
        /// <summary>
        /// Returns list of control vertices.
        /// </summary>
        /// <returns></returns>
        IPointEntity[] GetControlVertices();

        /// <summary>
        /// Returns knot vector.
        /// </summary>
        /// <returns></returns>
        double[] GetKnots();

        /// <summary>
        /// Returns weight vector
        /// </summary>
        /// <returns></returns>
        double[] GetWeights();

        /// <summary>
        /// Returns degree
        /// </summary>
        /// <returns></returns>
        int GetDegree();

        /// <summary>
        /// Return true if the curve is periodic.
        /// </summary>
        /// <returns></returns>
        bool GetIsPeriodic();

        /// <summary>
        /// Returns true if the cuve is closed.
        /// </summary>
        /// <returns></returns>
        bool GetIsClosed();

        /// <summary>
        /// Returns true if the cuve is closed.
        /// </summary>
        /// <returns></returns>
        bool GetIsRational();
    }

    [Browsable(false)]
    public interface IBRepEntity
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IFaceEntity[] GetFaces();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEdgeEntity[] GetEdges();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IVertexEntity[] GetVertices();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        int GetVertexCount();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        int GetEdgeCount();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        int GetFaceCount();
    }

    [Browsable(false)]
    public interface ISurfaceEntity : IGeometryEntity
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="crossSections"></param>
        /// <returns></returns>
        bool UpdateSurfaceByLoftedCrossSections(ICurveEntity[] crossSections);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="crossSections"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        bool UpdateSurfaceByLoftedCrossSectionsPath(ICurveEntity[] crossSections, ICurveEntity path);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="crossSections"></param>
        /// <param name="guides"></param>
        /// <returns></returns>
        bool UpdateSurfaceByLoftFromCrossSectionsGuides(ICurveEntity[] crossSections, ICurveEntity[] guides);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        bool UpdateSweep(ICurveEntity profile, ICurveEntity path);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="origin"></param>
        /// <param name="revolveAxis"></param>
        /// <param name="startAngle"></param>
        /// <param name="sweepAngle"></param>
        /// <returns></returns>
        bool UpdateRevolve(ICurveEntity profile, IPointEntity origin, IVector revolveAxis, double startAngle, double sweepAngle);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="thickness"></param>
        /// <param name="bothSides"></param>
        /// <returns></returns>
        ISolidEntity Thicken(double thickness, bool bothSides);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        ISurfaceEntity Offset(double distance);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IBSplineSurfaceEntity GetBsplineSurface();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        IVector GetNormalAtPoint(IPointEntity point);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        IVector TangentAtUParameter(double u, double v);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        IVector TangentAtVParameter(double u, double v);

        /// <summary>
        /// The returned coordination system use xAxis, yAxis and zAxis to represent the uDir, vDir and normal.
        /// The length of xAxis, yAxis represents the curvatures.
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        ICoordinateSystemEntity CurvatureAtParameter(double u, double v);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        IPointEntity PointAtParameter(double u, double v);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        IVector NormalAtPoint(IPointEntity point);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="curves"></param>
        /// <param name="planes"></param>
        /// <param name="surfaces"></param>
        /// <param name="solids"></param>
        /// <param name="point"></param>
        /// <param name="bAutoExtend"></param>
        /// <returns></returns>
        ISurfaceEntity Trim(ICurveEntity[] curves, IPlaneEntity[] planes,
                            ISurfaceEntity[] surfaces, ISolidEntity[] solids,
                            IPointEntity point, bool bAutoExtend);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="plane"></param>
        /// <returns></returns>
        ISurfaceEntity[] Split(IPlaneEntity plane);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="negativeHalf"></param>
        /// <param name="positiveHalf"></param>
        void Split(IPlaneEntity plane, ref ISurfaceEntity[] negativeHalf, ref ISurfaceEntity[] positiveHalf);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="surface"></param>
        /// <returns></returns>
        ISurfaceEntity[] Split(ISurfaceEntity surface);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        IGeometryEntity[] SubtractFrom(IGeometryEntity geometry);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        Tuple<double, double> GetUVParameterAtPoint(IPointEntity point);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        double GetArea();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        double GetPerimeter();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool GetIsClosedInU();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool GetIsClosedInV();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool GetIsClosed();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        ISurfaceEntity GetOffsetSurface(double distance);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IBSplineSurfaceEntity[] ConvertToBSplineSurface();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IBSplineSurfaceEntity ApproxBSpline(double tol);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="plane"></param>
        /// <returns></returns>
        IGeometryEntity[] IntersectWith(IPlaneEntity plane);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        IGeometryEntity[] IntersectWith(ISurfaceEntity geometry);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        IGeometryEntity[] Project(IGeometryEntity geometry, IVector direction);
    }

    [Browsable(false)]
    public interface IBSplineSurfaceEntity : ISurfaceEntity
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        int GetUDegree();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        int GetVDegree();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        int GetNumPolesAlongU();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        int GetNumPolesAlongV();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool GetIsPeriodicInU();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool GetIsPeriodicInV();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool GetIsRational();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IPointEntity[][] GetPoles();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IPointEntity[][] GetPoints();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        double[] GetUKnots();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        double[] GetVKnots();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        double[][] GetWeights();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        IVector NormalAtParameter(double u, double v);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isoDirection"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        ICurveEntity[] GetIsolines(int isoDirection, double parameter);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="poles"></param>
        /// <param name="uDegree"></param>
        /// <param name="vDegree"></param>
        /// <returns></returns>
        bool UpdateByPoles(IPointEntity[][] poles, int uDegree, int vDegree);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="points"></param>
        /// <param name="uDegree"></param>
        /// <param name="vDegree"></param>
        /// <returns></returns>
        bool UpdateByPoints(IPointEntity[][] points, int uDegree, int vDegree);
    }

    [Browsable(false)]
    public interface IPlaneEntity : IGeometryEntity
    {
        /// <summary>
        /// 
        /// </summary>
        IPointEntity Origin { get; }

        /// <summary>
        /// 
        /// </summary>
        IVector Normal { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ICoordinateSystemEntity GetCoordinateSystem();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="iPointEntity"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        IPointEntity Project(IPointEntity iPointEntity, IVector direction);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="perpPlane"></param>
        /// <returns></returns>
        ILineEntity IntersectWith(IPlaneEntity perpPlane);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="normal"></param>
        void Update(IPointEntity origin, IVector normal);
    }

    [Browsable(false)]
    public interface ISolidEntity : IGeometryEntity, IBRepEntity
    {
        #region GENERAL_SOLID_PROPERTIES
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        double GetArea();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        double GetVolume();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IPointEntity GetCentroid();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool IsNonManifold();

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        IGeometryEntity[] UnionWith(IGeometryEntity geometry);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        IGeometryEntity[] NonRegularUnionWith(IGeometryEntity geometry);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="solids"></param>
        /// <returns></returns>
        ISolidEntity NonRegularUnionWithMany(IGeometryEntity[] solids);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        ISolidEntity NonRegularImpose(ISolidEntity geometry);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        IGeometryEntity[] SubtractFrom(IGeometryEntity geometry);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        IGeometryEntity[] IntersectWith(ISolidEntity geometry);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        IGeometryEntity[] IntersectWith(IPlaneEntity geometry);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        IGeometryEntity[] Project(IGeometryEntity geometry, IVector direction);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        IGeometryEntity[] IntersectWith(ISurfaceEntity geometry);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="plane"></param>
        /// <returns></returns>
        IGeometryEntity[] SliceWithPlane(IPlaneEntity plane);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="planes"></param>
        /// <param name="surfaces"></param>
        /// <param name="solids"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        ISolidEntity Trim(IPlaneEntity[] planes, ISurfaceEntity[] surfaces,
                        ISolidEntity[] solids, IPointEntity point);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="plane"></param>
        /// <returns></returns>
        ISolidEntity NonRegularSliceWithPlane(IPlaneEntity plane);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="planes"></param>
        /// <returns></returns>
        ISolidEntity NonRegularSliceWithPlanes(IPlaneEntity[] planes);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="surface"></param>
        /// <returns></returns>
        IGeometryEntity[] SliceWithSurface(ISurfaceEntity surface);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="surfaces"></param>
        /// <returns></returns>
        IGeometryEntity[] SliceWithSurfaces(ISurfaceEntity[] surfaces);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="surface"></param>
        /// <returns></returns>
        ISolidEntity NonRegularSliceWithSurface(ISurfaceEntity surface);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="surfaces"></param>
        /// <returns></returns>
        ISolidEntity NonRegularSliceWithSurfaces(ISurfaceEntity[] surfaces);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="internalFaceThickness"></param>
        /// <param name="externalFaceThickness"></param>
        /// <returns></returns>
        ISolidEntity ThinShell(double internalFaceThickness, double externalFaceThickness);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="crossSections"></param>
        /// <param name="guides"></param>
        bool UpdateSolidByLoftFromCrossSectionsGuides(ICurveEntity[] crossSections, ICurveEntity[] guides);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="crossSections"></param>
        /// <param name="path"></param>
        bool UpdateSolidByLoftedCrossSectionsPath(ICurveEntity[] crossSections, ICurveEntity path);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="crossSections"></param>
        bool UpdateSolidByLoftedCrossSections(ICurveEntity[] crossSections);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="path"></param>
        bool UpdateSolidBySweep(ICurveEntity profile, ICurveEntity path);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profileCurve"></param>
        /// <param name="originPoint"></param>
        /// <param name="revolveAxis"></param>
        /// <param name="startAngle"></param>
        /// <param name="sweepAngle"></param>
        /// <returns></returns>
        bool UpdateSolidByRevolve(ICurveEntity profileCurve, IPointEntity originPoint, IVector revolveAxis, double startAngle, double sweepAngle);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IGeometryEntity[] SeparateSolid();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ISolidEntity Regularise();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IShellEntity[] GetShells();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ICellEntity[] GetCells();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="planeHosts"></param>
        /// <returns></returns>
        IGeometryEntity[] SliceWithPlanes(IPlaneEntity[] planeHosts);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        int GetCellCount();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        int GetShellCount();
    }

    [Browsable(false)]
    public interface IPrimitiveSolidEntity : ISolidEntity
    {
        /// <summary>
        /// Returns coordinate system of this primitive solid. This coordinate 
        /// system may be non-uniformly scaled or sheared.
        /// </summary>
        /// <returns>ICoordinateSystemEntity</returns>
        ICoordinateSystemEntity GetCoordinateSystem();
    }

    [Browsable(false)]
    public interface IConeEntity : IPrimitiveSolidEntity
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="startRadius"></param>
        /// <param name="endRadius"></param>
        void UpdateCone(IPointEntity startPoint, IPointEntity endPoint, double startRadius, double endRadius);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="startRadius"></param>
        /// <param name="endRadius"></param>
        /// <param name="length"></param>
        void UpdateCylinder(double[] data, double startRadius, double endRadius, double length);

        /// <summary>
        /// Returns start point of the cone in WCS
        /// </summary>
        /// <returns>IPointEntity</returns>
        IPointEntity GetStartPoint();

        /// <summary>
        /// Returns end point of the cone in WCS
        /// </summary>
        /// <returns></returns>
        IPointEntity GetEndPoint();

        /// <summary>
        /// Returns ratio of start and end radius of the cone
        /// </summary>
        /// <returns></returns>
        double GetRadiusRatio();

        /// <summary>
        /// Returns start radius of the cone, when it is multiplied by radius 
        /// ratio it gives end radius of the cone.
        /// </summary>
        /// <returns>Start radius</returns>
        double GetStartRadius();

        /// <summary>
        /// Returns height of the cone.
        /// </summary>
        /// <returns></returns>
        double GetHeight();
    }

    [Browsable(false)]
    public interface ICuboidEntity : IPrimitiveSolidEntity
    {
        /// <summary>
        /// Updates this cuboid with new transformation data and parameters.
        /// </summary>
        /// <param name="data">Transformation data</param>
        /// <param name="length">New length of cuboid</param>
        /// <param name="width">New width of cuboid</param>
        /// <param name="height">New height of cuboid</param>
        void UpdateCuboid(double[] data, double length, double width, double height);

        /// <summary>
        /// Returns length of the cuboid
        /// </summary>
        /// <returns></returns>
        double GetLength();

        /// <summary>
        /// Returns width of the cuboid
        /// </summary>
        /// <returns></returns>
        double GetWidth();

        /// <summary>
        /// Returns height of the cuboid
        /// </summary>
        /// <returns></returns>
        double GetHeight();
    }

    [Browsable(false)]
    public interface ISphereEntity : IPrimitiveSolidEntity
    {
        /// <summary>
        /// Updates this sphere with new center point and radius.
        /// </summary>
        /// <param name="centerPoint">New center point for update</param>
        /// <param name="radius">radius value for update</param>
        void UpdateSphere(IPointEntity centerPoint, double radius);

        /// <summary>
        /// Returns center point of the sphere
        /// </summary>
        /// <returns></returns>
        IPointEntity GetCenterPoint();

        /// <summary>
        /// Returns radius of the sphere
        /// </summary>
        /// <returns></returns>
        double GetRadius();
    }

    [Browsable(false)]
    public interface IPolygonEntity : IGeometryEntity
    {
        /// <summary>
        /// Returns trimmed polygon after trimming the input polygon using the 
        /// given array of planes as half spaces.
        /// </summary>
        /// <param name="halfSpaces">Trimming planes.</param>
        /// <returns>Trimmed Polygon</returns>
        IPolygonEntity Trim(IPlaneEntity[] halfSpaces);

        /// <summary>
        /// Returns vertices of this polygon.
        /// </summary>
        /// <returns>Array of IPointEntity.</returns>
        IPointEntity[] GetVertices();

        /// <summary>
        /// Computes average plane for the polygon.
        /// </summary>
        /// <returns>Average Plane.</returns>
        IPlaneEntity GetPlane();

        /// <summary>
        /// Returns maximum deviation from average plane of polygon.
        /// </summary>
        /// <returns>Out of plane value.</returns>
        double GetOutOfPlane();

        /// <summary>
        /// Updates vertices.
        /// </summary>
        /// <param name="positions">new vertex points.</param>
        void UpdateVertices(IPointEntity[] positions);
    }

    [Browsable(false)]
    public interface ISubDMeshEntity : IGeometryEntity
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        int GetNumVertices();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IPointEntity[] GetVertices();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IColor[] GetVertexColors();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IVector[] GetVertexNormals();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        int GetNumFaces();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        int[][] GetFaceIndices();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        int GetNumResultVertices();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IPointEntity[] GetResultVertices();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        int GetNumResultFaces();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        int[][] GetResultFaceIndices();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ILineEntity[] GetEdges();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        double ComputeSurfaceArea();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool GetIsClosed();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        double ComputeVolume();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="faceIndices"></param>
        /// <param name="subDLevel"></param>
        /// <returns></returns>
        bool UpdateByVerticesFaceIndices(IPointEntity[] vertices, int[][] faceIndices, int subDLevel);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="normals"></param>
        /// <param name="colors"></param>
        /// <param name="faceIndices"></param>
        /// <param name="subDLevel"></param>
        /// <returns></returns>
        bool UpdateSubDMeshColors(IPointEntity[] vertices, IVector[] normals,
            IColor[] colors, int[][] faceIndices, int subDLevel);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="colors"></param>
        /// <returns></returns>
        bool UpdateSubDMeshColors(IColor[] colors);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="normals"></param>
        /// <returns></returns>
        bool UpdateSubDMeshNormals(IVector[] normals);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bConvertAsSmooth"></param>
        /// <returns></returns>
        ISurfaceEntity ConvertToSurface(bool bConvertAsSmooth);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bConvertAsSmooth"></param>
        /// <returns></returns>
        ISolidEntity ConvertToSolid(bool bConvertAsSmooth);
    }

    [Browsable(false)]
    public interface IBlockEntity : IGeometryEntity
    {
        string GetBlockName();

        string GetFileName();

        IPointEntity GetCentroid();

        IGeometryEntity[] ExtractGeometry();
    }

    [Browsable(false)]
    public interface IBlockHelper
    {
        IBlockEntity InsertBlockFromFile(ICoordinateSystemEntity contextCoordinateSystem, string fileName, string blockName);

        IBlockEntity InsertBlockFromCurrentDocument(ICoordinateSystemEntity contextCoordinateSystem, string blockName);

        bool InsertBlockInTargetFile(ICoordinateSystemEntity contextCoordinateSystem, string blockName, string targetFileName);

        string ImportBlockFromFile(string fileName, string blockName);

        string[] ImportAllBlocksFromFile(string fileName);

        string[] ListAllBlocksInCurrentDocument();

        bool BlockExistsInCurrentDocument(string blockName);

        string[] ListContentsOfBlock(string blockName);

        bool DefineBlock(ICoordinateSystemEntity contextCoordinateSystem, string blockName, IGeometryEntity[] geometries);

        bool RenameBlock(string oldName, string newName);

        bool ExportBlock(string fileName, string sourceBlockName);

        bool ExportBlockDefinition(string fileName, string sourceBlockName);

        bool PurgeBlock(string blockName);
    }

    /// <summary>
    /// 
    /// </summary>
    [Browsable(false)]
    public interface ITopologyEntity : IDesignScriptEntity
    {
    }

    /// <summary>
    /// 
    /// </summary>
    [Browsable(false)]
    public interface IShellEntity : ITopologyEntity
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ISolidEntity GetSolidGeometry();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IFaceEntity[] GetFaces();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        int GetFaceCount();
    }

    /// <summary>
    /// 
    /// </summary>
    [Browsable(false)]
    public interface ICellEntity : ITopologyEntity
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ISolidEntity GetSolidGeometry();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ICellFaceEntity[] GetFaces();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IPointEntity GetCentroid();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        double GetArea();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        double GetVolume();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ICellEntity[] GetAdjacentCells();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        int GetFaceCount();
    }

    /// <summary>
    /// 
    /// </summary>
    [Browsable(false)]
    public interface IFaceEntity : ITopologyEntity
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ICellFaceEntity[] GetCellFaces();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IShellEntity GetShell();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ICellEntity[] GetCells();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IPointEntity GetCentroid();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        double GetArea();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEdgeEntity[] GetEdges();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IVertexEntity[] GetVertices();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ISurfaceEntity GetSurfaceGeometry();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        string GetFaceType();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        int GetCellFaceCount();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        int GetCellCount();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        int GetEdgeCount();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        int GetVertexCount();

        /// <summary>
        /// Checks if the face is planar
        /// </summary>
        /// <returns></returns>
        bool IsPlanar();
    }

    [Browsable(false)]
    public interface ICellFaceEntity : ITopologyEntity
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ICellEntity GetCell();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IFaceEntity GetFace();
    }

    [Browsable(false)]
    public interface IVertexEntity : ITopologyEntity
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEdgeEntity[] GetAdjacentEdges();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IFaceEntity[] GetAdjacentFaces();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IPointEntity GetPointGeometry();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        int GetAdjacentEdgeCount();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        int GetAdjacentFaceCount();
    }

    [Browsable(false)]
    public interface IEdgeEntity : ITopologyEntity
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IFaceEntity[] GetAdjacentFaces();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IVertexEntity GetStartVertex();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IVertexEntity GetEndVertex();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ICurveEntity GetCurveGeometry();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        int GetAdjacentFaceCount();
    }

    /// <summary>
    /// Represents Mesh entity
    /// </summary>
    [Browsable(false)]
    public interface IMeshEntity : ITopologyEntity, IBRepEntity
    {
        /// <summary>
        /// Gets underlying geometry object, this can be ISolidEntity or ISubDMeshEntity
        /// </summary>
        IGeometryEntity Geometry { get; }
    }

    [Browsable(false)]
    public interface ITextEntity : IGeometryEntity
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="orientation"></param>
        /// <param name="textString"></param>
        /// <param name="fontSize"></param>
        bool UpdateByCoordinateSystem(ICoordinateSystemEntity cs, int orientation, string textString, double fontSize);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        double GetFontSize();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        string GetString();
    }

    [Browsable(false)]
    public interface IGeometrySettings
    {
        /// <summary>
        /// Updates visibility of point glyph/marker
        /// </summary>
        bool PointVisibility { get; set; }
    }

    [Browsable(false)]
    public interface IGeometryFactory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        ICoordinateSystemEntity CoordinateSystemByData(double[] data);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contextCS"></param>
        /// <param name="radius"></param>
        /// <param name="theta"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        ICoordinateSystemEntity CoordinateSystemByCylindricalCoordinates(ICoordinateSystemEntity contextCS, double radius, double theta, double height);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contextCS"></param>
        /// <param name="radius"></param>
        /// <param name="theta"></param>
        /// <param name="phi"></param>
        /// <returns></returns>
        ICoordinateSystemEntity CoordinateSystemBySphericalCoordinates(ICoordinateSystemEntity contextCS, double radius, double theta, double phi);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ICoordinateSystemEntity CoordinateSystemByUniversalTransform(ICoordinateSystemEntity contextCoordinateSys, double[] scaleFactors,
            double[] rotationAngles, int[] rotationSequence, IVector translationVector, bool translationSequence);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contextCoordinateSys"></param>
        /// <param name="scaleFactors"></param>
        /// <param name="rotationAngles"></param>
        /// <param name="rotationSequence"></param>
        /// <param name="translationVector"></param>
        /// <param name="translationSequence"></param>
        /// <returns></returns>
        ICoordinateSystemEntity CoordinateSystemByUniversalTransform(ICoordinateSystemEntity contextCoordinateSys, double[] scaleFactors,
            double[] rotationAngles, int[] rotationSequence, double[] translationVector, bool translationSequence);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        IPointEntity CreatePoint(double x, double y, double z);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="controlVertices"></param>
        /// <param name="degree"></param>
        /// <param name="makePeriodic"></param>
        /// <returns></returns>
        IBSplineCurveEntity BSplineByControlVertices(IPointEntity[] controlVertices, int degree, bool makePeriodic);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts"></param>
        /// <param name="makePeriodic"></param>
        /// <returns></returns>
        IBSplineCurveEntity BSplineByPoints(IPointEntity[] pts, bool makePeriodic);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts"></param>
        /// <param name="startTangent"></param>
        /// <param name="endTangent"></param>
        /// <returns></returns>
        IBSplineCurveEntity BSplineByPoints(IPointEntity[] pts, IVector startTangent, IVector endTangent);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="firstPoint"></param>
        /// <param name="secondPoint"></param>
        /// <param name="thirdPoint"></param>
        /// <returns></returns>
        IArcEntity ArcByPointsOnCurve(IPointEntity firstPoint, IPointEntity secondPoint, IPointEntity thirdPoint);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <param name="startAngle"></param>
        /// <param name="endAngle"></param>
        /// <param name="normal"></param>
        /// <returns></returns>
        IArcEntity ArcByCenterPointRadiusAngle(IPointEntity center, double radius, double startAngle, double endAngle, IVector normal);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="centerPoint"></param>
        /// <param name="startPoint"></param>
        /// <param name="sweepAngle"></param>
        /// <param name="normal"></param>
        /// <returns></returns>
        IArcEntity ArcByCenterPointStartPointSweepAngle(IPointEntity centerPoint, IPointEntity startPoint, double sweepAngle, IVector normal);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="centerPoint"></param>
        /// <param name="startPoint"></param>
        /// <param name="sweepPoint"></param>
        /// <returns></returns>
        IArcEntity ArcByCenterPointStartPointSweepPoint(IPointEntity centerPoint, IPointEntity startPoint, IPointEntity sweepPoint);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        ILineEntity LineByStartPointEndPoint(IPointEntity startPoint, IPointEntity endPoint);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="normal"></param>
        /// <returns></returns>
        IPlaneEntity PlaneByOriginNormal(IPointEntity origin, IVector normal);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        IPointEntity PointByCartesianCoordinates(ICoordinateSystemEntity cs, double x, double y, double z);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <param name="normal"></param>
        /// <returns></returns>
        ICircleEntity CircleByCenterPointRadius(IPointEntity center, double radius, IVector normal);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="firstPoint"></param>
        /// <param name="secondPoint"></param>
        /// <param name="thirdPoint"></param>
        /// <returns></returns>
        ICircleEntity CircleByPointsOnCurve(IPointEntity firstPoint, IPointEntity secondPoint, IPointEntity thirdPoint);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="crossSections"></param>
        /// <returns></returns>
        ISurfaceEntity SurfaceByLoftCrossSections(ICurveEntity[] crossSections);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="crossSections"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        ISurfaceEntity SurfaceByLoftCrossSectionsPath(ICurveEntity[] crossSections, ICurveEntity path);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="crossSections"></param>
        /// <param name="guides"></param>
        /// <returns></returns>
        ISurfaceEntity SurfaceByLoftCrossSectionsGuides(ICurveEntity[] crossSections, ICurveEntity[] guides);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        ISurfaceEntity SurfaceBySweep(ICurveEntity profile, ICurveEntity path);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="axisOrigin"></param>
        /// <param name="axisDirection"></param>
        /// <param name="startAngle"></param>
        /// <param name="sweepAngle"></param>
        /// <returns></returns>
        ISurfaceEntity SurfaceByRevolve(ICurveEntity profile, IPointEntity axisOrigin, IVector axisDirection, double startAngle, double sweepAngle);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        ISurfaceEntity SurfacePatchFromCurve(ICurveEntity profile);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="controlVertices"></param>
        /// <param name="uDegree"></param>
        /// <param name="vDegree"></param>
        /// <returns></returns>
        IBSplineSurfaceEntity BSplineSurfaceByControlVertices(IPointEntity[][] controlVertices, int uDegree, int vDegree);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="points"></param>
        /// <param name="uDegree"></param>
        /// <param name="vDegree"></param>
        /// <returns></returns>
        IBSplineSurfaceEntity BSplineSurfaceByPoints(IPointEntity[][] points, int uDegree, int vDegree);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="crossSections"></param>
        /// <returns></returns>
        ISolidEntity SolidByLoftCrossSections(ICurveEntity[] crossSections);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="crossSections"></param>
        /// <param name="guides"></param>
        /// <returns></returns>
        ISolidEntity SolidByLoftCrossSectionsGuides(ICurveEntity[] crossSections, ICurveEntity[] guides);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="crossSections"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        ISolidEntity SolidByLoftCrossSectionsPath(ICurveEntity[] crossSections, ICurveEntity path);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        ISolidEntity SolidBySweep(ICurveEntity profile, ICurveEntity path);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <param name="axisOrigin"></param>
        /// <param name="axisDirection"></param>
        /// <param name="startAngle"></param>
        /// <param name="sweepAngle"></param>
        /// <returns></returns>
        ISolidEntity SolidByRevolve(ICurveEntity profile, IPointEntity axisOrigin, IVector axisDirection, double startAngle, double sweepAngle);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="startRadius"></param>
        /// <param name="endRadius"></param>
        /// <returns></returns>
        IConeEntity ConeByPointsRadius(IPointEntity startPoint, IPointEntity endPoint, double startRadius, double endRadius);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="startRadius"></param>
        /// <param name="endRadius"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        IConeEntity ConeByRadiusLength(ICoordinateSystemEntity cs, double startRadius, double endRadius, double height);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="centerPoint"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        ISphereEntity SphereByCenterPointRadius(IPointEntity centerPoint, double radius);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="length"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        ICuboidEntity CuboidByLengths(ICoordinateSystemEntity cs, double length, double width, double height);

        /// <summary>
        /// Constructs a closed Polygon that joins the vertices in the 1D array end-to-end.
        /// </summary>
        /// <param name="vertices">Array of Point.</param>
        /// <returns>Polygon</returns>
        IPolygonEntity PolygonByVertices(IPointEntity[] vertices);

        /// <summary>
        /// Constructs a subdivision mesh given an input array of vertex points
        /// and an input array of faces defined by a set of numbers, which are 
        /// the indices of the vertices in the 'vertices' array making up the 
        /// face. 'subDivisionLevel' is the initial smoothness level.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="faceIndices"></param>
        /// <param name="subDLevel"></param>
        /// <returns></returns>
        ISubDMeshEntity SubDMeshByVerticesFaceIndices(IPointEntity[] vertices, int[][] faceIndices, int subDLevel);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="points"></param>
        /// <param name="faceIndices"></param>
        /// <param name="vertexNormals"></param>
        /// <param name="vertexColors"></param>
        /// <param name="subDLevel"></param>
        /// <returns></returns>
        ISubDMeshEntity SubDMeshByVerticesFaceIndices(IPointEntity[] points,
            int[][] faceIndices, IVector[] vertexNormals, IColor[] vertexColors, int subDLevel);

        /// <summary>
        /// Constructs a quad SubDMesh from Surface or Solid geometry.
        /// </summary>
        /// <param name="geometry">Geometry object</param>
        /// <param name="maxEdgeLength">mesh size</param>
        /// <returns>ISubDMeshEntity</returns>
        ISubDMeshEntity SubDMeshFromGeometry(IGeometryEntity geometry, double maxEdgeLength);

        /// <summary>
        /// Constructs a topology mesh given an input array of vertex points
        /// and an input array of faces defined by a set of numbers, which are 
        /// the indices of the vertices in the 'vertices' array making up the 
        /// face. 
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="faceIndices"></param>
        /// <returns></returns>
        IMeshEntity MeshByVerticesFaceIndices(IPointEntity[] vertices, int[][] faceIndices);

        /// <summary>
        /// Constructs a topology mesh given an input array of vertex points
        /// and an input array of edges defined by a pair of numbers, which are 
        /// the indices of the vertices in the 'vertices' array making up the 
        /// edge. 
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="edgeIndices"></param>
        /// <returns></returns>
        IMeshEntity MeshByVerticesEdgeIndices(IPointEntity[] vertices, int[] edgeIndices);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contextCoordinateSystem"></param>
        /// <param name="orientation"></param>
        /// <param name="textString"></param>
        /// <param name="fontSize"></param>
        /// <returns></returns>
        ITextEntity TextByCoordinateSystem(ICoordinateSystemEntity contextCoordinateSystem, int orientation, string textString, double fontSize);

        /// <summary>
        /// Returns IBlockHelper
        /// </summary>
        /// <returns>IBlockHelper</returns>
        IBlockHelper GetBlockHelper();

        /// <summary>
        /// Load a sat file and returns the FFI objects
        /// </summary>
        /// <param name="satFile"></param>
        /// <returns></returns>
        IGeometryEntity[] LoadSat(string satFile);

        /// <summary>
        /// Save the objects to a given sat file
        /// </summary>
        /// <param name="satFile"></param>
        /// <param name="ffiObjects"></param>
        /// <returns></returns>
        bool SaveSat(string satFile, Object[] ffiObjects);

        /// <summary>
        /// Returns the settings object
        /// </summary>
        /// <returns></returns>
        IGeometrySettings GetSettings();
    }
}
