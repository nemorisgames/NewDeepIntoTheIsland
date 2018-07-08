using UnityEngine;
using UnityEditor;

namespace AC
{

	[CustomEditor (typeof (GameCamera25D))]
	public class GameCamera25DEditor : Editor
	{

		public override void OnInspectorGUI ()
		{
			GameCamera25D _target = (GameCamera25D) target;
			
			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Background image", EditorStyles.boldLabel);
		
			EditorGUILayout.BeginHorizontal ();
			_target.backgroundImage = (BackgroundImage) EditorGUILayout.ObjectField ("Prefab:", _target.backgroundImage, typeof (BackgroundImage), true);
			
			if (_target.backgroundImage)
			{
				if (GUILayout.Button ("Set as active", GUILayout.MaxWidth (90f)))
				{
					Undo.RecordObject (_target, "Set active background");
					
					_target.SetActiveBackground ();
					SnapCameraInEditor (_target);
				}
			}
			else
			{
				if (GUILayout.Button ("Create", GUILayout.MaxWidth (90f)))
				{
					Undo.RecordObject (_target, "Create Background Image");
					BackgroundImage newBackgroundImage = SceneManager.AddPrefab ("SetGeometry", "BackgroundImage", true, false, true).GetComponent <BackgroundImage>();
					
					string cameraName = _target.gameObject.name;

					newBackgroundImage.gameObject.name = AdvGame.UniqueName (cameraName + ": Background");
					_target.backgroundImage = newBackgroundImage;
				}
			}
			
			EditorGUILayout.EndHorizontal ();

			if (!_target.GetComponent <Camera>().orthographic)
			{
				EditorGUILayout.Space ();
				EditorGUILayout.LabelField ("Perspective offset", EditorStyles.boldLabel);
				_target.perspectiveOffset.x = EditorGUILayout.Slider ("Horizontal:", _target.perspectiveOffset.x, -0.05f, 0.05f);
				_target.perspectiveOffset.y = EditorGUILayout.Slider ("Vertical:", _target.perspectiveOffset.y, -0.05f, 0.05f);
			}

			EditorGUILayout.EndVertical ();

			if (_target.isActiveEditor)
			{
				UpdateCameraSnap (_target);
			}
			
			UnityVersionHandler.CustomSetDirty (_target);
		}


		private void UpdateCameraSnap (GameCamera25D _target)
		{
			if (KickStarter.mainCamera)
			{
				KickStarter.mainCamera.transform.position = _target.transform.position;
				KickStarter.mainCamera.transform.rotation = _target.transform.rotation;
				
				Camera.main.orthographic = _target.GetComponent <Camera>().orthographic;
				Camera.main.fieldOfView = _target.GetComponent <Camera>().fieldOfView;
				Camera.main.farClipPlane = _target.GetComponent <Camera>().farClipPlane;
				Camera.main.nearClipPlane = _target.GetComponent <Camera>().nearClipPlane;
				Camera.main.orthographicSize = _target.GetComponent <Camera>().orthographicSize;

				if (!Camera.main.orthographic)
				{
					Camera.main.projectionMatrix = AdvGame.SetVanishingPoint (Camera.main, _target.perspectiveOffset);
				}
			}
		}
		
		
		private void SnapCameraInEditor (GameCamera25D _target)
		{
			GameCamera25D[] camera25Ds = FindObjectsOfType (typeof (GameCamera25D)) as GameCamera25D[];
			foreach (GameCamera25D camera25D in camera25Ds)
			{
				if (camera25D == _target)
				{
					_target.isActiveEditor = true;
				}
				else
				{
					camera25D.isActiveEditor = false;
				}
			}
			
			UpdateCameraSnap (_target);
		}
		
	}

}