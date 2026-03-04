Shader "Custom/PS1Effect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _VertexInaccuracy ("Vertex Inaccuracy", Range(0, 200)) = 50
        // os parametros de fog sao controlados pelo DarknessManager via Shader.SetGlobal
        // nao os expor aqui evita que o Inspector os sobrescreva
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

            // ---------------------------------------------
            //  ESTRUTURAS
            // ---------------------------------------------
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float  heightFog  : TEXCOORD1;
                float  distFog    : TEXCOORD2;
            };

            // ---------------------------------------------
            //  VARIAVEIS DO MATERIAL
            // ---------------------------------------------
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float  _VertexInaccuracy;
            CBUFFER_END

            // ---------------------------------------------
            //  VARIAVEIS GLOBAIS (controladas pelo DarknessManager)
            // ---------------------------------------------
            // declaradas fora do CBUFFER para aceitar Shader.SetGlobal
            float  _HeightFogDensity;
            float  _HeightFogFalloff;
            float4 _HeightFogColor;
            float  _DistFogDensity;
            float  _DistFogStart;
            float  _DistFogEnd;
            float4 _DistFogColor;

            // ---------------------------------------------
            //  VERTEX
            // ---------------------------------------------
            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);

                // Vertex snapping (efeito PS1)
                if (_VertexInaccuracy > 0)
                {
                    float gridSize = _VertexInaccuracy;
                    OUT.positionCS.xy = floor(OUT.positionCS.xy * gridSize) / gridSize;
                }

                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);

                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);

                // -- Height fog --
                float heightAbove = worldPos.y - (-0.125);
                float hFog = exp(-max(0.0, heightAbove) * _HeightFogFalloff);
                OUT.heightFog = saturate(hFog * _HeightFogDensity);

                // -- Distance fog --
                float3 camPos = _WorldSpaceCameraPos;
                float dist    = distance(float2(worldPos.x, worldPos.z), float2(camPos.x, camPos.z));
                float dFog    = saturate((dist - _DistFogStart) / max(_DistFogEnd - _DistFogStart, 0.001));
                OUT.distFog   = dFog * _DistFogDensity;

                return OUT;
            }

            // ---------------------------------------------
            //  FRAGMENT
            // ---------------------------------------------
            half4 frag(Varyings IN) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                // Escurecer o chao pelo Y
                col.rgb = lerp(col.rgb, _HeightFogColor.rgb, IN.heightFog);

                // Escurecer ao longe
                col.rgb = lerp(col.rgb, _DistFogColor.rgb, IN.distFog);

                return col;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Unlit"
}
