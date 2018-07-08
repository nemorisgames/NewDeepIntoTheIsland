/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"DragBase.cs"
 * 
 *	This the base class of draggable/pickup-able objects
 * 
 */

using UnityEngine;

namespace AC
{

	/**
	 * The base class of objects that can be picked up and moved around with the mouse / touch.
	 */
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_drag_base.html")]
	#endif
	public class DragBase : Moveable
	{

		/** If True, the object is currently held by the player */
		public bool isHeld = false;
		/** If True, input vectors will be inverted */
		public bool invertInput = false;
		/** The maximum force magnitude that can be applied to itself */
		public float maxSpeed = 200f;
		/** How much player movement is reduced by when the object is being dragged */
		public float playerMovementReductionFactor = 0f;
		/** The influence that player movement has on the drag force */
		public float playerMovementInfluence = 1f;

		/** If True, the object can be moved towards and away from the camera */
		public bool allowZooming = false;
		/** The speed at which the object can be moved towards and away from the camera (if allowZooming = True) */
		public float zoomSpeed = 60f;
		/** The minimum distance that there can be between the object and the camera (if allowZooming = True) */
		public float minZoom = 1f;
		/** The maxiumum distance that there can be between the object and the camera (if allowZooming = True) */
		public float maxZoom = 3f;
		/** The speed by which the object can be rotated */
		public float rotationFactor = 1f;

		/** The object's Rigidbody, assigned automaticaly but read by other scripts */
		public Rigidbody _rigidbody;
		/** If True, then an icon will be displayed at the "grab point" when the object is held */
		public bool showIcon = false;
		/** The ID number of the CursorIcon that gets shown if showIcon = true, as defined in CursorManager's cursorIcons List */
		public int iconID = -1;

		/** The sound to play when the object is moved */
		public AudioClip moveSoundClip;
		/** The sound to play when the object has a collision */
		public AudioClip collideSoundClip;

		/** The minimum speed that the object must be moving by for sound to play */
		public float slideSoundThreshold = 0.03f;
		/** The factor by which the movement sound's pitch is adjusted in relation to speed */
		public float slidePitchFactor = 1f;
		/** If True, then the collision sound will only play when the object collides with its lower boundary collider */
		public bool onlyPlayLowerCollisionSound = false;

		/** If True, then the Physics system will ignore collisions between this object and the bounday colliders of any DragTrack that this is not locked ti */
		public bool ignoreMoveableRigidbodies;
		/** If True, then the Physics system will ignore collisions between this object and the player */
		public bool ignorePlayerCollider;
		/** If True, then this object's children will be placed on the same layer */
		public bool childrenShareLayer;

		protected Transform grabPoint;
		protected Transform cameraTransform;
		protected float distanceToCamera;
		
		protected float speedFactor = 0.16f;
		
		protected float originalDrag;
		protected float originalAngularDrag;

		protected int numCollisions = 0;

		protected CursorIconBase icon;
		protected Sound collideSound;
		protected Sound moveSound;

				
		protected virtual void Awake ()
		{
			GameObject newOb = new GameObject ();
			newOb.name = this.name + " (Grab point)";
			grabPoint = newOb.transform;
			grabPoint.parent = this.transform;

			if (moveSoundClip)
			{
				GameObject newSoundOb = new GameObject ();
				newSoundOb.name = this.name + " (Move sound)";
				newSoundOb.transform.parent = this.transform;
				newSoundOb.AddComponent <Sound>();
				newSoundOb.GetComponent <AudioSource>().playOnAwake = false;
				moveSound = newSoundOb.GetComponent <Sound>();
			}

			icon = GetMainIcon ();

			if (Camera.main)
			{
				cameraTransform = Camera.main.transform;
			}

			if (GetComponent <Sound>())
			{
				collideSound = GetComponent <Sound>();
			}

			if (GetComponent <Rigidbody>())
			{
				_rigidbody = GetComponent <Rigidbody>();
			}
			else
			{
				ACDebug.LogWarning ("A Rigidbody component is required for " + this.name);
			}
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
		 * Makes the object interactive.
		 */
		public void TurnOn ()
		{
			PlaceOnLayer (LayerMask.NameToLayer (KickStarter.settingsManager.hotspotLayer));
      	}


		/**
		 * Makes the object non-interactive.
		 */
		public void TurnOff ()
		{
			PlaceOnLayer (LayerMask.NameToLayer (KickStarter.settingsManager.deactivatedLayer));
		}


		private void PlaceOnLayer (int layerName)
		{
			gameObject.layer = layerName;
			if (childrenShareLayer)
			{
				foreach (Transform child in transform)
				{
					child.gameObject.layer = layerName;
				}
			}
		}


		/**
		 * Called every frame by StateHandler.
		 */
		public virtual void UpdateMovement ()
		{
			if (Camera.main)
			{
				cameraTransform = Camera.main.transform;
			}
		}


		/**
		 * If True, 'ToggleCursor' can be used while the object is held.
		 */
		public virtual bool CanToggleCursor ()
		{
			return false;
		}


		protected void BaseOnCollisionEnter (Collision collision)
		{
			if (collision.gameObject.tag != Tags.player)
			{
				numCollisions ++;

				if (collideSound && collideSoundClip && Time.time > 0f)
				{
					collideSound.Play (collideSoundClip, false);
				}
			}
		}


		private void OnCollisionExit (Collision collision)
		{
			if (collision.gameObject.tag != Tags.player)
			{
				numCollisions --;
			}
		}


		protected void PlayMoveSound (float speed, float trackValue)
		{
			if (speed > slideSoundThreshold)
			{
				moveSound.relativeVolume = (speed - slideSoundThreshold);// * 5f;
				moveSound.SetMaxVolume ();
				if (slidePitchFactor > 0f)
				{
					moveSound.GetComponent <AudioSource>().pitch = Mathf.Lerp (GetComponent <AudioSource>().pitch, Mathf.Min (1f, speed), Time.deltaTime * 5f);
				}
			}

			if (speed > slideSoundThreshold && !moveSound.IsPlaying ())// && trackValue > 0.02f && trackValue < 0.98f)
			{
				moveSound.relativeVolume = (speed - slideSoundThreshold);// * 5f;
				moveSound.Play (moveSoundClip, true);
			}
			else if (speed <= slideSoundThreshold && moveSound.IsPlaying () && !moveSound.IsFading ())
			{
				moveSound.FadeOut (0.2f);
			}
		}


		/**
		 * Draws an icon at the point of contact on the object, if appropriate.
		 */
		public virtual void DrawGrabIcon ()
		{
			if (isHeld && showIcon && Camera.main.WorldToScreenPoint (transform.position).z > 0f && icon != null)
			{
				Vector3 screenPosition = Camera.main.WorldToScreenPoint (grabPoint.position);
				icon.Draw (new Vector3 (screenPosition.x, screenPosition.y));
			}
		}


		/**
		 * <summary>Attaches the object to the player's control.</summary>
		 * <param name = "grabPosition">The point of contact on the object</param>
		 */
		public virtual void Grab (Vector3 grabPosition)
		{
			isHeld = true;
			grabPoint.position = grabPosition;
			originalDrag = _rigidbody.drag;
			originalAngularDrag = _rigidbody.angularDrag;
			_rigidbody.drag = 20f;
			_rigidbody.angularDrag = 20f;
		}


		/**
		 * Detaches the object from the player's control.
		 */
		public virtual void LetGo ()
		{
			isHeld = false;
			KickStarter.eventManager.Call_OnDropMoveable (this);
		}


		/**
		 * <summary>Checks if the the point of contact is visible on-screen.</summary>
		 * <returns>True if the point of contact is visible on-screen.</returns>
		 */
		public bool IsOnScreen ()
		{
			Vector2 screenPosition = Camera.main.WorldToScreenPoint (grabPoint.position);
			if (screenPosition.x < 0f || screenPosition.y < 0f || screenPosition.x > AdvGame.GetMainGameViewSize ().x || screenPosition.y > AdvGame.GetMainGameViewSize ().y)
			{
				return false;
			}
			return true;
		}


		/**
		 * <summary>Checks if the point of contact is close enough to the camera to continue being held.</summary>
		 * <param name = "maxDistance">The maximum-allowed distance between the point of contact and the camera</param>
		 */
		public bool IsCloseToCamera (float maxDistance)
		{
			if ((GetGrabPosition () - Camera.main.transform.position).magnitude < maxDistance)
			{
				return true;
			}
			return false;
		}


		/**
		 * <summary>Applies a drag force on the object, based on the movement of the cursor.</summary>
		 * <param name = "force">The force vector to apply</param>
		 * <param name = "mousePosition">The position of the mouse</param>
		 * <param name = "distanceToCamera">The distance between the object's centre and the camera</param>
		 */
		public virtual void ApplyDragForce (Vector3 force, Vector3 mousePosition, float distanceToCamera)
		{}


		protected void UpdateZoom ()
		{
			float zoom = Input.GetAxis ("ZoomMoveable");
			Vector3 moveVector = (transform.position - cameraTransform.position).normalized;
			
			if (distanceToCamera - minZoom < 1f && zoom < 0f)
			{
				moveVector *= (distanceToCamera - minZoom);
			}
			else if (maxZoom - distanceToCamera < 1f && zoom > 0f)
			{
				moveVector *= (maxZoom - distanceToCamera);
			}
			
			if ((distanceToCamera < minZoom && zoom < 0f) || (distanceToCamera > maxZoom && zoom > 0f))
			{
				_rigidbody.AddForce (-moveVector * zoom * zoomSpeed);
				_rigidbody.velocity = Vector3.zero;
			}
			else
			{
				_rigidbody.AddForce (moveVector * zoom * zoomSpeed);
			}
		}


		protected void LimitZoom ()
		{
			if (distanceToCamera < minZoom)
			{
				transform.position = cameraTransform.position + (transform.position - cameraTransform.position) / (distanceToCamera / minZoom);
			}
			else if (distanceToCamera > maxZoom)
			{
				transform.position = cameraTransform.position + (transform.position - cameraTransform.position) / (distanceToCamera / maxZoom);
			}
		}


		/**
		 * <summary>Gets the point of contact on the object, once grabbed.</summary>
		 * <returns>The point of contact on the object, once grabbed</returns>
		 */
		public Vector3 GetGrabPosition ()
		{
			return grabPoint.position;
		}


		private CursorIconBase GetMainIcon ()
		{
			if (KickStarter.cursorManager == null || iconID < 0)
			{
				return null;
			}
			
			return KickStarter.cursorManager.GetCursorIconFromID (iconID);
		}

	}

}