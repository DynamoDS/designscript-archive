using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Globalization;
using System.Windows.Media.Effects;
using System.Windows;

namespace DesignScriptStudio.Graph.Core
{

#if high_contrast

    public class Configurations
    {
        public static double NodeHeightLiteral = 20.0;
        public static double NodeWidthLiteral = 30.0;
        
        public static double NodeHeightFunction = 60.0;
        public static double NodeWidthFunction = 150.0;
        public static double FunctionNodeOffset = 100.0;

        public static double NodeHeightIdentifier = 20.0;
        public static double NodeWidthIdentifier = 150.0;
        public static double IdentifierNodeOffset = 150.0;

        public static double SlotOffset = 10.0;

        public static Typeface TypeFace = new Typeface("Segoe UI");
        
        // @TODO(Joy): Some names are not PascalCase. And instead of just "rectBack", 
        // try to give it more meaningful names (e.g. what is it used for?).
        public static Pen BorderPen = new Pen(new SolidColorBrush(Color.FromRgb(0,0,255)),2);
        public static SolidColorBrush RectBackground = new SolidColorBrush(Color.FromArgb(255,255,0,0));
        public static SolidColorBrush rectBack = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
        public static SolidColorBrush TextColor = new SolidColorBrush(Color.FromRgb(0,0,255));
        public static int CornerRadius = 2;

        public static int TextboxWidth = 120;
        public static int TextboxHeight = 24;
        public static int TextSize = 14;
        public static CultureInfo culture = CultureInfo.GetCultureInfo("en-us");
        public static int TextOffset = 6;

        // @TODO(Joy): Avoid random blank lines.

        //for EdgeController
        public static Pen GrayPen = new Pen(Brushes.Blue, 1.5);
        public static Pen LightGrayPen = new Pen(Brushes.Red, 1.5);

        //Signature
        public static uint DataHeaderSignature = Utilities.MakeFourCC('H', 'E', 'A', 'D');
        public static uint EdgeSignature = Utilities.MakeFourCC('E', 'D', 'G', 'E');
        public static uint GraphPropertiesSignature = Utilities.MakeFourCC('G', 'L', 'O', 'P');
        public static uint NodeSignature = Utilities.MakeFourCC('N', 'O', 'D', 'E');
        public static uint RuntimeStatesSignature = Utilities.MakeFourCC('R', 'U', 'N', 'S');
        public static uint SlotSignature = Utilities.MakeFourCC('S', 'L', 'O', 'T');
    }

#else

    public class Configurations
    {
        public static string DefaultName = "Graph";

        public static int ConfirmTimeSpan = 2;
        //public static int ErrorTimeSpan = 5;
        public static int FadeTimeSpan = 500;

        public static int MaxBackupFilesPerGraph = 3;

        //App Configurations:
        public static Boolean EnableLiveExection = true;
        public static Boolean DumpDebugInfo = true;

        //Library:
        public static int LibraryHeight = 400;
        public static int LibraryHeaderHeight = 25;
        public static int LibraryContentHeight = 375;
        public static int LibraryMinHeight = 75;
        public static int LibraryTopMargin = 5;

        public static Typeface LibraryTextFontFamily = new Typeface("Segoe UI");
        public static int LibraryTextFontSize = 12;
        public static Brush LibraryTextForegroundBrush = new SolidColorBrush(Color.FromRgb(102, 102, 102));

        public static int LibraryExpanderWidth = 20;
        public static int LibraryLevelIndent = 20;
        public static Thickness LibraryRootItemMargin = new Thickness(0, 0, 0, 1);

        public static FontWeight LibraryRootFontWeight = FontWeights.SemiBold;
        public static Thickness LibraryRootBorderThickness = new Thickness(0, 1, 0, 1);
        public static SolidColorBrush LibraryRootBorder = new SolidColorBrush(Color.FromRgb(222, 222, 222));
        public static SolidColorBrush LibraryRootBackground = new SolidColorBrush(Color.FromRgb(237, 237, 237));

        public static FontWeight LibrarySubTreeFontWeight = FontWeights.Normal;
        public static Thickness LibrarySubtreeBorderThickness = new Thickness(0, 0, 0, 1);
        public static SolidColorBrush LibrarySubtreeBorder = new SolidColorBrush(Color.FromRgb(245, 245, 245));
        public static SolidColorBrush LibrarySubtreeBackground = Brushes.White;

        public static int AddNodeToCanvasRegion = 100;

        //Selection Box
        public static double SelectionBoxMargin = 20;
        public static SolidColorBrush SelectionBoxBackgroundColor = new SolidColorBrush(Color.FromRgb(251, 254, 255));
        public static Pen SelectionBoxPen = new Pen(new SolidColorBrush(Color.FromRgb(135, 170, 182)), 2);

        //Node Visuals:
        public static double NodeHeightCodeBlock = 16.0;
        public static double NodeWidthCodeBlock = 16.0;
        public static double NodeMaxWidthCodeBlock = 500;

        public static double NodeHeightFunction = 40.0;
        public static double NodeWidthFunction = 22.0;
        public static double FunctionNodeCenterLine = 22.0;

        public static double NodeHeightDriver = 28.0;
        public static double NodeWidthDriver = 22.0;
        public static double DriverNodeCenterLine = 18.0;

        public static double NodeHeightIdentifier = 28.0;
        public static double NodeWidthIdentifer = 18.0;

        public static double NodeHeightRender = 64.0;
        public static double NodeWidthRender = 64.0;

        public static double NodeHeightProperty = 47.0;
        public static double NodeWidthProperty = 22.0;

        public static double NodeCreationOffset = 10.0;

        public static double SlotOffset = 10.0;
        public static double SlotSize = 8.0;
        public static SolidColorBrush SlotHitTestColor = Brushes.Transparent;

        public static string Font = "Segoe UI";
        public static Typeface TypeFace = new Typeface("Segoe UI");
        public static int TextboxWidth = 120;
        public static int TextboxHeight = 24;
        public static double TextboxMaxWidth = 172;
        public static int TextboxMaxCharacters = 25;
        public static double TextSize = 12;
        public static CultureInfo culture = CultureInfo.GetCultureInfo("en-us");
        public static double TextOffset = 4.0;
        public static double TextHorizontalOffset = 7.0;
        public static double TextVerticalOffset = 5.0;

        //CodeBlockNode Initial Message
        public static string CodeBlockInitialMessage = "Your code goes here";

        //VariableName Prefix
        public static string VariablePrefix = "Var";

        //TempVariableName Prefix
        public static string TempVariablePrefix = "_temp_";

        //DriverNode Initial Text Value
        public static string DriverInitialTextValue = "0";

        //  : Some names are not PascalCase. And instead of just "rectBack", 
        // try to give it more meaningful names (e.g. what is it used for?).
        public static Color BorderColor = Color.FromArgb(255, 240, 240, 240);
        public static Pen BorderPen = new Pen(new SolidColorBrush(Color.FromRgb(170, 170, 170)), 1);
        public static Pen NoBorderPen = new Pen(new SolidColorBrush(Color.FromArgb(255, 170, 170, 170)), 0);
        public static Pen SelectionBorderPen = new Pen(new SolidColorBrush(Color.FromArgb(255, 135, 170, 182)), 1);
        public static Pen WhitePen = new Pen(new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)), 1);

        public static SolidColorBrush RectGrey = new SolidColorBrush(Color.FromRgb(229, 229, 229));
        public static SolidColorBrush RectWhite = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
        public static SolidColorBrush RectHighlighted = new SolidColorBrush(Color.FromRgb(223, 246, 255));
        public static SolidColorBrush TextNormalColor = new SolidColorBrush(Color.FromRgb(85, 85, 85));
        public static SolidColorBrush TextErrorColor = new SolidColorBrush(Color.FromRgb(255, 0, 0));
        public static SolidColorBrush TextBoldColor = new SolidColorBrush(Color.FromRgb(51, 51, 51));

        public static Color SelectionColorWhite = Color.FromRgb(255, 255, 255);
        public static SolidColorBrush SlotBackground = new SolidColorBrush(Color.FromRgb(246, 246, 246));
        public static SolidColorBrush DotColor = new SolidColorBrush(Color.FromRgb(170, 170, 170));
        public static SolidColorBrush DotColorSelected = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        public static int DotGridSize = 9;
        public static int CornerRadius = 0;

        //Shadow parameters
        public static int BlurRadius = 3;
        public static int Direction = 0;
        public static int ShadowDepth = 0;
        public static double Opacity = 1;
        public static Color SelectionColor = Color.FromRgb(155, 204, 222);

        public static double HittestPixels = 8.0;
        public static double SlotStripeSize = 20.0;

        //Hittest
        public static double InputHittestPixels = 10.0;
        public static double OutputHittestPixels = 15.0;

        //Code Block offsets
        public static int CodeBlockHorizontalOffset = 8;
        public static int FontPixels = 16;

        //Replication Guides
        public static double ReplicationGuideWidth = 13.0;
        public static double TriangleBase = 16.0;
        public static double TriangleHeight = 5.0;
        public static int ArraySize = 10;
        public static string ReplicationInitialString = "<>";
        public static SolidColorBrush SquareColor = new SolidColorBrush(Color.FromRgb(228, 228, 228));

        //Radial menu
        public static int RadialMenuMaxItems = 7;
        public static double CenterOffsetX = 75;
        public static double CenterOffsetY = 75;
        public static double CircleRadius = 130;
        public static double MinimumTextSize = 12;
        public static double ItemSeparation = 5.0;
        public static double MaxHeightIncrement = 5;
        public static double AmplifyingFactor = 7.5;
        public static double EndRectangleLength = 20.0;
        public static Color GradientBlue = Color.FromRgb(170, 229, 255);
        public static Color ArcColor = Color.FromRgb(55, 163, 199);
        public static SolidColorBrush NormalTextColor = new SolidColorBrush(Color.FromRgb(56, 142, 185));
        public static SolidColorBrush SelectedTextColor = new SolidColorBrush(Color.FromRgb(34, 84, 118));
        public static SolidColorBrush FadedTextColor = new SolidColorBrush(Color.FromArgb(100, 56, 142, 185));
        public static SolidColorBrush BackgroundRectColor = new SolidColorBrush(Color.FromArgb(225, 255, 255, 255));
        public static double RadialMenuDelay = 250;
        //Radial menu colors
        public static SolidColorBrush DefaultItemColor = new SolidColorBrush(Color.FromRgb(85, 85, 85));
        public static SolidColorBrush SelectedItemColor = new SolidColorBrush(Colors.Black);
        //public static SolidColorBrush TextColor = new SolidColorBrush(Color.FromRgb(85, 85, 85));
        //Radial Menu Scroller
        public static Brush BlueBrush = new SolidColorBrush(Color.FromRgb(169, 200, 219));
        public static Brush WhiteBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        public static Brush HoverBrush = new SolidColorBrush(Color.FromRgb(191, 225, 237));
        public static double ScrollerHittestArea = 30.0;
        public static double ScrollerRectWidth = 20.0;
        public static double ScrollerRectHeight = 10.0;
        //Background Effects
        public static double RadialEffectBlurRadius = 10.0;
        public static double RadialEffectDirection = 160.0;
        public static double RadialEffectShadowDepth = 30.0;

        //Gaussian Function Parameters
        public static double GaussianMean = 0.0;
        public static double GaussianDeviation = 0.5;

        //Info Bubble
        public static int InfoBubbleTopMargin = 3;
        public static int InfoBubbleArrowHeight = 5;
        public static double InfoBubbleArrowWidthHalf = 5;
        public static double InfoBubbleArrowHitTestWidth = 30;
        public static double InfoBubbleArrowHitTestHeight = 15;
        public static int InfoBubbleMinWidth = 35;
        public static int InfoBubbleMaxWidth = 400;
        public static int InfoBubbleMaxHeight = 250;
        public static int InfoBubbleText = 12;
        public static int InfoBubbleMargin = 4;

        public static int InfoBubbleResizeMinimumWidth = 150;
        public static int InfoBubbleResizeMinimumHeight = 4 * InfoBubbleMargin + 2 * InfoBubbleText;

        //Error Bubble
        public static SolidColorBrush ErrorBubbleErrorTextColor = new SolidColorBrush(Colors.White);
        public static Pen ErrorBubbleErrorBorderPen = new Pen(new SolidColorBrush(Color.FromRgb(169, 69, 69)), 1);
        public static SolidColorBrush ErrorBubbleErrorBackgroundActive = new SolidColorBrush(Color.FromRgb(213, 85, 85));
        public static SolidColorBrush ErrorBubbleErrorBackgroundNonActive = new SolidColorBrush(Color.FromArgb(150, 213, 85, 85));
        public static SolidColorBrush ErrorBubbleWarningTextColor = new SolidColorBrush(Colors.Black);
        public static Pen ErrorBubbleWarningBorderPen = new Pen(new SolidColorBrush(Color.FromRgb(226, 154, 69)), 1);
        public static SolidColorBrush ErrorBubbleWarningBackground = new SolidColorBrush(Color.FromRgb(255, 216, 148));

        //Preview Bubble
        public static string PreviewBubbleTextDefault = "value preview";

        public static SolidColorBrush PreviewBubbleCondensedTextColor = new SolidColorBrush(Color.FromRgb(174, 174, 174));
        public static Pen PreviewBubbleCondensedBorderPen = new Pen(new SolidColorBrush(Color.FromRgb(219, 219, 219)), 1);
        public static SolidColorBrush PreviewBubbleCondensedBackground = new SolidColorBrush(Color.FromRgb(243, 243, 243));

        public static SolidColorBrush PreviewBubbleExtendedTextColor = new SolidColorBrush(Color.FromRgb(51, 51, 51));
        public static SolidColorBrush PreviewBubbleExtendedBorderBrush = new SolidColorBrush(Color.FromRgb(170, 170, 170));
        public static Pen PreviewBubbleExtendedBorderPen = new Pen(PreviewBubbleExtendedBorderBrush, 1);
        public static SolidColorBrush PreviewBubbleExtendedBackground = new SolidColorBrush(Color.FromRgb(229, 229, 229));
        public static SolidColorBrush PreviewBubbleExtendedTransparent = Brushes.Transparent;
        public static int PreviewBubbleExtendedScrollBarWidth = 7;
        public static SolidColorBrush PreviewBubbleExtendedScrollBarTract = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        public static SolidColorBrush PreviewBubbleExtendedScrollBarThumb = new SolidColorBrush(Color.FromRgb(214, 214, 214));

        public static int PreviewBubbleGeoSize = 100;
        public static Pen PreviewBubbleGeoBorderPen = new Pen(new SolidColorBrush(Color.FromRgb(17, 17, 17)), 1);
        public static SolidColorBrush PreviewBubbleGeoBackground = new SolidColorBrush(Color.FromRgb(25, 25, 25));
        public static Pen PreviewBubbleMinGeoBorderPen = new Pen(new SolidColorBrush(Color.FromRgb(123, 123, 123)), 1);
        public static SolidColorBrush PreviewBubbleMinGeoBackground = new SolidColorBrush(Color.FromRgb(170, 170, 170));

        //Context Menu
        public static int ContextMenuMargin = 2;

        //Edge
        public static Brush curveColor = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0));
        public static Pen GrayPen = new Pen(curveColor, 1.5);
        public static Pen GlowPen = new Pen(new SolidColorBrush(Color.FromArgb(30, 0, 174, 255)), 5);
        public static Pen SolidPen = new Pen(Configurations.curveColor, 1.5);
        public static Pen DashPen = new Pen(Configurations.curveColor, 1.5);
        public static Pen TransparentPen = new Pen(Brushes.Transparent, 5);

        //Zoom and Pan
        public static int SafeMargin = 50; // bring the visual back to view with additional offset against border
        public static int PanningSpeed = 100;
        public static double LongAnimationTime = 800;
        public static double ShortAnimationTime = 250;
        public static double ScrollSpeedFactor = 0.01;
        public static int MaxScrollSpeed = 1;
        public static int MinSliderStep = 0;
        public static int MaxSliderStep = 12;
        public static double[] SliderStep = { 0.1, 0.3, 0.6, 0.7, 0.8, 0.9, 1, 1.25, 1.5, 1.75, 2, 2.5, 3 };

        //Signature
        public static uint DataHeaderSignature = Utilities.MakeFourCC('H', 'E', 'A', 'D');
        public static uint EdgeSignature = Utilities.MakeFourCC('E', 'D', 'G', 'E');
        public static uint GraphPropertiesSignature = Utilities.MakeFourCC('G', 'L', 'O', 'P');
        public static uint NodeSignature = Utilities.MakeFourCC('N', 'O', 'D', 'E');
        public static uint RuntimeStatesSignature = Utilities.MakeFourCC('R', 'U', 'N', 'S');
        public static uint SlotSignature = Utilities.MakeFourCC('S', 'L', 'O', 'T');
        public static uint StatementSignature = Utilities.MakeFourCC('S', 'T', 'M', 'T');


        //MenuItem Id Base
        public static int PropertyBase = 1000;
        public static int MethodBase = 2000;
        public static int ConstructorBase = 3000;
        public static int OverloadBase = 4000;

        //static MenuItem Ids
        public static int AddReplicationGuides = 1;
        public static int RemoveReplicationGuides = 2;
        public static int DeleteNode = 3;
        public static int ConvertNodeToCode = 4;
        public static int GeometricOutput = 5;
        public static int TextualOutput = 6;
        public static int HidePreview = 7;
        public static int Copy = 8;

        public static string LoadingMessage = "Loading components...";
        public static string NoResultMessage = "No result found...";

        //data size
        public static int DataHeaderSize = 56;
        public static int UndoReDoDataHeaderSize = 12;

        private static bool initialized = false;
        internal static void InitializeOnce()
        {
            if (false != initialized)
                throw new InvalidOperationException("'Configurations.InitializeOnce' called twice!");

            Configurations.SolidPen.DashStyle = System.Windows.Media.DashStyles.Solid;
            Configurations.DashPen.DashStyle = System.Windows.Media.DashStyles.Dot;
            Configurations.SelectionBoxPen.DashStyle = System.Windows.Media.DashStyles.Dash;
            initialized = true;
        }
    }

    public class UiStrings
    {
        // Formatting strings to be used in conjunction with "string.Format".
        public static string RedefinitionFmt = "'{0}' is already defined.";
        public static string ArgumentNameFmt = "p{0}";
        public static string SignatureMismatchFmt = "Signature mismatched.\n\nExpected: {0}\nActual: {1}";
        public static string BackupFileNameFmt = "dss-backup-{0}-{1}-{2}.bin";
        public static string BackupFolderNameFmt = @"{0}\DesignScriptStudio\Backup\";
        public static string FutureFileVersionFmt = "This file can only be opened by DesignScript Studio version '{0}' and above.\n\n" +
                                                    "Click 'OK' button to visit official DesignScript website for the latest software.";

        // Regular static strings.
        public static string ProtoGeometryFriendlyName = "ProtoGeometry";
        public static string MathLibraryFriendlyName = "Math";
        public static string UserLibraryFriendlyName = "User Defined";
        public static string LoadingPreviewMessage = "loading...";

        // static string for overload display text
        public static string OverloadDisplayTextSetter = "(set)";
        public static string OverloadDisplayTextGetter = "(get)";
        public static string OverloadDisplayTextNoParameter = "NoParameter";

        //Dialog:
        public static string TitleStudio = "DesignScript Studio";
        public static string TitleImporting = "Select a Assembly Library file";
        public static string IllegalImporting = "DesignScriptStudio import supports only the .dll, .ds and .exe file format.";
        public static string ImportFailure = "Failed to Import.";
        public static string IncompatibleVersion = "Incompatible File Version";
        public static string DuplicateFailure = "The file fileName is imported already.";
        public static string EmptyWarning = "The file fileName is empty, failed to Import.";
        public static string ImportSuccess = "Import Successful.";
        public static string NodesContainError = "Selected nodes contain error";
        public static string IllegalFileName = "One or more illegal characters in file name, please rename.";
        public static string ReportIssueAcknowledge = "Thank you, your report has been sent!";
        public static string ReportIssueDisabled = "Report feedback depends on the usability data collection infrastructure. \nIn order to use this tool, you can opt in by enabling the 'usability data reporting' under the Set menu in the UI.\nOtherwise you can email us your feedback at labs.designscript@autodesk.com";
        public static string Relaunch = "Something went wrong while processing your actions. Sorry about that.\nYour files wil be available when you open DesignScript Studio again.";
    }

    public class CoreStrings
    {
        public static string SessionNameKey = "SessionNameKey";
        public static string DesignScriptSiteUrl = "http://www.designscript.org";
        public static string DesignScriptOrgReference = "http://designscript.org/reference";
        public static string IntroVideoUrl = "http://www.youtube.com/watch?v=jdY5GwTW4wY";
    }

    public class ResourceNames
    {
        public static string ResourceBaseUri = "pack://application:,,,/DesignScriptStudio.Graph.Ui;component/Resources/";
        public static string IntroductionVideo = "/DesignScriptStudio.Graph.Ui;component/Resources/intro.wmv";

        public static string Confirmation =
            "pack://application:,,,/DesignScriptStudio.Graph.Ui;component/Resources/confirmation.png";

        public static string Warning =
            "pack://application:,,,/DesignScriptStudio.Graph.Ui;component/Resources/warning.png";

        public static string Error =
            "pack://application:,,,/DesignScriptStudio.Graph.Ui;component/Resources/error.png";

        public static string Splash =
            "pack://application:,,,/DesignScriptStudio.Graph.Ui;component/Resources/splash_icon.png";

        public static string Save =
            "pack://application:,,,/DesignScriptStudio.Graph.Ui;component/Resources/save_dialog.png";

        public static string PreviewPlaceholder =
            "pack://application:,,,/DesignScriptStudio.Graph.Core;component/Resources/preview_placeholder.png";
    }

#endif

}
