/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionInputCheck.cs"
 * 
 *	This action checks if a specific key
 *	is being pressed
 * 
 */

using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	[System.Serializable]
	public class ActionInputCheck : ActionCheck
	{
		
		public string inputName;
		public int parameterID = -1;
		
		public InputCheckType checkType = InputCheckType.Button;
		
		public IntCondition axisCondition;
		public float axisValue;
		
		
		public ActionInputCheck ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Input;
			title = "Check";
			description = "Queries whether or not the player is invoking a button or axis declared in Unity's Input manager.";
		}
		
		
		override public void AssignValues (List<ActionParameter> parameters)
		{
			inputName = AssignString (parameters, parameterID, inputName);
		}
		
		
		override public float Run ()
		{
			if (!isRunning)
			{
				isRunning = true;
				return defaultPauseTime / 6f;
			}
			else
			{
				isRunning = false;
				return 0f;
			}
		}
		
		
		override public bool CheckCondition ()
		{
			switch (checkType)
			{
				case InputCheckType.SingleTapOrClick:
					return KickStarter.playerInput.ClickedRecently ();

				case InputCheckType.DoubleTapOrClick:
					return KickStarter.playerInput.ClickedRecently (true);

				case InputCheckType.Button:
					if (inputName != "" && KickStarter.playerInput.InputGetButton (inputName))
					{
						return true;
					}
					break;
					
				case InputCheckType.Axis:
					if (inputName != "")
					{
						return CheckAxisValue (KickStarter.playerInput.InputGetAxis (inputName));
					}
					break;
			}
			return false;
		}
		
		
		private bool CheckAxisValue (float fieldValue)
		{
			if (axisCondition == IntCondition.EqualTo)
			{
				if (Mathf.Approximately (fieldValue, axisValue))
				{
					return true;
				}
			}
			else if (axisCondition == IntCondition.NotEqualTo)
			{
				if (!Mathf.Approximately (fieldValue, axisValue))
				{
					return true;
				}
			}
			else if (axisCondition == IntCondition.LessThan)
			{
				if (fieldValue < axisValue)
				{
					return true;
				}
			}
			else if (axisCondition == IntCondition.MoreThan)
			{
				if (fieldValue > axisValue)
				{
					return true;
				}
			}
			
			return false;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			checkType = (InputCheckType) EditorGUILayout.EnumPopup ("Check type:" , checkType);
			
			if (checkType == InputCheckType.Axis || checkType == InputCheckType.Button)
			{
				parameterID = Action.ChooseParameterGUI (checkType.ToString () + " name:", parameters, parameterID, ParameterType.String);
				if (parameterID < 0)
				{
					inputName = EditorGUILayout.TextField (checkType.ToString () + " name:", inputName);
				}
				
				if (checkType == InputCheckType.Axis)
				{
					EditorGUILayout.BeginHorizontal ();
					axisCondition = (IntCondition) EditorGUILayout.EnumPopup (axisCondition);
					axisValue = EditorGUILayout.FloatField (axisValue);
					EditorGUILayout.EndHorizontal ();
				}
			}
		}
		
		
		public override string SetLabel ()
		{
			return (" (" + inputName + ")");
		}
		
		#endif
		
	}
	
}