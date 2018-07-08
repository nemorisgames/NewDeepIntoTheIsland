#if !UNITY_5_0 && (UNITY_5 || UNITY_2017) && (UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS)
#define ALLOW_VR
#endif

using UnityEngine;
using UnityEditor;

#if ALLOW_VR
using UnityEngine.VR;
#endif

namespace AC
{

	[CustomEditor(typeof(MainCamera))]
	public class MainCameraEditor : Editor
	{
		
		public override void OnInspectorGUI()
		{
			MainCamera _target = (MainCamera) target;

			EditorGUILayout.BeginVertical ("Button");
			_target.fadeTexture = (Texture2D) EditorGUILayout.ObjectField ("Fade texture:", _target.fadeTexture, typeof (Texture2D), false);
			_target.lookAtTransform = (Transform) EditorGUILayout.ObjectField ("LookAt child:", _target.lookAtTransform, typeof (Transform), true);

			#if ALLOW_VR
			if (PlayerSettings.virtualRealitySupported)
			{
				_target.restoreTransformOnLoadVR = EditorGUILayout.ToggleLeft ("Restore transform when loading?", _target.restoreTransformOnLoadVR);
			}
			#endif

			EditorGUILayout.EndVertical ();

			if (Application.isPlaying)
			{
				EditorGUILayout.BeginVertical ("Button");
				if (_target.attachedCamera)
				{
					_target.attachedCamera = (_Camera) EditorGUILayout.ObjectField ("Attached camera:", _target.attachedCamera, typeof (_Camera), true);
				}
				else
				{
					EditorGUILayout.LabelField ("Attached camera: None");
				}
				EditorGUILayout.EndVertical ();
			}

			UnityVersionHandler.CustomSetDirty (_target);
		}

	}

}