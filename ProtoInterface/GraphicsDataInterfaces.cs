using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Autodesk.DesignScript.Interfaces
{
    /// <summary>
    /// This interface caches render specific data.
    /// </summary>
    public interface IRenderPackage
    {
        List<double> PointVertices { get; }
        List<byte> PointVertexColors { get; }

        List<double> LineStripVertices { get; }
        List<byte> LineStripVertexColors { get; }
        List<int> LineStripVertexCounts { get; }

        List<double> TriangleVertices { get; }
        List<byte> TriangleVertexColors { get; }
        List<double> TriangleNormals { get; }
    }

    /// <summary>
    /// Represents a graphics item object, that can provide tesselated data
    /// into the given render package.
    /// </summary>
    public interface IGraphicItem
    {
        /// <summary>
        /// Gets the graphics/tesselation data in given render package object.
        /// </summary>
        /// <param name="package">The render package, where graphics data to be
        /// pushed.</param>
        void Tessellate(IRenderPackage package);
    }

    /// <summary>
    /// This interface provides graphics data into the RenderPackage interface 
    /// for given set of objects.
    /// </summary>
    public interface IGraphicDataProvider
    {
        /// <summary>
        /// Returns a list of IGraphicItem owned by the given object.
        /// </summary>
        /// <param name="obj">The object for which graphics items are queried.
        /// </param>
        /// <returns>List of IGraphicItem owned by the input object.</returns>
        List<IGraphicItem> GetGraphicItems(Object obj);

        /// <summary>
        /// Gets the Graphics/Render data into the given render package.
        /// </summary>
        /// <param name="objects">Objects which owns some graphics items</param>
        /// <param name="package">RenderPackage where graphics/render data can
        /// be pushed/set.</param>
        void Tessellate(List<Object> objects, IRenderPackage package);
    }
}
