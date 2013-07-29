using System;
using System.ComponentModel;
using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    internal class DsVector : IVector
    {
        public static IVector ByCoordinates(double x, double y, double z)
        {
            return new DsVector() { X = x, Y = y, Z = z };
        }

        public double X { get; set; }

        public double Y { get; set; }

        public double Z { get; set; }
    }

    public class DSVector
    {
        #region INSTANCE_VARIABLES
        DsVector vector = new DsVector();
        #endregion

        #region PROPERTIES

        static internal DSVector XAxis = new DSVector(1.0, 0.0, 0.0);
        static internal DSVector YAxis = new DSVector(0.0, 1.0, 0.0);
        static internal DSVector ZAxis = new DSVector(0.0, 0.0, 1.0);

        internal IVector IVector { get { return vector; } }
        /// <summary>
        /// 
        /// </summary>
        [Category("Primary")]
        public double X
        {
            get
            {
                return vector.X;
            }
            private set
            {
                vector.X = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Category("Primary")]
        public double Y
        {
            get
            {
                return vector.Y;
            }
            private set
            {
                vector.Y = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Category("Primary")]
        public double Z
        {
            get
            {
                return vector.Z;
            }
            private set
            {
                vector.Z = value;
            }
        }

        private bool? isNormalized;
        /// <summary>
        /// 
        /// </summary>
        public bool IsNormalized
        {
            get
            {
                if (!isNormalized.HasValue)
                {
                    if (DSGeometryExtension.Equals(GetSquaredLength(), 1.0))
                    {
                        isNormalized = true;
                    }
                    else
                    {
                        isNormalized = false;
                    }
                }
                return isNormalized.Value;
            }
            private set
            {
                isNormalized = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double Length
        {
            get
            {
                return GetLength();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double? T { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public double? Distance { get; internal set; }

        public DSCoordinateSystem ContextCoordinateSystem { get; private set; }
        #endregion

        #region CONSTRUCTORS

        internal DSVector(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
            
            //ParentCoordinateSystem = null;
        }

        internal DSVector(double x, double y, double z, bool normalize)
        {
            X = x;
            Y = y;
            Z = z;

            if (normalize)
            {
                var len = GetLength();
                if (len.EqualsTo(0.0))
                    return;
                this.X /= len;
                this.Y /= len;
                this.Z /= len;

                IsNormalized = normalize;
            }
            
        }

        internal DSVector(IVector vector)
        {
            this.X = vector.X;
            this.Y = vector.Y;
            this.Z = vector.Z;
        }

        //  this is stand-in for ByCoordinates
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static DSVector ByCoordinates(double x, double y, double z)
        {
            return ByCoordinates(x, y, z, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static DSVector ByCoordinateArrayN(double[] arr)
        {
            return ByCoordinates(arr[0], arr[1], arr[2], true);
        }

        //  this is stand-in for ByCoordinates(with normalization)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="normalize"></param>
        /// <returns></returns>
        public static DSVector ByCoordinates(double x, double y, double z, bool normalize)
        {
            var vec = new DSVector(x, y, z, normalize);
            return vec;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="normalize"></param>
        /// <returns></returns>
        public static DSVector ByCoordinateArrayN(double[] arr, bool normalize)
        {
            return ByCoordinates(arr[0], arr[1], arr[2], normalize);
        }

        public static DSVector ByCoordinates(DSCoordinateSystem coordinateSystem, double x, double y, double z)
        {
            if (coordinateSystem == null)
            {
                throw new System.ArgumentNullException("coordinateSystem");
            }
            using (var p = DSPoint.ByCartesianCoordinates(coordinateSystem, x, y, z))
            {
                DSVector vec = coordinateSystem.Origin.DirectionTo(p);
                vec.ContextCoordinateSystem = coordinateSystem;
                return vec;
            }
        }
        
        #endregion

        #region UTILITY_METHODS

        internal bool IsZeroVector()
        {
            return DSGeometryExtension.Equals(X, 0.0) && DSGeometryExtension.Equals(Y, 0.0) && DSGeometryExtension.Equals(Z, 0.0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DSVector Normalize()
        {
            return DSVector.ByCoordinates(vector.X, vector.Y, vector.Z, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double GetSquaredLength()
        {
            return vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double GetLength()
        {
            return Math.Sqrt(GetSquaredLength());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public double Dot(DSVector other)
        {
            if (other == null)
            {
                throw new System.ArgumentNullException("other");
            }
            return X * other.X + Y * other.Y + Z * other.Z;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsParallel(DSVector other)
        {
            if (other == null)
            {
                throw new System.ArgumentNullException("other");
            }
            var normalizedThis = Normalize();
            var normalizedOther = other.Normalize();

            var dotProd = normalizedThis.Dot(normalizedOther);
            return DSGeometryExtension.Equals(Math.Abs(dotProd), 1.0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsPerpendicular(DSVector other)
        {
            if (other == null)
            {
                throw new System.ArgumentNullException("other");
            }
            var normalizedThis = Normalize();
            var normalizedOther = other.Normalize();

            var dotProd = normalizedThis.Dot(normalizedOther);
            return DSGeometryExtension.Equals(Math.Abs(dotProd), 0.0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scalingFactor"></param>
        /// <returns></returns>
        public DSVector Scale(double scalingFactor)
        {
            return DSVector.ByCoordinates(X * scalingFactor,
                                Y * scalingFactor,
                                Z * scalingFactor);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public DSVector Cross(DSVector other)
        {
            if (other == null)
            {
                throw new System.ArgumentNullException("other");
            }
            return DSVector.ByCoordinates(Y * other.Z - Z * other.Y,
                                        Z * other.X - X * other.Z,
                                        X * other.Y - Y * other.X);
        }

        public DSVector Transform(DSCoordinateSystem fromCoordinateSystem, DSCoordinateSystem contextCoordinateSystem)
        {
            if (fromCoordinateSystem == null)
                throw new ArgumentNullException("fromCoordinateSystem");
            if (contextCoordinateSystem == null)
                throw new ArgumentNullException("contextCoordinateSystem");
            using (IPointEntity translatedPt = fromCoordinateSystem.Origin.PointEntity.CopyAndTranslate(vector) as IPointEntity)
            {
                using (IPointEntity transformedPt = translatedPt.CopyAndTransform(fromCoordinateSystem.CSEntity, contextCoordinateSystem.CSEntity) as IPointEntity)
                {
                    return contextCoordinateSystem.Origin.PointEntity.GetVectorTo(transformedPt);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public DSVector MultiplyBy(double size)
        {
            return ByCoordinates(X * size, Y * size, Z * size);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static DSVector operator -(DSVector first, DSVector second)
        {
            if (first == null)
            {
                throw new System.ArgumentNullException("first");
            } 
            if (second == null)
            {
                throw new System.ArgumentNullException("second");
            }
            return ByCoordinates(first.X - second.X, first.Y - second.Y, first.Z - second.Z);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static DSVector operator +(DSVector first, DSVector second)
        {
            if (first == null)
            {
                throw new System.ArgumentNullException("first");
            }
            if (second == null)
            {
                throw new System.ArgumentNullException("second");
            }
            return ByCoordinates(first.X + second.X, first.Y + second.Y, first.Z + second.Z);
        }


        internal DSVector Negate()
        {
            return ByCoordinates(-X, -Y, -Z);
        }

        #endregion

        #region FROM_OBJECT

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(this, obj))
            {
                return true;
            }

            DSVector vec = obj as DSVector;
            if (null == vec)
            {
                return false;
            }

            return DSGeometryExtension.Equals(vec.X, X) && DSGeometryExtension.Equals(vec.Y, Y) && DSGeometryExtension.Equals(vec.Z, Z);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var f6 = DSGeometryExtension.DoublePrintFormat;
            return string.Format("DSVector(X = {0}, Y = {1}, Z = {2}, Length = {3}, Normalized = {4})", 
                        X.ToString(f6), Y.ToString(f6), Z.ToString(f6), GetLength().ToString(f6), IsNormalized);
        }
        #endregion
    }
}
