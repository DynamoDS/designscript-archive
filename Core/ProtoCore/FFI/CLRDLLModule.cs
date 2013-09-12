﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using ProtoCore.AST.AssociativeAST;
using ProtoCore.Utils;

namespace ProtoFFI
{
    /// <summary>
    /// This class creates ClassDeclNode for a given type and caches all the
    /// imported types. This class also keeps the list of FFIFunctionPointer
    /// for the given type.
    /// </summary>
    public class CLRModuleType
    {
        #region CONSTRUCTORS

        /// <summary>
        /// Private constructor to create empty CLRModuleType.
        /// </summary>
        /// <param name="type">System.Type</param>
        private CLRModuleType(Type type)
        {
            CLRType = type;
            string classname = CLRObjectMarshler.GetTypeName(type);
            ClassNode = CreateEmptyClassNode(classname);
        }

        static CLRModuleType()
        {
            mDisposeMethod = typeof(CLRModuleType).GetMethod("Dispose", BindingFlags.Static | BindingFlags.NonPublic);
        }
        #endregion

        #region PUBLIC_METHODS_AND_PROPERTIES

        /// <summary>
        /// Gets CLRModuleType for given Type. If CLRModuleType instance for the
        /// given type is not found, it creates a new one. If CLRDLLModule is
        /// passed as null, it creates empty CLRModuleType.
        /// </summary>
        /// <param name="module">CLRDLLModule which imports this type</param>
        /// <param name="type">System.Type to be imported in DesignScript</param>
        /// <param name="alias">Alias name, if any. For now its not supported.</param>
        public static CLRModuleType GetInstance(Type type, CLRDLLModule module, string alias)
        {
            CLRModuleType mtype;
            if (!mTypes.TryGetValue(type, out mtype))
            {
                lock (mTypes)
                {
                    if (!mTypes.TryGetValue(type, out mtype))
                    {
                        mtype = new CLRModuleType(type);
                        //Now check that a type with same name is not imported.
                        Type otherType;
                        if (mTypeNames.TryGetValue(mtype.ClassName, out otherType))
                            throw new InvalidOperationException(string.Format("Can't import {0}, {1} is already imported as {2}, namespace support needed.", type.FullName, type.Name, otherType.FullName));

                        mTypes.Add(type, mtype);
                        mTypeNames.Add(mtype.ClassName, type);
                    }
                }
            }

            if (module != null && mtype.Module == null)
            {
                mtype.Module = module;
                if (type.IsEnum)
                    mtype.ClassNode = mtype.ParseEnumType(type, alias);
                else
                    mtype.ClassNode = mtype.ParseSystemType(type, alias);
            }

            return mtype;
        }

        /// <summary>
        /// Gets all the types, which was referenced by other types but were not
        /// imported explicitly. These are empty types and corresponding DS Type
        /// don't contain any methods, constructors, properties or fields
        /// </summary>
        /// <returns>List of CLRModuleType</returns>
        public static List<CLRModuleType> GetEmptyTypes()
        {
            return GetTypes(isEmpty);
        }

        /// <summary>
        /// Gets all the types for the given predicate.
        /// </summary>
        /// <param name="predicate">A delegate for defining criteria</param>
        /// <returns>List of CLRModuleType</returns>
        public static List<CLRModuleType> GetTypes(Func<CLRModuleType, bool> predicate)
        {
            List<CLRModuleType> types = new List<CLRModuleType>();
            foreach (var item in mTypes)
            {
                if (predicate(item.Value))
                    types.Add(item.Value);
            }

            return types;
        }

        /// <summary>
        /// Returns list of function pointers available on this type for a given
        /// function name
        /// </summary>
        /// <param name="name">Function name</param>
        /// <returns>List of FFIFunctionPointer</returns>
        public List<FFIFunctionPointer> GetFunctionPointers(string name)
        {
            List<FFIFunctionPointer> pointers = null;
            if (!mFunctionPointers.TryGetValue(name, out pointers))
            {
                pointers = new List<FFIFunctionPointer>();
                mFunctionPointers[name] = pointers;
            }
            return pointers;
        }

        /// <summary>
        /// Ensures that dispose method node is created for this empty type.
        /// </summary>
        /// <param name="module">Reference module</param>
        public void EnsureDisposeMethod(CLRDLLModule module)
        {
            foreach (var item in ClassNode.funclist)
            {
                if (item.Name == ProtoCore.DSDefinitions.Keyword.Dispose)
                    return; //Dispose method is already present.
            }
            bool resetModule = false;
            if (Module == null)
            {
                Module = module;
                resetModule = true;
            }
            AssociativeNode node = ParseMethod(mDisposeMethod);
            FunctionDefinitionNode func = node as FunctionDefinitionNode;
            if (func != null)
            {
                func.Name = ProtoCore.DSDefinitions.Keyword.Dispose;
                func.IsStatic = false;
                func.IsAutoGenerated = true;
                ClassNode.funclist.Add(func);
            }
            if (resetModule)
                Module = null;
        }

        public static MethodInfo DisposeMethod 
        {
            get
            {
                return mDisposeMethod;
            }
        }

        /// <summary>
        /// Imported ClassDeclNode
        /// </summary>
        public ClassDeclNode ClassNode { get; private set; }

        /// <summary>
        /// DesignScript Class name
        /// </summary>
        public string ClassName { get { return ClassNode.className; } }

        /// <summary>
        /// CLRDLLModule from which this type was imported.
        /// </summary>
        public CLRDLLModule Module { get; private set; }

        /// <summary>
        /// System.Type that was imported
        /// </summary>
        public Type CLRType { get; private set; }

        /// <summary>
        /// Imported ProtoCore.Type
        /// </summary>
        public ProtoCore.Type ProtoCoreType 
        { 
            get 
            { 
                if(null == mProtoCoreType)
                    mProtoCoreType = CLRObjectMarshler.GetUserDefinedType(CLRType);

                return mProtoCoreType.Value;
            } 
        }

        public static ProtoCore.Type GetProtoCoreType(Type type, CLRDLLModule module)
        {
            ProtoCore.Type protoCoreType;
            if (mTypeMaps.TryGetValue(type, out protoCoreType))
                return protoCoreType;

            if (type == typeof(object) || !CLRObjectMarshler.IsMarshaledAsNativeType(type))
            {
                if(type.IsEnum)
                    protoCoreType = CLRModuleType.GetInstance(type, module, string.Empty).ProtoCoreType;
                else
                    protoCoreType = CLRModuleType.GetInstance(type, null, string.Empty).ProtoCoreType;
            }
            else
                protoCoreType = CLRObjectMarshler.GetProtoCoreType(type);

            lock (mTypeMaps)
            {
                mTypeMaps[type] = protoCoreType;
            }
            return protoCoreType;
        }

        public static System.Type GetImportedType(string typename)
        {
            Type type = null;
            if (mTypeNames.TryGetValue(typename, out type))
                return type;

            return null;
        }
        #endregion

        #region PRIVATE_METHODS_AND_FIELDS

        private ProtoCore.Type? mProtoCoreType;

        readonly Dictionary<string, List<FFIFunctionPointer>> mFunctionPointers = new Dictionary<string, List<FFIFunctionPointer>>();

        private static readonly Dictionary<Type, CLRModuleType> mTypes = new Dictionary<Type, CLRModuleType>();

        private static readonly Dictionary<string, Type> mTypeNames = new Dictionary<string, Type>();

        private static readonly Dictionary<System.Type, ProtoCore.Type> mTypeMaps = new Dictionary<Type, ProtoCore.Type>();

        private Type GetBaseType(Type type)
        {
            Type b = type.BaseType;
            if (null != b && !IsBrowsable(b))
                return GetBaseType(b);

            return b;
        }

        private ClassDeclNode ParseEnumType(Type type, string alias)
        {
            Validity.Assert(type.IsEnum, "Non enum type is being imported as enum!!");

            string classname = alias;
            if (classname == null | classname == string.Empty)
                classname = CLRObjectMarshler.GetTypeName(type);

            ProtoCore.AST.AssociativeAST.ClassDeclNode classnode = CreateEmptyClassNode(classname);
            classnode.ExternLibName = Module.Name;
            classnode.className = classname;
            classnode.Name = type.Name;

            FieldInfo[] fields = type.GetFields();
            foreach (var f in fields)
            {
                if (f.FieldType != type)
                    continue;

                VarDeclNode variable = ParseFieldDeclaration(f);
                if (null == variable)
                    continue;
                variable.IsStatic = true;
                classnode.varlist.Add(variable);
                FunctionDefinitionNode func = ParseFieldAccessor(f);
                if (null != func)
                {
                    func.IsStatic = true;
                    RegisterFunctionPointer(func.Name, f, func.ReturnType);
                    classnode.funclist.Add(func);
                }
            }

            return classnode;
        }

        private ClassDeclNode ParseSystemType(Type type, string alias)
        {
            Validity.Assert(IsBrowsable(type), "Non browsable type is being imported!!");

            string classname = alias;
            if (classname == null | classname == string.Empty)
                classname = CLRObjectMarshler.GetTypeName(type);

            ProtoCore.AST.AssociativeAST.ClassDeclNode classnode = CreateEmptyClassNode(classname);
            classnode.ExternLibName = Module.Name;
            classnode.className = classname;
            classnode.Name = type.Name;
            Type baseType = GetBaseType(type);
            if (baseType != null && !CLRObjectMarshler.IsMarshaledAsNativeType(baseType))
            {
                string baseTypeName = CLRObjectMarshler.GetTypeName(baseType);

                classnode.superClass = new List<string>();
                classnode.superClass.Add(baseTypeName);
                //Make sure that base class is imported properly.
                CLRModuleType.GetInstance(baseType, Module, string.Empty);
            }

            ConstructorInfo[] ctors = type.GetConstructors();
            foreach (var c in ctors)
            {
                if (c.IsPublic && !c.IsGenericMethod && IsBrowsable(c))
                {
                    ConstructorDefinitionNode node = ParseConstructor(c, type);
                    classnode.funclist.Add(node);
                    RegisterFunctionPointer(node.Name, c, node.ReturnType);
                }
            }

            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;
            bool isDerivedClass = classnode.superClass != null;
            if (isDerivedClass) //has base class
                flags |= BindingFlags.DeclaredOnly; //for derived class, parse only class declared methods.

            bool isDisposable = typeof(IDisposable).IsAssignableFrom(type);
            MethodInfo[] methods = type.GetMethods(flags);
            bool hasDisposeMethod = false;
            foreach (var m in methods)
            {
                if (!IsBrowsable(m))
                    continue;

                //Don't include overriden methods or generic methods
                if (m.IsPublic && !m.IsGenericMethod && (m == m.GetBaseDefinition() || (m.GetBaseDefinition().DeclaringType == baseType && baseType == typeof(Object))))
                {
                    AssociativeNode node = ParseAndRegisterFunctionPointer(isDisposable, ref hasDisposeMethod, m);
                    classnode.funclist.Add(node);
                }
                else if (!hasDisposeMethod && isDisposable && baseType == typeof(Object) && isDisposeMethod(m))
                {
                    AssociativeNode node = ParseAndRegisterFunctionPointer(isDisposable, ref hasDisposeMethod, m);
                    classnode.funclist.Add(node);
                }
            }
            if (!hasDisposeMethod && !isDisposable)
            {
                AssociativeNode node = ParseAndRegisterFunctionPointer(true, ref hasDisposeMethod, mDisposeMethod);
                classnode.funclist.Add(node);
            }

            FieldInfo[] fields = type.GetFields();
            foreach (var f in fields)
            {
                if (!IsBrowsable(f))
                    continue;

                VarDeclNode variable = ParseFieldDeclaration(f);
                if (null == variable)
                    continue;
                classnode.varlist.Add(variable);
                FunctionDefinitionNode func = ParseFieldAccessor(f);
                if (null != func)
                    RegisterFunctionPointer(func.Name, f, func.ReturnType);
            }

            PropertyInfo[] properties = type.GetProperties(flags);
            foreach (var p in properties)
            {
                AssociativeNode node = ParseProperty(p);
                if(null != node)
                    classnode.varlist.Add(node);
            }

            return classnode;
        }

        private AssociativeNode ParseAndRegisterFunctionPointer(bool isDisposable, ref bool hasDisposeMethod, MethodInfo m)
        {
            AssociativeNode node = ParseMethod(m);
            FunctionDefinitionNode func = node as FunctionDefinitionNode;
            if (func != null)
            {
                //Rename the Dispose method to _Dispose, as required by DS.
                if (isDisposable && isDisposeMethod(m))
                {
                    hasDisposeMethod = true;
                    func.Name = ProtoCore.DSDefinitions.Keyword.Dispose;
                    func.IsStatic = false;
                    func.IsAutoGenerated = true;
                }

                RegisterFunctionPointer(func.Name, m, func.ReturnType);
            }
            else if (node is ConstructorDefinitionNode)
            {
                ConstructorDefinitionNode constr = node as ConstructorDefinitionNode;
                RegisterFunctionPointer(constr.Name, m, constr.ReturnType);
            }
            return node;
        }

        static readonly MethodInfo mDisposeMethod;
        private static void Dispose()
        {
            //Do nothing.
        }

        private static bool isEmpty(CLRModuleType type)
        {
            return null == type.Module;
        }

        private ClassDeclNode CreateEmptyClassNode(string classname)
        {
            ProtoCore.AST.AssociativeAST.ClassDeclNode classnode = new ProtoCore.AST.AssociativeAST.ClassDeclNode();
            classnode.className = classname;
            classnode.Name = null;
            classnode.IsImportedClass = true;

            classnode.IsExternLib = true;
            classnode.ExternLibName = null; //Dummy class.

            return classnode;
        }

        private bool isPropertyAccessor(MethodInfo m)
        {
            //The property accessor methods are compiler generated, hence is marked as special name.
            if (null == m || !m.IsSpecialName)
                return false;

            string name = m.Name;
            string propertyName;
            int nParams = 0;
            if (name.StartsWith("get_"))
                propertyName = name.Remove(0, 4);
            else if (name.StartsWith("set_"))
            {
                propertyName = name.Remove(0, 4);
                nParams = 1;
            }
            else
                return false;

            ParameterInfo[] indexParams = m.GetParameters();
            return (null == indexParams || indexParams.Length == nParams);
        }

        private bool isDisposeMethod(MethodInfo m)
        {
            ParameterInfo[] ps = m.GetParameters();
            if ((ps == null || ps.Length == 0) && m.Name == "Dispose")
                return true;
            return false;
        }

        private PropertyInfo GetProperty(ref Type type, string name)
        {
            PropertyInfo info = type.GetProperty(name);
            if (null != info)
                return info;
            if (type.BaseType != null)
                type = type.BaseType;
            else
                return null;

            return GetProperty(ref type, name);
        }

        private ProtoCore.AST.AssociativeAST.AssociativeNode ParseProperty(PropertyInfo p)
        {
            if (null == p || !IsBrowsable(p))
                return null;
            
            //Index properties are not parsed as property at this moment.
            ParameterInfo[] indexParams = p.GetIndexParameters();
            if (null != indexParams && indexParams.Length > 0)
                return null;

            MethodInfo m = p.GetAccessors(false)[0];
            //If this method hides the base class accessor method by signature
            if (m.IsHideBySig)
            {
                //Check if it has a base class
                Type baseType = p.DeclaringType.BaseType;
                PropertyInfo baseProp = (baseType != null) ? GetProperty(ref baseType, p.Name) : null;
                //If this property is also declared in base class, then no need to add this is derived class.
                if(null != baseProp && baseProp.DeclaringType != p.DeclaringType)
                {
                    //base class also has this method.
                    return null;
                }
            }
            
            ProtoCore.AST.AssociativeAST.VarDeclNode varDeclNode = ParseArgumentDeclaration(p.Name, p.PropertyType);
            if(null != varDeclNode)
                varDeclNode.IsStatic = m.IsStatic;
            return varDeclNode;
        }

        public static bool IsBrowsable(MemberInfo member)
        {
            if (null == member)
                return false;

            object[] atts = member.GetCustomAttributes(false);
            foreach (var item in atts)
            {
                BrowsableAttribute browsable = item as BrowsableAttribute;
                if (null != browsable)
                    return browsable.Browsable;
            }

            return true;
        }

        private bool AllowsRankReduction(MethodInfo method)
        {
            object[] atts = method.GetCustomAttributes(false);
            foreach (var item in atts)
            {
                if (item is AllowRankReductionAttribute)
                    return true;
            }

            return false;
        }

        private ProtoCore.AST.AssociativeAST.VarDeclNode ParseFieldDeclaration(FieldInfo f)
        {
            if (null == f || !IsBrowsable(f))
                return null;

            ProtoCore.AST.AssociativeAST.VarDeclNode varDeclNode = ParseArgumentDeclaration(f.Name, f.FieldType);
            //TODO: temporary limitation, can't have variable name matching with class name.
            if (null != CLRModuleType.GetImportedType(f.Name))
                return null;
            if (null != varDeclNode)
                varDeclNode.IsStatic = f.IsStatic;
            return varDeclNode;
        }

        private ProtoCore.AST.AssociativeAST.FunctionDefinitionNode ParseFieldAccessor(FieldInfo f)
        {
            if (null == f || !IsBrowsable(f))
                return null;

            ProtoCore.AST.AssociativeAST.FunctionDefinitionNode func = new ProtoCore.AST.AssociativeAST.FunctionDefinitionNode();
            func.Name = string.Format("%get_{0}", f.Name);
            func.Pattern = null;
            func.Singnature = new ProtoCore.AST.AssociativeAST.ArgumentSignatureNode();
            func.ReturnType = CLRModuleType.GetProtoCoreType(f.FieldType, Module);
            func.FunctionBody = null;
            func.access = ProtoCore.DSASM.AccessSpecifier.kPublic;
            func.IsDNI = false;
            func.IsExternLib = true;
            func.ExternLibName = Module.Name;
            func.IsStatic = f.IsStatic;

            return func;
        }

        private ProtoCore.AST.AssociativeAST.AssociativeNode ParseMethod(MethodInfo method)
        {
            ProtoCore.Type retype = CLRModuleType.GetProtoCoreType(method.ReturnType, Module);
            bool propaccessor = isPropertyAccessor(method);
            if (method.IsStatic && method.DeclaringType == method.ReturnType && !propaccessor)
            {
                //case for named constructor. Must return a pointer type
                if (!Object.Equals(method.ReturnType, CLRType))
                    throw new InvalidOperationException("Unexpected type for constructor {0D28FC00-F8F4-4049-AD1F-BBC34A68073F}");

                retype = ProtoCoreType;
                return ParsedNamedConstructor(method, method.Name, retype);
            }

            //Need to hide property accessor from design script users, prefix with %
            string prefix = propaccessor ? "%" : "";
            ProtoCore.AST.AssociativeAST.FunctionDefinitionNode func = new ProtoCore.AST.AssociativeAST.FunctionDefinitionNode();
            func.Name = string.Format("{0}{1}", prefix, method.Name);
            func.Pattern = null;
            func.Singnature = ParseArgumentSignature(method);
            if (retype.IsIndexable && AllowsRankReduction(method))
                retype.rank = -1;
            func.ReturnType = retype;
            func.FunctionBody = null;
            func.access = ProtoCore.DSASM.AccessSpecifier.kPublic;
            func.IsDNI = false;
            func.IsExternLib = true;
            func.ExternLibName = Module.Name;
            func.IsStatic = method.IsStatic;

            return func;
        }

        private void RegisterFunctionPointer(string functionName, MemberInfo method, ProtoCore.Type retype)
        {
            List<FFIFunctionPointer> pointers = GetFunctionPointers(functionName);
            FFIFunctionPointer f = null;
            if (functionName == ProtoCore.DSDefinitions.Keyword.Dispose)
                f = new DisposeFunctionPointer(Module, method, retype);
            else if (CoreUtils.IsGetter(functionName))
                f = new GetterFunctionPointer(Module, functionName, method, retype);
            else
                f = new CLRFFIFunctionPointer(Module, functionName, method, null, retype);

            if (!pointers.Contains(f))
                pointers.Add(f);
        }

        private ProtoCore.AST.AssociativeAST.ConstructorDefinitionNode ParseConstructor(ConstructorInfo c, System.Type type)
        {
            //Constructors should always return user defined type object, hence it should be pointer type.
            ProtoCore.Type selfType = ProtoCoreType;

            ProtoCore.AST.AssociativeAST.ConstructorDefinitionNode constr = ParsedNamedConstructor(c, type.Name, selfType);
            return constr;
        }

        private ProtoCore.AST.AssociativeAST.ConstructorDefinitionNode ParsedNamedConstructor(MethodBase method, string constructorName, ProtoCore.Type returnType)
        {
            ProtoCore.AST.AssociativeAST.ConstructorDefinitionNode constr = new ProtoCore.AST.AssociativeAST.ConstructorDefinitionNode();
            constr.Name = constructorName;
            constr.Pattern = null;
            constr.Signature = ParseArgumentSignature(method);
            constr.ReturnType = returnType;
            constr.FunctionBody = null;
            constr.access = ProtoCore.DSASM.AccessSpecifier.kPublic;
            constr.IsExternLib = true;
            constr.ExternLibName = Module.Name;

            return constr;
        }

        private ProtoCore.AST.AssociativeAST.ArgumentSignatureNode ParseArgumentSignature(MethodBase method)
        {
            ProtoCore.AST.AssociativeAST.ArgumentSignatureNode argumentSignature = new ProtoCore.AST.AssociativeAST.ArgumentSignatureNode();
            ParameterInfo[] parameters = method.GetParameters();
            foreach (var parameter in parameters)
                argumentSignature.AddArgument(ParseArgumentDeclaration(parameter.Name, parameter.ParameterType));

            return argumentSignature;
        }

        private ProtoCore.AST.AssociativeAST.VarDeclNode ParseArgumentDeclaration(string parameterName, Type parameterType)
        {
            ProtoCore.AST.AssociativeAST.VarDeclNode varDeclNode = new ProtoCore.AST.AssociativeAST.VarDeclNode();
            varDeclNode.memregion = ProtoCore.DSASM.MemoryRegion.kMemStack;
            varDeclNode.access = ProtoCore.DSASM.AccessSpecifier.kPublic;

            ProtoCore.AST.AssociativeAST.IdentifierNode identifierNode = new ProtoCore.AST.AssociativeAST.IdentifierNode
                                                                             {
                Value = parameterName,
                Name = parameterName,
                datatype = new ProtoCore.Type()
                {
                    Name = "var",
                    IsIndexable = false,
                    rank = 0,
                    UID = (int)ProtoCore.PrimitiveType.kTypeVar
                }
            };
            //Lets emit native DS type object
            ProtoCore.Type argtype = CLRModuleType.GetProtoCoreType(parameterType, Module);

            varDeclNode.NameNode = identifierNode;
            varDeclNode.ArgumentType = argtype;
            return varDeclNode;
        }

        #endregion

        #region INTERNAL_METHODS
        
        /// <summary>
        /// This method is for testing, to ensure cache is cleared before every test.
        /// </summary>
        public static void ClearTypes()
        {
            lock (mTypes)
            {
                mTypes.Clear();
                mTypeNames.Clear();
                mTypeMaps.Clear();
            }
        }

        #endregion
    }

    /// <summary>
    /// Implements DLLModule for CLR types and FFI. This class supports .NET
    /// module import to DesignScript and provides FFIFunctionPointer &
    /// FFIObjectMarshler.
    /// </summary>
    public class CLRDLLModule : DLLModule
    {
        readonly Dictionary<string, CLRModuleType> mTypes = new Dictionary<string,CLRModuleType>();
        public string Name
        {
            get;
            private set;
        }

        public Module Module
        {
            get;
            private set;
        }

        public Assembly Assembly { get; private set; }

        public CLRDLLModule(string name, Assembly assembly)
        {
            Name = name;
            Assembly = assembly;
        }

        public CLRDLLModule(string name, Module module)
        {
            Name = name;
            Module = module;
        }

        //this is incomplete todo: implement
        public override List<FFIFunctionPointer> GetFunctionPointers(string className, string name)
        {
            CLRModuleType type = null;
            if (mTypes.TryGetValue(className, out type))
                return type.GetFunctionPointers(name);

            if (name == ProtoCore.DSDefinitions.Keyword.Dispose)
            {
                List<FFIFunctionPointer> pointers = new List<FFIFunctionPointer>();
                pointers.Add( new DisposeFunctionPointer(this, CLRModuleType.DisposeMethod, CLRModuleType.GetProtoCoreType(CLRModuleType.DisposeMethod.ReturnType, this)));
                return pointers;
            }

            throw new KeyNotFoundException(string.Format("Function definition for {0}.{1}, not found", className, name));
        }

        private bool ClassFilter(Type type, object criteria)
        {
            if (type.FullName == (string)criteria)
                return true;

            return type.Name == (string)criteria;
        }

        public override FFIFunctionPointer GetFunctionPointer(string className, string name, List<ProtoCore.Type> argTypes, ProtoCore.Type returnType)
        {
            List<FFIFunctionPointer> pointers = GetFunctionPointers(className, name);
            if (null == pointers)
                return null;

            foreach (var ptr in pointers)
            {
                CLRFFIFunctionPointer clrPtr = ptr as CLRFFIFunctionPointer;
                if (clrPtr.Contains(name, argTypes, returnType))
                    return clrPtr;
            }

            return null;
        }

        public override CodeBlockNode ImportCodeBlock(string typeName, string alias, CodeBlockNode refNode)
        {
            Type[] types = GetTypes(typeName);
            Type exttype = typeof(IExtensionApplication);
#if PARALLEL
            System.Threading.Tasks.Parallel.ForEach(types, type =>
            {
                //For now there is no support for generic type.
                if (!type.IsGenericType && type.IsPublic && !exttype.IsAssignableFrom(type) && CLRModuleType.IsBrowsable(type))
                {
                    CLRModuleType importedType = CLRModuleType.GetInstance(type, this, alias);
                }
            });
#else
            foreach (var type in types)
            {
                //For now there is no support for generic type.
                if (!type.IsGenericType && type.IsPublic && !exttype.IsAssignableFrom(type) && CLRModuleType.IsBrowsable(type))
                {
                    CLRModuleType importedType = CLRModuleType.GetInstance(type, this, alias);
                    Type[] nestedTypes = type.GetNestedTypes();
                    if (null != nestedTypes && nestedTypes.Length > 0)
                    {
                        foreach (var item in nestedTypes)
                        {
                            importedType = CLRModuleType.GetInstance(item, this, string.Empty);
                        }
                    }
                }
            }
#endif

            CodeBlockNode node = new CodeBlockNode();
            //Get all the types available on this module.
            //TODO: need to optimize for performance.
            List<CLRModuleType> moduleTypes = CLRModuleType.GetTypes((CLRModuleType mtype) => { return mtype.Module == this; });
            foreach (var item in moduleTypes)
            {
                node.Body.Add(item.ClassNode);
                mTypes[item.ClassName] = item; //update Type dictionary.
            }

            //Also add all the available empty class nodes.
            List<CLRModuleType> emptyTypes = CLRModuleType.GetEmptyTypes();
            foreach (var item in emptyTypes)
            {
                item.EnsureDisposeMethod(this);
                node.Body.Add(item.ClassNode);
            }

            string ffidump = Environment.GetEnvironmentVariable("FFIDUMP");
            if (string.Compare(ffidump, "1") == 0)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                foreach (var item in node.Body)
                {
                    sb.Append(item.ToString());
                    sb.AppendLine();
                }
                using (System.IO.FileStream fs = new System.IO.FileStream(string.Format("{0}.ds", this.Name), System.IO.FileMode.Create))
                {
                    byte[] bytes = System.Text.Encoding.ASCII.GetBytes(sb.ToString());
                    fs.Write(bytes, 0, bytes.Length);
                }
            }

            return node;
        }

        public static System.Type GetImplemetationType(Assembly assembly, Type interfaceType, Type assemblyAttribute, bool searchFromAllExportedType)
        {
            if (null == assembly)
                return null;

            object[] attributes = assembly.GetCustomAttributes(assemblyAttribute, true);
            if (null != attributes && attributes.Length > 0 && assemblyAttribute.IsAssignableFrom(attributes[0].GetType()))
            {
                PropertyInfo prop = assemblyAttribute.GetProperty("Type");
                Type t = prop.GetValue(attributes[0], null) as Type;
                if (null != t && interfaceType.IsAssignableFrom(t))
                    return t;
            }

            if (!searchFromAllExportedType)
                return null;

            //Couldn't get interfaceType via assemblyAttribute, 
            //iterate  through all exported types and checkif there is a 
            //interfaceType implementation.
            //
            Type[] types = assembly.GetExportedTypes();
            foreach (var item in types)
            {
                if (!item.IsAbstract && !item.IsInterface && interfaceType.IsAssignableFrom(item))
                    return item;
            }

            return null;
        }

        public override Type GetExtensionAppType()
        {
            Type extensionAppType = typeof(IExtensionApplication);
            Type assemblyAttribute = typeof(Autodesk.DesignScript.Runtime.ExtensionApplicationAttribute);
            return GetImplemetationType((null != Module) ? Module.Assembly : Assembly,
                extensionAppType, assemblyAttribute, false);
        }

        private static void SetOption(Type configType, string setting, object value)
        {
            try
            {
                PropertyInfo prop = configType.GetProperty(setting);
                if (null == prop)
                    return;

                MethodInfo m = prop.GetSetMethod(true);
                if(null != m)
                    m.Invoke(null, new object[] { value });
            }
            catch (System.Exception)
            {
            }
        }

        private Type GetConfigurationType()
        {
            Type[] types = GetTypes(string.Empty);
            foreach (var item in types)
            {
                if ("Configuration" == CLRObjectMarshler.GetCategory(item))
                    return item;
            }
            return null;
        }

        private Type[] GetTypes(string typeName)
        {
            Type[] types = null;
            if (typeName == null || typeName == string.Empty)
            {
                if (Module == null)
                    types = Assembly.GetExportedTypes();
                else
                    types = Module.GetTypes();
            }
            else
            {
                if (Module == null)
                {
                    types = new Type[1];
                    types[0] = Assembly.GetType(typeName);
                }
                else
                {
                    TypeFilter myFilter = ClassFilter;
                    types = Module.FindTypes(myFilter, typeName);
                }

                System.Diagnostics.Debug.Assert(types.Length == 1, "More than one specified type found in the module.");
            }
            return types;
        }

        public override FFIObjectMarshler GetMarshaller(ProtoCore.Core core)
        {
            return CLRObjectMarshler.GetInstance(core);
        }
    }

    /// <summary>
    /// Helper class to load CLR modules.
    /// </summary>
    public class CSModuleHelper : ModuleHelper
    {
        readonly Dictionary<String, CLRDLLModule> mModules = new Dictionary<string, CLRDLLModule>(StringComparer.CurrentCultureIgnoreCase);

        /// <summary>
        /// Returns a CLRDLLModule after loading the given assembly.
        /// </summary>
        /// <param name="name">Name of assembly.</param>
        /// <returns>CLRDLLModule for given assembly/module name.</returns>
        public override DLLModule getModule(String name)
        {
            CLRDLLModule module = null;
            if (!mModules.TryGetValue(name, out module))
            {
                //see if it is a c# dll or native dll and create correct appropriate module and then query the module for function pointers.
                string extension = System.IO.Path.GetExtension(name);
                string filename = System.IO.Path.GetFileName(name);

                bool isDLL = string.Compare(extension, ".dll", StringComparison.OrdinalIgnoreCase) == 0;
                try
                {
                    Assembly theAssembly = FFIExecutionManager.Instance.LoadAssembly(name);
                    Module testDll = theAssembly.GetModule(filename);
                    if (testDll == null)
                        module = new CLRDLLModule(filename, theAssembly);
                    else
                        module = new CLRDLLModule(filename, testDll);
                    lock (mModules)
                    {
                        mModules.Add(name, module);
                    }
                }
                catch (System.Exception exception)
                {
                    // If the exception is having HRESULT of 0x80131515, then perhaps we need to instruct the user to "unblock" the downloaded DLL. Please seee the following link for details:
                    // http://blogs.msdn.com/b/brada/archive/2009/12/11/visual-studio-project-sample-loading-error-assembly-could-not-be-loaded-and-will-be-ignored-could-not-load-file-or-assembly-or-one-of-its-dependencies-operation-is-not-supported-exception-from-hresult-0x80131515.aspx
                    // 
                    System.Diagnostics.Debug.WriteLine(exception.Message);
                    System.Diagnostics.Debug.WriteLine(exception.StackTrace);
                    throw new System.Exception(string.Format("Fail to load file: {0}.", name));
                }
            }

            return module;
        }

        public override FFIObjectMarshler GetMarshaller(ProtoCore.Core core)
        {
            return CLRObjectMarshler.GetInstance(core);
        }
    }
}
