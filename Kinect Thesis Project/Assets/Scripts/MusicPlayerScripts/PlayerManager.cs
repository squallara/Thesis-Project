using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour {

    public GameObject player1InputObject, player2InputObject;

    UserInput player1Input, player2Input;

    MusicManager musicManager;

    public bool player2active, player2rythm, repeatTrack;

    string P1lastPlayed, P2lastPlayed;

    int melodicSetPos, rythmSetPos;

    // Use this for initialization
    void Start () {

        player1Input.isPlayer2 = false;
        player2Input.isPlayer2 = true;

        musicManager = GetComponent<MusicManager>();

        melodicSetPos = 0;
        rythmSetPos = 0;

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void PlayerIO(UserInput playerInput, ref int setPos)
    {

        if (playerInput.inputHigh == "" && playerInput.inputMid == "" && playerInput.inputLow == "")
        {
            Debug.LogError("Assign input controls");
        }
        else
        {
            musicManager.MuteOthers(setPos, playerInput);

            if (!playerInput.isPlayer2)
            {
                P1lastPlayed = playerInput.userInput;
            }
        }

    }

}
