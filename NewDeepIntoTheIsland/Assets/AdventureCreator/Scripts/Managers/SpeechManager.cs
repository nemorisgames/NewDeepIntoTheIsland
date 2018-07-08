/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"SpeechManager.cs"
 * 
 *	This script handles the "Speech" tab of the main wizard.
 *	It is used to auto-number lines for audio files, and handle translations.
 * 
 */
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	/**
	 * Handles the "Speech" tab of the Game Editor window.
	 * All translations for a game's text are stored here, as are the settings that control how speech is handled in-game.
	 */
	[System.Serializable]
	public class SpeechManager : ScriptableObject
	{

		/** If True, then speech text will scroll when displayed */
		public bool scrollSubtitles = true;
		/** If True, then narration text will scroll when displayed */
		public bool scrollNarration = false;
		/** The speed of scrolling text */
		public float textScrollSpeed = 50;
		/** The AudioClip to play when scrolling speech text */
		public AudioClip textScrollCLip = null;
		/** The AudioClip to play when scrolling narration text */
		public AudioClip narrationTextScrollCLip = null;
		/** If True, the textScrollClip audio will be played with every character addition to the subtitle text, as opposed to waiting for the previous audio to end */
		public bool playScrollAudioEveryCharacter = true;

		/** If True, then speech text will remain on the screen until the player skips it */
		public bool displayForever = false;
		/** If true, then narration text will remain on the screen until the player skips it.  This only has an effect if displayForever = false */
		public bool displayNarrationForever = false;
		/** If True, and displayForever = True, then a speaking character will play their talking animation for the whole duration that their speech text is alive */
		public bool playAnimationForever = true;
		/** If True, and subtitles can be skipped, then skipping can be achieved with mouse-clicks, as well as by invoking the SkipSpeech input */
		public bool canSkipWithMouseClicks = true;
		/** The minimum time, in seconds, that a speech line will be displayed (unless an AudioClip is setting it's length) */
		public float minimumDisplayTime = 1f;
		/** The time that speech text will be displayed, divided by the number of characters in the text, if displayForever = False */
		public float screenTimeFactor = 0.1f;
		/** If True, then speech text during a cutscene can be skipped by the player left-clicking */
		public bool allowSpeechSkipping = false;
		/** If True, then speech text during gameplay can be skipped by the player left-clicking */
		public bool allowGameplaySpeechSkipping = false;
		/** The minimum time that speech text must be displayed before it can be skipped, if allowSpeechSkipping = True */
		public float skipThresholdTime = 0f;
		/** If True, then left-clicking will complete any scrolling speech text */
		public bool endScrollBeforeSkip = false;
		/** If True, and text is scrolling, then the display time upon completion will be influenced by the length of the speech text */
		public bool scrollingTextFactorsLength = false;

		/** If True, then speech audio files will play when characters speak */
		public bool searchAudioFiles = true;
		/** If True, then the audio files associated with speech text will be named automatically according to their ID number */
		public bool autoNameSpeechFiles = true;
		/** The subdirectory within Resources that speech files are pulled from, if autoNameSpeechFiles = True */
		public string autoSpeechFolder = "Speech";
		/** The subdirectory within Resources that lipsync files are pulled from, if autoNameSpeechFiles = True */
		public string autoLipsyncFolder = "Lipsync";
		/** If True, then speech text will always display if no relevant audio file is found - even if Subtitles are off in the Options menu */
		public bool forceSubtitles = true;
		/** If True, then each translation will have its own set of speech audio files */
		public bool translateAudio = true;
		/** If True, then translations that don't have speech audio files will use the audio files from the game's original language */
		public bool fallbackAudio = false;
		/** If True, then the text stored in the speech buffer (in MenuLabel) will not be cleared when no speech text is active */
		public bool keepTextInBuffer = false;
		/** If True, then background speech audio will end if foreground speech audio begins to play */
		public bool relegateBackgroundSpeechAudio = false;
		/** If True, then speech audio spoken by the player will expect the audio filenames to be named after the player's prefab, rather than just "Player" */
		public bool usePlayerRealName = false;
		/** If True, usePlayerRealName = True, autoNameSpeechFiles = True, and playerSwitching = PlayerSwitching.Allow in SettingsManager, then speech lines marked as Player lines will have audio entries for each player prefab. */
		public bool separateSharedPlayerAudio = false;

		/** If True, then speech audio files will need to be placed in subfolders named after the character who speaks */
		public bool placeAudioInSubfolders = false;
		/** If True, then a speech line will be split by carriage returns into separate speech lines */
		public bool separateLines = false;
		/** The delay between carriage return-separated speech lines, if separateLines = True */
		public float separateLinePause = 1f;
		/** If True, then a character's expression will be reset with each new speech line */
		public bool resetExpressionsEachLine = true;

		/** All SpeechLines generated to store translations and audio filename references */
		public List<SpeechLine> lines = new List<SpeechLine> ();
		/** The names of the game's languages. The first is always "Original". */
		public List<string> languages = new List<string> ();
		/** A List of whether or not each language in the game reads right-to-left (Arabic / Hebrew-style) */
		public List<bool> languageIsRightToLeft = new List<bool> ();
		/** If True, then the game's original text cannot be displayed in-game, and only translations will be available */
		public bool ignoreOriginalText = false;
	
		/** The factor by which to reduce SFX audio when speech plays */
		public float sfxDucking = 0f;
		/** The factor by which to reduce music audio when speech plays */
		public float musicDucking = 0f;

		/** The game's lip-syncing method (Off, FromSpeechText, ReadPamelaFile, ReadSapiFile, ReadPapagayoFile, FaceFX, Salsa2D) */
		public LipSyncMode lipSyncMode = LipSyncMode.Off;
		/** What lip-syncing actually affects (Portrait, PortraitAndGameObject, GameObjectTexture) */
		public LipSyncOutput lipSyncOutput = LipSyncOutput.Portrait;
		/** The phoneme bins used to separate phonemes into animation frames */
		public List<string> phonemes = new List<string> ();
		/** The speed at which to process lip-sync data */
		public float lipSyncSpeed = 1f;

		public int maxID = -1;

		/** An override delegate for the GetAutoAssetPathAndName function, used to retrieve the full filepath of an auto-assigned speech audio or lipsync file */
		public GetAutoAssetPathAndNameDelegate GetAutoAssetPathAndNameOverride;
		/** A delegate template for overriding the GetAutoAssetPathAndName function */
		public delegate string GetAutoAssetPathAndNameDelegate (SpeechLine speechLine, string language, bool forLipSync);


		#if UNITY_EDITOR

		/** An array of all scene names in the Build settings */
		public string[] sceneFiles;
		/** The current SpeechLine selected to reveal its properties */
		public int activeLineID = -1;
		/** If True, then speech lines that are exactly the same will share the same ID number */
		public bool mergeMatchingSpeechIDs = false;

		/** If True, then 'Dialogue: Play speech' Actions can be assigned a SpeechTag, or label, to use when exporting script sheets */
		public bool useSpeechTags = false;
		/** A List of the available SpeechTags */
		public List<SpeechTag> speechTags = new List<SpeechTag>();
		
		private List<string> sceneNames = new List<string>();
		private List<SpeechLine> tempLines = new List<SpeechLine>();
		private string sceneLabel;

		private List<ActionListAsset> allActionListAssets;
		private string textFilter;
		private FilterSpeechLine filterSpeechLine = FilterSpeechLine.Text;
		private GameTextSorting gameTextSorting = GameTextSorting.None;
		private GameTextSorting lastGameTextSorting = GameTextSorting.None;
		private List<ActionListAsset> checkedAssets = new List<ActionListAsset>();
		private AC_TextType typeFilter = AC_TextType.Speech;
		private int tagFilter;
		private int sceneFilter;
		private int sideLanguage;

		private AudioFilter audioFilter;
		private enum AudioFilter { None, OnlyWithAudio, OnlyWithoutAudio };

		private enum TransferComment { NotAsked, Yes, No };
		private TransferComment transferComment;

		private bool showSubtitles = true;
		private bool showAudio = true;
		private bool showLipSyncing = true;
		private bool showTranslations = true;
		private bool showGameText = true;
		private bool showScrollingAudio = true;
		private int minOrderValue;


		/**
		 * Shows the GUI.
		 */
		public void ShowGUI ()
		{
			#if UNITY_WEBPLAYER
			EditorGUILayout.HelpBox ("Exporting game text cannot be performed in WebPlayer mode - please switch platform to do so.", MessageType.Warning);
			GUILayout.Space (10);
			#endif

			EditorGUILayout.BeginVertical (CustomStyles.thinBox);
			showSubtitles = CustomGUILayout.ToggleHeader (showSubtitles, "Subtitles");
			if (showSubtitles)
			{
				separateLines = CustomGUILayout.ToggleLeft ("Treat carriage returns as separate speech lines?", separateLines, "AC.KickStarter.speechManager.separateLines");
				if (separateLines)
				{
					separateLinePause = CustomGUILayout.Slider ("Split line delay (s):", separateLinePause, 0f, 1f, "AC.KickStarter.speechManager.separateLinePause");
				}
				scrollSubtitles = CustomGUILayout.ToggleLeft ("Scroll speech text?", scrollSubtitles, "AC.KickStarter.speechManager.scrollSubtitles");
				scrollNarration = CustomGUILayout.ToggleLeft ("Scroll narration text?", scrollNarration, "AC.KickStarter.speechManager.scrollNarration");
				if (scrollSubtitles || scrollNarration)
				{
					textScrollSpeed = CustomGUILayout.FloatField ("Text scroll speed:", textScrollSpeed, "AC.KickStarter.speechManager.textScrollSpeed");
				}
				
				displayForever = CustomGUILayout.ToggleLeft ("Display subtitles forever until user skips it?", displayForever, "AC.KickStarter.speechManager.displayForever");
				if (displayForever)
				{
					playAnimationForever = CustomGUILayout.ToggleLeft ("Play talking animations forever until user skips it?", playAnimationForever, "AC.KickStarter.speechManager.playAnimationForever");
				}
				else
				{
					displayNarrationForever = CustomGUILayout.ToggleLeft ("Display narration forever until user skips it?", displayNarrationForever, "AC.KickStarter.speechManager.displayNarrationForever");
				}

				if (displayForever && displayNarrationForever) {} else
				{
					minimumDisplayTime = CustomGUILayout.FloatField ("Minimum display time (s):", minimumDisplayTime, "AC.KickStarter.speechManager.minimumDisplayTime");
					screenTimeFactor = CustomGUILayout.FloatField ("Display time factor:", screenTimeFactor, "AC.KickStarter.speechManager.screenTimeFactor");
					allowSpeechSkipping = CustomGUILayout.ToggleLeft ("Subtitles can be skipped?", allowSpeechSkipping, "AC.KickStarter.speechManager.allowSpeechSkipping");

					if (screenTimeFactor > 0f)
					{
						if (scrollSubtitles || scrollNarration)
						{
							scrollingTextFactorsLength = CustomGUILayout.ToggleLeft ("Text length influences display time?", scrollingTextFactorsLength, "AC.KickStarter.speechManager.scrollingTextFactorsLength");
							if (scrollingTextFactorsLength)
							{
								EditorGUILayout.HelpBox ("This option will be ignored if speech has accompanying audio.", MessageType.Info);
							}
						}
					}
				}

				if (displayForever || displayNarrationForever || allowSpeechSkipping)
				{
					canSkipWithMouseClicks = CustomGUILayout.ToggleLeft ("Can skip with mouse clicks?", canSkipWithMouseClicks, "AC.KickStarter.speechManager.canSkipWithMouseClicks");
					skipThresholdTime = CustomGUILayout.FloatField ("Time before can skip (s):", skipThresholdTime, "AC.KickStarter.speechManager.skipThresholdTime");

					if (scrollSubtitles || scrollNarration)
					{
						endScrollBeforeSkip = CustomGUILayout.ToggleLeft ("Skipping speech first displays currently-scrolling text?", endScrollBeforeSkip, "AC.KickStarter.speechManager.endScrollBeforeSkip");
					}
					allowGameplaySpeechSkipping = CustomGUILayout.ToggleLeft ("Subtitles during gameplay can also be skipped?", allowGameplaySpeechSkipping, "AC.KickStarter.speechManager.allowGameplaySpeechSkipping");
				}
				
				keepTextInBuffer = CustomGUILayout.ToggleLeft ("Retain subtitle text buffer once line has ended?", keepTextInBuffer, "AC.KickStarter.speechManager.keepTextInBuffer");
				resetExpressionsEachLine = CustomGUILayout.ToggleLeft ("Reset character expression with each line?", resetExpressionsEachLine, "AC.KickStarter.speechManager.resetExpressionsEachLine");

				if (GUILayout.Button ("Edit speech tags"))
				{
					SpeechTagsWindow.Init ();
				}

				if (scrollSubtitles || scrollNarration)
				{
					EditorGUILayout.Space ();
					showScrollingAudio = CustomGUILayout.ToggleHeader (showScrollingAudio, "Subtitle-scrolling audio");
					if (showScrollingAudio)
					{
						if (scrollSubtitles)
						{
							textScrollCLip = (AudioClip) CustomGUILayout.ObjectField <AudioClip> ("Speech text scroll audio:", textScrollCLip, false, "AC.KickStarter.speechManager.textScrollClip");
						}
						if (scrollNarration)
						{
							narrationTextScrollCLip = (AudioClip) CustomGUILayout.ObjectField <AudioClip> ("Narration text scroll audio:", narrationTextScrollCLip, false, "AC.KickStarter.speechManager.narrationTextScrollCLip");
						}
						playScrollAudioEveryCharacter = CustomGUILayout.Toggle ("Play audio on every letter?", playScrollAudioEveryCharacter, "AC.KickStarter.speechManager.playScrollAudioEveryCharacter");
					}
				}
			}
			EditorGUILayout.EndVertical ();
			EditorGUILayout.Space ();

			EditorGUILayout.BeginVertical (CustomStyles.thinBox);
			showAudio = CustomGUILayout.ToggleHeader (showAudio, "Speech audio");
			if (showAudio)
			{
				forceSubtitles = CustomGUILayout.ToggleLeft ("Force subtitles to display when no speech audio is found?", forceSubtitles, "AC.KickStarter.speechManager.forceSubtitles");
				searchAudioFiles = CustomGUILayout.ToggleLeft ("Auto-play speech audio files?", searchAudioFiles, "AC.KickStarter.speechManager.searchAudioFiles");
				autoNameSpeechFiles = CustomGUILayout.ToggleLeft ("Auto-name speech audio files?", autoNameSpeechFiles, "AC.KickStarter.speechManager.autoNameSpeechFiles");

				if (autoNameSpeechFiles)
				{
					autoSpeechFolder = CustomGUILayout.TextField ("Speech audio directory:", autoSpeechFolder, "AC.KickStarter.speechManager.autoSpeechFolder");
				}

				#if UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL
				if (!autoNameSpeechFiles)
				{
					EditorGUILayout.HelpBox ("Manually-assigning speech files takes memory - consider auto-naming on this platform.", MessageType.Warning);
				}
				#endif

				translateAudio = CustomGUILayout.ToggleLeft ("Speech audio can be translated?", translateAudio, "AC.KickStarter.speechManager.translateAudio");
				if (translateAudio)
				{
					fallbackAudio = CustomGUILayout.ToggleLeft ("Use original language audio if none found?", fallbackAudio, "AC.KickStarter.speechManager.fallbackAudio");
				}
				usePlayerRealName = CustomGUILayout.ToggleLeft ("Use Player prefab name in filenames?", usePlayerRealName, "AC.KickStarter.speechManager.usePlayerRealName");
				if (autoNameSpeechFiles && usePlayerRealName && KickStarter.settingsManager != null && KickStarter.settingsManager.playerSwitching == PlayerSwitching.Allow)
				{
					separateSharedPlayerAudio = CustomGUILayout.ToggleLeft ("'Player' lines have separate audio for each player?", separateSharedPlayerAudio, "AC.KickStarter.speechManager.separateSharedPlayerAudio");
				}
				if (autoNameSpeechFiles)
				{
					placeAudioInSubfolders = CustomGUILayout.ToggleLeft ("Place audio files in speaker subfolders?", placeAudioInSubfolders, "AC.KickStarter.speechManager.placeAudioInSubfolders");
				}

				sfxDucking = CustomGUILayout.Slider ("SFX reduction during:", sfxDucking, 0f, 1f, "AC.KickStarter.speechManager.sfxDucking");
				musicDucking = CustomGUILayout.Slider ("Music reduction during:", musicDucking, 0f, 1f, "AC.KickStarter.speechManager.musicDucking");
				relegateBackgroundSpeechAudio = CustomGUILayout.ToggleLeft ("End background speech audio if non-background plays?", relegateBackgroundSpeechAudio, "AC.KickStarter.speechManager.relegateBackgroundSpeechAudio");
				mergeMatchingSpeechIDs = CustomGUILayout.ToggleLeft ("Give matching speech lines the same ID?", mergeMatchingSpeechIDs, "AC.KickStarter.speechManager.mergeMatchingSpeechIDs");
			}
			EditorGUILayout.EndVertical ();
			EditorGUILayout.Space ();

			EditorGUILayout.Space ();
			EditorGUILayout.BeginVertical (CustomStyles.thinBox);
			showLipSyncing = CustomGUILayout.ToggleHeader (showLipSyncing, "Lip syncing");
			if (showLipSyncing)
			{
				lipSyncMode = (LipSyncMode) CustomGUILayout.EnumPopup ("Lip syncing:", lipSyncMode, "AC.KickStarter.speechManager.lipSyncMode");
				if (lipSyncMode == LipSyncMode.FromSpeechText || lipSyncMode == LipSyncMode.ReadPamelaFile || lipSyncMode == LipSyncMode.ReadSapiFile || lipSyncMode == LipSyncMode.ReadPapagayoFile)
				{
					lipSyncOutput = (LipSyncOutput) CustomGUILayout.EnumPopup ("Perform lipsync on:", lipSyncOutput, "AC.KickStarter.speechManager.lipSyncOutput");
					lipSyncSpeed = CustomGUILayout.FloatField ("Process speed:", lipSyncSpeed, "AC.KickStarter.speechManager.lipSyncSpeed");
					
					if (GUILayout.Button ("Edit phonemes"))
					{
						PhonemesWindow.Init ();
					}

					if (lipSyncOutput == LipSyncOutput.GameObjectTexture)
					{
						EditorGUILayout.HelpBox ("Characters will require the 'LipSyncTexture' component in order to perform lip-syncing.", MessageType.Info);
					}
				}
				else if (lipSyncMode == LipSyncMode.FaceFX && !FaceFXIntegration.IsDefinePresent ())
				{
					EditorGUILayout.HelpBox ("The 'FaceFXIsPresent' preprocessor define must be declared in the Player Settings.", MessageType.Warning);
				}
				else if (lipSyncMode == LipSyncMode.Salsa2D)
				{
					lipSyncOutput = (LipSyncOutput) CustomGUILayout.EnumPopup ("Perform lipsync on:", lipSyncOutput, "AC.KickStarter.speechManager.lipSyncOutput");
					
					EditorGUILayout.HelpBox ("Speaking animations must have 4 frames: Rest, Small, Medium and Large.", MessageType.Info);
					
					#if !SalsaIsPresent
					EditorGUILayout.HelpBox ("The 'SalsaIsPresent' preprocessor define must be declared in the Player Settings.", MessageType.Warning);
					#endif
				}
				else if (lipSyncMode == LipSyncMode.RogoLipSync && !RogoLipSyncIntegration.IsDefinePresent ())
				{
					EditorGUILayout.HelpBox ("The 'RogoLipSyncIsPresent' preprocessor define must be declared in the Player Settings.", MessageType.Warning);
				}

				if (autoNameSpeechFiles && lipSyncMode != LipSyncMode.Off && lipSyncMode != LipSyncMode.FaceFX)
				{
					autoLipsyncFolder = CustomGUILayout.TextField ("Lipsync data directory:", autoLipsyncFolder, "AC.KickStarter.speechManager.autoLipsyncFolder");
				}
			}
			EditorGUILayout.EndVertical ();

			EditorGUILayout.Space ();
			LanguagesGUI ();
			
			EditorGUILayout.Space ();

			EditorGUILayout.BeginVertical (CustomStyles.thinBox);
			showGameText = CustomGUILayout.ToggleHeader (showGameText, "Game text");
			if (showGameText)
			{
				EditorGUILayout.BeginVertical (CustomStyles.thinBox);
				string numLines = (lines != null) ? lines.Count.ToString () : "0";
				EditorGUILayout.LabelField ("Gathered " + numLines + " lines of text.");
				EditorGUILayout.EndVertical ();

				EditorGUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Gather text", EditorStyles.miniButtonLeft))
				{
					PopulateList ();
					
					if (sceneFiles != null && sceneFiles.Length > 0)
					{
						Array.Sort (sceneFiles);
					}
				}
				if (GUILayout.Button ("Reset text", EditorStyles.miniButtonMid))
				{
					ClearList ();
				}

				if (lines.Count == 0)
				{
					GUI.enabled = false;
				}
				
				if (GUILayout.Button ("Create script sheet..", EditorStyles.miniButtonRight))
				{
					if (lines.Count > 0)
					{
						ScriptSheetWindow.Init ();
					}
				}
				EditorGUILayout.EndHorizontal ();

				EditorGUILayout.BeginHorizontal ();
				if (GUILayout.Button ("Import text...", EditorStyles.miniButtonLeft))
				{
					ImportGameText ();
				}
				if (GUILayout.Button ("Export text...", EditorStyles.miniButtonRight))
				{
					ExportGameText ();
				}
				EditorGUILayout.EndHorizontal ();

				GUI.enabled = true;

				if (lines.Count > 0)
				{
					EditorGUILayout.Space ();

					if (Application.isPlaying && !EditorApplication.isPaused)
					{
						EditorGUILayout.HelpBox ("To aid performance, game text is hidden while the game is runninng - to show it, either stop or pause the game.", MessageType.Info);
					}
					else
					{
						ListLines ();
					}
				}
			}
			EditorGUILayout.EndVertical ();

			if (GUI.changed)
			{
				EditorUtility.SetDirty (this);
			}
		}
		
		
		public string[] GetSceneNames ()
		{
			sceneNames.Clear ();
			sceneNames.Add ("(No scene)");
			sceneNames.Add ("(Any or no scene)");
			foreach (string sceneFile in sceneFiles)
			{
				int slashPoint = sceneFile.LastIndexOf ("/") + 1;
				string sceneName = sceneFile.Substring (slashPoint);

				if (sceneName.Length > 6)
				{
					sceneNames.Add (sceneName.Substring (0, sceneName.Length - 6));
				}
				else
				{
					ACDebug.LogError ("Invalid scene file name '" + sceneFile + "' is this saved and named properly in the Assets folder?");
				}
			}
			return sceneNames.ToArray ();
		}


		private Dictionary<int, SpeechLine> displayedLinesDictionary = new Dictionary<int, SpeechLine>();
		private void CacheDisplayLines ()
		{
			List<SpeechLine> sortedLines = new List<SpeechLine>();
			foreach (SpeechLine line in lines)
			{
				sortedLines.Add (new SpeechLine (line));
			}

			if (gameTextSorting == GameTextSorting.ByID)
			{
				sortedLines.Sort (delegate (SpeechLine a, SpeechLine b) {return a.lineID.CompareTo (b.lineID);});
			}
			else if (gameTextSorting == GameTextSorting.ByDescription)
			{
				sortedLines.Sort (delegate (SpeechLine a, SpeechLine b) {return a.description.CompareTo (b.description);});
			}

			string selectedScene = sceneNames[sceneFilter] + ".unity";

			displayedLinesDictionary.Clear ();
			foreach (SpeechLine line in sortedLines)
			{
				if (line.textType == typeFilter && line.Matches (textFilter, filterSpeechLine))
				{
					string scenePlusExtension = (line.scene != "") ? (line.scene + ".unity") : "";
					
					if ((line.scene == "" && sceneFilter == 0)
					    || sceneFilter == 1
					    || (line.scene != "" && sceneFilter > 1 && line.scene.EndsWith (selectedScene))
					    || (line.scene != "" && sceneFilter > 1 && scenePlusExtension.EndsWith (selectedScene)))
					{
						if (tagFilter <= 0
						|| ((tagFilter-1) < speechTags.Count && line.tagID == speechTags[tagFilter-1].ID))
						{
							if (typeFilter == AC_TextType.Speech && !autoNameSpeechFiles)
							{
								if (audioFilter == AudioFilter.OnlyWithAudio)
								{
									if (translateAudio && languages != null && languages.Count > 1)
									{
										for (int i=0; i<(languages.Count-1); i++)
										{
											if (line.customTranslationAudioClips.Count > i)
											{
												if (line.customTranslationAudioClips[i] == null) continue;
											}
										}
									}
									if (line.customAudioClip == null) continue;
								}
								else if (audioFilter == AudioFilter.OnlyWithoutAudio)
								{
									bool hasAllAudio = true;

									if (translateAudio && languages != null && languages.Count > 1)
									{
										for (int i=0; i<(languages.Count-1); i++)
										{
											if (line.customTranslationAudioClips.Count > i)
											{
												if (line.customTranslationAudioClips[i] == null) hasAllAudio = false;
											}
										}
									}
									if (line.customAudioClip == null) hasAllAudio = false;

									if (hasAllAudio) continue;
								}
							}
						
							displayedLinesDictionary.Add (line.lineID, line);
						}
					}
				}
			}
		}
		
		
		private void ListLines ()
		{
			if (sceneNames == null || sceneNames == new List<string>() || sceneNames.Count != (sceneFiles.Length + 2))
			{
				sceneFiles = AdvGame.GetSceneFiles ();
				GetSceneNames ();
			}
			
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Type filter:", GUILayout.Width (65f));
			typeFilter = (AC_TextType) EditorGUILayout.EnumPopup (typeFilter);
			EditorGUILayout.EndHorizontal ();
			
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Scene filter:", GUILayout.Width (65f));
			sceneFilter = EditorGUILayout.Popup (sceneFilter, sceneNames.ToArray ());
			EditorGUILayout.EndHorizontal ();
			
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Text filter:", GUILayout.Width (65f));
			filterSpeechLine = (FilterSpeechLine) EditorGUILayout.EnumPopup (filterSpeechLine, GUILayout.MaxWidth (100f));
			textFilter = EditorGUILayout.TextField (textFilter);
			EditorGUILayout.EndHorizontal ();

			if (typeFilter == AC_TextType.Speech && useSpeechTags && speechTags != null && speechTags.Count > 1)
			{
				List<string> tagNames = new List<string>();
				tagNames.Add ("(Any or no tag)");
				foreach (SpeechTag speechTag in speechTags)
				{
					tagNames.Add (speechTag.label);
				}

				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Tag filter:", GUILayout.Width (65f));
				tagFilter = EditorGUILayout.Popup (tagFilter, tagNames.ToArray ());
				EditorGUILayout.EndHorizontal ();
			}
			else
			{
				tagFilter = 0;
			}

			if (typeFilter == AC_TextType.Speech && !autoNameSpeechFiles)
			{
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField ("Audio filter:", GUILayout.Width (65f));
				audioFilter = (AudioFilter) EditorGUILayout.EnumPopup (audioFilter);
				EditorGUILayout.EndHorizontal ();
			}

			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Sort by:", GUILayout.Width (65f));
			gameTextSorting = (GameTextSorting) EditorGUILayout.EnumPopup (gameTextSorting);
			if (lastGameTextSorting != gameTextSorting)
			{
				activeLineID = -1;
			}
			lastGameTextSorting = gameTextSorting;
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.Space ();

			if (sceneNames.Count <= sceneFilter)
			{
				sceneFilter = 0;
				return;
			}

			bool doCache = GUI.changed;

			if (doCache || (displayedLinesDictionary.Count == 0 && lines.Count > 0))
			{
				CacheDisplayLines ();
			}

			foreach (KeyValuePair<int, SpeechLine> displayedLine in displayedLinesDictionary)
			{
				displayedLine.Value.ShowGUI ();
			}

			doCache = GUI.changed;

			if (doCache)
			{
				// Place back
				for (int j=0; j<lines.Count; j++)
				{
					SpeechLine displayedLine;
					if (displayedLinesDictionary.TryGetValue (lines[j].lineID, out displayedLine))
					{
						lines[j] = new SpeechLine (displayedLine);
					}
				}
			}
		}
		
		
		private void LanguagesGUI ()
		{
			EditorGUILayout.BeginVertical (CustomStyles.thinBox);
			showTranslations = CustomGUILayout.ToggleHeader (showTranslations, "Languages");
			if (showTranslations)
			{
				if (languages.Count == 0)
				{
					ClearLanguages ();
				}

				SyncLanguageData ();

				/*if (languages.Count > 1)
				{
					ignoreOriginalText = CustomGUILayout.ToggleLeft ("Prevent original language from being used?", ignoreOriginalText, "AC.KickStarter.speechManager.ignoreOriginalText");
				}*/

				ignoreOriginalText = CustomGUILayout.ToggleLeft ("Prevent original language from being used?", ignoreOriginalText, "AC.KickStarter.speechManager.ignoreOriginalText");
				if (ignoreOriginalText && languages.Count <= 1)
				{
					EditorGUILayout.HelpBox ("At least one translation must be defined below for the original language to be ignored.", MessageType.Warning);
				}

				if (languages.Count < 1 || !ignoreOriginalText)
				{
					EditorGUILayout.BeginVertical (CustomStyles.thinBox);
					EditorGUILayout.BeginHorizontal ();
					languages[0] = EditorGUILayout.TextField ("Original language:", languages[0]);

					if (GUILayout.Button (Resource.CogIcon, GUILayout.Width (20f), GUILayout.Height (15f)))
					{
						SideMenu (0);
					}
					EditorGUILayout.EndHorizontal ();
					languageIsRightToLeft[0] = CustomGUILayout.Toggle ("Reads right-to-left?", languageIsRightToLeft[0], "AC.KickStarter.speechManager.languageIsRightToLeft[0]");
					EditorGUILayout.EndVertical ();
				}

				if (languages.Count > 1)
				{
					for (int i=1; i<languages.Count; i++)
					{
						EditorGUILayout.BeginVertical (CustomStyles.thinBox);
						EditorGUILayout.BeginHorizontal ();
						EditorGUILayout.LabelField ("Language #" + i.ToString () + ":", GUILayout.Width (146f));
						languages[i] = EditorGUILayout.TextField (languages[i]);

						if (GUILayout.Button (Resource.CogIcon, GUILayout.Width (20f), GUILayout.Height (15f)))
						{
							SideMenu (i);
						}
						EditorGUILayout.EndHorizontal ();
						languageIsRightToLeft[i] = CustomGUILayout.Toggle ("Reads right-to-left?", languageIsRightToLeft[i], "AC.KickStarter.speechManager.languageIsRightToLeft[" + i.ToString () + "]");
						EditorGUILayout.EndVertical ();
					}
				}

				if (GUILayout.Button ("Create new translation"))
				{
					Undo.RecordObject (this, "Add translation");
					CreateLanguage ("New " + languages.Count.ToString ());
				}

				if (lines.Count == 0)
				{
					EditorGUILayout.HelpBox ("No text has been gathered for translations - add your scenes to the build, and click 'Gather text' below.", MessageType.Warning);
				}
			}

			if (Application.isPlaying)
			{
				EditorGUILayout.HelpBox ("Changes made will not be updated until the game is restarted.", MessageType.Info);
			}

			EditorGUILayout.EndVertical ();
		}


		private void SideMenu (int i)
		{
			GenericMenu menu = new GenericMenu ();

			sideLanguage = i;

			if (i > 0)
			{
				menu.AddItem (new GUIContent ("Import"), false, MenuCallback, "Import translation");
				menu.AddItem (new GUIContent ("Export"), false, MenuCallback, "Export translation");
				menu.AddItem (new GUIContent ("Delete"), false, MenuCallback, "Delete translation");

				if (languages.Count > 2)
				{
					menu.AddSeparator ("");

					if (i > 1)
					{
						menu.AddItem (new GUIContent ("Move up"), false, MenuCallback, "Move translation up");
					}

					if (i < (languages.Count - 1))
					{
						menu.AddItem (new GUIContent ("Move down"), false, MenuCallback, "Move translation down");
					}
				}

			}

			if (lines.Count > 0)
			{
				menu.AddSeparator ("");
				menu.AddItem (new GUIContent ("Create script sheet.."), false, MenuCallback, "Create script sheet");
			}

			menu.ShowAsContext ();
		}


		private void MenuCallback (object obj)
		{
			if (sideLanguage >= 0)
			{
				int i = sideLanguage;
				switch (obj.ToString ())
				{
				case "Import translation":
					ImportTranslation (i);
					break;

				case "Export translation":
					ExportWizardWindow.Init (this, i);
					break;

				case "Delete translation":
					Undo.RecordObject (this, "Delete translation '" + languages[i] + "'");
					DeleteLanguage (i);
					break;

				case "Move translation down":
					Undo.RecordObject (this, "Move down translation '" + languages[i] + "'");
					MoveLanguageDown (i);
					break;

				case "Move translation up":
					Undo.RecordObject (this, "Move up translation '" + languages[i] + "'");
					MoveLanguageUp (i);
					break;

				case "Create script sheet":
					ScriptSheetWindow.Init (i);
					break;
				}
			}
			
			sideLanguage = -1;
		}
		

		TransferComment correctTranslationCount;
		private void CreateLanguage (string name)
		{
			correctTranslationCount = TransferComment.NotAsked;
			int numFixes = 0;

			foreach (SpeechLine line in lines)
			{
				if (line.translationText.Count > (languages.Count - 1))
				{
					if (correctTranslationCount == TransferComment.NotAsked)
					{
						bool canFix = EditorUtility.DisplayDialog ("Fix translations", "One or more lines have been found to have translations for languages that no longer exist.  Shall AC remove these for you?  You should back up the project beforehand.", "Yes", "No");
						correctTranslationCount = (canFix) ? TransferComment.Yes : TransferComment.No;
					}

					if (correctTranslationCount == TransferComment.Yes)
					{
						numFixes ++;
						while (line.translationText.Count > (languages.Count - 1))
						{
							line.translationText.RemoveAt (line.translationText.Count-1);
						}
					}
				}
			}

			if (numFixes > 0)
			{
				ACDebug.Log ("Fixed " + numFixes + " translation mismatches.");
			}
						

			languages.Add (name);
			
			foreach (SpeechLine line in lines)
			{
				line.translationText.Add (line.text);
			}

			languageIsRightToLeft.Add (false);
		}

		
		private void DeleteLanguage (int i)
		{
			languages.RemoveAt (i);
			languageIsRightToLeft.RemoveAt (i);
			
			foreach (SpeechLine line in lines)
			{
				line.translationText.RemoveAt (i-1);

				if (line.customTranslationAudioClips != null && line.customTranslationAudioClips.Count > (i-1))
				{
					line.customTranslationAudioClips.RemoveAt (i-1);
				}
				if (line.customTranslationLipsyncFiles != null && line.customTranslationLipsyncFiles.Count > (i-1))
				{
					line.customTranslationLipsyncFiles.RemoveAt (i-1);
				}
			}
		}


		private void MoveLanguageDown (int i)
		{
			string thisLanguage = languages[i];
			languages.Insert (i+2, thisLanguage);
			languages.RemoveAt (i);

			bool thisLanguageIsRightToLeft = languageIsRightToLeft[i];
			languageIsRightToLeft.Insert (i+2, thisLanguageIsRightToLeft);
			languageIsRightToLeft.RemoveAt (i);

			foreach (SpeechLine line in lines)
			{
				string thisTranslationText = line.translationText[i-1];
				line.translationText.Insert (i+1, thisTranslationText);
				line.translationText.RemoveAt (i-1);

				if (line.customTranslationAudioClips != null && line.customTranslationAudioClips.Count > (i-1))
				{
					AudioClip thisAudioClip = line.customTranslationAudioClips[i-1];
					line.customTranslationAudioClips.Insert (i+1, thisAudioClip);
					line.customTranslationAudioClips.RemoveAt (i-1);
				}
				if (line.customTranslationLipsyncFiles != null && line.customTranslationLipsyncFiles.Count > (i-1))
				{
					UnityEngine.Object thisLipSyncFile = line.customTranslationLipsyncFiles[i-1];
					line.customTranslationLipsyncFiles.Insert (i+1, thisLipSyncFile);
					line.customTranslationLipsyncFiles.RemoveAt (i-1);
				}
			}
		}


		private void MoveLanguageUp (int i)
		{
			string thisLanguage = languages[i];
			languages.Insert (i-1, thisLanguage);
			languages.RemoveAt (i+1);

			bool thisLanguageIsRightToLeft = languageIsRightToLeft[i];
			languageIsRightToLeft.Insert (i-1, thisLanguageIsRightToLeft);
			languageIsRightToLeft.RemoveAt (i+1);

			foreach (SpeechLine line in lines)
			{
				string thisTranslationText = line.translationText[i-1];
				line.translationText.Insert (i-2, thisTranslationText);
				line.translationText.RemoveAt (i);

				if (line.customTranslationAudioClips != null && line.customTranslationAudioClips.Count > (i-1))
				{
					AudioClip thisAudioClip = line.customTranslationAudioClips[i-1];
					line.customTranslationAudioClips.Insert (i-2, thisAudioClip);
					line.customTranslationAudioClips.RemoveAt (i);
				}
				if (line.customTranslationLipsyncFiles != null && line.customTranslationLipsyncFiles.Count > (i-1))
				{
					UnityEngine.Object thisLipSyncFile = line.customTranslationLipsyncFiles[i-1];
					line.customTranslationLipsyncFiles.Insert (i-2, thisLipSyncFile);
					line.customTranslationLipsyncFiles.RemoveAt (i);
				}
			}
		}


		/**
		 * Removes all translations.
		 */
		public void ClearLanguages ()
		{
			languages.Clear ();
			
			foreach (SpeechLine line in lines)
			{
				line.translationText.Clear ();
				line.customTranslationAudioClips.Clear ();
				line.customTranslationLipsyncFiles.Clear ();
			}
			
			languages.Add ("Original");	

			languageIsRightToLeft.Clear ();
			languageIsRightToLeft.Add (false);
		}


		public void LocateLine (SpeechLine speechLine)
		{
			if (speechLine == null) return;
			if (speechLine.textType == AC_TextType.Speech)
			{
				LocateDialogueLine (speechLine);
			}
			else if (speechLine.textType == AC_TextType.DialogueOption)
			{
				LocateDialogueOption (speechLine);
			}
		}


		private void LocateDialogueOption (SpeechLine speechLine)
		{
			if (speechLine.scene != "")
			{
				if (UnityVersionHandler.SaveSceneIfUserWants ())
				{
					sceneFiles = AdvGame.GetSceneFiles ();

					foreach (string sceneFile in sceneFiles)
					{
						UnityVersionHandler.OpenScene (sceneFile);

						ActionList[] actionLists = GameObject.FindObjectsOfType (typeof (ActionList)) as ActionList[];
						foreach (ActionList list in actionLists)
						{
							if (list.source == ActionListSource.InScene)
							{
								foreach (Action action in list.actions)
								{
									if (action != null && action is ActionDialogOptionRename)
									{
										ActionDialogOptionRename actionDialogOptionRename = (ActionDialogOptionRename) action;
										if (actionDialogOptionRename.lineID == speechLine.lineID)
										{
											EditorGUIUtility.PingObject (list);
											return;
										}
									}
								}
							}
						}

						Conversation[] conversations = GameObject.FindObjectsOfType (typeof (Conversation)) as Conversation[];
						foreach (Conversation conversation in conversations)
						{
							if (conversation.interactionSource == InteractionSource.InScene)
							{
								if (conversation.options != null)
								{
									foreach (ButtonDialog option in conversation.options)
									{
										if (option != null && option.lineID == speechLine.lineID)
										{
											EditorGUIUtility.PingObject (conversation);
											return;
										}
									}
								}
							}
						}

						ACDebug.Log ("Could not find line " + speechLine.lineID + " - is the scene added to the Build Settings?");
					}
				}
			}
			else
			{
				// Asset file

				CollectAllActionListAssets ();
				foreach (ActionListAsset actionListAsset in allActionListAssets)
				{
					foreach (Action action in actionListAsset.actions)
					{
						if (action != null && action is ActionDialogOptionRename)
						{
							ActionDialogOptionRename actionDialogOptionRename = (ActionDialogOptionRename) action;
							if (actionDialogOptionRename.lineID == speechLine.lineID)
							{
								EditorGUIUtility.PingObject (actionListAsset);
								return;
							}
						}
					}
				}

				ACDebug.Log ("Could not find line " + speechLine.lineID + " - is ActionList asset still present?");
			}
		}


		private void LocateDialogueLine (SpeechLine speechLine)
		{
			if (speechLine.scene != "")
			{
				// In a scene

				if (UnityVersionHandler.SaveSceneIfUserWants ())
				{
					sceneFiles = AdvGame.GetSceneFiles ();

					foreach (string sceneFile in sceneFiles)
					{
						UnityVersionHandler.OpenScene (sceneFile);

						ActionList[] actionLists = GameObject.FindObjectsOfType (typeof (ActionList)) as ActionList[];
						foreach (ActionList list in actionLists)
						{
							if (list.source == ActionListSource.InScene)
							{
								foreach (Action action in list.actions)
								{
									if (action != null && action is ActionSpeech)
									{
										ActionSpeech actionSpeech = (ActionSpeech) action;
										if (actionSpeech.lineID == speechLine.lineID)
										{
											EditorGUIUtility.PingObject (list);
											return;
										}
									}
								}
							}
						}

						ACDebug.Log ("Could not find line " + speechLine.lineID + " - is the scene added to the Build Settings?");
					}
				}
			}
			else
			{
				// Asset file

				CollectAllActionListAssets ();
				foreach (ActionListAsset actionListAsset in allActionListAssets)
				{
					foreach (Action action in actionListAsset.actions)
					{
						if (action != null && action is ActionSpeech)
						{
							ActionSpeech actionSpeech = (ActionSpeech) action;
							if (actionSpeech.lineID == speechLine.lineID)
							{
								EditorGUIUtility.PingObject (actionListAsset);
								return;
							}
						}
					}
				}

				ACDebug.Log ("Could not find line " + speechLine.lineID + " - is ActionList asset still present?");
			}
		}


		private int[] GetIDArray ()
		{
			List<int> idArray = new List<int>();
			
			foreach (SpeechLine line in lines)
			{
				idArray.Add (line.lineID);
			}

			if (tempLines != null)
			{
				foreach (SpeechLine tempLine in tempLines)
				{
					idArray.Add (tempLine.lineID);
				}
			}
			
			idArray.Sort ();
			return idArray.ToArray ();
		}


		private int GetEmptyID ()
		{
			int[] idArray = GetIDArray ();

			if (idArray != null && idArray.Length > 0)
			{
				int lastEntry = idArray [idArray.Length-1];

				if (lastEntry > maxID)
				{
					maxID = lastEntry;
				}
			}

			return maxID + 1;
		}
		
		
		private void PopulateList ()
		{
			transferComment = TransferComment.NotAsked;
			string originalScene = UnityVersionHandler.GetCurrentSceneFilepath ();

			bool canProceed = EditorUtility.DisplayDialog ("Gather game text", "AC will now go through your game, and collect all game text so that it can be translated/voiced.\n\nIt is recommended to back up your project beforehand.", "OK", "Cancel");
			if (!canProceed) return;

			if (UnityVersionHandler.SaveSceneIfUserWants ())
			{
				Undo.RecordObject (this, "Update speech list");

				int originalLineCount = (lines != null) ? lines.Count : 0;

				// Store the lines temporarily, so that we can update the translations afterwards
				BackupTranslations ();
				
				lines.Clear ();
				checkedAssets.Clear ();
				
				sceneFiles = AdvGame.GetSceneFiles ();
				GetSceneNames ();
				
				// First look for lines that already have an assigned lineID
				foreach (string sceneFile in sceneFiles)
				{
					GetLinesInScene (sceneFile, false);
				}

				GetLinesFromSettings (false);
				GetLinesFromInventory (false);
				GetLinesFromVariables (true, false);
				GetLinesFromCursors (false);
				GetLinesFromMenus (false);

				CollectAllActionListAssets ();
				foreach (ActionListAsset actionListAsset in allActionListAssets)
				{
					ProcessActionListAsset (actionListAsset, false);
				}

				checkedAssets.Clear ();
				
				// Now look for new lines, which don't have a unique lineID
				foreach (string sceneFile in sceneFiles)
				{
					GetLinesInScene (sceneFile, true);
				}

				GetLinesFromSettings (true);
				GetLinesFromInventory (true);
				GetLinesFromVariables (true, true);
				GetLinesFromCursors (true);
				GetLinesFromMenus (true);

				foreach (ActionListAsset actionListAsset in allActionListAssets)
				{
					ProcessActionListAsset (actionListAsset, true);
				}

				if (mergeMatchingSpeechIDs)
				{
					MergeMatchingSpeechIDs ();
				}

				RestoreTranslations ();
				checkedAssets.Clear ();

				GetEmptyID ();

				allActionListAssets.Clear ();
				UnityVersionHandler.OpenScene (originalScene);
	
				int newLineCount = (lines != null) ? lines.Count : 0;
				int differenceLineCount = newLineCount - originalLineCount;
				EditorUtility.DisplayDialog ("Gather game text", "Process complete.  " + newLineCount + " entries gathered, " + differenceLineCount + " entries added.", "OK");
			}
		}
		
		
		private void ExtractConversation (Conversation conversation, bool onlySeekNew)
		{
			foreach (ButtonDialog dialogOption in conversation.options)
			{
				if (onlySeekNew && dialogOption.lineID < 1)
				{
					// Assign a new ID on creation
					SpeechLine newLine;
					newLine = new SpeechLine (GetEmptyID (), UnityVersionHandler.GetCurrentSceneName (), dialogOption.label, languages.Count - 1, AC_TextType.DialogueOption);
					dialogOption.lineID = newLine.lineID;
					lines.Add (newLine);
				}
				
				else if (!onlySeekNew && dialogOption.lineID > 0)
				{
					// Already has an ID, so don't replace
					SpeechLine existingLine = new SpeechLine (dialogOption.lineID, UnityVersionHandler.GetCurrentSceneName (), dialogOption.label, languages.Count - 1, AC_TextType.DialogueOption);
					
					int lineID = SmartAddLine (existingLine);
					if (lineID >= 0) dialogOption.lineID = lineID;
				}
			}
		}
		
		
		private void ExtractHotspot (Hotspot hotspot, bool onlySeekNew)
		{
			string hotspotName = hotspot.name;
			if (hotspot.hotspotName != "")
			{
				hotspotName = hotspot.hotspotName;
			}
			
			if (onlySeekNew && hotspot.lineID == -1)
			{
				// Assign a new ID on creation
				SpeechLine newLine = new SpeechLine (GetEmptyID (), UnityVersionHandler.GetCurrentSceneName (), hotspotName, languages.Count - 1, AC_TextType.Hotspot);
				
				hotspot.lineID = newLine.lineID;
				lines.Add (newLine);
			}
			
			else if (!onlySeekNew && hotspot.lineID > -1)
			{
				// Already has an ID, so don't replace
				SpeechLine existingLine = new SpeechLine (hotspot.lineID, UnityVersionHandler.GetCurrentSceneName (), hotspotName, languages.Count - 1, AC_TextType.Hotspot);
				
				int lineID = SmartAddLine (existingLine);
				if (lineID >= 0) hotspot.lineID = lineID;
			}
		}


		private void ExtractCharacter (AC.Char character, bool onlySeekNew)
		{
			if (character == null) return;
			string characterName = character.speechLabel;

			if (character.speechLabel != null && character.speechLabel.Length > 0)
			{
				if (onlySeekNew && character.lineID == -1)
				{
					// Assign a new ID on creation
					SpeechLine newLine = new SpeechLine (GetEmptyID (), UnityVersionHandler.GetCurrentSceneName (), characterName, languages.Count - 1, AC_TextType.Character);
					
					character.lineID = newLine.lineID;
					lines.Add (newLine);
				}
				
				else if (!onlySeekNew && character.lineID > -1)
				{
					// Already has an ID, so don't replace
					SpeechLine existingLine = new SpeechLine (character.lineID, UnityVersionHandler.GetCurrentSceneName (), characterName, languages.Count - 1, AC_TextType.Character);
					
					int lineID = SmartAddLine (existingLine);
					if (lineID >= 0) character.lineID = lineID;
				}
			}
		}

		
		private void ExtractInventory (InvItem invItem, bool onlySeekNew)
		{
			if (onlySeekNew && invItem.lineID == -1)
			{
				// Assign a new ID on creation
				SpeechLine newLine;
				string _label = invItem.label;
				if (invItem.altLabel != "")
				{
					_label = invItem.altLabel;
				}
				
				newLine = new SpeechLine (GetEmptyID (), "", _label, languages.Count - 1, AC_TextType.InventoryItem);
				invItem.lineID = newLine.lineID;
				lines.Add (newLine);
			}
			
			else if (!onlySeekNew && invItem.lineID > -1)
			{
				// Already has an ID, so don't replace
				string _label = invItem.label;
				if (invItem.altLabel != "")
				{
					_label = invItem.altLabel;
				}
				
				SpeechLine existingLine = new SpeechLine (invItem.lineID, "", _label, languages.Count - 1, AC_TextType.InventoryItem);
				
				int lineID = SmartAddLine (existingLine);
				if (lineID >= 0) invItem.lineID = lineID;
			}
		}
		
		
		private void ExtractPrefix (HotspotPrefix prefix, bool onlySeekNew)
		{
			if (onlySeekNew && prefix.lineID == -1)
			{
				// Assign a new ID on creation
				SpeechLine newLine;
				newLine = new SpeechLine (GetEmptyID (), "", prefix.label, languages.Count - 1, AC_TextType.HotspotPrefix);
				prefix.lineID = newLine.lineID;
				lines.Add (newLine);
			}
			else if (!onlySeekNew && prefix.lineID > -1)
			{
				// Already has an ID, so don't replace
				SpeechLine existingLine = new SpeechLine (prefix.lineID, "", prefix.label, languages.Count - 1, AC_TextType.HotspotPrefix);
				
				int lineID = SmartAddLine (existingLine);
				if (lineID >= 0) prefix.lineID = lineID;
			}
		}
		
		
		private void ExtractIcon (CursorIcon icon, bool onlySeekNew)
		{
			if (onlySeekNew && icon.lineID == -1)
			{
				// Assign a new ID on creation
				SpeechLine newLine;
				newLine = new SpeechLine (GetEmptyID (), "", icon.label, languages.Count - 1, AC_TextType.CursorIcon);
				icon.lineID = newLine.lineID;
				lines.Add (newLine);
			}
			
			else if (!onlySeekNew && icon.lineID > -1)
			{
				// Already has an ID, so don't replace
				SpeechLine existingLine = new SpeechLine (icon.lineID, "", icon.label, languages.Count - 1, AC_TextType.CursorIcon);
				
				int lineID = SmartAddLine (existingLine);
				if (lineID >= 0) icon.lineID = lineID;
			}
		}
		
		
		private void ExtractElement (MenuElement element, string elementLabel, bool onlySeekNew)
		{
			if (elementLabel == null || elementLabel.Length == 0)
			{
				element.lineID = -1;
				return;
			}

			if (onlySeekNew && element.lineID == -1)
			{
				// Assign a new ID on creation
				SpeechLine newLine = new SpeechLine (GetEmptyID (), "", element.title, elementLabel, languages.Count - 1, AC_TextType.MenuElement);
				element.lineID = newLine.lineID;
				lines.Add (newLine);
			}
			
			else if (!onlySeekNew && element.lineID > -1)
			{
				// Already has an ID, so don't replace
				SpeechLine existingLine = new SpeechLine (element.lineID, "", element.title, elementLabel, languages.Count - 1, AC_TextType.MenuElement);
				
				int lineID = SmartAddLine (existingLine);
				if (lineID >= 0) element.lineID = lineID;
			}
		}


		private void ExtractToggleElement (MenuToggle toggle, bool onlySeekNew)
		{
			if (toggle.onText == null || toggle.onText.Length == 0)
			{
				toggle.onTextLineID = -1;
			}
			else
			{
				if (onlySeekNew && toggle.onTextLineID == -1)
				{
					// Assign a new ID on creation
					SpeechLine newLine = new SpeechLine (GetEmptyID (), "", toggle.title, toggle.onText, languages.Count - 1, AC_TextType.MenuElement);
					toggle.onTextLineID = newLine.lineID;
					lines.Add (newLine);
				}
				else if (!onlySeekNew && toggle.onTextLineID > -1)
				{
					// Already has an ID, so don't replace
					SpeechLine existingLine = new SpeechLine (toggle.onTextLineID, "", toggle.title, toggle.onText, languages.Count - 1, AC_TextType.MenuElement);
					
					int lineID = SmartAddLine (existingLine);
					if (lineID >= 0) toggle.onTextLineID = lineID;
				}
			}

			if (toggle.offText == null || toggle.offText.Length == 0)
			{
				toggle.offTextLineID = -1;
			}
			else
			{
				if (onlySeekNew && toggle.offTextLineID == -1)
				{
					// Assign a new ID on creation
					SpeechLine newLine = new SpeechLine (GetEmptyID (), "", toggle.title, toggle.offText, languages.Count - 1, AC_TextType.MenuElement);
					toggle.offTextLineID = newLine.lineID;
					lines.Add (newLine);
				}
				else if (!onlySeekNew && toggle.offTextLineID > -1)
				{
					// Already has an ID, so don't replace
					SpeechLine existingLine = new SpeechLine (toggle.offTextLineID, "", toggle.title, toggle.offText, languages.Count - 1, AC_TextType.MenuElement);
					
					int lineID = SmartAddLine (existingLine);
					if (lineID >= 0) toggle.offTextLineID = lineID;
				}
			}
		}
		
		
		private void ExtractHotspotOverride (MenuButton button, string hotspotLabel, bool onlySeekNew)
		{
			if (hotspotLabel == "")
			{
				button.hotspotLabelID = -1;
				return;
			}
			
			if (onlySeekNew && button.hotspotLabelID == -1)
			{
				// Assign a new ID on creation
				SpeechLine newLine = new SpeechLine (GetEmptyID (), "", button.title, hotspotLabel, languages.Count - 1, AC_TextType.MenuElement);
				button.hotspotLabelID = newLine.lineID;
				lines.Add (newLine);
			}
			
			else if (!onlySeekNew && button.hotspotLabelID > -1)
			{
				// Already has an ID, so don't replace
				SpeechLine existingLine = new SpeechLine (button.hotspotLabelID, "", button.title, hotspotLabel, languages.Count - 1, AC_TextType.MenuElement);
				
				int lineID = SmartAddLine (existingLine);
				if (lineID >= 0) button.hotspotLabelID = lineID;
			}
		}
		
		
		private void ExtractJournalElement (MenuJournal journal, List<JournalPage> pages, bool onlySeekNew)
		{
			foreach (JournalPage page in pages)
			{
				if (onlySeekNew && page.lineID == -1)
				{
					// Assign a new ID on creation
					SpeechLine newLine;
					newLine = new SpeechLine (GetEmptyID (), "", journal.title, page.text, languages.Count - 1, AC_TextType.JournalEntry);
					page.lineID = newLine.lineID;
					lines.Add (newLine);
				}
				
				else if (!onlySeekNew && page.lineID > -1)
				{
					// Already has an ID, so don't replace
					SpeechLine existingLine = new SpeechLine (page.lineID, "", journal.title, page.text, languages.Count - 1, AC_TextType.JournalEntry);
					
					int lineID = SmartAddLine (existingLine);
					if (lineID >= 0) page.lineID = lineID;
				}
			}
		}
		
		
		private void ExtractSpeech (ActionSpeech action, bool onlySeekNew, bool isInScene, int tagID, string actionListName)
		{
			string speaker = "";
			bool isPlayer = action.isPlayer;
			if (!isPlayer && action.speaker != null && action.speaker is Player)
			{
				isPlayer = true;
			}
			
			if (isPlayer)
			{
				speaker = "Player";

				if (action.isPlayer && KickStarter.settingsManager != null && KickStarter.settingsManager.player)
				{
					speaker = KickStarter.settingsManager.player.name;
				}
				else if (!action.isPlayer && action.speaker != null)
				{
					speaker = action.speaker.name;
				}
			}
			else
			{
				if (!isInScene)
				{
					action.SetSpeaker ();
				}

				if (action.speaker)
				{
					speaker = action.speaker.name;
				}
				else
				{
					speaker = "Narrator";
				}
			}

			if (!string.IsNullOrEmpty (action.comment))
			{
				PromptCommentTransfer ();
			}

			string comment = (transferComment == TransferComment.Yes && !string.IsNullOrEmpty (action.comment)) ? action.comment : "";

			if (speaker != "" && action.messageText != "")
			{
				if (separateLines)
				{
					string[] messages = action.GetSpeechArray ();
					if (messages != null && messages.Length > 0)
					{
						action.lineID = ProcessSpeechLine (onlySeekNew, isInScene, action.lineID, speaker, messages[0], isPlayer, tagID, comment, actionListName);

						if (messages.Length > 1)
						{
							if (action.multiLineIDs == null || action.multiLineIDs.Length != (messages.Length - 1))
							{
								List<int> lineIDs = new List<int>();
								for (int i=1; i<messages.Length; i++)
								{
									if (action.multiLineIDs != null && action.multiLineIDs.Length > (i-1))
									{
										lineIDs.Add (action.multiLineIDs[i-1]);
									}
									else
									{
										lineIDs.Add (-1);
									}
								}
								action.multiLineIDs = lineIDs.ToArray ();
							}

							for (int i=1; i<messages.Length; i++)
							{
								action.multiLineIDs [i-1] = ProcessSpeechLine (onlySeekNew, isInScene, action.multiLineIDs [i-1], speaker, messages[i], isPlayer, tagID, comment, actionListName);
							}
						}
						else
						{
							action.multiLineIDs = null;
						}
					}
				}
				else
				{
					action.lineID = ProcessSpeechLine (onlySeekNew, isInScene, action.lineID, speaker, action.messageText, isPlayer, tagID, comment, actionListName);
				}
			}
			else
			{
				// Remove from SpeechManager
				action.lineID = -1;
				action.multiLineIDs = null;
			}
		}


		private void PromptCommentTransfer ()
		{
			if (transferComment == TransferComment.NotAsked)
			{
				bool canTransfer = EditorUtility.DisplayDialog ("Transfer speech comments", "One or more 'Dialogue: Play speech' Actions have been found with comments embedded.\r\nWould you like to transfer them all to the Speech Manager as line descriptions?\r\nIf not, line descriptions will be set to the name of the ActionList they're placed in.", "Yes", "No");
				transferComment = (canTransfer) ? TransferComment.Yes : TransferComment.No;
			}
		}


		private int ProcessSpeechLine (bool onlySeekNew, bool isInScene, int lineID, string speaker, string messageText, bool isPlayer, int tagID, string description, string actionListName)
		{
			actionListName = "From: " + actionListName;

			if (onlySeekNew && lineID == -1)
			{
				// Assign a new ID on creation
				string _scene = "";
				SpeechLine newLine;
				if (isInScene)
				{
					_scene = UnityVersionHandler.GetCurrentSceneName ();
				}
				newLine = new SpeechLine (GetEmptyID (), _scene, speaker, messageText, languages.Count - 1, AC_TextType.Speech, isPlayer);
				newLine.tagID = tagID;
				newLine.TransferActionComment (description, actionListName);

				lineID = newLine.lineID;
				lines.Add (newLine);
			}
			else if (!onlySeekNew && lineID > -1)
			{
				// Already has an ID, so don't replace
				string _scene = "";
				SpeechLine existingLine;
				if (isInScene)
				{
					_scene = UnityVersionHandler.GetCurrentSceneName ();
				}
				existingLine = new SpeechLine (lineID, _scene, speaker, messageText, languages.Count - 1, AC_TextType.Speech, isPlayer);
				existingLine.tagID = tagID;
				existingLine.TransferActionComment (description, actionListName);

				int _lineID = SmartAddLine (existingLine);
				if (_lineID >= 0) lineID = _lineID;
			}
			return lineID;
		}


		private void ExtractHotspotName (ActionRename action, bool onlySeekNew, bool isInScene)
		{
			if (action.newName != "")
			{
				string _scene = "";
				if (isInScene)
				{
					_scene = UnityVersionHandler.GetCurrentSceneName ();
				}

				if (onlySeekNew && action.lineID == -1)
				{
					// Assign a new ID on creation
					SpeechLine newLine = new SpeechLine (GetEmptyID (), _scene, action.newName, languages.Count - 1, AC_TextType.Hotspot);

					action.lineID = newLine.lineID;
					lines.Add (newLine);
				}
				
				else if (!onlySeekNew && action.lineID > -1)
				{
					// Already has an ID, so don't replace
					SpeechLine existingLine = new SpeechLine (action.lineID, _scene, action.newName, languages.Count - 1, AC_TextType.Hotspot);

					int lineID = SmartAddLine (existingLine);
					if (lineID >= 0) action.lineID = lineID;
				}
			}
			else
			{
				// Remove from SpeechManager
				action.lineID = -1;
			}
		}


		private void ExtractCharacterName (ActionCharRename action, bool onlySeekNew, bool isInScene)
		{
			if (action.newName != "")
			{
				string _scene = "";
				if (isInScene)
				{
					_scene = UnityVersionHandler.GetCurrentSceneName ();
				}
				
				if (onlySeekNew && action.lineID == -1)
				{
					// Assign a new ID on creation
					SpeechLine newLine = new SpeechLine (GetEmptyID (), _scene, action.newName, languages.Count - 1, AC_TextType.Character);
					
					action.lineID = newLine.lineID;
					lines.Add (newLine);
				}
				
				else if (!onlySeekNew && action.lineID > -1)
				{
					// Already has an ID, so don't replace
					SpeechLine existingLine = new SpeechLine (action.lineID, _scene, action.newName, languages.Count - 1, AC_TextType.Character);
					
					int lineID = SmartAddLine (existingLine);
					if (lineID >= 0) action.lineID = lineID;
				}
			}
			else
			{
				// Remove from SpeechManager
				action.lineID = -1;
			}
		}

		
		private int SmartAddLine (SpeechLine existingLine)
		{
			if (DoLinesMatchID (existingLine.lineID))
			{
				// Same ID, different text, so re-assign ID
				int lineID = GetEmptyID ();

				ACDebug.LogWarning ("Conflicting ID number (" + existingLine.lineID + ") found with '"  + existingLine.text + "'. Changing to " + lineID + ".");
				existingLine.lineID = lineID;
				lines.Add (existingLine);
				return lineID;
			}
			else
			{
				lines.Add (existingLine);
			}
			return -1;
		}
		
		
		private bool DoLinesMatchID (int newLineID)
		{
			if (lines == null || lines.Count == 0)
			{
				return false;
			}
			
			foreach (SpeechLine line in lines)
			{
				if (line.lineID == newLineID)
				{
					return true;
				}
			}

			return false;
		}

		
		private void ExtractJournalEntry (ActionMenuState action, bool onlySeekNew, bool isInScene)
		{
			if (action.changeType == ActionMenuState.MenuChangeType.AddJournalPage && action.journalText != "")
			{
				if (onlySeekNew && action.lineID == -1)
				{
					// Assign a new ID on creation
					SpeechLine newLine;
					if (isInScene)
					{
						newLine = new SpeechLine (GetEmptyID (), UnityVersionHandler.GetCurrentSceneName (), action.journalText, languages.Count - 1, AC_TextType.JournalEntry);
					}
					else
					{
						newLine = new SpeechLine (GetEmptyID (), "", action.journalText, languages.Count - 1, AC_TextType.JournalEntry);
					}
					action.lineID = newLine.lineID;
					lines.Add (newLine);
				}
				
				else if (!onlySeekNew && action.lineID > -1)
				{
					// Already has an ID, so don't replace
					SpeechLine existingLine;
					if (isInScene)
					{
						existingLine = new SpeechLine (action.lineID, UnityVersionHandler.GetCurrentSceneName (), action.journalText, languages.Count - 1, AC_TextType.JournalEntry);
					}
					else
					{
						existingLine = new SpeechLine (action.lineID, "", action.journalText, languages.Count - 1, AC_TextType.JournalEntry);
					}
					
					int lineID = SmartAddLine (existingLine);
					if (lineID >= 0) action.lineID = lineID;
				}
			}
			else
			{
				// Remove from SpeechManager
				action.lineID = -1;
			}
		}


		private void ExtractDialogOption (ActionDialogOptionRename action, bool onlySeekNew, bool isInScene)
		{
			if (onlySeekNew && action.lineID == -1)
			{
				// Assign a new ID on creation
				SpeechLine newLine;
				if (isInScene)
				{
					newLine = new SpeechLine (GetEmptyID (), UnityVersionHandler.GetCurrentSceneName (), action.newLabel, languages.Count - 1, AC_TextType.DialogueOption);
				}
				else
				{
					newLine = new SpeechLine (GetEmptyID (), "", action.newLabel, languages.Count - 1, AC_TextType.DialogueOption);
				}
				action.lineID = newLine.lineID;
				lines.Add (newLine);
			}
			
			else if (!onlySeekNew && action.lineID > -1)
			{
				// Already has an ID, so don't replace
				SpeechLine existingLine;
				if (isInScene)
				{
					existingLine = new SpeechLine (action.lineID, UnityVersionHandler.GetCurrentSceneName (), action.newLabel, languages.Count - 1, AC_TextType.DialogueOption);
				}
				else
				{
					existingLine = new SpeechLine (action.lineID, "", action.newLabel, languages.Count - 1, AC_TextType.DialogueOption);
				}
				
				int lineID = SmartAddLine (existingLine);
				if (lineID >= 0) action.lineID = lineID;
			}
		}


		private void ExtractVariable (ActionVarSet action, bool onlySeekNew, bool isInScene)
		{
			if (!action.IsTranslatable ())
			{
				return;
			}

			if (onlySeekNew && action.lineID == -1)
			{
				// Assign a new ID on creation
				SpeechLine newLine;
				if (isInScene)
				{
					newLine = new SpeechLine (GetEmptyID (), UnityVersionHandler.GetCurrentSceneName (), action.stringValue, languages.Count - 1, AC_TextType.Variable);
				}
				else
				{
					newLine = new SpeechLine (GetEmptyID (), "", action.stringValue, languages.Count - 1, AC_TextType.Variable);
				}
				action.lineID = newLine.lineID;
				lines.Add (newLine);
			}
			
			else if (!onlySeekNew && action.lineID > -1)
			{
				// Already has an ID, so don't replace
				SpeechLine existingLine;
				if (isInScene)
				{
					existingLine = new SpeechLine (action.lineID, UnityVersionHandler.GetCurrentSceneName (), action.stringValue, languages.Count - 1, AC_TextType.Variable);
				}
				else
				{
					existingLine = new SpeechLine (action.lineID, "", action.stringValue, languages.Count - 1, AC_TextType.Variable);
				}
				
				int lineID = SmartAddLine (existingLine);
				if (lineID >= 0) action.lineID = lineID;
			}
		}


		private void GetLinesFromSettings (bool onlySeekNew)
		{
			SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;
			
			if (settingsManager)
			{
				if (settingsManager.playerSwitching == PlayerSwitching.Allow)
				{
					foreach (PlayerPrefab playerPrefab in settingsManager.players)
					{
						if (playerPrefab != null && playerPrefab.playerOb != null)
						{
							ExtractCharacter (playerPrefab.playerOb, onlySeekNew);
							EditorUtility.SetDirty (playerPrefab.playerOb);
						}
					}
				}
				else if (settingsManager.player)
				{
					ExtractCharacter (settingsManager.player, onlySeekNew);
					EditorUtility.SetDirty (settingsManager.player);
				}
			}
		}
		
		
		private void GetLinesFromInventory (bool onlySeekNew)
		{
			InventoryManager inventoryManager = AdvGame.GetReferences ().inventoryManager;
			
			if (inventoryManager)
			{
				ProcessInventoryProperties (inventoryManager.items, inventoryManager.invVars, onlySeekNew);
				
				// Item-specific events
				if (inventoryManager.items.Count > 0)
				{
					foreach (InvItem item in inventoryManager.items)
					{
						// Label
						ExtractInventory (item, onlySeekNew);

						// Prefixes
						if (item.overrideUseSyntax)
						{
							ExtractPrefix (item.hotspotPrefix1, onlySeekNew);
							ExtractPrefix (item.hotspotPrefix2, onlySeekNew);
						}
					}
				}

				EditorUtility.SetDirty (inventoryManager);
			}
		}


		private void GetLinesFromVariables (bool readGlobal, bool onlySeekNew)
		{
			if (readGlobal)
			{
				VariablesManager variablesManager = AdvGame.GetReferences ().variablesManager;
				if (variablesManager != null)
				{
					variablesManager.vars = ExtractVariables (variablesManager.vars, readGlobal, onlySeekNew);
					EditorUtility.SetDirty (variablesManager);
				}
			}
			else
			{
				LocalVariables localVariables = GameObject.FindObjectOfType (typeof (LocalVariables)) as LocalVariables;
				if (localVariables != null)
				{
					localVariables.localVars = ExtractVariables (localVariables.localVars, readGlobal, onlySeekNew);
					EditorUtility.SetDirty (localVariables);
				}
			}
		}


		private List<GVar> ExtractVariables (List<GVar> vars, bool readGlobal, bool onlySeekNew)
		{
			string sceneName = (readGlobal) ? "" : UnityVersionHandler.GetCurrentSceneName ();

			foreach (GVar _var in vars)
			{
				if (!_var.canTranslate)
				{
					continue;
				}
				if (_var.type == VariableType.String)
				{
					if (onlySeekNew && _var.textValLineID == -1)
					{
						// Assign a new ID on creation
						SpeechLine newLine = new SpeechLine (GetEmptyID (), sceneName, _var.textVal, languages.Count - 1, AC_TextType.Variable);
						
						_var.textValLineID = newLine.lineID;
						lines.Add (newLine);
					}
					else if (!onlySeekNew && _var.textValLineID > -1)
					{
						// Already has an ID, so don't replace
						SpeechLine existingLine = new SpeechLine (_var.textValLineID, sceneName, _var.textVal, languages.Count - 1, AC_TextType.Variable);
						
						int lineID = SmartAddLine (existingLine);
						if (lineID >= 0) _var.textValLineID = lineID;
					}
				}
				else if (_var.type == VariableType.PopUp)
				{
					if (onlySeekNew && _var.popUpsLineID == -1)
					{
						// Assign a new ID on creation
						SpeechLine newLine = new SpeechLine (GetEmptyID (), sceneName, _var.GetPopUpsString (), languages.Count - 1, AC_TextType.Variable);
						
						_var.popUpsLineID = newLine.lineID;
						lines.Add (newLine);
					}
					else if (!onlySeekNew && _var.popUpsLineID > -1)
					{
						// Already has an ID, so don't replace
						SpeechLine existingLine = new SpeechLine (_var.popUpsLineID, sceneName, _var.GetPopUpsString (), languages.Count - 1, AC_TextType.Variable);
						
						int lineID = SmartAddLine (existingLine);
						if (lineID >= 0) _var.popUpsLineID = lineID;
					}
				}
			}
			return vars;
		}
		
		
		private void GetLinesFromMenus (bool onlySeekNew)
		{
			MenuManager menuManager = AdvGame.GetReferences ().menuManager;
			
			if (menuManager)
			{
				// Gather elements
				if (menuManager.menus.Count > 0)
				{
					foreach (AC.Menu menu in menuManager.menus)
					{
						foreach (MenuElement element in menu.elements)
						{
							if (element is MenuButton)
							{
								MenuButton menuButton = (MenuButton) element;
								ExtractElement (element, menuButton.label, onlySeekNew);
								ExtractHotspotOverride (menuButton, menuButton.hotspotLabel, onlySeekNew);
							}
							else if (element is MenuCycle)
							{
								MenuCycle menuCycle = (MenuCycle) element;
								ExtractElement (element, menuCycle.label, onlySeekNew);
							}
							else if (element is MenuDrag)
							{
								MenuDrag menuDrag = (MenuDrag) element;
								ExtractElement (element, menuDrag.label, onlySeekNew);
							}
							else if (element is MenuInput)
							{
								MenuInput menuInput = (MenuInput) element;
								ExtractElement (element, menuInput.label, onlySeekNew);
							}
							else if (element is MenuLabel)
							{
								MenuLabel menuLabel = (MenuLabel) element;
								if (menuLabel.CanTranslate ())
								{
									ExtractElement (element, menuLabel.label, onlySeekNew);
								}
								else
								{
									menuLabel.lineID = -1;
								}
							}
							else if (element is MenuSavesList)
							{
								MenuSavesList menuSavesList = (MenuSavesList) element;
								if (menuSavesList.saveListType == AC_SaveListType.Save && menuSavesList.showNewSaveOption)
								{
									ExtractElement (element, menuSavesList.newSaveText, onlySeekNew);
								}
							}
							else if (element is MenuSlider)
							{
								MenuSlider menuSlider = (MenuSlider) element;
								ExtractElement (element, menuSlider.label, onlySeekNew);
							}
							else if (element is MenuToggle)
							{
								MenuToggle menuToggle = (MenuToggle) element;
								ExtractElement (element, menuToggle.label, onlySeekNew);
								ExtractToggleElement (menuToggle, onlySeekNew);
							}
							else if (element is MenuJournal)
							{
								MenuJournal menuJournal = (MenuJournal) element;
								ExtractJournalElement (menuJournal, menuJournal.pages, onlySeekNew);
							}
						}
					}
				}
				
				EditorUtility.SetDirty (menuManager);
			}
		}
		
		
		private void GetLinesFromCursors (bool onlySeekNew)
		{
			CursorManager cursorManager = AdvGame.GetReferences ().cursorManager;
			
			if (cursorManager)
			{
				// Prefixes
				ExtractPrefix (cursorManager.hotspotPrefix1, onlySeekNew);
				ExtractPrefix (cursorManager.hotspotPrefix2, onlySeekNew);
				ExtractPrefix (cursorManager.hotspotPrefix3, onlySeekNew);
				ExtractPrefix (cursorManager.hotspotPrefix4, onlySeekNew);
				ExtractPrefix (cursorManager.walkPrefix, onlySeekNew);

				// Gather icons
				if (cursorManager.cursorIcons.Count > 0)
				{
					foreach (CursorIcon icon in cursorManager.cursorIcons)
					{
						ExtractIcon (icon, onlySeekNew);
					}
				}
				
				EditorUtility.SetDirty (cursorManager);
			}
		}
		
		
		private void GetLinesInScene (string sceneFile, bool onlySeekNew)
		{
			UnityVersionHandler.OpenScene (sceneFile);

			// Speech lines and journal entries
			ActionList[] actionLists = GameObject.FindObjectsOfType (typeof (ActionList)) as ActionList[];
			foreach (ActionList list in actionLists)
			{
				if (list.source == ActionListSource.InScene)
				{
					ProcessActionList (list, onlySeekNew);
				}
			}

			// Hotspots
			Hotspot[] hotspots = GameObject.FindObjectsOfType (typeof (Hotspot)) as Hotspot[];
			foreach (Hotspot hotspot in hotspots)
			{
				ExtractHotspot (hotspot, onlySeekNew);
				EditorUtility.SetDirty (hotspot);
			}

			// Characters
			AC.Char[] characters = GameObject.FindObjectsOfType (typeof (AC.Char)) as AC.Char[];
			foreach (AC.Char character in characters)
			{
				ExtractCharacter (character, onlySeekNew);
				EditorUtility.SetDirty (character);
			}
			
			// Dialogue options
			Conversation[] conversations = GameObject.FindObjectsOfType (typeof (Conversation)) as Conversation[];
			foreach (Conversation conversation in conversations)
			{
				ExtractConversation (conversation, onlySeekNew);
				EditorUtility.SetDirty (conversation);
			}

			// Local variables
			GetLinesFromVariables (false, onlySeekNew);

			// Save the scene
			UnityVersionHandler.SaveScene ();
			EditorUtility.SetDirty (this);
		}

		
		
		private void RestoreTranslations ()
		{
			// Match IDs for each entry in lines and tempLines, send over translation data
			foreach (SpeechLine tempLine in tempLines)
			{
				foreach (SpeechLine line in lines)
				{
					if (tempLine.lineID == line.lineID)
					{
						line.RestoreBackup (tempLine);
						break;
					}
				}
			}
			
			tempLines = null;
		}
		
		
		private void BackupTranslations ()
		{
			tempLines = new List<SpeechLine>();
			foreach (SpeechLine line in lines)
			{
				tempLines.Add (line);
			}
		}


		private void ImportTranslation (int i)
		{
			bool canProceed = EditorUtility.DisplayDialog ("Import translation", "AC will now prompt you for a CSV file to import. It is recommended to back up your project beforehand.", "OK", "Cancel");
			if (!canProceed) return;

			string fileName = EditorUtility.OpenFilePanel ("Import all game text data", "Assets", "csv");
			if (string.IsNullOrEmpty (fileName))
			{
				return;
			}
			
			if (File.Exists (fileName))
			{
				string csvText = Serializer.LoadFile (fileName);
				string [,] csvOutput = CSVReader.SplitCsvGrid (csvText);

				ImportWizardWindow.Init (this, csvOutput, i);
			}
		}


		private void UpdateTranslation (int i, int _lineID, string translationText)
		{
			foreach (SpeechLine line in lines)
			{
				if (line.lineID == _lineID)
				{
					line.translationText [i-1] = translationText;
					return;
				}
			}
		}


		private void ExportGameText ()
		{
			ExportWizardWindow.Init (this);
		}


		private void ImportGameText ()
		{
			bool canProceed = EditorUtility.DisplayDialog ("Import game text", "AC will now prompt you for a CSV file to import. It is recommended to back up your project beforehand.", "OK", "Cancel");
			if (!canProceed) return;

			string fileName = EditorUtility.OpenFilePanel ("Import all game text data", "Assets", "csv");
			if (fileName.Length == 0)
			{
				return;
			}
			
			if (File.Exists (fileName))
			{
				string csvText = Serializer.LoadFile (fileName);
				string [,] csvOutput = CSVReader.SplitCsvGrid (csvText);

				ImportWizardWindow.Init (this, csvOutput);
			}
		}


		private void ClearList ()
		{
			if (EditorUtility.DisplayDialog ("Reset game text", "This will completely reset the IDs of every text line in your game, removing any supplied translations and invalidating speech audio filenames. Continue?", "OK", "Cancel"))
			{
				string originalScene = UnityVersionHandler.GetCurrentSceneFilepath ();
				
				if (UnityVersionHandler.SaveSceneIfUserWants ())
				{
					lines.Clear ();
					checkedAssets.Clear ();
					
					sceneFiles = AdvGame.GetSceneFiles ();
					GetSceneNames ();
					
					// First look for lines that already have an assigned lineID
					foreach (string sceneFile in sceneFiles)
					{
						ClearLinesInScene (sceneFile);
					}

					CollectAllActionListAssets ();
					foreach (ActionListAsset actionListAsset in allActionListAssets)
					{
						ClearLinesFromActionListAsset (actionListAsset);
					}

					ClearLinesFromInventory ();
					ClearLinesFromCursors ();
					ClearLinesFromMenus ();
					
					checkedAssets.Clear ();

					if (originalScene == "")
					{
						UnityVersionHandler.NewScene ();
					}
					else
					{
						UnityVersionHandler.OpenScene (originalScene);
					}

					allActionListAssets.Clear ();
					maxID = -1;
					EditorUtility.DisplayDialog ("Reset game text", "Process complete.", "OK");
				}
			}
		}
		
		
		private void ClearLinesInScene (string sceneFile)
		{
			UnityVersionHandler.OpenScene (sceneFile);
			
			// Speech lines and journal entries
			ActionList[] actionLists = GameObject.FindObjectsOfType (typeof (ActionList)) as ActionList[];
			foreach (ActionList list in actionLists)
			{
				if (list.source == ActionListSource.InScene)
				{
					ClearLinesFromActionList (list);
				}
			}
			
			// Hotspots
			Hotspot[] hotspots = GameObject.FindObjectsOfType (typeof (Hotspot)) as Hotspot[];
			foreach (Hotspot hotspot in hotspots)
			{
				hotspot.lineID = -1;
				EditorUtility.SetDirty (hotspot);
			}
			
			// Dialogue options
			Conversation[] conversations = GameObject.FindObjectsOfType (typeof (Conversation)) as Conversation[];
			foreach (Conversation conversation in conversations)
			{
				foreach (ButtonDialog dialogOption in conversation.options)
				{
					dialogOption.lineID = -1;
				}
				EditorUtility.SetDirty (conversation);
			}
			
			// Save the scene
			UnityVersionHandler.SaveScene ();
			EditorUtility.SetDirty (this);
		}
		
		
		private void ClearLinesFromActionListAsset (ActionListAsset actionListAsset)
		{
			if (actionListAsset != null && !checkedAssets.Contains (actionListAsset))
			{
				checkedAssets.Add (actionListAsset);
				ClearLines (actionListAsset.actions);
				EditorUtility.SetDirty (actionListAsset);
			}
		}
		
		
		private void ClearLinesFromActionList (ActionList actionList)
		{
			if (actionList != null)
			{
				ClearLines (actionList.actions);
				EditorUtility.SetDirty (actionList);
			}
		}
		
		
		private void ClearLines (List<Action> actions)
		{
			if (actions == null)
			{
				return;
			}
			
			foreach (Action action in actions)
			{
				if (action == null)
				{
					continue;
				}

				if (action is ActionSpeech)
				{
					ActionSpeech actionSpeech = (ActionSpeech) action;
					actionSpeech.lineID = -1;
				}
				else if (action is ActionRename)
				{
					ActionRename actionRename = (ActionRename) action;
					actionRename.lineID = -1;
				}
				else if (action is ActionCharRename)
				{
					ActionCharRename actionCharRename = (ActionCharRename) action;
					actionCharRename.lineID = -1;
				}
				else if (action is ActionMenuState)
				{
					ActionMenuState actionMenuState = (ActionMenuState) action;
					actionMenuState.lineID = -1;
				}
				else if (action is ActionDialogOptionRename)
				{
					ActionDialogOptionRename actionDialogOptionRename = (ActionDialogOptionRename) action;
					actionDialogOptionRename.lineID = -1;
				}
				else if (action is ActionVarSet)
				{
					ActionVarSet actionVarSet = (ActionVarSet) action;
					actionVarSet.lineID = -1;
				}
			}
			
		}

		
		private void ClearLinesFromInventory ()
		{
			InventoryManager inventoryManager = AdvGame.GetReferences ().inventoryManager;
			
			if (inventoryManager != null && inventoryManager.items.Count > 0)
			{
				foreach (InvItem item in inventoryManager.items)
				{
					// Label
					item.lineID = -1;
				}
			}
				
			EditorUtility.SetDirty (inventoryManager);
		}
		
		
		private void ClearLinesFromCursors ()
		{
			CursorManager cursorManager = AdvGame.GetReferences ().cursorManager;
			
			if (cursorManager)
			{
				// Prefixes
				cursorManager.hotspotPrefix1.lineID = -1;
				cursorManager.hotspotPrefix2.lineID = -1;
				cursorManager.hotspotPrefix3.lineID = -1;
				cursorManager.hotspotPrefix4.lineID = -1;
				cursorManager.walkPrefix.lineID = -1;
				
				// Gather icons
				if (cursorManager.cursorIcons.Count > 0)
				{
					foreach (CursorIcon icon in cursorManager.cursorIcons)
					{
						icon.lineID = -1;
					}
				}
				
				EditorUtility.SetDirty (cursorManager);
			}
		}
		
		
		private void ClearLinesFromMenus ()
		{
			MenuManager menuManager = AdvGame.GetReferences ().menuManager;
			
			if (menuManager)
			{
				// Gather elements
				if (menuManager.menus.Count > 0)
				{
					foreach (AC.Menu menu in menuManager.menus)
					{
						foreach (MenuElement element in menu.elements)
						{
							if (element is MenuButton)
							{
								MenuButton menuButton = (MenuButton) element;
								menuButton.lineID = -1;
								menuButton.hotspotLabelID = -1;
							}
							else if (element is MenuCycle)
							{
								MenuCycle menuCycle = (MenuCycle) element;
								menuCycle.lineID = -1;
							}
							else if (element is MenuDrag)
							{
								MenuDrag menuDrag = (MenuDrag) element;
								menuDrag.lineID = -1;
							}
							else if (element is MenuInput)
							{
								MenuInput menuInput = (MenuInput) element;
								menuInput.lineID = -1;
							}
							else if (element is MenuLabel)
							{
								MenuLabel menuLabel = (MenuLabel) element;
								menuLabel.lineID = -1;
							}
							else if (element is MenuSavesList)
							{
								MenuSavesList menuSavesList = (MenuSavesList) element;
								menuSavesList.lineID = -1;
							}
							else if (element is MenuSlider)
							{
								MenuSlider menuSlider = (MenuSlider) element;
								menuSlider.lineID = -1;
							}
							else if (element is MenuToggle)
							{
								MenuToggle menuToggle = (MenuToggle) element;
								menuToggle.lineID = -1;
							}
							else if (element is MenuJournal)
							{
								MenuJournal menuJournal = (MenuJournal) element;
								menuJournal.lineID = -1;
							}
						}
					}
				}
				
				EditorUtility.SetDirty (menuManager);
			}		
		}


		private void MergeMatchingSpeechIDs ()
		{
			if (lines == null || lines.Count == 0) return;

			List<SpeechLine> linesToCheck = new List<SpeechLine>();
			foreach (SpeechLine line in lines)
			{
				if (line.textType == AC_TextType.Speech)
				{
					linesToCheck.Add (line);
				}
			}

			if (linesToCheck.Count <= 1) return;

			CollectAllActionListAssets ();

			while (linesToCheck.Count > 0)
			{
				SpeechLine lineToCheck = linesToCheck[0];
				for (int i=1; i<linesToCheck.Count; i++)
				{
					if (linesToCheck[i].IsMatch (lineToCheck, true))
					{
						// Found a match
						SpeechLine matchingLine = linesToCheck[i];
						int originalID = matchingLine.lineID;
						int newID = lineToCheck.lineID;
						matchingLine.lineID = newID;

						if (matchingLine.customAudioClip != null && lineToCheck.customAudioClip == null)
						{
							lineToCheck.customAudioClip = matchingLine.customAudioClip;
							lineToCheck.customLipsyncFile = matchingLine.customLipsyncFile;
							lineToCheck.customTranslationAudioClips = matchingLine.customTranslationAudioClips;
							lineToCheck.customTranslationLipsyncFiles = matchingLine.customTranslationLipsyncFiles;
						}
						
						// Update ActionSpeech
						if (matchingLine.scene == "")
						{
							// In an asset
							foreach (ActionListAsset actionListAsset in allActionListAssets)
							{
								foreach (Action action in actionListAsset.actions)
								{
									if (action != null && action is ActionSpeech)
									{
										ActionSpeech actionSpeech = (ActionSpeech) action;
										if (actionSpeech.lineID == originalID)
										{
											actionSpeech.lineID = newID;
											EditorUtility.SetDirty (actionListAsset);
										}
									}
								}
							}
						}
						else
						{
							// In a scene
							foreach (string sceneFile in sceneFiles)
							{
								if (sceneFile.EndsWith (matchingLine.scene + ".unity"))
								{
									UnityVersionHandler.OpenScene (sceneFile);
									ActionList[] actionLists = GameObject.FindObjectsOfType (typeof (ActionList)) as ActionList[];
									foreach (ActionList actionList in actionLists)
									{
										if (actionList.source == ActionListSource.InScene)
										{
											foreach (Action action in actionList.actions)
											{
												if (action != null && action is ActionSpeech)
												{
													ActionSpeech actionSpeech = (ActionSpeech) action;
													if (actionSpeech.lineID == originalID)
													{
														actionSpeech.lineID = newID;
														EditorUtility.SetDirty (actionList);
													}
												}
											}
										}
									}

									UnityVersionHandler.SaveScene ();
								}
							}
						}

						lines.Remove (matchingLine);
					}

				}

				linesToCheck.RemoveAt (0);
			}

			allActionListAssets.Clear ();
			EditorUtility.SetDirty (this);
			AssetDatabase.SaveAssets ();
		}


		/**
		 * <summary>Gets all ActionList assets referenced by scenes, Managers and other asset files in the project</summary>
		 * <returns>All ActionList assets referenced by scenes, Managers and other asset files in the project</returns>
		 */
		public ActionListAsset[] GetAllActionListAssets ()
		{
			CollectAllActionListAssets ();
			return allActionListAssets.ToArray ();
		}


		private void CollectAllActionListAssets ()
		{
			allActionListAssets = new List<ActionListAsset>();

			// Search scenes
			foreach (string sceneFile in sceneFiles)
			{
				UnityVersionHandler.OpenScene (sceneFile);

				// ActionLists
				ActionList[] actionLists = GameObject.FindObjectsOfType (typeof (ActionList)) as ActionList[];
				foreach (ActionList actionList in actionLists)
				{
					if (actionList.useParameters && actionList.parameters != null)
					{
						if (!actionList.syncParamValues && actionList.source == ActionListSource.AssetFile && actionList.assetFile != null && actionList.assetFile.useParameters)
						{
							foreach (ActionParameter parameter in actionList.parameters)
							{
								if (parameter.parameterType == ParameterType.UnityObject)
								{
									if (parameter.objectValue != null)
									{
										if (parameter.objectValue is ActionListAsset)
										{
											ActionListAsset _actionListAsset = (ActionListAsset) parameter.objectValue;
											SmartAddAsset (_actionListAsset);
										}
									}
								}
							}
						}
					}

					if (actionList.source == ActionListSource.AssetFile)
					{
						SmartAddAsset (actionList.assetFile);
					}
					else
					{
						GetActionListAssetsFromActions (actionList.actions);
					}
				}

				// Hotspots
				Hotspot[] hotspots = GameObject.FindObjectsOfType (typeof (Hotspot)) as Hotspot[];
				foreach (Hotspot hotspot in hotspots)
				{
					if (hotspot.interactionSource == InteractionSource.AssetFile)
					{
						SmartAddAsset (hotspot.useButton.assetFile);
						SmartAddAsset (hotspot.lookButton.assetFile);
						SmartAddAsset (hotspot.unhandledInvButton.assetFile);

						foreach (Button _button in hotspot.useButtons)
						{
							SmartAddAsset (_button.assetFile);
						}
						
						foreach (Button _button in hotspot.invButtons)
						{
							SmartAddAsset (_button.assetFile);
						}
					}
				}
				
				// Dialogue options
				Conversation[] conversations = GameObject.FindObjectsOfType (typeof (Conversation)) as Conversation[];
				foreach (Conversation conversation in conversations)
				{
					foreach (ButtonDialog dialogOption in conversation.options)
					{
						SmartAddAsset (dialogOption.assetFile);
					}
					EditorUtility.SetDirty (conversation);
				}
			}

			// Settings Manager
			SettingsManager settingsManager = AdvGame.GetReferences ().settingsManager;
			if (settingsManager)
			{
				SmartAddAsset (settingsManager.actionListOnStart);
				if (settingsManager.activeInputs != null)
				{
					foreach (ActiveInput activeInput in settingsManager.activeInputs)
					{
						SmartAddAsset (activeInput.actionListAsset);
					}
				}
			}

			// Inventory Manager
			InventoryManager inventoryManager = AdvGame.GetReferences ().inventoryManager;
			if (inventoryManager)
			{
				SmartAddAsset (inventoryManager.unhandledCombine);
				SmartAddAsset (inventoryManager.unhandledHotspot);
				SmartAddAsset (inventoryManager.unhandledGive);

				if (inventoryManager.items.Count > 0)
				{
					foreach (InvItem item in inventoryManager.items)
					{
						SmartAddAsset (item.useActionList);
						SmartAddAsset (item.lookActionList);
						SmartAddAsset (item.unhandledActionList);
						SmartAddAsset (item.unhandledCombineActionList);

						foreach (InvInteraction invInteraction in item.interactions)
						{
							SmartAddAsset (invInteraction.actionList);
						}

						foreach (ActionListAsset actionList in item.combineActionList)
						{
							SmartAddAsset (actionList);
						}
					}
				}
				foreach (Recipe recipe in inventoryManager.recipes)
				{
					SmartAddAsset (recipe.invActionList);
					SmartAddAsset (recipe.actionListOnCreate);
				}
			}

			// Cursor Manager
			CursorManager cursorManager = AdvGame.GetReferences ().cursorManager;
			if (cursorManager)
			{
				foreach (ActionListAsset actionListAsset in cursorManager.unhandledCursorInteractions)
				{
					SmartAddAsset (actionListAsset);
				}
			}

			// Menu Manager
			MenuManager menuManager = AdvGame.GetReferences ().menuManager;
			if (menuManager)
			{
				if (menuManager.menus.Count > 0)
				{
					foreach (AC.Menu menu in menuManager.menus)
					{
						SmartAddAsset (menu.actionListOnTurnOn);
						SmartAddAsset (menu.actionListOnTurnOff);

						foreach (MenuElement element in menu.elements)
						{
							if (element is MenuButton)
							{
								MenuButton menuButton = (MenuButton) element;
								if (menuButton.buttonClickType == AC_ButtonClickType.RunActionList)
								{
									SmartAddAsset (menuButton.actionList);
								}
							}
							else if (element is MenuSavesList)
							{
								MenuSavesList menuSavesList = (MenuSavesList) element;
								SmartAddAsset (menuSavesList.actionListOnSave);
							}
							else if (element is MenuCycle)
							{
								MenuCycle menuCycle = (MenuCycle) element;
								SmartAddAsset (menuCycle.actionListOnClick);
							}
							else if (element is MenuJournal)
							{
								MenuJournal menuJournal = (MenuJournal) element;
								SmartAddAsset (menuJournal.actionListOnAddPage);
							}
							else if (element is MenuSlider)
							{
								MenuSlider menuSlider = (MenuSlider) element;
								SmartAddAsset (menuSlider.actionListOnChange);
							}
							else if (element is MenuToggle)
							{
								MenuToggle menuToggle = (MenuToggle) element;
								SmartAddAsset (menuToggle.actionListOnClick);
							}
							else if (element is MenuProfilesList)
							{
								MenuProfilesList menuProfilesList = (MenuProfilesList) element;
								SmartAddAsset (menuProfilesList.actionListOnClick);
							}
						}
					}
				}
			}
		}


		private void SmartAddAsset (ActionListAsset asset)
		{
			if (asset != null)
			{
				if (allActionListAssets.Contains (asset))
				{
					return;
				}

				allActionListAssets.Add (asset);
				GetActionListAssetsFromActions (asset.actions);
			}
		}


		private void GetActionListAssetsFromActions (List<Action> actions)
		{
			if (actions != null)
			{
				foreach (Action action in actions)
				{
					if (action == null) continue;

					if (action is ActionRunActionList)
					{
						ActionRunActionList actionRunActionList = (ActionRunActionList) action;
						if (actionRunActionList.listSource == ActionRunActionList.ListSource.AssetFile)
						{
							SmartAddAsset (actionRunActionList.invActionList);
						}

						if ((actionRunActionList.actionList != null && actionRunActionList.actionList.useParameters) ||
							(actionRunActionList.linkedAsset != null && actionRunActionList.linkedAsset.useParameters))
						{
							if (actionRunActionList.localParameters != null)
							{
								foreach (ActionParameter localParameter in actionRunActionList.localParameters)
								{
									if (localParameter.parameterType == ParameterType.UnityObject)
									{
										if (localParameter.objectValue != null)
										{
											if (localParameter.objectValue is ActionListAsset)
											{
												ActionListAsset _actionListAsset = (ActionListAsset) localParameter.objectValue;
												SmartAddAsset (_actionListAsset);
											}
										}
									}
								}
							}
						}
					}

					if (action is ActionParamSet)
					{
						ActionParamSet actionParamSet = (ActionParamSet) action;
						if (actionParamSet.setParamMethod == SetParamMethod.EnteredHere)
						{
							if (actionParamSet.unityObjectValue != null)
							{
								if (actionParamSet.unityObjectValue is ActionListAsset)
								{
									ActionListAsset _actionListAsset = (ActionListAsset) actionParamSet.unityObjectValue;
									SmartAddAsset (_actionListAsset);
								}
							}
						}
					}
					
					if (action is ActionCheck)
					{
						ActionCheck actionCheck = (ActionCheck) action;
						if (actionCheck.resultActionTrue == ResultAction.RunCutscene)
						{
							SmartAddAsset (actionCheck.linkedAssetTrue);
						}
						if (actionCheck.resultActionFail == ResultAction.RunCutscene)
						{
							SmartAddAsset (actionCheck.linkedAssetFail);
						}
					}
					else if (action is ActionCheckMultiple)
					{
						ActionCheckMultiple actionCheckMultiple = (ActionCheckMultiple) action;
						foreach (ActionEnd ending in actionCheckMultiple.endings)
						{
							if (ending.resultAction == ResultAction.RunCutscene)
							{
								SmartAddAsset (ending.linkedAsset);
							}
						}
					}
					else if (action is ActionParallel)
					{
						ActionParallel actionParallel = (ActionParallel) action;
						foreach (ActionEnd ending in actionParallel.endings)
						{
							if (ending.resultAction == ResultAction.RunCutscene)
							{
								SmartAddAsset (ending.linkedAsset);
							}
						}
					}
					else
					{
						if (action != null && action.endAction == ResultAction.RunCutscene)
						{
							SmartAddAsset (action.linkedAsset);
						}
					}
				}
			}
		}


		private void ProcessInventoryProperties (List<InvItem> items, List<InvVar> vars, bool onlySeekNew)
		{
			foreach (InvItem item in items)
			{
				foreach (InvVar var in item.vars)
				{
					if (var.type == VariableType.String)
					{
						if (onlySeekNew && var.textValLineID == -1)
						{
							// Assign a new ID on creation
							SpeechLine newLine = new SpeechLine (GetEmptyID (), "", var.textVal, languages.Count - 1, AC_TextType.InventoryItemProperty);
							
							var.textValLineID = newLine.lineID;
							lines.Add (newLine);
						}
						else if (!onlySeekNew && var.textValLineID > -1)
						{
							// Already has an ID, so don't replace
							SpeechLine existingLine = new SpeechLine (var.textValLineID, "", var.textVal, languages.Count - 1, AC_TextType.InventoryItemProperty);
							
							int lineID = SmartAddLine (existingLine);
							if (lineID >= 0) var.textValLineID = lineID;
						}
					}
				}
			}

			foreach (InvVar var in vars)
			{
				if (onlySeekNew && var.popUpsLineID == -1)
				{
					// Assign a new ID on creation
					SpeechLine newLine = new SpeechLine (GetEmptyID (), "", var.GetPopUpsString (), languages.Count - 1, AC_TextType.InventoryItemProperty);
					
					var.popUpsLineID = newLine.lineID;
					lines.Add (newLine);
				}
				else if (!onlySeekNew && var.popUpsLineID > -1)
				{
					// Already has an ID, so don't replace
					SpeechLine existingLine = new SpeechLine (var.popUpsLineID, "", var.GetPopUpsString (), languages.Count - 1, AC_TextType.InventoryItemProperty);
					
					int lineID = SmartAddLine (existingLine);
					if (lineID >= 0) var.popUpsLineID = lineID;
				}
			}
		}
		
		
		private void ProcessActionListAsset (ActionListAsset actionListAsset, bool onlySeekNew)
		{
			if (actionListAsset != null && !checkedAssets.Contains (actionListAsset))
			{
				checkedAssets.Add (actionListAsset);
				ProcessActions (actionListAsset.actions, onlySeekNew, false, actionListAsset.tagID, actionListAsset.name, actionListAsset.GetHashCode ());
				EditorUtility.SetDirty (actionListAsset);
			}
		}
		
		
		private void ProcessActionList (ActionList actionList, bool onlySeekNew)
		{
			if (actionList != null)
			{
				ProcessActions (actionList.actions, onlySeekNew, true, actionList.tagID, actionList.name, actionList.GetHashCode ());
				EditorUtility.SetDirty (actionList);
			}
			
		}
		
		
		private void ProcessActions (List<Action> actions, bool onlySeekNew, bool isInScene, int tagID, string actionListName, int hashCode)
		{
			foreach (Action action in actions)
			{
				if (action == null)
				{
					continue;
				}

				action.name = action.name.Replace ("(Clone)", "");

				if (action is ActionSpeech)
				{
					ExtractSpeech (action as ActionSpeech, onlySeekNew, isInScene, tagID, actionListName);
				}
				else if (action is ActionRename)
				{
					ExtractHotspotName (action as ActionRename, onlySeekNew, isInScene);
				}
				else if (action is ActionCharRename)
				{
					ExtractCharacterName (action as ActionCharRename, onlySeekNew, isInScene);
				}
				else if (action is ActionMenuState)
				{
					ExtractJournalEntry (action as ActionMenuState, onlySeekNew, isInScene);
				}
				else if (action is ActionDialogOptionRename)
				{
					ExtractDialogOption (action as ActionDialogOptionRename, onlySeekNew, isInScene);
				}
				else if (action is ActionVarSet)
				{
					ExtractVariable (action as ActionVarSet, onlySeekNew, isInScene);
				}
			}

			if (onlySeekNew)
			{
				SetOrderIDs (actions, actionListName, hashCode);
			}
		}


		private void SetOrderIDs (List<Action> actions, string actionListName, int hashCode)
		{
			string prefix = actionListName + "_" + hashCode + "_";

			foreach (Action action in actions)
			{
				action.isMarked = true;
			}

			minOrderValue = 0;

			ArrangeFromIndex (actions, prefix, 0);

			minOrderValue ++;
			foreach (Action _action in actions)
			{
				if (_action.isMarked)
				{
					// Wasn't arranged
					_action.isMarked = false;

					if (_action is ActionSpeech)
					{
						ActionSpeech actionSpeech = (ActionSpeech) _action;
						SpeechLine speechLine = GetLine (actionSpeech.lineID);
						if (speechLine != null)
						{
							speechLine.orderID = minOrderValue;
							speechLine.orderPrefix = prefix;
							minOrderValue ++;
						}
					}
				}
			}
		}


		private void ArrangeFromIndex (List<Action> actionList, string prefix, int i)
		{
			while (i > -1 && actionList.Count > i)
			{
				Action _action = actionList[i];

				if (_action is ActionSpeech && _action.isMarked)
				{
					int yPos = minOrderValue;

					if (i > 0)
					{
						// Find top-most Y position
						bool doAgain = true;
						
						while (doAgain)
						{
							int numChanged = 0;
							foreach (Action otherAction in actionList)
							{
								if (otherAction is ActionSpeech && otherAction != _action)
								{
									ActionSpeech otherActionSpeech = (ActionSpeech) otherAction;
									SpeechLine otherSpeechLine = GetLine (otherActionSpeech.lineID);
									if (otherSpeechLine != null)
									{
										int orderOrderID = otherSpeechLine.orderID;

										if (orderOrderID >= yPos)
										{
											yPos ++;
											numChanged ++;
										}
									}
								}
							}
							
							if (numChanged == 0)
							{
								doAgain = false;
							}
						}
					}

					ActionSpeech actionSpeech = (ActionSpeech) _action;
					SpeechLine speechLine = GetLine (actionSpeech.lineID);
					if (speechLine != null)
					{
						speechLine.orderID = yPos;
						speechLine.orderPrefix = prefix;
					}

					if (yPos > minOrderValue)
					{
						minOrderValue = yPos;
					}
				}
				
				if (_action.isMarked == false)
				{
					return;
				}
				
				_action.isMarked = false;

				if (_action is ActionCheckMultiple)
				{
					ActionCheckMultiple _actionCheckMultiple = (ActionCheckMultiple) _action;
					
					for (int j=_actionCheckMultiple.endings.Count-1; j>=0; j--)
					{
						ActionEnd ending = _actionCheckMultiple.endings [j];
						if (j >= 0)
						{
							if (ending.resultAction == ResultAction.Skip)
							{
								ArrangeFromIndex (actionList, prefix, ending.skipAction); // actionList.IndexOf (ending.skipActionActual));
							}
							else if (ending.resultAction == ResultAction.Continue)
							{
								ArrangeFromIndex (actionList, prefix, i+1);
							}
						}
					}
				}
				else if (_action is ActionParallel)
				{
					ActionParallel _ActionParallel = (ActionParallel) _action;
					
					for (int j=_ActionParallel.endings.Count-1; j>=0; j--)
					{
						ActionEnd ending = _ActionParallel.endings [j];
						if (ending.resultAction == ResultAction.Skip)
						{
							ArrangeFromIndex (actionList, prefix, ending.skipAction); // actionList.IndexOf (ending.skipActionActual));
						}
						else if (ending.resultAction == ResultAction.Continue)
						{
							ArrangeFromIndex (actionList, prefix, i+1);
						}
					}

					i = -1;
				}
				else if (_action is ActionCheck)
				{
					ActionCheck _actionCheck = (ActionCheck) _action;

					if (_actionCheck.resultActionFail == ResultAction.Stop || _actionCheck.resultActionFail == ResultAction.RunCutscene)
					{
						if (_actionCheck.resultActionTrue == ResultAction.Skip)
						{
							i = _actionCheck.skipActionTrue; // actionList.IndexOf (_actionCheck.skipActionTrueActual);
						}
						else if (_actionCheck.resultActionTrue == ResultAction.Continue)
						{
							i++;
						}
						else
						{
							i = -1;
						}
					}
					else
					{
						if (_actionCheck.resultActionTrue == ResultAction.Skip)
						{
							ArrangeFromIndex (actionList, prefix, _actionCheck.skipActionTrue); // actionList.IndexOf (_actionCheck.skipActionTrueActual));
						}
						else if (_actionCheck.resultActionTrue == ResultAction.Continue)
						{
							ArrangeFromIndex (actionList, prefix, i+1);
						}
						
						if (_actionCheck.resultActionFail == ResultAction.Skip)
						{
							i = _actionCheck.skipActionFail; //actionList.IndexOf (_actionCheck.skipActionFailActual);
						}
						else if (_actionCheck.resultActionFail == ResultAction.Continue)
						{
							i++;
						}
						else
						{
							i = -1;
						}
					}
				}
				else
				{
					if (_action.endAction == ResultAction.Skip)
					{
						i = _action.skipAction; //actionList.IndexOf (_action.skipActionActual);
					}
					else if (_action.endAction == ResultAction.Continue)
					{
						i++;
					}
					else
					{
						i = -1;
					}
				}
			}
		}



		/**
		 * <summary>Gets a defined SpeechTag.</summary>
		 * <param name = "ID">The ID number of the SpeechTag to get</param>
		 * <returns>The SpeechTag</summary>
		 */
		public SpeechTag GetSpeechTag (int ID)
		{
			foreach (SpeechTag speechTag in speechTags)
			{
				if (speechTag.ID == ID)
				{
					return speechTag;
				}
			}
			return null;
		}


		/**
		 * <summary>Converts the Speech Managers's references from a given local variable to a given global variable</summary>
		 * <param name = "variable">The old local variable</param>
		 * <param name = "newGlobalID">The ID number of the new global variable</param>
		 */
		public void ConvertLocalVariableToGlobal (GVar variable, int newGlobalID)
		{
			bool wasAmended = false;

			int lineID = -1;
			if (variable.type == VariableType.String)
			{
				lineID = variable.textValLineID;
			}
			else if (variable.type == VariableType.PopUp)
			{
				lineID = variable.popUpsLineID;
			}

			if (lineID >= 0)
			{
				SpeechLine speechLine = GetLine (lineID);
				if (speechLine != null && speechLine.textType == AC_TextType.Variable)
				{
					speechLine.scene = "";
					ACDebug.Log ("Updated Speech Manager line " + lineID);
					wasAmended = true;
				}
			}

			if (wasAmended)
			{
				EditorUtility.SetDirty (this);
			}
		}


		/**
		 * <summary>Converts the Speech Managers's references from a given global variable to a given local variable</summary>
		 * <param name = "variable">The old global variable</param>
		 * <param name = "sceneName">The name of the scene that the new variable lives in</param>
		 */
		public void ConvertGlobalVariableToLocal (GVar variable, string sceneName)
		{
			bool wasAmended = false;

			int lineID = -1;
			if (variable.type == VariableType.String)
			{
				lineID = variable.textValLineID;
			}
			else if (variable.type == VariableType.PopUp)
			{
				lineID = variable.popUpsLineID;
			}

			if (lineID >= 0)
			{
				SpeechLine speechLine = GetLine (lineID);
				if (speechLine != null && speechLine.textType == AC_TextType.Variable)
				{
					speechLine.scene = sceneName;
					ACDebug.Log ("Updated Speech Manager line " + lineID);
					wasAmended = true;
				}
			}

			if (wasAmended)
			{
				EditorUtility.SetDirty (this);
			}
		}

		#endif


		/*
		 * The subdirectory within Resources that speech files are pulled from, if autoNameSpeechFiles = True.  Always ends with a forward-slash '/'.
		 */
		public string AutoSpeechFolder
		{
			get
			{
				if (string.IsNullOrEmpty (autoSpeechFolder))
				{
					return string.Empty;
				}
				if (!autoSpeechFolder.EndsWith ("/"))
				{
					return autoSpeechFolder + "/";
				}
				return autoSpeechFolder;
			}
		}


		/*
		 * The subdirectory within Resources that lipsync files are pulled from, if autoNameSpeechFiles = True.  Always ends with a forward-slash '/'.
		 */
		public string AutoLipsyncFolder
		{
			get
			{
				if (string.IsNullOrEmpty (autoLipsyncFolder))
				{
					return string.Empty;
				}
				if (!autoLipsyncFolder.EndsWith ("/"))
				{
					return autoLipsyncFolder + "/";
				}
				return autoLipsyncFolder;
			}
		}


		private void SyncLanguageData ()
		{
			if (languages.Count < languageIsRightToLeft.Count)
			{
				languageIsRightToLeft.RemoveRange (languages.Count, languageIsRightToLeft.Count - languages.Count);
			}
			else if (languages.Count > languageIsRightToLeft.Count)
			{
				if (languages.Count > languageIsRightToLeft.Capacity)
				{
					languageIsRightToLeft.Capacity = languages.Count;
				}
				for (int i=languageIsRightToLeft.Count; i<languages.Count; i++)
				{
					languageIsRightToLeft.Add (false);
				}
			}
		}


		/**
		 * <summary>Gets the audio filename of a SpeechLine.</summary>
		 * <param name = "_lineID">The translation ID number generated by SpeechManager's PopulateList() function</param>
		 * <param name = "speakerName">The name of the speaking character, which is only used if separating shared player audio</param>
		 * <returns>The audio filename of the speech line</summary>
		 */
		public string GetLineFilename (int _lineID, string speakerName = "")
		{
			foreach (SpeechLine line in lines)
			{
				if (line.lineID == _lineID)
				{
					return line.GetFilename (speakerName);
				}
			}
			return "";
		}


		/**
		 * <summary>Gets the full folder and filename for a speech line's audio or lipsync file, relative to the "Resources" Assets directory in which it is placed.</summary>
		 * <param name = "lineID">The ID number of the speech line</param>
		 * <param name = "speaker">The speaking character, if not a narration</param>
		 * <param name = "language">The language of the audio</param>
		 * <param name = "forLipSync">True if this is for a lipsync file</param>
		 * <returns>A string of the folder name that the audio or lipsync file should be placed in</returns>
		 */
		public string GetAutoAssetPathAndName (int lineID, Char speaker, string language, bool forLipsync = false)
		{
			SpeechLine speechLine = GetLine (lineID);
			if (speechLine != null)
			{
				if (GetAutoAssetPathAndNameOverride != null)
				{
					return GetAutoAssetPathAndNameOverride (speechLine, language, forLipsync);
				}

				string speakerOverride = (speaker != null) ? speaker.name : string.Empty;
				return speechLine.GetAutoAssetPathAndName (language, forLipsync, speakerOverride);
			}

			return "";
		}


		/**
		 * <summary>Gets a SpeechLine class, as generated by the Speech Manager.</summary>
		 * <param name = "_lineID">The translation ID number generated by SpeechManager's PopulateList() function</param>
		 * <returns>The generated SpeechLine class</summary>
		 */
		public SpeechLine GetLine (int _lineID)
		{
			foreach (SpeechLine line in lines) {
				if (line.lineID == _lineID) {
					return line;
				}
			}
			return null;
		}


		/**
		 * <summary>Checks if the current lipsyncing method relies on external text files for each line.</summary>
		 * <returns>True if the current lipsyncing method relies on external text files for each line.</returns>
		 */
		public bool UseFileBasedLipSyncing ()
		{
			if (lipSyncMode == LipSyncMode.ReadPamelaFile || lipSyncMode == LipSyncMode.ReadPapagayoFile || lipSyncMode == LipSyncMode.ReadSapiFile || lipSyncMode == LipSyncMode.RogoLipSync) {
				return true;
			}
			return false;
		}
		
	}
	
}