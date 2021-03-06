﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kinect = Windows.Kinect;
using UnityEngine.SceneManagement;

public class KinectForTutorial : MonoBehaviour
{

    public static KinectForTutorial instance;

    Kinect.KinectSensor sensor;
    Kinect.MultiSourceFrameReader reader;
    IList<Kinect.Body> bodies;
    [HideInInspector]
    public List<ulong> playersIdTut;       //Remove the public. Did only for observation.
    bool[] players;
    int countBodies;        //Counts how many bodies are active at the same frame.
    bool foundId;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }

        sensor = Kinect.KinectSensor.GetDefault();

        if (sensor != null)
        {
            sensor.Open();
        }

        reader = sensor.OpenMultiSourceFrameReader(Kinect.FrameSourceTypes.Body | Kinect.FrameSourceTypes.Color | Kinect.FrameSourceTypes.Depth | Kinect.FrameSourceTypes.Infrared);
        reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
        playersIdTut = new List<ulong>();
        playersIdTut.Clear();
        players = new bool[6];
        countBodies = 0;
    }

    private void Reader_MultiSourceFrameArrived(object sender, Kinect.MultiSourceFrameArrivedEventArgs e)
    {
        var reference = e.FrameReference.AcquireFrame();

        //Body
        using (var frame = reference.BodyFrameReference.AcquireFrame())
        {
            if (frame != null)
            {
                bodies = new Kinect.Body[frame.BodyFrameSource.BodyCount];  //BodyCount is always 6. I guess they have it on 6 because it is the maximum of the players that they can track per frame.
                frame.GetAndRefreshBodyData(bodies);
                var p = 0; //p counts the bodies so it counts the players

                foreach (var body in bodies)
                {
                    foundId = false;
                    if (body != null)
                    {
                        if (body.IsTracked)
                        {
                            players[p] = true;

                            if (playersIdTut.Count == 0)
                            {
                                playersIdTut.Add(body.TrackingId);
                            }

                            foreach (ulong id in playersIdTut)
                            {
                                if (body.TrackingId == id)
                                {
                                    foundId = true;
                                }
                            }

                            if (foundId == false)
                            {
                                playersIdTut.Add(body.TrackingId);
                            }
                        }
                        else
                        {
                            players[p] = false;
                        }
                    }
                    p++;
                }
            }
        }
    }


    void Update()
    {
        countBodies = 0;
        for (int i = 0; i < 6; i++)
        {
            if (players[i] == true)
            {
                countBodies++;
            }
        }

        if (countBodies != playersIdTut.Count)
        {
            for (int i = 0; i < playersIdTut.Count; i++)
            {
                if (players[i] == false)
                {
                    playersIdTut.RemoveAt(i);
                }
            }
        }
    }   

}

