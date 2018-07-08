/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionListAsset.cs"
 * 
 *	This script stores a list of Actions in an asset file.
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
	 * An ActionListAsset is a ScriptableObject that allows a List of Action objects to be stored within an asset file.
	 * When the file is run, the Actions are transferred to a local instance of RuntimeActionList and run from there.
	 */
	[System.Serializable]
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_action_list_asset.html")]
	#endif
	public class ActionListAsset : ScriptableObject
	{

		/** The Actions within this asset file */
		public List<AC.Action> actions = new List<AC.Action>();
		/** If True, the Actions will be skipped when the user presses the 'EndCutscene' Input button */
		public bool isSkippable = true;
		/** The effect that running the Actions has on the rest of the game (PauseGameplay, RunInBackground) */
		public ActionListType actionListType = ActionListType.PauseGameplay;
		/** If True, the game will un-freeze itself while the Actions run if the game was previously paused due to an enabled Menu */
		public bool unfreezePauseMenus = true;
		/** If True, ActionParameters can be used to override values within the Action objects */
		public bool useParameters = false;
		/** If True, then multiple instances of this asset can be run simultaneously in the scene */
		public bool canRunMultipleInstances = false;
		/** A List of ActionParameter objects that can be used to override values within the Actions, if useParameters = True */
		public List<ActionParameter> parameters = new List<ActionParameter>();
		/** The ID of the associated SpeechTag */
		[HideInInspector] public int tagID;


		#if UNITY_EDITOR

		[MenuItem("CONTEXT/ActionListAsset/Convert to Cutscene")]
		public static void ConvertToCutscene (MenuCommand command)
		{
			ActionListAsset actionListAsset = (ActionListAsset) command.context;
			GameObject newOb = new GameObject (actionListAsset.name);
			if (GameObject.Find ("_Cutscenes") != null && GameObject.Find ("_Cutscenes").transform.position == Vector3.zero)
			{
				newOb.transform.parent = GameObject.Find ("_Cutscenes").transform;
			}
			Cutscene cutscene = newOb.AddComponent <Cutscene>();
			cutscene.CopyFromAsset (actionListAsset);
			EditorGUIUtility.PingObject (newOb);
		}


		[MenuItem("CONTEXT/ActionList/Convert to ActionList asset")]
		public static void ConvertToActionListAsset (MenuCommand command)
		{
			ActionList actionList = (ActionList) command.context;
			ScriptableObject t = CustomAssetUtility.CreateAsset <ActionListAsset> (actionList.gameObject.name);

			ActionListAsset actionListAsset = (ActionListAsset) t;
			actionListAsset.CopyFromActionList (actionList);
			AssetDatabase.SaveAssets ();
			EditorGUIUtility.PingObject (t);

			EditorUtility.SetDirty (actionListAsset);
		}


		public void CopyFromActionList (ActionList actionList)
		{
			isSkippable = actionList.isSkippable;
			actionListType = actionList.actionListType;
			useParameters = actionList.useParameters;
			
			// Copy parameters
			parameters = new List<ActionParameter>();
			parameters.Clear ();
			foreach (ActionParameter parameter in actionList.parameters)
			{
				parameters.Add (new ActionParameter (parameter));
			}
			
			// Actions
			actions = new List<Action>();
			actions.Clear ();
			
			Vector2 firstPosition = new Vector2 (14f, 14f);
			foreach (Action originalAction in actionList.actions)
			{
				if (originalAction == null)
				{
					continue;
				}
				
				AC.Action duplicatedAction = Object.Instantiate (originalAction) as AC.Action;
				
				if (actionList.actions.IndexOf (originalAction) == 0)
				{
					duplicatedAction.nodeRect.x = firstPosition.x;
					duplicatedAction.nodeRect.y = firstPosition.y;
				}
				else
				{
					duplicatedAction.nodeRect.x = firstPosition.x + (originalAction.nodeRect.x - firstPosition.x);
					duplicatedAction.nodeRect.y = firstPosition.y + (originalAction.nodeRect.y - firstPosition.y);
				}

				duplicatedAction.isAssetFile = true;
				duplicatedAction.AssignConstantIDs ();
				duplicatedAction.isMarked = false;
				duplicatedAction.ClearIDs ();

				duplicatedAction.hideFlags = HideFlags.HideInHierarchy;
				
				AssetDatabase.AddObjectToAsset (duplicatedAction, this);
				AssetDatabase.ImportAsset (AssetDatabase.GetAssetPath (duplicatedAction));
				AssetDatabase.SaveAssets ();
				AssetDatabase.Refresh ();

				actions.Add (duplicatedAction);
			}
		}

		#endif

		/**
		 * <summary>Checks if the ActionListAsset is skippable. This is safer than just reading 'isSkippable', because it also accounts for actionListType - since ActionLists that run in the background cannot be skipped</summary>
		 * <returns>True if the ActionListAsset is skippable</returns>
		 */
		public bool IsSkippable ()
		{
			if (isSkippable && actionListType == ActionListType.PauseGameplay)
			{
				return true;
			}
			return false;
		}


		/**
		 * <summary>Runs the ActionList asset file</summary>
		 */
		public void Interact ()
		{
			AdvGame.RunActionListAsset (this);
		}


		/**
		 * <summary>Runs the ActionList asset file from a set point.</summary>
		 * <param name = "index">The index number of actions to start from</param>
		 */
		public void RunFromIndex (int index)
		{
			AdvGame.RunActionListAsset (this, index, true);
		}


		/**
		 * <summary>Runs the ActionList asset file, after setting the value of an integer parameter if it has one.</summary>
		 * <param name = "parameterID">The ID of the Integer parameter to set</param>
		 * <param name = "parameterValue">The value to set the Integer parameter to</param>
		 */
		public RuntimeActionList Interact (int parameterID, int parameterValue)
		{
			return AdvGame.RunActionListAsset (this, parameterID, parameterValue);
		}


		/**
		 * <summary>Kills all currently-running instances of the asset.</summary>
		 */
		public void KillAllInstances ()
		{
			if (KickStarter.actionListAssetManager != null)
			{
				int numRemoved = KickStarter.actionListAssetManager.EndAssetList (this, null, true);
				ACDebug.Log ("Ended " + numRemoved + " instances of the ActionList Asset '" + this.name + "'", this);
			}
		}

	}


	public class ActionListAssetMenu
	{

		#if UNITY_EDITOR
	
		[MenuItem ("Assets/Create/Adventure Creator/ActionList")]
		public static ActionListAsset CreateAsset ()
		{
			string assetName = "New ActionList";

			ScriptableObject t = CustomAssetUtility.CreateAsset <ActionListAsset> (assetName);
			EditorGUIUtility.PingObject (t);
			ACDebug.Log ("Created ActionList: " + assetName);
			return (ActionListAsset) t;
		}


		public static ActionListAsset CreateAsset (string assetName)
		{
			if (string.IsNullOrEmpty (assetName))
			{
				return CreateAsset ();
			}

			ScriptableObject t = CustomAssetUtility.CreateAsset <ActionListAsset> (assetName);
			EditorGUIUtility.PingObject (t);
			ACDebug.Log ("Created ActionList: " + assetName);
			return (ActionListAsset) t;
		}


		public static ActionListAsset AssetGUI (string label, ActionListAsset actionListAsset, string api = "", string defaultName = "")
		{
			EditorGUILayout.BeginHorizontal ();
			actionListAsset = (ActionListAsset) CustomGUILayout.ObjectField <ActionListAsset> (label, actionListAsset, false, api);

			if (actionListAsset == null)
			{
				if (GUILayout.Button ("Create", GUILayout.MaxWidth (60f)))
				{
					#if !(UNITY_WP8 || UNITY_WINRT)
					defaultName = System.Text.RegularExpressions.Regex.Replace (defaultName, "[^\\w\\_]", "");
					#else
					defaultName = "";
					#endif

					actionListAsset = ActionListAssetMenu.CreateAsset (defaultName);
				}
			}

			EditorGUILayout.EndHorizontal ();
			return actionListAsset;
		}


		public static Cutscene CutsceneGUI (string label, Cutscene cutscene)
		{
			EditorGUILayout.BeginHorizontal ();
			cutscene = (Cutscene) EditorGUILayout.ObjectField (label, cutscene, typeof (Cutscene), true);

			if (cutscene == null)
			{
				if (GUILayout.Button ("Create", GUILayout.MaxWidth (60f)))
				{
					cutscene = SceneManager.AddPrefab ("Logic", "Cutscene", true, false, true).GetComponent <Cutscene>();
					cutscene.Initialise ();
				}
			}

			EditorGUILayout.EndHorizontal ();
			return cutscene;
		}

		#endif


	}

}