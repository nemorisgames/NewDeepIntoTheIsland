using UnityEngine;
using UnityEditor;

namespace AC
{

	[CustomEditor(typeof(Moveable_PickUp))]
	public class Moveable_PickUpEditor : DragBaseEditor
	{

		public override void OnInspectorGUI ()
		{
			Moveable_PickUp _target = (Moveable_PickUp) target;
			GetReferences ();

			EditorGUILayout.BeginVertical ("Button");
				EditorGUILayout.LabelField ("Movment settings:", EditorStyles.boldLabel);
				_target.maxSpeed = EditorGUILayout.FloatField ("Max speed:", _target.maxSpeed);
				_target.playerMovementReductionFactor = EditorGUILayout.Slider ("Player movement reduction:", _target.playerMovementReductionFactor, 0f, 1f);
				_target.invertInput = EditorGUILayout.Toggle ("Invert input?", _target.invertInput);
				_target.breakForce = EditorGUILayout.FloatField ("Break force:", _target.breakForce);
				_target.initialLift = EditorGUILayout.Slider ("Initial lift:", _target.initialLift, 0f, 1f);

				EditorGUILayout.BeginHorizontal ();
				_target.interactionOnGrab = (Interaction) EditorGUILayout.ObjectField ("Interaction on grab:", _target.interactionOnGrab, typeof (Interaction), true);
				
				if (_target.interactionOnGrab == null)
				{
					if (GUILayout.Button ("Create", GUILayout.MaxWidth (60f)))
					{
						Undo.RecordObject (_target, "Create Interaction");
						Interaction newInteraction = SceneManager.AddPrefab ("Logic", "Interaction", true, false, true).GetComponent <Interaction>();
						newInteraction.gameObject.name = AdvGame.UniqueName ("Move : " + _target.gameObject.name);
						_target.interactionOnGrab = newInteraction;
					}
				}
				EditorGUILayout.EndHorizontal ();
			EditorGUILayout.EndVertical ();

			EditorGUILayout.BeginVertical ("Button");
				EditorGUILayout.LabelField ("Rotation settings:", EditorStyles.boldLabel);
				_target.allowRotation = EditorGUILayout.Toggle ("Allow rotation?", _target.allowRotation);
				if (_target.allowRotation)
				{
					_target.rotationFactor = EditorGUILayout.FloatField ("Rotation factor:", _target.rotationFactor);
				}
			EditorGUILayout.EndVertical ();

			EditorGUILayout.BeginVertical ("Button");
				EditorGUILayout.LabelField ("Zoom settings:", EditorStyles.boldLabel);
				_target.allowZooming = EditorGUILayout.Toggle ("Allow zooming?", _target.allowZooming);
				if (_target.allowZooming)
				{
					_target.zoomSpeed = EditorGUILayout.FloatField ("Zoom speed:", _target.zoomSpeed);
					_target.minZoom = EditorGUILayout.FloatField ("Closest distance:", _target.minZoom);
					_target.maxZoom = EditorGUILayout.FloatField ("Farthest distance:", _target.maxZoom);
				}
			EditorGUILayout.EndVertical ();

			EditorGUILayout.BeginVertical ("Button");
				EditorGUILayout.LabelField ("Throw settings:", EditorStyles.boldLabel);
				_target.allowThrow = EditorGUILayout.Toggle ("Allow throwing?", _target.allowThrow);
				if (_target.allowThrow)
				{
					_target.throwForce = EditorGUILayout.FloatField ("Force scale:", _target.throwForce);
					_target.chargeTime = EditorGUILayout.FloatField ("Charge time:", _target.chargeTime);
					_target.pullbackDistance = EditorGUILayout.FloatField ("Pull-back distance:", _target.pullbackDistance);
				}		
			EditorGUILayout.EndVertical ();

			SharedGUI (_target, false);

			DisplayInputList (_target);
		
			UnityVersionHandler.CustomSetDirty (_target);
		}


		private void DisplayInputList (Moveable_PickUp _target)
		{
			string result = "";

			if (_target.allowRotation)
			{
				result += "\n";
				result += "- RotateMoveable (Button)";
				result += "\n";
				result += "- RotateMoveableToggle (Button";
			}
			if (_target.allowZooming)
			{
				result += "\n";
				result += "- ZoomMoveable (Axis)";
			}
			if (_target.allowThrow)
			{
				result += "\n";
				result += "- ThrowMoveable (Button)";
			}

			if (result != "")
			{
				EditorGUILayout.Space ();
				EditorGUILayout.LabelField ("Required inputs:", EditorStyles.boldLabel);
				EditorGUILayout.HelpBox ("The following input axes are available for the chosen settings:" + result, MessageType.Info);
			}
		}

	}

}