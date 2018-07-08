using UnityEngine;
using UnityEditor;

namespace AC
{

	[CustomEditor(typeof(GameCamera))]
	public class GameCameraEditor : Editor
	{
		
		public override void OnInspectorGUI()
		{
			GameCamera _target = (GameCamera) target;
			
			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Cursor influence", EditorStyles.boldLabel);
			_target.followCursor = EditorGUILayout.Toggle ("Follow cursor?", _target.followCursor);
			if (_target.followCursor)
			{
				_target.cursorInfluence = EditorGUILayout.Vector2Field ("Panning factor:", _target.cursorInfluence);
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
			EditorGUILayout.Space ();
			
			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("X-axis movement", EditorStyles.boldLabel);
			
			_target.lockXLocAxis = EditorGUILayout.Toggle ("Lock?", _target.lockXLocAxis);
			
			if (!_target.lockXLocAxis)
			{
				_target.xLocConstrainType = (CameraLocConstrainType) EditorGUILayout.EnumPopup ("Affected by:", _target.xLocConstrainType);
				
				EditorGUILayout.BeginVertical ("Button");
				if (_target.xLocConstrainType == CameraLocConstrainType.SideScrolling)
				{
					_target.xFreedom = EditorGUILayout.FloatField ("Track freedom:", _target.xFreedom);
				}
				else
				{
					_target.xGradient = EditorGUILayout.FloatField ("Influence:", _target.xGradient);
				}
				_target.xOffset = EditorGUILayout.FloatField ("Offset:", _target.xOffset);
				EditorGUILayout.EndVertical ();
	
				_target.limitX = EditorGUILayout.BeginToggleGroup ("Constrain?", _target.limitX);
				
				EditorGUILayout.BeginVertical ("Button");
				_target.constrainX[0] = EditorGUILayout.FloatField ("Minimum:", _target.constrainX[0]);
				_target.constrainX[1] = EditorGUILayout.FloatField ("Maximum:", _target.constrainX[1]);
				EditorGUILayout.EndVertical ();
	
				EditorGUILayout.EndToggleGroup ();
			}
				
			EditorGUILayout.EndVertical ();
			EditorGUILayout.Space ();

			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Y-axis movement", EditorStyles.boldLabel);
			
			_target.lockYLocAxis = EditorGUILayout.Toggle ("Lock?", _target.lockYLocAxis);
			
			if (!_target.lockYLocAxis)
			{
				_target.yLocConstrainType = (CameraLocConstrainType) EditorGUILayout.EnumPopup ("Affected by:", _target.yLocConstrainType);

				EditorGUILayout.BeginVertical ("Button");
				if (_target.yLocConstrainType == CameraLocConstrainType.SideScrolling)
				{
					_target.yFreedom = EditorGUILayout.FloatField ("Track freedom:", _target.yFreedom);
				}
				else
				{
					_target.yGradientLoc = EditorGUILayout.FloatField ("Influence:", _target.yGradientLoc);
				}
				_target.yOffsetLoc = EditorGUILayout.FloatField ("Offset:", _target.yOffsetLoc);
				EditorGUILayout.EndVertical ();

				_target.limitYLoc = EditorGUILayout.BeginToggleGroup ("Constrain?", _target.limitYLoc);
				
				EditorGUILayout.BeginVertical ("Button");
				_target.constrainYLoc[0] = EditorGUILayout.FloatField ("Minimum:", _target.constrainYLoc[0]);
				_target.constrainYLoc[1] = EditorGUILayout.FloatField ("Maximum:", _target.constrainYLoc[1]);
				EditorGUILayout.EndVertical ();
				
				EditorGUILayout.EndToggleGroup ();
			}
			
			EditorGUILayout.EndVertical ();
			EditorGUILayout.Space ();

			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Z-axis movement", EditorStyles.boldLabel);
	
			_target.lockZLocAxis = EditorGUILayout.Toggle ("Lock?", _target.lockZLocAxis);
			
			if (!_target.lockZLocAxis)
			{
				_target.zLocConstrainType = (CameraLocConstrainType) EditorGUILayout.EnumPopup ("Affected by:", _target.zLocConstrainType);
				
				EditorGUILayout.BeginVertical ("Button");
				if (_target.zLocConstrainType == CameraLocConstrainType.SideScrolling)
				{
					_target.zFreedom = EditorGUILayout.FloatField ("Track freedom:", _target.zFreedom);
				}
				else
				{
					_target.zGradient = EditorGUILayout.FloatField ("Influence:", _target.zGradient);
				}
				_target.zOffset = EditorGUILayout.FloatField ("Offset:", _target.zOffset);
				EditorGUILayout.EndVertical ();
	
				_target.limitZ = EditorGUILayout.BeginToggleGroup ("Constrain?", _target.limitZ);
				
				EditorGUILayout.BeginVertical ("Button");
				_target.constrainZ[0] = EditorGUILayout.FloatField ("Minimum:", _target.constrainZ[0]);
				_target.constrainZ[1] = EditorGUILayout.FloatField ("Maximum:", _target.constrainZ[1]);
				EditorGUILayout.EndVertical ();
	
				EditorGUILayout.EndToggleGroup ();
			}
			
			EditorGUILayout.EndVertical ();
			EditorGUILayout.Space ();
			
			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Pitch rotation", EditorStyles.boldLabel);
			
			_target.lockXRotAxis = EditorGUILayout.Toggle ("Lock?", _target.lockXRotAxis);
			
			if (!_target.lockXRotAxis)
			{
				_target.xRotConstrainType = (CameraLocConstrainType) EditorGUILayout.EnumPopup ("Affected by:", _target.xRotConstrainType);
				
				if(_target.xRotConstrainType == CameraLocConstrainType.SideScrolling)
				{
					EditorGUILayout.HelpBox ("This option is not available for Pitch rotation", MessageType.Warning);
				}
				else
				{
					EditorGUILayout.BeginVertical ("Button");
					_target.xGradientRot = EditorGUILayout.FloatField ("Influence:", _target.xGradientRot);
					_target.xOffsetRot = EditorGUILayout.FloatField ("Offset:", _target.xOffsetRot);
					EditorGUILayout.EndVertical ();
				}
				
				_target.limitXRot = EditorGUILayout.BeginToggleGroup ("Constrain?", _target.limitXRot);
				
				EditorGUILayout.BeginVertical ("Button");
				_target.constrainXRot[0] = EditorGUILayout.FloatField ("Minimum:", _target.constrainXRot[0]);
				_target.constrainXRot[1] = EditorGUILayout.FloatField ("Maximum:", _target.constrainXRot[1]);
				EditorGUILayout.EndVertical ();
				
				EditorGUILayout.EndToggleGroup ();
			}
			
			EditorGUILayout.EndVertical ();
			EditorGUILayout.Space ();
			
			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Spin rotation", EditorStyles.boldLabel);
			
			_target.lockYRotAxis = EditorGUILayout.Toggle ("Lock?", _target.lockYRotAxis);
			
			if (!_target.lockYRotAxis)
			{
				_target.yRotConstrainType = (CameraRotConstrainType) EditorGUILayout.EnumPopup ("Affected by:", _target.yRotConstrainType);
				
				if (_target.yRotConstrainType != CameraRotConstrainType.LookAtTarget)
				{
					EditorGUILayout.BeginVertical ("Button");
					_target.directionInfluence = EditorGUILayout.FloatField ("Target direction fac.:", _target.directionInfluence);
					_target.yGradient = EditorGUILayout.FloatField ("Influence:", _target.yGradient);
					_target.yOffset = EditorGUILayout.FloatField ("Offset:", _target.yOffset);
					EditorGUILayout.EndVertical ();
					
					_target.limitY = EditorGUILayout.BeginToggleGroup ("Constrain?", _target.limitY);
					
					EditorGUILayout.BeginVertical ("Button");
					_target.constrainY[0] = EditorGUILayout.FloatField ("Minimum:", _target.constrainY[0]);
					_target.constrainY[1] = EditorGUILayout.FloatField ("Maximum:", _target.constrainY[1]);
					EditorGUILayout.EndVertical ();
					
					EditorGUILayout.EndToggleGroup ();
				}
				else
				{
					EditorGUILayout.BeginVertical ("Button");
					_target.directionInfluence = EditorGUILayout.FloatField ("Target direction fac.:", _target.directionInfluence);
					_target.targetHeight = EditorGUILayout.FloatField ("Target height offset:", _target.targetHeight);
					_target.targetXOffset = EditorGUILayout.FloatField ("Target X offset:", _target.targetXOffset);
					_target.targetZOffset = EditorGUILayout.FloatField ("Target Z offset:", _target.targetZOffset);
					EditorGUILayout.EndVertical ();
				}
			}
			
			EditorGUILayout.EndVertical ();
			EditorGUILayout.Space ();

			EditorGUILayout.BeginVertical ("Button");
			if (_target.GetComponent <Camera>() && _target.GetComponent <Camera>().orthographic)
			{
				EditorGUILayout.LabelField ("Orthographic size", EditorStyles.boldLabel);
			}
			else if (_target.GetComponentInChildren <Camera>() && _target.GetComponentInChildren <Camera>().orthographic)
			{
				EditorGUILayout.LabelField ("Orthographic size", EditorStyles.boldLabel);
			}
			else
			{
				EditorGUILayout.LabelField ("Field of view", EditorStyles.boldLabel);
			}
			
			_target.lockFOV = EditorGUILayout.Toggle ("Lock?", _target.lockFOV);
			
			if (!_target.lockFOV)
			{
				EditorGUILayout.HelpBox ("This value will vary with the target's distance from the camera.", MessageType.Info);
				
				EditorGUILayout.BeginVertical ("Button");
					_target.FOVGradient = EditorGUILayout.FloatField ("Influence:", _target.FOVGradient);
					_target.FOVOffset = EditorGUILayout.FloatField ("Offset:", _target.FOVOffset);
				EditorGUILayout.EndVertical ();
				
				_target.limitFOV = EditorGUILayout.BeginToggleGroup ("Constrain?", _target.limitFOV);
				
				EditorGUILayout.BeginVertical ("Button");
				_target.constrainFOV[0] = EditorGUILayout.FloatField ("Minimum:", _target.constrainFOV[0]);
				_target.constrainFOV[1] = EditorGUILayout.FloatField ("Maximum:", _target.constrainFOV[1]);
				EditorGUILayout.EndVertical ();
				
				EditorGUILayout.EndToggleGroup ();
			}
			
			EditorGUILayout.EndVertical ();
			EditorGUILayout.Space ();

			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Depth of field", EditorStyles.boldLabel);
			_target.focalPointIsTarget = EditorGUILayout.Toggle ("Focal point is target object?", _target.focalPointIsTarget);
			if (!_target.focalPointIsTarget)
			{
				_target.focalDistance = EditorGUILayout.FloatField ("Focal distance:", _target.focalDistance);
			}
			else if (Application.isPlaying)
			{
				EditorGUILayout.LabelField ("Focal distance: " +  _target.focalDistance.ToString (), EditorStyles.miniLabel);
			}
			EditorGUILayout.EndVertical ();
			EditorGUILayout.Space ();

			if (!_target.lockXLocAxis || !_target.lockYRotAxis || !_target.lockFOV || !_target.lockYLocAxis || !_target.lockZLocAxis || _target.focalPointIsTarget)
			{
				EditorGUILayout.BeginVertical ("Button");
					EditorGUILayout.LabelField ("Target object to control camera movement", EditorStyles.boldLabel);
					
					_target.targetIsPlayer = EditorGUILayout.Toggle ("Target is Player?", _target.targetIsPlayer);
					
					if (!_target.targetIsPlayer)
					{
						_target.target = (Transform) EditorGUILayout.ObjectField ("Target:", _target.target, typeof(Transform), true);
					}
					
					_target.dampSpeed = EditorGUILayout.FloatField ("Follow speed:", _target.dampSpeed);
					_target.actFromDefaultPlayerStart = EditorGUILayout.Toggle ("Use default PlayerStart?", _target.actFromDefaultPlayerStart);
				EditorGUILayout.EndVertical ();
			}
			
			UnityVersionHandler.CustomSetDirty (_target);
		}
	}

}