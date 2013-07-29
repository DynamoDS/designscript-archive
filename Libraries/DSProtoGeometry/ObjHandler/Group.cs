using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autodesk.DesignScript.Geometry
{
    class Group
    {
        public String Name { get; set; }
        public List<DSMeshFace> Faces { get; private set; }
        //public Material material { get; set; }
        public Group(String name)
        {
            Name = name;
            Faces = new List<DSMeshFace>();
        }
        public bool AddFace(int[][] facePoints)
        {
            if (facePoints.Length < 3)
                return false;
            DSMeshFace face = DSMeshFace.CreateFace(facePoints.Length);
            for(int i=0;i< facePoints.Length; ++i)
            {
                if(facePoints[i].Length != 3)
                    return false;
                face.AddFacePoint(facePoints[i][0],facePoints[i][1],facePoints[i][2],i);
            }
            Faces.Add(face);
            return true;
        }
        public bool AddFace(int[] vertexPoints)
        {
            if (vertexPoints.Length < 3)
                return false;
            DSMeshFace face = DSMeshFace.CreateFace(vertexPoints.Length);
            for (int i = 0; i < vertexPoints.Length; ++i)
                face.AddFacePoint(vertexPoints[i], i);
            Faces.Add(face);
            return true;
        }
    }
}
