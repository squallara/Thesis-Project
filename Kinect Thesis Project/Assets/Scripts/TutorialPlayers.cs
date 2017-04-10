using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialPlayers : MonoBehaviour
{

    public static TutorialPlayers instance;

    public List<ulong> playersPlayedTut = new List<ulong>();
    public GameObject tutObj;

    void Start()
    {

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(tutObj);
        }
        else
        {
            DestroyImmediate(tutObj);
        }

    }
}
