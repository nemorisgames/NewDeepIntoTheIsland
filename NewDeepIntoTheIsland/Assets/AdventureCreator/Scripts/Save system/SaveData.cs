/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"SaveData.cs"
 * 
 *	This script contains all the non-scene-specific data we wish to save.
 * 
 */

using System.Collections.Generic;

namespace AC
{

	/**
	 * A data container for all global data that gets stored in save games.
	 */
	[System.Serializable]
	public class SaveData
	{

		/** An instance of the MainData class */
		public MainData mainData;
		/** Instances of PlayerData for each of the game's Players */
		public List<PlayerData> playerData = new List<PlayerData>();

		/**
		 * The default Constructor.
		 */
		public SaveData () { }

	}


	/**
	 * A data container for all non-player global data that gets stored in save games.
	 * A single instance of this class is stored in SaveData by SaveSystem.
	 */
	[System.Serializable]
	public struct MainData
	{

		/** The ID number of the currently-active Player */
		public int currentPlayerID;
		/** The game's current timeScale */
		public float timeScale;

		/** The current values of all Global Variables */
		public string runtimeVariablesData;
		/** All user-generated CustomToken variables */
		public string customTokenData;

		/** The locked state of all Menu variables */
		public string menuLockData;
		/** The visibility state of all Menu instances */
		public string menuVisibilityData;
		/** The visibility state of all MenuElement instances */
		public string menuElementVisibilityData;
		/** The page data of all MenuJournal instances */
		public string menuJournalData;

		/** The Constant ID number of the currently-active ArrowPrompt */
		public int activeArrows;
		/** The Constant ID number of the currently-active Conversation */
		public int activeConversation;

		/** The ID number of the currently-selected InvItem */
		public int selectedInventoryID;
		/** True if the currently-selected InvItem is in "give" mode, as opposed to "use" */
		public bool isGivingItem;

		/** True if the cursor system, PlayerCursor, is disabled */
		public bool cursorIsOff;
		/** True if the input system, PlayerInput, is disabled */
		public bool inputIsOff;
		/** True if the interaction system, PlayerInteraction, is disabled */
		public bool interactionIsOff;
		/** True if the menu system, PlayerMenus, is disabled */
		public bool menuIsOff;
		/** True if the movement system, PlayerMovement, is disabled */
		public bool movementIsOff;
		/** True if the camera system is disabled */
		public bool cameraIsOff;
		/** True if Triggers are disabled */
		public bool triggerIsOff;
		/** True if Players are disabled */
		public bool playerIsOff;
		/** True if keyboard/controller can be used to control menus during gameplay */
		public bool canKeyboardControlMenusDuringGameplay;
		/** The state of the cursor toggle (1 = on, 2 = off) */
		public int toggleCursorState;

		/** The IDs and loop states of all queued music tracks, including the one currently-playing */
		public string musicQueueData;
		/** The IDs and loop states of the last set of queued music tracks */
		public string lastMusicQueueData;
		/** The time position of the current music track */
		public int musicTimeSamples;
		/** The time position of the last-played music track */
		public int lastMusicTimeSamples;
		/** The IDs and time positions of all tracks that have been played before */
		public string oldMusicTimeSamples;

		/** The IDs and loop states of all queued ambience tracks, including the one currently-playing */
		public string ambienceQueueData;
		/** The IDs and loop states of the last set of queued ambience tracks */
		public string lastAmbienceQueueData;
		/** The time position of the current ambience track */
		public int ambienceTimeSamples;
		/** The time position of the last-played ambience track */
		public int lastAmbienceTimeSamples;
		/** The IDs and time positions of all ambience tracks that have been played before */
		public string oldAmbienceTimeSamples;

		/** The currently-set AC_MovementMethod enum, converted to an integer */
		public int movementMethod;
		/** Data regarding paused and skipping ActionList assets */
		public string activeAssetLists;
		/** Data regarding active inputs */
		public string activeInputsData;

	}


	/**
	 * A data container for saving the state of a Player.
	 * Each Player in a game has its own instance of this class stored in SaveData by SaveSystem.
	 */
	[System.Serializable]
	public struct PlayerData
	{

		/** The ID number of the Player that this data references */
		public int playerID;
		/** The current scene number */
		public int currentScene;
		/** The last-visited scene number */
		public int previousScene;
		/** The current scene name */
		public string currentSceneName;
		/** The last-visited scene name */
		public string previousSceneName;
		/** The details any sub-scenes that are also open */
		public string openSubScenes;

		/** The Player's X position */
		public float playerLocX;
		/** The Player's Y position */
		public float playerLocY;
		/** The Player's Z position */
		public float playerLocZ;
		/** The Player'sY rotation */
		public float playerRotY;

		/** The walk speed */
		public float playerWalkSpeed;
		/** The run speed */
		public float playerRunSpeed;

		/** The idle animation */
		public string playerIdleAnim;
		/** The walk animation */
		public string playerWalkAnim;
		/** The talk animation */
		public string playerTalkAnim;
		/** The run animation */
		public string playerRunAnim;

		/** A unique identifier for the walk sound AudioClip */
		public string playerWalkSound;
		/** A unique identifier for the run sound AudioClip */
		public string playerRunSound;
		/** A unique identified for the portrait graphic */
		public string playerPortraitGraphic;
		/** The Player's display name */
		public string playerSpeechLabel;
		/** The ID number that references the Player's name, as generated by the Speech Manager */
		public int playerDisplayLineID;

		/** The target node number of the current Path */
		public int playerTargetNode;
		/** The previous node number of the current Path */
		public int playerPrevNode;
		/** The positions of each node in a pathfinding-generated Path */
		public string playerPathData;
		/** True if the Player is currently running */
		public bool playerIsRunning;
		/** True if the Player is locked along a Path */
		public bool playerLockedPath;
		/** The Constant ID number of the Player's current Path */
		public int playerActivePath;
		/** True if the Player's current Path affects the Y position */
		public bool playerPathAffectY;

		/** The target node number of the Player's last-used Path */
		public int lastPlayerTargetNode;
		/** The previous node number of the Player's last-used Path */
		public int lastPlayerPrevNode;
		/** The Constant ID number of the Player's last-used Path */
		public int lastPlayerActivePath;

		/** True if the Player cannot move up */
		public bool playerUpLock;
		/** True if the Player cannot move down */
		public bool playerDownLock;
		/** True if the Player cannot move left */
		public bool playerLeftlock;
		/** True if the Player cannot move right */
		public bool playerRightLock;
		/** True if the Player cannot run */
		public int playerRunLock;
		/** True if free-aiming is prevented */
		public bool playerFreeAimLock;
		/** True if the Player's Rigidbody is unaffected by gravity */
		public bool playerIgnoreGravity;

		/** True if a sprite-based Player is locked to face a particular direction */
		public bool playerLockDirection;
		/** The direction that a sprite-based Player is currently facing */
		public string playerSpriteDirection;
		/** True if a sprite-based Player has its scale locked */
		public bool playerLockScale;
		/** The scale of a sprite-based Player */
		public float playerSpriteScale;
		/** True if a sprite-based Player has its sorting locked */
		public bool playerLockSorting;
		/** The sorting order of a sprite-based Player */
		public int playerSortingOrder;
		/** The order in layer of a sprite-based Player */
		public string playerSortingLayer;

		/** What Inventory Items (see: InvItem) the player is currently carrying */
		public string inventoryData;

		/** True if the Player's head is facing a Hotspot */
		public bool playerLockHotspotHeadTurning;
		/** True if the Player's head is facing a particular object */
		public bool isHeadTurning;
		/** The ConstantID number of the head target Transform */
		public int headTargetID;
		/** The Player's head target's X position (offset) */
		public float headTargetX;
		/** The Player's head target's Y position (offset) */
		public float headTargetY;
		/** The Player's head target's Z position (offset) */
		public float headTargetZ;

		/** The Constant ID number of the active _Camera */
		public int gameCamera;
		/** The Constant ID number of the last active _Camera during gameplay */
		public int lastNavCamera;
		/** The Constant ID number of the last active-but-one _Camera during gameplay */
		public int lastNavCamera2;

		/** The MainCamera's X position */
		public float mainCameraLocX;
		/** The MainCamera's Y position */
		public float mainCameraLocY;
		/** The MainCamera's Z position */
		public float mainCameraLocZ;
		/** The MainCamera's X rotation */
		public float mainCameraRotX;
		/** The MainCamera's Y rotation */
		public float mainCameraRotY;
		/** The MainCamera's Z rotation */
		public float mainCameraRotZ;

		/** True if split-screen is currently active */
		public bool isSplitScreen;
		/** True if the gameplay is performed in the top (or left) side during split-screen */
		public bool isTopLeftSplit;
		/** True if split-screen is arranged vertically  */
		public bool splitIsVertical;
		/** The Constant ID number of the split-screen camera that gameplay is not performed in */
		public int splitCameraID;
		/** During split-screen, the proportion of the screen that the gameplay camera takes up */
		public float splitAmountMain;
		/** During split-screen, the proportion of the screen that the non-gameplay camera take up */
		public float splitAmountOther;
		/** The intensity of the current camera shake */
		public float shakeIntensity;
		/** The duration, in seconds, of the current camera shake */
		public float shakeDuration;
		/** The int-converted value of CamersShakeEffect */
		public int shakeEffect;

		/** True if the NPC has a FollowSortingMap component that follows the scene's default SortingMap */
		public bool followSortingMap;
		/** The ConstantID number of the SortingMap that the NPC's FollowSortingMap follows, if not the scene's default */
		public int customSortingMapID;


		/**
		 * <summary>Copies values from another PlayerData instance onto itself.</summary>
		 * <param name = "originalData">The PlayerData to copy</param>
		 */
		public void CopyData (PlayerData originalData)
		{
			playerID = originalData.playerID;
			currentScene = originalData.currentScene;
			previousScene = originalData.previousScene;
			currentSceneName = originalData.currentSceneName;
			previousSceneName = originalData.previousSceneName;
			openSubScenes = originalData.openSubScenes;
			
			playerLocX = originalData.playerLocX;
			playerLocY = originalData.playerLocY;
			playerLocZ = originalData.playerLocZ;
			playerRotY = originalData.playerRotY;
			
			playerWalkSpeed = originalData.playerWalkSpeed;
			playerRunSpeed = originalData.playerRunSpeed;
			
			playerIdleAnim = originalData.playerIdleAnim;
			playerWalkAnim = originalData.playerWalkAnim;
			playerTalkAnim = originalData.playerTalkAnim;
			playerRunAnim = originalData.playerRunAnim;
			
			playerWalkSound = originalData.playerWalkSound;
			playerRunSound = originalData.playerRunSound;
			playerPortraitGraphic = originalData.playerPortraitGraphic;
			playerSpeechLabel = originalData.playerSpeechLabel;
			playerDisplayLineID = originalData.playerDisplayLineID;

			playerTargetNode = originalData.playerTargetNode;
			playerPrevNode = originalData.playerPrevNode;
			playerPathData = originalData.playerPathData;
			playerIsRunning = originalData.playerIsRunning;
			playerLockedPath = originalData.playerLockedPath;
			playerActivePath = originalData.playerActivePath;
			playerPathAffectY = originalData.playerPathAffectY;
			
			lastPlayerTargetNode = originalData.lastPlayerTargetNode;
			lastPlayerPrevNode = originalData.lastPlayerPrevNode;
			lastPlayerActivePath = originalData.lastPlayerActivePath;
			
			playerUpLock = originalData.playerUpLock;
			playerDownLock = originalData.playerDownLock;
			playerLeftlock = originalData.playerLeftlock;
			playerRightLock = originalData.playerRightLock;
			playerRunLock = originalData.playerRunLock;
			playerFreeAimLock = originalData.playerFreeAimLock;
			playerIgnoreGravity = originalData.playerIgnoreGravity;
			
			playerLockDirection = originalData.playerLockDirection;
			playerSpriteDirection = originalData.playerSpriteDirection;
			playerLockScale = originalData.playerLockScale;
			playerSpriteScale = originalData.playerSpriteScale;
			playerLockSorting = originalData.playerLockSorting;
			playerSortingOrder = originalData.playerSortingOrder;
			playerSortingLayer = originalData.playerSortingLayer;
			
			inventoryData = originalData.inventoryData;
			
			isHeadTurning = originalData.isHeadTurning;
			headTargetID = originalData.headTargetID;
			headTargetX = originalData.headTargetX;
			headTargetY = originalData.headTargetY;
			headTargetZ = originalData.headTargetZ;
			
			gameCamera = originalData.gameCamera;
			lastNavCamera = originalData.lastNavCamera;
			lastNavCamera2 = originalData.lastNavCamera2;
			mainCameraLocX = originalData.mainCameraLocX;
			mainCameraLocY = originalData.mainCameraLocY;
			mainCameraLocZ = originalData.mainCameraLocZ;
			
			mainCameraRotX = originalData.mainCameraRotX;
			mainCameraRotY = originalData.mainCameraRotY;
			mainCameraRotZ = originalData.mainCameraRotZ;
			
			isSplitScreen = originalData.isSplitScreen;
			isTopLeftSplit = originalData.isTopLeftSplit;
			splitIsVertical = originalData.splitIsVertical;
			splitCameraID = originalData.splitCameraID;
			splitAmountMain = originalData.splitAmountMain;
			splitAmountOther = originalData.splitAmountOther;
			shakeIntensity = originalData.shakeIntensity;
			shakeDuration = originalData.shakeDuration;
			shakeEffect = originalData.shakeEffect;

			followSortingMap = originalData.followSortingMap;
			customSortingMapID = originalData.customSortingMapID;
		}

	}

}