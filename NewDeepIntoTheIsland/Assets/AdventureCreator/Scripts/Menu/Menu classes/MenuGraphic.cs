/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"MenuGraphic.cs"
 * 
 *	This MenuElement provides a space for
 *	animated and static textures
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
	 * A MenuElement that provides a space for animated or still images.
	 */
	public class MenuGraphic : MenuElement
	{

		/** The Unity UI Image this is linked to (Unity UI Menus only) */
		public Image uiImage;
		/** The type of graphic that is shown (Normal, DialogPortrait) */
		public AC_GraphicType graphicType = AC_GraphicType.Normal;
		/** The CursorIconBase that stores the graphic and animation data */
		public CursorIconBase graphic;

		public RawImage uiRawImage;
		[SerializeField] private UIImageType uiImageType = UIImageType.Image;
		private enum UIImageType { Image, RawImage };
		
		private Sprite sprite;
		private Speech speech;
		private bool speechIsAnimating;
		private Texture speechTex;
		private Rect speechRect;
		private bool isDuppingSpeech;
		

		/**
		 * Initialises the element when it is created within MenuManager.
		 */
		public override void Declare ()
		{
			uiImage = null;
			uiRawImage = null;
			
			graphicType = AC_GraphicType.Normal;
			isVisible = true;
			isClickable = false;
			graphic = new CursorIconBase ();
			numSlots = 1;
			SetSize (new Vector2 (10f, 5f));
			
			base.Declare ();
		}
		

		public override MenuElement DuplicateSelf (bool fromEditor, bool ignoreUnityUI)
		{
			MenuGraphic newElement = CreateInstance <MenuGraphic>();
			newElement.Declare ();
			newElement.CopyGraphic (this, ignoreUnityUI);
			return newElement;
		}
		
		
		private void CopyGraphic (MenuGraphic _element, bool ignoreUnityUI)
		{
			if (ignoreUnityUI)
			{
				uiImage = null;
			}
			else
			{
				uiImage = _element.uiImage;
			}
			uiRawImage = _element.uiRawImage;
			uiImageType = _element.uiImageType;

			graphicType = _element.graphicType;
			graphic = new CursorIconBase ();
			graphic.Copy (_element.graphic);
			base.Copy (_element);
		}
		

		/**
		 * <summary>Initialises the linked Unity UI GameObjects.</summary>
		 * <param name = "_menu">The element's parent Menu</param>
		 */
		public override void LoadUnityUI (AC.Menu _menu, Canvas canvas)
		{
			if (uiImageType == UIImageType.Image)
			{
				uiImage = LinkUIElement <Image> (canvas);
			}
			else if (uiImageType == UIImageType.RawImage)
			{
				uiRawImage = LinkUIElement <RawImage> (canvas);
			}
		}
		

		/**
		 * <summary>Gets the boundary of a slot</summary>
		 * <param name = "_slot">Ignored by this subclass</param>
		 * <returns>The boundary Rect of the slot</returns>
		 */
		public override RectTransform GetRectTransform (int _slot)
		{
			if (uiImageType == UIImageType.Image && uiImage != null)
			{
				return uiImage.rectTransform;
			}
			else if (uiImageType == UIImageType.RawImage && uiRawImage != null)
			{
				return uiRawImage.rectTransform;
			}
			return null;
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI (Menu menu)
		{
			string apiPrefix = "(AC.PlayerMenus.GetElementWithName (\"" + menu.title + "\", \"" + title + "\") as AC.MenuGraphic)";

			MenuSource source = menu.menuSource;
			EditorGUILayout.BeginVertical ("Button");
			
			if (source != MenuSource.AdventureCreator)
			{
				uiImageType = (UIImageType) EditorGUILayout.EnumPopup ("UI image type:", uiImageType);
				if (uiImageType == UIImageType.Image)
				{
					uiImage = LinkedUiGUI <Image> (uiImage, "Linked Image:", source);
				}
				else if (uiImageType == UIImageType.RawImage)
				{
					uiRawImage = LinkedUiGUI <RawImage> (uiRawImage, "Linked Raw Image:", source);
				}
				EditorGUILayout.EndVertical ();
				EditorGUILayout.BeginVertical ("Button");
			}
			
			graphicType = (AC_GraphicType) CustomGUILayout.EnumPopup ("Graphic type:", graphicType, apiPrefix + ".graphicType");
			if (graphicType == AC_GraphicType.Normal)
			{
				graphic.ShowGUI (false, false, "Texture:", CursorRendering.Software, apiPrefix + ".graphic");
			}
			EditorGUILayout.EndVertical ();
			
			base.ShowGUI (menu);
		}
		
		#endif


		private void UpdateSpeechLink ()
		{
			if (!isDuppingSpeech && KickStarter.dialog.GetLatestSpeech () != null)
			{
				speech = KickStarter.dialog.GetLatestSpeech ();
			}
		}
		

		/**
		 * <summary>Assigns the element to a specific Speech line.</summary>
		 * <param name = "_speech">The Speech line to assign the element to</param>
		 */
		public override void SetSpeech (Speech _speech)
		{
			isDuppingSpeech = true;
			speech = _speech;
		}
		

		/**
		 * Clears any speech text on display.
		 */
		public override void ClearSpeech ()
		{
			if (graphicType == AC_GraphicType.DialoguePortrait)
			{
				speechTex = null;
			}
		}


		/**
		 * <summary>Performs all calculations necessary to display the element.</summary>
		 * <param name = "_slot">Ignored by this subclass</param>
		 * <param name = "languageNumber">The index number of the language to display text in</param>
		 * <param name = "isActive">If True, then the element will be drawn as though highlighted</param>
		 */
		public override void PreDisplay (int _slot, int languageNumber, bool isActive)
		{
			if (graphicType == AC_GraphicType.DialoguePortrait)
			{
				UpdateSpeechLink ();
				if (speech != null)
				{
					speechTex = speech.GetPortrait ();
					speechIsAnimating = speech.IsAnimating ();
				}
			}
			
			SetUIGraphic ();
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

			if (graphicType == AC_GraphicType.Normal)
			{
				if (graphic != null)
				{
					graphic.DrawAsInteraction (ZoomRect (relativeRect, zoom), true);
				}
			}
			else
			{
				if (speechTex != null)
				{
					if (speechIsAnimating)
					{
						if (speech != null)
						{
							speechRect = speech.GetAnimatedRect ();
						}
						GUI.DrawTextureWithTexCoords (ZoomRect (relativeRect, zoom), speechTex, speechRect);
					}
					else
					{
						GUI.DrawTexture (ZoomRect (relativeRect, zoom), speechTex, ScaleMode.StretchToFill, true, 0f);
					}
				}
			}
		}
		

		/**
		 * <summary>Recalculates the element's size.
		 * This should be called whenever a Menu's shape is changed.</summary>
		 * <param name = "source">How the parent Menu is displayed (AdventureCreator, UnityUiPrefab, UnityUiInScene)</param>
		 */
		public override void RecalculateSize (MenuSource source)
		{
			graphic.Reset ();
			SetUIGraphic ();
			base.RecalculateSize (source);
		}


		private void SetUIGraphic ()
		{
			if (uiImageType == UIImageType.Image && uiImage != null)
			{
				if (graphicType == AC_GraphicType.Normal)
				{
					uiImage.sprite = graphic.GetAnimatedSprite (true);
				}
				else if (speech != null)
				{
					uiImage.sprite = speech.GetPortraitSprite ();
				}
				UpdateUIElement (uiImage);
			}
			if (uiImageType == UIImageType.RawImage && uiRawImage != null)
			{
				if (graphicType == AC_GraphicType.Normal)
				{
					if (graphic.texture != null && graphic.texture is RenderTexture)
					{
						uiRawImage.texture = graphic.texture;
					}
					else
					{
						uiRawImage.texture = graphic.GetAnimatedTexture (true);
					}
				}
				else if (speech != null)
				{
					uiRawImage.texture = speech.GetPortrait ();
				}
				UpdateUIElement (uiRawImage);
			}
		}
		
		
		protected override void AutoSize ()
		{
			if (graphicType == AC_GraphicType.Normal && graphic.texture != null)
			{
				GUIContent content = new GUIContent (graphic.texture);
				AutoSize (content);
			}
		}
		
	}
	
}