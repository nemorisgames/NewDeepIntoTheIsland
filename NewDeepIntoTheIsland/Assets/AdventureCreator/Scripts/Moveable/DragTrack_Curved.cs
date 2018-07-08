/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"DragTrack_Curved.cs"
 * 
 *	This track constrains Moveable_Drag objects to a circular ring.
 * 
 */

using UnityEngine;

namespace AC
{

	/**
	 * A track that constrains Moveable_Drag objects to a circular ring.
	 * Unlike a hinge track (see DragTrack_Hinge), the object will be translated as well as rotated.
	 */
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_drag_track___curved.html")]
	#endif
	public class DragTrack_Curved : DragTrack
	{

		/** The angle of the tracks's curve */
		public float maxAngle = 60f;
		/** The track's radius */
		public float radius = 2f;
		/** If True, then the track forms a complete loop */
		public bool doLoop = false;

		private Vector3 startPosition;


		/**
		 * <summary>Initialises two end colliders for an object that prevent it from moving beyond the track.</summary>
		 * <param name = "draggable">The Moveable_Drag object to create colliders for</param>
		 */
		public override void AssignColliders (Moveable_Drag draggable)
		{
			if (draggable.maxCollider == null)
			{
				draggable.maxCollider = (Collider) Instantiate (Resources.Load (Resource.dragCollider, typeof (Collider)));
			}
			
			if (draggable.minCollider == null)
			{
				draggable.minCollider = (Collider) Instantiate (Resources.Load (Resource.dragCollider, typeof (Collider)));
			}

			if (maxAngle > 360f)
			{
				maxAngle = 360f;
			}

			float offsetAngle = Mathf.Asin (draggable.colliderRadius / radius) * Mathf.Rad2Deg;

			draggable.maxCollider.transform.position = startPosition;
			draggable.maxCollider.transform.up = -transform.up;
			draggable.maxCollider.transform.RotateAround (transform.position, transform.forward, maxAngle + offsetAngle);

			draggable.minCollider.transform.position = startPosition;
			draggable.minCollider.transform.up = transform.up;
			draggable.minCollider.transform.RotateAround (transform.position, transform.forward, -offsetAngle);

			base.AssignColliders (draggable);
		}


		/**
		 * <summary>Gets the proportion along the track that an object is positioned.</summary>
		 * <param name = "draggable">The Moveable_Drag object to check the position of</param>
		 * <param name = "The proportion along the track that the Moveable_Drag object is positioned (0 to 1)</returns>
		 */
		public override void Connect (Moveable_Drag draggable)
		{
			if (draggable._rigidbody.useGravity)
			{
				draggable._rigidbody.useGravity = false;
				ACDebug.LogWarning ("Curved tracks do not work with Rigidbodys that obey gravity - disabling");
			}

			startPosition = transform.position + (radius * transform.right);
			
			if (doLoop)
			{
				maxAngle = 360f;
				base.AssignColliders (draggable);
				return;
			}

			AssignColliders (draggable);
		}


		/**
		 * <summary>Applies a force that, when applied every frame, pushes an object connected to the track towards a specific point along it.</summary>
		 * <param name = "_position">The proportiona along which to place the Moveable_Drag object (0 to 1)</param>
		 */
		public override void ApplyAutoForce (float _position, float _speed, Moveable_Drag draggable)
		{
			Vector3 tangentForce = draggable.transform.up * _speed;
			if (draggable.trackValue < _position)
			{
				draggable._rigidbody.AddForce (tangentForce * Time.deltaTime);
			}
			else
			{
				draggable._rigidbody.AddForce (-tangentForce * Time.deltaTime);
			}
		}


		/**
		 * <summary>Applies a force to an object connected to the track.</summary>
		 * <param name = "force">The drag force vector input by the player</param>
		 * <param name = "draggable">The Moveable_Drag object to apply the force to</param>
		 */
		public override void ApplyDragForce (Vector3 force, Moveable_Drag draggable)
		{
			float dotProduct = Vector3.Dot (force, draggable.transform.up);
			
			// Calculate the amount of force along the tangent
			Vector3 tangentForce = draggable.transform.up * dotProduct;
			draggable._rigidbody.AddForce (tangentForce);
		}


		/**
		 * <summary>Positions an object on a specific point along the track.</summary>
		 * <param name = "proportionAlong">The proportion along which to place the Moveable_Drag object (0 to 1)</param>
		 * <param name = "draggable">The Moveable_Drag object to reposition</param>
		 */
		public override void SetPositionAlong (float proportionAlong, Moveable_Drag draggable)
		{
			Quaternion rotation = Quaternion.AngleAxis (proportionAlong * maxAngle, transform.forward);
			draggable.transform.position = RotatePointAroundPivot (startPosition, transform.position, rotation);
			draggable.transform.rotation = Quaternion.AngleAxis (proportionAlong * maxAngle, transform.forward) * transform.rotation;

			if (!doLoop)
			{
				UpdateColliders (proportionAlong, draggable);
			}
		}


		private Vector3 RotatePointAroundPivot (Vector3 point, Vector3 pivot, Quaternion angle)
		{
			return angle * (point - pivot) + pivot;
		}


		/**
		 * <summary>Gets the proportion along the track that an object is positioned.</summary>
		 * <param name = "draggable">The Moveable_Drag object to check the position of</param>
		 * <returns>The proportion along the track that the Moveable_Drag object is positioned (0 to 1)</returns>
		 */
		public override float GetDecimalAlong (Moveable_Drag draggable)
		{
			float angle = Vector3.Angle (-transform.right, draggable.transform.position - transform.position);

			// Sign of angle?
			if (angle < 170f && Vector3.Dot (draggable.transform.position - transform.position, transform.up) < 0f)
			{
				angle *= -1f;
			}

			return ((180f - angle) / maxAngle);
		}


		/**
		 * <summary>Corrects the position of an object so that it is placed along the track.</summary>
		 * <param name = "draggable">The Moveable_Drag object to snap onto the track</param>
		 * <param name = "onStart">Is True if the game has just begun (i.e. this function is being run for the first time)</param>
		 */
		public override void SnapToTrack (Moveable_Drag draggable, bool onStart)
		{
			Vector3 LookAt = draggable.transform.position - transform.position;

			draggable.transform.position = transform.position + LookAt / (LookAt.magnitude / radius);

			if (onStart)
			{
				float proportionAlong = GetDecimalAlong (draggable);

				if (proportionAlong < 0f)
				{
					proportionAlong = 0f;
				}
				else if (proportionAlong > 1f)
				{
					proportionAlong = 1f;
				}

				draggable.transform.rotation = Quaternion.AngleAxis (proportionAlong * maxAngle, transform.forward) * transform.rotation;
				SetPositionAlong (proportionAlong, draggable);
			}
			else
			{
				draggable.transform.rotation = Quaternion.AngleAxis (draggable.trackValue * maxAngle, transform.forward) * transform.rotation;
			}
		}


		/**
		 * <summary>Updates the position of an object connected to the track. This is called every frame.</summary>
		 * <param name = "draggable">The Moveable_Drag object to update the position of</param>
		 */
		public override void UpdateDraggable (Moveable_Drag draggable)
		{
			draggable.trackValue = GetDecimalAlong (draggable);

			SnapToTrack (draggable, false);

			if (!doLoop)
			{
				UpdateColliders (draggable.trackValue, draggable);
			}
		}


		private void UpdateColliders (float trackValue, Moveable_Drag draggable)
		{
			if (trackValue > 1f)
			{
				return;
			}

			if (trackValue > 0.5f)
			{
				draggable.minCollider.enabled = false;
				draggable.maxCollider.enabled = true;
			}
			else
			{
				draggable.minCollider.enabled = true;
				draggable.maxCollider.enabled = false;
			}
		}

	}

}