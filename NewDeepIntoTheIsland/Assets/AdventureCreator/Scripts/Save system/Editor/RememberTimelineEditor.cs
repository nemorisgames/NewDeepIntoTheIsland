using UnityEditor;

namespace AC
{

	[CustomEditor (typeof (RememberTimeline), true)]
	public class RememberTimelineEditor : ConstantIDEditor
	{
		
		public override void OnInspectorGUI ()
		{
			RememberTimeline _target = (RememberTimeline) target;

			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Timeline", EditorStyles.boldLabel);
			_target.saveBindings = EditorGUILayout.Toggle ("Save bindings?", _target.saveBindings);
			_target.saveTimelineAsset = EditorGUILayout.Toggle ("Save Timeline asset?", _target.saveTimelineAsset);
			if (_target.saveTimelineAsset)
			{
				EditorGUILayout.HelpBox ("Both the original and new 'Timeline' assets will need placing in a Resources folder.", MessageType.Info);
			}
			EditorGUILayout.EndVertical ();

			SharedGUI ();
		}
		
	}

}