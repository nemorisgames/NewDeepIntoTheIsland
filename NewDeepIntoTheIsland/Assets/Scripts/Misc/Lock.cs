﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lock : MonoBehaviour {
    public LockKey[] Keys;
    public GameObject buttonDone;
    public bool opened = false;
    bool rotating = false;
    public LockedObject objectToUnlock;
    public TweenScale tweenScale;
    // Use this for initialization
    void Start () {
		
	}

    void RotateKey(int index)
    {
        if (rotating) return;
        rotating = true;
        Keys[index].tweenRotation.from = Keys[index].tweenRotation.to;
        Keys[index].tweenRotation.to = new Vector3(0f, 0f, Keys[index].tweenRotation.from.z + 120f);
        Keys[index].tweenRotation.ResetToBeginning();
        Keys[index].tweenRotation.PlayForward();
        Keys[index].currentIndex = (Keys[index].currentIndex+1)%3;
    }

    public void RotationFinished()
    {
        rotating = false;
        CheckLock();
    }
	
	void LateUpdate () {
        if (opened) return;
        if ((Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))&& !CellPhone.phoneUp)
        {
            // Get mask for the raycast
            int layerMask = LayerMask.GetMask("Interactable");
            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.forward), out hit, 3f, layerMask))
            {
                Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
                bool found = false;
                for(int i = 0; i < Keys.Length; i++)
                {
                    if(!found && hit.transform.gameObject == Keys[i].tweenRotation.gameObject)
                    {
                        found = true;
                        RotateKey(i);
                    }
                }
                if (!found)
                {
                    if(hit.transform.gameObject == buttonDone)
                    {
                        CheckLock();
                    }
                }
            }
        }
	}

    void CheckLock(){
        opened = CheckLockCombination();
        if (opened)
        {
            if (objectToUnlock != null) objectToUnlock.Unlock();
            tweenScale.PlayForward();
        }
    }

    bool CheckLockCombination()
    {
        bool success = true;
        for (int i = 0; i < Keys.Length; i++)
        {
            if (Keys[i].currentIndex != Keys[i].correctIndex)
            {
                success = false;
            }
        }
        return success;
    }
}

[System.Serializable]
public class LockKey
{
    public TweenRotation tweenRotation;
    public int correctIndex;
    public int currentIndex = 0;
}
