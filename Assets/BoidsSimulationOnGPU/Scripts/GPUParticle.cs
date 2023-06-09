﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;


namespace BoidsSimulationOnGPU
{
    public class GPUParticle : MonoBehaviour
    {
        // Boidデータの構造体
        [System.Serializable]
        struct BoidData
        {
            public Vector3 Velocity; // 速度
            public Vector3 Position; // 位置
            public Vector3 targetPosition; // 目標位置
            public Vector3 textTarget;
            public Vector2 UV1;
            public Vector2 UV2;
            public Vector2 UV3;
            public Vector4 Color1;
            public Vector4 Color2;
            public float Size;
            public int pagingButton;
        }
        // スレッドグループのスレッドのサイズ
        const int SIMULATION_BLOCK_SIZE = 256;

        public OperationBase operationBase;

        #region Boids Parameters
        // 最大オブジェクト数
        [Range(256, 100000)]
        public int MaxObjectNum = 16384;

        // 結合を適用する他の個体との半径
        public float CohesionNeighborhoodRadius = 2.0f;
        // 整列を適用する他の個体との半径
        public float AlignmentNeighborhoodRadius = 2.0f;
        // 分離を適用する他の個体との半径
        public float SeparateNeighborhoodRadius = 1.0f;

        // 速度の最大値
        public float MaxSpeed = 5.0f;
        // 操舵力の最大値
        public float MaxSteerForce = 0.5f;

        // 結合する力の重み
        public float CohesionWeight = 1.0f;
        // 整列する力の重み
        public float AlignmentWeight = 1.0f;
        // 分離する力の重み
        public float SeparateWeight = 3.0f;

        // 壁を避ける力の重み
        public float AvoidWallWeight = 10.0f;
        
        public float DisplayScale = 10.0f;

        // 壁の中心座標   
        public Vector3 WallCenter = Vector3.zero;
        // 壁のサイズ
        public Vector3 WallSize = new Vector3(32.0f, 32.0f, 32.0f);
        #endregion

        #region Built-in Resources
        // Boidsシミュレーションを行うComputeShaderの参照
        public ComputeShader BoidsCS;
        
        #endregion

        public ComputeBuffer boidDataBuffer;

        // #region Private Resources
        // Boidの操舵力（Force）を格納したバッファ
        // ComputeBuffer _boidForceBuffer;
        // Boidの基本データ（速度, 位置, Transformなど）を格納したバッファ
        // public ComputeBuffer boidDataBuffer;
        // #endregion

        #region Accessors
        // Boidの基本データを格納したバッファを取得
        public ComputeBuffer GetBoidDataBuffer()
        {
            return this.boidDataBuffer != null ? this.boidDataBuffer : null;
        }

        // オブジェクト数を取得
        public int GetMaxObjectNum()
        {
            return this.MaxObjectNum;
        }

        // シミュレーション領域の中心座標を返す
        public Vector3 GetSimulationAreaCenter()
        {
            return this.WallCenter;
        }

        // シミュレーション領域のボックスのサイズを返す
        public Vector3 GetSimulationAreaSize()
        {
            return this.WallSize;
        }
        #endregion

        #region MonoBehaviour Functions
        void Start()
        {
            // バッファを初期化
            InitBuffer();
        }

        void Update()
        {
            // シミュレーション
            Simulation();
        }

        void OnDestroy()
        {
            // バッファを破棄
            ReleaseBuffer();
        }

        void OnDrawGizmos()
        {
            // デバッグとしてシミュレーション領域をワイヤーフレームで描画
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(WallCenter, WallSize);
        }
        #endregion

        #region Private Functions
        // バッファを初期化
        void InitBuffer()
        {
            // バッファを初期化
            boidDataBuffer = new ComputeBuffer(MaxObjectNum,
                Marshal.SizeOf(typeof(BoidData)));
            // _boidForceBuffer = new ComputeBuffer(MaxObjectNum,
            //     Marshal.SizeOf(typeof(Vector3)));

            // Boidデータ, Forceバッファを初期化
            // var forceArr = new Vector3[MaxObjectNum];
            var boidDataArr = new BoidData[MaxObjectNum];
            Vector3 tmp_pos_scl = new Vector3(1.77f*DisplayScale, DisplayScale, 1.0f);
            for (var i = 0; i < MaxObjectNum; i++)
            {
                // forceArr[i] = Vector3.zero;
                // boidDataArr[i].Position = Random.insideUnitSphere * 1.0f;
                boidDataArr[i].UV1 = new Vector2(Random.value, Random.value);
                boidDataArr[i].UV2 = new Vector2(Random.value, Random.value);
                boidDataArr[i].Position = Vector3.Scale(new Vector3(boidDataArr[i].UV1.x-0.5f, boidDataArr[i].UV1.y-0.5f, 0) , tmp_pos_scl);
                // boidDataArr[i].targetPosition = Vector3.Scale(new Vector3(Random.value-0.5f, Random.value-0.5f, 0) , tmp_pos_scl);
                boidDataArr[i].targetPosition = Vector3.Scale(new Vector3(boidDataArr[i].UV1.x-0.5f, boidDataArr[i].UV1.y-0.5f, 0) , tmp_pos_scl);
                // boidDataArr[i].Velocity = Random.insideUnitSphere * 0.1f;
                boidDataArr[i].Velocity = Vector3.zero;
                boidDataArr[i].Size = 1;
                
            }
            // _boidForceBuffer.SetData(forceArr);
            boidDataBuffer.SetData(boidDataArr);
            // forceArr = null;
            boidDataArr = null;
        }

        // シミュレーション
        void Simulation()
        {
            ComputeShader cs = BoidsCS;
            int id = -1;

            // スレッドグループの数を求める
            // int threadGroupSize = Mathf.CeilToInt(MaxObjectNum / SIMULATION_BLOCK_SIZE);
            int threadGroupSize = Mathf.CeilToInt((float)MaxObjectNum / (float)SIMULATION_BLOCK_SIZE);
            // // 操舵力を計算
            // id = cs.FindKernel("ForceCS"); // カーネルIDを取得
            id = cs.FindKernel("IntegrateCS"); // カーネルIDを取得
            cs.SetInt("_MaxBoidObjectNum", MaxObjectNum);
            // cs.SetFloat("_CohesionNeighborhoodRadius", CohesionNeighborhoodRadius);
            // cs.SetFloat("_AlignmentNeighborhoodRadius", AlignmentNeighborhoodRadius);
            // cs.SetFloat("_SeparateNeighborhoodRadius", SeparateNeighborhoodRadius);
            cs.SetFloat("_MaxSpeed", MaxSpeed);
            cs.SetFloat("_MaxSteerForce", MaxSteerForce);
            // cs.SetFloat("_SeparateWeight", SeparateWeight);
            // cs.SetFloat("_CohesionWeight", CohesionWeight);
            // cs.SetFloat("_AlignmentWeight", AlignmentWeight);
            cs.SetVector("_WallCenter", WallCenter);
            cs.SetVector("_WallSize", WallSize);
            cs.SetVector("_scaleOffset1", new Vector4(1.77f*DisplayScale, DisplayScale,0,0));
            cs.SetVector("_scaleOffset2", new Vector4(2*7.5f, 2,0,0));
            cs.SetVector("_scaleOffset3", new Vector4(1.77f*DisplayScale, DisplayScale,0,0));
            cs.SetFloat("_AvoidWallWeight", AvoidWallWeight);
            cs.SetFloat("_CurlNoiseWeight", 2);
            cs.SetBuffer(id, "boidDataBufferRead", boidDataBuffer);
            // cs.SetBuffer(id, "_BoidForceBufferWrite", _boidForceBuffer);
            // cs.Dispatch(id, threadGroupSize, 1, 1); // ComputeShaderを実行

            // 操舵力から、速度と位置を計算
            // id = cs.FindKernel("IntegrateCS"); // カーネルIDを取得
            cs.SetFloat("_DeltaTime", Time.deltaTime);
            cs.SetFloat("_Time", Time.time);
            cs.SetFloat("_Circle", operationBase._CirclePhase);
            cs.SetFloat("_textTargetWeight", operationBase._textTargetPhase);
            // cs.SetBuffer(id, "_BoidForceBufferRead", _boidForceBuffer);
            cs.SetBuffer(id, "boidDataBufferWrite", boidDataBuffer);
            cs.SetTexture(id, "_tex0", operationBase.texC);
            cs.SetTexture(id, "_tex2", operationBase.texB);
            cs.SetTexture(id, "_tex1", operationBase.texA);
            cs.Dispatch(id, threadGroupSize, 1, 1); // ComputeShaderを実行
        }

        // バッファを解放
        void ReleaseBuffer()
        {
            if (boidDataBuffer != null)
            {
                boidDataBuffer.Release();
                boidDataBuffer = null;
            }

            // if (_boidForceBuffer != null)
            // {
            //     _boidForceBuffer.Release();
            //     _boidForceBuffer = null;
            // }
        }
        #endregion
    } // class
} // namespace