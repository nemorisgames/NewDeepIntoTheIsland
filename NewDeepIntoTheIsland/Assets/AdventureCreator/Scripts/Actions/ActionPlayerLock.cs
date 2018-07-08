/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionPlayerLock.cs"
 * 
 *	This action constrains the player in various ways (movement, saving etc)
 *	In Direct control mode, the player can be assigned a path,
 *	and will only be able to move along that path during gameplay.
 * 
 */

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class ActionPlayerLock : Action
	{
		
		public LockType doUpLock = LockType.NoChange;
		public LockType doDownLock = LockType.NoChange;
		public LockType doLeftLock = LockType.NoChange;
		public LockType doRightLock = LockType.NoChange;
		
		public PlayerMoveLock doRunLock = PlayerMoveLock.NoChange;
		public LockType doJumpLock = LockType.NoChange;
		public LockType freeAimLock = LockType.NoChange;
		public LockType cursorState = LockType.NoChange;
		public LockType doGravityLock = LockType.NoChange;
		public LockType doHotspotHeadTurnLock = LockType.NoChange;
		public Paths movePath;

		
		public ActionPlayerLock ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Player;
			title = "Constrain";
			description = "Locks and unlocks various aspects of Player control. When using Direct or First Person control, can also be used to specify a Path object to restrict movement to.";
		}
		
		
		override public float Run ()
		{
			Player player = KickStarter.player;

			if (KickStarter.playerInput)
			{
				if (IsSingleLockMovement ())
				{
					doLeftLock = doUpLock;
					doRightLock = doUpLock;
					doDownLock = doUpLock;
				}

				if (doUpLock == LockType.Disabled)
				{
					KickStarter.playerInput.SetUpLock (true);
				}
				else if (doUpLock == LockType.Enabled)
				{
					KickStarter.playerInput.SetUpLock (false);
				}
		
				if (doDownLock == LockType.Disabled)
				{
					KickStarter.playerInput.SetDownLock (true);
				}
				else if (doDownLock == LockType.Enabled)
				{
					KickStarter.playerInput.SetDownLock (false);
				}
				
				if (doLeftLock == LockType.Disabled)
				{
					KickStarter.playerInput.SetLeftLock (true);
				}
				else if (doLeftLock == LockType.Enabled)
				{
					KickStarter.playerInput.SetLeftLock (false);
				}
		
				if (doRightLock == LockType.Disabled)
				{
					KickStarter.playerInput.SetRightLock (true);
				}
				else if (doRightLock == LockType.Enabled)
				{
					KickStarter.playerInput.SetRightLock (false);
				}

				if (KickStarter.settingsManager.movementMethod != MovementMethod.PointAndClick)
				{
					if (doJumpLock == LockType.Disabled)
					{
						KickStarter.playerInput.SetJumpLock (true);
					}
					else if (doJumpLock == LockType.Enabled)
					{
						KickStarter.playerInput.SetJumpLock (false);
					}
				}

				if (IsInFirstPerson ())
				{
					if (freeAimLock == LockType.Disabled)
					{
						KickStarter.playerInput.SetFreeAimLock (true);
					}
					else if (freeAimLock == LockType.Enabled)
					{
						KickStarter.playerInput.SetFreeAimLock (false);
					}
				}

				if (cursorState == LockType.Disabled)
				{
					KickStarter.playerInput.SetInGameCursorState (false);
				}
				else if (cursorState == LockType.Enabled)
				{
					KickStarter.playerInput.SetInGameCursorState (true);
				}

				if (doRunLock != PlayerMoveLock.NoChange)
				{
					KickStarter.playerInput.runLock = doRunLock;
				}
			}
			
			if (player)
			{
				if (movePath)
				{
					player.SetLockedPath (movePath);
					player.SetMoveDirectionAsForward ();
				}
				else if (player.GetPath ())
				{
					if (player.IsPathfinding () && !ChangingMovementLock () && (doRunLock == PlayerMoveLock.AlwaysWalk || doRunLock == PlayerMoveLock.AlwaysRun))
					{
						if (doRunLock == PlayerMoveLock.AlwaysRun)
						{
							player.GetPath ().pathSpeed = PathSpeed.Run;
							player.isRunning = true;
						}
						else if (doRunLock == PlayerMoveLock.AlwaysWalk)
						{
							player.GetPath ().pathSpeed = PathSpeed.Walk;
							player.isRunning = false;
						}
					}
					else
					{
						player.EndPath ();
					}
				}

				if (doGravityLock == LockType.Enabled)
				{
					player.ignoreGravity = false;
				}
				else if (doGravityLock == LockType.Disabled)
				{
					player.ignoreGravity = true;
				}

				if (AllowHeadTurning ())
				{
					if (doHotspotHeadTurnLock == LockType.Disabled)
					{
						player.SetHotspotHeadTurnLock (true);
					}
					else if (doHotspotHeadTurnLock == LockType.Enabled)
					{
						player.SetHotspotHeadTurnLock (false);
					}
				}
			}
			
			return 0f;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI ()
		{
			if (IsSingleLockMovement ())
			{
				doUpLock = (LockType) EditorGUILayout.EnumPopup ("Movement:", doUpLock);
			}
			else
			{
				doUpLock = (LockType) EditorGUILayout.EnumPopup ("Up movement:", doUpLock);
				doDownLock = (LockType) EditorGUILayout.EnumPopup ("Down movement:", doDownLock);
				doLeftLock = (LockType) EditorGUILayout.EnumPopup ("Left movement:", doLeftLock);
				doRightLock = (LockType) EditorGUILayout.EnumPopup ("Right movement:", doRightLock);
			}

			if (AdvGame.GetReferences () != null && AdvGame.GetReferences ().settingsManager != null && KickStarter.settingsManager.movementMethod != MovementMethod.PointAndClick)
			{
				doJumpLock = (LockType) EditorGUILayout.EnumPopup ("Jumping:", doJumpLock);
			}

			if (IsInFirstPerson ())
			{
				freeAimLock = (LockType) EditorGUILayout.EnumPopup ("Free-aiming:", freeAimLock);
			}

			cursorState = (LockType) EditorGUILayout.EnumPopup ("Cursor lock:", cursorState);
			doRunLock = (PlayerMoveLock) EditorGUILayout.EnumPopup ("Walk / run:", doRunLock);
			doGravityLock = (LockType) EditorGUILayout.EnumPopup ("Affected by gravity?", doGravityLock);
			movePath = (Paths) EditorGUILayout.ObjectField ("Move path:", movePath, typeof (Paths), true);

			if (AllowHeadTurning ())
			{
				doHotspotHeadTurnLock = (LockType) EditorGUILayout.EnumPopup ("Hotspot head-turning?", doHotspotHeadTurnLock);
			}
			
			AfterRunningOption ();
		}
		
		#endif


		private bool AllowHeadTurning ()
		{
			if (SceneSettings.CameraPerspective != CameraPerspective.TwoD && AdvGame.GetReferences ().settingsManager.playerFacesHotspots)
			{
				return true;
			}
			return false;
		}


		private bool IsSingleLockMovement ()
		{
			if (AdvGame.GetReferences ().settingsManager)
			{
				SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;
				if (settingsManager.movementMethod == MovementMethod.PointAndClick || settingsManager.movementMethod == MovementMethod.Drag || settingsManager.movementMethod == MovementMethod.StraightToCursor)
				{
					return true;
				}
			}
			return false;
		}


		private bool ChangingMovementLock ()
		{
			if (doUpLock != LockType.NoChange)
			{
				return true;
			}

			if (!IsSingleLockMovement ())
			{
				if (doDownLock != LockType.NoChange || doLeftLock != LockType.NoChange || doRightLock != LockType.NoChange)
				{
					return true;
				}
			}
			return false;
		}


		private bool IsInFirstPerson ()
		{
			if (AdvGame.GetReferences ().settingsManager && AdvGame.GetReferences ().settingsManager.IsInFirstPerson ())
			{
				return true;
			}
			return false;
		}

	}

}