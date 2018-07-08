/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"LightSwitch.cs"
 * 
 *	This can be used, via the Object: Send Message Action,
 *	to turn its attached light component on and off.
 * 
 */

using UnityEngine;

namespace AC
{

	/**
	 * This script provides functions to enable and disable the Light component on the GameObject it is attached to.
	 * These functions can be called either through script, or with the "Object: Send message" Action.
	 */
	[RequireComponent (typeof (Light))]
	[AddComponentMenu("Adventure Creator/Misc/Light switch")]
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_light_switch.html")]
	#endif
	public class LightSwitch : MonoBehaviour
	{

		/** If True, then the Light component will be enabled when the game begins. */
		public bool enableOnStart = false;
		
		
		private void Awake ()
		{
			Switch (enableOnStart);
		}
		

		/**
		 * Enables the Light component on the GameObject this script is attached to.
		 */
		public void TurnOn ()
		{
			Switch (true);
		}
		

		/**
		 * Disables the Light component on the GameObject this script is attached to.
		 */
		public void TurnOff ()
		{
			Switch (false);
		}


		private void Switch (bool turnOn)
		{
			GetComponent <Light>().enabled = turnOn;
		}
		
	}

}