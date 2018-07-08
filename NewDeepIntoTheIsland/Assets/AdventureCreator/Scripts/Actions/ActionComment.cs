/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionComment.cs"
 * 
 *	This action simply displays a comment in the Editor / Inspector.
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
	public class ActionComment : Action
	{
		
		public string commentText = "";

		private enum ACLogType { No, AsInfo, AsWarning, AsError };
		[SerializeField] private ACLogType acLogType = ACLogType.AsInfo;
		private string convertedText;
		
		
		public ActionComment ()
		{
			this.isDisplayed = true;
			category = ActionCategory.ActionList;
			title = "Comment";
			description = "Prints a comment for debug purposes.";
		}


		public override void AssignValues (System.Collections.Generic.List<ActionParameter> parameters)
		{
			convertedText = AdvGame.ConvertTokens (commentText, 0, null, parameters);
		}


		public override float Run ()
		{
			if (acLogType != ACLogType.No && !string.IsNullOrEmpty (convertedText))
			{
				if (acLogType == ACLogType.AsInfo)
				{
					ACDebug.Log (convertedText);
				}
				else if (acLogType == ACLogType.AsWarning)
				{
					ACDebug.LogWarning (convertedText);
				}
				else if (acLogType == ACLogType.AsError)
				{
					ACDebug.LogError (convertedText);
				}
			}
			return 0f;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI ()
		{
			EditorStyles.textField.wordWrap = true;
			commentText = EditorGUILayout.TextArea (commentText, GUILayout.MaxWidth (280f));

			acLogType = (ACLogType) EditorGUILayout.EnumPopup ("Display in Console?", acLogType);

			AfterRunningOption ();
		}
		
		
		public override string SetLabel ()
		{
			if (!string.IsNullOrEmpty (commentText))
			{
				int i = commentText.IndexOf ("\n");
				if (i > 0)
				{
					return (" (" + commentText.Substring (0, i) + ")");
				}
				return (" (" + commentText + ")");
			}
			return "";
		}


		public override bool ConvertLocalVariableToGlobal (int oldLocalID, int newGlobalID)
		{
			bool wasAmended = base.ConvertLocalVariableToGlobal (oldLocalID, newGlobalID);

			string updatedCommentText = AdvGame.ConvertLocalVariableTokenToGlobal (commentText, oldLocalID, newGlobalID);
			if (commentText != updatedCommentText)
			{
				wasAmended = true;
				commentText = updatedCommentText;
			}
			return wasAmended;
		}


		public override bool ConvertGlobalVariableToLocal (int oldGlobalID, int newLocalID, bool isCorrectScene)
		{
			bool isAffected = base.ConvertGlobalVariableToLocal (oldGlobalID, newLocalID, isCorrectScene);

			string updatedCommentText = AdvGame.ConvertGlobalVariableTokenToLocal (commentText, oldGlobalID, newLocalID);
			if (commentText != updatedCommentText)
			{
				isAffected = true;
				if (isCorrectScene)
				{
					commentText = updatedCommentText;
				}
			}
			return isAffected;
		}


		public override int GetVariableReferences (List<ActionParameter> parameters, VariableLocation location, int varID)
		{
			int thisCount = 0;
			string tokenText = (location == VariableLocation.Local) ? "[localvar:" + varID.ToString () + "]"
																		: "[var:" + varID.ToString () + "]";
			if (commentText.Contains (tokenText))
			{
				thisCount ++;
			}
			thisCount += base.GetVariableReferences (parameters, location, varID);
			return thisCount;
		}

		#endif
		
	}
	
}