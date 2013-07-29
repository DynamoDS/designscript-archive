using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;
namespace DSGeometry
{
    public class GeometryFactory : IGeometryFactory
    {
        public IArcEntity ArcByCenterPointRadiusAngle(IPointEntity center, double radius, double startAngle, double endAngle, IVector normal)
        {
            DSGeometryApplication.Check();
            double sweepAngle = endAngle - startAngle;
            return new ArcEntity(center, normal, radius, startAngle, sweepAngle);
        }

        public IArcEntity ArcByCenterPointStartPointSweepAngle(IPointEntity centerPoint, IPointEntity startPoint, double sweepAngle, IVector normal)
        {
            DSGeometryApplication.Check();
            double radius = startPoint.DistanceTo(centerPoint);
            double startAngle = 30;
            return new ArcEntity(centerPoint, normal, radius, startAngle, sweepAngle);
        }

        public IArcEntity ArcByCenterPointStartPointSweepPoint(IPointEntity centerPoint, IPointEntity startPoint, IPointEntity sweepPoint)
        {
            DSGeometryApplication.Check();
            Vector start_center = Vector.ByCoordinates(startPoint.X - centerPoint.X, startPoint.Y - centerPoint.Y, startPoint.Z - centerPoint.Z);
            Vector sweep_center = Vector.ByCoordinates(sweepPoint.X - centerPoint.X, sweepPoint.Y - centerPoint.Y, sweepPoint.Z - centerPoint.Z);
            Vector normal = start_center.Cross(sweep_center);
            double radius = start_center.GetLength();
            Vector Axis = Vector.ByCoordinates(1, 0, 0);
            double startAngle = Math.Acos(start_center.Dot(Axis) / (start_center.GetLength() * Axis.GetLength()));
            double sweepAngle = Math.Acos(start_center.Dot(sweep_center) / (start_center.GetLength() * sweep_center.GetLength()));
            return new ArcEntity(centerPoint, normal.ToIVector(), radius, startAngle, sweepAngle);
        }

        public IArcEntity ArcByPointsOnCurve(IPointEntity firstPoint, IPointEntity secondPoint, IPointEntity thirdPoint)
        {
            DSGeometryApplication.Check();
            return new ArcEntity();
        }

        public IBSplineCurveEntity BSplineByControlVertices(IPointEntity[] controlVertices, int degree, bool makePeriodic)
        {
            DSGeometryApplication.Check();
            return new BSplineCurveEntity(controlVertices,degree,makePeriodic);
        }

        public IBSplineCurveEntity BSplineByPoints(IPointEntity[] pts, IVector startTangent, IVector endTangent)
        {
            DSGeometryApplication.Check();
            return new BSplineCurveEntity();
        }

        public IBSplineCurveEntity BSplineByPoints(IPointEntity[] pts, bool makePeriodic)
        {
            DSGeometryApplication.Check();
            return new BSplineCurveEntity();
        }

        public IBSplineSurfaceEntity BSplineSurfaceByControlVertices(IPointEntity[][] controlVertices, int uDegree, int vDegree)
        {
            DSGeometryApplication.Check();
            IBSplineSurfaceEntity surface = new BSplineSurfaceEntity();
            surface.UpdateByPoles(controlVertices, uDegree, vDegree);
            return surface;
        }

        public IBSplineSurfaceEntity BSplineSurfaceByPoints(IPointEntity[][] points, int uDegree, int vDegree)
        {
            DSGeometryApplication.Check();
            IBSplineSurfaceEntity surface = new BSplineSurfaceEntity();
            surface.UpdateByPoints(points, uDegree, vDegree);
            return surface;
        }

        public ICircleEntity CircleByCenterPointRadius(IPointEntity center, double radius, IVector normal)
        {
            DSGeometryApplication.Check();
            return new CircleEntity(center, radius, normal);
        }

        public ICircleEntity CircleByPointsOnCurve(IPointEntity firstPoint, IPointEntity secondPoint, IPointEntity thirdPoint)
        {
            DSGeometryApplication.Check();
            return new CircleEntity();
        }

        public IConeEntity ConeByPointsRadius(IPointEntity startPoint, IPointEntity endPoint, double startRadius, double endRadius)
        {
            DSGeometryApplication.Check();
            ConeEntity cone = new ConeEntity();
            cone.UpdateCone(startPoint, endPoint, startRadius, endRadius);
            return cone;
        }

        public IConeEntity ConeByRadiusLength(ICoordinateSystemEntity cs, double startRadius, double endRadius, double height)
        {
            DSGeometryApplication.Check();
            ConeEntity cone = new ConeEntity();
            IPointEntity startPoint = cs.Origin;
            IPointEntity endPoint = new PointEntity(cs.Origin.X, cs.Origin.Y, cs.Origin.Z + height);
            cone.UpdateCone(startPoint, endPoint, startRadius, endRadius);
            return cone;
        }

        public ICoordinateSystemEntity CoordinateSystemByCylindricalCoordinates(ICoordinateSystemEntity contextCS, double radius, double theta, double height)
        {
            DSGeometryApplication.Check();
            CoordinateEntity cs = new CoordinateEntity();
            cs.Set(new PointEntity() { X = radius * Math.Cos(DegreeToRadian(theta)), Y = radius * Math.Sin(DegreeToRadian(theta)), Z = height }, contextCS.XAxis, contextCS.YAxis, contextCS.ZAxis);
            return cs;
           // return new CoordinateEntity() { Origin = new PointEntity() { X = radius*Math.Sin(theta), Y = radius*Math.Cos(theta), Z = height } };
           // return new CoordinateEntity();
        }

        public ICoordinateSystemEntity CoordinateSystemByData(double[] data)
        {
            DSGeometryApplication.Check();
            return new CoordinateEntity();
        }

        public ICoordinateSystemEntity CoordinateSystemBySphericalCoordinates(ICoordinateSystemEntity contextCS, double radius, double theta, double phi)
        {
            DSGeometryApplication.Check();
            CoordinateEntity cs = new CoordinateEntity();
            cs.Set(new PointEntity() { X = radius * Math.Sin(DegreeToRadian(theta)) * Math.Cos(DegreeToRadian(phi)), Y = radius * Math.Sin(DegreeToRadian(theta)) * Math.Sin(DegreeToRadian(phi)), Z = radius * Math.Cos(DegreeToRadian(theta)) }, contextCS.XAxis, contextCS.YAxis, contextCS.ZAxis);
            return cs;
        }

        public ICoordinateSystemEntity CoordinateSystemByUniversalTransform(ICoordinateSystemEntity contextCoordinateSys, double[] scaleFactors, double[] rotationAngles, int[] rotationSequence, double[] translationVector, bool translationSequence)
        {
            DSGeometryApplication.Check();
            return new CoordinateEntity();
        }

        public ICoordinateSystemEntity CoordinateSystemByUniversalTransform(ICoordinateSystemEntity contextCoordinateSys, double[] scaleFactors, double[] rotationAngles, int[] rotationSequence, IVector translationVector, bool translationSequence)
        {
            DSGeometryApplication.Check();
            return new CoordinateEntity();
        }

        public IPointEntity CreatePoint(double x, double y, double z)
        {
            DSGeometryApplication.Check();
            return new PointEntity(x, y, z);
        }

        public ICuboidEntity CuboidByLengths(ICoordinateSystemEntity cs, double length, double width, double height)
        {
            DSGeometryApplication.Check();
            ICuboidEntity cub = new CuboidEntity();
            cub.UpdateCuboid(new double[] { cs.Origin.X, cs.Origin.Y, cs.Origin.Z }, length, width, height);
            return cub;
        }

        public ILineEntity LineByStartPointEndPoint(IPointEntity startPoint, IPointEntity endPoint)
        {
            DSGeometryApplication.Check();
            LineEntity lnhost = new LineEntity();
            lnhost.UpdateEndPoints(startPoint, endPoint);
            return lnhost;
        }

        public IPlaneEntity PlaneByOriginNormal(IPointEntity origin, IVector normal)
        {
            DSGeometryApplication.Check();
            IPlaneEntity plane = new PlaneEntity();
            plane.Update(origin, normal);
            return plane;
        }

        public IPointEntity PointByCartesianCoordinates(ICoordinateSystemEntity cs, double x, double y, double z)
        {
            DSGeometryApplication.Check();
            return new PointEntity(cs.Origin.X + x,cs.Origin.Y + y,cs.Origin.Z + z);
        }

        public IPolygonEntity PolygonByVertices(IPointEntity[] vertices)
        {
            DSGeometryApplication.Check();
            return new PolygonEntity();
        }

        public ISolidEntity SolidByLoftCrossSections(ICurveEntity[] crossSections)
        {
            DSGeometryApplication.Check();
            return new SolidEntity();
        }

        public ISolidEntity SolidByLoftCrossSectionsGuides(ICurveEntity[] crossSections, ICurveEntity[] guides)
        {
            DSGeometryApplication.Check();
            return new SolidEntity();
        }

        public ISolidEntity SolidByLoftCrossSectionsPath(ICurveEntity[] crossSections, ICurveEntity path)
        {
            DSGeometryApplication.Check();
            return new SolidEntity();
        }

        public ISolidEntity SolidByRevolve(ICurveEntity profile, IPointEntity axisOrigin, IVector axisDirection, double startAngle, double sweepAngle)
        {
            DSGeometryApplication.Check();
            return new SolidEntity();
        }

        public ISolidEntity SolidBySweep(ICurveEntity profile, ICurveEntity path)
        {
            DSGeometryApplication.Check();
            return new SolidEntity();
        }

        public ISphereEntity SphereByCenterPointRadius(IPointEntity centerPoint, double radius)
        {
            DSGeometryApplication.Check();
            ISphereEntity sph = new SphereEntity();
            sph.UpdateSphere(centerPoint, radius);
            return sph;
        }

        public ISubDMeshEntity SubDMeshByVerticesFaceIndices(IPointEntity[] points, int[][] faceIndices, IVector[] vertexNormals, IColor[] vertexColors, int subDLevel)
        {
            DSGeometryApplication.Check();
            return new SubDMeshEntity();
        }

        public ISubDMeshEntity SubDMeshByVerticesFaceIndices(IPointEntity[] vertices, int[][] faceIndices, int subDLevel)
        {
            DSGeometryApplication.Check();
            return new SubDMeshEntity();
        }

        public ISurfaceEntity SurfaceByLoftCrossSections(ICurveEntity[] crossSections)
        {
            DSGeometryApplication.Check();
            return new SurfaceEntity();
        }

        public ISurfaceEntity SurfaceByLoftCrossSectionsGuides(ICurveEntity[] crossSections, ICurveEntity[] guides)
        {
            DSGeometryApplication.Check();
            return new SurfaceEntity();
        }

        public ISurfaceEntity SurfaceByLoftCrossSectionsPath(ICurveEntity[] crossSections, ICurveEntity path)
        {
            DSGeometryApplication.Check();
            return new SurfaceEntity();
        }

        public ISurfaceEntity SurfaceByRevolve(ICurveEntity profile, IPointEntity axisOrigin, IVector axisDirection, double startAngle, double sweepAngle)
        {
            DSGeometryApplication.Check();
            return new SurfaceEntity();
        }

        public ISurfaceEntity SurfaceBySweep(ICurveEntity profile, ICurveEntity path)
        {
            DSGeometryApplication.Check();
            return new SurfaceEntity();
        }

        public ISurfaceEntity SurfacePatchFromCurve(ICurveEntity profile)
        {
            DSGeometryApplication.Check();
            return new SurfaceEntity();
        }

        public ITextEntity TextByCoordinateSystem(ICoordinateSystemEntity contextCoordinateSystem, int orientation, string textString, double fontSize)
        {
            DSGeometryApplication.Check();
            return new TextEntity();
        }
        internal static double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }


        public IBlockHelper GetBlockHelper()
        {
            throw new NotImplementedException();
        }
        
        public ISubDMeshEntity SubDMeshFromGeometry(IGeometryEntity geometry, double maxEdgeLength)
        {
            DSGeometryApplication.Check();
            return new SubDMeshEntity();
        }

        public IGeometrySettings GetSettings()
        {
            return new Settings();
        }


        public IGeometryEntity[] LoadSat(string satFile)
        {
            return new IGeometryEntity[] {new SolidEntity(), new SolidEntity(), new SolidEntity()};
        }

        public bool SaveSat(string satFile, object[] ffiObjects)
        {
            return false;
        }

        public IMeshEntity MeshByVerticesFaceIndices(IPointEntity[] vertices, int[][] faceIndices)
        {
            return new MeshEntity();
        }

        public IMeshEntity MeshByVerticesEdgeIndices(IPointEntity[] vertices, int[] edgeIndices)
        {
            return new MeshEntity();
        }
    }

    class Settings : IGeometrySettings
    {
        public bool PointVisibility { get; set; }
    }
}
