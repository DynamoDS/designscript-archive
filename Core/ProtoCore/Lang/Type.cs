using System;
using System.Collections.Generic;
using System.Diagnostics;
using ProtoCore.BuildData;
using ProtoCore.DSASM;
using ProtoCore.Utils;

namespace ProtoCore
{
    public struct Type
    {
        public string Name;
        public int UID;

        public bool IsIndexable;
        public int rank;

        /// <summary>
        /// Comment Jun: Initialize a type to the default values
        /// </summary>
        public void Initialize()
        {
            Name = string.Empty;
            UID = ProtoCore.DSASM.Constants.kInvalidIndex;
            IsIndexable = false;
            rank = DSASM.Constants.kArbitraryRank;
        }

        public override string ToString()
        {
            string retName = (Name == "" ? UID.ToString() : Name);

            string rankText = "";

            if (IsIndexable)
            {
                if (rank == DSASM.Constants.kArbitraryRank)
                    rankText = "[]..[]";
                else
                {
                    for (int i = 0; i < rank; i++)
                        rankText += "[]";
                }
            }

            return retName + rankText;
        }

        public bool Equals(Type type)
        {
            return this.Name == type.Name && this.UID == type.UID && this.rank == type.rank;
        }

    }

    public enum PrimitiveType
    {
        kInvalidType = -1,
        kTypeNull,
        kTypeArray,
        kTypeDouble,
        kTypeInt,
        kTypeDynamic,
        kTypeBool,
        kTypeChar,
        kTypeString,
        kTypeVar,
        kTypeVoid,
        kTypeHostEntityID,
        kTypePointer,
        kTypeFunctionPointer,
        kTypeReturn,
        kMaxPrimitives
    }

    public class TypeSystem
    {
        public ProtoCore.DSASM.ClassTable classTable { get; private set; }
        public Dictionary<ProtoCore.DSASM.AddressType, int> addressTypeClassMap { get; set; }

        public TypeSystem()
        {
            SetTypeSystem();
            BuildAddressTypeMap();
        }


        public void SetClassTable(ProtoCore.DSASM.ClassTable table)
        {
            Debug.Assert(null != table);
            Debug.Assert(0 == table.ClassNodes.Count);

            if (0 != table.ClassNodes.Count)
            {
                return;
            }

            for (int i = 0; i < classTable.ClassNodes.Count; ++i)
            {
                table.Append(classTable.ClassNodes[i]);
            }
            classTable = table;
        }

        public void BuildAddressTypeMap()
        {
            addressTypeClassMap = new Dictionary<DSASM.AddressType, int>();
            addressTypeClassMap.Add(ProtoCore.DSASM.AddressType.Null, (int)PrimitiveType.kTypeNull);
            addressTypeClassMap.Add(ProtoCore.DSASM.AddressType.ArrayPointer, (int)PrimitiveType.kTypeArray);
            addressTypeClassMap.Add(ProtoCore.DSASM.AddressType.Double, (int)PrimitiveType.kTypeDouble);
            addressTypeClassMap.Add(ProtoCore.DSASM.AddressType.Char, (int)PrimitiveType.kTypeChar);
            addressTypeClassMap.Add(ProtoCore.DSASM.AddressType.String, (int)PrimitiveType.kTypeString);
            addressTypeClassMap.Add(ProtoCore.DSASM.AddressType.Int, (int)PrimitiveType.kTypeInt);
            addressTypeClassMap.Add(ProtoCore.DSASM.AddressType.Boolean, (int)PrimitiveType.kTypeBool);
            addressTypeClassMap.Add(ProtoCore.DSASM.AddressType.Pointer, (int)PrimitiveType.kTypePointer);
            addressTypeClassMap.Add(ProtoCore.DSASM.AddressType.FunctionPointer, (int)PrimitiveType.kTypeFunctionPointer);
            addressTypeClassMap.Add(ProtoCore.DSASM.AddressType.DefaultArg, (int)PrimitiveType.kTypeVar);
        }


        public void SetTypeSystem()
        {
            Debug.Assert(null == classTable);
            if (null != classTable)
            {
                return;
            }

            classTable = new DSASM.ClassTable();

            classTable.Reserve((int)PrimitiveType.kMaxPrimitives);

            ProtoCore.DSASM.ClassNode cnode;

            cnode = new ProtoCore.DSASM.ClassNode { name = DSDefinitions.Kw.kw_array, size = 0, rank = 5, symbols = null, vtable = null, typeSystem = this };
            /*cnode.coerceTypes.Add((int)PrimitiveType.kTypeDouble, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeInt, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeBool, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeChar, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeString, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            */
            cnode.classId = (int)PrimitiveType.kTypeArray;
            classTable.SetClassNodeAt(cnode, (int)PrimitiveType.kTypeArray);

            //
            //
            cnode = new ProtoCore.DSASM.ClassNode { name = DSDefinitions.Kw.kw_double, size = 0, rank = 4, symbols = null, vtable = null, typeSystem = this };
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeBool, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeInt, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceDoubleToIntScore);
            cnode.classId = (int)PrimitiveType.kTypeDouble;
            classTable.SetClassNodeAt(cnode, (int)PrimitiveType.kTypeDouble);

            //
            //
            cnode = new ProtoCore.DSASM.ClassNode { name = DSDefinitions.Kw.kw_int, size = 0, rank = 3, symbols = null, vtable = null, typeSystem = this };
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeBool, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeDouble, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceIntToDoubleScore);
            cnode.classId = (int)PrimitiveType.kTypeInt;
            classTable.SetClassNodeAt(cnode, (int)PrimitiveType.kTypeInt);

            //
            //
            cnode = new ProtoCore.DSASM.ClassNode { name = DSDefinitions.Kw.kw_bool, size = 0, rank = 2, symbols = null, vtable = null, typeSystem = this };
            // if convert operator to method call, without the following statement
            // a = true + 1 will fail, because _add expects two integers 
            //cnode.coerceTypes.Add((int)PrimitiveType.kTypeInt, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.classId = (int)PrimitiveType.kTypeBool;
            classTable.SetClassNodeAt(cnode, (int)PrimitiveType.kTypeBool);

            //
            //
            cnode = new ProtoCore.DSASM.ClassNode { name = DSDefinitions.Kw.kw_char, size = 0, rank = 1, symbols = null, vtable = null, typeSystem = this };
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeBool, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeString, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);

            cnode.classId = (int)PrimitiveType.kTypeChar;
            classTable.SetClassNodeAt(cnode, (int)PrimitiveType.kTypeChar);

            //
            //
            cnode = new ProtoCore.DSASM.ClassNode { name = DSDefinitions.Kw.kw_string, size = 0, rank = 0, symbols = null, vtable = null, typeSystem = this };
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeBool, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.classId = (int)PrimitiveType.kTypeString;
            classTable.SetClassNodeAt(cnode, (int)PrimitiveType.kTypeString);

            //
            //
            cnode = new ProtoCore.DSASM.ClassNode { name = DSDefinitions.Kw.kw_var, size = 0, rank = 0, symbols = null, vtable = null, typeSystem = this };
            /*cnode.coerceTypes.Add((int)PrimitiveType.kTypeDouble, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeInt, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeBool, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeChar, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeString, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);*/
            cnode.classId = (int)PrimitiveType.kTypeVar;
            classTable.SetClassNodeAt(cnode,(int)PrimitiveType.kTypeVar);

            //
            //
            cnode = new ProtoCore.DSASM.ClassNode { name = DSDefinitions.Kw.kw_null, size = 0, rank = 0, symbols = null, vtable = null, typeSystem = this };
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeDouble, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeInt, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeBool, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeChar, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeString, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.classId = (int)PrimitiveType.kTypeNull;
            classTable.SetClassNodeAt(cnode, (int)PrimitiveType.kTypeNull);

            //
            //
            cnode = new ProtoCore.DSASM.ClassNode { name = DSDefinitions.Kw.kw_void, size = 0, rank = 0, symbols = null, vtable = null, typeSystem = this };
            cnode.classId = (int)PrimitiveType.kTypeVoid;
            classTable.SetClassNodeAt(cnode,(int)PrimitiveType.kTypeVoid);

            //
            //
            cnode = new ProtoCore.DSASM.ClassNode { name = "hostentityid", size = 0, rank = 0, symbols = null, vtable = null, typeSystem = this };
            cnode.classId = (int)PrimitiveType.kTypeHostEntityID;
            classTable.SetClassNodeAt(cnode, (int)PrimitiveType.kTypeHostEntityID);
            //
            //
            cnode = new ProtoCore.DSASM.ClassNode { name = "pointer_reserved", size = 0, rank = 0, symbols = null, vtable = null, typeSystem = this };
            // if convert operator to method call, without the following statement, 
            // a = b.c + d.e will fail, b.c and d.e are resolved as pointer and _add method requires two integer
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeInt, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.classId = (int)PrimitiveType.kTypePointer;
            classTable.SetClassNodeAt(cnode, (int)PrimitiveType.kTypePointer);
            //
            //
            cnode = new ProtoCore.DSASM.ClassNode { name = DSDefinitions.Kw.kw_functionpointer, size = 0, rank = 0, symbols = null, vtable = null, typeSystem = this };
            cnode.coerceTypes.Add((int)PrimitiveType.kTypeInt, (int)ProtoCore.DSASM.ProcedureDistance.kCoerceScore);
            cnode.classId = (int)PrimitiveType.kTypeFunctionPointer;
            classTable.SetClassNodeAt(cnode, (int)PrimitiveType.kTypeFunctionPointer);

            //
            //
            cnode = new ProtoCore.DSASM.ClassNode { name = "return_reserved", size = 0, rank = 0, symbols = null, vtable = null, typeSystem = this };
            cnode.classId = (int)PrimitiveType.kTypeReturn;
            classTable.SetClassNodeAt(cnode, (int)PrimitiveType.kTypeReturn);
        }

        public bool IsHigherRank(int t1, int t2)
        {
            // TODO Jun: Refactor this when we implement operator overloading
            Debug.Assert(null != classTable);
            Debug.Assert(null != classTable.ClassNodes);
            if (t1 == (int)PrimitiveType.kInvalidType || t1 >= classTable.ClassNodes.Count)
            {
                return true;
            }
            else if (t2 == (int)PrimitiveType.kInvalidType || t2 >= classTable.ClassNodes.Count)
            {
                return false;
            }
            return classTable.ClassNodes[t1].rank >= classTable.ClassNodes[t2].rank;
        }


        public Type BuildTypeObject(PrimitiveType ptype, bool isArray, int rank = Constants.kUndefinedRank)
        {
            return BuildTypeObject((int)ptype, isArray, rank);
        }

        //@TODO(Luke): Once the type system has been refactored, get rid of this
        public Type BuildTypeObject(int UID, bool isArray, int rank = Constants.kUndefinedRank)
        {
            Type type = new Type();
            type.Name = GetType(UID);
            type.UID = UID;
            type.IsIndexable = isArray;
            type.rank = rank;
            return type;
        }

        public string GetType(int UID)
        {
            Debug.Assert(null != classTable);
            return classTable.GetTypeName(UID);
        }

        public string GetType(Type type)
        {
            Validity.Assert(null != classTable);
            return classTable.GetTypeName(type.UID);
        }

        public int GetType(string ident)
        {
            Debug.Assert(null != classTable);
            return classTable.IndexOf(ident);
        }

        public int GetType(ProtoCore.DSASM.StackValue sv)
        {
            int type = (int)ProtoCore.DSASM.Constants.kInvalidIndex;
            if (sv.IsReferenceType())
            {
                type = (int)sv.metaData.type;
            }
            else
            {
                if (!addressTypeClassMap.TryGetValue(sv.optype, out type))
                {
                    type = (int)PrimitiveType.kInvalidType;
                }
            }
            return type;
        }

        public static bool IsConvertibleTo(int fromType, int toType, Core core)
        {
            if (ProtoCore.DSASM.Constants.kInvalidIndex != fromType && ProtoCore.DSASM.Constants.kInvalidIndex != toType)
            {
                if (fromType == toType)
                {
                    return true;
                }

                return core.DSExecutable.classTable.ClassNodes[fromType].ConvertibleTo(toType);
            }
            return false;
        }

        //@TODO: Factor this into the type system

        public static StackValue ClassCoerece(StackValue sv, Type targetType, Core core)
        {
            //@TODO: Add proper coersion testing here.

            if (targetType.UID == (int)PrimitiveType.kTypeBool)
                return StackUtils.BuildBoolean(true);

            return sv.ShallowClone();
        }

        public static StackValue Coerce(StackValue sv, int UID, int rank, Core core)
        {
            Type t = new Type();
            t.UID = UID;
            if (rank == Constants.kUndefinedRank)
            {
                rank = DSASM.Constants.kArbitraryRank;
            }
            t.rank = rank;
            t.IsIndexable = (t.rank != 0);

            return Coerce(sv, t, core);
        }

        public static StackValue Coerce(StackValue sv, Type targetType, Core core)
        {

            //@TODO(Jun): FIX ME - abort coersion for default args
            if (sv.optype == AddressType.DefaultArg)
                return sv;

            if ( !(
                (int)sv.metaData.type == targetType.UID || 
                (core.DSExecutable.classTable.ClassNodes[(int)sv.metaData.type].ConvertibleTo(targetType.UID))
                || sv.optype == AddressType.ArrayPointer))
            {
                core.RuntimeStatus.LogWarning(RuntimeData.WarningID.kConversionNotPossible, ProtoCore.RuntimeData.WarningMessage.kConvertNonConvertibleTypes);
                return StackUtils.BuildNull();

            }






            //if it's an array
            if (sv.optype == AddressType.ArrayPointer && !targetType.IsIndexable && targetType.rank != DSASM.Constants.kUndefinedRank)// && targetType.UID != (int)PrimitiveType.kTypeVar)
            {
                //This is an array rank reduction
                //this may only be performed in recursion and is illegal here
                string errorMessage = String.Format(ProtoCore.RuntimeData.WarningMessage.kConvertArrayToNonArray, core.TypeSystem.GetType(targetType.UID));
                core.RuntimeStatus.LogWarning(RuntimeData.WarningID.kConversionNotPossible, errorMessage);
                return StackUtils.BuildNull();
            }


            if (sv.optype == AddressType.ArrayPointer && targetType.IsIndexable)
            {
                Validity.Assert(ArrayUtils.IsArray(sv));

                //We're being asked to convert an array into an array
                //walk over the structure converting each othe elements
                
                if (targetType.UID == (int)PrimitiveType.kTypeVar && targetType.rank == DSASM.Constants.kArbitraryRank && core.Heap.IsTemporaryPointer(sv))
                {
                    return sv;
                }

                HeapElement he = core.Heap.Heaplist[(int)sv.opdata];
                StackValue[] newSubSVs = new StackValue[he.VisibleSize];

                //Validity.Assert(targetType.rank != -1, "Arbitrary rank array conversion not yet implemented {2EAF557F-62DE-48F0-9BFA-F750BBCDF2CB}");

                //Decrease level of reductions by one
                Type newTargetType = new Type();
                newTargetType.UID = targetType.UID;
                if (targetType.rank != ProtoCore.DSASM.Constants.kArbitraryRank)
                {
                    newTargetType.rank = targetType.rank - 1;
                    newTargetType.IsIndexable = newTargetType.rank > 0;
                }
                else
                {
                    if (ArrayUtils.GetMaxRankForArray(sv, core) == 1)
                    {
                        //Last unpacking
                        newTargetType.rank = 0;
                        newTargetType.IsIndexable = false;
                    }
                    else
                    {

                        newTargetType.rank = ProtoCore.DSASM.Constants.kArbitraryRank;
                        newTargetType.IsIndexable = true;
                    }
                    
                }


                for (int i = 0; i < he.VisibleSize; i++)
                {
                    StackValue coercedValue = Coerce(he.Stack[i], newTargetType, core);
                    GCUtils.GCRetain(coercedValue, core);
                    newSubSVs[i] = coercedValue;
                }

                StackValue newSV = HeapUtils.StoreArray(newSubSVs, core);
                return newSV;
            }

            if (sv.optype != AddressType.ArrayPointer && sv.optype != AddressType.Null && 
                targetType.IsIndexable && 
                targetType.rank != DSASM.Constants.kArbitraryRank)
            {
                //We're being asked to promote the value into an array
                if (targetType.rank == 1)
                {
                    Type newTargetType = new Type();
                    newTargetType.UID = targetType.UID;
                    newTargetType.IsIndexable = false;
                    newTargetType.Name = targetType.Name;
                    newTargetType.rank = 0;
                    

                    //Upcast once
                    StackValue coercedValue = Coerce(sv, newTargetType, core);
                    GCUtils.GCRetain(coercedValue, core);
                    StackValue newSv = HeapUtils.StoreArray(new StackValue[] { coercedValue }, core);
                    return newSv;
                }
                else
                {
                    Validity.Assert(targetType.rank > 1, "Target rank should be greater than one for this clause");

                    Type newTargetType = new Type();
                    newTargetType.UID = targetType.UID;
                    newTargetType.IsIndexable = true;
                    newTargetType.Name = targetType.Name;
                    newTargetType.rank = targetType.rank -1;


                    //Upcast once
                    StackValue coercedValue = Coerce(sv, newTargetType, core);
                    GCUtils.GCRetain(coercedValue, core);
                    StackValue newSv = HeapUtils.StoreArray(new StackValue[] { coercedValue }, core);
                    return newSv;
                }


            }


            if (sv.optype == AddressType.Pointer)
            {
                StackValue ret = ClassCoerece(sv, targetType, core);
                return ret;
            }

            //If it's anything other than array, just create a new copy
            switch (targetType.UID)
            {
                case (int)PrimitiveType.kInvalidType:
                    Validity.Assert(false, "Can't convert invalid type");
                    break;

                case (int)PrimitiveType.kTypeBool:
                    return sv.AsBoolean(core);

                case (int)PrimitiveType.kTypeChar:
                    {
                        StackValue newSV = sv.ShallowClone();
                        newSV.metaData = new MetaData { type = (int)PrimitiveType.kTypeChar };
                        return newSV;
                    }

                case (int)PrimitiveType.kTypeDouble:
                    return sv.AsDouble();

                case (int)PrimitiveType.kTypeFunctionPointer:
                    if (sv.metaData.type != (int)PrimitiveType.kTypeFunctionPointer)
                    {
                        core.RuntimeStatus.LogWarning(RuntimeData.WarningID.kTypeMismatch, ProtoCore.RuntimeData.WarningMessage.kFailToConverToFunction);
                        return StackUtils.BuildNull();
                    }
                    return sv.ShallowClone();

                case (int)PrimitiveType.kTypeHostEntityID:
                    {
                        StackValue newSV = sv.ShallowClone();
                        newSV.metaData = new MetaData { type = (int)PrimitiveType.kTypeHostEntityID };
                        return newSV;
                    }

                case (int)PrimitiveType.kTypeInt:
                    {
                        if (sv.metaData.type == (int)PrimitiveType.kTypeDouble)
                        {
                            core.RuntimeStatus.LogWarning(RuntimeData.WarningID.kTypeConvertionCauseInfoLoss, ProtoCore.RuntimeData.WarningMessage.kConvertDoubleToInt);
                        }
                        return sv.AsInt();
                    }

                case (int)PrimitiveType.kTypeNull:
                    {
                        if (sv.metaData.type != (int)PrimitiveType.kTypeNull)
                        {
                            core.RuntimeStatus.LogWarning(RuntimeData.WarningID.kTypeMismatch, ProtoCore.RuntimeData.WarningMessage.kFailToConverToNull);
                            return StackUtils.BuildNull();
                        }
                        return sv.ShallowClone();
                    }

                case (int)PrimitiveType.kTypePointer:
                    {
                        if (sv.metaData.type != (int)PrimitiveType.kTypeNull)
                        {
                            core.RuntimeStatus.LogWarning(RuntimeData.WarningID.kTypeMismatch, ProtoCore.RuntimeData.WarningMessage.kFailToConverToPointer);
                            return StackUtils.BuildNull();
                        }
                        StackValue ret = sv.ShallowClone();
                        return ret;
                    }

                case (int)PrimitiveType.kTypeString:
                    {
                        
                        StackValue newSV = sv.ShallowClone();
                        newSV.metaData = new MetaData { type = (int)PrimitiveType.kTypeString };
                        if (sv.metaData.type == (int)PrimitiveType.kTypeChar)
                        {
                            char ch = ProtoCore.Utils.EncodingUtils.ConvertInt64ToCharacter(newSV.opdata);
                            newSV = StackUtils.BuildString(ch.ToString(), core.Heap);
                        }
                        return newSV;
                    } 

                case (int)PrimitiveType.kTypeVar:
                    {
                        return sv;
                    }


                case (int)PrimitiveType.kTypeArray:
                    {

                        HeapElement he = core.Heap.Heaplist[(int)sv.opdata];
                        StackValue[] newSubSVs = new StackValue[he.VisibleSize];

                        for (int i = 0; i < he.VisibleSize; i++)
                        {
                            StackValue coercedValue = Coerce(he.Stack[i], targetType, core);
                            GCUtils.GCRetain(coercedValue, core);
                            newSubSVs[i] = coercedValue;
                        }

                        StackValue newSV = HeapUtils.StoreArray(newSubSVs, core);
                        return newSV;
                    }


                default:
                    if (sv.optype == AddressType.Null)
                        return StackUtils.BuildNull();
                    else
                    throw new NotImplementedException("Requested coercion not implemented");
            }



            throw new NotImplementedException("Requested coercion not implemented");


        }

    }
}
