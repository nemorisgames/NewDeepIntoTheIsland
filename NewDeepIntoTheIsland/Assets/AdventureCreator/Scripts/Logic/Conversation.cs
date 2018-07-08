/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"Conversation.cs"
 * 
 *	This script is handles character conversations.
 *	It generates instances of DialogOption for each line
 *	that the player can choose to say.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AC
{

	/**
	 * This component provides the player with a list of dialogue options that their character can say.
	 * Options are display in a MenuDialogList element, and will usually run a DialogueOption ActionList when clicked - unless overrided by the "Dialogue: Start conversation" Action that triggers it.
	 */
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_conversation.html")]
	#endif
	public class Conversation : MonoBehaviour
	{

		/** The source of the commands that are run when an option is chosen (InScene, AssetFile, CustomScript) */	
		public AC.InteractionSource interactionSource;
		/** All available dialogue options that make up the Conversation */
		public List<ButtonDialog> options = new List<ButtonDialog>();
		/** The option selected within the Conversation's Inspector  */
		public ButtonDialog selectedOption;

		/** The index number of the last-chosen Conversation dialogue option */
		public int lastOption = -1;

		/** If True, and only one option is available, then the option will be chosen automatically */
		public bool autoPlay = false;
		/** If True, then the Conversation is timed, and the options will only be shown for a fixed period */
		public bool isTimed = false;
		/** The duration, in seconds, that the Conversation is active, if isTime = True */
		public float timer = 5f;
		/** The index number of the option to select, if isTimed = True and the timer runs out before the player has made a choice. If -1, then the conversation will end */
		public int defaultOption = 0;

		private float startTime;

		
		private void Awake ()
		{
			Upgrade ();
		}


		/**
		 * Show the Conversation's dialogue options.
		 */
		public void Interact ()
		{
			Interact (null);
		}


		/**
		 * <summary>Shows the Conversation's dialogue options.</summary>
		 * <param name = "actionConversation">The "Dialogue: Start conversation" Action that calls this function.  This is necessary when that Action overrides the Converstion's options.</param>
		 */
		public void Interact (ActionConversation actionConversation)
		{
			KickStarter.actionListManager.SetConversationPoint (actionConversation);
			KickStarter.eventManager.Call_OnStartConversation (this);

			CancelInvoke ("RunDefault");
			int numPresent = 0;
			foreach (ButtonDialog _option in options)
			{
				if (_option.CanShow ())
				{
					numPresent ++;
				}
			}
			
			if (KickStarter.playerInput)
			{
				if (numPresent == 1 && autoPlay)
				{
					foreach (ButtonDialog _option in options)
					{
						if (_option.CanShow ())
						{
							RunOption (_option);
							return;
						}
					}
				}
				else if (numPresent > 0)
				{
					KickStarter.playerInput.activeConversation = this;
					KickStarter.stateHandler.gameState = GameState.DialogOptions;
				}
				else
				{
					KickStarter.playerInput.EndConversation ();
				}
			}
			
			if (isTimed)
			{
				startTime = Time.time;
				Invoke ("RunDefault", timer);
			}
		}


		private void RunOption (ButtonDialog _option)
		{
			KickStarter.actionListManager.SetCorrectGameState ();

			_option.hasBeenChosen = true;
			if (options.Contains (_option))
			{
				lastOption = options.IndexOf (_option);
				if (KickStarter.actionListManager.OverrideConversation (lastOption))
				{
					return;
				}
				lastOption = -1;
			}

			Conversation endConversation = null;
			if (interactionSource != AC.InteractionSource.CustomScript)
			{
				if (_option.conversationAction == ConversationAction.ReturnToConversation)
				{
					endConversation = this;
				}
				else if (_option.conversationAction == ConversationAction.RunOtherConversation && _option.newConversation != null)
				{
					endConversation = _option.newConversation;
				}
			}

			if (interactionSource == AC.InteractionSource.AssetFile && _option.assetFile)
			{
				AdvGame.RunActionListAsset (_option.assetFile, endConversation);
			}
			else if (interactionSource == AC.InteractionSource.CustomScript)
			{
				if (_option.customScriptObject != null && !string.IsNullOrEmpty (_option.customScriptFunction))
				{
					_option.customScriptObject.SendMessage (_option.customScriptFunction);
				}
			}
			else if (interactionSource == AC.InteractionSource.InScene && _option.dialogueOption)
			{
				_option.dialogueOption.conversation = endConversation;
				_option.dialogueOption.Interact ();
			}
			else
			{
				ACDebug.Log ("No Interaction object found!");

				if (endConversation != null)
				{
					endConversation.Interact ();
				}
				else
				{
					KickStarter.stateHandler.gameState = GameState.Normal;
				}
			}

			KickStarter.eventManager.Call_OnClickConversation (this, _option.ID);
		}
		

		/**
		 * Show the Conversation's dialogue options.
		 */
		public void TurnOn ()
		{
			Interact ();
		}
		

		/**
		 * Hides the Conversation's dialogue options, if it is the currently-active Conversation.
		 */
		public void TurnOff ()
		{
			if (KickStarter.playerInput != null && KickStarter.playerInput.activeConversation == this)
			{
				CancelInvoke ("RunDefault");
				KickStarter.playerInput.EndConversation ();
				KickStarter.actionListManager.OnEndConversation ();
				KickStarter.actionListManager.SetCorrectGameState ();
			}
		}
		
		
		private void RunDefault ()
		{
			if (KickStarter.playerInput && KickStarter.playerInput.IsInConversation ())
			{
				if (defaultOption < 0 || defaultOption >= options.Count)
				{
					TurnOff ();
				}
				else
				{
					RunOption (defaultOption);
				}
			}
		}
		
		
		private IEnumerator RunOptionCo (int i)
		{
			KickStarter.playerInput.PendingOptionConversation = this;
			yield return new WaitForSeconds (0.3f);
			RunOption (options[i]);

			if (KickStarter.playerInput.PendingOptionConversation == this)
			{
				KickStarter.playerInput.PendingOptionConversation = null;
			}
		}
		

		/**
		 * <summary>Runs a dialogue option.</summary>
		 * <param name = "slot">The index number of the dialogue option to run</param>
		 */
		public void RunOption (int slot)
		{
			CancelInvoke ("RunDefault");
			int i = ConvertSlotToOption (slot);
			if (i == -1)
			{
				return;
			}

			KickStarter.playerInput.EndConversation ();

			if (interactionSource == AC.InteractionSource.CustomScript)
			{
				RunOption (options[i]);
			}
			else
			{
				StartCoroutine (RunOptionCo (i));
			}
		}
		

		/**
		 * <summary>Gets the time remaining before a timed Conversation ends.</summary>
		 * <returns>The time remaining before a timed Conversation ends.</returns>
		 */
		public float GetTimeRemaining ()
		{
			return ((startTime + timer - Time.time) / timer);
		}
		
		
		private int ConvertSlotToOption (int slot)
		{
			int foundSlots = 0;
			for (int j=0; j<options.Count; j++)
			{
				if (options[j].CanShow ())
				{
					foundSlots ++;
					if (foundSlots == (slot+1))
					{
						return j;
					}
				}
			}
			return -1;
		}
		

		/**
		 * <summary>Gets the display label of a dialogue option.</summary>
		 * <param name = "slot">The index number of the dialogue option to find</param>
		 * <returns>The display label of the dialogue option</returns>
		 */
		public string GetOptionName (int slot)
		{
			int i = ConvertSlotToOption (slot);
			if (i == -1)
			{
				i = 0;
			}

			string translatedLine = KickStarter.runtimeLanguages.GetTranslation (options[i].label, options[i].lineID, Options.GetLanguage ());
			return AdvGame.ConvertTokens (translatedLine);
		}
		

		/**
		 * <summary>Gets the display icon of a dialogue option.</summary>
		 * <param name = "slot">The index number of the dialogue option to find</param>
		 * <returns>The display icon of the dialogue option</returns>
		 */
		public CursorIconBase GetOptionIcon (int slot)
		{
			int i = ConvertSlotToOption (slot);
			if (i == -1)
			{
				i = 0;
			}
			return options[i].cursorIcon;
		}


		/**
		 * <summary>Gets the ButtonDialog data container, which stores data for a dialogue option.</summary>
		 * <param name = "slot">The index number of the dialogue option to find</param>
		 * <returns>The ButtonDialog data container</returns>
		 */
		public ButtonDialog GetOption (int slot)
		{
			int i = ConvertSlotToOption (slot);
			if (i == -1)
			{
				i = 0;
			}
			return options[i];
		}


		/**
		 * <summary>Checks if a dialogue option has been chosen at least once by the player.</summary>
		 * <param name = "slot">The index number of the dialogue option to find</param>
		 * <returns>True if the dialogue option has been chosen at least once by the player.</returns>
		 */
		public bool OptionHasBeenChosen (int slot)
		{
			int i = ConvertSlotToOption (slot);
			if (i == -1)
			{
				i = 0;
			}
			return options[i].hasBeenChosen;
		}


		/**
		 * <summary>Turns a dialogue option on, provided that it is unlocked.</summary>
		 * <param name = "id">The ID number of the dialogue option to enable</param>
		 */
		public void TurnOptionOn (int id)
		{
			foreach (ButtonDialog option in options)
			{
				if (option.ID == id)
				{
					if (!option.isLocked)
					{
						option.isOn = true;
					}
					else
					{
						ACDebug.Log (gameObject.name + "'s option '" + option.label + "' cannot be turned on as it is locked.");
					}
					return;
				}
			}
		}


		/**
		 * <summary>Turns a dialogue option off, provided that it is unlocked.</summary>
		 * <param name = "id">The ID number of the dialogue option to disable</param>
		 */
		public void TurnOptionOff (int id)
		{
			foreach (ButtonDialog option in options)
			{
				if (option.ID == id)
				{
					if (!option.isLocked)
					{
						option.isOn = false;
					}
					else
					{
						ACDebug.LogWarning (gameObject.name + "'s option '" + option.label + "' cannot be turned off as it is locked.");
					}
					return;
				}
			}
		}


		/**
		 * <summary>Sets the enabled and locked states of a dialogue option, provided that it is unlocked.</summary>
		 * <param name = "id">The ID number of the dialogue option to change</param>
		 * <param name = "flag">The "on/off" state to set the option</param>
		 * <param name = "isLocked">The "locked/unlocked" state to set the option</param>
		 */
		public void SetOptionState (int id, bool flag, bool isLocked)
		{
			foreach (ButtonDialog option in options)
			{
				if (option.ID == id)
				{
					if (!option.isLocked)
					{
						option.isLocked = isLocked;
						option.isOn = flag;
					}
					KickStarter.playerMenus.RefreshDialogueOptions ();
					return;
				}
			}
		}


		/**
		 * <summary>Renames a dialogue option.</summary>
		 * <param name = "id">The ID number of the dialogue option to rename</param>
		 * <param name = "newLabel">The new label text to give the dialogue option<param>
		 * <param name = "newLindID">The line ID number to give the dialogue option, as set by the Speech Manager</param>
		 */
		public void RenameOption (int id, string newLabel, int newLineID)
		{
			foreach (ButtonDialog option in options)
			{
				if (option.ID == id)
				{
					option.label = newLabel;
					option.lineID = newLineID;
					return;
				}
			}
		}
		

		/**
		 * <summary>Gets the number of enabled dialogue options.</summary>
		 * <returns>The number of enabled dialogue options</summary>
		 */
		public int GetCount ()
		{
			int numberOn = 0;
			foreach (ButtonDialog _option in options)
			{
				if (_option.CanShow ())
				{
					numberOn ++;
				}
			}
			return numberOn;
		}


		/**
		 * Upgrades the Conversation from a previous version of Adventure Creator.
		 */
		public void Upgrade ()
		{
			bool wasUpgraded = false;

			// Set IDs as index + 1 (because default is 0 when not upgraded)
			if (options.Count > 0 && options[0].ID == 0)
			{
				for (int i=0; i<options.Count; i++)
				{
					options[i].ID = i+1;
				}

				wasUpgraded = true;
			}

			for (int i=0; i<options.Count; i++)
			{
				bool upgradedTexture = options[i].Upgrade ();
				if (upgradedTexture)
				{
					wasUpgraded = true;
				}
			}

			if (wasUpgraded)
			{
				#if UNITY_EDITOR
				if (Application.isPlaying)
				{
					ACDebug.LogWarning ("Conversation '" + gameObject.name + "' has been temporarily upgraded - please view its Inspector when the game ends and save the scene.");
				}
				else
				{
					UnityVersionHandler.CustomSetDirty (this, true);
					if (!this.gameObject.activeInHierarchy)
					{
						// Asset file
						UnityEditor.AssetDatabase.SaveAssets ();
					}
					ACDebug.LogWarning ("Upgraded Conversation '" + gameObject.name + "', please save the scene.");
				}
				#endif
			}
		}


		#if UNITY_EDITOR

		/**
		 * <summary>Converts the Conversations's references from a given local variable to a given global variable</summary>
		 * <param name = "oldLocalID">The ID number of the old local variable</param>
		 * <param name = "newGlobalID">The ID number of the new global variable</param>
		 * <returns>True if the Action was amended</returns>
		 */
		public bool ConvertLocalVariableToGlobal (int oldLocalID, int newGlobalID)
		{
			bool wasAmened = false;

			if (options != null)
			{
				foreach (ButtonDialog option in options)
				{
					string newLabel = AdvGame.ConvertLocalVariableTokenToGlobal (option.label, oldLocalID, newGlobalID);
					if (newLabel != option.label)
					{
						option.label = newLabel;
						wasAmened = true;
					}
				}
			}

			return wasAmened;
		}


		/**
		 * <summary>Gets the number of references to a given local or global variable</summary>
		 * <param name = "location">The location of the variable (Global, Local)</param>
		 * <param name = "varID">The ID number of the variable</param>
		 * <returns>The number of references to the variable</returns>
		 */
		public int GetVariableReferences (VariableLocation location, int varID)
		{
			int numFound = 0;
			if (options != null)
			{
				string tokenText = (location == VariableLocation.Local) ? "[localvar:" + varID.ToString () + "]"
																		: "[var:" + varID.ToString () + "]";

				foreach (ButtonDialog option in options)
				{
					if (option.label.Contains (tokenText))
					{
						numFound ++;
					}
				}
			}
			return numFound;
		}


		public int GetInventoryReferences (int itemID)
		{
			int numFound = 0;
			foreach (ButtonDialog option in options)
			{
				if (option.linkToInventory && option.linkedInventoryID == itemID)
				{
					numFound ++;
				}
			}
			return numFound;
		}


		/**
		 * <summary>Converts the Conversations's references from a given global variable to a given local variable</summary>
		 * <param name = "oldLocalID">The ID number of the old global variable</param>
		 * <param name = "newLocalID">The ID number of the new local variable</param>
		 * <returns>True if the Action was amended</returns>
		 */
		public bool ConvertGlobalVariableToLocal (int oldGlobalID, int newLocalID, bool isCorrectScene)
		{
			bool wasAmened = false;

			if (options != null)
			{
				foreach (ButtonDialog option in options)
				{
					string newLabel = AdvGame.ConvertGlobalVariableTokenToLocal (option.label, oldGlobalID, newLocalID);
					if (newLabel != option.label)
					{
						wasAmened = true;
						if (isCorrectScene)
						{
							option.label = newLabel;
						}
					}
				}
			}

			return wasAmened;
		}

		#endif


		/**
		 * <summmary>Gets an array of ID numbers of existing ButtonDialog classes, so that a unique number can be generated.</summary>
		 * <returns>Gets an array of ID numbers of existing ButtonDialog classes</returns>
		 */
		public int[] GetIDArray ()
		{
			List<int> idArray = new List<int>();
			foreach (ButtonDialog option in options)
			{
				idArray.Add (option.ID);
			}
			
			idArray.Sort ();
			return idArray.ToArray ();
		}

	}

}