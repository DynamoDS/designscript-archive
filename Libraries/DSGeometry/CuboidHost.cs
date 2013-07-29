using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;
namespace DSGeometry
{
    class CuboidEntity : SolidEntity, ICuboidEntity
    {
        public ICoordinateSystemEntity GetCoordinateSystem()
        {
            return ContextCoordinateSystem;
        }

        public void UpdateCuboid(double[] data, double length, double width, double height)
        {
            PointEntity origin = new PointEntity(data[0],data[1],data[2]);
            ContextCoordinateSystem.Set(origin, DsVector.ByCoordinates(length, 0, 0), DsVector.ByCoordinates(0, width, 0), DsVector.ByCoordinates(0, 0, height));
        }
        public override double GetArea()
        {
            return (2 * GetLength() * GetWidth() + 2 * GetLength() * GetHeight() + 2 * GetWidth() * GetHeight());
        }


        public override int GetEdgeCount()
        {
            return 12;
        }

        public override int GetFaceCount()
        {
            return 6;
        }

        public override int GetVertexCount()
        {
            return 8;
        }

        public override double GetVolume()
        {
            return GetLength() * GetWidth()* GetHeight();
        }
        private ICoordinateSystemEntity mContextCoordinateSystem = null;
        public ICoordinateSystemEntity ContextCoordinateSystem
        {
            get
            {
                if (mContextCoordinateSystem == null)
                    mContextCoordinateSystem = new CoordinateEntity();
                return mContextCoordinateSystem;
            }
            protected set { mContextCoordinateSystem = value; }
        }

        public double GetLength()
        {
            return ContextCoordinateSystem.XAxis.Length();
        }

        public double GetWidth()
        {
            return ContextCoordinateSystem.YAxis.Length();
        }

        public double GetHeight()
        {
            return ContextCoordinateSystem.ZAxis.Length();
        }
    }
}
