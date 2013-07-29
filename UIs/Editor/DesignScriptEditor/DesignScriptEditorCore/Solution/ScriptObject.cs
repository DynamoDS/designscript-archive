using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DesignScript.Parser;
using System.IO;
using Microsoft.Win32;
using ProtoCore;

namespace DesignScript.Editor.Core
{
    class ScriptObject : IScriptObject
    {
        private IParsedScript parsedScript = null;
        private ScriptState scriptState = null;
        private IDialogProvider dialog = TextEditorCore.DialogProvider;
        private FileSystemWatcher fileWatcher = null;

        #region IScriptObject Members

        public bool SaveScript(bool saveAs)
        {
            if (parsedScript == null)
                return false;
            string filePath = parsedScript.GetScriptPath();

            if (false == saveAs) // This is just a simple save. 
            {
                if (scriptState.textBuffer.ScriptModified == false)
                    return true; // Nothing has changed, no need! 
            }

            if ((File.Exists(filePath) &&
                ((File.GetAttributes(filePath) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)) || SystemFile(filePath))
            {
                if (saveAs == false)
                {
                    if (!SystemFile(filePath))
                    {
                        ReadOnlyDialogResult result = dialog.ShowReadOnlyDialog(true);

                        if (result == ReadOnlyDialogResult.OverWrite)
                        {
                            FileAttributes attributes = File.GetAttributes(filePath);
                            File.SetAttributes(filePath, attributes ^ FileAttributes.ReadOnly);
                        }
                        else if (result == ReadOnlyDialogResult.SaveAs)
                            saveAs = true;
                        else
                            return false;
                    }
                    else
                    {
                        ReadOnlyDialogResult result = dialog.ShowReadOnlyDialog(false);
                        if (result == ReadOnlyDialogResult.SaveAs)
                            saveAs = true;
                        else
                            return false;
                    }
                }
            }

            if (filePath == "" || saveAs)
            {
                string newFilePath = null;
                while (true)
                {
                    newFilePath = PromptScriptSave();
                    if (string.IsNullOrEmpty(newFilePath))
                        return false;

                    int newFileIndex = Solution.Current.GetScriptIndexFromPath(newFilePath);
                    if (newFileIndex != -1 && newFileIndex != Solution.Current.ActiveScriptIndex)
                        dialog.ShowFileAlreadyOpenDialog();
                    else
                        break;
                }

                // The check for the filePath is to see if the file is
                // being saved into the same file or a new file.
                if (!string.Equals(filePath, newFilePath))
                {
                    scriptState.textBuffer.ScriptModified = true;
                    filePath = newFilePath;
                    InitializeFileWatcher(newFilePath);
                }



                if (null != newFilePath)
                    TextEditorCore.Instance.Data.AddToRecentFileList(newFilePath);
            }

            if (fileWatcher != null)
                fileWatcher.EnableRaisingEvents = false;

            try
            {
                StreamWriter streamWriter = new StreamWriter(filePath);
                string fileContent = scriptState.textBuffer.GetContent();
                fileContent = fileContent.Replace("\n", "\r\n");
                streamWriter.Write(fileContent);
                streamWriter.Close();
            }
            catch (Exception)
            {
                IDialogProvider dialog = TextEditorCore.DialogProvider;
                string message = string.Format("Could not access file '{0}', please try again.", filePath);
                dialog.DisplayStatusMessage(StatusTypes.Warning, message, 5);
                return false;
            }

            if (fileWatcher != null)
                fileWatcher.EnableRaisingEvents = true;

            bool parseResult = parsedScript.ParseScript(filePath);
            scriptState.textBuffer.ScriptModified = false;

            return parseResult;
        }

        public CharPosition CreateCharPosition()
        {
            return (new CharPosition(scriptState.textBuffer));
        }

        public ITextBuffer GetTextBuffer()
        {
            return scriptState.textBuffer;
        }

        public IParsedScript GetParsedScript()
        {
            return this.parsedScript;
        }

        #endregion

        #region Public Class Operational Methods

        internal ScriptObject(IParsedScript parsedScript)
        {
            this.ScriptModifiedExternal = false;
            this.parsedScript = parsedScript;
            scriptState = new ScriptState(new TextBuffer(this));

            if (null == this.parsedScript)
                throw new InvalidOperationException("Invalid 'parsedScript'!");

            InitializeFileWatcher(parsedScript.GetScriptPath());
        }

        internal ScriptObject(string fileContent)
        {
            this.ScriptModifiedExternal = false;
            this.parsedScript = InterfaceFactory.CreateParsedScript();

            if (string.IsNullOrEmpty(fileContent) == false)
            {
                MemoryStream inputStream = new MemoryStream(
                    Encoding.Default.GetBytes(fileContent));

                this.parsedScript.ParseStream(inputStream);
            }

            scriptState = new ScriptState(new TextBuffer(fileContent));
        }

        internal void DestroyScript()
        {
            if (null != this.parsedScript)
            {
                this.parsedScript.DestroyScript();
                this.parsedScript = null;
            }
        }

        #endregion

        #region Public Class Properties

        internal ScriptState States { get { return scriptState; } }

        public bool ScriptModifiedExternal { get; set; }

        #endregion

        #region Private Class Helper Methods

        private string PromptScriptSave()
        {
            string fileName = "Document";
            if (null != parsedScript)
            {
                string filePath = parsedScript.GetScriptPath();
                fileName = Path.GetFileNameWithoutExtension(filePath);
            }
            // Configure save file dialog box

            IDialogProvider saveDialogProvider = TextEditorCore.DialogProvider;
            if (saveDialogProvider.ShowSaveFileDialog(ref fileName))
                return fileName;

            return null;
        }

        private bool InitializeFileWatcher(string scriptPath)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(scriptPath);
                if (fileWatcher == null)
                {
                    fileWatcher = new FileSystemWatcher(fileInfo.DirectoryName);
                    fileWatcher.Filter = fileInfo.Name;
                    fileWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
                    fileWatcher.Changed += new FileSystemEventHandler(OnFileModifiedExternally);
                    fileWatcher.Deleted += new FileSystemEventHandler(OnFileModifiedExternally);
                    fileWatcher.Renamed += new RenamedEventHandler(OnFileModifiedExternally);
                    fileWatcher.EnableRaisingEvents = true;
                }
                else
                {
                    fileWatcher.Path = fileInfo.DirectoryName;
                    fileWatcher.Filter = fileInfo.Name;
                }
                return true;
            }

            catch (FileNotFoundException e)
            {
                throw new FileNotFoundException(e.FileName + " does not exist!");
            }
        }

        private void OnFileModifiedExternally(object source, FileSystemEventArgs e)
        {
            this.ScriptModifiedExternal = true;
        }

        private bool SystemFile(string filepath)
        {
            string executingAssemblyPathName = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string rootModuleDirectory = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(executingAssemblyPathName));

            if (null == rootModuleDirectory || null == filepath)
                return false;

            if (filepath.ToLower().StartsWith(rootModuleDirectory.ToLower()))
                return true;
            else
                return false;
        }

        #endregion
    }
}
