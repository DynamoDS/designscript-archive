using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    public class DSFace : DSTopology
    {
        #region INTERNAL_METHODS

        internal IFaceEntity FaceEntity { get { return HostImpl as IFaceEntity; } }

        internal DSFace(IFaceEntity host) : base(host) { }

        protected override DSGeometry GetGeometryCore(out bool autodispose)
        {
            ISurfaceEntity surf = FaceEntity.GetSurfaceGeometry();
            autodispose = true;
            return DSGeometry.ToGeometry(surf);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DSGeometryExtension.DisposeObject(ref mCellFaces);
                DSGeometryExtension.DisposeObject(ref mAdjacentCells);
                DSGeometryExtension.DisposeObject(ref mEdges);
                DSGeometryExtension.DisposeObject(ref mVertices);

                DSGeometryExtension.DisposeObject(ref mShell);
                DSGeometryExtension.DisposeObject(ref mCentroid);
            }
            base.Dispose(disposing);
        }

        #endregion

        #region DESIGNSCRIPT_PROPERTIES

        /// <summary>
        /// Accesses the surface geometry of the Face
        /// </summary>
        public DSSurface SurfaceGeometry
        {
            get { return Geometry as DSSurface; }
        }

        /// <summary>
        /// Accesses the Cells the Face belongs to
        /// </summary>
        internal DSCellFace[] CellFaces
        {
            get
            {
                if (null == mCellFaces)
                {
                    ICellFaceEntity[] faces = FaceEntity.GetCellFaces();
                    mCellFaces = faces.ConvertAll((ICellFaceEntity host) => new DSCellFace(host));
                }

                return mCellFaces;
            }
        }

        /// <summary>
        /// Accesses the containing Shell that the Face belongs to 
        /// </summary>
        public DSShell ContainingShell
        {
            get
            {
                if (null == mShell)
                {
                    IShellEntity entity = FaceEntity.GetShell();
                    if (null == entity)
                        return null;

                    mShell = new DSShell(entity);
                }

                return mShell;
            }
        }

        /// <summary>
        /// Accesses the adjacent Cells the Face belongs to
        /// </summary>
        public DSCell[] AdjacentCells
        {
            get
            {
                if (null == mAdjacentCells)
                {
                    ICellEntity[] cells = FaceEntity.GetCells();
                    mAdjacentCells = cells.ConvertAll((ICellEntity host) => new DSCell(host));
                }

                return mAdjacentCells;
            }
        }

        /// <summary>
        /// Accesses the Face Edges 
        /// </summary>
        public DSEdge[] Edges
        {
            get
            {
                if (null == mEdges)
                {
                    IEdgeEntity[] edges = FaceEntity.GetEdges();
                    mEdges = edges.ConvertAll((IEdgeEntity host) => new DSEdge(host));
                }

                return mEdges;
            }
        }

        /// <summary>
        /// Accesses the Face Vertices 
        /// </summary>
        public DSVertex[] Vertices
        {
            get
            {
                if (null == mVertices)
                {
                    IVertexEntity[] vertices = FaceEntity.GetVertices();
                    mVertices = vertices.ConvertAll((IVertexEntity host) => new DSVertex(host));
                }

                return mVertices;
            }
        }

        /// <summary>
        /// Accesses the centroid of the Face
        /// </summary>
        public DSPoint Centroid
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

            return string.Format("DSFace(Vertices = {0}, Edges = {1}, AdjacentCells = {2}, CellFaces = {3}, FaceType = {4})", nVertices, nEdges, nCells, nFaces, FaceType);
        }

        #endregion

        #region DATA MEMBERS

        private DSCellFace[] mCellFaces;
        private DSShell mShell;
        private DSCell[] mAdjacentCells;
        private DSEdge[] mEdges;
        private DSVertex[] mVertices;
        private DSPoint mCentroid;
        private double? mArea;

        #endregion
    }
}
