﻿Shader "CG/BlinnPhong"
{
    Properties
    {
        _DiffuseColor ("Diffuse Color", Color) = (0.14, 0.43, 0.84, 1)
        _SpecularColor ("Specular Color", Color) = (0.7, 0.7, 0.7, 1)
        _AmbientColor ("Ambient Color", Color) = (0.05, 0.13, 0.25, 1)
        _Shininess ("Shininess", Range(0.1, 50)) = 10
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

                // From UnityCG
                uniform fixed4 _LightColor0;

                // Declare used properties
                uniform fixed4 _DiffuseColor;
                uniform fixed4 _SpecularColor;
                uniform fixed4 _AmbientColor;
                uniform float _Shininess;

                struct appdata {
                    float4 vertex : POSITION;
                    float3 normal : NORMAL;
                };

                struct v2f {
                    float4 pos : SV_POSITION;
                    float3 normalDirection : NORMAL;                    
                };
        
                v2f vert(appdata input) 
                {
                    v2f output;
        
                    output.normalDirection = normalize(mul(unity_ObjectToWorld, input.normal));
                    output.pos = UnityObjectToClipPos(input.vertex);

                    return output;
                }
        
                fixed4 frag(v2f input) : SV_Target
                {
                    // Calculate directions
                    float3 lightDirection = normalize(_WorldSpaceLightPos0);
                    float3 normalDirection = normalize(input.normalDirection);

                    // Calculate illuminated colors
                    fixed4 color_a = _LightColor0 * _AmbientColor;
                    fixed4 color_d = _LightColor0 * _DiffuseColor *
                                                max(0, dot(normalDirection, lightDirection));

                    float3 h = normalize((lightDirection + normalize(_WorldSpaceCameraPos)) / 2);
                    fixed4 color_s = _LightColor0 * _SpecularColor *
                                        pow(max(dot(normalDirection, h), 0), _Shininess);

                    return color_a + color_d + color_s;
                }

            ENDCG
        }
    }
}