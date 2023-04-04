Shader "Hidden/BoidsSimulationOnGPU/BoidsRender"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader
	{
		// Tags {  "Queue" = "Transparent" "RenderType"="Transparent" }
        Tags { "RenderType"="Opaque" }
		LOD 200
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ColorMask RGB
		
		CGPROGRAM
        #pragma surface surf Standard vertex:vert addshadow  keepalpha
        #pragma instancing_options procedural:setup
        
        #pragma target 4.0

        sampler2D _MainTex;
		
		struct Input
		{
			float2 uv_MainTex;
            float3 vertex;
            float3 mPos;
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
            float indivTargetWight;
            float indivTurblulanceWight;
            float indivGravityWight;
            float indivBoidsPileWight;
        };

		#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
		// Boidデータの構造体バッファ
		StructuredBuffer<BoidData> _BoidDataBuffer;
		#endif

		// sampler2D _MainTex; // テクスチャ

		half   _Glossiness; // 光沢
		half   _Metallic;   // 金属特性
		fixed4 _Color;      // カラー

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

		// 頂点シェーダ
		void vert(inout appdata_full v, out Input o)
		{
            UNITY_INITIALIZE_OUTPUT(Input, o);
			#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            

			// インスタンスIDからBoidのデータを取得
            BoidData boidData = _BoidDataBuffer[unity_InstanceID]; 
            o.mPos = v.vertex;
            float3 pos = boidData.position.xyz; // Boidの位置を取得
            float3 scl = _ObjectScale * (boidData.size + 0.05*length(boidData.velocity));          // Boidのスケールを取得

            // オブジェクト座標からワールド座標に変換する行列を定義
            float3 camDir = -normalize(_WorldSpaceCameraPos - pos);
            float4x4 object2world = (float4x4)0; 
            // スケール値を代入
            object2world._11_22_33_44 = float4(scl.xyz, 1.0);
            // 速度からY軸についての回転を算出
            // float rotY = 
            //     atan2(boidData.velocity.x, boidData.velocity.z);
            // // 速度からX軸についての回転を算出
            // float rotX = 
            //     -asin(boidData.velocity.y / (length(boidData.velocity.xyz) + 1e-8));
            float rotY = 
                atan2(camDir.x, camDir.z);
            // 速度からX軸についての回転を算出
            float rotX = 
                -asin(camDir.y / (length(camDir) + 1e-8));
            // オイラー角（ラジアン）から回転行列を求める
            float4x4 rotMatrix = eulerAnglesToRotationMatrix(float3(rotX, rotY, 0));
            // 行列に回転を適用
            object2world = mul(rotMatrix, object2world);
            // 行列に位置（平行移動）を適用
            object2world._14_24_34 += pos.xyz;

            // 頂点を座標変換
            v.vertex = mul(object2world, v.vertex);
            // 法線を座標変換
            v.normal = normalize(mul(object2world, v.normal));
			
            o.vertex = v.vertex;
            #endif
		}
		
		void setup()
		{
		}

        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)
		// サーフェスシェーダ
		void surf (Input IN, inout SurfaceOutputStandard o)
		{
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			// fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;

            // sample the texture
            fixed4 col =  tex2D (_MainTex, IN.uv_MainTex) * _Color;
            col = float4(1,1,1,col.w);
            float phase = length(IN.mPos);
            
            BoidData boidData = _BoidDataBuffer[unity_InstanceID]; 
            
            col.xyz *= boidData.color1.xyz * 1.2;
            col.xyz = lerp(col.xyz, float3(1,0.2,0.2), 0.05*length(boidData.velocity));
            col.a = max(0.0, col.a - 0.85) * 2;
            col.a = max(0.0, 1 - phase*20);
            
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
            o.Albedo = col.rgb;
            // o.Alpha = col.a;
            
            o.Alpha = 1.0;
            #endif

            // col.a = 1.0;
            // apply fog
            // UNITY_APPLY_FOG(i.fogCoord, col);

            // return col;
		}
		ENDCG
	}
	FallBack "Diffuse"
}