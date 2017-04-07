using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialProgress : MonoBehaviour {

    int counter;
    string text;

	// Use this for initialization
	void Start () {

        counter = 0;
        text = GetComponent<Text>().text;
	}
	
	// Update is called once per frame
	void Update () {


		if(Input.GetKeyDown(KeyCode.Space))
        {
            counter++;

        }

        switch(counter)
        {
            case 0:
                text = "Løft hænderne højt op";
                GetComponent<Text>().text = text;
                break;
            case 1:
                text = "Fang prikken";
                GetComponent<Text>().text = text;
                Manager.instance.displayZoneDots = true;
                if(Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    Manager.instance.counterForDots++;
                }
                break;
            case 2:
                text = "Bevæg dig frem og tilbage";
                GetComponent<Text>().text = text;
                Manager.instance.changeInstCol = true;
                Manager.instance.displayZoneDots = false;
                break;
            case 3:
                text = "Lav en High-five";
                GetComponent<Text>().text = text;
                Manager.instance.tutorialReadyToH5 = true;
                break;
            case 4:
                text = "Tillykke! Du er nu klar til at spille.";
                GetComponent<Text>().text = text;
                break;
        }
	}
}
