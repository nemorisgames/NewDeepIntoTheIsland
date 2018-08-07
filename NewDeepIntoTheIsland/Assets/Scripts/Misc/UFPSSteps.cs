using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UFPSSteps : MonoBehaviour {
    public vp_FPCamera camera;
    public AudioClip[] steps;
    public AudioSource leftFoot;
    public AudioSource rightFoot;
    bool leftSound = false;
    // Use this for initialization
    void Start () {
    }

    private void OnEnable()
    {
        camera.BobStepCallback += Stomp;
    }

    private void OnDisable()
    {
        camera.BobStepCallback -= Stomp;
    }

    void Stomp()
    {
        if (leftSound)
            leftFoot.PlayOneShot(steps[Random.Range(0, steps.Length)]);
        else
            rightFoot.PlayOneShot(steps[Random.Range(0, steps.Length)]);
        leftSound = !leftSound;
    }

    // Update is called once per frame
    void Update () {
		
	}
}
