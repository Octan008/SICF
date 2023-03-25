Shader "Unlit/ParticleRenderUnlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // Tags { "RenderType"="Opaque" }
        Tags {  "Queue" = "Transparent" "RenderType"="TransparentCutout" }
        LOD 100
         ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha 

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #pragma instancing_options procedural:setup

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                uint InstanceId           : SV_InstanceID; 
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            // Boidの構造体
            struct BoidData
            {
                float3 velocity; // 速度
                float3 position; // 位置
                float3 target; // 目標位置
                float3 textTarget;
                float2 uv1;
                float2 uv2;
                float2 uv3;
                float4 color1;
                float4 color2;
                float size;
                int pagingButton;
            };

            // #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            // Boidデータの構造体バッファ
            StructuredBuffer<BoidData> _BoidDataBuffer;
            // #endif

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD1;
                uint InstanceId  : SV_InstanceID; 
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float3 _ObjectScale; // Boidオブジェクトのスケール
            // オイラー角（ラジアン）を回転行列に変換
            float4x4 eulerAnglesToRotationMatrix(float3 angles)
            {
                float ch = cos(angles.y); float sh = sin(angles.y); // heading
                float ca = cos(angles.z); float sa = sin(angles.z); // attitude
                float cb = cos(angles.x); float sb = sin(angles.x); // bank

                // Ry-Rx-Rz (Yaw Pitch Roll)
                return float4x4(
                    ch * ca + sh * sb * sa, -ch * sa + sh * sb * ca, sh * cb, 0,
                    cb * sa, cb * ca, -sb, 0,
                    -sh * ca + ch * sb * sa, sh * sa + ch * sb * ca, ch * cb, 0,
                    0, 0, 0, 1
                );
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                // #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED

                // インスタンスIDからBoidのデータを取得
                BoidData boidData = _BoidDataBuffer[v.InstanceId]; 

                float3 pos = boidData.position.xyz; // Boidの位置を取得
                float3 scl = _ObjectScale * boidData.size;          // Boidのスケールを取得

                // オブジェクト座標からワールド座標に変換する行列を定義
                float4x4 object2world = (float4x4)0; 
                // スケール値を代入
                object2world._11_22_33_44 = float4(scl.xyz, 1.0);
                // 速度からY軸についての回転を算出
                float rotY = 
                    atan2(boidData.velocity.x, boidData.velocity.z);
                // 速度からX軸についての回転を算出
                float rotX = 
                    -asin(boidData.velocity.y / (length(boidData.velocity.xyz) + 1e-8));
                // オイラー角（ラジアン）から回転行列を求める
                float4x4 rotMatrix = eulerAnglesToRotationMatrix(float3(rotX, rotY, 0));
                // 行列に回転を適用
                // object2world = mul(rotMatrix, object2world);
                // 行列に位置（平行移動）を適用
                object2world._14_24_34 += pos.xyz;

                // 頂点を座標変換
                v.vertex = mul(object2world, v.vertex);
                // 法線を座標変換
                v.normal = normalize(mul(object2world, v.normal));
                // #endif
                o.uv = v.uv;    
                o.vertex = v.vertex;
                // o.vertex = UnityObjectToClipPos(v.vertex);
                o.vertex = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_MV, float4(0, 0, 0, 1)) + float4(v.vertex.x, v.vertex.y, 0, 0));
                o.InstanceId = v.InstanceId;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                BoidData boidData = _BoidDataBuffer[i.InstanceId]; 
                col.xyz *= boidData.color1.xyz;
                col.a = max(0.0, col.a - 0.5) * 2;
                // col.a = 1.0;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
