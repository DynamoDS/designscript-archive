using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    public class DSEdge : DSTopology
    {
        #region INTERNAL_METHODS

        internal IEdgeEntity EdgeEntity { get { return HostImpl as IEdgeEntity; } }

        internal DSEdge(IEdgeEntity host) : base(host) { }

        protected override DSGeometry GetGeometryCore(out bool autodispose)
        {
            DSCurve c = EdgeEntity.GetCurveGeometry().ToCurve(false, null);
            autodispose = true;
            return c;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DSGeometryExtension.DisposeObject(ref mAdjacentFaces);
                DSGeometryExtension.DisposeObject(ref mStartVertex);
                DSGeometryExtension.DisposeObject(ref mEndVertex);
            }
            base.Dispose(disposing);
        }

        #endregion

        #region DESIGNSCRIPT_PROPERTIES

        /// <summary>
        /// Accesses the Faces adjacent to the Edge
        /// </summary>
        public DSFace[] AdjacentFaces
        {
            get
            {
                if (null == mAdjacentFaces)
                {
                    IFaceEntity[] faces = EdgeEntity.GetAdjacentFaces();
                    mAdjacentFaces = faces.ConvertAll((IFaceEntity host) => new DSFace(host));
                }

                return mAdjacentFaces;
            }
        }

        /// <summary>
        /// Accesses the start Vertex of the Edge
        /// </summary>
        public DSVertex StartVertex
        {
            get
            {
                if (null == mStartVertex)
                {
                    IVertexEntity host = EdgeEntity.GetStartVertex();
                    if (null == host)
                        return null;

                    mStartVertex = new DSVertex(host);
                }
                return mStartVertex;
            }
        }

        /// <summary>
        /// Accesses the end Vertex of the Edge
        /// </summary>
        public DSVertex EndVertex
        {
            get
            {
                if (null == mEndVertex)
                {
                    IVertexEntity host = EdgeEntity.GetEndVertex();
                    if (null == host)
                        return null;

                    mEndVertex = new DSVertex(host);
                }
                return mEndVertex;
            }
        }

        /// <summary>
        /// Accesses the Curve geometry of an Edge
        /// </summary>
        public DSCurve CurveGeometry
        {
            get { return Geometry as DSCurve; }
        }
        #endregion

        #region OBJECT_METHODS

        public override string ToString()
        {
            int nFaces = EdgeEntity.GetAdjacentFaceCount();
            return string.Format("DSEdge({0}, {1}, AdjacentFaces = {2})", StartVertex, EndVertex, nFaces);
        }

        #endregion

        #region DATA MEMBERS

        private DSVertex mStartVertex;
        private DSVertex mEndVertex;
        private DSFace[] mAdjacentFaces;

        #endregion
    }
}
