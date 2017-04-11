using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialPlayers : MonoBehaviour
{

    public static TutorialPlayers instance;

    public List<ulong> playersPlayedTut;
    public GameObject tutObj;

    void Start()
    {

        if (instance == null)
        {
            instance = this;
            playersPlayedTut = new List<ulong>();
            playersPlayedTut.Clear();
            DontDestroyOnLoad(tutObj);           
        }
        else
        {
            DestroyImmediate(tutObj);
        }

    }
}
