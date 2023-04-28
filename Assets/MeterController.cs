// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.InputSystem;

// public class MeterController : MonoBehaviour
// {
//     InputAction mainFader;
    
//     // Start is called before the first frame update
//     void Start()
//     {
//         var playerInput = GetComponent<PlayerInput>();
//         mainFader = playerInput.currentActionMap["MainFader"];

//     }

//     // Update is called once per frame
//     void Update()
//     {
//         var fader =  mainFader.ReadValue<int>();
//         Debug.Log(fader);
//     }
// }
