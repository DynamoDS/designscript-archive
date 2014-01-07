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
        private UV(double u, double v)
        {
            this._u = u;
            this._v = v;
        }

        #region Public properties

        private double _u = 0;
        public double U
        {
            get
            {
                return _u;
            }
        }

        private double _v = 0;
        public double V
        {
            get
            {
                return _v;
            }
        }

        #endregion

        #region Public static constructors

        public static UV ByCoordinates(double u, double v)
        {
            return new UV(u,v);
        }

        #endregion

    }
}
