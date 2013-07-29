using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace DesignScriptLauncher
{
    public class Startup
    {
        public static void Main(string[] args)
        {
            if (null != args && args.Length > 0)
            {
                if (string.Compare(args[0], "EULA", true) == 0)
                {
                    string tmpdir = System.IO.Path.GetTempPath();
                    string eulafile = Path.Combine(tmpdir, "DesignScriptEULA.rtf");
                    UpdateEULAContent(eulafile);
                    System.Diagnostics.Process.Start(eulafile);
                }
                else if (string.Compare(args[0], "APP", true) == 0)
                {
                    string apppath = GetDesignScriptAppLocation("DesignScript.App.exe");
                    LaunchProcess(apppath, string.Empty);
                }
                else if (string.Compare(args[0], "STUDIO", true) == 0)
                {
                    string studioPath = GetDesignScriptAppLocation("DesignScriptStudio.App.exe");
                    LaunchProcess(studioPath, string.Empty);
                }
                return;
            }

            string exepath = GetAcadLocationFromRegistry();
            string argument = string.Empty;

            if (!string.IsNullOrEmpty(exepath) && File.Exists(exepath))
            {
                //Create sentinal file to launch Editor in AutoCAD
                string sentinalpath = Path.GetTempPath();
                sentinalpath = Path.Combine(sentinalpath, @"DesignScriptLaunched");
                if (!File.Exists(sentinalpath))
                {
                    using (FileStream fs = File.Create(sentinalpath))
                    {
                        fs.Close();
                    }
                }
            }
            else
            {
                exepath = GetDesignScriptAppLocation("DesignScript.App.exe");
                bool launchexe = File.Exists(exepath);
                string message = string.Format(Resource.AutoCADNotInstalled, ""); ;
                if (!launchexe)
                {
                    System.Windows.Forms.MessageBox.Show(message, Resource.ErrorCaption, System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    return;
                }
            }

            LaunchProcess(exepath, argument);
        }

        private static void UpdateEULAContent(string eulafile)
        {
            try
            {
                using(StreamWriter writer = new StreamWriter(eulafile, false))
                {
                    writer.Write(Resource.EULATEXT);
                }
            }
            catch (IOException)
            {
                //File was in use, probably it's already open.
            }
        }

        /// <summary>
        /// Launches given exe with the given argument.
        /// </summary>
        /// <param name="exepath">Path of the executable to launch</param>
        /// <param name="argument">Argument for executable, can be empty string</param>
        private static void LaunchProcess(string exepath, string argument)
        {
            if (string.IsNullOrEmpty(exepath))
                return;

            ProcessStartInfo processinfo = new ProcessStartInfo(exepath);
            if (!string.IsNullOrEmpty(argument))
                processinfo.Arguments = argument;

            //Display error dialog for diagnostics
            processinfo.ErrorDialog = true;
            //Start from temp working directory
            processinfo.WorkingDirectory = Path.GetTempPath();

            //Finally launch the process
            System.Diagnostics.Process.Start(processinfo);
        }

        /// <summary>
        /// Returns location of DesignScript App. By default it should be
        /// present at the location where DesignScriptLauncher is present.
        /// </summary>
        /// <returns></returns>
        private static string GetDesignScriptAppLocation(string appName)
        {
            string assemblypath = Assembly.GetExecutingAssembly().Location;
            string installdir = Path.GetDirectoryName(assemblypath);
            assemblypath = Path.Combine(installdir, appName);
            if (!File.Exists(assemblypath))
                System.Windows.Forms.MessageBox.Show(string.Format("DesignScript App is not found at {0}", assemblypath), "DesginScript Launch Error");

            return assemblypath;
        }

        /// <summary>
        /// Generates DesignScriptStartup.scr script at temp path and returns
        /// acad argument string to run the script at startup.
        /// </summary>
        /// <returns>acad argument string for startup</returns>
        private static string GetAcadStartupArgumentString()
        {
            //create a temp file to execute with acad.exe to open editor with it
            string filePath = System.IO.Path.GetTempPath() + "DesignScriptStartup.scr";
            if (!File.Exists(filePath))
            {
                StreamWriter writer = File.AppendText(filePath);
                writer.WriteLine("_EDITOR");
                writer.Close();
            }

            return "/b " + filePath;
        }

        /// <summary>
        /// Looks at HKLM\SOFTWARE\Autodesk\AutoCAD\R19.0 registry entry and
        /// finds AcadLocation key to get the acad exe path.
        /// </summary>
        /// <returns>path of acad exe or an empty string.</returns>
        private static string GetAcadLocationFromRegistry()
        {
            //match with this regex
            var rex = new Regex("^ACAD-.*");
            string acadLocation = string.Empty;

            RegistryKey acadKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Autodesk\\AutoCAD");
            if (null == acadKey)
                return acadLocation;

            string[] versions = acadKey.GetSubKeyNames();
            if (versions == null)
                return acadLocation;

            Array.Sort(versions);
            for (int i = versions.Count() - 1; i >= 0; --i)
            {
                string versionKey = versions[i];

                RegistryKey key = acadKey.OpenSubKey(versionKey);
                if (null == key)
                    continue;

                string[] subkeys = key.GetSubKeyNames();
                if (null == subkeys || subkeys.Length == 0)
                    continue;

                //search all subkeys for regex match
                foreach (string subkey in subkeys)
                {
                    if (rex.Match(subkey).Success)
                        acadLocation = key.OpenSubKey(subkey).GetValue("AcadLocation") as string;
                    if (!string.IsNullOrEmpty(acadLocation))
                    {
                        acadLocation = Path.Combine(acadLocation, @"acad.exe");
                        if (File.Exists(acadLocation))
                            return acadLocation;
                    }
                }
            }
            return string.Empty;
        }
    }
}
