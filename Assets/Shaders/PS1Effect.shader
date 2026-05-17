Shader "Custom/PS1Effect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _VertexInaccuracy ("Vertex Inaccuracy", Range(0, 200)) = 50
        _EmissionMap ("Emission Map", 2D) = "black" {}
        _EmissionColor ("Emission Color", Color) = (0, 0, 0, 1)
        _EmissionPower ("Emission Power", Range(0, 10)) = 1
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
            #pragma target 2.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 worldPos   : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_EmissionMap);
            SAMPLER(sampler_EmissionMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _EmissionMap_ST;
                float  _VertexInaccuracy;
                float4 _EmissionColor;
                float  _EmissionPower;
            CBUFFER_END

            float4 _DarknessColor;
            float  _AmbientLight;
            float  _DarknessRadius;
            float  _DarknessSoftness;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);

                if (_VertexInaccuracy > 0)
                {
                    float gridSize = _VertexInaccuracy;
                    OUT.positionCS.xy = floor(OUT.positionCS.xy * gridSize) / gridSize;
                }

                OUT.uv       = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.worldPos = TransformObjectToWorld(IN.positionOS.xyz);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                
                // Amostra o mapa de emissão
                half4 emissionSample = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, IN.uv);
                
                // Calcula emissão pura (não é afetada pelo darkness)
                half3 emission = emissionSample.rgb * _EmissionColor.rgb * _EmissionColor.a * _EmissionPower;

                float dist = distance(IN.worldPos, _WorldSpaceCameraPos);

                float visibility = 1.0 - smoothstep(
                    _DarknessRadius - _DarknessSoftness,
                    _DarknessRadius,
                    dist
                );

                visibility = max(visibility, _AmbientLight);
                col.rgb = lerp(_DarknessColor.rgb, col.rgb, visibility);
                
                // Adiciona a emissão (passa através de escuridão/paredes)
                // Usa max para que emissão nunca fica escura
                col.rgb = max(col.rgb, emission);

                return col;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Unlit"
}