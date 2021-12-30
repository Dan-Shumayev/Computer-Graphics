Shader "CG/Earth"
{
    Properties
    {
        [NoScaleOffset] _AlbedoMap ("Albedo Map", 2D) = "defaulttexture" {}
        _Ambient ("Ambient", Range(0, 1)) = 0.15
        [NoScaleOffset] _SpecularMap ("Specular Map", 2D) = "defaulttexture" {}
        _Shininess ("Shininess", Range(0.1, 100)) = 50
        [NoScaleOffset] _HeightMap ("Height Map", 2D) = "defaulttexture" {}
        _BumpScale ("Bump Scale", Range(1, 100)) = 30
        [NoScaleOffset] _CloudMap ("Cloud Map", 2D) = "black" {}
        _AtmosphereColor ("Atmosphere Color", Color) = (0.8, 0.85, 1, 1)
    }
    SubShader
    {
        Pass
        {
            Tags { "LightMode" = "ForwardBase" }

            CGPROGRAM

                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"
                #include "CGUtils.cginc"

                // Declare used properties
                uniform sampler2D _AlbedoMap;
                uniform float _Ambient;
                uniform sampler2D _SpecularMap;
                uniform float _Shininess;
                uniform sampler2D _HeightMap;
                uniform float4 _HeightMap_TexelSize;
                uniform float _BumpScale;
                uniform sampler2D _CloudMap;
                uniform fixed4 _AtmosphereColor;

                struct appdata
                {
                    float4 vertex : POSITION;
                };

                struct v2f
                {
                    float4 pos           : SV_POSITION;
                    float3 object_normal : TEXCOORD0;
                    float3 world_pos     : TEXCOORD1;
                };

                v2f vert (appdata input)
                {
                    float4 earth_center = float4(0, 0, 0, 1);

                    v2f output;
                    output.pos = UnityObjectToClipPos(input.vertex);
                    output.object_normal = input.vertex - earth_center;
                    output.world_pos = mul(unity_ObjectToWorld, input.vertex);
                    return output;
                }

                fixed4 frag (v2f input) : SV_Target
                {
                    float2 uv = getSphericalUV(input.object_normal);

                    float3 viewDir = normalize(_WorldSpaceCameraPos - input.world_pos);
                    float3 lightDir = normalize(_WorldSpaceLightPos0);
                    float3 halfWayVec = normalize(lightDir + viewDir);
                    float3 normal = normalize(mul(unity_ObjectToWorld, input.object_normal));

                    bumpMapData bump;
                    bump.normal = normal;
                    bump.tangent = normalize(cross(normal, float3(0, 1, 0)));
                    bump.uv = uv;
                    bump.heightMap = _HeightMap;
                    bump.du = _HeightMap_TexelSize.x;
                    bump.dv = _HeightMap_TexelSize.y;
                    bump.bumpScale = _BumpScale / 10000;

                    fixed4 specular = tex2D(_SpecularMap, uv);

                    float3 final_normal = (1 - specular) * getBumpMappedNormal(bump)
                                          + specular * normal;

                    fixed3 blinn = blinnPhong(final_normal,
                                              halfWayVec,
                                              lightDir,
                                              _Shininess,
                                              tex2D(_AlbedoMap, uv),
                                              specular,
                                              _Ambient);

                    float3 lambert = max(0, dot(normal, lightDir));
                    fixed3 atmosphere = (1 - max(0, dot(normal, viewDir))) * sqrt(lambert) * _AtmosphereColor.xyz;
                    fixed3 clouds = tex2D(_CloudMap, uv) * (sqrt(lambert) + _Ambient);

                    return fixed4(blinn + atmosphere + clouds, 1);
                }

            ENDCG
        }
    }
}
