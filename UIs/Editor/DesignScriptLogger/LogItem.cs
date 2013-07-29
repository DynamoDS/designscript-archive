using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DesignScript.Instrumentation
{
    public class LogItem
    {
        internal enum Type : uint
        {
            Info,
            Perf,
            Debug,
            Error
        }

        internal LogItem(LogItem.Type itemType, string tag, string data)
        {
            this.ItemType = itemType;
            this.Tag = tag;
            this.Data = data;
        }

        internal LogItem.Type ItemType { get; private set; }
        internal string Tag { get; private set; }
        internal string Data { get; private set; }
    }
}
