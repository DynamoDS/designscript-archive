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

namespace DesignScript.Editor
{
    /// <summary>
    /// Interaction logic for Disassembly.xaml
    /// </summary>
    public partial class Disassembly : UserControl
    {
        private static Disassembly instance = null;
        private ObservableCollection<DisassemblyEntry> instructions = null;

        public Disassembly()
        {
            InitializeComponent();

            instructions = new ObservableCollection<DisassemblyEntry>();
            this.DisassemblyListBox.ItemsSource = instructions;
            this.DisassemblyListBox.DisplayMemberPath = "InstructionString";
        }

        internal static Disassembly Instance
        {
            get
            {
                if (null == Disassembly.instance)
                {
                    Disassembly.instance = new Disassembly();
                    TextEditorControl.Instance.InsertWidget(
                        EditorWidgetBar.Widget.Disassembly, Disassembly.instance);
                }

                return Disassembly.instance;
            }
        }

        internal void BindNewInstructionList(List<DisassemblyEntry> newInstrs)
        {
            this.instructions.Clear();
            foreach (DisassemblyEntry entry in newInstrs)
                this.instructions.Add(entry);
        }

        private void OnDisassemblyMenuCopy(object sender, RoutedEventArgs e)
        {
            MenuItem menuitem = e.Source as MenuItem;

            if (menuitem.Header.ToString() == "Copy All")
            {
                CopyAllDisassemblyMessage();
            }
        }

        private void CopyAllDisassemblyMessage()
        {
            string textToCopy = null;

            foreach (DisassemblyEntry entry in this.instructions)
            {
                if (null != textToCopy)
                    textToCopy += '\n';
                textToCopy += entry.InstructionString;
            }

            if (textToCopy == null)
                return;
            else
                textToCopy.Replace("\n", "\r\n");

            try
            {
                Clipboard.SetData(DataFormats.Text, textToCopy);
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                System.Threading.Thread.Sleep(0);
                try
                {
                    Clipboard.SetData(DataFormats.Text, textToCopy);
                }
                catch (System.Runtime.InteropServices.COMException)
                {
                    MessageBox.Show("Can't Access Clipboard");
                }
            }
        }
    }
}
