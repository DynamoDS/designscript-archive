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
        byte AlphaValue { get; }
        byte RedValue { get; }
        byte GreenValue { get; }
        byte BlueValue { get; }
    }

    /// <summary>
    /// Interface to represent vector data object
    /// </summary>
    [Browsable(false)]
    public interface IVectorEntity : IDesignScriptEntity, ITransformableEntity
    {
        double X { get; }
        double Y { get; }
        double Z { get; }

        /// <summary>
        /// Tolerance is set at the Host level
        /// </summary>
        bool IsAlmostEqualTo(IVectorEntity other);
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
    public interface ITransformableEntity 
    {
        void Translate(double x, double y, double z);
        void Translate(IVectorEntity vec);

        void TransformBy(ICoordinateSystemEntity cs);
        void TransformFromTo(ICoordinateSystemEntity from, ICoordinateSystemEntity to);

        /// <summary>
        /// Rotates an object around an origin and an axis by a specified 
        /// degree
        /// </summary>
        void Rotate(IPointEntity origin, IVectorEntity axis, double degrees);

        /// <summary>
        /// Rotates an object around the Plane origin and normal by a specified 
        /// degree
        /// </summary>
        void Rotate(IPlaneEntity origin, double degrees);

        void Scale(double amount);
        void Scale(double xamount, double yamount, double zamount);
        void Scale(IPointEntity from, IPointEntity to);

        void Scale1D(IPointEntity from, IPointEntity to);
        void Scale2D(IPointEntity from, IPointEntity to);
    }


    [Browsable(false)]
    public interface ICoordinateSystemEntity : IDesignScriptEntity, ITransformableEntity
    {
        ICoordinateSystemEntity Inverse();

        ICoordinateSystemEntity Mirror(IPlaneEntity mirror_plane);

        ICoordinateSystemEntity PostMultiplyBy(ICoordinateSystemEntity other);
        ICoordinateSystemEntity PreMultiplyBy(ICoordinateSystemEntity other);

        bool IsSingular { get; }
        bool IsScaledOrtho { get; }
        bool IsUniscaledOrtho { get; }
        double Determinant { get; }

        /// <summary>
        /// Returns a Vector containing the X, Y, and Z scale factors
        /// </summary>
        IVectorEntity Scale();

        bool IsEqualTo(ICoordinateSystemEntity other);

        IPointEntity Origin { get; }

        IVectorEntity XAxis { get; }
        IVectorEntity YAxis { get; }
        IVectorEntity ZAxis { get; }

        double XScaleFactor { get; }
        double YScaleFactor { get; }
        double ZScaleFactor { get; }

        IDisplayable Display { get; }
    }

    [Browsable(false)]
    public interface IUV
    {
        double U { get; }
        double V { get; }
    }

    [Browsable(false)]
    public interface IGeometryData
    {
        IntPtr EntityPtr { get; }
        IntPtr BodyPtr { get; }
    }

    [Browsable(false)]
    public interface IGeometryEntity : IDesignScriptEntity, ITransformableEntity
    {
        IGeometryEntity Clone();

        IGeometryEntity CopyAndTranslate(IVectorEntity offset);
        IGeometryEntity CopyAndTransform(ICoordinateSystemEntity toCS);
        IGeometryEntity CopyAndTransform(ICoordinateSystemEntity fromCS, ICoordinateSystemEntity toCS);

        double DistanceTo(IGeometryEntity entity);

        IPointEntity GetClosestPoint(IGeometryEntity entity);

        bool DoesIntersect(IGeometryEntity entity);

        bool IsWithin(IPointEntity point); 

        ICoordinateSystemEntity CoordinateSystem { get; }

        IGeometryEntity[] Intersect(IGeometryEntity entity);
        IGeometryEntity[] Intersect(IGeometryEntity[] entity);

        IGeometryEntity[] Split(IGeometryEntity tool);
        IGeometryEntity[] Split(IGeometryEntity[] tools);

        /// <summary>
        /// Removes elements of the entity closest to the pick point
        /// </summary>
        IGeometryEntity[] Trim(IGeometryEntity tool, IPointEntity pick);

        /// <summary>
        /// Removes elements of the entity closest to the pick point
        /// </summary>
        IGeometryEntity[] Trim(IGeometryEntity[] tools, IPointEntity pick);

        /// <summary>
        /// Separates compound or non-separated elements into their component
        /// parts.
        /// </summary>
        IGeometryEntity[] Explode();

        IColor Color { get; }

        IBoundingBoxEntity BoundingBox { get; }
    }

    [Browsable(false)]
    public interface IPointEntity : IGeometryEntity
    {
        double X { get; }
        double Y { get; }
        double Z { get; }
    }


    [Browsable(false)]
    public interface ICurveEntity : IGeometryEntity
    {
        IPointEntity PointAtParameter(double param);
        IVectorEntity TangentAtParameter(double param);
        IVectorEntity NormalAtParameter(double param);
        ICoordinateSystemEntity CoordinateSystemAtParameter(double param);
        ICoordinateSystemEntity HorizantalFrameAtParameter(double param);
        ICoordinateSystemEntity GetNoTwistFrameAtParameter(double param);

        double DistanceAtParameter(double param);

        double ParameterAtDistance(double distance);

        double StartParameter();
        double EndParameter();

        double Length { get; }
        double GetLength(double startParam, double endParam);

        IPointEntity PointAtDistance(double distance);

        double ParameterAtPoint(IPointEntity point);

        ICurveEntity Reverse();

        bool IsPlanar { get; }
        bool IsClosed { get; }

        IPointEntity StartPoint { get; }
        IPointEntity EndPoint { get; }

        IVectorEntity Normal { get; }

        ICurveEntity Offset(double distance);

        /// <summary>
        /// Tolerance is set at the Host level
        /// </summary>
        bool IsAlmostEqualTo(IPointEntity other);

        /// <summary>
        /// Removes the start of the Curve at the specified parameter
        /// </summary>
        ICurveEntity ParameterTrimStart(double startParameter);
        /// <summary>
        /// Removes the end of the Curve at the specified parameter
        /// </summary>
        ICurveEntity ParameterTrimEnd(double endParameter);
        /// <summary>
        /// Removes the beginning and ends of at the specified parameters.
        /// </summary>
        ICurveEntity ParameterTrim(double startParameter, double endParameter);
        /// <summary>
        /// Removes the interior portion of a Curve at the specified parameters
        /// </summary>
        ICurveEntity[] ParameterTrimInterior(double startParameter, double endParameter);
        /// <summary>
        /// Removes several segments of the curve, discarding the 1st, 3rd, 5th ... segments
        /// </summary>
        ICurveEntity[] ParameterTrimSegments(double[] parameters);
        /// <summary>
        /// Removes several segments of the Curve, disgarding 2nd, 4th, 6th ... segments if the bool is true
        /// </summary>
        ICurveEntity[] ParameterTrimSegments(double[] parameters, bool discardEvenSegments);

        ICurveEntity[] ParameterSplit(double parameter);
        ICurveEntity[] ParameterSplit(double[] parameters);

        /// <summary>
        /// Join combines this curve and the input curve into a new PolyCurve,
        /// maintaining the original curves exactly.
        /// </summary>
        IPolyCurveEntity Join(ICurveEntity curve);

        /// <summary>
        /// Create an approximate NurbsCurve representing the two curves.
        /// </summary>
        INurbsCurveEntity Merge(ICurveEntity curve);

        /// <summary>
        /// Extrudes a Curve in the normal Vector direction
        /// </summary>
        ISurfaceEntity Extrude(double distance);
        /// <summary>
        /// Extrudes a Curve in the specified direction, by the length of the 
        /// input Vector
        /// </summary>
        ISurfaceEntity Extrude(IVectorEntity direction);
        /// <summary>
        /// Extrudes a Curve in the specified direction, by the specified distance
        /// </summary>
        ISurfaceEntity Extrude(IVectorEntity direction, double distance);

        /// <summary>
        /// Extrudes a Curve in the normal Vector direction. Curve must be closed.
        /// </summary>
        ISolidEntity ExtrudeAsSolid(double distance);
        /// <summary>
        /// Extrudes a Curve in the specified direction, by the length of the 
        /// input Vector. Curve must be closed.
        /// </summary>
        ISolidEntity ExtrudeAsSolid(IVectorEntity direction);
        /// <summary>
        /// Extrudes a Curve in the specified direction, by the specified distance. Curve must be closed.
        /// </summary>
        ISolidEntity ExtrudeAsSolid(IVectorEntity direction, double distance);
        
        ICurveEntity Extend(double distance, IPointEntity pickSide);
        ICurveEntity ExtendStart(double distance);
        ICurveEntity ExtendEnd(double distance);

        ICurveEntity[] ApproximateWithArcAndLineSegments();

        IGeometryEntity[] Project(IGeometryEntity otherGeom, IVectorEntity vec);

        /// <summary>
        /// Converts the Curve to a NurbsCurve, if needs be
        /// </summary>
        INurbsCurveEntity ToNurbsCurve();

        ICurveEntity PullOntoPlane(IPlaneEntity plane);

        /// <summary>
        /// Patch a closed Curve
        /// </summary>
        ISurfaceEntity Patch();
    }

    [Browsable(false)]
    public interface ILineEntity : ICurveEntity
    {
    }

    [Browsable(false)]
    public interface IHelixEntity : ICurveEntity
    {
        double Angle { get; }
        double Pitch { get; }
        double Radius { get; }

        IVectorEntity AxisDirection { get; }
        IPointEntity AxisPoint { get; }
    }

    [Browsable(false)]
    public interface ICircleEntity : ICurveEntity
    {
        IPointEntity CenterPoint { get; }
        double Radius { get; }
    }

    [Browsable(false)]
    public interface IEllipseEntity : ICurveEntity
    {
        IPointEntity CenterPoint { get; }
        IVectorEntity MajorAxis { get; }
        IVectorEntity MinorAxis { get; }
    }

    [Browsable(false)]
    public interface IRectangleEntity : ICurveEntity
    {
        double Width { get; }
        double Height { get; }

        IPointEntity Center { get; }

        IPointEntity[] Corners { get; }
    }

    [Browsable(false)]
    public interface IArcEntity : ICurveEntity
    {
        IPointEntity CenterPoint { get; }
        double Radius { get; }

        /// <summary>
        /// Returns Start angle in Radians
        /// </summary>
        double StartAngle { get; }

        /// <summary>
        /// Returns Sweep angle in Radians
        /// </summary>
        double SweepAngle { get; }
    }

    [Browsable(false)]
    public interface IBoundingBoxEntity : IDesignScriptEntity, ITransformableEntity
    {
        IPointEntity MinPoint { get; }
        IPointEntity MaxPoint { get; }

        IBoundingBoxEntity Intersection(IBoundingBoxEntity other);
        bool Intersects(IBoundingBoxEntity other);

        bool IsEmpty();

        bool Contains(IPointEntity point);

        ICuboidEntity ToCuboid();
        IPolySurfaceEntity ToPolySurface();
    }

    [Browsable(false)]
    public interface IPolyCurveEntity : IGeometryEntity
    {

    }

    [Browsable(false)]
    public interface INurbsCurveEntity : ICurveEntity
    {
        IPointEntity[] GetControlVertices();

        double[] GetKnots();
        double[] GetWeights();

        int Degree { get; }
        bool IsPeriodic { get; }
        bool IsRational { get; }
    }

    [Browsable(false)]
    public interface IBRepEntity
    {
        IVertexEntity[] GetVertices();
        IEdgeEntity[] GetEdges();
        IFaceEntity[] GetFaces();

        int GetVertexCount();
        int GetEdgeCount();
        int GetFaceCount();
    }

    [Browsable(false)]
    public interface ISurfaceEntity : IGeometryEntity
    {
        /// <summary>
        /// Gets a Nurbs representation of the Surface. This method may approximate
        /// Surface in certain circumstances.
        /// </summary>
        INurbsSurfaceEntity ToNurbsSurface();
        INurbsSurfaceEntity ApproximateWithTolerance(double tolerance);

        /// <summary>
        /// Thickens the Surface on both sides
        /// </summary>
        ISolidEntity Thicken(double thickness);
        ISolidEntity Thicken(double thickness, bool bothSides);

        ISurfaceEntity Offset(double distance);

        /// <summary>
        /// The returned coordination system use xAxis, yAxis and zAxis to represent the uDir, vDir and normal.
        /// The length of xAxis, yAxis represents the curvatures.
        /// </summary>
        ICoordinateSystemEntity CurvatureAtParameter(double u, double v);
        ICoordinateSystemEntity CoordinateSystemAtParameter(double u, double v);
        IVectorEntity TangentAtUParameter(double u, double v);
        IVectorEntity TangentAtVParameter(double u, double v);
        IVectorEntity NormalAtParameter(double u, double v);
        IVectorEntity[] DerivativesAtParameter(double u, double v);
        double GaussianCurvatureAtParameter(double u, double v);
        double[] PrincipalCurvaturesAtParameter(double u, double v);
        IVectorEntity[] PrincipalDirectionsAtParameter(double u, double v);
        IPointEntity PointAtParameter(double u, double v);

        Tuple<double, double> UVParameterAtPoint(IPointEntity point);

        double Area { get; }
        double Perimeter { get; }

        bool ClosedInU { get; }
        bool ClosedInV { get; }
        bool Closed { get; }

        ICurveEntity[] PerimeterCurves();

        /// <summary>
        /// TODO: this needs documentation
        /// </summary>
        ICurveEntity[] GetIsolines(int isoDirection, double parameter);

        /// <summary>
        /// Returns a new Surface with the Normal flipped. Leaves this surface
        /// unchanged.
        /// </summary>
        ISurfaceEntity FlipNormalDirection();
    }

    [Browsable(false)]
    public interface INurbsSurfaceEntity : ISurfaceEntity
    {
        int DegreeU { get; }
        int DegreeV { get; }

        int NumControlPointsU { get; }
        int NumControlPointsV { get; }

        bool IsPeriodicInU { get; }
        bool IsPeriodicInV { get; }

        bool IsRational { get; }

        IPointEntity[][] GetPoles();
        IPointEntity[][] GetControlPoints();
        double[][] GetWeights();

        double[] GetUKnots();
        double[] GetVKnots();
    }

    [Browsable(false)]
    public interface IPlaneEntity : IGeometryEntity
    {
        IPointEntity Origin { get; }
        IVectorEntity Normal { get; }

        ICoordinateSystemEntity ToCoordinateSystem();

        IPlaneEntity Offset(double dist);

        /// <summary>
        /// The XAxis bias of the Plane
        /// </summary>
        IVectorEntity XAxis { get; }
        /// <summary>
        /// The YAxis bais of the Plane
        /// </summary>
        IVectorEntity YAxis { get; }
    }

    [Browsable(false)]
    public interface ISolidEntity : IGeometryEntity, IBRepEntity
    {      
        /// <summary>
        /// Returns the surface area -- sum of all the areas of all faces
        /// </summary>
        double Area { get; }
        double Volume { get; }

        IPointEntity GetCenterOfGravity();
        IPointEntity GetCentroid();

        ISolidEntity CSGUnion(ISolidEntity geometry);
        ISolidEntity CSGDifference(ISolidEntity geometry);
        ISolidEntity CSGIntersect(ISolidEntity geometry);

        ISolidEntity[] ThinShell(double internalFaceThickness, double externalFaceThickness);

        ISolidEntity Regularise();
        bool IsNonManifold();

        IShellEntity[] GetShells();
        ICellEntity[] GetCells();

        int GetCellCount();
        int GetShellCount();
    }

    [Browsable(false)]
    public interface IPrimitiveSolidEntity : ISolidEntity
    {

    }

    [Browsable(false)]
    public interface IConeEntity : IPrimitiveSolidEntity
    {
        IPointEntity StartPoint { get; }
        IPointEntity EndPoint { get; } 

        double RadiusRatio { get; }

        double StartRadius { get; }
        double EndRadius { get; }

        double Height { get; }
    }

    [Browsable(false)]
    public interface ICuboidEntity : IPrimitiveSolidEntity
    {
        double Length { get; }
        double Width { get; }
        double Height { get; }
    }

    [Browsable(false)]
    public interface ISphereEntity : IPrimitiveSolidEntity
    {
        IPointEntity CenterPoint { get; }
        double Radius { get; }
    }

    [Browsable(false)]
    public interface IPolygonEntity : ISurfaceEntity
    {
        IPointEntity[] GetPoints();

        IPlaneEntity Plane { get; }

        /// <summary>
        /// Returns maximum deviation from average plane of polygon.
        /// </summary>
        double PlaneDeviation { get; }
    }


    [Browsable(false)]
    public interface IPolySurfaceEntity : IGeometryEntity
    {
        ISurfaceEntity[] Surfaces();
    }

    /// <summary>
    /// This is a "dumb" mesh, such as an OBJ or STL, verses a SubD Mesh
    /// </summary>
    [Browsable(false)]
    public interface IPolyMeshEntity : IGeometryEntity
    {
    }

    [Browsable(false)]
    public interface ISubDMeshEntity : IGeometryEntity
    {
        int NumVertices { get; }
        int NumFaces { get; }
        int NumResultVertices { get; }
        int NumResultFaces { get; }

        IPointEntity[] GetVertices();
        IColor[] GetVertexColors();
        IVectorEntity[] GetVertexNormals();
        int[][] GetFaceIndices();

        IPointEntity[] GetResultVertices();
        int[][] GetResultFaceIndices();

        ILineEntity[] GetEdges();

        double Area { get; }
        double Volume { get; }

        bool GetIsClosed();

        ISurfaceEntity ConvertToSurface(bool bConvertAsSmooth);
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

    [Browsable(false)]
    public interface ITopologyEntity : IDesignScriptEntity
    {
    }

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

    public interface IIndiceGroup
    {
        /// <summary>
        /// Either 3 or 4, depending if it represents a triangle or a quad
        /// </summary>
        int Count { get; set; }

        int A { get; set; }
        int B { get; set; }
        int C { get; set; }
        int D { get; set; }
    }

    [Browsable(false)]
    public interface ITextEntity : IDesignScriptEntity
    {
        /// <summary>
        /// Returns the height in absolute units;
        /// </summary>
        double Height { get; }
        string Text { get; }

        /// <summary>
        /// Gets the encoding of the underlying text
        /// </summary>
        System.Text.Encoding GetEncoding();
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
        IArcEntity ArcByPointsOnCurve(IPointEntity firstPoint, IPointEntity secondPoint, IPointEntity thirdPoint);
        IArcEntity ArcByCenterPointRadiusAngle(IPointEntity center, double radius, double startAngle, double endAngle, IVectorEntity normal);
        IArcEntity ArcByCenterPointStartPointSweepAngle(IPointEntity centerPoint, IPointEntity startPoint, double sweepAngle, IVectorEntity normal);
        IArcEntity ArcByCenterPointStartPointSweepPoint(IPointEntity centerPoint, IPointEntity startPoint, IPointEntity sweepPoint);

        IBoundingBoxEntity BoundingBoxByGeometry(IGeometryEntity geom);
        IBoundingBoxEntity BoundingBoxByGeometries(IGeometryEntity[] geom);
        IBoundingBoxEntity BoundingBoxByGeometry(IGeometryEntity geom, ICoordinateSystemEntity cs);
        IBoundingBoxEntity BoundingBoxByGeometries(IGeometryEntity[] geom, ICoordinateSystemEntity cs);

        INurbsCurveEntity NurbsCurveByControlVertices(IPointEntity[] points);
        INurbsCurveEntity NurbsCurveByControlVertices(IPointEntity[] points, int degree);
        INurbsCurveEntity NurbsCurveByControlVertices(IPointEntity[] points, int degree, bool close_curve);
        // weight array: the array size must be the size of the control vertices
        // knot array: the array size must be num_control_points + degree + 1
        INurbsCurveEntity NurbsCurveByControlVertices(IPointEntity[] points, int degree, double[] weights, double[] knots);
        INurbsCurveEntity NurbsCurveByPoints(IPointEntity[] points);
        INurbsCurveEntity NurbsCurveByPoints(IPointEntity[] points, int degree);
        INurbsCurveEntity NurbsCurveByPoints(IPointEntity[] pts, IVectorEntity startTangent, IVectorEntity endTangent);

        INurbsSurfaceEntity NurbsSurfaceByPoints(IPointEntity[][] points, int uDegree, int vDegree);
        INurbsSurfaceEntity NurbsSurfaceByControlVertices(IPointEntity[][] controlVertices, int uDegree, int vDegree);
        INurbsSurfaceEntity NurbsSurfaceByControlVertices(IPointEntity[][] controlVertices, int uDegree, int vDegree, double[][] weights, double[][] knots);

        ICircleEntity CircleByCenterPointRadius(IPointEntity centerPoint, double radius);
        ICircleEntity CircleByCenterPointRadiusNormal(IPointEntity centerPoint, double radius, IVectorEntity normal);
        ICircleEntity CircleByPlaneRadius(IPlaneEntity plane, double radius);
        ICircleEntity CircleByThreePoints(IPointEntity p1, IPointEntity p2, IPointEntity p3);

        IConeEntity ConeByPointsRadius(IPointEntity startPoint, IPointEntity endPoint, double startRadius);
        IConeEntity ConeByPointsRadii(IPointEntity startPoint, IPointEntity endPoint, double startRadius, double endRadius);
        IConeEntity ConeByCoordinateSystemHeightRadius(ICoordinateSystemEntity cs, double height, double startRadius);
        IConeEntity ConeByCoordinateSystemHeightRadii(ICoordinateSystemEntity cs, double height, double startRadius, double endRadius);

        ICoordinateSystemEntity CoordinateSystemByOrigin(double x, double y);
        ICoordinateSystemEntity CoordinateSystemByOrigin(double x, double y, double z);
        ICoordinateSystemEntity CoordinateSystemByOrigin(IPointEntity origin);
        ICoordinateSystemEntity CoordinateSystemByPlane(IPlaneEntity plane);
        ICoordinateSystemEntity CoordinateSystemByOriginVectors(IPointEntity origin, IVectorEntity xAxis, IVectorEntity yAxis);
        ICoordinateSystemEntity CoordinateSystemByOriginVectors(IPointEntity origin, IVectorEntity xAxis, IVectorEntity yAxis, IVectorEntity zAxis);
        ICoordinateSystemEntity CoordinateSystemByCylindricalCoordinates(IPointEntity origin, double radius, double theta, double height);
        ICoordinateSystemEntity CoordinateSystemByCylindricalCoordinates(ICoordinateSystemEntity contextCS, double radius, double theta, double height);
        ICoordinateSystemEntity CoordinateSystemBySphericalCoordinates(IPointEntity origin, double radius, double theta, double phi);
        ICoordinateSystemEntity CoordinateSystemBySphericalCoordinates(ICoordinateSystemEntity contextCS, double radius, double theta, double phi);

        ICuboidEntity CuboidByLengths(double width, double length, double height);
        ICuboidEntity CuboidByLengths(IPointEntity originPoint, double width, double length, double height);
        ICuboidEntity CuboidByLengths(ICoordinateSystemEntity cs, double width, double length, double height);
        ICuboidEntity CuboidByCorners(IPointEntity lowPoint, IPointEntity highPoint);

        IUV UVByUV(double u, double v);

        ICurveEntity CurveByParameterLineOnSurface(ISurfaceEntity baseSurface, IUV startParams, IUV endParams);

        IEllipseEntity EllipseByOriginRadii(IPointEntity origin, double xAxisRadius, double yAxisRadius);
        IEllipseEntity EllipseByOriginVectors(IPointEntity origin, IVectorEntity xAxisRadius, IVectorEntity yAxisRadius);
        IEllipseEntity EllipseByCoordinateSystemRadii(ICoordinateSystemEntity origin, double xAxisRadius, double yAxisRadius);
        IEllipseEntity EllipseByPlaneRadii(IPlaneEntity plane, double xAxisRadius, double yAxisRadius);

        IHelixEntity HelixByAxis(IPointEntity axisPoint, IVectorEntity axisDirection, IPointEntity startPoint, double pitch, double angleTurns);

        ILineEntity LineByStartPointEndPoint(IPointEntity startPoint, IPointEntity endPoint);
        ILineEntity LineByBestFit(IPointEntity[] bestFitPoints);
        ILineEntity LineByTangency(ICurveEntity curve, double parameter);


        IPlaneEntity PlaneByOriginNormal(IPointEntity origin, IVectorEntity normal);
        IPlaneEntity PlaneByOriginNormalXAxis(IPointEntity origin, IVectorEntity normal, IVectorEntity xAxis);
        IPlaneEntity PlaneByOriginXAxisYAxis(IPointEntity origin, IVectorEntity xAxis, IVectorEntity yAxis);
        IPlaneEntity PlaneByBestFitThroughPoints(IPointEntity[] points);
        /// <summary>
        /// Create the Plane that lies in the three Points determined by the Line start Point
        /// Line end Point, and the input Point
        /// </summary>
        IPlaneEntity PlaneByLineAndPoint(ILineEntity line, IPointEntity point);
        IPlaneEntity PlaneByThreePoints(IPointEntity p1, IPointEntity p2, IPointEntity p3);

        IPointEntity PointByCoordinates(double x, double y);
        IPointEntity PointByCoordinates(double x, double y, double z);
        IPointEntity PointByCartesianCoordinates(ICoordinateSystemEntity cs, double x, double y, double z);
        IPointEntity PointByCylindricalCoordinates(ICoordinateSystemEntity cs, double angle, double elevation, double offset);
        IPointEntity PointBySphereicalCoordinates(ICoordinateSystemEntity cs, double phi, double theta, double radius);

        IPolygonEntity PolygonByPoints(IPointEntity[] points);

        IPolyCurveEntity PolyCurveByJoinedCurves(ICurveEntity[] curves);

        IRectangleEntity RectangleByCornerPoints(IPointEntity[] points);
        IRectangleEntity RectangleByCornerPoints(IPointEntity p1, IPointEntity p2, IPointEntity p3, IPointEntity p4);
        IRectangleEntity RectangleByWidthLength(double width, double length);
        IRectangleEntity RectangleByWidthLength(IPlaneEntity plane, double width, double length);
        IRectangleEntity RectangleByWidthLength(ICoordinateSystemEntity cs, double width, double length);

        ISurfaceEntity SurfaceByLoft(ICurveEntity[] crossSections);
        ISurfaceEntity SurfaceByLoft(ICurveEntity[] crossSections, ICurveEntity guideCurve);
        ISurfaceEntity SurfaceBySweep1Rail(ICurveEntity rail, ICurveEntity[] crossSections);
        ISurfaceEntity SurfaceBySweep2Rails(ICurveEntity rail1, ICurveEntity rail2, ICurveEntity[] crossSections);
        ISurfaceEntity SurfaceByRevolve(ICurveEntity profile, IPointEntity axisOrigin, IVectorEntity axisDirection, double startAngle, double sweepAngle);

        ISolidEntity SolidByLoft(ICurveEntity[] crossSections);
        ISolidEntity SolidByLoft(ICurveEntity[] crossSections, ICurveEntity guideCurve);
        ISolidEntity SolidBySweep1Rail(ICurveEntity rail, ICurveEntity[] crossSections);
        ISolidEntity SolidBySweep2Rails(ICurveEntity rail1, ICurveEntity rail2, ICurveEntity[] crossSections);
        ISolidEntity SolidByRevolve(ICurveEntity profile, IPointEntity axisOrigin, IVectorEntity axisDirection, double startAngle, double sweepAngle);

        ISphereEntity SphereByCenterPointRadius(IPointEntity centerPoint, double radius);

        /// <summary>
        /// Create a Text at the origin Point, oriented along the XAxis, pointing upward to the ZAxis 
        /// </summary>
        ITextEntity TextByPoint(IPointEntity origin, string textString, double textHeight);
        /// <summary>
        /// Create a Text at the origin Point, oriented along the XAxis, pointing upward to the ZAxis 
        /// </summary>
        ITextEntity TextByPoint(IPointEntity origin, string textString, double textHeight, System.Text.Encoding textEncoding);
        /// <summary>
        /// Create a Text in the XZ Plane of the input CoordinateSystem.
        /// </summary>
        ITextEntity TextByCoordinateSystem(ICoordinateSystemEntity cs, string textString, double textHeight);
        /// <summary>
        /// Create a Text in the XZ Plane of the input CoordinateSystem.
        /// </summary>
        ITextEntity TextByCoordinateSystem(ICoordinateSystemEntity cs, string textString, double textHeight, System.Text.Encoding textEncoding);

        IPolyMeshEntity PolyMeshByVerticesFaceIndices(IPointEntity[] vertices, IIndiceGroup[] indices);


        IBlockHelper GetBlockHelper();


        IGeometryEntity[] LoadSat(string satFile);
        bool SaveSat(string satFile, Object[] ffiObjects);


        IGeometrySettings GetSettings();


        // NOTE: (by Patrick) I don't think we should support SubDMeshes for the 
        // first pass of this API. They are not easily supported by ASM, and
        // have no equivalence in Revit

        /// <summary>
        /// Constructs a subdivision mesh given an input array of vertex points
        /// and an input array of faces defined by a set of numbers, which are 
        /// the indices of the vertices in the 'vertices' array making up the 
        /// face. 'subDivisionLevel' is the initial smoothness level.
        /// </summary>
        //ISubDMeshEntity SubDMeshByVerticesFaceIndices(IPointEntity[] vertices, int[][] faceIndices, int subDLevel);
        //ISubDMeshEntity SubDMeshByVerticesFaceIndices(IPointEntity[] points,
        //    int[][] faceIndices, IVectorEntity[] vertexNormals, IColor[] vertexColors, int subDLevel);

        /// <summary>
        /// Constructs a quad SubDMesh from Surface or Solid geometry.
        /// </summary>
        //ISubDMeshEntity SubDMeshFromGeometry(IGeometryEntity geometry, double maxEdgeLength);
    }
}
