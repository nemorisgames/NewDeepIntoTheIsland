/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionPauseActionList.cs"
 * 
 *	This action pauses and resumes ActionLists.
 * 
 */

using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	[System.Serializable]
	public class ActionPauseActionList : Action
	{

		public enum PauseResume { Pause, Resume };
		public PauseResume pauseResume = PauseResume.Pause;

		public ActionRunActionList.ListSource listSource = ActionRunActionList.ListSource.InScene;
		public ActionListAsset actionListAsset;

		public ActionList actionList;
		public int constantID = 0;
		public int parameterID = -1;

		private RuntimeActionList[] runtimeActionLists = new RuntimeActionList[0];

		
		public ActionPauseActionList ()
		{
			this.isDisplayed = true;
			category = ActionCategory.ActionList;
			title = "Pause or resume";
			description = "Pauses and resumes ActionLists.";
		}
		
		
		override public void AssignValues (List<ActionParameter> parameters)
		{
			if (listSource == ActionRunActionList.ListSource.InScene)
			{
				actionList = AssignFile <ActionList> (parameters, parameterID, constantID, actionList);
			}
		}
		
		
		override public float Run ()
		{
			if (!isRunning)
			{
				isRunning = true;
				runtimeActionLists = new RuntimeActionList[0];

				if (pauseResume == PauseResume.Pause)
				{
					if (listSource == ActionRunActionList.ListSource.AssetFile && actionListAsset != null && !actionListAsset.actions.Contains (this))
					{
						runtimeActionLists = KickStarter.actionListAssetManager.Pause (actionListAsset);

						if (willWait && runtimeActionLists.Length > 0)
						{
							return defaultPauseTime;
						}
					}
					else if (listSource == ActionRunActionList.ListSource.InScene && actionList != null && !actionList.actions.Contains (this))
					{
						actionList.Pause ();

						if (willWait)
						{
							return defaultPauseTime;
						}
					}
				}
				else if (pauseResume == PauseResume.Resume)
				{
					if (listSource == ActionRunActionList.ListSource.AssetFile && actionListAsset != null && !actionListAsset.actions.Contains (this))
					{
						KickStarter.actionListAssetManager.Resume (actionListAsset);
					}
					else if (listSource == ActionRunActionList.ListSource.InScene && actionList != null && !actionList.actions.Contains (this))
					{
						KickStarter.actionListManager.Resume (actionList);
					}
				}
			}
			else
			{
				if (listSource == ActionRunActionList.ListSource.AssetFile)
				{
					foreach (RuntimeActionList runtimeActionList in runtimeActionLists)
					{
						if (runtimeActionList != null && KickStarter.actionListManager.IsListRunning (runtimeActionList))
						{
							return defaultPauseTime;
						}
					}
				}
				else if (listSource == ActionRunActionList.ListSource.InScene)
				{
					if (KickStarter.actionListManager.IsListRunning (actionList))
					{
						return defaultPauseTime;
					}
				}

				isRunning = false;
				return 0f;
			}

			return 0f;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			pauseResume = (PauseResume) EditorGUILayout.EnumPopup ("Method:", pauseResume);

			listSource = (ActionRunActionList.ListSource) EditorGUILayout.EnumPopup ("Source:", listSource);
			if (listSource == ActionRunActionList.ListSource.InScene)
			{
				actionList = (ActionList) EditorGUILayout.ObjectField ("ActionList:", actionList, typeof (ActionList), true);
				
				constantID = FieldToID <ActionList> (actionList, constantID);
				actionList = IDToField <ActionList> (actionList, constantID, true);

				if (actionList != null && actionList.actions.Contains (this))
				{
					EditorGUILayout.HelpBox ("An ActionList cannot " + pauseResume.ToString () + " itself - it must be performed indirectly.", MessageType.Warning);
				}
			}
			else if (listSource == ActionRunActionList.ListSource.AssetFile)
			{
				actionListAsset = (ActionListAsset) EditorGUILayout.ObjectField ("ActionList asset:", actionListAsset, typeof (ActionListAsset), false);

				if (actionListAsset != null && actionListAsset.actions.Contains (this))
				{
					EditorGUILayout.HelpBox ("An ActionList Asset cannot " + pauseResume.ToString () + " itself - it must be performed indirectly.", MessageType.Warning);
				}
			}
			
			if (pauseResume == PauseResume.Pause)
			{
				willWait = EditorGUILayout.Toggle ("Wait until finish?", willWait);
				if (willWait)
				{
					EditorGUILayout.HelpBox ("The ActionList will complete any currently-running Actions before it pauses.", MessageType.Info);
				}
			}

			AfterRunningOption ();
		}


		override public void AssignConstantIDs (bool saveScriptsToo)
		{
			if (listSource == ActionRunActionList.ListSource.InScene)
			{
				AssignConstantID <ActionList> (actionList, constantID, parameterID);
			}
		}
		
		
		public override string SetLabel ()
		{
			string labelAdd = "";
			
			if (listSource == ActionRunActionList.ListSource.InScene && actionList != null)
			{
				labelAdd += " (" + pauseResume.ToString () + " " + actionList.name + ")";
			}
			else if (listSource == ActionRunActionList.ListSource.AssetFile && actionList != null)
			{
				labelAdd += " (" + pauseResume.ToString () + " " + actionList.name + ")";
			}
			
			return labelAdd;
		}
		
		#endif
		
	}
	
}