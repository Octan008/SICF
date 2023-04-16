using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

public class hibachiOperationBase : MonoBehaviour
{
   public VisualEffect vfx_title;
   public Material camMat;
   public BoidsSimulationOnGPU.GPUParticle GPUParticleScript;
   public float _CirclePhase = 1.0f;

   public float _targetFade = 1.0f;
   public float _turbulanceFade = 1.0f;
   public float _gravityFade = 1.0f;
   public float _boidPileFade = 1.0f;

   public Texture2D texA, texB;
   // public RenderTexture texC;
   public  Texture2D texC;

   public Slider targetSlider, turbulanceSlider, gravitySlider, pileSlider;


    public void SetTargetFade(float value) {
       Debug.Log("Value: " + value);
       _targetFade = value;
       targetSlider.value = value;
    }
    public void SetTurbulanceFade(float value) {
       Debug.Log("Value: " + value);
       _turbulanceFade = value;
         turbulanceSlider.value = value;
    }
    public void SetGravityFade(float value) {
       Debug.Log("Value: " + value);
       _gravityFade = value;
         gravitySlider.value = value;
    }
    public void SetPileFade(float value) {
       Debug.Log("Value: " + value);
       _boidPileFade = value;
      pileSlider.value = value;
    }
   //  public void SetTitleIntensity(float value) {
   //     Debug.Log("Value: " + value);
   //  }
   //  public void SetCamIntensity(float value) {
   //     Debug.Log("Value: " + value);
   //     vfx_title.SetFloat("camIntensity", value);
   //  }
   // public void SetCirclePhase(float value) {
   //     Debug.Log("Value: " + value);
   //     camMat.SetFloat("_Circle", value);
   //     _CirclePhase = value;
   //  }
}
