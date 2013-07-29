using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;

namespace DSGeometry
{
    class DsVector : IVector
    {
        public static DsVector ByCoordinates(double x, double y, double z)
        {
            return new DsVector { X = x, Y = y, Z = z };
        }

        public double X { get; set; }

        public double Y { get; set; }

        public double Z { get; set; }
    }

    class DsColor : IColor
    {
        public byte AlphaValue { get; set; }

        public byte RedValue { get; set; }

        public byte GreenValue { get; set; }

        public byte BlueValue { get; set; }
    }

    static class Utils
    {
        public static Vector ToVector(this IVector v)
        {
            return Vector.ByCoordinates(v.X, v.Y, v.Z);
        }

        public static IVector ToIVector(this Vector v)
        {
            return new DsVector { X = v.X, Y = v.Y, Z = v.Z };
        }

        public static IVector Scale(this IVector v, double scale)
        {
            return v.ToVector().Scale(scale).ToIVector();
        }

        public static double Length(this IVector v)
        {
            return v.ToVector().Length;
        }

        public static Color ToColor(this IColor c)
        {
            return Color.ByARGB(c.AlphaValue, c.RedValue, c.GreenValue, c.BlueValue);
        }

        public static IColor ToIColor(this Color c)
        {
            return new DsColor { AlphaValue = c.AlphaValue, RedValue = c.RedValue, GreenValue = c.GreenValue, BlueValue = c.BlueValue };
        }
    }
}
