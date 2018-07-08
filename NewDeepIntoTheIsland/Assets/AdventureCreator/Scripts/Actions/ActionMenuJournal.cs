/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionMenuJournal.cs"
 * 
 *	This Action allows you to set the page number of a MenuJournal.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{
	
	[System.Serializable]
	public class ActionMenuJournal : Action
	{

		public string menuToChange = "";
		public int menuToChangeParameterID = -1;
		
		public string elementToChange = "";
		public int elementToChangeParameterID = -1;

		public SetJournalPage setJournalPage = SetJournalPage.FirstPage;

		public int pageNumber;
		public int pageNumberParameterID = -1;

		
		public ActionMenuJournal ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Menu;
			title = "Set Journal page";
			description = "Set which page of a Journal is currently open.";
		}


		override public void AssignValues (List<ActionParameter> parameters)
		{
			menuToChange = AssignString (parameters, menuToChangeParameterID, menuToChange);
			elementToChange = AssignString (parameters, elementToChangeParameterID, elementToChange);
			pageNumber = AssignInteger (parameters, pageNumberParameterID, pageNumber);
		}
		
		
		override public float Run ()
		{
			MenuElement _element = PlayerMenus.GetElementWithName (menuToChange, elementToChange);
			if (_element != null)
			{
				if (_element is MenuJournal)
				{
					MenuJournal journal = (MenuJournal) _element;

					if (journal.pages.Count > 0)
					{
						if (setJournalPage == SetJournalPage.FirstPage)
						{
							journal.showPage = 1;
						}
						else if (setJournalPage == SetJournalPage.LastPage)
						{
							journal.showPage = journal.pages.Count;
						}
						else if (setJournalPage == SetJournalPage.SetHere)
						{
							journal.showPage = Mathf.Min (journal.pages.Count, pageNumber);
						}
					}
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

			return 0f;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI (List<ActionParameter> parameters)
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

			setJournalPage = (SetJournalPage) EditorGUILayout.EnumPopup ("Page to set to:", setJournalPage);
			if (setJournalPage == SetJournalPage.SetHere)
			{
				pageNumberParameterID = Action.ChooseParameterGUI ("Page #:", parameters, pageNumberParameterID, ParameterType.Integer);
				if (pageNumberParameterID < 0)
				{
					pageNumber = EditorGUILayout.IntField ("Page #:", pageNumber);
				}
			}
			
			AfterRunningOption ();
		}
		
		
		public override string SetLabel ()
		{
			return (" (" + setJournalPage.ToString () + ")");
		}
		
		#endif
		
	}
	
}