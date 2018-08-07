using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerSendMessage : MonoBehaviour {
    public GameObject objective;
    public string messageOnEnter;
    public string messageOnExit;
    public string parameter;
    public bool selfDestroy = true;
    // Use this for initialization
    void Start () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        if (objective != null && messageOnEnter != "")
            objective.SendMessage(messageOnEnter, parameter);
        if (selfDestroy) Destroy(gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if (objective != null && messageOnExit != "")
            objective.SendMessage(messageOnExit, parameter);
        if (selfDestroy) Destroy(gameObject);
    }

    // Update is called once per frame
    void Update () {
		
	}
}
