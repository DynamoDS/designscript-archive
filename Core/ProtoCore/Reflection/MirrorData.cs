using System.Collections.Generic;
using Autodesk.DesignScript.Interfaces;
using ProtoCore.Utils;

namespace ProtoCore
{
    namespace Mirror
    {
        /// <summary>
        ///  An object that performs marshalling of all relevant data associated with this object
        /// </summary>
        public class MirrorData
        {

            /// <summary>
            ///  The stack value associated with this mirror data
            /// </summary>
            private ProtoCore.DSASM.StackValue svData;


            //
            // Comment Jun:
            // Experimental - have a copy of the core so the data marshaller has access to it
            // The proper solution is to either:
            //      1. Move the MirrorData properties in the RuntimeMirror class or ...
            //      2. Do the data analysis of the MirrorData in the MirrorData class itself
            //
            private ProtoCore.Core core = null;

            /// <summary>
            /// 
            /// </summary>
            private static GraphicDataProvider dataProvider = new GraphicDataProvider();

            /// <summary>
            /// Experimental constructor that takes in a core object
            /// </summary>
            /// <param name="sv"></param>
            public MirrorData(ProtoCore.Core core, ProtoCore.DSASM.StackValue sv)
            {
                this.core = core;
                svData = sv;
            }


            /// <summary>
            ///  Retrieves list of IGraphicItem to get the graphic 
            ///  representation/preview of this Data.
            /// </summary>
            /// <returns>List of IGraphicItem</returns>
            public List<IGraphicItem> GetGraphicsItems()
            {
                List<DSASM.StackValue> values = new List<DSASM.StackValue>();
                GetPointersRecursively(svData, values);

                List<IGraphicItem> graphicItems = new List<IGraphicItem>();
                foreach (var sv in values)
                {
                    List<IGraphicItem> items = dataProvider.GetGraphicItems(sv, this.core);
                    if (items != null && (items.Count > 0))
                        graphicItems.AddRange(items);
                }
                if (graphicItems.Count > 0)
                    return graphicItems;

                return null;
            }

            /// <summary>
            /// Recursively finds all Pointers from the stack value
            /// </summary>
            /// <param name="sv">Stack value</param>
            /// <param name="values">Stack values</param>
            private void GetPointersRecursively(DSASM.StackValue sv, List<DSASM.StackValue> values)
            {
                switch (sv.optype)
                {
                    case ProtoCore.DSASM.AddressType.Pointer:
                        values.Add(sv);
                        break;
                    case ProtoCore.DSASM.AddressType.ArrayPointer:
                        DSASM.StackValue[] stackValues = GetArrayStackValues(sv);
                        foreach (var item in stackValues)
                            GetPointersRecursively(item, values);

                        break;
                    default:
                        break;
                }
            }

            private DSASM.StackValue[] GetArrayStackValues(DSASM.StackValue sv)
            {
                return core.Rmem.GetArrayElements(sv);
            }


            /// <summary>
            ///  Retrieve the stack value for this mirror
            /// </summary>
            /// <returns></returns>
            public ProtoCore.DSASM.StackValue GetStackValue()
            {
                return svData;
            }
        }
    }
}
