using UnityEngine;
using UnityEditor;

namespace AC
{

	[CustomEditor (typeof (AlignToCamera))]
	public class AlignToCameraEditor : Editor
	{
		
		private AlignToCamera _target;
		
		
		private void OnEnable ()
		{
			_target = (AlignToCamera) target;
		}
		
		
		public override void OnInspectorGUI ()
		{
			_target.cameraToAlignTo = (_Camera) EditorGUILayout.ObjectField ("Camera to align to:", _target.cameraToAlignTo, typeof (_Camera), true);

			if (_target.cameraToAlignTo)
			{
				_target.alignType = (AlignType) EditorGUILayout.EnumPopup ("Align type:", _target.alignType);
				_target.lockDistance = EditorGUILayout.BeginToggleGroup ("Lock distance?", _target.lockDistance);
				_target.distanceToCamera = EditorGUILayout.FloatField ("Distance from camera:", _target.distanceToCamera);
				_target.lockScale = EditorGUILayout.Toggle ("Lock perceived scale?", _target.lockScale);
				EditorGUILayout.EndToggleGroup ();

				if (GUILayout.Button ("Centre to camera"))
				{
					Undo.RecordObject (_target, "Centre to camera");
					_target.CentreToCamera ();
				}
			}

			UnityVersionHandler.CustomSetDirty (_target);
		}

	}

}