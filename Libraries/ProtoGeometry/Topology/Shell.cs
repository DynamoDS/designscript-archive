using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>
    /// Represents Shell topology of a BRep solid.
    /// </summary>
    public class Shell : Topology
    {
        #region INTERNAL_METHODS

        internal IShellEntity ShellEntity { get { return HostImpl as IShellEntity; } }

        internal Shell(IShellEntity host) : base(host) { }

        protected override Geometry GetGeometryCore(out bool autodispose)
        {
            ISolidEntity solid = ShellEntity.GetSolidGeometry();
            autodispose = true;
            return Geometry.ToGeometry(solid);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GeometryExtension.DisposeObject(ref mFaces);
            }
            base.Dispose(disposing);
        }

        #endregion

        #region DESIGNSCRIPT_PROPERTIES

        /// <summary>
        /// Accesses the faces of the Shell
        /// </summary>
        public Face[] Faces
        {
            get
            {
                if (null == mFaces)
                {
                    IFaceEntity[] faces = ShellEntity.GetFaces();
                    mFaces = faces.ConvertAll((IFaceEntity e) => new Face(e));
                }

                return mFaces;
            }
        }

        /// <summary>
        /// Accesses the Solid geometry of the Shell
        /// </summary>
        public Solid SolidGeometry
        {
            get { return Geometry as Solid; }
        }

        #endregion

        #region OBJECT_METHODS

        public override string ToString()
        {
            int nFaces = ShellEntity.GetFaceCount();

            return string.Format("Shell(Faces = {0})", nFaces);
        }

        #endregion

        #region PRIVATE_MEMBERS

        private Face[] mFaces;
        #endregion
    }
}
