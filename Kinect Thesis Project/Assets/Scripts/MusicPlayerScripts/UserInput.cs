using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInput : MonoBehaviour {

    public string InputHigh, InputMid, InputLow;

    [HideInInspector]
    public string userInput, getInput;

    public AudioSource audioSource;

    // Use this for initialization
    void Start () {

        audioSource = gameObject.AddComponent<AudioSource>();

	}
	
	// Update is called once per frame
	void Update () {

        getInput = Input.inputString;

        if (getInput == InputHigh || getInput == InputLow || getInput == InputMid)
        {
            userInput = Input.inputString;
        }

    }
}
