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
using System.Windows.Shapes;
using System.Xml.Serialization;
using DesignScript.Editor.Core;
using System.Reflection;

namespace DesignScript.Editor.Automation
{
    /// <summary>
    /// Interaction logic for AddAssert.xaml
    /// </summary>
    public partial class AddAssert : Window
    {
        List<string> propertiesList = null;
        IAssertableProperties assertable = null;
        Type type;
        List<CommandAssert> asserts = null;
        bool keyDownFlag = false;

        public AddAssert(IAssertableProperties assertable)
        {
            this.assertable = assertable;

            asserts = null;
            InitializeComponent();
            InitializeAssertWindow();
            propertiesList = new List<string>();
            BindPropertyListData();
            PropertyList.SelectionChanged += new SelectionChangedEventHandler(OnComboBoxSelectionChanged);
            AddButton.Click += new RoutedEventHandler(OnAddAssertButton);
        }

        public void BindPropertyListData()
        {
            type = typeof(IAssertableProperties);
            foreach (PropertyInfo property in type.GetProperties())
                propertiesList.Add(property.Name);

            PropertyList.ItemsSource = propertiesList;
        }

        public void OnComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox propertyBox = sender as ComboBox;
            int selected = propertyBox.SelectedIndex;
            if (selected < 0)
                return;

            string propertyName = propertiesList[selected];
            PropertyInfo property = type.GetProperty(propertyName);

            tbMessage.Text = string.Empty;
            object propertyValue = (property.GetValue(this.assertable, null));
            if (null != propertyValue)
            {
                PropertyValue.Text = propertyValue.ToString();

                if (!AssertValue.Text.Equals(PropertyValue.Text))
                    AssertValue.Text = PropertyValue.Text;
            }
        }

        public void OnAddAssertButton(object sender, RoutedEventArgs e)
        {
            if (null == asserts)
                asserts = new List<CommandAssert>();

            asserts.Add(new CommandAssert(PropertyList.SelectedValue.ToString(), AssertValue.Text));
            AssertValue.Text = "";
            tbMessage.Text = "Assert added!";

        }

        public List<CommandAssert> GetAsserts()
        {
            List<CommandAssert> temp = asserts;
            asserts = null;
            return temp;
        }

        void InitializeAssertWindow()
        {
            this.AddHandler(FrameworkElement.KeyDownEvent, new KeyEventHandler(OnAssertWindowKeyDown), true);
            this.AddHandler(FrameworkElement.KeyUpEvent, new KeyEventHandler(OnAssertWindowKeyUp), true);
        }

        void OnAssertWindowKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Escape))
                keyDownFlag = true;
        }

        void OnAssertWindowKeyUp(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyUp(Key.Escape) && keyDownFlag == true)
            {
                keyDownFlag = false;
                this.Close();
            }
        }
    }
}
