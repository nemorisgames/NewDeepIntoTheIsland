/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"OuyaIntegration.cs"
 * 
 *	This script serves as a bridge between Adventure Creator and the OUYA platform.
 *	To use it, add it to a GameObject in your scene, and make sure that 'Assume inputs are defined?' is UNCHECKED in the Settings Manager.
 *
 *	You must then add the 'OUYAISPresent' preprocessor to your game.  This can be done from 'Edit -> Project Settings -> Player', and entering 'OUYAIsPresent' into the Scripting Define Symbols text box for your game's build platform.
 *
 *	This bridge script provides a robust integration for controlling AC using an OUYA controller.
 *	If you wish to build upon it for more custom gameplay, duplicate the script and make such changes to the copy.
 *	You can then add your new script to the scene instead.
 * 
 */

using UnityEngine;

#if OUYAIsPresent
using tv.ouya.console.api;
#endif

namespace AC
{

	/**
	 * This script serves as a bridge between Adventure Creator and the OUYA platform.
	 * To use it, add it to a GameObject in your scene, and make sure that 'Assume inputs are defined?' is UNCHECKED in the Settings Manager.
	 *
	 * You must then add the 'OUYAISPresent' preprocessor to your game. This can be done from 'Edit -> Project Settings -> Player', and entering 'OUYAIsPresent' into the Scripting Define Symbols text box for your game's build platform.
	 *
	 * This bridge script provides a robust integration for controlling AC using an OUYA controller.
	 * If you wish to build upon it for more custom gameplay, duplicate the script and make such changes to the copy.
	 * You can then add your new script to the scene instead.
	 */
	[AddComponentMenu("Adventure Creator/3rd-party/OUYA integration")]
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_ouya_integration.html")]
	#endif
	public class OuyaIntegration : MonoBehaviour
	{

		#if OUYAIsPresent
		private Vector2 currentMousePosition;

		public Vector2 screenScale = new Vector2 (1f, 1f);
		public float mouseMoveSpeed = 0.4f;
		public Vector2 mouseDeadZone = new Vector2 (0.26f, 0.2f);
		public bool invertMouseY = true;
		#endif


		private void Start ()
		{
			#if OUYAIsPresent && UNITY_ANDROID && !UNITY_EDITOR

			AssignOverrides ();

			#elif OUYAIsPresent

			ACDebug.Log ("OUYA integration is ready to go, but will only take effect in Android builds of your game - not in the Editor.");

			#else

			ACDebug.LogWarning ("'OUYAIsPresent' must be listed in your Unity Player Setting's 'Scripting define symbols' for AC's OUYA integration to work.");

			#endif
		}


		#if OUYAIsPresent && UNITY_ANDROID && !UNITY_EDITOR

		private void AssignOverrides ()
		{
			if (KickStarter.playerInput)
			{
				KickStarter.playerInput.InputMousePositionDelegate = MousePosition;
				KickStarter.playerInput.InputGetAxisDelegate = GetAxis;
				KickStarter.playerInput.InputGetButtonDelegate = GetButton;
				KickStarter.playerInput.InputGetButtonDownDelegate = GetButtonDown;
				KickStarter.playerInput.InputGetMouseButtonDelegate = GetMouseButton;
				KickStarter.playerInput.InputGetMouseButtonDownDelegate = GetMouseButtonDown;

				currentMousePosition = new Vector2 (Screen.width * 0.5f, Screen.height * 0.5f);
				KickStarter.settingsManager.assumeInputsDefined = false;
			}
		}


		#if UNITY_5_4_OR_NEWER
		private void Awake ()
		{
			UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneLoaded;
		}
		private void OnDestroy ()
		{
			UnityEngine.SceneManagement.SceneManager.sceneLoaded -= SceneLoaded;
		}
		private void SceneLoaded (UnityEngine.SceneManagement.Scene _scene, UnityEngine.SceneManagement.LoadSceneMode _loadSceneMode)
		{
			AssignOverrides ();
		}
		#else
		private void OnLevelWasLoaded ()
		{
			AssignOverrides ();
		}
		#endif


		private Vector2 MousePosition (bool cursorIsLocked = false)
		{
			Vector2 mouseDelta = new Vector2 (OuyaSDK.OuyaInput.GetAxis(0, OuyaController.AXIS_LS_X),
											  OuyaSDK.OuyaInput.GetAxis(0, OuyaController.AXIS_LS_Y) * ((invertMouseY) ? -1f : 1f));
			
			if (Mathf.Abs (mouseDelta.x) < mouseDeadZone.x)
			{
				mouseDelta.x = 0f;
			}
			if (Mathf.Abs (mouseDelta.y) < mouseDeadZone.y)
			{
				mouseDelta.y = 0f;
			}
			mouseDelta *= Time.deltaTime * mouseMoveSpeed;

			Vector3 newTargetPosition = currentMousePosition;
			newTargetPosition = new Vector2 (Mathf.Clamp (newTargetPosition.x + (Screen.width * screenScale.x * mouseDelta.x), 0, Screen.width),
			                                 Mathf.Clamp (newTargetPosition.y + (Screen.height * screenScale.y * mouseDelta.y),0f,Screen.height));

			currentMousePosition = newTargetPosition;
			return currentMousePosition;
		}
		
		
		private float GetAxis (string axisName)
		{
			switch (axisName)
			{
			case "Horizontal":
				return OuyaSDK.OuyaInput.GetAxis(0, OuyaController.AXIS_LS_X);
			case "Vertical":
				return OuyaSDK.OuyaInput.GetAxis(0, OuyaController.AXIS_LS_Y);
			case "CursorHorizontal":
				return OuyaSDK.OuyaInput.GetAxis(0, OuyaController.AXIS_RS_X);
			case "CursorVertical":
				return OuyaSDK.OuyaInput.GetAxis(0, OuyaController.AXIS_RS_Y);
			default:
				ACDebug.LogError (string.Format("Unknown Axis: {0}", axisName));
				break;
			}
			return 0f;
		}
		

		private bool GetButton (string buttonName)
		{
			switch (buttonName)
			{
			case "InteractionA":
				return OuyaSDK.OuyaInput.GetButton (0, OuyaController.BUTTON_O);
			case "InteractionB":
				return OuyaSDK.OuyaInput.GetButton (0, OuyaController.BUTTON_A);
			case "ToggleCursor":
				return OuyaSDK.OuyaInput.GetButton (0, OuyaController.BUTTON_R1);
			case "EndCutscene":
				return OuyaSDK.OuyaInput.GetButton (0, OuyaController.BUTTON_Y);
			case "Jump":
				return OuyaSDK.OuyaInput.GetButton (0, OuyaController.BUTTON_U);
			case "Run":
				return OuyaSDK.OuyaInput.GetButton (0, OuyaController.BUTTON_L1);
			case "FlashHotspots":
				return OuyaSDK.OuyaInput.GetButton (0, OuyaController.BUTTON_R3);
			case "Menu":
				return OuyaSDK.OuyaInput.GetButton (0, OuyaController.BUTTON_MENU);
			default:
				ACDebug.LogError (string.Format("Unknown Button: {0}", buttonName));
				return false;
			}
		}
		
		
		private bool GetButtonDown (string buttonName)
		{
			switch (buttonName)
			{
			case "InteractionA":
				return OuyaSDK.OuyaInput.GetButtonDown (0, OuyaController.BUTTON_O);
			case "InteractionB":
				return OuyaSDK.OuyaInput.GetButtonDown (0, OuyaController.BUTTON_A);
			case "ToggleCursor":
				return OuyaSDK.OuyaInput.GetButtonDown (0, OuyaController.BUTTON_R1);
			case "EndCutscene":
				return OuyaSDK.OuyaInput.GetButtonDown (0, OuyaController.BUTTON_Y);
			case "Jump":
				return OuyaSDK.OuyaInput.GetButtonDown (0, OuyaController.BUTTON_U);
			case "Run":
				return OuyaSDK.OuyaInput.GetButtonDown (0, OuyaController.BUTTON_L1);
			case "FlashHotspots":
				return OuyaSDK.OuyaInput.GetButtonDown (0, OuyaController.BUTTON_R3);
			case "Menu":
				return OuyaSDK.OuyaInput.GetButtonDown (0, OuyaController.BUTTON_MENU);
			default:
				ACDebug.LogError (string.Format ("Unknown Button: {0}", buttonName));
				return false;
			}
		}
		
		
		private bool GetMouseButton (int button)
		{
			switch (button)
			{
			case 0:
				return OuyaSDK.OuyaInput.GetButton (0, OuyaController.BUTTON_O);
			case 1:
				return OuyaSDK.OuyaInput.GetButton (0, OuyaController.BUTTON_A);
			default:
				ACDebug.LogError (string.Format ("Unknown Button: {0}", button));
				return false;
			}
		}
		
		
		private bool GetMouseButtonDown (int button)
		{
			switch (button)
			{
			case 0:
				return OuyaSDK.OuyaInput.GetButtonDown (0, OuyaController.BUTTON_O);
			case 1:
				return OuyaSDK.OuyaInput.GetButtonDown (0, OuyaController.BUTTON_A);
			default:
				ACDebug.LogError (string.Format("Unknown Button: {0}", button));
				return false;
			}
		}

		#endif

	}

}