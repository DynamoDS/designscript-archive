using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace DesignScript.Editor.Core
{
    public class Utility
    {
        public static bool AreEqual(double valueOne, double valueTwo)
        {
            double difference = Math.Abs(valueOne * 0.00000001);
            return (Math.Abs(valueOne - valueTwo) <= difference);
        }

        public static bool IsPointInRegion(System.Drawing.Point regionStart,
            System.Drawing.Point regionEnd, System.Drawing.Point point)
        {
            if (point.Y < regionStart.Y || (point.Y > regionEnd.Y))
                return false;
            if (point.Y == regionStart.Y && (point.X < regionStart.X))
                return false;
            if (point.Y == regionEnd.Y && (point.X > regionEnd.X))
                return false;

            return true;
        }

        public static double ValidateAgainstScreenWidth(double value, bool capMin, bool capMax)
        {
            double minAllowed = SystemParameters.FullPrimaryScreenWidth / 8.0;
            double maxAllowed = SystemParameters.FullPrimaryScreenWidth / 2.0;

            if (false != capMin && (value < minAllowed))
                value = minAllowed;
            else if (false != capMax && (value > maxAllowed))
                value = maxAllowed;

            return value;
        }

        public static double ValidateAgainstScreenHeight(double value, bool capMin, bool capMax)
        {
            double minAllowed = SystemParameters.FullPrimaryScreenHeight / 8.0;
            double maxAllowed = SystemParameters.FullPrimaryScreenHeight / 2.0;

            if (false != capMin && (value < minAllowed))
                value = minAllowed;
            else if (false != capMax && (value > maxAllowed))
                value = maxAllowed;

            return value;
        }
    }

    public class BinaryVersion
    {
        #region Public Class Operational Methods

        public static BinaryVersion FromString(string version)
        {
            if (string.IsNullOrEmpty(version))
                return null;

            string[] parts = version.Split('.');
            if (null == parts || (parts.Length != 4))
                return null;

            ushort major = 0, minor = 0, build = 0, priv = 0;
            if (!ushort.TryParse(parts[0], out major))
                return null;
            if (!ushort.TryParse(parts[1], out minor))
                return null;
            if (!ushort.TryParse(parts[2], out build))
                return null;
            if (!ushort.TryParse(parts[3], out priv))
                return null;

            return new BinaryVersion(major, minor, build, priv);
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}.{2}.{3}",
                this.FileMajor.ToString(),
                this.FileMinor.ToString(),
                this.FileBuild.ToString(),
                this.FilePrivate.ToString());
        }

        public override int GetHashCode()
        {
            int high = (int)((this.Value & 0xffffffff00000000) >> 32);
            int low = (int)(this.Value & 0x00000000ffffffff);
            return high ^ low;
        }

        public override bool Equals(object other)
        {
            BinaryVersion rhs = other as BinaryVersion;
            return (this == rhs);
        }

        public static bool operator <(BinaryVersion lhs, BinaryVersion rhs)
        {
            if (System.Object.ReferenceEquals(lhs, rhs))
                return false;

            if (((object)lhs) == null || (((object)rhs) == null))
                return false; // Cannot compare with either one being null.

            return lhs.Value < rhs.Value;
        }

        public static bool operator <=(BinaryVersion lhs, BinaryVersion rhs)
        {
            if (System.Object.ReferenceEquals(lhs, rhs))
                return false;

            if (((object)lhs) == null || (((object)rhs) == null))
                return false; // Cannot compare with either one being null.

            return lhs.Value <= rhs.Value;
        }

        public static bool operator >(BinaryVersion lhs, BinaryVersion rhs)
        {
            if (System.Object.ReferenceEquals(lhs, rhs))
                return false;

            if (((object)lhs) == null || (((object)rhs) == null))
                return false; // Cannot compare with either one being null.

            return lhs.Value > rhs.Value;
        }

        public static bool operator >=(BinaryVersion lhs, BinaryVersion rhs)
        {
            if (System.Object.ReferenceEquals(lhs, rhs))
                return false;

            if (((object)lhs) == null || (((object)rhs) == null))
                return false; // Cannot compare with either one being null.

            return lhs.Value >= rhs.Value;
        }

        public static bool operator ==(BinaryVersion lhs, BinaryVersion rhs)
        {
            if (System.Object.ReferenceEquals(lhs, rhs))
                return true;

            if (((object)lhs) == null || (((object)rhs) == null))
                return false;

            return lhs.Value == rhs.Value;
        }

        public static bool operator !=(BinaryVersion lhs, BinaryVersion rhs)
        {
            return !(lhs == rhs);
        }

        #endregion

        #region Public Class Properties

        internal ushort FileMajor { get; private set; }
        internal ushort FileMinor { get; private set; }
        internal ushort FileBuild { get; private set; }
        internal ushort FilePrivate { get; private set; }
        internal ulong Value { get; private set; }

        #endregion

        private BinaryVersion(ushort major, ushort minor, ushort build, ushort priv)
        {
            this.FileMajor = major;
            this.FileMinor = minor;
            this.FileBuild = build;
            this.FilePrivate = priv;

            ulong v1 = ((((ulong)major) << 48) & 0xffff000000000000);
            ulong v2 = ((((ulong)minor) << 32) & 0x0000ffff00000000);
            ulong v3 = ((((ulong)build) << 16) & 0x00000000ffff0000);
            ulong v4 = (((ulong)priv) & 0x000000000000ffff);
            this.Value = v1 | v2 | v3 | v4;
        }
    }
}
