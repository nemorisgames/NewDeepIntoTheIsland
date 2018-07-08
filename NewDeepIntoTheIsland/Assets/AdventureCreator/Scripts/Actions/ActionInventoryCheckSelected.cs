/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionInventoryCheckSelected.cs"
 * 
 *	This action is used to check the currently-selected item.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	[System.Serializable]
	public class ActionInventoryCheckSelected : ActionCheck
	{
		
		public int parameterID = -1;
		public int invID;
		public int binID;
		public bool checkNothing = false; // Deprecated
		public bool includeLast = false;

		[SerializeField] private SelectedCheckMethod selectedCheckMethod = SelectedCheckMethod.SpecificItem;
		[SerializeField] private enum SelectedCheckMethod { SpecificItem, InSpecificCategory, NoneSelected };

		#if UNITY_EDITOR
		private InventoryManager inventoryManager;
		#endif


		public ActionInventoryCheckSelected ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Inventory;
			title = "Check selected";
			description = "Queries whether or not the chosen item, or no item, is currently selected.";
		}

		
		override public void AssignValues (List<ActionParameter> parameters)
		{
			invID = AssignInvItemID (parameters, parameterID, invID);

			Upgrade ();
		}
		
		
		override public bool CheckCondition ()
		{
			if (KickStarter.runtimeInventory)
			{
				switch (selectedCheckMethod)
				{
					case SelectedCheckMethod.NoneSelected:
						if (KickStarter.runtimeInventory.SelectedItem == null)
						{
							return true;
						}
						break;

					case SelectedCheckMethod.SpecificItem:
						if (includeLast)
						{
							if (KickStarter.runtimeInventory.LastSelectedItem != null && KickStarter.runtimeInventory.LastSelectedItem.id == invID)
							{
								return true;
							}
						}
						else
						{
							if (KickStarter.runtimeInventory.SelectedItem != null && KickStarter.runtimeInventory.SelectedItem.id == invID)
							{
								return true;
							}
						}
						break;

					case SelectedCheckMethod.InSpecificCategory:
						if (includeLast)
						{
							if (KickStarter.runtimeInventory.LastSelectedItem != null && KickStarter.runtimeInventory.LastSelectedItem.binID == binID)
							{
								return true;
							}
						}
						else
						{
							if (KickStarter.runtimeInventory.SelectedItem != null && KickStarter.runtimeInventory.SelectedItem.binID == binID)
							{
								return true;
							}
						}
						break;
				}
			}
			return false;
		}


		private void Upgrade ()
		{
			if (checkNothing)
			{
				selectedCheckMethod = SelectedCheckMethod.NoneSelected;
				checkNothing = false;
			}
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			Upgrade ();

			if (inventoryManager == null)
			{
				inventoryManager = AdvGame.GetReferences ().inventoryManager;
			}

			selectedCheckMethod = (SelectedCheckMethod) EditorGUILayout.EnumPopup ("Check selected item is:", selectedCheckMethod);

			if (inventoryManager != null)
			{
				if (selectedCheckMethod == SelectedCheckMethod.InSpecificCategory)
				{
					// Create a string List of the field's names (for the PopUp box)
					List<string> labelList = new List<string>();
					
					int i = 0;
					int binNumber = 0;
					if (parameterID == -1)
					{
						binNumber = -1;
					}
					
					if (inventoryManager.bins != null && inventoryManager.bins.Count > 0)
					{
						foreach (InvBin _bin in inventoryManager.bins)
						{
							labelList.Add (_bin.label);
							
							// If a category has been removed, make sure selected is still valid
							if (_bin.id == binID)
							{
								binNumber = i;
							}
							
							i++;
						}
						
						if (binNumber == -1)
						{
							ACDebug.LogWarning ("Previously chosen category no longer exists!");
							binID = 0;
						}
						
						binNumber = EditorGUILayout.Popup ("Inventory category:", binNumber, labelList.ToArray());
						binID = inventoryManager.items[binNumber].id;

						includeLast = EditorGUILayout.Toggle ("Include last-selected?", includeLast);
					}
					else
					{
						EditorGUILayout.HelpBox ("No inventory categories exist!", MessageType.Info);
						binID = -1;
					}
				}
				else if (selectedCheckMethod == SelectedCheckMethod.SpecificItem)
				{
					// Create a string List of the field's names (for the PopUp box)
					List<string> labelList = new List<string>();
					
					int i = 0;
					int invNumber = 0;
					if (parameterID == -1)
					{
						invNumber = -1;
					}
					
					if (inventoryManager.items.Count > 0)
					{
						foreach (InvItem _item in inventoryManager.items)
						{
							labelList.Add (_item.label);
							
							// If an item has been removed, make sure selected variable is still valid
							if (_item.id == invID)
							{
								invNumber = i;
							}
							
							i++;
						}
						
						if (invNumber == -1)
						{
							ACDebug.LogWarning ("Previously chosen item no longer exists!");
							invID = 0;
						}
						
						parameterID = Action.ChooseParameterGUI ("Inventory item:", parameters, parameterID, ParameterType.InventoryItem);
						if (parameterID >= 0)
						{
							invNumber = Mathf.Min (invNumber, inventoryManager.items.Count-1);
							invID = -1;
						}
						else
						{
							invNumber = EditorGUILayout.Popup ("Inventory item:", invNumber, labelList.ToArray());
							invID = inventoryManager.items[invNumber].id;
						}

						includeLast = EditorGUILayout.Toggle ("Include last-selected?", includeLast);
					}
					else
					{
						EditorGUILayout.HelpBox ("No inventory items exist!", MessageType.Info);
						invID = -1;
					}
				}
			}

		}
		
		
		override public string SetLabel ()
		{
			switch (selectedCheckMethod)
			{
				case SelectedCheckMethod.NoneSelected:
					return (" (Nothing)");

				case SelectedCheckMethod.SpecificItem:
					if (inventoryManager)
					{
						return (" (" + inventoryManager.GetLabel (invID) + ")");
					}
					break;

				case SelectedCheckMethod.InSpecificCategory:
					if (inventoryManager)
					{
						InvBin category = inventoryManager.GetCategory (binID);
						if (category != null)
						{
							return (" (" + category.label + ")");
						}
					}
					break;
			}
			return "";
		}


		public override int GetInventoryReferences (List<ActionParameter> parameters, int _invID)
		{
			if (selectedCheckMethod == SelectedCheckMethod.SpecificItem && invID == _invID)
			{
				return 1;
			}
			return 0;
		}
		
		#endif
		
	}
	
}