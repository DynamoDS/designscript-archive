using System;
using System.ComponentModel;
using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    public class CoordinateSystem : DesignScriptEntity
    {
        #region DATA MEMBERS
        private double? xTranslation;
        private double? yTranslation;
        private double? zTranslation;
        private Vector localXAxis;
        private Vector localYAxis;
        private Vector localZAxis;
        private bool visibilityForHighlight;
        private Point mOrigin;
        private Point mXPoint;
        private Point mYPoint;
        private Curve mContextCurve;
        private Surface mContextSurface;
        private static CoordinateSystem mWCS;
        private bool? isNormalized;
        #endregion

        #region CONSTRUCTORS

        private CoordinateSystem(ICoordinateSystemEntity host, bool visible)
            : base(host)
        {
            InitializeGuaranteedProperties();
            if (visible)
                Visible = visible;
        }

        private static CoordinateSystem CreateCoordinateSystem(CoordinateSystem contextCoordinateSystem, ICoordinateSystemEntity localCoordSys, bool visible)
        {
            ICoordinateSystemEntity csEntity = contextCoordinateSystem.CSEntity.PostMultiplyBy(localCoordSys);
            var cs = new CoordinateSystem(csEntity, visible);
            cs.ContextCoordinateSystem = contextCoordinateSystem;
            cs.LocalXAxis = new Vector(localCoordSys.XAxis);
            cs.LocalYAxis = new Vector(localCoordSys.YAxis);
            cs.LocalZAxis = new Vector(localCoordSys.ZAxis);
            cs.XTranslation = localCoordSys.Origin.X;
            cs.YTranslation = localCoordSys.Origin.Y;
            cs.ZTranslation = localCoordSys.Origin.Z;
            return cs;
        }

        internal static CoordinateSystem ToCS(ICoordinateSystemEntity host, bool visible)
        {
            if(null != host)
            {
                return host.Owner == null ? new CoordinateSystem(host, visible) : host.Owner as CoordinateSystem;
            }

            return null;
        }

        internal ICoordinateSystemEntity CSEntity
        {
            get { return HostImpl as ICoordinateSystemEntity; }
        }

        protected override void DisposeDisplayable()
        {
            Display.DisposeObject();
            base.DisposeDisplayable();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GeometryExtension.DisposeObject(ref mOrigin);
                GeometryExtension.DisposeObject(ref mXPoint);
                GeometryExtension.DisposeObject(ref mYPoint);
                GeometryExtension.DisposeObject(ref mContextCurve);
                GeometryExtension.DisposeObject(ref mContextSurface);
            }
            base.Dispose(disposing);
        }

        #endregion

        #region INTERNAL_METHODS

        private void InitializeGuaranteedProperties()
        {
            XAxis = new Vector(CSEntity.XAxis);
            YAxis = new Vector(CSEntity.YAxis);
            ZAxis = new Vector(CSEntity.ZAxis);
            Display = CSEntity.Display;
        }

        #endregion

        #region PROTECTED_CONSTRUCTORS
        //empty
        #endregion

        #region DESIGNSCRIPT_CONSTRUCTORS

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static CoordinateSystem Identity()
        {
            var xaxis = Vector.ByCoordinates(1, 0, 0);
            var yaxis = Vector.ByCoordinates(0, 1, 0);
            var origin = Point.ByCoordinates(0, 0, 0);

            var cs = HostFactory.Factory.CoordinateSystemByOriginVectors(origin.PointEntity, xaxis.VectorEntity,
                yaxis.VectorEntity);

            if (null == cs)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed,
                    "CoordinateSystem.Identity"));

            var coordSys = new CoordinateSystem(cs, true)
            {
                IsNormalized = true
            };

            return coordSys;
        }

        /// <summary>
        /// Constructs a CoordinateSystem using an origin and two vectors for the X and Y axes. It assumes that the axes are all normalized and orthogonal.
        /// </summary>
        /// <param name="origin">the origin of the CoordinateSystem to be constructed</param>
        /// <param name="xAxis">the x-axis of the CoordinateSystem to be constructed</param>
        /// <param name="yAxis">the y-axis of the CoordinateSystem to be constructed</param>
        /// <returns></returns>
        public static CoordinateSystem ByOriginVectors(Point origin, Vector xAxis, Vector yAxis)
        {
            if (xAxis == null)
                throw new System.ArgumentNullException("xAxis");
            else if (yAxis == null)
                throw new System.ArgumentNullException("yAxis");
            else if (xAxis.IsZeroVector())
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZeroVector, "x axis"), "xAxis");
            else if (yAxis.IsZeroVector())
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZeroVector, "y axis"), "yAxis");

            var zAxis = xAxis.Cross(yAxis);
            return ByOriginVectors(origin, xAxis, yAxis, zAxis, true);
        }

        /// <summary>
        /// Constructs a CoordinateSystem using an origin, three axes, and a flag to select whether to normalize the axes.
        /// </summary>
        /// <param name="origin">the origin of the CoordinateSystem to be constructed</param>
        /// <param name="xAxis">the x-axis of the CoordinateSystem to be constructed</param>
        /// <param name="yAxis">the y-axis of the CoordinateSystem to be constructed</param>
        /// <param name="zAxis">the z-axis of the CoordinateSystem to be constructed</param>
        /// <param name="normalized">flag to select whether to normalize the axes</param>
        /// <returns></returns>
        public static CoordinateSystem ByOriginVectors(Point origin, Vector xAxis, Vector yAxis, Vector zAxis, bool normalized)
        {
            return ByOriginVectors(origin, xAxis, yAxis, zAxis, false, normalized);
        }

        /// <summary>
        /// Constructs a CoordinateSystem using an origin and 3 axes as input. 
        /// It assumes the axes to be normalized.
        /// </summary>
        /// <param name="origin">the origin of the CoordinateSystem to be constructed</param>
        /// <param name="xAxis">the x-axis of the CoordinateSystem to be constructed</param>
        /// <param name="yAxis">the y-axis of the CoordinateSystem to be constructed</param>
        /// <param name="zAxis">the z-axis of the CoordinateSystem to be constructed</param>
        /// <returns></returns>
        public static CoordinateSystem ByOriginVectors(Point origin, Vector xAxis, Vector yAxis, Vector zAxis)
        {
            return ByOriginVectors(origin, xAxis, yAxis, zAxis, false, true);
        }

        /// <summary>
        /// Constructs a CoordinateSystem from an origin point, and two axis vectors as input. The 'sheared' flag is used to determine if the axes stay sheared or orthogonal to each other.
        /// </summary>
        /// <param name="origin">the origin of the CoordinateSystem to be constructed</param>
        /// <param name="xAxis">the x-axis of the CoordinateSystem to be constructed</param>
        /// <param name="yAxis">the y-axis of the CoordinateSystem to be constructed</param>
        /// <param name="isSheared">The 'sheared' flag is used to determine if the axes stay sheared or orthogonal to each other.</param>
        /// <returns></returns>
        public static CoordinateSystem ByOriginVectors(Point origin, Vector xAxis, Vector yAxis, bool isSheared)
        {
            return ByOriginVectors(origin, xAxis, yAxis, isSheared, true);
        }

        /// <summary>
        /// Constructs a CoordinateSystem from an origin point, and two axis 
        /// vectors as input. The 'sheared' flag is used to determine if the 
        /// axes stay sheared or orthogonal to each other and 'normalized' flag 
        /// to normalize axes or not.
        /// </summary>
        /// <param name="origin">the origin of the CoordinateSystem to be constructed</param>
        /// <param name="xAxis">the x-axis of the CoordinateSystem to be constructed</param>
        /// <param name="yAxis">the y-axis of the CoordinateSystem to be constructed</param>
        /// <param name="isSheared">The 'sheared' flag is used to determine if the 
        /// axes stay sheared or orthogonal to each other.</param>
        /// <param name="isNormalized">'normalized' flag is used to determine if axes 
        /// should be normalized or not</param>
        /// <returns></returns>
        public static CoordinateSystem ByOriginVectors(Point origin, Vector xAxis, Vector yAxis, bool isSheared, bool isNormalized)
        {
            if (xAxis == null)
                throw new System.ArgumentNullException("xAxis");
            else if (yAxis == null)
                throw new System.ArgumentNullException("yAxis");
            else if (xAxis.IsZeroVector())
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZeroVector, "x axis"), "xAxis");
            else if (yAxis.IsZeroVector())
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZeroVector, "y axis"), "yAxis");

            var zAxis = xAxis.Cross(yAxis);
            return ByOriginVectors(origin, xAxis, yAxis, zAxis, isSheared, isNormalized);
        }

        /// <summary>
        /// Constructs a CoordinateSystem using origin point, X,Y and Z axis vectors, 'sheared' flag to determine if axes should be sheared or orthogonal and 'normalized' flag to normalize axes or not.
        /// </summary>
        /// <param name="origin">the origin of the CoordinateSystem to be constructed</param>
        /// <param name="xAxis">the x-axis of the CoordinateSystem to be constructed</param>
        /// <param name="yAxis">the y-axis of the CoordinateSystem to be constructed</param>
        /// <param name="zAxis">the z-axis of the CoordinateSystem to be constructed</param>
        /// <param name="isSheared">The 'sheared' flag is used to determine if the axes stay sheared or orthogonal to each other.</param>
        /// <param name="isNormalized">'normalized' flag is used to determine if axes should be normalized or not</param>
        /// <returns></returns>
        public static CoordinateSystem ByOriginVectors(Point origin, Vector xAxis, Vector yAxis, Vector zAxis, 
                                                        bool isSheared, bool isNormalized)
        {
            return ByOriginVectors(origin, xAxis, yAxis, zAxis, isSheared, isNormalized, true);
        }
        internal static CoordinateSystem ByOriginVectors(Point origin, Vector xAxis, Vector yAxis, Vector zAxis,
                                                        bool isSheared, bool isNormalized, bool visible)
        {
            if (origin == null)
                throw new System.ArgumentNullException("origin");
            else if (xAxis == null)
                throw new System.ArgumentNullException("xAxis");
            else if (yAxis == null)
                throw new System.ArgumentNullException("yAxis");
            else if (zAxis == null)
                throw new System.ArgumentNullException("zAxis");
            else if (xAxis.IsZeroVector())
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZeroVector, "x axis"), "xAxis");
            else if (yAxis.IsZeroVector())
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZeroVector, "y axis"), "yAxis");
            else if (zAxis.IsZeroVector())
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZeroVector, "z axis"), "zAxis");
            else if (xAxis.IsParallel(yAxis))
                throw new System.ArgumentException(string.Format(Properties.Resources.IsParallel, "x axis", "y axis", "CoordinateSystem.ByOriginVectors"), "xAxis, yAxis");
            else if (!isSheared && (!xAxis.IsPerpendicular(yAxis) || !yAxis.IsPerpendicular(zAxis) || !zAxis.IsPerpendicular(xAxis)))
            {
                //  this is not the case for sheared but axes are not orthogonal
                //
                zAxis = xAxis.Cross(yAxis);
                yAxis = zAxis.Cross(xAxis);
            }

            if (isNormalized)
            {
                xAxis = xAxis.Normalize();
                yAxis = yAxis.Normalize();
                zAxis = zAxis.Normalize();
            }

            var cs = HostFactory.Factory.CoordinateSystemByOriginVectors(origin.PointEntity, xAxis.VectorEntity,
                yAxis.VectorEntity, zAxis.VectorEntity );

            var coordSys = new CoordinateSystem(cs, visible);

            return coordSys;
        }
        /// <summary>
        /// Constructs a CoordinateSystem using an origin and two points, one on the X-axis and the other on the Y-axis of the new CoordinateSystem. It assumes the three axes of the resulting CoordinateSystem will be orthogonal and normalized even if the input points are not
        /// </summary>
        /// <param name="origin">the origin of the CoordinateSystem to be constructed</param>
        /// <param name="xPoint">a point on the X-axis</param>
        /// <param name="yPoint">a point on the Y-axis</param>
        /// <returns></returns>
        public static CoordinateSystem ByThreePoints(Point origin, Point xPoint, Point yPoint)
        {
            return ByThreePoints(origin, xPoint, yPoint, false, true);
        }

        /// <summary>
        /// Constructs a CoordinateSystem from an origin point, one point on the X-axis and the other on the Y-axis of the new CoordinateSystem. It assumes the axes to be normalized and the 'sheared' flag determines if the axes stay sheared or be made orthogonal.
        /// </summary>
        /// <param name="origin">the origin of the CoordinateSystem to be constructed</param>
        /// <param name="xPoint">a point on the X-axis</param>
        /// <param name="yPoint">a point on the Y-axis</param>
        /// <param name="isSheared">'sheared' flag determines if the axes stay sheared or be made orthogonal.</param>
        /// <returns></returns>
        public static CoordinateSystem ByThreePoints(Point origin, Point xPoint, Point yPoint, bool isSheared)
        {
            return ByThreePoints(origin, xPoint, yPoint, isSheared, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin">the origin of the CoordinateSystem to be constructed</param>
        /// <param name="xPoint">a point on the X-axis</param>
        /// <param name="yPoint">a point on the Y-axis</param>
        /// <param name="isSheared">'sheared' flag determines if the axes stay sheared or be made orthogonal.</param>
        /// <param name="isNormalized">'normalized' flag determines if the axes are normalized or not.</param>
        /// <returns></returns>
        public static CoordinateSystem ByThreePoints(Point origin, Point xPoint, Point yPoint, bool isSheared, bool isNormalized)
        {
            if (origin == null)
                throw new System.ArgumentNullException("origin");
            else if (xPoint == null)
                throw new System.ArgumentNullException("xPoint");
            else if (yPoint == null)
                throw new System.ArgumentNullException("yPoint");

            var xAxis = origin.DirectionTo(xPoint);
            var yAxis = origin.DirectionTo(yPoint);
            var zAxis = xAxis.Cross(yAxis);

            var coordSys = ByOriginVectors(origin, xAxis, yAxis, zAxis, isSheared, isNormalized);
            if (null == coordSys)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "CoordinateSystem.ByThreePoints"));

            coordSys.XPoint = xPoint;
            coordSys.YPoint = yPoint;

            return coordSys;
        }

        /// <summary>
        /// Constructs a CoordinateSystem where the x-axis is tangential to the cylindrical surface and is parallel to 
        /// the base-plane of the cylinder. The y-axis is parallel to the axis of the cylinder and z-axis is normal to 
        /// the cylindrical surface at the given point on the cylinder whose position is defined by the r, theta and h values. 
        /// </summary>
        /// <param name="contextCoordinateSystem">
        /// the parent coordinate system to be used to construct the coordinate system
        /// </param>
        /// <param name="radius"></param>
        /// <param name="theta"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static CoordinateSystem ByCylindricalCoordinates(CoordinateSystem contextCoordinateSystem, double radius, double theta, double height)
        {
            //  what can be the constraints on theta
            if (contextCoordinateSystem == null)
                throw new System.ArgumentNullException("contextCoordinateSystem");
            //else if (height.EqualsTo(0.0))
            //    throw new System.ArgumentException(string.Format(Properties.Resources.IsZero, "height"), "height");
            else if (radius.EqualsTo(0.0))
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZero, "radius"), "radius");

            using (var localCSEntity = HostFactory.Factory.CoordinateSystemByCylindricalCoordinates(WCS.CSEntity, radius, theta, height))
            {
                if (null == localCSEntity)
                    throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "CoordinateSystem.ByCylindricalCoordinates"));

                var cylCS = CreateCoordinateSystem(contextCoordinateSystem, localCSEntity, true);
                cylCS.Radius = radius;
                cylCS.Theta = theta;
                cylCS.Height = height;

                return cylCS;
            }
        }

        /// <summary>
        /// Constructs a CoordinateSystem where the x-axis of the CoordinateSystem is tangential to the sphere along the 
        /// latitudinal direction, the z-axis is normal to the spherical surface and the y-axis is along the longitudinal 
        /// direction at the point defined on the sphere by the r, theta, phi values.
        /// </summary>
        /// <param name="contextCoordinateSystem">
        /// the parent coordinate system to be used to construct the coordinate system
        /// </param>
        /// <param name="radius">
        /// the radius of the spherical surface
        /// </param>
        /// <param name="theta">
        /// the theta value of the spherical surface
        /// </param>
        /// <param name="phi">
        /// the phi value of the spherical surface
        /// </param>
        /// <returns>
        /// CoordinateSystem created in Spherical world
        /// </returns>
        public static CoordinateSystem BySphericalCoordinates(CoordinateSystem contextCoordinateSystem, double radius, double theta, double phi)
        {
            //  what can be the constraints on theta
            if (contextCoordinateSystem == null)
                throw new System.ArgumentNullException("contextCoordinateSystem ");
            else if (radius.EqualsTo(0.0))
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZero, "radius"), "radius");

            using (var localCSEntity = HostFactory.Factory.CoordinateSystemBySphericalCoordinates(WCS.CSEntity, radius, theta, phi))
            {
                if (null == localCSEntity)
                    throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "CoordinateSystem.BySphericalCoordinates"));

                var cylCS = CreateCoordinateSystem(contextCoordinateSystem, localCSEntity, true);
                cylCS.Radius = radius;
                cylCS.Theta = theta;
                cylCS.Phi = phi;

                return cylCS;
            }
        }

        #region Deprecated ByUniversalTransform

        /*
        /// <summary>
        /// Constructs a CoordinateSystem by transforming a parent CoordinateSystem, by a resultant transformation matrix of scaling [S], rotation [R] and transformation [T] matrices. 'translationSequence' = false implies [S][R][T] otherwise means [S][T][R].
        /// </summary>
        /// <param name="contextCoordinateSystem">the parent coordinate system to be used to construct the coordinate system</param>
        /// <param name="scaleFactors">the factor by which the parent coordinate system is to be scaled</param>
        /// <param name="rotationAngles">the rotation angle to be applied to the parent coordinate system</param>
        /// <param name="rotationSequence">the rotation sequence to be applied to the parent coordinate system</param>
        /// <param name="translationVector">the translation vector to be applied to the parent coordinate system in the form of array of doubles</param>
        /// <param name="translationSequence">the translation sequence to be applied to the parent coordinate system</param>
        /// <returns></returns>
        public static CoordinateSystem ByUniversalTransform(CoordinateSystem contextCoordinateSystem,
            double[] scaleFactors,
            double[] rotationAngles,
            int[] rotationSequence,
            double[] translationVector,
            bool translationSequence)
        {
            if (contextCoordinateSystem == null)
                throw new System.ArgumentNullException("contextCoordinateSystem");
            else if (scaleFactors == null)
                throw new System.ArgumentNullException("scaleFactors");
            else if (rotationAngles == null)
                throw new System.ArgumentNullException("rotationAngles");
            else if (rotationSequence == null)
                throw new System.ArgumentNullException("rotationSequence");
            else if (translationVector == null)
                throw new System.ArgumentNullException("translationVector");
            else if (scaleFactors.Length < 3)
                throw new System.ArgumentException(string.Format(Properties.Resources.LessThan, "number of scale factors", "three"), "scaleFactors");
            else if (rotationAngles.Length < 3)
                throw new System.ArgumentException(string.Format(Properties.Resources.LessThan, "number of rotation angles", "three"), "rotationAngles");
            else if (rotationSequence.Length < 3)
                throw new System.ArgumentException(string.Format(Properties.Resources.LessThan, "number of rotation sequences", "three"), "rotationSequence");
            else if (translationVector.Length < 3)
                throw new System.ArgumentException(string.Format(Properties.Resources.LessThan, "number of  translation vectors", "three"), "translationVector");

            using (var localCSEntity = HostFactory.Factory.CoordinateSystemByUniversalTransform(WCS.CSEntity, scaleFactors,
                                            rotationAngles, rotationSequence, translationVector, translationSequence))
            {
                if (null == localCSEntity)
                    throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "CoordinateSystem.ByUniversalTransform"));

                var cs = CreateCoordinateSystem(contextCoordinateSystem, localCSEntity, true);
                cs.ScaleFactors = scaleFactors;
                cs.RotationAngles = rotationAngles;
                cs.RotationSequence = rotationSequence;
                cs.TranslationVector = new Vector(translationVector[0], translationVector[1], translationVector[2]);
                cs.TranslationSequence = translationSequence;

                return cs;
            }
        }

        /// <summary>
        /// Constructs a CoordinateSystem by transforming a parent CoordinateSystem, by a resultant transformation matrix of scaling [S], rotation [R] and transformation [T] matrices in the order [S][R][T].
        /// </summary>
        /// <param name="contextCoordinateSystem">the parent coordinate system to be used to construct the coordinate system</param>
        /// <param name="scaleFactors">the factor by which the parent coordinate system is to be scaled</param>
        /// <param name="rotationAngles">the rotation angle to be applied to the parent coordinate system</param>
        /// <param name="rotationSequence">the rotation sequence to be applied to the parent coordinate system</param>
        /// <param name="translationVector">the translation vector to be applied to the parent coordinate system</param>
        /// <returns></returns>
        public static CoordinateSystem ByUniversalTransform(CoordinateSystem contextCoordinateSystem,
            double[] scaleFactors,
            double[] rotationAngles,
            int[] rotationSequence,
            double[] translationVector)
        {
            return ByUniversalTransform(contextCoordinateSystem, scaleFactors,
                                            rotationAngles, rotationSequence, translationVector, false);
        }

        /// <summary>
        /// Constructs a CoordinateSystem by transforming a parent CoordinateSystem, by a resultant transformation matrix of scaling [S], rotation [R] and transformation [T] matrices. 'translationSequence' = false implies [S][R][T] otherwise means [S][T][R].
        /// </summary>
        /// <param name="contextCoordinateSystem">the parent coordinate system to be used to construct the coordinate system</param>
        /// <param name="scaleFactors">the factor by which the parent coordinate system is to be scaled</param>
        /// <param name="rotationAngles">the rotation angle to be applied to the parent coordinate system</param>
        /// <param name="rotationSequence">the rotation sequence to be applied to the parent coordinate system</param>
        /// <param name="translationVector">the translation vector to be applied to the parent coordinate system</param>
        /// <param name="translationSequence">the translation sequence to be applied to the parent coordinate system</param>
        /// <returns></returns>
        public static CoordinateSystem ByUniversalTransform(CoordinateSystem contextCoordinateSystem, double[] scaleFactors, double[] rotationAngles,
           int[] rotationSequence, Vector translationVector, bool translationSequence)
        {
            if (contextCoordinateSystem == null)
                throw new System.ArgumentNullException("contextCoordinateSystem");
            else if (scaleFactors == null)
                throw new System.ArgumentNullException("scaleFactors");
            else if (rotationAngles == null)
                throw new System.ArgumentNullException("rotationAngles");
            else if (rotationSequence == null)
                throw new System.ArgumentNullException("rotationSequence");
            else if (translationVector == null)
                throw new System.ArgumentNullException("translationVector");
            else if (scaleFactors.Length < 3)
                throw new System.ArgumentException(string.Format(Properties.Resources.LessThan, "number of scale factors", "three"), "scaleFactors");
            else if (rotationAngles.Length < 3)
                throw new System.ArgumentException(string.Format(Properties.Resources.LessThan, "number of rotation angles", "three"), "rotationAngles");
            else if (rotationSequence.Length < 3)
                throw new System.ArgumentException(string.Format(Properties.Resources.LessThan, "number of rotation sequences", "three"), "rotationSequence");

            using (ICoordinateSystemEntity localCSEntity = HostFactory.Factory.CoordinateSystemByUniversalTransform(contextCoordinateSystem.CSEntity, scaleFactors,
                                            rotationAngles, rotationSequence, translationVector.VectorEntity, translationSequence))
            {
                if (null == localCSEntity)
                    throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "CoordinateSystem.ByUniversalTransform"));

                var cs = CreateCoordinateSystem(contextCoordinateSystem, localCSEntity, true);
                cs.ScaleFactors = scaleFactors;
                cs.RotationAngles = rotationAngles;
                cs.RotationSequence = rotationSequence;
                cs.TranslationVector = translationVector;
                cs.TranslationSequence = translationSequence;

                return cs;
            }
        }

        */
        #endregion

        /// <summary>
        /// Constructs a a CoordinateSystem with its origin at the given parameter on the given curve. Its x-axis is tangential to the curve, 
        /// y-axis is along the normal and z-axis is along bi-normal at that point. This method is complementary to the 
        /// CoordinateSystemAtParameterAlongCurve() method in Curve class and has consistent behavior.
        /// </summary>
        /// <param name="contextCurve"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static CoordinateSystem AtParameter(Curve contextCurve, double t)
        {
            if (contextCurve == null)
                throw new System.ArgumentNullException("contextCurve");

            var cs = contextCurve.CoordinateSystemAtParameterCore(t, null);
            if (null == cs)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "CoordinateSystem.AtParameter"));

            cs.ContextCurve = contextCurve;
            cs.T = t;
            cs.Distance = contextCurve.DistanceAtParameter(t);

            return cs;
        }

        /// <summary>
        /// Constructor that does the same as CoordinateSystem.AtParameter but that it checks if the z-axis is
        /// in the same general direction as the given upVector. If not, it flips the z-axis to make it point 
        /// in the same overall direction as the upVector and also flips the y-axis accordingly to preserve 
        /// the right-handed CoordinateSystem rule.
        /// </summary>
        /// <param name="contextCurve"></param>
        /// <param name="t"></param>
        /// <param name="upVector"></param>
        /// <returns></returns>
        public static CoordinateSystem AtParameter(Curve contextCurve, double t, Vector upVector)
        {
            if (contextCurve == null)
                throw new System.ArgumentNullException("contextCurve");
            else if (upVector == null)
                throw new System.ArgumentNullException("upVector");
            else if (upVector.IsZeroVector())
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZeroVector, "up vector"), "upVector");
            
            var cs = contextCurve.CoordinateSystemAtParameterCore(t, upVector);
            if (null == cs)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "CoordinateSystem.AtParameter"));

            //  property assigments
            //
            cs.ContextCurve = contextCurve;
            cs.T = t;
            cs.Distance = contextCurve.DistanceAtParameter(t);
            cs.UpVector = upVector;

            return cs;
        }

        /// <summary>
        /// Constructs a CoordinateSystem at the given u, v parameters on the given surface. The x-axis of the CoordinateSystem 
        /// is aligned with the u-directional tangent, the y-axis with the v-directional tangent and the z-axis is mutually 
        /// perpendicular to the x and y axes.
        /// </summary>
        /// <param name="contextSurface"></param>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static CoordinateSystem AtParameter(Surface contextSurface, double u, double v)
        {
            return AtParameterCore(contextSurface, u, v, true);//make sure it's visible
        }

        internal static CoordinateSystem AtParameterCore(Surface contextSurface, double u, double v, bool visible)
        {
            if (contextSurface == null)
                throw new System.ArgumentNullException("contextSurface");

            var cs = contextSurface.GetCSAtParameters(u, v);
            if (null == cs)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "CoordinateSystem.AtParameter"));

            cs.ContextSurface = contextSurface;
            cs.U = u;
            cs.V = v;
            cs.Visible = visible; 
            return cs;
        }

        /// <summary>
        /// Contructs a coordinate system using a one dimension array representing a transform matrix
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected static CoordinateSystem ByData(double[] data)
        {
            if (data == null)
                throw new System.ArgumentNullException("data");
            else if (data.Length != 16)
                throw new System.ArgumentException(string.Format(Properties.Resources.NotEqual, "number of datas", 16), "data");

            ICoordinateSystemEntity cs = HostFactory.Factory.CoordinateSystemByMatrix(data);
            if (null == cs)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "CoordinateSystem.ByData"));

            return new CoordinateSystem(cs, true);
        }

        /// <summary>
        /// Inverts the transformation matrix of the current CoordinateSystem and returns a CoordinateSystem
        /// </summary>
        /// <returns>Returns inverted CoordinateSystem</returns>
        public CoordinateSystem Inverse()
        {
            ICoordinateSystemEntity invertedMat = CSEntity.Inverse();
            if (null == invertedMat)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "CoordinateSystem.Inverse"));

            var cs = new CoordinateSystem(invertedMat, true);
            return cs;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public CoordinateSystem Multiply(CoordinateSystem other)
        {
            if (other == null)
                throw new System.ArgumentNullException("other");

            ICoordinateSystemEntity resultantMat = CSEntity.PostMultiplyBy(other.CSEntity);
            if (resultantMat == null)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "CoordinateSystem.Multiply"));

            return new CoordinateSystem(resultantMat, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rightSide"></param>
        /// <returns></returns>
        public CoordinateSystem PostMultiplyBy(CoordinateSystem rightSide)
        {
            if (rightSide == null)
                throw new System.ArgumentNullException("rightSide");

            var resultantMat = CSEntity.PostMultiplyBy(rightSide.CSEntity);
            if (null == resultantMat)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "CoordinateSystem.PostMultiplyBy"));

            return new CoordinateSystem(resultantMat, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="leftSide"></param>
        /// <returns></returns>
        public CoordinateSystem PreMultiplyBy(CoordinateSystem leftSide)
        {
            if (leftSide == null)
                throw new System.ArgumentNullException("leftSide");

            ICoordinateSystemEntity resultantMat = CSEntity.PreMultiplyBy(leftSide.CSEntity);
            if (null == resultantMat)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "CoordinateSystem.PreMultiplyBy"));

            return new CoordinateSystem(resultantMat, true);
        }
        
        #endregion

        #region DESIGNSCRIPT_METHODS    

        /// <summary>
        /// Rotates the CoordinateSystem by the given angle about the given rotation axis and input CoordinateSystem's origin
        /// </summary>
        /// <param name="rotationAngle">The angle to be rotated through</param>
        /// <param name="axis">The axis to be rotated about</param>
        /// <returns>Returns a rotated CoordinateSystem</returns>
        public CoordinateSystem Rotate(double rotationAngle, Vector axis)
        {
            return this.Rotate(rotationAngle, axis, Origin);
        }

        /// <summary>
        /// Rotates the input CoordinateSystem about its own axes and about the global origin (0,0,0) by the given rotation angles 
        /// in the given rotation sequence. The rotation angles are always specified in the order of rotation about (xAxis, yAxis, zAxis).
        /// </summary>
        /// <param name="rotationAngle">The angle to be rotated through</param>
        /// <param name="axis">The axis to be rotated about</param>
        /// <param name="origin">The global origin</param>
        /// <returns>Returns a rotated CoordinateSystem</returns>
        public CoordinateSystem Rotate(double rotationAngle, Vector axis, Point origin)
        {
            if (axis == null)
                throw new System.ArgumentNullException("axis");
            else if (origin == null)
                throw new System.ArgumentNullException("origin");
            else if (axis.IsZeroVector())
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZeroVector, "axis"), "axis");

            ICoordinateSystemEntity rotatedCSEntity = this.CSEntity.Clone();
            rotatedCSEntity.Rotate(Origin.PointEntity, axis.VectorEntity, rotationAngle);
            return new CoordinateSystem(rotatedCSEntity, true);
        }

        /// <summary>
        /// Rotates the input CoordinateSystem about its own axes and about the global origin (0,0,0) by the given rotation angles 
        /// in the given rotation sequence. The rotation angles are always specified in the order of rotation about (xAxis, yAxis, zAxis).
        /// </summary>
        /// <param name="rotationAngles">the rotation angle to be applied to the parent coordinate system</param>
        /// <param name="rotationSequence">the rotation sequence to be applied to the parent coordinate system</param>
        /// <param name="origin">the origin of the new coordinate system</param>
        /// <returns></returns>
        public CoordinateSystem Rotate(double[] rotationAngles, int[] rotationSequence, Point origin)
        {
            if (rotationAngles == null)
                throw new System.ArgumentNullException("rotationAngles");
            else if (rotationSequence == null)
                throw new System.ArgumentNullException("rotationSequence");

            // apply rotation in order
            ICoordinateSystemEntity rotatedCSEntity = this.CSEntity.Clone();

            for (var i = 0; i < rotationSequence.Length; i++)
            {
                switch (rotationSequence[i])
                {
                    case 0:
                        rotatedCSEntity.Rotate(origin.PointEntity, this.XAxis.VectorEntity, rotationAngles[i]);
                        continue;
                    case 1:
                        rotatedCSEntity.Rotate(origin.PointEntity, this.YAxis.VectorEntity, rotationAngles[i]);
                        continue;
                    case 2:
                        rotatedCSEntity.Rotate(origin.PointEntity, this.ZAxis.VectorEntity, rotationAngles[i]);
                        continue;
                    // by default, perform no rotation
                }
            }

            return new CoordinateSystem(rotatedCSEntity, true);
        }

        /// <summary>
        /// Scales the given CoordinateSystem by a uniform scaling factor 
        /// </summary>
        /// <remarks>
        /// ScaleFactor cannot be equal to 0.0, with-in tolerance
        /// </remarks>
        /// <param name="scaleFactor">the scale factor</param>
        /// <returns>Returns a scaled CoordinateSystem</returns>
        public CoordinateSystem Scale(double scaleFactor)
        {
            if (GeometryExtension.Equals(scaleFactor, 0.0))
            {
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZero, "scale factor"), "scaleFactor");
            }

            return Scale(scaleFactor, scaleFactor, scaleFactor);
        }

        /// <summary>
        /// Scales the given CoordinateSystem by a non-uniform scaling factor along each axis
        /// </summary>
        /// <param name="scaleX"></param>
        /// <param name="scaleY"></param>
        /// <param name="scaleZ"></param>
        /// <returns></returns>
        public CoordinateSystem Scale(double scaleX, double scaleY, double scaleZ)
        {
            if (GeometryExtension.Equals(scaleX, 0.0))
            {
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZero, "scale x"), "scaleX");
            }
            else if (GeometryExtension.Equals(scaleY, 0.0))
            {
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZero, "scale y"), "scaleY");
            }
            else if (GeometryExtension.Equals(scaleZ, 0.0))
            {
                throw new System.ArgumentException(string.Format(Properties.Resources.IsZero, "scale z"), "scaleZ");
            }

            ICoordinateSystemEntity clone = this.CSEntity.Clone();
            clone.Scale(scaleX, scaleY, scaleZ);
            var cs = new CoordinateSystem(clone, true);

            return cs;
        }

        /// <summary>
        /// Translates the coordinate system in the direction of the vector and by the distance specified
        /// </summary>
        /// <param name="translationVector">The direction of translation</param>
        /// <param name="distance">The distance of translation</param>
        /// <returns></returns>
        public CoordinateSystem Translate(Vector translationVector, double distance)
        {
            if (translationVector == null)
            {
                throw new System.ArgumentNullException("translationVector");
            }
            translationVector = translationVector.Normalize().MultiplyBy(distance);

            ICoordinateSystemEntity clone = this.CSEntity.Clone();
            clone.Translate(translationVector.VectorEntity);
            var cs = new CoordinateSystem(clone, true);

            return cs;
        }

        /// <summary>
        /// Translates the coordinate system by the coordinates specified
        /// </summary>
        /// <param name="xTranslation">Translation in the x direction</param>
        /// <param name="yTranslation">Translation in the y direction</param>
        /// <param name="zTranslation">Translation in the z direction</param>
        /// <returns></returns>
        public CoordinateSystem Translate(double xTranslation, double yTranslation, double zTranslation)
        {
            Vector direction = new Vector(xTranslation, yTranslation, zTranslation);
            return Translate(direction, direction.GetLength());
        }

        #endregion

        #region PROPERTIES

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsSingular()
        {
            return CSEntity.IsSingular;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsScaledOrtho()
        {
            return CSEntity.IsScaledOrtho;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsUniscaledOrtho()
        {
            return CSEntity.IsUniscaledOrtho;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public double GetDeterminant()
        {
            return CSEntity.Determinant;
        }

        /// <summary>
        /// 
        /// </summary>
        [Category("Primary")]
        public Point Origin
        { 
            get 
            {
                if(null == mOrigin)
                    mOrigin = CSEntity.Origin.ToPoint(false, null);
                return mOrigin; 
            } 
        }

        /// <summary>
        /// 
        /// </summary>
        [Category("Primary")]
        public Vector XAxis { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        [Category("Primary")]
        public Vector YAxis { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        [Category("Primary")]
        public Vector ZAxis { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public double X { get { return Origin.X; } }

        /// <summary>
        /// 
        /// </summary>
        public double Y { get { return Origin.Y; } }

        /// <summary>
        /// 
        /// </summary>
        public double Z { get { return Origin.Z; } }

        /// <summary>
        /// 
        /// </summary>
        public bool IsNormalized
        {
            get
            {
                if (!isNormalized.HasValue)
                {
                    isNormalized = XAxis.IsNormalized && YAxis.IsNormalized && ZAxis.IsNormalized;
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
        public bool IsSheared 
        {
            get { return !CSEntity.IsScaledOrtho; }
        }

        /// <summary>
        /// 
        /// </summary>
        public CoordinateSystem ContextCoordinateSystem { get; private set; }

        /// <summary>
        /// Gets the XYPlane of this coordinate system
        /// </summary>
        public Plane XYPlane
        {
            get { return new Plane(this.Origin, this.ZAxis, Plane.kDefaultSize); }
        }

        /// <summary>
        /// Gets the YZPlane of this coordinate system
        /// </summary>
        public Plane YZPlane
        {
            get { return new Plane(this.Origin, this.XAxis, Plane.kDefaultSize); }
        }

        /// <summary>
        /// Gets the XYPlane of this coordinate system
        /// </summary>
        public Plane ZXPlane
        {
            get { return new Plane(this.Origin, this.YAxis, Plane.kDefaultSize); }
        }

        /// <summary>
        /// 
        /// </summary>
        public double XTranslation
        {
            get
            {
                if (!xTranslation.HasValue)
                {
                    xTranslation = CSEntity.Origin.X;
                }
                return xTranslation.Value;
            }
            private set
            {
                xTranslation = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double YTranslation
        {
            get
            {
                if (!yTranslation.HasValue)
                {
                    yTranslation = CSEntity.Origin.Y;
                }
                return yTranslation.Value;
            }
            private set
            {
                yTranslation = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double ZTranslation
        {
            get
            {
                if (!zTranslation.HasValue)
                {
                    zTranslation = CSEntity.Origin.Z;
                }
                return zTranslation.Value;
            }
            private set
            {
                zTranslation = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Vector LocalXAxis
        {
            get
            {
                if (localXAxis == null)
                {
                    localXAxis = new Vector(CSEntity.XAxis);
                }
                return localXAxis;
            }

            private set
            {
                localXAxis = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Vector LocalYAxis
        {
            get
            {
                if (localYAxis == null)
                {
                    localYAxis = new Vector(CSEntity.YAxis);
                }
                return localYAxis;
            }

            private set
            {
                localYAxis = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Vector LocalZAxis
        {
            get
            {
                if (localZAxis == null)
                {
                    localZAxis = new Vector(CSEntity.ZAxis);
                }
                return localZAxis;
            }

            private set
            {
                localZAxis = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double[] ScaleFactors { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public Vector TranslationVector { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public int[] RotationSequence { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public double[] RotationAngles { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public bool? TranslationSequence { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public double? Radius { get; private set; }
        
        /// <summary>
        /// 
        /// </summary>
        public double? Theta { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public double? Phi { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public double? Height { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public Point XPoint
        {
            get { return mXPoint; }
            private set { value.AssignTo(ref mXPoint); }
        }

        /// <summary>
        /// 
        /// </summary>
        public Point YPoint
        {
            get { return mYPoint; }
            private set { value.AssignTo(ref mYPoint); }
        }

        /// <summary>
        /// 
        /// </summary>
        public double? U { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public double? V { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public double? Distance { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public double? T { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        public Curve ContextCurve
        {
            get { return mContextCurve; }
            private set { value.AssignTo(ref mContextCurve); }
        }

        /// <summary>
        /// 
        /// </summary>
        public Surface ContextSurface
        {
            get { return mContextSurface; }
            private set { value.AssignTo(ref mContextSurface); }
        }

        /// <summary>
        /// 
        /// </summary>
        public Vector UpVector { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public static CoordinateSystem WCS
        {
            get 
            {
                //@TODO : Ideally the static field should be used here, but there is
                //a defect in language, that calls dispose on static property
                //and then it becomes unusable.
                //if (mWCS == null)
                //    mWCS = CoordinateSystem.Identity();

                return CoordinateSystem.Identity();
            }
        }

        internal IDisplayable Display { get; private set; }
        #endregion

        #region FROM_OBJECT

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected override bool Equals(DesignScriptEntity obj)
        {
            var otherCS = obj as CoordinateSystem;
            if( null == otherCS)
            {
                return false;
            }
            
            return CSEntity.IsEqualTo(otherCS.CSEntity);
        }

        public override string ToString()
        {
            var strRep = string.Format("CoordinateSystem(Origin={0}, XAxis={1}, YAxis={2}, ZAxis={3})", Origin, XAxis, YAxis, ZAxis);
            return strRep;
        }

        #endregion

        #region FROM_IDISPLAYATTRIBUTES

        public bool Highlight(bool visibility)
        {
            if (null == Display)
                return false;
            if (!Display.Visible && visibility)
            {
                visibilityForHighlight = true;
                Display.Visible = true;
            }
            if (Display.Visible && !visibility && visibilityForHighlight)
            {
                visibilityForHighlight = false;
                Display.Visible = false;
            }
            return true; 
        }
        private CoordinateSystem SetVisibilityCore(bool visible)
        {
            this.Visible = visible;
            return this;
        }
        public CoordinateSystem SetVisibility(bool visible)
        {
            return SetVisibilityCore(visible);
        }     
        public bool Visible
        {
            get 
            {
                if (null != Display)
                    return Display.Visible;

                throw new NotSupportedException("Host doesn't support Visible property.");
            }
            set
            {
                if (null != Display)
                    Display.SetVisibility(value);
            }
        }

        #endregion
    }
}
