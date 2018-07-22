﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class GameManager : MonoBehaviour {
    [Header("General Settings")]
    public static GameManager instance = null;
    public enum PlayerStatus {Normal, Dead};
    public enum GameStatus { Normal, Begin, Paused, End, Menu };
    [HideInInspector]
    public PlayerStatus playerStatus = PlayerStatus.Normal;
    [HideInInspector]
    public GameStatus gameStatus = GameStatus.Normal;
    [HideInInspector]
    public vp_FPInput playerInput;
    [Header("Battery Settings")]
    [HideInInspector]
    public bool batteryDepleded = false;
    [Header("Light Settings")]
    [HideInInspector]
    public bool lightActivated = false;
    public float timeUntilDarknessKill = 30f;
    float currentTimeUntilDarknessKill = 0f;
    public PostProcessVolume postProcessVolume;
    ColorGrading colorGrading;
    Vector2 exposureRange = new Vector2(-1f, 1.95f);
    public delegate void AudioTransformsUpdater(Transform root, Transform eye);
    public AudioTransformsUpdater audioTransformsUpdater;

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
        GameObject player = GameObject.FindWithTag("Player");
        playerInput = player.GetComponent<vp_FPInput>();
        if (audioTransformsUpdater != null)
            audioTransformsUpdater(player.transform, Camera.main.transform);
    }

    // Update is called once per frame
    void Update () {
        if (playerStatus == PlayerStatus.Dead) return;
        if (currentTimeUntilDarknessKill < timeUntilDarknessKill)
        {
            currentTimeUntilDarknessKill = Mathf.Clamp(currentTimeUntilDarknessKill + (lightActivated ? -1f : 1f) * Time.deltaTime, 0f, timeUntilDarknessKill);
            colorGrading.postExposure.value = (exposureRange.y - (exposureRange.y - exposureRange.x) * (currentTimeUntilDarknessKill / timeUntilDarknessKill));
        }
        else playerStatus = PlayerStatus.Dead;
    }
}

