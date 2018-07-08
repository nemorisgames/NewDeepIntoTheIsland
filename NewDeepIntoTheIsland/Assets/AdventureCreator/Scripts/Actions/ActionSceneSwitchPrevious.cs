/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionScene.cs"
 * 
 *	This action loads a new scene.
 * 
 */

using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class ActionSceneSwitchPrevious : Action
	{
		
		public bool assignScreenOverlay;
		public bool onlyPreload = false;

		public bool relativePosition = false;
		public Marker relativeMarker;
		public int relativeMarkerID;
		public int relativeMarkerParameterID = -1;


		public ActionSceneSwitchPrevious ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Scene;
			title = "Switch previous";
			description = "Moves the Player to the previously-loaded scene. The scene must be listed in Unity's Build Settings. By default, the screen will cut to black during the transition, but the last frame of the current scene can instead be overlayed. This allows for cinematic effects: if the next scene fades in, it will cause a crossfade effect; if the next scene doesn't fade, it will cause a straight cut.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			relativeMarker = AssignFile <Marker> (parameters, relativeMarkerParameterID, relativeMarkerID, relativeMarker);
		}
		
		
		override public float Run ()
		{
			if (!assignScreenOverlay || (!relativePosition && onlyPreload))
			{
				ChangeScene ();
				return 0f;
			}

			if (!isRunning)
			{
				if (KickStarter.sceneChanger.previousSceneInfo == null || KickStarter.sceneChanger.previousSceneInfo.IsNull)
				{
					ACDebug.LogWarning ("Cannot load previous scene as there is no data stored - is this the first scene in the game?");
					return 0f;
				}

				isRunning = true;
				KickStarter.mainCamera._ExitSceneWithOverlay ();
				return defaultPauseTime;
			}
			else
			{
				ChangeScene ();
				isRunning = false;
				return 0f;
			}
		}


		override public void Skip ()
		{
			ChangeScene ();
		}


		private void ChangeScene ()
		{
			SceneInfo sceneInfo = KickStarter.sceneChanger.previousSceneInfo;
			if (sceneInfo == null || sceneInfo.IsNull)
			{
				ACDebug.LogWarning ("Cannot load previous scene as there is no data stored - is this the first scene in the game?");
				return;
			}

			if (!onlyPreload && relativePosition && relativeMarker != null)
			{
				KickStarter.sceneChanger.SetRelativePosition (relativeMarker.transform);
			}

			if (onlyPreload && !relativePosition)
			{
				if (AdvGame.GetReferences ().settingsManager.useAsyncLoading)
				{
					KickStarter.sceneChanger.PreloadScene (sceneInfo);
				}
				else
				{
					ACDebug.LogWarning ("To pre-load scenes, 'Load scenes asynchronously?' must be enabled in the Settings Manager.");
				}
			}
			else
			{
				KickStarter.sceneChanger.ChangeScene (sceneInfo, true);
			}
		}


		override public ActionEnd End (List<Action> actions)
		{
			if (onlyPreload && !relativePosition)
			{
				return base.End (actions);
			}
			return GenerateStopActionEnd ();
		}
		

		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			onlyPreload = EditorGUILayout.ToggleLeft ("Don't change scene, just preload data?", onlyPreload);

			if (!onlyPreload)
			{
				relativePosition = EditorGUILayout.ToggleLeft ("Position Player relative to Marker?", relativePosition);
				if (relativePosition)
				{
					relativeMarkerParameterID = Action.ChooseParameterGUI ("Relative Marker:", parameters, relativeMarkerParameterID, ParameterType.GameObject);
					if (relativeMarkerParameterID >= 0)
					{
						relativeMarkerID = 0;
						relativeMarker = null;
					}
					else
					{
						relativeMarker = (Marker) EditorGUILayout.ObjectField ("Relative Marker:", relativeMarker, typeof(Marker), true);
						
						relativeMarkerID = FieldToID (relativeMarker, relativeMarkerID);
						relativeMarker = IDToField (relativeMarker, relativeMarkerID, false);
					}
				}
			}

			if (onlyPreload && !relativePosition)
			{
				if (AdvGame.GetReferences () != null && AdvGame.GetReferences ().settingsManager != null && AdvGame.GetReferences ().settingsManager.useAsyncLoading)
				{}
				else
				{
					EditorGUILayout.HelpBox ("To pre-load scenes, 'Load scenes asynchronously?' must be enabled in the Settings Manager.", MessageType.Warning);
				}

				numSockets = 1;
				AfterRunningOption ();
			}
			else
			{
				numSockets = 0;
				assignScreenOverlay = EditorGUILayout.ToggleLeft ("Overlay current screen during switch?", assignScreenOverlay);
			}
		}


		override public void AssignConstantIDs (bool saveScriptsToo)
		{
			AssignConstantID (relativeMarker, relativeMarkerID, relativeMarkerParameterID);
		}
		
		#endif
		
	}

}