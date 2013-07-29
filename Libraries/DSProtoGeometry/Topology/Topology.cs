using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    /// <summary>
    /// Represents the base topology class.
    /// </summary>
    public abstract class DSTopology : DesignScriptEntity
    {
        #region PROTECTED_MEMBERS

        internal DSTopology(ITopologyEntity host) : base(host) { }

        protected abstract DSGeometry GetGeometryCore(out bool autodispose);

        private DSGeometry mGeometry;
        private bool mAutoDispose;

        protected override void DisposeDisplayable()
        {
            if (null != mGeometry && mAutoDispose)
            {
                mGeometry.Dispose();
                mGeometry = null;
            }
        }

        protected DSGeometry Geometry
        {
            get
            {
                if (null == mGeometry)
                {
                    mGeometry = GetGeometryCore(out mAutoDispose);
                }
                return mGeometry;
            }
        }

        internal override bool TessellateCore(IRenderPackage package)
        {
            if(!base.TessellateCore(package))
                this.Geometry.Tessellate(package);
            return true;
        }

        #endregion

        #region EXTRACT_GEOMETRY

        /// <summary>
        /// Extracts the underlying geometry from the topology and makes it 
        /// persistent.
        /// </summary>
        /// <param name="color">Color to be assigned to extracted geometry.</param>
        /// <returns>Geometry</returns>
        public DSGeometry _ExtractGeometry(DSColor color)
        {
            DSGeometry geom = this.Geometry;
            if (null != geom)
            {
                mAutoDispose = false; //someone else is taking control
                geom.Persist();
                if (null != color)
                    geom.Color = color;
            }

            return geom;
        }

        /// <summary>
        /// Extracts the underlying geometry from the topology and makes it 
        /// persistent. The extracted geometry will inherit color from this
        /// topology object, if there was some color applied to it.
        /// </summary>
        /// <returns>Geometry</returns>
        public DSGeometry _ExtractGeometry()
        {
            return _ExtractGeometry(null);
        }

        #endregion

        #region DISPLAYATTRIBUTE_METHODS

        /// <summary>
        /// Highlights the underlying geometry.
        /// </summary>
        /// <param name="visibility">flag to highlight/unhighlight</param>
        /// <returns>true for success</returns>
        public bool Highlight(bool visibility)
        {
            DSGeometry geom = Geometry;
            if (null != geom)
            {
                geom.Highlight(visibility);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets color by RGB value to the underlying geometry of this topology.
        /// </summary>
        /// <param name="redValue">Red value for the color</param>
        /// <param name="greenValue">Green value for the color</param>
        /// <param name="blueValue">Blue value for the color</param>
        /// <returns>This topology after color is applied</returns>
        public DSTopology SetColor(byte redVal, byte greenVal, byte blueVal)
        {
            return SetColor(DSColor.ByARGB(255, redVal, greenVal, blueVal));
        }

       
        /// <summary>
        /// Returns if the Geometry of the topology is visible
        /// </summary>
        public bool Visible
        {
            get 
            {
                if (null != mGeometry)
                    return mGeometry.Visible;

                return false;
            }
            set
            {
                DSGeometry geom = Geometry;
                if (null != geom)
                    geom.SetVisibility(value);
            }
        }
        public DSColor Color
        {
            get
            {
                if (null != mGeometry)
                    return mGeometry.Color;

                return null;
            }
            set
            {
                DSGeometry geom = Geometry;
                if (null != geom)
                    geom.SetColor(value);
            }
        }
        /// <summary>
        /// Sets color to entity/geometry
        /// </summary>
        /// <param name="color">Color value</param>
        public DSTopology SetColor(DSColor color)
        {
            return SetColorCore(color);
        }

        /// <summary>
        /// Sets the visibility of underlying geometry.
        /// </summary>
        /// <param name="visible">flag to set the visibility</param>
        public DSTopology SetVisibility(bool visible)
        {
            return SetVisibilityCore(visible);
        }

        DSTopology SetColorCore(DSColor color)
        {
            this.Color = color;
            return this;
        }

        DSTopology SetVisibilityCore(bool visible)
        {
            this.Visible = visible;
            return this;
        }
        #endregion
    }
}
