using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;

namespace DSGeometry
{
    class CoordinateEntity : DesignScriptEntity, ICoordinateSystemEntity
    {
        internal CoordinateEntity()
        {
            this.Origin = new PointEntity() { X = 0, Y = 0, Z = 0 };
            this.XAxis = DsVector.ByCoordinates(1, 0, 0);
            this.YAxis = DsVector.ByCoordinates(0, 1, 0);
            this.ZAxis = DsVector.ByCoordinates(0, 0, 1);
        }
        public IDisplayable Display
        {
            get { return null; }
        }

        public double GetDeterminant()
        {
            return 1.0;
        }

        public double[] GetScaleFactors()
        {
            return new double[] { 1, 1, 1 };
        }

        public ICoordinateSystemEntity Inverse()
        {
            return this;
        }

        public bool IsEqualTo(ICoordinateSystemEntity other)
        {
            return false;
        }

        public bool IsScaledOrtho()
        {
            return true;
        }

        public bool IsSingular()
        {
            return true;
        }

        public bool IsUniscaledOrtho()
        {
            return true;
        }
        
        public ICoordinateSystemEntity PostMultiplyBy(ICoordinateSystemEntity other)
        {
            return other;
        }

        public ICoordinateSystemEntity PreMultiplyBy(ICoordinateSystemEntity other)
        {
            return this;
        }

        public ICoordinateSystemEntity Rotation(double rotationAngle, IVector axis, IPointEntity origin)
        {
            return new CoordinateEntity();
        }

        public ICoordinateSystemEntity Scale(double scaleX, double scaleY, double scaleZ)
        {
            CoordinateEntity cs = new CoordinateEntity();
            cs.Set(this.Origin, this.XAxis.Scale(scaleX), this.YAxis.Scale(scaleY), this.ZAxis.Scale(scaleZ));
            return cs;
        }

        public void Set(IPointEntity origin, IVector xAxis, IVector yAxis, IVector zAxis)
        {
            this.Origin = origin;
            this.XAxis = xAxis;
            this.YAxis = yAxis;
            this.ZAxis = zAxis;
        }

        public ICoordinateSystemEntity Translate(IVector translationVector)
        {
            CoordinateEntity cs = new CoordinateEntity();
            PointEntity pt = new PointEntity() { X = cs.Origin.X + translationVector.X, Y = cs.Origin.Y + translationVector.Y, Z = cs.Origin.Z + translationVector.Z };
            cs.Set(pt, this.XAxis, this.YAxis, this.ZAxis);
            return cs;
        }

        public ICoordinateSystemEntity Transpose()
        {
            return this;
        }


        public IPointEntity Origin
        {
            get;
            protected set;
        }
        public IVector XAxis
        {
            get;
            protected set;
        }
        public IVector YAxis
        {
            get;
            protected set;
        }
        public IVector ZAxis
        {
            get;
            protected set;
        }
    }
}
