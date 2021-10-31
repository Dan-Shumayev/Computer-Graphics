using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    public TextAsset BVHFile; // The BVH file that defines the animation and skeleton
    public bool animate; // Indicates whether or not the animation should be running

    private BVHData data; // BVH data of the BVHFile will be loaded here
    private int currFrame = 0; // Current frame of the animation

    private static readonly Vector3 Scale = new Vector3(2.0f, 2.0f, 2.0f);
    private static readonly Vector3 HeadScale = new Vector3(8.0f, 8.0f, 8.0f);

    // Start is called before the first frame update
    void Start()
    {
        BVHParser parser = new BVHParser();
        data = parser.Parse(BVHFile);

        CreateJoint(data.rootJoint, Vector3.zero); // Position the center of our skeleton (Root joint object)
                                                   // at the origin (World-Space)

        Debug.Log(RotateTowardsVector(new Vector3(1.0f, 1.0f, 1.0f)).MultiplyVector(Vector3.up));
        Debug.Log(MatrixUtils.RotateTowardsVector(new Vector3(1.0f, 1.0f, 1.0f)).MultiplyVector(Vector3.up));
    }

    // Returns a Matrix4x4 representing a rotation aligning
    // the up direction of an object with the given v
    Matrix4x4 RotateTowardsVector(Vector3 v)
    {
        // The normal of the plane spanned by `v` and the object to be directed
        Vector3 rotationAxis = Vector3.Cross(Vector3.up, v);

        // The angle (in degrees) between `Vector3.up` and `v`, positive if `v` lies clockwise from
        // `Vector3.up`, and negative otherwise.
        float rotationAngle = Vector3.SignedAngle(Vector3.up, v, rotationAxis);

        // The rotation process described in TA-2
        return RotateAroundVec(rotationAxis, rotationAngle);
    }

    // Summary:
    //      This method returns a 4X4 transform' matrix that rotates our object by
    //      rotationAngle degrees around the received vector, `v`.
    Matrix4x4 RotateAroundVec(Vector3 v, float rotationAngle)
    {
        // Find the angles that `v` induces on each of the 3-axes - and return rotation transform accordingly
        Matrix4x4 xRotation = MatrixUtils.RotateX(-90.0f + Mathf.Atan2(v.y, v.z) * Mathf.Rad2Deg);
        Matrix4x4 zRotation = MatrixUtils.RotateZ(90.0f -
                        Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(v.y, 2) + Mathf.Pow(v.z, 2)), v.x) * Mathf.Rad2Deg);
        Matrix4x4 yRotation = MatrixUtils.RotateY(rotationAngle);

        return xRotation.inverse * zRotation.inverse * yRotation * zRotation * xRotation;
    }

    // Creates a Cylinder GameObject between two given points in 3D space
    GameObject CreateCylinderBetweenPoints(Vector3 p1, Vector3 p2, float diameter)
    {
        var cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

        var heightScaling = (Vector3.Distance(p1, p2) / 2) * Vector3.up;
        var diameterScaling = diameter * (Vector3.right + Vector3.forward);
        var scaling = MatrixUtils.Scale(heightScaling + diameterScaling);

        var rotation = RotateTowardsVector(p2 - p1);

        var translation = MatrixUtils.Translate((p1 + p2) / 2);

        MatrixUtils.ApplyTransform(cylinder, translation * rotation * scaling);

        return cylinder;
    }

    // Creates a GameObject representing a given BVHJoint and recursively creates GameObjects for its child joints
    GameObject CreateJoint(BVHJoint joint, Vector3 parentPosition)
    {
        if (joint.isEndSite)
        {
            return null;
        }

        joint.gameObject = new GameObject(joint.name);
        // Create a sphere representing the joint, and make it a child of joint
        GameObject jointSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        jointSphere.transform.parent = joint.gameObject.transform;

        // Apply Scale Transform' on the sphere
        ScaleSphere(joint, jointSphere);

        // Apply Translate Transform' on joint
        Vector3 relativePos = joint.offset + parentPosition;
        MatrixUtils.ApplyTransform(joint.gameObject, MatrixUtils.Translate(relativePos));

        foreach (BVHJoint child in joint.children)
        {
            CreateJoint(child, relativePos);
        }

        return joint.gameObject;
    }

    private void ScaleSphere(BVHJoint joint, GameObject jointSphere)
    {
        MatrixUtils.ApplyTransform(jointSphere,
            joint.name == "Head" ? MatrixUtils.Scale(HeadScale) : MatrixUtils.Scale(Scale));
    }

    // Transforms BVHJoint according to the keyframe channel data, and recursively transforms its children
    private void TransformJoint(BVHJoint joint, Matrix4x4 parentTransform, float[] keyframe)
    {
        // Your code here
    }

    // Update is called once per frame
    void Update()
    {
        if (animate)
        {
            // Your code here
        }
    }
}
