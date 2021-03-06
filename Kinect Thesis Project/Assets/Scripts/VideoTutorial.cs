﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VideoTutorial : MonoBehaviour
{

    public List<Material> videoMats = new List<Material>();
    public GameObject canvas;
    int counter, countReps;
    Renderer rend;
    MovieTexture video;
    public int repeatVideo;
    bool videoStarted, readyToPlay, tutorialStarted;
    float countToStart;
    AudioSource aud;

    void Start()
    {
        counter = 0;
        countReps = 0;
        countToStart = 0;
        rend = GetComponent<Renderer>();
        videoStarted = false;
        tutorialStarted = false;
        readyToPlay = true;
        aud = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (KinectForTutorial.instance.playersIdTut.Count > 0)
        {
            if (!tutorialStarted)
            {
                countToStart += Time.deltaTime;
                if (countToStart >= TutorialPlayers.instance.timeToStartTemp)
                {
                    canvas.SetActive(false);
                    tutorialStarted = true;
                }
            }
        }
        else
        {
            countToStart = 0;
            tutorialStarted = false;
            canvas.SetActive(true);
            counter = 0;
            countReps = 0;
            videoStarted = false;
            readyToPlay = true;
        }

        if (counter < videoMats.Count)
        {
            if (!canvas.activeInHierarchy)
            {
                if (readyToPlay)
                {

                    rend.material = videoMats[counter];
                    video = (MovieTexture)rend.material.mainTexture;

                    video.Play();
                    aud.Play();
                    //aud.clip = video.audioClip;
                    videoStarted = true;
                    readyToPlay = false;
                }

                if (videoStarted)
                {
                    if (!video.isPlaying)
                    {
                        video.Stop();
                        aud.Stop();
                        countReps++;
                        if (countReps == repeatVideo)
                        {
                            counter++;
                            countReps = 0;
                        }
                        videoStarted = false;
                        readyToPlay = true;
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < KinectForTutorial.instance.playersIdTut.Count; i++)
            {
                TutorialPlayers.instance.playersPlayedTut.Add(KinectForTutorial.instance.playersIdTut[i]);
            }
            if (SavedData.instance.song == 0)
            {
                SceneManager.LoadScene(1);
            }
            else
            {
                SceneManager.LoadScene(SavedData.instance.song);  //Fixed to have only two gameplay scenes into the game.
            }
        }
    }
}
