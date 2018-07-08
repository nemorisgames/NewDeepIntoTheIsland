/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionMenuSetInputBox.cs"
 * 
 *	This action replaces the text within an Input box element.
 * 
 */

using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	[System.Serializable]
	public class ActionMenuSetInputBox : Action
	{
		
		public string menuName;
		public int menuNameParameterID = -1;
		public string elementName;
		public int elementNameParameterID = -1;

		public string newLabel;
		public int newLabelParameterID = -1;

		public enum SetMenuInputBoxSource { EnteredHere, FromGlobalVariable };
		public SetMenuInputBoxSource setMenuInputBoxSource = SetMenuInputBoxSource.EnteredHere;

		public int varID = 0;
		public int varParameterID = -1;

		
		public ActionMenuSetInputBox ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Menu;
			title = "Set Input box text";
			description = "Replaces the text within an Input box element.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			menuName = AssignString (parameters, menuNameParameterID, menuName);
			elementName = AssignString (parameters, elementNameParameterID, elementName);
			newLabel = AssignString (parameters, newLabelParameterID, newLabel);
			varID = AssignVariableID (parameters, varParameterID, varID);
		}

		
		override public float Run ()
		{
			if (menuName != "" && elementName != "")
			{
				MenuElement menuElement = PlayerMenus.GetElementWithName (menuName, elementName);
				if (menuElement is MenuInput)
				{
					MenuInput menuInput = (MenuInput) menuElement;
					if (setMenuInputBoxSource == SetMenuInputBoxSource.EnteredHere)
					{
						menuInput.SetLabel (newLabel);
					}
					else if (setMenuInputBoxSource == SetMenuInputBoxSource.FromGlobalVariable)
					{
						menuInput.SetLabel (GlobalVariables.GetStringValue (varID));
					}
				}
			}
			
			return 0f;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			menuNameParameterID = Action.ChooseParameterGUI ("Menu containing Input:", parameters, menuNameParameterID, ParameterType.String);
			if (menuNameParameterID < 0)
			{
				menuName = EditorGUILayout.TextField ("Menu containing Input:", menuName);
			}
			
			elementNameParameterID = Action.ChooseParameterGUI ("Input element name:", parameters, elementNameParameterID, ParameterType.String);
			if (elementNameParameterID < 0)
			{
				elementName = EditorGUILayout.TextField ("Input element name:", elementName);
			}

			setMenuInputBoxSource = (SetMenuInputBoxSource) EditorGUILayout.EnumPopup ("New text is:", setMenuInputBoxSource);
			if (setMenuInputBoxSource == SetMenuInputBoxSource.EnteredHere)
			{
				newLabelParameterID = Action.ChooseParameterGUI ("New text:", parameters, newLabelParameterID, ParameterType.String);
				if (newLabelParameterID < 0)
				{
					newLabel = EditorGUILayout.TextField ("New text:", newLabel);
				}
			}
			else if (setMenuInputBoxSource == SetMenuInputBoxSource.FromGlobalVariable)
			{
				varParameterID = Action.ChooseParameterGUI ("String variable:", parameters, varParameterID, ParameterType.String);
				if (varParameterID == -1)
				{
					varID = AdvGame.GlobalVariableGUI ("String variable:", varID, VariableType.String);
				}
			}

			AfterRunningOption ();
		}
		
		
		override public string SetLabel ()
		{
			string labelAdd = "";

			if (elementName != "")
			{
				labelAdd = " (" + elementName + " - ";
				if (setMenuInputBoxSource == SetMenuInputBoxSource.EnteredHere)
				{
					labelAdd += "'" + newLabel + "'";
				}
				else if (setMenuInputBoxSource == SetMenuInputBoxSource.FromGlobalVariable)
				{
					labelAdd += "from Variable";
				}
				labelAdd += ")";
			}
			return labelAdd;
		}


		public override bool ConvertLocalVariableToGlobal (int oldLocalID, int newGlobalID)
		{
			bool wasAmended = base.ConvertLocalVariableToGlobal (oldLocalID, newGlobalID);

			string updatedLabel = AdvGame.ConvertLocalVariableTokenToGlobal (newLabel, oldLocalID, newGlobalID);
			if (newLabel != updatedLabel)
			{
				wasAmended = true;
				newLabel = updatedLabel;
			}
			return wasAmended;
		}


		public override bool ConvertGlobalVariableToLocal (int oldGlobalID, int newLocalID, bool isCorrectScene)
		{
			bool isAffected = base.ConvertGlobalVariableToLocal (oldGlobalID, newLocalID, isCorrectScene);

			string updatedLabel = AdvGame.ConvertGlobalVariableTokenToLocal (newLabel, oldGlobalID, newLocalID);
			if (newLabel != updatedLabel)
			{
				isAffected = true;
				if (isCorrectScene)
				{
					newLabel = updatedLabel;
				}
			}
			return isAffected;
		}


		public override int GetVariableReferences (List<ActionParameter> parameters, VariableLocation location, int varID)
		{
			int thisCount = 0;
			string tokenText = (location == VariableLocation.Local) ? "[localvar:" + varID.ToString () + "]"
																		: "[var:" + varID.ToString () + "]";
			if (newLabel.Contains (tokenText))
			{
				thisCount ++;
			}
			thisCount += base.GetVariableReferences (parameters, location, varID);
			return thisCount;
		}

		#endif
		
	}
	
}
