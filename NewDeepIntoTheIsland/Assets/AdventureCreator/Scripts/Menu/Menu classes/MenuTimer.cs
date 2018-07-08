/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"MenuTimer.cs"
 * 
 *	This MenuElement can be used in conjunction with MenuDialogList to create
 *	timed conversations, "Walking Dead"-style.
 * 
 */

using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	/**
	 * A MenuElement that provides a "countdown" timer that can either show the time remaining to choose a Conversation's dialogue option or complete a QTE, or the progress made in the current QTE.
	 */
	public class MenuTimer : MenuElement
	{

		/** The Unity UI Slider this is linked to (Unity UI Menus only) */
		public Slider uiSlider;
		/** If True, then the value will be inverted, and the timer will move in the opposite direction */
		public bool doInvert;
		/** The texture of the slider bar (OnGUI Menus only) */
		public Texture2D timerTexture;
		/** What the value of the timer represents (Conversation, QuickTimeEventProgress, QuickTimeEventRemaining) */
		public AC_TimerType timerType = AC_TimerType.Conversation;
		/** The method which this element are hidden from view when made invisible (DisableObject, DisableInteractability) */
		public UISelectableHideStyle uiSelectableHideStyle = UISelectableHideStyle.DisableObject;

		private Rect timerRect;


		/**
		 * Initialises the MenuElement when it is created within MenuManager.
		 */
		public override void Declare ()
		{
			uiSlider = null;
			doInvert = false;
			isVisible = true;
			isClickable = false;
			timerType = AC_TimerType.Conversation;
			numSlots = 1;
			SetSize (new Vector2 (20f, 5f));
			uiSelectableHideStyle = UISelectableHideStyle.DisableObject;

			base.Declare ();
		}


		public override MenuElement DuplicateSelf (bool fromEditor, bool ignoreUnityUI)
		{
			MenuTimer newElement = CreateInstance <MenuTimer>();
			newElement.Declare ();
			newElement.CopyTimer (this, ignoreUnityUI);
			return newElement;
		}
		
		
		private void CopyTimer (MenuTimer _element, bool ignoreUnityUI)
		{
			if (ignoreUnityUI)
			{
				uiSlider = null;
			}
			else
			{
				uiSlider = _element.uiSlider;
			}

			doInvert = _element.doInvert;
			timerTexture = _element.timerTexture;
			timerType = _element.timerType;
			uiSelectableHideStyle = _element.uiSelectableHideStyle;
			
			base.Copy (_element);
		}


		/**
		 * <summary>Initialises the linked Unity UI GameObject.</summary>
		 * <param name = "_menu">The element's parent Menu</param>
		 */
		public override void LoadUnityUI (AC.Menu _menu, Canvas canvas)
		{
			uiSlider = LinkUIElement <Slider> (canvas);
			if (uiSlider)
			{
				uiSlider.minValue = 0f;
				uiSlider.maxValue = 1f;
				uiSlider.wholeNumbers = false;
				uiSlider.value = 1f;
				uiSlider.interactable = false;
			}
		}


		/**
		 * <summary>Gets the boundary of the element.</summary>
		 * <param name = "_slot">Ignored by this subclass</param>
		 * <returns>The boundary Rect of the element</returns>
		 */
		public override RectTransform GetRectTransform (int _slot)
		{
			if (uiSlider)
			{
				return uiSlider.GetComponent <RectTransform>();
			}
			return null;
		}


		#if UNITY_EDITOR
		
		public override void ShowGUI (Menu menu)
		{
			string apiPrefix = "(AC.PlayerMenus.GetElementWithName (\"" + menu.title + "\", \"" + title + "\") as AC.MenuTimer)";

			MenuSource source = menu.menuSource;
			EditorGUILayout.BeginVertical ("Button");

			timerType = (AC_TimerType) CustomGUILayout.EnumPopup ("Timer type:", timerType, apiPrefix + ".timerType");
			if (timerType == AC_TimerType.LoadingProgress && AdvGame.GetReferences ().settingsManager != null && !AdvGame.GetReferences ().settingsManager.useAsyncLoading)
			{
				EditorGUILayout.HelpBox ("Loading progress cannot be displayed unless asynchonised loading is enabled within the Settings Manager.", MessageType.Warning);
			}
			doInvert = CustomGUILayout.Toggle ("Invert value?", doInvert, apiPrefix + ".doInvert");

			if (source == MenuSource.AdventureCreator)
			{
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Timer texture:", GUILayout.Width (145f));
				timerTexture = (Texture2D) CustomGUILayout.ObjectField <Texture2D> (timerTexture, false, GUILayout.Width (70f), GUILayout.Height (30f), apiPrefix + ".timerTexture");
				EditorGUILayout.EndHorizontal ();
			}
			else
			{
				uiSlider = LinkedUiGUI <Slider> (uiSlider, "Linked Slider:", source);
				uiSelectableHideStyle = (UISelectableHideStyle) CustomGUILayout.EnumPopup ("When invisible:", uiSelectableHideStyle, apiPrefix + ".uiSelectableHideStyle");
			}
			EditorGUILayout.EndVertical ();

			if (source == MenuSource.AdventureCreator)
			{
				EndGUI (apiPrefix);
			}
		}

		#endif
		

		/**
		 * <summary>Performs all calculations necessary to display the element.</summary>
		 * <param name = "_slot">Ignored by this subclass</param>
		 * <param name = "languageNumber">The index number of the language to display text in</param>
		 * <param name = "isActive">If True, then the element will be drawn as though highlighted</param>
		 */
		public override void PreDisplay (int _slot, int languageNumber, bool isActive)
		{
			if (Application.isPlaying)
			{
				float progress = 0f;

				if (timerType == AC_TimerType.Conversation)
				{
					if (KickStarter.playerInput.activeConversation && KickStarter.playerInput.activeConversation.isTimed)
					{
						progress = KickStarter.playerInput.activeConversation.GetTimeRemaining ();
					}
				}
				else if (timerType == AC_TimerType.QuickTimeEventProgress)
				{
					if (!KickStarter.playerQTE.QTEIsActive ())
					{
						return;
					}
					progress = KickStarter.playerQTE.GetProgress ();
				}
				else if (timerType == AC_TimerType.QuickTimeEventRemaining)
				{
					if (!KickStarter.playerQTE.QTEIsActive ())
					{
						return;
					}
					progress = KickStarter.playerQTE.GetRemainingTimeFactor ();
				}
				else if (timerType == AC_TimerType.LoadingProgress)
				{
					progress = KickStarter.sceneChanger.GetLoadingProgress ();
				}

				if (doInvert)
				{
					progress = 1f - progress;
				}

				if (uiSlider)
				{
					uiSlider.value = progress;
					UpdateUISelectable (uiSlider, uiSelectableHideStyle);
				}
				else
				{
					timerRect = relativeRect;
					timerRect.width *= progress;
				}
			}
			else
			{
				timerRect = relativeRect;
			}
		}
		

		/**
		 * <summary>Draws the element using OnGUI</summary>
		 * <param name = "_style">The GUIStyle to draw with</param>
		 * <param name = "_slot">Ignored by this subclass</param>
		 * <param name = "zoom">The zoom factor</param>
		 * <param name = "isActive">If True, then the element will be drawn as though highlighted</param>
		 */
		public override void Display (GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			if (timerTexture)
			{
				GUI.DrawTexture (ZoomRect (timerRect, zoom), timerTexture, ScaleMode.StretchToFill, true, 0f);
			}
			
			base.Display (_style, _slot, zoom, isActive);
		}

	}

}