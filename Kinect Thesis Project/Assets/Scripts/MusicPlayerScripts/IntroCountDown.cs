using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntroCountDown : MonoBehaviour {

    public GameObject introTextObject;
    public GameObject[] countDownObjects;

    Text introText;
    Shadow introTextShadow;

    public float introTime;
    float timer;

    [HideInInspector]
    public bool introIsPlaying;

    bool countDownActive;

    // Use this for initialization
    void Awake()
    {
        introIsPlaying = false;
    }

    void Start () {

        introText = introTextObject.GetComponent<Text>();
        introTextShadow = introTextObject.GetComponent<Shadow>();

        InitializeCountDownText();
        DeactivateAll();

        timer = introTime;
	}
	
	// Update is called once per frame
	void Update () {

        if (introIsPlaying && timer != -1)
        {
            timer = timer - Time.deltaTime;

            if(timer > 3)
            {    
                ActivateText(true);
            }
            else if(timer <= 3 && timer > 2)
            {
                ActivateText(false);
                ActivateCountDown(3, true);
            }
            else if(timer <= 2 && timer > 1)
            {
                ActivateCountDown(3, false);
                ActivateCountDown(2, true);
            }
            else if(timer <= 1 && timer > 0)
            {
                ActivateCountDown(2, false);
                ActivateCountDown(1, true);
            }
            else if(timer <= 0 && timer > -1)
            {
                ActivateCountDown(1, false);
                ActivateCountDown(0, true);
            }
            else
            {
                DeactivateAll();
                //print("Now I am playing");
                timer = -1;
            }
        }


	}

    void InitializeCountDownText()
    {
        int textObjectAmount = introTextObject.transform.childCount;

        countDownObjects = new GameObject[textObjectAmount];

        for(int i = 0; i < textObjectAmount; i++)
        {
            countDownObjects[i] = introTextObject.transform.GetChild(i).gameObject;
        }


    }

    void DeactivateAll()
    {


        for(int i = 0; i < countDownObjects.Length; i++)
        {
            countDownObjects[i].SetActive(false);
        }

        introText.enabled = false;
        introTextShadow.enabled = false;

    }

    void ActivateText(bool setActive)
    {
        introText.enabled = setActive;
        introTextShadow.enabled = setActive;
    }

    void ActivateCountDown(int numberToActivate, bool setActive)
    {
        countDownObjects[numberToActivate].SetActive(setActive);
    }

}
