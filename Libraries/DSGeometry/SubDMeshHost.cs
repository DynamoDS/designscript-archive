using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;

namespace DSGeometry
{
    class SubDMeshEntity : GeometryEntity, ISubDMeshEntity
    {
        public double ComputeSurfaceArea()
        {
            return 100;
        }

        public double ComputeVolume()
        {
            return 400;
        }

        public ISolidEntity ConvertToSolid(bool bConvertAsSmooth)
        {
            return new SolidEntity();
        }

        public ISurfaceEntity ConvertToSurface(bool bConvertAsSmooth)
        {
            return new SurfaceEntity();
        }

        public ILineEntity[] GetEdges()
        {
            return new ILineEntity[2] { new LineEntity(), new LineEntity() };
        }

        public int[][] GetFaceIndices()
        {
            return new int[][] { new int[] { 1, 2, 3 }, new int[] { 3, 2, 1 } };
        }

        public bool GetIsClosed()
        {
            return false;
        }

        public int GetNumFaces()
        {
            return 4;
        }

        public int GetNumResultFaces()
        {
            return 2;
        }

        public int GetNumResultVertices()
        {
            return 6;
        }

        public int GetNumVertices()
        {
            return 8;
        }

        public int[][] GetResultFaceIndices()
        {
            return new int[][] { new int[] { 3, 2, 1 }, new int[] { 1, 2, 3 } };
        }

        public IPointEntity[] GetResultVertices()
        {
            return new IPointEntity[3] { new PointEntity(), new PointEntity(1,1,1), new PointEntity(2,2,2) };
        }

        public IColor[] GetVertexColors()
        {
            return new IColor[2] { Color.Black.ToIColor(), Color.Blue.ToIColor() };
        }

        public IVector[] GetVertexNormals()
        {
            return new DsVector[2] { DsVector.ByCoordinates(0, 0, 1), DsVector.ByCoordinates(1, 0, 0) };
        }

        public IPointEntity[] GetVertices()
        {
            return new IPointEntity[3] { new PointEntity(), new PointEntity(0,7,6), new PointEntity(3,2,1) };
        }

        public bool UpdateByVerticesFaceIndices(IPointEntity[] vertices, int[][] faceIndices, int subDLevel)
        {
            return false;
        }

        public bool UpdateSubDMeshColors(IColor[] colors)
        {
            return false;
        }

        public bool UpdateSubDMeshColors(IPointEntity[] vertices, IVector[] normals, IColor[] colors, int[][] faceIndices, int subDLevel)
        {
            return false;
        }

        public bool UpdateSubDMeshNormals(IVector[] normals)
        {
            return false;
        }
    }
}
