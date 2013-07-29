using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace DesignScript.Editor.Automation
{
    public struct Action
    {
        public enum ActionType
        {
            InsertUnicode,
            InsertSymbol,
            InsertKeyword,
            OpenFile,
            Navigate,
            Delete
        }

        public ActionType Type { get; set; }
        public byte[] Data { get; set; }
    }

    public abstract class Strategy
    {
        public enum StrategyType
        {
            ValidFileOpen,
            WhiteNoise,
            SymbolGenerator,
            KeywordGenerator
        }

        protected Action nextAction;
        protected StrategyType strategyType;

        public StrategyType GetStrategyType()
        {
            return strategyType;
        }

        protected Random randomValue;
        public abstract Action GetNextAction();
    }

    public class ValidFileOpen : Strategy
    {
        List<string> filePaths = null;
        List<Key> deletions = null;
        public ValidFileOpen()
        {
            nextAction = new Action();
            filePaths = new List<string>();
            filePaths.AddRange(new string[] 
            {
                "C:\\Users\\t_sekas\\Desktop\\TestCases\\Valid\\Assignment02.ds",
                "C:\\Users\\t_sekas\\Desktop\\TestCases\\Valid\\Assignment03.ds",
                "C:\\Users\\t_sekas\\Desktop\\TestCases\\Valid\\Assignment04.ds",
                "C:\\Users\\t_sekas\\Desktop\\TestCases\\Valid\\Assignment05.ds",
                "C:\\Users\\t_sekas\\Desktop\\TestCases\\Valid\\Assignment06.ds",
                "C:\\Users\\t_sekas\\Desktop\\TestCases\\Valid\\Assignment07.ds",
                "C:\\Users\\t_sekas\\Desktop\\TestCases\\Valid\\Assignment08.ds",
                "C:\\Users\\t_sekas\\Desktop\\TestCases\\Valid\\Assignment09.ds",
                "C:\\Users\\t_sekas\\Desktop\\TestCases\\Valid\\Assignment10.ds",
                "C:\\Users\\t_sekas\\Desktop\\TestCases\\Valid\\Assignment12.ds",
                "C:\\Users\\t_sekas\\Desktop\\TestCases\\Valid\\Assignment13.ds",
                "C:\\Users\\t_sekas\\Desktop\\TestCases\\Valid\\Assignment14.ds",
                "C:\\Users\\t_sekas\\Desktop\\TestCases\\Valid\\Assignment19.ds",
                "C:\\Users\\t_sekas\\Desktop\\TestCases\\Valid\\Assignment20.ds",
                "C:\\Users\\t_sekas\\Desktop\\TestCases\\Valid\\Assignment21.ds",
                "C:\\Users\\t_sekas\\Desktop\\TestCases\\Valid\\Assignment22.ds"
            });
            deletions = new List<Key>();
            deletions.AddRange(new Key[] 
            {
                Key.Delete,
                Key.Back
            });

            strategyType = StrategyType.ValidFileOpen;
            randomValue = new Random((int)DateTime.Now.Ticks);
        }
        public override Action GetNextAction()
        {
            nextAction.Type = Action.ActionType.OpenFile;
            int fileNumber = randomValue.Next(0, filePaths.Count);
            string filePath = filePaths.ElementAt(fileNumber);
            // See where to get these files from
            System.Text.Encoding encoded = System.Text.Encoding.ASCII;
            nextAction.Data = encoded.GetBytes(filePath);
            return nextAction;
        }
    }

    public class WhiteNoise : Strategy
    {
        public WhiteNoise()
        {
            nextAction = new Action();
            strategyType = StrategyType.WhiteNoise;
            randomValue = new Random((int)DateTime.Now.Ticks);
        }

        public override Action GetNextAction()
        {
            nextAction.Type = Action.ActionType.InsertUnicode;
            // Get UniCode character into String
            int value = randomValue.Next(0x0000, 0x7FFF);
            byte[] unicodeChar = new byte[2];
            unicodeChar[1] = (byte)((value & 0xFF00) >> 8);
            unicodeChar[0] = (byte)(value & 0xFF);
            nextAction.Data = unicodeChar;

            //for (int i = 0; i < length * 2; i += 2)
            //{
            //    int chr = randomValue.Next(0xD7FF);
            //    unicodeChar[i + 1] = (byte)((chr & 0xFF00) >> 8);
            //    unicodeChar[i] = (byte)(chr & 0xFF);
            //}

            return nextAction;
        }
    }

    public class SymbolGenerator : Strategy
    {
        List<Key> keyPresses = null;
        public SymbolGenerator()
        {
            nextAction = new Action();
            keyPresses = new List<Key>();
            keyPresses.AddRange(new Key[] { Key.Delete, Key.Back, Key.Left, Key.Right, Key.Up, Key.Down });
            strategyType = StrategyType.SymbolGenerator;
            randomValue = new Random((int)DateTime.Now.Ticks);
        }
        public override Action GetNextAction()
        {
            nextAction.Type = Action.ActionType.InsertSymbol;
            int choice = randomValue.Next(0, 4), valueChosen;
            switch (choice)
            {
                case 0:
                    // Choose between series of sybmols from ' ' to '@'
                    valueChosen = randomValue.Next((int)' ', (int)'@' + 1);
                    break;
                case 1:
                    // Choose between series of sybmols from '[' to '`'
                    valueChosen = randomValue.Next((int)'[', (int)'`' + 1);
                    break;
                case 2:
                    // Choose between series of sybmols from '{' to '~'
                    valueChosen = randomValue.Next((int)'{', (int)'~' + 1);
                    break;
                case 3:
                default:
                    int keyPress = randomValue.Next(0, keyPresses.Count);
                    valueChosen = Convert.ToInt32(keyPresses.ElementAt(keyPress));
                    if (keyPress < 2) // First two in the keyPress list are for deletion
                        nextAction.Type = Action.ActionType.Delete;
                    else
                        nextAction.Type = Action.ActionType.Navigate;
                    break;
            }

            nextAction.Data = BitConverter.GetBytes(valueChosen);
            return nextAction;
        }
    }

    public class KeywordGenerator : Strategy
    {
        List<Key> keyPresses = null;
        List<string> keywords = null;
        public KeywordGenerator()
        {
            nextAction = new Action();
            keyPresses = new List<Key>();
            keyPresses.AddRange(new Key[] { Key.Delete, Key.Back, Key.Left, Key.Right, Key.Up, Key.Down });
            keywords = new List<string>();
            // Add list of keywords
            keywords.AddRange(new string[] 
            { 
                " native ",
                " class ",
                " constructor ",
                " def ",
                " external ",
                " extends ",
                " public ",
                " private ",
                " protected ",
                " __heap ",
                " if ",
                " elseif ",
                " else ",
                " while ", 
                " for ",
                " double ", 
                " int ",
                " var ", 
                " function ",
                " import ",
                " from ",
                " prefix ",
                " static ",
                " break ",
                " continue ",
                " true ",
                " false ",
                " null ",
                " in ",
                " Associative ",
                " Imperative ",
                " Options "
            });
            strategyType = StrategyType.KeywordGenerator;
            randomValue = new Random((int)DateTime.Now.Ticks);
        }

        public override Action GetNextAction()
        {
            nextAction.Type = Action.ActionType.InsertKeyword;
            string keywordChosen = null;
            int keyPressChosen;
            int choice = randomValue.Next(0, keywords.Count + keyPresses.Count);
            if (choice < keywords.Count)
            {
                keywordChosen = keywords.ElementAt(choice);
                Encoding encoded = Encoding.ASCII;
                nextAction.Data = encoded.GetBytes(keywordChosen);
            }
            else
            {
                keyPressChosen = Convert.ToInt32(keyPresses.ElementAt(choice - keywords.Count));
                nextAction.Data = BitConverter.GetBytes(keyPressChosen);
                if (choice - keywords.Count < 2) // First two in the keyPress list are for deletion
                    nextAction.Type = Action.ActionType.Delete;
                else
                    nextAction.Type = Action.ActionType.Navigate;
            }

            return nextAction;
        }
    }
}
