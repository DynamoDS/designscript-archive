using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>
    /// Represents Shell topology of a BRep solid.
    /// </summary>
    public class DSShell : DSTopology
    {
        #region INTERNAL_METHODS

        internal IShellEntity ShellEntity { get { return HostImpl as IShellEntity; } }

        internal DSShell(IShellEntity host) : base(host) { }

        protected override DSGeometry GetGeometryCore(out bool autodispose)
        {
            ISolidEntity solid = ShellEntity.GetSolidGeometry();
            autodispose = true;
            return DSGeometry.ToGeometry(solid);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DSGeometryExtension.DisposeObject(ref mFaces);
            }
            base.Dispose(disposing);
        }

        #endregion

        #region DESIGNSCRIPT_PROPERTIES

        /// <summary>
        /// Accesses the faces of the Shell
        /// </summary>
        public DSFace[] Faces
        {
            get
            {
                if (null == mFaces)
                {
                    IFaceEntity[] faces = ShellEntity.GetFaces();
                    mFaces = faces.ConvertAll((IFaceEntity e) => new DSFace(e));
                }

                return mFaces;
            }
        }

        /// <summary>
        /// Accesses the Solid geometry of the Shell
        /// </summary>
        public DSSolid SolidGeometry
        {
            get { return Geometry as DSSolid; }
        }

        #endregion

        #region OBJECT_METHODS

        public override string ToString()
        {
            int nFaces = ShellEntity.GetFaceCount();

            return string.Format("DSShell(Faces = {0})", nFaces);
        }

        #endregion

        #region PRIVATE_MEMBERS

        private DSFace[] mFaces;
        #endregion
    }
}
