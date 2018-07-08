/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionParamSet.cs"
 * 
 *	This action sets the value of an ActionList's parameter.
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
	public class ActionParamSet : Action
	{

		public ActionListSource actionListSource = ActionListSource.InScene;
		
		public ActionList actionList;
		public int actionListConstantID;
		public ActionListAsset actionListAsset;

		public bool changeOwn;
		public int parameterID = -1;
		public int parameterToCopyID = -1;
		
		public int intValue, intValueMax;
		public float floatValue, floatValueMax;
		public string stringValue;

		public GameObject gameobjectValue;
		public int gameObjectConstantID;

		public Object unityObjectValue;

		public Vector3 vector3Value;

		public SetParamMethod setParamMethod = SetParamMethod.EnteredHere;
		public int globalVariableID;

		public int ownParamID = -1;

		private ActionParameter _parameter;
		private ActionParameter _parameterToCopy;
		#if UNITY_EDITOR
		[SerializeField] private string parameterLabel = "";
		#endif
		
		
		public ActionParamSet ()
		{
			this.isDisplayed = true;
			category = ActionCategory.ActionList;
			title = "Set parameter";
			description = "Sets the value of a parameter in an ActionList.";
		}
		
		
		override public void AssignValues (List<ActionParameter> parameters)
		{	
			if (!changeOwn)
			{
				if (actionListSource == ActionListSource.InScene)
				{
					actionList = AssignFile <ActionList> (actionListConstantID, actionList);
					if (actionList != null)
					{
						if (actionList.source == ActionListSource.AssetFile && actionList.assetFile != null)
						{
							if (actionList.syncParamValues && actionList.assetFile.useParameters)
							{
								_parameter = GetParameterWithID (actionList.assetFile.parameters, parameterID);
								_parameterToCopy = GetParameterWithID (actionList.assetFile.parameters, parameterToCopyID);
							}
							else
							{
								_parameter = GetParameterWithID (actionList.parameters, parameterID);
								_parameterToCopy = GetParameterWithID (actionList.parameters, parameterToCopyID);
							}
						}
						else if (actionList.source == ActionListSource.InScene && actionList.useParameters)
						{
							_parameter = GetParameterWithID (actionList.parameters, parameterID);
							_parameterToCopy = GetParameterWithID (actionList.parameters, parameterToCopyID);
						}
					}
				}
				else if (actionListSource == ActionListSource.AssetFile)
				{
					if (actionListAsset != null)
					{
						_parameter = GetParameterWithID (actionListAsset.parameters, parameterID);
						_parameterToCopy = GetParameterWithID (actionListAsset.parameters, parameterToCopyID);

						if (_parameter.parameterType == ParameterType.GameObject && !isAssetFile && gameobjectValue != null && gameObjectConstantID == 0)
						{
							if (gameobjectValue.GetComponent <ConstantID>())
							{
								gameObjectConstantID = gameobjectValue.GetComponent <ConstantID>().constantID;
							}
							else
							{
								ACDebug.LogWarning ("The GameObject '" + gameobjectValue.name + "' must have a Constant ID component in order to be passed as a parameter to an asset file.", gameobjectValue);
							}
						}
					}
				}
			}
			else
			{
				_parameter = GetParameterWithID (parameters, parameterID);
				_parameterToCopy = GetParameterWithID (parameters, parameterToCopyID);

				if (_parameter.parameterType == ParameterType.GameObject && isAssetFile && gameobjectValue != null && gameObjectConstantID == 0)
				{
					if (gameobjectValue.GetComponent <ConstantID>())
					{
						gameObjectConstantID = gameobjectValue.GetComponent <ConstantID>().constantID;
					}
					else
					{
						ACDebug.LogWarning ("The GameObject '" + gameobjectValue.name + "' must have a Constant ID component in order to be passed as a parameter to an asset file.", gameobjectValue);
					}
				}
			}

			gameobjectValue = AssignFile (gameObjectConstantID, gameobjectValue);

			if (setParamMethod == SetParamMethod.EnteredHere && _parameter != null)
			{
				switch (_parameter.parameterType)
				{
					case ParameterType.Boolean:
						BoolValue boolValue = (intValue == 1) ? BoolValue.True : BoolValue.False; 
						boolValue = AssignBoolean (parameters, ownParamID, boolValue);
						intValue = (boolValue == BoolValue.True) ? 1 : 0;
						break;

					case ParameterType.Float:
						floatValue = AssignFloat (parameters, ownParamID, floatValue);
						break;

					case ParameterType.GameObject:
						gameobjectValue = AssignFile (parameters, ownParamID, gameObjectConstantID, gameobjectValue);
						break;

					case ParameterType.GlobalVariable:
					case ParameterType.LocalVariable:
						intValue = AssignVariableID (parameters, ownParamID, intValue);
						break;

					case ParameterType.Integer:
						intValue = AssignInteger (parameters, ownParamID, intValue);
						break;

					case ParameterType.InventoryItem:
						intValue = AssignInvItemID (parameters, ownParamID, intValue);
						break;

					case ParameterType.String:
						stringValue = AssignString (parameters, ownParamID, stringValue);
						break;

					case ParameterType.UnityObject:
						unityObjectValue = AssignObject <Object> (parameters, ownParamID, unityObjectValue);
						break;

					case ParameterType.Vector3:
						vector3Value = AssignVector3 (parameters, ownParamID, vector3Value);
						break;
				}
			}
		}
		
		
		override public float Run ()
		{
			if (_parameter == null)
			{
				ACDebug.LogWarning ("Cannot set parameter value since it cannot be found!");
				return 0f;
			}

			if (setParamMethod == SetParamMethod.CopiedFromGlobalVariable)
			{
				GVar gVar = GlobalVariables.GetVariable (globalVariableID);
				if (gVar != null)
				{
					if (_parameter.parameterType == ParameterType.Boolean ||
						_parameter.parameterType == ParameterType.Integer)
					{
						_parameter.intValue = gVar.val;
					}
					else if (_parameter.parameterType == ParameterType.Float)
					{
						_parameter.floatValue = gVar.floatVal;
					}
					else if (_parameter.parameterType == ParameterType.Vector3)
					{
						_parameter.vector3Value = gVar.vector3Val;
					}
					else if (_parameter.parameterType == ParameterType.String)
					{
						_parameter.stringValue = GlobalVariables.GetStringValue (globalVariableID, true, Options.GetLanguage ());
					}
				}
			}
			else if (setParamMethod == SetParamMethod.EnteredHere)
			{
				if (_parameter.parameterType == ParameterType.Boolean ||
				    _parameter.parameterType == ParameterType.Integer ||
				    _parameter.parameterType == ParameterType.GlobalVariable ||
				    _parameter.parameterType == ParameterType.LocalVariable ||
				    _parameter.parameterType == ParameterType.InventoryItem)
				{
					_parameter.intValue = intValue;
				}
				else if (_parameter.parameterType == ParameterType.Float)
				{
					_parameter.floatValue = floatValue;
				}
				else if (_parameter.parameterType == ParameterType.String)
				{
					_parameter.stringValue = stringValue;
				}
				else if (_parameter.parameterType == ParameterType.GameObject)
				{
					_parameter.gameObject = gameobjectValue;
					_parameter.intValue = gameObjectConstantID;
				}
				else if (_parameter.parameterType == ParameterType.UnityObject)
				{
					_parameter.objectValue = unityObjectValue;
				}
				else if (_parameter.parameterType == ParameterType.Vector3)
				{
					_parameter.vector3Value = vector3Value;
				}
			}
			else if (setParamMethod == SetParamMethod.Random)
			{
				if (_parameter.parameterType == ParameterType.Boolean)
				{
					_parameter.intValue = Random.Range (0, 2);
				}
				else if (_parameter.parameterType == ParameterType.Integer)
				{
					_parameter.intValue = Random.Range (intValue, intValueMax + 1);
				}
				else if (_parameter.parameterType == ParameterType.Float)
				{
					_parameter.floatValue = Random.Range (floatValue, floatValueMax);
				}
				else
				{
					ACDebug.LogWarning ("Parameters of type '" + _parameter.parameterType + "' cannot be set randomly.");
				}
			}
			else if (setParamMethod == SetParamMethod.CopiedFromParameter)
			{
				if (_parameterToCopy == null)
				{
					ACDebug.LogWarning ("Cannot copy parameter value since it cannot be found!");
					return 0f;
				}

				_parameter.CopyValues (_parameterToCopy);
			}

			return 0f;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			changeOwn = EditorGUILayout.Toggle ("Change own?", changeOwn);
			if (changeOwn)
			{
				if (parameters == null || parameters.Count == 0)
				{
					EditorGUILayout.HelpBox ("This ActionList has no parameters defined!", MessageType.Warning);
				}

				parameterID = Action.ChooseParameterGUI (parameters, parameterID);
				SetParamGUI (parameters);
			}
			else
			{
				actionListSource = (ActionListSource) EditorGUILayout.EnumPopup ("Source:", actionListSource);
				if (actionListSource == ActionListSource.InScene)
				{
					actionList = (ActionList) EditorGUILayout.ObjectField ("ActionList:", actionList, typeof (ActionList), true);
					
					actionListConstantID = FieldToID <ActionList> (actionList, actionListConstantID);
					actionList = IDToField <ActionList> (actionList, actionListConstantID, true);

					if (actionList != null)
					{
						if (actionList.source == ActionListSource.InScene)
						{
							if (actionList.useParameters && actionList.parameters.Count > 0)
							{
								parameterID = Action.ChooseParameterGUI (actionList.parameters, parameterID);
								SetParamGUI (actionList.parameters, parameters);
							}
							else
							{
								EditorGUILayout.HelpBox ("This ActionList has no parameters defined!", MessageType.Warning);
							}
						}
						else if (actionList.source == ActionListSource.AssetFile && actionList.assetFile != null)
						{
							if (actionList.assetFile.useParameters && actionList.assetFile.parameters.Count > 0)
							{
								parameterID = Action.ChooseParameterGUI (actionList.assetFile.parameters, parameterID);
								SetParamGUI (actionList.assetFile.parameters, parameters, true);
							}
							else
							{
								EditorGUILayout.HelpBox ("This ActionList has no parameters defined!", MessageType.Warning);
							}
						}
						else
						{
							EditorGUILayout.HelpBox ("This ActionList has no parameters defined!", MessageType.Warning);
						}
					}
				}
				else if (actionListSource == ActionListSource.AssetFile)
				{
					actionListAsset = (ActionListAsset) EditorGUILayout.ObjectField ("ActionList asset:", actionListAsset, typeof (ActionListAsset), true);

					if (actionListAsset != null)
					{
						if (actionListAsset.useParameters && actionListAsset.parameters.Count > 0)
						{
							parameterID = Action.ChooseParameterGUI (actionListAsset.parameters, parameterID);
							SetParamGUI (actionListAsset.parameters, parameters, true);
						}
						else
						{
							EditorGUILayout.HelpBox ("This ActionList Asset has no parameters defined!", MessageType.Warning);
						}
					}
				}
			}

			AfterRunningOption ();
		}
		
		
		private void SetParamGUI (List<ActionParameter> parameters, List<ActionParameter> ownParameters = null, bool forceConstantIDs = false)
		{
			if (parameters == null || parameters.Count == 0)
			{
				parameterLabel = "";
				return;
			}

			_parameter = GetParameterWithID (parameters, parameterID);

			if (_parameter == null)
			{
				parameterLabel = "";
				return;
			}

			parameterLabel = _parameter.label;

			setParamMethod = (SetParamMethod) EditorGUILayout.EnumPopup ("New value is:", setParamMethod);

			if (setParamMethod == SetParamMethod.EnteredHere)
			{
				if (ownParameters != null && ownParameters.Count > 0)
				{
					ownParamID = Action.ChooseParameterGUI ("Set as:", ownParameters, ownParamID, _parameter.parameterType);
					if (ownParamID >= 0)
					{
						return;
					}
				}

				if (_parameter.parameterType == ParameterType.Boolean)
				{
					bool boolValue = (intValue == 1) ? true : false;
					boolValue = EditorGUILayout.Toggle ("Set as:", boolValue);
					intValue = (boolValue) ? 1 : 0;
				}
				else if (_parameter.parameterType == ParameterType.Integer)
				{
					intValue = EditorGUILayout.IntField ("Set as:", intValue);
				}
				else if (_parameter.parameterType == ParameterType.Float)
				{
					floatValue = EditorGUILayout.FloatField ("Set as:", floatValue);
				}
				else if (_parameter.parameterType == ParameterType.String)
				{
					stringValue = EditorGUILayout.TextField ("Set as:", stringValue);
				}
				else if (_parameter.parameterType == ParameterType.GameObject)
				{
					gameobjectValue = (GameObject) EditorGUILayout.ObjectField ("Set to:", gameobjectValue, typeof (GameObject), true);

					gameObjectConstantID = FieldToID (gameobjectValue, gameObjectConstantID, forceConstantIDs);
					gameobjectValue = IDToField (gameobjectValue, gameObjectConstantID, false, forceConstantIDs);
				}
				else if (_parameter.parameterType == ParameterType.GlobalVariable)
				{
					if (AdvGame.GetReferences ().variablesManager == null || AdvGame.GetReferences ().variablesManager.vars == null || AdvGame.GetReferences ().variablesManager.vars.Count == 0)
					{
						EditorGUILayout.HelpBox ("No Global variables exist!", MessageType.Info);
					}
					else
					{
						intValue = ShowVarSelectorGUI (AdvGame.GetReferences ().variablesManager.vars, intValue);
					}
				}
				else if (_parameter.parameterType == ParameterType.UnityObject)
				{
					unityObjectValue = (Object) EditorGUILayout.ObjectField ("Set to:", unityObjectValue, typeof (Object), true);
				}
				else if (_parameter.parameterType == ParameterType.InventoryItem)
				{
					intValue = ShowInvSelectorGUI (intValue);
				}
				else if (_parameter.parameterType == ParameterType.LocalVariable)
				{
					if (isAssetFile)
					{
						EditorGUILayout.HelpBox ("Cannot access local variables from an asset file.", MessageType.Warning);
					}
					else if (KickStarter.localVariables == null || KickStarter.localVariables.localVars == null || KickStarter.localVariables.localVars.Count == 0)
					{
						EditorGUILayout.HelpBox ("No Local variables exist!", MessageType.Info);
					}
					else
					{
						intValue = ShowVarSelectorGUI (KickStarter.localVariables.localVars, intValue);
					}
				}
				else if (_parameter.parameterType == ParameterType.Vector3)
				{
					vector3Value = EditorGUILayout.Vector3Field ("Set as:", vector3Value);
				}
			}
			else if (setParamMethod == SetParamMethod.Random)
			{
				if (_parameter.parameterType == ParameterType.Boolean)
				{}
				else if (_parameter.parameterType == ParameterType.Integer)
				{
					intValue = EditorGUILayout.IntField ("Minimum:", intValue);
					intValueMax = EditorGUILayout.IntField ("Maximum:", intValueMax);
					if (intValueMax < intValue) intValueMax = intValue;
				}
				else if (_parameter.parameterType == ParameterType.Float)
				{
					floatValue = EditorGUILayout.FloatField ("Minimum:", floatValue);
					floatValueMax = EditorGUILayout.FloatField ("Maximum:", floatValueMax);
					if (floatValueMax < floatValue) floatValueMax = floatValue;
				}
				else
				{
					EditorGUILayout.HelpBox ("Parameters of type '" + _parameter.parameterType + "' cannot be set randomly.", MessageType.Warning);
				}
			}
			else if (setParamMethod == SetParamMethod.CopiedFromGlobalVariable)
			{
				if (AdvGame.GetReferences () != null && AdvGame.GetReferences ().variablesManager != null && AdvGame.GetReferences ().variablesManager.vars != null && AdvGame.GetReferences ().variablesManager.vars.Count > 0)
				{
					if (_parameter.parameterType == ParameterType.Vector3)
					{
						globalVariableID = AdvGame.GlobalVariableGUI ("Vector3 variable:", globalVariableID, VariableType.Vector3);
					}
					else if (_parameter.parameterType == ParameterType.GameObject || _parameter.parameterType == ParameterType.GlobalVariable || _parameter.parameterType == ParameterType.InventoryItem || _parameter.parameterType == ParameterType.LocalVariable || _parameter.parameterType == ParameterType.UnityObject)
					{
						EditorGUILayout.HelpBox ("Parameters of type '" + _parameter.parameterType + "' cannot have values transferred from Global Variables.", MessageType.Warning);
					}
					else
					{
						globalVariableID = AdvGame.GlobalVariableGUI ("Variable:", globalVariableID);
					}
				}
				else
				{
					EditorGUILayout.HelpBox ("No Global Variables found!", MessageType.Warning);
				}
			}
			else if (setParamMethod == SetParamMethod.CopiedFromParameter)
			{
				if (changeOwn)
				{
					parameterToCopyID = Action.ChooseParameterGUI (parameters, parameterToCopyID);
				}
				else
				{
					if (actionListSource == ActionListSource.InScene && actionList != null)
					{
						if (actionList.source == ActionListSource.InScene)
						{
							if (actionList.useParameters && actionList.parameters.Count > 0)
							{
								parameterToCopyID = Action.ChooseParameterGUI (actionList.parameters, parameterToCopyID);
							}
						}
						else if (actionList.source == ActionListSource.AssetFile && actionList.assetFile != null)
						{
							if (actionList.assetFile.useParameters && actionList.assetFile.parameters.Count > 0)
							{
								parameterToCopyID = Action.ChooseParameterGUI (actionList.assetFile.parameters, parameterToCopyID);
							}
						}
					}
					else if (actionListSource == ActionListSource.AssetFile && actionListAsset != null)
					{
						if (actionListAsset.useParameters && actionListAsset.parameters.Count > 0)
						{
							parameterToCopyID = Action.ChooseParameterGUI (actionListAsset.parameters, parameterToCopyID);
						}
					}
				}
			}
		}
		
		
		override public void AssignConstantIDs (bool saveScriptsToo)
		{
			AssignConstantID (gameobjectValue, gameObjectConstantID, 0);
		}
		
		
		override public string SetLabel ()
		{
			if (parameterLabel != "")
			{
				return (" (" + parameterLabel + ")");
			}
			return "";
		}


		private int ShowVarSelectorGUI (List<GVar> vars, int ID)
		{
			int variableNumber = -1;
			
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
			
			variableNumber = EditorGUILayout.Popup ("Variable:", variableNumber, labelList.ToArray());
			ID = vars[variableNumber].id;
			
			return ID;
		}
		
		
		private int ShowInvSelectorGUI (int ID)
		{
			InventoryManager inventoryManager = AdvGame.GetReferences ().inventoryManager;
			if (inventoryManager == null)
			{
				return ID;
			}
			
			int invNumber = -1;
			List<string> labelList = new List<string>();
			int i=0;
			foreach (InvItem _item in inventoryManager.items)
			{
				labelList.Add (_item.label);
				
				// If an item has been removed, make sure selected variable is still valid
				if (_item.id == ID)
				{
					invNumber = i;
				}
				
				i++;
			}
			
			if (invNumber == -1)
			{
				// Wasn't found (item was possibly deleted), so revert to zero
				ACDebug.LogWarning ("Previously chosen item no longer exists!");
				
				invNumber = 0;
				ID = 0;
			}
			
			invNumber = EditorGUILayout.Popup ("Inventory item:", invNumber, labelList.ToArray());
			ID = inventoryManager.items[invNumber].id;
			
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


		public override int GetVariableReferences (List<ActionParameter> parameters, VariableLocation location, int varID)
		{
			int thisCount = 0;
			if (setParamMethod == SetParamMethod.CopiedFromGlobalVariable && location == VariableLocation.Global && globalVariableID == varID)
			{
				thisCount ++;
			}

			if (setParamMethod == SetParamMethod.EnteredHere)
			{
				ActionParameter _param = null;

				if (changeOwn && parameters != null)
				{
					_param = GetParameterWithID (parameters, parameterID);
				}
				else
				{
					if (actionListSource == ActionListSource.InScene && actionList != null)
					{
						if (actionList.source == ActionListSource.InScene && actionList.useParameters)
						{
							_param = GetParameterWithID (actionList.parameters, parameterID);
						}
						else if (actionList.source == ActionListSource.AssetFile && actionList.assetFile != null && actionList.assetFile.useParameters)
						{
							_param = GetParameterWithID (actionList.assetFile.parameters, parameterID);
						}
					}
					else if (actionListSource == ActionListSource.AssetFile && actionListAsset != null && actionListAsset.useParameters)
					{
						_param = GetParameterWithID (actionListAsset.parameters, parameterID);
					}
				}

				if (_param != null && _param.parameterType == ParameterType.LocalVariable && location == VariableLocation.Local && varID == intValue)
				{
					thisCount ++;
				}
				else if (_param != null && _param.parameterType == ParameterType.GlobalVariable && location == VariableLocation.Global && varID == intValue)
				{
					thisCount ++;
				}
			}

			thisCount += base.GetVariableReferences (parameters, location, varID);
			return thisCount;
		}


		public override int GetInventoryReferences (List<ActionParameter> parameters, int _invID)
		{
			if (setParamMethod == SetParamMethod.EnteredHere)
			{
				ActionParameter _param = null;

				if (changeOwn && parameters != null)
				{
					_param = GetParameterWithID (parameters, parameterID);
				}
				else
				{
					if (actionListSource == ActionListSource.InScene && actionList != null)
					{
						if (actionList.source == ActionListSource.InScene && actionList.useParameters)
						{
							_param = GetParameterWithID (actionList.parameters, parameterID);
						}
						else if (actionList.source == ActionListSource.AssetFile && actionList.assetFile != null && actionList.assetFile.useParameters)
						{
							_param = GetParameterWithID (actionList.assetFile.parameters, parameterID);
						}
					}
					else if (actionListSource == ActionListSource.AssetFile && actionListAsset != null && actionListAsset.useParameters)
					{
						_param = GetParameterWithID (actionListAsset.parameters, parameterID);
					}
				}

				if (_param != null && _param.parameterType == ParameterType.InventoryItem && _invID == intValue)
				{
					return 1;
				}
			}

			return 0;
		}

		#endif
		
	}
	
}