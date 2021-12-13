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
                    float3 object_normal : NORMAL;
                };

                v2f vert (appdata input)
                {
                    v2f output;
                    output.pos = UnityObjectToClipPos(input.vertex);
                    // TODO: Is this correct? Looks okay, but the specular highlight
                    //       is not in the exact same position as in the exercise description.
                    output.object_normal = input.vertex;
                    return output;
                }

                fixed4 frag (v2f input) : SV_Target
                {
                    // TODO: There's a strange discontinuity on the west side of the Earth.

                    float2 uv = getSphericalUV(input.object_normal);

                    float3 v = normalize(_WorldSpaceCameraPos);
                    float3 l = normalize(_WorldSpaceLightPos0);
                    float3 h = normalize((l + v) / 2);
                    float3 n = normalize(mul(unity_ObjectToWorld, input.object_normal));

                    bumpMapData bump;
                    bump.normal = n;
                    bump.tangent = normalize(cross(n, float3(0, 1, 0)));
                    bump.uv = uv;
                    bump.heightMap = _HeightMap;
                    bump.du = _HeightMap_TexelSize[0];
                    bump.dv = _HeightMap_TexelSize[1];
                    bump.bumpScale = _BumpScale / 10000;

                    fixed4 specular = tex2D(_SpecularMap, uv);

                    float3 final_normal = (1 - specular) * getBumpMappedNormal(bump)
                                          + specular * n;

                    fixed3 blinn = blinnPhong(final_normal,
                                              h,
                                              l,
                                              _Shininess,
                                              tex2D(_AlbedoMap, uv),
                                              specular,
                                              _Ambient);

                    float3 lambert = max(0, dot(n, l));
                    fixed3 atmosphere = (1 - max(0, dot(n, v))) * sqrt(lambert) * _AtmosphereColor.xyz;
                    fixed3 clouds = tex2D(_CloudMap, uv) * (sqrt(lambert) + _Ambient);

                    // TODO: Not sure about setting the alpha to 1 here.
                    //       Maybe blinnPhong should actually return fixed4 and not fixed3?
                    return fixed4(blinn + atmosphere + clouds, 1);
                }

            ENDCG
        }
    }
}
