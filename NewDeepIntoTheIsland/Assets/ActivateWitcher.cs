using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateWitcher : MonoBehaviour {
	public bool triggerEnabled = true;
	private Witcher witcher;
	public Witcher.SpawnPos spawnPosition;
	public Weather weather;
	public float weatherTime = 0.5f;
	public bool thunder = true;
	public bool useFixedDistance = false;
	public float fixedDistance = 8f;
	public bool chasePlayer = true;

	// Use this for initialization
	void Start () {
		witcher = GameManager.instance.witcher;
	}
	
	void OnTriggerEnter(Collider c){
		if(c.tag == "Player"){
			if(witcher == null)
				return;
			if(GameManager.instance.witcherStatus == GameManager.WitcherStatus.Hidden)
			{
				witcher.StartWatching(spawnPosition, weather, weatherTime, thunder, (useFixedDistance ? fixedDistance : -1f), chasePlayer);
				triggerEnabled = false;
			}
		}
	}
}
