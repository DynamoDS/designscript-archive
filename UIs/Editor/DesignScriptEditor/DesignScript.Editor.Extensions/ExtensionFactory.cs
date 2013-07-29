using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls.Primitives;
using DesignScript.Editor.Core;
using System.Windows;
using System.Reflection;
using System.IO;
using DesignScript.Parser;

namespace DesignScript.Editor.Extensions
{
    class ExtensionPopup : Popup
    {
        private EditorExtension owningExtension = null;

        #region Public Class Operational Methods

        internal ExtensionPopup(EditorExtension owningExtension)
        {
            this.owningExtension = owningExtension;
            this.Closed += new EventHandler(OnExtensionPopupClosed);
            this.CustomPopupPlacementCallback += new CustomPopupPlacementCallback(CustomPlacement);
        }

        internal void DismissExtensionPopup(bool shiftFocusToCanvas)
        {
            this.IsOpen = false;
            if (false != shiftFocusToCanvas)
                owningExtension.SetKeyboardFocusOnCanvas();
        }

        internal void RouteEventToCanvas(RoutedEventArgs e)
        {
            owningExtension.RouteEventToCanvas(e);
        }

        #endregion

        #region Public Class Properties

        internal System.Drawing.Point CursorPosition { get; set; }

        #endregion

        #region Private Class Helper Methods

        private CustomPopupPlacement[] CustomPlacement(Size popupSize, Size targetSize, Point offset)
        {
            System.Windows.Point screenPoint = owningExtension.TranslateToScreenPoint(CursorPosition);


            CustomPopupPlacement bottomPlacement = new CustomPopupPlacement()
            {
                Point = new Point(screenPoint.X, screenPoint.Y + Configurations.FontDisplayHeight),
                PrimaryAxis = PopupPrimaryAxis.Vertical
            };

            CustomPopupPlacement topPlacement = new CustomPopupPlacement()
            {
                Point = new Point(screenPoint.X, screenPoint.Y - popupSize.Height),
                PrimaryAxis = PopupPrimaryAxis.Vertical
            };

            return (new CustomPopupPlacement[] { bottomPlacement, topPlacement });
        }

        private void OnExtensionPopupClosed(object sender, EventArgs e)
        {
        }

        #endregion
    }

    public class ExtensionFactory
    {
        /// <summary>
        /// This method is a static call made via reflection from the Caller of the Extensions.
        /// It instantiates all the IDE extensions and passes the list back to the caller, where
        /// different methods can be called for different visual behaviours from the controls.
        /// </summary>
        /// <param name="textEditorControl"> Main TextEditorControl object </param>
        /// <param name="textCore"> TextCore singleton </param>
        /// <returns></returns>
        public static List<EditorExtension> EnumerateExtensions(TextEditorControl textEditorControl, ITextEditorCore textCore)
        {
            ExtensionFactory.textEditorCore = textCore;
            List<EditorExtension> extensions = new List<EditorExtension>();

            // Instantiate all popups
            FunctionSignatureExtension functionSignatureExt = new FunctionSignatureExtension();
            AutoCompleteExtension autoCompleteExt = new AutoCompleteExtension();
            NumericSliderExtension numericSliderExt = new NumericSliderExtension();
            InspectionToolTipExtension inspectionToolTipExt = new InspectionToolTipExtension();

            // Instantiate Core
            functionSignatureExt.SetEditorCore(textEditorControl, textCore);
            autoCompleteExt.SetEditorCore(textEditorControl, textCore);
            numericSliderExt.SetEditorCore(textEditorControl, textCore);
            inspectionToolTipExt.SetEditorCore(textEditorControl, textCore);

            // Add to main list
            extensions.Add(functionSignatureExt);
            extensions.Add(autoCompleteExt);
            extensions.Add(numericSliderExt);
            extensions.Add(inspectionToolTipExt);

            return extensions;
        }

        public static List<string> GetSearchPaths()
        {
            List<string> includeDirectories = new List<string>();
            string assemblyPath = Assembly.GetAssembly(typeof(ExtensionFactory)).Location;
            includeDirectories.Add(Path.GetDirectoryName(assemblyPath));

            if (Directory.Exists(textEditorCore.TextEditorSettings.IncludePath))
                includeDirectories.Add(textEditorCore.TextEditorSettings.IncludePath);

            IScriptObject entryPointScript = Solution.Current.ActiveScript;
            if (null != entryPointScript)
            {
                IParsedScript parsedScript = entryPointScript.GetParsedScript();
                if (null != parsedScript)
                {
                    string scriptPath = parsedScript.GetScriptPath();
                    if (string.IsNullOrEmpty(scriptPath) == false)
                    {
                        string directoryName = Path.GetDirectoryName(scriptPath);
                        includeDirectories.Add(directoryName);
                    }
                }
            }

            return includeDirectories;
        }

        private static ITextEditorCore textEditorCore = null;
    }
}
