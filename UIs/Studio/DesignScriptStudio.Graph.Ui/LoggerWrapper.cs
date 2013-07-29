using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DesignScriptStudio.Graph.Ui
{
    using DesignScriptStudio.Graph.Core.Services;
    using DesignScript.Editor.Common;

    public class LoggerWrapper : ILoggerWrapper
    {
        public void EnableLogging(bool enable)
        {
            Logger.EnableLogging(enable);
        }

        public void LogInfo(string tag, string data)
        {
            Logger.LogInfo(tag, data);
        }

        public void LogDebug(string tag, string data)
        {
            Logger.LogDebug(tag, data);
        }

        public void LogPerf(string tag, string data)
        {
            Logger.LogPerf(tag, data);
        }

        public void LogError(string tag, string data)
        {
            Logger.LogError(tag, data);
        }

        public void FORCE_Log(string tag, string data)
        {
            Logger.FORCE_Log(tag, data);
        }

        public void CancelProcessing()
        {
            Logger.CancelProcessing();
        }
    }
}
