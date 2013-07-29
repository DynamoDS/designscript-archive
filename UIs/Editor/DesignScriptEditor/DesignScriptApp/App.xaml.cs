using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using DesignScript.Editor;
using DesignScript.Editor.Core;
using Microsoft.Win32;

using System.IO;
using ProtoScript.Runners;
using ProtoScript.Config;
using DesignScript.Parser;
using ProtoCore.CodeModel;
using System.Text;
using ProtoCore;
using Autodesk.DesignScript.Interfaces;
using System.Xml.Serialization;


namespace DesignScript.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string[] arguments;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(ApplicationUnhandledException);

            try
            {
                ProtoCore.Utils.Validity.AssertExpiry();
                if (e.Args.Length > 0)
                {
                    arguments = e.Args;
                    this.MainWindow = new MainWindow();
                }
                else
                {
                    arguments = null;
                    this.MainWindow = new MainWindow();
                }
            }
            catch (Exception exception)
            {
                HandleExceptionInternal(exception);
            }
            finally
            {
                if (null == this.MainWindow || MainWindow.ShowDialog() == false)
                    this.Shutdown();
            }
        }

        // This method is to catch global application exception (defect IDE-133).
        void ApplicationUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleExceptionInternal(e.ExceptionObject);
        }

        void HandleExceptionInternal(object exceptionObject)
        {
            if (exceptionObject is ProtoCore.Utils.ProductExpiredException)
            {
                Exception exception = exceptionObject as Exception;
                MessageBox.Show(exception.Message, "Product Expired", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
            else if (null != exceptionObject)
            {
                string message = "An exception has occurred and DesignScript Studio will now close.";

                Exception exception = exceptionObject as Exception;
                if (null != exception)
                {
                    message += "\n\n" + exception.GetType().ToString();
                    message += "\n" + exception.Message;
                }

                MessageBox.Show(message, "Runtime Exception", MessageBoxButton.OK, MessageBoxImage.Stop);
                Logger.LogError("Runtime-Exception", exceptionObject.ToString());
            }
        }
    }

    class HostApplication : IHostApplication
    {
        string[] arguments = null;

        public HostApplication()
        {
            Application.Current.Exit += new ExitEventHandler(OnExit);
        }

        void OnExit(object sender, ExitEventArgs e)
        {
            if (null != BeginQuit)
                BeginQuit(sender, e);
        }

        public HostApplication(string[] args)
        {
            arguments = args;
            Application.Current.Exit += new ExitEventHandler(OnExit);
        }

        #region IHostApplication Members

        public string PromptScriptSelection()
        {
            string fileFilter = "DesignScript Files (*.ds)|*.ds" +
                "|All Files (*.*)|*.*";

            // Displays an OpenFileDialog for user to select a source file.
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = fileFilter;
            openFileDialog.Title = "Select a DesignScript File";

            bool? result = openFileDialog.ShowDialog();
            if (result.HasValue && (result.Value == true))
                return openFileDialog.FileName;

            return null;
        }

        public string[] GetApplicationArguments()
        {
            return arguments;
        }

        public bool PreExecutionSetup(bool debugMode, ref string errorMessage)
        {
            return true;
        }

        public bool PreStepSetup(ref string errorMessage)
        {
            return true;
        }

        public void PostStepTearDown()
        {
        }

        public void PostExecutionTearDown()
        {
        }

        public void QuitApplication()
        {
            Application.Current.Shutdown();
        }

        public event EventHandler BeginQuit;

        #endregion

        public bool GetContextualMenu(Dictionary<int, string> menuItems, int line, int column, ITextBuffer context)
        {
            return false;
        }

        public bool HandleMenuItemClick(int id)
        {
            return false;
        }

        public Dictionary<string, object> Configurations
        {
            get
            {
                return null;
            }
        }
    }
}
