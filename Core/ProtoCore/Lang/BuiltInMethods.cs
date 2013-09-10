using System.Collections.Generic;
using ProtoCore.Utils;

namespace ProtoCore.Lang
{
    public class BuiltInMethods
    {
        public enum BuiltInMethodID
        {
            kInvalidMethodID = -1,
            kAllFalse,
            kAllTrue,
            kAverage,
            kCancat,
            kContains,
            kCount,
            kCountTrue,
            kCountFalse,
            kDifference,
            kDot,
            kDotDynamic,
            kEquals,
            kGetElapsedTime,
            kGetType,
            kFlatten,
            kImportData,
            kIndexOf,
            kInsert,
            kIntersection,
            kInvalid,
            kIsUniformDepth,
            kIsRectangular,
            kIsHomogeneous,
            kLoadCSVWithMode,
            kLoadCSV,
            kMap,
            kMapTo,
            kNormalizeDepth,
            kNormalizeDepthWithRank,
            kPrint,
            kPrintIndexable,
            kRank,
            kRemove,
            kRemoveDuplicates,
            kRemoveNulls,
            kRemoveIfNot,
            kReverse,
            kSleep,
            kSomeFalse,
            kSomeNulls,
            kSomeTrue,
            kSort,
            kSortWithMode,
            kSortIndexByValue,
            kSortIndexByValueWithMode,
            kSortPointer,
            kReorder,
            kRangeExpression,
            kSum,
            kToString,
            kTranspose,
            kUnion,
            kInlineConditional,
            kBreak,
        }

        public class BuiltInMethod
        {
            public BuiltInMethodID ID { get; set; }
            public string Name { get; set; }
            public List<KeyValuePair<string, ProtoCore.Type>> Parameters { get; set; }
            public ProtoCore.Type ReturnType { get; set; }
        }


        public List<BuiltInMethod> Methods { get; set; }

        public BuiltInMethods(ProtoLanguage.CompileStateTracker compileState)
        {
            Validity.Assert(null == Methods);
            Methods = new List<BuiltInMethod>();
            Methods.Add(new BuiltInMethod
                            {
                Name = "Count",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeInt,
                    rank = 0,
                    IsIndexable = false,
                    Name = "int",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.BuiltInMethodID.kCount
            });
            Methods.Add(new BuiltInMethod
                            {
                Name = "SomeNulls",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeBool,
                    rank = 0,
                    IsIndexable = false,
                    Name = "bool",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.BuiltInMethodID.kSomeNulls
            });
            Methods.Add(new BuiltInMethod
                            {
                Name = "Rank",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeInt,
                    rank = 0,
                    IsIndexable = false,
                    Name = "int"
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.BuiltInMethodID.kRank
            });
            Methods.Add(new BuiltInMethod
                            {
                Name = "Flatten",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = 1,
                    IsIndexable = true,
                    Name = "var"
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.BuiltInMethodID.kFlatten
            });
            Methods.Add(new BuiltInMethod
                            {
                Name = "Concat",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = DSASM.Constants.kArbitraryRank,
                    IsIndexable = true,
                    Name = "var"
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array1", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                    new KeyValuePair<string, ProtoCore.Type>("array2", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.BuiltInMethodID.kCancat
            });
            Methods.Add(new BuiltInMethod
                            {
                Name = "SetIntersection",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = 1,
                    IsIndexable = true,
                    Name = "var"
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array1", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = 1 }),
                    new KeyValuePair<string, ProtoCore.Type>("array2", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = 1 })
                },
                ID = BuiltInMethods.BuiltInMethodID.kIntersection
            });
            Methods.Add(new BuiltInMethod
                            {
                Name = "SetUnion",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = 1,
                    IsIndexable = true,
                    Name = "var"
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array1", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = 1 }),
                    new KeyValuePair<string, ProtoCore.Type>("array2", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = 1 })
                },
                ID = BuiltInMethods.BuiltInMethodID.kUnion
            });
            Methods.Add(new BuiltInMethod
                            {
                Name = "SetDifference",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = 1,
                    IsIndexable = true,
                    Name = "var"
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array1", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = 1 }),
                    new KeyValuePair<string, ProtoCore.Type>("array2", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = 1 })
                },
                ID = BuiltInMethods.BuiltInMethodID.kDifference
            });
            Methods.Add(new BuiltInMethod
                            {
                Name = "CountTrue",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeInt,
                    rank = 0,
                    IsIndexable = false,
                    Name = "int",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.BuiltInMethodID.kCountTrue
            });
           
            Methods.Add(new BuiltInMethod
                            {
                Name = "CountFalse",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeInt,
                    rank = 0,
                    IsIndexable = false,
                    Name = "int",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.BuiltInMethodID.kCountFalse
            });
            Methods.Add(new BuiltInMethod
                            {
                Name = "AllFalse",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeBool,
                    rank = 0,
                    IsIndexable = false,
                    Name = "bool",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.BuiltInMethodID.kAllFalse
            });
            Methods.Add(new BuiltInMethod
            {
                Name = "AllTrue",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeBool,
                    rank = 0,
                    IsIndexable = false,
                    Name = "bool",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.BuiltInMethodID.kAllTrue
            });
            Methods.Add(new BuiltInMethod
            {
                Name = "IsHomogeneous",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeBool,
                    rank = 0,
                    IsIndexable = false,
                    Name = "bool",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.BuiltInMethodID.kIsHomogeneous
            });
            Methods.Add(new BuiltInMethod
                            {
                Name = "Sum",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeInt,
                    rank = 0,
                    IsIndexable = false,
                    Name = "int",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeInt, Name = "int", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.BuiltInMethodID.kSum
            });
            Methods.Add(new BuiltInMethod
            {
                Name = "Sum",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeDouble,
                    rank = 0,
                    IsIndexable = false,
                    Name = "double",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeDouble, Name = "double", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.BuiltInMethodID.kSum
            });
            Methods.Add(new BuiltInMethod
                            {
                Name = "Average",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeDouble,
                    rank = 0,
                    IsIndexable = false,
                    Name = "double",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeInt, Name = "int", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.BuiltInMethodID.kAverage
            });
            Methods.Add(new BuiltInMethod
            {
                Name = "Average",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeDouble,
                    rank = 0,
                    IsIndexable = false,
                    Name = "double",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeDouble, Name = "double", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.BuiltInMethodID.kAverage
            });
            Methods.Add(new BuiltInMethod
                            {
                Name = "SomeTrue",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeBool,
                    rank = 0,
                    IsIndexable = false,
                    Name = "bool",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.BuiltInMethodID.kSomeTrue
            });
            Methods.Add(new BuiltInMethod()
            {
                Name = "Sleep",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVoid,
                    rank = 0,
                    IsIndexable = false,
                    Name = "void",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("milliseconds", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeInt, Name = "int", IsIndexable = false, rank = 0 }),
                },
                ID = BuiltInMethods.BuiltInMethodID.kSleep
            });
            Methods.Add(new BuiltInMethod
                            {
                Name = "SomeFalse",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeBool,
                    rank = 0,
                    IsIndexable = false,
                    Name = "bool",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.BuiltInMethodID.kSomeFalse
            });
            Methods.Add(new BuiltInMethod
                            {
                Name = "Remove",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = DSASM.Constants.kArbitraryRank,
                    IsIndexable = true,
                    Name = "var",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                    new KeyValuePair<string, ProtoCore.Type>("index", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeInt, Name = "int", IsIndexable = false, rank = 0 })
                },
                ID = BuiltInMethods.BuiltInMethodID.kRemove
            });
            Methods.Add(new BuiltInMethod
                            {
                Name = "RemoveDuplicates",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = DSASM.Constants.kArbitraryRank,
                    IsIndexable = true,
                    Name = "var",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.BuiltInMethodID.kRemoveDuplicates
            });
            Methods.Add(new BuiltInMethod
                            {
                Name = "RemoveNulls",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = ProtoCore.DSASM.Constants.kArbitraryRank,
                    IsIndexable = true,
                    Name = "var",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.BuiltInMethodID.kRemoveNulls
            });
            Methods.Add(new BuiltInMethod
                            {
                Name = "RemoveIfNot",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = ProtoCore.DSASM.Constants.kArbitraryRank,
                    IsIndexable = true,
                    Name = "var",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                    new KeyValuePair<string, ProtoCore.Type>("type", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeString, Name = "string", IsIndexable = false, rank = 0 })
                },
                ID = BuiltInMethods.BuiltInMethodID.kRemoveIfNot
            });
            Methods.Add(new BuiltInMethod
                            {
                Name = "Reverse",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = ProtoCore.DSASM.Constants.kArbitraryRank,
                    IsIndexable = true,
                    Name = "var",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.BuiltInMethodID.kReverse
            });
            Methods.Add(new BuiltInMethod
            {
                Name = "Equals",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeBool,
                    rank = 0,
                    IsIndexable = false,
                    Name = "bool",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("ObjectA", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = false, rank = 0 }),
                    new KeyValuePair<string, ProtoCore.Type>("ObjectB", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = false, rank = 0 }),
                },
                ID = BuiltInMethods.BuiltInMethodID.kEquals
            });
            Methods.Add(new BuiltInMethod
            {
                Name = "Equals",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeBool,
                    rank = 0,
                    IsIndexable = false,
                    Name = "bool",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("ObjectA", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = DSASM.Constants.kArbitraryRank }),
                    new KeyValuePair<string, ProtoCore.Type>("ObjectB", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = DSASM.Constants.kArbitraryRank }),
                },
                ID = BuiltInMethods.BuiltInMethodID.kEquals
            });
            Methods.Add(new BuiltInMethod
                            {
                Name = "Contains",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeBool,
                    rank = 0,
                    IsIndexable = false,
                    Name = "var",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                    new KeyValuePair<string, ProtoCore.Type>("member", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.BuiltInMethodID.kContains
            });
            Methods.Add(new BuiltInMethod
                            {
                Name = "Contains",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeBool,
                    rank = 0,
                    IsIndexable = false,
                    Name = "bool",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                    new KeyValuePair<string, ProtoCore.Type>("member", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = false, rank = 0 })
                },
                ID = BuiltInMethods.BuiltInMethodID.kContains
            });
            Methods.Add(new BuiltInMethod
                            {
                Name = "IndexOf",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeInt,
                    rank = 0,
                    IsIndexable = false,
                    Name = "int",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                    new KeyValuePair<string, ProtoCore.Type>("member", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = false, rank = 0 })
                },
                ID = BuiltInMethods.BuiltInMethodID.kIndexOf
            }); 
            Methods.Add(new BuiltInMethod
                            {
                Name = "IndexOf",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeInt,
                    rank = 0,
                    IsIndexable = false,
                    Name = "int",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                    new KeyValuePair<string, ProtoCore.Type>("member", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.BuiltInMethodID.kIndexOf
            });
            Methods.Add(new BuiltInMethod
                            {
                Name = "Insert",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = ProtoCore.DSASM.Constants.kArbitraryRank,
                    IsIndexable = true,
                    Name = "var",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                    new KeyValuePair<string, ProtoCore.Type>("element", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = false , rank = 0 }),
                    new KeyValuePair<string, ProtoCore.Type>("index", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeInt, Name = "int", IsIndexable = false, rank = 0 })
                },
                ID = BuiltInMethods.BuiltInMethodID.kInsert
            });
            Methods.Add(new BuiltInMethod
                            {
                Name = "Insert",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = ProtoCore.DSASM.Constants.kArbitraryRank,
                    IsIndexable = true,
                    Name = "var",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                    new KeyValuePair<string, ProtoCore.Type>("element", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true , rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                    new KeyValuePair<string, ProtoCore.Type>("index", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeInt, Name = "int", IsIndexable = false, rank = 0 })
                },
                ID = BuiltInMethods.BuiltInMethodID.kInsert
            });
            //Sort, SortWithMode, SortIndexByValue & SortIndexByValueWithMode
            Methods.Add(new BuiltInMethod
            {
                Name = "Sort",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeInt,
                    rank = 1,
                    IsIndexable = true,
                    Name = "int",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeInt, Name = "int", IsIndexable = true, rank = 1 }),
                },
                ID = BuiltInMethods.BuiltInMethodID.kSort
            });
            Methods.Add(new BuiltInMethod
            {
                Name = "Sort",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeInt,
                    rank = 1,
                    IsIndexable = true,
                    Name = "int",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeInt, Name = "int", IsIndexable = true, rank = 1 }),
                    new KeyValuePair<string, ProtoCore.Type>("ascending", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeBool, Name = "bool", IsIndexable = false, rank = 0 }),
                },
                ID = BuiltInMethods.BuiltInMethodID.kSortWithMode
            });
            Methods.Add(new BuiltInMethod
                            {
                Name = "Sort",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeDouble,
                    rank = 1,
                    IsIndexable = true,
                    Name = "double",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeDouble, Name = "double", IsIndexable = true, rank = 1 }),
                },
                ID = BuiltInMethods.BuiltInMethodID.kSort
            });
            Methods.Add(new BuiltInMethod
            {
                Name = "Sort",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeDouble,
                    rank = 1,
                    IsIndexable = true,
                    Name = "double",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeDouble, Name = "double", IsIndexable = true, rank = 1 }),
                    new KeyValuePair<string, ProtoCore.Type>("ascending", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeBool, Name = "bool", IsIndexable = false, rank = 0 }),
                },
                ID = BuiltInMethods.BuiltInMethodID.kSortWithMode
            });
            Methods.Add(new BuiltInMethod
            {
                Name = "Sort",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = 1,
                    IsIndexable = true,
                    Name = "var",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("comparerFunction", new ProtoCore.Type{ UID = (int)PrimitiveType.kTypeFunctionPointer, Name = "function", IsIndexable = false, rank = 0 }),
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = 1 }),
                },
                ID = BuiltInMethods.BuiltInMethodID.kSortPointer
            });
            Methods.Add(new BuiltInMethod
                            {
                Name = "SortIndexByValue",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeInt,
                    rank = 1,
                    IsIndexable = true,
                    Name = "int",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeDouble, Name = "double", IsIndexable = true, rank = 1 }),
                },
                ID = BuiltInMethods.BuiltInMethodID.kSortIndexByValue
            });
            Methods.Add(new BuiltInMethod
            {
                Name = "SortIndexByValue",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeInt,
                    rank = 1,
                    IsIndexable = true,
                    Name = "int",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeDouble, Name = "double", IsIndexable = true, rank = 1 }),
                    new KeyValuePair<string, ProtoCore.Type>("ascending", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeBool, Name = "bool", IsIndexable = false, rank = 0 }),
                },
                ID = BuiltInMethods.BuiltInMethodID.kSortIndexByValueWithMode
            });
            Methods.Add(new BuiltInMethod
                            {
                Name = "Reorder",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = 1,
                    IsIndexable = true,
                    Name = "var",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = 1 }),
                    new KeyValuePair<string, ProtoCore.Type>("indice", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = 1 }),
                },
                ID = BuiltInMethods.BuiltInMethodID.kReorder
            });
            Methods.Add(new BuiltInMethod
                            {
                Name = "IsUniformDepth",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeBool,
                    rank = 0,
                    IsIndexable = false,
                    Name = "bool",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                },
                ID = BuiltInMethods.BuiltInMethodID.kIsUniformDepth
            });
            Methods.Add(new BuiltInMethod
            {
                Name = "IsRectangular",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeBool,
                    rank = 0,
                    IsIndexable = false,
                    Name = "bool",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                },
                ID = BuiltInMethods.BuiltInMethodID.kIsRectangular
            }); 
            Methods.Add(new BuiltInMethod
                            {
                Name = "NormalizeDepth",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = DSASM.Constants.kArbitraryRank,
                    IsIndexable = true,
                    Name = "var",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                },
                ID = BuiltInMethods.BuiltInMethodID.kNormalizeDepth
            });
            Methods.Add(new BuiltInMethod
                            {
                Name = "NormalizeDepth",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = DSASM.Constants.kArbitraryRank,
                    IsIndexable = true,
                    Name = "var",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                    new KeyValuePair<string, ProtoCore.Type>("rank", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeInt, Name = "var", IsIndexable = false, rank = 0 }),
                },
                ID = BuiltInMethods.BuiltInMethodID.kNormalizeDepthWithRank
            }); 
            Methods.Add(new BuiltInMethod
                            {
                Name = "Map",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeDouble,
                    rank = 0,
                    IsIndexable = false,
                    Name = "double",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("rangeMin", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeDouble, Name = "double", IsIndexable = false, rank = 0 }),
                    new KeyValuePair<string, ProtoCore.Type>("rangeMax", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeDouble, Name = "double", IsIndexable = false, rank = 0 }),
                    new KeyValuePair<string, ProtoCore.Type>("inputValue", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeDouble, Name = "double", IsIndexable = false, rank = 0 }),
                    },
                ID = BuiltInMethods.BuiltInMethodID.kMap
            });
            Methods.Add(new BuiltInMethod
                            {
                Name = "MapTo",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeDouble,
                    rank = 0,
                    IsIndexable = false,
                    Name = "var",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("rangeMin", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeDouble, Name = "double", IsIndexable = false, rank = 0 }),
                    new KeyValuePair<string, ProtoCore.Type>("rangeMax", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeDouble, Name = "double", IsIndexable = false, rank = 0 }),
                    new KeyValuePair<string, ProtoCore.Type>("inputValue", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeDouble, Name = "double", IsIndexable = false, rank = 0 }),
                    new KeyValuePair<string, ProtoCore.Type>("targetRangeMin", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeDouble, Name = "double", IsIndexable = false, rank = 0 }),
                    new KeyValuePair<string, ProtoCore.Type>("targetRangeMax", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeDouble, Name = "double", IsIndexable = false, rank = 0 }),
                },
                ID = BuiltInMethods.BuiltInMethodID.kMapTo
            });
            Methods.Add(new BuiltInMethod
            {
                Name = "Transpose",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = DSASM.Constants.kArbitraryRank,
                    IsIndexable = true,
                    Name = "var",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("Array", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = DSASM.Constants.kArbitraryRank }),
                },
                ID = BuiltInMethods.BuiltInMethodID.kTranspose
            });
            Methods.Add(new BuiltInMethod
                            {
                Name = ProtoCore.DSASM.Constants.kFunctionRangeExpression,
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeArray,
                    rank = 0,
                    IsIndexable = true,
                    Name = ProtoCore.DSDefinitions.Kw.kw_array,
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("start", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = false, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                    new KeyValuePair<string, ProtoCore.Type>("end", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = false, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                    new KeyValuePair<string, ProtoCore.Type>("step", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = false, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                    new KeyValuePair<string, ProtoCore.Type>("op", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeInt, Name = "int", IsIndexable = false, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                    new KeyValuePair<string, ProtoCore.Type>("nostep", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeBool, Name = "bool", IsIndexable = false, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.BuiltInMethodID.kRangeExpression
            });
            Methods.Add(new BuiltInMethod()
            {
                Name = "ImportFromCSV",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeDouble,
                    rank = ProtoCore.DSASM.Constants.kArbitraryRank,
                    IsIndexable = true,
                    Name = "double",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("filePath", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeString, Name = "string", IsIndexable = false, rank = 0 }),
                },
                ID = BuiltInMethods.BuiltInMethodID.kLoadCSV
            });
            Methods.Add(new BuiltInMethod()
            {
                Name = "ImportFromCSV",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeDouble,
                    rank = ProtoCore.DSASM.Constants.kArbitraryRank,
                    IsIndexable = true,
                    Name = "double",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("filePath", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeString, Name = "string", IsIndexable = false, rank = 0 }),
                    new KeyValuePair<string, ProtoCore.Type>("transpose", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeBool, Name = "bool", IsIndexable = false, rank = 0 }),
                },
                ID = BuiltInMethods.BuiltInMethodID.kLoadCSVWithMode
            });
            Methods.Add(new BuiltInMethod()
            {
                Name = "Print",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVoid,
                    rank = 0,
                    IsIndexable = false,
                    Name = "void",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("msg", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = false, rank = 0 }),                    
                },
                ID = BuiltInMethods.BuiltInMethodID.kPrint
            });
            Methods.Add(new BuiltInMethod()
            {
                Name = "Print",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVoid,
                    rank = 0,
                    IsIndexable = false,
                    Name = "void",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("msg", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),                    
                },
                ID = BuiltInMethods.BuiltInMethodID.kPrintIndexable
            });
            Methods.Add(new BuiltInMethod()
            {
                Name = "GetElapsedTime",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeInt,
                    rank = 0,
                    IsIndexable = false,
                    Name = "int",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                { },
                ID = BuiltInMethods.BuiltInMethodID.kGetElapsedTime
            });

            // the %dot function
            {
                Methods.Add(new BuiltInMethod
                {
                    Name = ProtoCore.DSASM.Constants.kDotMethodName,
                    ReturnType = new Type
                    {
                        UID = (int)PrimitiveType.kTypeVar,
                        rank = DSASM.Constants.kArbitraryRank,
                        IsIndexable = true,
                        Name = "var",
                    },
                    Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("lhsPtr", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                },
                        ID = BuiltInMethods.BuiltInMethodID.kDot
                    });
            }

            Methods.Add(new BuiltInMethod
            {
                Name = ProtoCore.DSASM.Constants.kDotDynamicResolve,
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = DSASM.Constants.kArbitraryRank,
                    IsIndexable = true,
                    Name = "var",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
            {
                new KeyValuePair<string, ProtoCore.Type>("lhsPtr", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                new KeyValuePair<string, ProtoCore.Type>("functionIndex", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeInt, Name = "int", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
            },
                ID = BuiltInMethods.BuiltInMethodID.kDotDynamic
            });

 

            Methods.Add(new BuiltInMethod
            {
                Name = ProtoCore.DSASM.Constants.kInlineConditionalMethodName,
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = ProtoCore.DSASM.Constants.kArbitraryRank,
                    IsIndexable = true,
                    Name = "var",
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("condition", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeBool, Name = "bool", IsIndexable = false, rank = 0 }),
                    new KeyValuePair<string, ProtoCore.Type>("dyn1", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank }),
                    new KeyValuePair<string, ProtoCore.Type>("dyn2", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank })
                },
                ID = BuiltInMethods.BuiltInMethodID.kInlineConditional
            });


            Methods.Add(new BuiltInMethod
            {
                Name = ProtoCore.DSASM.Constants.kGetTypeMethodName,
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeBool,
                    rank = 0,
                    IsIndexable = false,
                    Name = "bool"
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>> 
                {
                    new KeyValuePair<string, ProtoCore.Type>("object", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = false, rank = 0}),
                },
                ID = BuiltInMethods.BuiltInMethodID.kGetType
            });

            Methods.Add(new BuiltInMethod
            {
                Name = ProtoCore.DSASM.Constants.kGetTypeMethodName,
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeBool,
                    rank = 0,
                    IsIndexable = false,
                    Name = "bool"
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>> 
                {
                    new KeyValuePair<string, ProtoCore.Type>("object", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank}),
                },
                ID = BuiltInMethods.BuiltInMethodID.kGetType
            });

            Methods.Add(new BuiltInMethod
            {
                Name = "ToString",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeString,
                    rank = 0,
                    IsIndexable = false,
                    Name = "string"
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>> 
                {
                    new KeyValuePair<string, ProtoCore.Type>("object", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank}),
                },
                ID = BuiltInMethods.BuiltInMethodID.kToString
            });

            Methods.Add(new BuiltInMethod
            {
                Name = "Break",
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVoid,
                    rank = 0,
                    IsIndexable = false,
                    Name = "void",
                },
                Parameters = new List<KeyValuePair<string,Type>>(),
                ID = BuiltInMethods.BuiltInMethodID.kBreak
            });

            Methods.Add(new BuiltInMethod
            {
                Name = ProtoCore.DSASM.Constants.kImportData,
                ReturnType = new Type
                {
                    UID = (int)PrimitiveType.kTypeVar,
                    rank = ProtoCore.DSASM.Constants.kUndefinedRank,
                    IsIndexable = false,
                    Name = "var"
                },
                Parameters = new List<KeyValuePair<string, ProtoCore.Type>>
                {
                    new KeyValuePair<string, ProtoCore.Type>("appname", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeString, Name = "string", IsIndexable = false, rank = 0}),
                    new KeyValuePair<string, ProtoCore.Type>("connectionParameters", new ProtoCore.Type { UID = (int)PrimitiveType.kTypeVar, Name = "var", IsIndexable = true, rank = ProtoCore.DSASM.Constants.kArbitraryRank})
                },
                ID = BuiltInMethods.BuiltInMethodID.kImportData
            });
        }
    }
}
