Shader "Custom/PS1Effect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _VertexInaccuracy ("Vertex Inaccuracy", Range(0, 200)) = 50
        _DistanciaTeste ("Distancia da Distorcao", Range(0.0, 10)) = 1.0 
        _EmissionMap ("Emission Map", 2D) = "black" {}
        _EmissionColor ("Emission Color", Color) = (0, 0, 0, 1)
        _EmissionPower ("Emission Power", Range(0, 10)) = 1
        _Smoothness ("Smoothness", Range(0, 1)) = 0.0
        _Metallic ("Metallic", Range(0, 1)) = 0.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }
        LOD 100
        Cull Back

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 3.0

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 uv         : TEXCOORD0; 
                float3 worldPos   : TEXCOORD1;
                float2 uv_persp   : TEXCOORD2; 
                float3 normalWS   : TEXCOORD3;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_EmissionMap);
            SAMPLER(sampler_EmissionMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _EmissionMap_ST;
                float  _VertexInaccuracy;
                float  _DistanciaTeste;
                float4 _EmissionColor;
                float  _EmissionPower;
                float  _Smoothness;
                float  _Metallic;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);

                if (_VertexInaccuracy > 0)
                {
                    float gridSize = _VertexInaccuracy;
                    OUT.positionCS.xy = floor(OUT.positionCS.xy * gridSize) / gridSize;
                }

                float2 uv = TRANSFORM_TEX(IN.uv, _MainTex);
                
                OUT.uv.xy = uv * OUT.positionCS.w;
                OUT.uv.z = OUT.positionCS.w;
                
                OUT.uv_persp = uv;
                
                OUT.worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float depth = max(0.001, abs(IN.uv.z));
                
                float2 affineUV = IN.uv.xy / depth;
                float2 perspUV = IN.uv_persp;
                
                float warpFactor = 1.0 - smoothstep(_DistanciaTeste * 0.1, _DistanciaTeste, depth);
                float2 finalUV = lerp(perspUV, affineUV, warpFactor);
                
                half4 albedoSample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, finalUV);
                half4 emissionSample = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, finalUV);
                half3 emission = emissionSample.rgb * _EmissionColor.rgb * _EmissionColor.a * _EmissionPower;

                float3 normalWS = normalize(IN.normalWS);
                float3 viewDirWS = SafeNormalize(GetCameraPositionWS() - IN.worldPos);

                #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
                    float4 shadowCoord = GetShadowCoord(GetVertexPositionInputs(IN.worldPos));
                #else
                    float4 shadowCoord = TransformWorldToShadowCoord(IN.worldPos);
                #endif

                InputData inputData = (InputData)0;
                inputData.positionWS = IN.worldPos;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = viewDirWS;
                inputData.shadowCoord = shadowCoord;
                inputData.fogCoord = ComputeFogFactor(IN.positionCS.z);
                inputData.vertexLighting = half3(0,0,0);
                inputData.bakedGI = SampleSH(normalWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.positionCS);
                inputData.shadowMask = half4(1,1,1,1);

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = albedoSample.rgb;
                surfaceData.metallic = 0.0;
                surfaceData.specular = half3(0,0,0);
                surfaceData.smoothness = 0.0;
                surfaceData.normalTS = half3(0,0,1);
                surfaceData.emission = emission;
                surfaceData.occlusion = 1.0;
                surfaceData.alpha = albedoSample.a;

                half4 finalColor = UniversalFragmentPBR(inputData, surfaceData);
                return finalColor;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            Cull Off

            HLSLPROGRAM
            #pragma vertex vertShadow
            #pragma fragment fragShadow

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct AttributesShadow
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct VaryingsShadow
            {
                float4 positionCS : SV_POSITION;
            };

            float3 _LightDirection;

            VaryingsShadow vertShadow(AttributesShadow IN)
            {
                VaryingsShadow OUT;
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
                return OUT;
            }

            half4 fragShadow(VaryingsShadow IN) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}