using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class BezierMesh : MonoBehaviour
{
    private BezierCurve curve; // The Bezier curve around which to build the mesh

    public float Radius = 0.5f; // The distance of mesh vertices from the curve
    public int NumSteps = 16; // Number of points along the curve to sample
    public int NumSides = 8; // Number of vertices created at each point

    // Awake is called when the script instance is being loaded
    public void Awake()
    {
        curve = GetComponent<BezierCurve>();
        BuildMesh();
    }

    // Returns a "tube" Mesh built around the given Bézier curve
    public static Mesh GetBezierMesh(BezierCurve curve, float radius, int numSteps, int numSides)
    {
        QuadMeshData meshData = new QuadMeshData();

        List<float> samplePointSteps = Enumerable.Range(0, numSteps + 1).Select(stepIdx => (float)stepIdx / numSteps).ToList();
        List<Vector3> samplePoints = samplePointSteps.Select(step => curve.GetPoint(step)).ToList();

        foreach (var tuple in samplePoints.Zip(samplePointSteps, (x, y) => (Point: x, Param: y)))
        {
            meshData.vertices.AddRange(Enumerable.Range(0, numSides)
                                .Select(sideIdx => (float)(360.0f / numSides) * sideIdx)
                                .Select(deg => GetUnitCirclePoint(deg))
                                .Select(circlePoint => (circlePoint[0] * curve.GetBinormal(tuple.Param) + circlePoint[1] * curve.GetNormal(tuple.Param)).normalized)
                                .Select(circlePointDir =>
                                                        tuple.Point +
                                                        circlePointDir * radius));
        }

        for (int stepIdx = 0; stepIdx < numSteps; ++stepIdx)
        {
            int currStep = stepIdx * numSides;
            int incToNextStep = numSides;

            meshData.quads.AddRange(Enumerable.Range(0, numSides)
                                    .Select(circlePointIdx =>
                                        new Vector4(circlePointIdx + currStep,
                                        circlePointIdx + currStep + incToNextStep,
                                        currStep + incToNextStep + (circlePointIdx + 1) % numSides,
                                        currStep + (circlePointIdx + 1) % numSides)
                                        ));
        }

        return meshData.ToUnityMesh();
    }

    // Returns 2D coordinates of a point on the unit circle at a given angle from the x-axis
    private static Vector2 GetUnitCirclePoint(float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        return new Vector2(Mathf.Sin(radians), Mathf.Cos(radians));
    }

    public void BuildMesh()
    {
        var meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = GetBezierMesh(curve, Radius, NumSteps, NumSides);
    }

    // Rebuild mesh when BezierCurve component is changed
    public void CurveUpdated()
    {
        BuildMesh();
    }
}



[CustomEditor(typeof(BezierMesh))]
class BezierMeshEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Update Mesh"))
        {
            var bezierMesh = target as BezierMesh;
            bezierMesh.BuildMesh();
        }
    }
}