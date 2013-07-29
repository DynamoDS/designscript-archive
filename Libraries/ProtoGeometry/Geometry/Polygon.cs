using System.ComponentModel;
using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    public class Polygon : Geometry
    {
        #region DATA MEMBERS
        private Line[] mEdges;
        private Point[] mVertices;
        private Plane mPlane;
        private double? mOutOfPlane;
        #endregion

        #region PRIVATE_CONSTRUCTORS

        internal IPolygonEntity PolygonEntity { get { return HostImpl as IPolygonEntity; } }

        static void InitType()
        {
            RegisterHostType(typeof(IPolygonEntity), (IGeometryEntity host, bool persist) => { return new Polygon(host as IPolygonEntity, persist); });
        }

        private Polygon(IPolygonEntity host, bool persist = false) : base(host, persist)
        {
            InitializeGuaranteedProperties();
        }

        #endregion

        #region INTERNAL_METHODS

        private void InitializeGuaranteedProperties()
        {
        }

        internal override bool TessellateCore(IRenderPackage package)
        {
            if (base.TessellateCore(package))
                return true;

            Color c = (this.Color == null) ? Color.Yellow : this.Color;
            Point[] vertices = this.Vertices;
            foreach (var item in vertices)
            {
                package.PushLineStripVertex(item.X, item.Y, item.Z);
                package.PushLineStripVertexColor(c.RedValue, c.GreenValue, c.BlueValue, c.AlphaValue);
            }
            //Close the loop
            package.PushLineStripVertex(vertices[0].X, vertices[0].Y, vertices[0].Z);
            package.PushLineStripVertexColor(c.RedValue, c.GreenValue, c.BlueValue, c.AlphaValue);
            package.PushLineStripVertexCount(vertices.Length + 1);
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GeometryExtension.DisposeObject(ref mEdges);
                GeometryExtension.DisposeObject(ref mVertices);
                GeometryExtension.DisposeObject(ref mPlane);
            }
            base.Dispose(disposing);
        }
        #endregion

        #region PROTECTED_CONSTRUCTORS

        protected internal Polygon(Point[] vertices,bool persist)
            : base(ByVerticesCore(vertices),persist)
        {
            InitializeGuaranteedProperties();
            Vertices = vertices;
        }


        #endregion

        #region PROPERTIES

        /// <summary>
        /// 
        /// </summary>
        public Line[] Edges 
        {
            get
            {
                if (mEdges == null)
                    mEdges = GeometryExtension.CreateEdges(Vertices);

                return mEdges;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Point[] Vertices
        {
            get
            {
                if (null == mVertices)
                    mVertices = PolygonEntity.GetVertices().CreatePoints();

                return mVertices;
            }
            protected set
            {
                value.AssignTo(ref mVertices);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Plane Plane
        {
            get
            {
                if (null == mPlane)
                    mPlane = PolygonEntity.GetPlane().ToPlane(false, this);
                return mPlane;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Vector Normal { get { return Plane.Normal; } }

        /// <summary>
        /// 
        /// </summary>
        public double OutOfPlane 
        {
            get
            {
                if (!mOutOfPlane.HasValue)
                    mOutOfPlane = PolygonEntity.GetOutOfPlane();
                return mOutOfPlane.Value;
            }
        }

        #endregion

        #region DESIGNSCRIPT_CONSTRUCTORS

        /// <summary>
        /// Constructs a closed Polygon that joins the vertices in the 1D array end-to-end.
        /// </summary>
        /// <param name="vertices">Array of Point.</param>
        /// <returns>Polygon</returns>
        public static Polygon ByVertices(Point[] vertices)
        {
            return new Polygon(vertices, true);
        }

        #endregion

        #region CORE_METHODS

        private static IPolygonEntity ByVerticesCore(Point[] vertices)
        {
            IPointEntity[] hosts = vertices.ConvertAll(GeometryExtension.ToEntity<Point, IPointEntity>);
            if (hosts.Length <= 2)
                throw new System.ArgumentException(string.Format(Properties.Resources.LessThan, "number of vertices", "3"), "vertices");

            if (hosts.ArePointsColinear())
                throw new System.ArgumentException(string.Format(Properties.Resources.PointsColinear, "vertices"), "vertices");

            IPolygonEntity entity = HostFactory.Factory.PolygonByVertices(hosts);
            if (null == entity)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "Polygon.ByVertices"));
            return entity;
        }

        #endregion

        #region DESIGNSCRIPT_METHODS

        /// <summary>
        /// Returns trimmed polygon after trimming this polygon using the 
        /// given array of planes as half spaces.
        /// </summary>
        /// <param name="halfSpaces">Trimming planes.</param>
        /// <returns>Trimmed Polygon</returns>
        public Polygon Trim(Plane[] halfSpaces)
        {
            IPlaneEntity[] hosts = halfSpaces.ConvertAll(GeometryExtension.ToEntity<Plane, IPlaneEntity>);
            IPolygonEntity entity = PolygonEntity.Trim(hosts);
            if (null == entity)
                return null;

            Hide(this);
            Hide(halfSpaces);

            return new Polygon(entity, true);
        }

        #endregion
    }
}
