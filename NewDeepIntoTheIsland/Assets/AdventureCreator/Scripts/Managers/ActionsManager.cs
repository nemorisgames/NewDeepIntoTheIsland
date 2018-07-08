/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionsManager.cs"
 * 
 *	This script handles the "Actions" tab of the Game Editor window.
 *	Custom actions can be added and removed by selecting them with this.
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
	 * Handles the "Actions" tab of the Game Editor window.
	 * All available Actions are listed here, and custom Actions can be added.
	 */
	[System.Serializable]
	public class ActionsManager : ScriptableObject
	{
		
		#if UNITY_EDITOR

		/** The folder path to any custom Actions */
		public string customFolderPath = "AdventureCreator/Scripts/Actions";

		#endif

		/** If True, then Actions can be displayed in an ActionList's Inspector window */
		public bool displayActionsInInspector = true;
		/** How Actions are arranged in the ActionList Editor window (ArrangedVertically, ArrangedHorizontally) */
		public DisplayActionsInEditor displayActionsInEditor = DisplayActionsInEditor.ArrangedVertically;
		/** If True, then multiple ActionList Editor windows can be opened at once */
		public bool allowMultipleActionListWindows = false;
		/** The effect the mouse scrollwheel has inside the ActionList Editor window (PansWindow, ZoomsWindow) */
		public ActionListEditorScrollWheel actionListEditorScrollWheel = ActionListEditorScrollWheel.PansWindow;
		/** If True, then panning is inverted in the ActionList Editor window (useful for Macbooks) */
		public bool invertPanning = false;
		/** The speed factor for panning/zooming */
		public float panSpeed = 1f;
		/** The index number of the default Action (deprecated) */
		public int defaultClass;
		/** The class name of the default Action */
		public string defaultClassName;
		/** A List of all Action classes found */
		public List<ActionType> AllActions = new List<ActionType>();
		/** A List of all Action classes enabled */
		public List<ActionType> EnabledActions = new List<ActionType>();

		#if UNITY_EDITOR

		private ActionType selectedClass = null;
		private bool[] toggles = new bool[19];

		private bool showEditing = true;
		private bool showCustom = true;
		private bool showCategories = true;

		#endif


		/**
		 * <summary>Gets the filename of the default Action.</summary>
		 * <returns>The filename of the default Action.</returns>
		 */
		public string GetDefaultAction ()
		{
			Upgrade ();

			if (!string.IsNullOrEmpty (defaultClassName))
			{
				return defaultClassName;
			}

			return "";
		}


		private void Upgrade ()
		{
			if (defaultClass >= 0 && EnabledActions.Count > 0 && EnabledActions.Count > defaultClass)
			{
				defaultClassName = EnabledActions[defaultClass].fileName;
				defaultClass = -1;
			}

			if (string.IsNullOrEmpty (defaultClassName) && EnabledActions.Count > 0)
			{
				defaultClassName = EnabledActions[0].fileName;
				defaultClass = -1;
			}
		}
		
		
		#if UNITY_EDITOR

		public void ShowGUI ()
		{
			EditorGUILayout.BeginVertical (CustomStyles.thinBox);
			showEditing = CustomGUILayout.ToggleHeader (showEditing, "ActionList editing settings");
			if (showEditing)
			{
				displayActionsInInspector = CustomGUILayout.ToggleLeft ("List Actions in Inspector window?", displayActionsInInspector, "AC.KickStarter.actionsManager.displayActionsInInspector");
				displayActionsInEditor = (DisplayActionsInEditor) CustomGUILayout.EnumPopup ("Actions in Editor are:", displayActionsInEditor, "AC.KickStarter.actionsManager.displayActionsInEditor");
				actionListEditorScrollWheel = (ActionListEditorScrollWheel) CustomGUILayout.EnumPopup ("Using scroll-wheel:", actionListEditorScrollWheel, "AC.KickStarter.actionsManager.actionListEditorScrollWheel");

				if (actionListEditorScrollWheel == ActionListEditorScrollWheel.ZoomsWindow)
				{
					EditorGUILayout.HelpBox ("Panning is possible by holding down the middle-mouse button.", MessageType.Info);
				}

				panSpeed = CustomGUILayout.FloatField ((actionListEditorScrollWheel == ActionListEditorScrollWheel.PansWindow) ? "Panning speed:" : "Zoom speed:", panSpeed, "AC.KickStarter.actionsManager.panSpeed");
				invertPanning = CustomGUILayout.ToggleLeft ("Invert panning in ActionList Editor?", invertPanning, "AC.KickStarter.actionsManager.invertPanning");
				allowMultipleActionListWindows = CustomGUILayout.ToggleLeft ("Allow multiple ActionList Editor windows?", allowMultipleActionListWindows, "AC.KickStarter.actionsManager.allowMultipleActionListWindows");
			}
			EditorGUILayout.EndVertical ();
			EditorGUILayout.Space ();
			
			EditorGUILayout.BeginVertical (CustomStyles.thinBox);
			showCustom = CustomGUILayout.ToggleHeader (showCustom, "Custom Action scripts");
			if (showCustom)
			{
				GUILayout.BeginHorizontal ();
				GUILayout.Label ("Folder to search:", GUILayout.Width (110f));
				GUILayout.Label (customFolderPath, EditorStyles.textField);
				GUILayout.EndHorizontal ();
				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Set directory", EditorStyles.miniButtonLeft))
				{
					string path = EditorUtility.OpenFolderPanel ("Set custom Actions directory", "Assets", "");
					string dataPath = Application.dataPath;
					if (path.Contains (dataPath))
					{
						if (path == dataPath)
						{
							customFolderPath = "";
						}
						else
						{
							customFolderPath = path.Replace (dataPath + "/", "");
						}
					}
					else
					{
						ACDebug.LogError ("Cannot set new directory - be sure to select within the Assets directory.");
					}
				}
				if (GUILayout.Button ("Clear", EditorStyles.miniButtonRight))
				{
					customFolderPath = "";
				}
				GUILayout.EndHorizontal ();
			}
			EditorGUILayout.EndVertical ();
			
			if (AllActions.Count > 0)
			{
				GUILayout.Space (10);

				Upgrade ();

				EditorGUILayout.BeginVertical (CustomStyles.thinBox);
				showCategories = CustomGUILayout.ToggleHeader (showCategories, "Action categories");
				if (showCategories)
				{
					ActionCategory[] categories = (ActionCategory[]) System.Enum.GetValues (typeof(ActionCategory));

					for (int i=0; i<categories.Length; i++)
					{
						toggles[i] = GUILayout.Toggle (toggles[i], categories[i].ToString (), "Button");
						if (toggles[i])
						{
							int j=-1;
							foreach (ActionType subclass in AllActions)
							{
								if (subclass.category == categories[i])
								{
									j++;
									int enabledIndex = -1;
									if (EnabledActions.Contains (subclass))
									{
										enabledIndex = EnabledActions.IndexOf (subclass);
									}

									if (selectedClass != null && subclass.category == selectedClass.category && subclass.title == selectedClass.title)
									{
										EditorGUILayout.BeginVertical ("Button");
										SpeechLine.ShowField ("Name:", subclass.GetFullTitle (), false);
										SpeechLine.ShowField ("Filename:", subclass.fileName + ".cs", false);
										SpeechLine.ShowField ("Description:", subclass.description, true);
										subclass.isEnabled = true;
										EditorGUILayout.BeginHorizontal ();
										if (enabledIndex >= 0)
										{
											if (!string.IsNullOrEmpty (defaultClassName) && subclass.fileName == defaultClassName)
											{
												EditorGUILayout.LabelField ("DEFAULT",  CustomStyles.subHeader, GUILayout.Width (140f));
											}
											else if (subclass.isEnabled)
											{
												if (GUILayout.Button ("Make default?", GUILayout.Width (140f)))
												{
													if (EnabledActions.Contains (subclass))
													{
														defaultClassName = subclass.fileName;
													}
												}
											}
										}
										subclass.color = EditorGUILayout.ColorField ("Node colour:", subclass.color);
										
										EditorGUILayout.EndHorizontal ();
										EditorGUILayout.BeginHorizontal ();
										
										if (GUILayout.Button ("Search local instances"))
										{
											SearchForInstances (true, subclass);
										}
										if (GUILayout.Button ("Search all instances"))
										{
											if (UnityVersionHandler.SaveSceneIfUserWants ())
											{
												SearchForInstances (false, subclass);
											}
										}
										
										EditorGUILayout.EndHorizontal ();
										EditorGUILayout.EndVertical ();
									}
									else
									{
										EditorGUILayout.BeginHorizontal ();
										if (GUILayout.Button (j.ToString () + ": " + subclass.GetFullTitle (), EditorStyles.label, GUILayout.Width (200f)))
										{
											selectedClass = subclass;
										}
										if (!string.IsNullOrEmpty (defaultClassName) && subclass.fileName == defaultClassName)
										{
											EditorGUILayout.LabelField ("DEFAULT",  CustomStyles.subHeader, GUILayout.Width (60f));
										}
										EditorGUILayout.EndHorizontal ();
										GUILayout.Box ("", GUILayout.ExpandWidth (true), GUILayout.Height(1));
									}
								}
							}
							if (j < 0)
							{
								EditorGUILayout.HelpBox ("There are no Actions of this category type present!", MessageType.Info);
							}
						}
					}
				}
				EditorGUILayout.EndVertical ();

				if (defaultClass > EnabledActions.Count - 1)
				{
					defaultClass = EnabledActions.Count - 1;
				}
			}
			else
			{
				EditorGUILayout.HelpBox ("No Action subclass files found.", MessageType.Warning);
			}

			if (GUI.changed)
			{
				SetEnabled ();
				Upgrade ();
				EditorUtility.SetDirty (this);
			}
		}


		private void SearchForInstances (bool justLocal, ActionType actionType)
		{
			if (justLocal)
			{
				SearchSceneForType ("", actionType);
				return;
			}
			
			// First look for lines that already have an assigned lineID
			string[] sceneFiles = AdvGame.GetSceneFiles ();
			if (sceneFiles == null || sceneFiles.Length == 0)
			{
				Debug.LogWarning ("Cannot search scenes - no enabled scenes could be found in the Build Settings.");
			}
			else
			{
				foreach (string sceneFile in sceneFiles)
				{
					SearchSceneForType (sceneFile, actionType);
				}
			}

			ActionListAsset[] allActionListAssets = AdvGame.GetReferences ().speechManager.GetAllActionListAssets ();
			foreach (ActionListAsset actionListAsset in allActionListAssets)
			{
				int[] foundIDs = SearchActionsForType (actionListAsset.actions, actionType);
				if (foundIDs != null && foundIDs.Length > 0)
				{
					ACDebug.Log ("(Asset: " + actionListAsset.name + ") Found " + foundIDs.Length + " instances of '" + actionType.GetFullTitle () + "' " + CreateIDReport (foundIDs), actionListAsset);
				}
			}
		}
		
		
		private void SearchSceneForType (string sceneFile, ActionType actionType)
		{
			string sceneLabel = "";
			
			if (sceneFile != "")
			{
				sceneLabel = "(Scene: " + sceneFile + ") ";
				UnityVersionHandler.OpenScene (sceneFile);
			}

			// Speech lines and journal entries
			ActionList[] actionLists = GameObject.FindObjectsOfType (typeof (ActionList)) as ActionList[];
			foreach (ActionList list in actionLists)
			{
				int[] foundIDs = SearchActionsForType (list.GetActions (), actionType);
				if (foundIDs != null && foundIDs.Length > 0)
				{
					ACDebug.Log (sceneLabel + " Found " + foundIDs.Length + " instances in '" + list.gameObject.name + "' " + CreateIDReport (foundIDs), list.gameObject);
				}
			}
		}


		private string CreateIDReport (int[] foundIDs)
		{
			string idLabel = "(IDs ";
			for (int i=0; i<foundIDs.Length; i++)
			{
				idLabel += foundIDs[i];
				if (i < (foundIDs.Length - 1))
				{
					idLabel += ", ";
				}
			}
			idLabel += ")";
			return idLabel;
		}
		
		
		private int[] SearchActionsForType (List<Action> actionList, ActionType actionType)
		{
			List<int> foundIDs = new List<int>();
			if (actionList != null)
			{
				foreach (Action action in actionList)
				{
					if ((action.category == actionType.category && action.title == actionType.title) ||
					    (action.GetType ().ToString () == actionType.fileName) ||
					    (action.GetType ().ToString () == "AC." + actionType.fileName))
					{
						int id = actionList.IndexOf (action);
						foundIDs.Add (id);
					}
				}
			}
			return foundIDs.ToArray ();
		}


		/** The folder path to the default Actions */
		public string FolderPath
		{
			get
			{
				return Resource.MainFolderPathRelativeToAssets + "/Scripts/Actions";
			}
		}


		public bool UsingCustomActionsFolder
		{
			get
			{
				return (customFolderPath != FolderPath);
			}
		}
		
		#endif
		

		/**
		 * Re-gathers the List of EnabledActions, based on which Actions in AllActions have isEnabled = True.
		 */
		public void SetEnabled ()
		{
			EnabledActions.Clear ();
			
			foreach (ActionType subclass in AllActions)
			{
				if (subclass.isEnabled)
				{
					EnabledActions.Add (subclass);
				}
			}
		}
		

		/**
		 * <summary>Gets the filename of an enabled Action.</summary>
		 * <param name = "i">The index number of the Action, in EnabledActions, to get the filename of</param>
		 * <returns>Gets the filename of the Action</returns>
		 */
		public string GetActionName (int i)
		{
			return (EnabledActions [i].fileName);
		}


		/**
		 * <summary>Checks if any enabled Actions have a specific filename.</summary>
		 * <param name = "_name">The filename to check for</param>
		 * <returns>True if any enabled Actions have the supplied filename</returns>
		 */
		public bool DoesActionExist (string _name)
		{
			foreach (ActionType actionType in EnabledActions)
			{
				if (_name == actionType.fileName || _name == ("AC." + actionType.fileName))
				{
					return true;
				}
			}
			return false;
		}
		

		/**
		 * <summary>Gets the number of enabled Actions.</summary>
		 * <returns>The number of enabled Actions</returns>
		 */
		public int GetActionsSize ()
		{
			return (EnabledActions.Count);
		}


		/**
		 * <summary>Gets all Action titles within EnabledActions.</summary>
		 * <returns>A string array of all Action titles within EnabledActions</returns>
		 */
		public string[] GetActionTitles ()
		{
			List<string> titles = new List<string>();
			
			foreach (ActionType type in EnabledActions)
			{
				titles.Add (type.title);
			}
			
			return (titles.ToArray ());
		}


		/**
		 * <summary>Gets the index number of an Action within EnabledActions.</summary>
		 * <param name = "_action">The Action to search for</param>
		 * <returns>The index number of the Action within EnabledActions</returns>
		 */
		public int GetActionTypeIndex (Action _action)
		{
			string className = _action.GetType ().ToString ();
			className = className.Replace ("AC.", "");
			foreach (ActionType actionType in EnabledActions)
			{
				if (actionType.fileName == className)
				{
					return EnabledActions.IndexOf (actionType);
				}
			}
			return defaultClass;
		}


		/**
		 * <summary>Gets the index number of an Action within EnabledActions.</summary>
		 * <param name = "_category">The category of the Action to search for</param>
		 * <param name = "subCategoryIndex">The index number of the Action in a list of all Actions that share its category</param>
		 * <returns>The index number of the Action within EnabledActions</returns>
		 */
		public int GetActionTypeIndex (ActionCategory _category, int subCategoryIndex)
		{
			List<ActionType> types = new List<ActionType>();
			foreach (ActionType type in EnabledActions)
			{
				if (type.category == _category)
				{
					types.Add (type);
				}
			}
			if (types.Count > subCategoryIndex)
			{
				return EnabledActions.IndexOf (types[subCategoryIndex]);
			}
			return 0;
		}


		/**
		 * <summary>Gets all found Action titles within a given ActionCategory.</summary>
		 * <param name = "_category">The category of the Actions to get the titles of.</param>
		 * <returns>A string array of all Action titles within the ActionCategory</returns>
		 */
		public string[] GetActionSubCategories (ActionCategory _category)
		{
			List<string> titles = new List<string>();

			foreach (ActionType type in EnabledActions)
			{
				if (type.category == _category)
				{
					titles.Add (type.title);
				}
			}
			
			return (titles.ToArray ());
		}
		

		/**
		 * <summary>Gets the ActionCategory of an Action within EnabledActions.</summary>
		 * <param name = "number">The index number of the Action's place in EnabledActions</param>
		 * <returns>The ActionCategory of the Action</returns>
		 */
		public ActionCategory GetActionCategory (int number)
		{
			if (EnabledActions == null || EnabledActions.Count == 0 || EnabledActions.Count < number)
			{
				return 0;
			}
			return EnabledActions[number].category;
		}
		

		/**
		 * <summary>Gets the index of an Action within a list of all Actions that share its category.</summary>
		 * <param name = "_action">The Action to get the index of</param>
		 * <returns>The index of the Action within a list of all Actions that share its category</returns>
		 */
		public int GetActionSubCategory (Action _action)
		{
			string fileName = _action.GetType ().ToString ().Replace ("AC.", "");
			ActionCategory _category = _action.category;
			
			// Learn category
			foreach (ActionType type in EnabledActions)
			{
				if (type.fileName == fileName)
				{
					_category = type.category;
				}
			}
			
			// Learn subcategory
			int i=0;
			foreach (ActionType type in EnabledActions)
			{
				if (type.category == _category)
				{
					if (type.fileName == fileName)
					{
						return i;
					}
					i++;
				}
			}
			
			ACDebug.LogWarning ("Error building Action " + _action);
			return 0;
		}

	}
	
}
