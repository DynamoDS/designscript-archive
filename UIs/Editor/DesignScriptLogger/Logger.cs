using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace DesignScript.Instrumentation
{
    public class LoggerImpl : ParallelGateway<LogItem>
    {
        #region Private Class Data Members

        private static LoggerImpl instance = null;
        private net.riversofdata.dhlogger.Log log = null;

        #endregion

        #region Public Static Logging Methods

        public static void LogInfo(string tag, string data)
        {
            GetInstance().AppendWorkItem(new LogItem(LogItem.Type.Info, tag, data));
        }

        public static void LogDebug(string tag, string data)
        {
            GetInstance().AppendWorkItem(new LogItem(LogItem.Type.Debug, tag, data));
        }

        public static void LogPerf(string tag, string data)
        {
            tag = "Perf-" + tag;
            GetInstance().AppendWorkItem(new LogItem(LogItem.Type.Perf, tag, data));
        }

        public static void LogError(string tag, string data)
        {
             GetInstance().AppendWorkItem(new LogItem(LogItem.Type.Error, tag, data));
        }


        /// <summary>
        /// Log an item ignoring the ColectFeedback settings
        /// This method must not be used for anything except insturmentation
        /// on/off reporting without explicit permission from the user
        /// Uses of this method must be approved by TL + Legal
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="data"></param>
        public static void FORCE_Log(string tag, string data)
        {
            GetInstance().AppendWorkItem(new LogItem(LogItem.Type.Info, tag, data));
        }



        public static void CancelProcessing()
        {
            GetInstance().CancelWorkInternal();
        }

        #endregion

        #region Protected Overridable Methods

        protected override bool HandleWorkTask(LogItem logItem)
        {
            switch (logItem.ItemType)
            {
                case LogItem.Type.Info:
                case LogItem.Type.Perf:
                    log.Info(logItem.Tag, logItem.Data);
                    break;

                case LogItem.Type.Debug:
                    log.Debug(logItem.Tag, logItem.Data);
                    break;

                case LogItem.Type.Error:
                    log.Error(logItem.Tag, logItem.Data);
                    break;
            }

            return true; // Continue to the next item, please.
        }

        protected override void OutputDiagnostics(LogItem workItem)
        {
            System.Diagnostics.Debug.WriteLine(workItem.Tag + " " + workItem.Data);
        }

        #endregion

        #region Private Class Helper Methods

        private LoggerImpl()
        {
            string userID = GetUserID();
            string sessionID = Guid.NewGuid().ToString();

            this.EnableDiagnosticsOutput = false;
            log = new net.riversofdata.dhlogger.Log("DesignScript-Studio", userID, sessionID);
        }

        private String GetUserID()
        {
            // The name of the key must include a valid root.
            const string userRoot = "HKEY_CURRENT_USER";
            const string subkey = "Software\\DesignScriptStudioUXG";
            const string keyName = userRoot + "\\" + subkey;

            // An int value can be stored without specifying the
            // registry data type, but long values will be stored
            // as strings unless you specify the type. Note that
            // the int is stored in the default name/value
            // pair.

            String tryGetValue = Registry.GetValue(keyName, "InstrumentationGUID", null) as String;

            if (tryGetValue != null)
            {
                System.Diagnostics.Debug.WriteLine("User id found: " + tryGetValue);
                return tryGetValue;
            }
            else
            {
                String newGUID = Guid.NewGuid().ToString();
                Registry.SetValue(keyName, "InstrumentationGUID", newGUID);
                System.Diagnostics.Debug.WriteLine("New User id: " + newGUID);
                return newGUID;
            }
        }

        private static LoggerImpl GetInstance()
        {
            lock (typeof(LoggerImpl))
            {
                if (instance == null)
                    instance = new LoggerImpl();

                return instance;
            }
        }

        #endregion
    }

}
