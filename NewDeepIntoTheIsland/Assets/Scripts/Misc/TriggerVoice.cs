using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerVoice : MonoBehaviour {
    public int voiceIndex;
    public bool selfDestroy = true;
    // Use this for initialization
    void Start () {
    }

    private void OnTriggerEnter(Collider other)
    {
        AudioManager.Instance.PlayVoice(voiceIndex);
        if (selfDestroy) Destroy(gameObject);
    }

    // Update is called once per frame
    void Update () {
		
	}
}
