using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Autodesk.DesignScript.Geometry
{
    public class BSplineCurve : Curve
    {
        internal IBSplineCurveHost BSplineHost { get { return HostImpl as IBSplineCurveHost; } }

        #region CURVE_OVERRIDES

        bool EvaluteISClosed()
        {
            var result = this.IsPeriodic || this.StartPoint == this.EndPoint;
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool IsClosed
        {
            get
            {
                return EvaluteISClosed();
            }
            protected set
            {
                throw new NotImplementedException();
            }
        }

        bool EvaluteIsCircular()
        {
            /*
            var dbEntity = Id.Open(OpenMode.ForRead) as Autodesk.AutoCAD.DatabaseServices.Spline;
            Debug.Assert(dbEntity != null);
            return dbEntity.g*/
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
            protected set
            {
                throw new NotImplementedException();
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
            protected set
            {
                throw new NotImplementedException();
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
            protected set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool IsPlanar
        {
            get
            {
                return CurveHost.IsPlanar;
            }
            protected set
            {
                throw new NotImplementedException();
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
            protected set
            {
                throw new NotImplementedException();
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

        protected override Point GetEndPoint()
        {
            IPointHost result = CurveHost.EndPoint;
            return new Point(result);            
        }

        protected override Point GetStartPoint()
        {
            IPointHost result = CurveHost.StartPoint;
            return new Point(result);
        }

        #endregion

        #region PROPERTIES

        /// <summary>
        /// 
        /// </summary>
        public Point[] ControlVertices { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public double[] Knots { get; private set;}

        /// <summary>
        /// 
        /// </summary>
        public int Degree { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public Point[] Points { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public Vector StartTangent { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public Vector EndTangent { get; private set; }

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
        public bool IsPeriodic
        {
            get;
            private set;
        }

        #endregion

        #region PRIVATE_CONSTRUCTORS

        internal BSplineCurve(IBSplineCurveHost host, bool persist = false)
            : base(host, persist)
        {
        }

        #endregion

        #region DESIGNSCRIPT_CONSTRUCTORS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="controlVertices"></param>
        /// <param name="degree"></param>
        /// <returns></returns>
        public static BSplineCurve ByControlVertices(Point[] controlVertices, int degree )
        {
            if (controlVertices == null || degree < 3)
            {
                return null;
            }
            else if (controlVertices.Length < degree+1)
            {
                return null;
            }

            var ent = HostFactory.Factory.BSplineByControlVertices(controlVertices.ToHostArray(), degree, false);
            BSplineCurve spline = new BSplineCurve(ent, true);
            spline.ControlVertices = controlVertices;
            spline.Degree = degree;

            return spline;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="controlVertices"></param>
        /// <param name="degree"></param>
        /// <param name="makePeriodic"></param>
        /// <returns></returns>
        public static BSplineCurve ByControlVertices(Point[] controlVertices, int degree, bool makePeriodic)
        {
            if (controlVertices == null || degree < 3)
            {
                return null;
            }
            else if (controlVertices.Length < degree + 1)
            {
                return null;
            }

            var ent = HostFactory.Factory.BSplineByControlVertices(controlVertices.ToHostArray(), degree, makePeriodic);
            var spline = new BSplineCurve(ent, true);
            spline.ControlVertices = controlVertices;
            spline.Degree = degree;

            return spline;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts"></param>
        /// <returns></returns>
        public static BSplineCurve ByPoints(Point[] pts)
        {
            if (pts == null)
            {
                return null;
            }

            int degree = 3;
            var ent = HostFactory.Factory.BSplineByPoints(pts.ToHostArray(), false);
            var spline = new BSplineCurve(ent, true);
            spline.Points = pts;
            spline.Degree = degree;

            return spline;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts"></param>
        /// <param name="makePeriodic"></param>
        /// <returns></returns>
        public static BSplineCurve ByPoints(Point[] pts, bool makePeriodic)
        {
            if (pts == null)
            {
                return null;
            }

            var ent = HostFactory.Factory.BSplineByPoints(pts.ToHostArray(), makePeriodic);
            var spline = new BSplineCurve(ent, true);
            spline.Points = pts;
            spline.Degree = 3;
            spline.IsPeriodic = makePeriodic;

            return spline;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts"></param>
        /// <param name="startTangent"></param>
        /// <param name="endTangent"></param>
        /// <returns></returns>
        public static BSplineCurve ByPoints(Point[] pts, Vector startTangent, Vector endTangent)
        {
            if (pts == null || startTangent == null || endTangent == null || 
                startTangent.IsZeroVector() || endTangent.IsZeroVector())
            {
                return null;
            }

            IBSplineCurveHost ent = HostFactory.Factory.BSplineByPoints(pts.ToHostArray(), startTangent, endTangent);
            var spline = new BSplineCurve(ent, true);
            spline.Points = pts;
            spline.Degree = 3;
            spline.StartTangent = startTangent;
            spline.EndTangent = endTangent;

            return spline;
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
