using UnityEngine;
using UnityEditor;

namespace AC
{

	[CustomEditor(typeof(AC_Trigger))]
	[System.Serializable]
	public class AC_TriggerEditor : ActionListEditor
	{

		public override void OnInspectorGUI ()
		{
			AC_Trigger _target = (AC_Trigger) target;
			PropertiesGUI (_target);
			base.DrawSharedElements (_target);

			UnityVersionHandler.CustomSetDirty (_target);
		}


		public static void PropertiesGUI (AC_Trigger _target)
		{
			string[] Options = { "On enter", "Continuous", "On exit" };

			if (Application.isPlaying)
			{
				if (!_target.IsOn ())
				{
					EditorGUILayout.HelpBox ("Current state: OFF", MessageType.Info);
				}
			}

			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Trigger properties", EditorStyles.boldLabel);
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
			_target.triggerType = EditorGUILayout.Popup ("Trigger type:", _target.triggerType, Options);
			_target.triggerReacts = (TriggerReacts) EditorGUILayout.EnumPopup ("Reacts:", _target.triggerReacts);
			_target.cancelInteractions = EditorGUILayout.Toggle ("Cancels interactions?", _target.cancelInteractions);
			_target.tagID = ShowTagUI (_target.actions.ToArray (), _target.tagID);
			_target.useParameters = EditorGUILayout.Toggle ("Set collider as parameter?", _target.useParameters);
			
			EditorGUILayout.Space ();
			_target.detects = (TriggerDetects) EditorGUILayout.EnumPopup ("Trigger detects:", _target.detects);
			if (_target.detects == TriggerDetects.AnyObjectWithComponent)
			{
				_target.detectComponent = EditorGUILayout.TextField ("Component name:", _target.detectComponent);
				EditorGUILayout.HelpBox ("Multiple component names should be separated by a colon ';'", MessageType.Info);
			}
			else if (_target.detects == TriggerDetects.AnyObjectWithTag)
			{
				_target.detectComponent = EditorGUILayout.TextField ("Tag name:", _target.detectComponent);
				EditorGUILayout.HelpBox ("Multiple tags should be separated by a colon ';'", MessageType.Info);
			}
			else if (_target.detects == TriggerDetects.SetObject)
			{
				_target.obToDetect = (GameObject) EditorGUILayout.ObjectField ("Object to detect:", _target.obToDetect, typeof (GameObject), true);
			}
			EditorGUILayout.EndVertical ();

			if (_target.useParameters)
			{
				if (_target.parameters.Count != 1)
				{
					ActionParameter newParameter = new ActionParameter (0);
					newParameter.parameterType = ParameterType.GameObject;
					newParameter.label = "Collision object";
					_target.parameters.Clear ();
					_target.parameters.Add (newParameter);
				}
			}
	    }

	}

}