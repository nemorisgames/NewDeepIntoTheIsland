/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"DragTrack_Hinge.cs"
 * 
 *	This track fixes a Moveable_Drag's position, so it can only be rotated
 *	in a circle.
 * 
 */

using UnityEngine;

namespace AC
{

	/**
	 * A track that constrains a Moveable_Drag's position, so that it can only be rotated.
	 * This makes it suitable for objects that pivot, such as levers, doors, etc.
	 */
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_drag_track___hinge.html")]
	#endif
	public class DragTrack_Hinge : DragTrack
	{
	
		/** How much an object can be rotated by */
		public float maxAngle = 60f;
		/** The track's radius (for visualising in the Scene window) */
		public float radius = 2f;
		/** If True, then objects can be rotated a full revolution */
		public bool doLoop = false;
		/** If True, and doLoop = True, then the number of revolutions an object can rotate is limited */
		public bool limitRevolutions = false;
		/** If limitRevolutions = True, the maximum number of revolutions an object can be rotated by */
		public int maxRevolutions = 0;
		/** If True, then the calculated drag vector will be based on the track's orientation, rather than the object being rotated, so that the input drag vector will always need to be the same direction */
		public bool alignDragToFront = false;


		/**
		 * Overrides the base (DragTrack) AssignColliders() function and does nothing.
		 */
		public override void AssignColliders (Moveable_Drag draggable)
		{
			return;
		}
		

		/**
		 * <summary>Connects an object to the track when the game begins.</summary>
		 * <param name = "draggable">The Moveable_Drag object to connect to the track</param>
		 */
		public override void Connect (Moveable_Drag draggable)
		{
			LimitCollisions (draggable);

			if (doLoop)
			{
				maxAngle = 360f;
			}
		}


		/**
		 * <summary>Applies a force that, when applied every frame, pushes an object connected to the track towards a specific point along it.</summary>
		 * <param name = "_position">The proportiona along which to place the Moveable_Drag object (0 to 1)</param>
		 */
		public override void ApplyAutoForce (float _position, float _speed, Moveable_Drag draggable)
		{
			Vector3 tangentForce = draggable.transform.forward * _speed;
			if (draggable.trackValue < _position)
			{
				draggable._rigidbody.AddTorque (tangentForce * Time.deltaTime);
			}
			else
			{
				draggable._rigidbody.AddTorque (-tangentForce * Time.deltaTime);
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
			Vector3 axisOffset = Vector2.zero;

			if (!alignDragToFront)
			{
				dotProduct = Vector3.Dot (force, draggable.transform.up);

				// Invert force if on the "back" side
				axisOffset = GetAxisOffset (draggable.GetGrabPosition ());
				if (Vector3.Dot (draggable.transform.right, axisOffset) < 0f)
				{
					dotProduct *= -1f;
				}
			}
			else
			{
				// Use the Hinge's transform, not the Draggable's
				dotProduct = Vector3.Dot (force, transform.up);

				// Invert force if on the "back" side
				axisOffset = GetAxisOffset (draggable._dragVector);
				if (Vector3.Dot (transform.right, axisOffset) < 0f)
				{
					dotProduct *= -1f;
				}
			}

			// Calculate the amount of force along the tangent
			Vector3 tangentForce = (draggable.transform.forward * dotProduct).normalized;
			tangentForce *= force.magnitude;

			// Take radius into account
			tangentForce /= axisOffset.magnitude / 0.43f;

			draggable._rigidbody.AddTorque (tangentForce);
		}


		private Vector3 GetAxisOffset (Vector3 grabPosition)
		{
			float dist = Vector3.Dot (grabPosition, transform.forward);
			Vector3 axisPoint = transform.position + (transform.forward * dist);
			return (grabPosition - axisPoint);
		}
		

		/**
		 * <summary>Positions an object on a specific point along the track.</summary>
		 * <param name = "proportionAlong">The proportion along which to place the Moveable_Drag object (0 to 1)</param>
		 * <param name = "draggable">The Moveable_Drag object to reposition</param>
		 */
		public override void SetPositionAlong (float proportionAlong, Moveable_Drag draggable)
		{
			draggable.transform.position = transform.position;
			draggable.transform.rotation = Quaternion.AngleAxis (proportionAlong * maxAngle, transform.forward) * transform.rotation;
		}
		

		/**
		 * <summary>Gets the proportion along the track that an object is positioned.</summary>
		 * <param name = "draggable">The Moveable_Drag object to check the position of</param>
		 * <returns>The proportion along the track that the Moveable_Drag object is positioned (0 to 1)</returns>
		 */
		public override float GetDecimalAlong (Moveable_Drag draggable)
		{
			float angle = Vector3.Angle (transform.up, draggable.transform.up);

			if (Vector3.Dot (-transform.right, draggable.transform.up) < 0f)
			{
				angle = 360f - angle;
			}
			if (angle > 180f + maxAngle / 2f)
			{
				angle = 0f;
			}

			return (angle / maxAngle);
		}
		

		/**
		 * <summary>Corrects the position of an object so that it is placed along the track.</summary>
		 * <param name = "draggable">The Moveable_Drag object to snap onto the track</param>
		 * <param name = "onStart">Is True if the game has just begun (i.e. this function is being run for the first time)</param>
		 */
		public override void SnapToTrack (Moveable_Drag draggable, bool onStart)
		{
			draggable.transform.position = transform.position;

			if (onStart)
			{
				draggable.transform.rotation = transform.rotation;
				draggable.trackValue = 0f;
			}
		}
		

		/**
		 * <summary>Updates the position of an object connected to the track. This is called every frame.</summary>
		 * <param name = "draggable">The Moveable_Drag object to update the position of</param>
		 */
		public override void UpdateDraggable (Moveable_Drag draggable)
		{
			float oldValue = draggable.trackValue;

			draggable.transform.position = transform.position;
			draggable.trackValue = GetDecimalAlong (draggable);

			if (draggable.trackValue <= 0f || draggable.trackValue > 1f)
			{
				if (draggable.trackValue < 0f)
				{
					draggable.trackValue = 0f;
				}
				else if (draggable.trackValue > 1f)
				{
					draggable.trackValue = 1f;
				}

				SetPositionAlong (draggable.trackValue, draggable);
				draggable._rigidbody.angularVelocity = Vector3.zero;
			}

			if (doLoop && limitRevolutions)
			{
				if (oldValue < 0.1f && draggable.trackValue > 0.9f)
				{
					draggable.revolutions --;
				}
				else if (oldValue > 0.9f && draggable.trackValue < 0.1f)
				{
					draggable.revolutions ++;
				}

				if (draggable.revolutions < 0)
				{
					draggable.revolutions = 0;
					draggable.trackValue = 0f;
					SetPositionAlong (draggable.trackValue, draggable);
					draggable._rigidbody.angularVelocity = Vector3.zero;
				}
				else if (draggable.revolutions > maxRevolutions - 1)
				{
					draggable.revolutions = maxRevolutions - 1;
					draggable.trackValue = 1f;
					SetPositionAlong (draggable.trackValue, draggable);
					draggable._rigidbody.angularVelocity = Vector3.zero;
				}
			}
		}

	}

}
