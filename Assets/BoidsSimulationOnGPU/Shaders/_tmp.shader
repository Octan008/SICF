Shader "InstancingExample"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
             "IgnoreProjector" = "True"
             "PreviewType" = "Plane"
             "PerformanceChecks" = "False"
             "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:ParticleInstancingSetup

            // ParticlesInstancing.cgincのDefaultParticleInstanceDataの代わりにこちらを使う設定
            // ParticlesInstancing.cgincのインクルードよりも先に書くこと
            #define UNITY_PARTICLE_INSTANCE_DATA MyDefaultParticleInstanceData
            struct MyDefaultParticleInstanceData
            {
                float3x4 transform;
                float3 foo;
            };
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            // ParticleのシェーダでGPUインスタンシング対応する場合にはこれをインクルードする
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ParticlesInstancing.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
#ifndef UNITY_PARTICLE_INSTANCING_ENABLED
                float3 foo : TEXCOORD0;
#endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 foo : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                UNITY_SETUP_INSTANCE_ID(input);
                
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);

                #ifdef UNITY_PARTICLE_INSTANCING_ENABLED
                    // インスタンスのデータを取得する
                    UNITY_PARTICLE_INSTANCE_DATA data = unity_ParticleInstanceData[unity_InstanceID];
                    output.foo = data.foo;
                #else
                    output.foo = input.foo;
                #endif
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return float4(abs(input.foo), 1);
            }
            ENDHLSL
        }
    }
}