/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionManageSaves.cs"
 * 
 *	This Action renames and deletes save game files
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
	public class ActionManageSaves : Action
	{
		
		public ManageSaveType manageSaveType = ManageSaveType.DeleteSave;
		public SelectSaveType selectSaveType = SelectSaveType.SetSlotIndex;

		public int saveIndex = 0;
		public int saveIndexParameterID = -1;

		public int varID;
		public int slotVarID;
		
		public string menuName = "";
		public string elementName = "";
		
		
		public ActionManageSaves ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Save;
			title = "Manage saves";
			description = "Renames and deletes save game files.";
		}
		
		
		override public void AssignValues (List<ActionParameter> parameters)
		{
			saveIndex = AssignInteger (parameters, saveIndexParameterID, saveIndex);
		}
		
		
		override public float Run ()
		{
			string newSaveLabel = "";
			if (manageSaveType == ManageSaveType.RenameSave)
			{
				GVar gVar = GlobalVariables.GetVariable (varID);
				if (gVar != null)
				{
					newSaveLabel = gVar.textVal;
				}
				else
				{
					ACDebug.LogWarning ("Could not " + manageSaveType.ToString () + " - no variable found.");
					return 0f;
				}
			}

			int i = Mathf.Max (0, saveIndex);
			
			if (selectSaveType == SelectSaveType.SlotIndexFromVariable)
			{
				GVar gVar = GlobalVariables.GetVariable (slotVarID);
				if (gVar != null)
				{
					i = gVar.val;
				}
				else
				{
					ACDebug.LogWarning ("Could not rename save - no variable found.");
					return 0f;
				}
			}
			
			if (menuName != "" && elementName != "")
			{
				MenuElement menuElement = PlayerMenus.GetElementWithName (menuName, elementName);
				if (menuElement != null && menuElement is MenuSavesList)
				{
					MenuSavesList menuSavesList = (MenuSavesList) menuElement;
					i += menuSavesList.GetOffset ();
				}
				else
				{
					ACDebug.LogWarning ("Cannot find SavesList element '" + elementName + "' in Menu '" + menuName + "'.");
				}
			}
			else
			{
				ACDebug.LogWarning ("No SavesList element referenced when trying to find save slot " + i.ToString ());
			}
			
			if (manageSaveType == ManageSaveType.DeleteSave)
			{
				KickStarter.saveSystem.DeleteSave (i, -1, false);
			}
			else if (manageSaveType == ManageSaveType.RenameSave)
			{
				KickStarter.saveSystem.RenameSave (newSaveLabel, i);
			}
			
			return 0f;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			manageSaveType = (ManageSaveType) EditorGUILayout.EnumPopup ("Method:", manageSaveType);
			
			if (manageSaveType == ManageSaveType.RenameSave)
			{
				varID = AdvGame.GlobalVariableGUI ("Label as String variable:", varID, VariableType.String);
			}

			string _action = "delete";
			if (manageSaveType == ManageSaveType.RenameSave)
			{
				_action = "rename";
			}
			
			selectSaveType = (SelectSaveType) EditorGUILayout.EnumPopup ("Save to " + _action + ":", selectSaveType);
			if (selectSaveType == SelectSaveType.SetSlotIndex)
			{
				saveIndexParameterID = Action.ChooseParameterGUI ("Slot index to " + _action + ":", parameters, saveIndexParameterID, ParameterType.Integer);
				if (saveIndexParameterID == -1)
				{
					saveIndex = EditorGUILayout.IntField ("Slot index to " + _action + ":", saveIndex);
				}
			}
			else if (selectSaveType == SelectSaveType.SlotIndexFromVariable)
			{
				slotVarID = AdvGame.GlobalVariableGUI ("Integer variable:", slotVarID, VariableType.Integer);
			}

			EditorGUILayout.Space ();
			menuName = EditorGUILayout.TextField ("Menu with SavesList:", menuName);
			elementName = EditorGUILayout.TextField ("SavesList element:", elementName);

			AfterRunningOption ();
		}
		
		
		public override string SetLabel ()
		{
			return (" (" + manageSaveType.ToString () + ")");
		}
		
		#endif
		
	}
	
}