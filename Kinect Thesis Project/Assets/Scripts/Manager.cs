using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kinect = Windows.Kinect;

public class Manager : MonoBehaviour
{
    public bool useMidOutput;
    Kinect.KinectSensor sensor;
    Kinect.MultiSourceFrameReader reader;
    IList<Kinect.Body> bodies;
    float[,] jointsPos;
    List<float[,]> bodyJoints;
    List<float[]> playersJointsHeight; // Heights of the joints per player
    List<float[,]> playersMinMaxHeight; // Min-Max height of joints per player
    //List<Kinect.ColorSpacePoint> colorPoints;   //The pixels position for every pref bodyJoint;
    List<Kinect.ColorSpacePoint> player1TrailHandLeft, player1TrailHandRight, player2TrailHandLeft, player2TrailHandRight; //The trails of 2 players and their two hands. FIXED THING.
    public List<ulong> playersId;       //Remove the public. Did only for observation.
    List<float[,]> playersMinMaxHeightInPixels;
    bool[] players;
    float[] jointsHeight;       //With the order of the prefJoints. (heights of the joints in general) (the y variable nto the actual y in pixels)
    float[,] minMaxHeight;
    float[,] minMaxHeightInPixels;
    List<float> spineMidHeightInPixels;   //The spineMid position of each player in pixels
    List<float> spineMidPos;
    List<Texture> playersMidHighLowMat;
    int countBodies;        //Counts how many bodies are active at the same frame.
    bool foundId, initializeMidPos, zoneChanged, moveHands;
    float countDown;

    //public Texture texture;
    public List<Texture> playersMat;
    public List<Texture> LowHighMats; //2 cells per player.
    public List<Kinect.JointType> prefJoints;       //Assign in the inspector the pref joints you want to detect. ALWAYS first the main body.
    public int trailAmount;
    public int textureWidth, textureHeight;
    public float timeToStopInputs;

    public GameObject player1, player2;
    UserInput p1UInput, p2UInput;

    void Start()
    {
        sensor = Kinect.KinectSensor.GetDefault();

        if (sensor != null)
        {
            sensor.Open();
        }

        reader = sensor.OpenMultiSourceFrameReader(Kinect.FrameSourceTypes.Body | Kinect.FrameSourceTypes.Color | Kinect.FrameSourceTypes.Depth | Kinect.FrameSourceTypes.Infrared);
        reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
        playersId = new List<ulong>();
        bodyJoints = new List<float[,]>();
        playersJointsHeight = new List<float[]>();
        playersMinMaxHeight = new List<float[,]>();
        playersMinMaxHeightInPixels = new List<float[,]>();
        player1TrailHandLeft = new List<Kinect.ColorSpacePoint>();
        player1TrailHandRight = new List<Kinect.ColorSpacePoint>();
        player2TrailHandLeft = new List<Kinect.ColorSpacePoint>();
        player2TrailHandRight = new List<Kinect.ColorSpacePoint>();
        spineMidHeightInPixels = new List<float>();
        spineMidPos = new List<float>();
        playersMidHighLowMat = new List<Texture>();
        players = new bool[6];
        countBodies = 0;
        initializeMidPos = false;
        zoneChanged = false;
        moveHands = false;
        countDown = 0;

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
                var p = 0;

                foreach (var body in bodies)
                {
                    var i = 0;
                    jointsPos = new float[25, 2];
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
                                bodyJoints.Add(null);
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
                                bodyJoints.Add(null);
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

                                if (jt == Kinect.JointType.SpineMid)
                                {
                                    print(Mathf.Floor(body.Joints[jt].Position.Z));
                                }

                                var k = 0;  //k counts the which joint in the jointHeight matrix corresponding to the amount of the prefJoints
                                foreach (Kinect.JointType joint in prefJoints)
                                {
                                    if (jt == joint)
                                    {
                                        jointsHeight[k] = body.Joints[jt].Position.Y;
                                        for (int j = 0; j < playersId.Count; j++)       //j counts whos is the player
                                        {
                                            if (body.TrackingId == playersId[j])
                                            {
                                                if (k == 0)
                                                {
                                                    spineMidHeightInPixels[j] = colorPoint.Y;
                                                    if (initializeMidPos == false)
                                                    {
                                                        spineMidPos[j] = Mathf.Floor(body.Joints[jt].Position.Z);
                                                        initializeMidPos = true;
                                                    }
                                                    else
                                                    {
                                                        if(Mathf.Floor(body.Joints[jt].Position.Z) != spineMidPos[j])
                                                        {
                                                            print("body changed zone");
                                                            //StopAllCoroutines();
                                                            zoneChanged = true;
                                                            for(int l=1; l<prefJoints.Count; l++)
                                                            {
                                                                playersMinMaxHeight[j][l, 0] = 1;
                                                                playersMinMaxHeight[j][l, 1] = -1;
                                                                playersMinMaxHeightInPixels[j][l, 0] = 1080;
                                                                playersMinMaxHeightInPixels[j][l, 1] = 1080;
                                                            }

                                                            //if (Mathf.Floor(body.Joints[jt].Position.Z) > spineMidPos[j])
                                                            //{
                                                            //    //further away from kinect (Low)
                                                            //    if (j == 0)
                                                            //    {
                                                            //        p1UInput.userInput = "low";
                                                            //        playersMidHighLowMat[j] = LowHighMats[0];
                                                            //    }
                                                            //    else if (j == 1)
                                                            //    {
                                                            //        p2UInput.userInput = "low";
                                                            //        playersMidHighLowMat[j] = LowHighMats[2];
                                                            //    }
                                                            //}
                                                            //else if (Mathf.Floor(body.Joints[jt].Position.Z) < spineMidPos[j])
                                                            //{
                                                            //    //closer to kinect (High)
                                                            //    if (j == 0)
                                                            //    {
                                                            //        p1UInput.userInput = "high";
                                                            //        playersMidHighLowMat[j] = LowHighMats[1];
                                                            //    }
                                                            //    else if (j == 1)
                                                            //    {
                                                            //        p2UInput.userInput = "high";
                                                            //        playersMidHighLowMat[j] = LowHighMats[3];
                                                            //    }
                                                            //}

                                                            spineMidPos[j] = Mathf.Floor(body.Joints[jt].Position.Z);
                                                        }
                                                        else
                                                        {
                                                            //if (countDown >= timeToStopInputs)
                                                            //{
                                                            //    if (j == 0)
                                                            //    {
                                                            //    print("I am here");
                                                            //        p1UInput.userInput = null;
                                                            //    }
                                                            //    else if (j == 1)
                                                            //    {
                                                            //        p2UInput.userInput = null;
                                                            //    }

                                                            //    countDown = 0;
                                                            //}
                                                            //StartCoroutine(StopInput(j));
                                                        }
                                                    }
                                                }
                                                SortMinMax(body.Joints[jt].Position.Y, j, k);
                                                SortLine(colorPoint.Y, j, k);

                                                ///////////FIXED CODE FOR THE TRAILRENDERER. IT RELIES HARD TO THE PREFJOINTS TO BE 3 SPINEMIDM,HANDLEFT,HANDRIGHT

                                                if (k == 1)
                                                {
                                                    if (j == 0)
                                                    {
                                                        if (player1TrailHandLeft.Count < trailAmount)
                                                        {
                                                            player1TrailHandLeft.Add(colorPoint);
                                                        }
                                                        else
                                                        {
                                                            player1TrailHandLeft.Clear();
                                                        }
                                                    }

                                                    if (j == 1)
                                                    {
                                                        if (player2TrailHandLeft.Count < trailAmount)
                                                        {
                                                            player2TrailHandLeft.Add(colorPoint);
                                                        }
                                                        else
                                                        {
                                                            player2TrailHandLeft.Clear();
                                                        }
                                                    }
                                                }

                                                if (k == 2)
                                                {
                                                    if (j == 0)
                                                    {
                                                        if (player1TrailHandRight.Count < trailAmount)
                                                        {
                                                            player1TrailHandRight.Add(colorPoint);
                                                        }
                                                        else
                                                        {
                                                            player1TrailHandRight.Clear();
                                                        }
                                                    }

                                                    if (j == 1)
                                                    {
                                                        if (player2TrailHandRight.Count < trailAmount)
                                                        {
                                                            player2TrailHandRight.Add(colorPoint);
                                                        }
                                                        else
                                                        {
                                                            player2TrailHandRight.Clear();
                                                        }
                                                    }
                                                }

                                                //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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
            }
        }
    }


    void Update()
    {
        //print(p1UInput.userInput);
        countDown += Time.deltaTime;

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
            //var diff = playersId.Count - countBodies;
            //for (int i = 1; i <= diff; i++)
            //{
            //    bodyJoints.RemoveAt(bodyJoints.Count - i);
            //    playersJointsHeight.RemoveAt(playersJointsHeight.Count - i);
            //    playersMinMaxHeight.RemoveAt(playersMinMaxHeight.Count - i);
            //    playersId.RemoveAt(playersId.Count - i);
            //}

            for (int i = 0; i < playersId.Count; i++)
            {
                if (players[i] == false)
                {
                    bodyJoints.RemoveAt(i);
                    playersJointsHeight.RemoveAt(i);
                    playersMinMaxHeight.RemoveAt(i);
                    playersMinMaxHeightInPixels.RemoveAt(i);
                    spineMidHeightInPixels.RemoveAt(i);
                    spineMidPos.RemoveAt(i);
                    playersMidHighLowMat.RemoveAt(i);
                    playersId.RemoveAt(i);
                }
            }

        }
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
                                if (bodyJoints[k][i, 0] != 0 && bodyJoints[k][i, 1] != 0)
                                {
                                    if (jt == prefJoints[0]) //the base bodyjoint like spinemid needs to be at the first place ALWAYS!!!!!!
                                    {
                                        if (/*zoneChanged == false &&*/ k < 2)
                                        {
                                            Graphics.DrawTexture(new Rect(bodyJoints[k][i, 0] - 70 / 2, bodyJoints[k][i, 1] - 70 / 2, 70, 70), playersMat[k]);
                                        }
                                        //else if (zoneChanged == true && k < 2)
                                        //{
                                        //    if (playersMidHighLowMat[k] != null)
                                        //    {
                                        //        Graphics.DrawTexture(new Rect(bodyJoints[k][i, 0] - 70 / 2, bodyJoints[k][i, 1] - 70 / 2, 70, 70), playersMidHighLowMat[k]);

                                        //        //if (playersMidHighLowMat[k] == LowHighMats[0] || playersMidHighLowMat[k] == LowHighMats[2])
                                        //        //{
                                        //        //    if (k == 0)
                                        //        //    {
                                        //        //        p1UInput.userInput = "low";
                                        //        //    }
                                        //        //    else if (k == 1)
                                        //        //    {
                                        //        //        p2UInput.userInput = "low";
                                        //        //    }
                                        //        //}
                                        //        //else if (playersMidHighLowMat[k] == LowHighMats[1] || playersMidHighLowMat[k] == LowHighMats[3])
                                        //        //{
                                        //        //    if (k == 0)
                                        //        //    {
                                        //        //        p1UInput.userInput = "high";
                                        //        //    }
                                        //        //    else if (k == 1)
                                        //        //    {
                                        //        //        p2UInput.userInput = "high";
                                        //        //    }
                                        //        //}
                                        //    }
                                        //}
                                    }
                                    else
                                    {
                                        if (playersJointsHeight[k][p] > playersJointsHeight[k][0] && k < 2)          //K will be restricted to 2 for only two players.
                                        {
                                            moveHands = true;

                                            Graphics.DrawTexture(new Rect(bodyJoints[k][i, 0] - 125 / 2, ((playersMinMaxHeightInPixels[k][p, 0] + playersMinMaxHeightInPixels[k][p, 1])/2) - 15 / 2, 125, 15), playersMat[k]);

                                            if (useMidOutput == false)
                                            {
                                                if (playersJointsHeight[k][p] > (playersMinMaxHeight[k][p, 0] + playersMinMaxHeight[k][p, 1]) / 2)
                                                {
                                                    //print("High");      ////////////////////////////////////  HERE IS THE HIGH INPUT////////////////////////////////////

                                                    if (k == 0)
                                                    {
                                                        //print("I am here");
                                                        p1UInput.userInput = "high";
                                                        //print(p1UInput.userInput);
                                                        
                                                    }
                                                    else if (k == 1)
                                                    {
                                                        p2UInput.userInput = "high";
                                                    }

                                                    switch (k)
                                                    {
                                                        case 0:
                                                            Graphics.DrawTexture(new Rect(bodyJoints[k][i, 0] - textureWidth / 2, bodyJoints[k][i, 1] - textureHeight / 2, textureWidth, textureHeight), LowHighMats[1]);

                                                            if (p == 1)
                                                            {
                                                                foreach (Kinect.ColorSpacePoint point in player1TrailHandLeft)
                                                                {
                                                                    Graphics.DrawTexture(new Rect(point.X - textureWidth / 2, point.Y - textureHeight / 2, textureWidth, textureHeight), LowHighMats[1]);
                                                                }
                                                            }
                                                            else if (p == 2)
                                                            {
                                                                foreach (Kinect.ColorSpacePoint point in player1TrailHandRight)
                                                                {
                                                                    Graphics.DrawTexture(new Rect(point.X - textureWidth / 2, point.Y - textureHeight / 2, textureWidth, textureHeight), LowHighMats[1]);
                                                                }
                                                            }

                                                            break;
                                                        case 1:
                                                            Graphics.DrawTexture(new Rect(bodyJoints[k][i, 0] - textureWidth / 2, bodyJoints[k][i, 1] - textureHeight / 2, textureWidth, textureHeight), LowHighMats[3]);

                                                            if (p == 1)
                                                            {
                                                                foreach (Kinect.ColorSpacePoint point in player2TrailHandLeft)
                                                                {
                                                                    Graphics.DrawTexture(new Rect(point.X - textureWidth / 2, point.Y - textureHeight / 2, textureWidth, textureHeight), LowHighMats[3]);
                                                                }
                                                            }
                                                            else if (p == 2)
                                                            {
                                                                foreach (Kinect.ColorSpacePoint point in player2TrailHandRight)
                                                                {
                                                                    Graphics.DrawTexture(new Rect(point.X - textureWidth / 2, point.Y - textureHeight / 2, textureWidth, textureHeight), LowHighMats[3]);
                                                                }
                                                            }

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

                                                            if (p == 1)
                                                            {
                                                                foreach (Kinect.ColorSpacePoint point in player1TrailHandLeft)
                                                                {
                                                                    Graphics.DrawTexture(new Rect(point.X - textureWidth / 2, point.Y - textureHeight / 2, textureWidth, textureHeight), LowHighMats[0]);
                                                                }
                                                            }
                                                            else if (p == 2)
                                                            {
                                                                foreach (Kinect.ColorSpacePoint point in player1TrailHandRight)
                                                                {
                                                                    Graphics.DrawTexture(new Rect(point.X - textureWidth / 2, point.Y - textureHeight / 2, textureWidth, textureHeight), LowHighMats[0]);
                                                                }
                                                            }

                                                            break;
                                                        case 1:
                                                            Graphics.DrawTexture(new Rect(bodyJoints[k][i, 0] - textureWidth / 2, bodyJoints[k][i, 1] - textureHeight / 2, textureWidth, textureHeight), LowHighMats[2]);

                                                            if (p == 1)
                                                            {
                                                                foreach (Kinect.ColorSpacePoint point in player2TrailHandLeft)
                                                                {
                                                                    Graphics.DrawTexture(new Rect(point.X - textureWidth / 2, point.Y - textureHeight / 2, textureWidth, textureHeight), LowHighMats[2]);
                                                                }
                                                            }
                                                            else if (p == 2)
                                                            {
                                                                foreach (Kinect.ColorSpacePoint point in player2TrailHandRight)
                                                                {
                                                                    Graphics.DrawTexture(new Rect(point.X - textureWidth / 2, point.Y - textureHeight / 2, textureWidth, textureHeight), LowHighMats[2]);
                                                                }
                                                            }

                                                            break;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                /////////////////CODE FOR THE 3RD OUTPUT. NEEDS TESTINGS. WORKS BUT NOT PERFECT//////////////
                                                if (playersJointsHeight[k][1] > (playersMinMaxHeight[k][1, 0] + playersMinMaxHeight[k][1, 1]) / 2
                                                    && playersJointsHeight[k][2] > (playersMinMaxHeight[k][2, 0] + playersMinMaxHeight[k][2, 1]) / 2)
                                                {
                                                    print("High");


                                                }
                                                else if (playersJointsHeight[k][1] <= (playersMinMaxHeight[k][1, 0] + playersMinMaxHeight[k][1, 1]) / 2
                                                    && playersJointsHeight[k][2] <= (playersMinMaxHeight[k][2, 0] + playersMinMaxHeight[k][2, 1]) / 2)
                                                {
                                                    print("Low");

                                                }
                                                else if (playersJointsHeight[k][1] > playersJointsHeight[k][0] && playersJointsHeight[k][2] > playersJointsHeight[k][0])
                                                {
                                                    print("Mid");
                                                }
                                            }
                                        }
                                        else if (k < 2 && playersJointsHeight[k][1] <= playersJointsHeight[k][0] && playersJointsHeight[k][2] <= playersJointsHeight[k][0] /*&& moveHands == true*/)
                                        {
                                            if (k == 0)
                                            {
                                                p1UInput.userInput = null;
                                            }
                                            else if (k == 1)
                                            {
                                                p2UInput.userInput = null;
                                            }

                                            //moveHands = false;
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

    void SortMinMax(float height, int player, int joint)
    {
        //if (joint == 0)
        //{
        //    if (height < playersMinMaxHeight[player][joint, 0])
        //    {
        //        playersMinMaxHeight[player][joint, 0] = height;
        //    }
        //}
        //else
        //{
        //    if (height < playersMinMaxHeight[player][joint, 0] && height > jointsHeight[0])
        //    {
        //        playersMinMaxHeight[player][joint, 0] = height;
        //    }
        //}

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


    IEnumerator StopInput(int j)
    {
        yield return new WaitForSeconds(timeToStopInputs);
        if (j == 0)
        {
            p1UInput.userInput = null;
        }
        else if (j == 1)
        {
            p2UInput.userInput = null;
        }
    }

}
