/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"Ambience.cs"
 * 
 *	This script handles the playback of Ambience when played using the 'Sound: Play ambience' Action.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

namespace AC
{

	/**
	 * This script handles the playback of Ambience when played using the 'Sound: Play ambience' Action.
	 */
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_ambience.html")]
	#endif
	public class Ambience : Soundtrack
	{

		protected new void Awake ()
		{
			soundType = SoundType.SFX;
			playWhilePaused = false;
			base.Awake ();
		}


		protected override List<MusicStorage> Storages
		{
			get
			{
				return KickStarter.settingsManager.ambienceStorages;
			}
		}


		public override MainData SaveMainData (MainData mainData)
		{
			mainData.lastAmbienceQueueData = CreateLastSoundtrackString ();
			mainData.ambienceQueueData = CreateTimesampleString ();

			mainData.ambienceTimeSamples = 0;
			mainData.lastAmbienceTimeSamples = LastTimeSamples;

			if (GetCurrentTrackID () >= 0)
			{
				MusicStorage musicStorage = GetSoundtrack (GetCurrentTrackID ());
				if (musicStorage != null && musicStorage.audioClip != null && audioSource.clip == musicStorage.audioClip && audioSource.isPlaying)
				{
					mainData.ambienceTimeSamples = audioSource.timeSamples;
				}
			}

			mainData.oldAmbienceTimeSamples = CreateOldTimesampleString ();

			return mainData;
		}


		public override void LoadMainData (MainData mainData)
		{
			LoadMainData (mainData.ambienceTimeSamples, mainData.oldAmbienceTimeSamples, mainData.lastAmbienceTimeSamples, mainData.lastAmbienceQueueData, mainData.ambienceQueueData);
		}

	}

}