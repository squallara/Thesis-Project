using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class LogData : MonoBehaviour
{

    public static LogData instance;
    DateTime dateAndTime;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        dateAndTime = DateTime.Now;
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
