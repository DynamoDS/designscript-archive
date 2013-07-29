using System.Collections.Generic;
using Autodesk.DesignScript.Interfaces;
using System.IO;
using System;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>
    /// 
    /// </summary>
    public class DSSubDivisionMesh : DSSurface
    {
        internal ISubDMeshEntity SubDMeshEntity { get { return HostImpl as ISubDMeshEntity; } }

        #region DATA MEMBERS
        private DSPoint[] mVertices;
        private DSLine[] mEdges;
        private double? mSurfaceArea;
        private DSColor[] mColors;
        private DSVector[] mNormals;
        private ISurfaceEntity mSurfaceEntity;
        #endregion

        #region INTERNAL_METHODS

        private void InitializeGuaranteedProperties()
        {
        }

        #endregion

        #region PRIVATE_CONSTRUCTORS
        static void InitType()
        {
            RegisterHostType(typeof(ISubDMeshEntity), (IGeometryEntity host, bool persist) => { return new DSSubDivisionMesh(host as ISubDMeshEntity, persist); });
        }

        private DSSubDivisionMesh(ISubDMeshEntity host, bool persist = false)
            : base(host, persist)
        {
            InitializeGuaranteedProperties();
        }

        internal override ISurfaceEntity GetSurfaceEntity()
        {
            if (null == mSurfaceEntity)
            {
                mSurfaceEntity = SubDMeshEntity.ConvertToSurface(true);
            }
            return mSurfaceEntity;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DSGeometryExtension.DisposeObject(ref mVertices);
                DSGeometryExtension.DisposeObject(ref mEdges);
                mSurfaceEntity.DisposeObject(); mSurfaceEntity = null;
            }
            base.Dispose(disposing);
        }

        protected override bool GetIsClosed()
        {
            return SubDMeshEntity.GetIsClosed();
        }

        #endregion

        #region PROTECTED_CONSTRUCTORS

        protected internal DSSubDivisionMesh(DSPoint[] vertices, int[][] faceIndices, int subDivisionLevel,bool persist)
            : base(ByVerticesFaceIndicesCore(vertices, faceIndices, subDivisionLevel),persist)
        {
            InitializeGuaranteedProperties();
            SubDivisionLevel = subDivisionLevel;
            Vertices = vertices;
        }

        #endregion

        #region PROPERTIES

        /// <summary>
        /// 
        /// </summary>
        public int[][] FaceIndices { get { return SubDMeshEntity.GetFaceIndices(); } }

        /// <summary>
        /// 
        /// </summary>
        public int? SubDivisionLevel { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public DSPoint[] Vertices 
        {
            get
            {
                if (null == mVertices)
                    mVertices = SubDMeshEntity.GetVertices().CreatePoints();
                return mVertices;
            }
            private set
            {
                value.AssignTo(ref mVertices);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DSColor[] VertexColors
        {
            get
            {
                if (mColors == null)
                    mColors = SubDMeshEntity.GetVertexColors().ConvertAll((IColor c) => DSColor.FromIColor(c));
                return mColors;
            }
            set
            {
                if (SubDivisionLevel > 0)
                    throw new System.InvalidOperationException(Properties.Resources.VertexColorNotSupported);

                ISubDMeshEntity host = SubDMeshEntity;
                //Get the existing set of colors.
                if (mColors == null)
                    mColors = host.GetVertexColors().ConvertAll((IColor c) => DSColor.FromIColor(c));

                if (null == value || value.Length == 0)
                    mColors = new DSColor[host.GetNumVertices()];
                else
                {
                    if (mColors == null || mColors.Length == 0)
                        mColors = new DSColor[host.GetNumVertices()];

                    //Update the cached color with the input, so that local cache is updated.
                    for (int i = 0; i < value.Length && i < mColors.Length; ++i)
                        mColors[i] = value[i];
                }
                //Update the mesh with these colors.
                host.UpdateSubDMeshColors(mColors.ConvertAll((DSColor c)=> c.IColor));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DSVector[] VertexNormals
        {
            get
            {
                if (null == mNormals)
                    mNormals = SubDMeshEntity.GetVertexNormals().ConvertAll((IVector v) => new DSVector(v));
                return mNormals;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DSLine[] Edges
        {
            get
            {
                if (null == mEdges)
                {
                    ILineEntity[] edgeLines = SubDMeshEntity.GetEdges();
                    mEdges = edgeLines.ToArray<DSLine, ILineEntity>(false);
                }

                return mEdges;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double SurfaceArea
        {
            get
            {
                if (!mSurfaceArea.HasValue)
                    mSurfaceArea = SubDMeshEntity.ComputeSurfaceArea();
                return mSurfaceArea.Value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int NumVertices { get { return SubDMeshEntity.GetNumVertices(); } }

        /// <summary>
        /// 
        /// </summary>
        public int NumFaces { get { return SubDMeshEntity.GetNumFaces(); } }

        /// <summary>
        /// 
        /// </summary>
        public DSPoint[] ResultVertices { get { return SubDMeshEntity.GetResultVertices().CreatePoints(); } }

        /// <summary>
        /// 
        /// </summary>
        public int[][] ResultFaceIndices { get { return SubDMeshEntity.GetResultFaceIndices(); } }

        /// <summary>
        /// 
        /// </summary>
        public double Volume { get { return SubDMeshEntity.ComputeVolume(); } }

        /// <summary>
        /// 
        /// </summary>
        public int NumResultFaces { get { return SubDMeshEntity.GetNumResultFaces(); } }

        /// <summary>
        /// 
        /// </summary>
        public int NumResultVertices { get { return SubDMeshEntity.GetNumResultVertices(); } }

        #endregion

        #region DESIGNSCRIPT_CONSTRUCTORS
        /// <summary>
        /// Creates a SubDivisionMesh from Solid or Surface geometry by faceting 
        /// the faces of Solid or Surface.
        /// </summary>
        /// <param name="context">Input geometry, Solid or Surface.</param>
        /// <param name="maxEdgeLength">Maximum allowed edge length 
        /// to define coarseness of the mesh.</param>
        /// <returns>SubDivisionMesh</returns>
        public static DSSubDivisionMesh FromGeometry(DSGeometry context, double maxEdgeLength)
        {
            string kMethodName = "DSSubDivisionMesh.FromGeometry";
            if (null == context)
                throw new System.ArgumentNullException("context");

            ISubDMeshEntity entity = HostFactory.Factory.SubDMeshFromGeometry(context.GeomEntity, maxEdgeLength);
            if(null == entity)
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));

            DSSubDivisionMesh mesh = new DSSubDivisionMesh(entity, true);
            mesh.Context = context;
            mesh.SubDivisionLevel = 0;
            return mesh;
        }

        /// <summary>
        /// Creates an array of Sub Division Mesh from the .Obj file given by the user.
        /// Each Group within the .Obj file is represented by one SubDivsion Mesh. 
        /// </summary>
        /// <param name="filePath">The file to be imported</param>
        /// <returns></returns>
        public static DSSubDivisionMesh[] ImportFromOBJ(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new System.ArgumentNullException("filePath");
            filePath = DSGeometryExtension.LocateFile(filePath);
            if (!File.Exists(filePath))
                throw new System.ArgumentException(string.Format(Properties.Resources.FileNotFound, filePath), "filePath");
            DSMeshData result = ObjHandler.Import(filePath);
            return result.ConvertToSubDivisionMesh();
        }

        /// <summary>
        /// Exports the SubDivision meshes provided by the user to an .Obj file. Each Subvision Mesh
        /// is represented by a group. Groups are automatically named.
        /// </summary>
        /// <param name="mesh">The SubDivision Mesh array to be exported in one file</param>
        /// <param name="filePath">The meshes are exported to this file Path, or the active directory if only file name is provided</param>
        /// <returns></returns>
        public static bool ExportToOBJ(DSSubDivisionMesh[] mesh, string filePath)
        {
            if (mesh == null || mesh.Length == 0)
                throw new System.ArgumentNullException("mesh");
            if (string.IsNullOrWhiteSpace(filePath))
                throw new System.ArgumentNullException("filePath");
            DSMeshData meshData = new DSMeshData();
            for (int i = 0; i < mesh.Length; ++i)
            {
                meshData.AddGroup("Group_" + i);
                foreach (var vertices in mesh[i].FaceIndices)
                {
                    for (int j = 0; j < vertices.Length; ++j)
                        vertices[j] += meshData.Vertices.Count + 1;
                    meshData.Groups[i].AddFace(vertices);
                }
                meshData.Vertices.AddRange(mesh[i].Vertices);
            }
            if (!filePath.EndsWith(".obj"))
                filePath += ".obj";
            if (!Path.IsPathRooted(filePath))
            {
                string foldername = Path.GetDirectoryName(DSGeometrySettings.RootModulePath);
                filePath = Path.Combine(foldername, filePath);
            }
            return ObjHandler.Export(meshData, filePath);
        }

        /// <summary>
        /// Exports the SubDivision mesh object provided by the user to an .Obj file. One group is created and named automatically.
        /// </summary>
        /// <param name="filePath">>The meshes are exported to this file Path, or the active directory if only file name is provided</param>
        /// <returns></returns>
        public bool ExportToOBJ(string filePath)
        {
            return ExportToOBJ(new DSSubDivisionMesh[] { this }, filePath);        
        }

        /// <summary>
        /// Constructs a subdivision mesh given an input array of vertex points
        /// and an input array of faces defined by a set of numbers, which are 
        /// the indices of the vertices in the 'vertices' array making up the 
        /// face. 'subDivisionLevel' is the initial smoothness level.
        /// </summary>
        /// <param name="vertices">Input array of vertex points</param>
        /// <param name="faceIndices">Input array of faces indices for each 
        /// vertex </param>
        /// <param name="subDivisionLevel">Initial smoothness level, subDivisionLevel must 
        /// have a value greater than or equal to 0 and less than 5.</param>
        /// <returns>SubDivisionMesh</returns>
        public static DSSubDivisionMesh ByVerticesFaceIndices(DSPoint[] vertices, int[][] faceIndices, int subDivisionLevel)
        {
            return new DSSubDivisionMesh(vertices, faceIndices, subDivisionLevel, true);
        }

        /// <summary>
        /// Constructs a subdivision mesh by vertex shading, given an input 
        /// array of vertex points and an input array of faces defined by a set 
        /// of numbers, which are the indices of the vertices in the 'vertices' 
        /// array making up the face. 
        /// </summary>
        /// <param name="vertices">Input array of vertex points</param>
        /// <param name="faceIndices">Input array of faces indices for each 
        /// vertex </param>
        /// <param name="vertexColors">Input array of color assigned to each
        /// vertex </param>
        /// <param name="subDivisionLevel">Initial smoothness level, subDivisionLevel must 
        /// have a value greater than or equal to 0 and less than 5.</param>
        /// <returns>SubDivisionMesh</returns>
        public static DSSubDivisionMesh ByVerticesFaceIndices(DSPoint[] vertices,
            int[][] faceIndices, DSColor[] vertexColors, int subDivisionLevel)
        {
            return ByVerticesFaceIndices(vertices, faceIndices, null, vertexColors, subDivisionLevel);
        }

        /// <summary>
        /// Constructs a subdivision mesh by vertex shading, given an input 
        /// array of vertex points and an input array of faces defined by a set 
        /// of numbers, which are the indices of the vertices in the 'vertices' 
        /// array making up the face. 
        /// </summary>
        /// <param name="vertices">Input array of vertex points</param>
        /// <param name="faceIndices">Input array of faces indices for each 
        /// vertex </param>
        /// <param name="vertexNormals">Input array of vertex normals</param>
        /// <param name="vertexColors">Input array of color assigned to each
        /// vertex </param>
        /// <param name="subDivisionLevel">Initial smoothness level, subDivisionLevel must 
        /// have a value greater than or equal to 0 and less than 5.</param>
        /// <returns>SubDivisionMesh</returns>
        public static DSSubDivisionMesh ByVerticesFaceIndices(DSPoint[] vertices, 
            int[][] faceIndices, DSVector[] vertexNormals, DSColor[] vertexColors, int subDivisionLevel)
        {
            string kMethodName = "DSSubDivisionMesh.ByVerticesFaceIndices";
            if (subDivisionLevel < 0 || subDivisionLevel >= 5)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "subDivisionLevel", kMethodName));

            IPointEntity[] points = vertices.ConvertAll(DSGeometryExtension.ToEntity<DSPoint, IPointEntity>);
            if (points.Length < 3 || points.ArePointsColinear())
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "vertices", kMethodName));

            ISubDMeshEntity entity = HostFactory.Factory.SubDMeshByVerticesFaceIndices(points, faceIndices, subDivisionLevel);
            if (null == entity)
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));

            DSSubDivisionMesh mesh = new DSSubDivisionMesh(entity, true);
            try
            {
                if (null != vertexNormals && vertexNormals.Length > 0)
                {
                    if (vertexNormals.Length != entity.GetNumVertices())
                        throw new System.ArgumentException(string.Format(Properties.Resources.NotEqual, "size of vertexNormals", "number of vertices"), "vertexNormals");

                    if (!entity.UpdateSubDMeshNormals(vertexNormals.ConvertAll((DSVector v)=>v.IVector)))
                        throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));
                }

                if (null != vertexColors && vertexColors.Length > 0)
                {
                    if (subDivisionLevel > 0)
                        throw new System.InvalidOperationException(Properties.Resources.VertexColorNotSupported);

                    if (vertexColors.Length != entity.GetNumVertices())
                        throw new System.ArgumentException(string.Format(Properties.Resources.NotEqual, "size of vertexColors", "number of vertices"), "vertexColors");

                    if (!entity.UpdateSubDMeshColors(vertexColors.ConvertAll((DSColor c)=>c.IColor)))
                        throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));
                }
            }
            catch (System.Exception ex)
            {
                mesh.Dispose();
                throw ex;
            }

            mesh.SubDivisionLevel = subDivisionLevel;
            return mesh;
        }

        #endregion

        #region CORE_METHODS

        protected override DSGeometry Translate(DSVector offset)
        {
            DSSubDivisionMesh mesh = base.Translate(offset) as DSSubDivisionMesh;
            if (null != mesh)
            {
                mesh.SubDivisionLevel = SubDivisionLevel;
            }

            return mesh;
        }

        internal override DSGeometry TransformBy(ICoordinateSystemEntity csEntity)
        {
            DSSubDivisionMesh mesh = base.TransformBy(csEntity) as DSSubDivisionMesh;
            mesh.SubDivisionLevel = this.SubDivisionLevel;
            return mesh;
        }

        private static ISubDMeshEntity ByVerticesFaceIndicesCore(DSPoint[] vertices, int[][] faceIndices, int subDivisionLevel)
        {
            string kMethodName = "DSSubDivisionMesh.ByVerticesFaceIndices";
            if (subDivisionLevel < 0 || subDivisionLevel >= 5)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "subDivisionLevel", kMethodName));

            IPointEntity[] points = vertices.ConvertAll(DSGeometryExtension.ToEntity<DSPoint, IPointEntity>);
            if (points.Length < 3 || points.ArePointsColinear())
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "vertices", kMethodName));

            ISubDMeshEntity entity = HostFactory.Factory.SubDMeshByVerticesFaceIndices(points, faceIndices, subDivisionLevel);
            if (null == entity)
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));
            return entity;
        }
        
        #endregion

        #region DESIGNSCRIPT_METHODS

        /// <summary>
        /// Extracts the polygonal faces from the given mesh and returns them 
        /// as an array of Polygons.
        /// </summary>
        /// <returns>Array of Polygons.</returns>
        public DSPolygon[] ExtractPolygon()
        {
            int nFaces = FaceIndices.Length;
            List<IPolygonEntity> polygons = new List<IPolygonEntity>();
            for (int iFace = 0; iFace < nFaces; ++iFace )
            {
                int nVerts = FaceIndices[iFace].Length;
                IPointEntity[] vertices = new IPointEntity[nVerts];
                for (int i = 0; i < nVerts; ++i)
                    vertices[i] = Vertices[FaceIndices[iFace][i]].PointEntity;

                IPolygonEntity polygon = HostFactory.Factory.PolygonByVertices(vertices);
                if(null == polygon)
                    continue;

                polygons.Add(polygon);
            }

            return polygons.ToArray().ToArray<DSPolygon, IPolygonEntity>(true); //don't persist returned polygons.
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bConvertAsSmooth"></param>
        /// <returns></returns>
        public DSSolid ConvertToSolid(bool bConvertAsSmooth)
        {
            ISolidEntity entity = SubDMeshEntity.ConvertToSolid(bConvertAsSmooth);
            if (null == entity)
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, "DSSubDivisionMesh.ConvertToSolid"));

            return entity.ToSolid(true, this);
        }

        #endregion
    }
}
