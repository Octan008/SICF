// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/hibachiNoiseBox"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [Toggle(Horizontal)] _IsHorizontal("Is Horizontal", Float) = 0
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
            // make fog work
            #pragma multi_compile_fog
            #pragma shader_feature _ Horizontal 

            #include "UnityCG.cginc"
            
            #include "./SimplexNoise.cginc"
            #include "./NoiseMath.cginc"
            #include "./Curlnoise.cginc"

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
                float3 wpos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _IsHorizontal;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                float3 worldPos = mul (unity_ObjectToWorld, v.vertex).xyz;
                o.wpos = worldPos;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                if(_IsHorizontal == 1) i.uv = float2(i.uv.y, i.uv.x);
                fixed4 col = tex2D(_MainTex, i.uv);
                col = 1- col;
                col *= float4(1,1,1,1);
                // col = float4(1,1,1,1);
                float nn = CurlNoise(i.wpos*100, _Time.y*10);
                // col.xyz = float3(nn,nn,nn);
                float z = i.wpos.z;
                float n = CurlNoise(float3(i.wpos.xy*200 , 1), _Time.y*10);
                // float n = 0;
                // if(_IsHorizontal) n = CurlNoise(float3(i.wpos.y*100, 1 , 1), _Time.y*10);
                // n = n - (i.wpos.z+0.1)*1;
                n = max(0, min(1, n));

                // col.xyz *= n;
                col.xyz *= min(1, max(0, 1-z)) * 0.55;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
