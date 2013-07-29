using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    public class DSCuboid : DSSolid
    {
        internal ICuboidEntity CuboidEntity { get { return HostImpl as ICuboidEntity; } }

        #region INTERNAL_METHODS

        private void InitializeGuaranteedProperties(ICuboidEntity entity = null)
        {
            ICuboidEntity cuboidEntity = entity;
            if (entity == null)
                cuboidEntity = CuboidEntity;
        }

        #endregion

        #region PRIVATE_CONSTRUCTORS

        static void InitType()
        {
            RegisterHostType(typeof(ICuboidEntity), (IGeometryEntity host, bool persist) => { return new DSCuboid(host as ICuboidEntity, persist); });
        }

        private DSCuboid(ICuboidEntity entity, bool persist = false)
            : base(entity, persist) 
        {
            InitializeGuaranteedProperties();
        }

        #endregion

        #region PROTECTED_CONSTRUCTORS

        protected DSCuboid(DSCoordinateSystem contextCoordinateSystem, double length, double width, double height,bool persist)
            : base(CuboidByLengthsCore(contextCoordinateSystem.CSEntity, length, width, height),persist)
        {
            InitializeGuaranteedProperties();
        }

        #endregion

        #region DESIGNSCRIPT_CONSTRUCTORS

        /// <summary>
        /// Constructs a cuboid on a coordinate system with length on X Axis, width on Y Axis and Height on Z Axis. The origin of coordinate system is the centroid of the cuboid.
        /// </summary>
        /// <param name="contextCoordinateSystem">The parent CoordinateSystem of the cuboid</param>
        /// <param name="length">The length of the cuboid</param>
        /// <param name="width">The width of the cuboid</param>
        /// <param name="height">The height of the cuboid</param>
        /// <returns></returns>
        public static DSCuboid ByLengths(DSCoordinateSystem contextCoordinateSystem, double length, double width, double height)
        {
            return new DSCuboid(contextCoordinateSystem, length, width, height, true);
        }

        #endregion

        #region CORE_METHODS

        private static ICuboidEntity CuboidByLengthsCore(ICoordinateSystemEntity contextCoordinateSystem, double length, double width, double height)
        {
            string kMethod = "DSCuboid.ByLengths";

            if (length.LessThanOrEqualTo(0.0))
                throw new System.ArgumentException(string.Format(Properties.Resources.LessThanZero, "length"), "length");
            if (height.LessThanOrEqualTo(0.0))
                throw new System.ArgumentException(string.Format(Properties.Resources.LessThanZero, "height"), "height");
            if (width.LessThanOrEqualTo(0.0))
                throw new System.ArgumentException(string.Format(Properties.Resources.LessThanZero, "width"), "width");
            if (null == contextCoordinateSystem)
                throw new System.ArgumentNullException("contextCoordinateSystem");
            if(!contextCoordinateSystem.IsScaledOrtho())
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "Shear DSCoordinateSystem", kMethod));

            ICuboidEntity entity = HostFactory.Factory.CuboidByLengths(contextCoordinateSystem, length, width, height);
            if (null == entity)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, kMethod));
            return entity;
        }

        #endregion

        #region PROPERTIES

        private double? length;
        /// <summary>
        /// 
        /// </summary>
        public double Length 
        {
            get
            {
                if (!length.HasValue)
                {
                    length = CuboidEntity.GetLength();
                }
                return length.Value;
            }
            private set
            {
                length = value;
            }
        }

        private double? width;
        /// <summary>
        /// 
        /// </summary>
        public double Width 
        {
            get
            {
                if (!width.HasValue)
                {
                    width = CuboidEntity.GetWidth();
                }
                return width.Value;
            }
            private set
            {
                width = value;
            }
        }

        private double? height;
        /// <summary>
        /// 
        /// </summary>
        public double Height 
        {
            get
            {
                if (!height.HasValue)
                {
                    height = CuboidEntity.GetHeight();
                }
                return height.Value;
            }
            private set
            {
                height = value;
            }
        }

        #endregion
    }
}
