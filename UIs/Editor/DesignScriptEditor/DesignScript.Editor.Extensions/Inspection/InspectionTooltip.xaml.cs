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
using DesignScript.Editor.Core;

namespace DesignScript.Editor.Extensions
{
    /// <summary>
    /// Interaction logic for InspectionToolTip.xaml
    /// </summary>
    public partial class InspectionToolTip : UserControl
    {
        InspectionDataCollection inspectionData = null;

        #region Public Operational Class Methods
        /// <summary>
        /// Constructor for the Inspection ToolTip functionality
        /// </summary>
        public InspectionToolTip()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Method to activate a variable for inspection tooltip. This is done by adding 
        /// the variable into the inspection list. Only one variable can be shown in tooltips.
        /// </summary>
        /// <param name="name"> Name of variable to be added to inspection </param>
        internal bool ActivateTooltip(string name)
        {
            Logger.LogInfo("ActivateTooltip", name);
            if (string.IsNullOrEmpty(name))
                return false;

            if (null == inspectionData)
            {
                inspectionData = new InspectionDataCollection();
                inspectionToolTip.ItemsSource = inspectionData.Data;
            }

            if (!inspectionData.AddInspectionData(name, true))
                return false;

            this.MaxHeight = SystemParameters.FullPrimaryScreenHeight / 2.0;
            inspectionToolTip.Items.Refresh();
            colName.Width = inspectionData.MaxNameWidth();
            colValue.Width = inspectionData.MaxValueWidth();
            return true;
        }

        /// <summary>
        /// Remove the tooltip from activation
        /// </summary>
        internal void DeactivateTooltip()
        {
            Logger.LogInfo("DeactivateTooltip", "DeactivateTooltip");
            if (null != inspectionData)
                inspectionData.RemoveAllInspectionData();
        }

        /// <summary>
        /// Method to return the height of the InspectionToolTip
        /// This is necessary in case any amendments to the height shown on-screen need to be 
        /// made in the future.
        /// </summary>
        /// <returns> Double value of inspection control height </returns>
        internal double GetHeight()
        {
            return inspectionToolTip.Height;

        }

        /// <summary>
        /// Returns width of the inspection control by adding the widths of each column
        /// </summary>
        /// <returns> Double value of inspection control width </returns>
        internal double GetWidth()
        {
            // colValue is the column that holds the Inspection Value
            // colName is the column that holds the Inspection Name
            return colValue.Width + colName.Width;
        }
        #endregion

        #region Private Class Event Handlers
        /// <summary>
        /// 'Expander' is part of the toggle button defined in the XAML that shows the [+] or [-]
        /// sign when there are items under the current watch variable that can be inspected. 
        /// To maximise efficiency, the expansion of items must happen only when the expander is clicked
        /// when showing [+]. This generated the child items from the Virtual Machine.
        /// </summary>
        /// <param name="sender"> The sender is the Toggle Button</param>
        /// <param name="e"></param>
        private void OnExpanderClicked(object sender, RoutedEventArgs e)
        {
            if (null == inspectionData) // Not possible...
                return; // ... but, just to be sure.

            Logger.LogInfo("Insepction-OnExpanderClicked", "OnExpanderClicked");
            ToggleButton expanderToggle = (ToggleButton)sender;

            // The parent we are trying to locate here is the InspectionViewItem. The 
            // expander is buried beneath several layers of stackpanels and grids under the Item
            // and therefore we have to make numerous calls to find the InspectionViewItem.
            InspectionViewItem parent = (InspectionViewItem)VisualTreeHelper.GetParent
                (VisualTreeHelper.GetParent
                (VisualTreeHelper.GetParent
                (VisualTreeHelper.GetParent
                (VisualTreeHelper.GetParent
                (VisualTreeHelper.GetParent(expanderToggle))))));

            // expanderToggle.IsChecked == true means the [+] --> [-]
            if (expanderToggle.IsChecked == true)
            {
                parent.IsExpanded = false;
                InspectionData selectedData = (InspectionData)parent.DataContext;
                inspectionData.ExpandInspection(selectedData);

                // If there is a large amount of data, the user may have to wait. 
                // This is indicated by updating the mouse cursor.
                Mouse.SetCursor(Cursors.Wait);
                parent.IsSelected = true;
                parent.IsExpanded = true;
                selectedData.IsExpanded = true;

                CheckExpansion(inspectionData.Data);
                inspectionToolTip.Items.Refresh();
                DoExpansion(inspectionData.Data);
                Mouse.SetCursor(Cursors.Arrow);

                // In the tooltip, the width of items may vary and the user cannot adjust the width
                // of the datagrid. Therefore there must be a way top retrieve the maximum width of each column
                // and adjust the setting so that the Inspection Values are not blocked for the user
                colName.Width = inspectionData.MaxNameWidth();
                colValue.Width = inspectionData.MaxValueWidth();
                this.Width = colName.Width + colValue.Width;
            }
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            Logger.LogInfo("Inspection-OnLostKeyboardFocus", "OnLostKeyboardFocus");

            if (this.IsKeyboardFocusWithin == false)
                (this.Parent as ExtensionPopup).DismissExtensionPopup(false);

            base.OnLostKeyboardFocus(e);
        }

        #endregion

        #region Private Class Helper Methods
        /// <summary>
        /// An 'Items.Refresh' call erases all the previous expansion data. This recursive
        /// method complements 'CheckExpansion' method where it does the expansion of items
        /// that were expanded before the refresh method closed them.
        /// </summary>
        /// <param name="dataVal">
        /// The Observable Collection that holds all the data for the level below. 
        /// Each InspectionData.Derivations is also an Observable Collection.
        /// </param>
        /// <param name="parent">
        /// If parent is null, it means there is no parent and it is the top level data.
        /// Otherwise, send the parent item of the dataVal parameter
        /// </param>
        private void DoExpansion(ICollection<InspectionData> dataVal, TreeViewItem parent = null)
        {
            if (dataVal == null)
                return;

            foreach (InspectionData item in dataVal)
            {
                InspectionViewItem TLVitem;

                if (parent == null)
                    TLVitem = (InspectionViewItem)(inspectionToolTip.ItemContainerGenerator.ContainerFromItem(item));
                else
                {
                    parent.IsExpanded = true;
                    parent.UpdateLayout();
                    TLVitem = (InspectionViewItem)(parent.ItemContainerGenerator.ContainerFromItem(item));
                }

                if (item.IsExpanded)
                {
                    if (null != TLVitem)
                    {
                        TLVitem.IsExpanded = true;
                    }
                }
                if (item.HasItems && item.IsExpanded)
                {
                    DoExpansion(item.Derivations, TLVitem);
                }
            }
        }

        /// <summary>
        /// This method checks all the items in the Inspection ToolTip and takes note 
        /// of what is expanded by toggling the 'isExpanded' flag in the InspectionData class.
        /// This information is later used by 'DoExpansion' to do the actual expansion
        /// </summary>
        /// <param name="dataVal">
        /// The Observable Collection that holds all the data for the level below. 
        /// Each InspectionData.Derivations is also an Observable Collection.
        /// </param>
        /// <param name="parent">
        /// If parent is null, it means there is no parent and it is the top level data.
        /// Otherwise, send the parent item of the dataVal parameter
        /// </param>
        private void CheckExpansion(ICollection<InspectionData> dataVal, TreeViewItem parent = null)
        {
            if (dataVal == null)
                return;

            foreach (InspectionData item in dataVal)
            {
                InspectionViewItem TLVitem;
                if (parent == null)
                    TLVitem = (InspectionViewItem)(inspectionToolTip.ItemContainerGenerator.ContainerFromItem(item));
                else
                    TLVitem = (InspectionViewItem)(parent.ItemContainerGenerator.ContainerFromItem(item));


                if (null != TLVitem)
                {
                    if (TLVitem.IsExpanded)
                        item.IsExpanded = true;
                    else
                        item.IsExpanded = false;
                }

                if (item.HasItems && item.IsExpanded)
                {
                    CheckExpansion(item.Derivations, TLVitem);
                }
            }
        }
        #endregion

    }
}
