using CurveLib.Interpolation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace CurveLib.Curves
{
    [Serializable]
    public class CubicBezierCurve : BaseCurve
    {
        private Vector3 v0;
        private Vector3 v1;
        private Vector3 v2;
        private Vector3 v3;

        public CubicBezierCurve(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3) : base()
        {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }

        public override Vector3 GetPoint(float t)
        {
            var point = CurveInterpolations.CubicBezier(t, v0, v1, v2, v3);
            return point;
        }
    }
}