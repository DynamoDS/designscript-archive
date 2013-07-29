using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System.IO;
using System.Text.RegularExpressions;

namespace DesignScriptStudio.Graph.Core
{
    public class Utilities
    {
        #region Public Methods

        public static uint MakeFourCC(int ch0, int ch1, int ch2, int ch3)
        {
            return ((uint)(byte)(ch0) | ((uint)(byte)(ch1) << 8) | ((uint)(byte)(ch2) << 16) | ((uint)(byte)(ch3) << 24));
        }

        public static ulong MakeEightCC(int ch0, int ch1, int ch2, int ch3, int ch4, int ch5, int ch6, int ch7)
        {
            return ((ulong)(byte)(ch0) | ((ulong)(byte)(ch1) << 8) |
                    ((ulong)(byte)(ch2) << 16) | ((ulong)(byte)(ch3) << 24) |
                    ((ulong)(byte)(ch4) << 32) | ((ulong)(byte)(ch5) << 40) |
                    ((ulong)(byte)(ch6) << 48) | ((ulong)(byte)(ch7) << 56));
        }

        public static bool IsTempVariable(string variable)
        {
            return variable.Contains(Configurations.TempVariablePrefix);
        }

        public static double GetGaussianValue(double value)
        {
            double standardDeviation = Configurations.GaussianDeviation;
            double mean = Configurations.GaussianMean;

            double exponent = value - mean;
            exponent = -1 * Math.Pow(exponent, 2);
            exponent /= 2 * (Math.Pow(standardDeviation, 2));
            double result = standardDeviation * (Math.Sqrt(2 * Math.PI));
            result = 1 / result;
            result *= Math.Exp(exponent);
            return result;
        }

        //@TODO(Ben) Rename: GetTextWidth or actually compute the size and return that
        public static double GetTextWidth(string str)
        {
            if (str == null)
                return 0;

            FormattedText newText = new FormattedText(str,
                    Configurations.culture,
                    FlowDirection.LeftToRight,
                    Configurations.TypeFace,
                    Configurations.TextSize,
                    Configurations.TextNormalColor);

            return newText.WidthIncludingTrailingWhitespace;
        }

        public static double GetFormattedTextHeight(string str)
        {
            FormattedText newText = new FormattedText(str,
                Configurations.culture,
                FlowDirection.LeftToRight,
                Configurations.TypeFace,
                Configurations.TextSize,
                Configurations.TextNormalColor);

            newText.MaxTextWidth = Configurations.NodeMaxWidthCodeBlock;
            newText.Trimming = TextTrimming.None;
            return newText.Height;
        }

        public static double GetFormattedTextWidth(string str)
        {
            FormattedText newText = new FormattedText(str,
                Configurations.culture,
                FlowDirection.LeftToRight,
                Configurations.TypeFace,
                Configurations.TextSize,
                Configurations.TextNormalColor);

            newText.MaxTextWidth = Configurations.NodeMaxWidthCodeBlock;
            newText.Trimming = TextTrimming.None;
            return newText.Width;
        }

        [DllImport("user32.dll")]
        public static extern int ToUnicode(
            uint wVirtKey,
            uint wScanCode,
            byte[] lpKeyState,
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 4)] 
            StringBuilder pwszBuff,
            int cchBuff,
            uint wFlags);

        [DllImport("user32.dll")]
        public static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint uCode, uint uMapType);

        public static char GetKeyboardCharacter(KeyEventArgs e)
        {
            char ch = ' ';

            byte[] keyboardState = new byte[256];
            GetKeyboardState(keyboardState);

            uint virtualKey = ((uint)KeyInterop.VirtualKeyFromKey(e.Key));
            uint scanCode = MapVirtualKey(virtualKey, 0x0);
            StringBuilder stringBuilder = new StringBuilder(2);

            int result = ToUnicode(virtualKey, scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0);
            switch (result)
            {
                case -1:
                case 0:
                    break;
                case 1:
                default:
                    ch = stringBuilder[0];
                    break;
            }
            if (ch == '\r')
                ch = '\n';
            return ch;
        }

        /// <summary>
        /// swapping statement of an assignment
        /// E.g: 
        ///      Swap line 0
        ///      [t123 = a;] => [a = t123 ;]
        ///      Swap line 1
        ///      [a=b;     ]    [a=b;      ]
        ///      [t123 = x;] => [x = t123 ;]
        /// </summary>
        /// <param name="compilableText"> the full code text that user write which appear in the code block</param>
        /// <param name="lineNumber">the index of line that the swap should be done </param>
        /// <returns>return string with the 2 statements on the 2 side been swap</returns>
        public static string SwapAssignment(string compilalbleText, int lineNumber)
        {
            string[] splitedCompilableText = compilalbleText.Split('\n');
            if (lineNumber < splitedCompilableText.Length)
                splitedCompilableText[lineNumber] = SwapVariablePosition(splitedCompilableText[lineNumber]);
            return string.Join("\n", splitedCompilableText);
        }

        /// <summary>
        /// discard the right hand side of an assignment
        /// E.g: 
        /// Discard line no 0
        ///      [a = t123;] => [a ;]
        ///      
        /// Discard line no 1
        ///      [a=b;     ]    [a=b;]
        ///      [x = t234;] => [x ; ]
        /// </summary>
        /// <param name="compilableText"> the full code text that user write which appear in the code block</param>
        /// <param name="lineNumbers">the index of lines that the discard should be done </param>
        /// <returns>return string with the right hand side of all considered assignment have been discard</returns>
        public static string DiscardTemporaryVariableAssignment(string compilableText, int[] lineNumbers)
        {
            string[] splitedCompiableText = compilableText.Split('\n');
            for (int i = 0; i < lineNumbers.Length; i++)
                splitedCompiableText[lineNumbers[i]] = DiscardRHSAssignment(splitedCompiableText[lineNumbers[i]]);
            return string.Join("\n", splitedCompiableText);
        }

        public static bool IsValidVariableName(string variableName)
        {
            Regex regex = new Regex("^[a-zA-Z][a-zA-Z0-9_]*$", RegexOptions.Compiled);
            Match match = regex.Match(variableName);
            if (match.Success)
                return true;
            return false;
        }

        public static bool IsLiteralValue(string text)
        {
            Regex regex = new Regex(@"\d+(,\d+)?(.\d+)?;");
            Match match = regex.Match(text);
            if (match.Success && match.Value == text)
                return true;
            return false;
        }

        #endregion

        #region Public Drawing Utilities

        public static void DrawText(DrawingContext drawingContext, string str, Point pt, SolidColorBrush textColor, int maxWidth = -1)
        {
            FormattedText newText = new FormattedText(str,
                Configurations.culture,
                FlowDirection.LeftToRight,
                Configurations.TypeFace,
                Configurations.TextSize,
                textColor);
            if (maxWidth != -1)
            {
                newText.MaxTextWidth = maxWidth;
                newText.Trimming = TextTrimming.None;
            }

            // In order to improve performance, text should be rendered using DrawText API
            // Converting the text to geometry before rendering using BuildGeometry, improves
            // rendering quality but at unacceptable loss of performance. Performance profiling 
            // showed a 20% increase in processing when the BuildGeometry and DrawGeometry are used
            // More details can be found in this defect: 
            // IDE-1290: Labels and the object name is little bit blurry, at certain zoom level. 

            //Geometry textGeometry = newText.BuildGeometry(pt);
            //drawingContext.DrawGeometry(Configurations.TextColor, null, textGeometry);
            drawingContext.DrawText(newText, pt);
        }

        public static void DrawBoldText(DrawingContext drawingContext, string str, Point pt)
        {
            FormattedText newText = new FormattedText(str,
                Configurations.culture,
                FlowDirection.LeftToRight,
                Configurations.TypeFace,
                Configurations.TextSize,
                Configurations.TextBoldColor);

            newText.SetFontWeight(FontWeights.SemiBold);
            Geometry textGeometry = newText.BuildGeometry(pt);
            drawingContext.DrawGeometry(Configurations.TextBoldColor, null, textGeometry);
            //drawingContext.DrawText(newText, pt);
        }

        public static void DrawSlot(DrawingContext drawingContext, Point p1)
        {
            //@TODO(Joy)DONE magic number 10?
            p1.X = (int)p1.X;
            p1.Y = (int)p1.Y;
            Point p2 = p1;
            p2.Y = p1.Y + Configurations.SlotSize;
            p1.Offset(0.5, 0);
            p2.Offset(0.5, 0);
            drawingContext.DrawLine(Configurations.BorderPen, p1, p2);

            p1.Offset(-Configurations.OutputHittestPixels, -Configurations.SlotStripeSize / 2 + Configurations.SlotSize / 2);
            //draw hit test rectangle
            Size hitTestSize = new Size(Configurations.OutputHittestPixels, Configurations.SlotStripeSize);
            Rect hitTestRect = new Rect(p1, hitTestSize);
            drawingContext.DrawRectangle(Configurations.SlotHitTestColor, Configurations.NoBorderPen, hitTestRect);
        }

        #endregion

        #region Auto-save and Backup Recovery Methods

        public static string GetBackupFileName(string sessionName, uint graphId, int index)
        {
            // The names of the backup files always follow a predefined format:
            // 
            //      dss-backup-SSSSSSSS-SSSS-SSSS-SSSS-SSSSSSSSSSSS-G-I.bin
            // 
            // Where "S" represents the session name in the regular 128-bit 
            // GUID. Each instance of DesignScript Studio has a unique session 
            // name that is generated during start-up.
            // 
            // The "G" represents the value of "GraphController.Identifier".
            // 
            // The "I" represents the incremental index for each saving.
            // 
            // When an instance of DesignScript Studio crashes, it attempts to 
            // launch a new instance of DesignScript Studio. In doing so, it 
            // also passes its session name to the new instance through an 
            // environment variable that the new instance can access. The new 
            // instance then uses the same session name to get hold of those 
            // backup files that exist before the crash happened.
            // 
            // Generate a wildcard file name when there's no index.
            string strIndex = ((index >= 0) ? index.ToString() : "*");

            // Generate a wildcard graph id when it's not specified.
            string strGraphId = "*";
            if (graphId != uint.MaxValue && (graphId != 0))
                strGraphId = graphId.ToString();

            return string.Format(UiStrings.BackupFileNameFmt,
                sessionName, strGraphId, strIndex);
        }

        public static void GetBackupFileIndex(string backupFileName, ref uint graphId, ref int index)
        {
            graphId = uint.MaxValue;
            index = -1;

            if (string.IsNullOrEmpty(backupFileName))
            {
                string format = "Invalid backup file name format: {0} (C17A22F27428)";
                throw new ArgumentException(string.Format(format, backupFileName));
            }

            // Strip away full path information, keep only name.
            backupFileName = Path.GetFileName(backupFileName);
            if (string.IsNullOrEmpty(backupFileName))
            {
                string format = "Invalid backup file name format: {0} (C880705972C6)";
                throw new ArgumentException(string.Format(format, backupFileName));
            }

            // The names of the backup files always follow a predefined format:
            // 
            //      dss-backup-SSSSSSSS-SSSS-SSSS-SSSS-SSSSSSSSSSSS-G-I.bin
            // 
            // We know the index is always at the last position before the file 
            // extension. The index is also prefixed with a dash character.
            // 
            if (backupFileName.Count((x) => (x == '-')) != 8)
            {
                string format = "Invalid backup file name format: {0} (5DD3B74F1BD9)";
                throw new ArgumentException(string.Format(format, backupFileName));
            }

            // There must be a 'dot' character that goes after the '-'.
            int lastHyphen = backupFileName.LastIndexOf('-');
            int lastPeriod = backupFileName.LastIndexOf('.');
            if (-1 == lastPeriod || (lastPeriod <= lastHyphen))
            {
                string format = "Invalid backup file name format: {0} (C16BA5A43202)";
                throw new ArgumentException(string.Format(format, backupFileName));
            }

            // We are guaranteed to have the second last hyphen when we got here.
            string nameWithoutIndex = backupFileName.Substring(0, lastHyphen);
            int secondLastHyphen = nameWithoutIndex.LastIndexOf('-');
            int length = lastHyphen - secondLastHyphen - 1;
            if (length <= 0)
            {
                string format = "Invalid backup file name format: {0} (75BA05ABB7D8)";
                throw new ArgumentException(string.Format(format, backupFileName));
            }

            string graphIdString = backupFileName.Substring(secondLastHyphen + 1, length);
            if (!uint.TryParse(graphIdString, out graphId))
            {
                string format = "Invalid backup file name format: {0} (B8E83EA67DF2)";
                throw new ArgumentException(string.Format(format, backupFileName));
            }

            length = lastPeriod - lastHyphen - 1;
            string indexString = backupFileName.Substring(lastHyphen + 1, length);
            if (!Int32.TryParse(indexString, out index))
            {
                string format = "Invalid backup file name format: {0} (D785108660C2)";
                throw new ArgumentException(string.Format(format, backupFileName));
            }
        }

        public static List<string> GetBackupFilePaths(string sessionName, uint graphId)
        {
            string searchPattern = Utilities.GetBackupFileName(sessionName, graphId, -1);
            DirectoryInfo directory = new DirectoryInfo(Utilities.GetBackupFileFolder());
            FileInfo[] files = directory.GetFiles(searchPattern);
            if (null == files || (files.Length <= 0))
                return null;

            List<string> backupFiles = new List<string>();
            foreach (FileInfo file in files)
                backupFiles.Add(file.FullName);

            // Sort the list in ascending order.
            backupFiles.Sort((x, y) =>
            {
                int indexX = -1, indexY = -1;
                uint graphIdX = uint.MaxValue, graphIdY = uint.MaxValue;
                Utilities.GetBackupFileIndex(x, ref graphIdX, ref indexX);
                Utilities.GetBackupFileIndex(y, ref graphIdY, ref indexY);

                if (graphIdX == graphIdY)
                    return indexX - indexY;

                return (((int)graphIdX) - ((int)graphIdY));
            });

            return backupFiles;
        }

        public static string GetBackupFileFolder()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string backupFolder = string.Format(UiStrings.BackupFolderNameFmt, appDataPath);
            if (!Directory.Exists(backupFolder))
                Directory.CreateDirectory(backupFolder);

            return backupFolder; // Callers assume this to have a trailing backslash.
        }

        public static void ClearBackupFiles(string sessionName, uint graphId)
        {
            // Done with backup files, delete them away.
            List<string> backupFiles = Utilities.GetBackupFilePaths(sessionName, graphId);
            if (null != backupFiles && (backupFiles.Count > 0))
            {
                foreach (string backupFile in backupFiles)
                    File.Delete(backupFile);
            }
        }

        public static Dictionary<uint, string> GetFilesToRecover(string sessionName)
        {
            List<string> backupFiles = GetBackupFilePaths(sessionName, uint.MaxValue);
            if (null == backupFiles || (backupFiles.Count <= 0))
                return null;

            Dictionary<uint, string> uniqueFiles = new Dictionary<uint, string>();
            foreach (string backupFile in backupFiles)
            {
                try
                {
                    int index = -1; uint graphId = uint.MaxValue;
                    Utilities.GetBackupFileIndex(backupFile, ref graphId, ref index);

                    // Keep overwriting the one in dictionary since the names in 
                    // backupFiles are always sorted in accordance to the index.
                    uniqueFiles[graphId] = backupFile;
                }
                catch (Exception)
                {
                    // Some ill-formed backup file names (ignore them here).
                }
            }

            return uniqueFiles;
        }

        #endregion

        #region Private Helper Methods

        private static string SwapVariablePosition(string inputString)
        {
            int equalIndex = inputString.IndexOf("=");
            int semiColonIndex = inputString.IndexOf(";");
            if (equalIndex == -1 || semiColonIndex == -1)
                return inputString;
            string RHS = inputString.Substring(equalIndex + 1, semiColonIndex - equalIndex - 1).Trim();
            string LHS = inputString.Substring(0, equalIndex).Trim();
            if (!IsValidVariableName(RHS))
                return inputString;
            string resultString = RHS + " = " + LHS + inputString.Substring(semiColonIndex);
            return resultString;
        }

        private static string DiscardRHSAssignment(string assignment)
        {
            if (string.IsNullOrEmpty(assignment) || assignment.IndexOf('=') == -1)
                return assignment;
            int equalIndex = assignment.IndexOf("=");
            int semiColonIndex = assignment.IndexOf(";");
            string resultstring = assignment.Remove(equalIndex, semiColonIndex - equalIndex);
            return resultstring;
        }
        #endregion

    }
}
