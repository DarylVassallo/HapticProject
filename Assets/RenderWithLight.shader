Shader "Custom/RevealingUnderLight_URP"
{
    Properties
    {
        _ReverseStrength("Reverse Strength", Integer) = 0
        
        _MyColor("Color", Color) = (1,1,1,1)
        _MyMainTex("Albedo (RGB)", 2D) = "white" {}

        _MyGlossiness("Smoothness", Range(0,1)) = 0.5
        _MyMetallic("Metallic", Range(0,1)) = 0.0

	   _PCLightRange("Light Range", Float) = 5
        _PCLightDirection("Light Direction", Vector) = (0,0,1,0)
        _PCLightPosition("Light Position", Vector) = (0,0,0,0)
        _PCLightAngle("Light Angle", Range(0,180)) = 45
        _PCStrengthScalor("Strength", Float) = 50

	   _VRLightRange("Light Range", Float) = 5
        _VRLightDirection("Light Direction", Vector) = (0,0,1,0)
        _VRLightPosition("Light Position", Vector) = (0,0,0,0)
        _VRLightAngle("Light Angle", Range(0,180)) = 45
        _VRStrengthScalor("Strength", Float) = 50
        
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
        ZWrite On
        Cull Back

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            
            #pragma multi_compile_instancing
		 #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MyMainTex);
            SAMPLER(sampler_MyMainTex);

            CBUFFER_START(UnityPerMaterial)
                int _ReverseStrength;
                
                float4 _MyColor;
                float4 _MyMainTex_ST;

                float _MyGlossiness;
                float _MyMetallic;

			float _PCLightRange;
                float4 _PCLightDirection;
                float4 _PCLightPosition;
                float _PCLightAngle;
                float _PCStrengthScalor;

			float _VRLightRange;
                float4 _VRLightDirection;
                float4 _VRLightPosition;
                float _VRLightAngle;
                float _VRStrengthScalor;

                
                float4 _RimColor;
                float _RimPower;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 positionWS  : TEXCOORD1;
                float3 normalWS    : TEXCOORD2;
                
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert (Attributes v)
            {
                Varyings o;
                
                UNITY_SETUP_INSTANCE_ID(v);
    			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionHCS = TransformWorldToHClip(o.positionWS);
                o.uv = TRANSFORM_TEX(v.uv, _MyMainTex);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                
                return o;
            }

		 float ComputeStrength(	float3 _currLightPosition, 
		 					float3 _currLightDirection, 
		 					float _currLightAngle, 
		 					float _currLightRange, 
		 					float _currLightStrength,
		 					float3 _worldPosition)
		 {
		 	float dist = distance(_currLightPosition, _worldPosition);
                float3 dir = normalize(_currLightPosition - _worldPosition);
                float3 lightDir = normalize(_currLightDirection);

                float scale = dot(dir, lightDir);

                float angleRad = radians(_currLightAngle* 0.5);
			float threshold = cos(angleRad);
			
			float range = saturate(1.0 - (dist * dist) / (_currLightRange * _currLightRange));
                float strength = scale - threshold;
                strength = saturate(strength * _currLightStrength) * range;

			return strength;
		 }
		 
            half4 frag (Varyings i) : SV_Target
            {
            	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
            	
            	Light mainLight = GetMainLight();
            	float3 normal = normalize(i.normalWS);
            	
            	float NdotL = saturate(dot(normal, mainLight.direction));
            	float3 lighting = mainLight.color * NdotL;
            	
            	float pcStrength = ComputeStrength(	_PCLightPosition.xyz,
            								_PCLightDirection.xyz,
            								_PCLightAngle,
            								_PCLightRange,
            								_PCStrengthScalor,
            								i.positionWS);

			float vrStrength = ComputeStrength(	_VRLightPosition.xyz,
            								_VRLightDirection.xyz,
            								_VRLightAngle,
            								_VRLightRange,
            								_VRStrengthScalor,
            								i.positionWS);
            	
            	float strength = saturate(pcStrength + vrStrength);
            	strength = lerp(strength, 1.0 - strength, _ReverseStrength);
            	
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