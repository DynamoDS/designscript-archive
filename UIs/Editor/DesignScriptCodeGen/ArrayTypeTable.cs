using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoCore;

namespace DesignScript.Editor.CodeGen
{
    class ScopedArray
    {
        private string arrayName = string.Empty;
        private ScopeInfo scopeInfo = default(ScopeInfo);

        internal ScopedArray(ScopeInfo scopeInfo, string name)
        {
            this.arrayName = name;
            this.scopeInfo = scopeInfo;
        }

        public override bool Equals(object obj)
        {
            ScopedArray rhs = obj as ScopedArray;
            if (((object)rhs) == null)
                return false;

            return ((this.scopeInfo == rhs.scopeInfo) && (this.arrayName == rhs.arrayName));
        }

        public override int GetHashCode()
        {
            return (arrayName.GetHashCode()) ^ (scopeInfo.GetHashCode());
        }

        public static bool operator ==(ScopedArray lhs, ScopedArray rhs)
        {
            if (System.Object.ReferenceEquals(lhs, rhs))
                return true;

            if (((object)lhs) == null || (((object)rhs) == null))
                return false;

            return ((lhs.scopeInfo == rhs.scopeInfo) && (lhs.arrayName == rhs.arrayName));
        }

        public static bool operator !=(ScopedArray lhs, ScopedArray rhs)
        {
            return !(lhs == rhs);
        }

        internal string ArrayName { get { return arrayName; } }
        internal ScopeInfo ScopeInfo { get { return scopeInfo; } }
    }

    class ArrayElementType
    {
        int elementDepth = -1;
        ProtoCore.Type elementType;
        List<ArrayElementType> childNodes = null;

        #region Public Class Operational Methods

        internal ArrayElementType()
        {
            // This creates a root node representing the array.
            elementType.UID = (int)PrimitiveType.kInvalidType;
        }

        internal ArrayElementType AppendChildType(ProtoCore.Type type)
        {
            if (null == childNodes)
                childNodes = new List<ArrayElementType>();

            ArrayElementType element = new ArrayElementType(
                type, this.elementDepth + 1);

            childNodes.Add(element);
            return element;
        }

        internal void AppendChild(ArrayElementType childElementType)
        {
            if (null == childNodes)
                childNodes = new List<ArrayElementType>();

            childNodes.Add(childElementType);
        }

        internal ArrayElementType GetElementFromIndices(string[] indices)
        {
            // First index should be the identifier name.
            if (null == indices || (indices.Length < 2))
                return null;

            // So we start searching from the second.
            return GetElementFromIndices(indices, 1);
        }

        #endregion

        #region Public Class Properties

        internal int Depth { get { return this.elementDepth; } }
        internal bool IsLeafElement { get { return (null == childNodes); } }
        internal ProtoCore.Type Type { get { return elementType; } }

        #endregion

        #region Private Class Helper Methods

        private ArrayElementType(ProtoCore.Type type, int depth)
        {
            this.elementDepth = depth;
            this.elementType = type;
        }

        private ArrayElementType GetElementFromIndices(string[] indices, int level)
        {
            if (null == childNodes || (childNodes.Count <= 0))
                return null;

            int elementIndex = -1;
            if (int.TryParse(indices[level], out elementIndex) == false)
                return null;

            if (elementIndex < 0 || (elementIndex >= childNodes.Count))
                return null; // Invalid array index specified.

            ArrayElementType element = childNodes[elementIndex];
            if (level == indices.Length - 1)
                return element;

            return (element.GetElementFromIndices(indices, level + 1));
        }

        #endregion
    }

    class ArrayTypeTable
    {
        Dictionary<ScopedArray, ArrayElementType> arrayTypeTable = null;

        internal void InsertRootElementType(ScopedArray scopedArray, ArrayElementType rootElement)
        {
            if (null != scopedArray)
            {
                if (null == arrayTypeTable)
                    arrayTypeTable = new Dictionary<ScopedArray, ArrayElementType>();

                arrayTypeTable.Add(scopedArray, rootElement);
            }
        }

        internal int GetArrayElementType(ScopeInfo scopeInfo, string[] identList)
        {
            if (null == arrayTypeTable)
                return ((int)PrimitiveType.kInvalidType);

            string arrayName = identList[0];
            foreach (KeyValuePair<ScopedArray, ArrayElementType> arrayType in arrayTypeTable)
            {
                if (arrayType.Key.ScopeInfo == scopeInfo)
                {
                    if (arrayType.Key.ArrayName == arrayName)
                    {
                        ArrayElementType arrayRoot = arrayType.Value;
                        ArrayElementType element = arrayRoot.GetElementFromIndices(identList);
                        if (null != element)
                            return element.Type.UID;
                    }
                }
            }

            return ((int)PrimitiveType.kInvalidType);
        }
    }
}
