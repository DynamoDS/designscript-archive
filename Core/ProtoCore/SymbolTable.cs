using System.Collections.Generic;
using System.Linq;

namespace ProtoCore.DSASM
{ 
    /// <summary>
    /// Extension to the normal Dictionary. This class can store more than one value for every key. It keeps a HashSet for every Key value.
    /// Calling Add with the same Key and multiple values will store each value under the same Key in the Dictionary. Obtaining the values
    /// for a Key will return the HashSet with the Values of the Key. 
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public class MultiValueDictionary<TKey, TValue> : Dictionary<TKey, HashSet<TValue>>
    {
        /// <summary>
        /// Adds the specified value under the specified key
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Add(TKey key, TValue value)
        {
            //ArgumentVerifier.CantBeNull(key, "key");

            HashSet<TValue> container = null;
            if (!TryGetValue(key, out container))
            {
                container = new HashSet<TValue>();
                base.Add(key, container);
            }
            container.Add(value);
        }


        /// <summary>
        /// Determines whether this dictionary contains the specified value for the specified key 
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>true if the value is stored for the specified key in this dictionary, false otherwise</returns>
        public bool ContainsValue(TKey key, TValue value)
        {
            //ArgumentVerifier.CantBeNull(key, "key");
            bool toReturn = false;
            HashSet<TValue> values = null;
            if (TryGetValue(key, out values))
            {
                toReturn = values.Contains(value);
            }
            return toReturn;
        }


        /// <summary>
        /// Removes the specified value for the specified key. It will leave the key in the dictionary.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Remove(TKey key, TValue value)
        {
            //ArgumentVerifier.CantBeNull(key, "key");

            HashSet<TValue> container = null;
            if (TryGetValue(key, out container))
            {
                container.Remove(value);
                if (container.Count <= 0)
                {
                    Remove(key);
                }
            }
        }


        /// <summary>
        /// Merges the specified multivaluedictionary into this instance.
        /// </summary>
        /// <param name="toMergeWith">To merge with.</param>
        public void Merge(MultiValueDictionary<TKey, TValue> toMergeWith)
        {
            if (toMergeWith == null)
            {
                return;
            }

            foreach (KeyValuePair<TKey, HashSet<TValue>> pair in toMergeWith)
            {
                foreach (TValue value in pair.Value)
                {
                    Add(pair.Key, value);
                }
            }
        }


        /// <summary>
        /// Gets the values for the key specified. This method is useful if you want to avoid an exception for key value retrieval and you can't use TryGetValue
        /// (e.g. in lambdas)
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="returnEmptySet">if set to true and the key isn't found, an empty hashset is returned, otherwise, if the key isn't found, null is returned</param>
        /// <returns>
        /// This method will return null (or an empty set if returnEmptySet is true) if the key wasn't found, or
        /// the values if key was found.
        /// </returns>
        public HashSet<TValue> GetValues(TKey key, bool returnEmptySet)
        {
            HashSet<TValue> toReturn = null;
            if (!base.TryGetValue(key, out toReturn) && returnEmptySet)
            {
                toReturn = new HashSet<TValue>();
            }
            return toReturn;
        }
    }

    [System.Diagnostics.DebuggerDisplay("{name}, fi = {functionIndex}, ci = {classScope}, block = {runtimeTableIndex}")]
    public class SymbolNode
    {
        public string           name;
        public string           forArrayName;
        public int              index;
        public int              heapIndex;
        public int              classScope;
        public int              functionIndex;
        public int              absoluteClassScope;
        public int              absoluteFunctionIndex;
        public ProtoCore.Type   datatype;
        public ProtoCore.Type   staticType;
        
        public bool             isArgument;
        public bool             isTemp;
        public int              size;
        public int              datasize;
        public bool             isArray;
        public List<int>        arraySizeList;
        public MemoryRegion     memregion;
        public int              symbolTableIndex = ProtoCore.DSASM.Constants.kInvalidIndex;
        public int              runtimeTableIndex = ProtoCore.DSASM.Constants.kInvalidIndex;
        public AccessSpecifier  access;
        public bool isStatic;
        public List<AttributeEntry> Attributes { get; set; }
        public int codeBlockId = ProtoCore.DSASM.Constants.kInvalidIndex;
        public string ExternLib = "";

        public SymbolNode()
        {
            isArray         = false;
            arraySizeList   = null;
            memregion       = MemoryRegion.kInvalidRegion;
            classScope      = ProtoCore.DSASM.Constants.kInvalidIndex;
            functionIndex   = ProtoCore.DSASM.Constants.kGlobalScope;
            absoluteClassScope = ProtoCore.DSASM.Constants.kGlobalScope;
            absoluteFunctionIndex = ProtoCore.DSASM.Constants.kGlobalScope;
            isStatic        = false;
            isTemp          = false;
        }

        public SymbolNode(
            string name,
            int index, 
            int heapIndex, 
            int functionIndex,
            ProtoCore.Type datatype,
            ProtoCore.Type enforcedType,
            int size,
            int datasize, 
            bool isArgument, 
            int runtimeIndex,
            MemoryRegion memregion = MemoryRegion.kInvalidRegion, 
            bool isArray = false, 
            List<int> arraySizeList = null, 
            int scope = -1,
            AccessSpecifier access = ProtoCore.DSASM.AccessSpecifier.kPublic,
            bool isStatic = false,
            int codeBlockId = ProtoCore.DSASM.Constants.kInvalidIndex)
        {
            this.name           = name;
            isTemp         = name.StartsWith("%");
            this.index          = index;
            this.functionIndex = functionIndex;
            this.absoluteFunctionIndex = functionIndex;
            this.datatype       = datatype;
            this.staticType   = enforcedType;
            this.size           = size;
            this.datasize       = datasize;
            this.isArgument     = isArgument;
            this.arraySizeList  = arraySizeList;
            this.memregion      = memregion;
            this.classScope     = scope;
            this.absoluteClassScope = scope;
            runtimeTableIndex = runtimeIndex;
            this.access = access;
            this.isStatic = isStatic;
            this.codeBlockId = codeBlockId;
        }

        public SymbolNode(
            string name,
            string forArrayName,
            int index,
            int heapIndex,
            int functionIndex,
            ProtoCore.Type datatype,
            ProtoCore.Type enforcedType,
            int size,
            int datasize,
            bool isArgument,
            int runtimeIndex,
            MemoryRegion memregion = MemoryRegion.kInvalidRegion,
            bool isArray = false,
            List<int> arraySizeList = null,
            int scope = -1,
            AccessSpecifier access = ProtoCore.DSASM.AccessSpecifier.kPublic,
            bool isStatic = false,
            int codeBlockId = ProtoCore.DSASM.Constants.kInvalidIndex)
        {
            this.name = name;
            isTemp = name.StartsWith("%");
            this.index = index;
            this.functionIndex = functionIndex;
            this.absoluteFunctionIndex = functionIndex;
            this.datatype = datatype;
            this.staticType = enforcedType;
            this.size = size;
            this.datasize = datasize;
            this.isArgument = isArgument;
            this.arraySizeList = arraySizeList;
            this.memregion = memregion;
            this.classScope = scope;
            this.absoluteClassScope = scope;
            runtimeTableIndex = runtimeIndex;
            this.access = access;
            this.isStatic = isStatic;
            this.codeBlockId = codeBlockId;
            this.forArrayName = forArrayName;
        }

        public bool IsEqual(SymbolNode rhs)
        {
            return functionIndex == rhs.functionIndex && name == rhs.name;
        }

        public bool IsEqualAtScope(SymbolNode rhs)
        {
            return functionIndex == rhs.functionIndex && name == rhs.name && classScope == rhs.classScope && codeBlockId == rhs.codeBlockId;
        }



        public void SetStaticType(ProtoCore.Type newtype)
        {
            if (staticType.Equals(newtype))
            {
                return;
            }

            staticType = newtype;
            if (staticType.UID != (int)PrimitiveType.kTypeVar || staticType.rank != 0)
            {
                datatype = staticType;
            }
        }
    }

    public class SymbolTable
    {
        private int size;
        public string name { get; set; }
        //public SymbolTable parent{ get; set; }
        //public List<SymbolTable> children { get; set; }
        public int runtimeIndex { get; private set; }

        public Dictionary<int,SymbolNode> symbolList { get; set; }
        private MultiValueDictionary<string, SymbolNode> nameToSymbolList { get; set; }

        public SymbolTable(string scopename, int runtimeindex)
        {
            size = 0;
            name = scopename;
            //parent = null;
            //children = new List<SymbolTable>();
            symbolList = new Dictionary<int, SymbolNode>();
            nameToSymbolList = new MultiValueDictionary<string, SymbolNode>();
            runtimeIndex = runtimeindex;
        }

        public int GetGlobalSize()
        {
            //// Retrieves the size of this table and all its children
            //Debug.Assert(children.Count() >= 0);
            //if (0 == children.Count())
            //{
            //    return size;
            //}

            //foreach (SymbolTable table in children)
            //{
            //    size += table.GetGlobalSize();
            //}
            return size;
        }

        // TODO Jun: return failed status
        public int Append( SymbolNode node )
        {
            if (IndexOf(node) == ProtoCore.DSASM.Constants.kInvalidIndex)
            {   
                //@SARANG -- make a common method out of this
                symbolList[symbolList.Count] = node;
                node.symbolTableIndex = symbolList.Count - 1;
                nameToSymbolList.Add(node.name, node);

                if (ProtoCore.DSASM.Constants.kGlobalScope == node.functionIndex)
                {
                    size += node.size;
                }
                return node.symbolTableIndex;
            }
            return ProtoCore.DSASM.Constants.kInvalidIndex;
        }

        public bool Remove(SymbolNode node)
        {
            bool rtBool = true;
            rtBool = rtBool & nameToSymbolList.Remove(node.name);
            rtBool = rtBool & symbolList.Remove(node.symbolTableIndex);
            return rtBool;
        }

        public void RemoveGlobalSymbols()
        {
            List<int> keysToRemove = new List<int>();
            foreach (var symbol in symbolList)
            {
                bool removeGlobalsFromCurrentScope = symbol.Value.functionIndex == ProtoCore.DSASM.Constants.kInvalidIndex && 
                    symbol.Value.classScope == ProtoCore.DSASM.Constants.kInvalidIndex && 
                    string.IsNullOrEmpty(symbol.Value.ExternLib);
                if (removeGlobalsFromCurrentScope)
                {
                    keysToRemove.Add(symbol.Key);
                }
            }
            foreach (var key in keysToRemove)
            {
                SymbolNode symbol = symbolList[key];
                if (nameToSymbolList.ContainsValue(symbol.name, symbol))
                {
                    nameToSymbolList.Remove(symbol.name, symbol);
                }
                symbolList.Remove(key);
            }
        }

        public void RemoveGlobalSymbols(string symbolName)
        {
            foreach (var symbol in symbolList.Where(kvp => kvp.Value.name == symbolName).ToList())
            {
                bool removeGlobalsFromCurrentScope = symbol.Value.functionIndex == ProtoCore.DSASM.Constants.kInvalidIndex &&
                    symbol.Value.classScope == ProtoCore.DSASM.Constants.kInvalidIndex && string.IsNullOrEmpty(symbol.Value.ExternLib);
                if (removeGlobalsFromCurrentScope)
                {
                    nameToSymbolList.Remove(symbolName, symbol.Value);                    
                    symbolList.Remove(symbol.Key);
                        
                }
            }
            
        }

        public void AppendTable(SymbolTable symbols)
        {
            foreach (SymbolNode node in symbols.symbolList.Values)
            {
                Append(node);
            }
        }

        public HashSet<SymbolNode> GetNodeForName(string name)
        {
            if (nameToSymbolList.ContainsKey(name))
            {
                return nameToSymbolList[name];
            }
            return null;
        }

        public bool DoesSymbolExist(SymbolNode symbol)
        {
            return IndexOf(symbol) != ProtoCore.DSASM.Constants.kInvalidIndex;
        }

        public int IndexOf(SymbolNode symbol)
        {
            return symbol.symbolTableIndex;
        }

        public int IndexOf(string name)
        {
            if (!nameToSymbolList.ContainsKey(name))
            {
                return ProtoCore.DSASM.Constants.kInvalidIndex;
            }
            else
            {
                //@TODO: fix this logic to be more predictable
                //
                var symbol = nameToSymbolList[name];
                int idx = -1;
                foreach( var symb in symbol)
                {
                    idx = symb.symbolTableIndex;
                    break;
                }
                return idx;
            }
        }

        public int IndexOf(string name, int classScope, int functionindex)
        {
            var symbols = GetNodeForName(name);
            if (symbols != null)
            {
                foreach (var symbol in symbols)
                {
                    if (classScope == symbol.classScope && functionindex == symbol.functionIndex)
                    {
                        return symbol.symbolTableIndex;
                    }
                }
            }
            return ProtoCore.DSASM.Constants.kInvalidIndex;
        }

        public int IndexOfClass(string name, int classScope, int functionindex)
        {
            var symbols = GetNodeForName(name);
            if (symbols != null)
            {
                foreach (var symbol in symbols)
                {
                    if (symbol.functionIndex == -1)
                    {
                        return symbol.symbolTableIndex;
                    }
                    else if (classScope == symbol.classScope && functionindex == symbol.functionIndex)
                    {
                        return symbol.symbolTableIndex;
                    }
                }
            }
            return ProtoCore.DSASM.Constants.kInvalidIndex;
        }

        public int IndexOf(string name, int classScope)
        {
            var symbols = GetNodeForName(name);
            if (symbols != null)
            {
                foreach (var symbol in symbols)
                {
                    if (classScope == symbol.classScope)
                    {
                        return symbol.symbolTableIndex;
                    }
                }
            }
            return ProtoCore.DSASM.Constants.kInvalidIndex;
        }

        public void SetDataType(string ident, int functionIndex, ProtoCore.Type type)
        {
            foreach(SymbolNode node in symbolList.Values)
            {
                if (node.functionIndex == functionIndex && node.name == ident)
                {
                    node.datatype = type;
                    return;
                }
            }
        }
    }
}
