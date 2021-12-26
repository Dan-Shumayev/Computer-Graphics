Shader "CG/Water"
{
    Properties
    {
        _CubeMap("Reflection Cube Map", Cube) = "" {}
        _NoiseScale("Texture Scale", Range(1, 100)) = 10 
        _TimeScale("Time Scale", Range(0.1, 5)) = 3 
        _BumpScale("Bump Scale", Range(0, 0.5)) = 0.05
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM

                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"
                #include "CGUtils.cginc"
                #include "CGRandom.cginc"

                #define DELTA 0.01

                // Declare used properties
                uniform samplerCUBE _CubeMap;
                uniform float _NoiseScale;
                uniform float _TimeScale;
                uniform float _BumpScale;

                struct appdata
                { 
                    float4 vertex   : POSITION;
                    float3 normal   : NORMAL;
                    float4 tangent  : TANGENT;
                    float2 uv       : TEXCOORD0;
                };

                struct v2f
                {
                    float4 pos       : SV_POSITION;
                    float2 uv        : TEXCOORD0;
                    float3 normal    : TEXCOORD1;
                    float4 world_pos : TEXCOORD2;
                    float4 tangent   : TEXCOORD3;
                };

                // Returns the value of a noise function simulating water, at coordinates uv and time t
                float waterNoise(float2 uv, float t)
                {
                    return perlin3d(0.5 * float3(uv, t)) + 0.5 * perlin3d(float3(uv, t)) + 0.2 * perlin3d(float3(2 * uv, 3 * t));
                }

                // Returns the world-space bump-mapped normal for the given bumpMapData and time t
                float3 getWaterBumpMappedNormal(bumpMapData i, float t)
                {
                    float fp = waterNoise(i.uv, t);
                    float fu = (waterNoise(float2(i.uv.x + i.du, i.uv.y), t) - fp) / i.du;
                    float fv = (waterNoise(float2(i.uv.x, i.uv.y + i.dv), t) - fp) / i.dv;

                    float3 nh = normalize(float3(-i.bumpScale * fu, -i.bumpScale * fv, 1));

                    float3 binormal = normalize(cross(i.tangent, i.normal));

                    float3 n_world = normalize(i.tangent * nh.x + i.normal * nh.z + binormal * nh.y);

                    return n_world;
                }


                v2f vert (appdata input)
                {
                    v2f output;
                    float3 displacement = normalize(input.normal) * _BumpScale * perlin2d(_NoiseScale * input.uv);
                    output.pos = UnityObjectToClipPos(input.vertex + displacement);
                    output.uv = input.uv;
                    output.normal = mul(unity_ObjectToWorld, input.normal);
                    output.world_pos = mul(unity_ObjectToWorld, input.vertex + displacement);
                    output.tangent = mul(unity_ObjectToWorld, input.tangent);
                    return output;
                }

                fixed4 frag (v2f input) : SV_Target
                {
                    bumpMapData bump;
                    bump.normal = normalize(input.normal);
                    bump.tangent = normalize(input.tangent);
                    bump.uv = _NoiseScale * input.uv;
                    bump.du = DELTA;
                    bump.dv = DELTA;
                    bump.bumpScale = _BumpScale;

                    float3 v = normalize(_WorldSpaceCameraPos - input.world_pos);
                    float3 n = getWaterBumpMappedNormal(bump, _TimeScale * _Time.y);
                    float3 r = normalize(2 * dot(v, n) * n - v);

                    half4 reflected_color = texCUBE(_CubeMap, r);

                    half4 color = (1 - max(0, dot(n, v)) + 0.2) * reflected_color;

                    return color;
                }

            ENDCG
        }
    }
}
