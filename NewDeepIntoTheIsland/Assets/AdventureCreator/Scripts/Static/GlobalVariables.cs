/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"GlobalVariables.cs"
 * 
 *	This script contains static functions to access Global Variables at runtime.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

namespace AC
{

	/**
	 * A class that can manipulate and retrieve the game's Global Variables at runtime.
	 */
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_global_variables.html")]
	#endif
	public class GlobalVariables : MonoBehaviour
	{

		/**
		 * <summary>Returns a list of all global variables.</summary>
		 * <returns>A List of GVar variables</returns>
		 */
		public static List<GVar> GetAllVars ()
		{
			if (KickStarter.runtimeVariables)
			{
				return KickStarter.runtimeVariables.globalVars;
			}
			return null;
		}


		/**
		 * Backs up the values of all global variables.
		 * Necessary when skipping ActionLists that involve checking variable values.
		 */
		public static void BackupAll ()
		{
			if (KickStarter.runtimeVariables)
			{
				foreach (GVar _var in KickStarter.runtimeVariables.globalVars)
				{
					_var.BackupValue ();
				}
			}
		}
		
		
		/**
		 * Uploads the values all linked variables to their linked counterparts.
		 */
		public static void UploadAll ()
		{
			if (KickStarter.runtimeVariables)
			{
				foreach (GVar var in KickStarter.runtimeVariables.globalVars)
				{
					var.Upload ();
				}
			}
		}
		
		
		/**
		 * Downloads the values of all linked variables from their linked counterparts.
		 */
		public static void DownloadAll ()
		{
			if (KickStarter.runtimeVariables)
			{
				foreach (GVar var in KickStarter.runtimeVariables.globalVars)
				{
					var.Download ();
				}
			}
		}
		

		/**
		 * <summary>Returns a global variable.</summary>
		 * <param name = "_id">The ID number of the variable</param>
		 */
		public static GVar GetVariable (int _id)
		{
			if (KickStarter.runtimeVariables)
			{
				foreach (GVar _var in KickStarter.runtimeVariables.globalVars)
				{
					if (_var.id == _id)
					{
						return _var;
					}
				}
			}
			return null;
		}
		
		
		/**
		 * <summary>Returns the value of a global Integer variable.</summary>
		 * <param name = "_id">The ID number of the variable</param>
		 * <param name = "synchronise">If True, then the variable's value will be synchronised with any external link it may have.</param>
		 * <returns>The integer value of the variable</returns>
		 */
		public static int GetIntegerValue (int _id, bool synchronise = true)
		{
			GVar var = GetVariable (_id);
			if (var != null)
			{
				if (synchronise)
				{
					var.Download ();
				}
				return var.val;
			}

			ACDebug.LogWarning ("Variable with ID=" + _id + " not found!");
			return 0;
		}
		
		
		/**
		 * <summary>Returns the value of a global Boolean variable.<summary>
		 * <param name = "_id">The ID number of the variable</param>
		 * <param name = "synchronise">If True, then the variable's value will be synchronised with any external link it may have.</param>
		 * <returns>The bool value of the variable</returns>
		 */
		public static bool GetBooleanValue (int _id, bool synchronise = true)
		{
			GVar var = GetVariable (_id);
			if (var != null)
			{
				if (synchronise)
				{
					var.Download ();
				}

				if (var.val == 1)
				{
					return true;
				}
				return false;
			}

			ACDebug.LogWarning ("Variable with ID=" + _id + " not found!");
			return false;
		}
		
		
		/**
		 * <summary>Returns the value of a global String variable.</summary>
		 * <param name = "_id">The ID number of the variable</param>
		 * <param name = "synchronise">If True, then the variable's value will be synchronised with any external link it may have.</param>
		 * <param name = "languageNumber">The index number of the game's current language</param>
		 * <returns>The string value of the variable</returns>
		 */
		public static string GetStringValue (int _id, bool synchronise = true, int languageNumber = 0)
		{
			GVar var = GetVariable (_id);
			if (var != null)
			{
				if (synchronise)
				{
					var.Download ();
				}
				return var.GetValue (languageNumber);
			}
			
			ACDebug.LogWarning ("Variable with ID=" + _id + " not found!");
			return string.Empty;
		}
		

		/**
		 * <summary>Returns the value of a global Float variable.</summary>
		 * <param name = "_id">The ID number of the variable</param>
		 * <param name = "synchronise">If True, then the variable's value will be synchronised with any external link it may have.</param>
		 * <returns>The float value of the variable</returns>
		 */
		public static float GetFloatValue (int _id, bool synchronise = true)
		{
			GVar var = GetVariable (_id);
			if (var != null)
			{
				if (synchronise)
				{
					var.Download ();
				}
				return var.floatVal;
			}
			
			ACDebug.LogWarning ("Variable with ID=" + _id + " not found!");
			return 0f;
		}


		/**
		 * <summary>Returns the value of a global Vector3 variable.</summary>
		 * <param name = "_id">The ID number of the variable</param>
		 * <param name = "synchronise">If True, then the variable's value will be synchronised with any external link it may have.</param>
		 * <returns>The Vector3 value of the variable</returns>
		 */
		public static Vector3 GetVector3Value (int _id, bool synchronise = true)
		{
			GVar var = GetVariable (_id);
			if (var != null)
			{
				if (synchronise)
				{
					var.Download ();
				}
				return var.vector3Val;
			}
			
			ACDebug.LogWarning ("Variable with ID=" + _id + " not found!");
			return Vector3.zero;
		}
		

		/**
		 * <summary>Returns the value of a global Popup variable.</summary>
		 * <param name = "_id">The ID number of the variable</param>
		 * <param name = "synchronise">If True, then the variable's value will be synchronised with any external link it may have.</param>
		 * <param name = "languageNumber">The index number of the game's current language</param>
		 * <returns>The string value of the variable</returns>
		 */
		public static string GetPopupValue (int _id, bool synchronise = true, int languageNumber = 0)
		{
			GVar var = GetVariable (_id);
			if (var != null)
			{
				if (synchronise)
				{
					var.Download ();
				}
				return var.GetValue (languageNumber);
			}
			
			ACDebug.LogWarning ("Variable with ID=" + _id + " not found!");
			return string.Empty;
		}


		/**
		 * <summary>Sets the value of a global Integer variable.</summary>
		 * <param name = "_id">The ID number of the variable</param>
		 * <param name = "_value">The new integer value of the variable</param>
		 * <param name = "synchronise">If True, then the variable's value will be synchronised with any external link it may have.</param>
		 */
		public static void SetIntegerValue (int _id, int _value, bool synchronise = true)
		{
			GVar var = GetVariable (_id);
			if (var != null)
			{
				var.val = _value;

				if (synchronise)
				{
					var.Upload ();
				}
			}
		}

		
		/**
		 * <summary>Sets the value of a global Boolean variable.</summary>
		 * <param name = "_id">The ID number of the variable</param>
		 * <param name = "_value">The new bool value of the variable</param>
		 * <param name = "synchronise">If True, then the variable's value will be synchronised with any external link it may have.</param>
		 */
		public static void SetBooleanValue (int _id, bool _value, bool synchronise = true)
		{
			GVar var = GetVariable (_id);
			if (var != null)
			{
				if (_value)
				{
					var.val = 1;
				}
				else
				{
					var.val = 0;
				}
				
				if (synchronise)
				{
					var.Upload ();
				}
			}
		}
		
		
		/**
		 * <summary>Sets the value of a global String variable.</summary>
		 * <param name = "_id">The ID number of the variable</param>
		 * <param name = "_value">The new string value of the variable</param>
		 * <param name = "synchronise">If True, then the variable's value will be synchronised with any external link it may have.</param>
		 */
		public static void SetStringValue (int _id, string _value, bool synchronise = true)
		{
			GVar var = GetVariable (_id);
			if (var != null)
			{
				var.textVal = _value;
				
				if (synchronise)
				{
					var.Upload ();
				}
			}
		}
		
		
		/**
		 * <summary>Sets the value of a global Float variable.</summary>
		 * <param name = "_id">The ID number of the variable</param>
		 * <param name = "_value">The new float value of the variable</param>
		 * <param name = "synchronise">If True, then the variable's value will be synchronised with any external link it may have.</param>
		 */
		public static void SetFloatValue (int _id, float _value, bool synchronise = true)
		{
			GVar var = GetVariable (_id);
			if (var != null)
			{
				var.floatVal = _value;
				
				if (synchronise)
				{
					var.Upload ();
				}
			}
		}


		/**
		 * <summary>Sets the value of a global Vector3 variable.</summary>
		 * <param name = "_id">The ID number of the variable</param>
		 * <param name = "_value">The new float value of the variable</param>
		 * <param name = "synchronise">If True, then the variable's value will be synchronised with any external link it may have.</param>
		 */
		public static void SetVector3Value (int _id, Vector3 _value, bool synchronise = true)
		{
			GVar var = GetVariable (_id);
			if (var != null)
			{
				var.vector3Val = _value;
				
				if (synchronise)
				{
					var.Upload ();
				}
			}
		}


		/**
		 * <summary>Sets the value of a global PopUp variable.</summary>
		 * <param name = "_id">The ID number of the variable</param>
		 * <param name = "_value">The new index value of the variable</param>
		 * <param name = "synchronise">If True, then the variable's value will be synchronised with any external link it may have.</param>
		 */
		public static void SetPopupValue (int _id, int _value, bool synchronise = true)
		{
			SetIntegerValue (_id, _value, synchronise);
		}

	}

}