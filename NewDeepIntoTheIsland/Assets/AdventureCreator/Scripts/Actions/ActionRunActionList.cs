/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionRunActionList.cs"
 * 
 *	This Action runs other ActionLists
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
	public class ActionRunActionList : Action
	{
		
		public enum ListSource { InScene, AssetFile };
		public ListSource listSource = ListSource.InScene;

		public ActionList actionList;
		public int constantID = 0;
		public int parameterID = -1;

		public ActionListAsset invActionList;
		public int assetParameterID = -1;

		public bool runFromStart = true;
		public int jumpToAction;
		public int jumpToActionParameterID = -1;
		public AC.Action jumpToActionActual;
		public bool runInParallel = false; // No longer visible, but needed for legacy upgrades

		public bool isSkippable = false; // Important: Set by ActionList, to determine whether or not the ActionList it runs should be added to the skip queue

		public List<ActionParameter> localParameters = new List<ActionParameter>();
		public List<int> parameterIDs = new List<int>();

		public bool setParameters = true;

		private RuntimeActionList runtimeActionList;


		public ActionRunActionList ()
		{
			this.isDisplayed = true;
			category = ActionCategory.ActionList;
			title = "Run";
			description = "Runs any ActionList (either scene-based like Cutscenes, Triggers and Interactions, or ActionList assets). If the new ActionList takes parameters, this Action can be used to set them.";
		}


		private void Upgrade ()
		{
			if (!runInParallel)
			{
				numSockets = 1;
				runInParallel = true;
				endAction = ResultAction.Stop;
			}
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			if (listSource == ListSource.InScene)
			{
				actionList = AssignFile <ActionList> (parameters, parameterID, constantID, actionList);
				jumpToAction = AssignInteger (parameters, jumpToActionParameterID, jumpToAction);
			}
			else if (listSource == ListSource.AssetFile)
			{
				invActionList = (ActionListAsset) AssignObject <ActionListAsset> (parameters, assetParameterID, invActionList);
			}

			if (localParameters != null && localParameters.Count > 0)
			{
				for (int i=0; i<localParameters.Count; i++)
				{
					if (parameterIDs != null && parameterIDs.Count > i && parameterIDs[i] >= 0)
					{
						int ID = parameterIDs[i];
						foreach (ActionParameter parameter in parameters)
						{
							if (parameter.ID == ID)
							{
								localParameters[i].CopyValues (parameter);
								break;
							}
						}
					}
				}
			}
		}


		override public float Run ()
		{
			if (!isRunning)
			{
				Upgrade ();

				isRunning = true;
				runtimeActionList = null;

				if (listSource == ListSource.InScene && actionList != null && !actionList.actions.Contains (this))
				{
					KickStarter.actionListManager.EndList (actionList);

					if (actionList.source == ActionListSource.AssetFile && actionList.assetFile != null && actionList.assetFile.useParameters)
					{
						if (actionList.syncParamValues)
						{
							SendParameters (actionList.assetFile.parameters, true);
						}
						else
						{
							SendParameters (actionList.parameters, false);
						}
					}
					else if (actionList.source == ActionListSource.InScene && actionList.useParameters)
					{
						SendParameters (actionList.parameters, false);
					}

					if (runFromStart)
					{
						actionList.Interact (0, !isSkippable);
					}
					else
					{
						actionList.Interact (GetSkipIndex (actionList.actions), !isSkippable);
					}
				}
				else if (listSource == ListSource.AssetFile && invActionList != null && !invActionList.actions.Contains (this))
				{
					if (!invActionList.canRunMultipleInstances)
					{
						KickStarter.actionListAssetManager.EndAssetList (invActionList);
					}

					if (invActionList.useParameters)
					{
						SendParameters (invActionList.parameters, true);
					}

					if (runFromStart)
					{
						runtimeActionList = AdvGame.RunActionListAsset (invActionList, 0, !isSkippable);
					}
					else
					{
						runtimeActionList = AdvGame.RunActionListAsset (invActionList, GetSkipIndex (invActionList.actions), !isSkippable);
					}
				}

				if (!runInParallel || (runInParallel && willWait))
				{
					return defaultPauseTime;
				}
			}
			else
			{
				if (listSource == ListSource.InScene && actionList != null)
				{
					if (KickStarter.actionListManager.IsListRunning (actionList))
					{
						return defaultPauseTime;
					}
					else
					{
						isRunning = false;
					}
				}
				else if (listSource == ListSource.AssetFile && invActionList != null)
				{
					if (invActionList.canRunMultipleInstances)
					{
						if (runtimeActionList != null && KickStarter.actionListManager.IsListRunning (runtimeActionList))
						{
							return defaultPauseTime;
						}
						isRunning = false;
					}
					else
					{
						if (KickStarter.actionListAssetManager.IsListRunning (invActionList))
						{
							return defaultPauseTime;
						}
						isRunning = false;
					}
				}
			}

			return 0f;
		}


		override public void Skip ()
		{
			if (listSource == ListSource.InScene && actionList != null)
			{
				if (actionList.source == ActionListSource.AssetFile && actionList.assetFile != null && actionList.assetFile.useParameters)
				{
					if (actionList.syncParamValues)
					{
						SendParameters (actionList.assetFile.parameters, true);
					}
					else
					{
						SendParameters (actionList.parameters, false);
					}
				}
				else if (actionList.source == ActionListSource.InScene && actionList.useParameters)
				{
					SendParameters (actionList.parameters, false);
				}

				if (runFromStart)
				{
					actionList.Skip ();
				}
				else
				{
					actionList.Skip (GetSkipIndex (actionList.actions));
				}
			}
			else if (listSource == ListSource.AssetFile && invActionList != null)
			{
				if (invActionList.useParameters)
				{
					SendParameters (invActionList.parameters, true);
				}

				if (runFromStart)
				{
					AdvGame.SkipActionListAsset (invActionList);
				}
				else
				{
					AdvGame.SkipActionListAsset (invActionList, GetSkipIndex (invActionList.actions));
				}
			}
		}


		private int GetSkipIndex (List<Action> _actions)
		{
			int skip = jumpToAction;
			if (jumpToActionActual && _actions.IndexOf (jumpToActionActual) > 0)
			{
				skip = _actions.IndexOf (jumpToActionActual);
			}
			return skip;
		}


		private void SendParameters (List<ActionParameter> externalParameters, bool sendingToAsset)
		{
			if (!setParameters)
			{
				return;
			}

			SyncLists (externalParameters, localParameters);

			for (int i=0; i<externalParameters.Count; i++)
			{
				if (localParameters.Count > i)
				{
					if (externalParameters[i].parameterType == ParameterType.String)
					{
						externalParameters[i].SetValue (localParameters[i].stringValue);
					}
					else if (externalParameters[i].parameterType == ParameterType.Float)
					{
						externalParameters[i].SetValue (localParameters[i].floatValue);
					}
					else if (externalParameters[i].parameterType == ParameterType.UnityObject)
					{
						externalParameters[i].SetValue (localParameters[i].objectValue);
					}
					else if (externalParameters[i].parameterType == ParameterType.Vector3)
					{
						externalParameters[i].SetValue (localParameters[i].vector3Value);
					}
					else if (externalParameters[i].parameterType != ParameterType.GameObject)
					{
						externalParameters[i].SetValue (localParameters[i].intValue);
					}
					else
					{
						if (sendingToAsset)
						{
							if (isAssetFile)
							{
								if (localParameters[i].gameObject != null)
								{
									externalParameters[i].SetValue (localParameters[i].gameObject);
								}
								else
								{
									externalParameters[i].SetValue (localParameters[i].intValue);
								}
							}
							else if (localParameters[i].gameObject != null)
							{
								int idToSend = 0;
								if (localParameters[i].gameObject && localParameters[i].gameObject.GetComponent <ConstantID>())
								{
									idToSend = localParameters[i].gameObject.GetComponent <ConstantID>().constantID;
								}
								else
								{
									ACDebug.LogWarning (localParameters[i].gameObject.name + " requires a ConstantID script component!");
								}
								externalParameters[i].SetValue (localParameters[i].gameObject, idToSend);
							}
							else
							{
								externalParameters[i].SetValue (localParameters[i].intValue);
							}
						}
						else if (localParameters[i].gameObject != null)
						{
							externalParameters[i].SetValue (localParameters[i].gameObject);
						}
						else
						{
							externalParameters[i].SetValue (localParameters[i].intValue);
						}
					}
				}
			}
		}


		#if UNITY_EDITOR
				
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			listSource = (ListSource) EditorGUILayout.EnumPopup ("Source:", listSource);
			if (listSource == ListSource.InScene)
			{
				parameterID = Action.ChooseParameterGUI ("ActionList:", parameters, parameterID, ParameterType.GameObject);
				if (parameterID >= 0)
				{
					localParameters.Clear ();
					constantID = 0;
					actionList = null;
				}
				else
				{
					actionList = (ActionList) EditorGUILayout.ObjectField ("ActionList:", actionList, typeof (ActionList), true);
					
					constantID = FieldToID <ActionList> (actionList, constantID);
					actionList = IDToField <ActionList> (actionList, constantID, true);

					if (actionList != null)
					{
						if (actionList.actions.Contains (this))
						{
							EditorGUILayout.HelpBox ("This Action cannot be used to run the ActionList it is in - use the Skip option below instead.", MessageType.Warning);
						}
						else if (actionList.source == ActionListSource.AssetFile && actionList.assetFile != null && actionList.assetFile.useParameters && actionList.assetFile.parameters.Count > 0)
						{
							SetParametersGUI (actionList.assetFile.parameters, parameters);
						}
						else if (actionList.source == ActionListSource.InScene && actionList.useParameters && actionList.parameters.Count > 0)
						{
							SetParametersGUI (actionList.parameters, parameters);
						}
					}
				}


				runFromStart = EditorGUILayout.Toggle ("Run from start?", runFromStart);

				if (!runFromStart)
				{
					jumpToActionParameterID = Action.ChooseParameterGUI ("Action # to skip to:", parameters, jumpToActionParameterID, ParameterType.Integer);
					if (jumpToActionParameterID == -1 && actionList != null && actionList.actions.Count > 1)
					{
						JumpToActionGUI (actionList.actions);
					}
				}
			}
			else if (listSource == ListSource.AssetFile)
			{
				assetParameterID = Action.ChooseParameterGUI ("ActionList asset:", parameters, assetParameterID, ParameterType.UnityObject);
				if (assetParameterID < 0)
				{
					invActionList = (ActionListAsset) EditorGUILayout.ObjectField ("ActionList asset:", invActionList, typeof (ActionListAsset), true);
				}

				if (invActionList != null)
				{
					if (invActionList.actions.Contains (this))
					{
						EditorGUILayout.HelpBox ("This Action cannot be used to run the ActionList it is in - use the Skip option below instead.", MessageType.Warning);
					}
					else if (invActionList.useParameters && invActionList.parameters.Count > 0)
					{
						SetParametersGUI (invActionList.parameters, parameters);
					}
				}

				runFromStart = EditorGUILayout.Toggle ("Run from start?", runFromStart);
				
				if (!runFromStart && invActionList != null && invActionList.actions.Count > 1)
				{
					JumpToActionGUI (invActionList.actions);
				}
			}

			if (!runInParallel)
			{
				Upgrade ();
			}

			willWait = EditorGUILayout.Toggle ("Wait until finish?", willWait);
			AfterRunningOption ();
		}


		private void JumpToActionGUI (List<Action> actions)
		{
			int tempSkipAction = jumpToAction;
			List<string> labelList = new List<string>();
			
			if (jumpToActionActual)
			{
				bool found = false;
				
				for (int i = 0; i < actions.Count; i++)
				{
					labelList.Add (i.ToString () + ": " + actions [i].title);
					
					if (jumpToActionActual == actions [i])
					{
						jumpToAction = i;
						found = true;
					}
				}

				if (!found)
				{
					jumpToAction = tempSkipAction;
				}
			}
			
			if (jumpToAction < 0)
			{
				jumpToAction = 0;
			}
			
			if (jumpToAction >= actions.Count)
			{
				jumpToAction = actions.Count - 1;
			}
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField ("  Action to skip to:");
			tempSkipAction = EditorGUILayout.Popup (jumpToAction, labelList.ToArray());
			jumpToAction = tempSkipAction;
			EditorGUILayout.EndHorizontal();
			jumpToActionActual = actions [jumpToAction];
		}


		public static int ShowVarSelectorGUI (string label, List<GVar> vars, int ID)
		{
			int variableNumber = -1;
			
			List<string> labelList = new List<string>();
			labelList.Add (" (None)");
			foreach (GVar _var in vars)
			{
				labelList.Add (_var.label);
			}
			
			variableNumber = GetVarNumber (vars, ID) + 1;
			variableNumber = EditorGUILayout.Popup (label, variableNumber, labelList.ToArray()) - 1;

			if (variableNumber >= 0)
			{
				return vars[variableNumber].id;
			}

			return -1;
		}


		public static int ShowInvItemSelectorGUI (string label, List<InvItem> items, int ID)
		{
			int invNumber = -1;
			
			List<string> labelList = new List<string>();
			labelList.Add (" (None)");
			foreach (InvItem _item in items)
			{
				labelList.Add (_item.label);
			}
			
			invNumber = GetInvNumber (items, ID) + 1;
			invNumber = EditorGUILayout.Popup (label, invNumber, labelList.ToArray()) - 1;

			if (invNumber >= 0)
			{
				return items[invNumber].id;
			}
			return -1;
		}


		private static int GetVarNumber (List<GVar> vars, int ID)
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


		private static int GetInvNumber (List<InvItem> items, int ID)
		{
			int i = 0;
			foreach (InvItem _item in items)
			{
				if (_item.id == ID)
				{
					return i;
				}
				i++;
			}
			return -1;
		}


		private void SetParametersGUI (List<ActionParameter> externalParameters, List<ActionParameter> ownParameters)
		{
			setParameters = EditorGUILayout.Toggle ("Set parameters?", setParameters);
			if (!setParameters)
			{
				return;
			}

			SyncLists (externalParameters, localParameters);

			EditorGUILayout.BeginVertical ("Button");
			for (int i=0; i<externalParameters.Count; i++)
			{
				string label = externalParameters[i].label;
				int linkedID = parameterIDs[i];

				localParameters[i].parameterType = externalParameters[i].parameterType;
				if (externalParameters[i].parameterType == ParameterType.GameObject)
				{
					linkedID = Action.ChooseParameterGUI (label + ":", ownParameters, linkedID, ParameterType.GameObject);
					if (linkedID < 0)
					{
						if (isAssetFile)
						{
							localParameters[i].gameObject = (GameObject) EditorGUILayout.ObjectField (label + ":", localParameters[i].gameObject, typeof (GameObject), true);
							if (localParameters[i].gameObject != null)
							{
								if (PrefabUtility.GetPrefabType (localParameters[i].gameObject) != PrefabType.Prefab)
								{
									localParameters[i].intValue = FieldToID (localParameters[i].gameObject, localParameters[i].intValue);
									localParameters[i].gameObject = IDToField (localParameters[i].gameObject, localParameters[i].intValue, true);
								}
							}
							else
							{
								localParameters[i].intValue = EditorGUILayout.IntField (label + " (ID #):", localParameters[i].intValue);
							}
						}
						else
						{
							// Gameobject
							localParameters[i].gameObject = (GameObject) EditorGUILayout.ObjectField (label + ":", localParameters[i].gameObject, typeof (GameObject), true);
							localParameters[i].intValue = 0;
							if (localParameters[i].gameObject != null && localParameters[i].gameObject.GetComponent <ConstantID>() == null)
							{
								localParameters[i].gameObject.AddComponent <ConstantID>();
							}
						}
					}	
				}
				else if (externalParameters[i].parameterType == ParameterType.UnityObject)
				{
					linkedID = Action.ChooseParameterGUI (label + ":", ownParameters, linkedID, ParameterType.UnityObject);
					if (linkedID < 0)
					{
						localParameters[i].objectValue = (Object) EditorGUILayout.ObjectField (label + ":", localParameters[i].objectValue, typeof (Object), true);
					}	
				}
				else if (externalParameters[i].parameterType == ParameterType.GlobalVariable)
				{
					if (AdvGame.GetReferences () && AdvGame.GetReferences ().variablesManager)
					{
						linkedID = Action.ChooseParameterGUI (label + ":", ownParameters, linkedID, ParameterType.GlobalVariable);
						if (linkedID < 0)
						{
							VariablesManager variablesManager = AdvGame.GetReferences ().variablesManager;
							localParameters[i].intValue = ShowVarSelectorGUI (label + ":", variablesManager.vars, localParameters[i].intValue);
						}	
					}
					else
					{
						EditorGUILayout.HelpBox ("A Variables Manager is required to pass Global Variables.", MessageType.Warning);
					}
				}
				else if (externalParameters[i].parameterType == ParameterType.InventoryItem)
				{
					if (AdvGame.GetReferences () && AdvGame.GetReferences ().inventoryManager)
					{
						linkedID = Action.ChooseParameterGUI (label + ":", ownParameters, linkedID, ParameterType.InventoryItem);
						if (linkedID < 0)
						{
							InventoryManager inventoryManager = AdvGame.GetReferences ().inventoryManager;
							localParameters[i].intValue = ShowInvItemSelectorGUI (label + ":", inventoryManager.items, localParameters[i].intValue);
						}
					}
					else
					{
						EditorGUILayout.HelpBox ("An Inventory Manager is required to pass Inventory items.", MessageType.Warning);
					}
				}
				else if (externalParameters[i].parameterType == ParameterType.LocalVariable)
				{
					if (KickStarter.localVariables)
					{
						linkedID = Action.ChooseParameterGUI (label + ":", ownParameters, linkedID, ParameterType.LocalVariable);
						if (linkedID < 0)
						{
							localParameters[i].intValue = ShowVarSelectorGUI (label + ":", KickStarter.localVariables.localVars, localParameters[i].intValue);
						}
					}
					else
					{
						EditorGUILayout.HelpBox ("A GameEngine prefab is required to pass Local Variables.", MessageType.Warning);
					}
				}
				else if (externalParameters[i].parameterType == ParameterType.String)
				{
					linkedID = Action.ChooseParameterGUI (label + ":", ownParameters, linkedID, ParameterType.String);
					if (linkedID < 0)
					{
						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField (label + ":", GUILayout.Width (145f));
						EditorStyles.textField.wordWrap = true;
						localParameters[i].stringValue = EditorGUILayout.TextArea (localParameters[i].stringValue, GUILayout.MaxWidth (400f));
						EditorGUILayout.EndHorizontal ();
					}
				}
				else if (externalParameters[i].parameterType == ParameterType.Float)
				{
					linkedID = Action.ChooseParameterGUI (label + ":", ownParameters, linkedID, ParameterType.Float);
					if (linkedID < 0)
					{
						localParameters[i].floatValue = EditorGUILayout.FloatField (label + ":", localParameters[i].floatValue);
					}
				}
				else if (externalParameters[i].parameterType == ParameterType.Integer)
				{
					linkedID = Action.ChooseParameterGUI (label + ":", ownParameters, linkedID, ParameterType.Integer);
					if (linkedID < 0)
					{
						localParameters[i].intValue = EditorGUILayout.IntField (label + ":", localParameters[i].intValue);
					}
				}
				else if (externalParameters[i].parameterType == ParameterType.Vector3)
				{
					linkedID = Action.ChooseParameterGUI (label + ":", ownParameters, linkedID, ParameterType.Vector3);
					if (linkedID < 0)
					{
						localParameters[i].vector3Value = EditorGUILayout.Vector3Field (label + ":", localParameters[i].vector3Value);
					}
				}
				else if (externalParameters[i].parameterType == ParameterType.Boolean)
				{
					linkedID = Action.ChooseParameterGUI (label + ":", ownParameters, linkedID, ParameterType.Boolean);
					if (linkedID < 0)
					{
						BoolValue boolValue = BoolValue.False;
						if (localParameters[i].intValue == 1)
						{
							boolValue = BoolValue.True;
						}

						boolValue = (BoolValue) EditorGUILayout.EnumPopup (label + ":", boolValue);

						if (boolValue == BoolValue.True)
						{
							localParameters[i].intValue = 1;
						}
						else
						{
							localParameters[i].intValue = 0;
						}
					}
				}

				parameterIDs[i] = linkedID;
			}
			EditorGUILayout.EndVertical ();
		}


		override public void AssignConstantIDs (bool saveScriptsToo)
		{
			AssignConstantID <ActionList> (actionList, constantID, parameterID);
		}


		public override string SetLabel ()
		{
			string labelAdd = "";
			
			if (listSource == ListSource.InScene && actionList != null)
			{
				labelAdd += " (" + actionList.name + ")";
			}
			else if (listSource == ListSource.AssetFile && invActionList != null)
			{
				labelAdd += " (" + invActionList.name + ")";
			}
			
			return labelAdd;
		}


		public override int GetVariableReferences (List<ActionParameter> parameters, VariableLocation location, int varID)
		{
			int thisCount = 0;

			if (listSource == ListSource.InScene && actionList != null)
			{
				if (actionList.source == ActionListSource.InScene && actionList.useParameters)
				{
					SyncLists (actionList.parameters, localParameters);
				}
				else if (actionList.source == ActionListSource.AssetFile && actionList.assetFile != null && actionList.assetFile.useParameters)
				{
					SyncLists (actionList.assetFile.parameters, localParameters);
				}
			}
			else if (listSource == ListSource.AssetFile && invActionList != null && invActionList.useParameters)
			{
				SyncLists (invActionList.parameters, localParameters);
			}

			foreach (ActionParameter localParameter in localParameters)
			{
				if (localParameter != null && localParameter.parameterType == ParameterType.LocalVariable && location == VariableLocation.Local && varID == localParameter.intValue)
				{
					thisCount ++;
				}
				else if (localParameter != null && localParameter.parameterType == ParameterType.GlobalVariable && location == VariableLocation.Global && varID == localParameter.intValue)
				{
					thisCount ++;
				}
			}

			thisCount += base.GetVariableReferences (parameters, location, varID);
			return thisCount;
		}


		public override int GetInventoryReferences (List<ActionParameter> parameters, int _invID)
		{
			int thisCount = 0;

			if (listSource == ListSource.InScene && actionList != null)
			{
				if (actionList.source == ActionListSource.InScene && actionList.useParameters)
				{
					SyncLists (actionList.parameters, localParameters);
				}
				else if (actionList.source == ActionListSource.AssetFile && actionList.assetFile != null && actionList.assetFile.useParameters)
				{
					SyncLists (actionList.assetFile.parameters, localParameters);
				}
			}
			else if (listSource == ListSource.AssetFile && invActionList != null && invActionList.useParameters)
			{
				SyncLists (invActionList.parameters, localParameters);
			}

			foreach (ActionParameter localParameter in localParameters)
			{
				if (localParameter != null && localParameter.parameterType == ParameterType.InventoryItem && _invID == localParameter.intValue)
				{
					thisCount ++;
				}
			}

			return thisCount;
		}
		
		#endif


		[SerializeField] private bool hasUpgradedAgain = false;
		private void SyncLists (List<ActionParameter> externalParameters, List<ActionParameter> oldLocalParameters)
		{
			if (!hasUpgradedAgain)
			{
				// If parameters were deleted before upgrading, there may be a mismatch - so first ensure that order internal IDs match external

				if (oldLocalParameters != null && externalParameters != null && oldLocalParameters.Count != externalParameters.Count && oldLocalParameters.Count > 0)
				{
					ACDebug.LogWarning ("Parameter mismatch detected - please check the 'ActionList: Run' Action for its parameter values.");
				}

				for (int i=0; i<externalParameters.Count; i++)
				{
					if (i < oldLocalParameters.Count)
					{
						oldLocalParameters[i].ID = externalParameters[i].ID;
					}
				}

				hasUpgradedAgain = true;
			}

			// Now that all parameter IDs match to begin with, we can rebuild the internal record based on the external parameters

			localParameters = new List<ActionParameter>();
			List<int> newParameterIDs = new List<int>();

			foreach (ActionParameter externalParameter in externalParameters)
			{
				bool foundMatch = false;

				for (int i=0; i<oldLocalParameters.Count; i++)
				{
					if (!foundMatch && oldLocalParameters[i].ID == externalParameter.ID)
					{
						localParameters.Add (new ActionParameter (oldLocalParameters[i], true));
						oldLocalParameters.RemoveAt (i);

						if (parameterIDs != null && i < parameterIDs.Count)
						{
							newParameterIDs.Add (parameterIDs[i]);
							parameterIDs.RemoveAt (i);
						}
						else
						{
							newParameterIDs.Add (-1);
						}

						foundMatch = true;
					}
				}

				if (!foundMatch)
				{
					ActionParameter newParameter = new ActionParameter (externalParameter.ID);
					localParameters.Add (newParameter);

					newParameterIDs.Add (-1);
				}
			}

			parameterIDs = newParameterIDs;
		}

	}

}