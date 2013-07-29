using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;

namespace DSGeometry
{
    class BSplineCurveEntity : CurveEntity, IBSplineCurveEntity
    {
        internal BSplineCurveEntity()
        {
            this.ControlVertices = new PointEntity[2]{new PointEntity(), new PointEntity(1,1,1)};
            this.degree = 1;
            this.periodic = false;
        }
        internal BSplineCurveEntity(IPointEntity[] controlVertices, int degree, bool periodic)
        {
            this.ControlVertices = controlVertices;
            this.degree = degree;
            this.periodic = periodic;
        }

        public IPointEntity[] GetControlVertices()
        {
            return ControlVertices;
        }

        public int GetDegree()
        {
            return Degree;
        }

        public bool GetIsPeriodic()
        {
            return Periodic;
        }

        public bool GetIsRational()
        {
            return Rational;
        }

        public bool GetIsClosed()
        {
            return IsClosed;
        }

        public double[] GetKnots()
        {
            return Knots;
        }

        public double[] GetWeights()
        {
            return Weights;
        }

        public override bool IsClosed
        {
            get { return false; }
        }

        public override bool IsPlanar
        {
            get { return false; }
        }
        private int degree;
        public int Degree
        {
            get { return degree; }
            protected set { degree = value; }
        }
        private bool periodic;
        public bool Periodic
        {
            get { return periodic; }
            protected set { periodic = value; }
        }
        private bool rational;
        public bool Rational
        {
            get { return rational; }
            protected set { rational = value; }
        }

        private double[] knots;
        public double[] Knots
        {
            get { return knots; }
            protected set { knots = value; }
        }

        private double[] weights;
        public double[] Weights
        {
            get { return weights; }
            protected set { weights = value; }
        }

        private IPointEntity[] controlVertices;
        public IPointEntity[] ControlVertices
        {
            get { return controlVertices; }
            protected set { controlVertices = value; }
        }
    }
}
