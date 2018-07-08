/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"ActionSpeechStop.cs"
 * 
 *	This Action forces off all playing speech
 * 
 */

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

	[System.Serializable]
	public class ActionSpeechStop : Action
	{

		public bool forceMenus;
		public SpeechMenuLimit speechMenuLimit = SpeechMenuLimit.All;
		public SpeechMenuType speechMenuType = SpeechMenuType.All;
		public string limitToCharacters = "";


		public ActionSpeechStop ()
		{
			this.isDisplayed = true;
			category = ActionCategory.Dialogue;
			title = "Stop speech";
			description = "Ends any currently-playing speech instantly.";
		}
		
		
		override public float Run ()
		{
			KickStarter.dialog.KillDialog (true, forceMenus, speechMenuLimit, speechMenuType, limitToCharacters);

			return 0f;
		}
		
		
		#if UNITY_EDITOR
		
		override public void ShowGUI ()
		{
			speechMenuLimit = (SpeechMenuLimit) EditorGUILayout.EnumPopup ("Speech to stop:", speechMenuLimit);
			speechMenuType = (SpeechMenuType) EditorGUILayout.EnumPopup ("Characters to stop", speechMenuType);

			if (speechMenuType == SpeechMenuType.SpecificCharactersOnly)
			{
				limitToCharacters = EditorGUILayout.TextField ("Character(s) to stop:", limitToCharacters);
				EditorGUILayout.HelpBox ("Multiple character names should be separated by a colon ';'", MessageType.Info);
			}
			else if (speechMenuType == SpeechMenuType.AllExceptSpecificCharacters)
			{
				limitToCharacters = EditorGUILayout.TextField ("Character(s) to not stop:", limitToCharacters);
				EditorGUILayout.HelpBox ("Multiple character names should be separated by a colon ';'", MessageType.Info);
			}

			forceMenus = EditorGUILayout.Toggle ("Force off subtitles?", forceMenus);

			AfterRunningOption ();
		}
		
		
		public override string SetLabel ()
		{
			return (" (" + speechMenuLimit.ToString () + ")");
		}
		
		#endif
		
	}

}