intersectCylinderY
===================

In essence, a Y-axis aligned cylinder consists of three parts:

1. A round body centered on point C, with height H, and radius R.
2. A top ciruclar cap with radius R, centered at (C.x, C.y + H/2, C.z).
3. A bottom ciruclar cap with radius R, centered at (C.x, C.y - H/2, C.z).

Thus, to find a ray's intersection point with the cylinder, we need to compute
intersections with all three of these parts, and pick the closest. An intersection with
a cap can be computed using intersectCircle, which reduces to finding
the intersection between the ray and the cap's plane, and checking that the hit point
lies within the cap's radius.

Intersections with the cylinder body can be computed by substituting the ray's formula
into the infinite cylinder's equation, and solving for t. This results in a quadratic
equation in t (see https://imgur.com/a/mlXPkP6), from which we can pick the smallest
non-negative solution (if it exists). Then, we only have to check that the hit point
is at a distance of at most H/2 along the Y-axis from the cylinder's center.
