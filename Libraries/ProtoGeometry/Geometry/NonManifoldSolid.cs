using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    public class NonManifoldSolid : Solid
    {
        #region PRIVATE_CONSTRUCTORS

        static void InitType()
        {
            RegisterHostType(typeof(ISolidEntity), NonManifoldSolid.CreateSolid);
        }

        static Solid CreateSolid(IGeometryEntity host, bool persist)
        {
            ISolidEntity entity = host as ISolidEntity;
            if (null == entity)
                return null;
            if (entity.IsNonManifold())
                return new NonManifoldSolid(entity, persist);

            return Solid.CreateSolid(entity, persist);
        }

        internal NonManifoldSolid(ISolidEntity entity, bool persist = false)
            : base(entity, persist) 
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                GeometryExtension.DisposeObject(ref mCells);

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
        public Solid ThinShell(double internalFaceThickness, double externalFaceThickness)
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
        public Solid Regularise()
        {
            ISolidEntity host = SolidEntity.Regularise();
            if (host == null)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "Solid.Regularise"));
            return host.ToSolid(true, this);
        }

        #endregion

        #region DESIGNSCRIPT_PROPERTIES

        public Cell[] Cells 
        {
            get
            {
                if (null == mCells)
                    mCells = SolidEntity.GetCells().ConvertAll((ICellEntity c) => new Cell(c));

                return mCells;
            }
        }

        private Cell[] mCells;
        #endregion

        #region PROTECTED_METHODS

        internal override IGeometryEntity[] IntersectWithSurface(Surface surf)
        {
            throw new System.InvalidOperationException(string.Format(Properties.Resources.InvalidIntersect, GetType().Name, "Surface"));
        }

        #endregion

        #region OBJECT_METHODS

        public override string ToString()
        {
            int nCells = SolidEntity.GetCellCount();

            return string.Format("NonManifoldSolid({0}, Cells = {1})", GetTopologyInfo(), nCells);
        }

        #endregion
    }
}
