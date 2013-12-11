using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Autodesk.DesignScript.Interfaces;
using System.ComponentModel;
using System.Configuration;

namespace Autodesk.DesignScript.Geometry
{
    [Browsable(false)]
    public class HostFactory
    {

        #region PRIVATE_METHODS

        static HostFactory mSelf;
        readonly IGeometryFactory mGeometryFactory;
        readonly IPersistenceManager mPersistenceManager;
        readonly List<IExtensionApplication> mExtensionApplications = new List<IExtensionApplication>();
        readonly List<Type> mExtensionApplicationTypes = new List<Type>();
        string mErrorString = string.Empty;

        private HostFactory()
        {
            try
            {
                string factoryFilePath = null;
                string persistentMgrFilePath = null;

                bool found = GetFactoryFileFromSessionConfig(ref factoryFilePath, ref persistentMgrFilePath);

                if(!found)
                    found = GetFactoryFileFromExeConfig(ref factoryFilePath, ref persistentMgrFilePath);

                // If not set by external applications, use the one in ProtoGeometry.config as a back-up.
                if (!found)
                    found = GetFactoryFileFromGeometryConfig(ref factoryFilePath, ref persistentMgrFilePath);

                if(found)
                {
                    factoryFilePath = GeometryExtension.LocateFile(factoryFilePath);
                    persistentMgrFilePath = GeometryExtension.LocateFile(persistentMgrFilePath);
                }

                string protoAcadGeometryPath = null;
                //If the configured factory file path is not correct or does not exist, use the default one
                if (string.IsNullOrEmpty(factoryFilePath) || !File.Exists(factoryFilePath))
                {
                    factoryFilePath = GeometryExtension.LocateFile("ProtoAcadGeometry.dll");
                    if (!File.Exists(factoryFilePath))
                    {
                        factoryFilePath = GeometryExtension.LocateFile("DSGeometry.dll"); //For nunit-testing
                        if (!File.Exists(factoryFilePath))
                            factoryFilePath = null;
                    }
                    else
                    {
                        protoAcadGeometryPath = factoryFilePath;
                    }
                }

                //If the configured persistent manager file path is not correct or does not exist, use "ProtoAcadGeometry.dll"
                if (string.IsNullOrEmpty(persistentMgrFilePath) || !File.Exists(persistentMgrFilePath))
                {
                    persistentMgrFilePath = protoAcadGeometryPath;
                }

                //Lastly create the factory and persistent manager instance
                if (null == mGeometryFactory && !string.IsNullOrEmpty(factoryFilePath))
                    mGeometryFactory = CreateInstance(typeof(IGeometryFactory), factoryFilePath) as IGeometryFactory;
                if (null == mPersistenceManager && !string.IsNullOrEmpty(persistentMgrFilePath))
                    mPersistenceManager = CreateInstance(typeof(IPersistenceManager), persistentMgrFilePath) as IPersistenceManager;

                //Initialize persistence manager with geometry factory
                if (null != mPersistenceManager && null != mGeometryFactory)
                    mPersistenceManager.GeometryFactory = mGeometryFactory;

                if (!ExtensionApplicationStarted && mExtensionApplications.Count > 0)
                {
                    foreach (IExtensionApplication app in mExtensionApplications)
                    {
                        app.OnBeginExecution(Application.Instance.Session);
                    }
                    ExtensionApplicationStarted = true;
                }
            }
            catch(System.Exception ex)
            {
                if (ex.InnerException != null)
                    System.Diagnostics.Debug.Write(ex.InnerException);
                System.Diagnostics.Debug.Write(ex);
                mErrorString = ex.Message;
            }
        }

        // Explicitly start up geometry extension application.
        public void StartUp()
        {
            foreach (IExtensionApplication app in mExtensionApplications)
            {
                app.StartUp();
            }
        }

        // Explicity shut down geometry extension application.
        public void ShutDown()
        {
            foreach (IExtensionApplication app in mExtensionApplications)
            {
                app.ShutDown();
            }
        }

        private static bool GetFactoryFileFromGeometryConfig(ref string factoryFilePath, ref string persistentMgrFilePath)
        {
            IProtoGeometryConfiguration configuration = ProtoGeometryConfigurationManager.Settings;
            if (null != configuration)
            {
                factoryFilePath = configuration.GeometryFactoryFileName;
                persistentMgrFilePath = configuration.PersistentManagerFileName;
            }

            return !String.IsNullOrEmpty(factoryFilePath); //If geometry factory is located we are done,
                                                           //we can deal without persistent manager.
        }

        private static bool GetFactoryFileFromExeConfig(ref string factoryFilePath, ref string persistentMgrFilePath)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (null == config)
                return false;

            var geometryFactoryKeyValue = config.AppSettings.Settings[ConfigurationKeys.GeometryFactory];
            if (geometryFactoryKeyValue != null)
            {
                factoryFilePath = geometryFactoryKeyValue.Value;
            }

            var persistentMgrKeyValue = config.AppSettings.Settings[ConfigurationKeys.PersistentManager];
            if (persistentMgrKeyValue != null)
            {
                persistentMgrFilePath = persistentMgrKeyValue.Value;
            }

            return !String.IsNullOrEmpty(factoryFilePath); //If geometry factory is located we are done,
                                                           //we can deal without persistent manager.
        }

        private static bool GetFactoryFileFromSessionConfig(ref string factoryFilePath, ref string persistentMgrFilePath)
        {
            var session = Application.Instance.Session;
            if (null == session)
                return false;

            IConfiguration executionConfig = session.Configuration;
            if (executionConfig == null)
                return false;

            factoryFilePath = executionConfig.GetConfigValue(ConfigurationKeys.GeometryFactory) as string;
            persistentMgrFilePath = executionConfig.GetConfigValue(ConfigurationKeys.PersistentManager) as string;
            return !String.IsNullOrEmpty(factoryFilePath); //If geometry factory is located we are done,
                                                           //we can deal without persistent manager.
        }

        private object CreateInstance(Type type, string library)
        {
            Object returnValue = null;
            Type externalExtensionType = null;
            Type extensionType = typeof(IExtensionApplication);
            System.Diagnostics.Debug.Write("Trying to load assembly: " + library);
            Assembly assembly = Assembly.LoadFrom(library);
            Type[] types = assembly.GetExportedTypes();
            foreach (var item in types)
            {
                if (item.IsAbstract)
                    continue;

                if (type.IsAssignableFrom(item))
                {
                    returnValue = Activator.CreateInstance(item);
                }

                if (extensionType.IsAssignableFrom(item))
                {
                    externalExtensionType = item;
                }

                if (null != externalExtensionType && null != returnValue)
                    break;
            }

            //If the GeometryFactory instance or the persistent manager is created successfully, and the extension application in that module
            // has not yet been created, then create a extension application as well.
            if (null != returnValue && null != externalExtensionType && !mExtensionApplicationTypes.Contains(externalExtensionType))
            {
                IExtensionApplication extensionApplication = Activator.CreateInstance(externalExtensionType) as IExtensionApplication;
                if (null != extensionApplication)
                {
                    mExtensionApplicationTypes.Add(externalExtensionType);
                    mExtensionApplications.Add(extensionApplication);
                }
            }

            return returnValue;
        }

        //Double check locking for multi-threaded singleton class.
        private static readonly Object syncRoot = new Object();
        public static HostFactory Instance
        {
            get
            {
                if (null == mSelf)
                {
                    lock (syncRoot)
                    {
                        if (null == mSelf)
                            mSelf = new HostFactory();
                    }
                }
                return mSelf;
            }
        }

        internal static HostFactory CurrentInstance
        {
            get
            {
                return mSelf;
            }
        }

        internal static bool ExtensionApplicationStarted { get; set; }

        #endregion

        #region PUBLIC METHODS

        public static IGeometryFactory Factory
        {
            get 
            {
                if (null == Instance.mGeometryFactory)
                {
                    if (string.IsNullOrEmpty(Instance.mErrorString))
                        Instance.mErrorString = "No implementation of IGeometryFactory found, possibly ProtoAcadGeometry.dll is missing.";
                    throw new NotImplementedException(Instance.mErrorString);
                }
                return Instance.mGeometryFactory;
            }
        }

        public static IPersistenceManager PersistenceManager
        {
            get 
            {
                return Instance.mPersistenceManager;
            }
        }

        public static List<IExtensionApplication> ExtensionApplications
        {
            get
            {
                return Instance.mExtensionApplications;
            }
        }

        #endregion
    }

    [Category("Configuration")]
    public class GeometrySettings
    {
        private static IGeometrySettings mHostSettings = null;
        static GeometrySettings()
        {
            Reset();
        }

        private GeometrySettings() { }

        /// <summary>
        /// Provides path of the root script in execution.
        /// </summary>
        public static string RootModulePath { get; internal set; }

        /// <summary>
        /// Provides information about whether to dump XML property for ToString() method
        /// </summary>
        [Browsable(false)]
        public static bool GeometryXmlProperties { get; internal set; }

        /// <summary>
        /// Allows setting default color to all Displayable objects.
        /// </summary>
        public static Color DefaultColor { get; set; }

        /// <summary>
        /// Make point markers visible
        /// </summary>
        public static bool PointVisibility 
        {
            get 
            {
                if (null == mHostSettings)
                    mHostSettings = HostFactory.Factory.GetSettings();
                if (null == mHostSettings)
                    return false;
                return mHostSettings.PointVisibility; 
            }

            set 
            {
                if (null == mHostSettings)
                    mHostSettings = HostFactory.Factory.GetSettings();
                if (null != mHostSettings)
                    mHostSettings.PointVisibility = value; 
            }
        }

        /// <summary>
        /// Resets the geometry settings to default
        /// </summary>
        internal static void Reset()
        {
            DefaultColor = null;
            RootModulePath = string.Empty;
            GeometryXmlProperties = false;
            if(null != mHostSettings)
                mHostSettings.PointVisibility = true; 
        }
    }
}
