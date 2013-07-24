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
        void PushPointVertex(double x, double y, double z);
        void PushPointVertexColor(byte red, byte green, byte blue, byte alpha);

        void PushTriangleVertex(double x, double y, double z);
        void PushTriangleVertexNormal(double x, double y, double z);
        void PushTriangleVertexColor(byte red, byte green, byte blue, byte alpha);

        void PushLineStripVertex(double x, double y, double z);
        void PushLineStripVertexCount(int n);
        void PushLineStripVertexColor(byte red, byte green, byte blue, byte alpha);

        /// <summary>
        /// Returns pointer to abstract C++ class RenderPackage, which is 
        /// actually the implementation class for this interface.
        /// </summary>
        IntPtr NativeRenderPackage { get; }
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
