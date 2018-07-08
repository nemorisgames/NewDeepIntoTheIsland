using UnityEditor;

namespace AC
{

	[CustomEditor(typeof(PlayerStart))]
	public class PlayerStartEditor : Editor
	{

		public override void OnInspectorGUI ()
		{
			PlayerStart _target = (PlayerStart) target;

			if (KickStarter.sceneSettings != null && KickStarter.sceneSettings.defaultPlayerStart == _target)
			{
				EditorGUILayout.HelpBox ("This PlayerStart is the scene's default, and will be used if a more appropriate one is not found.", MessageType.Info);
			}

			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Previous scene that activates", EditorStyles.boldLabel);
			_target.chooseSceneBy = (ChooseSceneBy) EditorGUILayout.EnumPopup ("Choose scene by:", _target.chooseSceneBy);
			if (_target.chooseSceneBy == ChooseSceneBy.Name)
			{
				_target.previousSceneName = EditorGUILayout.TextField ("Previous scene:", _target.previousSceneName);
			}
			else
			{
				_target.previousScene = EditorGUILayout.IntField ("Previous scene:", _target.previousScene);
			}
			EditorGUILayout.EndVertical ();

			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Camera settings", EditorStyles.boldLabel);
			_target.cameraOnStart = (_Camera) EditorGUILayout.ObjectField ("Camera on start:", _target.cameraOnStart, typeof (_Camera), true);
			_target.fadeInOnStart = EditorGUILayout.Toggle ("Fade in on start?", _target.fadeInOnStart);
			if (_target.fadeInOnStart)
			{
				_target.fadeSpeed = EditorGUILayout.FloatField ("Fade speed:", _target.fadeSpeed);
			}
			EditorGUILayout.EndVertical ();

			UnityVersionHandler.CustomSetDirty (_target);
		}

	}

}