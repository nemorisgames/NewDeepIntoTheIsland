/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionSystemLock.cs"
 * 
 *	This action handles the enabling / disabling
 *	of individual AC systems, allowing for
 *	minigames or other non-adventure elements
 *	to be run.
 * 
 */

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class ActionSystemLock : Action
	{

		public bool changeMovementMethod = false;
		public MovementMethod newMovementMethod;

		public LockType cursorLock = LockType.NoChange;
		public LockType inputLock = LockType.NoChange;
		public LockType interactionLock = LockType.NoChange;
		public LockType menuLock = LockType.NoChange;
		public LockType movementLock = LockType.NoChange;
		public LockType cameraLock = LockType.NoChange;
		public LockType triggerLock = LockType.NoChange;
		public LockType playerLock = LockType.NoChange;
		public LockType saveLock = LockType.NoChange;
		public LockType keyboardGameplayMenusLock = LockType.NoChange;

		
		public ActionSystemLock ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Engine;
			title = "Manage systems";
			description = "Enables and disables individual systems within Adventure Creator, such as Interactions. Can also be used to change the 'Movement method', as set in the Settings Manager, but note that this change will not be recorded in save games.";
		}
		
		
		override public float Run ()
		{
			if (changeMovementMethod)
			{
				KickStarter.playerInput.InitialiseCursorLock (newMovementMethod);
				KickStarter.settingsManager.movementMethod = newMovementMethod;
			}

			if (cursorLock == LockType.Enabled)
			{
				KickStarter.stateHandler.SetCursorSystem (true);
			}
			else if (cursorLock == LockType.Disabled)
			{
				KickStarter.stateHandler.SetCursorSystem (false);
			}

			if (inputLock == LockType.Enabled)
			{
				KickStarter.stateHandler.SetInputSystem (true);
			}
			else if (inputLock == LockType.Disabled)
			{
				KickStarter.stateHandler.SetInputSystem (false);
			}

			if (interactionLock == LockType.Enabled)
			{
				KickStarter.stateHandler.SetInteractionSystem (true);
			}
			else if (interactionLock == LockType.Disabled)
			{
				KickStarter.stateHandler.SetInteractionSystem (false);
			}

			if (menuLock == LockType.Enabled)
			{
				KickStarter.stateHandler.SetMenuSystem (true);
			}
			else if (menuLock == LockType.Disabled)
			{
				KickStarter.stateHandler.SetMenuSystem (false);
			}

			if (movementLock == LockType.Enabled)
			{
				KickStarter.stateHandler.SetMovementSystem (true);
			}
			else if (movementLock == LockType.Disabled)
			{
				KickStarter.stateHandler.SetMovementSystem (false);
			}

			if (cameraLock == LockType.Enabled)
			{
				KickStarter.stateHandler.SetCameraSystem (true);
			}
			else if (cameraLock == LockType.Disabled)
			{
				KickStarter.stateHandler.SetCameraSystem (false);
			}

			if (triggerLock == LockType.Enabled)
			{
				KickStarter.stateHandler.SetTriggerSystem (true);
			}
			else if (triggerLock == LockType.Disabled)
			{
				KickStarter.stateHandler.SetTriggerSystem (false);
			}

			if (playerLock == LockType.Enabled)
			{
				KickStarter.stateHandler.SetPlayerSystem (true);
			}
			else if (playerLock == LockType.Disabled)
			{
				KickStarter.stateHandler.SetPlayerSystem (false);
			}

			if (saveLock == LockType.Disabled)
			{
				KickStarter.playerMenus.SetManualSaveLock (true);
			}
			else if (saveLock == LockType.Enabled)
			{
				KickStarter.playerMenus.SetManualSaveLock (false);
			}

			if (keyboardGameplayMenusLock == LockType.Disabled)
			{
				KickStarter.playerInput.canKeyboardControlMenusDuringGameplay = false;
			}
			else if (keyboardGameplayMenusLock == LockType.Enabled)
			{
				KickStarter.playerInput.canKeyboardControlMenusDuringGameplay = true;
			}

			return 0f;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI ()
		{
			changeMovementMethod = EditorGUILayout.BeginToggleGroup ("Change movement method?", changeMovementMethod);
			newMovementMethod = (MovementMethod) EditorGUILayout.EnumPopup ("Movement method:", newMovementMethod);
			EditorGUILayout.EndToggleGroup ();

			EditorGUILayout.Space ();

			cursorLock = (LockType) EditorGUILayout.EnumPopup ("Cursor:", cursorLock);
			inputLock = (LockType) EditorGUILayout.EnumPopup ("Input:", inputLock);
			interactionLock = (LockType) EditorGUILayout.EnumPopup ("Interactions:", interactionLock);
			menuLock = (LockType) EditorGUILayout.EnumPopup ("Menus:", menuLock);
			movementLock = (LockType) EditorGUILayout.EnumPopup ("Movement:", movementLock);
			cameraLock = (LockType) EditorGUILayout.EnumPopup ("Camera:", cameraLock);
			triggerLock = (LockType) EditorGUILayout.EnumPopup ("Triggers:", triggerLock);
			playerLock = (LockType) EditorGUILayout.EnumPopup ("Player:", playerLock);
			saveLock = (LockType) EditorGUILayout.EnumPopup ("Saving:", saveLock);

			if (AdvGame.GetReferences () != null && AdvGame.GetReferences ().settingsManager != null && AdvGame.GetReferences ().settingsManager.inputMethod == InputMethod.KeyboardOrController)
			{
				keyboardGameplayMenusLock = (LockType) EditorGUILayout.EnumPopup ("Control in-game Menus:", keyboardGameplayMenusLock);
			}

			AfterRunningOption ();
		}
		
		#endif
		
	}

}