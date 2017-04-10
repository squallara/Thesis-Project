﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BreakScript : MonoBehaviour
{

    public float baseWaitingTime, extraWaitingTime;
    float countTime;
    int scene;
    bool reachedTime;


    void Start()
    {
        countTime = 0;
        scene = -1;
        reachedTime = false;
    }


    void Update()
    {
        countTime += Time.deltaTime;

        if (countTime >= baseWaitingTime && !reachedTime)
        {
            scene = KinectForTutorial.instance.CheckPlayersAfterGame();
            reachedTime = true;
            
            if(scene == 1)
            {
                SceneManager.LoadScene(scene);
            }
            else if(scene == 0)
            {
                StartCoroutine(WaitBitMore(scene));
            }
        }
    }

    IEnumerator WaitBitMore(int scene)
    {
        yield return new WaitForSeconds(extraWaitingTime);
        TutorialPlayers.instance.playersPlayedTut.Clear();
        SceneManager.LoadScene(scene);
    }
}