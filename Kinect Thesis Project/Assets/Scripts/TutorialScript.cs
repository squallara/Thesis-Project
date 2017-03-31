using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialScript : MonoBehaviour {

    public GameObject player1InputObject, player2InputObject;

    UserInput player1Input, player2Input;

    public GameObject audioPlayer1, audioPlayer2;

    toneHolder audio1, audio2;

    public float tutorialTime;
    float tutorialTimer;

    bool tutorialStarted;

    string userInput1, userInput2;

	// Use this for initialization
	void Start () {

        player1Input = player1InputObject.GetComponent<UserInput>();
        player2Input = player2InputObject.GetComponent<UserInput>();

        audio1 = audioPlayer1.GetComponent<toneHolder>();
        audio2 = audioPlayer2.GetComponent<toneHolder>();

        tutorialStarted = false;

	}
	
	// Update is called once per frame
	void Update () {

        PlayerIO(player1Input, userInput1);
        PlayerIO(player2Input, userInput2);

        if(userInput1 != null || userInput2 != null)
        {
            tutorialStarted = true;

        }

		
	}

    string PlayerIO(UserInput playerInput, string userInput)
    {

        if(playerInput.userInput != null)
        {
            userInput = playerInput.userInput;
        }
        else
        {
            userInput = null;
        }

        return userInput;

    }

    void UserInputNull()
    {
        userInput1 = null;
        userInput2 = null;
    }

}
