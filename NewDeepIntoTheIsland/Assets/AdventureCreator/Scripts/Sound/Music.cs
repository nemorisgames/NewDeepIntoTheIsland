/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"Music.cs"
 * 
 *	This script handles the playback of Music when played using the 'Sound: Play music' Action.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

namespace AC
{

	/**
	 * This script handles the playback of Music when played using the 'Sound: Play music' Action.
	 */
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_music.html")]
	#endif
	public class Music : Soundtrack
	{

		protected new void Awake ()
		{
			soundType = SoundType.Music;
			playWhilePaused = KickStarter.settingsManager.playMusicWhilePaused;
			base.Awake ();
		}


		protected override List<MusicStorage> Storages
		{
			get
			{
				return KickStarter.settingsManager.musicStorages;
			}
		}


		public override MainData SaveMainData (MainData mainData)
		{
			mainData.lastMusicQueueData = CreateLastSoundtrackString ();
			mainData.musicQueueData = CreateTimesampleString ();

			mainData.musicTimeSamples = 0;
			mainData.lastMusicTimeSamples = LastTimeSamples;

			if (GetCurrentTrackID () >= 0)
			{
				MusicStorage musicStorage = GetSoundtrack (GetCurrentTrackID ());
				if (musicStorage != null && musicStorage.audioClip != null && audioSource.clip == musicStorage.audioClip && audioSource.isPlaying)
				{
					mainData.musicTimeSamples = audioSource.timeSamples;
				}
			}

			mainData.oldMusicTimeSamples = CreateOldTimesampleString ();

			return mainData;
		}


		public override void LoadMainData (MainData mainData)
		{
			LoadMainData (mainData.musicTimeSamples, mainData.oldMusicTimeSamples, mainData.lastMusicTimeSamples, mainData.lastMusicQueueData, mainData.musicQueueData);
		}


		protected override bool EndsOthers ()
		{
			return true;
		}

	}

}