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
                    float4 vertex    : SV_POSITION;
                    float3 normal    : TEXCOORD0;
                    float4 tangent   : TEXCOORD1;
                    float2 uv        : TEXCOORD2;
                    float3 world_pos : TEXCOORD3;
                };

                v2f vert (appdata input)
                {
                    v2f output;
                    
                    output.vertex = UnityObjectToClipPos(input.vertex);
                    output.normal = mul(unity_ObjectToWorld, input.normal);
                    output.tangent = mul(unity_ObjectToWorld, input.tangent);
                    output.uv = input.uv;
                    output.world_pos = mul(unity_ObjectToWorld, input.vertex);

                    return output;
                }

                fixed4 frag (v2f input) : SV_Target
                {
                    float3 lightDir = normalize(_WorldSpaceLightPos0);
                    float3 viewDir = normalize(_WorldSpaceCameraPos - input.world_pos);
                    float3 halfWayVec = normalize(lightDir + viewDir);

                    bumpMapData bump = {
                        normalize(input.normal).xyz,
                        normalize(input.tangent).xyz,
                        input.uv,
                        _HeightMap,
                        _HeightMap_TexelSize.x,
                        _HeightMap_TexelSize.y,
                        _BumpScale / 10000
                    };

                    // Alpha channel is probably a dummy value, so let it be 1/0.
                    //  Consider changing the ``BlinnPhong``'s return value to fixed4,
                    //      which makes sense in respect to its arguments' types.
                    return fixed4(blinnPhong(getBumpMappedNormal(bump),
                                                halfWayVec,
                                                lightDir,
                                                _Shininess,
                                                tex2D(_AlbedoMap, input.uv),
                                                tex2D(_SpecularMap, input.uv),
                                                _Ambient), 1);
                }

            ENDCG
        }
    }
}
