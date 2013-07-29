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
using System.Windows.Media.TextFormatting;
using System.Globalization;

namespace DesignScript.Editor
{
    using DesignScript.Parser;
    using DesignScript.Editor.Core;
    using ProtoCore.CodeModel;

    public class EditorVisualHost : FrameworkElement
    {
        // Create a collection of child visual objects.
        private VisualCollection children = null;
        private ITextBuffer textBuffer = null;

        // Create Layers
        private SourceDisplayLayer sourceDisplay = null;
        private LineHeadingLayer lineHeading = null;
        private HighlightLayer highlight = null;
        private AuxiliaryLayer auxiliary = null;

        // Session-wide data members.
        private List<CodeRange> breakpoints = null;

        public EditorVisualHost(TextEditorCanvas textEditorCanvas)
        {
            Configurations.InitializeFontSizes();

            children = new VisualCollection(this);
            highlight = new HighlightLayer(textEditorCanvas);
            auxiliary = new AuxiliaryLayer(textEditorCanvas);
            lineHeading = new LineHeadingLayer(textEditorCanvas);
            sourceDisplay = new SourceDisplayLayer(textEditorCanvas);
        }

        internal void SetExecutionCursor(ProtoCore.CodeModel.CodeRange cursor)
        {
            highlight.SetExecutionCursor(cursor);
            lineHeading.SetExecutionCursor(cursor);

            highlight.Render();
            lineHeading.Render();
        }

        internal void ClearExecutionCursor()
        {
            highlight.ResetHighlightLayer();
            highlight.ResetExecutionCursor();
            lineHeading.ResetExecutionCursor();

            highlight.Render();
            lineHeading.Render();
        }

        internal void UpdateVisualForScript(IScriptObject hostScript)
        {
            if (null == hostScript)
                return;

            // If there's no switching of active script, 
            // then simply do a screen refresh and get out of here!
            if (textBuffer == hostScript.GetTextBuffer())
            {
                highlight.Render();
                lineHeading.Render();
                sourceDisplay.Render();
                auxiliary.Render();
                return;
            }

            children.Clear(); // Clears all children before populating it again.

            textBuffer = hostScript.GetTextBuffer();
            highlight.UpdateLayerForScript(hostScript);
            lineHeading.UpdateLayerForScript(hostScript);
            sourceDisplay.UpdateLayerForScript(hostScript);
            auxiliary.UpdateLayerForScript(hostScript);

            highlight.ResetHighlightLayer();
            highlight.ResetExecutionCursor();

            children.Add(auxiliary.Render());
            children.Add(highlight.Render());
            children.Add(lineHeading.Render());
            children.Add(sourceDisplay.Render());
        }

        internal void UpdateVisualOnLayout()
        {
            lineHeading.Render();
            sourceDisplay.Render();
            highlight.Render();
            auxiliary.Render();
        }

        internal void SetSelectionRange(System.Drawing.Point start, System.Drawing.Point end)
        {
            highlight.SetSelectionRange(start, end);
            highlight.Render();
        }

        internal void BreakpointsUpdated()
        {
            if (null == breakpoints)
                breakpoints = new List<CodeRange>();

            breakpoints.Clear();
            Solution.Current.GetBreakpoints(breakpoints);
            highlight.SetBreakpointsReference(breakpoints);
            lineHeading.SetBreakpointsReference(breakpoints);

            highlight.Render();
            lineHeading.Render();
        }

        internal void PauseCaretTimer(bool pauseTimer)
        {
            auxiliary.PauseCaretTimer(pauseTimer);
        }

        internal void SetCursorScreenPos(System.Windows.Point cursorPosition)
        {
            if (null != this.auxiliary)
            {
                this.auxiliary.SetCursorScreenPos(cursorPosition);
                this.auxiliary.Render();
            }
        }

        #region Protected Class Overridable Methods

        // Provide a required override for the VisualChildrenCount property.
        protected override int VisualChildrenCount
        {
            get { return children.Count; }
        }

        // Provide a required override for the GetVisualChild method.
        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= children.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return children[index];
        }

        #endregion
    }
}
