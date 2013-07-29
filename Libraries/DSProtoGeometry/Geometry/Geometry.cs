#define DEFAULTPERSIST
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;

namespace Autodesk.DesignScript.Geometry
{
    [Browsable(false)]
    public abstract class DesignScriptEntity : IDisposable
    {
        #region DATA MEMBERS
        private IDesignScriptEntity mEntity;
        protected Func<IDesignScriptEntity> mConstructor;
        private int mRefCount = 1;
        #endregion

        #region INTERNAL_METHODS
        internal DesignScriptEntity(Func<IDesignScriptEntity> constructor)
        {
            mConstructor = constructor;
        }

        internal DesignScriptEntity(IDesignScriptEntity entity)
        {
            InitEntity(entity);
        }

        internal void InitEntity(IDesignScriptEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException("host", "DesignScriptEntity can't be created with null IDesignScriptEntity");

            if (entity.Owner != null)
                throw new ArgumentException("Input host already has owner assigned to it. DesignScriptEntity can't be created with it.", "host");

            mEntity = entity;
            mEntity.SetOwner(this);
        }

        internal void Tessellate(IRenderPackage package)
        {
            IGraphicItem graphics = mEntity as IGraphicItem;
            if (null != graphics)
                graphics.Tessellate(package);
            else
                this.TessellateCore(package);
        }

        internal virtual bool TessellateCore(IRenderPackage package)
        {
            //Does nothing
            return false;
        }

        #endregion

        #region IDISPOSABLE

        public void Dispose()
        {
            //Erase the displayable objects only while script is executing
            //When script has stopped executing, the displayable objects must
            //be left as output of execution.
            if(DSApplication.Instance.IsExecuting)
                DisposeDisplayable();
            ReleaseEntity();
        }

        /// <summary>
        /// This method is called when the displayable is no more needed.
        /// </summary>
        virtual protected void DisposeDisplayable()
        {
        }

        ~DesignScriptEntity()
        {
            Dispose(false);
        }

        /// <summary>
        /// This method should be called whenever this object is referenced by other object
        /// </summary>
        internal void RetainEntity()
        {
            //System.Diagnostics.Trace.WriteLine(string.Format("****** RETAIN : {0} *********", mRefCount));
            //System.Diagnostics.Trace.Write(Environment.StackTrace);
            ++mRefCount;
        }

        internal void ReleaseEntity()
        {
            bool dispose = (mRefCount == 1);
            --mRefCount;
            if(dispose)
                Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            //System.Diagnostics.Trace.WriteLine("****** Dispose *********");
            //System.Diagnostics.Trace.Write(Environment.StackTrace);
            //System.Diagnostics.Trace.Assert(mEntity != null);
            //System.Diagnostics.Trace.Write(string.Format("{0} is being disposed with disposing = {1} and refcount = {2}", this.ToString(), disposing, mRefCount));

            if (disposing)
            {
                if (mRefCount <= 0)
                {
                    mRefCount = 0;
                    if (mEntity != null)
                    {
                        mEntity.Dispose();
                        mEntity = null;
                        GC.SuppressFinalize(this);
                    }
                }
            }
            else
            {
                mEntity = null;
                mConstructor = null;
            }
        }

        internal bool IsDisposed
        {
            get { return null == mEntity || mRefCount <= 0; }
        }

        #endregion

        #region OBJECT OVERRIDES

        public override string ToString()
        {
            return string.Format("{0}()", GetType().Name);
        }

        private int? mHashCode;
        public override sealed int GetHashCode() 
        {
            if (!mHashCode.HasValue)
                mHashCode = ComputeHashCode();

            return mHashCode.Value;
        }

        protected virtual int ComputeHashCode()
        {
            int hashcode = (null != mEntity) ? mEntity.GetHashCode() : 0;
            if (hashcode != 0)
                return hashcode;

            return base.GetHashCode();
        }

        public override sealed bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
                return true;

            DesignScriptEntity dsentity = obj as DesignScriptEntity;
            if (null == dsentity)
                return false;

            return Equals(dsentity);
        }

        protected virtual bool Equals(DesignScriptEntity dsentity)
        {
            if (null != mEntity && object.ReferenceEquals(mEntity, dsentity.mEntity))
                return true;

            return this.GetHashCode() == dsentity.GetHashCode();
        }

        #endregion

        #region PROPERTIES
        internal virtual IDesignScriptEntity HostImpl
        {
            get
            {
                if (null == mEntity && null != mConstructor)
                    InitEntity(mConstructor());

                return mEntity;
            }
        }
        #endregion
    }

    public abstract class DSGeometry : DesignScriptEntity
    {
        #region DATA MEMBERS
        /// <summary>
        /// A map between IGeometryEntity types and Geometry constructors using host.
        /// </summary>
        private static Dictionary<Type, Func<IGeometryEntity, bool, DSGeometry>> mGeometryContructors;

        private DSCoordinateSystem mContextCS = null;
        private DSGeometry mContext = null;

        internal IPersistentObject mPersistent = null;
        private IDisplayable mDisplayAttributes = null;
        private DSColor mColor = null;
        private bool visibilityForHighlight = false;
        #endregion

        #region TYPE_INITIALIZATION_METHODS

        static DSGeometry()
        {
            mGeometryContructors = new Dictionary<Type, Func<IGeometryEntity, bool, DSGeometry> >();
            Type[] types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (Type t in types)
            {
                if (t.IsClass && !t.IsAbstract && typeof(DSGeometry).IsAssignableFrom(t))
                {
                    MethodInfo m = t.GetMethod("InitType", BindingFlags.NonPublic | BindingFlags.Static);
                    System.Diagnostics.Debug.Assert(null != m, string.Format("static void InitType() is not implemented for {0}", t.FullName));
                    if(null != m)
                        m.Invoke(null, null); 
                }
            } 
        }

        /// <summary>
        /// Geometry Type registration mechanism. 
        /// </summary>
        /// <param name="hostType">Type of IGeometryEntity derived interfaces.</param>
        /// <param name="contructor">A delegate to construct Geometry.</param>
        internal static void RegisterHostType(Type hostType, Func<IGeometryEntity, bool, DSGeometry> contructor)
        {
            mGeometryContructors[hostType] = contructor;
        }

        internal static DSGeometry ToGeometry(IGeometryEntity host, bool persist = false, DSGeometry context = null)
        {
            if (host == null)
                return null;

            if (host.Owner != null)
                return host.Owner as DSGeometry;

            Func<IGeometryEntity, bool, DSGeometry> constructor = GetGeomConstructor(host);
            if (null == constructor)
                throw new InvalidOperationException(string.Format("Can't locate DSGeometry constructor for type: {0}.", host.GetType()));

            DSGeometry geom = constructor(host, persist);
            if (null != context)
                geom.Context = context;

            return geom;
        }

        private static Func<IGeometryEntity, bool, DSGeometry> GetGeomConstructor(IGeometryEntity host)
        {
            Type type = host.GetType();
            Type[] interfaces = type.GetInterfaces();
            for (int i = interfaces.Length - 1; i >= 0; --i)
            {
                Func<IGeometryEntity, bool, DSGeometry> constructor;
                if (mGeometryContructors.TryGetValue(interfaces[i], out constructor))
                    return constructor;
            }

            return null;
        }

        #endregion

        #region INTERNAL_METHODS
        internal DSGeometry(IGeometryEntity host, bool persist)
            : base(host)
        {
            InitGeometry(persist);
        }

        internal DSGeometry(Func<IGeometryEntity> constructor, bool persist)
            : base(constructor)
        {
            InitGeometry(persist);
        }

        private void InitGeometry(bool persist)
        {
            mDisplayAttributes = null;
#if DEFAULTPERSIST
            if (persist)
                Persist();
#else
            if (persist)
                SetVisibility(true);
#endif
            if(null != DSGeometrySettings.DefaultColor)
                SetColor(DSGeometrySettings.DefaultColor);
        }

        internal IPersistentObject Persist()
        {
            if (null != mPersistent)
                return mPersistent;

            //In the absense of persistent manager, bail out.
            if (null == HostFactory.PersistenceManager)
                return null;

            IGeometryEntity geometry = HostImpl as IGeometryEntity;
            if (geometry == null)
                return null;

            DisposeDisplay();
            mPersistent = HostFactory.PersistenceManager.Persist(geometry);
            return mPersistent;
        }

        protected override void DisposeDisplayable()
        {
            DisposeDisplay();

            if (null != mPersistent)
                mPersistent.Erase();

            base.DisposeDisplayable();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (null != mPersistent && !Object.ReferenceEquals(mPersistent, base.HostImpl))
                    mPersistent.Dispose();

                DSGeometryExtension.DisposeObject(ref mContext);
                DSGeometryExtension.DisposeObject(ref mContextCS);
            }

            mPersistent = null;
            mDisplayAttributes = null;
            base.Dispose(disposing); //allow base class to release it's resources
        }

        internal override bool TessellateCore(IRenderPackage package)
        {
            IGraphicItem graphics = mPersistent as IGraphicItem;
            if (null != graphics)
            {
                graphics.Tessellate(package);
                return true;
            }

            return false;
        }

        private void DisposeDisplay()
        {
            if (null != mDisplayAttributes)
            {
                if (!Object.ReferenceEquals(mDisplayAttributes, mPersistent) &&
                    !Object.ReferenceEquals(mDisplayAttributes, base.HostImpl))
                    mDisplayAttributes.DisposeObject();
            }
            mDisplayAttributes = null;
        }

        internal static int GetIndexOfNearestGeometry(IGeometryEntity[] entities, IPointEntity point)
        {
            double minDistance = entities[0].DistanceTo(point);
            int minIndex = 0;
            for (int i = 1; i < entities.Length; ++i)
            {
                double currentDistance = entities[i].DistanceTo(point);
                if (currentDistance < minDistance)
                {
                    minDistance = currentDistance;
                    minIndex = i;
                }
            }
            return minIndex;
        }

        internal virtual IGeometryEntity[] IntersectWithCurve(DSCurve curve)
        {
            throw new System.InvalidOperationException(string.Format(Properties.Resources.InvalidIntersect, GetType().Name, "DSCurve"));
        }

        internal virtual IGeometryEntity[] IntersectWithPlane(DSPlane plane)
        {
            throw new System.InvalidOperationException(string.Format(Properties.Resources.InvalidIntersect, GetType().Name, "DSPlane"));
        }

        internal virtual IGeometryEntity[] IntersectWithSurface(DSSurface surf)
        {
            throw new System.InvalidOperationException(string.Format(Properties.Resources.InvalidIntersect, GetType().Name, "DSSurface"));
        }

        internal IGeometryEntity[] IntersectWithPoint(DSPoint point)
        {
            if (GeomEntity.DistanceTo(point.PointEntity).EqualsTo(0.0))
                return new IGeometryEntity[] { point.PointEntity };

            return null;
        }

        internal virtual IGeometryEntity[] IntersectWithSolid(DSSolid solid)
        {
            throw new System.InvalidOperationException(string.Format(Properties.Resources.InvalidIntersect, GetType().Name, "DSSolid"));
        }

        internal virtual IGeometryEntity[] ProjectOn(DSGeometry other, DSVector direction)
        {
            throw new InvalidOperationException(string.Format(Properties.Resources.InvalidProjection, GetType().Name, other.GetType().Name));
        }

        internal virtual IPointEntity ClosestPointTo(IPointEntity otherPoint)
        {
            try
            {
                return GeomEntity.GetClosestPoint(otherPoint);
            }
            catch (System.NotImplementedException)
            {
                throw new System.InvalidOperationException(string.Format(Properties.Resources.NotSupported, "ClosestPointTo", GetType().Name));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        protected virtual DSGeometry Translate(DSVector offset)
        {
            IGeometryEntity clone = GeomEntity.CopyAndTranslate(offset.IVector);
            if (null == clone)
                throw new InvalidOperationException("Failed to clone and translate geometry.");

            DSGeometry geom = ToGeometry(clone, true);
            return geom;
        }

        internal virtual DSGeometry TransformBy(ICoordinateSystemEntity csEntity)
        {
            IGeometryEntity clone = GeomEntity.CopyAndTransform(DSCoordinateSystem.WCS.CSEntity, csEntity);
            return ToGeometry(clone, true);
        }

        #endregion

        #region PUBLIC_METHODS

        /// <summary>
        /// Translates any given geometry by the given displacements in the x, y,
        /// and z directions defined in WCS respectively. 
        /// </summary>
        /// <param name="xTranslation">Displacement along X-axis.</param>
        /// <param name="yTranslation">Displacement along Y-axis.</param>
        /// <param name="zTranslation">Displacement along Z-axis.</param>
        /// <returns>Transformed Geometry.</returns>
        public DSGeometry Translate(double xTranslation, double yTranslation, double zTranslation)
        {
            DSVector direction = new DSVector(xTranslation, yTranslation, zTranslation);
            return Translate(direction, direction.GetLength());
        }

        /// <summary>
        /// Translates any geometry type by the given distance in the given 
        /// direction.
        /// </summary>
        /// <param name="direction">Displacement direction.</param>
        /// <param name="distance">Displacement distance along given direction.</param>
        /// <returns>Transformed Geometry.</returns>
        public DSGeometry Translate(DSVector direction, double distance)
        {
            DSVector offset = direction.Normalize().MultiplyBy(distance);
            DSGeometry geom = Translate(offset);
            SetDisplayPropertiesTo(geom.Display);
            return geom;
        }

        /// <summary>
        /// Transforms this geometry from source CoordinateSystem to a new 
        /// context CoordinateSystem.
        /// </summary>
        /// <param name="fromCoordinateSystem"></param>
        /// <param name="contextCoordinateSystem"></param>
        /// <returns>Transformed Geometry.</returns>
        public DSGeometry Transform(DSCoordinateSystem fromCoordinateSystem, DSCoordinateSystem contextCoordinateSystem)
        {
            ICoordinateSystemEntity csEntity = contextCoordinateSystem.CSEntity.PostMultiplyBy(fromCoordinateSystem.CSEntity.Inverse());
            DSGeometry geom = TransformBy(csEntity);
            if (null == geom)
                throw new InvalidOperationException(string.Format(Properties.Resources.OperationFailed, "DSGeometry.Transform"));

            geom.ContextCoordinateSystem = contextCoordinateSystem;
            SetDisplayPropertiesTo(geom.Display);
            return geom;
        }


        /// <summary><para>
        /// Generic Intersect method to intersect any two geometry sub-type 
        /// entities and returns an array of intersection results. </para>
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        [Autodesk.DesignScript.Runtime.AllowRankReduction]
        public DSGeometry[] Intersect(DSGeometry other)
        {
            if (null == other)
                throw new ArgumentNullException("other");

            IGeometryEntity[] geoms = null;
            DSPlane plane = other as DSPlane;
            if (plane != null)
                geoms = IntersectWithPlane(plane);
            else
            {
                DSSurface surf = other as DSSurface;
                if (surf != null)
                    geoms = IntersectWithSurface(surf);
                else
                {
                    DSSolid solid = other as DSSolid;
                    if (solid != null)
                        geoms = IntersectWithSolid(solid);
                    else
                    {
                        DSCurve curve = other as DSCurve;
                        if (curve != null)
                            geoms = IntersectWithCurve(curve);
                        else
                        {
                            DSPoint point = other as DSPoint;
                            if (null != point)
                                geoms = IntersectWithPoint(point);
                            else
                                throw new System.InvalidOperationException(string.Format(Properties.Resources.InvalidIntersect, GetType().Name, other.GetType().Name));
                        }
                    }
                }
            }

            return geoms.ToArray<DSGeometry, IGeometryEntity>(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public DSGeometry[] Project(DSGeometry other, DSVector direction)
        {
            if (null == other)
                throw new ArgumentNullException("other");

            IGeometryEntity[] geoms = null;
            geoms = other.ProjectOn(this, direction);

            return geoms.ToArray<DSGeometry, IGeometryEntity>(true);
        }

        /// <summary>
        /// Returns the closest point on the Geometry object to another point
        /// </summary>
        /// <param name="otherPoint">The other point for classification</param>
        /// <returns>point on this geometry</returns>
        public DSPoint ClosestPointTo(DSPoint otherPoint)
        {
            if (null == otherPoint)
                throw new System.ArgumentNullException("otherPoint");

            IPointEntity entity = ClosestPointTo(otherPoint.PointEntity);
            if (null == entity)
                throw new System.InvalidOperationException(string.Format(Properties.Resources.OperationFailed, "ClosestPointTo"));

            return entity.ToPoint(true, this);
        }

        #endregion

        #region SAT_IMPORT_EXPORT

        [AllowRankReduction]
        public static DSGeometry[] ImportFromSAT(string filePath)
        {
            IGeometryEntity[] objects = ImportFromSAT(ref filePath);
            return objects.ToArray<DSGeometry, IGeometryEntity>();
        }

        internal static IGeometryEntity[] ImportFromSAT(ref string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new System.ArgumentNullException("fileName");
            fileName = DSGeometryExtension.LocateFile(fileName);
            if (!File.Exists(fileName))
                throw new System.ArgumentException(string.Format(Properties.Resources.FileNotFound, fileName), "fileName");
            IGeometryEntity[] objects = HostFactory.Factory.LoadSat(fileName);
            return objects;
        }

        public bool ExportToSAT(string filePath)
        {
            List<DSGeometry> geometries = new List<DSGeometry>();
            geometries.Add(this);
            return ExportToSAT(geometries.ToArray(), filePath);
        }

        public static bool ExportToSAT(DSGeometry[] geometry, string filePath)
        {
            List<IGeometryEntity> hosts = new List<IGeometryEntity>();
            foreach (DSGeometry geom in geometry)
            {   
                IGeometryEntity geomEntity = geom.GeomEntity as IGeometryEntity;
                if (null != geomEntity)
                    hosts.Add(geomEntity);
            }

            if (hosts.Count == 0)
                return false;

            if (!filePath.EndsWith(".sat"))
                filePath += ".sat";
            if (!Path.IsPathRooted(filePath))
            {
                string foldername = Path.GetDirectoryName(DSGeometrySettings.RootModulePath);
                filePath = Path.Combine(foldername, filePath);
            }
            return HostFactory.Factory.SaveSat(filePath, hosts.ToArray());
        }
        #endregion

        #region Update ACAD display
        public static void UpdateDisplay()
        {
            HostFactory.PersistenceManager.UpdateDisplay();
        }
        #endregion

        #region DISPLAY METHODS
        public bool Highlight(bool visibility)
        {
            if (null == Display)
                return false;
            if (!Display.Visible && visibility)
            {
                visibilityForHighlight = true;
                Display.Visible = true;
            }
            if (Display.Visible && !visibility && visibilityForHighlight)
            {
                visibilityForHighlight = false;
                Display.Visible = false;
            }
            return Display.Highlight(visibility);
        }

        private IDisplayable CreateDisplay()
        {
            return (null == mPersistent) ? null : mPersistent.Display;
        }

        /// <summary>
        /// Sets color by RGB value to this geometry.
        /// </summary>
        /// <param name="redValue">Red value for the color</param>
        /// <param name="greenValue">Green value for the color</param>
        /// <param name="blueValue">Blue value for the color</param>
        /// <returns>This geometry after color is applied</returns>
        public DSGeometry SetColor(byte redValue, byte greenValue, byte blueValue)
        {
            return SetColor(DSColor.ByARGB(255, redValue, greenValue, blueValue));
        }

        public DSGeometry SetColor(DSColor color)
        {
            mColor = color;
            if (null != Display)
                Display.SetColor(mColor.IColor);
            return this;
        }

        public DSGeometry SetVisibility(bool visible)
        {
            this.Visible = visible;
            return this;
        }

        internal void SetDisplayPropertiesTo(IDisplayable display)
        {
            if (this.mPersistent != null)
                display.SetVisibility(this.Visible);

            if (null != this.Color)
                display.SetColor(this.Color.IColor);
        }

        #endregion

        #region FROM_OBJECT

        protected override int ComputeHashCode()
        {
            int hashcode = (null != mPersistent) ? mPersistent.GetHashCode() : 0;
            if (hashcode != 0)
                return hashcode;

            return base.ComputeHashCode();
        }

        #endregion

        #region Utility Methods
        internal static string GetFullPath(string fileName)
        {
            if (Path.IsPathRooted(fileName))
                return fileName;

            var executionConfig = DSApplication.Instance.Session as IConfiguration;
            return Path.Combine(Path.GetDirectoryName(executionConfig.RootModulePath), fileName);
        }

        protected static void Hide(DSGeometry[] geometries)
        {
            if (null != geometries)
            {
                foreach (DSGeometry geom in geometries)
                    if (null != geom)
                        geom.Display.SetVisibility(false);
            }
        }
        protected static void Hide(DSGeometry geometry)
        {
            if (null != geometry)
                geometry.Display.SetVisibility(false);
        }
        #endregion

        #region PROPERTIES
        /// <summary>
        /// Get the context/reference coordinate system that was used to create this geometry.
        /// </summary>
        public DSCoordinateSystem ContextCoordinateSystem
        {
            get
            {
                if (null == mContextCS)
                    mContextCS = DSCoordinateSystem.Identity();
                return mContextCS;
            }
            protected set
            {
                value.AssignTo(ref mContextCS);
            }
        }

        /// <summary>
        /// Gets the context/reference geometry that was used to create this geometry.
        /// </summary>
        public DSGeometry Context
        {
            get { return mContext; }
            protected set { value.AssignTo(ref mContext); }
        }

        /// <summary>
        /// Sets visibility to entity/geometry
        /// </summary>      
        public virtual bool Visible
        {
            get
            {
                if (null != mDisplayAttributes)
                    return mDisplayAttributes.Visible;

                return mPersistent != null; //If persisted, its visible
            }
            set
            {
                if (null != Display)
                    Display.SetVisibility(value);
            }
        }

        /// <summary>
        /// Gets the color of this geometry
        /// </summary>
        public DSColor Color
        {
            get { return mColor; }
            set { SetColor(value); }
        }

        internal override IDesignScriptEntity HostImpl
        {
            get
            {
                if (mPersistent != null)
                    return mPersistent.Geometry;

                return base.HostImpl;
            }
        }

        internal IGeometryEntity GeomEntity { get { return HostImpl as IGeometryEntity; } }

        internal bool IsPersistent { get { return null != mPersistent; } }

        internal virtual IDisplayable Display
        {
            get
            {
                if (null == mDisplayAttributes)
                    mDisplayAttributes = CreateDisplay();

                return mDisplayAttributes;
            }
        }
        #endregion
    }

    class DoubleComparer : IComparer<double>
    {
        private bool mAscending;
        public DoubleComparer(bool ascending)
        {
            mAscending = ascending;
        }
        public int Compare(double x, double y)
        {
            if (!mAscending)
            {
                double temp = x;
                x = y;
                y = temp;
            }
            if (Math.Abs(x - y) < 0.0000001)
                return 0;
            if (x > y)
                return 1;
            else
                return -1;
        }
    }
}
