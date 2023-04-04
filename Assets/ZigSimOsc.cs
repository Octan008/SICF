using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using OscJack;

public class ZigSimOsc : MonoBehaviour {

    public GameObject go;
    public int port = 10000;
    public float mult = 10.0f;
    public Transform hibachiRoot;

    private OscServer _server;
    private Vector3 _arpos;
    private Quaternion _rot;
    public Transform _criterion;
    public Vector3 shift;
    private string _dspBusName;
    public bool CamControl = false;

    public Transform activator;
    public Vector3 targetPosition;
    

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
                _arpos.z = -data.GetElementAsFloat(0);
            }
        );
        _server.MessageDispatcher.AddCallback(
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
        );
        _server.MessageDispatcher.AddCallback(
            "/ZIGSIM/octaniPad/touch02",
            (string address, OscDataHandle data) =>
            {
                targetPosition.x = data.GetElementAsFloat(0) * 10;
            }
        );
        _server.MessageDispatcher.AddCallback(
            "/ZIGSIM/octaniPad/touch01",
            (string address, OscDataHandle data) =>
            {
                targetPosition.z = data.GetElementAsFloat(0) * 10;
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
        //_light.color = _color;
        if(CamControl){
            // go.transform.rotation = _rot;
            // go.transform.localPosition = (_arpos+shift - _criterion.position)*mult*hibachiRoot.localScale.x + _criterion.position;
            go.transform.position = (_arpos+shift - _criterion.position)*mult*hibachiRoot.localScale.x + _criterion.position;
            Debug.Log(_arpos+shift);
            //Debug.Log(_arpos);
        }
        activator.position = Vector3.Lerp(activator.position, targetPosition, 0.5f);
    }
}