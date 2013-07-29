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
using System.Collections.ObjectModel;
using DesignScript.Editor.Core;

namespace DesignScript.Editor.Automation
{
    class RuntimeProperty
    {
        public string PropertyName { get; set; }
        public string PropertyValue { get; set; }
    }

    class RuntimeProperties : ObservableCollection<RuntimeProperty>
    {
        private enum Property
        {
            LineCount,
            BreakpointCount,
            SelectionText,
            ActiveScriptName,
            CursorPosition,
            SelectionStart,
            SelectionEnd,
            ExecutionCursorStart,
            ExecutionCursorEnd,
            TotalProperties // Always last.
        }

        private const string LineCount = "LineCount";
        private const string BreakpointCount = "BreakpointCount";
        private const string SelectionText = "SelectionText";
        private const string ActiveScriptName = "ActiveScriptName";
        private const string CursorPosition = "CursorPosition";
        private const string SelectionStart = "SelectionStart";
        private const string SelectionEnd = "SelectionEnd";
        private const string ExecutionCursorStart = "ExecutionCursorStart";
        private const string ExecutionCursorEnd = "ExecutionCursorEnd";

        internal RuntimeProperties()
        {
            for (Property index = 0; index < Property.TotalProperties; ++index)
            {
                this.Add(new RuntimeProperty()
                {
                    PropertyName = index.ToString(),
                    PropertyValue = "Uninitialized"
                });
            }
        }

        internal void UpdatePropertyValue(string name, string value)
        {
            Property propertyIndex = 0;

            switch (name)
            {
                case LineCount: propertyIndex = Property.LineCount; break;
                case BreakpointCount: propertyIndex = Property.BreakpointCount; break;
                case SelectionText: propertyIndex = Property.SelectionText; break;
                case ActiveScriptName: propertyIndex = Property.ActiveScriptName; break;
                case CursorPosition: propertyIndex = Property.CursorPosition; break;
                case SelectionStart: propertyIndex = Property.SelectionStart; break;
                case SelectionEnd: propertyIndex = Property.SelectionEnd; break;
                case ExecutionCursorStart: propertyIndex = Property.ExecutionCursorStart; break;
                case ExecutionCursorEnd: propertyIndex = Property.ExecutionCursorEnd; break;
                default: return;
            }

            RuntimeProperty property = this[(int)propertyIndex];
            property.PropertyValue = value;
        }
    }

    public partial class PlaybackSnapshot : Window
    {
        IAssertableProperties assertable = null;
        RuntimeProperties properties = null;

        public PlaybackSnapshot()
        {
            InitializeComponent();
            this.Closing += new System.ComponentModel.CancelEventHandler(OnPlaybackSnapshotClosing);
            StepNextCommand.Click += new RoutedEventHandler(OnStepNextCommandClicked);
        }

        internal void Initialize(IAssertableProperties assertable)
        {
            this.assertable = assertable;
            if (null == properties)
            {
                properties = new RuntimeProperties();
                PropertyListView.ItemsSource = properties;
            }
        }

        internal void RefreshProperties()
        {
            if (null == properties || (null == assertable))
                return;

            string selectionText = assertable.SelectionText;
            selectionText = selectionText.Replace('\t', (char)0x00bb);
            selectionText = selectionText.Replace('\n', (char)0x25be);

            properties.UpdatePropertyValue("LineCount", assertable.LineCount.ToString());
            properties.UpdatePropertyValue("BreakpointCount", assertable.BreakpointCount.ToString());
            properties.UpdatePropertyValue("SelectionText", selectionText);
            properties.UpdatePropertyValue("ActiveScriptName", assertable.ActiveScriptName);
            properties.UpdatePropertyValue("CursorPosition", assertable.CursorPosition.ToString());
            properties.UpdatePropertyValue("SelectionStart", assertable.SelectionStart.ToString());
            properties.UpdatePropertyValue("SelectionEnd", assertable.SelectionEnd.ToString());
            properties.UpdatePropertyValue("ExecutionCursorStart", assertable.ExecutionCursorStart.ToString());
            properties.UpdatePropertyValue("ExecutionCursorEnd", assertable.ExecutionCursorEnd.ToString());
            PropertyListView.Items.Refresh();
        }

        private void OnPlaybackSnapshotClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Collapsed;
            TextEditorControl.Instance.PauseActionPlayback(false);
        }

        private void OnStepNextCommandClicked(object sender, RoutedEventArgs e)
        {
            TextEditorControl.Instance.PlaybackNextCommand();
            this.RefreshProperties();
        }
    }
}
