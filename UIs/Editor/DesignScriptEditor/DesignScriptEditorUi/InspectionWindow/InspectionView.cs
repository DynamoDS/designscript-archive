using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace DesignScript.Editor
{
    public class InspectionView : TreeView
    {
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new InspectionViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is InspectionViewItem;
        }

        #region Public Properties

        /// <summary> GridViewColumn List that shows the headings in the Watch Window</summary>
        public GridViewColumnCollection Columns
        {
            
            get
            {
                if (_columns == null)
                {
                    _columns = new GridViewColumnCollection();
                }
                return _columns;
            }
        }

        private GridViewColumnCollection _columns;

        #endregion
    }

    public class InspectionViewItem : TreeViewItem
    {
        /// <summary>
        /// Item's hierarchy in the tree
        /// </summary>
        public int Level
        {
            get
            {
                if (_level == -1)
                {
                    InspectionViewItem parent = ItemsControl.ItemsControlFromItemContainer(this) as InspectionViewItem;
                    _level = (parent != null) ? parent.Level + 1 : 0;
                }
                return _level;
            }
        }


        protected override DependencyObject GetContainerForItemOverride()
        {
            return new InspectionViewItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is InspectionViewItem;
        }

        private int _level = -1;
    }
}
