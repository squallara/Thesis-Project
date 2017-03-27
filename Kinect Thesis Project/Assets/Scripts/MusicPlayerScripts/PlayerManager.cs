using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{

    public GameObject player1InputObject, player2InputObject;

    UserInput player1Input, player2Input;

    MusicManager musicManager;

    public bool player2active, player2rythm, repeatTrack;

    string P1lastPlayed, P2lastPlayed;

    int melodicSetPos, rythmSetPos;

    bool canSetLastInputP1, canSetLastInputP2;

    // Use this for initialization
    void Start()
    {

        player1Input = player1InputObject.GetComponent<UserInput>();
        player2Input = player2InputObject.GetComponent<UserInput>();

        player1Input.isPlayer2 = false;
        player2Input.isPlayer2 = true;

        musicManager = GetComponent<MusicManager>();

        melodicSetPos = 0;
        rythmSetPos = 0;

        canSetLastInputP1 = false;
        canSetLastInputP2 = false;
        
    }

    // Update is called once per frame
    void Update()
    {

       // Debug.Log("Player Manager Running");

        PlayerIO(player1Input, ref melodicSetPos);
        PlayerIO(player2Input, ref rythmSetPos);

        Debug.Log("last played P1 = " + P1lastPlayed);

        if (!musicManager.melodyPlayable)
        {
            if (!player2rythm)
            {
                canSetLastInputP2 = true;
            }
            else if (player2rythm)
            {
                canSetLastInputP1 = true;
            }
        }
        if (!musicManager.rythmPlayable)
        {
            if (!player2rythm)
            {
                canSetLastInputP1 = true;
            }
            else if (player2rythm)
            {
                canSetLastInputP2 = true;
            }
        }

    }

    public void PlayerIO(UserInput playerInput, ref int setPos)
    {

        if (playerInput.inputHigh == "" && playerInput.inputMid == "" && playerInput.inputLow == "")
        {
           Debug.LogError("Assign input controls");
        }
        else
        {

            if (playerInput.userInput != null && playerInput.userInput != "")
            {
                if (!musicManager.startMusic)
                {
                  //  Debug.Log("Player Starting Music");
                    musicManager.startMusic = true;
                }
            }


            if (!playerInput.isPlayer2)
            {
                // Debug.Log("Player 1 playing");

                if (player2rythm)
                {
                    
                    if (musicManager.melodyPlayable)
                    {
                        
                        if (musicManager.melodicUseTwoSets && canSetLastInputP1)
                        {
                            canSetLastInputP1 = false;
                            if (P1lastPlayed == null)
                            {
                                P1lastPlayed = playerInput.userInput;
                                Debug.Log("Last input = " + P1lastPlayed);
                            }
                            else if (playerInput.userInput == P1lastPlayed)
                            {
                                if (setPos == 0)
                                {
                                    setPos = 1;
                                }
                                else
                                {
                                    setPos = 0;
                                }
                            }
                            else
                            {
                                P1lastPlayed = playerInput.userInput;
                                //Debug.Log("P1 Last input = " + P1lastPlayed);
                            }
                        }
                        //Debug.Log("Player 1 sending to MusicManager");
                        musicManager.MuteOthersMelodic(setPos, playerInput);
                        musicManager.melodyPlayable = false;
                    }
                }
                else if (!player2rythm)
                {
                    if (musicManager.rythmPlayable)
                    {
                        
                        if (musicManager.rythmUseTwoSets && canSetLastInputP1)
                        {
                            canSetLastInputP1 = false;
                            if (P1lastPlayed == null)
                            {
                                P1lastPlayed = playerInput.userInput;
                                Debug.Log("Last input = " + P1lastPlayed);
                            }
                            else if (playerInput.userInput == P1lastPlayed)
                            {
                                if (setPos == 0)
                                {
                                    setPos = 1;
                                }
                                else
                                {
                                    setPos = 0;
                                }
                            }
                            else
                            {
                                P1lastPlayed = playerInput.userInput;
                                //Debug.Log("P1 Last input = " + P1lastPlayed);
                            }
                        }
                        //Debug.Log("Player 1 sending to MusicManager");
                        musicManager.MuteOthersRythm(setPos, playerInput);
                        musicManager.rythmPlayable = false;
                    }
                }
            }
            else if (playerInput.isPlayer2)
            {
                if (player2rythm)
                {
                    if (musicManager.rythmPlayable)
                    {
                        if (musicManager.rythmUseTwoSets && canSetLastInputP2)
                        {
                            canSetLastInputP2 = false;
                            if (P2lastPlayed == null)
                            {
                                P2lastPlayed = playerInput.userInput;
                                Debug.Log("Last input = " + P2lastPlayed);
                            }
                            else if (playerInput.userInput == P2lastPlayed)
                            {
                                if (setPos == 0)
                                {
                                    setPos = 1;
                                }
                                else
                                {
                                    setPos = 0;
                                }
                            }
                            else
                            {
                                P2lastPlayed = playerInput.userInput;
                                //Debug.Log("P1 Last input = " + P1lastPlayed);
                            }
                        }
                        //Debug.Log("Player 1 sending to MusicManager");
                        musicManager.MuteOthersRythm(setPos, playerInput);
                        musicManager.rythmPlayable = false;
                    }
                }
                else if (!player2rythm)
                {
                    if (musicManager.melodyPlayable)
                    {
                        if (musicManager.melodicUseTwoSets && canSetLastInputP2)
                        {
                            canSetLastInputP2 = false;
                            if (P2lastPlayed == null)
                            {
                                P2lastPlayed = playerInput.userInput;
                                Debug.Log("Last input = " + P2lastPlayed);
                            }
                            else if (playerInput.userInput == P2lastPlayed)
                            {
                                if (setPos == 0)
                                {
                                    setPos = 1;
                                }
                                else
                                {
                                    setPos = 0;
                                }
                            }
                            else
                            {
                                P2lastPlayed = playerInput.userInput;
                                //Debug.Log("P1 Last input = " + P1lastPlayed);
                            }
                        }
                        //Debug.Log("Player 1 sending to MusicManager");
                        musicManager.MuteOthersMelodic(setPos, playerInput);
                        musicManager.melodyPlayable = false;
                    }
                }
            }
        }
    }
}
