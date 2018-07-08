/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionAmbience.cs"
 * 
 *	This action can be used to play, fade or queue ambience clips.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	[System.Serializable]
	public class ActionAmbience : Action
	{

		public int trackID;
		public int trackIDParameterID = -1;

		public float fadeTime;
		public int fadeTimeParameterID = -1;

		public bool loop;
		public bool isQueued;

		public bool resumeFromStart = true;
		public bool resumeIfPlayedBefore = false;

		public MusicAction musicAction;

		
		public ActionAmbience ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Sound;
			title = "Play ambience";
			description = "Plays or queues ambience clips.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			trackID = AssignInteger (parameters, trackIDParameterID, trackID);
			fadeTime = AssignFloat (parameters, fadeTimeParameterID, fadeTime);
		}
		
		
		override public float Run ()
		{
			if (!isRunning)
			{
				isRunning = true;
				float waitTime = Perform (fadeTime);

				if (willWait && waitTime > 0f && !isQueued)
				{
					return (waitTime);
				}
			}
			else
			{
				isRunning = false;
			}
			return 0f;
		}
		
		
		override public void Skip ()
		{
			Perform (0f);
		}


		private float Perform (float _time)
		{
			Ambience ambience = KickStarter.stateHandler.GetAmbienceEngine ();
			if (ambience != null)
			{
				if (musicAction == MusicAction.Play)
				{
					return ambience.Play (trackID, loop, isQueued, _time, resumeIfPlayedBefore);
				}
				else if (musicAction == MusicAction.Crossfade)
				{
					return ambience.Crossfade (trackID, loop, isQueued, _time, resumeIfPlayedBefore);
				}
				else if (musicAction == MusicAction.Stop)
				{
					return ambience.StopAll (_time);
				}
				else if (musicAction == MusicAction.ResumeLastStopped)
				{
					return ambience.ResumeLastQueue (_time, resumeFromStart);
				}
			}
			return 0f;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			if (AdvGame.GetReferences ().settingsManager != null)
			{
				SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;

				if (GUILayout.Button ("Ambience Storage window"))
				{
					AmbienceStorageWindow.Init ();
				}
				
				if (settingsManager.ambienceStorages.Count == 0)
				{
					EditorGUILayout.HelpBox ("Before a track can be selected, it must be added to the Ambience Storage window.", MessageType.Info);
					return;
				}

				musicAction = (MusicAction) EditorGUILayout.EnumPopup ("Method:", (MusicAction) musicAction);

				string fadeLabel = "Transition time (s):";
				if (musicAction == MusicAction.Play || musicAction == MusicAction.Crossfade)
				{
					GetTrackIndex (settingsManager.ambienceStorages.ToArray (), parameters);
				
					loop = EditorGUILayout.Toggle ("Loop?", loop);
					isQueued = EditorGUILayout.Toggle ("Queue?", isQueued);
					resumeIfPlayedBefore = EditorGUILayout.Toggle ("Resume if played before?", resumeIfPlayedBefore);
				}
				else if (musicAction == MusicAction.Stop)
				{
					fadeLabel = "Fade-out time (s):";
				}
				else if (musicAction == MusicAction.ResumeLastStopped)
				{
					resumeFromStart = EditorGUILayout.Toggle ("Restart track?", resumeFromStart);
				}

				fadeTimeParameterID = Action.ChooseParameterGUI (fadeLabel, parameters, fadeTimeParameterID, ParameterType.Float);
				if (fadeTimeParameterID < 0)
				{
					fadeTime = EditorGUILayout.Slider (fadeLabel, fadeTime, 0f, 10f);
				}

				if (fadeTime > 0f && !isQueued)
				{
					willWait = EditorGUILayout.Toggle ("Wait until transition ends?", willWait);
				}
			}
			else
			{
				EditorGUILayout.HelpBox ("A Settings Manager must be defined for this Action to function correctly. Please go to your Game Window and assign one.", MessageType.Warning);
			}

			AfterRunningOption ();
		}


		private int GetTrackIndex (MusicStorage[] musicStorages, List<ActionParameter> parameters, bool showGUI = true)
		{
			int trackIndex = -1;
			List<string> labelList = new List<string>();

			for (int i=0; i<musicStorages.Length; i++)
			{
				string label = musicStorages[i].ID + ": ";
				if (musicStorages[i].audioClip == null)
				{
					label += "(Unassigned)";
				}
				else
				{
					label += musicStorages[i].audioClip.name;
				}
				labelList.Add (label);

				if (musicStorages[i].ID == trackID)
				{
					trackIndex = i;
				}
			}

			if (musicStorages.Length == 0)
			{
				labelList.Add ("(None set)");
			}

			if (showGUI)
			{
				trackIDParameterID = Action.ChooseParameterGUI ("Ambience track ID:", parameters, trackIDParameterID, ParameterType.Integer);
				if (trackIDParameterID < 0)
				{
					trackIndex = Mathf.Max (trackIndex, 0);
					trackIndex = EditorGUILayout.Popup ("Ambience track:", trackIndex, labelList.ToArray());
				}
			}

			if (trackIndex >= 0 && trackIndex < musicStorages.Length)
			{
				trackID = musicStorages[trackIndex].ID;
			}
			else
			{
				trackID = 0;
			}
			return trackIndex;
		}
		
		
		override public string SetLabel ()
		{
			string labelAdd = " (" + musicAction.ToString ();
			if (musicAction == MusicAction.Play &&
			    AdvGame.GetReferences ().settingsManager != null &&
			    AdvGame.GetReferences ().settingsManager.ambienceStorages != null)
			{
				int trackIndex = GetTrackIndex (AdvGame.GetReferences ().settingsManager.ambienceStorages.ToArray (), null, false);
				if (trackIndex >= 0)
				{
					labelAdd += " " + AdvGame.GetReferences ().settingsManager.ambienceStorages[trackIndex].audioClip.name.ToString ();
				}
			}
			labelAdd += ")";

			return labelAdd;
		}
		
		#endif
		
	}
	
}