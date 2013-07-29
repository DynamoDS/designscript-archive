using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;

namespace DSGeometry
{
    class DesignScriptEntity : IDesignScriptEntity
    {
        private Object mOwner = null;
        public virtual Object Owner
        {
            get { return mOwner; }
        }

        public virtual void SetOwner(Object owner)
        {
            mOwner = owner;
        }

        public void Dispose()
        {
            //Disposed
        }
    }

    class GeometryEntity : DesignScriptEntity, IGeometryEntity
    {
        public virtual IGeometryEntity Clone()
        {
            return this;
        }

        public virtual IGeometryEntity CopyAndTransform(ICoordinateSystemEntity fromCS, ICoordinateSystemEntity toCS)
        {
            return this;
        }

        public virtual IGeometryEntity CopyAndTranslate(IVector offset)
        {
            return this;
        }

        public virtual IDisplayable Display
        {
            get { return new DisplayEntity(); }
        }

        public virtual double DistanceTo(IPointEntity point)
        {
            return 10;
        }

        public virtual IPointEntity GetClosestPoint(IPointEntity otherPoint)
        {
            throw new System.NotImplementedException("ClosestPointTo");
        }
    }
}
