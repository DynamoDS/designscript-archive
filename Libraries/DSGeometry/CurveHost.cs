using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;

namespace DSGeometry
{
    abstract class CurveEntity : GeometryEntity, ICurveEntity
    {
        public virtual double DistanceAtParameter(double param)
        {
            return param * 2;
        }

        public virtual double ParameterAtDistance(double distance)
        {
            return distance * 0.5;
        }

        public virtual double EndParameter()
        {
            return 1;
        }

        public IPointEntity EndPoint
        {
            get;
            protected set;
        }

        public virtual ISurfaceEntity Extrude(IVector direction, double distance)
        {
            return new SurfaceEntity();
        }

        public virtual IPointEntity GetClosestPointTo(ICurveEntity iCurveEntity)
        {
            return this.StartPoint;
        }

        public virtual IPointEntity GetClosestPointTo(IPointEntity point, IVector direction, bool extend)
        {
            return this.StartPoint;
        }

        public virtual IPointEntity GetClosestPointTo(IPointEntity point, bool extend)
        {
            return this.StartPoint;
        }

        public virtual double GetLength()
        {
            return 1;
        }

        public virtual ICurveEntity GetOffsetCurve(double distance)
        {
            return new LineEntity();
        }

        public virtual IPointEntity[] IntersectWith(ICurveEntity otherCurve)
        {
            return new IPointEntity[2] { new PointEntity(), new PointEntity() };
        }

        public virtual IGeometryEntity[] IntersectWith(IPlaneEntity plane)
        {
            return new IGeometryEntity[2] { new GeometryEntity(), new GeometryEntity() };
        }

        public virtual IGeometryEntity[] IntersectWith(ISurfaceEntity surface)
        {
            return new IGeometryEntity[2] { new GeometryEntity(), new GeometryEntity() };
        }

        public virtual IGeometryEntity[] IntersectWith(ISolidEntity solid)
        {
            return new IGeometryEntity[2] { new GeometryEntity(), new GeometryEntity() };
        }

        public abstract bool IsClosed
        {
            get;
        }

        public abstract bool IsPlanar
        {
            get; 
        }

        public virtual ICurveEntity[] JoinWith(ICurveEntity[] curves, double bridgeTolerance)
        {
            return new ICurveEntity[3] { new LineEntity(), new LineEntity(), new LineEntity() };
        }

        public virtual IBSplineCurveEntity JoinWith(ICurveEntity[] curves)
        {
            return new BSplineCurveEntity();
        }

        public virtual IVector NormalAtParameter(double param)
        {
            return DsVector.ByCoordinates(param, param, param);
        }

        public virtual double ParameterAtPoint(IPointEntity point)
        {
            return 1;
        }

        public virtual IPointEntity PointAtDistance(double distance)
        {
            return new PointEntity();
        }

        public virtual IPointEntity PointAtParameter(double param)
        {
            return new PointEntity();
        }

        public virtual IPointEntity Project(IPointEntity point, IVector direction)
        {
            return new PointEntity();
        }

        public virtual IGeometryEntity[] ProjectOn(ISurfaceEntity surface, IVector direction)
        {
            return new IGeometryEntity[2] { new GeometryEntity(), new GeometryEntity() };
        }

        public virtual ICurveEntity ProjectOn(IPlaneEntity plane, IVector direction)
        {
            return new LineEntity();
        }

        public virtual ICurveEntity Reverse()
        {
            return new LineEntity();
        }

        public virtual ICurveEntity[] Split(double[] parameters)
        {
            return new ICurveEntity[3] { new LineEntity(), new LineEntity(), new LineEntity() };
        }

        public virtual double StartParameter()
        {
            return 0;
        }

        public virtual IPointEntity StartPoint
        {
            get;
            protected set;
        }

        public virtual IVector TangentAtParameter(double param)
        {
            if(null != StartPoint && null != EndPoint)
                return DsVector.ByCoordinates(EndPoint.X - StartPoint.X, EndPoint.Y - StartPoint.Y, EndPoint.Z - StartPoint.Z);

            return DsVector.ByCoordinates(param, param, param);
        }

        public virtual ICurveEntity[] Trim(double startParameter, double endParameter, bool discardBetweenParams)
        {
            return new ICurveEntity[3] { new LineEntity(), new LineEntity(), new LineEntity() };
        }

        public virtual ICurveEntity[] Trim(double[] parameters, bool discardEvenSegments)
        {
            return new ICurveEntity[3] { new LineEntity(), new LineEntity(), new LineEntity() };
        }
    }
}
