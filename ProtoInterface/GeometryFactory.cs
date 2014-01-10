using System;
using Autodesk.DesignScript.Interfaces;
using System.ComponentModel;

namespace Autodesk.DesignScript.Geometry
{
    //[Browsable(false)]
    //public abstract class GeometryFactory : IGeometryFactory
    //{
        
    //}

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
