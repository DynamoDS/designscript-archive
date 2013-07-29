using System.ComponentModel;
using Autodesk.DesignScript.Interfaces;
namespace Autodesk.DesignScript.Geometry
{
    public class Color
    {
        private ARGBColor mIColor;
        private class ARGBColor : Color, IColor
        {
            public ARGBColor(byte[] argb) : base(argb)
            {
            }
        }

        protected byte[] mARGBData { get; set; }
        private string Name { get; set; }
        protected Color(byte alpha, byte red, byte green, byte blue)
        {
            mARGBData = new[] { alpha, red, green, blue };
        }

        protected Color(byte[] argb)
        {
            mARGBData = argb;
        }

        internal IColor IColor
        {
            get 
            {
                if (null == mIColor)
                    mIColor = new ARGBColor(this.mARGBData);
                return mIColor;
            }
        }

        internal static Color FromIColor(IColor color)
        {
            if (null == color)
                return null;
            ARGBColor c = color as ARGBColor;
            if (null != c)
                return c;
            return Color.ByARGB(color.AlphaValue, color.RedValue, color.GreenValue, color.BlueValue);
        }

        public Color this[string name]
        {
            get
            {
                return typeof(Color).GetProperty(name).GetGetMethod().Invoke(null, null) as Color;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static Color Red
        {
            get { return Color.ByARGB(255, 255, 0, 0, "Red"); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static Color Black
        {
            get { return Color.ByARGB(255, 0, 0, 0, "Black"); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static Color White
        {
            get { return Color.ByARGB(255, 255, 255, 255, "White"); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static Color Green
        {
            get { return Color.ByARGB(255, 0, 255, 0, "Green"); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static Color Blue
        {
            get { return Color.ByARGB(255, 0, 0, 255,"Blue"); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static Color Yellow
        {
            get { return Color.ByARGB(255, 255, 255, 0,"Yellow"); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static Color Cyan
        {
            get { return Color.ByARGB(255, 0, 255, 255,"Cyan"); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static Color Magenta
        {
            get { return Color.ByARGB(255, 255, 0, 255,"Magenta"); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static Color Purple
        {
            get { return Color.ByARGB(255, 160, 32, 240,"Purple"); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static Color Periwinkle
        {
            get { return Color.ByARGB(255, 142, 130, 254,"Periwinkle"); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static Color Orange
        {
            get { return Color.ByARGB(255, 249, 115, 6,"Orange"); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        /// <returns></returns>
        public static Color ByARGB(byte alpha, byte red, byte green, byte blue)
        {
            Color clr = new Color(alpha, red, green, blue);
            return clr;
        }

        private static Color ByARGB(byte alpha, byte red, byte green, byte blue, string name)
        {
            Color clr = new Color(alpha, red, green, blue);
            clr.Name = name;
            return clr;
        }

        /// <summary>
        /// 
        /// </summary>
        [Category("Primary")]
        public byte AlphaValue { get { return mARGBData[0]; } }

        /// <summary>
        /// 
        /// </summary>
        [Category("Primary")]
        public byte RedValue { get { return mARGBData[1]; } }

        /// <summary>
        /// 
        /// </summary>
        [Category("Primary")]
        public byte GreenValue { get { return mARGBData[2]; } }

        /// <summary>
        /// 
        /// </summary>
        [Category("Primary")]
        public byte BlueValue { get { return mARGBData[3]; } }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (string.IsNullOrEmpty(Name))
                return string.Format("Color(A:{0}, R:{1}, G:{2}, B:{3})", mARGBData[0], mARGBData[1], mARGBData[2], mARGBData[3]);
            else
                return "Color: " + Name;
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
                return true;

            Color color = obj as Color;
            if (null != color)
                return CheckEquals(color);

            IColor icolor = obj as IColor;
            return icolor != null && this.CheckEquals(icolor);
        }

        private bool CheckEquals(IColor color)
        {
            if (color == null)
                return false;

            return color.AlphaValue == this.AlphaValue &&
                color.RedValue == this.RedValue &&
                color.GreenValue == this.GreenValue &&
                color.BlueValue == this.BlueValue;
        }

        private bool CheckEquals(Color color)
        {
            if (color == null)
                return false;

            return color.AlphaValue == this.AlphaValue &&
                color.RedValue == this.RedValue &&
                color.GreenValue == this.GreenValue &&
                color.BlueValue == this.BlueValue;
        }

        public override int GetHashCode()
        {
            return mARGBData.GetHashCode();
        }
    }
}
