using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;
namespace DSGeometry
{
    class PolygonEntity : GeometryEntity, IPolygonEntity
    {
        internal PolygonEntity()
        {
            Positions = new IPointEntity[2] { new PointEntity(), new PointEntity(1,1,1) };
        }
        public double GetOutOfPlane()
        {
            return 0;
        }

        public IPlaneEntity GetPlane()
        {
            return new PlaneEntity();
        }

        public IPointEntity[] GetVertices()
        {
            return new IPointEntity[2] { new PointEntity(), new PointEntity() };
        }

        public IPolygonEntity Trim(IPlaneEntity[] halfSpaces)
        {
            return this;
        }

        public void UpdateVertices(IPointEntity[] positions)
        {
            this.Positions = positions;
        }
        public IPointEntity[] Positions { get; protected set; }
    }
}
