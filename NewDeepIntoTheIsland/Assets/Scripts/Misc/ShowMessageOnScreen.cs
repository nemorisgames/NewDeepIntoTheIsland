using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowMessageOnScreen : MonoBehaviour {
    public static ShowMessageOnScreen Instance = null;
    public Text text;
    public GameObject background;
    public float timeOnScreen = 4f;
    float currentTimeOnScreen;
	// Use this for initialization
	void Start () {
        Instance = this;
        background.SetActive(false);
    }
	public void SetText(string message)
    {
        text.text = message;
        currentTimeOnScreen = Time.time + timeOnScreen;
        background.SetActive(true);
    }
	// Update is called once per frame
	void Update () {
        if (text.text != "" && currentTimeOnScreen > 0f && currentTimeOnScreen <= Time.time)
        {
            text.text = "";
            background.SetActive(false);
        }
	}
}
