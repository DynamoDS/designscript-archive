using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    public class Edge : Topology
    {
        #region INTERNAL_METHODS

        internal IEdgeEntity EdgeEntity { get { return HostImpl as IEdgeEntity; } }

        internal Edge(IEdgeEntity host) : base(host) { }

        protected override Geometry GetGeometryCore(out bool autodispose)
        {
            Curve c = EdgeEntity.GetCurveGeometry().ToCurve(false, null);
            autodispose = true;
            return c;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GeometryExtension.DisposeObject(ref mAdjacentFaces);
                GeometryExtension.DisposeObject(ref mStartVertex);
                GeometryExtension.DisposeObject(ref mEndVertex);
            }
            base.Dispose(disposing);
        }

        #endregion

        #region DESIGNSCRIPT_PROPERTIES

        /// <summary>
        /// Accesses the Faces adjacent to the Edge
        /// </summary>
        public Face[] AdjacentFaces
        {
            get
            {
                if (null == mAdjacentFaces)
                {
                    IFaceEntity[] faces = EdgeEntity.GetAdjacentFaces();
                    mAdjacentFaces = faces.ConvertAll((IFaceEntity host) => new Face(host));
                }

                return mAdjacentFaces;
            }
        }

        /// <summary>
        /// Accesses the start Vertex of the Edge
        /// </summary>
        public Vertex StartVertex
        {
            get
            {
                if (null == mStartVertex)
                {
                    IVertexEntity host = EdgeEntity.GetStartVertex();
                    if (null == host)
                        return null;

                    mStartVertex = new Vertex(host);
                }
                return mStartVertex;
            }
        }

        /// <summary>
        /// Accesses the end Vertex of the Edge
        /// </summary>
        public Vertex EndVertex
        {
            get
            {
                if (null == mEndVertex)
                {
                    IVertexEntity host = EdgeEntity.GetEndVertex();
                    if (null == host)
                        return null;

                    mEndVertex = new Vertex(host);
                }
                return mEndVertex;
            }
        }

        /// <summary>
        /// Accesses the Curve geometry of an Edge
        /// </summary>
        public Curve CurveGeometry
        {
            get { return Geometry as Curve; }
        }
        #endregion

        #region OBJECT_METHODS

        public override string ToString()
        {
            int nFaces = EdgeEntity.GetAdjacentFaceCount();
            return string.Format("Edge({0}, {1}, AdjacentFaces = {2})", StartVertex, EndVertex, nFaces);
        }

        #endregion

        #region DATA MEMBERS

        private Vertex mStartVertex;
        private Vertex mEndVertex;
        private Face[] mAdjacentFaces;

        #endregion
    }
}
