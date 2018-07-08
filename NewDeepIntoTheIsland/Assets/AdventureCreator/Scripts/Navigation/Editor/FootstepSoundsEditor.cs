using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace AC
{

	[CustomEditor(typeof(FootstepSounds))]
	public class FootstepSoundsEditor : Editor
	{
		
		public override void OnInspectorGUI ()
		{
			FootstepSounds _target = (FootstepSounds) target;

			EditorGUILayout.Space ();
			_target.footstepSounds = ShowClipsGUI (_target.footstepSounds, "Walking sounds");
			EditorGUILayout.Space ();
			_target.runSounds = ShowClipsGUI (_target.runSounds, "Running sounds (optional)");
			EditorGUILayout.Space ();

			EditorGUILayout.BeginVertical ("Button");
			_target.character = (Char) EditorGUILayout.ObjectField ("Character:", _target.character, typeof (Char), true);
			_target.doGroundedCheck = EditorGUILayout.ToggleLeft ("Only play when grounded?", _target.doGroundedCheck);
			if (_target.doGroundedCheck && _target.character == null)
			{
				EditorGUILayout.HelpBox ("A Character must be assigned for the 'grounded' check to work.", MessageType.Warning);
			}
			_target.soundToPlayFrom = (Sound) EditorGUILayout.ObjectField ("Sound to play from:", _target.soundToPlayFrom, typeof (Sound), true);

			_target.footstepPlayMethod = (FootstepSounds.FootstepPlayMethod) EditorGUILayout.EnumPopup ("Play sounds:", _target.footstepPlayMethod);
			if (_target.footstepPlayMethod == FootstepSounds.FootstepPlayMethod.Automatically)
			{
				_target.walkSeparationTime = EditorGUILayout.FloatField ("Walking separation time (s):", _target.walkSeparationTime);
				_target.runSeparationTime = EditorGUILayout.FloatField ("Running separation time (s):", _target.runSeparationTime);
			}
			else if (_target.footstepPlayMethod == FootstepSounds.FootstepPlayMethod.ViaAnimationEvents)
			{
				EditorGUILayout.HelpBox ("A sound will be played whenever this component's PlayFootstep function is run.", MessageType.Info);
			}
			EditorGUILayout.EndVertical ();

			UnityVersionHandler.CustomSetDirty (_target);
		}


		private AudioClip[] ShowClipsGUI (AudioClip[] clips, string headerLabel)
		{
			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField (headerLabel, EditorStyles.boldLabel);
			List<AudioClip> clipsList = new List<AudioClip>();

			if (clips != null)
			{
				foreach (AudioClip clip in clips)
				{
					clipsList.Add (clip);
				}
			}

			int numParameters = clipsList.Count;
			numParameters = EditorGUILayout.IntField ("# of footstep sounds:", numParameters);

			if (numParameters < clipsList.Count)
			{
				clipsList.RemoveRange (numParameters, clipsList.Count - numParameters);
			}
			else if (numParameters > clipsList.Count)
			{
				if (numParameters > clipsList.Capacity)
				{
					clipsList.Capacity = numParameters;
				}
				for (int i=clipsList.Count; i<numParameters; i++)
				{
					clipsList.Add (null);
				}
			}

			for (int i=0; i<clipsList.Count; i++)
			{
				clipsList[i] = (AudioClip) EditorGUILayout.ObjectField ("Sound #" + (i+1).ToString (), clipsList[i], typeof (AudioClip), false);
			}
			if (clipsList.Count > 1)
			{
				EditorGUILayout.HelpBox ("Sounds will be chosen at random.", MessageType.Info);
			}
			EditorGUILayout.EndVertical ();

			return clipsList.ToArray ();
		}

	}
}