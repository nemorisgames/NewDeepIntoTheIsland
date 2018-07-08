/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"LevelStorage.cs"
 * 
 *	This script handles the loading and unloading of per-scene data.
 *	Below the main class is a series of data classes for the different object types.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

namespace AC
{

	/**
	 * Manages the loading and storage of per-scene data (the various Remember scripts).
	 * This needs to be attached to the PersistentEngine prefab
	 */
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_level_storage.html")]
	#endif
	public class LevelStorage : MonoBehaviour
	{

		/** A collection of level data for each visited scene */
		[HideInInspector] public List<SingleLevelData> allLevelData = new List<SingleLevelData>();
		
		
		public void OnAwake ()
		{
			ClearAllLevelData ();
		}


		/**
		 * Wipes all stored scene save data from memory.
		 */
		public void ClearAllLevelData ()
		{
			allLevelData.Clear ();
			allLevelData = new List<SingleLevelData>();
		}



		/**
		 * Wipes the currently-loaded scene's save data from memory
		 */
		public void ClearCurrentLevelData ()
		{
			foreach (SingleLevelData levelData in allLevelData)
			{
				if (levelData.sceneNumber == UnityVersionHandler.GetCurrentSceneNumber ())
				{
					allLevelData.Remove (levelData);
					return;
				}
			}
		}
		

		/**
		 * <summary>Returns the currently-loaded scene's save data to the appropriate Remember components.</summary>
		 * <param name = "restoringSaveFile">True if the game is currently loading a saved game file, as opposed to just switching scene</param>
		 */
		public void ReturnCurrentLevelData (bool restoringSaveFile)
		{
			// Main scene
			foreach (SingleLevelData levelData in allLevelData)
			{
				if (levelData.sceneNumber == UnityVersionHandler.GetCurrentSceneNumber ())
				{
					SendDataToScene (levelData, restoringSaveFile);
					break;
				}
			}

			AssetLoader.UnloadAssets ();
		}


		/**
		 * <summary>Returns a sub-scene's save data to the appropriate Remember components.</summary>
		 * <param name = "subScene">The SubScene component associated with the sub-scene</param>
		 * <param name = "restoringSaveFile">True if the game is currently loading a saved game file, as opposed to just switching scene</param>
		 */
		public void ReturnSubSceneData (SubScene subScene, bool restoringSaveFile)
		{
			// Sub-scenes
			foreach (SingleLevelData levelData in allLevelData)
			{
				if (subScene.SceneInfo.number == levelData.sceneNumber)
				{
					SendDataToScene (levelData, restoringSaveFile, subScene);
				}
			}

			AssetLoader.UnloadAssets ();
		}


		private void SendDataToScene (SingleLevelData levelData, bool restoringSaveFile, SubScene subScene = null)
		{
			SceneSettings sceneSettings = (subScene == null) ? KickStarter.sceneSettings : subScene.SceneSettings;
			LocalVariables localVariables = (subScene == null) ? KickStarter.localVariables : subScene.LocalVariables;
			KickStarter.actionListManager.LoadData (levelData.activeLists, subScene);

			UnloadCutsceneOnLoad (levelData.onLoadCutscene, sceneSettings);
			UnloadCutsceneOnStart (levelData.onStartCutscene, sceneSettings);
			UnloadNavMesh (levelData.navMesh, sceneSettings);
			UnloadPlayerStart (levelData.playerStart, sceneSettings);
			UnloadSortingMap (levelData.sortingMap, sceneSettings);
			UnloadTintMap (levelData.tintMap, sceneSettings);

			UnloadTransformData (levelData.allTransformData, subScene);

			foreach (ScriptData _scriptData in levelData.allScriptData)
			{
				if (_scriptData.data != null && _scriptData.data.Length > 0)
				{
					// Get objects in active scene, and "DontDestroyOnLoad" scene
					Remember[] saveObjects = Serializer.returnComponents <Remember> (_scriptData.objectID);

					foreach (Remember saveObject in saveObjects)
					{
						if (saveObject != null && UnityVersionHandler.ObjectIsInActiveScene (saveObject.gameObject))
						{
							// May have more than one Remember script on the same object, so check all
							Remember[] saveScripts = saveObject.gameObject.GetComponents <Remember>();
							foreach (Remember saveScript in saveScripts)
							{
								saveScript.LoadData (_scriptData.data, restoringSaveFile);
							}
						}
					}
				}
			}

			UnloadVariablesData (levelData.localVariablesData, localVariables);
		}
		

		/**
		 * Combs the active scene for data to store, combines it into a SingleLevelData variable, and adds it to the SingleLevelData List, allLevelData.
		 */
		public void StoreCurrentLevelData ()
		{
			// Active scene
			SendSceneToData ();
		}


		/**
		 * Combs all open scenes for data to store, combines each into a SingleLevelData variable, and adds them to the SingleLevelData List, allLevelData.
		 */
		public void StoreAllOpenLevelData ()
		{
			// Active scene
			SendSceneToData ();
		
			// Sub-scenes
			foreach (SubScene subScene in KickStarter.sceneChanger.GetSubScenes ())
			{
				SendSceneToData (subScene);
			}
		}


		/**
		 * <summary>Combs a sub-scene for data to store, combines it into a SingleLevelData variable, and adds it to the SingleLevelData List, allLevelData.</summary>
		 * <param name = "subScene">The SubScene component associated with the sub-scene</param>
		 */
		public void StoreSubSceneData (SubScene subScene)
		{
			SendSceneToData (subScene);
		}


		private void SendSceneToData (SubScene subScene = null)
		{
			SceneSettings sceneSettings = (subScene == null) ? KickStarter.sceneSettings : subScene.SceneSettings;
			LocalVariables localVariables = (subScene == null) ? KickStarter.localVariables : subScene.LocalVariables;

			List<TransformData> thisLevelTransforms = PopulateTransformData (subScene);
			List<ScriptData> thisLevelScripts = PopulateScriptData (subScene);

			SingleLevelData thisLevelData = new SingleLevelData ();
			thisLevelData.sceneNumber = (subScene == null) ? UnityVersionHandler.GetCurrentSceneNumber () : subScene.SceneInfo.number;

			thisLevelData.activeLists = KickStarter.actionListManager.GetSaveData (subScene);
			
			if (sceneSettings != null)
			{
				if (sceneSettings.navMesh && sceneSettings.navMesh.GetComponent <ConstantID>())
				{
					thisLevelData.navMesh = Serializer.GetConstantID (sceneSettings.navMesh.gameObject);
				}
				if (sceneSettings.defaultPlayerStart && sceneSettings.defaultPlayerStart.GetComponent <ConstantID>())
				{
					thisLevelData.playerStart = Serializer.GetConstantID (sceneSettings.defaultPlayerStart.gameObject);
				}
				if (sceneSettings.sortingMap && sceneSettings.sortingMap.GetComponent <ConstantID>())
				{
					thisLevelData.sortingMap = Serializer.GetConstantID (sceneSettings.sortingMap.gameObject);
				}
				if (sceneSettings.cutsceneOnLoad && sceneSettings.cutsceneOnLoad.GetComponent <ConstantID>())
				{
					thisLevelData.onLoadCutscene = Serializer.GetConstantID (sceneSettings.cutsceneOnLoad.gameObject);
				}
				if (sceneSettings.cutsceneOnStart && sceneSettings.cutsceneOnStart.GetComponent <ConstantID>())
				{
					thisLevelData.onStartCutscene = Serializer.GetConstantID (sceneSettings.cutsceneOnStart.gameObject);
				}
				if (sceneSettings.tintMap && sceneSettings.tintMap.GetComponent <ConstantID>())
				{
					thisLevelData.tintMap = Serializer.GetConstantID (sceneSettings.tintMap.gameObject);
				}
			}

			thisLevelData.localVariablesData = SaveSystem.CreateVariablesData (localVariables.localVars, false, VariableLocation.Local);
			thisLevelData.allTransformData = thisLevelTransforms;
			thisLevelData.allScriptData = thisLevelScripts;

			bool found = false;
			for (int i=0; i<allLevelData.Count; i++)
			{
				if (allLevelData[i].sceneNumber == thisLevelData.sceneNumber)
				{
					allLevelData[i] = thisLevelData;
					found = true;
					break;
				}
			}
			
			if (!found)
			{
				allLevelData.Add (thisLevelData);
			}
		}

		
		private void UnloadNavMesh (int navMeshInt, SceneSettings sceneSettings)
		{
			NavigationMesh navMesh = Serializer.returnComponent <NavigationMesh> (navMeshInt, sceneSettings.gameObject);

			if (navMesh && sceneSettings && sceneSettings.navigationMethod != AC_NavigationMethod.UnityNavigation)
			{
				if (sceneSettings.navMesh)
				{
					NavigationMesh oldNavMesh = sceneSettings.navMesh;
					oldNavMesh.TurnOff ();
				}

				navMesh.TurnOn ();
				sceneSettings.navMesh = navMesh;

				// Bugfix: Need to cycle this otherwise weight caching doesn't always work
				navMesh.TurnOff ();
				navMesh.TurnOn ();
			}
		}


		private void UnloadPlayerStart (int playerStartInt, SceneSettings sceneSettings)
		{
			PlayerStart playerStart = Serializer.returnComponent <PlayerStart> (playerStartInt, sceneSettings.gameObject);

			if (playerStart && sceneSettings)
			{
				sceneSettings.defaultPlayerStart = playerStart;
			}
		}


		private void UnloadSortingMap (int sortingMapInt, SceneSettings sceneSettings)
		{
			SortingMap sortingMap = Serializer.returnComponent <SortingMap> (sortingMapInt, sceneSettings.gameObject);

			if (sortingMap && sceneSettings)
			{
				sceneSettings.sortingMap = sortingMap;
				KickStarter.sceneSettings.UpdateAllSortingMaps ();
			}
		}


		private void UnloadTintMap (int tintMapInt, SceneSettings sceneSettings)
		{
			TintMap tintMap = Serializer.returnComponent <TintMap> (tintMapInt, sceneSettings.gameObject);
			
			if (tintMap && sceneSettings)
			{
				sceneSettings.tintMap = tintMap;
				
				// Reset all FollowTintMap components
				FollowTintMap[] followTintMaps = FindObjectsOfType (typeof (FollowTintMap)) as FollowTintMap[];
				foreach (FollowTintMap followTintMap in followTintMaps)
				{
					followTintMap.ResetTintMap ();
				}
			}
		}


		private void UnloadCutsceneOnLoad (int cutsceneInt, SceneSettings sceneSettings)
		{
			Cutscene cutscene = Serializer.returnComponent <Cutscene> (cutsceneInt, sceneSettings.gameObject);

			if (cutscene && sceneSettings)
			{
				sceneSettings.cutsceneOnLoad = cutscene;
			}
		}


		private void UnloadCutsceneOnStart (int cutsceneInt, SceneSettings sceneSettings)
		{
			Cutscene cutscene = Serializer.returnComponent <Cutscene> (cutsceneInt, sceneSettings.gameObject);

			if (cutscene && sceneSettings)
			{
				sceneSettings.cutsceneOnStart = cutscene;
			}
		}


		private List<TransformData> PopulateTransformData (SubScene subScene)
		{
			List<TransformData> allTransformData = new List<TransformData>();
			RememberTransform[] transforms = UnityVersionHandler.GetOwnSceneComponents <RememberTransform> ((subScene != null) ? subScene.gameObject : null);

			foreach (RememberTransform _transform in transforms)
			{
				if (_transform.constantID != 0)
				{
					allTransformData.Add (_transform.SaveTransformData ());
				}
				else
				{
					ACDebug.LogWarning ("GameObject " + _transform.name + " was not saved because its ConstantID has not been set!");
				}
			}
			
			return allTransformData;
		}


		private void UnloadTransformData (List<TransformData> _transforms, SubScene subScene)
		{
			// Delete any objects (if told to)
			RememberTransform[] currentTransforms = UnityVersionHandler.GetOwnSceneComponents <RememberTransform> ((subScene != null) ? subScene.gameObject : null);
			foreach (RememberTransform transformOb in currentTransforms)
			{
				if (transformOb.saveScenePresence)
				{
					// Was object not saved?
					bool found = false;
					foreach (TransformData _transform in _transforms)
					{
						if (_transform.objectID == transformOb.constantID)
						{
							//found = true;
							found = !_transform.savePrevented;
						}
					}

					if (!found)
					{
						// Can't find: delete
						KickStarter.sceneChanger.ScheduleForDeletion (transformOb.gameObject);
					}
				}
			}

			Object[] prefabAssets = Resources.LoadAll ("SaveableData/Prefabs", typeof (GameObject));
			if (prefabAssets == null || prefabAssets.Length == 0)
			{
				prefabAssets = Resources.LoadAll ("", typeof (GameObject));
			}

			foreach (TransformData _transform in _transforms)
			{
				RememberTransform saveObject = Serializer.returnComponent <RememberTransform> (_transform.objectID);
				if (saveObject == null)
				{
					// Restore any deleted objects (if told to)
					if (_transform.bringBack && !_transform.savePrevented)
					{
						bool foundObject = false;
						foreach (Object prefabAsset in prefabAssets)
						{
							if (prefabAsset is GameObject)
							{
								GameObject prefabGameObject = (GameObject) prefabAsset;
								if (prefabGameObject.GetComponent <RememberTransform>())
								{
									int prefabID = prefabGameObject.GetComponent <ConstantID>().constantID;

									if ((_transform.linkedPrefabID != 0 && prefabID == _transform.linkedPrefabID) ||
										(_transform.linkedPrefabID == 0 && prefabID == _transform.objectID))
									{
										GameObject newObject = (GameObject) Instantiate (prefabGameObject);
										newObject.name = prefabGameObject.name;
										saveObject = newObject.GetComponent <RememberTransform>();
										foundObject = true;

										if (_transform.linkedPrefabID != 0 && prefabID == _transform.linkedPrefabID)
										{
											// Spawned object has wrong ID, re-assign it
											ConstantID[] idScripts = saveObject.GetComponents <ConstantID>();
											foreach (ConstantID idScript in idScripts)
											{
												idScript.constantID = _transform.objectID;
											}
										}

										break;
									}
								}
							}
						}

						if (!foundObject)
						{
							ACDebug.LogWarning ("Could not find Resources prefab with ID " + _transform.objectID + " - is it placed in a Resources folder?");
						}
					}
				}

				if (saveObject != null)
				{
					saveObject.LoadTransformData (_transform);
				}
			}

			Resources.UnloadUnusedAssets ();
			KickStarter.stateHandler.IgnoreNavMeshCollisions ();
		}


		private List<ScriptData> PopulateScriptData (SubScene subScene)
		{
			List<ScriptData> allScriptData = new List<ScriptData>();
			Remember[] scripts = UnityVersionHandler.GetOwnSceneComponents <Remember> ((subScene != null) ? subScene.gameObject : null);

			foreach (Remember _script in scripts)
			{
				if (_script.constantID != 0)
				{
					allScriptData.Add (new ScriptData (_script.constantID, _script.SaveData ()));
				}
				else
				{
					ACDebug.LogWarning ("GameObject " + _script.name + " was not saved because its ConstantID has not been set!");
				}
			}
			
			return allScriptData;
		}


		private void AssignMenuLocks (List<Menu> menus, string menuLockData)
		{
			if (string.IsNullOrEmpty (menuLockData))
			{
				return;
			}

			string[] lockArray = menuLockData.Split (SaveSystem.pipe[0]);
			
			foreach (string chunk in lockArray)
			{
				string[] chunkData = chunk.Split (SaveSystem.colon[0]);
				
				int _id = 0;
				int.TryParse (chunkData[0], out _id);
				
				bool _lock = false;
				bool.TryParse (chunkData[1], out _lock);
				
				foreach (AC.Menu _menu in menus)
				{
					if (_menu.id == _id)
					{
						_menu.isLocked = _lock;
						break;
					}
				}
			}
		}


		private void UnloadVariablesData (string data, LocalVariables localVariables)
		{
			if (data == null)
			{
				return;
			}

			if (data.Length > 0)
			{
				string[] varsArray = data.Split (SaveSystem.pipe[0]);
				
				foreach (string chunk in varsArray)
				{
					string[] chunkData = chunk.Split (SaveSystem.colon[0]);
					
					int _id = 0;
					int.TryParse (chunkData[0], out _id);

					GVar var = LocalVariables.GetVariable (_id, localVariables);
					if (var.type == VariableType.String)
					{
						string _text = chunkData[1];
						var.SetStringValue (_text);
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
		}

	}
		

	/**
	 * A data container for a single scene's save data. Used by the LevelStorage component.
	 */
	[System.Serializable]
	public class SingleLevelData
	{

		/** A List of all data recorded by the scene's Remember scripts */
		public List<ScriptData> allScriptData;
		/** A List of all data recorded by the scene's RememberTransform scripts */
		public List<TransformData> allTransformData;
		/** The scene number this data is for */
		public int sceneNumber;

		/** The ConstantID number of the default NavMesh */
		public int navMesh;
		/** The ConstantID number of the default PlayerStart */
		public int playerStart;
		/** The ConstantID number of the scene's SortingMap */
		public int sortingMap;
		/** The ConstantID number of the scene's TintMap */
		public int tintMap;
		/** The ConstantID number of the "On load" Cutscene */
		public int onLoadCutscene;
		/** The ConstantID number of the "On start" Cutscene */
		public int onStartCutscene;
		/** Data regarding paused and skipping ActionLists */
		public string activeLists;

		/** The values of the scene's local Variables, combined into a single string */
		public string localVariablesData;


		/**
		 * The default Constructor.
		 */
		public SingleLevelData ()
		{
			allScriptData = new List<ScriptData>();
			allTransformData = new List<TransformData>();
		}

	}


	/**
	 * A data container for save data returned by each Remember script.  Used by the SingleLevelData class.
	 */
	[System.Serializable]
	public struct ScriptData
	{

		/** The Constant ID number of the Remember script component */
		public int objectID;
		/** The data returned by the Remember script, serialised into a string */
		public string data;


		/**
		 * <summary>The default Constructor.</summary>
		 * <param name = "_objectID">The Remember script's Constant ID number</param>
		 * <param name = "_data">The serialised data</param>
		 */
		public ScriptData (int _objectID, string _data)
		{
			objectID = _objectID;
			data = _data;
		}
	}

}