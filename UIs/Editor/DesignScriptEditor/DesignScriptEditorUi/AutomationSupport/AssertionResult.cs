using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace DesignScript.Editor.Automation
{
    public class AssertionResult
    {
        [XmlIgnore]
        public BitmapImage image { get; set; }

        [XmlAttribute]
        public string AssertNumber { get; set; }
        [XmlAttribute]
        public string AssertResult { get; set; }
        [XmlAttribute]
        public string PassOrFail { get; set; }

        public AssertionResult()
        {
        }

        public AssertionResult(string PassOrFail, string assertNum, string result)
        {
            this.PassOrFail = PassOrFail;
            if (PassOrFail == "Pass")
                image = new BitmapImage(new Uri(Images.AssertPass, UriKind.Absolute));
            else if (PassOrFail == "Fail")
                image = new BitmapImage(new Uri(Images.AssertFail, UriKind.Absolute));

            AssertNumber = assertNum;
            AssertResult = result;
        }
    }
}
