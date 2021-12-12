﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

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
        Debug.Assert(0 <= t && t <= 1);

        return Mathf.Pow(1 - t, 3) * p0
               + 3 * Mathf.Pow(1 - t, 2) * t * p1
               + 3 * (1 - t) * Mathf.Pow(t, 2) * p2
               + Mathf.Pow(t, 3) * p3;
    }

    // Returns first derivative B'(t) for given parameter 0 <= t <= 1
    public Vector3 GetFirstDerivative(float t)
    {
        Debug.Assert(0 <= t && t <= 1);

        // https://en.wikipedia.org/w/index.php?title=B%C3%A9zier_curve&oldid=1058020248#Cubic_B%C3%A9zier_curves
        return 3 * Mathf.Pow(1 - t, 2) * (p1 - p0)
               + 6 * (1 - t) * t * (p2 - p1)
               + 3 * Mathf.Pow(t, 2) * (p3 - p2);
    }

    // Returns second derivative B''(t) for given parameter 0 <= t <= 1
    public Vector3 GetSecondDerivative(float t)
    {
        Debug.Assert(0 <= t && t <= 1);

        https://en.wikipedia.org/w/index.php?title=B%C3%A9zier_curve&oldid=1058020248#Cubic_B%C3%A9zier_curves
        return 6 * (1 - t) * (p2 - 2 * p1 + p0)
               + 6 * t * (p3 - 2 * p2 + p1);
    }

    // Returns the tangent vector to the curve at point B(t) for a given 0 <= t <= 1
    public Vector3 GetTangent(float t)
    {
        Debug.Assert(0 <= t && t <= 1);

        return GetFirstDerivative(t).normalized;
    }

    // Returns the Frenet normal to the curve at point B(t) for a given 0 <= t <= 1
    public Vector3 GetNormal(float t)
    {
        Debug.Assert(0 <= t && t <= 1);

        return Vector3.Cross(GetTangent(t), GetBinormal(t)).normalized;
    }

    // Returns the Frenet binormal to the curve at point B(t) for a given 0 <= t <= 1
    public Vector3 GetBinormal(float t)
    {
        Debug.Assert(0 <= t && t <= 1);

        Vector3 velocityVec = GetFirstDerivative(t);
        Vector3 accelerationVec = GetSecondDerivative(t);

        Vector3 infinitesimalCloseVelocity = velocityVec + accelerationVec;
        Vector3 infinitesimalCloseTangent = infinitesimalCloseVelocity.normalized;

        return Vector3.Cross(velocityVec.normalized, infinitesimalCloseTangent).normalized;
    }

    // Calculates the arc-lengths lookup table
    public void CalcCumLengths()
    {
        int numOfSampleLengths = numSteps + 1; // Amount of entries in LUT

        List<Vector3> samplePoints = BezierMesh.GetSamplePoints(this, numOfSampleLengths);
        cumLengths = new float[numOfSampleLengths]; // Initialize zeroed

        int idx = 1;
        foreach (float cumSum in CumulativeMagnitudeSum(samplePoints, numOfSampleLengths))
        {
            cumLengths[idx++] = cumSum;
        }
    }

    private IEnumerable<float> CumulativeMagnitudeSum(List<Vector3> samplePoints, int numOfSampleLengths)
    {
        float cumSum = 0.0f;

        foreach (var idx in Enumerable.Range(1, numOfSampleLengths - 1))
        {
            cumSum += (samplePoints[idx] - samplePoints[idx - 1]).magnitude;

            yield return cumSum;
        }
    }

    // Returns the total arc-length of the Bezier curve
    public float ArcLength()
    {
        return cumLengths[numSteps];
    }

    // Returns approximate t s.t. the arc-length to B(t) = arcLength
    public float ArcLengthToT(float a)
    {
        if (cumLengths.Contains(a))
        {
            return (float)Array.IndexOf(cumLengths, a) / numSteps;
        }

        for (int idx = 0; idx < numSteps; ++idx)
        {
            if (cumLengths[idx] <= a && a <= cumLengths[idx + 1])
            {
                return Mathf.Lerp((float)idx / numSteps, (float)(idx + 1) / numSteps,
                                  Mathf.InverseLerp(cumLengths[idx], cumLengths[idx + 1], a));
            }
        }

        return -1.0f; // `a` is either strictly greater or smaller than all the cumLengths
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