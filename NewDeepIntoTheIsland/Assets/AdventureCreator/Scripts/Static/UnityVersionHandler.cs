/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"UnityVersionHandler.cs"
 * 
 *	This is a static class that contains commonly-used functions that vary depending on which version of Unity is being used.
 * 
 */

using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
	#if UNITY_5_3 || UNITY_5_4 || UNITY_5_3_OR_NEWER
		using UnityEditor.SceneManagement;
	#endif
#endif

namespace AC
{

	/**
	 * This is a class that contains commonly-used functions that vary depending on which version of Unity is being used.
	 */
	public class UnityVersionHandler
	{

		/**
		 * <summary>Performs a Physics2D.Raycast on Triggers in the scene.</summary>
		 * <param name = "origin">The ray's origin</param>
		 * <param name = "direction">The ray's direction</param>
		 * <param name = "layerMask">The LayerMask to act upon</param>
		 * <returns>The result of the Physics2D.Raycast</returns>
		 */
		public static RaycastHit2D Perform2DRaycast (Vector2 origin, Vector2 direction, float length, LayerMask layerMask)
		{
			#if UNITY_5_6 || UNITY_5_6_OR_NEWER
			RaycastHit2D[] hits = new RaycastHit2D [1];
			ContactFilter2D filter = new ContactFilter2D();
			filter.useTriggers = true;
			filter.SetLayerMask (layerMask);
			filter.ClearDepth ();
			Physics2D.Raycast (origin, direction, filter, hits, length);
			return hits[0];
			#else
			return Physics2D.Raycast (origin, direction, length, layerMask);
			#endif
		}


		/**
		 * <summary>Performs a Physics2D.Raycast on Triggers in the scene.</summary>
		 * <param name = "origin">The ray's origin</param>
		 * <param name = "direction">The ray's direction</param>
		 * <param name = "length">The ray's length</param>
		 * <returns>The result of the Physics2D.Raycast</returns>
		 */
		public static RaycastHit2D Perform2DRaycast (Vector2 origin, Vector2 direction, float length)
		{
			#if UNITY_5_6 || UNITY_5_6_OR_NEWER
			RaycastHit2D[] hits = new RaycastHit2D [1];
			ContactFilter2D filter = new ContactFilter2D();
			filter.useTriggers = true;
			filter.ClearDepth ();
			Physics2D.Raycast (origin, direction, filter, hits, length);
			return hits[0];
			#else
			return Physics2D.Raycast (origin, direction, length);
			#endif
		}


		/**
		 * <summary>Performs a Physics2D.OverlapCircle on Triggers in the scene.</summary>
		 * <param name = "point">The position of the circle's centre</param>
		 * <param name = "radius">The radius of the circle</param>
		 * <param name = "results">An array of results</param>
		 * <param name = "layerMask">The LayerMask to act upon</param>
		 * <returns>The result of the Physics2D.OverlapCircle</returns>
		 */
		public static int Perform2DOverlapCircle (Vector2 point, float radius, Collider2D[] results, LayerMask layerMask)
		{
			#if UNITY_5_6 || UNITY_5_6_OR_NEWER
			ContactFilter2D filter = new ContactFilter2D();
			filter.useTriggers = true;
			filter.SetLayerMask (layerMask);
			filter.ClearDepth ();

			return Physics2D.OverlapCircle (point, radius, filter, results);
			#else
			return Physics2D.OverlapCircleNonAlloc (point, radius, results, layerMask);
			#endif
		}


		/**
		 * <summary>Performs a Physics2D.OverlapPoint on Triggers in the scene.</summary>
		 * <param name = "point">The position of the point</param>
		 * <param name = "results">An array of results</param>
		 * <param name = "layerMask">The LayerMask to act upon</param>
		 * <returns>The result of the Physics2D.OverlapPoint</returns>
		 */
		public static int Perform2DOverlapPoint (Vector2 point, Collider2D[] results, LayerMask layerMask)
		{
			#if UNITY_5_6 || UNITY_5_6_OR_NEWER
			ContactFilter2D filter = new ContactFilter2D();
			filter.useTriggers = true;
			filter.SetLayerMask (layerMask);
			filter.ClearDepth ();

			return Physics2D.OverlapPoint (point, filter, results);
			#else
			return Physics2D.OverlapPointNonAlloc (point, results, layerMask);
			#endif
		}


		/**
		 * <summary>Gets the offset/centre of a 2D Hotspot's icon in relation to the Hotspot's centre.</summary>
		 * <param name = "_boxCollider2D">The Hotspot's BoxCollider2D component.</param>
		 * <param name = "transform">The Hotspot's Transform component.</param>
		 * <returns>The offset/centre of a 2D Hotspot's icon in relation to the Hotspot's centre.</returns>
		 */
		public static Vector3 Get2DHotspotOffset (BoxCollider2D _boxCollider2D, Transform transform)
		{
			#if UNITY_5 || UNITY_2017_1_OR_NEWER
			return new Vector3 (_boxCollider2D.offset.x, _boxCollider2D.offset.y * transform.localScale.y, 0f);
			#else
			return new Vector3 (_boxCollider2D.center.x, _boxCollider2D.center.y * transform.localScale.y, 0f);
			#endif
		}


		/**
		 * <summary>Sets the visiblity of the cursor.</summary>
		 * <param name = "state">If True, the cursor will be shown. If False, the cursor will be hidden."</param>
		 */
		public static void SetCursorVisibility (bool state)
		{
			#if UNITY_EDITOR
			if (KickStarter.cursorManager != null && KickStarter.cursorManager.forceCursorInEditor)
			{
				state = true;
			}
			#endif

			#if UNITY_5 || UNITY_2017_1_OR_NEWER
			Cursor.visible = state;
			#else
			Screen.showCursor = state;
			#endif
		}


		/**
		 * The 'lock' state of the cursor.
		 */
		public static bool CursorLock
		{
			get
			{
				#if UNITY_5 || UNITY_2017_1_OR_NEWER
				return (Cursor.lockState == CursorLockMode.Locked) ? true : false;
				#else
				return Screen.lockCursor;
				#endif
			}
			set
			{
				if (KickStarter.cursorManager.cursorRendering == CursorRendering.Software && !KickStarter.cursorManager.lockSystemCursor)
				{
					return;
				}

				#if UNITY_5 || UNITY_2017_1_OR_NEWER
				if (value)
				{
					Cursor.lockState = CursorLockMode.Locked;
				}
				else
				{
					Cursor.lockState = CursorLockMode.None;
				}
				#else
				Screen.lockCursor = value;
				#endif
			}
		}


		/**
		 * <summary>Gets the index number of the active scene, as listed in the Build Settings.</summary>
		 * <returns>The index number of the active scene, as listed in the Build Settings.</returns>
		 */
		public static int GetCurrentSceneNumber ()
		{
			#if UNITY_5_3 || UNITY_5_4 || UNITY_5_3_OR_NEWER
			return UnityEngine.SceneManagement.SceneManager.GetActiveScene ().buildIndex;
			#else
			return Application.loadedLevel;
			#endif
		}


		/**
		 * <summary>Gets a SceneInfo class for the scene that a given GameObject is in.</summary>
		 * <param name = "_gameObject">The GameObject in the scene</param>
		 * <returns>A SceneInfo class for the scene that a given GameObject is in.</returns>
		 */
		public static SceneInfo GetSceneInfoFromGameObject (GameObject _gameObject)
		{
			#if UNITY_5_3 || UNITY_5_4 || UNITY_5_3_OR_NEWER
			return new SceneInfo (_gameObject.scene.name, _gameObject.scene.buildIndex);
			#else
			return new SceneInfo ();
			#endif
		}


		/**
		 * <summary>Gets the LocalVariables component that is in the same scene as a given GameObject.</summary>
		 * <param name = "_gameObject">The GameObject in the scene</param>
		 * <returns>The LocalVariables component that is in the same scene as the given GameObject</returns>
		 */
		public static LocalVariables GetLocalVariablesOfGameObject (GameObject _gameObject)
		{
			if (UnityVersionHandler.ObjectIsInActiveScene (_gameObject))
			{
				return KickStarter.localVariables;
			}

			#if UNITY_5_3 || UNITY_5_4 || UNITY_5_3_OR_NEWER
			UnityEngine.SceneManagement.Scene scene = _gameObject.scene;
			if (Application.isPlaying)
			{
				foreach (SubScene subScene in KickStarter.sceneChanger.GetSubScenes ())
				{
					if (subScene.gameObject.scene == scene)
					{
						return subScene.LocalVariables;
					}
				}
			}
			else
			{
				foreach (LocalVariables localVariables in GameObject.FindObjectsOfType <LocalVariables>())
				{
					if (localVariables.gameObject.scene == scene)
					{
						return localVariables;
					}
				}
			}
			#endif
			return null;
		}


		/**
		 * <summary>Gets the SceneSettings component that is in the same scene as a given GameObject.</summary>
		 * <param name = "_gameObject">The GameObject in the scene</param>
		 * <returns>The SceneSettings component that is in the same scene as the given GameObject</returns>
		 */
		public static SceneSettings GetSceneSettingsOfGameObject (GameObject _gameObject)
		{
			if (UnityVersionHandler.ObjectIsInActiveScene (_gameObject))
			{
				return KickStarter.sceneSettings;
			}

			#if UNITY_5_3 || UNITY_5_4 || UNITY_5_3_OR_NEWER
			UnityEngine.SceneManagement.Scene scene = _gameObject.scene;
			if (Application.isPlaying)
			{
				foreach (SubScene subScene in KickStarter.sceneChanger.GetSubScenes ())
				{
					if (subScene.gameObject.scene == scene)
					{
						return subScene.SceneSettings;
					}
				}
			}
			else
			{
				foreach (SceneSettings sceneSettings in GameObject.FindObjectsOfType <SceneSettings>())
				{
					if (sceneSettings.gameObject.scene == scene)
					{
						return sceneSettings;
					}
				}
			}
			#endif
			return null;
		}


		/**
		 * <summary>Gets the name of the active scene.</summary>
		 * <returns>The name of the active scene. If this is called in the Editor, the full filepath is also returned.</returns>
		 */
		public static string GetCurrentSceneName ()
		{
			#if UNITY_5_3 || UNITY_5_4 || UNITY_5_3_OR_NEWER
				#if UNITY_EDITOR
				if (!Application.isPlaying)
				{
					return UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene ().name;
				}
				#endif
				return UnityEngine.SceneManagement.SceneManager.GetActiveScene ().name;
			#else
				#if UNITY_EDITOR
				if (!Application.isPlaying)
				{
					return EditorApplication.currentScene;
				}
				#endif
				return Application.loadedLevelName;
			#endif
		}


		/**
		 * <summary>Loads a scene by name.</summary>
		 * <param name = "sceneName">The name of the scene to load</param>
		 * <param name = "forceReload">If True, the scene will be re-loaded if it is already open.</param>
		 * <param name = "loadAdditively">If True, the scene will be loaded on top of the active scene (Unity 5.3 only)</param>
		 */
		public static void OpenScene (string sceneName, bool forceReload = false, bool loadAdditively = false)
		{
			if (string.IsNullOrEmpty (sceneName)) return;

			try
			{
				if (forceReload || GetCurrentSceneName () != sceneName)
				{
					#if UNITY_5_3 || UNITY_5_4 || UNITY_5_3_OR_NEWER
						#if UNITY_EDITOR
						if (!Application.isPlaying)
						{
							UnityEditor.SceneManagement.EditorSceneManager.OpenScene (sceneName);
							return;
						}
						#endif

						UnityEngine.SceneManagement.LoadSceneMode loadSceneMode = (loadAdditively) ? UnityEngine.SceneManagement.LoadSceneMode.Additive : UnityEngine.SceneManagement.LoadSceneMode.Single;
						UnityEngine.SceneManagement.SceneManager.LoadScene (sceneName, loadSceneMode);
					#else
						if (loadAdditively)
						{
							ACDebug.LogWarning ("Additive scene loading is only available when using Unity 5.3 or newer.");
						}
						#if UNITY_EDITOR
						if (!Application.isPlaying)
						{
							EditorApplication.OpenScene (sceneName);
							return;
						}
						#endif
						Application.LoadLevel (sceneName);
					#endif
				}
			}
			catch (System.Exception e)
 			{
				Debug.LogWarning ("Error when opening scene " + sceneName + ": " + e);
 			}
		}


		/**
		 * <summary>Loads a scene by index number, as listed in the Build Settings.</summary>
		 * <param name = "sceneNumber">The index number of the scene to load, as listed in the Build Settings</param>
		 * <param name = "forceReload">If True, the scene will be re-loaded if it is already open.</param>
		 * <param name = "loadAdditively">If True, the scene will be loaded on top of the active scene (Unity 5.3 only)</param>
		 */
		public static void OpenScene (int sceneNumber, bool forceReload = false, bool loadAdditively = false)
		{
			if (sceneNumber < 0) return;

			if (KickStarter.settingsManager.reloadSceneWhenLoading)
			{
				forceReload = true;
			}

			try
			{
				if (forceReload || GetCurrentSceneNumber () != sceneNumber)
				{
					#if UNITY_5_3 || UNITY_5_4 || UNITY_5_3_OR_NEWER
						UnityEngine.SceneManagement.LoadSceneMode loadSceneMode = (loadAdditively) ? UnityEngine.SceneManagement.LoadSceneMode.Additive : UnityEngine.SceneManagement.LoadSceneMode.Single;
						UnityEngine.SceneManagement.SceneManager.LoadScene (sceneNumber, loadSceneMode);
					#else
						if (loadAdditively)
						{
							ACDebug.LogWarning ("Additive scene loading is only available when using Unity 5.3 or newer.");
						}
						Application.LoadLevel (sceneNumber);
					#endif
				}
			}
			catch (System.Exception e)
 			{
				Debug.LogWarning ("Error when opening scene " + sceneNumber + ": " + e);
 			}
		}


		/**
		 * <summary>Closes a scene by name.</summary>
		 * <param name = "sceneName">The name of the scene to load</param>
		 * <returns>True if the close was successful</returns>
		 */
		public static bool CloseScene (string sceneName)
		{
			if (string.IsNullOrEmpty (sceneName)) return false;

			if (GetCurrentSceneName () != sceneName)
			{
				#if UNITY_5_5_OR_NEWER
				UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync (sceneName);
				return true;
				#elif UNITY_5_3 || UNITY_5_4 || UNITY_5_3_OR_NEWER
				UnityEngine.SceneManagement.SceneManager.UnloadScene (sceneName);
				return true;
				#else
				ACDebug.LogWarning ("Additive scene loading is only available when using Unity 5.3 or newer.");
				#endif
			}
			return false;
		}


		/**
		 * <summary>Closes a scene by index number, as listed in the Build Settings.</summary>
		 * <param name = "sceneNumber">The index number of the scene to load, as listed in the Build Settings</param>
		 * <returns>True if the close was successful</returns>
		 */
		public static bool CloseScene (int sceneNumber)
		{
			if (sceneNumber < 0) return false;

			if (GetCurrentSceneNumber () != sceneNumber)
			{
				#if UNITY_5_5_OR_NEWER
				UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync (sceneNumber);
				return true;
				#elif UNITY_5_3 || UNITY_5_4 || UNITY_5_3_OR_NEWER
				UnityEngine.SceneManagement.SceneManager.UnloadScene (sceneNumber);
				return true;
				#else
				ACDebug.LogWarning ("Additive scene loading is only available when using Unity 5.3 or newer.");
				#endif
			}
			return false;
		}


		/**
		 * <summary>Loads the scene asynchronously.</summary>
		 * <param name = "sceneNumber">The index number of the scene to load.</param>
		 * <param name = "sceneName">The name of the scene to load. If this is blank, sceneNumber will be used instead.</param>
		 * <returns>The generated AsyncOperation class</returns>
		 */
		public static AsyncOperation LoadLevelAsync (int sceneNumber, string sceneName = "")
		{
			if (sceneName != "")
			{
				#if UNITY_5_3 || UNITY_5_4 || UNITY_5_3_OR_NEWER
				return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync (sceneName);
				#else
				return Application.LoadLevelAsync (sceneName);
				#endif
			}

			#if UNITY_5_3 || UNITY_5_4 || UNITY_5_3_OR_NEWER
			return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync (sceneNumber);
			#else
			return Application.LoadLevelAsync (sceneNumber);
			#endif
		}


		/**
		 * <summary>Checks if Json serialization is supported by the current version of Unity.</summary>
		 * <returns>True if Json serialization is supported by the current version of Unity.</returns>
		 */
		public static bool CanUseJson ()
		{
			#if UNITY_5_3 || UNITY_5_4 || UNITY_5_3_OR_NEWER
			return true;
			#else
			return false;
			#endif
		}


		/**
		 * <summary>Places a supplied GameObject in a "folder" scene object, as generated by the Scene Manager.</summary>
		 * <param name = "ob">The GameObject to move into a folder</param>
		 * <param name = "folderName">The name of the folder scene object</param>
		 * <returns>True if a suitable folder object was found, and ob was successfully moved.</returns>
		 */
		public static bool PutInFolder (GameObject ob, string folderName)
		{
			if (ob == null || string.IsNullOrEmpty (folderName)) return false;

			#if UNITY_5_3 || UNITY_5_4 || UNITY_5_3_OR_NEWER
			
			UnityEngine.Object[] folders = Object.FindObjectsOfType (typeof (GameObject));
			foreach (GameObject folder in folders)
			{
				if (folder.name == folderName && folder.transform.position == Vector3.zero && folderName.Contains ("_") && folder.gameObject.scene == UnityEngine.SceneManagement.SceneManager.GetActiveScene ())
				{
					ob.transform.parent = folder.transform;
					return true;
				}
			}

			#else

			if (ob && GameObject.Find (folderName))
			{
				if (GameObject.Find (folderName).transform.position == Vector3.zero && folderName.Contains ("_"))
				{
					ob.transform.parent = GameObject.Find (folderName).transform;
					return true;
				}
			}

			#endif
					
			return false;
		}


		#if UNITY_EDITOR

		public static void NewScene ()
		{
			#if UNITY_5_3 || UNITY_5_4 || UNITY_5_3_OR_NEWER
			EditorSceneManager.NewScene (NewSceneSetup.DefaultGameObjects);
			#else
			EditorApplication.NewScene ();
			#endif
		}


		public static void SaveScene ()
		{
			#if UNITY_5_3 || UNITY_5_4 || UNITY_5_3_OR_NEWER
			UnityEngine.SceneManagement.Scene currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene ();
			EditorSceneManager.SaveScene (currentScene);
			#else
			EditorApplication.SaveScene ();
			#endif
		}


		public static bool SaveSceneIfUserWants ()
		{
			#if UNITY_5_3 || UNITY_5_4 || UNITY_5_3_OR_NEWER
			return EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo ();
			#else
			return EditorApplication.SaveCurrentSceneIfUserWantsTo ();
			#endif
		}


		/**
		 * <summary>Sets the title of an editor window (Unity Editor only).</summary>
		 * <param name = "window">The EditorWindow to affect</param>
		 * <param name = "label">The title of the window</param>
		 */
		public static void SetWindowTitle <T> (T window, string label) where T : EditorWindow
		{
			#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_3_OR_NEWER
			window.titleContent.text = label;
			#else
			window.title = label;
			#endif
		}


		public static Vector2 GetBoxCollider2DCentre (BoxCollider2D _boxCollider2D)
		{
			#if UNITY_5 || UNITY_2017_1_OR_NEWER
			return _boxCollider2D.offset;
			#else
			return _boxCollider2D.center;
			#endif
		}


		public static void SetBoxCollider2DCentre (BoxCollider2D _boxCollider2D, Vector2 offset)
		{
			#if UNITY_5 || UNITY_2017_1_OR_NEWER
			_boxCollider2D.offset = offset;
			#else
			_boxCollider2D.center = offset;
			#endif
		}


		/**
		 * <summary>Gets the name of the active scene, if multiple scenes are being edited.</summary>
		 * <returns>The name of the active scene, if multiple scenes are being edited. Returns nothing otherwise.</returns>
		 */
		public static string GetActiveSceneName ()
		{
			#if UNITY_5_3 || UNITY_5_4 || UNITY_5_3_OR_NEWER
			if (UnityEngine.SceneManagement.SceneManager.sceneCount <= 1)
			{
				return "";
			}
			UnityEngine.SceneManagement.Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene ();
			if (activeScene.name != "")
			{
				return activeScene.name;
			}
			return "New scene";
			#else
			return "";
			#endif
		}


		/**
		 * <summary>Checks if a suppplied GameObject is present within the active scene.</summary>
		 * <param name = "gameObjectName">The name of the GameObject to check for</param>
		 * <returns>True if the GameObject is present within the active scene.</returns>
		 */
		public static bool ObjectIsInActiveScene (string gameObjectName)
		{
			if (string.IsNullOrEmpty (gameObjectName) || !GameObject.Find (gameObjectName))
			{
				return false;
			}

			#if UNITY_5_3 || UNITY_5_4 || UNITY_5_3_OR_NEWER
			UnityEngine.SceneManagement.Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene ();

			UnityEngine.Object[] allObjects = Object.FindObjectsOfType (typeof (GameObject));
			foreach (GameObject _object in allObjects)
			{
				if ((_object.name == gameObjectName && _object.scene == activeScene) ||
					_object.scene.name == "DontDestroyOnLoad" ||
					(_object.scene.name == null && _object.scene.buildIndex == -1)) // Because on Android, DontDestroyOnLoad scene has no name
				{
					return true;
				}
			}
			return false;
			#else
			return GameObject.Find (gameObjectName);
			#endif
		}


		/**
		 * <Summary>Marks an object as dirty so that changes made will be saved.
		 * In Unity 5.3 and above, the scene itself is marked as dirty to ensure it is properly changed.</summary>
		 * <param name = "_target">The object to mark as dirty</param>
		 * <param name = "force">If True, then the object will be marked as dirty regardless of whether or not GUI.changed is true. This should not be set if called every frame.</param>
		 */
		public static void CustomSetDirty (Object _target, bool force = false)
		{
			if (_target != null && (force || GUI.changed))
			{
				#if UNITY_5_3 || UNITY_5_4 || UNITY_5_3_OR_NEWER
				if (!Application.isPlaying && PrefabUtility.GetPrefabType (_target) != PrefabType.Prefab)
				{
					if (_target is MonoBehaviour)
					{
						MonoBehaviour monoBehaviour = (MonoBehaviour) _target;
						UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty (monoBehaviour.gameObject.scene);
					}
					else
					{
						UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty ();
					}
				}
				#endif
				EditorUtility.SetDirty (_target);
			}
		}


		public static string GetCurrentSceneFilepath ()
		{
			string sceneName = GetCurrentSceneName ();

			if (!string.IsNullOrEmpty (sceneName))
			{
				foreach (UnityEditor.EditorBuildSettingsScene S in UnityEditor.EditorBuildSettings.scenes)
				{
					if (S.enabled)
					{
						if (S.path.Contains (sceneName))
						{
							return S.path;
						}
					}
				}
			}
			return "";
		}


		public static bool ShouldAssignPrefabConstantID (GameObject gameObject)
		{
			#if UNITY_2018_2_OR_NEWER
			if (PrefabUtility.GetCorrespondingObjectFromSource (gameObject) == null && PrefabUtility.GetPrefabObject (gameObject) != null)
			{
				return true;
			}
			#else
			if (PrefabUtility.GetPrefabParent (gameObject) == null && PrefabUtility.GetPrefabObject (gameObject) != null)
			{
				return true;
			}
			#endif
			return false;
		}

		#endif


		/**
		 * <summary>Checks if a suppplied GameObject is present within the active scene.</summary>
		 * <param name = "gameObject">The GameObject to check for</param>
		 * <returns>True if the GameObject is present within the active scene.</returns>
		 */
		public static bool ObjectIsInActiveScene (GameObject gameObject)
		{
			if (gameObject == null)
			{
				return false;
			}
			#if UNITY_EDITOR
			if (UnityEditor.PrefabUtility.GetPrefabType (gameObject) == PrefabType.Prefab)
			{
				return false;
			}
			#endif
			#if UNITY_5_3 || UNITY_5_4 || UNITY_5_3_OR_NEWER
			UnityEngine.SceneManagement.Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene ();
			if (gameObject.scene == activeScene ||
				gameObject.scene.name == "DontDestroyOnLoad" ||
				(gameObject.scene.name == null && gameObject.scene.buildIndex == -1)) // Because on Android, DontDestroyOnLoad scene has no name
			{
				return true;
			}
			return false;
			#else
			return true;
			#endif
		}


		/**
		 * <summary>Finds the correct instance of a component required by the KickStarter script.</summary>
		 */
		public static T GetKickStarterComponent <T> () where T : Behaviour
		{
			#if UNITY_EDITOR && (UNITY_5_3 || UNITY_5_4 || UNITY_5_3_OR_NEWER)
			if (Object.FindObjectsOfType <T>() != null)
			{
				UnityEngine.SceneManagement.Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene ();
				T[] instances = Object.FindObjectsOfType <T>() as T[];
				foreach (T instance in instances)
				{
					if (instance.gameObject.scene == activeScene || instance.gameObject.scene.name == "DontDestroyOnLoad")
					{
						return instance;
					}
				}
			}
			#else
			if (Object.FindObjectOfType <T>())
			{
				return Object.FindObjectOfType <T>();
			}
			#endif
			return null;
		}


		/**
		 * <summary>Gets a Behaviour that is in the same scene as a given GameObject.</summary>
		 * <param name = "_gameObject">The GameObject in the scene</param>
		 * <returns>The Behaviour that is in the same scene as the given GameObject</returns>
		 */
		public static T GetOwnSceneInstance <T> (GameObject gameObject) where T : Behaviour
		{
			#if UNITY_5_3 || UNITY_5_4 || UNITY_5_3_OR_NEWER

			UnityEngine.SceneManagement.Scene ownScene = gameObject.scene;

			T[] instances = Object.FindObjectsOfType (typeof (T)) as T[];
			foreach (T instance in instances)
			{
				if (instance != null && instance.gameObject.scene == ownScene)
				{
					return instance;
				}
			}

			#endif

			return null;
		}


		/**
		 * <summary>Gets all instances of a Component that are in the same scene as a given GameObject.</summary>
		 * <param name = "_gameObject">The GameObject in the scene</param>
		 * <returns>All instances of a Component that are in the same scene as the given GameObject</returns>
		 */
		public static T[] GetOwnSceneComponents <T> (GameObject gameObject = null) where T : Component
		{
			T[] instances = Object.FindObjectsOfType (typeof (T)) as T[];
			if (gameObject == null)
			{
				return instances;
			}

			#if UNITY_5_3 || UNITY_5_4 || UNITY_5_3_OR_NEWER

			UnityEngine.SceneManagement.Scene ownScene = gameObject.scene;
			List<T> instancesToSend = new List<T>();
			foreach (T instance in instances)
			{
				if (instance != null && instance.gameObject.scene == ownScene)
				{
					instancesToSend.Add (instance);
				}
			}
			return instancesToSend.ToArray ();

			#else

			return instances;

			#endif
		}


		/**
		 * Creates a generic EventSystem object for Unity UI-based Menus to use.
		 */
		public static UnityEngine.EventSystems.EventSystem CreateEventSystem ()
		{
			GameObject eventSystemObject = new GameObject ();
			eventSystemObject.name = "EventSystem";
			UnityEngine.EventSystems.EventSystem _eventSystem = eventSystemObject.AddComponent <UnityEngine.EventSystems.EventSystem>();

			if (KickStarter.settingsManager.inputMethod == InputMethod.TouchScreen)
			{
				eventSystemObject.AddComponent <StandaloneInputModule>();
			}
			else
			{
				eventSystemObject.AddComponent <OptionalMouseInputModule>();
			}
			#if UNITY_5_3 || UNITY_5_4 || UNITY_5_3_OR_NEWER
			#else
			eventSystemObject.AddComponent <TouchInputModule>();
			#endif
			return _eventSystem;
		}

	}

}