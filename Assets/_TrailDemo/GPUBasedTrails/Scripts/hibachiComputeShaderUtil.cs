using UnityEngine;

namespace BoidsSimulationOnGPU
{
    public class hibachiComputeShaderUtil
    {
        public static void Dispatch(ComputeShader cs, int kernel, Vector3 threadNum)
        {
            uint x, y, z;
            cs.GetKernelThreadGroupSizes(kernel, out x, out y, out z);
            cs.Dispatch(kernel, Mathf.CeilToInt(threadNum.x / x), Mathf.CeilToInt(threadNum.y / y), Mathf.CeilToInt(threadNum.z / z));
        }
    }
}
