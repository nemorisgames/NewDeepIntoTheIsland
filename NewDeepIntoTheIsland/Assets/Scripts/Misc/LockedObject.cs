using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockedObject : MonoBehaviour {
    public bool unlocked = false;
	// Use this for initialization
	void Start () {
		
	}

    public void Unlock()
    {
        unlocked = true;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
