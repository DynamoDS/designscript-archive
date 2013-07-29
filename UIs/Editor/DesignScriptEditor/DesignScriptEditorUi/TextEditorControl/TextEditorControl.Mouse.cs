using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

namespace DesignScript.Editor
{
    using ProtoCore.CodeModel;
    using System.Diagnostics;
    using DesignScript.Parser;
    using DesignScript.Editor.Core;
    using System.Windows.Threading;

    public partial class TextEditorControl : UserControl
    {
        int clickedLineIndex = -1;
        int breakpointLineY = -1;
        bool mouseCursorCaptured = false;
        bool ignoreMouseMoveEvent = false;
        DispatcherTimer inspectionToolTipTimer = null;
        System.Drawing.Point mouseCharacterPosition;

        #region Scrollviewer Mouse Events

        private void OnScrollViewerMouseMove(object sender, MouseEventArgs e)
        {
            // When "scrollViewer.CaptureMouse()" is called, mouse-move event will be 
            // sent before "CaptureMouse" returns. So if this event is sent due to the 
            // mouse being captured, then we won't process it at all.
            // 
            if (false != ignoreMouseMoveEvent)
                return;

            IScriptObject activeScript = Solution.Current.ActiveScript;
            if (activeScript == null)
                return; // There's no active script just yet.

            if (IsMouseInClickableRegion(sender, e) == false)
                return;

            // Retreive the coordinates of the mouse move event. Note that we always 
            // want the mouse coordinates relative to the top-left corner of the canvas 
            // instead of the scroll viewer (whose top coordinates are off if scroll 
            // offset is not at the top of the view).
            // 
            CharPosition cursor = activeScript.CreateCharPosition();
            System.Windows.Point screenPoint = GetRelativeCanvasPosition(e);
            cursor.SetScreenPosition(screenPoint.X, screenPoint.Y);
            mouseCharacterPosition = cursor.GetCharacterPosition();

            if (-1 == clickedLineIndex)
            {
                if (textCore.InternalDragSourceExists == false)
                {
                    // There is nothing being dragged right now.
                    textCore.SetMouseMovePosition(mouseCharacterPosition.X, mouseCharacterPosition.Y, e);
                }
                else
                {
                    // Stop caret blinking while dragging.
                    textCanvas.PauseCaretTimer(true);
                    DragDrop.AddDragOverHandler(scrollViewer, OnDragOver);
                    DragDrop.AddDropHandler(scrollViewer, OnDrop);

                    try
                    {
                        string textToMove = textCore.SelectionText;
                        textToMove = textToMove.Replace("\n", "\r\n");

                        // Beginning the modal loop of "DoDragDrop" will immediately trigger
                        // a mouse-mvoe event before it returns (e.g. drop has been made or 
                        // cancelled). So here we set the "ignoreMouseMoveEvent" to true and 
                        // ignore the immediate mouse event from being processed (which will 
                        // result in yet another "DoDragDrop" being called, causing two drop 
                        // operations).
                        // 
                        ignoreMouseMoveEvent = true;
                        DragDrop.DoDragDrop(textCanvas, textToMove, DragDropEffects.All);
                        ignoreMouseMoveEvent = false;
                    }
                    finally
                    {
                        DragDrop.RemoveDragOverHandler(scrollViewer, OnDragOver);
                        DragDrop.RemoveDropHandler(scrollViewer, OnDrop);

                        textCore.ClearDragDropState();
                        textCanvas.PauseCaretTimer(false); // Resume caret blinking...
                    }
                }

                // We turn the cursor to an arrow if it is within the breakpoint or line 
                // column, and I-beam otherwise. Clicking on the breakpoint column triggers 
                // a breakpoint and clicking on the line column selects the entire line.
                // 
                scrollViewer.Cursor = Cursors.Arrow;
                if (!IsPointWithinLineColumn(screenPoint))
                {
                    if (!IsPointWithinBreakColumn(GetRelativeCanvasPosition(e)))
                    {
                        if (!textCore.IsPointInSelection(mouseCharacterPosition.X, mouseCharacterPosition.Y))
                            scrollViewer.Cursor = Cursors.IBeam;
                    }
                }

                if (textCore.ReadOnlyState && (false != TextCanvasHasKeyboardFocus))
                {
                    if (inspectionToolTipTimer == null)
                    {
                        inspectionToolTipTimer = new DispatcherTimer();
                        inspectionToolTipTimer.Tick += new EventHandler(OnInspectionToolTipTimerTick);

                        // One has to hover over a word for 3/4ths of a second to be considered 
                        // a valid 'Mouse Hover'. Yes, this is decided spontaneously.
                        inspectionToolTipTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
                    }

                    // Restart timer as cursor moves.
                    inspectionToolTipTimer.Stop();
                    inspectionToolTipTimer.Start();
                }
            }
            else
            {
                scrollViewer.Cursor = Cursors.Arrow;
                int currentLine = mouseCharacterPosition.Y;
                textCore.SelectLines(clickedLineIndex, currentLine - clickedLineIndex);
            }

            if ((e.LeftButton & MouseButtonState.Pressed) != 0)
            {
                // If this message is sent by dragging a thumb on a scrollbar, then the 
                // user wishes to scroll the text canvas, in which case we should not force 
                // to bring the caret into view (which is what 'UpdateCaretPosition' does).
                // 
                if ((e.OriginalSource as Thumb) == null)
                    UpdateCaretPosition();
            }
        }

        /// <summary>
        /// Inspection tooltip timer has ticked and a check must be made to see if it is hovering
        /// over the same fragment as before the tick and if so, trigger an inspection tooltip if it 
        /// is a valid variable.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnInspectionToolTipTimerTick(object sender, EventArgs e)
        {
            inspectionToolTipTimer.Stop();
            CodeFragment currFragment = null;
            textCore.GetFragmentForInspection(mouseCharacterPosition.X, mouseCharacterPosition.Y, out currFragment);
            
            foreach (EditorExtension extension in editorExtensions)
                extension.OnMouseHover(currFragment);
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
            if ((e.KeyStates & DragDropKeyStates.ControlKey) != 0)
                e.Effects = DragDropEffects.Copy;

            // First, validate arbitrary screen coordinates...
            IScriptObject activeScript = Solution.Current.ActiveScript;
            CharPosition mousePoint = activeScript.CreateCharPosition();
            mousePoint.SetScreenPosition(GetRelativeCanvasPosition(e));

            // Here when the content is dragged to the first line in view, try to 
            // bring it onto the previous line. This is so that the previous line 
            // gets to scroll into view later on. If the cursor is on the last 
            // visible line, then try to set it one line beyond that. This is so 
            // that the next line get to scroll into view the same way.
            // 
            int characterY = mousePoint.CharacterY;
            int lastVisibleLine = textCanvas.FirstVisibleLine + textCanvas.MaxVisibleLines;
            if (characterY == textCanvas.FirstVisibleLine)
                characterY = characterY - 1;
            else if (characterY >= lastVisibleLine - 1)
                characterY = characterY + 1;

            mousePoint.SetCharacterPosition(mousePoint.CharacterX, characterY);

            // Then get the precised cursor coordinates, and then set the 
            // character position so we get screen coordinates at character 
            // boundaries...
            System.Drawing.Point cursor = mousePoint.GetCharacterPosition();
            mousePoint.SetCharacterPosition(cursor);
            textCanvas.SetCursorScreenPos(mousePoint.GetScreenPosition());
            textCanvas.EnsureCursorVisible(mousePoint.CharacterX, mousePoint.CharacterY);
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            if (textCore.ReadOnlyState == true)
                DisplayStatusMessage(StatusTypes.Error, Configurations.EditingError, 3);

            e.Handled = true;
            IScriptObject activeScript = Solution.Current.ActiveScript;
            CharPosition mousePoint = activeScript.CreateCharPosition();
            mousePoint.SetScreenPosition(GetRelativeCanvasPosition(e));

            bool copyText = ((e.KeyStates & DragDropKeyStates.ControlKey) != 0);
            System.Drawing.Point destination = mousePoint.GetCharacterPosition();
            textCore.MoveSelectedText(destination.X, destination.Y, copyText);
            textCanvas.BreakpointsUpdated();
            UpdateCaretPosition();
            UpdateUiForModifiedScript(Solution.Current.ActiveScript);
        }

        private void OnScrollViewerLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (null == Solution.Current.ActiveScript)
                return; // No active script.

            if (e.ClickCount > 1)
            {
                HandleMultipleClicks(e);
                return;
            }

            // We need the canvas to be focusable in order to receive key inputs.
            System.Diagnostics.Debug.Assert(textCanvas.Focusable == true);
            textCanvas.Focus(); // Set input focus on the canvas.

            if (IsMouseInClickableRegion(sender, e) == false)
                return;
            System.Windows.Point screenPoint = GetRelativeCanvasPosition(e);
            IScriptObject activeScript = Solution.Current.ActiveScript;
            CharPosition mousePoint = activeScript.CreateCharPosition();
            mousePoint.SetScreenPosition(screenPoint);
            System.Drawing.Point cursor = mousePoint.GetCharacterPosition();

            breakpointLineY = -1;
            if (IsPointWithinLineColumn(screenPoint))
            {
                clickedLineIndex = cursor.Y;
                textCore.SelectLines(cursor.Y, 0);
            }
            else
            {
                clickedLineIndex = -1;
                if (IsPointWithinBreakColumn(GetRelativeCanvasPosition(e)))
                    breakpointLineY = cursor.Y;

                textCore.SetMouseDownPosition(cursor.X, cursor.Y, e);
            }

            // Capturing mouse input results in an immediate mouse-move event, 
            // but we won't want to handle that as we know that we are 
            // currently in a button-down event. So here we ignore the immediate 
            // mouse-move event by setting "stopMouseMoveReentrant" to true.
            // 
            ignoreMouseMoveEvent = true;
            TextEditorScrollViewer scrollViewer = sender as TextEditorScrollViewer;
            mouseCursorCaptured = scrollViewer.CaptureMouse();
            ignoreMouseMoveEvent = false;
            UpdateCaretPosition();
        }

        private void OnScrollViewerLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (false != mouseCursorCaptured)
            {
                mouseCursorCaptured = false;
                // Similarly on Releasing the mouse capture results in a mouse-move
                // event, which we ignore.
                ignoreMouseMoveEvent = true;
                ScrollViewer scrollViewer = sender as ScrollViewer;
                scrollViewer.ReleaseMouseCapture();
                ignoreMouseMoveEvent = false;
            }

            IScriptObject activeScript = Solution.Current.ActiveScript;
            ITextBuffer textBuffer = ((null == activeScript) ? null : activeScript.GetTextBuffer());
            if (null == textBuffer)
                return;

            CharPosition mousePoint = activeScript.CreateCharPosition();
            mousePoint.SetScreenPosition(GetRelativeCanvasPosition(e));
            System.Drawing.Point cursor = mousePoint.GetCharacterPosition();
            System.Windows.Point canvasPosition = GetRelativeCanvasPosition(e);

            if (IsPointWithinBreakColumn(canvasPosition))
            {
                int line = mousePoint.CharacterY;
                bool isInDebugMode = false;

                if (breakpointLineY == cursor.Y)
                {
                    if (textCore.ToggleBreakpoint())
                        textCanvas.BreakpointsUpdated();
                    else if (string.IsNullOrEmpty(activeScript.GetParsedScript().GetScriptPath()))
                        DisplayStatusMessage(StatusTypes.Warning, Configurations.SaveFileBreakpointWarning, 3);
                    else if (Solution.Current.ExecutionSession.IsExecutionActive(ref isInDebugMode))
                        DisplayStatusMessage(StatusTypes.Warning, Configurations.BreakpointDebugWarning, 3);
                }

                scrollViewer.Cursor = Cursors.Arrow;
            }
            else
            {
                if (null != editorExtensions)
                {
                    foreach (EditorExtension extension in editorExtensions)
                        extension.OnMouseUp(e);
                }

                if (IsPointOnInlineIconColumn(canvasPosition))
                {
                    int line = mousePoint.CharacterY;
                    bool runScript = false;
                    List<InlineMessageItem> outputMessages = Solution.Current.GetInlineMessage();
                    if (null != outputMessages)
                    {
                        foreach (InlineMessageItem message in outputMessages)
                        {
                            if (message.Line == line)
                            {
                                if (message.Type == InlineMessageItem.OutputMessageType.PossibleError ||
                                    message.Type == InlineMessageItem.OutputMessageType.PossibleWarning)
                                    runScript = true;
                            }
                        }
                    }
                    if (runScript == true)
                    {
                        if (prevRunMode.Equals(RunModes.Run))
                        {
                            RunScript();
                            prevRunMode = RunModes.Run;
                        }
                        else
                        {
                            if (false == textCore.ReadOnlyState)
                                OutputWindow.ClearOutput();

                            // Activate Watch Window tab
                            if (UpdateUiForStepNext(textCore.Step(RunMode.RunTo)))
                                editorWidgetBar.ActivateWidget(EditorWidgetBar.Widget.Watch, true);
                            else
                            {
                                int errorcount = ErrorCount();
                                if (errorcount > 0)
                                    editorWidgetBar.ActivateWidget(EditorWidgetBar.Widget.Errors, true);
                                else
                                    editorWidgetBar.ActivateWidget(EditorWidgetBar.Widget.Watch, true);
                            }

                            prevRunMode = RunModes.RunDebug;
                        }
                    }

                }
            }

            if (-1 == clickedLineIndex)
                textCore.SetMouseUpPosition(cursor.X, cursor.Y, e);

            //For cross highlighting
            UpdateScriptDisplay(activeScript);

            clickedLineIndex = -1;
            UpdateCaretPosition();
        }

        private void OnScrollViewerRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (textCore.HasSelection != false)
                return; // We don't want to destroy selection.

            // Compute the coordinates from screen to character.
            System.Windows.Point screenPoint = GetRelativeCanvasPosition(e);
            IScriptObject activeScript = Solution.Current.ActiveScript;
            CharPosition mousePoint = activeScript.CreateCharPosition();
            mousePoint.SetScreenPosition(screenPoint);

            // Finally set the actual cursor position and refresh the display.
            textCore.SetCursorPosition(mousePoint.CharacterX, mousePoint.CharacterY);
            UpdateCaretPosition();
            clickedLineIndex = -1;
        }

        private void OnTabItemsMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            //Change focus of item on right click
            ((TabItem)sender).IsSelected = true;
        }

        private void HandleMultipleClicks(MouseButtonEventArgs e)
        {
            if (null == textCore)
                return;

            IScriptObject activeScript = Solution.Current.ActiveScript;
            CharPosition mousePoint = activeScript.CreateCharPosition();
            mousePoint.SetScreenPosition(GetRelativeCanvasPosition(e));
            System.Drawing.Point cursor = mousePoint.GetCharacterPosition();

            switch (e.ClickCount)
            {
                case 2: // Double clicking event.
                    textCore.SelectFragment(mousePoint.CharacterX, mousePoint.CharacterY);
                    break;

                case 3: // Triple clicking event.
                    textCore.SelectLines(mousePoint.CharacterY, 0);
                    break;
            }
        }

        #endregion
    }
}
