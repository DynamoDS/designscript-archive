using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace DesignScript.Editor
{
    class OutputMessage
    {
        private BitmapImage outputPointImage = null;
        private string textContent;

        public OutputMessage()
        {
            outputPointImage = new BitmapImage(new Uri(Images.OutputMessage, UriKind.Absolute));
        }

        public string TextContent
        {
            get {return textContent;}
            set {textContent = value ;}
        }
    }
}
