/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionSaveCheck.cs"
 * 
 *	This Action creates and deletes save game profiles
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
	public class ActionSaveCheck : ActionCheck
	{

		public SaveCheck saveCheck = SaveCheck.NumberOfSaveGames;
		public bool includeAutoSaves = true;
		public bool checkByElementIndex = false;
		public bool checkByName = false;

		public int intValue;
		public int checkParameterID = -1;
		public IntCondition intCondition;

		public string menuName = "";
		public string elementName = "";

		public int profileVarID;


		public ActionSaveCheck ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Save;
			title = "Check";
			description = "Queries the number of save files or profiles created, or if saving is possible.";
		}
		
		
		override public void AssignValues (List<ActionParameter> parameters)
		{
			intValue = AssignInteger (parameters, checkParameterID, intValue);
		}
		
		
		override public ActionEnd End (List<AC.Action> actions)
		{
			int actualNumber = 0;

			if (saveCheck == SaveCheck.NumberOfSaveGames)
			{
				actualNumber = KickStarter.saveSystem.GetNumSaves (includeAutoSaves);
			}
			else if (saveCheck == SaveCheck.NumberOfProfiles)
			{
				actualNumber = KickStarter.options.GetNumProfiles ();
			}
			else if (saveCheck == SaveCheck.IsSlotEmpty)
			{
				return ProcessResult (!SaveSystem.DoesSaveExist (intValue, intValue, !checkByElementIndex), actions);
			}
			else if (saveCheck == SaveCheck.DoesProfileExist)
			{
				if (checkByElementIndex)
				{
					int i = Mathf.Max (0, intValue);
					bool includeActive = true;
					if (menuName != "" && elementName != "")
					{
						MenuElement menuElement = PlayerMenus.GetElementWithName (menuName, elementName);
						if (menuElement != null && menuElement is MenuProfilesList)
						{
							MenuProfilesList menuProfilesList = (MenuProfilesList) menuElement;

							if (menuProfilesList.fixedOption)
							{
								ACDebug.LogWarning ("Cannot refer to ProfilesLst " + elementName + " in Menu " + menuName + ", as it lists a fixed profile ID only!");
								return ProcessResult (false, actions);
							}

							i += menuProfilesList.GetOffset ();
							includeActive = menuProfilesList.showActive;
						}
						else
						{
							ACDebug.LogWarning ("Cannot find ProfilesList element '" + elementName + "' in Menu '" + menuName + "'.");
						}
					}
					else
					{
						ACDebug.LogWarning ("No ProfilesList element referenced when trying to delete profile slot " + i.ToString ());
					}

					bool result = KickStarter.options.DoesProfileExist (i, includeActive);
					return ProcessResult (result, actions);
				}
				else
				{
					// intValue is the profile ID
					bool result = Options.DoesProfileIDExist (intValue);
					return ProcessResult (result, actions);
				}
			}
			else if (saveCheck == SaveCheck.DoesProfileNameExist)
			{
				bool result = false;

				GVar gVar = GlobalVariables.GetVariable (profileVarID);
				if (gVar != null)
				{
					string profileName = gVar.textVal;
					result = KickStarter.options.DoesProfileExist (profileName);
				}
				else
				{
					ACDebug.LogWarning ("Could not check for profile name - no variable found.");
				}

				return ProcessResult (result, actions);
			}
			else if (saveCheck == SaveCheck.IsSavingPossible)
			{
				return ProcessResult (!PlayerMenus.IsSavingLocked (this), actions);
			}

			return ProcessResult (CheckCondition (actualNumber), actions);
		}
		
		
		private bool CheckCondition (int fieldValue)
		{
			if (intCondition == IntCondition.EqualTo)
			{
				if (fieldValue == intValue)
				{
					return true;
				}
			}
			else if (intCondition == IntCondition.NotEqualTo)
			{
				if (fieldValue != intValue)
				{
					return true;
				}
			}
			else if (intCondition == IntCondition.LessThan)
			{
				if (fieldValue < intValue)
				{
					return true;
				}
			}
			else if (intCondition == IntCondition.MoreThan)
			{
				if (fieldValue > intValue)
				{
					return true;
				}
			}

			return false;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			saveCheck = (SaveCheck) EditorGUILayout.EnumPopup ("Check to make:", saveCheck);
			if (saveCheck == SaveCheck.NumberOfSaveGames)
			{
				includeAutoSaves = EditorGUILayout.Toggle ("Include auto-save?", includeAutoSaves);
			}

			if (saveCheck == SaveCheck.IsSlotEmpty)
			{
				checkByElementIndex = EditorGUILayout.Toggle ("Check by SavesList slot index?", checkByElementIndex);

				string intValueLabel = (checkByElementIndex) ? "SavesList slot index:" : "Save ID:";
				checkParameterID = Action.ChooseParameterGUI (intValueLabel, parameters, checkParameterID, ParameterType.Integer);
				if (checkParameterID < 0)
				{
					intValue = EditorGUILayout.IntField (intValueLabel, intValue);
				}
			}
			else if (saveCheck == SaveCheck.DoesProfileExist)
			{
				checkByElementIndex = EditorGUILayout.ToggleLeft ("Check by ProfilesList slot index?", checkByElementIndex);

				string intValueLabel = (checkByElementIndex) ? "ProfilesList slot index:" : "Profile ID:";
				checkParameterID = Action.ChooseParameterGUI (intValueLabel, parameters, checkParameterID, ParameterType.Integer);
				if (checkParameterID < 0)
				{
					intValue = EditorGUILayout.IntField (intValueLabel, intValue);
				}

				if (checkByElementIndex)
				{
					EditorGUILayout.Space ();
					menuName = EditorGUILayout.TextField ("Menu with ProfilesList:", menuName);
					elementName = EditorGUILayout.TextField ("ProfilesList element:", elementName);
				}
			}
			else if (saveCheck == SaveCheck.DoesProfileNameExist)
			{
				profileVarID = AdvGame.GlobalVariableGUI ("String variable with name:", profileVarID, VariableType.String);
			}
			else if (saveCheck != SaveCheck.IsSavingPossible)
			{
				intCondition = (IntCondition) EditorGUILayout.EnumPopup ("Value is:", intCondition);
				checkParameterID = Action.ChooseParameterGUI ("Integer:", parameters, checkParameterID, ParameterType.Integer);
				if (checkParameterID < 0)
				{
					intValue = EditorGUILayout.IntField ("Integer:", intValue);
				}
			}
		}
		
		
		public override string SetLabel ()
		{
			return (" (" + saveCheck.ToString () + ")");
		}
		
		#endif
		
	}
	
}