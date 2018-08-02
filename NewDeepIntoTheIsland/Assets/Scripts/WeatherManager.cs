using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public enum Weather
{
    RainNone,
	RainLow,
	RainNormal,
	RainHeavy,
	RainStorm,
    RainInside
}
public enum WeatherEffects
{
	Flash,
	Thunder,
}
public class WeatherManager : MonoBehaviour
{
	//public bool debug = false;
	public static WeatherManager Instance = null;
	public ParticleSystem particles;
	public ParticleSystem rainSheet;
	GameObject rainSoundGO;
	[HideInInspector]
	public AudioSource source;
	[HideInInspector]
	public AudioSource source_second;
	public AudioClip[] weather_Sounds;
	//public AudioClip[] weather_effects;
	public GameObject thunderSpawns;
	public AudioSource[] thunderSounds;
	Weather currentWeather = Weather.RainNone;
    public GameObject player;

	void Awake()
	{
        Instance = this;
        player = GameObject.FindGameObjectWithTag("Player");
        rainSoundGO = new GameObject();
        rainSoundGO.name = "WeatherSound";
        rainSoundGO.transform.parent = player.transform;

		if (source == null)
		{
			if (rainSoundGO.GetComponent<AudioSource>() == null)
			{
				rainSoundGO.AddComponent<AudioSource>();
				rainSoundGO.AddComponent<AudioSource>();
			}
			AudioSource[] sources = rainSoundGO.GetComponents<AudioSource>();
			source = sources[0];
			source.volume = 0;
			source_second = sources[1];
			source_second.volume = 0;
		}
		//thunderSounds = thunderSpawns.GetComponentsInChildren<AudioSource>();
	}

	IEnumerator SmoothAudioChange(float time, float objectiveVolume, Weather weatherSound)
	{
		AudioSource one;
		AudioSource two;
		if (source.isPlaying)
		{
			one = source;
			two = source_second;
		}
		else
		{
			two = source;
			one = source_second;
		}
        switch (weatherSound)
        {
            case Weather.RainNone:
                two.clip = null;
                break;
            case Weather.RainLow:
                two.clip = weather_Sounds[0];
                break;
            case Weather.RainNormal:
                two.clip = weather_Sounds[1];
                break;
            case Weather.RainHeavy:
                two.clip = weather_Sounds[2];
                break;
            case Weather.RainStorm:
                two.clip = weather_Sounds[3];
                break;
            case Weather.RainInside:
                two.clip = weather_Sounds[4];
                break;
        }
		two.Play();
		two.loop = true;
		two.volume = 0f;
		int steps = 10;
		if (time == 0) time = steps;
		float volumeStep = (((objectiveVolume)) / steps);
		Debug.Log(volumeStep);
		for (int i = 0; i < steps; i++)
		{
			yield return new WaitForSeconds(time / steps);
			two.volume += volumeStep;
			one.volume -= volumeStep;
		}
		two.volume = objectiveVolume;
		one.Stop();
		one.volume = objectiveVolume;
	}
	IEnumerator SmoothWeatherChange(float time, float objectiveRate, float objectiveRateSheet)
	{
        /*
		var em = particles.emission;
		var rate = new ParticleSystem.MinMaxCurve();
		var emSheet = rainSheet.emission;
		var rateSheet = new ParticleSystem.MinMaxCurve();
		rateSheet.mode = ParticleSystemCurveMode.Constant;
		rate.mode = ParticleSystemCurveMode.Constant;*/
        //rate = em.rateOverTime;
        //rateSheet = emSheet.rateOverTime;

        ParticleSystemRenderer p = particles.GetComponent<ParticleSystemRenderer>();
        float currentRate = p.material.GetColor("_TintColor").a;
        ParticleSystemRenderer r = rainSheet.GetComponent<ParticleSystemRenderer>();
        float currentRateSheet = r.material.GetColor("_TintColor").a;
        //particles.
        float steps = 100;// Mathf.Abs(((objectiveRate - currentRate)) / time);
		//float sheetStep = ((objectiveRateSheet - currentRateSheet) / steps);
		for (int i = 0; i < steps; i++)
		{
			yield return new WaitForSeconds(time / steps);
            /*
			rate.constant += ((objectiveRate - currentRate)) / steps;
			rateSheet.constant += sheetStep;
			em.rateOverTime = rate;
			emSheet.rateOverTime = rateSheet;*/
            p.material.SetColor("_TintColor", new Color(0.5f, 0.5f, 0.5f, currentRate + i * ((objectiveRate - currentRate)) / steps));
            r.material.SetColor("_TintColor", new Color(0.5f, 0.5f, 0.5f, currentRateSheet + i * ((objectiveRateSheet - currentRateSheet)) / steps));
        }
    }

	public void StartNewWeather(Weather newWeather, float time = 5f)
	{
		print("weather " + currentWeather);
		currentWeather = newWeather;
		Debug.Log("Changing weather to " + currentWeather);
		if (particles != null)
		{
			StopCoroutine("SmoothWeatherChange");
			//RemoveOldWeather();
			switch (currentWeather)
            {
                case Weather.RainNone:
                    StartCoroutine(SmoothWeatherChange(time, 0f, 0f));
                    StartCoroutine(SmoothAudioChange(time, 0.5f, currentWeather));
                    //Debug.Log ("Low Rain");
                    break;
                case Weather.RainLow:
					StartCoroutine(SmoothWeatherChange(time, 0.15f, 0.1f));
					StartCoroutine(SmoothAudioChange(time, 0.5f, currentWeather));
					//Debug.Log ("Low Rain");
					break;
				case Weather.RainNormal:
					StartCoroutine(SmoothWeatherChange(time, 0.25f, 0.25f));
					StartCoroutine(SmoothAudioChange(time, 0.5f, currentWeather));
					//Debug.Log ("Normal Rain");
					break;
				case Weather.RainHeavy:
					StartCoroutine(SmoothWeatherChange(time, 0.5f, 0.4f));
					StartCoroutine(SmoothAudioChange(time, 0.75f, currentWeather));
					//Debug.Log ("Heavy Rain");
					break;
				case Weather.RainStorm:
					StartCoroutine(SmoothWeatherChange(time, 0.7f, 0.5f));
					StartCoroutine(SmoothAudioChange(time, 0.75f, currentWeather));
					ThunderAndFlash();
					//Debug.Log ("Thunder");
					break;
				case Weather.RainInside:
                    //lluvia interior
                    StartCoroutine(SmoothAudioChange(time, 0.75f, currentWeather));
                    break;
				default:
					break;
			}
		}
	}
	IEnumerator FlashEffect(int pos)
	{
		thunderSounds[pos].transform.GetChild(0).gameObject.SetActive(true);
		yield return new WaitForSeconds(0.2f);
		thunderSounds[pos].transform.GetChild(0).gameObject.SetActive(false);
		yield return new WaitForSeconds(0.1f);
		thunderSounds[pos].transform.GetChild(0).gameObject.SetActive(true);
		yield return new WaitForSeconds(0.5f);
		thunderSounds[pos].transform.GetChild(0).gameObject.SetActive(false);
	}

	IEnumerator ThunderSound(ulong time, int pos, float volume)
	{
		yield return new WaitForSeconds(time);
		thunderSounds[pos].Play();
		Debug.Log("ThunderSound");
	}

	public void ThunderAndFlash(int pos = -1)
	{
		if (pos == -1)
			pos = Random.Range(0, thunderSounds.Length);
		//play lightning visual effect
		StartCoroutine(FlashEffect(pos));
		//calculate time based on distance of lightning to Cara
		float dist = Vector3.Distance(GameObject.FindGameObjectWithTag("Player").transform.position, thunderSounds[pos].transform.position);
		float time = Vector3.Distance(thunderSounds[pos].transform.position, GameObject.FindWithTag("Player").transform.position) * 0.05f;
		StartCoroutine(ThunderSound((ulong)time, pos, 1f));
	}
	/*void NextWeather()
	{
		currentWeather++;
		if (currentWeather > 3)
		{
			currentWeather = 0;
		}
		StartNewWeather();
	}
	void LastWeather()
	{
		currentWeather--;
		if (currentWeather < 0)
			currentWeather = 2;
		StartNewWeather();
	}*/
	void Update()
	{
        particles.transform.position = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);
        //for debuging
        /*if (!debug)
			return;
		if (Input.GetAxis("Mouse ScrollWheel") != 0f)
		{
			if (Input.GetAxis("Mouse ScrollWheel") < 0f)
			{
				NextWeather();
			}
			if (Input.GetAxis("Mouse ScrollWheel") > 0f)
			{
				LastWeather();
			}
		}*/
    }
}
