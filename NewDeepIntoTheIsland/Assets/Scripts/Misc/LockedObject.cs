using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockedObject : MonoBehaviour {
    public bool unlocked = false;
    public CallbackObject[] callbacks;
    public AudioClip openSound;
	// Use this for initialization
	void Start () {
		
	}

    public void Unlock()
    {
        if(openSound != null)
        {
            AudioManager.Instance.PlayOneShot(openSound);
        }

        unlocked = true;
        if (callbacks.Length > 0)
        {
            foreach (CallbackObject c in callbacks)
                c.Call();
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
