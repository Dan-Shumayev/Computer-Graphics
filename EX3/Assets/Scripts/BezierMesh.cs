using System.Collections.Generic;
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
        var meshData = new QuadMeshData();

        // The list of vertices will contain the vertices around the first sample point,
        // then around the second, and so on.
        // Similarly, the list of quads will contain the quads between the first and second points,
        // then between the second and third, and so on.
        var vertices = new List<Vector3>((numSteps + 1) * numSides);
        var quads = new List<Vector4>(numSteps * numSides);

        // For each sample point...
        for (var step = 0; step <= numSteps; ++step)
        {
            float t = (float)step / numSteps;
            Vector3 point = curve.GetPoint(t);
            Vector3 normal = curve.GetNormal(t);
            Vector3 binormal = curve.GetBinormal(t);

            // For each vertex around the sample point...
            for (var vertexIndex = 0; vertexIndex < numSides; ++vertexIndex)
            {
                // Get a uniformly-distributed point on the unit circle...
                Vector2 unitCirclePoint = GetUnitCirclePoint((float)vertexIndex / numSides * 360);

                // Set its radius...
                Vector2 circlePoint = radius * unitCirclePoint;

                // Transform it to the normal-binormal plane...
                // Since we have two coordinates on the XY plane and two orthonormal vectors,
                // we can transform the coordinates to be on the vectors' plane by simple multiplication.
                // We consider the binormal to be the new X axis and the normal the new Y axis,
                // but the returned circle points have their coordinates swapped ((y,x) and not (x,y)),
                // so we remedy that. It doesn't actually matter, since the order of the coordinates
                // determines only whether we're going clockwise or counter-clockwise around the sample
                // point, and the absolute positions of the vertices. The resulting mesh will still
                // look mostly the same. The order here was chosen because of the picture on page 7
                // of the exercise description.
                Vector3 circlePointOnTargetPlane = circlePoint.x * normal + circlePoint.y * binormal;

                // And move it to be around the sample point.
                Vector3 vertex = point + circlePointOnTargetPlane;

                vertices.Add(vertex);

                // There are no quads after the last sample point.
                if (step == numSteps)
                {
                    continue;
                }

                // The quad consists of the following points, clockwise:
                // 1. The current vertex
                // 2. The next vertex
                // 3. The next vertex *around the next sample point*
                // 4. The "current vertex" *around the next sample point*
                // See the illustation on page 8 of the exercise description.

                int nextVertexIndex = (vertexIndex + 1) % numSides;
                int currentStepVerticesBegin = step * numSides;
                int nextStepVerticesBegin = (step + 1) * numSides;

                var quad = new Vector4(
                    currentStepVerticesBegin + vertexIndex,
                    currentStepVerticesBegin + nextVertexIndex,
                    nextStepVerticesBegin + nextVertexIndex,
                    nextStepVerticesBegin + vertexIndex);

                quads.Add(quad);
            }
        }

        meshData.vertices = vertices;
        meshData.quads = quads;

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