Shader "Custom/PS1Effect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _VertexInaccuracy ("Vertex Inaccuracy", Range(0, 200)) = 50
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

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float  _VertexInaccuracy;
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

                float dist = distance(IN.worldPos, _WorldSpaceCameraPos);

                float visibility = 1.0 - smoothstep(
                    _DarknessRadius - _DarknessSoftness,
                    _DarknessRadius,
                    dist
                );

                visibility = max(visibility, _AmbientLight);
                col.rgb = lerp(_DarknessColor.rgb, col.rgb, visibility);

                return col;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Unlit"
}
