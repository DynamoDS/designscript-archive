using System.ComponentModel;
using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    public class DSPolygon : DSGeometry
    {
        #region DATA MEMBERS
        private DSLine[] mEdges;
        private DSPoint[] mVertices;
        private DSPlane mPlane;
        private double? mOutOfPlane;
        #endregion

        #region PRIVATE_CONSTRUCTORS

        internal IPolygonEntity PolygonEntity { get { return HostImpl as IPolygonEntity; } }

        static void InitType()
        {
            RegisterHostType(typeof(IPolygonEntity), (IGeometryEntity host, bool persist) => { return new DSPolygon(host as IPolygonEntity, persist); });
        }

        private DSPolygon(IPolygonEntity host, bool persist = false) : base(host, persist)
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

            DSColor c = (this.Color == null) ? DSColor.Yellow : this.Color;
            DSPoint[] vertices = this.Vertices;
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
                DSGeometryExtension.DisposeObject(ref mEdges);
                DSGeometryExtension.DisposeObject(ref mVertices);
                DSGeometryExtension.DisposeObject(ref mPlane);
            }
            base.Dispose(disposing);
        }
        #endregion

        #region PROTECTED_CONSTRUCTORS

        protected internal DSPolygon(DSPoint[] vertices,bool persist)
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
        public DSLine[] Edges 
        {
            get
            {
                if (mEdges == null)
                    mEdges = DSGeometryExtension.CreateEdges(Vertices);

                return mEdges;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DSPoint[] Vertices
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
        public DSPlane Plane
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
        public DSVector Normal { get { return Plane.Normal; } }

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
        public static DSPolygon ByVertices(DSPoint[] vertices)
        {
            return new DSPolygon(vertices, true);
        }

        #endregion

        #region CORE_METHODS

        private static IPolygonEntity ByVerticesCore(DSPoint[] vertices)
        {
            IPointEntity[] hosts = vertices.ConvertAll(DSGeometryExtension.ToEntity<DSPoint, IPointEntity>);
            if (hosts.Length <= 2)
                throw new System.ArgumentException(string.Format(Properties.Resources.LessThan, "number of vertices", "3"), "vertices");

            if (hosts.ArePointsColinear())
                throw new System.ArgumentException(string.Format(Properties.Resources.PointsColinear, "vertices"), "vertices");

            IPolygonEntity entity = HostFactory.Factory.PolygonByVertices(hosts);
            if (null == entity)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "DSPolygon.ByVertices"));
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
        public DSPolygon Trim(DSPlane[] halfSpaces)
        {
            IPlaneEntity[] hosts = halfSpaces.ConvertAll(DSGeometryExtension.ToEntity<DSPlane, IPlaneEntity>);
            IPolygonEntity entity = PolygonEntity.Trim(hosts);
            if (null == entity)
                return null;

            Hide(this);
            Hide(halfSpaces);

            return new DSPolygon(entity, true);
        }

        #endregion
    }
}
