using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KilledByWitcher : MonoBehaviour {
    public Camera cameraKill;
    public Transform witcher;
    Animator animator;

	// Use this for initialization
	void Start () {
        cameraKill.gameObject.SetActive(false);
        animator = witcher.GetComponent<Animator>();
        witcher.gameObject.SetActive(false);
    }

    public void Kill()
    {
        cameraKill.transform.SetPositionAndRotation(Camera.main.transform.position, Camera.main.transform.rotation);
        GameObject.FindWithTag("Player").gameObject.SetActive(false);
        cameraKill.gameObject.SetActive(true);
        witcher.gameObject.SetActive(true);
        witcher.SetPositionAndRotation(cameraKill.transform.position + cameraKill.transform.forward * 1f + cameraKill.transform.up * -2.8f, cameraKill.transform.rotation);
        witcher.Rotate(0f, 200f, 0f);
        animator.SetTrigger("Kill");
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
