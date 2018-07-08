/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"VariablesManager.cs"
 * 
 *	This script handles the "Variables" tab of the main wizard.
 *	Boolean and integer, which can be used regardless of scene, are defined here.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	/**
	 * Handles the "Variables" tab of the Game Editor window.
	 * All global variables are defined here. Local variables are also managed here, but they are stored within the LocalVariables component on the GameEngine prefab.
	 * When the game begins, global variables are transferred to the RuntimeVariables component on the PersistentEngine prefab.
	 */
	[System.Serializable]
	public class VariablesManager : ScriptableObject
	{

		/** A List of the game's global variables */
		public List<GVar> vars = new List<GVar>();
		/** A List of preset values that the variables can be bulk-assigned to */
		public List<VarPreset> varPresets = new List<VarPreset>();
		/** If True, then the Variables Manager GUI will show the live values of each variable, rather than their default values */
		public bool updateRuntime = true;

		
		#if UNITY_EDITOR

		private int chosenPresetID = 0;

		private GVar selectedVar;
		private int sideVar = -1;
		private VariableLocation sideVarLocation = VariableLocation.Global;
		private string[] boolType = {"False", "True"};
		private string filter = "";
		private enum VarFilter { ByName, ByDescription };
		private VarFilter varFilter;

		private Vector2 scrollPos;
		private bool showGlobalTab = true;
		private bool showLocalTab = false;

		private bool showSettings = true;
		private bool showPresets = true;
		private bool showVariablesList = true;
		private bool showVariablesProperties = true;


		/**
		 * Shows the GUI.
		 */
		public void ShowGUI ()
		{
			string sceneName = MultiSceneChecker.EditActiveScene ();
			if (sceneName != "")
			{
				EditorGUILayout.LabelField ("Editing scene: '" + sceneName + "'",  CustomStyles.subHeader);
				EditorGUILayout.Space ();
			}

			EditorGUILayout.Space ();
			GUILayout.BeginHorizontal ();

			string label = (vars.Count > 0) ? ("Global (" + vars.Count + ")") : "Global";
			if (GUILayout.Toggle (showGlobalTab, label, "toolbarbutton"))
			{
				SetTab (0);
			}

			label = (KickStarter.localVariables != null && KickStarter.localVariables.localVars.Count > 0) ? ("Local (" +  KickStarter.localVariables.localVars.Count + ")") : "Local";
			if (GUILayout.Toggle (showLocalTab, label, "toolbarbutton"))
			{
				SetTab (1);
			}

			GUILayout.EndHorizontal ();
			EditorGUILayout.Space ();

			EditorGUILayout.BeginVertical (CustomStyles.thinBox);
			showSettings = CustomGUILayout.ToggleHeader (showSettings, "Editor settings");
			if (showSettings)
			{
				updateRuntime = CustomGUILayout.Toggle ("Show realtime values?", updateRuntime, "AC.KickStarter.variablesManager.updateRuntime");

				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Filter by:", GUILayout.Width (65f));
				varFilter = (VarFilter) EditorGUILayout.EnumPopup (varFilter, GUILayout.MaxWidth (100f));
				filter = EditorGUILayout.TextField (filter);
				EditorGUILayout.EndHorizontal ();
			}
		
			EditorGUILayout.EndVertical ();

			EditorGUILayout.Space ();

			if (showGlobalTab)
			{
				varPresets = ShowPresets (varPresets, vars, VariableLocation.Global);

				if (Application.isPlaying && updateRuntime && KickStarter.runtimeVariables != null)
				{
					ShowVarList (KickStarter.runtimeVariables.globalVars, VariableLocation.Global, false);
				}
				else
				{
					ShowVarList (vars, VariableLocation.Global, true);

					foreach (VarPreset varPreset in varPresets)
					{
						varPreset.UpdateCollection (vars);
					}
				}
			}
			else if (showLocalTab)
			{
				if (KickStarter.localVariables != null)
				{
					KickStarter.localVariables.varPresets = ShowPresets (KickStarter.localVariables.varPresets, KickStarter.localVariables.localVars, VariableLocation.Local);

					if (Application.isPlaying && updateRuntime)
					{
						ShowVarList (KickStarter.localVariables.localVars, VariableLocation.Local, false);
					}
					else
					{
						ShowVarList (KickStarter.localVariables.localVars, VariableLocation.Local, true);
					}
				}
				else
				{
					EditorGUILayout.LabelField ("Local variables",  CustomStyles.subHeader);
					EditorGUILayout.HelpBox ("A GameEngine prefab must be present in the scene before Local variables can be defined", MessageType.Info);
				}
			}

			EditorGUILayout.Space ();
			if (selectedVar != null && (!Application.isPlaying || !updateRuntime))
			{
				int i = selectedVar.id;
				if (vars.Contains (selectedVar))
				{
					ShowVarGUI (VariableLocation.Global, varPresets, "AC.GlobalVariables.GetVariable (" + i + ")");
				}
				else if (KickStarter.localVariables != null && KickStarter.localVariables.localVars.Contains (selectedVar))
				{
					ShowVarGUI (VariableLocation.Local, KickStarter.localVariables.varPresets, "AC.LocalVariables.GetVariable (" + i + ")");
				}
			}

			if (GUI.changed)
			{
				EditorUtility.SetDirty (this);

				if (KickStarter.localVariables != null)
				{
					UnityVersionHandler.CustomSetDirty (KickStarter.localVariables);
				}
			}
		}


		private void ResetFilter ()
		{
			filter = "";
		}


		private void SideMenu (GVar _var, List<GVar> _vars, VariableLocation location)
		{
			GenericMenu menu = new GenericMenu ();
			sideVar = _vars.IndexOf (_var);
			sideVarLocation = location;

			menu.AddItem (new GUIContent ("Insert after"), false, Callback, "Insert after");
			if (_vars.Count > 0)
			{
				menu.AddItem (new GUIContent ("Delete"), false, Callback, "Delete");
			}

			if (sideVar > 0 || sideVar < _vars.Count-1)
			{
				menu.AddSeparator ("");
			}

			if (sideVar > 0)
			{
				menu.AddItem (new GUIContent ("Re-arrange/Move to top"), false, Callback, "Move to top");
				menu.AddItem (new GUIContent ("Re-arrange/Move up"), false, Callback, "Move up");
			}
			if (sideVar < (_vars.Count - 1))
			{
				menu.AddItem (new GUIContent ("Re-arrange/Move down"), false, Callback, "Move down");
				menu.AddItem (new GUIContent ("Re-arrange/Move to bottom"), false, Callback, "Move to bottom");
			}
			menu.AddSeparator ("");

			if (location == VariableLocation.Local)
			{
				menu.AddItem (new GUIContent ("Convert to Global"), false, Callback, "Convert to Global");
				menu.AddItem (new GUIContent ("Find local references"), false, Callback, "Find local references");
			}
			else if (location == VariableLocation.Global)
			{
				menu.AddItem (new GUIContent ("Convert to Local"), false, Callback, "Convert to Local");
				menu.AddItem (new GUIContent ("Find global references"), false, Callback, "Find global references");
			}
			
			menu.ShowAsContext ();
		}
		
		
		private void Callback (object obj)
		{
			if (sideVar >= 0)
			{
				ResetFilter ();
				List<GVar> _vars = new List<GVar>();

				if (sideVarLocation == VariableLocation.Global)
				{
					_vars = vars;
				}
				else
				{
					_vars = KickStarter.localVariables.localVars;
				}
				GVar tempVar = _vars[sideVar];

				switch (obj.ToString ())
				{
				case "Insert after":
					Undo.RecordObject (this, "Insert Variable");
					_vars.Insert (sideVar+1, new GVar (GetIDArray (_vars)));
					DeactivateAllVars ();
					break;
					
				case "Delete":
					Undo.RecordObject (this, "Delete Variable");
					_vars.RemoveAt (sideVar);
					DeactivateAllVars ();
					break;

				case "Move to top":
					Undo.RecordObject (this, "Move Variable to top");
					_vars.RemoveAt (sideVar);
					_vars.Insert (0, tempVar);
					break;

				case "Move up":
					Undo.RecordObject (this, "Move Variable up");
					_vars.RemoveAt (sideVar);
					_vars.Insert (sideVar-1, tempVar);
					break;

				case "Move down":
					Undo.RecordObject (this, "Move Variable down");
					_vars.RemoveAt (sideVar);
					_vars.Insert (sideVar+1, tempVar);
					break;

				case "Move to bottom":
					Undo.RecordObject (this, "Move Variable to bottom");
					_vars.RemoveAt (sideVar);
					_vars.Insert (_vars.Count, tempVar);
					break;

				case "Convert to Global":
					ConvertLocalToGlobal (_vars[sideVar], sideVar);
					break;

				case "Convert to Local":
					ConvertGlobalToLocal (_vars[sideVar]);
					break;

				case "Find local references":
					FindLocalReferences (tempVar);
					break;

				case "Find global references":
					FindGlobalReferences (tempVar);
					break;
				}
			}

			sideVar = -1;

			if (sideVarLocation == AC.VariableLocation.Global)
			{
				EditorUtility.SetDirty (this);
				AssetDatabase.SaveAssets ();
			}
			else
			{
				if (KickStarter.localVariables)
				{
					EditorUtility.SetDirty (KickStarter.localVariables);
				}
			}
		}


		private void ConvertLocalToGlobal (GVar localVariable, int localIndex)
		{
			if (localVariable == null) return;

			if (EditorUtility.DisplayDialog ("Convert " + localVariable.label + " to Global Variable?", "This will update all Actions and Managers that refer to this Variable.  This is a non-reversible process, and you should back up your project first. Continue?", "OK", "Cancel"))
			{
				if (UnityVersionHandler.SaveSceneIfUserWants ())
				{
					// Create new Global
					DeactivateAllVars ();
					GVar newGlobalVariable = new GVar (localVariable);
					int newGlobalID = newGlobalVariable.AssignUniqueID (GetIDArray (vars));
					vars.Add (newGlobalVariable);

					// Update current scene
					bool updatedScene = false;
					ActionList[] actionLists = FindObjectsOfType <ActionList>();
					foreach (ActionList actionList in actionLists)
					{
						foreach (Action action in actionList.actions)
						{
							if (action != null)
							{
								bool updatedActionList = action.ConvertLocalVariableToGlobal (localVariable.id, newGlobalID);
								if (updatedActionList)
								{
									updatedScene = true;
									UnityVersionHandler.CustomSetDirty (actionList, true);
									ACDebug.Log ("Updated Action " + actionList.actions.IndexOf (action) + " of ActionList '" + actionList.name + "'", actionList);
								}
							}
						}
					}

					Conversation[] conversations = FindObjectsOfType <Conversation>();
					foreach (Conversation conversation in conversations)
					{
						bool updatedConversation = conversation.ConvertLocalVariableToGlobal (localVariable.id, newGlobalID);
						if (updatedConversation)
						{
							updatedScene = true;
							UnityVersionHandler.CustomSetDirty (conversation, true);
							ACDebug.Log ("Updated Conversation '" + conversation + "'");
						}
					}

					if (updatedScene)
					{
						UnityVersionHandler.SaveScene ();
					}

					// Update Speech Manager
					if (KickStarter.speechManager)
					{
						KickStarter.speechManager.ConvertLocalVariableToGlobal (localVariable, newGlobalID);
					}

					// Remove old Local
					KickStarter.localVariables.localVars.RemoveAt (localIndex);
					EditorUtility.SetDirty (KickStarter.localVariables);
					UnityVersionHandler.SaveScene ();

					// Mark for saving
					EditorUtility.SetDirty (this);

					AssetDatabase.SaveAssets ();
				}
			}
		}


		private void FindLocalReferences (GVar localVariable)
		{
			if (localVariable == null) return;

			int totalNumReferences = 0;

			ActionList[] actionLists = FindObjectsOfType <ActionList>();
			foreach (ActionList actionList in actionLists)
			{
				foreach (Action action in actionList.actions)
				{
					int thisNumReferences = action.GetVariableReferences (actionList.parameters, VariableLocation.Local, localVariable.id);
					if (thisNumReferences > 0)
					{
						totalNumReferences += thisNumReferences;
						ACDebug.Log ("Found " + thisNumReferences + " references to local variable '" + localVariable.label + "' in Action #" + actionList.actions.IndexOf (action) + " of ActionList '" + actionList.name + "'", actionList);
					}
				}
			}

			Conversation[] conversations = FindObjectsOfType <Conversation>();
			foreach (Conversation conversation in conversations)
			{
				int thisNumReferences = conversation.GetVariableReferences (VariableLocation.Local, localVariable.id);
				if (thisNumReferences > 0)
				{
					totalNumReferences += thisNumReferences;
					ACDebug.Log ("Found " + thisNumReferences + " references to local variable '" + localVariable.label + "' in Conversation '" + conversation.name + "'", conversation);
				}
			}

			EditorUtility.DisplayDialog ("Variable search complete", "In total, found " + totalNumReferences + " references to local variable '" + localVariable.label + "' in the scene.  Please see the Console window for full details.", "OK");
		}


		private void FindGlobalReferences (GVar globalVariable)
		{
			if (globalVariable == null) return;

			if (EditorUtility.DisplayDialog ("Search '" + globalVariable.label + "' references?", "The Editor will search assets, and active scenes listed in the Build Settings, for references to the variable.  The current scene will need to be saved and listed to be included in the search process. Continue?", "OK", "Cancel"))
			{
				if (UnityVersionHandler.SaveSceneIfUserWants ())
				{
					int totalNumReferences = 0;

					// Search scenes
					string originalScene = UnityVersionHandler.GetCurrentSceneFilepath ();
					string[] sceneFiles = AdvGame.GetSceneFiles ();

					foreach (string sceneFile in sceneFiles)
					{
						UnityVersionHandler.OpenScene (sceneFile);

						ActionList[] actionLists = FindObjectsOfType <ActionList>();
						foreach (ActionList actionList in actionLists)
						{
							foreach (Action action in actionList.actions)
							{
								int thisNumReferences = action.GetVariableReferences (actionList.parameters, VariableLocation.Global, globalVariable.id);
								if (thisNumReferences > 0)
								{
									totalNumReferences += thisNumReferences;
									ACDebug.Log ("Found " + thisNumReferences + " references to global variable '" + globalVariable.label + "' in Action #" + actionList.actions.IndexOf (action) + " of ActionList '" + actionList.name + "'", actionList);
								}
							}
						}

						Conversation[] conversations = FindObjectsOfType <Conversation>();
						foreach (Conversation conversation in conversations)
						{
							int thisNumReferences = conversation.GetVariableReferences (VariableLocation.Global, globalVariable.id);
							if (thisNumReferences > 0)
							{
								totalNumReferences += thisNumReferences;
								ACDebug.Log ("Found " + thisNumReferences + " references to global variable '" + globalVariable.label + "' in Conversation '" + conversation.name + "'", conversation);
							}
						}
					}

					UnityVersionHandler.OpenScene (originalScene);

					// Search assets
					if (AdvGame.GetReferences ().speechManager != null)
					{
						ActionListAsset[] allActionListAssets = AdvGame.GetReferences ().speechManager.GetAllActionListAssets ();
						foreach (ActionListAsset actionListAsset in allActionListAssets)
						{
							foreach (Action action in actionListAsset.actions)
							{
								int thisNumReferences = action.GetVariableReferences (actionListAsset.parameters, VariableLocation.Global, globalVariable.id);
								if (thisNumReferences > 0)
								{
									totalNumReferences += thisNumReferences;
									ACDebug.Log ("Found " + thisNumReferences + " references to global variable '" + globalVariable.label + "' in Action #" + actionListAsset.actions.IndexOf (action) + " of ActionList asset '" + actionListAsset.name + "'", actionListAsset);
								}
							}
						}
					}

					// Search menus
					if (AdvGame.GetReferences ().menuManager != null && AdvGame.GetReferences ().menuManager.menus != null)
					{
						foreach (Menu menu in AdvGame.GetReferences ().menuManager.menus)
						{
							if (menu != null)
							{
								int thisNumReferences = menu.GetVariableReferences (globalVariable.id);
								if (thisNumReferences > 0)
								{
									totalNumReferences += thisNumReferences;
									ACDebug.Log ("Found " + thisNumReferences + " references to global variable '" + globalVariable.label + "' in Menu '" + menu.title + "'");
								}
							}
						}
					}

					EditorUtility.DisplayDialog ("Variable search complete", "In total, found " + totalNumReferences + " references to global variable '" + globalVariable.label + "' in the project.  Please see the Console window for full details.", "OK");
				}
			}
		}


		private void ConvertGlobalToLocal (GVar globalVariable)
		{
			if (globalVariable == null) return;

			if (KickStarter.localVariables == null)
			{
				ACDebug.LogWarning ("Cannot convert variable to local since the scene has not been prepared for AC.");
				return;
			}

			if (EditorUtility.DisplayDialog ("Convert " + globalVariable.label + " to Local Variable?", "This will update all Actions and Managers that refer to this Variable.  This is a non-reversible process, and you should back up your project first. Continue?", "OK", "Cancel"))
			{
				if (UnityVersionHandler.SaveSceneIfUserWants ())
				{
					// Create new Local
					DeactivateAllVars ();
					GVar newLocalVariable = new GVar (globalVariable);
					int newLocalID = newLocalVariable.AssignUniqueID (GetIDArray (KickStarter.localVariables.localVars));
					KickStarter.localVariables.localVars.Add (newLocalVariable);
					UnityVersionHandler.CustomSetDirty (KickStarter.localVariables, true);
					UnityVersionHandler.SaveScene ();

					// Update current scene
					bool updatedScene = false;
					string originalScene = UnityVersionHandler.GetCurrentSceneFilepath ();

					ActionList[] actionLists = FindObjectsOfType <ActionList>();
					foreach (ActionList actionList in actionLists)
					{
						foreach (Action action in actionList.actions)
						{
							bool updatedActionList = action.ConvertGlobalVariableToLocal (globalVariable.id, newLocalID, true);
							if (updatedActionList)
							{
								updatedScene = true;
								UnityVersionHandler.CustomSetDirty (actionList, true);
								ACDebug.Log ("Updated Action " + actionList.actions.IndexOf (action) + " of ActionList '" + actionList.name + "' in scene '" + originalScene + "'", actionList);
							}
						}
					}

					Conversation[] conversations = FindObjectsOfType <Conversation>();
					foreach (Conversation conversation in conversations)
					{
						bool updatedConversation = conversation.ConvertGlobalVariableToLocal (globalVariable.id, newLocalID, true);
						if (updatedConversation)
						{
							updatedScene = true;
							UnityVersionHandler.CustomSetDirty (conversation, true);
							ACDebug.Log ("Updated Conversation " + conversation + ") in scene '" + originalScene + "'");
						}
					}

					if (updatedScene)
					{
						UnityVersionHandler.SaveScene ();
					}

					// Update other scenes
					string[] sceneFiles = AdvGame.GetSceneFiles ();
					foreach (string sceneFile in sceneFiles)
					{
						if (sceneFile == originalScene)
						{
							continue;
						}
						UnityVersionHandler.OpenScene (sceneFile);

						actionLists = FindObjectsOfType <ActionList>();
						foreach (ActionList actionList in actionLists)
						{
							foreach (Action action in actionList.actions)
							{
								if (action != null)
								{
									bool isAffected = action.ConvertGlobalVariableToLocal (globalVariable.id, newLocalID, false);
									if (isAffected)
									{
										ACDebug.LogWarning ("Cannot update Action " + actionList.actions.IndexOf (action) + " in ActionList '" + actionList.name + "' in scene '" + sceneFile + "' because it cannot access the Local Variable in scene '" + originalScene + "'.");
									}
								}
							}
						}

						conversations = FindObjectsOfType <Conversation>();
						foreach (Conversation conversation in conversations)
						{
							bool isAffected = conversation.ConvertGlobalVariableToLocal (globalVariable.id, newLocalID, false);
							if (isAffected)
							{
								ACDebug.LogWarning ("Cannot update Conversation " + conversation + ") in scene '" + sceneFile + "' because it cannot access the Local Variable in scene '" + originalScene + "'.");
							}
						}
					}

					UnityVersionHandler.OpenScene (originalScene);

					// Update Menu Manager
					if (KickStarter.menuManager)
					{
						KickStarter.menuManager.CheckConvertGlobalVariableToLocal (globalVariable.id, newLocalID);
					}

					//  Update Speech Manager
					if (KickStarter.speechManager)
					{
						// Search asset files
						ActionListAsset[] allActionListAssets = KickStarter.speechManager.GetAllActionListAssets ();
						UnityVersionHandler.OpenScene (originalScene);

						if (allActionListAssets != null)
						{
							foreach (ActionListAsset actionListAsset in allActionListAssets)
							{
								foreach (Action action in actionListAsset.actions)
								{
									if (action != null)
									{
										bool isAffected = action.ConvertGlobalVariableToLocal (globalVariable.id, newLocalID, false);
										if (isAffected)
										{
											ACDebug.LogWarning ("Cannot update Action " + actionListAsset.actions.IndexOf (action) + " in ActionList asset '" + actionListAsset.name + "' because asset files cannot refer to Local Variables.");
										}
									}
								}
							}
						}

						KickStarter.speechManager.ConvertGlobalVariableToLocal (globalVariable, UnityVersionHandler.GetCurrentSceneName ());
					}

					// Remove old Global
					vars.Remove (globalVariable);

					// Mark for saving
					EditorUtility.SetDirty (this);
					if (KickStarter.localVariables != null)
					{
						UnityVersionHandler.CustomSetDirty (KickStarter.localVariables);
					}

					AssetDatabase.SaveAssets ();
				}
			}
		}


		/**
		 * <summary>Selects a Variable for editing</summary>
		 * <param name = "variableID">The ID of the Variable to select</param>
		 * <param name = "location">The Variable's location (Global, Local)</param>
		 */
		public void ShowVariable (int variableID, VariableLocation location)
		{
			if (location == VariableLocation.Global)
			{
				GVar varToActivate = GetVariable (variableID);
				if (varToActivate != null)
				{
					DeactivateAllVars ();
					ActivateVar (varToActivate);
				}
				SetTab (0);
			}
			else if (location == VariableLocation.Local)
			{
				GVar varToActivate = LocalVariables.GetVariable (variableID);
				if (varToActivate != null)
				{
					DeactivateAllVars ();
					ActivateVar (varToActivate);
				}
				SetTab (1);
			}
		}


		private void ActivateVar (GVar varToActivate)
		{
			if (varToActivate == null) return;

			if (selectedVar != varToActivate)
			{
				varToActivate.isEditing = true;
				selectedVar = varToActivate;
				EditorGUIUtility.editingTextField = false;
			}
		}
		
		
		private void DeactivateAllVars ()
		{
			if (KickStarter.localVariables)
			{
				foreach (GVar var in KickStarter.localVariables.localVars)
				{
					var.isEditing = false;
				}
			}

			foreach (GVar var in vars)
			{
				var.isEditing = false;
			}
			selectedVar = null;
			EditorGUIUtility.editingTextField = false;
		}


		private int[] GetIDArray (List<GVar> _vars)
		{
			// Returns a list of id's in the list
			
			List<int> idArray = new List<int>();
			
			foreach (GVar variable in _vars)
			{
				idArray.Add (variable.id);
			}
			
			idArray.Sort ();
			return idArray.ToArray ();
		}


		private int[] GetIDArray (List<VarPreset> _varPresets)
		{
			// Returns a list of id's in the list
			
			List<int> idArray = new List<int>();
			
			foreach (VarPreset _varPreset in _varPresets)
			{
				idArray.Add (_varPreset.ID);
			}
			
			idArray.Sort ();
			return idArray.ToArray ();
		}


		private bool VarMatchesFilter (GVar _var)
		{
			if (string.IsNullOrEmpty (filter))
			{
				return true;
			}

			if (_var != null)
			{
				if (varFilter == VarFilter.ByName && _var.label.ToLower ().Contains (filter.ToLower ()))
				{
					return true;
				}
				else if (varFilter == VarFilter.ByDescription && _var.description.ToLower ().Contains (filter.ToLower ()))
				{
					return true;
				}
			}
			return false;
		}


		private void ShowVarList (List<GVar> _vars, VariableLocation location, bool allowEditing)
		{
			EditorGUILayout.BeginVertical (CustomStyles.thinBox);
			showVariablesList = CustomGUILayout.ToggleHeader (showVariablesList, location + " variables");
			if (showVariablesList)
			{
				scrollPos = EditorGUILayout.BeginScrollView (scrollPos, GUILayout.Height (Mathf.Min (_vars.Count * 21, 235f)+5));
				bool varFound = false;

				foreach (GVar _var in _vars)
				{
					if (VarMatchesFilter (_var))
					{
						varFound = true;
						EditorGUILayout.BeginHorizontal ();
						
						string buttonLabel = _var.id + ": ";
						if (buttonLabel == "")
						{
							_var.label += "(Untitled)";	
						}
						else
						{
							buttonLabel += _var.label;

							if (buttonLabel.Length > 30)
							{
								buttonLabel = buttonLabel.Substring (0, 30);
							}
						}

						string varValue = _var.GetValue ();
						varValue = varValue.Replace("\r\n", "");
						varValue = varValue.Replace("\n", "");
						varValue = varValue.Replace("\r", "");
						if (varValue.Length > 20)
						{
							varValue = varValue.Substring (0, 20);
						}

						buttonLabel += " (" + _var.type.ToString () + " - " + varValue + ")";

						if (allowEditing)
						{
							if (GUILayout.Toggle (_var.isEditing, buttonLabel, "Button"))
							{
								if (selectedVar != _var)
								{
									DeactivateAllVars ();
									ActivateVar (_var);
								}
							}
							
							if (GUILayout.Button (Resource.CogIcon, GUILayout.Width (20f), GUILayout.Height (15f)))
							{
								SideMenu (_var, _vars, location);
							}
						}
						else
						{
							GUILayout.Label (buttonLabel, "Button");
						}
						
						EditorGUILayout.EndHorizontal ();
					}
				}

				if (!varFound)
				{
					if (_vars.Count > 0 && !string.IsNullOrEmpty (filter))
					{
						EditorGUILayout.HelpBox ("No variables with '" + filter + "' in their " + ((varFilter == VarFilter.ByName) ? "name" : "description") + " found.", MessageType.Info);
					}
				}

				EditorGUILayout.EndScrollView ();

				if (allowEditing)
				{
					EditorGUILayout.Space ();
					EditorGUILayout.BeginHorizontal ();
					if (GUILayout.Button("Create new " + location + " variable"))
					{
						ResetFilter ();
						Undo.RecordObject (this, "Add " + location + " variable");
						_vars.Add (new GVar (GetIDArray (_vars)));
						DeactivateAllVars ();
						ActivateVar (_vars [_vars.Count-1]);
					}

					if (GUILayout.Button (Resource.CogIcon, GUILayout.Width (20f), GUILayout.Height (15f)))
					{
						ExportSideMenu ();
					}
					EditorGUILayout.EndHorizontal ();
				}
			}
			EditorGUILayout.EndVertical ();
		}


		private void ExportSideMenu ()
		{
			GenericMenu menu = new GenericMenu ();
			//menu.AddItem (new GUIContent ("Import variables..."), false, ExportCallback, "Import");
			menu.AddItem (new GUIContent ("Export variables..."), false, ExportCallback, "Export");
			menu.ShowAsContext ();
		}


		private void ExportCallback (object obj)
		{
			switch (obj.ToString ())
			{
				case "Import":
					//ImportItems ();
					break;

				case "Export":
					VarExportWizardWindow.Init (this);
					break;
			}
		}


		private void ShowVarGUI (VariableLocation location, List<VarPreset> _varPresets = null, string apiPrefix = "")
		{
			EditorGUILayout.BeginVertical (CustomStyles.thinBox);
			showVariablesProperties = CustomGUILayout.ToggleHeader (showVariablesProperties, location + " variable '" + selectedVar.label + "' properties");
			if (showVariablesProperties)
			{
				selectedVar.label = CustomGUILayout.TextField ("Label:", selectedVar.label, apiPrefix + ".label");
				selectedVar.type = (VariableType) CustomGUILayout.EnumPopup ("Type:", selectedVar.type, apiPrefix + ".type");

				if (selectedVar.type == VariableType.Boolean)
				{
					if (selectedVar.val != 1)
					{
						selectedVar.val = 0;
					}
					selectedVar.val = CustomGUILayout.Popup ("Initial value:", selectedVar.val, boolType, apiPrefix + ".val");
				}
				else if (selectedVar.type == VariableType.Integer)
				{
					selectedVar.val = CustomGUILayout.IntField ("Initial value:", selectedVar.val, apiPrefix + ".val");
				}
				else if (selectedVar.type == VariableType.PopUp)
				{
					selectedVar.popUps = PopupsGUI (selectedVar.popUps);
					selectedVar.val = CustomGUILayout.Popup ("Initial value:", selectedVar.val, selectedVar.popUps, apiPrefix + ".val");
					selectedVar.canTranslate = CustomGUILayout.Toggle ("Values can be translated?", selectedVar.canTranslate, apiPrefix + ".canTranslate");
				}
				else if (selectedVar.type == VariableType.String)
				{
					//selectedVar.textVal = CustomGUILayout.TextField ("Initial value:", selectedVar.textVal, apiPrefix + ".textVal");

					EditorGUILayout.BeginHorizontal ();
					CustomGUILayout.LabelField ("Initial value:", GUILayout.Width (140f), apiPrefix + ".textVal");
					EditorStyles.textField.wordWrap = true;
					selectedVar.textVal = CustomGUILayout.TextArea (selectedVar.textVal, GUILayout.MaxWidth (800f), apiPrefix + ".textVal");
					EditorGUILayout.EndHorizontal ();
					selectedVar.canTranslate = CustomGUILayout.Toggle ("Values can be translated?", selectedVar.canTranslate, apiPrefix + ".canTranslate");
				}
				else if (selectedVar.type == VariableType.Float)
				{
					selectedVar.floatVal = CustomGUILayout.FloatField ("Initial value:", selectedVar.floatVal, apiPrefix + ".floatVal");
				}
				else if (selectedVar.type == VariableType.Vector3)
				{
					selectedVar.vector3Val = CustomGUILayout.Vector3Field ("Initial value:", selectedVar.vector3Val, apiPrefix + ".vector3Val");
				}

				if (location == VariableLocation.Local)
				{
					CustomGUILayout.TokenLabel ("[localvar:" + selectedVar.id.ToString () + "]");
				}
				else
				{
					CustomGUILayout.TokenLabel ("[var:" + selectedVar.id.ToString () + "]");
				}
				
				if (_varPresets != null)
				{
					foreach (VarPreset _varPreset in _varPresets)
					{
						// Local
						string apiPrefix2 = (location == VariableLocation.Local) ? 
											"AC.KickStarter.localVariables.GetPreset (" + _varPreset.ID + ").GetPresetValue (" + selectedVar.id + ")" :
											"AC.KickStarter.runtimeVariables.GetPreset (" + _varPreset.ID + ").GetPresetValue (" + selectedVar.id + ")";

						_varPreset.UpdateCollection (selectedVar);

						string label = "'" + _varPreset.label + "' value:";
						PresetValue presetValue = _varPreset.GetPresetValue (selectedVar);
						if (selectedVar.type == VariableType.Boolean)
						{
							presetValue.val = CustomGUILayout.Popup (label, presetValue.val, boolType, apiPrefix2 + ".val");
						}
						else if (selectedVar.type == VariableType.Integer)
						{
							presetValue.val = CustomGUILayout.IntField (label, presetValue.val, apiPrefix2 + ".val");
						}
						else if (selectedVar.type == VariableType.PopUp)
						{
							presetValue.val = CustomGUILayout.Popup (label, presetValue.val, selectedVar.popUps, apiPrefix2 + ".val");
						}
						else if (selectedVar.type == VariableType.String)
						{
							presetValue.textVal = CustomGUILayout.TextField (label, presetValue.textVal, apiPrefix2 + ".textVal");
						}
						else if (selectedVar.type == VariableType.Float)
						{
							presetValue.floatVal = CustomGUILayout.FloatField (label, presetValue.floatVal, apiPrefix2 + ".floatVal");
						}
						else if (selectedVar.type == VariableType.Vector3)
						{
							presetValue.vector3Val = CustomGUILayout.Vector3Field (label, presetValue.vector3Val, apiPrefix2 + ".vector3Val");
						}
					}
				}

				EditorGUILayout.Space ();
				if (location == VariableLocation.Local)
				{
					selectedVar.link = VarLink.None;
				}
				else
				{
					selectedVar.link = (VarLink) CustomGUILayout.EnumPopup ("Link to:", selectedVar.link, apiPrefix + ".link");
					if (selectedVar.link == VarLink.PlaymakerGlobalVariable)
					{
						if (PlayMakerIntegration.IsDefinePresent ())
						{
							selectedVar.pmVar = CustomGUILayout.TextField ("Playmaker Global Variable:", selectedVar.pmVar, apiPrefix + ".pmVar");
							selectedVar.updateLinkOnStart = CustomGUILayout.Toggle ("Use PM for initial value?", selectedVar.updateLinkOnStart, apiPrefix + ".updateLinkOnStart");
						}
						else
						{
							EditorGUILayout.HelpBox ("The 'PlayMakerIsPresent' Scripting Define Symbol must be listed in the\nPlayer Settings. Please set it from Edit -> Project Settings -> Player", MessageType.Warning);
						}
					}
					else if (selectedVar.link == VarLink.OptionsData)
					{
						EditorGUILayout.HelpBox ("This Variable will be stored in PlayerPrefs, and not in saved game files.", MessageType.Info);
					}
					else if (selectedVar.link == VarLink.CustomScript)
					{
						selectedVar.updateLinkOnStart = CustomGUILayout.Toggle ("Script sets initial value?", selectedVar.updateLinkOnStart, apiPrefix + ".updateLinkOnStart");
						EditorGUILayout.HelpBox ("See the Manual's 'Global variable linking' chapter for details on how to synchronise values.", MessageType.Info);
					}
				}

				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Internal description:", GUILayout.MaxWidth (146f));
				selectedVar.description = EditorGUILayout.TextArea (selectedVar.description);
				EditorGUILayout.EndHorizontal ();
			}
			EditorGUILayout.EndVertical ();
		}


		public static string[] PopupsGUI (string[] popUps)
		{
			List<string> popUpList = new List<string>();
			if (popUps != null && popUps.Length > 0)
			{
				foreach (string p in popUps)
				{
					popUpList.Add (p);
				}
			}

			for (int i=0; i<popUpList.Count; i++)
			{
				EditorGUILayout.BeginHorizontal ();
				popUpList[i] = EditorGUILayout.TextField ("Value " + i.ToString ()+":", popUpList[i]);

				if (GUILayout.Button ("-", GUILayout.MaxWidth (20f)))
				{
					popUpList.RemoveAt (i);
					EditorGUIUtility.editingTextField = false;
					i=-1;
				}

				EditorGUILayout.EndHorizontal ();
			}

			if (GUILayout.Button ("Add new value"))
			{
				popUpList.Add (string.Empty);
			}

			return popUpList.ToArray ();
		}


		private void SetTab (int tab)
		{
			if (tab == 0)
			{
				if (showLocalTab)
				{
					selectedVar = null;
					EditorGUIUtility.editingTextField = false;
				}
				showGlobalTab = true;
				showLocalTab = false;
			}
			else if (tab == 1)
			{
				if (showGlobalTab)
				{
					selectedVar = null;
					EditorGUIUtility.editingTextField = false;
				}
				showLocalTab = true;
				showGlobalTab = false;
			}
		}


		private List<VarPreset> ShowPresets (List<VarPreset> _varPresets, List<GVar> _vars, VariableLocation location)
		{
			if (_vars == null || _vars.Count == 0)
			{
				return _varPresets;
			}

			if (!Application.isPlaying || _varPresets.Count > 0)
			{
				EditorGUILayout.BeginVertical (CustomStyles.thinBox);
				showPresets = CustomGUILayout.ToggleHeader (showPresets, "Preset configurations");
			}

			if (showPresets && (!Application.isPlaying || _varPresets.Count > 0))
			{
				List<string> labelList = new List<string>();
				
				int i = 0;
				int presetNumber = -1;
				
				if (_varPresets.Count > 0)
				{
					foreach (VarPreset _varPreset in _varPresets)
					{
						if (_varPreset.label != "")
						{
							labelList.Add (i.ToString () + ": " + _varPreset.label);
						}
						else
						{
							labelList.Add (i.ToString () + ": (Untitled)");
						}
						
						if (_varPreset.ID == chosenPresetID)
						{
							presetNumber = i;
						}
						i++;
					}
					
					if (presetNumber == -1)
					{
						chosenPresetID = 0;
					}
					else if (presetNumber >= _varPresets.Count)
					{
						presetNumber = Mathf.Max (0, _varPresets.Count - 1);
					}
					else
					{
						presetNumber = EditorGUILayout.Popup ("Created presets:", presetNumber, labelList.ToArray());
						chosenPresetID = _varPresets[presetNumber].ID;
					}
				}
				else
				{
					chosenPresetID = presetNumber = -1;
				}

				if (presetNumber >= 0)
				{
					string apiPrefix = ((location == VariableLocation.Local) ? "AC.KickStarter.localVariables.GetPreset (" + chosenPresetID + ")" : "AC.KickStarter.runtimeVariables.GetPreset (" + chosenPresetID + ")");

					if (!Application.isPlaying)
					{
						_varPresets [presetNumber].label = CustomGUILayout.TextField ("Preset name:", _varPresets [presetNumber].label, apiPrefix + ".label");
					}

					EditorGUILayout.BeginHorizontal ();
					if (!Application.isPlaying)
					{
						GUI.enabled = false;
					}
					if (GUILayout.Button ("Bulk-assign"))
					{
						if (presetNumber >= 0 && _varPresets.Count > presetNumber)
						{
							if (location == VariableLocation.Global)
							{
								if (KickStarter.runtimeVariables)
								{
									KickStarter.runtimeVariables.AssignFromPreset (_varPresets [presetNumber]);
									ACDebug.Log ("Global variables updated to " + _varPresets [presetNumber].label);
								}
							}
							else if (location == VariableLocation.Local)
							{
								if (KickStarter.localVariables)
								{
									KickStarter.localVariables.AssignFromPreset (_varPresets [presetNumber]);
									ACDebug.Log ("Local variables updated to " + _varPresets [presetNumber].label);
								}
							}
						}
					}

					GUI.enabled = !Application.isPlaying;
					if (GUILayout.Button ("Delete"))
					{
						_varPresets.RemoveAt (presetNumber);
						presetNumber = 0;
						chosenPresetID = 0;
					}

					GUI.enabled = true;
					EditorGUILayout.EndHorizontal ();
				}

				if (!Application.isPlaying)
				{
					if (GUILayout.Button ("Create new preset"))
					{
						VarPreset newVarPreset = new VarPreset (_vars, GetIDArray (_varPresets));
						_varPresets.Add (newVarPreset);
						chosenPresetID = newVarPreset.ID;
					}
				}
			}
			if (!Application.isPlaying || _varPresets.Count > 0)
			{
				EditorGUILayout.EndVertical ();
			}

			EditorGUILayout.Space ();

			return _varPresets;
		}

		#endif


		/**
		 * <summary>Gets a global variable</summary>
		 * <param name = "_id">The ID number of the global variable to find</param>
		 * <returns>The global variable</returns>
		 */
		public GVar GetVariable (int _id)
		{
			foreach (GVar _var in vars)
			{
				if (_var.id == _id)
				{
					return _var;
				}
			}
			return null;
		}

	}

}