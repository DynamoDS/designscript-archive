using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace DesignScript.Editor
{
    using DesignScript.Parser;
    using DesignScript.Editor.Core;
    using System.Windows.Controls.Primitives;
    using System.Windows.Media;
    using System.Diagnostics;
    using System.Windows.Threading;
    using DesignScript.Editor.Automation;
    using ProtoCore;

    public partial class TextEditorControl : UserControl
    {
        private enum RunModes { Run, RunDebug };
        private RunModes prevRunMode;

        public static RoutedCommand Save = new RoutedCommand();
        public static RoutedCommand New = new RoutedCommand();
        public static RoutedCommand Open = new RoutedCommand();
        public static RoutedCommand Redo = new RoutedCommand();
        public static RoutedCommand Undo = new RoutedCommand();
        public static RoutedCommand Copy = new RoutedCommand();
        public static RoutedCommand Cut = new RoutedCommand();
        public static RoutedCommand Delete = new RoutedCommand();
        public static RoutedCommand Find = new RoutedCommand();
        public static RoutedCommand SelectAll = new RoutedCommand();
        public static RoutedCommand SaveAs = new RoutedCommand();
        public static RoutedCommand Paste = new RoutedCommand();
        public static RoutedCommand FormatDocument = new RoutedCommand();
        public static RoutedCommand StepNextCommand = new RoutedCommand();
        public static RoutedCommand StepInto = new RoutedCommand();
        public static RoutedCommand ToggleBreakpoint = new RoutedCommand();
        public static RoutedCommand RunCommand = new RoutedCommand();
        public static RoutedCommand RunDebug = new RoutedCommand();
        public static RoutedCommand StepOut = new RoutedCommand();
        public static RoutedCommand StopDebug = new RoutedCommand();
        public static RoutedCommand SetAssert = new RoutedCommand();
        public static RoutedCommand SetBaseState = new RoutedCommand();
        public static RoutedCommand DeleteCurrentLine = new RoutedCommand();
        public static RoutedCommand CommentLines = new RoutedCommand();
        public static RoutedCommand UncommentLines = new RoutedCommand();
        public static RoutedCommand CloseScript = new RoutedCommand();
        public static RoutedCommand OpenSolution = new RoutedCommand();
        public static RoutedCommand SaveSolution = new RoutedCommand();
        public static RoutedCommand CloseSolution = new RoutedCommand();
        public static RoutedCommand DecreaseSize = new RoutedCommand();
        public static RoutedCommand AsynchronousCommand = new RoutedCommand();
        public static RoutedCommand IncreaseSize = new RoutedCommand();
        public static RoutedCommand SearchBoxFocus = new RoutedCommand();
        public static RoutedCommand CloseMatchPanel = new RoutedCommand();
        public static RoutedCommand ShowFunctionSignature = new RoutedCommand();
        public static RoutedCommand SlowMotion = new RoutedCommand();
        public static RoutedCommand DisplayOutput = new RoutedCommand();
        public static RoutedCommand SmartFormatter = new RoutedCommand();
        public static RoutedCommand NumericSlider = new RoutedCommand();
        public static RoutedCommand SlowMotionValue = new RoutedCommand();

        private bool IsExecuting()
        {
            bool isInDebugMode = false;
            return (IsExecuting(ref isInDebugMode));
        }

        private bool IsExecuting(ref bool isInDebugMode)
        {
            isInDebugMode = false;
            IExecutionSession session = Solution.Current.ExecutionSession;
            if (session != null)
                return (session.IsExecutionActive(ref isInDebugMode));

            return false;
        }

        private void DeleteCurrentLineCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = true;
        }

        private void DeleteCurrentLineExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            if (textCore.ReadOnlyState == true)
                DisplayStatusMessage(StatusTypes.Error, Configurations.EditingError, 3);
            else
            {
                textCore.DeleteCurrentLine();
                textCanvas.BreakpointsUpdated();
            }
        }

        private void StepOutCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            bool isInDebugMode = false;
            IsExecuting(ref isInDebugMode);
            e.CanExecute = isInDebugMode;
            e.Handled = true;
        }

        private void StepOutExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;

            Logger.LogDebug("Text-Editor-Control-OnStepOutButton", "UserPressed");

            Stopwatch sw = new Stopwatch();
            sw.Start();

            UpdateUiForStepNext(textCore.Step(RunMode.StepOut));
            Logger.LogPerf("Text-Editor-Control-TextEditorControl.OnStepOutButton", sw.ElapsedMilliseconds + " ms");
        }

        private void RunCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            if (Solution.Current.ActiveScript == null)
                e.CanExecute = false;
            else
                e.CanExecute = !IsExecuting();
        }

        // This is to prevent re-entrant of button event.
        static bool RunDebugExecutedInProgress = false;

        private void RunExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            RunScriptInternal();
        }

        private void RunDebugCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            if (Solution.Current.ActiveScript == null)
                e.CanExecute = false;
            else
                e.CanExecute = true;
        }

        private void RunDebugExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            DebugScriptInternal();
        }

        private void CloseMatchPanelCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = true;
        }

        private void CloseMatchPanelExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            findPane.ReplacePanel.Visibility = Visibility.Collapsed;
            textCanvas.Focus();
        }

        private void DisplayOutputCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            if (Solution.Current.ActiveScript == null)
                e.CanExecute = false;
            else
                e.CanExecute = true;
        }

        private void DisplayOutputExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ITextEditorSettings settings = textCore.TextEditorSettings;
            settings.ToggleDisplayOutput(); // Toggle display output option.
        }

        private void SmartFormatterCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            if (Solution.Current.ActiveScript == null)
                e.CanExecute = false;
            else
                e.CanExecute = true;
        }

        private void SmartFormatterExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ITextEditorSettings settings = textCore.TextEditorSettings;
            settings.ToggleSmartFormatting(); // Toggle SmartFormatter option.
        }

        private void NumericSliderCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            if (Solution.Current.ActiveScript == null)
                e.CanExecute = false;
            else
                e.CanExecute = true;
        }

        private void NumericSliderExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ITextEditorSettings settings = textCore.TextEditorSettings;
            settings.ToggleNumericSlider(); // Toggle Numeric Slider option.
        }

        private void SlowMotionValueCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            if (Solution.Current.ActiveScript == null)
                e.CanExecute = false;
            else
                e.CanExecute = true;
        }

        private void SlowMotionValueExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        private void ShowFunctionSignatureCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = true;
        }

        private void ShowFunctionSignatureExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            int x = textCore.CursorPosition.X;
            int y = textCore.CursorPosition.Y;
            ITextBuffer textBuffer = textCore.CurrentTextBuffer;
            FunctionCallContext callContext = textBuffer.GetFunctionCallContext(y, x);

            string message = "No function call context";
            if ((null != callContext) && callContext.IsValidCallContext)
            {
                int argumentIndex = -1;
                string functionName = callContext.GetFunctionAtPoint(x, y, out argumentIndex);
                if (!string.IsNullOrEmpty(functionName))
                    message = string.Format("Function: {0}, Argument Index: {1}", functionName, argumentIndex);
            }

            this.DisplayStatusMessage(StatusTypes.Info, message, 2);
        }

        private void SearchBoxFocusCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = true;
        }

        private void SearchBoxFocusExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            findPane.findTextbox.Focus();
            Keyboard.Focus(findPane.findTextbox);
        }

        private void ToggleBreakpointCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = true;
        }

        private void ToggleBreakpointExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void FormatDocumentCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = true;
        }

        private void FormatDocumentExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Logger.LogDebug("Text-Editor-Control-FormatDocumentExecuted", "FormatDocumentExecuted");


            e.Handled = true;
            if (!textCore.TextEditorSettings.EnableSmartFormatting)
                return;
            textCore.FormatDocument();
        }

        private void StepIntoCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            bool isInDebugMode = false;
            IsExecuting(ref isInDebugMode);
            e.CanExecute = isInDebugMode;
            e.Handled = true;
        }

        private void StepIntoExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            Logger.LogDebug("Text-Editor-Control-OnStepIntoButton", "UserPressed");

            Stopwatch sw = new Stopwatch();
            sw.Start();

            UpdateUiForStepNext(textCore.Step(RunMode.StepIn));
            Logger.LogPerf("Text-Editor-Control-TextEditorControl.OnStepIntoButton", sw.ElapsedMilliseconds + " ms");
        }

        private void StepNextCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            bool isInDebugMode = false;
            IsExecuting(ref isInDebugMode);
            e.CanExecute = isInDebugMode;
            e.Handled = true;
        }

        private void StepNextExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            Logger.LogDebug("Text-Editor-Control-StepNextExecuted", "UserPressed");

            Stopwatch sw = new Stopwatch();
            sw.Start();

            UpdateUiForStepNext(textCore.Step(RunMode.StepNext));
            Logger.LogPerf("TextEditorControl.StepNextButton", sw.ElapsedMilliseconds + " ms");
        }

        private void SaveCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            if (null == Solution.Current.ActiveScript)
            {
                e.CanExecute = false;
                return;
            }

            e.CanExecute = true;
            ITextBuffer textBuffer = Solution.Current.ActiveScript.GetTextBuffer();
            if (textBuffer.ScriptModified == false || IsExecuting())
            {
                e.CanExecute = false;
            }
        }

        private void SaveExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Logger.LogDebug("Text-Editor-Control-SaveExecuted", "SaveExecuted");

            e.Handled = true;
            textCore.SaveScriptToFile(false);
        }

        private void NewScriptExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            Logger.LogDebug("Text-Editor-Control-OnNewScriptButton", "UserPressed");
            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (parseTimer == null)
            {
                parseTimer = new DispatcherTimer();
                parseTimer.Tick += new EventHandler(OnParseTimerTick);
                parseTimer.Interval = new TimeSpan(0, 0, 1);
                parseTimer.Start();
            }
            newDocCount++;

            textCore.CreateNewScript();

            SetupTabInternal(null);
            HandleScriptActivation();

            grid.UpdateLayout();
            CommandManager.InvalidateRequerySuggested();
            textCanvas.Focus();

            Logger.LogPerf("TextEditorControl.OnNewScriptButton", sw.ElapsedMilliseconds + " ms");
        }

        private void NewScriptCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !IsExecuting();
            e.Handled = true;
        }

        private void OpenScriptCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !IsExecuting();
            e.Handled = true;
        }

        private void OpenScriptExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            Logger.LogDebug("Text-Editor-Control-OnOpenScriptButton", "UserPressed");
            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (null == TextEditorControl.HostApplication)
                return; // Nothing to do.

            string filePath = TextEditorControl.HostApplication.PromptScriptSelection();
            if (string.IsNullOrEmpty(filePath))
                return; // Nothing to do, again.

            if (parseTimer == null)
            {
                parseTimer = new DispatcherTimer();
                parseTimer.Tick += new EventHandler(OnParseTimerTick);
                parseTimer.Interval = new TimeSpan(0, 0, 1);
                parseTimer.Start();
            }

            if (textCore.LoadScriptFromFile(filePath))
                SetupTabInternal(filePath);
            else
            {
                // Even though "LoadScriptFromFile" returns false, it may be 
                // due to the fact that the script has already been opened. 
                // In such cases, we will be able to get the script index 
                // given its name. If the index turns out to be valid here,
                // then we will simply activate the corresponding tab.
                // 
                int index = Solution.Current.GetScriptIndexFromPath(filePath);
                if (index >= 0)
                    ScriptTabControl.ActivateTab(index);
                else
                    MessageBox.Show(Configurations.UnsupportedFileType);
            }

            HandleScriptActivation();

            grid.UpdateLayout();
            CommandManager.InvalidateRequerySuggested();
            textCanvas.Focus();

            IScriptObject script = Solution.Current.ActiveScript;
            Logger.LogPerf("TextEditorControl.OnOpenScriptButton", sw.ElapsedMilliseconds + " ms");
            if (null != script)
                Logger.LogInfo("Text-Editor-Control-OnOpenScriptButton-Script", script.GetTextBuffer().GetContent());
        }

        private void SaveAsCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            if (Solution.Current.ActiveScript == null)
                e.CanExecute = false;
            else
                e.CanExecute = !IsExecuting();
        }

        private void SaveAsExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Logger.LogDebug("SaveAsExecuted", "SaveAsExecuted");

            e.Handled = true;
            textCore.SaveScriptToFile(true);
        }

        private void UndoCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            if (Solution.Current.ActiveScript == null)
                e.CanExecute = false;
            else
                e.CanExecute = !IsExecuting();
        }

        private void UndoExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Logger.LogDebug("Text-Editor-Control-UndoExecuted", "UndoExecuted");


            if (textCore.ReadOnlyState == true)
                DisplayStatusMessage(StatusTypes.Error, Configurations.EditingError, 3);
            e.Handled = true;
            //Clearin the find replace highlight
            textCore.FindReplace(null, null, FindOptions.FindNext);
            textCore.UndoEditing();
            textCanvas.BreakpointsUpdated();
            UpdateCaretPosition();
            UpdateUiForModifiedScript(Solution.Current.ActiveScript);
            CommandManager.InvalidateRequerySuggested();
        }

        private void RedoCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            if (Solution.Current.ActiveScript == null)
                e.CanExecute = false;
            else
                e.CanExecute = !IsExecuting();
        }

        private void RedoExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Logger.LogDebug("Text-Editor-Control-RedoExecuted", "RedoExecuted");


            if (textCore.ReadOnlyState == true)
                DisplayStatusMessage(StatusTypes.Error, Configurations.EditingError, 3);
            e.Handled = true;
            textCore.RedoEditing();
            textCanvas.BreakpointsUpdated();
            UpdateCaretPosition();
            UpdateUiForModifiedScript(Solution.Current.ActiveScript);
            CommandManager.InvalidateRequerySuggested();
        }

        private void StopCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = IsExecuting();
        }

        private void StopExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            Logger.LogDebug("Text-Editor-Control-OnStopButton", "UserPressed");
            Stopwatch sw = new Stopwatch();
            sw.Start();

            UpdateUiForStop(textCore.Stop());

            Logger.LogPerf("Text-Editor-Control-TextEditorControl.OnStopButton", sw.ElapsedMilliseconds + " ms");
        }

        private void SelectAllCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = true;
        }

        private void SelectAllExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Logger.LogDebug("Text-Editor-Control-SelectAllExecuted", "SelectAllExecuted");

            e.Handled = true;
            textCore.SelectAllText();
            UpdateCaretPosition();
        }

        private void PasteCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            if (Solution.Current.ActiveScript == null)
                e.CanExecute = false;
            else
                e.CanExecute = !IsExecuting();
        }

        private void PasteExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Logger.LogDebug("Text-Editor-Control-PasteExecuted", "PasteExecuted");


            e.Handled = true;
            if (textCore.ReadOnlyState == true)
            {
                DisplayStatusMessage(StatusTypes.Error, Configurations.EditingError, 3);
                return;
            }

            textCore.DoPasteText();
            textCanvas.BreakpointsUpdated();
            UpdateCaretPosition();
            UpdateUiForModifiedScript(Solution.Current.ActiveScript);
        }

        private void FindCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = true;
        }

        private void FindExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Logger.LogDebug("Text-Editor-Control-FindExecuted", "FindExecuted");

            e.Handled = true;

        }

        private void DeleteCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = true;
        }

        private void DeleteExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Logger.LogDebug("Text-Editor-Control-DeleteExecuted", "DeleteExecuted");

            e.Handled = true;

        }

        private void CutCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            if (Solution.Current.ActiveScript == null)
                e.CanExecute = false;
            else
                e.CanExecute = !IsExecuting() && textCore.HasSelection;
        }

        private void CutExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Logger.LogDebug("Text-Editor-Control-CutExecuted", "CutExecuted");

            e.Handled = true;

            parseTimer.Stop();
            parseTimer.Start();
            textCanvas.PauseCaretTimer(true);
            textCanvas.PauseCaretTimer(false);

            textCore.DoCopyText(true);
            if (textCore.ReadOnlyState == true)
                DisplayStatusMessage(StatusTypes.Error, Configurations.EditingError, 3);
        }

        private void CopyCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            if (Solution.Current.ActiveScript == null)
                e.CanExecute = false;
            else
                e.CanExecute = textCore.HasSelection;
        }

        private void CopyExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Logger.LogDebug("Text-Editor-Control-CopyExecuted", "CopyExecuted");

            e.Handled = true;

            parseTimer.Stop();
            parseTimer.Start();
            textCanvas.PauseCaretTimer(true);
            textCanvas.PauseCaretTimer(false);

            textCore.DoCopyText(false);

            UpdateCaretPosition();
            UpdateUiForModifiedScript(Solution.Current.ActiveScript);
        }

        private void CommentCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            if (Solution.Current.ActiveScript == null)
                e.CanExecute = false;
            else
                e.CanExecute = !IsExecuting();
        }

        private void CommentExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Logger.LogDebug("Text-Editor-Control-CommentExecuted", "CommentExecuted");

            e.Handled = true;
            DoCommentLines(true);
        }

        private void UncommentCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            if (Solution.Current.ActiveScript == null)
                e.CanExecute = false;
            else
                e.CanExecute = !IsExecuting();
        }

        private void UncommentExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Logger.LogDebug("Text-Editor-Control-UncommentExecuted", "UncommentExecuted");

            e.Handled = true;
            DoCommentLines(false);
        }

        private void SetAssertCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = true;
        }

        private void SetAssertExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Logger.LogDebug("Text-Editor-Control-SetAssertExecuted", "SetAssertExecuted");

            if (null == asserts)
                asserts = new List<CommandAssert>();

            AddAssert addAssert = new AddAssert(textCore.GetAssertableProperties());
            if (addAssert.ShowDialog() == false)
                actionRecorder.AddAssertsToCurrentCommand(addAssert.GetAsserts());
        }

        private void SetBaseStateCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = true;
        }

        private void SetBaseStateExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (null == actionRecorder)
                return;

            string question = "Set Base State for recorder here?";
            string caption = "DesignScript IDE - Base State";
            MessageBoxResult result = MessageBox.Show(question, caption, MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
                actionRecorder.SetBaseState();
        }

        private void CloseScriptCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            if (Solution.Current.ActiveScript == null)
                e.CanExecute = false;
            else
                e.CanExecute = true;
        }

        private void CloseScriptExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Logger.LogDebug("Text-Editor-Control-CloseScriptExecuted", "CloseScriptExecuted");


            ScriptTabControl.CloseCurrentTab();
        }

        private void OpenSolutionCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !IsExecuting();
            e.Handled = true;
        }

        private void OpenSolutionExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Logger.LogDebug("Text-Editor-Control-OpenSolutionExecuted", "OpenSolutionExecuted");

            // Check to see if the solution needs saving. But then we cannot do 
            // this because we ALWAYS have a start-up script, so user will be 
            // prompt to save the solution even if she has not done anything after 
            // launching DesignScript Studio.
            // 
            // if (SaveSolutionInternal() == false)
            //    return;


            if (OpenSolutionInternal() == false)
                return;

            ScriptTabControl.CloseAllTabs();

            int count = Solution.Current.ScriptCount;
            for (int index = 0; index < count; ++index)
            {
                string path = Solution.Current.GetScriptPathFromIndex(index);
                string name = System.IO.Path.GetFileName(path);
                ScriptTabControl.InsertNewTab(name, scrollViewer);
            }

            int activeScript = Solution.Current.ActiveScriptIndex;
            ScriptTabControl.ActivateTab(activeScript);

            HandleScriptActivation();

            // Remove and add all expressions stored in the solution.
            InspectionViewControl.Instance.PopulateVariablesFromSolution();
            CommandManager.InvalidateRequerySuggested();
        }


        private void CloseSolutionCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            if (null == Solution.Current.ActiveScript)
            {
                e.CanExecute = false;
                return;
            }

            e.CanExecute = !IsExecuting();
        }

        private void CloseSolutionExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Logger.LogDebug("Text-Editor-Control-CloseSolutionExecuted", "CloseSolutionExecuted");

            if (Solution.Current.IsModified != false && Solution.Current.ShowSaveDialog != false)
            {
                MessageBoxResult result = MessageBoxResult.Cancel;
                result = MessageBox.Show(Configurations.ConfirmSaveSolution,
                    "Save Solution", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        if (SaveSolutionInternal() == false)
                            return;
                        Solution.CloseSolution(Solution.Current, false);
                        break;

                    case MessageBoxResult.No:
                        // Proceed to discard the solution.
                        Solution.CloseSolution(Solution.Current, true);
                        break;

                    case MessageBoxResult.Cancel:
                        return; // Abort close solution.
                }
            }
            else
                Solution.CloseSolution(Solution.Current, false);

            ScriptTabControl.CloseAllTabs();
            ShowTabControls(false);
            OutputWindow.ClearOutput();

            InspectionViewControl.Instance.RemoveAllVariables();
            CommandManager.InvalidateRequerySuggested();
        }

        private void SaveSolutionCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            if (null == Solution.Current.ActiveScript)
            {
                e.CanExecute = false;
                return;
            }

            e.CanExecute = !IsExecuting();
        }

        private void SaveSolutionExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Logger.LogDebug("Text-Editor-Control-SaveSolutionExecuted", "SaveSolutionExecuted");

            SaveSolutionInternal();
        }

        private void IncreaseSizeCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            if (Solution.Current.ActiveScript == null)
                e.CanExecute = false;
            else
                e.CanExecute = true;
        }

        private void IncreaseSizeExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Logger.LogDebug("Text-Editor-Control-IncreaseSizeExecuted", "IncreaseSizeExecuted");

            Configurations.IncreaseFontSize();
            TextEditorControl.Instance.UpdateCanvasDimension();
            TextEditorControl.Instance.UpdateScriptDisplay(Solution.Current.ActiveScript);
            TextEditorControl.Instance.UpdateCaretPosition();
        }

        private void DecreaseSizeCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            if (Solution.Current.ActiveScript == null)
                e.CanExecute = false;
            else
                e.CanExecute = true;
        }

        private void AsynchronousCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            e.CanExecute = false;

#if DEBUG
            // This option is for internal use.
            e.CanExecute = !IsExecuting();
#endif
        }

        private void AsynchronousCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;

#if DEBUG
            // This option is not read for public use yet.
            Solution.Current.Asynchronous = AsynchronousItem.IsChecked;
#endif
        }

        private void SlowMotionCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
            if (Solution.Current.ActiveScript == null)
                e.CanExecute = false;
            else
                e.CanExecute = !IsExecuting();
        }

        private void SlowMotionExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
            if (textCore.ReadOnlyState == true)
                DisplayStatusMessage(StatusTypes.Error, Configurations.SlowMotionError, 3);
            else
            {
                double timeInterval = Convert.ToDouble(txtSlowMoTimer.Text);
                SlowMotionPlayBack(timeInterval);
            }
        }


        private void DecreaseSizeExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Logger.LogDebug("Text-Editor-Control-DecreaseSizeExecuted", "DecreaseSizeExecuted");

            Configurations.DecreaseFontSize();
            TextEditorControl.Instance.UpdateCanvasDimension();
            TextEditorControl.Instance.UpdateScriptDisplay(Solution.Current.ActiveScript);
            TextEditorControl.Instance.UpdateCaretPosition();
        }

        private void OnScriptCountChanged(object sender, ScriptCountChangedEventArgs e)
        {
            // Subscribe to Modified event on newly added text buffers.
            foreach (IScriptObject addedScript in e.ScriptsAdded)
            {
                ITextBuffer textBuffer = addedScript.GetTextBuffer();
                textBuffer.Modified += scriptModifiedHandler;
            }

            // Unsubscribe from text buffers that are being removed.
            foreach (IScriptObject removedScript in e.ScriptsRemoved)
            {
                ITextBuffer textBuffer = removedScript.GetTextBuffer();
                textBuffer.Modified -= scriptModifiedHandler;
            }
        }

        private void OnTextBufferModified(object sender, EventArgs e)
        {
            ITextBuffer textBuffer = (ITextBuffer)sender;
            if (textBuffer == null)
                return;

            // Changing Save Button State
            //this.SaveButton.IsEnabled = textBuffer.ScriptModified;
            //this.SaveItem.IsEnabled = textBuffer.ScriptModified;

            //Changing name of the script on CurvyTab
            int tabIndex;
            IScriptObject owningScript = textBuffer.OwningScript;
            if (owningScript == null)
                tabIndex = ScriptTabControl.CurrentTab;
            else
            {
                string currentScriptPath = owningScript.GetParsedScript().GetScriptPath();
                tabIndex = Solution.Current.GetScriptIndexFromPath(currentScriptPath);
            }

            string currentDisplayText = ScriptTabControl.GetDisplayText(tabIndex);
            ScriptTabControl.SetHighlightTab(tabIndex, textBuffer.ScriptModified);

            if (textBuffer.ScriptModified == true)
            {
                ScriptTabControl.SetDisplayText(tabIndex, "*" + currentDisplayText);
            }

            if (textBuffer.ScriptModified == false)
                UpdateTabDisplayText();
        }

        private void OnTextCanvasGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (textCore.ReadOnlyState == false)
            {
                List<int> scriptsToClose = new List<int>();
                textCore.CheckForExternalModifications(ref scriptsToClose);
                if (scriptsToClose.Count > 0)
                {
                    foreach (int index in scriptsToClose)
                    {
                        ScriptTabControl.CloseTab(index);
                        textCore.CloseScript(index);
                    }

                    // IDE-644 Crash on file reload if existing open file renamed in explorer.
                    // If at the end of all these there's no script left, clean up script display.
                    if (Solution.Current.ScriptCount <= 0)
                    {
                        ShowTabControls(false);
                        CheckTabControlVisibility(true);
                    }
                    else
                    {
                        // Repaint text canvas...
                        HandleScriptActivation();
                    }
                }
            }
        }

        private void DoCommentLines(bool commentText)
        {
            Logger.LogDebug("Text-Editor-Control-DoCommentLines", "DoCommentLines");

            if (textCore.ReadOnlyState == true)
            {
                DisplayStatusMessage(StatusTypes.Error, Configurations.EditingError, 3);
                return;
            }
            if (textCore.CurrentTextBuffer.GetLineCount() == 0)
                return;
            textCore.CommentLines(commentText);
            textCanvas.BreakpointsUpdated();
            UpdateCaretPosition();
            UpdateUiForModifiedScript(Solution.Current.ActiveScript);
        }
    }
}
