intersectPlaneCheckered
========================

First, observe that the problem boils down to finding the Manhattan distance between
the hit point and the plane's "origin", going by squares with side 0.5.
If the distance is even, the corresponding square is black. Otherwise, it is white.

Thus, we take the vector from the plane's origin to the hit point, project it on
the plane's axes (e.g. for the XZ plane, we project on X and on Z), divide
each coordinate by 0.5, and floor it. If the original hit point coordinate was
positive relative to the plane origin, this yield the Manhattan distance in that
coordinate's direction. If the coordinate was negative, this yields the Manhattan
distance + 1. For instance:

1. If the hit point was at (0.3, 0.7) we get (0, 1).
2. If the hit point was at (-0.3, 0.7) we get (-1, 1).

This is a good thing! Since for negative coordinates the checkerboard pattern
is inverted: even squares are white, and odd are black.

Now, we add the two coordinates, divide by 2, and check the fractional part. If it is 0,
we use material 1. Otherwise, we use material 2.

Wait, but shouldn't we take the absolute value of each coordinate before adding them?
After all, Manhattan distance is a non-negative value. Techincally that is correct, but
we only care about the parity of the sum, and that is not affected by the signs of the
operands.


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
