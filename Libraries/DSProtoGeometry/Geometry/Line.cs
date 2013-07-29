using System;
using Autodesk.DesignScript.Interfaces;
using System.Text;

namespace Autodesk.DesignScript.Geometry
{
    public class DSLine : DSCurve
    {
        internal ILineEntity LineEntity { get { return HostImpl as ILineEntity; } }

        #region INTERNAL_METHODS

        private void InitializeGuaranteedProperties()
        {
            Direction = StartPoint.DirectionTo(EndPoint).Normalize();
        }

        internal override IPointEntity[] GetSamplePoints(int maxSamples)
        {
            return new IPointEntity[] { LineEntity.StartPoint, LineEntity.EndPoint };
        }

        #endregion

        #region PRIVATE_CONSTRUCTORS

        static void InitType()
        {
            RegisterHostType(typeof(ILineEntity), (IGeometryEntity host, bool persist) => { return new DSLine(host as ILineEntity, persist); });
        }

        private DSLine(ILineEntity entity, bool persist = false)
            : base(entity, persist)
        {
            InitializeGuaranteedProperties();
        }
        #endregion

        #region PROTECTED_CONSTRUCTORS

        protected internal DSLine(DSPoint startPt, DSPoint endPt, bool persist)
            : base(ByStartPointEndPointCore(startPt, endPt),persist)
        {
            InitializeGuaranteedProperties();
        }

        protected DSLine(DSPoint startPt, DSVector direction, double length,bool persist)
            : base(ByStartPointDirectionLengthCore(startPt, direction, length),persist)
        {
            InitializeGuaranteedProperties();
        }

        #endregion

        #region PROPERTIES
        /// <summary>
        /// 
        /// </summary>
        public DSVector Direction { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public override double Length
        {
            get 
            {
                return StartPoint.DistanceTo(EndPoint);
            }
        }

        #endregion 

        #region CURVE_OVERRIDES

        /// <summary>
        /// 
        /// </summary>
        public override bool IsCircular
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool IsElliptical
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool IsLinear
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool IsPlanar
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool IsSelfIntersecting
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool IsClosed
        {
            get
            {
                return false;
            }
        }

        protected override DSPoint GetEndPoint()
        {
            return LineEntity.EndPoint.ToPoint(false, this);
        }

        protected override DSPoint GetStartPoint()
        {
            return LineEntity.StartPoint.ToPoint(false, this);
        }

        #endregion

        #region DESIGNSCRIPT_CONSTRUCTORS

        /// <summary>
        /// constructs a Line joining the start point and end point as inputs.
        /// </summary>
        /// <param name="startPt">The start point of the line</param>
        /// <param name="endPoint">The end point of the line</param>
        /// <returns></returns>
        public static DSLine ByStartPointEndPoint(DSPoint startPoint, DSPoint endPoint)
        {
            return new DSLine(startPoint, endPoint, true);
        }
        
        /// <summary>
        /// Constructs a Line from the start point in the direction of the 'direction' vector and having a given input length
        /// </summary>
        /// <param name="startPoint">The start point of the line</param>
        /// <param name="direction">The direction vector of the line</param>
        /// <param name="length">The length of the line</param>
        /// <returns></returns>
        public static DSLine ByStartPointDirectionLength(DSPoint startPoint, DSVector direction, double length)
        {
            return new DSLine(startPoint, direction, length, true);
        }
           
        #endregion

        #region CORE_METHODS

        private static ILineEntity ByStartPointEndPointCore(DSPoint startPt, DSPoint endPt)
        {
            if (startPt == null)
            {
                throw new ArgumentNullException("startPt");
            }
            else if (endPt == null)
            {
                throw new ArgumentNullException("endPt");
            }
            else if (startPt.Equals(endPt))
            {
                throw new ArgumentException(string.Format(Properties.Resources.EqualGeometry, "start point", "end point"));
            }
            ILineEntity entity = HostFactory.Factory.LineByStartPointEndPoint(startPt.PointEntity, endPt.PointEntity);
            if (null == entity)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "DSLine.ByStartPointEndPoint"));
            return entity;
        }

        private static ILineEntity ByStartPointDirectionLengthCore(DSPoint startPt, DSVector direction, double length)
        {
            if (startPt == null)
            {
                throw new ArgumentNullException("startPt");
            }
            else if (direction == null)
            {
                throw new ArgumentNullException("direction");
            }
            else if (direction.IsZeroVector() || length == 0.0)
            {
                throw new ArgumentException(string.Format(Properties.Resources.IsZeroVector, "direction"));
            }
            else if (length == 0.0)
            {
                throw new ArgumentException(Properties.Resources.IsZeroLength);
            }
            //Temporary end point is created by translating, should be disposed
            DSVector offset = direction.Normalize().MultiplyBy(length);
            var endPt = startPt.Translate(offset, false);
            ILineEntity entity = HostFactory.Factory.LineByStartPointEndPoint(startPt.PointEntity, endPt.PointEntity);
            if (null == entity)
                throw new System.Exception(string.Format(Properties.Resources.OperationFailed, "DSLine.ByStartPointDirectionLength"));

            return entity;
        }

        #endregion

        #region DESIGNSCRIPT_METHODS

        /// <summary>
        /// Checks whether input lines are parallel.
        /// </summary>
        /// <param name="otherLine"></param>
        /// <returns></returns>
        public bool IsParallel(DSLine otherLine)
        {
            if (null == otherLine)
                return false;

            return Direction.IsParallel(otherLine.Direction);
        }

        /// <summary>
        /// Checks whether input lines are colinear
        /// </summary>
        /// <param name="otherLine"></param>
        /// <returns></returns>
        public bool IsColinear(DSLine otherLine)
        {
            if (!IsParallel(otherLine))
                return false;

            IPointEntity[] endpts = { StartPoint.PointEntity, EndPoint.PointEntity, otherLine.StartPoint.PointEntity};
            return endpts.ArePointsColinear();
        }

        #endregion

        #region FROM_OBJECT

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("DSLine( StartPoint = {0}, EndPoint = {1}, Length= {2})",
                StartPoint, EndPoint, Length.ToString(DSGeometryExtension.DoublePrintFormat));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected override bool Equals(DesignScriptEntity obj)
        {
            if (base.Equals(obj))
            {
                return true;
            }

            var otherLine = obj as DSLine;
            if (otherLine == null)
            {
                return false;
            }

            return StartPoint.Equals(otherLine.StartPoint) && EndPoint.Equals(otherLine.EndPoint);
        }

        #endregion
    }
}
