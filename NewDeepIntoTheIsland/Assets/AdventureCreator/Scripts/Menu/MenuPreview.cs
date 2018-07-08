using System;
using UnityEngine;

namespace AC
{

	/**
	 * Allows Menus (see: Menu) made in MenuManager to be shown in the Game Window when the game is not running, allowing for real-time previews as they are built.
	 */
	[ExecuteInEditMode]
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_menu_preview.html")]
	#endif
	public class MenuPreview : MonoBehaviour
	{

		#if UNITY_EDITOR

		private MenuManager menuManager;
		private GUIStyle normalStyle;
		private GUIStyle highlightedStyle;
		private Vector2 screenSize;


		private void SetStyles (MenuElement element)
		{
			normalStyle = new GUIStyle();
			normalStyle.normal.textColor = element.fontColor;
			normalStyle.font = element.font;
			normalStyle.fontSize = element.GetFontSize ();
			normalStyle.alignment = TextAnchor.MiddleCenter;

			highlightedStyle = new GUIStyle();
			highlightedStyle.font = element.font;
			highlightedStyle.fontSize = element.GetFontSize ();
			highlightedStyle.normal.textColor = element.fontHighlightColor;
			highlightedStyle.normal.background = element.highlightTexture;
			highlightedStyle.alignment = TextAnchor.MiddleCenter;
		}

		
		private void Update ()
		{
			if (!Application.isPlaying)
			{
				if (AdvGame.GetReferences ())
				{
					menuManager = AdvGame.GetReferences ().menuManager;
					
					if (menuManager)
					{
						if (menuManager.GetSelectedMenu () != null)
						{
							AC.Menu menu = menuManager.GetSelectedMenu ();

							if (menu.IsUnityUI ())
							{
								return;
							}

							foreach (MenuElement element in menu.visibleElements)
							{
								for (int i=0; i<element.GetNumSlots (); i++)
								{
									if (menuManager.GetSelectedElement () == element && element.isClickable && i == 0)
									{
										element.PreDisplay (i, 0, false);
									}
									
									else
									{
										element.PreDisplay (i, 0, false);
									}
								}
							}
						}
					}
				}
			}
		}
		
		
		private void OnGUI ()
		{
			if (!Application.isPlaying)
			{
				if (AdvGame.GetReferences ())
				{
					menuManager = AdvGame.GetReferences ().menuManager;

					if (menuManager && menuManager.drawInEditor && AdvGame.GetReferences ().viewingMenuManager)
					{
						if (menuManager.GetSelectedMenu () != null)
						{
							AC.Menu menu = menuManager.GetSelectedMenu ();

							if (menu.IsUnityUI ())
							{
								return;
							}

							if (KickStarter.mainCamera != null)
							{
								KickStarter.mainCamera.DrawBorders ();
							}

							CheckScreenSize (menu);

							if ((menu.appearType == AppearType.Manual || menu.appearType == AppearType.OnInputKey) && menu.pauseWhenEnabled && menuManager.pauseTexture)
							{
								GUI.DrawTexture (AdvGame.GUIRect (0.5f, 0.5f, 1f, 1f), menuManager.pauseTexture, ScaleMode.ScaleToFit, true, 0f);
							}
							
							if ((menu.positionType == AC_PositionType.FollowCursor || menu.positionType == AC_PositionType.AppearAtCursorAndFreeze || menu.positionType == AC_PositionType.OnHotspot || menu.positionType == AC_PositionType.AboveSpeakingCharacter || menu.positionType == AC_PositionType.AbovePlayer) && AdvGame.GetReferences ().cursorManager && AdvGame.GetReferences ().cursorManager.pointerIcon.texture)
							{
								CursorIconBase icon = AdvGame.GetReferences ().cursorManager.pointerIcon;
								GUI.DrawTexture (AdvGame.GUIBox (new Vector2 (AdvGame.GetMainGameViewSize ().x / 2f, AdvGame.GetMainGameViewSize ().y / 2f), icon.size), icon.texture, ScaleMode.ScaleToFit, true, 0f);
							}
							
							menu.StartDisplay ();
											
							foreach (MenuElement element in menu.visibleElements)
							{
								SetStyles (element);
								
								for (int i=0; i<element.GetNumSlots (); i++)
								{
									if (menuManager.GetSelectedElement () == element && element.isClickable && i == 0)
									{
										element.Display (highlightedStyle, i, 1f, true);
									}
									
									else
									{
										element.Display (normalStyle, i, 1f, false);
									}
								}

								if (UnityEditor.EditorWindow.mouseOverWindow != null && UnityEditor.EditorWindow.mouseOverWindow.ToString () != null && UnityEditor.EditorWindow.mouseOverWindow.ToString ().Contains ("(UnityEditor.GameView)"))
								{
									if (menu.IsPointerOverSlot (element, 0, Event.current.mousePosition + new Vector2 (menu.GetRect ().x, menu.GetRect ().y)))
									{
										menuManager.SelectElementFromPreview (menu, element);
									}
								}
							}
					
							menu.EndDisplay ();
							
							if (menuManager.drawOutlines)
							{
								menu.DrawOutline (menuManager.GetSelectedElement ());
							}
						}
					}
				}
			}
		}


		private void CheckScreenSize (AC.Menu menu)
		{
			#if UNITY_5_4_OR_NEWER
			menu.Recalculate ();
			#endif

			if (!Mathf.Approximately (screenSize.x, Screen.width) || !Mathf.Approximately (screenSize.y, Screen.height))			
			{
				screenSize = new Vector2 (Screen.width, Screen.height);
				menu.Recalculate ();
			}
		}

		#endif

	}

}