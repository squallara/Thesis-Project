using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInput : MonoBehaviour {

    //preset input strings. Input variables must match these strings for input
    [HideInInspector]
    public string inputHigh, inputMid, inputLow, targetHandHighInput, targetHandLowInput, targetForwardInput, targetBackInput;

    //input variables to set an input to
    //public string handInput1, handInput2, forwardInput, backInput;

    [HideInInspector]
    public string userInput, userDepth, getInput, depthInput;

    [HideInInspector]
    public bool isPlayer2;

    public AudioSource audioSource;

    // Use this for initialization
    void Start () {

        inputHigh = "high";
        inputLow = "low";

        targetHandHighInput = "thh";
        targetHandLowInput = "thl";

        targetForwardInput = "fwd";
        targetBackInput = "bwd";

        audioSource = gameObject.GetComponent<AudioSource>();

	}
	
}
