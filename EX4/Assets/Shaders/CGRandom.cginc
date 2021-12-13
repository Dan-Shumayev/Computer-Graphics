#ifndef CG_RANDOM_INCLUDED
// Upgrade NOTE: excluded shader from DX11 because it uses wrong array syntax (type[size] name)
#pragma exclude_renderers d3d11
#define CG_RANDOM_INCLUDED

// Returns a psuedo-random float between -1 and 1 for a given float c
float random(float c)
{
    return -1.0 + 2.0 * frac(43758.5453123 * sin(c));
}

// Returns a psuedo-random float2 with componenets between -1 and 1 for a given float2 c 
float2 random2(float2 c)
{
    c = float2(dot(c, float2(127.1, 311.7)), dot(c, float2(269.5, 183.3)));

    float2 v = -1.0 + 2.0 * frac(43758.5453123 * sin(c));
    return v;
}

// Returns a psuedo-random float3 with componenets between -1 and 1 for a given float3 c 
float3 random3(float3 c)
{
    float j = 4096.0 * sin(dot(c, float3(17.0, 59.4, 15.0)));
    float3 r;
    r.z = frac(512.0*j);
    j *= .125;
    r.x = frac(512.0*j);
    j *= .125;
    r.y = frac(512.0*j);
    r = -1.0 + 2.0 * r;
    return r.yzx;
}

// Interpolates a given array v of 4 float2 values using bicubic interpolation
// at the given ratio t (a float2 with components between 0 and 1)
//
// [0]=====o==[1]
//         |
//         t
//         |
// [2]=====o==[3]
//
float bicubicInterpolation(float2 v[4], float2 t)
{
    float2 u = t * t * (3.0 - 2.0 * t); // Cubic interpolation

    // Interpolate in the x direction
    float x1 = lerp(v[0], v[1], u.x);
    float x2 = lerp(v[2], v[3], u.x);

    // Interpolate in the y direction and return
    return lerp(x1, x2, u.y);
}

// Interpolates a given array v of 4 float2 values using biquintic interpolation
// at the given ratio t (a float2 with components between 0 and 1)
float biquinticInterpolation(float2 v[4], float2 t)
{
    float2 u = t * t * t * (6 * t * t - 15 * t + 10); // Quintic interpolation

    // Interpolate in the x direction
    float x1 = lerp(v[0], v[1], u.x);
    float x2 = lerp(v[2], v[3], u.x);

    // Interpolate in the y direction and return
    return lerp(x1, x2, u.y);
}

// Interpolates a given array v of 8 float3 values using triquintic interpolation
// at the given ratio t (a float3 with components between 0 and 1)
float triquinticInterpolation(float3 v[8], float3 t)
{
    // Your implementation
    return 0;
}

float4x4 cellCorners(float2 c)
{
    float2 cell_origin = floor(c);

	return float4x4(
        float4(cell_origin, 0, 0),
        float4(float2(cell_origin.x + 1, cell_origin.y), 0, 0),
        float4(float2(cell_origin.x, cell_origin.y + 1), 0, 0),
        float4(cell_origin + 1, 0, 0)
    );
}

// Returns the value of a 2D value noise function at the given coordinates c
float value2d(float2 c)
{
    // TODO: This doesn't look *quite* like the expected picture.

    float4x4 corners = cellCorners(c);
    
    float2 color00 = random2(corners[0])[0];
    float2 color10 = random2(corners[1])[0];
    float2 color01 = random2(corners[2])[0];
    float2 color11 = random2(corners[3])[0];
    float2 colors[] = { color00, color10, color01, color11 };

    return bicubicInterpolation(colors, frac(c));
}

// Returns the value of a 2D Perlin noise function at the given coordinates c
float perlin2d(float2 c)
{
    // TODO: This doesn't look *quite* like the expected picture.

    float4x4 corners = cellCorners(c);
    
    float2 grad00 = random2(corners[0]);
    float2 grad10 = random2(corners[1]);
    float2 grad01 = random2(corners[2]);
    float2 grad11 = random2(corners[3]);

    float2 distance00 = c - corners[0];
    float2 distance10 = c - corners[1];
    float2 distance01 = c - corners[2];
    float2 distance11 = c - corners[3];

    float2 influence00 = dot(distance00, grad00);
    float2 influence10 = dot(distance10, grad10);
    float2 influence01 = dot(distance01, grad01);
    float2 influence11 = dot(distance11, grad11);

    float2 influences[] = { influence00, influence10, influence01, influence11 };

    return biquinticInterpolation(influences, frac(c));
}

// Returns the value of a 3D Perlin noise function at the given coordinates c
float perlin3d(float3 c)
{                    
    // Your implementation
    return 0;
}


#endif // CG_RANDOM_INCLUDED
