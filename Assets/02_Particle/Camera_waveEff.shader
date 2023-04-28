Shader "Unlit/Camera"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _PerlinNoiseTex ("Perlin Noise Texture", 2D) = "white" {}
        _Circle ("Circle", Float) = 0
        _Circle_scale ("Circle_scale", Float) = 0
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off Cull Off ZTest LEqual Fog { Mode Off }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;
            sampler2D _PerlinNoiseTex;
            float4 _PerlinNoiseTex_ST;
            float _Circle;
            float _Circle_scale;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 perlinnoise = tex2D(_PerlinNoiseTex, i.uv*1.5);
                fixed4 noise = tex2D(_NoiseTex, i.uv);
                float shift = perlinnoise.r*2 + noise.r;
                float dist = length(i.worldPos);
                dist += shift;
                if (dist > _Circle*_Circle_scale)
                {
                    col.a = 1;
                    float lit = max(1.0 - (dist - _Circle*_Circle_scale), 0.0);
                    col.rgb += lit*noise.rgb;
                }
                else{
                    col.a = 0;
                }
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
