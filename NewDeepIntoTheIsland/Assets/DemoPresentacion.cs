using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DemoPresentacion : MonoBehaviour {
	public string sceneName;
	public TweenAlpha fade;
	bool loading = false;
	AsyncOperation loadScene;
	public bool autoLoad = false;

	void Update(){
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        vp_Utility.LockCursor = false;
        if (Input.GetKeyDown(KeyCode.Escape))
			Application.Quit();
	}

	void Start(){
		if(fade != null){
			//fade.GetComponent<UI2DSprite>().alpha = 1;
			//fade.PlayForward();
		}
		loading = false;
		StartCoroutine(LoadScene());
	}

	public void Play(){
		if(!loading)
			StartCoroutine(LoadWithFade());
	}

	IEnumerator LoadScene(){
		loadScene = SceneManager.LoadSceneAsync(sceneName);
		loadScene.allowSceneActivation = false;
		if(autoLoad){
			yield return new WaitForSeconds((fade != null ? fade.duration : 2f) + 3f);
			Play();
		}
		yield return loadScene;
	}

	IEnumerator LoadWithFade(){
		if(loadScene.progress >= 0.9f){
			Debug.Log("load scene");
			loading = true;
			if(fade != null)
				fade.PlayReverse();
			yield return new WaitForSeconds(fade != null ? fade.duration : 2f);
			loadScene.allowSceneActivation = true;
		}
	}
}
