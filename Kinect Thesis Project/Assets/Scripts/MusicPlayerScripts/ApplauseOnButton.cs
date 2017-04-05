using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplauseOnButton : MonoBehaviour {

    public AudioClip feedback;
   // public Particle fireworks;
    public GameObject visualFeedback;
    public string inputKey;
    public float visualDisplayTime;
    public bool isFireworks;
    AudioSource applauseSource;
    bool visualCountdown;
    float visualTimer;

    GameObject[] fireworksObjects;
    ParticleSystem[] fireworksSystem;

	// Use this for initialization
	void Start () {
        applauseSource = gameObject.AddComponent<AudioSource>();

        if (isFireworks)
        {
            GetParticleSystem();
        }
        if (isFireworks)
        {
            visualFeedback.SetActive(true);
            StopParticleSystem();
        }
        else
        {
            visualFeedback.SetActive(false);
        } 
        visualCountdown = false;
        visualTimer = visualDisplayTime;

    }
	
	// Update is called once per frame
	void Update () {

        if (/*Input.GetButtonDown(inputKey)*/ Manager.didHigh5 == true && !applauseSource.isPlaying)
        {
            applauseSource.PlayOneShot(feedback);
            
            if (isFireworks)
            {
                PlayParticleSystem();
            }
            else
            {
                visualFeedback.SetActive(true);
            }
            visualCountdown = true;
        }

        if (visualCountdown)
        {
            visualTimer = visualTimer - Time.deltaTime;
        }
        

        if (visualTimer <= 0)
        {
            Manager.didHigh5 = false;       //I don't know if it is the correct placement here. It plays two applauses before it stops.
            if (isFireworks)
            {
                StopParticleSystem();
            }
            else
            {
                visualFeedback.SetActive(false);
            }
            
            visualCountdown = false;
            visualTimer = visualDisplayTime;
        }

	}

    void GetParticleSystem()
    {
        int childObjectAmount;

        childObjectAmount = visualFeedback.transform.childCount;
        fireworksSystem = new ParticleSystem[childObjectAmount];

        for(int i = 0; i < childObjectAmount; i++)
        {
            fireworksSystem[i] = visualFeedback.transform.GetChild(i).GetComponent<ParticleSystem>();
        }

    }

    void PlayParticleSystem()
    {
        for(int i = 0; i < fireworksSystem.Length; i++)
        {
            fireworksSystem[i].Play(true);
        }
    }

    void StopParticleSystem()
    {
        for (int i = 0; i < fireworksSystem.Length; i++)
        {
            fireworksSystem[i].Stop(true);
        }
    }

}
