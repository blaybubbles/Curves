using CurveLib.Curves;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class SplineSceneController : MonoBehaviour
{
    public bool dirty = true;

    public float tension = 0.5f;
    public List<LineRenderer> generatedCubic;
    public LineRenderer generatedSpline;
    public LineRenderer generatedSpline_t;
    public LineRenderer generatedSplineCatmullrom;
    public LineRenderer generatedSplineChordal;

    public Material red;
    public Material blue;
    public Material green;
    public Material yellow;
    public bool byAngle;

    private void OnEnable()
    {
        if (generatedCubic == null)
        {
            generatedCubic = new List<LineRenderer>();

        }

        if (generatedSpline == null)
        {
            var go = new GameObject("CentripetalSpline");
            generatedSpline = go.AddComponent<LineRenderer>();
        }
        if (generatedSpline_t == null)
        {
            var go = new GameObject("CentripetalSpline2");
            generatedSpline_t = go.AddComponent<LineRenderer>();
            generatedSplineChordal.startWidth = 0.2f;
            generatedSplineChordal.endWidth = 0.2f;
        }

        if (generatedSplineChordal == null)
        {
            var go = new GameObject("ChordalSpline");
            generatedSplineChordal = go.AddComponent<LineRenderer>();
            generatedSplineChordal.startWidth = 0.2f;
            generatedSplineChordal.endWidth = 0.2f;
        }
        if (generatedSplineCatmullrom == null)
        {
            var go = new GameObject("CatmullromSpline");
            generatedSplineCatmullrom = go.AddComponent<LineRenderer>();
            generatedSplineCatmullrom.startWidth = 0.2f;
            generatedSplineCatmullrom.endWidth = 0.2f;
        }
        dirty = true;
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (dirty)
        {
            var count = transform.childCount;
            var points = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                points[i] = transform.GetChild(i).position;
            }

            if (count > 2)
            {
                var curvePath = new CubiqBezierCurvePath(points, tension, byAngle);
                var len = curvePath.GetLength();
                var curvePoints = curvePath.GetPoints((int)(len * 4));
                Debug.Log($"divisions {curvePoints.Length}");

                if (generatedCubic.Count < 1)
                {
                    var go = new GameObject("CubicBezierCurve_path", typeof(LineRenderer));
                    LineRenderer line = go.GetComponent<LineRenderer>();
                    generatedCubic.Add(line);
                    line.startWidth = 0.2f;
                    line.endWidth = 0.2f;
                    line.material = green;
                }

                generatedCubic[0].positionCount = curvePoints.Length;
                generatedCubic[0].SetPositions(curvePoints);

            }


            if (count > 4)
            {
                int countCubic = (count - 1) / 2;
                Vector3 dir2 = Vector3.zero;
                for (int i = 0; i < countCubic-1; i++)
                {


                    CubicBezierCurve cubicBezierCurve = new CubicBezierCurve(points[i * 3], points[i * 3 + 1], points[i * 3 + 2], points[i * 3 + 3]);
                    var len = cubicBezierCurve.GetLength();
                    var curvePoints = cubicBezierCurve.GetPoints((int)(len * 8));
                    Debug.Log($"divisions {curvePoints.Length}");

                    if (generatedCubic.Count < i + 2)
                    {
                        var go = new GameObject("CubicBezierCurve_" + i, typeof(LineRenderer));
                        LineRenderer line = go.GetComponent<LineRenderer>();
                        generatedCubic.Add(line);
                        line.startWidth = 0.2f;
                        line.endWidth = 0.2f;
                        line.material = blue;
                    }

                    generatedCubic[i+1].positionCount = curvePoints.Length;
                    generatedCubic[i+1].SetPositions(curvePoints);


                }
            }




            var curve0 = new SplineCurve(points, tension: tension);
            var len1 = curve0.GetLength();
            var ps1 = curve0.GetPoints((int)(len1 * 4));
            generatedSpline.positionCount = ps1.Length;
            generatedSpline.SetPositions(ps1);

            {
                var ps = curve0.GetPoints(0f, 0.3f, (int)(len1 * 4 * 0.3f));
                generatedSpline_t.positionCount = ps.Length;
                generatedSpline_t.SetPositions(ps);
            }
            {
                var curve3 = new SplineCurve(points, false, SplineType.Chordal, tension: tension);

                var len = curve3.GetLength();
                var ps = curve3.GetPoints((int)(len * 4));
                generatedSplineChordal.positionCount = ps.Length;
                generatedSplineChordal.SetPositions(ps);
            }
            {
                var curve2 = new SplineCurve(points, false, SplineType.Catmullrom, tension: tension);

                var len = curve2.GetLength();
                var ps = curve2.GetPoints((int)(len * 4));
                generatedSplineCatmullrom.positionCount = ps.Length;
                generatedSplineCatmullrom.SetPositions(ps);
            }

            dirty = false;
        }


    }
}
