/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionEndGame.cs"
 * 
 *	This Action will force the game to either
 *	restart an autosave, or quit.
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
	public class ActionEndGame : Action
	{
		
		public enum AC_EndGameType { QuitGame, LoadAutosave, ResetScene, RestartGame };
		public AC_EndGameType endGameType;
		public ChooseSceneBy chooseSceneBy = ChooseSceneBy.Number;
		public int sceneNumber;
		public string sceneName;
		public bool resetMenus;
		
		
		public ActionEndGame ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Engine;
			title = "End game";
			description = "Ends the current game, either by loading an autosave, restarting or quitting the game executable.";
			numSockets = 0;
		}
		
		
		override public float Run ()
		{
			if (endGameType == AC_EndGameType.QuitGame)
			{
				#if UNITY_EDITOR
					UnityEditor.EditorApplication.isPlaying = false;
				#else
					Application.Quit ();
				#endif
			}
			else if (endGameType == AC_EndGameType.LoadAutosave)
			{
				SaveSystem.LoadAutoSave ();
			}
			else
			{
				KickStarter.runtimeInventory.SetNull ();
				KickStarter.runtimeInventory.RemoveRecipes ();

				DestroyImmediate (GameObject.FindWithTag (Tags.player));

				if (endGameType == AC_EndGameType.RestartGame)
				{
					KickStarter.ResetPlayer (KickStarter.settingsManager.GetDefaultPlayer (), KickStarter.settingsManager.GetDefaultPlayerID (), false, Quaternion.identity);

					KickStarter.saveSystem.ClearAllData ();
					KickStarter.levelStorage.ClearAllLevelData ();
					KickStarter.runtimeInventory.OnStart ();
					KickStarter.runtimeVariables.OnStart ();

					if (resetMenus)
					{
						KickStarter.playerMenus.RebuildMenus ();
					}

					KickStarter.stateHandler.CanGlobalOnStart ();
					KickStarter.sceneChanger.ChangeScene (new SceneInfo (chooseSceneBy, sceneName, sceneNumber), false, true);
				}
				else if (endGameType == AC_EndGameType.ResetScene)
				{
					sceneNumber = UnityVersionHandler.GetCurrentSceneNumber ();
					KickStarter.levelStorage.ClearCurrentLevelData ();
					KickStarter.sceneChanger.ChangeScene (new SceneInfo ("", sceneNumber), false, true);
				}
			}

			return 0f;
		}
		
		
		override public ActionEnd End (List<Action> actions)
		{
			return GenerateStopActionEnd ();
		}
		
		
		#if UNITY_EDITOR

		override public void ShowGUI ()
		{
			endGameType = (AC_EndGameType) EditorGUILayout.EnumPopup ("Command:", endGameType);

			if (endGameType == AC_EndGameType.RestartGame)
			{
				chooseSceneBy = (ChooseSceneBy) EditorGUILayout.EnumPopup ("Choose scene by:", chooseSceneBy);
				if (chooseSceneBy == ChooseSceneBy.Name)
				{
					sceneName = EditorGUILayout.TextField ("Scene to restart to:", sceneName);
				}
				else
				{
					sceneNumber = EditorGUILayout.IntField ("Scene to restart to:", sceneNumber);
				}

				resetMenus = EditorGUILayout.Toggle ("Reset Menus too?", resetMenus);
			}
		}
		

		public override string SetLabel ()
		{
			return (" (" + endGameType.ToString () + ")");
		}

		#endif
		
	}

}