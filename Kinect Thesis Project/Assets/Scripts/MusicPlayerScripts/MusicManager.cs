using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{

    public GameObject melodicObject, rythmObject;

    toneHolder[] melodicSets, rythmSets;
    int melodicSetAmount, rythmSetAmount;

    public AudioClip drumClip;
    AudioClip rythmSetOneHigh, rythmSetOneLow, rythmSetTwoHigh, rythmSetTwoLow;
    AudioClip melodicSetOneHigh, melodicSetOneLow, melodicSetTwoHigh, melodicSetTwoLow;

    AudioSource drumSource;
    AudioSource rythmSourceOneHigh, rythmSourceOneLow, rythmSourceTwoHigh, rythmSourceTwoLow;
    AudioSource melodicSourceOneHigh, melodicSourceOneLow, melodicSourceTwoHigh, melodicSourceTwoLow;

    public bool rythmUseTwoSets, melodicUseTwoSets;

    [HideInInspector]
    public bool startMusic;

    public float mainTrackTime;
    float mainTrackTimer;

    public float melodicBeatTime, rythmBeatTime, beatTime;
    public float beatTimer;

    

    // Use this for initialization
    void Start()
    {
        startMusic = false;
        
    }

    // Update is called once per frame
    void Update()
    {

        if (startMusic)
        {
            MutePlayers();
            StartMusic();
        }

    }

    void GetToneSets()
    {
        melodicSetAmount = melodicObject.transform.childCount;
        rythmSetAmount = rythmObject.transform.childCount;

        melodicSets = new toneHolder[melodicSetAmount];
        rythmSets = new toneHolder[rythmSetAmount];

        for (int i = 0; i < melodicSetAmount; i++)
        {
            melodicSets[i] = melodicObject.transform.GetChild(i).gameObject.GetComponent<toneHolder>();
        }
        for (int i = 0; i < rythmSetAmount; i++)
        {
            rythmSets[i] = rythmObject.transform.GetChild(i).gameObject.GetComponent<toneHolder>();
        }

    }

    void AudioSetup()
    {
        drumSource = gameObject.AddComponent<AudioSource>();
        melodicSourceOneHigh = melodicObject.AddComponent<AudioSource>();
        melodicSourceOneLow = melodicObject.AddComponent<AudioSource>();
        melodicSourceTwoHigh = melodicObject.AddComponent<AudioSource>();
        melodicSourceTwoLow = melodicObject.AddComponent<AudioSource>();

        rythmSourceOneHigh = rythmObject.AddComponent<AudioSource>();
        rythmSourceOneLow = rythmObject.AddComponent<AudioSource>();
        rythmSourceTwoHigh = rythmObject.AddComponent<AudioSource>();
        rythmSourceTwoLow = rythmObject.AddComponent<AudioSource>();

        melodicSetOneHigh = melodicSets[0].high;
        melodicSetOneLow = melodicSets[0].low;

        if (melodicUseTwoSets)
        {
            melodicSetTwoHigh = melodicSets[1].high;
            melodicSetTwoLow = melodicSets[1].low;
        }
        rythmSetOneHigh = rythmSets[0].high;
        rythmSetOneLow = rythmSets[0].low;

        if (rythmUseTwoSets)
        {
            rythmSetTwoHigh = rythmSets[1].high;
            rythmSetTwoLow = rythmSets[1].low;
        }
    }

    void StartMusic()
    {

        drumSource.PlayOneShot(drumClip);

        melodicSourceOneHigh.PlayOneShot(melodicSetOneHigh);
        melodicSourceOneLow.PlayOneShot(melodicSetOneLow);

        if (melodicUseTwoSets)
        {
            melodicSourceTwoHigh.PlayOneShot(melodicSetTwoHigh);
            melodicSourceTwoLow.PlayOneShot(melodicSetTwoLow);
        }

        rythmSourceOneHigh.PlayOneShot(rythmSetOneHigh);
        rythmSourceOneLow.PlayOneShot(rythmSetOneLow);

        if (rythmUseTwoSets)
        {
            rythmSourceTwoHigh.PlayOneShot(rythmSetTwoHigh);
            rythmSourceTwoLow.PlayOneShot(rythmSetTwoLow);
        }
    }

    public void MuteOthers(int setPos, UserInput userInput)
    {
        if (!userInput.isPlayer2)
        {

            Debug.Log("Player 1");
            if (setPos == 0 && userInput.userInput == userInput.inputHigh)
            {
                Debug.Log("Player 1, Set " + setPos + ", input " + userInput.userInput);

                melodicSourceOneHigh.mute = false;
                melodicSourceOneLow.mute = true;

                if (melodicUseTwoSets)
                {
                    melodicSourceTwoHigh.mute = true;
                    melodicSourceTwoLow.mute = true;
                }
            }
            else if (setPos == 0 && userInput.userInput == userInput.inputLow)
            {
                Debug.Log("Player 1, Set " + setPos + ", input " + userInput.userInput);
                melodicSourceOneHigh.mute = true;
                melodicSourceOneLow.mute = false;

                if (melodicUseTwoSets)
                {
                    melodicSourceTwoHigh.mute = true;
                    melodicSourceTwoLow.mute = true;
                }
            }
            else if (setPos == 1 && userInput.userInput == userInput.inputHigh)
            {
                Debug.Log("Player 1, Set " + setPos + ", input " + userInput.userInput);
                melodicSourceOneHigh.mute = true;
                melodicSourceOneLow.mute = true;

                melodicSourceTwoHigh.mute = false;
                melodicSourceTwoLow.mute = true;

            }
            else if (setPos == 1 && userInput.userInput == userInput.inputLow)
            {
                Debug.Log("Player 1, Set " + setPos + ", input " + userInput.userInput);
                melodicSourceOneHigh.mute = true;
                melodicSourceOneLow.mute = true;

                melodicSourceTwoHigh.mute = true;
                melodicSourceTwoLow.mute = false;
            }
            else
            {
                melodicSourceOneHigh.mute = true;
                melodicSourceOneLow.mute = true;

                melodicSourceTwoHigh.mute = true;
                melodicSourceTwoLow.mute = true;
            }
        }

        if (userInput.isPlayer2)
        {
            if (setPos == 0 && userInput.userInput == userInput.inputHigh)
            {
                Debug.Log("Player 2, Set " + setPos + ", input " + userInput.userInput);
                rythmSourceOneHigh.mute = false;
                rythmSourceOneLow.mute = true;

                if (rythmUseTwoSets)
                {
                    rythmSourceTwoHigh.mute = true;
                    rythmSourceTwoLow.mute = true;
                }
            }
            else if (setPos == 0 && userInput.userInput == userInput.inputLow)
            {
                Debug.Log("Player 2, Set " + setPos + ", input " + userInput.userInput);
                rythmSourceOneHigh.mute = true;
                rythmSourceOneLow.mute = false;

                if (rythmUseTwoSets)
                {
                    rythmSourceTwoHigh.mute = true;
                    rythmSourceTwoLow.mute = true;
                }
            }
            else if (setPos == 1 && userInput.userInput == userInput.inputHigh)
            {
                Debug.Log("Player 2, Set " + setPos + ", input " + userInput.userInput);
                rythmSourceOneHigh.mute = true;
                rythmSourceOneLow.mute = true;

                rythmSourceTwoHigh.mute = false;
                rythmSourceTwoLow.mute = true;


            }
            else if (setPos == 1 && userInput.userInput == userInput.inputLow)
            {
                Debug.Log("Player 2, Set " + setPos + ", input " + userInput.userInput);
                rythmSourceOneHigh.mute = true;
                rythmSourceOneLow.mute = true;

                rythmSourceTwoHigh.mute = true;
                rythmSourceTwoLow.mute = false;
            }
            else
            {
                Debug.Log("Player 2, Set " + setPos + ", input " + userInput.userInput);
                rythmSourceOneHigh.mute = true;
                rythmSourceOneLow.mute = true;

                rythmSourceTwoHigh.mute = true;
                rythmSourceTwoLow.mute = true;
            }
        }

    }

    public void MuteAll()
    {

        drumSource.mute = true;

        melodicSourceOneHigh.mute = true;
        melodicSourceOneLow.mute = true;
        melodicSourceTwoHigh.mute = true;
        melodicSourceTwoLow.mute = true;

        rythmSourceOneHigh.mute = true;
        rythmSourceOneLow.mute = true;
        rythmSourceTwoHigh.mute = true;
        rythmSourceTwoLow.mute = true;
    }

    public void MutePlayers()
    {
        melodicSourceOneHigh.mute = true;
        melodicSourceOneLow.mute = true;
        melodicSourceTwoHigh.mute = true;
        melodicSourceTwoLow.mute = true;

        rythmSourceOneHigh.mute = true;
        rythmSourceOneLow.mute = true;
        rythmSourceTwoHigh.mute = true;
        rythmSourceTwoLow.mute = true;
    }

    public void ResetMusic()
    {

        startMusic = false;

        drumSource.Stop();

        melodicSourceOneHigh.Stop();
        melodicSourceOneLow.Stop();
        melodicSourceTwoHigh.Stop();
        melodicSourceTwoLow.Stop();

        rythmSourceOneHigh.Stop();
        rythmSourceOneLow.Stop();
        rythmSourceTwoHigh.Stop();
        rythmSourceTwoLow.Stop();

    }

}


