using System;
using System.Collections.Generic;
using System.Linq;
using ProtoCore.DSASM;
using ProtoCore.Exceptions;

namespace ProtoCore.Utils
{
    public static class ArrayUtils
    {
        private static int RECURSION_LIMIT = 1024;

        /// <summary>
        /// If an empty array is passed, the result will be null
        /// if there are instances, but they share no common supertype the result will be var
        /// </summary>
        /// <param name="array"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static ClassNode GetGreatestCommonSubclassForArray(StackValue array, Core core)
        {
            if (!IsArray(array))
                throw new ArgumentException("The stack value provided was not an array");

            Dictionary<ClassNode, int> typeStats = GetTypeStatisticsForArray(array, core);


            //@PERF: This could be improved with a 
            List<List<int>> chains = new List<List<int>>();
            HashSet<int> commonTypeIDs = new HashSet<int>();

            foreach (ClassNode cn in typeStats.Keys)
            {
//<<<<<<< .mine
                List<int> chain = ClassUtils.GetClassUpcastChain(cn, core);

                //Now add in the other conversions - as we don't have a common superclass yet
                //@TODO(Jun): Remove this hack when we have a proper casting structure
                foreach (int id in cn.coerceTypes.Keys)
                    if (!chain.Contains(id))
                        chain.Add((id));

//=======
//                List<int> chain = GetConversionChain(cn, core);
//>>>>>>> .r2886
                chains.Add(chain);

                foreach (int nodeId in chain)
                    commonTypeIDs.Add(nodeId);

 

            }

            //Remove nulls if they exist
            {
 
            if (commonTypeIDs.Contains(
                (int)PrimitiveType.kTypeNull))
                commonTypeIDs.Remove((int)PrimitiveType.kTypeNull);

                List<List<int>> nonNullChains = new List<List<int>>();

                foreach (List<int> chain in chains)
                {
                    if (chain.Contains((int)PrimitiveType.kTypeNull))
                        chain.Remove((int)PrimitiveType.kTypeNull);

                    if (chain.Count > 0)
                        nonNullChains.Add(chain);
                }

                chains = nonNullChains;
                    
            }


            //Contract the hashset so that it contains only the nodes present in all chains
            //@PERF: this is very inefficent
            {
                foreach (List<int> chain in chains)
                {
                    commonTypeIDs.IntersectWith(chain);
                    

                }
            }

            //No common subtypes
            if (commonTypeIDs.Count == 0)
                return null;

            if (commonTypeIDs.Count == 1)
                return core.DSExecutable.classTable.ClassNodes[commonTypeIDs.First()];


            List<int> lookupChain = chains[0];

            
            //Insertion sort the IDs, we may only have a partial ordering on them.
            List<int> orderedTypes = new List<int>();

            foreach (int typeToInsert in commonTypeIDs)
            {
                bool inserted = false;

                for (int i = 0; i < orderedTypes.Count; i++)
                {
                    int orderedType = orderedTypes[i];

                    if (lookupChain.IndexOf(typeToInsert) < lookupChain.IndexOf(orderedType))
                    {
                        inserted = true;
                        orderedTypes.Insert(i, typeToInsert);
                        break;
                    }
                }

                if (!inserted)
                    orderedTypes.Add(typeToInsert);
            }

            return core.DSExecutable.classTable.ClassNodes[orderedTypes.First()];
        }

        /// <summary>
        /// For a class node using single inheritence, get the chain of inheritences
        /// </summary>
        /// <param name="cn"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static List<int> GetConversionChain(ClassNode cn, Core core)
        {
            List<int> ret = new List<int>();
            List<int> coercableTypes = new List<int>();

            foreach (int typeID in cn.coerceTypes.Keys)
            {
                bool inserted = false;

                for (int i = 0; i < coercableTypes.Count; i++)
                {
                    if (cn.coerceTypes[typeID] < cn.coerceTypes[coercableTypes[i]])
                    {
                        inserted = true;
                        coercableTypes.Insert(typeID, i);
                        break;
                    }
                }
                if (!inserted)
                    coercableTypes.Add(typeID);
            }
            coercableTypes.Add(core.DSExecutable.classTable.ClassNodes.IndexOf(cn));



            ret.AddRange(coercableTypes);
            return ret;

        }

        public static Dictionary<int, StackValue> GetTypeExamplesForLayer(StackValue array, Core core)
        {
            if (!IsArray(array))
            {
                Dictionary<int, StackValue> ret = new Dictionary<int, StackValue>();
                ret.Add((int)array.metaData.type, array);
                return ret;
            }

            Dictionary<int, StackValue> usageFreq = new Dictionary<int, StackValue>();

            //This is the element on the heap that manages the data structure
            HeapElement heapElement = core.Heap.Heaplist[(int)array.opdata];

            for (int i = 0; i < heapElement.VisibleSize; ++i)
            {
                StackValue sv = heapElement.Stack[i];
                if (!usageFreq.ContainsKey((int)sv.metaData.type))
                    usageFreq.Add((int)sv.metaData.type, sv);
            }

            return usageFreq;
        }



        /// <summary>
        /// Generate type statistics for given layer of an array
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static Dictionary<ClassNode, int> GetTypeStatisticsForLayer(StackValue array, Core core)
        {
            if (!IsArray(array))
            {
                Dictionary<ClassNode, int> ret = new Dictionary<ClassNode, int>();
                ret.Add(core.DSExecutable.classTable.ClassNodes[(int)array.metaData.type], 1);
                return ret;
            }

            Dictionary<ClassNode, int> usageFreq = new Dictionary<ClassNode,int>();

            //This is the element on the heap that manages the data structure
            HeapElement heapElement = core.Heap.Heaplist[(int)array.opdata];

            for (int i = 0; i < heapElement.VisibleSize; ++i)
            {
                StackValue sv = heapElement.Stack[i];
                ClassNode cn = core.DSExecutable.classTable.ClassNodes[(int)sv.metaData.type];
                if (!usageFreq.ContainsKey(cn))
                    usageFreq.Add(cn, 0);

                usageFreq[cn] = usageFreq[cn] + 1;

            }

            return usageFreq;
        }

        /// <summary>
        /// Generate type statistics for the whole array
        /// </summary>
        /// <param name="array"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static Dictionary<ClassNode, int> GetTypeStatisticsForArray(StackValue array, Core core)
        {
            if (!IsArray(array))
            {
                Dictionary<ClassNode, int> ret = new Dictionary<ClassNode, int>();
                ret.Add(core.DSExecutable.classTable.ClassNodes[(int) array.metaData.type], 1);
                return ret;
            }

            Dictionary<ClassNode, int> usageFreq = new Dictionary<ClassNode, int>();

            //This is the element on the heap that manages the data structure
            HeapElement heapElement = core.Heap.Heaplist[(int)array.opdata];

            for (int i = 0; i < heapElement.VisibleSize; ++i)
            {
                StackValue sv = heapElement.Stack[i];

                if (sv.optype == AddressType.ArrayPointer)
                {
                    //Recurse
                    Dictionary<ClassNode, int> subLayer = GetTypeStatisticsForArray(sv, core);
                    foreach (ClassNode cn in subLayer.Keys)
                    {
                        if (!usageFreq.ContainsKey(cn))
                            usageFreq.Add(cn, 0);

                        usageFreq[cn] = usageFreq[cn] + subLayer[cn];

                    }
                }
                else
                {

                    ClassNode cn = core.DSExecutable.classTable.ClassNodes[(int)sv.metaData.type];
                    if (!usageFreq.ContainsKey(cn))
                        usageFreq.Add(cn, 0);

                    usageFreq[cn] = usageFreq[cn] + 1;
                }
            }

            return usageFreq;
        }

        private static int GetMaxRankForArray(StackValue array, Core core, int tracer)
        {
            if (tracer > RECURSION_LIMIT)
                throw new CompilerInternalException("Internal Recursion limit exceeded in Rank Check - Possible heap corruption {3317D4F6-4758-4C19-9680-75B68DA0436D}");

            if (!IsArray(array))
                return 0;
            //throw new ArgumentException("The stack value provided was not an array");

            int ret = 1;

            //This is the element on the heap that manages the data structure
            HeapElement heapElement = core.Heap.Heaplist[(int)array.opdata];


            int largestSub = 0;

            for (int i = 0; i < heapElement.VisibleSize; ++i)
            {
                StackValue sv = heapElement.Stack[i];

                if (sv.optype == AddressType.ArrayPointer)
                {
                    int subArrayRank = GetMaxRankForArray(sv, core, tracer + 1);

                    largestSub = Math.Max(subArrayRank, largestSub);
                }
            }

            return largestSub + ret;


        }
        public static int GetMaxRankForArray(StackValue array, Core core)
        {
            return GetMaxRankForArray(array, core, 0);

        }

        /// <summary>
        /// Is the StackValue An Array
        /// </summary>
        /// <param name="sv"></param>
        /// <returns></returns>
        public static bool IsArray(StackValue sv)
        {
            return sv.optype == AddressType.ArrayPointer;
        }

        /// <summary>
        /// Whether sv is double or arrays contains double value.
        /// </summary>
        /// <param name="sv"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static bool ContainsDoubleElement(StackValue sv, Core core)
        {
            if (!IsArray(sv))
                return core.TypeSystem.GetType(sv) == (int)PrimitiveType.kTypeDouble;

            StackValue[] svArray = core.Rmem.GetArrayElements(sv);
            foreach (var item in svArray)
            {
                if (IsArray(item) && ContainsDoubleElement(item, core))
                    return true;

                if (core.TypeSystem.GetType(item) == (int)PrimitiveType.kTypeDouble)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// If the passed in value is not an array or an empty array or an array which contains only empty arrays, return false.
        /// Otherwise, return true;
        /// </summary>
        /// <param name="sv"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static bool ContainsNonArrayElement(StackValue sv, Core core)
        {
            if (!IsArray(sv))
                return true;

            StackValue[] svArray = core.Rmem.GetArrayElements(sv);
            foreach (var item in svArray)
            {
                if (ContainsNonArrayElement(item, core))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Pull the heap element out of an array pointer
        /// </summary>
        /// <param name="sv"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static HeapElement GetHeapElement(StackValue sv, Core core)
        {
            Validity.Assert(IsArray(sv));

            HeapElement he = core.Heap.Heaplist[(int)sv.opdata];
            return he;
        }

        public static StackValue GetArrayElement(StackValue svPtr, List<StackValue> svDimList, Core core)
        {
            Validity.Assert(IsArray(svPtr));
            HeapElement heapElem = null;

            StackValue svFinal = StackUtils.BuildNull();
            foreach (StackValue sv in svDimList)
            {
                if (svPtr.optype != AddressType.ArrayPointer)
                {
                    core.RuntimeStatus.LogWarning(RuntimeData.WarningID.kOverIndexing, ProtoCore.RuntimeData.WarningMessage.kArrayOverIndexed);
                    return StackUtils.BuildNull();
                }
                heapElem = core.Heap.Heaplist[(int)svPtr.opdata];
                svPtr = StackUtils.GetValue(heapElem, (int)sv.opdata, core);
                svFinal = svPtr;
            }
            return svFinal;
        }

        public static bool IsUniform(StackValue sv, Core core)
        {
            if (!IsArray(sv))
                return false;

            if (Utils.ArrayUtils.GetTypeStatisticsForArray(sv, core).Count != 1)
                return false;

            return true;
        }
    
    
        [Obsolete]
        public static StackValue CoerceArray(StackValue array, Type typ, Core core)
        {
            //@TODO(Luke) handle array rank coersions

            Validity.Assert(IsArray(array), "Argument needs to be an array {99FB71A6-72AD-4C93-8F1E-0B1F419C1A6D}");

            //This is the element on the heap that manages the data structure
            HeapElement heapElement = core.Heap.Heaplist[(int)array.opdata];
            StackValue[] newSVs = new StackValue[heapElement.VisibleSize];

            for (int i = 0; i < heapElement.VisibleSize; ++i)
            {
                StackValue sv = heapElement.Stack[i];
                StackValue coercedValue;

                if (IsArray(sv))
                {
                    Type typ2 = new Type();
                    typ2.UID = typ.UID;
                    typ2.rank = typ.rank - 1;
                    typ2.IsIndexable = (typ2.rank == -1 || typ2.rank > 0);

                    coercedValue = CoerceArray(sv, typ2, core);
                }
                else
                {
                    coercedValue = TypeSystem.Coerce(sv, typ, core);
                }

                GCUtils.GCRetain(coercedValue, core);
                newSVs[i] = coercedValue;
            }
            
            return HeapUtils.StoreArray(newSVs, core);


        }



        // Retrieve the first non-array element in an array 
        public static bool GetFirstNonArrayStackValue(StackValue svArray, ref StackValue sv, Core core)
        {
            if (AddressType.ArrayPointer != svArray.optype)
            {
                return false;
            }

            int ptr = (int)svArray.opdata;
            while (IsArray(core.Rmem.Heap.Heaplist[ptr].Stack[0]))
            {
                ptr = (int)core.Rmem.Heap.Heaplist[ptr].Stack[0].opdata;
            }

            sv.optype = core.Rmem.Heap.Heaplist[ptr].Stack[0].optype;
            sv.opdata = core.Rmem.Heap.Heaplist[ptr].Stack[0].opdata;
            sv.metaData = core.Rmem.Heap.Heaplist[ptr].Stack[0].metaData;
            return true;
        }

        public static StackValue SetDataAtIndex(StackValue svArray, int index, StackValue svData, Core core)
        {
            Validity.Assert(svArray.optype == AddressType.ArrayPointer || svArray.optype == AddressType.String);
            if (svArray.optype == AddressType.String && svData.optype != AddressType.Char)
            {
                core.RuntimeStatus.LogWarning(RuntimeData.WarningID.kTypeMismatch, RuntimeData.WarningMessage.kAssignNonCharacterToString);
                return StackUtils.BuildNull();
            }

            lock (core.Heap.cslock)
            {
                HeapElement arrayHeap = core.Heap.Heaplist[(int)svArray.opdata];
                index = arrayHeap.ExpandByAcessingAt(index);
                StackValue oldValue = arrayHeap.SetValue(index, svData);
                return oldValue;
            }
        }

        public static StackValue SetDataAtIndices(StackValue array, int[] indices, StackValue data, Core core)
        {
            Validity.Assert(array.optype == AddressType.ArrayPointer || array.optype == AddressType.String);

            StackValue arrayItem = array;
            for (int i = 0; i < indices.Length - 1; ++i)
            {
                HeapElement arrayHeap = core.Heap.Heaplist[(int)arrayItem.opdata];
                int index = arrayHeap.ExpandByAcessingAt(indices[i]);
                arrayItem = arrayHeap.Stack[index];

                if (arrayItem.optype == AddressType.String)
                {
                    core.RuntimeStatus.LogWarning(RuntimeData.WarningID.kOverIndexing, ProtoCore.RuntimeData.WarningMessage.kStringOverIndexed);
                    return StackUtils.BuildNull();
                }
                else if (arrayItem.optype != AddressType.ArrayPointer)
                {
                    StackValue sv = HeapUtils.StoreArray(new StackValue[] { arrayItem }, core);
                    GCUtils.GCRetain(sv, core);
                    arrayHeap.Stack[index] = sv;
                    arrayItem = arrayHeap.Stack[index];
                }
            }

            return ArrayUtils.SetDataAtIndex(arrayItem, indices[indices.Length - 1], data, core);
        }

        public static int[][] GetZippedIndices(List<StackValue> svIndices, Core core)
        {
            // Get replication count firstly to avoid iterate a potentially 
            // large array later on
            int repCount = System.Int32.MaxValue;
            foreach (var svIndex in svIndices)
            {
                if (svIndex.optype == AddressType.ArrayPointer)
                {
                    int count = core.Heap.Heaplist[(int)svIndex.opdata].VisibleSize;
                    if (repCount > count)
                    {
                        repCount = count;
                    }
                }
                else
                {
                    repCount = 1;
                    break;
                }
            }

            if (repCount == 1)
            {
                int[] indices = new int[svIndices.Count];
                for (int i = 0; i < svIndices.Count; ++i)
                {
                    StackValue svIndex = svIndices[i];
                    StackValue coercedIndex;

                    if (svIndex.optype == AddressType.ArrayPointer)
                    {
                        coercedIndex = core.Heap.Heaplist[(int)svIndex.opdata].Stack[0].AsInt();
                    }
                    else
                    {
                        coercedIndex = svIndex.AsInt();
                    }

                    if (coercedIndex.optype == AddressType.Null)
                    {
                        return null;
                    }
                    else
                    {
                        indices[i] = (int)coercedIndex.opdata;
                    }
                }
                return new int[][] { indices };
            }
            else
            {
                int dimCount = svIndices.Count;
                int[][] zippedIndices = new int[repCount][];
                for (int i = 0; i < repCount; ++i)
                {
                    zippedIndices[i] = new int[dimCount];
                }

                for (int i = 0; i < dimCount; ++i)
                {
                    StackValue svIndex = svIndices[i];
                    HeapElement subIndicesHeap = core.Heap.Heaplist[(int)svIndex.opdata];

                    for (int j = 0; j < repCount; ++j)
                    {
                        StackValue coercedIndex = subIndicesHeap.Stack[j].AsInt();
                        if (coercedIndex.optype == AddressType.Null)
                        {
                            return null;
                        }
                        else
                        {
                            zippedIndices[j][i] = (int)coercedIndex.opdata;
                        }
                    }
                }

                return zippedIndices;
            }
        }

        public static StackValue SetDataAtIndices(StackValue array, List<StackValue> indices, StackValue data, Type t, Core core)
        {
            int[][] zippedIndices = ArrayUtils.GetZippedIndices(indices, core);
            if (zippedIndices == null || zippedIndices.Length == 0)
            {
                return StackUtils.BuildNull();
            }

            if (zippedIndices.Length == 1)
            {
                StackValue coercedData = TypeSystem.Coerce(data, t, core);
                GCUtils.GCRetain(coercedData, core);
                return ArrayUtils.SetDataAtIndices(array, zippedIndices[0], coercedData, core);
            }
            else if (data.optype == AddressType.ArrayPointer)
            {
                if (t.rank > 0)
                {
                    t.rank = t.rank - 1;
                    if (t.rank == 0)
                    {
                        t.IsIndexable = false;
                    }
                }

                HeapElement dataHeapElement = core.Heap.Heaplist[(int)data.opdata];
                int length = Math.Min(zippedIndices.Length, dataHeapElement.VisibleSize);

                StackValue[] oldValues = new StackValue[length];
                for (int i = 0; i < length; ++i)
                {
                    StackValue coercedData = TypeSystem.Coerce(dataHeapElement.Stack[i], t, core);
                    GCUtils.GCRetain(coercedData, core);
                    oldValues[i] = ArrayUtils.SetDataAtIndices(array, zippedIndices[i], coercedData, core);
                }
                return HeapUtils.StoreArray(oldValues, core);
            }
            else
            {
                StackValue coercedData = TypeSystem.Coerce(data, t, core);
                GCUtils.GCRetain(coercedData, core);

                StackValue[] oldValues = new StackValue[zippedIndices.Length];
                for (int i = 0; i < zippedIndices.Length; ++i)
                {
                    oldValues[i] = ArrayUtils.SetDataAtIndices(array, zippedIndices[i], coercedData, core);
                }
                return HeapUtils.StoreArray(oldValues, core);
            }
        }


        public static StackValue GetArrayElementAt(StackValue array, int index, Core core)
        {
            int ptr = (int)array.opdata;
            HeapElement hs = core.Rmem.Heap.Heaplist[ptr];
            return hs.Stack[index];
        }
    }
}
