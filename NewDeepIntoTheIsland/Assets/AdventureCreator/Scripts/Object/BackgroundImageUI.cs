/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"BackgroundImageUI.cs"
 * 
 *	The BackgroundImageUI prefab is used to control a Unity UI canvas for use in background images for 2.5D games.
 * 
 */

 using UnityEngine;
 using UnityEngine.UI;

namespace AC
{

	public class BackgroundImageUI : MonoBehaviour
	{

		#region Variables

		[SerializeField] private Canvas canvas;
		[SerializeField] private RawImage rawImage;
		[SerializeField] private Texture emptyTexture;

		private RectTransform rawImageRectTransform;

		#endregion


		#region UnityStandards

		private void Start ()
		{
			rawImageRectTransform = rawImage.GetComponent <RectTransform>();
		}

		#endregion


		#region PublicFunctions

		/**
		 * <summary>Sets the RawImage component's texture to a given texture</summary>
		 * <param name = "texture">The texture to assign</param>
		 */
		public void SetTexture (Texture texture)
		{
			if (texture == null) return;

			if (canvas.worldCamera == null)
			{
				BackgroundCamera backgroundCamera = Object.FindObjectOfType <BackgroundCamera>();
				if (backgroundCamera != null)
				{
					canvas.worldCamera = backgroundCamera.GetComponent <Camera>();
				}
				else
				{
					ACDebug.LogWarning ("No 'BackgroundCamera' found - is it present in the scene? If not, drag it in from /AdventureCreator/Prefabs/Automatic.");
				}
			}

			canvas.planeDistance = 0.015f;
			rawImage.texture = texture;
		}


		/**
		 * <summary>Clears the RawImage component's texture</summary>
		 * <param name = "texture">If not null, the texture will only be cleared if this texture is currently assigned.</param>
		 */
		public void ClearTexture (Texture texture)
		{
			if (rawImage.texture == texture || texture == null)
			{
				rawImage.texture = emptyTexture;
			}
		}


		/**
		 * <summary>Creates a shake effect by scaling and repositioning the background image</summary>
		 * <param name = "intensity">The intensity of the effect</param>
		 */
		public void SetShakeIntensity (float intensity)
		{
			float scale = 1f + (intensity / 50f);
			rawImageRectTransform.localScale = Vector3.one * scale;

			float xShift = Random.Range (-intensity, intensity) * 2f;
			float yShift = Random.Range (-intensity, intensity) * 2f;

			Vector2 offset = new Vector2 (xShift, yShift);
			rawImageRectTransform.localPosition = offset;
		}

		#endregion


		#region PrivateFunctions

		private void AssignCamera ()
		{
			if (canvas.worldCamera == null)
			{
				BackgroundCamera backgroundCamera = Object.FindObjectOfType <BackgroundCamera>();
				if (backgroundCamera != null)
				{
					canvas.worldCamera = backgroundCamera.GetComponent <Camera>();
				}
				else
				{
					ACDebug.LogWarning ("No 'BackgroundCamera' found - is it present in the scene? If not, drag it in from /AdventureCreator/Prefabs/Automatic.");
				}
			}
		}

		#endregion


		#region Static

		private static BackgroundImageUI instance;
		public static BackgroundImageUI Instance
		{
			get
			{
				if (instance == null)
				{ 
					instance = (BackgroundImageUI) Object.FindObjectOfType <BackgroundImageUI>();
					if (instance == null)
					{
						GameObject newInstanceOb = (GameObject) Instantiate (Resources.Load (Resource.backgroundImageUI));
						instance = newInstanceOb.GetComponent <BackgroundImageUI>();
						newInstanceOb.name = Resource.backgroundImageUI;
						instance.AssignCamera ();
					}
				}
				return instance;
			}
		}

		#endregion

	}

}