/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionVarSet.cs"
 * 
 *	This action is used to set the value of Global and Local Variables
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
	public class ActionVarSet : Action
	{
		
		public SetVarMethod setVarMethod;
		public SetVarMethodString setVarMethodString = SetVarMethodString.EnteredHere;
		public SetVarMethodIntBool setVarMethodIntBool = SetVarMethodIntBool.EnteredHere;

		public SetVarMethodVector setVarMethodVector = SetVarMethodVector.SetValue;
		public enum SetVarMethodVector { SetValue, IncreaseByValue };

		public int parameterID = -1;
		public int variableID;
		public int variableNumber;

		public int setParameterID = -1;
		public int slotNumber = 0;
		public int slotNumberParameterID = -1;
		
		public int intValue;
		public float floatValue;
		public BoolValue boolValue;
		public string stringValue;
		public string formula;
		public Vector3 vector3Value;

		public int lineID = -1;

		public VariableLocation location;

		public string menuName;
		public string elementName;

		public Animator animator;
		public string parameterName;

		private LocalVariables localVariables;

			
		public ActionVarSet ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Variable;
			title = "Set";
			description = "Sets the value of both Global and Local Variables, as declared in the Variables Manager. Integers can be set to absolute, incremented or assigned a random value. Strings can also be set to the value of a MenuInput element, while Integers, Booleans and Floats can also be set to the value of a Mecanim parameter. When setting Integers and Floats, you can also opt to type in a forumla (e.g. 2 + 3 *4), which can also include tokens of the form [var:ID] to denote the value of a Variable, where ID is the unique number given to a Variable in the Variables Manager.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			variableID = AssignVariableID (parameters, parameterID, variableID);

			intValue = AssignInteger (parameters, setParameterID, intValue);
			boolValue = AssignBoolean (parameters, setParameterID, boolValue);
			floatValue = AssignFloat (parameters, setParameterID, floatValue);
			vector3Value = AssignVector3 (parameters, setParameterID, vector3Value);
			stringValue = AssignString (parameters, setParameterID, stringValue);
			formula = AssignString (parameters, setParameterID, formula);
			slotNumber = AssignInteger (parameters, slotNumberParameterID, slotNumber);
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


		override public float Run ()
		{
			if (variableID != -1)
			{
				if (location == VariableLocation.Local && !isAssetFile)
				{
					SetVariable (LocalVariables.GetVariable (variableID, localVariables), VariableLocation.Local, false);
				}
				else
				{
					SetVariable (GlobalVariables.GetVariable (variableID), VariableLocation.Global, false);
				}
			}

			return 0f;
		}


		override public void Skip ()
		{
			if (variableID != -1)
			{
				if (location == VariableLocation.Local && !isAssetFile)
				{
					SetVariable (LocalVariables.GetVariable (variableID, localVariables), VariableLocation.Local, true);
				}
				else
				{
					SetVariable (GlobalVariables.GetVariable (variableID), VariableLocation.Global, true);
				}
			}
		}

		
		private void SetVariable (GVar var, VariableLocation location, bool doSkip)
		{
			if (var == null)
			{
				return;
			}

			if (location == VariableLocation.Global)
			{
				var.Download ();
			}

			if (var.type == VariableType.Integer)
			{
				int _value = 0;

				if (setVarMethodIntBool == SetVarMethodIntBool.EnteredHere)
				{
					if (setVarMethod == SetVarMethod.Formula)
					{
						_value = (int) AdvGame.CalculateFormula (AdvGame.ConvertTokens (formula, Options.GetLanguage (), localVariables));
					}
					else
					{
						_value = intValue;
					}
				}
				else if (setVarMethodIntBool == SetVarMethodIntBool.SetAsMecanimParameter)
				{
					if (animator && parameterName != "")
					{
						_value = animator.GetInteger (parameterName);
						setVarMethod = SetVarMethod.SetValue;
					}	
				}

				if (setVarMethod == SetVarMethod.IncreaseByValue && doSkip)
				{
					var.RestoreBackupValue ();
				}

				var.SetValue (_value, setVarMethod);

				if (doSkip)
				{
					var.BackupValue ();
				}
			}
			if (var.type == VariableType.Float)
			{
				float _value = 0;
				
				if (setVarMethodIntBool == SetVarMethodIntBool.EnteredHere)
				{
					if (setVarMethod == SetVarMethod.Formula)
					{
						_value = (float) AdvGame.CalculateFormula (AdvGame.ConvertTokens (formula, Options.GetLanguage (), localVariables));
					}
					else
					{
						_value = floatValue;
					}
				}
				else if (setVarMethodIntBool == SetVarMethodIntBool.SetAsMecanimParameter)
				{
					if (animator && parameterName != "")
					{
						_value = animator.GetFloat (parameterName);
						setVarMethod = SetVarMethod.SetValue;
					}	
				}

				if (setVarMethod == SetVarMethod.IncreaseByValue && doSkip)
				{
					var.RestoreBackupValue ();
				}
				
				var.SetFloatValue (_value, setVarMethod);
				
				if (doSkip)
				{
					var.BackupValue ();
				}
			}
			else if (var.type == VariableType.Boolean)
			{
				int _value = 0;

				if (setVarMethodIntBool == SetVarMethodIntBool.EnteredHere)
				{
					_value = (int) boolValue;
				}
				else if (setVarMethodIntBool == SetVarMethodIntBool.SetAsMecanimParameter)
				{
					if (animator && parameterName != "")
					{
						if (animator.GetBool (parameterName))
						{
							_value = 1;
						}
					}
				}

				var.SetValue (_value, SetVarMethod.SetValue);
			}
			else if (var.type == VariableType.Vector3)
			{
				Vector3 newValue = vector3Value;
				if (setVarMethodVector == SetVarMethodVector.IncreaseByValue)
				{
					newValue += var.vector3Val;
				}

				var.SetVector3Value (newValue);
			}
			else if (var.type == VariableType.PopUp)
			{
				int _value = 0;
				
				if (setVarMethod == SetVarMethod.Formula)
				{
					_value = (int) AdvGame.CalculateFormula (AdvGame.ConvertTokens (formula, Options.GetLanguage (), localVariables));
				}
				else if (setVarMethod == SetVarMethod.SetAsRandom)
				{
					if (var.popUps != null)
					{
						_value = var.popUps.Length;
					}
				}
				else
				{
					_value = intValue;
				}

				if (setVarMethod == SetVarMethod.IncreaseByValue && doSkip)
				{
					var.RestoreBackupValue ();
				}
				
				var.SetValue (_value, setVarMethod);
				
				if (doSkip)
				{
					var.BackupValue ();
				}
			}
			else if (var.type == VariableType.String)
			{
				string _value = "";

				if (setVarMethodString == SetVarMethodString.EnteredHere)
				{
					_value = AdvGame.ConvertTokens (stringValue, Options.GetLanguage (), localVariables);
				}
				else if (setVarMethodString == SetVarMethodString.SetAsMenuElementText)
				{
					MenuElement menuElement = PlayerMenus.GetElementWithName (menuName, elementName);
					if (menuElement != null)
					{
						if (menuElement is MenuInput)
						{
							MenuInput menuInput = (MenuInput) menuElement;
							_value = menuInput.GetContents ();

							if (KickStarter.runtimeLanguages.LanguageReadsRightToLeft (Options.GetLanguage ()) && _value.Length > 0)
							{
								// Invert
								char[] charArray = _value.ToCharArray ();
								_value = "";
								for (int i = charArray.Length-1; i >= 0; i --)
								{
									_value += charArray[i];
								}
							}
						}
						else
						{
							PlayerMenus.GetMenuWithName (menuName).Recalculate ();
							menuElement.PreDisplay (slotNumber, Options.GetLanguage (), false);
							_value = menuElement.GetLabel (slotNumber, Options.GetLanguage ());
						}
					}
					else
					{
						ACDebug.LogWarning ("Could not find MenuInput '" + elementName + "' in Menu '" + menuName + "'");
					}
				}

				var.SetStringValue (_value, lineID);
			}

			if (location == VariableLocation.Global)
			{
				var.Upload ();
			}

			KickStarter.actionListManager.VariableChanged ();
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
					ShowVarGUI (AdvGame.GetReferences ().variablesManager.vars, parameters, ParameterType.GlobalVariable);
				}
			}
			
			else if (location == VariableLocation.Local)
			{
				if (localVariables)
				{
					ShowVarGUI (localVariables.localVars, parameters, ParameterType.LocalVariable);
				}
				else
				{
					EditorGUILayout.HelpBox ("No 'Local Variables' component found in the scene. Please add an AC GameEngine object from the Scene Manager.", MessageType.Info);
				}
			}
		}


		public bool IsTranslatable ()
		{
			if (setVarMethodString == SetVarMethodString.EnteredHere && setParameterID < 0)
			{
				GVar variable = null;

				if (location == VariableLocation.Global)
				{
					VariablesManager variablesManager = AdvGame.GetReferences ().variablesManager;
					if (variablesManager != null)
					{
						variable = variablesManager.GetVariable (variableID);
					}
				}
				else
				{
					variable = LocalVariables.GetVariable (variableID);
				}

				if (variable != null && variable.type == VariableType.String)
				{
					return true;
				}
			}
			return false;
		}


		private void ShowVarGUI (List<GVar> vars, List<ActionParameter> parameters, ParameterType parameterType)
		{
			// Create a string List of the field's names (for the PopUp box)
			List<string> labelList = new List<string>();
			
			int i = 0;
			if (parameterID == -1)
			{
				variableNumber = -1;
			}
			
			if (vars.Count > 0)
			{
				foreach (GVar _var in vars)
				{
					labelList.Add (_var.label);
					
					// If a GlobalVar variable has been removed, make sure selected variable is still valid
					if (_var.id == variableID)
					{
						variableNumber = i;
					}
					
					i ++;
				}
				
				if (variableNumber == -1 && (parameters == null || parameters.Count == 0 || parameterID == -1))
				{
					// Wasn't found (variable was deleted?), so revert to zero
					ACDebug.LogWarning ("Previously chosen variable no longer exists!");
					variableNumber = 0;
					variableID = 0;
				}

				parameterID = Action.ChooseParameterGUI ("Variable:", parameters, parameterID, parameterType);
				if (parameterID >= 0)
				{
					//variableNumber = 0;
					variableNumber = Mathf.Min (variableNumber, vars.Count-1);
					variableID = -1;
				}
				else
				{
					variableNumber = EditorGUILayout.Popup ("Variable:", variableNumber, labelList.ToArray());
					variableID = vars [variableNumber].id;
				}

				string label = "Statement: ";

				if (vars [variableNumber].type == VariableType.Boolean)
				{
					setVarMethodIntBool = (SetVarMethodIntBool) EditorGUILayout.EnumPopup ("New value is:", setVarMethodIntBool);

					label += "=";
					if (setVarMethodIntBool == SetVarMethodIntBool.EnteredHere)
					{
						setParameterID = Action.ChooseParameterGUI (label, parameters, setParameterID, ParameterType.Boolean);
						if (setParameterID < 0)
						{
							boolValue = (BoolValue) EditorGUILayout.EnumPopup (label, boolValue);
						}
					}
					else if (setVarMethodIntBool == SetVarMethodIntBool.SetAsMecanimParameter)
					{
						ShowMecanimGUI ();
					}
				}
				if (vars [variableNumber].type == VariableType.PopUp)
				{
					setVarMethod = (SetVarMethod) EditorGUILayout.EnumPopup ("Method:", setVarMethod);
					
					if (setVarMethod == SetVarMethod.Formula)
					{
						label += "=";
						
						setParameterID = Action.ChooseParameterGUI (label, parameters, setParameterID, ParameterType.String);
						if (setParameterID < 0)
						{
							formula = EditorGUILayout.TextField (label, formula);
						}
						
						#if UNITY_WP8
						EditorGUILayout.HelpBox ("This feature is not available for Windows Phone 8.", MessageType.Warning);
						#endif
					}
					else if (setVarMethod == SetVarMethod.IncreaseByValue || setVarMethod == SetVarMethod.SetValue)
					{
						if (setVarMethod == SetVarMethod.IncreaseByValue)
						{
							label += "+=";
						}
						else if (setVarMethod == SetVarMethod.SetValue)
						{
							label += "=";
						}

						setParameterID = Action.ChooseParameterGUI (label, parameters, setParameterID, ParameterType.Integer);
						if (setParameterID < 0)
						{
							if (setVarMethod == SetVarMethod.SetValue)
							{
								intValue = EditorGUILayout.Popup (label, intValue, vars[variableNumber].popUps);
							}
							else
							{
								intValue = EditorGUILayout.IntField (label, intValue);
							}
							
							if (setVarMethod == SetVarMethod.SetAsRandom && intValue < 0)
							{
								intValue = 0;
							}
						}
					}

				}
				else if (vars [variableNumber].type == VariableType.Integer)
				{
					setVarMethodIntBool = (SetVarMethodIntBool) EditorGUILayout.EnumPopup ("New value is:", setVarMethodIntBool);

					if (setVarMethodIntBool == SetVarMethodIntBool.EnteredHere)
					{
						setVarMethod = (SetVarMethod) EditorGUILayout.EnumPopup ("Method:", setVarMethod);

						if (setVarMethod == SetVarMethod.Formula)
						{
							label += "=";
							
							setParameterID = Action.ChooseParameterGUI (label, parameters, setParameterID, ParameterType.String);
							if (setParameterID < 0)
							{
								formula = EditorGUILayout.TextField (label, formula);
							}
							
							#if UNITY_WP8
							EditorGUILayout.HelpBox ("This feature is not available for Windows Phone 8.", MessageType.Warning);
							#endif
						}
						else
						{
							if (setVarMethod == SetVarMethod.IncreaseByValue)
							{
								label += "+=";
							}
							else if (setVarMethod == SetVarMethod.SetValue)
							{
								label += "=";
							}
							else if (setVarMethod == SetVarMethod.SetAsRandom)
							{
								label += ("= 0 to");
							}

							setParameterID = Action.ChooseParameterGUI (label, parameters, setParameterID, ParameterType.Integer);
							if (setParameterID < 0)
							{
								intValue = EditorGUILayout.IntField (label, intValue);

								if (setVarMethod == SetVarMethod.SetAsRandom && intValue < 0)
								{
									intValue = 0;
								}
							}
						}
					}
					else if (setVarMethodIntBool == SetVarMethodIntBool.SetAsMecanimParameter)
					{
						ShowMecanimGUI ();
					}
				}
				else if (vars [variableNumber].type == VariableType.Float)
				{
					setVarMethodIntBool = (SetVarMethodIntBool) EditorGUILayout.EnumPopup ("New value is:", setVarMethodIntBool);

					if (setVarMethodIntBool == SetVarMethodIntBool.EnteredHere)
					{
						setVarMethod = (SetVarMethod) EditorGUILayout.EnumPopup ("Method:", setVarMethod);

						if (setVarMethod == SetVarMethod.Formula)
						{
							label += "=";

							setParameterID = Action.ChooseParameterGUI (label, parameters, setParameterID, ParameterType.String);
							if (setParameterID < 0)
							{
								formula = EditorGUILayout.TextField (label, formula);
							}
							
							#if UNITY_WP8
							EditorGUILayout.HelpBox ("This feature is not available for Windows Phone 8.", MessageType.Warning);
							#endif
						}
						else
						{
							if (setVarMethod == SetVarMethod.IncreaseByValue)
							{
								label += "+=";
							}
							else if (setVarMethod == SetVarMethod.SetValue)
							{
								label += "=";
							}
							else if (setVarMethod == SetVarMethod.SetAsRandom)
							{
								label += "= 0 to";
							}

							setParameterID = Action.ChooseParameterGUI (label, parameters, setParameterID, ParameterType.Float);
							if (setParameterID < 0)
							{
								floatValue = EditorGUILayout.FloatField (label, floatValue);
								
								if (setVarMethod == SetVarMethod.SetAsRandom && floatValue < 0f)
								{
									floatValue = 0f;
								}
							}
						}
					}
					else if (setVarMethodIntBool == SetVarMethodIntBool.SetAsMecanimParameter)
					{
						ShowMecanimGUI ();
					}
				}
				else if (vars [variableNumber].type == VariableType.String)
				{
					setVarMethodString = (SetVarMethodString) EditorGUILayout.EnumPopup ("New value is:", setVarMethodString);

					label += "=";
					if (setVarMethodString == SetVarMethodString.EnteredHere)
					{
						setParameterID = Action.ChooseParameterGUI (label, parameters, setParameterID, ParameterType.String);
						if (setParameterID < 0)
						{
							EditorGUILayout.BeginHorizontal ();
							EditorGUILayout.LabelField (label, GUILayout.Width (140f));
							EditorStyles.textField.wordWrap = true;
							stringValue = EditorGUILayout.TextArea (stringValue, GUILayout.MaxWidth (400f));
							EditorGUILayout.EndHorizontal ();
						}
					}
					else if (setVarMethodString == SetVarMethodString.SetAsMenuElementText)
					{
						menuName = EditorGUILayout.TextField ("Menu name:", menuName);
						elementName = EditorGUILayout.TextField ("Element name:", elementName);

						slotNumberParameterID = Action.ChooseParameterGUI ("Slot # (optional):", parameters, slotNumberParameterID, ParameterType.Integer);
						if (slotNumberParameterID < 0)
						{
							slotNumber = EditorGUILayout.IntField ("Slot # (optional):", slotNumber);
						}
					}
				}
				else if (vars [variableNumber].type == VariableType.Vector3)
				{
					setVarMethodVector = (SetVarMethodVector) EditorGUILayout.EnumPopup ("Method:", setVarMethodVector);

					if (setVarMethodVector == SetVarMethodVector.IncreaseByValue)
					{
						label += "+=";
					}
					else if (setVarMethodVector == SetVarMethodVector.SetValue)
					{
						label += "=";
					}

					setParameterID = Action.ChooseParameterGUI (label, parameters, setParameterID, ParameterType.Vector3);
					if (setParameterID < 0)
					{
						vector3Value = EditorGUILayout.Vector3Field (label, vector3Value);
					}
				}

				AfterRunningOption ();
			}
			else
			{
				EditorGUILayout.HelpBox ("No variables exist!", MessageType.Info);
				variableID = -1;
				variableNumber = -1;
			}
		}


		private void ShowMecanimGUI ()
		{
			animator = (Animator) EditorGUILayout.ObjectField ("Animator:", animator, typeof (Animator), true);
			parameterName = EditorGUILayout.TextField ("Parameter name:", parameterName);
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

			if (vars.Count > 0 && variableNumber > -1 && vars.Count > variableNumber)
			{
				labelAdd = " (" + vars [variableNumber].label;

				if (vars[variableNumber].type == VariableType.Integer)
				{
					if (setVarMethodIntBool == SetVarMethodIntBool.EnteredHere)
					{
						if (setVarMethod == SetVarMethod.IncreaseByValue)
						{
							labelAdd += " += " + intValue;
						}
						else if (setVarMethod == SetVarMethod.SetValue)
						{
							labelAdd += " = " + intValue;
						}
						else if (setVarMethod == SetVarMethod.SetAsRandom)
						{
							labelAdd += " = 0 to " + intValue;
						}
						else if (setVarMethod == SetVarMethod.Formula)
						{
							labelAdd += " = " + formula;
						}
					}
					else
					{
						labelAdd += " = " + parameterName;
					}
				}
				else if (vars[variableNumber].type == VariableType.Boolean)
				{
					if (setVarMethodIntBool == SetVarMethodIntBool.EnteredHere)
					{
						labelAdd += " = " + boolValue;
					}
					else
					{
						labelAdd += " = " + parameterName;
					}
				}
				else if (vars[variableNumber].type == VariableType.PopUp)
				{
					if (intValue >= 0 && intValue < vars[variableNumber].popUps.Length)
					{
						labelAdd += " = " + vars[variableNumber].popUps[intValue];
					}
				}
				else if (vars[variableNumber].type == VariableType.Float)
				{
					if (setVarMethodIntBool == SetVarMethodIntBool.EnteredHere)
					{
						if (setVarMethod == SetVarMethod.IncreaseByValue)
						{
							labelAdd += " += " + floatValue;
						}
						else if (setVarMethod == SetVarMethod.SetValue)
						{
							labelAdd += " = " + floatValue;
						}
						else if (setVarMethod == SetVarMethod.SetAsRandom)
						{
							labelAdd += " = 0 to " + floatValue;
						}
						else if (setVarMethod == SetVarMethod.Formula)
						{
							labelAdd += " = " + formula;
						}
					}
					else
					{
						labelAdd += " = " + parameterName;
					}
				}
				else if (vars[variableNumber].type == VariableType.String)
				{
					if (setVarMethodString == SetVarMethodString.EnteredHere)
					{
						labelAdd += " = " + stringValue;
					}
					else
					{
						labelAdd += " = " + elementName;
					}
				}

				labelAdd += ")";
			}

			return labelAdd;
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

			thisCount += base.GetVariableReferences (parameters, location, varID);
			return thisCount;
		}

		#endif

	}

}