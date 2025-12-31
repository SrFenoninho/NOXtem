Shader "Custom/PS1Effect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _VertexInaccuracy ("Vertex Inaccuracy", Range(0, 200)) = 50
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _VertexInaccuracy;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                // Vertex snapping (PS1 effect)
                if (_VertexInaccuracy > 0)
                {
                    float gridSize = _VertexInaccuracy;
                    o.vertex.xy = floor(o.vertex.xy * gridSize) / gridSize;
                }
                
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}