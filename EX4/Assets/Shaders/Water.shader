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
                };

                // Returns the value of a noise function simulating water, at coordinates uv and time t
                float waterNoise(float2 uv, float t)
                {
                    return perlin2d(uv);
                }

                // Returns the world-space bump-mapped normal for the given bumpMapData and time t
                float3 getWaterBumpMappedNormal(bumpMapData i, float t)
                {
                    // Your implementation
                    return 0;
                }


                v2f vert (appdata input)
                {
                    v2f output;
                    float3 displacement = normalize(input.normal) * _BumpScale * perlin2d(_NoiseScale * input.uv);
                    output.pos = UnityObjectToClipPos(input.vertex + displacement);
                    output.uv = input.uv;
                    output.normal = mul(unity_ObjectToWorld, input.normal);
                    output.world_pos = mul(unity_ObjectToWorld, input.vertex);
                    return output;
                }

                fixed4 frag (v2f input) : SV_Target
                {
                    float3 v = normalize(_WorldSpaceCameraPos - input.world_pos);
                    float3 n = normalize(input.normal);
                    float3 r = 2 * dot(v, n) * n - v;

                    half4 reflected_color = texCUBE(_CubeMap, r);

                    half4 color = (1 - max(0, dot(n, v)) + 0.2) * reflected_color;

                    return color;
                }

            ENDCG
        }
    }
}
