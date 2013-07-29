using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DesignScript.Editor.Automation
{
    using System.Windows.Input;
    using DesignScript.Editor.Core;
    using System.Windows.Threading;

    class Generator
    {
        string currentFilePath;
        int maxActions = 10000;
        TextEditorCommand command = null;
        ITextEditorCore textEditorCore = null;
        TextEditorControl textEditorControl = null;
        string filePath = @"C:\Users\t_sekas\Desktop\test1.bat";
        Random random;

        // For MadTypistBeingMad
        StreamReader streamReader = null;
        string[] fileList = null;
        int madTypingCurrentFile = 0;
        DispatcherTimer dispatchTimer = null;
        const int minTime = 100;
        const int maxTime = 300;

        public Generator(string[] arguments, ITextEditorCore textCore, TextEditorControl textEditorControl)
        {
            textEditorCore = textCore;
            this.textEditorControl = textEditorControl;

            if (arguments.Length > 2 && arguments[1] == "/path")
            {
                filePath = arguments[2];
                MadTypistBeingMad();
            }
            else
            {
                currentFilePath = filePath;
                Start();
                random = new Random((int)DateTime.Now.Ticks);
            }
        }

        public void Start()
        {
            Strategy newStrategy = null;
            int strategyChoice = random.Next(0, Enum.GetValues(typeof(Strategy.StrategyType)).Length);

            switch ((Strategy.StrategyType)strategyChoice)
            {
                case Strategy.StrategyType.ValidFileOpen:
                    newStrategy = new ValidFileOpen();
                    break;
                case Strategy.StrategyType.WhiteNoise:
                    newStrategy = new WhiteNoise();
                    break;
                case Strategy.StrategyType.SymbolGenerator:
                    newStrategy = new SymbolGenerator();
                    break;
                case Strategy.StrategyType.KeywordGenerator:
                    newStrategy = new KeywordGenerator();
                    break;
            }

            InitializeStrategy(newStrategy);
            OnDispatchTimer();
        }

        private void MadTypistBeingMad()
        {
            textEditorCore.EnableRegularCommands = false;
            // Close the default script that is open
            command = new TextEditorCommand(TextEditorCommand.Method.CloseScript);
            command.AppendArgument(0);
            textEditorCore.PlaybackCommand(command);
            textEditorControl.PlaybackCloseTab(0);
            command = new TextEditorCommand(TextEditorCommand.Method.ChangeScript);
            command.AppendArgument(0);
            textEditorCore.PlaybackCommand(command);
            textEditorControl.HandleScriptActivation();
            dispatchTimer = new DispatcherTimer();
            dispatchTimer.Tick += new EventHandler(OnDispatchTimerTick);

            fileList = Directory.GetFiles(filePath,
                                    "*.ds", SearchOption.AllDirectories);
            dispatchTimer.Start();
        }

        private void OnDispatchTimerTick(object sender, EventArgs e)
        {
            if (streamReader == null)
            {
                // All files are done
                if (madTypingCurrentFile == fileList.Length)
                {
                    dispatchTimer.Stop();
                    textEditorCore.EnableRegularCommands = true;
                    return;
                }
                else
                {
                    // Get next file
                    string testFile = fileList.ElementAt(madTypingCurrentFile++);
                    streamReader = new StreamReader(testFile);
                    // Create new script
                    command = new TextEditorCommand(TextEditorCommand.Method.CreateNewScript);
                    textEditorCore.PlaybackCommand(command);
                    // Activate script
                    textEditorControl.SetupTabInternal(null);
                    command = new TextEditorCommand(TextEditorCommand.Method.ChangeScript);
                    command.AppendArgument(0);
                    textEditorCore.PlaybackCommand(command);
                    textEditorControl.HandleScriptActivation();
                    textEditorControl.UpdateScriptDisplay(Solution.Current.ActiveScript);
                    textEditorControl.UpdateCaretPosition();
                }
            }

            if (streamReader.Peek() >= 0)
            {
                // Insert characters
                command = new TextEditorCommand(TextEditorCommand.Method.InsertText);
                command.AppendArgument((char)streamReader.Read());
                textEditorCore.PlaybackCommand(command);

            }
            else
            {
                // Close script
                command = new TextEditorCommand(TextEditorCommand.Method.CloseScript);
                command.AppendArgument(0);
                textEditorCore.PlaybackCommand(command);
                textEditorControl.PlaybackCloseTab(0);
                command = new TextEditorCommand(TextEditorCommand.Method.ChangeScript);
                command.AppendArgument(0);
                textEditorCore.PlaybackCommand(command);
                textEditorControl.HandleScriptActivation();
                streamReader = null;

            }

            textEditorControl.UpdateScriptDisplay(Solution.Current.ActiveScript);
            textEditorControl.UpdateCaretPosition();
            random = new Random();
            dispatchTimer.Interval = TimeSpan.FromMilliseconds(random.Next(minTime, maxTime));
        }

        private void InitializeStrategy(Strategy strategy)
        {
            KeywordGenerator keywordStrategy = new KeywordGenerator();
            SymbolGenerator symbolStrategy = new SymbolGenerator();

            Strategy.StrategyType strategyType = strategy.GetStrategyType();
            int numberOfActions = 0;
            Action nextAction = new Action();
            byte[] endlineChar = BitConverter.GetBytes('\n');

            if (strategyType == Strategy.StrategyType.ValidFileOpen)
            {
                nextAction = strategy.GetNextAction();
                Action.ActionType type = nextAction.Type;
                byte[] actionData = nextAction.Data;

                using (BinaryWriter binaryWriter = new BinaryWriter(File.Open(currentFilePath, FileMode.Create)))
                {
                    binaryWriter.Write(Convert.ToInt32(nextAction.Type));
                    binaryWriter.Write(actionData.Length);
                    binaryWriter.Write(actionData);

                    binaryWriter.Close();
                }
            }
            else
            {
                FileStream fileStream = File.Open(currentFilePath, FileMode.Create);
                using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
                {
                    while (numberOfActions < maxActions)
                    {
                        if (strategyType == Strategy.StrategyType.SymbolGenerator || strategyType == Strategy.StrategyType.KeywordGenerator)
                        {
                            
                            int choice = random.Next(0, 2); // Choose between whether to insert a symbol or a keyword
                            if (choice == 0)
                                nextAction = keywordStrategy.GetNextAction();
                            else
                                nextAction = symbolStrategy.GetNextAction();
                        }
                        else
                            nextAction = strategy.GetNextAction();

                        Action.ActionType type = nextAction.Type;
                        byte[] actionData = nextAction.Data;


                        if (numberOfActions % 10 == 0) // To insert an endline character after every 100 insertions
                        {
                            binaryWriter.Write(Convert.ToInt32(Action.ActionType.InsertSymbol));
                            binaryWriter.Write(endlineChar.Length);
                            binaryWriter.Write(endlineChar);
                        }

                        binaryWriter.Write(Convert.ToInt32(nextAction.Type));
                        binaryWriter.Write(actionData.Length);
                        binaryWriter.Write(actionData);

                        numberOfActions++;
                    }

                    binaryWriter.Close();
                }
            }
        }

        private void OnDispatchTimer()
        {
            using (BinaryReader binaryReader = new BinaryReader(File.Open(currentFilePath, FileMode.Open)))
            {
                int pos = 0;
                int streamLength = (int)binaryReader.BaseStream.Length;
                
                while (pos < streamLength)
                {
                    Action.ActionType actionType = (Action.ActionType)binaryReader.ReadInt32();
                    int actionLength = binaryReader.ReadInt32();
                    byte[] actionData = binaryReader.ReadBytes(actionLength);
                    int lengthOfOneAction = sizeof(Action.ActionType) + sizeof(int) + actionLength;

                    switch (actionType)
                    {
                        case Action.ActionType.OpenFile:
                            string filePath = Encoding.UTF8.GetString(actionData);
                            command = new TextEditorCommand(TextEditorCommand.Method.LoadScriptFromFile);
                            command.AppendArgument(filePath);
                            textEditorCore.PlaybackCommand(command);
                            textEditorControl.SetupTabInternal(filePath);
                            break;

                        case Action.ActionType.InsertKeyword:
                            string keyword = Encoding.UTF8.GetString(actionData);
                            command = new TextEditorCommand(TextEditorCommand.Method.InsertText);
                            command.AppendArgument(keyword);
                            textEditorCore.PlaybackCommand(command);
                            break;

                        case Action.ActionType.InsertSymbol:
                            char symbol = BitConverter.ToChar(actionData, 0);
                            command = new TextEditorCommand(TextEditorCommand.Method.InsertText);
                            command.AppendArgument(symbol);
                            textEditorCore.PlaybackCommand(command);
                            break;

                        case Action.ActionType.InsertUnicode:
                            string unicodeChar = Encoding.UTF8.GetString(actionData);
                            command = new TextEditorCommand(TextEditorCommand.Method.InsertText);
                            command.AppendArgument(unicodeChar);
                            textEditorCore.PlaybackCommand(command);
                            break;

                        case Action.ActionType.Navigate:
                            Key navigationKey = (Key)BitConverter.ToInt32(actionData, 0);
                            command = new TextEditorCommand(TextEditorCommand.Method.DoNavigation);
                            command.AppendArgument(navigationKey);
                            textEditorCore.PlaybackCommand(command);
                            break;

                        case Action.ActionType.Delete:
                            Key deletionKey = (Key)BitConverter.ToInt32(actionData, 0);
                            command = new TextEditorCommand(TextEditorCommand.Method.DoControlCharacter);
                            command.AppendArgument(deletionKey);
                            textEditorCore.PlaybackCommand(command);
                            break;
                    }

                    pos += lengthOfOneAction;
                }

                textEditorControl.UpdateScriptDisplay(Solution.Current.ActiveScript);
                textEditorControl.UpdateCaretPosition();
            }
        }
    }
}
