using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Autodesk.DesignScript.Interfaces;
using System.Collections;

namespace Autodesk.DesignScript.Geometry
{
    public class Mesh : Topology
    {
        #region DATA MEMBERS

        private Point[] mVertexPositions;
        private Vertex[] mVertices;
        private Edge[] mEdges;
        private Face[] mFaces;
        private Vector[] mNormals;

        #endregion

        #region PRIVATE CONSTRUCTORS

        private IPolyMeshEntity MeshEntity { get { return HostImpl as IPolyMeshEntity; } }

        internal Mesh(IPolyMeshEntity mesh)
            : base( mesh )
        {
        }

        internal Mesh(IPolyMeshEntity mesh, bool isVisible)
            : base( mesh )
        {
            SetVisibility(isVisible);
        }

        protected override Geometry GetGeometryCore(out bool autodispose)
        {
            IGeometryEntity entity = MeshEntity.Geometry;
            autodispose = true;
            return Geometry.ToGeometry(entity, true);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GeometryExtension.DisposeObject(ref mVertexPositions);
                GeometryExtension.DisposeObject(ref mVertices);
                GeometryExtension.DisposeObject(ref mEdges);
                GeometryExtension.DisposeObject(ref mFaces);
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
        public static Mesh ByVerticesFaceIndices(Point[] vertices, IIndexGroup[] faceIndices)
        {
            var points = vertices.ConvertAll(GeometryExtension.ToEntity<Point, IPointEntity>);
            var entity = ByVerticesFaceIndicesCore(points, faceIndices);
            var mesh = new Mesh(entity, true);
            mesh.FaceIndices = faceIndices;
            mesh.VertexPositions = vertices;
            return mesh;
        }

        // PB: I don't understand this method

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="vertices"></param>
        ///// <param name="edgeIndices"></param>
        ///// <returns></returns>
        //private static Mesh ByVerticesEdgeIndices(Point[] vertices, int[] edgeIndices)
        //{
        //    var points = vertices.ConvertAll(GeometryExtension.ToEntity<Point, IPointEntity>);
        //    var entity = ByVerticesEdgeIndicesCore(points, edgeIndices);
        //    var mesh = new Mesh(entity);
        //    mesh.EdgeIndices = edgeIndices;
        //    mesh.VertexPositions = vertices;
        //    return mesh;
        //}

        #endregion

        #region PROPERTIES

        /// <summary>
        /// 
        /// </summary>
        private Point[] VertexPositions
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
        public IIndexGroup[] FaceIndices { get; protected set; }

        /// <summary>
        /// 
        /// </summary>
        public Vector[] VertexNormals
        {
            get
            {
                if (null == mNormals)
                    mNormals = GetVertexNormals().ConvertAll((IVectorEntity v) => new Vector(v));
                return mNormals;
            }
        }

        /// <summary>
        /// Accesses the Mesh Vertices 
        /// </summary>
        public Vertex[] Vertices
        {
            get
            {
                if (null == mVertices)
                {
                    var vertices = MeshEntity.GetVertices();
                    mVertices = vertices.Select(x => new Vertex(x)).ToArray();
                }

                return mVertices;
            }
        }

        /// <summary>
        /// Accesses the Mesh Edges 
        /// </summary>
        public Edge[] Edges
        {
            get
            {
                if (null == mEdges)
                {
                    IEdgeEntity[] edges = MeshEntity.GetEdges();
                    mEdges = ConvertAll(edges, (IEdgeEntity host) => new Edge(host));
                }

                return mEdges;
            }
        }

        /// <summary>
        /// Accesses the Mesh Faces
        /// </summary>
        public Face[] Faces
        {
            get
            {
                if (null == mFaces)
                {
                    IFaceEntity[] faces = MeshEntity.GetFaces();
                    mFaces = ConvertAll(faces, (IFaceEntity host) => new Face(host));
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
        internal static IPolyMeshEntity ByVerticesFaceIndicesCore(IPointEntity[] points, IIndexGroup[] faceIndices)
        {
            string kMethodName = "Mesh.ByVerticesFaceIndices";

            if (points.Length < 3 || points.ArePointsColinear())
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "vertices", kMethodName));

            var entity = HostFactory.Factory.PolyMeshByVerticesFaceIndices(points, faceIndices);
            if (null == entity)
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));

            return entity;
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="points"></param>
        ///// <param name="edgeIndices"></param>
        ///// <returns></returns>
        //internal static IPolyMeshEntity ByVerticesEdgeIndicesCore(IPointEntity[] points, int[] edgeIndices)
        //{
        //    string kMethodName = "Mesh.ByVerticesEdgeIndices";

        //    if (points.Length < 3 || points.ArePointsColinear())
        //        throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "vertices", kMethodName));

        //    var entity = HostFactory.Factory.PolyMeshByVerticesEdgeIndices(points, edgeIndices);
        //    if (null == entity)
        //        throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));

        //    return entity;
        //}

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

        private IVectorEntity[] GetVertexNormals()
        {
            if (null == mVertexPositions || null == FaceIndices)
                return null;

            return this.VertexNormals.Select(x => x.VectorEntity).ToArray();
        }

        #endregion


        /// <summary>
        /// Creates an array of Sub Division Mesh from the .Obj file given by the user.
        /// Each Group within the .Obj file is represented by one SubDivsion Mesh. 
        /// </summary>
        /// <param name="filePath">The file to be imported</param>
        /// <returns></returns>
        public static Geometry[] ImportFromOBJ(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new System.ArgumentNullException("filePath");
            filePath = GeometryExtension.LocateFile(filePath);
            if (!File.Exists(filePath))
                throw new System.ArgumentException(string.Format(Properties.Resources.FileNotFound, filePath), "filePath");
            var result = ObjHandler.Import(filePath);
            return result.ToMesh().Select(x =>
            {
                var autodispose = true;
                return x.GetGeometryCore(out autodispose);
            }).ToArray();
        }

    }
}
