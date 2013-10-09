using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Autodesk.DesignScript.Runtime
{
    /// <summary>
    /// This attribute is used to specify the Type of class that implements
    /// IExtensionApplication interface in the specified assembly. This 
    /// attribute can be used only once at assembly level. Having this attribute
    /// saves the cost of reflection on each exported types to find the type
    /// that implements IExtensionApplication interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public sealed class ExtensionApplicationAttribute : Attribute
    {
        public ExtensionApplicationAttribute(Type entryPointType)
        {
            Type = entryPointType;
        }

        public Type Type { get; private set; }
    }

    /// <summary>
    /// This attribute is used to specify the Type of class that implements
    /// IGraphicDataProvider interface in the specified assembly. This 
    /// attribute can be used only once at assembly level. Having this attribute
    /// saves the cost of reflection on each exported types to find the type
    /// that implements IGraphicDataProvider interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public sealed class GraphicDataProviderAttribute : Attribute
    {
        public GraphicDataProviderAttribute(Type providerType)
        {
            Type = providerType;
        }

        public Type Type { get; private set; }
    }
    
    /// <summary>
    /// This attribute is used to specify the Type of class that implements
    /// IContextDataProvider interface in the specified assembly. This 
    /// attribute can be used only once at assembly level. Having this attribute
    /// saves the cost of reflection on each exported types to find the type
    /// that implements IContextDataProvider interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public sealed class ContextDataProviderAttribute : Attribute
    {
        private Func<bool> mCapturesData = () => true;

        /// <summary>
        /// Constructor to construct this attribute with a delegate to check
        /// whether this data provider captures data.
        /// </summary>
        /// <param name="dataProviderType">Type that implements 
        /// IContextDataProvider interface</param>
        /// <param name="capturesData">Delegate to check if the provider can
        /// capture data</param>
        public ContextDataProviderAttribute(Type dataProviderType, Func<bool> capturesData)
        {
            Type = dataProviderType;
            if(null != capturesData)
                mCapturesData = capturesData;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProviderType">Type that implements 
        /// IContextDataProvider interface</param>
        public ContextDataProviderAttribute(Type dataProviderType)
        {
            Type = dataProviderType;
        }

        /// <summary>
        /// Type implementing IContextDataProvider interface.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// Checks if this type can capture data interactively
        /// </summary>
        public bool CapturesData 
        {
            get { return mCapturesData(); }
        }
    }

    /// <summary>
    /// This attribute can be applied to methods that return collection of
    /// objects, but with some combination of input parameters it returns a 
    /// collection of single object and at designscript side we want the method
    /// to return a single object instead of a collection of single object.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class AllowRankReductionAttribute : Attribute
    {
        /// <summary>
        /// Utility method to get the single object from the collection of 
        /// single object. If the input object is neither a collection nor a
        /// collection of single object, this method returns the input object.
        /// </summary>
        /// <param name="collection">Input object to be converted to singleton.
        /// </param>
        /// <returns>An object from the collection of single object or the 
        /// input object.</returns>
        /// 
        public object ReduceRank(object collection)
        {
            if (null == collection)
                return null;

            Type type = collection.GetType();
            if (type.IsArray)
            {
                Array arr = collection as Array;
                if (null != arr && arr.Length == 1)
                    return arr.GetValue(0);
            }
            else if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                IEnumerable arr = collection as IEnumerable;
                if (null != arr)
                {
                    int count = 0;
                    object first = null;
                    foreach (var item in arr)
                    {
                        ++count;
                        if (count <= 1)
                            first = item;
                        else if (count > 1)
                            break;
                    }
                    if (count == 1)
                        return first;
                }
            }

            return collection;
        }

        /// <summary>
        /// Checks if the input object is a collection of single object.
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static bool IsRankReducible(object collection)
        {
            if (null == collection)
                return false;

            Type type = collection.GetType();
            if (type.IsArray)
            {
                Array arr = collection as Array;
                if (null != arr && arr.Length == 1)
                    return true;
            }
            else if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                IEnumerable arr = collection as IEnumerable;
                if (null != arr)
                {
                    int count = 0;
                    foreach (var item in arr)
                    {
                        ++count;
                        if (count > 1)
                            return false;
                    }
                    if (count == 1)
                        return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// This attribute can be applied to method which returns a dictionary. It
    /// describles key and the corresponding DesignScript type of value, so that
    /// DesignScript runtime can return values from dictionary.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class MultiReturnAttribute: Attribute
    {
        public string Name { get; set;}
        public string Type { get; set;}

        public MultiReturnAttribute(string name, string type)
        {
            Name = name;
            Type = type;
        }
    }

    /// <summary>
    /// This attribute can be applied to method which requires some runtime 
    /// support from DesignScript, e.g., tracing. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RuntimeRequirementAttribute: Attribute
    {
        public bool RequireTracing { get; set; }
    }

    /// <summary>
    /// This attribute can be applied to methods that register callsite with
    /// trace mechanism.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RegisterForTraceAttribute: Attribute { }
}
