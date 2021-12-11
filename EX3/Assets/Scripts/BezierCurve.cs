using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class BezierCurve : MonoBehaviour
{
    // Bezier control points
    public Vector3 p0;
    public Vector3 p1;
    public Vector3 p2;
    public Vector3 p3;

    private float[] cumLengths; // Cumulative lengths lookup table
    private readonly int numSteps = 128; // Number of points to sample for the cumLengths LUT

    // Returns position B(t) on the Bezier curve for given parameter 0 <= t <= 1
    public Vector3 GetPoint(float t)
    {
        Debug.Assert(0.0f <= t && t <= 1.0f);

        return Mathf.Pow(1.0f - t, 3.0f) * p0 +
                3.0f * Mathf.Pow(1.0f - t, 2.0f) * t * p1 +
                3.0f * (1.0f - t) * Mathf.Pow(t, 2.0f) * p2 +
                Mathf.Pow(t, 3.0f) * p3;
    }

    // Returns first derivative B'(t) for given parameter 0 <= t <= 1
    public Vector3 GetFirstDerivative(float t)
    {
        Debug.Assert(0.0f <= t && t <= 1.0f);

        return -3.0f * (Mathf.Pow(1.0f - t, 2.0f) * p0 +
                (-3.0f * Mathf.Pow(t, 2.0f) + 4.0f * t - 1.0f) * p1 +
                (3.0f * t * p2 - 2 * p2 - p3 * t) * t);
    }

    // Returns second derivative B''(t) for given parameter 0 <= t <= 1
    public Vector3 GetSecondDerivative(float t)
    {
        Debug.Assert(0.0f <= t && t <= 1.0f);

        return -6.0f * (-p0 * (1.0f - t) +
                        p1 * (2.0f - 3.0f * t) +
                        3.0f * p2 * t -
                        p2 -
                        p3 * t);
    }

    // Returns the tangent vector to the curve at point B(t) for a given 0 <= t <= 1
    public Vector3 GetTangent(float t)
    {
        Debug.Assert(0.0f <= t && t <= 1.0f);

        return GetFirstDerivative(t).normalized;
    }

    // Returns the Frenet normal to the curve at point B(t) for a given 0 <= t <= 1
    public Vector3 GetNormal(float t)
    {
        Debug.Assert(0.0f <= t && t <= 1.0f);

        return Vector3.Cross(GetTangent(t), GetBinormal(t)).normalized;
    }

    // Returns the Frenet binormal to the curve at point B(t) for a given 0 <= t <= 1
    public Vector3 GetBinormal(float t)
    {
        Debug.Assert(0.0f <= t && t <= 1.0f);

        Vector3 velocityVec = GetFirstDerivative(t);
        Vector3 accelerationVec = GetSecondDerivative(t);

        Vector3 infinitesimalCloseVelocity = velocityVec + accelerationVec;
        Vector3 infinitesimalCloseTangent = infinitesimalCloseVelocity.normalized;

        return Vector3.Cross(velocityVec.normalized, infinitesimalCloseTangent).normalized;
    }

    // Calculates the arc-lengths lookup table
    public void CalcCumLengths()
    {
        // Your implementation here...
    }

    // Returns the total arc-length of the Bezier curve
    public float ArcLength()
    {
        return 0;
    }

    // Returns approximate t s.t. the arc-length to B(t) = arcLength
    public float ArcLengthToT(float a)
    {
        return 0;
    }

    // Start is called before the first frame update
    public void Start()
    {
        Refresh();
    }

    // Update the curve and send a message to other components on the GameObject
    public void Refresh()
    {
        CalcCumLengths();
        if (Application.isPlaying)
        {
            SendMessage("CurveUpdated", SendMessageOptions.DontRequireReceiver);
        }
    }

    // Set default values in editor
    public void Reset()
    {
        p0 = new Vector3(1f, 0f, 1f);
        p1 = new Vector3(1f, 0f, -1f);
        p2 = new Vector3(-1f, 0f, -1f);
        p3 = new Vector3(-1f, 0f, 1f);

        Refresh();
    }
}