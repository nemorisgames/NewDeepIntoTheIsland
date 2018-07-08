/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"SubScene.cs"
 * 
 *	This component keeps track of data regarding any scene added to the active scene during gameplay.
 *	It is generated automatically by the sub-scene's MultiSceneChecker, and should not be added manually.
 * 
 */

using UnityEngine;

namespace AC
{

	/**
	 *	This component keeps track of data regarding any scene added to the active scene during gameplay.
	 *	It is generated automatically by the sub-scene's MultiSceneChecker, and should not be added manually.
	 */
	public class SubScene : MonoBehaviour
	{
		#if UNITY_5_3 || UNITY_5_4 || UNITY_5_3_OR_NEWER

		private SceneInfo sceneInfo;

		private LocalVariables localVariables;
		private SceneSettings sceneSettings;

		private KickStarter kickStarter;
		private MainCamera mainCamera;

		#endif

		/**
		 * Gets the SceneInfo for the scene that this component represents.
		 */
		public SceneInfo SceneInfo
		{
			get
			{
				#if UNITY_5_3 || UNITY_5_4 || UNITY_5_3_OR_NEWER
				return sceneInfo;
				#else
				return null;
				#endif
			}
		}


		/**
		 * Gets the LocalVariables for the scene that this component represents.
		 */
		public LocalVariables LocalVariables
		{
			get
			{
				#if UNITY_5_3 || UNITY_5_4 || UNITY_5_3_OR_NEWER
				return localVariables;
				#else
				return null;
				#endif
			}
		}


		/**
		 * Gets the SceneSettings for the scene that this component represents.
		 */
		public SceneSettings SceneSettings
		{
			get
			{
				#if UNITY_5_3 || UNITY_5_4 || UNITY_5_3_OR_NEWER
				return sceneSettings;
				#else
				return null;
				#endif
			}
		}


		/**
		 * <summary>Syncs the component with the correct scene.</summary>
		 * <param name = "_multiSceneChecker">The MultiSceneChecker component in the scene for which this component is to sync with.</param>
		 */
		public void Initialise (MultiSceneChecker _multiSceneChecker)
		{
			#if UNITY_5_3 || UNITY_5_4 || UNITY_5_3_OR_NEWER
			
			UnityEngine.SceneManagement.Scene scene = _multiSceneChecker.gameObject.scene;
			gameObject.name = "SubScene " + scene.buildIndex;

			kickStarter = _multiSceneChecker.GetComponent <KickStarter>();

			sceneInfo = new SceneInfo (scene.name, scene.buildIndex);

			localVariables = _multiSceneChecker.GetComponent <LocalVariables>();
			sceneSettings = _multiSceneChecker.GetComponent <SceneSettings>();

			UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene (gameObject, scene);

			kickStarter = UnityVersionHandler.GetOwnSceneInstance <KickStarter> (gameObject);
			if (kickStarter != null)
			{
				kickStarter.gameObject.SetActive (false);
			}

			mainCamera = UnityVersionHandler.GetOwnSceneInstance <MainCamera> (gameObject);
			if (mainCamera != null)
			{
				mainCamera.gameObject.SetActive (false);
			}

			Player ownPlayer = UnityVersionHandler.GetOwnSceneInstance <Player> (gameObject);
			if (ownPlayer != null)
			{
				ownPlayer.gameObject.SetActive (false);
			}

			if (sceneSettings.OverridesCameraPerspective ())
			{
				ACDebug.LogError ("The added scene (" + scene.name + ", " + scene.buildIndex + ") overrides the default camera perspective - this feature should not be used in conjunction with multiple-open scenes.", gameObject);
			}

			KickStarter.sceneChanger.RegisterSubScene (this);

			#endif
		}


		/**
		 * Prepares the sub-scene to become the new active scene, due to the active scene being removed.  The gameobject will be destroyed afterwards.
		 */
		public void MakeMain ()
		{
			#if UNITY_5_3 || UNITY_5_4 || UNITY_5_3_OR_NEWER

			if (mainCamera)
			{
				mainCamera.gameObject.SetActive (true);
			}
			if (kickStarter)
			{
				kickStarter.gameObject.SetActive (true);
			}

			KickStarter.SetGameEngine (gameObject);
			KickStarter.mainCamera = mainCamera;

			UnityEngine.SceneManagement.SceneManager.SetActiveScene (gameObject.scene);
			Destroy (gameObject);

			#endif
		}

	}

}