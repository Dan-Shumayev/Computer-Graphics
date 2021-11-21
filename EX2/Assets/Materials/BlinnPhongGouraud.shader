Shader "CG/BlinnPhongGouraud"
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

                struct appdata
                { 
                    float4 vertex : POSITION;
                    float3 normal : NORMAL;
                };

                struct v2f
                {
                    float4 pos      : SV_POSITION;
                    fixed4 color    : COLOR0;
                };


                v2f vert (appdata input)
                {
                    float3 world_space_normal = normalize(mul(unity_ObjectToWorld, input.normal));
                    float3 world_light_direction = normalize(_WorldSpaceLightPos0);

                    fixed4 color_d = max(dot(world_light_direction, world_space_normal), 0) * _DiffuseColor * _LightColor0;

                    float3 h = normalize((world_light_direction + normalize(_WorldSpaceCameraPos)) / 2);
                    fixed4 color_s = pow(max(dot(world_space_normal, h), 0), _Shininess) * _SpecularColor * _LightColor0;

                    fixed4 color_a = _AmbientColor * _LightColor0;

                    v2f output;
                    output.pos = UnityObjectToClipPos(input.vertex);
                    output.color = color_d + color_s + color_a;
                    return output;
                }


                fixed4 frag (v2f input) : SV_Target
                {
                    return input.color;
                }

            ENDCG
        }
    }
}
