using UnityEngine;
using UnityEditor;

namespace AC
{

	[CustomEditor (typeof (GameCamera2DDrag))]
	public class GameCamera2DDragEditor : Editor
	{

		public override void OnInspectorGUI ()
		{
			GameCamera2DDrag _target = (GameCamera2DDrag) target;

			// X
			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("X movement", EditorStyles.boldLabel, GUILayout.Width (130f));
			_target.xLock = (RotationLock) EditorGUILayout.EnumPopup (_target.xLock);
			EditorGUILayout.EndHorizontal ();
			if (_target.xLock != RotationLock.Locked)
			{
				_target.xSpeed = EditorGUILayout.FloatField ("Speed:", _target.xSpeed);
				_target.xAcceleration = EditorGUILayout.FloatField ("Acceleration:", _target.xAcceleration);
				_target.xDeceleration = EditorGUILayout.FloatField ("Deceleration:", _target.xDeceleration);
				_target.invertX = EditorGUILayout.Toggle ("Invert?", _target.invertX);
				_target.xOffset = EditorGUILayout.FloatField ("Offset:", _target.xOffset);

				if (_target.xLock == RotationLock.Limited)
				{
					_target.minX = EditorGUILayout.FloatField ("Minimum X:", _target.minX);
					_target.maxX = EditorGUILayout.FloatField ("Maximum X:", _target.maxX);
				}
			}
			EditorGUILayout.EndVertical ();

			// Y
			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Y movement", EditorStyles.boldLabel, GUILayout.Width (130f));
			_target.yLock = (RotationLock) EditorGUILayout.EnumPopup (_target.yLock);
			EditorGUILayout.EndHorizontal ();
			if (_target.yLock != RotationLock.Locked)
			{
				_target.ySpeed = EditorGUILayout.FloatField ("Speed:", _target.ySpeed);
				_target.yAcceleration = EditorGUILayout.FloatField ("Acceleration:", _target.yAcceleration);
				_target.yDeceleration = EditorGUILayout.FloatField ("Deceleration:", _target.yDeceleration);
				_target.invertY = EditorGUILayout.Toggle ("Invert?", _target.invertY);
				_target.yOffset = EditorGUILayout.FloatField ("Offset:", _target.yOffset);
				
				if (_target.yLock == RotationLock.Limited)
				{
					_target.minY = EditorGUILayout.FloatField ("Minimum Y:", _target.minY);
					_target.maxY = EditorGUILayout.FloatField ("Maximum Y:", _target.maxY);
				}
			}
			EditorGUILayout.EndVertical ();

			UnityVersionHandler.CustomSetDirty (_target);
		}

	}

}