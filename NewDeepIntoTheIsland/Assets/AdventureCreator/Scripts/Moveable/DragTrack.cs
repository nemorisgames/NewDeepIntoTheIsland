/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"DragTrack.cs"
 * 
 *	The base class for "tracks", which are used to
 *	constrain Moveable_Drag objects along set paths
 * 
 */

using UnityEngine;

namespace AC
{

	/**
	 * The base class for "tracks", which are used to contrain Moveable_Drag objects along a pre-determined path
	 */
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_drag_track.html")]
	#endif
	public class DragTrack : MonoBehaviour
	{

		/** The Physics Material to give the track's end colliders */
		public PhysicMaterial colliderMaterial;
		/** The size of the track's end colliders, as seen in the Scene window */
		public float discSize = 0.2f;
		/** The colour of Scene window Handles */
		public Color handleColour = Color.white;


		/**
		 * <summary>Initialises two end colliders for an object that prevent it from moving beyond the track.</summary>
		 * <param name = "draggable">The Moveable_Drag object to create colliders for</param>
		 */
		public virtual void AssignColliders (Moveable_Drag draggable)
		{
			if (draggable.minCollider != null && draggable.maxCollider != null)
			{
				draggable.maxCollider.transform.rotation = Quaternion.AngleAxis (90f, draggable.maxCollider.transform.right) * draggable.maxCollider.transform.rotation;
				draggable.minCollider.transform.rotation = Quaternion.AngleAxis (90f, draggable.minCollider.transform.right) * draggable.minCollider.transform.rotation;

				if (colliderMaterial)
				{
					draggable.maxCollider.material = colliderMaterial;
					draggable.minCollider.material = colliderMaterial;
				}

				draggable.maxCollider.transform.parent = this.transform;
				draggable.minCollider.transform.parent = this.transform;

				draggable.maxCollider.name = draggable.name + "_UpperLimit";
				draggable.minCollider.name = draggable.name + "_LowerLimit";
			}

			LimitCollisions (draggable);
		}


		/**
		 * <summary>Gets the proportion along the track that an object is positioned.</summary>
		 * <param name = "draggable">The Moveable_Drag object to check the position of</param>
		 * <returns>The proportion along the track that the Moveable_Drag object is positioned (0 to 1)</returns>
		 */
		public virtual float GetDecimalAlong (Moveable_Drag draggable)
		{
			return 0f;
		}


		/**
		 * <summary>Positions an object on a specific point along the track.</summary>
		 * <param name = "proportionAlong">The proportion along which to place the Moveable_Drag object (0 to 1)</param>
		 * <param name = "draggable">The Moveable_Drag object to reposition</param>
		 */
		public virtual void SetPositionAlong (float proportionAlong, Moveable_Drag draggable)
		{}


		/**
		 * <summary>Connects an object to the track when the game begins.</summary>
		 * <param name = "draggable">The Moveable_Drag object to connect to the track</param>
		 */
		public virtual void Connect (Moveable_Drag draggable)
		{}


		/**
		 * <summary>Applies a force to an object connected to the track.</summary>
		 * <param name = "force">The drag force vector input by the player</param>
		 * <param name = "draggable">The Moveable_Drag object to apply the force to</param>
		 */
		public virtual void ApplyDragForce (Vector3 force, Moveable_Drag draggable)
		{}


		/**
		 * <summary>Applies a force that, when applied every frame, pushes an object connected to the track towards a specific point along it.</summary>
		 * <param name = "_position">The proportiona along which to place the Moveable_Drag object (0 to 1)</param>
		 */
		public virtual void ApplyAutoForce (float _position, float _speed, Moveable_Drag draggable)
		{}


		/**
		 * <summary>Updates the position of an object connected to the track. This is called every frame.</summary>
		 * <param name = "draggable">The Moveable_Drag object to update the position of</param>
		 */
		public virtual void UpdateDraggable (Moveable_Drag draggable)
		{
			draggable.trackValue = GetDecimalAlong (draggable);
		}


		/**
		 * <summary>Corrects the position of an object so that it is placed along the track.</summary>
		 * <param name = "draggable">The Moveable_Drag object to snap onto the track</param>
		 * <param name = "onStart">Is True if the game has just begun (i.e. this function is being run for the first time)</param>
		 */
		public virtual void SnapToTrack (Moveable_Drag draggable, bool onStart)
		{}


		protected void LimitCollisions (Moveable_Drag draggable)
		{
			Collider[] allColliders = FindObjectsOfType (typeof(Collider)) as Collider[];
			Collider[] dragColliders = draggable.GetComponentsInChildren <Collider>();

			// Disable all collisions on max/min colliders
			if (draggable.minCollider != null && draggable.maxCollider != null)
			{
				foreach (Collider _collider in allColliders)
				{
					if (_collider.enabled)
					{
						if (_collider != draggable.minCollider && draggable.minCollider.enabled)
						{
							Physics.IgnoreCollision (_collider, draggable.minCollider, true);
						}
						if (_collider != draggable.maxCollider && draggable.maxCollider.enabled)
						{
							Physics.IgnoreCollision (_collider, draggable.maxCollider, true);
						}
					}
				}
			}

			// Set collisions on draggable's colliders
			foreach (Collider _collider in allColliders)
			{
				foreach (Collider dragCollider in dragColliders)
				{
					if (_collider == dragCollider)
					{
						continue;
					}

					bool result = true;

					if ((draggable.minCollider != null && draggable.minCollider == _collider) || (draggable.maxCollider != null && draggable.maxCollider == _collider))
					{
						result = false;
					}
					else if (_collider.gameObject.tag == Tags.player)
					{
						result = draggable.ignorePlayerCollider;
					}
					else if (_collider.GetComponent <Rigidbody>() && _collider.gameObject != draggable.gameObject)
					{
						if (_collider.GetComponent <Moveable>())
						{
							result = draggable.ignoreMoveableRigidbodies;
						}
						else
						{
							result = false;
						}
					}

					if (_collider.enabled && dragCollider.enabled)
					{
						Physics.IgnoreCollision (_collider, dragCollider, result);
					}
				}
			}

			// Enable collisions between max/min collisions and draggable's colliders
			if (draggable.minCollider != null && draggable.maxCollider != null)
			{
				foreach (Collider _collider in dragColliders)
				{
					if (_collider.enabled && draggable.minCollider.enabled)
					{
						Physics.IgnoreCollision (_collider, draggable.minCollider, false);
					}
					if (_collider.enabled && draggable.maxCollider.enabled)
					{
						Physics.IgnoreCollision (_collider, draggable.maxCollider, false);
					}
				}
			}
		}


		/**
		 * <summary>Checks if the icon that can display when an object is moved along the track remains in the same place as the object moves.</summary>
		 * <returns>True if the icon remains in the same place (always False unless overridden by subclasses)</returns>
		 */
		public virtual bool IconIsStationary ()
		{
			return false;
		}

	}

}
