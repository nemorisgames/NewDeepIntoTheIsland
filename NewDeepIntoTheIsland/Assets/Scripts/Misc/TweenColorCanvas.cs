using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TweenColorCanvas : MonoBehaviour {
    public Color startColor;
    public Color endColor;
    public float tweenTime = 1f;
    Image image;
    [HideInInspector]
    public bool running = false;
    // Use this for initialization
    void Start() {
        image = GetComponent<Image>();
        image.color = startColor;
    }

    public void StopTween()
    {
        running = false;
        StopCoroutine(colorChangeRoutine());
    }

    public void StartTween()
    {
        running = true;
        StartCoroutine(colorChangeRoutine());
    }
    
    IEnumerator colorChangeRoutine() {
        bool forward = true;
        while (running)
        {
            print("starting " + image.color);
            if(forward)
                image.CrossFadeColor(endColor, tweenTime, false, false);
            else
                image.CrossFadeColor(startColor, tweenTime, false, false);
            yield return new WaitForSeconds(tweenTime);
            forward = !forward;
            print("finishing " + image.color);
        }

    }
	
	// Update is called once per frame
	void Update () {
	}
}
