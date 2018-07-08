/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionSceneAdd.cs"
 * 
 *	This action adds or removes a scene without affecting any other open scenes.
 * 
 */

using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class ActionSceneAdd : Action
	{

		public enum SceneAddRemove { Add, Remove };
		public SceneAddRemove sceneAddRemove = SceneAddRemove.Add;
		public bool runCutsceneOnStart;

		public ChooseSceneBy chooseSceneBy = ChooseSceneBy.Number;
		public int sceneNumber;
		public int sceneNumberParameterID = -1;
		public string sceneName;
		public int sceneNameParameterID = -1;

		private bool waitedOneMoreFrame = false;


		public ActionSceneAdd ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Scene;
			title = "Add or remove";
			description = "Adds or removes a scene without affecting any other open scenes.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			sceneNumber = AssignInteger (parameters, sceneNumberParameterID, sceneNumber);
			sceneName = AssignString (parameters, sceneNameParameterID, sceneName);
		}
		
		
		override public float Run ()
		{
			SceneInfo sceneInfo = new SceneInfo (chooseSceneBy, AdvGame.ConvertTokens (sceneName), sceneNumber);

			if (!isRunning)
			{
				waitedOneMoreFrame = false;
				isRunning = true;

				if (KickStarter.sceneSettings.OverridesCameraPerspective ())
				{
					ACDebug.LogError ("The current scene overrides the default camera perspective - this feature should not be used in conjunction with multiple-open scenes.");
				}

				if (sceneAddRemove == SceneAddRemove.Add)
				{
					if (KickStarter.sceneChanger.AddSubScene (sceneInfo))
					{
						return defaultPauseTime;
					}
				}
				else if (sceneAddRemove == SceneAddRemove.Remove)
				{
					KickStarter.sceneChanger.RemoveScene (sceneInfo);
				}
			}
			else
			{
				if (!waitedOneMoreFrame)
				{
					waitedOneMoreFrame = true;
					return defaultPauseTime;
				}

				if (sceneAddRemove == SceneAddRemove.Add)
				{
					bool found = false;
					foreach (SubScene subScene in KickStarter.sceneChanger.GetSubScenes ())
					{
						if (subScene.SceneInfo.Matches (sceneInfo))
						{
							found = true;

							if (runCutsceneOnStart && subScene.SceneSettings != null && subScene.SceneSettings.cutsceneOnStart != null)
							{
								subScene.SceneSettings.cutsceneOnStart.Interact ();
							}
						}
					}

					if (!found)
					{
						ACDebug.LogWarning ("Adding a non-AC scene additively!  A GameEngine prefab must be placed in scene '" + sceneInfo.GetLabel () + "'.");
					}
				}

				isRunning = false;
			}

			return 0f;
		}


		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			#if UNITY_5_3 || UNITY_5_4 || UNITY_5_3_OR_NEWER
			sceneAddRemove = (SceneAddRemove) EditorGUILayout.EnumPopup ("Method:", sceneAddRemove);

			chooseSceneBy = (ChooseSceneBy) EditorGUILayout.EnumPopup ("Choose scene by:", chooseSceneBy);
			if (chooseSceneBy == ChooseSceneBy.Name)
			{
				sceneNameParameterID = Action.ChooseParameterGUI ("Scene name:", parameters, sceneNameParameterID, ParameterType.String);
				if (sceneNameParameterID < 0)
				{
					sceneName = EditorGUILayout.TextField ("Scene name:", sceneName);
				}
			}
			else
			{
				sceneNumberParameterID = Action.ChooseParameterGUI ("Scene number:", parameters, sceneNumberParameterID, ParameterType.Integer);
				if (sceneNumberParameterID < 0)
				{
					sceneNumber = EditorGUILayout.IntField ("Scene number:", sceneNumber);
				}
			}

			if (sceneAddRemove == SceneAddRemove.Add)
			{
				runCutsceneOnStart = EditorGUILayout.Toggle ("Run 'Cutscene on start'?", runCutsceneOnStart);
			}
			#else
			EditorGUILayout.HelpBox ("This Action is only available for Unity 5.3 or greater.", MessageType.Info);
			#endif

			AfterRunningOption ();
		}


		override public string SetLabel ()
		{
			if (chooseSceneBy == ChooseSceneBy.Name)
			{
				return (" (" + sceneAddRemove.ToString () + " " + sceneName + ")");
			}
			return (" (" + sceneAddRemove.ToString () + " " + sceneNumber + ")");
		}

		#endif
		
	}

}