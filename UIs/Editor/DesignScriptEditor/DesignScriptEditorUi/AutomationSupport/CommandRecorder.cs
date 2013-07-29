using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Xml;
using System.ComponentModel;
using System.Diagnostics;
using DesignScript.Editor.Core;

namespace DesignScript.Editor.Automation
{
    public class CommandRecorder : ICommandRecorder
    {
        //HACK: Replace this with a proper data path
        public static StringBuilder AccumulatedLog = new StringBuilder();

        private Stream outputXmlFile = null;
        private StreamWriter xmlStreamWriter = null;
        private XmlSerializer commandSerializer = null;
        private TextEditorCommand previousCommand = null;
        DateTime previousTime = DateTime.Now;

        private int currentCommandIndex = 0;
        private string xmlFilePath = string.Empty;

        public void Record(TextEditorCommand newCommand)
        {
            if (null == outputXmlFile)
                CreateNewRecordSession();

            bool recordPreviousCommand = false;
            if (null != previousCommand) // If there was a previous command...
            {
                // If we have associated any assertions to the previous 
                // command, then we need to persist that information.
                if (null != previousCommand.Asserts)
                    recordPreviousCommand = true;
                else if (newCommand.MethodName != previousCommand.MethodName)
                    recordPreviousCommand = true;
                else
                {
                    // Same command, but not repeatable.
                    if (IsRepeatable(previousCommand) == false)
                        recordPreviousCommand = true;
                }
            }

            if (false != recordPreviousCommand)
                SerializeCommand(previousCommand);

            previousCommand = newCommand;
            if (newCommand.MethodName == TextEditorCommand.Method.CloseScript)
            {
                if (Solution.Current.ScriptCount == 1) // Closing the final script.
                {
                    SerializeCommand(newCommand);
                    previousCommand = null;
                    FinalizeRecordSession();
                }
            }
        }

        internal void AddAssertsToCurrentCommand(List<CommandAssert> asserts)
        {
            if (null != previousCommand)
                previousCommand.AppendAsserts(asserts);
        }

        internal void SetBaseState()
        {
            SerializeCommand(previousCommand);
            previousCommand = null;
            FinalizeRecordSession();
        }

        public long CommandTimer()
        {
            DateTime currentTime = DateTime.Now;
            TimeSpan duration = currentTime - previousTime;
            previousTime = currentTime;
            long milliseconds = ((long)duration.TotalMilliseconds);
            return ((milliseconds > 220) ? 220 : milliseconds);
        }

        #region Private Class Helper Methods

        private void CreateNewRecordSession()
        {
            commandSerializer = new XmlSerializer(typeof(TextEditorCommand));
            string recordPath = Path.GetTempPath() + "\\" + Configurations.RecordFolderName;

            if (!(Directory.Exists(recordPath)))
                Directory.CreateDirectory(recordPath);

            string format = "{0}{1}Recorder-{2:yyyy-MM-dd_hh-mm-ss-tt}.xml";
            xmlFilePath = string.Format(format, recordPath, Path.DirectorySeparatorChar, DateTime.Now);
            outputXmlFile = new FileStream(xmlFilePath, FileMode.Create);
            xmlStreamWriter = new StreamWriter(outputXmlFile);

            string smartFormatAttrib = string.Empty;
            ITextEditorSettings settings = TextEditorControl.Instance.GetEditorSettings();
            if (false != settings.EnableSmartFormatting)
                smartFormatAttrib = "EnableSmartFormatting=\"true\"";

            // Write out XML file header.
            xmlStreamWriter.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            xmlStreamWriter.WriteLine("<Commands xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" "
                + "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" " + smartFormatAttrib + ">");

            currentCommandIndex = 1;
        }

        private void FinalizeRecordSession()
        {
            xmlStreamWriter.Write("\n\n</Commands>");
            xmlStreamWriter.Flush();
            outputXmlFile.Close();
            outputXmlFile = null;
        }

        private void SerializeCommand(TextEditorCommand command)
        {
            if (null == command)
                return;

            command.IntervalTime = CommandTimer(); // Update command time.
            command.CommandNumber = currentCommandIndex++; // Assign a unique index.

            StringWriter writer = new StringWriter();
            commandSerializer.Serialize(writer, command);
            writer.Close();

            string serializedContent = writer.ToString();
            int begin = serializedContent.IndexOf(" CommandNumber");
            string processed = "\n<TextEditorCommand" + serializedContent.Substring(begin);

            Logger.LogInfo("RecorderXML-SC", processed);
            AccumulatedLog.Append(processed);
            xmlStreamWriter.Write(processed);
            xmlStreamWriter.Flush();
        }

        private bool IsRepeatable(TextEditorCommand command)
        {
            switch (command.MethodName)
            {
                case TextEditorCommand.Method.SetMouseMovePosition:
                    return true;
            }

            return false;
        }

        #endregion
    }
}
