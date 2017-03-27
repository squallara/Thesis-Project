using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInput : MonoBehaviour {

    public string inputHigh, inputMid, inputLow, depthFar, depthMid, depthClose;

    [HideInInspector]
    public string userInput, userDepth, getInput, depthInput;

    [HideInInspector]
    public bool isPlayer2;

    public AudioSource audioSource;

    // Use this for initialization
    void Start () {

        audioSource = gameObject.GetComponent<AudioSource>();

	}
	
	// Update is called once per frame
	void Update () {

        depthInput = Input.inputString;

        if(depthInput == depthFar || depthInput == depthMid || depthInput == depthClose)
        {
            userDepth = depthInput;
        }

    }
}
