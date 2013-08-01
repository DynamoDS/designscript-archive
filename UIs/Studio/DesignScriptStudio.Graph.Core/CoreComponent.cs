using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using DesignScriptStudio.Graph.Core.Services;
using ProtoFFI;
using ProtoCore.AST.AssociativeAST;
using System.Reflection;
using System.IO;
using System.Xml.Serialization;
using DesignScriptStudio.Renderer;
using System.Windows;
using System.Collections.ObjectModel;
using Autodesk.DesignScript.Interfaces;
using System.Windows.Media.Imaging;
using ProtoCore.Mirror;

namespace DesignScriptStudio.Graph.Core
{
    class CoreComponent : ICoreComponent, IRenderServiceConsumer
    {
        #region Class Data Members

        private static CoreComponent coreComponent = null;
        private RenderService renderService = null;
        private BitmapSource previewPlaceholder = null;
        private IGraphUiContainer uiContainer = null;
        private ComponentInitializedDelegate initialized = null;
        private Heartbeat heartbeat = null; //Holds the heartbeat reporter
        private PersistentSettings studioSettings = null;
        private string sessionName = string.Empty;
        private string filteredClasses = string.Empty;
        private LibraryItem rootLibraryItem = null;
        private LibraryItem rootLibraryMethodProperty = null;

        #endregion

        #region public properties

        public string SessionName
        {
            get
            {
                if (string.IsNullOrEmpty(this.sessionName))
                    throw new InvalidOperationException("'ICoreComponent.SessionName' called too early");

                return this.sessionName;
            }
        }

        public bool LaunchedForRecovery
        {
            get;
            private set;
        }

        public PersistentSettings StudioSettings
        {
            get
            {
                if (null == studioSettings)
                {
                    string settingsFilePath = GetSettingsFilePath();
                    studioSettings = PersistentSettings.Deserialize(settingsFilePath);
                }

                return this.studioSettings;
            }
        }

        internal BitmapSource PreviewPlaceholder
        {
            get
            {
                if (null == previewPlaceholder)
                {
                    Uri uriSource = new Uri(ResourceNames.PreviewPlaceholder, UriKind.Absolute);
                    previewPlaceholder = new BitmapImage(uriSource);
                }

                return previewPlaceholder;
            }
        }

        #endregion

        #region Public Interface Methods

        public void InitializeAsync(ComponentInitializedDelegate initialized)
        {
            if (null == initialized)
                throw new ArgumentNullException("initialized");

            this.initialized = initialized;

            BackgroundWorker libraryLoader = new BackgroundWorker();
            libraryLoader.DoWork += new DoWorkEventHandler(OnLibraryLoaderDoWork);
            libraryLoader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(OnLibraryLoaderCompleted);
            libraryLoader.RunWorkerAsync();

            Configurations.InitializeOnce();
        }

        public void Initialize()
        {
            List<LibraryItem> root = LoadLibraryInternal();
            this.rootLibraryItem = root[0];
            this.rootLibraryMethodProperty = root[1];
        }

        public void Shutdown()
        {
            if (null != studioSettings)
            {
                string settingsFilePath = GetSettingsFilePath();
                PersistentSettings.Serialize(settingsFilePath, studioSettings);
                studioSettings = null;
            }

            if (null != this.renderService)
            {
                this.renderService.Shutdown();
                this.renderService = null;
            }
        }

        public LibraryItem GetRootLibraryItem()
        {
            return rootLibraryItem;
        }

        public bool ImportAssembly(string assemblyFilePath, string rootModulePath, bool ignoreDuplicateOrEmptyFileError)
        {
            // check if the file is already imported
            if (this.studioSettings.LoadedAssemblies.Exists((string file) => assemblyFilePath.Equals(file, StringComparison.InvariantCultureIgnoreCase)))
            {
                if (!ignoreDuplicateOrEmptyFileError)
                    AddFeedbackMessage(ResourceNames.Error, UiStrings.DuplicateFailure.Replace("fileName", Path.GetFileName(assemblyFilePath)));

                return false;
            }

            int importedNodes = -1;
            //Set the root module path
            GraphToDSCompiler.GraphUtilities.SetRootModulePath(rootModulePath);
            LibraryItem assemblyItem, assemblyItemMethodProperty;
            importedNodes = ProcessImportAssembly(assemblyFilePath, out assemblyItem, out assemblyItemMethodProperty);

            ProtoCore.BuildStatus buildStatus = GraphToDSCompiler.GraphUtilities.BuildStatus;
            if (importedNodes < 0)
            {
                if (buildStatus.ErrorCount > 0)
                {
                    string fileName = Path.GetFileName(buildStatus.Errors[0].FileName);
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        AddFeedbackMessage(ResourceNames.Error, buildStatus.Errors[0].Message
                            + " At line " + buildStatus.Errors[0].Line + ", column " + buildStatus.Errors[0].Col + ", in " + fileName);
                    }
                    else
                        AddFeedbackMessage(ResourceNames.Error, buildStatus.Errors[0].Message);
                }
                else
                    AddFeedbackMessage(ResourceNames.Error, UiStrings.ImportFailure);
                return false;
            }

            if (buildStatus.WarningCount > 0)
            {
                string fileName = Path.GetFileName(buildStatus.Warnings[0].FileName);
                if (!string.IsNullOrEmpty(fileName))
                {
                    AddFeedbackMessage(ResourceNames.Warning, buildStatus.Warnings[0].msg
                        + " At line " + buildStatus.Warnings[0].line + ", column " + buildStatus.Warnings[0].col + ", in " + fileName);
                }
                else
                    AddFeedbackMessage(ResourceNames.Error, buildStatus.Warnings[0].msg);
            }
            else if (importedNodes == 0 && !ignoreDuplicateOrEmptyFileError)
            {
                AddFeedbackMessage(ResourceNames.Error, UiStrings.EmptyWarning.Replace("fileName", Path.GetFileName(assemblyFilePath)));
                return false;
            }
            else
                AddFeedbackMessage(ResourceNames.Confirmation, UiStrings.ImportSuccess);

            if (Path.GetExtension(assemblyFilePath) != ".ds")
            {
                this.studioSettings.LoadedAssemblies.Add(assemblyFilePath);
            }

            if (assemblyItem != null)
            {
                assemblyItem.IsExternal = true;
                if (assemblyItemMethodProperty != null)
                    assemblyItemMethodProperty.IsExternal = true;

                if (assemblyItem.Children != null && assemblyItem.Children.Count > 0)
                {
                    GroupOverloadProcedure(assemblyItem);
                    SetLibraryItemLevel(assemblyItem, 0);

                    LibraryItem NoResultItem = rootLibraryItem.Children[0];
                    rootLibraryItem.Children.RemoveAt(0);
                    rootLibraryItem.AddChildItem(assemblyItem);
                    rootLibraryItem.Children.Insert(0, NoResultItem);
                }

                if (assemblyItemMethodProperty != null && assemblyItemMethodProperty.Children != null && assemblyItemMethodProperty.Children.Count > 0)
                {
                    GroupOverloadProcedure(assemblyItemMethodProperty);
                    rootLibraryMethodProperty.AddChildItem(assemblyItemMethodProperty);
                }
            }

            return true;
        }

        public void RefreshAssembly(string assemblyFileName)
        {
        }

        public void RemoveAssembly(string assemblyFileName)
        {
            if (!assemblyFileName.ToLower().EndsWith(".ds"))
                RemoveAssemblyFromList(assemblyFileName, this.studioSettings.LoadedAssemblies);

            foreach (LibraryItem item in rootLibraryItem.Children)
            {
                if (item.DisplayText == assemblyFileName)
                {
                    rootLibraryItem.Children.Remove(item);
                    break;
                }
            }
            foreach (LibraryItem itemMethodProperty in rootLibraryMethodProperty.Children)
            {
                if (itemMethodProperty.DisplayText == assemblyFileName)
                {
                    rootLibraryMethodProperty.Children.Remove(itemMethodProperty);
                    break;
                }
            }
        }

        public void DisableSplash()
        {
            if (this.studioSettings == null)
                throw new InvalidOperationException("Settings shouldn't be null (D4D4F1769B3D)");
            else
                this.studioSettings.DontShowSplash = true;
        }

        #endregion

        #region Public Class Methods

        public void NotifyThumbnailReady(ThumbnailData thumbnail)
        {
            if (null != CoreComponent.CurrentDispatcher) // If we're not running in NUnit mode.
            {
                CoreComponent.CurrentDispatcher.Invoke(new Action(() =>
                {
                    uint controllerId = ((uint)thumbnail.documentId);
                    uint nodeId = ((uint)thumbnail.packageId);

                    IGraphController controller = uiContainer.GetController(controllerId);
                    if (null != controller)
                    {
                        VisualNode visual = (controller as GraphController).GetVisualNode(nodeId);
                        if (null != visual)
                            visual.SetPreviewValue(thumbnail);
                    }
                }));
            }
        }

        #endregion

        #region Internal Class Methods

        internal static CoreComponent CreateSingleton(IGraphUiContainer uiContainer, bool enableGeometricPreview)
        {
            if (null != coreComponent)
                throw new InvalidOperationException("'CoreComponent.CreateSingleton' called twice");

            CoreComponent.coreComponent = new CoreComponent(uiContainer, enableGeometricPreview);
            return CoreComponent.coreComponent;
        }

        internal static void DestroySingleton()
        {
            if (null != coreComponent)
            {
                GraphToDSCompiler.GraphUtilities.CleanUp();
                coreComponent.Shutdown();
                coreComponent = null;
            }
        }

        internal static CoreComponent Instance
        {
            get { return coreComponent; }
        }

        internal static System.Windows.Threading.Dispatcher CurrentDispatcher
        {
            get
            {
                if (null != coreComponent.uiContainer)
                    return coreComponent.uiContainer.CurrentDispatcher;

                return null;
            }
        }

        internal IGraphUiContainer UiContainer
        {
            get { return uiContainer; }
        }

        internal IGraphEditorHostApplication HostApplication
        {
            get
            {
                if (null != this.uiContainer)
                    return this.uiContainer.HostApplication;

                return null;
            }
        }

        internal bool RequestVisualization(uint controllerId, uint nodeId, List<IGraphicItem> graphicItems)
        {
            if (null == this.renderService)
                throw new InvalidOperationException("Invalid call as geometric preview is disabled.");
            if (this.renderService.GetServiceStatus() == RenderService.ServiceStatus.NoDisplayDriver)
                return false;

            IRenderPackage package = renderService.CreateRenderPackage(controllerId, nodeId);
            foreach (IGraphicItem graphicItem in graphicItems)
                graphicItem.Tessellate(package);

            return renderService.QueueRenderPackage(package); // Queue for preview generation.
        }

        internal void AddFeedbackMessage(string iconPath, string message)
        {
            if (uiContainer != null)
                uiContainer.AddFeedbackMessage(iconPath, message);
        }

        internal bool GeometricPreviewEnabled
        {
            get { return (null != this.renderService); }
        }

        internal string MapAssemblyPath(string assembly)
        {
            foreach (string assemblyPath in studioSettings.LoadedAssemblies)
            {
                if (Path.GetFileName(assemblyPath) == assembly)
                    return assemblyPath;
            }
            return assembly;
        }

        #endregion

        #region Internal Library Supporting Methods

        internal List<string> GetArgumentNames(string assembly, string qualifiedName, string argumentTypes)
        {
            List<string> result = new List<string>();
            LibraryItem libraryItem;

            // Currently the "Folder" name is the asssembly name,
            // no "MapAssembly" needed
            //
            string assem = assembly;// this.MapAssembly(assembly);
            string className = SplitStringByDot(qualifiedName)[0];
            string constructorName = className;
            if (SplitStringByDot(qualifiedName).Length > 1)
                constructorName = SplitStringByDot(qualifiedName)[1];

            libraryItem = GetLibraryItem(rootLibraryMethodProperty.Children, assem, className, qualifiedName, argumentTypes);
            if (libraryItem == null)
                libraryItem = GetLibraryItem(rootLibraryItem.Children, assem, className, qualifiedName, argumentTypes);

            if (libraryItem != null)
                result = libraryItem.ArgumentNames.Split(',').ToList<string>();

            if (null == result || (result.Count == 0))
                result = this.GetDefaultArgumentNames(argumentTypes);

            return result;
        }

        internal ObservableCollection<LibraryItem> GetConstructors(string assembly, string qualifiedName)
        {
            List<string> result = new List<string>();
            ObservableCollection<LibraryItem> libraryItems = rootLibraryItem.Children;
            ObservableCollection<LibraryItem> temp = new ObservableCollection<LibraryItem>();

            string assem = assembly;// this.MapAssembly(assembly);
            string className = SplitStringByDot(qualifiedName)[0];
            string constructorName = className;
            if (SplitStringByDot(qualifiedName).Length > 1)
                constructorName = SplitStringByDot(qualifiedName)[1];

            libraryItems = this.TraverseLibrary(libraryItems, assem);
            libraryItems = this.TraverseLibrary(libraryItems, className);
            foreach (LibraryItem item in libraryItems)
            {
                //if (item.ItemType == NodeType.None && item.Children != null && item.Children.Count > 0)
                //{
                //    foreach (LibraryItem overloadItem in item.Children)
                //    {
                //        if (overloadItem.Type == LibraryItem.MemberType.Constructor)
                //            temp.Add(overloadItem);
                //    }
                //}
                //else if (item.Type == LibraryItem.MemberType.Constructor)
                temp.Add(item);
            }
            return temp;
        }

        internal ObservableCollection<LibraryItem> GetOverloads(LibraryItem item)
        {
            if (item == null)
                return new ObservableCollection<LibraryItem>();
            if (item.Children == null || item.Children.Count <= 0)
                return new ObservableCollection<LibraryItem>();
            return item.Children;
        }

        internal ObservableCollection<LibraryItem> GetMethodsAndProperties(string assembly, string returnType)
        {
            List<string> result = new List<string>();
            ObservableCollection<LibraryItem> libraryItems = rootLibraryMethodProperty.Children;
            string assem = assembly;// this.MapAssembly(assembly);
            libraryItems = this.TraverseLibrary(libraryItems, assem);
            libraryItems = this.TraverseLibrary(libraryItems, returnType);
            return libraryItems;
        }

        internal LibraryItem GetLibraryItem(string assembly, string qualifiedName, string argumentTypes)
        {
            // Some older BIN files have spaces among the argument types, here we 
            // remove them to avoid getting spaces in argument types comparison.
            argumentTypes = argumentTypes.Replace(" ", string.Empty);

            string assem = assembly;// this.MapAssembly(assembly);
            string className = SplitStringByDot(qualifiedName)[0];

            LibraryItem libraryItem = GetLibraryItem(rootLibraryMethodProperty.Children, assem, className, qualifiedName, argumentTypes);

            if (libraryItem == null) // check in rootLibraryItem
            {
                libraryItem = GetLibraryItem(rootLibraryItem.Children, assem, className, qualifiedName, argumentTypes);
            }
            return libraryItem;
        }

        #endregion

        #region Private Class Event Handlers

        private void OnLibraryLoaderDoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = LoadLibraryInternal();
        }

#if !ImplementationWithMirrors
        private List<LibraryItem> LoadLibraryInternal()
        {
            LibraryItem rootItem = new LibraryItem();
            LibraryItem rootItemMethodProperty = new LibraryItem();

            LoadBuiltInItems(rootItem);

            try
            {
                DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());
                List<string> coreAssemblies = new List<string>();
                coreAssemblies.Add("ProtoGeometry.dll");
                coreAssemblies.Add("Math.dll");

                GraphToDSCompiler.GraphUtilities.PreloadAssembly(coreAssemblies);

                ProcessImportClassTable(GraphToDSCompiler.GraphUtilities.ClassTable, rootItem, rootItemMethodProperty);

                ProcessImportBuiltInFunctions(GraphToDSCompiler.GraphUtilities.BuiltInMethods, rootItem);

                LoadExternalLibraries(rootItem, rootItemMethodProperty);
            }
            catch (ProtoCore.BuildHaltException exception)
            {
                System.Diagnostics.Debug.WriteLine("DSS: " + exception.Message);
                System.Diagnostics.Debug.WriteLine("DSS: " + exception.errorMsg);
                System.Diagnostics.Debug.WriteLine("DSS: " + exception.StackTrace);
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine("DSS: " + exception.Message);
                System.Diagnostics.Debug.WriteLine("DSS: " + exception.StackTrace);
            }

            if (rootItem.Children != null)
            {
                foreach (LibraryItem assemblyItem in rootItem.Children)
                    GroupOverloadProcedure(assemblyItem);
            }

            if (rootItemMethodProperty.Children != null)
            {
                foreach (LibraryItem assemblyItemMethodProperty in rootItemMethodProperty.Children)
                    GroupOverloadProcedure(assemblyItemMethodProperty);
            }

            SetLibraryItemLevel(rootItem, -1);

            List<LibraryItem> root = new List<LibraryItem>();
            root.Add(rootItem);
            root.Add(rootItemMethodProperty);

            return root;
        }
#else
        private LibraryItem LoadLibraryInternal()
        {
            LibraryItem rootItem = new LibraryItem();

            LoadBuiltInItems(rootItem);

            try
            {
                ProcessImportBuiltInFunctions(StaticMirror.BuiltInMethods, rootItem);

                List<string> assemblies = new List<string>();
                assemblies.Add("ProtoGeometry.dll");
                assemblies.Add("Math.dll");

                if (coreComponent.studioSettings.LoadedAssemblies != null
                && coreComponent.studioSettings.LoadedAssemblies.Count > 0)
                {
                    foreach (string externalLibrary in coreComponent.studioSettings.LoadedAssemblies)
                        assemblies.Add(externalLibrary);
                }

                foreach (string assembly in assemblies)
                    ProcessImportAssembly(assembly, rootItem);
            }
            catch (ProtoCore.BuildHaltException exception)
            {
                System.Diagnostics.Debug.WriteLine("DSS: " + exception.Message);
                System.Diagnostics.Debug.WriteLine("DSS: " + exception.errorMsg);
                System.Diagnostics.Debug.WriteLine("DSS: " + exception.StackTrace);
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine("DSS: " + exception.Message);
                System.Diagnostics.Debug.WriteLine("DSS: " + exception.StackTrace);
            }

            SetLibraryItemLevel(rootItem, -1);
            return rootItem;
        }

#endif
        private void OnLibraryLoaderCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            List<LibraryItem> root = e.Result as List<LibraryItem>;
            this.rootLibraryItem = root[0];
            this.rootLibraryMethodProperty = root[1];
            initialized(null != this.rootLibraryItem);
            this.uiContainer.FinishLoadingLibrary();
        }

        #endregion

        #region Private Class Helper Methods

        private CoreComponent(IGraphUiContainer uiContainer, bool enableGeometricPreview)
        {
            // Either create or reuse a session name.
            EstablishSessionName(uiContainer);

            this.uiContainer = uiContainer;
            if (false != enableGeometricPreview)
                this.renderService = new RenderService(this);

            if (this.HostApplication != null)
            {
                object filteredClasses = null;
                Dictionary<string, object> configs = this.HostApplication.Configurations;
                if (configs.TryGetValue(ConfigurationKeys.FilteredClasses, out filteredClasses))
                {
                    this.filteredClasses = ((string)filteredClasses).ToLower();
                    if (!this.filteredClasses.EndsWith(";"))
                        this.filteredClasses += ';';
                }
            }

            this.heartbeat = Heartbeat.GetInstance();
            this.studioSettings = StudioSettings;
        }

        private void EstablishSessionName(IGraphUiContainer uiContainer)
        {
            this.LaunchedForRecovery = false;
            this.sessionName = Guid.NewGuid().ToString("D").ToLower();

            // When an instance of DesignScript Studio crashes, its session name 
            // will be passed to a new instance of the application through command 
            // line argument. See "GraphController.GetBackupFileNameFormat" for 
            // more details.
            // 
            if (null == uiContainer || (null == uiContainer.HostApplication))
                return;

            Dictionary<string, object> configs = uiContainer.HostApplication.Configurations;
            if (null == configs || (configs.Count <= 0))
                return;

            object value = null;
            if (configs.TryGetValue(CoreStrings.SessionNameKey, out value))
            {
                string session = value as string;
                if (!string.IsNullOrEmpty(session))
                {
                    Guid dummyGuid; // Make sure it is a GUID value.
                    if (Guid.TryParse(session, out dummyGuid))
                    {
                        this.sessionName = session;
                        this.LaunchedForRecovery = true;
                    }
                }
            }
        }

        private void LoadBuiltInItems(LibraryItem rootItem)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(LibraryItem));
            TextReader stringReader = new StringReader(Properties.Resources.BuiltInLibrary);

            LibraryItem libraryItem = serializer.Deserialize(stringReader) as LibraryItem;
            stringReader.Close();

            foreach (LibraryItem item in libraryItem.Children)
                rootItem.AddChildItem(item);
        }

        private void LoadExternalLibraries(LibraryItem rootItem, LibraryItem rootItemMethodProperty)
        {
            if (coreComponent.studioSettings.LoadedAssemblies != null
                && coreComponent.studioSettings.LoadedAssemblies.Count > 0)
            {
                foreach (string externalLibrary in coreComponent.studioSettings.LoadedAssemblies)
                {
                    LibraryItem assemblyItem, assemblyItemMethodProperty;
                    ProcessImportAssembly(externalLibrary, out assemblyItem, out assemblyItemMethodProperty);

                    if (assemblyItem != null && assemblyItem.Children != null && assemblyItem.Children.Count > 0)
                    {
                        assemblyItem.IsExternal = true;
                        rootItem.AddChildItem(assemblyItem);
                    }
                    if (assemblyItemMethodProperty != null && assemblyItemMethodProperty.Children != null && assemblyItemMethodProperty.Children.Count > 0)
                    {
                        assemblyItemMethodProperty.IsExternal = true;
                        rootItemMethodProperty.AddChildItem(assemblyItemMethodProperty);
                    }
                }
            }
        }

#if ImplementationWithMirrors
        private void ProcessImportAssembly(string assembly, LibraryItem rootItem)
        {
            LibraryItem assemblyItem = new LibraryItem(NodeType.None, Path.GetFileName(assembly), null);

            LibraryMirror assemblyMirror = GraphToDSCompiler.GraphUtilities.GetLibraryMirror(assembly);

            // Global Functions in DS file
            if (Path.GetExtension(assembly) == ".ds")
            {
                List<MethodMirror> globalFunctions = assemblyMirror.GetGlobalMethods();

                foreach (MethodMirror globalFunctionMirror in globalFunctions)
                {
                    LibraryItem globalFunctionItem = new LibraryItem(NodeType.Function, globalFunctionMirror.MethodName, globalFunctionMirror);
                    globalFunctionItem.ArgumentTypes = GetArgumentTypes(globalFunctionMirror.GetArgumentTypes());
                    globalFunctionItem.Assembly = assemblyMirror.LibraryName;
                    globalFunctionItem.Type = LibraryItem.MemberType.GlobalFunction;
                    assemblyItem.AddChildItem(globalFunctionItem);
                }

                GroupOverloadedItem(assemblyItem);
            }

            foreach (ClassMirror classMirror in assemblyMirror.GetClasses())
            {
                LibraryItem classItem = new LibraryItem(NodeType.None, classMirror.ClassName, null);

                ProcessClassItem(classMirror, classItem);

                if (classItem != null && classItem.Children != null && classItem.Children.Count > 0)
                    assemblyItem.AddChildItem(classItem);
            }
        }

        private void ProcessClassItem(ClassMirror classMirror, LibraryItem classItem)
        {
            classItem.Assembly = classMirror.GetAssembly().LibraryName;

            foreach (MethodMirror constructorMirror in classMirror.GetConstructors())
            {
                LibraryItem constructorItem = new LibraryItem(NodeType.Function, constructorMirror.MethodName, constructorMirror);
                constructorItem.ArgumentTypes = GetArgumentTypes(constructorMirror.GetArgumentTypes());
                constructorItem.Assembly = classMirror.GetAssembly().LibraryName;
                constructorItem.Type = LibraryItem.MemberType.Constructor;
                classItem.AddChildItem(constructorItem);
            }

            foreach (MethodMirror methodMirror in classMirror.GetFunctions())
            {
                if (!methodMirror.IsStatic)
                    continue;

                LibraryItem staticMethodItem = new LibraryItem(NodeType.Function, methodMirror.MethodName, methodMirror);
                staticMethodItem.ArgumentTypes = GetArgumentTypes(methodMirror.GetArgumentTypes());
                staticMethodItem.Assembly = classMirror.GetAssembly().LibraryName;
                staticMethodItem.Type = LibraryItem.MemberType.StaticMethod;
                classItem.AddChildItem(staticMethodItem);
            }

            GroupOverloadedItem(classItem);

            foreach (PropertyMirror propertyMirror in classMirror.GetProperties())
            {
                if (!propertyMirror.IsStatic)
                    continue;

                LibraryItem staticPropertyItem = new LibraryItem(NodeType.Function, propertyMirror.PropertyName, propertyMirror);
                staticPropertyItem.ArgumentTypes = string.Empty;//GetArgumentTypes(propertyMirror.
                staticPropertyItem.Type = LibraryItem.MemberType.StaticProperty;

                if (propertyMirror.IsSetter)
                    staticPropertyItem.DisplayText += UiStrings.OverloadDisplayTextSetter;
                else
                    staticPropertyItem.DisplayText += UiStrings.OverloadDisplayTextGetter;

                classItem.AddChildItem(staticPropertyItem);
            }
        }

        private void ProcessImportBuiltInFunctions(List<MethodMirror> builtinFunctions, LibraryItem rootItem)
        {
            LibraryItem assemblyItem = new LibraryItem(NodeType.None, "Built-in Functions", null);

            foreach (MethodMirror builtinFunction in builtinFunctions)
            {
                if (this.filteredClasses != string.Empty)
                {
                    string procedureName = "Built-in Functions;" + builtinFunction.MethodName + ';';
                    if (filteredClasses.Contains(procedureName.ToLower()))
                        continue;
                }

                LibraryItem builtinFunctionItem = new LibraryItem(NodeType.Function, builtinFunction.MethodName, builtinFunction);
                builtinFunctionItem.ArgumentTypes = GetArgumentTypes(builtinFunction.GetArgumentTypes());
                builtinFunctionItem.Assembly = assemblyItem.DisplayText;
                builtinFunctionItem.Type = LibraryItem.MemberType.GlobalFunction;
                assemblyItem.AddChildItem(builtinFunctionItem);
            }

            GroupOverloadedItem(assemblyItem);

            if (assemblyItem.Children != null || assemblyItem.Children.Count > 0)
                rootItem.AddChildItem(assemblyItem);
        }

        private void GroupOverloadedItem(LibraryItem parentItem)
        {
            int i = 0;
            while (i < parentItem.Children.Count)
            {
                // find items that should be grouped together (from i to j)
                int j = i + 1;
                while (j < parentItem.Children.Count && parentItem.Children[i].DisplayText == parentItem.Children[j].DisplayText)
                    j++;
                j--;

                // grouping
                if (i < j && parentItem.Children[i].DisplayText == parentItem.Children[j].DisplayText)
                {
                    // create group folder
                    LibraryItem groupItem = new LibraryItem(NodeType.None, parentItem.Children[i].QualifiedName, null);
                    groupItem.Assembly = parentItem.Children[i].Assembly;
                    groupItem.Type = parentItem.Children[i].Type;

                    // group i to j to the folder
                    for (int k = i; k <= j; k++)
                    {
                        parentItem.Children[i].IsOverloaded = true;

                        if (parentItem.Children[i].ArgumentNames == "")
                            parentItem.Children[i].DisplayText = UiStrings.OverloadDisplayTextNoParameter;
                        else
                        {
                            List<string> argumentNames = ((MethodMirror)parentItem.Children[i].DataMirror).GetArgumentNames();
                            parentItem.Children[i].DisplayText = GetArgumentNames(argumentNames);
                        }

                        groupItem.AddChildItem(parentItem.Children[i]);
                        parentItem.Children.RemoveAt(i);
                    }

                    parentItem.AddChildItem(groupItem);
                }
                i++;
            }
        }

        private string GetArgumentTypes(List<ProtoCore.Type> argumentTypeList)
        {
            string argumentTypes = string.Empty;

            foreach (ProtoCore.Type argumentType in argumentTypeList)
            {
                if (argumentTypes == string.Empty)
                    argumentTypes = argumentType.ToString();
                else
                    argumentTypes += ',' + argumentType.ToString();
            }

            return argumentTypes;
        }

        private string GetArgumentNames(List<string> argumentNamesList)
        {
            string argumentNames = string.Empty;

            foreach (string argumentName in argumentNamesList)
            {
                if (argumentNames == string.Empty)
                    argumentNames = argumentName;
                else
                    argumentNames += ',' + argumentName;
            }

            return argumentNames;
        }
#else
        private void ProcessImportClassTable(ProtoCore.DSASM.ClassTable classTable, LibraryItem parentItem, LibraryItem parentItemMethodProperty)
        {
            if (classTable == null && classTable.ClassNodes.Count <= (int)ProtoCore.PrimitiveType.kMaxPrimitives)
                return;

            for (int i = (int)ProtoCore.PrimitiveType.kMaxPrimitives; i < classTable.ClassNodes.Count; i++)
            {
                if (this.filteredClasses != string.Empty)
                {
                    string className = classTable.ClassNodes[i].ExternLib + ';' + classTable.ClassNodes[i].name + ';';
                    if (filteredClasses.Contains(className.ToLower()))
                        continue;
                }

                LibraryItem classItem = new LibraryItem(NodeType.None, classTable.ClassNodes[i].name);
                LibraryItem classItemMethodProperty = new LibraryItem(NodeType.None, classTable.ClassNodes[i].name);

                ProcessClassNode(classTable.ClassNodes[i], classTable, classItem, classItemMethodProperty);

                // add the libraryItem to respect assembly libraryItem,
                // if there is no such assembly, create one
                LibraryItem assemblyItem = null;
                LibraryItem assemblyItemMethodProperty = null;

                // Only include the class if it has at least one method listed.
                if (null != classItem.Children && (classItem.Children.Count > 0))
                {
                    if (!string.IsNullOrEmpty(classTable.ClassNodes[i].ExternLib) && parentItem.Children != null && parentItem.Children.Count > 0)
                    {
                        foreach (LibraryItem item in parentItem.Children)
                        {
                            if (item.Assembly == classTable.ClassNodes[i].ExternLib)
                            {
                                assemblyItem = item;
                                break;
                            }
                        }
                    }

                    if (assemblyItem == null)
                    {
                        if (string.IsNullOrEmpty(classTable.ClassNodes[i].ExternLib)) // Custom Class
                        {
                        }
                        assemblyItem = new LibraryItem(NodeType.None, Path.GetFileName(classTable.ClassNodes[i].ExternLib));
                        assemblyItem.Assembly = classTable.ClassNodes[i].ExternLib;
                        assemblyItem.IsExternal = false;
                        parentItem.AddChildItem(assemblyItem);
                    }
                    assemblyItem.AddChildItem(classItem);
                }

                if (null != classItemMethodProperty.Children && (classItemMethodProperty.Children.Count > 0))
                {
                    if (!string.IsNullOrEmpty(classTable.ClassNodes[i].ExternLib) && parentItemMethodProperty.Children != null && parentItemMethodProperty.Children.Count > 0)
                    {
                        foreach (LibraryItem itemMethodProperty in parentItemMethodProperty.Children)
                        {
                            if (itemMethodProperty.Assembly == classTable.ClassNodes[i].ExternLib)
                            {
                                assemblyItemMethodProperty = itemMethodProperty;
                                break;
                            }
                        }
                    }

                    if (assemblyItemMethodProperty == null)
                    {
                        if (string.IsNullOrEmpty(classTable.ClassNodes[i].ExternLib)) // Custom Class
                        {
                        }
                        assemblyItemMethodProperty = new LibraryItem(NodeType.None, Path.GetFileName(classTable.ClassNodes[i].ExternLib));
                        assemblyItemMethodProperty.Assembly = classTable.ClassNodes[i].ExternLib;
                        assemblyItemMethodProperty.IsExternal = false;
                        parentItemMethodProperty.AddChildItem(assemblyItemMethodProperty);
                    }
                    assemblyItemMethodProperty.AddChildItem(classItemMethodProperty);
                }
            }
        }

        private void ProcessImportBuiltInFunctions(List<ProtoCore.DSASM.ProcedureNode> functionList, LibraryItem rootItem)
        {
            LibraryItem assemblyItem = new LibraryItem(NodeType.None, "Built-in Functions");

            foreach (ProtoCore.DSASM.ProcedureNode procedureNode in functionList)
            {
                if (this.filteredClasses != string.Empty)
                {
                    string procedureName = "Built-in Functions;" + procedureNode.name + ';';
                    if (filteredClasses.Contains(procedureName.ToLower()))
                        continue;
                }

                LibraryItem item = ProcessProcedureNode(procedureNode, GraphToDSCompiler.GraphUtilities.ClassTable);
                if (item != null)
                {
                    item.Type = LibraryItem.MemberType.GlobalFunction;
                    item.Assembly = assemblyItem.DisplayText;
                    item.QualifiedName = item.DisplayText;
                    assemblyItem.AddChildItem(item);
                }
            }

            if (assemblyItem.Children != null || assemblyItem.Children.Count > 0)
                rootItem.AddChildItem(assemblyItem);
        }

        private int ProcessImportAssembly(string assemblyFilePath, out LibraryItem assemblyItem, out LibraryItem assemblyItemMethodProperty)
        {
            int importedNodes = -1;

            assemblyItem = null;
            assemblyItemMethodProperty = null;

            DLLFFIHandler.Register(FFILanguage.CSharp, new CSModuleHelper());

            IList<ProtoCore.DSASM.ClassNode> classNodes = GraphToDSCompiler.GraphUtilities.GetClassesForAssembly(assemblyFilePath);
            importedNodes = classNodes.Count();

            ProtoCore.BuildStatus buildStatus = GraphToDSCompiler.GraphUtilities.BuildStatus;
            if (buildStatus.ErrorCount > 0)
                return -1;

            assemblyItem = new LibraryItem(NodeType.None, Path.GetFileName(assemblyFilePath));
            assemblyItemMethodProperty = new LibraryItem(NodeType.None, Path.GetFileName(assemblyFilePath));

            foreach (ProtoCore.DSASM.ClassNode classNode in classNodes)
            {
                LibraryItem classItem = new LibraryItem(NodeType.None, classNode.name);
                LibraryItem classItemMethodProperty = new LibraryItem(NodeType.None, classNode.name);

                ProcessClassNode(classNode, GraphToDSCompiler.GraphUtilities.ClassTable, classItem, classItemMethodProperty);

                if (classItem != null && classItem.Children != null && classItem.Children.Count > 0)
                    assemblyItem.AddChildItem(classItem);
                if (classItemMethodProperty != null && classItemMethodProperty.Children != null && classItemMethodProperty.Children.Count > 0)
                    assemblyItemMethodProperty.AddChildItem(classItemMethodProperty);
            }

            // Global Functions in DS file
            if (Path.GetExtension(assemblyFilePath) == ".ds")
            {
                List<ProtoCore.DSASM.ProcedureNode> procNodes = GraphToDSCompiler.GraphUtilities.GetGlobalMethods(assemblyFilePath);

                importedNodes += procNodes.Count();

                foreach (ProtoCore.DSASM.ProcedureNode procedureNode in procNodes)
                {
                    LibraryItem item = ProcessProcedureNode(procedureNode, GraphToDSCompiler.GraphUtilities.ClassTable);
                    if (item != null)
                    {
                        item.Type = LibraryItem.MemberType.GlobalFunction;
                        item.Assembly = Path.GetFileName(assemblyFilePath);
                        item.QualifiedName = item.DisplayText;
                        assemblyItem.AddChildItem(item);
                    }
                }
            }

            return importedNodes;
        }

        private void ProcessClassNode(ProtoCore.DSASM.ClassNode classNode, ProtoCore.DSASM.ClassTable classTable, LibraryItem classItem, LibraryItem classItemMethodProperty)
        {
            //TODO: Victor
            // Temperarily fix for multiple setter
            LibraryItem setterItem = null;

            string friendlyName = GetFriendlyName(classNode.ExternLib);
            if (UiStrings.ProtoGeometryFriendlyName == friendlyName)
            {
                // Fix: IDE-1604 Usage of text crashes the IDE. The OpenGL renderer 
                // does not have a way of accepting text inputs, therefore we don't 
                // have a way to render text on the preview, filter it out for now.
                if (classNode.name == "Text")
                    return;
            }

            classItem.Assembly = classNode.ExternLib;
            classItemMethodProperty.Assembly = classNode.ExternLib;

            foreach (ProtoCore.DSASM.ProcedureNode procedureNode in classNode.vtable.procList)
            {
                if (procedureNode.isAutoGeneratedThisProc) // Temporary functions to be excluded.
                    continue;

                LibraryItem item = ProcessProcedureNode(procedureNode, classTable);
                if (item != null)
                {
                    //TODO: Victor
                    // Temperarily fix for multiple setter
                    if (procedureNode.name.StartsWith(ProtoCore.DSASM.Constants.kSetterPrefix))
                    {
                        if (setterItem != null && item.DisplayText == setterItem.DisplayText)
                            continue;
                        else
                            setterItem = item;
                    }


                    if (classNode.ExternLib != null)
                        item.Assembly = classNode.ExternLib;
                    else
                        item.Assembly = string.Empty;

                    item.QualifiedName = string.Format("{0}.{1}", classNode.name, item.DisplayText);

                    if (item.Type == LibraryItem.MemberType.InstanceMethod ||  // add the "this" input to instance method and property(setter only)
                        (item.Type == LibraryItem.MemberType.InstanceProperty && procedureNode.name.StartsWith(ProtoCore.DSASM.Constants.kSetterPrefix)))
                    {
                        if (string.IsNullOrEmpty(item.ArgumentNames))
                        {
                            item.ArgumentNames = "this";
                            item.ArgumentTypes = "this";
                        }
                        else
                        {
                            item.ArgumentNames = "this," + item.ArgumentNames;
                            item.ArgumentTypes = "this," + item.ArgumentTypes;
                        }
                    }

                    if (procedureNode.isStatic || procedureNode.isConstructor)
                        classItem.AddChildItem(item);
                    else
                        classItemMethodProperty.AddChildItem(item);
                }
            }
        }

        private LibraryItem ProcessProcedureNode(ProtoCore.DSASM.ProcedureNode procedureNode, ProtoCore.DSASM.ClassTable classTable)
        {
            LibraryItem item;
            string argumentTypes = string.Empty;
            string argumentNames = string.Empty;
            string returnType = string.Empty;

            string getterPrefix = ProtoCore.DSASM.Constants.kGetterPrefix;
            string setterPrefix = ProtoCore.DSASM.Constants.kSetterPrefix;

            if (procedureNode.name.StartsWith(getterPrefix))  //Property
            {
                item = new LibraryItem(NodeType.Property, procedureNode.name.Remove(0, getterPrefix.Length));

                if (procedureNode.isStatic)
                    item.Type = LibraryItem.MemberType.StaticProperty;
                else
                    item.Type = LibraryItem.MemberType.InstanceProperty;
            }
            else // Constructor or Method
            {
                if (procedureNode.name.StartsWith(setterPrefix))
                {
                    item = new LibraryItem(NodeType.Function, procedureNode.name.Remove(0, setterPrefix.Length));

                    if (procedureNode.isStatic)
                        item.Type = LibraryItem.MemberType.StaticProperty;
                    else
                        item.Type = LibraryItem.MemberType.InstanceProperty;
                }
                else
                {
                    item = new LibraryItem(NodeType.Function, procedureNode.name);

                    if (procedureNode.isConstructor)
                        item.Type = LibraryItem.MemberType.Constructor;
                    else if (procedureNode.isStatic)
                        item.Type = LibraryItem.MemberType.StaticMethod;
                    else
                        item.Type = LibraryItem.MemberType.InstanceMethod;
                }
                if (procedureNode.argInfoList != null && procedureNode.argInfoList.Count() > 0)
                {
                    for (int i = 0; i < procedureNode.argInfoList.Count; i++)
                    {
                        if (argumentNames.Length > 0)
                            argumentNames += ",";

                        argumentNames += procedureNode.argInfoList[i].Name;

                        if (argumentTypes.Length > 0)
                            argumentTypes += ",";

                        argumentTypes += procedureNode.argTypeList[i].Name;
                    }
                }
            }
            item.ReturnType = classTable.ClassNodes[procedureNode.returntype.UID].name;
            item.ArgumentNames = argumentNames;
            item.ArgumentTypes = argumentTypes;

            return item;
        }

        private void GroupOverloadProcedure(LibraryItem parentItem)
        {
            int i = 0;
            while (i < parentItem.Children.Count)
            {
                // it's a folder, no grouping needed, process to group its children
                if (parentItem.Children[i].ItemType == NodeType.None)
                {
                    if (parentItem.Children[i].Children.Count > 1)
                        GroupOverloadProcedure(parentItem.Children[i]);
                }
                else
                {
                    // find items that should be grouped together (from i to j)
                    int j = i + 1;
                    while (j < parentItem.Children.Count && parentItem.Children[i].DisplayText == parentItem.Children[j].DisplayText)
                        j++;

                    j--;

                    // grouping
                    if (i < j && parentItem.Children[i].DisplayText == parentItem.Children[j].DisplayText)
                    {
                        // create group folder
                        LibraryItem groupItem = new LibraryItem(NodeType.None, parentItem.Children[i].QualifiedName);
                        groupItem.Assembly = parentItem.Children[i].Assembly;
                        groupItem.Type = parentItem.Children[i].Type;

                        // group i to j to the folder
                        for (int k = i; k <= j; k++)
                        {
                            parentItem.Children[i].IsOverloaded = true;

                            // Update the display text
                            if (parentItem.Children[i].Type == LibraryItem.MemberType.StaticProperty
                                || parentItem.Children[i].Type == LibraryItem.MemberType.InstanceProperty)
                            {
                                if (parentItem.Children[i].ArgumentNames == "" || parentItem.Children[i].ItemType == NodeType.Property)
                                    parentItem.Children[i].DisplayText += UiStrings.OverloadDisplayTextGetter;
                                else
                                    parentItem.Children[i].DisplayText += UiStrings.OverloadDisplayTextSetter;
                            }
                            else
                            {
                                if (parentItem.Children[i].ArgumentNames == "")
                                    parentItem.Children[i].DisplayText = UiStrings.OverloadDisplayTextNoParameter;
                                else
                                    parentItem.Children[i].DisplayText = parentItem.Children[i].ArgumentNames;
                            }

                            groupItem.AddChildItem(parentItem.Children[i]);
                            parentItem.Children.RemoveAt(i);
                        }

                        parentItem.AddChildItem(groupItem);
                    }
                }
                i++;
            }
        }
#endif

        private void SetLibraryItemLevel(LibraryItem item, int level)
        {
            item.Level = level;
            if (item.Children != null)
                if (item.Children.Count != 0)
                {
                    level++;
                    foreach (LibraryItem subItem in item.Children)
                        SetLibraryItemLevel(subItem, level);
                }
        }

        private void RemoveAssemblyFromList(string assemblyFileName, List<string> list)
        {
            string libraryPath = string.Empty;
            foreach (string path in list)
            {
                if (Path.GetFileName(path) == assemblyFileName)
                {
                    libraryPath = path;
                    break;
                }
            }
            if (libraryPath != string.Empty)
                list.Remove(libraryPath);
        }

        private string GetCurrentDirectory()
        {
            string fullPath = Assembly.GetAssembly(typeof(LibraryItem)).Location;
            return Path.GetDirectoryName(fullPath);
        }

        private ObservableCollection<LibraryItem> TraverseLibrary(ObservableCollection<LibraryItem> libraryItems, string str)
        {
            if (libraryItems != null)
            {
                foreach (LibraryItem item in libraryItems)
                {
                    if (str == item.QualifiedName)
                    {
                        if (item.Children != null && item.Children.Count > 0)
                            return item.Children;
                        else
                        {
                            ObservableCollection<LibraryItem> temp = new ObservableCollection<LibraryItem>();
                            temp.Add(item);
                            return temp;
                        }
                    }
                }
            }
            return new ObservableCollection<LibraryItem>();
        }

        private string[] SplitStringByDot(string str)
        {
            return str.Split('.');
        }

        private string GetFriendlyName(string assembly)
        {
            if (string.IsNullOrEmpty(assembly))
                return UiStrings.UserLibraryFriendlyName;

            switch (assembly)
            {
                case "ProtoGeometry.dll": return UiStrings.ProtoGeometryFriendlyName;
                case "Math.dll": return UiStrings.MathLibraryFriendlyName;
                default: return assembly;
            }
        }

        private string MapAssembly(string assembly)
        {
            string assem = string.Empty;
            switch (assembly)
            {
                case "":
                    assem = "Special Nodes";
                    break;
                default:
                    assem = assembly;
                    //assem = Path.GetFileNameWithoutExtension(assembly);
                    break;
            }
            return assem;
        }

        private List<string> GetDefaultArgumentNames(string argumentTypes)
        {
            List<string> argumentNames = new List<string>();
            if (string.IsNullOrEmpty(argumentTypes))
                return argumentNames;

            // Some older BIN files have spaces among the argument types, here we 
            // remove them to avoid getting spaces in argument types comparison.
            argumentTypes = argumentTypes.Replace(" ", string.Empty);

            if (!string.IsNullOrEmpty(argumentTypes))
            {
                int arguments = argumentTypes.Count(c => c == ',') + 1;
                for (int index = 0; index < arguments; ++index)
                {
                    argumentNames.Add(string.Format(UiStrings.ArgumentNameFmt, index));
                }
            }

            return argumentNames;
        }

        private string GetSettingsFilePath()
        {
            try
            {
                string appDataFolder = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.ApplicationData);

                if (!appDataFolder.EndsWith("\\"))
                    appDataFolder += "\\";

                appDataFolder += @"Autodesk\DesignScript Studio\";
                if (Directory.Exists(appDataFolder) == false)
                    Directory.CreateDirectory(appDataFolder);

                return (appDataFolder + @"StudioSettings.xml");
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private LibraryItem GetLibraryItem(ObservableCollection<LibraryItem> libraryItems, string assem, string className, string qualifiedName, string argumentTypes)
        {
            libraryItems = this.TraverseLibrary(libraryItems, assem);
            libraryItems = this.TraverseLibrary(libraryItems, className);
            foreach (LibraryItem item in libraryItems)
            {
                if (item.QualifiedName != qualifiedName)
                    continue;

                // Overloaded folder
                if (item.ItemType == NodeType.None && item.Children != null && item.Children.Count > 0)
                {
                    foreach (LibraryItem overloadItem in item.Children)
                    {
                        if (MatchArgumentTypes(overloadItem, className, argumentTypes))
                            return overloadItem;
                    }
                }
                else if (MatchArgumentTypes(item, className, argumentTypes))
                    return item;
            }
            return null;
        }

        private bool MatchArgumentTypes(LibraryItem item, string className, string argumentTypes)
        {
            if (argumentTypes == item.ArgumentTypes)
                return true;

            // for old files that got either classname or "" in the argumentTypes
            if ((item.Type == LibraryItem.MemberType.InstanceProperty || item.Type == LibraryItem.MemberType.InstanceMethod)
                && MatchInstancePropertyArgumentTypes(className, argumentTypes, item.ArgumentTypes))
                return true;

            return false;
        }

        private bool MatchInstancePropertyArgumentTypes(string className, string key, string library)
        {
            if (key == "" && library == "this")
                return true;
            if (key.Contains(className))
            {
                string keyWithoutFirstParameter = key.Remove(key.IndexOf(className), className.Count());
                string libraryWithourFirstParametere = library.Remove(library.IndexOf("this"), "this".Count());
                if (keyWithoutFirstParameter == libraryWithourFirstParametere)
                    return true;
            }

            return false;
        }

        #endregion
    }
}
