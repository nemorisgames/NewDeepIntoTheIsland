using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KilledByWitcher : MonoBehaviour {
    public Camera cameraKill;
    public Transform witcher;
    public Transform realWitcher;
    public Transform witcherRightHand;
    public Transform witcherEyes;
    Animator animator;
    bool grabbed = false;
    bool grabbing = false;

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
        //witcher.SetPositionAndRotation(cameraKill.transform.position + cameraKill.transform.forward * 1f + cameraKill.transform.up * -2.8f + cameraKill.transform.right * 0.3f, Quaternion.Euler(0f, cameraKill.transform.eulerAngles.y, cameraKill.transform.eulerAngles.z));

        witcher.SetPositionAndRotation(realWitcher.position + realWitcher.up * 0f, Quaternion.Euler(0f, realWitcher.eulerAngles.y, realWitcher.eulerAngles.z));
        //witcher.Rotate(0f, 160f, 0f);
        witcher.transform.position += witcher.transform.forward * -0.1f + witcher.transform.up * 0f + witcher.transform.right * -0.3f;
        animator.SetTrigger("Kill");
        StartCoroutine(GetGrabbed());
    }

    IEnumerator GetGrabbed()
    {
        grabbing = true;
        yield return new WaitForSeconds(0.5f);
        grabbing = false;
        cameraKill.transform.parent = witcherRightHand;
        cameraKill.transform.localPosition += new Vector3(-0.3f, 0f, 0f);
        grabbed = true;
    }
	
	// Update is called once per frame
	void Update () {
        if (grabbing)
            cameraKill.transform.forward = Vector3.Lerp(cameraKill.transform.forward, (witcher.position + witcher.up * 3f) - cameraKill.transform.position, 5f * Time.deltaTime);//new Vector3(Mathf.Lerp(cameraKill.transform.eulerAngles.x, 0f, 1f * Time.deltaTime), cameraKill.transform.eulerAngles.y, cameraKill.transform.eulerAngles.z);
    }
}
