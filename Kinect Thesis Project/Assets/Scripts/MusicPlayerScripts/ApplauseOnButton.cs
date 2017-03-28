using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplauseOnButton : MonoBehaviour {

    public AudioClip applause;
    AudioSource applauseSource;



	// Use this for initialization
	void Start () {
        applauseSource = gameObject.AddComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetButtonDown("Jump") && !applauseSource.isPlaying)
        {
            applauseSource.PlayOneShot(applause);
        }

	}
}
