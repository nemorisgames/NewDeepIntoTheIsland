/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionMenuSlotCheck.cs"
 * 
 *	This Action checks the number of slots on a menu element
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
	public class ActionMenuSlotCheck : ActionCheck
	{
		
		public string menuToCheck = "";
		public int menuToCheckParameterID = -1;
		
		public string elementToCheck = "";
		public int elementToCheckParameterID = -1;

		public int numToCheck;
		public int numToCheckParameterID = -1;
		public IntCondition intCondition;

		
		public ActionMenuSlotCheck ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Menu;
			title = "Check num slots";
			description = "Queries the number of slots on a given menu element.";
		}
		
		
		override public void AssignValues (List<ActionParameter> parameters)
		{
			menuToCheck = AssignString (parameters, menuToCheckParameterID, menuToCheck);
			elementToCheck = AssignString (parameters, elementToCheckParameterID, elementToCheck);
			numToCheck = AssignInteger (parameters, numToCheckParameterID, numToCheck);
		}
		
		
		override public bool CheckCondition ()
		{
			MenuElement menuElement = PlayerMenus.GetElementWithName (menuToCheck, elementToCheck);
			if (menuElement != null)
			{
				int numSlots = menuElement.GetNumSlots ();

				if (intCondition == IntCondition.EqualTo)
				{
					if (numToCheck == numSlots)
					{
						return true;
					}
				}
				else if (intCondition == IntCondition.LessThan)
				{
					if (numToCheck > numSlots)
					{
						return true;
					}
				}
				else if (intCondition == IntCondition.MoreThan)
				{
					if (numToCheck < numSlots)
					{
						return true;
					}
				}
				else if (intCondition == IntCondition.NotEqualTo)
				{
					if (numToCheck != numSlots)
					{
						return true;
					}
				}
			}
			
			return false;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			menuToCheckParameterID = Action.ChooseParameterGUI ("Menu containing element:", parameters, menuToCheckParameterID, ParameterType.String);
			if (menuToCheckParameterID < 0)
			{
				menuToCheck = EditorGUILayout.TextField ("Menu containing element:", menuToCheck);
			}
			
			elementToCheckParameterID = Action.ChooseParameterGUI ("Element to check:", parameters, elementToCheckParameterID, ParameterType.String);
			if (elementToCheckParameterID < 0)
			{
				elementToCheck = EditorGUILayout.TextField ("Element to check:", elementToCheck);
			}

			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Number of slots is:", GUILayout.Width (145f));
			intCondition = (IntCondition) EditorGUILayout.EnumPopup (intCondition);
			EditorGUILayout.EndHorizontal ();

			numToCheckParameterID = Action.ChooseParameterGUI ("Value:", parameters, numToCheckParameterID, ParameterType.Integer);
			if (numToCheckParameterID < 0)
			{
				numToCheck = EditorGUILayout.IntField ("Value:", numToCheck);
			}
		}
		
		
		public override string SetLabel ()
		{
			string labelAdd = " (" + menuToCheck + " " + elementToCheck + ")";
			return labelAdd;
		}
		
		#endif
		
	}
	
}