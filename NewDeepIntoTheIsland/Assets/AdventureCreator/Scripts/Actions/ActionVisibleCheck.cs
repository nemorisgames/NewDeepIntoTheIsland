/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionVisibleCheck.cs"
 * 
 *	This action checks the visibilty of a GameObject.
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
	public class ActionVisibleCheck : ActionCheck
	{
		
		public int parameterID = -1;
		public int constantID = 0;
		public GameObject obToAffect;

		public CheckVisState checkVisState = CheckVisState.InScene;

		
		public ActionVisibleCheck ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Object;
			title = "Check visibility";
			description = "Checks the visibility of a GameObject.";
		}
		
		
		override public void AssignValues (List<ActionParameter> parameters)
		{
			obToAffect = AssignFile (parameters, parameterID, constantID, obToAffect);
		}


		override public bool CheckCondition ()
		{
			if (obToAffect)
			{
				if (obToAffect.GetComponent <Renderer>())
				{
					if (checkVisState == CheckVisState.InCamera)
					{
						return obToAffect.GetComponent <Renderer>().isVisible;
					}
					else if (checkVisState == CheckVisState.InScene)
					{
						return obToAffect.GetComponent <Renderer>().enabled;
					}
				}
				ACDebug.LogWarning ("Cannot check visibility of " + obToAffect.name + " as it has no renderer component");
			}
			return false;
		}
		
			
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			parameterID = Action.ChooseParameterGUI ("Object to check:", parameters, parameterID, ParameterType.GameObject);
			if (parameterID >= 0)
			{
				constantID = 0;
				obToAffect = null;
			}
			else
			{
				obToAffect = (GameObject) EditorGUILayout.ObjectField ("Object to check:", obToAffect, typeof (GameObject), true);
				
				constantID = FieldToID (obToAffect, constantID);
				obToAffect = IDToField (obToAffect, constantID, false);
			}

			checkVisState = (CheckVisState) EditorGUILayout.EnumPopup ("Visibility to check:", checkVisState);
		}


		override public void AssignConstantIDs (bool saveScriptsToo)
		{
			AssignConstantID (obToAffect, constantID, parameterID);
		}

		
		override public string SetLabel ()
		{
			string labelAdd = "";
			
			if (obToAffect)
				labelAdd = " (" + obToAffect.name + ")";
			
			return labelAdd;
		}
		
		#endif
		
	}
	
}