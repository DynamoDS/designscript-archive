using System.Collections;
using System.Collections.Generic;
using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    public class DSCell : DSTopology
    {
        #region INTERNAL_METHODS

        internal ICellEntity CellEntity { get { return HostImpl as ICellEntity; } }

        internal DSCell(ICellEntity host) : base(host) {}

        protected override DSGeometry GetGeometryCore(out bool autodispose)
        {
            ISolidEntity entity = CellEntity.GetSolidGeometry();
            autodispose = true;
            return DSGeometry.ToGeometry(entity);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DSGeometryExtension.DisposeObject(ref mFaces);
                DSGeometryExtension.DisposeObject(ref mAdjacentCells);
                DSGeometryExtension.DisposeObject(ref mCentroid);
            }
            base.Dispose(disposing);
        }

        #endregion

        #region DESIGNSCRIPT_PROPERTIES

        /// <summary>
        /// Accesses the Solid geometry of the Cell
        /// </summary>
        public DSSolid SolidGeometry
        {
            get { return Geometry as DSSolid; }
        }

        /// <summary>
        /// 
        /// </summary>
        public DSFace[] Faces
        {
            get
            {
                if (null == mFaces)
                {
                    ICellFaceEntity[] cellFaces = CellEntity.GetFaces();
                    mFaces = cellFaces.ConvertAll((ICellFaceEntity h) => new DSFace(h.GetFace()));
                }
                return mFaces;
            }
        }

        /// <summary>
        /// Accesses the Cells adjacent to the given Cell
        /// </summary>
        public DSCell[] AdjacentCells
        {
            get
            {
                if (null == mAdjacentCells)
                {
                    ICellEntity[] cells = CellEntity.GetAdjacentCells();
                    mAdjacentCells = cells.ConvertAll((ICellEntity h) => new DSCell(h));
                }

                return mAdjacentCells;
            }
        }

        /// <summary>
        /// Accesses the center of gravity of the Cell
        /// </summary>
        public DSPoint Centroid
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

            return string.Format("DSCell(CellFaces = {0})", nFaces);
        }

        #endregion

        #region DATA MEMBERS

        private DSFace[] mFaces;
        private DSCell[] mAdjacentCells;
        private DSPoint mCentroid;
        private double? mVolume;
        private double? mArea;

        #endregion
    }
}
