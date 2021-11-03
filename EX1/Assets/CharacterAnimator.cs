using System;
using System.Linq;
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    public TextAsset BVHFile; // The BVH file that defines the animation and skeleton
    public bool animate; // Indicates whether or not the animation should be running

    private BVHData data; // BVH data of the BVHFile will be loaded here
    private int currFrame = 0; // Current frame of the animation

    private static readonly Vector3 Scale = new Vector3(2.0f, 2.0f, 2.0f);
    private static readonly Vector3 HeadScale = new Vector3(8.0f, 8.0f, 8.0f);

    /// <summary>
    /// Time in seconds since the last animation frame was drawn.
    /// </summary>
    private float _timeSinceLastFrame = 0f;

    // Start is called before the first frame update
    void Start()
    {
        BVHParser parser = new BVHParser();
        data = parser.Parse(BVHFile);

        CreateJoint(data.rootJoint, Vector3.zero); // Position the center of our skeleton (Root joint object)
                                                   // at the origin (World-Space)
    }

    /// <summary>
    /// Returns a Matrix4x4 representing a rotation aligning the up direction of an
    /// object with the given vector <paramref name="v"/>.
    /// </summary>
    /// <param name="v">A vector representing the new up direction, after the rotation.</param>
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

    /// <summary>
    /// Returns a 4x4 transform matrix that rotates an object by <paramref name="rotationAngle"/>
    /// degrees around the vector <paramref name="v"/>, clockwise.
    /// </summary>
    /// <param name="v">Vector to rotate around.</param>
    /// <param name="rotationAngle">Angle, in degrees, to rotate around the vector.</param>
    private static Matrix4x4 RotateAroundVec(Vector3 v, float rotationAngle)
    {
        // The following assumes v is normalized.
        v = v.normalized;

        // Find the angles that `v` induces on each of the 3-axes - and return rotation transform accordingly
        Matrix4x4 xRotation = MatrixUtils.RotateX(-(90.0f - Mathf.Atan2(v.y, v.z) * Mathf.Rad2Deg));
        Matrix4x4 zRotation = MatrixUtils.RotateZ(90.0f -
                        Mathf.Atan2(Mathf.Sqrt(Mathf.Pow(v.y, 2) + Mathf.Pow(v.z, 2)), v.x) * Mathf.Rad2Deg);
        Matrix4x4 yRotation = MatrixUtils.RotateY(rotationAngle);

        return xRotation.inverse * zRotation.inverse * yRotation * zRotation * xRotation;
    }

    /// <summary>
    /// Creates a Cylinder GameObject between two given points in 3D space
    /// </summary>
    /// <param name="p1">First point from which to draw the cylinder</param>
    /// <param name="p2">Second point at which the cylinder should end</param>
    /// <param name="diameter">Width (diameter) of the cylinder to be drawn</param>
    GameObject CreateCylinderBetweenPoints(Vector3 p1, Vector3 p2, float diameter)
    {
        // Create a cylinder with height 2 and diameter 1, centered at the origin.
        var cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

        // Create a scaling matrix for the required cylinder dimensions. We scale
        // vertically (Y) by the distance between the points divided by the initial height (2),
        // and horizontally (X, Z) by the diameter.
        var heightScaling = (Vector3.Distance(p1, p2) / 2) * Vector3.up;
        var diameterScaling = diameter * (Vector3.right + Vector3.forward);
        var scaling = MatrixUtils.Scale(heightScaling + diameterScaling);

        // Rotate so that the cylinder points in the direction of the line p1 - p2.
        var rotation = RotateTowardsVector(p2 - p1);

        // Put the center of the cylinder at the midpoint between p1 and p2.
        var translation = MatrixUtils.Translate((p1 + p2) / 2);

        // Apply the complete transform.
        MatrixUtils.ApplyTransform(cylinder, translation * rotation * scaling);

        return cylinder;
    }

    /// <summary>
    /// Creates a GameObject representing a given BVHJoint and recursively creates GameObjects for
    /// its child joints.
    /// </summary>
    /// <param name="joint">The joint that will be drawn, along with any child joints it may have.</param>
    /// <param name="parentPosition">The 3D position of the parent of the given joint.</param>
    GameObject CreateJoint(BVHJoint joint, Vector3 parentPosition)
    {
        joint.gameObject = new GameObject(joint.name);

        // Create a sphere representing the joint, and make it a child of joint
        GameObject jointSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        jointSphere.transform.parent = joint.gameObject.transform;

        // Apply Scale Transform' on the sphere
        ScaleSphere(joint, jointSphere);

        // Apply Translate Transform on joint
        Vector3 relativePos = joint.offset + parentPosition;
        MatrixUtils.ApplyTransform(joint.gameObject, MatrixUtils.Translate(relativePos));

        foreach (BVHJoint child in joint.children)
        {
            var childJoint = CreateJoint(child, relativePos);
            var bone = CreateCylinderBetweenPoints(relativePos, childJoint.transform.position, 0.5f);
            bone.transform.parent = joint.gameObject.transform;
        }

        return joint.gameObject;
    }

    /// <summary>
    /// Scales a joint's sphere to the correct dimensions.
    /// </summary>
    /// <param name="joint">Joint whose sphere is to be scaled.</param>
    /// <param name="jointSphere">The sphere to scale.</param>
    private static void ScaleSphere(BVHJoint joint, GameObject jointSphere)
    {
        MatrixUtils.ApplyTransform(jointSphere,
            joint.name == "Head" ? MatrixUtils.Scale(HeadScale) : MatrixUtils.Scale(Scale));
    }

    /// <summary>
    /// Transforms a BVHJoint according to the keyframe channel data, and recursively transforms
    /// its children.
    /// </summary>
    /// <param name="joint">The joint to be transformed, along with any child joints it may have.</param>
    /// <param name="parentTransform">The parent joint's transformation matrix.</param>
    /// <param name="keyframe">The channel data to use in the transforms.</param>
    private void TransformJoint(BVHJoint joint, Matrix4x4 parentTransform, float[] keyframe)
    {
        // Create a rotation matrix. Since end sites don't have rotations,
        // we default to the identity matrix.

        var rotation = Matrix4x4.identity;

        if (!joint.isEndSite)
        {
            var xRotation = MatrixUtils.RotateX(keyframe[joint.rotationChannels.x]);
            var yRotation = MatrixUtils.RotateY(keyframe[joint.rotationChannels.y]);
            var zRotation = MatrixUtils.RotateZ(keyframe[joint.rotationChannels.z]);

            var rotations = new[] { xRotation, yRotation, zRotation };
            var rotationOrders = new[] { joint.rotationOrder.x, joint.rotationOrder.y, joint.rotationOrder.z };
            Array.Sort(rotationOrders, rotations);

            rotation = rotations.Aggregate((a, b) => a * b);
        }

        // Create a translation matrix. Even end sites have this.
        var translation = MatrixUtils.Translate(joint.offset);

        var localTransform = translation * rotation;

        var globalTransform = parentTransform * localTransform;

        MatrixUtils.ApplyTransform(joint.gameObject, globalTransform);

        foreach (var child in joint.children)
        {
            TransformJoint(child, globalTransform, keyframe);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!animate)
        {
            return;
        }

        _timeSinceLastFrame += Time.deltaTime;

        var advancedFrames = (int)(_timeSinceLastFrame / data.frameLength);

        if (advancedFrames == 0)
        {
            return;
        }

        currFrame = (currFrame + advancedFrames) % data.numFrames;

        var currentKeyframe = data.keyframes[currFrame];

        var rootPosition = new Vector3(currentKeyframe[data.rootJoint.positionChannels.x],
            currentKeyframe[data.rootJoint.positionChannels.y],
            currentKeyframe[data.rootJoint.positionChannels.z]);
        var rootTranslation = MatrixUtils.Translate(rootPosition);

        TransformJoint(data.rootJoint, rootTranslation, currentKeyframe);

        _timeSinceLastFrame -= advancedFrames * data.frameLength;
    }
}
