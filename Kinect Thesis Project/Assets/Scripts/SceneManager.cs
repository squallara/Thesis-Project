using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour {

    public static SceneManager instance;

    [HideInInspector]
    public GameObject player1HandLeft, player1HandRight, player2HandLeft, player2HandRight, spineMidPlayer1, spineMidPlayer2;
    [HideInInspector]
    public float[] playersHandsSpeed; //where 0-p1LeftSpeed, 1-p1RightSpeed, 2-p2LeftSpeed, 3-p2RightSpeed
    [HideInInspector]
    public float[] maxSpeed;  //same order like before
    [HideInInspector]
    public float[] distribution, zoneDistribution; //same order. For the zoneDistribution I using cell 0 for player1 and cell 1 for player2
    
    public GameObject player1, player2;
    public Material player1mat, player2mat;
    public int minMovementSpeed, maxMovementSpeed; //we need a restriction to the max movement speed due to some jumps of the sensor that triggers huge speeds

    private enum Zone {far, neutral, close};
    private Zone zone;

    private Vector3 p1LeftPrevPos, p1RightPrevPos, p2LeftPrevPos, p2RightPrevPos;
    private float currVel;
    private bool p1PosAssigned, p2PosAssigned, p1ZoneAssigned, p2ZoneAssigned;
    private float[,] minMaxDistance; // 0,0 - minPlayer1    0,1 - maxPlayer1    1,0 - minPlayer2    1,1 - maxPlayer2


    void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
        playersHandsSpeed = new float[4];
        maxSpeed = new float[4];
        distribution = new float[4];
        zoneDistribution = new float[2];
        minMaxDistance = new float[2, 2];
        p1PosAssigned = false;
        p2PosAssigned = false;
        p1ZoneAssigned = false;
        p2ZoneAssigned = false;
    }

    void Update()
    {
        if(player1HandLeft != null && player1HandRight != null)
        {
            if (p1PosAssigned == false)
            {
                p1LeftPrevPos = player1HandLeft.transform.position;
                p1RightPrevPos = player1HandRight.transform.position;
                p1PosAssigned = true;
            }
            playersHandsSpeed[0] = SpeedDetection(player1HandLeft);
            playersHandsSpeed[1] = SpeedDetection(player1HandRight);
        }
        if(player2HandLeft != null && player2HandRight != null)
        {
            if (p2PosAssigned == false)
            {
                p2LeftPrevPos = player2HandLeft.transform.position;
                p2RightPrevPos = player2HandRight.transform.position;
                p2PosAssigned = true;
            }
            playersHandsSpeed[2] = SpeedDetection(player2HandLeft);
            playersHandsSpeed[3] = SpeedDetection(player2HandRight);
        }
        if(spineMidPlayer1 != null)
        {
            if (p1ZoneAssigned == false)
            {
                AssignMinMaxZone(spineMidPlayer1);
            }
            else
            {
                DetectDistance(spineMidPlayer1);
                if (spineMidPlayer1.transform.position.z <= (minMaxDistance[0,0] + zoneDistribution[0])) //Put a restriction so it doesn't start from close. chacke the assigned boolean
                {
                    print("zone: Close");
                }
                else if (spineMidPlayer1.transform.position.z > (minMaxDistance[0, 0] + zoneDistribution[0]) && spineMidPlayer1.transform.position.z <= ((minMaxDistance[0, 0] + zoneDistribution[0]) + zoneDistribution[0]))
                {
                    print("zone: Neutral");
                }
                else if (spineMidPlayer1.transform.position.z > ((minMaxDistance[0, 0] + zoneDistribution[0]) + zoneDistribution[0]))
                {
                    print("zone: Far");
                }
            }
        }
        if(spineMidPlayer2 != null)
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
                    print("zone: Close");
                }
                else if (spineMidPlayer2.transform.position.z > (minMaxDistance[1, 0] + zoneDistribution[1]) && spineMidPlayer2.transform.position.z <= ((minMaxDistance[1, 0] + zoneDistribution[1]) + zoneDistribution[1]))
                {
                    print("zone: Neutral");
                }
                else if (spineMidPlayer2.transform.position.z > ((minMaxDistance[1, 0] + zoneDistribution[1]) + zoneDistribution[1]))
                {
                    print("zone: Far");
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
                    SortMax(i);
                    distribution[i] = (maxSpeed[i] - minMovementSpeed) / 3;
                    if (Mathf.Round(playersHandsSpeed[i]) <= (distribution[i] + minMovementSpeed))   //////GOES TO LOW AND MID BEFORE IT GOES TO HIGH (Same problem for Mid, goes first to low) AND IT IS LOGICAL BECAUSE THE MOVEMENT OF THE HAND IT STARTS FROM LOW BEFORE IT REACHES HIGH. SOLUTION?
                    {                                                           //////IS THIS A PROBLEM ACTUALLY? SHOULDN'T THE SOUNDS CHANGES BEING CONTINUOUSLY?    CALCULATE THE ACCELERATION ALSO IN ORDER TO CLARIFY THEIR HAND'S INTENTION.
                        print("Low sound effect");
                    }
                    else if (Mathf.Round(playersHandsSpeed[i]) > (distribution[i] + minMovementSpeed) && Mathf.Round(playersHandsSpeed[i]) <= ((distribution[i] + minMovementSpeed) + distribution[i]))
                    {
                        print("Mid sound effect");
                    }
                    else if (Mathf.Round(playersHandsSpeed[i]) > ((distribution[i] + minMovementSpeed) + distribution[i]))
                    {
                        print("High sound effect");
                    }
                }
            }
        }


        ///////////////////////////Enabling/Disabling the trail renderer seems weird//////////////////////////
        //if (p1LeftSpeed < 1f)
        //{
        //    player1HandLeft.GetComponent<TrailRenderer>().enabled = false;
        //}
        //else
        //{
        //    player1HandLeft.GetComponent<TrailRenderer>().enabled = true;
        //}
    }


    //Detect their mean speed by using simple physics.
    private float SpeedDetection(GameObject obj)
    {
        if(obj == player1HandLeft)
        {
            currVel = (obj.transform.position - p1LeftPrevPos).magnitude / Time.deltaTime;
            p1LeftPrevPos = obj.transform.position;
        }
        else if(obj == player1HandRight)
        {
            currVel = (obj.transform.position - p1RightPrevPos).magnitude / Time.deltaTime;
            p1RightPrevPos = obj.transform.position;
        }
        else if (obj == player2HandLeft)
        {
            currVel = (obj.transform.position - p2LeftPrevPos).magnitude / Time.deltaTime;
            p2LeftPrevPos = obj.transform.position;
        }
        else if(obj == player2HandRight)
        {
            currVel = (obj.transform.position - p2RightPrevPos).magnitude / Time.deltaTime;
            p2RightPrevPos = obj.transform.position;
        }

        return currVel;
    }


    //Sort only the maximum speed because min is defined always by the threshold that they have to overpass in order to register a movement.
    private void SortMax(int k)
    {
        if(Mathf.Round(playersHandsSpeed[k]) > maxSpeed[k])
        {
            maxSpeed[k] = Mathf.Round(playersHandsSpeed[k]);
        }
    }


    //Organize which is the min/max distance they can travel from kinect in order to arrange the zones.
    private void DetectDistance(GameObject obj)
    {
        if(obj == spineMidPlayer1)
        {
            if(spineMidPlayer1.transform.position.z < minMaxDistance[0,0])
            {
                minMaxDistance[0, 0] = spineMidPlayer1.transform.position.z;
            }
            if(spineMidPlayer1.transform.position.z > minMaxDistance[0,1])
            {
                minMaxDistance[0, 1] = spineMidPlayer1.transform.position.z;
            }
            zoneDistribution[0] = (minMaxDistance[0, 1] - minMaxDistance[0, 0]) / 3;

        }
        else if(obj = spineMidPlayer2)
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
        if(obj == spineMidPlayer1)
        {
            minMaxDistance[0, 0] = spineMidPlayer1.transform.position.z;
            minMaxDistance[0, 1] = spineMidPlayer1.transform.position.z;
            p1ZoneAssigned = true;
        }
        else if(obj == spineMidPlayer2)
        {
            minMaxDistance[1, 0] = spineMidPlayer2.transform.position.z;
            minMaxDistance[1, 1] = spineMidPlayer2.transform.position.z;
            p2ZoneAssigned = true;
        }
    }

}
