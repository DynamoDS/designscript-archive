using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Interfaces
{
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
        INurbsCurveEntity NurbsCurveByPoints(IPointEntity[] points, bool closeCurve);
        INurbsCurveEntity NurbsCurveByPoints(IPointEntity[] points, int degree);
        INurbsCurveEntity NurbsCurveByPoints(IPointEntity[] points, int degree, bool closeCurve);
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

        /// <summary>
        /// Create a CoordinateSystem from a "4x4 transformation matrix given in
        /// pre-multiplier form." (From ASM Docs) Input should be 16 doubles 
        /// long, representing a 4x4 matrix in form: r1c1, r1c2, r1c3, r1c4, 
        /// r2c1, ...
        /// </summary>
        ICoordinateSystemEntity CoordinateSystemByMatrix(double[] matrix);
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

        IUVEntity UVByUV(double u, double v);

        ICurveEntity CurveByParameterLineOnSurface(ISurfaceEntity baseSurface, IUVEntity startParams, IUVEntity endParams);

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
        ISurfaceEntity SurfaceByLoft(ICurveEntity[] crossSections, ICurveEntity[] guideCurves);
        ISurfaceEntity SurfaceBySweep1Rail(ICurveEntity rail, ICurveEntity crossSection);
        ISurfaceEntity SurfaceBySweep1Rail(ICurveEntity rail, ICurveEntity[] crossSections);
        ISurfaceEntity SurfaceBySweep2Rails(ICurveEntity rail1, ICurveEntity rail2, ICurveEntity[] crossSections);
        ISurfaceEntity SurfaceByRevolve(ICurveEntity profile, IPointEntity axisOrigin, IVectorEntity axisDirection, double startAngle, double sweepAngle);
        ISurfaceEntity SurfaceByPatch(ICurveEntity closedCurve);
        ISurfaceEntity SurfaceByPatch(ICurveEntity[] curves);

        ISolidEntity SolidByLoft(ICurveEntity[] crossSections);
        ISolidEntity SolidByLoft(ICurveEntity[] crossSections, ICurveEntity guideCurve);
        ISolidEntity SolidByLoft(ICurveEntity[] crossSections, ICurveEntity[] guideCurves);
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

        IPolyMeshEntity PolyMeshByVerticesFaceIndices(IPointEntity[] vertices, IIndexGroup[] indices);

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
