Shader "CG/Bricks"
{
    Properties
    {
        [NoScaleOffset] _AlbedoMap ("Albedo Map", 2D) = "defaulttexture" {}
        _Ambient ("Ambient", Range(0, 1)) = 0.15
        [NoScaleOffset] _SpecularMap ("Specular Map", 2D) = "defaulttexture" {}
        _Shininess ("Shininess", Range(0.1, 100)) = 50
        [NoScaleOffset] _HeightMap ("Height Map", 2D) = "defaulttexture" {}
        _BumpScale ("Bump Scale", Range(-100, 100)) = 40
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

                struct appdata
                { 
                    float4 vertex   : POSITION;
                    float3 normal   : NORMAL;
                    float4 tangent  : TANGENT;
                    float2 uv       : TEXCOORD0;
                };

                struct v2f
                {
                    float4 pos     : SV_POSITION;
                    float3 normal  : NORMAL;
                    float4 tangent : TANGENT;
                    float2 uv      : TEXCOORD0;
                };

                v2f vert (appdata input)
                {
                    v2f output;
                    output.pos = UnityObjectToClipPos(input.vertex);
                    output.normal = normalize(mul(unity_ObjectToWorld, input.normal));
                    output.tangent = normalize(mul(unity_ObjectToWorld, input.tangent));
                    output.uv = input.uv;
                    return output;
                }

                fixed4 frag (v2f input) : SV_Target
                {
                    float3 l = normalize(_WorldSpaceLightPos0);
                    float3 h = normalize((l + normalize(_WorldSpaceCameraPos)) / 2);

                    bumpMapData bump;
                    bump.normal = normalize(input.normal);
                    bump.tangent = normalize(input.tangent);
                    bump.uv = input.uv;
                    bump.heightMap = _HeightMap;
                    bump.du = _HeightMap_TexelSize[0];
                    bump.dv = _HeightMap_TexelSize[1];
                    bump.bumpScale = _BumpScale / 10000;

                    fixed3 color = blinnPhong(getBumpMappedNormal(bump),
                                              h,
                                              l,
                                              _Shininess,
                                              tex2D(_AlbedoMap, input.uv),
                                              tex2D(_SpecularMap, input.uv),
                                              _Ambient);

                    // TODO: Not sure about setting the alpha to 1 here.
                    //       Maybe blinnPhong should actually return fixed4 and not fixed3?
                    return fixed4(color, 1);
                }

            ENDCG
        }
    }
}
