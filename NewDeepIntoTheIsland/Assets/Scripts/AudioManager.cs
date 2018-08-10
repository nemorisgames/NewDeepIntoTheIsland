using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
	public static AudioManager Instance = null;
    public AudioClip[] CaraBreathingLevels;
    public AudioMixerGroup breathMixer;

    public AudioClip[] backgrounds;
    public AudioMixerGroup ambientMixer;
	public AudioClip[] voices;
    public AudioMixerGroup voicesMixer;
    public AudioClip[] situation;
	//public AudioClip[] effects;
    public AudioMixerGroup situationMixer;
    
    AudioSource bgSource;
	AudioSource bgSource_second;
	AudioSource voiceSource;
	AudioSource breathSource;
	//public AudioSource efSource;
	//public AudioSource efSource_second;
	AudioSource situationSource;
	AudioSource situationSource_second;
	private List<AudioSource> playingloop;

	private AudioSource one;
	private AudioSource two;

	[SerializeField]
	private float volume = 1f;
	void Awake()
	{
		Instance = this;
		playingloop = new List<AudioSource>();
		if (GameObject.FindGameObjectWithTag("AudioManager").GetComponents<AudioSource>() == null)
		{
			bgSource = GameObject.FindGameObjectWithTag("AudioManager").AddComponent<AudioSource>();
            bgSource.outputAudioMixerGroup = ambientMixer;
			bgSource.playOnAwake = false;
			voiceSource = GameObject.FindGameObjectWithTag("AudioManager").AddComponent<AudioSource>();
            voiceSource.outputAudioMixerGroup = voicesMixer;
            voiceSource.clip = CaraBreathingLevels[0];
            voiceSource.playOnAwake = true;
            voiceSource.loop = true;
            voiceSource.spatialBlend = 0f;
			//efSource = GameObject.FindGameObjectWithTag("AudioManager").AddComponent<AudioSource>();
			//efSource.playOnAwake = false;
			situationSource = GameObject.FindGameObjectWithTag("AudioManager").AddComponent<AudioSource>();
            situationSource.outputAudioMixerGroup = situationMixer;
            situationSource.playOnAwake = false;


			bgSource_second = GameObject.FindGameObjectWithTag("AudioManager").AddComponent<AudioSource>();
            bgSource_second.outputAudioMixerGroup = ambientMixer;
            bgSource_second.playOnAwake = false;
			breathSource = GameObject.FindGameObjectWithTag("AudioManager").AddComponent<AudioSource>();
            breathSource.outputAudioMixerGroup = breathMixer;
            breathSource.playOnAwake = false;
            voiceSource.spatialBlend = 1f;
            //efSource_second = GameObject.FindGameObjectWithTag("AudioManager").AddComponent<AudioSource>();
            //efSource_second.playOnAwake = false;
            situationSource_second = GameObject.FindGameObjectWithTag("AudioManager").AddComponent<AudioSource>();
            situationSource_second.outputAudioMixerGroup = situationMixer;
            situationSource_second.playOnAwake = false;
		}
		else
		{
			AudioSource[] sources = GameObject.FindGameObjectWithTag("AudioManager").GetComponents<AudioSource>();
			for (int i = 0; i < 8 - sources.Length; i++)
			{
				GameObject.FindGameObjectWithTag("AudioManager").AddComponent<AudioSource>();
			}
			sources = GameObject.FindGameObjectWithTag("AudioManager").GetComponents<AudioSource>();
			bgSource = sources[0];
			bgSource.playOnAwake = false;
            bgSource.outputAudioMixerGroup = ambientMixer;
            voiceSource = sources[1];
            voiceSource.playOnAwake = true;
            voiceSource.loop = true;
            voiceSource.Play();
            voiceSource.outputAudioMixerGroup = voicesMixer;
            //efSource = sources[2];
            //efSource.playOnAwake = false;
            situationSource = sources[3];
			situationSource.playOnAwake = false;
            situationSource.outputAudioMixerGroup = situationMixer;

            bgSource_second = sources[4];
			bgSource_second.playOnAwake = false;
            bgSource_second.outputAudioMixerGroup = ambientMixer;
            breathSource = sources[5];
			breathSource.playOnAwake = false;
            breathSource.outputAudioMixerGroup = breathMixer;
            //efSource_second = sources[6];
            //efSource_second.playOnAwake = false;
            situationSource_second = sources[7];
			situationSource_second.playOnAwake = false;
            situationSource_second.outputAudioMixerGroup = situationMixer;
        }

        PlayAmbientMusic(0);
        PlayBreath(0);

    }

    public void PlayOneShot(AudioClip audioClip)
    {
        situationSource.PlayOneShot(audioClip);
    }

    public void PlayVoice(int i, float volume = 0)
	{
		if (volume != 0)
			voiceSource.PlayOneShot(voices[i], volume);
		else
			voiceSource.PlayOneShot(voices[i]);
	}
	public void PlayAmbientMusic(int i, float time = 1f)
	{
		StopCoroutine("FadeAmbient");
		StartCoroutine(FadeAmbient(time, i));
	}
	public void PlaySituationMusic(int i, float time = 1f)
	{
		StopCoroutine("FadeSituation");
		StartCoroutine(FadeSituacion(time, i));
	}
	public void PlayOnlySituation(int i, float time = 1f)
	{
		StopCoroutine("FadeSituation");
		StartCoroutine(FadeSituacion(time, i, false));
	}
	public void PlayOnlyAmbient(int i, float time = 1f)
	{
		StopCoroutine("FadeAmbient");
		StartCoroutine(FadeAmbient(time, i, false));
	}
	IEnumerator FadeAll(float time)
	{
		int steps = 10;
		if (time == 0) time = steps;
		float timeStep = time / steps * 1.0f;
		float volumeStep = volume / (steps);
		for (int i = 0; i < steps; i++)
		{
			yield return new WaitForSeconds(timeStep);
			situationSource.volume -= volumeStep;
			bgSource_second.volume -= volumeStep;
			situationSource_second.volume -= volumeStep;
			bgSource.volume -= volumeStep;
		}
		situationSource.Stop();
		bgSource.Stop();
		situationSource.volume = volume;
		bgSource.volume = volume;
		situationSource_second.Stop();
		bgSource_second.Stop();
		situationSource_second.volume = volume;
		bgSource_second.volume = volume;
	}
    
    public void PlayBreath(int severity)
    {
        if (breathSource.clip != CaraBreathingLevels[severity])
        {
            breathSource.clip = CaraBreathingLevels[severity];
            breathSource.Play();
        }
    }

    public void StopBreath()
    {
        if (breathSource.isPlaying)
        {
            breathSource.clip = null;
            breathSource.Stop();
        }
    }

    IEnumerator FadeSituacion(float time, int pos, bool playAnother = true)
	{
		if (situationSource.isPlaying)
		{
			one = situationSource;
			two = situationSource_second;
		}
		else
		{
			two = situationSource;
			one = situationSource_second;
		}
		two.clip = situation[pos];
		two.Play();
		two.loop = true;
		int steps = 10;
		if (time == 0) time = steps;
		float timeStep = time / steps * 1.0f;
		float volumeStep = volume / (steps);
		two.volume = 0;
		if (playAnother)
		{
			for (int i = 0; i < steps; i++)
			{
				yield return new WaitForSeconds(timeStep);
				one.volume -= volumeStep;
				two.volume += volumeStep;
			}
		}
		else
			yield return (FadeAll(time));
		one.Stop();
		one.volume = volume;
		/*
		for (int i = 0; i < steps; i++)
		{
			yield return new WaitForSeconds(timeStep);
			
		}*/
	}
	IEnumerator FadeAmbient(float time, int pos, bool playAnother = true)
	{
		if (bgSource.isPlaying)
		{
			//Debug.Log("yes");
			one = bgSource;
			two = bgSource_second;
		}
		else
		{
			//Debug.Log("NO");
			two = bgSource;
			one = bgSource_second;
		}
		two.clip = backgrounds[pos];
		two.Play();
		two.loop = true;
		int steps = 10;
		if (time == 0) time = steps;
		float timeStep = time / steps * 1.0f;
		float volumeStep = volume / (steps);
		Debug.Log("volume step: " + volumeStep);
		two.volume = 0;
		if (playAnother)
		{
			for (int i = 0; i < steps; i++)
			{
				yield return new WaitForSeconds(timeStep);
				one.volume -= volumeStep;
				two.volume += volumeStep;
				//Debug.Log("one volume " + one.volume + " two volume " + two.volume/* + " sources: " + one.clip != null ? one.clip.name : "nope" + " " + two.clip != null ? two.clip.name : "nope"*/);
			}
		}
		else
			yield return (FadeAll(time));
		one.Stop();
		one.volume = volume;
	}
	IEnumerator Demo()
	{
		PlayAmbientMusic(6, 0f);
		//PlayAmbientMusic(0);
		yield return new WaitForSeconds(10f);
		Debug.Log("Switching to new music");
		PlayAmbientMusic(2, 5f);
		//PlaySituationMusic(2);
		//yield return new WaitForSeconds(2f);
		//CellPhone.Instance.SendMessage("ShowNotification");
	}

    private void Update()
    {
        //print(GameObject.FindWithTag("Player").GetComponent<vp_FPController>().);

        if (vp_Input.GetButtonDown("Run"))
        {
            PlayBreath(1);
        }
        if (vp_Input.GetButtonUp("Run"))
        {
            PlayBreath(0);
            //StopBreath();
        }
    }
}
