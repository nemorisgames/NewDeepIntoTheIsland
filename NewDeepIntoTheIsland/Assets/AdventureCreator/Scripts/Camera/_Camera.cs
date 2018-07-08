/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"_Camera.cs"
 * 
 *	This is the base class for all other cameras.
 * 
 */


using UnityEngine;

namespace AC
{

	/**
	 * The base class for all Adventure Creator cameras.
	 * To integrate a custom camera script to AC, just add this component to the same object as the Camera component, and it will be visible to AC's fields, functions and Actions.
	 */
	[AddComponentMenu("Adventure Creator/Camera/Basic camera")]
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1___camera.html")]
	#endif
	public class _Camera : MonoBehaviour
	{

		/** The Unity Camera component */
		public Camera _camera;
		/** If True, the camera will move according to the Player prefab */
		public bool targetIsPlayer = true;
		/** The Transform that affects the camera's movement */
		public Transform target;
		/** If True, then the camera can be drag-controlled (used for GameCameraThirdPerson only) */
		public bool isDragControlled = false;
		/** The camera's focal distance */
		public float focalDistance = 10f;

		private Char targetChar;
		protected Vector2 inputMovement;

		[SerializeField] [HideInInspector] private bool is2D = false;


		protected virtual void Awake ()
		{
			if (GetComponent <Camera>())
			{
				_camera = GetComponent <Camera>();

				if (KickStarter.mainCamera)
				{
					_camera.enabled = false;
				}
			}
			else
			{
				if (GetComponentInChildren <Camera>())
				{
					_camera = GetComponentInChildren <Camera>();
				}
				else
				{
					ACDebug.LogWarning (this.name + " has no Camera component!");
				}
			}

			SwitchTarget (target);
		}


		private void OnEnable ()
		{
			if (KickStarter.stateHandler) KickStarter.stateHandler.Register (this);
		}


		protected virtual void Start ()
		{
			if (KickStarter.stateHandler) KickStarter.stateHandler.Register (this);
		}


		private void OnDisable ()
		{
			if (KickStarter.stateHandler) KickStarter.stateHandler.Unregister (this);
		}


		/**
		 * <summary>Switches the camera's target.</summary>
		 * <param name = "_target">The new target</param>
		 */
		public virtual void SwitchTarget (Transform _target)
		{
			target = _target;

			if (target != null)
			{
				targetChar = _target.GetComponent <Char>();
			}
			else
			{
				targetChar = null;
			}
		}


		/**
		 * True if the game plays in 2D, making use of 2D colliders and raycasts
		 */
		public bool isFor2D
		{
			get
			{
				return is2D;
			}
			set
			{
				is2D = value;
			}
		}


		protected Vector3 TargetForward
		{
			get
			{
				if (targetChar != null)
				{
					return targetChar.TransformForward;
				}
				if (target != null)
				{
					return target.forward;
				}
				return Vector3.zero;
			}
		}


		/**
		 * <summary>Checks if the camera is for 2D games.  This is necessary for working out if the MainCamera needs to change its projection matrix.</summary>
		 * <returns>True if the camera is for 2D games</returns>
		 */
		public virtual bool Is2D ()
		{
			return is2D;
		}


		/**
		 * Updates the camera.
		 * This is called every frame by StateHandler.
		 */
		public virtual void _Update ()
		{}


		/**
		 * Auto-assigns "_camera" as the Unity Camera component on the same object as this script
		 */
		public void SetCameraComponent ()
		{
			if (_camera == null)
			{
				if (GetComponent <Camera>())
				{
					_camera = GetComponent <Camera>();
				}
				else if (GetComponentInChildren <Camera>())
				{
					_camera = GetComponentInChildren <Camera>();
				}
			}

		}


		/**
		 * Auto-assigns "target" as the Player prefab Transform if targetIsPlayer = True.
		 */
		public virtual void ResetTarget ()
		{
			if (targetIsPlayer && KickStarter.player)
			{
				SwitchTarget (KickStarter.player.transform);
			}
		}


		protected Vector3 PositionRelativeToCamera (Vector3 _position)
		{
			return (_position.x * ForwardVector ()) + (_position.z * RightVector ());
		}
		
		
		protected Vector3 RightVector ()
		{
			return (transform.right);
		}
		
		
		protected Vector3 ForwardVector ()
		{
			Vector3 camForward;
			
			camForward = transform.forward;
			camForward.y = 0;
			
			return (camForward);
		}
		

		/**
		 * Moves the camera instantly to its destination.
		 */
		public virtual void MoveCameraInstant ()
		{ }


		protected float ConstrainAxis (float desired, Vector2 range)
		{
			if (range.x < range.y)
			{
				desired = Mathf.Clamp (desired, range.x, range.y);
			}
			
			else if (range.x > range.y)
			{
				desired = Mathf.Clamp (desired, range.y, range.x);
			}
			
			else
			{
				desired = range.x;
			}
				
			return desired;
		}


		/**
		 * Enables the camera for split-screen, using the MainCamera as the "main" part of the split, with all the data.
		 */
		public void SetSplitScreen ()
		{
			_camera.enabled = true;
			_camera.rect = KickStarter.mainCamera.GetSplitScreenRect (false);
		}


		/**
		 * Removes the split-screen effect on this camera.
		 */
		public void RemoveSplitScreen ()
		{
			if (_camera.enabled)
			{
				_camera.rect = new Rect (0f, 0f, 1f, 1f);
				_camera.enabled = false;
			}
		}


		/**
		 * <summary>Gets the actual horizontal and vertical panning offsets.</summary>
		 * <returns>The actual horizontal and vertical panning offsets</returns>
		 */
		public virtual Vector2 GetPerspectiveOffset ()
		{
			return Vector2.zero;
		}


		/**
		 * <summary>Checks if the _Camera is currently the MainCamera's active camera (attachedCamera)</summary>
		 * <returns>True if the _Camera is currently the MainCamera's active camera (attachedCamera)</returns>
		 */
		public bool IsActive ()
		{
			if (KickStarter.mainCamera != null)
			{
				return (KickStarter.mainCamera.attachedCamera == this);
			}
			return false;
		}

	}

}