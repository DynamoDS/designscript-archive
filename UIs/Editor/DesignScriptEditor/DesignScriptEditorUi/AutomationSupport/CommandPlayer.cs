using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Windows.Threading;
using System.Xml;
using DesignScript.Editor.Core;
using System.Reflection;

namespace DesignScript.Editor.Automation
{
    class CommandPlayer
    {
        private List<AutomationResult> testResults = null;
        private List<AssertionResult> assertions = null;
        private ITextEditorCore textEditorCore = null;
        private TextEditorControl textEditorControl = null;

        private bool playbackComplete = false;
        private string storageFolder = string.Empty;
        private DispatcherTimer playbackTimer = null;

        private int commandIndex = -1;
        private int currentXmlFile = -1;
        private string currentTestName = string.Empty;
        private List<string> xmlTestFiles = null;
        private List<TextEditorCommand> editorCommands = null;

        internal CommandPlayer(TextEditorControl control, ITextEditorCore core)
        {
            textEditorControl = control;
            textEditorCore = core;
            textEditorCore.SetCommandRecorder(null);
            textEditorCore.EnableRegularCommands = false;
        }

        internal void SetApplicationArguments(string[] args)
        {
            xmlTestFiles = new List<string>();

            if (null != args && (args.Length > 1))
            {
                if (args[0] == "/t")
                    xmlTestFiles.Add(args[1]);
                else if (args[0] == "/p")
                {
                    string[] files = Directory.GetFiles(args[1],
                        "test.*.xml", SearchOption.AllDirectories);
                    xmlTestFiles.AddRange(files);
                }
            }

            storageFolder = System.IO.Path.GetTempPath();
            if (null != args && (args.Length >= 4))
                storageFolder = args[3];
        }

        internal void BeginPlayback()
        {
            if (null == xmlTestFiles || (xmlTestFiles.Count <= 0))
                return; // Nothing to playback from.

            currentXmlFile = -1;
            PlaybackNextXmlFile();
        }

        internal void PausePlayback(bool pausePlayback)
        {
            if (null != playbackTimer)
                playbackTimer.IsEnabled = !pausePlayback;
        }

        internal void PlaybackNextCommand()
        {
            if (null == playbackTimer || (playbackTimer.IsEnabled == true))
                return; // Timer is still enabled, leave it to do its job.

            PlaybackCommandInternal();
        }

        private bool PlaybackNextXmlFile()
        {
            bool donePlayingAllXmlFiles = false;
            currentXmlFile = currentXmlFile + 1;
            if (currentXmlFile < 0 || (null == xmlTestFiles))
                donePlayingAllXmlFiles = true;
            if (currentXmlFile >= xmlTestFiles.Count)
                donePlayingAllXmlFiles = true;

            if (false != donePlayingAllXmlFiles)
            {
                if (null != playbackTimer)
                    playbackTimer.Stop();

                playbackTimer = null;
                SerialiseTestResults();
                textEditorCore.EnableRegularCommands = true;

                // Close out the editor for multi-playback.
                if (xmlTestFiles.Count > 1)
                    System.Windows.Application.Current.Shutdown();

                return false;
            }

            string xmlFilePath = xmlTestFiles[currentXmlFile];
            if (File.Exists(xmlFilePath) == false)
                return false;

            if (null == editorCommands)
                editorCommands = new List<TextEditorCommand>();
            editorCommands.Clear(); // Clear up any existing steps.

            string baseFilePath = FindMatchingBaseFile(xmlFilePath);
            if (string.IsNullOrEmpty(baseFilePath) == false)
            {
                Commands baseSteps = DeserializeFromXml(baseFilePath);
                editorCommands.AddRange(baseSteps.TextEditorCommands);
            }

            Commands recordedSteps = DeserializeFromXml(xmlFilePath);
            ITextEditorSettings settings = textEditorCore.TextEditorSettings;
            if (settings.EnableSmartFormatting != recordedSteps.EnableSmartFormatting)
                settings.ToggleSmartFormatting(); // Formatting on a per-test basis.

            editorCommands.AddRange(recordedSteps.TextEditorCommands);
            PlaybackVisualizer.Instance.SetCurrentFilePath(xmlFilePath);
            PlaybackVisualizer.Instance.SetEditorCommands(editorCommands);

            if (null == playbackTimer)
            {
                playbackTimer = new DispatcherTimer();
                playbackTimer.Tick += new EventHandler(OnPlaybackTimerTick);
                playbackTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            }

            commandIndex = -1;
            playbackComplete = false;
            playbackTimer.Start();
            Solution.CloseSolution(Solution.Current, true);
            return true;
        }

        private Commands DeserializeFromXml(string filePath)
        {
            Commands commandsList = null;
            TextReader textReader = new StreamReader(filePath);

            try
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreWhitespace = false;
                XmlReader reader = XmlReader.Create(textReader, settings);

                XmlSerializer deserializer = new XmlSerializer(typeof(Commands));
                commandsList = (Commands)deserializer.Deserialize(reader);
            }
            finally
            {
                textReader.Close();
            }

            return commandsList;
        }

        private string FindMatchingBaseFile(string xmlFilePath)
        {
            string xmlFileName = Path.GetFileName(xmlFilePath);
            string[] parts = xmlFileName.Split(new char[] { '.' });
            if (null == parts || (parts.Length <= 2))
                return string.Empty;

            string baseIndex = parts[2];
            currentTestName = xmlFileName;

            int dummyIndex = -1;
            if (int.TryParse(baseIndex, out dummyIndex)) // There's a base file.
            {
                string directory = Path.GetDirectoryName(xmlFilePath);
                if (directory.EndsWith(@"\") == false)
                    directory += @"\";

                string baseFilePath = string.Format("{0}base.{1}.xml", directory, baseIndex);
                if (File.Exists(baseFilePath))
                    return baseFilePath;
            }

            return string.Empty;
        }

        private bool PlaybackSingleCommand()
        {
            if (null == assertions)
                assertions = new List<AssertionResult>();

            commandIndex = commandIndex + 1;
            if (commandIndex >= editorCommands.Count)
            {
                if (xmlTestFiles.Count > 1) // Multi-playback mode.
                {
                    if (null == testResults)
                        testResults = new List<AutomationResult>();

                    testResults.Add(new AutomationResult(currentTestName, assertions));
                }

                playbackComplete = true;
                assertions = null;
                return false; // Done playing back.
            }

            TextEditorCommand command = editorCommands[commandIndex];
            int commandInterval = (int)command.IntervalTime;
            commandInterval = ((commandInterval > 220) ? 220 : commandInterval);
            playbackTimer.Interval = TimeSpan.FromMilliseconds(commandInterval);
            bool result = textEditorCore.PlaybackCommand(command);

            PlaybackVisualizer.Instance.SetCurrentCommand(command);

            switch (command.MethodName)
            {
                case TextEditorCommand.Method.LoadScriptFromFile:
                    if (!result)
                    {
                        assertions = new List<AssertionResult>();
                        assertions.Add(new AssertionResult("Fail", "0", "Invalid path! Cannot load script!"));
                        if (null == testResults)
                            testResults = new List<AutomationResult>();
                        testResults.Add(new AutomationResult(currentTestName, assertions));
                        playbackComplete = true;
                        assertions = null;
                        TextEditorCommand closeScriptCommand = new TextEditorCommand(TextEditorCommand.Method.CloseScript);
                        closeScriptCommand.AppendArgument(0);
                        int scriptCount = textEditorControl.ScriptTabControl.TabCount;
                        bool closeScriptResult = textEditorCore.PlaybackCommand(closeScriptCommand);
                        while (--scriptCount != 0)
                            closeScriptResult = textEditorCore.PlaybackCommand(closeScriptCommand);
                        textEditorControl.ScriptTabControl.CloseAllTabs();
                        textEditorControl.ShowTabControls(false);

                        return false; // Exit playback.
                    }
                    textEditorControl.SetupTabInternal(command.Arguments[0] as string);
                    break;

                case TextEditorCommand.Method.CreateNewScript:
                    textEditorControl.SetupTabInternal(null);
                    break;

                case TextEditorCommand.Method.CloseScript:
                    textEditorControl.PlaybackCloseTab((int)command.Arguments[0]);
                    break;

                case TextEditorCommand.Method.ChangeScript:
                    textEditorControl.PlaybackSwitchTab((int)command.Arguments[0]);
                    break;

                case TextEditorCommand.Method.Step:
                    textEditorControl.UpdateUiForStepNext(result);
                    break;

                case TextEditorCommand.Method.Run:
                    textEditorControl.UpdateUiForRun(result);
                    break;

                case TextEditorCommand.Method.Stop:
                    textEditorControl.UpdateUiForStop(result);
                    break;
            }

            textEditorControl.UpdateScriptDisplay(Solution.Current.ActiveScript);
            textEditorControl.UpdateCaretPosition();

            if (null != command.Asserts)
                DoAssertions(command.Asserts);

            return true;
        }

        private void DoAssertions(List<CommandAssert> asserts)
        {
            IAssertableProperties assertable = textEditorCore.GetAssertableProperties();

            Type type = typeof(IAssertableProperties);
            foreach (CommandAssert assert in asserts)
            {
                string queriedValue = string.Empty;

                try
                {
                    PropertyInfo property = type.GetProperty(assert.PropertyName);
                    object propertyValue = (property.GetValue(assertable, null));
                    queriedValue = propertyValue.ToString();
                }
                catch (Exception exception)
                {
                    queriedValue = exception.Message;
                }
                finally
                {
                    if (queriedValue == assert.PropertyValue)
                    {
                        string result = "Property: " + assert.PropertyName + '\n' +
                                        "Expected: " + assert.PropertyValue + '\n' +
                                        "Actual: " + queriedValue + '\n';

                        assert.Passed = true;
                        assertions.Add(new AssertionResult("Pass", assert.AssertNumber.ToString(), result));
                    }
                    else
                    {
                        string result = "Property: " + assert.PropertyName + '\n' +
                                        "Expected: " + assert.PropertyValue + '\n' +
                                        "Actual: " + queriedValue + '\n';

                        assert.Passed = false;
                        assertions.Add(new AssertionResult("Fail", assert.AssertNumber.ToString(), result));
                    }
                }
            }
        }

        private void OnPlaybackTimerTick(object sender, EventArgs e)
        {
            PlaybackCommandInternal();
        }

        private void PlaybackCommandInternal()
        {
            if (false != playbackComplete)
            {
                playbackTimer.Stop();
                PlaybackNextXmlFile();
                return;
            }

            PlaybackSingleCommand();
        }

        private void SerialiseTestResults()
        {
            IHostApplication application = TextEditorControl.HostApplication;
            XmlSerializer serializer = new XmlSerializer(typeof(List<AutomationResult>));

            string outputFilePath = string.Format("{0}{1}Asserts-{2:yyyy-MM-dd}.xml",
                storageFolder, Path.DirectorySeparatorChar, DateTime.Now);

            Stream xmlFileStream = new FileStream(outputFilePath, FileMode.Create);
            TextWriter textWriter = new StreamWriter(xmlFileStream);
            serializer.Serialize(textWriter, testResults);
            textWriter.Close();
        }
    }
}
