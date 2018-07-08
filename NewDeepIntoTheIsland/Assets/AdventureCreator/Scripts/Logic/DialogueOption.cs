/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"DialogueOption.cs"
 * 
 *	This ActionList is used by Conversations
 *	Each instance of the script handles a particular dialog option.
 * 
 */

using UnityEngine;

namespace AC
{

	/**
	 * An ActionList that is run when a Conversation's dialogue option is clicked on, unless the Conversation has been overridden with the "Dialogue: Start conversation" Action.
	 */
	[System.Serializable]
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_dialogue_option.html")]
	#endif
	public class DialogueOption : ActionList
	{ }
	
}