Shader "Custom/PS1Effect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _VertexInaccuracy ("Vertex Inaccuracy", Range(0, 200)) = 50
        // fog controlada via Shader.SetGlobal pelo DarknessManager
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
                float3 worldPos   : TEXCOORD1; // posicao mundo passada ao fragment
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
            //  VARIAVEIS GLOBAIS (DarknessManager)
            // ---------------------------------------------
            float  _GlobalDarkness;
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

                // passar posicao mundo ao fragment para calcular fog por pixel
                OUT.worldPos = TransformObjectToWorld(IN.positionOS.xyz);

                return OUT;
            }

            // ---------------------------------------------
            //  FRAGMENT
            // ---------------------------------------------
            half4 frag(Varyings IN) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                // -- Escuridao base (por pixel) --
                col.rgb *= (1.0 - _GlobalDarkness);

                // -- Height fog (por pixel) --
                // maximo no chao, dissipa para cima
                float heightAbove = IN.worldPos.y - (-0.125);
                float hFog = exp(-max(0.0, heightAbove) * _HeightFogFalloff);
                float heightFogFactor = saturate(hFog * _HeightFogDensity);
                col.rgb = lerp(col.rgb, _HeightFogColor.rgb, heightFogFactor);

                // -- Distance fog (por pixel) --
                // distancia horizontal da camara
                float dist = distance(
                    float2(IN.worldPos.x, IN.worldPos.z),
                    float2(_WorldSpaceCameraPos.x, _WorldSpaceCameraPos.z)
                );
                float normDist = max(dist - _DistFogStart, 0.0) / max(_DistFogEnd - _DistFogStart, 0.001);
                float dFog = 1.0 - exp(-normDist * normDist * 1.0);
                float distFogFactor = dFog * _DistFogDensity;
                col.rgb = lerp(col.rgb, _DistFogColor.rgb, distFogFactor);

                return col;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Unlit"
}
