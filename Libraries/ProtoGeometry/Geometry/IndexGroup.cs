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

        public uint A
        {
            get
            {
                return IndexGroupEntity.A;
            }
        }

        public uint B
        {
            get
            {
                return IndexGroupEntity.B;
            }
        }

        public uint C
        {
            get
            {
                return IndexGroupEntity.C;
            }
        }

        public uint D
        {
            get
            {
                return IndexGroupEntity.D;
            }
        }

        public uint Count
        {
            get
            {
                return IndexGroupEntity.Count;
            }
        }

        #endregion

        #region Public static constructors

        public static IndexGroup ByIndices(uint a, uint b, uint c, uint d)
        {
            return new IndexGroup(HostFactory.Factory.IndexGroupByIndices(a, b, c, d));
        }

        public static IndexGroup ByIndices(uint a, uint b, uint c)
        {
            return new IndexGroup(HostFactory.Factory.IndexGroupByIndices(a, b, c));
        }

        #endregion

    }
}
