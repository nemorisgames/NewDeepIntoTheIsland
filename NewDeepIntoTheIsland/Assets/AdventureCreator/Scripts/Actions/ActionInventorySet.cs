/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionInventorySet.cs"
 * 
 *	This action is used to add or remove items from the player's inventory, defined in the Inventory Manager.
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
	public class ActionInventorySet : Action
	{
		
		public InvAction invAction;

		public int parameterID = -1;
		public int invID;
		public int replaceParameterID = -1;
		public int invIDReplace;
		private int invNumber;
		private int replaceInvNumber;
		
		public bool setAmount = false;
		public int amountParameterID = -1;
		public int amount = 1;

		public bool setPlayer = false;
		public int playerID;

		public bool addToFront = false;

		#if UNITY_EDITOR
		private InventoryManager inventoryManager;
		private SettingsManager settingsManager;
		#endif


		public ActionInventorySet ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Inventory;
			title = "Add or remove";
			description = "Adds or removes an item from the Player's inventory. Items are defined in the Inventory Manager. If the player can carry multiple amounts of the item, more options will show.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			invID = AssignInvItemID (parameters, parameterID, invID);
			invIDReplace = AssignInvItemID (parameters, replaceParameterID, invIDReplace);
			amount = AssignInteger (parameters, amountParameterID, amount);
		}
		
		
		override public float Run ()
		{
			if (KickStarter.runtimeInventory)
			{
				if (!setAmount)
				{
					amount = 1;
				}

				int _playerID = -1;

				if (KickStarter.settingsManager.playerSwitching == PlayerSwitching.Allow && !KickStarter.settingsManager.shareInventory && setPlayer)
				{
					_playerID = playerID;
				}

				if (invAction == InvAction.Add)
				{
					KickStarter.runtimeInventory.Add (invID, amount, false, _playerID, addToFront);
				}
				else if (invAction == InvAction.Remove)
				{
					if (KickStarter.runtimeInventory.SelectedItem != null && KickStarter.runtimeInventory.SelectedItem.id == invID)
					{
						KickStarter.runtimeInventory.SetNull ();
					}

					KickStarter.runtimeInventory.Remove (invID, amount, setAmount, _playerID);
				}
				else if (invAction == InvAction.Replace)
				{
					if (KickStarter.runtimeInventory.SelectedItem != null && KickStarter.runtimeInventory.SelectedItem.id == invIDReplace)
					{
						KickStarter.runtimeInventory.SetNull ();
					}

					KickStarter.runtimeInventory.Replace (invID, invIDReplace, amount);
				}
			}
			
			return 0f;
		}

		
		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			if (inventoryManager == null && AdvGame.GetReferences ().inventoryManager)
			{
				inventoryManager = AdvGame.GetReferences ().inventoryManager;
			}
			if (settingsManager == null && AdvGame.GetReferences ().settingsManager)
			{
				settingsManager = AdvGame.GetReferences ().settingsManager;
			}
			
			if (inventoryManager != null)
			{
				// Create a string List of the field's names (for the PopUp box)
				List<string> labelList = new List<string>();
				
				int i = 0;
				if (parameterID == -1)
				{
					invNumber = -1;
				}
				
				if (inventoryManager.items.Count > 0)
				{
					foreach (InvItem _item in inventoryManager.items)
					{
						labelList.Add (_item.label);
						
						// If a item has been removed, make sure selected variable is still valid
						if (_item.id == invID)
						{
							invNumber = i;
						}
						if (_item.id == invIDReplace)
						{
							replaceInvNumber = i;
						}
						
						i++;
					}
					
					if (invNumber == -1)
					{
						ACDebug.Log ("Previously chosen item no longer exists!");
						invNumber = 0;
						invID = 0;
					}

					if (invAction == InvAction.Replace && replaceInvNumber == -1)
					{
						ACDebug.Log ("Previously chosen item no longer exists!");
						replaceInvNumber = 0;
						invIDReplace = 0;
					}

					invAction = (InvAction) EditorGUILayout.EnumPopup ("Method:", invAction);

					string label = "Item to add:";
					if (invAction == InvAction.Remove)
					{
						label = "Item to remove:";
					}

					parameterID = Action.ChooseParameterGUI (label, parameters, parameterID, ParameterType.InventoryItem);
					if (parameterID >= 0)
					{
						invNumber = Mathf.Min (invNumber, inventoryManager.items.Count-1);
						invID = -1;
					}
					else
					{
						invNumber = EditorGUILayout.Popup (label, invNumber, labelList.ToArray());
						invID = inventoryManager.items[invNumber].id;
					}

					if (inventoryManager.items[invNumber].canCarryMultiple)
					{
						setAmount = EditorGUILayout.Toggle ("Set amount?", setAmount);
					
						if (setAmount)
						{
							string _label = (invAction == InvAction.Remove) ? "Reduce count by:" : "Increase count by:";

							amountParameterID = Action.ChooseParameterGUI (_label, parameters, amountParameterID, ParameterType.Integer);
							if (parameterID < 0)
							{
								amount = EditorGUILayout.IntField (_label, amount);
							}
						}
					}

					if (invAction == InvAction.Replace)
					{
						replaceParameterID = Action.ChooseParameterGUI ("Item to remove:", parameters, replaceParameterID, ParameterType.InventoryItem);
						if (replaceParameterID >= 0)
						{
							replaceInvNumber = Mathf.Min (replaceInvNumber, inventoryManager.items.Count-1);
							invIDReplace = -1;
						}
						else
						{
							replaceInvNumber = EditorGUILayout.Popup ("Item to remove:", replaceInvNumber, labelList.ToArray());
							invIDReplace = inventoryManager.items[replaceInvNumber].id;
						}
					}
					else if (invAction == InvAction.Add)
					{
						addToFront = EditorGUILayout.Toggle ("Add to front?", addToFront);
					}
				}
				else
				{
					EditorGUILayout.HelpBox ("No inventory items exist!", MessageType.Info);
					invID = -1;
					invNumber = -1;
					invIDReplace = -1;
					replaceInvNumber = -1;
				}

				if (settingsManager != null && settingsManager.playerSwitching == PlayerSwitching.Allow && !settingsManager.shareInventory && invAction != InvAction.Replace)
				{
					EditorGUILayout.Space ();

					setPlayer = EditorGUILayout.Toggle ("Affect specific player?", setPlayer);
					if (setPlayer)
					{
						ChoosePlayerGUI ();
					}
				}
				else
				{
					setPlayer = false;
				}
			}
			else
			{
				EditorGUILayout.HelpBox ("An Inventory Manager must be assigned for this Action to work", MessageType.Warning);
			}

			AfterRunningOption ();
		}
		
		
		override public string SetLabel ()
		{
			string labelAdd = "";
			string labelItem = "";

			if (!inventoryManager)
			{
				inventoryManager = AdvGame.GetReferences ().inventoryManager;
			}

			if (inventoryManager)
			{
				labelItem = " " + inventoryManager.GetLabel (invID);
			}
			
			if (invAction == InvAction.Remove)
			{
				labelAdd = " (Remove" + labelItem + ")";
			}
			else
			{
				labelAdd = " (Add" + labelItem + ")";
			}
		
			return labelAdd;
		}


		private void ChoosePlayerGUI ()
		{
			List<string> labelList = new List<string>();
			int i = 0;
			int playerNumber = -1;

			if (settingsManager.players.Count > 0)
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

				string label = "Add to player:";
				if (invAction == InvAction.Remove)
				{
					label = "Remove from player:";
				}

				playerNumber = EditorGUILayout.Popup (label, playerNumber, labelList.ToArray());
				playerID = settingsManager.players[playerNumber].ID;
			}
		}


		public override int GetInventoryReferences (List<ActionParameter> parameters, int _invID)
		{
			if (invID == _invID)
			{
				return 1;
			}
			return 0;
		}

		#endif

	}

}