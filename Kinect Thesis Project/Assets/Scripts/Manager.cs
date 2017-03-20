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
    public List<ulong> playersId;       //Remove the public. Did only for observation.
    bool[] players;
    float[] jointsHeight;       //With the order of the prefJoints. (heights of the joints in general)
    float[,] minMaxHeight;
    int countBodies;
    bool foundId;

    public Texture texture;
    Kinect.PointF point;
    public List<Kinect.JointType> prefJoints;       //Assign in the inspector the pref joints you want to detect. ALWAYS first the main body.
    //bool assigned = false;
    //int count = 0;

    void Start()
    {
        sensor = Kinect.KinectSensor.GetDefault();

        if (sensor != null)
        {
            sensor.Open();
        }

        reader = sensor.OpenMultiSourceFrameReader(Kinect.FrameSourceTypes.Body | Kinect.FrameSourceTypes.Color | Kinect.FrameSourceTypes.Depth | Kinect.FrameSourceTypes.Infrared);
        reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
        point = new Kinect.PointF();
        playersId = new List<ulong>();
        bodyJoints = new List<float[,]>();
        playersJointsHeight = new List<float[]>();
        playersMinMaxHeight = new List<float[,]>();
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
                var p = 0;

                foreach (var body in bodies)
                {
                    var i = 0;
                    jointsPos = new float[25, 2];
                    jointsHeight = new float[prefJoints.Count];
                    minMaxHeight = new float[prefJoints.Count, 2];
                    for (int j = 0; j < prefJoints.Count; j++)
                    {
                        minMaxHeight[j, 0] = 1;
                        minMaxHeight[j, 1] = -1;
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
                                bodyJoints.Add(null);
                                playersJointsHeight.Add(null);
                                playersMinMaxHeight.Add(minMaxHeight);
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
                                bodyJoints.Add(null);
                                playersJointsHeight.Add(null);
                                playersMinMaxHeight.Add(minMaxHeight);
                            }

                            for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
                            {
                                var k = 0;  //k counts the which joint in the jointHeight matrix corresponding to the amount of the prefJoints
                                foreach (Kinect.JointType joint in prefJoints)
                                {
                                    if (jt == joint)
                                    {
                                        //print(body.Joints[jt].Position.Y);
                                        jointsHeight[k] = body.Joints[jt].Position.Y;
                                        for (int j = 0; j < playersId.Count; j++)       //j counts whos is the player
                                        {
                                            if (body.TrackingId == playersId[j])
                                            {
                                                SortMinMax(body.Joints[jt].Position.Y, j, k);
                                            }
                                        }
                                    }
                                    k++;
                                }

                                //3d coordinates in meters
                                Kinect.CameraSpacePoint skeletonPoint = body.Joints[jt].Position;

                                Kinect.ColorSpacePoint colorPoint = sensor.CoordinateMapper.MapCameraPointToColorSpace(skeletonPoint);

                                point.X = colorPoint.X;
                                point.Y = colorPoint.Y;

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
                                            //playersMinMaxHeight[j] = minMaxHeight;
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
        countBodies = 0;
        for (int i = 0; i < 6; i++)
        {
            if (players[i] == true)
            {
                countBodies++;
            }

            //if(i==5)
            //{
            //    print(countBodies);
            //}
        }

        if (countBodies != playersId.Count)
        {
            var diff = playersId.Count - countBodies;
            for (int i = 1; i <= diff; i++)
            {
                bodyJoints.RemoveAt(bodyJoints.Count - i);
                playersId.RemoveAt(playersId.Count - i);
                playersJointsHeight.RemoveAt(playersJointsHeight.Count - i);
                playersMinMaxHeight.RemoveAt(playersMinMaxHeight.Count - i);
            }
        }

        //print(playersMinMaxHeight[0][1, 1]);
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
                                    if (jt == prefJoints[0])
                                    {
                                        Graphics.DrawTexture(new Rect(bodyJoints[k][i, 0], bodyJoints[k][i, 1], 70, 70), texture);
                                    }
                                    else
                                    {
                                        if (playersJointsHeight[k][p] > playersJointsHeight[k][0])
                                        {
                                            Graphics.DrawTexture(new Rect(bodyJoints[k][i, 0], bodyJoints[k][i, 1], 20, 20), texture);

                                            if (useMidOutput == false)
                                            {
                                                if (playersJointsHeight[k][p] > (playersMinMaxHeight[k][p, 0] + playersMinMaxHeight[k][p, 1]) / 2)
                                                {
                                                    print("High");
                                                }
                                                else
                                                {
                                                    print("Low");
                                                }
                                            }
                                            else
                                            {
                                                /////////////////CODE FOR THE 3RD OUTPUT. NEEDS TESTINGS. WORKS BUT NOT PERFECT//////////////
                                                if(playersJointsHeight[k][1] > (playersMinMaxHeight[k][1, 0] + playersMinMaxHeight[k][1, 1]) / 2 
                                                    && playersJointsHeight[k][2] > (playersMinMaxHeight[k][2, 0] + playersMinMaxHeight[k][2, 1]) / 2)
                                                {
                                                    print("High");
                                                }
                                                else if (playersJointsHeight[k][1] <= (playersMinMaxHeight[k][1, 0] + playersMinMaxHeight[k][1, 1]) / 2
                                                    && playersJointsHeight[k][2] <= (playersMinMaxHeight[k][2, 0] + playersMinMaxHeight[k][2, 1]) / 2)
                                                {
                                                    print("Low");
                                                }
                                                else if(playersJointsHeight[k][1] > playersJointsHeight[k][0] && playersJointsHeight[k][2] > playersJointsHeight[k][0])
                                                {
                                                    print("Mid");
                                                }
                                            }
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
        if (joint == 0)
        {
            if (height < playersMinMaxHeight[player][joint, 0])
            {
                playersMinMaxHeight[player][joint, 0] = height;
            }
        }
        else
        {
            if(height < playersMinMaxHeight[player][joint, 0] && height > jointsHeight[0])
            {
                playersMinMaxHeight[player][joint, 0] = height;
            }
        }

        if (height > playersMinMaxHeight[player][joint, 1])
        {
            playersMinMaxHeight[player][joint, 1] = height;
        }
    }
}
