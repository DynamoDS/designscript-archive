using System.Collections;
using System.Collections.Generic;
using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    public class Cell : Topology
    {
        #region INTERNAL_METHODS

        internal ICellEntity CellEntity { get { return HostImpl as ICellEntity; } }

        internal Cell(ICellEntity host) : base(host) {}

        protected override Geometry GetGeometryCore(out bool autodispose)
        {
            ISolidEntity entity = CellEntity.GetSolidGeometry();
            autodispose = true;
            return Geometry.ToGeometry(entity);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GeometryExtension.DisposeObject(ref mFaces);
                GeometryExtension.DisposeObject(ref mAdjacentCells);
                GeometryExtension.DisposeObject(ref mCentroid);
            }
            base.Dispose(disposing);
        }

        #endregion

        #region DESIGNSCRIPT_PROPERTIES

        /// <summary>
        /// Accesses the Solid geometry of the Cell
        /// </summary>
        public Solid SolidGeometry
        {
            get { return Geometry as Solid; }
        }

        /// <summary>
        /// 
        /// </summary>
        public Face[] Faces
        {
            get
            {
                if (null == mFaces)
                {
                    ICellFaceEntity[] cellFaces = CellEntity.GetFaces();
                    mFaces = cellFaces.ConvertAll((ICellFaceEntity h) => new Face(h.GetFace()));
                }
                return mFaces;
            }
        }

        /// <summary>
        /// Accesses the Cells adjacent to the given Cell
        /// </summary>
        public Cell[] AdjacentCells
        {
            get
            {
                if (null == mAdjacentCells)
                {
                    ICellEntity[] cells = CellEntity.GetAdjacentCells();
                    mAdjacentCells = cells.ConvertAll((ICellEntity h) => new Cell(h));
                }

                return mAdjacentCells;
            }
        }

        /// <summary>
        /// Accesses the center of gravity of the Cell
        /// </summary>
        public Point Centroid
        {
            get
            {
                if (null == mCentroid)
                    mCentroid = CellEntity.GetCentroid().ToPoint(false, null);

                return mCentroid;
            }
        }

        /// <summary>
        /// Accesses the volume of the Cell
        /// </summary>
        public double Volume
        {
            get
            {
                if (null == mVolume)
                    mVolume = CellEntity.GetVolume();
                return mVolume.Value;
            }
        }

        /// <summary>
        /// Accesses the surface area of the Cell
        /// </summary>
        public double Area
        {
            get
            {
                if (null == mArea)
                    mArea = CellEntity.GetArea();

                return mArea.Value;
            }
        }

        #endregion

        #region OBJECT_METHODS

        public override string ToString()
        {
            int nFaces = CellEntity.GetFaceCount();

            return string.Format("Cell(CellFaces = {0})", nFaces);
        }

        #endregion

        #region DATA MEMBERS

        private Face[] mFaces;
        private Cell[] mAdjacentCells;
        private Point mCentroid;
        private double? mVolume;
        private double? mArea;

        #endregion
    }
}
