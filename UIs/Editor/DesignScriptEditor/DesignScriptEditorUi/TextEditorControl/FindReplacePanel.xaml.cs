using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Automation.Peers;
using System.Windows.Automation;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace DesignScript.Editor
{
    using DesignScript.Editor.Core;
    using DesignScript.Parser;
    using ProtoCore.DSASM.Mirror;
    using ProtoFFI;
    using ProtoCore;
    using ProtoCore.Exceptions;
    using ProtoCore.CodeModel;
    using DesignScript.Editor.Automation;

    public partial class FindReplacePanel : UserControl
    {
        TextEditorControl textEditorControl = TextEditorControl.Instance;

        public FindReplacePanel()
        {
            InitializeComponent();
        }

        private void OnFindTextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            if (ReplacePanel.Visibility == Visibility.Collapsed)
            {
                ReplacePanel.Visibility = Visibility.Visible;
                ReplacePanel.Focus();
            }

            if (Solution.Current.ActiveScript == null)
                return;

            Search();
        }

        private void OnFindTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            bool isAltPressed;

            if (Solution.Current.ActiveScript == null)
                return;

            if (e.Key == Key.LeftAlt || e.Key == Key.RightAlt)
                isAltPressed = true;
            else
                isAltPressed = false;

            if ((e.SystemKey == Key.None) && isAltPressed == false)
            {
                ITextBuffer buffer = textEditorControl.TextCore.CurrentTextBuffer;
                if (e.Key == Key.Enter)
                {
                    if (buffer.SearchResult != null && buffer.SearchResult.Count != 0)
                    {
                        textEditorControl.TextCore.FindReplace(findTextbox.Text, null, FindOptions.FindNext);
                        int searchIndex = buffer.CurrentSearchIndex;
                        if (buffer.SearchResult.Count != 0 && searchIndex != -1)
                        {
                            FindPosition currentPosition = buffer.SearchResult[searchIndex];
                            textEditorControl.textCanvas.TextEditorCore.SetCursorPosition(currentPosition.startPoint.X, currentPosition.startPoint.Y);
                            textEditorControl.UpdateCaretPosition();
                            textEditorControl.UpdateScriptDisplay(Solution.Current.ActiveScript);
                        }
                    }
                }
                else
                {
                    Search();
                }
            }
        }

        private void OnDownArrorClick(object sender, RoutedEventArgs e)
        {
            ITextBuffer buffer = textEditorControl.TextCore.CurrentTextBuffer;
            if (null == Solution.Current.ActiveScript)
                return;

            if (buffer.SearchResult != null && buffer.SearchResult.Count != 0)
            {
                textEditorControl.TextCore.FindReplace(findTextbox.Text, null, FindOptions.FindNext);
                int searchIndex = buffer.CurrentSearchIndex;
                if (buffer.SearchResult.Count != 0 && searchIndex != -1)
                {
                    FindPosition currentPosition = buffer.SearchResult[searchIndex];
                    textEditorControl.textCanvas.TextEditorCore.SetCursorPosition(currentPosition.startPoint.X, currentPosition.startPoint.Y);
                    textEditorControl.UpdateCaretPosition();
                    textEditorControl.UpdateScriptDisplay(Solution.Current.ActiveScript);
                }
            }
        }

        private void OnUpArrowClick(object sender, RoutedEventArgs e)
        {
            ITextBuffer buffer = textEditorControl.TextCore.CurrentTextBuffer;
            if (Solution.Current.ActiveScript == null)
                return;

            if (buffer.SearchResult != null && buffer.SearchResult.Count != 0)
            {
                textEditorControl.TextCore.FindReplace(findTextbox.Text, null, FindOptions.FindPrevious);
                int searchIndex = buffer.CurrentSearchIndex;
                if (buffer.SearchResult.Count != 0 && searchIndex != -1)
                {
                    FindPosition currentPosition = buffer.SearchResult[searchIndex];
                    textEditorControl.textCanvas.TextEditorCore.SetCursorPosition(currentPosition.startPoint.X, currentPosition.startPoint.Y);
                    textEditorControl.UpdateCaretPosition();
                    textEditorControl.UpdateScriptDisplay(Solution.Current.ActiveScript);
                }
            }
        }

        private void OnReplaceClick(object sender, RoutedEventArgs e)
        {
            if (Solution.Current.ActiveScript == null || replaceTextbox.Text.Equals(String.Empty))
                return;

            if (textEditorControl.TextCore.ReadOnlyState == true)
                textEditorControl.DisplayStatusMessage(StatusTypes.Error, Configurations.EditingError, 3);
            else
            {
                ITextBuffer buffer = textEditorControl.TextCore.CurrentTextBuffer;
                FindOptions findOptions = FindOptions.ReplaceOnce;

                if (match_checkbox.IsChecked == true)
                {
                    findOptions |= FindOptions.MatchCase;
                    textEditorControl.TextCore.FindReplace(findTextbox.Text, replaceTextbox.Text, findOptions);
                }
                else
                    textEditorControl.TextCore.FindReplace(findTextbox.Text, replaceTextbox.Text, FindOptions.ReplaceOnce);

                int searchIndex = buffer.CurrentSearchIndex;
                if (buffer.SearchResult.Count != 0 && searchIndex != -1)
                {
                    FindPosition currentPosition = buffer.SearchResult[searchIndex];
                    textEditorControl.textCanvas.TextEditorCore.SetCursorPosition(currentPosition.startPoint.X, currentPosition.startPoint.Y);
                    textEditorControl.UpdateCaretPosition();
                    textEditorControl.UpdateScriptDisplay(Solution.Current.ActiveScript);
                }

                match_label.Text = buffer.SearchResult.Count.ToString() + " Results found";
            }

        }

        private void OnReplaceAllClick(object sender, RoutedEventArgs e)
        {
            if (Solution.Current.ActiveScript == null || replaceTextbox.Text.Equals(String.Empty))
                return;

            if (textEditorControl.TextCore.ReadOnlyState == true)
                textEditorControl.DisplayStatusMessage(StatusTypes.Error, Configurations.EditingError, 3);
            else
            {
                ITextBuffer buffer = textEditorControl.TextCore.CurrentTextBuffer;
                FindOptions findOptions = FindOptions.ReplaceAll;
                if (match_checkbox.IsChecked == true)
                {
                    findOptions |= FindOptions.MatchCase;
                    textEditorControl.TextCore.FindReplace(findTextbox.Text, replaceTextbox.Text, findOptions);
                }
                else
                    textEditorControl.TextCore.FindReplace(findTextbox.Text, replaceTextbox.Text, findOptions);

                match_label.Text = "0 Results found";
            }
        }

        private void OnExpanderMouseDown(object sender, MouseButtonEventArgs e)
        {
            expander.IsExpanded = true;
        }

        private void OnReplacePanelGotFocus(object sender, RoutedEventArgs e)
        {
            ReplacePanel.Visibility = Visibility.Visible;
        }

        private void OnMatchCaseChecked(object sender, RoutedEventArgs e)
        {
            ITextBuffer buffer = textEditorControl.TextCore.CurrentTextBuffer;
            if (Solution.Current.ActiveScript == null)
                return;

            if (buffer.SearchResult != null)
                buffer.SearchResult.Clear();
            textEditorControl.UpdateScriptDisplay(Solution.Current.ActiveScript);

            if (match_checkbox.IsChecked == true)
                textEditorControl.TextCore.FindReplace(findTextbox.Text, null, FindOptions.MatchCase);
            else
                textEditorControl.TextCore.FindReplace(findTextbox.Text, null, FindOptions.ReplaceOnce);

            int searchIndex = buffer.CurrentSearchIndex;
            if (buffer.SearchResult.Count != 0 && searchIndex != -1)
            {
                FindPosition currentPosition = buffer.SearchResult[searchIndex];
                textEditorControl.textCanvas.TextEditorCore.SetCursorPosition(currentPosition.startPoint.X, currentPosition.startPoint.Y);
                textEditorControl.UpdateCaretPosition();
                textEditorControl.UpdateScriptDisplay(Solution.Current.ActiveScript);
            }

            if (buffer.SearchResult != null)
                match_label.Text = buffer.SearchResult.Count.ToString() + " Results found";
        }

        private void Search()
        {
            ITextBuffer buffer = textEditorControl.TextCore.CurrentTextBuffer;

            if (match_checkbox.IsChecked == false)
                textEditorControl.TextCore.FindReplace(findTextbox.Text, null, FindOptions.ReplaceOnce);
            else if (match_checkbox.IsChecked == true)
                textEditorControl.TextCore.FindReplace(findTextbox.Text, null, FindOptions.MatchCase);

            if (buffer.SearchResult != null)
            {
                int searchIndex = buffer.CurrentSearchIndex;

                if (buffer.SearchResult.Count == 0)
                {
                    textEditorControl.TextCore.FindReplace(null, null, FindOptions.FindNext);
                    textEditorControl.UpdateScriptDisplay(Solution.Current.ActiveScript);
                }

                if (searchIndex != -1 && buffer.SearchResult.Count > 0)
                {
                    FindPosition currentPosition = buffer.SearchResult[searchIndex];
                    textEditorControl.textCanvas.TextEditorCore.SetCursorPosition(currentPosition.startPoint.X, currentPosition.startPoint.Y);
                    textEditorControl.UpdateCaretPosition();
                    textEditorControl.UpdateScriptDisplay(Solution.Current.ActiveScript);
                }

                match_label.Text = buffer.SearchResult.Count.ToString() + " Results found";
            }
        }

    }
}
