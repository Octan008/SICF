using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SekisouManager : MonoBehaviour
{
    public Material Mat;
    public int phase = 0;
    public float phase_f = 0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    [ContextMenu("PhaseUp")]
    public void PhaseUp()
    {
        phase++;
    }

    // Update is called once per frame
    void Update()
    {
        phase_f = Mathf.Lerp(phase_f, phase, 0.1f);
        Mat.SetFloat("_Phase", phase_f);
    }
}
