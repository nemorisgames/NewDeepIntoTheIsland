/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"MenuDrag.cs"
 * 
 *	This MenuElement can be used to drag around its parent Menu.
 * 
 */

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;	
#endif

namespace AC
{

	/**
	 * A MenuElement that can be used to drag around another element, or its parent Menu.
	 * This element type cannot be used in Unity UI-based Menus, because Unity UI has its own classes that perform the same functionality.
	 */
	[System.Serializable]
	public class MenuDrag : MenuElement
	{

		/** The text that's displayed on-screen */
		public string label = "Element";
		/** The text alignment */
		public TextAnchor anchor;
		/** The special FX applied to the text (None, Outline, Shadow, OutlineAndShadow) */
		public TextEffects textEffects;
		/** The boundary that the draggable Menu or MenuElement can be moved within */
		public Rect dragRect;
		/** What the MenuDrag can be used to move (EntireMenu, SingleElement) */
		public DragElementType dragType = DragElementType.EntireMenu;
		/** The name of the MenuElement that can be dragged, if dragType = DragElementType.SingleElement */
		public string elementName;

		private Vector2 dragStartPosition;
		private AC.Menu menuToDrag;
		private MenuElement elementToDrag;
		private string fullText;


		/**
		 * Initialises the element when it is created within MenuManager.
		 */
		public override void Declare ()
		{
			label = "Button";
			isVisible = true;
			isClickable = true;
			textEffects = TextEffects.None;
			numSlots = 1;
			anchor = TextAnchor.MiddleCenter;
			SetSize (new Vector2 (10f, 5f));
			dragRect = new Rect (0,0,0,0);
			dragType = DragElementType.EntireMenu;
			elementName = "";

			base.Declare ();
		}


		public override MenuElement DuplicateSelf (bool fromEditor, bool ignoreUnityUI)
		{
			MenuDrag newElement = CreateInstance <MenuDrag>();
			newElement.Declare ();
			newElement.CopyDrag (this);
			return newElement;
		}
		
		
		private void CopyDrag (MenuDrag _element)
		{
			label = _element.label;
			anchor = _element.anchor;
			textEffects = _element.textEffects;
			dragRect = _element.dragRect;
			dragType = _element.dragType;
			elementName = _element.elementName;

			base.Copy (_element);
		}

		
		#if UNITY_EDITOR
		
		public override void ShowGUI (Menu menu)
		{
			string apiPrefix = "(AC.PlayerMenus.GetElementWithName (\"" + menu.title + "\", \"" + title + "\") as AC.MenuDrag)";

			MenuSource source = menu.menuSource;
			if (source != MenuSource.AdventureCreator)
			{
				EditorGUILayout.HelpBox ("This Element type is not necessary in Unity's UI, as it can be recreated using ScrollBars and ScrollRects.", MessageType.Info);
				return;
			}

			EditorGUILayout.BeginVertical ("Button");
			label = CustomGUILayout.TextField ("Button text:", label, apiPrefix + ".label");
			anchor = (TextAnchor) CustomGUILayout.EnumPopup ("Text alignment:", anchor, apiPrefix + ".anchor");
			textEffects = (TextEffects) CustomGUILayout.EnumPopup ("Text effect:", textEffects, apiPrefix + ".textEffects");

			dragType = (DragElementType) CustomGUILayout.EnumPopup ("Drag type:", dragType, apiPrefix + ".dragType");
			if (dragType == DragElementType.SingleElement)
			{
				elementName = CustomGUILayout.TextField ("Element name:", elementName, apiPrefix + ".elementName");
			}

			dragRect = EditorGUILayout.RectField ("Drag boundary:", dragRect);

			ChangeCursorGUI (menu);

			EditorGUILayout.EndVertical ();
			
			base.ShowGUI (menu);
		}


		public override void DrawOutline (bool isSelected, AC.Menu _menu)
		{
			if (dragType == DragElementType.EntireMenu)
			{
				DrawStraightLine.DrawBox (_menu.GetRectAbsolute (GetDragRectRelative ()), Color.white, 1f, false, 1);
			}
			else
			{
				if (elementName != "")
				{
					MenuElement element = MenuManager.GetElementWithName (_menu.title, elementName);
					if (element != null)
					{
						Rect dragBox = _menu.GetRectAbsolute (GetDragRectRelative ());
						dragBox.x += element.GetSlotRectRelative (0).x;
						dragBox.y += element.GetSlotRectRelative (0).y;
						DrawStraightLine.DrawBox (dragBox, Color.white, 1f, false, 1);
					}
				}
			}
			
			base.DrawOutline (isSelected, _menu);
		}

		#endif


		/**
		 * <summary>Performs all calculations necessary to display the element.</summarys>
		 * <param name = "_slot">Ignored by this subclass</param>
		 * <param name = "languageNumber">The index number of the language to display text in</param>
		 * <param name = "isActive">If True, then the element will be drawn as though highlighted</param>
		 */
		public override void PreDisplay (int _slot, int languageNumber, bool isActive)
		{
			fullText = TranslateLabel (label, languageNumber);
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
			base.Display (_style, _slot, zoom, isActive);
			
			_style.alignment = anchor;
			if (zoom < 1f)
			{
				_style.fontSize = (int) ((float) _style.fontSize * zoom);
			}
			
			if (textEffects != TextEffects.None)
			{
				AdvGame.DrawTextEffect (ZoomRect (relativeRect, zoom), fullText, _style, Color.black, _style.normal.textColor, 2, textEffects);
			}
			else
			{
				GUI.Label (ZoomRect (relativeRect, zoom), fullText, _style);
			}
		}
		

		/**
		 * <summary>Gets the display text of the element</summary>
		 * <param name = "slot">Ignored by this subclass</param>
		 * <param name = "languageNumber">The index number of the language number to get the text in</param>
		 * <returns>The display text of the element's slot, or the whole element if it only has one slot</returns>
		 */
		public override string GetLabel (int slot, int languageNumber)
		{
			return TranslateLabel (label, languageNumber);
		}
		
		
		protected override void AutoSize ()
		{
			if (label == "" && backgroundTexture != null)
			{
				GUIContent content = new GUIContent (backgroundTexture);
				AutoSize (content);
			}
			else
			{
				GUIContent content = new GUIContent (TranslateLabel (label, Options.GetLanguage ()));
				AutoSize (content);
			}
		}


		private void StartDrag (AC.Menu _menu)
		{
			menuToDrag = _menu;

			if (dragType == DragElementType.SingleElement)
			{
				if (elementName != "")
				{
					MenuElement element = PlayerMenus.GetElementWithName (_menu.title, elementName);
					if (element == null)
					{
						ACDebug.LogWarning ("Cannot drag " + elementName + " as it cannot be found on " + _menu.title);
					}
					else if (element.positionType == AC_PositionType2.Aligned)
					{
						ACDebug.LogWarning ("Cannot drag " + elementName + " as its Position is set to Aligned");
					}
					else if (_menu.sizeType == AC_SizeType.Automatic)
					{
						ACDebug.LogWarning ("Cannot drag " + elementName + " as its parent Menu's Size is set to Automatic");
					}
					else
					{
						elementToDrag = element;
						dragStartPosition = elementToDrag.GetDragStart ();
					}
				}
			}
			else
			{
				dragStartPosition = _menu.GetDragStart ();
			}
		}


		/**
		 * <summary>Performs the drag.</summary>
		 * <param name = "_dragVector">The amount and direction to drag by</param>
		 * <returns>True if the drag effect was successful</returns>
		 */
		public bool DoDrag (Vector2 _dragVector)
		{
			if (dragType == DragElementType.EntireMenu)
			{
				if (menuToDrag == null)
				{
					return false;
				}
				
				if (!menuToDrag.IsEnabled () || menuToDrag.IsFading ())
				{
					return false;
				}
			}
			
			if (elementToDrag == null && dragType == DragElementType.SingleElement)
			{
				return false;
			}
			
			// Convert dragRect to screen co-ordinates
			Rect dragRectAbsolute = dragRect;
			if (sizeType != AC_SizeType.AbsolutePixels)
			{
				dragRectAbsolute = new Rect (dragRect.x * AdvGame.GetMainGameViewSize(true).x / 100f,
											 dragRect.y * AdvGame.GetMainGameViewSize(true).y / 100f,
											 dragRect.width * AdvGame.GetMainGameViewSize(true).x / 100f,
											 dragRect.height * AdvGame.GetMainGameViewSize(true).y / 100f);
			}
			
			if (dragType == DragElementType.EntireMenu)
			{
				menuToDrag.SetDragOffset (_dragVector + dragStartPosition, dragRectAbsolute);
			}
			else if (dragType == AC.DragElementType.SingleElement)
			{
				elementToDrag.SetDragOffset (_dragVector + dragStartPosition, dragRectAbsolute);
			}
			
			return true;
		}


		/**
		 * <summary>Checks if the dragging should be cancelled.</summary>
		 * <param name = "mousePosition">The position of the mouse cursor</param>
		 * <returns>True if the dragging should be cancelled</returns>
		 */
		public bool CheckStop (Vector2 mousePosition)
		{
			if (menuToDrag == null)
			{
				return false;
			}
			if (dragType == DragElementType.EntireMenu && !menuToDrag.IsPointerOverSlot (this, 0, mousePosition))
			{
				return true;
			}
			if (dragType == DragElementType.SingleElement && elementToDrag != null && !menuToDrag.IsPointerOverSlot (this, 0, mousePosition))
			{
				return true;
			}
			return false;
		}


		private Rect GetDragRectRelative ()
		{
			Rect positionRect = dragRect;

			if (sizeType != AC_SizeType.AbsolutePixels)
			{
				positionRect.x = dragRect.x / 100f * AdvGame.GetMainGameViewSize(true).x;
				positionRect.y = dragRect.y / 100f * AdvGame.GetMainGameViewSize(true).y;

				positionRect.width = dragRect.width / 100f * AdvGame.GetMainGameViewSize(true).x;
				positionRect.height = dragRect.height / 100f * AdvGame.GetMainGameViewSize(true).y;
			}

			return (positionRect);
		}


		/**
		 * <summary>Performs what should happen when the element is clicked on.</summary>
		 * <param name = "_menu">The element's parent Menu</param>
		 * <param name = "_slot">Ignored by this subclass</param>
		 * <param name = "_mouseState">The state of the mouse button</param>
		 */
		public override void ProcessClick (AC.Menu _menu, int _slot, MouseState _mouseState)
		{
			if (_mouseState == MouseState.SingleClick)
			{
				base.ProcessClick (_menu, _slot, _mouseState);
				StartDrag (_menu);
				KickStarter.playerInput.SetActiveDragElement (this);
			}
		}
		
	}
	
}