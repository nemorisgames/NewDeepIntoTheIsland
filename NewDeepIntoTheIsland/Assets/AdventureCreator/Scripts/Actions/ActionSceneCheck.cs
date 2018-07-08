/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionSceneCheck.cs"
 * 
 *	This action checks the player's last-visited scene,
 *	useful for running specific "player enters the room" cutscenes.
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
	public class ActionSceneCheck : ActionCheck
	{
		
		public enum IntCondition { EqualTo, NotEqualTo };
		public enum SceneToCheck { Current, Previous };
		public ChooseSceneBy chooseSceneBy = ChooseSceneBy.Number;
		public SceneToCheck sceneToCheck = SceneToCheck.Current;

		public int sceneNumberParameterID = -1;
		public int sceneNumber;

		public int sceneNameParameterID = -1;
		public string sceneName;

		public IntCondition intCondition;


		public ActionSceneCheck ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Scene;
			title = "Check";
			description = "Queries either the current scene, or the last one visited.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			sceneNumber = AssignInteger (parameters, sceneNumberParameterID, sceneNumber);
			sceneName = AssignString (parameters, sceneNameParameterID, sceneName);
		}

		
		override public bool CheckCondition ()
		{
			int actualSceneNumber = 0;
			string actualSceneName = "";

			if (sceneToCheck == SceneToCheck.Previous)
			{
				actualSceneNumber = KickStarter.sceneChanger.previousSceneInfo.number;
				actualSceneName = KickStarter.sceneChanger.previousSceneInfo.name;
			}
			else
			{
				actualSceneNumber = UnityVersionHandler.GetCurrentSceneNumber ();
				actualSceneName = UnityVersionHandler.GetCurrentSceneName ();
			}

			if (intCondition == IntCondition.EqualTo)
			{
				if (chooseSceneBy == ChooseSceneBy.Name && actualSceneName == AdvGame.ConvertTokens (sceneName))
				{
					return true;
				}

				if (chooseSceneBy == ChooseSceneBy.Number && actualSceneNumber == sceneNumber)
				{
					return true;
				}
			}
			
			else if (intCondition == IntCondition.NotEqualTo)
			{
				if (chooseSceneBy == ChooseSceneBy.Name && actualSceneName != AdvGame.ConvertTokens (sceneName))
				{
					return true;
				}

				if (chooseSceneBy == ChooseSceneBy.Number && actualSceneNumber != sceneNumber)
				{
					return true;
				}
			}
			
			return false;
		}

		
		#if UNITY_EDITOR

		override public void ShowGUI (List<ActionParameter> parameters)
		{
			sceneToCheck = (SceneToCheck) EditorGUILayout.EnumPopup ("Check previous or current:", sceneToCheck);
			chooseSceneBy = (ChooseSceneBy) EditorGUILayout.EnumPopup ("Choose scene by:", chooseSceneBy);

			EditorGUILayout.BeginHorizontal ();
			if (chooseSceneBy == ChooseSceneBy.Name)
			{
				EditorGUILayout.LabelField ("Scene name is:", GUILayout.Width (100f));
				intCondition = (IntCondition) EditorGUILayout.EnumPopup (intCondition);

				sceneNameParameterID = Action.ChooseParameterGUI ("", parameters, sceneNameParameterID, ParameterType.String);
				if (sceneNameParameterID < 0)
				{
					sceneName = EditorGUILayout.TextField (sceneName);
				}
			}
			else
			{
				EditorGUILayout.LabelField ("Scene number is:", GUILayout.Width (100f));
				intCondition = (IntCondition) EditorGUILayout.EnumPopup (intCondition);

				sceneNumberParameterID = Action.ChooseParameterGUI ("", parameters, sceneNumberParameterID, ParameterType.Integer);
				if (sceneNumberParameterID < 0)
				{
					sceneNumber = EditorGUILayout.IntField (sceneNumber);
				}
			}
			EditorGUILayout.EndHorizontal ();
		}

		#endif

	}

}