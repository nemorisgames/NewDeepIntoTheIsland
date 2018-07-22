
using UnityEngine;
using AxelF;
using Gameplay;

public class CameraAudioTransformsUpdater : MonoBehaviour
{
    protected void Awake()
    {
        var gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();
        if (gameController)
        GameManager.instance.audioTransformsUpdater = (player, eye) => {
            Heartbeat.playerTransform = player;
            Heartbeat.listenerTransform = eye;
        };
    }
}

