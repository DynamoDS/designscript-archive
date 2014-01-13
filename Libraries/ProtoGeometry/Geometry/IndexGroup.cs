using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>
    /// A type representing a collection of indices. This is useful for representing a face in a mesh.
    /// </summary>
    public class IndexGroup
    {
        internal IIndexGroupEntity IndexGroupEntity;

        private IndexGroup(IIndexGroupEntity entity)
        {
            this.IndexGroupEntity = entity;
        }

        #region Public properties

        public int A
        {
            get
            {
                return IndexGroupEntity.A;
            }
        }

        public int B
        {
            get
            {
                return IndexGroupEntity.B;
            }
        }

        public int C
        {
            get
            {
                return IndexGroupEntity.C;
            }
        }

        public int D
        {
            get
            {
                return IndexGroupEntity.D;
            }
        }

        public int Count
        {
            get
            {
                return IndexGroupEntity.Count;
            }
        }

        #endregion

        #region Public static constructors

        public static IndexGroup ByIndices(int a, int b, int c, int d)
        {
            return new IndexGroup(HostFactory.Factory.IndexGroupByIndices(a, b, c, d));
        }

        public static IndexGroup ByIndices(int a, int b, int c)
        {
            return new IndexGroup(HostFactory.Factory.IndexGroupByIndices(a, b, c));
        }

        #endregion

    }
}
