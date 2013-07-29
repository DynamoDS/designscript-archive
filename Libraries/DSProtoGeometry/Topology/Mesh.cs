using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Interfaces;
using System.Collections;

namespace Autodesk.DesignScript.Geometry
{
    public class DSMesh : DSTopology
    {
        #region DATA MEMBERS
        private DSPoint[] mVertexPositions;
        private DSVertex[] mVertices;
        private DSEdge[] mEdges;
        private DSFace[] mFaces;
        private DSVector[] mNormals;
        #endregion

        #region PRIVATE CONSTRUCTORS

        private IMeshEntity MeshEntity { get { return HostImpl as IMeshEntity; } }

        internal DSMesh(IMeshEntity mesh) : base(mesh)
        {
        }

        internal DSMesh(IMeshEntity mesh, bool isVisible) : base(mesh)
        {
            SetVisibility(isVisible);
        }

        protected override DSGeometry GetGeometryCore(out bool autodispose)
        {
            IGeometryEntity entity = MeshEntity.Geometry;
            autodispose = true;
            return DSGeometry.ToGeometry(entity, true);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DSGeometryExtension.DisposeObject(ref mVertexPositions);
                DSGeometryExtension.DisposeObject(ref mVertices);
                DSGeometryExtension.DisposeObject(ref mEdges);
                DSGeometryExtension.DisposeObject(ref mFaces);
            }
            base.Dispose(disposing);
        }

        #endregion

        #region DESIGNSCRIPT CONSTRUCTORS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="faceIndices"></param>
        /// <returns></returns>
        public static DSMesh ByVerticesFaceIndices(DSPoint[] vertices, int[][] faceIndices)
        {
            IPointEntity[] points = vertices.ConvertAll(DSGeometryExtension.ToEntity<DSPoint, IPointEntity>);
            IMeshEntity entity = ByVerticesFaceIndicesCore(points, faceIndices);
            DSMesh mesh = new DSMesh(entity, true);
            mesh.FaceIndices = faceIndices;
            mesh.VertexPositions = vertices;
            return mesh;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="edgeIndices"></param>
        /// <returns></returns>
        private static DSMesh ByVerticesEdgeIndices(DSPoint[] vertices, int[] edgeIndices)
        {
            IPointEntity[] points = vertices.ConvertAll(DSGeometryExtension.ToEntity<DSPoint, IPointEntity>);
            IMeshEntity entity = ByVerticesEdgeIndicesCore(points, edgeIndices);
            DSMesh mesh = new DSMesh(entity);
            mesh.EdgeIndices = edgeIndices;
            mesh.VertexPositions = vertices;
            return mesh;
        }

        #endregion

        #region PROPERTIES

        /// <summary>
        /// 
        /// </summary>
        private DSPoint[] VertexPositions
        {
            get
            {
                return mVertexPositions;
            }
            set
            {
                value.AssignTo(ref mVertexPositions);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private int[] EdgeIndices { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int[][] FaceIndices { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public DSVector[] VertexNormals
        {
            get
            {
                if (null == mNormals)
                    mNormals = GetVertexNormals().ConvertAll((IVector v) => new DSVector(v));
                return mNormals;
            }
        }

        /// <summary>
        /// Accesses the Mesh Vertices 
        /// </summary>
        public DSVertex[] Vertices
        {
            get
            {
                if (null == mVertices)
                {
                    IVertexEntity[] vertices = MeshEntity.GetVertices();
                    mVertices = ConvertAll(vertices, (IVertexEntity host) => new DSVertex(host));
                }

                return mVertices;
            }
        }

        /// <summary>
        /// Accesses the Mesh Edges 
        /// </summary>
        public DSEdge[] Edges
        {
            get
            {
                if (null == mEdges)
                {
                    IEdgeEntity[] edges = MeshEntity.GetEdges();
                    mEdges = ConvertAll(edges, (IEdgeEntity host) => new DSEdge(host));
                }

                return mEdges;
            }
        }

        /// <summary>
        /// Accesses the Mesh Faces
        /// </summary>
        public DSFace[] Faces
        {
            get
            {
                if (null == mFaces)
                {
                    IFaceEntity[] faces = MeshEntity.GetFaces();
                    mFaces = ConvertAll(faces, (IFaceEntity host) => new DSFace(host));
                }

                return mFaces;
            }
        }

        #endregion

        #region INTERNAL METHODS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="points"></param>
        /// <param name="faceIndices"></param>
        /// <returns></returns>
        internal static IMeshEntity ByVerticesFaceIndicesCore(IPointEntity[] points, int[][] faceIndices)
        {
            string kMethodName = "DSMesh.ByVerticesFaceIndices";

            if (points.Length < 3 || points.ArePointsColinear())
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "vertices", kMethodName));

            IMeshEntity entity = HostFactory.Factory.MeshByVerticesFaceIndices(points, faceIndices);
            if (null == entity)
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));

            return entity;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="points"></param>
        /// <param name="edgeIndices"></param>
        /// <returns></returns>
        internal static IMeshEntity ByVerticesEdgeIndicesCore(IPointEntity[] points, int[] edgeIndices)
        {
            string kMethodName = "DSMesh.ByVerticesEdgeIndices";

            if (points.Length < 3 || points.ArePointsColinear())
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "vertices", kMethodName));

            IMeshEntity entity = HostFactory.Factory.MeshByVerticesEdgeIndices(points, edgeIndices);
            if (null == entity)
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));

            return entity;
        }

        private static TOutput[] ConvertAll<TInput, TOutput>(TInput[] array, Converter<TInput, TOutput> converter)
        {
            if (null == array)
                return null;

            TOutput[] retArray = new TOutput[array.Length];
            for(int i = 0; i < array.Length; ++i)
            {
                if (null == array[i])
                    continue;

                TOutput val = converter(array[i]);
                if (null == val)
                    continue;

                retArray[i] = val;
            }

            return retArray;
        }

        private IVector[] GetVertexNormals()
        {
            if (null == mVertexPositions || null == FaceIndices)
                return null;

            IPointEntity[] points = mVertexPositions.ConvertAll(DSGeometryExtension.ToEntity<DSPoint, IPointEntity>);
            using (ISubDMeshEntity mesh = HostFactory.Factory.SubDMeshByVerticesFaceIndices(points, FaceIndices, 0))
            {
                return mesh.GetVertexNormals();
            }
        }

        #endregion
    }
}
