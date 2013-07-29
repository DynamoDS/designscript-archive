using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Globalization;
using System.Windows;
using DesignScript.Editor.Core;
using System.IO;
using System.Diagnostics;

namespace DesignScript.Editor
{
    public class Configurations
    {
        // Permanent configurations.
        public const int CurvyTabFontHeight = 12;
        public const int IconPadding = 2;
        public const int InspectionArrayLimit = 100;
        public const int ShadowWidth = 2;

        public static int FontHeight = 12;
        public static int FontDisplayHeight = 15;
        public static int BreakpointColumnWidth = 20;
        public static int LineNumberColumnStart = 15;
        public static int LineNumberColumnEnd = 68;
        public static int CanvasMarginTop = 0;
        public static int CanvasMarginLeft = LineNumberColumnEnd + ShadowWidth;
        public static int LineNumberColumnWidth = LineNumberColumnEnd - LineNumberColumnStart;
        public static int InlineIconStart = 15;
        public static int LineNumberColumnPadding = 3;
        public static int LineNumberCeiling = 0;
        public static int LineNumberMargin = 1;
        public static int IconWidth = 16;
        public static int IconHeight = 14;
        public static double FormatFontWidth = 6.6;
        public const string FontFace = "Consolas";
        public const string TabSpaces = "    ";
        public const string RecordFolderName = "IDESession";
        public const string EditingError = "Changes to application in Run mode aren't allowed!";
        public const string SlowMotionError = "Slow motion playback cannot execute when application is in run mode";
        public const string NumericSliderWarning = "Please enter a valid value";
        public const string WatchWindowDuplicateInfo = "A variable with the same name already exists in the watch window";
        public const string MultipleInstancesOfSameFile = "Cannot open multiple copies of same file!";
        public const string SaveFileBreakpointWarning = "Save file to insert break point";
        public const string BreakpointDebugWarning = "Breakpoint cannot be inserted at this line in debug mode";
        public const string BreakpointWarning = "Breakpoint cannot be set on empty line or comments";
        public const string ConfirmSaveSolution = "Do you want to save the current solution?";
        public const string OpenScriptNotSaved = "DesignScript IDE cannot determine the file path of one or more scripts. " +
            "Please save the scripts before Solution can be saved.";
        public const string UnsupportedFileType = "DesignScript does not support this file type";
        public const string FindText = "Find Text";
        public const string ReplaceText = "Change to text";
        public const string ReportIssueContent = "Your message goes here, thanks!";
        public const string ReportIssueEmailId = "emailid@example.com";
        public const string ReportIssueAcknowledge = "Thank you, your report has been sent!";
        public const string AppVersionFileName = @".\appversion.txt";
        public const string OpenImportedFile = "Open '{0}'";

        // Online references.
        public const string DesignScriptOrgManual = "http://designscript.org/manual";
        public const string DesignScriptOrgPlugIns = "http://designscript.org/plug_ins";
        public const string DesignScriptOrgGallery = "http://designscript.org/model_gallery";
        public const string DesignScriptOrgReference = "http://designscript.org/reference";
        public const string AutodeskComForum = "http://forums.autodesk.com/t5/DesignScript/bd-p/DesignScript";
        public const string YouTubeComVideo = "http://www.youtube.com/designscript";

        public static void InitializeFontSizes()
        {
            ITextEditorSettings settings = TextEditorCore.Instance.TextEditorSettings;
            ChangeFontSize(settings.FontMultiplier);
        }

        public static void IncreaseFontSize()
        {
            if (FontHeight <= 24)
            {
                ChangeFontSize(1);
                ITextEditorSettings settings = TextEditorCore.Instance.TextEditorSettings;
                settings.FontMultiplier = settings.FontMultiplier + 1;
            }
        }

        public static void DecreaseFontSize()
        {
            if (FontHeight >= 12)
            {
                ChangeFontSize(-1);
                ITextEditorSettings settings = TextEditorCore.Instance.TextEditorSettings;
                settings.FontMultiplier = settings.FontMultiplier - 1;
            }
        }

        public static string GetSettingsFilePath()
        {
            try
            {
                string appDataFolder = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.ApplicationData);

                if (!appDataFolder.EndsWith("\\"))
                    appDataFolder += "\\";

                appDataFolder += @"Autodesk\DesignScript Studio\";
                if (Directory.Exists(appDataFolder) == false)
                    Directory.CreateDirectory(appDataFolder);

                return (appDataFolder + @"EditorSettings.xml");
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static void HelpAndReferenceClick(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                fileName = "Help.htm";

            try
            {
                string executingAssemblyPathName = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string rootModuleDirectory = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(executingAssemblyPathName));
                string fullPathName = System.IO.Path.Combine(rootModuleDirectory, "Help", fileName);

                if (File.Exists(fullPathName))
                    Process.Start(fullPathName);
                else
                    Process.Start(Configurations.DesignScriptOrgReference);
            }
            catch
            {
                Process.Start(Configurations.DesignScriptOrgReference);
            }
        }

        private static void ChangeFontSize(int multiplier)
        {
            string textContent = "DesignScript";
            FontHeight += 2 * multiplier;
            BreakpointColumnWidth += 2 * multiplier;
            InlineIconStart = LineNumberColumnStart;
            LineNumberColumnWidth += 7 * multiplier;
            LineNumberColumnPadding -= multiplier * 2;
            LineNumberColumnStart += 2 * multiplier;
            LineNumberColumnEnd = LineNumberColumnStart + LineNumberColumnWidth;
            CanvasMarginLeft = LineNumberColumnEnd + 2;
            LineNumberColumnPadding += 2 * multiplier;
            FormattedText formattedText = new FormattedText(
                    textContent,
                    CultureInfo.GetCultureInfo("en-us"),
                    FlowDirection.LeftToRight,
                    new Typeface(Configurations.FontFace),
                    FontHeight,
                    new SolidColorBrush(Colors.Black));
            FormatFontWidth = formattedText.Width / textContent.Length;
            FontDisplayHeight += 2 * multiplier;
            IconWidth += 2 * multiplier;
            IconHeight += 2 * multiplier;
        }
    }

    public class Images
    {
        public const string BreakpointImage = "pack://application:,,,/DesignScript.Editor.Ui;component/Resources/breakpoint.png";
        public const string OutputMessage = "pack://application:,,,/DesignScript.Editor.Ui;component/Resources/output-message.png";
        public const string AssertPass = "pack://application:,,,/DesignScript.Editor.Ui;component/Resources/assert-pass.png";
        public const string AssertFail = "pack://application:,,,/DesignScript.Editor.Ui;component/Resources/assert-fail.png";
        public const string AutoCompleteConstructor = "pack://application:,,,/DesignScript.Editor.Ui;component/Resources/auto-constructor.png";
        public const string AutoCompleteField = "pack://application:,,,/DesignScript.Editor.Ui;component/Resources/auto-fields.png";
        public const string AutoCompleteMethod = "pack://application:,,,/DesignScript.Editor.Ui;component/Resources/auto-methods.png";
        public const string AutoCompleteMissing = "pack://application:,,,/DesignScript.Editor.Ui;component/Resources/auto-missing.png";
        public const string StatusWarning = "pack://application:,,,/DesignScript.Editor.Ui;component/Resources/status-warning.png";
        public const string StatusError = "pack://application:,,,/DesignScript.Editor.Ui;component/Resources/status-error.png";
        public const string StatusInfo = "pack://application:,,,/DesignScript.Editor.Ui;component/Resources/status-info.png";
        public const string ErrorImage = "pack://application:,,,/DesignScript.Editor.Ui;component/Resources/error_button.png";
        public const string ErrorEditImage = "pack://application:,,,/DesignScript.Editor.Ui;component/Resources/error_edit_button.png";
        public const string WarningImage = "pack://application:,,,/DesignScript.Editor.Ui;component/Resources/warning_symbol.png";
        public const string RunIndicator = "pack://application:,,,/DesignScript.Editor.Ui;component/Resources/run_indicator.png";
        public const string TabErrorIcon = "pack://application:,,,/DesignScript.Editor.Ui;component/Resources/error_symbol_bottomtab.png";
        public const string TabWarningIcon = "pack://application:,,,/DesignScript.Editor.Ui;component/Resources/warning_symbol_bottomtab.png";
    }

    public class UIColors
    {
        public static readonly Color BreakpointColor = Color.FromRgb(249, 249, 249);
        public static readonly Color LineColumnBrushColor = Color.FromRgb(234, 234, 234);
        public static readonly Color ShadowDarkColor = Color.FromArgb(150, 138, 138, 138);
        public static readonly Color ShadowLightColor = Color.FromArgb(150, 200, 200, 200);
        public static readonly Color CrossHighlightColor = Color.FromArgb(100, 139, 134, 142);
        public static readonly Color TextCanvasBackground = Color.FromRgb(240, 240, 240);
        public static readonly Color ScrollViewerBackground = Color.FromRgb(240, 240, 240);
        public static readonly Color CurvyTabOuterColor = Color.FromRgb(109, 109, 109);
        public static readonly Color CurvyTabInnerColor = Color.FromRgb(249, 249, 249);
        public static readonly Color SelectionsColor = Color.FromArgb(240, 164, 206, 235);
        public static readonly Color CursorSelectionColor = Color.FromArgb(100, 240, 240, 238);
        public static readonly Color InlineWarningColor = Color.FromArgb(140, 255, 255, 140);
        public static readonly Color InlinErrorColor = Color.FromArgb(140, 255, 200, 200);
        public static readonly Color InlinPossibleErrorColor = Color.FromArgb(120, 255, 240, 240);
        public static readonly Color InlinPossibleWarningColor = Color.FromArgb(50, 255, 255, 90);
        public static readonly Color BreakpointLineColor = Color.FromArgb(150, 180, 200, 218);
        public static readonly Color ExecutionCursorColor = Color.FromArgb(240, 212, 247, 158);
        public static readonly Color PlayBackPassedColor = Color.FromArgb(255, 128, 255, 128);
        public static readonly Color PlayBackFailedColor = Color.FromArgb(255, 255, 0, 128);
        public static readonly Color PlayBackBorderLineColor = Color.FromArgb(255, 112, 146, 190);
    }
}
