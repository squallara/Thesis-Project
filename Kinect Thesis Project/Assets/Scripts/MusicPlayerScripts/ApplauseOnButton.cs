using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplauseOnButton : MonoBehaviour {

    public AudioClip feedback;
   // public Particle fireworks;
    public GameObject visualFeedback;
    public string inputKey;
    public float visualDisplayTime;
    AudioSource applauseSource;
    bool visualCountdown;
    float visualTimer;

	// Use this for initialization
	void Start () {
        applauseSource = gameObject.AddComponent<AudioSource>();
        visualFeedback.SetActive(false);
        visualCountdown = false;
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetButtonDown(inputKey) && !applauseSource.isPlaying)
        {
            applauseSource.PlayOneShot(feedback);
            visualFeedback.SetActive(true);
            visualCountdown = true;
        }

        if (visualCountdown)
        {
            visualTimer = visualTimer - Time.deltaTime;
        }
        

        if (visualTimer <= 0)
        {
            visualFeedback.SetActive(false);
            visualCountdown = false;
            visualTimer = visualDisplayTime;
        }

	}
}
