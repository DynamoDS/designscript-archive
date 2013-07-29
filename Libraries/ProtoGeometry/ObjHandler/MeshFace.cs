using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autodesk.DesignScript.Geometry
{
    class MeshFace
    {
        public int[] VertexArray { get; private set; }
        public int[] TextureArray { get; private set; }
        public int[] NormalArray { get; private set; }
        private MeshFace(int numberOfVertices)
        {
            VertexArray = new int[numberOfVertices];
            TextureArray = new int[numberOfVertices];
            NormalArray = new int[numberOfVertices];
        }
        public static MeshFace CreateFace(int numberOfVertices)
        {
            if (numberOfVertices < 3)
                return null;
            return new MeshFace(numberOfVertices);
        }
        public void AddFacePoint(int vertexIndex, int textureIndex, int normalIndex, int index)
        {
            VertexArray[index] = vertexIndex;
            TextureArray[index] = textureIndex;
            NormalArray[index] = normalIndex;
        }
        public void AddFacePoint(int vertexIndex, int index)
        {
            VertexArray[index] = vertexIndex;
            TextureArray[index] = 0;
            NormalArray[index] = 0;
        }
    }
}
