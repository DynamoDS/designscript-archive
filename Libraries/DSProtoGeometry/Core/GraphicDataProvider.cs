using System.Collections.Generic;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

[assembly: GraphicDataProvider(typeof(Autodesk.DesignScript.Geometry.GraphicDataProvider))]
namespace Autodesk.DesignScript.Geometry
{
    class GraphicDataProvider : IGraphicDataProvider
    {
        public List<IGraphicItem> GetGraphicItems(object obj)
        {
            DesignScriptEntity dsEntity = obj as DesignScriptEntity;
            if (null == dsEntity)
                return null;

            IGraphicItem graphic = new GeometryGraphicItem(dsEntity);
            List<IGraphicItem> items = new List<IGraphicItem>();
            items.Add(graphic);
            return items;
        }

        public void Tessellate(List<object> objects, IRenderPackage package)
        {
            foreach (var item in objects)
            {
                List<IGraphicItem> graphicItems = GetGraphicItems(item);
                if (null == graphicItems || graphicItems.Count == 0)
                    continue;

                foreach (var g in graphicItems)
                {
                    g.Tessellate(package);
                }
            }
        }
    }

    class GeometryGraphicItem : IGraphicItem
    {
        private DesignScriptEntity mEntity;
        public GeometryGraphicItem(DesignScriptEntity entity)
        {
            mEntity = entity;
        }

        public void Tessellate(IRenderPackage package)
        {
            mEntity.Tessellate(package);
        }
    }
}
