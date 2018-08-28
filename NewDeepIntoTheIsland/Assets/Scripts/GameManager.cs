using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    [Header("General Settings")]
    public static GameManager instance = null;
    public enum PlayerStatus {Normal, Dead};
    public enum GameStatus { Normal, Begin, Paused, End, Menu };
    public delegate void AudioTransformsUpdater(Transform root, Transform eye);
    public AudioTransformsUpdater audioTransformsUpdater;
    [HideInInspector]
    public PlayerStatus playerStatus = PlayerStatus.Normal;
    [HideInInspector]
    public GameStatus gameStatus = GameStatus.Normal;
    [HideInInspector]
    public vp_FPInput playerInput;
    [HideInInspector]
    public vp_FPController playerController;
    public float runEnergyDeplededSpeed = 30f;
    [HideInInspector]
    public float currentRunEnergy = 100f;
    [HideInInspector]
    public bool runEnabled = true; //value readed by vp_FPInput when the run button is pressed
    [HideInInspector]
    public GameObject player;
    [Header("Battery Settings")]
    [HideInInspector]
    public bool batteryDepleded = false;
    [Header("Light Settings")]
    public bool threatActivated = true;
    [HideInInspector]
    public bool lightActivated = false;
    public float timeUntilDarknessKill = 30f;
    float currentTimeUntilDarknessKill = 0f;
    public PostProcessVolume postProcessVolume;
    ColorGrading colorGrading;
    Vector2 exposureRange = new Vector2(-1f, 1.95f);

    public enum WitcherStatus { Hidden, Watching, Chasing, Hiding };
    [Header("Witcher Settings")]
    //[HideInInspector]
    public WitcherStatus witcherStatus = WitcherStatus.Hidden;
    public KilledByWitcher killedByWitcher;
    [HideInInspector]
    public Witcher witcher;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            if (instance != null)
            {
                Destroy(gameObject);
            }
        }
        postProcessVolume.profile.TryGetSettings(out colorGrading);
        player = GameObject.FindWithTag("Player");
        GameObject w = GameObject.FindWithTag("Witcher");
        if(w != null)
            witcher = w.GetComponent<Witcher>();
        playerInput = player.GetComponent<vp_FPInput>();
        playerController = player.GetComponent<vp_FPController>();
        if (audioTransformsUpdater != null)
            audioTransformsUpdater(player.transform, Camera.main.transform);
        currentRunEnergy = 100f;
    }

    public void KillPlayer()
    {
        if(GameMenu.Instance.menuActive)
            GameMenu.Instance.EnableMenu(false);
        ScreenManager.Instance.CloseScreen();
        playerStatus = PlayerStatus.Dead;
        playerInput.enabled = false;
        killedByWitcher.Kill();
        StartCoroutine(DarkenSceneRoutine(5f));
    }

    public void DarkenScene(float time = 0.5f)
    {
        StartCoroutine(DarkenSceneRoutine(time));
    }

    IEnumerator DarkenSceneRoutine(float time = 0.5f)
    {
        yield return new WaitForSeconds(time);
        for (int i = 0; i < 100; i++)
        {
            yield return new WaitForSeconds(0.01f);
            colorGrading.postExposure.value -= 0.05f;
        }
        print("finish");
        ShowGameOver();
    }

    public void ShowGameOver()
    {
        SceneManager.LoadScene("GameOver");
    }

    public void ShowAccomplished()
    {
        SceneManager.LoadScene("Accomplished");
    }

    IEnumerator TiredFromRunning()
    {
        runEnabled = false;
        for (int i = 0; i < 100; i++)
        {
            yield return new WaitForSeconds(5f / 100f);
            currentRunEnergy += 1f;
        }
        //yield return new WaitForSeconds(5f);
        runEnabled = true;
        currentRunEnergy = 100f;
    }

    // Update is called once per frame
    void Update () {
        if (playerController.StateEnabled("Run"))
        {
            if (runEnabled)
            {
                currentRunEnergy -= Time.deltaTime * runEnergyDeplededSpeed;
                if (currentRunEnergy <= 0f)
                {
                    StartCoroutine(TiredFromRunning());
                }
            }
        }
        else
            if(runEnabled)
                currentRunEnergy = Mathf.Clamp(currentRunEnergy + Time.deltaTime * runEnergyDeplededSpeed * 0.5f, 0f, 100f);
        if (threatActivated)
        {
            if (currentTimeUntilDarknessKill < timeUntilDarknessKill)
            {
                currentTimeUntilDarknessKill = Mathf.Clamp(currentTimeUntilDarknessKill + (lightActivated ? -1f : 1f) * Time.deltaTime, 0f, timeUntilDarknessKill);
                colorGrading.postExposure.value = (exposureRange.y - (exposureRange.y - exposureRange.x) * (currentTimeUntilDarknessKill / timeUntilDarknessKill));
            }
            else KillPlayer();
        }
    }
}

[System.Serializable]
public class CallbackObject
{
    public GameObject objective;
    public string callback;

    public void Call()
    {
        this.objective.SendMessage(callback);
    }
}

