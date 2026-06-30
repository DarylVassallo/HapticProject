Shader "Custom/RevealingUnderLight_URP"
{
    Properties
    {
        _ReverseStrength("Reverse Strength", Integer) = 0
        
        _MapBlend("Blend", Range(0,1)) = 0
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1,1,1,1)
	   [MainTexture] _AlternateBaseMap("Alternate Base Map", 2D) = "white" {}
        [MainColor] _AlternateBaseColor("Alternate Base Color", Color) = (1,1,1,1)

                
        _Smoothness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
        
        [NoScaleOffset] _MetallicGlossMap("Metallic Map", 2D) = "white" {}
	   [NoScaleOffset][Normal] _BumpMap("Normal Map", 2D) = "bump" {}
	   _BumpScale("Normal Scale", Float) = 1.0
	   [NoScaleOffset] _OcclusionMap("Occlusion Map", 2D) = "white" {}
	   _OcclusionStrength("Occlusion Strength", Range(0,1)) = 1.0

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

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
		 TEXTURE2D(_AlternateBaseMap);
            SAMPLER(sampler_AlternateBaseMap);

	            
            TEXTURE2D(_MetallicGlossMap);
		 SAMPLER(sampler_MetallicGlossMap);
		
		 TEXTURE2D(_BumpMap);
		 SAMPLER(sampler_BumpMap);
		
		 TEXTURE2D(_OcclusionMap);
		 SAMPLER(sampler_OcclusionMap);

            CBUFFER_START(UnityPerMaterial)
                int _ReverseStrength;
                
                float _MapBlend;
                float4 _BaseColor;
                float4 _AlternateBaseColor;
                float4 _BaseMap_ST;

                float _Smoothness;
                float _Metallic;
                float _BumpScale;
			float _OcclusionStrength;

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
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
                
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 positionWS  : TEXCOORD1;
                float3 normalWS    : TEXCOORD2;
                float4 tangentWS   : TEXCOORD3;
                
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert (Attributes v)
            {
                Varyings o;
                
                UNITY_SETUP_INSTANCE_ID(v);
    			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    
                o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionHCS = TransformWorldToHClip(o.positionWS);
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                float3 tangentWS = TransformObjectToWorldDir(v.tangentOS.xyz);
			o.tangentWS = float4(tangentWS, v.tangentOS.w * GetOddNegativeScale());
                
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
            	
            	float3 geomNormal = normalize(i.normalWS);
		     float3 tangentWS = normalize(i.tangentWS.xyz);
		     float3 bitangentWS = cross(geomNormal, tangentWS) * i.tangentWS.w;
		     float3x3 TBN = float3x3(tangentWS, bitangentWS, geomNormal);
		
		     half4 normalSample = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, i.uv);
		     float3 normalTS = UnpackNormalScale(normalSample, _BumpScale);
		     float3 normal = normalize(mul(normalTS, TBN));
		
		     half4 metallicGloss = SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, i.uv);
		     float metallic = metallicGloss.r * _Metallic;
		     float smoothness = metallicGloss.a * _Smoothness;
		
		     half occlusionSample = SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, i.uv).g;
		     float occlusion = lerp(1.0, occlusionSample, _OcclusionStrength);
            	
            	float pcStrength = ComputeStrength(	_PCLightPosition.xyz,
            								_PCLightDirection.xyz,
            								_PCLightAngle,
            								_PCLightRange,
            								_PCStrengthScalor,
            								i.positionWS);
            	float pcNdotL = saturate(dot(normal, normalize(_PCLightDirection.xyz)));
            	
			float vrStrength = ComputeStrength(	_VRLightPosition.xyz,
            								_VRLightDirection.xyz,
            								_VRLightAngle,
            								_VRLightRange,
            								_VRStrengthScalor,
            								i.positionWS);
            	float vrNdotL = saturate(dot(normal, normalize(_VRLightDirection.xyz)));
            	            	
            	float3 lighting = 	pcNdotL * pcStrength * float3(1,1,1) + 
            					vrNdotL * vrStrength * float3(1,1,1);
            	
            	float strength = saturate(pcStrength + vrStrength);
            	strength = lerp(strength, 1.0 - strength, _ReverseStrength);
            	
                half4 texOriginal = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv) * _BaseColor;
                half4 texAlternate = SAMPLE_TEXTURE2D(_AlternateBaseMap, sampler_AlternateBaseMap, i.uv) * _AlternateBaseColor;
                half4 tex = lerp(texOriginal, texAlternate, _MapBlend);

                float3 albedo = tex.rgb;
                
			float3 diffuseAlbedo = albedo * (1.0 - metallic);
			
                float alpha = strength * tex.a;
                float3 emission = diffuseAlbedo * tex.a * strength;

			float3 litColor = diffuseAlbedo * lighting;
			
			litColor *= occlusion;
			emission *= occlusion;
			
                half4 col;
                col.rgb = litColor + emission;
                col.a = alpha;

                return col;
            }

            ENDHLSL
        }
    }
}