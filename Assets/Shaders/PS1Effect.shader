Shader "Custom/PS1Effect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _VertexInaccuracy ("Vertex Inaccuracy", Range(0, 200)) = 50
        // escuridao controlada via Shader.SetGlobal pelo DarknessManager e Lighter
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
                float3 worldPos   : TEXCOORD1;
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
            //  VARIAVEIS GLOBAIS
            //  DarknessManager: _DarknessColor, _AmbientLight
            //  Lighter:         _DarknessRadius, _DarknessSoftness
            // ---------------------------------------------

            // cor da escuridao (preto por defeito)
            float4 _DarknessColor;

            // luz ambiente minima mesmo no escuro - evita preto absoluto
            // 0 = preto total, 0.05 = ligeiramente visivel
            float  _AmbientLight;

            // raio de visibilidade em metros a volta da camara
            // Lighter controla este valor: pequeno sem isqueiro, maior com isqueiro
            float  _DarknessRadius;

            // quao suave e o fade no limite do raio
            // valores maiores = fade mais gradual
            float  _DarknessSoftness;

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

                OUT.uv      = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.worldPos = TransformObjectToWorld(IN.positionOS.xyz);

                return OUT;
            }

            // ---------------------------------------------
            //  FRAGMENT
            // ---------------------------------------------
            half4 frag(Varyings IN) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                // distancia 3D deste pixel a camara
                float dist = distance(IN.worldPos, _WorldSpaceCameraPos);

                // visibilidade: 1.0 dentro do raio, 0.0 fora
                // smoothstep faz o fade suave automaticamente - sem circulo abrupto
                float visibility = 1.0 - smoothstep(
                    _DarknessRadius - _DarknessSoftness,
                    _DarknessRadius,
                    dist
                );

                // garantir luz ambiente minima mesmo fora do raio
                visibility = max(visibility, _AmbientLight);

                // aplicar escuridao - pixels fora do raio ficam com a cor de escuridao
                col.rgb = lerp(_DarknessColor.rgb, col.rgb, visibility);

                return col;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Unlit"
}
