using System;
using Autodesk.DesignScript.Interfaces;
using System.ComponentModel;

namespace Autodesk.DesignScript.Geometry
{
    [Browsable(false)]
    public abstract class GeometryFactory : IGeometryFactory
    {
        public virtual ICoordinateSystemEntity CoordinateSystemByData(double[] data)
        {
            throw new NotImplementedException("Factory method CoordinateSystemByData not implemented");
        }

        public virtual ICoordinateSystemEntity CoordinateSystemByCylindricalCoordinates(ICoordinateSystemEntity contextCS, double radius, double theta, double height)
        {
            throw new NotImplementedException("Factory method CoordinateSystemByCylindricalCoordinates not implemented");
        }

        public virtual ICoordinateSystemEntity CoordinateSystemBySphericalCoordinates(ICoordinateSystemEntity contextCS, double radius, double theta, double phi)
        {
            throw new NotImplementedException("Factory method CoordinateSystemBySphericalCoordinates not implemented");
        }

        public virtual ICoordinateSystemEntity CoordinateSystemByUniversalTransform(ICoordinateSystemEntity contextCoordinateSys, double[] scaleFactors,
            double[] rotationAngles, int[] rotationSequence, IVector translationVector, bool translationSequence)
        {
            throw new NotImplementedException("Factory method CoordinateSystemByUniversalTransform not implemented");
        }

        public virtual ICoordinateSystemEntity CoordinateSystemByUniversalTransform(ICoordinateSystemEntity contextCoordinateSys, double[] scaleFactors,
            double[] rotationAngles, int[] rotationSequence, double[] translationVector, bool translationSequence)
        {
            throw new NotImplementedException("Factory method CoordinateSystemByUniversalTransform not implemented");
        }

        public virtual IPointEntity CreatePoint(double x, double y, double z)
        {
            throw new NotImplementedException("Factory method CoCreatePointordinateSystemByData not implemented");
        }

        public virtual IBSplineCurveEntity BSplineByControlVertices(IPointEntity[] controlVertices, int degree, bool makePeriodic)
        {
            throw new NotImplementedException("Factory method BSplineByControlVertices not implemented");
        }

        public virtual IBSplineCurveEntity BSplineByPoints(IPointEntity[] pts, bool makePeriodic)
        {
            throw new NotImplementedException("Factory method BSplineByPoints not implemented");
        }

        public virtual IBSplineCurveEntity BSplineByPoints(IPointEntity[] pts, IVector startTangent, IVector endTangent)
        {
            throw new NotImplementedException("Factory method BSplineByPoints not implemented");
        }

        public virtual IArcEntity ArcByPointsOnCurve(IPointEntity firstPoint, IPointEntity secondPoint, IPointEntity thirdPoint)
        {
            throw new NotImplementedException("Factory method ArcByPointsOnCurve not implemented");
        }

        public virtual IArcEntity ArcByCenterPointRadiusAngle(IPointEntity center, double radius, double startAngle, double sweepAngle, IVector normal)
        {
            throw new NotImplementedException("Factory method ArcByCenterPointRadiusAngle not implemented");
        }

        public virtual IArcEntity ArcByCenterPointStartPointSweepAngle(IPointEntity centerPoint, IPointEntity startPoint, double sweepAngle, IVector normal)
        {
            throw new NotImplementedException("Factory method ArcByCenterPointStartPointSweepAngle not implemented");
        }

        public virtual IArcEntity ArcByCenterPointStartPointSweepPoint(IPointEntity centerPoint, IPointEntity startPoint, IPointEntity sweepPoint)
        {
            throw new NotImplementedException("Factory method ArcByCenterPointStartPointSweepPoint not implemented");
        }


        public virtual ILineEntity LineByStartPointEndPoint(IPointEntity startPoint, IPointEntity endPoint)
        {
            throw new NotImplementedException("Factory method LineByStartPointEndPoint not implemented");
        }

        public virtual IPlaneEntity PlaneByOriginNormal(IPointEntity origin, IVector normal)
        {
            throw new NotImplementedException("Factory method PlaneByOriginNormal not implemented");
        }

        public virtual IPointEntity PointByCartesianCoordinates(ICoordinateSystemEntity cs, double x, double y, double z)
        {
            throw new NotImplementedException("Factory method PointByCartesianCoordinates not implemented");
        }

        public virtual ICircleEntity CircleByCenterPointRadius(IPointEntity center, double radius, IVector normal)
        {
            throw new NotImplementedException("Factory method CircleByCenterPointRadius not implemented");
        }

        public virtual ICircleEntity CircleByPointsOnCurve(IPointEntity firstPoint, IPointEntity secondPoint, IPointEntity thirdPoint)
        {
            throw new NotImplementedException("Factory method CircleByPointsOnCurve not implemented");
        }

        public virtual ISurfaceEntity SurfaceByLoftCrossSections(ICurveEntity[] crossSections)
        {
            throw new NotImplementedException("Factory method SurfaceByLoftCrossSections not implemented");
        }

        public virtual ISurfaceEntity SurfaceByLoftCrossSectionsPath(ICurveEntity[] crossSections, ICurveEntity path)
        {
            throw new NotImplementedException("Factory method SurfaceByLoftCrossSectionsPath not implemented");
        }

        public virtual ISurfaceEntity SurfaceByLoftCrossSectionsGuides(ICurveEntity[] crossSections, ICurveEntity[] guides)
        {
            throw new NotImplementedException("Factory method SurfaceByLoftCrossSectionsGuides not implemented");
        }

        public virtual ISurfaceEntity SurfaceBySweep(ICurveEntity profile, ICurveEntity path)
        {
            throw new NotImplementedException("Factory method SurfaceBySweep not implemented");
        }

        public virtual ISurfaceEntity SurfaceByRevolve(ICurveEntity profile, IPointEntity axisOrigin, IVector axisDirection, double startAngle, double sweepAngle)
        {
            throw new NotImplementedException("Factory method SurfaceByRevolve not implemented");
        }

        public virtual ISurfaceEntity SurfacePatchFromCurve(ICurveEntity profile)
        {
            throw new NotImplementedException("Factory method SurfacePatchFromCurve not implemented");
        }

        public virtual IBSplineSurfaceEntity BSplineSurfaceByControlVertices(IPointEntity[][] controlVertices, int uDegree, int vDegree)
        {
            throw new NotImplementedException("Factory method BSplineSurfaceByControlVertices not implemented");
        }

        public virtual IBSplineSurfaceEntity BSplineSurfaceByPoints(IPointEntity[][] points, int uDegree, int vDegree)
        {
            throw new NotImplementedException("Factory method BSplineSurfaceByPoints not implemented");
        }

        public virtual ISolidEntity SolidByLoftCrossSections(ICurveEntity[] crossSections)
        {
            throw new NotImplementedException("Factory method SolidByLoftCrossSections not implemented");
        }

        public virtual ISolidEntity SolidByLoftCrossSectionsGuides(ICurveEntity[] crossSections, ICurveEntity[] guides)
        {
            throw new NotImplementedException("Factory method SolidByLoftCrossSectionsGuides not implemented");
        }

        public virtual ISolidEntity SolidByLoftCrossSectionsPath(ICurveEntity[] crossSections, ICurveEntity path)
        {
            throw new NotImplementedException("Factory method SolidByLoftCrossSectionsPath not implemented");
        }

        public virtual ISolidEntity SolidBySweep(ICurveEntity profile, ICurveEntity path)
        {
            throw new NotImplementedException("Factory method SolidBySweep not implemented");
        }

        public virtual ISolidEntity SolidByRevolve(ICurveEntity profile, IPointEntity axisOrigin, IVector axisDirection, double startAngle, double sweepAngle)
        {
            throw new NotImplementedException("Factory method SolidByRevolve not implemented");
        }

        public virtual IConeEntity ConeByPointsRadius(IPointEntity startPoint, IPointEntity endPoint, double startRadius, double endRadius)
        {
            throw new NotImplementedException("Factory method ConeByPointsRadius not implemented");
        }

        public virtual IConeEntity ConeByRadiusLength(ICoordinateSystemEntity cs, double startRadius, double endRadius, double height)
        {
            throw new NotImplementedException("Factory method ConeByRadiusLength not implemented");
        }

        public virtual ISphereEntity SphereByCenterPointRadius(IPointEntity centerPoint, double radius)
        {
            throw new NotImplementedException("Factory method SphereByCenterPointRadius not implemented");
        }

        public virtual ICuboidEntity CuboidByLengths(ICoordinateSystemEntity cs, double length, double width, double height)
        {
            throw new NotImplementedException("Factory method CuboidByLengths not implemented");
        }

        public virtual IPolygonEntity PolygonByVertices(IPointEntity[] vertices)
        {
            throw new NotImplementedException("Factory method PolygonByVertices not implemented");
        }

        public virtual ISubDMeshEntity SubDMeshByVerticesFaceIndices(IPointEntity[] points, int[][] faceIndices, int subDLevel)
        {
            throw new NotImplementedException("Factory method SubDMeshByVerticesFaceIndices not implemented");
        }

        public virtual ISubDMeshEntity SubDMeshByVerticesFaceIndices(IPointEntity[] points, int[][] faceIndices, IVector[] vertexNormals, IColor[] vertexColors, int subDLevel)
        {
            throw new NotImplementedException("Factory method SubDMeshByVerticesFaceIndices not implemented");
        }

        public virtual ISubDMeshEntity SubDMeshFromGeometry(IGeometryEntity geometry, double maxEdgeLength)
        {
            throw new NotImplementedException("Factory method SubDMeshFromGeometry not implemented");
        }

        public virtual ITextEntity TextByCoordinateSystem(ICoordinateSystemEntity parentCoordinateSystem, int orientation, string textString, double fontSize)
        {
            throw new NotImplementedException("Factory method TextByCoordinateSystem not implemented");
        }

        public virtual IBlockHelper GetBlockHelper()
        {
            throw new NotImplementedException("Factory method GetBlockHelper not implemented");
        }

        public virtual IGeometryEntity[] LoadSat(string satFile)
        {
            throw new NotImplementedException("Factory method LoadSat not implemented");
        }

        public virtual bool SaveSat(string satFile, Object[] ffiObjects)
        {
            throw new NotImplementedException("Factory method SaveSat not implemented");
        }

        public virtual IGeometrySettings GetSettings()
        {
            throw new NotImplementedException("Factory method GetSettings not implemented");
        }

        public virtual IMeshEntity MeshByVerticesFaceIndices(IPointEntity[] vertices, int[][] faceIndices)
        {
            throw new NotImplementedException("Factory method MeshByVerticesFaceIndices not implemented");
        }

        public virtual IMeshEntity MeshByVerticesEdgeIndices(IPointEntity[] vertices, int[] edgeIndices)
        {
            throw new NotImplementedException("Factory method MeshByVerticesEdgeIndices not implemented");
        }
    }

    [Browsable(false)]
    public abstract class PersistenceManager : IPersistenceManager
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
