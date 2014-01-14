using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Interfaces
{
    [Browsable(false)]
    public interface IGeometryFactory
    {
        IArcEntity ArcByThreePoints(IPointEntity firstPoint, IPointEntity secondPoint, IPointEntity thirdPoint);
        IArcEntity ArcByCenterPointRadiusAngle(IPointEntity center, double radius, double startAngle, double endAngle, IVectorEntity normal);
        IArcEntity ArcByCenterPointStartPointSweepAngle(IPointEntity centerPoint, IPointEntity startPoint, double sweepAngle, IVectorEntity normal);
        IArcEntity ArcByCenterPointStartPointEndPoint(IPointEntity centerPoint, IPointEntity startPoint, IPointEntity endPoint);

        IBoundingBoxEntity BoundingBoxByGeometry(IGeometryEntity geom);
        IBoundingBoxEntity BoundingBoxByGeometry(IGeometryEntity[] geom);
        IBoundingBoxEntity BoundingBoxByGeometryCoordinateSystem(IGeometryEntity geom, ICoordinateSystemEntity cs);
        IBoundingBoxEntity BoundingBoxByGeometryCoordinateSystem(IGeometryEntity[] geom, ICoordinateSystemEntity cs);

        ICircleEntity CircleByCenterPointRadius(IPointEntity centerPoint, double radius);
        ICircleEntity CircleByCenterPointRadiusNormal(IPointEntity centerPoint, double radius, IVectorEntity normal);
        ICircleEntity CircleByPlaneRadius(IPlaneEntity plane, double radius);
        ICircleEntity CircleByThreePoints(IPointEntity p1, IPointEntity p2, IPointEntity p3);

        IConeEntity ConeByPointsRadius(IPointEntity startPoint, IPointEntity endPoint, double startRadius);
        IConeEntity ConeByPointsRadii(IPointEntity startPoint, IPointEntity endPoint, double startRadius, double endRadius);
        IConeEntity ConeByCoordinateSystemHeightRadius(ICoordinateSystemEntity cs, double height, double startRadius);
        IConeEntity ConeByCoordinateSystemHeightRadii(ICoordinateSystemEntity cs, double height, double startRadius, double endRadius);

        ICoordinateSystemEntity CoordinateSystemByMatrix(double[] matrix);
        ICoordinateSystemEntity CoordinateSystemByOrigin(double x, double y);
        ICoordinateSystemEntity CoordinateSystemByOrigin(double x, double y, double z);
        ICoordinateSystemEntity CoordinateSystemByOrigin(IPointEntity origin);
        ICoordinateSystemEntity CoordinateSystemByPlane(IPlaneEntity plane);
        ICoordinateSystemEntity CoordinateSystemByOriginVectors(IPointEntity origin, IVectorEntity xAxis, IVectorEntity yAxis);
        ICoordinateSystemEntity CoordinateSystemByOriginVectors(IPointEntity origin, IVectorEntity xAxis, IVectorEntity yAxis, IVectorEntity zAxis);
        ICoordinateSystemEntity CoordinateSystemByCylindricalCoordinates(ICoordinateSystemEntity iCoordinateSystemEntity, double radius, double theta, double height);
        ICoordinateSystemEntity CoordinateSystemBySphericalCoordinates(ICoordinateSystemEntity iCoordinateSystemEntity, double radius, double theta, double phi);

        ICuboidEntity CuboidByLengths(double width, double length, double height);
        ICuboidEntity CuboidByLengths(IPointEntity originPoint, double width, double length, double height);
        ICuboidEntity CuboidByLengths(ICoordinateSystemEntity cs, double width, double length, double height);
        ICuboidEntity CuboidByCorners(IPointEntity lowPoint, IPointEntity highPoint);

        ICurveEntity CurveByParameterLineOnSurface(ISurfaceEntity baseSurface, IUVEntity startParams, IUVEntity endParams);

        IEllipseEntity EllipseByOriginRadii(IPointEntity origin, double xAxisRadius, double yAxisRadius);
        IEllipseEntity EllipseByOriginVectors(IPointEntity origin, IVectorEntity xAxisRadius, IVectorEntity yAxisRadius);
        IEllipseEntity EllipseByCoordinateSystemRadii(ICoordinateSystemEntity origin, double xAxisRadius, double yAxisRadius);
        IEllipseEntity EllipseByPlaneRadii(IPlaneEntity plane, double xAxisRadius, double yAxisRadius);

        IHelixEntity HelixByAxis(IPointEntity axisPoint, IVectorEntity axisDirection, IPointEntity startPoint, double pitch, double angleTurns);

        IIndexGroupEntity IndexGroupByIndices(uint a, uint b, uint c, uint d);
        IIndexGroupEntity IndexGroupByIndices(uint a, uint b, uint c);

        ILineEntity LineByStartPointEndPoint(IPointEntity startPoint, IPointEntity endPoint);
        ILineEntity LineByBestFit(IPointEntity[] bestFitPoints);
        ILineEntity LineByTangency(ICurveEntity curve, double parameter);

        INurbsCurveEntity NurbsCurveByControlVertices(IPointEntity[] points);
        INurbsCurveEntity NurbsCurveByControlVertices(IPointEntity[] points, int degree);
        INurbsCurveEntity NurbsCurveByControlVertices(IPointEntity[] points, int degree, bool close_curve);
        INurbsCurveEntity NurbsCurveByControlVertices(IPointEntity[] points, int degree, double[] weights, double[] knots);

        INurbsCurveEntity NurbsCurveByPoints(IPointEntity[] points);
        INurbsCurveEntity NurbsCurveByPoints(IPointEntity[] hosts, bool makePeriodic);
        INurbsCurveEntity NurbsCurveByPoints(IPointEntity[] points, int degree);
        INurbsCurveEntity NurbsCurveByPointsTangents(IPointEntity[] pts, IVectorEntity startTangent, IVectorEntity endTangent);

        INurbsSurfaceEntity NurbsSurfaceByPoints(IPointEntity[][] points, int uDegree, int vDegree);
        INurbsSurfaceEntity NurbsSurfaceByControlVertices(IPointEntity[][] controlVertices, int uDegree, int vDegree);
        INurbsSurfaceEntity NurbsSurfaceByControlVertices(IPointEntity[][] controlVertices, int uDegree, int vDegree, double[][] weights, double[][] knots);

        IPlaneEntity PlaneByOriginNormal(IPointEntity origin, IVectorEntity normal);
        IPlaneEntity PlaneByOriginNormalXAxis(IPointEntity origin, IVectorEntity normal, IVectorEntity xAxis);
        IPlaneEntity PlaneByOriginXAxisYAxis(IPointEntity origin, IVectorEntity xAxis, IVectorEntity yAxis);
        IPlaneEntity PlaneByBestFitThroughPoints(IPointEntity[] points);

        IPlaneEntity PlaneByLineAndPoint(ILineEntity line, IPointEntity point);
        IPlaneEntity PlaneByThreePoints(IPointEntity p1, IPointEntity p2, IPointEntity p3);

        IPointEntity PointByCoordinates(double x, double y);
        IPointEntity PointByCoordinates(double x, double y, double z);
        IPointEntity PointByCartesianCoordinates(ICoordinateSystemEntity cs, double x, double y, double z);
        IPointEntity PointByCylindricalCoordinates(ICoordinateSystemEntity cs, double angle, double elevation, double offset);
        IPointEntity PointBySphericalCoordinates(ICoordinateSystemEntity cs, double phi, double theta, double radius);

        IPolygonEntity PolygonByPoints(IPointEntity[] points);

        IPolyCurveEntity PolyCurveByJoinedCurves(ICurveEntity[] curves);

        IRectangleEntity RectangleByCornerPoints(IPointEntity[] points);
        IRectangleEntity RectangleByCornerPoints(IPointEntity p1, IPointEntity p2, IPointEntity p3, IPointEntity p4);
        IRectangleEntity RectangleByWidthHeight(double width, double length);
        IRectangleEntity RectangleByWidthHeight(IPlaneEntity plane, double width, double length);
        IRectangleEntity RectangleByWidthHeight(ICoordinateSystemEntity cs, double width, double length);

        ISurfaceEntity SurfaceByLoft(ICurveEntity[] crossSections);
        ISurfaceEntity SurfaceByLoft(ICurveEntity[] crossSections, ICurveEntity guideCurve);
        ISurfaceEntity SurfaceByLoftGuides(ICurveEntity[] hostXCurves, ICurveEntity[] hostGuides);
        ISurfaceEntity SurfaceBySweep(ICurveEntity iCurveEntity, ICurveEntity iCurveEntity_2);
        ISurfaceEntity SurfaceBySweep2Rails(ICurveEntity rail1, ICurveEntity rail2, ICurveEntity profile);
        ISurfaceEntity SurfaceByRevolve(ICurveEntity profile, IPointEntity axisOrigin, IVectorEntity axisDirection, double startAngle, double sweepAngle);
        ISurfaceEntity SurfaceByPatch(ICurveEntity iCurveEntity);

        ISolidEntity SolidByLoft(ICurveEntity[] crossSections);
        ISolidEntity SolidByLoft(ICurveEntity[] crossSections, ICurveEntity guideCurve);
        ISolidEntity SolidByLoftGuides(ICurveEntity[] xsections, ICurveEntity[] iCurveEntity);
        ISolidEntity SolidBySweep(ICurveEntity iCurveEntity, ICurveEntity iCurveEntity_2);
        ISolidEntity SolidBySweep2Rails(ICurveEntity rail1, ICurveEntity rail2, ICurveEntity profile);
        ISolidEntity SolidByRevolve(ICurveEntity profile, IPointEntity axisOrigin, IVectorEntity axisDirection, double startAngle, double sweepAngle);

        ISphereEntity SphereByCenterPointRadius(IPointEntity centerPoint, double radius);

        ITextEntity TextByPoint(IPointEntity origin, string textString, double textHeight);
        ITextEntity TextByCoordinateSystem(ICoordinateSystemEntity cs, string textString, double textHeight);

        IUVEntity UVByCoordinates(double u, double v);

        IVectorEntity VectorByCoordinates(double x, double y, double z);
        IVectorEntity VectorByCoordinates(double x, double y, double z, bool normalized);

        IPolyMeshEntity PolyMeshByVerticesFaceIndices(IPointEntity[] vertices, IIndexGroupEntity[] indices);

        IBlockHelper GetBlockHelper();
        IGeometryEntity[] LoadSat(string satFile);
        bool SaveSat(string satFile, Object[] ffiObjects);
        IGeometrySettings GetSettings();

    }
}
