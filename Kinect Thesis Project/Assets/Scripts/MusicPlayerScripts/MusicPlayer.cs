using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour {

    int toneSetAmount, rythmToneSetAmount, bassSetAmount, tonePos, rythmPos, bassPos;

    public BeatManager beatMan;

    public UserInput Player1, Player2;

    string p2name;

    public GameObject toneSetObject, rythmSetObject, bassSetObject;

    public toneHolder[] melodicToneSet, rythmToneSet, bassToneSet;

    float toneTimer, rythmTimer, beatManTime1, beatManTime2, toneLength, rythmLength;

    bool rythmPlayable, tonePlayable;

    public bool player2active, usingToneMatch, bassFollowsRythm, player2rythm;

    public float disableTime;
    

	// Use this for initialization
	void Start () {

        InitializeToneSets();

        tonePos = 0;
        rythmPos = 0;
        bassPos = 0;

        toneLength = melodicToneSet[tonePos].timeLength;
        rythmLength = rythmToneSet[rythmPos].timeLength;

        tonePlayable = true;
        rythmPlayable = true;

        p2name = Player2.name;


	}
	
	// Update is called once per frame
	void Update () {

        StatusError();

        beatManTime1 = beatMan.melodicTimer;
        beatManTime2 = beatMan.rythmTimer;

        rythmTimer = rythmTimer - Time.deltaTime;
        toneTimer = toneTimer - Time.deltaTime;

        DisableTones();

        EnableTones();

        ResetBeatManagerTime();

        if (player2rythm)
        {
            Debug.Log("Player 1 Melodic");
            PlayerIO(Player1, melodicToneSet, toneSetAmount, ref tonePos, ref tonePlayable);
            
        }
        else
        {
            Debug.Log("Player 1 Rythm");
            if (bassFollowsRythm)
            {
                Debug.Log("Player 1 rythm, bass follows Rythm");
                BassFollowRythm(Player1);
            }
            else
            {
                Debug.Log("Player 1 Rythm, bass follows beat");
                PlayerIO(Player1, rythmToneSet, rythmToneSetAmount, ref rythmPos, ref rythmPlayable);
            }
            
        }
        

        if (player2active)
        {
            Debug.Log("Player 2 active");
            if (player2rythm)
            {
                Debug.Log("Player 2 Rythm");
                if (bassFollowsRythm)
                {
                    Debug.Log("Player 2 Rythm, bass follows rythm");
                    BassFollowRythm(Player2);
                }
                else
                {
                    Debug.Log("Player 2 Rythm, bass follows beat");
                    PlayerIO(Player2, rythmToneSet, rythmToneSetAmount, ref rythmPos, ref rythmPlayable);
                    
                }
            }
            else
            {
                Debug.Log("Player 2 Melodic");
                PlayerIO(Player2, melodicToneSet, toneSetAmount, ref tonePos, ref tonePlayable);
                
            }
            
        }
        if (!bassFollowsRythm)
        {
            Debug.Log("Bass Follows Beat");
            if (player2rythm)
            {
                BassFollowBeat(Player2, bassToneSet,bassSetAmount,ref rythmPos);
            }
            else
            {
                BassFollowBeat(Player1, bassToneSet, bassSetAmount, ref rythmPos);
            }
        }
        
	}

    public void PlayerIO(UserInput playerInput, toneHolder[] toneSet, int toneSetAmount, ref int tonePos, ref bool isPlayable)
    {

        AudioClip high, mid, low;

        bool isRythmPlayer = false;

        if(player2rythm && playerInput.name == p2name)
        {
            isRythmPlayer = true;
        }
        else if(!player2rythm && playerInput.name != p2name)
        {
            isRythmPlayer = true;
        }

        if (usingToneMatch)
        {
            InputMatcher();
        }  

        if (tonePos >= toneSetAmount)
        {
            tonePos = 0;
        }

        toneHolder currentToneSet = toneSet[tonePos];

        high = currentToneSet.high;
        mid = currentToneSet.mid;
        low = currentToneSet.low;

        if (playerInput.InputHigh == "" && playerInput.InputMid == "" && playerInput.InputLow == "")
        {
            Debug.LogError("Assign input controls");
        }
        else
        {

            if (playerInput.userInput == playerInput.InputHigh && isPlayable == true)
            {
                isPlayable = false;
                
                playerInput.audioSource.PlayOneShot(high);

                if (!usingToneMatch)
                {
                    tonePos++;
                }

                if(isRythmPlayer)
                {
                    rythmLength = toneSet[tonePos].timeLength;
                }
                else
                {
                    toneLength = toneSet[tonePos].timeLength;
                }

                playerInput.userInput = null;

            }

            else if (playerInput.userInput == playerInput.InputMid && isPlayable == true)
            {
                playerInput.audioSource.PlayOneShot(mid);

                if (!usingToneMatch)
                {
                    tonePos++;
                }

                isPlayable = false;

                if (isRythmPlayer)
                {
                    rythmTimer = toneSet[tonePos].timeLength;
                }
                else
                {
                    toneLength = toneSet[tonePos].timeLength;
                }

                playerInput.userInput = null;

            }

            else if(playerInput.userInput == playerInput.InputLow && isPlayable == true)
            {
                playerInput.audioSource.PlayOneShot(low);

                if (!usingToneMatch)
                {
                    tonePos++;
                }

                isPlayable = false;

                if (isRythmPlayer)
                {
                    rythmTimer = toneSet[tonePos].timeLength;
                }
                else
                {
                    toneLength = toneSet[tonePos].timeLength;
                }

                playerInput.userInput = null;

            }
        }

    }

    void StatusError()
    {

        if(Player1 == null || Player2 == null)
        {
            Debug.LogError("Missing Player GameObject. Player 1 = "+Player1.name+". Player 2 = "+Player2.name+".");
        }
        if(toneSetObject == null || rythmSetObject == null)
        {
            Debug.LogError("Missing Tone Set GameObject. Tone Set Object 1 = " + toneSetObject.name + ". Tone Set Object 2 = " + rythmSetObject.name + ".");
        }

    }
    
    void InputMatcher()
    {

        if (Player2.userInput == Player2.InputHigh)
        {
            Debug.Log("Input High Matched");
            tonePos = 0;
        }
        if(Player2.userInput == Player2.InputMid)
        {
            Debug.Log("Input Mid Matched");
            tonePos = 1;
        }
        if(Player2.userInput == Player2.InputLow)
        {
            Debug.Log("Input Low Matched");
            tonePos = 2;
        }

    }

    void DisableTones()
    {

        float beatManThreshold1 = beatMan.toneFollowBeat - disableTime;
        float beatManThreshold2 = beatMan.rythmFollowBeat - disableTime;

        if(beatManTime1 < beatManThreshold1){

            tonePlayable = false;

        }

        if (beatManTime1 < beatManThreshold2)
        {

            rythmPlayable = false;
            beatMan.startBass = false;

        }

    }

    void EnableTones()
    {

        if (rythmTimer <= 0 && beatManTime2 <= 0)
        {
            rythmPlayable = true;
            rythmTimer = rythmLength;
        }
        if (toneTimer <= 0 && beatManTime1 <= 0)
        {
            tonePlayable = true;
            toneTimer = toneLength;
        }

    }

    void ResetBeatManagerTime()
    {

        if (beatManTime1 <= 0)
        {
            beatMan.melodicTimer = beatMan.toneFollowBeat;
        }
        if (beatManTime2 <= 0)
        {
            beatMan.rythmTimer = beatMan.rythmFollowBeat;
        }

    }

    void InitializeToneSets()
    {

        toneSetAmount = toneSetObject.transform.childCount;
        rythmToneSetAmount = rythmSetObject.transform.childCount;
        bassSetAmount = bassSetObject.transform.childCount;

        melodicToneSet = new toneHolder[toneSetAmount];
        rythmToneSet = new toneHolder[rythmToneSetAmount];
        bassToneSet = new toneHolder[bassSetAmount];


        for (int i = 0; i < toneSetAmount; i++)
        {

            melodicToneSet[i] = toneSetObject.transform.GetChild(i).gameObject.GetComponent<toneHolder>();

        }

        for (int i = 0; i < rythmToneSetAmount; i++)
        {

            rythmToneSet[i] = rythmSetObject.transform.GetChild(i).gameObject.GetComponent<toneHolder>();

        }
        for (int i = 0; i < bassSetAmount; i++)
        {
            bassToneSet[i] = bassSetObject.transform.GetChild(i).gameObject.GetComponent<toneHolder>();
        }

    }

    void BassFollowRythm(UserInput playerInput)
    {

        PlayerIO(playerInput, rythmToneSet, rythmToneSetAmount, ref rythmPos, ref rythmPlayable);

        PlayerIO(playerInput, bassToneSet, bassSetAmount, ref bassPos, ref rythmPlayable);

        playerInput.userInput = null;
    }

    void BassFollowBeat(UserInput playerInput, toneHolder[] bassHolder, int toneSetAmount,ref int tonePos)
    {

        AudioClip bassTone;

        if(tonePos >= toneSetAmount)
        {
            tonePos = 0;
        }

        toneHolder currentBassSet;
        currentBassSet = bassHolder[tonePos];

        bassTone = currentBassSet.low;

        if(playerInput.userInput == playerInput.InputHigh)
        {
            bassTone = currentBassSet.high;
        }
        else if(playerInput.userInput == playerInput.InputMid)
        {
            bassTone = currentBassSet.mid;
        }
        else if(playerInput.userInput == playerInput.InputLow)
        {
            bassTone = currentBassSet.low;
        }


        if (beatMan.rythmTimer <= 0)
        {
            beatMan.bassAudioSource.PlayOneShot(bassTone);
        }

    }

}
