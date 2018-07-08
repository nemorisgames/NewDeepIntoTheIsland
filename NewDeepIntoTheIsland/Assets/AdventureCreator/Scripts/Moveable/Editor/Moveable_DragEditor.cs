using UnityEngine;
using UnityEditor;

namespace AC
{

	[CustomEditor(typeof(Moveable_Drag))]
	public class Moveable_DragEditor : DragBaseEditor
	{

		public override void OnInspectorGUI ()
		{
			Moveable_Drag _target = (Moveable_Drag) target;
			GetReferences ();

			EditorGUILayout.BeginVertical ("Button");
			EditorGUILayout.LabelField ("Movment settings:", EditorStyles.boldLabel);
			_target.maxSpeed = EditorGUILayout.FloatField ("Max speed:", _target.maxSpeed);
			_target.playerMovementReductionFactor = EditorGUILayout.Slider ("Player movement reduction:", _target.playerMovementReductionFactor, 0f, 1f);
			_target.playerMovementInfluence = EditorGUILayout.FloatField ("Player movement influence:", _target.playerMovementInfluence);
			_target.invertInput = EditorGUILayout.Toggle ("Invert input?", _target.invertInput);
			EditorGUILayout.EndVertical ();

			EditorGUILayout.BeginVertical ("Button");

			EditorGUILayout.LabelField ("Drag settings:", EditorStyles.boldLabel);
			_target.dragMode = (DragMode) EditorGUILayout.EnumPopup ("Drag mode:", _target.dragMode);
			if (_target.dragMode == DragMode.LockToTrack)
			{
				_target.track = (DragTrack) EditorGUILayout.ObjectField ("Track to stick to:", _target.track, typeof (DragTrack), true);

				if (_target.track != null && _target.track is DragTrack_Straight)
				{
					EditorGUILayout.HelpBox ("For best results, ensure the first collider on this GameObject is a Sphere Collider covering the breath of the mesh.\r\nIt can be disabled if necessary, but will be used to set correct limit boundaries.", MessageType.Info);
				}

				_target.setOnStart = EditorGUILayout.Toggle ("Set starting position?", _target.setOnStart);
				if (_target.setOnStart)
				{
					_target.trackValueOnStart = EditorGUILayout.Slider ("Initial distance along:", _target.trackValueOnStart, 0f, 1f);
				}
				_target.retainOriginalTransform = EditorGUILayout.ToggleLeft ("Maintain original child transforms?", _target.retainOriginalTransform);

				EditorGUILayout.BeginHorizontal ();
				_target.interactionOnMove = (Interaction) EditorGUILayout.ObjectField ("Interaction on move:", _target.interactionOnMove, typeof (Interaction), true);
				
				if (_target.interactionOnMove == null)
				{
					if (GUILayout.Button ("Create", GUILayout.MaxWidth (60f)))
					{
						Undo.RecordObject (_target, "Create Interaction");
						Interaction newInteraction = SceneManager.AddPrefab ("Logic", "Interaction", true, false, true).GetComponent <Interaction>();
						newInteraction.gameObject.name = AdvGame.UniqueName ("Move : " + _target.gameObject.name);
						_target.interactionOnMove = newInteraction;
					}
				}
				EditorGUILayout.EndVertical ();
			}
			else if (_target.dragMode == DragMode.MoveAlongPlane)
			{
				_target.alignMovement = (AlignDragMovement) EditorGUILayout.EnumPopup ("Align movement:", _target.alignMovement);
				if (_target.alignMovement == AlignDragMovement.AlignToPlane)
				{
					_target.plane = (Transform) EditorGUILayout.ObjectField ("Movement plane:", _target.plane, typeof (Transform), true);
				}
			}
			else if (_target.dragMode == DragMode.RotateOnly)
			{
				_target.rotationFactor = EditorGUILayout.FloatField ("Rotation factor:", _target.rotationFactor);
				_target.allowZooming = EditorGUILayout.Toggle ("Allow zooming?", _target.allowZooming);
				if (_target.allowZooming)
				{
					_target.zoomSpeed = EditorGUILayout.FloatField ("Zoom speed:", _target.zoomSpeed);
					_target.minZoom = EditorGUILayout.FloatField ("Closest distance:", _target.minZoom);
					_target.maxZoom = EditorGUILayout.FloatField ("Farthest distance:", _target.maxZoom);
				}
			}

			if (_target.dragMode != DragMode.LockToTrack)
			{
				_target.noGravityWhenHeld = EditorGUILayout.Toggle ("Disable gravity when held?", _target.noGravityWhenHeld);
			}

			if (Application.isPlaying && _target.dragMode == DragMode.LockToTrack && _target.track)
			{
				EditorGUILayout.Space ();
				EditorGUILayout.LabelField ("Distance along: " + _target.GetPositionAlong ().ToString (), EditorStyles.miniLabel);
			}

			EditorGUILayout.EndVertical ();

			if (_target.dragMode == DragMode.LockToTrack && _target.track is DragTrack_Hinge)
			{
				SharedGUI (_target, true);
			}
			else
			{
				SharedGUI (_target, false);
			}

			DisplayInputList (_target);

			UnityVersionHandler.CustomSetDirty (_target);
		}


		private void DisplayInputList (Moveable_Drag _target)
		{
			string result = "";

			if (_target.dragMode == DragMode.RotateOnly)
			{
				if (_target.allowZooming)
				{
					result += "\n";
					result += "- ZoomMoveable";
				}
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