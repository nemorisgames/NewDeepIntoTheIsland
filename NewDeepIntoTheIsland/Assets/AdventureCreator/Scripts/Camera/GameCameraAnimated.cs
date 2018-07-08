using UnityEngine;

namespace AC
{

	/**
	 * A camera that plays an animation when it is made active.
	 * The animation will either play normally, or alternatively, set match its normalised time with the target's position along a Paths object -
	 * allowing for fancy camera movement as the Player moves around a scene.
	 */
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_game_camera_animated.html")]
	#endif
	public class GameCameraAnimated : _Camera
	{

		/** If True, then the camera will rotate towards the cursor's position on-screen */
		public bool followCursor = false;
		/** The influence that the cursor's position has on rotation, if followCursor = True */
		public Vector2 cursorInfluence = new Vector2 (0.3f, 0.1f);
		/** If True, and followCursor = True, then camera rotation according to the cursor's X position will be limited */
		public bool constrainCursorInfluenceX = false;
		/** The lower and upper limits, if constrainCursorInfluenceX = True */
		public Vector2 limitCursorInfluenceX;
		/** If True, and followCursor = True, then camera rotation according to the cursor's Y position will be limited */
		public bool constrainCursorInfluenceY = false;
		/** The lower and upper limits, if constrainCursorInfluenceY = True */
		public Vector2 limitCursorInfluenceY;

		/** The animation to play when this camera is made active */
		public AnimationClip clip;
		/** If True, and animatedCameraType = AnimatedCameraType.PlayWhenActive, then the animation will loop */
		public bool loopClip;
		/** If True, and animatedCameraType = AnimatedCameraType.PlayWhenActive, then the animation will play when the scene begins, rather than waiting for it to become active */
		public bool playOnStart;
		/** How animations are played (PlayWhenActive, SyncWithTargetMovement) */
		public AnimatedCameraType animatedCameraType = AnimatedCameraType.PlayWhenActive;
		/** The Paths object to sync with animation, animatedCameraType = AnimatedCameraType.SyncWithTargetMovement */
		public Paths pathToFollow;
		
		private float pathLength;
		
		
		protected override void Start ()
		{
			base.Start ();

			if (animatedCameraType == AnimatedCameraType.PlayWhenActive)
			{
				if (playOnStart)
				{
					PlayClip ();
				}
			}
			else if (pathToFollow)
			{
				pathLength = pathToFollow.GetTotalLength ();
				ResetTarget ();
				
				if (target)
				{
					MoveCameraInstant ();
				}
			}
		}
		
		
		public override void _Update ()
		{
			MoveCamera ();
		}


		/**
		 * <summary>Checks if the AnimationClip "clip" is playing.</summary>
		 * <returns>True if the AnimationClip "clip" is playing</returns>
		 */
		public bool isPlaying ()
		{
			if (clip && GetComponent <Animation>() && GetComponent <Animation>().IsPlaying (clip.name))
			{
				return true;
			}

			return false;
		}
		

		/**
		 * Plays the AnimationClip "clip" if animatedCameraType = AnimatedCameraType.PlayWhenActive.
		 */
		public void PlayClip ()
		{
			if (GetComponent <Animation>() == null)
			{
				ACDebug.LogError ("Cannot play animation on " + this.name + " - no Animation component is attached.");
				return;
			}
			
			if (clip && animatedCameraType == AnimatedCameraType.PlayWhenActive)
			{
				WrapMode wrapMode = WrapMode.Once;
				if (loopClip)
				{
					wrapMode = WrapMode.Loop;
				}
				AdvGame.PlayAnimClip (GetComponent <Animation>(), 0, clip, AnimationBlendMode.Blend, wrapMode, 0f, null, false);
			}
		}
		
		
		public override void MoveCameraInstant ()
		{
			MoveCamera ();
		}
		
		
		private void MoveCamera ()
		{
			if (target && animatedCameraType == AnimatedCameraType.SyncWithTargetMovement && clip && target)
			{
				AdvGame.PlayAnimClipFrame (GetComponent <Animation>(), 0, clip, AnimationBlendMode.Blend, WrapMode.Once, 0f, null, GetProgress ());
			}
		}


		private float GetProgress ()
		{
			if (pathToFollow.nodes.Count <= 1)
			{
				return 0f;
			}

			double nearest_dist = 1000f;
			Vector3 nearestPoint = Vector3.zero;
			int i =0;

			for (i=1; i <pathToFollow.nodes.Count; i++)
			{
				Vector3 p1 = pathToFollow.nodes[i-1];
				Vector3 p2 = pathToFollow.nodes[i];
				
				Vector3 p = GetNearestPointOnSegment (p1, p2);
				if (p != nearestPoint)
				{
					float d = Mathf.Sqrt (Vector3.Distance (target.position, p));
					if (d < nearest_dist)
					{
						nearest_dist = d;
						nearestPoint = p;
					}
					else
						break;
				}
			}
			
			return (pathToFollow.GetLengthToNode (i-2) + Vector3.Distance (pathToFollow.nodes[i-2], nearestPoint)) / pathLength;
		}

		
		private Vector3 GetNearestPointOnSegment (Vector3 p1, Vector3 p2)
		{
			float d2 = (p1.x - p2.x)*(p1.x - p2.x) + (p1.z - p2.z)*(p1.z - p2.z);
			float t = ((target.position.x - p1.x) * (p2.x - p1.x) + (target.position.z - p1.z) * (p2.z - p1.z)) / d2;
			
			if (t < 0)
			{
				return p1;
			}
			if (t > 1)
			{
				return p2;
			}
			
			return new Vector3 ((p1.x + t * (p2.x - p1.x)), 0f, (p1.z + t * (p2.z - p1.z)));
		}

	}
	
}

