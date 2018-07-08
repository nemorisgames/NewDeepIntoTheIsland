/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"SceneSettings.cs"
 * 
 *	This script defines which cutscenes play when the scene is loaded,
 *	and where the player should begin from.
 * 
 */

#if UNITY_STANDALONE && (UNITY_5 || UNITY_2017_1_OR_NEWER || UNITY_PRO_LICENSE)
#define ALLOW_MOVIETEXTURES
#endif

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	/**
	 * This component is where settings specific to a scene are stored, such as the navigation method, and the Cutscene to play when the scene begins.
	 * The SceneManager provides a UI to assign these fields.
	 * This component should be placed on the GameEngine prefab.
	 */
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_scene_settings.html")]
	#endif
	public class SceneSettings : MonoBehaviour
	{

		/** The source of Actions used for the scene's main cutscenes (InScene, AssetFile) */
		public ActionListSource actionListSource = ActionListSource.InScene;

		/** The Cutscene to run whenever the game beings from this scene, or when this scene is visited during gameplay, if actionListSource = ActionListSource.InScene */
		public Cutscene cutsceneOnStart;
		/** The Cutscene to run whenever this scene is loaded after restoring a saved game file, if actionListSource = ActionListSource.InScene */
		public Cutscene cutsceneOnLoad;
		/** The Cutscene to run whenever a variable's value is changed, if actionListSource = ActionListSource.InScene */
		public Cutscene cutsceneOnVarChange;

		/** The ActionListAsset to run whenever the game beings from this scene, or when this scene is visited during gameplay, if actionListSource = ActionListSource.AssetFile */
		public ActionListAsset actionListAssetOnStart;
		/** The ActionListAsset to run whenever this scene is loaded after restoring a saved game file, if actionListSource = ActionListSource.AssetFile */
		public ActionListAsset actionListAssetOnLoad;
		/** The ActionListAsset to run whenever a variable's value is changed, if actionListSource = ActionListSource.AssetFile */
		public ActionListAsset actionListAssetOnVarChange;

		/** The scene's default PlayerStart prefab */
		public PlayerStart defaultPlayerStart;
		/** The scene's navigation method (meshCollider, UnityNavigation, PolygonCollider) */
		public AC_NavigationMethod navigationMethod = AC_NavigationMethod.meshCollider;
		/** The class name of the NavigationEngine ScriptableObject that is used to handle pathfinding, if navigationMethod = AC_NavigationMethod.Custom */
		public string customNavigationClass;
		/** The scene's active NavigationMesh, if navigationMethod != AC_NavigationMethod.UnityNavigation */
		public NavigationMesh navMesh;
		/** The scene's default SortingMap prefab */
		public SortingMap sortingMap;
		/** The scene's default Sound prefab */
		public Sound defaultSound;
		/** The scene's default LightMap prefab */
		public TintMap tintMap;

		/** The scene's attributes */
		public List<InvVar> attributes = new List<InvVar>();

		/** If this is assigned, and the currently-loaded Manager assets do not match those defined within, then a Warning message will appear in the Console */
		public ManagerPackage requiredManagerPackage;

		/** If True, then the global verticalReductionFactor in SettingsManager will be overridden with a scene-specific value */
		public bool overrideVerticalReductionFactor = false;
		/** How much slower vertical movement is compared to horizontal movement, if the game is in 2D and overriderVerticalReductionFactor = True */
		public float verticalReductionFactor = 0.7f;

		/** The distance to offset a character by when it is in the same area of a SortingMap as another (to correct display order) */
		public float sharedLayerSeparationDistance = 0.001f;

		[SerializeField] private bool overrideCameraPerspective = false;
		[SerializeField] private CameraPerspective cameraPerspective;
		[SerializeField] private MovingTurning movingTurning = MovingTurning.Unity2D;

		#if ALLOW_MOVIETEXTURES
		private MovieTexture fullScreenMovie;
		#endif
		private IStateChange[] stateChangeHooks;
		private AudioSource defaultAudioSource;
		

		public void OnAwake ()
		{
			KickStarter.navigationManager.OnAwake ();

			// Turn off all NavMesh objects
			NavigationMesh[] navMeshes = FindObjectsOfType (typeof (NavigationMesh)) as NavigationMesh[];
			foreach (NavigationMesh _navMesh in navMeshes)
			{
				if (navMesh != _navMesh)
				{
					_navMesh.TurnOff ();
				}
			}
			
			if (navMesh)
			{
				navMesh.TurnOn ();
			}

			stateChangeHooks = GetStateChangeHooks (GetComponents (typeof (IStateChange)));

			if (defaultSound != null)
			{
				defaultAudioSource = defaultSound.GetComponent <AudioSource>();
			}
		}
		
		
		public void OnStart ()
		{
			if (KickStarter.settingsManager == null || KickStarter.settingsManager.IsInLoadingScene ())
			{
				return;
			}

			if (KickStarter.saveSystem.loadingGame == LoadingGame.No)
			{
				KickStarter.levelStorage.ReturnCurrentLevelData (false);
				FindPlayerStart ();
			}
			else if (KickStarter.saveSystem.loadingGame == LoadingGame.JustSwitchingPlayer)
			{
				KickStarter.levelStorage.ReturnCurrentLevelData (false);
			}
			else
			{
			//	KickStarter.saveSystem.loadingGame = LoadingGame.No;
			}

			CheckRequiredManagerPackage ();
			PlayStartCutscene ();
		}
		

		/**
		 * Links all SortingMaps with their associated FollowSortingMaps.
		 */
		public void UpdateAllSortingMaps ()
		{
			if (KickStarter.stateHandler != null)
			{
				foreach (FollowSortingMap followSortingMap in KickStarter.stateHandler.FollowSortingMaps)
				{
					followSortingMap.UpdateSortingMap ();
				}
			}
		}
		
		
		private void FindPlayerStart ()
		{
			PlayerStart playerStart = GetPlayerStart ();
			if (playerStart != null)
			{
				playerStart.SetPlayerStart ();
			}
		}


		private void PlayStartCutscene ()
		{
			if (KickStarter.saveSystem.loadingGame != LoadingGame.No)
			{
				return;
			}

			bool playedGlobal = KickStarter.stateHandler.PlayGlobalOnStart ();
			if (!playedGlobal)
			{
				// Place in a temporary cutscene to set everything up
				KickStarter.stateHandler.gameState = GameState.Cutscene;
			}
			Invoke ("RunCutsceneOnStart", 0.01f);
	
			if (cutsceneOnStart == null && !KickStarter.playerMenus.ArePauseMenusOn ())
			{
				KickStarter.stateHandler.gameState = GameState.Normal;
			}
		}


		private void RunCutsceneOnStart ()
		{
			KickStarter.eventManager.Call_OnStartScene ();

			if (actionListSource == ActionListSource.InScene)
			{
				if (cutsceneOnStart != null)
				{
					KickStarter.stateHandler.gameState = GameState.Normal;
					cutsceneOnStart.Interact ();
				}
			}
			else if (actionListSource == ActionListSource.AssetFile)
			{
				if (actionListAssetOnStart != null)
				{
					KickStarter.stateHandler.gameState = GameState.Normal;
					actionListAssetOnStart.Interact ();
				}
			}
		}
		

		/**
		 * <summary>Gets the appropriate PlayerStart to use when the scene begins.</summary>
		 * <returns>The appropriate PlayerStart to use when the scene begins</returns>
		 */
		public PlayerStart GetPlayerStart ()
		{
			PlayerStart[] starters = FindObjectsOfType (typeof (PlayerStart)) as PlayerStart[];
			foreach (PlayerStart starter in starters)
			{
				if (starter.chooseSceneBy == ChooseSceneBy.Name && starter.previousSceneName != "" && starter.previousSceneName == KickStarter.sceneChanger.previousSceneInfo.name)
				{
					return starter;
				}
				if (starter.chooseSceneBy == ChooseSceneBy.Number && starter.previousScene > -1 && starter.previousScene == KickStarter.sceneChanger.previousSceneInfo.number)
				{
					return starter;
				}
			}
			
			if (defaultPlayerStart)
			{
				return defaultPlayerStart;
			}
			
			return null;
		}
		

		/**
		 * Runs the "cutsceneOnLoad" Cutscene.
		 */
		public void OnLoad ()
		{
			if (actionListSource == ActionListSource.InScene)
			{
				if (cutsceneOnLoad != null)
				{
					cutsceneOnLoad.Interact ();
				}
			}
			else if (actionListSource == ActionListSource.AssetFile)
			{
				if (actionListAssetOnLoad != null)
				{
					actionListAssetOnLoad.Interact ();
				}
			}
		}
		

		/**
		 * <summary>Plays an AudioClip on the default Sound prefab.</summary>
		 * <param name = "audioClip">The AudioClip to play</param>
		 * <param name = "doLoop">If True, the sound will loop</param>
		 */
		public void PlayDefaultSound (AudioClip audioClip, bool doLoop)
		{
			if (defaultSound == null)
			{
				ACDebug.Log ("Cannot play sound since no Default Sound Prefab is defined - please set one in the Scene Manager.");
				return;
			}
			
			if (audioClip != null && defaultAudioSource != null)
			{
				defaultAudioSource.clip = audioClip;
				defaultSound.Play (doLoop);
			}
		}


		/**
		 * Pauses the game by freezing time and sounds.
		 */
		public void PauseGame ()
		{
			// Work out which Sounds will have to be re-played after pausing
			Sound[] sounds = FindObjectsOfType (typeof (Sound)) as Sound[];
			List<Sound> soundsToResume = new List<Sound>();
			foreach (Sound sound in sounds)
			{
				if (sound.playWhilePaused && sound.IsPlaying ())
				{
					soundsToResume.Add (sound);
				}
			}

			#if !(UNITY_5 || UNITY_2017_1_OR_NEWER)
			// Disable Interactive Cloth components
			InteractiveCloth[] interactiveCloths = FindObjectsOfType (typeof (InteractiveCloth)) as InteractiveCloth[];
			foreach (InteractiveCloth interactiveCloth in interactiveCloths)
			{
				interactiveCloth.enabled = false;
			}
			#endif

			Time.timeScale = 0f;
			//AudioListener.pause = true; // Now delay audiolistener pause for a frame becuase of a bug with Unity 2017

			#if ALLOW_MOVIETEXTURES
			if (fullScreenMovie != null)
			{
				fullScreenMovie.Pause ();
			}
			#endif

			#if !(UNITY_5 || UNITY_2017_1_OR_NEWER)
			foreach (Sound sound in soundsToResume)
			{
				sound.ContinueFix ();
			}
			#endif

			StartCoroutine (PauseAudio ());
		}


		private IEnumerator PauseAudio ()
		{
			yield return null;
			AudioListener.pause = true;
		}


		/**
		 * Unpauses the game by unfreezing time.
		 */
		public void UnpauseGame (float newScale)
		{
			StopAllCoroutines ();

			Time.timeScale = newScale;

			#if ALLOW_MOVIETEXTURES
			if (fullScreenMovie != null)
			{
				fullScreenMovie.Play ();
			}
			#endif

			#if !(UNITY_5 || UNITY_2017_1_OR_NEWER)
			// Enable Interactive Cloth components
			InteractiveCloth[] interactiveCloths = FindObjectsOfType (typeof (InteractiveCloth)) as InteractiveCloth[];
			foreach (InteractiveCloth interactiveCloth in interactiveCloths)
			{
				interactiveCloth.enabled = true;
			}
			#endif
		}

		/**
		 * <summary>Gets how much slower vertical movement is compared to horizontal movement, if the game is in 2D.</summary>
		 * <returns>Gets how much slower vertical movement is compared to horizontal movement</returns>
		 */
		public float GetVerticalReductionFactor ()
		{
			if (overrideVerticalReductionFactor)
			{
				return verticalReductionFactor;
			}
			return KickStarter.settingsManager.verticalReductionFactor;
		}


		#if ALLOW_MOVIETEXTURES
		/**
		 * <summary>Assigns a MovieTexture as the one to pause when the game is paused.</summary>
		 * <param name = "movieTexture">The MovieTexture to pause when the game is paused.</summary>
		 */
		public void SetFullScreenMovie (MovieTexture movieTexture)
		{
			fullScreenMovie = movieTexture;
		}


		/**
		 * <summary>Unassigns the currently-set MovieTexture to pause when the game is paused.
		 * This should be called once the movie has finished playing.</summary>
		 */
		public void StopFullScreenMovie ()
		{
			fullScreenMovie = null;
		}

		#endif


		/*
		 * <summary>Calls any IStateCheck hooks when the game state is changed.</summary>
		 */
		public void OnStateChange ()
		{
			if (stateChangeHooks != null && stateChangeHooks.Length > 0)
			{
				GameState gameState = KickStarter.stateHandler.gameState;
				foreach (IStateChange stateChangeHook in stateChangeHooks)
				{
					stateChangeHook.OnStateChange (gameState);
				}
			}
		}


		private IStateChange[] GetStateChangeHooks (IList list)
		{
			IStateChange[] ret = new IStateChange [list.Count];
			list.CopyTo (ret, 0);
			return ret;
		}


		private void CheckRequiredManagerPackage ()
		{
			if (requiredManagerPackage == null)
			{
				return;
			}

			#if UNITY_EDITOR

			if ((requiredManagerPackage.sceneManager != null && requiredManagerPackage.sceneManager != KickStarter.sceneManager) ||
			    (requiredManagerPackage.settingsManager != null && requiredManagerPackage.settingsManager != KickStarter.settingsManager) ||
			    (requiredManagerPackage.actionsManager != null && requiredManagerPackage.actionsManager != KickStarter.actionsManager) ||
			    (requiredManagerPackage.variablesManager != null && requiredManagerPackage.variablesManager != KickStarter.variablesManager) ||
			    (requiredManagerPackage.inventoryManager != null && requiredManagerPackage.inventoryManager != KickStarter.inventoryManager) ||
			    (requiredManagerPackage.speechManager != null && requiredManagerPackage.speechManager != KickStarter.speechManager) ||
			    (requiredManagerPackage.cursorManager != null && requiredManagerPackage.cursorManager != KickStarter.cursorManager) ||
			    (requiredManagerPackage.menuManager != null && requiredManagerPackage.menuManager != KickStarter.menuManager))
			{
				if (requiredManagerPackage.settingsManager != null)
				{
					if (requiredManagerPackage.settingsManager.name == "Demo_SettingsManager" && UnityVersionHandler.GetCurrentSceneName () == "Basement")
					{
						ACDebug.LogWarning ("The demo scene's required Manager asset files are not all loaded - please stop the game, and choose 'Adventure Creator -> Getting started -> Load 3D Demo managers from the top toolbar, and re-load the scene.");
						return;
					}
					else if (requiredManagerPackage.settingsManager.name == "Demo2D_SettingsManager" && UnityVersionHandler.GetCurrentSceneName () == "Park")
					{
						ACDebug.LogWarning ("The 2D demo scene's required Manager asset files are not all loaded - please stop the game, and choose 'Adventure Creator -> Getting started -> Load 2D Demo managers from the top toolbar, and re-load the scene.");
						return;
					}
				}

				ACDebug.LogWarning ("This scene's required Manager asset files are not all loaded - please find the asset file '" + requiredManagerPackage.name + "' and click 'Assign managers' in its Inspector.");
			}

			#endif
		}


		/**
		 * <summary>Gets a scene attribute.</summary>
		 * <param name = "ID">The ID number of the attribute to get</param>
		 * <returns>The attribute of the scene</returns>
		 */
		public InvVar GetAttribute (int ID)
		{
			if (ID >= 0)
			{
				foreach (InvVar attribute in attributes)
				{
					if (attribute.id == ID)
					{
						return attribute;
					}
				}
			}
			return null;
		}


		/**
		 * <summary>Checks if the scene is in 2D, and plays in screen-space (i.e. characters do not move towards or away from the camera).</summary>
		 * <returns>True if the game is in 2D, and plays in screen-space</returns>
		 */
		public static bool ActInScreenSpace ()
		{
			if (KickStarter.sceneSettings != null && KickStarter.sceneSettings.overrideCameraPerspective)
			{
				if ((KickStarter.sceneSettings.movingTurning == MovingTurning.ScreenSpace || KickStarter.sceneSettings.movingTurning == MovingTurning.Unity2D) && KickStarter.sceneSettings.cameraPerspective == CameraPerspective.TwoD)
				{
					return true;
				}
			}
			else if (KickStarter.settingsManager != null)
			{
				if ((KickStarter.settingsManager.movingTurning == MovingTurning.ScreenSpace || KickStarter.settingsManager.movingTurning == MovingTurning.Unity2D) && KickStarter.settingsManager.cameraPerspective == CameraPerspective.TwoD)
				{
					return true;
				}
			}
			return false;
		}
		

		/**
		 * <summary>Checks if the scene uses Unity 2D for its camera perspective.<summary>
		 * <returns>True if the game uses Unity 2D for its camera perspective</returns>
		 */
		public static bool IsUnity2D ()
		{
			if (KickStarter.sceneSettings != null && KickStarter.sceneSettings.overrideCameraPerspective)
			{
				if (KickStarter.sceneSettings.movingTurning == MovingTurning.Unity2D && KickStarter.sceneSettings.cameraPerspective == CameraPerspective.TwoD)
				{
					return true;
				}
			}
			else if (KickStarter.settingsManager != null)
			{
				if (KickStarter.settingsManager.movingTurning == MovingTurning.Unity2D && KickStarter.settingsManager.cameraPerspective == CameraPerspective.TwoD)
				{
					return true;
				}
			}
			return false;
		}


		/**
		 * <summary>Checks if the scene uses Top Down for its camera perspective.<summary>
		 * <returns>True if the game uses Top Down for its camera perspective</returns>
		 */
		public static bool IsTopDown ()
		{
			if (KickStarter.sceneSettings != null && KickStarter.sceneSettings.overrideCameraPerspective)
			{
				if (KickStarter.sceneSettings.movingTurning == MovingTurning.TopDown && KickStarter.sceneSettings.cameraPerspective == CameraPerspective.TwoD)
				{
					return true;
				}
			}
			else if (KickStarter.settingsManager != null)
			{
				if (KickStarter.settingsManager.movingTurning == MovingTurning.TopDown && KickStarter.settingsManager.cameraPerspective == CameraPerspective.TwoD)
				{
					return true;
				}
			}
			return false;
		}


		/**
		 * The camera perspective of the current scene.
		 */
		public static CameraPerspective CameraPerspective
		{
			get
			{
				if (KickStarter.sceneSettings != null && KickStarter.sceneSettings.overrideCameraPerspective)
				{
					return KickStarter.sceneSettings.cameraPerspective;
				}
				else if (KickStarter.settingsManager != null)
				{
					return KickStarter.settingsManager.cameraPerspective;
				}
				return CameraPerspective.ThreeD;
			}
		}


		public bool OverridesCameraPerspective ()
		{
			return overrideCameraPerspective;
		}


		#if UNITY_EDITOR

		private string[] cameraPerspective_list = { "2D", "2.5D", "3D" };

		public void SetOverrideCameraPerspective (CameraPerspective _cameraPerspective, MovingTurning _movingTurning)
		{
			overrideCameraPerspective = true;
			cameraPerspective = _cameraPerspective;
			movingTurning = _movingTurning;
		}


		public void ShowCameraOverrideLabel ()
		{
			if (overrideCameraPerspective)
			{
				int cameraPerspective_int = (int) cameraPerspective;

				string persp = cameraPerspective_list[cameraPerspective_int];
				if (cameraPerspective == CameraPerspective.TwoD) persp += " (" + movingTurning + ")";
				UnityEditor.EditorGUILayout.LabelField ("This scene's camera perspective is overriding the default and is " + persp + ".", UnityEditor.EditorStyles.boldLabel);

			}
		}

		#endif

	}
	
}
