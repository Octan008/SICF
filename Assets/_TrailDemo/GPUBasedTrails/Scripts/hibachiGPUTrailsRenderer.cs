using UnityEngine;

namespace BoidsSimulationOnGPU
{
    [RequireComponent(typeof(hibachiGPUTrails))]
    public class hibachiGPUTrailsRenderer : MonoBehaviour
    {
        public Material _material;
        hibachiGPUTrails _trails;


        private void Start()
        {
            _trails = GetComponent<hibachiGPUTrails>();
        }

        void OnRenderObject()
        {
            _material.SetInt(hibachiGPUTrails.CSPARAM.NODE_NUM_PER_TRAIL, _trails.nodeNum);
            _material.SetFloat(hibachiGPUTrails.CSPARAM.LIFE, _trails.life);
            _material.SetBuffer(hibachiGPUTrails.CSPARAM.TRAIL_BUFFER, _trails.trailBuffer);
            _material.SetBuffer(hibachiGPUTrails.CSPARAM.NODE_BUFFER, _trails.nodeBuffer);
            _material.SetPass(0);

            var nodeNum = _trails.nodeNum;
            var trailNum = _trails.trailNum;
            Graphics.DrawProceduralNow(MeshTopology.Points, nodeNum, trailNum);
        }
    }
}
