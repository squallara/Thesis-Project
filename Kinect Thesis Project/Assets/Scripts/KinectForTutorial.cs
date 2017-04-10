using System.Collections;
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
    public List<ulong> playersId;       //Remove the public. Did only for observation.
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
        playersId = new List<ulong>();
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

                            if (playersId.Count == 0)
                            {
                                playersId.Add(body.TrackingId);
                            }

                            foreach (ulong id in playersId)
                            {
                                if (body.TrackingId == id)
                                {
                                    foundId = true;
                                }
                            }

                            if (foundId == false)
                            {
                                playersId.Add(body.TrackingId);
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

        if (countBodies != playersId.Count)
        {
            for (int i = 0; i < playersId.Count; i++)
            {
                if (players[i] == false)
                {
                    playersId.RemoveAt(i);
                }
            }
        }
    }


    public int CheckPlayersAfterGame()
    {
        if (playersId.Count == 0) //All the players left the game. Go to welcome screen/tutorial
        {
            return 0; /////////Fixed case where tutorial scene is always scene 0.
        }
        else //They are some still remaining to play
        {
            if (playersId.Count == 1)
            {
                bool foundPlayer = false;
                for (int i = 0; i < TutorialPlayers.instance.playersPlayedTut.Count; i++)
                {
                    if (playersId[0] == TutorialPlayers.instance.playersPlayedTut[i])
                    {
                        foundPlayer = true;
                    }
                }

                if (foundPlayer) //P1 had already passed the tutorial
                {
                    return 1;
                }
                else //New player1
                {
                    return 0;   //Remaining P1 hasn't passed the tutorial. Needs to see it.
                }
            }
            else
            {
                bool[] foundPlayer = new bool[2];
                for (int j = 0; j < 2; j++) ////////Runs only for P1 and P2
                {
                    for (int i = 0; i < TutorialPlayers.instance.playersPlayedTut.Count; i++) //if only one finished the tutorial then they will see it again
                    {
                        if (playersId[j] == TutorialPlayers.instance.playersPlayedTut[i])
                        {
                            foundPlayer[j] = true;
                        }
                    }

                }

                if (foundPlayer[0] == true && foundPlayer[1] == true)    //Fixed case for two players. both they have passed the tutorial.
                {
                    return 1;
                }
                else
                {
                    return 0; //One of the two hasn't seen the tutorial.
                }
            }
        }

    }

}

