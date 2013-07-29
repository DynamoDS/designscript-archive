using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DesignScriptStudio.Graph.Core.Services;

namespace DesignScriptStudio.Graph.Core
{
    /// <summary>
    /// Class to handle all errors generated within the system
    /// </summary>
    internal class ErrorManager
    {
        /// <summary>
        /// Record an internal error that represents an invalid system state
        /// </summary>
        /// <param name="e"></param>
        private static void LogInternalError(Exception e)
        {
            LogInternalError(e.Message);
        }

        /// <summary>
        /// Record an internal error that represents an invalid system state
        /// </summary>
        /// <param name="message">The message to be logged</param>
        private static void LogInternalError(String message)
        {
            DateTime dt = DateTime.Now;
            System.Diagnostics.Debug.WriteLine(dt.ToString("yyyy-MM-dd HH:mm:ss - " + message));

            Logger.LogError("Error", message);

        }

        /// <summary>
        /// Process an exception
        /// </summary>
        /// <param name="e"></param>
        public static void HandleError(Exception e)
        {
            LogInternalError(e);
            throw e;
        }
    }
}
