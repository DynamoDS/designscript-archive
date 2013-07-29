using System;
using System.Diagnostics;
using DesignScript.Instrumentation;
using System.Threading;

namespace DesignScriptStudio.Graph.Core.Services
{
    /// <summary>
    /// Interception class to handle whether logging is called or not
    /// </summary>
    public class Logger
    {
        private static bool loggingEnabled = false;

        public static void EnableLogging(bool enable)
        {
            Logger.loggingEnabled = enable;
        }

        public static void LogInfo(string tag, string data)
        {
            if (!Logger.loggingEnabled)
                return;

            LoggerImpl.LogInfo(tag, data);
        }

        public static void LogDebug(string tag, string data)
        {
            if (!Logger.loggingEnabled)
                return;

            LoggerImpl.LogDebug(tag, data);
        }

        public static void LogPerf(string tag, string data)
        {
            if (!Logger.loggingEnabled)
                return;

            LoggerImpl.LogPerf(tag, data);
        }

        public static void LogError(string tag, string data)
        {
            if (!Logger.loggingEnabled)
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

    /// <summary>
    /// Class to automatically report various metrics of the application usage
    /// </summary>
    public class Heartbeat
    {
        private static Heartbeat instance;
        private const int WARMUP_DELAY_MS = 5000;
        private const int HEARTBEAT_INTERVAL_MS = 60 * 1000;
        private DateTime startTime;
        private Thread heartbeatThread;


        private Heartbeat()
        {
            startTime = DateTime.Now;
            heartbeatThread = new Thread(new ThreadStart(this.ExecThread));
            heartbeatThread.IsBackground = true;
            heartbeatThread.Start();
        }

        public static Heartbeat GetInstance()
        {
            lock (typeof(Heartbeat))
            {
                if (instance == null)
                    instance = new Heartbeat();
            }

            return instance;
        }


        private void ExecThread()
        {
            Thread.Sleep(WARMUP_DELAY_MS);

            while (true)
            {
                Logger.LogInfo("Heartbeat-Uptime-s", DateTime.Now.Subtract(startTime).TotalSeconds.ToString());
                Thread.Sleep(HEARTBEAT_INTERVAL_MS);
            }



        }



    }

}
