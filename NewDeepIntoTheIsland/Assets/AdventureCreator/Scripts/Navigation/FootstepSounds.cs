/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"FootstepSounds.cs"
 * 
 *	A component that can play footstep sounds whenever a Mecanim-animated Character moves.
 * The component stores an array of AudioClips, one of which is played at random whenever the PlayFootstep method is called.
 * This method should be invoked as part of a Unity AnimationEvent: http://docs.unity3d.com/Manual/animeditor-AnimationEvents.html
 * 
 */

using UnityEngine;

namespace AC
{

	/**
	 * A component that can play footstep sounds whenever a Mecanim-animated Character moves.
	 * The component stores an array of AudioClips, one of which is played at random whenever the PlayFootstep method is called.
	 * This method should be invoked as part of a Unity AnimationEvent: http://docs.unity3d.com/Manual/animeditor-AnimationEvents.html
	 */
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_footstep_sounds.html")]
	#endif
	[AddComponentMenu("Adventure Creator/Characters/Footstep sounds")]
	public class FootstepSounds: MonoBehaviour
	{

		/** An array of footstep AudioClips to play at random */
		public AudioClip[] footstepSounds;
		/** An array of footstep AudioClips to play at random when running - if left blank, normal sounds will play */
		public AudioClip[] runSounds;
		/** The Sound object to play from */
		public Sound soundToPlayFrom;
		/** How the sounds are played */
		public FootstepPlayMethod footstepPlayMethod = FootstepPlayMethod.ViaAnimationEvents;
		public enum FootstepPlayMethod { Automatically, ViaAnimationEvents };
		/** The Player or NPC that this component is for */
		public Char character;
		/** If True, and character is assigned, sounds will only play when the character is grounded */
		public bool doGroundedCheck;

		/** The separation time between sounds when walking */
		public float walkSeparationTime = 0.5f;
		/** The separation time between sounds when running */
		public float runSeparationTime = 0.25f;

		private int lastIndex;
		private AudioSource audioSource;
		private float delayTime;
		
		
		private void Awake ()
		{
			if (soundToPlayFrom != null)
			{
				audioSource = soundToPlayFrom.GetComponent <AudioSource>();
			}

			if (character == null)
			{
				character = GetComponent <Char>();
			}
			delayTime = walkSeparationTime / 2f;
		}


		private void Update ()
		{
			if (character == null || footstepPlayMethod == FootstepPlayMethod.ViaAnimationEvents) return;

			if (character.charState == CharState.Move && !character.isJumping)
			{
				delayTime -= Time.deltaTime;

				if (delayTime <= 0f)
				{
					delayTime = (character.isRunning) ? runSeparationTime : walkSeparationTime;
					PlayFootstep ();
				}
			}
			else
			{
				delayTime = walkSeparationTime / 2f;
			}
		}
		

		/**
		 * Plays one of the footstepSounds at random on the assigned Sound object.
		 */
		public void PlayFootstep ()
		{
			if (audioSource != null && footstepSounds.Length > 0 &&
			    (character == null || character.charState == CharState.Move))
			{
				if (doGroundedCheck && character != null)
				{
					if (!character.IsGrounded ())
					{
						return;
					}
				}

				bool doRun = (character.isRunning && runSounds.Length > 0) ? true : false;
				if (doRun)
				{
					PlaySound (runSounds);
				}
				else
				{
					PlaySound (footstepSounds);
				}
			}
		}


		private void PlaySound (AudioClip[] sounds)
		{
			if (sounds == null) return;

			if (sounds.Length == 1)
			{
				audioSource.clip = sounds [0];
				soundToPlayFrom.Play (false);
				return;
			}

			int newIndex = Random.Range (0, sounds.Length - 1);
			if (newIndex == lastIndex)
			{
				newIndex ++;
				if (newIndex >= sounds.Length)
				{
					newIndex = 0;
				}
			}

			if (sounds[newIndex] != null)
			{
				audioSource.clip = sounds [newIndex];
				soundToPlayFrom.Play (false);
			}

			lastIndex = newIndex;
		}

	}

}