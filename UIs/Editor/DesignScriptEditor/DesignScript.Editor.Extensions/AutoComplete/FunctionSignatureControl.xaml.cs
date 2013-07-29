using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DesignScript.Editor.CodeGen;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using DesignScript.Editor.Core;

namespace DesignScript.Editor.Extensions
{
    /// <summary>
    /// Interaction logic for FunctionSignatureControl.xaml
    /// </summary>
    public partial class FunctionSignatureControl : UserControl, INotifyPropertyChanged
    {
        // Private data members.
        private string currentFunctionName = string.Empty;
        private int currentArgumentIndex = -2;

        private List<AutoCompletionHelper.MethodSignature> signatures = null;

        #region Public Class Operational Methods

        public FunctionSignatureControl()
        {
            InitializeComponent();
        }

        internal bool UpdateForFunctionArgument(int line, int column, string function, int argument)
        {
            if (string.IsNullOrEmpty(function))
                return false;

            IScriptObject activeScript = Solution.Current.ActiveScript;
            string filePath = activeScript.GetParsedScript().GetScriptPath();
            if (string.IsNullOrEmpty(filePath))
                filePath = null;

            bool updateVisual = false;
            if (currentFunctionName != function)
            {
                AutoCompletionHelper.SetSearchPaths(ExtensionFactory.GetSearchPaths());

                signatures = AutoCompletionHelper.GetMethodParameterList(
                    line, column, function, filePath);

                currentFunctionName = function;
                updateVisual = true;
            }

            if (null == signatures || (signatures.Count == 0))
                return false; // Could not retrieve any function information.

            // Update visual if there's change in argument index.
            updateVisual = (updateVisual || (this.currentArgumentIndex != argument));
            this.currentArgumentIndex = argument;

            if (false == updateVisual)
                return true; // Visual remains the same.

            if (0 == CurrentOverload || (CurrentOverload > TotalOverloads))
                CurrentOverload = 1;

            NotifyPropertyChanged("CurrentOverload");
            NotifyPropertyChanged("TotalOverloads");
            NotifyPropertyChanged("IsOverloaded");
            UpdateFormattedOutput();

            Logger.LogInfo("AutoComplete-UpdateToolTip", GetLoggingInfo());
            return true;
        }

        internal void ResetContent()
        {
            if (null != signatures)
                signatures.Clear();

            CurrentOverload = 0;
            currentFunctionName = string.Empty;
            currentArgumentIndex = -2;
            FormattedOutput.Inlines.Clear();
        }

        internal void ChangeOverload(Key key)
        {
            if (!IsOverloaded)
                return;

            if (key == Key.Down)
            {
                if (CurrentOverload < TotalOverloads)
                {
                    CurrentOverload++;
                    NotifyPropertyChanged("CurrentOverload");
                    UpdateFormattedOutput();

                    Logger.LogInfo("AutoComplete-UpdateToolTip", GetLoggingInfo());
                }
            }
            else if (key == Key.Up)
            {
                if (CurrentOverload > 1)
                {
                    CurrentOverload--;
                    NotifyPropertyChanged("CurrentOverload");
                    UpdateFormattedOutput();

                    Logger.LogInfo("AutoComplete-UpdateToolTip", GetLoggingInfo());
                }
            }
        }

        #endregion

        #region Public Class Properties

        public bool IsOverloaded { get { return (this.TotalOverloads > 1); } }
        public int CurrentOverload { get; private set; }

        public int TotalOverloads
        {
            get { return ((null == signatures) ? 0 : signatures.Count); }
        }

        #endregion

        private void OnFunctionSignatureControlLoaded(object sender, RoutedEventArgs e)
        {
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected override void OnKeyDown(KeyEventArgs e)
        {
            (this.Parent as ExtensionPopup).RouteEventToCanvas(e);
            base.OnKeyDown(e);
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            if (this.IsKeyboardFocusWithin == false)
                (this.Parent as ExtensionPopup).DismissExtensionPopup(true);

            base.OnLostKeyboardFocus(e);
        }

        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private void UpdateFormattedOutput()
        {
            FormattedOutput.Inlines.Clear();
            if (this.TotalOverloads < 1)
                return;
            if (this.CurrentOverload > this.TotalOverloads)
                return;

            AutoCompletionHelper.MethodSignature signature = null;
            signature = signatures[this.CurrentOverload - 1];

            List<string> segments = new List<string>();
            signature.FormatSegments(currentArgumentIndex, segments);

            if (segments.Count > 0)
            {
                FormattedOutput.Inlines.Add(new Run(segments[0]));
                if (segments.Count > 2)
                {
                    FormattedOutput.Inlines.Add(new Run(segments[1])
                    {
                        FontWeight = FontWeights.Bold
                    });

                    FormattedOutput.Inlines.Add(new Run(segments[2]));
                }
            }
        }

        private string GetLoggingInfo()
        {
            string currentArgument = string.Empty;
            if (this.TotalOverloads > 0)
            {
                // CurrentOverload is 1 based, not 0 based (for XAML data binding).
                if (this.CurrentOverload <= this.TotalOverloads)
                    currentArgument = signatures[CurrentOverload - 1].ToString();
            }

            return (string.Format("{0} {1} {2} {3}",
                IsOverloaded.ToString(), CurrentOverload.ToString(),
                TotalOverloads.ToString(), currentArgument));
        }
    }
}
