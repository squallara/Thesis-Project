using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

public class BodyPartsDetection : MonoBehaviour
{
    //public Material BoneMaterial;
    public GameObject BodySourceManager;

    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    private BodySourceManager _BodyManager;
    private bool player1Assigned;
    private ulong player1Id, player2Id;

    void Start()
    {
        player1Assigned = false;
    }


    void Update()
    {
        if (BodySourceManager == null)
        {
            return;
        }

        _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
        if (_BodyManager == null)
        {
            return;
        }

        Kinect.Body[] data = _BodyManager.GetData();
        if (data == null)
        {
            return;
        }

        List<ulong> trackedIds = new List<ulong>();
        foreach (var body in data)
        {
            if (body == null)
            {
                continue;
            }

            if (body.IsTracked)
            {
                trackedIds.Add(body.TrackingId);
            }
        }

        List<ulong> knownIds = new List<ulong>(_Bodies.Keys);

        // First delete untracked bodies
        foreach (ulong trackingId in knownIds)
        {
            if (!trackedIds.Contains(trackingId))
            {
                Destroy(_Bodies[trackingId]);
                _Bodies.Remove(trackingId);
                if(trackingId == player1Id)
                {
                    player1Assigned = false;
                }
            }
        }

        foreach (var body in data)
        {
            if (body == null)
            {
                continue;
            }

            if (body.IsTracked)
            {
                if (!_Bodies.ContainsKey(body.TrackingId))
                {
                    _Bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);
                }

                RefreshBodyObject(body, _Bodies[body.TrackingId]);
            }
        }
    }

    private GameObject CreateBodyObject(ulong id)
    {
        GameObject body = new GameObject("Body:" + id);
        if(player1Assigned == false)
        {
            player1Id = id;
            player1Assigned = true;
        }
        else
        {
            player2Id = id;
        }

        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            //GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            if (jt == Kinect.JointType.SpineMid || jt == Kinect.JointType.HandLeft || jt == Kinect.JointType.HandRight)
            {
                /////////////NEED TO KNOW WHO IS THE PLAYER/////////////////////////////////////////////////////
                if (id == player1Id)
                {
                    GameObject jointObj = Instantiate(SceneManager.instance.player1);

                    if (jt == Kinect.JointType.SpineMid)
                    {
                        jointObj.transform.localScale = new Vector3(1.5f, 1.5f, 0.1f);
                        SceneManager.instance.spineMidPlayer1 = jointObj;
                    }
                    else
                    {
                        jointObj.transform.localScale = new Vector3(1f, 1f, 1f);
                        jointObj.GetComponent<MeshRenderer>().enabled = false;
                        jointObj.AddComponent<TrailRenderer>();
                        var trRend = jointObj.GetComponent<TrailRenderer>();
                        trRend.time = 0.40f;
                        trRend.widthMultiplier = 0.35f;
                        trRend.numCapVertices = 5;
                        trRend.material = SceneManager.instance.player1mat;
                        //trRend.enabled = false;
                        if(jt == Kinect.JointType.HandLeft)
                        {
                            SceneManager.instance.player1HandLeft = jointObj;
                        }
                        else
                        {
                            SceneManager.instance.player1HandRight = jointObj;
                        }
                    }
                    jointObj.name = jt.ToString();
                    jointObj.transform.parent = body.transform;
                }
                else
                {
                    //print("Do something for player2");
                    GameObject jointObj = Instantiate(SceneManager.instance.player2);

                    if (jt == Kinect.JointType.SpineMid)
                    {
                        jointObj.transform.localScale = new Vector3(1.5f, 1.5f, 0.1f);
                        SceneManager.instance.spineMidPlayer2 = jointObj;
                    }
                    else
                    {
                        jointObj.transform.localScale = new Vector3(1f, 1f, 1f);
                        jointObj.GetComponent<MeshRenderer>().enabled = false;
                        jointObj.AddComponent<TrailRenderer>();
                        var trRend = jointObj.GetComponent<TrailRenderer>();
                        trRend.time = 0.40f;
                        trRend.widthMultiplier = 0.35f;
                        trRend.numCapVertices = 5;
                        trRend.material = SceneManager.instance.player2mat;
                        //trRend.enabled = false;
                        if (jt == Kinect.JointType.HandLeft)
                        {
                            SceneManager.instance.player2HandLeft = jointObj;
                        }
                        else
                        {
                            SceneManager.instance.player2HandRight = jointObj;
                        }
                    }
                    jointObj.name = jt.ToString();
                    jointObj.transform.parent = body.transform;
                }
            }
        }

        return body;
    }

    private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
    {
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            Kinect.Joint sourceJoint = body.Joints[jt];
            Kinect.Joint? targetJoint = null;

            if (jt == Kinect.JointType.SpineMid || jt == Kinect.JointType.HandLeft || jt == Kinect.JointType.HandRight)
            {
                Transform jointObj = bodyObject.transform.FindChild(jt.ToString());
                jointObj.localPosition = GetVector3FromJoint(sourceJoint);
            }
        }
    }


    private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
    }
}
