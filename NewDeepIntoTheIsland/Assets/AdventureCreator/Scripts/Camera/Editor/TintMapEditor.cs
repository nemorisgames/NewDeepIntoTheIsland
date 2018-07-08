using UnityEngine;
using UnityEditor;

namespace AC
{

	[CustomEditor (typeof (TintMap))]
	public class TintMapEditor : Editor
	{

		public override void OnInspectorGUI ()
		{
			TintMap _target = (TintMap) target;

			_target.tintMapTexture = (Texture2D) EditorGUILayout.ObjectField ("Texture to use (optional):", _target.tintMapTexture, typeof (Texture2D), false);
			if (_target.tintMapTexture && !Application.isPlaying)
			{
				EditorGUILayout.HelpBox ("The supplied texture will be applied to the Mesh Renderer's material when the game begins.", MessageType.Info);
			}
			_target.disableRenderer = EditorGUILayout.Toggle ("Disable mesh renderer?", _target.disableRenderer);

			UnityVersionHandler.CustomSetDirty (_target);
		}

	}

}