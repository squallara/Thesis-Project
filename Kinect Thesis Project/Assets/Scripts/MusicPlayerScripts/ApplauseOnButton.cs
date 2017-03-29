using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplauseOnButton : MonoBehaviour {

    public AudioClip feedback;
   // public Particle fireworks;
    public GameObject fireworksSystem;
    public string inputKey;
    AudioSource applauseSource;
    bool fireworkCountdown;
    float fireworkTimer;

	// Use this for initialization
	void Start () {
        applauseSource = gameObject.AddComponent<AudioSource>();
        fireworksSystem.SetActive(false);
        fireworkCountdown = false;
        fireworkTimer = 5;
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetButtonDown(inputKey) && !applauseSource.isPlaying)
        {
            applauseSource.PlayOneShot(feedback);
            fireworksSystem.SetActive(true);
            fireworkCountdown = true;
        }

        if (fireworkCountdown)
        {
            fireworkTimer = fireworkTimer - Time.deltaTime;
        }
        

        if (fireworkTimer <= 0)
        {
            fireworksSystem.SetActive(false);
            fireworkCountdown = false;
            fireworkTimer = 5;
        }

	}
}
