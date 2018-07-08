/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionCameraTP.cs"
 * 
 *	This action can rotate a GameCameraThirdPerson to a set rotation.
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
	public class ActionCameraTP : Action
	{
		
		public int constantID = 0;
		public int parameterID = -1;
		public GameCameraThirdPerson linkedCamera;
		
		public float transitionTime;
		public int transitionTimeParameterID = -1;

		public bool controlPitch = false;
		public bool controlSpin = false;

		public bool isRelativeToTarget = false;
		public float newPitchAngle = 0f;
		public float newSpinAngle = 0f;
		
		public AnimationCurve timeCurve = new AnimationCurve (new Keyframe(0, 0), new Keyframe(1, 1));
		public MoveMethod moveMethod;

		
		
		public ActionCameraTP ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Camera;
			title = "Rotate third-person";
			description = "Rotates a Game Camera Third-person to face a certain direction, either fixed or relative to its target.";
		}
		
		
		override public void AssignValues (List<ActionParameter> parameters)
		{
			linkedCamera = AssignFile <GameCameraThirdPerson> (parameters, parameterID, constantID, linkedCamera);
			transitionTime = AssignFloat (parameters, transitionTimeParameterID, transitionTime);
		}
		
		
		override public float Run ()
		{
			if (!isRunning)
			{
				isRunning = true;

				if (DoRotation (transitionTime) && transitionTime > 0f && willWait)
				{
					return transitionTime;
				}
			}
			else
			{
				isRunning = false;
				return 0f;
			}

			return 0f;
		}
		
		
		override public void Skip ()
		{
			DoRotation (0f);
		}


		private bool DoRotation (float _transitionTime)
		{
			if (linkedCamera != null && (controlPitch || controlSpin))
			{
				float _newPitchAngle = newPitchAngle;
				float _newSpinAngle = newSpinAngle;

				if (controlSpin)
				{
					if (isRelativeToTarget)
					{
						_newSpinAngle += linkedCamera.target.localEulerAngles.y;
					}
					else
					{
						_newSpinAngle += 180f;
					}
				}

				if (_newSpinAngle > 360f)
				{
					_newSpinAngle -= 360f;
				}

				if (_transitionTime > 0f)
				{
					linkedCamera.ForceRotation (controlPitch, _newPitchAngle, controlSpin, _newSpinAngle, _transitionTime, moveMethod, timeCurve);
				}
				else
				{
					linkedCamera.ForceRotation (controlPitch, _newPitchAngle, controlSpin, _newSpinAngle);
				}

				return true;
			}
			return false;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			parameterID = Action.ChooseParameterGUI ("Third-person camera:", parameters, parameterID, ParameterType.GameObject);
			if (parameterID >= 0)
			{
				constantID = 0;
				linkedCamera = null;
			}
			else
			{
				linkedCamera = (GameCameraThirdPerson) EditorGUILayout.ObjectField ("Third-person camera:", linkedCamera, typeof (GameCameraThirdPerson), true);
				
				constantID = FieldToID <GameCameraThirdPerson> (linkedCamera, constantID);
				linkedCamera = IDToField <GameCameraThirdPerson> (linkedCamera, constantID, true);
			}

			controlPitch = EditorGUILayout.Toggle ("Control pitch?", controlPitch);
			if (controlPitch)
			{
				newPitchAngle = EditorGUILayout.FloatField ("New pitch angle:", newPitchAngle);
			}

			controlSpin = EditorGUILayout.Toggle ("Control spin?", controlSpin);
			if (controlSpin)
			{
				newSpinAngle = EditorGUILayout.FloatField ("New spin angle:", newSpinAngle);
				isRelativeToTarget = EditorGUILayout.Toggle ("Spin relative to target?", isRelativeToTarget);
			}

			if (controlPitch || controlSpin)
			{

				transitionTimeParameterID = Action.ChooseParameterGUI ("Transition time (s):", parameters, transitionTimeParameterID, ParameterType.Float);
				if (transitionTimeParameterID < 0)
				{
					transitionTime = EditorGUILayout.FloatField ("Transition time (s):", transitionTime);
				}
				
				if (transitionTime > 0f)
				{
					moveMethod = (MoveMethod) EditorGUILayout.EnumPopup ("Move method:", moveMethod);

					if (moveMethod == MoveMethod.CustomCurve)
					{
						timeCurve = EditorGUILayout.CurveField ("Time curve:", timeCurve);
					}

					willWait = EditorGUILayout.Toggle ("Wait until finish?", willWait);
				}
			}
			
			AfterRunningOption ();
		}


		override public void AssignConstantIDs (bool saveScriptsToo)
		{
			if (saveScriptsToo)
			{
				AddSaveScript <ConstantID> (linkedCamera);
			}
			AssignConstantID <GameCameraThirdPerson> (linkedCamera, constantID, parameterID);
		}
		
		
		override public string SetLabel ()
		{
			string labelAdd = "";
			if (linkedCamera)
			{
				labelAdd = " (" + linkedCamera.name + ")";
			}
			
			return labelAdd;
		}
		
		#endif
		
	}
	
}