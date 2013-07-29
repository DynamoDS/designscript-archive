using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;

namespace DSGeometry
{
    class PointEntity : GeometryEntity, IPointEntity
    {
        internal PointEntity()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }
        internal PointEntity(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
        internal IVector directionTo(IPointEntity point)
        {
            return DsVector.ByCoordinates(point.X - this.X, point.Y - this.Y, point.Z - this.Z);
        }
        public double X
        {
            get;
            set;
        }

        public double Y
        {
            get;
            set;
        }

        public double Z
        {
            get;
            set;
        }
        public override double DistanceTo(IPointEntity point)
        {
            return Math.Sqrt(((this.X - point.X) * (this.X - point.X)) + ((this.Y - point.Y) * (this.Y - point.Y)) + ((this.Z - point.Z) * (this.Z - point.Z)));
        }

        public override IPointEntity GetClosestPoint(IPointEntity otherPoint)
        {
            return this;
        }
    }
}
