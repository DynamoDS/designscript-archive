using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    public class SurfaceCurvature : IDisplayable
    {
        #region DATA MEMBERS
        private CoordinateSystem mCoordinateSystem;
        private Point mPointOnSurface;
        private Surface mContextSurface;
        #endregion

        #region PROPERTIES
        public CoordinateSystem CoordinateSystem
        {
            get { return mCoordinateSystem; }
        }

        public Point PointOnSurface
        {
            get { return mPointOnSurface; }
        }

        public Surface ContextSurface
        {
            get { return mContextSurface; }
            private set { value.AssignTo(ref mContextSurface); }
        }

        public Vector FirstPrincipleCurvature { get; private set; }
        public Vector SecondPrincipleCurvature { get; private set; }
        public double GaussianCurvature { get; private set; }
        public double U { get; private set; }
        public double V { get; private set; }
        #endregion

        private SurfaceCurvature(Surface contextSurface, double u, double v, ICoordinateSystemEntity coordinateSystemEntity)
        {
            FirstPrincipleCurvature = new Vector(coordinateSystemEntity.XAxis);
            SecondPrincipleCurvature = new Vector(coordinateSystemEntity.YAxis);
            GaussianCurvature = FirstPrincipleCurvature.Length * SecondPrincipleCurvature.Length;
            mPointOnSurface = Point.ToGeometry(coordinateSystemEntity.Origin, false, contextSurface) as Point;
            U = u;
            V = v;
            ContextSurface = contextSurface;
            mCoordinateSystem = CoordinateSystem.ToCS(coordinateSystemEntity, false);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="contextSurface"></param>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static SurfaceCurvature BySurfaceParameters(Surface contextSurface, double u, double v)
        {
            if (contextSurface == null)
                return null;

            ISurfaceEntity host = contextSurface.GetSurfaceEntity();
            if (host == null)
                return null;

            ICoordinateSystemEntity coordinateSystemEntity = host.CurvatureAtParameter(u, v);
            if (null != coordinateSystemEntity)
            {
                SurfaceCurvature surfCurv = new SurfaceCurvature(contextSurface, u, v, coordinateSystemEntity);
                return surfCurv;
            }

            return null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                GeometryExtension.DisposeObject(ref mCoordinateSystem);
                GeometryExtension.DisposeObject(ref mPointOnSurface);
                GeometryExtension.DisposeObject(ref mContextSurface);
            }
        }

        #region FROM_IDISPLAYATTRIBUTES

        public bool Highlight(bool visibility)
        {
            return CoordinateSystem.Highlight(visibility);
        }
        private SurfaceCurvature SetVisibilityCore(bool visible)
        {
            CoordinateSystem.SetVisibility(visible);
            this.Visible = visible;
            return this;
        }
        public SurfaceCurvature SetVisibility(bool visible)
        {
            return SetVisibilityCore(visible);
        }
        IDisplayable IDisplayable.SetVisibility(bool visible)
        {
            return SetVisibilityCore(visible);
        }
        IDisplayable IDisplayable.SetColor(IColor color)
        {
            throw new NotImplementedException();
        }
        IColor IDisplayable.Color
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public bool Visible
        {
            get
            {
                return CoordinateSystem.Visible;
            }
            set
            {
                CoordinateSystem.Visible = value;
            }
        }

        #endregion
    }
}
