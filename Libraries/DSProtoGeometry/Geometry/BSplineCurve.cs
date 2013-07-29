using System.ComponentModel;
using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    public class DSBSplineCurve : DSCurve
    {
        internal IBSplineCurveEntity BSplineEntity { get { return HostImpl as IBSplineCurveEntity; } }
        
        #region DATA MEMBERS
        private DSPoint[] mControlVertices;
        private DSVector mStartTangent = null;
        private DSVector mEndTangent = null;
        private DSPoint[] mPoints;
        #endregion

        #region OVERRIDES

        bool EvaluteISClosed()
        {
            var result = IsPeriodic || StartPoint.Equals(EndPoint);
            return result;
        }

        public override bool IsClosed
        {
            get
            {
                return EvaluteISClosed();
            }
        }

        bool EvaluteIsCircular()
        {
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool IsCircular
        {
            get
            {
                return EvaluteIsCircular();
            }
        }

        bool EvaluateIsElliptical()
        {
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool IsElliptical
        {
            get
            {
                return EvaluateIsElliptical();
            }
        }


        bool EvaluateIsLinear()
        {
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool IsLinear
        {
            get
            {
                return EvaluateIsLinear();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool IsPlanar
        {
            get
            {
                return CurveEntity.IsPlanar;
            }
        }

        bool EvaluateIsSelfIntersecting()
        {
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool IsSelfIntersecting
        {
            get
            {
                return EvaluateIsSelfIntersecting();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsRational
        {
            get;
            private set;
        }

        protected override DSPoint GetEndPoint()
        {
            IPointEntity result = CurveEntity.EndPoint;
            return result.ToPoint(false, this);
        }

        protected override DSPoint GetStartPoint()
        {
            IPointEntity result = CurveEntity.StartPoint;
            return result.ToPoint(false, this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DSGeometryExtension.DisposeObject(ref mControlVertices);
                DSGeometryExtension.DisposeObject(ref mPoints);
            }

            base.Dispose(disposing);
        }

        #endregion

        #region PROPERTIES

        /// <summary>
        /// 
        /// </summary>
        [Category("Primary")]
        public DSPoint[] ControlVertices 
        {
            get
            {
                if (null == mControlVertices)
                    mControlVertices = BSplineEntity.GetControlVertices().ToArray<DSPoint, IPointEntity>(false);

                return mControlVertices;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double[] Knots { get { return BSplineEntity.GetKnots(); } }

        /// <summary>
        /// 
        /// </summary>
        [Category("Primary")]
        public int Degree { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        [Category("Primary")]
        public DSPoint[] Points
        { 
            get { return mPoints; }
            private set { value.AssignTo(ref mPoints); }
        }

        /// <summary>
        /// 
        /// </summary>
        public DSVector StartTangent
        {
            get
            {
                if (mStartTangent == null)
                    mStartTangent = TangentAtParameter(0);
                return mStartTangent;
            }
            private set { mStartTangent = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public DSVector EndTangent
        {
            get
            {
                if (mEndTangent == null)
                    mEndTangent = TangentAtParameter(1);
                return mEndTangent;
            }
            private set { mEndTangent = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public int Order
        {
            get
            {
                return Degree + 1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Category("Primary")]
        public bool IsPeriodic
        {
            get;
            private set;
        }

        #endregion

        #region PRIVATE_CONSTRUCTORS

        static void InitType()
        {
            RegisterHostType(typeof(IBSplineCurveEntity), (IGeometryEntity host, bool persist) => { return new DSBSplineCurve(host as IBSplineCurveEntity, persist); });
        }

        private DSBSplineCurve(IBSplineCurveEntity host, bool persist = false)
            : base(host, persist)
        {
            InitializeGuaranteedProperties();
        }
        #endregion

        #region INTERNAL_METHODS

        private void InitializeGuaranteedProperties()
        {
            Degree = BSplineEntity.GetDegree();
            IsPeriodic = BSplineEntity.GetIsPeriodic();
        }

        #endregion

        #region PROTECTED_CONSTRUCTORS

        protected DSBSplineCurve(DSPoint[] points, DSVector startTangent, DSVector endTangent, bool makePeriodic,bool persist)
            : base(ByPointsCore(ref points, startTangent, endTangent, makePeriodic),persist)
        {
            InitializeGuaranteedProperties();
            Points = points;
            StartTangent = startTangent;
            EndTangent = endTangent;
        }

        protected DSBSplineCurve(DSPoint[] controlVertices, int degree, bool makePeriodic,bool persist)
            : base(ByControlVerticesCore(controlVertices, ref degree, makePeriodic), persist)
        {
            InitializeGuaranteedProperties();
        }

        #endregion

        #region DESIGNSCRIPT_CONSTRUCTORS

        /// <summary>
        /// Constructs a non-periodic BSplineCurve with given control vertices and degree.
        /// </summary>
        /// <param name="controlVertices">Array of points</param>
        /// <param name="degree">Degree of the curve</param>
        /// <returns>BSplineCurve</returns>
        public static DSBSplineCurve ByControlVertices(DSPoint[] controlVertices, int degree )
        {
            return ByControlVertices(controlVertices, degree, false);
        }

        /// <summary>
        /// Constructs a Bspline curve from a given list of input control 
        /// vertices and degree,  The 'makePeriodic' flag determines if the 
        /// curve should be closed with C1 continuity.
        /// </summary>
        /// <param name="controlVertices">Array of points</param>
        /// <param name="degree">Degree of the curve</param>
        /// <param name="makePeriodic">Flag to make it periodic</param>
        /// <returns>BSplineCurve</returns>
        public static DSBSplineCurve ByControlVertices(DSPoint[] controlVertices, int degree, bool makePeriodic)
        {
            return new DSBSplineCurve(controlVertices, degree, makePeriodic, true);
        }

        /// <summary>
        /// Constructs a non-periodic Bspline curve by fitting the given array 
        /// of points. The degree is 3. 
        /// </summary>
        /// <param name="points">Array of points</param>
        /// <returns>BSplineCurve</returns>
        public static DSBSplineCurve ByPoints(DSPoint[] points)
        {
            return ByPoints(points, false);
        }

        /// <summary>
        /// Constructs a Bspline curve from a given list of input points,  The 
        /// 'makePeriodic' flag determines if the curve should be closed with 
        /// C1 continuity. The degree is 3. 
        /// </summary>
        /// <param name="points">Array of points</param>
        /// <param name="makePeriodic">Flag to make it periodic</param>
        /// <returns>BSplineCurve</returns>
        public static DSBSplineCurve ByPoints(DSPoint[] points, bool makePeriodic)
        {
            return ByPoints(points, null, null, makePeriodic);
        }

        /// <summary>
        /// Constructs a Bspline curve interpolating the given set of input 
        /// points and tangent to the first and second tangent vectors at the 
        /// start and end points respectively with degree 3.
        /// </summary>
        /// <param name="points">Array of points</param>
        /// <param name="startTangent">Start tangent vector</param>
        /// <param name="endTangent">End tangent vector</param>
        /// <returns>BSplineCurve</returns>
        public static DSBSplineCurve ByPoints(DSPoint[] points, DSVector startTangent, DSVector endTangent)
        {
            if (startTangent == null || endTangent == null ||
                startTangent.IsZeroVector() || endTangent.IsZeroVector())
            {
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidArguments, "points"), "points");
            }

            return ByPoints(points, startTangent, endTangent, false);
        }

        /// <summary>
        /// Internal function to create BSplineCurve by fit points.
        /// </summary>
        /// <param name="points">Array of points</param>
        /// <param name="startTangent">Start tangent vector</param>
        /// <param name="endTangent">End tangent vector</param>
        /// <param name="makePeriodic">Flag to make it periodic</param>
        /// <returns>BSplineCurve</returns>
        private static DSBSplineCurve ByPoints(DSPoint[] points, DSVector startTangent, DSVector endTangent, bool makePeriodic)
        {
            return new DSBSplineCurve(points, startTangent, endTangent, makePeriodic, true);
        }

        #endregion

        #region CORE_METHODS

        private static IBSplineCurveEntity ByControlVerticesCore(DSPoint[] controlVertices, ref int degree, bool makePeriodic)
        {
            IPointEntity[] hosts = controlVertices.ConvertAll(DSGeometryExtension.ToEntity<DSPoint, IPointEntity>);
            if (hosts == null)
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidInput, "controlVertices", "DSBSplineCurve"), "controlVertices");

            if (degree < 1 || degree > 11)
            {
                throw new System.ArgumentException(string.Format(Properties.Resources.LessThan, "degree", "one"), "degree");
            }
            if (hosts.AreCoincident())
                throw new System.ArgumentException(string.Format(Properties.Resources.PointsCoincident, "controlVertices"), "controlVertices"); //Can't create BSpline curve with all coincident points.

            // making sure if # of controlVertices > degree by atleast 1
            int udiff = hosts.Length - degree;
            if (udiff < 1)
                degree += udiff - 1; //Fix DID DG-1464903

            var ent = HostFactory.Factory.BSplineByControlVertices(hosts, degree, makePeriodic);
            if (null == ent)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "DSBSplineCurve.ByControlVertices"));
            return ent;
        }

        private static IBSplineCurveEntity ByPointsCore(ref DSPoint[] points, DSVector startTangent, DSVector endTangent, bool makePeriodic)
        {
            IPointEntity[] hosts = points.ConvertAll(DSGeometryExtension.ToEntity<DSPoint, IPointEntity>);
            if (hosts == null || hosts.Length < 2)
            {
                throw new System.ArgumentException(string.Format(Properties.Resources.InvalidArguments, "points"), "points");
            }
            if (hosts.AreCoincident())
                throw new System.ArgumentException(string.Format(Properties.Resources.PointsCoincident, "points"), "points"); //Can't create BSpline curve with all coincident points.

            IBSplineCurveEntity ent = null;
            if (null != startTangent && null != endTangent)
                ent = HostFactory.Factory.BSplineByPoints(hosts, startTangent.IVector, endTangent.IVector);
            else
                ent = HostFactory.Factory.BSplineByPoints(hosts, makePeriodic);
            if (ent == null)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "DSBSplineCurve.ByPoints"));
            points = hosts.ToArray<DSPoint, IPointEntity>(false);
            return ent;
        }
        
        #endregion

        #region FROM_OBJECT


        //@TODO: implement Equals and GetHashCode
        //
        //  


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string nullStr = "null";

            var ctrlVertices = nullStr;
            if (ControlVertices != null)
            {
                var builder = new System.Text.StringBuilder();
                builder.Append("{");
                foreach (var pt in ControlVertices)
                {
                    builder.Append(pt.ToString());
                }
                builder.Append("}");
                ctrlVertices = builder.ToString();
            }

            var pts = nullStr;
            if (Points != null)
            {
                var builder = new System.Text.StringBuilder();
                builder.Append("{");
                foreach (var pt in Points)
                {
                    builder.Append(pt.ToString());
                }
                builder.Append("}");
                pts = builder.ToString();
            }

            return string.Format("BSpline(Degree = {0}, ControlVertices = {1}, Points = {2})", 
                                Degree, ctrlVertices , pts);
        }

        #endregion
    }
}
