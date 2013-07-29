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
using System.Windows.Threading;
using DesignScriptStudio.Graph.Core;

namespace DesignScriptStudio.Graph.Ui
{
    /// <summary>
    /// Interaction logic for CustomTextBox.xaml
    /// </summary>
    public partial class CustomTextBox : UserControl
    {
        GraphCanvas graphCanvas = null;
        Point cursorScreenPosition = new Point();
        bool selectAll;
        double emptyWidth;
        internal bool isInNormalTextStage;

        public CustomTextBox(GraphCanvas newGraphCanvas, bool acceptReturn, SolidColorBrush backgroundColor, String font,
            FontWeight weight, double fontSize, double maxWidth, double minWidth)
        {
            InitializeComponent();
            this.InternalTextBox.Text = "";
            emptyWidth = this.InternalTextBox.ActualWidth;

            this.Focusable = true;
            this.IsEnabled = true;
            TextBoxGrid.Focusable = true;
            TextBoxGrid.IsEnabled = true;
            InternalTextBox.Focusable = true;
            InternalTextBox.IsEnabled = true;
            InternalTextBox.AcceptsTab = true;

            InternalTextBox.Background = backgroundColor;
            InternalTextBox.FontFamily = new FontFamily(font);
            InternalTextBox.FontWeight = weight;
            InternalTextBox.FontSize = fontSize;
            InternalTextBox.TextAlignment = TextAlignment.Left;
            if (acceptReturn != true)
            {
                this.InternalTextBox.MaxWidth = 175; //textbox size for 25chars
                InternalTextBox.AcceptsTab = false;
            }
            else
            {
                this.InternalTextBox.MaxWidth = 510;
                this.InternalTextBox.TextWrapping = TextWrapping.Wrap;
                this.SelectAll = false;
            }
            if (minWidth > 172)
                minWidth = 172;

            InternalTextBox.AcceptsReturn = acceptReturn;
            InternalTextBox.MinWidth = minWidth;
            InternalTextBox.Loaded += new RoutedEventHandler(OnInternalTextBoxLoaded);
            InternalTextBox.TextChanged += new TextChangedEventHandler(OnInternalTextBoxTextChanged);
            this.graphCanvas = newGraphCanvas;
            isInNormalTextStage = false;
        }

        void OnInternalTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (InternalTextBox.IsReadOnly)
                return;

            if (this.MultiLineSupport == true)
            {
                if ((isInNormalTextStage && InternalTextBox.Text.Equals("")) ||
                    this.Text == Configurations.CodeBlockInitialMessage)
                {
                    ChangeWaterMarkStage(Configurations.CodeBlockInitialMessage, false);
                    InternalTextBox.CaretIndex = 0;
                }
                else if (!isInNormalTextStage && InternalTextBox.Text.Length > 1 &&
                    (InternalTextBox.Text.Substring(1) == Configurations.CodeBlockInitialMessage || InternalTextBox.Text.Substring(1) == Configurations.CodeBlockInitialMessage.Substring(1)))
                {
                    ChangeWaterMarkStage(InternalTextBox.Text.Substring(0, 1), true);
                    InternalTextBox.CaretIndex = InternalTextBox.Text.Length;
                }
            }

            if (InternalTextBox.Text.Equals(Configurations.CodeBlockInitialMessage) == false && isInNormalTextStage == false)
                isInNormalTextStage = true;

            if (this.InternalTextBox.AcceptsReturn != true && this.InternalTextBox.Text.Length >= 25)
            {
                string str = this.InternalTextBox.Text.Substring(0, 25);
                FormattedText visibleText = new FormattedText(str,
                    Configurations.culture,
                    FlowDirection.LeftToRight,
                    Configurations.TypeFace,
                    Configurations.TextSize,
                    Configurations.TextNormalColor);
                this.InternalTextBox.MaxWidth = visibleText.WidthIncludingTrailingWhitespace + 10;
            }
        }

        #region Internal Class Properties

        internal string Text
        {
            get { return InternalTextBox.Text; }
            set { this.InternalTextBox.Text = value; }
        }

        internal TextAlignment Alignment
        {
            get { return this.InternalTextBox.TextAlignment; }
            set { this.InternalTextBox.TextAlignment = value; }
        }

        internal bool SelectAll
        {
            get { return selectAll; }
            set { selectAll = value; }
        }

        internal bool MultiLineSupport
        {
            get { return InternalTextBox.AcceptsReturn; }
        }

        #endregion

        #region Internal Class Methods

        internal void UpdateCursor()
        {
            InternalTextBox.CaretIndex = InternalTextBox.Text.Length;
        }

        internal void SetCursorScreenPos(Point cursorScreenPosition)
        {
            this.cursorScreenPosition = cursorScreenPosition;
        }

        #endregion

        private void OnInternalTextBoxLoaded(object sender, RoutedEventArgs e)
        {
            cursorScreenPosition.X -= (double)this.GetValue(Canvas.LeftProperty);
            cursorScreenPosition.Y -= (double)this.GetValue(Canvas.TopProperty);

            int index = InternalTextBox.GetCharacterIndexFromPoint(cursorScreenPosition, true);
            if (index >= 0)
            {
                Rect left = InternalTextBox.GetRectFromCharacterIndex(index, false);
                Rect right = InternalTextBox.GetRectFromCharacterIndex(index, true);
                double x = cursorScreenPosition.X;
                bool closerToFront = (x - left.Left < (right.Right - x));
                if (false == closerToFront)
                    index = index + 1;
            }

            if (index >= 0)
                InternalTextBox.CaretIndex = index;
            InternalTextBox.Focusable = true;
            InternalTextBox.Visibility = Visibility.Visible;
            if (isInNormalTextStage == false)
                InternalTextBox.Foreground = new SolidColorBrush(Colors.Gray);
            if (this.selectAll == true)
                this.InternalTextBox.SelectAll();
            if (this.Text == Configurations.ReplicationInitialString)
                InternalTextBox.CaretIndex = 1;
            InternalTextBox.Focus();
        }

        // prevent unwanted scroll on canvas once the preview is expanded
        private void OnTextBoxGridMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
        }

        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!InternalTextBox.IsReadOnly && !isInNormalTextStage && this.MultiLineSupport == true)
            {
                ChangeWaterMarkStage("", true);
            }
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (this.MultiLineSupport == true)
            {
                if (!isInNormalTextStage && (e.Key == Key.Right || e.Key == Key.Left || e.Key == Key.Delete || e.Key == Key.LeftCtrl ||
                e.Key == Key.LeftAlt || e.Key == Key.LeftShift || e.Key == Key.RightAlt || e.Key == Key.RightCtrl ||
                e.Key == Key.RightShift || e.Key == Key.End || e.Key == Key.Enter || e.Key == Key.PageDown || e.Key == Key.PageUp))
                {
                    ChangeWaterMarkStage("", true);
                }
            }
        }

        private void ChangeWaterMarkStage(string text, bool changedStatus)
        {
            Console.WriteLine("ChangeWaterMarkStage:    " + text);
            Console.WriteLine("ChangeWaterMarkStage:    " + changedStatus);
            InternalTextBox.Text = text;
            isInNormalTextStage = changedStatus;

            if (!isInNormalTextStage)
                InternalTextBox.Foreground = new SolidColorBrush(Colors.Gray);
            else
                InternalTextBox.Foreground = new SolidColorBrush(Colors.Black);
        }
    }
}