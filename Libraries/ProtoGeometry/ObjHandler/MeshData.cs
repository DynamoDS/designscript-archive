using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autodesk.DesignScript.Geometry
{
    internal class MeshData
    {
        Group currentGroup;
        public List<Point> Vertices { get; private set; }
        public List<Vector> Normals { get; private set; }
        public List<Group> Groups { get; private set; }
        //public List<Texture> Textures;
        //public List<Material> Materials;

        public MeshData()
        {
            Vertices = new List<Point>();
            Normals = new List<Vector>();
            Groups = new List<Group>();
            //Textures = new List<Texture>();
            //Materials = new List<Material>();
        }
        public bool AddVertex(String rawData)
        {
            string[] parts = rawData.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3)
                return false;
            double x, y, z;
            if (!Double.TryParse(parts[0], out x) || !Double.TryParse(parts[1], out y) || !Double.TryParse(parts[2], out z))
                return false;
            Vertices.Add(new Point(x, y, z,false));
            return true;
        }
        public bool AddFace(String rawData)
        {
            if (currentGroup == null)
                AddGroup("Default");
            string[] verticesInfo = rawData.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (verticesInfo.Length == 0)
                return false;
            MeshFace face = MeshFace.CreateFace(verticesInfo.Length);
            for (int i = 0; i < verticesInfo.Length; ++i )
            {
                string[] parts = verticesInfo[i].Split(new[] { '/' }, StringSplitOptions.None);
                if (parts.Length == 0)
                    return false;
                int vertex, texture = 0, normal = 0;
                if (!int.TryParse(parts[0], out vertex))
                    return false;
                if (parts.Length > 1 && !String.IsNullOrWhiteSpace(parts[1]) && !int.TryParse(parts[1], out texture))
                    return false;
                if (parts.Length > 2 && !String.IsNullOrWhiteSpace(parts[2]) && !int.TryParse(parts[2], out normal))
                    return false;
                face.AddFacePoint(vertex - 1, texture - 1, normal - 1, i);
            }
            currentGroup.Faces.Add(face);
            return true;
        }
        public bool AddNormal(String rawData)
        {
            string[] parts = rawData.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3)
                return false;
            double x, y, z;
            if (!Double.TryParse(parts[0], out x) || !Double.TryParse(parts[1], out y) || !Double.TryParse(parts[2], out z))
                return false;
            Normals.Add(new Vector(x, y, z));
            return true;
        }
        public bool AddGroup(String rawData)
        {
            currentGroup = new Group(rawData);
            Groups.Add(currentGroup);
            return true;
        }
        public bool AddTexture(String rawData)
        {
            //string[] parts = rawData.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            //if (parts.Length != 2)
            //    return false;
            //double x, y;
            //if (!Double.TryParse(parts[0], out x) || !Double.TryParse(parts[1], out y))
            //    return false;
            //Textures.Add(new Texture(x, y));
            return true;
        }

        public bool AddMaterial(String rawData)
        {
            //Material material = new Material(rawData);
            //Materials.Add(material);
            return true;
        }

        internal void SetMaterial(String rawData)
        {
            //if (currentGroup == null)
            //    AddGroup("Default");
            //currentGroup.material = Materials.SingleOrDefault(x => x.Name.Equals(rawData));
        }
        public SubDivisionMesh[] ConvertToSubDivisionMesh()
        {
            List<SubDivisionMesh> importedMeshes = new List<SubDivisionMesh>();
            foreach (var group in this.Groups)
            {
                List<int[]> faces = new List<int[]>();
                SortedList<int, Point> sortedIndexPointList = new SortedList<int, Point>();
                //create the used points list
                foreach (var face in group.Faces)
                {
                    foreach (var vertex in face.VertexArray)
                        sortedIndexPointList[vertex] = this.Vertices[vertex];
                }

                //get the indices from the used point list and add face
                foreach (var face in group.Faces)
                {
                    int[] newVertexArray = new int[face.VertexArray.Length];
                    for (int i = 0; i < face.VertexArray.Length; ++i)
                        newVertexArray[i] = sortedIndexPointList.IndexOfKey(face.VertexArray[i]);
                    faces.Add(newVertexArray);
                }

                if (faces.Count == 0)
                    continue;
                Point[] usedPointsList = new Point[sortedIndexPointList.Count];
                sortedIndexPointList.Values.CopyTo(usedPointsList, 0);
                try
                {
                    importedMeshes.Add(new SubDivisionMesh(usedPointsList, faces.ToArray(), 0, true));
                }
                catch (System.InvalidOperationException)
                {
                }
            }
            return importedMeshes.ToArray();
        }
    }
}
