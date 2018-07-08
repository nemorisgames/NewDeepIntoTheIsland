/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"SaveSystem.cs"
 * 
 *	This script processes saved game data to and from the scene objects.
 * 
 * 	It is partially based on Zumwalt's code here:
 * 	http://wiki.unity3d.com/index.php?title=Save_and_Load_from_XML
 *  and uses functions by Nitin Pande:
 *  http://www.eggheadcafe.com/articles/system.xml.xmlserialization.asp 
 * 
 */

#if UNITY_WEBPLAYER || UNITY_WINRT || UNITY_WII || UNITY_PS4
#define SAVE_IN_PLAYERPREFS
#endif

#if UNITY_IPHONE || UNITY_WP8 || UNITY_WINRT || UNITY_WII || UNITY_PS4
#define SAVE_USING_XML
#endif

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	/**
	 * Processes save game data to and from scene objects.
	 */
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_save_system.html")]
	#endif
	public class SaveSystem : MonoBehaviour
	{

		/** What type of load is being performed (No, InNewScene, InSameScene, JustSwitchingPlayer) */
		public LoadingGame loadingGame;
		/** An List of SaveFile variables, storing all available save files. */
		public List<SaveFile> foundSaveFiles = new List<SaveFile>();
		/** A List of SaveFile variables, storing all available import files. */
		public List<SaveFile> foundImportFiles = new List<SaveFile>();

		public const string pipe = "|";
		public const string colon = ":";

		private float gameplayInvokeTime = 0.01f;
		private SaveData saveData = new SaveData ();
		private SelectiveLoad activeSelectiveLoad = new SelectiveLoad ();

		private static iSaveFileHandler saveFileHandlerOverride = null;
		private static iFileFormatHandler fileFormatHandlerOverride = null;

		private SaveFile requestedLoad = null;
		private SaveFile requestedImport = null;
		private SaveFile requestedSave = null;


		#if UNITY_5_4_OR_NEWER
		private bool loadedInitialScene = false;
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
		    if (loadedInitialScene)
		    {
		        _OnLevelWasLoaded ();
		    }
		    loadedInitialScene = true;
		}
		#else
		private void OnLevelWasLoaded ()
		{
		    _OnLevelWasLoaded ();
		}
		#endif


		/**
		 * <summary>Sets the delay after loading a saved game before gameplay is resumed. This is useful in games with custom systems, e.g. weapons, where we want to prevent firing being possible immediately after loading.</summary>
		 * <param name = "The new delay time, in seconds</param>
		 */
		public void SetGameplayReturnTime (float _gameplayInvokeTime)
		{
			gameplayInvokeTime = _gameplayInvokeTime;
		}


		/**
		 * Searches the filesystem for all available save files, and stores them in foundSaveFiles.
		 */
		public void GatherSaveFiles ()
		{
			foundSaveFiles = SaveFileHandler.GatherSaveFiles (Options.GetActiveProfileID ());

			if (KickStarter.settingsManager != null && KickStarter.settingsManager.orderSavesByUpdateTime)
			{
				foundSaveFiles.Sort (delegate (SaveFile a, SaveFile b) {return a.updatedTime.CompareTo (b.updatedTime);});
			}

			UpdateSaveFileLabels ();
		}


		private void UpdateSaveFileLabels ()
		{
			// Now get save file labels
			if (Options.optionsData != null && !string.IsNullOrEmpty (Options.optionsData.saveFileNames))
			{
				string[] profilesArray = Options.optionsData.saveFileNames.Split (SaveSystem.pipe[0]);
				foreach (string chunk in profilesArray)
				{
					string[] chunkData = chunk.Split (SaveSystem.colon[0]);
					
					int _id = 0;
					int.TryParse (chunkData[0], out _id);
					string _label = chunkData[1];

					for (int i=0; i<Mathf.Min (50, foundSaveFiles.Count); i++)
					{
						if (foundSaveFiles[i].saveID == _id)
						{
							SaveFile newSaveFile = new SaveFile (foundSaveFiles [i]);
							newSaveFile.SetLabel (_label);
							foundSaveFiles[i] = newSaveFile;
						}
					}
				}
			}
		}


		/**
		 * <summary>Searches the filesystem for all available import files, and stores them in foundImportFiles.</summary>
		 * <param name = "projectName">The project name of the game whose save files we're looking to import</param>
		 * <param name = "filePrefix">The "save filename" of the game whose save files we're looking to import, as set in the Settings Manager</param>
		 * <param name = "boolID">If >= 0, the ID of the boolean Global Variable that must be True for the file to be considered valid for import</param>
		 */
		public void GatherImportFiles (string projectName, string filePrefix, int boolID)
		{
			#if !UNITY_STANDALONE
			ACDebug.LogWarning ("Cannot import save files unless running on Windows, Mac or Linux standalone platforms.");
			#else
			foundImportFiles = SaveFileHandler.GatherImportFiles (Options.GetActiveProfileID (), boolID, projectName, filePrefix);
			#endif
		}


		/**
		 * <summary>Gets the extension of the current save method.</summary>
		 * <returns>The extension of the current save method</returns>
		 */
		public string GetSaveExtension ()
		{
			return FileFormatHandler.GetSaveExtension ();
		}


		/**
		 * <summary>Checks if an import file with a particular ID number exists.</summary>
		 * <param name = "saveID">The import ID to check for</param>
		 * <returns>True if an import file with a matching ID number exists</returns>
		 */
		public static bool DoesImportExist (int saveID)
		{
			if (KickStarter.saveSystem)
			{
				foreach (SaveFile file in KickStarter.saveSystem.foundImportFiles)
				{
					if (file.saveID == saveID)
					{
						return true;
					}
				}
			}
			return false;
		}


		/**
		 * <summary>Checks if a save file with a particular ID number exists</summary>
		 * <param name = "elementSlot">The slot index of the MenuSavesList element</param>
		 * <param name = "saveID">The save ID to check for</param>
		 * <param name = "useSaveID">If True, the saveID overrides the elementSlot to determine which file to check for</param>
		 * <returns>True if a save file with a matching ID number exists</returns>
		 */
		public static bool DoesSaveExist (int elementSlot, int saveID, bool useSaveID)
		{
			if (!useSaveID)
			{
				if (elementSlot >= 0 && KickStarter.saveSystem.foundSaveFiles.Count > elementSlot)
				{
					saveID = KickStarter.saveSystem.foundSaveFiles[elementSlot].saveID;
				}
				else
				{
					saveID = -1;
				}
			}

			if (KickStarter.saveSystem)
			{
				foreach (SaveFile file in KickStarter.saveSystem.foundSaveFiles)
				{
					if (file.saveID == saveID)
					{
						return true;
					}
				}
			}
			return false;
		}


		/**
		 * <summary>Checks if a save file with a particular ID number exists</summary>
		 * <param name = "saveID">The save ID to check for</param>
		 * <returns>True if a save file with a matching ID number exists</returns>
		 */
		public static bool DoesSaveExist (int saveID)
		{
			return DoesSaveExist (0, saveID, true);
		}


		/**
		 * Loads the AutoSave save file.  If multiple save profiles are used, the current profiles AutoSave will be loaded.
		 */
		public static void LoadAutoSave ()
		{
			if (KickStarter.saveSystem)
			{
				if (DoesSaveExist (0))
				{
					SaveSystem.LoadGame (0);
				}
				else
				{
					ACDebug.LogWarning ("Could not load autosave - file does not exist.");
				}
			}
		}


		/**
		 * <summary>Imports a save file from another Adventure Creator game.</summary>
		 * <param name = "elementSlot">The slot index of the MenuProfilesList element that was clicked on</param>
		 * <param name = "saveID">The save ID to import</param>
		 * <param name = "useSaveID">If True, the saveID overrides the elementSlot to determine which file to import</param>
		 */
		public static void ImportGame (int elementSlot, int saveID, bool useSaveID)
		{
			if (KickStarter.saveSystem)
			{
				if (!useSaveID)
				{
					if (KickStarter.saveSystem.foundImportFiles.Count > elementSlot)
					{
						saveID = KickStarter.saveSystem.foundImportFiles[elementSlot].saveID;
					}
				}
				
				if (saveID >= 0)
				{
					KickStarter.saveSystem.ImportSaveGame (saveID);
				}
			}
		}


		/**
		 * <summary>Sets the local instance of SelectiveLoad, which determines which save data is restored the next time (and only the next time) LoadGame is called.</summary>
		 * <param name = "selectiveLoad">An instance of SelectiveLoad the defines what elements to load</param>
		 */
		public void SetSelectiveLoadOptions (SelectiveLoad selectiveLoad)
		{
			activeSelectiveLoad = selectiveLoad;
		}


		/**
		 * Loads the last-recorded save game file.
		 */
		public static void ContinueGame ()
		{
			if (Options.optionsData != null && Options.optionsData.lastSaveID >= 0)
			{
				SaveSystem.LoadGame (Options.optionsData.lastSaveID);
			}
		}


		/**
		 * <summary>Loads a save game file.</summary>
		 * <param name = "saveID">The save ID of the file to load</param>
		 */
		public static void LoadGame (int saveID)
		{
			LoadGame (0, saveID, true);
		}


		/**
		 * <summary>Loads a save game file.</summary>
		 * <param name = "elementSlot">The slot index of the MenuSavesList element that was clicked on</param>
		 * <param name = "saveID">The save ID to load</param>
		 * <param name = "useSaveID">If True, the saveID overrides the elementSlot to determine which file to load</param>
		 */
		public static void LoadGame (int elementSlot, int saveID, bool useSaveID)
		{
			if (KickStarter.saveSystem)
			{
				if (!useSaveID)
				{
					if (elementSlot >= 0 && KickStarter.saveSystem.foundSaveFiles.Count > elementSlot)
					{
						saveID = KickStarter.saveSystem.foundSaveFiles[elementSlot].saveID;
					}
					else
					{
						ACDebug.LogWarning ("Can't select save slot " + elementSlot + " because only " + KickStarter.saveSystem.foundSaveFiles.Count + " have been found!");
					}
				}

				foreach (SaveFile foundSaveFile in KickStarter.saveSystem.foundSaveFiles)
				{
					if (foundSaveFile.saveID == saveID)
					{
						SaveFile saveFileToLoad = foundSaveFile;
						KickStarter.saveSystem.LoadSaveGame (saveFileToLoad);
						return;
					}
				}
				
				ACDebug.LogWarning ("Could not load game: file with ID " + saveID + " does not exist.");
			}
		}
		

		/**
		 * Clears all save data stored in the SaveData class.
		 */
		public void ClearAllData ()
		{
			saveData = new SaveData ();
		}


		/**
		 * <summary>Requests that a save game from another AC game be imported. This will call the iSaveFileHandler's Import function, which in turn will call ReceiveDataToImport once complete.</summary>
		 * <param name = "saveID">The ID number of the save file to import</param>
		 */
		private void ImportSaveGame (int saveID)
		{
			foreach (SaveFile foundImportFile in foundImportFiles)
			{
				if (foundImportFile.saveID == saveID)
				{
					requestedImport = new SaveFile (foundImportFile);
					SaveFileHandler.Load (foundImportFile, true);
					return;
				}
			}
		}


		/**
		 * <summary>Processes the save data requested by ImportSaveGame</summary>
		 * <param name = "saveFile">A data container for information about the save file to import.  Its saveID and profileID need to match up with that requested in the iSaveFileHandler's Import function in order for the data to be processed</param>
		 * <param name = "saveFileContents">The file contents of the save file. This is empty if the import failed.</param>
		 */
		public void ReceiveDataToImport (SaveFile saveFile, string saveFileContents)
		{
			if (requestedImport != null && saveFile != null && requestedImport.saveID == saveFile.saveID && requestedImport.profileID == saveFile.profileID)
			{
				// Received data matches requested
				requestedImport = null;

				if (!string.IsNullOrEmpty (saveFileContents))
				{
					KickStarter.eventManager.Call_OnImport (FileAccessState.Before);

					int divider = saveFileContents.IndexOf ("||");
					string mainData = saveFileContents.Substring (0, divider);

					saveData = (SaveData) Serializer.DeserializeObject <SaveData> (mainData);
					
					// Stop any current-running ActionLists, dialogs and interactions
					KillActionLists ();
					SaveSystem.AssignVariables (saveData.mainData.runtimeVariablesData);

					KickStarter.eventManager.Call_OnImport (FileAccessState.After);
				}
				else
				{
					KickStarter.eventManager.Call_OnImport (FileAccessState.Fail);
				}
			}
		}


		private void LoadSaveGame (SaveFile saveFile)
		{
			requestedLoad = new SaveFile (saveFile);
			SaveFileHandler.Load (saveFile, true);
		}


		/**
		 * <summary>Processes the save data requested by LoadSaveGame</summary>
		 * <param name = "saveFile">A data container for information about the save file to load.  Its saveID and profileID need to match up with that requested in the iSaveFileHandler's Load function in order for the data to be processed</param>
		 * <param name = "saveFileContents">The file contents of the save file. This is empty if the load failed.</param>
		 */
		public void ReceiveDataToLoad (SaveFile saveFile, string saveFileContents)
		{
			if (requestedLoad != null && saveFile != null && requestedLoad.saveID == saveFile.saveID && requestedLoad.profileID == saveFile.profileID)
			{
				// Received data matches requested
				requestedLoad = null;

				if (!string.IsNullOrEmpty (saveFileContents))
				{
					KickStarter.eventManager.Call_OnLoad (FileAccessState.Before);

					int divider = saveFileContents.IndexOf ("||");
					string mainData = saveFileContents.Substring (0, divider);
					string roomData = saveFileContents.Substring (divider + 2);

					saveData = (SaveData) Serializer.DeserializeObject <SaveData> (mainData);

					if (activeSelectiveLoad.loadSceneObjects)
					{
						KickStarter.levelStorage.allLevelData = FileFormatHandler.DeserializeAllRoomData (roomData);
					}

					// Stop any current-running ActionLists, dialogs and interactions
					KillActionLists ();
					
					// If player has changed, destroy the old one and load in the new one
					if (KickStarter.settingsManager.playerSwitching == PlayerSwitching.Allow)
					{
						if ((KickStarter.player == null && saveData.mainData.currentPlayerID != KickStarter.settingsManager.GetEmptyPlayerID ()) ||
							(KickStarter.player != null && KickStarter.player.ID != saveData.mainData.currentPlayerID))
						{
							KickStarter.ResetPlayer (GetPlayerByID (saveData.mainData.currentPlayerID), saveData.mainData.currentPlayerID, true, Quaternion.identity, false, true);
						}
					}

					int newScene = GetPlayerScene (saveData.mainData.currentPlayerID, saveData.playerData);
					
					// Load correct scene
					bool forceReload = KickStarter.settingsManager.reloadSceneWhenLoading;
					if (forceReload || (newScene != UnityVersionHandler.GetCurrentSceneNumber () && activeSelectiveLoad.loadScene))
					{
						loadingGame = LoadingGame.InNewScene;
						KickStarter.sceneChanger.ChangeScene (new SceneInfo ("", newScene), false, forceReload);
					}
					else
					{
						loadingGame = LoadingGame.InSameScene;

						// Already in the scene
						Sound[] sounds = FindObjectsOfType (typeof (Sound)) as Sound[];
						foreach (Sound sound in sounds)
						{
							if (sound.GetComponent <AudioSource>())
							{
								if (sound.soundType != SoundType.Music && !sound.GetComponent <AudioSource>().loop)
								{
									sound.Stop ();
								}
							}
						}

						_OnLevelWasLoaded ();
					}
				}
				else
				{
					KickStarter.eventManager.Call_OnLoad (FileAccessState.Fail);
				}
			}
		}


		private Player GetPlayerByID (int id)
		{
			SettingsManager settingsManager = KickStarter.settingsManager;

			foreach (PlayerPrefab playerPrefab in settingsManager.players)
			{
				if (playerPrefab.ID == id)
				{
					if (playerPrefab.playerOb)
					{
						return playerPrefab.playerOb;
					}

					return null;
				}
			}

			return null;
		}


		private int GetPlayerScene (int playerID, List<PlayerData> _playerData)
		{
			if (KickStarter.settingsManager.playerSwitching == PlayerSwitching.DoNotAllow && playerID > 0)
			{
				playerID = 0;
			}

			foreach (PlayerData _data in _playerData)
			{
				if (_data.playerID == playerID)
				{
					return (_data.currentScene);
				}
			}
		
			return UnityVersionHandler.GetCurrentSceneNumber ();
		}


		private string GetPlayerSceneName (int playerID, List<PlayerData> _playerData)
		{
			if (KickStarter.settingsManager.playerSwitching == PlayerSwitching.DoNotAllow && playerID > 0)
			{
				playerID = 0;
			}

			foreach (PlayerData _data in _playerData)
			{
				if (_data.playerID == playerID)
				{
					return (_data.currentSceneName);
				}
			}

			return UnityVersionHandler.GetCurrentSceneName ();
		}


		private void _OnLevelWasLoaded ()
		{
			KickStarter.stateHandler.AfterLoad ();

			if (KickStarter.settingsManager.IsInLoadingScene ())
			{
				return;
			}

			if (KickStarter.sceneSettings == null)
			{
				return;
			}

			ResetSceneObjects ();
			if (loadingGame == LoadingGame.InNewScene || loadingGame == LoadingGame.InSameScene)
			{
				if (KickStarter.dialog)
				{
					KickStarter.dialog.KillDialog (true, true);
				}
				
				if (KickStarter.playerInteraction)
				{
					KickStarter.playerInteraction.StopMovingToHotspot ();
				}

				ReturnMainData ();
				KickStarter.levelStorage.ReturnCurrentLevelData (true);
				CustomLoadHook ();
				KickStarter.eventManager.Call_OnLoad (FileAccessState.After);
			}
					
			if (KickStarter.runtimeInventory)
		    {
				KickStarter.runtimeInventory.RemoveRecipes ();
			}

			if (loadingGame == LoadingGame.JustSwitchingPlayer)
			{
				foreach (PlayerData _data in saveData.playerData)
				{
					if (_data.playerID == KickStarter.player.ID)
					{
						ReturnCameraData (_data);
						KickStarter.playerInput.LoadPlayerData (_data);
						KickStarter.sceneChanger.LoadPlayerData (_data);
						break;
					}
				}

				KickStarter.sceneSettings.UnpauseGame (KickStarter.playerInput.timeScale);//
				KickStarter.stateHandler.gameState = GameState.Cutscene;
				KickStarter.mainCamera.FadeIn (0.5f);

				Invoke ("ReturnToGameplay", gameplayInvokeTime);
			}
			else
			{
				activeSelectiveLoad = new SelectiveLoad ();
			}

			AssetLoader.UnloadAssets ();
		}


		/**
		 * <summary>Create a new save game file.</summary>
		 * <param name = "overwriteLabel">True if the label should be updated</param>
		 * <param name = "newLabel">The new label, if it can be set</param>
		 */
		public static void SaveNewGame (bool overwriteLabel = true, string newLabel = "")
		{
			if (KickStarter.saveSystem)
			{
				KickStarter.saveSystem.SaveNewSaveGame (overwriteLabel, newLabel);
			}
		}
		

		private void SaveNewSaveGame (bool overwriteLabel = true, string newLabel = "")
		{
			if (foundSaveFiles != null && foundSaveFiles.Count > 0)
			{
				int expectedID = -1;

				for (int i=0; i<foundSaveFiles.Count; i++)
				{
					if (expectedID != -1 && expectedID != foundSaveFiles[i].saveID)
					{
						SaveSaveGame (expectedID, overwriteLabel, newLabel);
						return;
					}

					expectedID = foundSaveFiles[i].saveID + 1;
				}

				// Saves present, but no gap
				int newSaveID = (foundSaveFiles [foundSaveFiles.Count-1].saveID+1);
				SaveSaveGame (newSaveID, overwriteLabel, newLabel);
			}
			else
			{
				SaveSaveGame (1, overwriteLabel, newLabel);
			}
		}


		/**
		 * <summary>Overwrites the AutoSave file.</summary>
		 */
		public static void SaveAutoSave ()
		{
			if (KickStarter.saveSystem)
			{
				KickStarter.saveSystem.SaveSaveGame (0);
			}
		}


		/**
		 * <summary>Saves the game.</summary>
		 * <param name = "saveID">The save ID to save</param>
		 * <param name = "overwriteLabel">True if the label should be updated</param>
		 * <param name = "newLabel">The new label, if it can be set. If blank, a default label will be generated.</param>
		 */
		public static void SaveGame (int saveID, bool overwriteLabel = true, string newLabel = "")
		{
			SaveSystem.SaveGame (0, saveID, true, overwriteLabel, newLabel);
		}
		

		/**
		 * <summary>Saves the game.</summary>
		 * <param name = "elementSlot">The slot index of the MenuSavesList element that was clicked on</param>
		 * <param name = "saveID">The save ID to save</param>
		 * <param name = "useSaveID">If True, the saveID overrides the elementSlot to determine which file to save</param>
		 * <param name = "overwriteLabel">True if the label should be updated</param>
		 * <param name = "newLabel">The new label, if it can be set. If blank, a default label will be generated.</param>
		 */
		public static void SaveGame (int elementSlot, int saveID, bool useSaveID, bool overwriteLabel = true, string newLabel = "")
		{
			if (KickStarter.saveSystem)
			{
				if (!useSaveID)
				{
					if (KickStarter.saveSystem.foundSaveFiles.Count > elementSlot)
					{
						saveID = KickStarter.saveSystem.foundSaveFiles[elementSlot].saveID;
					}
					else
					{
						saveID = -1;
					}
				}

				if (saveID == -1)
				{
					SaveSystem.SaveNewGame (overwriteLabel, newLabel);
				}
				else
				{
					KickStarter.saveSystem.SaveSaveGame (saveID, overwriteLabel, newLabel);
				}
			}
		}


		/**
		 * <summary>Saves the game, once found to exist.</summary>
		 * <param name = "saveID">The save ID to save</param>
		 * <param name = "overwriteLabel">True if the label should be updated</param>
		 * <param name = "newLabel">The new label, if it can be set. If blank, a default label will be generated.</param>
		 */
		private void SaveSaveGame (int saveID, bool overwriteLabel = true, string newLabel = "")
		{
			if (GetNumSaves () >= KickStarter.settingsManager.maxSaves && !DoesSaveExist (saveID))
			{
				ACDebug.LogWarning ("Cannot save - maximum number of save files has already been reached.");
				KickStarter.eventManager.Call_OnSave (FileAccessState.Fail);
				return;
			}

			KickStarter.eventManager.Call_OnSave (FileAccessState.Before);
			CustomSaveHook ();
			KickStarter.levelStorage.StoreAllOpenLevelData ();
			
			Player player = KickStarter.player;

			if (KickStarter.playerInput && KickStarter.runtimeInventory && KickStarter.sceneChanger && KickStarter.settingsManager && KickStarter.stateHandler)
			{
				if (saveData != null && saveData.playerData != null && saveData.playerData.Count > 0)
				{
					foreach (PlayerData _data in saveData.playerData)
					{
						if (player != null && _data.playerID == player.ID)
						{
							saveData.playerData.Remove (_data);
							break;
						}
						else if (player == null && _data.playerID == KickStarter.settingsManager.GetEmptyPlayerID ())
						{
							saveData.playerData.Remove (_data);
							break;
						}
					}
				}
				else
				{
					saveData = new SaveData ();
					saveData.mainData = new MainData ();
					saveData.playerData = new List<PlayerData>();
				}

				PlayerData playerData = SavePlayerData (player);
				saveData.playerData.Add (playerData);

				// Main data
				saveData.mainData = KickStarter.stateHandler.SaveMainData (saveData.mainData);
				saveData.mainData.movementMethod = (int) KickStarter.settingsManager.movementMethod;
				saveData.mainData.activeInputsData = ActiveInput.CreateSaveData (KickStarter.settingsManager.activeInputs);

				if (player != null)
				{
					saveData.mainData.currentPlayerID = player.ID;
				}
				else
				{
					saveData.mainData.currentPlayerID = KickStarter.settingsManager.GetEmptyPlayerID ();
				}

				saveData.mainData = KickStarter.playerInput.SaveMainData (saveData.mainData);
				saveData.mainData = KickStarter.runtimeInventory.SaveMainData (saveData.mainData);
				saveData.mainData = KickStarter.runtimeVariables.SaveMainData (saveData.mainData);
				saveData.mainData = KickStarter.playerMenus.SaveMainData (saveData.mainData);
				saveData.mainData.activeAssetLists = KickStarter.actionListAssetManager.GetSaveData ();

				string mainData = Serializer.SerializeObject <SaveData> (saveData, true);

				string levelData = FileFormatHandler.SerializeAllRoomData (KickStarter.levelStorage.allLevelData);

				string allData = mainData + "||" + levelData;

				// Update label
				if (overwriteLabel)
				{
					if (string.IsNullOrEmpty (newLabel))
					{
						newLabel = SaveFileHandler.GetDefaultSaveLabel (saveID);
					}
				}
				else
				{
					newLabel = "";
				}

				int profileID = Options.GetActiveProfileID ();
				SaveFile prepareSave = new SaveFile (saveID, profileID, newLabel, "", false, null, "");

				StopAllCoroutines ();
				StartCoroutine (PrepareSaveCoroutine (prepareSave, allData));
			}
			else
			{
				if (KickStarter.playerInput == null)
				{
					ACDebug.LogWarning ("Save failed - no PlayerInput found.");
				}
				if (KickStarter.runtimeInventory == null)
				{
					ACDebug.LogWarning ("Save failed - no RuntimeInventory found.");
				}
				if (KickStarter.sceneChanger == null)
				{
					ACDebug.LogWarning ("Save failed - no SceneChanger found.");
				}
				if (KickStarter.settingsManager == null)
				{
					ACDebug.LogWarning ("Save failed - no Settings Manager found.");
				}
			}
		}


		private IEnumerator PrepareSaveCoroutine (SaveFile saveFile, string allData)
		{
			#if !SAVE_IN_PLAYERPREFS
			if (KickStarter.settingsManager.takeSaveScreenshots)
			{
				KickStarter.stateHandler.PreScreenshotBackup ();

				yield return new WaitForEndOfFrame ();
				
				Texture2D screenshotTex = new Texture2D (Screen.width, Screen.height, TextureFormat.RGB24, false);
				
				screenshotTex.ReadPixels (new Rect (0f, 0f, Screen.width, Screen.height), 0, 0);
				screenshotTex.Apply ();

				saveFile.screenShot = screenshotTex;

				SaveFileHandler.SaveScreenshot (saveFile);
				saveFile.screenShot = screenshotTex;
				Destroy (screenshotTex);
				
				KickStarter.stateHandler.PostScreenshotBackup ();
			}
			#endif

			requestedSave = new SaveFile (saveFile);

			SaveFileHandler.Save (requestedSave, allData);
			yield return null;
		}


		/**
		 * <summary>Handles the what happens once a save file has been written</summary>
		 * <param name = "saveFile">A data container for information about the save file to that was loaded.  Its saveID and profileID need to match up with that requested in the iSaveFileHandler's Save function in order for the data to be processed</param>
		 * <param name = "wasSuccesful">True if the file saving was succesful</param>
		 */
		public void OnFinishSaveRequest (SaveFile saveFile, bool wasSuccesful)
		{
			if (requestedSave != null && saveFile != null && requestedSave.saveID == saveFile.saveID && requestedSave.profileID == saveFile.profileID)
			{
				// Received data matches requested
				requestedSave = null;

				if (!wasSuccesful)
				{
					KickStarter.eventManager.Call_OnSave (FileAccessState.Fail);
					return;
				}
			
				GatherSaveFiles ();

				// Update label
				if (!string.IsNullOrEmpty (saveFile.label))
				{
					for (int i=0; i<Mathf.Min (50, foundSaveFiles.Count); i++)
					{
						if (foundSaveFiles[i].saveID == saveFile.saveID)
						{
							SaveFile newSaveFile = new SaveFile (foundSaveFiles [i]);
							newSaveFile.SetLabel (saveFile.label);
							foundSaveFiles[i] = newSaveFile;
							break;
						}
					}
				}

				// Update PlayerPrefs
				Options.optionsData.lastSaveID = saveFile.saveID;
				Options.UpdateSaveLabels (foundSaveFiles.ToArray ());

				UpdateSaveFileLabels ();
			
				KickStarter.eventManager.Call_OnSave (FileAccessState.After);
			}
		}


		/**
		 * Stores the PlayerData of the active Player.
		 */
		public void SaveCurrentPlayerData ()
		{
			if (loadingGame == LoadingGame.JustSwitchingPlayer)
			{
				// When switching player, new player is loaded into old scene first before switching - so in this case we don't want to save the player data
				return;
			}

			if (saveData != null && saveData.playerData != null && saveData.playerData.Count > 0)
			{
				foreach (PlayerData _data in saveData.playerData)
				{
					if ((KickStarter.player != null && _data.playerID == KickStarter.player.ID) ||
						(KickStarter.player == null && _data.playerID == KickStarter.settingsManager.GetEmptyPlayerID ()))
					{
						saveData.playerData.Remove (_data);
						break;
					}
				}
			}
			else
			{
				saveData = new SaveData ();
				saveData.mainData = new MainData ();
				saveData.playerData = new List<PlayerData>();
			}

			PlayerData playerData = SavePlayerData (KickStarter.player);
			saveData.playerData.Add (playerData);
		}


		private PlayerData SavePlayerData (Player player)
		{
			PlayerData playerData = new PlayerData ();

			playerData.currentScene = UnityVersionHandler.GetCurrentSceneNumber ();
			playerData.currentSceneName = UnityVersionHandler.GetCurrentSceneName ();

			playerData = KickStarter.sceneChanger.SavePlayerData (playerData);
			playerData = KickStarter.playerInput.SavePlayerData (playerData);

			KickStarter.runtimeInventory.RemoveRecipes ();
			playerData.inventoryData = CreateInventoryData (KickStarter.runtimeInventory.localItems);

			// Camera
			MainCamera mainCamera = KickStarter.mainCamera;
			playerData = mainCamera.SaveData (playerData);
           
			if (player == null)
			{
				playerData.playerPortraitGraphic = "";
				playerData.playerID = KickStarter.settingsManager.GetEmptyPlayerID ();
				return playerData;
			}
			
			playerData = player.SavePlayerData (playerData);

			return playerData;
		}


		/**
		 * <summary>Gets the number of found import files.</summary>
		 * <returns>The number of found import files</returns>
		 */
		public static int GetNumImportSlots ()
		{
			return KickStarter.saveSystem.foundImportFiles.Count;
		}


		/**
		 * <summary>Gets the number of found save files.</summary>
		 * <returns>The number of found save files</returns>
		 */
		public static int GetNumSlots ()
		{
			return KickStarter.saveSystem.foundSaveFiles.Count;
		}


		/**
		 * <summary>Checks that another game's save file data is OK to import, by checking the state of a given boolean variable</summary>
		 * <param name = "fileData">The de-serialized data string from the file</param>
		 * <param name = "boolID">The ID number of the Boolean Global Variable that must be True in the fileData for the import check to pass</param>
		 * <returns>True if the other game's save file data is OK to import</returns>
		 */
		public bool DoImportCheck (string fileData, int boolID)
		{
			if (fileData.ToString () != "")
			{
				int divider = fileData.IndexOf ("||");
				string mainData = fileData.Substring (0, divider);

				SaveData tempSaveData = (SaveData) Serializer.DeserializeObject <SaveData> (mainData);
				if (tempSaveData == null)
				{
					tempSaveData = new SaveData ();
				}

				string varData = tempSaveData.mainData.runtimeVariablesData;
				if (!string.IsNullOrEmpty (varData))
				{
					string[] varsArray = varData.Split (SaveSystem.pipe[0]);
					
					foreach (string chunk in varsArray)
					{
						string[] chunkData = chunk.Split (SaveSystem.colon[0]);
						
						int _id = 0;
						int.TryParse (chunkData[0], out _id);

						if (_id == boolID)
						{
							int _value = 0;
							int.TryParse (chunkData[1], out _value);

							if (_value == 1)
							{
								return true;
							}
							return false;
						}
					}
				}
			}
			return false;
		}


		/**
		 * <summary>Creates a suffix for save filenames based on a given save slot and profile</summary>
		 * <param name = "saveID">The ID of the save slot</param>
		 * <param name = "profileID">The ID of the profile</param>
		 * <return>A save file suffix based on the slot and profile</returns>
		 */
		public string GenerateSaveSuffix (int saveID, int profileID = -1)
		{
			if (KickStarter.settingsManager != null && KickStarter.settingsManager.useProfiles)
			{
				if (profileID == -1)
				{
					// None set, so just use the active profile
					profileID = Options.GetActiveProfileID ();
				}
				return ("_" + saveID.ToString () + "_" + profileID.ToString ());
			}
			return ("_" + saveID.ToString ());
		}


		private void KillActionLists ()
		{
			KickStarter.actionListManager.KillAllLists ();

			Moveable[] moveables = FindObjectsOfType (typeof (Moveable)) as Moveable[];
			foreach (Moveable moveable in moveables)
			{
				moveable.StopMoving ();
			}
		}


		/**
		 * <summary>Gets the label of an import file.</summary>
		 * <param name = "elementSlot">The slot index of the MenuProfilesList element that was clicked on</param>
		 * <param name = "saveID">The save ID to import</param>
		 * <param name = "useSaveID">If True, the saveID overrides the elementSlot to determine which file to import</param>
		 * <returns>The label of the import file.</returns>
		 */
		public static string GetImportSlotLabel (int elementSlot, int saveID, bool useSaveID)
		{
			if (Application.isPlaying && KickStarter.saveSystem.foundImportFiles != null)
			{
				return KickStarter.saveSystem.GetSlotLabel (elementSlot, saveID, useSaveID, KickStarter.saveSystem.foundImportFiles.ToArray ());
			}
			return ("Save test (01/01/2001 12:00:00)"); 
		}


		/**
		 * <summary>Gets the label of a save file.</summary>
		 * <param name = "elementSlot">The slot index of the MenuSavesList element that was clicked on</param>
		 * <param name = "saveID">The save ID to save</param>
		 * <param name = "useSaveID">If True, the saveID overrides the elementSlot to determine which file to save</param>
		 * <returns>The label of the save file.  If the save file is not found, an empty string is returned.</returns>
		 */
		public static string GetSaveSlotLabel (int elementSlot, int saveID, bool useSaveID)
		{
			if (!Application.isPlaying)
			{
				if (useSaveID)
				{
					elementSlot = saveID;
				}
				return SaveFileHandler.GetDefaultSaveLabel (elementSlot);
			}
			else if (KickStarter.saveSystem.foundSaveFiles != null)
			{
				return KickStarter.saveSystem.GetSlotLabel (elementSlot, saveID, useSaveID, KickStarter.saveSystem.foundSaveFiles.ToArray ());
			}

			return ("Save game file"); 
		}


		/**
		 * <summary>Gets the label of a save file.</summary>
		 * <param name = "elementSlot">The slot index of the MenuSavesList element that was clicked on</param>
		 * <param name = "saveID">The save ID to save</param>
		 * <param name = "useSaveID">If True, the saveID overrides the elementSlot to determine which file to save</param>
		 * <param name = "saveFiles">An array of SaveFile instances that the save file to retrieve is assumed to be in</param>
		 * <returns>The label of the save file.  If the save file is not found, an empty string is returned.</returns>
		 */
		public string GetSlotLabel (int elementSlot, int saveID, bool useSaveID, SaveFile[] saveFiles)
		{
			if (Application.isPlaying)
			{
				if (useSaveID)
				{
					foreach (SaveFile saveFile in saveFiles)
					{
						if (saveFile.saveID == saveID)
						{
							return saveFile.label;
						}
					}
				}
				else if (elementSlot >= 0)
				{
					if (elementSlot < saveFiles.Length)
					{
						return saveFiles [elementSlot].label;
					}
				}
				return "";
			}
			return ("Save test (01/01/2001 12:00:00)");
		}


		/**
		 * <summary>Gets the screenshot of an import file.</summary>
		 * <param name = "elementSlot">The slot index of the MenuSavesList element that was clicked on</param>
		 * <param name = "saveID">The save ID to get the screenshot of</param>
		 * <param name = "useSaveID">If True, the saveID overrides the elementSlot to determine which file to look for</param>
		 * <returns>The import files's screenshots as a Texture2D.  If the save file is not found, null is returned.</returns>
		 */
		public static Texture2D GetImportSlotScreenshot (int elementSlot, int saveID, bool useSaveID)
		{
			if (Application.isPlaying && KickStarter.saveSystem.foundImportFiles != null)
			{
				return KickStarter.saveSystem.GetScreenshot (elementSlot, saveID, useSaveID, KickStarter.saveSystem.foundImportFiles.ToArray ());
			}
			return null;
		}
		

		/**
		 * <summary>Gets the screenshot of a save file.</summary>
		 * <param name = "elementSlot">The slot index of the MenuSavesList element that was clicked on</param>
		 * <param name = "saveID">The save ID to get the screenshot of</param>
		 * <param name = "useSaveID">If True, the saveID overrides the elementSlot to determine which file to look for</param>
		 * <return>The save files's screenshots as a Texture2D.  If the save file is not found, null is returned.</returns>
		 */
		public static Texture2D GetSaveSlotScreenshot (int elementSlot, int saveID, bool useSaveID)
		{
			if (Application.isPlaying && KickStarter.saveSystem.foundSaveFiles != null)
			{
				return KickStarter.saveSystem.GetScreenshot (elementSlot, saveID, useSaveID, KickStarter.saveSystem.foundSaveFiles.ToArray ());
			}
			return null;
		}


		/**
		 * <summary>Gets the screenshot of a save file.</summary>
		 * <param name = "elementSlot">The slot index of the MenuSavesList element that was clicked on</param>
		 * <param name = "saveID">The save ID to get the screenshot of</param>
		 * <param name = "useSaveID">If True, the saveID overrides the elementSlot to determine which file to look for</param>
		 * <param name = "saveFiles">An array of SaveFile instances that the save file to retrieve is assumed to be in</param>
		 * <returns>The save files's screenshots as a Texture2D.  If the save file is not found, null is returned.</returns>
		 */
		public Texture2D GetScreenshot (int elementSlot, int saveID, bool useSaveID, SaveFile[] saveFiles)
		{
			if (Application.isPlaying)
			{
				if (useSaveID)
				{
					foreach (SaveFile saveFile in saveFiles)
					{
						if (saveFile.saveID == saveID)
						{
							return saveFile.screenShot;
						}
					}
				}
				else if (elementSlot >= 0)
				{
					if (elementSlot < saveFiles.Length)
					{
						return saveFiles [elementSlot].screenShot;
					}
				}
			}
			return null;
		}


		private void ReturnMainData ()
		{
			if (KickStarter.playerInput && KickStarter.runtimeInventory && KickStarter.settingsManager && KickStarter.stateHandler)
			{
				PlayerData playerData = new PlayerData ();

				int playerID = saveData.mainData.currentPlayerID;
				if (KickStarter.settingsManager.playerSwitching == PlayerSwitching.DoNotAllow && playerID > 0)
				{
					playerID = 0;
				}

				foreach (PlayerData _data in saveData.playerData)
				{
					if (_data.playerID == saveData.mainData.currentPlayerID)
					{
						playerData = _data;
					}
				}
			
				if (activeSelectiveLoad.loadPlayer)
				{
					ReturnPlayerData (playerData, KickStarter.player);
				}
				if (activeSelectiveLoad.loadSceneObjects)
				{
					ReturnCameraData (playerData);
				}

				KickStarter.stateHandler.LoadMainData (saveData.mainData);
				KickStarter.actionListAssetManager.LoadData (saveData.mainData.activeAssetLists);
				KickStarter.settingsManager.movementMethod = (MovementMethod) saveData.mainData.movementMethod;
				ActiveInput.LoadSaveData (saveData.mainData.activeInputsData);

				if (activeSelectiveLoad.loadScene)
				{
					KickStarter.sceneChanger.LoadPlayerData (playerData);
				}

				if (activeSelectiveLoad.loadPlayer)
				{
					KickStarter.playerInput.LoadPlayerData (playerData);
				}

				// Inventory
				KickStarter.runtimeInventory.RemoveRecipes ();
				if (activeSelectiveLoad.loadInventory)
				{
					KickStarter.runtimeInventory.AssignPlayerInventory (AssignInventory (KickStarter.runtimeInventory, playerData.inventoryData));
					if (saveData.mainData.selectedInventoryID > -1)
					{
						if (saveData.mainData.isGivingItem)
						{
							KickStarter.runtimeInventory.SelectItemByID (saveData.mainData.selectedInventoryID, SelectItemMode.Give);
						}
						else
						{
							KickStarter.runtimeInventory.SelectItemByID (saveData.mainData.selectedInventoryID, SelectItemMode.Use);
						}
					}
					else
					{
						KickStarter.runtimeInventory.SetNull ();
					}
					KickStarter.runtimeInventory.RemoveRecipes ();
				}

				KickStarter.playerInput.LoadMainData (saveData.mainData);

				// Variables
				if (activeSelectiveLoad.loadVariables)
				{
					SaveSystem.AssignVariables (saveData.mainData.runtimeVariablesData);
					KickStarter.runtimeVariables.AssignCustomTokensFromString (saveData.mainData.customTokenData);
				}

				// Menus
				KickStarter.playerMenus.LoadMainData (saveData.mainData);

				KickStarter.mainCamera.HideScene ();
				KickStarter.sceneSettings.UnpauseGame (KickStarter.playerInput.timeScale);//
				KickStarter.stateHandler.gameState = GameState.Cutscene;
				KickStarter.mainCamera.FadeIn (0.5f);

				Invoke ("ReturnToGameplay", gameplayInvokeTime);
			}
			else
			{
				if (KickStarter.playerInput == null)
				{
					ACDebug.LogWarning ("Load failed - no PlayerInput found.");
				}
				if (KickStarter.runtimeInventory == null)
				{
					ACDebug.LogWarning ("Load failed - no RuntimeInventory found.");
				}
				if (KickStarter.sceneChanger == null)
				{
					ACDebug.LogWarning ("Load failed - no SceneChanger found.");
				}
				if (KickStarter.settingsManager == null)
				{
					ACDebug.LogWarning ("Load failed - no Settings Manager found.");
				}
			}
		}


		/**
		 * <summary>Checks if PlayerData for a given Player has been generated.</summary>
		 * <param name = "ID">The ID number of the Player to check</param>
		 * <param name = "doSceneCheck">If True, the check will only be successful if the Player is currently within a scene</param>
		 * <returns>True if PlayerData for the given Player exists</returns>
		 */
		public bool DoesPlayerDataExist (int ID, bool doSceneCheck = false)
		{
			if (saveData != null && saveData.playerData.Count > 0)
			{
				foreach (PlayerData _data in saveData.playerData)
				{
					if (_data.playerID == ID)
					{
						if (doSceneCheck && _data.currentScene == -1)
						{
							return false;
						}
						return true;
					}
				}
			}

			return false;
		}


		/**
		 * <summary>Gets the current scene number that a Player is in.</summary>
		 * <param name = "ID">The ID number of the Player to check</param>
		 * <returns>The current scene number that the Player is in.  If the Player is not found, the currently-open scene number will be returned.</returns>
		 */
		public int GetPlayerScene (int ID)
		{
			if (KickStarter.player)
			{
				if (saveData.playerData.Count > 0)
				{
					foreach (PlayerData _data in saveData.playerData)
					{
						if (_data.playerID == ID)
						{
							return (_data.currentScene);
						}
					}
				}
			}
			return UnityVersionHandler.GetCurrentSceneNumber ();
		}


		/**
		 * <summary>Gets the current scene nsmr that a Player is in.</summary>
		 * <param name = "ID">The ID number of the Player to check</param>
		 * <returns>The current scene name that the Player is in.  If the Player is not found, the currently-open scene name will be returned.</returns>
		 */
		public string GetPlayerSceneName (int ID)
		{
			if (KickStarter.player)
			{
				if (saveData.playerData.Count > 0)
				{
					foreach (PlayerData _data in saveData.playerData)
					{
						if (_data.playerID == ID)
						{
							return (_data.currentSceneName);
						}
					}
				}
			}
			
			return UnityVersionHandler.GetCurrentSceneName ();
		}


		/**
		 * <summary>Updates a Player object with its associated saved animation data, if it exists.</summary>
		 * <param name = "player">The Player to load animation data for</param>
		 */
		public void AssignPlayerAnimData (Player player)
		{
			if (player != null && saveData.playerData.Count > 0)
			{
				foreach (PlayerData _data in saveData.playerData)
				{
					if (player.ID == _data.playerID)
					{
						player.LoadPlayerData (_data, true);
					}
				}
			}
		}


		/**
		 * <summary>Updates a Player object with its associated saved data, if it exists.</summary>
		 * <param name = "player">The Player to load animation data for</param>
		 */
		public void AssignPlayerAllData (Player player)
		{
			if (player != null && saveData.playerData.Count > 0)
			{
				foreach (PlayerData _data in saveData.playerData)
				{
					if (player.ID == _data.playerID)
					{
						player.LoadPlayerData (_data);
					}
				}
			}
		}


		/**
		 * <summary>Unloads stored PlayerData back onto the Player object.</summary>
		 * <param name = "ID">The ID of the Player to affect</param>
		 * <param name = "doInventory">If True, updates the Player's inventory</param>
		 * <returns>The Player's new scene number</returns>
		 */
		public int AssignPlayerData (int ID, bool doInventory)
		{
			if (KickStarter.player)
			{
				if (saveData.playerData.Count > 0)
				{
					foreach (PlayerData _data in saveData.playerData)
					{
						if (_data.playerID == ID)
						{
							if (_data.currentScene != -1)
							{
								// If -1, data only exists because we updated inventory, so only restore Inventory in this case

								ReturnPlayerData (_data, KickStarter.player);
								ReturnCameraData (_data);
								KickStarter.playerInput.LoadPlayerData (_data);
								KickStarter.sceneChanger.LoadPlayerData (_data);
							}

							KickStarter.runtimeInventory.SetNull ();
							KickStarter.runtimeInventory.RemoveRecipes ();

							if (doInventory)
							{
								KickStarter.runtimeInventory.AssignPlayerInventory (AssignInventory (KickStarter.runtimeInventory, _data.inventoryData));
							}

							return (_data.currentScene);
						}
					}
				}
			}
			AssetLoader.UnloadAssets ();
			return UnityVersionHandler.GetCurrentSceneNumber ();
		}


		private void ReturnPlayerData (PlayerData playerData, Player player)
		{
			if (player == null)
			{
				return;
			}

			player.LoadPlayerData (playerData);
		}


		private void ReturnCameraData (PlayerData playerData)
		{
			// Camera
			MainCamera mainCamera = KickStarter.mainCamera;
			mainCamera.LoadData (playerData);
		}


		private void ReturnToGameplay ()
		{
			loadingGame = LoadingGame.No;
			KickStarter.playerInput.ReturnToGameplayAfterLoad ();
			if (KickStarter.sceneSettings)
			{
				KickStarter.sceneSettings.OnLoad ();
			}
		}
		

		/**
		 * <summary>Unloads stored global variable data back into the RuntimeVariables script.</summary>
		 * <param name = "runtimeVariablesData">The values of all global variables, combined into a stingle string</param>
		 * <param name = "fromOptions">If true, only global variables that are linked to OptionsData will be affected</param>
		 */
		public static void AssignVariables (string runtimeVariablesData, bool fromOptions = false)
		{
			if (runtimeVariablesData == null)
			{
				return;
			}
			KickStarter.runtimeVariables.ClearSpeechLog ();

			if (!string.IsNullOrEmpty (runtimeVariablesData))
			{
				string[] varsArray = runtimeVariablesData.Split (SaveSystem.pipe[0]);
				foreach (string chunk in varsArray)
				{
					string[] chunkData = chunk.Split (SaveSystem.colon[0]);
					
					int _id = 0;
					int.TryParse (chunkData[0], out _id);
					GVar var = GlobalVariables.GetVariable (_id);
				
					if (var == null)
					{
						continue;
					}
					if (fromOptions && var.link != VarLink.OptionsData)
					{
						continue;
					}

					if (var.type == VariableType.String)
					{
						string _text = chunkData[1];
						_text = AdvGame.PrepareStringForLoading (_text);

						int _textLineID = -1;
						if (chunkData.Length > 2)
						{
							int.TryParse (chunkData[2], out _textLineID);
						}

						var.SetStringValue (_text, _textLineID);
					}
					else if (var.type == VariableType.Float)
					{
						float _value = 0f;
						float.TryParse (chunkData[1], out _value);
						var.SetFloatValue (_value, SetVarMethod.SetValue);
					}
					else if (var.type == VariableType.Vector3)
					{
						string _text = chunkData[1];
						_text = AdvGame.PrepareStringForLoading (_text);

						Vector3 _value = Vector3.zero;
						string[] valuesArray = _text.Split (","[0]);
						if (valuesArray != null && valuesArray.Length == 3)
						{
							float xValue = 0f;
							float.TryParse (valuesArray[0], out xValue);

							float yValue = 0f;
							float.TryParse (valuesArray[1], out yValue);

							float zValue = 0f;
							float.TryParse (valuesArray[2], out zValue);

							_value = new Vector3 (xValue, yValue, zValue);
						}

						var.SetVector3Value (_value);
					}
					else
					{
						int _value = 0;
						int.TryParse (chunkData[1], out _value);
						var.SetValue (_value, SetVarMethod.SetValue);
					}
				}
			}
			
			GlobalVariables.UploadAll ();
		}

		
		private List<InvItem> AssignInventory (RuntimeInventory _runtimeInventory, string inventoryData)
		{
			List<InvItem> invItems = new List<InvItem>();

			if (!string.IsNullOrEmpty (inventoryData))
			{
				string[] countArray = inventoryData.Split (SaveSystem.pipe[0]);
				
				foreach (string chunk in countArray)
				{
					string[] chunkData = chunk.Split (SaveSystem.colon[0]);
					
					int _id = 0;
					int.TryParse (chunkData[0], out _id);
		
					int _count = 0;
					int.TryParse (chunkData[1], out _count);

					invItems = _runtimeInventory.Add (_id, _count, invItems, false);
				}
			}

			return invItems;
		}


		private string CreateInventoryData (List<InvItem> invItems)
		{
			System.Text.StringBuilder inventoryString = new System.Text.StringBuilder ();
			
			foreach (InvItem item in invItems)
			{
				if (item != null)
				{
					inventoryString.Append (item.id.ToString ());
					inventoryString.Append (SaveSystem.colon);
					inventoryString.Append (item.count.ToString ());
					inventoryString.Append (SaveSystem.pipe);
				}
			}
			
			if (invItems != null && invItems.Count > 0)
			{
				inventoryString.Remove (inventoryString.Length-1, 1);
			}
			return inventoryString.ToString ();		
		}
		

		/**
		 * <summary>Condenses the values of a List of variables into a single string.</summary>
		 * <param name = "vars">A List of variables (see GVar) to condense</param>
		 * <param name = "isOptionsData">If True, only global variables that are linked to OptionsData will be included</param>
		 * <param name = "location">The variables' location (Local, Variable)</param>
		 * <returns>The variable's values, condensed into a single string</returns>
		 */
		public static string CreateVariablesData (List<GVar> vars, bool isOptionsData, VariableLocation location)
		{
			System.Text.StringBuilder variablesString = new System.Text.StringBuilder ();

			foreach (GVar _var in vars)
			{
				if ((isOptionsData && _var.link == VarLink.OptionsData) || (!isOptionsData && _var.link != VarLink.OptionsData) || location == VariableLocation.Local)
				{
					variablesString.Append (_var.id.ToString ());
					variablesString.Append (SaveSystem.colon);
					if (_var.type == VariableType.String)
					{
						string textVal = _var.textVal;
						textVal = AdvGame.PrepareStringForSaving (textVal);
						variablesString.Append (textVal);

						// The ID can be changed with SetStringValue, so needs recording
						variablesString.Append (SaveSystem.colon);
						variablesString.Append (_var.textValLineID);
					}
					else if (_var.type == VariableType.Float)
					{
						variablesString.Append (_var.floatVal.ToString ());
					}
					else if (_var.type == VariableType.Vector3)
					{
						string vector3Val = _var.vector3Val.x.ToString () + "," + _var.vector3Val.y.ToString () + "," + _var.vector3Val.z.ToString ();
						vector3Val = AdvGame.PrepareStringForSaving (vector3Val);
						variablesString.Append (vector3Val);
					}
					else
					{
						variablesString.Append (_var.val.ToString ());
					}
					variablesString.Append (SaveSystem.pipe);
				}
			}
			
			if (variablesString.Length > 0)
			{
				variablesString.Remove (variablesString.Length-1, 1);
			}

			return variablesString.ToString ();		
		}


		/**
		 * <summary>Returns a list of inventory items currently carried by a particular Player.</summary>
		 * <param name = "_playerID">The ID number of the Player to check the inventory of</param>
		 * <returns>A List of InvItem instances representing the inventory items</returns>
		 */
		public List<InvItem> GetItemsFromPlayer (int _playerID)
		{
			if (KickStarter.player.ID == _playerID)
			{
				return KickStarter.runtimeInventory.localItems;
			}

			if (saveData != null && saveData.playerData != null)
			{
				foreach (PlayerData _data in saveData.playerData)
				{
					if (_data.playerID == _playerID)
					{
						return AssignInventory (KickStarter.runtimeInventory, _data.inventoryData);
					}
				}
			}
			return new List<InvItem>();
		}


		/**
		 * <summary>Re-assigns the inventory items currently carried by a particular Player.</summary>
		 * <param name = "invItems">A List of InvItem instances representing the inventory items</param>
		 * <param name = "_playerID">The ID number of the Player to assign the inventory of</param>
		 */
		public void AssignItemsToPlayer (List<InvItem> invItems, int _playerID)
		{
			string invData = CreateInventoryData (invItems);

			if (saveData != null && saveData.playerData != null)
			{
				foreach (PlayerData data in saveData.playerData)
				{
					if (data.playerID == _playerID)
					{
						PlayerData newPlayerData = new PlayerData ();
						newPlayerData.CopyData (data);
						newPlayerData.inventoryData = invData;

						saveData.playerData.Remove (data);
						saveData.playerData.Add (newPlayerData);

						return;
					}
				}
			}

			PlayerData playerData = new PlayerData ();
			playerData.playerID = _playerID;
			playerData.inventoryData = invData;
			playerData.currentScene = -1;

			if (saveData == null)
			{
				ClearAllData ();
			}

			saveData.playerData.Add (playerData);
		}


		private void CustomSaveHook ()
		{
			ISave[] saveHooks = GetSaveHooks (GetComponents (typeof (ISave)));
			if (saveHooks != null && saveHooks.Length > 0)
			{
				foreach (ISave saveHook in saveHooks)
				{
					saveHook.PreSave ();
				}
			}
		}


		private void CustomLoadHook ()
		{
			ISave[] saveHooks = GetSaveHooks (GetComponents (typeof (ISave)));
			if (saveHooks != null && saveHooks.Length > 0)
			{
				foreach (ISave saveHook in saveHooks)
				{
					saveHook.PostLoad ();
				}
			}
		}


		private ISave[] GetSaveHooks (IList list)
		{
			ISave[] ret = new ISave[list.Count];
			list.CopyTo (ret, 0);
			return ret;
		}


		/**
		 * <summary>Renames the label of a save game file.</summary>
		 * <param name = "newLabel">The new label to give the save game file</param>
		 * <param name = "saveIndex">The index of the foundSaveFiles List that represents the save file to affect</param>
		 */
		public void RenameSave (string newLabel, int saveIndex)
		{
			if (string.IsNullOrEmpty (newLabel))
			{
				return;
			}

			GatherSaveFiles ();

			if (foundSaveFiles.Count > saveIndex && saveIndex >= 0)
			{
				SaveFile newSaveFile = new SaveFile (foundSaveFiles [saveIndex]);
				newSaveFile.SetLabel (newLabel);
				foundSaveFiles [saveIndex] = newSaveFile;
				Options.UpdateSaveLabels (foundSaveFiles.ToArray ());
			}
		}


		/**
		 * <summary>Deletes a player profile by referencing its entry in a MenuProfilesList element.</summary>
		 * <param name = "profileIndex">The index in the MenuProfilesList element that represents the profile to delete. If it is set to its default, -2, the active profile will be deleted</param>
		 * <param name = "includeActive">If True, then the MenuProfilesList element that the profile was selected from also displays the active profile</param>
		 */
		public void DeleteProfile (int profileIndex = -2, bool includeActive = true)
		{
			if (!KickStarter.settingsManager.useProfiles)
			{
				return;
			}
			
			int profileID = KickStarter.options.ProfileIndexToID (profileIndex, includeActive);
			if (profileID == -1)
			{
				ACDebug.LogWarning ("Invalid profile index: " + profileIndex + " - nothing to delete!");
				return;
			}
			else if (profileIndex == -2)
			{
				profileID = Options.GetActiveProfileID ();
			}

			DeleteProfileID (profileID);
		}


		/**
		 * <summary>Deletes a player profile ID.</summary>
		 * <param name = "profileID">The profile ID to delete</param>
		 */
		public void DeleteProfileID (int profileID)
		{
			if (!KickStarter.settingsManager.useProfiles || profileID < 0)
			{
				return;
			}

			if (!Options.DoesProfileIDExist (profileID))
			{
				ACDebug.LogWarning ("Cannot delete profile ID " + profileID + " as it does not exist!");
				return;
			}

			// Delete save files
			SaveFileHandler.DeleteAll (profileID);

			bool isActive = (profileID == Options.GetActiveProfileID ()) ? true : false;
			Options.DeleteProfilePrefs (profileID);
			if (isActive)
			{
				GatherSaveFiles ();
			}
			KickStarter.playerMenus.RecalculateAll ();

			ACDebug.Log ("Profile ID " + profileID + " deleted.");
		}


		/**
		 * <summary>Deletes a save game file.</summary>
		 * <param name = "saveID">The save ID of the file to load</param>
		 */
		public static void DeleteSave (int saveID)
		{
			KickStarter.saveSystem.DeleteSave (0, saveID, true);
		}


		/**
		 * <summary>Deletes a save game file.</summary>
		 * <param name = "elementSlot">The slot index of the MenuProfilesList element that was clicked on</param>
		 * <param name = "saveID">The save ID to delete</param>
		 * <param name = "useSaveID">If True, the saveID overrides the elementSlot to determine which file to delete</param>
		 */
		public void DeleteSave (int elementSlot, int saveID, bool useSaveID)
		{
			if (!useSaveID)
			{
				// For this to work, must have loaded the list of saves into a SavesList
				saveID = KickStarter.saveSystem.foundSaveFiles[elementSlot].saveID;
			}

			foreach (SaveFile saveFile in foundSaveFiles)
			{
				if (saveFile.saveID == saveID)
				{
					SaveFileHandler.Delete (saveFile);
				}
			}

			// Also remove save label
			GatherSaveFiles ();
			foreach (SaveFile saveFile in foundSaveFiles)
			{
				if (saveFile.saveID == saveID)
				{
					foundSaveFiles.Remove (saveFile);
					Options.UpdateSaveLabels (foundSaveFiles.ToArray ());
					break;
				}
			}

			if (Options.optionsData != null && Options.optionsData.lastSaveID == saveID)
			{
				Options.optionsData.lastSaveID = -1;
				Options.SavePrefs ();
			}
			KickStarter.playerMenus.RecalculateAll ();
		}


		/**
		 * <summary>Gets the number of save game files found.</summary>
		 * <param name = "includeAutoSaves">If True, then autosave files will be included in the result</param>
		 * <returns>The number of save games found</returns>
		 */
		public int GetNumSaves (bool includeAutoSaves = true)
		{
			int numFound = 0;
			foreach (SaveFile saveFile in foundSaveFiles)
			{
				if (!saveFile.isAutoSave || includeAutoSaves)
				{
					numFound ++;
				}
			}
			return numFound;
		}


		private void ResetSceneObjects ()
		{
			AC.Char[] characters = FindObjectsOfType (typeof (AC.Char)) as AC.Char[];
			foreach (AC.Char character in characters)
			{
				character.AfterLoad ();
			}

			AC.Sound[] sounds = FindObjectsOfType (typeof (AC.Sound)) as AC.Sound[];
			foreach (AC.Sound sound in sounds)
			{
				if (sound != null)
				{
					sound.AfterLoad ();
				}
			}

			FirstPersonCamera[] firstPersonCameras = FindObjectsOfType (typeof (FirstPersonCamera)) as FirstPersonCamera[];
			foreach (FirstPersonCamera firstPersonCamera in firstPersonCameras)
			{
				firstPersonCamera.AfterLoad ();
			}

			FollowTintMap[] followTintMaps = FindObjectsOfType (typeof (FollowTintMap)) as FollowTintMap[];
			foreach (FollowTintMap followTintMap in followTintMaps)
			{
				followTintMap.AfterLoad ();
			}

			FollowSortingMap[] followSortingMaps = FindObjectsOfType (typeof (FollowSortingMap)) as FollowSortingMap[];
			foreach (FollowSortingMap followSortingMap in followSortingMaps)
			{
				followSortingMap.AfterLoad ();
			}

			DetectHotspots[] detectHotspots = FindObjectsOfType (typeof (DetectHotspots)) as DetectHotspots[];
			foreach (DetectHotspots detectHotspot in detectHotspots)
			{
				detectHotspot.AfterLoad ();
			}

			KickStarter.playerMenus.AfterLoad ();
			KickStarter.runtimeInventory.AfterLoad ();
			KickStarter.sceneChanger.AfterLoad ();
			KickStarter.options.AfterLoad ();
			
			KickStarter.kickStarter.AfterLoad ();
		}


		/**
		 * The iSaveFileHandler class that handles the creation, loading, and deletion of save files
		 */
		public static iSaveFileHandler SaveFileHandler
		{
			get
			{
				if (saveFileHandlerOverride != null)
				{
					return saveFileHandlerOverride;
				}

				#if SAVE_IN_PLAYERPREFS
				return new SaveFileHandler_PlayerPrefs ();
				#else
				return new SaveFileHandler_SystemFile ();
				#endif
			}
			set
			{
				saveFileHandlerOverride = value;
			}
		}


		/**
		 * The iFileFormatHandler class that handles the serialization and deserialzation of data
		 */
		public static iFileFormatHandler FileFormatHandler
		{
			get
			{
				if (fileFormatHandlerOverride != null)
				{
					return fileFormatHandlerOverride;
				}

				if (UnityVersionHandler.CanUseJson () && KickStarter.settingsManager != null && KickStarter.settingsManager.useJsonSerialization)
				{
					return new FileFormatHandler_Json ();
				}

				#if SAVE_USING_XML
				return new FileFormatHandler_Xml ();
				#else
				return new FileFormatHandler_Binary ();
				#endif
			}
			set
			{
				fileFormatHandlerOverride = value;
			}
		}

	}

}