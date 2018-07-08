/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"Moveable_PickUp.cs"
 * 
 *	Attaching this script to a GameObject allows it to be
 *	picked up and manipulated freely by the player.
 * 
 */

using UnityEngine;

namespace AC
{

	/**
	 * Attaching this component to a GameObject allows it to be picked up and manipulated freely by the player.
	 */
	[RequireComponent (typeof (Rigidbody))]
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_moveable___pick_up.html")]
	#endif
	public class Moveable_PickUp : DragBase
	{

		/** If True, the object can be rotated */
		public bool allowRotation = false;
		/** The maximum force magnitude that can be applied by the player - if exceeded, control will be removed */
		public float breakForce = 300f;
		/** If True, the object can be thrown */
		public bool allowThrow = false;
		/** How long a "charge" takes, if the object cen be thrown */
		public float chargeTime = 0.5f;
		/** How far the object is pulled back while chargine, if the object can be thrown */
		public float pullbackDistance = 0.6f;
		/** How far the object can be thrown */
		public float throwForce = 400f;
		/** The Interaction to run whenever the object is picked up by the player */
		public Interaction interactionOnGrab;
		/** The lift to give objects picked up, so that they aren't touching the ground when initially held */
		public float initialLift = 0.05f;

		private bool isChargingThrow = false;
		private float throwCharge = 0f;
		private float chargeStartTime;
		private bool inRotationMode = false;
		private FixedJoint fixedJoint;
		private float originalDistanceToCamera;

		private Vector3 worldMousePosition;
		private Vector3 deltaMovement;
		private LerpUtils.Vector3Lerp fixedJointLerp = new LerpUtils.Vector3Lerp ();

		
		protected override void Start ()
		{
			LimitCollisions ();
			base.Start ();
		}


		/**
		 * Called every frame by StateHandler.
		 */
		public override void UpdateMovement ()
		{
			base.UpdateMovement ();

			if (moveSound && moveSoundClip && !inRotationMode)
			{
				if (numCollisions > 0)
			    {
					PlayMoveSound (_rigidbody.velocity.magnitude, 0.5f);
				}
				else if (moveSound.IsPlaying ())
				{
					moveSound.Stop ();
				}
			}
		}


		private void ChargeThrow ()
		{
			if (!isChargingThrow)
			{
				isChargingThrow = true;
				chargeStartTime = Time.time;
				throwCharge = 0f;
			}
			else if (throwCharge < 1f)
			{
				throwCharge = (Time.time - chargeStartTime) / chargeTime;
			}

			if (throwCharge > 1f)
			{
				throwCharge = 1f;
			}
		}


		private void ReleaseThrow ()
		{
			LetGo ();

			_rigidbody.useGravity = true;
			_rigidbody.drag = originalDrag;
			_rigidbody.angularDrag = originalAngularDrag;

			Vector3 moveVector = (transform.position - cameraTransform.position).normalized;
			_rigidbody.AddForce (throwForce * throwCharge * moveVector);
		}
		
		
		private void CreateFixedJoint ()
		{
			GameObject go = new GameObject (this.name + " (Joint)");
			Rigidbody body = go.AddComponent <Rigidbody>();
			body.constraints = RigidbodyConstraints.FreezeAll;
			body.useGravity = false;
			fixedJoint = go.AddComponent <FixedJoint>();
			fixedJoint.breakForce = fixedJoint.breakTorque = breakForce;

			go.AddComponent <JointBreaker>();
		}
		

		/**
		 * <summary>Attaches the object to the player's control.</summary>
		 * <param name = "grabPosition">The point of contact on the object</param>
		 */
		public override void Grab (Vector3 grabPosition)
		{
			inRotationMode = false;
			isChargingThrow = false;
			throwCharge = 0f;

			if (fixedJoint == null)
			{
				CreateFixedJoint ();
			}
			fixedJoint.transform.position = grabPosition;
			fixedJointOffset = Vector3.zero;
			deltaMovement = Vector3.zero;

			_rigidbody.velocity = _rigidbody.angularVelocity = Vector3.zero;
			originalDistanceToCamera = (grabPosition - cameraTransform.position).magnitude;

			base.Grab (grabPosition);

			if (interactionOnGrab)
			{
				interactionOnGrab.Interact ();
			}
		}


		/**
		 * Detaches the object from the player's control.
		 */
		public override void LetGo ()
		{
			if (inRotationMode)
			{
				SetRotationMode (false);
			}

			if (fixedJoint != null && fixedJoint.connectedBody)
			{
				fixedJoint.connectedBody = null;
			}

			_rigidbody.drag = originalDrag;
			_rigidbody.angularDrag = originalAngularDrag;

			if (inRotationMode)
			{
				_rigidbody.velocity = Vector3.zero;
			}
			else if (!isChargingThrow)
			{
				_rigidbody.AddForce (deltaMovement * Time.deltaTime / Time.fixedDeltaTime * 10f);
			}

			_rigidbody.useGravity = true;

			base.LetGo ();
		}


		/**
		 * If True, 'ToggleCursor' can be used while the object is held.
		 */
		public override bool CanToggleCursor ()
		{
			if (isChargingThrow || inRotationMode)
			{
				return false;
			}
			return true;
		}


		private Vector3 fixedJointOffset;
		private void SetRotationMode (bool on)
		{
			_rigidbody.velocity = Vector3.zero;
			_rigidbody.useGravity = !on;

			if (inRotationMode != on)
			{
				if (on)
				{
					KickStarter.playerInput.forceGameplayCursor = ForceGameplayCursor.KeepUnlocked;
					fixedJoint.connectedBody = null;
				}
				else
				{
					KickStarter.playerInput.forceGameplayCursor = ForceGameplayCursor.None;

					if (!KickStarter.playerInput.GetInGameCursorState ())
					{
						fixedJointOffset = GetWorldMousePosition () - fixedJoint.transform.position;
						deltaMovement = Vector3.zero;
					}
				}
			}

			inRotationMode = on;
		}


		/**
		 * <summary>Applies a drag force on the object, based on the movement of the cursor.</summary>
		 * <param name = "force">The force vector to apply</param>
		 * <param name = "_screenMousePosition">The position of the mouse</param>
		 * <param name = "_distanceToCamera">The distance between the object's centre and the camera</param>
		 */
		public override void ApplyDragForce (Vector3 force, Vector3 _screenMousePosition, float _distanceToCamera)
		{
			distanceToCamera = _distanceToCamera;

			if (inRotationMode)
			{
				// Scale force
				force *= speedFactor * _rigidbody.drag * distanceToCamera * Time.deltaTime;
				
				// Limit magnitude
				if (force.magnitude > maxSpeed)
				{
					force *= maxSpeed / force.magnitude;
				}

				Vector3 newRot = Vector3.Cross (force, cameraTransform.forward);
				newRot /= Mathf.Sqrt ((grabPoint.position - transform.position).magnitude) * 2.4f * rotationFactor;
				_rigidbody.AddTorque (newRot);
			}
			else
			{
				UpdateFixedJoint ();
			}
		}


		private void Update ()
		{
			if (!isHeld) return;

			if (allowThrow)
			{
				if (KickStarter.playerInput.InputGetButton ("ThrowMoveable"))
				{
					ChargeThrow ();
				}
				else if (isChargingThrow)
				{
					ReleaseThrow ();
				}
			}

			if (allowRotation)
			{
				if (KickStarter.playerInput.InputGetButton ("RotateMoveable"))
				{
					SetRotationMode (true);
				}
				else if (KickStarter.playerInput.InputGetButtonUp ("RotateMoveable"))
				{
					SetRotationMode (false);
					return;
				}

				if (KickStarter.playerInput.InputGetButtonDown ("RotateMoveableToggle"))
				{
					SetRotationMode (!inRotationMode);
					if (!inRotationMode)
					{
						return;
					}
				}
			}

			if (allowZooming)
			{
				UpdateZoom ();
			}
		}


		private void LateUpdate ()
		{
			if (!isHeld || inRotationMode) return;

			worldMousePosition = GetWorldMousePosition ();
		
			Vector3 deltaPositionRaw = (worldMousePosition - fixedJointOffset - fixedJoint.transform.position) * 100f;
			deltaMovement = Vector3.Lerp (deltaMovement, deltaPositionRaw, Time.deltaTime * 2f);
		}


		private void UpdateFixedJoint ()
		{
			if (fixedJoint)
			{
				fixedJoint.transform.position = fixedJointLerp.Update (fixedJoint.transform.position, worldMousePosition - fixedJointOffset, 10f);

				if (!inRotationMode && fixedJoint.connectedBody != _rigidbody)
				{
					fixedJoint.connectedBody = _rigidbody;
				}
			}
		}


		new private void UpdateZoom ()
		{
			float zoom = Input.GetAxis ("ZoomMoveable");

			if ((originalDistanceToCamera <= minZoom && zoom < 0f) || (originalDistanceToCamera >= maxZoom && zoom > 0f))
			{}
			else
			{
				originalDistanceToCamera += (zoom * zoomSpeed / 10f * Time.deltaTime);
			}

			originalDistanceToCamera = Mathf.Clamp (originalDistanceToCamera, minZoom, maxZoom);
		}


		/**
		 * Unsets the FixedJoint used to hold the object in place
		 */
		public void UnsetFixedJoint ()
		{
			fixedJoint = null;
			isHeld = false;
		}


		protected void LimitCollisions ()
		{
			Collider[] ownColliders = GetComponentsInChildren <Collider>();

			foreach (Collider _collider1 in ownColliders)
			{
				foreach (Collider _collider2 in ownColliders)
				{
					if (_collider1 == _collider2)
					{
						continue;
					}
					Physics.IgnoreCollision (_collider1, _collider2, true);
					Physics.IgnoreCollision (_collider1, _collider2, true);
				}

				if (ignorePlayerCollider && KickStarter.player != null)
				{
					Collider[] playerColliders = KickStarter.player.gameObject.GetComponentsInChildren <Collider>();
					foreach (Collider playerCollider in playerColliders)
					{
						Physics.IgnoreCollision (playerCollider, _collider1, true);
					}
				}
			}

		}


		private void OnCollisionEnter (Collision collision)
		{
			BaseOnCollisionEnter (collision);
		}
		
		
		private void OnDestroy ()
		{
			if (fixedJoint)
			{
				Destroy (fixedJoint.gameObject);
				fixedJoint = null;
			}
		}


		private Vector3 GetWorldMousePosition ()
		{
			Vector3 screenMousePosition = KickStarter.playerInput.GetMousePosition ();
			float alignedDistance = GetAlignedDistance (screenMousePosition);

			screenMousePosition.z = alignedDistance - (throwCharge * pullbackDistance);

			Vector3 pos = Camera.main.ScreenToWorldPoint (screenMousePosition);
			pos += Vector3.up * initialLift;

			return pos;
		}


		private float GetAlignedDistance (Vector3 screenMousePosition)
		{
			screenMousePosition.z = 1f;
			Vector3 tempWorldMousePosition = Camera.main.ScreenToWorldPoint (screenMousePosition);

			float angle = Vector3.Angle (Camera.main.transform.forward, tempWorldMousePosition - Camera.main.transform.position);

			return originalDistanceToCamera * Mathf.Cos (angle * Mathf.Deg2Rad);
		}

	}

}