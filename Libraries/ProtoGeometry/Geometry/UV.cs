using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>
    /// A type representing a pair of parameters on a surface
    /// </summary>
    public class UV
    {
        private IUVEntity _uvEntity;

        private UV( IUVEntity uvEntity )
        {
            this._uvEntity = uvEntity;
        }

        #region Public properties

        public double U
        {
            get
            {
                return _uvEntity.U;
            }
        }

        public double V
        {
            get
            {
                return _uvEntity.V;
            }
        }

        #endregion

        #region Public static constructors

        public static UV ByCoordinates(double u, double v)
        {
            return new UV(HostFactory.Factory.UVByCoordinates(u, v));
        }

        #endregion

    }
}
