// Implements an adjusted version of the Blinn-Phong lighting model
float3 blinnPhong(float3 n, float3 v, float3 l, float shininess, float3 albedo)
{
    float3 h = normalize(l + v);

    float3 diffuse = max(0, dot(n, l)) * albedo;
    float3 specular = pow(max(0, dot(n, h)), shininess) * 0.4;

    return diffuse + specular;
}

// Reflects the given ray from the given hit point
void reflectRay(inout Ray ray, RayHit hit)
{
    float3 v = -ray.direction;
    float3 r = normalize(2 * dot(v, hit.normal) * hit.normal - v);

    ray.origin = hit.position + EPS * hit.normal;
    ray.direction = r;
    ray.energy *= hit.material.specular;
}

// Refracts the given ray from the given hit point
void refractRay(inout Ray ray, RayHit hit)
{
    float3 n = hit.normal;
    float nu1 = 1;  // Air
    float nu2 = hit.material.refractiveIndex;

    if (dot(n, ray.direction) > 0)
    {
        // Ray exiting material

        n = -n;
        nu1 = hit.material.refractiveIndex;
        nu2 = 1;  // Air
    }

    float nu = nu1 / nu2;
    float c1 = abs(dot(n, ray.direction));
    float c2 = sqrt(1 - nu * nu * (1 - c1 * c1));

    float3 t = nu * ray.direction + (nu * c1 - c2) * n;
    t = normalize(t);

    ray.origin = hit.position - EPS * n;
    ray.direction = t;
}

// Samples the _SkyboxTexture at a given direction vector
float3 sampleSkybox(float3 direction)
{
    float theta = acos(direction.y) / -PI;
    float phi = atan2(direction.x, -direction.z) / -PI * 0.5f;
    return _SkyboxTexture.SampleLevel(sampler_SkyboxTexture, float2(phi, theta), 0).xyz;
}
