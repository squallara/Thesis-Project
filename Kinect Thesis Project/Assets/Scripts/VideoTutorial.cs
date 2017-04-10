using System.Collections;
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
    public float timeToStart;

    void Start()
    {
        counter = 0;
        countReps = 0;
        countToStart = 0;
        rend = GetComponent<Renderer>();
        videoStarted = false;
        tutorialStarted = false;
        readyToPlay = true;
    }

    void Update()
    {       
        if(KinectForTutorial.instance.playersId.Count > 0)
        {
            if (!tutorialStarted)
            {
                countToStart += Time.deltaTime;
                if (countToStart >= timeToStart)
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
                    videoStarted = true;
                    readyToPlay = false;
                }

                if (videoStarted)
                {
                    if (!video.isPlaying)
                    {
                        video.Stop();
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
            for(int i=0; i< KinectForTutorial.instance.playersId.Count; i++)
            {
                TutorialPlayers.instance.playersPlayedTut.Add(KinectForTutorial.instance.playersId[i]);
            }
            SceneManager.LoadScene(1);  //Fixed to have only two scenes into the game.
        }
    }
}
