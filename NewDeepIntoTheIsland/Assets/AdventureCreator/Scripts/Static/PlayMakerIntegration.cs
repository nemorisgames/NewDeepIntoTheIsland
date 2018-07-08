/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"PlayMakerIntegration.cs"
 * 
 *	This script contains static functions for use
 *	in calling PlayMaker FSMs.
 *
 *	To allow for PlayMaker integration, the 'PlayMakerIsPresent'
 *	preprocessor must be defined.  This can be done from
 *	Edit -> Project Settings -> Player, and entering
 *	'PlayMakerIsPresent' into the Scripting Define Symbols text box
 *	for your game's build platform.
 * 
 */

using UnityEngine;

#if PlayMakerIsPresent
using HutongGames.PlayMaker;
#endif

namespace AC
{

	/**
	 * A class the contains a number of static functions to assist with PlayMaker integration.
	 * To use PlayMaker with Adventure Creator, the 'PlayMakerIsPresent' preprocessor must be defined.
	 */
	public class PlayMakerIntegration : ScriptableObject
	{

		/**
		 * <summary>Checks if the 'PlayMakerIsPresent' preprocessor has been defined.</summary>
		 * <returns>True if the 'PlayMakerIsPresent' preprocessor has been defined</returns>
		 */
		public static bool IsDefinePresent ()
		{
			#if PlayMakerIsPresent
			return true;
			#else
			return false;
			#endif
		}


		/**
		 * <summary>Calls a PlayMaker event on a specific FSM.</summary>
		 * <param name = "linkedObject">The GameObject with the PlayMakerFSM component</param>
		 * <param name = "eventName">The name of the event to call</param>
		 * <param name = "fsmNme">The name of the FSM to call</param>
		 */
		public static void CallEvent (GameObject linkedObject, string eventName, string fsmName)
		{
			#if PlayMakerIsPresent
			PlayMakerFSM[] playMakerFsms = linkedObject.GetComponents<PlayMakerFSM>();
			foreach (PlayMakerFSM playMakerFSM in playMakerFsms)
			{
				if (playMakerFSM.FsmName == fsmName)
				{
					playMakerFSM.Fsm.Event (eventName);
				}
			}
			#endif
		}
		

		/**
		 * <summary>Calls a PlayMaker FSM event.</summary>
		 * <param name = "linkedObject">The GameObject with the PlayMakerFSM component</param>
		 * <param name = "eventName">The name of the event to call</param>
		 */
		public static void CallEvent (GameObject linkedObject, string eventName)
		{
			#if PlayMakerIsPresent
			if (linkedObject.GetComponent <PlayMakerFSM>())
			{
				PlayMakerFSM playMakerFSM = linkedObject.GetComponent <PlayMakerFSM>();
				playMakerFSM.Fsm.Event (eventName);
			}
			#endif
		}
		

		/**
		 * <summary>Gets the value of a PlayMaker global integer.</summary>
		 * <param name = "_name">The name of the PlayMaker global integer to search for</param>
		 * <returns>The value of the PlayMaker global integer</returns>
		 */
		public static int GetGlobalInt (string _name)
		{
			#if PlayMakerIsPresent
			FsmInt fsmInt = FsmVariables.GlobalVariables.FindFsmInt (_name);
			if (fsmInt != null)
			{
				return fsmInt.Value;
			}
			ACDebug.LogWarning ("Cannot find Playmaker global Integer with then name '" + _name + "'");
			#endif
			return 0;
		}
		

		/**
		 * <summary>Gets the value of a PlayMaker global boolean.</summary>
		 * <param name = "_name">The name of the PlayMaker global boolean to search for</param>
		 * <returns>The value of the PlayMaker global boolean</returns>
		 */
		public static bool GetGlobalBool (string _name)
		{
			#if PlayMakerIsPresent
			FsmBool fsmBool = FsmVariables.GlobalVariables.FindFsmBool (_name);
			if (fsmBool != null)
			{
				return fsmBool.Value;
			}
			ACDebug.LogWarning ("Cannot find Playmaker global Boolean with then name '" + _name + "'");
			#endif
			return false;
		}
		

		/**
		 * <summary>Gets the value of a PlayMaker global string.</summary>
		 * <param name = "_name">The name of the PlayMaker global string to search for</param>
		 * <returns>The value of the PlayMaker global string</returns>
		 */
		public static string GetGlobalString (string _name)
		{
			#if PlayMakerIsPresent
			FsmString fsmString = FsmVariables.GlobalVariables.FindFsmString (_name);
			if (fsmString != null)
			{
				return fsmString.Value;
			}
			ACDebug.LogWarning ("Cannot find Playmaker global String with then name '" + _name + "'");
			#endif
			return "";
		}
		

		/**
		 * <summary>Gets the value of a PlayMaker global float.</summary>
		 * <param name = "_name">The name of the PlayMaker global float to search for</param>
		 * <returns>The value of the PlayMaker global float</returns>
		 */
		public static float GetGlobalFloat (string _name)
		{
			#if PlayMakerIsPresent
			FsmFloat fsmFloat = FsmVariables.GlobalVariables.FindFsmFloat (_name);
			if (fsmFloat != null)
			{
				return fsmFloat.Value;
			}
			ACDebug.LogWarning ("Cannot find Playmaker global Float with then name '" + _name + "'");
			#endif
			return 0f;
		}


		/**
		 * <summary>Gets the value of a PlayMaker global Vector3.</summary>
		 * <param name = "_name">The name of the PlayMaker global Vector3 to search for</param>
		 * <returns>The value of the PlayMaker global Vector3</returns>
		 */
		public static Vector3 GetGlobalVector3 (string _name)
		{
			#if PlayMakerIsPresent
			FsmVector3 fsmVector3 = FsmVariables.GlobalVariables.FindFsmVector3 (_name);
			if (fsmVector3 != null)
			{
				return fsmVector3.Value;
			}
			ACDebug.LogWarning ("Cannot find Playmaker global Vector3 with then name '" + _name + "'");
			#endif
			return Vector3.zero;
		}
		

		/**
		 * <summary>Sets the value of a PlayMaker global integer.</summary>
		 * <param name = "_name">The name of the PlayMaker global integer to update</param>
		 * <param name = "_val">The new value to assign the PlayMaker global integer</param>
		 */
		public static void SetGlobalInt (string _name, int _val)
		{
			#if PlayMakerIsPresent
			FsmInt fsmInt = FsmVariables.GlobalVariables.FindFsmInt (_name);
			if (fsmInt != null)
			{
				fsmInt.Value = _val;
			}
			else
			{
				ACDebug.LogWarning ("Cannot find Playmaker global Integer with then name '" + _name + "'");
			}
			#endif
		}
		

		/**
		 * <summary>Sets the value of a PlayMaker global booleam.</summary>
		 * <param name = "_name">The name of the PlayMaker global booleam to update</param>
		 * <param name = "_val">The new value to assign the PlayMaker global booleam</param>
		 */
		public static void SetGlobalBool (string _name, bool _val)
		{
			#if PlayMakerIsPresent
			FsmBool fsmBool = FsmVariables.GlobalVariables.FindFsmBool (_name);
			if (fsmBool != null)
			{
				fsmBool.Value = _val;
			}
			else
			{
				ACDebug.LogWarning ("Cannot find Playmaker global Boolean with then name '" + _name + "'");
			}
			#endif
		}
		

		/**
		 * <summary>Sets the value of a PlayMaker global string.</summary>
		 * <param name = "_name">The name of the PlayMaker global string to update</param>
		 * <param name = "_val">The new value to assign the PlayMaker global string</param>
		 */
		public static void SetGlobalString (string _name, string _val)
		{
			#if PlayMakerIsPresent
			FsmString fsmString = FsmVariables.GlobalVariables.FindFsmString (_name);
			if (fsmString != null)
			{
				fsmString.Value = _val;
			}
			else
			{
				ACDebug.LogWarning ("Cannot find Playmaker global String with then name '" + _name + "'");
			}

			FsmVariables.GlobalVariables.FindFsmString (_name).Value = _val;
			#endif
		}
		

		/**
		 * <summary>Sets the value of a PlayMaker global float.</summary>
		 * <param name = "_name">The name of the PlayMaker global float to update</param>
		 * <param name = "_val">The new value to assign the PlayMaker global float</param>
		 */
		public static void SetGlobalFloat (string _name, float _val)
		{
			#if PlayMakerIsPresent
			FsmFloat fsmFloat = FsmVariables.GlobalVariables.FindFsmFloat (_name);
			if (fsmFloat != null)
			{
				fsmFloat.Value = _val;
			}
			else
			{
				ACDebug.LogWarning ("Cannot find Playmaker global Float with then name '" + _name + "'");
			}
			#endif
		}


		/**
		 * <summary>Sets the value of a PlayMaker global Vector3.</summary>
		 * <param name = "_name">The name of the PlayMaker global Vector3 to update</param>
		 * <param name = "_val">The new value to assign the PlayMaker global Vector3</param>
		 */
		public static void SetGlobalVector3 (string _name, Vector3 _val)
		{
			#if PlayMakerIsPresent
			FsmVector3 fsmVector3 = FsmVariables.GlobalVariables.FindFsmVector3 (_name);
			if (fsmVector3 != null)
			{
				fsmVector3.Value = _val;
			}
			else
			{
				ACDebug.LogWarning ("Cannot find Playmaker global Vector3 with then name '" + _name + "'");
			}
			#endif
		}
		
	}
	
}