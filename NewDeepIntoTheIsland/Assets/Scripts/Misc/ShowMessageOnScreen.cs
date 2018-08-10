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
    public CanvasGroup panel;
    bool fading = false;
    public float pauseTime = 0f;
    public bool showingMessage = false;
	// Use this for initialization
	void Start () {
        Instance = this;
        //background.SetActive(false);
    }
	public void SetText(string message)
    {
        /*text.text = message;
        currentTimeOnScreen = Time.time + timeOnScreen;
        background.SetActive(true);*/
        StartCoroutine(FadeIn(message));
    }
	// Update is called once per frame
	void Update () {
        if (text.text != "" && currentTimeOnScreen > 0f && currentTimeOnScreen <= Time.time && !GameMenu.Instance.menuActive)
        {
            //text.text = "";
            //background.SetActive(false);
            StartCoroutine(FadeOut());
        }
	}

    IEnumerator FadeIn(string message){
        if(fading)
            yield break;
        fading = true;
        showingMessage = true;
        text.text = message;

        float div = 10;
        if(panel.alpha != 1){
            for(float i = 0; i < 0.5f; i+= 0.5f/div){
                panel.alpha += 1/div;
                yield return new WaitForSeconds(0.5f/div);
            }
        }
        
        currentTimeOnScreen = Time.time + timeOnScreen;
        //background.SetActive(true);

        fading = false;
    }

    IEnumerator FadeOut(){
        if(fading)
            yield break;
        fading = true;

        float div = 10;
        if(panel.alpha != 0){
            for(float i = 0; i < 0.5f; i+= 0.5f/div){
                panel.alpha -= 1/div;
                yield return new WaitForSeconds(0.5f/div);
            }
        }

        text.text = "";
        //background.SetActive(false);

        fading = false;
        showingMessage = false;
    }

    float panelAlphaAux = 0f;
    public void Pause(bool b){
        if(!showingMessage)
            return;
        if(b){
            pauseTime = Time.time;
            panelAlphaAux = panel.alpha;
            panel.alpha = 0f;
        }
        else{
            pauseTime = Time.time - pauseTime;
            currentTimeOnScreen += pauseTime;
            panel.alpha = panelAlphaAux;
        }
    }
}
