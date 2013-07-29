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
using System.Windows.Controls.Primitives;
using DesignScript.Editor.CodeGen;
using DesignScript.Editor.Core;

namespace DesignScript.Editor.Extensions
{
    /// <summary>
    /// Interaction logic for AutoCompleteList.xaml
    /// </summary>
    public partial class AutoCompleteList : UserControl
    {
        private string filterString = string.Empty;
        private List<AutoCompleteItem> autoCompleteItems;
        private TextEditorControl textEditorControl = null;

        #region Public Operational Class Methods

        /// <summary>
        /// Constructor for AutoCompleteList
        /// </summary>
        internal AutoCompleteList()
        {
            InitializeComponent();
            autoCompleteItems = new List<AutoCompleteItem>();
            lstAutoComplete.ItemsSource = autoCompleteItems;
            lstAutoComplete.ItemContainerGenerator.StatusChanged += new EventHandler(OnItemContainerGeneratorStatusChanged);
        }

        /// <summary>
        /// Method to set TextEditorControl within the AutoCompleteList. This is necessary
        /// because the AutoCompleteList acts as router of keyboard events back to the main window.
        /// </summary>
        /// <param name="control"></param>
        internal void SetTextEditorControl(TextEditorControl control)
        {
            if (textEditorControl != null)
                return;

            textEditorControl = control;
        }

        /// <summary>
        /// This method adds an AutoComplete item to the List of AutoComplete items. The AutoCompleteItem
        /// class is databound to the ListBox lstAutoComplete
        /// </summary>
        /// <param name="pair"> KeyValuePair of autocomplete items retrieved from the IDECodeGen</param>
        internal void AddItemsToList(List<KeyValuePair<AutoCompletionHelper.MemberType, string>> list)
        {
            foreach (KeyValuePair<AutoCompletionHelper.MemberType, string> pair in list)
                autoCompleteItems.Add(new AutoCompleteItem(pair));

            // Order the items and bind the list of AutoCompleteItems to list box.
            autoCompleteItems = autoCompleteItems.OrderBy(x => x.Item).ToList();
            lstAutoComplete.ItemsSource = autoCompleteItems;
            lstAutoComplete.Items.Refresh();

            if (lstAutoComplete.Items.Count > 0)
                lstAutoComplete.SelectedIndex = 0;
        }

        /// <summary>
        /// The first Item in the AutoCompleteList must be
        /// automatically focused when the List is opened. This enhances user experience without having to
        /// do a manual focus shift
        /// </summary>
        internal void DoFocusOnFirstItem()
        {
            if (autoCompleteItems.Count > 0)
            {
                // Index used is 0 as first item is the one to be focused on
                ListBoxItem listViewItem = (ListBoxItem)(lstAutoComplete.ItemContainerGenerator.ContainerFromIndex(0));

                if (listViewItem != null)
                {
                    listViewItem.Focus();
                }
            }
        }

        /// <summary>
        /// Method used to Clear the AutoComplete list and refresh the list of items
        /// </summary>
        internal void ClearList()
        {
            autoCompleteItems.Clear();
            lstAutoComplete.Items.Refresh();
            filterString = string.Empty;
        }

        #endregion

        #region Private Class Helper Methods

        /// <summary>
        /// This method filters the AutoCompleteList based on the filterString
        /// </summary>
        private void DoFilter()
        {
            int selectedIndex = 1;
            int index = 0;
            double minSearchIndex = double.MaxValue;

            if (string.IsNullOrEmpty(filterString) == false)
            {
                if (IsValidIdentifierName(filterString) == false)
                {
                    CommitSelection();
                    ResetAndHideAutoCompletePopup(true);
                    return;
                }

                List<AutoCompleteItem> searchResult = new List<AutoCompleteItem>();
                foreach (AutoCompleteItem item in autoCompleteItems)
                {
                    // Make the Search string uppercase and compare with the uppercase version
                    // of all list items
                    string result = item.Item.ToUpperInvariant();
                    if (result.IndexOf(filterString.ToUpperInvariant()) != -1)
                        searchResult.Add(item);
                }
                foreach (AutoCompleteItem item in searchResult)
                {
                    index++;
                    string result = item.Item.ToUpperInvariant();
                    int searchIndex = result.IndexOf(filterString.ToUpperInvariant());
                    if (searchIndex < minSearchIndex)
                    {
                        minSearchIndex = searchIndex;
                        selectedIndex = index;
                    }
                }

                if (searchResult.Count() != 0)
                {
                    int prevItems = lstAutoComplete.Items.Count;
                    lstAutoComplete.ItemsSource = searchResult;
                    lstAutoComplete.Items.Refresh();
                    lstAutoComplete.SelectedIndex = selectedIndex - 1;
                    lstAutoComplete.SelectedItem = lstAutoComplete.Items[selectedIndex - 1];
                    ListBoxItem listBoxItem = (ListBoxItem)lstAutoComplete.ItemContainerGenerator.ContainerFromItem(lstAutoComplete.SelectedItem);
                    listBoxItem.Focus();
                    lstAutoComplete.UpdateLayout();
                }
            }
            else
            {
                ResetAndHideAutoCompletePopup(true);
            }
        }

        private bool IsValidIdentifierName(string identifier)
        {
            if (string.IsNullOrEmpty(identifier))
                return false;

            foreach (char character in identifier)
            {
                if (char.IsLetterOrDigit(character))
                    continue;

                switch (character)
                {
                    case '@':
                    case '_':
                        continue;

                    default:
                        return false;
                }
            }

            return true;
        }

        #endregion

        #region Private Class Event Handlers

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            Logger.LogInfo("AutoComplete-OnLostKeyboardFocus", "OnLostKeyboardFocus");
            if (this.IsKeyboardFocusWithin == false)
                ResetAndHideAutoCompletePopup(false);
            base.OnLostKeyboardFocus(e);
        }

        /// <summary>
        /// The item to be focused on is retrieved using the ItemContainerGenerator. On the first injection of
        /// the AutoCompleteList, the ItemContainerGenerator is still not ready for focusing because the generator
        /// stats only after all the rendering. In this case, an event trigger was added to focus on the first Item in
        /// the autocomplete list to focus automatically on the first item after the ItemContainerGenerator has
        /// finished generating the containers. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnItemContainerGeneratorStatusChanged(object sender, EventArgs e)
        {
            if (autoCompleteItems.Count > 0)
            {
                ListBoxItem listViewItem = null;
                if (lstAutoComplete.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                    listViewItem = (ListBoxItem)(lstAutoComplete.ItemContainerGenerator.ContainerFromIndex(0));

                if (listViewItem != null)
                {
                    Typeface typeFace = new Typeface(listViewItem.FontFamily, listViewItem.FontStyle,
                        listViewItem.FontWeight, listViewItem.FontStretch);

                    double maxWidth = 0.0;
                    foreach (AutoCompleteItem item in autoCompleteItems)
                    {
                        if (string.IsNullOrEmpty(item.Item))
                            continue;

                        FormattedText formattedText = new FormattedText(
                            item.Item,
                            System.Globalization.CultureInfo.GetCultureInfo("en-us"),
                            FlowDirection.LeftToRight, typeFace, listViewItem.FontSize,
                            new SolidColorBrush(Colors.Black));

                        if (maxWidth < formattedText.Width)
                            maxWidth = formattedText.Width;
                    }

                    maxWidth += 36 + SystemParameters.VerticalScrollBarWidth;

                    // Make sure things are of reasonable sizes.
                    this.Width = Utility.ValidateAgainstScreenWidth(maxWidth, true, true);
                    this.MaxHeight = SystemParameters.FullPrimaryScreenHeight / 3.0;
                    listViewItem.Focus();
                }
            }
        }

        /// <summary>
        /// Behaviour of AutoCompleteList when anything is typed when AutoCompleteList is focused
        /// Enter, Space - Select current selected AutoCompleteItem and render on screen
        /// Backspace, Delete - if nothing has been searched for, remove AutoCompleteList, 
        /// otherwise remove relevant letter from filter string
        /// Navigation keys, shift, Capslock - nothing should happen
        /// Any other character - Render character on screen and filter AutoComplete list to show relevant items
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OnlstAutoCompleteKeyDown(object sender, KeyEventArgs e)
        {
            Logger.LogInfo("AutoComplete-OnlstAutoCompleteKeyDown", e.Key.ToString());

            if (e.Key == Key.Enter || e.Key == Key.Space || e.Key == Key.Tab)
            {
                CommitSelection();
                ResetAndHideAutoCompletePopup(true);
            }
            else if (e.Key == Key.Escape)
                ResetAndHideAutoCompletePopup(true);
            else if ((e.Key == Key.Back || e.Key == Key.Delete || e.Key == Key.Decimal) && (filterString == string.Empty))
                ResetAndHideAutoCompletePopup(true);
            else if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down
                || e.Key == Key.LeftShift || e.Key == Key.RightShift || e.Key == Key.CapsLock)
            {
                //nothing is coming!
            }
            else
            {
                Logger.LogInfo("OnlstAutoCompleteKeyDown-Else", e.Key.ToString());

                bool shift = Keyboard.IsKeyDown(Key.RightShift) || Keyboard.IsKeyDown(Key.LeftShift);
                bool capsLock = Console.CapsLock;

                if (e.Key == Key.Back)
                {
                    filterString = filterString.Remove(
                        filterString.Length - 1);
                }
                else
                {

                    filterString += TextEditorControl.GetKeyboardCharacter(e);
                    Logger.LogInfo("OnlstAutoCompleteKeyDown-Filter", filterString);

                }
                // Method to filter our AutoComplete list
                DoFilter();

                // Pass the character back to parent to display!
                (this.Parent as ExtensionPopup).RouteEventToCanvas(e);
            }
        }

        /// <summary>
        /// If the user selects the appropriate item with the mouse, set as selected item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnlstAutoCompleteMouseUp(object sender, MouseButtonEventArgs e)
        {
            Logger.LogInfo("AutoComplete-OnlstAutoCompleteMouseUp", "OnlstAutoCompleteMouseUp Called");

            CommitSelection();
            ResetAndHideAutoCompletePopup(false);
        }

        private void CommitSelection()
        {
            if (null != lstAutoComplete.SelectedItem)
            {
                string selectedItem = ((AutoCompleteItem)lstAutoComplete.SelectedItem).Item;
                if (string.IsNullOrEmpty(selectedItem) == false)
                    textEditorControl.ReplaceIdentifierAtCursor(selectedItem);

                Logger.LogInfo("AutoComplete-CommitSelection", selectedItem);
                Logger.LogInfo("AutoComplete-CommitSelection-code", Solution.Current.ActiveScript.GetTextBuffer().GetContent());
            }
        }

        private void ResetAndHideAutoCompletePopup(bool shiftFocusToCanvas)
        {
            Logger.LogInfo("AutoComplete-ResetAndHideAutoCompletePopup", "ResetAndHideAutoCompletePopup Called");

            this.ClearList();
            (this.Parent as ExtensionPopup).DismissExtensionPopup(shiftFocusToCanvas);
        }

        #endregion
    }

    /// <summary>
    /// Class to represent an AutoComplete Item databound to Listbox of AutoComplete items
    /// </summary>
    public class AutoCompleteItem
    {
        public string Item { get; private set; }
        public AutoCompletionHelper.MemberType Type { get; private set; }

        public AutoCompleteItem(KeyValuePair<AutoCompletionHelper.MemberType, string> item)
        {
            Item = item.Value;
            Type = item.Key;
        }
    }

    /// <summary>
    /// Value converter for AutoCompleteItem.Type which converts it into relevant Image to display on list
    /// </summary>
    [ValueConversion(typeof(AutoCompletionHelper.MemberType), typeof(string))]
    public class AutoCompleteItemTypeConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((((AutoCompletionHelper.MemberType)value) & AutoCompletionHelper.MemberType.Constructor) != 0)
                return Images.AutoCompleteConstructor;
            else if ((((AutoCompletionHelper.MemberType)value) & AutoCompletionHelper.MemberType.Fields) != 0)
                return Images.AutoCompleteField;
            else if ((((AutoCompletionHelper.MemberType)value) & AutoCompletionHelper.MemberType.Methods) != 0)
                return Images.AutoCompleteMethod;
            else
                return Images.AutoCompleteMissing;
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
