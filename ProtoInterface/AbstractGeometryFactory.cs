using System;
using Autodesk.DesignScript.Interfaces;
using System.ComponentModel;

namespace Autodesk.DesignScript.Geometry
{
    [Browsable(false)]
    public abstract class AbstractGeometryFactory : IGeometryFactory
    {

        public virtual IArcEntity ArcByThreePoints(IPointEntity firstPoint, IPointEntity secondPoint, IPointEntity thirdPoint)
        {
            throw new NotImplementedException();
        }

        public virtual IArcEntity ArcByCenterPointRadiusAngle(IPointEntity center, double radius, double startAngle, double endAngle, IVectorEntity normal)
        {
            throw new NotImplementedException();
        }

        public virtual IArcEntity ArcByCenterPointStartPointSweepAngle(IPointEntity centerPoint, IPointEntity startPoint, double sweepAngle, IVectorEntity normal)
        {
            throw new NotImplementedException();
        }

        public virtual IArcEntity ArcByCenterPointStartPointEndPoint(IPointEntity centerPoint, IPointEntity startPoint, IPointEntity endPoint)
        {
            throw new NotImplementedException();
        }

        public virtual IBoundingBoxEntity BoundingBoxByGeometry(IGeometryEntity geom)
        {
            throw new NotImplementedException();
        }

        public virtual IBoundingBoxEntity BoundingBoxByGeometry(IGeometryEntity[] geom)
        {
            throw new NotImplementedException();
        }

        public virtual IBoundingBoxEntity BoundingBoxByGeometryCoordinateSystem(IGeometryEntity geom, ICoordinateSystemEntity cs)
        {
            throw new NotImplementedException();
        }

        public virtual IBoundingBoxEntity BoundingBoxByGeometryCoordinateSystem(IGeometryEntity[] geom, ICoordinateSystemEntity cs)
        {
            throw new NotImplementedException();
        }

        public virtual ICircleEntity CircleByCenterPointRadius(IPointEntity centerPoint, double radius)
        {
            throw new NotImplementedException();
        }

        public virtual ICircleEntity CircleByCenterPointRadiusNormal(IPointEntity centerPoint, double radius, IVectorEntity normal)
        {
            throw new NotImplementedException();
        }

        public virtual ICircleEntity CircleByPlaneRadius(IPlaneEntity plane, double radius)
        {
            throw new NotImplementedException();
        }

        public virtual ICircleEntity CircleByThreePoints(IPointEntity p1, IPointEntity p2, IPointEntity p3)
        {
            throw new NotImplementedException();
        }

        public virtual IConeEntity ConeByPointsRadius(IPointEntity startPoint, IPointEntity endPoint, double startRadius)
        {
            throw new NotImplementedException();
        }

        public virtual IConeEntity ConeByPointsRadii(IPointEntity startPoint, IPointEntity endPoint, double startRadius, double endRadius)
        {
            throw new NotImplementedException();
        }

        public virtual IConeEntity ConeByCoordinateSystemHeightRadius(ICoordinateSystemEntity cs, double height, double startRadius)
        {
            throw new NotImplementedException();
        }

        public virtual IConeEntity ConeByCoordinateSystemHeightRadii(ICoordinateSystemEntity cs, double height, double startRadius, double endRadius)
        {
            throw new NotImplementedException();
        }

        public virtual ICoordinateSystemEntity CoordinateSystemByMatrix(double[] matrix)
        {
            throw new NotImplementedException();
        }

        public virtual ICoordinateSystemEntity CoordinateSystemByOrigin(double x, double y)
        {
            throw new NotImplementedException();
        }

        public virtual ICoordinateSystemEntity CoordinateSystemByOrigin(double x, double y, double z)
        {
            throw new NotImplementedException();
        }

        public virtual ICoordinateSystemEntity CoordinateSystemByOrigin(IPointEntity origin)
        {
            throw new NotImplementedException();
        }

        public virtual ICoordinateSystemEntity CoordinateSystemByPlane(IPlaneEntity plane)
        {
            throw new NotImplementedException();
        }

        public virtual ICoordinateSystemEntity CoordinateSystemByOriginVectors(IPointEntity origin, IVectorEntity xAxis, IVectorEntity yAxis)
        {
            throw new NotImplementedException();
        }

        public virtual ICoordinateSystemEntity CoordinateSystemByOriginVectors(IPointEntity origin, IVectorEntity xAxis, IVectorEntity yAxis, IVectorEntity zAxis)
        {
            throw new NotImplementedException();
        }

        public virtual ICoordinateSystemEntity CoordinateSystemByCylindricalCoordinates(ICoordinateSystemEntity iCoordinateSystemEntity, double radius, double theta, double height)
        {
            throw new NotImplementedException();
        }

        public virtual ICoordinateSystemEntity CoordinateSystemBySphericalCoordinates(ICoordinateSystemEntity iCoordinateSystemEntity, double radius, double theta, double phi)
        {
            throw new NotImplementedException();
        }

        public virtual ICuboidEntity CuboidByLengths(double width, double length, double height)
        {
            throw new NotImplementedException();
        }

        public virtual ICuboidEntity CuboidByLengths(IPointEntity originPoint, double width, double length, double height)
        {
            throw new NotImplementedException();
        }

        public virtual ICuboidEntity CuboidByLengths(ICoordinateSystemEntity cs, double width, double length, double height)
        {
            throw new NotImplementedException();
        }

        public virtual ICuboidEntity CuboidByCorners(IPointEntity lowPoint, IPointEntity highPoint)
        {
            throw new NotImplementedException();
        }

        public virtual IUVEntity UVByCoordinates(double u, double v)
        {
            throw new NotImplementedException();
        }

        public virtual ICurveEntity CurveByParameterLineOnSurface(ISurfaceEntity baseSurface, IUVEntity startParams, IUVEntity endParams)
        {
            throw new NotImplementedException();
        }

        public virtual IEllipseEntity EllipseByOriginRadii(IPointEntity origin, double xAxisRadius, double yAxisRadius)
        {
            throw new NotImplementedException();
        }

        public virtual IEllipseEntity EllipseByOriginVectors(IPointEntity origin, IVectorEntity xAxisRadius, IVectorEntity yAxisRadius)
        {
            throw new NotImplementedException();
        }

        public virtual IEllipseEntity EllipseByCoordinateSystemRadii(ICoordinateSystemEntity origin, double xAxisRadius, double yAxisRadius)
        {
            throw new NotImplementedException();
        }

        public virtual IEllipseEntity EllipseByPlaneRadii(IPlaneEntity plane, double xAxisRadius, double yAxisRadius)
        {
            throw new NotImplementedException();
        }

        public virtual IHelixEntity HelixByAxis(IPointEntity axisPoint, IVectorEntity axisDirection, IPointEntity startPoint, double pitch, double angleTurns)
        {
            throw new NotImplementedException();
        }

        public virtual ILineEntity LineByStartPointEndPoint(IPointEntity startPoint, IPointEntity endPoint)
        {
            throw new NotImplementedException();
        }

        public virtual ILineEntity LineByBestFit(IPointEntity[] bestFitPoints)
        {
            throw new NotImplementedException();
        }

        public virtual ILineEntity LineByTangency(ICurveEntity curve, double parameter)
        {
            throw new NotImplementedException();
        }

        public virtual INurbsCurveEntity NurbsCurveByControlVertices(IPointEntity[] points)
        {
            throw new NotImplementedException();
        }

        public virtual INurbsCurveEntity NurbsCurveByControlVertices(IPointEntity[] points, int degree)
        {
            throw new NotImplementedException();
        }

        public virtual INurbsCurveEntity NurbsCurveByControlVertices(IPointEntity[] points, int degree, bool close_curve)
        {
            throw new NotImplementedException();
        }

        public virtual INurbsCurveEntity NurbsCurveByControlVertices(IPointEntity[] points, int degree, double[] weights, double[] knots)
        {
            throw new NotImplementedException();
        }

        public virtual INurbsCurveEntity NurbsCurveByPoints(IPointEntity[] points)
        {
            throw new NotImplementedException();
        }

        public virtual INurbsCurveEntity NurbsCurveByPoints(IPointEntity[] hosts, bool makePeriodic)
        {
            throw new NotImplementedException();
        }

        public virtual INurbsCurveEntity NurbsCurveByPoints(IPointEntity[] points, int degree)
        {
            throw new NotImplementedException();
        }

        public virtual INurbsCurveEntity NurbsCurveByPointsTangents(IPointEntity[] pts, IVectorEntity startTangent, IVectorEntity endTangent)
        {
            throw new NotImplementedException();
        }

        public virtual INurbsSurfaceEntity NurbsSurfaceByPoints(IPointEntity[][] points, int uDegree, int vDegree)
        {
            throw new NotImplementedException();
        }

        public virtual INurbsSurfaceEntity NurbsSurfaceByControlVertices(IPointEntity[][] controlVertices, int uDegree, int vDegree)
        {
            throw new NotImplementedException();
        }

        public virtual INurbsSurfaceEntity NurbsSurfaceByControlVertices(IPointEntity[][] controlVertices, int uDegree, int vDegree, double[][] weights, double[][] knots)
        {
            throw new NotImplementedException();
        }

        public virtual IPlaneEntity PlaneByOriginNormal(IPointEntity origin, IVectorEntity normal)
        {
            throw new NotImplementedException();
        }

        public virtual IPlaneEntity PlaneByOriginNormalXAxis(IPointEntity origin, IVectorEntity normal, IVectorEntity xAxis)
        {
            throw new NotImplementedException();
        }

        public virtual IPlaneEntity PlaneByOriginXAxisYAxis(IPointEntity origin, IVectorEntity xAxis, IVectorEntity yAxis)
        {
            throw new NotImplementedException();
        }

        public virtual IPlaneEntity PlaneByBestFitThroughPoints(IPointEntity[] points)
        {
            throw new NotImplementedException();
        }

        public virtual IPlaneEntity PlaneByLineAndPoint(ILineEntity line, IPointEntity point)
        {
            throw new NotImplementedException();
        }

        public virtual IPlaneEntity PlaneByThreePoints(IPointEntity p1, IPointEntity p2, IPointEntity p3)
        {
            throw new NotImplementedException();
        }

        public virtual IPointEntity PointByCoordinates(double x, double y)
        {
            throw new NotImplementedException();
        }

        public virtual IPointEntity PointByCoordinates(double x, double y, double z)
        {
            throw new NotImplementedException();
        }

        public virtual IPointEntity PointByCartesianCoordinates(ICoordinateSystemEntity cs, double x, double y, double z)
        {
            throw new NotImplementedException();
        }

        public virtual IPointEntity PointByCylindricalCoordinates(ICoordinateSystemEntity cs, double angle, double elevation, double offset)
        {
            throw new NotImplementedException();
        }

        public virtual IPointEntity PointBySphericalCoordinates(ICoordinateSystemEntity cs, double phi, double theta, double radius)
        {
            throw new NotImplementedException();
        }

        public virtual IPolygonEntity PolygonByPoints(IPointEntity[] points)
        {
            throw new NotImplementedException();
        }

        public virtual IPolyCurveEntity PolyCurveByJoinedCurves(ICurveEntity[] curves)
        {
            throw new NotImplementedException();
        }

        public virtual IRectangleEntity RectangleByCornerPoints(IPointEntity[] points)
        {
            throw new NotImplementedException();
        }

        public virtual IRectangleEntity RectangleByCornerPoints(IPointEntity p1, IPointEntity p2, IPointEntity p3, IPointEntity p4)
        {
            throw new NotImplementedException();
        }

        public virtual IRectangleEntity RectangleByWidthHeight(double width, double length)
        {
            throw new NotImplementedException();
        }

        public virtual IRectangleEntity RectangleByWidthHeight(IPlaneEntity plane, double width, double length)
        {
            throw new NotImplementedException();
        }

        public virtual IRectangleEntity RectangleByWidthHeight(ICoordinateSystemEntity cs, double width, double length)
        {
            throw new NotImplementedException();
        }

        public virtual ISurfaceEntity SurfaceByLoft(ICurveEntity[] crossSections)
        {
            throw new NotImplementedException();
        }

        public virtual ISurfaceEntity SurfaceByLoft(ICurveEntity[] crossSections, ICurveEntity guideCurve)
        {
            throw new NotImplementedException();
        }

        public virtual ISurfaceEntity SurfaceByLoftGuides(ICurveEntity[] hostXCurves, ICurveEntity[] hostGuides)
        {
            throw new NotImplementedException();
        }

        public virtual ISurfaceEntity SurfaceBySweep(ICurveEntity iCurveEntity, ICurveEntity iCurveEntity_2)
        {
            throw new NotImplementedException();
        }

        public virtual ISurfaceEntity SurfaceBySweep2Rails(ICurveEntity rail1, ICurveEntity rail2, ICurveEntity profile)
        {
            throw new NotImplementedException();
        }

        public virtual ISurfaceEntity SurfaceByRevolve(ICurveEntity profile, IPointEntity axisOrigin, IVectorEntity axisDirection, double startAngle, double sweepAngle)
        {
            throw new NotImplementedException();
        }

        public virtual ISurfaceEntity SurfaceByPatch(ICurveEntity iCurveEntity)
        {
            throw new NotImplementedException();
        }

        public virtual ISolidEntity SolidByLoft(ICurveEntity[] crossSections)
        {
            throw new NotImplementedException();
        }

        public virtual ISolidEntity SolidByLoft(ICurveEntity[] crossSections, ICurveEntity guideCurve)
        {
            throw new NotImplementedException();
        }

        public virtual ISolidEntity SolidByLoftGuides(ICurveEntity[] xsections, ICurveEntity[] iCurveEntity)
        {
            throw new NotImplementedException();
        }

        public virtual ISolidEntity SolidBySweep(ICurveEntity iCurveEntity, ICurveEntity iCurveEntity_2)
        {
            throw new NotImplementedException();
        }

        public virtual ISolidEntity SolidBySweep2Rails(ICurveEntity rail1, ICurveEntity rail2, ICurveEntity profile)
        {
            throw new NotImplementedException();
        }

        public virtual ISolidEntity SolidByRevolve(ICurveEntity profile, IPointEntity axisOrigin, IVectorEntity axisDirection, double startAngle, double sweepAngle)
        {
            throw new NotImplementedException();
        }

        public virtual ISphereEntity SphereByCenterPointRadius(IPointEntity centerPoint, double radius)
        {
            throw new NotImplementedException();
        }

        public virtual ITextEntity TextByPoint(IPointEntity origin, string textString, double textHeight)
        {
            throw new NotImplementedException();
        }

        public virtual ITextEntity TextByCoordinateSystem(ICoordinateSystemEntity cs, string textString, double textHeight)
        {
            throw new NotImplementedException();
        }

        public virtual IPolyMeshEntity PolyMeshByVerticesFaceIndices(IPointEntity[] vertices, IIndexGroup[] indices)
        {
            throw new NotImplementedException();
        }

        public virtual IBlockHelper GetBlockHelper()
        {
            throw new NotImplementedException();
        }

        public virtual IGeometryEntity[] LoadSat(string satFile)
        {
            throw new NotImplementedException();
        }

        public virtual bool SaveSat(string satFile, object[] ffiObjects)
        {
            throw new NotImplementedException();
        }

        public virtual IGeometrySettings GetSettings()
        {
            throw new NotImplementedException();
        }
    }

    [Browsable(false)]
    public abstract class AbstractPersistenceManager : IPersistenceManager
    {
        public virtual IPersistentObject Persist(IDesignScriptEntity entity)
        {
            throw new NotImplementedException("Persist method is not implemented by PersistenceManager.");
        }

        public virtual void UpdateDisplay()
        {
            throw new NotImplementedException("UpdateDisplay method is not implemented by PersistenceManager.");
        }

        public virtual IPersistentObject GetPersistentObjectFromHandle(object handle)
        {
            throw new NotImplementedException("GetPersistentObjectFromHandle method is not implemented by PersistenceManager.");
        }

        public virtual IGeometryFactory GeometryFactory
        {
            get;
            set;
        }

        public virtual bool SupportsGeometryCapture()
        {
            return false;
        }

        public virtual IDesignScriptEntity[] CaptureGeometry()
        {
            throw new NotImplementedException("CaptureGeometry method is not implemented by PersistenceManager.");
        }

        public virtual IPersistentObject FromObject(long ptr)
        {
            throw new NotImplementedException("FromObject method is not implemented by PersistenceManager.");
        }
    }
}
