using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenu : MonoBehaviour {
	public static GameMenu Instance {get{return _instance;}}
	static GameMenu _instance;
	public bool menuActive = false;
	CanvasGroup panel;
	bool canToggle = true;
	vp_FPController player;
	vp_FPInput playerInput;
	AudioSource playerAudio, witcherAudio;
	Witcher witcher;
	float playerSpeed;

	// Use this for initialization
	void Awake () {
		panel = GetComponent<CanvasGroup>();
		_instance = this;
	}

	void Start(){
		player = GameManager.instance.player.GetComponent<vp_FPController>();
		playerSpeed = player.MotorAcceleration;
		playerAudio = AudioManager.Instance.voiceSource;
		witcher = GameManager.instance.witcher;
		witcherAudio = witcher.GetComponent<AudioSource>();
		playerInput = player.GetComponent<vp_FPInput>();
		panel.alpha = 0;
		StartCoroutine(autoHideCursor());
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.Escape)){
			EnableMenu(!menuActive);
			if(!menuActive){
				StartCoroutine(autoHideCursor());
			}
		}
	}

	public void Resume(){
        if (!menuActive) return;
		Debug.Log("Resume");
		EnableMenu(false);
	}

	public void Restart()
    {
        if (!menuActive) return;
        vp_Utility.LockCursor = true;
		playerInput.MouseLookSensitivity = new Vector2(0,0);
		loadScene.allowSceneActivation = true;
	}

	IEnumerator autoHideCursor(){
		yield return new WaitForSeconds(0.25f);
		if(!vp_Utility.LockCursor)
			vp_Utility.LockCursor = true;
	}

	public void Quit()
    {
        if (!menuActive) return;
        Application.Quit();
	}

	AsyncOperation loadScene;

	IEnumerator LoadScene(){
		loadScene = SceneManager.LoadSceneAsync("SceneExterior");
		loadScene.allowSceneActivation = false;
		yield return loadScene;
	}

	public void EnableMenu(bool b){
		Debug.Log("menu "+b);
		if(!canToggle || GameManager.instance.playerStatus == GameManager.PlayerStatus.Dead)
			return;
		if(loadScene == null)
			StartCoroutine(LoadScene());
		//show cursor
		playerInput.MouseCursorForced = b;
		canToggle = false;
		menuActive = b;

		//Fade in menu
		StartCoroutine(FadeIn(b));

		//Guardar celular
		if(CellPhone.phoneUp)
			CellPhone.Instance.TogglePhone();

		//Pausar movimiento
		player.MotorAcceleration = b ? 0 : playerSpeed;

		//Pausar audio
		if(b && playerAudio.isPlaying)
			playerAudio.Pause();
		else if(!b)
			playerAudio.UnPause();
		
		//Pausar brujo
		witcher.Pause(b);
		if(b)
			witcherAudio.Pause();
		else
			witcherAudio.UnPause();

		//Pausar mensajes ui
		ShowMessageOnScreen.Instance.Pause(b);
		//hide cursor
		if(!b)
			vp_Utility.LockCursor = true;
	}

	IEnumerator FadeIn(bool b){
		int sign = 1;
		if(!b)
			sign = -1;
		
		float div = 10;
		for(float i = 0; i < 0.5f; i+= 0.5f/div){
            panel.alpha += sign*1/div;
            yield return new WaitForSeconds(0.5f/div);
        }

		if(b)
			panel.alpha = 1;
		else
			panel.alpha = 0;

		canToggle = true;
	}
}
