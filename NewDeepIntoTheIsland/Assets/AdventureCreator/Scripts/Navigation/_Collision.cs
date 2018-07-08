/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"_Collision.cs"
 * 
 *	This script allows colliders that block the Player's movement
 *	to be turned on and off easily via actions.
 * 
 */

using UnityEngine;

namespace AC
{

	/**
	 * Provides functions to easily turn Collider components on and off, either through script or with the "Object: Send message" Action.
	 * This script is attached to AC's Collider prefabs.
	 */
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1___collision.html")]
	#endif
	public class _Collision : MonoBehaviour
	{

		/** If True, then a Gizmo will be drawn in the Scene window at the Collider's position */
		[HideInInspector] public bool showInEditor = false;


		/**
		 * Enables 3D and 2D colliders attached to the GameObject, and places it on the Hotspot (Default) layer - causing it to block Hotspot raycasts.
		 */
		public void TurnOn ()
		{
			if (GetComponent <Collider>())
			{
				GetComponent <Collider>().enabled = true;
			}
			else if (GetComponent <Collider2D>())
			{
				GetComponent <Collider2D>().enabled = true;
			}
			gameObject.layer = LayerMask.NameToLayer (KickStarter.settingsManager.hotspotLayer);
		}
		

		/**
		 * Disables 3D and 2D colliders attached to the GameObject, and places it on the Deactivated (Ignore Raycast) layer - allowing Hotspot raycasts to pass through it.
		 */
		public void TurnOff ()
		{
			if (GetComponent <Collider>())
			{
				GetComponent <Collider>().enabled = false;
			}
			else if (GetComponent <Collider2D>())
			{
				GetComponent <Collider2D>().enabled = false;
			}
			gameObject.layer = LayerMask.NameToLayer (KickStarter.settingsManager.deactivatedLayer);
		}


		#if UNITY_EDITOR
		
		private void OnDrawGizmos ()
		{
			if (showInEditor)
			{
				DrawGizmos ();
			}
		}
		
		
		private void OnDrawGizmosSelected ()
		{
			DrawGizmos ();
		}
		
		
		private void DrawGizmos ()
		{
			AdvGame.DrawCubeCollider (transform, new Color (0f, 1f, 1f, 0.8f));
		}

		#endif
		
	}

}