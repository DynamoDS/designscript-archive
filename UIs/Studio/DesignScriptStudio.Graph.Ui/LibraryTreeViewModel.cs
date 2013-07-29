using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using DesignScriptStudio.Graph.Core;

namespace DesignScriptStudio.Graph.Ui
{
    class LibraryTreeViewModel
    {
        ObservableCollection<LibraryItemViewModel> rootItems;
        LibraryItemViewModel libary;
        LibraryView libraryView;

        public LibraryTreeViewModel(LibraryItem libary, LibraryView libraryView)
        {
            this.libraryView = libraryView;

            this.libary = new LibraryItemViewModel(libary, libraryView);
            rootItems = new ObservableCollection<LibraryItemViewModel>();

            foreach (LibraryItem rootItem in libary.Children)
            {
                rootItems.Add(new LibraryItemViewModel(rootItem, libraryView));
            }

            if (rootItems[0].DisplayText == Configurations.NoResultMessage)
                rootItems[0].Visibility = Visibility.Collapsed;
        }

        public ObservableCollection<LibraryItemViewModel> RootItems
        {
            get { return this.rootItems; }
        }
    }

    class LibraryItemViewModel : INotifyPropertyChanged
    {
        #region Data

        ObservableCollection<LibraryItemViewModel> children;
        LibraryItemViewModel parent;
        LibraryItem libraryItem;
        LibraryView libraryView;

        bool isExpanded;
        bool isSelected;
        Visibility visibility;
        bool isChildVisible;
        //List<LibraryView.DecoratedString> header;
        string prePiece, highlightPiece, postPiece;

        #endregion

        #region Constructors

        public LibraryItemViewModel(LibraryItem libraryItem, LibraryView libraryView)
            : this(libraryItem, null, libraryView)
        {
        }

        private LibraryItemViewModel(LibraryItem libraryItem, LibraryItemViewModel parent, LibraryView libraryView)
        {
            this.libraryView = libraryView;

            this.libraryItem = libraryItem;
            this.parent = parent;
            this.children = new ObservableCollection<LibraryItemViewModel>();

            this.isExpanded = false;
            this.isSelected = false;
            this.visibility = Visibility.Visible;

            //this.header = new List<LibraryView.DecoratedString>();
            //LibraryView.DecoratedString item = new LibraryView.DecoratedString();
            //item.isHilighted = false;
            //item.text = this.libraryItem.DisplayText;
            //this.header.Add(item);

            this.prePiece = this.libraryItem.DisplayText;
            this.highlightPiece = string.Empty;
            this.postPiece = string.Empty;

            if (this.libraryItem.Children != null && this.libraryItem.Children.Count > 0)
            {
                foreach (LibraryItem childItem in libraryItem.Children)
                {
                    this.children.Add(new LibraryItemViewModel(childItem, this, libraryView));
                }
            }
        }

        #endregion

        #region LibraryItem Properties

        public ObservableCollection<LibraryItemViewModel> Children
        {
            get { return this.children; }
        }

        public int Level
        {
            get { return this.libraryItem.Level; }
        }

        public string DisplayText
        {
            get { return this.libraryItem.DisplayText; }
        }

        public bool IsOverloaded
        {
            get
            {
                if (this.libraryItem.Children != null && this.libraryItem.Children.Count > 0)
                    return this.libraryItem.Children[0].IsOverloaded;
                else
                    return false;
            }
        }

        public LibraryItem LibraryItem
        {
            get { return this.libraryItem; }
        }

        #endregion

        #region Presentation Members

        public bool IsExpanded
        {
            get { return this.isExpanded; }
            set
            {
                if (value != this.isExpanded)
                {
                    this.isExpanded = value;
                    this.UpdateIsChildVisible();
                    this.OnPropertyChanged("IsExpanded");
                }
            }
        }

        public bool IsSelected
        {
            get { return this.isSelected; }
            set
            {
                if (value != this.isSelected)
                {
                    this.isSelected = value;
                    this.OnPropertyChanged("IsSelected");
                }
            }
        }

        public Visibility Visibility
        {
            get { return this.visibility; }
            set
            {
                if (value != this.visibility)
                {
                    this.visibility = value;
                    if (this.parent != null)
                        this.parent.UpdateIsChildVisible();
                    this.OnPropertyChanged("Visibility");
                }
            }
        }

        public bool IsChildVisible
        {
            get { return this.isChildVisible; }
            set
            {
                if (value != this.isChildVisible)
                {
                    this.isChildVisible = value;
                    this.OnPropertyChanged("IsChildVisible");
                }
            }
        }

        public string PrePiece
        {
            get { return this.prePiece; }
            set
            {
                if (value != this.prePiece)
                {
                    this.prePiece = value;
                    this.OnPropertyChanged("PrePiece");
                }
            }
        }

        public string HighlightPiece
        {
            get { return this.highlightPiece; }
            set
            {
                if (value != this.highlightPiece)
                {
                    this.highlightPiece = value;
                    this.OnPropertyChanged("HighlightPiece");
                }
            }
        }

        public string PostPiece
        {
            get { return this.postPiece; }
            set
            {
                if (value != this.postPiece)
                {
                    this.postPiece = value;
                    this.OnPropertyChanged("PostPiece");
                }
            }
        }

        //public List<LibraryView.DecoratedString> Header
        //{
        //    get { return this.header; }
        //    set
        //    {
        //        if (value != this.header)
        //        {
        //            this.header = value;
        //            this.OnPropertyChanged("Header");
        //        }
        //    }
        //}

        public ContextMenu CM
        {
            get
            {
                if (this.children.Count < 1)
                    return this.libraryView.ItemMenu;
                else if (this.libraryItem.Level == 0 && this.libraryItem.IsExternal)
                    return this.libraryView.ExternalFolderMenu;
                else
                    return this.libraryView.FolderMenu;
            }
        }

        public LibraryItemViewModel Parent
        {
            get { return this.parent; }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        internal void UpdateIsChildVisible()
        {
            if (this.children.Count < 1 || !this.isExpanded)
            {
                this.IsChildVisible = false;
                return;
            }

            foreach (LibraryItemViewModel childItem in this.children)
            {
                if (childItem.Visibility != Visibility.Visible)
                {
                    this.IsChildVisible = false;
                    return;
                }
            }

            this.IsChildVisible = true;
        }
    }
}
