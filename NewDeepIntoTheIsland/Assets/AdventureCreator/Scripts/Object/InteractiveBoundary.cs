/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"InteractiveBoundary.cs"
 * 
 *	This script is used to limit Hotspot interactivity to players that are within a given volume.
 * 
 */

using UnityEngine;

namespace AC
{

	/**
	 * Used to limit a Hotspot's interactivity to Players that are within a given volume.
	 * Attach this to a Trigger collider, and assign in a Hotspot's Inspector. When assigned, the Hotspot will only be interactable when the Player is within the collider's boundary.
	 */
	[AddComponentMenu("Adventure Creator/Hotspots/Interactive boundary")]
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_interactive_boundary.html")]
	#endif
	public class InteractiveBoundary : MonoBehaviour
	{

		#region Variables

		private bool playerIsPresent;

		#endregion


		#region UnityStandards

		private void OnEnable ()
		{
			EventManager.OnSetPlayer += OnSwitchPlayer;
		}


		private void OnDisable ()
		{
			EventManager.OnSetPlayer -= OnSwitchPlayer;
		}


		private void OnSwitchPlayer (Player player)
		{
			playerIsPresent = false;
		}


		private void OnTriggerEnter (Collider other)
		{
			if (KickStarter.player != null && other.gameObject == KickStarter.player.gameObject)
			{
				playerIsPresent = true;
			}
        }


		private void OnTriggerExit (Collider other)
		{
			if (KickStarter.player != null && other.gameObject == KickStarter.player.gameObject)
			{
				playerIsPresent = false;
			}
        }


		private void OnTriggerStay2D (Collider2D other)
		{
			if (KickStarter.player != null && other.gameObject == KickStarter.player.gameObject)
			{
				playerIsPresent = true;
			}
		}


		private void OnTriggerExit2D (Collider2D other)
		{
			if (KickStarter.player != null && other.gameObject == KickStarter.player.gameObject)
			{
				playerIsPresent = false;
			}
		}

		#endregion


		#region GetSet

		/** True if the active Player is within the Collider boundary */
		public bool PlayerIsPresent
		{
			get
			{
				return playerIsPresent;
			}
		}

		#endregion

	}

}