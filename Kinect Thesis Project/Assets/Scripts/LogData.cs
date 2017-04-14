using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class LogData : MonoBehaviour
{
    public static LogData instance;
    DateTime dateAndTime;

    //[HideInInspector]
    public List<ulong> pID;
    //[HideInInspector]
    public List<string> color;
    //[HideInInspector]
    public List<bool> active;
    //[HideInInspector]
    public List<float> timeSpentAlone, playAlone;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        dateAndTime = DateTime.Now;
        pID = new List<ulong>();
        color = new List<string>();
        active = new List<bool>();

    }

    public void WriteFile()
    {
        if (!File.Exists("C:/Users/User/Desktop/DataCollection/" + dateAndTime.ToString("yyyy_MM_dd__HH_mm_ss") + ".txt"))
        {

            //////////////////////////////EXAMPLE/////////////////////////////////
            File.AppendAllText("C:/Users/User/Desktop/DataCollection/" + dateAndTime.ToString("yyyy_MM_dd__HH_mm_ss") + ".txt", "Players,TimeSpent,Highes,Lows,H5s" + "\r\n734587945983,50,15,20,5");
            //"\r\n" + Manager.instance.playersId[0].ToString() + "\r\n" + "played: " + Manager.instance.P1Highes + " highes")
        }

    }
}
