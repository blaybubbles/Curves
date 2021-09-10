using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace CurveLib.Curves
{
    public enum SplineType
    {
        Centripetal,
        Chordal,
        Catmullrom
    }

    [Serializable]
    public class SplineCurve : BaseCurve
    {
        private Vector3[] points;
        private bool closed;
        private SplineType curveType;
        private float tension;

        public SplineCurve(Vector3[] points, bool closed = false, SplineType curveType = SplineType.Centripetal, float tension = 0.5f)
        {
            this.points = points;
            this.closed = closed;
            this.curveType = curveType;
            this.tension = tension;
        }

        public override Vector3 GetPoint(float t)
        {
            var l = points.Length;

            var p = (l - (this.closed ? 0 : 1)) * t;
            var intPoint = Mathf.FloorToInt(p);
            var weight = p - intPoint;

            if (this.closed)
            {
                intPoint += intPoint > 0 ? 0 : (Mathf.FloorToInt(Mathf.Abs(intPoint) / l) + 1) * l;
            }
            else if (weight == 0 && intPoint == l - 1)
            {
                intPoint = l - 2;
                weight = 1;
            }

            Vector3 p0, p3; // 4 points (p1 & p2 defined below)

            if (this.closed || intPoint > 0)
            {
                p0 = points[(intPoint - 1) % l];
            }
            else
            {
                // extrapolate first point
                p0 = (points[0]- points[1])*2 + points[0];
            }

            var p1 = points[intPoint % l];
            var p2 = points[(intPoint + 1) % l];

            if (this.closed || intPoint + 2 < l)
            {
                p3 = points[(intPoint + 2) % l];
            }
            else
            {
                // extrapolate last point
                p3 = points[l - 1] - points[l - 2] + points[l - 1];
            }

            CubicPoly1D px = new CubicPoly1D(), py= new CubicPoly1D(), pz = new CubicPoly1D();
            if (this.curveType ==  SplineType.Centripetal || this.curveType == SplineType.Chordal)
            {

                // init Centripetal / Chordal Catmull-Rom
                var pow = this.curveType == SplineType.Chordal ? 0.5f : 0.25f;
                var dt0 = Mathf.Pow((p0-p1).sqrMagnitude, pow);
                var dt1 = Mathf.Pow((p1 - p2).sqrMagnitude , pow);
                var dt2 = Mathf.Pow((p2 - p3).sqrMagnitude , pow);

                // safety check for repeated points
                var delta = 0.0001f;
                if (dt1 < delta) dt1 = 1.0f;
                if (dt0 < delta) dt0 = dt1;
                if (dt2 < delta) dt2 = dt1;

                px.InitNonuniformCatmullRom(p0.x, p1.x, p2.x, p3.x, dt0, dt1, dt2);
                py.InitNonuniformCatmullRom(p0.y, p1.y, p2.y, p3.y, dt0, dt1, dt2);
                pz.InitNonuniformCatmullRom(p0.z, p1.z, p2.z, p3.z, dt0, dt1, dt2);

            }
            else if (this.curveType == SplineType.Catmullrom)
            {

                px.InitCatmullRom(p0.x, p1.x, p2.x, p3.x, this.tension);
                py.InitCatmullRom(p0.y, p1.y, p2.y, p3.y, this.tension);
                pz.InitCatmullRom(p0.z, p1.z, p2.z, p3.z, this.tension);

            }

            var point = new Vector3(
                px.Calc(weight),
                py.Calc(weight),
                pz.Calc(weight)
            );

            return point;

        }
    }

    public class CubicPoly1D
    {
        float c0;
        float c1;
        float c2;
        float c3;

        /*
         * Compute coefficients for a cubic polynomial
         *   p(s) = c0 + c1*s + c2*s^2 + c3*s^3
         * such that
         *   p(0) = x0, p(1) = x1
         *  and
         *   p'(0) = t0, p'(1) = t1.
         */
        public void Init(float x0, float x1, float t0, float t1 )
        {

            c0 = x0;
            c1 = t0;
            c2 = -3 * x0 + 3 * x1 - 2 * t0 - t1;
            c3 = 2 * x0 - 2 * x1 + t0 + t1;

        }

        public void InitCatmullRom(float x0, float x1, float x2, float x3, float tension )
        {

            Init(x1, x2, tension * (x2 - x0), tension * (x3 - x1));

        }

        public void InitNonuniformCatmullRom(float x0, float x1, float x2, float x3, float dt0, float dt1, float dt2 )
        {

            // compute tangents when parameterized in [t1,t2]
            var t1 = (x1 - x0) / dt0 - (x2 - x0) / (dt0 + dt1) + (x2 - x1) / dt1;
            var t2 = (x2 - x1) / dt1 - (x3 - x1) / (dt1 + dt2) + (x3 - x2) / dt2;

            // rescale tangents for parametrization in [0,1]
            t1 *= dt1;
            t2 *= dt1;

            Init(x1, x2, t1, t2);

        }

		public float Calc(float t )
        {

            float t2 = t * t;
            float t3 = t2 * t;
            return c0 + c1 * t + c2 * t2 + c3 * t3;

        }

    }

    public class CubicPoly3D
    {
        Vector3 c0;
        Vector3 c1;
        Vector3 c2;
        Vector3 c3;

        /*
         * Compute coefficients for a cubic polynomial
         *   p(s) = c0 + c1*s + c2*s^2 + c3*s^3
         * such that
         *   p(0) = x0, p(1) = x1
         *  and
         *   p'(0) = t0, p'(1) = t1.
         */
        public void Init(Vector3 x0, Vector3 x1, Vector3 t0, Vector3 t1)
        {

            c0 = x0;
            c1 = t0;
            c2 = -3 * x0 + 3 * x1 - 2 * t0 - t1;
            c3 = 2 * x0 - 2 * x1 + t0 + t1;

        }

        public void InitCatmullRom(Vector3 x0, Vector3 x1, Vector3 x2, Vector3 x3, float tension)
        {

            Init(x1, x2, tension * (x2 - x0), tension * (x3 - x1));

        }

        public void InitNonuniformCatmullRom(Vector3 x0, Vector3 x1, Vector3 x2, Vector3 x3, float dt0, float dt1, float dt2)
        {

            // compute tangents when parameterized in [t1,t2]
            var t1 = (x1 - x0) / dt0 - (x2 - x0) / (dt0 + dt1) + (x2 - x1) / dt1;
            var t2 = (x2 - x1) / dt1 - (x3 - x1) / (dt1 + dt2) + (x3 - x2) / dt2;

            // rescale tangents for parametrization in [0,1]
            t1 *= dt1;
            t2 *= dt1;

            Init(x1, x2, t1, t2);

        }

        public Vector3 Calc(float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            return c0 + c1 * t + c2 * t2 + c3 * t3;
        }

    }
}
