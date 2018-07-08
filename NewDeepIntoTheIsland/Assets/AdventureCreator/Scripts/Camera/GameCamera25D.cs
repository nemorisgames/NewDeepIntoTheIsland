/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"GameCamera25D.cs"
 * 
 *	This GameCamera is fixed, but allows for a background image to be displayed.
 * 
 */

using UnityEngine;

namespace AC
{

	/**
	 * A fixed camera that allows for a BackgroundImage to be displayed underneath all scene objects.
	 */
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_game_camera25_d.html")]
	#endif
	public class GameCamera25D : _Camera
	{

		/** The BackgroundImage to display underneath all scene objects. */
		public BackgroundImage backgroundImage;
		/** If True, then the MainCamera will copy its position when the Inspector is viewed */
		public bool isActiveEditor = false;
		/** The offset in perspective from the camera's centre */
		public Vector2 perspectiveOffset = Vector2.zero;


		/**
		 * Enables the assigned backgroundImage, disables all other BackgroundImage objects, and ensures MainCamera can view it.
		 */
		public void SetActiveBackground ()
		{
			if (backgroundImage)
			{
				// Move background images onto correct layer
				BackgroundImage[] backgroundImages = FindObjectsOfType (typeof (BackgroundImage)) as BackgroundImage[];
				foreach (BackgroundImage image in backgroundImages)
				{
					if (image == backgroundImage)
					{
						image.TurnOn ();
					}
					else
					{
						image.TurnOff ();
					}
				}
				
				// Set MainCamera's Clear Flags
				KickStarter.mainCamera.PrepareForBackground ();
			}
		}


		new public void ResetTarget ()
		{}


		public override Vector2 GetPerspectiveOffset ()
		{
			return perspectiveOffset;
		}


		public override void MoveCameraInstant ()
		{
			SetProjection ();
		}


		private void SetProjection ()
		{
			if (!_camera.orthographic)
			{
				_camera.projectionMatrix = AdvGame.SetVanishingPoint (_camera, perspectiveOffset);
			}
		}

	}
		
}