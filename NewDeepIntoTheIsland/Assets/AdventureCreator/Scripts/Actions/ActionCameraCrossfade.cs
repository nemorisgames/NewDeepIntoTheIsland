/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionCameraCrossfade.cs"
 * 
 *	This action crossfades the MainCamera from one
 *	GameCamera to another.
 * 
 */

using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class ActionCameraCrossfade : Action
	{

		public int parameterID = -1;
		public int constantID = 0;
		public _Camera linkedCamera;

		public float transitionTime;
		public int transitionTimeParameterID = -1;

		public bool returnToLast;

		
		public ActionCameraCrossfade ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Camera;
			title = "Crossfade";
			description = "Crossfades the camera from its current GameCamera to a new one, over a specified time.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			linkedCamera = AssignFile <_Camera> (parameters, parameterID, constantID, linkedCamera);
			transitionTime = AssignFloat (parameters, transitionTimeParameterID, transitionTime);

			if (returnToLast)
			{
				linkedCamera = KickStarter.mainCamera.GetLastGameplayCamera ();
			}
		}

		
		override public float Run ()
		{
			if (!isRunning)
			{
				isRunning = true;
				MainCamera mainCam = KickStarter.mainCamera;

				if (linkedCamera != null && mainCam.attachedCamera != linkedCamera)
				{
					if (linkedCamera is GameCameraThirdPerson)
					{
						GameCameraThirdPerson tpCam = (GameCameraThirdPerson) linkedCamera;
						tpCam.ResetRotation ();
					}
					else if (linkedCamera is GameCameraAnimated)
					{
						GameCameraAnimated animCam = (GameCameraAnimated) linkedCamera;
						animCam.PlayClip ();
					}
					
					linkedCamera.MoveCameraInstant ();
					mainCam.Crossfade (transitionTime, linkedCamera);
						
					if (transitionTime > 0f && willWait)
					{
						return (transitionTime);
					}
				}
			}
			else
			{
				isRunning = false;
			}
			
			return 0f;
		}
		
		
		override public void Skip ()
		{
			MainCamera mainCam = KickStarter.mainCamera;

			if (linkedCamera != null && mainCam.attachedCamera != linkedCamera)
			{
				if (linkedCamera is GameCameraThirdPerson)
				{
					GameCameraThirdPerson tpCam = (GameCameraThirdPerson) linkedCamera;
					tpCam.ResetRotation ();
				}
				else if (linkedCamera is GameCameraAnimated)
				{
					GameCameraAnimated animCam = (GameCameraAnimated) linkedCamera;
					animCam.PlayClip ();
				}
				
				linkedCamera.MoveCameraInstant ();
				mainCam.SetGameCamera (linkedCamera);
			}
		}


		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			returnToLast = EditorGUILayout.Toggle ("Return to last gameplay?", returnToLast);
			if (!returnToLast)
			{
				parameterID = Action.ChooseParameterGUI ("New camera:", parameters, parameterID, ParameterType.GameObject);
				if (parameterID >= 0)
				{
					constantID = 0;
					linkedCamera = null;
				}
				else
				{
					linkedCamera = (_Camera) EditorGUILayout.ObjectField ("New camera:", linkedCamera, typeof(_Camera), true);
					
					constantID = FieldToID <_Camera> (linkedCamera, constantID);
					linkedCamera = IDToField <_Camera> (linkedCamera, constantID, true);
				}
			}

			transitionTimeParameterID = Action.ChooseParameterGUI ("Transition time (s):", parameters, transitionTimeParameterID, ParameterType.Float);
			if (transitionTimeParameterID < 0)
			{
				transitionTime = EditorGUILayout.FloatField ("Transition time (s):", transitionTime);
			}
			willWait = EditorGUILayout.Toggle ("Wait until finish?", willWait);

			AfterRunningOption ();
		}


		override public void AssignConstantIDs (bool saveScriptsToo)
		{
			if (saveScriptsToo)
			{
				AddSaveScript <ConstantID> (linkedCamera);
			}
			AssignConstantID <_Camera> (linkedCamera, constantID, parameterID);
		}

		
		override public string SetLabel ()
		{
			if (linkedCamera != null)
			{
				return (" (" + linkedCamera.name + ")");
			}
			return "";
		}
		
		#endif
		
	}

}