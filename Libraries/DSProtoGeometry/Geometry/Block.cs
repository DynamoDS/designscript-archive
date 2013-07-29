using System;
using System.ComponentModel;
using System.IO;
using Autodesk.DesignScript.Interfaces;
using System.Collections.Generic;

namespace Autodesk.DesignScript.Geometry
{
    public class DSBlockInstance : DSGeometry
    {
        internal IBlockEntity BlockEntity { get { return HostImpl as IBlockEntity; } }

        #region PRIVATE_CONSTRUCTORS

        static void InitType()
        {
            RegisterHostType(typeof(IBlockEntity), (IGeometryEntity host, bool persist) => { return new DSBlockInstance(host as IBlockEntity, persist); });
        }

        private DSBlockInstance(IBlockEntity entity, bool persist = false)
            : base(entity, persist)
        {
            InitializeGuaranteedProperties();
        }

        #endregion

        #region INTERNAL_METHODS

        private void InitializeGuaranteedProperties()
        {
        }

        #endregion

        #region INTERNAL_CONSTRUCTORS

        internal DSBlockInstance(DSCoordinateSystem contextCoordinateSystem, string fileName, string blockName, bool persist)
            : base(DSBlock.InsertCore(contextCoordinateSystem, ref fileName, blockName), persist)
        {
            Definition = new DSBlock(blockName, fileName);
            ContextCoordinateSystem = contextCoordinateSystem;
        }

        internal DSBlockInstance(DSCoordinateSystem contextCoordinateSystem, string blockName, bool persist)
            : base(DSBlock.InsertCore(contextCoordinateSystem, blockName),persist)
        {
            Definition = new DSBlock(blockName);
            ContextCoordinateSystem = contextCoordinateSystem;
        }
        #endregion


        #region PUBLIC_METHODS

        /// <summary>
        /// Extracts the geometries contained in the block reference
        /// </summary>
        /// <returns></returns>
        public DSGeometry[] ExtractGeometry()
        {
            List<DSGeometry> geometries = new List<DSGeometry>();
            IGeometryEntity[] entities = BlockEntity.ExtractGeometry();
            foreach (var entity in entities)
            {
                if (null != entity)
                    geometries.Add(DSGeometry.ToGeometry(entity, true, this));
            }
            return geometries.ToArray();
        }

        [Category("Primary")]
        public DSBlock Definition { get; set; }
        #endregion
    }

    public class DSBlock
    {
        /// <summary>
        /// Creates a block from blockName
        /// </summary>
        /// <param name="blockName"></param>
        internal DSBlock(string blockName)
        {
            Name = blockName;
        }

        /// <summary>
        /// Creates a block from blockName and fileName
        /// </summary>
        /// <param name="blockName"></param>
        /// <param name="fileName"></param>
        internal DSBlock(string blockName, string fileName)
        {
            Name = blockName;
            SourceFileName = fileName;
        }

        #region PUBLIC_METHODS

        /// <summary>
        /// Creates a block with the given name, reference point and from the
        /// specified geometries
        /// </summary>
        /// <param name="blockName">the block name</param>
        /// <param name="referencePoint">the reference point</param>
        /// <param name="contents">the geometries contained in the block</param>
        /// <returns></returns>
        public static DSBlock FromGeometry(string blockName, DSPoint referencePoint,
             DSGeometry[] contents)
        {
            DSCoordinateSystem referenceCoordinateSystem = DSCoordinateSystem.Identity();
            referenceCoordinateSystem.Translate(referencePoint.X, referencePoint.Y,
                referencePoint.Z);
            return FromGeometry(blockName, referenceCoordinateSystem, contents);
        }

        /// <summary>
        /// Creates a block with the given name, reference coordinate system and from the
        /// specified geometries
        /// </summary>
        /// <param name="blockName">the block name</param>
        /// <param name="referenceCoordinateSystem">the reference coordinate system</param>
        /// <param name="contents">the geometries contained in the block</param>
        /// <returns></returns>
        public static DSBlock FromGeometry(string blockName, DSCoordinateSystem referenceCoordinateSystem,
             DSGeometry[] contents)
        {
            string kMethodName = "DSBlock.FromGeometry";
            if (null == referenceCoordinateSystem)
                throw new ArgumentNullException("contextCoordinateSystem");
            if (string.IsNullOrEmpty(blockName))
                throw new ArgumentException(string.Format(Properties.Resources.InvalidInput, blockName, kMethodName), "blockName");

            IGeometryEntity[] hosts = contents.ConvertAll(DSGeometryExtension.ToEntity<DSGeometry, IGeometryEntity>);
            if (null == hosts || hosts.Length == 0)
                throw new ArgumentException(string.Format(Properties.Resources.InvalidInput, "geometries", kMethodName), "geometries");

            IBlockHelper helper = HostFactory.Factory.GetBlockHelper();
            if (null == helper)
                throw new InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));

            if (helper.DefineBlock(referenceCoordinateSystem.CSEntity, blockName, hosts))
            {
                return new DSBlock(blockName);
            }

            return null;
        }

        /// <summary>
        /// Imports a block with the given name from the outside file to the
        /// current drawing
        /// </summary>
        /// <param name="blockName">the given block name</param>
        /// <param name="filePath">the file path for the outside file</param>
        /// <returns></returns>
        public static DSBlock Import(string blockName, string filePath)
        {
            string kMethodName = "DSBlock.Import";

            filePath = DSGeometryExtension.LocateFile(filePath);
            if (!File.Exists(filePath))
                throw new ArgumentException(string.Format(Properties.Resources.FileNotFound, filePath), "filePath");

            if (string.IsNullOrEmpty(blockName))
                throw new ArgumentException(string.Format(Properties.Resources.InvalidInput, blockName, kMethodName), "blockName");

            IBlockHelper helper = HostFactory.Factory.GetBlockHelper();
            if (null == helper)
                throw new InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));

            helper.ImportBlockFromFile(filePath, blockName);
            return new DSBlock(blockName, filePath);
        }

        /// <summary>
        /// Imports all blocks in the outside file to the current drawing
        /// </summary>
        /// <param name="filePath">the file path for the outside file</param>
        /// <returns></returns>
        public static DSBlock[] ImportAll(string filePath)
        {
            string kMethodName = "DSBlock.ImportAll";

            filePath = DSGeometryExtension.LocateFile(filePath);
            if (!File.Exists(filePath))
                throw new ArgumentException(string.Format(Properties.Resources.FileNotFound, filePath), "filePath");

            IBlockHelper helper = HostFactory.Factory.GetBlockHelper();
            if (null == helper)
                throw new InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));

            string[] blockNames = helper.ImportAllBlocksFromFile(filePath);
            List<DSBlock> blocks = new List<DSBlock>();
            foreach (var name in blockNames)
            {
                DSBlock block = new DSBlock(name, filePath);
                blocks.Add(block);
            }
            return blocks.ToArray();
        }

        /// <summary>
        /// Lists all available blocks in the current drawing
        /// </summary>
        /// <returns></returns>
        public static DSBlock[] AvailableBlockDefinitions()
        {
            IBlockHelper helper = HostFactory.Factory.GetBlockHelper();
            if (null == helper)
                throw new InvalidOperationException(string.Format(Properties.Resources.OperationFailed, "DSBlock.AvailableBlockDefinitions"));

            string[] blockNames = helper.ListAllBlocksInCurrentDocument();
            List<DSBlock> blocks = new List<DSBlock>();
            foreach (var name in blockNames)
            {
                DSBlock block = new DSBlock(name);
                blocks.Add(block);
            }
            return blocks.ToArray();
        }
        
        /// <summary>
        /// Removes unused block definitions from current file
        /// </summary>
        /// <param name="blockName">
        /// The name of the block to be purged, the block must exist in current 
        /// file</param>
        /// <returns>Returns true if the block-definition could be purged, 
        /// else false</returns>
        public static bool Purge(string blockName)
        {
            string kMethodName = "DSBlock.Purge";
            if (string.IsNullOrEmpty(blockName))
                throw new ArgumentException(string.Format(Properties.Resources.InvalidInput, blockName, kMethodName), "blockName");

            IBlockHelper helper = HostFactory.Factory.GetBlockHelper();
            if (null == helper)
                throw new InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));

            return helper.PurgeBlock(blockName);
        }

        /// <summary>
        /// Rename specified block to new name
        /// </summary>
        /// <param name="oldName">
        /// The block to be renamed, must exist in current file</param>
        /// <param name="newName">
        /// New name of the block, must not exist</param>
        /// <returns>Returns true if successfully renamed, else false</returns>
        public bool Rename(string newName)
        {
            string kMethodName = "DSBlock.Rename";
            if (string.IsNullOrEmpty(Name))
                throw new ArgumentException(string.Format(Properties.Resources.InvalidInput, Name, kMethodName), "oldName");

            if (string.IsNullOrEmpty(newName))
                throw new ArgumentException(string.Format(Properties.Resources.InvalidInput, newName, kMethodName), "newName");

            IBlockHelper helper = HostFactory.Factory.GetBlockHelper();
            if (null == helper)
                throw new InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));

            if (helper.RenameBlock(Name, newName))
            {
                Name = newName;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Lists the types of geometry in the block
        /// </summary>
        /// <returns>All the type names</returns>
        public string[] ContainedGeometryTypes()
        {
            string kMethodName = "DSBlock.ContainedGeometryTypes";
            if (string.IsNullOrEmpty(Name))
                throw new ArgumentException(string.Format(Properties.Resources.InvalidInput, Name, kMethodName), "blockName");

            IBlockHelper helper = HostFactory.Factory.GetBlockHelper();
            if (null == helper)
                throw new InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));

            return helper.ListContentsOfBlock(Name);
        }

        /// <summary>
        /// Exports the block definition, without creating instance.
        /// This means a new block record will be created in the target file.
        /// If the block with the same name already exists in the target file, the old
        /// block will be overwritten.
        /// </summary>
        /// <param name="filePath">The outside file path</param>
        /// <returns>Returns true if the export operation is successful</returns>
        public bool Export(string filePath)
        {
            filePath = DSGeometry.GetFullPath(filePath);

            string kMethodName = "DSBlock.Export";
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException(string.Format(Properties.Resources.InvalidInput, filePath, kMethodName), "filePath");

            if (string.IsNullOrEmpty(Name))
                throw new ArgumentException(string.Format(Properties.Resources.InvalidInput, Name, kMethodName), "sourceBlockName");

            IBlockHelper helper = HostFactory.Factory.GetBlockHelper();
            if (null == helper)
                throw new InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));

            return helper.ExportBlockDefinition(filePath, Name);
        }

        /// <summary>
        /// Exports the geometries of the block to the outside file.
        /// This is similar to AutoCAD's wblock operation.
        /// </summary>
        /// <param name="filePath">The outside file path</param>
        /// <returns>Returns true if the export operation is successful</returns>
        public bool ExportGeometry(string filePath)
        {
            filePath = DSGeometry.GetFullPath(filePath);

            string kMethodName = "DSBlock.ExportGeometry";
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException(string.Format(Properties.Resources.InvalidInput, filePath, kMethodName), "filePath");

            if (string.IsNullOrEmpty(Name))
                throw new ArgumentException(string.Format(Properties.Resources.InvalidInput, Name, kMethodName), "sourceBlockName");

            IBlockHelper helper = HostFactory.Factory.GetBlockHelper();
            if (null == helper)
                throw new InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));

            return helper.ExportBlock(filePath, Name);
        }

        /// <summary>
        /// Instantiates the specified block by name with the orientation 
        /// specified by the coordinate system
        /// </summary>
        /// <param name="contextCoordinateSystem">
        /// Specifies the orientation of the block. Origin is the placement point.
        /// This coordinate system must be orthogonal, can be non-uniformly scaled
        /// </param>
        /// <returns>BlockInstance</returns>
        public DSBlockInstance ByCoordinateSystem(DSCoordinateSystem contextCoordinateSystem)
        {
            return new DSBlockInstance(contextCoordinateSystem, Name, true);
        }

        /// <summary>
        /// Instantiates the specified block by name with the orientation 
        /// specified by the coordinate system into the target file.
        /// If the block does not exist in the target file, the block will be exported to the
        /// target file first.
        /// If the block already exists in the target file, the old block will be replaced with
        /// the new one.
        /// </summary>
        /// <param name="contextCoordinateSystem">
        /// Specifies the orientation of the block. Origin is the placement point.
        /// This coordinate system must be orthogonal, can be non-uniformly scaled
        /// </param>
        /// <param name="targetFileName">the outside file name</param>
        /// <returns>If the insertion succeeds, returns true</returns>
        public bool ByCoordinateSystem(DSCoordinateSystem contextCoordinateSystem, string targetFileName)
        {
            string kMethodName = "DSBlock.ByCoordinateSystem ";
            if (null == contextCoordinateSystem)
                throw new ArgumentNullException("contextCoordinateSystem");
            if (contextCoordinateSystem.IsSheared)
                throw new ArgumentException(string.Format(Properties.Resources.Sheared, "contextCoordinateSystem"), "contextCoordinateSystem");
            if (string.IsNullOrEmpty(targetFileName))
                throw new ArgumentException(string.Format(Properties.Resources.InvalidInput, Name, kMethodName), "blockName");

            targetFileName = DSGeometry.GetFullPath(targetFileName);

            IBlockHelper helper = HostFactory.Factory.GetBlockHelper();
            if (null == helper)
                throw new InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));

            return helper.InsertBlockInTargetFile(contextCoordinateSystem.CSEntity, Name, targetFileName);
        }
        
        /// <summary>
        /// Checks if specified block exists in current file
        /// </summary>
        /// <param name="blockName">Name of the block</param>
        /// <returns>Returns true if the block exists else false</returns>
        public static bool Exists(string blockName)
        {
            if (string.IsNullOrEmpty(blockName))
                throw new ArgumentException(string.Format(Properties.Resources.InvalidInput, blockName, "DSBlock.Exists"), "blockName");

            IBlockHelper helper = HostFactory.Factory.GetBlockHelper();
            if (null == helper)
                throw new InvalidOperationException(string.Format(Properties.Resources.OperationFailed, "DSBlock.Exists"));

            return helper.BlockExistsInCurrentDocument(blockName);
        }

        #endregion

        #region CORE_METHODS

        internal static IBlockEntity InsertCore(DSCoordinateSystem contextCoordinateSystem, ref string fileName, string blockName)
        {
            string kMethodName = "DSBlock.ByCoordinateSystem ";
            if (null == contextCoordinateSystem)
                throw new ArgumentNullException("contextCoordinateSystem");
            if (contextCoordinateSystem.IsSheared)
                throw new ArgumentException(string.Format(Properties.Resources.Sheared, "contextCoordinateSystem"), "contextCoordinateSystem");
            if (string.IsNullOrEmpty(blockName))
                throw new ArgumentException(string.Format(Properties.Resources.InvalidInput, blockName, kMethodName), "blockName");

            fileName = DSGeometryExtension.LocateFile(fileName);
            if (!File.Exists(fileName))
                throw new ArgumentException(string.Format(Properties.Resources.FileNotFound, fileName), "fileName");

            IBlockHelper helper = HostFactory.Factory.GetBlockHelper();
            if (null == helper)
                throw new InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));

            IBlockEntity entity = helper.InsertBlockFromFile(contextCoordinateSystem.CSEntity, fileName, blockName);
            if (null == entity)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, kMethodName));
            return entity;
        }

        internal static IBlockEntity InsertCore(DSCoordinateSystem contextCoordinateSystem, string blockName)
        {
            string kMethodName = "DSBlock.ByCoordinateSystem ";
            if (null == contextCoordinateSystem)
                throw new ArgumentNullException("contextCoordinateSystem");
            if (contextCoordinateSystem.IsSheared)
                throw new ArgumentException(string.Format(Properties.Resources.Sheared, "contextCoordinateSystem"), "contextCoordinateSystem");
            if (string.IsNullOrEmpty(blockName))
                throw new ArgumentException(string.Format(Properties.Resources.InvalidInput, blockName, kMethodName), "blockName");

            IBlockHelper helper = HostFactory.Factory.GetBlockHelper();
            if (null == helper)
                throw new InvalidOperationException(string.Format(Properties.Resources.OperationFailed, kMethodName));
            if (!helper.BlockExistsInCurrentDocument(blockName))
                throw new System.ArgumentException(string.Format(Properties.Resources.DoesNotExist, "DSBlock : " + blockName));

            IBlockEntity entity = helper.InsertBlockFromCurrentDocument(contextCoordinateSystem.CSEntity, blockName);
            if (null == entity)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, kMethodName));
            return entity;
        }

        #endregion

        #region PROPERTIES

        [Category("Primary")]
        public string Name 
        {
            get
            {
                return mBlockName;
            }
            private set { mBlockName = value; }
        }

        [Category("Primary")]
        public string SourceFileName
        {
            get
            {
                return mFileName;
            }
            private set { mFileName = value; }
        }
        
        #endregion

        #region PRIVATE_MEMBERS

        private string mBlockName;
        private string mFileName;

        #endregion
    }
}
