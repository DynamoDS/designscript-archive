using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    public class DSVertex : DSTopology
    {
        #region INTERNAL_METHODS

        internal IVertexEntity VertexEntity { get { return HostImpl as IVertexEntity; } }

        internal DSVertex(IVertexEntity host) : base(host) { }

        protected override DSGeometry GetGeometryCore(out bool autodispose)
        {
            DSPoint p = VertexEntity.GetPointGeometry().ToPoint(false, null);
            autodispose = true;
            return p;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DSGeometryExtension.DisposeObject(ref mAdjacentEdges);
                DSGeometryExtension.DisposeObject(ref mAdjacentFaces);
            }
            base.Dispose(disposing);
        }

        #endregion

        #region DESIGNSCRIPT_PROPERTIES

        /// <summary>
        /// Accesses the Faces adjacent to the vertex
        /// </summary>
        public DSFace[] AdjacentFaces 
        {
            get
            {
                if (null == mAdjacentFaces)
                {
                    IFaceEntity[] faces = VertexEntity.GetAdjacentFaces();
                    mAdjacentFaces = faces.ConvertAll((IFaceEntity host) => new DSFace(host));
                }

                return mAdjacentFaces;
            }
        }

        /// <summary>
        /// Accesses the Edges adjacent to the vertex
        /// </summary>
        public DSEdge[] AdjacentEdges 
        {
            get
            {
                if (null == mAdjacentEdges)
                {
                    IEdgeEntity[] edges = VertexEntity.GetAdjacentEdges();
                    mAdjacentEdges = edges.ConvertAll((IEdgeEntity host) => new DSEdge(host));
                }

                return mAdjacentEdges;
            }
        }

        /// <summary>
        /// Accesses the Point geometry of the Vertex
        /// </summary>
        public DSPoint PointGeometry
        {
            get { return Geometry as DSPoint; }
        }

        #endregion

        #region OBJECT_METHODS

        public override string ToString()
        {
            IPointEntity pos = VertexEntity.GetPointGeometry();
            int nEdges = VertexEntity.GetAdjacentEdgeCount();
            int nFaces = VertexEntity.GetAdjacentFaceCount();
            var f6 = DSGeometryExtension.DoublePrintFormat;
            string position = string.Format("Position({0}, {1}, {2})", pos.X.ToString(f6), pos.Y.ToString(f6), pos.Z.ToString(f6));
            return string.Format("DSVertex({0}, AdjacentEdges = {1}, AdjacentFaces = {2})", position, nEdges, nFaces);
        }

        #endregion

        #region PRIVATE_MEMBERS

        private DSFace[] mAdjacentFaces;
        private DSEdge[] mAdjacentEdges;

        #endregion
    }
}
