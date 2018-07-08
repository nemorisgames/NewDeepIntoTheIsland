using UnityEngine;
using UnityEditor;

namespace AC
{

	[CustomEditor (typeof (RememberVisibility), true)]
	public class RememberVisibilityEditor : ConstantIDEditor
	{
		
		public override void OnInspectorGUI()
		{
			RememberVisibility _target = (RememberVisibility) target;

			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Visibility", EditorStyles.boldLabel);
			_target.startState = (AC_OnOff) EditorGUILayout.EnumPopup ("Visibility on start:", _target.startState);
			_target.affectChildren = EditorGUILayout.Toggle ("Affect children?", _target.affectChildren);

			if (_target.GetComponent <SpriteFader>() == null && _target.GetComponent <SpriteRenderer>() != null)
			{
				_target.saveColour = EditorGUILayout.Toggle ("Save colour/alpha?", _target.saveColour);
			}

			EditorGUILayout.EndVertical ();

			SharedGUI ();
		}
		
	}

}