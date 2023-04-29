Shader "Unlit/Sekisou"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Phase("Phase", Float) = 0.0
        _Frequency("Frequency", Float) = 0.1
        _Color1("Color1", Color) = (1.0,1.0,1.0,1.0)
        _Color2("Color2", Color) = (1.0,1.0,1.0,1.0)
        _Color3("Color3", Color) = (1.0,1.0,1.0,1.0)
        _Color4("Color4", Color) = (1.0,1.0,1.0,1.0)
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

            #include "UnityCG.cginc"
            #include "SimplexNoise.cginc"
            #include "NoiseMath.cginc"
            #include "Curlnoise.cginc"


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
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Phase;
            float _Frequency;
            float4 _Color1, _Color2, _Color3, _Color4;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                float ex_uvy = pow(i.uv.y, 0.5);
                fixed4 col_noise =  tex2D(_MainTex, i.uv*4);
                float noise_pow = 3;
                float noise = length(col_noise) * pow(1-ex_uvy, noise_pow);
                int num_col = 4;
                
               ex_uvy -= +noise*3*noise_pow;
                bool black =  _Frequency > ex_uvy * _Frequency+_Phase;
                if(black){
                    col = float4(0,0,0,1);
                    return col;
                }
                 

                
                float pp = ex_uvy * _Frequency+_Phase;

                float pp_f = frac(pp);
                pp_f = pow(pp_f, 1);
                int pp_i = floor(pp)%num_col;
                float4 col1 = float4(1,1,1,1);
                float4 col2 = float4(1,1,1,1);
                if(pp_i == 0){
                    col1 = _Color1;
                    col2 = _Color2;
                }
                if(pp_i == 1){
                    col1 = _Color2;
                    col2 = _Color3;
                }
                if(pp_i == 2){
                    col1 = _Color3;
                    col2 = _Color4;
                }
                if(pp_i == 3){
                    col1 = _Color4;
                    col2 = _Color1;
                }
                float saturation = min(1,i.uv.y*0.8);
                
                col *= i.uv.y;
                col = col1 * (1-pp_f) + col2 * pp_f;
                float gamma = (col.x + col.y + col.z) / 3;
                float3 gray = float3(1,1,1) * gamma;
                gray = float3(0.3,0.3,0.3);
                col.xyz = saturation * col + (1-saturation) * gray;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
