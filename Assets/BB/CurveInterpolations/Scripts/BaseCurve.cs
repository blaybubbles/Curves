using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public abstract class BaseCurve
{
    private float[] cacheArcLengths;
    protected bool needsUpdate;

    public abstract Vector3 GetPoint(float t);

    public Vector3 GetPointAt(float u)
    {

        var t = this.GetUtoTmapping(u);
        return this.GetPoint(t);

    }

    // Get sequence of points using getPoint( t )

    public virtual Vector3[] GetPoints(int divisions = 5)
    {

        Vector3[] points = new Vector3[divisions + 1];

        for (int d = 0; d <= divisions; d++)
        {
            points[d] = this.GetPoint((float)d / divisions);
        }

        return points;

    }

    public virtual Vector3[] GetPoints(float from, float to, int divisions = 5)
    {

        Vector3[] points = new Vector3[divisions + 1];
        var delta = (to - from);
        for (int d = 0; d <= divisions; d++)
        {
            float t = (float)d / divisions*delta;

            
            {
                points[d] = this.GetPoint(t);
            }
        }

        return points;

    }

    // Get sequence of points using getPointAt( u )

    public Vector3[] GetSpacedPoints(int divisions = 5)
    {

        Vector3[] points = new Vector3[divisions + 1];


        for (int d = 0; d <= divisions; d++)
        {

            points[d] = (this.GetPointAt((float)d / divisions));

        }

        return points;

    }

    // Get total curve arc length

    public virtual float GetLength(int divisions = 200)
    {

        var lengths = this.GetLengths(divisions);
        return lengths[lengths.Length - 1];

    }

    // Get list of cumulative segment lengths

    public virtual float[] GetLengths(int divisions = 200)
    {

        if (this.cacheArcLengths != null &&
            (this.cacheArcLengths.Length == divisions + 1) &&
            !this.needsUpdate)
        {

            return this.cacheArcLengths;

        }

        this.needsUpdate = false;

        float[] cache = new float[divisions + 1];
        Vector3 current, last = this.GetPoint(0);
        float sum = 0;

        cache[0] = 0;

        for (int p = 1; p <= divisions; p++)
        {

            current = this.GetPoint((float)p / divisions);
            sum += Vector3.Distance(current, last);
            cache[p] = (sum);
            last = current;

        }

        this.cacheArcLengths = cache;

        return cache; // { sums: cache, sum: sum }; Sum is in the last element.

    }



    public virtual void UpdateArcLengths()
    {

        this.needsUpdate = true;
        this.GetLengths();

    }

    // Given u ( 0 .. 1 ), get a t to find p. This gives you points which are equidistant

    public float GetUtoTmapping(float u, float distance = 0)
    {

        var arcLengths = this.GetLengths();

        int i = 0;
        int il = arcLengths.Length;

        float targetArcLength; // The targeted u distance value to get

        if (distance != 0)
        {
            targetArcLength = distance;
        }
        else
        {
            targetArcLength = u * arcLengths[il - 1];
        }

        // binary search for the index with largest value smaller than target u distance

        int low = 0, high = il - 1;
        float comparison;

        while (low <= high)
        {

            i = Mathf.FloorToInt((float)(low + (high - low)) / 2f);

            comparison = arcLengths[i] - targetArcLength;

            if (comparison < 0)
            {
                low = i + 1;
            }
            else if (comparison > 0)
            {
                high = i - 1;
            }
            else
            {

                high = i;
                break;

                // DONE

            }

        }

        i = high;

        if (arcLengths[i] == targetArcLength)
        {
            return i / (il - 1);

        }

        // we could get finer grain at lengths, or use simple interpolation between two points

        var lengthBefore = arcLengths[i];
        var lengthAfter = arcLengths[i + 1];

        var segmentLength = lengthAfter - lengthBefore;

        // determine where we are between the 'before' and 'after' points

        var segmentFraction = (targetArcLength - lengthBefore) / segmentLength;

        // add that fractional amount to t

        var t = (i + segmentFraction) / (il - 1);

        return t;

    }

    // Returns a unit vector tangent at t
    // In case any sub curve does not implement its tangent derivation,
    // 2 points a small delta apart will be used to find its gradient
    // which seems to give a reasonable approximation

    public Vector3 GetTangent(float t, Vector3 optionalTarget = default)
    {

        var delta = 0.0001f;
        var t1 = t - delta;
        var t2 = t + delta;

        // Capping in case of danger

        if (t1 < 0) t1 = 0;
        if (t2 > 1) t2 = 1;

        var pt1 = this.GetPoint(t1);
        var pt2 = this.GetPoint(t2);

        var tangent = (pt2 - pt1).normalized;

        return tangent;
    }

    public Vector3 GetTangentAt(float u, Vector3 optionalTarget = default)
    {

        var t = this.GetUtoTmapping(u);
        return this.GetTangent(t, optionalTarget);

    }

    public (Vector3[] tangents, Vector3[] normals, Vector3[] binormals) ComputeFrenetFrames(int segments, bool closed)
    {

        // see http://www.cs.indiana.edu/pub/techreports/TR425.pdf

        var normal = new Vector3();

        Vector3[] tangents = new Vector3[segments];
        Vector3[] normals = new Vector3[segments];
        Vector3[] binormals = new Vector3[segments];


        // compute the tangent vectors for each segment on the curve

        for (int i = 0; i <= segments; i++)
        {

            float u = (float)i / segments;

            tangents[i] = this.GetTangentAt(u, new Vector3());
            tangents[i].Normalize();

        }

        // select an initial normal vector perpendicular to the first tangent vector,
        // and in the direction of the minimum tangent xyz component

        normals[0] = new Vector3();
        binormals[0] = new Vector3();
        var min = float.MaxValue;
        var tx = Math.Abs(tangents[0].x);
        var ty = Math.Abs(tangents[0].y);
        var tz = Math.Abs(tangents[0].z);

        if (tx <= min)
        {

            min = tx;
            normal = Vector3.right;

        }

        if (ty <= min)
        {

            min = ty;

            normal = Vector3.up;

        }

        if (tz <= min)
        {

            normal = Vector3.forward;

        }

        var vec = Vector3.Cross(tangents[0], normal).normalized;

        normals[0] = Vector3.Cross(tangents[0], vec);
        binormals[0] = Vector3.Cross(tangents[0], normals[0]);


        // compute the slowly-varying normal and binormal vectors for each segment on the curve

        for (int i = 1; i <= segments; i++)
        {

            normals[i] = normals[i - 1];

            binormals[i] = binormals[i - 1];

            vec = Vector3.Cross(tangents[i - 1], tangents[i]);

            if (vec.magnitude > float.Epsilon)
            {

                vec.Normalize();
                var theta = Mathf.Acos(Mathf.Clamp(Vector3.Dot(tangents[i - 1], tangents[i]), -1, 1)); // clamp for floating pt errors

                normals[i] = Quaternion.AngleAxis(theta, vec) * normals[i];
            }

            binormals[i] = Vector3.Cross(tangents[i], normals[i]);

        }

        // if the curve is closed, postprocess the vectors so the first and last normal vectors are the same

        if (closed == true)
        {

            var theta = Mathf.Acos(Mathf.Clamp(Vector3.Dot(normals[0], normals[segments]), -1, 1));
            theta /= segments;

            if (Vector3.Dot(tangents[0], Vector3.Cross(normals[0], normals[segments])) > 0)
            {
                theta = -theta;
            }

            for (int i = 1; i <= segments; i++)
            {

                // twist a little...
                normals[i] = Quaternion.AngleAxis(theta, tangents[i]) * normals[i];
                binormals[i] = Vector3.Cross(tangents[i], normals[i]);

            }

        }

        return (
            tangents: tangents,
            normals: normals,
            binormals: binormals

        );

    }




}
