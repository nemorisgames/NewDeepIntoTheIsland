/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionVarSequence.cs"
 * 
 *	This action reads a Popup Variable and performs
 *	different follow-up Actions based on its value.
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
	public class ActionVarPopup : ActionCheckMultiple
	{
		
		public int variableID;
		public int variableNumber;
		public VariableLocation location = VariableLocation.Global;

		private LocalVariables localVariables;

		
		public ActionVarPopup ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Variable;
			title = "Pop Up switch";
			description = "Uses the value of a Pop Up Variable to determine which Action is run next. An option for each possible value the Variable can take will be displayed, allowing for different subsequent Actions to run.";
		}


		override public void AssignParentList (ActionList actionList)
		{
			if (actionList != null)
			{
				localVariables = UnityVersionHandler.GetLocalVariablesOfGameObject (actionList.gameObject);
			}
			if (localVariables == null)
			{
				localVariables = KickStarter.localVariables;
			}
		}
		
		
		override public ActionEnd End (List<Action> actions)
		{
			if (numSockets <= 0)
			{
				ACDebug.LogWarning ("Could not compute Random check because no values were possible!");
				return GenerateStopActionEnd ();
			}
			
			if (variableID == -1)
			{
				return GenerateStopActionEnd ();
			}
			
			GVar var = GetVariable ();

			if (var != null)
			{
				if (var.type == VariableType.PopUp)
				{
					var.Download ();
					return ProcessResult (var.val, actions);
				}
				else
				{
					ACDebug.LogWarning ("Variable: Run sequence Action is referencing a Variable that does not exist!");
				}
			}
			
			return GenerateStopActionEnd ();
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			if (isAssetFile)
			{
				location = VariableLocation.Global;
			}
			else
			{
				location = (VariableLocation) EditorGUILayout.EnumPopup ("Source:", location);
			}
			
			if (location == VariableLocation.Global)
			{
				if (AdvGame.GetReferences ().variablesManager)
				{
					variableID = AdvGame.GlobalVariableGUI ("Global PopUp var:", variableID, VariableType.PopUp);
				}
			}
			
			else if (location == VariableLocation.Local)
			{
				if (localVariables)
				{
					variableID = AdvGame.LocalVariableGUI ("Local PopUp var:", variableID, VariableType.PopUp);
				}
				else
				{
					EditorGUILayout.HelpBox ("No 'Local Variables' component found in the scene. Please add an AC GameEngine object from the Scene Manager.", MessageType.Info);
				}
			}

			GVar _var = GetVariable ();
			if (_var != null)
			{
				numSockets = _var.popUps.Length;
			}
		}


		override public string SetLabel ()
		{
			if (location == VariableLocation.Local && !isAssetFile)
			{
				if (localVariables)
				{
					return GetLabelString (localVariables.localVars);
				}
			}
			else
			{
				if (AdvGame.GetReferences ().variablesManager)
				{
					return GetLabelString (AdvGame.GetReferences ().variablesManager.vars);
				}
			}
			
			return "";
		}
		
		
		private string GetLabelString (List<GVar> vars)
		{
			string labelAdd = "";

			if (vars != null)
			{
				foreach (GVar _var in vars)
				{
					if (_var.id == variableID && _var.type == VariableType.PopUp)
					{
						labelAdd = " (" + _var.label + ")";
					}
				}
			}

			return labelAdd;
		}


		override public void SkipActionGUI (List<Action> actions, bool showGUI)
		{
			if (numSockets < 0)
			{
				numSockets = 0;
			}
		
			if (numSockets < endings.Count)
			{
				endings.RemoveRange (numSockets, endings.Count - numSockets);
			}
			else if (numSockets > endings.Count)
			{
				if (numSockets > endings.Capacity)
				{
					endings.Capacity = numSockets;
				}
				for (int i=endings.Count; i<numSockets; i++)
				{
					ActionEnd newEnd = new ActionEnd ();
					endings.Add (newEnd);
				}
			}
			
			foreach (ActionEnd ending in endings)
			{
				if (showGUI)
				{
					EditorGUILayout.Space ();
					int i = endings.IndexOf (ending);

					GVar _var = GetVariable ();
					if (_var != null)
					{
						ending.resultAction = (ResultAction) EditorGUILayout.EnumPopup ("If result is " + _var.popUps[i] + ":", (ResultAction) ending.resultAction);
					}
					else
					{
						ending.resultAction = (ResultAction) EditorGUILayout.EnumPopup ("If result is " + (i+1).ToString () + ":", (ResultAction) ending.resultAction);
					}
				}
				
				if (ending.resultAction == ResultAction.RunCutscene && showGUI)
				{
					if (isAssetFile)
					{
						ending.linkedAsset = (ActionListAsset) EditorGUILayout.ObjectField ("ActionList to run:", ending.linkedAsset, typeof (ActionListAsset), false);
					}
					else
					{
						ending.linkedCutscene = (Cutscene) EditorGUILayout.ObjectField ("Cutscene to run:", ending.linkedCutscene, typeof (Cutscene), true);
					}
				}
				else if (ending.resultAction == ResultAction.Skip)
				{
					SkipActionGUI (ending, actions, showGUI);
				}
				/*else
				{
					EditorGUILayout.Space ();
					EditorGUILayout.Space ();
					EditorGUILayout.Space ();
				}*/
			}
		}


		public override bool ConvertLocalVariableToGlobal (int oldLocalID, int newGlobalID)
		{
			bool wasAmended = base.ConvertLocalVariableToGlobal (oldLocalID, newGlobalID);

			if (location == VariableLocation.Local && variableID == oldLocalID)
			{
				location = VariableLocation.Global;
				variableID = newGlobalID;
				wasAmended = true;
			}
			return wasAmended;
		}


		public override bool ConvertGlobalVariableToLocal (int oldGlobalID, int newLocalID, bool isCorrectScene)
		{
			bool wasAmended = base.ConvertGlobalVariableToLocal (oldGlobalID, newLocalID, isCorrectScene);

			if (location == VariableLocation.Global && variableID == oldGlobalID)
			{
				wasAmended = true;
				if (isCorrectScene)
				{
					location = VariableLocation.Local;
					variableID = newLocalID;
				}
			}
			return wasAmended;
		}


		public override int GetVariableReferences (List<ActionParameter> parameters, VariableLocation _location, int varID)
		{
			int thisCount = 0;

			if (location == _location && variableID == varID)
			{
				thisCount ++;
			}

			thisCount += base.GetVariableReferences (parameters, _location, varID);
			return thisCount;
		}
		#endif


		private GVar GetVariable ()
		{
			GVar var = null;
			
			if (location == VariableLocation.Local && !isAssetFile)
			{
				var = LocalVariables.GetVariable (variableID, localVariables);
			}
			else
			{
				if (Application.isPlaying)
				{
					var = GlobalVariables.GetVariable (variableID);
				}
				else if (AdvGame.GetReferences ().variablesManager)
				{
					var = AdvGame.GetReferences ().variablesManager.GetVariable (variableID);
				}
			}
			
			if (var != null && var.type == VariableType.PopUp)
			{
				return var;
			}
			
			return null;
		}

	}
	
}