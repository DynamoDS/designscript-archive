using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    public class Vertex : Topology
    {
        #region INTERNAL_METHODS

        internal IVertexEntity VertexEntity { get { return HostImpl as IVertexEntity; } }

        internal Vertex(IVertexEntity host) : base(host) { }

        protected override Geometry GetGeometryCore(out bool autodispose)
        {
            Point p = VertexEntity.GetPointGeometry().ToPoint(false, null);
            autodispose = true;
            return p;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GeometryExtension.DisposeObject(ref mAdjacentEdges);
                GeometryExtension.DisposeObject(ref mAdjacentFaces);
            }
            base.Dispose(disposing);
        }

        #endregion

        #region DESIGNSCRIPT_PROPERTIES

        /// <summary>
        /// Accesses the Faces adjacent to the vertex
        /// </summary>
        public Face[] AdjacentFaces 
        {
            get
            {
                if (null == mAdjacentFaces)
                {
                    IFaceEntity[] faces = VertexEntity.GetAdjacentFaces();
                    mAdjacentFaces = faces.ConvertAll((IFaceEntity host) => new Face(host));
                }

                return mAdjacentFaces;
            }
        }

        /// <summary>
        /// Accesses the Edges adjacent to the vertex
        /// </summary>
        public Edge[] AdjacentEdges 
        {
            get
            {
                if (null == mAdjacentEdges)
                {
                    IEdgeEntity[] edges = VertexEntity.GetAdjacentEdges();
                    mAdjacentEdges = edges.ConvertAll((IEdgeEntity host) => new Edge(host));
                }

                return mAdjacentEdges;
            }
        }

        /// <summary>
        /// Accesses the Point geometry of the Vertex
        /// </summary>
        public Point PointGeometry
        {
            get { return Geometry as Point; }
        }

        #endregion

        #region OBJECT_METHODS

        public override string ToString()
        {
            IPointEntity pos = VertexEntity.GetPointGeometry();
            int nEdges = VertexEntity.GetAdjacentEdgeCount();
            int nFaces = VertexEntity.GetAdjacentFaceCount();
            var f6 = GeometryExtension.DoublePrintFormat;
            string position = string.Format("Position({0}, {1}, {2})", pos.X.ToString(f6), pos.Y.ToString(f6), pos.Z.ToString(f6));
            return string.Format("Vertex({0}, AdjacentEdges = {1}, AdjacentFaces = {2})", position, nEdges, nFaces);
        }

        #endregion

        #region PRIVATE_MEMBERS

        private Face[] mAdjacentFaces;
        private Edge[] mAdjacentEdges;

        #endregion
    }
}
