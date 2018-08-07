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
        if (audioTransformsUpdater != null)
            audioTransformsUpdater(player.transform, Camera.main.transform);
    }

    public void KillPlayer()
    {
        ScreenManager.Instance.CloseScreen();
        playerStatus = PlayerStatus.Dead;
        playerInput.enabled = false;
        killedByWitcher.Kill();
        StartCoroutine(DarkenScene());
    }

    IEnumerator DarkenScene()
    {
        yield return new WaitForSeconds(5f);
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


    // Update is called once per frame
    void Update () {
        if (playerStatus == PlayerStatus.Dead) return;
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

