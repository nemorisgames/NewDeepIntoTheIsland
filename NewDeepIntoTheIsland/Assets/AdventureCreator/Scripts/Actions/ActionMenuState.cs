/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionMenuState.cs"
 * 
 *	This Action alters various variables of menus and menu elements.
 * 
 */

using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	[System.Serializable]
	public class ActionMenuState : Action
	{
		
		public enum MenuChangeType { TurnOnMenu, TurnOffMenu, HideMenuElement, ShowMenuElement, LockMenu, UnlockMenu, AddJournalPage, RemoveJournalPage };
		public MenuChangeType changeType = MenuChangeType.TurnOnMenu;
		
		public string menuToChange = "";
		public int menuToChangeParameterID = -1;
		
		public string elementToChange = "";
		public int elementToChangeParameterID = -1;
		
		public string journalText = "";
		public bool onlyAddNewJournal = false;

		public bool doFade = false;
		public int lineID = -1;

		public int journalPageIndex = -1;
		public int journalPageIndexParameterID = -1;

		
		public ActionMenuState ()
		{
			this.isDisplayed = true;
			lineID = -1;
			category = ActionCategory.Menu;
			title = "Change state";
			description = "Provides various options to show and hide both menus and menu elements.";
		}
		
		
		override public void AssignValues (List<ActionParameter> parameters)
		{
			menuToChange = AssignString (parameters, menuToChangeParameterID, menuToChange);
			elementToChange = AssignString (parameters, elementToChangeParameterID, elementToChange);
			journalPageIndex = AssignInteger (parameters, journalPageIndexParameterID, journalPageIndex);
		}
		
		
		override public float Run ()
		{
			if (!isRunning)
			{
				isRunning = true;
				AC.Menu _menu = PlayerMenus.GetMenuWithName (menuToChange);

				if (_menu != null)
				{
					if (changeType == MenuChangeType.TurnOnMenu)
					{
						if (_menu.IsManualControlled ())
						{
							if (!_menu.TurnOn (doFade))
							{
								// Menu is already on
								isRunning = false;
								return 0f;
							}
							
							if (doFade && willWait)
							{
								return _menu.fadeSpeed;
							}
						}
						else
						{
							ACDebug.LogWarning ("Can only turn on Menus with an Appear Type of Manual, OnInputKey or OnContainer - did you mean 'Unlock Menu'?");
						}
					}
					else if (changeType == MenuChangeType.TurnOffMenu)
					{
						if (_menu.IsManualControlled () || _menu.appearType == AppearType.OnInteraction)
						{
							if (!_menu.TurnOff (doFade))
							{
								// Menu is already off
								isRunning = false;
								return 0f;
							}
							
							if (doFade && willWait)
							{
								return _menu.fadeSpeed;
							}
						}
						else
						{
							ACDebug.LogWarning ("Can only turn off Menus with an Appear Type of Manual, OnInputKey or OnContainer - did you mean 'Lock Menu'?");
						}
					}
					else if (changeType == MenuChangeType.LockMenu)
					{
						if (doFade)
						{
							_menu.TurnOff (true);
						}
						else
						{
							_menu.ForceOff ();
						}
						_menu.isLocked = true;
						
						if (doFade && willWait)
						{
							return _menu.fadeSpeed;
						}
					}
					else
					{
						RunInstant (_menu);
					}
				}
				else if (menuToChange != "")
				{
					ACDebug.LogWarning ("Could not find menu of name '" + menuToChange + "'");
				}
			}
			else
			{
				isRunning = false;
				return 0f;
			}
			
			return 0f;
		}
		
		
		override public void Skip ()
		{
			AC.Menu _menu = PlayerMenus.GetMenuWithName (menuToChange);
			
			if (_menu != null)
			{
				if (changeType == MenuChangeType.TurnOnMenu)
				{
					if (_menu.appearType == AppearType.Manual || _menu.appearType == AppearType.OnInputKey)
					{
						_menu.TurnOn (false);
					}
				}
				else if (changeType == MenuChangeType.TurnOffMenu)
				{
					if (_menu.appearType == AppearType.Manual || _menu.appearType == AppearType.OnInputKey || _menu.appearType == AppearType.OnContainer)
					{
						_menu.ForceOff ();
					}
				}
				else if (changeType == MenuChangeType.LockMenu)
				{
					_menu.isLocked = true;
					_menu.ForceOff ();
				}
				else
				{
					RunInstant (_menu);
				}
			}
		}


		private void RunInstant (AC.Menu _menu)
		{
			if (changeType == MenuChangeType.HideMenuElement || changeType == MenuChangeType.ShowMenuElement)
			{
				MenuElement _element = PlayerMenus.GetElementWithName (menuToChange, elementToChange);
				if (_element != null)
				{
					if (changeType == MenuChangeType.HideMenuElement)
					{
						_element.isVisible = false;
						KickStarter.playerMenus.DeselectInputBox (_element);
					}
					else
					{
						_element.isVisible = true;
					}
					
					_menu.ResetVisibleElements ();
					_menu.Recalculate ();

					KickStarter.playerMenus.FindFirstSelectedElement ();
				}
				else
				{
					ACDebug.LogWarning ("Could not find element of name '" + elementToChange + "' on menu '" + menuToChange + "'");
				}
			}
			else if (changeType == MenuChangeType.UnlockMenu)
			{
				_menu.isLocked = false;
			}
			else if (changeType == MenuChangeType.AddJournalPage)
			{
				MenuElement _element = PlayerMenus.GetElementWithName (menuToChange, elementToChange);
				if (_element != null)
				{
					if (journalText != "")
					{
						if (_element is MenuJournal)
						{
							MenuJournal journal = (MenuJournal) _element;
							JournalPage newPage = new JournalPage (lineID, journalText);
							journal.AddPage (newPage, onlyAddNewJournal, journalPageIndex);

							if (lineID == -1)
							{
								ACDebug.LogWarning ("The new Journal page has no ID number, and will not be included in save game files - this can be corrected by clicking 'Gather text' in the Speech Manager");
							}
						}
						else
						{
							ACDebug.LogWarning (_element.title + " is not a journal!");
						}
					}
					else
					{
						ACDebug.LogWarning ("No journal text to add!");
					}
				}
				else
				{
					ACDebug.LogWarning ("Could not find menu element of name '" + elementToChange + "' inside '" + menuToChange + "'");
				}
				_menu.Recalculate ();
			}
			else if (changeType == MenuChangeType.RemoveJournalPage)
			{
				MenuElement _element = PlayerMenus.GetElementWithName (menuToChange, elementToChange);
				if (_element != null)
				{
					if (_element is MenuJournal)
					{
						MenuJournal journal = (MenuJournal) _element;
						journal.RemovePage (journalPageIndex);
					}
					else
					{
						ACDebug.LogWarning (_element.title + " is not a journal!");
					}
				}
				else
				{
					ACDebug.LogWarning ("Could not find menu element of name '" + elementToChange + "' inside '" + menuToChange + "'");
				}
				_menu.Recalculate ();
			}
		}

		
		#if UNITY_EDITOR

		public override void ClearIDs ()
		{
			lineID = -1;
		}

		
		override public void ShowGUI (List<ActionParameter> parameters)
		{
			changeType = (MenuChangeType) EditorGUILayout.EnumPopup ("Change type:", changeType);
			
			if (changeType == MenuChangeType.TurnOnMenu)
			{
				menuToChangeParameterID = Action.ChooseParameterGUI ("Menu to turn on:", parameters, menuToChangeParameterID, ParameterType.String);
				if (menuToChangeParameterID < 0)
				{
					menuToChange = EditorGUILayout.TextField ("Menu to turn on:", menuToChange);
				}
				doFade = EditorGUILayout.Toggle ("Fade?", doFade);
			}
			
			else if (changeType == MenuChangeType.TurnOffMenu)
			{
				menuToChangeParameterID = Action.ChooseParameterGUI ("Menu to turn off:", parameters, menuToChangeParameterID, ParameterType.String);
				if (menuToChangeParameterID < 0)
				{
					menuToChange = EditorGUILayout.TextField ("Menu to turn off:", menuToChange);
				}
				doFade = EditorGUILayout.Toggle ("Fade?", doFade);
			}
			
			else if (changeType == MenuChangeType.HideMenuElement)
			{
				menuToChangeParameterID = Action.ChooseParameterGUI ("Menu containing element:", parameters, menuToChangeParameterID, ParameterType.String);
				if (menuToChangeParameterID < 0)
				{
					menuToChange = EditorGUILayout.TextField ("Menu containing element:", menuToChange);
				}
				
				elementToChangeParameterID = Action.ChooseParameterGUI ("Element to hide:", parameters, elementToChangeParameterID, ParameterType.String);
				if (elementToChangeParameterID < 0)
				{
					elementToChange = EditorGUILayout.TextField ("Element to hide:", elementToChange);
				}
			}
			
			else if (changeType == MenuChangeType.ShowMenuElement)
			{
				menuToChangeParameterID = Action.ChooseParameterGUI ("Menu containing element:", parameters, menuToChangeParameterID, ParameterType.String);
				if (menuToChangeParameterID < 0)
				{
					menuToChange = EditorGUILayout.TextField ("Menu containing element:", menuToChange);
				}
				
				elementToChangeParameterID = Action.ChooseParameterGUI ("Element to show:", parameters, elementToChangeParameterID, ParameterType.String);
				if (elementToChangeParameterID < 0)
				{
					elementToChange = EditorGUILayout.TextField ("Element to show:", elementToChange);
				}
			}
			
			else if (changeType == MenuChangeType.LockMenu)
			{
				menuToChangeParameterID = Action.ChooseParameterGUI ("Menu to lock:", parameters, menuToChangeParameterID, ParameterType.String);
				if (menuToChangeParameterID < 0)
				{
					menuToChange = EditorGUILayout.TextField ("Menu to lock:", menuToChange);
				}
				doFade = EditorGUILayout.Toggle ("Fade?", doFade);
			}
			
			else if (changeType == MenuChangeType.UnlockMenu)
			{
				menuToChangeParameterID = Action.ChooseParameterGUI ("Menu to unlock:", parameters, menuToChangeParameterID, ParameterType.String);
				if (menuToChangeParameterID < 0)
				{
					menuToChange = EditorGUILayout.TextField ("Menu to unlock:", menuToChange);
				}
			}
			
			else if (changeType == MenuChangeType.AddJournalPage)
			{
				if (lineID > -1)
				{
					EditorGUILayout.LabelField ("Speech Manager ID:", lineID.ToString ());
				}
				
				menuToChangeParameterID = Action.ChooseParameterGUI ("Menu containing element:", parameters, menuToChangeParameterID, ParameterType.String);
				if (menuToChangeParameterID < 0)
				{
					menuToChange = EditorGUILayout.TextField ("Menu containing element:", menuToChange);
				}
				
				elementToChangeParameterID = Action.ChooseParameterGUI ("Journal element:", parameters, elementToChangeParameterID, ParameterType.String);
				if (elementToChangeParameterID < 0)
				{
					elementToChange = EditorGUILayout.TextField ("Journal element:", elementToChange);
				}
				
				EditorGUILayout.LabelField ("New page text:");
				journalText = EditorGUILayout.TextArea (journalText);
				onlyAddNewJournal = EditorGUILayout.Toggle ("Only add if not already in?", onlyAddNewJournal);
				if (onlyAddNewJournal && lineID == -1)
				{
					EditorGUILayout.HelpBox ("The page text must be added to the Speech Manager by clicking the 'Gather text' button, in order for duplicates to be prevented.", MessageType.Warning);
				}

				journalPageIndexParameterID = Action.ChooseParameterGUI ("Index to insert into:", parameters, journalPageIndexParameterID, ParameterType.Integer);
				if (journalPageIndexParameterID < 0)
				{
					journalPageIndex = EditorGUILayout.IntField ("Index to insert into:", journalPageIndex);
					EditorGUILayout.HelpBox ("An index value of -1 will add the page to the end of the Journal.", MessageType.Info);
				}
			}

			else if (changeType == MenuChangeType.RemoveJournalPage)
			{
				menuToChangeParameterID = Action.ChooseParameterGUI ("Menu containing element:", parameters, menuToChangeParameterID, ParameterType.String);
				if (menuToChangeParameterID < 0)
				{
					menuToChange = EditorGUILayout.TextField ("Menu containing element:", menuToChange);
				}
				
				elementToChangeParameterID = Action.ChooseParameterGUI ("Journal element:", parameters, elementToChangeParameterID, ParameterType.String);
				if (elementToChangeParameterID < 0)
				{
					elementToChange = EditorGUILayout.TextField ("Journal element:", elementToChange);
				}
				
				journalPageIndexParameterID = Action.ChooseParameterGUI ("Page number to remove:", parameters, journalPageIndexParameterID, ParameterType.Integer);
				if (journalPageIndexParameterID < 0)
				{
					journalPageIndex = EditorGUILayout.IntField ("Page number to remove:", journalPageIndex);
					EditorGUILayout.HelpBox ("An index value of -1 will remove the last page of the Journal.", MessageType.Info);
				}
			}
			
			if (doFade && (changeType == MenuChangeType.TurnOnMenu || changeType == MenuChangeType.TurnOffMenu || changeType == MenuChangeType.LockMenu))
			{
				willWait = EditorGUILayout.Toggle ("Wait until finish?", willWait);
			}
			
			AfterRunningOption ();
		}
		
		
		public override string SetLabel ()
		{
			string labelAdd = " (" + changeType.ToString () + " '" + menuToChange;
			if (changeType == MenuChangeType.HideMenuElement || changeType == MenuChangeType.ShowMenuElement)
			{
				labelAdd += " " + elementToChange;
			}
			labelAdd += "')";
			return labelAdd;
		}


		public override bool ConvertLocalVariableToGlobal (int oldLocalID, int newGlobalID)
		{
			bool wasAmended = base.ConvertLocalVariableToGlobal (oldLocalID, newGlobalID);

			string updatedJournalText = AdvGame.ConvertLocalVariableTokenToGlobal (journalText, oldLocalID, newGlobalID);
			if (journalText != updatedJournalText)
			{
				wasAmended = true;
				journalText = updatedJournalText;
			}
			return wasAmended;
		}


		public override bool ConvertGlobalVariableToLocal (int oldGlobalID, int newLocalID, bool isCorrectScene)
		{
			bool isAffected = base.ConvertGlobalVariableToLocal (oldGlobalID, newLocalID, isCorrectScene);

			string updatedJournalText = AdvGame.ConvertGlobalVariableTokenToLocal (journalText, oldGlobalID, newLocalID);
			if (journalText != updatedJournalText)
			{
				isAffected = true;
				if (isCorrectScene)
				{
					journalText = updatedJournalText;
				}
			}
			return isAffected;
		}


		public override int GetVariableReferences (List<ActionParameter> parameters, VariableLocation location, int varID)
		{
			int thisCount = 0;
			string tokenText = (location == VariableLocation.Local) ? "[localvar:" + varID.ToString () + "]"
																		: "[var:" + varID.ToString () + "]";
			if (journalText.Contains (tokenText))
			{
				thisCount ++;
			}
			thisCount += base.GetVariableReferences (parameters, location, varID);
			return thisCount;
		}

		#endif
		
	}
	
}