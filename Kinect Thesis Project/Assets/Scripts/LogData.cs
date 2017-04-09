using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class LogData : MonoBehaviour {

    public static LogData instance;

    void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    public void WriteFile()
    {
        var counter = 0;
        if(File.Exists("C:/Users/User/Desktop/DataCollection/SaveData" + counter + ".txt"))
        {
            counter++;
        }

        //////////////////////////////EXAMPLE/////////////////////////////////
        File.AppendAllText("C:/Users/User/Desktop/DataCollection/SaveData" + counter + ".txt", "This is player 1: " + "\r\n" + Manager.instance.playersId[0].ToString() + "\r\n" + "played: " + Manager.instance.P1Highes + " highes");
    }
}
