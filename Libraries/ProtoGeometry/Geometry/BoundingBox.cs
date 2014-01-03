using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>
    /// A type representing a pair of parameters on a surface
    /// </summary>
    public class BoundingBox
    {

        public BoundingBox Intersection(BoundingBox box)
        {
            return new BoundingBox(BoundingBoxEntity.Intersection(box.BoundingBoxEntity));
        }

        public bool Intersects(BoundingBox box)
        {
            return BoundingBoxEntity.Intersects(box.BoundingBoxEntity);
        }

        #region Internal properties

        internal IBoundingBoxEntity BoundingBoxEntity
        {
            get;
            private set;
        }

        #endregion

        #region Internal constructors

        internal BoundingBox(IBoundingBoxEntity boundingBox)
        {
            this.BoundingBoxEntity = boundingBox;
        }

        #endregion

        #region Public properties

        public Point Min
        {
            get
            {
                return BoundingBoxEntity.MinPoint.ToPoint(true, null);
            }
        }

        public Point Max
        {
            get
            {
                return BoundingBoxEntity.MaxPoint.ToPoint(true, null);
            }
        }

        #endregion

        #region Public static constructors

        public static BoundingBox ByMinMax(Point u, Point v)
        {
            return BoundingBox.ByGeometry(new Geometry[]
            {
                u, v
            });
        }

        public static BoundingBox ByGeometry( Geometry geom )
        {
            var bbox = HostFactory.Factory.BoundingBoxByGeometry(geom.GeomEntity);
            return new BoundingBox(bbox);
        }

        public static BoundingBox ByGeometry( Geometry[] geoms )
        {
            var bbox = HostFactory.Factory.BoundingBoxByGeometries(geoms.Select(x => x.GeomEntity).ToArray());
            return new BoundingBox(bbox);
        }

        public static BoundingBox ByGeometryAndCoordinateSystem( Geometry geom, CoordinateSystem orientation )
        {
            var bbox = HostFactory.Factory.BoundingBoxByGeometry(geom.GeomEntity, orientation.CSEntity);
            return new BoundingBox(bbox);
        }

        public static BoundingBox ByGeometryAndCoordinateSystem( Geometry[] geoms, CoordinateSystem orientation )
        {
            var bbox = HostFactory.Factory.BoundingBoxByGeometries(geoms.Select(x => x.GeomEntity).ToArray(), orientation.CSEntity );
            return new BoundingBox(bbox);
        }

        #endregion

    }
}
