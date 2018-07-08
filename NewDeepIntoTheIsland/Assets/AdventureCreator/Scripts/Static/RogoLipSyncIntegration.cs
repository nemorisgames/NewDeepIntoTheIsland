using UnityEngine;

namespace AC
{

	/**
	 * A class the contains a number of static functions to assist with Rogo Digital LipSync integration.
	 * To use Rogo Digital LipSync with Adventure Creator, the 'RogoLipSyncIsPresent' preprocessor must be defined.
	 */
	public class RogoLipSyncIntegration : ScriptableObject
	{
		
		/**
		 * <summary>Checks if the 'RogoLipSyncIsPresent' preprocessor has been defined.</summary>
		 * <returns>True if the 'RogoLipSyncIsPresent' preprocessor has been defined</returns>
		 */
		public static bool IsDefinePresent ()
		{
			#if RogoLipSyncIsPresent
			return true;
			#else
			return false;
			#endif
		}


		public static void Play (Char speaker, int lineID, string language)
		{
			if (speaker == null)
			{
				return;
			}

			#if RogoLipSyncIsPresent
			if (lineID > -1 && speaker != null && KickStarter.speechManager.searchAudioFiles)
			{
				RogoDigital.Lipsync.LipSyncData lipSyncData = null;

				if (KickStarter.speechManager.autoNameSpeechFiles)
				{
					string fullName = KickStarter.speechManager.GetAutoAssetPathAndName (lineID, speaker, language, true);
					lipSyncData = Resources.Load (fullName) as RogoDigital.Lipsync.LipSyncData;

					if (lipSyncData == null && KickStarter.speechManager.fallbackAudio && Options.GetLanguage () > 0)
					{
						fullName = KickStarter.speechManager.GetAutoAssetPathAndName (lineID, speaker, string.Empty, true);
						lipSyncData = Resources.Load (fullName) as RogoDigital.Lipsync.LipSyncData;
					}

					if (lipSyncData == null)
					{
						ACDebug.LogWarning ("Lipsync file 'Resources/" + fullName + "' not found.");
					}
				}
				else
				{
					Object _object = KickStarter.runtimeLanguages.GetLineCustomLipsyncFile (lineID, Options.GetLanguage ());
					if (_object is RogoDigital.Lipsync.LipSyncData)
					{
						lipSyncData = (RogoDigital.Lipsync.LipSyncData) _object;
					}

					if (lipSyncData == null)
					{
						ACDebug.LogWarning ("No LipSync data found for " + speaker.gameObject.name + ", line ID " + lineID);
					}
				}

				if (lipSyncData != null)
				{
					RogoDigital.Lipsync.LipSync[] lipSyncs = speaker.GetComponentsInChildren <RogoDigital.Lipsync.LipSync>();
					if (lipSyncs != null && lipSyncs.Length > 0)
					{
						foreach (RogoDigital.Lipsync.LipSync lipSync in lipSyncs)
						{
							if (lipSync != null && lipSync.enabled)
							{
								lipSync.Play (lipSyncData);
							}
						}
					}
					else
					{
						ACDebug.LogWarning ("No LipSync component found on " + speaker.gameObject.name + " gameobject.");
					}
				}
			}
			#else
			ACDebug.LogError ("The 'RogoLipSyncIsPresent' preprocessor define must be declared in the Player Settings.");
			#endif
		}


		public static void Stop (Char speaker)
		{
			if (speaker == null)
			{
				return;
			}
			
			#if RogoLipSyncIsPresent
			RogoDigital.Lipsync.LipSync[] lipSyncs = speaker.GetComponentsInChildren <RogoDigital.Lipsync.LipSync>();
			if (lipSyncs != null && lipSyncs.Length > 0)
			{
				foreach (RogoDigital.Lipsync.LipSync lipSync in lipSyncs)
				{
					if (lipSync != null && lipSync.enabled)
					{
						lipSync.Stop (true);
					}
				}
			}
			#endif
		}
		
	}

}