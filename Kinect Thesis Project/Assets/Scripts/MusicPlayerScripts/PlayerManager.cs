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

    }

    // Update is called once per frame
    void Update()
    {

       // Debug.Log("Player Manager Running");

        PlayerIO(player1Input, ref melodicSetPos);
        PlayerIO(player2Input, ref rythmSetPos);
    }

    public void PlayerIO(UserInput playerInput, ref int setPos)
    {

        if (playerInput.inputHigh == "" && playerInput.inputMid == "" && playerInput.inputLow == "")
        {
           // Debug.LogError("Assign input controls");
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

            if (!playerInput.isPlayer2 && musicManager.melodicUseTwoSets)
            {
              //  Debug.Log("Player 1");
                if (P1lastPlayed == null)
                {
                    P1lastPlayed = playerInput.userInput;
                   // Debug.Log("P1 Last input = "+P1lastPlayed);
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
            else if (playerInput.isPlayer2 && musicManager.rythmUseTwoSets)
            {
                if (P2lastPlayed == null)
                {
                    P2lastPlayed = playerInput.userInput;
                    //Debug.Log("P2 Last input = " + P2lastPlayed);
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
                    //Debug.Log("P2 Last input = " + P2lastPlayed);
                }
            }

            if (!playerInput.isPlayer2)
            {
               // Debug.Log("Player 1 playing");
                if (musicManager.melodyPlayable)
                {
                    //Debug.Log("Player 1 sending to MusicManager");
                    musicManager.MuteOthers(setPos, playerInput);
                    musicManager.melodyPlayable = false;
                }
            }
            else if (playerInput.isPlayer2)
            {
               // Debug.Log("Player 2 playing");
                if (musicManager.rythmPlayable)
                {
                   // Debug.Log("Player 2 sending to MusicManager");
                    musicManager.MuteOthers(setPos, playerInput);
                    musicManager.rythmPlayable = false;
                }
            }



        }

    }

}
