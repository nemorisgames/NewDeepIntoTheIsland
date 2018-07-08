/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"Moveable_Drag.cs"
 * 
 *	Attach this script to a GameObject to make it
 *	moveable according to a set method, either
 *	by the player or through Actions.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	/**
	 * Attaching this component to a GameObject allows it to be dragged, through physics, according to a set method.
	 */
	[RequireComponent (typeof (Rigidbody))]
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_moveable___drag.html")]
	#endif
	public class Moveable_Drag : DragBase
	{

		/** The way in which the object can be dragged (LockedToTrack, MoveAlongPlane, RotateOnly) */
		public DragMode dragMode = DragMode.LockToTrack;
		/** The DragTrack the object is locked to (if dragMode = DragMode.LockToTrack */
		public DragTrack track;
		/** If True, and dragMode = DragMode.LockToTrack, then the position and rotation of all child objects will be maintained when the object is attached to the track */
		public bool retainOriginalTransform = false;

		/** If True, and the object is locked to a DragTrack, then the object will be placed at a specific point along the track when the game begins */
		public bool setOnStart = true;
		/** How far along its DragTrack that the object should be placed at when the game begins */
		public float trackValueOnStart = 0f;
		/** The Interaction to run whenever the object is moved by the player */
		public Interaction interactionOnMove = null;

		/** What movement is aligned to, if dragMode = DragMode.MoveAlongPlane (AlignToCamera, AlignToPlane) */
		public AlignDragMovement alignMovement = AlignDragMovement.AlignToCamera;
		/** The plane to align movement to, if alignMovement = AlignDragMovement.AlignToPlane */
		public Transform plane;
		/** If True, then gravity will be disabled on the object while it is held by the player */
		public bool noGravityWhenHeld = true;

		private LayerMask blockAutoLayerMask;
		private Vector3 grabPositionRelative;

		/** The radius of the GameObject's SphereCollider, if it has one */
		[HideInInspector] public float colliderRadius = 0.5f;
		/** How far along a track the object is, if it is locked to one */
		[HideInInspector] public float trackValue;
		/** A vector used in drag calculations */
		[HideInInspector] public Vector3 _dragVector;

		/** The upper-limit collider when locked to a DragTrack. */
		[HideInInspector] public Collider maxCollider;
		/** The lower-limit collider when locked to a DragTrack */
		[HideInInspector] public Collider minCollider;
		/** The number of revolutions the object has been rotated by, if placed on a DragTrack_Hinge */
		[HideInInspector] public int revolutions = 0;

		private bool canPlayCollideSound = false;
		private bool targetStartedGreater = false;
		private float targetTrackValue;
		private float targetTrackSpeed = 0f;


		protected override void Awake ()
		{
			base.Awake ();

			if (_rigidbody)
			{
				SetGravity (true);

				if (dragMode == DragMode.RotateOnly)
				{
					if (_rigidbody.constraints == RigidbodyConstraints.FreezeRotation ||
						_rigidbody.constraints == RigidbodyConstraints.FreezeRotationX ||
						_rigidbody.constraints == RigidbodyConstraints.FreezeRotationY ||
						_rigidbody.constraints == RigidbodyConstraints.FreezeRotationZ)
					{
						ACDebug.LogWarning ("Draggable " + gameObject.name + " has a Drag Mode of 'RotateOnly', but its rigidbody rotation is constrained. This may lead to inconsistent behaviour, and using a HingeTrack is advised instead.", gameObject);
					}
				}
			}

			if (GetComponent <SphereCollider>())
			{
				colliderRadius = GetComponent <SphereCollider>().radius * transform.localScale.x;
			}

			if (dragMode == DragMode.LockToTrack)
			{
				StartCoroutine (InitToTrack ());
			}

		}


		private IEnumerator InitToTrack ()
		{
			if (track != null)
			{
				ChildTransformData[] childTransformData = GetChildTransforms ();

				track.Connect (this);

				if (retainOriginalTransform)
				{
					track.SnapToTrack (this, true);
					SetChildTransforms (childTransformData);
					yield return new WaitForEndOfFrame ();
				}

				if (setOnStart)
				{
					track.SetPositionAlong (trackValueOnStart, this);
				}
				else
				{
					track.SnapToTrack (this, true);
				}
				trackValue = track.GetDecimalAlong (this);
			}
		}


		private class ChildTransformData
		{

			private Vector3 originalPosition;
			private Quaternion originalRotation;


			public ChildTransformData (Vector3 _originalPosition, Quaternion _originalRotation)
			{
				originalPosition = _originalPosition;
				originalRotation = _originalRotation;
			}


			public void UpdateTransform (Transform transform)
			{
				transform.position = originalPosition;
				transform.rotation = originalRotation;
			}
		}


		private ChildTransformData[] GetChildTransforms ()
		{
			List<ChildTransformData> childTransformData = new List<ChildTransformData>();
			for (int i=0; i<transform.childCount; i++)
			{
				Transform childTransform = transform.GetChild (i);
				childTransformData.Add (new ChildTransformData (childTransform.position, childTransform.rotation));
			}
			return childTransformData.ToArray ();
		}


		private void SetChildTransforms (ChildTransformData[] childTransformData)
		{
			for (int i=0; i<transform.childCount; i++)
			{
				Transform childTransform = transform.GetChild (i);
				childTransformData[i].UpdateTransform (childTransform);
			}
		}


		/**
		 * <summary>Gets how far the object is along its DragTrack.</summary>
		 * <returns>How far the object is along its DragTrack. This is normally 0 to 1, but if the object is locked to a looped DragTrack_Hinge, then the number of revolutions will be added to the result.</returns>
		 */
		public float GetPositionAlong ()
		{
			if (dragMode == DragMode.LockToTrack && track && track is DragTrack_Hinge)
			{
				return trackValue + (int) revolutions;
			}
			return trackValue;
		}


		/**
		 * Called every frame by StateHandler.
		 */
		public override void UpdateMovement ()
		{
			base.UpdateMovement ();
		
			if (dragMode == DragMode.LockToTrack && track)
			{
				if (track && (_rigidbody.angularVelocity != Vector3.zero || _rigidbody.velocity != Vector3.zero))
				{
					track.UpdateDraggable (this);

					if (interactionOnMove && gameObject.layer != LayerMask.NameToLayer (KickStarter.settingsManager.deactivatedLayer))
					{
						if (!KickStarter.actionListManager.IsListRunning (interactionOnMove))
						{
							interactionOnMove.Interact ();
						}
					}
				}
				else if (targetTrackSpeed > 0f)
				{
					trackValue = track.GetDecimalAlong (this);
				}

				if (targetTrackSpeed > 0f)
				{
					if ((Mathf.Approximately (targetTrackValue, 0f) && trackValue < 0.01f) ||
					    (Mathf.Approximately (targetTrackValue, 1f) && trackValue > 0.99f))
					{
						// Special case, since colliders cause ends to not quite be met
						StopAutoMove ();
					}
					else if ((targetStartedGreater && targetTrackValue > trackValue) || (!targetStartedGreater && targetTrackValue < trackValue))
					{
						track.ApplyAutoForce (targetTrackValue, targetTrackSpeed, this);
					}
					else
					{
						StopAutoMove ();
					}
				}

				if (collideSound && collideSoundClip && track is DragTrack_Hinge)
				{
					if (trackValue > 0.05f && trackValue < 0.95f)
					{
						canPlayCollideSound = true;
					}
					else if ((Mathf.Approximately (trackValue, 0f) || (!onlyPlayLowerCollisionSound && Mathf.Approximately (trackValue, 1f))) && canPlayCollideSound)
					{
						canPlayCollideSound = false;
						collideSound.Play (collideSoundClip, false);
					}
				}

				if (Mathf.Approximately (targetTrackSpeed, 0f) && !isHeld && (Mathf.Approximately (trackValue, 0f) || Mathf.Approximately (trackValue, 1f)))
				{
					_rigidbody.isKinematic = true;
				}
				else
				{
					_rigidbody.isKinematic = false;
				}
			}
			else if (isHeld)
			{
				if (dragMode == DragMode.RotateOnly && allowZooming && distanceToCamera > 0f)
				{
					LimitZoom ();
				}
			}

			if (moveSoundClip && moveSound)
			{
				if (dragMode == DragMode.LockToTrack && track && track is DragTrack_Hinge)
				{
					PlayMoveSound (_rigidbody.angularVelocity.magnitude, trackValue);
				}
				else
				{
					PlayMoveSound (_rigidbody.velocity.magnitude, trackValue);
				}
			}
		}


		/**
		 * Draws an icon at the point of contact on the object, if appropriate.
		 */
		public override void DrawGrabIcon ()
		{
			if (isHeld && showIcon && Camera.main.WorldToScreenPoint (transform.position).z > 0f && icon != null)
			{
				if (dragMode == DragMode.LockToTrack && track && track.IconIsStationary ())
				{
					Vector3 screenPosition = Camera.main.WorldToScreenPoint (grabPositionRelative + transform.position);
					icon.Draw (new Vector3 (screenPosition.x, screenPosition.y));
				}
				else
				{
					Vector3 screenPosition = Camera.main.WorldToScreenPoint (grabPoint.position);
					icon.Draw (new Vector3 (screenPosition.x, screenPosition.y));
				}
			}
		}


		/**
		 * <summary>Applies a drag force on the object, based on the movement of the cursor.</summary>
		 * <param name = "force">The force vector to apply</param>
		 * <param name = "mousePosition">The position of the mouse</param>
		 * <param name = "_distanceToCamera">The distance between the object's centre and the camera</param>
		 */
		public override void ApplyDragForce (Vector3 force, Vector3 mousePosition, float _distanceToCamera)
		{
			distanceToCamera = _distanceToCamera;

			// Scale force
			force *= speedFactor * _rigidbody.drag * distanceToCamera * Time.deltaTime;

			// Limit magnitude
			if (force.magnitude > maxSpeed)
			{
				force *= maxSpeed / force.magnitude;
			}

			if (dragMode == DragMode.LockToTrack)
			{
				if (track)
				{
					track.ApplyDragForce (force, this);
				}
			}
			else
			{
				Vector3 newRot = Vector3.Cross (force, cameraTransform.forward);

				if (dragMode == DragMode.MoveAlongPlane)
				{
					if (alignMovement == AlignDragMovement.AlignToPlane)
					{
						if (plane)
						{
							_rigidbody.AddForceAtPosition (Vector3.Cross (newRot, plane.up), transform.position + (plane.up * colliderRadius));
						}
						else
						{
							ACDebug.LogWarning ("No alignment plane assigned to " + this.name);
						}
					}
					else
					{
						_rigidbody.AddForceAtPosition (force, transform.position - (cameraTransform.forward * colliderRadius));
					}
				}
				else if (dragMode == DragMode.RotateOnly)
				{
					newRot /= Mathf.Sqrt ((grabPoint.position - transform.position).magnitude) * 2.4f * rotationFactor;
					_rigidbody.AddTorque (newRot);

					if (allowZooming)
					{
						UpdateZoom ();
					}
				}
			}
		}


		/**
		 * Detaches the object from the player's control.
		 */
		public override void LetGo ()
		{
			if (targetTrackSpeed <= 0)
			{
				// Not being auto-moved
				_rigidbody.drag = originalDrag;
				_rigidbody.angularDrag = originalAngularDrag;
			}

			SetGravity (true);

			if (dragMode == DragMode.RotateOnly)
			{
				_rigidbody.velocity = Vector3.zero;
			}
			
			base.LetGo ();
		}


		/**
		 * <summary>Attaches the object to the player's control.</summary>
		 * <param name = "grabPosition">The point of contact on the object</param>
		 */
		public override void Grab (Vector3 grabPosition)
		{
			if (targetTrackSpeed <= 0)
			{
				// Not being auto-moved
				originalDrag = _rigidbody.drag;
				originalAngularDrag = _rigidbody.angularDrag;
			}

			isHeld = true;
			grabPoint.position = grabPosition;
			grabPositionRelative = grabPosition - transform.position;
			_rigidbody.drag = 20f;
			_rigidbody.angularDrag = 20f;

			if (dragMode == DragMode.LockToTrack && track)
			{
				if (track is DragTrack_Straight)
				{
					UpdateScrewVector ();
				}
				else if (track is DragTrack_Hinge)
				{
					_dragVector = grabPosition;
				}
			}

			SetGravity (false);

			if (dragMode == DragMode.RotateOnly)
			{
				_rigidbody.velocity = Vector3.zero;
			}
		}
		
		
		private void SetGravity (bool value)
		{
			if (dragMode != DragMode.LockToTrack)
			{
				if (noGravityWhenHeld)
				{
					_rigidbody.useGravity = value;
				}
			}
		}


		/**
		 * If the object rotates like a screw along a DragTrack_Straight, this updates the correct drag vector.
		 */
		public void UpdateScrewVector ()
		{
			float forwardDot = Vector3.Dot (grabPoint.position - transform.position, transform.forward);
			float rightDot = Vector3.Dot (grabPoint.position - transform.position, transform.right);
			
			_dragVector = (transform.forward * -rightDot) + (transform.right * forwardDot);
		}


		/**
		 * <summary>Stops the object from moving without the player's direct input (i.e. through Actions).</summary>
		 * <param name = "snapToTarget">If True, then the object will snap instantly to the intended target position</param>
		 */
		public void StopAutoMove (bool snapToTarget = true)
		{
			StopAllCoroutines ();

			targetTrackSpeed = 0f;
			if (snapToTarget)
			{
				track.SetPositionAlong (targetTrackValue, this);
			}
			if (!_rigidbody.isKinematic)
			{
				_rigidbody.velocity = Vector3.zero;
				_rigidbody.angularVelocity = Vector3.zero;
			}
			_rigidbody.drag = originalDrag;
			_rigidbody.angularDrag = originalAngularDrag;
		}


		/**
		 * <summary>Checks if the object is moving without the player's direct input.</summary>
		 * <returns>True if the object is moving without the player's direct input (gravity doesn't count).</returns>
		 */
		public bool IsAutoMoving ()
		{
			if (targetTrackSpeed > 0f)
			{
				return true;
			}
			return false;
		}


		/**
		 * <summary>Forces the object to move along a DragTrack without the player's direct input.</summary>
		 * <param name = "_targetTrackValue">The intended proportion along the track to send the object to</param>
		 * <param name = "_targetTrackSpeed">The intended speed to move the object by</param>
		 * <param name = "removePlayerControl">If True and the player is currently moving the object, then player control will be removed</param>
		 */
		public void AutoMoveAlongTrack (float _targetTrackValue, float _targetTrackSpeed, bool removePlayerControl)
		{
			AutoMoveAlongTrack (_targetTrackValue, _targetTrackSpeed, removePlayerControl, 1 << 0);
		}


		/**
		 * <summary>Forces the object to move along a DragTrack without the player's direct input.</summary>
		 * <param name = "_targetTrackValue">The intended proportion along the track to send the object to</param>
		 * <param name = "_targetTrackSpeed">The intended speed to move the object by</param>
		 * <param name = "removePlayerControl">If True and the player is currently moving the object, then player control will be removed</param>
		 * <param name = "layerMask">A LayerMask that determines what collisions will cause the automatic movement to cease</param>
		 */
		public void AutoMoveAlongTrack (float _targetTrackValue, float _targetTrackSpeed, bool removePlayerControl, LayerMask layerMask)
		{
			if (dragMode == DragMode.LockToTrack && track != null)
			{
				blockAutoLayerMask = layerMask;

				if (_targetTrackSpeed < 0)
				{
					targetTrackSpeed = 0f;
				}
				else if (Mathf.Approximately (_targetTrackSpeed, 0))
				{
					targetTrackValue = _targetTrackValue;
					StopAutoMove ();
				}
				else
				{
					if (removePlayerControl)
					{
						isHeld = false;
					}

					targetTrackValue = _targetTrackValue;
					targetTrackSpeed = _targetTrackSpeed * 20f;

					if (targetTrackValue > trackValue)
					{
						targetStartedGreater = true;
					}
					else
					{
						targetStartedGreater = false;
					}

					if (!isHeld)
					{
						// Not being auto-moved
						originalDrag = _rigidbody.drag;
						originalAngularDrag = _rigidbody.angularDrag;
					}
					_rigidbody.drag = 20f;
					_rigidbody.angularDrag = 20f;
				}
			}
			else
			{
				ACDebug.LogWarning ("Cannot move " + this.name + " along a track, because no track has been assigned to it");
				targetTrackSpeed = 0f;
			}
		}


		private void OnCollisionEnter (Collision collision)
		{
			if ((blockAutoLayerMask.value & 1 << collision.gameObject.layer) != 0)
			{
				if (IsAutoMoving ())
				{
					StopAutoMove (false);
				}
			}

			BaseOnCollisionEnter (collision);
		}

	}
	
}