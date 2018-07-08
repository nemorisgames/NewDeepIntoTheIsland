using UnityEditor;

namespace AC
{

	[CustomEditor (typeof(Interaction))]
	[System.Serializable]
	public class InteractionEditor : ActionListEditor
	{

		public override void OnInspectorGUI ()
		{
			Interaction _target = (Interaction) target;
			PropertiesGUI (_target);
			base.DrawSharedElements (_target);
			
			UnityVersionHandler.CustomSetDirty (_target);
		}


		public static void PropertiesGUI (Interaction _target)
	    {
			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Interaction properties", EditorStyles.boldLabel);
			_target.source = (ActionListSource) EditorGUILayout.EnumPopup ("Actions source:", _target.source);
			if (_target.source == ActionListSource.AssetFile)
			{
				_target.assetFile = (ActionListAsset) EditorGUILayout.ObjectField ("ActionList asset:", _target.assetFile, typeof (ActionListAsset), false);
				_target.syncParamValues = EditorGUILayout.Toggle ("Sync parameter values?", _target.syncParamValues);
			}
			_target.actionListType = (ActionListType) EditorGUILayout.EnumPopup ("When running:", _target.actionListType);
			if (_target.actionListType == ActionListType.PauseGameplay)
			{
				_target.isSkippable = EditorGUILayout.Toggle ("Is skippable?", _target.isSkippable);
			}
			_target.tagID = ShowTagUI (_target.actions.ToArray (), _target.tagID);
			if (_target.source == ActionListSource.InScene)
			{
				_target.useParameters = EditorGUILayout.Toggle ("Use parameters?", _target.useParameters);
			}
			else if (_target.source == ActionListSource.AssetFile && _target.assetFile != null && !_target.syncParamValues && _target.assetFile.useParameters)
			{
				_target.useParameters = EditorGUILayout.Toggle ("Set local parameter values?", _target.useParameters);
			}
			EditorGUILayout.EndVertical ();

			if (_target.useParameters)
			{
				if (_target.source == ActionListSource.InScene)
				{
					EditorGUILayout.Space ();
					EditorGUILayout.BeginVertical ("Button");

					EditorGUILayout.LabelField ("Parameters", EditorStyles.boldLabel);
					ShowParametersGUI (_target, null, _target.parameters);

					EditorGUILayout.EndVertical ();
				}
				else if (!_target.syncParamValues && _target.source == ActionListSource.AssetFile && _target.assetFile != null && _target.assetFile.useParameters)
				{
					bool isAsset = (PrefabUtility.GetPrefabType (_target) == PrefabType.Prefab) ? true : false;

					EditorGUILayout.Space ();
					EditorGUILayout.BeginVertical ("Button");

					EditorGUILayout.LabelField ("Local parameter values", EditorStyles.boldLabel);
					ShowLocalParametersGUI (_target.parameters, _target.assetFile.parameters, isAsset);

					EditorGUILayout.EndVertical ();
				}
					
			}
	    }

	}

}