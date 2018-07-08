/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ArrowPrompt.cs"
 * 
 *	This script allows for "Walking Dead"-style on-screen arrows,
 *	which respond to player input.
 * 
 */

using UnityEngine;
using System.Collections;

namespace AC
{

	/**
	 * This component provides the ability to display up to four arrows on-screen.
	 * Each arrow responds to player input, and can run an ActionList when the relevant input is detected.
	 */
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_arrow_prompt.html")]
	#endif
	public class ArrowPrompt : MonoBehaviour
	{

		/** What kind of input the arrows respond to (KeyOnly, ClickOnly, KeyAndClick) */
		public ArrowPromptType arrowPromptType = ArrowPromptType.KeyAndClick;
		/** The "up" Arrow */
		public Arrow upArrow;
		/** The "down" Arrow */
		public Arrow downArrow;
		/** The "left" Arrow */
		public Arrow leftArrow;
		/** The "right" Arrow */
		public Arrow rightArrow;
		/** If True, then Hotspots will be disabled when the arrows are on screen */
		public bool disableHotspots = true;

		/** A factor for the arrow position */
		public float positionFactor = 1f;
		/** A factor for the arrow size */
		public float scaleFactor = 1f;

		private bool isOn = false;
		
		private AC_Direction directionToAnimate;
		private float alpha = 0f;
		private float arrowSize = 0.05f;


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
		 * Draws the arrow(s) on screen, if appropriate.
		 * This function is called every frame by StateHandler.
		 */
		public void DrawArrows ()
		{
			if (alpha > 0f)
			{
				if (directionToAnimate != AC_Direction.None)
				{
					SetGUIAlpha (alpha);

					switch (directionToAnimate)
					{
						case AC_Direction.Up:
							upArrow.rect = GetUpRect (arrowSize);
							break;

						case AC_Direction.Down:
							downArrow.rect = GetDownRect (arrowSize);
							break;

						case AC_Direction.Left:
							leftArrow.rect = GetLeftRect (arrowSize);
							break;

						case AC_Direction.Right:
							rightArrow.rect = GetRightRect (arrowSize);
							break;
					}
				}
				
				else
				{
					SetGUIAlpha (alpha);
					
					if (upArrow.isPresent)
					{
						upArrow.rect = GetUpRect ();
					}
		
					if (downArrow.isPresent)
					{
						downArrow.rect = GetDownRect ();
					}
				
					if (leftArrow.isPresent)
					{
						leftArrow.rect = GetLeftRect ();
					}
					
					if (rightArrow.isPresent)
					{
						rightArrow.rect = GetRightRect ();
					}
				}
			
				upArrow.Draw ();
				downArrow.Draw ();
				leftArrow.Draw ();
				rightArrow.Draw ();
			}
		}


		private Rect GetUpRect (float scale = 0.05f)
		{
			return KickStarter.mainCamera.LimitMenuToAspect (AdvGame.GUIRect (0.5f, 0.1f * positionFactor, scale * 2f * scaleFactor, scale * scaleFactor));
		}


		private Rect GetDownRect (float scale = 0.05f)
		{
			return KickStarter.mainCamera.LimitMenuToAspect (AdvGame.GUIRect (0.5f, 1f - (0.1f * positionFactor), scale * 2f * scaleFactor, scale * scaleFactor));
		}


		private Rect GetLeftRect (float scale = 0.05f)
		{
			return KickStarter.mainCamera.LimitMenuToAspect (AdvGame.GUIRect (0.05f * positionFactor * 2f, 0.5f, scale * scaleFactor, scale * 2f * scaleFactor));
		}


		private Rect GetRightRect (float scale = 0.05f)
		{
			return KickStarter.mainCamera.LimitMenuToAspect (AdvGame.GUIRect (1f - (0.05f * positionFactor * 2f), 0.5f, scale * scaleFactor, scale * 2f * scaleFactor));
		}


		/**
		 * <summary>Enables the ArrowPrompt.</summary>
		 */
		public void TurnOn ()
		{
			if (upArrow.isPresent || downArrow.isPresent || leftArrow.isPresent || rightArrow.isPresent)
			{
				if (KickStarter.playerInput)
				{
					KickStarter.playerInput.activeArrows = this;
				}
				
				StartCoroutine ("FadeIn");
				directionToAnimate = AC_Direction.None;
				arrowSize = 0.05f;
			}
		}
		
		
		private void Disable ()
		{
			if (KickStarter.playerInput)
			{
				KickStarter.playerInput.activeArrows = null;
			}
			
			isOn = false;
		}
		

		/**
		 * <summary>Disables the ArrowPrompt.</summary>
		 */
		public void TurnOff ()
		{
			Disable ();
			StopCoroutine ("FadeIn");
			alpha = 0f;
		}
		

		/**
		 * Triggers the "up" arrow.
		 */
		public void DoUp ()
		{
			if (upArrow.isPresent && isOn && directionToAnimate == AC_Direction.None)
			{
				StartCoroutine (FadeOut (AC_Direction.Up));
				Disable ();
				upArrow.Run ();
			}
		}
		

		/**
		 * Triggers the "down" arrow.
		 */
		public void DoDown ()
		{
			if (downArrow.isPresent && isOn && directionToAnimate == AC_Direction.None)
			{
				StartCoroutine (FadeOut (AC_Direction.Down));
				Disable ();
				downArrow.Run ();
			}
		}
		

		/**
		 * Triggers the "left" arrow.
		 */
		public void DoLeft ()
		{
			if (leftArrow.isPresent && isOn && directionToAnimate == AC_Direction.None)
			{
				StartCoroutine (FadeOut (AC_Direction.Left));
				Disable ();
				leftArrow.Run ();
			}
		}
		

		/**
		 * Triggers the "right" arrow.
		 */
		public void DoRight ()
		{
			if (rightArrow.isPresent && isOn && directionToAnimate == AC_Direction.None)
			{
				StartCoroutine (FadeOut (AC_Direction.Right));
				Disable ();
				rightArrow.Run ();
			}
		}
		
		
		private IEnumerator FadeIn ()
		{
			alpha = 0f;
			
			if (alpha < 1f)
			{
				while (alpha < 0.95f)
				{
					alpha += 0.05f;
					alpha = Mathf.Clamp01 (alpha);
					yield return new WaitForFixedUpdate();
				}
				
				alpha = 1f;
				isOn = true;
			}
		}
		
		
		private IEnumerator FadeOut (AC_Direction direction)
		{
			arrowSize = 0.05f;
			alpha = 1f;
			directionToAnimate = direction;
			
			if (alpha > 0f)
			{
				while (alpha > 0.05f)
				{
					arrowSize += 0.005f;
					
					alpha -= 0.05f;
					alpha = Mathf.Clamp01 (alpha);
					yield return new WaitForFixedUpdate();
				}
				alpha = 0f;

			}
		}
		
		
		private void SetGUIAlpha (float alpha)
		{
			Color tempColor = GUI.color;
			tempColor.a = alpha;
			GUI.color = tempColor;
		}


		private float LargeSize
		{
			get
			{
				return arrowSize*2 * scaleFactor;
			}
		}


		private float SmallSize
		{
			get
			{
				return arrowSize * scaleFactor;
			}
		}

	}


	/**
	 * A data container for an arrow that is used in an ArrowPrompt.
	 */
	[System.Serializable]
	public class Arrow
	{
			
		/** If True, the Arrow is defined and used in the ArrowPrompt */
		public bool isPresent;
		/** The Cutscene to run when the Arrow is triggered */
		public Cutscene linkedCutscene;
		/** The texture to draw on-screen */
		public Texture2D texture;
		/** The OnGUI Rect that defines the screen boundary */
		public Rect rect;
		

		/**
		 * The default Constructor.
		 */
		public Arrow ()
		{
			isPresent = false;
		}
		

		/**
		 * Runs the Arrow's linkedCutscene.
		 */
		public void Run ()
		{
			if (linkedCutscene)
			{
				linkedCutscene.SendMessage ("Interact");
			}
		}
		

		/**
		 * Draws the Arrow on screen.
		 * This is called every OnGUI call by StateHandler.
		 */
		public void Draw ()
		{
			if (texture)
			{
				GUI.DrawTexture (rect, texture, ScaleMode.StretchToFill, true);
			}
		}

	}

}