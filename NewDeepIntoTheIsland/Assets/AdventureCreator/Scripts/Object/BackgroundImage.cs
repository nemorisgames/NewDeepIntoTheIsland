/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"BackgroundImage.cs"
 * 
 *	The BackgroundImage prefab is used to store a GUITexture for use in background images for 2.5D games.
 * 
 */

#if !UNITY_2017_3_OR_NEWER
#define ALLOW_LEGACY_UI
#endif

#if UNITY_STANDALONE && (UNITY_5 || UNITY_2017_1_OR_NEWER || UNITY_PRO_LICENSE)
#define ALLOW_MOVIETEXTURES
#endif

using UnityEngine;
using System.Collections;

namespace AC
{

	/**
	 * Controls a GUITexture for use in background images in 2.5D games.
	 */
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_background_image.html")]
	#endif
	public class BackgroundImage : MonoBehaviour
	{

		#if ALLOW_LEGACY_UI
		public enum BackgroundMethod25D { UnityUI, GUITexture };
		/** How 2.5D backgrounds are renderered (GUITexture, UnityUI) */
		public BackgroundMethod25D backgroundMethod25D = BackgroundMethod25D.GUITexture;
		#endif

		public Texture backgroundTexture;

		#if ALLOW_MOVIETEXTURES
		/** If True, then any MovieTexture set as the background will be looped */
		public bool loopMovie = true;
		/** If True, then any MovieTexture set as the background will start from the beginning when the associated Camera is activated */
		public bool restartMovieWhenTurnOn = false;
		#endif

		private float shakeDuration;
		private float startTime;
		private float startShakeIntensity;
		private float shakeIntensity;
		private Rect originalPixelInset;


		private void Start ()
		{
			GetBackgroundTexture ();
		}


		/**
		 * <summary>Sets the background image to a supplied texture</summary>
		 * <param name = "_texture">The texture to set the background image to</param>
		 */
		public void SetImage (Texture2D _texture)
		{
			SetBackgroundTexture (_texture);
		}


		/**
		 * Displays the background image (within the GUITexture) fullscreen.
		 */
		public void TurnOn ()
		{
			if (LayerMask.NameToLayer (KickStarter.settingsManager.backgroundImageLayer) == -1)
			{
				ACDebug.LogWarning ("No '" + KickStarter.settingsManager.backgroundImageLayer + "' layer exists - please define one in the Tags Manager.");
			}
			else
			{
				gameObject.layer = LayerMask.NameToLayer (KickStarter.settingsManager.backgroundImageLayer);
			}

			#if ALLOW_LEGACY_UI
			if (backgroundMethod25D == BackgroundMethod25D.GUITexture)
			{
				SetBackgroundCameraFarClipPlane (0.02f);
				if (GUITexture)
				{
					GUITexture.enabled = true;
				}
			}
			else if (backgroundMethod25D == BackgroundMethod25D.UnityUI)
			{
			#endif
				SetBackgroundCameraFarClipPlane (0.02f);
				BackgroundImageUI.Instance.SetTexture (GetBackgroundTexture ());
			#if ALLOW_LEGACY_UI
			}
			#endif

			#if ALLOW_MOVIETEXTURES
			if (GetBackgroundTexture () && GetBackgroundTexture () is MovieTexture)
			{
				MovieTexture movieTexture = (MovieTexture) GetBackgroundTexture ();
				if (restartMovieWhenTurnOn)
				{
					movieTexture.Stop ();
				}
				movieTexture.loop = loopMovie;
				movieTexture.Play ();
			}
			#endif
		}


		private void SetBackgroundCameraFarClipPlane (float value)
		{
			BackgroundCamera backgroundCamera = Object.FindObjectOfType <BackgroundCamera>();
			if (backgroundCamera)
			{
				backgroundCamera.GetComponent <Camera>().farClipPlane = value;
			}
			else
			{
				ACDebug.LogWarning ("Cannot find BackgroundCamera");
			}
		}
		

		/**
		 * Hides the background image (within the GUITexture) from view.
		 */
		public void TurnOff ()
		{
			gameObject.layer = LayerMask.NameToLayer (KickStarter.settingsManager.deactivatedLayer);

			#if ALLOW_LEGACY_UI
			if (backgroundMethod25D == BackgroundMethod25D.GUITexture)
			{
				if (GUITexture)
				{
					GUITexture.enabled = false;
				}
			}
			else if (backgroundMethod25D == BackgroundMethod25D.UnityUI)
			{
			#endif
				BackgroundImageUI.Instance.ClearTexture (GetBackgroundTexture ());
			#if ALLOW_LEGACY_UI
			}
			#endif
		}
		

		/**
		 * <summary>Shakes the background image (within the GUITexture) for an earthquake-like effect.</summary>
		 * <param name = "_shakeIntensity">How intense the shake effect should be</param>
		 * <param name = "_duration">How long the shake effect should last, in seconds</param>
		 */
		public void Shake (float _shakeIntensity, float _duration)
		{
			#if ALLOW_LEGACY_UI
			if (backgroundMethod25D == BackgroundMethod25D.GUITexture && GUITexture)
			{
				if (shakeIntensity > 0f)
				{
					GUITexture.pixelInset = originalPixelInset;
				}
				originalPixelInset = GUITexture.pixelInset;
			}
			#endif

			shakeDuration = _duration;
			startTime = Time.time;
			shakeIntensity = _shakeIntensity;

			startShakeIntensity = shakeIntensity;

			StopCoroutine (UpdateShake ());
			StartCoroutine (UpdateShake ());
		}
		

		private IEnumerator UpdateShake ()
		{
			while (shakeIntensity > 0f)
			{
				float _size = Random.Range (0, shakeIntensity) * 0.2f;

				#if ALLOW_LEGACY_UI
				if (backgroundMethod25D == BackgroundMethod25D.GUITexture && GUITexture)
				{
					GUITexture.pixelInset = new Rect
					(
						originalPixelInset.x - Random.Range (0, shakeIntensity) * 0.1f,
						originalPixelInset.y - Random.Range (0, shakeIntensity) * 0.1f,
						originalPixelInset.width + _size,
						originalPixelInset.height + _size
					);
				}
				else if (backgroundMethod25D == BackgroundMethod25D.UnityUI)
				{
				#endif
					BackgroundImageUI.Instance.SetShakeIntensity (_size);
				#if ALLOW_LEGACY_UI
				}
				#endif

				shakeIntensity = Mathf.Lerp (startShakeIntensity, 0f, AdvGame.Interpolate (startTime, shakeDuration, MoveMethod.Linear, null));

				yield return new WaitForEndOfFrame ();
			}
			
			shakeIntensity = 0f;

			#if ALLOW_LEGACY_UI
			if (backgroundMethod25D == BackgroundMethod25D.GUITexture && GUITexture)
			{
				GUITexture.pixelInset = originalPixelInset;
			}
			else if (backgroundMethod25D == BackgroundMethod25D.UnityUI)
			{
			#endif
				BackgroundImageUI.Instance.SetShakeIntensity (0f);
			#if ALLOW_LEGACY_UI
			}
			#endif
		}


		private Texture GetBackgroundTexture ()
		{
			if (backgroundTexture == null)
			{
				#if ALLOW_LEGACY_UI
				if (GUITexture)
				{
					backgroundTexture = GUITexture.texture;
				}
				#endif
			}
			return backgroundTexture;
		}


		private void SetBackgroundTexture (Texture _texture)
		{
			backgroundTexture = _texture;
			#if ALLOW_LEGACY_UI
			if (GUITexture)
			{
				GUITexture.texture = _texture;
			}
			#endif
		}


		#if ALLOW_LEGACY_UI
		private GUITexture _guiTexture;
		public GUITexture GUITexture
		{
			get
			{
				if (_guiTexture == null)
				{
					_guiTexture = GetComponent<GUITexture>();
					if (_guiTexture == null)
					{
						ACDebug.LogWarning (this.name + " has no GUITexture component");
					}
				}
				return _guiTexture;
			}
		}
		#endif

		
	}

}