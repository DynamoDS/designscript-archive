using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Autodesk.DesignScript.Geometry
{
    internal class ObjHandler
    {
        internal static DSMeshData Import(String filePath)
        {
            StreamReader fileStream = new StreamReader(filePath);
            DSMeshData meshData = new DSMeshData();
            List<string> invalidData = new List<string>();
            while (!fileStream.EndOfStream)
            {
                String line = fileStream.ReadLine().Trim();
                if (String.IsNullOrEmpty(line) || line[0] == '#')
                    continue;
                String[] data = line.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (data.Length < 2)
                {
                    invalidData.Add(line);
                    continue;
                }
                KeyValuePair<string, string> parsedLineData = new KeyValuePair<string, string>(data[0], data[1]);
                switch (parsedLineData.Key)
                {
                    case "v":
                        //vertex
                        if (!meshData.AddVertex(parsedLineData.Value))
                            invalidData.Add(line);
                        break;
                    case "f":
                        //face
                        if (!meshData.AddFace(parsedLineData.Value))
                            invalidData.Add(line);
                        break;
                    case "g":
                        //group
                        if (!meshData.AddGroup(parsedLineData.Value))
                            invalidData.Add(line);
                        break;
                    case "vn":
                        //vertex normal
                        if (!meshData.AddNormal(parsedLineData.Value))
                            invalidData.Add(line);
                        break;
                    case "vt":
                        //texture
                        if (!meshData.AddTexture(parsedLineData.Value))
                            invalidData.Add(line);
                        break;
                    case "mtllib":
                        //material library  
                        if (!meshData.AddMaterial(parsedLineData.Value))
                            invalidData.Add(line);
                        break;
                    case "usemtl":
                        //use material library
                        meshData.SetMaterial(parsedLineData.Value);
                        break;
                    default:
                        invalidData.Add(line);
                        break;
                }
            }
            return meshData;
        }
        internal static bool Export(DSMeshData meshData, String filePath)
        {
            try
            {
                StreamWriter file = File.CreateText(filePath);
                file.WriteLine("# 3D Library generated .obj  file");
                file.WriteLine("# DesignScript Studio #");
                file.WriteLine("##");
                file.WriteLine();

                foreach (var vertex in meshData.Vertices)
                    file.WriteLine("v " + vertex.X + " " + vertex.Y + " " + vertex.Z);
                file.WriteLine("# " + meshData.Vertices.Count + " vertices");
                file.WriteLine();

                foreach (var normal in meshData.Normals)
                    file.WriteLine("vn " + normal.X + " " + normal.Y + " " + normal.Z);
                file.WriteLine("# " + meshData.Normals.Count + " vertex normals");
                file.WriteLine();

                //foreach (var texture in meshData.Textures)
                //    file.WriteLine("vt " + texture.X + " " + texture.Y);
                //file.WriteLine("# " + meshData.Textures.Count + " texture vertices");

                foreach (var group in meshData.Groups)
                {
                    file.WriteLine("g " + group.Name);
                    foreach (var face in group.Faces)
                    {
                        file.Write("f");
                        for (int i = 0; i < face.VertexArray.Length; ++i)
                            file.Write(" " + face.VertexArray[i] + "/" + (face.TextureArray[i] != 0 ? face.TextureArray[i].ToString() : "") + "/" + (face.NormalArray[i] != 0 ? face.NormalArray[i].ToString() : ""));
                        file.WriteLine();
                    }
                    file.WriteLine("# " + group.Faces.Count + " faces");
                    file.WriteLine();
                }
                file.Close();
                return true;
            }
            catch (System.UnauthorizedAccessException e)
            {
                throw new System.UnauthorizedAccessException(e.Message);
            }
            catch
            {
                throw new ArgumentException("Path is invalid");
            }
        }
    }
}
