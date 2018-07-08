/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"Parallax2D.cs"
 * 
 *	Attach this script to a GameObject when making a 2D game,
 *	to make it scroll as the camera moves.
 * 
 */

using UnityEngine;

namespace AC
{

	/**
	 * When used in 2D games, this script can be attached to scene objects to make them scroll as the camera moves, creating a parallax effect.
	 */
	[AddComponentMenu("Adventure Creator/Misc/Parallax 2D")]
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_parallax2_d.html")]
	#endif
	public class Parallax2D : MonoBehaviour
	{

		/** The intensity of the depth effect. Positive values will make the GameObject appear further away (i.e. in the background), negative values will make it appear closer to the camera (i.e. in the foreground). */
		public float depth;
		/** If True, then the GameObject will scroll in the X-direction */
		public bool xScroll;
		/** If True, then the GameObject will scroll in the Y-direction */
		public bool yScroll;
		/** An offset for the GameObject's initial position along the X-axis */
		public float xOffset;
		/** An offset for the GameObject's initial position along the Y-axis */
		public float yOffset;
		
		/** If True, scrolling in the X-direction will be constrained */
		public bool limitX;
		/** The minimum scrolling position in the X-direction, if limitX = True */
		public float minX;
		/** The maximum scrolling position in the X-direction, if limitX = True */
		public float maxX;

		/** If True, scrolling in the Y-direction will be constrained */
		public bool limitY;
		/** The minimum scrolling position in the Y-direction, if limitY = True */
		public float minY;
		/** The maximum scrolling position in the Y-direction, if limitY = True */
		public float maxY;

		/** What entity affects the parallax behaviour (Camera, Cursor) */
		public ParallaxReactsTo reactsTo = ParallaxReactsTo.Camera;

		private float xStart;
		private float yStart;
		private float xDesired;
		private float yDesired;
		private Vector2 perspectiveOffset;


		private void Awake ()
		{
			xStart = transform.localPosition.x;
			yStart = transform.localPosition.y;

			xDesired = xStart;
			yDesired = yStart;
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
		 * Updates the GameObject's position according to the camera.  This is called every frame by the StateHandler.
		 */
		public void UpdateOffset ()
		{
			switch (reactsTo)
			{
				case ParallaxReactsTo.Camera:
					if (KickStarter.mainCamera.attachedCamera is GameCamera2D && !KickStarter.mainCamera.attachedCamera._camera.orthographic)
					{
						perspectiveOffset = KickStarter.mainCamera.GetPerspectiveOffset ();
					}
					else
					{
						perspectiveOffset = new Vector2 (Camera.main.transform.position.x, Camera.main.transform.position.y);
					}
					break;

				case ParallaxReactsTo.Cursor:
					Vector2 screenCentre = AdvGame.GetMainGameViewSize () / 2f;
					Vector2 mousePosition = KickStarter.playerInput.GetMousePosition ();
					perspectiveOffset = new Vector2 (((1f - mousePosition.x) / screenCentre.x) + 1f, ((1f - mousePosition.y) / screenCentre.y + 1f));
					break;
			}

			if (limitX)
			{
				perspectiveOffset.x = Mathf.Clamp (perspectiveOffset.x, minX, maxX);
			}
			if (limitY)
			{
				perspectiveOffset.y = Mathf.Clamp (perspectiveOffset.y, minY, maxY);
			}

			xDesired = xStart;
			if (xScroll)
			{
				xDesired += perspectiveOffset.x * depth;
				xDesired += xOffset;
			}

			yDesired = yStart;
			if (yScroll)
			{
				yDesired += perspectiveOffset.y * depth;
				yDesired += yOffset;
			}

			if (xScroll && yScroll)
			{
				transform.localPosition = new Vector3 (xDesired, yDesired, transform.localPosition.z);
			}
			else if (xScroll)
			{
				transform.localPosition = new Vector3 (xDesired, transform.localPosition.y, transform.localPosition.z);
			}
			else if (yScroll)
			{
				transform.localPosition = new Vector3 (transform.localPosition.x, yDesired, transform.localPosition.z);
			}
		}

	}

}