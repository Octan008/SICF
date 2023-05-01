using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// namespace Pcx
// {
public class animationServer : MonoBehaviour
{
    public struct PointCloudModel{
        public Pcx.BakedPointCloud pcx;
        public Vector3 PositionOffset;
        public Vector3 RotationOffset;
        public Vector3 AnkerOffset;
    }
    public Pcx.BakedPointCloud[] models;
    public PointCloudModel[] pcModels;
    public GameObject[] refGOs;
    public Vector3[] list_PositionOffsets;
    public Vector3[] list_RotationOffsets;
    public Vector3[] list_AnkerOffsets;
    public Texture2D[] list_PositionMaps;
    public Texture2D[] list_ColorMaps;
    public float[] list_Scales;
    public int numFrames = 4;
    public string name = "noname";
    // Start is called before the first frame update
    public void InitProps(){
        numFrames = models.Length;
        list_PositionMaps = new Texture2D[numFrames];
        list_ColorMaps = new Texture2D[numFrames];
        for(int i=0; i<numFrames; i++){
            list_PositionOffsets[i] = refGOs[i].transform.position;
            list_Scales[i] = refGOs[i].transform.localScale.x;
            list_RotationOffsets[i] = refGOs[i].transform.rotation.eulerAngles;
            list_PositionMaps[i] = models[i].positionMap;
            list_ColorMaps[i] = models[i].colorMap;
            // list_RotationOffsets = refGOs[i].transform.rotation;
        }
    }
    void Start()
    {
        InitProps();
    }
    public Texture2D positionMap(int i){
        return models[i].positionMap;
    }
    public Texture2D colorMap(int i){
        return models[i].colorMap;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
// }
