using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kinect = Windows.Kinect;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
    public static Manager instance;

    Kinect.KinectSensor sensor;
    Kinect.MultiSourceFrameReader reader;
    IList<Kinect.Body> bodies;
    float[,] jointsPos;
    float[,] jointPosWorld; //Store the world position of every joint in a body. 3 columns for X, Y, Z
    List<float[,]> bodyJoints;
    List<float[,]> bodyJointsWorld; //Store the world position of every joint per player
    public List<float[]> playersJointsHeight; // Heights of the joints per player
    List<float[,]> playersMinMaxHeight; // Min-Max height of joints per player
    public List<ulong> playersId;       //Remove the public. Did only for observation.
    List<float[,]> playersMinMaxHeightInPixels;
    bool[] initializeMidPos, zoneChanged;
    public ulong[] IDs;
    float[] jointsHeight;       //With the order of the prefJoints. (heights of the joints in general) (the y variable nto the actual y in pixels)
    float[,] minMaxHeight;
    float[,] minMaxHeightInPixels;
    List<float> spineMidHeightInPixels;   //The spineMid position of each player in pixels
    List<float> spineMidPos, spineMidPosThreshold;
    List<Texture> playersMidHighLowMat;     //Counts how many bodies are active at the same frame.
    bool foundId, moveHands, ableToHigh5Left, ableToHigh5Right;
    [HideInInspector]
    public bool didHigh5, playTones, playTonesBody, playTonesBodyHands, clipEndedP1, clipEndedP2, tutorialRaisedHandsDown, p1MovedFrw, p1MovedBack, p2MovedFrw, p2MovedBack;

    //public Texture texture;
    public List<Texture> playersMat;
    public List<Texture> player1_2LineMat;
    public List<Texture> LowHighMats; //2 cells per player.
    public List<Texture> MidLowHighMats; ////2 cells per player.
    public List<Kinect.JointType> prefJoints;       //Assign in the inspector the pref joints you want to detect. ALWAYS first the main body.
    public int textureWidth, textureHeight, midTextureWidthP1, midTextureHeightP1, midTextureWidthP2, midTextureHeightP2, lineTextureWidth, lineTextureHeight, high5Distance;
    public float bodyDistanceThreshold, waitAtTheEnd;

    public GameObject player1, player2;
    UserInput p1UInput, p2UInput;

    void Start()
    {
        //print(SceneManager.GetActiveScene().name);

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
        playersId.Clear();
        bodyJoints = new List<float[,]>();
        bodyJointsWorld = new List<float[,]>();
        playersJointsHeight = new List<float[]>();
        playersMinMaxHeight = new List<float[,]>();
        playersMinMaxHeightInPixels = new List<float[,]>();
        spineMidHeightInPixels = new List<float>();
        spineMidPos = new List<float>();
        spineMidPosThreshold = new List<float>();
        playersMidHighLowMat = new List<Texture>();
        //players = new bool[6];
        IDs = new ulong[6];
        initializeMidPos = new bool[6];  //maximum 6 players
        zoneChanged = new bool[6];  //maximum 6 players
        moveHands = false;
        ableToHigh5Left = true;
        ableToHigh5Right = true;
        didHigh5 = false;
        playTones = false;
        playTonesBody = false;
        playTonesBodyHands = false;
        clipEndedP1 = true;
        clipEndedP2 = true;
        p1MovedBack = false;
        p1MovedFrw = false;
        p2MovedBack = false;
        p2MovedFrw = false;
        tutorialRaisedHandsDown = false;

        p1UInput = player1.GetComponent<UserInput>();
        p2UInput = player2.GetComponent<UserInput>();

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
                for (int h = 0; h < IDs.Length; h++)
                {
                    IDs[h] = 0;
                }

                foreach (var body in bodies)
                {
                    var i = 0;
                    jointsPos = new float[25, 2];
                    jointPosWorld = new float[prefJoints.Count, 3];
                    jointsHeight = new float[prefJoints.Count];
                    minMaxHeight = new float[prefJoints.Count, 2];
                    minMaxHeightInPixels = new float[prefJoints.Count, 2];
                    for (int j = 0; j < prefJoints.Count; j++)
                    {
                        minMaxHeight[j, 0] = 1;
                        minMaxHeight[j, 1] = -1;
                        minMaxHeightInPixels[j, 0] = 1080;
                        minMaxHeightInPixels[j, 1] = 1080;
                    }

                    if (body != null)
                    {
                        if (body.IsTracked)
                        {
                            //players[p] = true;
                            IDs[p] = body.TrackingId;

                            if (playersId.Count == 0)
                            {
                                playersId.Add(body.TrackingId);
                                spineMidHeightInPixels.Add(0);
                                spineMidPos.Add(0);
                                spineMidPosThreshold.Add(0);
                                bodyJoints.Add(null);
                                bodyJointsWorld.Add(null);
                                playersJointsHeight.Add(null);
                                playersMidHighLowMat.Add(null);
                                playersMinMaxHeight.Add(minMaxHeight);
                                playersMinMaxHeightInPixels.Add(minMaxHeightInPixels);
                            }
                            else
                            {
                                foundId = false;

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
                                    spineMidHeightInPixels.Add(0);
                                    spineMidPos.Add(0);
                                    spineMidPosThreshold.Add(0);
                                    bodyJoints.Add(null);
                                    bodyJointsWorld.Add(null);
                                    playersJointsHeight.Add(null);
                                    playersMidHighLowMat.Add(null);
                                    playersMinMaxHeight.Add(minMaxHeight);
                                    playersMinMaxHeightInPixels.Add(minMaxHeightInPixels);
                                }
                            }

                            for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
                            {

                                //3d coordinates in meters
                                Kinect.CameraSpacePoint skeletonPoint = body.Joints[jt].Position;

                                Kinect.ColorSpacePoint colorPoint = sensor.CoordinateMapper.MapCameraPointToColorSpace(skeletonPoint);

                                var k = 0;  //k counts the which joint in the jointHeight matrix corresponding to the amount of the prefJoints
                                foreach (Kinect.JointType joint in prefJoints)
                                {
                                    if (jt == joint)
                                    {
                                        jointsHeight[k] = body.Joints[jt].Position.Y;
                                        jointPosWorld[k, 0] = skeletonPoint.X;
                                        jointPosWorld[k, 1] = skeletonPoint.Y;
                                        jointPosWorld[k, 2] = skeletonPoint.Z;
                                        for (int j = 0; j < playersId.Count; j++)       //j counts whos is the player
                                        {
                                            if (body.TrackingId == playersId[j])
                                            {
                                                if (k == 0)
                                                {
                                                    spineMidHeightInPixels[j] = colorPoint.Y;
                                                    if (initializeMidPos[j] == false)
                                                    {
                                                        spineMidPos[j] = Mathf.Floor(body.Joints[jt].Position.Z); //spineMidPos is used for the zone control with the integer
                                                        spineMidPosThreshold[j] = body.Joints[jt].Position.Z;
                                                        initializeMidPos[j] = true;
                                                    }
                                                    else
                                                    {
                                                        //if (Mathf.Floor(body.Joints[jt].Position.Z) != spineMidPos[j])                ////Previous way of recalibrating where the users had to cross larger distance
                                                        //{
                                                        //    for (int l = 1; l < prefJoints.Count; l++)
                                                        //    {
                                                        //        playersMinMaxHeight[j][l, 0] = 1;
                                                        //        playersMinMaxHeight[j][l, 1] = -1;
                                                        //        playersMinMaxHeightInPixels[j][l, 0] = 1080;
                                                        //        playersMinMaxHeightInPixels[j][l, 1] = 1080;
                                                        //    }


                                                        //    spineMidPos[j] = Mathf.Floor(body.Joints[jt].Position.Z);
                                                        //}

                                                        if (body.Joints[jt].Position.Z > spineMidPosThreshold[j] + bodyDistanceThreshold || body.Joints[jt].Position.Z < spineMidPosThreshold[j] - bodyDistanceThreshold)
                                                        {
                                                            //Recalibration time
                                                            for (int l = 1; l < prefJoints.Count; l++)
                                                            {
                                                                playersMinMaxHeight[j][l, 0] = 1;
                                                                playersMinMaxHeight[j][l, 1] = -1;
                                                                playersMinMaxHeightInPixels[j][l, 0] = 1080;
                                                                playersMinMaxHeightInPixels[j][l, 1] = 1080;
                                                            }

                                                            zoneChanged[j] = true;
                                                            if (body.Joints[jt].Position.Z > spineMidPosThreshold[j] + bodyDistanceThreshold)
                                                            {
                                                                //further away from kinect (Low)
                                                                if (j == 0)
                                                                {
                                                                    p1UInput.userInput = p1UInput.targetBackInput;
                                                                    p1MovedBack = true;
                                                                    playersMidHighLowMat[j] = MidLowHighMats[0];
                                                                    clipEndedP1 = false;
                                                                }
                                                                else if (j == 1)
                                                                {
                                                                    p2UInput.userInput = p2UInput.targetBackInput;
                                                                    p2MovedBack = true;
                                                                    playersMidHighLowMat[j] = MidLowHighMats[2];
                                                                    clipEndedP2 = false;
                                                                }
                                                            }
                                                            else if (body.Joints[jt].Position.Z < spineMidPosThreshold[j] - bodyDistanceThreshold)
                                                            {
                                                                //closer to kinect (High)
                                                                if (j == 0)
                                                                {
                                                                    p1UInput.userInput = p1UInput.targetForwardInput;
                                                                    p1MovedFrw = true;
                                                                    playersMidHighLowMat[j] = MidLowHighMats[1];
                                                                    clipEndedP1 = false;
                                                                }
                                                                else if (j == 1)
                                                                {
                                                                    p2UInput.userInput = p2UInput.targetForwardInput;
                                                                    p2MovedFrw = true;
                                                                    playersMidHighLowMat[j] = MidLowHighMats[3];
                                                                    clipEndedP2 = false;
                                                                }
                                                            }

                                                            if (clipEndedP1 == false && clipEndedP2 == false)
                                                            {
                                                                playTonesBody = true;
                                                            }

                                                            spineMidPosThreshold[j] = body.Joints[jt].Position.Z;
                                                        }
                                                    }
                                                }
                                                SortMinMax(body.Joints[jt].Position.Y, j, k);
                                                SortLine(colorPoint.Y, j, k);
                                            }

                                        }
                                    }
                                    k++;
                                }


                                jointsPos[i, 0] = colorPoint.X;
                                jointsPos[i, 1] = colorPoint.Y;
                                i++;

                                if (jt == Kinect.JointType.ThumbRight)
                                {
                                    for (int j = 0; j < playersId.Count; j++)
                                    {
                                        if (body.TrackingId == playersId[j])
                                        {
                                            bodyJoints[j] = jointsPos;
                                            bodyJointsWorld[j] = jointPosWorld;
                                            playersJointsHeight[j] = jointsHeight;
                                        }
                                    }
                                }
                            }
                        }
                        //else
                        //{
                        //    //players[p] = false;
                        //    IDs[p] = 0;
                        //}
                    }
                    p++;
                }
                CheckHighFive();
                CheckActivePlayers();
                AddRowToData();
                TimeBeingActive();
                TimeSpentAlone();
                PlayAlone();
                if (playersId.Count < 2)
                {
                    playTones = false;
                    playTonesBody = false;
                    playTonesBodyHands = false;
                }
            }
        }
    }


    void AddRowToData()
    {
        for (int i = 0; i < playersId.Count; i++)
        {
            if (i < 2)
            {
                bool foundID = false;
                for (int j = 0; j < LogData.instance.pID.Count; j++)
                {
                    if (LogData.instance.pID[j] == playersId[i])
                    {
                        foundID = true;
                    }
                }

                if (foundID == false)
                {
                    LogData.instance.pID.Add(playersId[i]);
                    LogData.instance.timeBeingActive.Add(0);
                    LogData.instance.timeSpentAlone.Add(0);
                    LogData.instance.playAlone.Add(0);
                    LogData.instance.timeNotPlayingButPartnerPlays.Add(0);
                    LogData.instance.timePlayingTogether.Add(0);
                    LogData.instance.timePlayingTogetherHands.Add(0);
                    LogData.instance.timePlayingTogetherBodies.Add(0);
                    LogData.instance.timeBothNotPlaying.Add(0);
                    LogData.instance.timeLowHand.Add(0);
                    LogData.instance.timeHighHand.Add(0);
                    LogData.instance.timeBackBody.Add(0);
                    LogData.instance.timeForwBody.Add(0);
                    LogData.instance.timeNullInput.Add(0);
                    LogData.instance.didH5.Add(0);
                    if (i == 0)
                    {
                        for (int k = 0; k < LogData.instance.color.Count; k++)
                        {
                            if (LogData.instance.color[k] == "Blue")
                            {
                                LogData.instance.active[k] = false;
                            }
                        }
                        LogData.instance.color.Add("Blue");
                        LogData.instance.active.Add(true);
                    }
                    else
                    {
                        for (int k = 0; k < LogData.instance.color.Count; k++)
                        {
                            if (LogData.instance.color[k] == "Red")
                            {
                                LogData.instance.active[k] = false;
                            }
                        }
                        LogData.instance.color.Add("Red");
                        LogData.instance.active.Add(true);
                    }
                }
            }
        }
    }


    void TimeBeingActive()
    {
        for (int i = 0; i < LogData.instance.pID.Count; i++)
        {
            if (LogData.instance.active[i] == true)
            {
                LogData.instance.timeBeingActive[i] += Time.deltaTime;
            }
        }
    }

    void TimeSpentAlone()
    {
        if (playersId.Count == 1)
        {
            for (int i = 0; i < LogData.instance.active.Count; i++)
            {
                if (LogData.instance.active[i] == true)
                {
                    LogData.instance.timeSpentAlone[i] += Time.deltaTime;
                }
            }
        }
    }


    void PlayAlone()
    {
        if (playersId.Count > 1)
        {
            if ((p1UInput.userInput == "high" || p1UInput.userInput == "low" || p1UInput.userInput == p1UInput.targetBackInput || p1UInput.userInput == p1UInput.targetForwardInput) && p2UInput.userInput == null)  //This statement doesn't work properly cause of p1UInput.targetBackInput and p1UInput.targetForwardInput
            {
                for (int i = 0; i < LogData.instance.color.Count; i++)
                {
                    if (LogData.instance.color[i] == "Blue")
                    {
                        if (LogData.instance.active[i] == true)
                        {
                            LogData.instance.playAlone[i] += Time.deltaTime;
                        }
                    }
                    else if(LogData.instance.color[i] == "Red")
                    {
                        if(LogData.instance.active[i] == true)
                        {
                            LogData.instance.timeNotPlayingButPartnerPlays[i] += Time.deltaTime;
                        }
                    }
                }
            }
            else if ((p2UInput.userInput == "high" || p2UInput.userInput == "low" || p2UInput.userInput == p2UInput.targetBackInput || p2UInput.userInput == p2UInput.targetForwardInput) && p1UInput.userInput == null)  //This statement doesn't work properly cause of p2UInput.targetBackInput and p2UInput.targetForwardInput
            {
                for (int i = 0; i < LogData.instance.color.Count; i++)
                {
                    if (LogData.instance.color[i] == "Red")
                    {
                        if (LogData.instance.active[i] == true)
                        {
                            LogData.instance.playAlone[i] += Time.deltaTime;
                        }
                    }
                    else if (LogData.instance.color[i] == "Blue")
                    {
                        if (LogData.instance.active[i] == true)
                        {
                            LogData.instance.timeNotPlayingButPartnerPlays[i] += Time.deltaTime;
                        }
                    }
                }
            }
        }
    }

    void CheckActivePlayers()
    {
        for (int i = 0; i < playersId.Count; i++)
        {
            bool playerFound = false;
            for (int j = 0; j < IDs.Length; j++)
            {
                if (playersId[i] == IDs[j])
                {
                    playerFound = true;
                }
            }

            if (playerFound == false)
            {
                if (bodyJoints[i] != null)
                {
                    bodyJoints.RemoveAt(i);
                }
                if (bodyJointsWorld[i] != null)
                {
                    bodyJointsWorld.RemoveAt(i);
                }
                if (playersJointsHeight[i] != null)
                {
                    playersJointsHeight.RemoveAt(i);
                }
                if (playersMinMaxHeight[i] != null)
                {
                    playersMinMaxHeight.RemoveAt(i);
                }
                if (playersMinMaxHeightInPixels[i] != null)
                {
                    playersMinMaxHeightInPixels.RemoveAt(i);
                }
                if (spineMidHeightInPixels[i] != null)
                {
                    spineMidHeightInPixels.RemoveAt(i);
                }
                if (spineMidPos[i] != null)
                {
                    spineMidPos.RemoveAt(i);
                }
                if (spineMidPosThreshold[i] != null)
                {
                    spineMidPosThreshold.RemoveAt(i);
                }
                if (playersMidHighLowMat[i] != null)
                {
                    playersMidHighLowMat.RemoveAt(i);
                }
                if (initializeMidPos[i] != null)
                {
                    initializeMidPos[i] = false;
                }
                if (zoneChanged[i] != null)
                {
                    zoneChanged[i] = false;
                }
                if (playersId[i] != null)
                {
                    for (int k = 0; k < LogData.instance.pID.Count; k++)
                    {
                        if (LogData.instance.pID[k] == playersId[i])
                        {
                            if (i == 0)
                            {
                                playTones = false;
                                playTonesBody = false;
                                playTonesBodyHands = false;
                                for (int p = 0; p < LogData.instance.color.Count; p++)
                                {
                                    if (LogData.instance.color[p] == "Red")
                                    {
                                        if (LogData.instance.active[p] == true)
                                        {
                                            //Means p2 changes to p1. Add a new row where p2 becomes blue and the previous red becomes inactive
                                            LogData.instance.pID.Add(LogData.instance.pID[p]);
                                            LogData.instance.color.Add("Blue");
                                            LogData.instance.active.Add(true);
                                            LogData.instance.timeBeingActive.Add(0);
                                            LogData.instance.timeSpentAlone.Add(0);
                                            LogData.instance.playAlone.Add(0);
                                            LogData.instance.timeNotPlayingButPartnerPlays.Add(0);
                                            LogData.instance.timePlayingTogether.Add(0);
                                            LogData.instance.timePlayingTogetherHands.Add(0);
                                            LogData.instance.timePlayingTogetherBodies.Add(0);
                                            LogData.instance.timeBothNotPlaying.Add(0);
                                            LogData.instance.timeLowHand.Add(0);
                                            LogData.instance.timeHighHand.Add(0);
                                            LogData.instance.timeBackBody.Add(0);
                                            LogData.instance.timeForwBody.Add(0);
                                            LogData.instance.timeNullInput.Add(0);
                                            LogData.instance.didH5.Add(0);
                                            LogData.instance.active[p] = false;
                                            //LogData.instance.color[p] = "Blue";
                                        }
                                    }
                                }
                            }
                            else if (i == 1)
                            {
                                playTones = false;
                                playTonesBody = false;
                                playTonesBodyHands = false;
                            }
                            LogData.instance.active[k] = false;
                        }
                    }
                    playersId.RemoveAt(i);
                }
            }
        }
    }


    void CheckHowPlayMusic() //This function collects data for how much time they spent on playing high/low/back/forward
    {
        //Checks for player1
        if(p1UInput.userInput == "low")
        {
            for (int i = 0; i < LogData.instance.color.Count; i++)
            {
                if (LogData.instance.color[i] == "Blue")
                {
                    if (LogData.instance.active[i] == true)
                    {
                        LogData.instance.timeLowHand[i] += Time.deltaTime;
                    }
                }
            }
        }
        if(p1UInput.userInput == "high")
        {
            for (int i = 0; i < LogData.instance.color.Count; i++)
            {
                if (LogData.instance.color[i] == "Blue")
                {
                    if (LogData.instance.active[i] == true)
                    {
                        LogData.instance.timeHighHand[i] += Time.deltaTime;
                    }
                }
            }
        }
        if(p1MovedBack == true)         ///////Doesn't work properly it triggers High also because p1MovedBack depends on clipEndedp1 which seems that always push through high
        {
            for (int i = 0; i < LogData.instance.color.Count; i++)
            {
                if (LogData.instance.color[i] == "Blue")
                {
                    if (LogData.instance.active[i] == true)
                    {
                        LogData.instance.timeBackBody[i] += Time.deltaTime;
                    }
                }
            }
        }
        if(p1MovedFrw == true)         ///////Doesn't work properly it triggers High also because p1MovedFrw depends on clipEndedp1 which seems that always push through high
        {
            for (int i = 0; i < LogData.instance.color.Count; i++)
            {
                if (LogData.instance.color[i] == "Blue")
                {
                    if (LogData.instance.active[i] == true)
                    {
                        LogData.instance.timeForwBody[i] += Time.deltaTime;
                    }
                }
            }
        }
        if(p1UInput.userInput == null)
        {
            for (int i = 0; i < LogData.instance.color.Count; i++)
            {
                if (LogData.instance.color[i] == "Blue")
                {
                    if (LogData.instance.active[i] == true)
                    {
                        LogData.instance.timeNullInput[i] += Time.deltaTime;
                    }
                }
            }
        }

        //Checks for player2
        if (p2UInput.userInput == "low")
        {
            for (int i = 0; i < LogData.instance.color.Count; i++)
            {
                if (LogData.instance.color[i] == "Red")
                {
                    if (LogData.instance.active[i] == true)
                    {
                        LogData.instance.timeLowHand[i] += Time.deltaTime;
                    }
                }
            }
        }
        if (p2UInput.userInput == "high")
        {
            for (int i = 0; i < LogData.instance.color.Count; i++)
            {
                if (LogData.instance.color[i] == "Red")
                {
                    if (LogData.instance.active[i] == true)
                    {
                        LogData.instance.timeHighHand[i] += Time.deltaTime;
                    }
                }
            }
        }
        if (p2MovedBack == true)        ///////Doesn't work properly it triggers High also because p2MovedBack depends on clipEndedp2 which seems that always push through high
        {
            for (int i = 0; i < LogData.instance.color.Count; i++)
            {
                if (LogData.instance.color[i] == "Red")
                {
                    if (LogData.instance.active[i] == true)
                    {
                        LogData.instance.timeBackBody[i] += Time.deltaTime;
                    }
                }
            }
        }
        if (p2MovedFrw == true)         ///////Doesn't work properly it triggers High also because p2MovedFrw depends on clipEndedp2 which seems that always push through high
        {
            for (int i = 0; i < LogData.instance.color.Count; i++)
            {
                if (LogData.instance.color[i] == "Red")
                {
                    if (LogData.instance.active[i] == true)
                    {
                        LogData.instance.timeForwBody[i] += Time.deltaTime;
                    }
                }
            }
        }
        if (p2UInput.userInput == null)
        {
            for (int i = 0; i < LogData.instance.color.Count; i++)
            {
                if (LogData.instance.color[i] == "Red")
                {
                    if (LogData.instance.active[i] == true)
                    {
                        LogData.instance.timeNullInput[i] += Time.deltaTime;
                    }
                }
            }
        }

    }

    void Update()
    {
        CheckHandnBodyForNotes();

        //if(p1MovedBack == true)
        //{
        //    print("Player1 moved back");
        //}

        //if(p1MovedFrw == true)
        //{
        //    print("Player1 moved forward");
        //}

        if (playersId.Count > 1)
        {
            if (playTones == true)
            {
                for (int i = 0; i < LogData.instance.pID.Count; i++)
                {
                    if (LogData.instance.active[i] == true)
                    {
                        LogData.instance.timePlayingTogetherHands[i] += Time.deltaTime;
                    }
                }
            }

            if (playTonesBody == true)
            {
                for (int i = 0; i < LogData.instance.pID.Count; i++)
                {
                    if (LogData.instance.active[i] == true)
                    {
                        LogData.instance.timePlayingTogetherBodies[i] += Time.deltaTime;
                    }
                }
            }


            if (p1UInput.userInput == null && p2UInput.userInput == null)
            {
                for (int i = 0; i < LogData.instance.pID.Count; i++)
                {
                    if (LogData.instance.active[i] == true)
                    {
                        LogData.instance.timeBothNotPlaying[i] += Time.deltaTime;
                    }
                }
            }
        }

        CheckHowPlayMusic();

    }

    void OnGUI()
    {
        if (Event.current.type.Equals(EventType.Repaint))
        {
            if (playersId.Count != 0)
            {
                for (int k = 0; k < playersId.Count; k++)       // K counts players
                {
                    var i = -1;     //i Counts the joints. All of them. 25 in sum

                    for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
                    {
                        i++;
                        var p = 0;      //p counts the pref joints we chose.
                        foreach (Kinect.JointType joint in prefJoints)
                        {
                            if (jt == joint)
                            {
                                try
                                {
                                    if (bodyJoints[k][i, 0] != 0 && bodyJoints[k][i, 1] != 0)
                                    {
                                        if (jt == prefJoints[0]) //the base bodyjoint like spinemid needs to be at the first place ALWAYS!!!!!!
                                        {
                                            if (zoneChanged[k] == false && k < 2)
                                            {
                                                if (k == 0)
                                                {
                                                    Graphics.DrawTexture(new Rect(bodyJoints[k][i, 0] - midTextureWidthP1 / 2, bodyJoints[k][i, 1] - midTextureHeightP1 / 2, midTextureWidthP1, midTextureHeightP1), playersMat[k]);
                                                }
                                                else
                                                {
                                                    Graphics.DrawTexture(new Rect(bodyJoints[k][i, 0] - midTextureWidthP2 / 2, bodyJoints[k][i, 1] - midTextureHeightP2 / 2, midTextureWidthP2, midTextureHeightP2), playersMat[k]);
                                                }
                                            }
                                            else if (zoneChanged[k] == true && k < 2)
                                            {
                                                if (clipEndedP1 == true)
                                                {
                                                    if(p1MovedBack == true)
                                                    {
                                                        p1MovedBack = false;
                                                    }
                                                    else if(p1MovedFrw == true)
                                                    {
                                                        p1MovedFrw = false;
                                                    }

                                                    if (k == 0)
                                                    {
                                                        Graphics.DrawTexture(new Rect(bodyJoints[k][i, 0] - midTextureWidthP1 / 2, bodyJoints[k][i, 1] - midTextureHeightP1 / 2, midTextureWidthP1, midTextureHeightP1), playersMat[k]);
                                                    }
                                                }
                                                else
                                                {
                                                    if (k == 0)
                                                    {
                                                        if (playersMidHighLowMat[k] != null)
                                                        {
                                                            Graphics.DrawTexture(new Rect(bodyJoints[k][i, 0] - midTextureWidthP1 / 2, bodyJoints[k][i, 1] - midTextureHeightP1 / 2, midTextureWidthP1, midTextureHeightP1), playersMidHighLowMat[k]);
                                                        }
                                                    }
                                                }

                                                if (clipEndedP2 == true)
                                                {
                                                    if (p2MovedBack == true)
                                                    {
                                                        p2MovedBack = false;
                                                    }
                                                    else if (p2MovedFrw == true)
                                                    {
                                                        p2MovedFrw = false;
                                                    }

                                                    if (k == 1)
                                                    {
                                                        Graphics.DrawTexture(new Rect(bodyJoints[k][i, 0] - midTextureWidthP2 / 2, bodyJoints[k][i, 1] - midTextureHeightP2 / 2, midTextureWidthP2, midTextureHeightP2), playersMat[k]);
                                                    }
                                                }
                                                else
                                                {
                                                    if (k == 1)
                                                    {
                                                        if (playersMidHighLowMat[k] != null)
                                                        {
                                                            Graphics.DrawTexture(new Rect(bodyJoints[k][i, 0] - midTextureWidthP2 / 2, bodyJoints[k][i, 1] - midTextureHeightP2 / 2, midTextureWidthP2, midTextureHeightP2), playersMidHighLowMat[k]);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {

                                            if (playersJointsHeight[k][p] > playersJointsHeight[k][0] && k < 2)          //K will be restricted to 2 for only two players.
                                            {
                                                moveHands = true;

                                                var range = playersMinMaxHeightInPixels[k][p, 0] - playersMinMaxHeightInPixels[k][p, 1];
                                                var distribution = range / 3;

                                                Graphics.DrawTexture(new Rect(bodyJoints[k][i, 0] - lineTextureWidth / 2, (playersMinMaxHeightInPixels[k][p, 0] - (2 * distribution)) - lineTextureHeight / 2, lineTextureWidth, lineTextureHeight), player1_2LineMat[k]);

                                                var range2 = playersMinMaxHeight[k][p, 1] - playersMinMaxHeight[k][p, 0];
                                                var distribution2 = range2 / 3;

                                                if (playersJointsHeight[k][p] > (playersMinMaxHeight[k][p, 0] + (2 * distribution2)))
                                                {
                                                    //print("High");      ////////////////////////////////////  HERE IS THE HIGH INPUT////////////////////////////////////

                                                    if (k == 0)
                                                    {
                                                        p1UInput.userInput = "high";

                                                    }
                                                    else if (k == 1)
                                                    {
                                                        p2UInput.userInput = "high";
                                                    }

                                                    switch (k)
                                                    {
                                                        case 0:
                                                            Graphics.DrawTexture(new Rect(bodyJoints[k][i, 0] - textureWidth / 2, bodyJoints[k][i, 1] - textureHeight / 2, textureWidth, textureHeight), LowHighMats[1]);
                                                            break;
                                                        case 1:
                                                            Graphics.DrawTexture(new Rect(bodyJoints[k][i, 0] - textureWidth / 2, bodyJoints[k][i, 1] - textureHeight / 2, textureWidth, textureHeight), LowHighMats[3]);
                                                            break;
                                                    }
                                                }
                                                else
                                                {
                                                    //print("Low"); ////////////////////////////////////  HERE IS THE LOW INPUT////////////////////////////////////

                                                    if (k == 0)
                                                    {
                                                        p1UInput.userInput = "low";
                                                    }
                                                    else if (k == 1)
                                                    {
                                                        p2UInput.userInput = "low";
                                                    }

                                                    switch (k)
                                                    {
                                                        case 0:
                                                            Graphics.DrawTexture(new Rect(bodyJoints[k][i, 0] - textureWidth / 2, bodyJoints[k][i, 1] - textureHeight / 2, textureWidth, textureHeight), LowHighMats[0]);
                                                            break;
                                                        case 1:
                                                            Graphics.DrawTexture(new Rect(bodyJoints[k][i, 0] - textureWidth / 2, bodyJoints[k][i, 1] - textureHeight / 2, textureWidth, textureHeight), LowHighMats[2]);
                                                            break;
                                                    }
                                                }
                                            }
                                            else if (k < 2 && playersJointsHeight[k][1] <= playersJointsHeight[k][0] && playersJointsHeight[k][2] <= playersJointsHeight[k][0] && moveHands == true)
                                            {
                                                if (k == 0)
                                                {
                                                    p1UInput.userInput = null;
                                                }
                                                else if (k == 1)
                                                {
                                                    p2UInput.userInput = null;
                                                }

                                                moveHands = false;
                                            }
                                        }
                                    }
                                }
                                catch
                                {
                                    //print("Something wrong with the bodyJoints of someone player that is being registered into Kinect");
                                }
                            }
                            p++;
                        }
                    }

                }
            }
        }
    }


    void CheckHandnBodyForNotes()
    {
        /////////////////////FIXED CASE ONLY FOR TWO PLAYERS AND ONLY FOR TWO HANDS
        if (playersId.Count > 1)
        {
            try
            {
                if (playersJointsHeight[0][1] > playersJointsHeight[0][0] && (playersJointsHeight[1][1] > playersJointsHeight[1][0] || playersJointsHeight[1][2] > playersJointsHeight[1][0])
                    || playersJointsHeight[0][2] > playersJointsHeight[0][0] && (playersJointsHeight[1][1] > playersJointsHeight[1][0] || playersJointsHeight[1][2] > playersJointsHeight[1][0]))
                {
                    playTones = true;
                }
                else
                {
                    playTones = false;
                }

                if (((playersJointsHeight[0][1] > playersJointsHeight[0][0] || playersJointsHeight[0][2] > playersJointsHeight[0][0]) && clipEndedP2 == false) || ((playersJointsHeight[1][1] > playersJointsHeight[1][0] || playersJointsHeight[1][2] > playersJointsHeight[1][0]) && clipEndedP1 == false))

                {
                    playTonesBodyHands = true;
                }
                else
                {
                    playTonesBodyHands = false;
                }


            }
            catch
            {
                //print("Something went wrong with detecting the position of either the left hand or right hand or both from the player1 or player2");
            }
        }
        //////////////////////////////////////////////////////////////////////////
    }


    void SortMinMax(float height, int player, int joint)
    {
        playersMinMaxHeight[player][joint, 0] = jointsHeight[0];

        if (height > playersMinMaxHeight[player][joint, 1])
        {
            playersMinMaxHeight[player][joint, 1] = height;
        }
    }


    void SortLine(float height, int player, int joint)
    {
        playersMinMaxHeightInPixels[player][joint, 0] = spineMidHeightInPixels[player];

        if (height < playersMinMaxHeightInPixels[player][joint, 1])
        {
            playersMinMaxHeightInPixels[player][joint, 1] = height;
        }
    }


    void CheckHighFive()
    {
        if (playersId.Count > 1)
        {
            for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
            {
                var j = 0;  //j counts the joints in the prefJoints
                foreach (Kinect.JointType joint in prefJoints)
                {
                    if (jt == joint)
                    {
                        if (j != 0)
                        {
                            if (j == 1) //means handLeft
                            {
                                if (Mathf.Floor(bodyJointsWorld[0][j, 2]) == Mathf.Floor(bodyJointsWorld[1][j + 1, 2]) || Mathf.Floor(bodyJointsWorld[0][j, 2]) == Mathf.Floor(bodyJointsWorld[1][j, 2])) //Restriction only for two players
                                {
                                    //print("HandLeft of player1 and HandRight of player2 in the same zone");
                                    var differenceX_LR = Mathf.Abs(bodyJointsWorld[0][j, 0] - bodyJointsWorld[1][j + 1, 0]);
                                    var differenceY_LR = Mathf.Abs(bodyJointsWorld[0][j, 1] - bodyJointsWorld[1][j + 1, 1]);
                                    var differenceX_LL = Mathf.Abs(bodyJointsWorld[0][j, 0] - bodyJointsWorld[1][j, 0]);
                                    var differenceY_LL = Mathf.Abs(bodyJointsWorld[0][j, 1] - bodyJointsWorld[1][j, 1]);
                                    if ((Mathf.Round(differenceX_LR * high5Distance) == 0 && Mathf.Round(differenceY_LR * high5Distance) == 0) || (Mathf.Round(differenceX_LL * high5Distance) == 0 && Mathf.Round(differenceY_LL * high5Distance) == 0)) //Seems that the hands need to be very close. Change 5 to 7.5 like (10 seems so far away)
                                    {
                                        if (ableToHigh5Left == true)
                                        {
                                            //print("p1Left highFive with p2AnyHand");
                                            for (int i = 0; i < LogData.instance.pID.Count; i++)
                                            {
                                                if (LogData.instance.active[i] == true)
                                                {
                                                    LogData.instance.didH5[i]++;
                                                }
                                            }
                                            didHigh5 = true;    //Needs to be set on false when the fireworks were played;
                                            ableToHigh5Left = false;
                                        }
                                    }
                                    else if ((Mathf.Round(differenceX_LR * high5Distance) != 0 && Mathf.Round(differenceY_LR * high5Distance) != 0) && (Mathf.Round(differenceX_LL * high5Distance) != 0 && Mathf.Round(differenceY_LL * high5Distance) != 0))
                                    {
                                        ableToHigh5Left = true;
                                    }
                                }
                            }
                            else if (j == 2) //means handRight
                            {
                                if (Mathf.Floor(bodyJointsWorld[0][j, 2]) == Mathf.Floor(bodyJointsWorld[1][j - 1, 2]) || Mathf.Floor(bodyJointsWorld[0][j, 2]) == Mathf.Floor(bodyJointsWorld[1][j, 2])) //Restriction only for two players
                                {
                                    //print("HandRight of player1 and HandLeft of player2 in the same zone");
                                    var differenceX_RL = Mathf.Abs(bodyJointsWorld[0][j, 0] - bodyJointsWorld[1][j - 1, 0]);
                                    var differenceY_RL = Mathf.Abs(bodyJointsWorld[0][j, 1] - bodyJointsWorld[1][j - 1, 1]);
                                    var differenceX_RR = Mathf.Abs(bodyJointsWorld[0][j, 0] - bodyJointsWorld[1][j, 0]);
                                    var differenceY_RR = Mathf.Abs(bodyJointsWorld[0][j, 1] - bodyJointsWorld[1][j, 1]);
                                    if ((Mathf.Round(differenceX_RL * high5Distance) == 0 && Mathf.Round(differenceY_RL * high5Distance) == 0) || (Mathf.Round(differenceX_RR * high5Distance) == 0 && Mathf.Round(differenceY_RR * high5Distance) == 0))
                                    {
                                        if (ableToHigh5Right == true)
                                        {
                                            //print("p1Right highFive with p2AnyHand");
                                            for (int i = 0; i < LogData.instance.pID.Count; i++)
                                            {
                                                if (LogData.instance.active[i] == true)
                                                {
                                                    LogData.instance.didH5[i]++;
                                                }
                                            }
                                            didHigh5 = true;    //Needs to be set on false when the fireworks were played;
                                            ableToHigh5Right = false;
                                        }
                                    }
                                    else if ((Mathf.Round(differenceX_RL * high5Distance) != 0 && Mathf.Round(differenceY_RL * high5Distance) != 0) && (Mathf.Round(differenceX_RR * high5Distance) != 0 && Mathf.Round(differenceY_RR * high5Distance) != 0))
                                    {
                                        ableToHigh5Right = true;
                                    }
                                }
                            }

                        }
                    }
                    j++;
                }
            }
        }
    }


}

