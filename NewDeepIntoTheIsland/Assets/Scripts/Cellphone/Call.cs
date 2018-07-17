using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Call : MonoBehaviour {
    public bool signalAvailable = false;
    public GameObject noSignalSign;
	// Use this for initialization
	void Start () {
		
	}
    
    public void CheckSignal()
    {
        noSignalSign.SetActive(!signalAvailable);
    }
    // Update is called once per frame
    void Update () {
		
	}
}
