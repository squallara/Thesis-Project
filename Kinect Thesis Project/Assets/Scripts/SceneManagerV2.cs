using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManagerV2 : MonoBehaviour {

    public static SceneManagerV2 instance;

    [HideInInspector]
    public GameObject player1HandLeft, player1HandRight, player2HandLeft, player2HandRight, spineMidPlayer1, spineMidPlayer2;
    [HideInInspector]
    public float[] playersHandsSpeed; //where 0-p1LeftSpeed, 1-p1RightSpeed, 2-p2LeftSpeed, 3-p2RightSpeed
    [HideInInspector]
    public float[] playersHandsAcc; //where 0-p1LeftAcc, 1-p1RightAcc, 2-p2LeftAcc, 3-p2RightAcc
    [HideInInspector]
    public float[] maxSpeed;  //same order like before
    [HideInInspector]
    public float[] distribution, zoneDistribution; //same order. For the zoneDistribution I using cell 0 for player1 and cell 1 for player2
    [HideInInspector]
    public float[] playersPrevSpeed;    //where 0-p1LeftPrevSpeed, 1-p1RightPrevSpeed, 2-p2LeftPrevSpeed, 3-p2RightPrevSpeed

    public GameObject player1, player2;
    public Material player1mat, player2mat, player1HighMat, player1LowMat, player2HighMat, player2LowMat;
    public int minMovementSpeed, maxMovementSpeed; //we need a restriction to the max movement speed due to some jumps of the sensor that triggers huge speeds

    private enum Zone { far, neutral, close };
    private Zone zone;

    private Vector3 p1LeftPrevPos, p1RightPrevPos, p2LeftPrevPos, p2RightPrevPos;
    private float currVel, currAcc;
    private bool p1PosAssigned, p2PosAssigned, p1ZoneAssigned, p2ZoneAssigned;
    private float[,] minMaxDistance; // 0,0 - minPlayer1    0,1 - maxPlayer1    1,0 - minPlayer2    1,1 - maxPlayer2

    private float maxAcc;

    public UserInput userInput1, userInput2;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        playersHandsSpeed = new float[4];
        playersHandsAcc = new float[4];
        maxSpeed = new float[4];
        distribution = new float[4];
        playersPrevSpeed = new float[4];
        zoneDistribution = new float[2];
        minMaxDistance = new float[2, 2];
        p1PosAssigned = false;
        p2PosAssigned = false;
        p1ZoneAssigned = false;
        p2ZoneAssigned = false;
        maxAcc = 0f;
    }

    void Update()
    {
        if (player1HandLeft != null && player1HandRight != null)
        {
            if (p1PosAssigned == false)
            {
                p1LeftPrevPos = player1HandLeft.transform.position;
                p1RightPrevPos = player1HandRight.transform.position;
                p1PosAssigned = true;
            }
            playersHandsSpeed[0] = SpeedDetection(player1HandLeft);
            playersHandsSpeed[1] = SpeedDetection(player1HandRight);
            if (playersHandsSpeed[0] < maxMovementSpeed)
            {
                playersHandsAcc[0] = AccDetection(player1HandLeft);
            }
            if (playersHandsSpeed[1] < maxMovementSpeed)
            {
                playersHandsAcc[1] = AccDetection(player1HandRight);
            }
        }
        if (player2HandLeft != null && player2HandRight != null)
        {
            if (p2PosAssigned == false)
            {
                p2LeftPrevPos = player2HandLeft.transform.position;
                p2RightPrevPos = player2HandRight.transform.position;
                p2PosAssigned = true;
            }
            playersHandsSpeed[2] = SpeedDetection(player2HandLeft);
            playersHandsSpeed[3] = SpeedDetection(player2HandRight);
            if (playersHandsSpeed[2] < maxMovementSpeed)
            {
                playersHandsAcc[2] = AccDetection(player2HandLeft);
            }
            if (playersHandsSpeed[3] < maxMovementSpeed)
            {
                playersHandsAcc[3] = AccDetection(player2HandLeft);
            }
        }
        if (spineMidPlayer1 != null)
        {
            if (p1ZoneAssigned == false)
            {
                AssignMinMaxZone(spineMidPlayer1);
            }
            else
            {
                DetectDistance(spineMidPlayer1);
                if (spineMidPlayer1.transform.position.z <= (minMaxDistance[0, 0] + zoneDistribution[0])) //Put a restriction so it doesn't start from close. chacke the assigned boolean
                {
                    //print("zone: Close");
                    userInput1.depthClose = "Close";
                }
                else if (spineMidPlayer1.transform.position.z > (minMaxDistance[0, 0] + zoneDistribution[0]) && spineMidPlayer1.transform.position.z <= ((minMaxDistance[0, 0] + zoneDistribution[0]) + zoneDistribution[0]))
                {
                    //print("zone: Neutral");
                    userInput1.depthClose = "Mid";
                }
                else if (spineMidPlayer1.transform.position.z > ((minMaxDistance[0, 0] + zoneDistribution[0]) + zoneDistribution[0]))
                {
                    //print("zone: Far");
                    userInput1.depthClose = "Far";
                }
            }
        }
        if (spineMidPlayer2 != null)
        {
            if (p2ZoneAssigned == false)
            {
                AssignMinMaxZone(spineMidPlayer2);
            }
            else
            {
                DetectDistance(spineMidPlayer2);
                if (spineMidPlayer2.transform.position.z <= (minMaxDistance[1, 0] + zoneDistribution[1]))
                {
                    //print("zone: Close");
                    userInput2.depthClose = "Close";
                }
                else if (spineMidPlayer2.transform.position.z > (minMaxDistance[1, 0] + zoneDistribution[1]) && spineMidPlayer2.transform.position.z <= ((minMaxDistance[1, 0] + zoneDistribution[1]) + zoneDistribution[1]))
                {
                    //print("zone: Neutral");
                    userInput2.depthClose = "Mid";
                }
                else if (spineMidPlayer2.transform.position.z > ((minMaxDistance[1, 0] + zoneDistribution[1]) + zoneDistribution[1]))
                {
                    //print("zone: Far");
                    userInput2.depthClose = "Far";
                }
            }
        }

        if (player1HandLeft != null || player1HandRight != null || player2HandLeft != null || player2HandRight != null)
        {
            for (int i = 0; i < 4; i++)     //I would like to replace later number 4 with a more dynamic option.
            {
                if (Mathf.Round(playersHandsSpeed[i]) >= minMovementSpeed && Mathf.Round(playersHandsSpeed[i]) < maxMovementSpeed)
                {
                    //print(Mathf.Round(playersHandsSpeed[i]));
                    //switch(i)
                    //{
                    //    case 0:
                    //        if (player1HandLeft != null)
                    //        {
                    //            player1HandLeft.GetComponent<TrailRenderer>().enabled = true;
                    //        }
                    //        break;
                    //    case 1:
                    //        if (player1HandRight != null)
                    //        {
                    //            player1HandRight.GetComponent<TrailRenderer>().enabled = true;
                    //        }
                    //        break;
                    //    case 2:
                    //        if (player2HandLeft != null)
                    //        {
                    //            player2HandLeft.GetComponent<TrailRenderer>().enabled = true;
                    //        }
                    //        break;
                    //    case 3:
                    //        if (player2HandRight != null)
                    //        {
                    //            player2HandRight.GetComponent<TrailRenderer>().enabled = true;
                    //        }
                    //        break;
                    //}

                    SortMax(i);
                    distribution[i] = (maxSpeed[i] - minMovementSpeed) / 3;
                    if (Mathf.Round(playersHandsSpeed[i]) <= (distribution[i] + minMovementSpeed) && playersHandsAcc[i] < 5000f)   //////CALCULATE THE ACCELERATION ALSO IN ORDER TO CLARIFY THEIR HAND'S INTENTION. Right now I am using observed numbers for limiting the acceleration, need to be dynamic
                    {
                        //print("Low sound effect");
                        if (i < 2)
                        {
                            //userInput1.userInput = "low";
                            if (i == 0)
                            {
                                player1HandLeft.GetComponent<TrailRenderer>().material = player1LowMat;
                            }
                            else
                            {
                                player1HandRight.GetComponent<TrailRenderer>().material = player1LowMat;
                            }

                        }
                        else
                        {
                            //userInput2.userInput = "low";
                            if (i == 2)
                            {
                                player2HandLeft.GetComponent<TrailRenderer>().material = player2LowMat;
                            }
                            else
                            {
                                player2HandRight.GetComponent<TrailRenderer>().material = player2LowMat;
                            }
                        }
                    }
                    else if (Mathf.Round(playersHandsSpeed[i]) > (distribution[i] + minMovementSpeed) && Mathf.Round(playersHandsSpeed[i]) <= ((distribution[i] + minMovementSpeed) + distribution[i]) && playersHandsAcc[i] < 10000f)
                    {
                        //print("Mid sound effect");
                        if (i < 2)
                        {
                            //userInput1.userInput = "mid";
                            if (i == 0)
                            {
                                player1HandLeft.GetComponent<TrailRenderer>().material = player1mat;
                            }
                            else
                            {
                                player1HandRight.GetComponent<TrailRenderer>().material = player1mat;
                            }
                        }
                        else
                        {
                            //userInput2.userInput = "mid";
                            if (i == 2)
                            {
                                player2HandLeft.GetComponent<TrailRenderer>().material = player2mat;
                            }
                            else
                            {
                                player2HandRight.GetComponent<TrailRenderer>().material = player2mat;
                            }
                        }
                    }
                    else if (Mathf.Round(playersHandsSpeed[i]) > ((distribution[i] + minMovementSpeed) + distribution[i]) && playersHandsAcc[i] >= 10000f)
                    {
                        //print("High sound effect");
                        if (i < 2)
                        {
                            //userInput1.userInput = "high";
                            if (i == 0)
                            {
                                player1HandLeft.GetComponent<TrailRenderer>().material = player1HighMat;
                            }
                            else
                            {
                                player1HandRight.GetComponent<TrailRenderer>().material = player1HighMat;
                            }
                        }
                        else
                        {
                            //userInput2.userInput = "high";
                            if (i == 2)
                            {
                                player2HandLeft.GetComponent<TrailRenderer>().material = player2HighMat;
                            }
                            else
                            {
                                player2HandRight.GetComponent<TrailRenderer>().material = player2HighMat;
                            }
                        }
                    }
                }
                //else if(Mathf.Round(playersHandsSpeed[i]) < minMovementSpeed)
                //{
                //    switch (i)
                //    {
                //        case 0:
                //            if (player1HandLeft != null)
                //            {
                //                player1HandLeft.GetComponent<TrailRenderer>().enabled = false;
                //            }
                //            break;
                //        case 1:
                //            if (player1HandRight != null)
                //            {
                //                player1HandRight.GetComponent<TrailRenderer>().enabled = false;
                //            }
                //            break;
                //        case 2:
                //            if (player2HandLeft != null)
                //            {
                //                player2HandLeft.GetComponent<TrailRenderer>().enabled = false;
                //            }
                //            break;
                //        case 3:
                //            if (player2HandRight != null)
                //            {
                //                player2HandRight.GetComponent<TrailRenderer>().enabled = false;
                //            }
                //            break;
                //    }
                //}
            }
        }

        //if(playersHandsAcc[0] > maxAcc /*&& playersHandsAcc[0] < 10000f*/)
        //{
        //    maxAcc = playersHandsAcc[0];
        //    print(maxAcc);
        //}
    }


    //Detect their mean speed by using simple physics.
    private float SpeedDetection(GameObject obj)
    {
        if (obj == player1HandLeft)
        {
            currVel = (obj.transform.position - p1LeftPrevPos).magnitude / Time.deltaTime;
            p1LeftPrevPos = obj.transform.position;
        }
        else if (obj == player1HandRight)
        {
            currVel = (obj.transform.position - p1RightPrevPos).magnitude / Time.deltaTime;
            p1RightPrevPos = obj.transform.position;
        }
        else if (obj == player2HandLeft)
        {
            currVel = (obj.transform.position - p2LeftPrevPos).magnitude / Time.deltaTime;
            p2LeftPrevPos = obj.transform.position;
        }
        else if (obj == player2HandRight)
        {
            currVel = (obj.transform.position - p2RightPrevPos).magnitude / Time.deltaTime;
            p2RightPrevPos = obj.transform.position;
        }

        return currVel;
    }


    //Sort only the maximum speed because min is defined always by the threshold that they have to overpass in order to register a movement.
    private void SortMax(int k)
    {
        if (Mathf.Round(playersHandsSpeed[k]) > maxSpeed[k])
        {
            maxSpeed[k] = Mathf.Round(playersHandsSpeed[k]);
        }
    }


    //Detecting the acceleration.
    private float AccDetection(GameObject obj)
    {
        if (obj == player1HandLeft)
        {
            currAcc = Mathf.Abs((playersHandsSpeed[0] - playersPrevSpeed[0])) / Time.deltaTime;
            playersPrevSpeed[0] = playersHandsSpeed[0];
        }
        else if (obj == player1HandRight)
        {
            currAcc = Mathf.Abs((playersHandsSpeed[1] - playersPrevSpeed[1])) / Time.deltaTime;
            playersPrevSpeed[1] = playersHandsSpeed[1];
        }
        else if (obj == player2HandLeft)
        {
            currAcc = Mathf.Abs((playersHandsSpeed[2] - playersPrevSpeed[2])) / Time.deltaTime;
            playersPrevSpeed[2] = playersHandsSpeed[2];
        }
        else if (obj == player2HandRight)
        {
            currAcc = Mathf.Abs((playersHandsSpeed[3] - playersPrevSpeed[3])) / Time.deltaTime;
            playersPrevSpeed[3] = playersHandsSpeed[3];
        }

        return currAcc;
    }

    //Organize which is the min/max distance they can travel from kinect in order to arrange the zones.
    private void DetectDistance(GameObject obj)
    {
        if (obj == spineMidPlayer1)
        {
            if (spineMidPlayer1.transform.position.z < minMaxDistance[0, 0])
            {
                minMaxDistance[0, 0] = spineMidPlayer1.transform.position.z;
            }
            if (spineMidPlayer1.transform.position.z > minMaxDistance[0, 1])
            {
                minMaxDistance[0, 1] = spineMidPlayer1.transform.position.z;
            }
            zoneDistribution[0] = (minMaxDistance[0, 1] - minMaxDistance[0, 0]) / 3;

        }
        else if (obj = spineMidPlayer2)
        {
            if (spineMidPlayer2.transform.position.z < minMaxDistance[1, 0])
            {
                minMaxDistance[1, 0] = spineMidPlayer2.transform.position.z;
            }
            if (spineMidPlayer2.transform.position.z > minMaxDistance[1, 1])
            {
                minMaxDistance[1, 1] = spineMidPlayer2.transform.position.z;
            }
            zoneDistribution[1] = (minMaxDistance[1, 1] - minMaxDistance[1, 0]) / 3;
        }
    }


    //Initialize players max min distance from the kinect.
    private void AssignMinMaxZone(GameObject obj)
    {
        if (obj == spineMidPlayer1)
        {
            minMaxDistance[0, 0] = spineMidPlayer1.transform.position.z;
            minMaxDistance[0, 1] = spineMidPlayer1.transform.position.z;
            p1ZoneAssigned = true;
        }
        else if (obj == spineMidPlayer2)
        {
            minMaxDistance[1, 0] = spineMidPlayer2.transform.position.z;
            minMaxDistance[1, 1] = spineMidPlayer2.transform.position.z;
            p2ZoneAssigned = true;
        }
    }
}
