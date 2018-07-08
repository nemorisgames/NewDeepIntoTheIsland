/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"SpriteFader.cs"
 * 
 *	Attach this to any sprite you wish to fade.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	/**
	 * Provides functions that can fade a sprite in and out.
	 */
	[AddComponentMenu("Adventure Creator/Misc/Sprite fader")]
	[RequireComponent (typeof (SpriteRenderer))]
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_sprite_fader.html")]
	#endif
	public class SpriteFader : MonoBehaviour
	{

		/** True if the Sprite attached to the GameObject this script is attached to is currently fading */
		[HideInInspector] public bool isFading = true;
		/** The time at which the sprite began fading */
		[HideInInspector] public float fadeStartTime;
		/** The duration of the sprite-fading effect */
		[HideInInspector] public float fadeTime;
		/** The direction of the sprite-fading effect (fadeIn, fadeOut) */
		[HideInInspector] public FadeType fadeType;

		private SpriteRenderer spriteRenderer;


		private void Awake ()
		{
			spriteRenderer = GetComponent <SpriteRenderer>();
		}


		/**
		 * <summary>Forces the alpha value of a sprite to a specific value.</summary>
		 * <param name = "_alpha">The alpha value to assign the sprite attached to this GameObject</param>
		 */
		public void SetAlpha (float _alpha)
		{
			Color color = GetComponent <SpriteRenderer>().color;
			color.a = _alpha;
			GetComponent <SpriteRenderer>().color = color;
		}


		/**
		 * <summary>Fades the Sprite attached to this GameObject in or out.</summary>
		 * <param name = "_fadeType">The direction of the fade effect (fadeIn, fadeOut)</param>
		 * <param name = "_fadeTime">The duration, in seconds, of the fade effect</param>
		 * <param name = "startAlpha">The alpha value that the Sprite should have when the effect begins. If <0, the Sprite's original alpha will be used.</param>
		 */
		public void Fade (FadeType _fadeType, float _fadeTime, float startAlpha = -1)
		{
			StopCoroutine ("DoFade");

			float currentAlpha = spriteRenderer.color.a;

			if (startAlpha >= 0)
			{
				currentAlpha = startAlpha;
				SetAlpha (startAlpha);
			}
			else
			{
				if (spriteRenderer.enabled == false)
				{
					spriteRenderer.enabled = true;
					if (_fadeType == FadeType.fadeIn)
					{
						currentAlpha = 0f;
						SetAlpha (0f);
					}
				}
			}

			if (_fadeType == FadeType.fadeOut)
			{
				fadeStartTime = Time.time - (currentAlpha * _fadeTime);
			}
			else
			{
				fadeStartTime = Time.time - ((1f - currentAlpha) * _fadeTime);
			}
		
			fadeTime = _fadeTime;
			fadeType = _fadeType;

			if (fadeTime > 0f)
			{
				StartCoroutine ("DoFade");
			}
			else
			{
				EndFade ();
			}
		}


		/**
		 * Ends the sprite-fading effect, and sets the Sprite's alpha to its target value.
		 */
		public void EndFade ()
		{
			StopCoroutine ("DoFade");

			isFading = false;
			Color color = spriteRenderer.color;
			if (fadeType == FadeType.fadeIn)
			{
				color.a = 1f;
			}
			else
			{
				color.a = 0f;
			}
			spriteRenderer.color = color;
		}


		private IEnumerator DoFade ()
		{
			spriteRenderer.enabled = true;
			isFading = true;

			float alpha = spriteRenderer.color.a;

			if (fadeType == FadeType.fadeIn)
			{
				while (alpha < 1f)
				{
					alpha = -1f + AdvGame.Interpolate (fadeStartTime, fadeTime, MoveMethod.Linear, null);
					spriteRenderer.color = new Color (spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);
					yield return new WaitForFixedUpdate ();
				}
				spriteRenderer.color = new Color (spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 1f);
			}
			else
			{
				while (alpha > 0f)
				{
					alpha = 2f - AdvGame.Interpolate (fadeStartTime, fadeTime, MoveMethod.Linear, null);
					spriteRenderer.color = new Color (spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);
					yield return new WaitForFixedUpdate ();
				}
				spriteRenderer.color = new Color (spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0f);
			}
			isFading = false;
		}

	}

}