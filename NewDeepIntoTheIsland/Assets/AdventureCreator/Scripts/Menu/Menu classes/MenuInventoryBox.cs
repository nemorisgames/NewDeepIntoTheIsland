/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"MenuInventoryBox.cs"
 * 
 *	This MenuElement lists all inventory items held by the player.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	/**
	 * A MenuElement that lists inventory items (see: InvItem).
	 * It can be used to display all inventory items held by the player, those that are stored within a Container, or as part of an Interaction Menu.
	 */
	public class MenuInventoryBox : MenuElement
	{

		/** A List of UISlot classes that reference the linked Unity UI GameObjects (Unity UI Menus only) */
		public UISlot[] uiSlots;
		/** What pointer state registers as a 'click' for Unity UI Menus (PointerClick, PointerDown, PointerEnter) */
		public UIPointerState uiPointerState = UIPointerState.PointerClick;

		/** The special FX applied to the text (None, Outline, Shadow, OutlineAndShadow) */
		public TextEffects textEffects;
		/** The outline thickness, if textEffects != TextEffects.None */
		public float outlineSize = 2f;
		/** How the items to display are chosen (Default, HotspotBased, CustomScript, DisplaySelected, DisplayLastSelected, Container) */
		public AC_InventoryBoxType inventoryBoxType;
		/** The maximum number of inventory items that can be shown at once */
		public int maxSlots;
		/** If True, only inventory items (InvItem) with a specific category will be displayed */
		public bool limitToCategory;
		/** If True, then only inventory items that are listed in a Hotspot's / InvItem's interactions will be listed if inventoryBoxType = AC_InventoryBoxType.HotspotBased */
		public bool limitToDefinedInteractions = true;
		/** The category ID to limit the display of inventory items by, if limitToCategory = True (Deprecated) */
		public int categoryID;
		/** The category IDs to limit the display of inventory items by, if limitToCategory = True */
		public List<int> categoryIDs = new List<int>();
		/** What Image component the Element's Graphics should be linked to (ImageComponent, ButtonTargetGraphic) */
		public LinkUIGraphic linkUIGraphic = LinkUIGraphic.ImageComponent;

		/** The List of inventory items that are on display */
		public List<InvItem> items = new List<InvItem>();
		/** If True, and inventoryBoxType = AC_InventoryBoxType.Container, then items will be selected automatically when they are removed from the container */
		public bool selectItemsAfterTaking = true;
		/** How items are displayed (IconOnly, TextOnly, IconAndText) */
		public ConversationDisplayType displayType = ConversationDisplayType.IconOnly;
		/** The method which this element (or slots within it) are hidden from view when made invisible (DisableObject, ClearContent) */
		public UIHideStyle uiHideStyle = UIHideStyle.DisableObject;

		private Container overrideContainer;
		private string[] labels = null;
		private Menu uiMenu;


		/**
		 * Initialises the element when it is created within MenuManager.
		 */
		public override void Declare ()
		{
			uiSlots = null;
			uiPointerState = UIPointerState.PointerClick;

			isVisible = true;
			isClickable = true;
			inventoryBoxType = AC_InventoryBoxType.Default;
			numSlots = 0;
			SetSize (new Vector2 (6f, 10f));
			maxSlots = 10;
			limitToCategory = false;
			limitToDefinedInteractions = true;
			selectItemsAfterTaking = true;
			categoryID = -1;
			displayType = ConversationDisplayType.IconOnly;
			textEffects = TextEffects.None;
			outlineSize = 2f;
			uiHideStyle = UIHideStyle.DisableObject;
			items = new List<InvItem>();
			categoryIDs = new List<int>();
			linkUIGraphic = LinkUIGraphic.ImageComponent;
		}


		public override MenuElement DuplicateSelf (bool fromEditor, bool ignoreUnityUI)
		{
			MenuInventoryBox newElement = CreateInstance <MenuInventoryBox>();
			newElement.Declare ();
			newElement.CopyInventoryBox (this, ignoreUnityUI);
			return newElement;
		}
		
		
		private void CopyInventoryBox (MenuInventoryBox _element, bool ignoreUnityUI)
		{
			if (ignoreUnityUI)
			{
				uiSlots = null;
			}
			else
			{
				uiSlots = _element.uiSlots;
			}
			uiPointerState = _element.uiPointerState;

			isClickable = _element.isClickable;
			textEffects = _element.textEffects;
			outlineSize = _element.outlineSize;
			inventoryBoxType = _element.inventoryBoxType;
			numSlots = _element.numSlots;
			maxSlots = _element.maxSlots;
			limitToCategory = _element.limitToCategory;
			limitToDefinedInteractions = _element.limitToDefinedInteractions;
			categoryID = _element.categoryID;
			selectItemsAfterTaking = _element.selectItemsAfterTaking;
			displayType = _element.displayType;
			uiHideStyle = _element.uiHideStyle;
			categoryIDs = _element.categoryIDs;
			linkUIGraphic = _element.linkUIGraphic;

			UpdateLimitCategory ();

			items = GetItemList ();

			base.Copy (_element);

			if (Application.isPlaying)
			{
				if (!(inventoryBoxType == AC_InventoryBoxType.HotspotBased && maxSlots == 1))
				{
					alternativeInputButton = "";
				}
			}

			Upgrade ();
		}


		private void Upgrade ()
		{
			if (limitToCategory && categoryID >= 0)
			{
				categoryIDs.Add (categoryID);
				categoryID = -1;

				if (Application.isPlaying)
				{
					ACDebug.Log ("The inventory box element '" + title + "' has been upgraded - please view it in the Menu Manager and Save.");
				}
			}
		}


		private void UpdateLimitCategory ()
		{
			if (Application.isPlaying && AdvGame.GetReferences ().inventoryManager != null && AdvGame.GetReferences ().inventoryManager.bins != null)
			{
				foreach (InvBin invBin in KickStarter.inventoryManager.bins)
				{
					if (categoryIDs.Contains (invBin.id))
					{
						// Fine!
					}
					else
					{
						categoryIDs.Remove (invBin.id);
					}
				}
			}
		}


		/**
		 * <summary>Initialises the linked Unity UI GameObjects.</summary>
		 * <param name = "_menu">The element's parent Menu</param>
		 */
		public override void LoadUnityUI (AC.Menu _menu, Canvas canvas)
		{
			uiMenu = _menu;

			int i=0;
			foreach (UISlot uiSlot in uiSlots)
			{
				uiSlot.LinkUIElements (canvas, linkUIGraphic);
				if (uiSlot != null && uiSlot.uiButton != null)
				{
					int j=i;

					if (inventoryBoxType == AC_InventoryBoxType.Default || inventoryBoxType == AC_InventoryBoxType.CustomScript)
					{
						if (KickStarter.settingsManager != null &&
							KickStarter.settingsManager.interactionMethod != AC_InteractionMethod.ContextSensitive &&
							KickStarter.settingsManager.inventoryInteractions == InventoryInteractions.Multiple)
						{}
						else
						{
							uiPointerState = UIPointerState.PointerClick;
						}
					}

					CreateUIEvent (uiSlot.uiButton, _menu, uiPointerState, j, false);

					uiSlot.AddClickHandler (_menu, this, j);
				}
				i++;
			}
		}


		/**
		 * <summary>Gets the UI Button associated with an inventory item, provided that the Menus' Source is UnityUiPrefab or UnityUiInScene.</summary>
		 * <param name = "itemID">The ID number of the inventory item (InvItem) to search for</param>
		 * <returns>The UI Button associated with an inventory item, or null if a suitable Button cannot be found.</returns>
		 */
		public UnityEngine.UI.Button GetUIButtonWithItem (int itemID)
		{
			for (int i=0; i<items.Count; i++)
			{
				if (items[i] != null && items[i].id == itemID)
				{
					if (uiSlots != null && uiSlots.Length > i && uiSlots[i] != null)
					{
						return uiSlots[i].uiButton;
					}
					return null;
				}
			}
			return null;
		}


		/**
		 * <summary>Gets the linked Unity UI GameObject associated with this element.</summary>
		 * <returns>The Unity UI GameObject associated with the element</returns>
		 */
		public override GameObject GetObjectToSelect (int slotIndex = 0)
		{
			if (uiSlots != null && uiSlots.Length > slotIndex && uiSlots[slotIndex].uiButton != null)
			{
				return uiSlots[slotIndex].uiButton.gameObject;
			}
			return null;
		}
		

		/**
		 * <summary>Gets the boundary of the slot</summary>
		 * <param name = "_slot">The index number of the slot to get the boundary of</param>
		 * <returns>The boundary Rect of the slot</returns>
		 */
		public override RectTransform GetRectTransform (int _slot)
		{
			if (uiSlots != null && uiSlots.Length > _slot)
			{
				return uiSlots[_slot].GetRectTransform ();
			}
			return null;
		}
		
		
		#if UNITY_EDITOR
		
		public override void ShowGUI (Menu menu)
		{
			string apiPrefix = "(AC.PlayerMenus.GetElementWithName (\"" + menu.title + "\", \"" + title + "\") as AC.MenuInventoryBox)";

			MenuSource source = menu.menuSource;
			EditorGUILayout.BeginVertical ("Button");

			inventoryBoxType = (AC_InventoryBoxType) CustomGUILayout.EnumPopup ("Inventory box type:", inventoryBoxType, apiPrefix + ".inventoryBoxType");
			if (inventoryBoxType == AC_InventoryBoxType.Default || inventoryBoxType == AC_InventoryBoxType.CustomScript)
			{
				maxSlots = CustomGUILayout.IntSlider ("Max number of slots:", maxSlots, 1, 30, apiPrefix + ".maxSlots");
				isClickable = true;
			}
			else if (inventoryBoxType == AC_InventoryBoxType.DisplaySelected)
			{
				isClickable = false;
				maxSlots = 1;
			}
			else if (inventoryBoxType == AC_InventoryBoxType.DisplayLastSelected)
			{
				isClickable = true;
				maxSlots = 1;
			}
			else if (inventoryBoxType == AC_InventoryBoxType.Container)
			{
				isClickable = true;
				maxSlots = CustomGUILayout.IntSlider ("Max number of slots:", maxSlots, 1, 30, apiPrefix + ".maxSlots");
				selectItemsAfterTaking = CustomGUILayout.Toggle ("Select item after taking?", selectItemsAfterTaking, apiPrefix + ".selectItemsAfterTaking");
			}
			else
			{
				isClickable = true;
				if (source == MenuSource.AdventureCreator)
				{
					numSlots = CustomGUILayout.IntField ("Test slots:", numSlots, apiPrefix + ".numSlots");
				}
				maxSlots = CustomGUILayout.IntSlider ("Max number of slots:", maxSlots, 1, 30, apiPrefix + ".maxSlots");
			}

			if (inventoryBoxType == AC_InventoryBoxType.HotspotBased)
			{
				if (!ForceLimitByReference ())
				{
					limitToDefinedInteractions = CustomGUILayout.ToggleLeft ("Only show items referenced in Interactions?", limitToDefinedInteractions, apiPrefix + ".limitToDefinedInteractions");
				}

				if (maxSlots == 1)
				{
					alternativeInputButton = CustomGUILayout.TextField ("Alternative input button:", alternativeInputButton, apiPrefix + ".alternativeInputButton");
				}
			}

			displayType = (ConversationDisplayType) CustomGUILayout.EnumPopup ("Display type:", displayType, apiPrefix + ".displayType");
			if (displayType == ConversationDisplayType.IconAndText && source == MenuSource.AdventureCreator)
			{
				EditorGUILayout.HelpBox ("'Icon And Text' mode is only available for Unity UI-based Menus.", MessageType.Warning);
			}

			if (inventoryBoxType != AC_InventoryBoxType.DisplaySelected && inventoryBoxType != AC_InventoryBoxType.DisplayLastSelected && source == MenuSource.AdventureCreator)
			{
				slotSpacing = CustomGUILayout.Slider ("Slot spacing:", slotSpacing, 0f, 20f, apiPrefix + ".slotSpacing");
				orientation = (ElementOrientation) CustomGUILayout.EnumPopup ("Slot orientation:", orientation, apiPrefix + ".orientation");
				if (orientation == ElementOrientation.Grid)
				{
					gridWidth = CustomGUILayout.IntSlider ("Grid size:", gridWidth, 1, 10, apiPrefix + ".gridWidth");
				}
			}
			
			if (inventoryBoxType == AC_InventoryBoxType.CustomScript)
			{
				ShowClipHelp ();
			}

			uiHideStyle = (UIHideStyle) CustomGUILayout.EnumPopup ("When slot is empty:", uiHideStyle, apiPrefix + ".uiHideStyle");

			if (source != MenuSource.AdventureCreator)
			{
				EditorGUILayout.EndVertical ();
				EditorGUILayout.BeginVertical ("Button");
				EditorGUILayout.LabelField ("Linked button objects", EditorStyles.boldLabel);

				uiSlots = ResizeUISlots (uiSlots, maxSlots);
				
				for (int i=0; i<uiSlots.Length; i++)
				{
					uiSlots[i].LinkedUiGUI (i, source);
				}

				linkUIGraphic = (LinkUIGraphic) EditorGUILayout.EnumPopup ("Link graphics to:", linkUIGraphic);

				// Don't show if Single and Default or Custom Script
				if (inventoryBoxType == AC_InventoryBoxType.Default || inventoryBoxType == AC_InventoryBoxType.CustomScript)
				{
					if (KickStarter.settingsManager != null &&
						KickStarter.settingsManager.interactionMethod != AC_InteractionMethod.ContextSensitive &&
						KickStarter.settingsManager.inventoryInteractions == InventoryInteractions.Multiple)
					{
						uiPointerState = (UIPointerState) CustomGUILayout.EnumPopup ("Responds to:", uiPointerState, apiPrefix + ".uiPointerState");
					}
				}
				else
				{
					uiPointerState = (UIPointerState) CustomGUILayout.EnumPopup ("Responds to:", uiPointerState, apiPrefix + ".uiPointerState");
				}
			}

			ChangeCursorGUI (menu);
			EditorGUILayout.EndVertical ();

			if (CanBeLimitedByCategory ())
			{
				ShowCategoriesUI (apiPrefix);
			}

			base.ShowGUI (menu);
		}


		protected override void ShowTextGUI (string apiPrefix)
		{
			textEffects = (TextEffects) CustomGUILayout.EnumPopup ("Text effect:", textEffects, apiPrefix + ".textEffects");
			if (textEffects != TextEffects.None)
			{
				outlineSize = CustomGUILayout.Slider ("Effect size:", outlineSize, 1f, 5f, apiPrefix + ".outlineSize");
			}
		}


		private void ShowCategoriesUI (string apiPrefix)
		{
			EditorGUILayout.BeginVertical ("Button");
			limitToCategory = CustomGUILayout.Toggle ("Limit by category?", limitToCategory, apiPrefix + ".limitToCategory");
			if (limitToCategory)
			{
				Upgrade ();

				if (AdvGame.GetReferences ().inventoryManager)
				{
					List<InvBin> bins = AdvGame.GetReferences ().inventoryManager.bins;

					if (bins == null || bins.Count == 0)
					{
						categoryIDs.Clear ();
						EditorGUILayout.HelpBox ("No categories defined!", MessageType.Warning);
					}
					else
					{
						for (int i=0; i<bins.Count; i++)
						{
							bool include = (categoryIDs.Contains (bins[i].id)) ? true : false;
							include = EditorGUILayout.ToggleLeft (" " + i.ToString () + ": " + bins[i].label, include);

							if (include)
							{
								if (!categoryIDs.Contains (bins[i].id))
								{
									categoryIDs.Add (bins[i].id);
								}
							}
							else
							{
								if (categoryIDs.Contains (bins[i].id))
								{
									categoryIDs.Remove (bins[i].id);
								}
							}
						}

						if (categoryIDs.Count == 0)
						{
							EditorGUILayout.HelpBox ("At least one category must be checked for this to take effect.", MessageType.Info);
						}
					}
				}
				else
				{
					EditorGUILayout.HelpBox ("No Inventory Manager defined!", MessageType.Warning);
					categoryIDs.Clear ();
				}
			}
			EditorGUILayout.EndVertical ();
		}

		#endif


		/**
		 * Hides all linked Unity UI GameObjects associated with the element.
		 */
		public override void HideAllUISlots ()
		{
			LimitUISlotVisibility (uiSlots, 0, uiHideStyle);
		}


		public override void SetUIInteractableState (bool state)
		{
			SetUISlotsInteractableState (uiSlots, state);
		}
		

		/**
		 * <summary>Performs all calculations necessary to display the element.</summary>
		 * <param name = "_slot">The index number of the slot to display</param>
		 * <param name = "languageNumber">The index number of the language to display text in</param>
		 * <param name = "isActive">If True, then the element will be drawn as though highlighted</param>
		 */
		public override void PreDisplay (int _slot, int languageNumber, bool isActive)
		{
			if (_slot >= 0 && uiSlots != null && uiSlots.Length > _slot && uiSlots[_slot].uiButton != null && KickStarter.playerMenus.IsEventSystemSelectingObject (uiSlots[_slot].uiButton.gameObject))
			{
				if (uiMenu != null && uiMenu.CanCurrentlyKeyboardControl ()) // Check added to prevent when using mouse to control
				{
					isActive = true;
				}
			}

			if (items.Count > 0 && items.Count > (_slot+offset) && items [_slot+offset] != null)
			{
				string fullText = "";

				if (displayType == ConversationDisplayType.TextOnly || displayType == ConversationDisplayType.IconAndText)
				{
					fullText = items [_slot+offset].label;
					if (KickStarter.runtimeInventory != null)
					{
						fullText = KickStarter.runtimeInventory.GetLabel (items [_slot+offset], languageNumber);
					}
					string countText = GetCount (_slot);
					if (countText != "")
					{
						fullText += " (" + countText + ")";
					}
				}
				else
				{
					string countText = GetCount (_slot);
					if (countText != "")
					{
						fullText = countText;
					}
				}

				if (labels == null || labels.Length != numSlots)
				{
					labels = new string [numSlots];
				}
				labels [_slot] = fullText;
			}

			if (Application.isPlaying)
			{
				if (uiSlots != null && uiSlots.Length > _slot)
				{
					LimitUISlotVisibility (uiSlots, numSlots, uiHideStyle);

					uiSlots[_slot].SetText (labels [_slot]);
					if (displayType == ConversationDisplayType.IconOnly || displayType == ConversationDisplayType.IconAndText)
					{
						Texture tex = null;
						if (items.Count > (_slot+offset) && items [_slot+offset] != null)
						{
							if (inventoryBoxType != AC_InventoryBoxType.DisplaySelected && inventoryBoxType != AC_InventoryBoxType.DisplayLastSelected)
							{
								if (KickStarter.settingsManager.selectInventoryDisplay == SelectInventoryDisplay.HideFromMenu && ItemIsSelected (items [_slot+offset]))
								{
									if (!items[_slot+offset].CanSelectSingle ())
									{
										// Display as normal if we only have one selected from many
										uiSlots[_slot].SetImage (null);
										labels [_slot] = "";
										uiSlots[_slot].SetText (labels [_slot]);
										return;
									}
								}
								tex = GetTexture (items [_slot+offset], isActive);
							}

							if (tex == null)
							{
								tex = items [_slot+offset].tex;
							}
						}
						uiSlots[_slot].SetImage (tex);
					}

					if (KickStarter.settingsManager.SelectInteractionMethod () == SelectInteractions.CyclingMenuAndClickingHotspot &&
						inventoryBoxType == AC_InventoryBoxType.HotspotBased &&
						items[_slot + offset].id == KickStarter.playerInteraction.GetActiveInvButtonID ())
					{
						// Select through script, not by mouse-over
						if (uiSlots[_slot].uiButton != null)
						{
							uiSlots[_slot].uiButton.Select ();
						}
					}
				}
			}
		}


		private bool ItemIsSelected (InvItem item)
		{
			if (item != null && item == KickStarter.runtimeInventory.SelectedItem && (! KickStarter.settingsManager.InventoryDragDrop || KickStarter.playerInput.GetDragState () == DragState.Inventory))
			{
				return true;
			}
			return false;
		}


		/**
		 * <summary>Draws the element using OnGUI</summary>
		 * <param name = "_style">The GUIStyle to draw with</param>
		 * <param name = "_slot">The index number of the slot to display</param>
		 * <param name = "zoom">The zoom factor</param>
		 * <param name = "isActive If True, then the element will be drawn as though highlighted</param>
		 */
		public override void Display (GUIStyle _style, int _slot, float zoom, bool isActive)
		{
			base.Display (_style, _slot, zoom, isActive);
			_style.wordWrap = true;
			
			if (items.Count > 0 && items.Count > (_slot+offset) && items [_slot+offset] != null)
			{
				if (Application.isPlaying && KickStarter.settingsManager.selectInventoryDisplay == SelectInventoryDisplay.HideFromMenu && ItemIsSelected (items [_slot+offset]))
				{
					if (!items[_slot+offset].CanSelectSingle ())
					{
						// Display as normal if we only have one selected from many
						return;
					}
				}
			
				if (displayType == ConversationDisplayType.IconOnly)
				{
					GUI.Label (GetSlotRectRelative (_slot), "", _style);
					DrawTexture (ZoomRect (GetSlotRectRelative (_slot), zoom), items [_slot+offset], isActive);
					_style.normal.background = null;
					
					if (textEffects != TextEffects.None)
					{
						AdvGame.DrawTextEffect (ZoomRect (GetSlotRectRelative (_slot), zoom), GetCount (_slot), _style, Color.black, _style.normal.textColor, outlineSize, textEffects);
					}
					else
					{
						GUI.Label (ZoomRect (GetSlotRectRelative (_slot), zoom), GetCount (_slot), _style);
					}
				}
				else if (displayType == ConversationDisplayType.TextOnly)
				{
					if (textEffects != TextEffects.None)
					{
						AdvGame.DrawTextEffect (ZoomRect (GetSlotRectRelative (_slot), zoom), labels[_slot], _style, Color.black, _style.normal.textColor, outlineSize, textEffects);
					}
					else
					{
						GUI.Label (ZoomRect (GetSlotRectRelative (_slot), zoom), labels[_slot], _style);
					}
				}
			}
		}
		

		/**
		 * <summary>Performs what should happen when the element is clicked on, if inventoryBoxType = AC_InventoryBoxType.Default.</summary>
		 * <param name = "_mouseState">The state of the mouse button</param>
		 * <param name = "_slot">The index number of the slot that was clicked on</param>
		 * <param name = "interactionMethod">The game's interaction method (ContextSensitive, ChooseHotspotThenInteraction, ChooseInteractionThenHotspot)</param>
		 */
		public void HandleDefaultClick (MouseState _mouseState, int _slot, AC_InteractionMethod interactionMethod)
		{
			if (!KickStarter.settingsManager.allowInventoryInteractionsDuringConversations && KickStarter.stateHandler.gameState == GameState.DialogOptions)
			{
				return;
			}

			if (KickStarter.runtimeInventory != null)
			{
				KickStarter.playerMenus.CloseInteractionMenus ();

				KickStarter.runtimeInventory.HighlightItemOffInstant ();
				KickStarter.runtimeInventory.SetFont (font, GetFontSize (), fontColor, textEffects);

				int trueIndex = _slot + offset;

				if (inventoryBoxType == AC_InventoryBoxType.Default)
				{
					if (items.Count <= trueIndex || items[trueIndex] == null)
					{
						// Black space
						if (KickStarter.runtimeInventory.SelectedItem != null && KickStarter.settingsManager.canReorderItems)
						{
							if (limitToCategory && categoryIDs != null && categoryIDs.Count > 0)
							{
								// Need to change index because we want to affect the actual inventory, not the percieved one shown in the restricted menu
								List<InvItem> trueItemList = GetItemList (false);
								LimitedItemList limitedItemList = LimitByCategory (trueItemList, trueIndex);
								trueIndex += limitedItemList.Offset;
							}

							KickStarter.runtimeInventory.MoveItemToIndex (KickStarter.runtimeInventory.SelectedItem, trueIndex);
						}
						KickStarter.runtimeInventory.SetNull ();
						return;
					}
				}

				if (interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction)
				{
					if (KickStarter.runtimeInventory.SelectedItem != null)
					{
						if (_mouseState == MouseState.SingleClick)
						{
							if (items.Count <= trueIndex) return;
							KickStarter.runtimeInventory.Combine (KickStarter.runtimeInventory.SelectedItem, items [trueIndex]);
						}
						else if (_mouseState == MouseState.RightClick)
						{
							KickStarter.runtimeInventory.SetNull ();
						}
					}
					else
					{
						if (items.Count <= trueIndex) return;
						KickStarter.runtimeInventory.ShowInteractions (items [trueIndex]);
					}
				}
				else if (interactionMethod == AC_InteractionMethod.ChooseInteractionThenHotspot)
				{
					if (items.Count <= trueIndex) return;

					if (_mouseState == MouseState.SingleClick)
					{
						int cursorID = KickStarter.playerCursor.GetSelectedCursorID ();
						int cursor = KickStarter.playerCursor.GetSelectedCursor ();

						if (cursor == -2 && KickStarter.runtimeInventory.SelectedItem != null)
						{
							if (items [trueIndex] == KickStarter.runtimeInventory.SelectedItem)
							{
								KickStarter.runtimeInventory.SelectItem (items [trueIndex], SelectItemMode.Use);
							}
							else
							{
								KickStarter.runtimeInventory.Combine (KickStarter.runtimeInventory.SelectedItem, items [trueIndex]);
							}
						}
						else if (cursor == -1 && !KickStarter.settingsManager.selectInvWithUnhandled)
						{
							KickStarter.runtimeInventory.SelectItem (items [trueIndex], SelectItemMode.Use);
						}
						else if (cursorID > -1)
						{
							KickStarter.runtimeInventory.RunInteraction (items [trueIndex], cursorID);
						}
					}
				}
				else if (interactionMethod == AC_InteractionMethod.ContextSensitive)
				{
					if (_mouseState == MouseState.SingleClick)
					{
						if (items.Count <= trueIndex) return;

						if (KickStarter.runtimeInventory.SelectedItem == null)
						{
							if (KickStarter.cursorManager.lookUseCursorAction == LookUseCursorAction.RightClickCyclesModes && KickStarter.playerCursor.ContextCycleExamine)
							{
								KickStarter.runtimeInventory.Look (items [trueIndex]);
							}
							else
							{
								KickStarter.runtimeInventory.Use (items [trueIndex]);
							}
						}
						else
						{
							KickStarter.runtimeInventory.Combine (KickStarter.runtimeInventory.SelectedItem, items [trueIndex]);
						}
					}
					else if (_mouseState == MouseState.RightClick)
					{
						if (KickStarter.runtimeInventory.SelectedItem == null)
						{
							if (items.Count > trueIndex && KickStarter.cursorManager.lookUseCursorAction != LookUseCursorAction.RightClickCyclesModes)
							{
								KickStarter.runtimeInventory.Look (items [trueIndex]);
							}
						}
						else
						{
							KickStarter.runtimeInventory.SetNull ();
						}
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
			items = GetItemList ();

			if (inventoryBoxType == AC_InventoryBoxType.HotspotBased)
			{
				if (Application.isPlaying)
				{
					numSlots = Mathf.Clamp (items.Count, 0, maxSlots);
				}
				else
				{
					numSlots = Mathf.Clamp (numSlots, 0, maxSlots);
				}
			}
			else
			{
				numSlots = maxSlots;
			}

			if (uiHideStyle == UIHideStyle.DisableObject)
			{
				if (numSlots > items.Count)
				{
					offset = 0;
					numSlots = items.Count;
				}
			}

			LimitOffset (items.Count);

			labels = new string [numSlots];

			if (Application.isPlaying && uiSlots != null)
			{
				ClearSpriteCache (uiSlots);
			}

			if (!isVisible)
			{
				LimitUISlotVisibility (uiSlots, 0, uiHideStyle);
			}
			base.RecalculateSize (source);
		}
		
		
		private List<InvItem> GetItemList (bool doLimit = true)
		{
			List<InvItem> newItemList = new List<InvItem>();

			if (Application.isPlaying)
			{
				if (inventoryBoxType == AC_InventoryBoxType.HotspotBased)
				{
					if (limitToDefinedInteractions || ForceLimitByReference ())
					{
						newItemList = KickStarter.runtimeInventory.MatchInteractions ();
					}
					else
					{
						newItemList = KickStarter.runtimeInventory.localItems;
					}
				}
				else if (inventoryBoxType == AC_InventoryBoxType.DisplaySelected)
				{
					if (KickStarter.runtimeInventory.SelectedItem != null)
					{
						newItemList.Add (KickStarter.runtimeInventory.SelectedItem);
					}
				}
				else if (inventoryBoxType == AC_InventoryBoxType.DisplayLastSelected)
				{
					if (KickStarter.runtimeInventory.LastSelectedItem != null && KickStarter.runtimeInventory.IsItemCarried (KickStarter.runtimeInventory.LastSelectedItem))
					{
						newItemList.Add (KickStarter.runtimeInventory.LastSelectedItem);
					}
				}
				else if (inventoryBoxType == AC_InventoryBoxType.Container)
				{
					if (overrideContainer != null)
					{
						newItemList = GetItemsFromContainer (overrideContainer);
					}
					else if (KickStarter.playerInput.activeContainer != null)
					{
						newItemList = GetItemsFromContainer (KickStarter.playerInput.activeContainer);
					}
				}
				else
				{
					newItemList = new List<InvItem>();
					foreach (InvItem _item in KickStarter.runtimeInventory.localItems)
					{
						newItemList.Add (_item);
					}
				}
			}
			else
			{
				newItemList = new List<InvItem>();
				if (AdvGame.GetReferences ().inventoryManager)
				{
					foreach (InvItem _item in AdvGame.GetReferences ().inventoryManager.items)
					{
						newItemList.Add (_item);
						if (_item != null)
						{
							_item.recipeSlot = -1;
						}
					}
				}
			}

			if (Application.isPlaying && 
				(inventoryBoxType == AC_InventoryBoxType.Default || inventoryBoxType == AC_InventoryBoxType.CustomScript))
			{
				while (AreAnyItemsInRecipe (newItemList))
				{
					foreach (InvItem _item in newItemList)
					{
						if (_item != null && _item.recipeSlot > -1)
						{
							if (AdvGame.GetReferences ().settingsManager.canReorderItems)
								newItemList [newItemList.IndexOf (_item)] = null;
							else
								newItemList.Remove (_item);
							break;
						}
					}
				}
			}

			if (doLimit && CanBeLimitedByCategory ())
			{
				newItemList = LimitByCategory (newItemList, 0).LimitedItems;
			}

			return newItemList;
		}


		private List<InvItem> GetItemsFromContainer (Container container)
		{
			List<InvItem> newItemList = new List<InvItem>();
			newItemList.Clear ();
			foreach (ContainerItem containerItem in container.items)
			{
				InvItem referencedItem = new InvItem (KickStarter.inventoryManager.GetItem (containerItem.linkedID));
				referencedItem.count = containerItem.count;
				newItemList.Add (referencedItem);
			}
			return newItemList;
		}


		private bool CanBeLimitedByCategory ()
		{
			if (inventoryBoxType == AC_InventoryBoxType.Default ||
				inventoryBoxType == AC_InventoryBoxType.CustomScript ||
				inventoryBoxType == AC_InventoryBoxType.DisplaySelected ||
				inventoryBoxType == AC_InventoryBoxType.DisplayLastSelected)
			{
				return true;
			}

			if (inventoryBoxType == AC_InventoryBoxType.HotspotBased && !limitToDefinedInteractions && !ForceLimitByReference ())
			{
				return true;
			}

			return false;
		}


		/**
		 * <summary>Checks if the element's slots can be shifted in a particular direction.</summary>
		 * <param name = "shiftType">The direction to shift slots in (Left, Right)</param>
		 * <returns>True if the element's slots can be shifted in the particular direction</returns>
		 */
		public override bool CanBeShifted (AC_ShiftInventory shiftType)
		{
			if (items.Count == 0)
			{
				return false;
			}

			if (shiftType == AC_ShiftInventory.ShiftPrevious)
			{
				if (offset == 0)
				{
					return false;
				}
			}
			else
			{
				if ((maxSlots + offset) >= items.Count)
				{
					return false;
				}
			}
			return true;
		}


		private bool AreAnyItemsInRecipe (List<InvItem> _itemList)
		{
			foreach (InvItem item in _itemList)
			{
				if (item != null && item.recipeSlot >= 0)
				{
					return true;
				}
			}
			return false;
		}


		private LimitedItemList LimitByCategory (List<InvItem> itemsToLimit, int reverseItemIndex)
		{
			int offset = 0;

			List<InvItem> nonLinkedItemsToLimit = new List<InvItem>();
			foreach (InvItem itemToLimit in itemsToLimit)
			{
				nonLinkedItemsToLimit.Add (itemToLimit);
			}

			if (limitToCategory && categoryIDs.Count > 0)
			{
				for (int i=0; i<nonLinkedItemsToLimit.Count; i++)
				{
					if (nonLinkedItemsToLimit[i] != null && !categoryIDs.Contains (nonLinkedItemsToLimit[i].binID))
					{
						if (i <= reverseItemIndex)
						{
							offset ++;
						}

						nonLinkedItemsToLimit.RemoveAt (i);
						i = -1;
					}
				}

				// Bugfix: Remove extra nulls at end in case some where added as a result of re-ordering another menu
				if (nonLinkedItemsToLimit != null && Application.isPlaying)
				{
					nonLinkedItemsToLimit = KickStarter.runtimeInventory.RemoveEmptySlots (nonLinkedItemsToLimit);
				}
			}

			return new LimitedItemList (nonLinkedItemsToLimit, offset);
		}
		

		/**
		 * <summary>Shifts which slots are on display, if the number of slots the element has exceeds the number of slots it can show at once.</summary>
		 * <param name = "shiftType">The direction to shift slots in (Left, Right)</param>
		 * <param name = "amount">The amount to shift slots by</param>
		 */
		public override void Shift (AC_ShiftInventory shiftType, int amount)
		{
			if (numSlots >= maxSlots)
			{
				Shift (shiftType, maxSlots, items.Count, amount);
			}
		}


		private Texture GetTexture (InvItem _item, bool isActive)
		{
			if (ItemIsSelected (_item))
			{
				switch (KickStarter.settingsManager.selectInventoryDisplay)
				{
				case SelectInventoryDisplay.ShowSelectedGraphic:
					return _item.selectedTex;

				case SelectInventoryDisplay.ShowHoverGraphic:
					return _item.activeTex;

				default:
					break;
				}
			}
			else if (isActive && KickStarter.settingsManager.activeWhenHover)
			{
				return _item.activeTex;
			}
			return _item.tex;
		}
		
		
		private void DrawTexture (Rect rect, InvItem _item, bool isActive)
		{
			if (_item == null) return;

			Texture tex = null;
			if (Application.isPlaying && KickStarter.runtimeInventory != null && inventoryBoxType != AC_InventoryBoxType.DisplaySelected)
			{
				if (_item == KickStarter.runtimeInventory.highlightItem && _item.activeTex != null)
				{
					KickStarter.runtimeInventory.DrawHighlighted (rect);
					return;
				}

				if (inventoryBoxType != AC_InventoryBoxType.DisplaySelected && inventoryBoxType != AC_InventoryBoxType.DisplayLastSelected)
				{
					tex = GetTexture (_item, isActive);
				}

				if (tex == null)
				{
					tex = _item.tex;
				}
			}
			else if (_item.tex != null)
			{
				tex = _item.tex;
			}

			if (tex != null)
			{
				GUI.DrawTexture (rect, tex, ScaleMode.StretchToFill, true, 0f);
			}
		}


		/**
		 * <summary>Gets the display text of the element</summary>
		 * <param name = "slot">The index number of the slot</param>
		 * <param name = "languageNumber">The index number of the language number to get the text in</param>
		 * <returns>The display text of the element's slot, or the whole element if it only has one slot</returns>
		 */
		public override string GetLabel (int i, int languageNumber)
		{
			if (items.Count <= (i+offset) || items [i+offset] == null)
			{
				return null;
			}
			return items [i+offset].GetLabel (languageNumber);
		}


		public override bool IsSelectedByEventSystem (int slotIndex)
		{
			if (uiSlots != null && slotIndex >= 0 && uiSlots.Length > slotIndex && uiSlots[slotIndex] != null && uiSlots[slotIndex].uiButton != null)
			{
				return KickStarter.playerMenus.IsEventSystemSelectingObject (uiSlots[slotIndex].uiButton.gameObject);
			}
			return false;
		}


		/**
		 * <summary>Gets the inventory item shown in a specific slot</summary>
		 * <param name = "i">The index number of the slot</param>
		 * <returns>The inventory item shown in the slot</returns>
		 */
		public InvItem GetItem (int i)
		{
			if (items.Count <= (i+offset) || items [i+offset] == null)
			{
				return null;
			}

			return items [i+offset];
		}


		private string GetCount (int i)
		{
			if (Application.isPlaying)
			{
				if (items.Count <= (i+offset) || items [i+offset] == null)
				{
					return "";
				}

				if (items [i+offset].count < 2)
				{
					return "";
				}

				if (ItemIsSelected (items [i+offset]) && items [i+offset].CanSelectSingle ())
				{
					return (items [i+offset].count-1).ToString ();
				}

				return items [i + offset].count.ToString ();
			}

			if (items[i+offset].canCarryMultiple && !items[i+offset].useSeparateSlots && items[i+offset].count > 1)
			{
				return items[i+offset].count.ToString ();
			}
			return "";
		}


		/**
		 * Re-sets the "shift" offset, so that the first InvItem shown is the first InvItem in items.
		 */
		public void ResetOffset ()
		{
			offset = 0;
		}
		
		
		protected override void AutoSize ()
		{
			if (items.Count > 0)
			{
				foreach (InvItem _item in items)
				{
					if (_item != null)
					{
						if (displayType == ConversationDisplayType.IconOnly)
						{
							AutoSize (new GUIContent (_item.tex));
						}
						else if (displayType == ConversationDisplayType.TextOnly)
						{
							AutoSize (new GUIContent (_item.label));
						}
						return;
					}
				}
			}
			AutoSize (GUIContent.none);
		}


		/**
		 * <summary>Performs what should happen when the element is clicked on, if inventoryBoxType = AC_InventoryBoxType.Container.</summary>
		 * <param name = "_mouseState">The state of the mouse button</param>
		 * <param name = "_slot">The index number of the slot that was clicked on</param>
		 */
		public void ClickContainer (MouseState _mouseState, int _slot)
		{
			Container container = (overrideContainer != null) ? overrideContainer : KickStarter.playerInput.activeContainer;

			if (container == null || KickStarter.runtimeInventory == null) return;

			KickStarter.runtimeInventory.SetFont (font, GetFontSize (), fontColor, textEffects);

			if (_mouseState == MouseState.SingleClick)
			{
				if (KickStarter.runtimeInventory.SelectedItem == null)
				{
					if (container.items.Count > (_slot+offset) && container.items [_slot+offset] != null)
					{
						ContainerItem containerItem = container.items [_slot + offset];

						// Prevent if player is already carrying one, and multiple can't be carried
						InvItem invItem = KickStarter.inventoryManager.GetItem (containerItem.linkedID);
						if (KickStarter.runtimeInventory.IsCarryingItem (invItem.id) && !invItem.canCarryMultiple)
						{
							KickStarter.eventManager.Call_OnUseContainerFail (container, containerItem);
							return;
						}

						KickStarter.eventManager.Call_OnUseContainer (false, container, containerItem);
						if (KickStarter.inventoryManager.GetItem (containerItem.linkedID).CanSelectSingle (containerItem.count))
						{
							// Only take one
							KickStarter.runtimeInventory.Add (containerItem.linkedID, 1, selectItemsAfterTaking, -1);
							container.items [_slot+offset].count -= 1;
						}
						else
						{
							KickStarter.runtimeInventory.Add (containerItem.linkedID, containerItem.count, selectItemsAfterTaking, -1);
							container.items.Remove (containerItem);
						}
					}
				}
				else
				{
					// Placing an item inside the container
					int numToChange = (KickStarter.runtimeInventory.SelectedItem.CanSelectSingle ()) ? 1 : 0;
					ContainerItem containerItem = container.InsertAt (KickStarter.runtimeInventory.SelectedItem, _slot+offset, numToChange);
					if (containerItem != null)
					{
						KickStarter.runtimeInventory.Remove (KickStarter.runtimeInventory.SelectedItem, numToChange);
						KickStarter.eventManager.Call_OnUseContainer (true, container, containerItem);
					}
				}
			}

			else if (_mouseState == MouseState.RightClick)
			{
				if (KickStarter.runtimeInventory.SelectedItem != null)
				{
					KickStarter.runtimeInventory.SetNull ();
				}
			}
		}


		/**
		 * <summary>Performs what should happen when the element is clicked on.</summary>
		 * <param name = "_menu">The element's parent Menu</param>
		 * <param name = "_slot">The index number of ths slot that was clicked</param>
		 * <param name = "_mouseState The state of the mouse button</param>
		 */
		public override void ProcessClick (AC.Menu _menu, int _slot, MouseState _mouseState)
		{
			if (KickStarter.stateHandler.gameState == GameState.Cutscene)
			{
				return;
			}

			base.ProcessClick (_menu, _slot, _mouseState);

			if (_mouseState == MouseState.SingleClick)
			{
				KickStarter.runtimeInventory.lastClickedItem = GetItem (_slot);
			}

			if (inventoryBoxType == AC_InventoryBoxType.CustomScript)
			{
				MenuSystem.OnElementClick (_menu, this, _slot, (int) _mouseState);
			}
			else
			{
				KickStarter.runtimeInventory.ProcessInventoryBoxClick (_menu, this, _slot, _mouseState);
			}
		}


		/**
		 * <summary>Gets the slot index number that a given InvItem (inventory item) appears in.</summary>
		 * <param name = "itemID">The ID number of the InvItem to search for</param>
		 * <returns>The slot index number that the inventory item appears in</returns>
		 */
		public int GetItemSlot (int itemID)
		{
			foreach (InvItem invItem in items)
			{
				if (invItem != null && invItem.id == itemID)
				{
					return items.IndexOf (invItem) - offset;
				}
			}
			return 0;
		}


		private bool ForceLimitByReference ()
		{
			if (KickStarter.settingsManager != null && KickStarter.settingsManager.interactionMethod == AC_InteractionMethod.ChooseHotspotThenInteraction &&
				KickStarter.settingsManager.cycleInventoryCursors &&
				(KickStarter.settingsManager.selectInteractions == SelectInteractions.CyclingCursorAndClickingHotspot || KickStarter.settingsManager.selectInteractions == SelectInteractions.CyclingMenuAndClickingHotspot))
			{
				return true;
			}
			return false;
		}


		/**
		 * If set, and inventoryBoxType = AC_InventoryBoxType.Container, then this Container will be used instead of the global 'active' one.  Note that its Menu's 'Appear type' should not be set to 'On Container'.
		 */
		public Container OverrideContainer
		{
			set
			{
				overrideContainer = value;
			}
		}


		private struct LimitedItemList
		{

			List<InvItem> limitedItems;
			int offset;

	
			public LimitedItemList (List<InvItem> _limitedItems, int _offset)
			{
				limitedItems = _limitedItems;
				offset = _offset;
			}


			public List<InvItem> LimitedItems
			{
				get
				{
					return limitedItems;
				}
			}


			public int Offset
			{
				get
				{
					return offset;
				}
			}

		}

	}

}