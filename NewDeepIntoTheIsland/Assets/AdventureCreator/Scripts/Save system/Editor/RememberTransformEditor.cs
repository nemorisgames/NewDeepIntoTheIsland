using UnityEditor;

namespace AC
{

	[CustomEditor (typeof (RememberTransform), true)]
	public class RememberTransformEditor : ConstantIDEditor
	{
		
		public override void OnInspectorGUI ()
		{
			RememberTransform _target = (RememberTransform) target;

			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Transform", EditorStyles.boldLabel);
			_target.saveParent = EditorGUILayout.Toggle ("Save change in Parent?", _target.saveParent);
			_target.saveScenePresence = EditorGUILayout.Toggle ("Save scene presence?", _target.saveScenePresence);

			if (_target.saveScenePresence)
			{
				_target.linkedPrefabID = EditorGUILayout.IntField ("Linked prefab ConstantID:", _target.linkedPrefabID);
				EditorGUILayout.HelpBox ("If the above is non-zero, the Resources prefab with that ID number will be spawned if this is not present in the scene.  This allows multiple instances of the object can be spawned.", MessageType.Info);

				_target.retainInPrefab = true;
				EditorGUILayout.HelpBox ("This prefab must be placed in a 'Resources' asset folder", MessageType.Info);
			}
			EditorGUILayout.EndVertical ();

			SharedGUI ();
		}
		
	}

}