using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KilledByWitcher : MonoBehaviour {
    public Camera cameraKill;
    public Transform witcher;
    public Transform realWitcher;
    public Transform witcherRightHand;
    public Transform witcherEyes;
    public Vector3 offsetWitcher = new Vector3(-0.3f, 0f, -0.1f);
    public Vector3 offsetCamera = new Vector3(-0.3f, 0f, 0f);
    Animator animator;
    bool grabbed = false;
    bool grabbing = false;
    public float timeGrabbed = 0.5f;
    Transform cameraTransform;
    Transform player;
    public AudioClip[] grabSounds;

	// Use this for initialization
	void Start () {
        cameraKill.gameObject.SetActive(false);
        animator = witcher.GetComponent<Animator>();
        witcher.gameObject.SetActive(false);
        cameraTransform = Camera.main.transform;
        player = GameObject.FindWithTag("Player").transform;
    }

    public void Kill()
    {
        cameraKill.transform.SetPositionAndRotation(cameraTransform.position, cameraTransform.rotation);
        AudioManager.Instance.StopBreath();
        player.gameObject.SetActive(false);
        cameraKill.gameObject.SetActive(true);
        witcher.gameObject.SetActive(true);
        //witcher.SetPositionAndRotation(cameraKill.transform.position + cameraKill.transform.forward * 1f + cameraKill.transform.up * -2.8f + cameraKill.transform.right * 0.3f, Quaternion.Euler(0f, cameraKill.transform.eulerAngles.y, cameraKill.transform.eulerAngles.z));

        witcher.SetPositionAndRotation(realWitcher.position, Quaternion.Euler(0f, realWitcher.eulerAngles.y, realWitcher.eulerAngles.z));
        //witcher.Rotate(0f, 160f, 0f);
        witcher.transform.position += witcher.transform.forward * offsetWitcher.z + witcher.transform.up * offsetWitcher.y + witcher.transform.right * offsetWitcher.x;
        animator.SetTrigger("Kill");
        realWitcher.gameObject.SetActive(false);
        StartCoroutine(GetGrabbed());
    }

    IEnumerator GetGrabbed()
    {
        foreach(AudioClip a in grabSounds)
        {
            AudioManager.Instance.PlayOneShot(a);
        }
        grabbing = true;
        yield return new WaitForSeconds(timeGrabbed);
        grabbing = false;
        cameraKill.transform.parent = witcherRightHand;
        cameraKill.transform.localPosition += offsetCamera;
        AudioManager.Instance.PlayVoice(7);
        grabbed = true;
    }
	
	// Update is called once per frame
	void Update () {
        if (grabbing)
            cameraKill.transform.forward = Vector3.Lerp(cameraKill.transform.forward, (witcher.position + witcher.up * 3f) - cameraKill.transform.position, 5f * Time.deltaTime);//new Vector3(Mathf.Lerp(cameraKill.transform.eulerAngles.x, 0f, 1f * Time.deltaTime), cameraKill.transform.eulerAngles.y, cameraKill.transform.eulerAngles.z);
    }
}
