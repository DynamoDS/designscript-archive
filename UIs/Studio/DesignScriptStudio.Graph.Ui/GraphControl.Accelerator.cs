using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using DesignScriptStudio.Graph.Core;

namespace DesignScriptStudio.Graph.Ui
{
    public partial class GraphControl : UserControl
    {
        #region Routed Command Definitions

        public static RoutedCommand DisableCommand = new RoutedCommand();

        public static RoutedCommand NewCommand = new RoutedCommand();
        public static RoutedCommand SaveCommand = new RoutedCommand();
        public static RoutedCommand SaveAsCommand = new RoutedCommand();
        public static RoutedCommand LoadCommand = new RoutedCommand();
        public static RoutedCommand UndoCommand = new RoutedCommand();
        public static RoutedCommand RedoCommand = new RoutedCommand();
        public static RoutedCommand DeleteCommand = new RoutedCommand();
        public static RoutedCommand SwitchCanvasCommand = new RoutedCommand();
        public static RoutedCommand CloseCanvasCommand = new RoutedCommand();
        public static RoutedCommand ExitCommand = new RoutedCommand();
        public static RoutedCommand DumpUiCommand = new RoutedCommand();
        public static RoutedCommand DumpVmCommand = new RoutedCommand();
        public static RoutedCommand ConvertNodesToCodeCommand = new RoutedCommand();
        public static RoutedCommand PlayIntroVideoCommand = new RoutedCommand();
        public static RoutedCommand OpenHelpTopicsCommand = new RoutedCommand();
        public static RoutedCommand ReportIssueCommand = new RoutedCommand();

        public static RoutedCommand CloseAllCanvasCommand = new RoutedCommand();
        public static RoutedCommand CloseAllButCanvasCommand = new RoutedCommand();
        public static RoutedCommand CloseAllRightCanvasCommand = new RoutedCommand();

        public static RoutedCommand RenameCanvasCommand = new RoutedCommand();
        public static RoutedCommand CopyCanvasPathCommand = new RoutedCommand();
        public static RoutedCommand OpenCanvasFolderCommand = new RoutedCommand();

        public static RoutedCommand ImportCommand = new RoutedCommand();

        public static RoutedCommand ViewAllCommand = new RoutedCommand();
        public static RoutedCommand PanCommand = new RoutedCommand();
        public static RoutedCommand ZoomInCommand = new RoutedCommand();
        public static RoutedCommand ZoomOutCommand = new RoutedCommand();

        #endregion

        #region Command Binding Methods

        private void ConvertNodesToCodeCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void ConvertNodesToCodeExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            GraphCanvas graphCanvas = this.CurrentGraphCanvas;
            graphCanvas.VisualHost.HandleConvertNodesToCode();
        }

        private void PlayIntroVideoCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void PlayIntroVideoExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (System.Windows.SystemParameters.IsRemoteSession)
            {
                System.Diagnostics.Process.Start(CoreStrings.IntroVideoUrl);
            }
            else
            {
                IntroVideoPlayer player = new IntroVideoPlayer();
                player.ShowDialog();
            }
        }

        private void OpenHelpTopicsCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void OpenHelpTopicsExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            string fileName = "Help.htm";

            try
            {
                string executingAssemblyPathName = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string rootModuleDirectory = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(executingAssemblyPathName));
                string fullPathName = System.IO.Path.Combine(rootModuleDirectory, "Help", fileName);

                if (File.Exists(fullPathName))
                    System.Diagnostics.Process.Start(fullPathName);
                else
                    System.Diagnostics.Process.Start(CoreStrings.DesignScriptOrgReference);
            }
            catch
            {
                System.Diagnostics.Process.Start(CoreStrings.DesignScriptOrgReference);
            }
        }

        private void ReportIssueCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void ReportIssueExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            OnOptionReportClicked(sender, e);
        }

        private void DisableCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            e.Handled = true;
        }

        private void DisableExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            return;
        }

        private void NewCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void NewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.graphCanvases.Count() == 0 || tabControl.CloseTab(tabControl.TabControl.SelectedContent))
            {
                GraphCanvas graphCanvas = new GraphCanvas(this);
                AddGraphCanvas(graphCanvas, null);
            }
        }

        private void SaveCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            if (this.CurrentGraphCanvas != null && (this.CurrentGraphCanvas.VisualHost != null))
                e.CanExecute = this.CurrentGraphCanvas.Controller.IsModified;

            e.Handled = true;
        }

        private void SaveExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            GraphCanvas graphCanvas = this.CurrentGraphCanvas;
            if (null != graphCanvas)
                graphCanvas.VisualHost.HandleShortcutSave();
        }

        private void SaveAsCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.CurrentGraphCanvas != null)
                e.CanExecute = true;
            e.Handled = true;
        }

        private void SaveAsExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            GraphCanvas graphCanvas = this.CurrentGraphCanvas;
            if (null != graphCanvas)
                graphCanvas.VisualHost.HandleShortcutSaveAs();
        }

        private void LoadCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void LoadExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.graphCanvases.Count() == 0 || tabControl.CloseTab(tabControl.TabControl.SelectedContent))
            {
                string fileFilter = "DesignScript Graph Files " +
                    "(*.bin)|*.bin|All Files (*.*)|*.*";

                // Displays an OpenFileDialog for user to select a file.
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = fileFilter;
                openFileDialog.Title = "Select a DesignScript Graph file";
                openFileDialog.Multiselect = false;

                bool? result = openFileDialog.ShowDialog();
                if (result.HasValue && (result.Value == true))
                {
                    int index = tabControl.FindTab(openFileDialog.FileName);
                    if (index < 0)
                    {
                        GraphCanvas graphCanvas = new GraphCanvas(this, openFileDialog.FileName);
                        AddGraphCanvas(graphCanvas, (new FileInfo(openFileDialog.FileName)).Name);
                    }
                    else
                        tabControl.TabControl.SelectedIndex = index;
                }
            }

            if (this.CurrentGraphCanvas != null)
                this.CurrentGraphCanvas.Focus();
            else
                this.MasterGrid.Focus();

            e.Handled = true;
        }

        private void UndoCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            if (this.CurrentGraphCanvas != null && (this.CurrentGraphCanvas.VisualHost != null))
                e.CanExecute = this.CurrentGraphCanvas.Controller.CanUndo();

            e.Handled = true;
        }

        private void UndoExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            GraphCanvas graphCanvas = this.CurrentGraphCanvas;
            if (null != graphCanvas)
                graphCanvas.VisualHost.HandleShortcutUndo();
        }

        private void RedoCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            if (this.CurrentGraphCanvas != null && (this.CurrentGraphCanvas.VisualHost != null))
                e.CanExecute = this.CurrentGraphCanvas.Controller.CanRedo();

            e.Handled = true;
        }

        private void RedoExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            GraphCanvas graphCanvas = this.CurrentGraphCanvas;
            if (null != graphCanvas)
                graphCanvas.VisualHost.HandleShortcutRedo();
        }

        private void DeleteCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void DeleteExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            GraphCanvas graphCanvas = this.CurrentGraphCanvas;
            if (null != graphCanvas)
                graphCanvas.VisualHost.HandleShortcutDelete();
        }

        private void SwitchCanvasCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (graphCanvases.Count() > 0)
                e.CanExecute = true;
            else
                e.CanExecute = false;
            e.Handled = true;
        }

        private void SwitchCanvasExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            tabControl.SwitchTab();
        }

        private void CloseCanvasCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (graphCanvases.Count() > 0)
                e.CanExecute = true;
            else
                e.CanExecute = false;
            e.Handled = true;
        }

        private void CloseCanvasExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            TabItem tabItem = e.Parameter as TabItem;
            if (tabItem != null)
                tabControl.CloseTab(tabItem);
            else
                tabControl.CloseTab(tabControl.TabControl.SelectedContent);
        }

        private void ExitCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void ExitExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (tabControl.CloseAllTab())
                System.Windows.Window.GetWindow(this).Close();
        }

        private void DumpUiStateCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void DumpUiStateExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.HandleShortcutDumpState(true); // Dump recorded UI states.
        }

        private void DumpVmStateCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void DumpVmStateExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.HandleShortcutDumpState(false); // Dump recorded VM states.
        }

        private void CloseAllCanvasCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (graphCanvases.Count() > 1)
                e.CanExecute = true;
            else
                e.CanExecute = false;
            e.Handled = true;
        }

        private void CloseAllCanvasExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            tabControl.CloseAllTab();
        }

        private void CloseAllButCanvasCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (graphCanvases.Count() > 1)
                e.CanExecute = true;
            else
                e.CanExecute = false;
            e.Handled = true;
        }

        private void CloseAllButCanvasExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            tabControl.CloseAllButTab(tabControl.TabControl.SelectedContent);
        }

        private void CloseAllRightCanvasCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = tabControl.CloseAllRightTabCanExecute(tabControl.TabControl.SelectedContent);
            e.Handled = true;
        }

        private void CloseAllRightCanvasExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            tabControl.CloseAllRightTab(tabControl.TabControl.SelectedContent);
        }

        private void RenameCanvasCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void RenameCanvasExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            tabControl.RenameTab(((ContextMenu)e.Parameter).TemplatedParent);
        }

        private void CopyCanvasPathCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CheckPath(tabControl.TabControl.SelectedContent);
            e.Handled = true;
        }

        private void CopyCanvasPathExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            tabControl.CopyTabPath(tabControl.TabControl.SelectedContent);
        }

        private void OpenCanvasFolderCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CheckPath(tabControl.TabControl.SelectedContent);
            e.Handled = true;
        }

        private void OpenCanvasFolderExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            tabControl.OpenTabFolder(tabControl.TabControl.SelectedContent);
        }

        private void ImportCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.CurrentGraphCanvas != null)
                e.CanExecute = true;
            else
                e.CanExecute = false;
            e.Handled = true;
        }

        private void ImportExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.libraryView.HandleAddNew();
        }

        private void ViewAllCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.CurrentGraphCanvas != null)
                e.CanExecute = true;
            else
                e.CanExecute = false;
            e.Handled = true;
        }

        private void ViewAllExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.CurrentGraphCanvas.VisualHost.HandleZoomToFit();
        }

        private void PanCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.CurrentGraphCanvas != null)
                e.CanExecute = true;
            else
                e.CanExecute = false;
            e.Handled = true;
        }

        private void PanExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.CurrentGraphCanvas.VisualHost.HandleTogglePan();
        }

        private void ZoomInCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.CurrentGraphCanvas != null)
                e.CanExecute = true;
            else
                e.CanExecute = false;
            e.Handled = true;
        }

        private void ZoomInExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.CurrentGraphCanvas.VisualHost.HandleZoomIn();
        }

        private void ZoomOutCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.CurrentGraphCanvas != null)
                e.CanExecute = true;
            else
                e.CanExecute = false;
            e.Handled = true;
        }

        private void ZoomOutExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.CurrentGraphCanvas.VisualHost.HandleZoomOut();
        }

        #endregion
    }
}
