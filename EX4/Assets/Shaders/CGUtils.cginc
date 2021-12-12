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
    // Your implementation
    return 0;
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
    // Your implementation
    return 0;
}


#endif // CG_UTILS_INCLUDED
