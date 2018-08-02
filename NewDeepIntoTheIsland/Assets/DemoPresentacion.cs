using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DemoPresentacion : MonoBehaviour {
	public string sceneName;
	public TweenAlpha fade;
	bool loading = false;

	void Update(){
		if(Input.GetKeyDown(KeyCode.Escape))
			Application.Quit();
	}

	void Start(){
		fade.GetComponent<UI2DSprite>().alpha = 1;
	}

	public void Play(){
		if(!loading)
			StartCoroutine(LoadWithFade());
	}

	IEnumerator LoadWithFade(){
		loading = true;
		fade.PlayReverse();
		yield return new WaitForSeconds(2f);
		SceneManager.LoadScene(sceneName);
	}
}
