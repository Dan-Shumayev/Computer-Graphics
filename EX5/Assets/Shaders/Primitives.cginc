// Checks for an intersection between a ray and a sphere
// The sphere center is given by sphere.xyz and its radius is sphere.w
void intersectSphere(Ray ray, inout RayHit bestHit, Material material, float4 sphere)
{
    float A = 1;

    float B = 2 * dot(ray.origin - sphere.xyz, ray.direction);

    float C = dot(ray.origin - sphere.xyz, ray.origin - sphere.xyz) - pow(sphere.w, 2);

    float D = pow(B, 2) - 4 * A * C;
    if (D < 0)
    {
        return;
    }

    float t0 = (-B + sqrt(D)) / (2 * A);
    float t1 = (-B - sqrt(D)) / (2 * A);

    float t = 0;
    if (t0 < 0 && t1 < 0)
    {
        return;
    }
    else if (t0 < 0 || t1 < 0)
    {
        t = max(t0, t1);
    }
    else
    {
        t = min(t0, t1);
    }

    if (t >= bestHit.distance)
    {
        return;
    }

    bestHit.position = ray.origin + t * ray.direction;
    bestHit.distance = t;
    bestHit.normal = normalize(bestHit.position - sphere.xyz);
    bestHit.material = material;
}

// Checks for an intersection between a ray and a plane
// The plane passes through point c and has a surface normal n
void intersectPlane(Ray ray, inout RayHit bestHit, Material material, float3 c, float3 n)
{
    if (0 == dot(ray.direction, n))
    {
        return;
    }

    float t = -dot(ray.origin - c, n) / dot(ray.direction, n);

    if (t < 0)
    {
        return;
    }

    if (t >= bestHit.distance)
    {
        return;
    }

    bestHit.position = ray.origin + t * ray.direction;
    bestHit.distance = t;
    bestHit.normal = n;
    bestHit.material = material;
}

// Checks for an intersection between a ray and a plane
// The plane passes through point c and has a surface normal n
// The material returned is either m1 or m2 in a way that creates a checkerboard pattern
void intersectPlaneCheckered(Ray ray, inout RayHit bestHit, Material m1, Material m2, float3 c, float3 n)
{
    // TODO: explain this in the README

    //
    // Find intersection between the ray and the plane
    //

    RayHit planeHit = CreateRayHit();
    intersectPlane(ray, planeHit, m1, c, n);

    if (isinf(planeHit.distance))
    {
        // No hit
        return;
    }

    if (planeHit.distance >= bestHit.distance)
    {
        // Even if this point is on the plane, it's worse than the current
        // best hit. No sense in checking further.
        return;
    }

    //
    // Find the material to use
    //

    float u;
    float v;
    if (1 == abs(n.x))  // YZ plane
    {
        u = dot(planeHit.position - c, float3(0, 0, 1));
        v = dot(planeHit.position - c, float3(0, 1, 0));
    }
    else if (1 == abs(n.y))  // XZ plane
    {
        u = dot(planeHit.position - c, float3(1, 0, 0));
        v = dot(planeHit.position - c, float3(0, 0, 1));
    }
    else  // XY plane
    {
        u = dot(planeHit.position - c, float3(1, 0, 0));
        v = dot(planeHit.position - c, float3(0, 1, 0));
    }

    uint material = ((int)floor(u / 0.5) + (int)floor(v / 0.5)) % 2;
    if (material)
    {
        planeHit.material = m2;
    }
    else
    {
        planeHit.material = m1;
    }

    bestHit = planeHit;
}


// Checks for an intersection between a ray and a triangle
// The triangle is defined by points a, b, c
void intersectTriangle(Ray ray, inout RayHit bestHit, Material material, float3 a, float3 b, float3 c)
{
    float3 n = normalize(cross(a - c, b - c));

    //
    // Find intersection between the ray and the plane of the triangle
    //

    RayHit planeHit = CreateRayHit();
    intersectPlane(ray, planeHit, material, a, n);

    if (isinf(planeHit.distance))
    {
        // No hit
        return;
    }

    if (planeHit.distance >= bestHit.distance)
    {
        // Even if this point is within the triangle, it's worse than the current
        // best hit. No sense in checking further.
        return;
    }

    //
    // Check that the hit point lies within the triangle
    //

    if (dot(cross(b - a, planeHit.position - a), n) < 0)
    {
        return;
    }

    if (dot(cross(c - b, planeHit.position - b), n) < 0)
    {
        return;
    }

    if (dot(cross(a - c, planeHit.position - c), n) < 0)
    {
        return;
    }

    // Everything checks out

    bestHit = planeHit;
}


// Checks for an intersection between a ray and a 2D circle
// The circle center is given by circle.xyz, its radius is circle.w and its orientation vector is n
void intersectCircle(Ray ray, inout RayHit bestHit, Material material, float4 circle, float3 n)
{
    //
    // Find intersection between the ray and the plane of the circle
    //

    RayHit planeHit = CreateRayHit();
    intersectPlane(ray, planeHit, material, circle.xyz, n);

    if (isinf(planeHit.distance))
    {
        // No hit
        return;
    }

    if (planeHit.distance >= bestHit.distance)
    {
        // Even if this point is within the circle, it's worse than the current
        // best hit. No sense in checking further.
        return;
    }

    //
    // Check that the hit point lies within the circle
    //

    if (distance(planeHit.position, circle.xyz) > circle.w)
    {
        return;
    }

    // Everything checks out

    bestHit = planeHit;
}


// Checks for an intersection between a ray and a cylinder aligned with the Y axis
// The cylinder center is given by cylinder.xyz, its radius is cylinder.w and its height is h
// Intersection with the cylinder caps *is not* checked
void intersectCylinderYWithoutCaps(Ray ray, inout RayHit bestHit, Material material, float4 cylinder, float h)
{
    float A = dot(ray.direction.xz, ray.direction.xz);

    float B = 2 * dot(ray.origin.xz - cylinder.xz, ray.direction.xz);

    float C = dot(ray.origin.xz - cylinder.xz, ray.origin.xz - cylinder.xz) - pow(cylinder.w, 2);

    float D = pow(B, 2) - 4 * A * C;
    if (D < 0)
    {
        return;
    }

    float t0 = (-B + sqrt(D)) / (2 * A);
    float t1 = (-B - sqrt(D)) / (2 * A);

    float t = 0;
    if (t0 < 0 && t1 < 0)
    {
        return;
    }
    else if (t0 < 0 || t1 < 0)
    {
        t = max(t0, t1);
    }
    else
    {
        t = min(t0, t1);
    }

    if (t >= bestHit.distance)
    {
        return;
    }

    float3 position = ray.origin + t * ray.direction;

    if (distance(position.y, cylinder.y) > h / 2)
    {
        return;
    }

    bestHit.position = position;
    bestHit.distance = t;
    bestHit.normal = normalize(float3(position.x, 0, position.z) - float3(cylinder.x, 0, cylinder.z));
    bestHit.material = material;
}

// Checks for an intersection between a ray and a cylinder aligned with the Y axis
// The cylinder center is given by cylinder.xyz, its radius is cylinder.w and its height is h
void intersectCylinderY(Ray ray, inout RayHit bestHit, Material material, float4 cylinder, float h)
{
    // TODO: explain this in the README

    intersectCylinderYWithoutCaps(ray, bestHit, material, cylinder, h);

    // Intersect with the top cap
    intersectCircle(ray, bestHit, material, float4(cylinder.x, cylinder.y + h / 2, cylinder.z, cylinder.w), float3(0, 1, 0));

    // Intersect with the bottom cap
    intersectCircle(ray, bestHit, material, float4(cylinder.x, cylinder.y - h / 2, cylinder.z, cylinder.w), float3(0, -1, 0));
}
