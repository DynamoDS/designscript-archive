using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    internal class CellFace : Topology
    {
        #region INTERNAL_METHODS

        internal ICellFaceEntity FaceEntity { get { return HostImpl as ICellFaceEntity; } }

        internal CellFace(ICellFaceEntity host) : base(host) { }

        protected override Geometry GetGeometryCore(out bool autodispose)
        {
            throw new System.NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GeometryExtension.DisposeObject(ref mContainingCell);
                GeometryExtension.DisposeObject(ref mContainingFace);
            }
            base.Dispose(disposing);
        }

        #endregion

        #region DESIGNSCRIPT_PROPERTIES

        /// <summary>
        /// Accesses the parent Cell to which the CellFace belongs
        /// </summary>
        public Cell ContainingCell
        {
            get
            {
                if (null == mContainingCell)
                {
                    ICellEntity cell = FaceEntity.GetCell();
                    if (null == cell)
                        return null;
                    mContainingCell = new Cell(cell);
                }

                return mContainingCell;
            }
        }

        /// <summary>
        /// Accesses the parent Face to which the CellFace belongs 
        /// </summary>
        public Face ContainingFace
        {
            get
            {
                if (null == mContainingFace)
                {
                    IFaceEntity entity = FaceEntity.GetFace();
                    if (null == entity)
                        return null;

                    mContainingFace = new Face(entity);
                }

                return mContainingFace;
            }
        }

        #endregion

        #region OBJECT_METHODS

        public override string ToString()
        {
            return string.Format("CellFace()");
        }

        #endregion

        #region DATA MEMBERS

        private Cell mContainingCell;
        private Face mContainingFace;

        #endregion
    }
}
