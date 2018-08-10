using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Basilisco : MonoBehaviour {
    Animator animator;
    public TweenPosition tweenPosition;
    public KilledByWitcher killed;
    vp_FPInput input;
    Transform cam;
    public Transform referencePoint;
    bool attacking = false;
    public AudioClip scream;
    // Use this for initialization
    void Start () {
        animator = GetComponent<Animator>();
        input = GameObject.FindWithTag("Player").GetComponent<vp_FPInput>();
        cam = Camera.main.transform;// GameObject.FindWithTag("Player").GetComponent<vp_FPCamera>();
    }

    public void BasiliscScream()
    {
        AudioManager.Instance.PlayOneShot(scream);
    }

    public void Attack()
    {
        animator.SetTrigger("Attack");
        //cam = Camera.main.transform;
        transform.LookAt(referencePoint);

        Vector3 directionFromPlayerToBasilisco = referencePoint.position - transform.position;
        directionFromPlayerToBasilisco.Normalize();
        tweenPosition.from = transform.position;
        tweenPosition.to = referencePoint.position - directionFromPlayerToBasilisco * 1.7f - Vector3.up * 0.5f;
        tweenPosition.PlayForward();

        killed.cameraKill.transform.SetPositionAndRotation(cam.position, cam.rotation);
        attacking = true;

        GameObject.FindWithTag("Player").gameObject.SetActive(false);
        killed.cameraKill.gameObject.SetActive(true);
    }

    public void AttackFinished()
    {
        cam.SetPositionAndRotation(referencePoint.position, referencePoint.rotation);
        killed.Kill();
        GameManager.instance.DarkenScene(2f);
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (attacking) {
            killed.cameraKill.transform.position = Vector3.Lerp(killed.cameraKill.transform.position, referencePoint.position, 5f * Time.deltaTime);
            
            killed.cameraKill.transform.position = Vector3.MoveTowards(killed.cameraKill.transform.position, referencePoint.position, 1f * Time.deltaTime);
            /*Vector3 direction = transform.position - killed.cameraKill.transform.position;
            Quaternion toRotation = Quaternion.FromToRotation(killed.cameraKill.transform.forward, direction);
            killed.cameraKill.transform.rotation = Quaternion.Lerp(killed.cameraKill.transform.rotation, toRotation, 1f * Time.time);*/
            killed.cameraKill.transform.rotation = Quaternion.RotateTowards(killed.cameraKill.transform.rotation, referencePoint.rotation, 300f * Time.deltaTime);
        }
    }
}
