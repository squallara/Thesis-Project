using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // Update is called once per frame
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

        //print(KinectForTutorial.instance.playersId.Count);
        if (counter < videoMats.Count && !canvas.activeInHierarchy)
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
}
