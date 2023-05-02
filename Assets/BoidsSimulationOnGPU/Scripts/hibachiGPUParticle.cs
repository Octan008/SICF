using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using UnityEditor;

namespace BoidsSimulationOnGPU
{
    public class hibachiGPUParticle : MonoBehaviour
    {
        // Boidデータの構造体
        [System.Serializable]
        struct BoidData
        {
            public Vector3 Velocity; // 速度
            public Vector3 Position; // 位置
            public Vector3 targetPosition; // 目標位置
            // public Vector3 textTarget;
            // public Vector2 UV1;
            public Vector2 UV2;
            // public Vector2 UV3;
            public Vector4 Color1;
            public Vector4 Color2;
            public float Size;
            // public int pagingButton;
            public float indivTargetWight;
            public float indivTurblulanceWeight;
            public float indivGravityWight;
            public float indivBoidsPileWight;
            public float lifeTime;
            public int mode;
            public int animFrame;
            public Vector4 extras;
        }
        struct FrameProps{
            public Vector3 PositionOffset;
            public Vector3 RotationOffset;
            public Vector3 AnkerOffset;
            public float ScaleOffset;
        }
        // スレッドグループのスレッドのサイズ
        const int SIMULATION_BLOCK_SIZE = 256;

        public hibachiOperationBase operationBase;
        public animationServer[] Scenes;
        public int currentSceneId = 0;
        int SceneCount = 4;
        public bool continuing =true;


        public hibachiGPUTrails trails;
        public hibachiGPUTrailParticles trailParticles;
        public SekisouManager SekisouManager;
        // public hibachiGPUTrailsRenderer trailsRender;

        #region Boids Parameters
        // 最大オブジェクト数
        [Range(10000, 200000)]
        public int MaxObjectNum = 16384;
        public bool emitParticles = true;
        public float respawnLifeTime = 30.0f;
        public bool _renderTrails = true;
        public Vector3 position_offset, anker_offset, rotation_offset;

        public ZigSimOsc oscmanager;

        // 結合を適用する他の個体との半径
        public float CohesionNeighborhoodRadius = 2.0f;
        // 整列を適用する他の個体との半径
        public float AlignmentNeighborhoodRadius = 2.0f;
        // 分離を適用する他の個体との半径
        public float SeparateNeighborhoodRadius = 1.0f;

        public float tmp_ply_num = 1526;

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

        #region force params
        public float _targetWeight = 10.0f;    // 目標地点に向かう強さの重み
        public float _turbulanceWeight = 10.0f;// テクスチャの目標地点に向かう強さの重み
        public float _gravityWeight = 10.0f;// テクスチャの目標地点に向かう強さの重み
        public float _boidPileWeight = 10.0f;// テクスチャの目標地点に向かう強さの重み
        #endregion

        public Transform activator;

        #region Built-in Resources
        // Boidsシミュレーションを行うComputeShaderの参照
        public ComputeShader BoidsCS;
        #endregion
        public ComputeBuffer boidDataBuffer;
        ComputeBuffer _boidForceBuffer;

        public ComputeBuffer[] positionMapArray;
        public ComputeBuffer[] colorMapArray; 
        public Texture2DArray positionMapTexArray;
        public Texture2DArray colorMapTexArray;
        public ComputeBuffer[] propsArrayBuffer; 

        float endingTime = 0.0f;


        #region Private Resources
        // Boidの操舵力（Force）を格納したバッファ
        
        // Boidの基本データ（速度, 位置, Transformなど）を格納したバッファ
        // ComputeBuffer boidDataBuffer;
        #endregion

        public Vector2 compute_centered_uv(int width, float u, float v){
            u = (float)(int)u * width + 0.5f;
            v = (float)(int)v * width+ 0.5f;
            u /= width;
            v /= width;
            return new Vector2(u, v);
        }

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

        void Awake() {
            Application.targetFrameRate = 60; //60FPSに設定
        }

        #region MonoBehaviour Functions
        void Start()
        {
            // バッファを初期化
            tmp_ply_num = operationBase.texA.width;
            SceneCount = Scenes.Length;
            Debug.Log(tmp_ply_num);
            for(int i=0; i<SceneCount; i++){
                Scenes[i].InitProps();
            }
            InitBuffer();
            InitSceneBuffer();
            time_last = Time.time;
        }
        public float endingbuffer = 2;
        public int botMode = 0;

        void Update()
        {
            sceneControl();
            // シミュレーション
            if(continuing){
                Simulation();
                if(_renderTrails) trails.LateUpdate_Trails();
            }


            
            // trailParticles.Update_TrailParticles();
        }

        void OnDestroy()
        {
            // バッファを破棄
            ReleaseBuffer();
        }
        public bool forceLoop = false;

        void OnDrawGizmos()
        {
            // デバッグとしてシミュレーション領域をワイヤーフレームで描画
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(WallCenter, WallSize);
        }
        #endregion
        public Color[,] ReadPixels(Texture2D texture)
        {
            var _cacheColors = new Color[texture.width, texture.height];
            var copyTexture = new Texture2D(texture.width, texture.height, texture.format, false);
            // テクスチャのコピー
            Graphics.CopyTexture(texture, copyTexture);
            for (var x = 0; x < texture.width; x++)
            {
                for (var y = 0; y < texture.height; y++)
                {
                    _cacheColors[x, y] = copyTexture.GetPixel(x, y);
                }
            }
            return _cacheColors;
        }
        public static void SetTextureImporterFormat( Texture2D texture, bool isReadable)
        {
            if ( null == texture ) return;

            string assetPath = AssetDatabase.GetAssetPath( texture );
            var tImporter = AssetImporter.GetAtPath( assetPath ) as TextureImporter;
            if ( tImporter != null )
            {
                tImporter.textureType = TextureImporterType.Default;

                tImporter.isReadable = isReadable;

                AssetDatabase.ImportAsset( assetPath );
                AssetDatabase.Refresh();
            }
        }
        public bool Refreshing;
        [ContextMenu("Play")]
        public void PlayScene(){
            
           Refreshing = true;
           time_last = Time.time;
           Ending = false;
           endingSafety = true;
           SekisouManager.resetForScene();
           swingCount = 0;
        }
        public bool Ending;
        [ContextMenu("End")]
        public void EndScene(){
            Ending = true;
            endingTime = Time.time - time_last;
        }
        int endBotModeId = 3;
        bool endingSafety = true;
        

        void sceneControl(){
            //0503
            if(oscmanager.swingQueue == 1 && swingQueue == 0 || oscmanager.swingQueue == 0 && swingQueue == 1){
                Debug.Log("Swing");
                this.startSwing();
            }
            

            if(oscmanager.botMode == 0 && botMode != 0){
                Debug.Log("play");
                this.PlayScene();
            }
            
            //0503どちらか
            if(oscmanager.botMode == endBotModeId && botMode != endBotModeId){
                Debug.Log("End");
                EndScene();
            }
            

            // //0503どちらか
            // if(!Ending &&  (Time.time - time_last)  > Scenes[currentSceneId].sceneTimeLength){
            //     EndScene();
            // }
            


            if(Ending &&  (Time.time - time_last) - endingTime > endingbuffer){
                SekisouManager.PhaseUp();
                if(endingSafety) currentSceneId = (currentSceneId + 1)%SceneCount;
                endingSafety = false;
                if(forceLoop) PlayScene();
            }
            botMode = oscmanager.botMode;
            swingQueue = oscmanager.swingQueue;

        }

        public void SuperReset(){

        }
        void LateUpdate(){
            Refreshing = false;
            
            // Ending = false;
        }
        void InitSceneBuffer(){
            positionMapArray = new ComputeBuffer[SceneCount];
            colorMapArray = new ComputeBuffer[SceneCount];
            propsArrayBuffer = new ComputeBuffer[SceneCount];
            for(int i = 0; i < SceneCount; i++){
                // positionMapArray[i] = new ComputeBuffer(Scenes[i].numFrames, Marshal.SizeOf(typeof(Texture2D)));
                // colorMapArray[i] = new ComputeBuffer(Scenes[i].numFrames, Marshal.SizeOf(typeof(Texture2D)));
                propsArrayBuffer[i] = new ComputeBuffer(Scenes[i].numFrames, Marshal.SizeOf(typeof(FrameProps)));
                var positionMapArr = new Texture2D[Scenes[i].numFrames];
                var colorMapArr = new Texture2D[Scenes[i].numFrames];
                var propsArr = new FrameProps[Scenes[i].numFrames];
                for (int j = 0; j < Scenes[i].numFrames; j++){
                    // positionMapArr[j] = Scenes[i].positionMap(j);
                    // colorMapArr[j] = Scenes[i].colorMap(j);
                    propsArr[j].PositionOffset  = Scenes[i].list_PositionOffsets[j];
                    propsArr[j].RotationOffset     = Scenes[i].list_RotationOffsets[j];
                    propsArr[j].ScaleOffset        = Scenes[i].list_Scales[j];
                    propsArr[j].AnkerOffset        = Scenes[i].list_AnkerOffsets[j];
                }
                // positionMapArray[i].SetData(positionMapArr);
                // colorMapArray[i].SetData(colorMapArr);
                propsArrayBuffer[i].SetData(propsArr);
                // positionMapArr = null;
                // colorMapArr = null;
                propsArr = null;
            }
        }

        #region Private Functions
        // バッファを初期化
        void InitBuffer()
        {
            // バッファを初期化
            boidDataBuffer = new ComputeBuffer(MaxObjectNum,
                Marshal.SizeOf(typeof(BoidData)));
            _boidForceBuffer = new ComputeBuffer(MaxObjectNum,
                Marshal.SizeOf(typeof(Vector3)));

            // Boidデータ, Forceバッファを初期化
            var forceArr = new Vector3[MaxObjectNum];
            var boidDataArr = new BoidData[MaxObjectNum];
            // Vector3 tmp_pos_scl = new Vector3(1.77f*DisplayScale, DisplayScale, 1.0f);
            Vector3 tmp_pos_scl = new Vector3(DisplayScale, DisplayScale, DisplayScale);
            Color[,] texA = ReadPixels(operationBase.texA);
            Color[,] texB = ReadPixels(operationBase.texB);
            float height = 0.15f;
            for (var i = 0; i < MaxObjectNum; i++)
            {
                forceArr[i] = Vector3.zero;
                // boidDataArr[i].Position = Random.insideUnitSphere * 1.0f;
                // boidDataArr[i].UV1 = new Vector2(Random.value, Random.value);
                // boidDataArr[i].UV1 = compute_centered_uv((int)(tmp_ply_num-1)+1, Random.value, Random.value);
                // boidDataArr[i].Color1 = operationBase.texA.GetPixel(Mathf.CeilToInt(422*Random.value), Mathf.CeilToInt(422*Random.value));
                // int u = i/422;
                int u = Mathf.CeilToInt((tmp_ply_num-1)*Random.value);
                int v = Mathf.CeilToInt((tmp_ply_num-1)*Random.value);
                // int v = i%422;
                boidDataArr[i].Color1 = texA[u,v];
                boidDataArr[i].UV2 = new Vector2(Random.value, Random.value);
                // boidDataArr[i].Position = Vector3.Scale(new Vector3(boidDataArr[i].UV1.x-0.5f, boidDataArr[i].UV1.y-0.5f, 0) , tmp_pos_scl);
                Vector4 a = (Vector4)texB[u,v];
                // boidDataArr[i].Position = Vector3.Scale(new Vector3(a.x-0.5f, a.y-0.5f, a.z-0.5f) , tmp_pos_scl);
                Vector2 tmp =Random.insideUnitCircle*0.1f;
                boidDataArr[i].Position = new Vector3(tmp.x, height, tmp.y) ;
                //boidDataArr[i].lifeTime = -Random.value*5;
                boidDataArr[i].lifeTime = -Random.value * (respawnLifeTime + 0.0f) - 1.0f;
                boidDataArr[i].targetPosition = Vector3.Scale(new Vector3(a.x-0.5f, a.y-0.5f, a.z-0.5f) , tmp_pos_scl);
                // boidDataArr[i].Velocity = Random.insideUnitSphere * 0.1f;
                // // boidDataArr[i].Velocity = Random.insideUnitSphere * 0.1f;
                boidDataArr[i].Velocity = Vector3.zero;
                boidDataArr[i].Size = 1;
                
                boidDataArr[i].indivTargetWight = 0.0f;
                boidDataArr[i].indivTurblulanceWeight = 0.3f;
                boidDataArr[i].indivGravityWight = 1.0f;
                boidDataArr[i].indivBoidsPileWight = 0.0f;
                
            }
            _boidForceBuffer.SetData(forceArr);
            boidDataBuffer.SetData(boidDataArr);
            forceArr = null;
            boidDataArr = null;
        }
        float time_last = 0.0f;
        float time_swing_last;
        public int swingCount = 0;
        int swingQueue = 0;
        void startSwing(){
            Debug.Log("test");
            time_swing_last = Time.time;
            swingCount += 1;
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
            id = cs.FindKernel("ForceCS"); // カーネルIDを取得
            
            cs.SetInt("_MaxBoidObjectNum", MaxObjectNum);
            cs.SetFloat("_CohesionNeighborhoodRadius", CohesionNeighborhoodRadius);
            cs.SetFloat("_AlignmentNeighborhoodRadius", AlignmentNeighborhoodRadius);
            cs.SetFloat("_SeparateNeighborhoodRadius", SeparateNeighborhoodRadius);
            cs.SetFloat("_MaxSpeed", MaxSpeed);
            cs.SetFloat("_MaxSteerForce", MaxSteerForce);
            cs.SetFloat("_SeparateWeight", SeparateWeight);
            cs.SetFloat("_CohesionWeight", CohesionWeight);
            cs.SetFloat("_AlignmentWeight", AlignmentWeight);
            cs.SetVector("_WallCenter", WallCenter);
            cs.SetVector("_WallSize", WallSize);
            cs.SetVector("_scaleOffset1", new Vector4(1.77f*DisplayScale, DisplayScale,0,0));
            cs.SetVector("_scaleOffset2", new Vector4(2*7.5f, 2,0,0));
            cs.SetVector("_scaleOffset3", new Vector4(1.77f*DisplayScale, DisplayScale,0,0));
            cs.SetFloat("_AvoidWallWeight", AvoidWallWeight);
            cs.SetFloat("_CurlNoiseWeight", 2);
            cs.SetBuffer(id, "_BoidDataBufferRead", boidDataBuffer);
            cs.SetBuffer(id, "_BoidForceBufferWrite", _boidForceBuffer);
            cs.Dispatch(id, threadGroupSize, 1, 1); // ComputeShaderを実行

            // 操舵力から、速度と位置を計算
            id = cs.FindKernel("IntegrateCS"); // カーネルIDを取得
            cs.SetFloat("_DeltaTime", Time.deltaTime);
            cs.SetFloat("_Time", Time.time - time_last);
            // cs.SetFloat("_Circle", operationBase._CirclePhase);
            cs.SetFloat("_targetFade", operationBase._targetFade);
            cs.SetFloat("_turbulanceFade", operationBase._turbulanceFade);
            cs.SetFloat("_gravityFade", operationBase._gravityFade);
            cs.SetFloat("_boidPileFade", operationBase._boidPileFade);
            cs.SetInt("_emitParticles", emitParticles ? 1 : 0);
            cs.SetInt("_startingScene", Refreshing ? 1 : 0);
            cs.SetInt("_endingScene", Ending ? 1 : 0);
            cs.SetFloat("_endingTime", endingTime);
            cs.SetFloat("_particleLifeTime", respawnLifeTime);
            cs.SetVector("_activator", activator.position);
            cs.SetVector("_position_offset", position_offset);
            cs.SetVector("_anker_offset", anker_offset);
            cs.SetVector("_rotation_offset", rotation_offset);//+ new Vector3(0,Time.time*2,0));
            
            cs.SetFloat("_targetWeight", _targetWeight);
            cs.SetFloat("_turbulanceWeight", _turbulanceWeight);
            cs.SetFloat("_gravityWeight", _gravityWeight);
            cs.SetFloat("_boidPileWeight", _boidPileWeight);

            cs.SetInt("_currentSceneId", currentSceneId);
            cs.SetInt("_curretSceneFrameCount", Scenes[currentSceneId].numFrames);
            cs.SetFloat("_handParticleRate", Scenes[currentSceneId].handParticleRate);
            cs.SetFloat("_staticParticleRate", Scenes[currentSceneId].staticParticleRate);
            cs.SetBuffer(id, "_BoidForceBufferRead", _boidForceBuffer);
            cs.SetBuffer(id, "_BoidDataBufferWrite", boidDataBuffer);
            cs.SetTexture(id, "_tex1", operationBase.texA);
             if(Scenes[currentSceneId].haveStaticObj){
                cs.SetVector("_posOffset_static",  Scenes[currentSceneId].staticOffsets[0]);
                cs.SetVector("_rotOffset_static",  Scenes[currentSceneId].staticOffsets[1]);
                cs.SetFloat("_scaleOffset_static",  Scenes[currentSceneId].staticScale);
                cs.SetTexture(id, "_posTex_static", Scenes[currentSceneId].staticPositionMap());
                cs.SetTexture(id, "_colTex_static", Scenes[currentSceneId].staticColorMap());
            }
            else{
                cs.SetVector("_posOffset_static", new Vector3(0,0,0));
                cs.SetVector("_rotOffset_static",  new Vector3(0,0,0));
                cs.SetFloat("_scaleOffset_static",  1.0f);
                cs.SetTexture(id, "_posTex_static", Scenes[currentSceneId].positionMap(0));
                cs.SetTexture(id, "_colTex_static", Scenes[currentSceneId].positionMap(0));
            }
            for(int i=0; i<6; i++){ 
                if(i < Scenes[currentSceneId].numFrames){
                    cs.SetTexture(id, "_posTex"+(i.ToString()), Scenes[currentSceneId].positionMap(i));
                    cs.SetTexture(id, "_colTex"+(i.ToString()), Scenes[currentSceneId].colorMap(i));
                }
                else{
                    cs.SetTexture(id, "_posTex"+(i.ToString()), Scenes[currentSceneId].positionMap(0));
                    cs.SetTexture(id, "_colTex"+(i.ToString()), Scenes[currentSceneId].colorMap(0));
                }
            }
            cs.SetBuffer(id, "_framePropsBufferRead", propsArrayBuffer[currentSceneId]);
            cs.SetFloat("_swingTime", Time.time -  time_swing_last);
            cs.SetInt("_swingCount", swingCount);
            cs.SetVector("_sekisouColor", Scenes[currentSceneId].sekisouColor);
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

            if (boidDataBuffer != null)
            {
                boidDataBuffer.Release();
                boidDataBuffer = null;
            }
        }
        #endregion
    } // class
    [CustomEditor(typeof(hibachiGPUParticle))]//拡張するクラスを指定
    public class hibachiGPUParticleEditor : Editor {

    /// <summary>
    /// InspectorのGUIを更新
    /// </summary>
    public override void OnInspectorGUI(){
        //元のInspector部分を表示
        base.OnInspectorGUI ();
        hibachiGPUParticle basehibachi = target as hibachiGPUParticle;


        //ボタンを表示
        if (GUILayout.Button("Restart")){
            basehibachi.PlayScene();
}          if (GUILayout.Button("end")){
            basehibachi.EndScene();
        }  
    }

    }  
} // namespace