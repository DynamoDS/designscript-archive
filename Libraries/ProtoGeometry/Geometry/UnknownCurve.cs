using System;
using Autodesk.DesignScript.Interfaces;

namespace Autodesk.DesignScript.Geometry
{
    class UnknownCurve : Curve
    {
        static void InitType()
        {
            RegisterHostType(typeof(ICurveEntity), (IGeometryEntity host, bool persist) => { return new UnknownCurve(host as ICurveEntity, persist); });
        }

        private UnknownCurve(ICurveEntity host, bool persist = false)
            : base(host, persist)
        {
        }

        protected override Point GetStartPoint()
        {
            return CurveEntity.StartPoint.ToPoint(false, this);
        }

        protected override Point GetEndPoint()
        {
            return CurveEntity.EndPoint.ToPoint(false, this);
        }

        public override bool IsLinear
        {
            get
            {
                if (CurveEntity is ILineEntity)
                    return true;
                return false;
            }
        }

        public override bool IsCircular
        {
            get
            {
                if (CurveEntity is Circle)
                    return true;
                return false;
            }
        }

        public override bool IsPlanar
        {
            get
            {
                return CurveEntity.IsPlanar;
            }
        }

        public override bool IsElliptical
        {
            get
            {
                return false; //No support for elliptical curve.
            }
        }

        public override bool IsSelfIntersecting
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool IsClosed
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
