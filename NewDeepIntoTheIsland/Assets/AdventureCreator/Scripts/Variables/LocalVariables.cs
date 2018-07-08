/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2018
 *	
 *	"LocalVariables.cs"
 * 
 *	This script stores Local variables per-scene.
 * 
 */

using UnityEngine;
using System.Collections.Generic;

namespace AC
{

	/**
	 * Stores a scene's local variables.
	 * This component should be attached to the GameEngine prefab.
	 */
	[System.Serializable]
	#if !(UNITY_4_6 || UNITY_4_7 || UNITY_5_0)
	[HelpURL("http://www.adventurecreator.org/scripting-guide/class_a_c_1_1_local_variables.html")]
	#endif
	public class LocalVariables : MonoBehaviour
	{

		/** The List of local variables in the scene. */
		[HideInInspector] public List<GVar> localVars = new List<GVar>();
		/** A List of preset values that the variables can be bulk-assigned to */
		[HideInInspector] public List<VarPreset> varPresets = new List<VarPreset>();


		private void Awake ()
		{
			foreach (GVar _var in localVars)
			{
				_var.CreateRuntimeTranslations ();
			}
		}


		/**
		 * Backs up the values of all local variables.
		 * Necessary when skipping ActionLists that involve checking variable values.
		 */
		public void BackupAllValues ()
		{
			foreach (GVar _var in localVars)
			{
				_var.BackupValue ();
			}
		}


		/**
		 * <summary>Returns a local variable.</summary>
		 * <param name = "_id">The ID number of the variable</param>
		 * <param name = "localVariables">The LocalVariables script to read from, if not the active scene's GameEngine</param>
		 */
		public static GVar GetVariable (int _id, LocalVariables localVariables = null)
		{
			if (localVariables == null)
			{
				localVariables = KickStarter.localVariables;
			}

			if (localVariables)
			{
				foreach (GVar _var in localVariables.localVars)
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
		 * <summary>Returns a list of all local variables.</summary>
		 * <returns>A List of GVar variables</returns>
		 */
		public static List<GVar> GetAllVars ()
		{
			if (KickStarter.localVariables)
			{
				return KickStarter.localVariables.localVars;
			}
			return null;
		}


		/**
		 * <summary>Returns the value of a local Integer variable.</summary>
		 * <param name = "_id">The ID number of the variable</param>
		 * <returns>The integer value of the variable</returns>
		 */
		public static int GetIntegerValue (int _id)
		{
			return LocalVariables.GetVariable (_id).val;
		}
		
		
		/**
		 * <summary>Returns the value of a local Boolean variable.</summary>
		 * <param name = "_id">The ID number of the variable</param>
		 * <returns>The boolean value of the variable</returns>
		 */
		public static bool GetBooleanValue (int _id)
		{
			if (LocalVariables.GetVariable (_id).val == 1)
			{
				return true;
			}
			return false;
		}
		

		/**
		 * <summary>Returns the value of a local String variable.</summary>
		 * <param name = "_id">The ID number of the variable</param>
		 * <param name = "languageNumber">The index number of the game's current language</param>
		 * <returns>The string value of the variable</returns>
		 */
		public static string GetStringValue (int _id, int lanugageNumber = 0)
		{
			return LocalVariables.GetVariable (_id).GetValue (lanugageNumber);
		}
		
		
		/**
		 * <summary>Returns the value of a local Float variable.</summary>
		 * <param name = "_id">The ID number of the variable</param>
		 * <returns>The float value of the variable</returns>
		 */
		public static float GetFloatValue (int _id)
		{
			return LocalVariables.GetVariable (_id).floatVal;
		}


		/**
		 * <summary>Returns the value of a local Vector3 variable.</summary>
		 * <param name = "_id">The ID number of the variable</param>
		 * <returns>The Vector3 value of the variable</returns>
		 */
		public static Vector3 GetVector3Value (int _id)
		{
			return LocalVariables.GetVariable (_id).vector3Val;
		}

		
		/**
		 * <summary>Returns the value of a local Popup variable.</summary>
		 * <param name = "_id">The ID number of the variable</param>
		 * <param name = "languageNumber">The index number of the game's current language</param>
		 * <returns>The string value of the variable</returns>
		 */
		public static string GetPopupValue (int _id, int languageNumber = 0)
		{
			return LocalVariables.GetVariable (_id).GetValue (languageNumber);
		}
		
		
		/**
		 * <summary>Sets the value of a local Integer variable.</summary>
		 * <param name = "_id">The ID number of the variable</param>
		 * <param name = "_value">The new integer value of the variable</param>
		 */
		public static void SetIntegerValue (int _id, int _value)
		{
			LocalVariables.GetVariable (_id).val = _value;
		}
		
		
		/**
		 * <summary>Sets the value of a local Boolean variable.</summary>
		 * <param name = "_id">The ID number of the variable</param>
		 * <param name = "_value">The new bool value of the variable</param>
		 */
		public static void SetBooleanValue (int _id, bool _value)
		{
			if (_value)
			{
				LocalVariables.GetVariable (_id).val = 1;
			}
			else
			{
				LocalVariables.GetVariable (_id).val = 0;
			}
		}
		
		
		/**
		 * <summary>Sets the value of a local String variable.</summary>
		 * <param>_id">The ID number of the variable</param>
		 * <param>_value">The new string value of the variable</param>
		 */
		public static void SetStringValue (int _id, string _value)
		{
			LocalVariables.GetVariable (_id).textVal = _value;
		}
		

		/**
		 * <summary>Sets the value of a local Float variable.</summary>
		 * <param name = "_id">The ID number of the variable</param>
		 * <param name = "_value">The new float value of the variable</param>
		 */
		public static void SetFloatValue (int _id, float _value)
		{
			LocalVariables.GetVariable (_id).floatVal = _value;
		}


		/**
		 * <summary>Sets the value of a local Vector3 variable.</summary>
		 * <param name = "_id">The ID number of the variable</param>
		 * <param name = "_value">The new Vector3 value of the variable</param>
		 */
		public static void SetVector3Value (int _id, Vector3 _value)
		{
			LocalVariables.GetVariable (_id).vector3Val = _value;
		}


		/**
		 * <summary>Sets the value of a local PopUp variable.</summary>
		 * <param name = "_id">The ID number of the variable</param>
		 * <param name = "_value">The new index value of the variable</param>
		 */
		public static void SetPopupValue (int _id, int _value)
		{
			LocalVariables.GetVariable (_id).val = _value;
		}


		/**
		 * <summary>Assigns all Local Variables to preset values.</summary>
		 * <param name = "varPreset">The VarPreset that contains the preset values</param>
		 */
		public void AssignFromPreset (VarPreset varPreset)
		{
			foreach (GVar localVar in localVars)
			{
				foreach (PresetValue presetValue in varPreset.presetValues)
				{
					if (localVar.id == presetValue.id)
					{
						localVar.val = presetValue.val;
						localVar.floatVal = presetValue.floatVal;
						localVar.textVal = presetValue.textVal;
						localVar.vector3Val = presetValue.vector3Val;
					}
				}
			}
		}


		/**
		 * <summary>Assigns all Local Variables to preset values.</summary>
		 * <param name = "varPresetID">The ID number of the VarPreset that contains the preset values</param>
		 */
		public void AssignFromPreset (int varPresetID)
		{
			if (varPresets == null)
			{
				return;
			}

			foreach (VarPreset varPreset in varPresets)
			{
				if (varPreset.ID == varPresetID)
				{
					AssignFromPreset (varPreset);
					return;
				}
			}
		}


		/**
		 * <summary>Gets a Local Variable preset with a specific ID number.</summary>
		 * <param name = "varPresetID">The ID number of the VarPreset</param>
		 * <returns>The Local Variable preset</returns>
		 */
		public VarPreset GetPreset (int varPresetID)
		{
			if (varPresets == null)
			{
				return null;
			}

			foreach (VarPreset varPreset in varPresets)
			{
				if (varPreset.ID == varPresetID)
				{
					return varPreset;
				}
			}

			return null;
		}

	}

}