using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;

namespace DSGeometry
{
    class BSplineSurfaceEntity : SurfaceEntity, IBSplineSurfaceEntity
    {
        internal BSplineSurfaceEntity()
        {
            this.UDegree = 1;
            this.VDegree = 2;
            this.Points = new IPointEntity[][] { new IPointEntity[] { new PointEntity(0, 0, 0), new PointEntity(1, 1, 1), new PointEntity(2, 2, 2) }, new IPointEntity[] { new PointEntity(2, 2, 2), new PointEntity(1, 1, 1), new PointEntity(0, 0, 0) } };
            this.Poles = new IPointEntity[][] {new IPointEntity[] { new PointEntity(2, 2, 2), new PointEntity(1, 1, 1), new PointEntity(0, 0, 0) }, new IPointEntity[] { new PointEntity(0, 0, 0), new PointEntity(1, 1, 1), new PointEntity(2, 2, 2) } };
            this.IsRational = false;
            this.IsPeriodicInU = false;
            this.IsPeriodicInV = false;
        }
        public bool GetIsPeriodicInU()
        {
            return IsPeriodicInU;
        }

        public bool GetIsPeriodicInV()
        {
            return IsPeriodicInV;
        }

        public bool GetIsRational()
        {
            return IsRational;
        }

        public ICurveEntity[] GetIsolines(int isoDirection, double parameter)
        {
            return new ICurveEntity[3] { new LineEntity(), new LineEntity(), new LineEntity() };
        }

        public int GetNumPolesAlongU()
        {
            return 1;
        }

        public int GetNumPolesAlongV()
        {
            return 2;
        }

        public IPointEntity[][] GetPoints()
        {
            return Points;
        }

        public IPointEntity[][] GetPoles()
        {
            return Poles;
        }

        public int GetUDegree()
        {
            return UDegree;
        }

        public double[] GetUKnots()
        {
            return new double[] { 1, 2, 3 };
        }

        public int GetVDegree()
        {
            return VDegree;
        }

        public double[] GetVKnots()
        {
            return new double[] { 3, 2, 1 };
        }

        public double[][] GetWeights()
        {
            return new double[][] { new double[]{ 1, 2, 3 }, new double[]{ 3, 2, 1 } };
        }

        public IVector NormalAtParameter(double u, double v)
        {
            return DsVector.ByCoordinates(u, v, u + v);
        }

        public bool UpdateByPoints(IPointEntity[][] points, int uDegree, int vDegree)
        {
            this.Points = points;
            this.UDegree = uDegree;
            this.VDegree = vDegree;
            return true;
        }

        public bool UpdateByPoles(IPointEntity[][] poles, int uDegree, int vDegree)
        {
            this.Poles = poles;
            this.UDegree = uDegree;
            this.VDegree = vDegree;
            return true;
        }

        public bool IsPeriodicInU { get; protected set; }

        public bool IsPeriodicInV { get; protected set; }

        public bool IsRational { get; protected set; }

        public IPointEntity[][] Points { get; protected set; }

        public IPointEntity[][] Poles { get; protected set; }

        public int UDegree { get; protected set; }

        public int VDegree { get; protected set; }
    }
}
