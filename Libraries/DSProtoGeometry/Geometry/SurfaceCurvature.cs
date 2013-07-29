using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    public class DSSurfaceCurvature : IDisplayable
    {
        #region DATA MEMBERS
        private DSCoordinateSystem mCoordinateSystem;
        private DSPoint mPointOnSurface;
        private DSSurface mContextSurface;
        #endregion

        #region PROPERTIES
        public DSCoordinateSystem CoordinateSystem
        {
            get { return mCoordinateSystem; }
        }

        public DSPoint PointOnSurface
        {
            get { return mPointOnSurface; }
        }

        public DSSurface ContextSurface
        {
            get { return mContextSurface; }
            private set { value.AssignTo(ref mContextSurface); }
        }

        public DSVector FirstPrincipleCurvature { get; private set; }
        public DSVector SecondPrincipleCurvature { get; private set; }
        public double GaussianCurvature { get; private set; }
        public double U { get; private set; }
        public double V { get; private set; }
        #endregion

        private DSSurfaceCurvature(DSSurface contextSurface, double u, double v, ICoordinateSystemEntity coordinateSystemEntity)
        {
            FirstPrincipleCurvature = new DSVector(coordinateSystemEntity.XAxis);
            SecondPrincipleCurvature = new DSVector(coordinateSystemEntity.YAxis);
            GaussianCurvature = FirstPrincipleCurvature.Length * SecondPrincipleCurvature.Length;
            mPointOnSurface = DSPoint.ToGeometry(coordinateSystemEntity.Origin, false, contextSurface) as DSPoint;
            U = u;
            V = v;
            ContextSurface = contextSurface;
            mCoordinateSystem = DSCoordinateSystem.ToCS(coordinateSystemEntity, false);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="contextSurface"></param>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static DSSurfaceCurvature BySurfaceParameters(DSSurface contextSurface, double u, double v)
        {
            if (contextSurface == null)
                return null;

            ISurfaceEntity host = contextSurface.GetSurfaceEntity();
            if (host == null)
                return null;

            ICoordinateSystemEntity coordinateSystemEntity = host.CurvatureAtParameter(u, v);
            if (null != coordinateSystemEntity)
            {
                DSSurfaceCurvature surfCurv = new DSSurfaceCurvature(contextSurface, u, v, coordinateSystemEntity);
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
                DSGeometryExtension.DisposeObject(ref mCoordinateSystem);
                DSGeometryExtension.DisposeObject(ref mPointOnSurface);
                DSGeometryExtension.DisposeObject(ref mContextSurface);
            }
        }

        #region FROM_IDISPLAYATTRIBUTES

        public bool Highlight(bool visibility)
        {
            return CoordinateSystem.Highlight(visibility);
        }
        private DSSurfaceCurvature SetVisibilityCore(bool visible)
        {
            CoordinateSystem.SetVisibility(visible);
            this.Visible = visible;
            return this;
        }
        public DSSurfaceCurvature SetVisibility(bool visible)
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
