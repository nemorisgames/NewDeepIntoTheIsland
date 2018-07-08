/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionQTE.cs"
 * 
 *	This action checks if a specific key
 *	is being pressed within a set time limit
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
	public class ActionQTE : ActionCheck
	{

		public enum QTEType { SingleKeypress, HoldKey, ButtonMash, SingleAxis };
		public QTEType qteType = QTEType.SingleKeypress;

		public int menuNameParameterID = -1;
		public string menuName;
		public bool animateUI = false;
		public bool wrongKeyFails = false;

		public float axisThreshold = 0.2f;

		public int inputNameParameterID = -1;
		public string inputName;
		
		public int durationParameterID = -1;
		public float duration;
		
		public float holdDuration;
		public float cooldownTime;
		public int targetPresses;
		public bool doCooldown;

		
		public ActionQTE ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Input;
			title = "QTE";
			description = "Initiates a Quick Time Event for a set duration. The QTE type can either be a single key- press, holding a button down, or button-mashing. The Input button must be defined in Unity's Input Manager.";
		}
		
		
		override public void AssignValues (List<ActionParameter> parameters)
		{
			menuName = AssignString (parameters, menuNameParameterID, menuName);
			inputName = AssignString (parameters, inputNameParameterID, inputName);
			duration = AssignFloat (parameters, durationParameterID, duration);
		}
		
		
		override public float Run ()
		{
			if (string.IsNullOrEmpty (inputName) && (KickStarter.settingsManager.inputMethod != InputMethod.TouchScreen || qteType == QTEType.SingleAxis))
			{
				isRunning = false;
				return 0f;
			}

			if (duration <= 0f)
			{
				isRunning = false;
				return 0f;
			}

			if (!isRunning)
			{
				isRunning = true;

				Animator animator = null;
				if (!string.IsNullOrEmpty (menuName))
				{
					AC.Menu menu = PlayerMenus.GetMenuWithName (menuName);
					if (menu != null)
					{
						menu.TurnOn (true);
						if (animateUI && menu.canvas != null && menu.canvas.GetComponent <Animator>())
						{
							animator = menu.canvas.GetComponent <Animator>();
						}
					}
				}

				if (qteType == QTEType.SingleKeypress)
				{
					KickStarter.playerQTE.StartSinglePressQTE (inputName, duration, animator, wrongKeyFails);
				}
				else if (qteType == QTEType.SingleAxis)
				{
					KickStarter.playerQTE.StartSingleAxisQTE (inputName, duration, axisThreshold, animator, wrongKeyFails);
				}
				else if (qteType == QTEType.HoldKey)
				{
					KickStarter.playerQTE.StartHoldKeyQTE (inputName, duration, holdDuration, animator, wrongKeyFails);
				}
				else if (qteType == QTEType.ButtonMash)
				{
					KickStarter.playerQTE.StartButtonMashQTE (inputName, duration, targetPresses, doCooldown, cooldownTime, animator, wrongKeyFails);
				}

				return defaultPauseTime;
			}
			else
			{
				if (KickStarter.playerQTE.GetState () == QTEState.None)
				{
					return defaultPauseTime;
				}

				if (!string.IsNullOrEmpty (menuName))
				{
					AC.Menu menu = PlayerMenus.GetMenuWithName (menuName);
					if (menu != null)
					{
						menu.TurnOff (true);
					}
				}

				isRunning = false;
				return 0f;
			}
		}


		override public void Skip ()
		{
			KickStarter.playerQTE.SkipQTE ();
			if (menuName != "")
			{
				PlayerMenus.GetMenuWithName (menuName).TurnOff (true);
			}
		}
		

		override public bool CheckCondition ()
		{
			if (KickStarter.playerQTE.GetState () == QTEState.Win)
			{
				return true;
			}
			return false;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			qteType = (QTEType) EditorGUILayout.EnumPopup ("QTE type:" , qteType);

			string _label = (qteType == QTEType.SingleAxis) ? "axis" : "button";
			inputNameParameterID = Action.ChooseParameterGUI ("Input " + _label + " name:", parameters, inputNameParameterID, ParameterType.String);
			if (inputNameParameterID < 0)
			{
				inputName = EditorGUILayout.TextField ("Input " + _label + " name:", inputName);
			}

			if (qteType == QTEType.SingleAxis)
			{
				axisThreshold = EditorGUILayout.Slider ("Axis threshold:", axisThreshold, -1f, 1f);

				if (axisThreshold >= 0f)
				{
					_label = "Negative axis fails?";
				}
				else if (axisThreshold < 0f)
				{
					_label = "Positive axis fails?";
				}

				if (Mathf.Approximately (axisThreshold, 0f))
				{
					EditorGUILayout.HelpBox ("The 'Axis threshold' cannot be zero.", MessageType.Warning);
				}
				else if (axisThreshold > 0f)
				{
					EditorGUILayout.HelpBox ("The QTE will be succesful when the input value is greater than the 'Axis threshold'.", MessageType.Info);
				}
				else if (axisThreshold < 0f)
				{
					EditorGUILayout.HelpBox ("The QTE will be succesful when the input value is less than than the 'Axis threshold'.", MessageType.Info);
				}
			}
			else
			{
				_label = "Wrong button fails?";
		
				if (KickStarter.settingsManager != null && KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen)
				{
					EditorGUILayout.HelpBox ("If the input name field is left blank, then all screen taps will be valid.", MessageType.Info);
				}
			}

			wrongKeyFails = EditorGUILayout.Toggle (_label, wrongKeyFails);

			durationParameterID = Action.ChooseParameterGUI ("Duration (s):", parameters, durationParameterID, ParameterType.Float);
			if (durationParameterID < 0)
			{
				duration = EditorGUILayout.Slider ("Duration (s):", duration, 0f, 10f);
			}
			
			if (qteType == QTEType.ButtonMash)
			{
				targetPresses = EditorGUILayout.IntField ("Target # of presses:", targetPresses);
				doCooldown = EditorGUILayout.Toggle ("Cooldown effect?", doCooldown);
				if (doCooldown)
				{
					cooldownTime = EditorGUILayout.Slider ("Cooldown time (s):", cooldownTime, 0f, duration);
				}
			}
			else if (qteType == QTEType.HoldKey)
			{
				holdDuration = EditorGUILayout.Slider ("Required duration (s):", holdDuration, 0f, 10f);
			}

			menuNameParameterID = Action.ChooseParameterGUI ("Menu to display (optional):", parameters, menuNameParameterID, ParameterType.String);
			if (menuNameParameterID < 0)
			{
				menuName = EditorGUILayout.TextField ("Menu to display (optional):", menuName);
			}

			animateUI = EditorGUILayout.Toggle ("Animate UI?", animateUI);

			if (animateUI)
			{
				if (qteType == QTEType.SingleKeypress || qteType == QTEType.SingleAxis)
				{
					EditorGUILayout.HelpBox ("The Menu's Canvas must have an Animator with 2 States: Win, Lose.", MessageType.Info);
				}
				else if (qteType == QTEType.ButtonMash)
				{
					EditorGUILayout.HelpBox ("The Menu's Canvas must have an Animator with 3 States: Hit, Win, Lose.", MessageType.Info);
				}
				else if (qteType == QTEType.HoldKey)
				{
					EditorGUILayout.HelpBox ("The Menu's Canvas must have an Animator with 2 States: Win, Lose, and 1 Trigger: Held.", MessageType.Info);
				}
			}
		}
		
		
		public override string SetLabel ()
		{
			return (" (" + qteType.ToString () + " - " + inputName + ")");
		}
		
		#endif
		
	}
	
}
