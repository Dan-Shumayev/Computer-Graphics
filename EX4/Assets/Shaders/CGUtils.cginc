#ifndef CG_UTILS_INCLUDED
#define CG_UTILS_INCLUDED

#define PI 3.141592653

// A struct containing all the data needed for bump-mapping
struct bumpMapData
{ 
    float3 normal;       // Mesh surface normal at the point
    float3 tangent;      // Mesh surface tangent at the point
    float2 uv;           // UV coordinates of the point
    sampler2D heightMap; // Heightmap texture to use for bump mapping
    float du;            // Increment size for u partial derivative approximation
    float dv;            // Increment size for v partial derivative approximation
    float bumpScale;     // Bump scaling factor
};


// Receives pos in 3D cartesian coordinates (x, y, z)
// Returns UV coordinates corresponding to pos using spherical texture mapping
float2 getSphericalUV(float3 pos)
{
    float theta = atan2(pos.z, pos.x);
    float phi = acos(pos.y / length(pos));

    float u = 0.5 + theta / (2 * PI);
    float v = 1 - phi / PI;

    return float2(u, v);
}

// Implements an adjusted version of the Blinn-Phong lighting model
fixed3 blinnPhong(float3 n, float3 h, float3 l, float shininess, fixed4 albedo, fixed4 specularity, float ambientIntensity)
{
    fixed4 ambient = ambientIntensity * albedo;
    fixed4 diffuse = max(0, dot(n, l)) * albedo;
    fixed4 specular = pow(max(0, dot(n, h)), shininess) * specularity;
    return ambient + diffuse + specular;
}

// Returns the world-space bump-mapped normal for the given bumpMapData
float3 getBumpMappedNormal(bumpMapData i)
{
    float fp = tex2D(i.heightMap, i.uv);
    float fu = (tex2D(i.heightMap, float2(i.uv.x + i.du, i.uv.y)) - fp) / i.du;
    float fv = (tex2D(i.heightMap, float2(i.uv.x, i.uv.y + i.dv)) - fp) / i.dv;

    // TODO: Doing this manually works fine, but using cross(tv, tu)
    //       (with tv and tu defined as in the TA) gives a *negated* result.
    //       No idea why.
    float3 nh = normalize(float3(-i.bumpScale * fu, -i.bumpScale * fv, 1));

    float3 binormal = normalize(cross(i.tangent, i.normal));

    float3 n_world = normalize(i.tangent * nh.x + i.normal * nh.z + binormal * nh.y);

    return n_world;
}


#endif // CG_UTILS_INCLUDED
