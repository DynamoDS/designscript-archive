using System.ComponentModel;
using Autodesk.DesignScript.Interfaces;
namespace Autodesk.DesignScript.Geometry
{
    public class DSColor
    {
        private ARGBColor mIColor;
        private class ARGBColor : DSColor, IColor
        {
            public ARGBColor(byte[] argb) : base(argb)
            {
            }
        }

        protected byte[] mARGBData { get; set; }
        private string Name { get; set; }
        protected DSColor(byte alpha, byte red, byte green, byte blue)
        {
            mARGBData = new[] { alpha, red, green, blue };
        }

        protected DSColor(byte[] argb)
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

        internal static DSColor FromIColor(IColor color)
        {
            if (null == color)
                return null;
            ARGBColor c = color as ARGBColor;
            if (null != c)
                return c;
            return DSColor.ByARGB(color.AlphaValue, color.RedValue, color.GreenValue, color.BlueValue);
        }

        public DSColor this[string name]
        {
            get
            {
                return typeof(DSColor).GetProperty(name).GetGetMethod().Invoke(null, null) as DSColor;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static DSColor Red
        {
            get { return DSColor.ByARGB(255, 255, 0, 0, "Red"); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static DSColor Black
        {
            get { return DSColor.ByARGB(255, 0, 0, 0, "Black"); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static DSColor White
        {
            get { return DSColor.ByARGB(255, 255, 255, 255, "White"); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static DSColor Green
        {
            get { return DSColor.ByARGB(255, 0, 255, 0, "Green"); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static DSColor Blue
        {
            get { return DSColor.ByARGB(255, 0, 0, 255,"Blue"); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static DSColor Yellow
        {
            get { return DSColor.ByARGB(255, 255, 255, 0,"Yellow"); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static DSColor Cyan
        {
            get { return DSColor.ByARGB(255, 0, 255, 255,"Cyan"); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static DSColor Magenta
        {
            get { return DSColor.ByARGB(255, 255, 0, 255,"Magenta"); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static DSColor Purple
        {
            get { return DSColor.ByARGB(255, 160, 32, 240,"Purple"); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static DSColor Periwinkle
        {
            get { return DSColor.ByARGB(255, 142, 130, 254,"Periwinkle"); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static DSColor Orange
        {
            get { return DSColor.ByARGB(255, 249, 115, 6,"Orange"); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        /// <returns></returns>
        public static DSColor ByARGB(byte alpha, byte red, byte green, byte blue)
        {
            DSColor clr = new DSColor(alpha, red, green, blue);
            return clr;
        }

        private static DSColor ByARGB(byte alpha, byte red, byte green, byte blue, string name)
        {
            DSColor clr = new DSColor(alpha, red, green, blue);
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
                return string.Format("DSColor(A:{0}, R:{1}, G:{2}, B:{3})", mARGBData[0], mARGBData[1], mARGBData[2], mARGBData[3]);
            else
                return "DSColor: " + Name;
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
                return true;

            DSColor color = obj as DSColor;
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

        private bool CheckEquals(DSColor color)
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
