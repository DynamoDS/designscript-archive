using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;

namespace DSGeometry
{
    class PersistentHostManager : PersistenceManager
    {
        public override IPersistentObject Persist(IDesignScriptEntity entity)
        {
            return new PersistentObject(entity);
        }
    }
}
