/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionRandomCheck.cs"
 * 
 *	This action checks the value of a random number
 *	and performs different follow-up Actions accordingly.
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
	public class ActionRandomCheck : ActionCheckMultiple
	{

		public bool disallowSuccessive = false;
		public bool saveToVariable = true;
		private int ownVarValue = -1;

		public int parameterID = -1;
		public int variableID;
		public int variableNumber;
		public VariableLocation location = VariableLocation.Global;


		public ActionRandomCheck ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Variable;
			title = "Check random number";
			description = "Picks a number at random between zero and a specified integer – the value of which determine which subsequent Action is run next.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			variableID = AssignVariableID (parameters, parameterID, variableID);
		}
		
		
		override public ActionEnd End (List<Action> actions)
		{
			if (numSockets <= 0)
			{
				ACDebug.LogWarning ("Could not compute Random check because no values were possible!");
				return GenerateStopActionEnd ();
			}

			GVar linkedVariable = null;
			if (saveToVariable)
			{
				if (location == VariableLocation.Local && !isAssetFile)
				{
					linkedVariable = LocalVariables.GetVariable (variableID);
				}
				else
				{
					linkedVariable = GlobalVariables.GetVariable (variableID);
				}
			}

			int randomResult = Random.Range (0, numSockets);
			if (numSockets > 1 && disallowSuccessive)
			{
				if (saveToVariable)
				{
					if (linkedVariable != null && linkedVariable.type == VariableType.Integer)
					{
						ownVarValue = linkedVariable.val;
					}
					else
					{
						ACDebug.LogWarning ("'Variable: Check random number' Action is referencing a Variable that does not exist or is not an Integer!");
					}
				}

				while (ownVarValue == randomResult)
				{
					randomResult = Random.Range (0, numSockets);
				}

				ownVarValue = randomResult;

				if (saveToVariable && linkedVariable != null && linkedVariable.type == VariableType.Integer)
				{
					linkedVariable.SetValue (ownVarValue);
				}
			}

			return ProcessResult (randomResult, actions);
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			numSockets = EditorGUILayout.IntSlider ("# of possible values:", numSockets, 1, 100);
			numSockets = Mathf.Max (1, numSockets);

			disallowSuccessive = EditorGUILayout.ToggleLeft ("Prevent same value twice?", disallowSuccessive);

			if (disallowSuccessive)
			{
				saveToVariable = EditorGUILayout.Toggle ("Save last value?", saveToVariable);
				if (saveToVariable)
				{
					if (isAssetFile)
					{
						location = VariableLocation.Global;
					}
					else
					{
						location = (VariableLocation) EditorGUILayout.EnumPopup ("Variable source:", location);
					}
					
					if (location == VariableLocation.Global)
					{
						if (AdvGame.GetReferences ().variablesManager)
						{
							parameterID = Action.ChooseParameterGUI ("Integer variable:", parameters, parameterID, ParameterType.GlobalVariable);
							if (parameterID >= 0)
							{
								variableID = ShowVarGUI (AdvGame.GetReferences ().variablesManager.vars, variableID, false);
							}
							else
							{
								EditorGUILayout.BeginHorizontal ();
								variableID = ShowVarGUI (AdvGame.GetReferences ().variablesManager.vars, variableID, true);
								if (GUILayout.Button (Resource.CogIcon, GUILayout.Width (20f), GUILayout.Height (15f)))
								{
									SideMenu ();
								}
								EditorGUILayout.EndHorizontal ();
							}
						}
					}
					else if (location == VariableLocation.Local)
					{
						if (KickStarter.localVariables)
						{
							parameterID = Action.ChooseParameterGUI ("Integer variable:", parameters, parameterID, ParameterType.LocalVariable);
							if (parameterID >= 0)
							{
								variableID = ShowVarGUI (KickStarter.localVariables.localVars, variableID, false);
							}
							else
							{
								EditorGUILayout.BeginHorizontal ();
								variableID = ShowVarGUI (KickStarter.localVariables.localVars, variableID, true);
								if (GUILayout.Button (Resource.CogIcon, GUILayout.Width (20f), GUILayout.Height (15f)))
								{
									SideMenu ();
								}
								EditorGUILayout.EndHorizontal ();
							}
						}
						else
						{
							EditorGUILayout.HelpBox ("No 'Local Variables' component found in the scene. Please add an AC GameEngine object from the Scene Manager.", MessageType.Info);
						}
					}
				}
			}
		}


		private void SideMenu ()
		{
			GenericMenu menu = new GenericMenu ();

			menu.AddItem (new GUIContent ("Auto-create " + location.ToString () + " variable"), false, Callback, "AutoCreate");
			menu.ShowAsContext ();
		}
		
		
		private void Callback (object obj)
		{
			switch (obj.ToString ())
			{
			case "AutoCreate":
				AutoCreateVariableWindow.Init ("Random/New integer", location, VariableType.Integer, this);
				break;

			case "Show":
				if (AdvGame.GetReferences () != null && AdvGame.GetReferences ().variablesManager != null)
				{
					AdvGame.GetReferences ().variablesManager.ShowVariable (variableID, location);
				}
				break;
			}
		}

				
		private int ShowVarSelectorGUI (List<GVar> vars, int ID)
		{
			variableNumber = -1;
			
			List<string> labelList = new List<string>();
			foreach (GVar _var in vars)
			{
				labelList.Add (_var.label);
			}
			
			variableNumber = GetVarNumber (vars, ID);
			
			if (variableNumber == -1)
			{
				// Wasn't found (variable was deleted?), so revert to zero
				ACDebug.LogWarning ("Previously chosen variable no longer exists!");
				variableNumber = 0;
				ID = 0;
			}
			
			variableNumber = EditorGUILayout.Popup ("Integer variable:", variableNumber, labelList.ToArray ());
			ID = vars[variableNumber].id;
			
			return ID;
		}


		private int ShowVarGUI (List<GVar> vars, int ID, bool changeID)
		{
			if (vars.Count > 0)
			{
				if (changeID)
				{
					ID = ShowVarSelectorGUI (vars, ID);
				}
				variableNumber = Mathf.Min (variableNumber, vars.Count-1);
				if (changeID)
				{
					if (vars[variableNumber].type != VariableType.Integer)
					{
						EditorGUILayout.HelpBox ("The selected Variable must be an Integer!", MessageType.Warning);
					}
				}
			}
			else
			{
				EditorGUILayout.HelpBox ("No variables exist!", MessageType.Info);
				ID = -1;
				variableNumber = -1;
			}
			
			return ID;
		}
		
		
		private int GetVarNumber (List<GVar> vars, int ID)
		{
			int i = 0;
			foreach (GVar _var in vars)
			{
				if (_var.id == ID)
				{
					return i;
				}
				i++;
			}
			return -1;
		}


		public override bool ConvertLocalVariableToGlobal (int oldLocalID, int newGlobalID)
		{
			bool wasAmended = base.ConvertLocalVariableToGlobal (oldLocalID, newGlobalID);

			if (saveToVariable)
			{
				if (location == VariableLocation.Local && variableID == oldLocalID)
				{
					location = VariableLocation.Global;
					variableID = newGlobalID;
					wasAmended = true;
				}
			}

			return wasAmended;
		}


		public override int GetVariableReferences (List<ActionParameter> parameters, VariableLocation _location, int varID)
		{
			int thisCount = 0;
			if (saveToVariable && location == _location && variableID == varID)
			{
				thisCount ++;
			}
			thisCount += base.GetVariableReferences (parameters, _location, varID);
			return thisCount;
		}
 

		public override bool ConvertGlobalVariableToLocal (int oldGlobalID, int newLocalID, bool isCorrectScene)
		{
			bool isAffected = base.ConvertGlobalVariableToLocal (oldGlobalID, newLocalID, isCorrectScene);

			if (saveToVariable)
			{
				if (location == VariableLocation.Global && variableID == oldGlobalID)
				{
					isAffected = true;

					if (isCorrectScene)
					{
						location = VariableLocation.Local;
						variableID = newLocalID;
					}
				}
			}

			return isAffected;
		}
		
		#endif
		
	}

}