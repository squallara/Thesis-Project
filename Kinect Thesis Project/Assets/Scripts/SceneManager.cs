using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour {

    public static SceneManager instance;

    [HideInInspector]
    public GameObject player1HandLeft, player1HandRight, player2HandLeft, player2HandRight;
    public GameObject player1, player2;
    public Material player1mat, player2mat;

    public enum Speed {high, mid, low};
    public enum Zone {far, neutral, close};

    private Vector3 p1LeftPrevPos, p1RightPrevPos, p2LeftPrevPos, p2RightPrevPos;
    private float currVel, p1LeftSpeed, p1RightSpeed, p2LeftSpeed, p2RightSpeed;


    void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
        p1LeftPrevPos = Vector3.zero;
        p1RightPrevPos = Vector3.zero;
        p2LeftPrevPos = Vector3.zero;
        p2RightPrevPos = Vector3.zero;
    }

    void Update()
    {
        if(player1HandLeft != null && player1HandRight != null)
        {
            p1LeftSpeed = SpeedDetection(player1HandLeft);
            p1RightSpeed = SpeedDetection(player1HandRight);
        }
        if(player2HandLeft != null && player2HandRight != null)
        {
            p2LeftSpeed = SpeedDetection(player2HandLeft);
            p2RightSpeed = SpeedDetection(player2HandRight);
        }
        print(p1LeftSpeed);

        ///////////////////////////Enabling/Disabling the trail renderer seems weird//////////////////////////
        //if(p1LeftSpeed < 1f)
        //{
        //    player1HandLeft.GetComponent<TrailRenderer>().enabled = false;
        //}
        //else
        //{
        //    player1HandLeft.GetComponent<TrailRenderer>().enabled = true;
        //}
    }


    public float SpeedDetection(GameObject obj)
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

}
