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
	public class ActionInteraction : Action
	{

		public int parameterID = -1;
		public int constantID = 0;
		public Hotspot hotspot;

		public InteractionType interactionType;
		public ChangeType changeType = ChangeType.Enable;
		public int number = 0;

		
		public ActionInteraction ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Hotspot;
			title = "Change interaction";
			description = "Enables and disables individual Interactions on a Hotspot.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			hotspot = AssignFile <Hotspot> (parameters, parameterID, constantID, hotspot);
		}

		
		override public float Run ()
		{
			if (hotspot == null)
			{
				return 0f;
			}

			if (interactionType == InteractionType.Use)
			{
				if (hotspot.useButtons.Count > number)
				{
					ChangeButton (hotspot.useButtons [number]);
				}
				else
				{
					ACDebug.LogWarning ("Cannot change Hotspot " + hotspot.gameObject.name + "'s Use button " + number.ToString () + " because it doesn't exist!");
				}
			}
			else if (interactionType == InteractionType.Examine)
			{
				ChangeButton (hotspot.lookButton);
			}
			else if (interactionType == InteractionType.Inventory)
			{
				if (hotspot.invButtons.Count > number)
				{
					ChangeButton (hotspot.invButtons [number]);
				}
				else
				{
					ACDebug.LogWarning ("Cannot change Hotspot " + hotspot.gameObject.name + "'s Inventory button " + number.ToString () + " because it doesn't exist!");
				}
			}
			hotspot.ResetMainIcon ();

			return 0f;
		}


		private void ChangeButton (AC.Button button)
		{
			if (button == null)
			{
				return;
			}

			if (changeType == ChangeType.Enable)
			{
				button.isDisabled = false;
			}
			else if (changeType == ChangeType.Disable)
			{
				button.isDisabled = true;
			}
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			if (AdvGame.GetReferences () && AdvGame.GetReferences ().settingsManager)
			{
				parameterID = Action.ChooseParameterGUI ("Hotspot to change:", parameters, parameterID, ParameterType.GameObject);
				if (parameterID >= 0)
				{
					constantID = 0;
					hotspot = null;
				}
				else
				{
					hotspot = (Hotspot) EditorGUILayout.ObjectField ("Hotspot to change:", hotspot, typeof (Hotspot), true);
					
					constantID = FieldToID <Hotspot> (hotspot, constantID);
					hotspot = IDToField <Hotspot> (hotspot, constantID, false);
				}

				interactionType = (InteractionType) EditorGUILayout.EnumPopup ("Interaction to change:", interactionType);

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

				changeType = (ChangeType) EditorGUILayout.EnumPopup ("Change to make:", changeType);
			}
			else
			{
				EditorGUILayout.HelpBox ("A Settings Manager is required for this Action.", MessageType.Warning);
			}

			AfterRunningOption ();
		}


		override public void AssignConstantIDs (bool saveScriptsToo)
		{
			if (saveScriptsToo)
			{
				AddSaveScript <RememberHotspot> (hotspot);
			}

			AssignConstantID <Hotspot> (hotspot, constantID, parameterID);
		}
		
		
		public override string SetLabel ()
		{
			string labelAdd = "";
			if (hotspot != null)
			{
				labelAdd = " (" + hotspot.name + " - " + changeType + " " + interactionType;
				labelAdd += ")";
			}
			return labelAdd;
		}
		
		#endif
		
	}

}