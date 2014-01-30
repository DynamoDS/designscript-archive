using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace DSNodeTarget
{
    public class MinimalTracedClass
    {
        private const string __TEMP_REVIT_TRACE_ID = "{0459D869-0C72-447F-96D8-08A7FB92214B}-REVIT";
        private bool wasTraced = false;

        public MinimalTracedClass()
        {
            var retVal = DSNodeServices.TraceUtils.GetTraceData(__TEMP_REVIT_TRACE_ID);

            if (retVal != null)
            {
                wasTraced = true;
            }

            DSNodeServices.TraceUtils.SetTraceData(__TEMP_REVIT_TRACE_ID, new DummyDataHolder());
        }

        public bool WasCreatedWithTrace()
        {
            return wasTraced;
        }


    }

    public class IncrementerTracedClass
    {
        private const string __TEMP_REVIT_TRACE_ID = "{0459D869-0C72-447F-96D8-08A7FB92214B}-REVIT";
        private bool wasTraced = false;

        public IncrementerTracedClass()
        {
            var retVal = DSNodeServices.TraceUtils.GetTraceData(__TEMP_REVIT_TRACE_ID);

            if (retVal != null)
            {
                wasTraced = true;
            }

            DSNodeServices.TraceUtils.SetTraceData(__TEMP_REVIT_TRACE_ID, new DummyDataHolder());
        }

        public bool WasCreatedWithTrace()
        {
            return wasTraced;
        }


    }



    internal class DummyDataHolder : ISerializable
    {
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }
    }

}
