using UnityEngine;
using UnityEditor;

namespace AC
{

	[CustomEditor(typeof(DragTrack_Straight))]
	public class DragTrack_StraightEditor : DragTrackEditor
	{
		
		public override void OnInspectorGUI ()
		{
			DragTrack_Straight _target = (DragTrack_Straight) target;
			
			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Track shape:", EditorStyles.boldLabel);
			
			_target.maxDistance = EditorGUILayout.FloatField ("Length:", _target.maxDistance);
			_target.handleColour = EditorGUILayout.ColorField ("Handles colour:", _target.handleColour);
			_target.rotationType = (DragRotationType) EditorGUILayout.EnumPopup ("Rotation type:", _target.rotationType);

			if (_target.rotationType == DragRotationType.Screw)
			{
				_target.screwThread = EditorGUILayout.FloatField ("Screw thread:", _target.screwThread);
				_target.dragMustScrew = EditorGUILayout.Toggle ("Drag must rotate too?", _target.dragMustScrew);
			}

			EditorGUILayout.EndVertical ();

			SharedGUI (true);
		}
		
		
		public void OnSceneGUI ()
		{
			DragTrack_Straight _target = (DragTrack_Straight) target;
			
			Handles.color = _target.handleColour;
			Vector3 maxPosition = _target.transform.position + (_target.transform.up * _target.maxDistance);
			maxPosition = Handles.PositionHandle (maxPosition, Quaternion.identity);
			Handles.DrawSolidDisc (maxPosition, -_target.transform.up, _target.discSize);
			_target.maxDistance = Vector3.Dot (maxPosition - _target.transform.position, _target.transform.up);
			
			Handles.color = new Color (_target.handleColour.r / 2f, _target.handleColour.g / 2f, _target.handleColour.b / 2f, _target.handleColour.a);
			Vector3 minPosition = _target.transform.position;
			Handles.DrawSolidDisc (minPosition, _target.transform.up, _target.discSize);
			
			Handles.color = _target.handleColour;
			Handles.DrawLine (minPosition, maxPosition);

			UnityVersionHandler.CustomSetDirty (_target);
		}
		
	}

}