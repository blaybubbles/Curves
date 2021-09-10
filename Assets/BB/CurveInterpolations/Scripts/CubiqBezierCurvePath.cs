using System;
using System.Collections.Generic;
using UnityEngine;

namespace CurveLib.Curves
{

    public class CubiqBezierCurvePath : BaseCurve
    {
        private Vector3[] points;
        private bool byAngle;
        private bool closed;
        private float tension;
        private Vector3[] directions;
        private float[] cacheLengths;

        public CubiqBezierCurvePath(Vector3[] points, float tension = 0.5f, bool byAngle = false, bool closed = false)
        {
            this.points = points;
            this.tension = tension;
            this.byAngle = byAngle;
            this.closed = closed;
            var count = points.Length;
            this.directions = new Vector3[count];

            for (int i = 0; i < count ; i++)
            {
                Vector3 dir;
                if (i == 0)
                {
                    dir = (points[i + 1] - points[i]) * tension;
                }
                else if (i == count - 1)
                {
                    dir = (points[i] - points[i - 1]) * tension;
                }
                else
                {
                    Vector3 cDirection = points[i] - points[i - 1];
                    Vector3 nDirection = points[i + 1] - points[i];
                    cDirection = cDirection.magnitude < nDirection.magnitude ? cDirection : nDirection;
                    if (!byAngle)
                    {
                        dir = Vector3.Project(cDirection, points[i + 1] - points[i - 1]) * tension;

                    }
                    else
                    {
                        dir = Vector3.Project(cDirection, (points[i + 1] - points[i]).normalized - (points[i - 1] - points[i]).normalized) * tension;
                    }
                }
                this.directions[i] = dir;
            }
        }

        public override Vector3 GetPoint(float t)
        {
            float pathLen = this.GetLength();
            var d = t * pathLen;
            var curveLengths = this.GetCurveLengths();

            var point = Vector3.zero;
            // To think about boundaries points.
            for (int i = 0; i < curveLengths.Length; i++)
            {
                if (curveLengths[i] >= d && (i == curveLengths.Length - 1 || d < curveLengths[i + 1]))
                {

                    var diff = curveLengths[i] - d;
                    var curve = GetCubicBezierCurve(i);
                    var segmentLength = curve.GetLength();
                    var u = d / pathLen;// segmentLength == 0 ? 0 : 1 - diff / segmentLength;


                    point = curve.GetPoint(u);

                    break;
                }

            }
          

            return point;
        }

        public override Vector3[] GetPoints(int divisions = 12)
        {

            var splinePoints = new List<Vector3>();
            var last = Vector3.zero;

            for (int i = 0; i < points.Length - 1; i++)
            {

                var curve = GetCubicBezierCurve(i);
                var resolution = divisions / (points.Length - 1);

                var pts = curve.GetPoints(resolution);

                //splinePoints.AddRange(pts);
                for (int j = 0; j < pts.Length; j++)
                {

                    var point = pts[j];

                    if ((i != 0 || j != 0) && last == point) continue; // ensures no consecutive points are duplicates

                    splinePoints.Add(point);
                    last = point;
                }

            }

            if (this.closed && points.Length > 1 && points[points.Length - 1] != points[0])
            {

                splinePoints.Add(points[0]);

            }

            return splinePoints.ToArray();

        }

        public override float GetLength(int divisions = 200)
        {
            var lens = this.GetCurveLengths();
            return lens[lens.Length - 1];
        }

        public float[] GetCurveLengths()
        {
            // We use cache values if curves and cache array are same length

            if (this.cacheLengths != null && this.cacheLengths.Length == this.points.Length - 1)
            {
                return this.cacheLengths;
            }

            // Get length of sub-curve
            // Push sums into cached array

            var lengths = new float[this.points.Length - 1];
            float sums = 0;

            for (int i = 0; i < this.points.Length - 1; i++)
            {

                CubicBezierCurve cubicBezierCurve = GetCubicBezierCurve(i);
                sums += cubicBezierCurve.GetLength();
                lengths[i] = sums;
            }

            this.cacheLengths = lengths;

            return lengths;

        }

        private CubicBezierCurve GetCubicBezierCurve(int i)
        {
            return new CubicBezierCurve(points[i], points[i] + directions[i], points[i + 1] - directions[i + 1], points[i + 1]);
        }

        public override void UpdateArcLengths()
        {

            this.needsUpdate = true;
            this.cacheLengths = null;
            this.GetCurveLengths();

        }

    }
}
