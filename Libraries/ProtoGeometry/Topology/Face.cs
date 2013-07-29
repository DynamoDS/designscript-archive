using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    public class Face : Topology
    {
        #region INTERNAL_METHODS

        internal IFaceEntity FaceEntity { get { return HostImpl as IFaceEntity; } }

        internal Face(IFaceEntity host) : base(host) { }

        protected override Geometry GetGeometryCore(out bool autodispose)
        {
            ISurfaceEntity surf = FaceEntity.GetSurfaceGeometry();
            autodispose = true;
            return Geometry.ToGeometry(surf);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GeometryExtension.DisposeObject(ref mCellFaces);
                GeometryExtension.DisposeObject(ref mAdjacentCells);
                GeometryExtension.DisposeObject(ref mEdges);
                GeometryExtension.DisposeObject(ref mVertices);

                GeometryExtension.DisposeObject(ref mShell);
                GeometryExtension.DisposeObject(ref mCentroid);
            }
            base.Dispose(disposing);
        }

        #endregion

        #region DESIGNSCRIPT_PROPERTIES

        /// <summary>
        /// Accesses the surface geometry of the Face
        /// </summary>
        public Surface SurfaceGeometry
        {
            get { return Geometry as Surface; }
        }

        /// <summary>
        /// Accesses the Cells the Face belongs to
        /// </summary>
        internal CellFace[] CellFaces
        {
            get
            {
                if (null == mCellFaces)
                {
                    ICellFaceEntity[] faces = FaceEntity.GetCellFaces();
                    mCellFaces = faces.ConvertAll((ICellFaceEntity host) => new CellFace(host));
                }

                return mCellFaces;
            }
        }

        /// <summary>
        /// Accesses the containing Shell that the Face belongs to 
        /// </summary>
        public Shell ContainingShell
        {
            get
            {
                if (null == mShell)
                {
                    IShellEntity entity = FaceEntity.GetShell();
                    if (null == entity)
                        return null;

                    mShell = new Shell(entity);
                }

                return mShell;
            }
        }

        /// <summary>
        /// Accesses the adjacent Cells the Face belongs to
        /// </summary>
        public Cell[] AdjacentCells
        {
            get
            {
                if (null == mAdjacentCells)
                {
                    ICellEntity[] cells = FaceEntity.GetCells();
                    mAdjacentCells = cells.ConvertAll((ICellEntity host) => new Cell(host));
                }

                return mAdjacentCells;
            }
        }

        /// <summary>
        /// Accesses the Face Edges 
        /// </summary>
        public Edge[] Edges
        {
            get
            {
                if (null == mEdges)
                {
                    IEdgeEntity[] edges = FaceEntity.GetEdges();
                    mEdges = edges.ConvertAll((IEdgeEntity host) => new Edge(host));
                }

                return mEdges;
            }
        }

        /// <summary>
        /// Accesses the Face Vertices 
        /// </summary>
        public Vertex[] Vertices
        {
            get
            {
                if (null == mVertices)
                {
                    IVertexEntity[] vertices = FaceEntity.GetVertices();
                    mVertices = vertices.ConvertAll((IVertexEntity host) => new Vertex(host));
                }

                return mVertices;
            }
        }

        /// <summary>
        /// Accesses the centroid of the Face
        /// </summary>
        public Point Centroid
        {
            get
            {
                if (null == mCentroid)
                    mCentroid = FaceEntity.GetCentroid().ToPoint(false, null);

                return mCentroid;
            }
        }

        /// <summary>
        /// Accesses the surface area of the Face
        /// </summary>
        public double Area
        {
            get
            {
                if (null == mArea)
                    mArea = FaceEntity.GetArea();
                return mArea.Value;
            }
        }

        /// <summary>
        /// Accesses the Type of the Face - InsideOutside, DoubleInside, OR DoubleOutside
        /// </summary>
        public string FaceType
        {
            get { return FaceEntity.GetFaceType(); }
        }

        /// <summary>
        /// Checks if the Face is planar
        /// </summary>
        public bool IsPlanar
        {
            get { return FaceEntity.IsPlanar(); }
        }

        #endregion

        #region OBJECT_METHODS

        public override string ToString()
        {
            int nFaces = FaceEntity.GetCellFaceCount();
            int nCells = FaceEntity.GetCellCount();
            int nEdges = FaceEntity.GetEdgeCount();
            int nVertices = FaceEntity.GetVertexCount();

            return string.Format("Face(Vertices = {0}, Edges = {1}, AdjacentCells = {2}, CellFaces = {3}, FaceType = {4})", nVertices, nEdges, nCells, nFaces, FaceType);
        }

        #endregion

        #region DATA MEMBERS

        private CellFace[] mCellFaces;
        private Shell mShell;
        private Cell[] mAdjacentCells;
        private Edge[] mEdges;
        private Vertex[] mVertices;
        private Point mCentroid;
        private double? mArea;

        #endregion
    }
}
