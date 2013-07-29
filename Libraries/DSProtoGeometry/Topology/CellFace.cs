using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    internal class DSCellFace : DSTopology
    {
        #region INTERNAL_METHODS

        internal ICellFaceEntity FaceEntity { get { return HostImpl as ICellFaceEntity; } }

        internal DSCellFace(ICellFaceEntity host) : base(host) { }

        protected override DSGeometry GetGeometryCore(out bool autodispose)
        {
            throw new System.NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DSGeometryExtension.DisposeObject(ref mContainingCell);
                DSGeometryExtension.DisposeObject(ref mContainingFace);
            }
            base.Dispose(disposing);
        }

        #endregion

        #region DESIGNSCRIPT_PROPERTIES

        /// <summary>
        /// Accesses the parent Cell to which the CellFace belongs
        /// </summary>
        public DSCell ContainingCell
        {
            get
            {
                if (null == mContainingCell)
                {
                    ICellEntity cell = FaceEntity.GetCell();
                    if (null == cell)
                        return null;
                    mContainingCell = new DSCell(cell);
                }

                return mContainingCell;
            }
        }

        /// <summary>
        /// Accesses the parent Face to which the CellFace belongs 
        /// </summary>
        public DSFace ContainingFace
        {
            get
            {
                if (null == mContainingFace)
                {
                    IFaceEntity entity = FaceEntity.GetFace();
                    if (null == entity)
                        return null;

                    mContainingFace = new DSFace(entity);
                }

                return mContainingFace;
            }
        }

        #endregion

        #region OBJECT_METHODS

        public override string ToString()
        {
            return string.Format("DSCellFace()");
        }

        #endregion

        #region DATA MEMBERS

        private DSCell mContainingCell;
        private DSFace mContainingFace;

        #endregion
    }
}
