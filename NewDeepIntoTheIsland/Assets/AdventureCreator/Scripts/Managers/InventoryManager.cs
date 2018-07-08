/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"InventoryManager.cs"
 * 
 *	This script handles the "Inventory" tab of the main wizard.
 *	Inventory items are defined with this.
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
	 * Handles the "Inventory" tab of the Game Editor window.
	 * All inventory items, inventory categories and recipes are defined here.
	 */
	[System.Serializable]
	public class InventoryManager : ScriptableObject
	{
		
		/** The game's full list of inventory items */
		public List<InvItem> items = new List<InvItem>();
		/** The game's full list of inventory item categories */
		public List<InvBin> bins = new List<InvBin>();
		/** The game's full list of inventory item properties */
		public List<InvVar> invVars = new List<InvVar>();
		/** The default ActionListAsset to run if an inventory combination is unhandled */
		public ActionListAsset unhandledCombine;
		/** The default ActionListAsset to run if using an inventory item on a Hotspot is unhandled */
		public ActionListAsset unhandledHotspot;
		/** If True, the Hotspot clicked on to initiate unhandledHotspot will be sent as a parameter to the ActionListAsset */
		public bool passUnhandledHotspotAsParameter;
		/** The default ActionListAsset to run if giving an inventory item to an NPC is unhandled */
		public ActionListAsset unhandledGive;
		/** The game's full list of available recipes */
		public List<Recipe> recipes = new List<Recipe>();
		
		#if UNITY_EDITOR
		
		private SettingsManager settingsManager;
		private CursorManager cursorManager;
		
		private FilterInventoryItem filterType;
		private string nameFilter = "";
		private int categoryFilter = -1;
		private bool filterOnStart = false;
		
		private InvItem selectedItem;
		private InvVar selectedInvVar;
		private Recipe selectedRecipe;
		private int sideItem = -1;
		private int invNumber = 0;
		private int binNumber = -1;
		
		private Vector2 scrollPos;
		private bool showItemsTab = true;
		private bool showBinsTab = false;
		private bool showCraftingTab = false;
		private bool showPropertiesTab = false;

		private bool showUnhandledEvents = true;
		private bool showItemList = true;
		private bool showItemProperties = true;

		private bool showCategories = true;
		private bool showCraftingList = true;
		private bool showCraftingProperties = true;
		private bool showPropertiesList = true;
		private bool showPropertiesProperties = true;

		private string[] boolType = {"False", "True"};
		
		private static GUILayoutOption
			buttonWidth = GUILayout.MaxWidth (20f);
		
		private static GUIContent
			deleteContent = new GUIContent("-", "Delete item");
		
		
		/**
		 * Shows the GUI.
		 */
		public void ShowGUI ()
		{
			if (AdvGame.GetReferences ())
			{
				if (AdvGame.GetReferences ().settingsManager)
				{
					settingsManager = AdvGame.GetReferences ().settingsManager;
				}
				if (AdvGame.GetReferences ().cursorManager)
				{
					cursorManager = AdvGame.GetReferences ().cursorManager;
				}
			}
			
			EditorGUILayout.Space ();
			GUILayout.BeginHorizontal ();

			string label = (items.Count > 0) ? ("Items (" + items.Count + ")") : "Items";
			if (GUILayout.Toggle (showItemsTab, label, "toolbarbutton"))
			{
				SetTab (0);
			}

			label = (bins.Count > 0) ? ("Categories (" + bins.Count + ")") : "Categories";
			if (GUILayout.Toggle (showBinsTab,  label, "toolbarbutton"))
			{
				SetTab (1);
			}

			label = (recipes.Count > 0) ? ("Crafting (" + recipes.Count + ")") : "Crafting";
			if (GUILayout.Toggle (showCraftingTab, label, "toolbarbutton"))
			{
				SetTab (2);
			}

			label = (invVars.Count > 0) ? ("Properties (" + invVars.Count + ")") : "Properties";
			if (GUILayout.Toggle (showPropertiesTab, label, "toolbarbutton"))
			{
				SetTab (3);
			}
			GUILayout.EndHorizontal ();
			EditorGUILayout.Space ();
			
			if (showBinsTab)
			{
				BinsGUI ();
			}
			else if (showCraftingTab)
			{
				CraftingGUI ();
			}
			else if (showItemsTab)
			{
				ItemsGUI ();
			}
			else if (showPropertiesTab)
			{
				PropertiesGUI ();
			}
			
			if (GUI.changed)
			{
				EditorUtility.SetDirty (this);
			}
		}
		
		
		private void ItemsGUI ()
		{
			EditorGUILayout.BeginVertical (CustomStyles.thinBox);
			showUnhandledEvents = CustomGUILayout.ToggleHeader (showUnhandledEvents, "Global unhandled events");
			if (showUnhandledEvents)
			{
				unhandledCombine = ActionListAssetMenu.AssetGUI ("Combine:", unhandledCombine, "AC.KickStarter.runtimeInventory.unhandledCombine", "Inventory_Unhandled_Combine");
				unhandledHotspot = ActionListAssetMenu.AssetGUI ("Use on hotspot:", unhandledHotspot, "AC.KickStarter.runtimeInventory.unhandledHotspot", "Inventory_Unhandled_Hotspot");
				if (settingsManager != null && settingsManager.CanGiveItems ())
				{
					unhandledGive = ActionListAssetMenu.AssetGUI ("Give to NPC:", unhandledGive, "AC.KickStarter.runtimeInventory.unhandledGive", "Inventory_Unhandled_Give");
				}

				if (unhandledHotspot != null)
				{
					passUnhandledHotspotAsParameter = CustomGUILayout.ToggleLeft ("Pass Hotspot as GameObject parameter?", passUnhandledHotspotAsParameter, "AC.KickStarter.inventoryManager.passUnhandledHotspotAsParameter");
					if (passUnhandledHotspotAsParameter)
					{
						EditorGUILayout.HelpBox ("The Hotspot will be set as " + unhandledHotspot.name + "'s first parameter, which must be set to type 'GameObject'.", MessageType.Info);
					}
				}
			}
			EditorGUILayout.EndVertical ();

			List<string> binList = new List<string>();
			foreach (InvBin bin in bins)
			{
				binList.Add (bin.label);
			}

			EditorGUILayout.Space ();
			CreateItemsGUI (binList.ToArray ());
			EditorGUILayout.Space ();
			
			if (selectedItem != null && items.Contains (selectedItem))
			{
				string apiPrefix = "AC.KickStarter.runtimeInventory.GetItem (" + selectedItem.id + ")";

				EditorGUILayout.BeginVertical (CustomStyles.thinBox);
				showItemProperties = CustomGUILayout.ToggleHeader (showItemProperties, "Inventory item '" + selectedItem.label + "' settings");
				if (showItemProperties)
				{
					selectedItem.label = CustomGUILayout.TextField ("Name:", selectedItem.label, apiPrefix + ".label");
					selectedItem.altLabel = CustomGUILayout.TextField ("Label (if not name):", selectedItem.altLabel, apiPrefix + ".altLabel");
					
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Category:", GUILayout.Width (146f));
					if (bins.Count > 0)
					{
						binNumber = GetBinSlot (selectedItem.binID);
						binNumber = CustomGUILayout.Popup (binNumber, binList.ToArray(), apiPrefix + ".binID");
						selectedItem.binID = bins[binNumber].id;
					}
					else
					{
						selectedItem.binID = -1;
						EditorGUILayout.LabelField ("No categories defined!", EditorStyles.miniLabel, GUILayout.Width (146f));
					}
					EditorGUILayout.EndHorizontal ();

					selectedItem.carryOnStart = CustomGUILayout.Toggle ("Carry on start?", selectedItem.carryOnStart, apiPrefix + ".carryOnStart");
					if (selectedItem.carryOnStart && AdvGame.GetReferences ().settingsManager && AdvGame.GetReferences ().settingsManager.playerSwitching == PlayerSwitching.Allow && !AdvGame.GetReferences ().settingsManager.shareInventory)
					{
						selectedItem.carryOnStartNotDefault = CustomGUILayout.Toggle ("Give to non-default player?", selectedItem.carryOnStartNotDefault, apiPrefix + ".carryOnStartNotDefault");
						if (selectedItem.carryOnStartNotDefault)
						{
							selectedItem.carryOnStartID = ChoosePlayerGUI (selectedItem.carryOnStartID, apiPrefix + ".carryOnStartID");
						}
					}
					
					selectedItem.canCarryMultiple = CustomGUILayout.Toggle ("Can carry multiple?", selectedItem.canCarryMultiple, apiPrefix + ".canCarryMultiple");
					if (selectedItem.canCarryMultiple)
					{
						selectedItem.useSeparateSlots = CustomGUILayout.Toggle ("Place in separate slots?", selectedItem.useSeparateSlots, apiPrefix + ".useSeparateSlots");

						if (!selectedItem.useSeparateSlots)
						{
							selectedItem.selectSingle = CustomGUILayout.Toggle ("Select one-at-a-time?", selectedItem.selectSingle, apiPrefix + ".selectSingle");
						}
					}
					if (selectedItem.carryOnStart && selectedItem.canCarryMultiple)
					{
						selectedItem.count = CustomGUILayout.IntField ("Quantity on start:", selectedItem.count, apiPrefix + ".count");
					}
					else
					{
						selectedItem.count = 1;
					}
					
					selectedItem.overrideUseSyntax = CustomGUILayout.Toggle ("Override 'Use' syntax?", selectedItem.overrideUseSyntax, apiPrefix + ".overrideUseSyntax");
					if (selectedItem.overrideUseSyntax)
					{
						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField ("Use syntax:", GUILayout.Width (100f));
						selectedItem.hotspotPrefix1.label = EditorGUILayout.TextField (selectedItem.hotspotPrefix1.label, GUILayout.MaxWidth (80f));
						EditorGUILayout.LabelField ("(item)", GUILayout.MaxWidth (40f));
						selectedItem.hotspotPrefix2.label = EditorGUILayout.TextField (selectedItem.hotspotPrefix2.label, GUILayout.MaxWidth (80f));
						EditorGUILayout.LabelField ("(hotspot)", GUILayout.MaxWidth (55f));
						EditorGUILayout.EndHorizontal ();
					}

					selectedItem.linkedPrefab = (GameObject) CustomGUILayout.ObjectField <GameObject> ("Linked prefab:", selectedItem.linkedPrefab, false, apiPrefix + ".linkedPrefab");
					if (selectedItem.linkedPrefab != null)
					{
						EditorGUILayout.HelpBox ("This reference is only accessibly through scripting.", MessageType.Info);
					}

					GUILayout.Box ("", GUILayout.ExpandWidth (true), GUILayout.Height (1));

					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Main graphic:", GUILayout.Width (145));
					selectedItem.tex = (Texture) CustomGUILayout.ObjectField <Texture> (selectedItem.tex, false, GUILayout.Width (70), GUILayout.Height (70), apiPrefix + ".tex");
					EditorGUILayout.EndHorizontal ();

					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Active graphic:", GUILayout.Width (145));
					selectedItem.activeTex = (Texture) CustomGUILayout.ObjectField <Texture> (selectedItem.activeTex, false, GUILayout.Width (70), GUILayout.Height (70), apiPrefix + ".activeTex");
					EditorGUILayout.EndHorizontal ();


					if (AdvGame.GetReferences ().settingsManager != null && AdvGame.GetReferences ().settingsManager.selectInventoryDisplay == SelectInventoryDisplay.ShowSelectedGraphic)
					{
						selectedItem.selectedTex = (Texture) CustomGUILayout.ObjectField <Texture> ("Selected graphic:", selectedItem.selectedTex, false, apiPrefix + ".selectedTex");
					}
					if (AdvGame.GetReferences ().cursorManager != null)
					{
						CursorManager cursorManager = AdvGame.GetReferences ().cursorManager;
						if (cursorManager.inventoryHandling == InventoryHandling.ChangeCursor || cursorManager.inventoryHandling == InventoryHandling.ChangeCursorAndHotspotLabel)
						{
							GUILayout.Box ("", GUILayout.ExpandWidth (true), GUILayout.Height (1));
							selectedItem.cursorIcon.ShowGUI (true, true, "Cursor (optional):", cursorManager.cursorRendering, apiPrefix + ".cursorIcon");
							GUILayout.Box ("", GUILayout.ExpandWidth (true), GUILayout.Height (1));
						}
					}

					EditorGUILayout.Space ();
					EditorGUILayout.LabelField ("Standard interactions",  CustomStyles.subHeader);
					if (settingsManager && settingsManager.interactionMethod != AC_InteractionMethod.ContextSensitive && settingsManager.inventoryInteractions == InventoryInteractions.Multiple && AdvGame.GetReferences ().cursorManager)
					{
						CursorManager cursorManager = AdvGame.GetReferences ().cursorManager;
						
						List<string> iconList = new List<string>();
						foreach (CursorIcon icon in cursorManager.cursorIcons)
						{
							iconList.Add (icon.label);
						}
						
						if (cursorManager.cursorIcons.Count > 0)
						{
							foreach (InvInteraction interaction in selectedItem.interactions)
							{
								EditorGUILayout.BeginHorizontal ();
								invNumber = GetIconSlot (interaction.icon.id);
								invNumber = EditorGUILayout.Popup (invNumber, iconList.ToArray());
								interaction.icon = cursorManager.cursorIcons[invNumber];

								int i = selectedItem.interactions.IndexOf (interaction);
								string autoName = selectedItem.label + "_" + interaction.icon.label;
								interaction.actionList = ActionListAssetMenu.AssetGUI ("", interaction.actionList, apiPrefix + ".interactions[" + i + "].actionList", autoName);
								
								if (GUILayout.Button (deleteContent, EditorStyles.miniButtonRight, buttonWidth))
								{
									Undo.RecordObject (this, "Delete interaction");
									selectedItem.interactions.Remove (interaction);
									break;
								}
								EditorGUILayout.EndHorizontal ();
							}
						}
						else
						{
							EditorGUILayout.HelpBox ("No interaction icons defined - please use the Cursor Manager", MessageType.Warning);
						}
						if (GUILayout.Button ("Add interaction"))
						{
							Undo.RecordObject (this, "Add new interaction");
							selectedItem.interactions.Add (new InvInteraction (cursorManager.cursorIcons[0]));
						}
					}
					else
					{
						string autoName = selectedItem.label + "_Use";
						selectedItem.useActionList = ActionListAssetMenu.AssetGUI ("Use:", selectedItem.useActionList, apiPrefix + ".useActionList", autoName);
						if (cursorManager && cursorManager.allowInteractionCursorForInventory && cursorManager.cursorIcons.Count > 0)
						{
							int useCursor_int = cursorManager.GetIntFromID (selectedItem.useIconID) + 1;
							if (selectedItem.useIconID == -1) useCursor_int = 0;
							useCursor_int = CustomGUILayout.Popup ("Use cursor icon:", useCursor_int, cursorManager.GetLabelsArray (true), apiPrefix + ".useIconID");

							if (useCursor_int == 0)
							{
								selectedItem.useIconID = -1;
							}
							else if (cursorManager.cursorIcons.Count > (useCursor_int - 1))
							{
								selectedItem.useIconID = cursorManager.cursorIcons[useCursor_int-1].id;
							}
						}
						else
						{
							selectedItem.useIconID = 0;
						}
						autoName = selectedItem.label + "_Examine";
						selectedItem.lookActionList = ActionListAssetMenu.AssetGUI ("Examine:", selectedItem.lookActionList, apiPrefix + ".lookActionList", autoName);
					}
					
					if (settingsManager != null && settingsManager.CanSelectItems (false))
					{
						EditorGUILayout.Space ();
						EditorGUILayout.LabelField ("Unhandled interactions",  CustomStyles.subHeader);
						string autoName = selectedItem.label + "_Unhandled_Hotspot";
						selectedItem.unhandledActionList = ActionListAssetMenu.AssetGUI ("Unhandled use on Hotspot:", selectedItem.unhandledActionList, apiPrefix + ".unhandledActionList", autoName);
						autoName = selectedItem.label + "_Unhandled_Combine";
						selectedItem.unhandledCombineActionList = ActionListAssetMenu.AssetGUI ("Unhandled combine:", selectedItem.unhandledCombineActionList, apiPrefix + ".unhandledCombineActionList", autoName);
					}
					
					EditorGUILayout.Space ();
					EditorGUILayout.LabelField ("Combine interactions",  CustomStyles.subHeader);
					for (int i=0; i<selectedItem.combineActionList.Count; i++)
					{
						EditorGUILayout.BeginHorizontal ();
						invNumber = GetArraySlot (selectedItem.combineID[i]);
						invNumber = EditorGUILayout.Popup (invNumber, GetLabelList ());
						selectedItem.combineID[i] = items[invNumber].id;

						string autoName = selectedItem.label + "_Combine_" + GetLabelList () [invNumber];
						selectedItem.combineActionList[i] = ActionListAssetMenu.AssetGUI ("", selectedItem.combineActionList[i], apiPrefix + ".combineActionList[" + i + "]", autoName);
						
						if (GUILayout.Button (deleteContent, EditorStyles.miniButtonRight, buttonWidth))
						{
							Undo.RecordObject (this, "Delete combine event");
							selectedItem.combineActionList.RemoveAt (i);
							selectedItem.combineID.RemoveAt (i);
							break;
						}
						EditorGUILayout.EndHorizontal ();
					}
					if (GUILayout.Button ("Add combine event"))
					{
						Undo.RecordObject (this, "Add new combine event");
						selectedItem.combineActionList.Add (null);
						selectedItem.combineID.Add (0);
					}
					
					// List all "reverse" inventory combinations
					string reverseCombinations = "";
					foreach (InvItem otherItem in items)
					{
						if (otherItem != selectedItem)
						{
							if (otherItem.combineID.Contains (selectedItem.id))
							{
								reverseCombinations += "- " + otherItem.label + "\n";
								continue;
							}
						}
					}
					if (reverseCombinations.Length > 0)
					{
						EditorGUILayout.Space ();
						EditorGUILayout.HelpBox ("The following inventory items have combine interactions that reference this item:\n" + reverseCombinations, MessageType.Info);
					}
					
					if (invVars.Count > 0)
					{
						EditorGUILayout.Space ();
						EditorGUILayout.LabelField ("Properties",  CustomStyles.subHeader);
						
						RebuildProperties (selectedItem);
						
						// UI for setting property values
						if (selectedItem.vars.Count > 0)
						{
							foreach (InvVar invVar in selectedItem.vars)
							{
								string label = invVar.label + ":";
								if (string.IsNullOrEmpty (invVar.label))
								{
									label = "Property " + invVar.id.ToString () + ":";
								}
								
								if (invVar.type == VariableType.Boolean)
								{
									if (invVar.val != 1)
									{
										invVar.val = 0;
									}
									invVar.val = CustomGUILayout.Popup (label, invVar.val, boolType, apiPrefix + ".GetProperty (" + invVar.id + ").val");
								}
								else if (invVar.type == VariableType.Integer)
								{
									invVar.val = CustomGUILayout.IntField (label, invVar.val, apiPrefix + ".GetProperty (" + invVar.id + ").val");
								}
								else if (invVar.type == VariableType.PopUp)
								{
									invVar.val = CustomGUILayout.Popup (label, invVar.val, invVar.popUps, apiPrefix + ".GetProperty (" + invVar.id + ").val");
								}
								else if (invVar.type == VariableType.String)
								{
									invVar.textVal = CustomGUILayout.TextField (label, invVar.textVal, apiPrefix + ".GetProperty (" + invVar.id + ").textVal");
								}
								else if (invVar.type == VariableType.Float)
								{
									invVar.floatVal = CustomGUILayout.FloatField (label, invVar.floatVal, apiPrefix + ".GetProperty (" + invVar.id + ").floatVal");
								}
							}
						}
						else
						{
							EditorGUILayout.HelpBox ("No properties have been defined that this inventory item can use.", MessageType.Info);
						}
					}
				}
				EditorGUILayout.EndVertical ();
			}
		}
		
		
		private void BinsGUI ()
		{
			EditorGUILayout.BeginVertical (CustomStyles.thinBox);
			showCategories = CustomGUILayout.ToggleHeader (showCategories, "Categories");
			if (showCategories)
			{
				foreach (InvBin bin in bins)
				{
					EditorGUILayout.BeginHorizontal ();

					EditorGUILayout.LabelField (bin.id.ToString () + ":", GUILayout.Width (20f));
					bin.label = CustomGUILayout.TextField ("", bin.label, "AC.KickStarter.inventoryManager.GetCategory (" + bin.id + ").label");
					
					if (GUILayout.Button (deleteContent, EditorStyles.miniButton, GUILayout.MaxWidth(20f)))
					{
						Undo.RecordObject (this, "Delete category: " + bin.label);
						bins.Remove (bin);
						break;
					}
					EditorGUILayout.EndHorizontal ();
					
				}
				if (GUILayout.Button ("Create new category"))
				{
					Undo.RecordObject (this, "Add category");
					List<int> idArray = new List<int>();
					foreach (InvBin bin in bins)
					{
						idArray.Add (bin.id);
					}
					idArray.Sort ();
					bins.Add (new InvBin (idArray.ToArray ()));
				}
			}
			EditorGUILayout.EndVertical ();
		}
		
		
		private void PropertiesGUI ()
		{
			List<string> binList = new List<string>();
			foreach (InvBin bin in bins)
			{
				binList.Add (bin.label);
			}
			
			CreatePropertiesGUI ();
			EditorGUILayout.Space ();
			
			if (selectedInvVar != null && invVars.Contains (selectedInvVar))
			{
				string apiPrefix = "AC.KickStarter.variablesManager.GetProperty (" + selectedInvVar.id + ")";
				EditorGUILayout.BeginVertical (CustomStyles.thinBox);

				showPropertiesProperties = CustomGUILayout.ToggleHeader (showPropertiesProperties, "Inventory property '" + selectedInvVar.label + "' properties");
				if (showPropertiesProperties)
				{
					selectedInvVar.label = CustomGUILayout.TextField ("Name:", selectedInvVar.label, apiPrefix + ".label");
					selectedInvVar.type = (VariableType) CustomGUILayout.EnumPopup ("Type:", selectedInvVar.type, apiPrefix + ".type");

					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Internal description:", GUILayout.MaxWidth (146f));
					selectedInvVar.description = EditorGUILayout.TextArea (selectedInvVar.description);
					EditorGUILayout.EndHorizontal ();

					if (selectedInvVar.type == VariableType.PopUp)
					{
						selectedInvVar.popUps = VariablesManager.PopupsGUI (selectedInvVar.popUps);
					}
					
					selectedInvVar.limitToCategories = EditorGUILayout.BeginToggleGroup ("Limit to set categories?", selectedInvVar.limitToCategories);

					if (bins.Count > 0)
					{
						List<int> newCategoryIDs = new List<int>();
						foreach (InvBin bin in bins)
						{
							bool usesCategory = false;
							if (selectedInvVar.categoryIDs.Contains (bin.id))
							{
								usesCategory = true;
							}
							usesCategory = CustomGUILayout.Toggle ("Use in '" + bin.label + "'?", usesCategory, apiPrefix + ".categoryIDs");
							
							if (usesCategory)
							{
								newCategoryIDs.Add (bin.id);
							}
						}
						selectedInvVar.categoryIDs = newCategoryIDs;
					}
					else if (selectedInvVar.limitToCategories)
					{
						EditorGUILayout.HelpBox ("No categories are defined!", MessageType.Warning);
					}
					EditorGUILayout.EndToggleGroup ();
				}
				EditorGUILayout.EndVertical ();
			}
			
			if (GUI.changed)
			{
				foreach (InvItem item in items)
				{
					RebuildProperties (item);
				}
			}
		}
		
		
		private void RebuildProperties (InvItem item)
		{
			// Which properties are available?
			List<int> availableVarIDs = new List<int>();
			foreach (InvVar invVar in invVars)
			{
				if (!invVar.limitToCategories || bins.Count == 0 || invVar.categoryIDs.Contains (item.binID))
				{
					availableVarIDs.Add (invVar.id);
				}
			}
			
			// Create new properties / transfer existing values
			List<InvVar> newInvVars = new List<InvVar>();
			foreach (InvVar invVar in invVars)
			{
				if (availableVarIDs.Contains (invVar.id))
				{
					InvVar newInvVar = new InvVar (invVar);
					InvVar oldInvVar = item.GetProperty (invVar.id);
					if (oldInvVar != null)
					{
						newInvVar.TransferValues (oldInvVar);
					}
					newInvVars.Add (newInvVar);
				}
			}
			
			item.vars = newInvVars;
		}


		/**
		 * <summary>Gets an inventory property.</summary>
		 * <param name = "ID">The ID number of the property to get</param>
		 * <returns>The inventory property.</returns>
		 */
		public InvVar GetProperty (int ID)
		{
			if (invVars.Count > 0 && ID >= 0)
			{
				foreach (InvVar var in invVars)
				{
					if (var.id == ID)
					{
						return var;
					}
				}
			}
			return null;
		}
		
		
		private void ResetFilter ()
		{
			nameFilter = "";
			categoryFilter = -1;
			filterOnStart = false;
		}
		
		
		private void CreateItemsGUI (string[] binList)
		{
			EditorGUILayout.BeginVertical (CustomStyles.thinBox);
			showItemList = CustomGUILayout.ToggleHeader (showItemList, "Inventory items");
			if (showItemList)
			{
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Filter by:", GUILayout.Width (100f));
				filterType = (FilterInventoryItem) EditorGUILayout.EnumPopup (filterType, GUILayout.Width (80f));
				if (filterType == FilterInventoryItem.Name)
				{
					nameFilter = EditorGUILayout.TextField (nameFilter);
				}
				else if (filterType == FilterInventoryItem.Category)
				{
					if (bins == null || bins.Count == 0)
					{
						categoryFilter = -1;
						EditorGUILayout.HelpBox ("No categories defined!", MessageType.Info);
					}
					else
					{
						categoryFilter = EditorGUILayout.Popup (categoryFilter, binList);
					}
				}
				EditorGUILayout.EndHorizontal ();
				filterOnStart = EditorGUILayout.Toggle ("Filter by 'Carry on start?'?", filterOnStart);
				
				EditorGUILayout.Space ();
				
				scrollPos = EditorGUILayout.BeginScrollView (scrollPos, GUILayout.Height (Mathf.Min (items.Count * 21, 235f)+5));
				foreach (InvItem item in items)
				{
					if ((filterType == FilterInventoryItem.Name && (nameFilter == "" || item.label.ToLower ().Contains (nameFilter.ToLower ()))) ||
					    (filterType == FilterInventoryItem.Category && (categoryFilter == -1 || GetBinSlot (item.binID) == categoryFilter)))
					{
						if (!filterOnStart || item.carryOnStart)
						{
							EditorGUILayout.BeginHorizontal ();
							
							string buttonLabel = item.label;
							if (buttonLabel == "")
							{
								buttonLabel = "(Untitled)";	
							}
							
							if (GUILayout.Toggle (item.isEditing, item.id + ": " + buttonLabel, "Button"))
							{
								if (selectedItem != item)
								{
									DeactivateAllItems ();
									ActivateItem (item);
								}
							}
							
							if (GUILayout.Button (Resource.CogIcon, GUILayout.Width (20f), GUILayout.Height (15f)))
							{
								SideMenu (item);
							}
							
							EditorGUILayout.EndHorizontal ();
						}
					}
				}
				EditorGUILayout.EndScrollView ();

				EditorGUILayout.Space ();
				EditorGUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Create new item"))
				{
					Undo.RecordObject (this, "Create inventory item");
					
					ResetFilter ();
					InvItem newItem = CreateNewItem ();
					DeactivateAllItems ();
					ActivateItem (newItem);
				}

				if (GUILayout.Button (Resource.CogIcon, GUILayout.Width (20f), GUILayout.Height (15f)))
				{
					ExportSideMenu ();
				}
				EditorGUILayout.EndHorizontal ();
			}
			EditorGUILayout.EndVertical ();
		}


		private void ImportItems ()
		{
			bool canProceed = EditorUtility.DisplayDialog ("Import inventory items", "AC will now prompt you for a CSV file to import. It is recommended to back up your project beforehand.", "OK", "Cancel");
			if (!canProceed) return;

			string fileName = EditorUtility.OpenFilePanel ("Import inventory item data", "Assets", "csv");
			if (fileName.Length == 0)
			{
				return;
			}
			
			if (System.IO.File.Exists (fileName))
			{
				string csvText = Serializer.LoadFile (fileName);
				string [,] csvOutput = CSVReader.SplitCsvGrid (csvText);

				InvItemImportWizardWindow.Init (this, csvOutput);
			}
		}


		/**
		 * <summary>Creates a new inventory item</summary>
		 * <param name = "newID">If >= 0, the ID number of the item, if available.  If it is already taken, the item will not be created</param>
		 * <returns>The newly-created item</returns>
		 */
		public InvItem CreateNewItem (int newID = -1)
		{
			List<int> idList = GetIDList ();
			if (newID >= 0)
			{
				if (idList.Contains (newID))
				{
					return null;
				}
				InvItem newItem = new InvItem (newID);
				items.Add (newItem);
				return newItem;
			}
			else
			{
				InvItem newItem = new InvItem (idList.ToArray ());
				items.Add (newItem);
				return newItem;
			}
		}
		
		
		private void CreatePropertiesGUI ()
		{
			EditorGUILayout.BeginVertical (CustomStyles.thinBox);
			showPropertiesList = CustomGUILayout.ToggleHeader (showPropertiesList, "Inventory properties");
			if (showPropertiesList)
			{
				scrollPos = EditorGUILayout.BeginScrollView (scrollPos, GUILayout.Height (Mathf.Min (invVars.Count * 21, 235f)+5));
				foreach (InvVar invVar in invVars)
				{
					EditorGUILayout.BeginHorizontal ();
					
					string buttonLabel = invVar.label;
					if (buttonLabel == "")
					{
						buttonLabel = "(Untitled)";	
					}
					
					if (GUILayout.Toggle (invVar.isEditing, invVar.id + ": " + buttonLabel, "Button"))
					{
						if (selectedInvVar != invVar)
						{
							DeactivateAllInvVars ();
							ActivateItem (invVar);
						}
					}
					
					if (GUILayout.Button (Resource.CogIcon, GUILayout.Width (20f), GUILayout.Height (15f)))
					{
						SideMenu (invVar);
					}
					
					EditorGUILayout.EndHorizontal ();
				}
				EditorGUILayout.EndScrollView ();
				
				if (GUILayout.Button ("Create new property"))
				{
					Undo.RecordObject (this, "Create inventory property");
					
					InvVar newInvVar = new InvVar (GetIDArrayProperty ());
					invVars.Add (newInvVar);
					DeactivateAllInvVars ();
					ActivateItem (newInvVar);
				}
			}
			EditorGUILayout.EndVertical ();
		}
		
		
		private void ActivateItem (InvItem item)
		{
			item.isEditing = true;
			selectedItem = item;
			EditorGUIUtility.editingTextField = false;
		}
		
		
		private void ActivateItem (InvVar invVar)
		{
			invVar.isEditing = true;
			selectedInvVar = invVar;
			EditorGUIUtility.editingTextField = false;
		}
		
		
		private void DeactivateAllItems ()
		{
			foreach (InvItem item in items)
			{
				item.isEditing = false;
			}
			selectedItem = null;
			EditorGUIUtility.editingTextField = false;
		}
		
		
		private void DeactivateAllInvVars ()
		{
			foreach (InvVar invVar in invVars)
			{
				invVar.isEditing = false;
			}
			selectedInvVar = null;
		}
		
		
		private void ActivateRecipe (Recipe recipe)
		{
			recipe.isEditing = true;
			selectedRecipe = recipe;
		}
		
		
		private void DeactivateAllRecipes ()
		{
			foreach (Recipe recipe in recipes)
			{
				recipe.isEditing = false;
			}
			selectedRecipe = null;
		}


		private void ExportSideMenu ()
		{
			GenericMenu menu = new GenericMenu ();
			menu.AddItem (new GUIContent ("Import items..."), false, ExportCallback, "Import");
			menu.AddItem (new GUIContent ("Export items..."), false, ExportCallback, "Export");
			menu.ShowAsContext ();
		}


		private void ExportCallback (object obj)
		{
			switch (obj.ToString ())
			{
				case "Import":
					ImportItems ();
					break;

				case "Export":
					InvItemExportWizardWindow.Init (this);
					break;
			}
		}
		
		
		private void SideMenu (InvItem item)
		{
			GenericMenu menu = new GenericMenu ();
			sideItem = items.IndexOf (item);
			
			menu.AddItem (new GUIContent ("Insert after"), false, Callback, "Insert after");
			if (items.Count > 0)
			{
				menu.AddItem (new GUIContent ("Delete"), false, Callback, "Delete");
			}
			if (sideItem > 0 || sideItem < items.Count-1)
			{
				menu.AddSeparator ("");
			}
			if (sideItem > 0)
			{
				menu.AddItem (new GUIContent ("Re-arrange/Move to top"), false, Callback, "Move to top");
				menu.AddItem (new GUIContent ("Re-arrange/Move up"), false, Callback, "Move up");
			}
			if (sideItem < items.Count-1)
			{
				menu.AddItem (new GUIContent ("Re-arrange/Move down"), false, Callback, "Move down");
				menu.AddItem (new GUIContent ("Re-arrange/Move to bottom"), false, Callback, "Move to bottom");
			}

			menu.AddSeparator ("");
			menu.AddItem (new GUIContent ("Find references"), false, Callback, "Find references");

			menu.ShowAsContext ();
		}
		
		
		private void SideMenu (InvVar invVar)
		{
			GenericMenu menu = new GenericMenu ();
			sideItem = invVars.IndexOf (invVar);
			
			menu.AddItem (new GUIContent ("Insert after"), false, PropertyCallback, "Insert after");
			if (invVars.Count > 0)
			{
				menu.AddItem (new GUIContent ("Delete"), false, PropertyCallback, "Delete");
			}
			if (sideItem > 0 || sideItem < invVars.Count-1)
			{
				menu.AddSeparator ("");
			}
			if (sideItem > 0)
			{
				menu.AddItem (new GUIContent ("Move to top"), false, PropertyCallback, "Move to top");
				menu.AddItem (new GUIContent ("Move up"), false, PropertyCallback, "Move up");
			}
			if (sideItem < invVars.Count-1)
			{
				menu.AddItem (new GUIContent ("Move down"), false, PropertyCallback, "Move down");
				menu.AddItem (new GUIContent ("Move to bottom"), false, PropertyCallback, "Move to bottom");
			}

			menu.ShowAsContext ();
		}
		
		
		private void Callback (object obj)
		{
			if (sideItem >= 0)
			{
				ResetFilter ();
				InvItem tempItem = items[sideItem];
				
				switch (obj.ToString ())
				{
				case "Insert after":
					Undo.RecordObject (this, "Insert item");
					items.Insert (sideItem+1, new InvItem (GetIDList ().ToArray ()));
					break;
					
				case "Delete":
					Undo.RecordObject (this, "Delete item");
					DeactivateAllItems ();
					items.RemoveAt (sideItem);
					break;
					
				case "Move up":
					Undo.RecordObject (this, "Move item up");
					items.RemoveAt (sideItem);
					items.Insert (sideItem-1, tempItem);
					break;
					
				case "Move down":
					Undo.RecordObject (this, "Move item down");
					items.RemoveAt (sideItem);
					items.Insert (sideItem+1, tempItem);
					break;

				case "Move to top":
					Undo.RecordObject (this, "Move item to top");
					items.RemoveAt (sideItem);
					items.Insert (0, tempItem);
					break;

				case "Move to bottom":
					Undo.RecordObject (this, "Move item to bottom");
					items.Add (tempItem);
					items.RemoveAt (sideItem);
					break;

				case "Find references":
					FindReferences (tempItem);
					break;
				}
			}
			
			EditorUtility.SetDirty (this);
			AssetDatabase.SaveAssets ();
			
			sideItem = -1;
		}
		
		
		private void PropertyCallback (object obj)
		{
			if (sideItem >= 0)
			{
				ResetFilter ();
				InvVar tempVar = invVars[sideItem];
				
				switch (obj.ToString ())
				{
				case "Insert after":
					Undo.RecordObject (this, "Insert property");
					invVars.Insert (sideItem+1, new InvVar (GetIDArrayProperty ()));
					break;
					
				case "Delete":
					Undo.RecordObject (this, "Delete property");
					DeactivateAllInvVars ();
					invVars.RemoveAt (sideItem);
					break;
					
				case "Move up":
					Undo.RecordObject (this, "Move property up");
					invVars.RemoveAt (sideItem);
					invVars.Insert (sideItem-1, tempVar);
					break;
					
				case "Move down":
					Undo.RecordObject (this, "Move property down");
					invVars.RemoveAt (sideItem);
					invVars.Insert (sideItem+1, tempVar);
					break;

				case "Move to top":
					Undo.RecordObject (this, "Move property to top");
					invVars.RemoveAt (sideItem);
					invVars.Insert (0, tempVar);
					break;

				case "Move to bottom":
					Undo.RecordObject (this, "Move property to bottom");
					invVars.Add (tempVar);
					invVars.RemoveAt (sideItem);
					break;
				}
			}
			
			EditorUtility.SetDirty (this);
			AssetDatabase.SaveAssets ();
			
			sideItem = -1;
		}
		
		
		private void CraftingGUI ()
		{
			EditorGUILayout.BeginVertical (CustomStyles.thinBox);
			showCraftingList = CustomGUILayout.ToggleHeader (showCraftingList, "Crafting");
			if (showCraftingList)
			{
				if (items.Count == 0)
				{
					EditorGUILayout.HelpBox ("No inventory items defined!", MessageType.Info);
					EditorGUILayout.EndVertical ();
					return;
				}
				
				foreach (Recipe recipe in recipes)
				{
					EditorGUILayout.BeginHorizontal ();
					
					string buttonLabel = recipe.label;
					if (buttonLabel == "")
					{
						buttonLabel = "(Untitled)";	
					}
					
					if (GUILayout.Toggle (recipe.isEditing, recipe.id + ": " + buttonLabel, "Button"))
					{
						if (selectedRecipe != recipe)
						{
							DeactivateAllRecipes ();
							ActivateRecipe (recipe);
						}
					}
					
					if (GUILayout.Button (deleteContent, GUILayout.Width (20f), GUILayout.Height (15f)))
					{
						Undo.RecordObject (this, "Delete recipe");
						DeactivateAllRecipes ();
						recipes.Remove (recipe);
						AssetDatabase.SaveAssets();
						break;
					}
					
					EditorGUILayout.EndHorizontal ();
				}
				
				if (GUILayout.Button("Create new recipe"))
				{
					Undo.RecordObject (this, "Create inventory recipe");
					
					Recipe newRecipe = new Recipe (GetIDArrayRecipe ());
					recipes.Add (newRecipe);
					DeactivateAllRecipes ();
					ActivateRecipe (newRecipe);
				}
			}
			EditorGUILayout.EndVertical ();

			if (selectedRecipe != null && recipes.Contains (selectedRecipe))
			{
				string apiPrefix = "AC.KickStarter.inventoryManager.GetRecipe (" + selectedRecipe.id + ")";

				EditorGUILayout.Space ();
				EditorGUILayout.BeginVertical (CustomStyles.thinBox);
				showCraftingProperties = CustomGUILayout.ToggleHeader (showCraftingProperties, "Recipe '" + selectedRecipe.label + "' properties");
				if (showCraftingProperties)
				{
					selectedRecipe.label = CustomGUILayout.TextField ("Name:", selectedRecipe.label, apiPrefix + ".label");
					
					EditorGUILayout.BeginHorizontal ();
					EditorGUILayout.LabelField ("Resulting item:", GUILayout.Width (146f));
					int i = GetArraySlot (selectedRecipe.resultID);
					i = CustomGUILayout.Popup (i, GetLabelList (), apiPrefix + ".resultID");
					selectedRecipe.resultID = items[i].id;
					EditorGUILayout.EndHorizontal ();
					
					selectedRecipe.autoCreate = CustomGUILayout.Toggle ("Result is automatic?", selectedRecipe.autoCreate, apiPrefix + ".autoCreate");
					selectedRecipe.useSpecificSlots = CustomGUILayout.Toggle ("Requires specific pattern?", selectedRecipe.useSpecificSlots, apiPrefix + ".useSpecificSlots");
					selectedRecipe.actionListOnCreate = ActionListAssetMenu.AssetGUI ("ActionList when create:", selectedRecipe.actionListOnCreate, apiPrefix + ".actionListOnCreate", "ActionList_On_Create_" + selectedRecipe.label);

					selectedRecipe.onCreateRecipe = (OnCreateRecipe) CustomGUILayout.EnumPopup ("When click on result:", selectedRecipe.onCreateRecipe, apiPrefix + ".onCreateRecipe");
					if (selectedRecipe.onCreateRecipe == OnCreateRecipe.RunActionList)
					{
						selectedRecipe.invActionList = ActionListAssetMenu.AssetGUI ("ActionList when click:", selectedRecipe.invActionList, apiPrefix + ".invActionList", "ActionList_On_Click_" + selectedRecipe.label);
					}
					
					EditorGUILayout.Space ();
					EditorGUILayout.LabelField ("Ingredients",  CustomStyles.subHeader);
					
					foreach (Ingredient ingredient in selectedRecipe.ingredients)
					{
						int j = selectedRecipe.ingredients.IndexOf (ingredient);

						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField ("Ingredient:", GUILayout.Width (70f));
						i = GetArraySlot (ingredient.itemID);
						i = CustomGUILayout.Popup (i, GetLabelList (), apiPrefix + ".ingredients [" + j + "].itemID");
						ingredient.itemID = items[i].id;
						
						if (items[i].canCarryMultiple)
						{
							EditorGUILayout.LabelField ("Amount:", GUILayout.Width (50f));
							ingredient.amount = EditorGUILayout.IntField (ingredient.amount, GUILayout.Width (30f));
						}
						
						if (selectedRecipe.useSpecificSlots)
						{
							EditorGUILayout.LabelField ("Slot:", GUILayout.Width (30f));
							ingredient.slotNumber = EditorGUILayout.IntField (ingredient.slotNumber, GUILayout.Width (30f));
						}
						
						if (GUILayout.Button (deleteContent, GUILayout.Width (20f), GUILayout.Height (15f)))
						{
							Undo.RecordObject (this, "Delete ingredient");
							selectedRecipe.ingredients.Remove (ingredient);
							AssetDatabase.SaveAssets();
							break;
						}
						
						EditorGUILayout.EndHorizontal ();
					}
					
					if (GUILayout.Button("Add new ingredient"))
					{
						Undo.RecordObject (this, "Add recipe ingredient");
						
						Ingredient newIngredient = new Ingredient ();
						selectedRecipe.ingredients.Add (newIngredient);
					}
				}
				EditorGUILayout.EndVertical ();
			}
		}
		
		
		private List<int> GetIDList ()
		{
			List<int> idList = new List<int>();
			foreach (InvItem item in items)
			{
				idList.Add (item.id);
			}
			
			idList.Sort ();

			return idList;
		}
		
		
		private int[] GetIDArrayProperty ()
		{
			List<int> idArray = new List<int>();
			foreach (InvVar invVar in invVars)
			{
				idArray.Add (invVar.id);
			}
			
			idArray.Sort ();
			return idArray.ToArray ();
		}
		
		
		private int[] GetIDArrayRecipe ()
		{
			List<int> idArray = new List<int>();
			foreach (Recipe recipe in recipes)
			{
				idArray.Add (recipe.id);
			}
			
			idArray.Sort ();
			return idArray.ToArray ();
		}
		
		
		private int GetIconSlot (int _id)
		{
			int i = 0;
			foreach (CursorIcon icon in AdvGame.GetReferences ().cursorManager.cursorIcons)
			{
				if (icon.id == _id)
				{
					return i;
				}
				i++;
			}
			
			return 0;
		}
		
		
		private int GetArraySlot (int _id)
		{
			int i = 0;
			foreach (InvItem item in items)
			{
				if (item.id == _id)
				{
					return i;
				}
				i++;
			}
			
			return 0;
		}
		
		
		private string[] GetLabelList ()
		{
			List<string> labelList = new List<string>();
			foreach (InvItem _item in items)
			{
				labelList.Add (_item.label);
			}
			return labelList.ToArray ();
		}
		
		
		private int GetBinSlot (int _id)
		{
			int i = 0;
			foreach (InvBin bin in bins)
			{
				if (bin.id == _id)
				{
					return i;
				}
				i++;
			}
			
			return 0;
		}
		
		
		private int ChoosePlayerGUI (int playerID, string api)
		{
			List<string> labelList = new List<string>();
			int i = 0;
			int playerNumber = -1;
			
			if (AdvGame.GetReferences ().settingsManager.players.Count > 0)
			{
				foreach (PlayerPrefab playerPrefab in settingsManager.players)
				{
					if (playerPrefab.playerOb != null)
					{
						labelList.Add (playerPrefab.playerOb.name);
					}
					else
					{
						labelList.Add ("(Undefined prefab)");
					}
					
					// If a player has been removed, make sure selected player is still valid
					if (playerPrefab.ID == playerID)
					{
						playerNumber = i;
					}
					
					i++;
				}
				
				if (playerNumber == -1)
				{
					// Wasn't found (item was possibly deleted), so revert to zero
					ACDebug.LogWarning ("Previously chosen Player no longer exists!");
					
					playerNumber = 0;
					playerID = 0;
				}
				
				playerNumber = CustomGUILayout.Popup ("Item is carried by:", playerNumber, labelList.ToArray(), api);
				playerID = settingsManager.players[playerNumber].ID;
			}
			return playerID;
		}
		
		
		private void SetTab (int tab)
		{
			showItemsTab = (tab == 0) ? true : false;
			showBinsTab = (tab == 1) ? true : false;
			showCraftingTab = (tab == 2) ? true : false;
			showPropertiesTab = (tab == 3) ? true : false;
		}


		private void FindReferences (InvItem item)
		{
			if (item == null) return;

			if (EditorUtility.DisplayDialog ("Search '" + item.label + "' references?", "The Editor will search assets, and active scenes listed in the Build Settings, for references to the inventory item.  The current scene will need to be saved and listed to be included in the search process. Continue?", "OK", "Cancel"))
			{
				if (UnityVersionHandler.SaveSceneIfUserWants ())
				{
					int totalNumReferences = 0;
					int thisNumReferences = 0;

					// Search other items
					foreach (InvItem otherItem in items)
					{
						if (otherItem.id != item.id)
						{
							foreach (int combineID in otherItem.combineID)
							{
								if (combineID == item.id)
								{
									totalNumReferences ++;
									ACDebug.Log ("Found reference to inventory item '" + item.label + "' in inventory item '" + otherItem.label + "'s list of combine interactions.");
								}
							}
						}
					}

					// Search crafting
					foreach (Recipe recipe in recipes)
					{
						thisNumReferences = 0;
						if (recipe.resultID == item.id)
						{
							thisNumReferences ++;
						}
						foreach (Ingredient ingredient in recipe.ingredients)
						{
							if (ingredient.itemID == item.id)
							{
								thisNumReferences ++;
							}
						}
						if (thisNumReferences > 0)
						{
							ACDebug.Log ("Found " + thisNumReferences  + " reference to inventory item '" + item.label + "' in recipe '" + recipe.label + "'.");
						}
						totalNumReferences += thisNumReferences;
					}

					// Search scenes
					string originalScene = UnityVersionHandler.GetCurrentSceneFilepath ();
					string[] sceneFiles = AdvGame.GetSceneFiles ();
					foreach (string sceneFile in sceneFiles)
					{
						UnityVersionHandler.OpenScene (sceneFile);

						ActionList[] actionLists = FindObjectsOfType <ActionList>();
						foreach (ActionList actionList in actionLists)
						{
							foreach (Action action in actionList.actions)
							{
								thisNumReferences = action.GetInventoryReferences (actionList.parameters, item.id);
								if (thisNumReferences > 0)
								{
									totalNumReferences += thisNumReferences;
									ACDebug.Log ("Found " + thisNumReferences + " references to inventory item '" + item.label + "' in Action #" + actionList.actions.IndexOf (action) + " of ActionList '" + actionList + "' in scene '" + sceneFile + "'", actionList);
								}
							}
						}

						Container [] containers = FindObjectsOfType <Container>();
						foreach (Container container in containers)
						{
							thisNumReferences = container.GetInventoryReferences (item.id);
							if (thisNumReferences > 0)
							{
								totalNumReferences += thisNumReferences;
								ACDebug.Log ("Found " + thisNumReferences + " references to inventory item '" + item.label + "' in Container '" + container.name + "' in scene '" + sceneFile + "'", container);
							}
						}

						Conversation[] conversations = FindObjectsOfType <Conversation>();
						foreach (Conversation conversation in conversations)
						{
							thisNumReferences = conversation.GetInventoryReferences (item.id);
							if (thisNumReferences > 0)
							{
								totalNumReferences += thisNumReferences;
								ACDebug.Log ("Found " + thisNumReferences + " references to inventory item '" + item.label + "' in Conversation '" + conversation.name + "' in scene '" + sceneFile + "'", conversation);
							}
						}

						Hotspot[] hotspots = FindObjectsOfType <Hotspot>();
						foreach (Hotspot hotspot in hotspots)
						{
							thisNumReferences = hotspot.GetInventoryReferences (item.id);
							if (thisNumReferences > 0)
							{
								totalNumReferences += thisNumReferences;
								ACDebug.Log ("Found " + thisNumReferences + " references to inventory item '" + item.label + "' in Hotspot '" + hotspot.name + "'", hotspot);
							}
						}
					}

					UnityVersionHandler.OpenScene (originalScene);

					// Search assets
					if (AdvGame.GetReferences ().speechManager != null)
					{
						ActionListAsset[] allActionListAssets = AdvGame.GetReferences ().speechManager.GetAllActionListAssets ();
						foreach (ActionListAsset actionListAsset in allActionListAssets)
						{
							foreach (Action action in actionListAsset.actions)
							{
								thisNumReferences = action.GetInventoryReferences (actionListAsset.parameters, item.id);
								if (thisNumReferences > 0)
								{
									totalNumReferences += thisNumReferences;
									ACDebug.Log ("Found " + thisNumReferences + " references to inventory item '" + item.label + "' in Action #" + actionListAsset.actions.IndexOf (action) + " of ActionList asset '" + actionListAsset.name + "'", actionListAsset);
								}
							}
						}
					}

					EditorUtility.DisplayDialog ("Inventory search complete", "In total, found " + totalNumReferences + " references to inventory item '" + item.label + "' in the project.  Please see the Console window for full details.", "OK");
				}
			}
		}
		
		#endif
		
		
		/**
		 * <summary>Gets an inventory item's label.</summary>
		 * <param name = "_id">The ID number of the InvItem to find</param>
		 * <returns>The inventory item's label</returns>
		 */
		public string GetLabel (int _id)
		{
			string result = "";
			foreach (InvItem item in items)
			{
				if (item.id == _id)
				{
					result = item.label;
				}
			}
			
			return result;
		}
		
		
		/**
		 * <summary>Gets an inventory item.</summary>
		 * <param name = "_id">The ID number of the InvItem to find</param>
		 * <returns>The inventory item</returns>
		 */
		public InvItem GetItem (int _id)
		{
			foreach (InvItem item in items)
			{
				if (item.id == _id)
				{
					return item;
				}
			}
			return null;
		}


		/**
		 * <summary>Gets a Recipe.</summary>
		 * <param name = "_id">The ID number of the Recipe to find</param>
		 * <returns>The Recipe</returns>
		 */
		public Recipe GetRecipe (int _id)
		{
			foreach (Recipe recipe in recipes)
			{
				if (recipe.id == _id)
				{
					return recipe;
				}
			}
			return null;
		}


		/**
		 * <summary>Gets an inventory category.</summary>
		 * <param name = "categoryID">The ID number of the inventory category to find</param>
		 * <returns>The inventory category</returns>
		 */
		public InvBin GetCategory (int categoryID)
		{
			foreach (InvBin bin in bins)
			{
				if (bin.id == categoryID)
				{
					return bin;
				}
			}
			return null;
		}
		
		
		/**
		 * <summary>Checks if multiple instances of an inventory item can exist.</summary>
		 * <param name = "itemID">The ID number of the InvItem to find</param>
		 * <returns>True if multiple instances of the inventory item can exist</returns>
		 */
		public bool CanCarryMultiple (int itemID)
		{
			foreach (InvItem item in items)
			{
				if (item.id == itemID)
				{
					return item.canCarryMultiple;
				}
			}
			
			return false;
		}


		/**
		 * <summary>Gets an array of all inventory items in a given category</summary>
		 * <param name = "categoryID">The ID number of the category in question</param>
		 * <returns>An array of all inventory items in the category</returns>
		 */
		public InvItem[] GetItemsInCategory (int categoryID)
		{
			List<InvItem> itemsList = new List<InvItem>();
			foreach (InvItem item in items)
			{
				if (item.binID == categoryID)
				{
					itemsList.Add (item);
				}
			}

			return itemsList.ToArray ();
		}
		
	}
	
}