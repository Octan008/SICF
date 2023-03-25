using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class OperationBase : MonoBehaviour
{
   public VisualEffect vfx_title;
   public Material camMat;
   public BoidsSimulationOnGPU.GPUParticle GPUParticleScript;
   public float _CirclePhase = 0.0f;
   public float _textTargetPhase = 0.0f;
   public Texture2D texA, texB;
   public RenderTexture texC;

    public void SetTitleIntensity(float value) {
       Debug.Log("Value: " + value);
    }
    public void SetCamIntensity(float value) {
       Debug.Log("Value: " + value);
       vfx_title.SetFloat("camIntensity", value);
    }
    public void SetCirclePhase(float value) {
       Debug.Log("Value: " + value);
       camMat.SetFloat("_Circle", value);
       _CirclePhase = value;
    }
    public void SetTextTargetPhase(float value) {
       Debug.Log("Value: " + value);
       _textTargetPhase = value;
    }

    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
