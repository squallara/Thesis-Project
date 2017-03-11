using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour {

    public static SceneManager instance;

    public GameObject player1, player2;
    public Material player1mat, player2mat;

    void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
    }
}
