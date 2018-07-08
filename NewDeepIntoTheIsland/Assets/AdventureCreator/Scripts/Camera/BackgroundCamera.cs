/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"BackgroundCamera.cs"
 * 
 *	The BackgroundCamera is used to display background images underneath the scene geometry.
 * 
 */

using UnityEngine;

namespace AC
{

	/**
	 * This is used to display background images underneath scene geometry in 2.5D games.
	 * It should not normally render anything other than a BackgroundImage.
	 */
	[RequireComponent (typeof (Camera))]
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_background_camera.html")]
	#endif
	public class BackgroundCamera : MonoBehaviour
	{
		
		private Camera _camera;
		
		
		private void Awake ()
		{
			_camera = GetComponent <Camera>();
			
			UpdateRect ();
			
			if (KickStarter.settingsManager)
			{
				if (LayerMask.NameToLayer (KickStarter.settingsManager.backgroundImageLayer) == -1)
				{
					ACDebug.LogWarning ("No '" + KickStarter.settingsManager.backgroundImageLayer + "' layer exists - please define one in the Tags Manager.");
				}
				else
				{
					GetComponent <Camera>().cullingMask = (1 << LayerMask.NameToLayer (KickStarter.settingsManager.backgroundImageLayer));
				}
			}
			else
			{
				ACDebug.LogWarning ("A Settings Manager is required for this camera type");
			}
		}


		private void OnEnable ()
		{
			if (KickStarter.stateHandler) KickStarter.stateHandler.Register (this);
		}


		private void Start ()
		{
			if (KickStarter.stateHandler) KickStarter.stateHandler.Register (this);
		}


		private void OnDisable ()
		{
			if (KickStarter.stateHandler) KickStarter.stateHandler.Unregister (this);
		}
		

		/**
		 * Updates the Camera's Rect.
		 * 
		 */
		public void UpdateRect ()
		{
			if (_camera == null)
			{
				_camera = GetComponent <Camera>();
			}
			_camera.rect = Camera.main.rect;
		}
		
	}
	
}