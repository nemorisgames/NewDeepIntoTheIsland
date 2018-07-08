using UnityEngine;

namespace AC
{

	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_multi_scene_checker.html")]
	#endif
	public class MultiSceneChecker : MonoBehaviour
	{

		private KickStarter ownKickStarter;


		private void Awake ()
		{
			if (!UnityVersionHandler.ObjectIsInActiveScene (gameObject))
			{
				// Register self as a "sub-scene"

				GameObject subSceneOb = new GameObject ();
				SubScene newSubScene = subSceneOb.AddComponent <SubScene>();
				newSubScene.Initialise (this);
				return;
			}

			ownKickStarter = GetComponent <KickStarter>();

			GameObject taggedMainCamera = GameObject.FindWithTag (Tags.mainCamera);
			if (taggedMainCamera == null)
			{
				ACDebug.LogError ("No MainCamera found - please click 'Organise room objects' in the Scene Manager to create one.");
			}
			else
			{
				if (taggedMainCamera.GetComponent <MainCamera>() == null &&
					taggedMainCamera.GetComponentInParent <MainCamera>() == null)
				{
					ACDebug.LogError ("MainCamera has no MainCamera component.");
				}
			}

			if (ownKickStarter != null)
			{
				KickStarter.mainCamera.OnAwake ();
				ownKickStarter.OnAwake ();
				KickStarter.playerInput.OnAwake ();
				KickStarter.playerQTE.OnAwake ();
				KickStarter.sceneSettings.OnAwake ();
				KickStarter.dialog.OnAwake ();
				KickStarter.navigationManager.OnAwake ();
				KickStarter.actionListManager.OnAwake ();

				KickStarter.stateHandler.RegisterWithGameEngine ();
			}
			else
			{
				ACDebug.LogError ("No KickStarter component found in the scene!");
			}
		}


		private void Start ()
		{
			if (!UnityVersionHandler.ObjectIsInActiveScene (gameObject))
			{
				return;
			}

			if (ownKickStarter != null)
			{
				KickStarter.sceneSettings.OnStart ();
				KickStarter.playerMovement.OnStart ();
				KickStarter.mainCamera.OnStart ();
			}
		}


		#if UNITY_EDITOR

		/**
		 * <summary>Allows the Scene and Variables Managers to show UI controls for the currently-active scene, if multiple scenes are being edited.</summary>
		 * <returns>The name of the currently-open scene.</summary>
		 */
		public static string EditActiveScene ()
		{
			string openScene = UnityVersionHandler.GetActiveSceneName ();

			if (openScene != "" && !Application.isPlaying)
			{
				if (FindObjectOfType <KickStarter>() != null)
				{
					FindObjectOfType <KickStarter>().ClearVariables ();
				}
			}

			return openScene;
		}

		#endif
		
	}

}