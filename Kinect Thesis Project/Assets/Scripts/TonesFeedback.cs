using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TonesFeedback : MonoBehaviour {

    public List<Material> nodes = new List<Material>();
    public float changeToneTime;
    float timeCounter;

	// Use this for initialization
	void Start () {
        GetComponent<ParticleSystemRenderer>().material = nodes[0];
        timeCounter = 0;
        GetComponent<ParticleSystem>().enableEmission = false;

    }
	
	// Update is called once per frame
	void Update () {
		if(timeCounter > changeToneTime)
        {
            //Allakse tone
            var randomTone = Random.Range(0, nodes.Count);
            GetComponent<ParticleSystemRenderer>().material = nodes[randomTone];
            timeCounter = 0;
        }
        timeCounter += Time.deltaTime;

        if(Manager.instance.playTones == true || Manager.instance.playTonesBody == true || Manager.instance.playTonesBodyHands == true)
        {
            //if(Manager.instance.playTones == true)
            //{
            //    print("Notes caused of hands");
            //}
            //if(Manager.instance.playTonesBody == true)
            //{
            //    print("Notes caused of bodies");
            //}
            //if(Manager.instance.playTonesBodyHands == true)
            //{
            //    print("Notes caused of a mix between bodies and hands");
            //}
            GetComponent<ParticleSystem>().enableEmission = true;
            for(int i=0; i<LogData.instance.pID.Count; i++)
            {
                if(LogData.instance.active[i] == true)
                {
                    LogData.instance.timePlayingTogether[i] += Time.deltaTime;
                }
            }
        }
        else
        {
            GetComponent<ParticleSystem>().enableEmission = false;
        }
	}
}
