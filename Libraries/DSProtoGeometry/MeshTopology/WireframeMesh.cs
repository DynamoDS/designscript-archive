using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    class WireframeMesh : Topology
    {
        internal IWireframeMeshEntity WireframeMeshEntity { get { return HostImpl as IWireframeMeshEntity; } }

        #region PRIVATE_MEMBERS

        private Vertex[] mVertices;
        private Edge[] mEdges;

        #endregion


        #region DESIGNSCRIPT_PROPERTIES

        /// <summary>
        /// 
        /// </summary>
        public Vertex[] Vertices
        {
            get
            {
                if (null == mVertices)
                {
                    IVertexEntity[] vertices = WireframeMeshEntity.GetVertices();
                    mVertices = vertices.ConvertAll((IVertexEntity host) => new Vertex(host));
                }

                return mVertices;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Edge[] Edges
        {
            get
            {
                if (null == mEdges)
                {
                    IEdgeEntity[] edges = WireframeMeshEntity.GetEdges();
                    mEdges = edges.ConvertAll((IEdgeEntity host) => new Edge(host));
                }

                return mEdges;
            }
        }

        #endregion

        //protected WireframeMesh(IWireframeMeshEntity host) : base(host) { }

        protected WireframeMesh(Point[] vertices, int[][] edgeIndices) : base(ByVerticesEdgeIndicesCore(vertices, edgeIndices)) 
        {
            
        }

        
        private static IWireframeMeshEntity ByVerticesEdgeIndicesCore(Point[] vertices, int[][] edgeIndices)
        {
            string kMethodName = "WireFrameMesh.ByVerticesEdgeIndices";

            IPointEntity[] points = vertices.ConvertAll(GeometryExtension.ToEntity<Point, IPointEntity>);
            if (points.Length < 2)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "vertices", kMethodName));

            return HostFactory.Factory.WireframeMeshByVerticesFaceIndices(points, edgeIndices);
        }

        protected override Geometry GetGeometryCore(out bool autodispose)
        {
            throw new NotImplementedException("WireframeMesh::GetGeometryCore not implemented");
        }

        #region DESIGNSCRIPT_CONSTRUCTORS

        public static WireframeMesh ByVerticesEdgeIndices(Point[] vertices, int[][] edgeIndices)   
        {
            return new WireframeMesh(vertices, edgeIndices);
        }

        #endregion
    }
}
