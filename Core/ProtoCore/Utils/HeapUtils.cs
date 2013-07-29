using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoCore.DSASM;

namespace ProtoCore.Utils
{
    public static class HeapUtils
    {

        /// <summary>
        /// Take an array of already allocated StackValues and push them into the heap
        /// Returning the stack value that represents the array
        /// </summary>
        /// <param name="arrayElements"></param>
        /// <param name="core"></param>
        /// <returns></returns>
        public static StackValue StoreArray(StackValue[] arrayElements, Core core)
        {
            Heap heap = core.Heap;

            lock (heap.cslock)
            {
                int ptr = heap.Allocate(arrayElements);
                // ++heap.Heaplist[ptr].Refcount;
                StackValue overallSv = StackUtils.BuildArrayPointer(ptr);
                return overallSv;
            }
        }
    }
}
