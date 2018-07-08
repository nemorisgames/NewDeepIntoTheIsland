/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionSound.cs"
 * 
 *	This action triggers the sound component of any GameObject, overriding that object's play settings.
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
	public class ActionSound : Action
	{

		public int constantID = 0;
		public int parameterID = -1;
		public Sound soundObject;

		public AudioClip audioClip;
		public int audioClipParameterID = -1;

		public enum SoundAction { Play, FadeIn, FadeOut, Stop }
		public float fadeTime;
		public bool loop;
		public bool ignoreIfPlaying = false;
		public SoundAction soundAction;
		public bool affectChildren = false;

		private Sound runtimeSound;
		
		
		public ActionSound ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Sound;
			title = "Play";
			description = "Triggers a Sound object to start playing. Can be used to fade sounds in or out.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			audioClip = (AudioClip) AssignObject <AudioClip> (parameters, audioClipParameterID, audioClip);
			soundObject = AssignFile <Sound> (parameters, parameterID, constantID, soundObject);

			runtimeSound = soundObject;
			if (runtimeSound == null && audioClip != null)
			{
				runtimeSound = KickStarter.sceneSettings.defaultSound;
			}
		}

		
		override public float Run ()
		{
			if (runtimeSound == null)
			{
				return 0f;
			}

			if (!isRunning)
			{
				isRunning = true;

				if (ignoreIfPlaying && (soundAction == SoundAction.Play || soundAction == SoundAction.FadeIn))
				{
					if ((audioClip != null && runtimeSound.IsPlaying (audioClip)) || (audioClip == null && runtimeSound.IsPlaying ()))
					{
						// Sound object is already playing the desired clip
						return 0f;
					}
				}

				if (audioClip && runtimeSound.GetComponent <AudioSource>())
				{
					if (soundAction == SoundAction.Play || soundAction == SoundAction.FadeIn)
					{
						runtimeSound.GetComponent <AudioSource>().clip = audioClip;
					}
				}

				if (runtimeSound.soundType == SoundType.Music && (soundAction == SoundAction.Play || soundAction == SoundAction.FadeIn))
				{
					Sound[] sounds = FindObjectsOfType (typeof (Sound)) as Sound[];
					foreach (Sound sound in sounds)
					{
						sound.EndOld (SoundType.Music, runtimeSound);
					}
				}

				if (soundAction == SoundAction.Play)
				{
					runtimeSound.Play (loop);

					if (!loop && willWait)
					{
						return defaultPauseTime;
					}
				}
				else if (soundAction == SoundAction.FadeIn)
				{
					if (fadeTime <= 0f)
					{
						runtimeSound.Play (loop);
					}
					else
					{
						runtimeSound.FadeIn (fadeTime, loop);

						if (!loop && willWait)
						{
							return defaultPauseTime;
						}
					}
				}
				else if (soundAction == SoundAction.FadeOut)
				{
					if (fadeTime <= 0f)
					{
						runtimeSound.Stop ();
					}
					else
					{
						runtimeSound.FadeOut (fadeTime);

						if (willWait)
						{
							return fadeTime;
						}
					}
				}
				else if (soundAction == SoundAction.Stop)
				{
					runtimeSound.Stop ();

					if (affectChildren)
					{
						foreach (Transform child in runtimeSound.transform)
						{
							if (child.GetComponent <Sound>())
							{
								child.GetComponent <Sound>().Stop ();
							}
						}
					}
				}
			}
			else
			{
				if (soundAction == SoundAction.FadeOut)
				{
					isRunning = false;
					return 0f;
				}

				if (runtimeSound.IsPlaying ())
				{
					return defaultPauseTime;
				}
				else
				{
					isRunning = false;
				}
			}
			
			return 0f;
		}


		override public void Skip ()
		{
			if (soundAction == SoundAction.FadeOut || soundAction == SoundAction.Stop)
			{
				Run ();
			}
			else if (loop)
			{
				Run ();
			}
		}
		
		
		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			parameterID = Action.ChooseParameterGUI ("Sound object:", parameters, parameterID, ParameterType.GameObject);
			if (parameterID >= 0)
			{
				constantID = 0;
				soundObject = null;
			}
			else
			{
				soundObject = (Sound) EditorGUILayout.ObjectField ("Sound object:", soundObject, typeof (Sound), true);
				
				constantID = FieldToID <Sound> (soundObject, constantID);
				soundObject = IDToField <Sound> (soundObject, constantID, false);
			}

			soundAction = (SoundAction) EditorGUILayout.EnumPopup ("Sound action:", (SoundAction) soundAction);
			
			if (soundAction == SoundAction.Play || soundAction == SoundAction.FadeIn)
			{
				loop = EditorGUILayout.Toggle ("Loop?", loop);
				ignoreIfPlaying = EditorGUILayout.Toggle ("Ignore if already playing?", ignoreIfPlaying);

				audioClipParameterID = Action.ChooseParameterGUI ("New clip (optional):", parameters, audioClipParameterID, ParameterType.UnityObject);
				if (audioClipParameterID < 0)
				{
					audioClip = (AudioClip) EditorGUILayout.ObjectField ("New clip (optional):", audioClip, typeof (AudioClip), false);
				}
			}
			
			if (soundAction == SoundAction.FadeIn || soundAction == SoundAction.FadeOut)
			{
				fadeTime = EditorGUILayout.Slider ("Fade time:", fadeTime, 0f, 10f);
			}

			if (soundAction == SoundAction.Stop)
			{
				affectChildren = EditorGUILayout.Toggle ("Stop child Sounds, too?", affectChildren);
			}
			else
			{
				if (soundAction == SoundAction.FadeOut || !loop)
				{
					willWait = EditorGUILayout.Toggle ("Wait until finish?", willWait);
				}
			}

			AfterRunningOption ();
		}


		override public void AssignConstantIDs (bool saveScriptsToo)
		{
			if (saveScriptsToo)
			{
				AddSaveScript <RememberSound> (soundObject);
			}
			AssignConstantID <Sound> (soundObject, constantID, parameterID);
		}
		
		
		override public string SetLabel ()
		{
			string labelAdd = "";
			if (soundObject)
			{
				labelAdd = " (" + soundAction.ToString ();
				labelAdd += " " + soundObject.name + ")";
			}
			
			return labelAdd;
		}

		#endif

	}

}