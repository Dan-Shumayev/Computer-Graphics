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
    // Your implementation
}

// Checks for an intersection between a ray and a plane
// The plane passes through point c and has a surface normal n
// The material returned is either m1 or m2 in a way that creates a checkerboard pattern 
void intersectPlaneCheckered(Ray ray, inout RayHit bestHit, Material m1, Material m2, float3 c, float3 n)
{
    // Your implementation
}


// Checks for an intersection between a ray and a triangle
// The triangle is defined by points a, b, c
void intersectTriangle(Ray ray, inout RayHit bestHit, Material material, float3 a, float3 b, float3 c)
{
    // Your implementation
}


// Checks for an intersection between a ray and a 2D circle
// The circle center is given by circle.xyz, its radius is circle.w and its orientation vector is n 
void intersectCircle(Ray ray, inout RayHit bestHit, Material material, float4 circle, float3 n)
{
    // Your implementation
}


// Checks for an intersection between a ray and a cylinder aligned with the Y axis
// The cylinder center is given by cylinder.xyz, its radius is cylinder.w and its height is h
void intersectCylinderY(Ray ray, inout RayHit bestHit, Material material, float4 cylinder, float h)
{
    // Your implementation
}
