using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class textScript : MonoBehaviour
{
    public GameObject cube;
    [SerializeField] TextMeshProUGUI distanceText; //TextMeshProの変数宣言

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        distanceText.text = this.transform.position.x.ToString(); 
        
    }
}
