using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows;
using ProtoCore.Mirror;

namespace DesignScriptStudio.Graph.Core
{

    public class LibraryItem
    {
        public enum MemberType
        {
            None,
            Constructor,
            GlobalFunction,
            StaticMethod,
            InstanceMethod,
            StaticProperty,
            InstanceProperty,
        }

        private string qualifiedName = string.Empty;
        private StaticMirror dataMirror = null;

        #region Public Class Operational Methods

        public LibraryItem()
        {
            this.qualifiedName = String.Empty;

            this.dataMirror = null;
            Level = -1;
            Type = MemberType.None;
            ItemType = NodeType.None;
            DisplayText = String.Empty;
            Assembly = String.Empty;
            ArgumentTypes = String.Empty;
            ArgumentNames = String.Empty;
            IsOverloaded = false;
            IsExternal = false;
        }

        public LibraryItem(NodeType type, string qualifiedName, StaticMirror dataMirror)
        {
            this.qualifiedName = qualifiedName;
            UpdateDisplayText();

            this.dataMirror = dataMirror;
            Level = -1;
            Type = MemberType.None;
            ItemType = type;
            Assembly = String.Empty;
            ArgumentTypes = String.Empty;
            ArgumentNames = String.Empty;
            IsOverloaded = false;
            IsExternal = false;
        }

        public LibraryItem(NodeType type, string qualifiedName)
        {
            this.qualifiedName = qualifiedName;
            UpdateDisplayText();

            this.dataMirror = null;
            Level = -1;
            Type = MemberType.None;
            ItemType = type;
            Assembly = String.Empty;
            ArgumentTypes = String.Empty;
            ArgumentNames = String.Empty;
            IsOverloaded = false;
            IsExternal = false;
        }

        public void AddRootChildItem(LibraryItem childItem)
        {
            if (null == Children)
            {
                Children = new ObservableCollection<LibraryItem>();
                Children.Add(childItem);
                return;
            }

            if (String.Compare(Children[1].DisplayText, childItem.DisplayText) >= 0)
            {
                Children.Insert(1, childItem);
                return;
            }

            for (int i = 2; i < Children.Count(); i++)
            {
                if ((String.Compare(Children[i - 1].DisplayText, childItem.DisplayText) < 0) && (String.Compare(Children[i].DisplayText, childItem.DisplayText) >= 0))
                {
                    Children.Insert(i, childItem);
                    return;
                }
            }
            Children.Add(childItem);
        }

        public void AddChildItem(LibraryItem childItem)
        {
            if (null == Children)
            {
                Children = new ObservableCollection<LibraryItem>();
                Children.Add(childItem);
                return;
            }

            if (Children.Count == 0)
            {
                Children.Add(childItem);
                return;
            }

            if (String.Compare(Children[0].DisplayText, childItem.DisplayText) >= 0)
            {
                Children.Insert(0, childItem);
                return;
            }

            for (int i = 1; i < Children.Count(); i++)
            {
                if ((String.Compare(Children[i - 1].DisplayText, childItem.DisplayText) < 0) && (String.Compare(Children[i].DisplayText, childItem.DisplayText) >= 0))
                {
                    Children.Insert(i, childItem);
                    return;
                }
            }
            Children.Add(childItem);
        }

        #endregion

        #region Public Class Properties

        public StaticMirror DataMirror
        {
            get
            {
                return this.dataMirror;
            }
        }
        public bool IsExternal { get; set; }
        public MemberType Type { get; set; }
        public int Level { get; set; }
        public NodeType ItemType { get; set; }
        public string DisplayText { get; set; }
        public string Assembly { get; set; }
        public string QualifiedName
        {
            get { return this.qualifiedName; }
            set
            {
                this.qualifiedName = value;
                UpdateDisplayText();
            }
        }
        public string ArgumentTypes { get; set; }
        public string ArgumentNames { get; set; }
        public string ReturnType { get; set; }
        public bool IsOverloaded { get; set; }
        public ObservableCollection<LibraryItem> Children { get; set; }

        #endregion

        #region Private Class Helper Methods

        private void UpdateDisplayText()
        {
            if (String.IsNullOrEmpty(this.qualifiedName))
            {
                this.DisplayText = String.Empty;
                return;
            }

            this.DisplayText = qualifiedName;
            if (qualifiedName == Configurations.LoadingMessage)
                return;
            if (qualifiedName.ToLower().Contains(".dll")
                || qualifiedName.ToLower().Contains(".ds")
                || qualifiedName.ToLower().Contains(".exe"))
                return;

            // If there's a dot and it is not the last character...
            int dotIndex = qualifiedName.IndexOf('.');
            if (-1 != dotIndex && ((dotIndex + 1) < qualifiedName.Length))
            {
                // If 'qualifiedName' is in the form of "Xxx.Yyy"...
                if (Char.IsLetter(qualifiedName[dotIndex + 1]))
                    this.DisplayText = qualifiedName.Substring(dotIndex + 1);
            }
        }

        #endregion
    }
}
