﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class MusicPlayer : MonoBehaviour
{

    int toneSetAmount, rythmToneSetAmount, bassSetAmount, tonePos, rythmPos, bassPos;

    public BeatManager beatMan;

    public UserInput Player1, Player2;

    UserInput bassInput;

    string p2name;

    /*

        Use Gameobjects instead of tonesets to manage the possible toneHolders.
        Use single tone holder objects instead of arrays, to initialize toneholders
        TonePos might not be needed
    */

    public GameObject mainToneSetObject, mainRythmSetObject, mainBassSetObject;

    GameObject[] melodicToneSetObject, rythmToneSetObject, bassToneSetObject;

    toneHolder[] melodicToneSet, rythmToneSet, bassToneSet;

    float mainTrackTimer;

    float toneTimer, rythmTimer, beatManTime1, beatManTime2, toneLength, rythmLength;

    bool rythmPlayable, tonePlayable, bassPlayable;

    public bool player2active, usingToneMatch, bassFollowsRythm, player2rythm, useSeperateBass, useMidInput;

    public float disableTime;

    public float mainTrackTime, introTime, versTime, bridgeTime, chorusTime, vers2Time, bridge2Time;

    public int rythmIntroPos, rythmVersPos, rythmBridgePos, rythmChorPos, rythmVers2Pos, rythmBridge2Pos, rythmChor2Pos;

    public AudioMixer EQMixer;

    string rythmMixGroup = "Rythm", toneMixGroup = "Tone";

    bool MixerSet;

    bool P2isRythm;

    public bool repeatTrack;

    string lastPlayed;

    AudioMixerSnapshot[] snapshots;




    // Use this for initialization
    void Start()
    {

        InitializeToneSets(mainToneSetObject, mainRythmSetObject, mainBassSetObject);

        bassInput = beatMan.GetComponent<UserInput>();

        tonePos = 0;
        rythmPos = 0;
        bassPos = 0;

        toneLength = melodicToneSet[tonePos].timeLength;
        rythmLength = rythmToneSet[rythmPos].timeLength;

        tonePlayable = true;
        rythmPlayable = true;
        bassPlayable = true;

        p2name = Player2.name;

        MixerSet = false;

        P2isRythm = player2rythm;
        snapshots = new AudioMixerSnapshot[3];



    }

    // Update is called once per frame
    void Update()
    {

        StatusError();

        SoundtrackTimeManager();

        mainTrackTimer = mainTrackTimer + Time.deltaTime;

        //Debug.Log("Main track time = " + mainTrackTimer);

        beatManTime1 = beatMan.melodicTimer;
        beatManTime2 = beatMan.rythmTimer;

        rythmTimer = rythmTimer - Time.deltaTime;
        toneTimer = toneTimer - Time.deltaTime;

        if (player2rythm)
        {

            if (!MixerSet)
            {
                GetMixers(Player2, EQMixer, rythmMixGroup);
                GetMixers(Player1, EQMixer, toneMixGroup);
                MixerSet = true;
            }

        }
        else
        {

            if (!MixerSet)
            {
                GetMixers(Player1, EQMixer, rythmMixGroup);
                GetMixers(Player2, EQMixer, toneMixGroup);
                MixerSet = true;
            }

        }

        if (player2rythm != P2isRythm)
        {
            MixerSet = false;
            P2isRythm = player2rythm;
        }

        DisableTones();

        EnableTones();

        ResetBeatManagerTime();

        if (player2rythm)
        {
            //Debug.Log("Player 1 Melodic");
            PlayerIO(Player1, melodicToneSet, toneSetAmount, ref tonePos, ref tonePlayable);

        }
        else
        {
            //Debug.Log("Player 1 Rythm");
            if (bassFollowsRythm)
            {
                if (useSeperateBass)
                {
                    SetBassInput(Player1);
                    // Debug.Log("Player 1 rythm, bass follows Rythm");
                    BassFollowRythm(Player1);
                }
                else
                {
                    PlayerIO(Player1, rythmToneSet, rythmToneSetAmount, ref rythmPos, ref rythmPlayable);
                }
            }
            else
            {
                //Debug.Log("Player 1 Rythm, bass follows beat");
                PlayerIO(Player1, rythmToneSet, rythmToneSetAmount, ref rythmPos, ref rythmPlayable);

            }

        }


        if (player2active)
        {
            //Debug.Log("Player 2 active");
            if (player2rythm)
            {
                //Debug.Log("Player 2 Rythm");
                if (bassFollowsRythm)
                {
                    if (useSeperateBass)
                    {
                        SetBassInput(Player2);
                        // Debug.Log("Player 2 Rythm, bass follows rythm");
                        BassFollowRythm(Player2);
                    }
                    else
                    {
                        PlayerIO(Player2, rythmToneSet, rythmToneSetAmount, ref rythmPos, ref rythmPlayable);
                    }
                }
                else
                {
                    // Debug.Log("Player 2 Rythm, bass follows beat");
                    PlayerIO(Player2, rythmToneSet, rythmToneSetAmount, ref rythmPos, ref rythmPlayable);

                }
            }
            else
            {
                // Debug.Log("Player 2 Melodic");
                PlayerIO(Player2, melodicToneSet, toneSetAmount, ref tonePos, ref tonePlayable);
                //Debug.Log("Userinput P2 = " + Player2.userInput);

            }

        }
        if (!bassFollowsRythm)
        {
            //Debug.Log("Bass Follows Beat");
            if (player2rythm)
            {
                BassFollowBeat(Player2, bassToneSet, bassSetAmount, ref rythmPos);
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

        //Debug.Log("Looking for input " + playerInput.userInput);

        bool isRythmPlayer = false;

        if (player2rythm && playerInput.name == p2name)
        {
            isRythmPlayer = true;
        }
        else if (!player2rythm && playerInput.name != p2name)
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


        if (playerInput.userInput == lastPlayed)
        {
            if (lastPlayed == playerInput.inputHigh)
            {
                tonePos = toneSet[tonePos].tonesetRefHigh;
            }
            else if (lastPlayed == playerInput.inputLow)
            {
                tonePos = toneSet[tonePos].tonesetRefLow;
            }
        }


        toneHolder currentToneSet = toneSet[tonePos];

        //Debug.Log("TonePos = " + tonePos);

        high = currentToneSet.high;
        mid = currentToneSet.mid;
        low = currentToneSet.low;



        if (playerInput.inputHigh == "" && playerInput.inputMid == "" && playerInput.inputLow == "")
        {
            //Debug.LogError("Assign input controls");
        }
        else
        {

            if (playerInput.userInput == playerInput.inputHigh && isPlayable == true)
            {
                isPlayable = false;

                playerInput.audioSource.PlayOneShot(high);
                mainTrackTimer = 0;
                ActivateDrums();

                if (isRythmPlayer)
                {

                    //InitializeToneSets(melo, rythmToneSet, bassToneSet);
                    rythmLength = toneSet[tonePos].timeLength;

                }
                else
                {
                    toneLength = toneSet[tonePos].timeLength;
                }

                playerInput.userInput = null;
                lastPlayed = "high";

                if (tonePos == 0)
                {
                    tonePos = toneSet[tonePos].tonesetRefHigh;
                }

            }

            else if (playerInput.userInput == playerInput.inputMid && isPlayable == true && useMidInput == true)
            {
                playerInput.audioSource.PlayOneShot(mid);
                mainTrackTimer = 0;
                ActivateDrums();

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

            else if (playerInput.userInput == playerInput.inputLow && isPlayable == true)
            {
                playerInput.audioSource.PlayOneShot(low);
                mainTrackTimer = 0;
                ActivateDrums();

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
                lastPlayed = "low";
                if (tonePos == 0)
                {
                    tonePos = toneSet[tonePos].tonesetRefLow;
                }
                //tonePos = currentToneSet.tonesetRefLow;

            }
        }

    }

    public void PlayerDepth(UserInput P1Input, UserInput P2Input)
    {

        if (P1Input.userDepth == P1Input.depthFar)
        {
            //Player1.audioSource.outputAudioMixerGroup.audioMixer.TransitionToSnapshots(snapshots[0],25,1);
        }


    }

    void StatusError()
    {

        if (Player1 == null || Player2 == null)
        {
            //Debug.LogError("Missing Player GameObject. Player 1 = " + Player1.name + ". Player 2 = " + Player2.name + ".");
        }
        if (mainToneSetObject == null || mainRythmSetObject == null)
        {
            //Debug.LogError("Missing Tone Set GameObject. Tone Set Object 1 = " + mainToneSetObject.name + ". Tone Set Object 2 = " + mainRythmSetObject.name + ".");
        }

    }

    void InputMatcher()
    {

        if (player2rythm)
        {
            if (Player2.userInput == Player2.inputHigh)
            {
                //Debug.Log("Input High Matched");
                tonePos = 0;
            }
            if (Player2.userInput == Player2.inputMid)
            {
                //Debug.Log("Input Mid Matched");
                tonePos = 1;
            }
            if (Player2.userInput == Player2.inputLow)
            {
                //Debug.Log("Input Low Matched");
                tonePos = 2;
            }
        }
        else
        {
            if (Player1.userInput == Player1.inputHigh)
            {
                //Debug.Log("Input High Matched");
                tonePos = 0;
            }
            if (Player1.userInput == Player1.inputMid)
            {
                //Debug.Log("Input Mid Matched");
                tonePos = 1;
            }
            if (Player1.userInput == Player1.inputLow)
            {
                //Debug.Log("Input Low Matched");
                tonePos = 2;
            }
        }

    }

    void DisableTones()
    {

        float beatManThreshold1 = beatMan.toneFollowBeat - disableTime;
        float beatManThreshold2 = beatMan.rythmFollowBeat - disableTime;

        if (beatManTime1 < beatManThreshold1)
        {

            tonePlayable = false;

        }

        if (beatManTime2 < beatManThreshold2)
        {

            rythmPlayable = false;
            bassPlayable = false;
            beatMan.startBass = false;

        }

    }

    void EnableTones()
    {

        if (rythmTimer <= 0 && beatManTime2 <= 0)
        {
            rythmPlayable = true;
            bassPlayable = true;
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

    void InitializeToneSets(GameObject tonesetObject, GameObject rythmObject, GameObject bassObject)
    {

        toneSetAmount = tonesetObject.transform.childCount;
        rythmToneSetAmount = rythmObject.transform.childCount;
        bassSetAmount = bassObject.transform.childCount;

        melodicToneSet = new toneHolder[toneSetAmount];
        rythmToneSet = new toneHolder[rythmToneSetAmount];
        bassToneSet = new toneHolder[bassSetAmount];


        for (int i = 0; i < toneSetAmount; i++)
        {

            melodicToneSet[i] = tonesetObject.transform.GetChild(i).gameObject.GetComponent<toneHolder>();

        }

        for (int i = 0; i < rythmToneSetAmount; i++)
        {

            rythmToneSet[i] = rythmObject.transform.GetChild(i).gameObject.GetComponent<toneHolder>();

        }
        for (int i = 0; i < bassSetAmount; i++)
        {
            bassToneSet[i] = bassObject.transform.GetChild(i).gameObject.GetComponent<toneHolder>();
        }

    }

    void BassFollowRythm(UserInput playerInput)
    {

        BassPlayer(playerInput, bassToneSet, bassSetAmount, ref bassPos, ref bassPlayable);

        PlayerIO(playerInput, rythmToneSet, rythmToneSetAmount, ref rythmPos, ref rythmPlayable);

    }

    void BassFollowBeat(UserInput playerInput, toneHolder[] bassHolder, int toneSetAmount, ref int tonePos)
    {

        AudioClip bassTone;

        if (tonePos >= toneSetAmount)
        {
            tonePos = 0;
        }

        toneHolder currentBassSet;
        currentBassSet = bassHolder[tonePos];

        bassTone = currentBassSet.low;

        if (playerInput.userInput == playerInput.inputHigh)
        {
            bassTone = currentBassSet.high;
        }
        else if (playerInput.userInput == playerInput.inputMid)
        {
            bassTone = currentBassSet.mid;
        }
        else if (playerInput.userInput == playerInput.inputLow)
        {
            bassTone = currentBassSet.low;
        }


        if (beatMan.rythmTimer <= 0)
        {
            beatMan.bassAudioSource.PlayOneShot(bassTone);
        }

    }

    void BassPlayer(UserInput playerInput, toneHolder[] bassHolder, int toneSetAmount, ref int tonesetPos, ref bool isBassPlayable)
    {

        AudioClip highBass, midBass, lowBass;

        if (tonesetPos >= toneSetAmount)
        {
            tonesetPos = 0;
        }

        toneHolder currentBass = bassHolder[tonesetPos];

        highBass = currentBass.high;
        midBass = currentBass.mid;
        lowBass = currentBass.low;

        if (playerInput.inputHigh == "" && playerInput.inputMid == "" && playerInput.inputLow == "")
        {
            Debug.LogError("Assign input controls");
        }
        else
        {
            if (playerInput.userInput == playerInput.inputHigh && isBassPlayable == true)
            {
                beatMan.bassAudioSource.PlayOneShot(highBass);
                isBassPlayable = false;
            }
            else if (playerInput.userInput == playerInput.inputMid && isBassPlayable == true)
            {
                beatMan.bassAudioSource.PlayOneShot(midBass);
                isBassPlayable = false;
            }
            else if (playerInput.userInput == playerInput.inputLow && isBassPlayable == true)
            {
                beatMan.bassAudioSource.PlayOneShot(lowBass);
                isBassPlayable = false;
            }
        }


    }


    void SetBassInput(UserInput userInput)
    {
        bassInput.inputHigh = userInput.inputHigh;
        bassInput.inputMid = userInput.inputMid;
        bassInput.inputLow = userInput.inputLow;
        bassInput.userInput = userInput.userInput;

        bassInput.getInput = userInput.getInput;
    }

    void GetMixers(UserInput player, AudioMixer mixer, string mixerGroup)
    {

        player.audioSource.outputAudioMixerGroup = mixer.FindMatchingGroups(mixerGroup)[0];

    }

    void SetSnapshots()
    {

        snapshots[0] = EQMixer.FindSnapshot("Far");
        snapshots[1] = EQMixer.FindSnapshot("Mid");
        snapshots[2] = EQMixer.FindSnapshot("Close");

    }

    void SoundtrackTimeManager()
    {
        float offset = 0.5f;

        if (mainTrackTimer >= mainTrackTime)
        {
            mainTrackTimer = 0;
            beatMan.bassAudioSource.Stop();
        }

        if (mainTrackTimer == introTime - offset && introTime != 0)
        {
            rythmPos = rythmIntroPos;
        }
        else if (introTime == 0 && mainTrackTimer == introTime)
        {
            rythmPos = rythmIntroPos;
        }
        if (mainTrackTimer == versTime - offset && versTime != 0)
        {
            rythmPos = rythmVersPos;
        }
        else if (versTime == 0 && mainTrackTimer == versTime)
        {
            rythmPos = rythmVersPos;
        }




    }

    void ActivateDrums()
    {
        beatMan.useBeat = true;
    }


}
