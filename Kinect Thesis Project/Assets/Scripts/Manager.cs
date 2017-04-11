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
    bool[] players, initializeMidPos, zoneChanged;
    float[] jointsHeight;       //With the order of the prefJoints. (heights of the joints in general) (the y variable nto the actual y in pixels)
    float[,] minMaxHeight;
    float[,] minMaxHeightInPixels;
    List<float> spineMidHeightInPixels;   //The spineMid position of each player in pixels
    List<float> spineMidPos, spineMidPosThreshold;
    List<Texture> playersMidHighLowMat;
    int countBodies;        //Counts how many bodies are active at the same frame.
    bool foundId, moveHands, ableToHigh5Left, ableToHigh5Right;
    [HideInInspector]
    public bool didHigh5, playTones, clipEndedP1, clipEndedP2, tutorialRaisedHandsDown;
    [HideInInspector]
    public int P1Highes;

    //public Texture texture;
    public List<Texture> playersMat;
    public List<Texture> player1_2LineMat;
    public List<Texture> LowHighMats; //2 cells per player.
    public List<Texture> MidLowHighMats; ////2 cells per player.
    public List<Kinect.JointType> prefJoints;       //Assign in the inspector the pref joints you want to detect. ALWAYS first the main body.
    public int textureWidth, textureHeight, midTextureWidthP1, midTextureHeightP1, midTextureWidthP2, midTextureHeightP2, high5Distance;
    public float bodyDistanceThreshold, waitAtTheEnd;

    public GameObject player1, player2;
    UserInput p1UInput, p2UInput;

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
        players = new bool[6];
        countBodies = 0;
        initializeMidPos = new bool[2];  //RESTRICTED ONLY TO TWO PLAYERS
        zoneChanged = new bool[2];  //RESTRICTED ONLY TO TWO PLAYERS
        moveHands = false;
        ableToHigh5Left = true;
        ableToHigh5Right = true;
        didHigh5 = false;
        playTones = false;
        clipEndedP1 = false;
        clipEndedP2 = false;
        tutorialRaisedHandsDown = false;

        p1UInput = player1.GetComponent<UserInput>();
        p2UInput = player2.GetComponent<UserInput>();

        P1Highes = 0;

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
                    foundId = false;
                    if (body != null)
                    {
                        if (body.IsTracked)
                        {
                            players[p] = true;

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
                                                        if (Mathf.Floor(body.Joints[jt].Position.Z) != spineMidPos[j])
                                                        {
                                                            for (int l = 1; l < prefJoints.Count; l++)
                                                            {
                                                                playersMinMaxHeight[j][l, 0] = 1;
                                                                playersMinMaxHeight[j][l, 1] = -1;
                                                                playersMinMaxHeightInPixels[j][l, 0] = 1080;
                                                                playersMinMaxHeightInPixels[j][l, 1] = 1080;
                                                            }


                                                            spineMidPos[j] = Mathf.Floor(body.Joints[jt].Position.Z);
                                                        }

                                                        if (body.Joints[jt].Position.Z > spineMidPosThreshold[j] + bodyDistanceThreshold || body.Joints[jt].Position.Z < spineMidPosThreshold[j] - bodyDistanceThreshold)
                                                        {
                                                            zoneChanged[j] = true;
                                                            if (body.Joints[jt].Position.Z > spineMidPosThreshold[j] + bodyDistanceThreshold)
                                                            {
                                                                //further away from kinect (Low)
                                                                if (j == 0)
                                                                {
                                                                    p1UInput.userInput = p1UInput.targetBackInput;
                                                                    playersMidHighLowMat[j] = MidLowHighMats[0];
                                                                    clipEndedP1 = false;
                                                                }
                                                                else if (j == 1)
                                                                {
                                                                    p2UInput.userInput = p2UInput.targetBackInput;
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
                                                                    playersMidHighLowMat[j] = MidLowHighMats[1];
                                                                    clipEndedP1 = false;
                                                                }
                                                                else if (j == 1)
                                                                {
                                                                    p2UInput.userInput = p2UInput.targetForwardInput;
                                                                    playersMidHighLowMat[j] = MidLowHighMats[3];
                                                                    clipEndedP2 = false;
                                                                }
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
                        else
                        {
                            players[p] = false;
                        }
                    }
                    p++;
                }
                CheckHighFive();
            }
        }
    }


    void OnGUI()
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
                    bodyJoints.RemoveAt(i);
                    bodyJointsWorld.RemoveAt(i);
                    playersJointsHeight.RemoveAt(i);
                    playersMinMaxHeight.RemoveAt(i);
                    playersMinMaxHeightInPixels.RemoveAt(i);
                    spineMidHeightInPixels.RemoveAt(i);
                    spineMidPos.RemoveAt(i);
                    spineMidPosThreshold.RemoveAt(i);
                    playersMidHighLowMat.RemoveAt(i);
                    initializeMidPos[i] = false;
                    zoneChanged[i] = false;
                    playersId.RemoveAt(i);
                }
            }
        }

        if (playersId.Count < 2)
        {
            playTones = false;
        }

        GameplaySession();
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
                                            print("p1Left highFive with p2AnyHand");
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
                                            print("p1Right highFive with p2AnyHand");
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


    void GameplaySession()
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
                                            /////////////////////FIXED CASE ONLY FOR TWO PLAYERS AND ONLY FOR TWO HANDS
                                            if (playersId.Count > 1)
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
                                            }
                                            //////////////////////////////////////////////////////////////////////////

                                            moveHands = true;

                                            var range = playersMinMaxHeightInPixels[k][p, 0] - playersMinMaxHeightInPixels[k][p, 1];
                                            var distribution = range / 3;

                                            Graphics.DrawTexture(new Rect(bodyJoints[k][i, 0] - 125 / 2, (playersMinMaxHeightInPixels[k][p, 0] - (2 * distribution)) - 15 / 2, 125, 15), player1_2LineMat[k]);

                                            var range2 = playersMinMaxHeight[k][p, 1] - playersMinMaxHeight[k][p, 0];
                                            var distribution2 = range2 / 3;

                                            if (playersJointsHeight[k][p] > (playersMinMaxHeight[k][p, 0] + (2 * distribution2)))
                                            {
                                                //print("High");      ////////////////////////////////////  HERE IS THE HIGH INPUT////////////////////////////////////

                                                if (k == 0)
                                                {
                                                    p1UInput.userInput = "high";
                                                    P1Highes++;

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
                            p++;
                        }
                    }
                }
            }
        }
    }



}

