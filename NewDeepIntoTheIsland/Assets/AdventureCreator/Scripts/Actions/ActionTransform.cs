/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionTransform.cs"
 * 
 *	This action modifies a GameObject position, rotation or scale over a set time.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	[System.Serializable]
	public class ActionTransform : Action
	{

		public bool isPlayer;

		public Marker marker;
		public int markerParameterID = -1;
		public int markerID = 0;
		public bool doEulerRotation = false;
		public bool clearExisting = true;
		public bool inWorldSpace = false;
		
		public AnimationCurve timeCurve = new AnimationCurve (new Keyframe(0, 0), new Keyframe(1, 1));
		
		public int parameterID = -1;
		public int constantID = 0;
		public Moveable linkedProp;

		public enum SetVectorMethod { EnteredHere, FromVector3Variable };
		public SetVectorMethod setVectorMethod = SetVectorMethod.EnteredHere;

		public int newVectorParameterID = -1;
		public Vector3 newVector;

		public int vectorVarParameterID = -1;
		public int vectorVarID;
		public VariableLocation variableLocation = VariableLocation.Global;

		public float transitionTime;
		public int transitionTimeParameterID = -1;
		
		public TransformType transformType;
		public MoveMethod moveMethod;
		
		public enum ToBy { To, By };
		public ToBy toBy;

		private Vector3 nonSkipTargetVector = Vector3.zero;

		
		public ActionTransform ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Object;
			title = "Transform";
			description = "Transforms a GameObject over time, by or to a given amount, or towards a Marker in the scene. The GameObject must have a Moveable script attached.";
		}
		
		
		override public void AssignValues (List<ActionParameter> parameters)
		{
			if (isPlayer)
			{
				if (KickStarter.player != null)
				{
					linkedProp = KickStarter.player.GetComponent <Moveable>();

					if (linkedProp == null)
					{
						ACDebug.LogWarning ("The player " + KickStarter.player + " requires a Moveable component to be moved with the 'Object: Transform' Action.", KickStarter.player);
					}
				}
				else
				{
					linkedProp = null;
				}
			}
			else
			{
				linkedProp = AssignFile <Moveable> (parameters, parameterID, constantID, linkedProp);
			}

			marker = AssignFile <Marker> (parameters, markerParameterID, markerID, marker);
			transitionTime = AssignFloat (parameters, transitionTimeParameterID, transitionTime);
			newVector = AssignVector3 (parameters, newVectorParameterID, newVector);
			vectorVarID = AssignVariableID (parameters, vectorVarParameterID, vectorVarID);

			if (!(transformType == TransformType.CopyMarker ||
				(transformType == TransformType.Translate && toBy == ToBy.To) ||
				(transformType == TransformType.Rotate && toBy == ToBy.To)))
			{
				inWorldSpace = false;
			}
		}
		
		
		override public float Run ()	
		{
			if (!isRunning)
			{
				isRunning = true;
				
				if (linkedProp)
				{
					RunToTime (transitionTime, false);
					
					if (willWait && transitionTime > 0f)
					{
						return (defaultPauseTime);
					}
				}
			}
			else
			{
				if (linkedProp)
				{
					if (!linkedProp.IsMoving (transformType))
					{
						isRunning = false;
					}
					else
					{
						return defaultPauseTime;
					}
				}
			}
			
			return 0f;
		}
		
		
		override public void Skip ()	
		{
			if (linkedProp)
			{
				RunToTime (0f, true);
			}
		}
		

		private void RunToTime (float _time, bool isSkipping)
		{
			if (transformType == TransformType.CopyMarker)
			{
				if (marker)
				{
					linkedProp.Move (marker, moveMethod, inWorldSpace, _time, timeCurve);
				}
			}
			else
			{
				Vector3 targetVector = Vector3.zero;

				if (setVectorMethod == SetVectorMethod.FromVector3Variable)
				{
					if (variableLocation == VariableLocation.Global)
					{
						targetVector = GlobalVariables.GetVector3Value (vectorVarID);
					}
					else if (variableLocation == VariableLocation.Local && !isAssetFile)
					{
						targetVector = LocalVariables.GetVector3Value (vectorVarID);
					}
				}
				else if (setVectorMethod == SetVectorMethod.EnteredHere)
				{
					targetVector = newVector;
				}

				if (transformType == TransformType.Translate)
				{
					if (toBy == ToBy.By)
					{
						targetVector = SetRelativeTarget (targetVector, isSkipping, linkedProp.transform.localPosition);
					}
				}
				else if (transformType == TransformType.Rotate)
				{
					if (toBy == ToBy.By)
					{
						int numZeros = 0;
						if (Mathf.Approximately (targetVector.x, 0f)) numZeros ++;
						if (Mathf.Approximately (targetVector.y, 0f)) numZeros ++;
						if (Mathf.Approximately (targetVector.z, 0f)) numZeros ++;

						if (numZeros == 2)
						{
							targetVector = SetRelativeTarget (targetVector, isSkipping, linkedProp.transform.eulerAngles);
						}
						else
						{
							Quaternion currentRotation = linkedProp.transform.localRotation;
							linkedProp.transform.Rotate (targetVector, Space.World);
							targetVector = linkedProp.transform.localEulerAngles;
							linkedProp.transform.localRotation = currentRotation;
						}
					}
				}
				else if (transformType == TransformType.Scale)
				{
					if (toBy == ToBy.By)
					{
						targetVector = SetRelativeTarget (targetVector, isSkipping, linkedProp.transform.localScale);
					}
				}
				
				if (transformType == TransformType.Rotate)
				{
					linkedProp.Move (targetVector, moveMethod, inWorldSpace, _time, transformType, doEulerRotation, timeCurve, clearExisting);
				}
				else
				{
					linkedProp.Move (targetVector, moveMethod, inWorldSpace, _time, transformType, false, timeCurve, clearExisting);
				}
			}
		}


		private Vector3 SetRelativeTarget (Vector3 _targetVector, bool isSkipping, Vector3 normalAddition)
		{
			if (isSkipping && nonSkipTargetVector != Vector3.zero)
			{
				_targetVector = nonSkipTargetVector;
			}
			else
			{
				_targetVector += normalAddition;
				nonSkipTargetVector = _targetVector;
			}
			return _targetVector;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			isPlayer = EditorGUILayout.Toggle ("Move Player?", isPlayer);
			if (!isPlayer)
			{
				parameterID = Action.ChooseParameterGUI ("Moveable object:", parameters, parameterID, ParameterType.GameObject);
				if (parameterID >= 0)
				{
					constantID = 0;
					linkedProp = null;
				}
				else
				{
					linkedProp = (Moveable) EditorGUILayout.ObjectField ("Moveable object:", linkedProp, typeof (Moveable), true);

					constantID = FieldToID <Moveable> (linkedProp, constantID);
					linkedProp = IDToField <Moveable> (linkedProp, constantID, false);
				}
			}

			EditorGUILayout.BeginHorizontal ();
			transformType = (TransformType) EditorGUILayout.EnumPopup (transformType);
			if (transformType != TransformType.CopyMarker)
			{
				toBy = (ToBy) EditorGUILayout.EnumPopup (toBy);
			}
			EditorGUILayout.EndHorizontal ();
			
			if (transformType == TransformType.CopyMarker)
			{
				markerParameterID = Action.ChooseParameterGUI ("Marker:", parameters, markerParameterID, ParameterType.GameObject);
				if (markerParameterID >= 0)
				{
					markerID = 0;
					marker = null;
				}
				else
				{
					marker = (Marker) EditorGUILayout.ObjectField ("Marker:", marker, typeof (Marker), true);

					markerID = FieldToID <Marker> (marker, markerID);
					marker = IDToField <Marker> (marker, markerID, false);
				}
			}
			else
			{
				setVectorMethod = (SetVectorMethod) EditorGUILayout.EnumPopup ("Vector is: ", setVectorMethod);
				if (setVectorMethod == SetVectorMethod.EnteredHere)
				{
					newVectorParameterID = Action.ChooseParameterGUI ("Value:", parameters, newVectorParameterID, ParameterType.Vector3);
					if (newVectorParameterID < 0)
					{
						newVector = EditorGUILayout.Vector3Field ("Value:", newVector);
					}
				}
				else if (setVectorMethod == SetVectorMethod.FromVector3Variable)
				{
					if (isAssetFile)
					{
						variableLocation = VariableLocation.Global;
					}
					else
					{
						variableLocation = (VariableLocation) EditorGUILayout.EnumPopup ("Source:", variableLocation);
					}

					if (variableLocation == VariableLocation.Global)
					{
						vectorVarParameterID = Action.ChooseParameterGUI ("Vector3 variable:", parameters, vectorVarParameterID, ParameterType.GlobalVariable);
						if (vectorVarParameterID < 0)
						{
							vectorVarID = AdvGame.GlobalVariableGUI ("Vector3 variable:", vectorVarID, VariableType.Vector3);
						}
					}
					else if (variableLocation == VariableLocation.Local)
					{
						vectorVarParameterID = Action.ChooseParameterGUI ("Vector3 variable:", parameters, vectorVarParameterID, ParameterType.LocalVariable);
						if (vectorVarParameterID < 0)
						{
							vectorVarID = AdvGame.LocalVariableGUI ("Vector3 variable:", vectorVarID, VariableType.Vector3);
						}
					}
				}

				clearExisting = EditorGUILayout.Toggle ("Stop existing transforms?", clearExisting);
			}

			if (transformType == TransformType.CopyMarker ||
				(transformType == TransformType.Translate && toBy == ToBy.To) ||
				(transformType == TransformType.Rotate && toBy == ToBy.To))
			{
				inWorldSpace = EditorGUILayout.Toggle ("Act in world-space?", inWorldSpace);

				if (inWorldSpace && transformType == TransformType.CopyMarker)
				{
					EditorGUILayout.HelpBox ("The moveable object's scale will be changed in local space.", MessageType.Info);
				}
			}

			transitionTimeParameterID = Action.ChooseParameterGUI ("Transition time (s):", parameters, transitionTimeParameterID, ParameterType.Float);
			if (transitionTimeParameterID < 0)
			{
				transitionTime = EditorGUILayout.Slider ("Transition time (s):", transitionTime, 0, 10f);
			}
			
			if (transitionTime > 0f)
			{
				if (transformType == TransformType.Rotate)
				{
					doEulerRotation = EditorGUILayout.Toggle ("Euler rotation?", doEulerRotation);
				}
				moveMethod = (MoveMethod) EditorGUILayout.EnumPopup ("Move method:", moveMethod);
				if (moveMethod == MoveMethod.CustomCurve)
				{
					timeCurve = EditorGUILayout.CurveField ("Time curve:", timeCurve);
				}
				willWait = EditorGUILayout.Toggle ("Wait until finish?", willWait);
			}
			
			AfterRunningOption ();
		}


		override public void AssignConstantIDs (bool saveScriptsToo = false)
		{
			if (saveScriptsToo)
			{
				AddSaveScript <RememberMoveable> (linkedProp);
			}
			AssignConstantID <Moveable> (linkedProp, constantID, parameterID);
			AssignConstantID <Marker> (marker, markerID, markerParameterID);
		}


		override public string SetLabel ()
		{
			string labelAdd = "";
			if (linkedProp)
			{
				labelAdd = " (" + linkedProp.name + ")";
			}
			
			return labelAdd;
		}
		
		#endif
		
	}
	
}