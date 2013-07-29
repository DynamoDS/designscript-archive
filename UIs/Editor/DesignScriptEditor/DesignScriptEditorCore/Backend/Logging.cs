using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using DesignScript.Instrumentation;
using Microsoft.Win32;

namespace DesignScript.Editor.Core
{


    public class Logger
    {
                
        public static void LogInfo(string tag, string data)
        {
            //If logging is disabled
            if (!TextEditorCore.Instance.TextEditorSettings.CollectFeedback)
                return;
            
            LoggerImpl.LogInfo(tag, data);
        }

        public static void LogDebug(string tag, string data)
        {
            //If logging is disabled
            if (!TextEditorCore.Instance.TextEditorSettings.CollectFeedback)
                return;


            LoggerImpl.LogDebug(tag, data);
        }

        public static void LogPerf(string tag, string data)
        {
            //If logging is disabled
            if (!TextEditorCore.Instance.TextEditorSettings.CollectFeedback)
                return;

            LoggerImpl.LogPerf(tag, data);
        }

        public static void LogError(string tag, string data)
        {
            //If logging is disabled
            if (!TextEditorCore.Instance.TextEditorSettings.CollectFeedback)
                return;

            LoggerImpl.LogError(tag, data);
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
            LoggerImpl.FORCE_Log(tag, data);
        }



        public static void CancelProcessing()
        {
            LoggerImpl.CancelProcessing();
        }
    }

}
