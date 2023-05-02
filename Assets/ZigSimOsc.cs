using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OscJack;

using OscJack;

public class ZigSimOsc : MonoBehaviour {

    public GameObject go;
    public int port = 10000;
    public float mult = 10.0f;
    public Transform hibachiRoot;
    public Vector3 offset;
    public Vector3 offset_scale;

    private OscServer _server;
    private Vector3 _arpos;
    private Quaternion _rot;
    public Transform _criterion;
    public Vector3 shift;
    private string _dspBusName;
    public bool CamControl = false;
    public bool activatorCrontrol = false;

    public Transform activator;
    public Vector3 targetPosition;
    public bool debug_activator = false;

    public int botMode;   
    public int swingQueue;    
    public int cameraValid = 0;
    public int prev_cameraValid = 1;
    public float camFading = 0;
    bool camFadingnow = false;
    public Material fading;

    // Use this for initialization
    [ContextMenu("Reset")]
    void Reset()
    {
        shift = _criterion.position - _arpos;
        // mult = mult * hibachiRoot.localScale.x;
    }
    void Start () {
        targetPosition = activator.position;
        _server = new OscServer(port); // Port number

        _server.MessageDispatcher.AddCallback(
            "/botMode",
            (string address, OscDataHandle data) =>
            {
               botMode = (int)data.GetElementAsFloat(0);
            }
        );
        // _server.MessageDispatcher.AddCallback(
        //     //0503
        //     "/swingQueue",
        //     (string address, OscDataHandle data) =>
        //     {
        //        swingQueue = (int)data.GetElementAsFloat(0);
        //     }
        // );

        _server.MessageDispatcher.AddCallback(
            "/faceDetect",
            (string address, OscDataHandle data) =>
            {
               cameraValid = (int)data.GetElementAsFloat(0);
            }
        );
        _server.MessageDispatcher.AddCallback(
            "/ZIGSIM/octaniPad/arkitposition1",
            (string address, OscDataHandle data) =>
            {
                _arpos.x = data.GetElementAsFloat(0);
            }
        );
        _server.MessageDispatcher.AddCallback(
            "/ZIGSIM/octaniPad/arkitposition2",
            (string address, OscDataHandle data) =>
            {
                _arpos.y = data.GetElementAsFloat(0);
            }
        );
        _server.MessageDispatcher.AddCallback(
            "/ZIGSIM/octaniPad/arkitposition3",         
            (string address, OscDataHandle data) =>
            {
                _arpos.z = data.GetElementAsFloat(0);
            }
        );
/*        _server.MessageDispatcher.AddCallback(
            "/ZIGSIM/octaniPad/arkitrotation1",
            (string address, OscDataHandle data) =>
            {
                _rot.x = -data.GetElementAsFloat(0);
            }
        );
        _server.MessageDispatcher.AddCallback(
            "/ZIGSIM/octaniPad/arkitrotation2",
            (string address, OscDataHandle data) =>
            {
                _rot.y = data.GetElementAsFloat(0);
            }
        );
        _server.MessageDispatcher.AddCallback(
            "/ZIGSIM/octaniPad/arkitrotation3",
            (string address, OscDataHandle data) =>
            {
                _rot.z = data.GetElementAsFloat(0);
            }
        );
        _server.MessageDispatcher.AddCallback(
            "/ZIGSIM/octaniPad/arkitrotation4",
            (string address, OscDataHandle data) =>
            {
                _rot.w = -data.GetElementAsFloat(0);
            }
        );*/
        _server.MessageDispatcher.AddCallback(
            "/ZIGSIM/octaniPad/touch02",
            (string address, OscDataHandle data) =>
            {
                targetPosition.x = data.GetElementAsFloat(0);
            }
        );
        _server.MessageDispatcher.AddCallback(
            "/ZIGSIM/octaniPad/touch01",
            (string address, OscDataHandle data) =>
            {
                targetPosition.z = data.GetElementAsFloat(0);
            }
        );
    }

    private void OnDestroy()
    {
        _server.Dispose();
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log("botmode : "+botMode);
        //_light.color = _color;
        // if(!camFadingnow && CamControl && cameraValid == 1){
        if(CamControl){
            //go.transform.position = (_arpos+shift - _criterion.position)*mult*hibachiRoot.localScale.x + _criterion.position;
            go.transform.position = new Vector3(offset_scale.x*(_arpos.x - offset.x), offset_scale.y*(_arpos.y - offset.y), offset_scale.z* (_arpos.z - offset.z));
            Debug.Log(_arpos+shift);
            //Debug.Log(_arpos);
        }
        if(cameraValid == 0 && prev_cameraValid == 1 || cameraValid == 1 && prev_cameraValid == 0){
            camFadingnow = true;
        }
        // prev_cameraValid = cameraValid;
        // if(camFadingnow){
        //     if(cameraValid == 0){
        //         camFading += 0.1f;
        //         camFading = Mathf.Min(camFading, 1.0f);
        //         if(camFading == 1.0f){
        //             camFadingnow = false;
        //         }
        //     }else{
        //         camFading -= 0.1f;
        //         camFading = Mathf.Max(camFading, 0.0f);
        //         if(camFading == 0.0f){
        //             camFadingnow = false;
        //         }
        //     }
        //     fading.SetFloat("_Fading", camFading);
        // }
        if (activatorCrontrol)
        {
            activator.position = Vector3.Lerp(activator.position, targetPosition, 0.5f);
        }
        if(debug_activator){
            targetPosition.x = 2*Mathf.Sin(0.05f*10.0f * Time.time)*0.05f;
            targetPosition.z = 3*Mathf.Cos(0.05f*11.0f * Time.time)*0.05f;
            activator.position = Vector3.Lerp(activator.position, targetPosition, 0.5f);
        }
    }
}