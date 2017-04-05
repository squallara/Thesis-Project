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

        if(Manager.instance.playTones == true)
        {
            GetComponent<ParticleSystem>().enableEmission = true;
        }
        else
        {
            GetComponent<ParticleSystem>().enableEmission = false;
        }
	}
}
