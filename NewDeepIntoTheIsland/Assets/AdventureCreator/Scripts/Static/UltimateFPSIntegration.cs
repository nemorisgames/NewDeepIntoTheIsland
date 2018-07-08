/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"UltimateFPSIntegration.cs"
 * 
 *	This script serves as a bridge between Adventure Creator and Ultimate FPS.
 *	To use it, add it to your UFPS player's root object, and set your AC Movement method to 'First Person'.
 *
 *	To allow for UFPS integration, the 'UltimateFPSIsPresent'
 *	preprocessor must be defined.  This can be done from
 *	Edit -> Project Settings -> Player, and entering
 *	'UltimateFPSIsPresent' into the Scripting Define Symbols text box
 *	for your game's build platform.
 *
 *	This bridge script provides a robust integration for a simple UFPS character. If you wish to build upon it for more custom gameplay,
 *	duplicate the script and make such changes to the copy.  You can then add your new script to the UFPS player instead.
 * 
 */


using UnityEngine;


namespace AC
{

	/**
	 * This script serves as a bridge between Adventure Creator and Ultimate FPS.
	 * To use it, add it to your UFPS player's root object, and set your AC Movement method to 'First Person'.
	 *
	 * To allow for UFPS integration, the 'UltimateFPSIsPresent'
	 * preprocessor must be defined.  This can be done from
	 * Edit -> Project Settings -> Player, and entering
	 * 'UltimateFPSIsPresent' into the Scripting Define Symbols text box
	 * for your game's build platform.
	 *
	 * This bridge script provides a robust integration for a simple UFPS character. See the comments inside the script for information on how it works.
	 * If you wish to build upon it for more custom gameplay, duplicate the script and make such changes to the copy.
	 * You can then add your new script to the UFPS player instead.
	 */
	[RequireComponent (typeof (Player))]
	[AddComponentMenu("Adventure Creator/3rd-party/UFPS integration")]
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_ultimate_f_p_s_integration.html")]
	#endif
	public class UltimateFPSIntegration : MonoBehaviour
	{

		#if UltimateFPSIsPresent

		/** The player's walk speed when under AC's control */
		public float walkSpeed = 5f;
		/** The player's run speed when under AC's control */
		public float runSpeed = 10f;
		/** The player's turn speed when under AC's control */
		public float turnSpeed = 60f;
		/** If True, then weapons will be disabled */
		public bool disableWeapons = false;

		private vp_FPCamera fpCamera;
		private vp_FPController fpController;
		private vp_FPInput fpInput;
		private vp_FPPlayerEventHandler fpPlayerEventHandler;
		private vp_SimpleHUD simpleHUD;
		private vp_SimpleCrosshair simpleCrosshair;

		private AudioListener _audioListener;
		private Player player;
		private bool isClimbing;
		
		#endif
		
		
		private void Awake ()
		{
			#if UltimateFPSIsPresent

			// Tag the GameObject as 'Player' if it is not already
			gameObject.tag = Tags.player;
			player = GetComponent <Player>();
			
			// Assign the UFPS components, and report warnings if they are not present
			fpCamera = GetComponentInChildren <vp_FPCamera>();
			fpController = GetComponentInChildren <vp_FPController>();
			fpInput = GetComponentInChildren <vp_FPInput>();
			fpPlayerEventHandler = GetComponentInChildren <vp_FPPlayerEventHandler>();
			simpleHUD = GetComponentInChildren <vp_SimpleHUD>();
			simpleCrosshair = GetComponentInChildren <vp_SimpleCrosshair>();
			_audioListener = GetComponentInChildren <AudioListener>();
			
			if (fpController == null)
			{
				ACDebug.LogWarning ("Cannot find UFPS script 'vp_FPController' anywhere on '" + gameObject.name + "'.");
			}
			if (fpInput == null)
			{
				ACDebug.LogWarning ("Cannot find UFPS script 'vp_FPInput' anywhere on '" + gameObject.name + "'.");
			}
			if (fpCamera == null)
			{
				ACDebug.LogWarning ("Cannot find UFPS script 'vp_FPCamera' anywhere on '" + gameObject.name + "'.");
			}
			if (KickStarter.settingsManager != null && KickStarter.settingsManager.movementMethod != MovementMethod.FirstPerson)
			{
				ACDebug.Log ("The Ultimate FPS integration script requires the Settings Manager's 'Movement method' is set to 'First Person'");
			}

			#endif
		}
		
		
		private void Start ()
		{
			#if !UltimateFPSIsPresent
			
			ACDebug.LogError ("'UltimateFPSIsPresent' must be listed in your Unity Player Setting's 'Scripting define symbols' for AC's UFPS integration to work.", this);
			return;
			
			#else

			// Tell the AC Player component that we'l be controlling movement/rotation manually during Cutscenes
			player.FirstPersonCamera = fpCamera.transform;
			player.SetAnimEngine (AnimationEngine.Custom);
			player.motionControl = MotionControl.Manual;

			// Assign a short delay whenever we load a saved game, to prevent firing when click
			if (KickStarter.saveSystem)
			{
				KickStarter.saveSystem.SetGameplayReturnTime (0.1f);
			}

			// Fixes gun sounds from not always playing
			AudioListener.pause = false;

			// Add events to climbing interactable
			if (fpPlayerEventHandler != null)
			{
				fpPlayerEventHandler.Climb.StartCallbacks += OnStartClimb;
				fpPlayerEventHandler.Climb.StopCallbacks += OnStopClimb;
			}
			
			#endif
		}
		
		
		#if UltimateFPSIsPresent
		
		private void Update ()
		{
			// If AC is disabled or not present in the scene, ignore this script
			if (!CanOperate ())
			{
				return;
			}

			if (isClimbing)
			{
				SetMovementState (false);
			}
			else if (KickStarter.settingsManager.movementMethod != MovementMethod.FirstPerson)
			{
				// Disable everything unless we are using First Person movement
				SetMovementState (false);
				SetCameraEnabled (false);
				SetInputState (false);
				SetWeaponState (false);
				SetCursorState (true);
				return;
			}
			else if (KickStarter.stateHandler.gameState == GameState.Normal)
			{
				// During normal gameplay, give the player freedom of movement
				bool canControlPlayer = KickStarter.playerInput.CanDirectControlPlayer ();

				SetMovementState (canControlPlayer);
				SetCameraEnabled (true);
				SetInputState (canControlPlayer);
			}
			else if (KickStarter.stateHandler.gameState == GameState.DialogOptions && KickStarter.settingsManager.useFPCamDuringConversations)
			{
				// During a Conversation, restrict player movement, but don't disable the UFPS camera
				SetMovementState (false);
				SetCameraEnabled (true);
				SetInputState (false);
			}
			else if (KickStarter.stateHandler.gameState != GameState.Paused)
			{
				// During a Cutscene, manually control the player's position and rotation and restrict UFPS's control
				OverrideMovement ();
				
				SetMovementState (false);
				SetCameraEnabled (false);
				SetInputState (false);
			}
			
			// Disable weapons if we are in a Cutscene, loading a saved game, or moving a Draggable object
			if (disableWeapons ||
				KickStarter.stateHandler.gameState != GameState.Normal ||
				KickStarter.playerInput.GetDragState () != DragState.None ||
				KickStarter.saveSystem.loadingGame != LoadingGame.No)
			{
				SetWeaponState (false);
			}
			else
			{
				SetWeaponState (KickStarter.playerInput.IsCursorLocked ());
			}

			// Override the mouse completely if we've unlocked the cursor
			if (KickStarter.stateHandler.gameState == GameState.Normal)
			{
				SetCursorState (!KickStarter.playerInput.IsCursorLocked ());
				SetHUDState (true);
			}
			else
			{
				SetCursorState (true);
				SetHUDState (false);
			}
		}


		/*
		 * Any function named 'OnTeleport' present on a character's root object will be called after they are teleported.
		 */
		private void OnTeleport ()
		{
			Vector3 movePosition = player.GetTargetPosition ();

			Quaternion rot = player.GetTargetRotation ();
			Vector3 angles = rot.eulerAngles;
			angles.x = GetTiltAngle ();

			Teleport (movePosition, angles);
		}
		
		
		private void OverrideMovement ()
		{
			// Calculate AC's intended player position 
			Vector3 movePosition = (player.GetTargetPosition () - transform.position).normalized * Time.deltaTime;
			movePosition *= (player.isRunning) ? runSpeed : walkSpeed;
			movePosition += transform.position;

			// Calculate AC's intended player rotation
			Quaternion rot = Quaternion.RotateTowards (transform.rotation, player.GetTargetRotation (), turnSpeed * Time.deltaTime);
			Vector3 angles = rot.eulerAngles;
			angles.x = GetTiltAngle ();
			
			Teleport (movePosition, angles);
		}
		
		
		private float GetTiltAngle ()
		{
			// Get AC's head-tilt angle, if appropriate

			if (player.IsTilting ())
			{
				return player.GetTilt ();
			}
			return fpCamera.transform.localEulerAngles.x;
		}
		
		
		private void Teleport (Vector3 position, Vector3 eulerAngles)
		{
			// Set the controller's position, and camera's rotation

			if (fpController != null)
			{
				fpController.SetPosition (position);
			}
			if (fpCamera != null)
			{
				fpCamera.SetRotation (eulerAngles);
			}
		}
		
		
		private void SetCameraEnabled (bool state, bool force = false)
		{
			/*
			 * Both AC and UFPS like to have their camera tagged as 'MainCamera', which causes conflict.
			 * Therefore, this function ensures only one of these cameras is tagged as 'MainCamera' at any one time.
			 * The same goes for the UFPS camera's AudioListener
			 */

			if (fpCamera != null && KickStarter.mainCamera != null)
			{
				if (state)
				{
					KickStarter.mainCamera.attachedCamera = null;
				}
				
				if (KickStarter.mainCamera.attachedCamera == null && !state && !force)
				{
					// Don't do anything if the MainCamera has nothing else to do
					fpCamera.tag = Tags.mainCamera;
					KickStarter.mainCamera.SetCameraTag (Tags.untagged);
					return;
				}
				
				// Need to disable camera, not gameobject, otherwise weapon cams will get wrong FOV
				foreach (Camera _camera in fpCamera.GetComponentsInChildren <Camera>())
				{
					_camera.enabled = state;
				}
				
				if (_audioListener)
				{
					_audioListener.enabled = state;
					KickStarter.mainCamera.SetAudioState (!state);
				}
				
				if (state)
				{
					fpCamera.tag = Tags.mainCamera;
					KickStarter.mainCamera.SetCameraTag (Tags.untagged);
					KickStarter.mainCamera.Disable ();
				}
				else
				{
					fpCamera.tag = Tags.untagged;
					KickStarter.mainCamera.SetCameraTag (Tags.mainCamera);
					KickStarter.mainCamera.Enable ();
				}
			}
		}
		
		
		private void SetMovementState (bool state)
		{
			if (fpController)
			{
				if (state == false)
				{
					fpController.Stop ();
				}
			}
		}
		
		
		private void SetInputState (bool state)
		{
			if (fpInput)
			{
				fpInput.AllowGameplayInput = state;
			}
		}
		
		
		private void SetWeaponState (bool state)
		{
			if (KickStarter.playerInput.IsFreeAimingLocked ())
			{
				state = false;
			}
			
			if (!state)
			{
				if (fpInput != null && fpInput.FPPlayer != null)
				{
					fpInput.FPPlayer.Attack.TryStop ();
				}
				
			}
		}


		private void SetCursorState (bool state)
		{
			if (fpInput)
			{
				fpInput.MouseCursorForced = state;
			}
		}


		private void SetHUDState (bool state)
		{
			if (simpleHUD)
			{
				simpleHUD.ShowHUD = state;
			}

			if (simpleCrosshair)
			{
				simpleCrosshair.Hide = !state;
			}
		}
		
		
		private bool CanOperate ()
		{
			if (KickStarter.stateHandler == null ||
			    KickStarter.playerInput == null ||
			    !KickStarter.stateHandler.IsACEnabled () ||
			    (KickStarter.settingsManager != null && KickStarter.settingsManager.IsInLoadingScene ()))
			{
				return false;
			}
			return true;
		}


		private void OnStartClimb ()
		{
			isClimbing = true;
		}


		private void OnStopClimb ()
		{
			isClimbing = false;
		}
		
		#endif

	}
	
}