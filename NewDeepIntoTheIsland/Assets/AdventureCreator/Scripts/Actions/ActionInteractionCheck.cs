/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionInteraction.cs"
 * 
 *	This Action can enable and disable
 *	a Hotspot's individual Interactions.
 * 
 */

using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	[System.Serializable]
	public class ActionInteractionCheck : ActionCheck
	{
		
		public int parameterID = -1;
		public int constantID = 0;
		public Hotspot hotspot;
		
		public InteractionType interactionType;
		public int number = 0;
		
		
		public ActionInteractionCheck ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Hotspot;
			title = "Check interaction enabled";
			description = "Checks the enabled state of individual Interactions on a Hotspot.";
		}
		
		
		override public void AssignValues (List<ActionParameter> parameters)
		{
			hotspot = AssignFile <Hotspot> (parameters, parameterID, constantID, hotspot);
		}
		
		
		override public bool CheckCondition ()
		{
			if (hotspot == null)
			{
				return false;
			}
			
			if (interactionType == InteractionType.Use)
			{
				if (hotspot.useButtons.Count > number)
				{
					return !hotspot.useButtons [number].isDisabled;
				}
				else
				{
					ACDebug.LogWarning ("Cannot check Hotspot " + hotspot.gameObject.name + "'s Use button " + number.ToString () + " because it doesn't exist!");
				}
			}
			else if (interactionType == InteractionType.Examine)
			{
				return !hotspot.lookButton.isDisabled;
			}
			else if (interactionType == InteractionType.Inventory)
			{
				if (hotspot.invButtons.Count > number)
				{
					return !hotspot.invButtons [number].isDisabled;
				}
				else
				{
					ACDebug.LogWarning ("Cannot check Hotspot " + hotspot.gameObject.name + "'s Inventory button " + number.ToString () + " because it doesn't exist!");
				}
			}
			
			return false;
		}

		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			if (AdvGame.GetReferences () && AdvGame.GetReferences ().settingsManager)
			{
				parameterID = Action.ChooseParameterGUI ("Hotspot to checl:", parameters, parameterID, ParameterType.GameObject);
				if (parameterID >= 0)
				{
					constantID = 0;
					hotspot = null;
				}
				else
				{
					hotspot = (Hotspot) EditorGUILayout.ObjectField ("Hotspot to check:", hotspot, typeof (Hotspot), true);
					
					constantID = FieldToID <Hotspot> (hotspot, constantID);
					hotspot = IDToField <Hotspot> (hotspot, constantID, false);
				}
				
				interactionType = (InteractionType) EditorGUILayout.EnumPopup ("Interaction to check:", interactionType);
				
				if ((!isAssetFile && hotspot != null) || isAssetFile)
				{
					if (interactionType == InteractionType.Use)
					{
						if (isAssetFile)
						{
							number = EditorGUILayout.IntField ("Use interaction:", number);
						}
						else if (AdvGame.GetReferences ().cursorManager)
						{
							// Multiple use interactions
							List<string> labelList = new List<string>();
							
							foreach (AC.Button button in hotspot.useButtons)
							{
								labelList.Add (hotspot.useButtons.IndexOf (button) + ": " + AdvGame.GetReferences ().cursorManager.GetLabelFromID (button.iconID, 0));
							}
							
							number = EditorGUILayout.Popup ("Use interaction:", number, labelList.ToArray ());
						}
						else
						{
							EditorGUILayout.HelpBox ("A Cursor Manager is required.", MessageType.Warning);
						}
					}
					else if (interactionType == InteractionType.Inventory)
					{
						if (isAssetFile)
						{
							number = EditorGUILayout.IntField ("Inventory interaction:", number);
						}
						else if (AdvGame.GetReferences ().inventoryManager)
						{
							List<string> labelList = new List<string>();
							
							foreach (AC.Button button in hotspot.invButtons)
							{
								labelList.Add (hotspot.invButtons.IndexOf (button) + ": " + AdvGame.GetReferences ().inventoryManager.GetLabel (button.invID));
							}
							
							number = EditorGUILayout.Popup ("Inventory interaction:", number, labelList.ToArray ());
						}
						else
						{
							EditorGUILayout.HelpBox ("An Inventory Manager is required.", MessageType.Warning);
						}
					}
				}
			}
			else
			{
				EditorGUILayout.HelpBox ("A Settings Manager is required for this Action.", MessageType.Warning);
			}
		}


		override public void AssignConstantIDs (bool saveScriptsToo)
		{
			AssignConstantID <Hotspot> (hotspot, constantID, parameterID);
		}

		
		public override string SetLabel ()
		{
			string labelAdd = "";
			if (hotspot != null)
			{
				labelAdd = " (" + hotspot.name + " - " + interactionType;
				labelAdd += ")";
			}
			return labelAdd;
		}
		
		#endif
		
	}
	
}