/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ClickMarker.cs"
 * 
 *	This script demonstrates how to script a prefab that appears at the Player's
 *	intended destination during Point And Click mode.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	/**
	 * An example script that demonstrates how a "Click Marker" can be animated at the Player's intended destination during Point And Click mode.
	 * Click Markers can be set within SettingsManager.
	 */
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_click_marker.html")]
	#endif
	public class ClickMarker : MonoBehaviour
	{

		/** How long the marker will remain visible on-screen */
		public float lifeTime = 0.5f;

		private float startTime;
		private Vector3 startScale;
		private Vector3 endScale = Vector3.zero;


		private void Start ()
		{
			Destroy (this.gameObject, lifeTime);

			if (lifeTime > 0f)
			{
				startTime = Time.time;
				startScale = transform.localScale;
			}

			StartCoroutine ("ShrinkMarker");
		}


		private IEnumerator ShrinkMarker ()
		{
			while (lifeTime > 0f)
			{
				transform.localScale = Vector3.Lerp (startScale, endScale, AdvGame.Interpolate (startTime, lifeTime, MoveMethod.EaseIn, null));
				yield return new WaitForFixedUpdate ();
			}
		}

	}

}