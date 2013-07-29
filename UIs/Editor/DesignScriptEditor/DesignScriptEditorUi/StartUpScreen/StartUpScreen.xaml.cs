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
using System.Diagnostics;
using System.IO;

namespace DesignScript.Editor.StartUp
{
    using DesignScript.Editor.Core;
    using DesignScript.Editor;
    using System.Collections.ObjectModel;

    public partial class StartUpScreen : UserControl
    {
        List<RecentFile> recentFileList = new List<RecentFile>();
        ObservableCollection<SampleFileProperty> sampleFiles = null;

        public StartUpScreen()
        {
            InitializeComponent();
            recentFileList.AddRange(TextEditorControl.Instance.TextCore.Data.RecentFiles);
            recentFileList.Reverse();
            recentFileList.RemoveAll(item => !File.Exists(item.FilePath));
            lstRecentFiles.ItemsSource = recentFileList;
            lstRecentFiles.Items.Refresh();

            string executingAssemblyPathName = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string rootModuleDirectory = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(executingAssemblyPathName));
            string fullPathName = System.IO.Path.Combine(rootModuleDirectory, "Resources", "Samples");
            string root = fullPathName;

            SampleFileProperty rootProperty = new SampleFileProperty("Samples", "Path");
            WalkDirectoryTree(new System.IO.DirectoryInfo(root), rootProperty);

            sampleFiles = new ObservableCollection<SampleFileProperty>();
            sampleFiles.Add(rootProperty);
            InternalTreeView.ItemsSource = sampleFiles;
            this.Focus();
        }

        private void WalkDirectoryTree(System.IO.DirectoryInfo root, SampleFileProperty rootProperty)
        {
            try
            {
                // First try to get all the sub-directories before the files themselves.
                System.IO.DirectoryInfo[] directories = root.GetDirectories();
                if (null != directories && (directories.Length > 0))
                {
                    foreach (System.IO.DirectoryInfo directory in directories)
                    {
                        // Resursive call for each subdirectory.
                        SampleFileProperty subProperty = new SampleFileProperty(directory.Name, directory.FullName);
                        WalkDirectoryTree(directory, subProperty);
                        rootProperty.AddChildProperty(subProperty);
                    }
                }

                // Secondly, process all the files directly under this folder 
                System.IO.FileInfo[] scriptFiles = null;
                scriptFiles = root.GetFiles("*.ds", System.IO.SearchOption.TopDirectoryOnly);

                if (null != scriptFiles && (scriptFiles.Length > 0))
                {
                    foreach (System.IO.FileInfo file in scriptFiles)
                    {
                        // Add each file under the root directory property list.
                        rootProperty.AddChildProperty(new SampleFileProperty(file.Name, file.FullName));
                    }
                }
            }
            catch (Exception)
            {
                // Perhaps some permission problems?
            }
        }

        private void OnRecentFileClick(object sender, MouseButtonEventArgs e)
        {
            string filePath = null;
            TextEditorControl textEditorControl = TextEditorControl.Instance;

            if (null != lstRecentFiles.SelectedItem)
                filePath = ((RecentFile)lstRecentFiles.SelectedItem).FilePath;

            if (string.IsNullOrEmpty(filePath))
                return;

            if (!System.IO.File.Exists(filePath))
            {
                // @TODO(Ananya): Ask if the user wants to remove the file from list.
                string message = string.Format("File '{0}' cannot be found! Do you want to remove the file from the Recent file list", filePath);
                MessageBoxResult messageBoxResult = MessageBox.Show(message, " ", MessageBoxButton.YesNo);
                if (messageBoxResult.Equals(MessageBoxResult.Yes))
                    textEditorControl.TextCore.Data.RemoveFromRecentFileList(((RecentFile)lstRecentFiles.SelectedItem).FilePath);

                recentFileList.Clear();
                recentFileList.AddRange(textEditorControl.TextCore.Data.RecentFiles);
                recentFileList.Reverse();

                lstRecentFiles.Items.Refresh();

                return;
            }

            if (textEditorControl.TextCore.LoadScriptFromFile(filePath))
            {
                textEditorControl.SetupTabInternal(filePath);
            }
            else
            {
                int index = Solution.Current.GetScriptIndexFromPath(filePath);
                if (index >= 0)
                {
                    textEditorControl.ScriptTabControl.ActivateTab(index);
                }
                else
                {
                    MessageBox.Show(Configurations.UnsupportedFileType);
                    return;
                }
            }

            textEditorControl.HandleScriptActivation();
            textEditorControl.grid.UpdateLayout();
            CommandManager.InvalidateRequerySuggested();
            textEditorControl.textCanvas.Focus();
        }

        // @TODO(Ananya) Remove this.
        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void OnSampleFileClick(object sender, MouseButtonEventArgs e)
        {
            string filePath = null;
            TextEditorControl textEditorControl = TextEditorControl.Instance;

            if (null != InternalTreeView.SelectedItem)
                filePath = ((SampleFileProperty)InternalTreeView.SelectedItem).FilePath;

            if (string.IsNullOrEmpty(filePath) || !filePath.Contains(".ds"))
                return;

            if (!System.IO.File.Exists(filePath))
            {
                MessageBox.Show(string.Format("File '{0}' cannot be found!", filePath));
                return;
            }

            // @TODO(Ben): PRIORITY: Consolidate all these.
            if (textEditorControl.TextCore.LoadScriptFromFile(filePath))
                textEditorControl.SetupTabInternal(filePath);
            else
            {
                int index = Solution.Current.GetScriptIndexFromPath(filePath);
                if (index < 0)
                    return;

                textEditorControl.ScriptTabControl.ActivateTab(index);
            }

            textEditorControl.HandleScriptActivation();
            textEditorControl.grid.UpdateLayout();
            CommandManager.InvalidateRequerySuggested();
            textEditorControl.textCanvas.Focus();
        }

        private void OnLinkButtonClick(object sender, RoutedEventArgs e)
        {
            Button linkButton = sender as Button;
            if (null == linkButton)
                return;

            string link = string.Empty;
            switch (linkButton.Name)
            {
                case "StartupLink0":
                    link = Configurations.AutodeskComForum;
                    break;
                case "StartupLink1":
                    link = Configurations.YouTubeComVideo;
                    break;
                case "StartupLink2":
                    link = Configurations.DesignScriptOrgManual;
                    break;
                case "StartupLink3":
                    link = "DesignScriptMethods.htm";
                    break;
                case "StartupLink4":
                    link = Configurations.DesignScriptOrgPlugIns;
                    break;
                case "StartupLink5":
                    link = Configurations.DesignScriptOrgGallery;
                    break;
            }

            if (string.IsNullOrEmpty(link))
                return;

            if (link.StartsWith("http"))
                Process.Start(link);
            else
                HelpAndReferenceClick(link);
        }

        private void HelpAndReferenceClick(string fileName)
        {
            Configurations.HelpAndReferenceClick(fileName);
        }
    }

    public class SampleFileProperty
    {
        ObservableCollection<SampleFileProperty> childProperties = null;

        public SampleFileProperty(string name, string path)
        {
            this.FileName = name;
            this.FilePath = path;
        }

        public void AddChildProperty(SampleFileProperty childProperty)
        {
            if (null == childProperties)
                childProperties = new ObservableCollection<SampleFileProperty>();

            childProperties.Add(childProperty);
        }

        public string FileName { get; private set; }
        public string FilePath { get; private set; }
        public ObservableCollection<SampleFileProperty> Children { get { return childProperties; } }
    }
}
