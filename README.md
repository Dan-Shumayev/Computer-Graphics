## **Computer Graphics projects will be accordingly tagged, shared and managed via this repository**

<details>
<summary><code>Project 1</code> - <b>Motion Capture</b></summary>

- In this project we'll parse a *BVH* file which records a character's motion, in order to move it within a 3D-space `=>` 3D-animation.
    - *Motion Capture* - It's a method of recording actions of various characters in order to be able to animate them in space.
    - *BVH* - A file format storing motion capture data, which consists of the following:
        - **Hierarchy section**: The skeleton of the character to be animated, including its *Joints* and *Bones* (in case of a human being, leg, neck, etc.). The structure of this section is given by a tree (hierarchy), where each node represents a *Joint* of the skeleton. Between every two *Joints* we have a *Bone*. *Joints* with no children are called *End Sites* (parts which are not actually a joint and don't join between anything). There is one *Root Joint* from which all the other joints and bones derive.
            - Each joint contains three records:
                1. *Offset* - the position of the joint relative to its parent joint's location (3D-Vector).
                2. *Channels* - Transformation infomartion required to animate this specific joint.
                3. *Children* - A list of the joint under the said node in the hierarchy.
        - **Motion section**: Defines the way the skeleton described above will move in each frame. The records of this section are *Frames* (amount of frames in animation) and *Frame time* (the time every frame takes). Then, we have lines (*Keyframes*, line for each frame) spanning over till the EOF, which describes the rotation to be applied to every *Joint* (as required by its *Channels* record) chronologically (in respect to the hierarchy order).

        **Note** - Each entry in this line is either a position location or a rotation angle (around a respective axis) of the respective joint.

    - **Building the Skeleton** (Implementation details) - First, we have to parse the *BVH* file to fetch the hierarchy tree objects (*Joints*). Then, we calculate each *Joint* object's 3D-position by adding its offset to its parent's position. Then, at each joint we'll draw a sphere representing it in space. Then, between each joint and its parent we'll draw a cylinder representing the bone linking them.

    - **Animating the Skeleton** (Implementation details) - At each frame we have to build an animation transformation for each of our joints (as in case of moving the hand, we have to animate its fingers as well - each joint is affected by its parent joint). So, first we have to determine for each frame-joint pair its local space transformation, using the following:
        - *T* - Tanslation matrix - Given by the joint's offset
        - *R* - Rotation matrix - Given by the keyframe channel data (**Ordering matters!**)
        - *S* - Scaling matrix - *BVH* doesn't support scaling (Thus, 4x4 Id matrix)
        - We plug them all in `M=TRS` - the order is significant (as we've seen changing it in 3D-space results in different transorm's).
        - Following that, we now have the local transform' *M* describing the joint in local space orientation. However, we would like to get this joint in global space orientation, which is achievable by pre-multiplying *M* by all its parents' global transform' (the first global one is the *Root Joint* which in turn lets its children (one by one) yield their global transform' as well, by pre-multiplying by the root's transform').
</details>
<details>
<summary><code>Project 2</code> - <b>...</b></summary>

- 
</details>
