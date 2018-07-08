#if UNITY_5_6_OR_NEWER

using UnityEditor;

namespace AC
{

	[CustomEditor (typeof (RememberVideoPlayer), true)]
	public class RememberVideoPlayerEditor : ConstantIDEditor
	{
		
		public override void OnInspectorGUI ()
		{
			RememberVideoPlayer _target = (RememberVideoPlayer) target;

			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Video", EditorStyles.boldLabel);
			_target.saveClipAsset = EditorGUILayout.Toggle ("Save clip asset?", _target.saveClipAsset);
			if (_target.saveClipAsset)
			{
				EditorGUILayout.HelpBox ("Both the original and new 'Video clip' assets will need placing in a Resources folder.", MessageType.Info);
			}
			EditorGUILayout.EndVertical ();

			SharedGUI ();
		}
		
	}

}

#endif