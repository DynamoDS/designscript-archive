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
    public class NurbsSurface : Surface
    {
        #region DATA MEMBERS
        private Point[][] mPoles;
        private Point[][] mPoints;
        #endregion

        #region PRIVATE CONSTRUCTOR
        private new INurbsSurfaceEntity SurfaceEntity { get { return HostImpl as INurbsSurfaceEntity; } }

        static void InitType()
        {
            RegisterHostType(typeof(INurbsSurfaceEntity), (IGeometryEntity host, bool persist) => { return new NurbsSurface(host as INurbsSurfaceEntity, persist); });
        }

        internal NurbsSurface(INurbsSurfaceEntity host, bool persist = false)
            : base(host, persist)
        {
            InitializeGuaranteedProperties();
        }

        internal override ISurfaceEntity GetSurfaceEntity()
        {
            return HostImpl as INurbsSurfaceEntity;
        }

        protected override void  Dispose(bool disposing)
        {
            if (disposing)
            {
                GeometryExtension.DisposeObject(ref mPoles);
                GeometryExtension.DisposeObject(ref mPoints);
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

        protected NurbsSurface(Point[][] controlVertices, int uDegree, int vDegree,bool persist)
            : base(ByControlVerticesCore(controlVertices, ref uDegree, ref vDegree),persist)
        {
            InitializeGuaranteedProperties();
        }

        protected NurbsSurface(Point[][] points, int uDegree, int vDegree, bool persist,bool unused)
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
        public static NurbsSurface ByControlVertices(Point[][] controlVertices, int uDegree, int vDegree)
        {
            return new NurbsSurface(controlVertices, uDegree, vDegree, true);
        }

        
        /// <summary>
        /// Constructs a Bspline surface by fitting to given set of input points in a 2D grid and with given degree used in both u and v directions.
        /// </summary>
        /// <param name="points">Points through which the surface will pass</param>
        /// <param name="uDegree">Degree in u direction</param>
        /// <param name="vDegree">Degree in v direction</param>
        /// <returns></returns>
        public static NurbsSurface ByPoints(Point[][] points, int uDegree, int vDegree)
        {
            return new NurbsSurface(points, uDegree, vDegree, true, true);
        }

       
        /// <summary>
        /// Constructs a Bspline surface by fitting to given set of input points in a 2D grid. Currently only bicubic surface fitting is supported. UDegree = VDegree = 3
        /// </summary>
        /// <param name="points">Points through which the surface will pass</param>
        /// <returns></returns>
        public static NurbsSurface ByPoints(Point[][] points)
        {
            return ByPoints(points, 3, 3);
        }

        #endregion

        #region CORE_METHODS

        private static INurbsSurfaceEntity ByPointsCore(ref Point[][] points, int uDegree, int vDegree)
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
            INurbsSurfaceEntity entity = HostFactory.Factory.NurbsSurfaceByPoints(pts, uDegree, vDegree);
            if (entity == null)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "BSplineSurface.ByPoints"));
            return entity;
        }

        private static INurbsSurfaceEntity ByControlVerticesCore(Point[][] controlVertices, ref int uDegree, ref int vDegree)
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
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "controlVertices", "BSplineSurface"), "controlVertices");

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

            INurbsSurfaceEntity entity = HostFactory.Factory.NurbsSurfaceByControlVertices(pts, uDegree, vDegree);
            if (null == entity)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "BSplineSurface.ByControlVertices"));
            return entity;
        }

        #endregion

        #region PROPERTIES

        [Category("Primary")]
        public Point[][] ControlVertices 
        {
            get
            {
                if (mPoles == null)
                    mPoles = SurfaceEntity.GetPoles().ToPointArray();

                return mPoles;
            }
        }

        [Category("Primary")]
        public Point[][] Points 
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
            get { return SurfaceEntity.DegreeU; }
        }

        [Category("Primary")]
        public int VDegree
        {
            get { return SurfaceEntity.DegreeV; }
        }

        [Category("Primary")]
        public int NumControlVerticesU
        {
            get { return SurfaceEntity.NumControlPointsU; }
        }

        [Category("Primary")]
        public int NumControlVerticesV
        {
            get { return SurfaceEntity.NumControlPointsV; }
        }

        public bool IsPeriodicInU
        {
            get { return SurfaceEntity.IsPeriodicInU; }
        }

        public bool IsPeriodicInV
        {
            get { return SurfaceEntity.IsPeriodicInV; }
        }

        [Category("Primary")]
        public bool IsRational
        {
            get { return SurfaceEntity.IsRational; }
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
        public Curve[] IsoCurve(IsoDirection isoDirection, double parameter)
        {
            ICurveEntity[] isoLineHosts = SurfaceEntity.GetIsolines((int)isoDirection, parameter);
            return isoLineHosts.ToArray<Curve, ICurveEntity>(true);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public Vector NormalAtParameter(double u, double v)
        {
            var normal = SurfaceEntity.NormalAtParameter(u, v);
            if (null == normal)
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, "BSplineSurface.NormalAtParameter"));

            return new Vector(normal);
        }

        #endregion

        #region FROM_OBJECT

        public override string ToString()
        {
            Point[][] pts = ControlVertices;
            int nU = pts.Length;
            string controlPts = "{";
            for (int i = 0; i < nU; ++i)
            {
                controlPts += "{";
                int nV = pts[i].Length;
                for (int j = 0; j < nV; ++j)
                {
                    controlPts += "{ " + string.Format("{0}, {1}, {2}", pts[i][j].X.ToString(GeometryExtension.DoublePrintFormat), pts[i][j].Y.ToString(GeometryExtension.DoublePrintFormat), pts[i][j].Z.ToString(GeometryExtension.DoublePrintFormat));
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
            return string.Format("BSplineSurface(ControlVertices = {0}, UDegree = {1}, VDegree = {2})", controlPts, UDegree, VDegree);
        }

        #endregion
    }
}
