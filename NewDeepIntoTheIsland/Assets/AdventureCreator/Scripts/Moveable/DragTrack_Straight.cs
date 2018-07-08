/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"DragTrack_Linear.cs"
 * 
 *	This track constrains Moveable_Drag objects to a straight line
 * 
 */

using UnityEngine;

namespace AC
{

	/**
	 * A track that constrains a Moveable_Drag object along a straight line.
	 * The dragged object can also be made to rotate as it moves: either so it rolls, or rotates around the line's axis (like a screw).
	 */
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_drag_track___straight.html")]
	#endif
	public class DragTrack_Straight : DragTrack
	{

		/** The way in which the Moveable_Drag object rotates as it moves (None, Roll, Screw) */
		public DragRotationType rotationType = DragRotationType.None;
		/** The track's length */
		public float maxDistance = 2f;
		/** If True, and the Moveable_Drag object rotates like a screw, then the input drag vector must also rotate, so that it is always tangential to the dragged object */
		public bool dragMustScrew = false;
		/** The "thread" if the Moveable_Drag object rotates like a screw - effectively how fast the object rotates as it moves */
		public float screwThread = 1f;


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

			draggable.maxCollider.transform.position = transform.position + (transform.up * maxDistance) + (transform.up * draggable.colliderRadius);
			draggable.minCollider.transform.position = transform.position - (transform.up * draggable.colliderRadius);
			
			draggable.minCollider.transform.up = transform.up;
			draggable.maxCollider.transform.up = -transform.up;

			base.AssignColliders (draggable);
		}


		/**
		 * <summary>Connects an object to the track when the game begins.</summary>
		 * <param name = "draggable">The Moveable_Drag object to connect to the track</param>
		 */
		public override void Connect (Moveable_Drag draggable)
		{
			AssignColliders (draggable);
		}


		/**
		 * <summary>Gets the proportion along the track that an object is positioned.</summary>
		 * <param name = "draggable">The Moveable_Drag object to check the position of</param>
		 * <returns>The proportion along the track that the Moveable_Drag object is positioned (0 to 1)</returns>
		 */
		public override float GetDecimalAlong (Moveable_Drag draggable)
		{
			return (draggable.transform.position - transform.position).magnitude / maxDistance;
		}


		/**
		 * <summary>Positions an object on a specific point along the track.</summary>
		 * <param name = "proportionAlong">The proportion along which to place the Moveable_Drag object (0 to 1)</param>
		 * <param name = "draggable">The Moveable_Drag object to reposition</param>
		 */
		public override void SetPositionAlong (float proportionAlong, Moveable_Drag draggable)
		{
			draggable.transform.position = transform.position + (transform.up * proportionAlong * maxDistance);

			if (rotationType != DragRotationType.None)
			{
				SetRotation (draggable, proportionAlong);
			}
		}


		/**
		 * <summary>Corrects the position of an object so that it is placed along the track.</summary>
		 * <param name = "draggable">The Moveable_Drag object to snap onto the track</param>
		 * <param name = "onStart">Is True if the game has just begun (i.e. this function is being run for the first time)</param>
		 */
		public override void SnapToTrack (Moveable_Drag draggable, bool onStart)
		{
			Vector3 vec = draggable.transform.position - transform.position;
			float proportionAlong = Vector3.Dot (vec, transform.up) / maxDistance;

			if (onStart)
			{
				if (proportionAlong < 0f)
				{
					proportionAlong = 0f;
				}
				else if (proportionAlong > 1f)
				{
					proportionAlong = 1f;
				}

				if (rotationType != DragRotationType.None)
				{
					SetRotation (draggable, proportionAlong);
				}

				draggable._rigidbody.velocity = draggable._rigidbody.angularVelocity = Vector3.zero;
			}

			draggable.transform.position = transform.position + transform.up * proportionAlong * maxDistance;
		}


		/**
		 * <summary>Applies a force that, when applied every frame, pushes an object connected to the track towards a specific point along it.</summary>
		 * <param name = "_position">The proportiona along which to place the Moveable_Drag object (0 to 1)</param>
		 */
		public override void ApplyAutoForce (float _position, float _speed, Moveable_Drag draggable)
		{
			Vector3 tangentForce = transform.up * _speed;
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
			float dotProduct = 0f;

			if (rotationType == DragRotationType.Screw)
			{
				if (dragMustScrew)
				{
					draggable.UpdateScrewVector ();
				}

				dotProduct = Vector3.Dot (force, draggable._dragVector);
			}
			else
			{
				dotProduct = Vector3.Dot (force, transform.up);
			}

			// Calculate the amount of force along the tangent
			Vector3 tangentForce = transform.up * dotProduct;

			if (rotationType == DragRotationType.Screw)
			{
				if (dragMustScrew)
				{
					// Take radius into account
					tangentForce = (transform.up * dotProduct).normalized * force.magnitude;
					tangentForce /= Mathf.Sqrt ((draggable.GetGrabPosition () - draggable.transform.position).magnitude) / 0.4f;
				}
				tangentForce /= Mathf.Sqrt (screwThread);
			}

			draggable._rigidbody.AddForce (tangentForce);
		}


		/**
		 * <summary>Checks if the icon that can display when an object is moved along the track remains in the same place as the object moves.</summary>
		 * <returns>True if the icon remains in the same place (dependent on the way the object rotates as it moves)</returns>
		 */
		public override bool IconIsStationary ()
		{
			if (rotationType == DragRotationType.Roll || (rotationType == DragRotationType.Screw && !dragMustScrew))
			{
				return true;
			}
			return false;
		}


		/**
		 * <summary>Updates the position of an object connected to the track. This is called every frame.</summary>
		 * <param name = "draggable">The Moveable_Drag object to update the position of</param>
		 */
		public override void UpdateDraggable (Moveable_Drag draggable)
		{
			SnapToTrack (draggable, false);
			draggable.trackValue = GetDecimalAlong (draggable);

			if (rotationType != DragRotationType.None)
			{
				SetRotation (draggable, draggable.trackValue);
			}
		}


		private void SetRotation (Moveable_Drag draggable, float proportionAlong)
		{
			float angle = proportionAlong * maxDistance / draggable.colliderRadius / 2f * Mathf.Rad2Deg;

			if (rotationType == DragRotationType.Roll)
			{
				draggable._rigidbody.rotation = Quaternion.AngleAxis (angle, transform.forward) * transform.rotation;
			}
			else if (rotationType == DragRotationType.Screw)
			{
				draggable._rigidbody.rotation = Quaternion.AngleAxis (angle * screwThread, transform.up) * transform.rotation;
			}
		}

	}
	
}