using UnityEngine;
using UnityEditor;

namespace AC
{

	[CustomEditor (typeof (Parallax2D))]
	public class Parallax2DEditor : Editor
	{

		private Parallax2D _target;


		private void OnEnable ()
		{
			_target = (Parallax2D) target;
		}


		public override void OnInspectorGUI ()
		{
			EditorGUILayout.BeginVertical ("Button");
			_target.reactsTo = (ParallaxReactsTo) EditorGUILayout.EnumPopup ("Reacts to:", _target.reactsTo);
			_target.depth = EditorGUILayout.FloatField ("Depth:", _target.depth);
			EditorGUILayout.EndVertical ();

			EditorGUILayout.BeginVertical ("Button");
			_target.xScroll = EditorGUILayout.BeginToggleGroup ("Scroll in X direction?", _target.xScroll);
			_target.xOffset = EditorGUILayout.FloatField ("Offset:", _target.xOffset);
			_target.limitX = EditorGUILayout.Toggle ("Constrain?", _target.limitX);
			if (_target.limitX)
			{
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Minimum:", GUILayout.Width (70f));
				_target.minX = EditorGUILayout.FloatField (_target.minX);
				EditorGUILayout.LabelField ("Maximum:", GUILayout.Width (70f));
				_target.maxX = EditorGUILayout.FloatField (_target.maxX);
				EditorGUILayout.EndHorizontal ();
			}
			EditorGUILayout.EndToggleGroup ();
			EditorGUILayout.EndVertical ();

			EditorGUILayout.BeginVertical ("Button");
			_target.yScroll = EditorGUILayout.BeginToggleGroup ("Scroll in Y direction?", _target.yScroll);
			_target.yOffset = EditorGUILayout.FloatField ("Offset:", _target.yOffset);
			_target.limitY = EditorGUILayout.Toggle ("Constrain?", _target.limitY);
			if (_target.limitY)
			{
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Minimum:", GUILayout.Width (70f));
				_target.minY = EditorGUILayout.FloatField (_target.minY);
				EditorGUILayout.LabelField ("Maximum:", GUILayout.Width (70f));
				_target.maxY = EditorGUILayout.FloatField (_target.maxY);
				EditorGUILayout.EndHorizontal ();
			}
			EditorGUILayout.EndToggleGroup ();
			EditorGUILayout.EndVertical ();

			UnityVersionHandler.CustomSetDirty (_target);
		}
	}

}