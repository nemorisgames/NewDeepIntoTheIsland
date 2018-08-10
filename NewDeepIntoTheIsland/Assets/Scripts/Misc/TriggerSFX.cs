using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerSFX : MonoBehaviour {
    public AudioSource audioSource;
	// Use this for initialization
	void Start () {
		
	}

    void OnTriggerEnter(Collider c)
    {
        audioSource.Play();
    }

    // Update is called once per frame
    void Update () {
		
	}
}
