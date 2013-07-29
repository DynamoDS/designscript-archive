using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    public class DSNonManifoldSolid : DSSolid
    {
        #region PRIVATE_CONSTRUCTORS

        static void InitType()
        {
            RegisterHostType(typeof(ISolidEntity), DSNonManifoldSolid.CreateSolid);
        }

        static DSSolid CreateSolid(IGeometryEntity host, bool persist)
        {
            ISolidEntity entity = host as ISolidEntity;
            if (null == entity)
                return null;
            if (entity.IsNonManifold())
                return new DSNonManifoldSolid(entity, persist);

            return DSSolid.CreateSolid(entity, persist);
        }

        internal DSNonManifoldSolid(ISolidEntity entity, bool persist = false)
            : base(entity, persist) 
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                DSGeometryExtension.DisposeObject(ref mCells);

            base.Dispose(disposing);
        }
        #endregion

        #region DESIGNSCRIPT_METHODS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="internalFaceThickness"></param>
        /// <param name="externalFaceThickness"></param>
        /// <returns></returns>
        public DSSolid ThinShell(double internalFaceThickness, double externalFaceThickness)
        {
            if (internalFaceThickness.EqualsTo(0.0) || internalFaceThickness < 0)
                return null;

            if (externalFaceThickness.EqualsTo(0.0) || externalFaceThickness < 0)
                return null;

            ISolidEntity host = SolidEntity.ThinShell(internalFaceThickness, externalFaceThickness);
            if (null == host)
                return null;

            return host.ToSolid(true, this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DSSolid Regularise()
        {
            ISolidEntity host = SolidEntity.Regularise();
            if (host == null)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "DSSolid.Regularise"));
            return host.ToSolid(true, this);
        }

        #endregion

        #region DESIGNSCRIPT_PROPERTIES

        public DSCell[] Cells 
        {
            get
            {
                if (null == mCells)
                    mCells = SolidEntity.GetCells().ConvertAll((ICellEntity c) => new DSCell(c));

                return mCells;
            }
        }

        private DSCell[] mCells;
        #endregion

        #region PROTECTED_METHODS

        internal override IGeometryEntity[] IntersectWithSurface(DSSurface surf)
        {
            throw new System.InvalidOperationException(string.Format(Properties.Resources.InvalidIntersect, GetType().Name, "DSSurface"));
        }

        #endregion

        #region OBJECT_METHODS

        public override string ToString()
        {
            int nCells = SolidEntity.GetCellCount();

            return string.Format("DSNonManifoldSolid({0}, Cells = {1})", GetTopologyInfo(), nCells);
        }

        #endregion
    }
}
