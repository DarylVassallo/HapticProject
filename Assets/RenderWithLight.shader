Shader "Custom/RevealingUnderLight_URP"
{
    Properties
    {
        _MyColor("Color", Color) = (1,1,1,1)
        _MyMainTex("Albedo (RGB)", 2D) = "white" {}

        _MyGlossiness("Smoothness", Range(0,1)) = 0.5
        _MyMetallic("Metallic", Range(0,1)) = 0.0

	   _MyLightRange("Light Range", Float) = 5

        _MyLightDirection("Light Direction", Vector) = (0,0,1,0)
        _MyLightPosition("Light Position", Vector) = (0,0,0,0)

        _MyLightAngle("Light Angle", Range(0,180)) = 45
        _MyStrengthScalor("Strength", Float) = 50
        
        _RimColor("Rim Color", Color) = (1,1,1,1)
        _RimPower("Rim Power", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MyMainTex);
            SAMPLER(sampler_MyMainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MyColor;
                float4 _MyMainTex_ST;

                float _MyGlossiness;
                float _MyMetallic;

			float _MyLightRange;
                float4 _MyLightDirection;
                float4 _MyLightPosition;

                float _MyLightAngle;
                float _MyStrengthScalor;
                
                float4 _RimColor;
                float _RimPower;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 positionWS  : TEXCOORD1;
                float3 normalWS    : TEXCOORD2;
            };

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionHCS = TransformWorldToHClip(o.positionWS);
                o.uv = TRANSFORM_TEX(v.uv, _MyMainTex);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
            	Light mainLight = GetMainLight();
            	float3 normal = normalize(i.normalWS);
            	
            	float NdotL = saturate(dot(normal, mainLight.direction));
            	float3 lighting = mainLight.color * NdotL;
            	
            	float dist = distance(_MyLightPosition.xyz, i.positionWS);
                float3 dir = normalize(_MyLightPosition.xyz - i.positionWS);

                float3 lightDir = normalize(_MyLightDirection.xyz);

                float scale = dot(dir, lightDir);

                float angleRad = radians(_MyLightAngle * 0.5);
			float threshold = cos(angleRad);
			
			float range = saturate(1.0 - (dist * dist) / (_MyLightRange * _MyLightRange));
                float strength = scale - threshold;
                strength = saturate(strength * _MyStrengthScalor) * range;

                half4 tex = SAMPLE_TEXTURE2D(_MyMainTex, sampler_MyMainTex, i.uv) * _MyColor;

                float3 albedo = tex.rgb;

                float alpha = strength * tex.a;

                float3 emission = albedo * tex.a * strength;

			float3 litColor = albedo * lighting;
                half4 col;
                col.rgb = litColor + emission;
                col.a = alpha;

                return col;
            }

            ENDHLSL
        }
    }
}