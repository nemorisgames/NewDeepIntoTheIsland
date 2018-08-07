using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtObject : MonoBehaviour {
    public Transform objective;
    public Transform objectInBetween;
    public float refreshTime = 0.1f;
    float currentTime;
    float currentDistance;
    bool active = true;
    public GameObject window;
	// Use this for initialization
	void Start ()
    {
        transform.LookAt(objective);
        currentTime = Time.time + refreshTime;
        currentDistance = Vector3.Distance(objective.position, objectInBetween.position);
    }

    public void Activate()
    {
        active = true;
        window.SetActive(active);
    }

    public void Deactivate()
    {
        active = false;
        window.SetActive(active);
    }

    public void Toggle()
    {
        active = !active;
        window.SetActive(active);
    }
	
	// Update is called once per frame
	void Update () {
        if (!active) return;
		if(currentTime < Time.time)
        {
            transform.LookAt(objective);
            float step = Vector3.Distance(objective.position, objectInBetween.position) - currentDistance;
            currentDistance += step;
            objectInBetween.localPosition += new Vector3(0f, 0f, step);
            currentTime = Time.time + refreshTime;
        }
        
	}
}
