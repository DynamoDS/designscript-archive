using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using DesignScript.Editor.Core;

namespace DesignScript.Editor
{
    class AuxiliaryLayer : VisualLayer
    {
        private System.Windows.Visibility caretVisibility;
        private System.Windows.Rect caretRect;
        private System.Windows.Threading.DispatcherTimer caretTimer = null;

        #region Public Class Operational Methods

        public AuxiliaryLayer(TextEditorCanvas textEditorCanvas)
            : base(textEditorCanvas)
        {
            caretRect = new System.Windows.Rect();
            caretRect.Width = 1.3;
            caretRect.Height = Configurations.FontDisplayHeight;
            caretVisibility = System.Windows.Visibility.Visible;
        }

        public void PauseCaretTimer(bool pauseTimer)
        {
            if (false != pauseTimer)
            {
                if ((null != caretTimer) && caretTimer.IsEnabled)
                    caretTimer.Stop();
            }
            else
            {
                if (null == caretTimer)
                {
                    caretTimer = new System.Windows.Threading.DispatcherTimer();
                    caretTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
                    caretTimer.Tick += new EventHandler(OnCaretTimerTick);
                }

                if (caretTimer.IsEnabled == false)
                    caretTimer.Start();
            }

            caretVisibility = System.Windows.Visibility.Visible;
        }

        public void SetCursorVisibility(System.Windows.Visibility visibility)
        {
            // Make sure we don't get "Collapsed", only "Visible" or "Hidden".
            caretVisibility = visibility;
            if (caretVisibility != System.Windows.Visibility.Visible)
                caretVisibility = System.Windows.Visibility.Hidden;
        }

        public void SetCursorScreenPos(System.Windows.Point cursorPosition)
        {
            caretRect.X = cursorPosition.X;
            caretRect.Y = cursorPosition.Y;
        }

        #endregion

        protected override void UpdateLayerForScriptCore(IScriptObject hostScript)
        {
        }

        protected override DrawingVisual RenderCore(RenderParameters renderParams)
        {
            caretRect.Height = Configurations.FontDisplayHeight;
            if (null == drawingVisual)
            {
                drawingVisual = new DrawingVisual();
                PauseCaretTimer(false); // Get timer going!
            }

            // There's no script yet.
            if (null == this.currentScript)
                return drawingVisual;

            DrawingContext context = drawingVisual.RenderOpen();
            CodeFragment[] crossHighlight = textEditorCanvas.TextEditorCore.GetCrossHighlightArray();

            if (crossHighlight != null)
            {
                if (crossHighlight.Length != 0)
                {
                    List<Rect> rectList = new List<Rect>();
                    foreach (CodeFragment codeFragment in crossHighlight)
                    {
                        CharPosition start = currentScript.CreateCharPosition();
                        start.SetCharacterPosition(codeFragment.ColStart, codeFragment.Line);
                        CharPosition end = currentScript.CreateCharPosition();
                        end.SetCharacterPosition(codeFragment.ColEnd + 1, codeFragment.Line);
                        List<Rect> tempList = CalculateRectangles(start, end, renderParams.firstVisibleLine);
                        if (tempList != null)
                            rectList.AddRange(tempList);
                    }
                    //SolidColorBrush crossHighlightBrush = new SolidColorBrush(Color.FromRgb(226, 230, 214));
                    RenderRectangles(context, rectList, UIColors.CrossHighlightColor);
                }
            }

            // We don't care about keyboard focus when in playback mode.
            if (false == TextEditorControl.Instance.IsInPlaybackMode)
            {
                // If the focus is not within the canvas, or on any extension popup, 
                // then there's no need to show the caret (focus is probably other app).
                if (false == TextEditorControl.Instance.ShouldDisplayCaret())
                    caretVisibility = System.Windows.Visibility.Hidden;
            }

            // Cursor rendering pass...
            if (caretVisibility == System.Windows.Visibility.Visible)
            {
                System.Windows.Rect displayRect = caretRect;
                double firstVisibleColumn = textEditorCanvas.FirstVisibleColumn * Configurations.FormatFontWidth;
                displayRect.Offset(-firstVisibleColumn, -renderParams.firstVisibleLine * Configurations.FontDisplayHeight);
                context.DrawRectangle(Brushes.Black, null, displayRect);
            }

            context.Close();
            return drawingVisual;
        }

        #region Private Class Helper Methods

        void OnCaretTimerTick(object sender, EventArgs e)
        {
            if (caretVisibility == System.Windows.Visibility.Visible)
                caretVisibility = System.Windows.Visibility.Hidden;
            else
                caretVisibility = System.Windows.Visibility.Visible;

            if (null != drawingVisual)
                this.Render();
        }

        #endregion
    }
}
