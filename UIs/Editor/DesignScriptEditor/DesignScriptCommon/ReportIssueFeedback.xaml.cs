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
using System.Security.Cryptography;

namespace DesignScript.Editor.Common
{
    using DesignScript.Editor.Core;
    //using DesignScript.Editor.Automation;

    public partial class ReportIssueFeedback : Window
    {
        public ReportIssueFeedback()
        {
            InitializeComponent();
            InitializeReportWindow();
            this.SendMessage = false;
            SubmitButton.Click += new RoutedEventHandler(OnSubmitButton);
            cbFeedbackType.SelectedIndex = 0;
            txtResponse.Text = Configurations.ReportIssueContent;
            ReportContent.Text = Configurations.ReportIssueEmailId;
        }

        public void OnWindowClose(object sender, RoutedEventArgs e)
        {
            this.SendMessage = false;
            this.Close();
        }

        public void OnSubmitButton(object sender, RoutedEventArgs e)
        {
            this.SendMessage = true;
            this.Close();
        }

        public void InitializeReportWindow()
        {
            this.AddHandler(FrameworkElement.KeyUpEvent, new KeyEventHandler(OnReportWindowKeyUp), true);
        }

        public void OnReportWindowKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
        }

        private void OnTxtResponseGotFocus(object sender, RoutedEventArgs e)
        {
            if (txtResponse.Text == Configurations.ReportIssueContent)
                txtResponse.Text = "";
        }

        private void OnTxtResponseLostFocus(object sender, RoutedEventArgs e)
        {
            if (txtResponse.Text == "")
                txtResponse.Text = Configurations.ReportIssueContent;
        }

        private void OnReportContentGotFocus(object sender, RoutedEventArgs e)
        {
            if (ReportContent.Text == Configurations.ReportIssueEmailId)
                ReportContent.Text = "";
        }

        private void OnReportContentLostFocus(object sender, RoutedEventArgs e)
        {
            if (ReportContent.Text == "")
                ReportContent.Text = Configurations.ReportIssueEmailId;
        }

        public bool SendMessage { get; private set; }
        public string FeedbackType { get; private set; }
        public string Report { get; private set; }
        public string Email { get; private set; }
    }
}
