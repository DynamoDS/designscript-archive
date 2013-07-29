using System.ComponentModel;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>
    /// 
    /// </summary>
    public enum IsoDirection
    {
        /// <summary>
        /// IsoDirection in U
        /// </summary>
        U, 
        /// <summary>
        /// IsoDirection in V
        /// </summary>
        V
    }

    /// <summary>
    /// 
    /// </summary>
    public class DSBSplineSurface : DSSurface
    {
        #region DATA MEMBERS
        private DSPoint[][] mPoles;
        private DSPoint[][] mPoints;
        #endregion

        #region PRIVATE CONSTRUCTOR
        private new IBSplineSurfaceEntity SurfaceEntity { get { return HostImpl as IBSplineSurfaceEntity; } }

        static void InitType()
        {
            RegisterHostType(typeof(IBSplineSurfaceEntity), (IGeometryEntity host, bool persist) => { return new DSBSplineSurface(host as IBSplineSurfaceEntity, persist); });
        }

        internal DSBSplineSurface(IBSplineSurfaceEntity host, bool persist = false)
            : base(host, persist)
        {
            InitializeGuaranteedProperties();
        }

        internal override ISurfaceEntity GetSurfaceEntity()
        {
            return HostImpl as IBSplineSurfaceEntity;
        }

        protected override void  Dispose(bool disposing)
        {
            if (disposing)
            {
                DSGeometryExtension.DisposeObject(ref mPoles);
                DSGeometryExtension.DisposeObject(ref mPoints);
            }

            base.Dispose(disposing);
        }

        #endregion

        #region INTERNAL_METHODS

        private void InitializeGuaranteedProperties()
        {
         
        }

        #endregion

        #region PROTECTED_CONSTRUCTORS

        protected DSBSplineSurface(DSPoint[][] controlVertices, int uDegree, int vDegree,bool persist)
            : base(ByControlVerticesCore(controlVertices, ref uDegree, ref vDegree),persist)
        {
            InitializeGuaranteedProperties();
        }

        protected DSBSplineSurface(DSPoint[][] points, int uDegree, int vDegree, bool persist,bool unused)
            : base(ByPointsCore(ref points, uDegree, vDegree),persist)
        {
            InitializeGuaranteedProperties();
            Points = points;
        }

        #endregion

        #region DESIGNSCRIPT CONSTRUCTORS "STATIC METHODS"

        /// <summary>
        /// Constructs a Bspline surface from a given list of input control  vertices, a degree in the u-direction and a degree in the v-direction.
        /// </summary>
        /// <param name="controlVertices">Rectangular array of control vertices</param>
        /// <param name="uDegree">Degree in u direction</param>
        /// <param name="vDegree">Degree in v direction</param>
        /// <returns></returns>
        public static DSBSplineSurface ByControlVertices(DSPoint[][] controlVertices, int uDegree, int vDegree)
        {
            return new DSBSplineSurface(controlVertices, uDegree, vDegree, true);
        }

        
        /// <summary>
        /// Constructs a Bspline surface by fitting to given set of input points in a 2D grid and with given degree used in both u and v directions.
        /// </summary>
        /// <param name="points">Points through which the surface will pass</param>
        /// <param name="uDegree">Degree in u direction</param>
        /// <param name="vDegree">Degree in v direction</param>
        /// <returns></returns>
        public static DSBSplineSurface ByPoints(DSPoint[][] points, int uDegree, int vDegree)
        {
            return new DSBSplineSurface(points, uDegree, vDegree, true, true);
        }

       
        /// <summary>
        /// Constructs a Bspline surface by fitting to given set of input points in a 2D grid. Currently only bicubic surface fitting is supported. UDegree = VDegree = 3
        /// </summary>
        /// <param name="points">Points through which the surface will pass</param>
        /// <returns></returns>
        public static DSBSplineSurface ByPoints(DSPoint[][] points)
        {
            return ByPoints(points, 3, 3);
        }

        #endregion

        #region CORE_METHODS

        private static IBSplineSurfaceEntity ByPointsCore(ref DSPoint[][] points, int uDegree, int vDegree)
        {
            if (uDegree < 2)
                throw new System.ArgumentException(string.Format(Properties.Resources.LessThan, "uDegree", "two"), "uDegree");
            if (uDegree > 10)
                throw new System.ArgumentException(string.Format(Properties.Resources.GreaterThan, "uDegree", "ten"), "uDegree");
            if (vDegree < 2)
                throw new System.ArgumentException(string.Format(Properties.Resources.LessThan, "vDegree", "two"), "vDegree");
            if (vDegree > 10)
                throw new System.ArgumentException(string.Format(Properties.Resources.GreaterThan, "vDegree", "ten"), "vDegree");

            IPointEntity[][] pts = points.ToPointEntityArray(true); //must be rectangular array.
            if (null == pts)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidArguments, "points"), "points");
            
            // making sure if # of controlVertices > degree by atleast 1
            int udiff = pts.Length - uDegree;
            if (udiff < 1)
                uDegree += udiff - 1;

            int vdiff = pts[0].Length - vDegree;
            if (vdiff < 1)
                vDegree += vdiff - 1;

            if (pts.Length < 2)
                throw new System.ArgumentException(string.Format(Properties.Resources.LessThan, "Number of points in U direction", "two"), "points");
            if (pts[0].Length < 2)
                throw new System.ArgumentException(string.Format(Properties.Resources.LessThan, "Number of points in V direction", "two"), "points");

            points = pts.ToPointArray();
            IBSplineSurfaceEntity entity = HostFactory.Factory.BSplineSurfaceByPoints(pts, uDegree, vDegree);
            if (entity == null)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "DSBSplineSurface.ByPoints"));
            return entity;
        }

        private static IBSplineSurfaceEntity ByControlVerticesCore(DSPoint[][] controlVertices, ref int uDegree, ref int vDegree)
        {
            if (uDegree < 2)
                throw new System.ArgumentException(string.Format(Properties.Resources.LessThan, "uDegree", "two"), "uDegree");
            if (uDegree > 10)
                throw new System.ArgumentException(string.Format(Properties.Resources.GreaterThan, "uDegree", "ten"), "uDegree");
            if (vDegree < 2)
                throw new System.ArgumentException(string.Format(Properties.Resources.LessThan, "vDegree", "two"), "vDegree");
            if (vDegree > 10)
                throw new System.ArgumentException(string.Format(Properties.Resources.GreaterThan, "vDegree", "ten"), "vDegree");

            IPointEntity[][] pts = controlVertices.ToPointEntityArray(true); //must be rectangular array.
            if (null == pts)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "controlVertices", "DSBSplineSurface"), "controlVertices");

            // making sure if # of controlVertices > degree by atleast 1
            int udiff = pts.Length - uDegree;
            if (udiff < 1)
                uDegree += udiff - 1;

            int vdiff = pts[0].Length - vDegree;
            if (vdiff < 1)
                vDegree += vdiff - 1;

            if (pts.Length < 2)
                throw new System.ArgumentException(string.Format(Properties.Resources.LessThan, "Number of controlVertices in U direction", "two"), "controlVertices");
            if (pts[0].Length < 2)
                throw new System.ArgumentException(string.Format(Properties.Resources.LessThan, "Number of controlVertices in V direction", "two"), "points");

            IBSplineSurfaceEntity entity = HostFactory.Factory.BSplineSurfaceByControlVertices(pts, uDegree, vDegree);
            if (null == entity)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "DSBSplineSurface.ByControlVertices"));
            return entity;
        }

        #endregion

        #region PROPERTIES

        [Category("Primary")]
        public DSPoint[][] ControlVertices 
        {
            get
            {
                if (mPoles == null)
                    mPoles = SurfaceEntity.GetPoles().ToPointArray();

                return mPoles;
            }
        }

        [Category("Primary")]
        public DSPoint[][] Points 
        {
            get { return mPoints; }
            private set { value.AssignTo(ref mPoints); }
        }

        public double[][] Weights
        {
            get { return SurfaceEntity.GetWeights(); }
        }

        public double[] Uknots
        {
            get { return SurfaceEntity.GetUKnots(); }
        }

        public double[] Vknots
        {
            get { return SurfaceEntity.GetVKnots(); }
        }

        [Category("Primary")]
        public int UDegree
        {
            get { return SurfaceEntity.GetUDegree(); }
        }

        [Category("Primary")]
        public int VDegree
        {
            get { return SurfaceEntity.GetVDegree(); }
        }

        [Category("Primary")]
        public int NumControlVerticesU
        {
            get { return SurfaceEntity.GetNumPolesAlongU(); }
        }

        [Category("Primary")]
        public int NumControlVerticesV
        {
            get { return SurfaceEntity.GetNumPolesAlongV(); }
        }

        public bool IsPeriodicInU
        {
            get { return SurfaceEntity.GetIsPeriodicInU(); }
        }

        public bool IsPeriodicInV
        {
            get { return SurfaceEntity.GetIsPeriodicInV(); }
        }

        [Category("Primary")]
        public bool IsRational
        {
            get { return SurfaceEntity.GetIsRational(); }
        }
        
        #endregion

        #region METHODS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isoDirection"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        [AllowRankReduction]
        public DSCurve[] IsoCurve(IsoDirection isoDirection, double parameter)
        {
            ICurveEntity[] isoLineHosts = SurfaceEntity.GetIsolines((int)isoDirection, parameter);
            return isoLineHosts.ToArray<DSCurve, ICurveEntity>(true);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public DSVector NormalAtParameter(double u, double v)
        {
            IVector normal = SurfaceEntity.NormalAtParameter(u, v);
            if (null == normal)
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, "DSBSplineSurface.NormalAtParameter"));

            return new DSVector(normal);
        }

        #endregion

        #region FROM_OBJECT

        public override string ToString()
        {
            DSPoint[][] pts = ControlVertices;
            int nU = pts.Length;
            string controlPts = "{";
            for (int i = 0; i < nU; ++i)
            {
                controlPts += "{";
                int nV = pts[i].Length;
                for (int j = 0; j < nV; ++j)
                {
                    controlPts += "{ " + string.Format("{0}, {1}, {2}", pts[i][j].X.ToString(DSGeometryExtension.DoublePrintFormat), pts[i][j].Y.ToString(DSGeometryExtension.DoublePrintFormat), pts[i][j].Z.ToString(DSGeometryExtension.DoublePrintFormat));
                    if(j != nV-1)
                        controlPts += " }, ";
                    else
                        controlPts += " }";
                }
                if (i != nU - 1)
                    controlPts += "}, ";
                else
                    controlPts += "}}";
            }
            return string.Format("DSBSplineSurface(ControlVertices = {0}, UDegree = {1}, VDegree = {2})", controlPts, UDegree, VDegree);
        }

        #endregion
    }
}
