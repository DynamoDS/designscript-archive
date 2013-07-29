using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;

namespace DSGeometry
{
    class TextEntity : GeometryEntity, ITextEntity
    {
        private  double fontSize;
        private string textString;
        internal TextEntity()
        {
            FontSize = 10;
            String = "Hello World";
            Orientation = 0;
        }
        public double FontSize
        {
            get { return fontSize; }
            protected set { fontSize = value; }
        }
        public string String
        {
            get { return textString; }
            protected set { textString = value; }
        }
        public int Orientation
        {
            get;
            protected set;
        }
        public double GetFontSize()
        {
            return FontSize;
        }

        public string GetString()
        {
            return String;
        }

        public bool UpdateByCoordinateSystem(ICoordinateSystemEntity cs, int orientation, string textString, double fontSize)
        {
            this.Orientation = orientation;
            this.String = textString;
            this.FontSize = fontSize;
            return true;
        }
    }
}
