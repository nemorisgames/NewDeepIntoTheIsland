using UnityEditor;

namespace AC
{
	
	[CustomEditor (typeof (LimitVisibility))]
	public class LimitVisibilityEditor : Editor
	{

		public override void OnInspectorGUI ()
		{
			LimitVisibility _target = (LimitVisibility) target;

			_target.Upgrade ();

			int numOptions = _target.limitToCameras.Count;
			numOptions = EditorGUILayout.IntField ("Number of cameras:", _target.limitToCameras.Count);
			if (_target.limitToCameras.Count < 0)
			{
				numOptions = 0;
			}
			if (numOptions < 1)
			{
				numOptions = 1;
			}
			
			if (numOptions < _target.limitToCameras.Count)
			{
				_target.limitToCameras.RemoveRange (numOptions, _target.limitToCameras.Count - numOptions);
			}
			else if (numOptions > _target.limitToCameras.Count)
			{
				if(numOptions > _target.limitToCameras.Capacity)
				{
					_target.limitToCameras.Capacity = numOptions;
				}
				for (int i=_target.limitToCameras.Count; i<numOptions; i++)
				{
					_target.limitToCameras.Add (null);
				}
			}
			
			for (int i=0; i<_target.limitToCameras.Count; i++)
			{
				_target.limitToCameras [i] = (_Camera) EditorGUILayout.ObjectField ("Camera #" + i.ToString () + ":", _target.limitToCameras [i], typeof (_Camera), true);
			}

			_target.affectChildren = EditorGUILayout.Toggle ("Affect children too?", _target.affectChildren);

			UnityVersionHandler.CustomSetDirty (_target);
		}
	
	}
}
