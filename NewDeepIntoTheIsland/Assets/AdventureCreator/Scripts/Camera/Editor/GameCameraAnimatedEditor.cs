using UnityEngine;
using UnityEditor;

namespace AC
{

	[CustomEditor(typeof(GameCameraAnimated))]
	public class GameCameraAnimatedEditor : Editor
	{
		
		public override void OnInspectorGUI()
		{
			GameCameraAnimated _target = (GameCameraAnimated) target;

			if (_target.GetComponent <Animation>() == null)
			{
				EditorGUILayout.HelpBox ("This camera type requires an Animation component.", MessageType.Warning);
			}

			EditorGUILayout.BeginVertical ("Button");
				_target.animatedCameraType = (AnimatedCameraType) EditorGUILayout.EnumPopup ("Animated camera type:", _target.animatedCameraType);
				_target.clip = (AnimationClip) EditorGUILayout.ObjectField ("Animation clip:", _target.clip, typeof (AnimationClip), false);

				if (_target.animatedCameraType == AnimatedCameraType.PlayWhenActive)
				{
					_target.loopClip = EditorGUILayout.Toggle ("Loop animation?", _target.loopClip);
					_target.playOnStart = EditorGUILayout.Toggle ("Play on start?", _target.playOnStart);
				}
				else if (_target.animatedCameraType == AnimatedCameraType.SyncWithTargetMovement)
				{
					_target.pathToFollow = (Paths) EditorGUILayout.ObjectField ("Path to follow:", _target.pathToFollow, typeof (Paths), true);
					_target.targetIsPlayer = EditorGUILayout.Toggle ("Target is Player?", _target.targetIsPlayer);
					
					if (!_target.targetIsPlayer)
					{
						_target.target = (Transform) EditorGUILayout.ObjectField ("Target:", _target.target, typeof(Transform), true);
					}
				}
			EditorGUILayout.EndVertical ();

			if (_target.animatedCameraType == AnimatedCameraType.SyncWithTargetMovement)
			{
				EditorGUILayout.Space ();
				EditorGUILayout.BeginVertical ("Button");
				EditorGUILayout.LabelField ("Cursor influence", EditorStyles.boldLabel);
				_target.followCursor = EditorGUILayout.Toggle ("Follow cursor?", _target.followCursor);
				if (_target.followCursor)
				{
					_target.cursorInfluence = EditorGUILayout.Vector2Field ("Panning factor", _target.cursorInfluence);
					_target.constrainCursorInfluenceX = EditorGUILayout.ToggleLeft ("Constrain panning in X direction?", _target.constrainCursorInfluenceX);
					if (_target.constrainCursorInfluenceX)
					{
						_target.limitCursorInfluenceX[0] = EditorGUILayout.Slider ("Minimum X:", _target.limitCursorInfluenceX[0], -1.4f, 0f);
						_target.limitCursorInfluenceX[1] = EditorGUILayout.Slider ("Maximum X:", _target.limitCursorInfluenceX[1], 0f, 1.4f);
					}
					_target.constrainCursorInfluenceY = EditorGUILayout.ToggleLeft ("Constrain panning in Y direction?", _target.constrainCursorInfluenceY);
					if (_target.constrainCursorInfluenceY)
					{
						_target.limitCursorInfluenceY[0] = EditorGUILayout.Slider ("Minimum Y:", _target.limitCursorInfluenceY[0], -1.4f, 0f);
						_target.limitCursorInfluenceY[1] = EditorGUILayout.Slider ("Maximum Y:", _target.limitCursorInfluenceY[1], 0f, 1.4f);
					}

					if (Application.isPlaying && KickStarter.mainCamera != null && KickStarter.mainCamera.attachedCamera == _target)
					{
						EditorGUILayout.HelpBox ("Changes made to this panel will not be felt until the MainCamera switches to this camera again.", MessageType.Info);
					}
				}
				EditorGUILayout.EndVertical ();
			}

			UnityVersionHandler.CustomSetDirty (_target);
		}
	}

}